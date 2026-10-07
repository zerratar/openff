// FF4's field message window, drawn as a layout (LayoutScreen "ff4_field_hud": Data/hud/ff4_field_hud.xml and its
// styles/ff4_field_hud.css) - so a mod reshapes or restyles it as it does the battle HUD, with menus/hud/ff4_field_hud.xml
// or a sheet in menus/hud/styles/. The game's own window (menu.MessageWindow) still does all it does - opens, types its
// texts on page by page, waits for the press, holds a line the script holds - and draws nothing; this reads what it shows
// and binds it to the root "dialogue":
//   dialogue.open        the window is up (not the story scenes' bar); openX, openY how far it has opened (0..1: it
//                        grows from its middle over five frames, MessageWindow::mwOpen)
//   dialogue.text        the text typed so far, its lines as the game breaks them
//   dialogue.speaker     the name window's name (openCharacterNameWindow), or null; nameWidth its window's width in the
//                        layout's pixels: the drawn name and 27 pixels either side, as NameWindow::nwOpen pads it
//   dialogue.next        the page-turn arrow is wanted
//   dialogue.inPlace     the text starts where the window's does; placed when the script put the line elsewhere
//                        (setMessagePosition / setMessageAlignment): lineLeft, lineTop where its frame goes, lineAlign
//                        (0 left, 1 centred, 2 right) and lineDown (0 top, 1 middle, 2 bottom) how it stands on that place
//   dialogue.colour      the text's colour, the game's number (1 white, 3 red, 4 green, 5 blue, 6 cyan, 7 magenta, 8 yellow)
//
// The layout is in Steam's 1080p pixels, as the battle HUD's: a place of the game's 480 x 320 screen is Steam's on a 16:9
// window - the 480 in the middle of 569, 3.375 pixels a unit.

using System;

namespace OpenFF.Client
{
	internal sealed class Ff4Dialogue : GameService
	{
		public const string ScreenId = "ff4_field_hud";

		public sealed class DialogueData
		{
			public bool Open, Next, InPlace = true, Placed;
			public string Text = "", Speaker;
			public float NameWidth, LineLeft, LineTop, OpenX = 1, OpenY = 1;
			public int LineAlign, LineDown, Colour = 1;
		}

		private static LayoutScreen _screen;
		private static bool _loaded;
		private static readonly DialogueData _data = new DialogueData();

		/// <summary>Whether FF4's message window is the layout's to draw (the layout is there).</summary>
		public static bool Drawn
		{
			get
			{
				if (!GameProfile.IsFf4) return false;
				if (!_loaded)
				{
					_loaded = true;
					Ff4Ui.RegisterLayoutCells();
					_screen = LayoutScreen.Load(ScreenId);
				}
				return _screen != null;
			}
		}

		// A unit of the 480 x 320 screen in the layout's pixels, and where the 480 starts on Steam's 16:9 (569 wide).
		private const float Unit = 3.375f, Left = (1920f / Unit - 480f) / 2f;

		public override bool WantsUpdate => true;

		public override void OnUpdate()
		{
			if (!Drawn) return;
			GlobalScope.menu.MessageWindow window = null;
			try { window = GlobalScope.CCastCommandTransit.getInstance().cast_Field2D()?.MessageWindow()?.Window; } catch (Exception) { }
			if (window == null || !window.Ff4Open) return;
			Read(window);
			// The game's windows are under the screen's fade (the DS's master brightness): this one fades with them.
			int level = 0;
			try { level = Math.Abs(GlobalScope.dgs.CFade.Main().Level); } catch (Exception) { }
			_screen.Opacity = 1f - Math.Min(16, level) / 16f;
			if (_screen.Opacity <= 0f) return;
			_screen.Draw(OpenFF.Game.Draw, name => string.Equals(name, "dialogue", StringComparison.OrdinalIgnoreCase)
				? (true, _data)
				: (OpenFF.Game.Hud.TryGet(name, out object hud) ? (true, hud) : (false, null)));
		}

		// The name as the sheet draws it (its .message: font-size 30 of the layout's pixels), and 27 pixels either side of it -
		// NameWindow::nwOpen's rule (the name's width and 18 units, the name 8 in) with Steam's measure of it: 207 for Baigan.
		private const float NameFont = 30f, NamePad = 27f;

		private static float NameWidth(string name)
		{
			float wanted = NameFont * DrawList.ScreenHeight / 1080f;
			int drawn = (int)Math.Ceiling(wanted - 0.001f);
			float width = OpenFF.Game.Draw.MeasureText(name, drawn) * (wanted / drawn) * 1080f / DrawList.ScreenHeight;
			return width + 2 * NamePad;
		}

		private static void Read(GlobalScope.menu.MessageWindow window)
		{
			DialogueData d = _data;
			d.Open = true;
			(d.OpenX, d.OpenY) = window.Ff4Openness;
			d.Text = window.Ff4Text ?? "";
			d.Next = window.Ff4Next;
			int who = window.Ff4NameWho;
			d.Speaker = null;
			d.NameWidth = 0;
			if (who >= 0)
			{
				try { d.Speaker = GlobalScope.dgs.msg.CMessageSys.getInstance().Main().getMessage((uint)who)?.Trim(); } catch (Exception) { }
				if (string.IsNullOrEmpty(d.Speaker)) d.Speaker = null;
				else d.NameWidth = NameWidth(d.Speaker);
			}
			(int x, int y, int align, int down, int colour) = window.Ff4Line;
			d.Colour = colour;
			d.LineAlign = align;
			d.LineDown = down;
			d.Placed = x != 12 || y != 252 || align != 0 || down != 0;
			d.InPlace = !d.Placed;
			// The line's frame is the layout's width across and 100 high: its left so the place is its left, middle or right,
			// its top so the place is its top, middle or bottom.
			float px = (x + Left) * Unit, py = y * Unit;
			d.LineLeft = px - (align == 1 ? 960f : align == 2 ? 1920f : 0f);
			d.LineTop = py - (down == 1 ? 50f : down == 2 ? 100f : 0f);
		}
	}
}
