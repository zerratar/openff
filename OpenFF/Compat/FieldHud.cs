// The field's HUD - field_hud, the client's screen in WorldDefine.xbn (BattleHudLayout) - as a styled
// screen: the dialogue window and the map-name banner in the layout's places, with its looks
// (Crystal Style Sheets: MenuStyles) and frames of the mod's own inside them.
//
//   dialogue            the game's message window: its panel (-ff-panel: none takes the game's art away; a
//                       background, a painted box, a picture in its place), opacity and tint
//     text              where the text starts; its colour, size, lettering and opacity
//     name              the speaker's name (FF4's)
//     next              the page-turn arrow: hidden hides it, -ff-tint and opacity colour it; a background of its
//                       own (a picture, a painted box) takes its place, up when the game's would be
//   map_name            the banner a map's name comes up in; its text takes its look
//   confirm             the Yes / No box (the engine API's Ask): question, yes, no - the one the hand is on
//                       in :focus
//   menu_button, map_button, talk_button   the field's buttons, in the places the options put them (the
//                       menu's and the map's swap): hidden, -ff-tint, opacity; a background of its own in
//                       place of the game's art; frames of the mod's in them, shown and faded with them
//
// translate moves what a frame draws - its panel, its window, its texts, and its frames' with it - from
// where its layout put it (a transition's, an animation's: an arrow that bobs, a box that slides in).
//     <any other>       a frame of the mod's: a window, a background, a text (<data>, bind-text) - shown
//                       with the window, as its data says: {dialogue.speaker}, {dialogue.avatar}, ...
//
// Bindings (bind-text, bind-visible, bind-class, bind-style) read dialogue (Number, Text, Speaker,
// Avatar, Map), banner (Number, Text), hero, party and gil. Who speaks comes from the mods'
// speakers.json (a message's number to a speaker's name and picture) and the DialogueShown event:
//
//   { "speakers": { "elder": { "name": "Elder", "avatar": "images/elder.png" } },
//     "messages": { "1000142": "elder", "1000150-1000160": "elder" } }
//
// Game.Dialogue.Say("...", "elder") names a speaker too: a table's by that id or name, or the name as said.
//
// A picture is a file beside the table, or "resource:files/pc1_01.NCGR" for one of the game's; the
// binding gets it as a background image says it ({dialogue.avatar} is url("...") or resource("...")):
//
//   <frame bind-visible="dialogue.avatar" bind-style="background-image: {dialogue.avatar}; -ff-background-scale-mode: scale-to-fit">
//
// The layout is kept as the client built it (Capture, as WorldDefine.xbn loads), so the frames are
// the layout's while another file is loaded too (a battle's, the menus'). With nothing styled, the
// game's windows are drawn as they always were.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;
using OpenFF.Content;
using OpenFF.Modding;

namespace OpenFF.Client
{
	internal static class FieldHud
	{
		// ---- the layout ----

		private static XElement _source;                       // the screen cascaded: a mod's layout as written, else as built
		private static MenuStyles.Sheet _sheet = MenuStyles.Compile(null);
		private static string _directory;                      // the mod layout's folder: url() pictures and faces
		private static readonly Dictionary<string, BattleHud.Rect> _rects = new Dictionary<string, BattleHud.Rect>(StringComparer.OrdinalIgnoreCase);
		private static readonly Dictionary<XElement, string> _pathOf = new Dictionary<XElement, string>();
		private static readonly Dictionary<XElement, string> _ownStyle = new Dictionary<XElement, string>();
		// Each frame's translate as the layout was built with it: a translate moves what it draws by the difference.
		private static readonly Dictionary<XElement, string> _baseTranslate = new Dictionary<XElement, string>();
		private static MenuAnimation.Animator _animator = new MenuAnimation.Animator();
		private static readonly System.Diagnostics.Stopwatch _clock = System.Diagnostics.Stopwatch.StartNew();

		/// <summary>
		/// The field_hud screen as the client built it (WorldDefine.xbn, patched: its frames' places), and - when a mod's layout is
		/// it - that layout as written and its sheets, for the looks. Called as the file loads; kept until it loads again.
		/// </summary>
		public static void Capture(byte[] built, XElement written, IEnumerable<string> sheets, string directory)
		{
			try
			{
				XElement menu = MenuXbn.ToXml(built).Root?.Elements("menu").FirstOrDefault(m => (string)m.Element("name") == BattleHudLayout.FieldScreen);
				if (menu == null) return;
				_rects.Clear();
				foreach (XElement frame in menu.Elements("frame")) Walk(frame, null, 0, 0, (f, path, rect) => _rects[path] = rect);
				_source = written != null ? new XElement(written) : new XElement(menu);
				_sheet = MenuStyles.Compile(sheets ?? Enumerable.Empty<string>());
				_directory = written != null ? directory : null;
				_pathOf.Clear();
				_ownStyle.Clear();
				foreach (XElement frame in _source.Elements("frame")) Walk(frame, null, 0, 0, (f, path, rect) => { _pathOf[f] = path; _ownStyle[f] = (string)f.Attribute("style"); });
				_animator = new MenuAnimation.Animator();
				_baseTranslate.Clear();
				foreach ((XElement f, Dictionary<string, string> c) in MenuStyles.Computed(_source, _sheet)) _baseTranslate[f] = c.TryGetValue("translate", out string t) ? t : null;
				// What was up under the last layout goes; the buttons are made again over this one.
				Close(ref _dialogue);
				Close(ref _banner);
				Close(ref _confirm);
				foreach (int cell in _buttons.Keys.ToList()) { Shown b = _buttons[cell]; Close(ref b); }
				CloseOverlay();
				_buttons.Clear();
				foreach (int cell in _buttonSprites.Keys.ToList()) MakeButton(cell);
				_laid.Clear();
				_laidKey = null;
				_flows = MenuStyles.Computed(_source, _sheet).Any(c => c.Computed.TryGetValue("flex-direction", out string d) && (d.Trim() == "row" || d.Trim() == "column"));
				_version++;
				_titleLooks.Clear();
				Captured = true;
				LoadSpeakers();
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "field hud: " + ex.Message); }
		}

		private static void Walk(XElement frame, string parent, int px, int py, Action<XElement, string, BattleHud.Rect> each)
		{
			string id = ((string)frame.Element("id"))?.Trim();
			if (string.IsNullOrEmpty(id)) return;
			int x = px + Int(frame, "x"), y = py + Int(frame, "y");
			string path = parent == null ? id : parent + "/" + id;
			each(frame, path, new BattleHud.Rect { X = x, Y = y, Width = Int(frame, "width"), Height = Int(frame, "height"), Found = true });
			foreach (XElement child in frame.Elements("frame")) Walk(child, path, x, y, each);
		}

		private static int Int(XElement frame, string tag) => int.TryParse(((string)frame.Element(tag))?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : 0;

		// ---- the layout laid out again as the data says (a frame out of its row or column: display: none, bind-display) ----

		// The game places its texts, the window and the arrow by the layout as it was built (_rects: what BattleHud reads);
		// the HUD draws its panels and texts by the layout laid out again (_laid), and moves the game's by the difference.
		private static readonly Dictionary<string, BattleHud.Rect> _laid = new Dictionary<string, BattleHud.Rect>(StringComparer.OrdinalIgnoreCase);
		private static string _laidKey;
		private static bool _flows;   // whether any frame lays its children out in a row or a column

		/// <summary>A frame's box as the layout is laid out now (as built when nothing moved it).</summary>
		private static bool Laid(string path, out BattleHud.Rect rect) => _laid.TryGetValue(path ?? "", out rect) || _rects.TryGetValue(path ?? "", out rect);

		/// <summary>How far the layout laid out again moved a frame from where it was built (where the game puts what it draws there).</summary>
		private static (int X, int Y) LaidShift(string path) => path != null && _laid.TryGetValue(path, out BattleHud.Rect now) && _rects.TryGetValue(path, out BattleHud.Rect built) ? (now.X - built.X, now.Y - built.Y) : (0, 0);

		/// <summary>The rows and columns laid out again with the frames' classes, states and bound styles as they are now, when those changed.</summary>
		private static void Reflow()
		{
			if (!_flows || _source == null) return;
			string key = string.Join("|", _source.Descendants("frame").Select(f => (string)f.Attribute("class") + "~" + (string)f.Attribute("style") + "~" + (string)f.Attribute(MenuStyles.StateAttribute)));
			if (key == _laidKey) return;
			_laidKey = key;
			try
			{
				XElement copy = new XElement(_source);
				MenuStyles.Apply(copy, _sheet);
				MenuLayout.Bake(copy);
				bool changed = false;
				foreach (XElement frame in copy.Elements("frame"))
				{
					Walk(frame, null, 0, 0, (f, path, rect) =>
					{
						if (!Laid(path, out BattleHud.Rect was) || !SameRect(was, rect)) changed = true;
						_laid[path] = rect;
					});
				}
				if (changed) _version++;
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "field hud: laid out again: " + ex.Message); }
		}

		private static bool SameRect(BattleHud.Rect a, BattleHud.Rect b) => a.X == b.X && a.Y == b.Y && a.Width == b.Width && a.Height == b.Height;

		/// <summary>Whether field_hud has been read (WorldDefine.xbn loaded, or read ahead: ModMenus.PrepareFieldHud).</summary>
		public static bool Captured { get; private set; }

		/// <summary>A frame of field_hud by its path ("dialogue/text"), absolute, as the layout last built had it.</summary>
		public static bool TryRect(string path, out BattleHud.Rect rect) => _rects.TryGetValue(path ?? "", out rect);

		// ---- who speaks ----

		/// <summary>What the dialogue shows, for the bindings: {dialogue.speaker}, {dialogue.avatar}...</summary>
		public sealed class DialogueData
		{
			public int Number { get; set; } = -1;
			public string Text { get; set; }
			public string Speaker { get; set; }
			public string Avatar { get; set; }
			public string Map { get; set; }
		}

		/// <summary>What the map-name banner shows: {banner.text}.</summary>
		public sealed class BannerData
		{
			public int Number { get; set; } = -1;
			public string Text { get; set; }
		}

		public static DialogueData Dialogue { get; } = new DialogueData();

		/// <summary>The speaker Game.Dialogue.Say named, for the texts it shows (over a table's; a DialogueShown handler is over it).</summary>
		public static string SaidSpeaker;
		public static BannerData Banner { get; } = new BannerData();

		private static readonly Dictionary<int, (string Name, string Avatar)> _speakers = new Dictionary<int, (string, string)>();
		// The tables' speakers by their id and by their name: the one Game.Dialogue.Say names.
		private static readonly Dictionary<string, (string Name, string Avatar)> _speakersByName = new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase);

		/// <summary>The mods' speakers.json (in their menus folders, then the client's Data/menus): the first to say a message's speaker is heard.</summary>
		private static void LoadSpeakers()
		{
			_speakers.Clear();
			_speakersByName.Clear();
			List<string> folders = new List<string>();
			try { foreach (LoadedMod mod in OpenFF.Game.Mods) if (!string.IsNullOrEmpty(mod.Definition?.Menus)) folders.Add(mod.Definition.Menus); } catch (Exception) { }
			folders.Add(Path.Combine(AppContext.BaseDirectory, "Data", "menus"));
			foreach (string folder in folders)
			{
				string file = Path.Combine(folder, "speakers.json");
				if (!File.Exists(file)) continue;
				try
				{
					using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(file), new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
					Dictionary<string, (string, string)> who = new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase);
					if (doc.RootElement.TryGetProperty("speakers", out JsonElement speakers))
					{
						foreach (JsonProperty s in speakers.EnumerateObject())
						{
							string name = s.Value.TryGetProperty("name", out JsonElement n) ? n.GetString() : null;
							string avatar = s.Value.TryGetProperty("avatar", out JsonElement a) ? Picture(a.GetString(), folder) : null;
							who[s.Name] = (name, avatar);
							if (!_speakersByName.ContainsKey(s.Name)) _speakersByName[s.Name] = (name ?? s.Name, avatar);
							if (!string.IsNullOrWhiteSpace(name) && !_speakersByName.ContainsKey(name)) _speakersByName[name] = (name, avatar);
						}
					}
					int count = 0;
					if (doc.RootElement.TryGetProperty("messages", out JsonElement messages))
					{
						foreach (JsonProperty m in messages.EnumerateObject())
						{
							string key = m.Value.GetString();
							if (key == null || !who.TryGetValue(key, out (string, string) speaker)) continue;
							string[] range = m.Name.Split('-');
							if (!int.TryParse(range[0].Trim(), out int from)) continue;
							int to = range.Length > 1 && int.TryParse(range[1].Trim(), out int t) ? t : from;
							for (int i = from; i <= to && i - from < 100000; i++) if (!_speakers.ContainsKey(i)) { _speakers[i] = speaker; count++; }
						}
					}
					Log.Write(LogChannel.File, "field hud: " + file + " - " + who.Count + " speaker(s), " + count + " message(s)");
				}
				catch (Exception ex) { Log.Write(LogChannel.General, "field hud: " + file + ": " + ex.Message); }
			}
		}

		/// <summary>A picture as a background image says it: a file beside the table (url), or one of the game's (resource:...).</summary>
		private static string Picture(string value, string folder)
		{
			if (string.IsNullOrWhiteSpace(value)) return null;
			string v = value.Trim();
			if (v.StartsWith("url(", StringComparison.OrdinalIgnoreCase) || v.StartsWith("resource(", StringComparison.OrdinalIgnoreCase)) return v;
			if (v.StartsWith("resource:", StringComparison.OrdinalIgnoreCase)) return "resource(\"" + v.Substring(9).Trim() + "\")";
			return "url(\"" + Path.GetFullPath(Path.Combine(folder, v)).Replace('\\', '/') + "\")";
		}

		// ---- what is up: the dialogue, the banner, the Yes / No box, the field's buttons ----

		/// <summary>One of the HUD's things up: its frame, the game's own window, sprite and texts in it, and what the layout adds.</summary>
		private sealed class Shown
		{
			public string Root;
			/// <summary>The frames that are the game's (its window and texts); the rest are the layout's own.</summary>
			public readonly HashSet<string> Game = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			public GlobalScope.menu.BasicWindow Window;
			public (int X, int Y) WindowPlace;
			public (int X, int Y) WindowOffset;
			/// <summary>A button's sprite, and where the game put it.</summary>
			public GlobalScope.sys2d.Sprite Sprite;
			public (int X, int Y) SpritePlace;
			public Func<bool> Open = () => true;
			public bool Built;
			/// <summary>A button's own showing and fade (its alpha of 31), and where the game put it against the layout's place (the options swap the menu's and the map's).</summary>
			public bool Visible = true;
			public float Fade = 1f;
			public (int X, int Y) Shift;
			/// <summary>How far what is under the question moved up (a box opened without its question line: the confirm's), the box that much shorter.</summary>
			public int Lift;
			/// <summary>The game's texts, by the frame whose look each takes, and where the game put each.</summary>
			public readonly Dictionary<string, GlobalScope.dgs.DGSMessage> Texts = new Dictionary<string, GlobalScope.dgs.DGSMessage>(StringComparer.OrdinalIgnoreCase);
			public readonly Dictionary<GlobalScope.dgs.DGSMessage, (short X, short Y)> TextPlace = new Dictionary<GlobalScope.dgs.DGSMessage, (short, short)>();
			public readonly MenuPanels Panels = new MenuPanels();
			public readonly Dictionary<XElement, Built> Frames = new Dictionary<XElement, Built>();
			public readonly List<Extra> Extras = new List<Extra>();
			public Dictionary<XElement, MenuStyles.Look> Looks = new Dictionary<XElement, MenuStyles.Look>();

			public void Text(string path, GlobalScope.dgs.DGSMessage message)
			{
				if (message == null) return;
				Texts[path] = message;
				TextPlace[message] = (message.positionX(), message.positionY());
			}
		}

		/// <summary>What a frame's panel was made of, and the look, fade, place and showing it was put on with: a fade or a move put on in place, anything else made again.</summary>
		private sealed class Built
		{
			public MenuStyles.Look Look;
			public float Fade = 1f;
			public (int X, int Y) Offset;
			public bool Visible = true;
			public GlobalScope.menu.BasicWindow Window;
			public (int X, int Y) WindowPlace;
			public BattleHud.Rect Rect;
			public readonly List<GlobalScope.MenuPanelSprite> Sprites = new List<GlobalScope.MenuPanelSprite>();
			public readonly List<uint> Textures = new List<uint>();
		}

		/// <summary>Whether two looks make the same box but for its opacity and translate (a fade, a move needs nothing made again).</summary>
		private static bool SameBox(MenuStyles.Look a, MenuStyles.Look b) => a != null && b != null && a.Window == b.Window && a.Bar == b.Bar && a.NoPanel == b.NoPanel
			&& a.Tint == b.Tint && a.Background == b.Background && a.Hidden == b.Hidden;

		/// <summary>A frame of the layout's own in a thing of the HUD's: its text as a message of its own.</summary>
		private sealed class Extra
		{
			public XElement Frame;
			public string Path;
			public int Message = -1;
			public string Text;
			// Made by the menus' message manager (a menu button's text, drawn over the menu's panels), not the field's.
			public bool Menu;
		}

		private static Shown _dialogue, _banner, _confirm;
		// The field's buttons by their cell (MENU_PANEL_ANIM: 0 the menu's, 13 the map's, 14 talk's), their sprites kept for a layout loaded again.
		private static readonly Dictionary<int, Shown> _buttons = new Dictionary<int, Shown>();
		private static readonly Dictionary<int, (GlobalScope.sys2d.Sprite Sprite, int X, int Y)> _buttonSprites = new Dictionary<int, (GlobalScope.sys2d.Sprite, int, int)>();
		private static readonly Dictionary<int, byte> _buttonAlpha = new Dictionary<int, byte>();
		// The menus' buttons (CWMenuButton) among them as 100 + A 0, B 1, L 2, R 3: shown as the game says (their sprites are taken
		// away under a picture of the layout's), their labels.
		private const int MenuButtonKey = 100;
		private static readonly Dictionary<int, bool> _buttonShown = new Dictionary<int, bool>();
		private static readonly Dictionary<int, GlobalScope.dgs.DGSMessage> _buttonTexts = new Dictionary<int, GlobalScope.dgs.DGSMessage>();

		/// <summary>Whether field_hud's layout says anything about a look (else the game's things are left to themselves).</summary>
		private static bool Styled => _source != null && (MenuStyles.Has(_source) || _sheet.Rules.Count > 0 || _source.Descendants().Any(e => e.Name.LocalName is "background" or "textstyle" or "panel" or "tint" or "opacity" or "animation" or "colour" or "hidden"));

		private static Shown New(string root, GlobalScope.menu.BasicWindow window, Func<bool> open, params string[] game)
		{
			Shown s = new Shown { Root = root, Window = window, Open = open ?? (() => true) };
			s.Game.Add(root);
			foreach (string g in game) s.Game.Add(g);
			if (_rects.TryGetValue(root, out BattleHud.Rect r)) s.WindowPlace = (r.X, r.Y);
			return s;
		}

		/// <summary>The dialogue window was made (MessageWindow.mwSetWindow): the game's art taken away if the layout says none, the rest once it is open.</summary>
		public static void DialogueMade(GlobalScope.menu.BasicWindow window, Func<bool> open, GlobalScope.sys2d.Sprite3d next = null)
		{
			Close(ref _dialogue);
			_nextIcon = next;
			_nextWanted = false;
			if (!Styled) return;
			_dialogue = New("dialogue", window, open, "dialogue/text", "dialogue/name", "dialogue/next");
			PutWindowLook(_dialogue);
		}

		/// <summary>A text went into the dialogue (mwSetMessage / mwSetMessageText): who says it, and its look.</summary>
		public static void DialogueText(GlobalScope.dgs.DGSMessage message, int number, string text)
		{
			if (!(_spoken && number < 0)) Speak(number, text);
			_spoken = false;
			if (_dialogue == null) return;
			_dialogue.Text("dialogue/text", message);
			Refresh(_dialogue);
		}

		// Whether Wrap has already said who speaks the text coming (a Say's, broken into lines by the width that leaves).
		private static bool _spoken;

		/// <summary>Who says a text and what it is (the bindings' dialogue, DialogueShown), the frames bound to it and laid out again.</summary>
		private static void Speak(int number, string text)
		{
			Dialogue.Number = number;
			if (text == null && number >= 0)
			{
				try { text = GlobalScope.dgs.msg.CMessageSys.getInstance().Main().getMessage((uint)number); } catch (Exception) { }
			}
			Dialogue.Text = text;
			try { Dialogue.Map = GlobalScope.stg.CStageMng.CurrentName; } catch (Exception) { }
			(string name, string avatar) = number >= 0 && _speakers.TryGetValue(number, out (string, string) s) ? s : (null, null);
			// The speaker Say named: a table's by that id or name (its name and picture), else the name as said.
			if (number < 0 && SaidSpeaker != null) (name, avatar) = _speakersByName.TryGetValue(SaidSpeaker, out (string, string) named) ? named : (SaidSpeaker, null);
			OpenFF.Events.DialogueShown shown = new OpenFF.Events.DialogueShown { Number = number, Text = text, Map = Dialogue.Map, Speaker = name, Avatar = avatar };
			OpenFF.Game.Guard("DialogueShown", () => OpenFF.Game.Events.Publish(shown));
			Dialogue.Speaker = string.IsNullOrWhiteSpace(shown.Speaker) ? null : shown.Speaker;
			Dialogue.Avatar = string.IsNullOrWhiteSpace(shown.Avatar) ? null : shown.Avatar;
			_version++;
			if (_dialogue != null) Bind(_dialogue);
		}

		/// <summary>
		/// A text that comes whole (Game.Dialogue.Say's) broken into lines at the words that pass the layout's text frame's width,
		/// measured in its size and lettering; its own line breaks kept. The game's lines keep the breaks they were written with.
		/// </summary>
		public static string Wrap(string text, int fontSize)
		{
			if (string.IsNullOrEmpty(text) || !Styled) return text;
			Speak(-1, text);
			_spoken = true;
			if (!Laid("dialogue/text", out BattleHud.Rect frame) || frame.Width <= 0) return text;
			XElement textFrame = Subtree("dialogue").FirstOrDefault(f => PathOf(f) == "dialogue/text");
			MenuStyles.Look look = textFrame != null && MenuStyles.Looks(_source, _sheet).TryGetValue(textFrame, out MenuStyles.Look l) ? l : null;
			int size = look != null && int.TryParse(look.Font, out int n) && n >= 6 && n <= 31 ? n : fontSize;
			MenuText lettering = look == null ? null : Lettering(look.TextStyle);
			if (lettering?.Transform != null) text = lettering.Cased(text);
			MenuText was = TrueTypeText.Style;
			TrueTypeText.Style = lettering;
			try
			{
				List<string> lines = new List<string>();
				foreach (string paragraph in text.Split('\n'))
				{
					string line = "";
					foreach (string word in paragraph.Split(' '))
					{
						string tried = line.Length == 0 ? word : line + " " + word;
						if (line.Length > 0 && GlobalScope.getStringWidth(tried, size) > frame.Width) { lines.Add(line); line = word; }
						else line = tried;
					}
					lines.Add(line);
				}
				return string.Join("\n", lines);
			}
			catch (Exception) { return text; }
			finally { TrueTypeText.Style = was; }
		}

		/// <summary>The speaker's name message (FF4's): the name frame's look.</summary>
		public static void DialogueName(GlobalScope.dgs.DGSMessage message)
		{
			if (_dialogue == null) return;
			_dialogue.Text("dialogue/name", message);
			Refresh(_dialogue);
		}

		// ---- the page-turn arrow ----

		private static GlobalScope.sys2d.Sprite3d _nextIcon;
		private static bool _nextWanted;
		private static readonly MenuPanels _nextPanels = new MenuPanels();
		// In front of every panel the layout's frames draw (Refresh says where that is), and where the arrow is moved to.
		private static int _frontDepth;
		private static (int X, int Y) _nextOffset;
		private static Built _next;

		private static MenuStyles.Look NextLook => _dialogue?.Looks.FirstOrDefault(kv => PathOf(kv.Key) == "dialogue/next").Value;

		/// <summary>Whether the game's own arrow is drawn: not when the layout hides it or gives it a picture of its own.</summary>
		public static bool NextGame => NextLook is not MenuStyles.Look look || (!look.Hidden && look.Background == null);

		/// <summary>The game wants its arrow up or down (MessageWindow): a picture of the layout's in its place follows it.</summary>
		public static void NextShown(bool show)
		{
			_nextWanted = show;
			PutNext();
		}

		/// <summary>The arrow in front of the layout's panels, in its tint, opacity and place; or the next frame's own picture while the game's would be up.</summary>
		private static void PutNext()
		{
			MenuStyles.Look look = NextLook;
			bool picture = _dialogue != null && look != null && _nextWanted && look.Background != null && !look.Hidden;
			if (_next != null && (!picture || !SameBox(_next.Look, look))) { _nextPanels.Remove(_next.Sprites, _next.Textures); _next = null; }
			if (_dialogue == null || look == null) return;
			try
			{
				if (_nextIcon != null)
				{
					_nextIcon.SetDepth(_frontDepth);
					uint tint = ModMenus.StyleRgb(look.Tint) is uint rgb ? (rgb >> 16 & 0xFF) | (rgb & 0xFF00) | ((rgb & 0xFF) << 16) : 0xFFFFFFu;
					_nextIcon.SetColor(tint);
					_nextIcon.SetAlpha((byte)Math.Round(Math.Clamp(look.Opacity, 0, 1) * 31));   // the DS's 0..31
					(int nx, int ny) = BattleHud.DialogueNext();
					_nextIcon.SetPositionI(nx + _nextOffset.X, ny + _nextOffset.Y);
				}
				if (_next != null)
				{
					MenuPanels.Fade(_next.Sprites, look.Opacity);
					MenuPanels.Move(_next.Sprites, _nextOffset.X, _nextOffset.Y);
					_next.Look = look;
				}
				else if (picture && _rects.TryGetValue("dialogue/next", out BattleHud.Rect r))   // at the built place: _nextOffset carries the layout's move
				{
					_next = new Built { Look = look };
					_nextPanels.Add(_directory, "dialogue/next", r.X, r.Y, r.Width, r.Height, look.Background, look.Opacity, _frontDepth, _next.Sprites, _next.Textures);
					if (_nextOffset != (0, 0)) MenuPanels.Move(_next.Sprites, _nextOffset.X, _nextOffset.Y);
				}
			}
			catch (Exception) { }
		}

		public static void DialogueClosed() => Close(ref _dialogue);

		// ---- the banner ----

		/// <summary>The map-name banner came up (MapNameWindow.open), its text in it.</summary>
		public static void BannerMade(GlobalScope.menu.BasicWindow window, GlobalScope.dgs.DGSMessage text, int number)
		{
			Close(ref _banner);
			Banner.Number = number;
			try { Banner.Text = number >= 0 ? GlobalScope.dgs.msg.CMessageSys.getInstance().Main().getMessage((uint)number) : null; } catch (Exception) { Banner.Text = null; }
			_version++;
			if (!Styled) return;
			_banner = New("map_name", window, null);
			_banner.Text("map_name", text);
			PutWindowLook(_banner);
			Refresh(_banner);
		}

		public static void BannerClosed() => Close(ref _banner);

		// ---- the Yes / No box ----

		private static GlobalScope.sys2d.Sprite3d _confirmCursor;
		private static (int X, int Y) _cursorPlace;

		/// <summary>The Yes / No box came up (CConfirmWindow.open): its window, its texts and its hand.</summary>
		public static void ConfirmMade(GlobalScope.menu.BasicWindow window, GlobalScope.dgs.DGSMessage yes, GlobalScope.dgs.DGSMessage no, GlobalScope.dgs.DGSMessage question, GlobalScope.sys2d.Sprite3d cursor, int lift = 0)
		{
			Close(ref _confirm);
			_confirmCursor = cursor;
			if (!Styled) return;
			_confirm = New("confirm", window, null, "confirm/question", "confirm/yes", "confirm/no");
			_confirm.Lift = lift;
			_confirm.Text("confirm/yes", yes);
			_confirm.Text("confirm/no", no);
			_confirm.Text("confirm/question", question);
			PutWindowLook(_confirm);
			ConfirmFocus(true);
		}

		/// <summary>The hand moved to Yes or No (swCurPos): that one in :focus for the sheets, and the hand moved with the box.</summary>
		public static void ConfirmFocus(bool yes)
		{
			if (_confirm == null) return;
			try { _cursorPlace = (_confirmCursor.GetPositionI().x, _confirmCursor.GetPositionI().y); } catch (Exception) { }
			SetState("confirm/yes", "focus", yes);
			SetState("confirm/no", "focus", !yes);
			Refresh(_confirm);
		}

		public static void ConfirmClosed() => Close(ref _confirm);

		// ---- the field's buttons ----

		private static string ButtonFrame(int cell) => cell switch
		{
			0 => "menu_button", 13 => "map_button", 14 => "talk_button",
			MenuButtonKey => "a_button", MenuButtonKey + 1 => "b_button", MenuButtonKey + 2 => "l_button", MenuButtonKey + 3 => "r_button",
			_ => null,
		};

		/// <summary>A field button was set up (CMenuButton.setup): its frame's look over its sprite, while the layout has one.</summary>
		public static void ButtonMade(int cell, GlobalScope.sys2d.Sprite sprite)
		{
			if (_buttons.TryGetValue(cell, out Shown old)) { Close(ref old); _buttons.Remove(cell); }
			_buttonSprites[cell] = (sprite, 0, 0);
			_buttonAlpha.Remove(cell);
			MakeButton(cell);
		}

		private static void MakeButton(int cell)
		{
			string root = ButtonFrame(cell);
			if (root == null || !Styled || !_buttonSprites.TryGetValue(cell, out var made) || Subtree(root).FirstOrDefault() == null) return;
			Shown s = New(root, null, null, root + "/text");
			s.Sprite = made.Sprite;
			s.SpritePlace = (made.X, made.Y);
			s.Visible = false;
			// A field button stands where the options put it, its frames moved there with it; a menu's is where its frame is.
			if (cell < MenuButtonKey && _rects.TryGetValue(root, out BattleHud.Rect r)) s.Shift = (made.X - r.X, made.Y - r.Y);
			if (_buttonTexts.TryGetValue(cell, out GlobalScope.dgs.DGSMessage text)) s.Text(root + "/text", text);
			_buttons[cell] = s;
		}

		/// <summary>A button was put in a place (setPosition): where it stands against its own frame's place.</summary>
		public static void ButtonPlaced(int cell, int x, int y)
		{
			if (_buttonSprites.TryGetValue(cell, out var made)) _buttonSprites[cell] = (made.Sprite, x, y);
			if (!_buttons.TryGetValue(cell, out Shown s)) return;
			s.SpritePlace = (x, y);
			if (cell < MenuButtonKey && _rects.TryGetValue(s.Root, out BattleHud.Rect r)) s.Shift = (x - r.X, y - r.Y);
			if (s.Built) Refresh(s);
		}

		/// <summary>Before the button's own frame (CMenuButton.execute): its alpha as it left it, the layout's opacity taken off again.</summary>
		public static void ButtonBefore(int cell)
		{
			if (_buttons.TryGetValue(cell, out Shown s) && _buttonAlpha.TryGetValue(cell, out byte alpha)) { try { s.Sprite.SetAlpha(alpha); } catch (Exception) { } }
		}

		/// <summary>After it: the layout's frames shown and faded with it, its sprite in the layout's tint, opacity and place - or taken away for a picture of the layout's.</summary>
		public static void ButtonAfter(int cell)
		{
			if (cell < MenuButtonKey) OverlayTick();
			if (!_buttons.TryGetValue(cell, out Shown s)) return;
			try
			{
				byte alpha = s.Sprite.GetAlpha();
				_buttonAlpha[cell] = alpha;
				bool menu = _buttonShown.TryGetValue(cell, out bool wanted);
				bool visible = menu ? wanted : s.Sprite.IsShow();
				float fade = alpha / 31f;
				if (visible != s.Visible || Math.Abs(fade - s.Fade) > 0.01f || !s.Built || _animator.Active)
				{
					s.Visible = visible;
					s.Fade = fade;
					Refresh(s);
				}
				XElement top = Subtree(s.Root).FirstOrDefault();
				if (top == null || !s.Looks.TryGetValue(top, out MenuStyles.Look look)) return;
				if (look.Background != null || look.Hidden || look.NoPanel) { s.Sprite.SetShow(false); return; }
				if (menu) s.Sprite.SetShow(visible);   // the game shows a menu's button once, not each frame: shown again under a look that keeps it
				uint tint = ModMenus.StyleRgb(look.Tint) is uint rgb ? (rgb >> 16 & 0xFF) | (rgb & 0xFF00) | ((rgb & 0xFF) << 16) : 0xFFFFFFu;
				s.Sprite.SetColor(tint);
				s.Sprite.SetAlpha((byte)Math.Round(alpha * Math.Clamp(look.Opacity, 0, 1)));
				(int dx, int dy) = Offset(s, top, s.Looks);
				s.Sprite.SetPositionI(s.SpritePlace.X + dx - s.Shift.X, s.SpritePlace.Y + dy - s.Shift.Y);
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "field hud: " + s.Root + ": " + ex.Message); }
		}

		// ---- the overlay: the layout's own frames at its top (a party panel, a quest log), up in the field while its buttons are ----

		// The pieces of the HUD's the game draws itself; any other frame at field_hud's top is the layout's own, an overlay.
		private static readonly HashSet<string> Pieces = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"dialogue", "map_name", "confirm", "menu_button", "map_button", "talk_button", "title", "a_button", "b_button", "l_button", "r_button",
		};
		private static readonly List<Shown> _overlay = new List<Shown>();
		private static long _overlayFrame = -1, _overlayBoundAt = -1000;
		private static int _hudVersion = -1;
		private const int OverlayRebind = 10;   // steps between readings of its bindings (a third of a second)

		/// <summary>
		/// Once a step in the field (a field button's): the overlay up while the field's buttons are (and Game.Hud.Overlay), its
		/// bindings read again every few steps and at once when a mod's data changed - its panels and texts made again where
		/// they changed (a bar's width, a name, a quest's step).
		/// </summary>
		private static void OverlayTick()
		{
			long frame = -1;
			try { frame = OpenFF.Game.Time.Frame; } catch (Exception) { }
			if (frame == _overlayFrame) return;
			_overlayFrame = frame;
			if (!Styled) { CloseOverlay(); return; }
			if (_overlay.Count == 0)
			{
				foreach (XElement top in _source.Elements("frame"))
				{
					string root = PathOf(top);
					if (root != null && !Pieces.Contains(root)) _overlay.Add(New(root, null, null));
				}
				if (_overlay.Count == 0) return;
			}
			bool visible = OpenFF.Game.Hud.Overlay && FieldButtonsShown();
			int hud = OpenFF.Game.Hud.Version;
			bool rebind = frame - _overlayBoundAt >= OverlayRebind || hud != _hudVersion;
			if (rebind) { _overlayBoundAt = frame; _hudVersion = hud; }
			foreach (Shown s in _overlay)
			{
				s.Panels.Flush();
				try
				{
					if (!s.Built || s.Visible != visible || (visible && (rebind || _animator.Active)))
					{
						s.Visible = visible;
						Refresh(s);
					}
				}
				catch (Exception ex) { Log.Write(LogChannel.General, "field hud: " + s.Root + ": " + ex.Message); }
			}
		}

		/// <summary>Whether the field's buttons are up (any of them): the game takes them down for its events, menus and battles.</summary>
		private static bool FieldButtonsShown()
		{
			foreach (KeyValuePair<int, (GlobalScope.sys2d.Sprite Sprite, int X, int Y)> b in _buttonSprites)
			{
				if (b.Key >= MenuButtonKey) continue;
				try { if (_buttons.TryGetValue(b.Key, out Shown s) ? s.Visible : b.Value.Sprite.IsShow()) return true; } catch (Exception) { }
			}
			return false;
		}

		private static void CloseOverlay()
		{
			for (int i = 0; i < _overlay.Count; i++) { Shown s = _overlay[i]; Close(ref s); }
			_overlay.Clear();
		}

		/// <summary>The button is gone (CMenuButton.cleanup).</summary>
		public static void ButtonGone(int cell)
		{
			if (_buttons.TryGetValue(cell, out Shown s)) { Close(ref s); _buttons.Remove(cell); }
			_buttonSprites.Remove(cell);
			_buttonAlpha.Remove(cell);
			_buttonShown.Remove(cell);
			_buttonTexts.Remove(cell);
			if (!_buttonSprites.Keys.Any(k => k < MenuButtonKey)) CloseOverlay();   // out of the field: its overlay with its buttons
		}

		// ---- the menus' buttons (CWMenuButton: A, B, L, R) ----

		/// <summary>A menu's button was made (CWMenuButton.initialize): its frame's look over its sprite, hidden until the game shows it.</summary>
		public static void MenuButtonMade(int i, GlobalScope.sys2d.Sprite sprite)
		{
			int cell = MenuButtonKey + i;
			_buttonShown[cell] = false;
			_buttonTexts.Remove(cell);
			ButtonMade(cell, sprite);
			try { ButtonPlaced(cell, sprite.GetPositionI().x, sprite.GetPositionI().y); } catch (Exception) { }
		}

		/// <summary>A menu button's label (A's, B's), in its text frame's look.</summary>
		public static void MenuButtonText(int i, GlobalScope.dgs.DGSMessage message)
		{
			int cell = MenuButtonKey + i;
			if (message == null) return;
			_buttonTexts[cell] = message;
			if (_buttons.TryGetValue(cell, out Shown s)) s.Text(s.Root + "/text", message);
		}

		/// <summary>The game showed or hid a menu's button (SetButton?Activity, SetUpNormalVer...).</summary>
		public static void MenuButtonShown(int i, bool shown)
		{
			int cell = MenuButtonKey + i;
			_buttonShown[cell] = shown;
			ButtonAfter(cell);
		}

		public static void MenuButtonGone(int i) => ButtonGone(MenuButtonKey + i);

		/// <summary>Each frame of a menu: its buttons' panels, and what moves on them.</summary>
		public static void MenuButtonsTick()
		{
			// A pad plugged in or taken out (the input root's hints): the buttons that bind them made again.
			_ = InputHints.Current;
			bool input = InputHints.Version != _inputVersion;
			_inputVersion = InputHints.Version;
			foreach (int cell in _buttons.Keys.Where(k => k >= MenuButtonKey).ToList())
			{
				Shown s = _buttons[cell];
				s.Panels.Flush();
				if (input && s.Built) Refresh(s);
				if (!s.Built || _animator.Active) { ButtonBefore(cell); ButtonAfter(cell); }
			}
		}

		private static int _inputVersion = -1;

		// ---- the title's commands (ttl.CTitle2D: the column where title puts it, its labels drawn in ModListScreen) ----

		private static readonly Dictionary<(bool, bool), MenuStyles.Look> _titleLooks = new Dictionary<(bool, bool), MenuStyles.Look>();

		/// <summary>
		/// A title command's look - title/row's, the one the hand is on in :focus, a Continue with nothing to continue :disabled -, or
		/// null while the layout says nothing about looks. Worked out from the sheets as they stand (the title's rows do not move).
		/// </summary>
		public static MenuStyles.Look TitleLook(bool focus, bool disabled)
		{
			if (!Styled) return null;
			if (_titleLooks.TryGetValue((focus, disabled), out MenuStyles.Look cached)) return cached;
			XElement row = _source.Descendants("frame").FirstOrDefault(f => PathOf(f) == "title/row");
			MenuStyles.Look look = null;
			if (row != null)
			{
				XAttribute was = row.Attribute(MenuStyles.StateAttribute);
				string states = string.Join(" ", new[] { focus ? "focus" : null, disabled ? "disabled" : null }.Where(x => x != null));
				row.SetAttributeValue(MenuStyles.StateAttribute, states.Length == 0 ? null : states);
				try
				{
					List<(XElement Frame, Dictionary<string, string> Computed)> computed = MenuStyles.Computed(_source, _sheet);
					MenuStyles.Looks(_source, computed, computed.ToDictionary(c => c.Frame, c => c.Computed)).TryGetValue(row, out look);
				}
				finally { row.SetAttributeValue(MenuStyles.StateAttribute, was?.Value); }
			}
			_titleLooks[(focus, disabled)] = look;
			return look;
		}

		/// <summary>A look's lettering with its faces' files found (url() beside the layout).</summary>
		public static MenuText TitleLettering(MenuStyles.Look look) => look == null ? null : Lettering(look.TextStyle);

		/// <summary>Every frame while a thing of the HUD's is up: its panels once it is open, and what moves (transitions, animations).</summary>
		public static void Tick()
		{
			_nextPanels.Flush();
			foreach (Shown s in new[] { _dialogue, _banner, _confirm })
			{
				if (s == null) continue;
				s.Panels.Flush();
				try
				{
					if (!s.Built) { if (s.Open()) Refresh(s); }
					else if (_animator.Active) Refresh(s);
				}
				catch (Exception ex) { Log.Write(LogChannel.General, "field hud: " + ex.Message); }
			}
			foreach (Shown s in _buttons.Values) s.Panels.Flush();
		}

		// ---- the looks put on ----

		private static string PathOf(XElement frame) => frame != null && _pathOf.TryGetValue(frame, out string p) ? p : null;

		private static IEnumerable<XElement> Subtree(string root)
		{
			XElement top = _source?.Elements("frame").FirstOrDefault(f => PathOf(f) == root);
			return top == null ? Enumerable.Empty<XElement>() : top.DescendantsAndSelf("frame");
		}

		/// <summary>A frame's state (focus) on or off, for the sheets' pseudo-classes.</summary>
		private static void SetState(string path, string state, bool on)
		{
			XElement frame = _source?.Descendants("frame").FirstOrDefault(f => PathOf(f) == path);
			if (frame == null) return;
			List<string> states = ((string)frame.Attribute(MenuStyles.StateAttribute) ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
			if (states.Contains(state) == on) return;
			if (on) states.Add(state); else states.Remove(state);
			frame.SetAttributeValue(MenuStyles.StateAttribute, states.Count > 0 ? string.Join(" ", states) : null);
			_version++;
		}

		/// <summary>The game's window as the root frame's look has it: taken away (-ff-panel: none), the bar, opacity and tint.</summary>
		private static void PutWindowLook(Shown s)
		{
			if (s.Window == null) return;
			Dictionary<XElement, MenuStyles.Look> looks = CurrentLooks();
			XElement root = Subtree(s.Root).FirstOrDefault();
			if (root == null || !looks.TryGetValue(root, out MenuStyles.Look look)) return;
			try
			{
				if (look.NoPanel || look.Hidden) s.Window.SetLook(0f, null);
				else
				{
					if (look.Bar) s.Window.SetBarStyle();
					uint? tint = ModMenus.StyleRgb(look.Tint) is uint rgb ? (rgb >> 16 & 0xFF) | (rgb & 0xFF00) | ((rgb & 0xFF) << 16) : (uint?)null;
					if (look.Opacity < 1 || tint.HasValue) s.Window.SetLook((float)look.Opacity, tint);
				}
			}
			catch (Exception) { }
		}

		/// <summary>A frame's box in a thing opened without its question line (Shown.Lift): the root that much shorter, what is under the question moved up.</summary>
		private static BattleHud.Rect Lifted(Shown s, string path, BattleHud.Rect r)
		{
			if (s.Lift == 0) return r;
			if (path == s.Root) { r.Height -= s.Lift; return r; }
			if (_rects.TryGetValue(s.Root + "/question", out BattleHud.Rect q) && r.Y > q.Y) r.Y -= s.Lift;
			return r;
		}

		/// <summary>How far a frame's drawing is moved: its translate's difference from the one its layout was built with, its parents' with it (as CSS moves a box's children with it); a thing's shift from its frame's place at its root.</summary>
		private static (int X, int Y) Offset(Shown s, XElement frame, Dictionary<XElement, MenuStyles.Look> looks)
		{
			float x = 0, y = 0;
			for (XElement f = frame; f != null && f.Name.LocalName == "frame"; f = f.Parent)
			{
				string path = PathOf(f);
				if (path == null || !_rects.TryGetValue(path, out BattleHud.Rect r)) continue;
				looks.TryGetValue(f, out MenuStyles.Look look);
				(float nx, float ny) = MenuStyles.TranslateOf(look?.Translate, r.Width, r.Height);
				(float bx, float by) = MenuStyles.TranslateOf(_baseTranslate.TryGetValue(f, out string b) ? b : null, r.Width, r.Height);
				x += nx - bx;
				y += ny - by;
				if (path == s.Root) break;
			}
			return ((int)Math.Round(x) + s.Shift.X, (int)Math.Round(y) + s.Shift.Y);
		}

		/// <summary>Where a menu button's label (a_button/text, b_button/text) stands when the sheets give it a text-align: in its frame as laid out, left, centred or right, and centred down it; null otherwise.</summary>
		private static (int X, int Y)? AlignedLabel(string path, GlobalScope.dgs.DGSMessage message, MenuStyles.Look look)
		{
			if (path != "a_button/text" && path != "b_button/text") return null;
			string align = look?.Align?.Trim().ToLowerInvariant();
			if (align != "left" && align != "center" && align != "centre" && align != "right") return null;
			if (!Laid(path, out BattleHud.Rect r)) return null;
			GlobalScope.ds.Vector2<short> size = new GlobalScope.ds.Vector2<short>();
			try { message.getTextSize(size); } catch (Exception) { return null; }
			int x = align == "left" ? r.X : align == "right" ? r.X + r.Width - size.vx : r.X + (r.Width - size.vx) / 2;
			return (x, r.Y + (r.Height - size.vy) / 2);
		}

		/// <summary>A thing's frames' looks and data put on: its game texts' looks and places, its frames' texts, and - once it is open - its panels (a fade or a move in place, made again where the box changed).</summary>
		private static void Refresh(Shown s)
		{
			if (_source == null) return;
			Bind(s);
			Dictionary<XElement, MenuStyles.Look> looks = CurrentLooks();
			bool open = s.Built || s.Open();
			XElement top = Subtree(s.Root).FirstOrDefault();
			bool rootMoved = top == null || !s.Looks.TryGetValue(top, out MenuStyles.Look rootWas) || !looks.TryGetValue(top, out MenuStyles.Look rootNow) || !rootWas.SameAs(rootNow);
			s.Looks = looks;
			if (rootMoved) PutWindowLook(s);
			// The game's texts: each in its frame's look, moved with it.
			foreach (KeyValuePair<string, GlobalScope.dgs.DGSMessage> t in s.Texts)
			{
				XElement frame = Subtree(s.Root).FirstOrDefault(f => PathOf(f) == t.Key);
				if (frame == null || !looks.TryGetValue(frame, out MenuStyles.Look look)) continue;
				PutText(t.Value, look, s.Fade, s.Visible);
				(int dx, int dy) = Offset(s, frame, looks);
				// A menu button's label with a text-align of the sheets' stands in its frame as laid out (a pill's row of a
				// key and its label); without one, where the game centres it in the button, moved with its frame.
				if (AlignedLabel(t.Key, t.Value, look) is (int X, int Y) aligned)
				{
					try { t.Value.setPosition((short)(aligned.X + dx - s.Shift.X), (short)(aligned.Y + dy - s.Shift.Y), erase: true); } catch (Exception) { }
					continue;
				}
				(int lx, int ly) = LaidShift(t.Key);
				dx += lx;
				dy += ly;
				if (s.TextPlace.TryGetValue(t.Value, out (short X, short Y) at) && (dx != 0 || dy != 0 || rootMoved)) { try { t.Value.setPosition((short)(at.X + dx - s.Shift.X), (short)(at.Y + dy - s.Shift.Y), erase: true); } catch (Exception) { } }
			}
			if (!open) return;
			// The game's window moved with its frame.
			if (s.Window != null && top != null)
			{
				(int wx, int wy) = Offset(s, top, looks);
				wx -= s.Shift.X; wy -= s.Shift.Y;
				if ((wx, wy) != s.WindowOffset)
				{
					try { s.Window.SetPositionUL(new GlobalScope.ds.Vector2<short>((short)(s.WindowPlace.X + wx), (short)(s.WindowPlace.Y + wy))); } catch (Exception) { }
					s.WindowOffset = (wx, wy);
				}
			}
			int depth = 0, step = GlobalScope.ds.S32toFX32(32);
			try { if (s.Window != null) depth = s.Window.GetDepth(); } catch (Exception) { }
			int place = 0;
			foreach (XElement frame in Subtree(s.Root))
			{
				string path = PathOf(frame);
				if (path == "dialogue/next") continue;   // the arrow's (PutNext)
				bool game = s.Game.Contains(path);
				// Each frame of the mod's a step in front of the one before, the game's window at the back.
				int back = game && path == s.Root ? depth : depth - step * ++place;
				looks.TryGetValue(frame, out MenuStyles.Look look);
				bool shown = look != null && !look.Hidden && _rects.ContainsKey(path);
				float fade = look == null ? 1f : (float)look.Opacity * s.Fade;
				(int dx, int dy) = Offset(s, frame, looks);
				s.Frames.TryGetValue(frame, out Built built);
				Laid(path, out BattleHud.Rect box);
				box = Lifted(s, path, box);
				if (built != null && shown && SameBox(built.Look, look) && SameRect(built.Rect, box))
				{
					// The same box: a fade and a move are put on in place.
					if (Math.Abs(built.Fade - fade) > 0.001f)
					{
						MenuPanels.Fade(built.Sprites, fade);
						try { built.Window?.SetLook(fade, null); } catch (Exception) { }
						built.Fade = fade;
					}
					if (built.Offset != (dx, dy) || built.Visible != s.Visible)
					{
						MenuPanels.Move(built.Sprites, dx, dy, s.Visible);
						try
						{
							built.Window?.SetPositionUL(new GlobalScope.ds.Vector2<short>((short)(built.WindowPlace.X + dx), (short)(built.WindowPlace.Y + dy)));
							built.Window?.SetShow(s.Visible, user: true);
						}
						catch (Exception) { }
						built.Offset = (dx, dy);
						built.Visible = s.Visible;
					}
					built.Look = look;
					continue;
				}
				// Made again: the old taken away as the new comes (a frame late - the game's sprites draw from the next frame).
				if (built != null)
				{
					s.Panels.Remove(built.Sprites, built.Textures);
					try { built.Window?.Release(); } catch (Exception) { }
				}
				built = new Built { Look = look, Fade = fade, Offset = (dx, dy), Visible = s.Visible, Rect = box };
				s.Frames[frame] = built;
				if (!shown) continue;
				BattleHud.Rect r = box;
				if (!game && look.Window)
				{
					GlobalScope.menu.BasicWindow window = new GlobalScope.menu.BasicWindow();
					window.bwCreateUL(GlobalScope.sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, new GlobalScope.ds.Vector2<short>((short)(r.X + dx), (short)(r.Y + dy)), new GlobalScope.ds.Vector2<short>((short)r.Width, (short)r.Height), 3);
					window.SetPriority(3);
					window.SetStackDepth(back);
					if (look.Bar) window.SetBarStyle();
					if (fade < 1) window.SetLook(fade, null);
					window.SetShow(s.Visible, user: true);
					built.Window = window;
					built.WindowPlace = (r.X, r.Y);
				}
				if (look.Background != null)
				{
					s.Panels.Add(_directory, path, r.X, r.Y, r.Width, r.Height, look.Background, fade, back + GlobalScope.ds.S32toFX32(8), built.Sprites, built.Textures);
					if (dx != 0 || dy != 0 || !s.Visible) MenuPanels.Move(built.Sprites, dx, dy, s.Visible);
				}
			}
			foreach (Extra e in s.Extras) PutExtra(s, e, looks);
			if (s == _dialogue)
			{
				_frontDepth = depth - step * (place + 2);
				XElement next = Subtree(s.Root).FirstOrDefault(f => PathOf(f) == "dialogue/next");
				_nextOffset = next != null ? Offset(s, next, looks) : (0, 0);
				(int nlx, int nly) = LaidShift("dialogue/next");
				_nextOffset = (_nextOffset.X + nlx, _nextOffset.Y + nly);
				PutNext();
			}
			if (s == _confirm && _confirmCursor != null && top != null)
			{
				(int cx, int cy) = Offset(s, top, looks);
				try { _confirmCursor.SetPositionI(_cursorPlace.X + cx, _cursorPlace.Y + cy); } catch (Exception) { }
			}
			s.Built = true;
		}

		/// <summary>The frames' bindings under the HUD's data: their texts (for the extras), shown or not, classes and bound styles.</summary>
		private static void Bind(Shown s)
		{
			MenuBindingScope root = new MenuBindingScope { Root = Root };
			Dictionary<XElement, MenuBindingScope> scopes = new Dictionary<XElement, MenuBindingScope>();
			foreach (XElement frame in Subtree(s.Root))
			{
				string path = PathOf(frame);
				if (!s.Game.Contains(path) && IsText(frame) && !s.Extras.Any(e => e.Frame == frame)) s.Extras.Add(new Extra { Frame = frame, Path = path });
				MenuBindingScope scope = ScopeOf(frame, root, scopes);
				try
				{
					foreach (KeyValuePair<string, string> c in MenuStyles.Declarations((string)frame.Attribute("bind-class")))
						if (SetClass(frame, c.Key, MenuBindings.Test(c.Value, scope))) _version++;
					string own = _ownStyle.TryGetValue(frame, out string o) ? o : null;
					string bound = frame.Attribute("bind-style") is XAttribute bs ? MenuBindings.Format(bs.Value, scope) : null;
					string visible = frame.Attribute("bind-visible") is XAttribute bv ? (MenuBindings.Test(bv.Value, scope) ? null : "visibility: hidden") : null;
					// bind-display: out of the layout while false - its row or column closes up over its place (bind-visible keeps it).
					string display = frame.Attribute("bind-display") is XAttribute bd ? (MenuBindings.Test(bd.Value, scope) ? null : "display: none") : null;
					string style = string.Join("; ", new[] { own, bound, visible, display }.Where(x => !string.IsNullOrWhiteSpace(x)));
					if (style != ((string)frame.Attribute("style") ?? "")) { frame.SetAttributeValue("style", style.Length == 0 ? null : style); _version++; }
				}
				catch (Exception ex) { Log.Write(LogChannel.General, "field hud: " + path + ": " + ex.Message); }
			}
			foreach (Extra e in s.Extras)
			{
				string text = e.Frame.Attribute("bind-text") is XAttribute bt ? MenuBindings.Format(bt.Value, ScopeOf(e.Frame, root, scopes)) : ((string)e.Frame.Element("data"))?.Trim();
				if (text != e.Text) { e.Text = text; ReleaseExtra(e); }
			}
			Reflow();
		}

		/// <summary>A frame's binding scope: its data-source's (party[1] - its frames' {name} that member's), else its parent's, else the HUD's roots.</summary>
		private static MenuBindingScope ScopeOf(XElement frame, MenuBindingScope root, Dictionary<XElement, MenuBindingScope> scopes)
		{
			if (frame == null || frame.Name.LocalName != "frame") return root;
			if (scopes.TryGetValue(frame, out MenuBindingScope known)) return known;
			MenuBindingScope parent = ScopeOf(frame.Parent, root, scopes);
			string source = (string)frame.Attribute("data-source");
			MenuBindingScope scope = string.IsNullOrWhiteSpace(source) ? parent : parent.With(MenuBindings.Resolve(source, parent));
			scopes[frame] = scope;
			return scope;
		}

		private static bool IsText(XElement frame) => frame.Attribute("bind-text") != null || (string)frame.Element("behavior")?.Attribute("value") == "Text" || !string.IsNullOrWhiteSpace((string)frame.Element("data"));

		private static bool SetClass(XElement frame, string name, bool on)
		{
			HashSet<string> classes = new HashSet<string>(((string)frame.Attribute("class") ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries), StringComparer.Ordinal);
			if (!(on ? classes.Add(name) : classes.Remove(name))) return false;
			frame.SetAttributeValue("class", classes.Count == 0 ? null : string.Join(" ", classes));
			return true;
		}

		/// <summary>The roots the HUD's bindings reach: dialogue, banner, the game's hero, party and gil, and the input in hand.</summary>
		private static (bool, object) Root(string name)
		{
			switch (name.ToLowerInvariant())
			{
				case "dialogue": return (true, Dialogue);
				case "banner": return (true, Banner);
				case "hero": { IReadOnlyList<PartyMember> m = OpenFF.Game.Party?.Members; return (true, m != null && m.Count > 0 ? m[0] : null); }
				case "party": return (true, OpenFF.Game.Party?.Members);
				case "places": return (true, ModMenus.Places());
				case "gil": return (true, OpenFF.Game.Party?.Gil ?? 0);
				case "items": return (true, OpenFF.Game.Party?.Items);
				case "input": return (true, InputHints.Current);
				default: return OpenFF.Game.Hud.TryGet(name, out object hud) ? (true, hud) : (false, null);   // a mod's (Game.Hud.Set: a quest log's)
			}
		}

		// The looks once a frame of the game's (the animator steps once a frame for every thing of the HUD's), again when a binding or a state changed them.
		private static int _version;
		private static (long Frame, int Version) _looksAt = (-1, -1);
		private static Dictionary<XElement, MenuStyles.Look> _looks = new Dictionary<XElement, MenuStyles.Look>();

		/// <summary>The layout's looks now: the cascade (its sheets, its frames' bound classes, styles and states) and what is moving.</summary>
		private static Dictionary<XElement, MenuStyles.Look> CurrentLooks()
		{
			long frame = -1;
			try { frame = OpenFF.Game.Time.Frame; } catch (Exception) { }
			if (_looksAt == (frame, _version) && frame >= 0) return _looks;
			List<(XElement Frame, Dictionary<string, string> Computed)> computed = MenuStyles.Computed(_source, _sheet);
			Dictionary<XElement, Dictionary<string, string>> shown = _animator.Step(computed, _sheet, _clock.Elapsed.TotalSeconds);
			_looks = MenuStyles.Looks(_source, computed, shown);
			_looksAt = (frame, _version);
			return _looks;
		}

		/// <summary>A text of the game's (the dialogue's, the banner's, the box's) with a frame's look: its colour, opacity, size and lettering; faded and shown with its thing.</summary>
		private static void PutText(GlobalScope.dgs.DGSMessage message, MenuStyles.Look look, float fade = 1f, bool visible = true)
		{
			try
			{
				GlobalScope.NNSG2dTextCanvas canvas = message.m_TextCanvas;
				if (canvas == null) return;
				if (ModMenus.StyleRgb(look.Colour) is uint rgb) canvas.rgba = rgb << 8 | 0xFF;
				else if (!string.IsNullOrWhiteSpace(look.Colour)) { canvas.rgba = null; message.setMessageColor((GlobalScope.dgs.TXT_COLOR)(int)ModMenus.ColourWord(look.Colour)); }
				canvas.alpha = (byte)Math.Round(Math.Clamp(look.Opacity * fade, 0, 1) * 255);
				MenuText lettering = Lettering(look.TextStyle);
				int size = int.TryParse(look.Font, out int n) && n >= 6 && n <= 31 ? n : look.Font == "large" ? 16 : 0;
				if (size > 0 || lettering != null || canvas.style != null)
				{
					if (!(canvas.pFont != null && canvas.pFont.size == (size > 0 ? size : canvas.pFont.size) && canvas.style?.Key == lettering?.Key))
					{
						canvas.style = lettering;
						canvas.pFont = new GlobalScope.NNSG2dFont { size = size > 0 ? size : canvas.pFont?.size ?? 12, style = lettering };
					}
				}
				message.setVisibility(visible && !look.Hidden);
				message.Redraw();
			}
			catch (Exception) { }
		}

		/// <summary>A frame of the mod's with text: a message of its own at the frame's place (its alignment across, centred down), in its look, moved, faded and shown with its thing.</summary>
		private static void PutExtra(Shown s, Extra e, Dictionary<XElement, MenuStyles.Look> looks)
		{
			if (!looks.TryGetValue(e.Frame, out MenuStyles.Look look) || !Laid(e.Path, out BattleHud.Rect r)) return;
			r = Lifted(s, e.Path, r);
			if (e.Message < 0) e.Menu = MenuButtonRoots.Contains(s.Root);
			GlobalScope.dgs.msg.CMessageMng mm = Messages(e);
			if (e.Message < 0)
			{
				if (string.IsNullOrEmpty(e.Text)) return;
				e.Message = mm.createMessage(e.Text, (ushort)Math.Max(0, r.X), (ushort)Math.Max(0, r.Y), GlobalScope.dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_COMMON, GlobalScope.dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_12x12);
				if (e.Message < 0) return;
				GlobalScope.dgs.DGSMessage made = mm.Message(e.Message);
				made?.setDisplaySpeed(byte.MaxValue);
				made?.setShadow(b: true);
			}
			GlobalScope.dgs.DGSMessage message = mm.Message(e.Message);
			if (message == null) return;
			PutText(message, look, s.Fade, s.Visible);
			// Measured in the font it has now (its size and face from the sheets), not the area it last drew in.
			GlobalScope.ds.Vector2<short> size = new GlobalScope.ds.Vector2<short>();
			message.getTextSize(size);
			string align = (look.Align ?? "").Trim();
			(int dx, int dy) = Offset(s, e.Frame, looks);
			int x = (align == "right" ? r.X + r.Width - size.vx : align == "center" ? r.X + (r.Width - size.vx) / 2 : r.X) + dx;
			int y = (r.Height > 0 ? r.Y + (r.Height - size.vy) / 2 : r.Y) + dy;
			if (message.positionX() != x || message.positionY() != y) message.setPosition((short)x, (short)y, erase: true);
		}

		private static void ReleaseExtra(Extra e)
		{
			if (e.Message < 0) return;
			try { Messages(e).releaseMessage(e.Message); } catch (Exception) { }
			e.Message = -1;
		}

		private static readonly HashSet<string> MenuButtonRoots = new HashSet<string>(StringComparer.Ordinal) { "a_button", "b_button", "l_button", "r_button" };

		/// <summary>The message manager a text of the layout's is made by: the menus' for a menu button's (the field's are not drawn over a menu), the field's otherwise.</summary>
		private static GlobalScope.dgs.msg.CMessageMng Messages(Extra e) =>
			e.Menu ? GlobalScope.dgs.msg.CMessageSys.getInstance().Sub() : GlobalScope.dgs.msg.CMessageSys.getInstance().Main();

		/// <summary>A text's lettering, its faces' files beside the layout.</summary>
		private static MenuText Lettering(string declarations)
		{
			MenuText text = MenuText.Parse(declarations);
			if (text == null) return null;
			for (int i = 0; i < text.Families.Count; i++)
			{
				if (!text.Families[i].StartsWith("url:", StringComparison.Ordinal)) continue;
				text.Families[i] = _directory == null ? "missing" : "file:" + Path.GetFullPath(Path.Combine(_directory, text.Families[i].Substring(4)));
			}
			return text;
		}

		private static void Close(ref Shown s)
		{
			if (s == null) return;
			if (s == _dialogue) { _nextPanels.Clear(); _next = null; _nextWanted = false; }
			if (s == _confirm)
			{
				SetState("confirm/yes", "focus", false);
				SetState("confirm/no", "focus", false);
			}
			s.Panels.Clear();
			foreach (Built b in s.Frames.Values) { try { b.Window?.Release(); } catch (Exception) { } }
			foreach (Extra e in s.Extras) ReleaseExtra(e);
			s = null;
		}
	}
}
