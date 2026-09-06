// Which game the content is, and the few places the game logic has to know.
//
// The client's logic is FF3's: its party has jobs and growth tables, its text is
// SJIS/1252 decoded at load, its maps carry a jumps chain, its scripts dispatch
// through a 298-entry table. FF4 shares the engine and the formats but not those
// facts, so where the logic would read FF3's meaning into FF4's data it asks here.
// Everything is derived from the content in front (GameArchive.Game), overridable
// from the command line for testing.

using System;

namespace OpenFF.Client
{
	internal static class GameProfile
	{
		/// <summary>"ff3" or "ff4" - the shape of the shipped content decides.</summary>
		public static string Game => GameArchive.Game;

		public static bool IsFf4 => string.Equals(Game, "ff4", StringComparison.OrdinalIgnoreCase);

		/// <summary>The window's title: the client's name and the game whose content it runs ("OpenFF" alone once a mixed project is the content).</summary>
		public static string Title => IsFf4 ? "OpenFF - Final Fantasy IV" : string.Equals(Game, "ff3", StringComparison.OrdinalIgnoreCase) ? "OpenFF - Final Fantasy III" : "OpenFF";

		/// <summary>Whether player.chaindata is FF3's: jobs, growth types, job MP tables.</summary>
		public static bool Ff3Party => !IsFf4;

		/// <summary>FF4's .msd are UTF-16 already; FF3's are SJIS/1252 and get widened at load.</summary>
		public static bool MsdIsUtf16 => IsFf4;

		/// <summary>Whether a map's .pak is FF3's six chains (jumps, landforms, ... cameras).</summary>
		public static bool Ff3MapParameters => !IsFf4;

		/// <summary>The script command table the content's scripts were compiled against.</summary>
		public static ScriptOpTable ScriptOps => ScriptOpTable.For(Game);

		/// <summary>
		/// The map a --map start lands on when none is named: FF4's Baron town, FF3's Ur.
		/// (d01_00 is Baron's waterway entrance; its world exit is on the far coast.)
		/// </summary>
		// FF4's NewGameInitPart sends the world part "t00_00" (Baron castle's throne room) and
		// the origin; its script then plays the opening (conteEventJumpAndReturnMapJamp 1).
		public static string DefaultStage => IsFf4 ? "t00_00" : "t01_01";

		/// <summary>
		/// The model the party leader walks around as. FF3 derives it from the front
		/// character's job (j101...); FF4 has no jobs, so the name is fixed - Cecil's - unless
		/// --leader= says otherwise.
		/// </summary>
		public static string LeaderModel => Options.Get("leader") ?? (IsFf4 ? "p00_00" : null);

		/// <summary>
		/// The field motion set for a character model, by the game's naming. FF3 keeps them
		/// as w_act_<model> / w_act_man / w_<object>. FF4 keeps one set per party member
		/// (p00_00 walks with f00.ncap), one per NPC body type (f_man001, f_woman001,
		/// f_child001, f_fat001, f_old001 ...), and objects carry their own (o000.ncap).
		/// Which body type an FF4 NPC model uses is not known yet; they all walk as f_man001.
		/// </summary>
		public static string FieldMotion(string model)
		{
			if (string.IsNullOrEmpty(model) || !IsFf4)
			{
				return null;
			}
			switch (model[0])
			{
				case 'p':
					return model.Length >= 3 ? "f" + model.Substring(1, 2) : null;
				case 'o':
				case 'w':
					return model;
				case 'n':
					return "f_man001";
				default:
					return null;
			}
		}

		/// <summary>
		/// A motion's number in FF3's scheme. FF3's field packs number idle 1001, walk 1004
		/// and run 1005; FF4's number them 1000, 1001, 1002. The field code asks by FF3's
		/// numbers, so an FF4 pack's motions are renumbered as they register. FF4 numbers
		/// that would land on a renumbered slot move up by a hundred, out of the way.
		/// </summary>
		public static uint FieldMotionId(uint id)
		{
			if (!IsFf4)
			{
				return id;
			}
			switch (id)
			{
				case 1000: return 1001; // idle
				case 1001: return 1004; // walk
				case 1002: return 1005; // run
				case 1004:
				case 1005:
					return id + 100;
				default:
					return id;
			}
		}

		/// <summary>--map=<stage>: start straight in a map, through the jump part.</summary>
		public static string StartStage
		{
			get
			{
				string map = Options.Get("map");
				if (!string.IsNullOrEmpty(map))
				{
					return map;
				}
				// FF4 has no title of its own in this client yet, so it starts in a map.
				return IsFf4 ? DefaultStage : null;
			}
		}
	}
}
