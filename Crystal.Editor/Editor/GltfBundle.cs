// A mod's own model (glTF, under the project's assets/) as the map editor's 3D view draws
// models: the same ModelBundle the game's packages come out as, so a scene object wearing
// assets/hut.glb stands in the view like any n021. The shared reader (Shared/Graphics/
// GltfFile.cs) does the format; this lays its arrays out as x, y, z, u, v, r, g, b per
// vertex with a group per primitive, and serves the pictures.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenFF.Graphics;

namespace Crystal.Editor
{
	internal static class GltfBundle
	{
		public const string Folder = "assets";

		/// <summary>Whether a model name is one of the project's own files (assets/x.glb).</summary>
		public static bool IsAsset(string name) => name != null && (name.EndsWith(".glb", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".gltf", StringComparison.OrdinalIgnoreCase));

		/// <summary>The file on disk for a model name, inside the project, or null.</summary>
		public static string Resolve(Project project, string name)
		{
			if (project == null || string.IsNullOrEmpty(name)) return null;
			string full = Path.GetFullPath(Path.Combine(project.Directory, name.Replace('/', Path.DirectorySeparatorChar)));
			string root = Path.GetFullPath(project.Directory) + Path.DirectorySeparatorChar;
			return full.StartsWith(root, StringComparison.OrdinalIgnoreCase) && File.Exists(full) ? full : null;
		}

		/// <summary>The project's model files, as the model picker offers them: name (assets/hut.glb), size.</summary>
		public static List<object> List(Project project)
		{
			List<object> list = new List<object>();
			if (project == null) return list;
			string folder = Path.Combine(project.Directory, Folder);
			if (!Directory.Exists(folder)) return list;
			// Which game model each file stands in for (defs/models), so the list can say so.
			Dictionary<string, string> standsIn = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			try { foreach (OpenFF.Data.ModModel d in OpenFF.Data.ModModels.Load(new[] { project.Directory })) if (!string.IsNullOrEmpty(d.Gltf) && !string.IsNullOrEmpty(d.Model)) standsIn[d.Gltf.Replace('\\', '/')] = d.Model; }
			catch (Exception) { /* no definitions readable: the list says nothing */ }
			foreach (string file in Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories).Where(IsAsset).OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
			{
				// The kind (flagged in the file, else inferred): what the pickers offer it for - a
				// character under a weapon, a weapon in a hand. The JSON header only; a .gltf counts as a prop.
				string kind = "prop";
				if (file.EndsWith(".glb", StringComparison.OrdinalIgnoreCase))
				{
					try { kind = Gltf.KindOf(File.ReadAllBytes(file)).Kind; } catch (Exception) { /* unreadable: a prop */ }
				}
				string name = Path.GetRelativePath(project.Directory, file).Replace(Path.DirectorySeparatorChar, '/');
				list.Add(new { name, bytes = new FileInfo(file).Length, kind, standsInFor = standsIn.TryGetValue(name, out string model) ? model : null });
			}
			return list;
		}

		public static ModelBundle Read(Project project, string name)
		{
			string path = Resolve(project, name);
			if (path == null) return new ModelBundle { Name = name, Problem = "no file " + name + " in the project" };
			GltfFile file = GltfFile.Load(path);
			ModelBundle bundle = new ModelBundle { Name = name, Buffer = new List<float>(), Indices = new List<int>(), Groups = new List<ModelGroup>(), Nodes = file.Meshes.Select(m => m.Node).Distinct().ToList(), Notes = file.Notes };
			float[] light = Normalise(new[] { 0.4f, 1f, 0.6f });
			int vertexBase = 0;
			foreach (GltfMesh mesh in file.Meshes)
			{
				GltfMaterial material = mesh.Material >= 0 && mesh.Material < file.Materials.Count ? file.Materials[mesh.Material] : new GltfMaterial();
				ModelGroup group = new ModelGroup
				{
					Shape = mesh.Node, Node = mesh.Node, Material = material.Name ?? ("material " + mesh.Material),
					Texture = material.Image >= 0 ? "image" + material.Image : null,
					Start = bundle.Indices.Count, Count = mesh.Indices.Length,
					Colour = (Byte(material.BaseColour[0]) << 16) | (Byte(material.BaseColour[1]) << 8) | Byte(material.BaseColour[2]),
					Alpha = material.BaseColour[3], Translucent = material.Blend,
					WrapS = "repeat", WrapT = "repeat", Matrices = new List<float[]> { new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0 } }, Slots = new List<int>(), Piece = bundle.Groups.Count
				};
				for (int v = 0; v < mesh.VertexCount; v++)
				{
					float shade = 1f;
					if (mesh.Normals != null)
					{
						float dot = mesh.Normals[v * 3] * light[0] + mesh.Normals[v * 3 + 1] * light[1] + mesh.Normals[v * 3 + 2] * light[2];
						shade = 0.55f + 0.45f * Math.Max(0f, dot);
					}
					float r = material.BaseColour[0], g = material.BaseColour[1], b = material.BaseColour[2];
					if (mesh.Colours != null) { r *= mesh.Colours[v * 4]; g *= mesh.Colours[v * 4 + 1]; b *= mesh.Colours[v * 4 + 2]; }
					bundle.Buffer.Add(mesh.Positions[v * 3]); bundle.Buffer.Add(mesh.Positions[v * 3 + 1]); bundle.Buffer.Add(mesh.Positions[v * 3 + 2]);
					bundle.Buffer.Add(mesh.Uvs != null ? mesh.Uvs[v * 2] : 0f); bundle.Buffer.Add(mesh.Uvs != null ? mesh.Uvs[v * 2 + 1] : 0f);
					bundle.Buffer.Add(Math.Min(1f, r * shade)); bundle.Buffer.Add(Math.Min(1f, g * shade)); bundle.Buffer.Add(Math.Min(1f, b * shade));
					bundle.MatrixIndex.Add(0);
				}
				foreach (int i in mesh.Indices) bundle.Indices.Add(vertexBase + i);
				vertexBase += mesh.VertexCount;
				bundle.Groups.Add(group);
			}
			bundle.Vertices = vertexBase;
			bundle.Triangles = file.Triangles;
			bundle.Skin = SkinOf(project, name, file);
			// Whether the normals are the file's own or recalculated (the inspector's Normals card).
			if (path.EndsWith(".glb", StringComparison.OrdinalIgnoreCase))
			{
				try
				{
					byte[] glb = File.ReadAllBytes(path);
					(float? angle, bool source) = Gltf.NormalsOf(glb); bundle.NormalsAngle = angle; bundle.NormalsSource = source;
					(string kind, bool flagged) = Gltf.KindOf(glb); bundle.Kind = kind; bundle.KindFlagged = flagged;
				}
				catch (Exception) { /* a file the writer cannot read stays as it is */ }
			}
			else { bundle.Kind = bundle.Skin != null ? "character" : "prop"; }
			if (file.Meshes.Count > 0)
			{
				bundle.Centre = new[] { (file.Min[0] + file.Max[0]) / 2, (file.Min[1] + file.Max[1]) / 2, (file.Min[2] + file.Max[2]) / 2 };
				bundle.Radius = (float)Math.Sqrt(Math.Pow(file.Max[0] - file.Min[0], 2) + Math.Pow(file.Max[1] - file.Min[1], 2) + Math.Pow(file.Max[2] - file.Min[2], 2)) / 2;
			}
			else { bundle.Centre = new float[3]; bundle.Radius = 1; bundle.Problem = string.Join("; ", file.Notes); }
			return bundle;
		}

		/// <summary>
		/// The file's skin for the viewer, or null for a file without one: every skin's joints in
		/// one list, each vertex's four joints and weights against it, the mesh-space positions,
		/// and the game model to drive it - from a defs/models definition naming this file, else
		/// j101 when the joints carry the game's character bone names (hara, atama).
		/// </summary>
		private static ModelSkin SkinOf(Project project, string name, GltfFile file)
		{
			if (file.Skins.Count == 0 || !file.Meshes.Any(m => m.Skin >= 0 && m.Joints != null && m.Weights != null)) return null;
			ModelSkin skin = new ModelSkin();
			int[] offset = new int[file.Skins.Count];
			for (int s = 0; s < file.Skins.Count; s++)
			{
				offset[s] = skin.Joints.Count;
				GltfSkin one = file.Skins[s];
				for (int j = 0; j < one.Joints.Length; j++)
				{
					int node = one.Joints[j];
					skin.Joints.Add(node >= 0 && node < file.Nodes.Count ? file.Nodes[node].Name : ("joint" + j));
					skin.InverseBind.AddRange(one.InverseBind[j]);
					// The parent joint, by the file's node tree: the nearest ancestor that is a joint of this skin.
					int parent = -1;
					for (int up = node >= 0 && node < file.Nodes.Count ? file.Nodes[node].Parent : -1; up >= 0 && parent < 0; up = file.Nodes[up].Parent)
					{
						int at = Array.IndexOf(one.Joints, up);
						if (at >= 0) parent = offset[s] + at;
					}
					skin.Parents.Add(parent);
				}
			}
			foreach (GltfMesh mesh in file.Meshes)
			{
				bool skinned = mesh.Skin >= 0 && mesh.Skin < file.Skins.Count && mesh.Joints != null && mesh.Weights != null;
				float[] local = skinned ? mesh.LocalPositions : mesh.Positions;
				for (int v = 0; v < mesh.VertexCount; v++)
				{
					skin.Local.Add(local[v * 3]); skin.Local.Add(local[v * 3 + 1]); skin.Local.Add(local[v * 3 + 2]);
					for (int k = 0; k < 4; k++)
					{
						float w = skinned ? mesh.Weights[v * 4 + k] : 0f;
						int j = skinned && w > 0 ? offset[mesh.Skin] + (int)mesh.Joints[v * 4 + k] : -1;
						skin.JointIndex.Add(j);
						skin.Weights.Add(j >= 0 ? w : 0f);
					}
				}
			}
			foreach (OpenFF.Data.ModModel definition in OpenFF.Data.ModModels.Load(project != null ? new[] { project.Directory } : Array.Empty<string>()))
			{
				if (string.Equals(definition.Gltf?.Replace('\\', '/'), name.Replace('\\', '/'), StringComparison.OrdinalIgnoreCase)) { skin.Model = "files/" + definition.Model + ".nmdp.lz"; break; }
			}
			if (skin.Model == null && skin.Joints.Contains("hara") && skin.Joints.Contains("atama")) skin.Model = "files/j101.nmdp.lz";
			return skin;
		}

		/// <summary>A picture of the model, by the group's texture name (image0…), as its bytes and mime type.</summary>
		public static (byte[] bytes, string mime) Texture(Project project, string name, string texture)
		{
			string path = Resolve(project, name);
			if (path == null) throw new FileNotFoundException("no file " + name);
			GltfFile file = GltfFile.Load(path);
			if (texture != null && texture.StartsWith("image") && int.TryParse(texture.Substring(5), out int index) && index >= 0 && index < file.Images.Count && file.Images[index].Bytes != null)
				return (file.Images[index].Bytes, file.Images[index].MimeType ?? "image/png");
			throw new FileNotFoundException("no picture " + texture + " in " + name);
		}

		private static int Byte(float v) => (int)Math.Max(0, Math.Min(255, Math.Round(v * 255)));
		private static float[] Normalise(float[] v) { float l = (float)Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]); return new[] { v[0] / l, v[1] / l, v[2] / l }; }
	}
}
