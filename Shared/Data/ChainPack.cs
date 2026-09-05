// The chain pack: the container both games use for their tables (item_parameter.pak,
// player.chaindata, monster.chaindata, a map's .pak). A u32 count, then from byte 16 one
// (offset, size) pair per chain, offsets from the start of the file; the records of a chain
// follow each other at a fixed stride the reader knows.

using System;

namespace OpenFF.Data
{
	public sealed class ChainPack
	{
		public byte[] Data { get; }
		public int Count { get; }
		private readonly (int Offset, int Size)[] _chains;

		private ChainPack(byte[] data, (int, int)[] chains)
		{
			Data = data;
			Count = chains.Length;
			_chains = chains;
		}

		public static ChainPack Read(byte[] data)
		{
			if (data == null || data.Length < 16)
			{
				throw new InvalidOperationException("not a chain pack: too short");
			}
			int count = BitConverter.ToInt32(data, 0);
			if (count < 0 || count > 1024 || 16 + 8 * count > data.Length)
			{
				throw new InvalidOperationException("not a chain pack: " + count + " chains");
			}
			(int, int)[] chains = new (int, int)[count];
			for (int i = 0; i < count; i++)
			{
				int offset = BitConverter.ToInt32(data, 16 + 8 * i);
				int size = BitConverter.ToInt32(data, 20 + 8 * i);
				if (offset < 0 || size < 0 || offset + size > data.Length)
				{
					throw new InvalidOperationException("chain " + i + " runs past the file");
				}
				chains[i] = (offset, size);
			}
			return new ChainPack(data, chains);
		}

		public int Offset(int chain) => _chains[chain].Offset;
		public int Size(int chain) => _chains[chain].Size;

		/// <summary>How many whole records of <paramref name="stride"/> bytes the chain holds (Square's files are sometimes two bytes short of the last).</summary>
		public int Records(int chain, int stride) => stride <= 0 ? 0 : (Size(chain) + stride - 1) / stride == 0 ? 0 : (Size(chain) + 2) / stride;

		/// <summary>The bytes of one record, padded with zeros where the file ends early.</summary>
		public byte[] Record(int chain, int stride, int index)
		{
			byte[] record = new byte[stride];
			int at = Offset(chain) + stride * index;
			int end = Math.Min(Offset(chain) + Size(chain), Data.Length);
			int take = Math.Max(0, Math.Min(stride, end - at));
			if (take > 0) Buffer.BlockCopy(Data, at, record, 0, take);
			return record;
		}

		public static int U8(byte[] d, int at) => at < d.Length ? d[at] : 0;
		public static int S8(byte[] d, int at) => at < d.Length ? (sbyte)d[at] : 0;
		public static int U16(byte[] d, int at) => at + 2 <= d.Length ? BitConverter.ToUInt16(d, at) : 0;
		public static int S16(byte[] d, int at) => at + 2 <= d.Length ? BitConverter.ToInt16(d, at) : 0;
		public static uint U32(byte[] d, int at) => at + 4 <= d.Length ? BitConverter.ToUInt32(d, at) : 0u;
		public static int S32(byte[] d, int at) => at + 4 <= d.Length ? BitConverter.ToInt32(d, at) : 0;
	}
}
