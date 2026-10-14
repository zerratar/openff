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

		private enum State { Foot, Boarding, TakeOff, Riding, Landing, GettingOff, ChocoboOff, Descending, Leaving }

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
			_world = -1;
			if (_state != State.Foot)
			{
				Log.Write(LogChannel.General, "ride: the map left aboard " + (_ride?.Type ?? -1));
				// The keys are the next map's (the magiship's inside, the world arrived aboard takes them again).
				OpenFF.Game.Input.Capture = false;
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
			_state = State.Riding;
			_turnStep = 0x800;
			Log.Write(LogChannel.General, "ride: aboard type " + _ride.Type);
		}

		/// <summary>WSPrepare for a vehicle arrived on (a worldMapWarp's): aboard at once, in the air for an airship.</summary>
		private static void Arrive(Vehicle v)
		{
			_ride = v;
			Hero.setMCLCol(false);
			Hero.setHidden(true);
			OpenFF.Game.Input.Capture = true;
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
			Steer(input);
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
		private static bool CanGetOff(Vehicle v, out int groundY)
		{
			GlobalScope.stg.CStageMng stage = GlobalScope.stageMng;
			GlobalScope.MtxFx43 inv = new GlobalScope.MtxFx43();
			stage.getInvWldMtx(inv);
			GlobalScope.VecFx32 local = new GlobalScope.VecFx32(v.X, v.Y, v.Z);
			GlobalScope.MTX_MultVec43(local, inv, local);
			int above = v.Type == 5 ? -0x7000 : v.Type == 4 ? 0x9000 : 0x7000;
			groundY = v.Y;
			if (!Ground(local, above, 0x40000, out int y, out int land, out uint flags)) return false;
			groundY = y;
			if ((flags & (1u << 20)) != 0) return false;
			Ff4Encounters.Table table = Ff4Encounters.For(GlobalScope.stg.CStageMng.CurrentName);
			int kind = land >= 0 && land < 12 && table != null ? table.RawRates[land] : -1;
			Log.Write(LogChannel.File, "ride: get off at " + v.X + "," + v.Z + "? land form " + land + ", kind " + kind + ", ground flags 0x" + flags.ToString("x"));
			switch (v.Type)
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
			if (!CanGetOff(_ride, out int groundY))
			{
				// FUN_004cd0cc and a 30-frame wait (WSWait) before the pad steers again.
				OpenFF.Game.Audio.PlaySe(0, 0);
				_waitFrames = 30;
				return;
			}
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
			_ride.Y = Math.Max(_toY, _fromY - _frame * 0x2000);
			Place(_ride);
			FollowHero();
			if (_ride.Y > _toY) return;
			_state = State.Riding;
			_turnStep = 0x800;
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

		public override void OnUpdate()
		{
			if (OpenFF.Game.Input.Pressed(Pad.A)) _latchA = true;
			if (OpenFF.Game.Input.Pressed(Pad.B)) _latchB = true;
			if (_waitFrames > 0) _waitFrames--;
			if (_state == State.Foot && (_inReach != null || CanAct()) && FreeOnFoot()) DrawBalloon();
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
