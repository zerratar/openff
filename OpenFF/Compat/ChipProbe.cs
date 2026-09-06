// --chipprobe: what the drawer does to one field chip, mirrored or not.
//
// For the first model drawn with as many vertices as the option names (a chip is a few
// hundred), the raw display-list vertices and the same vertices through the matrix the
// drawer applies to each are gathered, and one line records both bounds, the matrix
// restores seen and the handedness of the model matrix. Comparing an FF4 run with the
// mirror on against one with it off says whether the mirrored chip's relief survives the
// transform. Diagnostic only; silent without the option.

using System;
using Microsoft.Xna.Framework;

namespace OpenFF.Client
{
	internal static class ChipProbe
	{
		private static readonly int _wanted = ParseWanted();
		private static bool _capturing, _done;
		private static int _count;
		private static long _det;
		private static readonly float[] _rawMin = new float[3], _rawMax = new float[3];
		private static readonly float[] _outMin = new float[3], _outMax = new float[3];
		private static readonly System.Collections.Generic.List<string> _matrices = new System.Collections.Generic.List<string>();
		private static Matrix _last;
		private static readonly float[] _shapeMin = new float[3], _shapeMax = new float[3];
		private static float _shapeRawMin, _shapeRawMax;
		private static int _shapeCount;
		private static bool _haveLast;

		private static int ParseWanted()
		{
			string value = Options.Get("chipprobe");
			return value != null && int.TryParse(value, out int n) ? n : 0;
		}

		public static void Begin(GlobalScope.MtxFx43 m, int vertices, int posScale)
		{
			if (_wanted == 0)
			{
				return;
			}
			if (_capturing)
			{
				Flush();
			}
			if (_done || vertices != _wanted || m == null)
			{
				return;
			}
			long a = m._00, b = m._01, c = m._02, d = m._10, e = m._11, f = m._12, g = m._20, h = m._21, i = m._22;
			_det = a * (e * i - f * h) - b * (d * i - f * g) + c * (d * h - e * g);
			_capturing = true;
			_count = 0;
			_haveLast = false;
			_matrices.Clear();
			for (int k = 0; k < 3; k++)
			{
				_rawMin[k] = _outMin[k] = float.MaxValue;
				_rawMax[k] = _outMax[k] = float.MinValue;
			}
			Log.Write(LogChannel.General, "chipprobe: model with " + vertices + " vertices, posScale " + posScale + ", det " + (_det / 4096.0 / 4096.0 / 4096.0).ToString("0.###")
				+ " matrix [" + m._00 + "," + m._01 + "," + m._02 + " / " + m._10 + "," + m._11 + "," + m._12 + " / " + m._20 + "," + m._21 + "," + m._22 + " / " + m._30 + "," + m._31 + "," + m._32 + "]");
		}

		public static void Vertex(float x, float y, float z, Matrix m)
		{
			if (!_capturing)
			{
				return;
			}
			_count++;
			Grow(_rawMin, _rawMax, x, y, z);
			float ox = x * m.M11 + y * m.M21 + z * m.M31 + m.M41;
			float oy = x * m.M12 + y * m.M22 + z * m.M32 + m.M42;
			float oz = x * m.M13 + y * m.M23 + z * m.M33 + m.M43;
			Grow(_outMin, _outMax, ox, oy, oz);
			Grow(_shapeMin, _shapeMax, ox, oy, oz);
			if (_matrices.Count >= 3 && _matrices.Count <= 4 && _shapeCount < 4)
			{
				Log.Write(LogChannel.General, "chipprobe: quad" + _matrices.Count + " raw (" + R(x) + "," + R(y) + "," + R(z) + ") -> view (" + R(ox) + "," + R(oy) + "," + R(oz) + ")");
			}
			if (y < _shapeRawMin) _shapeRawMin = y; if (y > _shapeRawMax) _shapeRawMax = y;
			_shapeCount++;
			if (!_haveLast || m != _last)
			{
				if (_haveLast && _matrices.Count <= 12 && _matrices.Count > 0)
				{
					_matrices[_matrices.Count - 1] += " shape view y " + R(_shapeMin[1]) + ".." + R(_shapeMax[1]) + " z " + R(_shapeMin[2]) + ".." + R(_shapeMax[2]) + " raw y " + R(_shapeRawMin) + ".." + R(_shapeRawMax) + " n=" + _shapeCount;
				}
				_shapeMin[0] = _shapeMin[1] = _shapeMin[2] = float.MaxValue;
				_shapeMax[0] = _shapeMax[1] = _shapeMax[2] = float.MinValue;
				_shapeRawMin = float.MaxValue; _shapeRawMax = float.MinValue; _shapeCount = 0;
				_last = m;
				_haveLast = true;
				if (_matrices.Count < 12)
				{
					_matrices.Add("[" + m.M11.ToString("0.##") + "," + m.M12.ToString("0.##") + "," + m.M13.ToString("0.##") + " / " + m.M21.ToString("0.##") + "," + m.M22.ToString("0.##") + "," + m.M23.ToString("0.##")
						+ " / " + m.M31.ToString("0.##") + "," + m.M32.ToString("0.##") + "," + m.M33.ToString("0.##") + " / " + m.M41.ToString("0.#") + "," + m.M42.ToString("0.#") + "," + m.M43.ToString("0.#") + "]");
				}
			}
		}

		private static void Grow(float[] min, float[] max, float x, float y, float z)
		{
			if (x < min[0]) min[0] = x; if (x > max[0]) max[0] = x;
			if (y < min[1]) min[1] = y; if (y > max[1]) max[1] = y;
			if (z < min[2]) min[2] = z; if (z > max[2]) max[2] = z;
		}

		private static void Flush()
		{
			_capturing = false;
			_done = true;
			Log.Write(LogChannel.General, "chipprobe: " + _count + " vertices raw x " + R(_rawMin[0]) + ".." + R(_rawMax[0]) + " y " + R(_rawMin[1]) + ".." + R(_rawMax[1]) + " z " + R(_rawMin[2]) + ".." + R(_rawMax[2])
				+ " | view x " + R(_outMin[0]) + ".." + R(_outMax[0]) + " y " + R(_outMin[1]) + ".." + R(_outMax[1]) + " z " + R(_outMin[2]) + ".." + R(_outMax[2]) + " | " + _matrices.Count + " matrices");
			foreach (string m in _matrices)
			{
				Log.Write(LogChannel.General, "chipprobe: matrix " + m);
			}
		}

		private static string R(float v) => v.ToString("0.#");
	}
}
