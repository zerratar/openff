// A game part that goes straight to a map.
//
// The phone build had a debug menu part (GAMEPART_DEBUG_MENU) whose body did not ship;
// the world, battle and map-jump code still check whether they were entered from it and
// then take a stage name and a position as given instead of a save or an event jump.
// This part fills that slot: it names the stage (--map=<stage>, or the game's default)
// and the position (--pos=x,y,z in world units, --rot=<degrees>), then hands over to
// the world part. For FF4, which has no title in this client yet, it is how the game
// starts; for FF3 it is a way to land in any map for testing.

using System;
using System.Globalization;

namespace FF3
{
	internal sealed class JumpPart : GlobalScope.sys.FF3GamePart
	{
		public static readonly JumpPart Instance = new JumpPart();

		public static void registerPart()
		{
			GlobalScope.sys.GGlobal.registerPart(GlobalScope.GAMEPART.GAMEPART_DEBUG_MENU, Instance);
		}

		/// <summary>The stage this part lands on: --map, or the game's default.</summary>
		public static string Stage => GameProfile.StartStage ?? GameProfile.DefaultStage;

		/// <summary>--pos=x,y,z in world units (the .mcl's units), as 20.12 fixed point.</summary>
		public static GlobalScope.VecFx32 StartPosition
		{
			get
			{
				// Not the exact origin: the world camera reads an all-zero position as
				// "not set yet" and skips its update, which leaves the camera matrix zero
				// and the screen black.
				GlobalScope.VecFx32 v = new GlobalScope.VecFx32(0, 0, 4096);
				string pos = Options.Get("pos");
				if (string.IsNullOrEmpty(pos))
				{
					// Where some other map's exit puts the party on arriving here, when the
					// scripts say (FF4: setInsideMapJump in the neighbouring maps).
					(int X, int Y, int Z, int Facing)? arrival = Ff4Exits.ArrivalInto(Stage);
					if (arrival.HasValue)
					{
						v.x = arrival.Value.X; v.y = arrival.Value.Y; v.z = arrival.Value.Z;
						if (v.x == 0 && v.y == 0 && v.z == 0)
						{
							v.z = 4096;
						}
					}
					return v;
				}
				string[] parts = pos.Split(',');
				if (parts.Length >= 3
					&& float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x)
					&& float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y)
					&& float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
				{
					v.x = (int)Math.Round(x * 4096);
					v.y = (int)Math.Round(y * 4096);
					v.z = (int)Math.Round(z * 4096);
				}
				return v;
			}
		}

		/// <summary>--rot=<degrees> about Y, in the engine's 16-bit angle units.</summary>
		public static int StartRotation
		{
			get
			{
				string rot = Options.Get("rot");
				return !string.IsNullOrEmpty(rot) && float.TryParse(rot, NumberStyles.Float, CultureInfo.InvariantCulture, out float degrees)
					? (int)Math.Round(degrees * 65536 / 360) : 0;
			}
		}

		protected override void doInitialize()
		{
			string stage = Stage;
			Log.Write(LogChannel.General, "jump: " + GameProfile.Game + " -> " + stage + " at " + (Options.Get("pos") ?? "0,0,0"));
			GlobalScope.sceneMng.setStage(stage);
			GlobalScope.sys.GGlobal.setNextPart(GlobalScope.GAMEPART.GAMEPART_WORLD);
			abort();
		}
	}
}
