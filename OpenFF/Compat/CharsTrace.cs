// --chars-trace=<file>[,<steps>] and --camera-trace=<file>[,<steps>]: every character the game has up, and the
// camera, every <steps> steps (15), in the columns Tools/ff4hook's 'chars' and 'camera' write for the Steam FF4.exe -
// so where a character stands, whether it is hidden, which motion it plays and where the camera looks can be set
// beside the real game's, shot by shot (Tools/ff4hook/chars_compare.py, camera_compare.py).

using System;
using System.Globalization;
using System.IO;

namespace OpenFF.Client
{
	internal static class CharsTrace
	{
		private static readonly int _charsEvery, _cameraEvery;
		private static readonly StreamWriter _chars = Open("chars-trace", out _charsEvery);
		private static readonly StreamWriter _camera = Open("camera-trace", out _cameraEvery);

		private static StreamWriter Open(string option, out int every)
		{
			every = 15;
			string arg = Options.Get(option);
			if (string.IsNullOrEmpty(arg)) return null;
			string[] parts = arg.Split(',');
			if (parts.Length > 1 && int.TryParse(parts[1], out int n) && n > 0) every = n;
			try { return new StreamWriter(parts[0], false); }
			catch (Exception ex) { Log.Write(LogChannel.General, option + ": " + parts[0] + ": " + ex.Message); return null; }
		}

		/// <summary>Once a game step.</summary>
		public static void Tick()
		{
			long frame = OpenFF.Game.Time.Frame;
			if (_chars != null && frame % _charsEvery == 0)
			{
				try
				{
					GlobalScope.characterMng.WriteState(_chars, frame);
					_chars.Flush();
				}
				catch (Exception ex) { Log.First(LogChannel.General, "chars-trace", 1, () => "chars trace: " + ex.Message); }
			}
			if (_camera != null && frame % _cameraEvery == 0)
			{
				// NitroSystem's global camera, as FF4.exe's NNS_G3dGlbGetCameraPos / Target / Up and its projection
				GlobalScope.NNSG3dGlb g = GlobalScope.NNS_G3dGlb;
				_camera.WriteLine(string.Join("\t", frame,
					F(g.camPos.x), F(g.camPos.y), F(g.camPos.z), F(g.camTarget.x), F(g.camTarget.y), F(g.camTarget.z),
					F4(g.camUp.x), F4(g.camUp.y), F4(g.camUp.z), F4(g.projMtx._00), F4(g.projMtx._11)));
				_camera.Flush();
			}
		}

		private static string F(int fx) => (fx / 4096.0).ToString("0.000", CultureInfo.InvariantCulture);
		private static string F4(int fx) => (fx / 4096.0).ToString("0.0000", CultureInfo.InvariantCulture);
	}
}
