// Writing a texture back into a TEX0 - the first asset import. In place: the picture keeps
// its size and its format, so the block's layout, every offset and every other texture in
// the package stay exactly as they were; only this texture's texels and its palette entries
// change. That is what a retexture is - a Red Cap out of a Goblin, a sign with new words -
// and it needs no relayout of a format with four kinds of offsets in it.
//
// The pixels come in as RGBA (the browser decodes the PNG on a canvas and sends the bytes),
// already at the texture's size. Palette formats are quantised: the picture's colours
// reduced to 15 bits as the DS has them, then median-cut to the palette the texture has
// room for (its entries in the palette block, up to the format's count), then every pixel
// to its nearest. Transparent pixels take entry 0 where the texture marks entry 0 as
// transparent (pal4/16/256); the alpha formats (a3i5, a5i3) carry alpha per pixel; rgb555
// carries one bit of it. The 4x4 compressed format - the battle monsters' - gets its own
// encoder below, block by block.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Crystal
{
	internal static class Tex0Write
	{
		/// <summary>The package's bytes (decompressed NMDP) with one texture's pixels replaced; throws with the reason when it cannot be.</summary>
		public static byte[] Replace(byte[] package, int index, byte[] rgba, int width, int height)
		{
			Tex0File file = Tex0.Read(package);
			if (index < 0 || index >= file.Textures.Count) throw new InvalidDataException("no texture " + index + " in this package");
			Tex0Texture texture = file.Textures[index];
			if (texture.Width != width || texture.Height != height)
				throw new InvalidDataException("the picture is " + width + "x" + height + "; the texture is " + texture.Width + "x" + texture.Height + " and keeps its size");
			if (rgba == null || rgba.Length < width * height * 4) throw new InvalidDataException("not enough pixels");
			if (texture.Format == 0) throw new InvalidDataException("this entry holds no pixels");

			byte[] data = (byte[])package.Clone();
			int texel = file.TexData + ((int)(texture.Param & 0xFFFFF) << 3);
			int count = width * height;

			if (texture.Format == 5)
			{
				Encode4x4(file, texture, data, rgba);
				return data;
			}

			if (texture.Format == 7)
			{
				// Straight colour: 15 bits and the alpha bit.
				for (int i = 0; i < count; i++)
				{
					int a = rgba[i * 4 + 3] >= 128 ? 0x8000 : 0;
					int c = To555(rgba, i * 4) | a;
					data[texel + i * 2] = (byte)c;
					data[texel + i * 2 + 1] = (byte)(c >> 8);
				}
				return data;
			}

			// How many palette entries this texture may use: the format's count, no more than
			// the room its palette has in the block before the next one starts.
			int formatColours = texture.Format switch { 1 => 32, 2 => 4, 3 => 16, 4 => 256, 6 => 8, _ => 0 };
			int room = PaletteRoom(file, texture);
			int colours = Math.Min(formatColours, room);
			if (colours <= 0) throw new InvalidDataException("the texture has no palette to write");
			bool alphaFormat = texture.Format == 1 || texture.Format == 6;
			bool transparent0 = texture.Transparent0 && !alphaFormat;

			// The colours to quantise: every pixel for the alpha formats (their alpha rides
			// beside the index), the opaque ones where entry 0 stands for see-through.
			List<int> samples = new List<int>(count);
			for (int i = 0; i < count; i++)
			{
				if (transparent0 && rgba[i * 4 + 3] < 128) continue;
				if (alphaFormat && rgba[i * 4 + 3] == 0) continue;
				samples.Add(To555(rgba, i * 4));
			}
			int usable = transparent0 ? colours - 1 : colours;
			List<int> palette = Quantise(samples, Math.Max(1, usable));
			if (transparent0) palette.Insert(0, 0);
			while (palette.Count < colours) palette.Add(0);

			// The palette entries, in place.
			int paletteAt = file.PaletteData + texture.PaletteOffset;
			for (int i = 0; i < colours; i++)
			{
				data[paletteAt + i * 2] = (byte)palette[i];
				data[paletteAt + i * 2 + 1] = (byte)(palette[i] >> 8);
			}

			// The texels.
			int[] nearestCache = new int[32768];
			for (int i = 0; i < nearestCache.Length; i++) nearestCache[i] = -1;
			int Nearest(int c555)
			{
				if (nearestCache[c555] >= 0) return nearestCache[c555];
				int best = transparent0 ? 1 : 0, bestD = int.MaxValue;
				for (int p = transparent0 ? 1 : 0; p < palette.Count && p < colours; p++)
				{
					int d = Distance(c555, palette[p]);
					if (d < bestD) { bestD = d; best = p; }
				}
				nearestCache[c555] = best;
				return best;
			}
			switch (texture.Format)
			{
				case 1: // a3i5
					for (int i = 0; i < count; i++)
					{
						int alpha = (rgba[i * 4 + 3] * 7 + 127) / 255;
						int idx = alpha == 0 ? 0 : Nearest(To555(rgba, i * 4));
						data[texel + i] = (byte)((alpha << 5) | (idx & 0x1F));
					}
					break;
				case 6: // a5i3
					for (int i = 0; i < count; i++)
					{
						int alpha = (rgba[i * 4 + 3] * 31 + 127) / 255;
						int idx = alpha == 0 ? 0 : Nearest(To555(rgba, i * 4));
						data[texel + i] = (byte)((alpha << 3) | (idx & 7));
					}
					break;
				case 2: // pal4: four pixels a byte, low bits first
					Array.Clear(data, texel, (count + 3) / 4);
					for (int i = 0; i < count; i++)
					{
						int idx = transparent0 && rgba[i * 4 + 3] < 128 ? 0 : Nearest(To555(rgba, i * 4));
						data[texel + (i >> 2)] |= (byte)((idx & 3) << ((i & 3) * 2));
					}
					break;
				case 3: // pal16: two pixels a byte
					Array.Clear(data, texel, (count + 1) / 2);
					for (int i = 0; i < count; i++)
					{
						int idx = transparent0 && rgba[i * 4 + 3] < 128 ? 0 : Nearest(To555(rgba, i * 4));
						data[texel + (i >> 1)] |= (byte)((idx & 0xF) << ((i & 1) * 4));
					}
					break;
				case 4: // pal256
					for (int i = 0; i < count; i++)
					{
						data[texel + i] = (byte)(transparent0 && rgba[i * 4 + 3] < 128 ? 0 : Nearest(To555(rgba, i * 4)));
					}
					break;
			}
			return data;
		}

		// ------------------------------------------------------------------ 4x4 blocks
		//
		// The compressed format (what the battle monsters wear): each 4x4 block is a 32-bit
		// word of sixteen 2-bit texels and a 16-bit control word - bits 0-11 where its colours
		// sit in the palette (in pairs), bit 14 interpolated or explicit, bit 15 the variant:
		//   explicit, bit15=0: c0 c1 c2 + transparent;  bit15=1: c0 c1 c2 c3
		//   interpolated, bit15=0: c0 c1 (c0+c1)/2 + transparent;  bit15=1: c0 c1 (5c0+3c1)/8 (3c0+5c1)/8
		// (Tex0.Decode4x4 is the reader this mirrors.) The encoder fits every block with two
		// endpoints along its colour range - the interpolated variants, two palette entries -
		// and with an explicit palette of three or four; keeps the better where the palette has
		// room, the two-entry one where it has not; then packs the blocks' palettes, shared
		// where equal, and merges the closest pairs until they fit the room the texture had.

		private sealed class Block4x4
		{
			public int[] Colours = new int[16];   // 15-bit
			public bool[] Clear = new bool[16];   // alpha < 128
			public bool Transparent;
			// The chosen encoding:
			public int[] Palette;                 // 2 or 4 entries (15-bit); explicit or endpoints
			public bool Interpolated;
			public bool Variant;                  // bit 15
			public uint Texels;
		}

		private static void Encode4x4(Tex0File file, Tex0Texture texture, byte[] data, byte[] rgba)
		{
			int width = texture.Width, height = texture.Height;
			int bw = width >> 2, bh = height >> 2;
			int address = (int)(texture.Param & 0xFFFFF) << 3;
			int blocksAt = file.Tex4x4Data + address;
			int indicesAt = file.Tex4x4Index + (address >> 1);
			int paletteAt = file.PaletteData + texture.PaletteOffset;
			int room = PaletteRoom(file, texture) & ~1;   // pairs
			if (room < 2) throw new InvalidDataException("the texture has no palette room to write");

			// Read the blocks.
			List<Block4x4> blocks = new List<Block4x4>(bw * bh);
			for (int by = 0; by < bh; by++)
				for (int bx = 0; bx < bw; bx++)
				{
					Block4x4 b = new Block4x4();
					for (int y = 0; y < 4; y++)
						for (int x = 0; x < 4; x++)
						{
							int p = ((by * 4 + y) * width + bx * 4 + x) * 4;
							b.Colours[y * 4 + x] = To555(rgba, p);
							b.Clear[y * 4 + x] = rgba[p + 3] < 128;
							if (b.Clear[y * 4 + x]) b.Transparent = true;
						}
					blocks.Add(b);
				}

			// Fit each block both ways; remember both so the palette packing can fall back.
			List<(int[] pal, bool interp, bool variant, uint texels, long error)> two = new List<(int[], bool, bool, uint, long)>();
			List<(int[] pal, bool interp, bool variant, uint texels, long error)> four = new List<(int[], bool, bool, uint, long)>();
			foreach (Block4x4 b in blocks)
			{
				two.Add(FitInterpolated(b));
				four.Add(FitExplicit(b));
			}

			// Prefer the better fit per block; if the palettes do not fit the room, fall back
			// to the two-entry fit for the blocks that gain least from four, then merge.
			bool[] useFour = new bool[blocks.Count];
			for (int i = 0; i < blocks.Count; i++) useFour[i] = four[i].error < two[i].error;
			List<int[]> palettes;
			while (true)
			{
				palettes = new List<int[]>();
				for (int i = 0; i < blocks.Count; i++)
				{
					var pick = useFour[i] ? four[i] : two[i];
					Block4x4 b = blocks[i];
					b.Palette = pick.pal; b.Interpolated = pick.interp; b.Variant = pick.variant; b.Texels = pick.texels;
				}
				// Shared where equal.
				Dictionary<string, int> where = new Dictionary<string, int>();
				int used = 0;
				foreach (Block4x4 b in blocks)
				{
					string key = string.Join(",", b.Palette);
					if (where.ContainsKey(key)) continue;
					where[key] = used;
					used += b.Palette.Length;
				}
				if (used <= room) break;
				// Too many: the four-entry blocks that gain least go to two entries; none left, merge.
				int candidate = -1; long least = long.MaxValue;
				for (int i = 0; i < blocks.Count; i++)
					if (useFour[i] && two[i].error - four[i].error < least) { least = two[i].error - four[i].error; candidate = i; }
				if (candidate >= 0) { useFour[candidate] = false; continue; }
				MergeEndpoints(blocks, two, room);
				for (int i = 0; i < blocks.Count; i++) { var pick = two[i]; blocks[i].Palette = pick.pal; blocks[i].Interpolated = pick.interp; blocks[i].Variant = pick.variant; blocks[i].Texels = pick.texels; }
				break;
			}

			// Lay the palettes out and write everything.
			Dictionary<string, int> offsets = new Dictionary<string, int>();
			int at = 0;
			List<int> paletteWords = new List<int>();
			foreach (Block4x4 b in blocks)
			{
				string key = string.Join(",", b.Palette);
				if (offsets.ContainsKey(key)) continue;
				offsets[key] = at;
				paletteWords.AddRange(b.Palette);
				at += b.Palette.Length;
			}
			if (at > room) throw new InvalidDataException("the picture needs " + at + " palette entries; the texture has room for " + room);
			for (int i = 0; i < room; i++)
			{
				int c = i < paletteWords.Count ? paletteWords[i] : 0;
				data[paletteAt + i * 2] = (byte)c;
				data[paletteAt + i * 2 + 1] = (byte)(c >> 8);
			}
			for (int n = 0; n < blocks.Count; n++)
			{
				Block4x4 b = blocks[n];
				int pairs = offsets[string.Join(",", b.Palette)] >> 1;
				int control = (pairs & 0xFFF) | (b.Interpolated ? 0x4000 : 0) | (b.Variant ? 0x8000 : 0);
				data[indicesAt + n * 2] = (byte)control;
				data[indicesAt + n * 2 + 1] = (byte)(control >> 8);
				data[blocksAt + n * 4] = (byte)b.Texels;
				data[blocksAt + n * 4 + 1] = (byte)(b.Texels >> 8);
				data[blocksAt + n * 4 + 2] = (byte)(b.Texels >> 16);
				data[blocksAt + n * 4 + 3] = (byte)(b.Texels >> 24);
			}
		}

		/// <summary>Two endpoints along the block's colour range; the mid or the 5/8-3/8 mixes between; a transparent texel where the block has one.</summary>
		private static (int[] pal, bool interp, bool variant, uint texels, long error) FitInterpolated(Block4x4 b)
		{
			List<int> opaque = new List<int>();
			for (int i = 0; i < 16; i++) if (!b.Clear[i]) opaque.Add(b.Colours[i]);
			int c0 = 0, c1 = 0;
			if (opaque.Count > 0)
			{
				// The two most distant colours of the block, then each pulled to the mean of its half.
				int bestD = -1;
				foreach (int a in opaque) foreach (int c in opaque) { int d = Distance(a, c); if (d > bestD) { bestD = d; c0 = a; c1 = c; } }
				c0 = Mean(opaque.Where(c => Distance(c, c0) <= Distance(c, c1)));
				c1 = Mean(opaque.Where(c => Distance(c, c1) < Distance(c, c0)).DefaultIfEmpty(c1));
			}
			int[] pal = { c0, c1 };
			bool variant = !b.Transparent;
			int[] ramp = variant ? new[] { c0, c1, Mix(c0, c1, 5, 3), Mix(c0, c1, 3, 5) } : new[] { c0, c1, Mix(c0, c1, 4, 4) };
			uint texels = 0; long error = 0;
			for (int i = 0; i < 16; i++)
			{
				int pick;
				if (b.Clear[i]) pick = 3;
				else
				{
					pick = 0; int bestD = int.MaxValue;
					for (int r = 0; r < ramp.Length; r++) { int d = Distance(b.Colours[i], ramp[r]); if (d < bestD) { bestD = d; pick = r; } }
					error += bestD;
				}
				texels |= (uint)pick << (i * 2);
			}
			return (pal, true, variant, texels, error);
		}

		/// <summary>Three colours and a transparent texel, or four colours, quantised from the block.</summary>
		private static (int[] pal, bool interp, bool variant, uint texels, long error) FitExplicit(Block4x4 b)
		{
			List<int> opaque = new List<int>();
			for (int i = 0; i < 16; i++) if (!b.Clear[i]) opaque.Add(b.Colours[i]);
			int slots = b.Transparent ? 3 : 4;
			List<int> pal = Quantise(opaque, slots);
			while (pal.Count < 4) pal.Add(0);
			uint texels = 0; long error = 0;
			for (int i = 0; i < 16; i++)
			{
				int pick;
				if (b.Clear[i]) pick = 3;
				else
				{
					pick = 0; int bestD = int.MaxValue;
					for (int r = 0; r < slots; r++) { int d = Distance(b.Colours[i], pal[r]); if (d < bestD) { bestD = d; pick = r; } }
					error += bestD;
				}
				texels |= (uint)pick << (i * 2);
			}
			return (pal.ToArray(), false, !b.Transparent, texels, error);
		}

		/// <summary>Too many distinct endpoint pairs for the room: the closest pairs are merged until they fit, and the blocks refitted to the merged pair.</summary>
		private static void MergeEndpoints(List<Block4x4> blocks, List<(int[] pal, bool interp, bool variant, uint texels, long error)> two, int room)
		{
			List<int[]> pairs = two.Select(t => t.pal).Distinct(new PairComparer()).ToList();
			int limit = room / 2;
			while (pairs.Count > limit)
			{
				int a = 0, bb = 1, best = int.MaxValue;
				for (int i = 0; i < pairs.Count; i++)
					for (int j = i + 1; j < pairs.Count; j++)
					{
						int d = Distance(pairs[i][0], pairs[j][0]) + Distance(pairs[i][1], pairs[j][1]);
						if (d < best) { best = d; a = i; bb = j; }
					}
				int[] merged = { Mean(new[] { pairs[a][0], pairs[bb][0] }), Mean(new[] { pairs[a][1], pairs[bb][1] }) };
				pairs.RemoveAt(bb); pairs.RemoveAt(a); pairs.Add(merged);
			}
			// Each block to the nearest surviving pair, texels chosen again.
			for (int n = 0; n < blocks.Count; n++)
			{
				Block4x4 b = blocks[n];
				int[] pair = pairs.OrderBy(p => Distance(p[0], two[n].pal[0]) + Distance(p[1], two[n].pal[1])).First();
				bool variant = !b.Transparent;
				int[] ramp = variant ? new[] { pair[0], pair[1], Mix(pair[0], pair[1], 5, 3), Mix(pair[0], pair[1], 3, 5) } : new[] { pair[0], pair[1], Mix(pair[0], pair[1], 4, 4) };
				uint texels = 0; long error = 0;
				for (int i = 0; i < 16; i++)
				{
					int pick;
					if (b.Clear[i]) pick = 3;
					else { pick = 0; int bestD = int.MaxValue; for (int r = 0; r < ramp.Length; r++) { int d = Distance(b.Colours[i], ramp[r]); if (d < bestD) { bestD = d; pick = r; } } error += bestD; }
					texels |= (uint)pick << (i * 2);
				}
				two[n] = (pair, true, variant, texels, error);
			}
		}

		private sealed class PairComparer : IEqualityComparer<int[]>
		{
			public bool Equals(int[] x, int[] y) => x.Length == y.Length && x.SequenceEqual(y);
			public int GetHashCode(int[] p) => p.Aggregate(17, (h, v) => h * 31 + v);
		}

		private static int Mix(int a, int b, int wa, int wb)
		{
			int r = ((a & 0x1F) * wa + (b & 0x1F) * wb) / 8, g = (((a >> 5) & 0x1F) * wa + ((b >> 5) & 0x1F) * wb) / 8, bl = (((a >> 10) & 0x1F) * wa + ((b >> 10) & 0x1F) * wb) / 8;
			return r | (g << 5) | (bl << 10);
		}

		private static int Mean(IEnumerable<int> colours)
		{
			long r = 0, g = 0, b = 0, n = 0;
			foreach (int c in colours) { r += c & 0x1F; g += (c >> 5) & 0x1F; b += (c >> 10) & 0x1F; n++; }
			if (n == 0) return 0;
			return (int)(r / n) | ((int)(g / n) << 5) | ((int)(b / n) << 10);
		}

		/// <summary>How many 16-bit entries the texture's palette has before the next palette (or the block's end) begins.</summary>
		private static int PaletteRoom(Tex0File file, Tex0Texture texture)
		{
			if (texture.Palette == null) return 0;
			// NNSG3dResPlttInfo at +44: vramKey u32, sizePltt u16 (in 8-byte units), flag, ofsDict at +52, ofsPlttData at +56.
			int blockSize = (file.Data[file.At + 48] | (file.Data[file.At + 49] << 8)) << 3;
			int end = blockSize;
			foreach (Tex0Texture other in file.Textures)
			{
				if (other.Palette == null || other.PaletteOffset <= texture.PaletteOffset) continue;
				if (other.PaletteOffset < end) end = other.PaletteOffset;
			}
			// The palette data block may run past the last entry the info says; the file's end is the last word.
			int room = (Math.Min(end, file.Data.Length - file.PaletteData) - texture.PaletteOffset) / 2;
			return Math.Max(0, room);
		}

		private static int To555(byte[] rgba, int at) => (rgba[at] >> 3) | ((rgba[at + 1] >> 3) << 5) | ((rgba[at + 2] >> 3) << 10);

		private static int Distance(int a, int b)
		{
			int dr = (a & 0x1F) - (b & 0x1F), dg = ((a >> 5) & 0x1F) - ((b >> 5) & 0x1F), db = ((a >> 10) & 0x1F) - ((b >> 10) & 0x1F);
			// Green weighs most to the eye, as in every quick colour distance.
			return dr * dr * 2 + dg * dg * 3 + db * db;
		}

		/// <summary>Median cut over 15-bit colours: at most `count` representatives, each the mean of its box.</summary>
		private static List<int> Quantise(List<int> samples, int count)
		{
			Dictionary<int, int> histogram = new Dictionary<int, int>();
			foreach (int c in samples) histogram[c] = histogram.TryGetValue(c, out int n) ? n + 1 : 1;
			if (histogram.Count == 0) return new List<int> { 0 };
			if (histogram.Count <= count) return histogram.Keys.OrderBy(c => c).ToList();

			List<List<(int colour, int weight)>> boxes = new List<List<(int, int)>> { histogram.Select(p => (p.Key, p.Value)).ToList() };
			while (boxes.Count < count)
			{
				// The box with the widest spread splits, along its widest channel, at the weighted median.
				int pick = -1, pickSpread = -1, pickChannel = 0;
				for (int b = 0; b < boxes.Count; b++)
				{
					if (boxes[b].Count < 2) continue;
					for (int ch = 0; ch < 3; ch++)
					{
						int min = 31, max = 0;
						foreach ((int colour, int _) in boxes[b]) { int v = (colour >> (ch * 5)) & 0x1F; if (v < min) min = v; if (v > max) max = v; }
						int spread = (max - min) * (ch == 1 ? 3 : ch == 0 ? 2 : 1);
						if (spread > pickSpread) { pickSpread = spread; pick = b; pickChannel = ch; }
					}
				}
				if (pick < 0) break;
				List<(int colour, int weight)> box = boxes[pick];
				box.Sort((x, y) => ((x.colour >> (pickChannel * 5)) & 0x1F).CompareTo((y.colour >> (pickChannel * 5)) & 0x1F));
				long total = box.Sum(e => (long)e.weight), run = 0;
				int cut = 0;
				for (; cut < box.Count - 1; cut++) { run += box[cut].weight; if (run * 2 >= total) { cut++; break; } }
				if (cut <= 0 || cut >= box.Count) cut = box.Count / 2;
				boxes[pick] = box.Take(cut).ToList();
				boxes.Add(box.Skip(cut).ToList());
			}
			List<int> palette = new List<int>();
			foreach (List<(int colour, int weight)> box in boxes)
			{
				long r = 0, g = 0, b = 0, w = 0;
				foreach ((int colour, int weight) in box) { r += (colour & 0x1F) * (long)weight; g += ((colour >> 5) & 0x1F) * (long)weight; b += ((colour >> 10) & 0x1F) * (long)weight; w += weight; }
				if (w == 0) continue;
				palette.Add((int)(r / w) | ((int)(g / w) << 5) | ((int)(b / w) << 10));
			}
			return palette;
		}
	}
}
