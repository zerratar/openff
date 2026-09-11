// The 3D textures, for the editor.
//
// These are not files on their own. A texture lives inside a TEX0 block, inside an
// NMDP package, inside an LZ-compressed archive entry - so nothing here can be served
// straight through the way the 2D art is. Every picture is decoded on the way out.
//
// Which is why the listing is in two steps. Naming the 1589 packages costs nothing;
// opening them all to count their textures means decompressing 1589 files, so that
// happens for one package at a time, when you click it.
//
// Textures are read-only for now. Putting one back means writing a TEX0 - re-quantising
// to a 256 colour palette, or to 4x4 blocks - and that is a bigger job than reading,
// so it is honest to say so rather than offer a button that half works.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Crystal.Editor
{
	internal sealed class TexturePackage
	{
		public string Name { get; set; }

		/// <summary>"model" for .nmdp, "textures" for .ntxp.</summary>
		public string Kind { get; set; }
	}

	internal sealed class TextureInfo
	{
		public int Index { get; set; }
		public string Name { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public string Format { get; set; }
		public string Palette { get; set; }
		public string Problem { get; set; }

		/// <summary>The project's full-size picture for this texture (textures/&lt;name&gt;.png), which the OpenFF client draws in its place, or null.</summary>
		public string FullSize { get; set; }
		public int FullWidth { get; set; }
		public int FullHeight { get; set; }
	}

	internal static class Textures
	{
		public static readonly string[] Extensions = { ".lz" };

		/// <summary>What each format is, in words, for the panel beside the picture.</summary>
		public static readonly Dictionary<string, string> FormatNotes =
			new Dictionary<string, string>(StringComparer.Ordinal)
			{
				["pal256"] = "one byte per pixel into a 256 colour palette",
				["pal16"] = "half a byte per pixel into a 16 colour palette",
				["pal4"] = "a quarter byte per pixel into a 4 colour palette",
				["a3i5"] = "5 bits of palette index, 3 of alpha - soft edges, few colours",
				["a5i3"] = "3 bits of palette index, 5 of alpha - what shadows use",
				["rgb555"] = "the colour itself, 15 bits, plus one bit of alpha",
				["4x4"] = "blocks of 4x4 sharing two or four colours, a quarter the size"
			};

		/// <summary>
		/// Every package that could hold textures, named but not opened. A package with
		/// no TEX0 is only found out by decompressing it, so all of them are listed and
		/// the ones with nothing in them say so when opened.
		/// </summary>
		public static List<TexturePackage> List(Workspace workspace)
		{
			return workspace.List(Extensions)
				.Where(e => Stem(e.Name).EndsWith(".nmdp", StringComparison.OrdinalIgnoreCase)
					|| Stem(e.Name).EndsWith(".ntxp", StringComparison.OrdinalIgnoreCase))
				.Select(e => new TexturePackage
				{
					Name = e.Name,
					Kind = Stem(e.Name).EndsWith(".nmdp", StringComparison.OrdinalIgnoreCase)
						? "model" : "textures"
				})
				.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
				.ToList();
		}

		/// <summary>What is inside one package; with a project, which textures have a full-size picture of the project's (textures/&lt;name&gt;.png) beside the package copy.</summary>
		public static List<TextureInfo> Contents(Workspace workspace, string name, Project project = null)
		{
			byte[] data = Lz.Decompress(workspace.Read(name));
			if (Tex0.Find(data) < 0)
			{
				return new List<TextureInfo>();
			}

			Tex0File package = Tex0.Read(data);
			List<TextureInfo> list = new List<TextureInfo>();
			string stem = Path.GetFileName(name);
			stem = stem.Substring(0, stem.IndexOf('.') < 0 ? stem.Length : stem.IndexOf('.'));   // "files/n011.ntxp.lz" -> "n011"
			for (int i = 0; i < package.Textures.Count; i++)
			{
				Tex0Texture texture = package.Textures[i];
				TextureInfo info = new TextureInfo
				{
					Index = i,
					Name = texture.Name,
					Width = texture.Width,
					Height = texture.Height,
					Format = texture.FormatName,
					Palette = texture.Palette,
					Problem = texture.Problem
				};
				string full = FullSizePath(project, stem, texture.Name);
				if (full != null)
				{
					info.FullSize = "textures/" + Path.GetFileName(full);
					(info.FullWidth, info.FullHeight) = PngSize(full);
				}
				list.Add(info);
			}
			return list;
		}

		/// <summary>The project's full-size PNG for a texture - "&lt;package&gt;.&lt;name&gt;.png" first, then "&lt;name&gt;.png" - or null.</summary>
		public static string FullSizePath(Project project, string packageStem, string textureName)
		{
			if (project == null || string.IsNullOrEmpty(textureName) || textureName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) return null;
			string folder = Path.Combine(project.Directory, "textures");
			if (!Directory.Exists(folder)) return null;
			foreach (string candidate in new[] { packageStem + "." + textureName + ".png", textureName + ".png" })
			{
				string path = Path.Combine(folder, candidate);
				if (File.Exists(path)) return path;
			}
			return null;
		}

		/// <summary>A PNG's size from its IHDR, without decoding it.</summary>
		private static (int, int) PngSize(string path)
		{
			try
			{
				using FileStream f = File.OpenRead(path);
				byte[] head = new byte[24];
				if (f.Read(head, 0, 24) < 24 || head[12] != (byte)'I' || head[13] != (byte)'H') return (0, 0);
				int w = (head[16] << 24) | (head[17] << 16) | (head[18] << 8) | head[19];
				int h = (head[20] << 24) | (head[21] << 16) | (head[22] << 8) | head[23];
				return (w, h);
			}
			catch (Exception) { return (0, 0); }
		}

		/// <summary>One texture as a PNG.</summary>
		/// <summary>
		/// One texture replaced by a picture of the same size (RGBA bytes), the package written
		/// into the project as an override - compressed again as it came. Returns the package
		/// size written; throws with the reason (size, a 4x4 texture) when it cannot be done.
		/// </summary>
		public static int Replace(Workspace workspace, string name, int index, byte[] rgba, int width, int height)
		{
			byte[] raw = workspace.Read(name);
			bool compressed = name.EndsWith(".lz", StringComparison.OrdinalIgnoreCase);
			byte[] package = compressed ? Lz.Decompress(raw) : raw;
			byte[] written = Tex0Write.Replace(package, index, rgba, width, height);
			byte[] output = compressed ? Lz.Compress(written) : written;
			workspace.Write(name, output);
			return output.Length;
		}

		/// <summary>A new texture package (.ntxp.lz) of the project's own from pictures; the size written.</summary>
		public static int Create(Workspace workspace, string name, IReadOnlyList<Tex0Write.NewTexture> textures)
		{
			byte[] package = Tex0Write.Build(textures);
			// Read back through the reader before anything is written: a package the editor
			// cannot read is one the game cannot either.
			Tex0File check = Tex0.Read(package);
			if (check.Textures.Count != textures.Count) throw new System.IO.InvalidDataException("the package read back with " + check.Textures.Count + " textures");
			byte[] output = name.EndsWith(".lz", StringComparison.OrdinalIgnoreCase) ? Lz.Compress(package) : package;
			workspace.Write(name, output);
			return output.Length;
		}

		public static byte[] Png(Workspace workspace, string name, int index)
		{
			Tex0File package = Tex0.Read(Lz.Decompress(workspace.Read(name)));
			if (index < 0 || index >= package.Textures.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(index), "no texture " + index + " in " + name);
			}

			Tex0Texture texture = package.Textures[index];
			return Crystal.Png.Encode(texture.Width, texture.Height,
				Tex0.Decode(package, texture));
		}

		/// <summary>"foo.nmdp.lz" -> "foo.nmdp".</summary>
		private static string Stem(string name)
		{
			return name.Substring(0, name.Length - 3);
		}
	}
}
