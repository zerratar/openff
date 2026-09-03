// FF4's mass files, and a content source that reads through them.
//
// FF4 on Steam leaves most of its data loose in files/, like FF3 does, but bundles the
// per-map pieces into containers: CAST_SCRIPT.dat holds every map's script,
// CAST_HICH.dat every placement file, CAST_EVENT_MSD.dat the per-map dialogue,
// MAPPARAMETER.dat the per-map .pak, and so on - thirty of them, all the same shape:
//
//   'SSAM' | count u32 | offset0 u32 | size0 u32
//   then count records of 40 bytes: name[32] | offset u32 | size u32
//   then the data, offsets relative to the end of the directory (8 + 40 * count)
//
// The first record's offset and size sit in the header, ahead of its name; every later
// record is self-contained. A trailing few bytes past the last entry are padding.
//
// The entries are FF3's formats - the same MHCS scripts, the same hich rows, the same
// MSDA text, the same pak chains - only compressed with the same LZ and given an .lz
// suffix. So this source presents them under the names FF3 uses, files/d01_01.script
// rather than d01_01.script.lz inside CAST_SCRIPT.dat, and decompresses on the way out.
// Everything above the workspace - the map editor, the reference index, the script
// decompiler - then reads FF4 without knowing it is doing so.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FF3.Content
{
	internal sealed class SsamEntry
	{
		/// <summary>The name inside the container, "d01_01.script.lz".</summary>
		public string Name { get; set; }
		public int Offset { get; set; }
		public int Size { get; set; }
	}

	internal static class Ssam
	{
		private const int HeaderSize = 8;
		private const int RecordSize = 40;
		private const int NameSize = 32;

		/// <summary>
		/// FF4 lays its containers out two ways, and both follow one rule: the next
		/// entry starts at roundup(size + 1, stride) after the last - an entry that is
		/// already a multiple of the stride still gets a full stride of gap, so
		/// roundup(size, stride) is wrong. Nineteen containers use a stride of 32 with
		/// the first entry at offset 0; six - EFFECT, FACE, BTL_CAMERA, EVT_CAMERA,
		/// MOTION_MENU, SIGHTRO, DEBUGJUMP - use 512 with the first at 504. Measured
		/// against every entry of every one, and read off the container rather than
		/// assumed, so a container is rewritten the way it came.
		/// </summary>
		private static int Padded(int size, int stride)
		{
			return (size + 1 + stride - 1) / stride * stride;
		}

		/// <summary>The stride a container's entries follow, or 0 if neither fits.</summary>
		private static int StrideOf(List<SsamEntry> entries)
		{
			foreach (int stride in new[] { 32, 512 })
			{
				bool fits = true;
				for (int i = 0; i + 1 < entries.Count && fits; i++)
				{
					fits = entries[i + 1].Offset == entries[i].Offset + Padded(entries[i].Size, stride);
				}
				if (fits)
				{
					return stride;
				}
			}
			return 0;
		}

		/// <summary>
		/// Whether this container is laid out the way Repack lays one out: first entry at
		/// offset 0, entries in order, every gap the stride rule's. Six of FF4's thirty
		/// have a different shape - a 504 byte gap after the directory - and four carry
		/// offsets past their own end. Those are read where they can be and never
		/// written; a container this cannot reproduce byte for byte is not one to
		/// rewrite behind somebody's back.
		/// </summary>
		public static bool CanRepack(byte[] data)
		{
			List<SsamEntry> entries;
			try
			{
				entries = Read(data);
			}
			catch (InvalidDataException)
			{
				return false;
			}
			if (entries.Count == 0)
			{
				return false;
			}
			// A directory whose entries lie past the end of the file is an index left
			// over from the Android build - NAVIMAP, STAGEMNG_*, battle_map - whose
			// entries Steam ships loose. Read() already refuses those.
			return StrideOf(entries) != 0;
		}

		/// <summary>
		/// The container with the given entries' bytes replaced, everything else - the
		/// order, the names, the padding rule, whatever trails the last entry - as it
		/// was. Names not in the container are an error rather than an addition: a
		/// mass file's entry count is what its game expects to find.
		/// </summary>
		public static byte[] Repack(byte[] data, IReadOnlyDictionary<string, byte[]> replacements)
		{
			if (!CanRepack(data))
			{
				throw new InvalidDataException("this mass file is not laid out in a way that can be rewritten");
			}
			List<SsamEntry> entries = Read(data);
			HashSet<string> names = new HashSet<string>(entries.Select(e => e.Name), StringComparer.OrdinalIgnoreCase);
			foreach (string name in replacements.Keys)
			{
				if (!names.Contains(name))
				{
					throw new InvalidDataException("no entry called " + name + " in this mass file");
				}
			}

			SsamEntry last = entries[entries.Count - 1];
			byte[] tail = data.Skip(last.Offset + last.Size).ToArray();
			int stride = StrideOf(entries);

			int directoryEnd = HeaderSize + RecordSize * entries.Count;
			using MemoryStream body = new MemoryStream();
			List<(int Offset, int Size)> placed = new List<(int, int)>(entries.Count);
			// The first entry sits where it sat - 0 in one layout, 504 in the other -
			// and the bytes before it are kept, not invented.
			int lead = entries[0].Offset - directoryEnd;
			body.Write(data, directoryEnd, lead);
			foreach (SsamEntry entry in entries)
			{
				byte[] bytes = replacements.TryGetValue(entry.Name, out byte[] replaced)
					? replaced
					: data.Skip(entry.Offset).Take(entry.Size).ToArray();
				placed.Add(((int)body.Position, bytes.Length));
				body.Write(bytes, 0, bytes.Length);
				int pad = Padded(bytes.Length, stride) - bytes.Length;
				for (int i = 0; i < pad; i++)
				{
					body.WriteByte(0);
				}
			}
			// The original does not pad after its last entry the way the stride would;
			// it carries whatever bytes it carries. Those go back as they were.
			body.SetLength(placed[placed.Count - 1].Offset + placed[placed.Count - 1].Size);
			body.Write(tail, 0, tail.Length);

			byte[] result = new byte[directoryEnd + body.Length];
			Array.Copy(data, 0, result, 0, HeaderSize);
			for (int i = 0; i < entries.Count; i++)
			{
				int at = HeaderSize + RecordSize * i;
				BitConverter.GetBytes((uint)placed[i].Offset).CopyTo(result, at);
				BitConverter.GetBytes((uint)placed[i].Size).CopyTo(result, at + 4);
				Array.Copy(data, at + 8, result, at + 8, NameSize);
			}
			body.Position = 0;
			body.Read(result, directoryEnd, (int)body.Length);
			return result;
		}

		public static bool Looks(byte[] data)
		{
			return data != null && data.Length >= 16
				&& data[0] == (byte)'S' && data[1] == (byte)'S'
				&& data[2] == (byte)'A' && data[3] == (byte)'M';
		}

		/// <summary>The directory, with offsets made absolute.</summary>
		public static List<SsamEntry> Read(byte[] data)
		{
			if (!Looks(data))
			{
				throw new InvalidDataException("not an SSAM mass file");
			}
			int count = (int)BitConverter.ToUInt32(data, 4);
			int directoryEnd = HeaderSize + RecordSize * count;
			if (count < 0 || count > 100000 || directoryEnd > data.Length)
			{
				throw new InvalidDataException("mass file entry count is not plausible: " + count);
			}

			List<SsamEntry> entries = new List<SsamEntry>(count);
			for (int i = 0; i < count; i++)
			{
				int at = HeaderSize + RecordSize * i;
				int offset = (int)BitConverter.ToUInt32(data, at);
				int size = (int)BitConverter.ToUInt32(data, at + 4);
				int nameAt = at + 8;
				int end = Array.IndexOf(data, (byte)0, nameAt, NameSize);
				string name = Encoding.ASCII.GetString(data, nameAt,
					(end < 0 ? nameAt + NameSize : end) - nameAt);
				int absolute = directoryEnd + offset;
				if (absolute < directoryEnd || absolute + size > data.Length)
				{
					throw new InvalidDataException("entry " + name + " points outside the mass file");
				}
				entries.Add(new SsamEntry { Name = name, Offset = absolute, Size = size });
			}
			return entries;
		}
	}

	/// <summary>
	/// A loose install that also has mass files - FF4 on Steam. Loose files first, the
	/// containers behind them, all under one namespace.
	/// </summary>
	internal sealed class SsamContentSource : IContentSource
	{
		private readonly LooseContentSource _loose;

		// name as the workspace asks for it -> where it is
		private readonly Dictionary<string, (string Container, SsamEntry Entry, bool Compressed)> _packed
			= new Dictionary<string, (string, SsamEntry, bool)>(StringComparer.OrdinalIgnoreCase);

		private readonly Dictionary<string, byte[]> _containers
			= new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

		public SsamContentSource(string root)
		{
			_loose = new LooseContentSource(root);
			root = _loose.Root;

			string files = Path.Combine(root, "files");
			foreach (string path in Directory.EnumerateFiles(files, "*.dat"))
			{
				byte[] head = new byte[16];
				using (FileStream stream = File.OpenRead(path))
				{
					if (stream.Read(head, 0, 16) < 16 || !Ssam.Looks(head))
					{
						continue;
					}
				}

				byte[] data = File.ReadAllBytes(path);
				List<SsamEntry> entries;
				try
				{
					entries = Ssam.Read(data);
				}
				catch (InvalidDataException)
				{
					continue;
				}
				// Containers are known by their content name - "files/CAST_SCRIPT.dat" -
				// the same way everything else is, so that whatever installs into one
				// can find it under the install root like any other file.
				string containerName = "files/" + Path.GetFileName(path);
				_containers[containerName] = data;

				foreach (SsamEntry entry in entries)
				{
					// FF3's name for the same thing: no container, no .lz.
					bool compressed = entry.Name.EndsWith(".lz", StringComparison.OrdinalIgnoreCase);
					string exposed = "files/" + (compressed
						? entry.Name.Substring(0, entry.Name.Length - 3) : entry.Name);
					// A loose copy wins - it is what the game would pick up too - and
					// the first container to claim a name keeps it.
					if (!_packed.ContainsKey(exposed))
					{
						_packed[exposed] = (containerName, entry, compressed);
					}
				}
			}
		}

		public string Kind => "loose files + mass files";

		public int Count => _loose.Count + _packed.Keys.Count(k => !_loose.TryRead(k, out _));

		public IEnumerable<string> Names
		{
			get
			{
				HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				foreach (string name in _loose.Names)
				{
					seen.Add(name);
					yield return name;
				}
				foreach (string name in _packed.Keys)
				{
					if (seen.Add(name))
					{
						yield return name;
					}
				}
			}
		}

		public bool TryRead(string name, out byte[] data)
		{
			if (_loose.TryRead(name, out data))
			{
				return true;
			}
			if (!_packed.TryGetValue(name, out (string Container, SsamEntry Entry, bool Compressed) where))
			{
				data = null;
				return false;
			}
			byte[] container = _containers[where.Container];
			byte[] raw = new byte[where.Entry.Size];
			Buffer.BlockCopy(container, where.Entry.Offset, raw, 0, raw.Length);
			data = where.Compressed && Lz.IsCompressed(raw) ? Lz.Decompress(raw) : raw;
			return true;
		}

		/// <summary>Whether a root has any mass file under files/.</summary>
		public static bool Looks(string root)
		{
			string files = Path.Combine(LooseContentSource.Resolve(root), "files");
			if (!Directory.Exists(files))
			{
				return false;
			}
			byte[] head = new byte[4];
			foreach (string path in Directory.EnumerateFiles(files, "*.dat"))
			{
				using FileStream stream = File.OpenRead(path);
				if (stream.Read(head, 0, 4) == 4 && head[0] == (byte)'S' && head[1] == (byte)'S'
					&& head[2] == (byte)'A' && head[3] == (byte)'M')
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// The container an exposed name lives in, for anything that needs to write it
		/// back: the container's path, the entry's own name inside it, and whether that
		/// entry is LZ compressed - so the bytes going in are compressed the same way.
		/// False for a name that is loose, which the game reads in preference anyway.
		/// </summary>
		public bool TryLocate(string name, out string container, out string entryName, out bool compressed)
		{
			if (!_loose.TryRead(name, out _)
				&& _packed.TryGetValue(name, out (string Container, SsamEntry Entry, bool Compressed) where))
			{
				container = where.Container;
				entryName = where.Entry.Name;
				compressed = where.Compressed;
				return true;
			}
			container = null;
			entryName = null;
			compressed = false;
			return false;
		}
	}
}
