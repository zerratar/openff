// glTF 2.0 read into plain arrays - the format an OpenFF mod's own models come in (a .glb, or
// a .gltf with its .bin and pictures beside it), as Blender exports them. Shared: the client
// draws it (Compat/GltfModel.cs makes MonoGame vertices of it), Crystal previews it in the
// map editor (Editor/GltfBundle.cs makes the browser's buffers of it). Read here: the scene's
// node tree with every node's matrix or translation/rotation/scale composed into a world
// matrix, meshes and their primitives (triangles; strips and fans made into triangles),
// positions, normals, texture coordinates, vertex colours, indices, materials (base colour
// factor and texture, double sided, alpha blend), images (bytes and mime type). Not read:
// skins, animations, cameras, lights.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace OpenFF.Graphics
{
	public sealed class GltfImage
	{
		public byte[] Bytes;
		public string MimeType;
	}

	public sealed class GltfMaterial
	{
		public string Name;
		public float[] BaseColour = { 1f, 1f, 1f, 1f };
		/// <summary>Index into Images, or -1.</summary>
		public int Image = -1;
		public bool DoubleSided;
		public bool Blend;
	}

	/// <summary>One primitive with its node's world matrix already applied to positions and normals: a triangle list.</summary>
	public sealed class GltfMesh
	{
		public string Node;
		public int Material = -1;
		public float[] Positions;    // 3 per vertex, world space
		public float[] Normals;      // 3 per vertex, or null
		public float[] Uvs;          // 2 per vertex, or null
		public float[] Colours;      // 4 per vertex, or null
		public int[] Indices;        // a triangle list into the vertices
		public int VertexCount => Positions.Length / 3;
	}

	public sealed class GltfFile
	{
		public List<GltfMesh> Meshes = new List<GltfMesh>();
		public List<GltfMaterial> Materials = new List<GltfMaterial>();
		public List<GltfImage> Images = new List<GltfImage>();
		public float[] Min = { float.MaxValue, float.MaxValue, float.MaxValue }, Max = { float.MinValue, float.MinValue, float.MinValue };
		public int Triangles;
		public List<string> Notes = new List<string>();

		public static GltfFile Load(string path) => Load(File.ReadAllBytes(path), Path.GetDirectoryName(path) ?? "");

		/// <summary>Reads a .glb or a .gltf's bytes; `folder` is where its .bin and pictures are looked for.</summary>
		public static GltfFile Load(byte[] file, string folder)
		{
			byte[] bin = null;
			string json;
			if (file.Length >= 12 && file[0] == 'g' && file[1] == 'l' && file[2] == 'T' && file[3] == 'F')
			{
				int at = 12;
				json = null;
				while (at + 8 <= file.Length)
				{
					int length = BitConverter.ToInt32(file, at);
					uint type = BitConverter.ToUInt32(file, at + 4);
					at += 8;
					if (at + length > file.Length) break;
					if (type == 0x4E4F534A) json = Encoding.UTF8.GetString(file, at, length);
					else if (type == 0x004E4942) { bin = new byte[length]; Array.Copy(file, at, bin, 0, length); }
					at += length;
				}
				if (json == null) throw new InvalidDataException("a .glb with no JSON chunk");
			}
			else json = Encoding.UTF8.GetString(file);

			GltfFile result = new GltfFile();
			using (JsonDocument doc = JsonDocument.Parse(json))
			{
				JsonElement root = doc.RootElement;
				List<byte[]> buffers = new List<byte[]>();
				if (root.TryGetProperty("buffers", out JsonElement bufs))
					foreach (JsonElement b in bufs.EnumerateArray())
						buffers.Add(b.TryGetProperty("uri", out JsonElement uri) ? ReadUri(uri.GetString(), folder) : (bin ?? Array.Empty<byte>()));
				JsonElement views = root.TryGetProperty("bufferViews", out JsonElement bv) ? bv : default;
				JsonElement accessors = root.TryGetProperty("accessors", out JsonElement ac) ? ac : default;

				if (root.TryGetProperty("images", out JsonElement images))
				{
					foreach (JsonElement image in images.EnumerateArray())
					{
						GltfImage img = new GltfImage { MimeType = image.TryGetProperty("mimeType", out JsonElement mt) ? mt.GetString() : null };
						try
						{
							if (image.TryGetProperty("uri", out JsonElement uri))
							{
								img.Bytes = ReadUri(uri.GetString(), folder);
								if (img.MimeType == null) img.MimeType = uri.GetString().EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || uri.GetString().EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ? "image/jpeg" : "image/png";
							}
							else
							{
								JsonElement view = views[image.GetProperty("bufferView").GetInt32()];
								byte[] buffer = buffers[view.GetProperty("buffer").GetInt32()];
								int offset = view.TryGetProperty("byteOffset", out JsonElement bo) ? bo.GetInt32() : 0;
								int length = view.GetProperty("byteLength").GetInt32();
								img.Bytes = new byte[length];
								Array.Copy(buffer, offset, img.Bytes, 0, length);
							}
						}
						catch (Exception ex) { result.Notes.Add("image: " + ex.Message); }
						if (img.MimeType == null && img.Bytes != null && img.Bytes.Length > 3) img.MimeType = img.Bytes[0] == 0xFF && img.Bytes[1] == 0xD8 ? "image/jpeg" : "image/png";
						result.Images.Add(img);
					}
				}

				if (root.TryGetProperty("materials", out JsonElement mats))
				{
					foreach (JsonElement m in mats.EnumerateArray())
					{
						GltfMaterial material = new GltfMaterial { Name = m.TryGetProperty("name", out JsonElement mn) ? mn.GetString() : null };
						if (m.TryGetProperty("pbrMetallicRoughness", out JsonElement pbr))
						{
							if (pbr.TryGetProperty("baseColorFactor", out JsonElement f)) material.BaseColour = new[] { f[0].GetSingle(), f[1].GetSingle(), f[2].GetSingle(), f[3].GetSingle() };
							if (pbr.TryGetProperty("baseColorTexture", out JsonElement bt) && root.TryGetProperty("textures", out JsonElement texes))
							{
								JsonElement tex = texes[bt.GetProperty("index").GetInt32()];
								if (tex.TryGetProperty("source", out JsonElement source)) material.Image = source.GetInt32();
							}
						}
						material.DoubleSided = m.TryGetProperty("doubleSided", out JsonElement ds) && ds.GetBoolean();
						material.Blend = m.TryGetProperty("alphaMode", out JsonElement am) && am.GetString() == "BLEND";
						result.Materials.Add(material);
					}
				}

				JsonElement nodes = root.TryGetProperty("nodes", out JsonElement nd) ? nd : default;
				JsonElement meshes = root.TryGetProperty("meshes", out JsonElement ms) ? ms : default;
				List<int> roots = new List<int>();
				if (root.TryGetProperty("scenes", out JsonElement scenes) && scenes.GetArrayLength() > 0)
				{
					int sceneIndex = root.TryGetProperty("scene", out JsonElement s) ? s.GetInt32() : 0;
					if (scenes[sceneIndex].TryGetProperty("nodes", out JsonElement rn)) foreach (JsonElement n in rn.EnumerateArray()) roots.Add(n.GetInt32());
				}
				else if (nodes.ValueKind == JsonValueKind.Array) for (int i = 0; i < nodes.GetArrayLength(); i++) roots.Add(i);

				void Visit(int nodeIndex, float[] parent, int depth)
				{
					if (depth > 64 || nodes.ValueKind != JsonValueKind.Array) return;
					JsonElement node = nodes[nodeIndex];
					float[] local = Mat.Identity();
					if (node.TryGetProperty("matrix", out JsonElement mx)) { for (int i = 0; i < 16; i++) local[i] = mx[i].GetSingle(); }
					else
					{
						float[] t = { 0, 0, 0 }, sc = { 1, 1, 1 }, q = { 0, 0, 0, 1 };
						if (node.TryGetProperty("translation", out JsonElement tr)) t = new[] { tr[0].GetSingle(), tr[1].GetSingle(), tr[2].GetSingle() };
						if (node.TryGetProperty("rotation", out JsonElement ro)) q = new[] { ro[0].GetSingle(), ro[1].GetSingle(), ro[2].GetSingle(), ro[3].GetSingle() };
						if (node.TryGetProperty("scale", out JsonElement sl)) sc = new[] { sl[0].GetSingle(), sl[1].GetSingle(), sl[2].GetSingle() };
						local = Mat.Compose(t, q, sc);
					}
					float[] world = Mat.Multiply(local, parent);
					string name = node.TryGetProperty("name", out JsonElement nn) ? nn.GetString() : "node" + nodeIndex;
					if (node.TryGetProperty("mesh", out JsonElement meshIndex) && meshes.ValueKind == JsonValueKind.Array)
					{
						foreach (JsonElement primitive in meshes[meshIndex.GetInt32()].GetProperty("primitives").EnumerateArray())
							result.AddPrimitive(primitive, world, name, accessors, views, buffers);
					}
					if (node.TryGetProperty("children", out JsonElement children))
						foreach (JsonElement child in children.EnumerateArray()) Visit(child.GetInt32(), world, depth + 1);
				}
				foreach (int r in roots) Visit(r, Mat.Identity(), 0);
			}
			if (result.Meshes.Count == 0) result.Notes.Add("no triangles in the file");
			return result;
		}

		private void AddPrimitive(JsonElement primitive, float[] world, string node, JsonElement accessors, JsonElement views, List<byte[]> buffers)
		{
			int mode = primitive.TryGetProperty("mode", out JsonElement md) ? md.GetInt32() : 4;
			if (mode != 4 && mode != 5 && mode != 6) return;
			JsonElement attributes = primitive.GetProperty("attributes");
			if (!attributes.TryGetProperty("POSITION", out JsonElement posAcc)) return;
			GltfMesh mesh = new GltfMesh { Node = node };
			mesh.Positions = Floats(accessors[posAcc.GetInt32()], views, buffers, 3);
			mesh.Normals = attributes.TryGetProperty("NORMAL", out JsonElement nAcc) ? Floats(accessors[nAcc.GetInt32()], views, buffers, 3) : null;
			mesh.Uvs = attributes.TryGetProperty("TEXCOORD_0", out JsonElement uvAcc) ? Floats(accessors[uvAcc.GetInt32()], views, buffers, 2) : null;
			if (attributes.TryGetProperty("COLOR_0", out JsonElement cAcc))
			{
				JsonElement acc = accessors[cAcc.GetInt32()];
				string type = acc.GetProperty("type").GetString();
				float[] c = Floats(acc, views, buffers, type == "VEC4" ? 4 : 3);
				if (type != "VEC4")
				{
					float[] rgba = new float[c.Length / 3 * 4];
					for (int i = 0; i < c.Length / 3; i++) { rgba[i * 4] = c[i * 3]; rgba[i * 4 + 1] = c[i * 3 + 1]; rgba[i * 4 + 2] = c[i * 3 + 2]; rgba[i * 4 + 3] = 1f; }
					c = rgba;
				}
				mesh.Colours = c;
			}
			mesh.Material = primitive.TryGetProperty("material", out JsonElement mi) ? mi.GetInt32() : -1;
			int count = mesh.VertexCount;
			int[] indices;
			if (primitive.TryGetProperty("indices", out JsonElement iAcc))
			{
				float[] f = Floats(accessors[iAcc.GetInt32()], views, buffers, 1);
				indices = new int[f.Length];
				for (int i = 0; i < f.Length; i++) indices[i] = (int)f[i];
			}
			else { indices = new int[count]; for (int i = 0; i < count; i++) indices[i] = i; }
			List<int> tris = new List<int>();
			if (mode == 4) tris.AddRange(indices);
			else if (mode == 5) for (int i = 2; i < indices.Length; i++) { if ((i & 1) == 0) { tris.Add(indices[i - 2]); tris.Add(indices[i - 1]); } else { tris.Add(indices[i - 1]); tris.Add(indices[i - 2]); } tris.Add(indices[i]); }
			else for (int i = 2; i < indices.Length; i++) { tris.Add(indices[0]); tris.Add(indices[i - 1]); tris.Add(indices[i]); }
			for (int i = 0; i < tris.Count; i++) if (tris[i] < 0 || tris[i] >= count) tris[i] = 0;
			mesh.Indices = tris.ToArray();
			Triangles += tris.Count / 3;

			// The node's matrix into the vertices.
			for (int v = 0; v < count; v++)
			{
				Mat.TransformPoint(world, mesh.Positions, v * 3);
				for (int k = 0; k < 3; k++) { Min[k] = Math.Min(Min[k], mesh.Positions[v * 3 + k]); Max[k] = Math.Max(Max[k], mesh.Positions[v * 3 + k]); }
				if (mesh.Normals != null) Mat.TransformNormal(world, mesh.Normals, v * 3);
			}
			Meshes.Add(mesh);
		}

		private static byte[] ReadUri(string uri, string folder)
		{
			if (uri.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) return Convert.FromBase64String(uri.Substring(uri.IndexOf(',') + 1));
			return File.ReadAllBytes(Path.Combine(folder, Uri.UnescapeDataString(uri)));
		}

		private static float[] Floats(JsonElement accessor, JsonElement views, List<byte[]> buffers, int width)
		{
			int count = accessor.GetProperty("count").GetInt32();
			int componentType = accessor.GetProperty("componentType").GetInt32();
			bool normalized = accessor.TryGetProperty("normalized", out JsonElement nz) && nz.GetBoolean();
			string type = accessor.GetProperty("type").GetString();
			int components = type switch { "SCALAR" => 1, "VEC2" => 2, "VEC3" => 3, "VEC4" => 4, _ => width };
			float[] result = new float[count * width];
			if (!accessor.TryGetProperty("bufferView", out JsonElement viewIndex)) return result;
			JsonElement view = views[viewIndex.GetInt32()];
			byte[] buffer = buffers[view.GetProperty("buffer").GetInt32()];
			int offset = (view.TryGetProperty("byteOffset", out JsonElement vo) ? vo.GetInt32() : 0) + (accessor.TryGetProperty("byteOffset", out JsonElement ao) ? ao.GetInt32() : 0);
			int size = componentType switch { 5120 => 1, 5121 => 1, 5122 => 2, 5123 => 2, 5125 => 4, _ => 4 };
			int stride = view.TryGetProperty("byteStride", out JsonElement bs) ? bs.GetInt32() : size * components;
			for (int i = 0; i < count; i++)
			{
				int at = offset + i * stride;
				for (int c = 0; c < width && c < components; c++)
				{
					int a = at + c * size;
					if (a + size > buffer.Length) break;
					float v = componentType switch
					{
						5126 => BitConverter.ToSingle(buffer, a),
						5121 => normalized ? buffer[a] / 255f : buffer[a],
						5120 => normalized ? Math.Max(-1f, (sbyte)buffer[a] / 127f) : (sbyte)buffer[a],
						5123 => normalized ? BitConverter.ToUInt16(buffer, a) / 65535f : BitConverter.ToUInt16(buffer, a),
						5122 => normalized ? Math.Max(-1f, BitConverter.ToInt16(buffer, a) / 32767f) : BitConverter.ToInt16(buffer, a),
						5125 => BitConverter.ToUInt32(buffer, a),
						_ => 0f
					};
					result[i * width + c] = v;
				}
			}
			return result;
		}

		/// <summary>4x4 matrices as glTF stores them - column-major, 16 floats - and the little arithmetic the node tree needs.</summary>
		private static class Mat
		{
			public static float[] Identity() => new float[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };

			/// <summary>local then parent: the point goes through local first.</summary>
			public static float[] Multiply(float[] local, float[] parent)
			{
				float[] r = new float[16];
				for (int col = 0; col < 4; col++)
					for (int row = 0; row < 4; row++)
					{
						float s = 0;
						for (int k = 0; k < 4; k++) s += parent[k * 4 + row] * local[col * 4 + k];
						r[col * 4 + row] = s;
					}
				return r;
			}

			public static float[] Compose(float[] t, float[] q, float[] s)
			{
				float x = q[0], y = q[1], z = q[2], w = q[3];
				float[] m = Identity();
				m[0] = (1 - 2 * (y * y + z * z)) * s[0]; m[1] = (2 * (x * y + z * w)) * s[0]; m[2] = (2 * (x * z - y * w)) * s[0];
				m[4] = (2 * (x * y - z * w)) * s[1]; m[5] = (1 - 2 * (x * x + z * z)) * s[1]; m[6] = (2 * (y * z + x * w)) * s[1];
				m[8] = (2 * (x * z + y * w)) * s[2]; m[9] = (2 * (y * z - x * w)) * s[2]; m[10] = (1 - 2 * (x * x + y * y)) * s[2];
				m[12] = t[0]; m[13] = t[1]; m[14] = t[2];
				return m;
			}

			public static void TransformPoint(float[] m, float[] v, int at)
			{
				float x = v[at], y = v[at + 1], z = v[at + 2];
				v[at] = m[0] * x + m[4] * y + m[8] * z + m[12];
				v[at + 1] = m[1] * x + m[5] * y + m[9] * z + m[13];
				v[at + 2] = m[2] * x + m[6] * y + m[10] * z + m[14];
			}

			public static void TransformNormal(float[] m, float[] v, int at)
			{
				float x = v[at], y = v[at + 1], z = v[at + 2];
				float nx = m[0] * x + m[4] * y + m[8] * z, ny = m[1] * x + m[5] * y + m[9] * z, nz = m[2] * x + m[6] * y + m[10] * z;
				float len = (float)Math.Sqrt(nx * nx + ny * ny + nz * nz);
				if (len > 1e-6f) { nx /= len; ny /= len; nz /= len; }
				v[at] = nx; v[at + 1] = ny; v[at + 2] = nz;
			}
		}
	}
}
