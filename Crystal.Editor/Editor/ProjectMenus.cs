// A project's menu screens of its own: menus/<id>.json (the screen: how it opens, the
// MenuBehaviours on its frames) beside menus/<id>.xml (the layout, in the game's own XML
// form - what the Menus tab draws and edits). OpenFF.Engine/Menus.cs says what the client
// makes of them. The editor lists them among the game's menus, edits the layout on the
// same canvas, and the definition in the inspector.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace Crystal.Editor
{
	internal static class ProjectMenus
	{
		public static string Directory(Project project) => Path.Combine(project.Directory, "menus");

		/// <summary>The Crystal Style Sheets of a folder of screens (styles/*.css), by name, as the client cascades them (MenuStyles.SheetsBeside).</summary>
		public static List<object> Sheets(string folder)
		{
			List<object> sheets = new List<object>();
			string styles = folder == null ? null : Path.Combine(folder, "styles");
			if (styles == null || !System.IO.Directory.Exists(styles)) return sheets;
			foreach (string file in System.IO.Directory.EnumerateFiles(styles, "*.css").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
			{
				sheets.Add(new { name = Path.GetFileName(file), css = File.ReadAllText(file) });
			}
			return sheets;
		}

		private static readonly string[] PictureExtensions = { ".png", ".jpg", ".jpeg", ".bmp" };

		/// <summary>The pictures of a folder of screens (images/, and any folder under it), for their backgrounds: url("images/...").</summary>
		public static List<object> Pictures(string folder)
		{
			List<object> pictures = new List<object>();
			string images = folder == null ? null : Path.Combine(folder, "images");
			if (images == null || !System.IO.Directory.Exists(images)) return pictures;
			foreach (string file in System.IO.Directory.EnumerateFiles(images, "*.*", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
			{
				if (!PictureExtensions.Contains(Path.GetExtension(file).ToLowerInvariant())) continue;
				int width = 0, height = 0;
				try { using System.Drawing.Image image = System.Drawing.Image.FromFile(file); width = image.Width; height = image.Height; } catch (Exception) { }
				pictures.Add(new { path = Path.GetRelativePath(folder, file).Replace(Path.DirectorySeparatorChar, '/'), width, height, bytes = new FileInfo(file).Length });
			}
			return pictures;
		}

		/// <summary>A file of the folder by its path in it (images/x.png), or null - never one outside it.</summary>
		public static string FileIn(string folder, string path)
		{
			if (folder == null || string.IsNullOrWhiteSpace(path)) return null;
			string root = Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
			string full = Path.GetFullPath(Path.Combine(folder, path.Replace('/', Path.DirectorySeparatorChar)));
			return full.StartsWith(root, StringComparison.OrdinalIgnoreCase) ? full : null;
		}

		/// <summary>A picture into images/ (a plain file name); returns its path in the folder.</summary>
		public static string SavePicture(string folder, string name, byte[] data)
		{
			string file = Path.GetFileName(name ?? "");
			string stem = Path.GetFileNameWithoutExtension(file), ext = Path.GetExtension(file).ToLowerInvariant();
			if (string.IsNullOrEmpty(stem) || !PictureExtensions.Contains(ext) || !System.Text.RegularExpressions.Regex.IsMatch(stem, "^[A-Za-z0-9_. -]+$")) throw new InvalidOperationException("a picture's name is letters, digits, space, . - and _, ending .png, .jpg or .bmp (" + name + ")");
			if (data == null || data.Length < 8) throw new InvalidOperationException("that is not a picture");
			string images = Path.Combine(folder, "images");
			System.IO.Directory.CreateDirectory(images);
			File.WriteAllBytes(Path.Combine(images, stem + ext), data);
			return "images/" + stem + ext;
		}

		/// <summary>
		/// One of the OpenFF client's own screens copied into the project: its definition and layout, and what they draw
		/// with - the client's stylesheets, pictures and sprites.json - where the project has none of the same name (the
		/// project's own are kept). The mod's screen then takes the client's place (a mod's screen of the same id wins), so
		/// the player's changes are theirs and an update of the client does not take them away. Returns what was copied
		/// and what was left because the project already had it.
		/// </summary>
		public static (List<string> Copied, List<string> Kept) AdoptClient(Project project, string clientFolder, string id)
		{
			if (Definition(clientFolder, id) is not JsonObject def) throw new InvalidOperationException("the client has no screen called '" + id + "'");
			string into = Directory(project);
			if (File.Exists(Path.Combine(into, id + ".json"))) throw new InvalidOperationException("the project already has menus/" + id + ".json - open that one");
			System.IO.Directory.CreateDirectory(into);
			List<string> copied = new List<string>(), kept = new List<string>();
			void Copy(string relative)
			{
				string from = Path.Combine(clientFolder, relative), to = Path.Combine(into, relative);
				if (!File.Exists(from)) return;
				if (File.Exists(to)) { kept.Add(relative.Replace('\\', '/')); return; }
				System.IO.Directory.CreateDirectory(Path.GetDirectoryName(to));
				File.Copy(from, to);
				copied.Add(relative.Replace('\\', '/'));
			}
			Copy(id + ".json");
			string layout = def["layout"]?.GetValue<string>();
			if (!string.IsNullOrWhiteSpace(layout)) Copy(layout);
			foreach (string folder in new[] { "styles", "images" })
			{
				string from = Path.Combine(clientFolder, folder);
				if (!System.IO.Directory.Exists(from)) continue;
				foreach (string file in System.IO.Directory.EnumerateFiles(from, "*.*", SearchOption.AllDirectories)) Copy(Path.GetRelativePath(clientFolder, file));
			}
			// The client's sprites merged under the project's: a sheet the project has sprites for keeps its own.
			string clientSprites = Path.Combine(clientFolder, OpenFF.Content.MenuSprites.FileName), ownSprites = Path.Combine(into, OpenFF.Content.MenuSprites.FileName);
			if (File.Exists(clientSprites))
			{
				JsonObject theirs = JsonNode.Parse(File.ReadAllText(clientSprites)) as JsonObject;
				JsonObject mine = File.Exists(ownSprites) ? JsonNode.Parse(File.ReadAllText(ownSprites)) as JsonObject : null;
				mine ??= new JsonObject();
				if (mine["sheets"] is not JsonObject sheets) { sheets = new JsonObject(); mine["sheets"] = sheets; }
				bool any = false;
				if (theirs?["sheets"] is JsonObject clientSheets)
				{
					foreach (KeyValuePair<string, JsonNode> sheet in clientSheets)
					{
						if (sheets.ContainsKey(sheet.Key)) { kept.Add("sprites of " + sheet.Key); continue; }
						sheets[sheet.Key] = sheet.Value?.DeepClone();
						any = true;
					}
				}
				if (any)
				{
					File.WriteAllText(ownSprites, mine.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
					copied.Add(OpenFF.Content.MenuSprites.FileName);
				}
			}
			return (copied, kept);
		}

		/// <summary>
		/// A setting of the mod's for one of the game's screens (its backdrop's lines): kept in the definition of the
		/// project's that reaches that screen - menus/&lt;id&gt;.json with "screen" and "file" - made for it when there is
		/// none (no layout: it only reaches the screen). Null takes the setting away. Read and written as the file is,
		/// so a definition of the project's own keeps everything else it says.
		/// </summary>
		public static string GameScreenSetting(Project project, string file, string screen, string key)
		{
			string path = ReachingPath(project, file, screen);
			if (path == null || !File.Exists(path)) return null;
			JsonNode value = (JsonNode.Parse(File.ReadAllText(path)) as JsonObject)?[key];
			return value == null ? null : value.GetValueKind() == System.Text.Json.JsonValueKind.String ? value.GetValue<string>() : value.ToJsonString();
		}

		public static string SetGameScreenSetting(Project project, string file, string screen, string key, string value)
		{
			string dir = Directory(project);
			string path = ReachingPath(project, file, screen);
			JsonObject def;
			if (path != null && File.Exists(path)) def = JsonNode.Parse(File.ReadAllText(path)) as JsonObject ?? new JsonObject();
			else
			{
				if (value == null) return null;
				string id = System.Text.RegularExpressions.Regex.Replace(screen ?? "screen", "[^A-Za-z0-9_-]", "_");
				path = Path.Combine(dir, id + ".json");
				def = new JsonObject { ["id"] = id, ["screen"] = screen, ["file"] = file ?? "MenuDefine.xbn", ["title"] = screen + " (the mod's settings for the game's screen)" };
			}
			// A number as a number ("background": 3), as the client's definitions read it.
			if (value == null) def.Remove(key);
			else if (int.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int number)) def[key] = number;
			else def[key] = value;
			System.IO.Directory.CreateDirectory(dir);
			File.WriteAllText(path, def.ToJsonString(new JsonSerializerOptions { WriteIndented = true }), new UTF8Encoding(false));
			return Path.GetFileName(path);
		}

		/// <summary>The project's definition that reaches one of the game's screens (its "screen", in the file said or MenuDefine.xbn).</summary>
		private static string ReachingPath(Project project, string file, string screen)
		{
			string dir = Directory(project);
			if (!System.IO.Directory.Exists(dir) || string.IsNullOrWhiteSpace(screen)) return null;
			string wanted = Path.GetFileName(file ?? "MenuDefine.xbn");
			foreach (string path in System.IO.Directory.EnumerateFiles(dir, "*.json"))
			{
				if (string.Equals(Path.GetFileName(path), OpenFF.Content.MenuSprites.FileName, StringComparison.OrdinalIgnoreCase)) continue;
				try
				{
					if (JsonNode.Parse(File.ReadAllText(path)) is not JsonObject def) continue;
					string s = def["screen"]?.GetValue<string>() ?? Path.GetFileNameWithoutExtension(path);
					string f = def["file"]?.GetValue<string>() ?? "MenuDefine.xbn";
					if (string.Equals(s, screen, StringComparison.OrdinalIgnoreCase) && string.Equals(Path.GetFileName(f), wanted, StringComparison.OrdinalIgnoreCase)) return path;
				}
				catch (Exception) { }
			}
			return null;
		}

		/// <summary>Writes styles/&lt;name&gt;.css (a plain name of letters, digits, - and _); an empty sheet is removed.</summary>
		public static void SaveSheet(string folder, string name, string css)
		{
			string stem = Path.GetFileNameWithoutExtension(name ?? "");
			if (string.IsNullOrEmpty(stem) || !System.Text.RegularExpressions.Regex.IsMatch(stem, "^[A-Za-z0-9_-]+$")) throw new InvalidOperationException("a stylesheet's name is letters, digits, - and _ (" + name + ")");
			string styles = Path.Combine(folder, "styles");
			string path = Path.Combine(styles, stem + ".css");
			if (string.IsNullOrWhiteSpace(css))
			{
				if (File.Exists(path)) File.Delete(path);
				return;
			}
			System.IO.Directory.CreateDirectory(styles);
			File.WriteAllText(path, css);
		}

		// The project's screens are its menus/ folder's; every call below also takes a folder of screens of its own -
		// the OpenFF client's Data/menus (OpenFFClient.MenusFolder), which the Menus tab lists beside the project's.

		public static List<string> Ids(Project project) => Ids(project == null ? null : Directory(project));
		public static JsonObject Definition(Project project, string id) => project == null ? null : Definition(Directory(project), id);
		public static string Layout(Project project, string id) => project == null ? null : Layout(Directory(project), id);
		public static List<object> Describe(Project project) => Describe(project == null ? null : Directory(project));
		public static void SaveDefinition(Project project, string id, JsonObject def) => SaveDefinition(Directory(project), id, def);
		public static void SaveLayout(Project project, string id, string xml) => SaveLayout(Directory(project), id, xml);

		/// <summary>The ids of a folder's screens: every *.json in it.</summary>
		public static List<string> Ids(string dir)
		{
			if (dir == null || !System.IO.Directory.Exists(dir)) return new List<string>();
			return System.IO.Directory.EnumerateFiles(dir, "*.json").Select(Path.GetFileNameWithoutExtension)
				.Where(n => !string.Equals(n + ".json", OpenFF.Content.MenuSprites.FileName, StringComparison.OrdinalIgnoreCase))   // the sheets' sprites, not a screen
				.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
		}

		/// <summary>A screen's definition as JSON (&lt;id&gt;.json), with defaults filled in; null for none.</summary>
		public static JsonObject Definition(string dir, string id)
		{
			string path = DefinitionPath(dir, id);
			if (path == null || !File.Exists(path)) return null;
			JsonObject def;
			try { def = JsonNode.Parse(File.ReadAllText(path), null, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true }) as JsonObject; }
			catch (Exception) { def = null; }
			def ??= new JsonObject();
			def["id"] ??= id;
			def["layout"] ??= id + ".xml";
			def["screen"] ??= id;
			def["background"] ??= 10;
			def["characterSelect"] ??= false;
			def["attachments"] ??= new JsonArray();
			def["file"] ??= "MenuDefine.xbn";   // the game's layout file the screen lives in (a screen of the game's own is reached by name)
			def["patch"] ??= false;
			def["definitionFile"] = "menus/" + id + ".json";
			def["layoutFile"] = "menus/" + def["layout"].GetValue<string>();
			def["layoutExists"] = File.Exists(Path.Combine(dir, def["layout"].GetValue<string>()));
			return def;
		}

		/// <summary>A screen's layout XML, or null.</summary>
		public static string Layout(string dir, string id)
		{
			JsonObject def = Definition(dir, id);
			if (def == null) return null;
			string path = Path.Combine(dir, def["layout"].GetValue<string>());
			return File.Exists(path) ? File.ReadAllText(path) : null;
		}

		/// <summary>The list for the library: id, title, the main menu entry, counts.</summary>
		public static List<object> Describe(string dir)
		{
			List<object> list = new List<object>();
			foreach (string id in Ids(dir))
			{
				JsonObject def = Definition(dir, id);
				if (def == null) continue;
				int frames = 0, focusable = 0;
				try
				{
					string xml = Layout(dir, id);
					if (xml != null)
					{
						XDocument doc = XDocument.Parse(xml);
						frames = doc.Descendants("frame").Count();
						focusable = doc.Descendants("frame").Count(f => f.Element("focus") != null);
					}
				}
				catch (Exception) { }
				list.Add(new
				{
					id, title = def["title"]?.GetValue<string>(), screen = def["screen"]?.GetValue<string>(),
					mainMenu = def["mainMenu"] is JsonObject mm ? mm["label"]?.GetValue<string>() : null,
					characterSelect = def["characterSelect"]?.GetValue<bool>() ?? false,
					attachments = (def["attachments"] as JsonArray)?.Count ?? 0,
					gameFile = def["file"]?.GetValue<string>(), patch = def["patch"]?.GetValue<bool>() ?? false,
					frames, focusable, file = "menus/" + id + ".json", layoutFile = def["layoutFile"]?.GetValue<string>()
				});
			}
			return list;
		}

		private static string DefinitionPath(string dir, string id)
		{
			if (dir == null || string.IsNullOrWhiteSpace(id) || id.IndexOfAny(new[] { '/', '\\', '.' }) >= 0) return null;
			return Path.Combine(dir, id + ".json");
		}

		/// <summary>Writes the definition (its editor-only fields dropped).</summary>
		public static void SaveDefinition(string dir, string id, JsonObject def)
		{
			string path = DefinitionPath(dir, id) ?? throw new ArgumentException("a screen's id is a plain word");
			System.IO.Directory.CreateDirectory(dir);
			JsonObject clean = (JsonObject)JsonNode.Parse(def.ToJsonString());
			clean.Remove("definitionFile"); clean.Remove("layoutFile"); clean.Remove("layoutExists");
			clean["id"] = id;
			if (clean["mainMenu"] is JsonObject mm && string.IsNullOrWhiteSpace(mm["label"]?.GetValue<string>())) clean.Remove("mainMenu");
			File.WriteAllText(path, clean.ToJsonString(new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }), new UTF8Encoding(false));
		}

		/// <summary>Writes the layout, checked as XML.</summary>
		public static void SaveLayout(string dir, string id, string xml)
		{
			JsonObject def = Definition(dir, id) ?? throw new ArgumentException("no screen called '" + id + "'");
			XDocument doc = XDocument.Parse(xml);
			string path = Path.Combine(dir, def["layout"].GetValue<string>());
			File.WriteAllText(path, doc.Declaration != null ? doc.Declaration + "\n" + doc.ToString() : doc.ToString(), new UTF8Encoding(false));
		}

		/// <summary>A new screen: a layout with a title, three rows and a Back frame, and a definition with a main menu entry when asked.</summary>
		public static string New(Project project, string name, bool mainMenu)
		{
			if (project == null) throw new InvalidOperationException("no project is open");
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("a screen needs a name");
			string id = ProjectItems.Slug(name);
			if (string.IsNullOrEmpty(id)) id = "screen";
			System.IO.Directory.CreateDirectory(Directory(project));
			string unique = id;
			for (int n = 2; File.Exists(Path.Combine(Directory(project), unique + ".json")) || File.Exists(Path.Combine(Directory(project), unique + ".xml")); n++) unique = id + "-" + n;
			XElement Frame(string fid, int x, int y, int w, int h, bool focus, string text, string up = null, string down = null)
			{
				XElement f = new XElement("frame");
				if (focus) f.Add(new XElement("focus"));
				f.Add(new XElement("id", fid), new XElement("x", x), new XElement("y", y), new XElement("width", w), new XElement("height", h));
				if (focus) f.Add(new XElement("up", up ?? "dummy"), new XElement("down", down ?? "dummy"), new XElement("left", "dummy"), new XElement("right", "dummy"), new XElement("align", "menu"));   // the hand cursor stands clear of the word
				f.Add(new XElement("behavior", new XAttribute("value", "Text"), new XElement("parameter", -1), new XElement("parameter", 8), new XElement("parameter", 0)));
				f.Add(new XElement("data", text));
				return f;
			}
			XElement Window(string wid, int x, int y, int w, int h, params XElement[] children)
			{
				XElement f = new XElement("frame", new XElement("window"), new XElement("id", wid), new XElement("x", x), new XElement("y", y), new XElement("width", w), new XElement("height", h));
				f.Add(children);
				return f;
			}
			// The canvas is the game's 480 x 288 (the bottom bar sits under); windows keep 4 px from the edges, rows
			// are 24 px apart with the text centred in them, as the game's own screens have it; a window's children
			// sit relative to it; the frames the cursor lands on stay at the top level so up/down/left/right reach each other.
			XDocument layout = new XDocument(new XDeclaration("1.0", "utf-8", null),
				new XComment(" A menu screen of the mod's own, in the game's own layout form: frames with a position and size, <focus/> where the cursor may land, up/down/left/right the ids it moves to, a Text behaviour showing <data>; a frame with <window/> is drawn with the game's window art and its nested frames sit relative to it. Crystal's Menus tab draws and edits it; a MenuBehaviour (menus/" + unique + ".json) writes the real texts and acts on presses. "),
				new XElement("menulist", new XElement("menu", new XElement("name", unique),
					Window("w_top", 4, 4, 472, 36, Frame("title", 12, 4, 448, 28, false, name.Trim())),
					Window("w_body", 4, 44, 472, 196),
					Window("w_desc", 4, 244, 472, 40, Frame("desc", 12, 2, 448, 18, false, "A: choose   B: back")),
					Frame("row1", 50, 56, 400, 24, true, "First row", "back", "row2"),
					Frame("row2", 50, 80, 400, 24, true, "Second row", "row1", "row3"),
					Frame("row3", 50, 104, 400, 24, true, "Third row", "row2", "back"),
					Frame("back", 50, 152, 400, 24, true, "Back", "row3", "row1"))));
			File.WriteAllText(Path.Combine(Directory(project), unique + ".xml"), layout.Declaration + "\n" + layout.ToString(), new UTF8Encoding(false));
			JsonObject def = new JsonObject
			{
				["id"] = unique, ["layout"] = unique + ".xml", ["screen"] = unique, ["title"] = name.Trim(),
				["background"] = 10, ["characterSelect"] = false,
				["attachments"] = new JsonArray(new JsonObject { ["target"] = "back", ["behaviour"] = "Back" })
			};
			if (mainMenu) def["mainMenu"] = new JsonObject { ["label"] = name.Trim(), ["after"] = "com_job" };
			SaveDefinition(project, unique, def);
			return unique;
		}

		/// <summary>
		/// One of the game's screens taken into the mod: its &lt;menu&gt; copied from the layout file (as the workspace has it,
		/// overrides and all) to menus/&lt;screen&gt;.xml, with a definition naming the screen and file; the client then plays
		/// the mod's copy in the game's place. Returns the id.
		/// </summary>
		public static string Adopt(Project project, string file, string screen, byte[] xbn)
		{
			if (project == null) throw new InvalidOperationException("no project is open");
			if (string.IsNullOrWhiteSpace(screen)) throw new ArgumentException("which screen?");
			XDocument doc = OpenFF.Content.MenuXbn.ToXml(xbn);
			if (file != null) OpenFF.Content.BattleHudLayout.Ensure(Path.GetFileName(file), doc);   // the client's battle_hud / field_hud screens
			XElement menu = doc.Root?.Elements().FirstOrDefault(m => (m.Name.LocalName == "menu" || m.Name.LocalName == "unit") && (string)m.Element("name") == screen)
				?? throw new ArgumentException("no screen called '" + screen + "' in " + file);
			string id = ProjectItems.Slug(screen);
			System.IO.Directory.CreateDirectory(Directory(project));
			if (File.Exists(Path.Combine(Directory(project), id + ".json"))) return id;   // taken already: open that
			XDocument layout = new XDocument(new XDeclaration("1.0", "utf-8", null),
				new XComment(" The game's " + screen + " (" + file + ") as the mod's own: edit it here and the client plays this copy in the game's place. The game's screen code still drives it, so keep the ids it reads; frames may move, grow, gain <window/>, <font>, <align>, <colour>, and behaviours in menus/" + id + ".json. "),
				new XElement("menulist", new XElement(menu)));
			File.WriteAllText(Path.Combine(Directory(project), id + ".xml"), layout.Declaration + "\n" + layout.ToString(), new UTF8Encoding(false));
			SaveDefinition(project, id, new JsonObject
			{
				["id"] = id, ["layout"] = id + ".xml", ["screen"] = screen, ["file"] = Path.GetFileName(file), ["title"] = screen + " (the game's)",
				["patch"] = false, ["background"] = 10, ["characterSelect"] = false, ["attachments"] = new JsonArray()
			});
			return id;
		}

		public static bool Delete(Project project, string id)
		{
			string path = DefinitionPath(Directory(project), id);
			if (path == null || !File.Exists(path)) return false;
			JsonObject def = Definition(project, id);
			File.Delete(path);
			string layout = def != null ? Path.Combine(Directory(project), def["layout"].GetValue<string>()) : null;
			if (layout != null && File.Exists(layout)) File.Delete(layout);
			return true;
		}
	}
}
