// FF4's exits, read from its scripts.
//
// FF3 keeps a map's exits in its .pak (a jumps chain) with the trigger in the collision
// mesh. FF4 declares them in the map's own script instead, one per door:
//
//     setInsideMapJump(trigger, map, ax, ay, az, facing, x1, y1, z1, x2, y2, z2)
//
// the destination, where the party arrives there (20.12 fixed, facing in eighths of a
// turn) and the door's box on this map. Decoding the script's bytecode with the shared
// command table (Shared/Script) gives all of them at once, before the script ever runs.
// The world then checks the party leader against the boxes each frame and, on entry,
// requests the same absolute map jump FF3's scripts request - the rest of the jump
// (fade, unload, load, arrival) is the engine's own.
//
// The same decode, over every script in the install, answers "where does the party
// arrive in map X" for a start straight into a map (--map without --pos).

using System;
using System.Collections.Generic;
using System.Text;

namespace OpenFF.Client
{
	internal static class Ff4Exits
	{
		public sealed class Exit
		{
			public string Trigger;
			public string Destination;
			public int ArrivalX, ArrivalY, ArrivalZ; // 20.12
			public int Facing;                       // eighths of a turn
			public int MinX, MinY, MinZ, MaxX, MaxY, MaxZ; // 20.12, this map
			public bool Armed;                       // false while the leader stands inside
			/// <summary>setRelationOfMapjumpobjAndFlag's (group, index) pairs, at most 4: set when the party goes out by it.</summary>
			public readonly List<(uint Group, uint Index)> Flags = new List<(uint, uint)>();
			/// <summary>setDoor's door: this map's material shown open (alpha 0) as the party steps in, and its sound.</summary>
			public string Door;
			public int DoorSeBank = -1, DoorSeNumber = -1;
			/// <summary>setRelationMapjumpToDoorAttr's door: the destination map's material shown open as it is set up.</summary>
			public string FarDoor;
			/// <summary>setMapjumpBGMOperation: the slot whose music stops over 15 frames as the exit is taken, -1 none.</summary>
			public int BgmStopSlot = -1;
		}

		private static readonly List<Exit> _exits = new List<Exit>();
		private static string _stage;
		private static Dictionary<string, (int X, int Y, int Z, int Facing)> _arrivals;

		public const int SetInsideMapJump = 333;

		/// <summary>The current map's exits; empty for FF3 or a map without a script.</summary>
		public static IReadOnlyList<Exit> Current => _exits;

		/// <summary>
		/// A new stage: no exits until its script declares them. Called when the world sets
		/// a stage up. The exits themselves arrive through Register, from setInsideMapJump
		/// and setOutsideMapJump as the script executes them - so an exit inside a branch
		/// exists only when that branch runs, the same as in FF4, and a script of a modder's
		/// own behaves as written.
		/// </summary>
		public static void Load(string stage)
		{
			_exits.Clear();
			_stage = stage;
			Ff4FootEffects.Clear();
			// The flags of the exit taken, set once the map it left is gone and before the new one's scripts run, as
			// WSMapJump's initialize does (loadTargetJumpObjectFlag): Baron's 405/406 pick its Exit spell's door, t17_00's
			// 851-854 which way the party came in.
			if (_pendingFlags.Count > 0)
			{
				foreach ((uint group, uint index) in _pendingFlags) GlobalScope.FlagManager.singleton().set(group, index);
				Log.Write(LogChannel.General, "exits: flags of the exit taken set: " + string.Join(", ", _pendingFlags.ConvertAll(f => f.Group + "/" + f.Index)));
				_pendingFlags.Clear();
			}
		}

		private static readonly List<(uint Group, uint Index)> _pendingFlags = new List<(uint, uint)>();
		private static string _pendingFarDoor;

		/// <summary>The stage is set: the door the party came through stands open (WSPrepare::wsProcessSetupStage's
		/// popOpenDoorAttr - the material at alpha 0 until the map is set up again).</summary>
		public static void StageSet()
		{
			string door = _pendingFarDoor;
			_pendingFarDoor = null;
			if (string.IsNullOrEmpty(door)) return;
			try { GlobalScope.stageMng?.setMaterialAlpha(door, 0); Log.Write(LogChannel.General, "exits: came through door " + door + ", open (alpha now " + GlobalScope.stageMng?.getMaterialAlpha(door) + ")"); }
			catch (Exception ex) { Log.Write(LogChannel.General, "exits: door " + door + " not opened: " + ex.Message); }
		}

		private static Exit Named(string trigger, string what)
		{
			Exit exit = _exits.Find(e => string.Equals(e.Trigger, trigger, StringComparison.OrdinalIgnoreCase));
			if (exit == null) Log.Write(LogChannel.General, "exits: " + (_stage ?? "?") + " " + what + " for " + trigger + ", no such exit");
			return exit;
		}

		/// <summary>setMapjumpBGMOperation's slot for the exit (-1: the music goes on).</summary>
		public static void SetBgmStop(string trigger, int slot)
		{
			Exit exit = Named(trigger, "music");
			if (exit != null) exit.BgmStopSlot = slot;
		}

		/// <summary>setDoor(door, trigger, bank, number, ?): the exit's door on this map and the sound it opens with.</summary>
		public static void SetDoor(string door, string trigger, int bank, int number)
		{
			Exit exit = Named(trigger, "door " + door);
			if (exit == null) return;
			exit.Door = door;
			exit.DoorSeBank = bank;
			exit.DoorSeNumber = number;
		}

		/// <summary>setRelationMapjumpToDoorAttr(door, trigger, ?): the door the exit comes out of on its destination.</summary>
		public static void SetFarDoor(string door, string trigger)
		{
			Exit exit = Named(trigger, "far door " + door);
			if (exit != null) exit.FarDoor = door;
		}

		/// <summary>MapJumpObject::door: a closed door of the exit taken opens - its material gone, its sound played.</summary>
		private static void OpenDoor(Exit exit)
		{
			if (string.IsNullOrEmpty(exit.Door)) return;
			try
			{
				if (GlobalScope.stageMng == null || GlobalScope.stageMng.getMaterialAlpha(exit.Door) == 0) return;
				GlobalScope.stageMng.setMaterialAlpha(exit.Door, 0);
				if (exit.DoorSeBank != -1 && exit.DoorSeNumber != -1) GlobalScope.MatrixSound.MtxSENDS_Play(exit.DoorSeBank, exit.DoorSeNumber, 100, 128);
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "exits: door " + exit.Door + ": " + ex.Message); }
		}

		/// <summary>setRelationOfMapjumpobjAndFlag(trigger, group, index, ?): MapJumpObject::addFlagSequence - a flag the exit sets
		/// when taken (up to 4; group or index 0xffffffff is none).</summary>
		public static void AddFlag(string trigger, uint group, uint index)
		{
			Exit exit = Named(trigger, "flag " + group + "/" + index);
			if (exit != null && exit.Flags.Count < 4) exit.Flags.Add((group, index));
		}

		/// <summary>
		/// Declares (or redeclares, by trigger name) an exit of the current stage. Whether the
		/// leader already stands inside it is checked first, so a door declared under the
		/// party's feet does not fire until they step out and back in.
		/// </summary>
		public static void Register(Exit exit, GlobalScope.VecFx32 leader)
		{
			if (exit == null || string.IsNullOrEmpty(exit.Destination))
			{
				return;
			}
			for (int i = 0; i < _exits.Count; i++)
			{
				if (string.Equals(_exits[i].Trigger, exit.Trigger, StringComparison.OrdinalIgnoreCase))
				{
					_exits.RemoveAt(i);
					break;
				}
			}
			exit.Armed = leader == null || !Inside(exit, leader);
			_exits.Add(exit);
			Log.Write(LogChannel.General, "exits: " + (_stage ?? "?") + " declares " + exit.Trigger + " -> " + exit.Destination
				+ " at " + exit.ArrivalX + " " + exit.ArrivalY + " " + exit.ArrivalZ + " facing " + exit.Facing + " (" + _exits.Count + " now)");
		}

		private static bool Inside(Exit exit, GlobalScope.VecFx32 leader)
		{
			return leader.x >= exit.MinX && leader.x <= exit.MaxX
				&& leader.z >= exit.MinZ && leader.z <= exit.MaxZ
				&& leader.y >= exit.MinY - 8 * 4096 && leader.y <= exit.MaxY + 8 * 4096;
		}

		/// <summary>
		/// Checks the leader against the exit boxes. An exit fires once when the leader
		/// walks into its box from outside; standing in a box on arrival arms nothing.
		/// </summary>
		public static void Update(GlobalScope.VecFx32 leader, GlobalScope.wld.CBaseSystem world)
		{
			if (_exits.Count == 0 || leader == null || world == null)
			{
				return;
			}
			foreach (Exit exit in _exits)
			{
				bool inside = Inside(exit, leader);
				if (!inside)
				{
					exit.Armed = true;
					continue;
				}
				if (!exit.Armed)
				{
					continue;
				}
				exit.Armed = false;
				Log.Write(LogChannel.General, "exits: " + exit.Trigger + " -> " + exit.Destination
					+ " at (" + exit.ArrivalX / 4096 + "," + exit.ArrivalY / 4096 + "," + exit.ArrivalZ / 4096 + ") facing " + exit.Facing * 45);
				GlobalScope.VecFx32 pos = new GlobalScope.VecFx32(exit.ArrivalX, exit.ArrivalY, exit.ArrivalZ);
				GlobalScope.VecFx32 rot = new GlobalScope.VecFx32(0, exit.Facing * 8192, 0);
				GlobalScope.CCastCommandTransit.CAST_MAPJUMP jump = GlobalScope.CCastCommandTransit.getInstance().castParam_MapJump();
				jump.initialize();
				string destination = exit.Destination;
				// The magiship's doors (t21_00's "back_from_magiship"): out by it, on the world it stays on.
				if (Ff4Vehicles.ResolveMap(ref destination, ref pos) && destination.Length == 3) destination = JumpPart.WithChip(destination, pos);
				jump.setUp(destination, 0, pos, rot, _Flag: true);
				world.setMapJump(b: true);
				Ff4Warp.ExitTaken(destination, pos.x, pos.y, pos.z, exit.Facing);
				_pendingFlags.Clear();
				_pendingFlags.AddRange(exit.Flags);
				_pendingFarDoor = exit.FarDoor;
				OpenDoor(exit);
				Ff4Music.ExitTaken(exit.BgmStopSlot);
				return;
			}
		}

		/// <summary>
		/// Where some other map's exit puts the party on arriving in <paramref name="stage"/>:
		/// the first such exit found across the install's scripts, or null.
		/// </summary>
		public static (int X, int Y, int Z, int Facing)? ArrivalInto(string stage)
		{
			if (!GameProfile.IsFf4 || GameArchive.Chain == null || string.IsNullOrEmpty(stage))
			{
				return null;
			}
			if (_arrivals == null)
			{
				_arrivals = new Dictionary<string, (int, int, int, int)>(StringComparer.OrdinalIgnoreCase);
				// The world map first, so a town's front door wins over a back room's.
				List<string> names = new List<string>();
				foreach (string name in GameArchive.Chain.Names)
				{
					if (name.EndsWith(".script", StringComparison.OrdinalIgnoreCase) && name.StartsWith("files/", StringComparison.OrdinalIgnoreCase))
					{
						names.Add(name);
					}
				}
				// The game's first map first (its exit onto the world map is where the story
				// begins), then the world map (a town's front door wins over a back room's).
				string home = "files/" + GameProfile.DefaultStage + ".script";
				int Rank(string n) => string.Equals(n, home, StringComparison.OrdinalIgnoreCase) ? 0
					: n.StartsWith("files/f", StringComparison.OrdinalIgnoreCase) ? 1 : 2;
				names.Sort((a, b) =>
				{
					int ra = Rank(a), rb = Rank(b);
					return ra == rb ? string.CompareOrdinal(a, b) : ra.CompareTo(rb);
				});
				foreach (string name in names)
				{
					foreach (Exit exit in Decode(GameArchive.Read(name)))
					{
						if (!_arrivals.ContainsKey(exit.Destination))
						{
							_arrivals[exit.Destination] = (exit.ArrivalX, exit.ArrivalY, exit.ArrivalZ, exit.Facing);
						}
					}
				}
				Log.Write(LogChannel.General, "exits: arrivals known for " + _arrivals.Count + " maps, from " + names.Count + " scripts");
			}
			return _arrivals.TryGetValue(stage, out (int X, int Y, int Z, int Facing) arrival) ? arrival : null;
		}

		/// <summary>An exit from the command's operands: trigger, map, arrival, facing, box corners (20.12).</summary>
		public static Exit FromOperands(string trigger, string destination, int ax, int ay, int az, int facing,
			int x1, int y1, int z1, int x2, int y2, int z2)
		{
			return new Exit
			{
				Trigger = trigger ?? "",
				Destination = destination,
				// babilCommand_SetInsideMapJump / SetOutsideMapJump take the arrival's z negated (the door's box as written).
				// The world map is the exception here: the port mirrors its chips instead (FieldMirror), which comes to the same.
				ArrivalX = ax, ArrivalY = ay, ArrivalZ = MirroredField(destination) ? az : -az,
				Facing = facing & 7,
				MinX = Math.Min(x1, x2), MaxX = Math.Max(x1, x2),
				MinY = Math.Min(y1, y2), MaxY = Math.Max(y1, y2),
				MinZ = Math.Min(z1, z2), MaxZ = Math.Max(z1, z2),
				Armed = false
			};
		}

		/// <summary>Whether a destination is the world map as the port lays it out mirrored (fNN, FieldMirror on).</summary>
		internal static bool MirroredField(string map) => FieldMirror.WantsFieldMirror && map != null && map.Length >= 3
			&& (map[0] == 'f' || map[0] == 'F') && char.IsDigit(map[1]) && char.IsDigit(map[2]);

		/// <summary>
		/// Every setInsideMapJump in a script's bytecode, decoded linearly with the FF4
		/// table, whatever branch it sits in. Only for choosing where a --map start lands
		/// (ArrivalInto); the exits a map actually has come from running its script.
		/// </summary>
		public static List<Exit> Decode(byte[] data)
		{
			List<Exit> exits = new List<Exit>();
			if (data == null || data.Length < 16 || data[0] != 'M' || data[1] != 'H' || data[2] != 'C' || data[3] != 'S')
			{
				return exits;
			}
			int casts = data[14] | (data[15] << 8);
			int at = 16 + casts * 16;
			int end = (int)Math.Min((uint)(data[8] | (data[9] << 8) | (data[10] << 16) | (data[11] << 24)), (uint)data.Length);
			ScriptOpTable ops = ScriptOpTable.Ff4;
			object[] operands = new object[16];
			while (at + 2 <= end)
			{
				int opcode = data[at] | (data[at + 1] << 8);
				at += 2;
				ScriptOp op = ops.Get(opcode);
				if (op == null)
				{
					break; // not bytecode any more; stop rather than guess
				}
				int count = 0;
				bool ok = true;
				foreach (Operand operand in op.Operands ?? Array.Empty<Operand>())
				{
					switch (operand)
					{
						case Operand.Byte:
							if (at + 1 > end) { ok = false; break; }
							operands[count++] = (uint)data[at]; at += 1; break;
						case Operand.Word:
							if (at + 2 > end) { ok = false; break; }
							operands[count++] = (uint)(data[at] | (data[at + 1] << 8)); at += 2; break;
						case Operand.Dword:
							if (at + 4 > end) { ok = false; break; }
							operands[count++] = (uint)(data[at] | (data[at + 1] << 8) | (data[at + 2] << 16) | (data[at + 3] << 24)); at += 4; break;
						case Operand.String:
						{
							int zero = Array.IndexOf(data, (byte)0, at, end - at);
							if (zero < 0) { ok = false; break; }
							operands[count++] = Encoding.UTF8.GetString(data, at, zero - at);
							at = zero + 1;
							break;
						}
					}
					if (!ok || count >= operands.Length)
					{
						break;
					}
				}
				if (!ok)
				{
					break;
				}
				if (opcode == SetInsideMapJump && count >= 12 && operands[1] is string destination)
				{
					int V(int i) => unchecked((int)(uint)operands[i]);
					exits.Add(FromOperands(operands[0] as string, destination, V(2), V(3), V(4), V(5),
						V(6), V(7), V(8), V(9), V(10), V(11)));
				}
			}
			return exits;
		}
	}
}
