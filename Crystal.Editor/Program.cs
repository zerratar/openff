// FF3 content tool.
//
//   dotnet run --project Crystal.Editor -- info    <file-or-dir>
//   dotnet run --project Crystal.Editor -- extract <xnb-dir> <out-dir>
//   dotnet run --project Crystal.Editor -- archives         <content-dir>
//   dotnet run --project Crystal.Editor -- extract-archives <content-dir> <out-dir> [pattern ...]
//   dotnet run --project Crystal.Editor -- xbn        <file.xbn> [out.xml]
//   dotnet run --project Crystal.Editor -- xbn-build  <file.xml> [out.xbn]
//   dotnet run --project Crystal.Editor -- msd        <file.msd | dir> [out]
//   dotnet run --project Crystal.Editor -- msd-build  <file.json> [out.msd]
//   dotnet run --project Crystal.Editor -- script       <file.script | dir> [out] [--text=<dir>]
//   dotnet run --project Crystal.Editor -- script-build <file.ffs | dir> [out]
//   dotnet run --project Crystal.Editor -- ops [filter]
//   dotnet run --project Crystal.Editor -- tex         <file.lz | dir> [out]
//   dotnet run --project Crystal.Editor -- mdl         <file.lz | dir> [out]
//   dotnet run --project Crystal.Editor -- cells       <file | dir> [out]
//   dotnet run --project Crystal.Editor -- hich        <file.hich | dir> [out]
//   dotnet run --project Crystal.Editor -- editor [--content=<dir>] [--port=5050]
//   dotnet run --project Crystal.Editor -- lz          <file.lz | dir> [out]
//   dotnet run --project Crystal.Editor -- lz-compress <file> [out.lz]
//   dotnet run --project Crystal.Editor -- pak        <file.pak | dir> [out]
//   dotnet run --project Crystal.Editor -- pak-build  <file.json> [out.pak]
//
// "extract" turns the shipped .xnb files back into editable sources:
//   Fonts/<name>.png   + <name>.json   glyph atlas and metrics
//   Audio/<name>.wav                   PCM/ADPCM audio, straight from the XNB
// and writes a Content.mgcb so the MonoGame pipeline can rebuild them.

using OpenFF.Content;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

namespace Crystal
{
	internal static class Program
	{
		private static int Main(string[] args)
		{
			// The editor is what this is mostly used for now, so it is what you get
			// when no command is named. Options still work: `crystal --port=5051`
			// opens the editor on that port. A word that is not a command is a typo,
			// not an invitation to open the editor and hope.
			if (args.Length == 0 || args[0].StartsWith("-", StringComparison.Ordinal))
			{
				return Editor(args);
			}

			try
			{
				switch (args[0].ToLowerInvariant())
				{
					case "info":
						return Info(args.Length > 1 ? args[1] : ".");
					case "api-docs":
					{
						string engine = args.FirstOrDefault(a => a.StartsWith("--engine=", StringComparison.OrdinalIgnoreCase))?.Substring("--engine=".Length).Trim('"');
						string output = args.Skip(1).FirstOrDefault(a => !a.StartsWith("--", StringComparison.Ordinal)) ?? Path.Combine("Docs", "API.md");
						return ApiDocs.Write(output, engine);
					}
					case "extract":
						if (args.Length < 3)
						{
							Usage();
							return 1;
						}
						return Extract(args[1], args[2]);
					case "archives":
						return Archives.Info(args.Length > 1 ? args[1] : "Content");
					case "extract-archives":
						if (args.Length < 3)
						{
							Usage();
							return 1;
						}
						return Archives.Extract(args[1], args[2], args.Skip(3).ToArray());
					case "xbn":
						if (args.Length < 2)
						{
							Usage();
							return 1;
						}
						return XbnDecode(args[1], args.Length > 2 ? args[2] : null);
					case "xbn-build":
						if (args.Length < 2)
						{
							Usage();
							return 1;
						}
						return XbnBuild(args[1], args.Length > 2 ? args[2] : null);
					case "msd":
						if (args.Length < 2)
						{
							Usage();
							return 1;
						}
						return MsdDecode(args[1], args.Length > 2 ? args[2] : null);
					case "msd-build":
						if (args.Length < 2)
						{
							Usage();
							return 1;
						}
						return MsdBuild(args[1], args.Length > 2 ? args[2] : null);
					case "script":
						if (args.Length < 2)
						{
							Usage();
							return 1;
						}
						return ScriptDump(args.Skip(1).ToArray());
					case "tex":
						if (args.Length < 2)
						{
							Usage();
							return 1;
						}
						return TexDump(args[1], args.Length > 2 ? args[2] : null);
					case "mdl":
						if (args.Length < 2)
						{
							Usage();
							return 1;
						}
						return MdlDump(args[1], args.Length > 2 ? args[2] : null);
					case "sample-assets":
					{
						string dir = args.Skip(1).FirstOrDefault(a => !a.StartsWith("--", StringComparison.Ordinal)) ?? Path.Combine("Samples", "Showcase", "assets");
						try
						{
							foreach ((string file, int bytes, string note) in Crystal.Editor.SampleAssets.WriteAll(dir))
								Console.WriteLine(file + " (" + bytes.ToString("N0") + " bytes): " + note);
							return 0;
						}
						catch (Exception ex) { Console.Error.WriteLine("sample-assets: " + ex.Message); return 1; }
					}
					case "mdl-import":
					{
						string[] positional = args.Skip(1).Where(a => !a.StartsWith("--", StringComparison.Ordinal)).ToArray();
						if (positional.Length < 2)
						{
							Usage();
							return 1;
						}
						string scaleText = args.FirstOrDefault(a => a.StartsWith("--scale=", StringComparison.OrdinalIgnoreCase))?.Substring(8);
						float scale = scaleText != null && float.TryParse(scaleText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float s) ? s : 1f;
						return MdlImport(positional[0], positional[1], positional.Length > 2 ? positional[2] : null, scale);
					}
					case "cells":
						if (args.Length < 2)
						{
							Usage();
							return 1;
						}
						return CellsDump(args[1], args.Length > 2 ? args[2] : null);
					case "hich":
						if (args.Length < 2)
						{
							Usage();
							return 1;
						}
						return HichDump(args[1], args.Length > 2 ? args[2] : null);
					case "mcl":
						if (args.Length < 2)
						{
							Usage();
							return 1;
						}
						return MclDump(args[1], args.Length > 2 ? args[2] : null);
					case "installs":
						return ListInstalls();
					case "projects":
						return ListProjects();
					case "install":
						return ModCommand(args, install: true);
					case "uninstall":
						return ModCommand(args, install: false);
					case "editor":
						return Editor(args.Skip(1).ToArray());
					case "lz":
						if (args.Length < 2)
						{
							Usage();
							return 1;
						}
						return LzDecompress(args[1], args.Length > 2 ? args[2] : null);
					case "lz-compress":
						if (args.Length < 2)
						{
							Usage();
							return 1;
						}
						return LzCompress(args[1], args.Length > 2 ? args[2] : null);
					case "ops":
						return Ops(args.Skip(1).FirstOrDefault(a => !a.StartsWith("--", StringComparison.Ordinal)),
							GameOption(args));
					case "script-build":
						if (args.Length < 2)
						{
							Usage();
							return 1;
						}
					{
						string[] positional = args.Skip(1)
							.Where(a => !a.StartsWith("--", StringComparison.Ordinal)).ToArray();
						return ScriptBuild(positional[0], positional.Length > 1 ? positional[1] : null,
							GameOption(args));
					}
					case "pak":
						if (args.Length < 2)
						{
							Usage();
							return 1;
						}
						return PakDecode(args.Skip(1).ToArray());
					case "tables":
						if (args.Length < 2)
						{
							Usage();
							return 1;
						}
						return Tables(args.Skip(1).ToArray());
					case "pak-build":
						if (args.Length < 2)
						{
							Usage();
							return 1;
						}
						return PakBuild(args[1], args.Length > 2 ? args[2] : null);
					default:
						Usage();
						return 1;
				}
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine("error: " + ex.Message);
				return 2;
			}
		}

		/// <summary>tables &lt;install root&gt; [--items=N]: the game's tables in the unified shape (OpenFF.Data), for checking a reader.</summary>
		private static int Tables(string[] args)
		{
			string root = args[0];
			int items = 12;
			foreach (string a in args.Skip(1))
			{
				if (a.StartsWith("--items=", StringComparison.Ordinal)) int.TryParse(a.Substring(8), out items);
			}
			if (!Directory.Exists(root))
			{
				Console.Error.WriteLine("not a directory: " + root);
				return 1;
			}
			ContentChain chain = ContentChain.Open(root);
			if (Environment.GetEnvironmentVariable("FF3_TABLES_DEBUG") != null)
			{
				Console.WriteLine(chain.Describe());
				foreach (string probe in new[] { "player.chaindata", "player.chaindata.lz", "files/player.chaindata.lz", "EXTRACTED_DATA/files/player.chaindata.lz", "item_parameter.pak", "babil_item.msd", "e01_00.dsc.lz", "d01_00.script" })
				{
					Console.WriteLine("  " + probe + ": " + chain.Exists(probe));
				}
			}
			string game = null;
			foreach (string a in args.Skip(1)) if (a.StartsWith("--game=", StringComparison.Ordinal)) game = a.Substring(7);
			OpenFF.Data.GameTables tables = OpenFF.Data.TableFiles.Read(chain, game);
			Console.WriteLine(tables.Describe());
			foreach (OpenFF.Data.JobDefinition job in tables.Jobs)
			{
				OpenFF.Data.LevelRow l1 = job.Levels.Length > 0 ? job.Levels[0] : null, l30 = job.Levels.Length >= 30 ? job.Levels[29] : null;
				Console.WriteLine("  job " + job.Id + " " + job.Name + (job.NameIsTentative ? "*" : "") + ": curves " + string.Join(",", job.GrowthTypes)
					+ (l1 != null ? "; L1 " + l1.Stats : "") + (l30 != null ? "; L30 " + l30.Stats + " hp ~" + job.MaxHpAt(30) + " charges " + string.Join("/", l30.Charges ?? Array.Empty<int>()) : ""));
			}
			if (items > 12)
			{
				int shown = 0;
				foreach (OpenFF.Data.ItemDefinition item in tables.Items)
				{
					if (shown++ >= items) break;
					Console.WriteLine("  " + item + (item.Equip != null ? " " + item.Equip : "") + (item.Caption != null ? " - " + item.Caption : ""));
				}
			}
			return 0;
		}

		private static void Usage()
		{
			Console.Error.WriteLine("usage:");
			Console.Error.WriteLine("  (no command)                      open the editor");
			Console.Error.WriteLine("  info    <file.xnb | directory>");
			Console.Error.WriteLine("  tables  <install root> [--items=N] [--game=ff3|ff4]    the game's tables in the unified shape (FF3 or FF4)");
			Console.Error.WriteLine("  api-docs [out.md] [--engine=<dll>]  the modding API reference from OpenFF.Engine (default Docs/API.md)");
			Console.Error.WriteLine("  extract <xnb-directory> <output-directory>");
			Console.Error.WriteLine("  archives         <content-directory>");
			Console.Error.WriteLine("  extract-archives <content-directory> <output-directory> [pattern ...]");
			Console.Error.WriteLine();
			Console.Error.WriteLine("  xbn        <file.xbn> [out.xml]   menu definition -> XML");
			Console.Error.WriteLine("  xbn-build  <file.xml> [out.xbn]   XML -> menu definition");
			Console.Error.WriteLine("  msd        <file.msd | dir> [out] game text -> JSON");
			Console.Error.WriteLine("  msd-build  <file.json> [out.msd]  JSON -> game text");
			Console.Error.WriteLine("  pak        <file.pak | dir> [out] [--text=<dir>]");
			Console.Error.WriteLine("                                    parameter tables -> JSON");
			Console.Error.WriteLine("  pak-build  <file.json> [out.pak]   JSON -> parameter tables");
			Console.Error.WriteLine("  script       <file.script | dir> [out] [--text=<dir>]");
			Console.Error.WriteLine("                                    event bytecode -> .ffs source");
			Console.Error.WriteLine("  script-build <file.ffs | dir> [out]");
			Console.Error.WriteLine("                                    .ffs source -> event bytecode");
			Console.Error.WriteLine("  ops [filter]                      list script instructions");
			Console.Error.WriteLine("  tex         <file.lz | dir> [out]  textures -> PNG");
			Console.Error.WriteLine("  mdl         <file.lz | dir> [out]  models -> OBJ");
			Console.Error.WriteLine("  mdl-import  <file.glb|.gltf> <name> [out-dir] [--scale=n]");
			Console.Error.WriteLine("                                    glTF -> <name>.nmdp.lz and <name>.ntxp.lz, the game's own model (a w123 for a weapon)");
			Console.Error.WriteLine("  sample-assets [out-dir]           write the sample glTFs (a sword, a shield, a chest, a shrine) - Samples/Showcase/assets by default");
			Console.Error.WriteLine("  cells       <file | dir> [out]     cells/screens/anim -> JSON");
			Console.Error.WriteLine("  hich        <file.hich | dir> [out] map placement -> JSON");
			Console.Error.WriteLine("  mcl         <file.mcl.lz | dir> [out] collision mesh -> JSON");
			Console.Error.WriteLine("  editor [--project=<name>] [--target=ours|steam] [--content=<dir>]");
			Console.Error.WriteLine("         [--override=<dir>] [--port=<n>] [--language=en] [--no-browser]");
			Console.Error.WriteLine("                                    open the content editor in a browser");
			Console.Error.WriteLine("  projects                          mods on this machine");
			Console.Error.WriteLine("  installs                          Steam copies of the game on this machine");
			Console.Error.WriteLine("  install   [--content=<dir>] [--override=<dir>]");
			Console.Error.WriteLine("                                    copy a mod into a Steam install");
			Console.Error.WriteLine("  uninstall [--content=<dir>] [--override=<dir>]");
			Console.Error.WriteLine("                                    put the originals back");
			Console.Error.WriteLine("  lz          <file.lz | dir> [out] decompress");
			Console.Error.WriteLine("  lz-compress <file> [out.lz]       compress");
			Console.Error.WriteLine();
			Console.Error.WriteLine("patterns are globs on the archived name, e.g. \"*.NCGR\" \"btl*\"");
		}

		private static IEnumerable<string> XnbFiles(string path)
		{
			if (File.Exists(path))
			{
				return new[] { path };
			}
			if (Directory.Exists(path))
			{
				return Directory.EnumerateFiles(path, "*.xnb").OrderBy(p => p, StringComparer.OrdinalIgnoreCase);
			}
			throw new FileNotFoundException("no such file or directory: " + path);
		}

		private static int Info(string path)
		{
			Dictionary<string, int> byReader = new Dictionary<string, int>();
			foreach (string file in XnbFiles(path))
			{
				Xnb xnb = Xnb.Open(file);
				try
				{
					string reader = xnb.PrimaryReaderName;
					byReader.TryGetValue(reader, out int n);
					byReader[reader] = n + 1;

					string detail = string.Empty;
					if (reader == "SpriteFontReader")
					{
						XnbSpriteFont font = xnb.ReadSpriteFont();
						detail = string.Format(CultureInfo.InvariantCulture,
							"  {0}x{1} {2}, {3} glyphs, lineSpacing={4}",
							font.Texture.Width, font.Texture.Height,
							SurfaceFormats.Name(font.Texture.SurfaceFormat),
							font.Characters.Count, font.LineSpacing);
					}
					else if (reader == "SoundEffectReader")
					{
						XnbSoundEffect sound = xnb.ReadSoundEffect();
						detail = string.Format(CultureInfo.InvariantCulture,
							"  fmt={0} {1}Hz {2}ch, {3} bytes, {4} ms",
							sound.FormatTag, sound.SampleRate, sound.Channels,
							sound.Data.Length, sound.DurationMs);
					}

					Console.WriteLine("{0,-20} platform={1} v{2} {3}{4}",
						Path.GetFileName(file), xnb.Header.Platform, xnb.Header.Version,
						reader, detail);
				}
				finally
				{
					xnb.Close();
				}
			}

			Console.WriteLine();
			foreach (KeyValuePair<string, int> row in byReader.OrderByDescending(r => r.Value))
			{
				Console.WriteLine("{0,6}  {1}", row.Value, row.Key);
			}
			return 0;
		}

		/// <summary>
		/// Decodes a menu definition to XML, and checks the result by building it
		/// straight back: if the bytes differ, the decode lost something and the file
		/// should not be edited through this tool yet.
		/// </summary>
		private static int XbnDecode(string input, string output)
		{
			byte[] original = File.ReadAllBytes(input);
			XDocument document = MenuXbn.ToXml(original);
			output = output ?? Path.ChangeExtension(input, ".xml");

			XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = true,
				IndentChars = "  ",
				Encoding = new UTF8Encoding(false)
			};
			using (XmlWriter writer = XmlWriter.Create(output, settings))
			{
				document.Save(writer);
			}

			byte[] rebuilt = MenuXbn.FromXml(document);
			string verdict = rebuilt.SequenceEqual(original)
				? "round trips byte for byte"
				: "WARNING: rebuild differs from the original (" + rebuilt.Length
					+ " vs " + original.Length + " bytes)";

			Console.WriteLine("{0} -> {1}  ({2} nodes, {3})",
				Path.GetFileName(input), output,
				document.Descendants().Count(), verdict);
			return rebuilt.SequenceEqual(original) ? 0 : 1;
		}

		private static int XbnBuild(string input, string output)
		{
			XDocument document = XDocument.Load(input, LoadOptions.None);
			byte[] data = MenuXbn.FromXml(document);
			output = output ?? Path.ChangeExtension(input, ".xbn");
			File.WriteAllBytes(output, data);
			Console.WriteLine("{0} -> {1}  ({2} bytes)",
				Path.GetFileName(input), output, data.Length);
			return 0;
		}

		/// <summary>
		/// Decodes game text to JSON, one file or a whole directory tree, checking each
		/// by building it straight back and comparing against the original bytes.
		/// </summary>
		private static int MsdDecode(string input, string output)
		{
			if (Directory.Exists(input))
			{
				string outputDir = output ?? input;
				int files = 0;
				int messages = 0;
				int mismatched = 0;
				foreach (string file in Directory
					.EnumerateFiles(input, "*.msd", SearchOption.AllDirectories)
					.OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
				{
					string relative = Path.GetRelativePath(input, file);
					string destination = Path.Combine(outputDir,
						Path.ChangeExtension(relative, ".json"));
					Directory.CreateDirectory(Path.GetDirectoryName(destination));

					MsdFile decoded = WriteJson(file, destination, out bool exact);
					files++;
					messages += decoded.Messages.Count;
					if (!exact)
					{
						mismatched++;
						Console.Error.WriteLine("does not round trip: " + relative);
					}
				}
				Console.WriteLine("{0} files, {1} messages -> {2}",
					files, messages, Path.GetFullPath(outputDir));
				if (mismatched > 0)
				{
					Console.WriteLine("{0} file(s) did not round trip", mismatched);
				}
				return mismatched > 0 ? 1 : 0;
			}

			output = output ?? Path.ChangeExtension(input, ".json");
			MsdFile file2 = WriteJson(input, output, out bool byteExact);
			Console.WriteLine("{0} -> {1}  ({2}, {3})",
				Path.GetFileName(input), output, Msd.Describe(file2),
				byteExact ? "round trips byte for byte"
					: "WARNING: rebuild differs from the original");
			return byteExact ? 0 : 1;
		}

		private static MsdFile WriteJson(string input, string output, out bool byteExact)
		{
			byte[] original = File.ReadAllBytes(input);
			MsdFile decoded = Msd.Read(original);
			File.WriteAllText(output, JsonSerializer.Serialize(decoded, Msd.Json),
				new UTF8Encoding(false));
			byteExact = Msd.Write(decoded).SequenceEqual(original);
			return decoded;
		}

		private static int MsdBuild(string input, string output)
		{
			MsdFile file = JsonSerializer.Deserialize<MsdFile>(
				File.ReadAllText(input), Msd.Json);
			byte[] data = Msd.Write(file);
			output = output ?? Path.ChangeExtension(input, ".msd");
			File.WriteAllBytes(output, data);
			Console.WriteLine("{0} -> {1}  ({2}, {3} bytes)",
				Path.GetFileName(input), output, Msd.Describe(file), data.Length);
			return 0;
		}

		/// <summary>
		/// Disassembles event bytecode. With --text=&lt;dir&gt; pointing at decoded
		/// messages, the lines a script shows are written in beside the calls that
		/// show them, which is what makes a script readable rather than merely legal.
		/// </summary>
		private static int ScriptDump(string[] args)
		{
			string input = null;
			string output = null;
			string textDir = null;
			ScriptOpTable ops = GameOption(args);
			foreach (string arg in args)
			{
				if (arg.StartsWith("--text=", StringComparison.OrdinalIgnoreCase))
				{
					textDir = arg.Substring("--text=".Length).Trim('"');
				}
				else if (arg.StartsWith("--game=", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				else if (input == null)
				{
					input = arg;
				}
				else
				{
					output = arg;
				}
			}

			Func<uint, string> lookup = LoadMessages(textDir);
			List<string> files = File.Exists(input)
				? new List<string> { input }
				: Directory.EnumerateFiles(input, "*.script", SearchOption.AllDirectories)
					.OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
			if (files.Count == 0)
			{
				Console.Error.WriteLine("no .script files in " + input);
				return 1;
			}

			string outputDir = output ?? (File.Exists(input)
				? Path.GetDirectoryName(Path.GetFullPath(input)) : input);
			int instructions = 0;
			int failed = 0;
			int notExact = 0;
			foreach (string file in files)
			{
				string relative = File.Exists(input)
					? Path.GetFileName(file) : Path.GetRelativePath(input, file);
				string destination = Path.Combine(outputDir,
					Path.ChangeExtension(relative, ".ffs"));
				Directory.CreateDirectory(Path.GetDirectoryName(destination));
				try
				{
					byte[] original = File.ReadAllBytes(file);
					ScriptFile script = ScriptFile.Read(original, ops);
					using (StreamWriter writer = new StreamWriter(destination, false,
						new UTF8Encoding(false)))
					{
						Ffs.SourceWriter.Write(writer, script, relative, lookup);
					}
					instructions += ScriptDisassembler.Disassemble(script).Code.Count;

					// Compile what was just written and compare: a decompiler that
					// cannot feed its own compiler has lost something, and it is
					// better to say so here than to find out after an edit.
					if (!Ffs.Compiler.Compile(
							Ffs.Parser.Parse(File.ReadAllText(destination)), ops)
						.SequenceEqual(original))
					{
						notExact++;
						Console.Error.WriteLine("does not round trip: " + relative);
					}
				}
				catch (Exception ex)
				{
					Console.Error.WriteLine(relative + ": " + ex.Message);
					failed++;
				}
			}

			Console.WriteLine("{0} script(s), {1} instructions -> {2}",
				files.Count - failed, instructions, Path.GetFullPath(outputDir));
			if (failed > 0)
			{
				Console.WriteLine("{0} could not be read", failed);
			}
			Console.WriteLine(notExact == 0
				? "all of them compile back to the exact bytes they came from"
				: notExact + " do not compile back to the same bytes");
			return failed > 0 || notExact > 0 ? 1 : 0;
		}

		/// <summary>
		/// Decodes the cell banks, screens and animation to JSON.
		///
		/// The check here is that every part asks for a rectangle that is actually in
		/// the sheet it draws from. A misread field - a size where an offset should be,
		/// a stride off by two - sends those coordinates somewhere impossible almost
		/// immediately, so it catches a wrong layout without needing a round-trip.
		/// </summary>
		private static int CellsDump(string input, string output)
		{
			List<string> files = File.Exists(input)
				? new List<string> { input }
				: Directory.EnumerateFiles(input, "*.*", SearchOption.AllDirectories)
					.Where(p => p.EndsWith(".NCER", StringComparison.OrdinalIgnoreCase)
						|| p.EndsWith(".NSCR", StringComparison.OrdinalIgnoreCase)
						|| p.EndsWith(".NANR", StringComparison.OrdinalIgnoreCase))
					.OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
			if (files.Count == 0)
			{
				Console.Error.WriteLine("no .NCER, .NSCR or .NANR files in " + input);
				return 1;
			}

			string outputDir = output ?? (File.Exists(input)
				? Path.GetDirectoryName(Path.GetFullPath(input)) : input);
			Directory.CreateDirectory(outputDir);

			// Every picture in the tree, by name, so a part can be checked against the
			// sheet it claims to cut from.
			Dictionary<string, (int Width, int Height)> sheets =
				new Dictionary<string, (int, int)>(StringComparer.OrdinalIgnoreCase);
			string tree = File.Exists(input) ? Path.GetDirectoryName(Path.GetFullPath(input)) : input;
			foreach (string picture in Directory.EnumerateFiles(tree, "*.*", SearchOption.AllDirectories))
			{
				if (!picture.EndsWith(".NCGR", StringComparison.OrdinalIgnoreCase)
					&& !picture.EndsWith(".NCBR", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				try
				{
					Crystal.Editor.ImageInfo info =
						Crystal.Editor.Images.Describe(File.ReadAllBytes(picture));
					sheets[Path.GetFileName(picture)] = (info.Width, info.Height);
				}
				catch (Exception)
				{
					// Not a PNG after all, so nothing to check against.
				}
			}

			int banks = 0, screens = 0, animations = 0;
			int cells = 0, parts = 0, sequences = 0;
			int inside = 0, missingSheet = 0, framesAddUp = 0;
			List<string> problems = new List<string>();

			foreach (string file in files)
			{
				byte[] data = File.ReadAllBytes(file);
				string name = Path.GetFileName(file);
				object result;

				try
				{
					if (Cells.IsCellBank(data))
					{
						CellBank bank = Cells.ReadCellBank(data, name, sheets.ContainsKey);
						banks++;
						cells += bank.Cells.Count;

						bool known = sheets.TryGetValue(bank.Sheet ?? string.Empty,
							out (int Width, int Height) size);
						if (!known)
						{
							missingSheet++;
						}

						foreach (Cell cell in bank.Cells)
						{
							foreach (CellPart part in cell.Parts)
							{
								parts++;
								if (!known)
								{
									continue;
								}
								if (part.SourceX >= 0 && part.SourceY >= 0
									&& part.SourceX + part.Width <= size.Width
									&& part.SourceY + part.Height <= size.Height)
								{
									inside++;
								}
								else
								{
									problems.Add(string.Format(CultureInfo.InvariantCulture,
										"{0} cell {1}: wants {2}x{3} at {4},{5} from {6}, which is {7}x{8}",
										name, cell.Index, part.Width, part.Height,
										part.SourceX, part.SourceY, bank.Sheet,
										size.Width, size.Height));
								}
							}
						}
						result = bank;
					}
					else if (Cells.IsScreen(data))
					{
						screens++;
						result = Cells.ReadScreen(data, name);
					}
					else if (Cells.IsAnimation(data))
					{
						AnimBank bank = Cells.ReadAnimation(data, name);
						animations++;
						sequences += bank.Sequences.Count;
						if (bank.GotFrames == bank.TotalFrames)
						{
							framesAddUp++;
						}
						else
						{
							problems.Add(string.Format(CultureInfo.InvariantCulture,
								"{0}: read {1} frames, but the header says {2}",
								name, bank.GotFrames, bank.TotalFrames));
						}
						result = bank;
					}
					else
					{
						problems.Add(name + ": no CEBK, SCRN or ABNK block");
						continue;
					}
				}
				catch (Exception ex)
				{
					problems.Add(name + ": " + ex.Message);
					continue;
				}

				File.WriteAllText(Path.Combine(outputDir, name + ".json"),
					JsonSerializer.Serialize(result, Msd.Json));
			}

			Console.WriteLine("{0} cell bank(s) with {1} cells and {2} parts, "
				+ "{3} screen(s), {4} animation(s) with {5} sequences -> {6}",
				banks, cells, sequences == 0 && animations == 0 ? parts : parts,
				screens, animations, sequences, Path.GetFullPath(outputDir));
			int checkable = inside + problems.Count(p => p.Contains("wants", StringComparison.Ordinal));
			Console.WriteLine("parts whose source rectangle is inside their sheet: {0}/{1}",
				inside, checkable);
			if (animations > 0)
			{
				Console.WriteLine("animation banks whose frames add up to their header: {0}/{1}",
					framesAddUp, animations);
			}
			if (missingSheet > 0)
			{
				Console.WriteLine("{0} bank(s) name a sheet that is not in this tree", missingSheet);
			}
			if (problems.Count > 0)
			{
				Console.WriteLine("{0} problem(s):", problems.Count);
				foreach (string problem in problems.Take(10))
				{
					Console.WriteLine("   " + problem);
				}
			}
			return problems.Count > 0 ? 1 : 0;
		}

		/// <summary>
		/// Turns the models into OBJ, and checks each one against what it says about
		/// itself. Every model records its own vertex, triangle and quad counts and a
		/// bounding box; a walk that has lost its place will miss all four, so those
		/// are the test rather than a round-trip - there is nothing to write back to.
		/// </summary>
		/// <summary>A glTF as the game's own model: <name>.nmdp.lz and <name>.ntxp.lz in the output directory (the file's by default), then the result read back and checked.</summary>
		private static int MdlImport(string input, string name, string outputDir, float scale)
		{
			if (!File.Exists(input))
			{
				Console.Error.WriteLine("no such file: " + input);
				return 1;
			}
			outputDir ??= Path.GetDirectoryName(Path.GetFullPath(input));
			try
			{
				OpenFF.Graphics.GltfFile file = OpenFF.Graphics.GltfFile.Load(input);
				Mdl0Write.Result made = Mdl0Write.Build(file, name, scale);
				Directory.CreateDirectory(outputDir);
				string modelPath = Path.Combine(outputDir, name + ".nmdp.lz"), texPath = Path.Combine(outputDir, name + ".ntxp.lz");
				File.WriteAllBytes(modelPath, Lz.Compress(made.Nmdp));
				File.WriteAllBytes(texPath, Lz.Compress(made.Ntxp));
				Console.WriteLine(name + ": " + made.Triangles + " triangles, " + made.Vertices + " vertices, " + made.Materials + " material(s) -> " + modelPath + " (" + made.Nmdp.Length + " bytes) and " + texPath + " (" + made.Ntxp.Length + " bytes)");
				foreach (string note in file.Notes.Concat(made.Notes)) Console.WriteLine("  note: " + note);
				// Read back through the game's reader: the counts and the box as the writer meant them.
				List<Mdl0Model> models = Mdl0.Read(made.Nmdp);
				Tex0File textures = Tex0.Read(made.Ntxp);
				foreach (Mdl0Model m in models)
					Console.WriteLine("  read back: " + m.Name + " - " + m.Vertices + " vertices, " + m.Triangles + " triangles, " + m.Materials.Count + " material(s), " + m.Pieces.Count + " piece(s); box " + m.BoxW.ToString("0.##") + " x " + m.BoxH.ToString("0.##") + " x " + m.BoxD.ToString("0.##") + " at " + m.BoxX.ToString("0.##") + ", " + m.BoxY.ToString("0.##") + ", " + m.BoxZ.ToString("0.##") + "; textures " + string.Join(", ", textures.Textures.Select(t => t.Name + " " + t.Width + "x" + t.Height)));
				return 0;
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(name + ": " + ex.Message);
				return 1;
			}
		}

		private static int MdlDump(string input, string output)
		{
			List<string> files = File.Exists(input)
				? new List<string> { input }
				: Directory.EnumerateFiles(input, "*.nmdp.lz", SearchOption.AllDirectories)
					.OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
			if (files.Count == 0)
			{
				Console.Error.WriteLine("no .nmdp.lz files in " + input);
				return 1;
			}

			string outputDir = output ?? (File.Exists(input)
				? Path.GetDirectoryName(Path.GetFullPath(input)) : input);
			Directory.CreateDirectory(outputDir);

			int models = 0;
			int written = 0;
			int countsMatch = 0;
			int insideBox = 0;
			int empty = 0;
			List<string> problems = new List<string>();

			foreach (string file in files)
			{
				byte[] data;
				try
				{
					data = Lz.Decompress(File.ReadAllBytes(file));
				}
				catch (InvalidDataException)
				{
					problems.Add(Path.GetFileName(file) + ": will not decompress");
					continue;
				}

				if (Mdl0.Find(data) < 0)
				{
					continue;
				}

				string stem = Path.GetFileNameWithoutExtension(
					Path.GetFileNameWithoutExtension(file));

				List<Mdl0Model> found;
				try
				{
					found = Mdl0.Read(data);
				}
				catch (Exception ex)
				{
					problems.Add(stem + ": " + ex.Message);
					continue;
				}

				foreach (Mdl0Model model in found)
				{
					models++;
					if (model.GotVertices == 0)
					{
						empty++;
						continue;
					}

					if (model.GotVertices == model.Vertices
						&& model.GotTriangles == model.Triangles
						&& model.GotQuads == model.Quads)
					{
						countsMatch++;
					}
					else
					{
						problems.Add(string.Format(CultureInfo.InvariantCulture,
							"{0}: vertices {1}/{2}, triangles {3}/{4}, quads {5}/{6}",
							stem, model.GotVertices, model.Vertices,
							model.GotTriangles, model.Triangles,
							model.GotQuads, model.Quads));
					}

					if (FitsBox(model))
					{
						insideBox++;
					}
					else
					{
						problems.Add(stem + ": geometry is larger than the box it declares");
					}

					// The textures usually live in the .ntxp of the same name, and the
					// model that has its own TEX0 keeps it in the same file.
					Dictionary<string, string> textures = WriteTextures(file, outputDir, stem);
					Obj.Write(Path.Combine(outputDir, stem + ".obj"), model, textures);
					written++;
				}
			}

			Console.WriteLine("{0} model(s), {1} written as OBJ -> {2}",
				models, written, Path.GetFullPath(outputDir));
			Console.WriteLine("counts match the model's own: {0}/{1}", countsMatch, models - empty);
			Console.WriteLine("geometry fits the declared box: {0}/{1}", insideBox, models - empty);
			if (empty > 0)
			{
				Console.WriteLine("{0} model(s) hold no geometry at all", empty);
			}
			if (problems.Count > 0)
			{
				Console.WriteLine("{0} problem(s):", problems.Count);
				foreach (string problem in problems.Take(10))
				{
					Console.WriteLine("   " + problem);
				}
			}
			return problems.Count > 0 ? 1 : 0;
		}

		/// <summary>
		/// How far the geometry reaches, against the box size the model declares.
		///
		/// Size, not position. The box corner is not where the SBC ends up putting the
		/// pieces - it covers the model across its animation, so a resting pose sits
		/// somewhere inside it rather than on its corner - but the size is a real
		/// bound: measured against all 832 models, the span comes out exactly equal on
		/// 1768 of 2481 axes and larger on one. A walk that lost its place would blow
		/// straight through it.
		/// </summary>
		private static bool FitsBox(Mdl0Model model)
		{
			float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
			float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;

			foreach (Mdl0Piece piece in model.Pieces)
			{
				foreach (Mdl0Run run in piece.Runs)
				{
					foreach (Mdl0Vertex v in run.Vertices)
					{
						minX = Math.Min(minX, v.X); maxX = Math.Max(maxX, v.X);
						minY = Math.Min(minY, v.Y); maxY = Math.Max(maxY, v.Y);
						minZ = Math.Min(minZ, v.Z); maxZ = Math.Max(maxZ, v.Z);
					}
				}
			}

			if (minX > maxX)
			{
				return true;
			}

			float slack = 0.05f + Math.Max(model.BoxW, Math.Max(model.BoxH, model.BoxD)) * 0.01f;
			return maxX - minX <= model.BoxW + slack
				&& maxY - minY <= model.BoxH + slack
				&& maxZ - minZ <= model.BoxD + slack;
		}

		/// <summary>
		/// The PNGs a model's materials point at, written beside the OBJ. A model can
		/// carry its own TEX0 or borrow the .ntxp of the same name, so both are tried.
		/// </summary>
		private static Dictionary<string, string> WriteTextures(string file, string outputDir,
			string stem)
		{
			Dictionary<string, string> written =
				new Dictionary<string, string>(StringComparer.Ordinal);

			foreach (string source in new[] { file, file.Replace(".nmdp.lz", ".ntxp.lz") })
			{
				if (!File.Exists(source))
				{
					continue;
				}

				try
				{
					byte[] data = Lz.Decompress(File.ReadAllBytes(source));
					if (Tex0.Find(data) < 0)
					{
						continue;
					}

					Tex0File package = Tex0.Read(data);
					foreach (Tex0Texture texture in package.Textures)
					{
						if (texture.Problem != null || written.ContainsKey(texture.Name))
						{
							continue;
						}

						string safe = texture.Name;
						foreach (char bad in Path.GetInvalidFileNameChars())
						{
							safe = safe.Replace(bad, '_');
						}

						string name = stem + "." + safe + ".png";
						Png.Write(Path.Combine(outputDir, name), texture.Width, texture.Height,
							Tex0.Decode(package, texture));
						written[texture.Name] = name;
					}
				}
				catch (Exception)
				{
					// A model whose textures will not read is still a model worth having.
				}
			}
			return written;
		}

		/// <summary>
		/// Pulls every texture out of the NMDP packages as PNG. Reports what it found
		/// by format, because a format nothing uses is a format nothing has tested.
		/// </summary>
		private static int TexDump(string input, string output)
		{
			List<string> files = File.Exists(input)
				? new List<string> { input }
				: Directory.EnumerateFiles(input, "*.lz", SearchOption.AllDirectories)
					.Where(p => p.EndsWith(".nmdp.lz", StringComparison.OrdinalIgnoreCase)
						|| p.EndsWith(".ntxp.lz", StringComparison.OrdinalIgnoreCase))
					.OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
			if (files.Count == 0)
			{
				Console.Error.WriteLine("no .nmdp.lz or .ntxp.lz files in " + input);
				return 1;
			}

			string outputDir = output ?? (File.Exists(input)
				? Path.GetDirectoryName(Path.GetFullPath(input)) : input);
			Directory.CreateDirectory(outputDir);

			Dictionary<string, int> byFormat = new Dictionary<string, int>(StringComparer.Ordinal);
			int packages = 0;
			int written = 0;
			int noTextures = 0;
			List<string> failed = new List<string>();

			foreach (string file in files)
			{
				byte[] data;
				try
				{
					data = Lz.Decompress(File.ReadAllBytes(file));
				}
				catch (InvalidDataException)
				{
					failed.Add(Path.GetFileName(file) + ": will not decompress");
					continue;
				}

				if (Tex0.Find(data) < 0)
				{
					noTextures++;
					continue;
				}

				Tex0File package;
				try
				{
					package = Tex0.Read(data);
				}
				catch (Exception ex)
				{
					failed.Add(Path.GetFileName(file) + ": " + ex.Message);
					continue;
				}

				packages++;
				string stem = Path.GetFileNameWithoutExtension(
					Path.GetFileNameWithoutExtension(file));

				foreach (Tex0Texture texture in package.Textures)
				{
					byFormat.TryGetValue(texture.FormatName, out int seen);
					byFormat[texture.FormatName] = seen + 1;

					if (texture.Problem != null)
					{
						failed.Add(stem + "/" + texture.Name + ": " + texture.Problem);
						continue;
					}
					try
					{
						byte[] rgba = Tex0.Decode(package, texture);
						string safe = texture.Name;
						foreach (char bad in Path.GetInvalidFileNameChars())
						{
							safe = safe.Replace(bad, '_');
						}
						Png.Write(Path.Combine(outputDir, stem + "." + safe + ".png"),
							texture.Width, texture.Height, rgba);
						written++;
					}
					catch (Exception ex)
					{
						failed.Add(stem + "/" + texture.Name + ": " + ex.Message);
					}
				}
			}

			Console.WriteLine("{0} package(s) with textures, {1} without, {2} PNGs -> {3}",
				packages, noTextures, written, Path.GetFullPath(outputDir));
			Console.WriteLine("by format: " + string.Join(", ", byFormat
				.OrderByDescending(f => f.Value)
				.Select(f => string.Format(CultureInfo.InvariantCulture, "{0} {1}", f.Value, f.Key))));
			if (failed.Count > 0)
			{
				Console.WriteLine("{0} could not be decoded:", failed.Count);
				foreach (string problem in failed.Take(10))
				{
					Console.WriteLine("   " + problem);
				}
			}
			return failed.Count > 0 ? 1 : 0;
		}

		/// <summary>
		/// Decodes map placement, checking each by writing it back and comparing.
		/// </summary>
		/// <summary>
		/// Reads every collision mesh, and checks each one three ways: it round trips to
		/// the exact bytes it came from, every index in it points at something that
		/// exists, and the twelve exit attributes it uses match the exits its map's .pak
		/// declares. The last one is the real check - it is the claim the reader is
		/// making about what this file is for.
		/// </summary>
		private static int MclDump(string input, string output)
		{
			List<string> files = File.Exists(input)
				? new List<string> { input }
				: Directory.EnumerateFiles(input, "*.mcl.lz", SearchOption.AllDirectories)
					.Concat(Directory.EnumerateFiles(input, "*.mcl", SearchOption.AllDirectories))
					.OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
			if (files.Count == 0)
			{
				Console.Error.WriteLine("no .mcl files in " + input);
				return 1;
			}

			string outputDir = output ?? (File.Exists(input)
				? Path.GetDirectoryName(Path.GetFullPath(input)) : input);

			int objects = 0;
			int polygons = 0;
			int materials = 0;
			int withExits = 0;
			int exitSlots = 0;
			int mismatched = 0;
			int rebuilt = 0;
			int broken = 0;

			foreach (string file in files)
			{
				string relative = File.Exists(input)
					? Path.GetFileName(file) : Path.GetRelativePath(input, file);
				byte[] raw = File.ReadAllBytes(file);
				byte[] original = file.EndsWith(".lz", StringComparison.OrdinalIgnoreCase)
					? Lz.Decompress(raw) : raw;

				MclFile decoded;
				try
				{
					decoded = Mcl.Read(original);
				}
				catch (Exception problem)
				{
					broken++;
					Console.Error.WriteLine("will not read: " + relative + " - "
						+ problem.Message);
					continue;
				}

				objects += decoded.Objects.Count;
				HashSet<int> slots = new HashSet<int>();

				foreach (MclObject item in decoded.Objects)
				{
					polygons += item.Polygons.Count;
					materials += item.Materials.Count;

					// Every index has to point at something. A reader that has the layout
					// slightly wrong still produces numbers; what it does not produce is
					// numbers that all land inside the arrays they index.
					foreach (MclPolygon polygon in item.Polygons)
					{
						if (polygon.Material >= item.Materials.Count)
						{
							broken++;
							Console.Error.WriteLine(relative + ": a polygon wants material "
								+ polygon.Material + " of " + item.Materials.Count);
							break;
						}
						if (polygon.Vertex.Any(v => v >= item.Points.Count))
						{
							broken++;
							Console.Error.WriteLine(relative + ": a polygon wants a point "
								+ "past the end of " + item.Points.Count);
							break;
						}
					}

					foreach (MclBlock block in item.Blocks)
					{
						if (block.Polygons.Any(p => p >= item.Polygons.Count))
						{
							broken++;
							Console.Error.WriteLine(relative + ": a block wants a polygon "
								+ "past the end of " + item.Polygons.Count);
							break;
						}
					}

					for (int slot = 1; slot <= Mcl.JumpSlots; slot++)
					{
						int attribute = Mcl.JumpAttribute(slot);
						if (item.Materials.Any(m => m.Has(attribute))) slots.Add(slot);
					}
				}

				if (slots.Count > 0)
				{
					withExits++;
					exitSlots += slots.Count;
				}

				if (!Mcl.Write(decoded).SequenceEqual(original))
				{
					mismatched++;
					Console.Error.WriteLine("does not round trip: " + relative);
				}

				// The stronger claim: not just "the bytes go back where they were" but
				// "the layout is understood well enough to work out where they go". That
				// is what lets an array grow, so it is worth checking on every file
				// rather than trusting it once.
				if (!Mcl.Build(decoded).SequenceEqual(original))
				{
					rebuilt++;
					Console.Error.WriteLine("does not rebuild: " + relative);
				}

				string destination = Path.Combine(outputDir,
					Path.GetFileName(relative).Replace(".mcl.lz", ".mcl") + ".json");
				Directory.CreateDirectory(Path.GetDirectoryName(destination));
				File.WriteAllText(destination, JsonSerializer.Serialize(new
				{
					version = decoded.Version,
					size = decoded.Size,
					objects = decoded.Objects.Select(o => new
					{
						o.Name,
						polygons = o.Polygons.Count,
						points = o.Points.Count,
						materials = o.Materials.Count,
						blocks = o.BlockCount,
						exits = Enumerable.Range(1, Mcl.JumpSlots)
							.Where(s => o.Materials.Any(m => m.Has(Mcl.JumpAttribute(s))))
							.ToList(),
						attributes = o.Materials
							.SelectMany(m => m.Attributes())
							.Distinct()
							.OrderBy(a => a)
							.Select(Mcl.AttributeName)
							.ToList()
					}).ToList()
				}, Msd.Json), new UTF8Encoding(false));
			}

			Console.WriteLine("{0} mesh(es), {1} objects, {2} polygons, {3} materials -> {4}",
				files.Count, objects, polygons, materials, Path.GetFullPath(outputDir));
			Console.WriteLine("{0} of them carry exits, {1} slots between them",
				withExits, exitSlots);
			Console.WriteLine(broken == 0
				? "every index in every one of them points at something that exists"
				: broken + " have an index that does not");
			Console.WriteLine(mismatched == 0
				? "all of them write back to the exact bytes they came from"
				: mismatched + " do not");
			Console.WriteLine(rebuilt == 0
				? "and all of them come back byte for byte with every offset recomputed"
				: rebuilt + " do not survive a rebuild");
			return mismatched > 0 || broken > 0 || rebuilt > 0 ? 1 : 0;
		}

		private static int HichDump(string input, string output)
		{
			List<string> files = File.Exists(input)
				? new List<string> { input }
				: Directory.EnumerateFiles(input, "*.hich", SearchOption.AllDirectories)
					.OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
			if (files.Count == 0)
			{
				Console.Error.WriteLine("no .hich files in " + input);
				return 1;
			}

			string outputDir = output ?? (File.Exists(input)
				? Path.GetDirectoryName(Path.GetFullPath(input)) : input);
			int entries = 0;
			int placed = 0;
			int mismatched = 0;

			foreach (string file in files)
			{
				string relative = File.Exists(input)
					? Path.GetFileName(file) : Path.GetRelativePath(input, file);
				byte[] original = File.ReadAllBytes(file);
				List<HichEntry> decoded = Hich.Read(original);

				string destination = Path.Combine(outputDir,
					Path.ChangeExtension(relative, ".json"));
				Directory.CreateDirectory(Path.GetDirectoryName(destination));
				File.WriteAllText(destination,
					JsonSerializer.Serialize(decoded, Msd.Json), new UTF8Encoding(false));

				entries += decoded.Count;
				placed += decoded.Count(e => e.Kind == 0);
				if (!Hich.Write(decoded).SequenceEqual(original))
				{
					mismatched++;
					Console.Error.WriteLine("does not round trip: " + relative);
				}
			}

			Console.WriteLine("{0} map(s), {1} entries, {2} placed characters -> {3}",
				files.Count, entries, placed, Path.GetFullPath(outputDir));
			Console.WriteLine(mismatched == 0
				? "all of them write back to the exact bytes they came from"
				: mismatched + " do not");
			return mismatched > 0 ? 1 : 0;
		}

		/// <summary>
		/// Serves the editor. Everything it can do is something the command line can
		/// already do - the point is that a person can see what they are changing.
		/// </summary>
		/// <summary>
		/// Where the content is, from --content or by looking. Our own Content directory
		/// wins when it is there, because that is what somebody working on the port
		/// means; a Steam install is the answer for everybody else, and it is not a path
		/// worth making them type.
		/// </summary>
		private static string ContentOrFind(string given)
		{
			if (!string.IsNullOrEmpty(given))
			{
				return given;
			}
			// The repository's Content folder holds only the pipeline file and the override notes
			// unless somebody has put a game's data beside them; only then is it the content.
			if (Directory.Exists("Content") && (File.Exists(Path.Combine("Content", "data000.bin")) || Directory.Exists(Path.Combine("Content", "files"))))
			{
				return "Content";
			}
			return OpenFF.Content.SteamInstalls.FindOne() ?? "Content";
		}

		private static int ListProjects()
		{
			List<Crystal.Editor.Project> all = Crystal.Editor.Project.All();
			if (all.Count == 0)
			{
				Console.WriteLine("no projects yet in {0}",
					Crystal.Editor.Project.Root);
				Console.WriteLine("make one with: crystal editor --project=<name>");
				return 0;
			}
			foreach (Crystal.Editor.Project project in all)
			{
				Console.WriteLine("  {0}", project.File.Name);
				Console.WriteLine("    for  {0}", string.Join(", ",
					project.File.Targets.Select(Crystal.Editor.Targets.Describe)));
				Console.WriteLine("    in   {0}", project.Directory);
				Console.WriteLine();
			}
			return 0;
		}

		private static int ListInstalls()
		{
			List<OpenFF.Content.SteamInstall> found = OpenFF.Content.SteamInstalls.Find();
			found.AddRange(OpenFF.Content.SteamInstalls.Find(
				OpenFF.Content.SteamInstalls.Ff4AppId));
			if (found.Count == 0)
			{
				Console.WriteLine("no Steam copy of either game found on this machine");
				Console.WriteLine("(looked for appmanifest_{0}.acf and appmanifest_{1}.acf in every Steam library)",
					OpenFF.Content.SteamInstalls.AppId, OpenFF.Content.SteamInstalls.Ff4AppId);
				return 1;
			}
			foreach (OpenFF.Content.SteamInstall install in found)
			{
				Console.WriteLine("  {0}", install.Name ?? "Final Fantasy III");
				Console.WriteLine("  {0}", install.Path);
				Console.WriteLine();
			}
			return 0;
		}

		/// <summary>install and uninstall, which differ only in which way they copy.</summary>
		private static int ModCommand(string[] args, bool install)
		{
			string content = null;
			string overrides = null;
			foreach (string arg in args)
			{
				if (arg.StartsWith("--content=", StringComparison.OrdinalIgnoreCase))
				{
					content = arg.Substring("--content=".Length).Trim('"');
				}
				else if (arg.StartsWith("--override=", StringComparison.OrdinalIgnoreCase))
				{
					overrides = arg.Substring("--override=".Length).Trim('"');
				}
			}

			Editor.Workspace workspace;
			try
			{
				workspace = new Editor.Workspace(ContentOrFind(content), overrides);
			}
			catch (Exception ex) when (ex is FileNotFoundException or IOException)
			{
				Console.Error.WriteLine(ex.Message);
				return 1;
			}

			Console.WriteLine("  content   {0} ({1})", workspace.ContentDirectory, workspace.Kind);
			Console.WriteLine("  mod       {0}", workspace.OverrideDirectory);
			Console.WriteLine("  originals {0}", Crystal.Editor.ModInstall.BackupDirectory(workspace));
			Console.WriteLine();

			Crystal.Editor.ModResult result = install
				? Crystal.Editor.ModInstall.Install(workspace)
				: Crystal.Editor.ModInstall.Uninstall(workspace);

			if (!result.Ok)
			{
				Console.Error.WriteLine(result.Error);
				return 1;
			}

			foreach ((string what, List<string> names) in new[]
			{
				("wrote", result.Wrote), ("restored", result.Restored),
				("removed", result.Removed), ("left alone", result.Skipped)
			})
			{
				foreach (string name in names)
				{
					Console.WriteLine("  {0,-10} {1}", what, name);
				}
			}
			foreach (string note in result.Notes)
			{
				Console.WriteLine("  note       {0}", note);
			}

			Console.WriteLine();
			Console.WriteLine("{0} file(s) written, {1} restored, {2} removed, {3} left alone",
				result.Wrote.Count, result.Restored.Count,
				result.Removed.Count, result.Skipped.Count);
			return 0;
		}

		private static int Editor(string[] args)
		{
			string content = null;
			string overrides = null;
			string language = "en";
			string projectName = null;
			string target = null;
			bool openBrowser = true;
			int port = 5050;

			foreach (string arg in args)
			{
				if (arg.StartsWith("--content=", StringComparison.OrdinalIgnoreCase))
				{
					content = arg.Substring("--content=".Length).Trim('"');
				}
				else if (arg.StartsWith("--override=", StringComparison.OrdinalIgnoreCase))
				{
					overrides = arg.Substring("--override=".Length).Trim('"');
				}
				else if (arg.StartsWith("--language=", StringComparison.OrdinalIgnoreCase))
				{
					language = arg.Substring("--language=".Length).Trim('"');
				}
				else if (arg.StartsWith("--project=", StringComparison.OrdinalIgnoreCase))
				{
					projectName = arg.Substring("--project=".Length).Trim('"');
				}
				else if (arg.StartsWith("--target=", StringComparison.OrdinalIgnoreCase))
				{
					target = arg.Substring("--target=".Length).Trim('"');
				}
				else if (arg.Equals("--no-browser", StringComparison.OrdinalIgnoreCase))
				{
					openBrowser = false;
				}
				else if (arg.StartsWith("--port=", StringComparison.OrdinalIgnoreCase))
				{
					port = int.Parse(arg.Substring("--port=".Length),
						CultureInfo.InvariantCulture);
				}
			}

			// BaseDirectory rather than Assembly.Location: published as a single file
			// - which is how somebody who does not have the SDK gets this - Location
			// is the empty string, and the editor would go looking for its own web
			// files in the root of the drive.
			string webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
			if (!Directory.Exists(webRoot))
			{
				Console.Error.WriteLine("the editor's files are missing from " + webRoot);
				return 1;
			}

			// A project decides both halves - which game's content to read, and where
			// the edits go - so it is resolved before the workspace. An explicit
			// --content or --override still wins, for opening something once without
			// making a project of it.
			// --target on its own opens that game's install without a project; it used
			// to be read only alongside --project and otherwise quietly opened FF3.
			if (projectName == null && content == null && target != null)
			{
				content = Crystal.Editor.Targets.Find(target);
				if (content == null)
				{
					Console.Error.WriteLine("no install found for "
						+ Crystal.Editor.Targets.Describe(target));
					return 1;
				}
			}

			Crystal.Editor.Project project = null;
			if (projectName != null)
			{
				project = OpenOrCreateProject(projectName, target);
				if (project == null)
				{
					return 1;
				}
				content ??= SafeContentOf(project);
				overrides ??= project.FilesFor(project.File.Active);
				if (content == null)
				{
					return 1;
				}
			}

			// A project opens every game it targets, each as its own session, so the
			// two can be worked on side by side. --content on its own opens one.
			if (project != null)
			{
				try
				{
					Editor.EditorServer forProject = new Editor.EditorServer(
						Path.GetFullPath(webRoot), language, project);
					Console.WriteLine("  project   {0} in {1}", project.File.Name, project.Directory);
					forProject.Run(port, openBrowser ? OpenInBrowser : null);
					return 0;
				}
				catch (Exception ex) when (ex is FileNotFoundException or IOException)
				{
					Console.Error.WriteLine(ex.Message);
					return 1;
				}
			}

			Editor.Workspace workspace;
			try
			{
				workspace = new Editor.Workspace(ContentOrFind(content), overrides);
			}
			catch (Exception ex) when (ex is FileNotFoundException or IOException)
			{
				Console.Error.WriteLine(ex.Message);
				return 1;
			}

			// Text is read through the workspace rather than a folder of extracted
			// files, so a line edited in the editor is visible everywhere at once.
			Editor.MessageIndex messages = new Editor.MessageIndex(workspace, language);
			Console.WriteLine("  language  {0} ({1} messages)", language, messages.Count);
			if (project != null)
			{
				Console.WriteLine("  project   {0} ({1}) in {2}",
					project.File.Name,
					Crystal.Editor.Targets.Describe(project.File.Active),
					project.Directory);
			}

			new Editor.EditorServer(workspace, Path.GetFullPath(webRoot), messages,
				language, project)
				.Run(port, openBrowser ? OpenInBrowser : null);
			return 0;
		}

		/// <summary>A project by name, made if it is not there yet.</summary>
		private static Crystal.Editor.Project OpenOrCreateProject(
			string name, string target)
		{
			// A path is taken as a path, so an existing mod folder can be opened from
			// anywhere; anything else is a name under the projects directory.
			Crystal.Editor.Project project =
				name.IndexOfAny(new[] { '/', '\\' }) >= 0 || Path.IsPathRooted(name)
					? Crystal.Editor.Project.TryOpen(name)
					: Crystal.Editor.Project.All()
						.FirstOrDefault(p => string.Equals(p.File.Name, name,
							StringComparison.OrdinalIgnoreCase));

			if (project == null)
			{
				try
				{
					project = Crystal.Editor.Project.Create(
						Path.GetFileName(name.TrimEnd('/', '\\')),
						new[] { target ?? Crystal.Editor.Targets.Steam });
					Console.WriteLine("  created   {0}", project.Directory);
				}
				catch (Exception ex) when (ex is ArgumentException or IOException)
				{
					Console.Error.WriteLine(ex.Message);
					return null;
				}
			}

			if (target != null)
			{
				if (!Crystal.Editor.Targets.Known(target))
				{
					Console.Error.WriteLine("no target called {0} - try {1}", target,
						string.Join(" or ", Crystal.Editor.Targets.All));
					return null;
				}
				project.File.Active = target;
				if (!project.File.Targets.Contains(target, StringComparer.OrdinalIgnoreCase))
				{
					project.File.Targets.Add(target);
				}
				project.Save();
			}
			return project;
		}

		private static string SafeContentOf(Crystal.Editor.Project project)
		{
			try
			{
				return project.ContentDirectory();
			}
			catch (FileNotFoundException ex)
			{
				Console.Error.WriteLine(ex.Message);
				return null;
			}
		}

		/// <summary>
		/// Opens the page. Without this the editor prints a URL and waits, and somebody
		/// who does not read the console never learns there is a UI at all.
		/// </summary>
		private static void OpenInBrowser(string url)
		{
			try
			{
				// UseShellExecute, because the URL has to go to whatever the machine
				// says is a browser rather than be run as a program.
				System.Diagnostics.Process.Start(
					new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
				Console.WriteLine("  opened it in your browser. --no-browser if you would rather it did not.");
			}
			catch (Exception ex)
			{
				// No browser, no default handler, a locked down machine. Not worth
				// failing to start over - the URL is on the console either way - but
				// worth saying, or the page simply never appears and nobody knows why.
				Console.WriteLine("  could not open a browser ({0}) - open the address above yourself.",
					ex.Message);
			}
		}

		/// <summary>
		/// Decompresses, and checks each result two ways: it has to come out the size
		/// the header promised, and compressing it again has to decompress back to the
		/// same bytes. The second is what says the compressor here is usable for
		/// putting edited content back.
		/// </summary>
		private static int LzDecompress(string input, string output)
		{
			List<string> files = File.Exists(input)
				? new List<string> { input }
				: Directory.EnumerateFiles(input, "*.lz", SearchOption.AllDirectories)
					.OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
			if (files.Count == 0)
			{
				Console.Error.WriteLine("no .lz files in " + input);
				return 1;
			}

			string outputDir = output ?? (File.Exists(input)
				? Path.GetDirectoryName(Path.GetFullPath(input)) : input);
			int done = 0;
			int failed = 0;
			int notReversible = 0;
			long compressed = 0;
			long plain = 0;

			foreach (string file in files)
			{
				string relative = File.Exists(input)
					? Path.GetFileName(file) : Path.GetRelativePath(input, file);
				try
				{
					byte[] source = File.ReadAllBytes(file);
					byte[] data = Lz.Decompress(source);

					string destination = Path.Combine(outputDir,
						Path.ChangeExtension(relative, null));
					Directory.CreateDirectory(Path.GetDirectoryName(destination));
					File.WriteAllBytes(destination, data);

					compressed += source.Length;
					plain += data.Length;
					done++;

					if (!Lz.Decompress(Lz.Compress(data)).SequenceEqual(data))
					{
						notReversible++;
						Console.Error.WriteLine("does not survive recompression: " + relative);
					}
				}
				catch (InvalidDataException ex)
				{
					failed++;
					Console.Error.WriteLine(relative + ": " + ex.Message);
				}
			}

			Console.WriteLine("{0} file(s), {1:N0} -> {2:N0} bytes -> {3}",
				done, compressed, plain, Path.GetFullPath(outputDir));
			if (failed > 0)
			{
				Console.WriteLine("{0} could not be decompressed", failed);
			}
			Console.WriteLine(notReversible == 0
				? "all of them compress back to something that decompresses identically"
				: notReversible + " do not survive recompression");
			return failed > 0 || notReversible > 0 ? 1 : 0;
		}

		private static int LzCompress(string input, string output)
		{
			byte[] data = File.ReadAllBytes(input);
			byte[] packed = Lz.Compress(data);
			output = output ?? input + ".lz";
			File.WriteAllBytes(output, packed);
			Console.WriteLine("{0} -> {1}  ({2:N0} -> {3:N0} bytes, {4:P0})",
				Path.GetFileName(input), output, data.Length, packed.Length,
				data.Length == 0 ? 0 : (double)packed.Length / data.Length);
			return 0;
		}

		/// <summary>
		/// The instruction set, as the script language spells it. Without this, writing
		/// a script means grepping the disassembly for something that looks right.
		/// </summary>
		private static int Ops(string filter, ScriptOpTable ops)
		{
			int shown = 0;
			foreach (KeyValuePair<string, int> entry in ops.Names.All
				.OrderBy(e => e.Key, StringComparer.OrdinalIgnoreCase))
			{
				if (filter != null
					&& entry.Key.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
				{
					continue;
				}

				ScriptOp op = ops.Get(entry.Value);
				string arguments = op.Operands == null || op.Operands.Length == 0
					? string.Empty
					: string.Join(", ", op.Operands
						.Select((operand, i) => Describe(ops, entry.Value, i, operand)));
				Console.WriteLine("{0,4}  {1,-42} {2}", entry.Value, entry.Key, arguments);
				shown++;
			}
			Console.WriteLine();
			Console.WriteLine("{0} instruction(s)", shown);
			return 0;
		}

		/// <summary>An operand as "name:type", with the type alone where no name is known.</summary>
		private static string Describe(ScriptOpTable ops, int opcode, int index, Operand operand)
		{
			string type;
			switch (operand)
			{
				case Operand.Byte: type = "byte"; break;
				case Operand.Word: type = "word"; break;
				case Operand.Dword: type = "dword"; break;
				default: type = "string"; break;
			}
			if (ops.IsFixed(opcode, index))
			{
				type += " fixed";
			}
			string name = ops.OperandName(opcode, index);
			return name == null ? type : name + ":" + type;
		}

		/// <summary>
		/// Which game's command table a script command works against: --game=ff4 for
		/// FF4 3D, FF3's otherwise. The bytecode does not say; the same numbers mean
		/// different instructions in the two games.
		/// </summary>
		private static ScriptOpTable GameOption(string[] args)
		{
			string game = args.FirstOrDefault(a => a.StartsWith("--game=", StringComparison.OrdinalIgnoreCase));
			return ScriptOpTable.For(game?.Substring("--game=".Length).Trim('"'));
		}

		/// <summary>Compiles script language source back to bytecode.</summary>
		private static int ScriptBuild(string input, string output, ScriptOpTable ops)
		{
			List<string> files = File.Exists(input)
				? new List<string> { input }
				: Directory.EnumerateFiles(input, "*.ffs", SearchOption.AllDirectories)
					.OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
			if (files.Count == 0)
			{
				Console.Error.WriteLine("no .ffs files in " + input);
				return 1;
			}

			string outputDir = output ?? (File.Exists(input)
				? Path.GetDirectoryName(Path.GetFullPath(input)) : input);
			int built = 0;
			int failed = 0;
			foreach (string file in files)
			{
				string relative = File.Exists(input)
					? Path.GetFileName(file) : Path.GetRelativePath(input, file);
				try
				{
					Ffs.ScriptDocument document = Ffs.Parser.Parse(File.ReadAllText(file));
					byte[] data = Ffs.Compiler.Compile(document, ops);

					string destination = Path.Combine(outputDir,
						Path.ChangeExtension(relative, ".script"));
					Directory.CreateDirectory(Path.GetDirectoryName(destination));
					File.WriteAllBytes(destination, data);
					built++;
					if (files.Count == 1)
					{
						Console.WriteLine("{0} -> {1}  ({2} bytes)",
							relative, destination, data.Length);
					}
				}
				catch (Ffs.ScriptSyntaxException ex)
				{
					Console.Error.WriteLine(relative + ": " + ex.Message);
					failed++;
				}
				catch (Ffs.ScriptCompileException ex)
				{
					Console.Error.WriteLine(relative + ": " + ex.Message);
					failed++;
				}
			}

			if (files.Count > 1)
			{
				Console.WriteLine("{0} script(s) built -> {1}", built, Path.GetFullPath(outputDir));
			}
			if (failed > 0)
			{
				Console.WriteLine("{0} failed", failed);
			}
			return failed > 0 ? 1 : 0;
		}

		/// <summary>Message id -> text, from a directory of decoded .msd JSON.</summary>
		private static Func<uint, string> LoadMessages(string textDir)
		{
			if (string.IsNullOrEmpty(textDir) || !Directory.Exists(textDir))
			{
				return null;
			}
			Dictionary<uint, string> messages = new Dictionary<uint, string>();
			foreach (string file in Directory.EnumerateFiles(textDir, "*.json",
				SearchOption.AllDirectories))
			{
				MsdFile decoded;
				try
				{
					decoded = JsonSerializer.Deserialize<MsdFile>(File.ReadAllText(file), Msd.Json);
				}
				catch (JsonException)
				{
					continue;               // not one of ours
				}
				foreach (MsdMessage message in decoded.Messages)
				{
					if (message.Pages.Count > 0 && !messages.ContainsKey(message.Id))
					{
						messages[message.Id] = message.Pages[0];
					}
				}
			}
			Console.WriteLine("{0} messages available for annotation", messages.Count);
			return id => messages.TryGetValue(id, out string text) ? text : null;
		}

		/// <summary>
		/// Decodes parameter tables to JSON, checking each by building it straight back
		/// and comparing against the original bytes.
		/// </summary>
		private static int PakDecode(string[] args)
		{
			string input = null;
			string output = null;
			string textDir = null;
			foreach (string arg in args)
			{
				if (arg.StartsWith("--", StringComparison.Ordinal) && !arg.StartsWith("--text=", StringComparison.OrdinalIgnoreCase))
				{
					continue;                                // --game=ff4 and friends are not paths
				}
				if (arg.StartsWith("--text=", StringComparison.OrdinalIgnoreCase))
				{
					textDir = arg.Substring("--text=".Length).Trim('"');
				}
				else if (input == null)
				{
					input = arg;
				}
				else
				{
					output = arg;
				}
			}

			Func<uint, string> lookup = LoadMessages(textDir);
			List<string> files = File.Exists(input)
				? new List<string> { input }
				: Directory.EnumerateFiles(input, "*.pak", SearchOption.AllDirectories)
					.Concat(Directory.EnumerateFiles(input, "*.chaindata", SearchOption.AllDirectories))
					.OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
			if (files.Count == 0)
			{
				Console.Error.WriteLine("no .pak or .chaindata files in " + input);
				return 1;
			}

			string outputDir = output ?? (File.Exists(input)
				? Path.GetDirectoryName(Path.GetFullPath(input)) : input);
			int written = 0;
			int mismatched = 0;
			int skipped = 0;
			foreach (string file in files)
			{
				string relative = File.Exists(input)
					? Path.GetFileName(file) : Path.GetRelativePath(input, file);
				try
				{
					byte[] original = File.ReadAllBytes(file);
					int chains = original.Length >= 4
						? BitConverter.ToInt32(original, 0) : 0;
					string family = Pak.FamilyOf(file, chains, GameOption(args).Game);
					PakFile decoded = Pak.Read(original, family, lookup);

					string destination = Path.Combine(outputDir,
						Path.ChangeExtension(relative, ".json"));
					Directory.CreateDirectory(Path.GetDirectoryName(destination));
					File.WriteAllText(destination,
						JsonSerializer.Serialize(decoded, Pak.Json), new UTF8Encoding(false));
					written++;

					if (!Pak.Write(decoded).SequenceEqual(original))
					{
						mismatched++;
						Console.Error.WriteLine("does not round trip: " + relative);
					}
					if (files.Count == 1)
					{
						Console.WriteLine("{0} -> {1}  ({2}, family {3}, {4})",
							Path.GetFileName(file), destination, Pak.Describe(decoded),
							family ?? "unknown",
							mismatched == 0 ? "round trips byte for byte"
								: "WARNING: rebuild differs");
					}
				}
				catch (InvalidDataException ex)
				{
					skipped++;
					Console.Error.WriteLine(relative + ": " + ex.Message);
				}
			}

			if (files.Count > 1)
			{
				Console.WriteLine("{0} file(s) -> {1}", written, Path.GetFullPath(outputDir));
				if (skipped > 0)
				{
					Console.WriteLine("{0} were not paks", skipped);
				}
				if (mismatched > 0)
				{
					Console.WriteLine("{0} did not round trip", mismatched);
				}
			}
			return mismatched > 0 ? 1 : 0;
		}

		private static int PakBuild(string input, string output)
		{
			PakFile file = JsonSerializer.Deserialize<PakFile>(
				File.ReadAllText(input), Pak.Json);
			byte[] data = Pak.Write(file);
			output = output ?? Path.ChangeExtension(input, ".pak");
			File.WriteAllBytes(output, data);
			Console.WriteLine("{0} -> {1}  ({2}, {3} bytes)",
				Path.GetFileName(input), output, Pak.Describe(file), data.Length);
			return 0;
		}

		private static int Extract(string inputDir, string outputDir)
		{
			string fontDir = Path.Combine(outputDir, "Fonts");
			string audioDir = Path.Combine(outputDir, "Audio");
			Directory.CreateDirectory(fontDir);
			Directory.CreateDirectory(audioDir);

			List<string> fonts = new List<string>();
			List<string> sounds = new List<string>();
			List<string> skipped = new List<string>();

			foreach (string file in XnbFiles(inputDir))
			{
				string name = Path.GetFileNameWithoutExtension(file);
				Xnb xnb = Xnb.Open(file);
				try
				{
					switch (xnb.PrimaryReaderName)
					{
						case "SpriteFontReader":
							ExtractFont(xnb, name, fontDir);
							fonts.Add(name);
							break;
						case "SoundEffectReader":
							ExtractSound(xnb, name, audioDir);
							sounds.Add(name);
							break;
						default:
							skipped.Add(name + " (" + xnb.PrimaryReaderName + ")");
							break;
					}
				}
				catch (Exception ex)
				{
					skipped.Add(name + " (" + ex.Message + ")");
				}
				finally
				{
					xnb.Close();
				}
			}

			WriteMgcb(outputDir, fonts, sounds);

			Console.WriteLine("fonts:   {0}", fonts.Count);
			Console.WriteLine("audio:   {0}", sounds.Count);
			Console.WriteLine("skipped: {0}", skipped.Count);
			foreach (string s in skipped.Take(20))
			{
				Console.WriteLine("  " + s);
			}
			Console.WriteLine();
			Console.WriteLine("written to " + Path.GetFullPath(outputDir));
			return 0;
		}

		private static void ExtractFont(Xnb xnb, string name, string dir)
		{
			XnbSpriteFont font = xnb.ReadSpriteFont();
			XnbTexture texture = font.Texture;

			if (!SurfaceFormats.CanDecode(texture.SurfaceFormat))
			{
				throw new NotSupportedException("surface format "
					+ SurfaceFormats.Name(texture.SurfaceFormat) + " not decodable yet");
			}

			byte[] rgba = SurfaceFormats.ToRgba(
				texture.SurfaceFormat, texture.Width, texture.Height, texture.Mips[0]);
			Png.Write(Path.Combine(dir, name + ".png"), texture.Width, texture.Height, rgba);

			// Metrics as JSON: enough to rebuild the font or to check the atlas by hand.
			StringBuilder json = new StringBuilder();
			json.AppendLine("{");
			json.AppendFormat(CultureInfo.InvariantCulture,
				"  \"name\": \"{0}\",\n  \"width\": {1},\n  \"height\": {2},\n  \"surfaceFormat\": \"{3}\",\n",
				name, texture.Width, texture.Height, SurfaceFormats.Name(texture.SurfaceFormat));
			json.AppendFormat(CultureInfo.InvariantCulture,
				"  \"lineSpacing\": {0},\n  \"spacing\": {1},\n",
				font.LineSpacing, font.Spacing.ToString("R", CultureInfo.InvariantCulture));
			json.AppendFormat(CultureInfo.InvariantCulture,
				"  \"defaultCharacter\": {0},\n",
				font.DefaultCharacter.HasValue
					? ((int)font.DefaultCharacter.Value).ToString(CultureInfo.InvariantCulture)
					: "null");
			json.AppendLine("  \"glyphs\": [");
			for (int i = 0; i < font.Characters.Count; i++)
			{
				int[] g = i < font.Glyphs.Count ? font.Glyphs[i] : new[] { 0, 0, 0, 0 };
				int[] c = i < font.Cropping.Count ? font.Cropping[i] : new[] { 0, 0, 0, 0 };
				float[] k = i < font.Kerning.Count ? font.Kerning[i] : new float[] { 0, 0, 0 };
				json.AppendFormat(CultureInfo.InvariantCulture,
					"    {{ \"char\": {0}, \"bounds\": [{1},{2},{3},{4}], \"cropping\": [{5},{6},{7},{8}], \"kerning\": [{9},{10},{11}] }}{12}\n",
					(int)font.Characters[i],
					g[0], g[1], g[2], g[3], c[0], c[1], c[2], c[3],
					k[0].ToString("R", CultureInfo.InvariantCulture),
					k[1].ToString("R", CultureInfo.InvariantCulture),
					k[2].ToString("R", CultureInfo.InvariantCulture),
					i == font.Characters.Count - 1 ? string.Empty : ",");
			}
			json.AppendLine("  ]");
			json.AppendLine("}");
			File.WriteAllText(Path.Combine(dir, name + ".json"), json.ToString());
		}

		private static void ExtractSound(Xnb xnb, string name, string dir)
		{
			XnbSoundEffect sound = xnb.ReadSoundEffect();
			Wav.Write(Path.Combine(dir, name + ".wav"), sound.Format, sound.Data);
		}

		/// <summary>
		/// A pipeline project for the extracted sources. Fonts come back as textures
		/// rather than .spritefont descriptions, because the originals are pre-rendered
		/// atlases whose exact glyph boxes the game indexes by page - regenerating them
		/// from a system font would not reproduce those.
		/// </summary>
		private static void WriteMgcb(string outputDir, List<string> fonts, List<string> sounds)
		{
			StringBuilder mgcb = new StringBuilder();
			mgcb.AppendLine("# ----------------------------- Global Properties ----------------------------#");
			mgcb.AppendLine();
			mgcb.AppendLine("/outputDir:bin/$(Platform)");
			mgcb.AppendLine("/intermediateDir:obj/$(Platform)");
			mgcb.AppendLine("/platform:DesktopGL");
			mgcb.AppendLine("/config:");
			mgcb.AppendLine("/profile:Reach");
			mgcb.AppendLine("/compress:False");
			mgcb.AppendLine();
			mgcb.AppendLine("# -------------------------------- References --------------------------------#");
			mgcb.AppendLine();
			mgcb.AppendLine("# ---------------------------------- Content ---------------------------------#");
			mgcb.AppendLine();

			foreach (string name in sounds)
			{
				mgcb.AppendLine("#begin Audio/" + name + ".wav");
				mgcb.AppendLine("/importer:WavImporter");
				mgcb.AppendLine("/processor:SoundEffectProcessor");
				mgcb.AppendLine("/processorParam:Quality=Best");
				mgcb.AppendLine("/build:Audio/" + name + ".wav");
				mgcb.AppendLine();
			}

			foreach (string name in fonts)
			{
				mgcb.AppendLine("#begin Fonts/" + name + ".png");
				mgcb.AppendLine("/importer:TextureImporter");
				mgcb.AppendLine("/processor:TextureProcessor");
				mgcb.AppendLine("/processorParam:ColorKeyEnabled=False");
				mgcb.AppendLine("/processorParam:GenerateMipmaps=False");
				mgcb.AppendLine("/processorParam:PremultiplyAlpha=True");
				mgcb.AppendLine("/processorParam:TextureFormat=Color");
				mgcb.AppendLine("/build:Fonts/" + name + ".png");
				mgcb.AppendLine();
			}

			File.WriteAllText(Path.Combine(outputDir, "Content.mgcb"), mgcb.ToString());
		}
	}
}
