// The place-name window with a mod's text in it (Game.Draw.Banner).
//
// When a map's name arrives at the top of the screen, it comes in a BasicWindow - the game's
// own frame, m000_window's corners and edges, half-clear over a battle - at the field_hud
// layout's map_name frame. A mod that wants to say something in that same voice ("Waiting for
// Arc...") asks for a Banner every frame it should show; the window is made from the same
// pieces at the same place, and goes when the asking stops. The text is drawn over it by the
// client's font, centred, with the shadow the place name has.

using System;

namespace OpenFF.Client
{
	internal static class Banner
	{
		private static GlobalScope.menu.BasicWindow _window;
		private static string _text;
		private static int _keptAt = -1;
		private static int _frame;

		/// <summary>The text asked for this frame (ModDraw, from the draw list).</summary>
		public static void Keep(string text)
		{
			_text = text;
			_keptAt = _frame;
		}

		/// <summary>Every frame, after the draw list: the window opens on a first ask, closes when a frame passes without one.</summary>
		public static void Tick()
		{
			_frame++;
			bool wanted = _keptAt == _frame - 1 && !string.IsNullOrEmpty(_text);
			if (wanted && _window == null) Open();
			else if (!wanted && _window != null) Close();
		}

		/// <summary>Where the text goes, in the 800x480 text space, and what it says; null when no banner is up.</summary>
		public static (float X, float Y, float W, float H, string Text)? Shown
		{
			get
			{
				if (_window == null || string.IsNullOrEmpty(_text)) return null;
				BattleHud.Rect r = BattleHud.MapName();
				float ox = (480 - GlobalScope.LCD_WIDTH) / 2f, oy = (320 - GlobalScope.LCD_HEIGHT) / 2f;
				return ((r.X - ox) / GlobalScope.LCD_WIDTH * 800f, (r.Y - oy) / GlobalScope.LCD_HEIGHT * 480f, r.Width / (float)GlobalScope.LCD_WIDTH * 800f, r.Height / (float)GlobalScope.LCD_HEIGHT * 480f, _text);
			}
		}

		private static void Open()
		{
			// The window pieces are loaded by the field's map-name window and the battle part; nowhere else can hold one.
			if (!EngineApi.InWorld && !OpenFF.Game.Battle.InBattle) return;
			try
			{
				BattleHud.Rect r = BattleHud.MapName();
				GlobalScope.ds.Vector2<short> centre = new GlobalScope.ds.Vector2<short>((short)(r.X + r.Width / 2), (short)(r.Y + r.Height / 2));
				GlobalScope.ds.Vector2<short> size = new GlobalScope.ds.Vector2<short>((short)r.Width, (short)r.Height);
				_window = new GlobalScope.menu.BasicWindow();
				_window.Initialize();
				_window.bwCreateCC(GlobalScope.sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, centre, size, 3);
				_window.SetPriority(3);
				_window.SetShow(show: true, user: true);
				Log.Write(LogChannel.File, "banner: \"" + _text + "\"");
			}
			catch (Exception ex)
			{
				Log.First(LogChannel.General, "banner", 3, () => "banner: " + ex.Message);
				_window = null;
			}
		}

		private static void Close()
		{
			try
			{
				_window.SetShow(show: false, user: true);
				_window.Release();
			}
			catch (Exception) { }
			_window = null;
			Log.Write(LogChannel.File, "banner: down");
		}

		/// <summary>The part changed (field to battle or back): the window's pieces went with it.</summary>
		public static void PartChanged()
		{
			if (_window == null) return;
			try { _window.Release(); } catch (Exception) { }
			_window = null;
		}
	}
}
