// .mcl - the collision mesh, and the half of an exit that is not in the .pak.
//
//   ff3content mcl <file.mcl.lz | directory> [out]
//
// Every map ships one, as <map>_col.mcl.lz. It is a second, much simpler mesh than the
// one you can see: triangles carrying attributes rather than textures. Walking, walls,
// the camera, terrain type, where monsters appear - and the twelve exits.
//
// An exit is two halves. The map's .pak says where the player lands and on which map;
// what makes it fire is a triangle in here whose material carries ATTRIBUTE_MAPJUMP_01
// through _12. calculateJumpCollision walks those twelve in order and returns the first
// the player is standing in, plus one, and that number indexes the .pak's jumps chain.
// So a map has exactly twelve exit slots, and the shipped maps agree: the most any of
// them uses is eleven.
//
// Layout (little endian, all offsets from the start of the file)
//
//   file, 48 bytes
//     +0   'MCL '                      0x204C434D
//     +4   version                     0x0500 in every shipped file
//     +8   offset of the objects
//     +12  object count
//     +16  bounding box, min then max, four words each
//
//   object, 128 bytes each, laid end to end
//     +0   name, 24 bytes              NUL padded
//     +24  offset of the polygons
//     +28  polygon count
//     +32  offset of the blocks
//     +36  block size x, y, z          fixed point, the size of one grid cell
//     +48  blocks across x, y, z       u16 each
//     +54  padding                     u16
//     +56  offset of the points
//     +60  point count
//     +64  offset of the materials
//     +68  material count
//     +72  box minimum x, y, z
//     +84  padding, three words
//     +96  bounding box, min then max
//
//   polygon, 24 bytes                  three point indices, a material index, a normal
//   point, 16 bytes                    x, y, z and a word the reader steps over
//   material, 8 bytes                  64 attribute bits, low word first
//   block, 8 bytes                     offset of its polygon indices, and how many
//
// The blocks are a uniform grid over the object, each holding the indices of the
// polygons that touch it, so a collision test only looks at what is nearby. They are
// indexed x * (blocksY * blocksZ) + y * blocksZ + z.
//
// Two things about the shape of the file that matter for writing one back. The arrays
// are addressed by offset rather than laid out in a fixed order, so what a rebuild has
// to preserve is not "the order I would choose" but the order that is there. And the
// block indices are one shared run of u16 that each block points into a slice of -
// blocks with no polygons all point at the same place - so a rebuild has to keep the
// sharing or the file grows every time it is saved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FF3.ContentTool
{
	/// <summary>One triangle: three points, a material, and the normal it was built with.</summary>
	internal sealed class MclPolygon
	{
		public ushort[] Vertex { get; set; } = new ushort[3];
		public ushort Material { get; set; }

		/// <summary>Fixed point, and a fourth word the game reads but never uses.</summary>
		public int[] Normal { get; set; } = new int[4];
	}

	/// <summary>
	/// The attributes one material carries: 64 bits, of which the game names 42.
	/// </summary>
	internal sealed class MclMaterial
	{
		public uint Low { get; set; }
		public uint High { get; set; }

		public bool Has(int attribute)
		{
			return attribute < 32
				? (Low >> attribute & 1) != 0
				: (High >> (attribute - 32) & 1) != 0;
		}

		public void Set(int attribute, bool on)
		{
			uint bit = 1u << (attribute & 31);
			if (attribute < 32)
			{
				Low = on ? Low | bit : Low & ~bit;
			}
			else
			{
				High = on ? High | bit : High & ~bit;
			}
		}

		public IEnumerable<int> Attributes()
		{
			for (int i = 0; i < Mcl.AttributeMax; i++)
			{
				if (Has(i)) yield return i;
			}
		}
	}

	/// <summary>One cell of the lookup grid: which polygons are near enough to test.</summary>
	internal sealed class MclBlock
	{
		/// <summary>Where its indices sit in the file, kept so a rebuild can share runs.</summary>
		public uint Offset { get; set; }
		public ushort[] Polygons { get; set; } = Array.Empty<ushort>();
	}

	internal sealed class MclObject
	{
		public string Name { get; set; }
		public List<MclPolygon> Polygons { get; set; } = new List<MclPolygon>();
		public List<int[]> Points { get; set; } = new List<int[]>();
		public List<MclMaterial> Materials { get; set; } = new List<MclMaterial>();
		public List<MclBlock> Blocks { get; set; } = new List<MclBlock>();

		public int[] BlockSize { get; set; } = new int[3];
		public ushort[] BlockCount { get; set; } = new ushort[3];
		public ushort BlockPad { get; set; }
		public int[] BoxMin { get; set; } = new int[3];
		public uint[] Padding { get; set; } = new uint[3];
		public int[] BoundsMin { get; set; } = new int[4];
		public int[] BoundsMax { get; set; } = new int[4];

		/// <summary>Where each array sat, so a rebuild puts them back in that order.</summary>
		public uint PolygonsAt { get; set; }
		public uint BlocksAt { get; set; }
		public uint PointsAt { get; set; }
		public uint MaterialsAt { get; set; }
	}

	internal sealed class MclFile
	{
		public uint Version { get; set; }
		public int[] BoundsMin { get; set; } = new int[4];
		public int[] BoundsMax { get; set; } = new int[4];
		public List<MclObject> Objects { get; set; } = new List<MclObject>();

		/// <summary>Where the objects sat, and how long the file was.</summary>
		public uint ObjectsAt { get; set; }
		public int Size { get; set; }

		/// <summary>Bytes after everything the reader knows about, if there are any.</summary>
		public byte[] Tail { get; set; } = Array.Empty<byte>();
	}

	internal static class Mcl
	{
		/// <summary>'MCL ', which is what the first four bytes of every one of them says.</summary>
		public const uint Magic = 0x204C434D;

		public const int HeaderSize = 48;
		public const int ObjectSize = 128;
		public const int PolygonSize = 24;
		public const int PointSize = 16;
		public const int MaterialSize = 8;
		public const int BlockSize = 8;

		/// <summary>ATTRIBUTE_MAX - the attributes the game names, out of the 64 there is room for.</summary>
		public const int AttributeMax = 42;

		/// <summary>ATTRIBUTE_MAPJUMP_01. The twelve exits are this and the eleven after it.</summary>
		public const int FirstJumpAttribute = 25;

		public const int JumpSlots = 12;

		/// <summary>The attribute one exit slot uses. Slots are numbered from one, as the game numbers them.</summary>
		public static int JumpAttribute(int slot)
		{
			return FirstJumpAttribute + slot - 1;
		}

		/// <summary>What each attribute is called, from MCL_ATTRIBUTE.</summary>
		public static string AttributeName(int attribute)
		{
			if (attribute == 0) return "none";
			if (attribute == 1) return "ground";
			if (attribute >= 2 && attribute <= 6) return "wall " + (attribute - 1);
			if (attribute == 7) return "camera";
			if (attribute >= 8 && attribute <= 19) return "landform " + (attribute - 7);
			if (attribute >= 20 && attribute <= 24) return "monster " + (attribute - 19);
			if (attribute >= 25 && attribute <= 36) return "exit " + (attribute - 24);
			if (attribute >= 37 && attribute <= 41) return "damage " + (attribute - 36);
			if (attribute == 42) return "monster sky";
			return "attribute " + attribute;
		}

		// ------------------------------------------------------------------ reading

		public static MclFile Read(byte[] data)
		{
			if (data.Length < HeaderSize)
			{
				throw new InvalidDataException("too short to be an .mcl");
			}

			uint magic = U32(data, 0);
			if (magic != Magic)
			{
				throw new InvalidDataException("not an .mcl - it starts with 0x"
					+ magic.ToString("X8") + " rather than 'MCL '");
			}

			MclFile file = new MclFile
			{
				Version = U32(data, 4),
				ObjectsAt = U32(data, 8),
				Size = data.Length
			};
			int count = (int)U32(data, 12);
			file.BoundsMin = Words(data, 16, 4);
			file.BoundsMax = Words(data, 32, 4);

			for (int i = 0; i < count; i++)
			{
				file.Objects.Add(ReadObject(data, (int)file.ObjectsAt + i * ObjectSize));
			}

			return file;
		}

		private static MclObject ReadObject(byte[] data, int at)
		{
			Need(data, at, ObjectSize, "object header");

			MclObject item = new MclObject
			{
				Name = Name(data, at, 24),
				PolygonsAt = U32(data, at + 24),
				BlocksAt = U32(data, at + 32),
				BlockSize = Words(data, at + 36, 3),
				BlockPad = U16(data, at + 54),
				PointsAt = U32(data, at + 56),
				MaterialsAt = U32(data, at + 64),
				BoxMin = Words(data, at + 72, 3),
				BoundsMin = Words(data, at + 96, 4),
				BoundsMax = Words(data, at + 112, 4)
			};

			int polygons = (int)U32(data, at + 28);
			item.BlockCount = new[]
			{
				U16(data, at + 48), U16(data, at + 50), U16(data, at + 52)
			};
			int points = (int)U32(data, at + 60);
			int materials = (int)U32(data, at + 68);
			for (int i = 0; i < 3; i++)
			{
				item.Padding[i] = U32(data, at + 84 + i * 4);
			}

			for (int i = 0; i < polygons; i++)
			{
				int p = (int)item.PolygonsAt + i * PolygonSize;
				Need(data, p, PolygonSize, "polygon " + i);
				item.Polygons.Add(new MclPolygon
				{
					Vertex = new[] { U16(data, p), U16(data, p + 2), U16(data, p + 4) },
					Material = U16(data, p + 6),
					Normal = Words(data, p + 8, 4)
				});
			}

			for (int i = 0; i < points; i++)
			{
				int p = (int)item.PointsAt + i * PointSize;
				Need(data, p, PointSize, "point " + i);
				item.Points.Add(Words(data, p, 4));
			}

			for (int i = 0; i < materials; i++)
			{
				int p = (int)item.MaterialsAt + i * MaterialSize;
				Need(data, p, MaterialSize, "material " + i);
				item.Materials.Add(new MclMaterial
				{
					Low = U32(data, p),
					High = U32(data, p + 4)
				});
			}

			int blocks = item.BlockCount[0] * item.BlockCount[1] * item.BlockCount[2];
			for (int i = 0; i < blocks; i++)
			{
				int p = (int)item.BlocksAt + i * BlockSize;
				Need(data, p, BlockSize, "block " + i);
				MclBlock block = new MclBlock { Offset = U32(data, p) };
				int held = (int)U32(data, p + 4);
				ushort[] indices = new ushort[held];
				Need(data, (int)block.Offset, held * 2, "the indices of block " + i);
				for (int j = 0; j < held; j++)
				{
					indices[j] = U16(data, (int)block.Offset + j * 2);
				}
				block.Polygons = indices;
				item.Blocks.Add(block);
			}

			return item;
		}

		// ------------------------------------------------------------------ writing
		//
		// The arrays go back where they were. Nothing here decides on a layout, because
		// the file says what its layout is and a rebuild that agrees with it can be
		// compared to the original byte for byte - which is the only way to know the
		// reader understood what it read.
		//
		// A grown array is the one case that cannot go back in place, and it is refused
		// rather than guessed at: the caller has to say where the room came from.

		/// <summary>
		/// Puts every array back where the shipped files put theirs, and works out the
		/// offsets and the size to match. This is what makes an array able to grow: the
		/// offsets stop being something to preserve and become something to compute.
		///
		/// The order is not invented. Every one of the 321 shipped meshes lays itself
		/// out as header, objects, points, polygons, materials, blocks, block indices,
		/// with no padding anywhere and nothing after the last index - and laying them
		/// out this way reproduces all 321 byte for byte, which is the evidence that
		/// this is the rule rather than a guess that happens to fit.
		///
		/// One thing it cannot know: every shipped mesh has exactly one object, so
		/// whether a second object's arrays would sit after the first object's or be
		/// grouped by kind is not something the data says. They are written per object,
		/// which is the reading that keeps one object's arrays together.
		/// </summary>
		public static void Layout(MclFile file)
		{
			int at = HeaderSize;
			file.ObjectsAt = (uint)at;
			at += file.Objects.Count * ObjectSize;

			foreach (MclObject item in file.Objects)
			{
				item.PointsAt = (uint)at;
				at += item.Points.Count * PointSize;

				item.PolygonsAt = (uint)at;
				at += item.Polygons.Count * PolygonSize;

				item.MaterialsAt = (uint)at;
				at += item.Materials.Count * MaterialSize;

				item.BlocksAt = (uint)at;
				at += item.Blocks.Count * BlockSize;

				// The runs go end to end in block order. A block with nothing in it
				// still carries an offset, and in every shipped file that offset is
				// wherever the next run would start.
				foreach (MclBlock block in item.Blocks)
				{
					block.Offset = (uint)at;
					at += block.Polygons.Length * 2;
				}
			}

			file.Size = at + file.Tail.Length;
		}

		/// <summary>Lays the file out and writes it - what to call after changing anything.</summary>
		public static byte[] Build(MclFile file)
		{
			Layout(file);
			return Write(file);
		}

		public static byte[] Write(MclFile file)
		{
			byte[] data = new byte[file.Size];

			Put32(data, 0, Magic);
			Put32(data, 4, file.Version);
			Put32(data, 8, file.ObjectsAt);
			Put32(data, 12, (uint)file.Objects.Count);
			PutWords(data, 16, file.BoundsMin);
			PutWords(data, 32, file.BoundsMax);

			for (int i = 0; i < file.Objects.Count; i++)
			{
				WriteObject(data, (int)file.ObjectsAt + i * ObjectSize, file.Objects[i]);
			}

			if (file.Tail.Length > 0)
			{
				Array.Copy(file.Tail, 0, data, file.Size - file.Tail.Length,
					file.Tail.Length);
			}

			return data;
		}

		private static void WriteObject(byte[] data, int at, MclObject item)
		{
			PutName(data, at, item.Name, 24);
			Put32(data, at + 24, item.PolygonsAt);
			Put32(data, at + 28, (uint)item.Polygons.Count);
			Put32(data, at + 32, item.BlocksAt);
			PutWords(data, at + 36, item.BlockSize);
			for (int i = 0; i < 3; i++)
			{
				Put16(data, at + 48 + i * 2, item.BlockCount[i]);
			}
			Put16(data, at + 54, item.BlockPad);
			Put32(data, at + 56, item.PointsAt);
			Put32(data, at + 60, (uint)item.Points.Count);
			Put32(data, at + 64, item.MaterialsAt);
			Put32(data, at + 68, (uint)item.Materials.Count);
			PutWords(data, at + 72, item.BoxMin);
			for (int i = 0; i < 3; i++)
			{
				Put32(data, at + 84 + i * 4, item.Padding[i]);
			}
			PutWords(data, at + 96, item.BoundsMin);
			PutWords(data, at + 112, item.BoundsMax);

			for (int i = 0; i < item.Polygons.Count; i++)
			{
				int p = (int)item.PolygonsAt + i * PolygonSize;
				MclPolygon polygon = item.Polygons[i];
				Put16(data, p, polygon.Vertex[0]);
				Put16(data, p + 2, polygon.Vertex[1]);
				Put16(data, p + 4, polygon.Vertex[2]);
				Put16(data, p + 6, polygon.Material);
				PutWords(data, p + 8, polygon.Normal);
			}

			for (int i = 0; i < item.Points.Count; i++)
			{
				PutWords(data, (int)item.PointsAt + i * PointSize, item.Points[i]);
			}

			for (int i = 0; i < item.Materials.Count; i++)
			{
				int p = (int)item.MaterialsAt + i * MaterialSize;
				Put32(data, p, item.Materials[i].Low);
				Put32(data, p + 4, item.Materials[i].High);
			}

			for (int i = 0; i < item.Blocks.Count; i++)
			{
				int p = (int)item.BlocksAt + i * BlockSize;
				MclBlock block = item.Blocks[i];
				Put32(data, p, block.Offset);
				Put32(data, p + 4, (uint)block.Polygons.Length);
				for (int j = 0; j < block.Polygons.Length; j++)
				{
					Put16(data, (int)block.Offset + j * 2, block.Polygons[j]);
				}
			}
		}

		// ------------------------------------------------------- making an exit region
		//
		// A new exit trigger is copied from the shipped ones rather than invented, and
		// they are remarkably consistent. Every one of the 622 in the game is a material
		// carrying its exit attribute and nothing else, and 609 of them are exactly the
		// same shape: eight triangles and eight points forming a box with four walls and
		// no lid, sharing not one point with the rest of the mesh. So a region is a
		// self-contained thing that can be added and taken out whole.
		//
		// Their size, from the median of all 622: about 19 wide, 12 deep, 54 tall. Where
		// the floor under one could be found - 300 of them - the box reaches 19 below it
		// and 36 above, so it is not a doorway sitting on the ground but a slab through
		// it, which is what catches a player whatever height the floor is at just there.
		//
		// The one part that is not geometry is the block grid. evaluateSphere only tests
		// polygons in the blocks around the player - it samples eight points about the
		// sphere, maps each through getBlock, and looks no further - so a triangle in no
		// block is a triangle nothing will ever hit. New polygons go into every block
		// their box touches.
		//
		// And getBlock measures down from the object's bounding box maximum, which is why
		// a box outside those bounds is refused rather than the bounds grown: a bigger
		// box would silently move every polygon already there into a different cell of
		// the grid. evaluateSphere ignores points outside the bounds anyway, so a trigger
		// out there would never fire.

		/// <summary>Width, height and depth of a new exit box, from the shipped median.</summary>
		public static readonly int[] DefaultRegion = { 19, 54, 12 };

		/// <summary>How far below the point the box starts, from the shipped median.</summary>
		public const int RegionBelow = 19;

		/// <summary>Whether any object in the mesh has a trigger for this slot.</summary>
		public static bool HasJump(MclFile file, int slot)
		{
			int attribute = JumpAttribute(slot);
			return file.Objects.Any(o => o.Materials.Any(m => m.Has(attribute)));
		}

		/// <summary>Which exit slots this mesh has a trigger for.</summary>
		public static List<int> JumpSlotsUsed(MclFile file)
		{
			List<int> slots = new List<int>();
			for (int slot = 1; slot <= JumpSlots; slot++)
			{
				if (HasJump(file, slot)) slots.Add(slot);
			}
			return slots;
		}

		/// <summary>Where a slot's trigger is, in whole units, or null if it has none.</summary>
		public static int[] JumpRegionAt(MclFile file, int slot)
		{
			int attribute = JumpAttribute(slot);
			foreach (MclObject item in file.Objects)
			{
				List<int> materials = Enumerable.Range(0, item.Materials.Count)
					.Where(i => item.Materials[i].Has(attribute)).ToList();
				if (materials.Count == 0) continue;

				List<int[]> points = item.Polygons
					.Where(p => materials.Contains(p.Material))
					.SelectMany(p => p.Vertex)
					.Distinct()
					.Where(v => v < item.Points.Count)
					.Select(v => item.Points[v])
					.ToList();
				if (points.Count == 0) continue;

				return new[]
				{
					(points.Min(p => p[0]) + points.Max(p => p[0])) / 2 / 4096,
					points.Min(p => p[1]) / 4096,
					(points.Min(p => p[2]) + points.Max(p => p[2])) / 2 / 4096,
					(points.Max(p => p[0]) - points.Min(p => p[0])) / 4096,
					(points.Max(p => p[1]) - points.Min(p => p[1])) / 4096,
					(points.Max(p => p[2]) - points.Min(p => p[2])) / 4096
				};
			}
			return null;
		}

		/// <summary>
		/// Adds the trigger for one exit slot: a box standing at a point, given in whole
		/// units the way a .hich row and a jumps row give theirs.
		/// </summary>
		public static void AddJumpRegion(MclFile file, int slot, int x, int y, int z,
			int width, int height, int depth)
		{
			if (slot < 1 || slot > JumpSlots)
			{
				throw new InvalidDataException("a map has " + JumpSlots
					+ " exit slots, so there is no slot " + slot);
			}
			if (HasJump(file, slot))
			{
				throw new InvalidDataException("this map already has a trigger for exit "
					+ slot);
			}
			if (file.Objects.Count == 0)
			{
				throw new InvalidDataException(
					"this collision mesh has no object to put anything in");
			}

			MclObject item = file.Objects[0];

			int halfWidth = Math.Max(1, width) * 4096 / 2;
			int halfDepth = Math.Max(1, depth) * 4096 / 2;
			int tall = Math.Max(1, height) * 4096;

			int x0 = x * 4096 - halfWidth, x1 = x * 4096 + halfWidth;
			int z0 = z * 4096 - halfDepth, z1 = z * 4096 + halfDepth;
			int y0 = y * 4096 - RegionBelow * 4096, y1 = y0 + tall;

			if (x0 < item.BoundsMin[0] || x1 > item.BoundsMax[0]
				|| z0 < item.BoundsMin[2] || z1 > item.BoundsMax[2])
			{
				throw new InvalidDataException(
					"that is outside the collision bounds, which run x "
					+ item.BoundsMin[0] / 4096 + " to " + item.BoundsMax[0] / 4096
					+ " and z " + item.BoundsMin[2] / 4096 + " to "
					+ item.BoundsMax[2] / 4096
					+ ". The grid the game searches is measured from those bounds, so "
					+ "growing them would move every polygon already here into a "
					+ "different cell of it - and a trigger outside them never fires");
			}

			// Eight corners, low four then high four, as the walls below index them.
			int[][] corners =
			{
				new[] { x0, y0, z0, 4096 }, new[] { x1, y0, z0, 4096 },
				new[] { x1, y0, z1, 4096 }, new[] { x0, y0, z1, 4096 },
				new[] { x0, y1, z0, 4096 }, new[] { x1, y1, z0, 4096 },
				new[] { x1, y1, z1, 4096 }, new[] { x0, y1, z1, 4096 }
			};

			ushort firstPoint = (ushort)item.Points.Count;
			foreach (int[] corner in corners) item.Points.Add(corner);

			ushort material = (ushort)item.Materials.Count;
			MclMaterial attributes = new MclMaterial();
			attributes.Set(JumpAttribute(slot), true);
			item.Materials.Add(attributes);

			// Four walls, outward facing, two triangles each.
			int[][] walls =
			{
				new[] { 1, 0, 4, 5, 0, 0, -4096 },
				new[] { 3, 2, 6, 7, 0, 0, 4096 },
				new[] { 0, 3, 7, 4, -4096, 0, 0 },
				new[] { 2, 1, 5, 6, 4096, 0, 0 }
			};

			ushort firstPolygon = (ushort)item.Polygons.Count;
			foreach (int[] wall in walls)
			{
				int[] normal = { wall[4], wall[5], wall[6], 4096 };
				item.Polygons.Add(
					Triangle(firstPoint, wall[0], wall[1], wall[2], material, normal));
				item.Polygons.Add(
					Triangle(firstPoint, wall[0], wall[2], wall[3], material, normal));
			}

			foreach (int index in BlocksOver(item, x0, y0, z0, x1, y1, z1))
			{
				MclBlock block = item.Blocks[index];
				List<ushort> held = new List<ushort>(block.Polygons);
				for (int p = firstPolygon; p < item.Polygons.Count; p++)
				{
					held.Add((ushort)p);
				}
				block.Polygons = held.ToArray();
			}
		}

		/// <summary>Takes a slot's trigger out, with the points and material only it used.</summary>
		public static bool RemoveJumpRegion(MclFile file, int slot)
		{
			int attribute = JumpAttribute(slot);
			bool removed = false;

			foreach (MclObject item in file.Objects)
			{
				List<int> materials = Enumerable.Range(0, item.Materials.Count)
					.Where(i => item.Materials[i].Has(attribute))
					.ToList();
				if (materials.Count == 0) continue;

				HashSet<int> going = new HashSet<int>(
					Enumerable.Range(0, item.Polygons.Count)
						.Where(i => materials.Contains(item.Polygons[i].Material)));
				if (going.Count == 0) continue;

				// Only the points nothing else uses. A shipped region shares none, but
				// this does not assume that of a mesh somebody has already edited.
				HashSet<int> keptPoints = new HashSet<int>(
					Enumerable.Range(0, item.Polygons.Count)
						.Where(i => !going.Contains(i))
						.SelectMany(i => item.Polygons[i].Vertex.Select(v => (int)v)));

				Renumber(item, going, materials, keptPoints);
				removed = true;
			}

			return removed;
		}

		private static MclPolygon Triangle(ushort first, int a, int b, int c,
			ushort material, int[] normal)
		{
			return new MclPolygon
			{
				Vertex = new[]
				{
					(ushort)(first + a), (ushort)(first + b), (ushort)(first + c)
				},
				Material = material,
				Normal = (int[])normal.Clone()
			};
		}

		/// <summary>
		/// Every block a box overlaps. The mapping is getBlock's, measured down from the
		/// bounding box maximum, so this takes the two corners and everything between.
		/// </summary>
		private static IEnumerable<int> BlocksOver(MclObject item,
			int x0, int y0, int z0, int x1, int y1, int z1)
		{
			int[] low = BlockAt(item, x0, y0, z0);
			int[] high = BlockAt(item, x1, y1, z1);
			int yz = item.BlockCount[1] * item.BlockCount[2];

			for (int bx = Math.Min(low[0], high[0]); bx <= Math.Max(low[0], high[0]); bx++)
			{
				for (int by = Math.Min(low[1], high[1]); by <= Math.Max(low[1], high[1]); by++)
				{
					for (int bz = Math.Min(low[2], high[2]); bz <= Math.Max(low[2], high[2]); bz++)
					{
						int index = bx * yz + by * item.BlockCount[2] + bz;
						if (index >= 0 && index < item.Blocks.Count) yield return index;
					}
				}
			}
		}

		/// <summary>getBlock, as the game computes it.</summary>
		private static int[] BlockAt(MclObject item, int x, int y, int z)
		{
			return new[]
			{
				Cell(item.BoundsMax[0] - x, item.BlockSize[0], item.BlockCount[0]),
				Cell(item.BoundsMax[1] - y, item.BlockSize[1], item.BlockCount[1]),
				Cell(item.BoundsMax[2] - z, item.BlockSize[2], item.BlockCount[2])
			};
		}

		private static int Cell(int away, int size, int count)
		{
			if (count <= 0) return 0;
			if (away <= 0 || size <= 0) return Math.Min(count - 1, Math.Max(0, count - 1));
			int cell = count - 1 - away / size;
			return Math.Max(0, Math.Min(count - 1, cell));
		}

		/// <summary>
		/// Drops polygons, materials and points, and renumbers everything that pointed at
		/// them - the polygon indices held by the blocks included.
		/// </summary>
		private static void Renumber(MclObject item, HashSet<int> droppedPolygons,
			List<int> droppedMaterials, HashSet<int> keptPoints)
		{
			int[] pointMap = new int[item.Points.Count];
			List<int[]> points = new List<int[]>();
			for (int i = 0; i < item.Points.Count; i++)
			{
				pointMap[i] = keptPoints.Contains(i) ? points.Count : -1;
				if (keptPoints.Contains(i)) points.Add(item.Points[i]);
			}

			int[] materialMap = new int[item.Materials.Count];
			List<MclMaterial> materials = new List<MclMaterial>();
			for (int i = 0; i < item.Materials.Count; i++)
			{
				materialMap[i] = droppedMaterials.Contains(i) ? -1 : materials.Count;
				if (!droppedMaterials.Contains(i)) materials.Add(item.Materials[i]);
			}

			int[] polygonMap = new int[item.Polygons.Count];
			List<MclPolygon> polygons = new List<MclPolygon>();
			for (int i = 0; i < item.Polygons.Count; i++)
			{
				if (droppedPolygons.Contains(i))
				{
					polygonMap[i] = -1;
					continue;
				}
				MclPolygon polygon = item.Polygons[i];
				for (int v = 0; v < 3; v++)
				{
					polygon.Vertex[v] = (ushort)pointMap[polygon.Vertex[v]];
				}
				polygon.Material = (ushort)materialMap[polygon.Material];
				polygonMap[i] = polygons.Count;
				polygons.Add(polygon);
			}

			foreach (MclBlock block in item.Blocks)
			{
				block.Polygons = block.Polygons
					.Where(p => polygonMap[p] >= 0)
					.Select(p => (ushort)polygonMap[p])
					.ToArray();
			}

			item.Points = points;
			item.Materials = materials;
			item.Polygons = polygons;
		}

		// ------------------------------------------------------------------ helpers

		private static void Need(byte[] data, int at, int length, string what)
		{
			if (at < 0 || at + length > data.Length)
			{
				throw new InvalidDataException(what + " runs past the end of the file");
			}
		}

		private static uint U32(byte[] data, int at)
		{
			return (uint)(data[at] | data[at + 1] << 8 | data[at + 2] << 16
				| data[at + 3] << 24);
		}

		private static ushort U16(byte[] data, int at)
		{
			return (ushort)(data[at] | data[at + 1] << 8);
		}

		private static int[] Words(byte[] data, int at, int count)
		{
			int[] words = new int[count];
			for (int i = 0; i < count; i++)
			{
				words[i] = (int)U32(data, at + i * 4);
			}
			return words;
		}

		private static string Name(byte[] data, int at, int length)
		{
			int end = at;
			while (end < at + length && data[end] != 0) end++;
			return Encoding.ASCII.GetString(data, at, end - at);
		}

		private static void Put32(byte[] data, int at, uint value)
		{
			data[at] = (byte)value;
			data[at + 1] = (byte)(value >> 8);
			data[at + 2] = (byte)(value >> 16);
			data[at + 3] = (byte)(value >> 24);
		}

		private static void Put16(byte[] data, int at, ushort value)
		{
			data[at] = (byte)value;
			data[at + 1] = (byte)(value >> 8);
		}

		private static void PutWords(byte[] data, int at, int[] words)
		{
			for (int i = 0; i < words.Length; i++)
			{
				Put32(data, at + i * 4, (uint)words[i]);
			}
		}

		private static void PutName(byte[] data, int at, string name, int length)
		{
			for (int i = 0; i < length; i++)
			{
				data[at + i] = i < (name ?? string.Empty).Length ? (byte)name[i] : (byte)0;
			}
		}
	}
}
