// What FramePacer needs from the system: the refresh of the display the window is on, the
// swap interval, and a wait finer than Thread.Sleep's millisecond.
//
// The refresh. SDL - MonoGame's desktop build runs on it, and its library is already loaded
// by the time the game draws - knows which display the window is on and gives that display's
// mode, but its refresh is a whole number: 59 for a 59.94 Hz display, 143 or 144 for 143.9 -
// or 0 or 1, which is Windows saying "the hardware's default rate" (some drivers, virtual and
// remote displays) and SDL passing it on as it is: that says nothing, and is taken as not
// known. On Windows the display configuration (QueryDisplayConfig) has the exact figure as a
// fraction (59997/1000); it is looked up for the same display, matched by its GDI name
// through the monitor at the middle of SDL's bounds for it, and taken when it agrees with
// SDL's to within a hertz and a half, or when SDL's says nothing. The configuration is asked
// again only when SDL's answer changes (the window moved to another display, a mode switch),
// not every time. PresentPlan believes a refresh only between 20 and 1000 Hz.
//
// The swap interval. MonoGame sets it to 1 or 0 by SynchronizeWithVerticalRetrace, and
// again at every device reset; a frame held two or four refreshes (sixty a second on a 120
// or 240 Hz display) needs SDL_GL_SetSwapInterval itself, called on the thread whose GL
// context is current - the game's.
//
// The wait. Thread.Sleep sleeps whole milliseconds and wakes up to one late; a
// high-resolution waitable timer (Windows 10 1803 on) wakes within a fraction of one, so
// FramePacer sleeps on it to shortly before its time and spins only the rest.
//
// SDL's functions are found in the SDL2 module MonoGame loaded (GetModuleHandle on Windows),
// never a second copy, whose GL state would not be the game's. Anything missing - another
// platform, an older Windows, a driver that will not answer - leaves the refresh unknown or
// the interval at MonoGame's, and the pacer paces by the clock as before.

using System;
using System.Runtime.InteropServices;

namespace OpenFF.Client
{
	internal static unsafe class DisplayTiming
	{
		// ---- SDL, from the module MonoGame loaded

		[StructLayout(LayoutKind.Sequential)]
		private struct SdlDisplayMode { public uint Format; public int W, H, RefreshRate; public IntPtr DriverData; }

		[StructLayout(LayoutKind.Sequential)]
		private struct SdlRect { public int X, Y, W, H; }

		private static bool _sdlLooked;
		private static delegate* unmanaged[Cdecl]<IntPtr, int> _getWindowDisplayIndex;
		private static delegate* unmanaged[Cdecl]<int, SdlDisplayMode*, int> _getCurrentDisplayMode;
		private static delegate* unmanaged[Cdecl]<int, SdlRect*, int> _getDisplayBounds;
		private static delegate* unmanaged[Cdecl]<int, int> _setSwapInterval;

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr GetModuleHandleW(string name);

		private static bool Sdl()
		{
			if (_sdlLooked) return _setSwapInterval != null;
			_sdlLooked = true;
			try
			{
				IntPtr module = IntPtr.Zero;
				if (OperatingSystem.IsWindows()) module = GetModuleHandleW("SDL2.dll");
				else if (!NativeLibrary.TryLoad(OperatingSystem.IsMacOS() ? "libSDL2-2.0.0.dylib" : "libSDL2-2.0.so.0", out module)) module = IntPtr.Zero;
				if (module == IntPtr.Zero) return false;
				if (NativeLibrary.TryGetExport(module, "SDL_GetWindowDisplayIndex", out IntPtr a)) _getWindowDisplayIndex = (delegate* unmanaged[Cdecl]<IntPtr, int>)a;
				if (NativeLibrary.TryGetExport(module, "SDL_GetCurrentDisplayMode", out IntPtr b)) _getCurrentDisplayMode = (delegate* unmanaged[Cdecl]<int, SdlDisplayMode*, int>)b;
				if (NativeLibrary.TryGetExport(module, "SDL_GetDisplayBounds", out IntPtr c)) _getDisplayBounds = (delegate* unmanaged[Cdecl]<int, SdlRect*, int>)c;
				if (NativeLibrary.TryGetExport(module, "SDL_GL_SetSwapInterval", out IntPtr d)) _setSwapInterval = (delegate* unmanaged[Cdecl]<int, int>)d;
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "pacing: SDL not reached for the display's timing: " + ex.Message);
			}
			return _setSwapInterval != null;
		}

		/// <summary>Whether a swap interval can be set here (SDL reached).</summary>
		public static bool CanSetSwapInterval => Sdl();

		/// <summary>Sets the swap interval (0 immediate, n: each frame held n refreshes) on the current GL context; false when it could not be.</summary>
		public static bool SetSwapInterval(int interval)
		{
			if (!Sdl()) return false;
			try { return _setSwapInterval(interval) == 0; }
			catch (Exception) { return false; }
		}

		private static int _sdlDisplay = -2, _sdlHz = -1;
		private static double _hz;
		private static string _source = "";

		/// <summary>Where the last refresh came from, for the log: "SDL" or "display configuration".</summary>
		public static string Source => _source;

		/// <summary>The refresh of the display the window (MonoGame's Window.Handle, an SDL window) is on, in Hz; 0 when it cannot be told.</summary>
		public static double RefreshHz(IntPtr window)
		{
			if (window == IntPtr.Zero || !Sdl() || _getWindowDisplayIndex == null || _getCurrentDisplayMode == null) return 0;
			try
			{
				int display = _getWindowDisplayIndex(window);
				if (display < 0) return _hz;
				SdlDisplayMode mode;
				if (_getCurrentDisplayMode(display, &mode) != 0) return _hz;
				if (display == _sdlDisplay && mode.RefreshRate == _sdlHz) return _hz;
				_sdlDisplay = display;
				_sdlHz = mode.RefreshRate;
				// 0 and 1 are Windows' "the hardware's default rate", passed on by SDL: nothing known, so
				// the display configuration's figure is taken if it has one. Taken as 1 Hz, it made "60"
				// a slideshow of a frame and a quarter a second.
				_hz = mode.RefreshRate > 1 ? mode.RefreshRate : 0;
				_source = "SDL";
				if (OperatingSystem.IsWindows() && _getDisplayBounds != null)
				{
					SdlRect r;
					if (_getDisplayBounds(display, &r) == 0)
					{
						double exact = ExactHz(r.X + r.W / 2, r.Y + r.H / 2);
						if (exact > 0 && (_hz <= 0 || Math.Abs(exact - _hz) <= 1.5))
						{
							_hz = exact;
							_source = "display configuration";
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.First(LogChannel.General, "pacing-refresh", 3, () => "pacing: the display's refresh could not be read: " + ex.Message);
			}
			return _hz;
		}

		// ---- Windows: the exact refresh from the display configuration

		[StructLayout(LayoutKind.Sequential)] private struct Point { public int X, Y; }
		[StructLayout(LayoutKind.Sequential)] private struct Rect { public int Left, Top, Right, Bottom; }
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct MonitorInfoEx
		{
			public int Size;
			public Rect Monitor, Work;
			public uint Flags;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string Device;
		}
		[StructLayout(LayoutKind.Sequential)] private struct Luid { public uint Low; public int High; }
		[StructLayout(LayoutKind.Sequential)] private struct Rational { public uint Numerator, Denominator; }
		[StructLayout(LayoutKind.Sequential)] private struct PathSource { public Luid Adapter; public uint Id, ModeIndex, Status; }
		[StructLayout(LayoutKind.Sequential)]
		private struct PathTarget
		{
			public Luid Adapter;
			public uint Id, ModeIndex, Technology, Rotation, Scaling;
			public Rational Refresh;
			public uint ScanLineOrdering;
			public int Available;
			public uint Status;
		}
		[StructLayout(LayoutKind.Sequential)] private struct PathInfo { public PathSource Source; public PathTarget Target; public uint Flags; }
		[StructLayout(LayoutKind.Explicit, Size = 64)]
		private struct ModeInfo
		{
			[FieldOffset(0)] public uint Type;
			[FieldOffset(4)] public uint Id;
			[FieldOffset(8)] public Luid Adapter;
			[FieldOffset(32)] public Rational VSync;   // a target mode's video signal: pixel rate at 16, hsync at 24, vsync at 32
		}
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct SourceName
		{
			public uint Type, Size;
			public Luid Adapter;
			public uint Id;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string GdiName;
		}

		[DllImport("user32.dll")] private static extern IntPtr MonitorFromPoint(Point pt, uint flags);
		[DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern bool GetMonitorInfoW(IntPtr monitor, ref MonitorInfoEx info);
		[DllImport("user32.dll")] private static extern int GetDisplayConfigBufferSizes(uint flags, out uint paths, out uint modes);
		[DllImport("user32.dll")] private static extern int QueryDisplayConfig(uint flags, ref uint paths, [Out] PathInfo[] pathInfo, ref uint modes, [Out] ModeInfo[] modeInfo, IntPtr topology);
		[DllImport("user32.dll")] private static extern int DisplayConfigGetDeviceInfo(ref SourceName request);

		/// <summary>The exact refresh of the monitor at this desktop point, from the display configuration; 0 when not found.</summary>
		private static double ExactHz(int x, int y)
		{
			const uint MonitorDefaultToNearest = 2, OnlyActivePaths = 2, GetSourceName = 1, TargetMode = 2;
			IntPtr monitor = MonitorFromPoint(new Point { X = x, Y = y }, MonitorDefaultToNearest);
			if (monitor == IntPtr.Zero) return 0;
			MonitorInfoEx info = new MonitorInfoEx { Size = Marshal.SizeOf<MonitorInfoEx>() };
			if (!GetMonitorInfoW(monitor, ref info) || string.IsNullOrEmpty(info.Device)) return 0;
			if (GetDisplayConfigBufferSizes(OnlyActivePaths, out uint pathCount, out uint modeCount) != 0) return 0;
			PathInfo[] paths = new PathInfo[pathCount];
			ModeInfo[] modes = new ModeInfo[modeCount];
			if (QueryDisplayConfig(OnlyActivePaths, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero) != 0) return 0;
			for (int i = 0; i < pathCount; i++)
			{
				SourceName name = new SourceName { Type = GetSourceName, Size = (uint)Marshal.SizeOf<SourceName>(), Adapter = paths[i].Source.Adapter, Id = paths[i].Source.Id };
				if (DisplayConfigGetDeviceInfo(ref name) != 0 || !string.Equals(name.GdiName, info.Device, StringComparison.OrdinalIgnoreCase)) continue;
				Rational r = paths[i].Target.Refresh;
				if (r.Denominator == 0 || r.Numerator == 0)
				{
					uint m = paths[i].Target.ModeIndex;
					if (m < modeCount && modes[m].Type == TargetMode) r = modes[m].VSync;
				}
				return r.Denominator == 0 ? 0 : (double)r.Numerator / r.Denominator;
			}
			return 0;
		}

		// ---- the wait

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)] private static extern IntPtr CreateWaitableTimerExW(IntPtr attributes, string name, uint flags, uint access);
		[DllImport("kernel32.dll")] private static extern bool SetWaitableTimer(IntPtr timer, ref long due, int period, IntPtr completion, IntPtr argument, bool resume);
		[DllImport("kernel32.dll")] private static extern uint WaitForSingleObject(IntPtr handle, uint milliseconds);

		private static bool _timerLooked;
		private static IntPtr _timer;

		/// <summary>Whether Sleep has a high-resolution timer to sleep on.</summary>
		public static bool PreciseSleep
		{
			get
			{
				if (!_timerLooked)
				{
					_timerLooked = true;
					const uint HighResolution = 0x2, AllAccess = 0x1F0003;
					try { if (OperatingSystem.IsWindows()) _timer = CreateWaitableTimerExW(IntPtr.Zero, null, HighResolution, AllAccess); }
					catch (Exception) { _timer = IntPtr.Zero; }
				}
				return _timer != IntPtr.Zero;
			}
		}

		/// <summary>Sleeps about this long on the high-resolution timer, waking within a fraction of a millisecond; false, not having slept, where there is none.</summary>
		public static bool Sleep(double seconds)
		{
			if (!PreciseSleep || seconds <= 0) return false;
			long due = -(long)(seconds * 1e7);   // relative, in 100 ns units
			if (due == 0 || !SetWaitableTimer(_timer, ref due, 0, IntPtr.Zero, IntPtr.Zero, false)) return false;
			WaitForSingleObject(_timer, 1000);
			return true;
		}
	}
}
