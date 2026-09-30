// Models of the mod's own for the battle: a glTF drawn on a game model's skeleton under a name the game does not have
// (defs/models/f300.json: { "model": "f300", "base": "f202", "gltf": "assets/wyrm.glb", ... } - Shared/Data/ModModels.cs,
// OpenFF/Compat/CharacterMeshes.cs). A summon names one by its number (SET_MODEL 300); the client loads the base's
// files and draws the glTF, its own clips standing in for the base's motions where the definition maps them.
//
// Server: /api/project/own-models (the project's), /api/project/own-models/save (one written, a new one numbered from
// 300), /api/model/gltf-clip (a clip's joint matrices frame by frame, for the Stage to skin the file with).

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using OpenFF.Data;
using OpenFF.Graphics;

namespace Crystal.Editor
{
	internal static class SummonModels
	{
		/// <summary>The first number a model of the mod's own takes: the game's go to f208.</summary>
		public const int First = 300;

		private static string Folder(Project project) => Path.Combine(project.Directory, ModModels.Folder.Replace('/', Path.DirectorySeparatorChar));

		private static List<ModModel> Own(Project project) =>
			project == null ? new List<ModModel>() : ModModels.Load(new[] { project.Directory }).Where(m => m.Base != null).ToList();

		private static int Number(string model) =>
			model != null && model.Length > 1 && (model[0] == 'f' || model[0] == 'F') && int.TryParse(model.Substring(1), NumberStyles.Integer, CultureInfo.InvariantCulture, out int n) ? n : -1;

		private static object Describe(ModModel m) => new
		{
			model = m.Model,
			number = Number(m.Model),
			@base = m.Base,
			gltf = m.Gltf,
			scale = m.Scale,
			rotation = m.Rotation,
			offset = m.Offset,
			clips = m.Clips.ToDictionary(p => p.Key, p => p.Value.Clip),
		};

		/// <summary>The project's models of its own (definitions with a base).</summary>
		public static object List(Project project) => new { ok = true, models = Own(project).Select(Describe).ToList() };

		/// <summary>
		/// One written: { model (f300, or none for the next free), base (f202), gltf (assets/x.glb), scale, rotation,
		/// offset, clips: { "101": "Idle" } }. What the body leaves out, the definition keeps.
		/// </summary>
		public static object Save(Project project, JsonNode body)
		{
			if (project == null) throw new InvalidOperationException("no project is open");
			string name = body?["model"]?.ToString();
			List<ModModel> own = Own(project);
			ModModel model = name == null ? null : own.FirstOrDefault(m => string.Equals(m.Model, name, StringComparison.OrdinalIgnoreCase));
			if (model == null)
			{
				if (name == null)
				{
					// The next number no definition of the project has.
					HashSet<int> taken = new HashSet<int>(ModModels.Load(new[] { project.Directory }).Select(m => Number(m.Model)));
					int n = First;
					while (taken.Contains(n)) n++;
					name = "f" + n.ToString("000", CultureInfo.InvariantCulture);
				}
				if (Number(name) < First) throw new ArgumentException("a model of the mod's own is f" + First + " or on (" + name + " is the game's)");
				model = new ModModel { Model = name };
			}
			if (body?["base"] != null) model.Base = body["base"].ToString().Trim();
			if (body?["gltf"] != null) model.Gltf = body["gltf"].ToString().Trim();
			if (string.IsNullOrWhiteSpace(model.Base)) throw new ArgumentException("a model of the mod's own needs a base (the game's model whose skeleton it takes, f202)");
			if (string.IsNullOrWhiteSpace(model.Gltf)) throw new ArgumentException("a model of the mod's own needs a glTF (assets/x.glb)");
			if (body?["scale"] is JsonValue sv && sv.TryGetValue(out double scale)) model.Scale = (float)Math.Max(0, scale);
			if (body?["rotation"] is JsonArray ra) model.Rotation = ra.Count >= 3 ? new[] { F(ra[0]), F(ra[1]), F(ra[2]) } : null;
			if (body?["offset"] is JsonArray oa) model.Offset = oa.Count >= 3 ? new[] { F(oa[0]), F(oa[1]), F(oa[2]) } : null;
			if (body?["clips"] is JsonObject clips)
			{
				model.Clips.Clear();
				foreach (KeyValuePair<string, JsonNode> pair in clips)
				{
					string clip = pair.Value?.ToString();
					if (!string.IsNullOrWhiteSpace(clip)) model.Clips[pair.Key] = new ModClip { Clip = clip };
				}
			}
			Directory.CreateDirectory(Folder(project));
			File.WriteAllText(Path.Combine(Folder(project), model.Model + ".json"), model.ToJson(), new UTF8Encoding(false));
			return new { ok = true, model = Describe(model) };
		}

		private static float F(JsonNode n) => n is JsonValue v && v.TryGetValue(out double d) ? (float)d : 0f;

		/// <summary>
		/// A clip of a glTF sampled for the Stage: each frame (30 a second) every skin joint's world matrix, the
		/// joints in the order the model bundle lists them (the skins one after another). No clip: the rest pose, one
		/// frame. With the file's box, so the Stage can stand it on its feet.
		/// </summary>
		public static object Clip(Project project, string name, string clipName)
		{
			string path = GltfBundle.Resolve(project, name) ?? throw new FileNotFoundException("no file " + name + " in the project");
			GltfFile file = GltfFile.Load(path);
			GltfAnimation clip = string.IsNullOrWhiteSpace(clipName) ? null : file.Animations.Find(a => string.Equals(a.Name, clipName, StringComparison.OrdinalIgnoreCase));
			if (!string.IsNullOrWhiteSpace(clipName) && clip == null) throw new ArgumentException("no clip " + clipName + " in " + name);
			List<int> nodes = new List<int>();
			foreach (GltfSkin skin in file.Skins) nodes.AddRange(skin.Joints);
			int frames = clip == null ? 1 : Math.Max(1, (int)Math.Ceiling(clip.Duration * 30));
			float[] matrices = new float[frames * nodes.Count * 16];
			for (int f = 0; f < frames; f++)
			{
				float[][] world = file.WorldMatrices(clip, clip == null ? 0f : Math.Min(clip.Duration, f / 30f));
				for (int j = 0; j < nodes.Count; j++)
				{
					float[] w = nodes[j] >= 0 && nodes[j] < world.Length ? world[nodes[j]] : null;
					int at = (f * nodes.Count + j) * 16;
					if (w == null) { matrices[at] = matrices[at + 5] = matrices[at + 10] = matrices[at + 15] = 1; continue; }
					Array.Copy(w, 0, matrices, at, 16);
				}
			}
			return new { ok = true, frames, joints = nodes.Count, matrices, min = file.Min, max = file.Max, clips = file.Animations.Select(a => a.Name).ToList() };
		}
	}
}
