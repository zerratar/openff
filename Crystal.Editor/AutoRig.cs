// A mesh with no rig bound to a game character's skeleton - the way to a model of your own
// that plays every motion the game has, without rigging it yourself.
//
//   crystal mdl-autorig <file.glb|.gltf> <model> [out.glb] [--target=steam] [--scale=n] [--rotation=x,y,z] [--offset=x,y,z]
//
// The mesh is fitted into the model's space - scaled to the model's height, stood on its
// floor, its middle over the model's, or as the options say - and then weighted by the
// game's own skinning: every vertex takes the bone weights of the nearest vertices of the
// original model (a handful, nearer ones counting more), which is where the game's own
// blends at the knees, elbows and hips come from, and the weights are then smoothed over
// the mesh so a seam or a stray triangle does not jump bones. What comes out is a skinned
// glTF whose joints are the model's nodes by name at the model's bind pose - what the OpenFF
// client draws exactly (CharacterMeshes, the direct path) and what Mdl0Reskin writes into
// the game's format for the viewer and the Steam game. Nothing here needs the mesh to
// resemble the original closely; the closer its volume, the better the weights land.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Crystal.Editor;
using OpenFF.Graphics;

namespace Crystal
{
	internal static class AutoRig
	{
		public sealed class Result
		{
			public byte[] Glb;
			public int Vertices, Triangles, Bones;
			public float Scale;
			public float[] Offset;
			public List<string> Notes = new List<string>();
		}

		/// <summary>
		/// The file's meshes bound to <paramref name="modelName"/>'s skeleton. <paramref name="scale"/> 0
		/// fits the model's height; <paramref name="offset"/> null stands it on the floor over the
		/// model's middle; <paramref name="rotation"/> turns it (degrees about x, y, z) before either.
		/// </summary>
		public static Result Build(Workspace workspace, string modelName, GltfFile file, float scale = 0, float[] rotation = null, float[] offset = null, string posePack = null, int poseIndex = 0, int poseFrame = 0)
		{
			if (file == null) throw new ArgumentNullException(nameof(file));
			List<GltfMesh> meshes = file.Meshes.Where(m => m.Indices != null && m.Indices.Length >= 3 && m.Positions != null).ToList();
			if (meshes.Count == 0) throw new InvalidDataException("the file has no triangles");
			ModelBundle original = Models.Read(workspace, modelName, shipped: true);
			if (original.Problem != null) throw new InvalidDataException(original.Problem);
			Models.Rig rig = Models.ReadRig(workspace, modelName, null, 0, false, shipped: true);
			Result result = new Result();

			// ---- the original's vertices with their weights: the reference ---------------------------------
			int originalCount = original.Buffer.Count / 8;
			int[] groupOf = new int[originalCount];
			for (int g = 0; g < original.Groups.Count; g++)
			{
				ModelGroup group = original.Groups[g];
				for (int i = group.Start; i < group.Start + group.Count; i++) { int v = original.Indices[i]; if (v >= 0 && v < originalCount) groupOf[v] = g; }
			}
			float[] refX = new float[originalCount], refY = new float[originalCount], refZ = new float[originalCount];
			(int Node, float Weight)[][] refWeights = new (int, float)[originalCount][];
			for (int v = 0; v < originalCount; v++)
			{
				refX[v] = original.Buffer[v * 8]; refY[v] = original.Buffer[v * 8 + 1]; refZ[v] = original.Buffer[v * 8 + 2];
				int local = original.MatrixIndex != null && v < original.MatrixIndex.Count ? original.MatrixIndex[v] : 0;
				refWeights[v] = rig.Weights.TryGetValue((groupOf[v], local), out (int, float)[] w) ? w : (rig.Weights.TryGetValue((groupOf[v], 0), out w) ? w : new[] { (0, 1f) });
			}

			// ---- the pose to match ----------------------------------------------------------------------------
			// The file's mesh stands as its author left it - arms down, as a rule - while the game's
			// bind pose is a T. Weights are transferred between like poses, so the original is put
			// into a frame of a motion that looks like the file (the battle idle's first frame has the
			// arms down) when the file's proportions are nearer that than the T; then the file's
			// vertices are taken back into the bind pose through the weights they were given. A pose
			// asked for outright is used as it is.
			float[][] delta = null;                    // per node: bind^-1 x posed, row-vector 4x3; null for the bind pose itself
			float[] fileMin = { float.MaxValue, float.MaxValue, float.MaxValue }, fileMax = { float.MinValue, float.MinValue, float.MinValue };
			float[] turn0 = Rotation(rotation ?? new float[3]);
			foreach (GltfMesh mesh in meshes)
				for (int i = 0; i + 2 < mesh.Positions.Length; i += 3)
				{
					float[] p = Apply(turn0, mesh.Positions[i], mesh.Positions[i + 1], mesh.Positions[i + 2]);
					for (int k = 0; k < 3; k++) { fileMin[k] = Math.Min(fileMin[k], p[k]); fileMax[k] = Math.Max(fileMax[k], p[k]); }
				}
			float fileAspect = (fileMax[0] - fileMin[0]) / Math.Max(1e-4f, fileMax[1] - fileMin[1]);
			string posedAs = "the bind pose";
			{
				string pack = posePack;
				if (pack == null)
				{
					List<Models.MotionPack> packs = Models.Motions(workspace, modelName);
					Models.MotionPack likely = packs.FirstOrDefault(p => p.Likely && p.Motions.Count > 0) ?? packs.FirstOrDefault(p => p.Fits && p.Motions.Count > 0);
					pack = likely?.Name;
					// A party member's j### has no pack of its own name; the battle set b_b01 is what it plays.
					if (System.Text.RegularExpressions.Regex.IsMatch(modelName, @"(^|/)j\d{3}\.", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
						pack = packs.FirstOrDefault(p => p.Fits && System.Text.RegularExpressions.Regex.IsMatch(p.Name, @"(^|/)b_b01\.ncap", System.Text.RegularExpressions.RegexOptions.IgnoreCase))?.Name ?? pack;
				}
				if (pack != null)
				{
					Models.Rig posed = Models.ReadRig(workspace, modelName, pack, poseIndex, false, shipped: true);
					if (posed.Motions.Count > 0 && posed.Motions[0].Worlds.Count > 0)
					{
						float[][] worlds = posed.Motions[0].Worlds[Math.Max(0, Math.Min(posed.Motions[0].Worlds.Count - 1, poseFrame))];
						float[][] candidate = new float[rig.Nodes.Count][];
						for (int n = 0; n < rig.Nodes.Count; n++) candidate[n] = Gltf.Mul(Gltf.Invert(rig.Bind[n]), worlds[n]);
						// The original's box in that pose against its bind box: which is the file nearer?
						float[] pMin = { float.MaxValue, float.MaxValue, float.MaxValue }, pMax = { float.MinValue, float.MinValue, float.MinValue };
						float[] bMin = { float.MaxValue, float.MaxValue, float.MaxValue }, bMax = { float.MinValue, float.MinValue, float.MinValue };
						for (int v = 0; v < originalCount; v++)
						{
							float[] q = Skinned(refX[v], refY[v], refZ[v], refWeights[v], candidate);
							for (int k = 0; k < 3; k++) { pMin[k] = Math.Min(pMin[k], q[k]); pMax[k] = Math.Max(pMax[k], q[k]); }
							float[] b = { refX[v], refY[v], refZ[v] };
							for (int k = 0; k < 3; k++) { bMin[k] = Math.Min(bMin[k], b[k]); bMax[k] = Math.Max(bMax[k], b[k]); }
						}
						float posedAspect = (pMax[0] - pMin[0]) / Math.Max(1e-4f, pMax[1] - pMin[1]);
						float bindAspect = (bMax[0] - bMin[0]) / Math.Max(1e-4f, bMax[1] - bMin[1]);
						if (posePack != null || Math.Abs(posedAspect - fileAspect) < Math.Abs(bindAspect - fileAspect))
						{
							delta = candidate;
							posedAs = Path.GetFileName(pack) + " " + (posed.Motions[0].Name ?? poseIndex.ToString()) + " frame " + poseFrame;
						}
						result.Notes.Add("the file is " + fileAspect.ToString("0.00") + " wide for its height; the model's bind pose is " + bindAspect.ToString("0.00") + ", its " + Path.GetFileName(pack) + " " + (posed.Motions[0].Name ?? "") + " frame " + poseFrame + " is " + posedAspect.ToString("0.00") + " - weights taken against " + posedAs);
					}
				}
			}
			// The reference as posed (or the bind pose), and its box.
			float[] oMin = { float.MaxValue, float.MaxValue, float.MaxValue }, oMax = { float.MinValue, float.MinValue, float.MinValue };
			if (delta != null)
			{
				for (int v = 0; v < originalCount; v++)
				{
					float[] q = Skinned(refX[v], refY[v], refZ[v], refWeights[v], delta);
					refX[v] = q[0]; refY[v] = q[1]; refZ[v] = q[2];
				}
			}
			for (int v = 0; v < originalCount; v++)
			{
				oMin[0] = Math.Min(oMin[0], refX[v]); oMin[1] = Math.Min(oMin[1], refY[v]); oMin[2] = Math.Min(oMin[2], refZ[v]);
				oMax[0] = Math.Max(oMax[0], refX[v]); oMax[1] = Math.Max(oMax[1], refY[v]); oMax[2] = Math.Max(oMax[2], refZ[v]);
			}

			// ---- the fit ------------------------------------------------------------------------------------------
			float[] r = rotation ?? new float[3];
			float[] turn = Rotation(r);
			float[] tMin = { float.MaxValue, float.MaxValue, float.MaxValue }, tMax = { float.MinValue, float.MinValue, float.MinValue };
			foreach (GltfMesh mesh in meshes)
				for (int i = 0; i + 2 < mesh.Positions.Length; i += 3)
				{
					float[] p = Apply(turn, mesh.Positions[i], mesh.Positions[i + 1], mesh.Positions[i + 2]);
					for (int k = 0; k < 3; k++) { tMin[k] = Math.Min(tMin[k], p[k]); tMax[k] = Math.Max(tMax[k], p[k]); }
				}
			float fileHeight = Math.Max(1e-4f, tMax[1] - tMin[1]);
			if (scale <= 0) scale = (oMax[1] - oMin[1]) / fileHeight;
			float[] shift = offset ?? new[]
			{
				(oMin[0] + oMax[0]) / 2 - (tMin[0] + tMax[0]) / 2 * scale,
				oMin[1] - tMin[1] * scale,
				(oMin[2] + oMax[2]) / 2 - (tMin[2] + tMax[2]) / 2 * scale
			};
			float[] fit = new float[12];
			for (int k = 0; k < 9; k++) fit[k] = turn[k] * scale;
			fit[9] = shift[0]; fit[10] = shift[1]; fit[11] = shift[2];
			result.Scale = scale;
			result.Offset = shift;
			result.Notes.Add("fitted at scale " + scale.ToString("0.###") + (rotation != null ? ", turned " + string.Join("/", r) : "") + ", offset " + shift[0].ToString("0.##") + ", " + shift[1].ToString("0.##") + ", " + shift[2].ToString("0.##") + " (the file " + (tMax[0] - tMin[0]).ToString("0.##") + " x " + fileHeight.ToString("0.##") + " x " + (tMax[2] - tMin[2]).ToString("0.##") + ", the model " + (oMax[0] - oMin[0]).ToString("0.#") + " x " + (oMax[1] - oMin[1]).ToString("0.#") + " x " + (oMax[2] - oMin[2]).ToString("0.#") + ")");

			// ---- the bundle: the file's meshes in the model's space -----------------------------------------------
			ModelBundle bundle = new ModelBundle { Name = original.Name, Nodes = rig.Nodes, Buffer = new List<float>(), Indices = new List<int>(), Groups = new List<ModelGroup>() };
			List<float> normals = new List<float>();
			bool anyNormals = meshes.All(m => m.Normals != null);
			Dictionary<int, string> textureNames = new Dictionary<int, string>();
			Dictionary<string, byte[]> textureBytes = new Dictionary<string, byte[]>(StringComparer.Ordinal);
			foreach (GltfMesh mesh in meshes)
			{
				GltfMaterial material = mesh.Material >= 0 && mesh.Material < file.Materials.Count ? file.Materials[mesh.Material] : new GltfMaterial();
				string texture = null;
				if (material.Image >= 0 && material.Image < file.Images.Count && file.Images[material.Image].Bytes != null)
				{
					if (!textureNames.TryGetValue(material.Image, out texture))
					{
						texture = original.Name + "_" + material.Image;
						textureNames[material.Image] = texture;
						textureBytes[texture] = file.Images[material.Image].Bytes;
					}
				}
				int first = bundle.Buffer.Count / 8;
				for (int v = 0; v < mesh.VertexCount; v++)
				{
					float[] p = Apply(fit, mesh.Positions[v * 3], mesh.Positions[v * 3 + 1], mesh.Positions[v * 3 + 2]);
					bundle.Buffer.Add(p[0]); bundle.Buffer.Add(p[1]); bundle.Buffer.Add(p[2]);
					bundle.Buffer.Add(mesh.Uvs != null ? mesh.Uvs[v * 2] : 0.5f); bundle.Buffer.Add(mesh.Uvs != null ? mesh.Uvs[v * 2 + 1] : 0.5f);
					float cr = 1f, cg = 1f, cb = 1f;
					if (mesh.Colours != null) { cr = mesh.Colours[v * 4]; cg = mesh.Colours[v * 4 + 1]; cb = mesh.Colours[v * 4 + 2]; }
					bundle.Buffer.Add(cr); bundle.Buffer.Add(cg); bundle.Buffer.Add(cb);
					bundle.MatrixIndex.Add(0);
					if (anyNormals)
					{
						float[] n = Turn(turn, mesh.Normals[v * 3], mesh.Normals[v * 3 + 1], mesh.Normals[v * 3 + 2]);
						normals.Add(n[0]); normals.Add(n[1]); normals.Add(n[2]);
					}
				}
				ModelGroup group = new ModelGroup
				{
					Shape = mesh.Node ?? ("mesh" + bundle.Groups.Count), Node = rig.Nodes.Count > 0 ? rig.Nodes[0] : "root", Material = material.Name ?? ("material" + bundle.Groups.Count),
					Texture = texture, Start = bundle.Indices.Count, Count = mesh.Indices.Length,
					Colour = ((int)Math.Round(Math.Clamp(material.BaseColour[0], 0, 1) * 255) << 16) | ((int)Math.Round(Math.Clamp(material.BaseColour[1], 0, 1) * 255) << 8) | (int)Math.Round(Math.Clamp(material.BaseColour[2], 0, 1) * 255),
					Alpha = material.BaseColour[3], Translucent = material.Blend, Matrices = new List<float[]> { Identity() }, Slots = new List<int>(), Piece = bundle.Groups.Count
				};
				foreach (int index in mesh.Indices) bundle.Indices.Add(first + index);
				bundle.Groups.Add(group);
				result.Triangles += mesh.Indices.Length / 3;
			}
			int count = bundle.Buffer.Count / 8;
			result.Vertices = count;

			// ---- the weights: the original's surface -------------------------------------------------------------
			// For each vertex, the nearest points on the original's triangles - the three nearest
			// triangles, the nearest counting far more (inverse fourth power, softened by a hundredth
			// of the height) - and at each such point the three corners' weights blended by where the
			// point lies (Blender's "nearest face interpolated"). A surface, not a scatter of 772
			// points, so a vertex between the original's arm and torso vertices takes the side it is on.
			List<(int A, int B, int C)> triangles = new List<(int, int, int)>();
			for (int g = 0; g < original.Groups.Count; g++)
			{
				ModelGroup group = original.Groups[g];
				if (group.Hidden) continue;
				for (int i = group.Start; i + 2 < group.Start + group.Count; i += 3)
				{
					int a = original.Indices[i], b = original.Indices[i + 1], c = original.Indices[i + 2];
					if (a < originalCount && b < originalCount && c < originalCount) triangles.Add((a, b, c));
				}
			}
			const int Nearest = 3;
			float soft = (oMax[1] - oMin[1]) * 0.01f; soft *= soft;
			Dictionary<int, float>[] weights = new Dictionary<int, float>[count];
			float[] nearDist = new float[Nearest];
			int[] nearTri = new int[Nearest];
			float[] nearU = new float[Nearest], nearV = new float[Nearest];
			for (int v = 0; v < count; v++)
			{
				float px = bundle.Buffer[v * 8], py = bundle.Buffer[v * 8 + 1], pz = bundle.Buffer[v * 8 + 2];
				for (int k = 0; k < Nearest; k++) { nearDist[k] = float.MaxValue; nearTri[k] = -1; }
				for (int t = 0; t < triangles.Count; t++)
				{
					(int a, int b, int c) = triangles[t];
					float d = ClosestOnTriangle(px, py, pz, refX[a], refY[a], refZ[a], refX[b], refY[b], refZ[b], refX[c], refY[c], refZ[c], out float u, out float w);
					if (d >= nearDist[Nearest - 1]) continue;
					int at = Nearest - 1;
					while (at > 0 && nearDist[at - 1] > d) { nearDist[at] = nearDist[at - 1]; nearTri[at] = nearTri[at - 1]; nearU[at] = nearU[at - 1]; nearV[at] = nearV[at - 1]; at--; }
					nearDist[at] = d; nearTri[at] = t; nearU[at] = u; nearV[at] = w;
				}
				Dictionary<int, float> blend = new Dictionary<int, float>();
				for (int k = 0; k < Nearest; k++)
				{
					if (nearTri[k] < 0) continue;
					float influence = 1f / ((nearDist[k] + soft) * (nearDist[k] + soft));
					(int a, int b, int c) = triangles[nearTri[k]];
					float u = nearU[k], w = nearV[k];
					AddWeights(blend, refWeights[a], influence * (1 - u - w));
					AddWeights(blend, refWeights[b], influence * u);
					AddWeights(blend, refWeights[c], influence * w);
				}
				weights[v] = blend;
			}

			// ---- smoothed over the mesh --------------------------------------------------------------------------
			// Neighbours by shared position, so a UV seam's twin vertices smooth as one.
			Dictionary<(int, int, int), int> cells = new Dictionary<(int, int, int), int>();
			int[] canonical = new int[count];
			for (int v = 0; v < count; v++)
			{
				(int, int, int) key = ((int)Math.Round(bundle.Buffer[v * 8] * 2000), (int)Math.Round(bundle.Buffer[v * 8 + 1] * 2000), (int)Math.Round(bundle.Buffer[v * 8 + 2] * 2000));
				if (!cells.TryGetValue(key, out int c)) { cells[key] = c = v; }
				canonical[v] = c;
			}
			HashSet<int>[] neighbours = new HashSet<int>[count];
			for (int i = 0; i + 2 < bundle.Indices.Count; i += 3)
			{
				int a = canonical[bundle.Indices[i]], b = canonical[bundle.Indices[i + 1]], c = canonical[bundle.Indices[i + 2]];
				Link(neighbours, a, b); Link(neighbours, b, c); Link(neighbours, a, c);
			}
			for (int pass = 0; pass < 1; pass++)
			{
				Dictionary<int, float>[] next = new Dictionary<int, float>[count];
				for (int v = 0; v < count; v++)
				{
					int c = canonical[v];
					if (c != v) continue;
					Dictionary<int, float> mine = Normalised(weights[c]);
					Dictionary<int, float> sum = new Dictionary<int, float>(mine);
					int n = 1;
					if (neighbours[c] != null)
						foreach (int other in neighbours[c])
						{
							foreach (KeyValuePair<int, float> pair in Normalised(weights[other])) sum[pair.Key] = (sum.TryGetValue(pair.Key, out float have) ? have : 0) + pair.Value;
							n++;
						}
					Dictionary<int, float> blended = new Dictionary<int, float>();
					foreach (KeyValuePair<int, float> pair in sum) blended[pair.Key] = 0.5f * (mine.TryGetValue(pair.Key, out float own) ? own : 0) + 0.5f * pair.Value / n;
					next[c] = blended;
				}
				for (int v = 0; v < count; v++) weights[v] = next[canonical[v]];
			}

			// Top four, summing to one; the bones in use counted.
			(int Node, float Weight)[][] final = new (int, float)[count][];
			HashSet<int> bonesUsed = new HashSet<int>();
			for (int v = 0; v < count; v++)
			{
				final[v] = Normalised(weights[v]).OrderByDescending(p => p.Value).Take(4).Select(p => (p.Key, p.Value)).ToArray();
				float total = final[v].Sum(p => p.Weight);
				if (total > 0) final[v] = final[v].Select(p => (p.Node, p.Weight / total)).ToArray();
				foreach ((int node, float _) in final[v]) bonesUsed.Add(node);
			}
			result.Bones = bonesUsed.Count;
			result.Notes.Add(result.Vertices.ToString("N0") + " vertices weighted from the original's " + originalCount.ToString("N0") + " to " + bonesUsed.Count + " bones: " + string.Join(", ", bonesUsed.OrderBy(b => b).Select(b => rig.Nodes[b])));

			// ---- back into the bind pose ---------------------------------------------------------------------------
			// The file was matched to the original posed; the skin's rest is the bind pose, so each
			// vertex goes back through the inverse of its bones' changes, blended by its new weights
			// (the inverse of a blend, taken bone by bone - exact where one bone rules, near enough at
			// the joints where the pose itself is a blend).
			if (delta != null)
			{
				float[][] undo = delta.Select(d => Gltf.Invert(d)).ToArray();
				for (int v = 0; v < count; v++)
				{
					float x = bundle.Buffer[v * 8], y = bundle.Buffer[v * 8 + 1], z = bundle.Buffer[v * 8 + 2];
					float[] p = Skinned(x, y, z, final[v], undo);
					bundle.Buffer[v * 8] = p[0]; bundle.Buffer[v * 8 + 1] = p[1]; bundle.Buffer[v * 8 + 2] = p[2];
					if (anyNormals)
					{
						float nx = 0, ny = 0, nz = 0;
						foreach ((int node, float weight) in final[v])
						{
							float[] n = Turn(undo[node], normals[v * 3], normals[v * 3 + 1], normals[v * 3 + 2]);
							nx += n[0] * weight; ny += n[1] * weight; nz += n[2] * weight;
						}
						float len = MathF.Sqrt(nx * nx + ny * ny + nz * nz);
						if (len > 1e-6f) { normals[v * 3] = nx / len; normals[v * 3 + 1] = ny / len; normals[v * 3 + 2] = nz / len; }
					}
				}
			}

			// ---- the skinned glTF -------------------------------------------------------------------------------
			Models.Frame(bundle);
			result.Glb = Gltf.Write(bundle, name => textureBytes.TryGetValue(name, out byte[] bytes) ? bytes : null, rig, v => final[v], anyNormals ? normals.ToArray() : null);
			return result;
		}

		/// <summary>A point through a blend of the nodes' matrices, weighted.</summary>
		private static float[] Skinned(float x, float y, float z, (int Node, float Weight)[] weights, float[][] matrices)
		{
			float[] p = new float[3];
			float total = 0;
			foreach ((int node, float weight) in weights)
			{
				if (node < 0 || node >= matrices.Length || matrices[node] == null || weight <= 0) continue;
				float[] q = Apply(matrices[node], x, y, z);
				p[0] += q[0] * weight; p[1] += q[1] * weight; p[2] += q[2] * weight;
				total += weight;
			}
			if (total <= 0) return new[] { x, y, z };
			if (Math.Abs(total - 1f) > 1e-4f) { p[0] /= total; p[1] /= total; p[2] /= total; }
			return p;
		}

		private static void AddWeights(Dictionary<int, float> into, (int Node, float Weight)[] weights, float factor)
		{
			if (factor <= 0) return;
			foreach ((int node, float weight) in weights) into[node] = (into.TryGetValue(node, out float have) ? have : 0) + factor * weight;
		}

		/// <summary>
		/// The squared distance from p to the nearest point of triangle abc, and that point's
		/// barycentric coordinates (u along ab, w along ac) - Ericson's closest-point-on-triangle.
		/// </summary>
		private static float ClosestOnTriangle(float px, float py, float pz, float ax, float ay, float az, float bx, float by, float bz, float cx, float cy, float cz, out float u, out float w)
		{
			float abx = bx - ax, aby = by - ay, abz = bz - az;
			float acx = cx - ax, acy = cy - ay, acz = cz - az;
			float apx = px - ax, apy = py - ay, apz = pz - az;
			float d1 = abx * apx + aby * apy + abz * apz, d2 = acx * apx + acy * apy + acz * apz;
			if (d1 <= 0 && d2 <= 0) { u = 0; w = 0; return apx * apx + apy * apy + apz * apz; }
			float bpx = px - bx, bpy = py - by, bpz = pz - bz;
			float d3 = abx * bpx + aby * bpy + abz * bpz, d4 = acx * bpx + acy * bpy + acz * bpz;
			if (d3 >= 0 && d4 <= d3) { u = 1; w = 0; return bpx * bpx + bpy * bpy + bpz * bpz; }
			float vc = d1 * d4 - d3 * d2;
			if (vc <= 0 && d1 >= 0 && d3 <= 0)
			{
				u = d1 / (d1 - d3); w = 0;
				return Sq(apx - abx * u) + Sq(apy - aby * u) + Sq(apz - abz * u);
			}
			float cpx = px - cx, cpy = py - cy, cpz = pz - cz;
			float d5 = abx * cpx + aby * cpy + abz * cpz, d6 = acx * cpx + acy * cpy + acz * cpz;
			if (d6 >= 0 && d5 <= d6) { u = 0; w = 1; return cpx * cpx + cpy * cpy + cpz * cpz; }
			float vb = d5 * d2 - d1 * d6;
			if (vb <= 0 && d2 >= 0 && d6 <= 0)
			{
				u = 0; w = d2 / (d2 - d6);
				return Sq(apx - acx * w) + Sq(apy - acy * w) + Sq(apz - acz * w);
			}
			float va = d3 * d6 - d5 * d4;
			if (va <= 0 && d4 - d3 >= 0 && d5 - d6 >= 0)
			{
				float t = (d4 - d3) / ((d4 - d3) + (d5 - d6));
				u = 1 - t; w = t;
				float qx = bx + (cx - bx) * t, qy = by + (cy - by) * t, qz = bz + (cz - bz) * t;
				return Sq(px - qx) + Sq(py - qy) + Sq(pz - qz);
			}
			float denom = 1f / (va + vb + vc);
			u = vb * denom; w = vc * denom;
			float rx = ax + abx * u + acx * w, ry = ay + aby * u + acy * w, rz = az + abz * u + acz * w;
			return Sq(px - rx) + Sq(py - ry) + Sq(pz - rz);
		}

		private static float Sq(float v) => v * v;

		private static void Link(HashSet<int>[] neighbours, int a, int b)
		{
			if (a == b) return;
			(neighbours[a] ??= new HashSet<int>()).Add(b);
			(neighbours[b] ??= new HashSet<int>()).Add(a);
		}

		private static Dictionary<int, float> Normalised(Dictionary<int, float> w)
		{
			float total = w.Values.Sum();
			Dictionary<int, float> n = new Dictionary<int, float>();
			foreach (KeyValuePair<int, float> pair in w) n[pair.Key] = total > 0 ? pair.Value / total : 0;
			return n;
		}

		private static float[] Identity() => new[] { 1f, 0, 0, 0, 1f, 0, 0, 0, 1f, 0, 0, 0 };

		/// <summary>Turns about x, then y, then z, in degrees, as a row-vector 4x3.</summary>
		private static float[] Rotation(float[] r)
		{
			float[] m = Identity();
			for (int axis = 0; axis < 3; axis++)
			{
				if (Math.Abs(r[axis]) < 0.0001f) continue;
				float a = r[axis] * MathF.PI / 180, c = MathF.Cos(a), s = MathF.Sin(a);
				// Row-vector rotation matrices (v x R).
				float[] rot = axis == 0 ? new[] { 1, 0, 0, 0, c, s, 0, -s, c } : axis == 1 ? new[] { c, 0, -s, 0, 1, 0, s, 0, c } : new[] { c, s, 0, -s, c, 0, 0, 0, 1 };
				float[] next = new float[12];
				for (int row = 0; row < 3; row++)
					for (int col = 0; col < 3; col++)
						next[row * 3 + col] = m[row * 3] * rot[col] + m[row * 3 + 1] * rot[3 + col] + m[row * 3 + 2] * rot[6 + col];
				m = next;
			}
			return m;
		}

		private static float[] Apply(float[] m, float x, float y, float z) => new[]
		{
			x * m[0] + y * m[3] + z * m[6] + m[9],
			x * m[1] + y * m[4] + z * m[7] + m[10],
			x * m[2] + y * m[5] + z * m[8] + m[11]
		};

		private static float[] Turn(float[] m, float x, float y, float z)
		{
			float[] n = { x * m[0] + y * m[3] + z * m[6], x * m[1] + y * m[4] + z * m[7], x * m[2] + y * m[5] + z * m[8] };
			float len = MathF.Sqrt(n[0] * n[0] + n[1] * n[1] + n[2] * n[2]);
			if (len > 1e-6f) { n[0] /= len; n[1] /= len; n[2] /= len; }
			return n;
		}
	}
}
