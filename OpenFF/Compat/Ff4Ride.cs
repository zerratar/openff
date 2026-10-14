// FF4's vehicles ridden on the world maps, as libff4's world states run them:
//   WSPrepare::wsProcessCreateVehicle  each type's model (object::createVO: p18_00 the hovercraft, p17_00 the Enterprise,
//                                      p20_00 / p21_00 the Falcon, p19_00 the Lunar Whale, p15_00 the chocobo, p16_00 the
//                                      black chocobo; their motion packs v00, v02, v05 / v06, v07, v04, v03 and scales) put
//                                      where Ff4Vehicles has it, shown when the party has it (checkAcquiredVehicle) and it
//                                      stays on this world (f00 the Earth, f01 the Underground, f02 the Moon); the chocobo
//                                      only when the party arrives riding it
//   VehicleObject::collision           walking within 10 of a vehicle the party has boards it (WSIntoVehicle: the lead slides
//                                      onto it over 30 frames, turning to it and fading out; the field BGM stops but for the
//                                      hovercraft and the chocobo)
//   WSVehicleTakeOff                   up to its height over 30 frames (2 the hovercraft, 25 the airships, 20 the black
//                                      chocobo), the camera easing to the flying one (not for the hovercraft), the airship
//                                      theme (BGM 30) for the Enterprise and the Falcon; 60 frames in all
//   WSVehicleMove::wsProcessVehicle    the pad steers it (the camera's axes), at its speed (2, 3.5, 4, 3, 2, 2 a frame),
//                                      turning towards the way it goes by a step growing 0x800 a frame; the walls of its
//                                      attributes (dgsredAccept / collisionWall_: 2 3 6 8 the hovercraft, 8 the airships -
//                                      the Falcon 8 and 7 until flag 902 -, 2 4 6 8 the chocobo, 8 9 2 the black chocobo)
//                                      stop it; it keeps its height in the air, follows the ground on it; the world wraps;
//                                      no encounters; Tab is the menu and M the map as on foot
//   checkGetOff / WSVehicleLanding     Enter gets off where the land form allows it (the map's land form table: 0 or 2 the
//                                      hovercraft, 0 or 7 the airships, 0 or 8 the Lunar Whale, under 3 the chocobo, 1 -
//                                      a forest - the black chocobo; never where the ground's flag 20 is up), else the
//                                      buzzer; down over 30 frames, the camera back, the world's BGM again (7 on the Earth,
//                                      35 below); WSGetOffVehicle puts the lead where it stood, fading in over 20 frames,
//                                      and the vehicle stays; WSChocoboEscape: the chocobo runs off
// The lead stays the camera's anchor while riding, unseen at the vehicle's place, so the field camera and the chips
// streaming in follow the vehicle. Still to come: the Lunar Whale's inside (t21_00), the hole to the Underground, the
// Enterprise's hook for the hovercraft, the black chocobo's flight home, the sounds.

using System;
using System.Collections.Generic;
using OpenFF;

namespace OpenFF.Client
{
	internal sealed class Ff4Ride : GameService
	{
		private static Ff4Ride _instance;
		public Ff4Ride() { _instance = this; }

		public override bool WantsUpdate => true;

		private enum State { Foot, Boarding, TakeOff, Riding, Landing, GettingOff, ChocoboOff, Descending, Leaving, LiftUp, LiftOff, HomeComing, Hole }

		private static State _state = State.Foot;

		/// <summary>Aboard a vehicle (boarding, flying, landing): no encounters, no saving, no map entrances.</summary>
		public static bool Riding => _state != State.Foot;

		/// <summary>A vehicle's step under way (boarding, taking off, landing, getting off): the menu and the map wait.</summary>
		public static bool Busy => _state != State.Foot && _state != State.Riding;

		// ---- each type's numbers (libff4) ----

		private static readonly string[] Models = { "p18_00", "p17_00", "p20_00", "p19_00", "p15_00", "p16_00" };
		private static readonly string[] Motions = { "v00", "v02", "v05", "v07", "v04", "v03" };
		private static readonly int[] Scales = { 0x1333, 0x1000, 0x1000, 0x1000, 0xe66, 0xe66 };
		private static readonly int[] Speeds = { 0x2000, 0x3800, 0x4000, 0x3000, 0x2000, 0x2000 };
		private static readonly int[] Heights = { 0x2000, 0x19000, 0x19000, 0x19000, 0, 0x14000 };
		private const int BoardDistance = 0xa000, WallRadius = 0x3000;
		private const int AirshipBgm = 30, EarthBgm = 7, UnderworldBgm = 35;

		private static bool Flag(int index) => GlobalScope.FlagManager.singleton().get(0, (uint)index) != 0;

		/// <summary>The Falcon's look: p21_00 / v06 once both its flags are up (904, 905).</summary>
		private static string Model(int type) => type == 2 && Flag(904) && Flag(905) ? "p21_00" : Models[type];
		private static string MotionPack(int type) => type == 2 && Flag(904) && Flag(905) ? "v06" : Motions[type];

		private static int[] Walls(int type)
		{
			switch (type)
			{
				case 0: return new[] { 2, 3, 6, 8 };
				case 2: return Flag(902) ? new[] { 8 } : new[] { 8, 7 };
				case 4: return new[] { 2, 4, 6, 8 };
				case 5: return new[] { 8, 9, 2 };
				default: return new[] { 8 };
			}
		}

		/// <summary>libff4's motion numbers as the port registers FF4's field motions (GameProfile.FieldMotionId).</summary>
		private static int Motion(int raw) => (int)GameProfile.FieldMotionId((uint)raw);

		// ---- the vehicles on this map ----

		private sealed class Vehicle
		{
			public int Type, Ctrl = -1;
			public int X, Y, Z, RotY;
			/// <summary>The ground under it (VO +0x364), for its shadow.</summary>
			public int GroundY;
			public bool Shown;
		}

		private static readonly Vehicle[] _vehicles = new Vehicle[6];
		private static int _world = -1;
		private static Vehicle _ride;

		private static GlobalScope.CCharacterMng Characters => GlobalScope.characterMng;

		/// <summary>mapname2stayworld: f00 0, f01 1, f02 2; -1 for any other map.</summary>
		private static int WorldOf(string stage)
		{
			if (string.IsNullOrEmpty(stage) || stage.Length != 3 || (stage[0] != 'f' && stage[0] != 'F')) return -1;
			int w = stage[2] - '0';
			return stage[1] == '0' && w >= 0 && w <= 2 ? w : -1;
		}

		public override void OnSceneUnloading(SceneInfo scene)
		{
			// The field's own setup sweeps the characters away with the map.
			for (int i = 0; i < 6; i++) _vehicles[i] = null;
			_tweens.Clear();
			_world = -1;
			if (_state != State.Foot)
			{
				Log.Write(LogChannel.General, "ride: the map left aboard " + (_ride?.Type ?? -1));
				// The keys are the next map's (the magiship's inside, the world arrived aboard takes them again).
				OpenFF.Game.Input.Capture = false;
				StopEngine();
			}
			_ride = null;
			_state = State.Foot;
		}

		public override void OnSceneLoaded(SceneInfo scene)
		{
			if (!GameProfile.IsFf4) return;
			_world = WorldOf(GlobalScope.stg.CStageMng.CurrentName);
			if (_world < 0) { Ff4Vehicles.ArriveOn = -1; return; }
			OpenFF.Game.Guard("ride: vehicles", CreateVehicles);
		}

		private static void CreateVehicles()
		{
			int arrive = Ff4Vehicles.ArriveOn;
			Ff4Vehicles.ArriveOn = -1;
			if (arrive >= 0)
			{
				// The vehicle the party arrives on stays on this world from now (wscSetVehicleStayingMap); it stands where it was
				// left (the scripts place it first - Steam's Enterprise flew from its stored spot, not the warp's).
				Ff4Vehicles.SetStay(arrive, _world);
			}
			for (int type = 0; type < 6; type++)
			{
				if (type == 4 && arrive != 4) continue;
				if (type == 5 && arrive != 5 && !Ff4Vehicles.Acquired(5)) continue;
				Vehicle v = Spawn(type);
				if (v == null) continue;
				_vehicles[type] = v;
				v.Shown = type == arrive || (Ff4Vehicles.Acquired(type) && Ff4Vehicles.Stay(type) == _world);
				Characters.setHidden(v.Ctrl, !v.Shown);
			}
			// Carried: the hovercraft is wherever the Enterprise is (WTSynchronizeEpHover), on its world.
			if (Ff4Vehicles.LiftHover && _vehicles[0] != null && _vehicles[1] != null)
			{
				Vehicle h = _vehicles[0], e = _vehicles[1];
				h.Shown = e.Shown;
				Characters.setHidden(h.Ctrl, !h.Shown);
				h.X = e.X; h.Y = Heights[1] - 0x4493; h.Z = e.Z; h.RotY = e.RotY;
				Place(h);
			}
			if (arrive >= 0 && _vehicles[arrive] != null) Arrive(_vehicles[arrive]);
		}

		private static Vehicle Spawn(int type)
		{
			int[] place = Ff4Vehicles.Place(type);
			string model = Model(type);
			GlobalScope.TexDivideLoader.getSingleton().tdlForceLoad();
			int ctrl = Characters.setCharacterWithTexture(model, model, GlobalScope.CCharacterMng.PRI_SCENE.PRI_SCENE_FIRST);
			if (ctrl < 0) ctrl = Characters.setCharacter(model, GlobalScope.CCharacterMng.PRI_SCENE.PRI_SCENE_FIRST);
			GlobalScope.TexDivideLoader.getSingleton().tdlForceLoad();
			if (ctrl < 0)
			{
				Log.Write(LogChannel.General, "ride: " + model + " did not load");
				return null;
			}
			Characters.addMotion(ctrl, MotionPack(type));
			Characters.setScale(ctrl, new GlobalScope.VecFx32(Scales[type], Scales[type], Scales[type]));
			Vehicle v = new Vehicle { Type = type, Ctrl = ctrl, X = place[0], Y = place[1], Z = place[2], RotY = place[4], GroundY = place[1] };
			Place(v);
			// The black chocobo stands on its idle (createVO's startMotion 0x3ea); the others hold still until boarded.
			if (type == 5) Characters.startMotion(ctrl, Motion(0x3ea), true, 0);
			Log.Write(LogChannel.File, "ride: " + model + " (type " + type + ") at " + v.X / 4096 + "," + v.Y / 4096 + "," + v.Z / 4096);
			return v;
		}

		private static readonly int[] ShadowX = { 6144, 5325, 5325, 13517, 5325, 5325 };
		private static readonly int[] ShadowZ = { 10240, 13517, 13517, 27853, 5325, 5325 };

		private static void Place(Vehicle v)
		{
			Characters.setPosition(v.Ctrl, new GlobalScope.VecFx32(v.X, v.Y, v.Z));
			Characters.setRotation(v.Ctrl, 0, (ushort)v.RotY, 0);
			// VehicleObject::shadowUpdate: the shadow on the ground under it (its height the ground's less the vehicle's, the
			// ground 0x1800 under the model as standing), smaller the higher it flies - half its size at the airships' 25.
			long t = ((long)v.Y << 12) / 0x19000;
			long k = 0x1000 - ((t + 1) >> 1);
			int sx = (int)((ShadowX[v.Type] * k + 0x800) >> 12), sz = (int)((ShadowZ[v.Type] * k + 0x800) >> 12);
			Characters.setShadowEnable(v.Ctrl, sx > 3);
			Characters.setShadowScale(v.Ctrl, new GlobalScope.VecFx32(sx, 0, sz));
			Characters.setShadowHeight(v.Ctrl, (v.GroundY > 0x13fff ? 0x1800 : v.GroundY + 0x1800) - v.Y);
		}

		// ---- each step ----

		private static GlobalScope.pl.CBasePlayer Hero => EngineApi.HeroPlayer;
		private static int _frame, _boardFrom, _lastDist = int.MaxValue;
		private static int _fromX, _fromY, _fromZ, _toY, _heroFromRot;
		private static GlobalScope.VecFx32 _camPos, _camTrg, _camPosFrom, _camTrgFrom, _camPosTo, _camTrgTo;
		private static int _turnStep = 0x800;
		private static bool _wasMoving;

		/// <summary>The game's steps (LegacyStep): the vehicles' states advance once each.</summary>
		public static void Step()
		{
			bool a = _latchA, b = _latchB;
			_latchA = _latchB = false;
			if (!GameProfile.IsFf4 || !EngineApi.InWorld || Hero == null) return;
			// Arriving aboard (a worldMapWarp's vehicle): the lead must not walk the world's exits before the ride takes it - the
			// chocobo forests set the party down on their own entrance.
			if (Ff4Vehicles.ArriveOn >= 0 && _world < 0 && WorldOf(GlobalScope.stg.CStageMng.CurrentName) >= 0) Hero.setMCLCol(false);
			if (_world < 0) return;
			_pressA = a; _pressB = b;
			if (_tweens.Count > 0) StepCastTweens();
			try
			{
				switch (_state)
				{
					case State.Foot: CheckBoard(); break;
					case State.Boarding: Boarding(); break;
					case State.TakeOff: TakeOff(); break;
					case State.Riding: Ride(); break;
					case State.Landing: Landing(); break;
					case State.GettingOff: GettingOff(); break;
					case State.ChocoboOff: ChocoboOff(); break;
					case State.Descending: Descending(); break;
					case State.LiftUp: LiftUp(); break;
					case State.LiftOff: LiftOff(); break;
					case State.HomeComing: HomeComing(); break;
					case State.Hole: Hole(); break;
				}
			}
			catch (Exception ex)
			{
				Log.First(LogChannel.General, "ride-step", 3, () => "ride: " + _state + ": " + ex);
			}
		}

		private static Vehicle _inReach;

		private static bool FreeOnFoot() => !Ff4Cutscene.Active && !Ff4Menu.EventBusy() && !(Ff4Menu.Instance?.IsOpen ?? false)
			&& !OpenFF.Game.Dialogue.IsOpen && !Ff4Battle.Active && !Ff4Extras.Busy && !Ff4Chests.Busy && !EncounterZoom.Playing
			&& !BlurRotate.Playing && !(Ff4NaviMap.Instance?.IsOpen ?? false);

		/// <summary>VehicleObject::collision: the lead within 10 of a vehicle the party has, walking towards it.</summary>
		private static void CheckBoard()
		{
			GlobalScope.VecFx32 at = Hero.getPosition();
			Vehicle near = null;
			int best = int.MaxValue;
			foreach (Vehicle v in _vehicles)
			{
				if (v == null || !v.Shown || !Ff4Vehicles.Acquired(v.Type)) continue;
				int d = Distance(at.x, at.y, at.z, v.X, v.Y, v.Z);
				if (d < best) { best = d; near = v; }
			}
			// Steam's: in reach (VehicleObject::collision's 10), the "!" balloon over the lead and Enter boards - walking
			// through a vehicle does nothing (Steam's frames: Cecil walked through the Enterprise, then boarded it on Enter).
			_inReach = near != null && best < BoardDistance && FreeOnFoot() ? near : null;
			if (_inReach != null && _pressA) Board(_inReach);
		}

		private static int Distance(int ax, int ay, int az, int bx, int by, int bz)
		{
			double dx = ax - bx, dy = ay - by, dz = az - bz;
			return (int)Math.Sqrt(dx * dx + dy * dy + dz * dz);
		}

		private static void Board(Vehicle v)
		{
			_inReach = null;
			_arrived = false;
			_ride = v;
			_state = State.Boarding;
			_frame = 0;
			OpenFF.Game.Input.Capture = true;
			GlobalScope.VecFx32 at = Hero.getPosition();
			_fromX = at.x; _fromY = at.y; _fromZ = at.z;
			_heroFromRot = Hero.getRotation().y;
			Hero.setMCLCol(false);
			// WSIntoVehicle: the field's BGM stops - not for the hovercraft or the chocobo, nor for the Lunar Whale (flag 445) or
			// the Enterprise (446) once those are up.
			bool keep = v.Type == 0 || v.Type == 4 || (v.Type == 3 && Flag(445)) || (v.Type == 1 && Flag(446));
			if (!keep) StopBgm(15);
			Log.Write(LogChannel.General, "ride: boarding " + Model(v.Type) + " (type " + v.Type + ")");
		}

		private static void Boarding()
		{
			_frame++;
			float t = Math.Min(1f, _frame / 30f);
			int x = _fromX + (int)((_ride.X - _fromX) * t), y = _fromY + (int)((_ride.Y - _fromY) * t), z = _fromZ + (int)((_ride.Z - _fromZ) * t);
			Hero.setPosition(new GlobalScope.VecFx32(x, y, z));
			int face = Yaw(_ride.X - _fromX, _ride.Z - _fromZ);
			Hero.setRotation(new GlobalScope.VecFx32(0, face, 0));
			HeroAlpha((int)Math.Round(31 * (1 - t)));
			if (_frame < 30) return;
			Hero.setHidden(true);
			if (_ride.Type == 3)
			{
				// WSIntoVehicle::wsProcessEnd for the magiship: into its inside (t21_00 at -23, 0, 24.1), its place kept; the field's
				// music stops unless flag 445 keeps it.
				Ff4Vehicles.SetPlace(3, _ride.X, _ride.Y, _ride.Z, _ride.RotY);
				if (!Flag(445)) StopBgm(15);
				JumpTo("t21_00", -0x17066, 0, 0x18204);
				return;
			}
			StartTakeOff();
		}

		private static void StartTakeOff()
		{
			_state = State.TakeOff;
			_frame = 0;
			_fromY = _ride.Y;
			_toY = _ride.Type == 4 ? _ride.Y : Heights[_ride.Type];
			if (_ride.Type != 4) Characters.setMotionPause(_ride.Ctrl, false);
			// WSVehicleTakeOff::wsProcessInit's sound: the hovercraft's 5/1, the airships' 6/0, the magiship's 7/0, the black
			// chocobo's 2/0 (not when arrived on); the chocobo none.
			switch (_ride.Type)
			{
				case 0: PlaySe(5, 1); break;
				case 1: case 2: PlaySe(6, 0); break;
				case 3: PlaySe(7, 0); break;
				case 5: if (!_arrived) PlaySe(2, 0); break;
			}
			if (Characters.isMotion(_ride.Ctrl, Motion(0x3e9))) Characters.startMotion(_ride.Ctrl, Motion(0x3e9), true, 2);
			// The camera to the flying one (WTWCameraLinearMoveByFrame to 0x2bc9c4 / 0x2bc9d0), not for the hovercraft.
			if (_ride.Type != 0 && _ride.Type != 4) CameraTo(0x6e000, 0x87000, -0x42000, -0x50000);
			// The airship theme for the Enterprise (until flag 446 or 447; 447 silences the field instead) and the Falcon.
			if (_ride.Type == 2 || (_ride.Type == 1 && !Flag(446) && !Flag(447))) PlayBgm(AirshipBgm);
			else if (_ride.Type == 1 && Flag(447)) StopBgm(0);
		}

		private static void TakeOff()
		{
			_frame++;
			if (_frame <= 30)
			{
				float t = _frame / 30f;
				_ride.Y = _fromY + (int)((_toY - _fromY) * t);
				Place(_ride);
				FollowHero();
			}
			StepCamera(_frame, 30);
			if (_frame < 60) return;
			if (_ride.Type == 5 && !_arrived) { StartHomeComing(); return; }
			_state = State.Riding;
			_turnStep = 0x800;
			StartEngine();
			Log.Write(LogChannel.General, "ride: aboard type " + _ride.Type);
		}

		/// <summary>WSPrepare for a vehicle arrived on (a worldMapWarp's): aboard at once, in the air for an airship.</summary>
		private static void Arrive(Vehicle v)
		{
			_ride = v;
			Hero.setMCLCol(false);
			Hero.setHidden(true);
			OpenFF.Game.Input.Capture = true;
			if ((v.Type == 1 || v.Type == 2) && Ff4Vehicles.WorldJump >= 0)
			{
				// WSDiveFromHole / WSRiseFromHole: out of the hole, turned to 0xC000, 15 down (or up) to the flying height at
				// 0.25 a frame while the screen fades in over 60 frames.
				bool down = Ff4Vehicles.WorldJump == 0;
				Ff4Vehicles.WorldJump = -1;
				v.X = down ? 2308915 : -784261; v.Z = down ? -2700780 : 4574823;
				v.Y = down ? 0x28000 : 0xa000;
				v.RotY = 0xC000;
				_fromY = v.Y;
				_toY = Heights[v.Type];
				_settleStep = 0x400;
				Characters.setMotionPause(v.Ctrl, false);
				if (Characters.isMotion(v.Ctrl, Motion(0x3e9))) Characters.startMotion(v.Ctrl, Motion(0x3e9), true, 0);
				CameraTo(0x6e000, 0x87000, -0x42000, -0x50000);
				StepCamera(1, 1);
				Place(v);
				FollowHero();
				GlobalScope.dgs.CFade.Main().fadeIn(60);
				GlobalScope.dgs.CFade.Sub().fadeIn(60);
				_state = State.Descending;
				_frame = 0;
				Log.Write(LogChannel.General, "ride: out of the hole " + (down ? "into the Underworld" : "onto the Overworld"));
				return;
			}
			if (v.Type == 5)
			{
				// WSPrepare::wsProcessStateScheduling: the black chocobo arrived on takes off from where it stands ("takeoff").
				Characters.startMotion(v.Ctrl, Motion(0x3e9), true, 0);
				_ride.GroundY = v.Y;
				Place(v);
				FollowHero();
				_arrived = true;
				StartTakeOff();
				return;
			}
			if (v.Type == 3)
			{
				// WSPrepare::wsProcessStateScheduling for the magiship: the trip between the worlds comes down from 60 above its
				// place (WSFromEarthToMoon / WSFromMoonToEarth); otherwise it takes off where it stands ("takeoff").
				int trip = Ff4Vehicles.MoonTrip;
				Ff4Vehicles.MoonTrip = -1;
				Characters.setMotionPause(v.Ctrl, false);
				if (Characters.isMotion(v.Ctrl, Motion(0x3e9))) Characters.startMotion(v.Ctrl, Motion(0x3e9), true, 0);
				_ride.GroundY = v.Y;
				FollowHero();
				if (trip >= 0)
				{
					_fromY = Heights[3] + 0x3c000;
					_toY = Heights[3];
					v.Y = _fromY;
					Place(v);
					FollowHero();
					CameraTo(0x6e000, 0x87000, -0x42000, -0x50000);
					StepCamera(1, 1);
					_state = State.Descending;
					_frame = 0;
					Log.Write(LogChannel.General, "ride: the magiship arrives from " + (trip == 0 ? "the Earth" : "the Moon"));
					return;
				}
				Place(v);
				_arrived = true;
				StartTakeOff();
				Log.Write(LogChannel.General, "ride: arrived aboard the magiship - it takes off");
				return;
			}
			if (v.Type != 0 && v.Type != 4)
			{
				v.Y = Heights[v.Type];
				CameraTo(0x6e000, 0x87000, -0x42000, -0x50000);
				StepCamera(1, 1);
			}
			Characters.setMotionPause(v.Ctrl, false);
			if (Characters.isMotion(v.Ctrl, Motion(0x3e9))) Characters.startMotion(v.Ctrl, Motion(0x3e9), true, 0);
			Place(v);
			FollowHero();
			_state = State.Riding;
			_turnStep = 0x800;
			// Carrying the hovercraft: the Enterprise flies with it (1004).
			if (v.Type == 1 && Ff4Vehicles.LiftHover) { Characters.startMotion(v.Ctrl, Motion(0x3ec), true, 0); CarryHover(); }
			StartEngine();
			Log.Write(LogChannel.General, "ride: arrived aboard type " + v.Type);
		}

		// ---- riding ----

		private static void Ride()
		{
			bool menu = Ff4Menu.Instance?.IsOpen ?? false;
			bool map = Ff4NaviMap.Instance?.IsOpen ?? false;
			if (menu) return;
			// The menu gives the keys back when it shuts; aboard they stay the vehicle's.
			OpenFF.Game.Input.Capture = true;
			InputState input = OpenFF.Game.Input;
			// Steam's key bar aboard: Backspace Dismount (babil_menu 60208, key_assign01); Enter is the Enterprise's hook (60209/10).
			if (!map && _pressB) { TryGetOff(); return; }
			// WSVehicleMove::wsProcess: Enter on the Enterprise with its hook (flag 903) raises the hovercraft in reach, or lowers
			// the one it carries where it may stand.
			if (!map && _pressA && _ride.Type == 1)
			{
				if (CanRaise()) { StartLiftUp(); return; }
				if (CanLower(out int y)) { StartLiftOff(y); return; }
			}
			Steer(input);
			if (Ff4Vehicles.LiftHover && _ride.Type == 1) CarryHover();
		}

		// ---- the Enterprise's hook (WSHoverLiftUp / WSHoverLiftOff, WTSynchronizeEpHover) ----

		private static Vehicle Hover => _vehicles[0];

		/// <summary>wsProcessEpHoverLiftup's test: the hook (flag 903), the hovercraft not carried, here and within 10 across.</summary>
		private static bool CanRaise()
		{
			Vehicle h = Hover;
			if (_ride == null || _ride.Type != 1 || !Flag(903) || Ff4Vehicles.LiftHover || h == null || !h.Shown) return false;
			long dx = _ride.X - h.X, dz = _ride.Z - h.Z;
			return dx * dx + dz * dz < (long)0xa000 * 0xa000;
		}

		/// <summary>wsProcessEpHoverLiftoff's test: carrying it, over ground of the kinds 0, 2 or 7, the magiship and the Falcon
		/// not within 20 across.</summary>
		private static bool CanLower(out int groundY)
		{
			groundY = 0;
			if (_ride == null || _ride.Type != 1 || !Ff4Vehicles.LiftHover) return false;
			foreach (int other in new[] { 3, 2 })
			{
				Vehicle o = _vehicles[other];
				if (o == null || !o.Shown) continue;
				long dx = _ride.X - o.X, dz = _ride.Z - o.Z;
				if (dx * dx + dz * dz < (long)0x14000 * 0x14000) return false;
			}
			int kind = GroundKind(_ride, out groundY, out _, false);
			return kind == 0 || kind == 2 || kind == 7;
		}

		/// <summary>WTSynchronizeEpHover: carried, the hovercraft hangs 4.29 under the Enterprise, turned as it is.</summary>
		private static void CarryHover()
		{
			Vehicle h = Hover;
			if (h == null || _ride == null) return;
			h.X = _ride.X; h.Y = _ride.Y - 0x4493; h.Z = _ride.Z; h.RotY = _ride.RotY;
			h.Shown = true;
			Characters.setHidden(h.Ctrl, false);
			Place(h);
		}

		private static int _liftPhase, _liftY;

		private static void StartLiftUp()
		{
			Vehicle h = Hover;
			_state = State.LiftUp;
			_liftPhase = 0;
			_frame = 0;
			_fromX = _ride.X; _fromY = _ride.Y; _fromZ = _ride.Z;
			_liftY = h.Y;
			// WSHoverLiftUp::wsInitialize: carried from now (the flag), flag 200 up.
			Ff4Vehicles.LiftHover = true;
			GlobalScope.FlagManager.singleton().set(0, 200);
			StopEngine();
			Log.Write(LogChannel.General, "ride: the hook raises the hovercraft");
		}

		/// <summary>Over it in 15 frames, turned to it; 5 more turning to its heading; the hook down (1002, sound 6/3); then up
		/// (1003) with it on the hook's locator4; then flying with it (1004) - Steam's: 15 + 5, 60, 82 frames.</summary>
		private static void LiftUp()
		{
			Vehicle h = Hover;
			_frame++;
			switch (_liftPhase)
			{
				case 0:
				{
					float t = Math.Min(1f, _frame / 15f);
					_ride.X = _fromX + (int)((h.X - _fromX) * t);
					_ride.Z = _fromZ + (int)((h.Z - _fromZ) * t);
					_ride.Y = _fromY + (int)((h.Y + 0x19000 - _fromY) * t);
					if (_fromX != h.X || _fromZ != h.Z) Turn(Yaw(h.X - _fromX, h.Z - _fromZ));
					Place(_ride);
					FollowHero();
					if (_frame >= 15) { _liftPhase = 1; _frame = 0; }
					break;
				}
				case 1:
					Turn(h.RotY);
					Place(_ride);
					FollowHero();
					if (_frame >= 5)
					{
						Characters.startMotion(_ride.Ctrl, Motion(0x3ea), false, 0);
						PlaySe(6, 3);
						_liftPhase = 2; _frame = 0;
					}
					break;
				case 2:
					if (_frame < 2 || !Characters.isEndOfMotion(_ride.Ctrl)) break;
					Characters.startMotion(_ride.Ctrl, Motion(0x3eb), false, 0);
					ReserveHook();
					_liftPhase = 3; _frame = 0;
					break;
				case 3:
					HangOnHook(h, _liftY, _ride.Y - 0x4493, 66);
					if (_frame < 2 || !Characters.isEndOfMotion(_ride.Ctrl)) break;
					Characters.startMotion(_ride.Ctrl, Motion(0x3ec), true, 0);
					CarryHover();
					_state = State.Riding;
					_turnStep = 0x800;
					StartEngine();
					break;
			}
		}

		private static void StartLiftOff(int groundY)
		{
			_state = State.LiftOff;
			_liftPhase = 0;
			_frame = 0;
			_liftY = groundY;
			// WSVehicleMove: the flag down before "liftoff"; WSHoverLiftOff: down on the hook (1005, sound 6/4).
			Ff4Vehicles.LiftHover = false;
			StopEngine();
			Characters.startMotion(_ride.Ctrl, Motion(0x3ed), false, 0);
			PlaySe(6, 4);
			ReserveHook();
			Log.Write(LogChannel.General, "ride: the hook lowers the hovercraft");
		}

		/// <summary>Down on the hook (1005), set on the ground (its place and heading kept), the hook back up (1006), flying.</summary>
		private static void LiftOff()
		{
			Vehicle h = Hover;
			_frame++;
			if (_liftPhase == 0)
			{
				HangOnHook(h, _ride.Y - 0x4493, _liftY, 60);
				if (_frame < 2 || !Characters.isEndOfMotion(_ride.Ctrl)) return;
				h.Y = _liftY;
				Place(h);
				Ff4Vehicles.SetPlace(0, h.X, h.Y, h.Z, h.RotY);
				Ff4Vehicles.SetStay(0, _world);
				Characters.startMotion(_ride.Ctrl, Motion(0x3ee), false, 0);
				_liftPhase = 1;
				_frame = 0;
				return;
			}
			if (_frame < 2 || !Characters.isEndOfMotion(_ride.Ctrl)) return;
			if (Characters.isMotion(_ride.Ctrl, Motion(0x3e9))) Characters.startMotion(_ride.Ctrl, Motion(0x3e9), true, 2);
			_state = State.Riding;
			_turnStep = 0x800;
			StartEngine();
		}

		private static void ReserveHook()
		{
			Characters.initJntMtx(_ride.Ctrl);
			Characters.reserveToGetJntMtx(_ride.Ctrl, "locator4");
		}

		/// <summary>The hovercraft on the hook's locator4 (its matrix, scaled to the hovercraft's size) while the motion plays; with
		/// no joint to read, from one height to the other over the motion's frames.</summary>
		private static void HangOnHook(Vehicle h, int fromY, int toY, int frames)
		{
			GlobalScope.MtxFx43 m = new GlobalScope.MtxFx43();
			GlobalScope.MTX_Identity43(m);
			if (Characters.getJntMtx(_ride.Ctrl, "locator4", m))
			{
				GlobalScope.MTX_ScaleApply43(m, m, Scales[0], Scales[0], Scales[0]);
				Characters.setPoseMtx(h.Ctrl, m);
				h.X = m._30; h.Y = m._31; h.Z = m._32;
				ReserveHook();
				return;
			}
			float t = Math.Min(1f, _frame / (float)frames);
			h.X = _ride.X; h.Z = _ride.Z; h.RotY = _ride.RotY;
			h.Y = fromY + (int)((toY - fromY) * t);
			Place(h);
		}

		// ---- the black chocobo flying home (WSBCHomeComming, WSVehicleLandingBCHomeComming) ----

		private static int _homeX, _homeZ, _homeY;

		/// <summary>Its theme (32), then straight for its home (the nearest of the point and its copies a world's size on, as
		/// FUN_004cf408 picks it) at 3 a frame, turned to it.</summary>
		private static void StartHomeComing()
		{
			_state = State.HomeComing;
			_frame = 0;
			PlayBgm(32);
			Log.Write(LogChannel.General, "ride: the black chocobo flies home");
		}

		private static void HomeComing()
		{
			int[] back = Ff4Vehicles.BlackChocoboBack;
			GlobalScope.stg.CStageMng stage = GlobalScope.stageMng;
			GlobalScope.VecFx32 size = new GlobalScope.VecFx32(stage.getSize());
			long best = long.MaxValue;
			int tx = back[0], tz = back[2];
			foreach (int ox in new[] { 0, size.x })
				foreach (int oz in new[] { 0, size.z })
				{
					long dx = back[0] + ox - _ride.X, dz = back[2] + oz - _ride.Z;
					if (dx * dx + dz * dz < best) { best = dx * dx + dz * dz; tx = back[0] + ox; tz = back[2] + oz; }
				}
			double d = Math.Sqrt(best);
			if (d <= 0x3000)
			{
				_ride.X = tx; _ride.Z = tz;
				Wrap(_ride);
				Place(_ride);
				FollowHero();
				StartHomeLanding();
				return;
			}
			_ride.X += (int)((double)(tx - _ride.X) * 0x3000 / d);
			_ride.Z += (int)((double)(tz - _ride.Z) * 0x3000 / d);
			Turn(Yaw(tx - _ride.X, tz - _ride.Z));
			Wrap(_ride);
			Place(_ride);
			FollowHero();
		}

		/// <summary>Down to its forest ("landing_bc": the landing's 30 frames, the camera back, its sound 2/0), then the party in
		/// its forest town - t17_00 by flag 911, else t22_00 - and the black chocobo gone (flag 908 down).</summary>
		private static void StartHomeLanding()
		{
			GroundKind(_ride, out int groundY, out _, false);
			_homeY = groundY;
			_fromY = _ride.Y;
			_frame = 0;
			_state = State.Landing;
			_toY = _homeY;
			CameraBack();
			PlaySe(2, 0);
			_homeLanding = true;
		}

		private static bool _homeLanding;

		private static void HomeArrived()
		{
			_homeLanding = false;
			GlobalScope.FlagManager.singleton().reset(0, 908);
			Characters.setHidden(_ride.Ctrl, true);
			_ride.Shown = false;
			if (Flag(911)) JumpTo("t17_00", 0, 0, 0x96000);
			else JumpTo("t22_00", -0x74000, 0, 0xc3000);
		}

		// ---- the vehicles as casts (changeHichNumber's 0x4005-0x400a: libff4's slots 200-205, a type each) ----

		/// <summary>The commands the world maps' events give the vehicles.</summary>
		public static readonly HashSet<string> CastCommands = new HashSet<string>(StringComparer.Ordinal)
		{
			"displayCharacter", "moveCharacter_AbsoluteCoordination2", "moveCharacter_RelativeCoordination",
			"setCharacter_ShadowAlpha", "turnCharacter_AbsoluteAngle2",
		};

		public static bool IsVehicleCast(uint cast) => GameProfile.IsFf4 && cast >= 0x4005 && cast <= 0x400a;

		private sealed class CastTween
		{
			public Vehicle V;
			public int Frame, Frames;
			public int FromX, FromY, FromZ, ToX, ToY, ToZ;
			public bool Move;
			public int FromRot, ToRot;
			public bool Turn;
			public int FromShadow, ToShadow;
			public bool Shadow;
		}

		private static readonly List<CastTween> _tweens = new List<CastTween>();

		private static Vehicle CastVehicle(uint cast)
		{
			int type = (int)cast - 0x4005;
			return type >= 0 && type < 6 ? _vehicles[type] : null;
		}

		/// <summary>One of <see cref="CastCommands"/> for a vehicle: its operands read as the stock handler reads them, the
		/// vehicle shown or hidden, moved, turned or its shadow faded - over the frames given, as the field's characters.</summary>
		public static void CastCommand(string name, GlobalScope.ScriptEngine engine)
		{
			uint cast = engine.getWord();
			Vehicle v = CastVehicle(cast);
			switch (name)
			{
				case "displayCharacter":
				{
					bool shown = engine.getDword() != 0;
					engine.getWord();
					if (v == null) return;
					v.Shown = shown;
					Characters.setHidden(v.Ctrl, !shown);
					return;
				}
				case "moveCharacter_AbsoluteCoordination2":
				{
					int x = (int)engine.getDword(), y = (int)engine.getDword(), z = (int)engine.getDword();
					int frames = engine.getWord();
					if (v != null) CastMoveTo(v, x, y, z, frames);
					return;
				}
				case "moveCharacter_RelativeCoordination":
				{
					uint of = engine.getWord();
					int dx = (int)engine.getDword(), dy = (int)engine.getDword(), dz = (int)engine.getDword();
					int frames = engine.getWord();
					if (v == null) return;
					GlobalScope.VecFx32 at = CastPosition(of);
					if (at != null) CastMoveTo(v, at.x + dx, at.y + dy, at.z + dz, frames);
					return;
				}
				case "setCharacter_ShadowAlpha":
				{
					int alpha = engine.getWord(), frames = engine.getWord();
					if (v == null) return;
					CastTween t = Tween(v);
					t.Shadow = true;
					t.FromShadow = Characters.getShadowAlpha(v.Ctrl);
					t.ToShadow = alpha;
					t.Frames = Math.Max(t.Frames, frames);
					if (frames == 0) { Characters.setShadowAlpha(v.Ctrl, (short)alpha); t.Shadow = false; }
					return;
				}
				case "turnCharacter_AbsoluteAngle2":
				{
					int degrees = (int)(engine.getDword() / 4096);
					int frames = engine.getWord();
					engine.getDword();
					engine.getByte();
					if (v == null) return;
					int to = (int)((long)(360 - degrees) * 65536 / 360) & 0xFFFF;
					if (frames == 0) { v.RotY = to; Place(v); return; }
					CastTween t = Tween(v);
					t.Turn = true;
					t.FromRot = v.RotY;
					t.ToRot = to;
					t.Frames = Math.Max(t.Frames, frames);
					return;
				}
			}
		}

		private static GlobalScope.VecFx32 CastPosition(uint cast)
		{
			Vehicle v = CastVehicle(cast);
			if (IsVehicleCast(cast)) return v == null ? null : new GlobalScope.VecFx32(v.X, v.Y, v.Z);
			int num = GlobalScope.CCastCommandTransit.getInstance().changeHichNumber(cast);
			if (num < 0) return null;
			GlobalScope.VecFx32 at = GlobalScope.CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)?.getPosition();
			return at == null ? null : new GlobalScope.VecFx32(at);
		}

		private static void CastMoveTo(Vehicle v, int x, int y, int z, int frames)
		{
			if (frames == 0)
			{
				v.X = x; v.Y = y; v.Z = z;
				v.GroundY = y;
				Place(v);
				return;
			}
			CastTween t = Tween(v);
			t.Move = true;
			t.FromX = v.X; t.FromY = v.Y; t.FromZ = v.Z;
			t.ToX = x; t.ToY = y; t.ToZ = z;
			t.Frame = 0;
			t.Frames = frames;
		}

		private static CastTween Tween(Vehicle v)
		{
			CastTween t = _tweens.Find(w => w.V == v);
			if (t == null) { t = new CastTween { V = v }; _tweens.Add(t); }
			return t;
		}

		/// <summary>A game step of the vehicles' scripted moves, turns and shadow fades.</summary>
		private static void StepCastTweens()
		{
			for (int i = _tweens.Count - 1; i >= 0; i--)
			{
				CastTween t = _tweens[i];
				t.Frame++;
				float k = t.Frames <= 0 ? 1f : Math.Min(1f, t.Frame / (float)t.Frames);
				Vehicle v = t.V;
				if (t.Move)
				{
					v.X = t.FromX + (int)((t.ToX - t.FromX) * k);
					v.Y = t.FromY + (int)((t.ToY - t.FromY) * k);
					v.Z = t.FromZ + (int)((t.ToZ - t.FromZ) * k);
				}
				if (t.Turn) v.RotY = (t.FromRot + (int)((short)(t.ToRot - t.FromRot) * k)) & 0xFFFF;
				Place(v);
				if (t.Shadow) Characters.setShadowAlpha(v.Ctrl, (short)(t.FromShadow + (int)((t.ToShadow - t.FromShadow) * k)));
				if (k >= 1f) _tweens.RemoveAt(i);
			}
		}

		// ---- the engine's sound (WSVehicleMove::wsInitialize: archive 0, 21 the hovercraft, 22 the airships, 23 the magiship) ----

		private static GlobalScope.MatrixSound.MtxSEHandle _engine;

		private static void StartEngine()
		{
			if (_ride == null || _ride.Type > 3 || _engine != null) return;
			int[] numbers = { 21, 22, 22, 23 };
			try { _engine = GlobalScope.MatrixSound.MtxSENDS_Play(0, numbers[_ride.Type], 127, 128); } catch (Exception) { }
		}

		private static void StopEngine()
		{
			if (_engine == null) return;
			try { GlobalScope.MatrixSound.MtxSENDS_Stop(_engine, 0); } catch (Exception) { }
			_engine = null;
		}

		/// <summary>The key bar aboard (FF4.exe's 0x56f330): with the hook, Enter Raise / Lower when it can; else Backspace
		/// Dismount where the vehicle may come down (never carrying the hovercraft).</summary>
		public static void KeyHints(out bool dismount, out bool raise, out bool lower)
		{
			dismount = raise = lower = false;
			if (_state != State.Riding || _ride == null) return;
			if (_ride.Type == 1 && Flag(903))
			{
				raise = CanRaise();
				lower = !raise && CanLower(out _);
				if (raise || lower) return;
			}
			dismount = !(_ride.Type == 1 && Ff4Vehicles.LiftHover) && GroundAllows(_ride);
		}

		private static void Steer(InputState input)
		{
			float up = (input.IsHeld(Pad.Up) ? 1 : 0) - (input.IsHeld(Pad.Down) ? 1 : 0);
			float right = (input.IsHeld(Pad.Right) ? 1 : 0) - (input.IsHeld(Pad.Left) ? 1 : 0);
			bool moving = up != 0 || right != 0;
			if (moving)
			{
				// The pad's directions on the ground as the camera sees it.
				(float fx, float fz) = CameraForward();
				float dx = up * fx - right * fz, dz = up * fz + right * fx;
				float len = (float)Math.Sqrt(dx * dx + dz * dz);
				dx /= len; dz /= len;
				// A unit at a time: a whole step (up to 4) jumps the walls' 3-unit spheres - the chocobo crossed the sea.
				int speed = Speeds[_ride.Type];
				int parts = Math.Max(1, (speed + 0xfff) / 0x1000);
				for (int i = 0; i < parts; i++)
				{
					int px = _ride.X, py = _ride.Y, pz = _ride.Z;
					_ride.X += (int)(dx * speed / parts);
					_ride.Z += (int)(dz * speed / parts);
					Collide(_ride, px, py, pz);
				}
				Wrap(_ride);
				Turn(Yaw(dx, dz));
			}
			else _turnStep = 0x800;
			if (_ride.Type == 4 && moving != _wasMoving)
			{
				Characters.startMotion(_ride.Ctrl, Motion(moving ? 0x3ea : 0x3e9), true, 2);
			}
			_wasMoving = moving;
			Place(_ride);
			FollowHero();
		}

		/// <summary>wsProcessVehicle's turn: towards the way it goes by a step that grows 0x800 a frame, onto it at last.</summary>
		private static void Turn(int target)
		{
			int cur = _ride.RotY & 0xFFFF;
			short diff = (short)(target - cur);
			if (diff == 0) { _turnStep = 0x800; return; }
			if (Math.Abs((int)diff) <= _turnStep) { cur = target; _turnStep = 0x800; }
			else { cur += diff > 0 ? _turnStep : -_turnStep; _turnStep += 0x800; }
			_ride.RotY = cur & 0xFFFF;
		}

		private static int Yaw(float dx, float dz) => (int)Math.Round(Math.Atan2(dx, dz) * 32768.0 / Math.PI) & 0xFFFF;

		/// <summary>The camera's way into the screen on the ground (its target less its place, flattened).</summary>
		private static (float, float) CameraForward()
		{
			GlobalScope.cmr.CWorldCamera camera = GlobalScope.CCastCommandTransit.getInstance().cast_FieldCamera();
			GlobalScope.VecFx32 p = camera?.PosOffset(), t = camera?.TrgOffset();
			if (p == null || t == null) return (0, -1);
			float fx = t.x - p.x, fz = t.z - p.z;
			float len = (float)Math.Sqrt(fx * fx + fz * fz);
			return len < 1 ? (0f, -1f) : (fx / len, fz / len);
		}

		// ---- the ground and the walls (the vehicles' dgsredAccept) ----

		private static IEnumerable<GlobalScope.dgs.CRestrictor> Restrictors()
		{
			for (GlobalScope.dgs.CRestrictor r = (GlobalScope.dgs.CRestrictor)GlobalScope.dgs.DGSLinkedList<GlobalScope.dgs.CRestrictor>.dgsllBase(); r != null; r = (GlobalScope.dgs.CRestrictor)r.dgsllNext())
			{
				if (r.rorActivity()) yield return r;
			}
		}

		private static void Collide(Vehicle v, int px, int py, int pz)
		{
			GlobalScope.stg.CStageMng stage = GlobalScope.stageMng;
			GlobalScope.MtxFx43 inv = new GlobalScope.MtxFx43(), wld = new GlobalScope.MtxFx43();
			stage.getInvWldMtx(inv);
			stage.getWldMtx(wld);
			GlobalScope.VecFx32 now = new GlobalScope.VecFx32(v.X, v.Y, v.Z), pre = new GlobalScope.VecFx32(px, py, pz);
			GlobalScope.MTX_MultVec43(now, inv, now);
			GlobalScope.MTX_MultVec43(pre, inv, pre);
			// The walls of the type's attributes: pushed back out of each by the radius (calculateWallCollision).
			int mx = now.x - pre.x, mz = now.z - pre.z;
			double ml = Math.Sqrt((double)mx * mx + (double)mz * mz);
			if (ml > 0)
			{
				GlobalScope.VecFx32 dir = new GlobalScope.VecFx32((int)(mx / ml * 4096), 0, (int)(mz / ml * 4096));
				GlobalScope.mcl.CollisionResult result = new GlobalScope.mcl.CollisionResult();
				foreach (int attr in Walls(v.Type))
				{
					foreach (GlobalScope.dgs.CRestrictor r in Restrictors())
					{
						result.clean();
						if (!r.rorEvaluateSphere(now, dir, WallRadius, attr, result)) continue;
						GlobalScope.VecFx32 n = result.normal;
						if (n.x == 0 && n.y == 0 && n.z == 0) continue;
						long push = WallRadius - result.length;
						now.x += (int)(push * n.x >> 12);
						now.z += (int)(push * n.z >> 12);
					}
				}
			}
			bool ground = Ground(now, v.Type == 4 ? 0x9000 : v.Type == 5 ? -0x7000 : 0x7000, v.Type == 4 ? 0x31000 : 0x40000, out int groundY, out _, out _);
			if (Flying(v)) now.y = Heights[v.Type];
			else if (ground) now.y = groundY;
			GlobalScope.MTX_MultVec43(now, wld, now);
			v.X = now.x; v.Y = now.y; v.Z = now.z;
			if (ground) v.GroundY = groundY;
		}

		/// <summary>In the air: the airships and the black chocobo, and the hovercraft at its 2 (VO +0x374, set by the take-off).</summary>
		private static bool Flying(Vehicle v) => v.Type != 4;

		/// <summary>The ground under a point in the stage's own space: a ray down from <paramref name="above"/> over it.</summary>
		private static bool Ground(GlobalScope.VecFx32 local, int above, int length, out int y, out int landForm, out uint flags)
		{
			y = local.y;
			landForm = -1;
			flags = 0;
			GlobalScope.VecFx32 start = new GlobalScope.VecFx32(local.x, local.y + above, local.z);
			GlobalScope.VecFx32 down = new GlobalScope.VecFx32(0, -4096, 0);
			GlobalScope.mcl.CollisionResult result = new GlobalScope.mcl.CollisionResult(), best = null;
			int bestLength = int.MaxValue;
			foreach (GlobalScope.dgs.CRestrictor r in Restrictors())
			{
				result.clean();
				if (!r.rorEvaluateArrow(start, down, length, 1, result) || result.length >= bestLength) continue;
				bestLength = result.length;
				best = result;
				result = new GlobalScope.mcl.CollisionResult();
			}
			if (best == null) return false;
			y = best.pos.y;
			landForm = GlobalScope.chr.CCharacterEureka.checkFf4LandForm(best);
			for (uint b = 0; b < 32; b++) if (best.material.getAttribute().isEnableFlag(b)) flags |= 1u << (int)b;
			return true;
		}

		private static void Wrap(Vehicle v)
		{
			GlobalScope.stg.CStageMng stage = GlobalScope.stageMng;
			if (!stage.getLoopEnable()) return;
			GlobalScope.VecFx32 min = new GlobalScope.VecFx32(stage.getEdgeMin()), max = new GlobalScope.VecFx32(stage.getEdgeMax()), size = new GlobalScope.VecFx32(stage.getSize());
			if (v.X < min.x) v.X += size.x; else if (v.X > max.x) v.X -= size.x;
			if (v.Z < min.z) v.Z += size.z; else if (v.Z > max.z) v.Z -= size.z;
		}

		/// <summary>The lead stays the camera's anchor, unseen at the vehicle's place.</summary>
		private static void FollowHero()
		{
			GlobalScope.VecFx32 at = new GlobalScope.VecFx32(_ride.X, _ride.Y, _ride.Z);
			Hero.setPosition(at);
			Hero.setRotation(new GlobalScope.VecFx32(0, _ride.RotY, 0));
		}

		// ---- getting off ----

		/// <summary>checkGetOffEx / checkGetOff: the land form under it allows the type, the ground's flag 20 is down.</summary>
		/// <summary>The kind of ground under the vehicle (landFormParameter(0) of its land form), -1 for none or the ground's
		/// flag 20.</summary>
		private static int GroundKind(Vehicle v, out int groundY, out uint flags, bool log)
		{
			GlobalScope.stg.CStageMng stage = GlobalScope.stageMng;
			GlobalScope.MtxFx43 inv = new GlobalScope.MtxFx43();
			stage.getInvWldMtx(inv);
			GlobalScope.VecFx32 local = new GlobalScope.VecFx32(v.X, v.Y, v.Z);
			GlobalScope.MTX_MultVec43(local, inv, local);
			int above = v.Type == 5 ? -0x7000 : v.Type == 4 ? 0x9000 : 0x7000;
			groundY = v.Y;
			flags = 0;
			if (!Ground(local, above, 0x40000, out int y, out int land, out flags)) return -1;
			groundY = y;
			if ((flags & (1u << 20)) != 0) return -1;
			Ff4Encounters.Table table = Ff4Encounters.For(GlobalScope.stg.CStageMng.CurrentName);
			int kind = land >= 0 && land < 12 && table != null ? table.RawRates[land] : -1;
			if (log) Log.Write(LogChannel.File, "ride: ground at " + v.X + "," + v.Z + ": land form " + land + ", kind " + kind + ", flags 0x" + flags.ToString("x"));
			return kind;
		}

		private static bool CanGetOff(Vehicle v, out int groundY)
		{
			if (v.Type == 1 && Ff4Vehicles.LiftHover) { groundY = v.Y; return false; }
			int kind = GroundKind(v, out groundY, out _, true);
			return KindAllows(v.Type, kind);
		}

		private static bool GroundAllows(Vehicle v) => KindAllows(v.Type, GroundKind(v, out _, out _, false));

		private static bool KindAllows(int type, int kind)
		{
			if (kind < 0) return false;
			switch (type)
			{
				case 0: return kind == 0 || kind == 2;
				case 3: return kind == 0 || kind == 8;
				case 4: return kind >= 0 && kind < 3;
				case 5: return kind == 1;
				default: return kind == 0 || kind == 7;
			}
		}

		private static int _waitFrames;
		/// <summary>The ride began by arriving on it (WSCVehicle's 0x17a down: the black chocobo's take-off sound left out).</summary>
		private static bool _arrived;

		private static void TryGetOff()
		{
			if (_waitFrames > 0) return;
			// WSVehicleMove::wsProcess: the Enterprise or the Falcon over the hole (its ground's bit 17) with flag 909 goes
			// through it - "divetohole" from the Overworld, "risetohole" from the Underworld.
			if ((_ride.Type == 1 || _ride.Type == 2) && Flag(909))
			{
				GroundKind(_ride, out _, out uint flags, false);
				if ((flags & (1u << 17)) != 0) { StartHole(); return; }
			}
			if (!CanGetOff(_ride, out int groundY))
			{
				// FUN_004cd0cc and a 30-frame wait (WSWait) before the pad steers again.
				OpenFF.Game.Audio.PlaySe(0, 0);
				_waitFrames = 30;
				return;
			}
			StopEngine();
			if (_ride.Type == 4) { StartChocoboOff(); return; }
			_state = State.Landing;
			_frame = 0;
			_fromY = _ride.Y;
			_toY = groundY;
			if (_ride.Type != 0) CameraBack();
			// WSVehicleLanding::wsInitialize's sound: the hovercraft's 5/2, the airships' 6/1, the magiship's 7/1, the black
			// chocobo's 2/0.
			switch (_ride.Type)
			{
				case 0: PlaySe(5, 2); break;
				case 1: case 2: PlaySe(6, 1); break;
				case 3: PlaySe(7, 1); break;
				case 5: PlaySe(2, 0); break;
			}
			// WSVehicleLanding: the airships' and the black chocobo's BGM stops; the world's comes back.
			if (_ride.Type == 1 || _ride.Type == 2 || _ride.Type == 5) StopBgm(15);
		}

		private static void Landing()
		{
			_frame++;
			if (_frame <= 30)
			{
				float t = _frame / 30f;
				_ride.Y = _fromY + (int)((_toY - _fromY) * t);
				Place(_ride);
				FollowHero();
			}
			StepCamera(_frame, 30);
			if (_frame < 40) return;
			if (_homeLanding) { HomeArrived(); return; }
			if (_ride.Type == 3)
			{
				// WSVehicleMove's magiship: down ("landing_ms", its place and turn kept), then into it at its door (t21_00 at
				// 48, 24, -122).
				Ff4Vehicles.SetPlace(3, _ride.X, _ride.Y, _ride.Z, _ride.RotY);
				JumpTo("t21_00", 0x30000, 0x18000, -0x7a000);
				return;
			}
			if (_ride.Type == 1 || _ride.Type == 2 || _ride.Type == 5) PlayBgm(_world == 1 ? UnderworldBgm : EarthBgm);
			StartGetOff();
		}

		private static void StartGetOff()
		{
			_state = State.GettingOff;
			_frame = 0;
			// WSGetOffVehicle: the lead where the vehicle stands, turned as it is, standing (1000), fading in; the vehicle's
			// place kept, its motion held (the black chocobo's idle on).
			Hero.setPosition(new GlobalScope.VecFx32(_ride.X, _ride.Y, _ride.Z));
			Hero.setRotation(new GlobalScope.VecFx32(0, _ride.RotY, 0));
			Hero.setHidden(false);
			HeroAlpha(0);
			int id = Hero.getCharacterId();
			if (id >= 0) Characters.startMotion(id, Characters.isMotion(id, 1000) ? 1000 : 1001, true, 0);
			Ff4Vehicles.SetPlace(_ride.Type, _ride.X, _ride.Y, _ride.Z, _ride.RotY);
			if (_ride.Type == 5) Characters.startMotion(_ride.Ctrl, Motion(0x3ea), true, 2);
			else Characters.setMotionPause(_ride.Ctrl, true);
		}

		private static void GettingOff()
		{
			_frame++;
			HeroAlpha(Math.Min(31, _frame * 31 / 20));
			if (_frame < 20) return;
			Foot();
		}

		private static void Foot()
		{
			Hero.setMCLCol(true);
			HeroAlpha(31);
			_state = State.Foot;
			_lastDist = 0;
			Log.Write(LogChannel.General, "ride: got off type " + (_ride?.Type ?? -1));
			_ride = null;
			OpenFF.Game.Input.Capture = false;
		}

		// ---- the chocobo running off (WSChocoboEscape) ----

		private static int _runX, _runZ;

		private static void StartChocoboOff()
		{
			_state = State.ChocoboOff;
			_frame = 0;
			StopBgm(15);
			_fromX = _ride.X; _fromZ = _ride.Z;
			double a = _ride.RotY * Math.PI / 32768.0;
			_runX = (int)(Math.Sin(a) * 0x96000);
			_runZ = (int)(Math.Cos(a) * 0x96000);
			Characters.startMotion(_ride.Ctrl, Motion(0x3ea), true, 2);
			Hero.setPosition(new GlobalScope.VecFx32(_ride.X, _ride.Y, _ride.Z));
			Hero.setRotation(new GlobalScope.VecFx32(0, _ride.RotY, 0));
			Hero.setHidden(false);
			HeroAlpha(0);
			int id = Hero.getCharacterId();
			if (id >= 0) Characters.startMotion(id, Characters.isMotion(id, 1000) ? 1000 : 1001, true, 0);
		}

		private static void ChocoboOff()
		{
			_frame++;
			float t = Math.Min(1f, _frame / 30f);
			Characters.setPosition(_ride.Ctrl, new GlobalScope.VecFx32(_fromX + (int)(_runX * t), _ride.Y, _fromZ + (int)(_runZ * t)));
			HeroAlpha(Math.Min(31, _frame * 31 / 20));
			if (_frame < 30) return;
			Characters.setHidden(_ride.Ctrl, true);
			_ride.Shown = false;
			PlayBgm(_world == 1 ? UnderworldBgm : EarthBgm);
			Foot();
		}

		// ---- the camera, the lead's alpha, the music ----

		private static void CameraTo(int offsetY, int offsetZ, int fromOffsetY, int fromOffsetZ)
		{
			GlobalScope.cmr.CWorldCamera camera = GlobalScope.CCastCommandTransit.getInstance().cast_FieldCamera();
			if (camera == null) return;
			_camPos = new GlobalScope.VecFx32(camera.PosOffset());
			_camTrg = new GlobalScope.VecFx32(camera.TrgOffset());
			// libff4's offsets put the camera on +z; the port's FF4 world camera stands on whichever side it already does.
			int side = _camPos.z < 0 ? -1 : 1;
			_camPosFrom = new GlobalScope.VecFx32(_camPos);
			_camTrgFrom = new GlobalScope.VecFx32(_camTrg);
			_camPosTo = new GlobalScope.VecFx32(0, offsetY, side * offsetZ);
			_camTrgTo = new GlobalScope.VecFx32(0, offsetY + fromOffsetY, side * (offsetZ + fromOffsetZ));
		}

		/// <summary>Back to the camera the lead had before boarding.</summary>
		private static void CameraBack()
		{
			// WSVehicleLanding::wsProcessInit: to the walking camera (0x2bc9dc 0, 100, 110 looking 0, -70, -80 on from it).
			GlobalScope.cmr.CWorldCamera camera = GlobalScope.CCastCommandTransit.getInstance().cast_FieldCamera();
			if (camera == null) return;
			_camPosFrom = new GlobalScope.VecFx32(camera.PosOffset());
			_camTrgFrom = new GlobalScope.VecFx32(camera.TrgOffset());
			int side = _camPosFrom.z < 0 ? -1 : 1;
			_camPosTo = new GlobalScope.VecFx32(0, 0x64000, side * 0x6e000);
			_camTrgTo = new GlobalScope.VecFx32(0, 0x64000 - 0x46000, side * (0x6e000 - 0x50000));
		}

		/// <summary>The trip's arrival: down 2 a frame (OSLinearMoveFixedAndRelative's 0x2000) to the flying height.</summary>
		private static void Descending()
		{
			_frame++;
			_ride.Y = _toY < _fromY ? Math.Max(_toY, _fromY - _frame * _settleStep) : Math.Min(_toY, _fromY + _frame * _settleStep);
			Place(_ride);
			FollowHero();
			if (_ride.Y != _toY) return;
			_settleStep = 0x2000;
			_state = State.Riding;
			_turnStep = 0x800;
			StartEngine();
		}

		private static int _settleStep = 0x2000;

		// ---- through the hole between the worlds (WSDiveToHole / WSRiseToHole) ----

		private static bool _holeDown;

		/// <summary>Over the hole in 15 frames (the Overworld's at -191.5, 25, 1116.9; the Underworld's at 563.7, 25, -659.4),
		/// turned to 0xC000 in 15 more, then 15 down (or up) at 0.25 a frame while the screen fades out over 60.</summary>
		private static void StartHole()
		{
			_holeDown = _world == 0;
			_state = State.Hole;
			_liftPhase = 0;
			_frame = 0;
			_fromX = _ride.X; _fromY = _ride.Y; _fromZ = _ride.Z;
			StopEngine();
			GlobalScope.FlagManager.singleton().set(0, 444);
			Log.Write(LogChannel.General, "ride: through the hole " + (_holeDown ? "down" : "up"));
		}

		private static void Hole()
		{
			_frame++;
			int hx = _holeDown ? -784261 : 2308915, hz = _holeDown ? 4574823 : -2700780;
			switch (_liftPhase)
			{
				case 0:
				{
					float t = Math.Min(1f, _frame / 15f);
					_ride.X = _fromX + (int)((hx - _fromX) * t);
					_ride.Z = _fromZ + (int)((hz - _fromZ) * t);
					_ride.Y = _fromY + (int)((0x19000 - _fromY) * t);
					if (_fromX != hx || _fromZ != hz) Turn(Yaw(hx - _fromX, hz - _fromZ));
					if (_frame >= 15) { _liftPhase = 1; _frame = 0; _heroFromRot = _ride.RotY; }
					break;
				}
				case 1:
				{
					int d = (short)(0xC000 - _heroFromRot);
					_ride.RotY = (_heroFromRot + d * Math.Min(_frame, 15) / 15) & 0xFFFF;
					if (_frame >= 15)
					{
						_liftPhase = 2; _frame = 0;
						GlobalScope.dgs.CFade.Main().fadeOut(60, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
						GlobalScope.dgs.CFade.Sub().fadeOut(60, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
					}
					break;
				}
				case 2:
					_ride.Y = 0x19000 + (_holeDown ? -1 : 1) * Math.Min(_frame, 60) * 0x400;
					if (_frame >= 61)
					{
						// wsProcessDive / wsProcessRise: the other world, aboard, the vehicle kept there.
						int type = _ride.Type, world = _holeDown ? 1 : 0;
						int ax = _holeDown ? 2308915 : -784261, az = _holeDown ? -2700780 : 4574823, ay = _holeDown ? 0x28000 : 0xa000;
						Ff4Vehicles.SetPlace(type, ax, ay, az, 0xC000);
						Ff4Vehicles.SetStay(type, world);
						Ff4Vehicles.ArriveOn = type;
						Ff4Vehicles.WorldJump = _holeDown ? 0 : 2;
						GlobalScope.VecFx32 at = new GlobalScope.VecFx32(ax, 0, az);
						string map = JumpPart.WithChip(_holeDown ? "f01" : "f00", at);
						_state = State.Leaving;
						GlobalScope.CCastCommandTransit transit = GlobalScope.CCastCommandTransit.getInstance();
						transit.castParam_MapJump().initialize();
						transit.castParam_MapJump().setUp(map, 0, at, new GlobalScope.VecFx32(0, 0xC000, 0), _Flag: true);
						transit.cast_BaseSystem().setMapJump(b: true);
						return;
					}
					break;
			}
			Place(_ride);
			FollowHero();
		}

		/// <summary>A map jump of the ride's own (the magiship's inside): the keys and the state wait for the map to change.</summary>
		private static void JumpTo(string map, int x, int y, int z)
		{
			_state = State.Leaving;
			GlobalScope.CCastCommandTransit transit = GlobalScope.CCastCommandTransit.getInstance();
			transit.castParam_MapJump().initialize();
			transit.castParam_MapJump().setUp(map, 0, new GlobalScope.VecFx32(x, y, z), new GlobalScope.VecFx32(0, 0, 0), _Flag: true);
			transit.cast_BaseSystem().setMapJump(b: true);
			Log.Write(LogChannel.General, "ride: into " + map + " at " + x / 4096 + "," + y / 4096 + "," + z / 4096);
		}

		private static void PlaySe(int archive, int number)
		{
			try { OpenFF.Game.Audio.PlaySe(archive, number); } catch (Exception) { }
		}

		private static void StepCamera(int frame, int frames)
		{
			if (_camPosTo == null || frame > frames) return;
			GlobalScope.cmr.CWorldCamera camera = GlobalScope.CCastCommandTransit.getInstance().cast_FieldCamera();
			if (camera == null) return;
			float t = Math.Min(1f, frame / (float)frames);
			GlobalScope.VecFx32 L(GlobalScope.VecFx32 a, GlobalScope.VecFx32 b) => new GlobalScope.VecFx32(a.x + (int)((b.x - a.x) * t), a.y + (int)((b.y - a.y) * t), a.z + (int)((b.z - a.z) * t));
			camera.setPosOffset(L(_camPosFrom, _camPosTo));
			camera.setTrgOffset(L(_camTrgFrom, _camTrgTo));
		}

		private static void HeroAlpha(int alpha)
		{
			int id = Hero.getCharacterId();
			if (id < 0) return;
			Characters.setTransparency(id, Math.Clamp(alpha, 0, 31));
			Characters.setShadowAlphaRate(id, Math.Clamp(alpha, 0, 31));
		}

		private static void PlayBgm(int bgm)
		{
			try { GlobalScope.MatrixSound.MtxSoundBGM.getSingleton().play(bgm, 127, 0, GlobalScope.MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0); } catch (Exception) { }
		}

		private static void StopBgm(int fade)
		{
			try { GlobalScope.MatrixSound.MtxSoundBGM.getSingleton().stop(fade, GlobalScope.MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0); } catch (Exception) { }
		}

		/// <summary>The presses since the last step: the pad's edges are the frame's, the ride's steps the game's (fewer).</summary>
		private static bool _latchA, _latchB, _pressA, _pressB;

		/// <summary>The "!" is over the lead this frame (Enter has something to do): the key bar shows Enter and the balloon.</summary>
		public static bool BalloonShown { get; private set; }

		public override void OnUpdate()
		{
			if (OpenFF.Game.Input.Pressed(Pad.A)) _latchA = true;
			if (OpenFF.Game.Input.Pressed(Pad.B)) _latchB = true;
			if (_waitFrames > 0) _waitFrames--;
			BalloonShown = _state == State.Foot && (_inReach != null || CanAct()) && FreeOnFoot();
			if (BalloonShown) DrawBalloon();
		}

		/// <summary>What Enter would act on (CPlayerHumanAction::checkAction, its key doors left out): a character the lead
		/// touches and faces, to talk to or to check. Steam's "!" shows for these too (Baron Castle's guards and chests).</summary>
		private static bool CanAct()
		{
			try
			{
				if (!(EngineApi.HeroPlayer is GlobalScope.pl.CPlayerCharacter p) || !p.isOperater() || !p.InputPermission()) return false;
				int act = p.getNowAct();
				if (act < 0 || act > 3 || (p.getColType() & 2) == 0) return false;
				GlobalScope.chr.CCharacterEureka t = p.getTarget();
				if (t == null || t.getCharacterId() == -1 || t.getNowAct() == 4 || t.getNowAct() == 5) return false;
				if (t.CharaKind() == GlobalScope.chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_VEHICLE) return false;
				if (t.CharaCheckType() == GlobalScope.chr.CHARACTER_CHECK_TYPE.CHARACTER_CHECK_TYPE_TALK)
					return !(t is GlobalScope.pl.CPlayerCharacter c && c.NPCAiManager().AiKind() == GlobalScope.pl.CNPCAiManager.AI_KIND.AI_KIND_AUTO_FOLLOW);
				return t.CharaCheckType() == GlobalScope.chr.CHARACTER_CHECK_TYPE.CHARACTER_CHECK_TYPE_CHECK;
			}
			catch (Exception) { return false; }
		}

		/// <summary>Steam's "!" (fukidashi.NCBR, 64 x 64 at the menus' scale) over the lead: centred on it, its foot 194 of
		/// Steam's 1080 pixels above the lead's feet (Steam's frame by the Enterprise).</summary>
		private static void DrawBalloon()
		{
			Texture sheet = Ff4Ui.Sheet("fukidashi.NCBR");
			Vector2? feet = OpenFF.Game.Camera.WorldToScreen(OpenFF.Game.Hero.Position);
			if (sheet == null || !OpenFF.Game.Hero.Present) return;
			if (!feet.HasValue) return;
			float size = 64f * Ff4Ui.Scale, bottom = feet.Value.Y - 194f * 480f / 1080f;
			OpenFF.Game.Draw.Sprite(sheet, feet.Value.X - size / 2, bottom - size, size, size);
		}
	}
}
