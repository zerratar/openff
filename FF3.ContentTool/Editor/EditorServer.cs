// The editor's back end: a small local HTTP server over the codecs.
//
//   ff3content editor [--content=<dir>] [--override=<dir>] [--language=en] [--port=5050]
//
// It binds to localhost only, has no authentication, and is meant to be run by the
// person editing their own copy of the game. It is not a service.
//
// The browser does the editing; this side only decodes, compiles and saves. Which
// means the rules that matter - what compiles, what a menu's coordinates mean - stay
// in one place and are the same ones the command line uses.

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

namespace FF3.ContentTool.Editor
{
	internal sealed class EditorServer
	{
		private static readonly JsonSerializerOptions Json = new JsonSerializerOptions
		{
			WriteIndented = false,
			// The browser side reads camelCase, so the wire format says camelCase.
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		// Not readonly, because opening a project replaces the lot. Everything here
		// is derived from one workspace - the message index, the flag and reference
		// indexes, the fonts - so switching which content is open means rebuilding all
		// of them together. Doing that in place beats making the person restart the
		// editor and find their way back to what they were looking at.
		private Workspace _workspace;
		private readonly string _webRoot;
		private MessageIndex _messages;
		private CharacterIds _characterIds;
		private FlagIndex _flags;
		private References _references;
		private Fonts _fonts;
		private Func<uint, string> _lookupMessage;
		private string _language;
		private Project _project;

		public EditorServer(Workspace workspace, string webRoot, MessageIndex messages,
			string language = "en", Project project = null)
		{
			_webRoot = webRoot;
			_language = language ?? "en";
			_project = project;
			Adopt(workspace, messages);
		}

		/// <summary>Takes a workspace and builds everything that hangs off it.</summary>
		private void Adopt(Workspace workspace, MessageIndex messages)
		{
			_workspace = workspace;
			_messages = messages ?? new MessageIndex(workspace, _language);
			// A field, not a method group: the indexes capture it, so it has to keep
			// pointing at whichever message index is current.
			_lookupMessage = id => _messages.Text(id);
			_characterIds = new CharacterIds(workspace);
			_flags = new FlagIndex(workspace, _lookupMessage);
			_references = new References(workspace, _lookupMessage);
			_fonts = new Fonts(workspace.ContentDirectory);
		}

		/// <summary>
		/// Opens a project: its active target says which game's content to read, and its
		/// files directory becomes the override. Nothing is written by this - it only
		/// changes what is being looked at.
		/// </summary>
		public void OpenProject(Project project)
		{
			Workspace workspace = new Workspace(project.ContentDirectory(), project.Files);
			_project = project;
			Adopt(workspace, null);
		}

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

			Console.WriteLine("FF3 content editor");
			Console.WriteLine("  content   {0} files, {1}, in {2}",
				_workspace.FileCount, _workspace.Kind, _workspace.ContentDirectory);
			Console.WriteLine("  overrides {0}", _workspace.OverrideDirectory);
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

			switch (path)
			{
				case "/api/status":
					SendJson(context, new
					{
						files = _workspace.FileCount,
						overrides = _workspace.OverrideDirectory,
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
							description = _project.File.Description
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

				case "/api/targets":
					SendJson(context, Targets.All.Select(name => new
					{
						name,
						label = Targets.Describe(name),
						content = Targets.Find(name)
					}).ToList());
					return;

				case "/api/mod/status":
					SendJson(context, ModInstall.Status(_workspace));
					return;

				case "/api/mod/install":
					SendJson(context, ModInstall.Install(_workspace));
					return;

				case "/api/mod/uninstall":
					SendJson(context, ModInstall.Uninstall(_workspace));
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
				SendJson(context, new { ok = true, active = target,
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
			_project.Save();
			SendJson(context, new { ok = true });
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
			ModResult result = ModInstall.Revert(_workspace, names);
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
			ModResult result = ModInstall.Revert(_workspace, new[] { name });
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
			context.Response.OutputStream.Write(body, 0, body.Length);
			context.Response.OutputStream.Close();
		}
	}
}
