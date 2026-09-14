// The game's frame, kept, and drawn again - in between two of them, interpolated.
//
// The game runs thirty steps a second and draws as it goes: a part's logic and its draws
// are one pass, so there is no draw phase to call a second time. But every draw reaches
// the device through two doors - NativeRenderer.Draw and Clear for everything with
// geometry (the 3D world, the sprites, the windows, the cursor), Graphics.DrawString for
// the text - and this stands in both. While a step runs (Recording), a draw is written down
// instead of done: the primitive, its vertices copied into an arena, its matrices, texture
// and state, the viewport of the moment; the text with its position and colour. Replay then
// draws the list. On the frame the step ran, and on every display frame after it until the
// next step, the list is what is drawn.
//
// Smoothing (FramePacer.Blend under 1) draws the list a fraction of the way back toward the
// frame before it: each draw is matched to one of the last frame's - the same primitive,
// count, texture and states, looked for from where the last match left off, so a list that
// gains or loses a draw stays aligned after it - and its matrices are interpolated, and its
// vertices too when the counts agree (a walk cycle moves vertices as well as matrices). A
// draw whose first vertex would cross more than a third of the screen between the two frames
// is a camera cut or a spawn, and is drawn where it is. Text is matched by its words and size
// and slides with them. Nothing here changes what the game computes: the same steps run at
// the same rate; only what stands on the screen between them is new.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenFF.Client
{
	// MonoGame's types by name: the engine's OpenFF.Color and vectors sit a namespace up.
	using Color = Microsoft.Xna.Framework.Color;
	using Vector2 = Microsoft.Xna.Framework.Vector2;
	using Vector3 = Microsoft.Xna.Framework.Vector3;
	using Vector4 = Microsoft.Xna.Framework.Vector4;
	using Matrix = Microsoft.Xna.Framework.Matrix;

	internal static class FrameCapture
	{
		/// <summary>Only the native renderer's draws come through the doors this stands in.</summary>
		public static bool Supported => NativeRenderer.Enabled;

		/// <summary>True while a step of the game runs and its draws are being written down.</summary>
		public static bool Recording { get; private set; }

		/// <summary>Whether a frame has been taken to draw.</summary>
		public static bool HasFrame => _current.Records.Count > 0;

		/// <summary>The last replay's draws that were interpolated, and those drawn as they were (for the overlay).</summary>
		public static int Blended, Snapped;

		private enum Kind : byte { Clear, Draw, TextBegin, Text, TextEnd }

		private struct Record
		{
			public Kind Kind;
			public int Key;
			public Viewport Viewport;
			// Draw
			public uint Mode;
			public int First, Count;
			public Matrix World, Projection;
			public Texture2D Texture;
			public TextureFilter Filter;
			public TextureAddressMode AddressU, AddressV;
			public bool AlphaTest, DepthTest, DepthWrite, Cull;
			public float AlphaReference;
			public CompareFunction AlphaFunction, DepthFunction;
			public CullMode CullMode;
			public Blend DestinationBlend;
			// Clear
			public ClearOptions Options;
			public Color Colour;
			public float Depth;
			public int Stencil;
			// Text
			public string Text;
			public float X, Y, Rotation;
			public int Size;
			public Vector2 Origin, Scale;
			public SpriteEffects Flip;
		}

		private sealed class Frame
		{
			public readonly List<Record> Records = new List<Record>(2048);
			public VertexPositionColorTexture[] Arena = new VertexPositionColorTexture[1 << 16];
			public int Used;

			public void Reset()
			{
				Records.Clear();
				Used = 0;
			}

			public int Take(VertexPositionColorTexture[] source, int first, int count)
			{
				if (Used + count > Arena.Length)
				{
					int size = Arena.Length;
					while (size < Used + count) size *= 2;
					Array.Resize(ref Arena, size);
				}
				Array.Copy(source, first, Arena, Used, count);
				int at = Used;
				Used += count;
				return at;
			}
		}

		private static Frame _current = new Frame();
		private static Frame _previous = new Frame();
		private static VertexPositionColorTexture[] _scratch = new VertexPositionColorTexture[4096];

		/// <summary>A step is about to run: the last frame becomes the one before, and the draws go into a fresh one.</summary>
		public static void Begin()
		{
			Frame t = _previous;
			_previous = _current;
			_current = t;
			_current.Reset();
			Recording = true;
		}

		public static void End()
		{
			Recording = false;
		}

		/// <summary>The scene changed under the frames (a new map, a battle): the one before is no longer worth blending toward.</summary>
		public static void Cut()
		{
			_previous.Reset();
		}

		// ---- the doors: what the game draws while a step runs

		public static void Clear(GraphicsDevice device, ClearOptions options, Color colour, float depth, int stencil)
		{
			_current.Records.Add(new Record { Kind = Kind.Clear, Viewport = device.Viewport, Options = options, Colour = colour, Depth = depth, Stencil = stencil });
		}

		public static void Draw(GraphicsDevice device, uint mode, VertexPositionColorTexture[] vertices, int first, int count,
			Matrix world, Matrix projection, Texture2D texture,
			TextureFilter filter, TextureAddressMode addressU, TextureAddressMode addressV,
			bool alphaTest, float alphaReference, CompareFunction alphaFunction,
			bool depthTest, bool depthWrite, CompareFunction depthFunction,
			bool cull, CullMode cullMode, Blend destinationBlend)
		{
			if (count <= 0 || vertices == null || first < 0 || first + count > vertices.Length) return;
			_current.Records.Add(new Record
			{
				Kind = Kind.Draw,
				Key = HashCode.Combine((int)mode, count, texture == null ? 0 : texture.GetHashCode(), alphaTest, (int)destinationBlend, depthTest),
				Viewport = device.Viewport,
				Mode = mode, First = _current.Take(vertices, first, count), Count = count,
				World = world, Projection = projection, Texture = texture,
				Filter = filter, AddressU = addressU, AddressV = addressV,
				AlphaTest = alphaTest, AlphaReference = alphaReference, AlphaFunction = alphaFunction,
				DepthTest = depthTest, DepthWrite = depthWrite, DepthFunction = depthFunction,
				Cull = cull, CullMode = cullMode, DestinationBlend = destinationBlend
			});
		}

		public static void TextBegin(GraphicsDevice device)
		{
			_current.Records.Add(new Record { Kind = Kind.TextBegin, Viewport = device.Viewport });
		}

		public static void Text(string text, float x, float y, int size, Color colour, float rotation, Vector2 origin, Vector2 scale, SpriteEffects flip)
		{
			if (string.IsNullOrEmpty(text)) return;
			_current.Records.Add(new Record
			{
				Kind = Kind.Text, Key = HashCode.Combine(text, size),
				Text = text, X = x, Y = y, Size = size, Colour = colour, Rotation = rotation, Origin = origin, Scale = scale, Flip = flip
			});
		}

		public static void TextEnd()
		{
			_current.Records.Add(new Record { Kind = Kind.TextEnd });
		}

		// ---- drawing the frame

		/// <summary>Draws the kept frame; with blend under 1, part of the way back toward the frame before it.</summary>
		public static void Replay(GraphicsDevice device, GlobalScope.Graphics graphics, float blend)
		{
			bool inText = false;
			try
			{
				Replay(device, graphics, blend, ref inText);
			}
			catch (Exception ex)
			{
				Log.First(LogChannel.General, "frame-replay", 3, () => "frame: replay failed: " + ex.GetType().Name + ": " + ex.Message);
			}
			finally
			{
				// A throw between the text's Begin and End would leave the batch open for everything drawn after.
				if (inText && graphics != null) { try { graphics.DrawStringEnd(); } catch (Exception) { } }
			}
		}

		private static void Replay(GraphicsDevice device, GlobalScope.Graphics graphics, float blend, ref bool inText)
		{
			List<Record> cur = _current.Records;
			List<Record> prev = _previous.Records;
			bool lerp = blend < 0.999f && prev.Count > 0;
			float t = MathHelper.Clamp(blend, 0f, 1f);
			int j = 0;
			Blended = 0;
			Snapped = 0;
			for (int i = 0; i < cur.Count; i++)
			{
				Record r = cur[i];
				switch (r.Kind)
				{
					case Kind.Clear:
						SetViewport(device, r.Viewport);
						NativeRenderer.Clear(device, r.Options, r.Colour, r.Depth, r.Stencil);
						break;

					case Kind.TextBegin:
						SetViewport(device, r.Viewport);
						if (graphics != null && !inText) { graphics.DrawStringStart(); inText = true; }
						break;

					case Kind.TextEnd:
						if (graphics != null && inText) { graphics.DrawStringEnd(); inText = false; }
						break;

					case Kind.Text:
					{
						if (graphics == null || !inText) break;
						float x = r.X, y = r.Y;
						if (lerp && Match(prev, ref j, r.Key, out int k))
						{
							Record p = prev[k];
							// Text that moved a screen's width has not slid there: a page turned.
							if (Math.Abs(p.X - r.X) < 240f && Math.Abs(p.Y - r.Y) < 160f)
							{
								x = MathHelper.Lerp(p.X, r.X, t);
								y = MathHelper.Lerp(p.Y, r.Y, t);
							}
						}
						graphics.DrawStringAs(r.Text, x, y, r.Size, r.Colour, r.Rotation, r.Origin, r.Scale, r.Flip);
						break;
					}

					case Kind.Draw:
					{
						if (r.Texture != null && r.Texture.IsDisposed) break;
						SetViewport(device, r.Viewport);
						VertexPositionColorTexture[] vertices = _current.Arena;
						int first = r.First;
						Matrix world = r.World, projection = r.Projection;
						if (lerp && Match(prev, ref j, r.Key, out int k))
						{
							Record p = prev[k];
							if (!Jumped(p, _previous.Arena, r, _current.Arena))
							{
								world = Matrix.Lerp(p.World, r.World, t);
								projection = Matrix.Lerp(p.Projection, r.Projection, t);
								if (p.Count == r.Count)
								{
									if (_scratch.Length < r.Count) Array.Resize(ref _scratch, Math.Max(r.Count, _scratch.Length * 2));
									VertexPositionColorTexture[] a = _previous.Arena, b = _current.Arena;
									for (int v = 0; v < r.Count; v++)
									{
										VertexPositionColorTexture vertex = b[r.First + v];
										vertex.Position = Vector3.Lerp(a[p.First + v].Position, vertex.Position, t);
										_scratch[v] = vertex;
									}
									vertices = _scratch;
									first = 0;
								}
								Blended++;
							}
							else Snapped++;
						}
						NativeRenderer.Draw(device, r.Mode, vertices, first, r.Count, world, projection, r.Texture,
							r.Filter, r.AddressU, r.AddressV, r.AlphaTest, r.AlphaReference, r.AlphaFunction,
							r.DepthTest, r.DepthWrite, r.DepthFunction, r.Cull, r.CullMode, r.DestinationBlend);
						break;
					}
				}
			}
			if (inText && graphics != null) { graphics.DrawStringEnd(); inText = false; }
		}

		/// <summary>The last frame's draw that is this one, looked for from where the last match left off - a few draws on at most, so an added or missing draw does not throw the rest off.</summary>
		private static bool Match(List<Record> prev, ref int j, int key, out int at)
		{
			int end = Math.Min(prev.Count, j + 24);
			for (int k = j; k < end; k++)
			{
				if (prev[k].Key == key)
				{
					at = k;
					j = k + 1;
					return true;
				}
			}
			at = -1;
			return false;
		}

		/// <summary>Whether the draw's first vertex moves more than a third of the screen between the two frames: a cut or a spawn, not motion.</summary>
		private static bool Jumped(in Record p, VertexPositionColorTexture[] pa, in Record r, VertexPositionColorTexture[] ra)
		{
			Vector4 a = Vector4.Transform(new Vector4(pa[p.First].Position, 1f), p.World * p.Projection);
			Vector4 b = Vector4.Transform(new Vector4(ra[r.First].Position, 1f), r.World * r.Projection);
			if (a.W <= 1e-5f || b.W <= 1e-5f) return a.W <= 1e-5f != b.W <= 1e-5f;
			float dx = a.X / a.W - b.X / b.W, dy = a.Y / a.W - b.Y / b.W;
			return dx * dx + dy * dy > 0.66f * 0.66f;
		}

		private static void SetViewport(GraphicsDevice device, Viewport v)
		{
			Viewport now = device.Viewport;
			if (now.X != v.X || now.Y != v.Y || now.Width != v.Width || now.Height != v.Height)
			{
				if (v.Width > 0 && v.Height > 0) device.Viewport = v;
			}
		}
	}
}
