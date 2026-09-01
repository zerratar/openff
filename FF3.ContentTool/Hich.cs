// .hich - what stands on a map.
//
//   ff3content hich <file.hich | directory> [out]
//
// One row per character: which model it wears, where it stands, and - the useful part
// - the cast number that gives it its behaviour. That cast is a program in the map's
// .script, so a hich row and a script cast are two halves of the same thing: one says
// what and where, the other says what it does.
//
// Layout (little endian)
//   +0  entry count
//   then 72 bytes each:
//     +0  character id      which model
//     +4  name, 8 bytes     the model's name, NUL padded
//     +12 id                the cast number in the map's script
//     +16 kind              0 placed character, 1 map logic, 2 extra logic
//     +20 kind parameter
//     +24 position x, y, z, w     whole units, not fixed point
//     +40 posture  x, y, z, w
//     +56 scale    x, y, z, w
//
// Positions here are the authored layout. A script can override one when it boots a
// character with bootCharacter_AbsoluteCoordination, and that operand *is* fixed
// point - the two are not the same units, which is worth knowing before moving
// anything.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FF3.ContentTool
{
	internal sealed class HichEntry
	{
		public uint CharacterId { get; set; }
		public string Model { get; set; }

		/// <summary>
		/// The eight name bytes as they stand. The shipped files leave whatever was in
		/// the buffer after the name's NUL, so an unchanged entry is written back
		/// verbatim rather than tidied - tidying it would change the file.
		/// </summary>
		public string ModelRaw { get; set; }
		public int Cast { get; set; }
		public int Kind { get; set; }
		public int KindParameter { get; set; }
		public int[] Position { get; set; } = new int[4];
		public int[] Posture { get; set; } = new int[4];
		public int[] Scale { get; set; } = new int[4];

		public string KindName
		{
			get
			{
				switch (Kind)
				{
					case 0: return "character";
					case 1: return "map logic";
					case 2: return "extra logic";
					default: return "unknown";
				}
			}
		}
	}

	internal static class Hich
	{
		private const int EntrySize = 72;
		private const int NameLength = 8;

		public static List<HichEntry> Read(byte[] data)
		{
			if (data == null || data.Length < 4)
			{
				throw new InvalidDataException("too small to be a hich file");
			}

			int count = ReadInt32(data, 0);
			if (count < 0 || 4 + (long)count * EntrySize > data.Length)
			{
				throw new InvalidDataException("entry count is not plausible: " + count);
			}

			List<HichEntry> entries = new List<HichEntry>(count);
			for (int i = 0; i < count; i++)
			{
				int at = 4 + i * EntrySize;
				HichEntry entry = new HichEntry
				{
					CharacterId = (uint)ReadInt32(data, at),
					Model = ReadName(data, at + 4),
					ModelRaw = Convert.ToHexString(data, at + 4, NameLength),
					Cast = ReadInt32(data, at + 12),
					Kind = ReadInt32(data, at + 16),
					KindParameter = ReadInt32(data, at + 20)
				};
				for (int j = 0; j < 4; j++)
				{
					entry.Position[j] = ReadInt32(data, at + 24 + j * 4);
					entry.Posture[j] = ReadInt32(data, at + 40 + j * 4);
					entry.Scale[j] = ReadInt32(data, at + 56 + j * 4);
				}
				entries.Add(entry);
			}
			return entries;
		}

		public static byte[] Write(List<HichEntry> entries)
		{
			byte[] data = new byte[4 + entries.Count * EntrySize];
			WriteInt32(data, 0, entries.Count);

			for (int i = 0; i < entries.Count; i++)
			{
				HichEntry entry = entries[i];
				int at = 4 + i * EntrySize;
				WriteInt32(data, at, (int)entry.CharacterId);
				WriteName(data, at + 4, entry.Model, entry.ModelRaw);
				WriteInt32(data, at + 12, entry.Cast);
				WriteInt32(data, at + 16, entry.Kind);
				WriteInt32(data, at + 20, entry.KindParameter);
				for (int j = 0; j < 4; j++)
				{
					WriteInt32(data, at + 24 + j * 4, Component(entry.Position, j));
					WriteInt32(data, at + 40 + j * 4, Component(entry.Posture, j));
					WriteInt32(data, at + 56 + j * 4, Component(entry.Scale, j));
				}
			}
			return data;
		}

		private static int Component(int[] values, int index)
		{
			return values != null && index < values.Length ? values[index] : 0;
		}

		/// <summary>
		/// The model name. The eight bytes are not always NUL terminated - the shipped
		/// files leave whatever was in the buffer after the name - so what follows the
		/// first NUL is kept as it stands and written back untouched.
		/// </summary>
		private static string ReadName(byte[] data, int at)
		{
			int end = at;
			while (end < at + NameLength && data[end] != 0)
			{
				end++;
			}
			return Encoding.ASCII.GetString(data, at, end - at);
		}

		private static void WriteName(byte[] data, int at, string name, string raw)
		{
			// Unchanged: put the original bytes back, junk and all.
			if (raw != null && raw.Length == NameLength * 2)
			{
				byte[] original = Convert.FromHexString(raw);
				if (string.Equals(ReadName(original, 0), name, StringComparison.Ordinal))
				{
					Buffer.BlockCopy(original, 0, data, at, NameLength);
					return;
				}
			}

			byte[] bytes = Encoding.ASCII.GetBytes(name ?? string.Empty);
			int length = Math.Min(bytes.Length, NameLength);
			Buffer.BlockCopy(bytes, 0, data, at, length);
		}

		private static int ReadInt32(byte[] data, int at)
		{
			return data[at] | (data[at + 1] << 8) | (data[at + 2] << 16) | (data[at + 3] << 24);
		}

		private static void WriteInt32(byte[] data, int at, int value)
		{
			data[at] = (byte)value;
			data[at + 1] = (byte)(value >> 8);
			data[at + 2] = (byte)(value >> 16);
			data[at + 3] = (byte)(value >> 24);
		}
	}
}
