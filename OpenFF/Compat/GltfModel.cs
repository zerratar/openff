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
		public List<GltfPrimitive> Primitives = new List<GltfPrimitive>();
		public Vector3 Min, Max;
		public int Triangles;
		public string Problem;

		/// <summary>Reads a .glb or .gltf; textures become Texture2Ds on the device given. Throws with the reason on a broken file.</summary>
		public static GltfModel Load(string path, GraphicsDevice device)
		{
			GltfFile file = GltfFile.Load(path);
			GltfModel model = new GltfModel { Path = path, Triangles = file.Triangles };
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
						using (MemoryStream stream = new MemoryStream(file.Images[image].Bytes)) texture = Texture2D.FromStream(device, stream);
				}
				catch (Exception ex) { model.Problem = "image " + image + ": " + ex.Message; }
				textures[image] = texture;
				return texture;
			}

			Vector3 light = Vector3.Normalize(new Vector3(0.4f, 1f, 0.6f));
			foreach (GltfMesh mesh in file.Meshes)
			{
				GltfMaterial material = mesh.Material >= 0 && mesh.Material < file.Materials.Count ? file.Materials[mesh.Material] : new GltfMaterial();
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
				model.Primitives.Add(new GltfPrimitive { Vertices = vertices, Texture = TextureOf(material.Image), DoubleSided = material.DoubleSided, Translucent = material.Blend });
			}
			if (file.Notes.Count > 0 && model.Problem == null) Log.Write(LogChannel.General, "meshes: " + System.IO.Path.GetFileName(path) + ": " + string.Join("; ", file.Notes));
			return model;
		}

		private static float Clamp(float v) => Math.Max(0f, Math.Min(1f, v));
	}
}
