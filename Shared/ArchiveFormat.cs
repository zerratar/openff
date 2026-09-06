// The data*.bin archive format, with no dependencies.
//
// Shared source: compiled into both OpenFF (which reads through it at runtime)
// and Crystal (which extracts through it). One implementation, so the two
// cannot drift.
//
// Layout
//   data000.bin   the file table:
//                   +0  format
//                   +4  file count      (big endian, as is everything here)
//                   +8  archive count
//                   +12 entries, 12 bytes each, sorted by name:
//                         +0 archive index
//                         +4 file index within that archive
//                         +8 offset of the NUL-terminated name, within this table
//   dataNNN.bin   archive N: a table of 32-bit offsets, entry i at (i + 1) * 4,
//                 each pointing at a 32-bit length followed by that many bytes.
//                 The top bit of an offset is a flag and is masked off.
//
// The phone build ran the table through an "encode" step first, but that function
// read the array and returned without touching it, so there is nothing to undo.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenFF.Formats
{
	public readonly struct ArchiveEntry
	{
		public readonly string Name;
		public readonly int Archive;
		public readonly int Index;

		public ArchiveEntry(string name, int archive, int index)
		{
			Name = name;
			Archive = archive;
			Index = index;
		}
	}

	public sealed class ArchiveIndex
	{
		private const int EntrySize = 12;
		private const int EntryTableStart = 12;

		private readonly byte[] _table;

		public int FileCount { get; }
		public int ArchiveCount { get; }

		private ArchiveIndex(byte[] table)
		{
			_table = table;
			FileCount = ReadInt32(table, 4);
			ArchiveCount = ReadInt32(table, 8);
		}

		/// <summary>Parses data000.bin. Throws if it is not a plausible table.</summary>
		public static ArchiveIndex Load(byte[] table)
		{
			if (table == null || table.Length < EntryTableStart)
			{
				throw new InvalidDataException("archive table is too small to be valid");
			}
			ArchiveIndex index = new ArchiveIndex(table);
			if (index.FileCount < 0 || index.ArchiveCount < 0
				|| EntryTableStart + (long)index.FileCount * EntrySize > table.Length)
			{
				throw new InvalidDataException("archive table header is not plausible: "
					+ index.FileCount + " files, " + index.ArchiveCount + " archives");
			}
			return index;
		}

		/// <summary>File name for archive N, e.g. "Content/data007.bin".</summary>
		public static string ArchiveName(string directory, int archive)
		{
			return string.Format("{0}/data{1:D3}.bin", directory, archive);
		}

		public ArchiveEntry this[int i] => new ArchiveEntry(
			NameAt(ReadInt32(_table, i * EntrySize + 20)),
			ReadInt32(_table, i * EntrySize + EntryTableStart),
			ReadInt32(_table, i * EntrySize + 16));

		public IEnumerable<ArchiveEntry> Entries
		{
			get
			{
				for (int i = 0; i < FileCount; i++)
				{
					yield return this[i];
				}
			}
		}

		/// <summary>Binary search over the name-sorted entries.</summary>
		public bool TryFind(string name, out ArchiveEntry entry)
		{
			byte[] wanted = Encoding.UTF8.GetBytes(name);
			int low = 0;
			int high = FileCount;

			while (high > low)
			{
				int middle = (low + high) / 2;
				int nameAt = ReadInt32(_table, middle * EntrySize + 20);

				int order = 0;
				for (int i = 0; i < wanted.Length && order == 0; i++)
				{
					order = (_table[nameAt + i] & 0xFF) - (wanted[i] & 0xFF);
				}
				if (order == 0)
				{
					// Equal so far: the table name must also end here.
					order = _table[nameAt + wanted.Length] & 0xFF;
				}

				if (order == 0)
				{
					entry = this[middle];
					return true;
				}
				if (order > 0)
				{
					high = middle;
				}
				else
				{
					low = middle + 1;
				}
			}

			entry = default;
			return false;
		}

		/// <summary>Reads one blob out of an open archive stream.</summary>
		public static byte[] ReadBlob(Stream archive, int index)
		{
			if (archive == null)
			{
				return null;
			}

			archive.Position = (index + 1) * 4L;
			int offset = ReadInt32(archive) & 0x7FFFFFFF;
			if (offset < 0 || offset + 4L > archive.Length)
			{
				return null;
			}

			archive.Position = offset;
			int length = ReadInt32(archive);
			if (length < 0 || offset + 4L + length > archive.Length)
			{
				return null;
			}

			byte[] data = new byte[length];
			int read = 0;
			while (read < length)
			{
				int step = archive.Read(data, read, length - read);
				if (step <= 0)
				{
					return null;
				}
				read += step;
			}
			return data;
		}

		private string NameAt(int at)
		{
			int end = at;
			while (end < _table.Length && _table[end] != 0)
			{
				end++;
			}
			return Encoding.UTF8.GetString(_table, at, end - at);
		}

		private static int ReadInt32(byte[] data, int at)
		{
			return (data[at] << 24) | ((data[at + 1] & 0xFF) << 16)
				| ((data[at + 2] & 0xFF) << 8) | (data[at + 3] & 0xFF);
		}

		private static int ReadInt32(Stream stream)
		{
			byte[] b = new byte[4];
			if (stream.Read(b, 0, 4) != 4)
			{
				return -1;
			}
			return (b[0] << 24) | (b[1] << 16) | (b[2] << 8) | b[3];
		}
	}
}
