// A scripted driver for headless tests: --drive=<file> plays key presses into the game
// from inside, so a test runs the same whether the window has focus, the desktop is locked,
// or nobody is at the machine. The script is one step per line:
//
//   wait <seconds>                 pause
//   press <key> [holdMs]           hold a key (XNA Keys names: K, Z, Down, Right, C, M...) - 120 ms unless said
//   tap <x> <y> [holdMs]           a touch at a point of the 800x480 view (a click on the window), released after the hold
//   stick <x> <y> [holdMs]         the pad's left stick held at (x, y), each -1..1 with y up, 1000 ms unless said
//   type <text>                    typed into the open text field (the name entry's); "type" alone clears it; submit / cancel are its Enter and Escape
//   flag <group>:<index> [on|off]  a game flag set (or cleared) - a story state without playing there
//   item <itemId> [count]          the item into the bag (Game.Party.AddItem); equip <member> <itemId> puts it on
//   member <id>                    a character into the party by the game's id (Game.Party.AddMember: FF3 1 Arc, 2 Refia, 3 Ingus)
//   battle <formation> [map]       a fight with that formation (Game.Battle.Start)
//   gil <amount>                   the party's gil (Game.Party.Gil)
//   characters                     every character up: model, motion files (with their use counts), motions held and playing
//   save <slot>                    FF4: the game written to a slot (Ff4Saves.Save), as the menu's Save does
//   jump <stage> [x y z]           a map jump there (world units), as a map's exit makes it - the leaving and entering states and their fades
//   shop <index> [table]           the game's shop screen for that shop (Game.Shops.Open: FF3's t01.shp unless a table is named)
//   motion <index> [loop] [all] [end]  the hero plays a motion by id, b_b01 bound first (706 the fall, 4101 the win pose): a pose on a map, in daylight;
//                                  "all" every character that has it (a battle's hero), "end" held at its last frame
//   hp <member> <hp>               a party member's HP (Game.Party.SetHp); 0 fells them - in a battle the fall is played and held
//   camera <x> <y> <z> [yaw] [pitch]  the free camera (F7) placed there, in world units and degrees, for a screenshot from a chosen eye; "camera off" gives the game its eye back
//   until <regex> [timeoutSeconds] wait for a log line matching the pattern (30 s unless said; "drive: timed out" if not);
//                                  a line written since the previous until was satisfied counts too
//   repeat <key> <seconds> until <regex> [timeoutSeconds]  the key pressed every so often while waiting as until does
//                                  (a battle's spoils messages pressed through, and no press past them)
//   say <text>                     a line in the log ("drive: <text>") to mark progress
//   shots <count>                  a screenshot of each of the next displayed frames (--screenshot-dir), for what lasts a frame
//   qol <option> <value>           a quality-of-life option for this run (a drive plays with the defaults otherwise):
//                                  speed 1-8, encounters on|off, exp / jobExp a multiplier, saveAnywhere on|off, jobAdjustment on|off
//   hud <name> [json]              the field HUD's data a mod would set (Game.Hud.Set): hud quest {"title": "...", "active": true}; no json clears it
//   dialogue <text> [| speaker]    the field's message window with the text (Game.Dialogue.Say: "@1000142" a line of the game's)
//   ask <question>                 the question with the Yes / No box (Game.Dialogue.Ask); the answer in the log ("drive: answered yes")
//   quit                           close the game
//   # comment
//
// Keys are injected at the same point the keyboard is read (DesktopInput.Injected), so pad
// bits, the engine's Game.Input and the mods see them as real presses. Timing is by the game's
// steps, thirty a second (GameHost.Step runs the drive once a step, whatever the display's
// rate), from the first step after the world part is up. Tests in
// Docs/Testing.md name their drive files under Docs/Drives.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Input;

namespace OpenFF.Client
{
	// MonoGame's types by name: the engine's OpenFF.Game, GameTime, Color and vectors sit a namespace up.
	using Game = Microsoft.Xna.Framework.Game;

	internal static class Drive
	{
		private sealed class Step
		{
			public string Verb;
			public string Arg;
			public double Number;
		}

		private static readonly List<Step> _steps = new List<Step>();
		private static int _at = -1;
		/// <summary>The game's steps a second: the drive's clock.</summary>
		private const int StepsPerSecond = 30;

		private static int _framesLeft;
		private static Regex _waitFor;
		private static bool _matched;
		private static string _path;
		private static bool _done;

		/// <summary>True when --drive names a script; the input layer then accepts injected keys without focus.</summary>
		public static bool Active => _path != null && !_done;

		public static void Initialise()
		{
			string path = Options.Get("drive");
			if (string.IsNullOrEmpty(path)) return;
			try
			{
				foreach (string raw in File.ReadAllLines(path))
				{
					string line = raw.Trim();
					if (line.Length == 0 || line[0] == '#') continue;
					string[] parts = line.Split(new[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
					Step step = new Step { Verb = parts[0].ToLowerInvariant(), Arg = parts.Length > 1 ? parts[1].Trim() : "" };
					_steps.Add(step);
				}
				_path = path;
				Log.Write(LogChannel.General, "drive: " + _steps.Count + " step(s) from " + path);
				Log.Written += OnLogLine;
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "drive: " + path + " not read: " + ex.Message);
			}
		}

		// Lines written since the last until was satisfied: a step's effect often lands in the
		// log before the drive reaches the until that waits for it.
		private static readonly List<string> _recent = new List<string>();
		// A repeat's key, pressed every _repeatEvery steps while its until waits.
		private static Keys? _repeatKey;
		private static int _repeatEvery, _repeatIn;
		private static readonly object _lock = new object();

		private static void OnLogLine(string line)
		{
			lock (_lock)
			{
				if (line.StartsWith("drive:", StringComparison.Ordinal)) return;
				if (_recent.Count < 2000) _recent.Add(line);
				Regex waitFor = _waitFor;
				if (waitFor != null && !_matched && waitFor.IsMatch(line)) _matched = true;
			}
		}

		/// <summary>Once per frame, before the input is read.</summary>
		public static void Update()
		{
			if (!Active) return;
			if (_framesLeft > 0)
			{
				_framesLeft--;
				if (_framesLeft == 0)
				{
					DesktopInput.Injected.Clear();
					DesktopInput.InjectedStick = null;
					if (_tapHeld)
					{
						_tapHeld = false;
						DesktopInput.InjectTouch(1, _tapX, _tapY);
					}
				}
				return;
			}
			if (_waitFor != null)
			{
				if (_matched) { _waitFor = null; _matched = false; _repeatKey = null; lock (_lock) _recent.Clear(); }
				else if (_framesLeft == 0 && --_timeoutFrames <= 0)
				{
					Log.Write(LogChannel.General, "drive: timed out waiting for /" + _waitFor + "/");
					_waitFor = null;
					_repeatKey = null;
				}
				else
				{
					if (_repeatKey != null && --_repeatIn <= 0)
					{
						DesktopInput.Injected.Add(_repeatKey.Value);
						_framesLeft = 4;
						_repeatIn = _repeatEvery;
					}
					return;
				}
			}
			_at++;
			if (_at >= _steps.Count)
			{
				_done = true;
				Log.Write(LogChannel.General, "drive: done");
				return;
			}
			Step step = _steps[_at];
			switch (step.Verb)
			{
				case "wait":
					_framesLeft = Math.Max(1, (int)Math.Round(Seconds(step.Arg, 1) * StepsPerSecond));
					break;
				case "press":
				{
					string[] bits = step.Arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					if (bits.Length == 0) break;
					if (Enum.TryParse(bits[0], true, out Keys key))
					{
						DesktopInput.Injected.Add(key);
						int hold = bits.Length > 1 && int.TryParse(bits[1], out int ms) ? ms : 120;
						_framesLeft = Math.Max(2, hold * StepsPerSecond / 1000);
						Log.Write(LogChannel.File, "drive: press " + key + " for " + _framesLeft + " frame(s)");
					}
					else
					{
						Log.Write(LogChannel.General, "drive: unknown key '" + bits[0] + "'");
					}
					break;
				}
				case "stick":
				{
					// The pad's left stick held at (x, y) - each -1..1, y up - for the hold, then let go:
					// the field's analog path (DesktopInput.LeftStick) and the eight-way bits read it as a pad's.
					string[] bits = step.Arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					if (bits.Length < 2 || !float.TryParse(bits[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float sx) || !float.TryParse(bits[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float sy))
					{
						Log.Write(LogChannel.General, "drive: stick wants <x> <y> [holdMs], each -1..1");
						break;
					}
					int hold = bits.Length > 2 && int.TryParse(bits[2], out int ms) ? ms : 1000;
					_framesLeft = Math.Max(2, hold * StepsPerSecond / 1000);
					DesktopInput.InjectedStick = new Microsoft.Xna.Framework.Vector2(sx, sy);
					Log.Write(LogChannel.File, "drive: stick " + sx.ToString(CultureInfo.InvariantCulture) + "," + sy.ToString(CultureInfo.InvariantCulture) + " for " + _framesLeft + " frame(s)");
					break;
				}
				case "tap":
				{
					// A touch at a point of the 800x480 view, held a moment then released - what a
					// click on the window does (DesktopInput.UpdateMouse).
					string[] bits = step.Arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					if (bits.Length < 2 || !int.TryParse(bits[0], out _tapX) || !int.TryParse(bits[1], out _tapY))
					{
						Log.Write(LogChannel.General, "drive: tap wants <x> <y> [holdMs] in the 800x480 view");
						break;
					}
					int hold = bits.Length > 2 && int.TryParse(bits[2], out int ms) ? ms : 120;
					_framesLeft = Math.Max(2, hold * StepsPerSecond / 1000);
					_tapHeld = true;
					DesktopInput.InjectTouch(0, _tapX, _tapY);
					Log.Write(LogChannel.File, "drive: tap " + _tapX + "," + _tapY + " for " + _framesLeft + " frame(s)");
					break;
				}
				case "type":
					// Into the text field that is open (the name entry's), as typing would.
					if (TextEntry.Instance != null && TextEntry.Instance.IsActive)
					{
						TextEntry.Instance.Inject(step.Arg);
						Log.Write(LogChannel.File, "drive: typed \"" + step.Arg + "\"");
					}
					else Log.Write(LogChannel.General, "drive: type - no text field is open");
					break;
				case "flag":
				{
					// A game flag set or cleared: "flag 0:14 on" - to put a map in a story state
					// without playing there (a scene that boots under it, a chest opened).
					string[] bits = step.Arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					string[] pair = bits.Length > 0 ? bits[0].Split(':') : new string[0];
					if (pair.Length != 2 || !uint.TryParse(pair[0], out uint group) || !uint.TryParse(pair[1], out uint index))
					{
						Log.Write(LogChannel.General, "drive: flag wants <group>:<index> [on|off]");
						break;
					}
					bool on = bits.Length < 2 || !string.Equals(bits[1], "off", StringComparison.OrdinalIgnoreCase);
					try
					{
						if (on) GlobalScope.FlagManager.singleton().set(group, index); else GlobalScope.FlagManager.singleton().reset(group, index);
						Log.Write(LogChannel.File, "drive: flag " + group + ":" + index + (on ? " on" : " off"));
					}
					catch (Exception ex) { Log.Write(LogChannel.General, "drive: flag " + bits[0] + " failed: " + ex.Message); }
					break;
				}
				case "party":
				{
					// "party 3,9,10 30": a test party of these FF4 character types at level 30, in place of the one there is.
					string[] bits = step.Arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					List<int> types = new List<int>();
					foreach (string t in bits.Length > 0 ? bits[0].Split(',') : new string[0]) if (int.TryParse(t, out int ty)) types.Add(ty);
					int level = bits.Length > 1 && int.TryParse(bits[1], out int lv) ? lv : 10;
					if (types.Count == 0 || !GameProfile.IsFf4) { Log.Write(LogChannel.General, "drive: party wants <type,type,...> [level] (FF4)"); break; }
					try { Ff4Party.TestParty(types, level); }
					catch (Exception ex) { Log.Write(LogChannel.General, "drive: party failed: " + ex.Message); }
					break;
				}
				case "member":
				{
					// A character into the party, as a mod would (Game.Party.AddMember) - a screen that moves between heroes, tested with more than one.
					if (!int.TryParse(step.Arg.Trim(), out int id)) { Log.Write(LogChannel.General, "drive: member wants <id>"); break; }
					try { Log.Write(LogChannel.File, "drive: member " + id + (OpenFF.Game.Party.AddMember(id) ? " joins" : " - refused (the party full, or no such)")); }
					catch (Exception ex) { Log.Write(LogChannel.General, "drive: member failed: " + ex.Message); }
					break;
				}
				case "item":
				case "equip":
				case "battle":
				{
					// Through the engine's own API, as a mod would: "item 1001 2" puts two of an item in the
					// bag, "equip 0 1001" puts one on party member 0 (the item's own slot), "battle 1 [map]"
					// starts the fight with that formation on that battle map.
					string[] bits = step.Arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					int a = bits.Length > 0 && int.TryParse(bits[0], out int a0) ? a0 : -1;
					int b = bits.Length > 1 && int.TryParse(bits[1], out int b0) ? b0 : (step.Verb == "item" ? 1 : (step.Verb == "battle" ? 0 : -1));
					if (a < 0 || b < 0)
					{
						Log.Write(LogChannel.General, "drive: " + step.Verb + " wants " + (step.Verb == "item" ? "<itemId> [count]" : step.Verb == "equip" ? "<member> <itemId>" : "<formation> [battleMap]"));
						break;
					}
					try
					{
						if (step.Verb == "item") { OpenFF.Game.Party.AddItem(a, b); Log.Write(LogChannel.File, "drive: item " + a + " x" + b); }
						else if (step.Verb == "equip") Log.Write(LogChannel.File, "drive: equip " + b + " on member " + a + (OpenFF.Game.Party.Equip(a, b) ? "" : " - refused"));
						else { OpenFF.Game.Battle.Start(a, b); Log.Write(LogChannel.File, "drive: battle " + a + " on map " + b); }
					}
					catch (Exception ex) { Log.Write(LogChannel.General, "drive: " + step.Verb + " failed: " + ex.Message); }
					break;
				}
				case "gil":
				{
					if (!int.TryParse(step.Arg.Trim(), out int gil)) { Log.Write(LogChannel.General, "drive: gil wants <amount>"); break; }
					try { OpenFF.Game.Party.Gil = gil; Log.Write(LogChannel.File, "drive: gil " + gil); }
					catch (Exception ex) { Log.Write(LogChannel.General, "drive: gil failed: " + ex.Message); }
					break;
				}
				case "jump":
				{
					string[] p = step.Arg.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
					if (p.Length == 0) { Log.Write(LogChannel.General, "drive: jump wants <stage> [x y z]"); break; }
					GlobalScope.VecFx32 at = new GlobalScope.VecFx32(0, 0, 4096);
					if (p.Length >= 4 && float.TryParse(p[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float x)
						&& float.TryParse(p[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float y)
						&& float.TryParse(p[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float z))
					{
						at = new GlobalScope.VecFx32((int)(x * 4096), (int)(y * 4096), (int)(z * 4096));
					}
					try
					{
						GlobalScope.CCastCommandTransit.getInstance().castParam_MapJump().initialize();
						GlobalScope.CCastCommandTransit.getInstance().castParam_MapJump().setUp(p[0], 0, at, new GlobalScope.VecFx32(0, 0, 0), true);
						GlobalScope.CCastCommandTransit.getInstance().cast_BaseSystem().setMapJump(true);
						Log.Write(LogChannel.General, "drive: jump to " + p[0]);
					}
					catch (Exception ex) { Log.Write(LogChannel.General, "drive: jump failed: " + ex.Message); }
					break;
				}
				case "characters":
					Log.Write(LogChannel.General, "drive: characters" + GlobalScope.characterMng.DescribeCharacters());
					break;
				case "save":
				{
					if (!int.TryParse(step.Arg.Trim(), out int slot)) { Log.Write(LogChannel.General, "drive: save wants <slot>"); break; }
					Log.Write(LogChannel.General, "drive: save " + slot + (GameProfile.IsFf4 && Ff4Saves.Save(slot) ? " written" : " not written (FF4, in a map)"));
					break;
				}
				case "shop":
				{
					// The game's shop screen, as a mod opens it: "shop 0" Ur's weaponsmith (t01.shp), "shop 2 t02" another town's.
					string[] bits = step.Arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					if (bits.Length < 1 || !int.TryParse(bits[0], out int index)) { Log.Write(LogChannel.General, "drive: shop wants <index> [table]"); break; }
					try { Log.Write(LogChannel.File, "drive: shop " + index + (bits.Length > 1 ? " of " + bits[1] : "") + (OpenFF.Game.Shops.Open(index, bits.Length > 1 ? bits[1] : null) ? "" : " - refused")); }
					catch (Exception ex) { Log.Write(LogChannel.General, "drive: shop failed: " + ex.Message); }
					break;
				}
				case "learn":
				{
					// A spell taught to a party member through the API: "learn 0 4101" (Fire to the first).
					string[] bits = step.Arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					if (bits.Length < 2 || !int.TryParse(bits[0], out int member) || !int.TryParse(bits[1], out int spell))
					{
						Log.Write(LogChannel.General, "drive: learn wants <member> <spellId>");
						break;
					}
					try { Log.Write(LogChannel.File, "drive: learn " + spell + " on member " + member + (OpenFF.Game.Party.LearnSpell(member, spell) ? "" : " - refused")); }
					catch (Exception ex) { Log.Write(LogChannel.General, "drive: learn failed: " + ex.Message); }
					break;
				}
				case "augment":
				{
					// An augment: "augment 0 17" (Dualcast learned by the first member), "augment 0 9139" (the Counter item used on it).
					string[] bits = step.Arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					if (bits.Length < 2 || !int.TryParse(bits[0], out int slot) || !int.TryParse(bits[1], out int ability)) { Log.Write(LogChannel.General, "drive: augment wants <member> <ability id>"); break; }
					OpenFF.Data.Character who = slot >= 0 && slot < Ff4Party.Party.Members.Count ? Ff4Party.Party.Members[slot] : null;
					// An augment item (9104..9166) used on the member as the Item menu does; an ability id learned outright.
					if (who != null && ability >= 9100) { Ff4Party.Party.AddItem(ability, 1); Log.Write(LogChannel.File, "drive: " + Ff4Augments.Use(ability, who)); }
					else if (who != null && !who.Abilities.Contains(ability)) { who.Abilities.Add(ability); Ff4Augments.Slots(who); }
					Log.Write(LogChannel.File, "drive: augment " + ability + " on " + (who?.Name ?? "no one"));
					break;
				}
				case "job":
				{
					// A party member's job through the API: "job 0 Evoker" (a name as definitions write it, or its number).
					string[] bits = step.Arg.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
					int job = bits.Length > 1 ? OpenFF.Data.ModCharacters.JobNumber(bits[1]) : -1;
					if (bits.Length < 2 || !int.TryParse(bits[0], out int member) || job < 0) { Log.Write(LogChannel.General, "drive: job wants <member> <job>"); break; }
					try { OpenFF.Game.Party.SetJob(member, (OpenFF.Job)job); Log.Write(LogChannel.File, "drive: job " + OpenFF.Data.ModCharacters.Jobs[job].Name + " on member " + member); }
					catch (Exception ex) { Log.Write(LogChannel.General, "drive: job failed: " + ex.Message); }
					break;
				}
				case "status":
				{
					// FF4: a battle condition put on (or, already on, taken off) a fighter - "status party 0 8" poisons the
					// first member, "status foe 1 1" puts the second foe to sleep (ys::Condition ids).
					string[] bits = step.Arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					if (bits.Length < 3 || !int.TryParse(bits[1], out int who) || !int.TryParse(bits[2], out int condition)) { Log.Write(LogChannel.General, "drive: status wants party|foe <index> <condition>"); break; }
					bool done = Ff4Battle.Instance != null && Ff4Battle.Instance.ToggleCondition(bits[0] == "foe", who, condition);
					Log.Write(LogChannel.File, "drive: status " + step.Arg + (done ? "" : " - no such fighter"));
					break;
				}
				case "level":
				{
					// A party member's level through the API: "level 0 99" (its charges and stats grow with it).
					string[] bits = step.Arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					if (bits.Length < 2 || !int.TryParse(bits[0], out int member) || !int.TryParse(bits[1], out int level)) { Log.Write(LogChannel.General, "drive: level wants <member> <level>"); break; }
					try { OpenFF.Game.Party.SetLevel(member, level); Log.Write(LogChannel.File, "drive: level " + level + " on member " + member); }
					catch (Exception ex) { Log.Write(LogChannel.General, "drive: level failed: " + ex.Message); }
					break;
				}
				case "heal":
				{
					// The whole party's HP and charges to their maximum, as an inn has them.
					try { OpenFF.Game.Party.HealAll(); Log.Write(LogChannel.File, "drive: heal"); }
					catch (Exception ex) { Log.Write(LogChannel.General, "drive: heal failed: " + ex.Message); }
					break;
				}
				case "charges":
				{
					// A spell level's charges through the API: "charges 0 5 9" - member 0, level 5, nine of nine.
					string[] bits = step.Arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					if (bits.Length < 3 || !int.TryParse(bits[0], out int member) || !int.TryParse(bits[1], out int level) || !int.TryParse(bits[2], out int count))
					{
						Log.Write(LogChannel.General, "drive: charges wants <member> <level> <count>");
						break;
					}
					try { OpenFF.Game.Party.SetCharges(member, level, count, count); Log.Write(LogChannel.File, "drive: charges " + count + " at level " + level + " on member " + member); }
					catch (Exception ex) { Log.Write(LogChannel.General, "drive: charges failed: " + ex.Message); }
					break;
				}
				case "effect":
				{
					// One of the mods' effects at the hero, through the API: "effect green-fire" (6 up), "effect green-fire 0 6 8".
					string[] bits = step.Arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					if (bits.Length < 1) { Log.Write(LogChannel.General, "drive: effect wants <id> [dx dy dz]"); break; }
					float Offset(int i, float otherwise) => bits.Length > i && float.TryParse(bits[i], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float v) ? v : otherwise;
					try
					{
						OpenFF.Vector3 at = OpenFF.Game.Hero.Position + new OpenFF.Vector3(Offset(1, 0), Offset(2, 6), Offset(3, 0));
						int id = OpenFF.Game.Effects.Spawn(bits[0], at);
						Log.Write(LogChannel.File, "drive: effect " + bits[0] + " at " + at + (id >= 0 ? " - " + id : " - refused"));
					}
					catch (Exception ex) { Log.Write(LogChannel.General, "drive: effect failed: " + ex.Message); }
					break;
				}
				case "motion":
				{
					// The hero plays a motion by its id, through the API as a mod would: "motion 706" (the
					// fall), "motion 4101 loop" (the win pose, looping). The battle set (b_b01) is bound
					// first, so a battle motion plays on the field too - the way to look at a model's
					// pose on a map, in daylight, without fighting for it.
					// "motion 706 all end": every character on the scene that has the motion (a battle's
					// hero, whom Game.Hero does not reach) plays it, held at its last frame.
					string[] bits = step.Arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					if (bits.Length < 1 || !int.TryParse(bits[0], out int index)) { Log.Write(LogChannel.General, "drive: motion wants <index> [loop] [all] [end]"); break; }
					bool loop = Array.Exists(bits, b => string.Equals(b, "loop", StringComparison.OrdinalIgnoreCase));
					bool all = Array.Exists(bits, b => string.Equals(b, "all", StringComparison.OrdinalIgnoreCase));
					bool end = Array.Exists(bits, b => string.Equals(b, "end", StringComparison.OrdinalIgnoreCase));
					try
					{
						if (all)
						{
							int played = 0;
							for (int ctrl = 0; ctrl < 22; ctrl++)
							{
								if (!GlobalScope.characterMng.isValidCharacter(ctrl) || !GlobalScope.characterMng.isMotion(ctrl, index)) continue;
								GlobalScope.characterMng.startMotion(ctrl, index, loop, 0u);
								if (end) GlobalScope.characterMng.setCurrentFrame(ctrl, GlobalScope.characterMng.getMaxFrame(ctrl));
								played++;
							}
							Log.Write(LogChannel.File, "drive: motion " + index + " on " + played + " character(s)" + (end ? " at the end" : ""));
						}
						else
						{
							OpenFF.Game.Hero.BindMotions("b_b01");
							OpenFF.Game.Hero.PlayMotion(index, loop);
							Log.Write(LogChannel.File, "drive: motion " + index + (loop ? " loop" : ""));
						}
					}
					catch (Exception ex) { Log.Write(LogChannel.General, "drive: motion failed: " + ex.Message); }
					break;
				}
				case "camera":
				{
					// The free camera (the debug overlay's F7) placed for a screenshot: "camera x y z [yaw] [pitch]"
					// in world units and degrees (yaw 0 looks down +Z, pitch positive up); "camera off" gives the
					// game its eye back. Player input is held off while it is on, as with F7.
					string[] bits = step.Arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					if (bits.Length == 1 && string.Equals(bits[0], "off", StringComparison.OrdinalIgnoreCase)) { FreeCamera.Off(GlobalScope.m_Graphics?.getGame()); break; }
					float[] v = new float[5];
					bool ok = bits.Length >= 3;
					for (int i = 0; ok && i < Math.Min(5, bits.Length); i++) ok = float.TryParse(bits[i], NumberStyles.Float, CultureInfo.InvariantCulture, out v[i]);
					if (!ok) { Log.Write(LogChannel.General, "drive: camera wants <x> <y> <z> [yaw] [pitch], or off"); break; }
					FreeCamera.Place(new Microsoft.Xna.Framework.Vector3(v[0], v[1], v[2]), v[3], bits.Length > 4 ? v[4] : 0f, GlobalScope.m_Graphics?.getGame());
					break;
				}
				case "hp":
				{
					// A party member's HP set (Game.Party.SetHp): "hp 0 0" fells member 0 - in a battle the
					// death state plays the fall (706) and holds its last frame, the way to look at it.
					string[] bits = step.Arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					if (bits.Length < 2 || !int.TryParse(bits[0], out int member) || !int.TryParse(bits[1], out int hp)) { Log.Write(LogChannel.General, "drive: hp wants <member> <hp>"); break; }
					try { OpenFF.Game.Party.SetHp(member, hp); Log.Write(LogChannel.File, "drive: hp " + hp + " on member " + member); }
					catch (Exception ex) { Log.Write(LogChannel.General, "drive: hp failed: " + ex.Message); }
					break;
				}
				case "submit":
				case "cancel":
					// Enter or Escape on the open text field.
					if (TextEntry.Instance != null && TextEntry.Instance.IsActive)
					{
						TextEntry.Instance.Finish(step.Verb == "submit");
						Log.Write(LogChannel.File, "drive: " + step.Verb);
					}
					else Log.Write(LogChannel.General, "drive: " + step.Verb + " - no text field is open");
					break;
				case "repeat":
				case "until":
				{
					string pattern = step.Arg;
					if (step.Verb == "repeat")
					{
						// repeat <key> <seconds> until <regex> [timeout]
						string[] head = pattern.Split(' ', 4, StringSplitOptions.RemoveEmptyEntries);
						if (head.Length < 4 || head[2] != "until" || !Enum.TryParse(head[0], true, out Keys rk))
						{
							Log.Write(LogChannel.General, "drive: repeat <key> <seconds> until <regex> [timeout] - not " + step.Arg);
							break;
						}
						_repeatKey = rk;
						_repeatEvery = Math.Max(5, (int)Math.Round(Seconds(head[1], 1) * StepsPerSecond));
						_repeatIn = _repeatEvery;
						pattern = head[3];
					}
					double timeout = 30;
					int space = pattern.LastIndexOf(' ');
					if (space > 0 && double.TryParse(pattern.Substring(space + 1), NumberStyles.Float, CultureInfo.InvariantCulture, out double t))
					{
						timeout = t;
						pattern = pattern.Substring(0, space).Trim();
					}
					try { _waitFor = new Regex(pattern, RegexOptions.IgnoreCase); }
					catch (Exception ex) { Log.Write(LogChannel.General, "drive: bad pattern " + pattern + ": " + ex.Message); break; }
					_timeoutFrames = (int)(timeout * StepsPerSecond);
					lock (_lock)
					{
						_matched = false;
						foreach (string line in _recent)
						{
							if (_waitFor.IsMatch(line)) { _matched = true; break; }
						}
					}
					break;
				}
				case "dialogue":
				{
					string[] parts = step.Arg.Split('|');
					OpenFF.Game.Dialogue.Say(parts[0].Trim(), parts.Length > 1 ? parts[1].Trim() : null);
					Log.Write(LogChannel.File, "drive: dialogue " + step.Arg);
					break;
				}
				case "ask":
					OpenFF.Game.Dialogue.Ask(step.Arg, yes => Log.Write(LogChannel.General, "drive: answered " + (yes ? "yes" : "no")));
					Log.Write(LogChannel.File, "drive: ask " + step.Arg);
					break;
				case "qol":
				{
					string[] bits = (step.Arg ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
					if (bits.Length < 2) { Log.Write(LogChannel.General, "drive: qol wants <option> <value>"); break; }
					DisplaySettings.QolSettings q = Qol.S;
					bool on = bits[1] == "on" || bits[1] == "true" || bits[1] == "1";
					double.TryParse(bits[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double n);
					switch (bits[0].ToLowerInvariant())
					{
						case "speed": q.Speed = Math.Clamp((int)n, 1, Qol.MostSpeed); break;
						case "encounters": q.Encounters = on; break;
						case "exp": q.Exp = n; break;
						case "jobexp": q.JobExp = n; break;
						case "saveanywhere": q.SaveAnywhere = on; break;
						case "jobadjustment": q.JobAdjustment = on; break;
						case "indicator": q.Indicator = on; break;
						default: Log.Write(LogChannel.General, "drive: no qol option " + bits[0]); break;
					}
					Log.Write(LogChannel.General, "drive: qol " + bits[0] + " " + bits[1]);
					break;
				}
				case "hud":
				{
					string arg = (step.Arg ?? "").Trim();
					int space = arg.IndexOf(' ');
					string name = space < 0 ? arg : arg.Substring(0, space);
					string json = space < 0 ? null : arg.Substring(space + 1).Trim();
					try { OpenFF.Game.Hud.Set(name, string.IsNullOrEmpty(json) ? null : HudValue(System.Text.Json.JsonDocument.Parse(json).RootElement)); }
					catch (Exception ex) { Log.Write(LogChannel.General, "drive: hud " + name + ": " + ex.Message); }
					Log.Write(LogChannel.File, "drive: hud " + arg);
					break;
				}
				case "draws":
				{
					// draws <name>: the next step's 2D draws in 1920 x 1080 pixels into the screenshot folder's <name>.tsv (DrawsDump)
					string dir = Options.Get("screenshot-dir");
					if (string.IsNullOrEmpty(dir)) dir = System.IO.Path.Combine(System.AppContext.BaseDirectory, "screenshots");
					System.IO.Directory.CreateDirectory(dir);
					DrawsDump.Pending = System.IO.Path.Combine(dir, (string.IsNullOrWhiteSpace(step.Arg) ? "draws" : step.Arg.Trim()) + ".tsv");
					ScreenCapture.Burst = Math.Max(1, ScreenCapture.Burst);
					break;
				}
				case "shots":
					ScreenCapture.Burst = Math.Max(1, int.TryParse(step.Arg, out int shots) ? shots : 1);
					Log.Write(LogChannel.General, "drive: shots " + ScreenCapture.Burst);
					break;
				case "say":
					Log.Write(LogChannel.General, "drive: " + step.Arg);
					Trace.Mark(step.Arg);
					break;
				case "quit":
					_done = true;
					Log.Write(LogChannel.General, "drive: quit");
					Log.Flush();
					try { GlobalScope.m_Graphics.getGame().Exit(); } catch (Exception) { Environment.Exit(0); }
					break;
				default:
					Log.Write(LogChannel.General, "drive: unknown step '" + step.Verb + "'");
					break;
			}
		}

		private static int _timeoutFrames;
		private static bool _tapHeld;
		private static int _tapX, _tapY;

		private static double Seconds(string text, double fallback)
		{
			return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double v) ? v : fallback;
		}

		/// <summary>A JSON value as the bindings read one: objects as dictionaries, arrays as lists, numbers, text, true and false.</summary>
		private static object HudValue(System.Text.Json.JsonElement e)
		{
			switch (e.ValueKind)
			{
				case System.Text.Json.JsonValueKind.Object:
					Dictionary<string, object> map = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
					foreach (System.Text.Json.JsonProperty p in e.EnumerateObject()) map[p.Name] = HudValue(p.Value);
					return map;
				case System.Text.Json.JsonValueKind.Array: return e.EnumerateArray().Select(HudValue).ToList();
				case System.Text.Json.JsonValueKind.Number: return e.TryGetInt32(out int n) ? n : e.GetDouble();
				case System.Text.Json.JsonValueKind.True: return true;
				case System.Text.Json.JsonValueKind.False: return false;
				case System.Text.Json.JsonValueKind.String: return e.GetString();
				default: return null;
			}
		}
	}
}
