// The field movement tuning the FF3 logic reads from two small tables, synthesised.
//
// FF3 shipped player_world_move_parameter.pak (how the party accelerates, turns and skids
// on foot and aboard vehicles; which terrain each may enter; footstep effect timing) and
// npc_world_move_parameter.pak (how NPCs wander and follow). FF4 compiled the same numbers
// into its executable, so an FF4 install has no such files - and nothing of FF3's may be
// needed to play FF4. These tables are OpenFF's own tuning, laid out the way the loaders
// read them (see pl.CPlayerWorldMoveParameter and friends), and served from memory as
// the last content source. A mod that ships either file under the same name replaces
// them; an FF3 install never sees them, since its own files come first.
//
// Units: speeds and accelerations are in the engine's fixed-point world units per frame
// as floats; frames are 30 per second.

using System;
using System.Collections.Generic;
using System.IO;
using OpenFF.Content;

namespace OpenFF.Client
{
	internal static class MovementDefaults
	{
		public const string PlayerTable = "files/player_world_move_parameter.pak";
		public const string NpcTable = "files/npc_world_move_parameter.pak";

		/// <summary>One movement profile: acceleration, top speed and skid on foot (N) and sprinting (S), turning likewise, and weight.</summary>
		private struct Move
		{
			public float MoveAccel, SprintAccel, MoveMax, SprintMax, MoveSkid;
			public float TurnAccel, SprintTurnAccel, TurnMax, SprintTurnMax, TurnSkid, Weight;

			public Move(float moveAccel, float sprintAccel, float moveMax, float sprintMax, float moveSkid,
				float turnAccel, float sprintTurnAccel, float turnMax, float sprintTurnMax, float turnSkid, float weight)
			{
				MoveAccel = moveAccel; SprintAccel = sprintAccel; MoveMax = moveMax; SprintMax = sprintMax; MoveSkid = moveSkid;
				TurnAccel = turnAccel; SprintTurnAccel = sprintTurnAccel; TurnMax = turnMax; SprintTurnMax = sprintTurnMax; TurnSkid = turnSkid; Weight = weight;
			}
		}

		// Turning feels the same for everyone on foot; the heavy craft turn slower.
		private static readonly float[] TurnOnFoot = { 3600f, 4000f, 4800f, 6000f, 3200f };
		private static readonly float[] TurnHeavy = { 2200f, 3400f, 3200f, 5400f, 1400f };

		private static Move Foot(float accel, float sprintAccel, float max, float sprintMax, float skid, float weight)
		{
			return new Move(accel, sprintAccel, max, sprintMax, skid, TurnOnFoot[0], TurnOnFoot[1], TurnOnFoot[2], TurnOnFoot[3], TurnOnFoot[4], weight);
		}

		private static Move Heavy(float accel, float sprintAccel, float max, float sprintMax, float skid, float weight)
		{
			return new Move(accel, sprintAccel, max, sprintMax, skid, TurnHeavy[0], TurnHeavy[1], TurnHeavy[2], TurnHeavy[3], TurnHeavy[4], weight);
		}

		/// <summary>The seventeen movement profiles the field code indexes by character and situation.</summary>
		private static readonly Move[] Moves =
		{
			Foot(100f, 240f, 480f, 1560f, 160f, 10f),   // 0 the party on foot, in town
			Foot(40f, 120f, 320f, 780f, 80f, 10f),      // 1 slower: in a dungeon
			Foot(20f, 60f, 160f, 360f, 40f, 10f),       // 2 slower still: shallow water, small rooms
			Foot(32f, 96f, 256f, 624f, 64f, 10f),       // 3 cramped
			Foot(120f, 280f, 520f, 1500f, 160f, 60f),   // 4 the party on the world map
			Heavy(20f, 40f, 120f, 450f, 32f, 8f),       // 5 a boat
			Heavy(10f, 20f, 60f, 225f, 16f, 8f),        // 6 a slow boat
			Foot(240f, 480f, 920f, 2500f, 480f, 60f),   // 7 an airship
			Foot(100f, 240f, 480f, 1560f, 160f, 10f),   // 8 the party on foot (alternate)
			Foot(100f, 240f, 480f, 1560f, 160f, 60f),   // 9 riding, world map
			Foot(240f, 480f, 920f, 2500f, 480f, 60f),   // 10 a fast airship
			Foot(480f, 960f, 1840f, 5000f, 960f, 60f),  // 11 the fastest craft
			Foot(100f, 240f, 480f, 1560f, 160f, 60f),   // 12 a heavy walker
			Foot(240f, 480f, 920f, 2500f, 480f, 60f),   // 13 a fast walker
			Foot(12f, 36f, 96f, 234f, 24f, 10f),        // 14 wading
			Foot(110f, 250f, 490f, 1570f, 150f, 10f),   // 15 a little quicker than 0
			Foot(50f, 130f, 330f, 790f, 90f, 10f),      // 16 a little quicker than 1
		};

		/// <summary>Which of the fifteen landform types each of eight movers may enter (1 = yes).</summary>
		private static readonly byte[][] Enter =
		{
			new byte[] { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1 }, // on foot: land, not water or sky
			new byte[] { 1, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1 }, // a boat: coast and water
			new byte[] { 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, // deep water craft
			new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, // flight
			new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
			new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
			new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
			new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
		};

		/// <summary>Vehicle rise and descent: rise speed, descent acceleration, rise skid, descent skid, target point (all 12.4 fixed as shorts).</summary>
		private static readonly short[][] VehicleMove =
		{
			new short[] { 0, 16256, 0, 16256, 0 }, new short[] { 16256, 0, 16256, 0, 16256 },
			new short[] { 0, 0, 0, 0, 0 }, new short[] { 0, 0, 0, 0, 0 },
			new short[] { 0, 16256, 0, 16256, 0 }, new short[] { 16256, 0, 16256, 0, 16256 },
			new short[] { 0, 16256, 0, 16256, 0 }, new short[] { 16256, 0, 16256, 0, 16256 },
			new short[] { 0, 16256, 0, 16256, 0 }, new short[] { 16256, 0, 16256, 0, 16256 },
			new short[] { 0, 16256, 0, 16256, 0 }, new short[] { 16256, 0, 16256, 0, 16256 },
			new short[] { 0, 16256, 0, 16256, 0 }, new short[] { 16256, 0, 16256, 0, 16256 },
		};

		/// <summary>Where each vehicle may be boarded (ten flag bytes each).</summary>
		private static readonly byte[][] VehicleEnter =
		{
			new byte[] { 1, 1, 1, 0, 0, 1, 0, 0, 0, 0 }, new byte[] { 1, 0, 1, 0, 0, 1, 0, 0, 0, 0 },
			new byte[] { 0, 0, 0, 1, 0, 0, 0, 0, 0, 0 }, new byte[] { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0 },
			new byte[] { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0 }, new byte[] { 1, 0, 1, 0, 0, 1, 0, 0, 0, 0 },
			new byte[] { 1, 0, 1, 0, 0, 1, 0, 0, 0, 0 }, new byte[] { 1, 0, 1, 0, 0, 1, 0, 0, 0, 0 },
		};

		/// <summary>Footstep effect timing per movement profile: wait frames, move frames (two), special-move frames (two).</summary>
		private static readonly short[] SeEffectPlay = { 30, 1, 14, 7, 14 };

		/// <summary>Footstep effects per landform: sound, wait effect (category, number), move effect (category, number); -1 = none.</summary>
		private static readonly short[][] SeEffectMap = BuildSeEffectMap();

		private static short[][] BuildSeEffectMap()
		{
			short[][] rows = new short[16][];
			for (int i = 0; i < rows.Length; i++)
			{
				rows[i] = new short[] { -1, -1, -1, -1, -1 };
			}
			rows[12] = new short[] { 25, 100, 1, 100, 2 }; // splashing through shallow water
			return rows;
		}

		/// <summary>NPC wandering: frames per state, random extra frames, wander distance (two).</summary>
		private static readonly short[] NpcRandomMove = { 50, 100, 10, 10 };
		private const int NpcRandomMoveKinds = 9;

		/// <summary>NPC following: frames per state, random extra, walk distance, run distance.</summary>
		private static readonly short[] NpcAutoFollow = { 15, 15, 12, 18 };
		private const int NpcAutoFollowKinds = 3;

		/// <summary>Adds the synthesised tables to the chain for whatever it does not already have.</summary>
		public static void Register(ContentChain chain)
		{
			if (chain == null)
			{
				return;
			}
			MemoryContentSource source = new MemoryContentSource("synthesised movement tables");
			if (!chain.Exists(PlayerTable))
			{
				source.Add(PlayerTable, BuildPlayerTable());
			}
			if (!chain.Exists(NpcTable))
			{
				source.Add(NpcTable, BuildNpcTable());
			}
			if (source.Count > 0)
			{
				chain.AddFallback(source);
			}
		}

		public static byte[] BuildPlayerTable()
		{
			List<byte[]> chains = new List<byte[]>();
			using (MemoryStream m = new MemoryStream())
			using (BinaryWriter w = new BinaryWriter(m))
			{
				foreach (Move move in Moves)
				{
					w.Write(move.MoveAccel); w.Write(move.SprintAccel); w.Write(move.MoveMax); w.Write(move.SprintMax); w.Write(move.MoveSkid);
					w.Write(move.TurnAccel); w.Write(move.SprintTurnAccel); w.Write(move.TurnMax); w.Write(move.SprintTurnMax); w.Write(move.TurnSkid);
					w.Write(move.Weight);
				}
				chains.Add(m.ToArray());
			}
			chains.Add(Concat(Enter));
			chains.Add(Shorts(VehicleMove));
			chains.Add(Concat(VehicleEnter));
			short[][] play = new short[Moves.Length][];
			for (int i = 0; i < play.Length; i++)
			{
				play[i] = SeEffectPlay;
			}
			chains.Add(Shorts(play));
			chains.Add(Shorts(SeEffectMap));
			return Pak(chains);
		}

		public static byte[] BuildNpcTable()
		{
			short[][] random = new short[NpcRandomMoveKinds][];
			for (int i = 0; i < random.Length; i++)
			{
				random[i] = NpcRandomMove;
			}
			short[][] follow = new short[NpcAutoFollowKinds][];
			for (int i = 0; i < follow.Length; i++)
			{
				follow[i] = NpcAutoFollow;
			}
			return Pak(new List<byte[]> { Shorts(random), Shorts(follow) });
		}

		private static byte[] Concat(byte[][] rows)
		{
			using (MemoryStream m = new MemoryStream())
			{
				foreach (byte[] row in rows)
				{
					m.Write(row, 0, row.Length);
				}
				return m.ToArray();
			}
		}

		private static byte[] Shorts(short[][] rows)
		{
			using (MemoryStream m = new MemoryStream())
			using (BinaryWriter w = new BinaryWriter(m))
			{
				foreach (short[] row in rows)
				{
					foreach (short v in row)
					{
						w.Write(v);
					}
				}
				return m.ToArray();
			}
		}

		/// <summary>
		/// The pak container the loaders read: a 16-byte header (chain count, then zeros), a
		/// table of (offset, size) pairs, then the chains in order.
		/// </summary>
		private static byte[] Pak(List<byte[]> chains)
		{
			using (MemoryStream m = new MemoryStream())
			using (BinaryWriter w = new BinaryWriter(m))
			{
				w.Write(chains.Count); w.Write(0); w.Write(0); w.Write(0);
				int offset = 16 + 8 * chains.Count;
				foreach (byte[] chain in chains)
				{
					w.Write(offset); w.Write(chain.Length);
					offset += chain.Length;
				}
				foreach (byte[] chain in chains)
				{
					w.Write(chain);
				}
				return m.ToArray();
			}
		}
	}
}
