// Reader for the game's own data*.bin archives.
//
// This is the format the whole game is built on - every map, sprite, model,
// script and table lives here - and it has nothing to do with XNA or the content
// pipeline, so it ports unchanged. Extracted out of DLActivity, which wrapped it
// in a Square Enix login and a resource download that a Windows build has no use
// for. The "obfuscation" the phone build applied to the table was a no-op
// (MainActivity.encode reads the array and returns without touching it), so
// nothing is lost by dropping the key handling with it.
//
// Layout
//   data000.bin   the file table:
//                   +0  format
//                   +4  file count      (big endian)
//                   +8  archive count
//                   +12 entries, 12 bytes each, sorted by name:
//                         +0 archive index
//                         +4 file index within that archive
//                         +8 offset of the NUL-terminated name, within this table
//   dataNNN.bin   archive N: a table of 32-bit offsets, entry i at (i + 1) * 4,
//                 each pointing at a 32-bit length followed by that many bytes.

using System;
using System.IO;

namespace FF3
{
	internal static class GameArchive
	{
		private static byte[] _table;

		public static bool IsLoaded => _table != null;

		/// <summary>Directory holding data000.bin, relative to the working directory.</summary>
		public static string DataPath => "Content";

		/// <summary>Loads the file table. Returns false if it or any archive is missing.</summary>
		public static bool Load()
		{
			try
			{
				string path = Path.Combine(DataPath, "data000.bin");
				string resolved = GameFiles.Resolve(path) ?? path;
				_table = File.ReadAllBytes(resolved);
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.File, "archive table could not be read: " + ex.Message);
				_table = null;
				return false;
			}

			int archives = ReadInt32(_table, 8);
			for (int i = 0; i < archives; i++)
			{
				if (GameFiles.Resolve(ArchivePath(i)) == null)
				{
					Log.Write(LogChannel.File, "missing archive " + ArchivePath(i));
					_table = null;
					return false;
				}
			}

			Log.Write(LogChannel.File, string.Format(
				"archives loaded: {0} files across {1} volumes",
				ReadInt32(_table, 4), archives));
			return true;
		}

		private static string ArchivePath(int index)
		{
			return string.Format("{0}/data{1:D3}.bin", DataPath, index);
		}

		/// <summary>Big-endian int32, the byte order the table is written in.</summary>
		private static int ReadInt32(byte[] data, int at)
		{
			return (data[at] << 24) | ((data[at + 1] & 0xFF) << 16)
				| ((data[at + 2] & 0xFF) << 8) | (data[at + 3] & 0xFF);
		}

		/// <summary>Reads one file by name, or null if it is not in the table.</summary>
		public static byte[] Read(string filename)
		{
			if (_table == null || string.IsNullOrEmpty(filename))
			{
				return null;
			}

			int entry = Find(filename);
			if (entry < 0)
			{
				return null;
			}

			int archive = ReadInt32(_table, entry * 12 + 12);
			int index = ReadInt32(_table, entry * 12 + 16);

			try
			{
				string path = GameFiles.Resolve(ArchivePath(archive));
				if (path == null)
				{
					return null;
				}
				using FileStream stream = File.OpenRead(path);
				using BinaryReader reader = new BinaryReader(stream);

				// Offset table entry for this file, then the blob it points at.
				stream.Position = (index + 1) * 4;
				int offset = ReadBigEndian(reader) & 0x7FFFFFFF;
				stream.Position = offset;
				int length = ReadBigEndian(reader);
				if (length < 0 || offset + 4L + length > stream.Length)
				{
					Log.Write(LogChannel.File, "bad archive entry for " + filename);
					return null;
				}
				return reader.ReadBytes(length);
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.File, "archive read failed for " + filename + ": " + ex.Message);
				return null;
			}
		}

		private static int ReadBigEndian(BinaryReader reader)
		{
			byte[] b = reader.ReadBytes(4);
			return (b[0] << 24) | (b[1] << 16) | (b[2] << 8) | b[3];
		}

		/// <summary>Binary search over the name-sorted entry table.</summary>
		private static int Find(string filename)
		{
			byte[] wanted = StringUtil.getBytes(filename);
			int low = 0;
			int high = ReadInt32(_table, 4);

			while (high > low)
			{
				int middle = (low + high) / 2;
				int nameAt = ReadInt32(_table, middle * 12 + 20);

				int order = 0;
				for (int i = 0; i < wanted.Length && order == 0; i++)
				{
					order = (_table[nameAt + i] & 0xFF) - (wanted[i] & 0xFF);
				}
				if (order == 0)
				{
					// Both must end here, or the table name is the longer one.
					order = _table[nameAt + wanted.Length] & 0xFF;
				}

				if (order == 0)
				{
					return middle;
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
			return -1;
		}
	}
}
