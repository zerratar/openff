// A mod's glTF in place of one of the game's models - a character (j101), a monster (f028) -
// with the game's own skeleton and motions driving it.
//
// The game keeps everything about the model but its look: the .nmdp still loads, the .ncap
// motions still play on its node tree, the weapon still hangs from R_te, the shadow, the alpha
// and the LOD are as before. What changes is the draw. When a render object is set up with a
// model whose name a definition (defs/models/<name>.json, Shared/Data/ModModels.cs) claims,
// the object gets a stand-in (CRenderObject.StandIn). Each frame the stand-in walks the
// model's SBC exactly as NNS_G3dDraw would - the animation applied, every node's matrix built
// in camera space - with the shapes masked off and a NODEDESC callback writing each node's
// matrix down; then it skins the glTF on the CPU (each vertex through up to four joints, the
// file's inverse bind matrices times those node matrices, weights as Blender painted them) and
// draws the triangles through NativeRenderer with the scene's projection, in the same opaque
// and translucent passes as the game's own shapes.
//
// The glTF's joints are matched to the model's nodes by name, which the export from Crystal's
// model viewer gives them (hara, mune, L_ude, R_te...); a joint the model has no node for is
// left out of the blend. The inverse bind matrices are the file's, so the armature must stay
// where the export put it - the same rule Mdl0Reskin has. A mesh without a skin rides on the
// model's first node.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenFF.Data;
using OpenFF.Graphics;

namespace OpenFF.Client
{
	using Color = Microsoft.Xna.Framework.Color;
	using Vector3 = Microsoft.Xna.Framework.Vector3;

	internal static class CharacterMeshes
	{
		private sealed class Look
		{
			public string Model;
			public string Path;
			public GltfModel Mesh;
			public bool Failed;
			/// <summary>Per model resource this look was bound to: the joint -> node map and the frame's buffers.</summary>
			public Dictionary<GlobalScope.NNSG3dResMdl, Binding> Bindings = new Dictionary<GlobalScope.NNSG3dResMdl, Binding>();
		}

		private sealed class Binding
		{
			public int NodeCount;
			/// <summary>Per skin, per joint: the model's node index, or -1.</summary>
			public int[][] JointNode;
			/// <summary>Per skin, per joint: the file's inverse bind matrix.</summary>
			public Matrix[][] InverseBind;
			/// <summary>The frame's node matrices (camera space) and which nodes the walk reached.</summary>
			public Matrix[] Nodes;
			public bool[] Seen;
			/// <summary>Per mesh: the frame's skinned triangle list, reused.</summary>
			public VertexPositionColorTexture[][] Skinned;
			/// <summary>Per mesh: each unique vertex's skinned position and shade, reused.</summary>
			public Vector3[][] Positions;
			public float[][] Shades;
			public int Draws;
		}

		private static readonly Dictionary<string, Look> _looks = new Dictionary<string, Look>(StringComparer.OrdinalIgnoreCase);

		/// <summary>The definitions in play (ModItemsLayer.Register): one look per game model named.</summary>
		public static void Register(IEnumerable<ModModel> models)
		{
			_looks.Clear();
			foreach (ModModel model in models ?? Array.Empty<ModModel>())
			{
				if (string.IsNullOrWhiteSpace(model.Model) || model.GltfPath == null) continue;
				if (_looks.ContainsKey(model.Model)) { Log.Write(LogChannel.General, "models: " + model.Model + " is given a glTF twice - the first (" + _looks[model.Model].Path + ") stands"); continue; }
				_looks[model.Model] = new Look { Model = model.Model, Path = model.GltfPath };
				Log.Write(LogChannel.General, "models: " + model.Model + " looks like " + model.Gltf + (File.Exists(model.GltfPath) ? "" : " - no such file"));
			}
		}

		/// <summary>Whether a game model has a glTF look.</summary>
		public static bool Has(string modelName) => modelName != null && _looks.ContainsKey(modelName);

		/// <summary>
		/// A render object was set up with a model (CRenderObject.setup): if a definition names it,
		/// the object draws the glTF from now on.
		/// </summary>
		public static void Attach(GlobalScope.ds.sys3d.CRenderObject ro, GlobalScope.NNSG3dResMdl mdl)
		{
			if (ro == null || mdl == null || mdl.name == null || !_looks.TryGetValue(mdl.name, out Look look)) return;
			if (look.Mesh == null && !look.Failed) Load(look);
			if (look.Mesh == null) return;
			if (!look.Bindings.TryGetValue(mdl, out Binding binding))
			{
				binding = Bind(look, mdl);
				look.Bindings[mdl] = binding;
			}
			ro.StandIn = r => Draw(look, binding, r);
			Log.First(LogChannel.File, "models-attached-" + look.Model, 3, () => "models: " + look.Model + " drawn as " + Path.GetFileName(look.Path));
		}

		private static void Load(Look look)
		{
			try
			{
				if (!File.Exists(look.Path))
				{
					look.Failed = true;
					Log.Write(LogChannel.General, "models: " + look.Path + ": no such file - " + look.Model + " stays the game's");
					return;
				}
				GraphicsDevice device = GlobalScope.m_Graphics.GetGraphicsDeviceManager().GraphicsDevice;
				look.Mesh = GltfModel.Load(look.Path, device);
				// GltfModel keeps the file only when it has animations; the skin is wanted regardless.
				if (look.Mesh.File == null) look.Mesh.File = GltfFile.Load(look.Path);
				if (look.Mesh.Problem != null) Log.Write(LogChannel.General, "models: " + Path.GetFileName(look.Path) + ": " + look.Mesh.Problem);
				GltfFile file = look.Mesh.File;
				int skinned = 0;
				foreach (GltfMesh mesh in file.Meshes) if (mesh.Skin >= 0 && mesh.Joints != null) skinned++;
				Log.Write(LogChannel.General, "models: " + Path.GetFileName(look.Path) + " for " + look.Model + ": " + look.Mesh.Triangles.ToString("N0") + " triangles, " + file.Meshes.Count + " part(s), " + skinned + " skinned, " + file.Skins.Count + " skin(s)" + (skinned == 0 ? " - no weights: the mesh rides on the model's root" : ""));
			}
			catch (Exception ex)
			{
				look.Failed = true;
				Log.Write(LogChannel.General, "models: " + Path.GetFileName(look.Path) + ": " + ex.Message);
			}
		}

		/// <summary>The joint -> node map for one model resource, by name, and the buffers a frame fills.</summary>
		private static Binding Bind(Look look, GlobalScope.NNSG3dResMdl mdl)
		{
			GltfFile file = look.Mesh.File;
			int count = mdl.nodeInfo?.dict?.numEntry ?? 0;
			Dictionary<string, int> nodeIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < count; i++)
			{
				string name = mdl.nodeInfo.dict.entry.name[i]?.name?.TrimEnd('\0', ' ');
				if (!string.IsNullOrEmpty(name) && !nodeIndex.ContainsKey(name)) nodeIndex[name] = i;
			}
			Binding binding = new Binding
			{
				NodeCount = count,
				Nodes = new Matrix[Math.Max(1, count)],
				Seen = new bool[Math.Max(1, count)],
				JointNode = new int[file.Skins.Count][],
				InverseBind = new Matrix[file.Skins.Count][],
				Skinned = new VertexPositionColorTexture[file.Meshes.Count][],
				Positions = new Vector3[file.Meshes.Count][],
				Shades = new float[file.Meshes.Count][]
			};
			List<string> unknown = new List<string>();
			for (int s = 0; s < file.Skins.Count; s++)
			{
				GltfSkin skin = file.Skins[s];
				binding.JointNode[s] = new int[skin.Joints.Length];
				binding.InverseBind[s] = new Matrix[skin.Joints.Length];
				for (int j = 0; j < skin.Joints.Length; j++)
				{
					string jointName = skin.Joints[j] >= 0 && skin.Joints[j] < file.Nodes.Count ? file.Nodes[skin.Joints[j]].Name : null;
					binding.JointNode[s][j] = jointName != null && nodeIndex.TryGetValue(jointName, out int node) ? node : -1;
					if (binding.JointNode[s][j] < 0 && jointName != null && !unknown.Contains(jointName)) unknown.Add(jointName);
					float[] m = skin.InverseBind[j];
					binding.InverseBind[s][j] = new Matrix(m[0], m[1], m[2], m[3], m[4], m[5], m[6], m[7], m[8], m[9], m[10], m[11], m[12], m[13], m[14], m[15]);
				}
			}
			if (unknown.Count > 0) Log.Write(LogChannel.General, "models: " + Path.GetFileName(look.Path) + ": bones " + look.Model + " does not have - " + string.Join(", ", unknown) + " - are left out of the blend");
			return binding;
		}

		private static readonly GlobalScope.MtxFx43 _current = new GlobalScope.MtxFx43();
		private static readonly float[] _floats = new float[16];

		private static Matrix ToMatrix(GlobalScope.MtxFx43 m)
		{
			float[] a = _floats;
			GlobalScope.MTX_Copy43ToGLfloat(m, a);
			return new Matrix(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], a[10], a[11], a[12], a[13], a[14], a[15]);
		}

		/// <summary>The stand-in's draw: the model's joints this frame, the glTF skinned through them, drawn in the scene's passes.</summary>
		private static void Draw(Look look, Binding binding, GlobalScope.ds.sys3d.CRenderObject ro)
		{
			if (!NativeRenderer.Enabled || look.Mesh == null) return;
			try
			{
				int alphaRate = Math.Max(0, Math.Min(100, ro.getAlphaRate()));
				if (alphaRate == 0) return;
				uint pass = GlobalScope.NNS_G3dGetDrawMask();
				// The joints: the SBC walked with the shapes masked off, each NODEDESC's matrix kept.
				// Another callback on the object (the battle's hand-joint store) keeps getting its calls.
				GlobalScope.NNSG3dRenderObj obj = ro.RenderObj;
				GlobalScope.NNSG3dSbcCallBackFunc previous = obj.cbFunc;
				byte previousCmd = obj.cbCmd, previousTiming = obj.cbTiming;
				Array.Clear(binding.Seen, 0, binding.Seen.Length);
				obj.cbFunc = rs =>
				{
					int node = GlobalScope.NNS_G3dRSGetCurrentNodeDescID(rs);
					if (node >= 0 && node < binding.NodeCount)
					{
						GlobalScope.NNS_G3dGetCurrentMtx(_current, null);
						binding.Nodes[node] = ToMatrix(_current);
						binding.Seen[node] = true;
					}
					if (previous != null && previousCmd == 6) previous(rs);
				};
				obj.cbCmd = 6;
				GlobalScope.NNS_G3dSetDrawMask(3);
				try
				{
					GlobalScope.NNS_G3dDraw(obj);
				}
				finally
				{
					GlobalScope.NNS_G3dSetDrawMask(pass);
					obj.cbFunc = previous;
					obj.cbCmd = previousCmd;
					obj.cbTiming = previousTiming;
				}

				GraphicsDevice device = GlobalScope.m_Graphics.GetGraphicsDeviceManager().GraphicsDevice;
				Matrix projection = GlobalScope.m_Graphics.getBasicEffect().Projection;
				GltfFile file = look.Mesh.File;
				Matrix root = binding.NodeCount > 0 && binding.Seen[0] ? binding.Nodes[0] : Matrix.Identity;
				if (binding.Draws++ == 0) Log.Write(LogChannel.File, "models: " + look.Model + " first drawn as " + Path.GetFileName(look.Path) + " - " + CountSeen(binding) + " of " + binding.NodeCount + " joints reached");

				GlobalScope.NNSG3dResMat materials = GlobalScope.NNS_G3dGetMat(ro.ModelRes);
				for (int i = 0; i < file.Meshes.Count && i < look.Mesh.Primitives.Count; i++)
				{
					GltfMesh mesh = file.Meshes[i];
					GltfPrimitive primitive = look.Mesh.Primitives[i];
					bool translucent = primitive.Translucent || alphaRate < 100;
					if (pass == 1 && translucent) continue;    // the opaque pass
					if (pass == 2 && !translucent) continue;   // the translucent pass
					GltfMaterial material = mesh.Material >= 0 && mesh.Material < file.Materials.Count ? file.Materials[mesh.Material] : null;
					Color tint = GameColour(materials, i, material);
					VertexPositionColorTexture[] vertices = Skin(binding, i, mesh, primitive, root, tint, alphaRate / 100f);
					NativeRenderer.Draw(device, 4u, vertices, 0, vertices.Length, Matrix.Identity, projection, primitive.Texture,
						TextureFilter.Linear, TextureAddressMode.Wrap, TextureAddressMode.Wrap,
						alphaTest: !translucent, alphaReference: 0.5f, alphaFunction: CompareFunction.Greater,
						depthTest: true, depthWrite: !translucent, depthFunction: CompareFunction.LessEqual,
						cull: false, cullMode: CullMode.None, destinationBlend: Blend.InverseSourceAlpha);
				}
			}
			catch (Exception ex)
			{
				Log.First(LogChannel.General, "models-draw-" + look.Model, 3, () => "models: " + look.Model + ": draw: " + ex.Message);
			}
		}

		/// <summary>
		/// The colour the game gives a part's vertices: DrawModel's RET pass, for the original
		/// model's material of the same index (the export keeps the order: m0, m1, m2 - eye,
		/// mouth, body on a character), from the scene's light - lit materials take
		/// light x diffuse / 4 + light x ambient / 4 + emission x 8, clamped; an unlit one with an
		/// emission takes that; the rest the diffuse as the material's colour - times the glTF
		/// material's base colour, so a tint painted in Blender still counts. This is what makes a
		/// character exactly as bright as the game's beside it, and dim with it in a dark place.
		/// </summary>
		private static Color GameColour(GlobalScope.NNSG3dResMat materials, int index, GltfMaterial material)
		{
			float r = 1f, g = 1f, b = 1f, a = 1f;
			int count = materials?.mat?.Length ?? 0;
			if (count > 0)
			{
				GlobalScope.NNSG3dResMatData m = materials.mat[Math.Min(index, count - 1)];
				uint light = GlobalScope.NNS_G3dGlb.lightColor[0];
				int alpha = (int)((m.polyAttr >> 16) & 0x1F);
				a = alpha == 31 ? 1f : alpha / 31f;
				if ((m.polyAttr & 1) != 0)
				{
					float R(int shift) => Math.Min(255f, ((light >> shift) & 0x1F) * ((m.diffAmb >> shift) & 0x1F) * 0.25f + ((light >> shift) & 0x1F) * ((m.diffAmb >> (16 + shift)) & 0x1F) * 0.25f + ((m.specEmi >> (16 + shift)) & 0x1F) * 8f) / 255f;
					r = R(0); g = R(5); b = R(10);
				}
				else if ((m.specEmi & 0xFFFF0000u) != 0)
				{
					r = ((m.specEmi >> 16) & 0x1F) * 8 / 255f; g = ((m.specEmi >> 21) & 0x1F) * 8 / 255f; b = ((m.specEmi >> 26) & 0x1F) * 8 / 255f;
				}
				else
				{
					r = (m.diffAmb & 0x1F) * 8 / 255f; g = ((m.diffAmb >> 5) & 0x1F) * 8 / 255f; b = ((m.diffAmb >> 10) & 0x1F) * 8 / 255f;
				}
			}
			if (material?.BaseColour != null)
			{
				r *= material.BaseColour[0]; g *= material.BaseColour[1]; b *= material.BaseColour[2]; a *= material.BaseColour[3];
			}
			return new Color(Clamp(r * 255f), Clamp(g * 255f), Clamp(b * 255f), Clamp(a * 255f));
		}

		private static int CountSeen(Binding binding)
		{
			int n = 0;
			foreach (bool s in binding.Seen) if (s) n++;
			return n;
		}

		/// <summary>
		/// One mesh's triangle list for the frame: each unique vertex through its joints (the
		/// file's inverse bind times the node's matrix, weighted), the normal likewise for a shade,
		/// then the list laid out by the indices with the material's colour and the texture coordinates.
		/// </summary>
		private static VertexPositionColorTexture[] Skin(Binding binding, int index, GltfMesh mesh, GltfPrimitive primitive, Matrix root, Color tint, float alpha)
		{
			int count = mesh.VertexCount;
			Vector3[] positions = binding.Positions[index];
			if (positions == null || positions.Length != count) binding.Positions[index] = positions = new Vector3[count];
			float[] shades = binding.Shades[index];
			if (shades == null || shades.Length != count) binding.Shades[index] = shades = new float[count];
			bool skinned = mesh.Skin >= 0 && mesh.Skin < binding.JointNode.Length && mesh.Joints != null && mesh.Weights != null;

			// The joint matrices for this skin: inverse bind, then the node as the walk built it.
			Matrix[] joints = null;
			if (skinned)
			{
				int[] map = binding.JointNode[mesh.Skin];
				Matrix[] inverse = binding.InverseBind[mesh.Skin];
				joints = new Matrix[map.Length];
				for (int j = 0; j < map.Length; j++)
				{
					int node = map[j];
					joints[j] = node >= 0 && binding.Seen[node] ? inverse[j] * binding.Nodes[node] : inverse[j] * root;
				}
			}
			float[] local = skinned ? mesh.LocalPositions : mesh.Positions;
			float[] normals = skinned ? mesh.LocalNormals : mesh.Normals;
			for (int v = 0; v < count; v++)
			{
				Vector3 p = new Vector3(local[v * 3], local[v * 3 + 1], local[v * 3 + 2]);
				Vector3 n = normals != null ? new Vector3(normals[v * 3], normals[v * 3 + 1], normals[v * 3 + 2]) : Vector3.Zero;
				Vector3 outP = Vector3.Zero, outN = Vector3.Zero;
				if (skinned)
				{
					float total = 0;
					for (int k = 0; k < 4; k++)
					{
						float w = mesh.Weights[v * 4 + k];
						int j = (int)mesh.Joints[v * 4 + k];
						if (w <= 0 || j < 0 || j >= joints.Length) continue;
						outP += Vector3.Transform(p, joints[j]) * w;
						if (normals != null) outN += Vector3.TransformNormal(n, joints[j]) * w;
						total += w;
					}
					if (total <= 0) { outP = Vector3.Transform(p, root); outN = normals != null ? Vector3.TransformNormal(n, root) : Vector3.Zero; }
					else if (Math.Abs(total - 1f) > 0.001f) { outP /= total; }
				}
				else
				{
					outP = Vector3.Transform(p, root);
					if (normals != null) outN = Vector3.TransformNormal(n, root);
				}
				positions[v] = outP;
				// No lighting: the game's own models get none in this port - their vertex colour is
				// the material's, the shading is painted into the textures - and a stand-in lit from
				// anywhere reads darker than the model beside it. The normals are skinned above so a
				// shade can be switched on here if a mod ever asks for one.
				shades[v] = 1f;
			}

			VertexPositionColorTexture[] template = primitive.Vertices;
			VertexPositionColorTexture[] vertices = binding.Skinned[index];
			if (vertices == null || vertices.Length != template.Length) binding.Skinned[index] = vertices = new VertexPositionColorTexture[template.Length];
			// The colour is the game's for the part (GameColour), as the game's own vertices get it
			// at the RET pass - the template's colour (GltfModel.Vertices, with a scene-mesh shade
			// baked in) is not used, only its texture coordinates. The frame's shade stays 1.
			Color c = new Color(tint.R, tint.G, tint.B, (byte)Math.Round(tint.A * alpha));
			for (int i = 0; i < template.Length && i < mesh.Indices.Length; i++)
			{
				int v = mesh.Indices[i];
				vertices[i].Position = positions[v];
				vertices[i].TextureCoordinate = template[i].TextureCoordinate;
				vertices[i].Color = shades[v] == 1f ? c : new Color(Clamp(c.R * shades[v]), Clamp(c.G * shades[v]), Clamp(c.B * shades[v]), c.A);
			}
			return vertices;
		}

		private static byte Clamp(float v) => (byte)Math.Max(0, Math.Min(255, Math.Round(v)));
	}
}
