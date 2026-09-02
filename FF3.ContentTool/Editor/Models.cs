// The 3D models, for the editor.
//
// The viewer in the browser wants triangles and a texture, not display lists and a
// matrix stack, so the unpacking happens here and what goes over the wire is one
// bundle per model: a flat vertex buffer, an index buffer, and a list of groups saying
// which range of indices uses which texture.
//
// Strips are expanded to triangles and quads split in two on this side. That is work
// the browser would otherwise repeat on every redraw, and it keeps the viewer to the
// one job of drawing what it is given.

using System;
using System.Collections.Generic;
using System.Linq;

namespace FF3.ContentTool.Editor
{
	internal sealed class ModelPackage
	{
		public string Name { get; set; }

		/// <summary>The .ntxp this model takes its textures from, if it has none.</summary>
		public string Textures { get; set; }
	}

	internal sealed class ModelGroup
	{
		public string Shape { get; set; }
		public string Node { get; set; }
		public string Material { get; set; }
		public string Texture { get; set; }

		/// <summary>Where in the index buffer this group's triangles are.</summary>
		public int Start { get; set; }
		public int Count { get; set; }

		public int Colour { get; set; }          // 0xRRGGBB, for an untextured group
		public float Alpha { get; set; }
		public bool Hidden { get; set; }

		/// <summary>
		/// Drawn in the second pass. The game's own test: alpha of 16 or less, or a
		/// texture in one of the two formats that carry their own alpha.
		/// </summary>
		public bool Translucent { get; set; }

		/// <summary>0 none, 1 faces the camera, 2 turns only about its vertical axis.</summary>
		public int Billboard { get; set; }

		/// <summary>What a billboard turns about.</summary>
		public float[] Pivot { get; set; }
	}

	internal sealed class ModelBundle
	{
		public string Name { get; set; }
		public int Vertices { get; set; }
		public int Triangles { get; set; }
		public int Quads { get; set; }
		public List<string> Nodes { get; set; }

		/// <summary>x, y, z, u, v, r, g, b per vertex - u and v already 0..1.</summary>
		public List<float> Buffer { get; set; }
		public List<int> Indices { get; set; }
		public List<ModelGroup> Groups { get; set; }

		public float[] Centre { get; set; }
		public float Radius { get; set; }
		public string Problem { get; set; }
	}

	internal static class Models
	{
		public static List<ModelPackage> List(Workspace workspace)
		{
			HashSet<string> textures = new HashSet<string>(
				workspace.List(".lz")
					.Where(e => e.Name.EndsWith(".ntxp.lz", StringComparison.OrdinalIgnoreCase))
					.Select(e => e.Name),
				StringComparer.OrdinalIgnoreCase);

			return workspace.List(".lz")
				.Where(e => e.Name.EndsWith(".nmdp.lz", StringComparison.OrdinalIgnoreCase))
				.Select(e =>
				{
					string sibling = e.Name.Substring(0, e.Name.Length - 8) + ".ntxp.lz";
					return new ModelPackage
					{
						Name = e.Name,
						Textures = textures.Contains(sibling) ? sibling : null
					};
				})
				.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
				.ToList();
		}

		/// <summary>One model, flattened into something a browser can draw.</summary>
		public static ModelBundle Read(Workspace workspace, string name)
		{
			byte[] data = Lz.Decompress(workspace.Read(name));
			if (Mdl0.Find(data) < 0)
			{
				return new ModelBundle { Name = name, Problem = "no geometry in this package" };
			}

			List<Mdl0Model> models = Mdl0.Read(data);
			if (models.Count == 0)
			{
				return new ModelBundle { Name = name, Problem = "no models in this package" };
			}

			Mdl0Model model = models[0];
			Dictionary<string, int> formats = TextureFormats(workspace, name);
			ModelBundle bundle = new ModelBundle
			{
				Name = model.Name,
				Vertices = model.Vertices,
				Triangles = model.Triangles,
				Quads = model.Quads,
				Nodes = model.Nodes,
				Buffer = new List<float>(),
				Indices = new List<int>(),
				Groups = new List<ModelGroup>()
			};

			foreach (Mdl0Piece piece in model.Pieces)
			{
				Mdl0Material material = model.Materials
					.FirstOrDefault(m => string.Equals(m.Name, piece.Material, StringComparison.Ordinal));
				float width = material != null && material.Width > 0 ? material.Width : 1f;
				float height = material != null && material.Height > 0 ? material.Height : 1f;

				ModelGroup group = new ModelGroup
				{
					Shape = piece.Shape,
					Node = piece.Node,
					Material = piece.Material,
					Texture = material?.Texture,
					Start = bundle.Indices.Count,
					Colour = material != null ? (material.R << 16) | (material.G << 8) | material.B : 0xFFFFFF,
					Alpha = material != null ? Math.Min(1f, material.Alpha / 31f) : 1f,
					Hidden = piece.Hidden,
					Translucent = Translucent(material, formats),
					Billboard = piece.Billboard,
					Pivot = piece.Billboard == 0
						? null : new[] { piece.PivotX, piece.PivotY, piece.PivotZ }
				};

				foreach (Mdl0Run run in piece.Runs)
				{
					int first = bundle.Buffer.Count / 8;
					foreach (Mdl0Vertex v in run.Vertices)
					{
						bundle.Buffer.Add(v.X);
						bundle.Buffer.Add(v.Y);
						bundle.Buffer.Add(v.Z);
						bundle.Buffer.Add(v.U / width);
						bundle.Buffer.Add(v.V / height);
						bundle.Buffer.Add(v.R / 255f);
						bundle.Buffer.Add(v.G / 255f);
						bundle.Buffer.Add(v.B / 255f);
					}

					Triangulate(run, first, bundle.Indices);
				}

				group.Count = bundle.Indices.Count - group.Start;
				bundle.Groups.Add(group);
			}

			Frame(bundle);
			return bundle;
		}

		/// <summary>
		/// Which pass a group belongs in, by the game's own rule: alpha of 16 or less
		/// out of 31, or a texture in format 1 (a3i5) or 6 (a5i3) - the two that carry
		/// alpha per pixel rather than per material.
		/// </summary>
		private static bool Translucent(Mdl0Material material, Dictionary<string, int> formats)
		{
			if (material == null)
			{
				return false;
			}
			if (material.Alpha <= 16)
			{
				return true;
			}

			return material.Texture != null
				&& formats.TryGetValue(material.Texture, out int format)
				&& (format == 1 || format == 6);
		}

		/// <summary>Texture name -> its format, for the pass split.</summary>
		private static Dictionary<string, int> TextureFormats(Workspace workspace, string name)
		{
			Dictionary<string, int> formats =
				new Dictionary<string, int>(StringComparer.Ordinal);

			string sibling = name.EndsWith(".nmdp.lz", StringComparison.OrdinalIgnoreCase)
				? name.Substring(0, name.Length - 8) + ".ntxp.lz" : null;

			foreach (string source in new[] { name, sibling })
			{
				if (source == null || !workspace.Exists(source))
				{
					continue;
				}
				try
				{
					byte[] data = Lz.Decompress(workspace.Read(source));
					if (Tex0.Find(data) < 0)
					{
						continue;
					}
					foreach (Tex0Texture texture in Tex0.Read(data).Textures)
					{
						if (!formats.ContainsKey(texture.Name))
						{
							formats[texture.Name] = texture.Format;
						}
					}
				}
				catch (Exception)
				{
					// No formats means everything lands in the opaque pass, which is
					// the same as before this existed.
				}
			}
			return formats;
		}

		/// <summary>
		/// Everything becomes triangles. Strips alternate their winding, so every other
		/// one is swapped back - without that, half of any strip faces inwards.
		/// </summary>
		private static void Triangulate(Mdl0Run run, int first, List<int> indices)
		{
			int count = run.Vertices.Count;
			switch (run.Kind)
			{
				case Mdl0Primitive.Triangles:
					for (int i = 0; i + 2 < count; i += 3)
					{
						indices.Add(first + i);
						indices.Add(first + i + 1);
						indices.Add(first + i + 2);
					}
					break;

				case Mdl0Primitive.Quads:
					for (int i = 0; i + 3 < count; i += 4)
					{
						Quad(indices, first + i, first + i + 1, first + i + 2, first + i + 3);
					}
					break;

				case Mdl0Primitive.TriangleStrip:
					for (int i = 0; i + 2 < count; i++)
					{
						if ((i & 1) == 0)
						{
							indices.Add(first + i);
							indices.Add(first + i + 1);
							indices.Add(first + i + 2);
						}
						else
						{
							indices.Add(first + i + 1);
							indices.Add(first + i);
							indices.Add(first + i + 2);
						}
					}
					break;

				case Mdl0Primitive.QuadStrip:
					for (int i = 0; i + 3 < count; i += 2)
					{
						Quad(indices, first + i, first + i + 1, first + i + 3, first + i + 2);
					}
					break;
			}
		}

		private static void Quad(List<int> indices, int a, int b, int c, int d)
		{
			indices.Add(a);
			indices.Add(b);
			indices.Add(c);
			indices.Add(a);
			indices.Add(c);
			indices.Add(d);
		}

		/// <summary>Where to point the camera, so a model shows up without hunting for it.</summary>
		private static void Frame(ModelBundle bundle)
		{
			if (bundle.Buffer.Count == 0)
			{
				bundle.Centre = new[] { 0f, 0f, 0f };
				bundle.Radius = 1f;
				return;
			}

			float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
			float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;
			for (int i = 0; i < bundle.Buffer.Count; i += 8)
			{
				minX = Math.Min(minX, bundle.Buffer[i]); maxX = Math.Max(maxX, bundle.Buffer[i]);
				minY = Math.Min(minY, bundle.Buffer[i + 1]); maxY = Math.Max(maxY, bundle.Buffer[i + 1]);
				minZ = Math.Min(minZ, bundle.Buffer[i + 2]); maxZ = Math.Max(maxZ, bundle.Buffer[i + 2]);
			}

			bundle.Centre = new[] { (minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2 };
			bundle.Radius = Math.Max(0.001f, Math.Max(maxX - minX,
				Math.Max(maxY - minY, maxZ - minZ)) / 2);
		}

		/// <summary>
		/// A texture a model's material asks for, by name. It is in the package itself
		/// or in the .ntxp beside it, so both are tried - the same rule the game uses.
		/// </summary>
		public static byte[] Texture(Workspace workspace, string name, string texture)
		{
			string sibling = name.EndsWith(".nmdp.lz", StringComparison.OrdinalIgnoreCase)
				? name.Substring(0, name.Length - 8) + ".ntxp.lz" : null;

			foreach (string source in new[] { name, sibling })
			{
				if (source == null || !workspace.Exists(source))
				{
					continue;
				}

				byte[] data;
				try
				{
					data = Lz.Decompress(workspace.Read(source));
				}
				catch (Exception)
				{
					continue;
				}

				if (Tex0.Find(data) < 0)
				{
					continue;
				}

				Tex0File package = Tex0.Read(data);
				foreach (Tex0Texture found in package.Textures)
				{
					if (string.Equals(found.Name, texture, StringComparison.Ordinal)
						&& found.Problem == null)
					{
						return Png.Encode(found.Width, found.Height, Tex0.Decode(package, found));
					}
				}
			}

			throw new KeyNotFoundException("no texture called " + texture + " for " + name);
		}
	}
}
