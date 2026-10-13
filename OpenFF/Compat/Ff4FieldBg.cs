// FF4's field BG commands: bgLoad, bgAlphaBlendSetting, createBGAlphaBlendControlTask, setVisiblePlane.
//
// A field map can lay a 2D picture over the 3D (t03_30: Rydia's "Summoner" caption, faded in and out). libff4 loads it
// into one of the DS's BG planes (WSCBg, sys2d::Bg::bgLoad and bgSetUp, which shows the plane), blends it over the 3D
// with G2_SetBlendAlpha and fades the blend with a WTBGAlphaFader task (ev1 from one value to another over frames).
// Steam's install keeps these as a cell (the .NSCR is an NCER) and a PNG (the .NCGR) - the same files the scenes' 2D
// plates use (np04_screen.NSCR is np04.NCER byte for byte) - so the picture is drawn as such a plate, at half size on
// the 480 x 320 sheet like the plates, its cell's own offset centring it on the screen. Shown while its plane's bit is
// up (GX_SetVisiblePlane), at the blend's first weight (0..16, the DS's).

using System;
using System.Collections.Generic;
using System.IO;

namespace OpenFF.Client
{
	internal static class Ff4FieldBg
	{
		private static readonly Dictionary<int, GlobalScope.sys2d.Sprite3d> _planes = new Dictionary<int, GlobalScope.sys2d.Sprite3d>();

		/// <summary>The blend's weights (G2_SetBlendAlpha / G2_ChangeBlendAlpha): ev1 is the picture's.</summary>
		private static int _ev1 = 16;

		private sealed class Fader
		{
			public int From1, To1, From2, To2, Frames, Hold, Tick, Held;
		}

		private static readonly List<Fader> _faders = new List<Fader>();

		/// <summary>bgLoad(plane, screen base, char base, screen, char, palette).</summary>
		public static void BgLoad(GlobalScope.ScriptEngine engine)
		{
			int plane = (int)engine.getDword() & 3;
			engine.getDword();
			engine.getDword();
			string screen = engine.getString(), chars = engine.getString(), palette = engine.getString();
			Ff4Cutscene.Guard("field BG " + screen, () =>
			{
				Release(plane);
				GlobalScope.sys2d.Sprite3d sprite = new GlobalScope.sys2d.Sprite3d();
				sprite.Load(GlobalScope.sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, Path.GetFileName(screen), null, Path.GetFileName(chars), palette == null ? null : Path.GetFileName(palette));
				sprite.SetCell(0);
				sprite.SetScaleF(2048, 2048);
				sprite.SetPositionI(240, 160);
				GlobalScope.sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(sprite);
				_planes[plane] = sprite;
				// bgSetUp shows its plane.
				GlobalScope.GX_SetVisiblePlane(GlobalScope.GX_GetVisiblePlane() | (1 << plane));
				Refresh();
				Log.Write(LogChannel.File, "script: FF4 field BG " + Path.GetFileName(screen) + " on plane " + plane);
			});
		}

		/// <summary>bgAlphaBlendSetting(plane 1, plane 2, ev1, ev2): G2_SetBlendAlpha.</summary>
		public static void BlendSetting(GlobalScope.ScriptEngine engine)
		{
			engine.getDword();
			engine.getDword();
			_ev1 = (int)engine.getDword();
			engine.getDword();
			Refresh();
		}

		/// <summary>createBGAlphaBlendControlTask(ev1 from, to, frames, ev2 from, to, frames): WTBGAlphaFader.</summary>
		public static void Fade(GlobalScope.ScriptEngine engine)
		{
			Fader f = new Fader
			{
				From1 = (int)engine.getDword(), To1 = (int)engine.getDword(), Frames = (int)engine.getDword(),
				From2 = (int)engine.getDword(), To2 = (int)engine.getDword(), Hold = (int)engine.getDword(),
			};
			_ev1 = f.From1;   // G2_ChangeBlendAlpha at once
			_faders.Add(f);
			Refresh();
		}

		/// <summary>setVisiblePlane(plane, on): 0..3 a BG plane's bit of GX_SetVisiblePlane (the value itself is the bit, as
		/// libff4 ORs it in), 5 the main screen's objects, 6 the second screen's (none here).</summary>
		public static void VisiblePlane(GlobalScope.ScriptEngine engine)
		{
			uint plane = engine.getDword();
			bool on = engine.getDword() == 1;
			if (plane < 4)
			{
				int bits = GlobalScope.GX_GetVisiblePlane();
				GlobalScope.GX_SetVisiblePlane(on ? bits | (int)plane : bits & ~(int)plane);
			}
			else if (plane == 5)
			{
				int bits = GlobalScope.GX_GetVisiblePlane();
				GlobalScope.GX_SetVisiblePlane((bits & ~0x10) | (on ? 0x10 : 0));
			}
			Refresh();
		}

		/// <summary>Each frame: the faders step (WTBGAlphaFader::wtUpdateTask).</summary>
		public static void Tick()
		{
			if (_faders.Count == 0) return;
			for (int i = _faders.Count - 1; i >= 0; i--)
			{
				Fader f = _faders[i];
				int t = f.Frames != 0 ? (f.Tick << 12) / f.Frames : 0;   // FX_Div(tick, frames)
				_ev1 = f.From1 + (int)(((long)t * ((f.To1 - f.From1) << 12)) >> 12 >> 12);
				f.Tick++;
				f.Held++;
				if (f.Frames <= f.Tick && f.Hold <= f.Held) _faders.RemoveAt(i);
			}
			Refresh();
		}

		/// <summary>The map is left: its pictures go with it.</summary>
		public static void Clear()
		{
			foreach (int plane in new List<int>(_planes.Keys)) Release(plane);
			_faders.Clear();
			_ev1 = 16;
		}

		private static void Release(int plane)
		{
			if (!_planes.TryGetValue(plane, out GlobalScope.sys2d.Sprite3d sprite)) return;
			_planes.Remove(plane);
			try
			{
				GlobalScope.sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(sprite);
				sprite.Release();
			}
			catch (Exception) { }
		}

		private static void Refresh()
		{
			if (_planes.Count == 0) return;
			int bits = GlobalScope.GX_GetVisiblePlane();
			int alpha = Math.Clamp(_ev1, 0, 16) * 31 / 16;
			foreach (KeyValuePair<int, GlobalScope.sys2d.Sprite3d> pair in _planes)
			{
				try
				{
					pair.Value.SetShow((bits & (1 << pair.Key)) != 0 && alpha > 0);
					pair.Value.SetAlpha((byte)alpha);
				}
				catch (Exception) { }
			}
		}
	}
}
