// A heartbeat for a frame that renders nothing.
//
// Every 90 frames for the first few hundred, the General channel gets one line: how many
// vertices the model drawer emitted last frame, whether 3D is enabled, the fade's state
// and where the field camera is. Enough to tell "the scene is empty" from "the scene is
// covered" from "the camera is elsewhere" without a debugger attached. Off unless --probe.

using System;

namespace FF3
{
	internal static class FrameProbe
	{
		private static readonly bool _enabled = Options.Get("probe") != null;
		private static int _frames;
		private static int _lastVertices;

		/// <summary>Vertices emitted by the model drawer so far this frame.</summary>
		public static int Vertices;

		/// <summary>Matrix restores this frame that found an all-zero stack slot.</summary>
		public static int ZeroRestores;
		private static int _lastZeroRestores;
		private static readonly System.Collections.Generic.SortedSet<int> _zeroSlots = new System.Collections.Generic.SortedSet<int>();

		public static void NoteSlot(int slot)
		{
			lock (_zeroSlots)
			{
				_zeroSlots.Add(slot);
			}
		}

		private static int _nodesLogged;

		/// <summary>A node descriptor as the drawer read it: the first few, once.</summary>
		public static void NoteNode(int node, int flags, int[] scale, int[] matrix, int words, int consumed)
		{
			if (!_enabled || _nodesLogged >= 8 || node == 0)
			{
				return;
			}
			_nodesLogged++;
			string m = "";
			for (int i = 0; i < 12 && i < matrix.Length; i++)
			{
				m += (i == 0 ? "" : ",") + matrix[i];
			}
			Log.Write(LogChannel.General, "node: " + node + " flags=0x" + flags.ToString("X4") + " scale=" + scale[0] + "," + scale[1] + "," + scale[2]
				+ " matrix=[" + m + "] words=" + words + " consumed=" + consumed);
		}

		private static int _shapesLogged;

		/// <summary>The matrix a shape is about to be drawn with, when it is degenerate: the first few, once.</summary>
		public static void NoteShapeMatrix(int shape, int material, int[] current, Microsoft.Xna.Framework.Matrix m, int nodeCount, int shapeCount)
		{
			if (!_enabled || _shapesLogged >= 6)
			{
				return;
			}
			bool zero = m.M11 == 0f && m.M12 == 0f && m.M13 == 0f && m.M21 == 0f && m.M22 == 0f && m.M23 == 0f && m.M31 == 0f && m.M32 == 0f && m.M33 == 0f;
			if (!zero)
			{
				return;
			}
			_shapesLogged++;
			string c = "";
			for (int i = 0; i < 12 && i < current.Length; i++)
			{
				c += (i == 0 ? "" : ",") + current[i];
			}
			Log.Write(LogChannel.General, "shape: " + shape + " material " + material + " of a model with " + nodeCount + " nodes and " + shapeCount
				+ " shapes drawn with a zero matrix; currentMtx=[" + c + "] converted M11=" + m.M11 + " M22=" + m.M22 + " M44=" + m.M44 + " M41=" + m.M41);
		}

		private static float _rawMax;
		private static int _zeroMatrices;
		private static string _lastRaw = "";

		/// <summary>A decoded (model-space) vertex and the matrix about to be applied to it.</summary>
		public static void NoteRaw(float x, float y, float z, Microsoft.Xna.Framework.Matrix m)
		{
			if (!_enabled)
			{
				return;
			}
			float a = Math.Max(Math.Abs(x), Math.Max(Math.Abs(y), Math.Abs(z)));
			if (a > _rawMax) _rawMax = a;
			if (m.M11 == 0f && m.M12 == 0f && m.M13 == 0f && m.M21 == 0f && m.M22 == 0f && m.M23 == 0f && m.M31 == 0f && m.M32 == 0f && m.M33 == 0f)
			{
				_zeroMatrices++;
			}
		}

		private static float _minX = float.MaxValue, _minY = float.MaxValue, _minZ = float.MaxValue;
		private static float _maxX = float.MinValue, _maxY = float.MinValue, _maxZ = float.MinValue;
		private static string _lastBounds = "";

		/// <summary>One emitted vertex position, for the frame's bounds.</summary>
		public static void Note(float x, float y, float z)
		{
			if (!_enabled)
			{
				return;
			}
			if (x < _minX) _minX = x; if (x > _maxX) _maxX = x;
			if (y < _minY) _minY = y; if (y > _maxY) _maxY = y;
			if (z < _minZ) _minZ = z; if (z > _maxZ) _maxZ = z;
		}

		public static void Tick()
		{
			if (!_enabled)
			{
				return;
			}
			_frames++;
			_lastVertices = Vertices;
			Vertices = 0;
			_lastZeroRestores = ZeroRestores;
			ZeroRestores = 0;
			_lastRaw = " rawMax=" + _rawMax.ToString("0.#") + " zeroMatrices=" + _zeroMatrices;
			_rawMax = 0f;
			_zeroMatrices = 0;
			if (_lastVertices > 0)
			{
				_lastBounds = " bounds x " + _minX.ToString("0.#") + ".." + _maxX.ToString("0.#")
					+ " y " + _minY.ToString("0.#") + ".." + _maxY.ToString("0.#")
					+ " z " + _minZ.ToString("0.#") + ".." + _maxZ.ToString("0.#");
			}
			_minX = _minY = _minZ = float.MaxValue;
			_maxX = _maxY = _maxZ = float.MinValue;
			if (_frames % 90 != 0 || _frames > 900)
			{
				return;
			}
			try
			{
				GlobalScope.dgs.CFade fade = GlobalScope.dgs.CFade.Main();
				GlobalScope.cmr.CWorldCamera camera = GlobalScope.CCastCommandTransit.getInstance().cast_FieldCamera();
				string where = camera != null
					? " camera pos=" + Fx(camera.Pos()) + "+" + Fx(camera.PosOffset()) + " target=" + Fx(camera.Trg()) + "+" + Fx(camera.TrgOffset())
					: " camera=none";
				int[] cm = GlobalScope.NNS_G3dGlb.cameraMtx.a;
				string cameraMatrix = "";
				for (int i = 0; i < cm.Length && i < 12; i++)
				{
					cameraMatrix += (i == 0 ? " cameraMtx=[" : ",") + (cm[i] / 4096f).ToString("0.##");
				}
				where += cameraMatrix + "]";
				if (camera != null)
				{
					GlobalScope.ds.sys3d.CameraInfo info = camera.getCameraInfo();
					where += " camInfo pos=" + Fx(info.position) + " target=" + Fx(info.target) + " up=" + Fx(info.camUp);
				}
				GlobalScope.wld.CWorldSystem world = GlobalScope.wld.WorldPart.getInstance()?.getWorldSystem();
				if (world != null)
				{
					where += " world mode=" + world.Mode() + " state=" + (world.CrtState()?.GetType().Name ?? "none") + " canRunPlayer=" + world.canRunPlayerMng();
				}
				Log.Write(LogChannel.General, "probe: frame " + _frames + " vertices=" + _lastVertices
					+ " enable3D=" + GlobalScope.enable3D
					+ " fade faded=" + fade.isFaded() + " cleared=" + fade.isCleared()
					+ " previousPart=" + GlobalScope.sys.GGlobal.getPreviousPart()
					+ where + _lastBounds + _lastRaw + " zeroRestores=" + _lastZeroRestores + " slots=" + string.Join(",", _zeroSlots));
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "probe: " + ex.GetType().Name + " " + ex.Message);
			}
		}

		private static string Fx(GlobalScope.VecFx32 v)
		{
			return v == null ? "null" : "(" + (v.x / 4096f).ToString("0.#") + "," + (v.y / 4096f).ToString("0.#") + "," + (v.z / 4096f).ToString("0.#") + ")";
		}
	}
}
