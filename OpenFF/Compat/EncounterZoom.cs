// FF4's random-encounter transition (world::WSEncountDirection1 and world::Encount, read from libff4.so): the field
// stops, the menu's sound (0, 10) plays and the music fades over 15 frames; then the field camera's field of view
// closes - from the 5th frame of the zoom at -174 (16-bit angle units) a frame, 8 faster each frame to at most 250, for
// 12 frames, then a quarter of it the other way, slowing back into the zoom - while each frame the DS captures the
// screen at 2/16 over 14/16 of the last capture (a long trail) and draws white rings, faint (alpha 8/31), one more a
// frame, ring j at 2j^2 of the DS's 256 across; 15 frames on, the screen fades to white over 15 and the battle comes.
//
// Here as a screen effect: a narrower field of view is the same picture scaled about its centre (tan a0 / tan a), so the
// field is read once as the zoom starts and drawn scaled each frame, the rings over it, into a picture that keeps 14/16
// of the last; the ring is the game's own ring.ntxp (64 x 64, A5I3: the alpha in each byte's top five bits).

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenFF.Client
{
	using Color = Microsoft.Xna.Framework.Color;
	using Vector2 = Microsoft.Xna.Framework.Vector2;
	using Rectangle = Microsoft.Xna.Framework.Rectangle;

	internal sealed class EncounterZoom : DrawableGameComponent
	{
		private const int StartSpeed = -174, Accel = -8, MostSpeed = -250, Rebound = 1024, ZoomFrames = 12, HoldFrames = 15, FadeFrames = 15;

		private static EncounterZoom _instance;
		private static int _state = -1;      // 1 the frame before the capture, 2 the zoom, 3 the rebound, 4 the fade, -1 idle
		private static int _count, _rings;
		private static bool _zooming;
		private static int _speed, _angle, _angle0;
		private static long _lastStep = -1;
		private static Action _then;

		private Texture2D _capture, _ring, _white;
		private Color[] _pixels;
		private RenderTarget2D _trail, _frame;
		private SpriteBatch _batch;
		private bool _haveCapture;

		private EncounterZoom(Microsoft.Xna.Framework.Game game) : base(game) { DrawOrder = int.MaxValue - 1; }

		public static void Attach(Microsoft.Xna.Framework.Game game)
		{
			_instance = new EncounterZoom(game);
			game.Components.Add(_instance);
		}

		/// <summary>Whether it is playing.</summary>
		public static bool Playing => _state >= 0;

		/// <summary>The transition starts; then runs once the screen is white (the battle).</summary>
		public static void Start(Action then)
		{
			_then = then;
			_state = 1;
			_count = 0;
			_rings = 0;
			_zooming = false;
			_lastStep = LegacyStep.Count;
			// WSEncountDirection1::prepare: the camera's half field of view as an angle index.
			_angle0 = 2731;
			try
			{
				GlobalScope.ds.sys3d.CCamera camera = GlobalScope.CCastCommandTransit.getInstance().cast_BaseSystem().WorldCamera();
				camera.getFOV(out int sin, out int cos);
				if (cos > 0) _angle0 = (int)Math.Round(Math.Atan2(sin, cos) * 65536.0 / (2 * Math.PI));
			}
			catch (Exception) { }
			_angle = _angle0;
			if (_instance != null) _instance._haveCapture = false;
			try { GlobalScope.MatrixSound.MtxSENDS_Play(0, 10, 127, 64); } catch (Exception) { }
			try { GlobalScope.MatrixSound.MtxSoundBGM.getSingleton().stop(15, GlobalScope.MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0); } catch (Exception) { }
			Log.Write(LogChannel.File, "encounter: zoom at step " + LegacyStep.Count + " (half fov index " + _angle0 + ")");
		}

		/// <summary>Encount::execute, a game step of it.</summary>
		private static void Step()
		{
			_count++;
			_rings += 2;
			switch (_state)
			{
				case 1:
					if (_count >= 2) { _state = 2; _count = 0; _rings = 0; }
					break;
				case 2:
					if (_count == 5) { _zooming = true; _speed = StartSpeed; }
					if (_count >= ZoomFrames) { _speed = -(Rebound * _speed) >> 12; _count = 0; _state = 3; }
					break;
				case 3:
					if (_count >= HoldFrames)
					{
						GlobalScope.dgs.CFade.Main().fadeOut(FadeFrames, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_WHITE);
						GlobalScope.dgs.CFade.Sub().fadeOut(FadeFrames, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_WHITE);
						_count = 0;
						_state = 4;
					}
					break;
				case 4:
					if (GlobalScope.dgs.CFade.Main().isFaded())
					{
						_state = -1;
						Action then = _then;
						_then = null;
						try { then?.Invoke(); } catch (Exception ex) { Log.Write(LogChannel.General, "encounter: " + ex.Message); }
						return;
					}
					break;
			}
			if (_zooming)
			{
				_speed = Math.Max(_speed + Accel, MostSpeed);
				_angle = Math.Max(_angle + _speed, 0x32);
			}
		}

		public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
		{
			if (_state < 0) return;
			long step = LegacyStep.Count;
			while (_lastStep < step && _state >= 0) { _lastStep++; Step(); }
			if (_state < 2) return;
			GraphicsDevice device = GraphicsDevice;
			int w = device.PresentationParameters.BackBufferWidth, h = device.PresentationParameters.BackBufferHeight;
			try
			{
				_batch ??= new SpriteBatch(device);
				if (_capture == null || _capture.Width != w || _capture.Height != h)
				{
					_capture?.Dispose(); _trail?.Dispose(); _frame?.Dispose();
					_capture = new Texture2D(device, w, h, false, SurfaceFormat.Color);
					_trail = new RenderTarget2D(device, w, h, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
					_frame = new RenderTarget2D(device, w, h, false, SurfaceFormat.Color, DepthFormat.None);
					_pixels = new Color[w * h];
					_haveCapture = false;
				}
				_ring ??= LoadRing(device);
				if (!_haveCapture)
				{
					// The field as the capture starts: what zooms from here on, and the first picture of the trail.
					device.GetBackBufferData(_pixels);
					_capture.SetData(_pixels);
					device.SetRenderTarget(_trail);
					_batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
					_batch.Draw(_capture, Vector2.Zero, Color.White);
					_batch.End();
					device.SetRenderTarget(null);
					_haveCapture = true;
				}
				// This frame: the field at the camera's field of view now, the rings over it.
				float scale = (float)(Math.Tan(_angle0 * Math.PI * 2 / 65536) / Math.Tan(Math.Max(0x32, _angle) * Math.PI * 2 / 65536));
				Vector2 centre = new Vector2(w / 2f, h / 2f);
				device.SetRenderTarget(_frame);
				_batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
				_batch.Draw(_capture, centre, null, Color.White, 0f, centre, scale, SpriteEffects.None, 0f);
				_batch.End();
				if (_ring != null)
				{
					_batch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
					float unit = w / 256f;
					Color faint = new Color(255, 255, 255, 8 * 255 / 31);
					for (int j = 1; j <= 15 && 2 * j < _rings; j++)
					{
						float half = 2f * j * j * unit;
						_batch.Draw(_ring, new Rectangle((int)(centre.X - half), (int)(centre.Y - half), (int)(2 * half), (int)(2 * half)), faint);
					}
					_batch.End();
				}
				// The capture: 2/16 of this frame over 14/16 of the last.
				device.SetRenderTarget(_trail);
				_batch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
				_batch.Draw(_frame, Vector2.Zero, new Color(255, 255, 255, 2 * 255 / 16));
				_batch.End();
				device.SetRenderTarget(null);
				_batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
				_batch.Draw(_trail, Vector2.Zero, Color.White);
				_batch.End();
				// The master brightness over it all (the fade to white).
				int level = GlobalScope.dgs.CFade.Main().Level;
				if (level != 0)
				{
					if (_white == null) { _white = new Texture2D(device, 1, 1); _white.SetData(new[] { Color.White }); }
					byte a = (byte)Math.Min(255, Math.Abs(level) * 255 / 16);
					Color tone = level > 0 ? new Color((byte)255, (byte)255, (byte)255, a) : new Color((byte)0, (byte)0, (byte)0, a);
					_batch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
					_batch.Draw(_white, new Rectangle(0, 0, w, h), tone);
					_batch.End();
				}
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "encounter: zoom: " + ex.Message);
				try { device.SetRenderTarget(null); } catch (Exception) { }
			}
		}

		/// <summary>ring.ntxp's texture (its TEX0's data: 64 x 64 A5I3, white), or null.</summary>
		private static Texture2D LoadRing(GraphicsDevice device)
		{
			try
			{
				byte[] data = GameArchive.Chain.Read("files/ring.ntxp") ?? GameArchive.Chain.Read("ring.ntxp");
				if (data == null) return null;
				int tex = -1;
				for (int i = 0; i + 4 <= data.Length; i++) if (data[i] == 'T' && data[i + 1] == 'E' && data[i + 2] == 'X' && data[i + 3] == '0') { tex = i; break; }
				if (tex < 0) return null;
				int at = tex + BitConverter.ToInt32(data, tex + 0x14);
				if (at + 4096 > data.Length) return null;
				Color[] texels = new Color[4096];
				for (int i = 0; i < 4096; i++) texels[i] = new Color((byte)255, (byte)255, (byte)255, (byte)((data[at + i] >> 3) * 255 / 31));
				Texture2D ring = new Texture2D(device, 64, 64);
				ring.SetData(texels);
				return ring;
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.File, "encounter: ring.ntxp: " + ex.Message);
				return null;
			}
		}
	}
}
