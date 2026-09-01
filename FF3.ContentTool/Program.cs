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
//   dotnet run --project FF3.ContentTool -- script       <file.script | dir> [out] [--text=<dir>]
//   dotnet run --project FF3.ContentTool -- script-build <file.ffs | dir> [out]
//   dotnet run --project FF3.ContentTool -- ops [filter]
//   dotnet run --project FF3.ContentTool -- editor [--content=<dir>] [--port=5050]
//   dotnet run --project FF3.ContentTool -- lz          <file.lz | dir> [out]
//   dotnet run --project FF3.ContentTool -- lz-compress <file> [out.lz]
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
						return Ops(args.Length > 1 ? args[1] : null);
					case "script-build":
						if (args.Length < 2)
						{
							Usage();
							return 1;
						}
						return ScriptBuild(args[1], args.Length > 2 ? args[2] : null);
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
			Console.Error.WriteLine("  script       <file.script | dir> [out] [--text=<dir>]");
			Console.Error.WriteLine("                                    event bytecode -> .ffs source");
			Console.Error.WriteLine("  script-build <file.ffs | dir> [out]");
			Console.Error.WriteLine("                                    .ffs source -> event bytecode");
			Console.Error.WriteLine("  ops [filter]                      list script instructions");
			Console.Error.WriteLine("  editor [--content=<dir>] [--override=<dir>] [--port=<n>] [--text=<dir>]");
			Console.Error.WriteLine("                                    open the content editor in a browser");
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
					ScriptFile script = ScriptFile.Read(original);
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
							Ffs.Parser.Parse(File.ReadAllText(destination)))
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
		/// Serves the editor. Everything it can do is something the command line can
		/// already do - the point is that a person can see what they are changing.
		/// </summary>
		private static int Editor(string[] args)
		{
			string content = "Content";
			string overrides = null;
			string textDir = null;
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
				else if (arg.StartsWith("--text=", StringComparison.OrdinalIgnoreCase))
				{
					textDir = arg.Substring("--text=".Length).Trim('"');
				}
				else if (arg.StartsWith("--port=", StringComparison.OrdinalIgnoreCase))
				{
					port = int.Parse(arg.Substring("--port=".Length),
						CultureInfo.InvariantCulture);
				}
			}

			string webRoot = Path.Combine(
				Path.GetDirectoryName(typeof(Program).Assembly.Location), "wwwroot");
			if (!Directory.Exists(webRoot))
			{
				Console.Error.WriteLine("the editor's files are missing from " + webRoot);
				return 1;
			}

			Editor.Workspace workspace;
			try
			{
				workspace = new Editor.Workspace(content, overrides);
			}
			catch (FileNotFoundException ex)
			{
				Console.Error.WriteLine(ex.Message);
				return 1;
			}

			new Editor.EditorServer(workspace, Path.GetFullPath(webRoot),
				LoadMessages(textDir)).Run(port);
			return 0;
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
		private static int Ops(string filter)
		{
			int shown = 0;
			foreach (KeyValuePair<string, int> entry in Ffs.Mnemonics.All
				.OrderBy(e => e.Key, StringComparer.OrdinalIgnoreCase))
			{
				if (filter != null
					&& entry.Key.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
				{
					continue;
				}

				ScriptOp op = ScriptOps.Get(entry.Value);
				string arguments = op.Operands == null || op.Operands.Length == 0
					? string.Empty
					: string.Join(", ", op.Operands
						.Select((operand, i) => Describe(entry.Value, i, operand)));
				Console.WriteLine("{0,4}  {1,-42} {2}", entry.Value, entry.Key, arguments);
				shown++;
			}
			Console.WriteLine();
			Console.WriteLine("{0} instruction(s)", shown);
			return 0;
		}

		/// <summary>An operand as "name:type", with the type alone where no name is known.</summary>
		private static string Describe(int opcode, int index, Operand operand)
		{
			string type;
			switch (operand)
			{
				case Operand.Byte: type = "byte"; break;
				case Operand.Word: type = "word"; break;
				case Operand.Dword: type = "dword"; break;
				default: type = "string"; break;
			}
			if (ScriptOperands.IsFixed(opcode, index))
			{
				type += " fixed";
			}
			string name = ScriptOperands.Name(opcode, index);
			return name == null ? type : name + ":" + type;
		}

		/// <summary>Compiles script language source back to bytecode.</summary>
		private static int ScriptBuild(string input, string output)
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
					byte[] data = Ffs.Compiler.Compile(document);

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
