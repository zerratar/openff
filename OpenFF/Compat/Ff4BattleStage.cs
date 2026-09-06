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

namespace OpenFF.Client
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

		// FF4's own spots (battle_parameter.chain chain 0, record 0, the front row), the stand-in when
		// the tables lack them: x 17..19, z from -25 at the top of the screen to 50 at the bottom.
		private static readonly Vector3[] FrontRow = { new Vector3(18f, 0f, -25f), new Vector3(17f, 0f, -5f), new Vector3(19f, 0f, 12f), new Vector3(17f, 0f, 35f), new Vector3(19f, 0f, 50f) };

		/// <summary>Where party member <paramref name="index"/> stands: FF4's party root for the normal fight (OpenFF.Data.PartyRoot 0), the front row unless <paramref name="backRow"/>; slot 0 at the top of the screen.</summary>
		public static Vector3 PartySpot(int index, int count, bool backRow = false)
		{
			int slot = Math.Clamp(index, 0, 4);
			OpenFF.Data.PartyRoot root = Ff4Party.Tables?.PartyRoot(0);
			OpenFF.Data.PartyRootSlot s = root != null && root.Rows[backRow ? 1 : 0] != null ? root.Rows[backRow ? 1 : 0][slot] : null;
			return s != null ? new Vector3(s.X, s.Y, s.Z) : FrontRow[slot];
		}

		/// <summary>The way a member faces at their spot: FF4's root says -90 degrees, towards -x and the monsters.</summary>
		public static Vector3 PartyFacing(int index)
		{
			OpenFF.Data.PartyRoot root = Ff4Party.Tables?.PartyRoot(0);
			float degrees = root != null && root.Rows[0] != null ? root.Rows[0][Math.Clamp(index, 0, 4)].Facing : -90f;
			double r = degrees * Math.PI / 180.0;
			return new Vector3((float)Math.Sin(r), 0f, (float)Math.Cos(r));
		}

		/// <summary>A monster's spot: the encounter table's own placement (stage units: x -8..-37 towards the monsters' side, z -35..32 along the line), else a row there.</summary>
		public static Vector3 MonsterSpot(Vector3 placement, int index, int count)
		{
			if (Math.Abs(placement.X) > 0.01f || Math.Abs(placement.Z) > 0.01f) return new Vector3(placement.X, 0f, placement.Z);
			return new Vector3(-22f - 8f * index, 0f, (index - (count - 1) / 2f) * 30f);
		}

		// FF4's battle camera, read from libff4.so (btl::CBattleDisplay, Tools/ff4_disasm.py and the
		// data symbols): initialize sets the field of view to 641/4046 (18 degrees) and the clip to
		// 10..2000; readyOpeningCamera puts the camera at (0, 32.7, 166) looking at (0, 0, -34) and
		// goOpeningCamera eases it a fifth of the way per frame for five frames, then snaps to
		// CAMERA_BATTLE_POSITION[type] looking at CAMERA_BATTLE_TARGET[type], type being byte 3 of
		// the encounter group's record (0 for 515 of the 520 groups). So the standing view is a long
		// shot from z 240, 45 up, looking a little down the stage towards -z at the backdrop (b01's
		// mountains and clouds stand along the far edge at z about -67): the monsters left (x -8..-37
		// in the tables' own placements), the party right. BTL_CAMERA.dat's CMS2 sets (s00_00..) are
		// the ability and summon cameras (ds::sys3d::CameraHandle), not this one.
		private static readonly Vector3[] BattlePositions = { new Vector3(0f, 45f, 240f), new Vector3(0f, 36f, 117.6f), new Vector3(0f, 45f, 240f) };
		private static readonly Vector3[] BattleTargets = { new Vector3(0f, -5f, -20f), new Vector3(0f, -10f, -44f), new Vector3(0f, -3f, -40f) };
		public static Vector3 CameraPosition(int type) => BattlePositions[type >= 0 && type < BattlePositions.Length ? type : 0];
		public static Vector3 CameraTarget(int type) => BattleTargets[type >= 0 && type < BattleTargets.Length ? type : 0];
		public static Vector3 OpeningPosition => new Vector3(0f, 32.7f, 166f);
		public static Vector3 OpeningTarget => new Vector3(0f, 0f, -34f);
		public const int OpeningFrames = 6;
		public const float CameraFov = 18f;
		public const float ClipNear = 10f, ClipFar = 2000f;

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
