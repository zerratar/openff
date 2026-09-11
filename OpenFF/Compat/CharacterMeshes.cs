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
// matrix down; then it skins the glTF on the CPU and draws the triangles through
// NativeRenderer with the scene's projection, in the same opaque and translucent passes as
// the game's own shapes, coloured as the game colours its own vertices (the scene's light
// times the original material).
//
// Two kinds of file:
//
// - The export from Crystal's model viewer, remodelled: its joints are the game's nodes by
//   name (hara, mune, L_ude, R_te...), its rest pose the game's bind pose. Each vertex goes
//   through the file's inverse bind matrices times the game's node matrices, weights as
//   Blender painted them. Exact.
//
// - A rig of another convention - Mixamo's, Tripo's, Rigify's, a hand-made one. Its bones are
//   matched to the game's by a table of the usual names (Hips, Spine1, LeftArm, R_Forearm,
//   mixamorig:Head...) plus the definition's own "bones"; twist and helper bones follow their
//   nearest matched ancestor. The model is scaled to the game model's height and stood on its
//   feet (or as "scale", "rotation", "offset" say). Then, since the two rest poses differ,
//   every bone is turned so it points the way the game's does at bind, and the game's motion
//   is applied as each game node's change from its bind pose - a rotation about the file's
//   own joint, plus the node's displacement. The game's idle, swings and staggers play on a
//   body of quite other proportions; what does not carry over is anything the rig has no
//   bone for.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
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
			public ModModel Definition;
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
			public string[] NodeNames;
			public int[] NodeParents;
			/// <summary>Per skin, per joint: the model's node index, or -1.</summary>
			public int[][] JointNode;
			/// <summary>Per skin, per joint: the file's inverse bind matrix.</summary>
			public Matrix[][] InverseBind;
			/// <summary>The frame's node matrices (camera space) and which nodes the walk reached.</summary>
			public Matrix[] Nodes;
			public bool[] Seen;
			/// <summary>Per skin, per joint: the frame's matrix from the file's local space to camera space.</summary>
			public Matrix[][] JointFrame;
			/// <summary>Per mesh: the frame's skinned triangle list and each unique vertex's position, reused.</summary>
			public VertexPositionColorTexture[][] Skinned;
			public Vector3[][] Positions;
			public int Draws;

			// ---- a rig of another convention, retargeted
			public bool Retarget;
			/// <summary>The fit from the file's space into the game model's: scale, turn, offset.</summary>
			public Matrix Fit = Matrix.Identity;
			/// <summary>Per skin, per joint: inverse bind x rest x fit x (to the joint's origin) x (turned into the game's bind direction).</summary>
			public Matrix[][] JointBase;
			/// <summary>Per skin, per joint: the joint's rest position in the game model's space.</summary>
			public Vector3[][] JointPos;
			/// <summary>Per skin, per joint: the nearest ancestor joint in the skin (-1 for a root), and the joints in an order that puts parents first.</summary>
			public int[][] JointParent;
			public int[][] JointOrder;
			/// <summary>Per skin, per joint: the frame's turn (about the joint) and position, in the game model's space - what a child's position is built from.</summary>
			public Matrix[][] JointTurn;
			public Vector3[][] JointNow;
			/// <summary>Per skin, per joint: the turn of the file's bone into the game bone's direction at rest.</summary>
			public Matrix[][] JointAlign;
			/// <summary>Per node: the game's bind pose in model space, and its inverse.</summary>
			public Matrix[] GameBind, GameBindInverse;
			public bool Prepared;
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
				_looks[model.Model] = new Look { Definition = model, Model = model.Model, Path = model.GltfPath };
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

		// ---- binding a file to a model ------------------------------------------------------------------

		/// <summary>The joint -> node map for one model resource, by name (the game's, or the usual conventions'), and the buffers a frame fills.</summary>
		private static Binding Bind(Look look, GlobalScope.NNSG3dResMdl mdl)
		{
			GltfFile file = look.Mesh.File;
			int count = mdl.nodeInfo?.dict?.numEntry ?? 0;
			Binding binding = new Binding
			{
				NodeCount = count,
				NodeNames = new string[count],
				NodeParents = NodeParents(mdl, count),
				Nodes = new Matrix[Math.Max(1, count)],
				Seen = new bool[Math.Max(1, count)],
				JointNode = new int[file.Skins.Count][],
				InverseBind = new Matrix[file.Skins.Count][],
				JointFrame = new Matrix[file.Skins.Count][],
				Skinned = new VertexPositionColorTexture[file.Meshes.Count][],
				Positions = new Vector3[file.Meshes.Count][]
			};
			Dictionary<string, int> nodeIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < count; i++)
			{
				string name = mdl.nodeInfo.dict.entry.name[i]?.name?.TrimEnd('\0', ' ');
				binding.NodeNames[i] = name ?? ("node" + i);
				if (!string.IsNullOrEmpty(name) && !nodeIndex.ContainsKey(name)) nodeIndex[name] = i;
			}

			// First by the game's own names: the export's joints. Enough of them and the file is
			// a remodelled export, taken as it is; otherwise it is another rig, retargeted.
			int joints = 0, exact = 0;
			for (int s = 0; s < file.Skins.Count; s++)
			{
				GltfSkin skin = file.Skins[s];
				binding.JointNode[s] = new int[skin.Joints.Length];
				binding.InverseBind[s] = new Matrix[skin.Joints.Length];
				binding.JointFrame[s] = new Matrix[skin.Joints.Length];
				for (int j = 0; j < skin.Joints.Length; j++)
				{
					string jointName = JointName(file, skin, j);
					binding.JointNode[s][j] = jointName != null && nodeIndex.TryGetValue(jointName, out int node) ? node : -1;
					if (binding.JointNode[s][j] >= 0) exact++;
					joints++;
					float[] m = skin.InverseBind[j];
					binding.InverseBind[s][j] = new Matrix(m[0], m[1], m[2], m[3], m[4], m[5], m[6], m[7], m[8], m[9], m[10], m[11], m[12], m[13], m[14], m[15]);
				}
			}
			// --retarget-force: the export's own rig through the retarget path, which must then come
			// out as the direct one (a check of the retarget's maths; Testing.md).
			// A fitted skeleton (Crystal's fitted auto-rig) has every bone the game's by name, but its
			// joints sit where the file has them, so it too is driven by retargeting.
			binding.Retarget = joints > 0 && (exact * 2 < joints || (look.Definition != null && look.Definition.Fitted) || Options.Get("retarget-force") != null);
			if (!binding.Retarget)
			{
				List<string> unknown = new List<string>();
				for (int s = 0; s < file.Skins.Count; s++)
					for (int j = 0; j < binding.JointNode[s].Length; j++)
						if (binding.JointNode[s][j] < 0) { string n = JointName(file, file.Skins[s], j); if (n != null && !unknown.Contains(n)) unknown.Add(n); }
				if (unknown.Count > 0) Log.Write(LogChannel.General, "models: " + Path.GetFileName(look.Path) + ": bones " + look.Model + " does not have - " + string.Join(", ", unknown) + " - are left out of the blend");
				return binding;
			}

			// Another rig: the table, the definition's own map, then the nearest mapped ancestor.
			List<string> mapped = new List<string>(), unmatched = new List<string>();
			for (int s = 0; s < file.Skins.Count; s++)
			{
				GltfSkin skin = file.Skins[s];
				for (int j = 0; j < skin.Joints.Length; j++)
				{
					string jointName = JointName(file, skin, j);
					int node = -1;
					// The definition's own map first, then the table (so a "Root" goes to trans, the
					// node the game moves for a hop, not to the model's fixed root), then the name itself.
					// A fitted skeleton has the game's own names throughout: those are taken as they are.
					bool fitted = look.Definition != null && look.Definition.Fitted;
					if (jointName != null && look.Definition.Bones.TryGetValue(jointName, out string to) && nodeIndex.TryGetValue(to, out int chosen)) node = chosen;
					else if (fitted && jointName != null && nodeIndex.TryGetValue(jointName, out int exactNode)) node = exactNode;
					else
					{
						string game = Alias(jointName);
						if (game != null && nodeIndex.TryGetValue(game, out int found)) node = found;
						else if (jointName != null && nodeIndex.TryGetValue(jointName, out int same)) node = same;
					}
					binding.JointNode[s][j] = node;
					if (node >= 0) mapped.Add(jointName + ">" + binding.NodeNames[node]);
				}
				// Twist and helper bones: whatever their nearest mapped ancestor got.
				for (int j = 0; j < skin.Joints.Length; j++)
				{
					if (binding.JointNode[s][j] >= 0) continue;
					int node = skin.Joints[j];
					int hops = 0;
					while (node >= 0 && node < file.Nodes.Count && hops++ < 64)
					{
						node = file.Nodes[node].Parent;
						int k = node < 0 ? -1 : Array.IndexOf(skin.Joints, node);
						if (k >= 0 && binding.JointNode[s][k] >= 0) { binding.JointNode[s][j] = binding.JointNode[s][k]; break; }
					}
					if (binding.JointNode[s][j] < 0) unmatched.Add(JointName(file, skin, j) ?? ("joint" + j));
				}
			}
			Log.Write(LogChannel.General, "models: " + Path.GetFileName(look.Path) + " is another rig (" + exact + " of " + joints + " bones named as " + look.Model + "'s): retargeted - " + string.Join(", ", mapped) + (unmatched.Count > 0 ? "; not matched (they ride on the root): " + string.Join(", ", unmatched) : ""));
			return binding;
		}

		private static string JointName(GltfFile file, GltfSkin skin, int j)
		{
			int node = skin.Joints[j];
			return node >= 0 && node < file.Nodes.Count ? file.Nodes[node].Name : null;
		}

		/// <summary>The model's node parents from its SBC's NODEDESC commands (-1 for a root), as Crystal's reader finds them.</summary>
		private static int[] NodeParents(GlobalScope.NNSG3dResMdl mdl, int count)
		{
			int[] parents = new int[count];
			for (int i = 0; i < count; i++) parents[i] = -1;
			byte[] sbc = mdl.sbc;
			if (sbc == null) return parents;
			int at = 0;
			while (at < sbc.Length)
			{
				int op = sbc[at] & 0x1F, flags = sbc[at] & 0xE0;
				int length;
				switch (op)
				{
					case 0: case 1: case 11: length = 1; break;
					case 2: length = 3; break;
					case 3: case 4: case 5: length = 2; break;
					case 6:
						length = 4 + ((flags & 0x20) != 0 ? 1 : 0) + ((flags & 0x40) != 0 ? 1 : 0);
						if (at + 2 < sbc.Length && sbc[at + 1] < count) parents[sbc[at + 1]] = sbc[at + 2] == sbc[at + 1] ? -1 : sbc[at + 2];
						break;
					case 7: case 8: length = 2 + ((flags & 0x20) != 0 ? 1 : 0) + ((flags & 0x40) != 0 ? 1 : 0); break;
					case 9: length = at + 2 < sbc.Length ? 3 + sbc[at + 2] * 3 : 3; break;
					default: return parents;
				}
				if (op == 1) break;
				at += length;
			}
			return parents;
		}

		// ---- the usual bone names -> the game's -------------------------------------------------------------

		private static readonly (string[] Names, string Game)[] Aliases =
		{
			(new[] { "hips", "hip", "pelvis0", "spine", "spine01", "waist", "lowerspine", "torso" }, "hara"),
			(new[] { "pelvis", "hipbone" }, "kosi"),
			(new[] { "spine1", "spine02", "spine2", "spine03", "spine3", "chest", "upperchest", "upperspine", "ribs" }, "mune"),
			(new[] { "neck", "necktwist01", "necktwist1", "neck1", "neck01", "necktwist02" }, "kubi"),
			(new[] { "head", "head1", "skull" }, "atama"),
			(new[] { "shoulder", "clavicle", "collar", "collarbone" }, "sakotu"),
			(new[] { "arm", "upperarm", "uparm", "shoulderarm", "humerus" }, "kata"),
			(new[] { "forearm", "lowerarm", "elbow", "lowarm", "ulna" }, "ude"),
			(new[] { "hand", "wrist", "palm" }, "te"),
			(new[] { "upleg", "thigh", "upperleg", "leg0", "femur", "hipjoint" }, "momo"),
			(new[] { "leg", "calf", "shin", "knee", "lowerleg", "lowleg", "tibia" }, "hiza"),
			(new[] { "foot", "ankle", "toe", "toebase", "toes", "ball" }, "asi"),
			(new[] { "root", "armature", "reference", "origin", "master" }, "trans")
		};

		// A side letter is followed by a separator or a capital (LeftArm, L_Hand, lThigh is not one): the capital check must stay case-sensitive, or Root reads as R + oot.
		private static readonly Regex SidePrefix = new Regex(@"^(left|right|l|r)(?=[_.\- ]|(?-i:[A-Z]))", RegexOptions.IgnoreCase);
		private static readonly Regex SideSuffix = new Regex(@"([_.\- ])(left|right|l|r)$", RegexOptions.IgnoreCase);

		/// <summary>A bone's name of another convention as the game's node name, or null when the table has nothing for it.</summary>
		private static string Alias(string name)
		{
			if (string.IsNullOrWhiteSpace(name)) return null;
			string n = name.Trim();
			int colon = n.LastIndexOf(':');
			if (colon >= 0) n = n.Substring(colon + 1);                       // mixamorig:LeftArm
			if (n.StartsWith("mixamorig", StringComparison.OrdinalIgnoreCase)) n = n.Substring(9).TrimStart('_', '.');
			string side = "";
			Match m = SidePrefix.Match(n);
			if (m.Success) { side = m.Groups[1].Value.Substring(0, 1).ToUpperInvariant(); n = n.Substring(m.Length); }
			else
			{
				m = SideSuffix.Match(n);
				if (m.Success) { side = m.Groups[2].Value.Substring(0, 1).ToUpperInvariant(); n = n.Substring(0, m.Index); }
			}
			string key = Regex.Replace(n, @"[_.\- ]", "").ToLowerInvariant();
			foreach ((string[] names, string game) in Aliases)
			{
				if (Array.IndexOf(names, key) < 0) continue;
				bool sided = game == "sakotu" || game == "kata" || game == "ude" || game == "te" || game == "momo" || game == "hiza" || game == "asi";
				if (sided && side.Length == 0) return null;
				return sided ? side + "_" + game : game;
			}
			return null;
		}

		// ---- the frame ----------------------------------------------------------------------------------------

		private static readonly GlobalScope.MtxFx43 _current = new GlobalScope.MtxFx43();
		private static readonly GlobalScope.MtxFx43 _pose = new GlobalScope.MtxFx43();
		private static readonly float[] _floats = new float[16];

		private static Matrix ToMatrix(GlobalScope.MtxFx43 m)
		{
			float[] a = _floats;
			GlobalScope.MTX_Copy43ToGLfloat(m, a);
			return new Matrix(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], a[10], a[11], a[12], a[13], a[14], a[15]);
		}

		/// <summary>The SBC walked for the frame's node matrices (camera space), the shapes masked off; another callback on the object keeps getting its calls.</summary>
		private static void CollectJoints(Binding binding, GlobalScope.NNSG3dRenderObj obj, bool bindPose)
		{
			GlobalScope.NNSG3dSbcCallBackFunc previous = obj.cbFunc;
			byte previousCmd = obj.cbCmd, previousTiming = obj.cbTiming;
			GlobalScope.NNSG3dAnmObj animations = obj.anmJnt;
			uint mask = GlobalScope.NNS_G3dGetDrawMask();
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
				if (!bindPose && previous != null && previousCmd == 6) previous(rs);
			};
			obj.cbCmd = 6;
			if (bindPose) obj.anmJnt = null;                                  // the tree as the file has it: the bind pose
			GlobalScope.NNS_G3dSetDrawMask(3);
			try
			{
				GlobalScope.NNS_G3dDraw(obj);
			}
			finally
			{
				GlobalScope.NNS_G3dSetDrawMask(mask);
				obj.anmJnt = animations;
				obj.cbFunc = previous;
				obj.cbCmd = previousCmd;
				obj.cbTiming = previousTiming;
			}
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
				GlobalScope.NNSG3dRenderObj obj = ro.RenderObj;
				GltfFile file = look.Mesh.File;

				// The model's placement this frame: what the joints' camera-space matrices are relative to.
				ro.getPoseMtx(_pose);
				Matrix placement = ToMatrix(_pose) * ToMatrix(GlobalScope.NNS_G3dGlb.cameraMtx);
				if (binding.Retarget && !binding.Prepared) Prepare(look, binding, ro.ModelRes, obj, placement);

				CollectJoints(binding, obj, bindPose: false);
				Matrix root = binding.NodeCount > 0 && binding.Seen[0] ? binding.Nodes[0] : placement;
				if (binding.Draws++ == 0) Log.Write(LogChannel.File, "models: " + look.Model + " first drawn as " + Path.GetFileName(look.Path) + " - " + CountSeen(binding) + " of " + binding.NodeCount + " joints reached" + (binding.Retarget ? ", retargeted" : ""));
				JointMatrices(binding, root, placement);

				GraphicsDevice device = GlobalScope.m_Graphics.GetGraphicsDeviceManager().GraphicsDevice;
				Matrix projection = GlobalScope.m_Graphics.getBasicEffect().Projection;
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
					VertexPositionColorTexture[] vertices = Skin(binding, i, mesh, primitive, root, placement, tint, alphaRate / 100f);
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
		/// Once, for a retargeted rig: the game's bind pose in model space (the SBC walked with no
		/// animation), the fit of the file into the model's space, and per joint the matrix that
		/// takes the file's local space to the game's space with the bone turned the game's way.
		/// </summary>
		private static void Prepare(Look look, Binding binding, GlobalScope.NNSG3dResMdl mdl, GlobalScope.NNSG3dRenderObj obj, Matrix placement)
		{
			binding.Prepared = true;
			GltfFile file = look.Mesh.File;
			ModModel definition = look.Definition;
			int count = binding.NodeCount;

			// The game's bind pose, model space.
			CollectJoints(binding, obj, bindPose: true);
			Matrix unplace = Matrix.Invert(placement);
			binding.GameBind = new Matrix[count];
			binding.GameBindInverse = new Matrix[count];
			for (int n = 0; n < count; n++)
			{
				binding.GameBind[n] = binding.Seen[n] ? binding.Nodes[n] * unplace : Matrix.Identity;
				binding.GameBindInverse[n] = Matrix.Invert(binding.GameBind[n]);
			}

			// The fit: the file's height to the model's box, feet on the box's floor, unless the definition says.
			float boxScale = mdl.info.boxPosScale / 4096f / 4096f;
			float boxY = mdl.info.boxY * boxScale, boxH = mdl.info.boxH * boxScale;
			float boxX = mdl.info.boxX * boxScale, boxW = mdl.info.boxW * boxScale, boxZ = mdl.info.boxZ * boxScale, boxD = mdl.info.boxD * boxScale;
			float[][] rest = file.WorldMatrices(null, 0f);
			Vector3 RestPosition(GltfSkin skin, int j) { float[] w = skin.Joints[j] >= 0 && skin.Joints[j] < rest.Length ? rest[skin.Joints[j]] : null; return w == null ? Vector3.Zero : new Vector3(w[12], w[13], w[14]); }
			float[] r = definition.Rotation;
			string facing = "as the definition says";
			if (r == null)
			{
				// Which way the file stands: up from its hips to its head; its front from which side
				// its left-named bones are on against the game's (L_sakotu, L_kata... sit at +x, so
				// the game's characters face +z). A file facing -z is turned half round.
				r = new float[3];
				Vector3 up = Vector3.Zero, leftSum = Vector3.Zero;
				int lefts = 0;
				for (int s = 0; s < file.Skins.Count; s++)
				{
					GltfSkin skin = file.Skins[s];
					Vector3 hips = Vector3.Zero, head = Vector3.Zero;
					bool haveHips = false, haveHead = false;
					for (int j = 0; j < skin.Joints.Length; j++)
					{
						int node = binding.JointNode[s][j];
						if (node < 0) continue;
						string name = binding.NodeNames[node];
						Vector3 p = RestPosition(skin, j);
						if (!haveHips && (name == "hara" || name == "kosi")) { hips = p; haveHips = true; }
						if (!haveHead && name == "atama") { head = p; haveHead = true; }
						if (name.StartsWith("L_", StringComparison.Ordinal)) { leftSum += p; lefts++; }
					}
					if (haveHips && haveHead) up += head - hips;
				}
				Matrix upright = Matrix.Identity;
				if (up.LengthSquared() > 1e-8f)
				{
					Vector3 u = Vector3.Normalize(up);
					if (Math.Abs(u.Z) > Math.Abs(u.Y) && Math.Abs(u.Z) > Math.Abs(u.X)) { r[0] = u.Z > 0 ? -90 : 90; upright = Matrix.CreateRotationX(MathHelper.ToRadians(r[0])); }
					else if (Math.Abs(u.X) > Math.Abs(u.Y)) { r[2] = u.X > 0 ? 90 : -90; upright = Matrix.CreateRotationZ(MathHelper.ToRadians(r[2])); }
					else if (u.Y < 0) { r[0] = 180; upright = Matrix.CreateRotationX(MathHelper.Pi); }
				}
				float gameLeft = 0;
				int gameLefts = 0;
				for (int n = 0; n < count; n++) if (binding.NodeNames[n].StartsWith("L_", StringComparison.Ordinal)) { gameLeft += binding.GameBind[n].Translation.X; gameLefts++; }
				if (lefts > 0 && gameLefts > 0)
				{
					float fileLeft = Vector3.Transform(leftSum / lefts, upright).X;
					// The file's left side against its own middle (the hips), not the origin.
					Vector3 middle = Vector3.Zero; int middles = 0;
					for (int s = 0; s < file.Skins.Count; s++) for (int j = 0; j < file.Skins[s].Joints.Length; j++) { int node = binding.JointNode[s][j]; if (node >= 0 && (binding.NodeNames[node] == "hara" || binding.NodeNames[node] == "kosi")) { middle += RestPosition(file.Skins[s], j); middles++; } }
					if (middles > 0) fileLeft -= Vector3.Transform(middle / middles, upright).X;
					if (Math.Abs(fileLeft) > 1e-4f && Math.Sign(fileLeft) != Math.Sign(gameLeft / gameLefts)) r[1] = 180;
				}
				facing = "found: up " + (r[0] != 0 || r[2] != 0 ? "turned " + r[0] + "/" + r[2] : "as is") + ", " + (r[1] != 0 ? "facing turned round" : "facing kept");
			}
			float fileHeight = Math.Max(1e-3f, file.Max[1] - file.Min[1]);
			if (r[0] != 0 || r[2] != 0)
			{
				// The height along another axis before the turn.
				fileHeight = Math.Max(1e-3f, Math.Abs(r[0]) == 90 ? file.Max[2] - file.Min[2] : Math.Abs(r[2]) == 90 ? file.Max[0] - file.Min[0] : fileHeight);
			}
			float scale = definition.Scale > 0 ? definition.Scale : (boxH > 0 ? boxH / fileHeight : 1f);
			Matrix turn = Matrix.CreateScale(scale) * Matrix.CreateRotationX(MathHelper.ToRadians(r[0])) * Matrix.CreateRotationY(MathHelper.ToRadians(r[1])) * Matrix.CreateRotationZ(MathHelper.ToRadians(r[2]));
			Vector3 offset;
			if (definition.Offset != null) offset = new Vector3(definition.Offset[0], definition.Offset[1], definition.Offset[2]);
			else
			{
				// The file stood on the model's floor (the box's bottom is sound; its z is not - the
				// port reads it as a magnitude) with its hips under the game's, else its box centre.
				Vector3 min = new Vector3(float.MaxValue), max = new Vector3(float.MinValue);
				for (int c = 0; c < 8; c++)
				{
					Vector3 corner = Vector3.Transform(new Vector3((c & 1) != 0 ? file.Max[0] : file.Min[0], (c & 2) != 0 ? file.Max[1] : file.Min[1], (c & 4) != 0 ? file.Max[2] : file.Min[2]), turn);
					min = Vector3.Min(min, corner); max = Vector3.Max(max, corner);
				}
				offset = new Vector3(-(min.X + max.X) / 2, boxY - min.Y, -(min.Z + max.Z) / 2);
				for (int s = 0; s < file.Skins.Count; s++)
				{
					for (int j = 0; j < file.Skins[s].Joints.Length; j++)
					{
						int node = binding.JointNode[s][j];
						if (node < 0 || (binding.NodeNames[node] != "hara" && binding.NodeNames[node] != "kosi")) continue;
						Vector3 hips = Vector3.Transform(RestPosition(file.Skins[s], j), turn);
						Vector3 gameHips = binding.GameBind[node].Translation;
						offset.X = gameHips.X - hips.X; offset.Z = gameHips.Z - hips.Z;
						s = file.Skins.Count; break;
					}
				}
			}
			// A fitted skeleton (Crystal's) is already in the model's space, its floor and middle the
			// model's: no fit of its own, or the auto-rig's would be undone by a hair.
			if (definition.Fitted && definition.Scale <= 0 && definition.Offset == null && definition.Rotation == null)
			{
				scale = 1f; r = new float[3]; turn = Matrix.Identity; offset = Vector3.Zero; facing = "a fitted skeleton, in the model's space as it is";
			}
			binding.Fit = turn * Matrix.CreateTranslation(offset);

			// Per joint: rest in the game's space, and the turn into the game bone's direction.
			binding.JointBase = new Matrix[file.Skins.Count][];
			binding.JointPos = new Vector3[file.Skins.Count][];
			binding.JointParent = new int[file.Skins.Count][];
			binding.JointOrder = new int[file.Skins.Count][];
			binding.JointTurn = new Matrix[file.Skins.Count][];
			binding.JointNow = new Vector3[file.Skins.Count][];
			binding.JointAlign = new Matrix[file.Skins.Count][];
			for (int s = 0; s < file.Skins.Count; s++)
			{
				GltfSkin skin = file.Skins[s];
				binding.JointBase[s] = new Matrix[skin.Joints.Length];
				binding.JointPos[s] = new Vector3[skin.Joints.Length];
				binding.JointTurn[s] = new Matrix[skin.Joints.Length];
				binding.JointNow[s] = new Vector3[skin.Joints.Length];
				binding.JointAlign[s] = new Matrix[skin.Joints.Length];
				// The skin's own hierarchy: each joint's nearest ancestor that is a joint, and an
				// order with parents before children.
				int[] parentOf = new int[skin.Joints.Length];
				for (int j = 0; j < skin.Joints.Length; j++)
				{
					parentOf[j] = -1;
					int node = skin.Joints[j];
					int hops = 0;
					while (node >= 0 && node < file.Nodes.Count && hops++ < 64)
					{
						node = file.Nodes[node].Parent;
						int k = node < 0 ? -1 : Array.IndexOf(skin.Joints, node);
						if (k >= 0) { parentOf[j] = k; break; }
					}
				}
				binding.JointParent[s] = parentOf;
				List<int> order = new List<int>();
				bool[] placed = new bool[skin.Joints.Length];
				for (int round = 0; round < skin.Joints.Length && order.Count < skin.Joints.Length; round++)
					for (int j = 0; j < skin.Joints.Length; j++)
						if (!placed[j] && (parentOf[j] < 0 || placed[parentOf[j]])) { placed[j] = true; order.Add(j); }
				for (int j = 0; j < skin.Joints.Length; j++) if (!placed[j]) order.Add(j);
				binding.JointOrder[s] = order.ToArray();
				Matrix[] restGame = new Matrix[skin.Joints.Length];
				for (int j = 0; j < skin.Joints.Length; j++)
				{
					float[] w = skin.Joints[j] >= 0 && skin.Joints[j] < rest.Length ? rest[skin.Joints[j]] : null;
					Matrix restWorld = w == null ? Matrix.Identity : new Matrix(w[0], w[1], w[2], w[3], w[4], w[5], w[6], w[7], w[8], w[9], w[10], w[11], w[12], w[13], w[14], w[15]);
					restGame[j] = restWorld * binding.Fit;
					binding.JointPos[s][j] = restGame[j].Translation;
				}
				for (int j = 0; j < skin.Joints.Length; j++)
				{
					int node = binding.JointNode[s][j];
					Vector3 pj = binding.JointPos[s][j];
					// Only a limb bone - a leg, an arm, the neck, the head - is turned into the game's
					// direction: a hub (hips, pelvis, chest, collar) has no one direction, its first child
					// being a thigh going sideways or a collarbone, and turning it would twist the body.
					bool limb = node >= 0 && IsLimb(binding.NodeNames[node]);
					// The file bone's direction: to its first child joint that is not part of the same
					// game bone (a toe under a foot is the foot's), else from its parent.
					Vector3 fileDir = Vector3.Zero;
					for (int k = 0; k < skin.Joints.Length && fileDir.LengthSquared() < 1e-8f; k++)
						if (skin.Joints[k] >= 0 && skin.Joints[k] < file.Nodes.Count && file.Nodes[skin.Joints[k]].Parent == skin.Joints[j] && binding.JointNode[s][k] != node) fileDir = binding.JointPos[s][k] - pj;
					if (fileDir.LengthSquared() < 1e-8f)
					{
						int parent = skin.Joints[j] >= 0 && skin.Joints[j] < file.Nodes.Count ? file.Nodes[skin.Joints[j]].Parent : -1;
						int k = parent < 0 ? -1 : Array.IndexOf(skin.Joints, parent);
						while (k >= 0 && (binding.JointPos[s][k] - pj).LengthSquared() < 1e-8f)
						{
							int up = skin.Joints[k] >= 0 && skin.Joints[k] < file.Nodes.Count ? file.Nodes[skin.Joints[k]].Parent : -1;
							k = up < 0 ? -1 : Array.IndexOf(skin.Joints, up);
						}
						if (k >= 0) fileDir = pj - binding.JointPos[s][k];
					}
					// The game bone's: to its first child node, else from its parent.
					Vector3 gameDir = Vector3.Zero;
					if (node >= 0)
					{
						Vector3 pn = binding.GameBind[node].Translation;
						for (int c = 0; c < count && gameDir.LengthSquared() < 1e-8f; c++) if (binding.NodeParents[c] == node) gameDir = binding.GameBind[c].Translation - pn;
						if (gameDir.LengthSquared() < 1e-8f && binding.NodeParents[node] >= 0) gameDir = pn - binding.GameBind[binding.NodeParents[node]].Translation;
					}
					Matrix align = Matrix.Identity;
					if (limb && fileDir.LengthSquared() > 1e-8f && gameDir.LengthSquared() > 1e-8f) align = RotationBetween(Vector3.Normalize(fileDir), Vector3.Normalize(gameDir));
					// Local -> file rest world -> game space -> about the joint -> turned the game's way.
					binding.JointAlign[s][j] = align;
					binding.JointBase[s][j] = binding.InverseBind[s][j] * restGame[j] * Matrix.CreateTranslation(-pj) * align;
				}
			}
			if (Options.Get("log-retarget") != null)
			{
				Log.Write(LogChannel.General, "models: " + Path.GetFileName(look.Path) + " box " + Fmt(file.Min) + " .. " + Fmt(file.Max) + "; game box " + boxX.ToString("0.##") + "," + boxY.ToString("0.##") + "," + boxZ.ToString("0.##") + " + " + boxW.ToString("0.##") + "," + boxH.ToString("0.##") + "," + boxD.ToString("0.##"));
				for (int s = 0; s < file.Skins.Count; s++)
					for (int j = 0; j < file.Skins[s].Joints.Length; j++)
					{
						int node = binding.JointNode[s][j];
						Vector3 p = RestPosition(file.Skins[s], j);
						Log.Write(LogChannel.General, "models:   joint " + JointName(file, file.Skins[s], j) + " rest " + p.X.ToString("0.###") + "," + p.Y.ToString("0.###") + "," + p.Z.ToString("0.###") + " -> " + (node >= 0 ? binding.NodeNames[node] + " bind " + Fmt(binding.GameBind[node].Translation) : "-") + " fitted " + Fmt(binding.JointPos[s][j]));
					}
			}
			Log.Write(LogChannel.General, "models: " + Path.GetFileName(look.Path) + " fitted to " + look.Model + ": scale " + scale.ToString("0.###") + ", turned " + string.Join("/", r) + " (" + facing + "), offset " + offset.X.ToString("0.##") + ", " + offset.Y.ToString("0.##") + ", " + offset.Z.ToString("0.##") + " (the model's box is " + boxW.ToString("0.#") + " x " + boxH.ToString("0.#") + " x " + boxD.ToString("0.#") + ", the file " + (file.Max[0] - file.Min[0]).ToString("0.##") + " x " + fileHeight.ToString("0.##") + " x " + (file.Max[2] - file.Min[2]).ToString("0.##") + ")");
		}

		/// <summary>The game's limb bones - those with one direction worth turning a file's bone into.</summary>
		private static bool IsLimb(string node)
		{
			string n = node.StartsWith("L_", StringComparison.Ordinal) || node.StartsWith("R_", StringComparison.Ordinal) ? node.Substring(2) : node;
			return n == "momo" || n == "hiza" || n == "sune" || n == "asi" || n == "kata" || n == "ude" || n == "te" || n == "kubi" || n == "atama";
		}

		private static string Fmt(float[] v) => v == null ? "-" : v[0].ToString("0.###") + "," + v[1].ToString("0.###") + "," + v[2].ToString("0.###");
		private static string Fmt(Vector3 v) => v.X.ToString("0.###") + "," + v.Y.ToString("0.###") + "," + v.Z.ToString("0.###");

		/// <summary>The rotation taking unit vector a onto unit vector b, the short way.</summary>
		private static Matrix RotationBetween(Vector3 a, Vector3 b)
		{
			float dot = Math.Max(-1f, Math.Min(1f, Vector3.Dot(a, b)));
			Vector3 axis = Vector3.Cross(a, b);
			if (axis.LengthSquared() < 1e-8f)
			{
				if (dot > 0) return Matrix.Identity;
				// Opposite: half a turn about any perpendicular.
				Vector3 any = Math.Abs(a.X) < 0.9f ? Vector3.UnitX : Vector3.UnitY;
				return Matrix.CreateFromAxisAngle(Vector3.Normalize(Vector3.Cross(a, any)), MathHelper.Pi);
			}
			return Matrix.CreateFromAxisAngle(Vector3.Normalize(axis), (float)Math.Acos(dot));
		}

		/// <summary>Per joint, the frame's matrix from the file's local space to camera space.</summary>
		private static void JointMatrices(Binding binding, Matrix root, Matrix placement)
		{
			if (!binding.Retarget)
			{
				for (int s = 0; s < binding.JointNode.Length; s++)
				{
					int[] map = binding.JointNode[s];
					for (int j = 0; j < map.Length; j++)
					{
						int node = map[j];
						bool seen = node >= 0 && binding.Seen[node];
						binding.JointFrame[s][j] = seen ? binding.InverseBind[s][j] * binding.Nodes[node] : binding.InverseBind[s][j] * root;
					}
				}
				return;
			}
			// Another rig: each joint's turn is the game node's change from its bind pose (a world
			// rotation, in model space); its position is the file's own rest offset from its parent
			// joint turned by the parent's turn - so limbs of other lengths stay in one piece - and
			// a root's the game node's displacement, which carries the hop and the knockback.
			Matrix unplace = Matrix.Invert(placement);
			for (int s = 0; s < binding.JointNode.Length; s++)
			{
				int[] map = binding.JointNode[s];
				foreach (int j in binding.JointOrder[s])
				{
					int node = map[j];
					int parent = binding.JointParent[s][j];
					bool seen = node >= 0 && binding.Seen[node] && binding.GameBind != null;
					// The joint's world turn from its rest: its bone aligned the game's way (JointAlign,
					// also folded into JointBase), then the game node's change. A joint with no node
					// of its own keeps its parent's world turn.
					Matrix turn, world;
					Vector3 shift = Vector3.Zero;
					if (seen)
					{
						Matrix now = binding.Nodes[node] * unplace;
						turn = binding.GameBindInverse[node] * now;
						shift = now.Translation - binding.GameBind[node].Translation;
						turn.Translation = Vector3.Zero;
						world = binding.JointAlign[s][j] * turn;
					}
					else
					{
						world = parent >= 0 ? binding.JointTurn[s][parent] : Matrix.Identity;
						turn = world;
					}
					Vector3 pj = binding.JointPos[s][j];
					Vector3 position;
					if (parent >= 0)
					{
						Vector3 offset = pj - binding.JointPos[s][parent];
						position = binding.JointNow[s][parent] + Vector3.TransformNormal(offset, binding.JointTurn[s][parent]);
					}
					else position = pj + shift;
					binding.JointTurn[s][j] = world;
					binding.JointNow[s][j] = position;
					binding.JointFrame[s][j] = binding.JointBase[s][j] * turn * Matrix.CreateTranslation(position) * placement;
				}
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
		/// One mesh's triangle list for the frame: each unique vertex through its joints' frame
		/// matrices, weighted, then the list laid out by the indices with the game's colour for
		/// the part and the file's texture coordinates. A mesh without a skin rides on the root.
		/// </summary>
		private static VertexPositionColorTexture[] Skin(Binding binding, int index, GltfMesh mesh, GltfPrimitive primitive, Matrix root, Matrix placement, Color tint, float alpha)
		{
			int count = mesh.VertexCount;
			Vector3[] positions = binding.Positions[index];
			if (positions == null || positions.Length != count) binding.Positions[index] = positions = new Vector3[count];
			bool skinned = mesh.Skin >= 0 && mesh.Skin < binding.JointFrame.Length && mesh.Joints != null && mesh.Weights != null;
			Matrix[] joints = skinned ? binding.JointFrame[mesh.Skin] : null;
			// An unskinned mesh: where the file puts it, through the fit for another rig, on the root.
			Matrix rigid = binding.Retarget ? binding.Fit * placement : root;
			float[] local = skinned ? mesh.LocalPositions : mesh.Positions;
			for (int v = 0; v < count; v++)
			{
				Vector3 p = new Vector3(local[v * 3], local[v * 3 + 1], local[v * 3 + 2]);
				if (!skinned) { positions[v] = Vector3.Transform(p, rigid); continue; }
				Vector3 outP = Vector3.Zero;
				float total = 0;
				for (int k = 0; k < 4; k++)
				{
					float w = mesh.Weights[v * 4 + k];
					int j = (int)mesh.Joints[v * 4 + k];
					if (w <= 0 || j < 0 || j >= joints.Length) continue;
					outP += Vector3.Transform(p, joints[j]) * w;
					total += w;
				}
				if (total <= 0) outP = Vector3.Transform(p, rigid);
				else if (Math.Abs(total - 1f) > 0.001f) outP /= total;
				positions[v] = outP;
			}

			VertexPositionColorTexture[] template = primitive.Vertices;
			VertexPositionColorTexture[] vertices = binding.Skinned[index];
			if (vertices == null || vertices.Length != template.Length) binding.Skinned[index] = vertices = new VertexPositionColorTexture[template.Length];
			// The colour is the game's for the part (GameColour), as the game's own vertices get it
			// at the RET pass; the template (GltfModel.Vertices) gives only the texture coordinates.
			// No lighting: the game's models get none in this port, their shading is painted.
			Color c = new Color(tint.R, tint.G, tint.B, (byte)Math.Round(tint.A * alpha));
			for (int i = 0; i < template.Length && i < mesh.Indices.Length; i++)
			{
				vertices[i].Position = positions[mesh.Indices[i]];
				vertices[i].TextureCoordinate = template[i].TextureCoordinate;
				vertices[i].Color = c;
			}
			return vertices;
		}

		private static byte Clamp(float v) => (byte)Math.Max(0, Math.Min(255, Math.Round(v)));
	}
}
