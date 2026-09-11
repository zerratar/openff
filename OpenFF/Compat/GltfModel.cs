// A mod's own model for the client: the shared glTF reader's arrays (Shared/Graphics/
// GltfFile.cs) made into MonoGame triangle lists and textures, drawn by ModMeshes with the
// field's camera. This is the OpenFF target's freedom: a Steam target must have a model in
// the game's own format, an OpenFF mod puts a Blender export in its assets folder and names
// it. Normals become a shade in the vertex colour, as the DS models carry theirs; skins and
// animations are not read yet - a model comes in at its bind pose.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenFF.Graphics;

namespace OpenFF.Client
{
	// MonoGame's types by name: the engine's OpenFF.Vector3 and friends sit a namespace up.
	using Vector2 = Microsoft.Xna.Framework.Vector2;
	using Vector3 = Microsoft.Xna.Framework.Vector3;
	using Color = Microsoft.Xna.Framework.Color;

	internal sealed class GltfPrimitive
	{
		public VertexPositionColorTexture[] Vertices;   // a triangle list
		public Texture2D Texture;                       // null for a plain colour
		public bool DoubleSided;
		public bool Translucent;
	}

	internal sealed class GltfModel
	{
		public string Path;
		/// <summary>The file, kept when it has animations, for posing; null for a static model.</summary>
		public GltfFile File;
		/// <summary>Primitive i is File.Meshes[i].</summary>
		public List<GltfPrimitive> Primitives = new List<GltfPrimitive>();
		public Vector3 Min, Max;
		public int Triangles;
		public string Problem;

		/// <summary>Reads a .glb or .gltf; textures become Texture2Ds on the device given. Throws with the reason on a broken file.</summary>
		public static GltfModel Load(string path, GraphicsDevice device)
		{
			GltfFile file = GltfFile.Load(path);
			GltfModel model = new GltfModel { Path = path, Triangles = file.Triangles, File = file.Animations.Count > 0 ? file : null };
			model.Min = new Vector3(file.Min[0], file.Min[1], file.Min[2]);
			model.Max = new Vector3(file.Max[0], file.Max[1], file.Max[2]);
			if (file.Meshes.Count == 0) { model.Problem = string.Join("; ", file.Notes); return model; }

			// One Texture2D per image, made when a material asks for it. The mip chain is built
			// knowing which texels the meshes' UVs cover (Coverage), so the padding between UV
			// islands never averages into them - the seams that show at a distance and not up close.
			Dictionary<int, Texture2D> textures = new Dictionary<int, Texture2D>();
			Texture2D TextureOf(int image)
			{
				if (image < 0 || image >= file.Images.Count) return null;
				if (textures.TryGetValue(image, out Texture2D have)) return have;
				Texture2D texture = null;
				try
				{
					if (file.Images[image].Bytes != null)
					{
						Texture2D flat;
						using (MemoryStream stream = new MemoryStream(file.Images[image].Bytes)) flat = Texture2D.FromStream(device, stream);
						texture = Mipmapped(device, flat, (w, h) => Coverage(file, image, w, h));
					}
				}
				catch (Exception ex) { model.Problem = "image " + image + ": " + ex.Message; }
				textures[image] = texture;
				return texture;
			}

			foreach (GltfMesh mesh in file.Meshes)
			{
				GltfMaterial material = mesh.Material >= 0 && mesh.Material < file.Materials.Count ? file.Materials[mesh.Material] : new GltfMaterial();
				model.Primitives.Add(new GltfPrimitive { Vertices = Vertices(mesh, material), Texture = TextureOf(material.Image), DoubleSided = material.DoubleSided, Translucent = material.Blend });
			}
			if (file.Notes.Count > 0 && model.Problem == null) Log.Write(LogChannel.General, "meshes: " + System.IO.Path.GetFileName(path) + ": " + string.Join("; ", file.Notes));
			return model;
		}

		/// <summary>
		/// Which texels of an image the file's meshes actually use: every UV triangle of every
		/// primitive whose material has this image, rasterised into a w x h mask (UVs wrapped into
		/// 0..1, as the sampler wraps them). Null when nothing maps it, in which case every texel counts.
		/// </summary>
		private static bool[] Coverage(GltfFile file, int image, int w, int h)
		{
			bool[] covered = null;
			foreach (GltfMesh mesh in file.Meshes)
			{
				GltfMaterial material = mesh.Material >= 0 && mesh.Material < file.Materials.Count ? file.Materials[mesh.Material] : null;
				if (material == null || material.Image != image || mesh.Uvs == null || mesh.Indices == null) continue;
				covered ??= new bool[w * h];
				float[] uv = mesh.Uvs;
				for (int t = 0; t + 2 < mesh.Indices.Length; t += 3)
				{
					int a = mesh.Indices[t], b = mesh.Indices[t + 1], c = mesh.Indices[t + 2];
					// Texel space; a triangle across the wrap is drawn where it lies, wrapped texel by texel.
					float ax = uv[a * 2] * w, ay = uv[a * 2 + 1] * h, bx = uv[b * 2] * w, by = uv[b * 2 + 1] * h, cx = uv[c * 2] * w, cy = uv[c * 2 + 1] * h;
					int x0 = (int)Math.Floor(Math.Min(ax, Math.Min(bx, cx))) - 1, x1 = (int)Math.Ceiling(Math.Max(ax, Math.Max(bx, cx))) + 1;
					int y0 = (int)Math.Floor(Math.Min(ay, Math.Min(by, cy))) - 1, y1 = (int)Math.Ceiling(Math.Max(ay, Math.Max(by, cy))) + 1;
					if (x1 - x0 > w * 4 || y1 - y0 > h * 4) continue;   // a triangle across dozens of wraps: not worth rasterising
					float area = (bx - ax) * (cy - ay) - (cx - ax) * (by - ay);
					if (Math.Abs(area) < 1e-12f) continue;
					// The texel counts when its centre is inside, or within half a texel of an edge (a
					// sliver of an island still owns its texel column; the rim beyond is the neighbour's).
					float tolerance = 0.5f * Math.Max(Math.Abs(bx - ax) + Math.Abs(by - ay), Math.Max(Math.Abs(cx - bx) + Math.Abs(cy - by), Math.Abs(ax - cx) + Math.Abs(ay - cy)));
					for (int y = y0; y <= y1; y++)
						for (int x = x0; x <= x1; x++)
						{
							float px = x + 0.5f, py = y + 0.5f;
							float w0 = ((bx - px) * (cy - py) - (cx - px) * (by - py)) / area;
							float w1 = ((cx - px) * (ay - py) - (ax - px) * (cy - py)) / area;
							float w2 = 1f - w0 - w1;
							float slack = tolerance / Math.Abs(area);
							if (w0 < -slack || w1 < -slack || w2 < -slack) continue;
							int tx = ((x % w) + w) % w, ty = ((y % h) + h) % h;
							covered[ty * w + tx] = true;
						}
				}
			}
			return covered;
		}

		/// <summary>
		/// The picture with a mipmap chain: Texture2D.FromStream makes none, and a 2048-square
		/// texture on a character a hundred pixels tall then shimmers with every frame's pick of
		/// raw texels. Each level is the one above box-filtered; the sampler's linear filter
		/// (NativeRenderer) is trilinear once the levels exist.
		///
		/// Built knowing which texels the meshes use (coverage): the space between UV islands is
		/// filled with the nearest island's colour a couple of texels out at every level, and a
		/// level's texel averages only the covered texels beneath it - so a small level never
		/// carries the background's colour into an island's edge. Without that, a texel at the
		/// edge of the face or the hair reads half background from a few metres away and shows as
		/// a crack along every seam, gone again up close where level 0 is sampled.
		/// </summary>
		private static Texture2D Mipmapped(GraphicsDevice device, Texture2D flat, Func<int, int, bool[]> coverage)
		{
			if (flat == null || flat.LevelCount > 1 || (flat.Width <= 1 && flat.Height <= 1)) return flat;
			// --gltf-mips: off (the default) is level 0 alone, capped at 2048 with the islands' rims cleaned;
			// auto a chain when the UVs leave room for one; full a chain regardless, from the coverage;
			// plain the box chain with no regard for the UVs; raw the picture exactly as it is, untouched.
			if (Options.Get("gltf-mips") == "raw") return flat;
			try
			{
				int w = flat.Width, h = flat.Height;
				Color[] level = new Color[w * h];
				flat.GetData(level);
				System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
				bool[] used = Options.Get("gltf-mips") == "plain" ? null : coverage?.Invoke(w, h);
				int covered = 0;
				if (used != null) foreach (bool u in used) if (u) covered++;
				Log.Write(LogChannel.File, "meshes: mips for a " + w + "x" + h + " picture" + (used != null ? ", " + (100.0 * covered / used.Length).ToString("0.0") + "% of it under the UVs" : ", no UV coverage") + " (" + clock.ElapsedMilliseconds + " ms to map)");
				// Texturing tools write 8192-square pictures; on a character a few hundred pixels tall
				// nothing above 2048 can show, and the memory would be a quarter of a gigabyte each.
				while (w > 2048 || h > 2048)
				{
					Downsample(level, used, w, h, out Color[] next, out bool[] nextUsed, out int nw, out int nh);
					level = next; used = nextUsed; w = nw; h = nh;
				}
				int islands = 0;
				if (used != null)
				{
					islands = Islands(used, w, h);
					// The islands' rims repainted from their insides: a generated atlas (hundreds of small
					// islands packed a few texels apart, the picture soft) has each island's outermost
					// texels already blended with its neighbours', and from a few metres off those rims are
					// what a texel covers - a grey line along every seam. Three texels in are shed and
					// grown back from the colour within, then two more out into the gaps for the mips.
					Erode(used, w, h, 3);
					Pad(level, used, w, h, 5);
				}
				// No chain unless asked (--gltf-mips=full, or auto for one when the UVs have room): an
				// atlas of many small islands packed tight cannot be mipmapped cleanly - at the levels a
				// character a few hundred pixels tall is drawn from, one texel spans two islands whatever
				// is done, and the seam between them shows as a line, gone up close where level 0 is
				// sampled. Level 0 alone (at most 2048 a side, the rims cleaned) is what Crystal's viewer
				// draws too, and shows no seam; what it costs is a little shimmer on a big texture far off.
				string mips = Options.Get("gltf-mips") ?? "off";
				bool chain = mips == "full" || mips == "plain" || (mips == "auto" && used != null && islands <= 48);
				Log.Write(LogChannel.File, "meshes: " + (chain ? "a mip chain" : "level 0 alone, no mip chain") + " for the " + w + "x" + h + " picture" + (used != null ? " of " + islands + " UV island(s)" : "") + " (--gltf-mips=" + mips + ")");
				Texture2D mipped = new Texture2D(device, w, h, chain, SurfaceFormat.Color);
				mipped.SetData(0, null, level, 0, level.Length);
				for (int i = 1; i < mipped.LevelCount; i++)
				{
					Downsample(level, used, w, h, out Color[] next, out bool[] nextUsed, out int nw, out int nh);
					level = next; used = nextUsed; w = nw; h = nh;
					if (used != null) Pad(level, used, w, h, 2);
					mipped.SetData(i, null, level, 0, level.Length);
				}
				flat.Dispose();
				return mipped;
			}
			catch (Exception)
			{
				return flat;
			}
		}

		/// <summary>Half the size: each texel the average of the covered texels beneath it (all four when none is covered), the coverage carried down as "any".</summary>
		private static void Downsample(Color[] level, bool[] used, int w, int h, out Color[] next, out bool[] nextUsed, out int nw, out int nh)
		{
			nw = Math.Max(1, w / 2); nh = Math.Max(1, h / 2);
			next = new Color[nw * nh];
			nextUsed = used == null ? null : new bool[nw * nh];
			for (int y = 0; y < nh; y++)
				for (int x = 0; x < nw; x++)
				{
					int r = 0, g = 0, b = 0, a = 0, n = 0, ru = 0, gu = 0, bu = 0, au = 0, nu = 0;
					for (int dy = 0; dy < 2; dy++)
						for (int dx = 0; dx < 2; dx++)
						{
							int sx = Math.Min(w - 1, x * 2 + dx), sy = Math.Min(h - 1, y * 2 + dy);
							Color c = level[sy * w + sx];
							r += c.R; g += c.G; b += c.B; a += c.A; n++;
							if (used != null && used[sy * w + sx]) { ru += c.R; gu += c.G; bu += c.B; au += c.A; nu++; }
						}
					next[y * nw + x] = nu > 0 ? new Color(ru / nu, gu / nu, bu / nu, au / nu) : new Color(r / n, g / n, b / n, a / n);
					if (nextUsed != null) nextUsed[y * nw + x] = nu > 0;
				}
		}

		/// <summary>How many separate patches the covered texels form (4-connected): the UV islands of the atlas, near enough.</summary>
		private static int Islands(bool[] used, int w, int h)
		{
			bool[] seen = new bool[used.Length];
			Stack<int> stack = new Stack<int>();
			int count = 0;
			for (int start = 0; start < used.Length; start++)
			{
				if (!used[start] || seen[start]) continue;
				count++;
				seen[start] = true;
				stack.Push(start);
				while (stack.Count > 0)
				{
					int at = stack.Pop();
					int x = at % w, y = at / w;
					for (int side = 0; side < 4; side++)
					{
						int nx = x + (side == 0 ? 1 : side == 1 ? -1 : 0), ny = y + (side == 2 ? 1 : side == 3 ? -1 : 0);
						if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
						int n = ny * w + nx;
						if (!used[n] || seen[n]) continue;
						seen[n] = true;
						stack.Push(n);
					}
				}
			}
			return count;
		}

		/// <summary>The covered texels within <paramref name="passes"/> of an uncovered one stop counting as covered - an island's rim shed, for Pad to grow back from the inside. An island thinner than twice the passes keeps its middle line.</summary>
		private static void Erode(bool[] used, int w, int h, int passes)
		{
			for (int pass = 0; pass < passes; pass++)
			{
				bool[] shrunk = (bool[])used.Clone();
				for (int y = 0; y < h; y++)
					for (int x = 0; x < w; x++)
					{
						if (!used[y * w + x]) continue;
						bool edge = false;
						for (int dy = -1; dy <= 1 && !edge; dy++)
							for (int dx = -1; dx <= 1 && !edge; dx++)
							{
								if (dx == 0 && dy == 0) continue;
								int sx = ((x + dx) % w + w) % w, sy = ((y + dy) % h + h) % h;
								if (!used[sy * w + sx]) edge = true;
							}
						if (edge) shrunk[y * w + x] = false;
					}
				// A sliver that would vanish keeps what it had: nothing to grow it back from otherwise.
				bool[] keep = (bool[])used.Clone();
				Array.Copy(shrunk, used, used.Length);
				if (pass == passes - 1) RestoreVanished(used, keep, w, h, passes);
			}
		}

		/// <summary>Islands eroded away entirely come back as they were (their colour is all there is for them).</summary>
		private static void RestoreVanished(bool[] used, bool[] before, int w, int h, int reach)
		{
			// A texel covered before with no covered texel within reach now: its island is gone; put it back.
			for (int y = 0; y < h; y++)
				for (int x = 0; x < w; x++)
				{
					if (!before[y * w + x] || used[y * w + x]) continue;
					bool near = false;
					for (int dy = -reach; dy <= reach && !near; dy++)
						for (int dx = -reach; dx <= reach && !near; dx++)
						{
							int sx = ((x + dx) % w + w) % w, sy = ((y + dy) % h + h) % h;
							if (used[sy * w + sx]) near = true;
						}
					if (!near) used[y * w + x] = true;
				}
		}

		/// <summary>The uncovered texels within <paramref name="passes"/> of a covered one take the nearest covered colour, and count as covered from then on.</summary>
		private static void Pad(Color[] level, bool[] used, int w, int h, int passes)
		{
			for (int pass = 0; pass < passes; pass++)
			{
				bool[] grown = (bool[])used.Clone();
				bool any = false;
				for (int y = 0; y < h; y++)
					for (int x = 0; x < w; x++)
					{
						if (used[y * w + x]) continue;
						int r = 0, g = 0, b = 0, a = 0, n = 0;
						for (int dy = -1; dy <= 1; dy++)
							for (int dx = -1; dx <= 1; dx++)
							{
								if (dx == 0 && dy == 0) continue;
								int sx = ((x + dx) % w + w) % w, sy = ((y + dy) % h + h) % h;
								if (!used[sy * w + sx]) continue;
								Color c = level[sy * w + sx];
								r += c.R; g += c.G; b += c.B; a += c.A; n++;
							}
						if (n == 0) continue;
						level[y * w + x] = new Color(r / n, g / n, b / n, a / n);
						grown[y * w + x] = true;
						any = true;
					}
				Array.Copy(grown, used, used.Length);
				if (!any) break;
			}
		}

		private static readonly Vector3 Light = Vector3.Normalize(new Vector3(0.4f, 1f, 0.6f));

		/// <summary>A mesh's triangle list from its Positions and Normals as they stand (the bind pose, or a pose an animation set).</summary>
		public static VertexPositionColorTexture[] Vertices(GltfMesh mesh, GltfMaterial material)
		{
			Vector3 light = Light;
			{
				VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[mesh.Indices.Length];
				for (int i = 0; i < mesh.Indices.Length; i++)
				{
					int v = mesh.Indices[i];
					Vector3 p = new Vector3(mesh.Positions[v * 3], mesh.Positions[v * 3 + 1], mesh.Positions[v * 3 + 2]);
					// The shade the DS models carry in their vertex colours, from the normal: lit from
					// above and a little to the front, never darker than a lamp-lit room.
					float shade = 1f;
					if (mesh.Normals != null)
					{
						Vector3 n = new Vector3(mesh.Normals[v * 3], mesh.Normals[v * 3 + 1], mesh.Normals[v * 3 + 2]);
						if (n.LengthSquared() > 0) shade = 0.55f + 0.45f * Math.Max(0f, Vector3.Dot(n, light));
					}
					float r = material.BaseColour[0], g = material.BaseColour[1], b = material.BaseColour[2], a = material.BaseColour[3];
					if (mesh.Colours != null) { r *= mesh.Colours[v * 4]; g *= mesh.Colours[v * 4 + 1]; b *= mesh.Colours[v * 4 + 2]; a *= mesh.Colours[v * 4 + 3]; }
					Color colour = new Color(Clamp(r * shade), Clamp(g * shade), Clamp(b * shade), Clamp(a));
					Vector2 uv = mesh.Uvs != null ? new Vector2(mesh.Uvs[v * 2], mesh.Uvs[v * 2 + 1]) : Vector2.Zero;
					vertices[i] = new VertexPositionColorTexture(p, colour, uv);
				}
				return vertices;
			}
		}

		private static float Clamp(float v) => Math.Max(0f, Math.Min(1f, v));
	}
}
