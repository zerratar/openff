// NCAP - a pack of joint animations for one skeleton.
//
// Both games keep character motion as .ncap.lz: a small header naming how many motions
// the pack holds and their ids, then a standard Nitro BCA0 file whose one block is a
// dictionary of NNSG3dResJntAnm - one per motion, each a table of per-node scale,
// rotation and translation across its frames. Which pack a model plays is the game's
// business (monsters take b_f<family>, field characters w_<name>, events w_event<nn>),
// so the editor lists every pack and lets the person pick.
//
// The evaluator is the SBC's NODEDESC handler with the drawing taken out: for one node
// and one frame it produces the same 4x3 matrix and scale the game would have built,
// starting from the node's own base matrix so that a channel the animation marks
// "base" keeps the model's value. Ported from the decompiled game (GlobalScope.Members
// around the NNS_G3D_SBC_NODEDESC case and NNSG3dResJntAnm's parser), not guessed.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FF3.ContentTool
{
	/// <summary>One motion: per-node channels over a number of frames.</summary>
	internal sealed class JointAnimation
	{
		public string Name;
		public int NumFrame;
		public int NumNode;

		/// <summary>Where the animation record starts in the file; channel offsets are relative to it.</summary>
		public int Base;

		public NodeChannels[] Nodes;
		public short[] Rot3;
		public short[] Rot5;

		internal sealed class NodeChannels
		{
			public uint Flag;
			public uint[] Words;                 // the channel words, in the order the flags dictate
		}
	}

	internal sealed class NcapFile
	{
		public int Version;
		public uint Flag;
		public uint[] MotionIds;
		public List<JointAnimation> Motions = new List<JointAnimation>();

		/// <summary>Reads a decompressed .ncap.</summary>
		public static NcapFile Read(byte[] d)
		{
			if (d == null || d.Length < 48 || d[0] != 'N' || d[1] != 'C' || d[2] != 'A' || d[3] != 'P')
			{
				throw new InvalidDataException("not an NCAP motion pack");
			}
			NcapFile file = new NcapFile { Version = (int)U32(d, 4) };
			int info = 16;
			int count = d[info];
			file.Flag = U32(d, info + 4);
			int ofsIndices = (int)U32(d, info + 12);
			int ofsMotions = (int)U32(d, info + 16);
			file.MotionIds = new uint[count];
			for (int i = 0; i < count && ofsIndices + 4 * i + 4 <= d.Length; i++)
			{
				file.MotionIds[i] = U32(d, ofsIndices + 4 * i);
			}
			if (ofsMotions <= 0 || ofsMotions + 16 > d.Length)
			{
				return file;
			}

			// BCA0: a Nitro resource file with its blocks; the joint animations are in the first.
			int res = ofsMotions;
			int blocks = U16(d, res + 14);
			for (int b = 0; b < blocks; b++)
			{
				int block = res + (int)U32(d, res + 16 + 4 * b);
				if (block + 8 > d.Length) continue;
				string kind = Encoding.ASCII.GetString(d, block, 4);
				if (kind != "J\0AC" && kind != "JNT0" && kind.Substring(0, 1) != "J") continue;
				foreach ((string name, int offset) in Dict(d, block + 8))
				{
					file.Motions.Add(ReadAnimation(d, block + offset, name));
				}
			}
			return file;
		}

		/// <summary>A Nitro dictionary: names and 32-bit entries, offsets relative to the block.</summary>
		private static List<(string, int)> Dict(byte[] d, int at)
		{
			List<(string, int)> list = new List<(string, int)>();
			int count = d[at + 1];
			int entries = at + U16(d, at + 6);
			int unit = U16(d, entries);
			int names = entries + U16(d, entries + 2);
			int values = entries + 4;
			for (int i = 0; i < count; i++)
			{
				int value = (int)U32(d, values + unit * i);
				int stop = 0;
				while (stop < 16 && d[names + 16 * i + stop] != 0) stop++;
				list.Add((Encoding.ASCII.GetString(d, names + 16 * i, stop), value));
			}
			return list;
		}

		private static JointAnimation ReadAnimation(byte[] d, int at, string name)
		{
			JointAnimation anim = new JointAnimation
			{
				Name = name,
				Base = at,
				NumFrame = U16(d, at + 4),
				NumNode = U16(d, at + 6)
			};
			int ofsRot3 = (int)U32(d, at + 12);
			int ofsRot5 = (int)U32(d, at + 16);
			anim.Nodes = new JointAnimation.NodeChannels[anim.NumNode];
			int firstTable = int.MaxValue;
			for (int i = 0; i < anim.NumNode; i++)
			{
				int tag = at + U16(d, at + 20 + 2 * i);
				uint flag = U32(d, tag);
				List<uint> words = new List<uint>();
				int p = tag + 4;
				if ((flag & 1) == 0)
				{
					if ((flag & 2) == 0 && (flag & 4) == 0)          // translation
					{
						for (int k = 0; k < 3; k++)
						{
							if ((flag & (8u << k)) != 0) { words.Add(U32(d, p)); p += 4; }
							else { words.Add(U32(d, p)); words.Add(U32(d, p + 4)); firstTable = Math.Min(firstTable, at + (int)U32(d, p + 4)); p += 8; }
						}
					}
					if ((flag & 0x40) == 0 && (flag & 0x80) == 0)   // rotation
					{
						if ((flag & 0x100) != 0) { words.Add((uint)U16(d, p)); p += 4; }
						else { words.Add(U32(d, p)); words.Add(U32(d, p + 4)); firstTable = Math.Min(firstTable, at + (int)U32(d, p + 4)); p += 8; }
					}
					if ((flag & 0x200) == 0 && (flag & 0x400) == 0) // scale
					{
						for (int k = 0; k < 3; k++)
						{
							if ((flag & (2048u << k)) != 0) { words.Add(U32(d, p)); words.Add(U32(d, p + 4)); p += 8; }
							else { words.Add(U32(d, p)); words.Add(U32(d, p + 4)); firstTable = Math.Min(firstTable, at + (int)U32(d, p + 4)); p += 8; }
						}
					}
				}
				anim.Nodes[i] = new JointAnimation.NodeChannels { Flag = flag, Words = words.ToArray() };
			}
			// The rotation tables: rot3 runs from its offset to rot5's; rot5 to the first
			// per-node table (or the end of the data). Same bounds the game uses.
			if (ofsRot3 != 0 && ofsRot5 > ofsRot3)
			{
				anim.Rot3 = ReadShorts(d, at + ofsRot3, (ofsRot5 - ofsRot3) / 2);
			}
			if (ofsRot5 != 0)
			{
				int end = firstTable == int.MaxValue ? d.Length : firstTable;
				int count = Math.Max(0, (end - (at + ofsRot5)) / 2);
				anim.Rot5 = ReadShorts(d, at + ofsRot5, count);
			}
			return anim;
		}

		/// <summary>
		/// The matrix and scale for one node at one frame, from the node's base pose.
		/// Returns the base pose unchanged for a node the animation marks identity/base.
		/// </summary>
		public static (int[] Matrix, int[] Scale) Evaluate(JointAnimation anim, int node, int frame,
			int[] baseMatrix, int[] baseScale, byte[] d)
		{
			int[] m = (int[])baseMatrix.Clone();
			int[] s = (int[])baseScale.Clone();
			if (node >= anim.NumNode)
			{
				return (m, s);
			}
			JointAnimation.NodeChannels ch = anim.Nodes[node];
			uint flag = ch.Flag;
			int f = Math.Min(Math.Max(frame, 0), anim.NumFrame - 1);
			if ((flag & 1) != 0)
			{
				// Identity: no motion on this node at all.
				return (Identity(), new[] { 4096, 4096, 4096 });
			}
			int w = 0;
			if ((flag & 2) != 0)
			{
				m[9] = m[10] = m[11] = 0;
			}
			else if ((flag & 4) == 0)
			{
				for (int k = 0; k < 3; k++)
				{
					if ((flag & (8u << k)) != 0)
					{
						m[9 + k] = (int)ch.Words[w++];
						continue;
					}
					uint info = ch.Words[w];
					int table = anim.Base + (int)ch.Words[w + 1];
					int index = f >> (int)(info >> 30);
					m[9 + k] = (info & 0x20000000) != 0
						? (short)U16(d, table + 2 * index)
						: (int)U32(d, table + 4 * index);
					w += 2;
				}
			}

			if ((flag & 0x40) != 0)
			{
				m[0] = m[4] = m[8] = 4096;
				m[1] = m[2] = m[3] = m[5] = m[6] = m[7] = 0;
			}
			else if ((flag & 0x80) == 0)
			{
				ushort rotIndex;
				if ((flag & 0x100) != 0)
				{
					rotIndex = (ushort)ch.Words[w++];
				}
				else
				{
					uint info = ch.Words[w];
					int table = anim.Base + (int)ch.Words[w + 1];
					rotIndex = (ushort)U16(d, table + 2 * (f >> (int)(info >> 30)));
					w += 2;
				}
				if ((rotIndex & 0x8000) != 0)
				{
					// A pivot rotation: one axis fixed, the other two given by a and b.
					int i = (rotIndex & 0x7FFF) * 3;
					short head = anim.Rot3[i];
					int a = anim.Rot3[i + 1];
					int b = anim.Rot3[i + 2];
					int c = (head & 0x20) != 0 ? -b : b;
					int dd = (head & 0x40) != 0 ? -a : a;
					int one = (head & 0x10) != 0 ? -4096 : 4096;
					for (int k = 0; k < 9; k++) m[k] = 0;
					switch (head & 0xF)
					{
						case 0: m[0] = one; m[4] = a; m[5] = b; m[7] = c; m[8] = dd; break;
						case 1: m[1] = one; m[3] = a; m[5] = b; m[6] = c; m[8] = dd; break;
						case 2: m[2] = one; m[3] = a; m[4] = b; m[6] = c; m[7] = dd; break;
						case 3: m[3] = one; m[1] = a; m[2] = b; m[7] = c; m[8] = dd; break;
						case 4: m[4] = one; m[0] = a; m[2] = b; m[6] = c; m[8] = dd; break;
						case 5: m[5] = one; m[0] = a; m[1] = b; m[6] = c; m[7] = dd; break;
						case 6: m[6] = one; m[1] = a; m[2] = b; m[4] = c; m[5] = dd; break;
						case 7: m[7] = one; m[0] = a; m[2] = b; m[3] = c; m[5] = dd; break;
						case 8: m[8] = one; m[0] = a; m[1] = b; m[3] = c; m[4] = dd; break;
					}
				}
				else
				{
					// Five packed shorts: the first two rows, the third is their cross product.
					int i = (rotIndex & 0x7FFF) * 5;
					short r0 = anim.Rot5[i], r1 = anim.Rot5[i + 1], r2 = anim.Rot5[i + 2], r3 = anim.Rot5[i + 3], r4 = anim.Rot5[i + 4];
					m[0] = r0 >> 3; m[1] = r1 >> 3; m[2] = r2 >> 3;
					m[3] = r3 >> 3; m[4] = r4 >> 3;
					m[5] = (short)(((r0 & 7) << 9) | ((r1 & 7) << 6) | ((r2 & 7) << 3) | (r3 & 7) | ((r4 & 1) * 61440));
					long cx = (long)m[1] * m[5] - (long)m[2] * m[4];
					long cy = (long)m[2] * m[3] - (long)m[0] * m[5];
					long cz = (long)m[0] * m[4] - (long)m[1] * m[3];
					m[6] = (int)(cx >> 12); m[7] = (int)(cy >> 12); m[8] = (int)(cz >> 12);
				}
			}

			if ((flag & 0x200) != 0)
			{
				s[0] = s[1] = s[2] = 4096;
			}
			else if ((flag & 0x400) == 0)
			{
				for (int k = 0; k < 3; k++)
				{
					if ((flag & (2048u << k)) != 0)
					{
						s[k] = (int)ch.Words[w];
						w += 2;
						continue;
					}
					uint info = ch.Words[w];
					int table = anim.Base + (int)ch.Words[w + 1];
					int index = f >> (int)(info >> 30);
					s[k] = (info & 0x20000000) != 0
						? (short)U16(d, table + 4 * index)          // pairs of s16: value, inverse
						: (int)U32(d, table + 8 * index);           // pairs of s32
					w += 2;
				}
			}
			return (m, s);
		}

		private static int[] Identity()
		{
			return new[] { 4096, 0, 0, 0, 4096, 0, 0, 0, 4096, 0, 0, 0 };
		}

		private static short[] ReadShorts(byte[] d, int at, int count)
		{
			count = Math.Max(0, Math.Min(count, (d.Length - at) / 2));
			short[] values = new short[count];
			for (int i = 0; i < count; i++)
			{
				values[i] = (short)U16(d, at + 2 * i);
			}
			return values;
		}

		private static uint U32(byte[] d, int a) =>
			(uint)(d[a] | (d[a + 1] << 8) | (d[a + 2] << 16) | (d[a + 3] << 24));

		private static int U16(byte[] d, int a) => d[a] | (d[a + 1] << 8);
	}
}
