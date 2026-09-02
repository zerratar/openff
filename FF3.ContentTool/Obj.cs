// Wavefront OBJ, for looking at what came out of MDL0.
//
// OBJ was picked because everything opens it and it is plain text, so a wrong vertex is
// something you can read rather than something you have to hex dump. It carries the
// three things the models actually use - position, texture coordinate, and a material
// per face group - and drops the one it does not, per-vertex colour, which the MTL
// format has no place for. Colour is written into the material's diffuse when a shape
// is untextured, which is the case where it is doing the visible work.
//
// Strips are expanded into triangles and quads here rather than left as runs, because
// OBJ has no notion of a strip. That is the only reshaping done; the numbers are the
// ones the decoder produced.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace FF3.ContentTool
{
	internal static class Obj
	{
		/// <summary>
		/// Writes one model as .obj plus .mtl. Texture file names are written as given -
		/// the caller decides whether to put the PNGs beside them.
		/// </summary>
		public static void Write(string path, Mdl0Model model,
			IDictionary<string, string> textureFiles = null)
		{
			string materialFile = Path.ChangeExtension(path, ".mtl");
			StringBuilder obj = new StringBuilder();
			CultureInfo n = CultureInfo.InvariantCulture;

			obj.Append("# ").Append(model.Name).Append(" - from FF3 MDL0\n");
			obj.Append("mtllib ").Append(Path.GetFileName(materialFile)).Append('\n');

			int vertex = 1;
			int coord = 1;
			foreach (Mdl0Piece piece in model.Pieces)
			{
				Mdl0Material material = Find(model, piece.Material);
				float width = material != null && material.Width > 0 ? material.Width : 1f;
				float height = material != null && material.Height > 0 ? material.Height : 1f;

				// A hidden piece is still written out - it is real geometry - but named
				// so it can be picked out and deleted if you want what the game draws.
				obj.Append("\no ").Append(Safe(piece.Shape))
					.Append('_').Append(Safe(piece.Node ?? "node"))
					.Append(piece.Hidden ? "_hidden" : string.Empty).Append('\n');
				if (piece.Material != null)
				{
					obj.Append("usemtl ").Append(Safe(piece.Material)).Append('\n');
				}

				List<int> indices = new List<int>();
				List<int> quadIndices = new List<int>();

				foreach (Mdl0Run run in piece.Runs)
				{
					int first = vertex;
					foreach (Mdl0Vertex v in run.Vertices)
					{
						obj.Append("v ").Append(v.X.ToString("0.####", n))
							.Append(' ').Append(v.Y.ToString("0.####", n))
							.Append(' ').Append(v.Z.ToString("0.####", n)).Append('\n');
						// OBJ counts texture coordinates up the image, the NDS down it.
						obj.Append("vt ").Append((v.U / width).ToString("0.#####", n))
							.Append(' ').Append((1f - v.V / height).ToString("0.#####", n))
							.Append('\n');
						vertex++;
						coord++;
					}

					Faces(run, first, indices, quadIndices);
				}

				for (int i = 0; i + 2 < indices.Count; i += 3)
				{
					Face(obj, indices[i], indices[i + 1], indices[i + 2]);
				}
				for (int i = 0; i + 3 < quadIndices.Count; i += 4)
				{
					Face(obj, quadIndices[i], quadIndices[i + 1],
						quadIndices[i + 2], quadIndices[i + 3]);
				}
			}

			File.WriteAllText(path, obj.ToString());
			File.WriteAllText(materialFile, Materials(model, textureFiles));
		}

		/// <summary>Turns a run into flat triangle and quad index lists.</summary>
		private static void Faces(Mdl0Run run, int first, List<int> triangles, List<int> quads)
		{
			int count = run.Vertices.Count;
			switch (run.Kind)
			{
				case Mdl0Primitive.Triangles:
					for (int i = 0; i + 2 < count; i += 3)
					{
						triangles.Add(first + i);
						triangles.Add(first + i + 1);
						triangles.Add(first + i + 2);
					}
					break;

				case Mdl0Primitive.Quads:
					for (int i = 0; i + 3 < count; i += 4)
					{
						quads.Add(first + i);
						quads.Add(first + i + 1);
						quads.Add(first + i + 2);
						quads.Add(first + i + 3);
					}
					break;

				case Mdl0Primitive.TriangleStrip:
					// Every other triangle is wound the other way round, so the two are
					// swapped back - otherwise half the model faces inwards.
					for (int i = 0; i + 2 < count; i++)
					{
						if ((i & 1) == 0)
						{
							triangles.Add(first + i);
							triangles.Add(first + i + 1);
							triangles.Add(first + i + 2);
						}
						else
						{
							triangles.Add(first + i + 1);
							triangles.Add(first + i);
							triangles.Add(first + i + 2);
						}
					}
					break;

				case Mdl0Primitive.QuadStrip:
					for (int i = 0; i + 3 < count; i += 2)
					{
						quads.Add(first + i);
						quads.Add(first + i + 1);
						quads.Add(first + i + 3);
						quads.Add(first + i + 2);
					}
					break;
			}
		}

		private static void Face(StringBuilder obj, params int[] indices)
		{
			obj.Append('f');
			foreach (int i in indices)
			{
				obj.Append(' ').Append(i.ToString(CultureInfo.InvariantCulture))
					.Append('/').Append(i.ToString(CultureInfo.InvariantCulture));
			}
			obj.Append('\n');
		}

		private static string Materials(Mdl0Model model, IDictionary<string, string> textureFiles)
		{
			CultureInfo n = CultureInfo.InvariantCulture;
			StringBuilder mtl = new StringBuilder();

			foreach (Mdl0Material material in model.Materials)
			{
				mtl.Append("newmtl ").Append(Safe(material.Name)).Append('\n');
				mtl.Append("Kd ")
					.Append((material.R / 255f).ToString("0.####", n)).Append(' ')
					.Append((material.G / 255f).ToString("0.####", n)).Append(' ')
					.Append((material.B / 255f).ToString("0.####", n)).Append('\n');
				// Alpha is 0-31 in polyAttr, and 31 means opaque.
				mtl.Append("d ").Append((material.Alpha / 31f).ToString("0.####", n)).Append('\n');

				string file = null;
				if (material.Texture != null && textureFiles != null)
				{
					textureFiles.TryGetValue(material.Texture, out file);
				}
				if (file != null)
				{
					mtl.Append("map_Kd ").Append(file).Append('\n');
				}
				else if (material.Texture != null)
				{
					mtl.Append("# texture ").Append(material.Texture)
						.Append(" is not in this package\n");
				}
				mtl.Append('\n');
			}
			return mtl.ToString();
		}

		private static Mdl0Material Find(Mdl0Model model, string name)
		{
			if (name == null)
			{
				return null;
			}
			foreach (Mdl0Material material in model.Materials)
			{
				if (string.Equals(material.Name, name, StringComparison.Ordinal))
				{
					return material;
				}
			}
			return null;
		}

		/// <summary>OBJ names cannot hold spaces, and a few of these do.</summary>
		private static string Safe(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return "unnamed";
			}

			StringBuilder safe = new StringBuilder(name.Length);
			foreach (char c in name)
			{
				safe.Append(c == ' ' || c == '\t' || c == '#' ? '_' : c);
			}
			return safe.ToString();
		}
	}
}
