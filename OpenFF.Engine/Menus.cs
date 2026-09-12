// Menu screens of a mod's own, in the game's own menu system.
//
// The game's menus are layouts (the XBN files Crystal's Menus tab edits: frames with a
// position, a size, focus neighbours and a widget behaviour such as Text) driven by a
// screen class each. A mod adds a screen the same way, without touching the engine:
//
//   menus/<id>.xml    the layout, one <menu> in the game's own XML form (Crystal draws and
//                     edits it as it does the game's); frames with <focus/> can be chosen
//   menus/<id>.json   the screen: which layout menu, how it opens, and MenuBehaviours
//                     attached to frames by id - the same shape as a scene's attachments
//
//   { "id": "abilities", "layout": "abilities.xml", "screen": "abilities",
//     "title": "Abilities", "mainMenu": { "label": "Abilities", "after": "com_job" },
//     "background": 10, "characterSelect": true,
//     "attachments": [
//       { "target": "", "behaviour": "AbilitiesScreen" },
//       { "target": "back", "behaviour": "Back" } ] }
//
// mainMenu puts an entry into the game's main menu (after the entry named, or last; or in
// place of one, "replaces": "com_job");
// the client merges the layout into MenuDefine.xbn as it loads, so the screen is built by
// name like the game's own, drawn with the game's windows, font and cursor, and moved
// through with the game's focus rules. A definition whose screen is one of the game's own
// ("screen": "status") reaches that screen instead: its behaviours hear the game's screen,
// its layout (if any) replaces the game's - or, with "patch": true, merges into it frame by
// id - in whichever of the eight layout files "file" names. characterSelect asks the player which hero first
// (as Status and Equip do); Menu.Hero says who. background is one of the game's menu
// backdrops (10 the plain one, the default; 3 Status's, 5 Job's, 9 the main menu's...).
//
// A MenuBehaviour is the unit of script on a screen: attached to a frame it hears that
// frame's events (OnFocus, OnPress); attached to the screen itself (target "") it hears
// every event, with Menu.Focused saying which frame. The engine's own - Back, OpenMenu,
// Label - cover the plain cases with no code; a mod's own subclass fills texts from
// Game.Party and acts on presses. Game.Menus.Open("abilities") opens a screen from
// anywhere; Menu.Open / Menu.Close move between screens.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace OpenFF
{
	/// <summary>A screen of a mod's own: menus/&lt;id&gt;.json.</summary>
	public sealed class MenuDefinition
	{
		public string Id { get; set; }
		/// <summary>The layout file beside the definition (menus/&lt;file&gt;.xml), one &lt;menu&gt; in the game's XML form. For one of the game's own screens it may be left out: the definition then only attaches behaviours to the screen as it is.</summary>
		public string Layout { get; set; }
		/// <summary>The game's layout file the screen lives in: MenuDefine.xbn (the default), ShopDefine.xbn, BattleDefine.xbn, WorldDefine.xbn, SpecialDefine.xbn, MogNet.xbn, ChocoboBank.xbn, NameEntry.xbn.</summary>
		public string File { get; set; } = "MenuDefine.xbn";
		/// <summary>For one of the game's screens with a layout of the mod's: merge - a frame with an id the game's screen has takes that frame's place, a new id is added - rather than replacing the whole screen.</summary>
		public bool Patch { get; set; }
		/// <summary>The &lt;name&gt; of the menu in the layout; the id when unsaid. Mods' names should not collide with the game's (main_menu, status, job...).</summary>
		public string Screen { get; set; }
		public string Title { get; set; }
		/// <summary>An entry in the game's main menu that opens this screen; null for none.</summary>
		public MenuEntry MainMenu { get; set; }
		/// <summary>The game's menu backdrop to draw behind: 10 the plain one (the default), 0 Item's, 1 Magic's, 2 Equipment's, 3 Status's, 5 Job's, 6 Config's, 7 Quicksave's, 8 Save's, 9 the main menu's, 13 and 14 the tips pages'.</summary>
		public int Background { get; set; } = 10;
		/// <summary>Ask which hero first, as Status and Equip do; Menu.Hero says who.</summary>
		public bool CharacterSelect { get; set; }
		public List<MenuAttachment> Attachments { get; set; } = new List<MenuAttachment>();
		/// <summary>The mod that defined it; set by the loader.</summary>
		public string ModId { get; set; }
		/// <summary>The folder the definition came from; set by the loader.</summary>
		public string Directory { get; set; }
		public string LayoutPath => Directory != null && Layout != null ? Path.Combine(Directory, Layout) : null;
	}

	public sealed class MenuEntry
	{
		/// <summary>The text of the entry.</summary>
		public string Label { get; set; }
		/// <summary>The id of the game's entry to follow: com_item, com_magic, com_equip, com_status, com_tairetu (Formation), com_job, com_config, com_half (Quicksave), com_save - or another mod screen's id; last when unsaid.</summary>
		public string After { get; set; }
		/// <summary>The id of the game's entry this one stands in place of (com_job for a mod whose heroes all grow FF5's way and have no use for the game's Job screen); the game's row goes, this one takes its place.</summary>
		public string Replaces { get; set; }
	}

	/// <summary>A MenuBehaviour on a frame of the screen ("" or null: the screen itself), with its fields.</summary>
	public sealed class MenuAttachment
	{
		public string Target { get; set; }
		public string Behaviour { get; set; }
		public Dictionary<string, JsonElement> Fields { get; set; }
	}

	/// <summary>A key the menu hears beyond confirm and cancel.</summary>
	public enum MenuKey { Up, Down, Left, Right, L, R, X, Y }

	/// <summary>The game's text colours (dgs.TXT_COLOR): White is the plain text, Disabled the grey of a command that cannot be taken, the rest as named.</summary>
	public enum MenuColour { White = 1, Black = 2, Red = 3, Green = 4, Blue = 5, Cyan = 6, Magenta = 7, Yellow = 8, PaleYellow = 9, PaleBlue = 10, PaleRed = 11, Disabled = 13 }

	/// <summary>A frame of an open screen.</summary>
	public interface IMenuWidget
	{
		string Id { get; }
		int X { get; }
		int Y { get; }
		int Width { get; }
		int Height { get; }
		/// <summary>The frame's text (a Text behaviour's); setting it replaces what the layout said.</summary>
		string Text { get; set; }
		/// <summary>The text's colour, from the game's own set.</summary>
		MenuColour Colour { set; }
		/// <summary>Whether the text is drawn.</summary>
		bool Visible { get; set; }
		/// <summary>Whether the frame is in the focus list (the cursor can land on it).</summary>
		bool Focusable { get; }
		/// <summary>The layout's work value of the frame - a number of the mod's own to tell rows apart.</summary>
		int Work { get; }
		/// <summary>The frame's rectangle in Game.Draw's screen units (800 x 480), for drawing pictures and shapes on it.</summary>
		(float X, float Y, float Width, float Height) ScreenRect { get; }
		IReadOnlyList<IMenuWidget> Children { get; }
	}

	/// <summary>An open screen: its frames, focus, and the way out.</summary>
	public interface IMenuScreen
	{
		MenuDefinition Definition { get; }
		string Id { get; }
		/// <summary>A frame by id, anywhere in the layout; null for none.</summary>
		IMenuWidget Widget(string id);
		/// <summary>Every frame of the layout, in layout order.</summary>
		IReadOnlyList<IMenuWidget> Widgets { get; }
		/// <summary>The id of the frame the cursor is on, or null.</summary>
		string Focused { get; }
		/// <summary>Puts the cursor on a frame (one that has &lt;focus/&gt;).</summary>
		void Focus(string id);
		/// <summary>Sets a frame's text (a Text behaviour's).</summary>
		void SetText(string id, string text);
		/// <summary>The hero picked when the screen asked for one (Definition.CharacterSelect), by the game's id; -1 otherwise.</summary>
		int Hero { get; }
		/// <summary>Leaves the screen: back to the main menu when it was opened from there, else out of the menus.</summary>
		void Close();
		/// <summary>Leaves for another screen of a mod's own.</summary>
		void Open(string menuId);
		/// <summary>The game's confirm sound.</summary>
		void SoundDecide();
		/// <summary>The game's refusal beep.</summary>
		void SoundBeep();
		/// <summary>The game's cancel sound.</summary>
		void SoundCancel();
	}

	/// <summary>
	/// The unit of script on a mod's menu screen. Attached to a frame (an attachment's target) it
	/// hears the frame's events; attached to the screen (target "") it hears them all. Public fields
	/// are its settings in Crystal, as a Behaviour's are.
	/// </summary>
	public abstract class MenuBehaviour
	{
		/// <summary>The open screen.</summary>
		public IMenuScreen Menu { get; internal set; }
		/// <summary>The frame this is on; null for one on the screen itself.</summary>
		public IMenuWidget Widget { get; internal set; }
		/// <summary>The frame's id, or "" on the screen.</summary>
		public string Target => Widget?.Id ?? "";
		/// <summary>The folder of the mod that defined the screen (for files of the mod's own); null for a screen with no mod folder.</summary>
		public string ModDirectory { get; internal set; }

		/// <summary>The screen has been built and is about to show: fill its texts.</summary>
		public virtual void OnOpen() { }
		/// <summary>The screen is going away.</summary>
		public virtual void OnClose() { }
		/// <summary>Every frame while the screen is up.</summary>
		public virtual void OnTick() { }
		/// <summary>The cursor landed on the frame (on the screen: on any frame; Menu.Focused says which).</summary>
		public virtual void OnFocus() { }
		/// <summary>The cursor left the frame.</summary>
		public virtual void OnBlur() { }
		/// <summary>Confirm on the frame. Return true when handled, so the screen's own behaviours are not asked too.</summary>
		public virtual bool OnPress() => false;
		/// <summary>Cancel anywhere on the screen. Return true when handled; otherwise the screen closes.</summary>
		public virtual bool OnCancel() => false;
		/// <summary>A key beyond confirm and cancel. Return true when handled.</summary>
		public virtual bool OnKey(MenuKey key) => false;

		/// <summary>The type's name and the frame it is on, for the log.</summary>
		public string Name => GetType().Name + (Widget != null ? "@" + Widget.Id : "");
	}

	/// <summary>Confirm on the frame leaves the screen.</summary>
	public sealed class Back : MenuBehaviour
	{
		public override bool OnPress() { Menu.SoundCancel(); Menu.Close(); return true; }
	}

	/// <summary>Confirm on the frame opens another screen of the mod's.</summary>
	public sealed class OpenMenu : MenuBehaviour
	{
		[Tooltip("The id of the screen to open (menus/<id>.json)")]
		public string Screen = "";
		public override bool OnPress()
		{
			if (string.IsNullOrWhiteSpace(Screen)) return false;
			Menu.SoundDecide();
			Menu.Open(Screen.Trim());
			return true;
		}
	}

	/// <summary>The frame's text, set as the screen opens - a way to write a label without editing the layout's message.</summary>
	public sealed class Label : MenuBehaviour
	{
		public string Text = "";
		public override void OnOpen() { if (Widget != null) Widget.Text = Text; }
	}

	/// <summary>A picture drawn over the frame's rectangle: a PNG of the mod's own (a path under the mod's folder) or one of the game's 2D sheets by name (m000_window.NCBR, a .NCGR - the Steam build's are PNGs).</summary>
	public sealed class Picture : MenuBehaviour
	{
		[Tooltip("A PNG under the mod's folder (pictures/banner.png), or one of the game's sheets by its name (menu_bg_01.NCGR)")]
		public string Path = "";
		[Tooltip("Stretch to the frame; otherwise drawn at the picture's own size from the frame's corner")]
		public bool Stretch = true;
		private Texture _texture;
		private string _loaded;

		public override void OnTick()
		{
			if (Widget == null || string.IsNullOrWhiteSpace(Path)) return;
			if (_loaded != Path) { _loaded = Path; _texture = MenuLoader.LoadPicture(Path, ModDirectory); }
			if (_texture == null) return;
			var r = Widget.ScreenRect;
			if (Stretch) Game.Draw.Sprite(_texture, r.X, r.Y, r.Width, r.Height);
			else Game.Draw.Sprite(_texture, r.X, r.Y, _texture.Width * r.Width / Math.Max(1, Widget.Width), _texture.Height * r.Height / Math.Max(1, Widget.Height));   // the picture's own size, in the frame's scale
		}
	}

	/// <summary>The mods' menu screens: opening one, what is open.</summary>
	public interface IMenus
	{
		/// <summary>Every screen the loaded mods define.</summary>
		IReadOnlyList<MenuDefinition> All { get; }
		/// <summary>A definition by id, or null.</summary>
		MenuDefinition Find(string id);
		/// <summary>Opens a screen from anywhere on the field (the game's menus open on it); false when no such screen or the field is not up.</summary>
		bool Open(string id);
		/// <summary>The screen up now, or null.</summary>
		IMenuScreen Current { get; }
	}

	public static partial class Game
	{
		/// <summary>Menu screens of the mods' own (menus/&lt;id&gt;.json), in the game's own menu system.</summary>
		public static IMenus Menus => Services.Get<IMenus>();
	}

	/// <summary>Reads the mods' menu definitions and makes their behaviours; the host drives the screens.</summary>
	public static class MenuLoader
	{
		/// <summary>Host entry: one of the game's files by name (a 2D sheet for Picture), or null.</summary>
		public static Func<string, byte[]> GameFile { get; set; }

		/// <summary>A picture for a Picture behaviour: a file under the mod's folder, an absolute path, or one of the game's sheets by name.</summary>
		public static Texture LoadPicture(string path, string modDirectory)
		{
			if (string.IsNullOrWhiteSpace(path)) return null;
			string p = path.Trim();
			string full = System.IO.Path.IsPathRooted(p) ? p : modDirectory != null ? System.IO.Path.Combine(modDirectory, p) : null;
			if (full != null && File.Exists(full)) return Game.Draw.LoadTexture(full);
			byte[] bytes = null;
			try { bytes = GameFile?.Invoke(p); } catch (Exception) { }
			if (bytes != null && bytes.Length > 8 && bytes[1] == (byte)'P' && bytes[2] == (byte)'N' && bytes[3] == (byte)'G') return Game.Draw.LoadTexture("game:" + p, bytes);
			Game.Warn("Picture: " + p + " is not a picture the mod has or the game has as a PNG");
			return null;
		}
		/// <summary>The definitions under a mod's menus folder, in file order; faults are warned and skipped.</summary>
		public static List<MenuDefinition> Read(string modId, string directory)
		{
			List<MenuDefinition> list = new List<MenuDefinition>();
			if (string.IsNullOrEmpty(directory) || !System.IO.Directory.Exists(directory)) return list;
			foreach (string file in System.IO.Directory.EnumerateFiles(directory, "*.json").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
			{
				try
				{
					MenuDefinition def = JsonSerializer.Deserialize<MenuDefinition>(File.ReadAllText(file), new JsonSerializerOptions { PropertyNameCaseInsensitive = true, ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
					if (def == null) continue;
					if (string.IsNullOrWhiteSpace(def.Id)) def.Id = Path.GetFileNameWithoutExtension(file);
					if (string.IsNullOrWhiteSpace(def.Layout) && File.Exists(Path.Combine(directory, def.Id + ".xml"))) def.Layout = def.Id + ".xml";
					if (string.IsNullOrWhiteSpace(def.Screen)) def.Screen = def.Id;
					def.ModId = modId;
					def.Directory = directory;
					if (def.Layout != null && !File.Exists(def.LayoutPath)) { Game.Warn("mod " + modId + ": menus/" + Path.GetFileName(file) + " names layout " + def.Layout + ", which is not there"); continue; }
					if (string.IsNullOrWhiteSpace(def.File)) def.File = "MenuDefine.xbn";
					def.Attachments ??= new List<MenuAttachment>();
					list.Add(def);
				}
				catch (Exception ex) { Game.Warn("mod " + modId + ": menus/" + Path.GetFileName(file) + ": " + ex.Message); }
			}
			return list;
		}

		/// <summary>The engine's own MenuBehaviours by name (Back, OpenMenu, Label).</summary>
		public static Type EngineBehaviour(string name)
		{
			return typeof(MenuBehaviour).Assembly.GetTypes()
				.FirstOrDefault(t => typeof(MenuBehaviour).IsAssignableFrom(t) && !t.IsAbstract && t.IsPublic && string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>The behaviours of a definition, made and bound to the screen: the mod's types first, then the engine's.</summary>
		public static List<MenuBehaviour> Make(MenuDefinition def, IMenuScreen screen, Modding.LoadedMod mod)
		{
			List<MenuBehaviour> made = new List<MenuBehaviour>();
			foreach (MenuAttachment a in def.Attachments)
			{
				if (a == null || string.IsNullOrWhiteSpace(a.Behaviour)) continue;
				Type type = null;
				if (mod != null) mod.MenuBehaviourTypes.TryGetValue(a.Behaviour, out type);
				type ??= EngineBehaviour(a.Behaviour);
				if (type == null) { Game.Warn("mod " + def.ModId + ": menus/" + def.Id + ".json names " + a.Behaviour + ", which neither the mod's code nor the engine has"); continue; }
				IMenuWidget widget = string.IsNullOrEmpty(a.Target) ? null : screen.Widget(a.Target);
				if (!string.IsNullOrEmpty(a.Target) && widget == null) { Game.Warn("mod " + def.ModId + ": menus/" + def.Id + ".json puts " + a.Behaviour + " on frame '" + a.Target + "', which the layout has not"); continue; }
				MenuBehaviour behaviour = null;
				Game.Guard("new " + type.Name, () => behaviour = (MenuBehaviour)Activator.CreateInstance(type));
				if (behaviour == null) continue;
				behaviour.Menu = screen;
				behaviour.Widget = widget;
				behaviour.ModDirectory = mod?.Directory ?? (def.Directory != null ? System.IO.Path.GetDirectoryName(def.Directory) : null);
				SceneLoader.SetFields(behaviour, a.Fields, def.ModId);
				made.Add(behaviour);
			}
			return made;
		}
	}
}
