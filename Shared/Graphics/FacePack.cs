// A face pack - the game's .face file (j101_001.face): a character's expressions, one set of eye and
// mouth pictures a number, put on a model by a scene (SetUpCharacter_FaceData) and shown one number
// at a time (ChangeFaceEye / ChangeFaceMouth: a blink, a start, a smile).
//
// Layout, from the game's ChainTextureManager:
//   +0   "NRTP", version, texel name count, palette name count, flags, 3 reserved
//   +32  texel names (16 bytes each: eye, mouth), then palette names (eye_pl, mouth_pl)
//   then a chain pack: a count, 12 bytes, and per set an offset and a size (from the pack's start)
//   to an NMDP whose BTX0 holds that number's pictures under the texel names.

using System;
using System.Collections.Generic;
using System.Text;

namespace OpenFF.Graphics
{
	internal sealed class FacePack
	{
		/// <summary>The parts a scene swaps (eye, mouth): the texel names.</summary>
		public List<string> Parts { get; } = new List<string>();

		/// <summary>Per number: the NMDP holding its pictures.</summary>
		public List<byte[]> Sets { get; } = new List<byte[]>();

		public static FacePack Read(byte[] data)
		{
			if (data == null || data.Length < 32 || data[0] != 'N' || data[1] != 'R' || data[2] != 'T' || data[3] != 'P') return null;
			int texels = (int)BitConverter.ToUInt32(data, 8);
			int palettes = (int)BitConverter.ToUInt32(data, 12);
			if (texels < 0 || palettes < 0 || texels > 16 || palettes > 16) return null;
			FacePack pack = new FacePack();
			int at = 32;
			for (int i = 0; i < texels; i++, at += 16) pack.Parts.Add(Name(data, at));
			at += palettes * 16;
			if (at + 16 > data.Length) return null;
			int count = (int)BitConverter.ToUInt32(data, at);
			for (int i = 0; i < count && at + 16 + i * 8 + 8 <= data.Length; i++)
			{
				int offset = (int)BitConverter.ToUInt32(data, at + 16 + i * 8);
				int size = (int)BitConverter.ToUInt32(data, at + 16 + i * 8 + 4);
				if (offset < 0 || size <= 0 || at + offset + size > data.Length) { pack.Sets.Add(null); continue; }
				byte[] set = new byte[size];
				Buffer.BlockCopy(data, at + offset, set, 0, size);
				pack.Sets.Add(set);
			}
			return pack;
		}

		/// <summary>A number's picture of a part as RGBA, or null when the set has none.</summary>
		public (int Width, int Height, byte[] Rgba)? Picture(int number, string part)
		{
			if (number < 0 || number >= Sets.Count || Sets[number] == null) return null;
			try
			{
				Tex0File file = Tex0.Read(Sets[number]);
				foreach (Tex0Texture texture in file.Textures)
					if (string.Equals(texture.Name, part, StringComparison.OrdinalIgnoreCase) && texture.Problem == null)
						return (texture.Width, texture.Height, Tex0.Decode(file, texture));
			}
			catch (Exception) { }
			return null;
		}

		/// <summary>How far apart two pictures are: the mean colour difference over the pixels either shows (0-255, a pixel
		/// shown in one and not the other counting 255); int.MaxValue for pictures of different sizes.</summary>
		public static int Difference(int width, int height, byte[] a, int otherWidth, int otherHeight, byte[] b)
		{
			if (a == null || b == null || width != otherWidth || height != otherHeight || a.Length != b.Length) return int.MaxValue;
			long sum = 0;
			int count = 0;
			for (int i = 0; i + 3 < a.Length; i += 4)
			{
				bool shownA = a[i + 3] >= 128, shownB = b[i + 3] >= 128;
				if (!shownA && !shownB) continue;
				count++;
				sum += shownA != shownB ? 255 : (Math.Abs(a[i] - b[i]) + Math.Abs(a[i + 1] - b[i + 1]) + Math.Abs(a[i + 2] - b[i + 2])) / 3;
			}
			return count == 0 ? 0 : (int)(sum / count);
		}

		/// <summary>A material's picture this close (Difference) to one of the game's pictures of a part is the game's - a
		/// model made from the game's sheets, which may then wear the game's expressions. Copies come out at 0; a
		/// re-saved or lightly touched-up one a few; a face drawn anew 40 and more.</summary>
		public const int SameAsGame = 16;

		/// <summary>The smallest Difference between a picture and this pack's pictures of a part.</summary>
		public int Closest(int width, int height, byte[] rgba, string part)
		{
			int best = int.MaxValue;
			for (int n = 0; n < Sets.Count && best > 0; n++)
				if (Picture(n, part) is (int w, int h, byte[] game)) best = Math.Min(best, Difference(width, height, rgba, w, h, game));
			return best;
		}

		/// <summary>The names the packs of a pack's model can have (j101_001 -> j101, j101_001 ... j101_020).</summary>
		public static IEnumerable<string> SiblingNames(string pack)
		{
			string model = pack;
			int cut = pack.LastIndexOf('_');
			if (cut > 0 && int.TryParse(pack.Substring(cut + 1), out _)) model = pack.Substring(0, cut);
			yield return model;
			for (int i = 1; i <= 20; i++) yield return model + "_" + i.ToString("000");
		}

		private static string Name(byte[] data, int at)
		{
			int end = at;
			while (end < at + 16 && end < data.Length && data[end] != 0) end++;
			return Encoding.ASCII.GetString(data, at, end - at);
		}
	}
}
