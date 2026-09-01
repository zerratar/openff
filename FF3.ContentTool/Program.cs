// FF3 content tool.
//
//   dotnet run --project FF3.ContentTool -- info    <file-or-dir>
//   dotnet run --project FF3.ContentTool -- extract <xnb-dir> <out-dir>
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
