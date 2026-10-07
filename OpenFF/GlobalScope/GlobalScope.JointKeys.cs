// PORT: a joint animation's keys read between them. The phone port's DrawModel samples a track at the whole frame
// (frame >> 12) and at the key at or before it (index frame >> step) - so a motion keyed every fourth frame, as most of
// FF4's are (step 2 below lastInterp), changes its pose 7.5 times a second at the game's 30, and one played at half
// speed only every other step; the in-between frames had nothing to blend. NitroSystem's own joint animation reads
// keys spaced by a step linearly between the two either side (below the track's lastInterp, its info word's bits
// 16-28; after it the keys are per frame), and here that is done with the frame's fraction as well: translation and
// scale lerped, a rotation's two matrices lerped as the engine mixes two motions (component by component) and made
// a rotation again. A pose then changes every step, and FrameCapture's smoothing takes it from there.

using System;

internal static partial class GlobalScope
{
	internal static class JointKeys
	{
		/// <summary>The key at or before the frame (<paramref name="fxFrame"/>, fx32), and when the frame lies between it and the
		/// next one inside the interpolated range, that one and how far (0..4095); the fraction 0 otherwise.</summary>
		public static int Index(uint info, int fxFrame, int numFrame, out int next, out int fraction)
		{
			int shift = (int)(info >> 30);
			int whole = fxFrame >> 12;
			int index = whole >> shift;
			next = index;
			fraction = 0;
			int last = shift == 0 ? numFrame - 1 : (int)((info >> 16) & 0x1FFF);
			if (whole >= last) return index;
			int f = (fxFrame >> shift) & 0xFFF;
			if (f == 0 || ((index + 1) << shift) > last) return index;
			next = index + 1;
			fraction = f;
			return index;
		}

		/// <summary>A translation or scale track's value at the frame (<paramref name="stride"/> 2 for scale's value-and-inverse pairs).</summary>
		public static int Value(uint info, int fxFrame, int numFrame, short[] t16, int[] t32, int stride)
		{
			bool use16 = (info & 0x20000000) != 0;
			int index = Index(info, fxFrame, numFrame, out int next, out int f);
			int a = Read(use16, t16, t32, index * stride);
			if (f == 0) return a;
			int b = Read(use16, t16, t32, next * stride);
			return a + (int)(((long)(b - a) * f) >> 12);
		}

		private static int Read(bool use16, short[] t16, int[] t32, int at)
		{
			if (use16) return t16[Math.Min(at, t16.Length - 1)];
			return t32[Math.Min(at, t32.Length - 1)];
		}

		private static readonly MtxFx43 _second = new MtxFx43();
		private static readonly float[] _mix = new float[9];
		private static readonly VecFx32 _row0 = new VecFx32(), _row1 = new VecFx32(), _row2 = new VecFx32();

		/// <summary>A rotation track's matrix at the frame into <paramref name="m"/>'s 3 x 3 (the translation left as it is).</summary>
		public static void Rotation(NNSG3dResJntAnm res, ushort[] table, uint info, int fxFrame, int numFrame, MtxFx43 m)
		{
			int index = Index(info, fxFrame, numFrame, out int next, out int f);
			Decode(res, table[Math.Min(index, table.Length - 1)], m);
			if (f == 0) return;
			Decode(res, table[Math.Min(next, table.Length - 1)], _second);
			float t = f / 4096f;
			float[] r = _mix;
			for (int k = 0; k < 9; k++) r[k] = (m.a[k] + (_second.a[k] - m.a[k]) * t) / 4096f;
			// Made a rotation again (Gram-Schmidt on the rows): the straight mix of two turns is a little short of one.
			Normalize(r, 0);
			float dot = r[0] * r[3] + r[1] * r[4] + r[2] * r[5];
			r[3] -= dot * r[0]; r[4] -= dot * r[1]; r[5] -= dot * r[2];
			Normalize(r, 3);
			float sign = r[6] * (r[1] * r[5] - r[2] * r[4]) + r[7] * (r[2] * r[3] - r[0] * r[5]) + r[8] * (r[0] * r[4] - r[1] * r[3]) < 0 ? -1f : 1f;
			r[6] = sign * (r[1] * r[5] - r[2] * r[4]);
			r[7] = sign * (r[2] * r[3] - r[0] * r[5]);
			r[8] = sign * (r[0] * r[4] - r[1] * r[3]);
			for (int k = 0; k < 9; k++) m.a[k] = (int)Math.Round(r[k] * 4096f);
		}

		private static void Normalize(float[] r, int at)
		{
			float length = (float)Math.Sqrt(r[at] * r[at] + r[at + 1] * r[at + 1] + r[at + 2] * r[at + 2]);
			if (length < 1e-6f) return;
			r[at] /= length;
			r[at + 1] /= length;
			r[at + 2] /= length;
		}

		/// <summary>A rotation key into <paramref name="m"/>'s 3 x 3: rot3 (a pivot and two values) or rot5 (two rows packed,
		/// the third their cross product) - DrawModel's own decoding, moved here to be read for two keys.</summary>
		public static void Decode(NNSG3dResJntAnm res, ushort code, MtxFx43 m)
		{
			if ((code & 0x8000) != 0)
			{
				short[] rot = res.rot3;
				int at = (code & 0x7FFF) * 6 / 2;
				short mode = rot[at];
				int b = rot[at + 1];
				int c = rot[at + 2];
				int d = ((mode & 0x20) != 0) ? -c : c;
				int e = ((mode & 0x40) != 0) ? -b : b;
				int one = ((mode & 0x10) != 0) ? -4096 : 4096;
				m._00 = 0; m._01 = 0; m._02 = 0;
				m._10 = 0; m._11 = 0; m._12 = 0;
				m._20 = 0; m._21 = 0; m._22 = 0;
				switch (mode & 0xF)
				{
					case 0: m._00 = one; m._11 = b; m._12 = c; m._21 = d; m._22 = e; break;
					case 1: m._01 = one; m._10 = b; m._12 = c; m._20 = d; m._22 = e; break;
					case 2: m._02 = one; m._10 = b; m._11 = c; m._20 = d; m._21 = e; break;
					case 3: m._10 = one; m._01 = b; m._02 = c; m._21 = d; m._22 = e; break;
					case 4: m._11 = one; m._00 = b; m._02 = c; m._20 = d; m._22 = e; break;
					case 5: m._12 = one; m._00 = b; m._01 = c; m._20 = d; m._21 = e; break;
					case 6: m._20 = one; m._01 = b; m._02 = c; m._11 = d; m._12 = e; break;
					case 7: m._21 = one; m._00 = b; m._02 = c; m._10 = d; m._12 = e; break;
					case 8: m._22 = one; m._00 = b; m._01 = c; m._10 = d; m._11 = e; break;
				}
				return;
			}
			short[] rot2 = res.rot5;
			int at5 = (code & 0x7FFF) * 10 / 2;
			m._00 = rot2[at5] >> 3;
			m._01 = rot2[at5 + 1] >> 3;
			m._02 = rot2[at5 + 2] >> 3;
			m._10 = rot2[at5 + 3] >> 3;
			m._11 = rot2[at5 + 4] >> 3;
			m._12 = (short)(((rot2[at5] & 7) << 9) | ((rot2[at5 + 1] & 7) << 6) | ((rot2[at5 + 2] & 7) << 3) | (rot2[at5 + 3] & 7) | ((rot2[at5 + 4] & 1) * 61440));
			_row0.x = m._00; _row0.y = m._01; _row0.z = m._02;
			_row1.x = m._10; _row1.y = m._11; _row1.z = m._12;
			VEC_CrossProduct(_row0, _row1, _row2);
			m._20 = _row2.x;
			m._21 = _row2.y;
			m._22 = _row2.z;
		}
	}
}
