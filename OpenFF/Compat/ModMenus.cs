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
			string summary = _all.Count == 0 ? null : "menus: " + _all.Count + " screen(s) of the mods' own: " + string.Join(", ", _all.Select(d => d.Id + (d.MainMenu != null ? " (main menu: " + d.MainMenu.Label + ")" : "")));
			if (summary != null && summary != _lastSummary) Log.Write(LogChannel.General, summary);
			_lastSummary = summary;
		}

		// ---- the layouts ----

		/// <summary>The screens of the game's own that mods reach (behaviours, a layout of the mod's, a patch), by screen name, filled as the files load.</summary>
		private static readonly Dictionary<string, List<MenuDefinition>> _gameScreenDefs = new Dictionary<string, List<MenuDefinition>>(StringComparer.OrdinalIgnoreCase);

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
				if (hud) BattleHudLayout.Ensure(file, doc, GlobalScope.BATTLE_COMMAND_X(), GlobalScope.BATTLE_COMMAND_Y(), GlobalScope.BATTLE_PLAYER_Y());
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
		/// (the second parameter, 16 or 8), <align>left|right|center|button</align> (the third: 0, 1, 2, 4 - button draws
		/// the game's button frame behind the text). <colour> is kept on the frame and put on as the screen opens.
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
			if (align != null) parameters[2].Value = align == "right" ? "1" : align == "center" || align == "centre" ? "2" : align == "button" ? "4" : "0";
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
		public static int CurrentBackground() { int b = Math.Clamp(_current?.Background ?? 10, 0, 14); return b == 4 ? 10 : b; }   // 4 is nobody's; the plain backdrop instead
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
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "menus: open: " + ex.Message); }
		}

		public static void ScreenClosed()
		{
			string closing = _screen?.Id ?? _current?.Id ?? "?";
			CloseWindows();
			foreach (MenuBehaviour b in _behaviours) OpenFF.Game.Guard(b.Name + ".OnClose", b.OnClose);
			_behaviours = new List<MenuBehaviour>();
			_screen = null;
			_host = null;
			Log.Write(LogChannel.General, "menus: " + closing + " closed");
		}

		public static void Tick()
		{
			foreach (MenuBehaviour b in _behaviours) OpenFF.Game.Guard(b.Name + ".OnTick", b.OnTick);
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
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "menus: " + name + ": " + ex.Message); }
		}

		/// <summary>MenuManager.execute() has run on one of the game's screens: focus, presses, cancel and keys go to the behaviours; one that takes a press or a cancel keeps it from the game's screen.</summary>
		public static void GameScreenTick()
		{
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
			foreach (IMenuWidget w in screen.Widgets)
			{
				if (!(w is ModMenuWidget m) || m.Medget.node()?.getFirstNodeByTagNameFromChildren("window") == null || w.Width <= 0 || w.Height <= 0) continue;
				try
				{
					GlobalScope.menu.BasicWindow window = new GlobalScope.menu.BasicWindow();
					window.bwCreateUL(GlobalScope.sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, new GlobalScope.ds.Vector2<short>((short)w.X, (short)w.Y), new GlobalScope.ds.Vector2<short>((short)w.Width, (short)w.Height), 3);
					window.SetPriority(3);
					window.SetShow(show: true, user: true);
					_windows.Add(window);
				}
				catch (Exception ex) { Log.Write(LogChannel.General, "menus: window " + w.Id + ": " + ex.Message); }
			}
		}

		private static void CloseWindows()
		{
			foreach (GlobalScope.menu.BasicWindow w in _windows) { try { w.Release(); } catch (Exception) { } }
			_windows.Clear();
		}

		// ---- the screen over the Medget tree ----

		private sealed class ModMenuScreen : IMenuScreen
		{
			private readonly GlobalScope.wmenu.CWMenuMod _host;
			private readonly bool _fromField;
			private readonly List<ModMenuWidget> _widgets = new List<ModMenuWidget>();
			private readonly Dictionary<string, ModMenuWidget> _byId = new Dictionary<string, ModMenuWidget>(StringComparer.OrdinalIgnoreCase);

			public ModMenuScreen(MenuDefinition def, GlobalScope.wmenu.CWMenuMod host, bool fromField)
			{
				Definition = def;
				_host = host;
				_fromField = fromField;
				GlobalScope.menu.Medget root = GlobalScope.menu.MenuManager.getSingleton().GetBaseMedget();
				for (GlobalScope.menu.Medget m = root?.childNode(); m != null; m = m.nextSibling()) Add(m, null);
				// The hero picked (a mod screen that asked; the game's per-hero screens - Status, Equipment - have one too).
				try { Hero = def.CharacterSelect || host == null ? GlobalScope.pl.PlayerParty.instance().player((byte)GlobalScope.menu.MenuManager.getSingleton().GetTargetCharNo()).playerId() : -1; }
				catch (Exception) { Hero = -1; }
			}

			private void Add(GlobalScope.menu.Medget m, ModMenuWidget parent)
			{
				ModMenuWidget w = new ModMenuWidget(m);
				_widgets.Add(w);
				parent?.ChildList.Add(w);
				if (!string.IsNullOrEmpty(w.Id) && !_byId.ContainsKey(w.Id)) _byId[w.Id] = w;
				for (GlobalScope.menu.Medget c = m.childNode(); c != null; c = c.nextSibling()) Add(c, w);
			}

			public MenuDefinition Definition { get; }
			public string Id => Definition.Id;
			public IMenuWidget Widget(string id) => id != null && _byId.TryGetValue(id, out ModMenuWidget w) ? w : null;
			public IReadOnlyList<IMenuWidget> Widgets => _widgets;
			public string Focused => GlobalScope.menu.MenuManager.getSingleton().getFocuseMedget()?._id();
			public int Hero { get; }

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
		}

		private sealed class ModMenuWidget : IMenuWidget
		{
			public readonly GlobalScope.menu.Medget Medget;
			public readonly List<ModMenuWidget> ChildList = new List<ModMenuWidget>();
			private string _text;
			private bool _visible = true;
			private MenuColour? _colour;
			private int _fontSize;

			public ModMenuWidget(GlobalScope.menu.Medget m)
			{
				Medget = m;
				string word = m.node()?.getFirstNodeByTagNameFromChildren("colour")?.nodeValueString() ?? m.node()?.getFirstNodeByTagNameFromChildren("color")?.nodeValueString();
				if (!string.IsNullOrWhiteSpace(word)) _colour = ColourWord(word);
				// <font>N</font>: a size of the text's own (6..31), drawn by the TrueType face at that size.
				string font = m.node()?.getFirstNodeByTagNameFromChildren("font")?.nodeValueString();
				if (int.TryParse(font?.Trim(), out int size) && size >= 6 && size <= 31) _fontSize = size;
			}

			/// <summary>The layout's colour, put on as the screen opens (a text drawn afresh comes up white).</summary>
			public void ApplyStyle()
			{
				if (_fontSize > 0) FontSize = _fontSize;
				if (_colour.HasValue) Colour = _colour.Value;
			}

			/// <summary>The text's size in the game's units (12 and 16 are the game's two); the message is drawn afresh at it.</summary>
			public int FontSize
			{
				get => _fontSize > 0 ? _fontSize : (Text_?.getMessage()?.m_TextCanvas?.pFont?.size ?? 12);
				set
				{
					_fontSize = Math.Clamp(value, 6, 31);
					GlobalScope.dgs.DGSMessage message = Text_?.getMessage();
					if (message?.m_TextCanvas == null) return;
					try
					{
						message.m_TextCanvas.pFont = new GlobalScope.NNSG2dFont { size = _fontSize };
						Text_.mbSetBufferMsg(_text ?? Text, decWidth: false);   // laid out and drawn again at the new size
						if (_colour.HasValue) Colour = _colour.Value;
					}
					catch (Exception) { }
				}
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

			public string Text
			{
				get => _text ?? Medget.node()?.getFirstNodeByTagName("data")?.nodeValueString() ?? "";
				set { _text = value ?? ""; Text_?.mbSetBufferMsg(_text.Length == 0 ? " " : _text, decWidth: false); if (_colour.HasValue) Colour = _colour.Value; }
			}

			public MenuColour Colour { set { _colour = value; try { Text_?.changeTextColor((GlobalScope.dgs.TXT_COLOR)(int)value); } catch (Exception) { } } }

			public bool Visible
			{
				get => _visible;
				set { _visible = value; Text_?.bmTextVisibility(value); }
			}
		}
	}
}
