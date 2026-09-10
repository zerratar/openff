// A mesh of your own on the game's skeleton: a skinned glTF written over a model of the
// game's (j101, a character; n441, a monster) so that the game's own motions still drive it.
//
//   crystal mdl-reskin <file.glb|.gltf> <model> [out-dir] [--target=steam|ours]
//
// What is kept from the original, byte for byte: the node dictionary and node data (the
// joints and their bind transforms), the envelope matrices, and the SBC's node-building
// commands - NODEDESC with its parent and stack slot, the billboards, NODEMIX (the envelope
// blends) - so the .ncap motions, which name nodes by index, play as before, and so the
// battle's weapon hangs from the same R_te. What is replaced: the shapes, the materials and
// the textures. The new SBC is the old one with MAT/SHP/NODE/POSSCALE stripped, then, once
// every node is built and every slot stored, NODE visible; MAT i; SHP i per material; RET.
//
// The mesh: the glTF's vertices at its rest pose (GltfFile poses a skinned mesh through its
// joints at load), moved into the game's model space by the root joint (so an armature
// moved in Blender still lands), then each vertex sent through the matrix stack slot whose
// weights are nearest its own - a node's slot (one bone at weight 1) or an envelope slot
// (the DS's blend of several, as the original had them at the knees and elbows). The DS
// draws a vertex through one matrix, so this is what "weights" can mean here; the vertex
// is stored in that slot's space (position x slot bind^-1), which is what the display list
// restores it through. A mesh without weights is bound to the nearest bone.
//
// Not carried: vertex colours (the DS lights with a normal or takes a colour, and a normal
// is what the game's characters have), weights to a bone the original never stored to a
// slot (they snap to the nearest slot's), and blends the original did not have (the same).

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenFF.Graphics;

namespace Crystal
{
	internal static class Mdl0Reskin
	{
		public sealed class Result
		{
			public byte[] Nmdp;
			public byte[] Ntxp;
			public string Model;
			public int Vertices, Triangles, Materials;
			/// <summary>Vertices that went through an envelope slot (several bones) and vertices whose weights had to be snapped to a slot that is not quite theirs.</summary>
			public int Blended, Snapped;
			public List<string> Notes = new List<string>();
		}

		/// <summary>
		/// The glTF written over <paramref name="original"/> (the game's package, decompressed).
		/// <paramref name="textureStem"/> names the textures (the model's own name, as the game does).
		/// </summary>
		public static Result Build(byte[] original, GltfFile file, string textureStem)
		{
			if (file == null) throw new ArgumentNullException(nameof(file));
			original = Crystal.Editor.Models.Unpack(original);
			List<Mdl0Model> models = Mdl0.Read(original);
			if (models.Count == 0) throw new InvalidDataException("no model in the original package");
			Mdl0Model model = models[0];
			(byte[] info, byte[] nodeSection, byte[] sbcOld, byte[] materialTemplate, byte[] envelopes) = Mdl0.Sections(original);
			Result result = new Result { Model = model.Name };
			int nodeCount = model.Nodes.Count;

			// The bind pose: every node's built matrix, from the same walk the game does.
			Dictionary<int, int[]> built = new Dictionary<int, int[]>();
			Mdl0.Posed(original, model.Pieces.Select(p => new HashSet<int>(p.SlotMatrices.Keys)).ToList(), null, built);
			float[][] bind = new float[nodeCount][];
			for (int i = 0; i < nodeCount; i++) bind[i] = built.TryGetValue(i, out int[] m) ? ToFloat(m) : Identity();
			int[] parents = new int[nodeCount];
			for (int i = 0; i < nodeCount; i++) parents[i] = model.NodeParents.TryGetValue(i, out int p) && p != i && p < nodeCount ? p : -1;

			// The slots a vertex can go through, each with its weights over the nodes. A slot the
			// original stored to twice (a node's matrix, then a blend over it) holds its last value
			// by the time our shapes draw, at the end of the SBC, so that last value is what counts.
			List<SlotChoice> slots = new List<SlotChoice>();
			foreach (KeyValuePair<int, Mdl0Slot> pair in model.Slots.OrderBy(s => s.Key))
			{
				Mdl0Slot slot = pair.Value;
				Dictionary<int, float> weights = new Dictionary<int, float>();
				if (slot.Node >= 0 && slot.Node < nodeCount) weights[slot.Node] = 1f;
				else if (slot.Blend != null)
				{
					float total = slot.Blend.Where(b => b.Node >= 0 && b.Node < nodeCount).Sum(b => (float)b.Weight);
					if (total <= 0) continue;
					foreach ((int node, int weight) in slot.Blend)
					{
						if (node < 0 || node >= nodeCount) continue;
						weights[node] = (weights.TryGetValue(node, out float w) ? w : 0) + weight / total;
					}
				}
				else continue;
				slots.Add(new SlotChoice { Slot = pair.Key, Weights = weights, Bind = ToFloat(slot.Matrix), Inverse = Gltf.Invert(ToFloat(slot.Matrix)) });
			}
			HashSet<int> nodesWithSlot = new HashSet<int>(slots.Where(s => s.Weights.Count == 1).Select(s => s.Weights.Keys.First()));
			// A node that stores no slot can still carry geometry: a shape drawn right after its
			// NODEDESC goes through the node's own matrix, as the game draws j101's head. Such a
			// node is a choice too (Slot -1, Node set); a whole triangle on it is drawn inline.
			for (int i = 0; i < nodeCount; i++)
			{
				if (nodesWithSlot.Contains(i)) continue;
				slots.Add(new SlotChoice { Slot = -1, Node = i, Weights = new Dictionary<int, float> { [i] = 1f }, Bind = bind[i], Inverse = Gltf.Invert(bind[i]) });
			}
			if (slots.Count == 0) throw new InvalidDataException(model.Name + " has no node a mesh can be bound to");
			List<SlotChoice> realSlots = slots.Where(s => s.Slot >= 0).ToList();

			// ---- the glTF: its joints against the model's nodes ---------------------------------------
			List<GltfMesh> meshes = file.Meshes.Where(m => m.Indices != null && m.Indices.Length >= 3 && m.Positions != null).ToList();
			if (meshes.Count == 0) throw new InvalidDataException("the file has no triangles");
			float[][] rest = file.WorldMatrices(null, 0f);
			// Joint (index within a skin) -> the model's node, by name.
			Dictionary<int, int[]> jointNodes = new Dictionary<int, int[]>();
			HashSet<string> unknown = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			for (int s = 0; s < file.Skins.Count; s++)
			{
				GltfSkin skin = file.Skins[s];
				int[] map = new int[skin.Joints.Length];
				for (int j = 0; j < map.Length; j++)
				{
					string jointName = skin.Joints[j] >= 0 && skin.Joints[j] < file.Nodes.Count ? file.Nodes[skin.Joints[j]].Name : null;
					map[j] = jointName == null ? -1 : model.Nodes.FindIndex(n => string.Equals(n, jointName, StringComparison.OrdinalIgnoreCase));
					if (map[j] < 0 && jointName != null) unknown.Add(jointName);
				}
				jointNodes[s] = map;
			}
			if (unknown.Count > 0) result.Notes.Add("bones the model does not have (their weight goes to the others): " + string.Join(", ", unknown.OrderBy(n => n)));

			// Into the game's space: by the root joint, wherever Blender left the armature. A file
			// with no skin (or no root by the model's name) is taken as already there.
			float[] toGame = Identity();
			int rootNode = Array.IndexOf(parents, -1);
			if (rootNode < 0) rootNode = 0;
			int rootGltf = file.Nodes.FindIndex(n => string.Equals(n.Name, model.Nodes[rootNode], StringComparison.OrdinalIgnoreCase));
			bool skinned = meshes.Any(m => m.Skin >= 0 && m.Joints != null && m.Weights != null);
			if (skinned && rootGltf >= 0 && rootGltf < rest.Length)
			{
				toGame = Gltf.Mul(Gltf.Invert(RowVector(rest[rootGltf])), bind[rootNode]);
				// How far the file's rest pose is from the game's bind pose: the motions assume the game's.
				float worst = 0; string worstNode = null;
				for (int i = 0; i < nodeCount; i++)
				{
					int g = file.Nodes.FindIndex(n => string.Equals(n.Name, model.Nodes[i], StringComparison.OrdinalIgnoreCase));
					if (g < 0 || g >= rest.Length) continue;
					float[] there = Gltf.Mul(RowVector(rest[g]), toGame);
					float d = MathF.Sqrt(Sq(there[9] - bind[i][9]) + Sq(there[10] - bind[i][10]) + Sq(there[11] - bind[i][11]));
					if (d > worst) { worst = d; worstNode = model.Nodes[i]; }
				}
				if (worst > 0.05f) result.Notes.Add("the file's rest pose is off the game's bind pose by up to " + worst.ToString("0.00") + " at " + worstNode + " - the mesh is bound where the game's bones are, so it may not sit as it did in Blender");
			}
			else if (!skinned) result.Notes.Add("the file has no bone weights: every vertex is bound to the nearest bone");

			// ---- textures and materials -----------------------------------------------------------------
			List<int> materialIds = meshes.Select(m => m.Material).Distinct().OrderBy(i => i).ToList();
			if (materialIds.Count > 255) throw new InvalidDataException("more than 255 materials");
			List<Tex0Write.NewTexture> textures = new List<Tex0Write.NewTexture>();
			List<bool> doubleSided = new List<bool>();
			for (int i = 0; i < materialIds.Count; i++)
			{
				GltfMaterial material = materialIds[i] >= 0 && materialIds[i] < file.Materials.Count ? file.Materials[materialIds[i]] : new GltfMaterial();
				textures.Add(Mdl0Write.TextureOf(file, material, materialIds.Count == 1 ? textureStem : textureStem + "_" + i, result.Notes));
				doubleSided.Add(material.DoubleSided);
			}
			while (textures.Sum(t => (long)Tex0Write.TexelBytes(t.Format, t.Width, t.Height)) > 0x7FFF8)
			{
				Tex0Write.NewTexture largest = textures.OrderByDescending(t => t.Width * t.Height).First();
				if (largest.Width <= 8 && largest.Height <= 8) break;
				int nw = Math.Max(8, largest.Width / 2), nh = Math.Max(8, largest.Height / 2);
				largest.Rgba = Mdl0Write.Resample(largest.Rgba, largest.Width, largest.Height, nw, nh);
				result.Notes.Add(largest.Name + ": " + largest.Width + "x" + largest.Height + " halved to " + nw + "x" + nh + " (a package holds 512 KB of texels)");
				largest.Width = nw; largest.Height = nh;
			}

			// ---- every vertex: the matrix it goes through, and its position in that matrix's space ----------
			// Resolved once per mesh. A vertex bound to a node without a slot can only be drawn
			// inline, right after that node's NODEDESC, and only in a triangle wholly on that node;
			// a mixed triangle's such vertices take the nearest real slot instead.
			Dictionary<string, SlotChoice> chosen = new Dictionary<string, SlotChoice>(StringComparer.Ordinal);
			Dictionary<string, SlotChoice> chosenReal = new Dictionary<string, SlotChoice>(StringComparer.Ordinal);
			List<string> snapExamples = new List<string>();
			List<Bound[]> boundPerMesh = new List<Bound[]>();
			float extent = 0;
			foreach (GltfMesh mesh in meshes)
			{
				int count = mesh.VertexCount;
				Bound[] bound = new Bound[count];
				int[] map = mesh.Skin >= 0 && jointNodes.TryGetValue(mesh.Skin, out int[] have) ? have : null;
				for (int v = 0; v < count; v++)
				{
					float[] p = Apply(toGame, mesh.Positions[v * 3], mesh.Positions[v * 3 + 1], mesh.Positions[v * 3 + 2]);
					float[] normalGame = mesh.Normals != null ? ApplyRotation(toGame, mesh.Normals[v * 3], mesh.Normals[v * 3 + 1], mesh.Normals[v * 3 + 2]) : null;
					// The vertex's weights over the model's nodes.
					Dictionary<int, float> weights = new Dictionary<int, float>();
					if (map != null && mesh.Joints != null && mesh.Weights != null)
					{
						for (int k = 0; k < 4; k++)
						{
							float w = mesh.Weights[v * 4 + k];
							int j = (int)mesh.Joints[v * 4 + k];
							if (w <= 0 || j < 0 || j >= map.Length || map[j] < 0) continue;
							weights[map[j]] = (weights.TryGetValue(map[j], out float had) ? had : 0) + w;
						}
					}
					if (weights.Count == 0)
					{
						weights[NearestBone(p, bind, parents, Enumerable.Range(0, nodeCount).ToHashSet())] = 1f;
					}
					float total = weights.Values.Sum();
					SlotChoice slot = Choose(weights, total, slots, chosen, out bool snapped);
					if (snapped)
					{
						result.Snapped++;
						if (snapExamples.Count < 3)
						{
							string Names(IEnumerable<KeyValuePair<int, float>> ws, float over) => string.Join(" + ", ws.OrderByDescending(w => w.Value).Select(w => model.Nodes[w.Key] + " " + (w.Value / over).ToString("0.00")));
							snapExamples.Add(Names(weights, total) + " -> " + Names(slot.Weights, 1f));
						}
					}
					if (slot.Weights.Count > 1) result.Blended++;
					bound[v] = new Bound { Choice = slot, World = p, Normal = normalGame, Weights = weights, Total = total };
				}
				boundPerMesh.Add(bound);
			}

			// Per triangle: inline on a node, or through the slots. Positions go into the chosen
			// matrix's space here, once the choice is final.
			List<Placed> placed = new List<Placed>();
			for (int mi = 0; mi < meshes.Count; mi++)
			{
				GltfMesh mesh = meshes[mi];
				Bound[] bound = boundPerMesh[mi];
				for (int tri = 0; tri + 2 < mesh.Indices.Length / 3 * 3; tri += 3)
				{
					int[] vs = { mesh.Indices[tri], mesh.Indices[tri + 1], mesh.Indices[tri + 2] };
					SlotChoice[] choice = vs.Select(v => bound[v].Choice).ToArray();
					int inlineNode = -1;
					if (choice.All(c => c.Slot < 0 && c.Node == choice[0].Node)) inlineNode = choice[0].Node;
					else
					{
						for (int k = 0; k < 3; k++)
						{
							if (choice[k].Slot >= 0) continue;
							Bound b = bound[vs[k]];
							choice[k] = Choose(b.Weights, b.Total, realSlots, chosenReal, out _);
							result.Snapped++;
						}
					}
					Placed tp = new Placed { Material = mesh.Material, InlineNode = inlineNode, Vertices = new PlacedVertex[3] };
					for (int k = 0; k < 3; k++)
					{
						Bound b = bound[vs[k]];
						float[] local = Apply(choice[k].Inverse, b.World[0], b.World[1], b.World[2]);
						float[] normal = b.Normal != null ? Turn(choice[k].Inverse, b.Normal[0], b.Normal[1], b.Normal[2]) : null;
						tp.Vertices[k] = new PlacedVertex
						{
							Slot = choice[k].Slot, Position = local, Normal = normal, World = b.World,
							U = mesh.Uvs != null ? mesh.Uvs[vs[k] * 2] : 0.5f, V = mesh.Uvs != null ? mesh.Uvs[vs[k] * 2 + 1] : 0.5f
						};
						extent = Math.Max(extent, Math.Max(Math.Abs(local[0]), Math.Max(Math.Abs(local[1]), Math.Abs(local[2]))));
					}
					if (mesh.Normals == null)
					{
						float[] flat = Cross(Sub(tp.Vertices[1].Position, tp.Vertices[0].Position), Sub(tp.Vertices[2].Position, tp.Vertices[0].Position));
						foreach (PlacedVertex pv in tp.Vertices) pv.Normal = flat;
					}
					placed.Add(tp);
				}
			}
			int shift = 0;
			while ((7.99f * (1 << shift)) < extent && shift < 12) shift++;
			int posScale = 4096 << shift;
			float toFixed = 4096f / (1 << shift);

			// ---- display lists ------------------------------------------------------------------------------------
			// The slot-drawn triangles: one list per material, the slot restored whenever it changes,
			// drawn at the end of the SBC. The inline ones: one list per (node, material), drawn
			// after that node's NODEDESC through the node's own matrix (no restore).
			List<byte[]> lists = new List<byte[]>();
			List<(int Material, int Shape)> finalShapes = new List<(int, int)>();
			Dictionary<int, List<(int Material, int Shape)>> inlineShapes = new Dictionary<int, List<(int, int)>>();
			float[] min = { float.MaxValue, float.MaxValue, float.MaxValue }, max = { float.MinValue, float.MinValue, float.MinValue };
			byte[] ListOf(IEnumerable<Placed> triangles, int texW, int texH, bool restores)
			{
				Mdl0Write.DisplayList dl = new Mdl0Write.DisplayList();
				dl.Command(0x40, 0);   // BEGIN_VTXS triangles
				int lastSlot = -1;
				int inRun = 0;
				foreach (Placed tp in triangles)
				{
					if (inRun + 3 > Mdl0Write.RunLimit)
					{
						// The game gathers a run into a fixed buffer: a new run before it fills.
						dl.Command(0x41); dl.Command(0x40, 0);
						inRun = 0;
						lastSlot = -1;
					}
					inRun += 3;
					foreach (PlacedVertex pv in tp.Vertices)
					{
						if (restores && pv.Slot != lastSlot)
						{
							dl.Command(0x14, (uint)pv.Slot);   // MTX_RESTORE
							lastSlot = pv.Slot;
						}
						int s = (int)Math.Round(pv.U * texW * 16), tt = (int)Math.Round(pv.V * texH * 16);
						dl.Command(0x22, (uint)((s & 0xFFFF) | ((tt & 0xFFFF) << 16)));
						float[] nrm = pv.Normal ?? new[] { 0f, 1f, 0f };
						dl.Command(0x21, Mdl0Write.Normal(nrm[0], nrm[1], nrm[2]));
						for (int c = 0; c < 3; c++) { min[c] = Math.Min(min[c], pv.World[c]); max[c] = Math.Max(max[c], pv.World[c]); }
						int fx = Mdl0Write.Fixed16(pv.Position[0] * toFixed), fy = Mdl0Write.Fixed16(pv.Position[1] * toFixed), fz = Mdl0Write.Fixed16(pv.Position[2] * toFixed);
						dl.Command(0x23, (uint)((fx & 0xFFFF) | ((fy & 0xFFFF) << 16)), (uint)(fz & 0xFFFF));
						result.Vertices++;
					}
					result.Triangles++;
				}
				dl.Command(0x41);   // END_VTXS
				return dl.Bytes();
			}
			foreach (int materialId in materialIds)
			{
				int t = materialIds.IndexOf(materialId);
				int texW = textures[t].Width, texH = textures[t].Height;
				List<Placed> ofMaterial = placed.Where(tp => tp.Material == materialId).ToList();
				List<Placed> viaSlots = ofMaterial.Where(tp => tp.InlineNode < 0).OrderBy(tp => tp.Vertices[0].Slot).ToList();
				if (viaSlots.Count > 0)
				{
					lists.Add(ListOf(viaSlots, texW, texH, true));
					finalShapes.Add((t, lists.Count - 1));
				}
				foreach (IGrouping<int, Placed> onNode in ofMaterial.Where(tp => tp.InlineNode >= 0).GroupBy(tp => tp.InlineNode))
				{
					lists.Add(ListOf(onNode, texW, texH, false));
					if (!inlineShapes.TryGetValue(onNode.Key, out List<(int, int)> here)) inlineShapes[onNode.Key] = here = new List<(int, int)>();
					here.Add((t, lists.Count - 1));
				}
			}
			result.Materials = materialIds.Count;
			if (result.Vertices == 0) throw new InvalidDataException("the file has no triangles");
			if (result.Vertices > Mdl0Write.ModelLimit) throw new InvalidDataException(result.Triangles.ToString("N0") + " triangles: the game draws at most " + (Mdl0Write.ModelLimit / 3).ToString("N0") + " per model (" + Mdl0Write.ModelLimit.ToString("N0") + " vertices) - decimate the mesh in Blender");
			if (lists.Count > 255) throw new InvalidDataException(lists.Count + " shapes: the model holds at most 255 - fewer materials, or fewer bones drawn without a slot");
			if (result.Vertices > 65535) result.Notes.Add(result.Vertices + " vertices: the model's own count is 16 bits and saturates; the geometry is all there");
			if (inlineShapes.Count > 0) result.Notes.Add("drawn on the node itself (no stack slot): " + string.Join(", ", inlineShapes.Keys.OrderBy(k => k).Select(k => model.Nodes[k])));
			if (result.Snapped > 0) result.Notes.Add(result.Snapped + " vertices' weights had no matrix of their own and took the nearest (the DS draws a vertex through one node, or one of the blends the original had), e.g. " + string.Join("; ", snapExamples));

			// ---- the SBC: the original's node building, inline shapes after their node, then the rest ------------
			List<byte> sbc = new List<byte>();
			int at = 0;
			int lastNode = 0;
			while (at < sbcOld.Length)
			{
				int op = sbcOld[at] & 0x1F, flags = sbcOld[at] & 0xE0;
				int length;
				switch (op)
				{
					case 0: case 11: length = 1; break;                                   // NOP, POSSCALE
					case 1: length = 1; break;                                            // RET
					case 2: length = 3; break;                                            // NODE (visibility)
					case 3: case 4: case 5: length = 2; break;                            // MTX, MAT, SHP
					case 6: length = 4 + ((flags & 0x20) != 0 ? 1 : 0) + ((flags & 0x40) != 0 ? 1 : 0); break;
					case 7: case 8: length = 2 + ((flags & 0x20) != 0 ? 1 : 0) + ((flags & 0x40) != 0 ? 1 : 0); break;
					case 9: length = 3 + sbcOld[at + 2] * 3; break;
					default: throw new InvalidDataException(model.Name + ": SBC command " + op + " is not one this writer knows");
				}
				if (at + length > sbcOld.Length) break;
				if (op == 1) break;
				// Kept: what builds nodes and slots. Dropped: what drew the old shapes.
				if (op == 3 || op == 6 || op == 7 || op == 8 || op == 9) for (int i = 0; i < length; i++) sbc.Add(sbcOld[at + i]);
				if (op == 6)
				{
					lastNode = sbcOld[at + 1];
					if (inlineShapes.TryGetValue(lastNode, out List<(int Material, int Shape)> here))
					{
						sbc.Add(0x02); sbc.Add((byte)lastNode); sbc.Add(0x01);            // NODE visible
						foreach ((int material, int shape) in here) { sbc.Add(0x04); sbc.Add((byte)material); sbc.Add(0x05); sbc.Add((byte)shape); }
					}
				}
				at += length;
			}
			sbc.Add(0x02); sbc.Add((byte)lastNode); sbc.Add(0x01);                    // NODE visible
			foreach ((int material, int shape) in finalShapes) { sbc.Add(0x04); sbc.Add((byte)material); sbc.Add(0x05); sbc.Add((byte)shape); }
			sbc.Add(0x01);
			while (sbc.Count % 4 != 0) sbc.Add(0x00);

			byte[] mat = Mdl0Write.MaterialBlock(textures, doubleSided, materialTemplate);
			byte[] shp = Mdl0Write.ShapeBlock(lists);

			// ---- the model: offsets, the original's info patched, its nodes, our SBC, materials, shapes, its envelopes
			int ofsNodeInfo = 64;
			int ofsSbc = ofsNodeInfo + nodeSection.Length;
			int ofsMat = ofsSbc + sbc.Count;
			int ofsShp = ofsMat + mat.Length;
			int ofsEvp = Mdl0Write.Align(ofsShp + shp.Length, 4);
			int modelSize = ofsEvp + (envelopes?.Length ?? 0);
			byte[] modelBytes = new byte[modelSize];
			Mdl0Write.Put32(modelBytes, 0, (uint)modelSize); Mdl0Write.Put32(modelBytes, 4, (uint)ofsSbc); Mdl0Write.Put32(modelBytes, 8, (uint)ofsMat); Mdl0Write.Put32(modelBytes, 12, (uint)ofsShp);
			Mdl0Write.Put32(modelBytes, 16, (uint)(envelopes != null ? ofsEvp : modelSize));
			Array.Copy(info, 0, modelBytes, 20, 44);
			int n = lists.Count;
			modelBytes[20 + 4] = (byte)n; modelBytes[20 + 5] = (byte)n;
			Mdl0Write.Put32(modelBytes, 20 + 8, (uint)posScale); Mdl0Write.Put32(modelBytes, 20 + 12, (uint)(4096 * 4096 / posScale));
			Mdl0Write.Put16(modelBytes, 20 + 16, (ushort)Math.Min(65535, result.Vertices)); Mdl0Write.Put16(modelBytes, 20 + 18, (ushort)Math.Min(65535, result.Triangles));
			Mdl0Write.Put16(modelBytes, 20 + 20, (ushort)Math.Min(65535, result.Triangles)); Mdl0Write.Put16(modelBytes, 20 + 22, 0);
			Mdl0Write.WriteBox(modelBytes, 20, min, max);
			Array.Copy(nodeSection, 0, modelBytes, ofsNodeInfo, nodeSection.Length);
			sbc.CopyTo(modelBytes, ofsSbc);
			Array.Copy(mat, 0, modelBytes, ofsMat, mat.Length);
			Array.Copy(shp, 0, modelBytes, ofsShp, shp.Length);
			if (envelopes != null) Array.Copy(envelopes, 0, modelBytes, ofsEvp, envelopes.Length);

			result.Nmdp = Mdl0Write.Package(modelBytes, model.Name);
			result.Ntxp = Tex0Write.Build(textures);
			return result;
		}

		/// <summary>A matrix a vertex can go through: a stack slot (Slot >= 0), or a node's own matrix drawn inline (Slot -1, Node set).</summary>
		private sealed class SlotChoice
		{
			public int Slot;
			public int Node = -1;
			public Dictionary<int, float> Weights;
			public float[] Bind, Inverse;
		}

		/// <summary>A vertex in the game's model space with its weights and the choice made for it.</summary>
		private sealed class Bound
		{
			public SlotChoice Choice;
			public float[] World, Normal;
			public Dictionary<int, float> Weights;
			public float Total;
		}

		private sealed class Placed
		{
			public int Material;
			public int InlineNode;
			public PlacedVertex[] Vertices;
		}

		private sealed class PlacedVertex
		{
			public int Slot;
			public float[] Position, Normal, World;
			public float U, V;
		}

		/// <summary>The slot whose weights are nearest the vertex's (least total difference); <paramref name="snapped"/> when the fit is not exact.</summary>
		private static SlotChoice Choose(Dictionary<int, float> weights, float total, List<SlotChoice> slots, Dictionary<string, SlotChoice> cache, out bool snapped)
		{
			if (total <= 0) total = 1;
			// Rounded to 1/64 so equal-enough vertices share a lookup.
			string key = string.Join(",", weights.OrderBy(w => w.Key).Select(w => w.Key + ":" + Math.Round(w.Value / total * 64)));
			SlotChoice best;
			float bestDistance = float.MaxValue;
			if (cache.TryGetValue(key, out best))
			{
				bestDistance = Distance(weights, total, best);
				snapped = bestDistance > 0.02f;
				return best;
			}
			foreach (SlotChoice slot in slots)
			{
				float d = Distance(weights, total, slot);
				if (d < bestDistance - 1e-6f || (Math.Abs(d - bestDistance) <= 1e-6f && best != null && slot.Weights.Count < best.Weights.Count))
				{
					bestDistance = d;
					best = slot;
				}
			}
			cache[key] = best;
			snapped = bestDistance > 0.02f;
			return best;
		}

		private static float Distance(Dictionary<int, float> weights, float total, SlotChoice slot)
		{
			float d = 0;
			foreach (KeyValuePair<int, float> w in weights) d += Math.Abs(w.Value / total - (slot.Weights.TryGetValue(w.Key, out float s) ? s : 0));
			foreach (KeyValuePair<int, float> s in slot.Weights) if (!weights.ContainsKey(s.Key)) d += s.Value;
			return d;
		}

		/// <summary>The node (among those with a slot of their own) whose bone - the segment from it to each child, or its point - is nearest.</summary>
		private static int NearestBone(float[] p, float[][] bind, int[] parents, HashSet<int> candidates)
		{
			int best = candidates.Count > 0 ? candidates.First() : 0;
			float bestD = float.MaxValue;
			foreach (int node in candidates)
			{
				float[] a = { bind[node][9], bind[node][10], bind[node][11] };
				float d = float.MaxValue;
				bool hasChild = false;
				for (int c = 0; c < parents.Length; c++)
				{
					if (parents[c] != node) continue;
					hasChild = true;
					float[] b = { bind[c][9], bind[c][10], bind[c][11] };
					d = Math.Min(d, SegmentDistance(p, a, b));
				}
				if (!hasChild) d = MathF.Sqrt(Sq(p[0] - a[0]) + Sq(p[1] - a[1]) + Sq(p[2] - a[2]));
				if (d < bestD) { bestD = d; best = node; }
			}
			return best;
		}

		private static float SegmentDistance(float[] p, float[] a, float[] b)
		{
			float[] ab = { b[0] - a[0], b[1] - a[1], b[2] - a[2] };
			float len2 = ab[0] * ab[0] + ab[1] * ab[1] + ab[2] * ab[2];
			float t = len2 > 1e-9f ? Math.Clamp(((p[0] - a[0]) * ab[0] + (p[1] - a[1]) * ab[1] + (p[2] - a[2]) * ab[2]) / len2, 0, 1) : 0;
			return MathF.Sqrt(Sq(p[0] - a[0] - ab[0] * t) + Sq(p[1] - a[1] - ab[1] * t) + Sq(p[2] - a[2] - ab[2] * t));
		}

		private static float Sq(float v) => v * v;

		private static float[] Identity() => new[] { 1f, 0, 0, 0, 1f, 0, 0, 0, 1f, 0, 0, 0 };

		private static float[] ToFloat(int[] m)
		{
			if (m == null) return Identity();
			float[] f = new float[12];
			for (int i = 0; i < 12; i++) f[i] = m[i] / 4096f;
			return f;
		}

		/// <summary>A glTF column-major 4x4 as the game's 4x3 row-vector matrix (rotation rows, then the translation).</summary>
		private static float[] RowVector(float[] c)
		{
			return new[] { c[0], c[1], c[2], c[4], c[5], c[6], c[8], c[9], c[10], c[12], c[13], c[14] };
		}

		/// <summary>v x M for a row-vector 4x3.</summary>
		private static float[] Apply(float[] m, float x, float y, float z) => new[]
		{
			x * m[0] + y * m[3] + z * m[6] + m[9],
			x * m[1] + y * m[4] + z * m[7] + m[10],
			x * m[2] + y * m[5] + z * m[8] + m[11]
		};

		private static float[] ApplyRotation(float[] m, float x, float y, float z)
		{
			float[] n = { x * m[0] + y * m[3] + z * m[6], x * m[1] + y * m[4] + z * m[7], x * m[2] + y * m[5] + z * m[8] };
			float len = MathF.Sqrt(n[0] * n[0] + n[1] * n[1] + n[2] * n[2]);
			if (len > 1e-6f) { n[0] /= len; n[1] /= len; n[2] /= len; }
			return n;
		}

		/// <summary>A normal through the rotation part of a slot's inverse bind, normalised.</summary>
		private static float[] Turn(float[] m, float x, float y, float z) => ApplyRotation(m, x, y, z);

		private static float[] Sub(float[] a, float[] b) => new[] { a[0] - b[0], a[1] - b[1], a[2] - b[2] };
		private static float[] Cross(float[] a, float[] b) => new[] { a[1] * b[2] - a[2] * b[1], a[2] * b[0] - a[0] * b[2], a[0] * b[1] - a[1] * b[0] };
	}
}
