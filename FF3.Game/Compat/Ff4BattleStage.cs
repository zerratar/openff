// The battle stage under the OpenFF battle on FF4.
//
// FF4 fights on a stage of its own: battle_map.dat holds b00..b30 (a model and its .namp
// animation each), and the map's land-form parameter names one per land form
// (world::battleMapID: the u16 at 0x18 + 2 x land form of MAPPARAMETER chain 0 - b08 for
// the Watery Pass, b01 for the Baron plain). The fight is a side view: the monsters stand
// on the left (the encounter table's placements are in these units, x across, z depth),
// the party on the right, the camera at the stage's short end looking down it.
//
// Getting there is a map jump, as FF4's own battle part is a part change: the field's
// characters, textures and scripts go with the map and come back with it (a stage swap
// under the running field, as a scene's ce_SetMap does, left the field's characters with
// dead textures on the way back). Begin issues the jump and remembers the field; the battle
// starts once the party stands on the stage (Arrived); Leave jumps back to the spot left.
// FF4's own battle camera sets (BTL_CAMERA.dat, sNN_00 CMS2 sets) are not played yet - a
// fixed camera stands in - and the sky is still the clear colour.

using System;
using OpenFF;

namespace FF3
{
	internal static class Ff4BattleStage
	{
		/// <summary>The party stands on the battle stage.</summary>
		public static bool Active { get; private set; }
		/// <summary>The jump to the battle stage is under way.</summary>
		public static bool Pending { get; private set; }
		public static int BattleMap { get; private set; } = -1;
		public static string StageName => BattleMap >= 0 ? "b" + BattleMap.ToString("00") : null;

		private static string _fieldMap;
		private static Vector3 _fieldPosition;
		private static int _fieldFacing;
		private static int _pendingFrames;

		/// <summary>Where party member <paramref name="index"/> of <paramref name="count"/> stands: the right side, staggered in depth.</summary>
		public static Vector3 PartySpot(int index, int count)
		{
			float z = (index - (count - 1) / 2f) * 18f;
			return new Vector3(42f + 4f * index, 0f, z);
		}

		/// <summary>A monster's spot from its encounter-table placement (x across from the centre, z depth); the left side.</summary>
		public static Vector3 MonsterSpot(Vector3 placement, int index, int count)
		{
			if (Math.Abs(placement.X) > 0.01f || Math.Abs(placement.Z) > 0.01f) return new Vector3(placement.X, 0f, placement.Z);
			return new Vector3(-35f - 6f * index, 0f, (index - (count - 1) / 2f) * 20f);
		}

		// The stage models run from about z -67 to +129 (b01 spans x -144..144); the camera stands
		// at the short end, a little to the party's side, looking down the stage.
		public static Vector3 CameraPosition => new Vector3(40f, 45f, -140f);
		public static Vector3 CameraTarget => new Vector3(-10f, 10f, 20f);

		/// <summary>Remembers the field and jumps to the battle stage; false when there is no such stage (the fight then stays on the field).</summary>
		public static bool Begin(int battleMap)
		{
			if (Active || Pending || battleMap < 0 || battleMap > 30) return false;
			string stage = "b" + battleMap.ToString("00");
			if (!GameArchive.Chain.Exists("files/" + stage + ".nmdp.lz") && !GameArchive.Chain.Exists(stage + ".nmdp.lz"))
			{
				Log.Write(LogChannel.General, "battle: no stage " + stage + " in the content");
				return false;
			}
			if (!EngineApi.InWorld || !Game.Hero.Present) return false;
			try
			{
				_fieldMap = Game.Field.Map;
				_fieldPosition = Game.Hero.Position;
				int rot = 0;
				try { rot = EngineApi.HeroPlayer.getRotation().y; } catch (Exception) { }
				_fieldFacing = (((int)Math.Round(rot / 8192.0)) % 8 + 8) % 8;
				if (string.IsNullOrEmpty(_fieldMap)) return false;
				BattleMap = battleMap;
				Pending = true;
				_pendingFrames = 0;
				Game.Field.Warp(stage, PartySpot(0, 1), 6);
				Log.Write(LogChannel.General, "battle: to stage " + stage + ", back to " + _fieldMap + " at " + _fieldPosition + " after");
				return true;
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "battle: stage jump failed: " + ex.Message);
				Pending = false;
				BattleMap = -1;
				return false;
			}
		}

		/// <summary>True once the party stands on the battle stage after Begin; gives up after a while.</summary>
		public static bool Arrived
		{
			get
			{
				if (!Pending) return false;
				if (++_pendingFrames > 600)
				{
					Log.Write(LogChannel.General, "battle: the stage never arrived; fighting where the party stands");
					Pending = false;
					return true;
				}
				return EngineApi.InWorld && Game.Hero.Present && string.Equals(Game.Field.Map, StageName, StringComparison.OrdinalIgnoreCase) && _pendingFrames > 2;
			}
		}

		public static void Arrive()
		{
			Pending = false;
			Active = string.Equals(Game.Field.Map, StageName, StringComparison.OrdinalIgnoreCase);
			if (!Active) BattleMap = -1;
		}

		/// <summary>Back to the field map, at the spot the party left.</summary>
		public static void Leave()
		{
			if (!Active && !Pending) return;
			Active = false;
			Pending = false;
			BattleMap = -1;
			try
			{
				GlobalScope.VecFx32 fx = new GlobalScope.VecFx32((int)Math.Round(_fieldPosition.X * 4096), (int)Math.Round(_fieldPosition.Y * 4096), (int)Math.Round(_fieldPosition.Z * 4096));
				Game.Field.Warp(JumpPart.WithChip(_fieldMap, fx), _fieldPosition, _fieldFacing);
				Log.Write(LogChannel.File, "battle: back to " + _fieldMap);
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "battle: return jump failed: " + ex.Message);
			}
		}
	}
}
