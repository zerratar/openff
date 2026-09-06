// The 2D cell banks (NCER) and screens (NSCR) of the phone and Steam builds of FF3 and FF4,
// read for drawing: which rectangle of a sheet goes where.
//
// The files keep the NDS block layout - a 16-byte header, then blocks tagged back to front
// ("CEBK" is written "KBEC", "SCRN" "NRCS") - but not the NDS OAM inside. The port rewrote
// each part as seven plain 16-bit words, read straight off the game's own draw call:
//
//   0,1  where to put it, relative to the cell's origin
//   2,3  how big it is
//   4,5  where to take it from, in the sheet
//   6    flags: 1 flip across, 2 flip down, 4 half size, 8 squash to 0.6 x 2/3
//
// Every cell header comes first (parts count, attributes, 8 bytes each) and every part
// list after, in that order. Crystal's Crystal.Editor/Cells.cs reads the same files with
// more bookkeeping (sheet pairing, screens, animation); this is the reader the client draws
// from - FF4's window frames, glove cursor and gauges under the OpenFF battle and menus.

using System;
using System.Collections.Generic;

namespace FF3.Content
{
	public sealed class CellPart
	{
		public int X, Y, Width, Height, SourceX, SourceY, Flags;
		public bool FlipX => (Flags & 1) != 0;
		public bool FlipY => (Flags & 2) != 0;
	}

	public sealed class Cell
	{
		public int Index;
		public int Attributes;
		public List<CellPart> Parts = new List<CellPart>();
	}

	public sealed class CellBank
	{
		public string Name;
		public int MappingMode;
		public List<Cell> Cells = new List<Cell>();

		public Cell this[int index] => index >= 0 && index < Cells.Count ? Cells[index] : null;
	}

	public static class CellBanks
	{
		/// <summary>Where a block's payload starts, or -1; tags are stored back to front.</summary>
		public static int Block(byte[] data, string tag)
		{
			if (data == null || data.Length < 16 || tag == null || tag.Length != 4) return -1;
			int size = (int)BitConverter.ToUInt32(data, 8);
			for (int at = 16; at < size && at + 8 <= data.Length;)
			{
				if (data[at] == tag[3] && data[at + 1] == tag[2] && data[at + 2] == tag[1] && data[at + 3] == tag[0]) return at + 8;
				int length = (int)BitConverter.ToUInt32(data, at + 4);
				if (length <= 0) break;
				at += length;
			}
			return -1;
		}

		public static bool IsCellBank(byte[] data) => Block(data, "CEBK") >= 0;

		/// <summary>The cells of an NCER (or one of the NSCR files that are cell banks under another name); null when the data is not one.</summary>
		public static CellBank Read(byte[] data, string name = null)
		{
			int at = Block(data, "CEBK");
			if (at < 0) return null;
			CellBank bank = new CellBank { Name = name };
			int count = BitConverter.ToUInt16(data, at);
			bank.MappingMode = (int)BitConverter.ToUInt32(data, at + 8);
			int p = at + 24;
			int[] parts = new int[count];
			for (int i = 0; i < count; i++)
			{
				if (p + 8 > data.Length) break;
				parts[i] = BitConverter.ToUInt16(data, p);
				bank.Cells.Add(new Cell { Index = i, Attributes = BitConverter.ToUInt16(data, p + 2) });
				p += 8;
			}
			for (int i = 0; i < bank.Cells.Count; i++)
			{
				for (int k = 0; k < parts[i] && p + 14 <= data.Length; k++)
				{
					bank.Cells[i].Parts.Add(new CellPart
					{
						X = BitConverter.ToInt16(data, p),
						Y = BitConverter.ToInt16(data, p + 2),
						Width = BitConverter.ToInt16(data, p + 4),
						Height = BitConverter.ToInt16(data, p + 6),
						SourceX = BitConverter.ToInt16(data, p + 8),
						SourceY = BitConverter.ToInt16(data, p + 10),
						Flags = BitConverter.ToInt16(data, p + 12),
					});
					p += 14;
				}
			}
			return bank;
		}
	}
}
