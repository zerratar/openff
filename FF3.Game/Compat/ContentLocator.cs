// Finds the game's Content directory.
//
// The extracted content is ~540 MB, so the project deliberately does not copy it
// into bin/. Instead the game runs with its working directory set to whichever
// folder holds Content. Override with the FF3_CONTENT environment variable.

using System;
using System.IO;

namespace FF3
{
	internal static class ContentLocator
	{
		/// <summary>A file that only the real content directory has, used to reject look-alikes.</summary>
		private const string Sentinel = "data000.bin";

		/// <summary>Absolute path of the Content directory, or null if it could not be found.</summary>
		public static string FindContentRoot()
		{
			string fromEnv = Environment.GetEnvironmentVariable("FF3_CONTENT");
			if (!string.IsNullOrEmpty(fromEnv) && IsContentRoot(fromEnv))
			{
				return Path.GetFullPath(fromEnv);
			}

			// Walk up from the executable. Covers both "Content sits next to the exe"
			// (a published build) and "…/Project/FF3.Game/bin/Debug/net8.0" running from
			// the repo, where the content lives in a sibling of Project/.
			DirectoryInfo dir = new DirectoryInfo(AppContext.BaseDirectory);
			while (dir != null)
			{
				string direct = Path.Combine(dir.FullName, "Content");
				if (IsContentRoot(direct))
				{
					return direct;
				}

				string unpacked = Path.Combine(dir.FullName, "Unpacked", "Content");
				if (IsContentRoot(unpacked))
				{
					return unpacked;
				}

				dir = dir.Parent;
			}

			return null;
		}

		private static bool IsContentRoot(string path)
		{
			try
			{
				return Directory.Exists(path) && File.Exists(Path.Combine(path, Sentinel));
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
