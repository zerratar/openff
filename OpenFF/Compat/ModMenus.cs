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

		/// <summary>MenuDefine.xbn with the mods' screens in it; other files, or no screens, as they were.</summary>
		public static Array Patch(string fileName, Array bytes)
		{
			if (bytes == null || !string.Equals(Path.GetFileName(fileName), "MenuDefine.xbn", StringComparison.OrdinalIgnoreCase)) return bytes;
			// The definitions are read again each time: a menus/<id>.json edited while the client runs (an attachment, the main menu entry) is on the next opening of the menu, as the layouts are.
			if (_screen == null) Gather(OpenFF.Game.Mods);
			if (_all.Count == 0) return bytes;
			try
			{
				XDocument doc = MenuXbn.ToXml((byte[])bytes);
				XElement list = doc.Root;
				if (list == null) return bytes;
				int added = 0;
				foreach (MenuDefinition def in _all)
				{
					XElement menu = LoadLayoutMenu(def);
					if (menu == null) continue;
					list.Elements("menu").Where(m => (string)m.Element("name") == def.Screen).ToList().ForEach(m => m.Remove());
					list.Add(menu);
					added++;
				}
				AddMainMenuEntries(list);
				if (Options.Get("dump-menus") != null) { string at = Path.Combine(Path.GetTempPath(), "MenuDefine.patched.xml"); doc.Save(at); Log.Write(LogChannel.General, "menus: patched layout written to " + at); }
				byte[] patched = MenuXbn.FromXml(doc);
				Log.Write(LogChannel.General, "menus: MenuDefine.xbn carries " + added + " screen(s) of the mods' own" + (_all.Any(d => d.MainMenu != null) ? ", " + _all.Count(d => d.MainMenu != null) + " main menu entr" + (_all.Count(d => d.MainMenu != null) == 1 ? "y" : "ies") : ""));
				return patched;
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "menus: MenuDefine.xbn not patched: " + ex.Message);
				return bytes;
			}
		}

		/// <summary>The definition's layout file's &lt;menu&gt; whose name is the screen's (or the file's only one), renamed to the screen's name.</summary>
		private static XElement LoadLayoutMenu(MenuDefinition def)
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
					if (frame.Element("focus") == null) continue;
					frame.SetElementValue("myTag", tag++);
					foreach (string side in new[] { "up", "down", "left", "right" }) if (frame.Element(side) == null) frame.Add(new XElement(side, "dummy"));
				}
				return menu;
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "menus: " + def.Id + ": " + def.Layout + ": " + ex.Message); return null; }
		}

		/// <summary>An entry per screen that asks for one in main_menu's command list, after the entry it names; the list re-spaced to fit, the focus ring closed.</summary>
		private static void AddMainMenuEntries(XElement list)
		{
			List<MenuDefinition> entries = _all.Where(d => d.MainMenu != null && !string.IsNullOrWhiteSpace(d.MainMenu.Label)).ToList();
			if (entries.Count == 0) return;
			XElement main = list.Elements("menu").FirstOrDefault(m => (string)m.Element("name") == "main_menu");
			XElement commands = main?.Elements("frame").FirstOrDefault(f => (string)f.Element("id") == "main_command");
			if (commands == null) { Log.Write(LogChannel.General, "menus: main_menu has no main_command list; the mods' entries are not added"); return; }
			List<XElement> rows = commands.Elements("frame").ToList();
			if (rows.Count == 0) return;
			XElement template = rows.FirstOrDefault(r => (string)r.Element("id") == "com_job") ?? rows[0];
			int index = 0;
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
				XElement after = rows.FirstOrDefault(r => (string)r.Element("id") == def.MainMenu.After);
				if (after != null) after.AddAfterSelf(row); else rows[rows.Count - 1].AddAfterSelf(row);
				rows = commands.Elements("frame").ToList();
				index++;
			}
			// The character panels (p1..p4) and anything else numbered after the commands move down the
			// focus list by as many entries as went in: myTag is a focus index the game moves by.
			int original = rows.Count - index;
			foreach (XElement frame in main.Descendants("frame"))
			{
				if (frame.Parent == commands) continue;
				XElement tag = frame.Element("myTag");
				if (tag != null && int.TryParse(tag.Value, out int t) && t >= original) tag.Value = (t + index).ToString();
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

		/// <summary>The main menu's entry with work = KIND + index was chosen: that screen is current.</summary>
		public static bool Select(int work)
		{
			int index = work - GlobalScope.wmenu.CWMenuMod.KIND;
			List<MenuDefinition> entries = _all.Where(d => d.MainMenu != null && !string.IsNullOrWhiteSpace(d.MainMenu.Label)).ToList();
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
				_mods.TryGetValue(_current.Id, out LoadedMod mod);
				_behaviours = MenuLoader.Make(_current, _screen, mod);
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
				try { Hero = def.CharacterSelect ? GlobalScope.pl.PlayerParty.instance().player((byte)GlobalScope.menu.MenuManager.getSingleton().GetTargetCharNo()).playerId() : -1; }
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
			public void Close() => _host.Leave(toMainMenu: !_fromField);
			public void Open(string menuId) => ((ModMenus)OpenFF.Game.Menus).Open(menuId);
			public void SoundDecide() => GlobalScope.menu.MenuManager.getSingleton().playSEDecide();
			public void SoundBeep() => GlobalScope.menu.MenuManager.getSingleton().playSEBeep();
			public void SoundCancel() => GlobalScope.menu.MenuManager.getSingleton().playSECancel();
		}

		private sealed class ModMenuWidget : IMenuWidget
		{
			public readonly GlobalScope.menu.Medget Medget;
			public readonly List<ModMenuWidget> ChildList = new List<ModMenuWidget>();
			private string _text;
			private bool _visible = true;

			public ModMenuWidget(GlobalScope.menu.Medget m) { Medget = m; }

			private GlobalScope.menu.MBText Text_ => Medget.behavior()?.queryInterface(GlobalScope.menu.MBText.classIdentifier()) as GlobalScope.menu.MBText;

			public string Id => Medget._id();
			public int X => Medget.x();
			public int Y => Medget.y();
			public int Width => Medget.width();
			public int Height => Medget.height();
			public int Work => Medget.work() is IConvertible w ? Convert.ToInt32(w) : 0;
			public bool Focusable => Medget.node()?.getFirstNodeByTagNameFromChildren("focus") != null;
			public IReadOnlyList<IMenuWidget> Children => ChildList;

			public string Text
			{
				get => _text ?? Medget.node()?.getFirstNodeByTagName("data")?.nodeValueString() ?? "";
				set { _text = value ?? ""; Text_?.mbSetBufferMsg(_text.Length == 0 ? " " : _text, decWidth: false); }
			}

			public MenuColour Colour { set { try { Text_?.changeTextColor((GlobalScope.dgs.TXT_COLOR)(int)value); } catch (Exception) { } } }

			public bool Visible
			{
				get => _visible;
				set { _visible = value; Text_?.bmTextVisibility(value); }
			}
		}
	}
}
