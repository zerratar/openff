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

		/// <summary>The stage this part lands on: a loaded save's (--load), --map, or the game's default.</summary>
		public static string Stage => Ff4Saves.Pending?.Map ?? GameProfile.StartStage ?? GameProfile.DefaultStage;

		/// <summary>--pos=x,y,z in world units (the .mcl's units), as 20.12 fixed point.</summary>
		public static GlobalScope.VecFx32 StartPosition
		{
			get
			{
				// Not the exact origin: the world camera reads an all-zero position as
				// "not set yet" and skips its update, which leaves the camera matrix zero
				// and the screen black.
				GlobalScope.VecFx32 v = new GlobalScope.VecFx32(0, 0, 4096);
				Ff4FieldState.Data saved = Ff4Saves.Pending;
				if (saved != null)
				{
					v.x = (int)Math.Round(saved.X * 4096);
					v.y = (int)Math.Round(saved.Y * 4096);
					v.z = (int)Math.Round(saved.Z * 4096);
					if (v.x == 0 && v.y == 0 && v.z == 0) v.z = 4096;
					return v;
				}
				string pos = Options.Get("pos");
				if (string.IsNullOrEmpty(pos))
				{
					// A new FF4 game: NewGameInitPart puts the party at the origin of t00_00.
					if (GameProfile.IsFf4 && string.IsNullOrEmpty(Options.Get("map")))
					{
						return v;
					}
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
				if (Ff4Saves.Pending != null) return Ff4Saves.Pending.Rotation;
				string rot = Options.Get("rot");
				return !string.IsNullOrEmpty(rot) && float.TryParse(rot, NumberStyles.Float, CultureInfo.InvariantCulture, out float degrees)
					? (int)Math.Round(degrees * 65536 / 360) : 0;
			}
		}

		/// <summary>
		/// A field stage is entered by chip - f00_12, the chip at column 1, row 2 of the
		/// overworld - and the stage profile says how big a chip is. Given just "f00", the
		/// chip under the start position is worked out the way the stage manager does.
		/// </summary>
		internal static string WithChip(string stage, GlobalScope.VecFx32 at)
		{
			if (stage == null || stage.Length != 3 || stage[0] != 'f' || !char.IsDigit(stage[1]) || !char.IsDigit(stage[2]))
			{
				return stage;
			}
			byte[] profile = GameArchive.Read("files/" + stage + ".stgprf");
			if (profile == null || profile.Length < 28)
			{
				return stage + "_00";
			}
			int chipsX = profile[14], chipsZ = profile[15];
			int sizeX = BitConverter.ToInt32(profile, 20), sizeZ = BitConverter.ToInt32(profile, 24);
			if (chipsX <= 0 || chipsZ <= 0 || sizeX <= 0 || sizeZ <= 0)
			{
				return stage + "_00";
			}
			// The stage manager's getSpot: the world's origin sits in the middle of the centre
			// chip (profile bytes 2 and 3), so a position is offset by half a chip and by the
			// centre chip's index, then wrapped around the world.
			int centreX = profile[2], centreZ = profile[3];
			long x = (long)at.x + sizeX / 2 + (long)sizeX * centreX;
			// FF4 places the chips with z negated (FieldMirror), so its positions look up the
			// chip at -z of FF3's layout.
			long z = (FieldMirror.WantsFieldMirror ? -(long)at.z : at.z) + sizeZ / 2 + (long)sizeZ * centreZ;
			long worldX = (long)sizeX * chipsX, worldZ = (long)sizeZ * chipsZ;
			x = ((x % worldX) + worldX) % worldX;
			z = ((z % worldZ) + worldZ) % worldZ;
			int spotX = (int)(x / sizeX), spotZ = (int)(z / sizeZ);
			return stage + "_" + spotX.ToString("x") + spotZ.ToString("x");
		}

		protected override void doInitialize()
		{
			string stage = WithChip(Stage, StartPosition);
			Log.Write(LogChannel.General, "jump: " + GameProfile.Game + " -> " + stage + " at " + (Ff4Saves.Pending != null ? "the saved spot" : Options.Get("pos") ?? "0,0,0"));
			GlobalScope.sceneMng.setStage(stage);
			GlobalScope.sys.GGlobal.setNextPart(GlobalScope.GAMEPART.GAMEPART_WORLD);
			abort();
		}
	}
}
