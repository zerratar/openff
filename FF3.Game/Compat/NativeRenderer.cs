// Native MonoGame rendering path, replacing the OpenGL ES emulation's submission.
//
// Enable with --renderer=native. The emulation stays in place as the A/B baseline;
// this intercepts only the two calls that actually touch the device - the draw and
// the clear - and does them the way MonoGame expects.
//
// Why this exists: the emulated layer is a GL ES 1.x fixed-function translator that
// was written against XNA on Direct3D 9 and is now running on OpenGL. Three
// conventions deep, and they disagree about clip-space depth. The emulation feeds
// BasicEffect a GL-convention projection (z -> [-1, 1]) where MonoGame's own
// matrices produce the Direct3D convention (z -> [0, 1]), which is why every
// depth-tested draw fails its comparison while everything with depth off is fine.
//
// This path takes the projection the game built and rebases its depth range, owns
// the depth/blend/raster state explicitly, and guarantees the depth buffer is
// actually cleared (a clear is masked out if depth writes happen to be disabled).

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FF3
{
	internal static class NativeRenderer
	{
		/// <summary>
		/// On by default. The GL emulation is kept behind --renderer=emulated purely as
		/// an A/B baseline while the rest of that layer is dismantled; it does not render
		/// 3D correctly.
		/// </summary>
		public static readonly bool Enabled = !IsLegacyRequested();

		private static bool IsLegacyRequested()
		{
			string requested = Options.Get("renderer");
			return string.Equals(requested, "emulated", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(requested, "legacy", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(requested, "gl", StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Rebases clip-space z from OpenGL's [-w, w] to Direct3D's [0, w], which is what
		/// MonoGame's pipeline and its depth range assume. z' = (z + w) / 2.
		/// </summary>
		private static readonly Matrix DepthRangeFix = new Matrix(
			1f, 0f, 0f, 0f,
			0f, 1f, 0f, 0f,
			0f, 0f, 0.5f, 0f,
			0f, 0f, 0.5f, 1f);

		private static readonly System.Collections.Generic.Dictionary<int, DepthStencilState> _depthStates
			= new System.Collections.Generic.Dictionary<int, DepthStencilState>();
		private static readonly System.Collections.Generic.Dictionary<int, BlendState> _blendStates
			= new System.Collections.Generic.Dictionary<int, BlendState>();

		private static BasicEffect _basic;
		private static AlphaTestEffect _alphaTest;

		private static void EnsureEffects(GraphicsDevice device)
		{
			if (_basic == null || _basic.GraphicsDevice != device)
			{
				_basic = new BasicEffect(device)
				{
					VertexColorEnabled = true,
					LightingEnabled = false,
					FogEnabled = false,
					World = Matrix.Identity,
					View = Matrix.Identity
				};
				_alphaTest = new AlphaTestEffect(device)
				{
					VertexColorEnabled = true,
					World = Matrix.Identity,
					View = Matrix.Identity
				};
			}
		}

		/// <summary>Clears, guaranteeing the depth buffer is actually written.</summary>
		public static void Clear(GraphicsDevice device, ClearOptions options, Color colour,
			float depth, int stencil)
		{
			// A depth clear is masked out when depth writes are disabled, and the game's
			// clear depth is float.MaxValue, which is outside the valid [0, 1] range.
			device.DepthStencilState = DepthStencilState.Default;
			float clamped = float.IsNaN(depth) ? 1f : MathHelper.Clamp(depth, 0f, 1f);
			device.Clear(options, colour, clamped, stencil);
		}

		private static short[] _fanIndices = new short[0];

		/// <summary>
		/// Draws one batch, taking the emulation's tracked state as intent.
		///
		/// Takes the raw GL primitive mode rather than a pre-decided PrimitiveType: the
		/// emulation turned strips and fans into an index buffer of only 16 entries,
		/// which silently dropped geometry on anything larger. Strips map straight onto
		/// TriangleStrip; fans are expanded here into an index buffer that grows.
		/// </summary>
		public static void Draw(GraphicsDevice device, uint mode,
			VertexPositionColorTexture[] vertices, int first, int count,
			Matrix world, Matrix projection, Texture2D texture,
			TextureFilter filter, TextureAddressMode addressU, TextureAddressMode addressV,
			bool alphaTest, float alphaReference, CompareFunction alphaFunction,
			bool depthTest, bool depthWrite, CompareFunction depthFunction,
			bool cull, CullMode cullMode, Blend destinationBlend)
		{
			if (count <= 0)
			{
				return;
			}
			EnsureEffects(device);

			Matrix corrected = Matrix.Multiply(projection, DepthRangeFix);

			device.DepthStencilState = RenderOverrides.Depth(
				DepthState(depthTest, depthWrite, depthFunction));
			device.BlendState = BlendStateFor(destinationBlend);
			device.RasterizerState = RenderOverrides.Rasterizer(cull
				? (cullMode == CullMode.CullClockwiseFace
					? RasterizerState.CullClockwise : RasterizerState.CullCounterClockwise)
				: RasterizerState.CullNone);

			// Per-texture filter and wrap mode. Omitting this leaves whatever the last
			// SpriteBatch set, which samples the wrong texels at atlas boundaries: dark
			// fringes around alpha-tested sprites, and decals such as the face overlay
			// picking up neighbouring cells entirely.
			if (texture != null && !texture.IsDisposed)
			{
				device.SamplerStates[0] = SamplerFor(filter, addressU, addressV);
			}

			Effect effect;
			if (texture != null && !texture.IsDisposed && RenderOverrides.AlphaTest(alphaTest))
			{
				_alphaTest.World = world;
				_alphaTest.Projection = corrected;
				_alphaTest.Texture = texture;
				_alphaTest.AlphaFunction = alphaFunction;
				_alphaTest.ReferenceAlpha = (int)(alphaReference * 255f);
				effect = _alphaTest;
			}
			else
			{
				_basic.World = world;
				_basic.Projection = corrected;
				_basic.TextureEnabled = texture != null && !texture.IsDisposed;
				_basic.Texture = _basic.TextureEnabled ? texture : null;
				effect = _basic;
			}

			// Applied unconditionally: the emulation's "is the effect still current"
			// tracking is exactly what kept going wrong.
			foreach (EffectPass pass in effect.CurrentTechnique.Passes)
			{
				pass.Apply();
				switch (mode)
				{
					case 4u:   // GL_TRIANGLES
						device.DrawUserPrimitives(PrimitiveType.TriangleList,
							vertices, first, count / 3);
						break;

					case 5u:   // GL_TRIANGLE_STRIP
						device.DrawUserPrimitives(PrimitiveType.TriangleStrip,
							vertices, first, count - 2);
						break;

					case 6u:   // GL_TRIANGLE_FAN - no direct equivalent, so expand it
						device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
							vertices, first, count, FanIndices(count), 0, count - 2);
						break;

					case 2u:   // GL_LINE_STRIP
						device.DrawUserPrimitives(PrimitiveType.LineStrip,
							vertices, first, count - 1);
						break;

					default:
						Log.First(LogChannel.Gl, "native-unknown-mode", 8,
							() => "unhandled GL primitive mode " + mode);
						break;
				}
			}
		}

		private static readonly System.Collections.Generic.Dictionary<int, SamplerState> _samplers
			= new System.Collections.Generic.Dictionary<int, SamplerState>();

		private static SamplerState SamplerFor(TextureFilter filter,
			TextureAddressMode addressU, TextureAddressMode addressV)
		{
			int key = ((int)filter << 8) | ((int)addressU << 4) | (int)addressV;
			lock (_samplers)
			{
				if (!_samplers.TryGetValue(key, out SamplerState state))
				{
					state = new SamplerState
					{
						Filter = filter,
						AddressU = addressU,
						AddressV = addressV
					};
					_samplers[key] = state;
				}
				return state;
			}
		}

		/// <summary>Triangle-fan indices (0, i-1, i), grown on demand.</summary>
		private static short[] FanIndices(int count)
		{
			int needed = (count - 2) * 3;
			if (_fanIndices.Length < needed)
			{
				_fanIndices = new short[needed];
				for (int i = 2, at = 0; i < count; i++)
				{
					_fanIndices[at++] = 0;
					_fanIndices[at++] = (short)(i - 1);
					_fanIndices[at++] = (short)i;
				}
			}
			return _fanIndices;
		}

		private static DepthStencilState DepthState(bool test, bool write, CompareFunction func)
		{
			if (!test)
			{
				return DepthStencilState.None;
			}
			int key = ((int)func << 1) | (write ? 1 : 0);
			lock (_depthStates)
			{
				if (!_depthStates.TryGetValue(key, out DepthStencilState state))
				{
					state = new DepthStencilState
					{
						DepthBufferEnable = true,
						DepthBufferWriteEnable = write,
						DepthBufferFunction = func
					};
					_depthStates[key] = state;
				}
				return state;
			}
		}

		private static BlendState BlendStateFor(Blend destination)
		{
			int key = (int)destination;
			lock (_blendStates)
			{
				if (!_blendStates.TryGetValue(key, out BlendState state))
				{
					state = new BlendState
					{
						ColorSourceBlend = Blend.SourceAlpha,
						AlphaSourceBlend = Blend.SourceAlpha,
						ColorDestinationBlend = destination,
						AlphaDestinationBlend = destination,
						ColorWriteChannels = ColorWriteChannels.All
					};
					_blendStates[key] = state;
				}
				return state;
			}
		}
	}
}
