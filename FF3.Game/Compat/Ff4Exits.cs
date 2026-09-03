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

namespace FF3
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
		}

		private static readonly List<Exit> _exits = new List<Exit>();
		private static string _stage;
		private static Dictionary<string, (int X, int Y, int Z, int Facing)> _arrivals;

		public const int SetInsideMapJump = 333;

		/// <summary>The current map's exits; empty for FF3 or a map without a script.</summary>
		public static IReadOnlyList<Exit> Current => _exits;

		/// <summary>Reads the exits of a stage's script. Called when the world sets a stage up.</summary>
		public static void Load(string stage)
		{
			_exits.Clear();
			_stage = stage;
			if (!GameProfile.IsFf4 || string.IsNullOrEmpty(stage))
			{
				return;
			}
			byte[] data = GameArchive.Read("files/" + stage + ".script");
			foreach (Exit exit in Decode(data))
			{
				_exits.Add(exit);
			}
			Log.Write(LogChannel.General, "exits: " + stage + " has " + _exits.Count + " scripted exit(s)");
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
				bool inside = leader.x >= exit.MinX && leader.x <= exit.MaxX
					&& leader.z >= exit.MinZ && leader.z <= exit.MaxZ
					&& leader.y >= exit.MinY - 8 * 4096 && leader.y <= exit.MaxY + 8 * 4096;
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
				jump.setUp(exit.Destination, 0, pos, rot, _Flag: true);
				world.setMapJump(b: true);
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
				names.Sort((a, b) =>
				{
					bool fa = a.StartsWith("files/f", StringComparison.OrdinalIgnoreCase);
					bool fb = b.StartsWith("files/f", StringComparison.OrdinalIgnoreCase);
					return fa == fb ? string.CompareOrdinal(a, b) : (fa ? -1 : 1);
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

		/// <summary>Every setInsideMapJump in a script's bytecode, decoded linearly with the FF4 table.</summary>
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
					exits.Add(new Exit
					{
						Trigger = operands[0] as string ?? "",
						Destination = destination,
						ArrivalX = V(2), ArrivalY = V(3), ArrivalZ = V(4),
						Facing = V(5) & 7,
						MinX = Math.Min(V(6), V(9)), MaxX = Math.Max(V(6), V(9)),
						MinY = Math.Min(V(7), V(10)), MaxY = Math.Max(V(7), V(10)),
						MinZ = Math.Min(V(8), V(11)), MaxZ = Math.Max(V(8), V(11)),
						Armed = false
					});
				}
			}
			return exits;
		}
	}
}
