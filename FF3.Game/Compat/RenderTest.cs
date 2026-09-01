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

namespace FF3
{
	internal sealed class RenderTest : DrawableGameComponent
	{
		private readonly string _mode;
		private VertexPositionColorTexture[] _verts;
		private uint _texture;
		private bool _ready;

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
			GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer,
				new Color(20, 20, 60), 1.0f, 0);

			if (_mode == "2d")
			{
				// Known-good path: the game's own 2D projection.
				GlobalScope.glMatrixMode(5889u);
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

			// 1: control - no alpha test, no depth. Known to render.
			GlobalScope.glDisable(3008u);
			GlobalScope.glDisable(2929u);
			GlobalScope.glDrawArrays(4u, 0, 6, _verts);

			// 2: alpha test on, exactly as the game sets it (GL_GREATER, ref 0.01).
			GlobalScope.glEnable(3008u);
			GlobalScope.glAlphaFunc(516u, 0.01f);
			GlobalScope.glDisable(2929u);
			GlobalScope.glDrawArrays(4u, 6, 6, _verts);

			// 3: depth test ENABLED but the comparison always passes. If this one is
			// missing too, the fault is not the comparison - it is having depth on at all.
			GlobalScope.glDisable(3008u);
			GlobalScope.glEnable(2929u);
			GlobalScope.glDepthFunc(515u);   // GL_LEQUAL again, now after an explicit depth clear
			GlobalScope.glDepthMask(1);
			GlobalScope.glDrawArrays(4u, 12, 6, _verts);

			// 4: both, i.e. the game's actual 3D state.
			GlobalScope.glEnable(3008u);
			GlobalScope.glAlphaFunc(516u, 0.01f);
			GlobalScope.glEnable(2929u);
			GlobalScope.glDepthFunc(515u);
			GlobalScope.glDrawArrays(4u, 18, 6, _verts);
		}
	}
}
