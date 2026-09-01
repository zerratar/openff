// Small helpers used by the logging probes: hex previews and raw-asset dumps.
//
// Dumping is opt-in via FF3_DUMP=<directory>. When set, decoded source blobs are
// written there so they can be inspected outside the game.

using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace FF3
{
	internal static class Diagnostics
	{
		private static string DumpDir => Options.Get("dump");
		private static int _counter;

		/// <summary>First <paramref name="count"/> bytes as hex, for identifying file formats.</summary>
		public static string Hex(byte[] data, int count)
		{
			if (data == null)
			{
				return "<null>";
			}
			int n = Math.Min(count, data.Length);
			StringBuilder sb = new StringBuilder(n * 3 + 8);
			for (int i = 0; i < n; i++)
			{
				sb.Append(data[i].ToString("x2", CultureInfo.InvariantCulture));
				sb.Append(' ');
			}
			sb.Append('|');
			for (int i = 0; i < n; i++)
			{
				char c = (char)data[i];
				sb.Append(c >= ' ' && c < (char)127 ? c : '.');
			}
			sb.Append('|');
			return sb.ToString();
		}

		/// <summary>Writes the blob to the dump directory if one is configured. Returns a log suffix.</summary>
		public static string DumpSource(byte[] data, string extension)
		{
			string dir = DumpDir;
			if (string.IsNullOrEmpty(dir) || data == null)
			{
				return string.Empty;
			}
			try
			{
				Directory.CreateDirectory(dir);
				int n = System.Threading.Interlocked.Increment(ref _counter);
				string path = Path.Combine(dir,
					string.Format(CultureInfo.InvariantCulture, "blob{0:D5}.{1}", n, extension));
				File.WriteAllBytes(path, data);
				return " dumped=" + path;
			}
			catch (Exception ex)
			{
				return " dump-failed=" + ex.Message;
			}
		}
	}
}
