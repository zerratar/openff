// A model of the game's given a mesh of the mod's own - on the OpenFF target, as a glTF.
//
//   defs/models/j101.json      { "model": "j101", "gltf": "assets/luneth-hd.glb" }
//
// The game's model (a character's j101, a monster's f028) keeps loading and animating as it
// does: its node tree, its .ncap motions, the hand the weapon hangs from, its shadow, its
// alpha. Only the draw is taken over: the client (Compat/CharacterMeshes.cs) walks the
// model's SBC for the frame's joint matrices and draws the glTF's skin through them - real
// smooth weights, any triangle count, any texture size - in place of the game's shapes.
// That is what a Steam target cannot do; there the same Blender file goes through Crystal's
// Remake (Mdl0Reskin) into the game's own format, one bone a vertex.
//
// The glTF's joints are matched to the model's nodes by name (hara, mune, L_ude, R_te...),
// which an export from Crystal gives them; then the skin's inverse bind matrices are taken as
// they are and the armature must stay where the export put it. A rig of another convention -
// Mixamo's, Tripo's, Rigify's (Hips, Spine, Head, LeftArm, L_Forearm, mixamorig:RightHand...) -
// is retargeted instead: its bones are matched to the game's by a table of the usual names
// (and "bones" for what the table does not know), the model is scaled to the game model's
// height and stood on its feet ("scale", "rotation", "offset" override the automatic fit),
// each bone is turned into the game's bind pose, and the game's motions drive it as
// rotations about its own joints.
//
//   { "model": "j101", "gltf": "assets/luneth-hd.glb",
//     "scale": 14.4, "rotation": [0, 180, 0], "offset": [0, 0, 0],
//     "bones": { "Hip": "hara", "Pelvis": "kosi" } }

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OpenFF.Data
{
	public sealed class ModModel
	{
		/// <summary>The game's model this replaces, by its model name (j101, f028) - the .nmdp's stem.</summary>
		public string Model;
		/// <summary>The glTF, relative to the mod's root (assets/luneth-hd.glb).</summary>
		public string Gltf;
		/// <summary>The definition file this came from.</summary>
		public string Source;

		/// <summary>The fit of a rig of another convention into the game model's space: a scale (0 = to the model's height), a turn in degrees about x, y, z, and an offset after both (null = stood on the game model's feet).</summary>
		public float Scale;
		public float[] Rotation;
		public float[] Offset;

		/// <summary>A skeleton of the game's names fitted to the file (its joints moved to the file's own elbows, knees...; Crystal's fitted auto-rig): driven by retargeting, though every bone is the game's by name.</summary>
		public bool Fitted;

		/// <summary>Bones of the file -> nodes of the model, for what the built-in table does not know ("Hip": "hara").</summary>
		public Dictionary<string, string> Bones = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// The file's own animation clips played in place of the game's motions, when the file has
		/// any: the key a role (idle, walk, run, attack, damage, death, magic, victory, guard, item,
		/// poise, levelup...), a motion's id (1004) or its pack name (b01_002_01); the value the
		/// clip's name, or { "clip", "sync", "speed" }. A motion with no entry plays as the game's.
		/// </summary>
		public Dictionary<string, ModClip> Clips = new Dictionary<string, ModClip>(StringComparer.OrdinalIgnoreCase);

		/// <summary>The mod's root: the definition lives in <root>/defs/models.</summary>
		public string Root
		{
			get
			{
				if (string.IsNullOrEmpty(Source)) return null;
				try { return Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetFullPath(Source)))); }
				catch (Exception) { return null; }
			}
		}

		/// <summary>The glTF's full path, or null when the definition names none.</summary>
		public string GltfPath
		{
			get
			{
				if (string.IsNullOrWhiteSpace(Gltf)) return null;
				if (Path.IsPathRooted(Gltf)) return Gltf;
				string root = Root;
				return root == null ? null : Path.Combine(root, Gltf.Replace('/', Path.DirectorySeparatorChar));
			}
		}

		public static ModModel Parse(string json, string source = null)
		{
			JsonNode node = JsonNode.Parse(json);
			if (node == null) return null;
			ModModel model = new ModModel
			{
				Model = node["model"]?.GetValue<string>(),
				Gltf = node["gltf"]?.GetValue<string>(),
				Scale = (float)(node["scale"]?.GetValue<double>() ?? 0),
				Rotation = Triple(node["rotation"]),
				Offset = Triple(node["offset"]),
				Fitted = node["fitted"]?.GetValue<bool>() ?? false,
				Source = source
			};
			if (node["bones"] is JsonObject bones)
			{
				foreach (KeyValuePair<string, JsonNode> pair in bones)
				{
					string to = pair.Value?.GetValue<string>();
					if (!string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(to)) model.Bones[pair.Key] = to;
				}
			}
			if (node["clips"] is JsonObject clips)
			{
				foreach (KeyValuePair<string, JsonNode> pair in clips)
				{
					if (string.IsNullOrWhiteSpace(pair.Key)) continue;
					ModClip clip = pair.Value is JsonObject o
						? new ModClip { Clip = o["clip"]?.GetValue<string>(), Sync = o["sync"]?.GetValue<bool>() ?? true, Speed = (float)(o["speed"]?.GetValue<double>() ?? 1) }
						: new ModClip { Clip = pair.Value?.GetValue<string>() };
					if (!string.IsNullOrWhiteSpace(clip.Clip)) model.Clips[pair.Key] = clip;
				}
			}
			return model;
		}

		private static float[] Triple(JsonNode node)
		{
			if (node is not JsonArray array || array.Count < 3) return null;
			float[] v = new float[3];
			for (int i = 0; i < 3; i++) v[i] = (float)(array[i]?.GetValue<double>() ?? 0);
			return v;
		}

		public string ToJson()
		{
			JsonObject node = new JsonObject { ["model"] = Model, ["gltf"] = Gltf };
			if (Scale > 0) node["scale"] = Scale;
			if (Rotation != null) node["rotation"] = new JsonArray(Rotation[0], Rotation[1], Rotation[2]);
			if (Offset != null) node["offset"] = new JsonArray(Offset[0], Offset[1], Offset[2]);
			if (Fitted) node["fitted"] = true;
			if (Bones.Count > 0)
			{
				JsonObject bones = new JsonObject();
				foreach (KeyValuePair<string, string> pair in Bones) bones[pair.Key] = pair.Value;
				node["bones"] = bones;
			}
			if (Clips.Count > 0)
			{
				JsonObject clips = new JsonObject();
				foreach (KeyValuePair<string, ModClip> pair in Clips)
					clips[pair.Key] = pair.Value.Sync && Math.Abs(pair.Value.Speed - 1f) < 1e-4f ? (JsonNode)pair.Value.Clip : new JsonObject { ["clip"] = pair.Value.Clip, ["sync"] = pair.Value.Sync, ["speed"] = pair.Value.Speed };
				node["clips"] = clips;
			}
			return node.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
		}
	}

	/// <summary>One of a file's own clips standing in for a game motion.</summary>
	public sealed class ModClip
	{
		/// <summary>The clip's name in the file.</summary>
		public string Clip;
		/// <summary>True: the clip runs with the game's motion, its whole length over the motion's frames (an attack lands when the game's does); false: at its own pace, looping.</summary>
		public bool Sync = true;
		/// <summary>The pace when not synced, 1 being the clip's own.</summary>
		public float Speed = 1f;
	}

	public static class ModModels
	{
		public const string Folder = "defs/models";

		/// <summary>Every definition under the folders given (a mod's, several mods'), in file order; a broken file is a note, not a stop.</summary>
		public static List<ModModel> Load(IEnumerable<string> roots, List<string> notes = null)
		{
			List<ModModel> models = new List<ModModel>();
			foreach (string root in roots ?? Enumerable.Empty<string>())
			{
				string directory = Path.Combine(root, Folder.Replace('/', Path.DirectorySeparatorChar));
				if (!Directory.Exists(directory)) continue;
				foreach (string file in Directory.EnumerateFiles(directory, "*.json").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
				{
					try
					{
						ModModel model = ModModel.Parse(File.ReadAllText(file), file);
						if (model == null) continue;
						if (string.IsNullOrWhiteSpace(model.Model)) model.Model = Path.GetFileNameWithoutExtension(file);
						if (string.IsNullOrWhiteSpace(model.Gltf)) { notes?.Add(file + ": a model definition names a gltf"); continue; }
						models.Add(model);
					}
					catch (Exception ex) { notes?.Add(file + ": " + ex.Message); }
				}
			}
			return models;
		}
	}
}
