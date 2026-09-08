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

	/// <summary>A node of the tree: its own transform, its parent, what it draws.</summary>
	public sealed class GltfNode
	{
		public string Name;
		public int Parent = -1;
		public List<int> Children = new List<int>();
		public float[] Translation = { 0, 0, 0 };
		public float[] Rotation = { 0, 0, 0, 1 };
		public float[] Scale = { 1, 1, 1 };
		/// <summary>A node given as a matrix has no TRS to animate; it is used as it is.</summary>
		public float[] Matrix;
		public int Mesh = -1;
		public int Skin = -1;
	}

	/// <summary>A skin: the joints (node indices) and each one's inverse bind matrix (16 floats, column-major).</summary>
	public sealed class GltfSkin
	{
		public int[] Joints;
		public float[][] InverseBind;
	}

	/// <summary>One animated property of one node: keyframe times and values (3 per key for translation and scale, 4 for rotation).</summary>
	public sealed class GltfChannel
	{
		public int Node;
		public string Path;          // translation | rotation | scale
		public float[] Times;
		public float[] Values;
		public bool Step;            // STEP interpolation; LINEAR (and CUBICSPLINE, taken as linear on the values) otherwise
	}

	public sealed class GltfAnimation
	{
		public string Name;
		public List<GltfChannel> Channels = new List<GltfChannel>();
		public float Duration;
	}

	/// <summary>One primitive with its node's world matrix already applied to positions and normals: a triangle list. Local (the file's own, before the node) copies are kept for posing.</summary>
	public sealed class GltfMesh
	{
		public string Node;
		/// <summary>The node that draws it, in Nodes.</summary>
		public int NodeIndex = -1;
		/// <summary>The skin that poses it, in Skins, or -1.</summary>
		public int Skin = -1;
		/// <summary>The positions and normals as the file has them, before the node's matrix.</summary>
		public float[] LocalPositions, LocalNormals;
		/// <summary>Per vertex, four joint indices (into the skin's Joints) and four weights, when skinned.</summary>
		public float[] Joints, Weights;
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
		public List<GltfNode> Nodes = new List<GltfNode>();
		public List<GltfSkin> Skins = new List<GltfSkin>();
		public List<GltfAnimation> Animations = new List<GltfAnimation>();
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

				// Every node as the file has it, for posing later; the parent links from the children lists.
				if (nodes.ValueKind == JsonValueKind.Array)
				{
					for (int i = 0; i < nodes.GetArrayLength(); i++)
					{
						JsonElement node = nodes[i];
						GltfNode n = new GltfNode { Name = node.TryGetProperty("name", out JsonElement nn) ? nn.GetString() : "node" + i };
						if (node.TryGetProperty("matrix", out JsonElement mx)) { n.Matrix = new float[16]; for (int k = 0; k < 16; k++) n.Matrix[k] = mx[k].GetSingle(); }
						if (node.TryGetProperty("translation", out JsonElement tr)) n.Translation = new[] { tr[0].GetSingle(), tr[1].GetSingle(), tr[2].GetSingle() };
						if (node.TryGetProperty("rotation", out JsonElement ro)) n.Rotation = new[] { ro[0].GetSingle(), ro[1].GetSingle(), ro[2].GetSingle(), ro[3].GetSingle() };
						if (node.TryGetProperty("scale", out JsonElement sl)) n.Scale = new[] { sl[0].GetSingle(), sl[1].GetSingle(), sl[2].GetSingle() };
						if (node.TryGetProperty("mesh", out JsonElement nm)) n.Mesh = nm.GetInt32();
						if (node.TryGetProperty("skin", out JsonElement ns)) n.Skin = ns.GetInt32();
						if (node.TryGetProperty("children", out JsonElement ch)) foreach (JsonElement c in ch.EnumerateArray()) n.Children.Add(c.GetInt32());
						result.Nodes.Add(n);
					}
					for (int i = 0; i < result.Nodes.Count; i++) foreach (int c in result.Nodes[i].Children) if (c >= 0 && c < result.Nodes.Count) result.Nodes[c].Parent = i;
				}
				if (root.TryGetProperty("skins", out JsonElement skins))
				{
					foreach (JsonElement s in skins.EnumerateArray())
					{
						GltfSkin skin = new GltfSkin();
						List<int> joints = new List<int>();
						if (s.TryGetProperty("joints", out JsonElement js)) foreach (JsonElement j in js.EnumerateArray()) joints.Add(j.GetInt32());
						skin.Joints = joints.ToArray();
						skin.InverseBind = new float[joints.Count][];
						float[] ibm = s.TryGetProperty("inverseBindMatrices", out JsonElement ib) ? Floats(accessors[ib.GetInt32()], views, buffers, 16) : null;
						for (int j = 0; j < joints.Count; j++)
						{
							skin.InverseBind[j] = Mat.Identity();
							if (ibm != null && (j + 1) * 16 <= ibm.Length) Array.Copy(ibm, j * 16, skin.InverseBind[j], 0, 16);
						}
						result.Skins.Add(skin);
					}
				}
				if (root.TryGetProperty("animations", out JsonElement anims))
				{
					int number = 0;
					foreach (JsonElement a in anims.EnumerateArray())
					{
						GltfAnimation animation = new GltfAnimation { Name = a.TryGetProperty("name", out JsonElement an) ? an.GetString() : "animation" + number };
						number++;
						JsonElement samplers = a.GetProperty("samplers");
						foreach (JsonElement c in a.GetProperty("channels").EnumerateArray())
						{
							JsonElement target = c.GetProperty("target");
							if (!target.TryGetProperty("node", out JsonElement tn)) continue;
							string path = target.GetProperty("path").GetString();
							if (path != "translation" && path != "rotation" && path != "scale") continue;
							JsonElement sampler = samplers[c.GetProperty("sampler").GetInt32()];
							string interpolation = sampler.TryGetProperty("interpolation", out JsonElement ip) ? ip.GetString() : "LINEAR";
							int width = path == "rotation" ? 4 : 3;
							float[] times = Floats(accessors[sampler.GetProperty("input").GetInt32()], views, buffers, 1);
							float[] values = Floats(accessors[sampler.GetProperty("output").GetInt32()], views, buffers, width);
							if (interpolation == "CUBICSPLINE" && values.Length == times.Length * width * 3)
							{
								// The in-tangent, the value, the out-tangent per key: the values alone, read linearly.
								float[] plain = new float[times.Length * width];
								for (int k = 0; k < times.Length; k++) Array.Copy(values, (k * 3 + 1) * width, plain, k * width, width);
								values = plain;
							}
							animation.Channels.Add(new GltfChannel { Node = tn.GetInt32(), Path = path, Times = times, Values = values, Step = interpolation == "STEP" });
							if (times.Length > 0) animation.Duration = Math.Max(animation.Duration, times[times.Length - 1]);
						}
						result.Animations.Add(animation);
					}
				}

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
						int skin = node.TryGetProperty("skin", out JsonElement sk) ? sk.GetInt32() : -1;
						foreach (JsonElement primitive in meshes[meshIndex.GetInt32()].GetProperty("primitives").EnumerateArray())
							result.AddPrimitive(primitive, name, nodeIndex, skin, accessors, views, buffers);
					}
					if (node.TryGetProperty("children", out JsonElement children))
						foreach (JsonElement child in children.EnumerateArray()) Visit(child.GetInt32(), world, depth + 1);
				}
				foreach (int r in roots) Visit(r, Mat.Identity(), 0);
			}
			// The bind pose into the vertices: each mesh through its node, a skinned one through its joints.
			result.Pose(result.WorldMatrices(null, 0f));
			if (result.Meshes.Count == 0) result.Notes.Add("no triangles in the file");
			return result;
		}

		private void AddPrimitive(JsonElement primitive, string node, int nodeIndex, int skin, JsonElement accessors, JsonElement views, List<byte[]> buffers)
		{
			int mode = primitive.TryGetProperty("mode", out JsonElement md) ? md.GetInt32() : 4;
			if (mode != 4 && mode != 5 && mode != 6) return;
			JsonElement attributes = primitive.GetProperty("attributes");
			if (!attributes.TryGetProperty("POSITION", out JsonElement posAcc)) return;
			GltfMesh mesh = new GltfMesh { Node = node, NodeIndex = nodeIndex, Skin = skin < Skins.Count ? skin : -1 };
			if (mesh.Skin >= 0 && attributes.TryGetProperty("JOINTS_0", out JsonElement jAcc) && attributes.TryGetProperty("WEIGHTS_0", out JsonElement wAcc))
			{
				mesh.Joints = Floats(accessors[jAcc.GetInt32()], views, buffers, 4);
				mesh.Weights = Floats(accessors[wAcc.GetInt32()], views, buffers, 4);
			}
			else mesh.Skin = -1;
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

			mesh.LocalPositions = (float[])mesh.Positions.Clone();
			mesh.LocalNormals = mesh.Normals == null ? null : (float[])mesh.Normals.Clone();
			Meshes.Add(mesh);
		}

		/// <summary>
		/// Every node's world matrix (column-major 16 floats): the tree's own transforms, with an
		/// animation's channels at a time laid over them (a channel names a node and one of its
		/// translation, rotation or scale; keys are interpolated, rotations by normalised lerp).
		/// </summary>
		public float[][] WorldMatrices(GltfAnimation animation, float time)
		{
			int n = Nodes.Count;
			float[][] local = new float[n][];
			for (int i = 0; i < n; i++)
			{
				GltfNode node = Nodes[i];
				if (node.Matrix != null && animation == null) { local[i] = (float[])node.Matrix.Clone(); continue; }
				float[] t = node.Translation, r = node.Rotation, s = node.Scale;
				if (node.Matrix != null) { Mat.Decompose(node.Matrix, out t, out r, out s); }
				if (animation != null)
				{
					foreach (GltfChannel c in animation.Channels)
					{
						if (c.Node != i || c.Times.Length == 0) continue;
						float[] v = Sample(c, time);
						if (c.Path == "translation") t = v; else if (c.Path == "rotation") r = v; else s = v;
					}
				}
				local[i] = Mat.Compose(t, r, s);
			}
			float[][] world = new float[n][];
			for (int i = 0; i < n; i++) WorldOf(i, local, world, 0);
			return world;
		}

		private void WorldOf(int i, float[][] local, float[][] world, int depth)
		{
			if (world[i] != null || depth > 64) return;
			int parent = Nodes[i].Parent;
			if (parent < 0) { world[i] = local[i]; return; }
			WorldOf(parent, local, world, depth + 1);
			world[i] = Mat.Multiply(local[i], world[parent] ?? Mat.Identity());
		}

		private static float[] Sample(GltfChannel c, float time)
		{
			int width = c.Path == "rotation" ? 4 : 3;
			float[] times = c.Times;
			int last = times.Length - 1;
			if (time <= times[0]) return Slice(c.Values, 0, width);
			if (time >= times[last]) return Slice(c.Values, last, width);
			int k = 0;
			while (k < last && times[k + 1] <= time) k++;
			float[] a = Slice(c.Values, k, width), b = Slice(c.Values, Math.Min(k + 1, last), width);
			if (c.Step) return a;
			float span = times[k + 1] - times[k];
			float u = span > 0 ? (time - times[k]) / span : 0f;
			float[] r = new float[width];
			if (width == 4)
			{
				// The shorter way round, then a normalised lerp - a slerp's look for a step of a frame.
				float dot = a[0] * b[0] + a[1] * b[1] + a[2] * b[2] + a[3] * b[3];
				float sign = dot < 0 ? -1f : 1f;
				float len = 0;
				for (int i = 0; i < 4; i++) { r[i] = a[i] * (1 - u) + b[i] * sign * u; len += r[i] * r[i]; }
				len = (float)Math.Sqrt(len);
				if (len > 1e-6f) for (int i = 0; i < 4; i++) r[i] /= len;
				return r;
			}
			for (int i = 0; i < width; i++) r[i] = a[i] * (1 - u) + b[i] * u;
			return r;
		}

		private static float[] Slice(float[] values, int key, int width)
		{
			float[] r = new float[width];
			if ((key + 1) * width <= values.Length) Array.Copy(values, key * width, r, 0, width);
			else if (width == 4) r[3] = 1f;
			return r;
		}

		/// <summary>The meshes' Positions and Normals from their Local copies through the node matrices given: an unskinned mesh through its node, a skinned one through its joints and weights. Min and Max follow.</summary>
		public void Pose(float[][] world)
		{
			Min = new[] { float.MaxValue, float.MaxValue, float.MaxValue };
			Max = new[] { float.MinValue, float.MinValue, float.MinValue };
			float[][] jointMatrices = null;
			int jointSkin = -1;
			foreach (GltfMesh mesh in Meshes)
			{
				int count = mesh.VertexCount;
				if (mesh.Skin >= 0 && mesh.Skin < Skins.Count && mesh.Joints != null && mesh.Weights != null)
				{
					GltfSkin skin = Skins[mesh.Skin];
					if (jointSkin != mesh.Skin)
					{
						jointMatrices = new float[skin.Joints.Length][];
						for (int j = 0; j < skin.Joints.Length; j++)
						{
							int node = skin.Joints[j];
							float[] jointWorld = node >= 0 && node < world.Length && world[node] != null ? world[node] : Mat.Identity();
							jointMatrices[j] = Mat.Multiply(skin.InverseBind[j], jointWorld);
						}
						jointSkin = mesh.Skin;
					}
					for (int v = 0; v < count; v++)
					{
						float px = 0, py = 0, pz = 0, nx = 0, ny = 0, nz = 0;
						for (int k = 0; k < 4; k++)
						{
							float w = mesh.Weights[v * 4 + k];
							if (w <= 0) continue;
							int j = (int)mesh.Joints[v * 4 + k];
							if (j < 0 || j >= jointMatrices.Length) continue;
							float[] m = jointMatrices[j];
							float x = mesh.LocalPositions[v * 3], y = mesh.LocalPositions[v * 3 + 1], z = mesh.LocalPositions[v * 3 + 2];
							px += w * (m[0] * x + m[4] * y + m[8] * z + m[12]);
							py += w * (m[1] * x + m[5] * y + m[9] * z + m[13]);
							pz += w * (m[2] * x + m[6] * y + m[10] * z + m[14]);
							if (mesh.LocalNormals != null)
							{
								float a = mesh.LocalNormals[v * 3], b = mesh.LocalNormals[v * 3 + 1], c = mesh.LocalNormals[v * 3 + 2];
								nx += w * (m[0] * a + m[4] * b + m[8] * c); ny += w * (m[1] * a + m[5] * b + m[9] * c); nz += w * (m[2] * a + m[6] * b + m[10] * c);
							}
						}
						mesh.Positions[v * 3] = px; mesh.Positions[v * 3 + 1] = py; mesh.Positions[v * 3 + 2] = pz;
						if (mesh.Normals != null)
						{
							float len = (float)Math.Sqrt(nx * nx + ny * ny + nz * nz);
							if (len > 1e-6f) { nx /= len; ny /= len; nz /= len; }
							mesh.Normals[v * 3] = nx; mesh.Normals[v * 3 + 1] = ny; mesh.Normals[v * 3 + 2] = nz;
						}
					}
				}
				else
				{
					float[] m = mesh.NodeIndex >= 0 && mesh.NodeIndex < world.Length && world[mesh.NodeIndex] != null ? world[mesh.NodeIndex] : Mat.Identity();
					Array.Copy(mesh.LocalPositions, mesh.Positions, mesh.LocalPositions.Length);
					if (mesh.LocalNormals != null) Array.Copy(mesh.LocalNormals, mesh.Normals, mesh.LocalNormals.Length);
					for (int v = 0; v < count; v++)
					{
						Mat.TransformPoint(m, mesh.Positions, v * 3);
						if (mesh.Normals != null) Mat.TransformNormal(m, mesh.Normals, v * 3);
					}
				}
				for (int v = 0; v < count; v++)
					for (int k = 0; k < 3; k++) { Min[k] = Math.Min(Min[k], mesh.Positions[v * 3 + k]); Max[k] = Math.Max(Max[k], mesh.Positions[v * 3 + k]); }
			}
			if (Meshes.Count == 0) { Min = new float[3]; Max = new float[3]; }
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

			/// <summary>A matrix back into translation, rotation and scale (no shear expected of a node), so an animation can replace one of them.</summary>
			public static void Decompose(float[] m, out float[] t, out float[] q, out float[] s)
			{
				t = new[] { m[12], m[13], m[14] };
				float sx = (float)Math.Sqrt(m[0] * m[0] + m[1] * m[1] + m[2] * m[2]);
				float sy = (float)Math.Sqrt(m[4] * m[4] + m[5] * m[5] + m[6] * m[6]);
				float sz = (float)Math.Sqrt(m[8] * m[8] + m[9] * m[9] + m[10] * m[10]);
				s = new[] { sx, sy, sz };
				float r00 = sx > 0 ? m[0] / sx : 1, r01 = sx > 0 ? m[1] / sx : 0, r02 = sx > 0 ? m[2] / sx : 0;
				float r10 = sy > 0 ? m[4] / sy : 0, r11 = sy > 0 ? m[5] / sy : 1, r12 = sy > 0 ? m[6] / sy : 0;
				float r20 = sz > 0 ? m[8] / sz : 0, r21 = sz > 0 ? m[9] / sz : 0, r22 = sz > 0 ? m[10] / sz : 1;
				// Column-major: column c is the image of axis c; the quaternion from the rotation part.
				float trace = r00 + r11 + r22;
				float x, y, z, w;
				if (trace > 0) { float k = (float)Math.Sqrt(trace + 1) * 2; w = k / 4; x = (r12 - r21) / k; y = (r20 - r02) / k; z = (r01 - r10) / k; }
				else if (r00 > r11 && r00 > r22) { float k = (float)Math.Sqrt(1 + r00 - r11 - r22) * 2; w = (r12 - r21) / k; x = k / 4; y = (r10 + r01) / k; z = (r20 + r02) / k; }
				else if (r11 > r22) { float k = (float)Math.Sqrt(1 + r11 - r00 - r22) * 2; w = (r20 - r02) / k; x = (r10 + r01) / k; y = k / 4; z = (r21 + r12) / k; }
				else { float k = (float)Math.Sqrt(1 + r22 - r00 - r11) * 2; w = (r01 - r10) / k; x = (r20 + r02) / k; y = (r21 + r12) / k; z = k / 4; }
				q = new[] { x, y, z, w };
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
