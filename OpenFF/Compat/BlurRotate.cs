// FF4's encounter whirl: SPBlurRotate, which a story scene plays when ce_CallBattle notes a battle
// (EventConteManager::executeBattleEncount). The DS captures the main screen and shows it on an affine background that
// turns about the screen's centre, 0x13e9 (about 28 degrees) more each frame, wrapping at its edges; from the 18th
// frame the master brightness climbs to white. Steam's frames (Tools/ff4hook, every frame from ce_CallBattle at 3490):
// the scene's last shot still to 3498, one sharp turned copy a frame from 3500, a double image as it whitens from 3516,
// white at 3528, the battle fading in from 3552. Here the back buffer is read once as the whirl starts and drawn
// turned (and wrapped) over every frame after, the frame before's turn under it from the white on, the brightness on
// top.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenFF.Client
{
	internal sealed class BlurRotate : DrawableGameComponent
	{
		// Steam's frames (ce_CallBattle at 3490): the spin from 3500, the white from 3516, all white at 3528.
		private const int LeadFrames = 8, SpinFrames = 32, WhiteFrom = 16, WhiteFrames = 12;
		private const int AngleStep = 0x13e9;

		private static BlurRotate _instance;
		private static int _count = -1;     // game steps since the start; -1 when idle
		private static long _lastStep = -1;
		private static bool _whitened;

		private Texture2D _capture;
		private Microsoft.Xna.Framework.Color[] _pixels;
		private SpriteBatch _batch;
		private bool _haveCapture;
		private Texture2D _white;

		private BlurRotate(Microsoft.Xna.Framework.Game game) : base(game) { }

		public static void Attach(Microsoft.Xna.Framework.Game game)
		{
			_instance = new BlurRotate(game);
			game.Components.Add(_instance);
		}

		/// <summary>The whirl starts (ce_CallBattle in a scene).</summary>
		public static void Start()
		{
			_count = 0;
			_whitened = false;
			_lastStep = LegacyStep.Count;
			if (_instance != null) _instance._haveCapture = false;
			Log.Write(LogChannel.File, "encounter: blur-rotate at step " + LegacyStep.Count);
		}

		/// <summary>Whether it is playing.</summary>
		public static bool Playing => _count >= 0 && _count < LeadFrames + SpinFrames;

		/// <summary>Stops it (a map change, the battle).</summary>
		public static void Stop() => _count = -1;

		public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
		{
			if (_count < 0) return;
			// Counted by the game's steps (a catch-up runs several in one draw).
			long step = LegacyStep.Count;
			if (step != _lastStep)
			{
				_count += (int)Math.Max(1, step - _lastStep);
				_lastStep = step;
				int spin = _count - LeadFrames;
				if (spin >= WhiteFrom && !_whitened)
				{
					_whitened = true;
					// GX_SetMasterBrightness(count - 17) from here: white a step a frame, as a white fade-out shows it.
					GlobalScope.dgs.CFade.Main().fadeOut(WhiteFrames, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_WHITE);
					GlobalScope.dgs.CFade.Sub().fadeOut(WhiteFrames, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_WHITE);
				}
			}
			int frame = _count - LeadFrames;
			if (frame < 0) return;
			if (frame >= SpinFrames)
			{
				_count = -1;
				return;
			}
			GraphicsDevice device = GraphicsDevice;
			int w = device.PresentationParameters.BackBufferWidth, h = device.PresentationParameters.BackBufferHeight;
			try
			{
				if (_capture == null || _capture.Width != w || _capture.Height != h)
				{
					_capture?.Dispose();
					_capture = new Texture2D(device, w, h, false, SurfaceFormat.Color);
					_pixels = new Microsoft.Xna.Framework.Color[w * h];
					_haveCapture = false;
				}
				_batch ??= new SpriteBatch(device);
				if (!_haveCapture)
				{
					// The screen as the whirl starts: what turns from here on (Steam's frames: the scene stops, its last
					// shot spins).
					device.GetBackBufferData(_pixels);
					_capture.SetData(_pixels);
					_haveCapture = true;
					return;
				}
				// The rotated copy fills the screen: the DS's affine background wraps, so the corners show the image again.
				float angle = (frame * AngleStep & 0xFFFF) * MathHelper.TwoPi / 65536f;
				Microsoft.Xna.Framework.Vector2 centre = new Microsoft.Xna.Framework.Vector2(w / 2f, h / 2f);
				Microsoft.Xna.Framework.Rectangle source = new Microsoft.Xna.Framework.Rectangle(-w, -h, 3 * w, 3 * h);
				Microsoft.Xna.Framework.Vector2 origin = new Microsoft.Xna.Framework.Vector2(1.5f * w, 1.5f * h);
				_batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone);
				_batch.Draw(_capture, centre, source, Microsoft.Xna.Framework.Color.White, -angle, origin, 1f, SpriteEffects.None, 0f);
				_batch.End();
				if (frame >= WhiteFrom)
				{
					// From the white on, the turn before shows through (Steam's double image as it whitens).
					float before = ((frame - 1) * AngleStep & 0xFFFF) * MathHelper.TwoPi / 65536f;
					_batch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone);
					_batch.Draw(_capture, centre, source, new Microsoft.Xna.Framework.Color(255, 255, 255, 128), -before, origin, 1f, SpriteEffects.None, 0f);
					_batch.End();
				}
				// The master brightness over it all (the fade the whirl started: drawn under this frame, covered by it).
				int level = GlobalScope.dgs.CFade.Main().Level;
				if (level != 0)
				{
					if (_white == null) { _white = new Texture2D(device, 1, 1); _white.SetData(new[] { Microsoft.Xna.Framework.Color.White }); }
					byte a = (byte)Math.Min(255, Math.Abs(level) * 255 / 16);
					Microsoft.Xna.Framework.Color tone = level > 0 ? new Microsoft.Xna.Framework.Color((byte)255, (byte)255, (byte)255, a) : new Microsoft.Xna.Framework.Color((byte)0, (byte)0, (byte)0, a);
					_batch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
					_batch.Draw(_white, new Microsoft.Xna.Framework.Rectangle(0, 0, w, h), tone);
					_batch.End();
				}
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "encounter: blur-rotate: " + ex.Message);
				_count = -1;
			}
		}
	}
}
