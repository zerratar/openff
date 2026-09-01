// FF3 content tool.
//
//   dotnet run --project FF3.ContentTool -- info    <file-or-dir>
//   dotnet run --project FF3.ContentTool -- extract <xnb-dir> <out-dir>
//   dotnet run --project FF3.ContentTool -- archives         <content-dir>
//   dotnet run --project FF3.ContentTool -- extract-archives <content-dir> <out-dir> [pattern ...]
//   dotnet run --project FF3.ContentTool -- xbn        <file.xbn> [out.xml]
//   dotnet run --project FF3.ContentTool -- xbn-build  <file.xml> [out.xbn]
//   dotnet run --project FF3.ContentTool -- msd        <file.msd | dir> [out]
//   dotnet run --project FF3.ContentTool -- msd-build  <file.json> [out.msd]
//   dotnet run --project FF3.ContentTool -- script     <file.script | dir> [out] [--text=<dir>]
//   dotnet run --project FF3.ContentTool -- pak        <file.pak | dir> [out]
//   dotnet run --project FF3.ContentTool -- pak-build  <file.json> [out.pak]
//
// "extract" turns the shipped .xnb files back into editable sources:
//   Fonts/<name>.png   + <name>.json   glyph atlas and metrics
//   Audio/<name>.wav                   PCM/ADPCM audio, straight from the XNB
// and writes a Content.mgcb so the MonoGame pipeline can rebuild them.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

namespace FF3.ContentTool
{
	internal static class Program
	{
		private static int Main(string[] args)
		{
			if (args.Length == 0)
			{
				Usage();
				return 1;
			}

			try
			{
				switch (args[0].ToLowerInvariant())
				{
					case "info":
						return Info(args.Length > 1 ? args[1] : ".");
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
					case "pak":
						if (args.Length < 2)
						{
							Usage();
							return 1;
						}
						return PakDecode(args.Skip(1).ToArray());
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

		private static void Usage()
		{
			Console.Error.WriteLine("usage:");
			Console.Error.WriteLine("  info    <file.xnb | directory>");
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
			Console.Error.WriteLine("  script     <file.script | dir> [out] [--text=<dir>]");
			Console.Error.WriteLine("                                    event bytecode -> disassembly");
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
			foreach (string arg in args)
			{
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
			foreach (string file in files)
			{
				string relative = File.Exists(input)
					? Path.GetFileName(file) : Path.GetRelativePath(input, file);
				string destination = Path.Combine(outputDir,
					Path.ChangeExtension(relative, ".txt"));
				Directory.CreateDirectory(Path.GetDirectoryName(destination));
				try
				{
					ScriptFile script = ScriptFile.Read(File.ReadAllBytes(file));
					using (StreamWriter writer = new StreamWriter(destination, false,
						new UTF8Encoding(false)))
					{
						ScriptDisassembler.Write(writer, script, relative, lookup);
					}
					instructions += ScriptDisassembler.Disassemble(script).Code.Count;
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
					string family = Pak.FamilyOf(file, chains);
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
