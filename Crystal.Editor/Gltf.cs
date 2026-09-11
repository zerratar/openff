// glTF 2.0 (.glb) out of a model bundle, with its textures and, if asked, its motions.
//
// This is the door to Blender. The viewer's bundle is already what glTF wants - a
// vertex buffer, triangle indices, groups with a material each - and the way the game
// skins it (every vertex goes through exactly one node's matrix) is a glTF skin with a
// weight of 1. The skin is the model's own skeleton: one joint per node of the model,
// parented as the SBC parents them, each bone's rest transform its local bind matrix and
// its inverse bind matrix the inverse of its world one (the vertices are stored in
// bind-pose world space, as the file has them). So Blender shows the bones where the
// joints are - hips, spine, arms, the hand joints a weapon hangs from - and a mesh of
// your own can be weighted to them.
//
// Each motion is an animation keying every joint's local translation, rotation and
// scale per frame at 30 fps, from the node matrices the same SBC walk builds for the
// game. Nothing here is lossy beyond the decompose: the NDS matrices are
// rotation x scale x translation.
//
// Import is not here. Turning a mesh back into NDS display lists and a motion back into
// packed joint tables is its own project; this gets the assets out so people can look,
// measure and retexture.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Crystal.Editor;

namespace Crystal
{
	internal static class Gltf
	{
		/// <summary>
		/// The .glb bytes. <paramref name="texturePng"/> gives a texture's PNG by name or
		/// null; <paramref name="rig"/> is the model's skeleton with whichever motions go along.
		/// </summary>
		public static byte[] Write(ModelBundle bundle, Func<string, byte[]> texturePng, Models.Rig rig)
		{
			return Write(bundle, texturePng, rig, null, null);
		}

		/// <summary>
		/// As above, with <paramref name="vertexWeights"/> giving each vertex's joints and weights
		/// outright (an auto-rigged mesh, whose bundle has no matrix instances) and
		/// <paramref name="normals"/> three floats a vertex when the mesh has them. A texture's
		/// bytes may be a JPEG; the image is marked by what the bytes are.
		/// </summary>
		public static byte[] Write(ModelBundle bundle, Func<string, byte[]> texturePng, Models.Rig rig, Func<int, (int Node, float Weight)[]> vertexWeights, float[] normals)
		{
			BinaryWriter bin = new BinaryWriter(new MemoryStream());
			JsonArray bufferViews = new JsonArray();
			JsonArray accessors = new JsonArray();

			int View(byte[] bytes, int? target = null, int? stride = null)
			{
				Align(bin, 4);
				long offset = bin.BaseStream.Position;
				bin.Write(bytes);
				JsonObject view = new JsonObject { ["buffer"] = 0, ["byteOffset"] = offset, ["byteLength"] = bytes.Length };
				if (target.HasValue) view["target"] = target.Value;
				if (stride.HasValue) view["byteStride"] = stride.Value;
				bufferViews.Add(view);
				return bufferViews.Count - 1;
			}

			int Accessor(int view, int componentType, int count, string type, float[] min = null, float[] max = null, bool normalized = false)
			{
				JsonObject accessor = new JsonObject
				{
					["bufferView"] = view, ["componentType"] = componentType, ["count"] = count, ["type"] = type
				};
				if (min != null) accessor["min"] = new JsonArray(min.Select(v => (JsonNode)v).ToArray());
				if (max != null) accessor["max"] = new JsonArray(max.Select(v => (JsonNode)v).ToArray());
				if (normalized) accessor["normalized"] = true;
				accessors.Add(accessor);
				return accessors.Count - 1;
			}

			// ---- the joints: one per node of the model ------------------------------------------
			// A vertex's JOINTS_0/WEIGHTS_0 name the nodes whose matrix moved it: the group's
			// own node or the node behind the stack slot its display list restored, at weight
			// 1 - or, for an envelope slot, the nodes blended into it with their weights. glTF
			// takes four per vertex; a blend of more keeps its four heaviest, made to sum to 1.
			int jointCount = Math.Max(1, rig.Nodes.Count);
			(int Node, float Weight)[] WeightsFor(int group, int local)
			{
				if (!rig.Weights.TryGetValue((group, local), out (int Node, float Weight)[] weights) || weights.Length == 0)
				{
					if (!rig.Weights.TryGetValue((group, 0), out weights) || weights.Length == 0) weights = new[] { (0, 1f) };
				}
				if (weights.Length > 4)
				{
					weights = weights.OrderByDescending(w => w.Weight).Take(4).ToArray();
					float total = weights.Sum(w => w.Weight);
					weights = weights.Select(w => (w.Node, w.Weight / total)).ToArray();
				}
				return weights;
			}

			// ---- vertices -----------------------------------------------------------------------
			int vertexCount = bundle.Buffer.Count / 8;
			byte[] positions = new byte[vertexCount * 12];
			byte[] coords = new byte[vertexCount * 8];
			byte[] colours = new byte[vertexCount * 12];
			byte[] jointIds = new byte[vertexCount * 8];             // ushort x4
			byte[] weights = new byte[vertexCount * 16];             // float x4
			float[] min = { float.MaxValue, float.MaxValue, float.MaxValue };
			float[] max = { float.MinValue, float.MinValue, float.MinValue };

			// Which group a vertex belongs to, from the index ranges.
			int[] groupOfVertex = new int[vertexCount];
			for (int g = 0; g < bundle.Groups.Count; g++)
			{
				ModelGroup group = bundle.Groups[g];
				for (int i = group.Start; i < group.Start + group.Count; i++)
				{
					int v = bundle.Indices[i];
					if (v >= 0 && v < vertexCount) groupOfVertex[v] = g;
				}
			}

			for (int v = 0; v < vertexCount; v++)
			{
				int at = v * 8;
				float x = bundle.Buffer[at], y = bundle.Buffer[at + 1], z = bundle.Buffer[at + 2];
				Put(positions, v * 12, x, y, z);
				min[0] = Math.Min(min[0], x); min[1] = Math.Min(min[1], y); min[2] = Math.Min(min[2], z);
				max[0] = Math.Max(max[0], x); max[1] = Math.Max(max[1], y); max[2] = Math.Max(max[2], z);
				Put(coords, v * 8, bundle.Buffer[at + 3], bundle.Buffer[at + 4]);
				Put(colours, v * 12, bundle.Buffer[at + 5], bundle.Buffer[at + 6], bundle.Buffer[at + 7]);
				int local = bundle.MatrixIndex != null && v < bundle.MatrixIndex.Count ? bundle.MatrixIndex[v] : 0;
				(int Node, float Weight)[] bones = vertexWeights != null ? (vertexWeights(v) ?? new[] { (0, 1f) }) : WeightsFor(groupOfVertex[v], local);
				if (bones.Length > 4) bones = bones.OrderByDescending(w => w.Weight).Take(4).ToArray();
				float[] w4 = new float[4];
				for (int k = 0; k < 4; k++)
				{
					int joint = k < bones.Length ? Math.Max(0, Math.Min(jointCount - 1, bones[k].Node)) : 0;
					BitConverter.GetBytes((ushort)joint).CopyTo(jointIds, v * 8 + k * 2);
					w4[k] = k < bones.Length ? bones[k].Weight : 0f;
				}
				Put(weights, v * 16, w4[0], w4[1], w4[2], w4[3]);
			}

			int positionView = View(positions, 34962);
			int coordView = View(coords, 34962);
			int colourView = View(colours, 34962);
			int jointView = View(jointIds, 34962);
			int weightView = View(weights, 34962);
			int positionAccessor = Accessor(positionView, 5126, vertexCount, "VEC3", min, max);
			int coordAccessor = Accessor(coordView, 5126, vertexCount, "VEC2");
			int colourAccessor = Accessor(colourView, 5126, vertexCount, "VEC3");
			int jointAccessor = Accessor(jointView, 5123, vertexCount, "VEC4");
			int weightAccessor = Accessor(weightView, 5126, vertexCount, "VEC4");
			int normalAccessor = -1;
			if (normals != null && normals.Length >= vertexCount * 3)
			{
				byte[] normalBytes = new byte[vertexCount * 12];
				for (int v = 0; v < vertexCount; v++) Put(normalBytes, v * 12, normals[v * 3], normals[v * 3 + 1], normals[v * 3 + 2]);
				normalAccessor = Accessor(View(normalBytes, 34962), 5126, vertexCount, "VEC3");
			}

			// ---- textures and materials ---------------------------------------------------------
			JsonArray images = new JsonArray();
			JsonArray textures = new JsonArray();
			JsonArray materials = new JsonArray();
			Dictionary<string, int> textureIndex = new Dictionary<string, int>(StringComparer.Ordinal);
			Dictionary<string, int> materialIndex = new Dictionary<string, int>(StringComparer.Ordinal);
			JsonArray samplers = new JsonArray
			{
				new JsonObject { ["magFilter"] = 9728, ["minFilter"] = 9728, ["wrapS"] = 10497, ["wrapT"] = 10497 }
			};

			int MaterialFor(ModelGroup group)
			{
				string key = (group.Texture ?? "#") + "|" + group.Colour.ToString("X6", CultureInfo.InvariantCulture)
					+ "|" + group.Alpha.ToString(CultureInfo.InvariantCulture) + "|" + (group.Translucent ? "t" : "o");
				if (materialIndex.TryGetValue(key, out int have)) return have;

				JsonObject pbr = new JsonObject { ["metallicFactor"] = 0, ["roughnessFactor"] = 1 };
				if (group.Texture != null)
				{
					if (!textureIndex.TryGetValue(group.Texture, out int texture))
					{
						byte[] png = texturePng(group.Texture);
						if (png != null)
						{
							int view = View(png);
							bool jpeg = png.Length > 2 && png[0] == 0xFF && png[1] == 0xD8;
							images.Add(new JsonObject { ["bufferView"] = view, ["mimeType"] = jpeg ? "image/jpeg" : "image/png", ["name"] = group.Texture });
							textures.Add(new JsonObject { ["source"] = images.Count - 1, ["sampler"] = 0, ["name"] = group.Texture });
							texture = textures.Count - 1;
						}
						else
						{
							texture = -1;
						}
						textureIndex[group.Texture] = texture;
					}
					if (texture >= 0)
					{
						pbr["baseColorTexture"] = new JsonObject { ["index"] = texture };
					}
				}
				float r = ((group.Colour >> 16) & 0xFF) / 255f, gg = ((group.Colour >> 8) & 0xFF) / 255f, b = (group.Colour & 0xFF) / 255f;
				pbr["baseColorFactor"] = new JsonArray(group.Texture != null ? 1f : r, group.Texture != null ? 1f : gg, group.Texture != null ? 1f : b, group.Alpha);
				JsonObject material = new JsonObject
				{
					["name"] = group.Material ?? ("material" + materials.Count),
					["pbrMetallicRoughness"] = pbr,
					["doubleSided"] = true,
					["alphaMode"] = group.Translucent || group.Alpha < 1f ? "BLEND" : "MASK"
				};
				if (!(group.Translucent || group.Alpha < 1f)) material["alphaCutoff"] = 0.05;
				materials.Add(material);
				materialIndex[key] = materials.Count - 1;
				return materials.Count - 1;
			}

			// ---- the mesh: one primitive per group -----------------------------------------------
			JsonArray primitives = new JsonArray();
			for (int g = 0; g < bundle.Groups.Count; g++)
			{
				ModelGroup group = bundle.Groups[g];
				if (group.Count == 0) continue;
				byte[] indices = new byte[group.Count * 4];
				for (int i = 0; i < group.Count; i++)
				{
					BitConverter.GetBytes((uint)bundle.Indices[group.Start + i]).CopyTo(indices, i * 4);
				}
				int indexView = View(indices, 34963);
				int indexAccessor = Accessor(indexView, 5125, group.Count, "SCALAR");
				JsonObject attributes = new JsonObject
				{
					["POSITION"] = positionAccessor, ["TEXCOORD_0"] = coordAccessor, ["COLOR_0"] = colourAccessor,
					["JOINTS_0"] = jointAccessor, ["WEIGHTS_0"] = weightAccessor
				};
				if (normalAccessor >= 0) attributes["NORMAL"] = normalAccessor;
				primitives.Add(new JsonObject
				{
					["attributes"] = attributes,
					["indices"] = indexAccessor,
					["material"] = MaterialFor(group),
					["mode"] = 4,
					["extras"] = new JsonObject { ["shape"] = group.Shape, ["node"] = group.Node, ["hidden"] = group.Hidden }
				});
			}

			// ---- nodes: the model, and the skeleton --------------------------------------------------
			// Joint j is node 1 + j. Its rest transform is its bind matrix made local to its
			// parent's (world = local x parentWorld in the game's row-vector convention, so
			// local = world x parentWorld^-1); roots hang off an armature node so Blender
			// makes one armature of them.
			JsonArray nodes = new JsonArray();
			nodes.Add(new JsonObject { ["name"] = bundle.Name ?? "model", ["mesh"] = 0, ["skin"] = 0 });
			JsonArray jointNodes = new JsonArray();
			JsonArray rootChildren = new JsonArray();
			float[] Identity = { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0 };
			float[] BindOf(int j) => j >= 0 && j < rig.Bind.Count && rig.Bind[j] != null ? rig.Bind[j] : Identity;
			int ParentOf(int j) => j < rig.Parents.Length ? rig.Parents[j] : -1;
			for (int j = 0; j < jointCount; j++)
			{
				int parent = ParentOf(j);
				float[] local = parent >= 0 ? Mul(BindOf(j), Invert(BindOf(parent))) : BindOf(j);
				Decompose(local, 0, out float[] tr, out float[] q, out float[] sc);
				JsonObject joint = new JsonObject
				{
					["name"] = j < rig.Nodes.Count ? rig.Nodes[j] : ("node" + j),
					["translation"] = new JsonArray(tr[0], tr[1], tr[2]),
					["rotation"] = new JsonArray(q[0], q[1], q[2], q[3]),
					["scale"] = new JsonArray(sc[0], sc[1], sc[2])
				};
				nodes.Add(joint);
				jointNodes.Add(nodes.Count - 1);
				if (parent < 0) rootChildren.Add(nodes.Count - 1);
			}
			for (int j = 0; j < jointCount; j++)
			{
				int parent = ParentOf(j);
				if (parent < 0) continue;
				JsonObject parentNode = (JsonObject)nodes[1 + parent];
				JsonArray children = parentNode["children"] as JsonArray;
				if (children == null) parentNode["children"] = children = new JsonArray();
				children.Add(1 + j);
			}
			nodes.Add(new JsonObject { ["name"] = "skeleton", ["children"] = rootChildren });
			int skeletonNode = nodes.Count - 1;

			// inverseBindMatrices: each joint's world bind matrix inverted - the vertices are
			// stored in bind-pose world space, so this takes them into the joint's own space.
			byte[] ibm = new byte[jointCount * 64];
			for (int j = 0; j < jointCount; j++)
			{
				PutMatrix(ibm, j * 64, Invert(BindOf(j)));
			}
			int ibmAccessor = Accessor(View(ibm), 5126, jointCount, "MAT4");

			// ---- the motions --------------------------------------------------------------------------
			// Per motion, every joint's local transform per frame - its animated world matrix
			// made local to its parent's animated one, as for the rest pose.
			JsonArray animations = new JsonArray();
			foreach (Models.RigMotion motion in rig.Motions ?? new List<Models.RigMotion>())
			{
				int frames = Math.Min(motion.Frames, motion.Worlds.Count);
				if (frames <= 0) continue;
				byte[] times = new byte[frames * 4];
				for (int f = 0; f < frames; f++) BitConverter.GetBytes(f / 30f).CopyTo(times, f * 4);
				int timeAccessor = Accessor(View(times), 5126, frames, "SCALAR", new[] { 0f }, new[] { (frames - 1) / 30f });

				JsonArray samplersA = new JsonArray();
				JsonArray channels = new JsonArray();
				for (int j = 0; j < jointCount; j++)
				{
					int parent = ParentOf(j);
					byte[] t = new byte[frames * 12];
					byte[] r = new byte[frames * 16];
					byte[] s = new byte[frames * 12];
					float[] previous = null;
					for (int f = 0; f < frames; f++)
					{
						float[][] worlds = motion.Worlds[f];
						float[] world = j < worlds.Length && worlds[j] != null ? worlds[j] : BindOf(j);
						float[] local = parent >= 0 && parent < worlds.Length && worlds[parent] != null ? Mul(world, Invert(worlds[parent])) : world;
						Decompose(local, 0, out float[] tr, out float[] q, out float[] sc);
						// q and -q are one rotation; keep the sign nearest the last frame's so a
						// linear step between keys does not swing the long way round.
						if (previous != null && q[0] * previous[0] + q[1] * previous[1] + q[2] * previous[2] + q[3] * previous[3] < 0)
						{
							for (int k = 0; k < 4; k++) q[k] = -q[k];
						}
						previous = q;
						Put(t, f * 12, tr[0], tr[1], tr[2]);
						Put(r, f * 16, q[0], q[1], q[2], q[3]);
						Put(s, f * 12, sc[0], sc[1], sc[2]);
					}
					int tAcc = Accessor(View(t), 5126, frames, "VEC3");
					int rAcc = Accessor(View(r), 5126, frames, "VEC4");
					int sAcc = Accessor(View(s), 5126, frames, "VEC3");
					foreach ((int acc, string path) in new[] { (tAcc, "translation"), (rAcc, "rotation"), (sAcc, "scale") })
					{
						samplersA.Add(new JsonObject { ["input"] = timeAccessor, ["output"] = acc, ["interpolation"] = "LINEAR" });
						channels.Add(new JsonObject
						{
							["sampler"] = samplersA.Count - 1,
							["target"] = new JsonObject { ["node"] = (int)jointNodes[j], ["path"] = path }
						});
					}
				}
				animations.Add(new JsonObject { ["name"] = motion.Name ?? "motion", ["samplers"] = samplersA, ["channels"] = channels });
			}

			// ---- the document -------------------------------------------------------------------------
			Align(bin, 4);
			byte[] binary = ((MemoryStream)bin.BaseStream).ToArray();
			JsonObject root = new JsonObject
			{
				["asset"] = new JsonObject { ["version"] = "2.0", ["generator"] = "Crystal - the OpenFF editor" },
				["scene"] = 0,
				["scenes"] = new JsonArray { new JsonObject { ["nodes"] = new JsonArray { 0, skeletonNode } } },
				["nodes"] = nodes,
				["meshes"] = new JsonArray { new JsonObject { ["name"] = bundle.Name ?? "model", ["primitives"] = primitives } },
				["skins"] = new JsonArray { new JsonObject { ["joints"] = jointNodes, ["inverseBindMatrices"] = ibmAccessor, ["skeleton"] = skeletonNode, ["name"] = "skin" } },
				["materials"] = materials,
				["textures"] = textures,
				["images"] = images,
				["samplers"] = samplers,
				["accessors"] = accessors,
				["bufferViews"] = bufferViews,
				["buffers"] = new JsonArray { new JsonObject { ["byteLength"] = binary.Length } }
			};
			if (animations.Count > 0) root["animations"] = animations;
			if (textures.Count == 0) { root.Remove("textures"); root.Remove("images"); root.Remove("samplers"); }

			byte[] json = Encoding.UTF8.GetBytes(root.ToJsonString());
			int jsonPadded = (json.Length + 3) & ~3;
			byte[] jsonChunk = new byte[jsonPadded];
			Array.Copy(json, jsonChunk, json.Length);
			for (int i = json.Length; i < jsonPadded; i++) jsonChunk[i] = 0x20;

			using MemoryStream glb = new MemoryStream();
			using BinaryWriter w = new BinaryWriter(glb);
			w.Write(0x46546C67u);                                     // glTF
			w.Write(2u);
			w.Write((uint)(12 + 8 + jsonChunk.Length + 8 + binary.Length));
			w.Write((uint)jsonChunk.Length); w.Write(0x4E4F534Au); w.Write(jsonChunk);   // JSON
			w.Write((uint)binary.Length); w.Write(0x004E4942u); w.Write(binary);        // BIN
			w.Flush();
			return glb.ToArray();
		}

		/// <summary>
		/// A .glb's JOINTS_0 and WEIGHTS_0 rewritten in place from the viewer's painted weights -
		/// four joint indices and four weights a vertex, vertices in the order the primitives come
		/// (GltfBundle's), the joints as the skin lists them. The accessors keep their types (a
		/// file of this writer's: ushort joints, float weights); anything else is refused.
		/// </summary>
		public static byte[] RewriteWeights(byte[] glb, int[] jointIndex, float[] weights) => RewriteWeights(glb, jointIndex, weights, null);

		/// <summary>
		/// As above, and with <paramref name="positions"/> (3 floats a vertex, the same order) the
		/// vertices' positions rewritten in place as well - the bind pose re-carried through repainted
		/// weights - the accessors' min and max refreshed. The JSON grows or shrinks by the min/max
		/// digits, so the file is rebuilt around it rather than patched.
		/// </summary>
		public static byte[] RewriteWeights(byte[] glb, int[] jointIndex, float[] weights, float[] positions)
		{
			if (glb.Length < 20 || BitConverter.ToUInt32(glb, 0) != 0x46546C67u) throw new InvalidDataException("not a .glb");
			int jsonLength = BitConverter.ToInt32(glb, 12);
			JsonNode root = JsonNode.Parse(Encoding.UTF8.GetString(glb, 20, jsonLength)) ?? throw new InvalidDataException("no JSON in the .glb");
			int binAt = 20 + jsonLength + 8;
			if (binAt > glb.Length || BitConverter.ToUInt32(glb, 20 + jsonLength + 4) != 0x004E4942u) throw new InvalidDataException("the .glb has no BIN chunk after its JSON");
			byte[] result = (byte[])glb.Clone();
			bool jsonChanged = false;
			JsonArray accessors = root["accessors"] as JsonArray, views = root["bufferViews"] as JsonArray, meshes = root["meshes"] as JsonArray, nodes = root["nodes"] as JsonArray;
			if (accessors == null || views == null || meshes == null) throw new InvalidDataException("the .glb has no meshes");
			// Primitives in the order the reader visits them: the scene's nodes, depth first.
			List<JsonObject> primitives = new List<JsonObject>();
			HashSet<int> seenMesh = new HashSet<int>();
			void Visit(int nodeIndex, int depth)
			{
				if (depth > 64 || nodes == null || nodeIndex < 0 || nodeIndex >= nodes.Count) return;
				JsonObject node = nodes[nodeIndex] as JsonObject;
				if (node?["mesh"] != null)
				{
					int mesh = node["mesh"].GetValue<int>();
					if (mesh >= 0 && mesh < meshes.Count) foreach (JsonNode p in meshes[mesh]["primitives"] as JsonArray ?? new JsonArray()) primitives.Add(p as JsonObject);
				}
				foreach (JsonNode c in node?["children"] as JsonArray ?? new JsonArray()) Visit(c.GetValue<int>(), depth + 1);
			}
			JsonArray scenes = root["scenes"] as JsonArray;
			int scene = root["scene"]?.GetValue<int>() ?? 0;
			if (scenes != null && scene < scenes.Count) foreach (JsonNode n in scenes[scene]["nodes"] as JsonArray ?? new JsonArray()) Visit(n.GetValue<int>(), 0);
			else if (nodes != null) for (int i = 0; i < nodes.Count; i++) Visit(i, 0);
			int vertex = 0;
			foreach (JsonObject primitive in primitives)
			{
				JsonObject attributes = primitive?["attributes"] as JsonObject;
				if (attributes?["POSITION"] == null) continue;
				JsonObject pAcc = accessors[attributes["POSITION"].GetValue<int>()] as JsonObject;
				int count = pAcc["count"].GetValue<int>();
				if (positions != null)
				{
					if (pAcc["componentType"].GetValue<int>() != 5126) throw new InvalidDataException("the file's positions are not floats");
					if ((vertex + count) * 3 > positions.Length) throw new InvalidDataException("the positions cover fewer vertices than the file has");
					int pAt = binAt + Offset(views, pAcc);
					float[] min = { float.MaxValue, float.MaxValue, float.MaxValue }, max = { float.MinValue, float.MinValue, float.MinValue };
					for (int v = 0; v < count; v++)
						for (int k = 0; k < 3; k++)
						{
							float f = positions[(vertex + v) * 3 + k];
							BitConverter.GetBytes(f).CopyTo(result, pAt + (v * 3 + k) * 4);
							min[k] = Math.Min(min[k], f); max[k] = Math.Max(max[k], f);
						}
					pAcc["min"] = new JsonArray(min[0], min[1], min[2]);
					pAcc["max"] = new JsonArray(max[0], max[1], max[2]);
					jsonChanged = true;
				}
				if (attributes["JOINTS_0"] == null || attributes["WEIGHTS_0"] == null) { vertex += count; continue; }
				JsonObject jAcc = accessors[attributes["JOINTS_0"].GetValue<int>()] as JsonObject, wAcc = accessors[attributes["WEIGHTS_0"].GetValue<int>()] as JsonObject;
				if (jAcc["componentType"].GetValue<int>() != 5123 || wAcc["componentType"].GetValue<int>() != 5126) throw new InvalidDataException("the file's joints are not 16-bit or its weights not floats - only a file written by Crystal can be repainted in place");
				int jAt = binAt + Offset(views, jAcc), wAt = binAt + Offset(views, wAcc);
				if (vertex + count > jointIndex.Length / 4) throw new InvalidDataException("the painted weights cover fewer vertices than the file has");
				for (int v = 0; v < count; v++)
				{
					for (int k = 0; k < 4; k++)
					{
						int j = jointIndex[(vertex + v) * 4 + k];
						float w = weights[(vertex + v) * 4 + k];
						BitConverter.GetBytes((ushort)Math.Max(0, j)).CopyTo(result, jAt + (v * 4 + k) * 2);
						BitConverter.GetBytes(j < 0 ? 0f : w).CopyTo(result, wAt + (v * 4 + k) * 4);
					}
				}
				vertex += count;
			}
			if (!jsonChanged) return result;
			// The JSON chunk rewritten (padded to 4 with spaces, as the format asks), the BIN chunk carried over.
			byte[] json = Encoding.UTF8.GetBytes(root.ToJsonString());
			int padded = (json.Length + 3) & ~3;
			byte[] bin = new byte[result.Length - binAt];
			Array.Copy(result, binAt, bin, 0, bin.Length);
			using MemoryStream stream = new MemoryStream();
			using BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(0x46546C67u); writer.Write(2u); writer.Write((uint)(12 + 8 + padded + 8 + bin.Length));
			writer.Write((uint)padded); writer.Write(0x4E4F534Au); writer.Write(json); for (int i = json.Length; i < padded; i++) writer.Write((byte)0x20);
			writer.Write((uint)bin.Length); writer.Write(0x004E4942u); writer.Write(bin);
			writer.Flush();
			return stream.ToArray();
		}

		/// <summary>
		/// What a .glb's normals are: the file's own, or recalculated at an angle (asset.extras.normals),
		/// and whether the file's own are still kept (_SOURCE_NORMAL) for a revert.
		/// </summary>
		public static (float? Angle, bool HasSource) NormalsOf(byte[] glb)
		{
			if (glb.Length < 20 || BitConverter.ToUInt32(glb, 0) != 0x46546C67u) return (null, false);
			JsonNode root = JsonNode.Parse(Encoding.UTF8.GetString(glb, 20, BitConverter.ToInt32(glb, 12)));
			float? angle = root?["asset"]?["extras"]?["normals"]?["angle"]?.GetValue<float>();
			bool source = false;
			foreach (JsonNode mesh in root?["meshes"] as JsonArray ?? new JsonArray())
				foreach (JsonNode p in mesh?["primitives"] as JsonArray ?? new JsonArray())
					if (p?["attributes"]?["_SOURCE_NORMAL"] != null) source = true;
			return (angle, source);
		}

		/// <summary>
		/// A .glb's normals recalculated from its triangles, the way an importer's "calculate normals"
		/// does: a vertex's normal is the area-weighted sum of the normals of the faces around its
		/// position - across the seams where the file split a vertex (a UV island's edge, a
		/// primitive's edge), so those smooth over too - taking in only faces within
		/// <paramref name="angleDegrees"/> of the vertex's own faces' direction, so a crease sharper
		/// than that stays a crease (vertices are not split: a single vertex on a crease takes the
		/// side its own faces lean to). The file's own normals are kept in a _SOURCE_NORMAL attribute
		/// the first time, so RevertNormals can put them back; asset.extras.normals records the angle.
		/// Primitives without NORMAL get one. Triangles only (mode 4, indexed).
		/// </summary>
		public static byte[] RecalculateNormals(byte[] glb, float angleDegrees)
		{
			Open(glb, out JsonNode root, out int binAt);
			JsonArray accessors = (JsonArray)root["accessors"], views = (JsonArray)root["bufferViews"], meshes = (JsonArray)root["meshes"];
			List<JsonObject> primitives = Primitives(root);
			// Every primitive's vertices and triangles, in one pool.
			List<float[]> positions = new List<float[]>();   // per primitive, 3 floats a vertex
			List<int[]> indices = new List<int[]>();
			List<int> bases = new List<int>();
			int total = 0;
			foreach (JsonObject primitive in primitives)
			{
				JsonObject attributes = primitive["attributes"] as JsonObject;
				int mode = primitive["mode"]?.GetValue<int>() ?? 4;
				if (attributes?["POSITION"] == null || primitive["indices"] == null || mode != 4) { positions.Add(null); indices.Add(null); bases.Add(total); continue; }
				JsonObject pAcc = (JsonObject)accessors[attributes["POSITION"].GetValue<int>()];
				positions.Add(ReadFloats(glb, binAt, views, pAcc, 3));
				indices.Add(ReadIndices(glb, binAt, views, (JsonObject)accessors[primitive["indices"].GetValue<int>()]));
				bases.Add(total);
				total += pAcc["count"].GetValue<int>();
			}
			// Face normals (area-weighted: the cross product's length is twice the area) and the
			// faces at each vertex; vertices grouped by position, so twins along a seam share.
			List<double[]> faces = new List<double[]>();          // nx, ny, nz (unnormalised)
			List<int>[] atVertex = new List<int>[total];
			for (int v = 0; v < total; v++) atVertex[v] = new List<int>();
			double[] all = new double[total * 3];
			for (int p = 0; p < primitives.Count; p++)
			{
				if (positions[p] == null) continue;
				float[] pos = positions[p]; int[] idx = indices[p]; int b = bases[p];
				for (int v = 0; v < pos.Length / 3; v++) { all[(b + v) * 3] = pos[v * 3]; all[(b + v) * 3 + 1] = pos[v * 3 + 1]; all[(b + v) * 3 + 2] = pos[v * 3 + 2]; }
				for (int t = 0; t + 2 < idx.Length; t += 3)
				{
					int a = b + idx[t], c = b + idx[t + 1], d = b + idx[t + 2];
					double ux = all[c * 3] - all[a * 3], uy = all[c * 3 + 1] - all[a * 3 + 1], uz = all[c * 3 + 2] - all[a * 3 + 2];
					double vx = all[d * 3] - all[a * 3], vy = all[d * 3 + 1] - all[a * 3 + 1], vz = all[d * 3 + 2] - all[a * 3 + 2];
					double[] n = { uy * vz - uz * vy, uz * vx - ux * vz, ux * vy - uy * vx };
					int f = faces.Count;
					faces.Add(n);
					atVertex[a].Add(f); atVertex[c].Add(f); atVertex[d].Add(f);
				}
			}
			Dictionary<(long, long, long), List<int>> byPosition = new Dictionary<(long, long, long), List<int>>();
			for (int v = 0; v < total; v++)
			{
				(long, long, long) key = ((long)Math.Round(all[v * 3] * 1e5), (long)Math.Round(all[v * 3 + 1] * 1e5), (long)Math.Round(all[v * 3 + 2] * 1e5));
				if (!byPosition.TryGetValue(key, out List<int> list)) byPosition[key] = list = new List<int>();
				list.Add(v);
			}
			double cosLimit = Math.Cos(Math.Max(0, Math.Min(180, angleDegrees)) * Math.PI / 180);
			float[] result = new float[total * 3];
			double[] Unit(double[] n) { double l = Math.Sqrt(n[0] * n[0] + n[1] * n[1] + n[2] * n[2]); return l < 1e-20 ? new[] { 0.0, 0.0, 0.0 } : new[] { n[0] / l, n[1] / l, n[2] / l }; }
			for (int v = 0; v < total; v++)
			{
				// The vertex's own direction: its own faces' sum.
				double[] own = new double[3];
				foreach (int f in atVertex[v]) { own[0] += faces[f][0]; own[1] += faces[f][1]; own[2] += faces[f][2]; }
				double[] dir = Unit(own);
				double[] sum = new double[3];
				(long, long, long) key = ((long)Math.Round(all[v * 3] * 1e5), (long)Math.Round(all[v * 3 + 1] * 1e5), (long)Math.Round(all[v * 3 + 2] * 1e5));
				HashSet<int> seen = new HashSet<int>();
				foreach (int twin in byPosition[key])
					foreach (int f in atVertex[twin])
					{
						if (!seen.Add(f)) continue;
						double[] u = Unit(faces[f]);
						double dot = u[0] * dir[0] + u[1] * dir[1] + u[2] * dir[2];
						if (dot >= cosLimit - 1e-9) { sum[0] += faces[f][0]; sum[1] += faces[f][1]; sum[2] += faces[f][2]; }
					}
				double[] n = Unit(sum);
				if (n[0] == 0 && n[1] == 0 && n[2] == 0) n = dir[0] == 0 && dir[1] == 0 && dir[2] == 0 ? new[] { 0.0, 1.0, 0.0 } : dir;
				result[v * 3] = (float)n[0]; result[v * 3 + 1] = (float)n[1]; result[v * 3 + 2] = (float)n[2];
			}
			// Written: NORMAL in place where it is float3, else a new accessor; the file's own kept
			// once as _SOURCE_NORMAL (a copy appended to the BIN).
			byte[] bin = new byte[glb.Length - binAt];
			Array.Copy(glb, binAt, bin, 0, bin.Length);
			MemoryStream grown = new MemoryStream();
			grown.Write(bin, 0, bin.Length);
			for (int p = 0; p < primitives.Count; p++)
			{
				if (positions[p] == null) continue;
				JsonObject attributes = (JsonObject)primitives[p]["attributes"];
				int count = positions[p].Length / 3, b = bases[p];
				byte[] fresh = new byte[count * 12];
				Buffer.BlockCopy(result, b * 12, fresh, 0, fresh.Length);
				if (attributes["NORMAL"] != null)
				{
					JsonObject nAcc = (JsonObject)accessors[attributes["NORMAL"].GetValue<int>()];
					if (attributes["_SOURCE_NORMAL"] == null)
					{
						// The file's own, kept: read as floats whatever their type, appended.
						float[] source = ReadFloats(glb, binAt, views, nAcc, 3);
						attributes["_SOURCE_NORMAL"] = Append(root, grown, source, 3);
					}
					if (TightFloat3(views, nAcc))
					{
						int at = Offset(views, nAcc);
						grown.Position = at; grown.Write(fresh, 0, fresh.Length); grown.Position = grown.Length;
						nAcc.Remove("min"); nAcc.Remove("max");
						continue;
					}
				}
				// Not float3 laid tight (interleaved, or short ints): a fresh accessor of ours, which is, so the next pass writes in place.
				attributes["NORMAL"] = Append(root, grown, result.Skip(b * 3).Take(count * 3).ToArray(), 3);
			}
			JsonObject asset = root["asset"] as JsonObject ?? new JsonObject();
			root["asset"] = asset;
			JsonObject extras = asset["extras"] as JsonObject ?? new JsonObject();
			asset["extras"] = extras;
			extras["normals"] = new JsonObject { ["angle"] = angleDegrees };
			return Close(root, grown.ToArray());
		}

		/// <summary>
		/// The file's own normals back from _SOURCE_NORMAL, copied into NORMAL's place (the copy stays,
		/// so applying again writes in place and the file does not grow); the record of the angle
		/// gone. Null when there is nothing to revert.
		/// </summary>
		public static byte[] RevertNormals(byte[] glb)
		{
			Open(glb, out JsonNode root, out int binAt);
			JsonArray accessors = (JsonArray)root["accessors"], views = (JsonArray)root["bufferViews"];
			byte[] bin = new byte[glb.Length - binAt];
			Array.Copy(glb, binAt, bin, 0, bin.Length);
			bool any = false;
			foreach (JsonObject primitive in Primitives(root))
			{
				JsonObject attributes = primitive["attributes"] as JsonObject;
				if (attributes?["_SOURCE_NORMAL"] == null) continue;
				JsonObject sAcc = (JsonObject)accessors[attributes["_SOURCE_NORMAL"].GetValue<int>()];
				JsonObject nAcc = attributes["NORMAL"] == null ? null : (JsonObject)accessors[attributes["NORMAL"].GetValue<int>()];
				if (nAcc != null && TightFloat3(views, nAcc) && nAcc["count"].GetValue<int>() == sAcc["count"].GetValue<int>())
				{
					Array.Copy(bin, Offset(views, sAcc), bin, Offset(views, nAcc), sAcc["count"].GetValue<int>() * 12);
					nAcc.Remove("min"); nAcc.Remove("max");
				}
				else attributes["NORMAL"] = attributes["_SOURCE_NORMAL"].GetValue<int>();   // the source accessor is one of this writer's: tight float3
				any = true;
			}
			if (!any) return null;
			JsonObject extras = root["asset"]?["extras"] as JsonObject;
			extras?.Remove("normals");
			return Close(root, bin);
		}

		/// <summary>A float VEC3 accessor laid tight (12 bytes a vertex): one this writer can overwrite in place.</summary>
		private static bool TightFloat3(JsonArray views, JsonObject accessor)
		{
			int stride = Stride(views, accessor);
			return accessor["componentType"].GetValue<int>() == 5126 && accessor["type"].GetValue<string>() == "VEC3" && (accessor["normalized"]?.GetValue<bool>() ?? false) == false && (stride == 0 || stride == 12);
		}

		private static void Open(byte[] glb, out JsonNode root, out int binAt)
		{
			if (glb.Length < 20 || BitConverter.ToUInt32(glb, 0) != 0x46546C67u) throw new InvalidDataException("not a .glb");
			int jsonLength = BitConverter.ToInt32(glb, 12);
			root = JsonNode.Parse(Encoding.UTF8.GetString(glb, 20, jsonLength)) ?? throw new InvalidDataException("no JSON in the .glb");
			binAt = 20 + jsonLength + 8;
			if (binAt > glb.Length || BitConverter.ToUInt32(glb, 20 + jsonLength + 4) != 0x004E4942u) throw new InvalidDataException("the .glb has no BIN chunk after its JSON");
			if (root["accessors"] == null || root["bufferViews"] == null || root["meshes"] == null) throw new InvalidDataException("the .glb has no meshes");
		}

		/// <summary>The JSON and BIN chunks back into one .glb (the JSON padded with spaces, the buffer's length refreshed).</summary>
		private static byte[] Close(JsonNode root, byte[] bin)
		{
			JsonArray buffers = root["buffers"] as JsonArray;
			if (buffers != null && buffers.Count > 0) ((JsonObject)buffers[0])["byteLength"] = bin.Length;
			byte[] json = Encoding.UTF8.GetBytes(root.ToJsonString());
			int padded = (json.Length + 3) & ~3;
			using MemoryStream stream = new MemoryStream();
			using BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(0x46546C67u); writer.Write(2u); writer.Write((uint)(12 + 8 + padded + 8 + bin.Length));
			writer.Write((uint)padded); writer.Write(0x4E4F534Au); writer.Write(json); for (int i = json.Length; i < padded; i++) writer.Write((byte)0x20);
			writer.Write((uint)bin.Length); writer.Write(0x004E4942u); writer.Write(bin);
			writer.Flush();
			return stream.ToArray();
		}

		/// <summary>The primitives in the order the reader visits them: the scene's nodes, depth first.</summary>
		private static List<JsonObject> Primitives(JsonNode root)
		{
			JsonArray meshes = (JsonArray)root["meshes"], nodes = root["nodes"] as JsonArray;
			List<JsonObject> primitives = new List<JsonObject>();
			void Visit(int nodeIndex, int depth)
			{
				if (depth > 64 || nodes == null || nodeIndex < 0 || nodeIndex >= nodes.Count) return;
				JsonObject node = nodes[nodeIndex] as JsonObject;
				if (node?["mesh"] != null)
				{
					int mesh = node["mesh"].GetValue<int>();
					if (mesh >= 0 && mesh < meshes.Count) foreach (JsonNode p in meshes[mesh]["primitives"] as JsonArray ?? new JsonArray()) primitives.Add(p as JsonObject);
				}
				foreach (JsonNode c in node?["children"] as JsonArray ?? new JsonArray()) Visit(c.GetValue<int>(), depth + 1);
			}
			JsonArray scenes = root["scenes"] as JsonArray;
			int scene = root["scene"]?.GetValue<int>() ?? 0;
			if (scenes != null && scene < scenes.Count) foreach (JsonNode n in scenes[scene]["nodes"] as JsonArray ?? new JsonArray()) Visit(n.GetValue<int>(), 0);
			else if (nodes != null) for (int i = 0; i < nodes.Count; i++) Visit(i, 0);
			return primitives;
		}

		/// <summary>An accessor's data as floats, <paramref name="width"/> a vertex, whatever its component type (normalised ints scaled).</summary>
		private static float[] ReadFloats(byte[] glb, int binAt, JsonArray views, JsonObject accessor, int width)
		{
			int count = accessor["count"].GetValue<int>(), type = accessor["componentType"].GetValue<int>();
			bool normalized = accessor["normalized"]?.GetValue<bool>() ?? false;
			int size = type == 5126 || type == 5125 ? 4 : type == 5123 || type == 5122 ? 2 : 1;
			int stride = Stride(views, accessor);
			if (stride == 0) stride = size * width;
			int at = binAt + Offset(views, accessor);
			float[] out_ = new float[count * width];
			for (int v = 0; v < count; v++)
				for (int k = 0; k < width; k++)
				{
					int o = at + v * stride + k * size;
					float f;
					switch (type)
					{
						case 5126: f = BitConverter.ToSingle(glb, o); break;
						case 5125: f = BitConverter.ToUInt32(glb, o); break;
						case 5123: f = normalized ? BitConverter.ToUInt16(glb, o) / 65535f : BitConverter.ToUInt16(glb, o); break;
						case 5122: f = normalized ? Math.Max(-1f, BitConverter.ToInt16(glb, o) / 32767f) : BitConverter.ToInt16(glb, o); break;
						case 5121: f = normalized ? glb[o] / 255f : glb[o]; break;
						default: f = normalized ? Math.Max(-1f, (sbyte)glb[o] / 127f) : (sbyte)glb[o]; break;
					}
					out_[v * width + k] = f;
				}
			return out_;
		}

		private static int[] ReadIndices(byte[] glb, int binAt, JsonArray views, JsonObject accessor)
		{
			int count = accessor["count"].GetValue<int>(), type = accessor["componentType"].GetValue<int>();
			int at = binAt + Offset(views, accessor);
			int[] out_ = new int[count];
			for (int i = 0; i < count; i++) out_[i] = type == 5125 ? (int)BitConverter.ToUInt32(glb, at + i * 4) : type == 5123 ? BitConverter.ToUInt16(glb, at + i * 2) : glb[at + i];
			return out_;
		}

		private static int Stride(JsonArray views, JsonObject accessor)
		{
			JsonObject view = views[accessor["bufferView"].GetValue<int>()] as JsonObject;
			return view?["byteStride"]?.GetValue<int>() ?? 0;
		}

		/// <summary>Floats appended to the BIN as a tight view and a float accessor of <paramref name="width"/>; the accessor's index.</summary>
		private static int Append(JsonNode root, MemoryStream bin, float[] data, int width)
		{
			JsonArray accessors = (JsonArray)root["accessors"], views = (JsonArray)root["bufferViews"];
			while (bin.Length % 4 != 0) bin.WriteByte(0);
			int offset = (int)bin.Length;
			byte[] bytes = new byte[data.Length * 4];
			Buffer.BlockCopy(data, 0, bytes, 0, bytes.Length);
			bin.Position = bin.Length; bin.Write(bytes, 0, bytes.Length);
			views.Add(new JsonObject { ["buffer"] = 0, ["byteOffset"] = offset, ["byteLength"] = bytes.Length });
			accessors.Add(new JsonObject { ["bufferView"] = views.Count - 1, ["componentType"] = 5126, ["count"] = data.Length / width, ["type"] = width == 3 ? "VEC3" : width == 2 ? "VEC2" : width == 4 ? "VEC4" : "SCALAR" });
			return accessors.Count - 1;
		}

		private static int Offset(JsonArray views, JsonObject accessor)
		{
			JsonObject view = views[accessor["bufferView"].GetValue<int>()] as JsonObject;
			return (view?["byteOffset"]?.GetValue<int>() ?? 0) + (accessor["byteOffset"]?.GetValue<int>() ?? 0);
		}

		/// <summary>a then b, both 4x3 row-vector (v' = v a b).</summary>
		internal static float[] Mul(float[] a, float[] b)
		{
			float[] r = new float[12];
			for (int i = 0; i < 4; i++)
			{
				for (int k = 0; k < 3; k++)
				{
					r[i * 3 + k] = a[i * 3] * b[k] + a[i * 3 + 1] * b[3 + k] + a[i * 3 + 2] * b[6 + k];
				}
			}
			for (int k = 0; k < 3; k++) r[9 + k] += b[9 + k];
			return r;
		}

		/// <summary>The inverse of a 4x3 row-vector affine matrix (a general 3x3 part, then the translation).</summary>
		internal static float[] Invert(float[] m)
		{
			double a = m[0], b = m[1], c = m[2], d = m[3], e = m[4], f = m[5], g = m[6], h = m[7], i = m[8];
			double det = a * (e * i - f * h) - b * (d * i - f * g) + c * (d * h - e * g);
			if (Math.Abs(det) < 1e-12) det = det < 0 ? -1e-12 : 1e-12;
			double[] inv =
			{
				(e * i - f * h) / det, (c * h - b * i) / det, (b * f - c * e) / det,
				(f * g - d * i) / det, (a * i - c * g) / det, (c * d - a * f) / det,
				(d * h - e * g) / det, (b * g - a * h) / det, (a * e - b * d) / det
			};
			float[] r = new float[12];
			for (int k = 0; k < 9; k++) r[k] = (float)inv[k];
			// t' = -t x R^-1
			for (int k = 0; k < 3; k++)
			{
				r[9 + k] = (float)-(m[9] * inv[k] + m[10] * inv[3 + k] + m[11] * inv[6 + k]);
			}
			return r;
		}

		/// <summary>A 4x3 row-vector matrix as a glTF column-major 4x4 (the rows of the one are the columns of the other).</summary>
		private static void PutMatrix(byte[] into, int at, float[] m)
		{
			Put(into, at, m[0], m[1], m[2], 0);
			Put(into, at + 16, m[3], m[4], m[5], 0);
			Put(into, at + 32, m[6], m[7], m[8], 0);
			Put(into, at + 48, m[9], m[10], m[11], 1);
		}

		/// <summary>A 4x3 row-vector matrix (rows then translation) into T, unit quaternion, S.</summary>
		private static void Decompose(IReadOnlyList<float> m, int at, out float[] translation, out float[] quaternion, out float[] scale)
		{
			// Rows of the 3x3 are the images of the axes (v' = v * R), so the columns of
			// the column-vector rotation are these rows.
			double[] c0 = { m[at], m[at + 1], m[at + 2] };
			double[] c1 = { m[at + 3], m[at + 4], m[at + 5] };
			double[] c2 = { m[at + 6], m[at + 7], m[at + 8] };
			double sx = Length(c0), sy = Length(c1), sz = Length(c2);
			if (sx < 1e-9) sx = 1e-9;
			if (sy < 1e-9) sy = 1e-9;
			if (sz < 1e-9) sz = 1e-9;
			// A negative determinant means a mirrored axis; fold it into one scale.
			double det = c0[0] * (c1[1] * c2[2] - c1[2] * c2[1]) - c0[1] * (c1[0] * c2[2] - c1[2] * c2[0]) + c0[2] * (c1[0] * c2[1] - c1[1] * c2[0]);
			if (det < 0) sz = -sz;
			double[][] r =
			{
				new[] { c0[0] / sx, c1[0] / sy, c2[0] / sz },
				new[] { c0[1] / sx, c1[1] / sy, c2[1] / sz },
				new[] { c0[2] / sx, c1[2] / sy, c2[2] / sz }
			};
			double trace = r[0][0] + r[1][1] + r[2][2];
			double qw, qx, qy, qz;
			if (trace > 0)
			{
				double s = Math.Sqrt(trace + 1) * 2;
				qw = 0.25 * s; qx = (r[2][1] - r[1][2]) / s; qy = (r[0][2] - r[2][0]) / s; qz = (r[1][0] - r[0][1]) / s;
			}
			else if (r[0][0] > r[1][1] && r[0][0] > r[2][2])
			{
				double s = Math.Sqrt(1 + r[0][0] - r[1][1] - r[2][2]) * 2;
				qw = (r[2][1] - r[1][2]) / s; qx = 0.25 * s; qy = (r[0][1] + r[1][0]) / s; qz = (r[0][2] + r[2][0]) / s;
			}
			else if (r[1][1] > r[2][2])
			{
				double s = Math.Sqrt(1 + r[1][1] - r[0][0] - r[2][2]) * 2;
				qw = (r[0][2] - r[2][0]) / s; qx = (r[0][1] + r[1][0]) / s; qy = 0.25 * s; qz = (r[1][2] + r[2][1]) / s;
			}
			else
			{
				double s = Math.Sqrt(1 + r[2][2] - r[0][0] - r[1][1]) * 2;
				qw = (r[1][0] - r[0][1]) / s; qx = (r[0][2] + r[2][0]) / s; qy = (r[1][2] + r[2][1]) / s; qz = 0.25 * s;
			}
			double n = Math.Sqrt(qx * qx + qy * qy + qz * qz + qw * qw);
			if (n < 1e-12) { qx = qy = qz = 0; qw = 1; n = 1; }
			translation = new[] { m[at + 9], m[at + 10], m[at + 11] };
			quaternion = new[] { (float)(qx / n), (float)(qy / n), (float)(qz / n), (float)(qw / n) };
			scale = new[] { (float)sx, (float)sy, (float)sz };
		}

		private static double Length(double[] v) => Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);

		private static void Put(byte[] into, int at, params float[] values)
		{
			for (int i = 0; i < values.Length; i++)
			{
				BitConverter.GetBytes(values[i]).CopyTo(into, at + 4 * i);
			}
		}

		private static void Align(BinaryWriter bin, int to)
		{
			while (bin.BaseStream.Position % to != 0)
			{
				bin.Write((byte)0);
			}
		}
	}
}
