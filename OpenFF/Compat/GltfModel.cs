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

			// One Texture2D per image, made when a material asks for it.
			Dictionary<int, Texture2D> textures = new Dictionary<int, Texture2D>();
			Texture2D TextureOf(int image)
			{
				if (image < 0 || image >= file.Images.Count) return null;
				if (textures.TryGetValue(image, out Texture2D have)) return have;
				Texture2D texture = null;
				try
				{
					if (file.Images[image].Bytes != null)
						using (MemoryStream stream = new MemoryStream(file.Images[image].Bytes)) texture = Mipmapped(device, Texture2D.FromStream(device, stream));
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
		/// The picture with a mipmap chain: Texture2D.FromStream makes none, and a 2048-square
		/// texture on a character a hundred pixels tall then shimmers with every frame's pick of
		/// raw texels. Each level is the one above box-filtered; the sampler's linear filter
		/// (NativeRenderer) is trilinear once the levels exist.
		/// </summary>
		private static Texture2D Mipmapped(GraphicsDevice device, Texture2D flat)
		{
			if (flat == null || flat.LevelCount > 1 || (flat.Width <= 1 && flat.Height <= 1)) return flat;
			try
			{
				int w = flat.Width, h = flat.Height;
				Color[] level = new Color[w * h];
				flat.GetData(level);
				// Texturing tools write 8192-square pictures; on a character a few hundred pixels tall
				// nothing above 2048 can show, and the memory would be a quarter of a gigabyte each.
				while (w > 2048 || h > 2048)
				{
					int nw = Math.Max(1, w / 2), nh = Math.Max(1, h / 2);
					Color[] next = new Color[nw * nh];
					for (int y = 0; y < nh; y++)
						for (int x = 0; x < nw; x++)
						{
							Color a = level[Math.Min(h - 1, y * 2) * w + Math.Min(w - 1, x * 2)], b = level[Math.Min(h - 1, y * 2) * w + Math.Min(w - 1, x * 2 + 1)];
							Color c = level[Math.Min(h - 1, y * 2 + 1) * w + Math.Min(w - 1, x * 2)], d = level[Math.Min(h - 1, y * 2 + 1) * w + Math.Min(w - 1, x * 2 + 1)];
							next[y * nw + x] = new Color((a.R + b.R + c.R + d.R) / 4, (a.G + b.G + c.G + d.G) / 4, (a.B + b.B + c.B + d.B) / 4, (a.A + b.A + c.A + d.A) / 4);
						}
					level = next; w = nw; h = nh;
				}
				Texture2D mipped = new Texture2D(device, w, h, true, SurfaceFormat.Color);
				mipped.SetData(0, null, level, 0, level.Length);
				for (int i = 1; i < mipped.LevelCount; i++)
				{
					int nw = Math.Max(1, w / 2), nh = Math.Max(1, h / 2);
					Color[] next = new Color[nw * nh];
					for (int y = 0; y < nh; y++)
						for (int x = 0; x < nw; x++)
						{
							int r = 0, g = 0, b = 0, a = 0, n = 0;
							for (int dy = 0; dy < 2; dy++)
								for (int dx = 0; dx < 2; dx++)
								{
									int sx = Math.Min(w - 1, x * 2 + dx), sy = Math.Min(h - 1, y * 2 + dy);
									Color c = level[sy * w + sx];
									r += c.R; g += c.G; b += c.B; a += c.A; n++;
								}
							next[y * nw + x] = new Color(r / n, g / n, b / n, a / n);
						}
					mipped.SetData(i, null, next, 0, next.Length);
					level = next; w = nw; h = nh;
				}
				flat.Dispose();
				return mipped;
			}
			catch (Exception)
			{
				return flat;
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
