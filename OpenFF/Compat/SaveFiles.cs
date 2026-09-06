// Save-game storage.
//
// The phone build wrote through Android's per-app private storage, which the shim
// mapped onto IsolatedStorage. On Windows the natural home is the user's roaming
// application data, where saves survive reinstalls and are easy to find and back up.

using System;
using System.IO;

namespace OpenFF.Client
{
	internal static class SaveFiles
	{
		private static string _directory;

		/// <summary>%APPDATA%\FF3, created on first use.</summary>
		public static string Directory_
		{
			get
			{
				if (_directory == null)
				{
					_directory = Path.Combine(
						Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FF3");
					System.IO.Directory.CreateDirectory(_directory);
					Log.Write(LogChannel.File, "save directory: " + _directory);
				}
				return _directory;
			}
		}

		public static string PathFor(string name) => Path.Combine(Directory_, name);

		/// <summary>Creates a zero-filled save file of the given size, replacing any existing one.</summary>
		public static void Create(string name, int size)
		{
			try
			{
				File.WriteAllBytes(PathFor(name), new byte[size]);
				Log.Write(LogChannel.File, "created save file " + name + " (" + size + " bytes)");
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.File, "could not create save file " + name + ": " + ex.Message);
			}
		}
	}
}
