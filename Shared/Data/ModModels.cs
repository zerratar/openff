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
//
// A model of the mod's own: "base" names the game's model whose skeleton and motions it borrows, and "model" a
// name the game does not have (f300). Asked for by that name (a summon's SET_MODEL 300), the game loads the base's
// files and the character is drawn as the glTF - the base itself, wherever else it is used, keeps its own look.
//
//   { "model": "f300", "base": "f202", "gltf": "assets/wyrm.glb", "clips": { "101": "Idle", "201": "Breath" } }
//
// The file's pictures can be swapped without touching it: "textures" gives a material (by its name in the file, or
// "#2" by its place) a picture of the mod's own, and "filter" draws every picture "nearest" (crisp texels, pixel art)
// or "linear" (smoothed) whatever the file's samplers say.
//
//   { "model": "j101", "gltf": "assets/werewolf.glb", "textures": { "eye": "assets/eye-shocked.png" }, "filter": "nearest" }
//
// Expressions: a scene puts a face pack on a character (SetUpCharacter_FaceData "j101_001" - the game's
// j101_001.face, a numbered set of eye and mouth pictures) and shows one of its pictures at a time
// (ChangeFaceEye 3: a blink, a wide-eyed start). "faces" gives a picture of the mod's own for a pack's part and
// number; the material of the part's name (or "faceParts"' choice) wears it while the scene shows that number. A
// number the mod has no picture for shows the material's own picture - the face stays as the file has it - unless the
// material's picture looks like the game's (a model made from the game's, its eye and mouth sheets kept): then the game's
// own pictures from the pack, so such a model has its expressions with nothing added. "gameFaces": true shows the game's
// whatever the file's picture is, false never.
//
//   { "model": "j101", "gltf": "assets/werewolf.glb",
//     "faces": { "j101_001": { "eye": { "0": "assets/faces/j101_001/eye-0.png", "3": "assets/faces/j101_001/eye-3.png" },
//                              "mouth": { "1": "assets/faces/j101_001/mouth-1.png" } } },
//     "faceParts": { "eye": "Eyes" } }

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
		/// <summary>A model of the mod's own: the game's model whose skeleton and motions it borrows (f202); null when it replaces Model's look.</summary>
		public string Base;
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

		/// <summary>Pictures in place of the file's own: a material's name in the file (or "#index") -> a picture relative to
		/// the mod's root (assets/eye-shocked.png).</summary>
		public Dictionary<string, string> Textures = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		/// <summary>"nearest" or "linear" for every picture of the file, in place of its samplers'; null keeps the file's.</summary>
		public string Filter;

		/// <summary>Expression pictures: "pack/part/number" (j101_001/eye/3) -> a picture relative to the mod's root.</summary>
		public Dictionary<string, string> Faces = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		/// <summary>A face part (eye, mouth) -> the material of the file that wears it, when not the material of its name.</summary>
		public Dictionary<string, string> FaceParts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		/// <summary>Whether a face picture the mod has none for shows the game's own: null (automatic) when the material's
		/// picture looks like the game's, true always, false never.</summary>
		public bool? GameFaces;

		/// <summary>"auto", "always" or "never", as Crystal shows it.</summary>
		public string GameFacesMode => GameFaces == true ? "always" : GameFaces == false ? "never" : "auto";

		public static string FaceKey(string pack, string part, int number) => (pack ?? "").ToLowerInvariant() + "/" + (part ?? "").ToLowerInvariant() + "/" + number;

		/// <summary>The face part (eye, mouth) a material wears: faceParts' choice, else the part the material is named after
		/// ("eye", "Eyes", "eye.001"); null for a material that is no part of the face.</summary>
		public string FacePartOf(string material, IEnumerable<string> parts)
		{
			if (string.IsNullOrEmpty(material)) return null;
			foreach (KeyValuePair<string, string> pair in FaceParts) if (string.Equals(pair.Value, material, StringComparison.OrdinalIgnoreCase)) return pair.Key;
			string stem = material.ToLowerInvariant();
			int cut = stem.IndexOfAny(new[] { '.', '_', '-', ' ' });
			if (cut > 0) stem = stem.Substring(0, cut);
			foreach (string part in parts)
			{
				if (FaceParts.ContainsKey(part)) continue;
				string p = part.ToLowerInvariant();
				if (stem == p || stem == p + "s") return part;
			}
			return null;
		}

		/// <summary>The mod's picture for a pack's part and number, as a full path; null when it has none.</summary>
		public string FacePath(string pack, string part, int number) =>
			Faces.TryGetValue(FaceKey(pack, part, number), out string path) ? Resolve(path) : null;

		private string Resolve(string path)
		{
			if (string.IsNullOrWhiteSpace(path)) return null;
			if (Path.IsPathRooted(path)) return path;
			string root = Root;
			return root == null ? null : Path.Combine(root, path.Replace('/', Path.DirectorySeparatorChar));
		}

		/// <summary>The picture a material is to wear in place of the file's own, as a full path; null for the file's.</summary>
		public string TexturePath(string material, int index)
		{
			if (Textures.Count == 0) return null;
			if (!(material != null && Textures.TryGetValue(material, out string path)) && !Textures.TryGetValue("#" + index, out path)) return null;
			return Resolve(path);
		}

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
				Base = string.IsNullOrWhiteSpace(node["base"]?.GetValue<string>()) ? null : node["base"].GetValue<string>().Trim(),
				Gltf = node["gltf"]?.GetValue<string>(),
				Scale = (float)(node["scale"]?.GetValue<double>() ?? 0),
				Rotation = Triple(node["rotation"]),
				Offset = Triple(node["offset"]),
				Fitted = node["fitted"]?.GetValue<bool>() ?? false,
				Filter = node["filter"]?.GetValue<string>() is string f && (f == "nearest" || f == "linear") ? f : null,
				GameFaces = node["gameFaces"] is JsonValue gameFaces && gameFaces.TryGetValue(out bool always) ? always : null,
				Source = source
			};
			if (node["textures"] is JsonObject textures)
			{
				foreach (KeyValuePair<string, JsonNode> pair in textures)
				{
					string to = pair.Value?.GetValue<string>();
					if (!string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(to)) model.Textures[pair.Key] = to.Replace('\\', '/');
				}
			}
			if (node["faces"] is JsonObject faces)
			{
				foreach (KeyValuePair<string, JsonNode> pack in faces)
				{
					if (pack.Value is not JsonObject parts) continue;
					foreach (KeyValuePair<string, JsonNode> part in parts)
					{
						if (part.Value is not JsonObject numbers) continue;
						foreach (KeyValuePair<string, JsonNode> number in numbers)
						{
							string to = number.Value?.GetValue<string>();
							if (int.TryParse(number.Key, out int n) && n >= 0 && !string.IsNullOrWhiteSpace(to)) model.Faces[FaceKey(pack.Key, part.Key, n)] = to.Replace('\\', '/');
						}
					}
				}
			}
			if (node["faceParts"] is JsonObject faceParts)
			{
				foreach (KeyValuePair<string, JsonNode> pair in faceParts)
				{
					string to = pair.Value?.GetValue<string>();
					if (!string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(to)) model.FaceParts[pair.Key] = to;
				}
			}
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
			if (Base != null) node["base"] = Base;
			if (Scale > 0) node["scale"] = Scale;
			if (Rotation != null) node["rotation"] = new JsonArray(Rotation[0], Rotation[1], Rotation[2]);
			if (Offset != null) node["offset"] = new JsonArray(Offset[0], Offset[1], Offset[2]);
			if (Fitted) node["fitted"] = true;
			if (Filter != null) node["filter"] = Filter;
			if (Textures.Count > 0)
			{
				JsonObject textures = new JsonObject();
				foreach (KeyValuePair<string, string> pair in Textures) textures[pair.Key] = pair.Value;
				node["textures"] = textures;
			}
			if (Faces.Count > 0)
			{
				JsonObject faces = new JsonObject();
				foreach (KeyValuePair<string, string> pair in Faces.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
				{
					string[] key = pair.Key.Split('/');
					if (key.Length != 3) continue;
					if (faces[key[0]] is not JsonObject pack) faces[key[0]] = pack = new JsonObject();
					if (pack[key[1]] is not JsonObject part) pack[key[1]] = part = new JsonObject();
					part[key[2]] = pair.Value;
				}
				node["faces"] = faces;
			}
			if (FaceParts.Count > 0)
			{
				JsonObject faceParts = new JsonObject();
				foreach (KeyValuePair<string, string> pair in FaceParts) faceParts[pair.Key] = pair.Value;
				node["faceParts"] = faceParts;
			}
			if (GameFaces != null) node["gameFaces"] = GameFaces.Value;
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
