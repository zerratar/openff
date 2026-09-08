// The editor's back end: a small local HTTP server over the codecs.
//
//   crystal editor [--content=<dir>] [--override=<dir>] [--language=en] [--port=5050]
//
// It binds to localhost only, has no authentication, and is meant to be run by the
// person editing their own copy of the game. It is not a service.
//
// The browser does the editing; this side only decodes, compiles and saves. Which
// means the rules that matter - what compiles, what a menu's coordinates mean - stay
// in one place and are the same ones the command line uses.

using OpenFF.Content;
using OpenFF.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Xml.Linq;

namespace Crystal.Editor
{
	internal sealed class EditorServer
	{
		private static readonly JsonSerializerOptions Json = new JsonSerializerOptions
		{
			WriteIndented = false,
			// The browser side reads camelCase, so the wire format says camelCase.
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		// One session per game the project targets, in the project's order. A request
		// names the one it means with ?ws=<target>; without that it gets the active one.
		// The request loop is single-threaded (GetContext, then Handle, then the next),
		// so a plain field is enough to carry "the session this request is about".
		private readonly Dictionary<string, Session> _sessions =
			new Dictionary<string, Session>(StringComparer.OrdinalIgnoreCase);
		private readonly List<string> _order = new List<string>();
		private readonly Dictionary<string, string> _missing =
			new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		private string _active;
		private Session _current;

		private readonly string _webRoot;
		private string _language;
		private Project _project;

		private Session Current => _current ?? _sessions[_active];

		// The handlers were written against one workspace and its indexes; these keep
		// that vocabulary and route it to whichever session the request is about.
		private Workspace _workspace => Current.Workspace;
		private MessageIndex _messages => Current.Messages;
		private CharacterIds _characterIds => Current.CharacterIds;
		private FlagIndex _flags => Current.Flags;
		private References _references => Current.References;
		private Fonts _fonts => Current.Fonts;
		private Func<uint, string> _lookupMessage => Current.LookupMessage;

		/// <summary>One workspace, opened by path: --content without a project.</summary>
		public EditorServer(Workspace workspace, string webRoot, MessageIndex messages,
			string language = "en", Project project = null)
		{
			_webRoot = webRoot;
			_language = language ?? "en";
			_project = project;
			string name = project?.File.Active ?? NameFor(workspace);
			Adopt(name, new Session(name, workspace, messages, _language));
			if (project == null)
			{
				OpenOtherInstalls();
			}
		}

		/// <summary>
		/// The target whose install this workspace is, so a workspace opened by path gets
		/// the same tab as it would under a project; "content" when it is none of them.
		/// </summary>
		private static string NameFor(Workspace workspace)
		{
			foreach (string target in Targets.All)
			{
				string found = Targets.Find(target);
				if (found != null && SameDirectory(found, workspace.ContentDirectory))
				{
					return target;
				}
			}
			return "content";
		}

		private static bool SameDirectory(string a, string b)
		{
			return string.Equals(Path.GetFullPath(a).TrimEnd(Path.DirectorySeparatorChar),
				Path.GetFullPath(b).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// With no project open, every game on the machine is open: the editor behaves as a
		/// project targeting all of them would (Karl, 2026-09-04). Each install found gets
		/// its own session beside the one the command line asked for, edits going to that
		/// game's default mod directory; one that cannot be read is listed as missing.
		/// </summary>
		private void OpenOtherInstalls()
		{
			foreach (string target in Targets.All)
			{
				if (_sessions.ContainsKey(target))
				{
					continue;
				}
				string found = Targets.Find(target);
				if (found == null || _sessions.Values.Any(s => SameDirectory(s.Workspace.ContentDirectory, found)))
				{
					// Not there, or that install is open already under another target's
					// name (FF4 in OpenFF and FF4 on Steam read the same folder): with no
					// project, one tab per game is the point.
					continue;
				}
				try
				{
					Workspace workspace = new Workspace(found, null);
					_sessions[target] = new Session(target, workspace, null, _language);
					_order.Add(target);
				}
				catch (Exception ex) when (ex is FileNotFoundException or IOException or InvalidDataException)
				{
					_missing[target] = ex.Message;
				}
			}
		}

		/// <summary>Every game the project targets, each in its own session.</summary>
		public EditorServer(string webRoot, string language, Project project)
		{
			_webRoot = webRoot;
			_language = language ?? "en";
			OpenProject(project);
		}

		private void Adopt(string name, Session session)
		{
			_sessions.Clear();
			_order.Clear();
			_missing.Clear();
			_sessions[name] = session;
			_order.Add(name);
			_active = name;
			_current = null;
		}

		/// <summary>
		/// Opens a project: a session per target whose content can be found, the
		/// project's active target first. Nothing is written by this - it only changes
		/// what is being looked at. Throws only when no target at all can be opened.
		/// </summary>
		public void OpenProject(Project project)
		{
			Dictionary<string, Session> sessions = new Dictionary<string, Session>(StringComparer.OrdinalIgnoreCase);
			Dictionary<string, string> missing = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			List<string> order = new List<string>();
			foreach (string target in project.File.Targets)
			{
				try
				{
					// Keep a session already open on the same content and override:
					// its indexes took seconds to build and nothing about them changed.
					if (_sessions.TryGetValue(target, out Session kept)
						&& string.Equals(kept.Workspace.ContentDirectory, project.ContentDirectoryFor(target), StringComparison.OrdinalIgnoreCase)
						&& string.Equals(kept.Workspace.OverrideDirectory, project.FilesFor(target), StringComparison.OrdinalIgnoreCase))
					{
						sessions[target] = kept;
					}
					else
					{
						Workspace workspace = new Workspace(project.ContentDirectoryFor(target), project.FilesFor(target));
						sessions[target] = new Session(target, workspace, null, _language);
					}
					order.Add(target);
				}
				catch (Exception ex) when (ex is FileNotFoundException or IOException or InvalidDataException)
				{
					missing[target] = ex.Message;
				}
			}
			if (order.Count == 0)
			{
				throw new FileNotFoundException(missing.Count > 0
					? string.Join(" ", missing.Values)
					: "the project has no targets");
			}

			_project = project;
			_sessions.Clear();
			_order.Clear();
			_missing.Clear();
			foreach (KeyValuePair<string, Session> entry in sessions) _sessions[entry.Key] = entry.Value;
			foreach (KeyValuePair<string, string> entry in missing) _missing[entry.Key] = entry.Value;
			_order.AddRange(order);
			_active = order.Contains(project.File.Active, StringComparer.OrdinalIgnoreCase)
				? project.File.Active : order[0];
			_current = null;
		}

		/// <summary>The sessions open right now, active first.</summary>
		public IEnumerable<Session> Sessions => _order.Select(name => _sessions[name]);

		/// <summary>The project being edited, if the editor was started with one.</summary>
		public Project CurrentProject => _project;

		/// <summary>
		/// Serves until stopped. <paramref name="started"/> is called once the listener
		/// is actually accepting, and is given the address - opening a browser before
		/// that point is a race the browser can win, and it lands on a dead port.
		/// </summary>
		public void Run(int port, Action<string> started = null)
		{
			string address = string.Format(CultureInfo.InvariantCulture,
				"http://localhost:{0}/", port);

			using HttpListener listener = new HttpListener();
			listener.Prefixes.Add(address);
			listener.Start();

			Console.WriteLine("Crystal - the OpenFF editor");
			foreach (Session session in Sessions)
			{
				Console.WriteLine("  {0,-12} {1} files, {2}, in {3}", session.Label,
					session.Workspace.FileCount, session.Workspace.Kind, session.Workspace.ContentDirectory);
				Console.WriteLine("  {0,-12} edits in {1}", string.Empty, session.Workspace.OverrideDirectory);
			}
			foreach (KeyValuePair<string, string> entry in _missing)
			{
				Console.WriteLine("  {0,-12} not opened: {1}", Targets.Describe(entry.Key), entry.Value);
			}
			Console.WriteLine();
			Console.WriteLine("  {0}", address);
			Console.WriteLine();
			started?.Invoke(address);
			Console.WriteLine("Ctrl+C to stop.");

			using ManualResetEventSlim stopping = new ManualResetEventSlim(false);
			Console.CancelKeyPress += (sender, e) =>
			{
				e.Cancel = true;
				stopping.Set();
				listener.Stop();
			};

			while (!stopping.IsSet)
			{
				HttpListenerContext context;
				try
				{
					context = listener.GetContext();
				}
				catch (HttpListenerException)
				{
					break;                                   // stopped
				}
				catch (InvalidOperationException)
				{
					break;
				}

				// Read before the handler runs. A request that fails is often one the
				// browser has already abandoned, and by then the request object can be
				// disposed - so asking it what it was, in the middle of reporting that
				// it failed, throws a second time.
				string what = Describe(context);

				try
				{
					Handle(context);
				}
				catch (Exception ex)
				{
					// Nothing in here may throw. Reloading the page cancels every request
					// that was in flight, and a handler part way through one then fails on
					// a socket nobody is holding; sending 500 down that same socket throws
					// again, and an exception raised inside a catch escapes the loop and
					// takes the whole editor with it. That was "Bytes to be written to the
					// stream exceed the Content-Length", and then "Cannot access a disposed
					// object" one line further down - both of them the second failure
					// rather than the first.
					try
					{
						Send(context, 500, "application/json",
							Encoding.UTF8.GetBytes(JsonSerializer.Serialize(
								new { error = ex.Message }, Json)));
					}
					catch (Exception)
					{
						// Nothing left to answer. The next request is what matters.
					}

					try
					{
						Console.Error.WriteLine("{0}: {1}", what, ex.Message);
					}
					catch (Exception)
					{
					}
				}
			}
		}

		/// <summary>
		/// What a request was, taken while it is still safe to ask. An abandoned request
		/// throws on every property, and the error path needs this after that has
		/// happened.
		/// </summary>
		private static string Describe(HttpListenerContext context)
		{
			try
			{
				return context.Request.HttpMethod + " " + context.Request.Url.AbsolutePath;
			}
			catch (Exception)
			{
				return "a request";
			}
		}

		private void Handle(HttpListenerContext context)
		{
			string path = context.Request.Url.AbsolutePath;

			if (!path.StartsWith("/api/", StringComparison.Ordinal))
			{
				ServeStatic(context, path);
				return;
			}

			// Which game this request is about. Unknown names fall back to the active
			// session rather than failing, so a stale tab still gets an answer.
			string wanted = context.Request.QueryString["ws"];
			_current = wanted != null && _sessions.TryGetValue(wanted, out Session named)
				? named : _sessions[_active];

			switch (path)
			{
				case "/api/status":
					SendJson(context, new
					{
						files = _workspace.FileCount,
						overrides = _workspace.OverrideDirectory,
						// Every game that is open, so the page can offer them side by side.
						workspace = Current.Target,
						active = _active,
						workspaces = _order.Select(name => new
						{
							target = name,
							label = _sessions[name].Label,
							game = _sessions[name].Game,
							// "openff" or "steam": what becomes of the edits. An install
							// opened without a project is browsed, and reads as Steam's.
							mod = Targets.KindOf(name),
							files = _sessions[name].Workspace.FileCount,
							kind = _sessions[name].Workspace.Kind,
							contentDirectory = _sessions[name].Workspace.ContentDirectory,
							overrides = _sessions[name].Workspace.OverrideDirectory,
							installable = _sessions[name].Installable
						}).ToList(),
						missing = _missing,
						// Every game this machine has, open or not, so the page can show a
						// tab for each and grey out the ones the project does not target.
						available = Targets.All.Select(name => new
						{
							target = name,
							label = Targets.Describe(name),
							game = Targets.GameOf(name),
							mod = Targets.KindOf(name),
							found = Targets.Find(name) != null,
							open = _order.Contains(name, StringComparer.OrdinalIgnoreCase)
						}).ToList(),
						// Which language the text was read as, and the directory it was
						// read from, so the page can open the file a line actually lives
						// in rather than guessing. The two are not the same thing: a
						// Steam install is one language with its .msd files straight in
						// files/, with no .lproj anywhere.
						language = _messages.Language,
						messagePrefix = _messages.Prefix,
						content = _workspace.Kind,
						game = _workspace.Game,
						contentDirectory = _workspace.ContentDirectory,
						project = _project == null ? null : new
						{
							name = _project.File.Name,
							directory = _project.Directory,
							targets = _project.File.Targets,
							active = _project.File.Active,
							author = _project.File.Author,
							version = _project.File.Version,
							description = _project.File.Description,
							code = ModCode.Has(_project),
							// The mod's entry point, for the header's button: the first GameService in the source.
							service = ModCode.Has(_project) ? ModCode.Sources(_project).FirstOrDefault(s => s.Kind == "service")?.File : null,
							client = OpenFFClient.Executable() != null,
							scenes = ProjectScenes.Maps(_project).Count,
							items = ProjectItems.All(_project).Count
						}
					});
					return;

				case "/api/projects":
					SendJson(context, Project.All().Select(p => new
					{
						name = p.File.Name,
						directory = p.Directory,
						targets = p.File.Targets,
						active = p.File.Active,
						current = _project != null && string.Equals(
							p.Directory, _project.Directory, StringComparison.OrdinalIgnoreCase)
					}).ToList());
					return;

				case "/api/project/create":
					CreateProject(context);
					return;

				case "/api/project/open":
					OpenProjectRequest(context);
					return;

				case "/api/project/target":
					SetTarget(context);
					return;

				case "/api/project/save":
					SaveProjectDetails(context);
					return;

				case "/api/project/export":
					ExportProject(context);
					return;

				case "/api/project/export-openff":
					ExportProjectToOpenFF(context);
					return;

				case "/api/project/code/create":
				case "/api/project/code/build":
				case "/api/project/code/open":
					ProjectCode(context, path.Substring("/api/project/code/".Length));
					return;

				case "/api/project/files":
				case "/api/project/file":
				case "/api/project/file/save":
				case "/api/project/file/new":
				case "/api/project/file/open":
					ProjectFiles(context, path.Substring("/api/project/".Length));
					return;

				case "/api/project/reveal":
					RevealProject(context);
					return;

				case "/api/project/code/catalog":
					ProjectCatalog(context);
					return;

				case "/api/project/scene":
					ProjectScene(context);
					return;

				case "/api/project/scene/save":
					SaveProjectScene(context);
					return;

				case "/api/project/tags":
					// Every tag the project's scene files use, with its use: the inspector's tag picker.
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					SendJson(context, new { ok = true, tags = ProjectScenes.Tags(_project), global = _project.File.Tags ?? new List<string>() });
					return;

				case "/api/project/items":
				{
					// The mod's item definitions (defs/items), each beside the record it starts from.
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					List<string> notes = new List<string>();
					List<ModItem> all = ProjectItems.All(_project, notes);
					string one = Query(context, "id");
					if (!string.IsNullOrEmpty(one))
					{
						ModItem item = all.FirstOrDefault(i => string.Equals(i.Id, one, StringComparison.OrdinalIgnoreCase));
						if (item == null) { SendJson(context, new { ok = false, error = "no item definition '" + one + "'" }); return; }
						SendJson(context, new { ok = true, item = ProjectItems.Describe(_workspace, item, _lookupMessage) });
						return;
					}
					SendJson(context, new { ok = true, items = all.Select(i => ProjectItems.Describe(_workspace, i, _lookupMessage)).ToList(), notes });
					return;
				}

				case "/api/project/items/save":
				{
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					JsonNode body = ReadBody(context);
					try
					{
						ModItem item = ModItem.Parse(body?.ToJsonString() ?? "{}");
						if (item == null) throw new ArgumentException("no item");
						if (item.Id != null && item.Id.IndexOfAny(new[] { '/', '\\', '.' }) >= 0) throw new ArgumentException("an item's id is a plain word");
						ProjectItems.Save(_project, item);
						SendJson(context, new { ok = true, item = ProjectItems.Describe(_workspace, item, _lookupMessage) });
					}
					catch (Exception ex) { SendJson(context, new { ok = false, error = ex.Message }); }
					return;
				}

				case "/api/project/items/new":
				{
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					JsonNode body = ReadBody(context);
					try
					{
						string name = body?["name"]?.GetValue<string>();
						int baseId = body?["base"]?.GetValue<int>() ?? 0;
						if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("an item needs a name");
						if (ProjectItems.BaseRecord(_workspace, baseId, out _) == null) throw new ArgumentException("no item " + baseId + " in the game's tables to start from");
						ModItem item = ProjectItems.New(_project, name, baseId);
						SendJson(context, new { ok = true, item = ProjectItems.Describe(_workspace, item, _lookupMessage) });
					}
					catch (Exception ex) { SendJson(context, new { ok = false, error = ex.Message }); }
					return;
				}

				case "/api/project/items/delete":
				{
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					JsonNode body = ReadBody(context);
					string id = body?["id"]?.GetValue<string>();
					SendJson(context, new { ok = ProjectItems.Delete(_project, id) });
					return;
				}

				case "/api/project/tags/global":
				{
					// The mod's own tag list (project.json): what every scene's picker offers.
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					JsonNode body = ReadBody(context);
					List<string> tags = new List<string>();
					foreach (JsonNode t in body?["tags"] as JsonArray ?? new JsonArray())
					{
						string tag = t?.GetValue<string>()?.Trim();
						if (!string.IsNullOrEmpty(tag) && !tags.Any(x => string.Equals(x, tag, StringComparison.OrdinalIgnoreCase))) tags.Add(tag);
					}
					tags.Sort(StringComparer.OrdinalIgnoreCase);
					_project.File.Tags = tags;
					_project.Save();
					SendJson(context, new { ok = true, global = tags });
					return;
				}

				case "/api/project/tags/retag":
				{
					// A tag renamed (or removed: no "to") on every object of every scene file, and in
					// the mod's list; the maps whose files changed come back.
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					JsonNode body = ReadBody(context);
					string from = body?["from"]?.GetValue<string>();
					string to = body?["to"]?.GetValue<string>() ?? "";
					if (string.IsNullOrWhiteSpace(from)) { SendJson(context, new { ok = false, error = "which tag?" }); return; }
					List<string> touched = ProjectScenes.Retag(_project, from, to);
					SendJson(context, new { ok = true, maps = touched, global = _project.File.Tags });
					return;
				}

				case "/api/project/session":
					ProjectSession(context);
					return;

				case "/api/map/convert-plan":
				{
					// What each of the map's characters would be as the mod's own (MapConvert).
					string name = Query(context, "name");
					try
					{
						SendJson(context, new { ok = true, map = name, casts = MapConvert.Plan(_workspace, name, _lookupMessage) });
					}
					catch (Exception ex) when (ex is IOException or InvalidDataException or ArgumentException or KeyNotFoundException)
					{
						SendJson(context, new { ok = false, error = ex.Message });
					}
					return;
				}

				case "/api/map/flags":
				{
					// The flags the map's script tests and sets, and by which casts: what a flag
					// field in the inspector offers to pick from.
					string name = Query(context, "name");
					try
					{
						SendJson(context, new { ok = true, map = name, flags = MapConvert.Flags(_workspace, name) });
					}
					catch (Exception ex) when (ex is IOException or InvalidDataException or ArgumentException or KeyNotFoundException)
					{
						SendJson(context, new { ok = false, error = ex.Message });
					}
					return;
				}

				case "/api/project/run":
					RunProject(context);
					return;

				case "/api/openff/reference":
					SendJson(context, new { types = ApiReference.Read(out string source) ?? new List<ApiType>(), source });
					return;

				case "/api/targets":
					SendJson(context, Targets.All.Select(name => new
					{
						name,
						label = Targets.Describe(name),
						game = Targets.GameOf(name),
						mod = Targets.KindOf(name),
						content = Targets.Find(name)
					}).ToList());
					return;

				case "/api/mod/status":
					SendJson(context, Current.Installable
						? ModInstall.Status(_workspace)
						: ModInstall.NotInstallable(_workspace, "an OpenFF mod is played by the client from the project - nothing to install; Export to OpenFF puts it in the mods folder"));
					return;

				case "/api/mod/install":
					SendJson(context, Current.Installable
						? ModInstall.Install(_workspace)
						: new ModResult { Ok = false, Error = "this is the OpenFF side of the project; the client reads it, nothing is installed" });
					return;

				case "/api/mod/uninstall":
					SendJson(context, Current.Installable
						? ModInstall.Uninstall(_workspace)
						: new ModResult { Ok = false, Error = "this is the OpenFF side of the project; nothing was installed" });
					return;

				case "/api/mod/revert":
					RevertMany(context);
					return;

				case "/api/list":
					SendJson(context, ListKind(Query(context, "kind")));
					return;

				case "/api/script":
					GetScript(context);
					return;

				case "/api/script/save":
					SaveScript(context);
					return;

				case "/api/text":
					GetText(context);
					return;

				case "/api/text/save":
					SaveText(context);
					return;

				case "/api/menu":
					GetMenu(context);
					return;

				case "/api/menu/save":
					SaveMenu(context);
					return;

				case "/api/table":
					GetTable(context);
					return;

				case "/api/table/save":
					SaveTable(context);
					return;

				case "/api/messages":
					GetMessages(context);
					return;

				case "/api/ops":
					GetOps(context);
					return;

				case "/api/maps":
					SendJson(context, _workspace.List(".hich")
						.Select(entry => Path.GetFileNameWithoutExtension(entry.Name))
						.OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
						.ToList());
					return;

				case "/api/map":
					SendJson(context, MapModel.Load(_workspace,
						Query(context, "name"), _lookupMessage));
					return;

				case "/api/models/placeable":
					SendJson(context, _characterIds.All());
					return;

				case "/api/map/scene":
					SendJson(context, MapScene.Load(_workspace,
						Query(context, "name"), _messages.Text, _references));
					return;

				case "/api/map/delete":
					DeleteFromMap(context);
					return;

				case "/api/map/save":
					SaveMap(context);
					return;

				case "/api/map/add":
					AddToMap(context);
					return;

				case "/api/menu/background":
					{
						string screen = Query(context, "screen") ?? string.Empty;
						SendJson(context, new
						{
							screen,
							bank = MenuBackgrounds.ForScreen.TryGetValue(screen,
								out string bank) ? bank : null
						});
					}
					return;

				case "/api/font":
					SendJson(context, new { sizes = _fonts.Sizes() });
					return;

				case "/api/font/layout":
					SendJson(context, _fonts.Layout(
						int.Parse(Query(context, "size") ?? "12", CultureInfo.InvariantCulture),
						Query(context, "text") ?? string.Empty));
					return;

				case "/api/font/page":
					GetFontPage(context);
					return;

				case "/api/items":
					GetItems(context);
					return;

				case "/api/data":
					SendJson(context, GameData.Page(_workspace, Query(context, "name")));
					return;

				case "/api/map/exit/save":
					SaveExit(context);
					return;

				case "/api/map/exit/add":
					AddExit(context);
					return;

				case "/api/map/exit/region":
					MoveExitRegion(context);
					return;

				case "/api/map/exit/delete":
					DeleteExit(context);
					return;

				case "/api/map/exits":
					SendJson(context, MapExits.State(_workspace, Query(context, "name")));
					return;

				case "/api/map/cast/references":
					SendJson(context, new
					{
						to = _references.ToCast(Query(context, "name"),
							int.Parse(Query(context, "cast"), CultureInfo.InvariantCulture)),
						says = _references.MessagesOf(Query(context, "name"),
							int.Parse(Query(context, "cast"), CultureInfo.InvariantCulture))
					});
					return;

				case "/api/map/exit/references":
					SendJson(context, new
					{
						to = _references.ToExit(Query(context, "name"),
							int.Parse(Query(context, "slot"), CultureInfo.InvariantCulture)),
						builtIn = _references.BuildMilliseconds,
						maps = _references.MapsRead,
						unreadable = _references.Unreadable
					});
					return;

				case "/api/images":
					SendJson(context, Images.List(_workspace));
					return;

				case "/api/image":
					GetImage(context);
					return;

				case "/api/image/upload":
					UploadImage(context);
					return;

				case "/api/cells":
					SendJson(context, CellBanks.List(_workspace));
					return;

				case "/api/cell":
					GetCellBank(context);
					return;

				case "/api/models":
					SendJson(context, Models.List(_workspace));
					return;

				case "/api/model":
					GetModel(context);
					return;

				case "/api/model/texture":
					GetModelTexture(context);
					return;

				case "/api/model/motions":
					SendJson(context, Models.Motions(_workspace, Query(context, "name")));
					return;

				case "/api/model/export":
					ExportModel(context);
					return;

				case "/api/model/pose":
					SendJson(context, Models.ReadPose(_workspace, Query(context, "name"),
						Query(context, "pack"),
						int.Parse(Query(context, "index") ?? "0", CultureInfo.InvariantCulture)));
					return;

				case "/api/textures":
					SendJson(context, new
					{
						packages = Textures.List(_workspace),
						formats = Textures.FormatNotes
					});
					return;

				case "/api/texture":
					GetTextureList(context);
					return;

				case "/api/texture/png":
					GetTexturePng(context);
					return;

				case "/api/audio":
					SendJson(context, Audio.List(_workspace.ContentDirectory, _workspace));
					return;

				case "/api/audio/uses":
					SendJson(context, Audio.Uses(_workspace, Query(context, "name")));
					return;

				case "/api/audio/wav":
					GetAudioWav(context);
					return;

				case "/api/revert":
					Revert(context);
					return;

				default:
					Send(context, 404, "application/json",
						Encoding.UTF8.GetBytes("{\"error\":\"no such endpoint\"}"));
					return;
			}
		}

		// ------------------------------------------------------------------ scripts

		private void GetScript(HttpListenerContext context)
		{
			string name = Query(context, "name");
			ScriptFile script = ScriptFile.Read(_workspace.Read(name), _workspace.Ops);

			using StringWriter text = new StringWriter();
			Ffs.SourceWriter.Write(text, script, Path.GetFileName(name), _lookupMessage);

			SendJson(context, new
			{
				name,
				overridden = _workspace.IsOverridden(name),
				source = text.ToString()
			});
		}

		private void SaveScript(HttpListenerContext context)
		{
			JsonNode body = ReadBody(context);
			string name = (string)body["name"];
			string source = (string)body["source"];
			bool save = body["save"] == null || (bool)body["save"];

			try
			{
				byte[] data = Ffs.Compiler.Compile(Ffs.Parser.Parse(source), _workspace.Ops);
				if (save)
				{
					_workspace.Write(name, data);
				}
				SendJson(context, new
				{
					ok = true,
					saved = save,
					bytes = data.Length,
					overridden = _workspace.IsOverridden(name),
					problems = Array.Empty<object>()
				});
			}
			catch (Ffs.ScriptSyntaxException ex)
			{
				SendJson(context, new
				{
					ok = false,
					saved = false,
					// Detail, not Message: the browser shows the place itself.
					problems = new[] { new { line = ex.Line, column = ex.Column, message = ex.Detail } }
				});
			}
			catch (Ffs.ScriptCompileException ex)
			{
				SendJson(context, new
				{
					ok = false,
					saved = false,
					problems = ex.Diagnostics
						.Select(d => new { line = d.Line, column = d.Column, message = d.Message })
						.ToArray()
				});
			}
		}

		// --------------------------------------------------------------------- text

		private void GetText(HttpListenerContext context)
		{
			string name = Query(context, "name");
			MsdFile decoded = Msd.Read(_workspace.Read(name));
			SendJson(context, new
			{
				name,
				overridden = _workspace.IsOverridden(name),
				messages = decoded.Messages.Select(m => new
				{
					id = m.Id,
					pages = m.Pages,
					encoding = m.TextEncoding
				})
			});
		}

		private void SaveText(HttpListenerContext context)
		{
			JsonNode body = ReadBody(context);
			string name = (string)body["name"];

			MsdFile file = new MsdFile();
			foreach (JsonNode message in body["messages"].AsArray())
			{
				MsdMessage entry = new MsdMessage
				{
					Id = (uint)message["id"],
					TextEncoding = (string)message["encoding"]
				};
				foreach (JsonNode page in message["pages"].AsArray())
				{
					entry.Pages.Add((string)page);
				}
				file.Messages.Add(entry);
			}

			byte[] data = Msd.Write(file);
			_workspace.Write(name, data);
			_messages.Invalidate();
			SendJson(context, new { ok = true, bytes = data.Length, overridden = true });
		}

		// -------------------------------------------------------------------- menus

		private void GetMenu(HttpListenerContext context)
		{
			string name = Query(context, "name");
			XDocument document = MenuXbn.ToXml(_workspace.Read(name));
			SendJson(context, new
			{
				name,
				overridden = _workspace.IsOverridden(name),
				xml = document.ToString(SaveOptions.None)
			});
		}

		private void SaveMenu(HttpListenerContext context)
		{
			JsonNode body = ReadBody(context);
			string name = (string)body["name"];
			string xml = (string)body["xml"];

			try
			{
				byte[] data = MenuXbn.FromXml(XDocument.Parse(xml));
				_workspace.Write(name, data);
				SendJson(context, new { ok = true, bytes = data.Length, overridden = true });
			}
			catch (Exception ex)
			{
				SendJson(context, new { ok = false, error = ex.Message });
			}
		}

		private void AddToMap(HttpListenerContext context)
		{
			JsonNode body = ReadBody(context);
			// The new line has to be visible to the very views that just wrote it.
			_messages.Invalidate();

			string behaviourName = (string)body["behaviour"] ?? "talk";
			if (!Enum.TryParse(behaviourName, true, out EntityBehaviour behaviour))
			{
				SendJson(context, new
				{
					ok = false,
					error = "there is no behaviour called " + behaviourName
				});
				return;
			}

			AddEntityResult added = AddEntity.Add(
				_workspace,
				(string)body["name"],
				(string)body["model"],
				(int)body["x"],
				(int)body["z"],
				behaviour,
				(string)body["text"],
				body["item"] == null ? 0 : (int)body["item"],
				body["gold"] == null ? 0 : (int)body["gold"],
				_lookupMessage,
				_characterIds,
				_flags);
			// A model placed for the first time is a model every other map can use now,
			// and a chest has taken a flag nothing else may have.
			_characterIds.Invalidate();
			if (added.Ok) _flags.Invalidate();
			SendJson(context, added);
		}

		/// <summary>Changes where one exit leads.</summary>
		private void SaveExit(HttpListenerContext context)
		{
			JsonNode body = ReadBody(context);
			_references.Invalidate();
			MapExitResult saved = MapExits.Save(_workspace, (string)body["name"],
				new MapExitEdit
				{
					Index = (int)body["index"],
					X = (int)body["x"],
					Y = (int)body["y"],
					Z = (int)body["z"],
					RotationY = (int)body["rotationY"],
					To = (string)body["to"],
					ModelNo = body["modelNo"] == null ? -1 : (int)body["modelNo"],
					ToIndex = (int)body["toIndex"],
					ConditionFlag = (int)body["conditionFlag"],
					Kind = (int)body["kind"]
				});
			SendJson(context, saved);
		}

		/// <summary>One page of the font atlas, as a PNG for the browser to blit from.</summary>
		private void GetFontPage(HttpListenerContext context)
		{
			int size = int.Parse(Query(context, "size") ?? "12", CultureInfo.InvariantCulture);
			int page = int.Parse(Query(context, "page") ?? "0", CultureInfo.InvariantCulture);
			byte[] png = _fonts.PagePng(size, page);
			// A glyph page never changes, so it is worth the browser keeping it.
			context.Response.Headers["Cache-Control"] = "max-age=86400";
			Send(context, 200, "image/png", png);
		}

		/// <summary>Makes a whole exit: the row, and the region that fires it.</summary>
		private void AddExit(HttpListenerContext context)
		{
			JsonNode body = ReadBody(context);
			int[] size = Mcl.DefaultRegion;
			MapExitResult added = MapExits.Add(_workspace, (string)body["name"],
				new MapExitEdit
				{
					X = (int)body["x"],
					Y = (int)body["y"],
					Z = (int)body["z"],
					RotationY = body["rotationY"] == null ? 0 : (int)body["rotationY"],
					To = (string)body["to"],
					ModelNo = body["modelNo"] == null ? -1 : (int)body["modelNo"],
					ToIndex = body["toIndex"] == null ? 0 : (int)body["toIndex"],
					ConditionFlag = body["conditionFlag"] == null
						? 1 : (int)body["conditionFlag"],
					Kind = body["kind"] == null ? -1 : (int)body["kind"]
				},
				body["width"] == null ? size[0] : (int)body["width"],
				body["height"] == null ? size[1] : (int)body["height"],
				body["depth"] == null ? size[2] : (int)body["depth"]);
			if (added.Ok) _references.Invalidate();
			SendJson(context, added);
		}

		/// <summary>Moves or resizes the region that fires an exit.</summary>
		private void MoveExitRegion(HttpListenerContext context)
		{
			JsonNode body = ReadBody(context);
			MapExitResult moved = MapExits.MoveRegion(_workspace,
				(string)body["name"],
				(int)body["slot"],
				(int)body["x"],
				(int)body["y"],
				(int)body["z"],
				body["width"] == null ? 0 : (int)body["width"],
				body["height"] == null ? 0 : (int)body["height"],
				body["depth"] == null ? 0 : (int)body["depth"]);
			SendJson(context, moved);
		}

		/// <summary>Takes both halves of an exit away again.</summary>
		private void DeleteExit(HttpListenerContext context)
		{
			JsonNode body = ReadBody(context);
			MapExitResult removed = MapExits.Remove(_workspace, (string)body["name"],
				(int)body["slot"], _references, _lookupMessage);
			if (removed.Ok) _references.Invalidate();
			SendJson(context, removed);
		}

		/// <summary>
		/// Every item a chest can hold, by id and name, so choosing one is a matter of
		/// reading rather than knowing that 3101 is a Leather Cap.
		/// </summary>
		private void GetItems(HttpListenerContext context)
		{
			const string name = "files/item_parameter.pak";
			PakFile decoded = Pak.Read(_workspace.Read(name), "Item", _lookupMessage);

			List<object> items = new List<object>();
			foreach (PakChainData chain in decoded.Chains)
			{
				if (chain.Records == null) continue;
				foreach (JsonObject record in chain.Records)
				{
					if (!record.TryGetPropertyValue("itemId", out JsonNode id)) continue;
					items.Add(new
					{
						id = Number(id),
						name = record.TryGetPropertyValue("nameText", out JsonNode itemName)
							? (string)itemName
							: string.Empty,
						category = chain.Label,
						price = record.TryGetPropertyValue("price", out JsonNode price)
							? Number(price)
							: 0
					});
				}
			}
			// The project's own items after the game's, with the number the game will know them
			// by, so a Chest's Item field (and any [ItemField]) can hold one.
			if (_project != null)
			{
				foreach (ModItem mine in ProjectItems.All(_project))
				{
					ProjectItems.BaseRecord(_workspace, mine.Base, out int chain);
					items.Add(new
					{
						id = mine.Number,
						name = (mine.Name ?? mine.Id) + " (mod)",
						category = chain >= 0 ? ModItems.ChainNames[chain] : "mod",
						price = mine.Buy ?? 0
					});
				}
			}

			SendJson(context, items);
		}

		/// <summary>
		/// A number out of a decoded record. The fields keep the width the game gave
		/// them - an item id is a short, a price an int - so a plain cast to int throws
		/// on half of them.
		/// </summary>
		private static int Number(JsonNode node)
		{
			return node != null && int.TryParse(node.ToJsonString(), NumberStyles.Integer,
				CultureInfo.InvariantCulture, out int value)
				? value
				: 0;
		}

		/// <summary>Removes a character and everything that only existed for it.</summary>
		private void DeleteFromMap(HttpListenerContext context)
		{
			JsonNode body = ReadBody(context);
			DeleteCharacterResult removed = DeleteCharacter.Delete(
				_workspace, (string)body["name"], (int)body["cast"], _lookupMessage);
			_messages.Invalidate();
			_characterIds.Invalidate();
			SendJson(context, removed);
		}

		private void SaveMap(HttpListenerContext context)
		{
			JsonNode body = ReadBody(context);
			string map = (string)body["name"];

			List<MapEdit> edits = new List<MapEdit>();
			foreach (JsonNode move in body["characters"].AsArray())
			{
				edits.Add(new MapEdit
				{
					Index = (int)move["index"],
					X = (int)move["x"],
					Y = (int)move["y"],
					Z = (int)move["z"],
					RotationY = (int)move["rotationY"],
					Model = (string)move["model"],
					Cast = move["cast"] is null ? null : (int?)(int)move["cast"]
				});
			}

			int changed = MapModel.Save(_workspace, map, edits, _characterIds);
			_characterIds.Invalidate();
			SendJson(context, new { ok = true, changed, overridden = true });
		}

		/// <summary>One cell bank, screen or animation.</summary>
		private void GetCellBank(HttpListenerContext context)
		{
			string name = Query(context, "name");
			try
			{
				SendJson(context, CellBanks.Read(_workspace, name));
			}
			catch (Exception ex)
			{
				SendJson(context, new { error = ex.Message });
			}
		}

		/// <summary>One model, already turned into triangles for the viewer.</summary>
		private void GetModel(HttpListenerContext context)
		{
			string name = Query(context, "name");
			try
			{
				SendJson(context, Models.Read(_workspace, name));
			}
			catch (Exception ex)
			{
				SendJson(context, new { error = ex.Message });
			}
		}

		/// <summary>
		/// The model as .glb, with the motion named in the body if any, written under the
		/// project (or beside the override directory when there is no project) in
		/// exports/, and revealed. A file on disk rather than a download, because the next
		/// step is opening it in Blender, and that wants a path.
		/// </summary>
		private void ExportModel(HttpListenerContext context)
		{
			JsonNode body = ReadBody(context);
			string name = (string)body?["name"];
			string pack = (string)body?["pack"];
			int index = body?["index"] != null ? (int)body["index"] : 0;
			if (string.IsNullOrEmpty(name))
			{
				SendJson(context, new { ok = false, error = "no model named" });
				return;
			}
			try
			{
				string root = _project != null ? _project.Directory
					: Path.GetDirectoryName(_workspace.OverrideDirectory) ?? _workspace.OverrideDirectory;
				string path = Models.Export(_workspace, name, pack, index, Path.Combine(root, "exports"));
				SendJson(context, new { ok = true, path, bytes = new FileInfo(path).Length });
			}
			catch (Exception ex)
			{
				SendJson(context, new { ok = false, error = ex.Message });
			}
		}

		/// <summary>A texture a model asks for, found by name rather than by index.</summary>
		private void GetModelTexture(HttpListenerContext context)
		{
			string name = Query(context, "name");
			string texture = Query(context, "texture");
			try
			{
				Send(context, 200, "image/png", Models.Texture(_workspace, name, texture));
			}
			catch (Exception ex)
			{
				Send(context, 404, "text/plain", Encoding.UTF8.GetBytes(ex.Message));
			}
		}

		/// <summary>
		/// What textures a package holds. Opening it means decompressing it, so this is
		/// asked for one package at a time rather than for all 1589 at once.
		/// </summary>
		private void GetTextureList(HttpListenerContext context)
		{
			string name = Query(context, "name");
			try
			{
				SendJson(context, Textures.Contents(_workspace, name));
			}
			catch (Exception ex)
			{
				SendJson(context, new { error = ex.Message });
			}
		}

		/// <summary>One texture, decoded on the way out.</summary>
		private void GetTexturePng(HttpListenerContext context)
		{
			string name = Query(context, "name");
			if (!int.TryParse(Query(context, "index"), NumberStyles.Integer,
				CultureInfo.InvariantCulture, out int index))
			{
				index = 0;
			}

			try
			{
				Send(context, 200, "image/png", Textures.Png(_workspace, name, index));
			}
			catch (Exception ex)
			{
				Send(context, 404, "text/plain", Encoding.UTF8.GetBytes(ex.Message));
			}
		}

		/// <summary>The picture itself, straight through - it is already a PNG.</summary>
		private void CreateProject(HttpListenerContext context)
		{
			JsonNode body = ReadBody(context);
			string name = (string)body?["name"];
			List<string> targets = (body?["targets"] as JsonArray)?
				.Select(n => (string)n).Where(s => s != null).ToList()
				?? new List<string>();

			try
			{
				Project project = Project.Create(name, targets);
				OpenProject(project);
				SendJson(context, new { ok = true, name = project.File.Name,
					directory = project.Directory, active = project.File.Active });
			}
			catch (Exception ex) when (ex is ArgumentException or IOException)
			{
				SendJson(context, new { ok = false, error = ex.Message });
			}
		}

		private void OpenProjectRequest(HttpListenerContext context)
		{
			string directory = (string)ReadBody(context)?["directory"];
			Project project = directory == null ? null : Project.TryOpen(directory);
			if (project == null)
			{
				SendJson(context, new { ok = false, error = "no project in " + directory });
				return;
			}
			try
			{
				OpenProject(project);
				SendJson(context, new { ok = true, name = project.File.Name,
					active = project.File.Active });
			}
			catch (Exception ex) when (ex is FileNotFoundException or IOException)
			{
				SendJson(context, new { ok = false, error = ex.Message });
			}
		}

		/// <summary>
		/// Switches which game the open project is edited against. The edits do not
		/// move - the same files directory is the override either way - so this only
		/// changes what they are read on top of.
		/// </summary>
		private void SetTarget(HttpListenerContext context)
		{
			string target = (string)ReadBody(context)?["target"];
			if (_project == null)
			{
				SendJson(context, new { ok = false, error = "no project is open" });
				return;
			}
			if (!Targets.Known(target))
			{
				SendJson(context, new { ok = false, error = "no target called " + target });
				return;
			}

			string was = _project.File.Active;
			_project.File.Active = target;
			if (!_project.File.Targets.Contains(target, StringComparer.OrdinalIgnoreCase))
			{
				_project.File.Targets.Add(target);
			}
			try
			{
				OpenProject(_project);
				_project.Save();
				_current = _sessions[_active];
				SendJson(context, new { ok = true, active = _active,
					content = _workspace.ContentDirectory, kind = _workspace.Kind });
			}
			catch (Exception ex) when (ex is FileNotFoundException or IOException)
			{
				// Put it back, so a target that cannot be opened does not leave the
				// project pointing at content that is not there.
				_project.File.Active = was;
				SendJson(context, new { ok = false, error = ex.Message });
			}
		}

		private void SaveProjectDetails(HttpListenerContext context)
		{
			if (_project == null)
			{
				SendJson(context, new { ok = false, error = "no project is open" });
				return;
			}
			JsonNode body = ReadBody(context);
			foreach ((string key, Action<string> set) in new (string, Action<string>)[]
			{
				("name", v => _project.File.Name = v),
				("author", v => _project.File.Author = v),
				("version", v => _project.File.Version = v),
				("description", v => _project.File.Description = v)
			})
			{
				if (body?[key] != null)
				{
					set((string)body[key]);
				}
			}

			// Targets can change too. Adding one opens a session for it; removing one
			// closes it, but the edits made for it stay on disk - deleting a folder of
			// somebody's work is not what a settings dialog should do.
			if (body?["targets"] is JsonArray wanted)
			{
				List<string> targets = wanted.Select(n => (string)n)
					.Where(Targets.Known).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
				if (targets.Count == 0)
				{
					SendJson(context, new { ok = false, error = "a project needs at least one game" });
					return;
				}
				_project.File.Targets = targets;
				if (!targets.Contains(_project.File.Active, StringComparer.OrdinalIgnoreCase))
				{
					_project.File.Active = targets[0];
				}
				try
				{
					OpenProject(_project);
				}
				catch (FileNotFoundException ex)
				{
					SendJson(context, new { ok = false, error = ex.Message });
					return;
				}
			}
			_project.Save();
			SendJson(context, new { ok = true, active = _active });
		}

		/// <summary>
		/// The project as a zip beside the projects folder: project.json, the edited
		/// files per target, and a README saying what it is and how to install it. What
		/// a modder uploads, so it should not need assembling by hand.
		/// </summary>
		private void ExportProject(HttpListenerContext context)
		{
			if (_project == null)
			{
				SendJson(context, new { ok = false, error = "no project is open" });
				return;
			}
			try
			{
				string zip = ProjectExport.Write(_project);
				SendJson(context, new { ok = true, path = zip, bytes = new FileInfo(zip).Length });
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				SendJson(context, new { ok = false, error = ex.Message });
			}
		}

		/// <summary>The project's C# code: make it, build it, or open it in the machine's editor.</summary>
		private void ProjectCode(HttpListenerContext context, string action)
		{
			if (_project == null)
			{
				SendJson(context, new { ok = false, error = "no project is open" });
				return;
			}
			try
			{
				switch (action)
				{
					case "create":
					{
						string csproj = ModCode.Create(_project);
						SendJson(context, new { ok = true, path = csproj, has = true });
						return;
					}
					case "build":
					{
						bool built = ModCode.Build(_project, out string output, out List<ModCode.Problem> problems);
						SendJson(context, new { ok = built, output, problems, error = built ? null : "the build failed", assemblies = ModCode.Assemblies(_project).Select(Path.GetFileName).ToList() });
						return;
					}
					case "open":
						ModCode.Open(_project);
						SendJson(context, new { ok = true });
						return;
				}
				SendJson(context, new { ok = false, error = "unknown code action " + action });
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or System.ComponentModel.Win32Exception)
			{
				SendJson(context, new { ok = false, error = ex.Message });
			}
		}

		/// <summary>
		/// The mod's own files - code/, scenes/, project.json - as the project tree's "OpenFF
		/// mod" folder shows them: the list, one file's text, a save, a new C# file, and
		/// opening one with the machine's editor. With no project open the list is empty
		/// rather than an error, since the folder is always in the tree.
		/// </summary>
		private void ProjectFiles(HttpListenerContext context, string action)
		{
			if (_project == null)
			{
				if (action == "files")
				{
					SendJson(context, new { ok = true, project = (string)null, code = false, files = new List<ModCode.Entry>() });
					return;
				}
				SendJson(context, new { ok = false, error = "no project is open" });
				return;
			}
			try
			{
				switch (action)
				{
					case "files":
						SendJson(context, new
						{
							ok = true,
							project = _project.File.Name,
							directory = _project.Directory,
							code = ModCode.Has(_project),
							csproj = ModCode.Has(_project) ? Path.GetRelativePath(_project.Directory, ModCode.ProjectFile(_project)).Replace('\\', '/') : null,
							built = ModCode.Assemblies(_project).Select(Path.GetFileName).ToList(),
							files = ModCode.Tree(_project)
						});
						return;
					case "file":
					{
						string name = Query(context, "name");
						string text = ModCode.ReadText(_project, name, out ModCode.Entry entry);
						SendJson(context, new { ok = true, name = entry.Name, kind = entry.Kind, entry.Bytes, entry.Modified, entry.ReadOnly, text });
						return;
					}
					case "file/save":
					{
						JsonNode body = ReadBody(context);
						ModCode.Entry entry = ModCode.WriteText(_project, body?["name"]?.GetValue<string>(), body?["text"]?.GetValue<string>());
						SendJson(context, new { ok = true, name = entry.Name, entry.Bytes, entry.Modified });
						return;
					}
					case "file/new":
					{
						JsonNode body = ReadBody(context);
						string made = ModCode.CreateFile(_project, body?["name"]?.GetValue<string>(), body?["template"]?.GetValue<string>());
						SendJson(context, new { ok = true, name = made });
						return;
					}
					case "file/open":
					{
						JsonNode body = ReadBody(context);
						ModCode.OpenFile(_project, body?["name"]?.GetValue<string>());
						SendJson(context, new { ok = true });
						return;
					}
				}
				SendJson(context, new { ok = false, error = "unknown file action " + action });
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or ArgumentException or System.ComponentModel.Win32Exception)
			{
				SendJson(context, new { ok = false, error = ex.Message });
			}
		}

		/// <summary>The behaviours and services the project's built code offers, for the inspector.</summary>
		private void ProjectCatalog(HttpListenerContext context)
		{
			if (_project == null)
			{
				SendJson(context, new { ok = false, error = "no project is open" });
				return;
			}
			try
			{
				ModCatalogResult catalog = ModCatalog.Read(_project);
				// The built types, and the classes the source declares (built or not) with their files.
				SendJson(context, new { ok = true, code = ModCode.Has(_project), built = catalog.Assemblies.Count > 0, catalog.Behaviours, catalog.Services, catalog.Assemblies, catalog.Problems, sources = ModCode.Sources(_project) });
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or BadImageFormatException)
			{
				SendJson(context, new { ok = false, error = ex.Message });
			}
		}

		/// <summary>A map's scene file: the behaviours attached to its objects (?map=).</summary>
		private void ProjectScene(HttpListenerContext context)
		{
			if (_project == null)
			{
				SendJson(context, new { ok = false, error = "no project is open" });
				return;
			}
			string map = Query(context, "map");
			if (string.IsNullOrWhiteSpace(map))
			{
				SendJson(context, new
				{
					ok = true,
					maps = ProjectScenes.Maps(_project).Select(m => new { map = m.Map, attachments = m.Attachments, points = m.Points, bytes = m.Bytes, modified = m.Modified })
				});
				return;
			}
			try
			{
				SendJson(context, new { ok = true, map, attachments = ProjectScenes.Read(_project, map), objects = ProjectScenes.Objects(_project, map) });
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
			{
				SendJson(context, new { ok = false, error = ex.Message });
			}
		}

		/// <summary>Writes a map's scene file: { map, attachments: [ { target, behaviour, fields } ] }.</summary>
		private void SaveProjectScene(HttpListenerContext context)
		{
			if (_project == null)
			{
				SendJson(context, new { ok = false, error = "no project is open" });
				return;
			}
			try
			{
				JsonNode body = ReadBody(context);
				string map = body?["map"]?.GetValue<string>();
				JsonArray attachments = body?["attachments"] as JsonArray;
				JsonArray objects = body?["objects"] as JsonArray ?? body?["points"] as JsonArray;
				if (string.IsNullOrWhiteSpace(map))
				{
					SendJson(context, new { ok = false, error = "which map?" });
					return;
				}
				ProjectScenes.Write(_project, map, attachments, objects);
				SendJson(context, new { ok = true, map, count = attachments?.Count ?? 0, points = objects?.Count ?? 0 });
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException or JsonException)
			{
				SendJson(context, new { ok = false, error = ex.Message });
			}
		}

		/// <summary>
		/// What the page had open, kept with the project as session.json - the tabs, which
		/// was focused, which library and game the panel showed - so opening the project
		/// again lands where the work stopped. GET reads it ({} when there is none), POST
		/// writes whatever the page sends; the page owns the shape. It is the page's state,
		/// not the mod's, so the export leaves it out.
		/// </summary>
		private void ProjectSession(HttpListenerContext context)
		{
			if (_project == null)
			{
				SendJson(context, new { ok = false, error = "no project is open" });
				return;
			}
			string path = Path.Combine(_project.Directory, "session.json");
			try
			{
				if (string.Equals(context.Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
				{
					JsonNode body = ReadBody(context) ?? new JsonObject();
					File.WriteAllText(path, body.ToJsonString(new JsonSerializerOptions { WriteIndented = true }), new UTF8Encoding(false));
					SendJson(context, new { ok = true });
					return;
				}
				if (!File.Exists(path))
				{
					SendJson(context, new { ok = true, session = (object)null });
					return;
				}
				JsonNode session = JsonNode.Parse(File.ReadAllText(path));
				SendJson(context, new { ok = true, session });
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
			{
				SendJson(context, new { ok = false, error = ex.Message });
			}
		}

		/// <summary>Export to OpenFF, then start the client (or leave a running one to hot-reload).</summary>
		private void RunProject(HttpListenerContext context)
		{
			if (_project == null)
			{
				SendJson(context, new { ok = false, error = "no project is open" });
				return;
			}
			string mods = OpenFFClient.ModsFolder();
			if (mods == null)
			{
				SendJson(context, new { ok = false, error = "the OpenFF client has not been found - start OpenFF.exe once (it records where it is), or build OpenFF beside this repository" });
				return;
			}
			try
			{
				string directory = ProjectExport.WriteToOpenFF(_project, mods);
				bool running = OpenFFClient.IsRunning();
				// "Play here": the map that is open and a spot on it, straight into the client -
				// the game the page is looking at, since the two name their maps alike.
				JsonNode body = ReadBody(context);
				string map = body?["map"]?.GetValue<string>();
				JsonArray pos = body?["pos"] as JsonArray;
				string ws = Query(context, "ws");
				string game = Targets.Known(ws) ? Targets.GameOf(ws) : "ff3";
				List<string> arguments = new List<string>();
				if (!string.IsNullOrWhiteSpace(map))
				{
					arguments.Add("--game=" + game);
					arguments.Add("--map=" + map.Trim());
					if (pos != null && pos.Count >= 3)
					{
						arguments.Add("--pos=" + string.Join(",", pos.Take(3).Select(p => Math.Round(p?.GetValue<double>() ?? 0).ToString(CultureInfo.InvariantCulture))));
					}
				}
				if (!running)
				{
					OpenFFClient.Launch(arguments.ToArray());
				}
				SendJson(context, new { ok = true, path = directory, started = !running, running, arguments });
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or System.ComponentModel.Win32Exception)
			{
				SendJson(context, new { ok = false, error = ex.Message });
			}
		}

		/// <summary>Writes the project as a mod into the OpenFF client's mods folder.</summary>
		private void ExportProjectToOpenFF(HttpListenerContext context)
		{
			if (_project == null)
			{
				SendJson(context, new { ok = false, error = "no project is open" });
				return;
			}
			string mods = OpenFFClient.ModsFolder();
			if (mods == null)
			{
				SendJson(context, new { ok = false, error = "the OpenFF client has not been found - start OpenFF.exe once (it records where it is), or build OpenFF beside this repository" });
				return;
			}
			try
			{
				string directory = ProjectExport.WriteToOpenFF(_project, mods);
				// The files per game (ff3/files, ff4/files) and any in the shared files/.
				int files = OpenFF.Content.ModsFolder.Games.Select(g => Path.Combine(directory, g, "files")).Append(Path.Combine(directory, "files"))
					.Where(Directory.Exists)
					.Sum(folder => Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories).Count());
				SendJson(context, new { ok = true, path = directory, files, mods });
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
			{
				SendJson(context, new { ok = false, error = ex.Message });
			}
		}

		/// <summary>Opens the project's folder (or a file in it) in the file manager.</summary>
		private void RevealProject(HttpListenerContext context)
		{
			if (_project == null)
			{
				SendJson(context, new { ok = false, error = "no project is open" });
				return;
			}
			string path = (string)ReadBody(context)?["path"] ?? _project.Directory;
			try
			{
				ProjectExport.Reveal(path);
				SendJson(context, new { ok = true });
			}
			catch (Exception ex)
			{
				SendJson(context, new { ok = false, error = ex.Message });
			}
		}

		private void GetImage(HttpListenerContext context)
		{
			string name = Query(context, "name");
			try
			{
				Send(context, 200, "image/png", _workspace.Read(name));
			}
			catch (Exception)
			{
				Send(context, 404, "text/plain", Encoding.UTF8.GetBytes("no such image"));
			}
		}

		/// <summary>
		/// Replaces one. The body is the PNG itself rather than a form, because that is
		/// all there is to send. It is checked before it is written - a file the game
		/// cannot read is worse than no change at all - and the old size is reported
		/// back, since the game lays some of these out expecting a particular one.
		/// </summary>
		private void UploadImage(HttpListenerContext context)
		{
			string name = Query(context, "name");
			using MemoryStream body = new MemoryStream();
			context.Request.InputStream.CopyTo(body);
			byte[] data = body.ToArray();

			ImageInfo was;
			try
			{
				was = Images.Describe(_workspace.Read(name));
			}
			catch (Exception ex)
			{
				SendJson(context, new { ok = false, error = "cannot read the original: " + ex.Message });
				return;
			}

			ImageInfo now;
			try
			{
				now = Images.Describe(data);
			}
			catch (Exception)
			{
				SendJson(context, new
				{
					ok = false,
					error = "that file is not a PNG. The game reads these as PNG, so it "
						+ "has to be one."
				});
				return;
			}

			_workspace.Write(name, data);
			SendJson(context, new
			{
				ok = true,
				bytes = data.Length,
				width = now.Width,
				height = now.Height,
				resized = now.Width != was.Width || now.Height != was.Height,
				wasWidth = was.Width,
				wasHeight = was.Height
			});
		}

		/// <summary>One part of a sound, as a wav the browser can play.</summary>
		private void GetAudioWav(HttpListenerContext context)
		{
			string name = Query(context, "name");
			int part = int.TryParse(Query(context, "part"), NumberStyles.Integer,
				CultureInfo.InvariantCulture, out int value) ? value : 0;

			// The name reaches the archives and the file system, so keep it to what a
			// sound is actually called.
			if (string.IsNullOrEmpty(name) || name.IndexOfAny(
					new[] { '/', Path.DirectorySeparatorChar, '.', ':' }) >= 0)
			{
				Send(context, 400, "text/plain", Encoding.UTF8.GetBytes("bad sound name"));
				return;
			}

			try
			{
				byte[] sound = Audio.Playable(_workspace, name, part, out string contentType);
				Send(context, 200, contentType, sound);
			}
			catch (FileNotFoundException)
			{
				Send(context, 404, "text/plain", Encoding.UTF8.GetBytes("no such sound"));
			}
		}

		/// <summary>
		/// The whole instruction set, with operand names and types. The editor needs
		/// it to highlight, to complete and to say what an argument is for - none of
		/// which should mean a round trip per keystroke, so it is sent once.
		/// </summary>
		private void GetOps(HttpListenerContext context)
		{
			List<object> ops = new List<object>();
			ScriptOpTable table = _workspace.Ops;
			foreach (KeyValuePair<string, int> entry in table.Names.All)
			{
				ScriptOp op = table.Get(entry.Value);
				Operand[] operands = op.Operands ?? Array.Empty<Operand>();
				ops.Add(new
				{
					name = entry.Key,
					opcode = entry.Value,
					handler = op.Name,
					operands = operands.Select((operand, i) => new
					{
						type = operand.ToString().ToLowerInvariant(),
						name = table.OperandName(entry.Value, i),
						@fixed = table.IsFixed(entry.Value, i)
					}).ToArray()
				});
			}
			// Conditions travel with them, so the editor does not keep its own copy of
			// a list that is derived from the opcode table in the first place.
			SendJson(context, new
			{
				ops,
				conditions = Ffs.Conditions.Forms(table)
					.OrderBy(form => form.Key, StringComparer.Ordinal)
					.Select(form => new
					{
						name = form.Key,
						arguments = form.Value.Arguments
					})
					.ToArray(),
				comparisons = Ffs.Conditions.Comparisons.Keys.ToArray()
			});
		}

		/// <summary>
		/// Message ids to their text, for the menu preview. A widget's label is a
		/// message id, so this is what turns a box marked com_item into one that says
		/// "Item". Ids with no message are left out rather than guessed at.
		/// </summary>
		private void GetMessages(HttpListenerContext context)
		{
			JsonNode body = ReadBody(context);
			Dictionary<string, string> found = new Dictionary<string, string>();

			if (_lookupMessage != null && body["ids"] != null)
			{
				foreach (JsonNode id in body["ids"].AsArray())
				{
					// Menu parameters are not all message ids, and some are negative
					// or larger than a message id can be. Skip those rather than
					// letting one odd widget take the whole preview down.
					if (!long.TryParse(id?.ToJsonString(), NumberStyles.Integer,
							CultureInfo.InvariantCulture, out long value)
						|| value < 0 || value > uint.MaxValue)
					{
						continue;
					}
					string text = _lookupMessage((uint)value);
					if (text != null)
					{
						found[value.ToString(CultureInfo.InvariantCulture)] = text;
					}
				}
			}

			SendJson(context, new { available = _lookupMessage != null, messages = found });
		}

		// ------------------------------------------------------------------- tables

		private void GetTable(HttpListenerContext context)
		{
			string name = Query(context, "name");
			byte[] data = _workspace.Read(name);
			// FF4's loose tables are LZ-compressed on disk; the layout is of the bytes inside.
			bool compressed = name.EndsWith(".lz", StringComparison.OrdinalIgnoreCase);
			if (compressed)
			{
				data = Lz.Decompress(data);
			}
			int chains = data.Length >= 4 ? BitConverter.ToInt32(data, 0) : 0;
			string family = Pak.FamilyOf(compressed ? name.Substring(0, name.Length - 3) : name,
				chains, _workspace.Game);
			PakFile decoded = Pak.Read(data, family, _lookupMessage);

			SendJson(context, new
			{
				name,
				family,
				overridden = _workspace.IsOverridden(name),
				// What each chain is for, so the grid is not 34 columns of mystery.
				notes = PakRecords.Chains.Concat(PakRecordsFf4.Chains)
					.Where(chain => chain.Family == family)
					.ToDictionary(chain => chain.Label, chain => chain.Note),
				// The decoded file goes across as it stands. Chains with no known
				// layout keep their bytes, so a table with one unmodelled chain can
				// still be edited and saved without losing it.
				file = JsonSerializer.SerializeToNode(decoded, Pak.Json)
			});
		}

		private void SaveTable(HttpListenerContext context)
		{
			JsonNode body = ReadBody(context);
			string name = (string)body["name"];
			PakFile file = body["file"].Deserialize<PakFile>(Pak.Json);

			byte[] data = Pak.Write(file);
			if (name.EndsWith(".lz", StringComparison.OrdinalIgnoreCase))
			{
				data = Lz.Compress(data);
			}
			_workspace.Write(name, data);
			SendJson(context, new { ok = true, bytes = data.Length, overridden = true });
		}

		// ------------------------------------------------------------------- revert

		/// <summary>The picked files, back to what the game shipped.</summary>
		private void RevertMany(HttpListenerContext context)
		{
			List<string> names = (ReadBody(context)?["names"] as JsonArray)?
				.Select(n => (string)n).Where(s => s != null).ToList()
				?? new List<string>();
			ModResult result = ModInstall.Revert(_workspace, names, Current.Installable);
			if (result.Reverted.Count > 0)
			{
				// A reverted .msd puts the shipped line back, and the views that show
				// dialogue have to see that rather than the one that was thrown away.
				_messages.Invalidate();
				_flags.Invalidate();
				_characterIds.Invalidate();
			}
			SendJson(context, result);
		}

		private void Revert(HttpListenerContext context)
		{
			JsonNode body = ReadBody(context);
			string name = (string)body["name"];
			// Through ModInstall rather than the workspace directly: if this edit had
			// been installed, deleting it here alone would leave the game still holding
			// it, and the file would look reverted everywhere except where it matters.
			ModResult result = ModInstall.Revert(_workspace, new[] { name }, Current.Installable);
			_messages.Invalidate();
			_flags.Invalidate();
			_characterIds.Invalidate();
			SendJson(context, new
			{
				ok = true,
				reverted = result.Reverted.Count > 0,
				overridden = _workspace.IsOverridden(name),
				restored = result.Restored.Count > 0,
				notes = result.Notes
			});
		}

		// ------------------------------------------------------------------ plumbing

		/// <summary>
		/// The files a browse kind lists. Tables need a second look: FF4 ships its three
		/// loose tables LZ-compressed, and Workspace.List matches on the last extension,
		/// which for item_parameter.pak.lz is .lz.
		/// </summary>
		private List<WorkspaceEntry> ListKind(string kind)
		{
			if (kind == "data")
			{
				// The unified game data: virtual documents, one per page (GameData).
				return GameData.PagesFor(_workspace).Select(page => new WorkspaceEntry { Name = page, Extension = "", Overridden = false, Size = 0 }).ToList();
			}
			List<WorkspaceEntry> found = _workspace.List(Extensions(kind));
			if (kind == "table")
			{
				found.AddRange(_workspace.List(".lz").Where(entry =>
					entry.Name.EndsWith(".pak.lz", StringComparison.OrdinalIgnoreCase)
					|| entry.Name.EndsWith(".chaindata.lz", StringComparison.OrdinalIgnoreCase)));
			}
			return found;
		}

		private static string[] Extensions(string kind)
		{
			switch (kind)
			{
				case "script": return new[] { ".script" };
				case "text": return new[] { ".msd" };
				case "menu": return new[] { ".xbn" };
				// FF4 ships its three tables loose and LZ-compressed: item_parameter.pak.lz.
				case "table": return new[] { ".pak", ".chaindata", ".pak.lz", ".chaindata.lz" };
				default: return Array.Empty<string>();
			}
		}

		private static string Query(HttpListenerContext context, string key)
		{
			return context.Request.QueryString[key];
		}

		private static JsonNode ReadBody(HttpListenerContext context)
		{
			using StreamReader reader = new StreamReader(
				context.Request.InputStream, Encoding.UTF8);
			return JsonNode.Parse(reader.ReadToEnd());
		}

		private static void SendJson(HttpListenerContext context, object value)
		{
			Send(context, 200, "application/json; charset=utf-8",
				Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value, Json)));
		}

		private void ServeStatic(HttpListenerContext context, string path)
		{
			string relative = path == "/" ? "index.html" : path.TrimStart('/');
			string full = Path.GetFullPath(Path.Combine(_webRoot, relative));

			if (!full.StartsWith(_webRoot + Path.DirectorySeparatorChar, StringComparison.Ordinal)
				|| !File.Exists(full))
			{
				Send(context, 404, "text/plain", Encoding.UTF8.GetBytes("not found"));
				return;
			}

			// The editor is rebuilt while it is open, so a browser holding on to an old
			// copy of a script is a real trap - it shows a bug that has already been
			// fixed, or hides one that has not.
			context.Response.Headers["Cache-Control"] = "no-store, must-revalidate";
			Send(context, 200, ContentType(full), File.ReadAllBytes(full));
		}

		private static string ContentType(string path)
		{
			switch (Path.GetExtension(path).ToLowerInvariant())
			{
				case ".html": return "text/html; charset=utf-8";
				case ".js": return "text/javascript; charset=utf-8";
				case ".css": return "text/css; charset=utf-8";
				case ".svg": return "image/svg+xml";
				default: return "application/octet-stream";
			}
		}

		private static void Send(HttpListenerContext context, int status, string type, byte[] body)
		{
			context.Response.StatusCode = status;
			context.Response.ContentType = type;
			context.Response.ContentLength64 = body.Length;
			// A HEAD asks for the headers alone (a preview tool probing the server does that).
			if (!string.Equals(context.Request.HttpMethod, "HEAD", StringComparison.OrdinalIgnoreCase))
			{
				context.Response.OutputStream.Write(body, 0, body.Length);
			}
			context.Response.OutputStream.Close();
		}
	}
}
