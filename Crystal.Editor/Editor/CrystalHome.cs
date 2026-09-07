// Where Crystal keeps what is its own: projects, and the edits made to a game install
// without a project.
//
//   %LOCALAPPDATA%\OpenFF\Crystal\projects\<name>     the projects (FF3_PROJECTS overrides)
//   %LOCALAPPDATA%\OpenFF\Crystal\mods\<install>      scratch edits per install, no project
//
// The folder used to be %LOCALAPPDATA%\FF3ContentTool, the tool's first name. The first
// time this runs and finds the old folder with no new one, it moves it - one rename, the
// projects and their backups inside it untouched - and says so. The OpenFF client looks in
// both when --project names a project by name, so an old client and a new editor agree.

using System;
using System.IO;

namespace Crystal.Editor
{
	internal static class CrystalHome
	{
		public const string OldName = "FF3ContentTool";

		private static string _root;

		/// <summary>%LOCALAPPDATA%\OpenFF\Crystal, migrated to on first use; the old folder when the move could not be made.</summary>
		public static string Root => _root ??= Resolve();

		public static string Projects => Path.Combine(Root, "projects");
		public static string Mods => Path.Combine(Root, "mods");

		private static string Resolve()
		{
			string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			string root = Path.Combine(local, "OpenFF", "Crystal");
			string old = Path.Combine(local, OldName);
			if (Directory.Exists(root) || !Directory.Exists(old)) return root;
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(root));
				Directory.Move(old, root);
				Console.WriteLine("  moved     {0} -> {1} (Crystal's projects and edits, under their new name)", old, root);
				return root;
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				// Something holds the old folder (a game running off an override in it, an
				// Explorer window): keep using it this time and try again next start.
				Console.Error.WriteLine("  could not move {0} to {1} ({2}); using the old folder this time", old, root, ex.Message);
				return old;
			}
		}
	}
}
