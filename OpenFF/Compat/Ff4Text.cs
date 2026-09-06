// FF4's text files, in the shape the FF3 message code reads.
//
// Both games keep dialogue in .msd: a header, twelve bytes per entry (id, how many
// strings, offset) and the strings. The phone FF3 stored them in SJIS or Windows-1252 and
// AppShell.decodeString rewrote each file at load into UTF-8, NUL-terminated - which is
// what the message manager walks. FF4 stores them as UTF-16LE. This does for UTF-16 what
// decodeString does for the legacy encodings, so the rest of the text path is unchanged.

using System;
using System.Text;

namespace OpenFF.Client
{
	internal static class Ff4Text
	{
		/// <summary>Whether the file's first string is UTF-16LE rather than a byte encoding.</summary>
		public static bool IsWide(byte[] file)
		{
			if (file == null || file.Length < 28)
			{
				return false;
			}
			int count = BitConverter.ToInt32(file, 8);
			if (count <= 0 || 16 + 12 * count > file.Length)
			{
				return false;
			}
			int offset = BitConverter.ToInt32(file, 16 + 8);
			return offset + 2 <= file.Length && file[offset] != 0 && file[offset + 1] == 0;
		}

		/// <summary>The UTF-16LE file rewritten with UTF-8 strings, the entry offsets following.</summary>
		public static byte[] DecodeWide(byte[] file)
		{
			int count = BitConverter.ToInt32(file, 8);
			int table = 16 + 12 * count;
			byte[] output = new byte[file.Length * 2 + 16];
			Buffer.BlockCopy(file, 0, output, 0, Math.Min(table, file.Length));
			int write = table;
			for (int i = 0; i < count; i++)
			{
				int entry = 16 + 12 * i;
				int strings = file[entry + 4];
				int read = BitConverter.ToInt32(file, entry + 8);
				output[entry + 8] = (byte)write;
				output[entry + 9] = (byte)(write >> 8);
				output[entry + 10] = (byte)(write >> 16);
				output[entry + 11] = (byte)(write >> 24);
				for (int j = 0; j < strings; j++)
				{
					int end = read;
					while (end + 1 < file.Length && (file[end] != 0 || file[end + 1] != 0))
					{
						end += 2;
					}
					byte[] utf8 = Encoding.UTF8.GetBytes(Encoding.Unicode.GetString(file, read, Math.Max(0, end - read)));
					if (write + utf8.Length + 2 > output.Length)
					{
						Array.Resize(ref output, (output.Length + utf8.Length + 2) * 2);
					}
					Buffer.BlockCopy(utf8, 0, output, write, utf8.Length);
					write += utf8.Length;
					output[write++] = 0;
					read = end + 2;
				}
				if (write + 1 > output.Length)
				{
					Array.Resize(ref output, output.Length * 2);
				}
				output[write++] = 0;
			}
			Array.Resize(ref output, write);
			return output;
		}
	}
}
