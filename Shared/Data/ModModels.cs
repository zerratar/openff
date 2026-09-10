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
// which an export from Crystal gives them; the skin's inverse bind matrices are taken as they
// are, so the armature must stay where the export put it.

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
			return new ModModel
			{
				Model = node["model"]?.GetValue<string>(),
				Gltf = node["gltf"]?.GetValue<string>(),
				Source = source
			};
		}

		public string ToJson()
		{
			JsonObject node = new JsonObject { ["model"] = Model, ["gltf"] = Gltf };
			return node.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
		}
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
