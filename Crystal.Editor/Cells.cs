// NCER cell banks, NSCR screens and NANR animation.
//
//   crystal cells <file | directory> [out]
//
// These are the tables that say which piece of a sheet goes where. The pictures were
// already readable - all 542 are PNGs - but a picture on its own is a sheet of parts,
// and this is what turns it back into a window frame, a button, or a whole menu.
//
// The names lie. 106 of the 109 .NSCR files are cell banks, not screens; only 3 are
// really screens. That was worth checking rather than assuming, and the check is the
// block tag inside, which is the only thing that actually decides.
//
// What is inside is not NDS OAM either. Real OAM packs a sprite into three cryptic
// 16-bit words; the port threw that out and wrote seven plain ones:
//
//   0,1  where to put it, relative to the cell's origin
//   2,3  how big it is
//   4,5  where to take it from, in the sheet
//   6    flags: 1 flip across, 2 flip down, 4 half size, 8 squash to 0.6 x 2/3
//
// That is read straight off the game's own draw call, which passes exactly those
// values to drawImage(x, y, w, h, sx, sy, sw, sh) - including the flips, which it
// does by moving the source corner and negating the width.

using System;
using System.Collections.Generic;
using System.IO;


namespace Crystal
{
	internal sealed class CellPart
	{
		public int X { get; set; }
		public int Y { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public int SourceX { get; set; }
		public int SourceY { get; set; }
		public int Flags { get; set; }

		public bool FlipX => (Flags & 1) != 0;
		public bool FlipY => (Flags & 2) != 0;
		public bool Half => (Flags & 4) != 0;
		public bool Squash => (Flags & 8) != 0;
	}

	internal sealed class Cell
	{
		public int Index { get; set; }
		public int Attributes { get; set; }
		public List<CellPart> Parts { get; } = new List<CellPart>();
	}

	internal sealed class CellBank
	{
		public string Name { get; set; }

		/// <summary>The sheet these parts are cut from, or null if nothing names one.</summary>
		public string Sheet { get; set; }

		/// <summary>How the sheet was found: "named in the game's source" or "same name".</summary>
		public string SheetFrom { get; set; }

		public int MappingMode { get; set; }
		public List<Cell> Cells { get; } = new List<Cell>();
	}

	internal sealed class ScreenData
	{
		public string Name { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public int ColourMode { get; set; }
		public int Format { get; set; }

		/// <summary>One entry per tile: index in the low 10 bits, then flip and palette.</summary>
		public List<ushort> Tiles { get; } = new List<ushort>();
	}

	internal sealed class AnimFrame
	{
		/// <summary>Which cell to show.</summary>
		public int Cell { get; set; }

		/// <summary>How long to show it, in 1/60ths.</summary>
		public int Hold { get; set; }
	}

	internal sealed class AnimSequence
	{
		public int Index { get; set; }
		public int LoopFrom { get; set; }

		/// <summary>0 plain, 1 with a transform, 2 with a translation.</summary>
		public int Type { get; set; }

		/// <summary>0 forward, 1 loop, 2 back and forth, 3 loop back and forth.</summary>
		public int PlayMode { get; set; }

		public List<AnimFrame> Frames { get; } = new List<AnimFrame>();
	}

	internal sealed class AnimBank
	{
		public string Name { get; set; }

		/// <summary>
		/// What the header says the frames add up to. Worth keeping: it is the bank's
		/// own account of itself, so a reader that has lost its place disagrees with it.
		/// </summary>
		public int TotalFrames { get; set; }

		public List<AnimSequence> Sequences { get; } = new List<AnimSequence>();

		public int GotFrames
		{
			get
			{
				int frames = 0;
				foreach (AnimSequence sequence in Sequences)
				{
					frames += sequence.Frames.Count;
				}
				return frames;
			}
		}
	}

	internal static class Cells
	{
		/// <summary>
		/// Where a block starts, or -1. Tags are stored back to front in the file, which
		/// is what the game compares against - "CEBK" is written "KBEC".
		/// </summary>
		public static int Block(byte[] data, string tag)
		{
			if (data == null || data.Length < 16)
			{
				return -1;
			}

			int size = (int)U32(data, 8);
			for (int at = 16; at < size && at + 8 <= data.Length;)
			{
				if (data[at] == tag[3] && data[at + 1] == tag[2]
					&& data[at + 2] == tag[1] && data[at + 3] == tag[0])
				{
					return at + 8;
				}

				int length = (int)U32(data, at + 4);
				if (length <= 0)
				{
					break;
				}
				at += length;
			}
			return -1;
		}

		public static bool IsCellBank(byte[] data) => Block(data, "CEBK") >= 0;

		public static bool IsScreen(byte[] data) => Block(data, "SCRN") >= 0;

		/// <summary>
		/// The cells. Every cell header is read first and every part list after, in two
		/// passes - which is not an optimisation, it is the layout: the parts follow all
		/// of the headers rather than each one following its own.
		/// </summary>
		public static CellBank ReadCellBank(byte[] data, string name,
			Func<string, bool> exists = null)
		{
			int at = Block(data, "CEBK");
			if (at < 0)
			{
				throw new InvalidDataException("no CEBK block in " + name);
			}

			CellBank bank = new CellBank { Name = name };
			int count = U16(data, at);
			bank.MappingMode = (int)U32(data, at + 8);

			int p = at + 24;
			int[] parts = new int[count];
			for (int i = 0; i < count; i++)
			{
				parts[i] = U16(data, p);
				bank.Cells.Add(new Cell { Index = i, Attributes = U16(data, p + 2) });
				p += 8;
			}

			for (int i = 0; i < count; i++)
			{
				for (int k = 0; k < parts[i]; k++)
				{
					bank.Cells[i].Parts.Add(new CellPart
					{
						X = S16(data, p),
						Y = S16(data, p + 2),
						Width = S16(data, p + 4),
						Height = S16(data, p + 6),
						SourceX = S16(data, p + 8),
						SourceY = S16(data, p + 10),
						Flags = S16(data, p + 12)
					});
					p += 14;
				}
			}

			Pair(bank, exists);
			return bank;
		}

		/// <summary>
		/// Which sheet the bank cuts from. The game names it at the call site, and those
		/// are in the generated table; everything else uses the sheet of the same name,
		/// which is what the call sites do anyway for all but a handful.
		///
		/// Same name is not quite the same extension: most sheets are .NCGR but 33 are
		/// .NCBR, so both are offered and the caller - which is the only thing that
		/// knows what is on disk - says which one is there.
		/// </summary>
		private static void Pair(CellBank bank, Func<string, bool> exists)
		{
			string file = Path.GetFileName(bank.Name);
			if (CellPairs.Sheet.TryGetValue(file, out string sheet))
			{
				bank.Sheet = sheet;
				bank.SheetFrom = "named in the game's source";
				return;
			}

			string stem = Path.GetFileNameWithoutExtension(file);
			bank.SheetFrom = "the sheet of the same name";
			bank.Sheet = stem + ".NCGR";
			if (exists != null && !exists(bank.Sheet) && exists(stem + ".NCBR"))
			{
				bank.Sheet = stem + ".NCBR";
			}
		}

		public static ScreenData ReadScreen(byte[] data, string name)
		{
			int at = Block(data, "SCRN");
			if (at < 0)
			{
				throw new InvalidDataException("no SCRN block in " + name);
			}

			ScreenData screen = new ScreenData
			{
				Name = name,
				Width = U16(data, at),
				Height = U16(data, at + 2),
				ColourMode = U16(data, at + 4),
				Format = U16(data, at + 6)
			};

			int bytes = (int)U32(data, at + 8);
			int p = at + 12;
			for (int i = 0; i + 1 < bytes && p + 1 < data.Length; i += 2, p += 2)
			{
				screen.Tiles.Add(U16(data, p));
			}
			return screen;
		}

		public static bool IsAnimation(byte[] data) => Block(data, "ABNK") >= 0;

		/// <summary>
		/// The animation sequences. Laid out in three passes like the cells: every
		/// sequence header, then every frame header, then every frame's content - so a
		/// reader that tries to take one sequence at a time reads the wrong bytes.
		/// </summary>
		public static AnimBank ReadAnimation(byte[] data, string name)
		{
			int at = Block(data, "ABNK");
			if (at < 0)
			{
				throw new InvalidDataException("no ABNK block in " + name);
			}

			AnimBank bank = new AnimBank { Name = name, TotalFrames = U16(data, at + 2) };
			int count = U16(data, at);
			int p = at + 24;

			int[] frameCount = new int[count];
			for (int i = 0; i < count; i++)
			{
				frameCount[i] = U16(data, p);
				bank.Sequences.Add(new AnimSequence
				{
					Index = i,
					LoopFrom = U16(data, p + 2),
					Type = (int)U32(data, p + 4),
					PlayMode = (int)U32(data, p + 8)
				});
				p += 16;
			}

			// The hold times come before any of the cell numbers do, so they are kept
			// until the second pass rather than read alongside.
			List<int[]> holds = new List<int[]>(count);
			for (int i = 0; i < count; i++)
			{
				int[] each = new int[frameCount[i]];
				for (int k = 0; k < each.Length; k++)
				{
					each[k] = U16(data, p + 4);
					p += 8;
				}
				holds.Add(each);
			}

			for (int i = 0; i < bank.Sequences.Count; i++)
			{
				foreach (int hold in holds[i])
				{
					bank.Sequences[i].Frames.Add(new AnimFrame
					{
						Cell = U16(data, p),
						Hold = hold
					});
					p += 8;
				}
			}

			return bank;
		}

		private static ushort U16(byte[] d, int a) => (ushort)(d[a] | (d[a + 1] << 8));
		private static short S16(byte[] d, int a) => (short)(d[a] | (d[a + 1] << 8));
		private static uint U32(byte[] d, int a) =>
			(uint)(d[a] | (d[a + 1] << 8) | (d[a + 2] << 16) | (d[a + 3] << 24));
	}
}
