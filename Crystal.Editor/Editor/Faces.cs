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

		/// <summary>The packs read during one request (a pack's pictures are compared across its siblings').</summary>
		private static Func<string, FacePack> Reader(Workspace workspace)
		{
			Dictionary<string, FacePack> read = new Dictionary<string, FacePack>(StringComparer.OrdinalIgnoreCase);
			return pack => read.TryGetValue(pack, out FacePack known) ? known : read[pack] = Read(workspace, pack);
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
			List<FacePack> read = new List<FacePack>();
			HashSet<string> parts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			Func<string, FacePack> packOf = Reader(workspace);
			foreach (WorkspaceEntry entry in workspace.List(".face").OrderBy(e => Path.GetFileName(e.Name), StringComparer.OrdinalIgnoreCase))
			{
				string file = Path.GetFileName(entry.Name);
				if (!mine.IsMatch(file)) continue;
				string pack = Path.GetFileNameWithoutExtension(file);
				FacePack faces = packOf(pack);
				if (faces == null) continue;
				read.Add(faces);
				foreach (string part in faces.Parts) parts.Add(part);
				packs.Add(new
				{
					name = pack,
					count = faces.Sets.Count,
					parts = faces.Parts.Select(part => new
					{
						name = part,
						numbers = Enumerable.Range(0, faces.Sets.Count).Select(n =>
						{
							// The mod's picture for it - its own, or the one given the same picture in another pack (linkedFrom).
							string path = null, from = null;
							if (definition != null && !definition.Faces.TryGetValue(ModModel.FaceKey(pack, part, n), out path))
								path = FacePack.LinkedPath(definition, pack, part, n, packOf, out from) != null ? LinkedRelative(definition, from) : null;
							return new
							{
								n,
								game = faces.Picture(n, part) is (int w, int h, byte[] _) ? new { width = w, height = h } : null,
								replaced = path,
								linkedFrom = from
							};
						}).ToList()
					}).ToList()
				});
			}

			// The material each part is drawn on: a glTF's by name (or the definition's faceParts), the game model's texture of the part's name.
			// And per part, whether that material's picture is one of the game's (a model made from the game's sheets): such a
			// stand-in wears the game's expressions where the mod has none; a face drawn anew keeps its own picture there.
			Dictionary<string, string> on = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			Dictionary<string, object> looks = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			if (GltfBundle.IsAsset(name))
			{
				string path = GltfBundle.Resolve(project, name);
				if (path != null)
				{
					OpenFF.Graphics.GltfFile file = OpenFF.Graphics.GltfFile.Load(path);
					ModModel rules = definition ?? new ModModel();
					for (int m = 0; m < file.Materials.Count; m++)
					{
						OpenFF.Graphics.GltfMaterial material = file.Materials[m];
						if (rules.FacePartOf(material.Name, parts) is not string part || on.ContainsKey(part)) continue;
						on[part] = material.Name;
						looks[part] = LooksLikeGame(file, material, m, rules, read, part);
					}
				}
			}
			else foreach (string part in parts) { on[part] = part; looks[part] = new { alike = true, difference = 0 }; }

			return new
			{
				model,
				standsIn = definition != null,
				definition = definition == null ? null : "defs/models/" + Path.GetFileName(definition.Source),
				gameFaces = (definition ?? new ModModel()).GameFacesMode,
				looks,
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
			if (!game && FacePack.LinkedPath(definition, pack, part, number, Reader(workspace), out _) is string own && File.Exists(own)) return File.ReadAllBytes(own);
			FacePack faces = Read(workspace, pack) ?? throw new FileNotFoundException("no face pack " + pack);
			if (faces.Picture(number, part) is (int w, int h, byte[] rgba)) return Png.Encode(w, h, rgba);
			throw new FileNotFoundException(pack + " has no " + part + " picture " + number);
		}

		/// <summary>A picture of the mod's own for a pack's part and number (bytes: a PNG), or the game's back (bytes null).</summary>
		public static object Replace(Workspace workspace, Project project, string name, string pack, string part, int number, byte[] bytes)
		{
			(string model, ModModel definition) = ModelOf(project, name);
			if (definition == null)
				throw new ArgumentException("expressions are replaced on a model's stand-in: open a glTF, pick the game model it replaces (Use in OpenFF), then replace them there"
					+ (model != null ? " - " + model + " has none in this project yet" : ""));
			if (string.IsNullOrWhiteSpace(pack) || string.IsNullOrWhiteSpace(part) || number < 0 || pack.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || part.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
				throw new ArgumentException("which picture?");
			string key = ModModel.FaceKey(pack, part, number);
			string written = null;
			// The same picture in the model's other packs (j101_002's 2 is j101_001's 3) goes with it: replaced once, replaced
			// wherever the game shows it; given back, given back there too.
			List<(string Pack, int Number)> twins = FacePack.Twins(PacksOf(workspace, model, null), pack, part, number);
			if (bytes == null)
			{
				string was = definition.Faces.TryGetValue(key, out string path) ? path
					: FacePack.LinkedPath(definition, pack, part, number, Reader(workspace), out string from) != null ? LinkedRelative(definition, from) : null;
				definition.Faces.Remove(key);
				foreach ((string twinPack, int twin) in twins)
				{
					string twinKey = ModModel.FaceKey(twinPack, part, twin);
					if (was != null && definition.Faces.TryGetValue(twinKey, out string theirs) && string.Equals(theirs, was, StringComparison.OrdinalIgnoreCase)) definition.Faces.Remove(twinKey);
				}
			}
			else
			{
				(int w, int _) = GltfBundle.PngSize(bytes);
				if (w == 0) throw new ArgumentException("not a PNG");
				string relative = Relative(pack, part, number);
				string full = Path.Combine(project.Directory, relative.Replace('/', Path.DirectorySeparatorChar));
				Directory.CreateDirectory(Path.GetDirectoryName(full));
				File.WriteAllBytes(full, bytes);
				definition.Faces[key] = written = relative;
				foreach ((string twinPack, int twin) in twins) definition.Faces[ModModel.FaceKey(twinPack, part, twin)] = relative;
			}
			File.WriteAllText(definition.Source, definition.ToJson());
			return new { ok = true, pack, part, n = number, picture = written, shared = twins.Select(t => t.Pack + " " + part + " " + t.Number).ToList(), definition = "defs/models/" + Path.GetFileName(definition.Source) };
		}

		/// <summary>The definition's entry (as written in it) for "pack part n".</summary>
		private static string LinkedRelative(ModModel definition, string from)
		{
			string[] bits = (from ?? "").Split(' ');
			return bits.Length == 3 && int.TryParse(bits[2], out int n) && definition.Faces.TryGetValue(ModModel.FaceKey(bits[0], bits[1], n), out string path) ? path : null;
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
			foreach (WorkspaceEntry entry in workspace.List(".face").OrderBy(e => Path.GetFileName(e.Name), StringComparer.OrdinalIgnoreCase))
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
			int written = 0, kept = 0, linked = 0, shared = 0;
			bool all = string.IsNullOrEmpty(pack) || pack == "*";
			Dictionary<string, string> first = new Dictionary<string, string>(StringComparer.Ordinal);   // a picture's signature -> its first key
			foreach ((string packName, FacePack faces) in PacksOf(workspace, model, pack))
			{
				foreach (string part in faces.Parts)
				{
					for (int n = 0; n < faces.Sets.Count; n++)
					{
						string key = ModModel.FaceKey(packName, part, n);
						// Every pack at once: a picture an earlier pack already gave is one file, not one per pack - the client
						// and Crystal take that file wherever the picture is (FacePack.LinkedPath). A copy an earlier export
						// wrote and nobody touched is let go for it.
						string signature = all ? faces.Signature(n, part) : null;
						if (signature != null && first.ContainsKey(signature))
						{
							if (definition != null && definition.Faces.TryGetValue(key, out string had) && string.Equals(had, Relative(packName, part, n), StringComparison.OrdinalIgnoreCase) && Untouched(project, had, faces, n, part))
							{
								definition.Faces.Remove(key);
								linked++;
							}
							shared++;
							continue;
						}
						if (signature != null) first[signature] = key;
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
			return new { ok = true, written, kept, linked, shared, folder, standsIn = definition != null };
		}

		/// <summary>Whether the project's file still holds exactly the game's picture (written by an export, never edited).</summary>
		private static bool Untouched(Project project, string relative, FacePack faces, int number, string part)
		{
			try
			{
				string full = Path.Combine(project.Directory, relative.Replace('/', Path.DirectorySeparatorChar));
				if (!File.Exists(full) || faces.Picture(number, part) is not (int w, int h, byte[] game)) return false;
				byte[] mine = PngRead.Decode(File.ReadAllBytes(full), out int mw, out int mh);
				return mine != null && mw == w && mh == h && mine.AsSpan().SequenceEqual(game);
			}
			catch (Exception) { return false; }
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

		/// <summary>The picture a stand-in's material shows (the definition's in place of the file's) set against the game's
		/// pictures of a part, as the client sets it (FacePack.SameAsGame).</summary>
		private static object LooksLikeGame(OpenFF.Graphics.GltfFile file, OpenFF.Graphics.GltfMaterial material, int index, ModModel rules, List<FacePack> packs, string part)
		{
			byte[] bytes = null;
			string own = rules.TexturePath(material.Name, index);
			if (own != null && File.Exists(own)) bytes = File.ReadAllBytes(own);
			else if (material.Image >= 0 && material.Image < file.Images.Count) bytes = file.Images[material.Image].Bytes;
			if (bytes == null) return new { alike = false, difference = (int?)null, reason = "no picture" };
			byte[] rgba;
			int width, height;
			try { rgba = PngRead.Decode(bytes, out width, out height); }
			catch (Exception) { return new { alike = false, difference = (int?)null, reason = "not a PNG" }; }
			int best = int.MaxValue;
			foreach (FacePack faces in packs) if (best > 0) best = Math.Min(best, faces.Closest(width, height, rgba, part));
			return new { alike = best <= FacePack.SameAsGame, difference = best == int.MaxValue ? (int?)null : best, reason = best == int.MaxValue ? "a different size" : null };
		}

		/// <summary>Whether a face picture the mod has none for shows the game's own on the stand-in: "auto" (when the material's
		/// picture looks like the game's), "always" or "never".</summary>
		public static object SetGameFaces(Project project, string name, string mode)
		{
			ModModel definition = ModelOf(project, name).Definition ?? throw new ArgumentException("no stand-in for this model yet");
			definition.GameFaces = mode == "always" ? true : mode == "never" ? false : null;
			File.WriteAllText(definition.Source, definition.ToJson());
			return new { ok = true, gameFaces = definition.GameFacesMode };
		}
	}
}
