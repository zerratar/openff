// Cell banks, for the editor.
//
// A bank is a cutting plan: "take this rectangle out of that sheet and put it here".
// On its own it is a table of numbers; against its sheet it is a window frame, a
// button, or a whole menu screen. The composing happens in the browser, because it is
// exactly a canvas drawImage per part and the sheet is already a PNG the browser can
// load - so what goes over the wire is the plan and the name of the sheet.
//
// The one job that cannot be done in the browser is finding the sheet. The bank names
// it as a bare file name and the workspace holds path-qualified ones, with the same
// picture appearing under several languages, so that lookup happens here.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Crystal.Editor
{
	internal sealed class CellFile
	{
		public string Name { get; set; }

		/// <summary>"cells", "screen" or "animation" - what is really inside.</summary>
		public string Kind { get; set; }
	}

	internal sealed class CellView
	{
		public string Name { get; set; }
		public string Kind { get; set; }

		/// <summary>The sheet, as a workspace name the browser can ask for.</summary>
		public string Sheet { get; set; }
		public string SheetFrom { get; set; }
		public int SheetWidth { get; set; }
		public int SheetHeight { get; set; }

		public List<Cell> Cells { get; set; }
		public ScreenData Screen { get; set; }
		public AnimBank Animation { get; set; }
		public string Problem { get; set; }
	}

	internal static class CellBanks
	{
		public static readonly string[] Extensions = { ".NCER", ".NSCR", ".NANR" };

		public static List<CellFile> List(Workspace workspace)
		{
			List<CellFile> files = new List<CellFile>();
			foreach (WorkspaceEntry entry in workspace.List(Extensions))
			{
				string kind;
				try
				{
					byte[] data = workspace.Read(entry.Name);
					kind = Cells.IsCellBank(data) ? "cells"
						: Cells.IsScreen(data) ? "screen"
						: Cells.IsAnimation(data) ? "animation"
						: "unknown";
				}
				catch (Exception)
				{
					kind = "unreadable";
				}
				files.Add(new CellFile { Name = entry.Name, Kind = kind });
			}
			return files.OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase).ToList();
		}

		public static CellView Read(Workspace workspace, string name)
		{
			byte[] data = workspace.Read(name);

			if (Cells.IsScreen(data))
			{
				return new CellView
				{
					Name = name,
					Kind = "screen",
					Screen = Cells.ReadScreen(data, name)
				};
			}

			if (Cells.IsAnimation(data))
			{
				return new CellView
				{
					Name = name,
					Kind = "animation",
					Animation = Cells.ReadAnimation(data, name)
				};
			}

			if (!Cells.IsCellBank(data))
			{
				return new CellView { Name = name, Kind = "unknown", Problem = "no CEBK, SCRN or ABNK block" };
			}

			Dictionary<string, string> pictures = Pictures(workspace);
			CellBank bank = Cells.ReadCellBank(data, name, pictures.ContainsKey);

			CellView view = new CellView
			{
				Name = name,
				Kind = "cells",
				SheetFrom = bank.SheetFrom,
				Cells = bank.Cells
			};

			if (bank.Sheet != null && pictures.TryGetValue(bank.Sheet, out string entry))
			{
				view.Sheet = entry;
				try
				{
					ImageInfo info = Images.Describe(workspace.Read(entry));
					view.SheetWidth = info.Width;
					view.SheetHeight = info.Height;
				}
				catch (Exception)
				{
					// The size only drives a warning, so not having it is survivable.
				}
			}
			else
			{
				view.Problem = "the sheet it draws from, " + (bank.Sheet ?? "unknown")
					+ ", is not in the content";
			}

			return view;
		}

		/// <summary>
		/// Bare picture name -> the workspace entry to ask for. The same picture appears
		/// under several languages; the first wins, which is the one the game would find
		/// too since the banks are not language-specific.
		/// </summary>
		private static Dictionary<string, string> Pictures(Workspace workspace)
		{
			Dictionary<string, string> pictures =
				new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			foreach (WorkspaceEntry entry in workspace.List(Images.Extensions))
			{
				int slash = entry.Name.LastIndexOf('/');
				string file = slash < 0 ? entry.Name : entry.Name.Substring(slash + 1);
				if (!pictures.ContainsKey(file))
				{
					pictures[file] = entry.Name;
				}
			}
			return pictures;
		}
	}
}
