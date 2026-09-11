// A mod's PNG in place of one of the game's textures, at any size.
//
// The game's textures live in .ntxp packages in the DS's formats - a palette of 4, 16 or 256
// colours or 4x4 blocks, sides of 8 to 1024 texels - and a character's is 64 x 64. Replacing
// one inside the package keeps that (Crystal's Replace with a PNG writes into the slot it
// has), which is what the Steam game can read. The OpenFF client draws through OpenGL, and
// its texture coordinates are normalised by the size the *material* declares (SendTextureParam:
// 1 / (8 << sizeS)), not by the picture's, so a picture of any size drawn in a texture's place
// maps the same. This is that: <mod>/textures/<name>.png (n021.png for the texture called n021
// in any package; <package>.<name>.png to name the package too) is decoded and uploaded when
// the game would decode the package's texels for that name, at the PNG's own size, with
// linear filtering (the game's own textures are drawn nearest, as the DS did). Crystal writes
// such a PNG beside the downsized package copy when a project targets OpenFF and the picture
// given is larger than the slot, so one replace serves both targets.

using System;
using System.Collections.Generic;
using System.IO;

namespace OpenFF.Client
{
	internal static class TextureOverrides
	{
		private static readonly Dictionary<string, string> _files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		private static readonly Dictionary<string, (int W, int H, byte[] Rgba)> _decoded = new Dictionary<string, (int, int, byte[])>(StringComparer.OrdinalIgnoreCase);

		/// <summary>The mods' textures folders scanned (the first root that has a name wins, as with files).</summary>
		public static void Register(IEnumerable<string> roots)
		{
			_files.Clear();
			_decoded.Clear();
			int count = 0;
			foreach (string root in roots ?? Array.Empty<string>())
			{
				string folder = Path.Combine(root, "textures");
				if (!Directory.Exists(folder)) continue;
				foreach (string file in Directory.EnumerateFiles(folder, "*.png", SearchOption.TopDirectoryOnly))
				{
					string key = Path.GetFileNameWithoutExtension(file);
					if (_files.ContainsKey(key)) continue;
					_files[key] = file;
					count++;
				}
			}
			if (count > 0) Log.Write(LogChannel.General, "textures: " + count + " PNG(s) of the mods' own in place of the game's (textures/<name>.png)");
		}

		public static bool Any => _files.Count > 0;

		/// <summary>
		/// The picture for a texture name (and the package it is in, when known), decoded to RGBA
		/// at its own size; false when no mod gives one. "<package>.<name>.png" is looked for first,
		/// then "<name>.png".
		/// </summary>
		public static bool TryGet(string texture, string package, out int width, out int height, out byte[] rgba)
		{
			width = height = 0; rgba = null;
			if (_files.Count == 0 || string.IsNullOrEmpty(texture)) return false;
			string key = null;
			if (!string.IsNullOrEmpty(package) && _files.ContainsKey(package + "." + texture)) key = package + "." + texture;
			else if (_files.ContainsKey(texture)) key = texture;
			if (key == null) return false;
			if (!_decoded.TryGetValue(key, out (int W, int H, byte[] Rgba) got))
			{
				got = Decode(_files[key]);
				_decoded[key] = got;
				if (got.Rgba != null) Log.Write(LogChannel.File, "textures: " + texture + " drawn from " + Path.GetFileName(_files[key]) + " (" + got.W + "x" + got.H + ")");
			}
			if (got.Rgba == null) return false;
			width = got.W; height = got.H; rgba = got.Rgba;
			return true;
		}

		private static (int, int, byte[]) Decode(string path)
		{
			try
			{
				int[] image = ImageDecoder.Decode(File.ReadAllBytes(path));
				if (image == null || image.Length < 2) return (0, 0, null);
				int w = image[0], h = image[1];
				byte[] rgba = new byte[w * h * 4];
				for (int i = 0; i < w * h; i++)
				{
					uint packed = (uint)image[i + 2];        // XNA's packing: R in the low byte, then G, B, A
					rgba[i * 4] = (byte)packed;
					rgba[i * 4 + 1] = (byte)(packed >> 8);
					rgba[i * 4 + 2] = (byte)(packed >> 16);
					rgba[i * 4 + 3] = (byte)(packed >> 24);
				}
				return (w, h, rgba);
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "textures: " + Path.GetFileName(path) + " does not read: " + ex.Message);
				return (0, 0, null);
			}
		}
	}
}
