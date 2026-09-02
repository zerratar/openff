// MDL0 - the geometry inside an NMDP package.
//
//   ff3content mdl <file.lz | directory> [out]
//
// A model is three things stacked on each other. The shapes hold NDS display lists,
// which are GPU command streams rather than vertex buffers. The SBC is a little byte
// code that walks the node tree, builds a matrix for each node and says "draw shape 3
// with material 1, here". The materials name a texture, which lives in the TEX0 next
// door - or, for 463 of the 833 models, in the .ntxp of the same name.
//
// All of it is ported from the game rather than guessed: the structures from
// NNSG3dResMdlSet down, the display list walk from NNSG3dResShpData.preBuild, and the
// node transforms and command lengths from the SBC interpreter in GlobalScope.Members.
//
// The animation path is deliberately left out. With no animation bound the game's own
// blend weight stays at 1.0 and the result is exactly the base pose, so a static export
// takes that branch and nothing else - the bind pose, which is what you want to look at.
//
// Everything is fixed point: positions and matrices in 1/4096ths, texture coordinates
// in 1/16ths of a texel, colours in 5 bits per channel.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FF3.ContentTool
{
	internal struct Mdl0Vertex
	{
		public float X, Y, Z;
		public float U, V;                       // in texels, not 0..1
		public byte R, G, B;
	}

	internal enum Mdl0Primitive
	{
		Triangles = 0,
		Quads = 1,
		TriangleStrip = 2,
		QuadStrip = 3
	}

	internal sealed class Mdl0Run
	{
		public Mdl0Primitive Kind;
		public List<Mdl0Vertex> Vertices = new List<Mdl0Vertex>();
	}

	/// <summary>One draw call: a shape, a material, and where the SBC put it.</summary>
	internal sealed class Mdl0Piece
	{
		public string Shape;
		public string Material;
		public string Node;

		/// <summary>
		/// Switched off by its node, so the game never draws it. Two shapes in the whole
		/// game are - one in b32, one in t10_02 - and both are still counted in their
		/// model's own vertex total, which is how they were found. They are kept rather
		/// than dropped: it is real geometry somebody authored, and throwing it away
		/// would make the decoder disagree with the file for no good reason.
		/// </summary>
		public bool Hidden;

		/// <summary>
		/// 0 for an ordinary node, 1 for a billboard, 2 for one that only turns about
		/// its vertical axis. The game post-multiplies such a node's rotation by the
		/// inverse camera, so the piece ends up facing the viewer; with no camera to
		/// hand, what is baked here is that same step with the camera at identity, and
		/// the pivot below is what a viewer needs to finish the job.
		/// </summary>
		public int Billboard;

		public float PivotX, PivotY, PivotZ;

		public List<Mdl0Run> Runs = new List<Mdl0Run>();
	}

	internal sealed class Mdl0Material
	{
		public string Name;
		public string Texture;
		public string Palette;
		public byte R, G, B;
		public int Alpha;                        // 0-31, 31 being opaque
		public int Width;
		public int Height;
		public bool RepeatS, RepeatT, FlipS, FlipT;
	}

	internal sealed class Mdl0Model
	{
		public string Name;
		public int Vertices;                     // what the model says about itself
		public int Polygons;
		public int Triangles;
		public int Quads;
		public float BoxX, BoxY, BoxZ, BoxW, BoxH, BoxD;

		public List<string> Nodes = new List<string>();
		public List<Mdl0Material> Materials = new List<Mdl0Material>();
		public List<Mdl0Piece> Pieces = new List<Mdl0Piece>();

		/// <summary>What was actually decoded, to set against the four above.</summary>
		public int GotVertices, GotTriangles, GotQuads;
	}

	internal static class Mdl0
	{
		/// <summary>Where the MDL0 block is, or -1 if this package has none.</summary>
		public static int Find(byte[] data)
		{
			if (data == null || data.Length < 48 || data[0] != 'N' || data[1] != 'M'
				|| data[2] != 'D' || data[3] != 'P')
			{
				return -1;
			}

			int start = (int)U32(data, 28);
			if (start < 0 || start + 16 > data.Length)
			{
				return -1;
			}
			if (Encoding.ASCII.GetString(data, start, 4) != "BMD0")
			{
				return -1;
			}

			int blocks = U16(data, start + 14);
			for (int i = 0; i < blocks; i++)
			{
				int at = start + (int)U32(data, start + 16 + i * 4);
				if (at >= 0 && at + 4 <= data.Length
					&& Encoding.ASCII.GetString(data, at, 4) == "MDL0")
				{
					return at;
				}
			}
			return -1;
		}

		public static List<Mdl0Model> Read(byte[] data)
		{
			int at = Find(data);
			if (at < 0)
			{
				throw new InvalidDataException("no MDL0 block in this package");
			}

			List<Mdl0Model> models = new List<Mdl0Model>();
			foreach ((string name, byte[] entry) in Dict(data, at + 8))
			{
				models.Add(ReadModel(data, at + (int)U32(entry, 0), name));
			}
			return models;
		}

		private static Mdl0Model ReadModel(byte[] data, int m, string name)
		{
			Mdl0Model model = new Mdl0Model { Name = name.TrimStart('\\') };

			int ofsSbc = (int)U32(data, m + 4);
			int ofsMat = (int)U32(data, m + 8);
			int ofsShp = (int)U32(data, m + 12);
			int ofsEvp = (int)U32(data, m + 16);

			int info = m + 20;
			model.Vertices = U16(data, info + 16);
			model.Polygons = U16(data, info + 18);
			model.Triangles = U16(data, info + 20);
			model.Quads = U16(data, info + 22);

			// The box has a scale of its own, separate from the model's. Both halves
			// are fixed point: the corner is in 1/4096ths, and so is the scale it is
			// then multiplied by.
			float boxScale = S32(data, info + 36) / 4096f / 4096f;
			model.BoxX = S16(data, info + 24) * boxScale;
			model.BoxY = S16(data, info + 26) * boxScale;
			model.BoxZ = S16(data, info + 28) * boxScale;
			model.BoxW = S16(data, info + 30) * boxScale;
			model.BoxH = S16(data, info + 32) * boxScale;
			model.BoxD = S16(data, info + 34) * boxScale;

			// posScale is always a power of two times 4096; the shift is how many.
			int posScale = S32(data, info + 8);
			int scale = 0;
			while ((4096 << scale) < posScale)
			{
				scale++;
			}

			// Node names, in dictionary order - the SBC refers to them by index.
			// Five offsets then a 44 byte info block is where the node dictionary starts.
			int nodeInfo = m + 20 + 44;
			List<(string, byte[])> nodes = Dict(data, nodeInfo);
			foreach ((string nodeName, byte[] _) in nodes)
			{
				model.Nodes.Add(nodeName);
			}

			List<(string, byte[])> shapes = ofsShp != 0
				? Dict(data, m + ofsShp) : new List<(string, byte[])>();
			if (ofsMat != 0)
			{
				ReadMaterials(data, m + ofsMat, model);
			}

			// The inverse-bind matrices, for the models that blend between nodes. Each
			// entry is a MtxFx43 followed by a MtxFx33, which is where the 84 comes from.
			int[][] evp = null;
			if (ofsEvp != 0)
			{
				evp = new int[nodes.Count][];
				for (int i = 0; i < nodes.Count; i++)
				{
					int e = m + ofsEvp + i * 84;
					evp[i] = e + 48 <= data.Length ? ReadMatrix(data, e) : Identity();
				}
			}

			if (ofsSbc != 0 && ofsShp != 0)
			{
				RunSbc(data, m, ofsSbc, ofsMat, ofsShp, nodeInfo, nodes, shapes, evp,
					scale, model);
			}

			return model;
		}

		private static void ReadMaterials(byte[] data, int at, Mdl0Model model)
		{
			List<(string Name, byte[] Entry)> materials = Dict(data, at + 4);

			// The other two dictionaries run the other way: each texture name lists the
			// materials that use it. Walking them backwards is what gives a material
			// its texture.
			string[] texture = new string[materials.Count];
			string[] palette = new string[materials.Count];
			Reverse(data, at, U16(data, at), texture, materials.Count);
			Reverse(data, at, U16(data, at + 2), palette, materials.Count);

			for (int i = 0; i < materials.Count; i++)
			{
				int p = at + (int)U32(materials[i].Entry, 0);
				uint diffAmb = U32(data, p + 4);
				uint polyAttr = U32(data, p + 12);
				uint texImageParam = U32(data, p + 20);

				model.Materials.Add(new Mdl0Material
				{
					Name = materials[i].Name,
					Texture = texture[i],
					Palette = palette[i],
					R = (byte)((diffAmb & 0x1F) << 3),
					G = (byte)(((diffAmb >> 5) & 0x1F) << 3),
					B = (byte)(((diffAmb >> 10) & 0x1F) << 3),
					Alpha = (int)((polyAttr >> 16) & 0x1F),
					Width = U16(data, p + 32),
					Height = U16(data, p + 34),
					RepeatS = (texImageParam & 0x10000) != 0,
					RepeatT = (texImageParam & 0x20000) != 0,
					FlipS = (texImageParam & 0x40000) != 0,
					FlipT = (texImageParam & 0x80000) != 0
				});
			}
		}

		/// <summary>
		/// A texture-to-material dictionary. Its entries are an offset and a length,
		/// and what lives there is a list of material indices.
		/// </summary>
		private static void Reverse(byte[] data, int at, int ofsDict, string[] into, int count)
		{
			if (ofsDict == 0)
			{
				return;
			}

			List<(string Name, byte[] Entry)> dict = Dict(data, at + ofsDict);
			foreach ((string name, byte[] entry) in dict)
			{
				uint packed = U32(entry, 0);
				int list = at + (int)(packed & 0xFFFF);
				int length = (int)((packed >> 16) & 0xFFFF);
				for (int i = 0; i < length && list + i < data.Length; i++)
				{
					int material = data[list + i];
					if (material < count)
					{
						into[material] = name;
					}
				}
			}
		}

		// ------------------------------------------------------------------ the SBC

		/// <summary>
		/// The node walker. Ports the interpreter's opcodes 0-9 and 11 with the
		/// animation branch taken out, which - with nothing bound - is what the game
		/// itself does. Billboards keep their base matrix, since which way they face
		/// depends on a camera that is not here.
		/// </summary>
		private static void RunSbc(byte[] data, int m, int ofsSbc, int ofsMat, int ofsShp,
			int nodeInfo, List<(string Name, byte[] Entry)> nodes,
			List<(string Name, byte[] Entry)> shapes, int[][] evp, int scale, Mdl0Model model)
		{
			int at = m + ofsSbc;
			int end = m + (ofsMat != 0 ? ofsMat : ofsSbc);

			int[][] stack = new int[64][];
			int[][] stackN = new int[64][];
			for (int i = 0; i < 64; i++)
			{
				stack[i] = Identity();
				stackN[i] = Identity();
			}

			int[] current = Identity();
			int[] currentN = Identity();
			int material = -1;
			int node = 0;
			bool visible = true;

			// Whether the matrix in hand came from a billboard, carried alongside the
			// matrices themselves so a restore brings the flag back with the matrix.
			int billboard = 0;
			int[] stackBillboard = new int[64];

			while (at < end && at < data.Length)
			{
				int op = data[at] & 0x1F;
				int flags = data[at] & 0xE0;

				switch (op)
				{
					case 1:                      // return
						return;

					case 0:
					case 11:
						at += 1;
						break;

					case 2:                      // node visibility
						visible = (data[at + 2] & 1) != 0;
						at += 3;
						break;

					case 3:                      // restore a matrix from the stack
						current = Copy(stack[data[at + 1]]);
						currentN = Copy(stackN[data[at + 1]]);
						billboard = stackBillboard[data[at + 1]];
						at += 2;
						break;

					case 4:
						material = data[at + 1];
						at += 2;
						break;

					case 5:
					{
						int shape = data[at + 1];
						at += 2;
						if (shape < shapes.Count)
						{
							AddPiece(data, m + ofsShp, shapes, shape, material, node,
								current, stack, scale, model, !visible, billboard);
						}
						break;
					}

					case 6:                      // node descriptor: build its matrix
					{
						node = data[at + 1];
						billboard = 0;         // an ordinary node clears it again
						int store = data[at + 3];
						int[] scaleBy = { 4096, 4096, 4096 };
						int[] baseMatrix = NodeMatrix(data, nodeInfo, nodes, node, scaleBy);

						at += 4;
						int id = 0;
						if ((flags & 0x20) != 0)
						{
							id = data[at++];
						}
						if ((flags & 0x40) != 0)
						{
							if (data[at] < 64)
							{
								current = Copy(stack[data[at]]);
								currentN = Copy(stackN[data[at]]);
							}
							at++;
						}

						currentN = Concat(baseMatrix, currentN);
						current = (store & 1) != 0 ? Copy(currentN) : Concat(baseMatrix, current);
						ScaleApply(current, scaleBy[0], scaleBy[1], scaleBy[2]);

						if ((flags & 0x20) != 0)
						{
							stack[id] = Copy(current);
							stackN[id] = Copy(currentN);
							stackBillboard[id] = 0;
						}
						break;
					}

					case 7:                      // billboard, and its y-only cousin
					case 8:
					{
						at += 2;
						int id = 0;
						if ((flags & 0x20) != 0)
						{
							id = data[at++];
						}
						if ((flags & 0x40) != 0)
						{
							current = Copy(stack[data[at++]]);
						}
						billboard = op == 7 ? 1 : 2;
						if ((flags & 0x20) != 0)
						{
							stack[id] = Copy(current);
							stackN[id] = Copy(current);
							stackBillboard[id] = billboard;
						}
						break;
					}

					case 9:                      // blend several nodes into one matrix
					{
						int into = data[at + 1];
						int count = data[at + 2];
						at += 3;

						int[] sum = new int[12];
						for (int i = 0; i < count; i++)
						{
							int from = data[at];
							int bind = data[at + 1];
							int weight = data[at + 2];
							at += 3;

							int[] bindMatrix = evp != null && bind < evp.Length
								? evp[bind] : Identity();
							int[] one = Concat(bindMatrix, stack[from]);
							for (int k = 0; k < 12; k++)
							{
								sum[k] += one[k] * weight;
							}
						}

						for (int k = 0; k < 12; k++)
						{
							sum[k] >>= 8;
						}
						stack[into] = sum;
						stackN[into] = Copy(sum);
						current = Copy(sum);
						currentN = Copy(sum);
						break;
					}

					default:
						// Every model in the game uses only the opcodes above; anything
						// else means the walk has lost its place, so stop rather than
						// invent geometry.
						return;
				}
			}
		}

		/// <summary>
		/// One node's base transform, from the packed form in nodeInfo. The low bits of
		/// the first word say which parts were left out because they are the identity.
		/// </summary>
		private static int[] NodeMatrix(byte[] data, int nodeInfo,
			List<(string Name, byte[] Entry)> nodes, int node, int[] scaleBy)
		{
			int[] m = Identity();
			if (node >= nodes.Count)
			{
				return m;
			}

			int p = nodeInfo + (int)U32(nodes[node].Entry, 0);
			int flags = U16(data, p);
			m[0] = S16(data, p + 2);
			p += 4;

			if ((flags & 1) == 0)                // translation
			{
				m[9] = S32(data, p);
				m[10] = S32(data, p + 4);
				m[11] = S32(data, p + 8);
				p += 12;
			}

			if ((flags & 2) == 0)                // rotation
			{
				if ((flags & 8) == 0)            // the whole 3x3, minus the _00 above
				{
					m[1] = S16(data, p);
					m[2] = S16(data, p + 2);
					m[3] = S16(data, p + 4);
					m[4] = S16(data, p + 6);
					m[5] = S16(data, p + 8);
					m[6] = S16(data, p + 10);
					m[7] = S16(data, p + 12);
					m[8] = S16(data, p + 14);
					p += 16;
				}
				else                             // a pivot: one axis, two values
				{
					int a = S16(data, p);
					int b = S16(data, p + 2);
					int c = (flags & 0x200) != 0 ? -b : b;
					int d = (flags & 0x400) != 0 ? -a : a;
					int one = (flags & 0x100) != 0 ? -4096 : 4096;
					m[0] = m[4] = m[8] = 0;

					switch ((flags >> 4) & 0xF)
					{
						case 0: m[0] = one; m[4] = a; m[5] = b; m[7] = c; m[8] = d; break;
						case 1: m[1] = one; m[3] = a; m[5] = b; m[6] = c; m[8] = d; break;
						case 2: m[2] = one; m[3] = a; m[4] = b; m[6] = c; m[7] = d; break;
						case 3: m[3] = one; m[1] = a; m[2] = b; m[7] = c; m[8] = d; break;
						case 4: m[4] = one; m[0] = a; m[2] = b; m[6] = c; m[8] = d; break;
						case 5: m[5] = one; m[0] = a; m[1] = b; m[6] = c; m[7] = d; break;
						case 6: m[6] = one; m[1] = a; m[2] = b; m[4] = c; m[5] = d; break;
						case 7: m[7] = one; m[0] = a; m[2] = b; m[3] = c; m[5] = d; break;
						case 8: m[8] = one; m[0] = a; m[1] = b; m[3] = c; m[4] = d; break;
					}
					p += 4;
				}
			}

			if ((flags & 4) == 0)                // scale, then its inverse, unused here
			{
				scaleBy[0] = S32(data, p);
				scaleBy[1] = S32(data, p + 4);
				scaleBy[2] = S32(data, p + 8);
			}

			return m;
		}

		// --------------------------------------------------------- the display list

		private static void AddPiece(byte[] data, int shp,
			List<(string Name, byte[] Entry)> shapes, int shape, int material, int node,
			int[] matrix, int[][] stack, int scale, Mdl0Model model, bool hidden,
			int billboard)
		{
			int s = shp + (int)U32(shapes[shape].Entry, 0);
			int list = s + (int)U32(data, s + 8);
			int size = (int)U32(data, s + 12);

			Mdl0Material used = material >= 0 && material < model.Materials.Count
				? model.Materials[material] : null;

			Mdl0Piece piece = new Mdl0Piece
			{
				Shape = shapes[shape].Name,
				Material = used?.Name,
				Node = node < model.Nodes.Count ? model.Nodes[node] : null,
				Hidden = hidden,
				Billboard = billboard,
				PivotX = matrix[9] / 4096f,
				PivotY = matrix[10] / 4096f,
				PivotZ = matrix[11] / 4096f
			};

			Walk(data, list, size, matrix, stack, scale, piece, model, used);
			model.Pieces.Add(piece);
		}

		/// <summary>
		/// NNSG3dResShpData.preBuild, plus the one thing it throws away: which
		/// primitive each run of vertices belongs to. The game does not need that -
		/// it hands the whole list to the GPU - but geometry on its own does.
		///
		/// Commands come four to a word, and their parameters follow the word they
		/// were packed into, which is why there are two pointers here.
		/// </summary>
		private static void Walk(byte[] data, int list, int size, int[] matrix, int[][] stack,
			int scale, Mdl0Piece piece, Mdl0Model model, Mdl0Material material)
		{
			int words = size / 4;
			if (words == 0 || list + size > data.Length)
			{
				return;
			}

			int n = 0;
			uint word = U32(data, list);
			int p = n + 1;
			int lane = 0;

			int x = 0, y = 0, z = 0;              // the running vertex, in 1/4096ths
			float u = 0, v = 0;

			// Colour starts at the material's own, which is what the game puts in the
			// colour register when it binds one.
			byte litR = material?.R ?? 255;
			byte litG = material?.G ?? 255;
			byte litB = material?.B ?? 255;
			byte r = litR, g = litG, b = litB;
			Mdl0Run run = null;

			// Which matrix the vertices go through. It starts as the one the SBC built
			// for this node, and a restore inside the list switches it - that is how a
			// character's arm ends up on its arm bone rather than at the origin.
			int[] active = matrix;

			while (n < words)
			{
				switch (word & 0xFF)
				{
					case 20:                      // restore a matrix from the stack
					{
						int id = (int)(U32(data, list + p * 4) & 63);
						active = stack[id];
						p++;
						break;
					}

					case 27:                      // scale
						p += 3;
						break;

					case 32:                      // colour
					{
						uint colour = U32(data, list + p * 4);
						r = (byte)(colour << 3);
						g = (byte)(colour >> 5 << 3);
						b = (byte)(colour >> 10 << 3);
						p++;
						break;
					}

					case 33:
						// A normal. On the hardware this recomputes the vertex colour
						// from the lighting equation, throwing away whatever the last
						// colour command set - and in these files a colour command is
						// always immediately followed by one, so those colours are
						// working values that were never meant to be seen. Taking them
						// at face value painted a blue robe black.
						//
						// Nothing here evaluates lighting, so the material's own colour
						// stands in for its result. Only 888 of the 8294 shapes carry
						// normals at all; the rest set colour and mean it, and they are
						// left alone.
						r = litR;
						g = litG;
						b = litB;
						p++;
						break;

					case 34:                      // texture coordinate, in 1/16 texels
					{
						uint packed = U32(data, list + p * 4);
						u = (short)packed / 16f;
						v = (short)(packed >> 16) / 16f;
						p++;
						break;
					}

					case 35:                      // a full 16-bit position
					{
						uint first = U32(data, list + p * 4);
						x = (short)first << scale;
						y = (short)(first >> 16) << scale;
						p++;
						z = (short)U32(data, list + p * 4) << scale;
						p++;
						Emit();
						break;
					}

					case 36:                      // three 10-bit values in one word
					{
						uint packed = U32(data, list + p * 4);
						x = (short)(packed << 6) << scale;
						y = (short)(packed >> 10 << 6) << scale;
						z = (short)(packed >> 20 << 6) << scale;
						p++;
						Emit();
						break;
					}

					case 37:                      // two axes, the third unchanged
					{
						uint packed = U32(data, list + p * 4);
						x = (short)packed << scale;
						y = (short)(packed >> 16) << scale;
						p++;
						Emit();
						break;
					}

					case 38:
					{
						uint packed = U32(data, list + p * 4);
						x = (short)packed << scale;
						z = (short)(packed >> 16) << scale;
						p++;
						Emit();
						break;
					}

					case 39:
					{
						uint packed = U32(data, list + p * 4);
						y = (short)packed << scale;
						z = (short)(packed >> 16) << scale;
						p++;
						Emit();
						break;
					}

					case 40:                      // a small step from the last position
					{
						uint packed = U32(data, list + p * 4);
						x += ((short)(packed << 6) >> 6) << scale;
						y += ((short)(packed >> 10 << 6) >> 6) << scale;
						z += ((short)(packed >> 20 << 6) >> 6) << scale;
						p++;
						Emit();
						break;
					}

					case 64:                      // begin: this is the primitive type
						run = new Mdl0Run { Kind = (Mdl0Primitive)(U32(data, list + p * 4) & 3) };
						piece.Runs.Add(run);
						p++;
						break;

					case 65:                      // end
						Close();
						run = null;
						break;

					case 0:
						break;

					default:
						// preBuild calls OS_Terminate here. Nothing in the game reaches
						// it, and stopping beats emitting nonsense if anything ever does.
						Close();
						return;
				}

				n++;
				word >>= 8;
				if (++lane == 4)
				{
					lane = 0;
					n = p;
					if (n >= words)
					{
						break;
					}
					word = U32(data, list + n * 4);
					p = n + 1;
				}
			}

			Close();
			return;

			void Emit()
			{
				if (run == null)
				{
					return;
				}

				// The matrix is 1/4096ths too, so the product needs shifting back.
				long px = x, py = y, pz = z;
				long tx = (px * active[0] + py * active[3] + pz * active[6] + 2048 >> 12) + active[9];
				long ty = (px * active[1] + py * active[4] + pz * active[7] + 2048 >> 12) + active[10];
				long tz = (px * active[2] + py * active[5] + pz * active[8] + 2048 >> 12) + active[11];

				run.Vertices.Add(new Mdl0Vertex
				{
					X = tx / 4096f,
					Y = ty / 4096f,
					Z = tz / 4096f,
					U = u,
					V = v,
					R = r,
					G = g,
					B = b
				});
				model.GotVertices++;
			}

			void Close()
			{
				if (run == null)
				{
					return;
				}

				int count = run.Vertices.Count;
				switch (run.Kind)
				{
					case Mdl0Primitive.Triangles: model.GotTriangles += count / 3; break;
					case Mdl0Primitive.Quads: model.GotQuads += count / 4; break;
					case Mdl0Primitive.TriangleStrip: model.GotTriangles += Math.Max(0, count - 2); break;
					case Mdl0Primitive.QuadStrip: model.GotQuads += Math.Max(0, count / 2 - 1); break;
				}
			}
		}

		// ------------------------------------------------------------------ helpers

		private static int[] Identity()
		{
			return new[] { 4096, 0, 0, 0, 4096, 0, 0, 0, 4096, 0, 0, 0 };
		}

		private static int[] Copy(int[] m)
		{
			return (int[])m.Clone();
		}

		/// <summary>MTX_Concat43, kept in fixed point so it rounds the way the game does.</summary>
		private static int[] Concat(int[] a, int[] b)
		{
			int[] r = new int[12];
			for (int row = 0; row < 4; row++)
			{
				for (int col = 0; col < 3; col++)
				{
					long sum = (long)a[row * 3] * b[col]
						+ (long)a[row * 3 + 1] * b[3 + col]
						+ (long)a[row * 3 + 2] * b[6 + col];
					r[row * 3 + col] = (int)(sum + 2048 >> 12) + (row == 3 ? b[9 + col] : 0);
				}
			}
			return r;
		}

		private static void ScaleApply(int[] m, int x, int y, int z)
		{
			for (int i = 0; i < 3; i++)
			{
				m[i] = (int)((long)m[i] * x + 2048 >> 12);
				m[3 + i] = (int)((long)m[3 + i] * y + 2048 >> 12);
				m[6 + i] = (int)((long)m[6 + i] * z + 2048 >> 12);
			}
		}

		private static List<(string, byte[])> Dict(byte[] data, int at)
		{
			int count = data[at + 1];
			int entries = at + U16(data, at + 6);
			int unit = U16(data, entries);
			int names = entries + U16(data, entries + 2);
			int values = entries + 4;

			List<(string, byte[])> list = new List<(string, byte[])>(count);
			for (int i = 0; i < count; i++)
			{
				byte[] entry = new byte[unit];
				Buffer.BlockCopy(data, values + unit * i, entry, 0, unit);
				list.Add((Name(data, names + 16 * i), entry));
			}
			return list;
		}

		/// <summary>A 16 byte name, read the way the texture names are.</summary>
		private static string Name(byte[] data, int at)
		{
			byte[] name = new byte[17];
			Buffer.BlockCopy(data, at, name, 0, Math.Min(16, data.Length - at));
			try
			{
				return ShiftJis.Decode(name, 0, out int _).Trim();
			}
			catch (NotSupportedException)
			{
				int stop = 0;
				while (stop < 16 && name[stop] != 0)
				{
					stop++;
				}
				return Encoding.ASCII.GetString(name, 0, stop).Trim();
			}
		}

		private static int[] ReadMatrix(byte[] data, int at)
		{
			int[] m = new int[12];
			for (int i = 0; i < 12; i++)
			{
				m[i] = S32(data, at + i * 4);
			}
			return m;
		}

		private static ushort U16(byte[] d, int a) => (ushort)(d[a] | (d[a + 1] << 8));
		private static short S16(byte[] d, int a) => (short)(d[a] | (d[a + 1] << 8));
		private static uint U32(byte[] d, int a) =>
			(uint)(d[a] | (d[a + 1] << 8) | (d[a + 2] << 16) | (d[a + 3] << 24));
		private static int S32(byte[] d, int a) => (int)U32(d, a);
	}
}
