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
			// The reference as posed (or the bind pose), and its box; the bind pose kept beside it.
			float[] bindX = (float[])refX.Clone(), bindY = (float[])refY.Clone(), bindZ = (float[])refZ.Clone();
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
			float soft = (oMax[1] - oMin[1]) * 0.01f; soft *= soft;

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

			// The file's vertices as fitted (the pose the author left them in), kept for the un-posing.
			float[] fileX = new float[count], fileY = new float[count], fileZ = new float[count];
			for (int v = 0; v < count; v++) { fileX[v] = bundle.Buffer[v * 8]; fileY[v] = bundle.Buffer[v * 8 + 1]; fileZ[v] = bundle.Buffer[v * 8 + 2]; }

			// ---- regions: the head, the arms, the rest ------------------------------------------------------------
			// Nearest surface cannot tell a chin from a chest: a big-headed file's lower face sits at
			// the height of the original's chest and neck, its baggy sleeves beside its belly, and they
			// would take those bones. So the file is cut by its own shape - the head above the neck (the
			// narrowest slice between the head's widest and the shoulders'), the arms outside the torso's
			// width - and each region takes weights only from the like region of the original: the head
			// from its head's triangles, a sleeve from its arm's. Regions the file has no cut for take
			// from the whole.
			bool leftIsPlusX = LeftIsPlusX(bindX, refWeights, rig);
			float armFloor = float.MaxValue;
			for (int n = 0; n < rig.Nodes.Count; n++)
			{
				string name = rig.Nodes[n];
				if (name != "L_te" && name != "R_te" && name != "kosi") continue;
				float[] world = delta != null ? Gltf.Mul(rig.Bind[n], delta[n]) : rig.Bind[n];
				armFloor = Math.Min(armFloor, world[10]);
			}
			if (armFloor == float.MaxValue) armFloor = oMin[1] + (oMax[1] - oMin[1]) * 0.4f;
			int[] vertexRegion = Regions(fileX, fileY, count, oMin[1], oMax[1], armFloor - (oMax[1] - oMin[1]) * 0.1f, leftIsPlusX, result.Notes);
			int[] triangleRegion = TriangleRegions(triangles, refWeights, rig);
			int[] boneRegion = rig.Nodes.Select(RegionOfBone).ToArray();

			// ---- pass one: against the original in the matched pose ----------------------------------------------
			(int Node, float Weight)[][] final = Finish(Smooth(Transfer(fileX, fileY, fileZ, count, refX, refY, refZ, triangles, refWeights, soft, vertexRegion, triangleRegion, boneRegion), neighbours, canonical), neighbours, canonical);

			// ---- the limbs onto the bones --------------------------------------------------------------------------
			// The matched pose is alike as a whole, not limb by limb: the file's forearm hangs straight
			// where the game's angles forward, and carried through the game's changes that difference
			// would stay as a constant turn of the forearm off its bone, in every motion. So each limb's
			// vertices are first turned about their bone's joint (as posed) onto the bone's direction -
			// the mesh limb's direction taken as joint -> the middle of the vertices the bone owns -
			// blended by the vertex's weight for the bone. Hubs (hips, chest) are left as they are.
			if (delta != null)
			{
				float[][] posedWorld = new float[rig.Nodes.Count][];
				for (int n = 0; n < rig.Nodes.Count; n++) posedWorld[n] = Gltf.Mul(rig.Bind[n], delta[n]);
				float[][] turnAbout = new float[rig.Nodes.Count][];
				int turned = 0;
				List<string> turnedNames = new List<string>();
				float modelHeight = oMax[1] - oMin[1];
				for (int n = 0; n < rig.Nodes.Count; n++)
				{
					// Arms differ most between rest poses (hanging, forward, out) and are long and thin, so
					// their middle is a fair direction; legs stand much alike, so a big turn there is not
					// the limb hanging differently but the mesh's proportions (a chibi's stub legs), and is
					// left alone. Feet point forward off the shin, and a head's middle is where its hair is.
					int limb = LimbKind(rig.Nodes[n]);
					if (limb == 0) continue;
					float cap = limb == 1 ? 90 : 35;
					float[] joint = { posedWorld[n][9], posedWorld[n][10], posedWorld[n][11] };
					// The game bone's direction, posed: to its first child, else from its parent.
					float[] boneDir = null;
					for (int c = 0; c < rig.Nodes.Count && boneDir == null; c++) if (rig.Parents[c] == n) boneDir = new[] { posedWorld[c][9] - joint[0], posedWorld[c][10] - joint[1], posedWorld[c][11] - joint[2] };
					if (boneDir == null && rig.Parents[n] >= 0) boneDir = new[] { joint[0] - posedWorld[rig.Parents[n]][9], joint[1] - posedWorld[rig.Parents[n]][10], joint[2] - posedWorld[rig.Parents[n]][11] };
					// Too short a bone has no direction worth the name (the game's knee sits on its shin).
					if (boneDir == null || Length(boneDir) < 0.04f * modelHeight) continue;
					// The mesh limb's: the joint to the middle of the vertices this bone rules.
					float[] middle = new float[3]; int owned = 0;
					for (int v = 0; v < count; v++)
					{
						(int Node, float Weight)[] w = final[v];
						if (w.Length == 0 || w[0].Node != n || w[0].Weight < 0.5f) continue;
						middle[0] += fileX[v]; middle[1] += fileY[v]; middle[2] += fileZ[v]; owned++;
					}
					if (owned < 8) continue;
					float[] meshDir = { middle[0] / owned - joint[0], middle[1] / owned - joint[1], middle[2] / owned - joint[2] };
					// The middle right by the joint says nothing about direction; a turn past a right angle
					// is not a limb hanging differently but something else owning those vertices.
					if (Length(meshDir) < 0.25f * Length(boneDir)) continue;
					float[] a = Unit(meshDir), b = Unit(boneDir);
					float degrees = MathF.Acos(Math.Max(-1f, Math.Min(1f, a[0] * b[0] + a[1] * b[1] + a[2] * b[2]))) * 180f / MathF.PI;
					if (degrees > cap) { result.Notes.Add(rig.Nodes[n] + " left alone: its " + owned + " vertices lie " + degrees.ToString("0") + "\u00b0 off the bone"); continue; }
					turnAbout[n] = RotationBetween(a, b, joint);
					turnedNames.Add(rig.Nodes[n] + " " + degrees.ToString("0") + "\u00b0");
					turned++;
				}
				if (turned > 0)
				{
					float[] ax = new float[count], ay = new float[count], az = new float[count];
					for (int v = 0; v < count; v++)
					{
						float[] p = { fileX[v], fileY[v], fileZ[v] };
						float[] sum = new float[3]; float total = 0;
						foreach ((int node, float weight) in final[v])
						{
							float[] q = turnAbout[node] != null ? Apply(turnAbout[node], p[0], p[1], p[2]) : p;
							sum[0] += q[0] * weight; sum[1] += q[1] * weight; sum[2] += q[2] * weight; total += weight;
						}
						if (total > 0) { ax[v] = sum[0] / total; ay[v] = sum[1] / total; az[v] = sum[2] / total; } else { ax[v] = p[0]; ay[v] = p[1]; az[v] = p[2]; }
					}
					fileX = ax; fileY = ay; fileZ = az;
					result.Notes.Add(turned + " limb bones had the mesh turned onto them before the carry into the bind pose: " + string.Join(", ", turnedNames));
				}
			}

			// ---- into the bind pose, and pass two against the original's bind pose -----------------------------------
			// In the matched pose the original's arms hang against its body, and a vertex on the side
			// of the chest is as near the arm's surface as the chest's - which is where a spike out of
			// the chest and a sleeve that stays behind come from. Carried into the T-pose bind with
			// the first weights, the arms stand clear of the body, and a second transfer there, against
			// the original's own bind pose, tells them apart. Twice, since the second carrying is truer.
			float[] bx = fileX, by = fileY, bz = fileZ;
			if (delta != null)
			{
				float[][] undo = delta.Select(d => Gltf.Invert(d)).ToArray();
				for (int round = 0; round < 2; round++)
				{
					bx = new float[count]; by = new float[count]; bz = new float[count];
					for (int v = 0; v < count; v++)
					{
						float[] p = Skinned(fileX[v], fileY[v], fileZ[v], final[v], undo);
						bx[v] = p[0]; by[v] = p[1]; bz[v] = p[2];
					}
					final = Finish(Smooth(Transfer(bx, by, bz, count, bindX, bindY, bindZ, triangles, refWeights, soft, vertexRegion, triangleRegion, boneRegion), neighbours, canonical), neighbours, canonical);
				}
				// The final carrying, with the final weights; the normals go the same way.
				bx = new float[count]; by = new float[count]; bz = new float[count];
				for (int v = 0; v < count; v++)
				{
					float[] p = Skinned(fileX[v], fileY[v], fileZ[v], final[v], undo);
					bx[v] = p[0]; by[v] = p[1]; bz[v] = p[2];
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
			HashSet<int> bonesUsed = new HashSet<int>();
			for (int v = 0; v < count; v++) foreach ((int node, float _) in final[v]) bonesUsed.Add(node);
			result.Bones = bonesUsed.Count;
			result.Notes.Add(result.Vertices.ToString("N0") + " vertices weighted from the original's " + originalCount.ToString("N0") + " to " + bonesUsed.Count + " bones: " + string.Join(", ", bonesUsed.OrderBy(b => b).Select(b => rig.Nodes[b])) + (delta != null ? " - a first pass in the matched pose, then two in the bind pose" : ""));

			// ---- the skinned glTF -------------------------------------------------------------------------------
			Models.Frame(bundle);
			result.Glb = Gltf.Write(bundle, name => textureBytes.TryGetValue(name, out byte[] bytes) ? bytes : null, rig, v => final[v], anyNormals ? normals.ToArray() : null);
			return result;
		}

		/// <summary>
		/// Weights for every point from the nearest points of the reference surface: the three
		/// nearest triangles, the nearest counting far more (inverse fourth power, softened), the
		/// corners' weights blended by where the nearest point lies on each.
		/// </summary>
		private static Dictionary<int, float>[] Transfer(float[] px, float[] py, float[] pz, int count, float[] refX, float[] refY, float[] refZ, List<(int A, int B, int C)> triangles, (int Node, float Weight)[][] refWeights, float soft, int[] vertexRegion, int[] triangleRegion, int[] boneRegion)
		{
			const int Nearest = 3;
			// A vertex of a region takes only from that region's triangles, when the original has any.
			bool[] regionHasTriangles = new bool[4];
			foreach (int r in triangleRegion) if (r >= 0) regionHasTriangles[r] = true;
			Dictionary<int, float>[] weights = new Dictionary<int, float>[count];
			float[] nearDist = new float[Nearest];
			int[] nearTri = new int[Nearest];
			float[] nearU = new float[Nearest], nearV = new float[Nearest];
			for (int v = 0; v < count; v++)
			{
				for (int k = 0; k < Nearest; k++) { nearDist[k] = float.MaxValue; nearTri[k] = -1; }
				int only = vertexRegion[v] >= 0 && regionHasTriangles[vertexRegion[v]] ? vertexRegion[v] : RegionAny;
				for (int t = 0; t < triangles.Count; t++)
				{
					if (only != RegionAny && triangleRegion[t] != only) continue;
					(int a, int b, int c) = triangles[t];
					float d = ClosestOnTriangle(px[v], py[v], pz[v], refX[a], refY[a], refZ[a], refX[b], refY[b], refZ[b], refX[c], refY[c], refZ[c], out float u, out float w);
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
				// Strictly the region's bones: what the original's triangles blend in from outside it
				// (the neck under its jaw, the chest under its shoulder) goes to the heaviest bone of
				// the region. The smoothing after still blends across the cuts.
				if (only != RegionAny)
				{
					int heaviest = -1; float most = 0, outside = 0;
					foreach (KeyValuePair<int, float> pair in blend)
					{
						if (boneRegion[pair.Key] == only) { if (pair.Value > most) { most = pair.Value; heaviest = pair.Key; } }
						else outside += pair.Value;
					}
					if (heaviest >= 0 && outside > 0)
					{
						foreach (int bone in blend.Keys.Where(k => boneRegion[k] != only).ToList()) blend.Remove(bone);
						blend[heaviest] += outside;
					}
				}
				weights[v] = blend;
			}
			return weights;
		}

		/// <summary>
		/// One smoothing pass over the mesh (each vertex half its own weights, half its neighbours'
		/// mean), after a vote that hands a vertex its neighbours' weights outright when its heaviest
		/// bone is one that fewer than a third of them have at all - a lone vertex the transfer put on
		/// the wrong bone, the spike in a pose.
		/// </summary>
		private static Dictionary<int, float>[] Smooth(Dictionary<int, float>[] weights, HashSet<int>[] neighbours, int[] canonical)
		{
			int count = weights.Length;
			Dictionary<int, float>[] voted = new Dictionary<int, float>[count];
			for (int v = 0; v < count; v++)
			{
				int c = canonical[v];
				if (c != v) continue;
				Dictionary<int, float> mine = Normalised(weights[c]);
				voted[c] = mine;
				if (neighbours[c] == null || neighbours[c].Count < 3 || mine.Count == 0) continue;
				int heaviest = mine.OrderByDescending(p => p.Value).First().Key;
				int agree = 0;
				Dictionary<int, float> around = new Dictionary<int, float>();
				foreach (int other in neighbours[c])
				{
					Dictionary<int, float> theirs = Normalised(weights[other]);
					if (theirs.TryGetValue(heaviest, out float w) && w > 0.05f) agree++;
					foreach (KeyValuePair<int, float> pair in theirs) around[pair.Key] = (around.TryGetValue(pair.Key, out float have) ? have : 0) + pair.Value / neighbours[c].Count;
				}
				if (agree * 3 < neighbours[c].Count) voted[c] = around;
			}
			for (int v = 0; v < count; v++) voted[v] = voted[canonical[v]];

			Dictionary<int, float>[] next = new Dictionary<int, float>[count];
			for (int v = 0; v < count; v++)
			{
				int c = canonical[v];
				if (c != v) continue;
				Dictionary<int, float> mine = voted[c];
				Dictionary<int, float> sum = new Dictionary<int, float>(mine);
				int n = 1;
				if (neighbours[c] != null)
					foreach (int other in neighbours[c])
					{
						foreach (KeyValuePair<int, float> pair in voted[other]) sum[pair.Key] = (sum.TryGetValue(pair.Key, out float have) ? have : 0) + pair.Value;
						n++;
					}
				Dictionary<int, float> blended = new Dictionary<int, float>();
				foreach (KeyValuePair<int, float> pair in sum) blended[pair.Key] = 0.5f * (mine.TryGetValue(pair.Key, out float own) ? own : 0) + 0.5f * pair.Value / n;
				next[c] = blended;
			}
			for (int v = 0; v < count; v++) next[v] = next[canonical[v]];
			return next;
		}

		/// <summary>Each vertex's four heaviest, summing to one.</summary>
		private static (int Node, float Weight)[][] Finish(Dictionary<int, float>[] weights, HashSet<int>[] neighbours, int[] canonical)
		{
			(int Node, float Weight)[][] final = new (int, float)[weights.Length][];
			for (int v = 0; v < weights.Length; v++)
			{
				final[v] = Normalised(weights[v]).OrderByDescending(p => p.Value).Take(4).Select(p => (p.Key, p.Value)).ToArray();
				float total = final[v].Sum(p => p.Weight);
				if (total > 0) final[v] = final[v].Select(p => (p.Node, p.Weight / total)).ToArray();
				else final[v] = new[] { (0, 1f) };
			}
			return final;
		}

		// ---- regions ------------------------------------------------------------------------------------------
		private const int RegionAny = -1, RegionBody = 0, RegionHead = 1, RegionLeftArm = 2, RegionRightArm = 3;

		/// <summary>The region a game bone belongs to: the head (atama), an arm (sakotu, kata, ude, te by side), else the body.</summary>
		private static int RegionOfBone(string node)
		{
			if (node == "atama") return RegionHead;
			bool left = node.StartsWith("L_", StringComparison.Ordinal), right = node.StartsWith("R_", StringComparison.Ordinal);
			string n = left || right ? node.Substring(2) : node;
			if ((left || right) && (n == "sakotu" || n == "kata" || n == "ude" || n == "te")) return left ? RegionLeftArm : RegionRightArm;
			return RegionBody;
		}

		private static int HeaviestBone((int Node, float Weight)[] weights)
		{
			int best = -1; float most = 0;
			foreach ((int node, float weight) in weights) if (weight > most) { most = weight; best = node; }
			return best;
		}

		/// <summary>Whether the original's left side lies at +x: its L_ bones' vertices against its R_ ones, in the bind pose.</summary>
		private static bool LeftIsPlusX(float[] bindX, (int Node, float Weight)[][] refWeights, Models.Rig rig)
		{
			double left = 0, right = 0; int nl = 0, nr = 0;
			for (int v = 0; v < refWeights.Length; v++)
			{
				int bone = HeaviestBone(refWeights[v]);
				if (bone < 0) continue;
				if (rig.Nodes[bone].StartsWith("L_", StringComparison.Ordinal)) { left += bindX[v]; nl++; }
				else if (rig.Nodes[bone].StartsWith("R_", StringComparison.Ordinal)) { right += bindX[v]; nr++; }
			}
			return nl == 0 || nr == 0 || left / nl > right / nr;
		}

		/// <summary>Each of the original's triangles' region: what two of its corners' heaviest bones agree on, else the first's.</summary>
		private static int[] TriangleRegions(List<(int A, int B, int C)> triangles, (int Node, float Weight)[][] refWeights, Models.Rig rig)
		{
			int[] region = new int[triangles.Count];
			for (int t = 0; t < triangles.Count; t++)
			{
				(int a, int b, int c) = triangles[t];
				int ra = RegionOf(a), rb = RegionOf(b), rc = RegionOf(c);
				region[t] = rb == rc ? rb : ra;
			}
			return region;
			int RegionOf(int v) { int bone = HeaviestBone(refWeights[v]); return bone < 0 ? RegionBody : RegionOfBone(rig.Nodes[bone]); }
		}

		/// <summary>
		/// Each file vertex's region by the file's own shape (in the model's space, standing up y): the
		/// head above the neck - the narrowest horizontal slice between the head's widest and the
		/// shoulders' - and, between the neck and <paramref name="armFloor"/>, an arm where a vertex
		/// lies outside the torso's width in its slice (the torso being the cluster of a slice's
		/// vertices about the middle, the arms the clusters a gap away from it). No neck found: every
		/// vertex takes from the whole original, as before.
		/// </summary>
		private static int[] Regions(float[] x, float[] y, int count, float yMin, float yMax, float armFloor, bool leftIsPlusX, List<string> notes)
		{
			int[] region = new int[count];
			float h = Math.Max(1e-4f, yMax - yMin);
			const int Slices = 64;
			int SliceOf(float at) => Math.Max(0, Math.Min(Slices - 1, (int)((at - yMin) / h * Slices)));
			float[] sMin = Enumerable.Repeat(float.MaxValue, Slices).ToArray(), sMax = Enumerable.Repeat(float.MinValue, Slices).ToArray();
			double centre = 0;
			for (int v = 0; v < count; v++) { int s = SliceOf(y[v]); sMin[s] = Math.Min(sMin[s], x[v]); sMax[s] = Math.Max(sMax[s], x[v]); centre += x[v]; }
			centre /= Math.Max(1, count);
			float[] wide = new float[Slices];
			for (int s = 0; s < Slices; s++) wide[s] = sMax[s] > sMin[s] ? sMax[s] - sMin[s] : -1;

			// The neck: the deepest valley in width across the top part, against the widest above and below it.
			int neck = -1; float bestDepth = 0;
			for (int s = (int)(Slices * 0.40); s < (int)(Slices * 0.96); s++)
			{
				if (wide[s] < 0) continue;
				float above = 0, below = 0;
				for (int t = s + 1; t < Slices; t++) above = Math.Max(above, wide[t]);
				for (int t = (int)(Slices * 0.30); t < s; t++) below = Math.Max(below, wide[t]);
				float cap = Math.Min(above, below);
				float depth = cap - wide[s];
				if (depth > bestDepth && depth >= 0.2f * cap) { bestDepth = depth; neck = s; }
			}
			if (neck < 0)
			{
				for (int v = 0; v < count; v++) region[v] = RegionAny;
				notes.Add("no neck found in the file's shape, so no head or arm regions: weights taken from the whole of the original");
				return region;
			}
			float neckY = yMin + (neck + 0.5f) / Slices * h;
			int heads = 0;
			for (int v = 0; v < count; v++) if (y[v] > neckY) { region[v] = RegionHead; heads++; }

			// The arms: in each slice between the neck and the floor, the vertices clustered by gaps in
			// x; the cluster about the middle is the torso, and a vertex beyond it either way an arm.
			// A slice whose arms touch the torso (one cluster) borrows the torso's width from the others.
			int floor = SliceOf(armFloor);
			float[] torsoMin = new float[Slices], torsoMax = new float[Slices];
			bool[] known = new bool[Slices];
			List<float>[] xs = new List<float>[Slices];
			for (int v = 0; v < count; v++) { int s = SliceOf(y[v]); if (s >= floor && s <= neck) (xs[s] ??= new List<float>()).Add(x[v]); }
			float gap = 0.02f * h;
			for (int s = floor; s <= neck; s++)
			{
				if (xs[s] == null || xs[s].Count < 4) continue;
				xs[s].Sort();
				List<(float Min, float Max)> clusters = new List<(float, float)>();
				float from = xs[s][0], last = xs[s][0];
				for (int i = 1; i < xs[s].Count; i++)
				{
					if (xs[s][i] - last > gap) { clusters.Add((from, last)); from = xs[s][i]; }
					last = xs[s][i];
				}
				clusters.Add((from, last));
				if (clusters.Count < 2) continue;
				(float Min, float Max) torso = clusters.OrderBy(c => Math.Abs((c.Min + c.Max) / 2 - centre)).First();
				torsoMin[s] = torso.Min; torsoMax[s] = torso.Max; known[s] = true;
			}
			int knownCount = known.Count(k => k);
			if (knownCount == 0)
			{
				notes.Add("regions: the head above the neck (" + heads.ToString("N0") + " vertices, cut " + ((neckY - yMin) / h * 100).ToString("0") + "% of the way up); the arms could not be told from the torso (no slice has them apart), so the rest takes from the whole of the original");
				for (int v = 0; v < count; v++) if (region[v] == RegionBody) region[v] = RegionAny;
				return region;
			}
			float[] knownMin = Enumerable.Range(0, Slices).Where(s => known[s]).Select(s => torsoMin[s]).OrderBy(f => f).ToArray();
			float[] knownMax = Enumerable.Range(0, Slices).Where(s => known[s]).Select(s => torsoMax[s]).OrderBy(f => f).ToArray();
			float medianMin = knownMin[knownMin.Length / 2], medianMax = knownMax[knownMax.Length / 2];
			int lefts = 0, rights = 0;
			for (int v = 0; v < count; v++)
			{
				if (region[v] != RegionBody) continue;
				int s = SliceOf(y[v]);
				if (s < floor || s > neck) continue;
				float lo = known[s] ? torsoMin[s] : medianMin, hi = known[s] ? torsoMax[s] : medianMax;
				int side = x[v] < lo - 0.005f * h ? -1 : x[v] > hi + 0.005f * h ? 1 : 0;
				if (side == 0) continue;
				bool left = (side > 0) == leftIsPlusX;
				region[v] = left ? RegionLeftArm : RegionRightArm;
				if (left) lefts++; else rights++;
			}
			notes.Add("regions by the file's shape: the head above the neck (" + heads.ToString("N0") + " vertices, cut " + ((neckY - yMin) / h * 100).ToString("0") + "% of the way up), the left arm " + lefts.ToString("N0") + ", the right arm " + rights.ToString("N0") + " (" + knownCount + " slices with the arms apart from the torso); each takes weights from the like part of the original");
			return region;
		}

		/// <summary>The game's limb bones a mesh limb can be turned onto: 1 an arm bone, 2 a leg bone, 0 neither.</summary>
		private static int LimbKind(string node)
		{
			string n = node.StartsWith("L_", StringComparison.Ordinal) || node.StartsWith("R_", StringComparison.Ordinal) ? node.Substring(2) : node;
			// Not the hand: a leaf's only direction is its parent's, and a mitten's middle is anywhere about the wrist.
			if (n == "kata" || n == "ude") return 1;
			if (n == "momo" || n == "hiza" || n == "sune") return 2;
			return 0;
		}

		private static float Length(float[] v) => MathF.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);
		private static float[] Unit(float[] v) { float l = Length(v); return l > 1e-9f ? new[] { v[0] / l, v[1] / l, v[2] / l } : new[] { 0f, 1f, 0f }; }

		/// <summary>The rotation taking unit vector a onto unit vector b, the short way, about <paramref name="pivot"/>: a row-vector 4x3.</summary>
		private static float[] RotationBetween(float[] a, float[] b, float[] pivot)
		{
			float dot = Math.Max(-1f, Math.Min(1f, a[0] * b[0] + a[1] * b[1] + a[2] * b[2]));
			float[] axis = { a[1] * b[2] - a[2] * b[1], a[2] * b[0] - a[0] * b[2], a[0] * b[1] - a[1] * b[0] };
			float sin = Length(axis);
			float[] r = Identity();
			if (sin > 1e-6f)
			{
				float[] k = { axis[0] / sin, axis[1] / sin, axis[2] / sin };
				float c = dot, s = sin, t = 1 - c;
				// Rodrigues, column-vector form, then transposed for v x R.
				float[,] R =
				{
					{ t * k[0] * k[0] + c, t * k[0] * k[1] - s * k[2], t * k[0] * k[2] + s * k[1] },
					{ t * k[0] * k[1] + s * k[2], t * k[1] * k[1] + c, t * k[1] * k[2] - s * k[0] },
					{ t * k[0] * k[2] - s * k[1], t * k[1] * k[2] + s * k[0], t * k[2] * k[2] + c }
				};
				for (int row = 0; row < 3; row++) for (int col = 0; col < 3; col++) r[row * 3 + col] = R[col, row];
			}
			else if (dot < 0)
			{
				// Opposite: half a turn about any axis perpendicular to a.
				float[] any = Math.Abs(a[0]) < 0.9f ? new[] { 1f, 0, 0 } : new[] { 0f, 1f, 0 };
				float[] k = Unit(new[] { a[1] * any[2] - a[2] * any[1], a[2] * any[0] - a[0] * any[2], a[0] * any[1] - a[1] * any[0] });
				for (int row = 0; row < 3; row++) for (int col = 0; col < 3; col++) r[row * 3 + col] = 2 * k[row] * k[col] - (row == col ? 1 : 0);
			}
			// About the pivot: t = pivot - pivot x R.
			float[] moved = Apply(r, pivot[0], pivot[1], pivot[2]);
			r[9] = pivot[0] - moved[0]; r[10] = pivot[1] - moved[1]; r[11] = pivot[2] - moved[2];
			return r;
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
