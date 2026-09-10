// Writing a model in the game's own format - a glTF into an NMDP package (BMD0 with one MDL0)
// and its texture package (the .ntxp beside it, Tex0Write.Build), which is what a Steam target
// needs: the Steam games read nothing but these. The OpenFF client reads them too, through the
// same recreated loader, which is how the writer was checked.
//
//   crystal mdl-import <file.glb|.gltf> <name> [out-dir] [--scale=n]
//
// The layout follows w005.nmdp, a weapon of the game's, byte for byte in its shape: a 48-byte
// NMDP head (kind 2, the BMD0's size at 24 and its offset 48 at 28); BMD0 with one block; the
// MDL0's model dictionary; the model - 20 bytes of offsets, the 44-byte info, the node
// dictionary and its node data, the SBC, the material block, the shape block - and no
// envelope matrices (ofsEvpMtx is the model's size, as the shipped files have it).
//
// What a glTF becomes: one node ("root", the identity - the meshes' world-space vertices are
// what the file's node tree makes of them, so a Blender hierarchy flattens into the model);
// one material and one shape per glTF material in use, the shape a display list of
// triangles with a normal and a texture coordinate per vertex; one texture per material -
// the material's picture (PNG; a JPEG is refused with a note and the colour stands in) times
// its base colour, or an 8x8 of the base colour for a picture-less material - resampled to a
// power-of-two side of 8..1024 and written as pal256 (transparent index 0 when the picture has
// any). Vertices are VTX_16 (1.12 fixed, +-8) under a position scale that is the smallest
// power of two the model fits; normals are 10-bit; texture coordinates are texels in 12.4.
// The material words (lighting, polygon attributes, wrap) are w005's; a double-sided glTF
// material renders both faces.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenFF.Graphics;

namespace Crystal
{
	internal static class Mdl0Write
	{
		/// <summary>
		/// The game gathers one BEGIN_VTXS..END_VTXS run into a buffer of 20,480 vertices and a
		/// whole model's into one of 32,768 (GlobalScope.DrawModel's `vertex` and `vtc`; the Steam
		/// port's arrays, so the Steam game's limits too). A run is split before the first;
		/// a model over the second is refused rather than written to crash.
		/// </summary>
		internal const int RunLimit = 20478, ModelLimit = 32768;

		/// <summary>
		/// The model-size check: a Steam target's model over ModelLimit is refused (the game
		/// would crash on it); an OpenFF target's (<paramref name="generous"/>) is written with a
		/// note, since the OpenFF client grows its buffers to fit.
		/// </summary>
		internal static void CheckSize(int vertices, int triangles, bool generous, List<string> notes)
		{
			if (vertices <= ModelLimit) return;
			string size = triangles.ToString("N0") + " triangles (" + vertices.ToString("N0") + " vertices)";
			if (!generous) throw new InvalidDataException(size + ": the Steam game draws at most " + (ModelLimit / 3).ToString("N0") + " triangles per model - decimate the mesh in Blender, or target OpenFF, which has no such limit");
			notes.Add(size + ": over the Steam game's " + (ModelLimit / 3).ToString("N0") + " per model - fine on OpenFF, which grows its buffers; a Steam target would refuse this file");
		}

		/// <summary>What the writer made: the two packages (uncompressed; the game's files are these under .lz) and a note of what went in.</summary>
		public sealed class Result
		{
			public byte[] Nmdp;
			public byte[] Ntxp;
			public int Vertices, Triangles, Materials;
			public List<string> Notes = new List<string>();
		}

		/// <summary>
		/// The glTF as the game's model `name` (w123, n099: at most 12 characters, the texture
		/// names are made from it) at `scale` times the file's units. Throws with the reason when
		/// the file has nothing to write or does not fit the format.
		/// </summary>
		public static Result Build(GltfFile file, string name, float scale = 1f, float[] rotation = null, float[] offset = null, bool generous = false)
		{
			if (file == null) throw new ArgumentNullException(nameof(file));
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("a model needs a name");
			name = name.Trim();
			if (name.Length > 12 || name.Any(c => c > 127 || c == '/' || c == '\\' || c == '.')) throw new ArgumentException("a model's name is at most 12 plain characters (w123)");
			if (scale <= 0) scale = 1f;
			// The definition's fit into the hand (modelRotation: degrees about x, then y, then z; modelOffset),
			// baked into the vertices - the same placement the OpenFF client gives the glTF (WeaponMeshes.Fit).
			float[] fit = FitMatrix(scale, rotation, offset);
			Result result = new Result();

			// The meshes with triangles, grouped by material; a mesh with no material takes a flat white.
			List<GltfMesh> meshes = file.Meshes.Where(m => m.Indices != null && m.Indices.Length >= 3 && m.Positions != null).ToList();
			if (meshes.Count == 0) throw new InvalidDataException("the file has no triangles");
			List<int> materialIds = meshes.Select(m => m.Material).Distinct().OrderBy(i => i).ToList();
			if (materialIds.Count > 255) throw new InvalidDataException("more than 255 materials");

			// Textures: one per material.
			List<Tex0Write.NewTexture> textures = new List<Tex0Write.NewTexture>();
			List<bool> doubleSided = new List<bool>();
			for (int i = 0; i < materialIds.Count; i++)
			{
				GltfMaterial material = materialIds[i] >= 0 && materialIds[i] < file.Materials.Count ? file.Materials[materialIds[i]] : new GltfMaterial();
				string texName = materialIds.Count == 1 ? name : name + "_" + i;
				textures.Add(TextureOf(file, material, texName, result.Notes));
				doubleSided.Add(material.DoubleSided);
			}

			// The texel block's size is a 16-bit count of 8-byte units (NNSG3dResTexInfo.sizeTex): 512 KB
			// at most in one package. Halve the largest picture until they fit.
			while (textures.Sum(t => (long)Tex0Write.TexelBytes(t.Format, t.Width, t.Height)) > 0x7FFF8)
			{
				Tex0Write.NewTexture largest = textures.OrderByDescending(t => t.Width * t.Height).First();
				if (largest.Width <= 8 && largest.Height <= 8) break;
				int nw = Math.Max(8, largest.Width / 2), nh = Math.Max(8, largest.Height / 2);
				largest.Rgba = Resample(largest.Rgba, largest.Width, largest.Height, nw, nh);
				result.Notes.Add(largest.Name + ": " + largest.Width + "x" + largest.Height + " halved to " + nw + "x" + nh + " (a package holds 512 KB of texels)");
				largest.Width = nw; largest.Height = nh;
			}

			// Geometry: the position scale that fits every vertex into VTX_16's +-8.
			float extent = 0;
			foreach (GltfMesh mesh in meshes)
				for (int i = 0; i + 2 < mesh.Positions.Length; i += 3)
				{
					float[] p = Place(fit, mesh.Positions[i], mesh.Positions[i + 1], mesh.Positions[i + 2]);
					extent = Math.Max(extent, Math.Max(Math.Abs(p[0]), Math.Max(Math.Abs(p[1]), Math.Abs(p[2]))));
				}
			int shift = 0;
			while ((7.99f * (1 << shift)) < extent && shift < 12) shift++;
			int posScale = 4096 << shift;
			float toFixed = 4096f / (1 << shift);   // world units (after `scale`) to VTX_16 under the position scale

			// One display list per material.
			List<byte[]> lists = new List<byte[]>();
			float[] min = { float.MaxValue, float.MaxValue, float.MaxValue }, max = { float.MinValue, float.MinValue, float.MinValue };
			foreach (int materialId in materialIds)
			{
				int t = materialIds.IndexOf(materialId);
				DisplayList dl = new DisplayList();
				dl.Command(0x40, 0);   // BEGIN_VTXS triangles
				int inRun = 0;
				foreach (GltfMesh mesh in meshes.Where(m => m.Material == materialId))
				{
					int texW = textures[t].Width, texH = textures[t].Height;
					for (int i = 0; i + 2 < mesh.Indices.Length; i += 3)
					{
						if (inRun + 3 > RunLimit)
						{
							dl.Command(0x41); dl.Command(0x40, 0);   // a new run before the game's buffer fills
							inRun = 0;
						}
						inRun += 3;
						// A flat normal for a mesh without its own.
						float[] flat = null;
						if (mesh.Normals == null)
						{
							int a = mesh.Indices[i], b = mesh.Indices[i + 1], c = mesh.Indices[i + 2];
							flat = Cross(Sub(mesh.Positions, b, a), Sub(mesh.Positions, c, a));
						}
						for (int k = 0; k < 3; k++)
						{
							int v = mesh.Indices[i + k];
							// The texture coordinate in texels (12.4); a mesh without any samples the middle of its (plain) texture.
							int s = mesh.Uvs != null ? (int)Math.Round(mesh.Uvs[v * 2] * texW * 16) : texW * 8;
							int tt = mesh.Uvs != null ? (int)Math.Round(mesh.Uvs[v * 2 + 1] * texH * 16) : texH * 8;
							dl.Command(0x22, (uint)((s & 0xFFFF) | ((tt & 0xFFFF) << 16)));
							float nx, ny, nz;
							if (flat != null) { nx = flat[0]; ny = flat[1]; nz = flat[2]; }
							else { nx = mesh.Normals[v * 3]; ny = mesh.Normals[v * 3 + 1]; nz = mesh.Normals[v * 3 + 2]; }
							float[] turned = Turn(fit, nx, ny, nz);
							dl.Command(0x21, Normal(turned[0], turned[1], turned[2]));
							float[] placed = Place(fit, mesh.Positions[v * 3], mesh.Positions[v * 3 + 1], mesh.Positions[v * 3 + 2]);
							float px = placed[0], py = placed[1], pz = placed[2];
							min[0] = Math.Min(min[0], px); min[1] = Math.Min(min[1], py); min[2] = Math.Min(min[2], pz);
							max[0] = Math.Max(max[0], px); max[1] = Math.Max(max[1], py); max[2] = Math.Max(max[2], pz);
							int fx = Fixed16(px * toFixed), fy = Fixed16(py * toFixed), fz = Fixed16(pz * toFixed);
							dl.Command(0x23, (uint)((fx & 0xFFFF) | ((fy & 0xFFFF) << 16)), (uint)(fz & 0xFFFF));
							result.Vertices++;
						}
						result.Triangles++;
					}
				}
				dl.Command(0x41);   // END_VTXS
				lists.Add(dl.Bytes());
			}
			result.Materials = materialIds.Count;
			if (result.Vertices == 0) throw new InvalidDataException("the file has no triangles");
			CheckSize(result.Vertices, result.Triangles, generous, result.Notes);

			// ---- the model
			// Node dictionary: one node, "root", the identity (flag 0xF807: no translation, no rotation, no scale, stack slot 31).
			byte[] nodeDict = Tex0Write.Dictionary(new List<string> { "root" }, 4, _ => BitConverter.GetBytes(0));
			byte[] nodeData = { 0x07, 0xF8, 0x00, 0x10 };
			// The node entry is the node data's offset from the node dictionary's start.
			Put32(nodeDict, 8 + 2 * 4 + 4, (uint)nodeDict.Length);

			// SBC: NODEDESC root; NODE root visible; MAT i, SHP i for each; RET; padded to 4.
			List<byte> sbc = new List<byte> { 0x06, 0x00, 0x00, 0x00, 0x02, 0x00, 0x01 };
			for (int i = 0; i < lists.Count; i++) { sbc.Add(0x04); sbc.Add((byte)i); sbc.Add(0x05); sbc.Add((byte)i); }
			sbc.Add(0x01);
			while (sbc.Count % 4 != 0) sbc.Add(0x00);

			int n = lists.Count;
			byte[] mat = MaterialBlock(textures, doubleSided, null);
			byte[] shp = ShapeBlock(lists);

			// The model: offsets, info, node dictionary + data, SBC, materials, shapes.
			int ofsNodeInfo = 20 + 44;
			int ofsSbc = ofsNodeInfo + nodeDict.Length + nodeData.Length;
			int ofsMat = ofsSbc + sbc.Count;
			int ofsShp = ofsMat + mat.Length;
			int modelSize = ofsShp + shp.Length;
			byte[] model = new byte[modelSize];
			Put32(model, 0, (uint)modelSize); Put32(model, 4, (uint)ofsSbc); Put32(model, 8, (uint)ofsMat); Put32(model, 12, (uint)ofsShp); Put32(model, 16, (uint)modelSize);
			int info = 20;
			model[info] = 0; model[info + 1] = 0; model[info + 2] = 0;            // sbcType, scalingRule (standard), texMtxMode
			model[info + 3] = 1; model[info + 4] = (byte)n; model[info + 5] = (byte)n; model[info + 6] = 0; model[info + 7] = 0;
			Put32(model, info + 8, (uint)posScale); Put32(model, info + 12, (uint)(4096 * 4096 / posScale));
			Put16(model, info + 16, (ushort)Math.Min(65535, result.Vertices)); Put16(model, info + 18, (ushort)Math.Min(65535, result.Triangles));
			Put16(model, info + 20, (ushort)Math.Min(65535, result.Triangles)); Put16(model, info + 22, 0);
			WriteBox(model, info, min, max);
			Array.Copy(nodeDict, 0, model, ofsNodeInfo, nodeDict.Length);
			Array.Copy(nodeData, 0, model, ofsNodeInfo + nodeDict.Length, nodeData.Length);
			sbc.CopyTo(model, ofsSbc);
			Array.Copy(mat, 0, model, ofsMat, mat.Length);
			Array.Copy(shp, 0, model, ofsShp, shp.Length);

			result.Nmdp = Package(model, name);
			result.Ntxp = Tex0Write.Build(textures);
			return result;
		}

		/// <summary>The info block's box (at info+24): its corner and its size in 1/4096ths under the box's own scale, a power of two that fits.</summary>
		internal static void WriteBox(byte[] model, int info, float[] min, float[] max)
		{
			float boxExtent = Math.Max(Math.Max(Math.Abs(min[0]), Math.Abs(min[1])), Math.Max(Math.Abs(min[2]), Math.Max(Math.Max(max[0] - min[0], max[1] - min[1]), max[2] - min[2])));
			int boxShift = 0;
			while ((7.99f * (1 << boxShift)) < boxExtent && boxShift < 12) boxShift++;
			float boxFixed = 4096f / (1 << boxShift);
			Put16(model, info + 24, (ushort)(short)Math.Round(min[0] * boxFixed)); Put16(model, info + 26, (ushort)(short)Math.Round(min[1] * boxFixed)); Put16(model, info + 28, (ushort)(short)Math.Round(min[2] * boxFixed));
			Put16(model, info + 30, (ushort)(short)Math.Round((max[0] - min[0]) * boxFixed)); Put16(model, info + 32, (ushort)(short)Math.Round((max[1] - min[1]) * boxFixed)); Put16(model, info + 34, (ushort)(short)Math.Round((max[2] - min[2]) * boxFixed));
			Put32(model, info + 36, (uint)(4096 << boxShift)); Put32(model, info + 40, (uint)(4096 >> boxShift));
		}

		/// <summary>
		/// The material block: [ofsDictTex u16][ofsDictPltt u16][mat dict][tex dict][pltt dict][lists][material data x44],
		/// one material per texture, each using its own texture and palette. The material words are
		/// w005's, or - given <paramref name="template"/>, a 44-byte record of the model being
		/// remade - that record's lighting and polygon attributes, so the piece is lit as the original was.
		/// </summary>
		internal static byte[] MaterialBlock(List<Tex0Write.NewTexture> textures, List<bool> doubleSided, byte[] template)
		{
			int n = textures.Count;
			byte[] matDict = Tex0Write.Dictionary(Enumerable.Range(0, n).Select(i => "m" + i).ToList(), 4, _ => new byte[4]);
			byte[] texDict = Tex0Write.Dictionary(textures.Select(tx => tx.Name).ToList(), 4, _ => new byte[4]);
			byte[] plttDict = Tex0Write.Dictionary(textures.Select(tx => tx.Name + "_pl").ToList(), 4, _ => new byte[4]);
			int ofsTexDict = 4 + matDict.Length;
			int ofsPlttDict = ofsTexDict + texDict.Length;
			int ofsLists = ofsPlttDict + plttDict.Length;
			// Each texture and each palette is used by exactly its own material: a one-byte list each.
			int listBytes = Align(n * 2, 4);
			int ofsMatData = ofsLists + listBytes;
			byte[] mat = new byte[ofsMatData + n * 44];
			Put16(mat, 0, (ushort)ofsTexDict); Put16(mat, 2, (ushort)ofsPlttDict);
			for (int i = 0; i < n; i++)
			{
				Put32(matDict, 8 + (n + 1) * 4 + 4 + i * 4, (uint)(ofsMatData + i * 44));
				mat[ofsLists + i] = (byte)i;
				mat[ofsLists + n + i] = (byte)i;
				Put32(texDict, 8 + (n + 1) * 4 + 4 + i * 4, (uint)((ofsLists + i) | (1 << 16)));
				Put32(plttDict, 8 + (n + 1) * 4 + 4 + i * 4, (uint)((ofsLists + n + i) | (1 << 16)));
			}
			Array.Copy(matDict, 0, mat, 4, matDict.Length);
			Array.Copy(texDict, 0, mat, ofsTexDict, texDict.Length);
			Array.Copy(plttDict, 0, mat, ofsPlttDict, plttDict.Length);
			for (int i = 0; i < n; i++)
			{
				int p = ofsMatData + i * 44;
				Put16(mat, p, 0); Put16(mat, p + 2, 44);
				uint diffAmb = 0x7FFFE739, speEmi = 0x7FFF0000, polyAttr = 0x3F1F208F, polyAttrMask = 0x3F1FF8FF;
				if (template != null && template.Length >= 44)
				{
					diffAmb = BitConverter.ToUInt32(template, 4); speEmi = BitConverter.ToUInt32(template, 8);
					polyAttr = BitConverter.ToUInt32(template, 12) & ~0xC0u; polyAttrMask = BitConverter.ToUInt32(template, 16);
				}
				Put32(mat, p + 4, diffAmb);             // diffuse (as vertex colour) and ambient, w005's
				Put32(mat, p + 8, speEmi);              // specular and emission, w005's
				// polyAttr: lights 0-3, modulation, front faces, alpha 31, polygon id 63 (w005's); both faces for a double-sided material.
				if (doubleSided[i]) polyAttr |= 0xC0; else polyAttr |= 0x80;
				Put32(mat, p + 12, polyAttr);
				Put32(mat, p + 16, polyAttrMask);
				Put32(mat, p + 20, 0x00030000);         // texImageParam: repeat S and T (the texture itself is bound by name)
				Put32(mat, p + 24, 0xFFFFFFFF);
				Put16(mat, p + 28, 0);                  // texPlttBase
				Put16(mat, p + 30, 0x1FCE);             // flag: no texture matrix
				Put16(mat, p + 32, (ushort)textures[i].Width); Put16(mat, p + 34, (ushort)textures[i].Height);
				Put32(mat, p + 36, 4096); Put32(mat, p + 40, 4096);
			}
			return mat;
		}

		/// <summary>The shape block: [dict][per shape: 16-byte header then its display list], one shape per list, with normals and texture coordinates.</summary>
		internal static byte[] ShapeBlock(List<byte[]> lists)
		{
			int n = lists.Count;
			byte[] shpDict = Tex0Write.Dictionary(Enumerable.Range(0, n).Select(i => "polygon" + i).ToList(), 4, _ => new byte[4]);
			int shpSize = shpDict.Length + lists.Sum(l => 16 + l.Length);
			byte[] shp = new byte[shpSize];
			Array.Copy(shpDict, 0, shp, 0, shpDict.Length);
			int at = shpDict.Length;
			for (int i = 0; i < n; i++)
			{
				Put32(shp, 8 + (n + 1) * 4 + 4 + i * 4, (uint)at);
				Put16(shp, at, 0); Put16(shp, at + 2, 16);
				Put32(shp, at + 4, 5);                  // flag: normals and texture coordinates
				Put32(shp, at + 8, 16);                 // the list follows the header
				Put32(shp, at + 12, (uint)lists[i].Length);
				Array.Copy(lists[i], 0, shp, at + 16, lists[i].Length);
				at += 16 + lists[i].Length;
			}
			return shp;
		}

		/// <summary>A model's bytes wrapped as the game ships them: MDL0 (head, a one-model dictionary, the model), BMD0 with that one block, the NMDP head (kind 2, a model).</summary>
		internal static byte[] Package(byte[] model, string name)
		{
			byte[] mdlDict = Tex0Write.Dictionary(new List<string> { name }, 4, _ => new byte[4]);
			int ofsModel = 8 + mdlDict.Length;
			Put32(mdlDict, 8 + 2 * 4 + 4, (uint)ofsModel);
			byte[] mdl0 = new byte[ofsModel + model.Length];
			Ascii(mdl0, 0, "MDL0"); Put32(mdl0, 4, (uint)mdl0.Length);
			Array.Copy(mdlDict, 0, mdl0, 8, mdlDict.Length);
			Array.Copy(model, 0, mdl0, ofsModel, model.Length);

			// BMD0: head 16 + one block offset.
			byte[] bmd0 = new byte[20 + mdl0.Length];
			Ascii(bmd0, 0, "BMD0"); Put16(bmd0, 4, 0xFEFF); Put16(bmd0, 6, 2); Put32(bmd0, 8, (uint)bmd0.Length); Put16(bmd0, 12, 16); Put16(bmd0, 14, 1); Put32(bmd0, 16, 20);
			Array.Copy(mdl0, 0, bmd0, 20, mdl0.Length);

			// NMDP wrapper, as w005.nmdp has it (kind 2: a model).
			byte[] nmdp = new byte[48 + bmd0.Length];
			Ascii(nmdp, 0, "NMDP"); Put32(nmdp, 4, 0x1000); Put32(nmdp, 16, 1); Put32(nmdp, 20, 2); Put32(nmdp, 24, (uint)bmd0.Length); Put32(nmdp, 28, 48);
			Array.Copy(bmd0, 0, nmdp, 48, bmd0.Length);
			return nmdp;
		}

		/// <summary>A material's texture: its picture times its base colour, or a plain 8x8 of the colour; a power-of-two side of 8..1024; pal256.</summary>
		internal static Tex0Write.NewTexture TextureOf(GltfFile file, GltfMaterial material, string texName, List<string> notes)
		{
			byte[] rgba = null; int w = 0, h = 0;
			if (material.Image >= 0 && material.Image < file.Images.Count && file.Images[material.Image].Bytes != null)
			{
				byte[] bytes = file.Images[material.Image].Bytes;
				try
				{
					// A PNG through our own reader; anything else (a JPEG, as texturing tools like to write) through the platform's.
					if (bytes.Length > 8 && bytes[0] == 0x89 && bytes[1] == (byte)'P') rgba = PngRead.Decode(bytes, out w, out h);
					else if (OperatingSystem.IsWindows()) rgba = DecodeWithPlatform(bytes, out w, out h);
					else throw new InvalidDataException("not a PNG, and only Windows decodes the rest");
				}
				catch (Exception ex) { notes.Add((material.Name ?? texName) + ": its picture is not read (" + ex.Message + ") - the base colour stands in"); rgba = null; }
			}
			if (rgba == null) { w = h = 8; rgba = new byte[8 * 8 * 4]; for (int i = 0; i < 64; i++) { rgba[i * 4] = rgba[i * 4 + 1] = rgba[i * 4 + 2] = rgba[i * 4 + 3] = 255; } }
			// Times the base colour.
			float[] c = material.BaseColour ?? new[] { 1f, 1f, 1f, 1f };
			if (c[0] != 1f || c[1] != 1f || c[2] != 1f || c[3] != 1f)
				for (int i = 0; i < w * h; i++)
				{
					rgba[i * 4] = (byte)Math.Round(rgba[i * 4] * c[0]); rgba[i * 4 + 1] = (byte)Math.Round(rgba[i * 4 + 1] * c[1]);
					rgba[i * 4 + 2] = (byte)Math.Round(rgba[i * 4 + 2] * c[2]); rgba[i * 4 + 3] = (byte)Math.Round(rgba[i * 4 + 3] * c[3]);
				}
			int pw = Pow2(w), ph = Pow2(h);
			if (pw != w || ph != h)
			{
				rgba = Resample(rgba, w, h, pw, ph);
				if (w >= 8 && h >= 8) notes.Add((material.Name ?? texName) + ": " + w + "x" + h + " resampled to " + pw + "x" + ph + " (a texture's sides are powers of two, 8..1024)");
				w = pw; h = ph;
			}
			bool clear = false;
			for (int i = 0; i < w * h && !clear; i++) clear = rgba[i * 4 + 3] < 128;
			return new Tex0Write.NewTexture { Name = texName, Format = 4, Transparent0 = clear, Rgba = rgba, Width = w, Height = h };
		}

		/// <summary>A picture in any format Windows decodes (JPEG, BMP, GIF...) as RGBA, through System.Drawing.</summary>
		[System.Runtime.Versioning.SupportedOSPlatform("windows")]
		private static byte[] DecodeWithPlatform(byte[] bytes, out int width, out int height)
		{
			using System.IO.MemoryStream stream = new System.IO.MemoryStream(bytes);
			using System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(stream);
			width = bitmap.Width; height = bitmap.Height;
			byte[] rgba = new byte[width * height * 4];
			System.Drawing.Imaging.BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			try
			{
				byte[] row = new byte[width * 4];
				for (int y = 0; y < height; y++)
				{
					System.Runtime.InteropServices.Marshal.Copy(data.Scan0 + y * data.Stride, row, 0, row.Length);
					for (int x = 0; x < width; x++)
					{
						int o = (y * width + x) * 4;
						rgba[o] = row[x * 4 + 2]; rgba[o + 1] = row[x * 4 + 1]; rgba[o + 2] = row[x * 4]; rgba[o + 3] = row[x * 4 + 3];
					}
				}
			}
			finally { bitmap.UnlockBits(data); }
			return rgba;
		}

		private static int Pow2(int v)
		{
			int p = 8;
			while (p < v && p < 1024) p <<= 1;
			return p;
		}

		/// <summary>A box resample to the new size (nearest when growing, averaged when shrinking).</summary>
		internal static byte[] Resample(byte[] src, int w, int h, int nw, int nh)
		{
			byte[] dst = new byte[nw * nh * 4];
			for (int y = 0; y < nh; y++)
			{
				int y0 = y * h / nh, y1 = Math.Max(y0 + 1, (y + 1) * h / nh);
				for (int x = 0; x < nw; x++)
				{
					int x0 = x * w / nw, x1 = Math.Max(x0 + 1, (x + 1) * w / nw);
					long r = 0, g = 0, b = 0, a = 0, count = 0;
					for (int yy = y0; yy < y1 && yy < h; yy++)
						for (int xx = x0; xx < x1 && xx < w; xx++)
						{
							int i = (yy * w + xx) * 4;
							r += src[i]; g += src[i + 1]; b += src[i + 2]; a += src[i + 3]; count++;
						}
					if (count == 0) count = 1;
					int o = (y * nw + x) * 4;
					dst[o] = (byte)(r / count); dst[o + 1] = (byte)(g / count); dst[o + 2] = (byte)(b / count); dst[o + 3] = (byte)(a / count);
				}
			}
			return dst;
		}

		private static float[] Sub(float[] p, int a, int b) => new[] { p[a * 3] - p[b * 3], p[a * 3 + 1] - p[b * 3 + 1], p[a * 3 + 2] - p[b * 3 + 2] };
		private static float[] Cross(float[] a, float[] b) => new[] { a[1] * b[2] - a[2] * b[1], a[2] * b[0] - a[0] * b[2], a[0] * b[1] - a[1] * b[0] };

		/// <summary>
		/// Scale, then turns about x, y and z (degrees, in that order), then an offset, as a row-major
		/// 3x3 with the translation in [9..11] - the order WeaponMeshes.Fit uses on the OpenFF target,
		/// so the same definition places the model the same way on both.
		/// </summary>
		private static float[] FitMatrix(float scale, float[] rotation, float[] offset)
		{
			float[] m = { scale, 0, 0, 0, scale, 0, 0, 0, scale, 0, 0, 0 };
			float[] r = rotation != null && rotation.Length >= 3 ? rotation : new float[3];
			for (int axis = 0; axis < 3; axis++)
			{
				if (Math.Abs(r[axis]) < 0.0001f) continue;
				float a = r[axis] * MathF.PI / 180, c = MathF.Cos(a), s = MathF.Sin(a);
				float[] rot = axis == 0 ? new[] { 1, 0, 0, 0, c, -s, 0, s, c } : axis == 1 ? new[] { c, 0, s, 0, 1, 0, -s, 0, c } : new[] { c, -s, 0, s, c, 0, 0, 0, 1 };
				float[] next = new float[12];
				for (int row = 0; row < 3; row++)
					for (int col = 0; col < 3; col++)
						next[row * 3 + col] = rot[row * 3] * m[col] + rot[row * 3 + 1] * m[3 + col] + rot[row * 3 + 2] * m[6 + col];
				m = next;
			}
			if (offset != null && offset.Length >= 3) { m[9] = offset[0]; m[10] = offset[1]; m[11] = offset[2]; }
			return m;
		}

		private static float[] Place(float[] m, float x, float y, float z) => new[] { m[0] * x + m[1] * y + m[2] * z + m[9], m[3] * x + m[4] * y + m[5] * z + m[10], m[6] * x + m[7] * y + m[8] * z + m[11] };
		private static float[] Turn(float[] m, float x, float y, float z) => new[] { m[0] * x + m[1] * y + m[2] * z, m[3] * x + m[4] * y + m[5] * z, m[6] * x + m[7] * y + m[8] * z };

		/// <summary>A NORMAL parameter: three 10-bit 1.9 fixed components.</summary>
		internal static uint Normal(float x, float y, float z)
		{
			float len = MathF.Sqrt(x * x + y * y + z * z);
			if (len < 1e-6f) { x = 0; y = 1; z = 0; } else { x /= len; y /= len; z /= len; }
			int nx = Math.Clamp((int)Math.Round(x * 511), -512, 511), ny = Math.Clamp((int)Math.Round(y * 511), -512, 511), nz = Math.Clamp((int)Math.Round(z * 511), -512, 511);
			return (uint)((nx & 0x3FF) | ((ny & 0x3FF) << 10) | ((nz & 0x3FF) << 20));
		}

		internal static int Fixed16(float v) => Math.Clamp((int)Math.Round(v), -32768, 32767);

		/// <summary>A DS display list: commands packed four to a word, each word's parameters after it, NOPs to fill the last word; a zero word after a trailing parameterless word, as the shipped lists end.</summary>
		internal sealed class DisplayList
		{
			private readonly List<(byte cmd, uint[] args)> _pending = new List<(byte, uint[])>();
			private readonly List<byte> _bytes = new List<byte>();

			public void Command(byte cmd, params uint[] args)
			{
				_pending.Add((cmd, args));
				if (_pending.Count == 4) Flush();
			}

			private void Flush()
			{
				uint word = 0;
				for (int i = 0; i < _pending.Count; i++) word |= (uint)_pending[i].cmd << (i * 8);
				Add(word);
				foreach ((byte _, uint[] args) in _pending) foreach (uint a in args) Add(a);
				_pending.Clear();
			}

			private void Add(uint v) { _bytes.Add((byte)v); _bytes.Add((byte)(v >> 8)); _bytes.Add((byte)(v >> 16)); _bytes.Add((byte)(v >> 24)); }

			public byte[] Bytes()
			{
				bool trailingNoArgs = _pending.Count > 0 && _pending.All(p => p.args.Length == 0);
				if (_pending.Count > 0) Flush();
				if (trailingNoArgs) Add(0);
				return _bytes.ToArray();
			}
		}

		internal static int Align(int v, int to) => (v + to - 1) / to * to;
		internal static void Ascii(byte[] d, int at, string s) { for (int i = 0; i < s.Length; i++) d[at + i] = (byte)s[i]; }
		internal static void Put16(byte[] d, int at, ushort v) { d[at] = (byte)v; d[at + 1] = (byte)(v >> 8); }
		internal static void Put32(byte[] d, int at, uint v) { d[at] = (byte)v; d[at + 1] = (byte)(v >> 8); d[at + 2] = (byte)(v >> 16); d[at + 3] = (byte)(v >> 24); }
	}
}
