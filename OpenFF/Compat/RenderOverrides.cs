// Debug overrides for the GL emulation's render state.
//
// The emulation picks cull / depth / alpha-test state from the NDS renderer's
// intent. When 3D geometry is submitted but nothing appears, the fastest way to
// find out why is to neutralise one piece of state at a time:
//
//   --cull=none        ignore back-face culling
//   --cull=cw|ccw      force a winding
//   --depth=off        ignore the depth test
//   --alphatest=off    draw through BasicEffect instead of AlphaTestEffect
//
// All default to "game", which changes nothing.

using System;
using Microsoft.Xna.Framework.Graphics;

namespace OpenFF.Client
{
	// MonoGame's types by name: the engine's OpenFF.Game, GameTime, Color and vectors sit a namespace up.
	using Game = Microsoft.Xna.Framework.Game;

	internal static class RenderOverrides
	{
		private static readonly string _cull = (Options.Get("cull") ?? "game").ToLowerInvariant();
		private static readonly bool _depthOff = IsOff(Options.Get("depth"));
		private static readonly bool _alphaTestOff = IsOff(Options.Get("alphatest"));

		private static bool IsOff(string value) =>
			value != null && (value.Equals("off", StringComparison.OrdinalIgnoreCase)
				|| value == "0" || value.Equals("false", StringComparison.OrdinalIgnoreCase));

		/// <summary>
		/// The effect last bound by the GL layer. The emulation chooses between
		/// BasicEffect and AlphaTestEffect per draw but had no way to notice that the
		/// choice had changed, so it would leave the previous effect bound - and leave
		/// the new one without a texture. Kept here rather than in the decompiled file.
		/// </summary>
		public static Effect LastEffect;

		/// <summary>--notex draws everything with vertex colours only. If shapes appear,
		/// the geometry is fine and the textures are the problem.</summary>
		public static readonly bool NoTextures = Options.Get("notex") != null;

		/// <summary>--start=&lt;part&gt; overrides the game part the title sequence begins in.</summary>
		public static GlobalScope.GAMEPART StartPart(GlobalScope.GAMEPART fallback)
		{
			string want = Options.Get("start");
			if (string.IsNullOrEmpty(want))
			{
				return fallback;
			}
			foreach (GlobalScope.GAMEPART part in Enum.GetValues(typeof(GlobalScope.GAMEPART)))
			{
				string name = part.ToString();
				if (name.Equals(want, StringComparison.OrdinalIgnoreCase)
					|| name.Equals("GAMEPART_" + want, StringComparison.OrdinalIgnoreCase))
				{
					Log.Write(LogChannel.General, "start part overridden: " + name);
					return part;
				}
			}
			Log.Write(LogChannel.General, "unknown --start=" + want + "; using " + fallback);
			return fallback;
		}

		public static bool AnyActive =>
			_cull != "game" || _depthOff || _alphaTestOff || NoTextures;

		/// <summary>Substitutes the rasteriser state the emulation chose.</summary>
		public static RasterizerState Rasterizer(RasterizerState chosen)
		{
			switch (_cull)
			{
				case "none": return RasterizerState.CullNone;
				case "cw": return RasterizerState.CullClockwise;
				case "ccw": return RasterizerState.CullCounterClockwise;
				default: return chosen;
			}
		}

		private static readonly System.Collections.Generic.Dictionary<int, DepthStencilState> _depthStates
			= new System.Collections.Generic.Dictionary<int, DepthStencilState>();

		/// <summary>
		/// Builds the depth state the game actually asked for.
		///
		/// The emulation decoded glDepthFunc into m_DepthFunc and then never used it,
		/// picking between the three stock DepthStencilState objects instead - so every
		/// depth-tested draw silently ran with LessEqual regardless of what was set.
		/// </summary>
		public static DepthStencilState DepthState(bool test, bool write, CompareFunction func)
		{
			if (_depthOff || !test)
			{
				return DepthStencilState.None;
			}
			int key = ((int)func << 2) | (write ? 1 : 0);
			lock (_depthStates)
			{
				if (!_depthStates.TryGetValue(key, out DepthStencilState state))
				{
					state = new DepthStencilState
					{
						DepthBufferEnable = true,
						DepthBufferWriteEnable = write,
						DepthBufferFunction = func
					};
					_depthStates[key] = state;
				}
				return state;
			}
		}

		/// <summary>Substitutes the depth state the emulation chose.</summary>
		public static DepthStencilState Depth(DepthStencilState chosen)
		{
			return _depthOff ? DepthStencilState.None : chosen;
		}

		/// <summary>False forces the BasicEffect path even when the game asked for alpha test.</summary>
		public static bool AlphaTest(bool chosen)
		{
			return _alphaTestOff ? false : chosen;
		}

		public static void LogState()
		{
			if (AnyActive)
			{
				Log.Write(LogChannel.General, string.Format(
					"render overrides active: cull={0} depth={1} alphatest={2}",
					_cull, _depthOff ? "off" : "game", _alphaTestOff ? "off" : "game"));
			}
		}
	}
}
