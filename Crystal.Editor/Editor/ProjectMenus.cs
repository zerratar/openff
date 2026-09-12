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

		/// <summary>The ids of the project's screens: every menus/*.json.</summary>
		public static List<string> Ids(Project project)
		{
			string dir = project == null ? null : Directory(project);
			if (dir == null || !System.IO.Directory.Exists(dir)) return new List<string>();
			return System.IO.Directory.EnumerateFiles(dir, "*.json").Select(Path.GetFileNameWithoutExtension).OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
		}

		/// <summary>A screen's definition as JSON (menus/&lt;id&gt;.json), with defaults filled in; null for none.</summary>
		public static JsonObject Definition(Project project, string id)
		{
			string path = DefinitionPath(project, id);
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
			def["layoutExists"] = File.Exists(Path.Combine(Directory(project), def["layout"].GetValue<string>()));
			return def;
		}

		/// <summary>A screen's layout XML, or null.</summary>
		public static string Layout(Project project, string id)
		{
			JsonObject def = Definition(project, id);
			if (def == null) return null;
			string path = Path.Combine(Directory(project), def["layout"].GetValue<string>());
			return File.Exists(path) ? File.ReadAllText(path) : null;
		}

		/// <summary>The list for the library: id, title, the main menu entry, counts.</summary>
		public static List<object> Describe(Project project)
		{
			List<object> list = new List<object>();
			foreach (string id in Ids(project))
			{
				JsonObject def = Definition(project, id);
				if (def == null) continue;
				int frames = 0, focusable = 0;
				try
				{
					string xml = Layout(project, id);
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

		private static string DefinitionPath(Project project, string id)
		{
			if (project == null || string.IsNullOrWhiteSpace(id) || id.IndexOfAny(new[] { '/', '\\', '.' }) >= 0) return null;
			return Path.Combine(Directory(project), id + ".json");
		}

		/// <summary>Writes the definition (its editor-only fields dropped).</summary>
		public static void SaveDefinition(Project project, string id, JsonObject def)
		{
			string path = DefinitionPath(project, id) ?? throw new ArgumentException("a screen's id is a plain word");
			System.IO.Directory.CreateDirectory(Directory(project));
			JsonObject clean = (JsonObject)JsonNode.Parse(def.ToJsonString());
			clean.Remove("definitionFile"); clean.Remove("layoutFile"); clean.Remove("layoutExists");
			clean["id"] = id;
			if (clean["mainMenu"] is JsonObject mm && string.IsNullOrWhiteSpace(mm["label"]?.GetValue<string>())) clean.Remove("mainMenu");
			File.WriteAllText(path, clean.ToJsonString(new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }), new UTF8Encoding(false));
		}

		/// <summary>Writes the layout, checked as XML.</summary>
		public static void SaveLayout(Project project, string id, string xml)
		{
			JsonObject def = Definition(project, id) ?? throw new ArgumentException("no screen called '" + id + "'");
			XDocument doc = XDocument.Parse(xml);
			string path = Path.Combine(Directory(project), def["layout"].GetValue<string>());
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
			string path = DefinitionPath(project, id);
			if (path == null || !File.Exists(path)) return false;
			JsonObject def = Definition(project, id);
			File.Delete(path);
			string layout = def != null ? Path.Combine(Directory(project), def["layout"].GetValue<string>()) : null;
			if (layout != null && File.Exists(layout)) File.Delete(layout);
			return true;
		}
	}
}
