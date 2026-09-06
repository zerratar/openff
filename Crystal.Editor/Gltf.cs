// glTF 2.0 (.glb) out of a model bundle, with its textures and, if asked, one motion.
//
// This is the door to Blender. The viewer's bundle is already what glTF wants - a
// vertex buffer, triangle indices, groups with a material each - and the way the viewer
// skins it (every vertex names the matrix that moved it) is exactly a glTF skin with one
// joint per matrix and weights of 1. So the export is: one mesh, one primitive per group,
// a skin whose joints are the model's matrix instances (bind = identity, since the
// vertices are already in bind-pose world space), and an animation that keys each joint
// with the per-frame delta the pose endpoint computes, decomposed into translation,
// rotation and scale at 30 frames a second.
//
// Textures are the PNGs the editor already makes, embedded. Nothing here is lossy
// beyond the decompose: the NDS matrices are rotation x scale x translation.
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
		/// null; <paramref name="pose"/> is optional, and <paramref name="motionName"/>
		/// names its animation.
		/// </summary>
		public static byte[] Write(ModelBundle bundle, Func<string, byte[]> texturePng,
			Models.Pose pose = null, string motionName = null)
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

			// ---- the matrix instances: one joint each ----------------------------------------
			// A global index per (group, matrix) pair; the vertex's JOINTS_0 names it.
			List<(int Group, int Local, string Name)> joints = new List<(int, int, string)>();
			Dictionary<(int, int), int> jointOf = new Dictionary<(int, int), int>();
			for (int g = 0; g < bundle.Groups.Count; g++)
			{
				ModelGroup group = bundle.Groups[g];
				int count = Math.Max(1, group.Matrices?.Count ?? 1);
				for (int i = 0; i < count; i++)
				{
					string name = (group.Node ?? ("group" + g)) + (i == 0 ? string.Empty : ".slot" + (group.Slots != null && i - 1 < group.Slots.Count ? group.Slots[i - 1] : i));
					jointOf[(g, i)] = joints.Count;
					joints.Add((g, i, name));
				}
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
				int joint = jointOf.TryGetValue((groupOfVertex[v], local), out int j) ? j : jointOf[(groupOfVertex[v], 0)];
				BitConverter.GetBytes((ushort)joint).CopyTo(jointIds, v * 8);
				Put(weights, v * 16, 1f, 0f, 0f, 0f);
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
							images.Add(new JsonObject { ["bufferView"] = view, ["mimeType"] = "image/png", ["name"] = group.Texture });
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
				primitives.Add(new JsonObject
				{
					["attributes"] = new JsonObject
					{
						["POSITION"] = positionAccessor, ["TEXCOORD_0"] = coordAccessor, ["COLOR_0"] = colourAccessor,
						["JOINTS_0"] = jointAccessor, ["WEIGHTS_0"] = weightAccessor
					},
					["indices"] = indexAccessor,
					["material"] = MaterialFor(group),
					["mode"] = 4,
					["extras"] = new JsonObject { ["shape"] = group.Shape, ["node"] = group.Node, ["hidden"] = group.Hidden }
				});
			}

			// ---- nodes: the model, and a joint per matrix instance -----------------------------------
			JsonArray nodes = new JsonArray();
			nodes.Add(new JsonObject { ["name"] = bundle.Name ?? "model", ["mesh"] = 0, ["skin"] = 0 });
			JsonArray jointNodes = new JsonArray();
			JsonArray rootChildren = new JsonArray();
			foreach ((int group, int local, string name) in joints)
			{
				nodes.Add(new JsonObject { ["name"] = name });
				jointNodes.Add(nodes.Count - 1);
				rootChildren.Add(nodes.Count - 1);
			}
			// The joints hang off an armature root so Blender groups them.
			nodes.Add(new JsonObject { ["name"] = "skeleton", ["children"] = rootChildren });
			int skeletonNode = nodes.Count - 1;

			// inverseBindMatrices: identity for every joint - vertices are already posed.
			byte[] ibm = new byte[joints.Count * 64];
			for (int j = 0; j < joints.Count; j++)
			{
				Put(ibm, j * 64, 1, 0, 0, 0); Put(ibm, j * 64 + 16, 0, 1, 0, 0);
				Put(ibm, j * 64 + 32, 0, 0, 1, 0); Put(ibm, j * 64 + 48, 0, 0, 0, 1);
			}
			int ibmAccessor = Accessor(View(ibm), 5126, joints.Count, "MAT4");

			// ---- the motion, if any -----------------------------------------------------------------
			JsonArray animations = new JsonArray();
			if (pose != null && pose.Frames > 0 && pose.Counts != null)
			{
				int stride = pose.Counts.Sum();
				byte[] times = new byte[pose.Frames * 4];
				for (int f = 0; f < pose.Frames; f++) BitConverter.GetBytes(f / 30f).CopyTo(times, f * 4);
				int timeAccessor = Accessor(View(times), 5126, pose.Frames, "SCALAR", new[] { 0f }, new[] { (pose.Frames - 1) / 30f });

				JsonArray samplersA = new JsonArray();
				JsonArray channels = new JsonArray();
				List<int> offsets = new List<int>();
				int running = 0;
				foreach (int c in pose.Counts) { offsets.Add(running); running += c; }

				for (int j = 0; j < joints.Count; j++)
				{
					(int group, int local, string _) = joints[j];
					if (group >= offsets.Count || local >= pose.Counts[group]) continue;
					byte[] t = new byte[pose.Frames * 12];
					byte[] r = new byte[pose.Frames * 16];
					byte[] s = new byte[pose.Frames * 12];
					for (int f = 0; f < pose.Frames; f++)
					{
						int at = (f * stride + offsets[group] + local) * 12;
						if (at + 12 > pose.Matrices.Count) break;
						Decompose(pose.Matrices, at, out float[] tr, out float[] q, out float[] sc);
						Put(t, f * 12, tr[0], tr[1], tr[2]);
						Put(r, f * 16, q[0], q[1], q[2], q[3]);
						Put(s, f * 12, sc[0], sc[1], sc[2]);
					}
					int tAcc = Accessor(View(t), 5126, pose.Frames, "VEC3");
					int rAcc = Accessor(View(r), 5126, pose.Frames, "VEC4");
					int sAcc = Accessor(View(s), 5126, pose.Frames, "VEC3");
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
				animations.Add(new JsonObject { ["name"] = motionName ?? pose.Name ?? "motion", ["samplers"] = samplersA, ["channels"] = channels });
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

		/// <summary>A 4x3 row-vector matrix (rows then translation) into T, unit quaternion, S.</summary>
		private static void Decompose(List<float> m, int at, out float[] translation, out float[] quaternion, out float[] scale)
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
