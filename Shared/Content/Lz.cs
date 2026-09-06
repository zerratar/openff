// LZ77, the NDS variety. 2734 of the archived files are compressed with it, which is
// most of the graphics and model data.
//
//   crystal lz          <file.lz | directory> [out]
//   crystal lz-compress <file> [out.lz]
//
// The decompressor is the same algorithm as the game's MI_ReadUncompLZ8, which is the
// live path - the one shot MI_UncompressLZ8 next to it is an empty stub in this build,
// so the game streams every compressed file through the routine mirrored here.
//
// Layout
//   +0  one 32 bit word: low nibble parameter, next nibble type (1 = LZ77),
//       top 24 bits the decompressed size
//   +4  a flag byte, then eight items, repeating:
//         flag bit 0  one literal byte
//         flag bit 1  two bytes: length = (first >> 4) + 3, and
//                     distance = ((first & 0xF) << 8) + second + 1, copied from
//                     what has already been written

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace OpenFF.Content
{
	internal static class Lz
	{
		private const int TypeLz = 1;
		private const int MinMatch = 3;
		private const int MaxMatch = 18;         // (0xF >> 0) + 3, four bits of length
		private const int MaxDistance = 4096;    // twelve bits, plus one

		public static bool IsCompressed(byte[] data)
		{
			return data != null && data.Length >= 4 && ((data[0] & 0xF0) >> 4) == TypeLz;
		}

		/// <summary>Decompressed size from the header, without decompressing.</summary>
		public static int SizeOf(byte[] data)
		{
			return data[1] | (data[2] << 8) | (data[3] << 16);
		}

		public static byte[] Decompress(byte[] data)
		{
			if (!IsCompressed(data))
			{
				throw new InvalidDataException("not LZ77 compressed");
			}

			byte[] destination = new byte[SizeOf(data)];
			int at = 4;
			int written = 0;
			int flags = 0;
			int flagBits = 0;

			while (written < destination.Length)
			{
				if (flagBits == 0)
				{
					if (at >= data.Length)
					{
						throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture,
							"ran out of input after {0} of {1} bytes",
							written, destination.Length));
					}
					flags = data[at++];
					flagBits = 8;
					continue;
				}

				flagBits--;
				if ((flags & (1 << flagBits)) == 0)
				{
					if (at >= data.Length)
					{
						throw new InvalidDataException("ran out of input mid literal");
					}
					destination[written++] = data[at++];
					continue;
				}

				if (at + 1 >= data.Length)
				{
					throw new InvalidDataException("ran out of input mid reference");
				}
				int first = data[at++];
				int distance = ((first & 0xF) << 8) + data[at++] + 1;
				int length = (first >> 4) + MinMatch;

				if (distance > written)
				{
					throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture,
						"a reference at {0} reaches {1} bytes back, before the start",
						written, distance));
				}
				for (int i = 0; i < length && written < destination.Length; i++)
				{
					destination[written] = destination[written - distance];
					written++;
				}
			}
			return destination;
		}

		/// <summary>
		/// Compresses, greedily: at each position take the longest match in range, or
		/// emit a literal. The game's own compressor is not here to copy, so this will
		/// not reproduce a shipped file byte for byte - what it produces is a file the
		/// game's decompressor turns back into exactly the input, which is what
		/// matters for putting edited content back.
		/// </summary>
		public static byte[] Compress(byte[] data)
		{
			if (data.Length > 0xFFFFFF)
			{
				throw new InvalidDataException(
					"the header holds a 24 bit size, and this is larger than that");
			}

			using MemoryStream output = new MemoryStream();
			output.WriteByte(TypeLz << 4);
			output.WriteByte((byte)data.Length);
			output.WriteByte((byte)(data.Length >> 8));
			output.WriteByte((byte)(data.Length >> 16));

			// Where each byte value last appeared, so a match search does not have to
			// walk the whole window for every position.
			Dictionary<int, List<int>> seen = new Dictionary<int, List<int>>();

			int at = 0;
			while (at < data.Length)
			{
				long flagPosition = output.Position;
				output.WriteByte(0);
				int flags = 0;

				for (int bit = 7; bit >= 0 && at < data.Length; bit--)
				{
					(int distance, int length) = LongestMatch(data, at, seen);
					if (length >= MinMatch)
					{
						flags |= 1 << bit;
						output.WriteByte((byte)(((length - MinMatch) << 4) | ((distance - 1) >> 8)));
						output.WriteByte((byte)((distance - 1) & 0xFF));
						for (int i = 0; i < length; i++)
						{
							Remember(seen, data, at + i);
						}
						at += length;
					}
					else
					{
						output.WriteByte(data[at]);
						Remember(seen, data, at);
						at++;
					}
				}

				long end = output.Position;
				output.Position = flagPosition;
				output.WriteByte((byte)flags);
				output.Position = end;
			}

			return output.ToArray();
		}

		private static void Remember(Dictionary<int, List<int>> seen, byte[] data, int at)
		{
			if (at + 1 >= data.Length)
			{
				return;
			}
			int key = data[at] | (data[at + 1] << 8);
			if (!seen.TryGetValue(key, out List<int> positions))
			{
				seen[key] = positions = new List<int>();
			}
			positions.Add(at);
		}

		private static (int Distance, int Length) LongestMatch(byte[] data, int at,
			Dictionary<int, List<int>> seen)
		{
			if (at + 1 >= data.Length)
			{
				return (0, 0);
			}
			int key = data[at] | (data[at + 1] << 8);
			if (!seen.TryGetValue(key, out List<int> positions))
			{
				return (0, 0);
			}

			int bestLength = 0;
			int bestDistance = 0;
			int limit = Math.Min(MaxMatch, data.Length - at);

			// Recent positions first, and stop once the window is left behind.
			for (int i = positions.Count - 1; i >= 0; i--)
			{
				int candidate = positions[i];
				int distance = at - candidate;
				if (distance > MaxDistance)
				{
					break;
				}

				int length = 0;
				while (length < limit && data[candidate + length] == data[at + length])
				{
					length++;
				}
				if (length > bestLength)
				{
					bestLength = length;
					bestDistance = distance;
					if (length == limit)
					{
						break;
					}
				}
			}

			return bestLength >= MinMatch ? (bestDistance, bestLength) : (0, 0);
		}
	}
}
