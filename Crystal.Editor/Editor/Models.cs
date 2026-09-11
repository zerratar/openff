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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Crystal.Editor
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
		/// <summary>The material's texture scale, rotation (sin, cos) and translation, when not the identity.</summary>
		public float[] TexMatrix { get; set; }
		/// <summary>
		/// How the texture wraps on each axis: "repeat", "mirror" (the DS's flip bit - repeat
		/// with every other copy reflected, which FF4's world map uses for the tiles that
		/// blend coast into grass) or "clamp".
		/// </summary>
		public string WrapS { get; set; }
		public string WrapT { get; set; }

		/// <summary>0 none, 1 faces the camera, 2 turns only about its vertical axis.</summary>
		public int Billboard { get; set; }

		/// <summary>
		/// The matrices this group's vertices went through, bind pose, as 4x3 in model
		/// units (rotation rows then translation): index 0 is the node's own, the rest
		/// are the stack slots its display list restored. Each vertex carries the index
		/// of its matrix as the ninth float of the buffer.
		/// </summary>
		public List<float[]> Matrices { get; set; }

		/// <summary>Which piece of the model this is, for posing.</summary>
		public int Piece { get; set; }

		/// <summary>The stack slots behind Matrices[1..], in order.</summary>
		public List<int> Slots { get; set; }

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

		/// <summary>Per vertex, which of its group's Matrices moved it (0 = the node's own).</summary>
		public List<int> MatrixIndex { get; set; } = new List<int>();
		public List<ModelGroup> Groups { get; set; }

		public float[] Centre { get; set; }
		public float Radius { get; set; }
		public string Problem { get; set; }

		/// <summary>True when the project's own copy of the package is what was read (a remake, a duplicate), not the game's.</summary>
		public bool Overridden { get; set; }

		/// <summary>The project's glTF the OpenFF client draws in place of this game model (defs/models), or null.</summary>
		public string ReplacedBy { get; set; }

		/// <summary>What the reader stepped over, if anything - see Mdl0Model.Notes.</summary>
		public List<string> Notes { get; set; }

		/// <summary>A glTF's skin, when it has one: what the viewer skins with a game model's motions (model-viewer.js).</summary>
		public ModelSkin Skin { get; set; }

		/// <summary>A glTF's normals: recalculated at this angle (Gltf.RecalculateNormals), or null for the file's own.</summary>
		public float? NormalsAngle { get; set; }

		/// <summary>True when the file's own normals are still kept beside recalculated ones, for a revert.</summary>
		public bool NormalsSource { get; set; }

		/// <summary>What a glTF asset is for - character, prop, weapon, shield, map, battle-map (Gltf.Kinds): flagged in the file, else inferred.</summary>
		public string Kind { get; set; }

		/// <summary>True when the kind was set by hand (asset.extras.kind), false when inferred from the file.</summary>
		public bool KindFlagged { get; set; }
	}

	/// <summary>
	/// A skinned glTF's rig for the browser: its joints by name, each one's inverse bind matrix,
	/// and per vertex the mesh-space position with four joints and weights. `Model` is the game
	/// model whose motions drive it - the one a definition binds it to, else the party's j101
	/// when the joints are the game's character bones.
	/// </summary>
	internal sealed class ModelSkin
	{
		public List<string> Joints { get; set; } = new List<string>();
		/// <summary>Per joint, the parent joint's index by the file's node tree, -1 at a root.</summary>
		public List<int> Parents { get; set; } = new List<int>();
		/// <summary>16 floats a joint, column-major as glTF has them.</summary>
		public List<float> InverseBind { get; set; } = new List<float>();
		/// <summary>3 floats a vertex: the mesh-space position the skin applies to.</summary>
		public List<float> Local { get; set; } = new List<float>();
		/// <summary>4 a vertex: joint indices into Joints, -1 for none.</summary>
		public List<int> JointIndex { get; set; } = new List<int>();
		/// <summary>4 a vertex.</summary>
		public List<float> Weights { get; set; } = new List<float>();
		public string Model { get; set; }
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
		/// <summary>
		/// The packages a model's textures may live in: the package itself, the .ntxp
		/// beside a .nmdp, and for a field chip (fNN_XY.flsc) the field's one sheet -
		/// fNN.ntxp in FF3, fNN_.ntxp in FF4 - which every chip of that field shares.
		/// </summary>
		private static IEnumerable<string> TextureSources(Workspace workspace, string name)
		{
			yield return name;
			if (name.EndsWith(".nmdp.lz", StringComparison.OrdinalIgnoreCase))
			{
				yield return name.Substring(0, name.Length - 8) + ".ntxp.lz";
			}
			Match chip = FieldChip.Match(name);
			if (chip.Success)
			{
				string field = chip.Groups[1].Value;
				yield return "files/" + field + ".ntxp.lz";
				yield return "files/" + field + ".ntxp";
				yield return "files/" + field + "_.ntxp.lz";
				yield return "files/" + field + "_.ntxp";
			}
		}

		private static readonly Regex FieldChip = new Regex(
			@"^files/(f\d\d)_[0-9a-fA-F]{2}\.flsc(\.lz)?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		/// <summary>
		/// A field chip (.flsc) is a pack - a 16-byte header, then (offset, size) pairs -
		/// whose first chain is the model package, the second its animation and the third
		/// its collision. Anything that is already a package comes back as it is.
		/// </summary>
		public static byte[] Unpack(byte[] data)
		{
			if (data == null || data.Length < 24 || (data[0] == 'N' && data[1] == 'M' && data[2] == 'D' && data[3] == 'P'))
			{
				return data;
			}
			uint offset = BitConverter.ToUInt32(data, 16);
			uint size = BitConverter.ToUInt32(data, 20);
			if (offset >= 24 && size >= 16 && offset + size <= (uint)data.Length
				&& data[offset] == 'N' && data[offset + 1] == 'M' && data[offset + 2] == 'D' && data[offset + 3] == 'P')
			{
				byte[] inner = new byte[size];
				Buffer.BlockCopy(data, (int)offset, inner, 0, (int)size);
				return inner;
			}
			return data;
		}

		public static ModelBundle Read(Workspace workspace, string name, bool shipped = false)
		{
			// shipped: the game's own copy even when the project overrides the name (AutoRig
			// weights a mesh against the game's model, not against an earlier remake of it).
			byte[] raw = shipped ? (workspace.ReadShipped(name) ?? workspace.Read(name)) : workspace.Read(name);
			byte[] data = Unpack(Lz.Decompress(raw));
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
				Notes = model.Notes.Count > 0 ? model.Notes : null,
				Triangles = model.Triangles,
				Quads = model.Quads,
				Nodes = model.Nodes,
				Overridden = workspace.IsOverridden(name),
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
				// The material's texture matrix, as the game composes it (GlobalScope's
				// DrawModel for a texture SRT animation, whose static case is this): scale
				// and rotation about the texture's centre, then the translation, taking
				// texel coordinates to the 0..1 the sampler wants.
				float m11 = 1f / width, m12 = 0, m21 = 0, m22 = 1f / height, m41 = 0, m42 = 0;
				if (material != null && material.HasTexMatrix)
				{
					float cs = material.RotCos, sn = material.RotSin;
					float sx = material.ScaleS / width, sy = material.ScaleT / height;
					m11 = cs * sx; m12 = sn * sx;
					m21 = -sn * sy; m22 = cs * sy;
					m41 = -material.TransS - (m11 * width + m21 * height) * 0.5f + 0.5f;
					m42 = material.TransT - (m12 * width + m22 * height) * 0.5f + 0.5f;
				}

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
					WrapS = material == null ? "repeat" : Wrap(material.RepeatS, material.FlipS),
					WrapT = material == null ? "repeat" : Wrap(material.RepeatT, material.FlipT),
					TexMatrix = material != null && material.HasTexMatrix
						? new[] { material.ScaleS, material.ScaleT, material.RotSin, material.RotCos, material.TransS, material.TransT }
						: null,
					Billboard = piece.Billboard,
					Pivot = piece.Billboard == 0
						? null : new[] { piece.PivotX, piece.PivotY, piece.PivotZ }
				};

				group.Piece = bundle.Groups.Count;
				group.Slots = piece.SlotMatrices.Keys.OrderBy(s => s).ToList();
				group.Matrices = new List<float[]> { ToFloat(piece.Matrix) };
				foreach (int slot in group.Slots)
				{
					group.Matrices.Add(ToFloat(piece.SlotMatrices[slot]));
				}
				foreach (Mdl0Run run in piece.Runs)
				{
					int first = bundle.Buffer.Count / 8;
					foreach (Mdl0Vertex v in run.Vertices)
					{
						bundle.Buffer.Add(v.X);
						bundle.Buffer.Add(v.Y);
						bundle.Buffer.Add(v.Z);
						bundle.Buffer.Add(v.U * m11 + v.V * m21 + m41);
						bundle.Buffer.Add(v.U * m12 + v.V * m22 + m42);
						bundle.Buffer.Add(v.R / 255f);
						bundle.Buffer.Add(v.G / 255f);
						bundle.Buffer.Add(v.B / 255f);
						// Beside the buffer rather than in it, so every reader of the
						// eight-float layout - the map scene, the thumbnails - is unchanged.
						bundle.MatrixIndex.Add(v.Slot < 0 ? 0 : group.Slots.IndexOf(v.Slot) + 1);
					}

					Triangulate(run, first, bundle.Indices);
				}

				group.Count = bundle.Indices.Count - group.Start;
				bundle.Groups.Add(group);
			}

			Frame(bundle);
			return bundle;
		}

		private static string Wrap(bool repeat, bool flip)
		{
			// Never clamp: FF3's materials leave the repeat bits clear and still expect repeat
			// (the game ignores them), so only flip - mirrored repeat - is read.
			return flip ? "mirror" : "repeat";
		}

		private static float[] ToFloat(int[] m)
		{
			if (m == null)
			{
				return new[] { 1f, 0, 0, 0, 1f, 0, 0, 0, 1f, 0, 0, 0 };
			}
			float[] f = new float[12];
			for (int i = 0; i < 12; i++)
			{
				f[i] = m[i] / 4096f;
			}
			return f;
		}

		// ---- motion ---------------------------------------------------------------------------

		/// <summary>One motion pack the workspace has, and what is in it.</summary>
		public sealed class MotionPack
		{
			public string Name { get; set; }
			public List<MotionInfo> Motions { get; set; } = new List<MotionInfo>();

			/// <summary>True when every motion has as many nodes as the model, and the name is close.</summary>
			public bool Likely { get; set; }
			public bool Fits { get; set; }
		}

		public sealed class MotionInfo
		{
			public int Index { get; set; }
			public uint Id { get; set; }
			public string Name { get; set; }
			public int Frames { get; set; }
			public int Nodes { get; set; }
		}

		private static readonly Dictionary<Workspace, List<(string Name, NcapFile File)>> _packs =
			new Dictionary<Workspace, List<(string, NcapFile)>>();

		/// <summary>Every .ncap in the workspace, read once and kept.</summary>
		private static List<(string Name, NcapFile File)> Packs(Workspace workspace)
		{
			lock (_packs)
			{
				if (_packs.TryGetValue(workspace, out List<(string, NcapFile)> known))
				{
					return known;
				}
			}
			List<(string, NcapFile)> packs = new List<(string, NcapFile)>();
			// Loose packs are .ncap.lz; the ones inside FF4's MOTION_MENU.dat come out of
			// the mass file already named .ncap, and may or may not still be compressed.
			foreach (WorkspaceEntry entry in workspace.List(".lz", ".ncap"))
			{
				if (!entry.Name.EndsWith(".ncap.lz", StringComparison.OrdinalIgnoreCase)
					&& !entry.Name.EndsWith(".ncap", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				try
				{
					byte[] raw = workspace.Read(entry.Name);
					packs.Add((entry.Name, NcapFile.Read(Lz.IsCompressed(raw) ? Lz.Decompress(raw) : raw)));
				}
				catch (Exception)
				{
					// A pack that does not parse is left out rather than failing the list.
				}
			}
			packs.Sort((a, b) => string.Compare(a.Item1, b.Item1, StringComparison.OrdinalIgnoreCase));
			lock (_packs)
			{
				_packs[workspace] = packs;
			}
			return packs;
		}

		/// <summary>
		/// The motion packs that could play on a model: the ones whose motions have the
		/// model's node count first, and among those the ones whose name shares a token
		/// with the model's (b_f005 for a family-5 monster, w_p00_02 for that character).
		/// Everything else follows, so a person can still try anything.
		/// </summary>
		public static List<MotionPack> Motions(Workspace workspace, string modelName)
		{
			int nodes = -1;
			string stem = Path.GetFileName(modelName ?? string.Empty);
			stem = stem.Substring(0, Math.Max(0, stem.IndexOf('.') < 0 ? stem.Length : stem.IndexOf('.')));
			try
			{
				byte[] data = Lz.Decompress(workspace.Read(modelName));
				List<Mdl0Model> models = Mdl0.Read(data);
				nodes = models.Count > 0 ? models[0].Nodes.Count : -1;
			}
			catch (Exception)
			{
			}
			string[] tokens = stem.Split('_');
			List<MotionPack> list = new List<MotionPack>();
			foreach ((string name, NcapFile file) in Packs(workspace))
			{
				string packStem = Path.GetFileName(name);
				packStem = packStem.Substring(0, packStem.IndexOf('.'));
				MotionPack pack = new MotionPack { Name = name };
				for (int i = 0; i < file.Motions.Count; i++)
				{
					JointAnimation motion = file.Motions[i];
					pack.Motions.Add(new MotionInfo
					{
						Index = i,
						Id = i < file.MotionIds.Length ? file.MotionIds[i] : 0,
						Name = motion.Name,
						Frames = motion.NumFrame,
						Nodes = motion.NumNode
					});
				}
				pack.Fits = pack.Motions.Count > 0 && pack.Motions.All(m => m.Nodes == nodes);
				pack.Likely = pack.Fits && (packStem.IndexOf(stem, StringComparison.OrdinalIgnoreCase) >= 0
					|| tokens.Any(t => t.Length >= 3 && packStem.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0));
				list.Add(pack);
			}
			return list.OrderByDescending(p => p.Likely).ThenByDescending(p => p.Fits)
				.ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
		}

		/// <summary>One motion, evaluated: per frame, per group, per matrix, the delta from bind pose.</summary>
		public sealed class Pose
		{
			public string Name { get; set; }
			public int Frames { get; set; }
			public int Fps { get; set; } = 30;

			/// <summary>How many matrices each group has - the layout of one frame.</summary>
			public List<int> Counts { get; set; }

			/// <summary>
			/// frames x (sum of Counts) x 12 floats: each 4x3 is animated x inverse(bind),
			/// so the viewer applies it straight to the vertices it already has.
			/// </summary>
			public List<float> Matrices { get; set; }
		}

		public static Pose ReadPose(Workspace workspace, string modelName, string packName, int index)
		{
			byte[] data = Lz.Decompress(workspace.Read(modelName));
			(string, NcapFile) found = Packs(workspace).FirstOrDefault(p => string.Equals(p.Name, packName, StringComparison.OrdinalIgnoreCase));
			byte[] packRaw = workspace.Read(packName);
			byte[] packData = Lz.IsCompressed(packRaw) ? Lz.Decompress(packRaw) : packRaw;
			NcapFile pack = found.Item2 ?? NcapFile.Read(packData);
			if (index < 0 || index >= pack.Motions.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(index), "no motion " + index + " in " + packName);
			}
			JointAnimation motion = pack.Motions[index];

			List<Mdl0Model> models = Mdl0.Read(data);
			Mdl0Model model = models[0];
			List<HashSet<int>> wanted = model.Pieces.Select(p => new HashSet<int>(p.SlotMatrices.Keys)).ToList();
			List<int[]> bindInverse = new List<int[]>();

			Pose pose = new Pose
			{
				Name = motion.Name,
				Frames = motion.NumFrame,
				Counts = model.Pieces.Select(p => 1 + p.SlotMatrices.Count).ToList(),
				Matrices = new List<float>()
			};
			for (int frame = 0; frame < motion.NumFrame; frame++)
			{
				int f = frame;
				List<Mdl0Piece> posed = Mdl0.Posed(data, wanted,
					(node, baseMatrix, baseScale) => NcapFile.Evaluate(motion, node, f, baseMatrix, baseScale, packData));
				for (int p = 0; p < model.Pieces.Count; p++)
				{
					Mdl0Piece bind = model.Pieces[p];
					Mdl0Piece now = p < posed.Count ? posed[p] : bind;
					Append(pose.Matrices, Delta(now.Matrix, bind.Matrix));
					foreach (int slot in bind.SlotMatrices.Keys.OrderBy(s => s))
					{
						int[] animated = now.SlotMatrices.TryGetValue(slot, out int[] a) ? a : bind.SlotMatrices[slot];
						Append(pose.Matrices, Delta(animated, bind.SlotMatrices[slot]));
					}
				}
			}
			return pose;
		}

		/// <summary>One joint of a model over a motion: per frame the node's built 4x3 in model units, what the game hands a weapon's pose from (getJntMtx).</summary>
		public sealed class Joint
		{
			public string Node { get; set; }
			public int Frames { get; set; }
			/// <summary>frames x 12 floats, row-vector 4x3 (rotation rows, then the translation).</summary>
			public List<float> Matrices { get; set; } = new List<float>();
			/// <summary>The model's node names, for a picker.</summary>
			public List<string> Nodes { get; set; }
		}

		/// <summary>
		/// The joint named <paramref name="nodeName"/> (R_te, L_te for the hands; R_ude, L_ude
		/// the forearms) of a model, over a motion of a pack - or its bind pose alone when no
		/// pack is named. The node's matrix is the SBC's, so it includes every parent's.
		/// </summary>
		public static Joint ReadJoint(Workspace workspace, string modelName, string packName, int index, string nodeName)
		{
			byte[] data = Lz.Decompress(workspace.Read(modelName));
			List<Mdl0Model> models = Mdl0.Read(data);
			Mdl0Model model = models[0];
			int node = model.Nodes.FindIndex(n => string.Equals(n, nodeName, StringComparison.OrdinalIgnoreCase));
			if (node < 0)
			{
				throw new ArgumentException("no joint '" + nodeName + "' in " + modelName + " (it has " + string.Join(", ", model.Nodes) + ")");
			}
			List<HashSet<int>> wanted = model.Pieces.Select(p => new HashSet<int>(p.SlotMatrices.Keys)).ToList();
			Joint joint = new Joint { Node = model.Nodes[node], Nodes = model.Nodes.ToList() };

			if (string.IsNullOrEmpty(packName))
			{
				Dictionary<int, int[]> built = new Dictionary<int, int[]>();
				Mdl0.Posed(data, wanted, null, built);
				joint.Frames = 1;
				Append(joint.Matrices, built.TryGetValue(node, out int[] m) ? m : Identity12());
				return joint;
			}

			(string, NcapFile) found = Packs(workspace).FirstOrDefault(p => string.Equals(p.Name, packName, StringComparison.OrdinalIgnoreCase));
			byte[] packRaw = workspace.Read(packName);
			byte[] packData = Lz.IsCompressed(packRaw) ? Lz.Decompress(packRaw) : packRaw;
			NcapFile pack = found.Item2 ?? NcapFile.Read(packData);
			if (index < 0 || index >= pack.Motions.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(index), "no motion " + index + " in " + packName);
			}
			JointAnimation motion = pack.Motions[index];
			joint.Frames = motion.NumFrame;
			for (int frame = 0; frame < motion.NumFrame; frame++)
			{
				int f = frame;
				Dictionary<int, int[]> built = new Dictionary<int, int[]>();
				Mdl0.Posed(data, wanted, (n, baseMatrix, baseScale) => NcapFile.Evaluate(motion, n, f, baseMatrix, baseScale, packData), built);
				Append(joint.Matrices, built.TryGetValue(node, out int[] m) ? m : Identity12());
			}
			return joint;
		}

		/// <summary>
		/// The model's skeleton as the game has it: the node tree from the SBC, every node's
		/// bind matrix, which node each of a group's matrices belongs to, and motions as the
		/// nodes' matrices frame by frame. What a glTF skin with real bones is built from.
		/// </summary>
		public sealed class Rig
		{
			public List<string> Nodes { get; set; }
			/// <summary>Per node, its parent's index, -1 for a root.</summary>
			public int[] Parents { get; set; }
			/// <summary>Per node, the built 4x3 (row-vector, model units) at bind pose.</summary>
			public List<float[]> Bind { get; set; }
			/// <summary>
			/// (group, matrix index within the group) -> the nodes that matrix is, with weights
			/// summing to 1: one node at weight 1 for an ordinary joint matrix, several for an
			/// envelope (the DS's skinning: each node's matrix through its inverse bind, blended).
			/// </summary>
			public Dictionary<(int Group, int Local), (int Node, float Weight)[]> Weights { get; set; } = new Dictionary<(int, int), (int, float)[]>();
			public List<RigMotion> Motions { get; set; } = new List<RigMotion>();
			/// <summary>True when any vertex is weighted to more than one node.</summary>
			public bool Blended { get; set; }
		}

		public sealed class RigMotion
		{
			public string Name { get; set; }
			public int Frames { get; set; }
			/// <summary>Per frame, per node, the built 4x3.</summary>
			public List<float[][]> Worlds { get; set; } = new List<float[][]>();
		}

		/// <summary>
		/// The rig of a model, with one motion of a pack (<paramref name="index"/>), every
		/// motion of it (<paramref name="all"/>), or none when no pack is named.
		/// </summary>
		public static Rig ReadRig(Workspace workspace, string modelName, string packName, int index, bool all, bool shipped = false)
		{
			byte[] data = Lz.Decompress(shipped ? (workspace.ReadShipped(modelName) ?? workspace.Read(modelName)) : workspace.Read(modelName));
			List<Mdl0Model> models = Mdl0.Read(data);
			if (models.Count == 0) throw new InvalidDataException("no models in " + modelName);
			Mdl0Model model = models[0];
			List<HashSet<int>> wanted = model.Pieces.Select(p => new HashSet<int>(p.SlotMatrices.Keys)).ToList();
			int count = model.Nodes.Count;

			Rig rig = new Rig { Nodes = model.Nodes.ToList(), Parents = new int[count], Bind = new List<float[]>() };
			for (int i = 0; i < count; i++)
			{
				int parent = model.NodeParents.TryGetValue(i, out int p) ? p : -1;
				rig.Parents[i] = parent >= 0 && parent < count && parent != i ? parent : -1;
			}
			Dictionary<int, int[]> built = new Dictionary<int, int[]>();
			Mdl0.Posed(data, wanted, null, built);
			for (int i = 0; i < count; i++)
			{
				rig.Bind.Add(ToFloat(built.TryGetValue(i, out int[] m) ? m : null));
			}
			(int, float)[] WeightsOf(int node, (int Node, int Weight)[] blend, int fallback)
			{
				if (node >= 0 && node < count) return new[] { (node, 1f) };
				if (blend != null && blend.Length > 0)
				{
					// Grouped by node (a node can appear twice in a blend), weights made to sum to 1.
					var grouped = blend.Where(b => b.Node >= 0 && b.Node < count).GroupBy(b => b.Node)
						.Select(gr => (Node: gr.Key, Weight: (float)gr.Sum(b => b.Weight))).Where(b => b.Weight > 0).ToList();
					float total = grouped.Sum(b => b.Weight);
					if (grouped.Count > 0 && total > 0)
					{
						if (grouped.Count > 1) rig.Blended = true;
						return grouped.OrderByDescending(b => b.Weight).Select(b => (b.Node, b.Weight / total)).ToArray();
					}
				}
				return new[] { (Math.Max(0, Math.Min(count - 1, fallback)), 1f) };
			}
			for (int g = 0; g < model.Pieces.Count; g++)
			{
				Mdl0Piece piece = model.Pieces[g];
				int own = piece.NodeIndex >= 0 ? piece.NodeIndex : model.Nodes.IndexOf(piece.Node);
				rig.Weights[(g, 0)] = WeightsOf(piece.NodeIndex, piece.Blend, own);
				int local = 1;
				foreach (int slot in piece.SlotMatrices.Keys.OrderBy(s => s))
				{
					int node = piece.SlotNodes.TryGetValue(slot, out int n) ? n : -1;
					piece.SlotBlends.TryGetValue(slot, out (int Node, int Weight)[] blend);
					rig.Weights[(g, local++)] = WeightsOf(node, blend, own);
				}
			}

			if (string.IsNullOrEmpty(packName)) return rig;
			(string, NcapFile) found = Packs(workspace).FirstOrDefault(p => string.Equals(p.Name, packName, StringComparison.OrdinalIgnoreCase));
			byte[] packRaw = workspace.Read(packName);
			byte[] packData = Lz.IsCompressed(packRaw) ? Lz.Decompress(packRaw) : packRaw;
			NcapFile pack = found.Item2 ?? NcapFile.Read(packData);
			IEnumerable<int> which = all ? Enumerable.Range(0, pack.Motions.Count) : new[] { index };
			foreach (int i in which)
			{
				if (i < 0 || i >= pack.Motions.Count)
				{
					throw new ArgumentOutOfRangeException(nameof(index), "no motion " + i + " in " + packName);
				}
				JointAnimation motion = pack.Motions[i];
				RigMotion rm = new RigMotion { Name = motion.Name ?? ("motion" + i), Frames = motion.NumFrame };
				for (int frame = 0; frame < motion.NumFrame; frame++)
				{
					int f = frame;
					Dictionary<int, int[]> now = new Dictionary<int, int[]>();
					Mdl0.Posed(data, wanted, (n, baseMatrix, baseScale) => NcapFile.Evaluate(motion, n, f, baseMatrix, baseScale, packData), now);
					float[][] worlds = new float[count][];
					for (int n = 0; n < count; n++)
					{
						worlds[n] = now.TryGetValue(n, out int[] m) ? ToFloat(m) : rig.Bind[n];
					}
					rm.Worlds.Add(worlds);
				}
				rig.Motions.Add(rm);
			}
			return rig;
		}

		private static int[] Identity12() => new[] { 4096, 0, 0, 0, 4096, 0, 0, 0, 4096, 0, 0, 0 };

		private static void Append(List<float> into, int[] fixedMatrix)
		{
			for (int i = 0; i < 12; i++) into.Add(fixedMatrix[i] / 4096f);
		}

		/// <summary>
		/// Writes the model as .glb into <paramref name="directory"/> - with a motion when a
		/// pack and index are given - and returns the path. The name carries the motion so
		/// several exports of one model can sit side by side.
		/// </summary>
		public static string Export(Workspace workspace, string modelName, string packName, int index, string directory, bool all = false)
		{
			(byte[] glb, string stem) = ExportBytes(workspace, modelName, packName, index, all);
			Directory.CreateDirectory(directory);
			string path = Path.Combine(directory, stem + ".glb");
			File.WriteAllBytes(path, glb);
			// The textures beside it as well - they are inside the .glb, but a PNG on disk is
			// what a paint program opens.
			ModelBundle bundle = Read(workspace, modelName);
			foreach (string texture in bundle.Groups.Select(g => g.Texture).Where(t => t != null).Distinct(StringComparer.Ordinal))
			{
				try
				{
					byte[] png = Texture(workspace, modelName, texture);
					if (png != null) File.WriteAllBytes(Path.Combine(directory, texture + ".png"), png);
				}
				catch (Exception)
				{
					// A texture that will not decode is left out; the model is still there.
				}
			}
			return path;
		}

		/// <summary>
		/// The model as .glb in memory, with the file stem it would be saved under (the model's
		/// name, then the motion's when one is given, or the pack's for all of them) - for a
		/// Save As in the browser. The skin is the model's node tree with real bones.
		/// </summary>
		public static (byte[] Glb, string Stem) ExportBytes(Workspace workspace, string modelName, string packName, int index, bool all = false)
		{
			ModelBundle bundle = Read(workspace, modelName);
			if (bundle.Problem != null)
			{
				throw new InvalidDataException(bundle.Problem);
			}
			Rig rig = ReadRig(workspace, modelName, packName, index, all);
			byte[] glb = Gltf.Write(bundle, texture =>
			{
				try
				{
					return Texture(workspace, modelName, texture);
				}
				catch (Exception)
				{
					return null;
				}
			}, rig);

			string stem = Path.GetFileName(modelName);
			stem = stem.Substring(0, stem.IndexOf('.') < 0 ? stem.Length : stem.IndexOf('.'));
			string Clean(string s) => new string((s ?? "motion").Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-').ToArray());
			if (all && !string.IsNullOrEmpty(packName))
			{
				string pack = Path.GetFileName(packName);
				stem += "." + Clean(pack.Substring(0, pack.IndexOf('.') < 0 ? pack.Length : pack.IndexOf('.')));
			}
			else if (rig.Motions.Count == 1)
			{
				stem += "." + Clean(rig.Motions[0].Name);
			}
			return (glb, stem);
		}

		/// <summary>animated x inverse(bind), in floats: what moves a bind-pose vertex to its animated place.</summary>
		private static float[] Delta(int[] animated, int[] bind)
		{
			double[] a = new double[12], b = new double[12];
			for (int i = 0; i < 12; i++)
			{
				a[i] = animated[i] / 4096.0;
				b[i] = bind[i] / 4096.0;
			}
			// Invert the bind 4x3 (row-vector convention: v' = v * R + t).
			double det = b[0] * (b[4] * b[8] - b[5] * b[7]) - b[1] * (b[3] * b[8] - b[5] * b[6]) + b[2] * (b[3] * b[7] - b[4] * b[6]);
			if (Math.Abs(det) < 1e-12)
			{
				return new[] { 1f, 0, 0, 0, 1f, 0, 0, 0, 1f, 0, 0, 0 };
			}
			double[] inv = new double[12];
			inv[0] = (b[4] * b[8] - b[5] * b[7]) / det;
			inv[1] = (b[2] * b[7] - b[1] * b[8]) / det;
			inv[2] = (b[1] * b[5] - b[2] * b[4]) / det;
			inv[3] = (b[5] * b[6] - b[3] * b[8]) / det;
			inv[4] = (b[0] * b[8] - b[2] * b[6]) / det;
			inv[5] = (b[2] * b[3] - b[0] * b[5]) / det;
			inv[6] = (b[3] * b[7] - b[4] * b[6]) / det;
			inv[7] = (b[1] * b[6] - b[0] * b[7]) / det;
			inv[8] = (b[0] * b[4] - b[1] * b[3]) / det;
			// t_inv = -t * R_inv
			inv[9] = -(b[9] * inv[0] + b[10] * inv[3] + b[11] * inv[6]);
			inv[10] = -(b[9] * inv[1] + b[10] * inv[4] + b[11] * inv[7]);
			inv[11] = -(b[9] * inv[2] + b[10] * inv[5] + b[11] * inv[8]);
			// delta = inv(bind) then animated: v * inv * A
			double[] d = new double[12];
			for (int r = 0; r < 3; r++)
			{
				for (int c = 0; c < 3; c++)
				{
					d[r * 3 + c] = inv[r * 3] * a[c] + inv[r * 3 + 1] * a[3 + c] + inv[r * 3 + 2] * a[6 + c];
				}
			}
			for (int c = 0; c < 3; c++)
			{
				d[9 + c] = inv[9] * a[c] + inv[10] * a[3 + c] + inv[11] * a[6 + c] + a[9 + c];
			}
			float[] result = new float[12];
			for (int i = 0; i < 12; i++)
			{
				result[i] = (float)d[i];
			}
			return result;
		}

		private static void Append(List<float> into, float[] m)
		{
			for (int i = 0; i < 12; i++)
			{
				into.Add(m[i]);
			}
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

			foreach (string source in TextureSources(workspace, name))
			{
				if (source == null || !workspace.Exists(source))
				{
					continue;
				}
				try
				{
					byte[] data = Unpack(Lz.Decompress(workspace.Read(source)));
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
		internal static void Frame(ModelBundle bundle)
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
			foreach (string source in TextureSources(workspace, name))
			{
				if (source == null || !workspace.Exists(source))
				{
					continue;
				}

				byte[] data;
				try
				{
					data = Unpack(Lz.Decompress(workspace.Read(source)));
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
