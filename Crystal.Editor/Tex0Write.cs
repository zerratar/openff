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
			int room = texture.Format == 7 ? 0 : PaletteRoom(file, texture);
			Encoded e = Encode(texture.Format, texture.Transparent0, rgba, width, height, room);
			if (texture.Format == 5)
			{
				int address = (int)(texture.Param & 0xFFFFF) << 3;
				Array.Copy(e.Texels, 0, data, file.Tex4x4Data + address, e.Texels.Length);
				Array.Copy(e.Indices, 0, data, file.Tex4x4Index + (address >> 1), e.Indices.Length);
			}
			else
			{
				Array.Copy(e.Texels, 0, data, texel, e.Texels.Length);
			}
			if (e.Palette != null)
			{
				int paletteAt = file.PaletteData + texture.PaletteOffset;
				for (int i = 0; i < room; i++)
				{
					int c = i < e.Palette.Length ? e.Palette[i] : 0;
					data[paletteAt + i * 2] = (byte)c;
					data[paletteAt + i * 2 + 1] = (byte)(c >> 8);
				}
			}
			return data;
		}

		/// <summary>A texture's encoded parts: the texels (for 4x4 the block words), the 4x4 control words, the palette entries (null for rgb555).</summary>
		public sealed class Encoded
		{
			public byte[] Texels;
			public byte[] Indices;
			public int[] Palette;
		}

		/// <summary>Bytes per pixel times eight, per format - how large a texture's texel data is.</summary>
		public static int TexelBytes(int format, int width, int height)
		{
			int count = width * height;
			switch (format)
			{
				case 1: case 4: case 6: return count;
				case 2: return (count + 3) / 4;
				case 3: return (count + 1) / 2;
				case 5: return count / 4;
				case 7: return count * 2;
				default: return 0;
			}
		}

		/// <summary>How many palette entries a format may have at most.</summary>
		public static int FormatColours(int format) => format switch { 1 => 32, 2 => 4, 3 => 16, 4 => 256, 6 => 8, _ => 0 };

		/// <summary>The pixels encoded for a format, with at most `room` palette entries (4x4: pairs of them).</summary>
		public static Encoded Encode(int format, bool transparent0, byte[] rgba, int width, int height, int room)
		{
			int count = width * height;
			if (format == 5) return Encode4x4(rgba, width, height, room & ~1);
			Encoded e = new Encoded { Texels = new byte[TexelBytes(format, width, height)] };
			if (format == 7)
			{
				for (int i = 0; i < count; i++)
				{
					int c = To555(rgba, i * 4) | (rgba[i * 4 + 3] >= 128 ? 0x8000 : 0);
					e.Texels[i * 2] = (byte)c;
					e.Texels[i * 2 + 1] = (byte)(c >> 8);
				}
				return e;
			}
			int colours = Math.Min(FormatColours(format), room);
			if (colours <= 0) throw new InvalidDataException("the texture has no palette to write");
			bool alphaFormat = format == 1 || format == 6;
			bool clear0 = transparent0 && !alphaFormat;
			List<int> samples = new List<int>(count);
			for (int i = 0; i < count; i++)
			{
				if (clear0 && rgba[i * 4 + 3] < 128) continue;
				if (alphaFormat && rgba[i * 4 + 3] == 0) continue;
				samples.Add(To555(rgba, i * 4));
			}
			List<int> palette = Quantise(samples, Math.Max(1, clear0 ? colours - 1 : colours));
			if (clear0) palette.Insert(0, 0);
			while (palette.Count < colours) palette.Add(0);
			e.Palette = palette.Take(colours).ToArray();
			int[] cache = new int[32768];
			for (int i = 0; i < cache.Length; i++) cache[i] = -1;
			int Nearest(int c555)
			{
				if (cache[c555] >= 0) return cache[c555];
				int best = clear0 ? 1 : 0, bestD = int.MaxValue;
				for (int p = clear0 ? 1 : 0; p < colours; p++) { int d = Distance(c555, e.Palette[p]); if (d < bestD) { bestD = d; best = p; } }
				cache[c555] = best;
				return best;
			}
			switch (format)
			{
				case 1:
					for (int i = 0; i < count; i++) { int a = (rgba[i * 4 + 3] * 7 + 127) / 255; e.Texels[i] = (byte)((a << 5) | (a == 0 ? 0 : Nearest(To555(rgba, i * 4)) & 0x1F)); }
					break;
				case 6:
					for (int i = 0; i < count; i++) { int a = (rgba[i * 4 + 3] * 31 + 127) / 255; e.Texels[i] = (byte)((a << 3) | (a == 0 ? 0 : Nearest(To555(rgba, i * 4)) & 7)); }
					break;
				case 2:
					for (int i = 0; i < count; i++) { int idx = clear0 && rgba[i * 4 + 3] < 128 ? 0 : Nearest(To555(rgba, i * 4)); e.Texels[i >> 2] |= (byte)((idx & 3) << ((i & 3) * 2)); }
					break;
				case 3:
					for (int i = 0; i < count; i++) { int idx = clear0 && rgba[i * 4 + 3] < 128 ? 0 : Nearest(To555(rgba, i * 4)); e.Texels[i >> 1] |= (byte)((idx & 0xF) << ((i & 1) * 4)); }
					break;
				case 4:
					for (int i = 0; i < count; i++) e.Texels[i] = (byte)(clear0 && rgba[i * 4 + 3] < 128 ? 0 : Nearest(To555(rgba, i * 4)));
					break;
				default: throw new InvalidDataException("format " + format + " holds no pixels");
			}
			return e;
		}
		// ------------------------------------------------------------------ a new package
		//
		// A texture package from nothing: NMDP wrapper, BTX0, one TEX0 with every texture
		// given - the layout n021.ntxp has, read off the file: a 48-byte NMDP head (the BTX0's
		// size at 24, its offset 48 at 28), a 16-byte BTX0 head with one block offset, then the
		// TEX0: three info blocks (texInfo at +8, tex4x4Info at +24, plttInfo at +44), the
		// texture dictionary at +0x3C, the palette dictionary after it, the texel data, the 4x4
		// block data, the 4x4 index data, the palette data. Dictionaries are NNSG3dResDict: an
		// 8-byte head, a patricia tree of numEntries+1 four-byte nodes, an entry head (unit
		// size, names offset), the entries, then 16-byte names. Textures and palettes of a 4x4
		// texture cross-reference by position; a palette is named <texture>_pl as the game's are.

		/// <summary>One texture to put in a new package.</summary>
		public sealed class NewTexture
		{
			public string Name;
			public int Format = 4;          // pal256
			public bool Transparent0;
			public byte[] Rgba;
			public int Width, Height;
		}

		/// <summary>A complete .ntxp (uncompressed NMDP) holding the textures given.</summary>
		public static byte[] Build(IReadOnlyList<NewTexture> textures)
		{
			if (textures == null || textures.Count == 0) throw new InvalidDataException("no textures to write");
			if (textures.Count > 255) throw new InvalidDataException("a package holds at most 255 textures");
			foreach (NewTexture t in textures)
			{
				if (!IsSize(t.Width) || !IsSize(t.Height)) throw new InvalidDataException(t.Name + ": " + t.Width + "x" + t.Height + " - a texture's sides are 8, 16, 32, 64, 128, 256, 512 or 1024");
				if (t.Format == 5 && (t.Width < 8 || t.Height < 8)) throw new InvalidDataException(t.Name + ": a 4x4 texture is at least 8x8");
				if (t.Rgba == null || t.Rgba.Length < t.Width * t.Height * 4) throw new InvalidDataException(t.Name + ": not enough pixels");
				if (t.Format < 1 || t.Format > 7) throw new InvalidDataException(t.Name + ": format " + t.Format + " holds no pixels");
			}

			// Encode every texture; the palette room a fresh texture gets is the format's full count
			// (4x4: four entries a block, so the fit is never forced down).
			List<Encoded> encoded = new List<Encoded>();
			foreach (NewTexture t in textures)
			{
				int room = t.Format == 5 ? (t.Width / 4) * (t.Height / 4) * 4 : FormatColours(t.Format);
				encoded.Add(Encode(t.Format, t.Transparent0, t.Rgba, t.Width, t.Height, room));
			}

			// Data layout: texels (non-4x4), then 4x4 blocks, then 4x4 indices, then palettes; each
			// texture's offset in 8-byte units, so every piece is padded to 8.
			int n = textures.Count;
			int[] texOfs = new int[n], plttOfs = new int[n];
			using (MemoryStream texels = new MemoryStream(), blocks = new MemoryStream(), indices = new MemoryStream(), palettes = new MemoryStream())
			{
				for (int i = 0; i < n; i++)
				{
					Encoded e = encoded[i];
					if (textures[i].Format == 5)
					{
						texOfs[i] = (int)blocks.Length;
						blocks.Write(e.Texels, 0, e.Texels.Length); Pad(blocks, 8);
						indices.Write(e.Indices, 0, e.Indices.Length); Pad(indices, 8);
					}
					else
					{
						texOfs[i] = (int)texels.Length;
						texels.Write(e.Texels, 0, e.Texels.Length); Pad(texels, 8);
					}
					if (e.Palette != null)
					{
						plttOfs[i] = (int)palettes.Length;
						foreach (int c in e.Palette) { palettes.WriteByte((byte)c); palettes.WriteByte((byte)(c >> 8)); }
						Pad(palettes, 8);
					}
					else plttOfs[i] = -1;
				}

				byte[] texDict = Dictionary(textures.Select(t => t.Name).ToList(), 8, i =>
				{
					NewTexture t = textures[i];
					uint param = (uint)(texOfs[i] >> 3) | ((uint)Log2(t.Width) << 20) | ((uint)Log2(t.Height) << 23) | ((uint)t.Format << 26) | (t.Transparent0 ? 1u << 29 : 0u);
					uint extra = (uint)t.Width | ((uint)t.Height << 11) | 0x80000000u;
					byte[] entry = new byte[8];
					Array.Copy(BitConverter.GetBytes(param), 0, entry, 0, 4);
					Array.Copy(BitConverter.GetBytes(extra), 0, entry, 4, 4);
					return entry;
				});
				List<int> withPalette = Enumerable.Range(0, n).Where(i => plttOfs[i] >= 0).ToList();
				byte[] plttDict = Dictionary(withPalette.Select(i => textures[i].Name + "_pl").ToList(), 4, k =>
				{
					int i = withPalette[k];
					byte[] entry = new byte[4];
					ushort ofs = (ushort)(plttOfs[i] >> 3);
					entry[0] = (byte)ofs; entry[1] = (byte)(ofs >> 8);
					entry[2] = (byte)(textures[i].Format == 2 ? 1 : 0);   // flag: a 4-colour palette
					return entry;
				});

				int headSize = 0x3C;
				int texDictAt = headSize;
				int plttDictAt = texDictAt + texDict.Length;
				int texDataAt = Align(plttDictAt + plttDict.Length, 8);
				int blocksAt = texDataAt + (int)texels.Length;
				int indicesAt = blocksAt + (int)blocks.Length;
				int plttDataAt = indicesAt + (int)indices.Length;
				int tex0Size = plttDataAt + (int)palettes.Length;

				byte[] tex0 = new byte[tex0Size];
				Ascii(tex0, 0, "TEX0");
				Put32(tex0, 4, (uint)tex0Size);
				// texInfo: vramKey, sizeTex>>3, ofsDict, flag, pad, ofsTex
				Put16(tex0, 8 + 4, (ushort)(texels.Length >> 3)); Put16(tex0, 8 + 6, (ushort)texDictAt); Put32(tex0, 8 + 12, (uint)texDataAt);
				// tex4x4Info at +24: vramKey, sizeTex>>3, ofsDict, flag, pad, ofsTex (+36), ofsTexPlttIdx (+40)
				Put16(tex0, 24 + 4, (ushort)(blocks.Length >> 3)); Put16(tex0, 24 + 6, (ushort)texDictAt); Put32(tex0, 24 + 12, (uint)blocksAt); Put32(tex0, 24 + 16, (uint)indicesAt);
				// plttInfo at +44: vramKey, sizePltt>>3 (+48), flag, ofsDict (+52), pad, ofsPlttData (+56)
				Put16(tex0, 44 + 4, (ushort)(palettes.Length >> 3)); Put16(tex0, 44 + 8, (ushort)plttDictAt); Put32(tex0, 44 + 12, (uint)plttDataAt);
				Array.Copy(texDict, 0, tex0, texDictAt, texDict.Length);
				Array.Copy(plttDict, 0, tex0, plttDictAt, plttDict.Length);
				texels.ToArray().CopyTo(tex0, texDataAt);
				blocks.ToArray().CopyTo(tex0, blocksAt);
				indices.ToArray().CopyTo(tex0, indicesAt);
				palettes.ToArray().CopyTo(tex0, plttDataAt);

				// BTX0: head 16 + one block offset (4) = 20, then the TEX0.
				byte[] btx0 = new byte[20 + tex0.Length];
				Ascii(btx0, 0, "BTX0"); Put16(btx0, 4, 0xFEFF); Put16(btx0, 6, 1); Put32(btx0, 8, (uint)btx0.Length); Put16(btx0, 12, 16); Put16(btx0, 14, 1); Put32(btx0, 16, 20);
				Array.Copy(tex0, 0, btx0, 20, tex0.Length);

				// NMDP wrapper: as n021.ntxp has it.
				byte[] nmdp = new byte[48 + btx0.Length];
				Ascii(nmdp, 0, "NMDP"); Put32(nmdp, 4, 0x1000); Put32(nmdp, 16, 1); Put32(nmdp, 20, 4); Put32(nmdp, 24, (uint)btx0.Length); Put32(nmdp, 28, 48);
				Array.Copy(btx0, 0, nmdp, 48, btx0.Length);
				return nmdp;
			}
		}

		/// <summary>An NNSG3dResDict with the names and entries given. The tree is the one the game's own single-entry dictionaries carry, grown by a node per entry: every name walks to its own entry when the game looks one up by name.</summary>
		internal static byte[] Dictionary(List<string> names, int unit, Func<int, byte[]> entry)
		{
			int count = names.Count;
			int nodes = count + 1;
			int entriesAt = 8 + nodes * 4;
			int size = entriesAt + 4 + count * unit + count * 16;
			byte[] d = new byte[size];
			d[0] = 0; d[1] = (byte)count; Put16(d, 2, (ushort)size); Put16(d, 4, 8); Put16(d, 6, (ushort)entriesAt);
			// The patricia tree. The root tests bit 127 (never set in a name of ASCII) and goes
			// left; each next node tests a bit that tells its entry from the ones before it, as
			// NNS's own builder does; for one entry the shipped files' node stands as it is.
			byte[][] keys = names.Select(nm => NameBytes(nm)).ToArray();
			d[8] = 0x7F; d[9] = 1; d[10] = 0; d[11] = 0;
			BuildTree(d, 8, keys);
			Put16(d, entriesAt, (ushort)unit); Put16(d, entriesAt + 2, (ushort)(4 + count * unit));
			for (int i = 0; i < count; i++)
			{
				byte[] e = entry(i);
				Array.Copy(e, 0, d, entriesAt + 4 + i * unit, Math.Min(unit, e.Length));
				Array.Copy(keys[i], 0, d, entriesAt + 4 + count * unit + i * 16, 16);
			}
			return d;
		}

		/// <summary>
		/// The patricia tree the game searches: NNS_G3dGetResDataByName walks from node 0, at each
		/// node testing one bit of the 128-bit name and going left or right, until it reaches a node
		/// whose refBit is not less than the last - then compares the name at that node's entry.
		/// Built as NNS's converter does: names inserted one by one; a new node splits at the highest
		/// bit where the new name differs from the name found for it.
		/// </summary>
		private static void BuildTree(byte[] d, int at, byte[][] keys)
		{
			int count = keys.Length;
			// node i (1..count) is entry i-1's node. Fields: refBit, idxLeft, idxRight, idxEntry.
			int[] refBit = new int[count + 1], left = new int[count + 1], right = new int[count + 1], entry = new int[count + 1];
			refBit[0] = 127; left[0] = 1; right[0] = 0; entry[0] = 0;
			// The first name: its node tests a bit of its own (any; the shipped files use one of
			// the name's set bits) and points to itself both ways.
			refBit[1] = 0x1D; left[1] = 0; right[1] = 1; entry[1] = 0;
			if (count == 1 && !Bit(keys[0], 0x1D)) { refBit[1] = FirstSetBit(keys[0]); }
			for (int i = 1; i < count; i++)
			{
				byte[] key = keys[i];
				// Search as the game does.
				int node = 0, next = left[0];
				while (refBit[next] < refBit[node])
				{
					node = next;
					next = Bit(key, refBit[node]) ? right[node] : left[node];
				}
				byte[] found = keys[entry[next]];
				int bit = 127;
				while (bit >= 0 && Bit(key, bit) == Bit(found, bit)) bit--;
				if (bit < 0) throw new InvalidDataException("two textures named " + NameOf(key));
				// Insert a node testing that bit, below the last node whose bit is higher.
				int me = i + 1;
				refBit[me] = bit; entry[me] = i;
				node = 0; next = left[0];
				while (refBit[next] < refBit[node] && refBit[next] > bit)
				{
					node = next;
					next = Bit(key, refBit[node]) ? right[node] : left[node];
				}
				if (Bit(key, bit)) { right[me] = me; left[me] = next; } else { left[me] = me; right[me] = next; }
				if (Bit(key, refBit[node])) right[node] = me; else left[node] = me;
			}
			// The game's walk (NNS_G3dGetResDictIdxByName) steps to the next node only while its index
			// grows and takes a step back or to itself as the end: so a node's children must be numbered
			// after it. Insertion order does not give that for every set of names (the third of
			// w150_0, w150_1, w150_2 split above the second and took index 3 as its parent); number the
			// nodes again by a walk from the root along the tree's forward edges - the ones to a lower
			// bit - so every parent comes before its children and every back edge goes to an ancestor.
			int[] renumber = new int[count + 1];
			for (int i = 0; i <= count; i++) renumber[i] = -1;
			int nextIndex = 0;
			void Visit(int node)
			{
				if (renumber[node] >= 0) return;
				renumber[node] = nextIndex++;
				if (left[node] != node && refBit[left[node]] < refBit[node]) Visit(left[node]);
				if (right[node] != node && refBit[right[node]] < refBit[node]) Visit(right[node]);
			}
			Visit(0);
			for (int i = 0; i <= count; i++) if (renumber[i] < 0) renumber[i] = nextIndex++;   // unreachable nodes (none expected) after the rest
			for (int i = 0; i <= count; i++)
			{
				int to = renumber[i];
				d[at + to * 4] = (byte)refBit[i]; d[at + to * 4 + 1] = (byte)renumber[left[i]]; d[at + to * 4 + 2] = (byte)renumber[right[i]]; d[at + to * 4 + 3] = (byte)entry[i];
			}
		}

		private static bool Bit(byte[] key, int bit) => bit >= 0 && bit < 128 && (key[bit >> 3] & (1 << (bit & 7))) != 0;
		private static int FirstSetBit(byte[] key) { for (int b = 0; b < 128; b++) if (Bit(key, b)) return b; return 0; }
		private static string NameOf(byte[] key) => System.Text.Encoding.ASCII.GetString(key).TrimEnd('\0');

		private static byte[] NameBytes(string name)
		{
			byte[] key = new byte[16];
			byte[] ascii = System.Text.Encoding.ASCII.GetBytes(name ?? "");
			Array.Copy(ascii, key, Math.Min(16, ascii.Length));
			return key;
		}

		private static bool IsSize(int v) => v >= 8 && v <= 1024 && (v & (v - 1)) == 0;
		private static int Log2(int v) { int n = 0; while ((8 << n) < v) n++; return n; }
		private static int Align(int v, int to) => (v + to - 1) / to * to;
		private static void Pad(MemoryStream s, int to) { while (s.Length % to != 0) s.WriteByte(0); }
		private static void Ascii(byte[] d, int at, string s) { for (int i = 0; i < s.Length; i++) d[at + i] = (byte)s[i]; }
		private static void Put16(byte[] d, int at, ushort v) { d[at] = (byte)v; d[at + 1] = (byte)(v >> 8); }
		private static void Put32(byte[] d, int at, uint v) { d[at] = (byte)v; d[at + 1] = (byte)(v >> 8); d[at + 2] = (byte)(v >> 16); d[at + 3] = (byte)(v >> 24); }

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

		private static Encoded Encode4x4(byte[] rgba, int width, int height, int room)
		{
			int bw = width >> 2, bh = height >> 2;
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
			Encoded e = new Encoded { Texels = new byte[blocks.Count * 4], Indices = new byte[blocks.Count * 2], Palette = paletteWords.ToArray() };
			for (int n = 0; n < blocks.Count; n++)
			{
				Block4x4 b = blocks[n];
				int pairs = offsets[string.Join(",", b.Palette)] >> 1;
				int control = (pairs & 0xFFF) | (b.Interpolated ? 0x4000 : 0) | (b.Variant ? 0x8000 : 0);
				e.Indices[n * 2] = (byte)control;
				e.Indices[n * 2 + 1] = (byte)(control >> 8);
				e.Texels[n * 4] = (byte)b.Texels;
				e.Texels[n * 4 + 1] = (byte)(b.Texels >> 8);
				e.Texels[n * 4 + 2] = (byte)(b.Texels >> 16);
				e.Texels[n * 4 + 3] = (byte)(b.Texels >> 24);
			}
			return e;
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
