// A glTF written from code: meshes with positions, normals and texture coordinates, materials
// with a colour or an embedded PNG, a node tree, and animations of node translation, rotation
// and scale - as one .glb (the JSON and one binary buffer). GltfWriter writes the smallest
// file the editor needs (a box); this is for the sample assets (SampleAssets.cs), which show
// a modder what a Blender export can carry into the game, and it writes nothing the client's
// reader (Shared/Graphics/GltfFile.cs) does not read back.
//
// Geometry comes as Parts: a Part is one primitive's arrays under one material, with helpers
// for the shapes the samples are built from (boxes, cylinders and cones, spheres, rings, lathes)
// and a transform to place them. Normals are the faces' own (every shape is built with its
// vertices split per face where the surface is meant to be sharp, and shared where smooth).

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Crystal.Editor
{
	/// <summary>One primitive: positions, normals, texture coordinates and triangle indices under one material.</summary>
	internal sealed class Part
	{
		public readonly List<float> Positions = new List<float>();
		public readonly List<float> Normals = new List<float>();
		public readonly List<float> Uvs = new List<float>();
		public readonly List<int> Indices = new List<int>();
		public int Material;

		public Part(int material) { Material = material; }

		public int VertexCount => Positions.Count / 3;

		public int Vertex(float x, float y, float z, float nx, float ny, float nz, float u, float v)
		{
			Positions.Add(x); Positions.Add(y); Positions.Add(z);
			Normals.Add(nx); Normals.Add(ny); Normals.Add(nz);
			Uvs.Add(u); Uvs.Add(v);
			return VertexCount - 1;
		}

		public void Triangle(int a, int b, int c) { Indices.Add(a); Indices.Add(b); Indices.Add(c); }
		public void Quad(int a, int b, int c, int d) { Triangle(a, b, c); Triangle(a, c, d); }

		/// <summary>A flat quad from four corners (counter-clockwise seen from outside), its normal the face's, the texture across it.</summary>
		public void Face(float[] a, float[] b, float[] c, float[] d, float uScale = 1f, float vScale = 1f)
		{
			float[] n = Normal(a, b, c);
			int i0 = Vertex(a[0], a[1], a[2], n[0], n[1], n[2], 0, 0);
			int i1 = Vertex(b[0], b[1], b[2], n[0], n[1], n[2], uScale, 0);
			int i2 = Vertex(c[0], c[1], c[2], n[0], n[1], n[2], uScale, vScale);
			int i3 = Vertex(d[0], d[1], d[2], n[0], n[1], n[2], 0, vScale);
			Quad(i0, i1, i2, i3);
		}

		/// <summary>A flat triangle from three corners (counter-clockwise seen from outside).</summary>
		public void Tri(float[] a, float[] b, float[] c, float[] uvA = null, float[] uvB = null, float[] uvC = null)
		{
			float[] n = Normal(a, b, c);
			uvA ??= new[] { 0f, 0f }; uvB ??= new[] { 1f, 0f }; uvC ??= new[] { 0.5f, 1f };
			int i0 = Vertex(a[0], a[1], a[2], n[0], n[1], n[2], uvA[0], uvA[1]);
			int i1 = Vertex(b[0], b[1], b[2], n[0], n[1], n[2], uvB[0], uvB[1]);
			int i2 = Vertex(c[0], c[1], c[2], n[0], n[1], n[2], uvC[0], uvC[1]);
			Triangle(i0, i1, i2);
		}

		/// <summary>A box between two corners, six faces, the texture once across each face (or tiled by `tile` units per repeat).</summary>
		public void Box(float x0, float y0, float z0, float x1, float y1, float z1, float tile = 0f)
		{
			float U(float len) => tile > 0 ? len / tile : 1f;
			float[] a = { x0, y0, z0 }, b = { x1, y0, z0 }, c = { x1, y1, z0 }, d = { x0, y1, z0 };
			float[] e = { x0, y0, z1 }, f = { x1, y0, z1 }, g = { x1, y1, z1 }, h = { x0, y1, z1 };
			Face(e, f, g, h, U(x1 - x0), U(y1 - y0));   // +z
			Face(b, a, d, c, U(x1 - x0), U(y1 - y0));   // -z
			Face(f, b, c, g, U(z1 - z0), U(y1 - y0));   // +x
			Face(a, e, h, d, U(z1 - z0), U(y1 - y0));   // -x
			Face(h, g, c, d, U(x1 - x0), U(z1 - z0));   // +y
			Face(a, b, f, e, U(x1 - x0), U(z1 - z0));   // -y
		}

		/// <summary>
		/// A lathe around the Y axis: a profile of (radius, y) points swept in `segments` steps,
		/// smooth across the sweep, sharp between profile rows. The texture's u runs around, v
		/// down the profile. Closed at either end when its radius is 0 (a cone's point) - or
		/// with a flat cap when `capTop` / `capBottom` say so.
		/// </summary>
		public void Lathe(float[][] profile, int segments, bool capBottom = false, bool capTop = false)
		{
			int rows = profile.Length;
			int[][] ring = new int[rows][];
			for (int r = 0; r < rows; r++)
			{
				ring[r] = new int[segments + 1];
				// The normal of the surface at this row: perpendicular to the profile's slope here.
				float[] prev = profile[Math.Max(0, r - 1)], next = profile[Math.Min(rows - 1, r + 1)];
				float dr = next[0] - prev[0], dy = next[1] - prev[1];
				float len = MathF.Sqrt(dr * dr + dy * dy);
				float nr = len > 0 ? dy / len : 1f, ny = len > 0 ? -dr / len : 0f;
				for (int s = 0; s <= segments; s++)
				{
					float angle = s * MathF.PI * 2 / segments;
					float cs = MathF.Cos(angle), sn = MathF.Sin(angle);
					ring[r][s] = Vertex(profile[r][0] * cs, profile[r][1], profile[r][0] * sn, nr * cs, ny, nr * sn, (float)s / segments, (float)r / (rows - 1));
				}
			}
			for (int r = 0; r + 1 < rows; r++)
				for (int s = 0; s < segments; s++)
				{
					if (profile[r][0] == 0 && profile[r + 1][0] == 0) continue;
					Quad(ring[r][s], ring[r + 1][s], ring[r + 1][s + 1], ring[r][s + 1]);
				}
			if (capBottom && profile[0][0] > 0) Cap(profile[0][0], profile[0][1], segments, false);
			if (capTop && profile[rows - 1][0] > 0) Cap(profile[rows - 1][0], profile[rows - 1][1], segments, true);
		}

		private void Cap(float radius, float y, int segments, bool up)
		{
			float ny = up ? 1f : -1f;
			int centre = Vertex(0, y, 0, 0, ny, 0, 0.5f, 0.5f);
			int[] rim = new int[segments + 1];
			for (int s = 0; s <= segments; s++)
			{
				float angle = s * MathF.PI * 2 / segments;
				rim[s] = Vertex(radius * MathF.Cos(angle), y, radius * MathF.Sin(angle), 0, ny, 0, 0.5f + 0.5f * MathF.Cos(angle), 0.5f + 0.5f * MathF.Sin(angle));
			}
			for (int s = 0; s < segments; s++)
			{
				if (up) Triangle(centre, rim[s + 1], rim[s]); else Triangle(centre, rim[s], rim[s + 1]);
			}
		}

		/// <summary>A cylinder (or a cone when a radius is 0) along Y from y0 to y1, with caps.</summary>
		public void Cylinder(float radiusBottom, float radiusTop, float y0, float y1, int segments, bool caps = true)
		{
			Lathe(new[] { new[] { radiusBottom, y0 }, new[] { radiusTop, y1 } }, segments, caps, caps);
		}

		/// <summary>A sphere of `rings` latitude rows and `segments` around, smooth.</summary>
		public void Sphere(float radius, int rings, int segments)
		{
			float[][] profile = new float[rings + 1][];
			for (int r = 0; r <= rings; r++)
			{
				float t = MathF.PI * r / rings;
				profile[r] = new[] { radius * MathF.Sin(t), radius * MathF.Cos(t) };
			}
			// A sphere's normals are radial: the lathe's slope normals are that here.
			Lathe(profile, segments);
		}

		/// <summary>A ring (torus) in the XZ plane: `radius` to the tube's centre, `tube` the tube's radius.</summary>
		public void Torus(float radius, float tube, int segments, int sides)
		{
			int[][] grid = new int[segments + 1][];
			for (int s = 0; s <= segments; s++)
			{
				grid[s] = new int[sides + 1];
				float a = s * MathF.PI * 2 / segments, ca = MathF.Cos(a), sa = MathF.Sin(a);
				for (int k = 0; k <= sides; k++)
				{
					float b = k * MathF.PI * 2 / sides, cb = MathF.Cos(b), sb = MathF.Sin(b);
					float nx = ca * cb, ny = sb, nz = sa * cb;
					grid[s][k] = Vertex((radius + tube * cb) * ca, tube * sb, (radius + tube * cb) * sa, nx, ny, nz, (float)s / segments, (float)k / sides);
				}
			}
			for (int s = 0; s < segments; s++)
				for (int k = 0; k < sides; k++)
					Quad(grid[s][k], grid[s][k + 1], grid[s + 1][k + 1], grid[s + 1][k]);
		}

		/// <summary>Moves every vertex added since `fromVertex` through a transform (positions), and its rotation (normals).</summary>
		public void Transform(int fromVertex, Xform x)
		{
			for (int i = fromVertex; i < VertexCount; i++)
			{
				float px = Positions[i * 3], py = Positions[i * 3 + 1], pz = Positions[i * 3 + 2];
				float[] p = x.Point(px, py, pz);
				Positions[i * 3] = p[0]; Positions[i * 3 + 1] = p[1]; Positions[i * 3 + 2] = p[2];
				float nx = Normals[i * 3], ny = Normals[i * 3 + 1], nz = Normals[i * 3 + 2];
				float[] n = x.Direction(nx, ny, nz);
				float len = MathF.Sqrt(n[0] * n[0] + n[1] * n[1] + n[2] * n[2]);
				if (len > 0) { n[0] /= len; n[1] /= len; n[2] /= len; }
				Normals[i * 3] = n[0]; Normals[i * 3 + 1] = n[1]; Normals[i * 3 + 2] = n[2];
			}
		}

		/// <summary>Another part's geometry appended (its material ignored).</summary>
		public void Append(Part other)
		{
			int baseIndex = VertexCount;
			Positions.AddRange(other.Positions); Normals.AddRange(other.Normals); Uvs.AddRange(other.Uvs);
			foreach (int i in other.Indices) Indices.Add(i + baseIndex);
		}

		public static float[] Normal(float[] a, float[] b, float[] c)
		{
			float ux = b[0] - a[0], uy = b[1] - a[1], uz = b[2] - a[2], vx = c[0] - a[0], vy = c[1] - a[1], vz = c[2] - a[2];
			float nx = uy * vz - uz * vy, ny = uz * vx - ux * vz, nz = ux * vy - uy * vx;
			float len = MathF.Sqrt(nx * nx + ny * ny + nz * nz);
			return len > 0 ? new[] { nx / len, ny / len, nz / len } : new[] { 0f, 1f, 0f };
		}
	}

	/// <summary>A 3x4 transform: scale, then rotation, then translation, composable.</summary>
	internal sealed class Xform
	{
		// Row-major 3x3 rotation*scale and a translation.
		public float[] M = { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
		public float[] T = { 0, 0, 0 };

		public static Xform Identity => new Xform();
		public static Xform Translate(float x, float y, float z) => new Xform { T = new[] { x, y, z } };
		public static Xform Scale(float s) => Scale(s, s, s);
		public static Xform Scale(float x, float y, float z) => new Xform { M = new[] { x, 0, 0, 0, y, 0, 0, 0, z } };
		public static Xform RotateX(float degrees) { float a = degrees * MathF.PI / 180, c = MathF.Cos(a), s = MathF.Sin(a); return new Xform { M = new[] { 1, 0, 0, 0, c, -s, 0, s, c } }; }
		public static Xform RotateY(float degrees) { float a = degrees * MathF.PI / 180, c = MathF.Cos(a), s = MathF.Sin(a); return new Xform { M = new[] { c, 0, s, 0, 1, 0, -s, 0, c } }; }
		public static Xform RotateZ(float degrees) { float a = degrees * MathF.PI / 180, c = MathF.Cos(a), s = MathF.Sin(a); return new Xform { M = new[] { c, -s, 0, s, c, 0, 0, 0, 1 } }; }

		/// <summary>This transform, then `next` (next applied after this).</summary>
		public Xform Then(Xform next)
		{
			Xform r = new Xform();
			for (int i = 0; i < 3; i++)
				for (int j = 0; j < 3; j++)
					r.M[i * 3 + j] = next.M[i * 3] * M[j] + next.M[i * 3 + 1] * M[3 + j] + next.M[i * 3 + 2] * M[6 + j];
			float[] t = next.Direction(T[0], T[1], T[2]);
			r.T = new[] { t[0] + next.T[0], t[1] + next.T[1], t[2] + next.T[2] };
			return r;
		}

		public float[] Point(float x, float y, float z)
		{
			float[] d = Direction(x, y, z);
			return new[] { d[0] + T[0], d[1] + T[1], d[2] + T[2] };
		}

		public float[] Direction(float x, float y, float z) => new[]
		{
			M[0] * x + M[1] * y + M[2] * z,
			M[3] * x + M[4] * y + M[5] * z,
			M[6] * x + M[7] * y + M[8] * z,
		};
	}

	internal sealed class GltfBuilder
	{
		private readonly List<string> _materials = new List<string>();
		private readonly List<byte[]> _images = new List<byte[]>();
		private readonly List<(string name, List<Part> parts)> _meshes = new List<(string, List<Part>)>();
		private readonly List<(string name, int mesh, int parent, float[] t, float[] q, float[] s)> _nodes = new List<(string, int, int, float[], float[], float[])>();
		private readonly List<(string name, List<(int node, string path, float[] times, float[] values)> channels)> _animations = new List<(string, List<(int, string, float[], float[])>)>();

		/// <summary>A material: a colour, and a PNG as its texture when given; double-sided draws both faces.</summary>
		public int Material(string name, float r, float g, float b, float a = 1f, byte[] png = null, bool doubleSided = false)
		{
			string texture = "";
			if (png != null)
			{
				_images.Add(png);
				texture = ", \"baseColorTexture\": { \"index\": " + (_images.Count - 1) + " }";
			}
			_materials.Add("{ \"name\": " + Json(name) + ", \"doubleSided\": " + (doubleSided ? "true" : "false") + ", \"pbrMetallicRoughness\": { \"baseColorFactor\": [" + F(r) + ", " + F(g) + ", " + F(b) + ", " + F(a) + "], \"metallicFactor\": 0.0, \"roughnessFactor\": 0.9" + texture + " } }");
			return _materials.Count - 1;
		}

		public int Mesh(string name, params Part[] parts)
		{
			_meshes.Add((name, parts.Where(p => p.Indices.Count > 0).ToList()));
			return _meshes.Count - 1;
		}

		/// <summary>A node; `q` is a quaternion (x, y, z, w).</summary>
		public int Node(string name, int mesh = -1, int parent = -1, float[] translation = null, float[] rotation = null, float[] scale = null)
		{
			_nodes.Add((name, mesh, parent, translation, rotation, scale));
			return _nodes.Count - 1;
		}

		public void Animation(string name, params (int node, string path, float[] times, float[] values)[] channels)
		{
			_animations.Add((name, channels.ToList()));
		}

		/// <summary>A quaternion for a turn about an axis.</summary>
		public static float[] Quaternion(float ax, float ay, float az, float degrees)
		{
			float len = MathF.Sqrt(ax * ax + ay * ay + az * az);
			if (len > 0) { ax /= len; ay /= len; az /= len; }
			float half = degrees * MathF.PI / 360, s = MathF.Sin(half);
			return new[] { ax * s, ay * s, az * s, MathF.Cos(half) };
		}

		/// <summary>The file as .glb: one JSON chunk, one BIN chunk with every array and picture.</summary>
		public byte[] Glb()
		{
			MemoryStream bin = new MemoryStream();
			List<string> views = new List<string>(), accessors = new List<string>(), meshesJson = new List<string>();
			int View(byte[] bytes, int? target = null)
			{
				while (bin.Length % 4 != 0) bin.WriteByte(0);
				int offset = (int)bin.Length;
				bin.Write(bytes, 0, bytes.Length);
				views.Add("{ \"buffer\": 0, \"byteOffset\": " + offset + ", \"byteLength\": " + bytes.Length + (target.HasValue ? ", \"target\": " + target.Value : "") + " }");
				return views.Count - 1;
			}
			int FloatAccessor(List<float> data, int width, bool bounds)
			{
				byte[] bytes = new byte[data.Count * 4];
				for (int i = 0; i < data.Count; i++) BitConverter.GetBytes(data[i]).CopyTo(bytes, i * 4);
				int view = View(bytes, 34962);
				string type = width switch { 1 => "SCALAR", 2 => "VEC2", 3 => "VEC3", _ => "VEC4" };
				string minMax = "";
				if (bounds)
				{
					float[] min = Enumerable.Repeat(float.MaxValue, width).ToArray(), max = Enumerable.Repeat(float.MinValue, width).ToArray();
					for (int i = 0; i < data.Count; i++) { min[i % width] = Math.Min(min[i % width], data[i]); max[i % width] = Math.Max(max[i % width], data[i]); }
					minMax = ", \"min\": [" + string.Join(", ", min.Select(F)) + "], \"max\": [" + string.Join(", ", max.Select(F)) + "]";
				}
				accessors.Add("{ \"bufferView\": " + view + ", \"componentType\": 5126, \"count\": " + (data.Count / width) + ", \"type\": \"" + type + "\"" + minMax + " }");
				return accessors.Count - 1;
			}
			int IndexAccessor(List<int> indices)
			{
				byte[] bytes = new byte[indices.Count * 4];
				for (int i = 0; i < indices.Count; i++) BitConverter.GetBytes((uint)indices[i]).CopyTo(bytes, i * 4);
				int view = View(bytes, 34963);
				accessors.Add("{ \"bufferView\": " + view + ", \"componentType\": 5125, \"count\": " + indices.Count + ", \"type\": \"SCALAR\" }");
				return accessors.Count - 1;
			}

			foreach ((string name, List<Part> parts) in _meshes)
			{
				List<string> primitives = new List<string>();
				foreach (Part p in parts)
				{
					int pos = FloatAccessor(p.Positions, 3, true), nrm = FloatAccessor(p.Normals, 3, false), uv = FloatAccessor(p.Uvs, 2, false), idx = IndexAccessor(p.Indices);
					primitives.Add("{ \"attributes\": { \"POSITION\": " + pos + ", \"NORMAL\": " + nrm + ", \"TEXCOORD_0\": " + uv + " }, \"indices\": " + idx + ", \"material\": " + p.Material + " }");
				}
				meshesJson.Add("{ \"name\": " + Json(name) + ", \"primitives\": [" + string.Join(", ", primitives) + "] }");
			}

			List<string> images = new List<string>(), textures = new List<string>();
			for (int i = 0; i < _images.Count; i++)
			{
				int view = View(_images[i]);
				images.Add("{ \"bufferView\": " + view + ", \"mimeType\": \"image/png\" }");
				textures.Add("{ \"source\": " + i + ", \"sampler\": 0 }");
			}

			List<string> nodesJson = new List<string>();
			List<int> roots = new List<int>();
			for (int i = 0; i < _nodes.Count; i++)
			{
				(string name, int mesh, int parent, float[] t, float[] q, float[] s) = _nodes[i];
				List<string> fields = new List<string> { "\"name\": " + Json(name) };
				if (mesh >= 0) fields.Add("\"mesh\": " + mesh);
				if (t != null) fields.Add("\"translation\": [" + string.Join(", ", t.Select(F)) + "]");
				if (q != null) fields.Add("\"rotation\": [" + string.Join(", ", q.Select(F)) + "]");
				if (s != null) fields.Add("\"scale\": [" + string.Join(", ", s.Select(F)) + "]");
				List<int> children = new List<int>();
				for (int k = 0; k < _nodes.Count; k++) if (_nodes[k].parent == i) children.Add(k);
				if (children.Count > 0) fields.Add("\"children\": [" + string.Join(", ", children) + "]");
				nodesJson.Add("{ " + string.Join(", ", fields) + " }");
				if (parent < 0) roots.Add(i);
			}

			List<string> animationsJson = new List<string>();
			foreach ((string name, List<(int node, string path, float[] times, float[] values)> channels) in _animations)
			{
				List<string> samplers = new List<string>(), chans = new List<string>();
				foreach ((int node, string path, float[] times, float[] values) in channels)
				{
					int input = FloatAccessor(times.ToList(), 1, true);
					int output = FloatAccessor(values.ToList(), path == "rotation" ? 4 : 3, false);
					samplers.Add("{ \"input\": " + input + ", \"output\": " + output + ", \"interpolation\": \"LINEAR\" }");
					chans.Add("{ \"sampler\": " + (samplers.Count - 1) + ", \"target\": { \"node\": " + node + ", \"path\": \"" + path + "\" } }");
				}
				animationsJson.Add("{ \"name\": " + Json(name) + ", \"samplers\": [" + string.Join(", ", samplers) + "], \"channels\": [" + string.Join(", ", chans) + "] }");
			}

			while (bin.Length % 4 != 0) bin.WriteByte(0);
			StringBuilder json = new StringBuilder();
			json.Append("{ \"asset\": { \"version\": \"2.0\", \"generator\": \"Crystal sample-assets\" }, \"scene\": 0, \"scenes\": [ { \"nodes\": [" + string.Join(", ", roots) + "] } ],\n");
			json.Append("  \"nodes\": [" + string.Join(",\n    ", nodesJson) + "],\n");
			json.Append("  \"meshes\": [" + string.Join(",\n    ", meshesJson) + "],\n");
			json.Append("  \"materials\": [" + string.Join(",\n    ", _materials) + "],\n");
			if (images.Count > 0)
			{
				json.Append("  \"images\": [" + string.Join(", ", images) + "],\n");
				json.Append("  \"textures\": [" + string.Join(", ", textures) + "],\n");
				json.Append("  \"samplers\": [ { \"magFilter\": 9729, \"minFilter\": 9987, \"wrapS\": 10497, \"wrapT\": 10497 } ],\n");
			}
			if (animationsJson.Count > 0) json.Append("  \"animations\": [" + string.Join(",\n    ", animationsJson) + "],\n");
			json.Append("  \"accessors\": [" + string.Join(",\n    ", accessors) + "],\n");
			json.Append("  \"bufferViews\": [" + string.Join(",\n    ", views) + "],\n");
			json.Append("  \"buffers\": [ { \"byteLength\": " + bin.Length + " } ] }\n");
			byte[] jsonBytes = Encoding.UTF8.GetBytes(json.ToString());
			int jsonPadded = (jsonBytes.Length + 3) / 4 * 4;
			byte[] binBytes = bin.ToArray();

			MemoryStream glb = new MemoryStream();
			void U32(uint v) { glb.Write(BitConverter.GetBytes(v), 0, 4); }
			U32(0x46546C67); U32(2); U32((uint)(12 + 8 + jsonPadded + 8 + binBytes.Length));
			U32((uint)jsonPadded); U32(0x4E4F534A); glb.Write(jsonBytes, 0, jsonBytes.Length); for (int i = jsonBytes.Length; i < jsonPadded; i++) glb.WriteByte(0x20);
			U32((uint)binBytes.Length); U32(0x004E4942); glb.Write(binBytes, 0, binBytes.Length);
			return glb.ToArray();
		}

		private static string F(float v) => v.ToString("0.#####", CultureInfo.InvariantCulture);
		private static string Json(string s) => "\"" + (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
	}
}
