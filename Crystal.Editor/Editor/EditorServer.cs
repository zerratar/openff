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
		/// project targeting all of them would (direction, 2026-09-04). Each install found gets
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
							items = ProjectItems.All(_project).Count,
							characters = ProjectCharacters.All(_project).Count,
							text = ProjectText.Lines(_project).Count,
							monsters = ProjectMonsters.All(_project).Count,
							formations = ProjectMonsters.AllFormations(_project).Count,
							jobs = ProjectJobs.All(_project).Count
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

				case "/api/samples":
					// The sample mods shipped beside Crystal (or the repository's), for Sample projectsâ€¦.
					SendJson(context, new { ok = true, folder = Samples.Folder(), samples = Samples.All() });
					return;

				case "/api/samples/open":
				{
					// A sample copied into a new project and opened.
					JsonNode body = ReadBody(context);
					try
					{
						Project project = Samples.OpenAsProject(body?["id"]?.GetValue<string>(), body?["name"]?.GetValue<string>());
						OpenProject(project);
						SendJson(context, new { ok = true, name = project.File.Name, directory = project.Directory, active = project.File.Active, code = ModCode.Has(project) });
					}
					catch (Exception ex) when (ex is ArgumentException or IOException or InvalidOperationException)
					{
						SendJson(context, new { ok = false, error = ex.Message });
					}
					return;
				}

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

				case "/api/project/maps/new":
				{
					// A map of the mod's own: { title, kind: town|dungeon, ground: assets/x.gltf or "", size, bgm }.
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					JsonNode body = ReadBody(context);
					try
					{
						string map = ProjectScenes.Create(_project,
							body?["title"]?.GetValue<string>(), body?["kind"]?.GetValue<string>(), body?["ground"]?.GetValue<string>(),
							body?["size"]?.GetValue<int>() ?? 100, body?["bgm"]?.GetValue<int>() ?? 0,
							candidate => _workspace.Exists("files/" + candidate + ".hich") || _workspace.Exists("files/" + candidate + ".pak"));
						SendJson(context, new { ok = true, map });
					}
					catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException or JsonException or FormatException)
					{
						SendJson(context, new { ok = false, error = ex.Message });
					}
					return;
				}

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

				case "/api/project/items/model-from-gltf":
				{
					// A weapon's look in the game's own format: the project's glTF written as a w### model
					// (Mdl0Write: <name>.nmdp.lz and <name>.ntxp.lz into this target's files, under the first
					// number from 300 the game does not use) and the definition's graphId set to it. What a
					// Steam target needs - it reads only the game's formats - and an option on an OpenFF one.
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					JsonNode body = ReadBody(context);
					try
					{
						string id = body?["id"]?.GetValue<string>();
						string asset = body?["asset"]?.GetValue<string>();
						float scale = (float)(body?["scale"]?.GetValue<double>() ?? 1.0);
						if (string.IsNullOrWhiteSpace(asset)) throw new ArgumentException("no glTF named");
						string gltfPath = GltfBundle.Resolve(_project, asset) ?? throw new ArgumentException("no file " + asset + " in the project");
						ModItem item = string.IsNullOrEmpty(id) ? null : ProjectItems.All(_project).FirstOrDefault(i => string.Equals(i.Id, id, StringComparison.OrdinalIgnoreCase));
						// The number: the definition's own graphId when it already names a model of the project's, else the first free from 300.
						int number = -1;
						if (item != null && item.Fields.TryGetValue("graphId", out int have) && have >= 300 && _workspace.IsOverridden("files/w" + have.ToString("000") + ".nmdp.lz")) number = have;
						for (int n = 300; number < 0 && n < 1000; n++) if (!_workspace.Exists("files/w" + n.ToString("000") + ".nmdp.lz")) number = n;
						if (number < 0) throw new InvalidOperationException("no free weapon model number");
						string name = "w" + number.ToString("000");
						// The fit (rotation in degrees about x, y, z; offset) as the form has it, else the definition's.
						float[] rotation = Triple(body?["rotation"]) ?? item?.ModelRotation, offset = Triple(body?["offset"]) ?? item?.ModelOffset;
						Mdl0Write.Result made = Mdl0Write.Build(OpenFF.Graphics.GltfFile.Load(gltfPath), name, scale, rotation, offset, generous: Crystal.Editor.Targets.IsOurs(Current.Target));
						_workspace.Write("files/" + name + ".nmdp.lz", Lz.Compress(made.Nmdp));
						_workspace.Write("files/" + name + ".ntxp.lz", Lz.Compress(made.Ntxp));
						if (item != null)
						{
							item.Fields["graphId"] = number;
							ProjectItems.Save(_project, item);
						}
						SendJson(context, new { ok = true, model = name, number, triangles = made.Triangles, vertices = made.Vertices, materials = made.Materials, notes = made.Notes, item = item != null ? ProjectItems.Describe(_workspace, item, _lookupMessage) : null });
					}
					catch (Exception ex) { SendJson(context, new { ok = false, error = ex.Message }); }
					return;
				}

				case "/api/project/models/weights":
				{
					// Painted weights back into a skinned glTF of the project's: { asset, jointIndex: [4 a vertex],
					// weights: [4 a vertex] } in the viewer's vertex order (GltfBundle's). The file is rewritten in place.
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					JsonNode body = ReadBody(context);
					try
					{
						string asset = body?["asset"]?.GetValue<string>();
						string gltfPath = GltfBundle.Resolve(_project, asset) ?? throw new ArgumentException("no file " + asset + " in the project");
						int[] joints = (body?["jointIndex"] as JsonArray ?? throw new ArgumentException("no weights")).Select(n => n?.GetValue<int>() ?? -1).ToArray();
						float[] weights = (body?["weights"] as JsonArray ?? throw new ArgumentException("no weights")).Select(n => (float)(n?.GetValue<double>() ?? 0)).ToArray();
						// With positions (3 a vertex): the bind pose carried again through the painted weights.
						float[] positions = body?["positions"] is JsonArray p ? p.Select(n => (float)(n?.GetValue<double>() ?? 0)).ToArray() : null;
						byte[] rewritten = Gltf.RewriteWeights(File.ReadAllBytes(gltfPath), joints, weights, positions);
						// Moved geometry with recalculated normals: the normals again, at the same angle.
						float? normalsAngle = positions != null ? Gltf.NormalsOf(rewritten).Angle : null;
						if (normalsAngle != null) rewritten = Gltf.RecalculateNormals(rewritten, normalsAngle.Value);
						File.WriteAllBytes(gltfPath, rewritten);
						SendJson(context, new { ok = true, asset, bytes = rewritten.Length, vertices = joints.Length / 4, positions = positions != null, normals = normalsAngle });
					}
					catch (Exception ex) { SendJson(context, new { ok = false, error = ex.Message }); }
					return;
				}

				case "/api/project/models/normals":
				{
					// A glTF's normals recalculated from its triangles at an angle - { asset, angle } - or the
					// file's own put back - { asset, revert: true }. The .glb is rewritten in place; the file's
					// own normals are kept inside it (_SOURCE_NORMAL) the first time, so a revert is exact.
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					JsonNode body = ReadBody(context);
					try
					{
						string asset = body?["asset"]?.GetValue<string>();
						string gltfPath = GltfBundle.Resolve(_project, asset) ?? throw new ArgumentException("no file " + asset + " in the project");
						if (!gltfPath.EndsWith(".glb", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("only a .glb is rewritten in place - export the model as .glb first");
						byte[] file = File.ReadAllBytes(gltfPath);
						bool revert = body?["revert"]?.GetValue<bool>() ?? false;
						byte[] rewritten;
						if (revert)
						{
							rewritten = Gltf.RevertNormals(file) ?? throw new ArgumentException("the file has no normals of its own kept to go back to");
						}
						else
						{
							float angle = (float)(body?["angle"]?.GetValue<double>() ?? 60);
							rewritten = Gltf.RecalculateNormals(file, angle);
						}
						File.WriteAllBytes(gltfPath, rewritten);
						(float? nowAngle, bool kept) = Gltf.NormalsOf(rewritten);
						SendJson(context, new { ok = true, asset, bytes = rewritten.Length, angle = nowAngle, source = kept });
					}
					catch (Exception ex) { SendJson(context, new { ok = false, error = ex.Message }); }
					return;
				}

				case "/api/project/models/bind":
				{
					// Which game model a glTF asset stands in for, set by hand: { asset, model } writes
					// defs/models/<model>.json naming the file (moving an existing definition of this file,
					// with its clips and fit, from another model; a definition already on that model is
					// taken over); { asset, model: null } takes the file's definition away, so the game's
					// own model draws again. Remake does the same with a rig; this is the plain switch.
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					JsonNode body = ReadBody(context);
					try
					{
						string asset = body?["asset"]?.GetValue<string>() ?? throw new ArgumentException("no file named");
						if (GltfBundle.Resolve(_project, asset) == null) throw new ArgumentException("no file " + asset + " in the project");
						string target = body?["model"]?.GetValue<string>();
						if (!string.IsNullOrWhiteSpace(target)) { target = Path.GetFileName(target); int dot = target.IndexOf('.'); if (dot > 0) target = target.Substring(0, dot); }
						string folder = Path.Combine(_project.Directory, "defs", "models");
						List<OpenFF.Data.ModModel> all = OpenFF.Data.ModModels.Load(new[] { _project.Directory });
						OpenFF.Data.ModModel mine = all.FirstOrDefault(d => string.Equals(d.Gltf?.Replace('\\', '/'), asset.Replace('\\', '/'), StringComparison.OrdinalIgnoreCase));
						List<string> notes = new List<string>();
						// The file's old definition goes (it is moved, or taken away).
						if (mine != null && (string.IsNullOrWhiteSpace(target) || !string.Equals(mine.Model, target, StringComparison.OrdinalIgnoreCase)))
						{
							if (File.Exists(mine.Source)) File.Delete(mine.Source);
							notes.Add(mine.Model + " draws as the game's own again");
						}
						if (string.IsNullOrWhiteSpace(target)) { SendJson(context, new { ok = true, asset, model = (string)null, notes }); return; }
						if (!System.Text.RegularExpressions.Regex.IsMatch(target, @"^[a-z]\d{3}$", System.Text.RegularExpressions.RegexOptions.IgnoreCase)) throw new ArgumentException("not a game model: " + target + " (a character is j101â€¦j423, a monster m###)");
						OpenFF.Data.ModModel definition = mine ?? new OpenFF.Data.ModModel();
						definition.Model = target;
						definition.Gltf = asset.Replace('\\', '/');
						// A fitted skeleton, when the auto-rig's record beside the original says so.
						if (mine == null && System.Text.RegularExpressions.Regex.IsMatch(asset, @"-rigged\.glb$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
						{
							JsonObject record = LoadCutsFile(System.Text.RegularExpressions.Regex.Replace(GltfBundle.Resolve(_project, asset), @"-rigged\.glb$", ".glb", System.Text.RegularExpressions.RegexOptions.IgnoreCase)) as JsonObject;
							definition.Fitted = record?["rigged"]?["fitted"] is JsonValue rf && rf.TryGetValue(out bool rfv) && rfv;
						}
						OpenFF.Data.ModModel other = all.FirstOrDefault(d => d != mine && string.Equals(d.Model, target, StringComparison.OrdinalIgnoreCase));
						if (other != null) notes.Add(target + " was drawn as " + Path.GetFileName(other.Gltf ?? "") + " - this file takes its place");
						Directory.CreateDirectory(folder);
						string definitionPath = Path.Combine(folder, target + ".json");
						File.WriteAllText(definitionPath, definition.ToJson());
						// The game-format override of that model, if one is left from a Steam-style remake, would fight the glTF.
						_workspace.Revert("files/" + target + ".nmdp.lz"); _workspace.Revert("files/" + target + ".ntxp.lz");
						notes.Add("the OpenFF client draws " + Path.GetFileName(asset) + " in place of " + target + " (defs/models/" + target + ".json)" + (definition.Fitted ? ", a fitted skeleton" : ""));
						SendJson(context, new { ok = true, asset, model = target, definition = "defs/models/" + target + ".json", fitted = definition.Fitted, notes });
					}
					catch (Exception ex) { SendJson(context, new { ok = false, error = ex.Message }); }
					return;
				}

				case "/api/project/models/kind":
				{
					// What a glTF asset is for - { asset, kind } with kind one of Gltf.Kinds, or "" for the
					// inferred kind again. Written into the .glb (asset.extras.kind); the pickers read it.
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					JsonNode body = ReadBody(context);
					try
					{
						string asset = body?["asset"]?.GetValue<string>();
						string gltfPath = GltfBundle.Resolve(_project, asset) ?? throw new ArgumentException("no file " + asset + " in the project");
						if (!gltfPath.EndsWith(".glb", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("only a .glb carries a kind - export the model as .glb first");
						byte[] rewritten = Gltf.SetKind(File.ReadAllBytes(gltfPath), body?["kind"]?.GetValue<string>());
						File.WriteAllBytes(gltfPath, rewritten);
						(string kind, bool flagged) = Gltf.KindOf(rewritten);
						SendJson(context, new { ok = true, asset, kind, flagged, kinds = Gltf.Kinds });
					}
					catch (Exception ex) { SendJson(context, new { ok = false, error = ex.Message }); }
					return;
				}

				case "/api/model/carry":
				{
					// How a rigged file's geometry is carried: for assets/<name>-rigged.glb, the original's
					// vertices as the auto-rig fitted them (before the carry into the bind pose) and the pose
					// they were carried out of, from the record beside the original (assets/<name>.rig.json).
					// The viewer carries them again through repainted weights, so a part painted onto another
					// bone moves to where that bone has it in the bind pose, and no weights at all shows the
					// file's own pose.
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					try
					{
						string name = Query(context, "name") ?? throw new ArgumentException("no file named");
						string rigged = GltfBundle.Resolve(_project, name) ?? throw new ArgumentException("no file " + name + " in the project");
						JsonObject record = null; string originPath = null;
						foreach (string candidate in new[] { System.Text.RegularExpressions.Regex.Replace(rigged, @"-rigged\.glb$", ".glb", System.Text.RegularExpressions.RegexOptions.IgnoreCase) })
						{
							JsonObject r = LoadCutsFile(candidate) as JsonObject;
							if (r?["rigged"] is JsonObject rig && string.Equals(rig["file"]?.GetValue<string>()?.Replace('\\', '/'), name.Replace('\\', '/'), StringComparison.OrdinalIgnoreCase)) { record = rig; originPath = candidate; }
						}
						if (record == null) { SendJson(context, new { ok = false, error = "no record of how " + name + " was rigged (rigged before Crystal kept one, or not by the auto-rig)" }); return; }
						if (!File.Exists(originPath)) { SendJson(context, new { ok = false, error = "the original " + Path.GetFileName(originPath) + " is gone" }); return; }
						OpenFF.Graphics.GltfFile origin = OpenFF.Graphics.GltfFile.Load(originPath);
						float[] positions = AutoRig.FittedPositions(origin, (float)record["scale"].GetValue<double>(), Triple(record["rotation"]), Triple(record["offset"]));
						JsonObject pose = record["pose"] as JsonObject;
						SendJson(context, new { ok = true, positions, pose = pose == null ? null : new { pack = pose["pack"]?.GetValue<string>(), index = pose["index"]?.GetValue<int>() ?? 0, frame = pose["frame"]?.GetValue<int>() ?? 0 }, model = record["model"]?.GetValue<string>(), fitted = record["fitted"] is JsonValue fv && fv.TryGetValue(out bool f) && f });
					}
					catch (Exception ex) { SendJson(context, new { ok = false, error = ex.Message }); }
					return;
				}

				case "/api/project/models/reskin":
				{
					// A model of the game's remade from the project's glTF (Mdl0Reskin): the skeleton and
					// motions the game's, the mesh and textures the file's. { model: files/j101.nmdp.lz,
					// asset: assets/hero.glb } writes the model and its .ntxp as this target's overrides;
					// { model, revert: true } takes them out again.
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					JsonNode body = ReadBody(context);
					try
					{
						string modelName = body?["model"]?.GetValue<string>();
						if (string.IsNullOrWhiteSpace(modelName) || !modelName.EndsWith(".nmdp.lz", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("no model (files/j101.nmdp.lz) named");
						string texturesName = modelName.Substring(0, modelName.Length - 8) + ".ntxp.lz";
						string stem = Path.GetFileName(modelName);
						stem = stem.Substring(0, stem.IndexOf('.'));
						// On an OpenFF target the client draws the glTF itself, skinned with the file's
						// weights (defs/models/<stem>.json, CharacterMeshes); the game-format remake is
						// written as well - it is what the viewer shows and what a Steam target gets.
						bool ours = Crystal.Editor.Targets.IsOurs(Current.Target);
						string definition = Path.Combine(_project.Directory, OpenFF.Data.ModModels.Folder.Replace('/', Path.DirectorySeparatorChar), stem + ".json");
						if (body?["revert"]?.GetValue<bool>() == true)
						{
							// The game-format override goes; the definition too, unless asked to keep it
							// (the game's own model back in the viewer, the client still drawing the glTF).
							bool had = _workspace.Revert(modelName) | _workspace.Revert(texturesName);
							if (body?["keepDefinition"]?.GetValue<bool>() != true && File.Exists(definition)) { File.Delete(definition); had = true; }
							SendJson(context, new { ok = true, reverted = had });
							return;
						}
						string asset = body?["asset"]?.GetValue<string>();
						if (string.IsNullOrWhiteSpace(asset)) throw new ArgumentException("no glTF named");
						string gltfPath = GltfBundle.Resolve(_project, asset) ?? throw new ArgumentException("no file " + asset + " in the project");
						byte[] shipped = _workspace.ReadShipped(modelName) ?? throw new ArgumentException("the game has no " + modelName + " to remake");
						OpenFF.Graphics.GltfFile file = OpenFF.Graphics.GltfFile.Load(gltfPath);
						// A file with no rig is bound to the model's skeleton first (AutoRig), the rigged
						// copy written beside it as assets/<name>-rigged.glb and used from here on.
						List<string> rigNotes = new List<string>();
						string rigged = null;
						AutoRig.Cuts cutsUsed = null;
						// A fitted skeleton (joints moved to the file's own) is driven by retargeting: the
						// definition says so, and a rigged file's record remembers it.
						bool fittedRig = false;
						if (file.Skins.Count == 0 || !file.Meshes.Any(m => m.Skin >= 0 && m.Joints != null))
						{
							// The cuts: the request's, else the ones saved beside the file last time
							// (assets/<name>.rig.json), else all found; saved back as used.
							JsonNode saved = LoadCutsFile(gltfPath);
							AutoRig.Cuts cuts = ReadCuts(body?["cuts"]) ?? ReadCuts(saved);
							AutoRig.Markers markers = body?["markers"] != null ? ReadMarkers(body["markers"]) : ReadMarkers(saved?["markers"]);
							bool fittedAsked = (body?["fitted"] ?? saved?["fitted"]) is JsonValue fv && fv.TryGetValue(out bool f) && f;
							AutoRig.Result bound = AutoRig.Build(_workspace, modelName, file, (float)(body?["scale"]?.GetValue<double>() ?? 0), Triple(body?["rotation"]), Triple(body?["offset"]), cuts: cuts, markers: markers, fitted: fittedAsked);
							fittedRig = bound.Fitted;
							string riggedName = Path.GetFileNameWithoutExtension(asset) + "-rigged.glb";
							string riggedPath = Path.Combine(_project.Directory, GltfBundle.Folder, riggedName);
							File.WriteAllBytes(riggedPath, bound.Glb);
							cutsUsed = bound.Cuts;
							rigged = GltfBundle.Folder + "/" + riggedName;
							// Beside the original: the cuts asked for, and how the file was rigged - its fit and
							// the pose it was carried out of - so its geometry can be carried again through
							// repainted weights (/api/model/carry, the weights route).
							JsonObject record = (LoadCutsFile(gltfPath) as JsonObject) ?? new JsonObject();
							if (body?["cuts"] is JsonObject asked) foreach (KeyValuePair<string, JsonNode> pair in asked) record[pair.Key] = pair.Value?.DeepClone();
							if (body?["markers"] != null) record["markers"] = body["markers"].DeepClone();
							if (body?["fitted"] != null) record["fitted"] = body["fitted"].DeepClone();
							record["rigged"] = new JsonObject
							{
								["file"] = rigged, ["origin"] = asset.Replace('\\', '/'), ["model"] = modelName, ["fitted"] = bound.Fitted,
								["scale"] = bound.Scale, ["rotation"] = new JsonArray(bound.Rotation[0], bound.Rotation[1], bound.Rotation[2]), ["offset"] = new JsonArray(bound.Offset[0], bound.Offset[1], bound.Offset[2]),
								["pose"] = bound.PosePack == null ? null : new JsonObject { ["pack"] = bound.PosePack, ["index"] = bound.PoseIndex, ["frame"] = bound.PoseFrame }
							};
							SaveCutsFile(gltfPath, record);
							asset = rigged;
							gltfPath = riggedPath;
							file = OpenFF.Graphics.GltfFile.Load(gltfPath);
							rigNotes.Add("no rig in the file: bound to " + stem + "'s skeleton as " + rigged + " - " + string.Join("; ", bound.Notes));
						}
						if (ours)
						{
							// On OpenFF the client draws the glTF itself, weights and textures as the file has
							// them, so the game's model is left as it is - only the definition is written (and
							// an older game-format override of this model, from before, is taken back out: the
							// DS format snaps blends to one bone and shows spikes the glTF never has).
							// A rigged file saved again (repainted weights): its record beside the original says whether it is fitted.
							if (!fittedRig && System.Text.RegularExpressions.Regex.IsMatch(gltfPath, @"-rigged\.glb$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
							{
								JsonObject record = LoadCutsFile(System.Text.RegularExpressions.Regex.Replace(gltfPath, @"-rigged\.glb$", ".glb", System.Text.RegularExpressions.RegexOptions.IgnoreCase)) as JsonObject;
								fittedRig = record?["rigged"]?["fitted"] is JsonValue rf && rf.TryGetValue(out bool rfv) && rfv;
							}
							Directory.CreateDirectory(Path.GetDirectoryName(definition));
							File.WriteAllText(definition, new OpenFF.Data.ModModel { Model = stem, Gltf = asset.Replace('\\', '/'), Fitted = fittedRig }.ToJson());
							_workspace.Revert(modelName); _workspace.Revert(texturesName);
							List<string> notes = new List<string>(rigNotes) { "the OpenFF client draws " + Path.GetFileName(asset) + " in place of " + stem + " with the file's own weights and textures (defs/models/" + stem + ".json); the game's model itself is untouched" };
							int triangles = file.Meshes.Sum(m => m.Indices != null ? m.Indices.Length / 3 : 0), vertices = file.Meshes.Sum(m => m.VertexCount);
							SendJson(context, new { ok = true, model = stem, triangles, vertices, materials = file.Materials.Count, blended = 0, snapped = 0, notes, gltf = true, rigged, cuts = CutsJson(cutsUsed) });
							return;
						}
						Mdl0Reskin.Result made = Mdl0Reskin.Build(Lz.Decompress(shipped), file, stem, generous: false);
						made.Notes.InsertRange(0, rigNotes);
						_workspace.Write(modelName, Lz.Compress(made.Nmdp));
						_workspace.Write(texturesName, Lz.Compress(made.Ntxp));
						SendJson(context, new { ok = true, model = made.Model, triangles = made.Triangles, vertices = made.Vertices, materials = made.Materials, blended = made.Blended, snapped = made.Snapped, notes = made.Notes, gltf = false, rigged, cuts = CutsJson(cutsUsed) });
					}
					catch (Exception ex) { SendJson(context, new { ok = false, error = ex.Message }); }
					return;
				}

				case "/api/project/models/cuts":
				{
					// The auto-rig's regions for an unrigged glTF against a model, without rigging: { asset,
					// model, cuts? } -> the cuts as they would be used (blanks found from the shape), each
					// region's vertex count, and the cuts saved beside the file. Writes nothing.
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					JsonNode body = ReadBody(context);
					try
					{
						string asset = body?["asset"]?.GetValue<string>() ?? throw new ArgumentException("no glTF named");
						string modelName = body?["model"]?.GetValue<string>() ?? "files/j101.nmdp.lz";
						string gltfPath = GltfBundle.Resolve(_project, asset) ?? throw new ArgumentException("no file " + asset + " in the project");
						OpenFF.Graphics.GltfFile file = OpenFF.Graphics.GltfFile.Load(gltfPath);
						JsonNode saved = LoadCutsFile(gltfPath);
						AutoRig.Cuts cuts = ReadCuts(body?["cuts"]) ?? ReadCuts(saved);
						AutoRig.Markers markers = body?["markers"] != null ? ReadMarkers(body["markers"]) : ReadMarkers(saved?["markers"]);
						AutoRig.Result found = AutoRig.Build(_workspace, modelName, file, (float)(body?["scale"]?.GetValue<double>() ?? 0), Triple(body?["rotation"]), Triple(body?["offset"]), cuts: cuts, analyseOnly: true, markers: markers);
						SendJson(context, new { ok = true, cuts = CutsJson(found.Cuts), regions = found.RegionCounts, saved = saved != null ? CutsJson(ReadCuts(saved)) : null, savedMarkers = saved?["markers"]?.DeepClone(), savedFitted = saved?["fitted"] is JsonValue sf && sf.TryGetValue(out bool sfv) && sfv, leftIsPlusX = found.LeftIsPlusX, notes = found.Notes });
					}
					catch (Exception ex) { SendJson(context, new { ok = false, error = ex.Message }); }
					return;
				}

				case "/api/project/monsters":
				{
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					List<string> notes = new List<string>();
					List<ModMonster> all = ProjectMonsters.All(_project, notes);
					string one = Query(context, "id");
					if (!string.IsNullOrEmpty(one))
					{
						ModMonster m = all.FirstOrDefault(x => string.Equals(x.Id, one, StringComparison.OrdinalIgnoreCase));
						if (m == null) { SendJson(context, new { ok = false, error = "no monster definition '" + one + "'" }); return; }
						SendJson(context, new { ok = true, monster = ProjectMonsters.Describe(_workspace, m) });
						return;
					}
					SendJson(context, new { ok = true, monsters = all.Select(m => ProjectMonsters.Describe(_workspace, m)).ToList(), notes });
					return;
				}

				case "/api/project/monsters/save":
				{
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					JsonNode body = ReadBody(context);
					try
					{
						ModMonster m = ModMonster.Parse(body?.ToJsonString() ?? "{}");
						if (m == null) throw new ArgumentException("no monster");
						if (m.Id != null && m.Id.IndexOfAny(new[] { '/', '\\', '.' }) >= 0) throw new ArgumentException("a monster's id is a plain word");
						ProjectMonsters.Save(_project, m);
						SendJson(context, new { ok = true, monster = ProjectMonsters.Describe(_workspace, m) });
					}
					catch (Exception ex) { SendJson(context, new { ok = false, error = ex.Message }); }
					return;
				}

				case "/api/project/monsters/new":
				{
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					JsonNode body = ReadBody(context);
					try
					{
						string name = body?["name"]?.GetValue<string>();
						int baseId = body?["base"]?.GetValue<int>() ?? -1;
						if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("a monster needs a name");
						if (ProjectMonsters.BaseRecord(_workspace, baseId) == null) throw new ArgumentException("no monster " + baseId + " in the game's tables to start from");
						ModMonster m = ProjectMonsters.New(_project, name, baseId);
						SendJson(context, new { ok = true, monster = ProjectMonsters.Describe(_workspace, m) });
					}
					catch (Exception ex) { SendJson(context, new { ok = false, error = ex.Message }); }
					return;
				}

				case "/api/project/monsters/delete":
				{
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					JsonNode body = ReadBody(context);
					SendJson(context, new { ok = ProjectMonsters.Delete(_project, body?["id"]?.GetValue<string>()) });
					return;
				}

				case "/api/project/formations":
				{
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					List<string> notes = new List<string>();
					List<ModFormation> all = ProjectMonsters.AllFormations(_project, notes);
					string one = Query(context, "id");
					if (!string.IsNullOrEmpty(one))
					{
						ModFormation f = all.FirstOrDefault(x => string.Equals(x.Id, one, StringComparison.OrdinalIgnoreCase));
						if (f == null) { SendJson(context, new { ok = false, error = "no formation '" + one + "'" }); return; }
						SendJson(context, new { ok = true, formation = ProjectMonsters.DescribeFormation(_workspace, _project, f) });
						return;
					}
					SendJson(context, new { ok = true, formations = all.Select(f => ProjectMonsters.DescribeFormation(_workspace, _project, f)).ToList(), notes });
					return;
				}

				case "/api/project/formations/save":
				{
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					JsonNode body = ReadBody(context);
					try
					{
						ModFormation f = ModFormation.Parse(body?.ToJsonString() ?? "{}");
						if (f == null) throw new ArgumentException("no formation");
						if (f.Id != null && f.Id.IndexOfAny(new[] { '/', '\\', '.' }) >= 0) throw new ArgumentException("a formation's id is a plain word");
						ProjectMonsters.SaveFormation(_project, f);
						SendJson(context, new { ok = true, formation = ProjectMonsters.DescribeFormation(_workspace, _project, f) });
					}
					catch (Exception ex) { SendJson(context, new { ok = false, error = ex.Message }); }
					return;
				}

				case "/api/project/formations/new":
				{
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					JsonNode body = ReadBody(context);
					try
					{
						string name = body?["name"]?.GetValue<string>();
						if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("a formation needs a name");
						List<int> monsters = body?["monsters"] is JsonArray a ? a.Select(n => n?.GetValue<int>() ?? -1).Where(n => n >= 0).ToList() : new List<int>();
						ModFormation f = ProjectMonsters.NewFormation(_project, name, monsters);
						SendJson(context, new { ok = true, formation = ProjectMonsters.DescribeFormation(_workspace, _project, f) });
					}
					catch (Exception ex) { SendJson(context, new { ok = false, error = ex.Message }); }
					return;
				}

				case "/api/project/formations/delete":
				{
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					JsonNode body = ReadBody(context);
					SendJson(context, new { ok = ProjectMonsters.DeleteFormation(_project, body?["id"]?.GetValue<string>()) });
					return;
				}

				case "/api/map/encounters":
					// A map's random encounters: the five groups of four monster parties in its .pak
					// (chain 2, CMapMonsterPartyParameter), the tiles' land forms pointing at a group.
					SendJson(context, MapEncounters.Read(_workspace, Query(context, "name")));
					return;

				case "/api/map/encounters/save":
				{
					JsonNode body = ReadBody(context);
					try
					{
						string map = body?["name"]?.GetValue<string>();
						int[][] groups = body?["groups"] is JsonArray a ? a.Select(g => (g as JsonArray)?.Select(n => n?.GetValue<int>() ?? 0).ToArray() ?? new int[4]).ToArray() : null;
						SendJson(context, MapEncounters.Write(_workspace, map, groups));
					}
					catch (Exception ex) { SendJson(context, new { ok = false, error = ex.Message }); }
					return;
				}

				case "/api/file/duplicate":
				{
					// New content from existing: a package copied under a name of the mod's own (a new
					// NPC model from n021, a monster's skin from the one it wears). The copy is the
					// project's file; the game loads it by its name like any other.
					JsonNode body = ReadBody(context);
					try
					{
						string name = body?["name"]?.GetValue<string>();
						string asName = body?["as"]?.GetValue<string>();
						bool overwrite = body?["overwrite"]?.GetValue<bool>() ?? false;
						if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(asName)) throw new ArgumentException("a file and a new name");
						if (asName.IndexOfAny(new[] { ':', '*', '?', '"', '<', '>', '|' }) >= 0 || asName.Contains("..")) throw new ArgumentException("not a content name: " + asName);
						if (!overwrite && _workspace.Exists(asName)) throw new ArgumentException(asName + " exists already");
						byte[] data = _workspace.Read(name);
						_workspace.Write(asName, data);
						_characterIds.Invalidate();
						SendJson(context, new { ok = true, name = asName, bytes = data.Length });
					}
					catch (Exception ex) { SendJson(context, new { ok = false, error = ex.Message }); }
					return;
				}

				case "/api/project/monsters/skin":
				{
					// A skin of the monster's own: the package it wears now (its look's, or the family's),
					// copied to the name the battle asks for - f<family>_<number>.ntxp.lz - ready for
					// Replace with a PNG in Textures. The alias is no longer needed once the file exists.
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					JsonNode body = ReadBody(context);
					try
					{
						string id = body?["id"]?.GetValue<string>();
						ModMonster m = ProjectMonsters.All(_project).FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
						if (m == null) throw new ArgumentException("no monster definition '" + id + "'");
						byte[] record = ProjectMonsters.BaseRecord(_workspace, m.Base);
						if (record == null) throw new ArgumentException("no monster " + m.Base + " in the game's tables");
						int family = ChainPack.S16(record, 4);
						int wear = m.Look > 0 ? m.Look : m.Base;
						string own = "files/f" + family.ToString("000") + "_" + m.Number.ToString("000") + ".ntxp.lz";
						string from = null;
						foreach (string candidate in new[] { "files/f" + family.ToString("000") + "_" + wear.ToString("000") + ".ntxp.lz", "files/f" + family.ToString("000") + ".ntxp.lz" })
							if (_workspace.Exists(candidate)) { from = candidate; break; }
						if (from == null) throw new ArgumentException("the family has no texture package to start from");
						if (!_workspace.Exists(own)) _workspace.Write(own, _workspace.Read(from));
						SendJson(context, new { ok = true, name = own, from });
					}
					catch (Exception ex) { SendJson(context, new { ok = false, error = ex.Message }); }
					return;
				}

				case "/api/typeface":
				{
					// A face of the mod's own, as bytes, for the preview's @font-face.
					string name = Query(context, "name") ?? "";
					if (!name.StartsWith("fonts/", StringComparison.OrdinalIgnoreCase) || !_workspace.Exists(name)) { Send(context, 404, "text/plain", Encoding.UTF8.GetBytes("no such font")); return; }
					Send(context, 200, name.EndsWith(".otf", StringComparison.OrdinalIgnoreCase) ? "font/otf" : "font/ttf", _workspace.Read(name));
					return;
				}

				case "/api/typeface/import":
				{
					// A TrueType or OpenType face in, as fonts/<file> of the project's: the client draws its
					// text from the first face it loads, and a mod's come first. { name, bytes: base64 }.
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					JsonNode body = ReadBody(context);
					try
					{
						string fileName = Path.GetFileName(body?["name"]?.GetValue<string>() ?? "");
						if (string.IsNullOrWhiteSpace(fileName) || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) throw new ArgumentException("a plain file name");
						if (!fileName.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase) && !fileName.EndsWith(".otf", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("a .ttf or .otf file");
						byte[] bytes = System.Convert.FromBase64String(body?["bytes"]?.GetValue<string>() ?? "");
						// TrueType begins 00 01 00 00 or "true"; OpenType with CFF outlines begins "OTTO"; a collection "ttcf".
						bool face = bytes.Length > 12 && ((bytes[0] == 0 && bytes[1] == 1 && bytes[2] == 0 && bytes[3] == 0)
							|| (bytes[0] == 'O' && bytes[1] == 'T' && bytes[2] == 'T' && bytes[3] == 'O') || (bytes[0] == 't' && bytes[1] == 'r' && bytes[2] == 'u' && bytes[3] == 'e'));
						if (!face) throw new ArgumentException("not a TrueType or OpenType face (a collection, .ttc, is not taken)");
						string entry = "fonts/" + fileName;
						_workspace.Write(entry, bytes);
						SendJson(context, new { ok = true, name = entry });
					}
					catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or FormatException)
					{
						SendJson(context, new { ok = false, error = ex.Message });
					}
					return;
				}

				case "/api/image/import":
				{
					// A picture in, as a file of the project's: a new one under a name of the mod's own,
					// or the game's replaced. The 2D pictures are PNGs under DS names (.NCGR, .NCBR).
					JsonNode body = ReadBody(context);
					try
					{
						string name = body?["name"]?.GetValue<string>();
						byte[] png = System.Convert.FromBase64String(body?["png"]?.GetValue<string>() ?? "");
						if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("a name for the picture");
						if (png.Length < 8 || png[0] != 0x89 || png[1] != 'P' || png[2] != 'N' || png[3] != 'G') throw new ArgumentException("not a PNG");
						if (name.IndexOfAny(new[] { ':', '*', '?', '"', '<', '>', '|' }) >= 0 || name.Contains("..")) throw new ArgumentException("not a content name: " + name);
						bool existed = _workspace.Exists(name);
						_workspace.Write(name, png);
						SendJson(context, new { ok = true, name, bytes = png.Length, replaced = existed });
					}
					catch (Exception ex) { SendJson(context, new { ok = false, error = ex.Message }); }
					return;
				}

				case "/api/texture/new":
				{
					// A texture package of the mod's own from pictures: name, and textures [{ name,
					// format (pal256|pal16|pal4|a3i5|a5i3|rgb555|4x4), transparent0, width, height, rgba }].
					JsonNode body = ReadBody(context);
					try
					{
						string name = body?["name"]?.GetValue<string>();
						if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("a name for the package");
						if (name.IndexOfAny(new[] { ':', '*', '?', '"', '<', '>', '|' }) >= 0 || name.Contains("..")) throw new ArgumentException("not a content name: " + name);
						if (!(body?["overwrite"]?.GetValue<bool>() ?? false) && _workspace.Exists(name)) throw new ArgumentException(name + " exists already");
						string[] formats = { "none", "a3i5", "pal4", "pal16", "pal256", "4x4", "a5i3", "rgb555" };
						List<Tex0Write.NewTexture> textures = new List<Tex0Write.NewTexture>();
						foreach (JsonNode t in body?["textures"] as JsonArray ?? new JsonArray())
						{
							string format = t?["format"]?.GetValue<string>() ?? "pal256";
							int code = Array.IndexOf(formats, format);
							if (code < 1) throw new ArgumentException("no format '" + format + "'");
							textures.Add(new Tex0Write.NewTexture
							{
								Name = t?["name"]?.GetValue<string>() ?? "texture",
								Format = code,
								Transparent0 = t?["transparent0"]?.GetValue<bool>() ?? false,
								Width = t?["width"]?.GetValue<int>() ?? 0,
								Height = t?["height"]?.GetValue<int>() ?? 0,
								Rgba = System.Convert.FromBase64String(t?["rgba"]?.GetValue<string>() ?? "")
							});
						}
						int bytes = Textures.Create(_workspace, name, textures);
						SendJson(context, new { ok = true, name, bytes, textures = textures.Count });
					}
					catch (Exception ex) { SendJson(context, new { ok = false, error = ex.Message }); }
					return;
				}

				case "/api/texture/replace":
				{
					// A texture written back from a picture: the browser decodes the PNG and sends
					// the pixels at the texture's size.
					JsonNode body = ReadBody(context);
					try
					{
						string name = body?["name"]?.GetValue<string>();
						int index = body?["index"]?.GetValue<int>() ?? 0;
						int width = body?["width"]?.GetValue<int>() ?? 0, height = body?["height"]?.GetValue<int>() ?? 0;
						byte[] rgba = System.Convert.FromBase64String(body?["rgba"]?.GetValue<string>() ?? "");
						int bytes = Textures.Replace(_workspace, name, index, rgba, width, height);
						// On an OpenFF project the picture at its own size as well (textures/<texture>.png): the
						// client draws that in the texture's place; the package copy above is what the Steam game
						// (and the viewer, for now) reads. Sent as the PNG's bytes when it is larger than the slot.
						string fullSize = null; int fullWidth = 0, fullHeight = 0;
						string png = body?["png"]?.GetValue<string>();
						if (!string.IsNullOrEmpty(png) && _project != null && _project.File.Active != null && Targets.IsOurs(_project.File.Active))
						{
							string textureName = body?["texture"]?.GetValue<string>();
							if (string.IsNullOrWhiteSpace(textureName)) throw new ArgumentException("no texture name for the full-size picture");
							if (textureName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) throw new ArgumentException("the texture's name is not a file name: " + textureName);
							string folder = Path.Combine(_project.Directory, "textures");
							Directory.CreateDirectory(folder);
							fullSize = Path.Combine(folder, textureName + ".png");
							File.WriteAllBytes(fullSize, System.Convert.FromBase64String(png));
							fullWidth = body?["pngWidth"]?.GetValue<int>() ?? 0; fullHeight = body?["pngHeight"]?.GetValue<int>() ?? 0;
							fullSize = "textures/" + textureName + ".png";
						}
						SendJson(context, new { ok = true, bytes, overridden = true, fullSize, fullWidth, fullHeight });
					}
					catch (Exception ex) { SendJson(context, new { ok = false, error = ex.Message }); }
					return;
				}

				case "/api/record":
				{
					// One of the game's own records as a form: an item, a monster, a monster party.
					int id = int.TryParse(Query(context, "id"), out int n) ? n : -1;
					SendJson(context, GameRecords.Describe(_workspace, Query(context, "kind"), id, _lookupMessage));
					return;
				}

				case "/api/record/save":
				{
					JsonNode body = ReadBody(context);
					try
					{
						string kind = body?["kind"]?.GetValue<string>();
						int id = body?["id"]?.GetValue<int>() ?? -1;
						object result = GameRecords.Save(_workspace, kind, id, body?["fields"] as JsonObject, body?["slots"] as JsonArray, _lookupMessage);
						SendJson(context, result);
					}
					catch (Exception ex) { SendJson(context, new { ok = false, error = ex.Message }); }
					return;
				}

				case "/api/monsters":
					// The game's monsters then the mod's, for the pickers (a formation's slots, a base to start from).
					SendJson(context, ProjectMonsters.Monsters(_workspace, _project));
					return;

				case "/api/formations":
					// The game's monster parties then the mod's formations, for an Encounter's [FormationField].
					SendJson(context, ProjectMonsters.Formations(_workspace, _project));
					return;

				case "/api/project/characters":
				{
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					List<string> notes = new List<string>();
					List<ModCharacter> all = ProjectCharacters.All(_project, notes);
					string one = Query(context, "id");
					if (!string.IsNullOrEmpty(one))
					{
						ModCharacter c = all.FirstOrDefault(x => string.Equals(x.Id, one, StringComparison.OrdinalIgnoreCase));
						if (c == null) { SendJson(context, new { ok = false, error = "no character definition '" + one + "'" }); return; }
						SendJson(context, new { ok = true, character = ProjectCharacters.Describe(c), jobs = ProjectCharacters.Jobs(), heroes = ProjectCharacters.Heroes });
						return;
					}
					SendJson(context, new { ok = true, characters = all.Select(ProjectCharacters.Describe).ToList(), jobs = ProjectCharacters.Jobs(), heroes = ProjectCharacters.Heroes, notes });
					return;
				}

				case "/api/project/text":
				{
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					List<string> notes = new List<string>();
					Dictionary<uint, string> lines = ProjectText.Lines(_project, notes);
					SendJson(context, new
					{
						ok = true,
						files = ProjectText.Files(_project),
						lines = lines.OrderBy(p => p.Key).Select(p => new { id = p.Key, text = p.Value }).ToList(),
						next = ProjectText.NextId(_project),
						notes
					});
					return;
				}

				case "/api/scene/cast-check":
				{
					// A CastScript's lines compiled as the client compiles them (Ffs.CastCode, the same
					// frame), so a slip shows in the editor with its line rather than in the log at Play.
					JsonNode body = ReadBody(context);
					int cast = body?["cast"]?.GetValue<int>() ?? 0;
					if (cast <= 0) cast = 5000;
					string[] lines = body?["lines"] is JsonArray array ? array.Select(n => n?.GetValue<string>() ?? "").ToArray() : Array.Empty<string>();
					byte[] bytes = Ffs.CastCode.Compile(new Dictionary<int, string[]> { [cast] = Ffs.CastCode.Substitute(cast, lines) }, _workspace.Ops, out List<Ffs.CastCodeProblem> problems);
					SendJson(context, new
					{
						ok = bytes != null,
						bytes = bytes?.Length ?? 0,
						problems = problems.Select(p => new { line = p.Line, column = p.Column, message = p.Message }).ToList()
					});
					return;
				}

				case "/api/project/text/new":
				{
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					JsonNode body = ReadBody(context);
					try
					{
						string made = ProjectText.New(_project, body?["name"]?.GetValue<string>());
						SendJson(context, new { ok = true, path = made, name = Path.GetFileNameWithoutExtension(made) });
					}
					catch (Exception ex) { SendJson(context, new { ok = false, error = ex.Message }); }
					return;
				}

				case "/api/project/characters/save":
				{
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					JsonNode body = ReadBody(context);
					try
					{
						ModCharacter c = ModCharacter.Parse(body?.ToJsonString() ?? "{}");
						if (c == null) throw new ArgumentException("no character");
						if (c.Id != null && c.Id.IndexOfAny(new[] { '/', '\\', '.' }) >= 0) throw new ArgumentException("a character's id is a plain word");
						ProjectCharacters.Save(_project, c);
						SendJson(context, new { ok = true, character = ProjectCharacters.Describe(c) });
					}
					catch (Exception ex) { SendJson(context, new { ok = false, error = ex.Message }); }
					return;
				}

				case "/api/project/characters/new":
				{
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					JsonNode body = ReadBody(context);
					try
					{
						string name = body?["name"]?.GetValue<string>();
						int slot = body?["slot"]?.GetValue<int>() ?? 0;
						if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("a character needs a name");
						ModCharacter c = ProjectCharacters.New(_project, name, slot);
						SendJson(context, new { ok = true, character = ProjectCharacters.Describe(c) });
					}
					catch (Exception ex) { SendJson(context, new { ok = false, error = ex.Message }); }
					return;
				}

				case "/api/project/characters/delete":
				{
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					JsonNode body = ReadBody(context);
					SendJson(context, new { ok = ProjectCharacters.Delete(_project, body?["id"]?.GetValue<string>()) });
					return;
				}

				case "/api/project/jobs":
				{
					// The job ladders (defs/jobs/<id>.json) of the mastery progression: all, or ?id= one, with
					// the ability catalogue and the jobs for the form's pickers.
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					List<string> notes = new List<string>();
					List<ModJob> all = ProjectJobs.All(_project, notes);
					string one = Query(context, "id");
					if (!string.IsNullOrEmpty(one))
					{
						ModJob j = all.FirstOrDefault(x => string.Equals(x.Id, one, StringComparison.OrdinalIgnoreCase));
						if (j == null) { SendJson(context, new { ok = false, error = "no job ladder '" + one + "'" }); return; }
						SendJson(context, new { ok = true, ladder = ProjectJobs.Describe(j, all), jobs = ProjectCharacters.Jobs(), abilities = ProjectJobs.Catalogue(all), gameCommands = ProjectJobs.GameCommands(), notes });
						return;
					}
					SendJson(context, new { ok = true, ladders = all.Select(j => ProjectJobs.Describe(j, all)).ToList(), jobs = ProjectCharacters.Jobs(), abilities = ProjectJobs.Catalogue(all), gameCommands = ProjectJobs.GameCommands(), notes });
					return;
				}

				case "/api/project/jobs/save":
				{
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					JsonNode body = ReadBody(context);
					try
					{
						ModJob j = ModJob.Parse(body?.ToJsonString() ?? "{}");
						if (j == null) throw new ArgumentException("no ladder");
						if (j.Id != null && j.Id.IndexOfAny(new[] { '/', '\\', '.' }) >= 0) throw new ArgumentException("a ladder's id is a plain word");
						ProjectJobs.Save(_project, j);
						List<ModJob> all = ProjectJobs.All(_project);
						ModJob saved = all.FirstOrDefault(x => string.Equals(x.Id, j.Id, StringComparison.OrdinalIgnoreCase)) ?? j;
						SendJson(context, new { ok = true, ladder = ProjectJobs.Describe(saved, all), abilities = ProjectJobs.Catalogue(all) });
					}
					catch (Exception ex) { SendJson(context, new { ok = false, error = ex.Message }); }
					return;
				}

				case "/api/project/jobs/new":
				{
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					JsonNode body = ReadBody(context);
					try
					{
						// One of FF3's jobs by number, or a job of the mod's own: a name and the FF3 base it stands on (and whose figures it wears).
						ModJob j = body?["base"] != null
							? ProjectJobs.NewOwn(_project, body["name"]?.GetValue<string>(), body["base"]?.GetValue<int>() ?? -1, body["look"]?.GetValue<int>() ?? -1)
							: ProjectJobs.New(_project, body?["job"]?.GetValue<int>() ?? -1);
						List<ModJob> all = ProjectJobs.All(_project);
						SendJson(context, new { ok = true, ladder = ProjectJobs.Describe(all.FirstOrDefault(x => x.Id == j.Id) ?? j, all) });
					}
					catch (Exception ex) { SendJson(context, new { ok = false, error = ex.Message }); }
					return;
				}

				case "/api/project/jobs/delete":
				{
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					JsonNode body = ReadBody(context);
					SendJson(context, new { ok = ProjectJobs.Delete(_project, body?["id"]?.GetValue<string>()) });
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
					if (Current.Installable && _project != null)
					{
						// The mod's items into the game's tables first: a Steam game reads files, not
						// definitions, so the composed pak and msd go in with the other edits.
						try
						{
							foreach (string written in ProjectItems.WriteTables(_project, _workspace, Current.Target)) Console.Error.WriteLine("items: " + written + " composed for the install");
							foreach (string written in ProjectMonsters.WriteTables(_project, _workspace, Current.Target)) Console.Error.WriteLine("monsters: " + written + " composed for the install");
						}
						catch (Exception ex) { Console.Error.WriteLine("items: tables not composed: " + ex.Message); }
					}
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

				case "/api/project/assets":
					// The project's own model files (assets/*.glb, *.gltf), for the model picker.
					SendJson(context, GltfBundle.List(_project));
					return;

				case "/api/project/assets/import":
				{
					// A model file in: bytes as base64 under assets/<name>; a .gltf's .bin and pictures come as more files.
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					JsonNode body = ReadBody(context);
					try
					{
						List<string> written = new List<string>();
						foreach (JsonNode f in body?["files"] as JsonArray ?? new JsonArray())
						{
							string fileName = Path.GetFileName(f?["name"]?.GetValue<string>() ?? "");
							if (string.IsNullOrWhiteSpace(fileName) || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) throw new ArgumentException("a plain file name");
							byte[] bytes = System.Convert.FromBase64String(f?["bytes"]?.GetValue<string>() ?? "");
							string folder = Path.Combine(_project.Directory, GltfBundle.Folder);
							Directory.CreateDirectory(folder);
							File.WriteAllBytes(Path.Combine(folder, fileName), bytes);
							written.Add(GltfBundle.Folder + "/" + fileName);
						}
						SendJson(context, new { ok = true, files = written, models = GltfBundle.List(_project) });
					}
					catch (Exception ex) { SendJson(context, new { ok = false, error = ex.Message }); }
					return;
				}

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

				case "/api/model/glb":
				{
					// The model as a .glb download (with the motion named, or every motion of the
					// pack with all=1), for the browser's Save As - no project needed: a model of the
					// game's, straight to Blender.
					try
					{
						string name = Query(context, "name");
						if (string.IsNullOrEmpty(name)) throw new ArgumentException("no model named");
						(byte[] glb, string stem) = Models.ExportBytes(_workspace, name, Query(context, "pack"),
							int.Parse(Query(context, "index") ?? "0", CultureInfo.InvariantCulture), Query(context, "all") == "1");
						context.Response.Headers["Content-Disposition"] = "attachment; filename=\"" + stem + ".glb\"";
						context.Response.Headers["Cache-Control"] = "no-store";
						Send(context, 200, "model/gltf-binary", glb);
					}
					catch (Exception ex)
					{
						SendJson(context, new { ok = false, error = ex.Message });
					}
					return;
				}

				case "/api/model/pose":
					SendJson(context, Models.ReadPose(_workspace, Query(context, "name"),
						Query(context, "pack"),
						int.Parse(Query(context, "index") ?? "0", CultureInfo.InvariantCulture)));
					return;

				case "/api/model/rig-pose":
				{
					// A game model's node matrices over one motion - frames x nodes x 12 floats, model
					// space - for the viewer to skin a glTF with (a remade or auto-rigged character).
					try
					{
						Models.Rig rig = Models.ReadRig(_workspace, Query(context, "name"), Query(context, "pack"),
							int.Parse(Query(context, "index") ?? "0", CultureInfo.InvariantCulture), false);
						Models.RigMotion motion = rig.Motions.Count > 0 ? rig.Motions[0] : null;
						List<float> worlds = new List<float>();
						if (motion != null) foreach (float[][] frame in motion.Worlds) foreach (float[] m in frame) worlds.AddRange(m ?? new float[12]);
						SendJson(context, new { nodes = rig.Nodes, parents = rig.Parents, frames = motion?.Frames ?? 0, name = motion?.Name, worlds, bind = rig.Bind.SelectMany(b => b).ToList() });
					}
					catch (Exception ex) { SendJson(context, new { ok = false, error = ex.Message }); }
					return;
				}

				case "/api/model/clips":
				{
					// A glTF's own animation clips, by name, with their length in the game's frames.
					try
					{
						string asset = Query(context, "name") ?? throw new ArgumentException("no file named");
						string filePath = GltfBundle.Resolve(_project, asset) ?? throw new ArgumentException("no file " + asset + " in the project");
						OpenFF.Graphics.GltfFile file = OpenFF.Graphics.GltfFile.Load(filePath);
						SendJson(context, new { ok = true, clips = file.Animations.Select(a => new { name = a.Name, frames = Math.Max(1, (int)Math.Ceiling(a.Duration * 30) + 1), seconds = a.Duration }).ToList() });
					}
					catch (Exception ex) { SendJson(context, new { ok = false, error = ex.Message }); }
					return;
				}

				case "/api/project/models/clips":
				{
					// The clips a model definition plays in the game's motions' place: { model: files/j101.nmdp.lz
					// or j101, clips: { idle: "Idle", walk: { clip, sync, speed }, ... } } into defs/models/<stem>.json
					// (the rest of the definition kept); an empty map takes them out. GET ?model= reads them.
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					try
					{
						JsonNode body = context.Request.HttpMethod == "POST" ? ReadBody(context) : null;
						string modelName = body?["model"]?.GetValue<string>() ?? Query(context, "model") ?? throw new ArgumentException("no model named");
						string stem = Path.GetFileName(modelName); if (stem.IndexOf('.') > 0) stem = stem.Substring(0, stem.IndexOf('.'));
						string definition = Path.Combine(_project.Directory, OpenFF.Data.ModModels.Folder.Replace('/', Path.DirectorySeparatorChar), stem + ".json");
						OpenFF.Data.ModModel model = File.Exists(definition) ? OpenFF.Data.ModModel.Parse(File.ReadAllText(definition), definition) : null;
						if (body?["clips"] is JsonObject clips)
						{
							if (model == null) throw new ArgumentException("no definition for " + stem + " yet - Remake the model from the glTF first");
							model.Clips.Clear();
							foreach (KeyValuePair<string, JsonNode> pair in clips)
							{
								OpenFF.Data.ModClip clip = pair.Value is JsonObject o
									? new OpenFF.Data.ModClip { Clip = o["clip"]?.GetValue<string>(), Sync = o["sync"]?.GetValue<bool>() ?? true, Speed = (float)(o["speed"]?.GetValue<double>() ?? 1) }
									: new OpenFF.Data.ModClip { Clip = pair.Value?.GetValue<string>() };
								if (!string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(clip.Clip)) model.Clips[pair.Key] = clip;
							}
							File.WriteAllText(definition, model.ToJson());
						}
						SendJson(context, new { ok = true, model = stem, gltf = model?.Gltf, fitted = model?.Fitted ?? false, clips = model?.Clips.ToDictionary(p => p.Key, p => new { clip = p.Value.Clip, sync = p.Value.Sync, speed = p.Value.Speed }) });
					}
					catch (Exception ex) { SendJson(context, new { ok = false, error = ex.Message }); }
					return;
				}

				case "/api/model/clip-pose":
				{
					// One of a glTF's own animation clips, sampled at the game's 30 frames a second into the
					// shape rig-pose has - nodes, parents, frames x nodes x 12 floats, bind - so the viewer
					// plays it on the file's own skeleton as it plays a game motion. ?name=assets/x.glb&clip=Walk
					try
					{
						string asset = Query(context, "name") ?? throw new ArgumentException("no file named");
						string clipName = Query(context, "clip") ?? throw new ArgumentException("no clip named");
						string filePath = GltfBundle.Resolve(_project, asset) ?? throw new ArgumentException("no file " + asset + " in the project");
						OpenFF.Graphics.GltfFile file = OpenFF.Graphics.GltfFile.Load(filePath);
						OpenFF.Graphics.GltfAnimation clip = file.Animations.Find(a => string.Equals(a.Name, clipName, StringComparison.OrdinalIgnoreCase)) ?? throw new ArgumentException("no clip " + clipName + " in " + asset);
						int frames = Math.Max(1, (int)Math.Ceiling(clip.Duration * 30) + 1);
						List<float> worlds = new List<float>(frames * file.Nodes.Count * 12);
						for (int f = 0; f < frames; f++)
						{
							float[][] w = file.WorldMatrices(clip, Math.Min(clip.Duration, f / 30f));
							foreach (float[] m in w) worlds.AddRange(new[] { m[0], m[1], m[2], m[4], m[5], m[6], m[8], m[9], m[10], m[12], m[13], m[14] });
						}
						// The bind: the nodes' worlds with no clip applied (the file's rest).
						float[][] rest = file.WorldMatrices(null, 0);
						List<float> bind = new List<float>(file.Nodes.Count * 12);
						foreach (float[] m in rest) bind.AddRange(new[] { m[0], m[1], m[2], m[4], m[5], m[6], m[8], m[9], m[10], m[12], m[13], m[14] });
						SendJson(context, new { nodes = file.Nodes.Select((n, i) => n.Name ?? ("node" + i)).ToList(), parents = file.Nodes.Select(n => n.Parent).ToList(), frames, name = clip.Name, worlds, bind });
					}
					catch (Exception ex) { SendJson(context, new { ok = false, error = ex.Message }); }
					return;
				}

				case "/api/model/joint":
					// A joint's matrix per frame (R_te for the right hand...), for a weapon posed on a character in the viewer.
					try
					{
						SendJson(context, Models.ReadJoint(_workspace, Query(context, "name"), Query(context, "pack"),
							int.Parse(Query(context, "index") ?? "0", CultureInfo.InvariantCulture), Query(context, "node") ?? "R_te"));
					}
					catch (Exception ex) { SendJson(context, new { error = ex.Message }); }
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

				case "/api/audio/free":
					// The first BGM number the game ships no tune for and the project has not taken.
				{
					List<AudioAsset> all = Audio.List(_workspace.ContentDirectory, _workspace);
					SendJson(context, new { ok = true, bgm = Audio.FreeBgm(_workspace, all), first = Audio.FirstFreeBgm, last = Audio.LastFreeBgm, se = Audio.FreeSe(_workspace, all), firstSe = Audio.FirstFreeSe, lastSe = Audio.LastFreeSe });
					return;
				}

				case "/api/audio/import":
				{
					// A sound of the mod's own: { name: BGM30, part: 0|1, bytes: base64 (Ogg Vorbis or WAV), loopMs? }.
					if (_project == null) { SendJson(context, new { ok = false, error = "no project is open" }); return; }
					JsonNode body = ReadBody(context);
					try
					{
						string written = Audio.Import(_workspace, body?["name"]?.GetValue<string>(), body?["part"]?.GetValue<int>() ?? 1,
							System.Convert.FromBase64String(body?["bytes"]?.GetValue<string>() ?? ""), body?["loopMs"]?.GetValue<int>());
						SendJson(context, new { ok = true, file = written });
					}
					catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or FormatException or InvalidOperationException)
					{
						SendJson(context, new { ok = false, error = ex.Message });
					}
					return;
				}

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
				// Not twice: a Steam project's files/ may hold the composed pak already (WriteTables).
				HashSet<int> have = new HashSet<int>(items.Select(i => (int)i.GetType().GetProperty("id").GetValue(i)));
				foreach (ModItem mine in ProjectItems.All(_project))
				{
					if (have.Contains(mine.Number)) continue;
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
				// A model of the project's own (assets/hut.glb): the shared glTF reader, laid out the same;
				// and which game model it stands in for, when a definition (defs/models) names it.
				if (GltfBundle.IsAsset(name))
				{
					ModelBundle asset = GltfBundle.Read(_project, name);
					if (_project != null && asset.Problem == null)
					{
						OpenFF.Data.ModModel definition = OpenFF.Data.ModModels.Load(new[] { _project.Directory }).FirstOrDefault(d => string.Equals(d.Gltf?.Replace('\\', '/'), name.Replace('\\', '/'), StringComparison.OrdinalIgnoreCase));
						if (definition != null) { asset.StandsInFor = definition.Model; asset.Definition = "defs/models/" + definition.Model + ".json"; asset.DefinitionFitted = definition.Fitted; }
						// An auto-rigged file: whether its record (beside the original) says fitted - what a Look on a character needs to know.
						string riggedPath = GltfBundle.Resolve(_project, name);
						if (riggedPath != null && System.Text.RegularExpressions.Regex.IsMatch(riggedPath, @"-rigged\.glb$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
						{
							JsonObject record = LoadCutsFile(System.Text.RegularExpressions.Regex.Replace(riggedPath, @"-rigged\.glb$", ".glb", System.Text.RegularExpressions.RegexOptions.IgnoreCase)) as JsonObject;
							asset.RigFitted = record?["rigged"]?["fitted"] is JsonValue rf && rf.TryGetValue(out bool rfv) && rfv;
						}
					}
					SendJson(context, asset);
					return;
				}
				ModelBundle bundle = Models.Read(_workspace, name);
				// A game model the project's definitions replace with a glTF in the OpenFF client (defs/models).
				if (_project != null && name.EndsWith(".nmdp.lz", StringComparison.OrdinalIgnoreCase))
				{
					string stem = Path.GetFileName(name); stem = stem.Substring(0, stem.IndexOf('.'));
					OpenFF.Data.ModModel definition = OpenFF.Data.ModModels.Load(new[] { _project.Directory }).FirstOrDefault(d => string.Equals(d.Model, stem, StringComparison.OrdinalIgnoreCase));
					if (definition != null) bundle.ReplacedBy = definition.Gltf?.Replace('\\', '/');
				}
				SendJson(context, bundle);
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
			bool all = body?["all"] != null && (bool)body["all"];
			if (string.IsNullOrEmpty(name))
			{
				SendJson(context, new { ok = false, error = "no model named" });
				return;
			}
			if (_project == null)
			{
				// Without a project there is no exports folder to speak of; the browser's Save As
				// (/api/model/glb) is the way then.
				SendJson(context, new { ok = false, error = "no project is open - use the Save As export" });
				return;
			}
			try
			{
				string path = Models.Export(_workspace, name, pack, index, Path.Combine(_project.Directory, "exports"), all);
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
				if (GltfBundle.IsAsset(name))
				{
					(byte[] bytes, string mime) picture = GltfBundle.Texture(_project, name, texture);
					Send(context, 200, picture.mime, picture.bytes);
					return;
				}
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
				SendJson(context, Textures.Contents(_workspace, name, _project));
			}
			catch (Exception ex)
			{
				SendJson(context, new { error = ex.Message });
			}
		}

		/// <summary>One texture, decoded on the way out; ?full=1 for the project's full-size PNG of it (textures/&lt;name&gt;.png), the one the OpenFF client draws.</summary>
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
				if (Query(context, "full") == "1")
				{
					string stem = Path.GetFileName(name ?? "");
					stem = stem.Substring(0, stem.IndexOf('.') < 0 ? stem.Length : stem.IndexOf('.'));
					List<TextureInfo> list = Textures.Contents(_workspace, name);
					string textureName = index >= 0 && index < list.Count ? list[index].Name : null;
					string full = Textures.FullSizePath(_project, stem, textureName) ?? throw new FileNotFoundException("no full-size picture for " + textureName);
					Send(context, 200, "image/png", File.ReadAllBytes(full));
					return;
				}
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
				// The Steam targets' tables composed from the definitions before the zip is packed.
				foreach (KeyValuePair<string, Session> pair in _sessions)
				{
					if (!pair.Value.Installable || !_project.File.Targets.Contains(pair.Key, StringComparer.OrdinalIgnoreCase)) continue;
					try { ProjectItems.WriteTables(_project, pair.Value.Workspace, pair.Key); ProjectMonsters.WriteTables(_project, pair.Value.Workspace, pair.Key); }
					catch (Exception ex) { Console.Error.WriteLine("items: tables not composed for " + pair.Key + ": " + ex.Message); }
				}
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
					maps = ProjectScenes.Maps(_project).Select(m => new { map = m.Map, attachments = m.Attachments, points = m.Points, bytes = m.Bytes, modified = m.Modified, own = m.Own, title = m.Title })
				});
				return;
			}
			try
			{
				ProjectScenes.Summary summary = ProjectScenes.Maps(_project).FirstOrDefault(m => string.Equals(m.Map, map, StringComparison.OrdinalIgnoreCase));
				SendJson(context, new { ok = true, map, own = summary?.Own ?? false, title = summary?.Title, attachments = ProjectScenes.Read(_project, map), objects = ProjectScenes.Objects(_project, map) });
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
					// "Play the cutscene": the Cutscene on this object plays the moment the map is up.
					string cutscene = body?["cutscene"]?.GetValue<string>();
					if (!string.IsNullOrWhiteSpace(cutscene)) arguments.Add("--cutscene=" + cutscene.Trim());
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
			// The mod's own lines (defs/text) answer where the game's files have nothing: the
			// client composes them into eureka_permanent.msd, so an "@40000001" says them.
			Dictionary<uint, string> own = _project != null ? ProjectText.Lines(_project) : null;

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
					if (text == null && own != null && own.TryGetValue((uint)value, out string line)) text = line;
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
				// The text library also lists the mod's own faces (fonts/*.ttf), which the client draws its text from.
				case "text": return new[] { ".msd", ".ttf", ".otf" };
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

		/// <summary>Three numbers from a JSON array (a rotation, an offset), or null when the body has none.</summary>
		/// <summary>The auto-rig's cuts from JSON ({ neck, hips, armFloor, torsoWidth: fractions or null; skirt, enabled: bools }), null for none.</summary>
		private static AutoRig.Cuts ReadCuts(JsonNode node)
		{
			if (node is not JsonObject o) return null;
			float? Fraction(string key) => o[key] is JsonValue v && v.TryGetValue(out double d) ? (float)Math.Clamp(d, 0, 1) : (float?)null;
			return new AutoRig.Cuts
			{
				Neck = Fraction("neck"), Hips = Fraction("hips"), ArmFloor = Fraction("armFloor"), TorsoWidth = Fraction("torsoWidth"),
				Skirt = o["skirt"] is JsonValue s && s.TryGetValue(out bool skirt) ? skirt : true,
				Enabled = o["enabled"] is JsonValue e && e.TryGetValue(out bool enabled) ? enabled : true
			};
		}

		private static object CutsJson(AutoRig.Cuts c) => c == null ? null : new { neck = c.Neck, hips = c.Hips, armFloor = c.ArmFloor, torsoWidth = c.TorsoWidth, skirt = c.Skirt, enabled = c.Enabled };

		/// <summary>The auto-rig's markers from JSON ({ chin: [x, y, z], groin, leftWrist, rightWrist, leftElbow, rightElbow, leftKnee, rightKnee }), null for none.</summary>
		private static AutoRig.Markers ReadMarkers(JsonNode node)
		{
			if (node is not JsonObject o) return null;
			AutoRig.Markers m = new AutoRig.Markers
			{
				Chin = Triple(o["chin"]), Groin = Triple(o["groin"]), LeftWrist = Triple(o["leftWrist"]), RightWrist = Triple(o["rightWrist"]),
				LeftElbow = Triple(o["leftElbow"]), RightElbow = Triple(o["rightElbow"]), LeftKnee = Triple(o["leftKnee"]), RightKnee = Triple(o["rightKnee"])
			};
			return m.Any ? m : null;
		}

		/// <summary>The cuts saved beside a glTF (assets/<name>.rig.json), or null.</summary>
		private static JsonNode LoadCutsFile(string gltfPath)
		{
			string path = Path.ChangeExtension(gltfPath, ".rig.json");
			try { return File.Exists(path) ? JsonNode.Parse(File.ReadAllText(path)) : null; } catch { return null; }
		}

		private static void SaveCutsFile(string gltfPath, JsonNode cuts)
		{
			File.WriteAllText(Path.ChangeExtension(gltfPath, ".rig.json"), cuts.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
		}

		private static float[] Triple(JsonNode node)
		{
			if (node is not JsonArray array || array.Count < 3) return null;
			float[] result = new float[3];
			for (int i = 0; i < 3; i++) result[i] = array[i] is JsonValue v && v.TryGetValue(out double d) ? (float)d : 0f;
			return result;
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
			string root = _webRoot;
			// The guide: the HTML tutorials, from the Guide folder beside the executable (a
			// release) or the repository's Docs\Guide, at /guide/.
			if (relative.StartsWith("guide/", StringComparison.OrdinalIgnoreCase) || relative.Equals("guide", StringComparison.OrdinalIgnoreCase))
			{
				root = GuideFolder();
				relative = relative.Length > 6 ? relative.Substring(6) : "index.html";
				if (relative.Length == 0 || relative.EndsWith("/", StringComparison.Ordinal)) relative += "index.html";
				if (root == null)
				{
					Send(context, 404, "text/html; charset=utf-8", Encoding.UTF8.GetBytes("<p>The guide was not found beside Crystal (a Guide folder, or the repository's Docs\\Guide).</p>"));
					return;
				}
			}
			string full = Path.GetFullPath(Path.Combine(root, relative));

			if (!full.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.Ordinal)
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

		/// <summary>Where the guide's pages are: Guide\ beside the executable (a release), else the repository's Docs\Guide up from it; null for neither.</summary>
		private static string GuideFolder()
		{
			string at = AppContext.BaseDirectory;
			for (int i = 0; i < 6 && !string.IsNullOrEmpty(at); i++)
			{
				foreach (string name in new[] { "Guide", Path.Combine("Docs", "Guide") })
				{
					string candidate = Path.Combine(at, name);
					if (File.Exists(Path.Combine(candidate, "index.html"))) return Path.GetFullPath(candidate).TrimEnd(Path.DirectorySeparatorChar);
				}
				at = Path.GetDirectoryName(at.TrimEnd(Path.DirectorySeparatorChar));
			}
			return null;
		}

		private static string ContentType(string path)
		{
			switch (Path.GetExtension(path).ToLowerInvariant())
			{
				case ".html": return "text/html; charset=utf-8";
				case ".js": return "text/javascript; charset=utf-8";
				case ".css": return "text/css; charset=utf-8";
				case ".svg": return "image/svg+xml";
				case ".png": return "image/png";
				case ".jpg": case ".jpeg": return "image/jpeg";
				case ".gif": return "image/gif";
				case ".webp": return "image/webp";
				case ".ico": return "image/x-icon";
				case ".woff2": return "font/woff2";
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
