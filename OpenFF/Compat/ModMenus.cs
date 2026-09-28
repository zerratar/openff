// The mods' menu screens (OpenFF.Engine/Menus.cs) in the game's own menu system.
//
//   - The layouts: as MenuManager loads MenuDefine.xbn, the file is decoded to XML (Shared's
//     MenuXbn, the codec Crystal's Menus tab uses), every mod screen's <menu> is appended, an
//     entry for each screen that asks for one is put into main_menu's command list (the list
//     re-spaced to fit), and the whole is encoded back. buildMenu then finds a mod's screen by
//     name as it finds the game's.
//   - The screen: wmenu.CWMenuMod, one WMENU_KIND past the game's, plays whichever mod screen
//     is current; the main menu's entries carry work = KIND + index, which CWMenuMain hands
//     here (Select) before terminating into the kind.
//   - The behaviours: made from the definition's attachments as the screen opens (MenuLoader),
//     bound to a screen adapter over the Medget tree (widgets by id, texts through MBText,
//     focus through MenuManager), told of every event from CWMenuMod.
//   - Game.Menus.Open from the field: a request the world's move state reads as the menu
//     button, with the main menu told to shift straight to the mod screen.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using OpenFF.Content;
using OpenFF.Modding;

namespace OpenFF.Client
{
	internal sealed class ModMenus : GameService, IMenus
	{
		private static readonly List<MenuDefinition> _all = new List<MenuDefinition>();
		private static readonly Dictionary<string, LoadedMod> _mods = new Dictionary<string, LoadedMod>(StringComparer.OrdinalIgnoreCase);
		private static MenuDefinition _current;
		private static bool _fromField;
		private static ModMenuScreen _screen;
		private static List<MenuBehaviour> _behaviours = new List<MenuBehaviour>();
		private static GlobalScope.wmenu.CWMenuMod _host;
		private static string _requested;
		private static string _lastSummary;
		// One of the game's own screens the mods reach, while it is up.
		private static ModMenuScreen _gameScreen;
		private static List<MenuBehaviour> _gameBehaviours = new List<MenuBehaviour>();
		private static string _gameFocused;

		public IReadOnlyList<MenuDefinition> All => _all;
		public MenuDefinition Find(string id) => _all.FirstOrDefault(d => string.Equals(d.Id, id, StringComparison.OrdinalIgnoreCase));
		public IMenuScreen Current => _screen;

		/// <summary>Whether a loaded mod defines a screen of that id.</summary>
		public static bool HasScreen(string id) => _all.Any(d => string.Equals(d.Id, id, StringComparison.OrdinalIgnoreCase));

		/// <summary>The definitions of every loaded mod, read once the engine has its mods (EngineHost calls after the load).</summary>
		public static void Gather(IEnumerable<LoadedMod> mods)
		{
			_all.Clear();
			_mods.Clear();
			foreach (LoadedMod mod in mods)
			{
				string folder = mod.Definition?.Menus;
				if (string.IsNullOrEmpty(folder)) continue;
				foreach (MenuDefinition def in MenuLoader.Read(mod.Id, folder))
				{
					if (_all.Any(d => string.Equals(d.Id, def.Id, StringComparison.OrdinalIgnoreCase))) { Log.Write(LogChannel.General, "menus: " + mod.Id + "/" + def.Id + " - another mod defines a screen of that id; skipped"); continue; }
					_all.Add(def);
					_mods[def.Id] = mod;
				}
			}
			// The client's own screens (Data/menus beside the executable: the Gambits), after the mods' - a mod's screen of the
			// same id takes the place of the client's.
			if (!GameProfile.IsFf4)
			{
				foreach (MenuDefinition def in MenuLoader.Read("openff", Path.Combine(AppContext.BaseDirectory, "Data", "menus")))
				{
					if (_all.Any(d => string.Equals(d.Id, def.Id, StringComparison.OrdinalIgnoreCase))) continue;
					_all.Add(def);
				}
			}
			string summary = _all.Count == 0 ? null : "menus: " + _all.Count + " screen(s) of the mods' own: " + string.Join(", ", _all.Select(d => d.Id + (d.MainMenu != null ? " (main menu: " + d.MainMenu.Label + ")" : "")));
			if (summary != null && summary != _lastSummary) Log.Write(LogChannel.General, summary);
			_lastSummary = summary;
		}

		// ---- the layouts ----

		/// <summary>The screens of the game's own that mods reach (behaviours, a layout of the mod's, a patch), by screen name, filled as the files load.</summary>
		private static readonly Dictionary<string, List<MenuDefinition>> _gameScreenDefs = new Dictionary<string, List<MenuDefinition>>(StringComparer.OrdinalIgnoreCase);

		/// <summary>Each screen's layout as written - classes, bindings, the frames' own styles - and its stylesheets, by the definition's id: what the screen restyles and binds from as it runs (the game's copy has them baked and gone).</summary>
		private static readonly Dictionary<string, (XElement Menu, List<string> Sheets)> _styleSources = new Dictionary<string, (XElement, List<string>)>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// One of the game's layout files with the mods' work in it: their own screens appended (MenuDefine.xbn,
		/// with the main menu entries), and the game's screens they reach replaced or patched frame by id. A file
		/// no definition names, or with nothing to do, goes through as it was.
		/// </summary>
		public static Array Patch(string fileName, Array bytes)
		{
			string file = Path.GetFileName(fileName ?? "");
			_loadedFile = file;
			// Out of the menus' file, no backdrop of a mod's stays put in place of a screen's.
			if (!string.Equals(file, "MenuDefine.xbn", StringComparison.OrdinalIgnoreCase) && _gameBackdrop != null)
			{
				if (_gameBackdrop < 0) { try { GlobalScope.wmenu.CWMenuManager.Instance().SetPrimaryBGVisibility(true); } catch (Exception) { } }
				_gameBackdrop = null;
			}
			if (bytes == null) return bytes;
			// The definitions are read again each time: a menus/<id>.json edited while the client runs is on the next opening, as the layouts are.
			if (_screen == null && _gameScreen == null) Gather(OpenFF.Game.Mods);
			List<MenuDefinition> defs = _all.Where(d => string.Equals(d.File, file, StringComparison.OrdinalIgnoreCase)).ToList();
			bool hud = BattleHudLayout.HasVirtual(file);   // BattleDefine's battle_hud, WorldDefine's field_hud
			if (defs.Count == 0 && !hud) return bytes;
			try
			{
				XDocument doc = MenuXbn.ToXml((byte[])bytes);
				XElement list = doc.Root;
				if (list == null) return bytes;
				// The code-drawn windows as screens (BattleHudLayout): the game's numbers unless a mod's copy takes their place below.
				// On a Steam install, Steam's own battle geometry (its build draws the HUD from code of its own).
				if (hud) BattleHudLayout.Ensure(file, doc, GlobalScope.BATTLE_COMMAND_X(), GlobalScope.BATTLE_COMMAND_Y(), GlobalScope.BATTLE_PLAYER_Y(), SteamLayout.Active);
				int added = 0, reached = 0;
				foreach (string key in _gameScreenDefs.Keys.ToList()) if (_gameScreenDefs[key].Any(d => string.Equals(d.File, file, StringComparison.OrdinalIgnoreCase))) _gameScreenDefs.Remove(key);
				foreach (MenuDefinition def in defs)
				{
					XElement existing = list.Elements("menu").FirstOrDefault(m => (string)m.Element("name") == def.Screen);
					if (existing != null)
					{
						// One of the game's own: its behaviours hear the game's screen; a layout replaces or patches it.
						if (!_gameScreenDefs.TryGetValue(def.Screen, out List<MenuDefinition> onScreen)) _gameScreenDefs[def.Screen] = onScreen = new List<MenuDefinition>();
						onScreen.Add(def);
						reached++;
					}
					if (def.Layout == null) continue;
					XElement menu = LoadLayoutMenu(def, renumber: existing == null || !def.Patch);
					if (menu == null) continue;
					if (existing != null && def.Patch) MergeInto(existing, menu, def);
					else { existing?.Remove(); list.Add(menu); if (existing == null) added++; }
				}
				if (string.Equals(file, "MenuDefine.xbn", StringComparison.OrdinalIgnoreCase)) AddMainMenuEntries(list);
				if (Options.Get("dump-menus") != null) { string at = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(file) + ".patched.xml"); doc.Save(at); Log.Write(LogChannel.General, "menus: patched layout written to " + at); }
				byte[] patched = MenuXbn.FromXml(doc);
				// The field's HUD kept as built, with a mod's layout of it as written (its looks), past this file's time.
				if (string.Equals(file, BattleHudLayout.FieldFile, StringComparison.OrdinalIgnoreCase))
				{
					MenuDefinition hudDef = defs.LastOrDefault(d => d.Layout != null && string.Equals(d.Screen, BattleHudLayout.FieldScreen, StringComparison.OrdinalIgnoreCase));
					if (hudDef != null && _styleSources.TryGetValue(hudDef.Id, out (XElement Menu, List<string> Sheets) hudStyle)) FieldHud.Capture(patched, hudStyle.Menu, hudStyle.Sheets, hudDef.Directory);
					else FieldHud.Capture(patched, null, null, null);
				}
				int entries = string.Equals(file, "MenuDefine.xbn", StringComparison.OrdinalIgnoreCase) ? Entries().Count : 0;
				Log.Write(LogChannel.General, "menus: " + file + " carries " + added + " screen(s) of the mods' own" + (reached > 0 ? ", " + reached + " of the game's reached" : "") + (entries > 0 ? ", " + entries + " main menu entr" + (entries == 1 ? "y" : "ies") : ""));
				return patched;
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "menus: " + file + " not patched: " + ex.Message);
				return bytes;
			}
		}

		/// <summary>
		/// The field's HUD captured before WorldDefine.xbn first loads - the title's commands, the buttons of a menu opened from
		/// the title: the file read and patched as its load would, what the loaded file's patch leaves (the file, its backdrop) as it was.
		/// </summary>
		public static void PrepareFieldHud()
		{
			if (FieldHud.Captured) return;
			try
			{
				string file = BattleHudLayout.FieldFile;
				uint size = GlobalScope.ds.g_File.getSize(file);
				if (size == 0) return;
				Array array = GlobalScope.ds.CHeap.alloc_app(size);
				if (array == null) return;
				GlobalScope.ds.g_File.load(array, file);
				string loaded = _loadedFile;
				int? backdrop = _gameBackdrop;
				Patch(file, array);
				_loadedFile = loaded;
				_gameBackdrop = backdrop;
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "field hud: not read ahead: " + ex.Message); }
		}

		/// <summary>A mod's frames into one of the game's screens: a frame whose id the screen has takes that frame's place (keeping the game's myTag when the mod's says none); a new one is added at the top level, numbered after the screen's focus list.</summary>
		private static void MergeInto(XElement existing, XElement patch, MenuDefinition def)
		{
			int replaced = 0, appended = 0;
			foreach (XElement frame in patch.Elements("frame").ToList())
			{
				string id = (string)frame.Element("id");
				XElement old = id == null ? null : existing.Descendants("frame").FirstOrDefault(f => (string)f.Element("id") == id);
				if (old != null)
				{
					if (frame.Element("myTag") == null && old.Element("myTag") != null) frame.Add(new XElement("myTag", old.Element("myTag").Value));
					old.ReplaceWith(new XElement(frame));
					replaced++;
				}
				else
				{
					XElement added = new XElement(frame);
					if (added.Element("focus") != null) added.SetElementValue("myTag", existing.Descendants("frame").Count(f => f.Element("focus") != null));
					existing.Add(added);
					appended++;
				}
			}
			Log.Write(LogChannel.File, "menus: " + def.Id + " patches " + def.Screen + ": " + replaced + " frame(s) replaced, " + appended + " added");
		}
		/// <summary>The definition's layout file's &lt;menu&gt; whose name is the screen's (or the file's only one), renamed to the screen's name.</summary>
		private static XElement LoadLayoutMenu(MenuDefinition def, bool renumber = true)
		{
			try
			{
				XDocument layout = XDocument.Load(def.LayoutPath);
				IEnumerable<XElement> menus = layout.Root?.Name.LocalName == "menu" ? new[] { layout.Root } : layout.Root?.Elements("menu") ?? Enumerable.Empty<XElement>();
				XElement menu = menus.FirstOrDefault(m => (string)m.Element("name") == def.Screen) ?? menus.FirstOrDefault();
				if (menu == null) { Log.Write(LogChannel.General, "menus: " + def.Id + ": " + def.Layout + " has no <menu>"); return null; }
				menu = new XElement(menu);
				// Crystal Style Sheets: styles/*.css beside the layout and its own <style>s, cascaded onto the
				// frames (MenuStyles) - the layout rules into their style for MenuXbn.FromXml to bake, the look
				// (colour, opacity, panel, tint, hidden) into the elements the widgets and windows read here.
				List<string> sheets = MenuStyles.SheetsBeside(def.LayoutPath).Concat(layout.Root.Elements("style").Select(s => s.Value)).ToList();
				XElement written = new XElement(menu);
				foreach (XElement own in written.Descendants("style").ToList()) { sheets.Add(own.Value); own.Remove(); }
				_styleSources[def.Id] = (written, sheets);
				MenuStyles.Apply(menu, sheets);
				XElement name = menu.Element("name");
				if (name == null) menu.AddFirst(new XElement("name", def.Screen)); else name.Value = def.Screen;
				// The game moves focus by a frame's myTag, which must be its place in the focus list (the
				// <focus/> frames in document order); the layout need not know - it is numbered here.
				int tag = 0;
				foreach (XElement frame in menu.Descendants("frame"))
				{
					// An empty <data/> would be a null string to the game's text widget; a space is a blank it can draw.
					XElement data = frame.Element("data");
					if (data != null && string.IsNullOrEmpty(data.Value)) data.Value = " ";
					ApplyStyle(frame);
					if (frame.Element("focus") == null) continue;
					if (renumber) frame.SetElementValue("myTag", tag++);
					foreach (string side in new[] { "up", "down", "left", "right" }) if (frame.Element(side) == null) frame.Add(new XElement(side, "dummy"));
				}
				return menu;
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "menus: " + def.Id + ": " + def.Layout + ": " + ex.Message); return null; }
		}

		/// <summary>
		/// A frame's style words, written into the Text behaviour's parameters the game reads: <font>large|normal</font>
		/// (the second parameter, 16 or 8), <align>left|right|center|button|menu</align> (the third: 0, 1, 2, 4, 6 - button draws
		/// the game's button frame behind the text; menu is the game's own list alignment, text at the left with the hand cursor
		/// standing clear of it, the one to use on the rows of a list). <colour> is kept on the frame and put on as the screen opens.
		/// </summary>
		private static void ApplyStyle(XElement frame)
		{
			XElement behaviour = frame.Element("behavior");
			if (behaviour == null || (string)behaviour.Attribute("value") != "Text") return;
			string font = frame.Element("font")?.Value?.Trim().ToLowerInvariant();
			string align = frame.Element("align")?.Value?.Trim().ToLowerInvariant();
			if (font == null && align == null) return;
			List<XElement> parameters = behaviour.Elements("parameter").ToList();
			while (parameters.Count < 3) { XElement p = new XElement("parameter", parameters.Count == 0 ? "-1" : parameters.Count == 1 ? "8" : "0"); behaviour.Add(p); parameters.Add(p); }
			if (font != null) parameters[1].Value = font == "large" || font == "big" || (int.TryParse(font, out int n) && n > 12) ? "16" : "8";   // a number: the nearer of the game's two here; the exact size goes on as the screen opens
			if (align != null) parameters[2].Value = align == "right" ? "1" : align == "center" || align == "centre" ? "2" : align == "button" ? "4" : align == "menu" || align == "list" ? SteamLayout.STEAM_ALIGN_MENU.ToString() : "0";
		}

		/// <summary>A colour word from a layout's <colour>, as the game's colour; White when unknown.</summary>
		/// <summary>A palette word's colour (0xRRGGBBAA, the game's text palette); null for a word that is none - for the styles' colours to move between the game's words and #hex, and for shadows and borders in its words.</summary>
		private static uint? PaletteColour(string word)
		{
			string w = word?.Trim().ToLowerInvariant();
			if (string.IsNullOrEmpty(w) || !char.IsLetter(w[0])) return null;
			MenuColour c;
			if (w == "grey" || w == "gray" || w == "disabled") c = MenuColour.Disabled;
			else if (w == "blue2") c = MenuColour.PaleBlue;
			else if (!Enum.TryParse(w.Replace("-", "").Replace(" ", ""), true, out c)) return null;
			try { return GlobalScope.TextPaletteColour((int)c); } catch (Exception) { return null; }
		}

		static ModMenus()
		{
			MenuAnimation.Palette = PaletteColour;
		}

		public static MenuColour ColourWord(string word)
		{
			return Enum.TryParse(word?.Trim().Replace("-", "").Replace(" ", ""), true, out MenuColour c) ? c : word?.Trim().ToLowerInvariant() switch { "grey" or "gray" or "disabled" => MenuColour.Disabled, "pale-blue" or "paleblue" or "blue2" => MenuColour.PaleBlue, _ => MenuColour.White };
		}

		/// <summary>An entry per screen that asks for one in main_menu's command list, after the entry it names; the list re-spaced to fit, the focus ring closed.</summary>
		private static void AddMainMenuEntries(XElement list)
		{
			List<MenuDefinition> entries = Entries();
			if (entries.Count == 0) return;
			XElement main = list.Elements("menu").FirstOrDefault(m => (string)m.Element("name") == "main_menu");
			XElement commands = main?.Elements("frame").FirstOrDefault(f => (string)f.Element("id") == "main_command");
			if (commands == null) { Log.Write(LogChannel.General, "menus: main_menu has no main_command list; the mods' entries are not added"); return; }
			List<XElement> rows = commands.Elements("frame").ToList();
			if (rows.Count == 0) return;
			// The rows as the game laid them out, for the backdrop's row lines (fitted to the rows there are once the entries are in).
			int commandsX = Int(commands.Element("x"), 0), commandsY = Int(commands.Element("y"), 0);
			int oldCount = rows.Count, oldTop = rows.Min(r => Int(r.Element("y"), 2)), oldBottom = rows.Max(r => Int(r.Element("y"), 2) + Int(r.Element("height"), 28));
			int left = commandsX + rows.Min(r => Int(r.Element("x"), 0)), right = commandsX + rows.Max(r => Int(r.Element("x"), 0) + Int(r.Element("width"), 96));
			XElement template = rows.FirstOrDefault(r => (string)r.Element("id") == "com_job") ?? rows[0];
			int index = 0, added = 0;
			foreach (MenuDefinition def in entries)
			{
				XElement row = new XElement(template);
				row.SetElementValue("id", "com_mod_" + def.Id);
				row.SetElementValue("work", GlobalScope.wmenu.CWMenuMod.KIND + index);
				XElement behaviour = row.Element("behavior");
				if (behaviour != null)
				{
					behaviour.Elements("parameter").FirstOrDefault()?.SetValue("-1");
					row.Elements("data").ToList().ForEach(d => d.Remove());
					row.Add(new XElement("data", def.MainMenu.Label));
				}
				// In place of one of the game's rows, or after one (the game's, or another mod screen's by its id), or last.
				XElement replaced = string.IsNullOrWhiteSpace(def.MainMenu.Replaces) ? null : rows.FirstOrDefault(r => (string)r.Element("id") == def.MainMenu.Replaces.Trim());
				string afterId = def.MainMenu.After;
				if (afterId != null && !afterId.StartsWith("com_") && entries.Any(e => string.Equals(e.Id, afterId, StringComparison.OrdinalIgnoreCase))) afterId = "com_mod_" + entries.First(e => string.Equals(e.Id, afterId, StringComparison.OrdinalIgnoreCase)).Id;
				XElement after = rows.FirstOrDefault(r => (string)r.Element("id") == afterId);
				if (replaced != null) { replaced.AddAfterSelf(row); replaced.Remove(); }
				else if (after != null) { after.AddAfterSelf(row); added++; }
				else { rows[rows.Count - 1].AddAfterSelf(row); added++; }
				rows = commands.Elements("frame").ToList();
				index++;
			}
			// The character panels (p1..p4) and anything else numbered after the commands move down the
			// focus list by as many entries as went in: myTag is a focus index the game moves by.
			int original = rows.Count - added;
			foreach (XElement frame in main.Descendants("frame"))
			{
				if (frame.Parent == commands) continue;
				XElement tag = frame.Element("myTag");
				if (tag != null && int.TryParse(tag.Value, out int t) && t >= original) tag.Value = (t + added).ToString();
			}
			// Re-space: the list's rows sat 28 apart from y 2; the same run shared among the rows now there.
			int top = rows.Min(r => Int(r.Element("y"), 2));
			int bottom = rows.Max(r => Int(r.Element("y"), 2) + Int(r.Element("height"), 28));
			int pitch = Math.Max(16, (bottom - top) / rows.Count);
			for (int i = 0; i < rows.Count; i++)
			{
				rows[i].SetElementValue("y", top + i * pitch);
				rows[i].SetElementValue("height", Math.Min(28, pitch));
				rows[i].SetElementValue("up", (string)rows[(i + rows.Count - 1) % rows.Count].Element("id"));
				rows[i].SetElementValue("down", (string)rows[(i + 1) % rows.Count].Element("id"));
				rows[i].SetElementValue("myTag", i);   // the focus list's index, which MoveCursor lands on
			}
			_mainMenuRows = new GlobalScope.BackdropRows
			{
				Top = commandsY + oldTop, OldPitch = (oldBottom - oldTop) / (float)oldCount, OldCount = oldCount,
				NewPitch = pitch, NewCount = rows.Count, Left = left, Right = right
			};
		}

		private static int Int(XElement e, int fallback) => e != null && int.TryParse(e.Value, out int v) ? v : fallback;

		// ---- the backdrop's row lines ----

		/// <summary>The main menu's commands as the game had them and as they are with the mods' entries; null when none went in.</summary>
		private static GlobalScope.BackdropRows _mainMenuRows;

		/// <summary>Set by CWMenuMod while it sets its screen's backdrop up: the lines are the mod screen's to say.</summary>
		public static bool SettingModBackdrop;

		/// <summary>The lines' mode for one of the game's screens, set by a definition reaching it as the screen is built.</summary>
		private static string _gameScreenLines;

		/// <summary>How the backdrop just set up draws its row lines: a mod screen's own say; the main menu's fitted to its commands unless a mod says otherwise; one of the game's screens as a definition reaching it says.</summary>
		public static string BackdropLinesMode(int backdrop, out GlobalScope.BackdropRows rows)
		{
			rows = null;
			if (SettingModBackdrop) return Mode(_current?.BackdropLines) ?? "game";
			if (_gameScreenLines != null) return _gameScreenLines;
			if (backdrop == 9)
			{
				rows = _mainMenuRows;
				return Reaching("main_menu") ?? "fit";
			}
			return "game";
		}

		private static string Mode(string word)
		{
			string w = word?.Trim().ToLowerInvariant();
			return w == "none" || w == "fit" || w == "game" ? w : null;
		}

		/// <summary>
		/// The backdrop a mod's definition puts in place of the one the game's screen on now picks (0..14, 4 the plain one
		/// instead; -1 none); null for the screen's own. Kept from the screen's building until the next's (the game's screen
		/// asks for its backdrop before it is built, and may again as it runs - Magic's pages).
		/// </summary>
		public static int? GameBackdrop => SettingModBackdrop ? null : _gameBackdrop;
		private static int? _gameBackdrop;
		// The layout file the game holds now (Patch hears each load).
		private static string _loadedFile;

		private static int? ReachingBackdrop(string screen)
		{
			if (!_gameScreenDefs.TryGetValue(screen, out List<MenuDefinition> defs)) return null;
			MenuDefinition said = defs.LastOrDefault(d => d.BackgroundSaid);
			if (said == null) return null;
			int b = said.Background;
			if (b < 0) return -1;
			b = Math.Clamp(b, 0, 14);
			return b == 4 ? 10 : b;
		}

		private static string Reaching(string screen) =>
			_gameScreenDefs.TryGetValue(screen, out List<MenuDefinition> defs) ? defs.Select(d => Mode(d.BackdropLines)).LastOrDefault(m => m != null) : null;

		// ---- opening ----

		/// <summary>The screens with a main menu entry, in the order the entries go in (the ones that follow another mod screen after it) - the order work = KIND + index counts by.</summary>
		private static List<MenuDefinition> Entries()
		{
			List<MenuDefinition> entries = _all.Where(d => d.MainMenu != null && !string.IsNullOrWhiteSpace(d.MainMenu.Label)).ToList();
			return entries.Where(e => e.MainMenu.After == null || e.MainMenu.After.StartsWith("com_")).Concat(entries.Where(e => e.MainMenu.After != null && !e.MainMenu.After.StartsWith("com_"))).ToList();
		}

		/// <summary>The main menu's entry with work = KIND + index was chosen: that screen is current.</summary>
		public static bool Select(int work)
		{
			int index = work - GlobalScope.wmenu.CWMenuMod.KIND;
			List<MenuDefinition> entries = Entries();
			if (index < 0 || index >= entries.Count) return false;
			_current = entries[index];
			_fromField = false;
			return true;
		}

		public bool Open(string id)
		{
			MenuDefinition def = Find(id);
			if (def == null) { Log.Write(LogChannel.General, "menus: no screen called '" + id + "'"); return false; }
			if (_host != null && _screen != null)
			{
				// From one mod screen to another: no character pick on the way (the hero is the one picked before).
				_current = def;
				_skipSelect = true;
				_host.LeaveFor();
				return true;
			}
			if (!EngineApi.InWorld) return false;
			_current = def;
			_fromField = true;
			_requested = id;
			return true;
		}

		/// <summary>The world's move state asks each frame whether a mod wants the menu open; true once per request, with the main menu told to shift to the screen.</summary>
		public static bool MenuRequested()
		{
			if (_requested == null) return false;
			_requested = null;
			try { GlobalScope.wmenu.CWMenuManager.Instance().GetWMenuMain().autoShift((GlobalScope.wmenu.CWMenuMemberBase.WMENU_KIND)GlobalScope.wmenu.CWMenuMod.KIND); }
			catch (Exception ex) { Log.Write(LogChannel.General, "menus: " + ex.Message); }
			return true;
		}

		/// <summary>A main menu entry's place in its list - what the main menu remembers the cursor by (its work value once, before the mods' entries could move the game's own down).</summary>
		public static int MainMenuCursor(GlobalScope.menu.Medget entry)
		{
			try
			{
				if (entry == null) return 0;
				int index = 0;
				for (GlobalScope.menu.Medget m = entry.parentNode()?.childNode(); m != null; m = m.nextSibling())
				{
					if (m == entry) return index;
					index++;
				}
				return (sbyte)entry.work();
			}
			catch (Exception) { return 0; }
		}

		public static string CurrentScreenName() => _current?.Screen;
		/// <summary>The backdrop for the screen: one of the game's (0..14; 4 is nobody's, the plain one instead), or -1 for none (black, or the screen's own background).</summary>
		public static int CurrentBackground() { int b = _current?.Background ?? 10; if (b < 0) return -1; b = Math.Clamp(b, 0, 14); return b == 4 ? 10 : b; }
		private static bool _skipSelect;
		public static bool CurrentWantsCharacterSelect()
		{
			if (_skipSelect) { _skipSelect = false; return false; }
			return _current?.CharacterSelect ?? false;
		}

		// ---- the screen's events, from CWMenuMod ----

		public static void ScreenOpened(GlobalScope.wmenu.CWMenuMod host)
		{
			_host = host;
			if (_current == null) return;
			try
			{
				_screen = new ModMenuScreen(_current, host, _fromField);
				OpenWindows(_screen);
				foreach (IMenuWidget w in _screen.Widgets) (w as ModMenuWidget)?.ApplyStyle();
				_mods.TryGetValue(_current.Id, out LoadedMod mod);
				_behaviours = MenuLoader.Make(_current, _screen, mod);
				_screen.BehaviourList = _behaviours;
				Log.Write(LogChannel.General, "menus: " + _current.Id + " opened - " + _screen.Widgets.Count + " frame(s), " + _behaviours.Count + " behaviour(s)" + (_screen.Hero >= 0 ? ", hero " + _screen.Hero : ""));
				foreach (MenuBehaviour b in _behaviours) OpenFF.Game.Guard(b.Name + ".OnOpen", b.OnOpen);
				_screen.UpdateBindings();   // after OnOpen, which may have put the screen's Data in
				_screen.StartStyles();
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "menus: open: " + ex.Message); }
		}

		public static void ScreenClosed()
		{
			string closing = _screen?.Id ?? _current?.Id ?? "?";
			CloseWindows();
			// A screen with no backdrop put it out of sight; the game's own screens expect it back - and its faces where they stood.
			try { GlobalScope.wmenu.CWMenuManager.Instance().SetPrimaryBGVisibility(true); } catch (Exception) { }
			RestoreFaces();
			_arrowsWanted = null;
			foreach (MenuBehaviour b in _behaviours) OpenFF.Game.Guard(b.Name + ".OnClose", b.OnClose);
			_behaviours = new List<MenuBehaviour>();
			_screen = null;
			_host = null;
			Log.Write(LogChannel.General, "menus: " + closing + " closed");
		}

		public static void Tick()
		{
			_screenPanels.Flush();
			foreach (MenuBehaviour b in _behaviours) OpenFF.Game.Guard(b.Name + ".OnTick", b.OnTick);
			_screen?.UpdateBindings();
			// A transition or an animation under way: the look worked out again for this frame.
			if (_screen != null && _screen.Animating) OpenFF.Game.Guard("menus.animate", _screen.Restyle);
		}

		private static string _gameStyleFocus;

		public static void FocusChanged(string from, string to)
		{
			_screen?.FocusMoved(to);   // the sheets' :focus
			foreach (MenuBehaviour b in _behaviours)
			{
				if (from != null && (b.Target == from || b.Target == "")) OpenFF.Game.Guard(b.Name + ".OnBlur", b.OnBlur);
			}
			foreach (MenuBehaviour b in _behaviours)
			{
				if (to != null && (b.Target == to || b.Target == "")) OpenFF.Game.Guard(b.Name + ".OnFocus", b.OnFocus);
			}
		}

		/// <summary>Confirm: the frame's own behaviours first, then the screen's, until one takes it.</summary>
		public static void Pressed(string id)
		{
			bool handled = false;
			foreach (MenuBehaviour b in _behaviours.Where(b => id != null && b.Target == id).Concat(_behaviours.Where(b => b.Target == "")))
			{
				if (handled) break;
				OpenFF.Game.Guard(b.Name + ".OnPress", () => handled = b.OnPress());
			}
			if (!handled) Log.Write(LogChannel.File, "menus: press on " + (id ?? "nothing") + " - nothing took it");
		}

		/// <summary>Cancel: the behaviours first; the screen closes when none takes it.</summary>
		public static void Cancelled()
		{
			bool handled = false;
			foreach (MenuBehaviour b in _behaviours)
			{
				if (handled) break;
				OpenFF.Game.Guard(b.Name + ".OnCancel", () => handled = b.OnCancel());
			}
			if (!handled) { GlobalScope.menu.MenuManager.getSingleton().playSECancel(); _screen?.Close(); }
		}

		public static void Key(MenuKey key)
		{
			bool handled = false;
			foreach (MenuBehaviour b in _behaviours)
			{
				if (handled) break;
				OpenFF.Game.Guard(b.Name + ".OnKey", () => handled = b.OnKey(key));
			}
		}

		// ---- the game's own screens the mods reach (MenuManager tells us as they are built, run and released) ----

		/// <summary>buildMenu(name) has built one of the game's screens: the definitions reaching it get their behaviours over it.</summary>
		private static string _lastBuilt;

		public static void GameScreenBuilt(string name)
		{
			try
			{
				GameScreenReleased();
				_lastBuilt = name;
				// A definition reaching the screen that says its backdrop: that one in place of the screen's (set up again now -
				// the screen asked for its own before it was built); one that says none gives the screen's own back.
				// Only the menus' screens (MenuDefine.xbn's) have backdrops: a screen of another file (an inn's question in the field) leaves them be.
				int? backdrop = name == null ? null : ReachingBackdrop(name);
				if (string.Equals(_loadedFile, "MenuDefine.xbn", StringComparison.OrdinalIgnoreCase) && backdrop != _gameBackdrop)
				{
					_gameBackdrop = backdrop;
					try
					{
						GlobalScope.wmenu.CWMenuManager menus = GlobalScope.wmenu.CWMenuManager.Instance();
						menus.SetPrimaryBGVisibility(backdrop == null || backdrop >= 0);
						menus.ReapplyPrimaryBG();
					}
					catch (Exception) { }
				}
				// A definition reaching the screen with a say on its backdrop's lines: the backdrop set up again with them.
				string lines = name == null || string.Equals(name, "main_menu", StringComparison.OrdinalIgnoreCase) ? null : Reaching(name);
				if (lines != null)
				{
					_gameScreenLines = lines;
					try { GlobalScope.wmenu.CWMenuManager.Instance().ReapplyPrimaryBG(); } catch (Exception) { }
					finally { _gameScreenLines = null; }
				}
				if (name != null) OpenFF.Game.Guard("MenuOpened", () => OpenFF.Game.Events.Publish(new OpenFF.Events.MenuOpened { Screen = name, Mod = _current != null && string.Equals(_current.Screen, name, StringComparison.OrdinalIgnoreCase) }));
				if (name == null || !_gameScreenDefs.TryGetValue(name, out List<MenuDefinition> defs) || defs.Count == 0) return;
				if (_current != null && string.Equals(_current.Screen, name, StringComparison.OrdinalIgnoreCase) && _host != null) return;   // a mod screen of that name: CWMenuMod plays it
				_gameScreen = new ModMenuScreen(defs[0], null, false);
				OpenWindows(_gameScreen);
				foreach (IMenuWidget w in _gameScreen.Widgets) (w as ModMenuWidget)?.ApplyStyle();
				_gameBehaviours = new List<MenuBehaviour>();
				foreach (MenuDefinition def in defs)
				{
					_mods.TryGetValue(def.Id, out LoadedMod mod);
					_gameBehaviours.AddRange(MenuLoader.Make(def, _gameScreen, mod));
				}
				_gameScreen.BehaviourList = _gameBehaviours;
				_gameFocused = null;
				_gameStyleFocus = null;
				_gameScreen.StartStyles();
				Log.Write(LogChannel.General, "menus: the game's " + name + " built - " + _gameScreen.Widgets.Count + " frame(s), " + _gameBehaviours.Count + " behaviour(s) of the mods'");
				foreach (MenuBehaviour b in _gameBehaviours) OpenFF.Game.Guard(b.Name + ".OnOpen", b.OnOpen);
				_gameScreen.UpdateBindings();
			}
				catch (Exception ex) { Log.Write(LogChannel.General, "menus: " + name + ": " + ex.Message); }
		}

		/// <summary>MenuManager.execute() has run on one of the game's screens: focus, presses, cancel and keys go to the behaviours; one that takes a press or a cancel keeps it from the game's screen.</summary>
		public static void GameScreenTick()
		{
			_gameScreen?.UpdateBindings();
			if (_gameScreen == null) return;
			_screenPanels.Flush();
			// The sheets' states and their animations, behaviours or none.
			try
			{
				string focused = GlobalScope.menu.MenuManager.getSingleton().getFocuseMedget()?._id();
				if (focused != _gameStyleFocus) { _gameStyleFocus = focused; _gameScreen.FocusMoved(focused); }
				if (_gameScreen.Animating) _gameScreen.Restyle();
			}
			catch (Exception) { }
			if (_gameBehaviours.Count == 0) return;
			try
			{
				GlobalScope.menu.MenuManager mgr = GlobalScope.menu.MenuManager.getSingleton();
				string now = mgr.getFocuseMedget()?._id();
				if (now != _gameFocused)
				{
					Dispatch(_gameBehaviours, b => _gameFocused != null && (b.Target == _gameFocused || b.Target == ""), b => { b.OnBlur(); return false; }, "OnBlur");
					Dispatch(_gameBehaviours, b => now != null && (b.Target == now || b.Target == ""), b => { b.OnFocus(); return false; }, "OnFocus");
					_gameFocused = now;
				}
				if (mgr.GetActivateButtonState() != 0)
				{
					if (mgr.GetDecideButtonState() == 0)
					{
						bool handled = Dispatch(_gameBehaviours.Where(b => now != null && b.Target == now).Concat(_gameBehaviours.Where(b => b.Target == "")).ToList(), b => true, b => b.OnPress(), "OnPress");
						if (handled) mgr.SetDecideButtonState(1);   // taken: the game's screen sees no press
					}
					else if (mgr.GetCancelButtonState() == 0)
					{
						bool handled = Dispatch(_gameBehaviours, b => true, b => b.OnCancel(), "OnCancel");
						if (handled) mgr.SetCancelButtonState(1);
					}
				}
				int edge = GlobalScope.ds.g_Pad.edge();
				if ((edge & 0x40) != 0) Dispatch(_gameBehaviours, b => true, b => b.OnKey(MenuKey.Up), "OnKey");
				if ((edge & 0x80) != 0) Dispatch(_gameBehaviours, b => true, b => b.OnKey(MenuKey.Down), "OnKey");
				if ((edge & 0x20) != 0) Dispatch(_gameBehaviours, b => true, b => b.OnKey(MenuKey.Left), "OnKey");
				if ((edge & 0x10) != 0) Dispatch(_gameBehaviours, b => true, b => b.OnKey(MenuKey.Right), "OnKey");
				if ((edge & 0x200) != 0) Dispatch(_gameBehaviours, b => true, b => b.OnKey(MenuKey.L), "OnKey");
				if ((edge & 0x100) != 0) Dispatch(_gameBehaviours, b => true, b => b.OnKey(MenuKey.R), "OnKey");
				if ((edge & 0x400) != 0) Dispatch(_gameBehaviours, b => true, b => b.OnKey(MenuKey.X), "OnKey");
				if ((edge & 0x800) != 0) Dispatch(_gameBehaviours, b => true, b => b.OnKey(MenuKey.Y), "OnKey");
				foreach (MenuBehaviour b in _gameBehaviours) OpenFF.Game.Guard(b.Name + ".OnTick", b.OnTick);
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "menus: " + ex.Message); }
		}

		/// <summary>The game's screen is being released (another built, or the menus left): its behaviours hear OnClose, its windows go.</summary>
		public static void GameScreenReleased()
		{
			if (_lastBuilt != null)
			{
				string was = _lastBuilt;
				_lastBuilt = null;
				OpenFF.Game.Guard("MenuClosed", () => OpenFF.Game.Events.Publish(new OpenFF.Events.MenuClosed { Screen = was }));
			}
			if (_gameScreen == null) return;
			foreach (MenuBehaviour b in _gameBehaviours) OpenFF.Game.Guard(b.Name + ".OnClose", b.OnClose);
			_gameBehaviours = new List<MenuBehaviour>();
			string name = _gameScreen.Definition.Screen;
			_gameScreen = null;
			CloseWindows();
			_arrowsWanted = null;
			Log.Write(LogChannel.File, "menus: the game's " + name + " released");
		}

		/// <summary>Behaviours in order until one answers true (when the event asks); each guarded.</summary>
		private static bool Dispatch(IEnumerable<MenuBehaviour> behaviours, Func<MenuBehaviour, bool> to, Func<MenuBehaviour, bool> call, string what)
		{
			bool handled = false;
			foreach (MenuBehaviour b in behaviours)
			{
				if (handled) break;
				if (!to(b)) continue;
				OpenFF.Game.Guard(b.Name + "." + what, () => handled = call(b));
			}
			return handled;
		}

		// ---- the windows: a frame with <window/> is drawn with the game's window art (BasicWindow), behind its texts ----

		private static readonly List<GlobalScope.menu.BasicWindow> _windows = new List<GlobalScope.menu.BasicWindow>();

		/// <summary>What a frame's panel is made of - its window, its sprites and their painted textures, the look they were made for - so a restyle that only changes its look (an animation, a transition) puts it on in place.</summary>
		private sealed class FramePanels
		{
			public int Back;
			public MenuStyles.Look Look;
			public (int X, int Y) Offset;
			public GlobalScope.menu.BasicWindow Window;
			public List<GlobalScope.MenuPanelSprite> Sprites = new List<GlobalScope.MenuPanelSprite>();
			public List<uint> Textures = new List<uint>();
		}

		private static readonly Dictionary<IMenuWidget, FramePanels> _framePanels = new Dictionary<IMenuWidget, FramePanels>();

		/// <summary>A frame's panel made again with a new look where only what can be put on in place changed (its background, opacity, tint); false when it needs the windows made again.</summary>
		private static bool UpdatePanel(ModMenuScreen screen, ModMenuWidget m, MenuStyles.Look look)
		{
			if (!_framePanels.TryGetValue(m, out FramePanels fp) || fp.Look == null) return false;
			MenuStyles.Look was = fp.Look;
			if (was.Window != look.Window || was.Bar != look.Bar || was.Hidden != look.Hidden || was.Portrait != look.Portrait || (was.Background == null) != (look.Background == null)) return false;
			if (look.Portrait != null && Math.Abs(was.Opacity - look.Opacity) > 0.001) return false;
			if (fp.Window != null && (Math.Abs(was.Opacity - look.Opacity) > 0.001 || was.Tint != look.Tint))
			{
				uint? tint = StyleRgb(look.Tint) is uint rgb ? (rgb >> 16 & 0xFF) | (rgb & 0xFF00) | ((rgb & 0xFF) << 16) : (uint?)null;
				fp.Window.SetLook((float)look.Opacity, tint);
			}
			if (was.Background == look.Background && Math.Abs(was.Opacity - look.Opacity) > 0.001) MenuPanels.Fade(fp.Sprites, look.Opacity);   // a fade: in place
			else if (was.Background != look.Background)
			{
				_screenPanels.Remove(fp.Sprites, fp.Textures);
				fp.Sprites.Clear();
				fp.Textures.Clear();
				if (look.Background != null && !look.Hidden) AddPanel(screen, m.Id, m.X, m.Y, m.Width, m.Height, look.Background, look.Opacity, fp.Back + GlobalScope.ds.S32toFX32(8), fp);
				if (fp.Offset != (0, 0)) MenuPanels.Move(fp.Sprites, fp.Offset.X, fp.Offset.Y);
			}
			fp.Look = look;
			return true;
		}

		/// <summary>A frame's panel moved from its place (its translate as it moves: ModMenuScreen.Restyle) - its sprites and its window.</summary>
		private static void MovePanel(ModMenuWidget m, int dx, int dy)
		{
			if (!_framePanels.TryGetValue(m, out FramePanels fp) || fp.Offset == (dx, dy)) return;
			fp.Offset = (dx, dy);
			MenuPanels.Move(fp.Sprites, dx, dy);
			try { fp.Window?.SetPositionUL(new GlobalScope.ds.Vector2<short>((short)(m.X + dx), (short)(m.Y + dy))); } catch (Exception) { }
		}

		private static void OpenWindows(ModMenuScreen screen)
		{
			CloseWindows();
			// The frames' looks as they are now (baked, then restyled as the screen runs): a panel, bar, opacity, tint, hidden.
			List<IMenuWidget> panels = screen.Widgets.Where(w => w is ModMenuWidget m && m.Look.HasPanel && w.Width > 0 && w.Height > 0).ToList();
			int place = 0;
			// The screen's own background (its <menu>'s style, or a sheet's menu rule): over the backdrop, behind every window.
			string whole = screen.ScreenBackground();
			if (whole != null) AddPanel(screen, screen.Id, 0, 0, GlobalScope.LCD_WIDTH, GlobalScope.LCD_HEIGHT, whole, 1, panels.Count * GlobalScope.ds.S32toFX32(32) + GlobalScope.ds.S32toFX32(8));
			foreach (IMenuWidget w in panels)
			{
				ModMenuWidget m = (ModMenuWidget)w;
				// Stacked in the layout's order, as CSS stacks them: each earlier window a step further back (a step
				// is two of the fill's 16-unit offsets behind its frame), the last where every window stood before.
				int back = (panels.Count - 1 - place++) * GlobalScope.ds.S32toFX32(32);
				try
				{
					MenuStyles.Look look = m.Look;
					FramePanels fp = new FramePanels { Back = back, Look = look };
					_framePanels[m] = fp;
					if (look.Hidden) continue;
					// A background (a colour, a picture - sliced, tiled, fitted...; a gradient, a border, round corners, shadows) between the window's fill and its frame.
					if (look.Background != null) AddPanel(screen, w, look, back, fp);
					// A portrait frame: the hero's face in its place, fitted, over its own window and background.
					if (look.Portrait != null && screen.FaceOf(look.Portrait) is string face)
						AddPanel(screen, w.Id, w.X, w.Y, w.Width, w.Height, "background-image: resource(\"" + face + "\"); -ff-background-scale-mode: scale-to-fit", look.Opacity, back - GlobalScope.ds.S32toFX32(4));
					if (!look.Window) continue;
					GlobalScope.menu.BasicWindow window = new GlobalScope.menu.BasicWindow();
					window.bwCreateUL(GlobalScope.sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, new GlobalScope.ds.Vector2<short>((short)w.X, (short)w.Y), new GlobalScope.ds.Vector2<short>((short)w.Width, (short)w.Height), 3);
					window.SetPriority(3);
					if (back > 0) window.SetStackDepth(window.GetDepth() + back);
					// The frame's style (MenuStyles bakes it into these): the game's translucent bar in place of the
					// window, the whole of it at an opacity, its art tinted.
					if (look.Bar) window.SetBarStyle();
					float opacity = (float)look.Opacity;
					uint? tint = StyleRgb(look.Tint) is uint rgb ? (rgb >> 16 & 0xFF) | (rgb & 0xFF00) | ((rgb & 0xFF) << 16) : (uint?)null;
					if (opacity < 1f || tint.HasValue) window.SetLook(opacity, tint);
					window.SetShow(show: true, user: true);
					_windows.Add(window);
					fp.Window = window;
				}
				catch (Exception ex) { Log.Write(LogChannel.General, "menus: window " + w.Id + ": " + ex.Message); }
			}
			HideGameFaces(screen);
			// A list's arrows went with the windows: back as the screen last had them.
			if (_arrowsWanted is var (s, first, last, shown, up, down) && ReferenceEquals(s, screen)) PutArrows(screen, first, last, shown, up, down);
		}

		/// <summary>A frame's &lt;opacity&gt; (0..1, MenuStyles's product of its and its parents'), 1 for none.</summary>
		private static float StyleOpacity(GlobalScope.XbnNode node)
		{
			string text = node?.getFirstNodeByTagNameFromChildren("opacity")?.nodeValueString();
			return float.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float o) ? Math.Clamp(o, 0f, 1f) : 1f;
		}

		/// <summary>#rrggbb (or #rrggbbaa, its alpha passed over) as 0xRRGGBB; null when it is not one.</summary>
		internal static uint? StyleRgb(string text)
		{
			string v = text?.Trim();
			if (v != null && v.Length == 9 && v[0] == '#') v = v.Substring(0, 7);
			if (v == null || v.Length != 7 || v[0] != '#') return null;
			return uint.TryParse(v.Substring(1), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out uint rgb) ? rgb : (uint?)null;
		}

		/// <summary>The game's faces a screen moved (Portrait on a screen with no portrait frame) and where each stood, to put back as it closes.</summary>
		private static readonly List<(int Pc, short X, short Y)> _movedFaces = new List<(int, short, short)>();

		private static void RestoreFaces()
		{
			try
			{
				GlobalScope.wmenu.CWMenuPCFaceManager faces = GlobalScope.wmenu.CWMenuManager.Instance().GetPcFace();
				foreach ((int pc, short x, short y) in _movedFaces) faces.pcfmSetPosition((uint)pc, x, y, clear: false);
			}
			catch (Exception) { }
			_movedFaces.Clear();
		}

		/// <summary>The game's own faces put away while a layout has a portrait frame of its own (the frame draws the face instead).</summary>
		private static void HideGameFaces(ModMenuScreen screen)
		{
			if (!screen.HasPortraitFrame) return;
			try { for (int i = 0; i < 4; i++) GlobalScope.wmenu.CWMenuManager.Instance().SetShowPcFace(i, show: false); } catch (Exception) { }
		}

		private static void CloseWindows()
		{
			foreach (GlobalScope.menu.BasicWindow w in _windows) { try { w.Release(); } catch (Exception) { } }
			_windows.Clear();
			_screenPanels.Clear();
			_framePanels.Clear();
			ReleaseArrows();
		}

		// ---- backgrounds: a frame's colour and picture (MenuBackground), its painted box (MenuPaint): sprites with the windows ----

		private static readonly MenuPanels _screenPanels = new MenuPanels();

		private static void AddPanel(ModMenuScreen screen, IMenuWidget w, MenuStyles.Look look, int back, FramePanels into)
		{
			AddPanel(screen, w.Id, w.X, w.Y, w.Width, w.Height, look.Background, look.Opacity, back + GlobalScope.ds.S32toFX32(8), into);
		}

		/// <summary>A background over a rectangle of the screen (a frame's, or the whole screen's), at a depth in the windows' stack; its sprites (and painted textures) noted in 'into' when given.</summary>
		private static void AddPanel(ModMenuScreen screen, string what, int x, int y, int width, int height, string declarations, double alpha, int depth, FramePanels into = null)
		{
			_screenPanels.Add(screen.Definition?.Directory, what, x, y, width, height, declarations, alpha, depth, into?.Sprites, into?.Textures);
		}

		// ---- scroll arrows: a list's, as the battle's command list has them (btl.Triangle) ----

		private static readonly GlobalScope.sys2d.Sprite3d[] _arrows = new GlobalScope.sys2d.Sprite3d[2];
		// What the screen asked for last, so the arrows come back when its windows are made again (a restyle).
		private static (ModMenuScreen Screen, string First, string Last, bool Shown, bool Up, bool Down)? _arrowsWanted;

		/// <summary>
		/// A list's arrows up, placed and lit: Steam's icon_16dot cells 9 and 13 at two thirds of a 16-dot icon, white
		/// when the list goes on that way and grey when it does not; the phone's icon_left / icon_right with their
		/// animations. Where frames "arrow_up" / "arrow_down" of the layout are, else at the right of the rows' window
		/// (their nearest window), level with the first row and the last.
		/// </summary>
		private static void PutArrows(ModMenuScreen screen, string firstRow, string lastRow, bool shown, bool up, bool down)
		{
			if (!ReferenceEquals(screen, _screen) && !ReferenceEquals(screen, _gameScreen)) return;
			_arrowsWanted = (screen, firstRow, lastRow, shown, up, down);
			IMenuWidget first = screen.Widget(firstRow), last = screen.Widget(lastRow) ?? first;
			if (!shown || first == null) { ReleaseArrows(); return; }
			bool steam = SteamLayout.Active;
			try
			{
				IMenuWidget box = first;
				while (box is ModMenuWidget m && !m.Look.Window && m.ParentWidget != null) box = m.ParentWidget;
				int right = box.X + box.Width;
				for (int i = 0; i < 2; i++)
				{
					if (_arrows[i] == null)
					{
						GlobalScope.sys2d.Sprite3d s = new GlobalScope.sys2d.Sprite3d();
						s.Load2(GlobalScope.sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, steam ? "icon_16dot" : i == 0 ? "icon_left" : "icon_right");
						GlobalScope.sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(s);
						s.SetCell(steam ? (ushort)(i == 0 ? 9 : 13) : (ushort)0);
						if (steam) s.SetScaleF(GlobalScope.FX32_CONST(0.68f), GlobalScope.FX32_CONST(0.68f));
						s.SetPriority(3);   // with the windows, in front of them (the hand's depth)
						s.SetDepth(0);
						_arrows[i] = s;
					}
					GlobalScope.sys2d.Sprite3d a = _arrows[i];
					IMenuWidget placed = screen.Widget(i == 0 ? "arrow_up" : "arrow_down");
					IMenuWidget row = i == 0 ? first : last;
					int x = placed?.X ?? right - ArrowRightInset;
					int y = placed?.Y ?? row.Y + row.Height / 2 - ArrowHalf;
					a.SetPositionI(x, y);
					bool lit = i == 0 ? up : down;
					if (steam) a.SetColor(lit ? 0xFFFFFFu : 0x7F7F7Fu);
					else { a.SetCell((ushort)(lit ? 0 : 1)); a.PlayAnimation((ushort)(lit ? 0 : 1), GlobalScope.NNSG2dAnimationPlayMode.NNS_G2D_ANIMATIONPLAYMODE_FORWARD); }
					a.SetShow(show: true);
				}
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "menus: scroll arrows: " + ex.Message); ReleaseArrows(); }
		}

		// The arrow's place against the rows' window (its right edge in, its middle on the row's): the battle's triangle
		// stands 16 in from its list's right, a cell of about 11 on Steam.
		private const int ArrowRightInset = 16;
		private const int ArrowHalf = 6;

		private static void ReleaseArrows()
		{
			for (int i = 0; i < 2; i++)
			{
				if (_arrows[i] == null) continue;
				try { GlobalScope.sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(_arrows[i]); _arrows[i].Release(); } catch (Exception) { }
				_arrows[i] = null;
			}
		}

		// ---- the screen over the Medget tree ----

		private sealed class ModMenuScreen : IMenuScreen
		{
			private readonly GlobalScope.wmenu.CWMenuMod _host;
			private readonly bool _fromField;
			private readonly List<ModMenuWidget> _widgets = new List<ModMenuWidget>();
			private readonly Dictionary<string, ModMenuWidget> _byId = new Dictionary<string, ModMenuWidget>(StringComparer.OrdinalIgnoreCase);
			// The layout as written (classes, bindings, the frames' own styles) and its sheets, for the screen's
			// restyling and binding as it runs; a copy of its own each time the screen opens. Null for one of the
			// game's screens that no layout of a mod's reaches.
			private readonly XElement _source;
			private readonly MenuStyles.Sheet _sheet;
			private readonly string _dataSource;
			private bool _bindingsFailed;
			private readonly Dictionary<string, XElement> _sourceById;
			private readonly MenuAnimation.Animator _animator = new MenuAnimation.Animator();
			private string _focusState;
			// Each frame's translate as its layout was built with it: a translate as it moves moves what the frame draws by the difference.
			private Dictionary<XElement, string> _baseTranslate;
			private static readonly System.Diagnostics.Stopwatch _clock = System.Diagnostics.Stopwatch.StartNew();

			/// <summary>Whether a transition or an animation is under way: the look is worked out again every frame while it is.</summary>
			public bool Animating => _source != null && _animator.Active;

			/// <summary>The styles begun as the screen opens: the cursor's frame in :focus, and the animator's first frame (animations start; nothing transitions from before the screen).</summary>
			public void StartStyles()
			{
				if (_source == null) return;
				_baseTranslate = new Dictionary<XElement, string>();
				foreach ((XElement f, Dictionary<string, string> c) in MenuStyles.Computed(_source, _sheet)) _baseTranslate[f] = c.TryGetValue("translate", out string t) ? t : null;
				try { SetFocusState(GlobalScope.menu.MenuManager.getSingleton().getFocuseMedget()?._id()); } catch (Exception) { }
				Restyle();
			}

			/// <summary>The cursor moved: the frame it is on now in :focus (and its parents in :focus-within), the look worked out again when a sheet asks for states.</summary>
			public void FocusMoved(string id)
			{
				if (_source == null || id == _focusState) return;
				SetFocusState(id);
				if (_sheet.UsesStates || _animator.Active) Restyle();
			}

			private void SetFocusState(string id)
			{
				if (_focusState != null && _sourceById.TryGetValue(_focusState, out XElement old)) State(old, "focus", false);
				_focusState = id;
				if (id != null && _sourceById.TryGetValue(id, out XElement now)) State(now, "focus", true);
			}

			/// <summary>A frame's state (focus, disabled) on or off, for the sheets' pseudo-classes; restyled when a sheet asks for it.</summary>
			public void SetState(string id, string state, bool on)
			{
				if (_source == null || id == null || !_sourceById.TryGetValue(id, out XElement frame)) return;
				if (State(frame, state, on) && _sheet.UsesStates) Restyle();
			}

			private static bool State(XElement frame, string state, bool on)
			{
				List<string> states = ((string)frame.Attribute(MenuStyles.StateAttribute) ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
				if (states.Contains(state) == on) return false;
				if (on) states.Add(state); else states.Remove(state);
				frame.SetAttributeValue(MenuStyles.StateAttribute, states.Count > 0 ? string.Join(" ", states) : null);
				return true;
			}

			/// <summary>A file of the screen's for a text's face (font-family: url(...)): the lettering with it made a path the face can be read from.</summary>
			public MenuText Lettering(string declarations)
			{
				MenuText text = MenuText.Parse(declarations);
				if (text == null) return null;
				for (int i = 0; i < text.Families.Count; i++)
				{
					if (!text.Families[i].StartsWith("url:", StringComparison.Ordinal)) continue;
					string dir = Definition?.Directory;
					text.Families[i] = dir == null ? "missing" : "file:" + Path.GetFullPath(Path.Combine(dir, text.Families[i].Substring(4)));
				}
				return text;
			}

			public ModMenuScreen(MenuDefinition def, GlobalScope.wmenu.CWMenuMod host, bool fromField)
			{
				Definition = def;
				_host = host;
				_fromField = fromField;
				if (def?.Id != null && _styleSources.TryGetValue(def.Id, out (XElement Menu, List<string> Sheets) style))
				{
					_source = new XElement(style.Menu);
					_sheet = MenuStyles.Compile(style.Sheets);
					_dataSource = (string)_source.Attribute("data-source");
				}
				Dictionary<string, XElement> sources = _source?.Descendants("frame").Where(f => !string.IsNullOrEmpty((string)f.Element("id")))
					.GroupBy(f => ((string)f.Element("id")).Trim(), StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
				_sourceById = sources ?? new Dictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
				GlobalScope.menu.Medget root = GlobalScope.menu.MenuManager.getSingleton().GetBaseMedget();
				for (GlobalScope.menu.Medget m = root?.childNode(); m != null; m = m.nextSibling()) Add(m, null, sources);
				// The hero picked (a mod screen that asked; the game's per-hero screens - Status, Equipment - have one too).
				try { Hero = def.CharacterSelect || host == null ? GlobalScope.pl.PlayerParty.instance().player((byte)GlobalScope.menu.MenuManager.getSingleton().GetTargetCharNo()).playerId() : -1; }
				catch (Exception) { Hero = -1; }
			}

			private void Add(GlobalScope.menu.Medget m, ModMenuWidget parent, Dictionary<string, XElement> sources)
			{
				string id = m._id();
				XElement source = id != null && sources != null && sources.TryGetValue(id.Trim(), out XElement s) ? s : null;
				ModMenuWidget w = new ModMenuWidget(m, this, parent, source);
				_widgets.Add(w);
				parent?.ChildList.Add(w);
				if (!string.IsNullOrEmpty(w.Id) && !_byId.ContainsKey(w.Id)) _byId[w.Id] = w;
				for (GlobalScope.menu.Medget c = m.childNode(); c != null; c = c.nextSibling()) Add(c, w, sources);
			}

			public MenuDefinition Definition { get; }
			public string Id => Definition.Id;
			public IMenuWidget Widget(string id) => id != null && _byId.TryGetValue(id, out ModMenuWidget w) ? w : null;
			public IReadOnlyList<IMenuWidget> Widgets => _widgets;
			public string Focused => GlobalScope.menu.MenuManager.getSingleton().getFocuseMedget()?._id();
			public int Hero { get; }
			public IDictionary<string, object> Data { get; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

			public void Focus(string id)
			{
				if (Widget(id) is ModMenuWidget w) GlobalScope.menu.MenuManager.getSingleton().setFocuseMedget(w.Medget);
			}

			public void SetText(string id, string text) { if (Widget(id) is ModMenuWidget w) w.Text = text; }
			/// <summary>Leaves: a mod screen to the main menu (or the field); one of the game's by the game's own cancel.</summary>
			public void Close()
			{
				if (_host != null) { _host.Leave(toMainMenu: !_fromField); return; }
				GlobalScope.menu.MenuManager.getSingleton().SetCancelButtonState(0);
			}

			/// <summary>Another screen of the mods': from a mod screen at once; from one of the game's, within the game's menu part.</summary>
			public void Open(string menuId)
			{
				if (_host != null) { ((ModMenus)OpenFF.Game.Menus).Open(menuId); return; }
				MenuDefinition def = ((ModMenus)OpenFF.Game.Menus).Find(menuId);
				if (def == null) { Log.Write(LogChannel.General, "menus: no screen called '" + menuId + "'"); return; }
				try
				{
					ModMenus._current = def;
					ModMenus._fromField = false;
					ModMenus._skipSelect = true;
					GlobalScope.wmenu.CWMenuManager.Instance().SetNextKind((GlobalScope.wmenu.CWMenuMemberBase.WMENU_KIND)GlobalScope.wmenu.CWMenuMod.KIND);
					GlobalScope.wmenu.CWMenuManager.Instance().SetProcState(GlobalScope.wmenu.CWMenuMemberBase.WMENU_PROCESS.WMENU_PROCESS_TERMINATE);
				}
				catch (Exception ex) { Log.Write(LogChannel.General, "menus: open " + menuId + " from the game's screen: " + ex.Message); }
			}
			public void SoundDecide() => GlobalScope.menu.MenuManager.getSingleton().playSEDecide();
			public void SoundBeep() => GlobalScope.menu.MenuManager.getSingleton().playSEBeep();
			public void SoundCancel() => GlobalScope.menu.MenuManager.getSingleton().playSECancel();
			/// <summary>The behaviours made for this screen (set once they are).</summary>
			public List<MenuBehaviour> BehaviourList = new List<MenuBehaviour>();
			public IReadOnlyList<MenuBehaviour> Behaviours => BehaviourList;
			public T Behaviour<T>(string target = null) where T : MenuBehaviour => BehaviourList.OfType<T>().FirstOrDefault(b => b.Target == (target ?? ""));

			// ---- selectors: the frames as the sheets see them, their classes as they are now ----

			public IMenuWidget Q(string selector) => Query(selector).FirstOrDefault();

			public IReadOnlyList<IMenuWidget> Query(string selector)
			{
				List<IMenuWidget> found = new List<IMenuWidget>();
				if (string.IsNullOrWhiteSpace(selector)) return found;
				XElement menu = new XElement("menu", new XElement("name", Definition?.Screen ?? Id));
				Dictionary<XElement, ModMenuWidget> mirror = new Dictionary<XElement, ModMenuWidget>();
				void Mirror(XElement into, IEnumerable<ModMenuWidget> widgets)
				{
					foreach (ModMenuWidget w in widgets)
					{
						XElement e = new XElement("frame", new XElement("id", w.Id ?? ""));
						if (w.Classes.Count > 0) e.SetAttributeValue("class", string.Join(" ", w.Classes));
						if (w.Look.Window) e.Add(new XElement("window"));
						if (w.IsText) e.Add(new XElement("behavior", new XAttribute("value", "Text")));
						into.Add(e);
						mirror[e] = w;
						Mirror(e, w.ChildList);
					}
				}
				Mirror(menu, _widgets.Where(w => w.ParentWidget == null));
				foreach (XElement e in menu.Descendants("frame")) if (MenuStyles.Matches(e, selector)) found.Add(mirror[e]);
				return found;
			}

			// ---- the look, cascaded again as classes and the frames' own styles change ----

			public bool Styled => _source != null;

			/// <summary>The screen's own background (the &lt;menu&gt;'s style and the sheets' rules for it), or null.</summary>
			public string ScreenBackground() => _source == null ? null : MenuStyles.ScreenBackground(_source, _sheet);

			/// <summary>
			/// The sheets cascaded over the layout as it is now (its classes, its frames' states), with the transitions and
			/// animations under way; each frame whose look changed gets it - its panel put on in place where it can be, the
			/// windows made again where it cannot (a window made or taken away).
			/// </summary>
			public void Restyle()
			{
				if (_source == null) return;
				List<(XElement Frame, Dictionary<string, string> Computed)> computed = MenuStyles.Computed(_source, _sheet);
				Dictionary<XElement, Dictionary<string, string>> shown = _animator.Step(computed, _sheet, _clock.Elapsed.TotalSeconds);
				Dictionary<XElement, MenuStyles.Look> looks = MenuStyles.Looks(_source, computed, shown);
				bool current = ReferenceEquals(this, _screen) || ReferenceEquals(this, _gameScreen);
				bool rebuild = false;
				foreach (ModMenuWidget w in _widgets)
				{
					if (w.Source == null || !looks.TryGetValue(w.Source, out MenuStyles.Look look) || look.SameAs(w.Look)) continue;
					bool panel = !look.SamePanel(w.Look) && (look.HasPanel || w.Look.HasPanel);
					w.PutLook(look);
					if (panel && current && !rebuild && !UpdatePanel(this, w, look)) rebuild = true;
				}
				if (rebuild && current) OpenWindows(this);
				// What moves (a translate's transition or animation): each frame's drawing by its translate's difference from the
				// one its layout was built with, its parents' with it - its text, its window and its panel.
				if (_baseTranslate != null && looks.Values.Any(l => l.Translate != null) || _moved)
				{
					_moved = false;
					Dictionary<XElement, ModMenuWidget> bySource = _widgets.Where(w => w.Source != null).GroupBy(w => w.Source).ToDictionary(g => g.Key, g => g.First());
					foreach (ModMenuWidget w in _widgets)
					{
						if (w.Source == null) continue;
						float x = 0, y = 0;
						for (XElement f = w.Source; f != null && f.Name.LocalName == "frame"; f = f.Parent)
						{
							if (!bySource.TryGetValue(f, out ModMenuWidget owner)) continue;
							looks.TryGetValue(f, out MenuStyles.Look look);
							(float nx, float ny) = MenuStyles.TranslateOf(look?.Translate, owner.Width, owner.Height);
							(float bx, float by) = MenuStyles.TranslateOf(_baseTranslate != null && _baseTranslate.TryGetValue(f, out string b) ? b : null, owner.Width, owner.Height);
							x += nx - bx;
							y += ny - by;
						}
						int dx = (int)Math.Round(x), dy = (int)Math.Round(y);
						if (dx != 0 || dy != 0) _moved = true;
						w.PutOffset(dx, dy);
						if (current) MovePanel(w, dx, dy);
					}
				}
			}

			// Whether a frame stood moved last time (so a move back to its place is put on too).
			private bool _moved;

			// ---- the portrait ----

			private int _portrait = -1;

			public bool HasPortraitFrame => _widgets.Any(w => w.Look.Portrait != null);

			/// <summary>The hero whose face is shown: the one set (Portrait), else the one picked, else the party's first.</summary>
			public int FaceHero()
			{
				if (_portrait >= 0) return _portrait;
				if (Hero >= 0) return Hero;
				IReadOnlyList<PartyMember> members = OpenFF.Game.Party?.Members;
				return members != null && members.Count > 0 ? members[0].Id : 0;
			}

			public int Portrait
			{
				get => _portrait;
				set
				{
					int was = FaceHero();
					_portrait = value;
					int now = FaceHero();
					if (now == was) return;
					if (HasPortraitFrame)
					{
						if (ReferenceEquals(this, _screen) || ReferenceEquals(this, _gameScreen)) OpenWindows(this);
						return;
					}
					// No frame of the layout's for it: the game's own face, swapped to that hero - in the place the first one
					// had (each hero's face stands at its party slot's own place), put back as the screen closes.
					try
					{
						GlobalScope.wmenu.CWMenuManager menus = GlobalScope.wmenu.CWMenuManager.Instance();
						GlobalScope.wmenu.CWMenuPCFaceManager faces = menus.GetPcFace();
						GlobalScope.NNSG2dSVec2 at = faces.pcfmGetPosition((uint)was);
						GlobalScope.NNSG2dSVec2 own = faces.pcfmGetPosition((uint)now);
						if (!_movedFaces.Any(m => m.Pc == now)) _movedFaces.Add((now, own.x, own.y));
						menus.SetShowPcFace(was, show: false);
						faces.pcfmSetPosition((uint)now, at.x, at.y, clear: true);
						menus.SetShowPcFace(now, show: true);
					}
					catch (Exception) { }
				}
			}

			/// <summary>The face picture for a portrait frame: its party slot ("0".."3"), or the screen's hero - files/pc&lt;hero&gt;_&lt;job&gt;.NCGR, as the game's own faces are.</summary>
			public string FaceOf(string portrait)
			{
				int id = FaceHero();
				IReadOnlyList<PartyMember> members = OpenFF.Game.Party?.Members;
				if (int.TryParse(portrait, out int slot) && members != null && slot >= 0 && slot < members.Count) id = members[slot].Id;
				PartyMember member = OpenFF.Game.Party?.Member(id);
				if (id < 0 || id > 3) return null;
				return "files/pc" + (id + 1) + "_" + ((member?.Job ?? 0) + 1).ToString("00", System.Globalization.CultureInfo.InvariantCulture) + ".NCGR";
			}

			// ---- bindings ----

			public void Refresh() => UpdateBindings();

			public void ScrollArrows(string firstRow, string lastRow, bool shown, bool up, bool down) => PutArrows(this, firstRow, lastRow, shown, up, down);

			/// <summary>Every frame's bindings worked out, with its data source's (and its parents'); what changed is put on.</summary>
			public void UpdateBindings()
			{
				MenuBindingScope top = new MenuBindingScope { Root = Root };
				try
				{
					if (_dataSource != null) top = top.With(MenuBindings.Resolve(_dataSource, top));
					bool restyle = false;
					foreach (ModMenuWidget w in _widgets.Where(w => w.ParentWidget == null)) restyle |= w.UpdateBindings(top);
					if (restyle) Restyle();
				}
				catch (Exception ex)
				{
					if (!_bindingsFailed) Log.Write(LogChannel.General, "menus: " + Id + " bindings: " + ex.Message);
					_bindingsFailed = true;
				}
			}

			/// <summary>The roots a binding's path may start with: the screen's own Data, then the game's.</summary>
			private (bool, object) Root(string name)
			{
				if (Data.TryGetValue(name, out object mine)) return (true, mine);
				switch (name.ToLowerInvariant())
				{
					case "hero": return (true, Hero >= 0 ? OpenFF.Game.Party?.Member(Hero) : null);
					case "party": return (true, OpenFF.Game.Party?.Members);
					case "gil": return (true, OpenFF.Game.Party?.Gil ?? 0);
					case "items": return (true, OpenFF.Game.Party?.Items);
					case "menu": return (true, this);
					default: return (false, null);
				}
			}
		}

		private sealed class ModMenuWidget : IMenuWidget
		{
			public readonly GlobalScope.menu.Medget Medget;
			public readonly List<ModMenuWidget> ChildList = new List<ModMenuWidget>();
			public readonly ModMenuScreen Screen;
			public readonly ModMenuWidget ParentWidget;
			/// <summary>The frame in the screen's layout as written (its classes, style and bindings), or null.</summary>
			public readonly XElement Source;
			private string _text;
			private bool _visible = true;
			private MenuColour? _colour;
			// The colour the text has of its own - the layout's, or what code set - to go back to when a style's (a :focus rule's) ends.
			private MenuColour? _ownColour;
			private uint? _ownRgba;
			private int _fontSize;
			// The style's look (MenuStyles): a colour of its own (#rrggbb, as 0xRRGGBBAA), its opacity, hidden.
			private uint? _rgba;
			private byte _alpha = 255;
			private bool _hidden;
			private readonly HashSet<string> _classes = new HashSet<string>(StringComparer.Ordinal);
			// A frame of one of the game's screens has no layout of a mod's to keep its own style in.
			private readonly Dictionary<string, string> _ownStyle = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			private readonly Dictionary<string, string> _binds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			private string _dataSource;
			private string _boundText;
			private bool? _boundVisible;
			// A bound style (bind-style) as it is put on the frame's own style, and the properties it put there.
			private string _boundStyle;
			private readonly List<string> _boundStyleKeys = new List<string>();

			/// <summary>The look the frame has now: as the layout's bake left it, then as the screen restyles it.</summary>
			public MenuStyles.Look Look { get; private set; }

			public ModMenuWidget(GlobalScope.menu.Medget m, ModMenuScreen screen, ModMenuWidget parent, XElement source)
			{
				Medget = m;
				Screen = screen;
				ParentWidget = parent;
				Source = source;
				GlobalScope.XbnNode node = m.node();
				string word = node?.getFirstNodeByTagNameFromChildren("colour")?.nodeValueString() ?? node?.getFirstNodeByTagNameFromChildren("color")?.nodeValueString();
				if (StyleRgb(word) is uint rgb) _rgba = rgb << 8 | 0xFF;
				else if (!string.IsNullOrWhiteSpace(word)) _colour = ColourWord(word);
				_alpha = (byte)Math.Round(StyleOpacity(node) * 255);
				_hidden = node?.getFirstNodeByTagNameFromChildren("hidden") != null;
				_ownColour = _colour;
				_ownRgba = _rgba;
				// <font>N</font>: a size of the text's own (6..31), drawn by the TrueType face at that size. The layout
				// writer stores a number as an int node (MenuXbn), a word as a string: read whichever it is.
				GlobalScope.XbnNode font = node?.getFirstNodeByTagNameFromChildren("font");
				if (font != null)
				{
					int size = font.nodeValueString() != null ? (int.TryParse(font.nodeValueString().Trim(), out int n) ? n : 0) : font.nodeValueInt();
					if (size >= 6 && size <= 31) _fontSize = size;
				}
				Look = new MenuStyles.Look
				{
					Colour = word?.Trim(),
					Font = font == null ? null : (font.nodeValueString()?.Trim() ?? font.nodeValueInt().ToString(System.Globalization.CultureInfo.InvariantCulture)),
					Opacity = StyleOpacity(node),
					Hidden = _hidden,
					Window = node?.getFirstNodeByTagNameFromChildren("window") != null,
					Bar = string.Equals(node?.getFirstNodeByTagNameFromChildren("panel")?.nodeValueString()?.Trim(), "bar", StringComparison.OrdinalIgnoreCase),
					Tint = node?.getFirstNodeByTagNameFromChildren("tint")?.nodeValueString()?.Trim(),
					Background = string.IsNullOrWhiteSpace(node?.getFirstNodeByTagNameFromChildren("background")?.nodeValueString()) ? null : node.getFirstNodeByTagNameFromChildren("background").nodeValueString().Trim(),
					Portrait = node?.getFirstNodeByTagNameFromChildren("portrait") == null ? null : (node.getFirstNodeByTagNameFromChildren("portrait").nodeValueString()?.Trim() ?? ""),
					TextStyle = Element(node, "textstyle"),
					Transition = Element(node, "transition"),
					Animation = Element(node, "animation")
				};
				// The layout's classes and bindings (attributes the game's own file never carried).
				foreach (string c in ((string)source?.Attribute("class") ?? "").Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)) _classes.Add(c);
				_dataSource = (string)source?.Attribute("data-source");
				if (source?.Attribute("bind-text") is XAttribute text) _binds["text"] = text.Value;
				if (source?.Attribute("bind-visible") is XAttribute visible) _binds["visible"] = visible.Value;
				if (source?.Attribute("bind-style") is XAttribute boundStyle) _binds["style"] = boundStyle.Value;
				foreach (KeyValuePair<string, string> c in MenuStyles.Declarations((string)source?.Attribute("bind-class"))) _binds["class." + c.Key] = c.Value;
			}

			private static string Element(GlobalScope.XbnNode node, string name)
			{
				string v = node?.getFirstNodeByTagNameFromChildren(name)?.nodeValueString();
				return string.IsNullOrWhiteSpace(v) ? null : v.Trim();
			}

			public bool IsText => Text_ != null;

			// The text's move (a translate): from the place its message was put, taken again for a message made afresh.
			private (int X, int Y) _offset;
			private GlobalScope.dgs.DGSMessage _placed;
			private (short X, short Y) _place;

			public void PutOffset(int dx, int dy)
			{
				GlobalScope.dgs.DGSMessage message = Text_?.getMessage();
				if (message == null) { _offset = (dx, dy); return; }
				if (!ReferenceEquals(message, _placed)) { _placed = message; _place = (message.positionX(), message.positionY()); }
				else if (_offset == (dx, dy)) return;
				_offset = (dx, dy);
				try { message.setPosition((short)(_place.X + dx), (short)(_place.Y + dy), erase: true); } catch (Exception) { }
			}

			/// <summary>The text laid out afresh (a new message, its size measured again): its place taken again, and the move put on it.</summary>
			private void PutOffsetAgain()
			{
				_placed = null;
				if (_offset != (0, 0)) PutOffset(_offset.X, _offset.Y);
			}

			// The text's own lettering (MenuText, its faces' files found): onto its canvas and its font.
			private MenuText _lettering;
			private bool _enabled = true;

			public bool Enabled
			{
				get => _enabled;
				set { if (_enabled == value) return; _enabled = value; Screen.SetState(Id, "disabled", !value); }
			}

			/// <summary>The layout's colour, put on as the screen opens (a text drawn afresh comes up white).</summary>
			public void ApplyStyle()
			{
				_lettering = Look.TextStyle == null ? null : Screen.Lettering(Look.TextStyle);
				if (_fontSize > 0 || _lettering != null) PutFont();
				if (_colour.HasValue) Colour = _colour.Value;
				PutLook();
				PutVisible();
			}

			/// <summary>A look the screen's restyling came to: its colour, size, opacity and hidden onto the text (the panel is the windows').</summary>
			public void PutLook(MenuStyles.Look look)
			{
				MenuStyles.Look was = Look;
				Look = look;
				if (look.Colour != was?.Colour)
				{
					if (StyleRgb(look.Colour) is uint rgb) { _rgba = rgb << 8 | 0xFF; _colour = null; }
					else if (!string.IsNullOrWhiteSpace(look.Colour)) { _colour = ColourWord(look.Colour); _rgba = null; }
					else { _rgba = _ownRgba; _colour = _ownRgba.HasValue ? null : _ownColour ?? MenuColour.White; }
					try { GlobalScope.NNSG2dTextCanvas canvas = Text_?.getMessage()?.m_TextCanvas; if (canvas != null) canvas.rgba = _rgba; if (_colour.HasValue) Text_?.changeTextColor((GlobalScope.dgs.TXT_COLOR)(int)_colour.Value); } catch (Exception) { }
				}
				if (look.Font != was?.Font && int.TryParse(look.Font, out int size) && size >= 6 && size <= 31) FontSize = size;
				if (look.TextStyle != was?.TextStyle)
				{
					_lettering = look.TextStyle == null ? null : Screen.Lettering(look.TextStyle);
					PutFont();
				}
				_alpha = (byte)Math.Round(Math.Clamp(look.Opacity, 0, 1) * 255);
				_hidden = look.Hidden;
				GlobalScope.NNSG2dTextCanvas c2 = Text_?.getMessage()?.m_TextCanvas;
				if (c2 != null) c2.alpha = _alpha;
				PutVisible();
				try { Text_?.getMessage()?.Redraw(); } catch (Exception) { }   // the message draws only when told: its new look now
			}

			/// <summary>The style's own colour and opacity onto the text's canvas - again whenever the message is made afresh (Text).</summary>
			private void PutLook()
			{
				if (!_rgba.HasValue && _alpha == 255) return;
				GlobalScope.NNSG2dTextCanvas canvas = Text_?.getMessage()?.m_TextCanvas;
				if (canvas == null) return;
				canvas.rgba = _rgba;
				canvas.alpha = _alpha;
			}

			/// <summary>Shown when the code (Visible), the style (hidden) and a binding (bind-visible) all let it be.</summary>
			private void PutVisible()
			{
				try { Text_?.bmTextVisibility(_visible && !_hidden && _boundVisible != false); } catch (Exception) { }
			}

			/// <summary>The text's size in the game's units (12 and 16 are the game's two); the message is drawn afresh at it.</summary>
			public int FontSize
			{
				get => _fontSize > 0 ? _fontSize : (Text_?.getMessage()?.m_TextCanvas?.pFont?.size ?? 12);
				set
				{
					_fontSize = Math.Clamp(value, 6, 31);
					PutFont();
				}
			}

			/// <summary>
			/// The size onto the text's canvas. The message is drawn afresh every frame from its canvas's font
			/// (NNS_G2dTextCanvasDrawText), so the font goes on after the message is made - mbSetBufferMsg makes
			/// a new message with the game's own font each time the text changes, so Text puts it on again.
			/// </summary>
			private void PutFont()
			{
				GlobalScope.dgs.DGSMessage message = Text_?.getMessage();
				if (message?.m_TextCanvas == null) return;
				if (_fontSize <= 0 && _lettering == null && message.m_TextCanvas.style == null) return;
				try
				{
					message.m_TextCanvas.style = _lettering;
					int size = _fontSize > 0 ? _fontSize : message.m_TextCanvas.pFont?.size ?? 12;
					GlobalScope.NNSG2dFont font = message.m_TextCanvas.pFont;
					if (font != null && font.size == size && ReferenceEquals(font.style, _lettering)) return;
					// A font of its own (the game's is shared by every text): its size, and its lettering for the measuring.
					message.m_TextCanvas.pFont = new GlobalScope.NNSG2dFont { size = size, style = _lettering };
					Text_.mbtSetAlignment();   // measured again at the new size: right and centre alignments, and the middle of a taller frame
					PutOffsetAgain();
				}
				catch (Exception) { }
			}

			private GlobalScope.menu.MBText Text_ => Medget.behavior()?.queryInterface(GlobalScope.menu.MBText.classIdentifier()) as GlobalScope.menu.MBText;

			public string Id => Medget._id();
			public int X => Medget.x();
			public int Y => Medget.y();
			public int Width => Medget.width();
			public int Height => Medget.height();
			public int Work => Medget.work() is IConvertible w ? Convert.ToInt32(w) : 0;
			/// <summary>The frame in Game.Draw's 800 x 480 units: the layout's units scaled as the game scales its text (Font.drawString).</summary>
			public (float X, float Y, float Width, float Height) ScreenRect
			{
				get
				{
					float sx = 800f / Math.Max(1, GlobalScope.LCD_WIDTH), sy = 480f / Math.Max(1, GlobalScope.LCD_HEIGHT);
					return (X * sx, Y * sy, Width * sx, Height * sy);
				}
			}
			public bool Focusable => Medget.node()?.getFirstNodeByTagNameFromChildren("focus") != null;
			public IReadOnlyList<IMenuWidget> Children => ChildList;
			public IMenuWidget Parent => ParentWidget;

			public string Text
			{
				get => _text ?? Medget.node()?.getFirstNodeByTagName("data")?.nodeValueString() ?? "";
				set { _text = value ?? ""; Text_?.mbSetBufferMsg(_text.Length == 0 ? " " : _text, decWidth: false); PutFont(); if (_colour.HasValue) Colour = _colour.Value; PutLook(); PutVisible(); PutOffsetAgain(); }   // a message made afresh comes up shown
			}

			// A palette colour set from code takes the place of a style's #hex one.
			public MenuColour Colour { set { _colour = value; _rgba = null; _ownColour = value; _ownRgba = null; try { Text_?.changeTextColor((GlobalScope.dgs.TXT_COLOR)(int)value); GlobalScope.NNSG2dTextCanvas canvas = Text_?.getMessage()?.m_TextCanvas; if (canvas != null) canvas.rgba = null; } catch (Exception) { } } }

			public bool Visible
			{
				get => _visible;
				set { _visible = value; PutVisible(); }
			}

			// ---- classes and the frame's own style ----

			public IReadOnlyCollection<string> Classes => _classes;
			public bool HasClass(string name) => name != null && _classes.Contains(name.Trim());
			public void AddClass(string name) => ToggleClass(name, true);
			public void RemoveClass(string name) => ToggleClass(name, false);

			public void ToggleClass(string name, bool? on = null)
			{
				if (!SetClass(name, on ?? !HasClass(name))) return;
				Screen.Restyle();
			}

			/// <summary>The class on or off, without restyling yet; true when that changed anything.</summary>
			public bool SetClass(string name, bool on)
			{
				name = name?.Trim();
				if (string.IsNullOrEmpty(name) || name.Contains(' ')) return false;
				if (!(on ? _classes.Add(name) : _classes.Remove(name))) return false;
				if (Source != null)
				{
					if (_classes.Count > 0) Source.SetAttributeValue("class", string.Join(" ", _classes));
					else Source.Attribute("class")?.Remove();
				}
				return true;
			}

			public string GetStyle(string property)
			{
				if (string.IsNullOrWhiteSpace(property)) return null;
				if (Source == null) return _ownStyle.TryGetValue(property.Trim(), out string v) ? v : null;
				foreach (KeyValuePair<string, string> d in MenuStyles.Declarations((string)Source.Attribute("style")))
				{
					if (string.Equals(d.Key, property.Trim(), StringComparison.OrdinalIgnoreCase)) return d.Value;
				}
				return null;
			}

			public void SetStyle(string property, string value)
			{
				property = property?.Trim().ToLowerInvariant();
				if (string.IsNullOrEmpty(property)) return;
				if (Source != null)
				{
					List<KeyValuePair<string, string>> own = MenuStyles.Declarations((string)Source.Attribute("style")).Where(d => d.Key != property).ToList();
					if (!string.IsNullOrWhiteSpace(value)) own.Add(new KeyValuePair<string, string>(property, value.Trim()));
					if (own.Count > 0) Source.SetAttributeValue("style", string.Join("; ", own.Select(d => d.Key + ": " + d.Value)));
					else Source.Attribute("style")?.Remove();
					Screen.Restyle();
					return;
				}
				// One of the game's screens: no sheets to cascade - the look properties put on directly.
				if (string.IsNullOrWhiteSpace(value)) _ownStyle.Remove(property); else _ownStyle[property] = value.Trim();
				MenuStyles.Look look = new MenuStyles.Look { Colour = Look.Colour, Font = Look.Font, Align = Look.Align, Opacity = Look.Opacity, Hidden = Look.Hidden, Window = Look.Window, Bar = Look.Bar, Tint = Look.Tint };
				string v = value?.Trim();
				switch (property)
				{
					case "color": look.Colour = MenuStyles.Colour(v); break;
					case "font-size": look.Font = v?.Replace("px", "").Trim(); break;
					case "opacity": look.Opacity = v == null ? 1 : v.EndsWith("%") && double.TryParse(v.TrimEnd('%'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double p) ? p / 100 : double.TryParse(v, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double o) ? o : 1; break;
					case "visibility": look.Hidden = string.Equals(v, "hidden", StringComparison.OrdinalIgnoreCase); break;
					default: return;
				}
				PutLook(look);
			}

			public float Opacity
			{
				get => (float)Look.Opacity;
				set => SetStyle("opacity", Math.Clamp(value, 0f, 1f).ToString("0.###", System.Globalization.CultureInfo.InvariantCulture));
			}

			// ---- bindings ----

			public string DataSource { get => _dataSource; set => _dataSource = string.IsNullOrWhiteSpace(value) ? null : value.Trim(); }

			public void Bind(string property, string expression)
			{
				property = property?.Trim();
				if (string.IsNullOrEmpty(property)) return;
				if (string.IsNullOrWhiteSpace(expression))
				{
					_binds.Remove(property);
					if (property.Equals("visible", StringComparison.OrdinalIgnoreCase)) { _boundVisible = null; PutVisible(); }
					if (property.Equals("text", StringComparison.OrdinalIgnoreCase)) _boundText = null;
				}
				else _binds[property] = expression;
			}

			public string Binding(string property) => property != null && _binds.TryGetValue(property.Trim(), out string e) ? e : null;

			/// <summary>A bound style's declarations onto the frame's own style (the last bound's taken off first); true when the screen restyles for it.</summary>
			private bool PutBoundStyle(string style)
			{
				if (Source == null) return false;
				List<KeyValuePair<string, string>> own = MenuStyles.Declarations((string)Source.Attribute("style")).Where(d => !_boundStyleKeys.Contains(d.Key)).ToList();
				_boundStyleKeys.Clear();
				foreach (KeyValuePair<string, string> d in MenuStyles.Declarations(style)) { own.RemoveAll(o => o.Key == d.Key); own.Add(d); _boundStyleKeys.Add(d.Key); }
				Source.SetAttributeValue("style", own.Count > 0 ? string.Join("; ", own.Select(d => d.Key + ": " + d.Value)) : null);
				return true;
			}

			/// <summary>This frame's bindings under its data source, then its frames'; true when a class changed (the screen restyles once for all of them).</summary>
			public bool UpdateBindings(MenuBindingScope scope)
			{
				if (_dataSource != null) scope = scope.With(MenuBindings.Resolve(_dataSource, scope));
				bool classes = false;
				foreach (KeyValuePair<string, string> b in _binds)
				{
					if (b.Key.Equals("text", StringComparison.OrdinalIgnoreCase))
					{
						string text = MenuBindings.Format(b.Value, scope);
						if (text != _boundText) { _boundText = text; Text = text; }
					}
					else if (b.Key.Equals("visible", StringComparison.OrdinalIgnoreCase))
					{
						bool on = MenuBindings.Test(b.Value, scope);
						if (on != _boundVisible) { _boundVisible = on; PutVisible(); }
					}
					else if (b.Key.Equals("style", StringComparison.OrdinalIgnoreCase))
					{
						string style = MenuBindings.Format(b.Value, scope);
						if (style != _boundStyle) { _boundStyle = style; classes |= PutBoundStyle(style); }
					}
					else if (b.Key.StartsWith("class.", StringComparison.OrdinalIgnoreCase))
					{
						classes |= SetClass(b.Key.Substring(6), MenuBindings.Test(b.Value, scope));
					}
				}
				foreach (ModMenuWidget c in ChildList) classes |= c.UpdateBindings(scope);
				return classes;
			}
		}
	}
}
