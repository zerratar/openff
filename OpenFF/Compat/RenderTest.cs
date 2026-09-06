// Isolated render harness for the GL emulation.
//
//   --test=3d     textured + vertex-coloured quads through a perspective projection
//   --test=2d     the same geometry through the game's 2D orthographic projection
//
// Enabled, this draws over whatever the game rendered, using the SAME emulation
// entry points the game uses (glMatrixMode / glLoadMatrixf / glBindTexture /
// glTexImage2D / glDrawArrays). That separates two very different questions:
//
//   * nothing appears here  -> the GL emulation itself is broken
//   * geometry appears here -> the emulation works and the fault is in the scene
//                              data or the state the game leaves behind
//
// The perspective matrix is the real one captured from a field scene, so the test
// exercises the same numbers the game does.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenFF.Client
{
	// MonoGame's types by name: the engine's OpenFF.Game, GameTime, Color and vectors sit a namespace up.
	using Game = Microsoft.Xna.Framework.Game;
	using GameTime = Microsoft.Xna.Framework.GameTime;
	using Color = Microsoft.Xna.Framework.Color;
	using Vector2 = Microsoft.Xna.Framework.Vector2;
	using Vector3 = Microsoft.Xna.Framework.Vector3;

	internal sealed class RenderTest : DrawableGameComponent
	{
		/// <summary>True when a render test owns the frame; the game must not draw.</summary>
		public static bool Active { get; private set; }

		private readonly string _mode;
		private VertexPositionColorTexture[] _verts;
		private uint _texture;
		private bool _ready;
		private VertexPositionColorTexture[] _model;
		private Texture2D _modelTexture;
		private uint _modelTextureId;
		private Vector3 _min, _max;

		private RenderTest(Game game, string mode)
			: base(game)
		{
			_mode = mode;
			// After the game, before the screenshot grabber.
			DrawOrder = int.MaxValue - 2;
		}

		public static void Attach(Game game)
		{
			string mode = Options.Get("test");
			if (string.IsNullOrEmpty(mode))
			{
				return;
			}
			Active = true;
			game.Components.Add(new RenderTest(game, mode.ToLowerInvariant()));
			Log.Write(LogChannel.General, "render test mode: " + mode);
		}

		private void Build()
		{
			// A 64x64 checkerboard, opaque, so a black or transparent result is obvious.
			const int size = 64;
			byte[] pixels = new byte[size * size * 4];
			for (int y = 0; y < size; y++)
			{
				for (int x = 0; x < size; x++)
				{
					bool light = ((x / 8) + (y / 8)) % 2 == 0;
					int i = (y * size + x) * 4;
					pixels[i] = (byte)(light ? 255 : 40);        // R
					pixels[i + 1] = (byte)(light ? 80 : 200);    // G
					pixels[i + 2] = (byte)(light ? 40 : 255);    // B
					pixels[i + 3] = 255;                          // A
				}
			}

			uint[] ids = new uint[1];
			GlobalScope.glGenTextures(1, ids);
			_texture = ids[0];
			GlobalScope.glBindTexture(3553u, _texture);
			GlobalScope.glTexParameteri(3553u, 10241u, 9729);
			GlobalScope.glTexParameteri(3553u, 10240u, 9729);
			GlobalScope.glTexImage2D(3553u, 0, 6408, size, size, 0, 6408u, 5121u, pixels);

			// Two triangles. Camera-space coordinates in the same range the real scene
			// uses (the field map sits around z = -100 with extents of a few tens).
			// Four quads across the visible width at z = -100, drawn with four different
			// render states so one frame answers which state is eating the pixels.
			_verts = new VertexPositionColorTexture[24];
			for (int q = 0; q < 4; q++)
			{
				float cx = -30f + q * 20f;
				Quad(q * 6, cx - 8f, cx + 8f, -14f, 14f, -100f, Color.White);
			}

			_ready = true;
			Log.Write(LogChannel.General, "render test: texture id " + _texture + " built");
		}

		private void Quad(int at, float x0, float x1, float y0, float y1, float z, Color colour)
		{
			Set(at + 0, x0, y0, z, 0f, 1f, colour);
			Set(at + 1, x0, y1, z, 0f, 0f, colour);
			Set(at + 2, x1, y1, z, 1f, 0f, colour);
			Set(at + 3, x0, y0, z, 0f, 1f, colour);
			Set(at + 4, x1, y1, z, 1f, 0f, colour);
			Set(at + 5, x1, y0, z, 1f, 1f, colour);
		}

		private void Set(int i, float x, float y, float z, float u, float v, Color colour)
		{
			_verts[i].Position = new Vector3(x, y, z);
			_verts[i].TextureCoordinate = new Vector2(u, v);
			_verts[i].Color = colour;
		}

		public override void Draw(GameTime gameTime)
		{
			if (!_ready)
			{
				Build();
			}

			// Clearing the depth buffer is masked out by the current depth-write state.
			// Force writes on first, or glClear leaves the depth buffer untouched.
			if (_mode == "model")
			{
				DrawModel(gameTime);
				return;
			}

			GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer,
				new Color(20, 20, 60), 1.0f, 0);

			if (_mode == "2d")
			{
				// Known-good path: the game's own 2D projection.
				GlobalScope.glMatrixMode(5889u);
				GlobalScope.glLoadIdentity();   // glOrthof multiplies onto the current matrix
				GlobalScope.glOrthof(0f, 480f, 320f, 0f, -1024f, 1024f);
			}
			else
			{
				// The exact perspective matrix captured from a field scene, in the
				// column-major layout glLoadMatrixf expects.
				GlobalScope.glMatrixMode(5889u);
				GlobalScope.glLoadMatrixf(new float[16]
				{
					2.5210f, 0f, 0f, 0f,
					0f, 4.1992f, 0f, 0f,
					0f, 0f, -1.0200f, -1.0000f,
					0f, 0f, -10.101f, 0f
				});
			}

			GlobalScope.glMatrixMode(5888u);
			GlobalScope.glLoadIdentity();

			GlobalScope.glDisable(2884u);   // never cull, that is already ruled out
			GlobalScope.glEnable(3042u);
			GlobalScope.glBlendFunc(770u, 771u);
			GlobalScope.glEnable(3553u);
			GlobalScope.glBindTexture(3553u, _texture);

			// 1: depth on + LEQUAL, drawn FIRST, immediately after the clear. If this
			// fails the depth buffer is not holding the 1.0 it was cleared to.
			GlobalScope.glDisable(3008u);
			GlobalScope.glEnable(2929u);
			GlobalScope.glDepthFunc(515u);
			GlobalScope.glDepthMask(1);
			GlobalScope.glDrawArrays(4u, 0, 6, _verts);

			// 2: control - depth off entirely.
			GlobalScope.glDisable(2929u);
			GlobalScope.glDrawArrays(4u, 6, 6, _verts);

			// 3: depth on, GL_ALWAYS - ignores whatever the buffer holds.
			GlobalScope.glEnable(2929u);
			GlobalScope.glDepthFunc(519u);
			GlobalScope.glDrawArrays(4u, 12, 6, _verts);

			// 4: depth on + LEQUAL again, now after other draws.
			GlobalScope.glDepthFunc(515u);
			GlobalScope.glDrawArrays(4u, 18, 6, _verts);
		}

		/// <summary>
		/// Draws a batch captured out of the real game, framed to fit, with every piece
		/// of state that has been implicated so far switched off. If the shape and the
		/// texture look right here, the asset decoding is sound and the fault is state.
		/// </summary>
		private void DrawModel(GameTime gameTime)
		{
			if (_model == null)
			{
				_model = ModelCapture.Load(GraphicsDevice, out _modelTexture);
				if (_model == null)
				{
					GraphicsDevice.Clear(new Color(60, 20, 20));
					return;
				}

				_min = new Vector3(float.MaxValue);
				_max = new Vector3(float.MinValue);
				foreach (VertexPositionColorTexture vertex in _model)
				{
					_min = Vector3.Min(_min, vertex.Position);
					_max = Vector3.Max(_max, vertex.Position);
				}

				// Re-upload the texture through the emulation so the viewer exercises the
				// same texture path the game does.
				if (_modelTexture != null)
				{
					Color[] pixels = new Color[_modelTexture.Width * _modelTexture.Height];
					_modelTexture.GetData(pixels);
					byte[] rgba = new byte[pixels.Length * 4];
					for (int i = 0; i < pixels.Length; i++)
					{
						rgba[i * 4] = pixels[i].R;
						rgba[i * 4 + 1] = pixels[i].G;
						rgba[i * 4 + 2] = pixels[i].B;
						rgba[i * 4 + 3] = pixels[i].A;
					}
					uint[] ids = new uint[1];
					GlobalScope.glGenTextures(1, ids);
					_modelTextureId = ids[0];
					GlobalScope.glBindTexture(3553u, _modelTextureId);
					GlobalScope.glTexParameteri(3553u, 10241u, 9729);
					GlobalScope.glTexParameteri(3553u, 10240u, 9729);
					GlobalScope.glTexImage2D(3553u, 0, 6408, _modelTexture.Width,
						_modelTexture.Height, 0, 6408u, 5121u, rgba);
				}

				Log.Write(LogChannel.General, string.Format(
					"model viewer: {0} verts, bounds ({1:F1},{2:F1},{3:F1})..({4:F1},{5:F1},{6:F1})",
					_model.Length, _min.X, _min.Y, _min.Z, _max.X, _max.Y, _max.Z));
			}

			GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer,
				new Color(18, 18, 34), 1.0f, 0);

			Vector3 centre = (_min + _max) * 0.5f;
			float radius = Math.Max((_max - _min).Length() * 0.5f, 1f);

			if (_mode == "modelflat")
			{
				// Straight-down orthographic view: shows the whole floor plan at once.
				GlobalScope.glMatrixMode(5889u);
				GlobalScope.glLoadIdentity();
				GlobalScope.glOrthof(_min.X, _max.X, _max.Y, _min.Y, -100000f, 100000f);
				GlobalScope.glMatrixMode(5888u);
				GlobalScope.glLoadIdentity();
			}
			else
			{
				// Slow orbit so the shape reads as 3D. The geometry's Y axis points down
				// (NDS convention), so "up" for the camera is -Y.
				float angle = (float)gameTime.TotalGameTime.TotalSeconds * 0.4f;
				// Mostly from above, orbiting slowly. A shallower angle just shows the
				// outside of the cave shell, which is unlit and reads as a black
				// silhouette - the interior is what the game actually renders.
				Vector3 offset = new Vector3(
					(float)Math.Sin(angle) * 0.45f, -1.15f, (float)Math.Cos(angle) * 0.45f);
				Vector3 eye = centre + offset * radius;

				Matrix view = Matrix.CreateLookAt(eye, centre, new Vector3(0f, -1f, 0f));
				Matrix projection = Matrix.CreatePerspectiveFieldOfView(
					MathHelper.PiOver4,
					GraphicsDevice.Viewport.AspectRatio,
					Math.Max(radius * 0.02f, 0.1f),
					radius * 8f);

				GlobalScope.glMatrixMode(5889u);
				GlobalScope.glLoadMatrixf(ToArray(projection));
				GlobalScope.glMatrixMode(5888u);
				GlobalScope.glLoadMatrixf(ToArray(view));
			}

			GlobalScope.glDisable(3008u);   // no alpha test
			GlobalScope.glDisable(2884u);   // no culling
			GlobalScope.glEnable(3042u);
			GlobalScope.glBlendFunc(770u, 771u);

			// Depth ON: with a working depth buffer the cave should sort correctly.
			// If the model vanishes here, depth is broken in the harness too.
			GlobalScope.glEnable(2929u);
			GlobalScope.glDepthFunc(515u);
			GlobalScope.glDepthMask(1);

			if (_modelTexture != null)
			{
				GlobalScope.glEnable(3553u);
				GlobalScope.glBindTexture(3553u, _modelTextureId);
			}
			else
			{
				GlobalScope.glDisable(3553u);
			}

			GlobalScope.glDrawArrays(4u, 0, _model.Length - (_model.Length % 3), _model);
		}

		/// <summary>Row-ordered elements, matching how glLoadMatrixf maps m[] to M11..M44.</summary>
		private static float[] ToArray(Matrix m)
		{
			return new float[16]
			{
				m.M11, m.M12, m.M13, m.M14,
				m.M21, m.M22, m.M23, m.M24,
				m.M31, m.M32, m.M33, m.M34,
				m.M41, m.M42, m.M43, m.M44
			};
		}
	}
}
