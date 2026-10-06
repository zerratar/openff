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

		/// <summary>
		/// Where the member at FF4 party position <paramref name="position"/> stands, in row <paramref name="row"/> (0 front, 1
		/// back), on party root <paramref name="rootId"/> - battle_parameter.chain's partyRoot[root][row][position], as
		/// btl::BattlePlayer::rootPosition reads it (the opening's fight: root 1, Cecil front at 1, (20, 0, 27)).
		/// </summary>
		public static Vector3 PartySpot(int rootId, int position, int row = 0)
		{
			OpenFF.Data.PartyRootSlot s = RootSlot(rootId, position, row);
			return s != null ? new Vector3(s.X, s.Y, s.Z) : FrontRow[Math.Clamp(position, 0, 4)];
		}

		/// <summary>The way that member faces, as a direction (BattlePlayer::rootRotation: the root's degrees about y; -90 faces -x, the monsters' side).</summary>
		public static Vector3 PartyFacing(int rootId, int position, int row = 0)
		{
			OpenFF.Data.PartyRootSlot s = RootSlot(rootId, position, row);
			return Facing(s != null ? s.Facing : -90f);
		}

		/// <summary>The same facing in degrees about y.</summary>
		public static float PartyFacingDegrees(int rootId, int position, int row = 0) => RootSlot(rootId, position, row)?.Facing ?? -90f;

		private static OpenFF.Data.PartyRootSlot RootSlot(int rootId, int position, int row)
		{
			OpenFF.Data.PartyRoot root = Ff4Party.Tables?.PartyRoot(rootId) ?? Ff4Party.Tables?.PartyRoot(0);
			OpenFF.Data.PartyRootSlot[] rowSlots = root?.Rows[Math.Clamp(row, 0, 1)];
			return rowSlots?[Math.Clamp(position, 0, 4)];
		}

		/// <summary>A direction from degrees about y, the battle tables' facings.</summary>
		public static Vector3 Facing(float degrees)
		{
			double r = degrees * Math.PI / 180.0;
			return new Vector3((float)Math.Sin(r), 0f, (float)Math.Cos(r));
		}

		/// <summary>A monster's spot: the encounter group's own placement as it is (x, y, z on the stage - party 900's Floating Eyes at (5, 5, -15) and (-15, 0, 5), as Steam stands them), else a row there.</summary>
		public static Vector3 MonsterSpot(Vector3 placement, int index, int count)
		{
			if (Math.Abs(placement.X) > 0.01f || Math.Abs(placement.Y) > 0.01f || Math.Abs(placement.Z) > 0.01f) return placement;
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
		public static Vector3 OpeningPosition => new Vector3(0f, 43f, 166f);   // readyOpeningCamera's (0, 0x2B000, 0xA6000)
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
				Ff4MapChange.PartJump();   // FF4's battle is a part change: the screen stays as the encounter left it (white)
				Game.Field.Warp(stage, PartySpot(0, 1), 6);
				Log.Write(LogChannel.General, "battle: to stage " + stage + ", back to " + _fieldMap + " at " + _fieldPosition + " after (step " + LegacyStep.Count + ")");
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
				return EngineApi.InWorld && Game.Hero.Present && string.Equals(Game.Field.Map, StageName, StringComparison.OrdinalIgnoreCase) && _pendingFrames > 0;
			}
		}

		public static void Arrive()
		{
			Pending = false;
			Active = string.Equals(Game.Field.Map, StageName, StringComparison.OrdinalIgnoreCase);
			if (!Active) BattleMap = -1;
		}

		/// <summary>Back to the field map, at the spot the party left - unless <paramref name="jumpBack"/> is false, when what follows the fight jumps on itself (a scene's battle goes on to its return map).</summary>
		public static void Leave(bool jumpBack = true)
		{
			if (!Active && !Pending) return;
			Active = false;
			Pending = false;
			BattleMap = -1;
			if (!jumpBack) return;
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
