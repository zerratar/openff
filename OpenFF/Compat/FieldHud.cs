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
//     <any other>       a frame of the mod's: a window, a background, a text (<data>, bind-text) - shown
//                       with the window, as its data says: {dialogue.speaker}, {dialogue.avatar}, ...
//   map_name            the banner a map's name comes up in; its text takes its look
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

		// ---- what is up: the dialogue, the banner ----

		/// <summary>A window of the HUD's up: its frame, the game's window and text, and what the layout adds.</summary>
		private sealed class Shown
		{
			public string Root;
			public GlobalScope.menu.BasicWindow Window;
			public Func<bool> Open;
			public bool Built;
			public GlobalScope.dgs.DGSMessage Text;    // the game's text in it
			public string TextFrame;                    // the frame whose look the text takes
			public GlobalScope.dgs.DGSMessage Name;
			public readonly MenuPanels Panels = new MenuPanels();
			public readonly Dictionary<XElement, Built> Frames = new Dictionary<XElement, Built>();
			public readonly List<Extra> Extras = new List<Extra>();
			public Dictionary<XElement, MenuStyles.Look> Looks = new Dictionary<XElement, MenuStyles.Look>();
		}

		/// <summary>What a frame's panel was made of, and the look it was made for: a fade put on in place, anything else made again.</summary>
		private sealed class Built
		{
			public MenuStyles.Look Look;
			public GlobalScope.menu.BasicWindow Window;
			public readonly List<GlobalScope.MenuPanelSprite> Sprites = new List<GlobalScope.MenuPanelSprite>();
			public readonly List<uint> Textures = new List<uint>();
		}

		/// <summary>Whether two looks make the same box but for its opacity (a fade needs nothing made again).</summary>
		private static bool SameBox(MenuStyles.Look a, MenuStyles.Look b) => a != null && b != null && a.Window == b.Window && a.Bar == b.Bar && a.NoPanel == b.NoPanel
			&& a.Tint == b.Tint && a.Background == b.Background && a.Hidden == b.Hidden;

		/// <summary>A frame of the layout's own in a window of the HUD's: its text as a message of its own.</summary>
		private sealed class Extra
		{
			public XElement Frame;
			public string Path;
			public int Message = -1;
			public string Text;
		}

		private static Shown _dialogue, _banner;

		/// <summary>Whether field_hud's layout says anything about a window's look (else the game's window is left to itself).</summary>
		private static bool Styled => _source != null && (MenuStyles.Has(_source) || _sheet.Rules.Count > 0 || _source.Descendants().Any(e => e.Name.LocalName is "background" or "textstyle" or "panel" or "tint" or "opacity" or "animation" or "colour"));

		/// <summary>The dialogue window was made (MessageWindow.mwSetWindow): the game's art taken away if the layout says none, the rest once it is open.</summary>
		public static void DialogueMade(GlobalScope.menu.BasicWindow window, Func<bool> open, GlobalScope.sys2d.Sprite3d next = null)
		{
			Close(ref _dialogue);
			_nextIcon = next;
			_nextWanted = false;
			if (!Styled) return;
			_dialogue = new Shown { Root = "dialogue", Window = window, Open = open, TextFrame = "dialogue/text" };
			PutWindowLook(_dialogue);
		}

		/// <summary>A text went into the dialogue (mwSetMessage / mwSetMessageText): who says it, and its look.</summary>
		public static void DialogueText(GlobalScope.dgs.DGSMessage message, int number, string text)
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
			if (_dialogue == null) return;
			_dialogue.Text = message;
			Refresh(_dialogue);
		}

		/// <summary>
		/// A text that comes whole (Game.Dialogue.Say's) broken into lines at the words that pass the layout's text frame's width,
		/// measured in its size and lettering; its own line breaks kept. The game's lines keep the breaks they were written with.
		/// </summary>
		public static string Wrap(string text, int fontSize)
		{
			if (string.IsNullOrEmpty(text) || !Styled || !_rects.TryGetValue("dialogue/text", out BattleHud.Rect frame) || frame.Width <= 0) return text;
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
			_dialogue.Name = message;
			Refresh(_dialogue);
		}

		// ---- the page-turn arrow ----

		private static GlobalScope.sys2d.Sprite3d _nextIcon;
		private static bool _nextWanted;
		private static readonly MenuPanels _nextPanels = new MenuPanels();
		// In front of every panel the layout's frames draw (Refresh says where that is).
		private static int _frontDepth;

		private static MenuStyles.Look NextLook => _dialogue?.Looks.FirstOrDefault(kv => PathOf(kv.Key) == "dialogue/next").Value;

		/// <summary>Whether the game's own arrow is drawn: not when the layout hides it or gives it a picture of its own.</summary>
		public static bool NextGame => NextLook is not MenuStyles.Look look || (!look.Hidden && look.Background == null);

		/// <summary>The game wants its arrow up or down (MessageWindow): a picture of the layout's in its place follows it.</summary>
		public static void NextShown(bool show)
		{
			_nextWanted = show;
			PutNext();
		}

		/// <summary>The arrow in front of the layout's panels, in its tint and opacity; or the next frame's own picture while the game's would be up.</summary>
		private static Built _next;

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
				}
				if (_next != null) { MenuPanels.Fade(_next.Sprites, look.Opacity); _next.Look = look; }
				else if (picture && _rects.TryGetValue("dialogue/next", out BattleHud.Rect r))
				{
					_next = new Built { Look = look };
					_nextPanels.Add(_directory, "dialogue/next", r.X, r.Y, r.Width, r.Height, look.Background, look.Opacity, _frontDepth, _next.Sprites, _next.Textures);
				}
			}
			catch (Exception) { }
		}

		public static void DialogueClosed() => Close(ref _dialogue);

		/// <summary>The map-name banner came up (MapNameWindow.open), its text in it.</summary>
		public static void BannerMade(GlobalScope.menu.BasicWindow window, GlobalScope.dgs.DGSMessage text, int number)
		{
			Close(ref _banner);
			Banner.Number = number;
			try { Banner.Text = number >= 0 ? GlobalScope.dgs.msg.CMessageSys.getInstance().Main().getMessage((uint)number) : null; } catch (Exception) { Banner.Text = null; }
			if (!Styled) return;
			_banner = new Shown { Root = "map_name", Window = window, Open = () => true, Text = text, TextFrame = "map_name" };
			PutWindowLook(_banner);
			Refresh(_banner);
		}

		public static void BannerClosed() => Close(ref _banner);

		/// <summary>Every frame while a window of the HUD's is up: its panels once it is open, and what moves (transitions, animations).</summary>
		public static void Tick()
		{
			_nextPanels.Flush();
			foreach (Shown s in new[] { _dialogue, _banner })
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
		}

		// ---- the looks put on ----

		private static string PathOf(XElement frame) => frame != null && _pathOf.TryGetValue(frame, out string p) ? p : null;

		private static IEnumerable<XElement> Subtree(string root)
		{
			XElement top = _source?.Elements("frame").FirstOrDefault(f => PathOf(f) == root);
			return top == null ? Enumerable.Empty<XElement>() : top.DescendantsAndSelf("frame");
		}

		/// <summary>The game's window as the root frame's look has it: taken away (-ff-panel: none), the bar, opacity and tint.</summary>
		private static void PutWindowLook(Shown s)
		{
			Dictionary<XElement, MenuStyles.Look> looks = Looks(s);
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

		/// <summary>The window's frames' looks and data put on: its game text's look, its frames' texts, and - once it is open - its panels (made again where they changed).</summary>
		private static void Refresh(Shown s)
		{
			if (_source == null) return;
			Bind(s);
			Dictionary<XElement, MenuStyles.Look> looks = Looks(s);
			bool open = s.Built || s.Open();
			XElement top = Subtree(s.Root).FirstOrDefault();
			bool rootMoved = top == null || !s.Looks.TryGetValue(top, out MenuStyles.Look rootWas) || !looks.TryGetValue(top, out MenuStyles.Look rootNow) || !rootWas.SameAs(rootNow);
			s.Looks = looks;
			if (rootMoved) PutWindowLook(s);
			XElement textFrame = Subtree(s.Root).FirstOrDefault(f => PathOf(f) == s.TextFrame);
			if (s.Text != null && textFrame != null && looks.TryGetValue(textFrame, out MenuStyles.Look textLook)) PutText(s.Text, textLook);
			XElement nameFrame = Subtree(s.Root).FirstOrDefault(f => PathOf(f) == "dialogue/name");
			if (s.Name != null && nameFrame != null && looks.TryGetValue(nameFrame, out MenuStyles.Look nameLook)) PutText(s.Name, nameLook);
			if (!open) return;
			int depth = 0, step = GlobalScope.ds.S32toFX32(32);
			try { depth = s.Window.GetDepth(); } catch (Exception) { }
			int place = 0;
			foreach (XElement frame in Subtree(s.Root))
			{
				string path = PathOf(frame);
				if (path == "dialogue/next") continue;   // the arrow's (PutNext)
				bool game = path == s.Root || path == "dialogue/text" || path == "dialogue/name";
				// Each frame of the mod's a step in front of the one before, the game's window at the back.
				int back = game && path == s.Root ? depth : depth - step * ++place;
				looks.TryGetValue(frame, out MenuStyles.Look look);
				bool shown = look != null && !look.Hidden && _rects.ContainsKey(path);
				s.Frames.TryGetValue(frame, out Built built);
				if (built != null && shown && SameBox(built.Look, look))
				{
					// The same box: a fade is put on in place.
					if (Math.Abs(built.Look.Opacity - look.Opacity) > 0.001)
					{
						MenuPanels.Fade(built.Sprites, look.Opacity);
						try { built.Window?.SetLook((float)look.Opacity, null); } catch (Exception) { }
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
				built = new Built { Look = look };
				s.Frames[frame] = built;
				if (!shown) continue;
				BattleHud.Rect r = _rects[path];
				if (!game && look.Window)
				{
					GlobalScope.menu.BasicWindow window = new GlobalScope.menu.BasicWindow();
					window.bwCreateUL(GlobalScope.sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, new GlobalScope.ds.Vector2<short>((short)r.X, (short)r.Y), new GlobalScope.ds.Vector2<short>((short)r.Width, (short)r.Height), 3);
					window.SetPriority(3);
					window.SetStackDepth(back);
					if (look.Bar) window.SetBarStyle();
					if (look.Opacity < 1) window.SetLook((float)look.Opacity, null);
					window.SetShow(show: true, user: true);
					built.Window = window;
				}
				if (look.Background != null) s.Panels.Add(_directory, path, r.X, r.Y, r.Width, r.Height, look.Background, look.Opacity, back + GlobalScope.ds.S32toFX32(8), built.Sprites, built.Textures);
			}
			foreach (Extra e in s.Extras) PutExtra(e, looks);
			if (s == _dialogue)
			{
				int front = 0;
				try { front = s.Window.GetDepth() - GlobalScope.ds.S32toFX32(32) * (Subtree(s.Root).Count() + 1); } catch (Exception) { }
				_frontDepth = front;
				PutNext();
			}
			s.Built = true;
		}

		/// <summary>The frames' bindings under the HUD's data: their texts (for the extras), shown or not, classes and bound styles. True when a class or a style changed (the looks are worked out again).</summary>
		private static bool Bind(Shown s)
		{
			MenuBindingScope scope = new MenuBindingScope { Root = Root };
			bool moved = false;
			foreach (XElement frame in Subtree(s.Root))
			{
				string path = PathOf(frame);
				bool game = path == s.Root || path == "dialogue/text" || path == "dialogue/name" || path == "dialogue/next";
				if (!game && IsText(frame) && !s.Extras.Any(e => e.Frame == frame)) s.Extras.Add(new Extra { Frame = frame, Path = path });
				try
				{
					foreach (KeyValuePair<string, string> c in MenuStyles.Declarations((string)frame.Attribute("bind-class")))
						moved |= SetClass(frame, c.Key, MenuBindings.Test(c.Value, scope));
					string own = _ownStyle.TryGetValue(frame, out string o) ? o : null;
					string bound = frame.Attribute("bind-style") is XAttribute bs ? MenuBindings.Format(bs.Value, scope) : null;
					string visible = frame.Attribute("bind-visible") is XAttribute bv ? (MenuBindings.Test(bv.Value, scope) ? null : "visibility: hidden") : null;
					string style = string.Join("; ", new[] { own, bound, visible }.Where(x => !string.IsNullOrWhiteSpace(x)));
					if (style != ((string)frame.Attribute("style") ?? "")) { frame.SetAttributeValue("style", style.Length == 0 ? null : style); moved = true; }
				}
				catch (Exception ex) { Log.Write(LogChannel.General, "field hud: " + path + ": " + ex.Message); }
			}
			foreach (Extra e in s.Extras)
			{
				string text = e.Frame.Attribute("bind-text") is XAttribute bt ? MenuBindings.Format(bt.Value, scope) : ((string)e.Frame.Element("data"))?.Trim();
				if (text != e.Text) { e.Text = text; ReleaseExtra(e); }
			}
			return moved;
		}

		private static bool IsText(XElement frame) => frame.Attribute("bind-text") != null || (string)frame.Element("behavior")?.Attribute("value") == "Text" || !string.IsNullOrWhiteSpace((string)frame.Element("data"));

		private static bool SetClass(XElement frame, string name, bool on)
		{
			HashSet<string> classes = new HashSet<string>(((string)frame.Attribute("class") ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries), StringComparer.Ordinal);
			if (!(on ? classes.Add(name) : classes.Remove(name))) return false;
			frame.SetAttributeValue("class", classes.Count == 0 ? null : string.Join(" ", classes));
			return true;
		}

		/// <summary>The roots the HUD's bindings reach: dialogue, banner, and the game's hero, party and gil.</summary>
		private static (bool, object) Root(string name)
		{
			switch (name.ToLowerInvariant())
			{
				case "dialogue": return (true, Dialogue);
				case "banner": return (true, Banner);
				case "hero": { IReadOnlyList<PartyMember> m = OpenFF.Game.Party?.Members; return (true, m != null && m.Count > 0 ? m[0] : null); }
				case "party": return (true, OpenFF.Game.Party?.Members);
				case "gil": return (true, OpenFF.Game.Party?.Gil ?? 0);
				default: return (false, null);
			}
		}

		/// <summary>The layout's looks now: the cascade (its sheets, its frames' bound classes and styles) and what is moving.</summary>
		private static Dictionary<XElement, MenuStyles.Look> Looks(Shown s)
		{
			List<(XElement Frame, Dictionary<string, string> Computed)> computed = MenuStyles.Computed(_source, _sheet);
			Dictionary<XElement, Dictionary<string, string>> shown = _animator.Step(computed, _sheet, _clock.Elapsed.TotalSeconds);
			return MenuStyles.Looks(_source, computed, shown);
		}

		/// <summary>A text of the game's (the dialogue's, the banner's) with a frame's look: its colour, opacity, size and lettering.</summary>
		private static void PutText(GlobalScope.dgs.DGSMessage message, MenuStyles.Look look)
		{
			try
			{
				GlobalScope.NNSG2dTextCanvas canvas = message.m_TextCanvas;
				if (canvas == null) return;
				if (ModMenus.StyleRgb(look.Colour) is uint rgb) canvas.rgba = rgb << 8 | 0xFF;
				else if (!string.IsNullOrWhiteSpace(look.Colour)) { canvas.rgba = null; message.setMessageColor((GlobalScope.dgs.TXT_COLOR)(int)ModMenus.ColourWord(look.Colour)); }
				canvas.alpha = (byte)Math.Round(Math.Clamp(look.Opacity, 0, 1) * 255);
				MenuText lettering = Lettering(look.TextStyle);
				int size = int.TryParse(look.Font, out int n) && n >= 6 && n <= 31 ? n : look.Font == "large" ? 16 : 0;
				if (size > 0 || lettering != null || canvas.style != null)
				{
					canvas.style = lettering;
					canvas.pFont = new GlobalScope.NNSG2dFont { size = size > 0 ? size : canvas.pFont?.size ?? 12, style = lettering };
				}
				message.setVisibility(!look.Hidden);
				message.Redraw();
			}
			catch (Exception) { }
		}

		/// <summary>A frame of the mod's with text: a message of its own at the frame's place (its alignment across, centred down), in its look.</summary>
		private static void PutExtra(Extra e, Dictionary<XElement, MenuStyles.Look> looks)
		{
			if (!looks.TryGetValue(e.Frame, out MenuStyles.Look look) || !_rects.TryGetValue(e.Path, out BattleHud.Rect r)) return;
			GlobalScope.dgs.msg.CMessageMng mm = GlobalScope.dgs.msg.CMessageSys.getInstance().Main();
			if (e.Message < 0)
			{
				if (string.IsNullOrEmpty(e.Text)) return;
				e.Message = mm.createMessage(e.Text, (ushort)r.X, (ushort)r.Y, GlobalScope.dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_COMMON, GlobalScope.dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_12x12);
				if (e.Message < 0) return;
				GlobalScope.dgs.DGSMessage made = mm.Message(e.Message);
				made?.setDisplaySpeed(byte.MaxValue);
				made?.setShadow(b: true);
			}
			GlobalScope.dgs.DGSMessage message = mm.Message(e.Message);
			if (message == null) return;
			PutText(message, look);
			GlobalScope.ds.Vector2<short> size = new GlobalScope.ds.Vector2<short>();
			message.getCompleteTextSize(size);
			string align = (look.Align ?? "").Trim();
			int x = align == "right" ? r.X + r.Width - size.vx : align == "center" ? r.X + (r.Width - size.vx) / 2 : r.X;
			int y = r.Height > 0 ? r.Y + (r.Height - size.vy) / 2 : r.Y;
			message.setPosition((short)x, (short)y, erase: true);
		}

		private static void ReleaseExtra(Extra e)
		{
			if (e.Message < 0) return;
			try { GlobalScope.dgs.msg.CMessageSys.getInstance().Main().releaseMessage(e.Message); } catch (Exception) { }
			e.Message = -1;
		}

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
			s.Panels.Clear();
			foreach (Built b in s.Frames.Values) { try { b.Window?.Release(); } catch (Exception) { } }
			foreach (Extra e in s.Extras) ReleaseExtra(e);
			s = null;
		}
	}
}
