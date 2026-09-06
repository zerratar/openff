// Raw (non-ContentManager) game file access.
//
// The phone build read its archives with TitleContainer.OpenStream, which resolves
// relative to the executable's directory. Here the content deliberately lives
// outside bin/ (it is ~540 MB), so paths like "Content/data000.bin" have to be
// resolved against the located content root instead.

using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace OpenFF.Client
{
	// MonoGame's types by name: the engine's OpenFF.Game, GameTime, Color and vectors sit a namespace up.
	using Game = Microsoft.Xna.Framework.Game;

	internal static class GameFiles
	{
		/// <summary>Directory that "Content/..." paths are relative to. Set once at start-up.</summary>
		public static string BaseDirectory { get; set; }

		/// <summary>Opens a game file for reading, or throws FileNotFoundException.</summary>
		public static Stream OpenRead(string path)
		{
			string resolved = Resolve(path);
			if (resolved != null)
			{
				return File.OpenRead(resolved);
			}

			// Fall back to MonoGame's own lookup so a published, self-contained
			// layout (Content next to the exe) keeps working unchanged.
			return TitleContainer.OpenStream(path);
		}

		/// <summary>Reads a whole game file. Throws if it is missing.</summary>
		public static byte[] ReadAllBytes(string path)
		{
			string resolved = Resolve(path);
			if (resolved != null)
			{
				return File.ReadAllBytes(resolved);
			}
			using Stream stream = OpenRead(path);
			using MemoryStream buffer = new MemoryStream();
			stream.CopyTo(buffer);
			return buffer.ToArray();
		}

		/// <summary>Absolute path of an existing game file, or null if there is no such file.</summary>
		public static string Resolve(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return null;
			}
			if (Path.IsPathRooted(path))
			{
				return File.Exists(path) ? path : null;
			}
			if (!string.IsNullOrEmpty(BaseDirectory))
			{
				string candidate = Path.Combine(BaseDirectory, path);
				if (File.Exists(candidate))
				{
					return candidate;
				}
			}
			string relative = Path.GetFullPath(path);
			return File.Exists(relative) ? relative : null;
		}
	}
}
