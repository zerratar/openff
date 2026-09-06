// Finds the game's Content directory.
//
// The extracted content is ~540 MB, so the project deliberately does not copy it
// into bin/. Instead the game runs with its working directory set to whichever
// folder holds Content. Override with the FF3_CONTENT environment variable.

using System;
using System.IO;

namespace OpenFF.Client
{
	internal static class ContentLocator
	{
		/// <summary>A file that only the real content directory has, used to reject look-alikes.</summary>
		private const string Sentinel = "data000.bin";

		/// <summary>
		/// Where the game data comes from: --content when it names our Content or a game
		/// install (Steam FF3's files/, FF4's EXTRACTED_DATA), else the Content directory
		/// found the usual way. The XNB assets - fonts, sound - still come from Content,
		/// which FindContentRoot finds regardless of what the data root is.
		/// </summary>
		public static string FindDataRoot()
		{
			string configured = Options.Get("content");
			if (!string.IsNullOrEmpty(configured))
			{
				// The first of a ;-separated list is the primary; the rest are fallbacks
				// GameArchive adds behind it.
				string first = configured.Split(';')[0].Trim().Trim('"');
				if (OpenFF.Content.ContentChain.Looks(first))
				{
					return Path.GetFullPath(first);
				}
			}
			// Nothing named: the game and source chosen or remembered (Steam by default).
			string chosen = Launch.ResolveRoot();
			if (chosen != null)
			{
				return Path.GetFullPath(chosen);
			}
			return FindContentRoot();
		}

		/// <summary>Absolute path of the Content directory, or null if it could not be found.</summary>
		public static string FindContentRoot()
		{
			string configured = Options.Get("content");
			if (!string.IsNullOrEmpty(configured))
			{
				string first = configured.Split(';')[0].Trim().Trim('"');
				if (IsContentRoot(first))
				{
					return Path.GetFullPath(first);
				}
			}

			// Walk up from the executable. Covers both "Content sits next to the exe"
			// (a published build) and "…/Project/OpenFF/bin/Debug/net8.0" running from
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

			// No Content directory anywhere: a machine with the game from Steam and this
			// executable on its own. The chosen game's install is the working directory
			// then; what the phone build kept as XNB assets comes from the install
			// (TrueType faces, .ogg sound) or is not needed.
			string chosen = Launch.ResolveRoot();
			if (chosen != null)
			{
				return Path.GetFullPath(chosen);
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
