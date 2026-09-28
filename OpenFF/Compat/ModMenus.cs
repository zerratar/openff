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
		}

		private static int Int(XElement e, int fallback) => e != null && int.TryParse(e.Value, out int v) ? v : fallback;

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
			foreach (MenuBehaviour b in _behaviours) OpenFF.Game.Guard(b.Name + ".OnClose", b.OnClose);
			_behaviours = new List<MenuBehaviour>();
			_screen = null;
			_host = null;
			Log.Write(LogChannel.General, "menus: " + closing + " closed");
		}

		public static void Tick()
		{
			foreach (MenuBehaviour b in _behaviours) OpenFF.Game.Guard(b.Name + ".OnTick", b.OnTick);
			_screen?.UpdateBindings();
		}

		public static void FocusChanged(string from, string to)
		{
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
			if (_gameScreen == null || _gameBehaviours.Count == 0) return;
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
					if (look.Hidden) continue;
					// A background (a colour, a picture - sliced, tiled, fitted...) between the window's fill and its frame.
					if (look.Background != null) AddPanel(screen, w, look, back);
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
				}
				catch (Exception ex) { Log.Write(LogChannel.General, "menus: window " + w.Id + ": " + ex.Message); }
			}
			HideGameFaces(screen);
		}

		/// <summary>A frame's &lt;opacity&gt; (0..1, MenuStyles's product of its and its parents'), 1 for none.</summary>
		private static float StyleOpacity(GlobalScope.XbnNode node)
		{
			string text = node?.getFirstNodeByTagNameFromChildren("opacity")?.nodeValueString();
			return float.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float o) ? Math.Clamp(o, 0f, 1f) : 1f;
		}

		/// <summary>#rrggbb as 0xRRGGBB; null when it is not one.</summary>
		private static uint? StyleRgb(string text)
		{
			string v = text?.Trim();
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
			foreach (GlobalScope.MenuPanelSprite p in _panels) { try { GlobalScope.sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(p); } catch (Exception) { } }
			_panels.Clear();
			foreach ((uint Id, int W, int H) t in _panelTextures.Values) { try { GlobalScope.MenuPanelRelease(t.Id); } catch (Exception) { } }
			_panelTextures.Clear();
		}

		// ---- backgrounds: a frame's colour and picture (MenuBackground), drawn as a sprite with the windows ----

		private static readonly List<GlobalScope.MenuPanelSprite> _panels = new List<GlobalScope.MenuPanelSprite>();
		private static readonly Dictionary<string, (uint Id, int W, int H)> _panelTextures = new Dictionary<string, (uint, int, int)>(StringComparer.OrdinalIgnoreCase);

		private static void AddPanel(ModMenuScreen screen, IMenuWidget w, MenuStyles.Look look, int back)
		{
			AddPanel(screen, w.Id, w.X, w.Y, w.Width, w.Height, look.Background, look.Opacity, back + GlobalScope.ds.S32toFX32(8));
		}

		/// <summary>A background over a rectangle of the screen (a frame's, or the whole screen's), at a depth in the windows' stack.</summary>
		private static void AddPanel(ModMenuScreen screen, string what, int x, int y, int width, int height, string declarations, double alpha, int depth)
		{
			MenuBackground bg = MenuBackground.Parse(declarations);
			if (bg == null) return;
			// A named sprite of the sheet (sprites.json beside the layouts): its part and its borders.
			if (bg.Sprite != null)
			{
				MenuSprites.Sprite named = MenuSprites.Find(screen.Definition?.Directory, bg.ImageKind, bg.ImagePath, bg.Sprite);
				if (named != null) bg.Use(named);
				else Log.Write(LogChannel.General, "menus: " + what + ": no sprite '" + bg.Sprite + "' on " + bg.ImagePath + " in " + MenuSprites.FileName);
			}
			GlobalScope.MenuPanelSprite sprite = new GlobalScope.MenuPanelSprite { Width = width, Height = height };
			float opacity = (float)Math.Clamp(alpha, 0, 1);
			byte[] Bytes(uint rgba) => new[] { (byte)(rgba >> 24), (byte)(rgba >> 16), (byte)(rgba >> 8), (byte)Math.Round((rgba & 0xFF) * opacity) };
			if (bg.Colour.HasValue) sprite.Fill = Bytes(bg.Colour.Value);
			sprite.Tint = Bytes(bg.Tint);
			if (bg.ImagePath != null)
			{
				string key = bg.ImageKind + ":" + bg.ImagePath + (bg.Linear ? "" : "#point");
				if (!_panelTextures.TryGetValue(key, out (uint Id, int W, int H) texture))
				{
					byte[] data = null;
					try
					{
						if (bg.ImageKind == "resource") data = GameArchive.Read(bg.ImagePath);
						else if (screen.Definition?.Directory != null)
						{
							string path = Path.GetFullPath(Path.Combine(screen.Definition.Directory, bg.ImagePath));
							if (File.Exists(path)) data = File.ReadAllBytes(path);
						}
						int tw = 0, th = 0;
						uint id = data == null ? 0 : GlobalScope.MenuPanelTexture(data, bg.Linear, out tw, out th);
						texture = id == 0 ? (0u, 0, 0) : (id, tw, th);
						if (id == 0 && data == null) Log.Write(LogChannel.General, "menus: " + what + ": no picture " + bg.ImageKind + "(\"" + bg.ImagePath + "\")");
					}
					catch (Exception ex) { Log.Write(LogChannel.General, "menus: " + what + ": picture " + bg.ImagePath + ": " + ex.Message); }
					_panelTextures[key] = texture;
				}
				if (texture.Id != 0)
				{
					sprite.Texture = texture.Id;
					sprite.TextureWidth = texture.W;
					sprite.TextureHeight = texture.H;
					sprite.Quads = bg.Layout(width, height, texture.W, texture.H);
				}
			}
			sprite.SetPlane(GlobalScope.sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D);
			sprite.SetPriority(3);
			// A frame's: between a window's fill (its depth and 16 more) and its frame (its depth) - its place in the stack and 8.
			sprite.SetDepth(depth);
			sprite.SetPositionI(x, y);
			sprite.SetShow(show: true);
			GlobalScope.sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(sprite);
			_panels.Add(sprite);
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

			/// <summary>The sheets cascaded over the layout as it is now; each frame whose look changed gets it, and the windows are made again if a panel did.</summary>
			public void Restyle()
			{
				if (_source == null) return;
				Dictionary<XElement, MenuStyles.Look> looks = MenuStyles.Looks(_source, _sheet);
				bool panels = false;
				foreach (ModMenuWidget w in _widgets)
				{
					if (w.Source == null || !looks.TryGetValue(w.Source, out MenuStyles.Look look) || look.SameAs(w.Look)) continue;
					if (!look.SamePanel(w.Look) && (look.Window || w.Look.Window)) panels = true;
					w.PutLook(look);
				}
				if (panels && (ReferenceEquals(this, _screen) || ReferenceEquals(this, _gameScreen))) OpenWindows(this);
			}

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
					Portrait = node?.getFirstNodeByTagNameFromChildren("portrait") == null ? null : (node.getFirstNodeByTagNameFromChildren("portrait").nodeValueString()?.Trim() ?? "")
				};
				// The layout's classes and bindings (attributes the game's own file never carried).
				foreach (string c in ((string)source?.Attribute("class") ?? "").Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)) _classes.Add(c);
				_dataSource = (string)source?.Attribute("data-source");
				if (source?.Attribute("bind-text") is XAttribute text) _binds["text"] = text.Value;
				if (source?.Attribute("bind-visible") is XAttribute visible) _binds["visible"] = visible.Value;
				foreach (KeyValuePair<string, string> c in MenuStyles.Declarations((string)source?.Attribute("bind-class"))) _binds["class." + c.Key] = c.Value;
			}

			public bool IsText => Text_ != null;

			/// <summary>The layout's colour, put on as the screen opens (a text drawn afresh comes up white).</summary>
			public void ApplyStyle()
			{
				if (_fontSize > 0) FontSize = _fontSize;
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
					else { _colour = MenuColour.White; _rgba = null; }
					try { GlobalScope.NNSG2dTextCanvas canvas = Text_?.getMessage()?.m_TextCanvas; if (canvas != null) canvas.rgba = _rgba; if (_colour.HasValue) Text_?.changeTextColor((GlobalScope.dgs.TXT_COLOR)(int)_colour.Value); } catch (Exception) { }
				}
				if (look.Font != was?.Font && int.TryParse(look.Font, out int size) && size >= 6 && size <= 31) FontSize = size;
				_alpha = (byte)Math.Round(Math.Clamp(look.Opacity, 0, 1) * 255);
				_hidden = look.Hidden;
				GlobalScope.NNSG2dTextCanvas c2 = Text_?.getMessage()?.m_TextCanvas;
				if (c2 != null) c2.alpha = _alpha;
				PutVisible();
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
				if (_fontSize <= 0) return;
				GlobalScope.dgs.DGSMessage message = Text_?.getMessage();
				if (message?.m_TextCanvas == null) return;
				try
				{
					if (message.m_TextCanvas.pFont?.size == _fontSize) return;
					message.m_TextCanvas.pFont = new GlobalScope.NNSG2dFont { size = _fontSize };
					Text_.mbtSetAlignment();   // measured again at the new size: right and centre alignments, and the middle of a taller frame
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
				set { _text = value ?? ""; Text_?.mbSetBufferMsg(_text.Length == 0 ? " " : _text, decWidth: false); PutFont(); if (_colour.HasValue) Colour = _colour.Value; PutLook(); PutVisible(); }   // a message made afresh comes up shown
			}

			// A palette colour set from code takes the place of a style's #hex one.
			public MenuColour Colour { set { _colour = value; _rgba = null; try { Text_?.changeTextColor((GlobalScope.dgs.TXT_COLOR)(int)value); GlobalScope.NNSG2dTextCanvas canvas = Text_?.getMessage()?.m_TextCanvas; if (canvas != null) canvas.rgba = null; } catch (Exception) { } } }

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
