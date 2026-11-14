// A character's expressions in Crystal: the game's face packs for a model (j101_001.face ... j101_008.face - a set of
// eye and mouth pictures per number, which a scene puts on and flips through: SetUpCharacter_FaceData, ChangeFaceEye,
// ChangeFaceMouth), listed beside the model with every picture, the mod's own in place of the game's where its
// definition says so ("faces" in defs/models/<model>.json), and replaced from here - a PNG written to
// assets/faces/<pack>/<part>-<number>.png and named in the definition.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using OpenFF.Data;

namespace Crystal.Editor
{
	internal static class Faces
	{
		/// <summary>The game model a document is about (j101 for j101.nmdp.lz or for a glTF standing in for it) and the
		/// project's definition for it, if any.</summary>
		private static (string Model, ModModel Definition) ModelOf(Project project, string name)
		{
			if (GltfBundle.IsAsset(name))
			{
				ModModel own = GltfBundle.Definition(project, name);
				return (own?.Base ?? own?.Model, own);
			}
			string stem = Path.GetFileName(name ?? "");
			int dot = stem.IndexOf('.');
			if (dot > 0) stem = stem.Substring(0, dot);
			ModModel definition = null;
			if (project != null)
			{
				try { definition = ModModels.Load(new[] { project.Directory }).FirstOrDefault(d => string.Equals(d.Model, stem, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(d.Gltf)); }
				catch (Exception) { }
			}
			return (stem, definition);
		}

		private static FacePack Read(Workspace workspace, string pack)
		{
			// Loose installs list it under files/, archives by its bare name.
			string name = pack + ".face";
			if (!workspace.Exists(name)) name = workspace.List(".face").FirstOrDefault(e => string.Equals(Path.GetFileName(e.Name), pack + ".face", StringComparison.OrdinalIgnoreCase))?.Name;
			return name != null ? FacePack.Read(workspace.Read(name)) : null;
		}

		/// <summary>The model's face packs, each with its parts and numbers, and the material each part is drawn on.</summary>
		public static object List(Workspace workspace, Project project, string name)
		{
			(string model, ModModel definition) = ModelOf(project, name);
			if (string.IsNullOrEmpty(model)) return new { model = (string)null, packs = new object[0] };
			Regex mine = new Regex("^" + Regex.Escape(model) + @"(_\d+)?\.face$", RegexOptions.IgnoreCase);
			List<object> packs = new List<object>();
			HashSet<string> parts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (WorkspaceEntry entry in workspace.List(".face"))
			{
				string file = Path.GetFileName(entry.Name);
				if (!mine.IsMatch(file)) continue;
				string pack = Path.GetFileNameWithoutExtension(file);
				FacePack faces = Read(workspace, pack);
				if (faces == null) continue;
				foreach (string part in faces.Parts) parts.Add(part);
				packs.Add(new
				{
					name = pack,
					count = faces.Sets.Count,
					parts = faces.Parts.Select(part => new
					{
						name = part,
						numbers = Enumerable.Range(0, faces.Sets.Count).Select(n => new
						{
							n,
							game = faces.Picture(n, part) is (int w, int h, byte[] _) ? new { width = w, height = h } : null,
							replaced = definition != null && definition.Faces.TryGetValue(ModModel.FaceKey(pack, part, n), out string path) ? path : null
						}).ToList()
					}).ToList()
				});
			}

			// The material each part is drawn on: a glTF's by name (or the definition's faceParts), the game model's texture of the part's name.
			Dictionary<string, string> on = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			if (GltfBundle.IsAsset(name))
			{
				string path = GltfBundle.Resolve(project, name);
				if (path != null)
				{
					OpenFF.Graphics.GltfFile file = OpenFF.Graphics.GltfFile.Load(path);
					ModModel rules = definition ?? new ModModel();
					foreach (OpenFF.Graphics.GltfMaterial material in file.Materials)
						if (rules.FacePartOf(material.Name, parts) is string part && !on.ContainsKey(part)) on[part] = material.Name;
				}
			}
			else foreach (string part in parts) on[part] = part;

			return new
			{
				model,
				standsIn = definition != null,
				definition = definition == null ? null : "defs/models/" + Path.GetFileName(definition.Source),
				gameFaces = definition?.GameFaces ?? true,
				faceParts = definition?.FaceParts,
				on,
				packs
			};
		}

		/// <summary>A pack's picture of a part and number as a PNG: the mod's own when the definition names one (unless the
		/// game's is asked for), else the game's.</summary>
		public static byte[] Picture(Workspace workspace, Project project, string name, string pack, string part, int number, bool game)
		{
			(string _, ModModel definition) = ModelOf(project, name);
			if (!game && definition?.FacePath(pack, part, number) is string own && File.Exists(own)) return File.ReadAllBytes(own);
			FacePack faces = Read(workspace, pack) ?? throw new FileNotFoundException("no face pack " + pack);
			if (faces.Picture(number, part) is (int w, int h, byte[] rgba)) return Png.Encode(w, h, rgba);
			throw new FileNotFoundException(pack + " has no " + part + " picture " + number);
		}

		/// <summary>A picture of the mod's own for a pack's part and number (bytes: a PNG), or the game's back (bytes null).</summary>
		public static object Replace(Project project, string name, string pack, string part, int number, byte[] bytes)
		{
			(string model, ModModel definition) = ModelOf(project, name);
			if (definition == null)
				throw new ArgumentException("expressions are replaced on a model's stand-in: open a glTF, pick the game model it replaces (Use in OpenFF), then replace them there"
					+ (model != null ? " - " + model + " has none in this project yet" : ""));
			if (string.IsNullOrWhiteSpace(pack) || string.IsNullOrWhiteSpace(part) || number < 0 || pack.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || part.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
				throw new ArgumentException("which picture?");
			string key = ModModel.FaceKey(pack, part, number);
			string written = null;
			if (bytes == null) definition.Faces.Remove(key);
			else
			{
				(int w, int _) = GltfBundle.PngSize(bytes);
				if (w == 0) throw new ArgumentException("not a PNG");
				string relative = Relative(pack, part, number);
				string full = Path.Combine(project.Directory, relative.Replace('/', Path.DirectorySeparatorChar));
				Directory.CreateDirectory(Path.GetDirectoryName(full));
				File.WriteAllBytes(full, bytes);
				definition.Faces[key] = written = relative;
			}
			File.WriteAllText(definition.Source, definition.ToJson());
			return new { ok = true, pack, part, n = number, picture = written, definition = "defs/models/" + Path.GetFileName(definition.Source) };
		}

		private static string Relative(string pack, string part, int number) =>
			GltfBundle.Folder + "/faces/" + pack.ToLowerInvariant() + "/" + part.ToLowerInvariant() + "-" + number + ".png";

		/// <summary>The packs of a model to work on: one by name, or every one of its packs (pack null or "*").</summary>
		private static IEnumerable<(string Name, FacePack Faces)> PacksOf(Workspace workspace, string model, string pack)
		{
			if (!string.IsNullOrEmpty(pack) && pack != "*")
			{
				FacePack one = Read(workspace, pack);
				if (one != null) yield return (pack, one);
				yield break;
			}
			Regex mine = new Regex("^" + Regex.Escape(model) + @"(_\d+)?\.face$", RegexOptions.IgnoreCase);
			foreach (WorkspaceEntry entry in workspace.List(".face"))
			{
				string file = Path.GetFileName(entry.Name);
				if (!mine.IsMatch(file)) continue;
				string name = Path.GetFileNameWithoutExtension(file);
				FacePack faces = Read(workspace, name);
				if (faces != null) yield return (name, faces);
			}
		}

		/// <summary>Every picture of a pack (or of all the model's packs) written into the project as
		/// assets/faces/&lt;pack&gt;/&lt;part&gt;-&lt;number&gt;.png and named in the definition - the files to edit in an image
		/// editor, which the client and Crystal take again as they change. A file already there is kept (your edit), only
		/// named; with no stand-in yet the files are written and named once one exists (export again).</summary>
		public static object Export(Workspace workspace, Project project, string name, string pack)
		{
			(string model, ModModel definition) = ModelOf(project, name);
			if (string.IsNullOrEmpty(model)) throw new ArgumentException("no model");
			int written = 0, kept = 0, linked = 0;
			foreach ((string packName, FacePack faces) in PacksOf(workspace, model, pack))
			{
				foreach (string part in faces.Parts)
				{
					for (int n = 0; n < faces.Sets.Count; n++)
					{
						string key = ModModel.FaceKey(packName, part, n);
						if (definition != null && definition.Faces.ContainsKey(key)) { kept++; continue; }
						if (faces.Picture(n, part) is not (int w, int h, byte[] rgba)) continue;
						string relative = Relative(packName, part, n);
						string full = Path.Combine(project.Directory, relative.Replace('/', Path.DirectorySeparatorChar));
						if (File.Exists(full)) kept++;
						else
						{
							Directory.CreateDirectory(Path.GetDirectoryName(full));
							File.WriteAllBytes(full, Png.Encode(w, h, rgba));
							written++;
						}
						if (definition != null) { definition.Faces[key] = relative; linked++; }
					}
				}
			}
			if (definition != null && linked > 0) File.WriteAllText(definition.Source, definition.ToJson());
			string folder = Path.Combine(project.Directory, GltfBundle.Folder, "faces");
			if (!string.IsNullOrEmpty(pack) && pack != "*") folder = Path.Combine(folder, pack.ToLowerInvariant());
			return new { ok = true, written, kept, linked, folder, standsIn = definition != null };
		}

		/// <summary>A pack's (or every pack's) pictures as a zip of &lt;pack&gt;/&lt;part&gt;-&lt;number&gt;.png - the mod's own where it
		/// has them, else the game's - to edit and drop back on the Expressions card.</summary>
		public static byte[] Zip(Workspace workspace, Project project, string name, string pack)
		{
			(string model, ModModel definition) = ModelOf(project, name);
			using MemoryStream memory = new MemoryStream();
			using (System.IO.Compression.ZipArchive zip = new System.IO.Compression.ZipArchive(memory, System.IO.Compression.ZipArchiveMode.Create, true))
			{
				foreach ((string packName, FacePack faces) in PacksOf(workspace, model ?? "", pack))
				{
					foreach (string part in faces.Parts)
					{
						for (int n = 0; n < faces.Sets.Count; n++)
						{
							byte[] png = null;
							if (definition?.FacePath(packName, part, n) is string own && File.Exists(own)) png = File.ReadAllBytes(own);
							else if (faces.Picture(n, part) is (int w, int h, byte[] rgba)) png = Png.Encode(w, h, rgba);
							if (png == null) continue;
							System.IO.Compression.ZipArchiveEntry entry = zip.CreateEntry(packName.ToLowerInvariant() + "/" + part.ToLowerInvariant() + "-" + n + ".png", System.IO.Compression.CompressionLevel.Fastest);
							using Stream stream = entry.Open();
							stream.Write(png, 0, png.Length);
						}
					}
				}
			}
			return memory.ToArray();
		}

		/// <summary>Which material of the stand-in wears a face part (null: the material of the part's name).</summary>
		public static object SetFacePart(Project project, string name, string part, string material)
		{
			ModModel definition = ModelOf(project, name).Definition ?? throw new ArgumentException("no stand-in for this model yet");
			if (string.IsNullOrWhiteSpace(part)) throw new ArgumentException("which part?");
			if (string.IsNullOrWhiteSpace(material)) definition.FaceParts.Remove(part); else definition.FaceParts[part] = material;
			File.WriteAllText(definition.Source, definition.ToJson());
			return new { ok = true, part, material };
		}

		/// <summary>Whether a face picture the mod has none for shows the game's own on the stand-in.</summary>
		public static object SetGameFaces(Project project, string name, bool on)
		{
			ModModel definition = ModelOf(project, name).Definition ?? throw new ArgumentException("no stand-in for this model yet");
			definition.GameFaces = on;
			File.WriteAllText(definition.Source, definition.ToJson());
			return new { ok = true, gameFaces = on };
		}
	}
}
