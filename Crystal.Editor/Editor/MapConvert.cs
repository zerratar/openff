// Converting a map's own characters into an OpenFF mod's objects - the plan for it.
//
// A character of the game's is a .hich row (model, place, cast) and a cast in the map's
// script; what the cast does is bytecode. Most casts do one of a handful of things, and
// those the mod's built-in components already do: a chest is setTreasureItem/Money in the
// boot cast (Chest); a villager is talkBegin, a message window, one or more lines, talkEnd,
// often behind flagOnJump branches and with a flagOn after (Talk, with When/Then); a prop
// has no code at all; moveCharacter_StartRandom in the boot makes one wander (Wander).
// This reads the script the way References.MessagesOf does - the disassembly, walked from
// the cast's entry points - and follows every path of flag branches, so each branch is one
// Talk with the flags along its path as the condition. A cast that does anything the
// components cannot (menus, effects, motions, camera, a shop) is reported, not converted:
// the page leaves that character the game's and says which commands stood in the way.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Crystal.Editor
{
	/// <summary>One branch of a cast's talk: the lines, under which flags, and the flags it sets.</summary>
	internal sealed class TalkPlan
	{
		public List<string> Lines { get; set; } = new List<string>();
		/// <summary>"0:14 !0:11": flags that must be on (or off, with !) for this branch.</summary>
		public string When { get; set; } = "";
		/// <summary>"0:13": flags set after the branch has been said.</summary>
		public string Then { get; set; } = "";
	}

	/// <summary>What one of the map's characters would become.</summary>
	internal sealed class CastPlan
	{
		public int Index { get; set; }
		public int Cast { get; set; }
		public string Model { get; set; }
		public int X { get; set; }
		public int Y { get; set; }
		public int Z { get; set; }
		public int RotationY { get; set; }
		/// <summary>chest, talk, prop, or unknown (left the game's).</summary>
		public string Kind { get; set; }
		/// <summary>For unknown: what stood in the way.</summary>
		public string Reason { get; set; }
		/// <summary>The commands the components do not cover, by name.</summary>
		public List<string> Commands { get; set; } = new List<string>();
		/// <summary>chest: "item" or "gil".</summary>
		public string Treasure { get; set; }
		public int TreasureValue { get; set; }
		public List<TalkPlan> Talks { get; set; } = new List<TalkPlan>();
		public bool Wander { get; set; }
		/// <summary>The walk's gait, moveCharacter_StartRandom's second operand (0 default, 1 man, 2 woman, 3 boy, 4 girl, 5 uncle, 6 aunt, 7 old man, 8 old woman).</summary>
		public int WanderGait { get; set; }
		/// <summary>The boot's own commands on the cast after booting it, as GameCast.Setup replays them (Npc.RunScript); "[flags] command(...)" for one under a test beyond the boot's.</summary>
		public List<string> Setup { get; set; } = new List<string>();
		/// <summary>A changeColorCharacter in the boot: the recoloured model the game really shows.</summary>
		public string ColorModel { get; set; }
		/// <summary>Whether it is a person (walks, turns, is talked to) rather than a thing.</summary>
		public bool Character { get; set; }
		/// <summary>Booted with bootPlainCharacter: the light walker with the model's own scale and kind (Npcs.SpawnPlain), not bootCharacter's.</summary>
		public bool Plain { get; set; }
		/// <summary>The flags the boot cast tests on its way to booting this one - "!0:14": the object is there only while they hold.</summary>
		public string When { get; set; } = "";
		/// <summary>chest: the game's flag for it, "1:22", from the treasure command.</summary>
		public string TreasureFlag { get; set; } = "";
		/// <summary>The motion set the boot binds (bindMotion) and the motion it starts (startMotionCharacter), for the idle.</summary>
		public string MotionSet { get; set; } = "";
		public int MotionIndex { get; set; }
		public bool MotionLoop { get; set; } = true;
		/// <summary>The cast's main as source lines (the scripts conversion, CastScript); null when the script has none.</summary>
		public List<string> Main { get; set; }
		/// <summary>Why the main cannot stand alone as the mod's code; null when it can.</summary>
		public string MainProblem { get; set; }
	}

	internal static class MapConvert
	{
		private const string TalkBegin = "0xB6744D73";
		private const string TalkEnd = "0xC39DEA76";

		/// <summary>The commands a talk cast may use and still be a Talk. Anything else is reported.</summary>
		private static readonly HashSet<string> TalkCommands = new HashSet<string>(StringComparer.Ordinal)
		{
			"startMessageWindow", "startMessage", "startMessage2", "deleteMessageWindow", "waitMessage",
			"flagOn", "flagOff", "flagOnJump", "flagOffJump", "jump", "end", "return", "nop", "wait", "setMessageColor",
		};

		public static List<CastPlan> Plan(Workspace workspace, string map, Func<uint, string> lookupMessage)
		{
			MapData data = MapModel.Load(workspace, map, lookupMessage);
			List<CastPlan> plans = new List<CastPlan>();
			string scriptName = "files/" + map + ".script";
			ScriptFile script = null;
			List<ScriptInstruction> code = new List<ScriptInstruction>();
			if (workspace.Exists(scriptName))
			{
				try
				{
					script = ScriptFile.Read(workspace.Read(scriptName), workspace.Ops);
					(code, _) = ScriptDisassembler.Disassemble(script);
				}
				catch (Exception)
				{
					script = null;
				}
			}
			Ffs.Mnemonics names = script?.Ops.Names ?? workspace.Ops.Names;
			Dictionary<uint, ScriptInstruction> at = code.ToDictionary(i => i.At, i => i);

			// The boot side: what the map does to each cast before anyone talks to it.
			Dictionary<int, (string kind, int value, string flag)> treasure = new Dictionary<int, (string, int, string)>();
			Dictionary<int, int> wander = new Dictionary<int, int>();
			Dictionary<int, string> colour = new Dictionary<int, string>();
			Dictionary<int, string> motionSet = new Dictionary<int, string>();
			Dictionary<int, (int index, bool loop)> motion = new Dictionary<int, (int, bool)>();
			foreach (ScriptInstruction i in code)
			{
				string name = names.Name(i.Opcode);
				if ((name == "setTreasureItem" || name == "setTreasureMoney") && i.Operands.Count >= 4 && Int(i.Operands[0], out int cast) && Int(i.Operands[1], out int value) && Int(i.Operands[2], out int group) && Int(i.Operands[3], out int index))
				{
					treasure[cast] = (name == "setTreasureItem" ? "item" : "gil", value, group.ToString(CultureInfo.InvariantCulture) + ":" + index.ToString(CultureInfo.InvariantCulture));
				}
				else if (name == "moveCharacter_StartRandom" && i.Operands.Count >= 1 && Int(i.Operands[0], out int wanderer))
				{
					// The second operand is the gait (NPC_RANDOM_MOVE_TYPE: man, woman, boy, girl, uncle, aunt, old man, old woman).
					wander[wanderer] = i.Operands.Count >= 2 && Int(i.Operands[1], out int gait) ? gait : 0;
				}
				else if (name == "changeColorCharacter" && i.Operands.Count >= 2 && Int(i.Operands[0], out int coloured) && i.Operands[1] is string model)
				{
					colour[coloured] = model;
				}
				else if (name == "bindMotion" && i.Operands.Count >= 2 && Int(i.Operands[0], out int bound) && i.Operands[1] is string set)
				{
					if (!motionSet.ContainsKey(bound)) motionSet[bound] = set;
				}
				else if (name == "startMotionCharacter" && i.Operands.Count >= 3 && Int(i.Operands[0], out int mover) && Int(i.Operands[1], out int which) && Int(i.Operands[2], out int loop))
				{
					if (!motion.ContainsKey(mover)) motion[mover] = (which, loop != 0);
				}
			}

			// Who boots whom: a character placed by the map's boot cast stands there from the
			// start and can be the mod's; one placed by another cast is a scene's - it appears
			// when the scene says, and a mod's object standing there all along would be wrong.
			Dictionary<int, HashSet<int>> bootedBy = BootedBy(script, code, names);
			int bootCast = script != null && script.Casts.Count > 0 ? (int)script.Casts.Min(c => c.Number) : 1;
			Dictionary<int, List<BootLine>> bootLines = BootLines(script, at, names, bootCast);

			foreach (MapCharacter character in data.Characters)
			{
				// The logic rows are the script's casts without a body - the boot, the scenes.
				if (string.Equals(character.KindName, "logic", StringComparison.OrdinalIgnoreCase) || string.Equals(character.Model, "Logic", StringComparison.OrdinalIgnoreCase)) continue;
				CastPlan plan = new CastPlan
				{
					Index = character.Index, Cast = character.Cast, Model = character.Model,
					X = character.X, Y = character.Y, Z = character.Z, RotationY = character.RotationY,
					Wander = wander.ContainsKey(character.Cast),
					WanderGait = wander.TryGetValue(character.Cast, out int gaitOf) ? gaitOf : 0,
					Character = !IsObjectModel(character.Model),
				};
				if (colour.TryGetValue(character.Cast, out string recoloured)) plan.ColorModel = recoloured;
				if (motionSet.TryGetValue(character.Cast, out string set)) plan.MotionSet = set;
				if (motion.TryGetValue(character.Cast, out (int index, bool loop) idle)) { plan.MotionIndex = idle.index; plan.MotionLoop = idle.loop; }
				bootedBy.TryGetValue(character.Cast, out HashSet<int> booters);
				if (booters == null || booters.Count == 0)
				{
					plan.Kind = "unknown";
					plan.Reason = "nothing boots cast " + plan.Cast + " - the row is not placed by the script";
					plan.Commands.Add("(never booted)");
					plans.Add(plan);
					continue;
				}
				if (!booters.Contains(bootCast))
				{
					// A scene's actor: the stand-in comes when the scene boots the cast, where it
					// boots it (GameCast.OnBoot), and the scene drives it. No idle or walk of its
					// own - the scene gives those.
					plan.Kind = "actor";
					plan.Reason = "cast " + plan.Cast + " is placed by cast " + string.Join(", ", booters.OrderBy(b => b)) + " (a scene): the object appears when the scene boots it";
					plan.Commands.Add("(booted by a scene: cast " + string.Join(", ", booters.OrderBy(b => b)) + ")");
					plan.Wander = false;
					plan.MotionSet = "";
					plan.MotionIndex = 0;
					plan.ColorModel = null;
					plans.Add(plan);
					continue;
				}
				// Everything the boot did to it, for the exact stand-in to replay, and the flags its boot hangs on.
				if (bootLines.TryGetValue(character.Cast, out List<BootLine> did)) FillSetup(plan, did);
				if (treasure.TryGetValue(character.Cast, out (string kind, int value, string flag) held))
				{
					plan.Kind = "chest";
					// A chest's own flag guards its boot (opened chests are not booted): When says so; a Chest component keeps the flag itself instead.
					plan.Treasure = held.kind;
					plan.TreasureValue = held.value;
					plan.TreasureFlag = held.flag;
					plan.MotionSet = "";
					plan.MotionIndex = 0;
					// Spawned the game's way (setUpWorldCharacter sends an o/w model through
					// setUpMapObject): that is where a chest's lid motions come from.
					plan.Character = false;
					plans.Add(plan);
					continue;
				}
				ScriptCast declared = script?.Casts.FirstOrDefault(c => c.Number == character.Cast);
				if (declared == null || declared.Normal == ScriptFile.NoScript)
				{
					plan.Kind = "prop";
					plans.Add(plan);
					continue;
				}
				Walk(plan, script, at, names, new List<uint> { declared.Normal }, lookupMessage);
				plans.Add(plan);
			}
			// The casts' mains as source, for the scripts conversion (CastScript): the lines from
			// "castN_main:" to the next function's label, as the disassembly writes them.
			if (script != null)
			{
				Dictionary<int, List<string>> mains = Mains(script, lookupMessage);
				foreach (CastPlan plan in plans)
				{
					if (!mains.TryGetValue(plan.Cast, out List<string> main)) continue;
					plan.Main = main;
					plan.MainProblem = Outside(main);
				}
			}
			return plans;
		}

		/// <summary>Every "castN_main:" function of the script as source lines (labels and commands, the label of the function itself left out), by cast.</summary>
		internal static Dictionary<int, List<string>> Mains(ScriptFile script, Func<uint, string> lookupMessage)
		{
			Dictionary<int, List<string>> mains = new Dictionary<int, List<string>>();
			string text;
			using (System.IO.StringWriter writer = new System.IO.StringWriter())
			{
				Ffs.SourceWriter.Write(writer, script, "script", lookupMessage);
				text = writer.ToString();
			}
			int current = -1;
			foreach (string raw in text.Split('\n'))
			{
				string line = raw.TrimEnd('\r');
				System.Text.RegularExpressions.Match head = System.Text.RegularExpressions.Regex.Match(line, @"^cast(\d+)_(main|exit|init)\s*:\s*$");
				if (head.Success)
				{
					current = head.Groups[2].Value == "main" ? int.Parse(head.Groups[1].Value, CultureInfo.InvariantCulture) : -1;
					if (current >= 0) mains[current] = new List<string>();
					continue;
				}
				if (current < 0) continue;
				// Another function's label at the margin ends this one (loc_ labels belong to it).
				if (line.Length > 0 && !char.IsWhiteSpace(line[0]) && line.EndsWith(":", StringComparison.Ordinal) && !line.StartsWith("loc_", StringComparison.Ordinal)) { current = -1; continue; }
				string t = line.Trim();
				if (t.Length == 0) continue;
				mains[current].Add(t);
			}
			return mains;
		}

		/// <summary>Why a main could not stand alone as the mod's code: a jump to a label outside it, a call into the map's own script; null when it can.</summary>
		private static string Outside(List<string> main)
		{
			HashSet<string> labels = new HashSet<string>(main.Where(l => l.EndsWith(":", StringComparison.Ordinal) && !l.Contains("(")).Select(l => l.TrimEnd(':').Trim()), StringComparer.Ordinal);
			foreach (string line in main)
			{
				if (line.StartsWith("call(", StringComparison.Ordinal) && !line.StartsWith("call(2,", StringComparison.Ordinal)) return "calls a function of the map's script: " + line;
				foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(line, @"\b(loc_[0-9A-Fa-f]+|cast\d+_\w+)\b"))
				{
					if (line.EndsWith(":", StringComparison.Ordinal)) continue;
					if (!labels.Contains(m.Value)) return "jumps to " + m.Value + ", outside its own code";
				}
			}
			return null;
		}

		/// <summary>One boot command on a cast, with the flags tested on the path to it.</summary>
		private sealed class BootLine
		{
			public string Name;
			public string Text;
			public HashSet<string> When;
			public ScriptInstruction Instruction;
		}

		/// <summary>
		/// Everything the boot cast does to each cast, in order, following every path: the
		/// boot command itself and the setup after it (treasure, motions, radii, sign effects,
		/// item events...). Any command whose cast operands (CastArgs) name the cast counts.
		/// </summary>
		private static Dictionary<int, List<BootLine>> BootLines(ScriptFile script, Dictionary<uint, ScriptInstruction> at, Ffs.Mnemonics names, int bootCast)
		{
			Dictionary<int, List<BootLine>> found = new Dictionary<int, List<BootLine>>();
			ScriptCast boot = script?.Casts.FirstOrDefault(c => (int)c.Number == bootCast);
			if (boot == null) return found;
			int paths = 0;

			void Follow(uint pc, List<string> when, HashSet<uint> seen, int depth)
			{
				while (true)
				{
					if (paths > 256 || depth > 600 || !at.TryGetValue(pc, out ScriptInstruction i) || !seen.Add(pc)) { paths++; return; }
					depth++;
					string name = names.Name(i.Opcode);
					if (name == "end" || name == "return") { paths++; return; }
					if (name == "jump") { if (!i.Target.HasValue) { paths++; return; } pc = i.Target.Value; continue; }
					if (name == "flagOnJump" || name == "flagOffJump")
					{
						string flag = Flag(i);
						bool onTaken = name == "flagOnJump";
						if (i.Target.HasValue) Follow(i.Target.Value, new List<string>(when) { (onTaken ? "" : "!") + flag }, new HashSet<uint>(seen), depth);
						when = new List<string>(when) { (onTaken ? "!" : "") + flag };
						pc = i.At + Math.Max(i.Length, 1u);
						continue;
					}
					if (name == "flagOnEnd" || name == "flagOffEnd")
					{
						when = new List<string>(when) { (name == "flagOnEnd" ? "!" : "") + Flag(i) };
					}
					if (CastArgs.Positions.TryGetValue(name, out int[] positions) && positions.Length > 0)
					{
						// The command belongs to the cast it acts on - the first cast operand
						// (turnCharacter_LookCharacter2(a, b) turns a); replayed once, by a's stand-in.
						int position = positions[0];
						if (position < i.Operands.Count && Int(i.Operands[position], out int cast))
						{
							if (!found.TryGetValue(cast, out List<BootLine> list)) found[cast] = list = new List<BootLine>();
							list.Add(new BootLine { Name = name, Text = Render(name, i), When = new HashSet<string>(when, StringComparer.Ordinal), Instruction = i });
						}
					}
					if ((name == "call" || name == "flagOnCall" || name == "flagOffCall") && i.Operands.Count >= 2
						&& i.Operands[i.Operands.Count - 2] is uint library && i.Operands[i.Operands.Count - 1] is uint function && library == 0)
					{
						// A conditional call runs the function under its flag; the code after it runs either way.
						List<string> inside = new List<string>(when);
						if (name == "flagOnCall") inside.Add(Flag(i));
						else if (name == "flagOffCall") inside.Add("!" + Flag(i));
						ScriptFunction f = script.Functions.FirstOrDefault(x => x.Id == function);
						if (f != null) Follow(f.Offset, inside, new HashSet<uint>(seen), depth);
					}
					pc = i.At + Math.Max(i.Length, 1u);
				}
			}

			foreach (uint entry in new[] { boot.Constructor, boot.Normal }) if (entry != ScriptFile.NoScript) Follow(entry, new List<string>(), new HashSet<uint>(), 0);
			return found;
		}

		/// <summary>The command as the disassembly writes it: name(operands), strings quoted, big numbers in hex. What Npc.RunScript reads.</summary>
		private static string Render(string name, ScriptInstruction i)
		{
			List<string> parts = new List<string>();
			foreach (object operand in i.Operands)
			{
				if (operand is string s) parts.Add("\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n") + "\"");
				else if (Int(operand, out int n)) parts.Add(n < 0 || n > 9999 ? "0x" + ((uint)n).ToString("X", CultureInfo.InvariantCulture) : n.ToString(CultureInfo.InvariantCulture));
				else parts.Add(operand?.ToString() ?? "0");
			}
			return name + "(" + string.Join(", ", parts) + ")";
		}

		/// <summary>
		/// The boot's setup for one cast as GameCast.Setup wants it: every command after the
		/// boot command, in order, once each; a line whose path tested flags beyond the boot's
		/// own condition carries them in brackets. The boot commands themselves are left out -
		/// the stand-in's spawn is their equivalent - but what they said (a model, a spot) goes
		/// into the plan.
		/// </summary>
		private static void FillSetup(CastPlan plan, List<BootLine> lines)
		{
			// The boot's own condition: every path that boots the cast, simplified to one
			// expression (alternatives with |). A line under the same condition needs no prefix.
			string bootWhen = Condition(lines.Where(l => l.Name.StartsWith("boot", StringComparison.Ordinal)).Select(l => l.When));
			plan.When = bootWhen;
			foreach (BootLine line in lines.Where(l => l.Name.StartsWith("boot", StringComparison.Ordinal)))
			{
				ScriptInstruction i = line.Instruction;
				if (line.Name == "bootPlainCharacter")
				{
					plan.Plain = true;
					if (i.Operands.Count >= 3 && i.Operands[2] is string model && model.Length > 0) plan.Model = model;
				}
				if (line.Name == "bootCharacter_AbsoluteCoordination" && i.Operands.Count >= 4 && Int(i.Operands[1], out int x) && Int(i.Operands[2], out int y) && Int(i.Operands[3], out int z))
				{
					plan.X = (int)Math.Round(x / 4096.0);
					plan.Y = (int)Math.Round(y / 4096.0);
					plan.Z = (int)Math.Round(z / 4096.0);
				}
			}
			// The setup lines, once each in the order first seen, each under the paths it was on.
			List<string> order = new List<string>();
			Dictionary<string, List<HashSet<string>>> paths = new Dictionary<string, List<HashSet<string>>>(StringComparer.Ordinal);
			foreach (BootLine line in lines.Where(l => !l.Name.StartsWith("boot", StringComparison.Ordinal)))
			{
				if (!paths.TryGetValue(line.Text, out List<HashSet<string>> list)) { paths[line.Text] = list = new List<HashSet<string>>(); order.Add(line.Text); }
				list.Add(line.When);
			}
			foreach (string text in order)
			{
				string when = Condition(paths[text]);
				plan.Setup.Add((when.Length > 0 && when != bootWhen ? "[" + when + "] " : "") + text);
			}
		}

		/// <summary>
		/// One flag expression for "reached on any of these paths": each path's tests as a
		/// conjunction, the paths as alternatives (" | "), simplified - contradictory paths
		/// (a flag on and off) drop, two alternatives that differ only in one flag's sense
		/// merge without it, an alternative implied by another goes. Empty when the paths
		/// cover every case. What WhenFlags.Holds reads.
		/// </summary>
		private static string Condition(IEnumerable<HashSet<string>> paths)
		{
			List<HashSet<string>> alts = new List<HashSet<string>>();
			foreach (HashSet<string> path in paths)
			{
				if (path.Any(f => path.Contains(f.StartsWith("!", StringComparison.Ordinal) ? f.Substring(1) : "!" + f))) continue;
				if (!alts.Any(a => a.SetEquals(path))) alts.Add(new HashSet<string>(path, StringComparer.Ordinal));
			}
			if (alts.Count == 0) return "";
			bool changed = true;
			while (changed)
			{
				changed = false;
				for (int a = 0; a < alts.Count && !changed; a++)
				{
					for (int b = a + 1; b < alts.Count && !changed; b++)
					{
						if (alts[a].Count != alts[b].Count) continue;
						List<string> onlyA = alts[a].Except(alts[b]).ToList();
						List<string> onlyB = alts[b].Except(alts[a]).ToList();
						if (onlyA.Count == 1 && onlyB.Count == 1 && onlyA[0].TrimStart('!') == onlyB[0].TrimStart('!'))
						{
							HashSet<string> merged = new HashSet<string>(alts[a], StringComparer.Ordinal);
							merged.Remove(onlyA[0]);
							alts.RemoveAt(b);
							alts.RemoveAt(a);
							if (!alts.Any(x => x.SetEquals(merged))) alts.Add(merged);
							changed = true;
						}
					}
				}
				// An alternative that another one implies (a superset of it) says nothing more.
				for (int a = alts.Count - 1; a >= 0 && !changed; a--)
				{
					HashSet<string> wide = alts[a];
					if (alts.Any(b => b != wide && wide.IsProperSupersetOf(b))) { alts.RemoveAt(a); changed = true; }
				}
			}
			if (alts.Any(a => a.Count == 0)) return "";
			return string.Join(" | ", alts.Select(a => string.Join(" ", a.OrderBy(f => f, StringComparer.Ordinal))).OrderBy(s => s, StringComparer.Ordinal));
		}

		/// <summary>One of the map's flags as the script uses it: who tests it, who sets it.</summary>
		internal sealed class FlagUse
		{
			/// <summary>"group:index".</summary>
			public string Flag { get; set; }
			/// <summary>The casts whose code tests it (flagOnJump, flagOffJump, flagOnEnd...).</summary>
			public List<int> TestedBy { get; set; } = new List<int>();
			/// <summary>The casts whose code sets or clears it (flagOn, flagOff), and the chests it belongs to (setTreasureItem).</summary>
			public List<int> SetBy { get; set; } = new List<int>();
			/// <summary>A chest's own flag: the cast of the chest.</summary>
			public int Chest { get; set; } = -1;
		}

		private static readonly HashSet<string> FlagTests = new HashSet<string>(StringComparer.Ordinal) { "flagOnJump", "flagOffJump", "flagOnEnd", "flagOffEnd", "flagOnCall", "flagOffCall" };
		private static readonly HashSet<string> FlagSets = new HashSet<string>(StringComparer.Ordinal) { "flagOn", "flagOff" };

		/// <summary>
		/// Every flag the map's script touches, with the casts that test and set it, sorted by
		/// group and index - what a flag field in the inspector offers, so a When is picked
		/// from the flags that mean something on this map rather than typed from memory.
		/// </summary>
		public static List<FlagUse> Flags(Workspace workspace, string map)
		{
			string scriptName = "files/" + map + ".script";
			if (!workspace.Exists(scriptName)) return new List<FlagUse>();
			ScriptFile script = ScriptFile.Read(workspace.Read(scriptName), workspace.Ops);
			(List<ScriptInstruction> code, _) = ScriptDisassembler.Disassemble(script);
			Ffs.Mnemonics names = script.Ops.Names;
			Dictionary<uint, ScriptInstruction> at = code.ToDictionary(i => i.At, i => i);
			Dictionary<string, FlagUse> uses = new Dictionary<string, FlagUse>(StringComparer.Ordinal);
			FlagUse Of(string flag)
			{
				if (!uses.TryGetValue(flag, out FlagUse use)) uses[flag] = use = new FlagUse { Flag = flag };
				return use;
			}
			foreach (ScriptCast cast in script.Casts)
			{
				HashSet<uint> seen = new HashSet<uint>();
				Stack<uint> work = new Stack<uint>();
				foreach (uint entry in new[] { cast.Constructor, cast.Normal, cast.Destructor }) if (entry != ScriptFile.NoScript) work.Push(entry);
				while (work.Count > 0)
				{
					uint pc = work.Pop();
					if (!seen.Add(pc) || !at.TryGetValue(pc, out ScriptInstruction i)) continue;
					string name = names.Name(i.Opcode);
					int who = (int)cast.Number;
					if (FlagTests.Contains(name)) { FlagUse u = Of(Flag(i)); if (!u.TestedBy.Contains(who)) u.TestedBy.Add(who); }
					else if (FlagSets.Contains(name)) { FlagUse u = Of(Flag(i)); if (!u.SetBy.Contains(who)) u.SetBy.Add(who); }
					else if ((name == "setTreasureItem" || name == "setTreasureMoney") && i.Operands.Count >= 4 && Int(i.Operands[0], out int chest) && Int(i.Operands[2], out int g) && Int(i.Operands[3], out int x))
					{
						FlagUse u = Of(g.ToString(CultureInfo.InvariantCulture) + ":" + x.ToString(CultureInfo.InvariantCulture));
						u.Chest = chest;
					}
					if (i.Target.HasValue) work.Push(i.Target.Value);
					foreach (uint t in i.Targets) work.Push(t);
					if ((name == "call" || name == "flagOnCall" || name == "flagOffCall") && i.Operands.Count >= 2
						&& i.Operands[i.Operands.Count - 2] is uint library && i.Operands[i.Operands.Count - 1] is uint function && library == 0)
					{
						foreach (ScriptFunction f in script.Functions) if (f.Id == function) work.Push(f.Offset);
					}
					if (name == "end" || name == "return" || name == "jump") continue;
					work.Push(i.At + Math.Max(i.Length, 1u));
				}
			}
			return uses.Values
				.Where(u => u.Flag != "?")
				.OrderBy(u => int.Parse(u.Flag.Split(':')[0], CultureInfo.InvariantCulture))
				.ThenBy(u => int.Parse(u.Flag.Split(':')[1], CultureInfo.InvariantCulture))
				.ToList();
		}

		/// <summary>For every cast that is booted: the casts whose reachable code boots it.</summary>
		private static Dictionary<int, HashSet<int>> BootedBy(ScriptFile script, List<ScriptInstruction> code, Ffs.Mnemonics names)
		{
			Dictionary<int, HashSet<int>> result = new Dictionary<int, HashSet<int>>();
			if (script == null) return result;
			Dictionary<uint, ScriptInstruction> at = code.ToDictionary(i => i.At, i => i);
			foreach (ScriptCast cast in script.Casts)
			{
				HashSet<uint> seen = new HashSet<uint>();
				Stack<uint> work = new Stack<uint>();
				foreach (uint entry in new[] { cast.Constructor, cast.Normal, cast.Destructor }) if (entry != ScriptFile.NoScript) work.Push(entry);
				while (work.Count > 0)
				{
					uint pc = work.Pop();
					if (!seen.Add(pc) || !at.TryGetValue(pc, out ScriptInstruction i)) continue;
					string name = names.Name(i.Opcode);
					if (name.StartsWith("boot", StringComparison.Ordinal) && CastArgs.Positions.TryGetValue(name, out int[] positions))
					{
						foreach (int position in positions)
						{
							if (position < i.Operands.Count && Int(i.Operands[position], out int booted))
							{
								if (!result.TryGetValue(booted, out HashSet<int> by)) result[booted] = by = new HashSet<int>();
								by.Add((int)cast.Number);
							}
						}
					}
					if (i.Target.HasValue) work.Push(i.Target.Value);
					foreach (uint t in i.Targets) work.Push(t);
					if ((name == "call" || name == "flagOnCall" || name == "flagOffCall") && i.Operands.Count >= 2
						&& i.Operands[i.Operands.Count - 2] is uint library && i.Operands[i.Operands.Count - 1] is uint function && library == 0)
					{
						foreach (ScriptFunction f in script.Functions) if (f.Id == function) work.Push(f.Offset);
					}
					if (name == "end" || name == "return" || name == "jump") continue;
					work.Push(i.At + Math.Max(i.Length, 1u));
				}
			}
			return result;
		}

		/// <summary>
		/// Every path from the cast's entry points to an end, as a Talk each: the messages
		/// along it in order, the flag tests taken and not taken as its condition, the flags
		/// it sets as its Then. A command outside the talk set on any path makes the whole
		/// cast unknown.
		/// </summary>
		private static void Walk(CastPlan plan, ScriptFile script, Dictionary<uint, ScriptInstruction> at, Ffs.Mnemonics names, List<uint> entries, Func<uint, string> lookupMessage)
		{
			List<TalkPlan> talks = new List<TalkPlan>();
			HashSet<string> odd = new HashSet<string>(StringComparer.Ordinal);
			int paths = 0;
			bool anyEmpty = false;

			void Follow(uint pc, List<string> when, List<string> then, List<string> lines, HashSet<uint> seen, int depth)
			{
				while (true)
				{
					if (paths > 64 || depth > 400) return;
					if (!at.TryGetValue(pc, out ScriptInstruction i) || !seen.Add(pc))
					{
						// A loop or a fall off the end: what was gathered is one path.
						End();
						return;
					}
					depth++;
					string name = names.Name(i.Opcode);
					switch (name)
					{
						case "end":
						case "return":
							End();
							return;
						case "jump":
							if (i.Target.HasValue) { pc = i.Target.Value; continue; }
							End();
							return;
						case "flagOnJump":
						case "flagOffJump":
						{
							string flag = Flag(i);
							bool onTaken = name == "flagOnJump";
							if (i.Target.HasValue)
							{
								Follow(i.Target.Value, new List<string>(when) { (onTaken ? "" : "!") + flag }, new List<string>(then), new List<string>(lines), new HashSet<uint>(seen), depth);
							}
							when = new List<string>(when) { (onTaken ? "!" : "") + flag };
							pc = i.At + Math.Max(i.Length, 1u);
							continue;
						}
						case "flagOn":
							then.Add(Flag(i));
							break;
						case "flagOff":
							then.Add("!" + Flag(i));
							break;
						case "startMessage":
						case "startMessage2":
							if (i.Operands.Count >= 2 && i.Operands[1] is uint id)
							{
								string text = lookupMessage?.Invoke(id) ?? ("message " + id);
								lines.Add(text.Replace(" / ", "\n").Replace("/ ", "\n").Replace(" /", "\n").Trim());
							}
							break;
						case "call":
						case "flagOnCall":
						case "flagOffCall":
						{
							// Library 2: the global script - only the talk frame is known. Library 0: this file's function, followed.
							if (i.Operands.Count >= 2 && i.Operands[i.Operands.Count - 2] is uint library && i.Operands[i.Operands.Count - 1] is uint function)
							{
								if (library == 2)
								{
									string hex = "0x" + function.ToString("X8", CultureInfo.InvariantCulture);
									if (hex != TalkBegin && hex != TalkEnd) odd.Add("call(2, " + hex + ")");
								}
								else if (library == 0)
								{
									ScriptFunction f = script.Functions.FirstOrDefault(x => x.Id == function);
									if (f != null) Follow(f.Offset, new List<string>(when), then, lines, new HashSet<uint>(seen), depth);
									else odd.Add(name);
								}
								else odd.Add(name);
							}
							break;
						}
						default:
							if (!TalkCommands.Contains(name)) odd.Add(name);
							break;
					}
					pc = i.At + Math.Max(i.Length, 1u);
				}

				void End()
				{
					paths++;
					if (lines.Count == 0 && then.Count == 0) { anyEmpty = true; return; }
					talks.Add(new TalkPlan { Lines = new List<string>(lines), When = string.Join(" ", when), Then = string.Join(" ", then.Distinct()) });
				}
			}

			foreach (uint entry in entries)
			{
				Follow(entry, new List<string>(), new List<string>(), new List<string>(), new HashSet<uint>(), 0);
			}

			if (odd.Count > 0)
			{
				// Code the components do not cover: the stand-in runs the cast itself (GameCast),
				// which is exact; the names say what a component version would have to do.
				plan.Kind = "script";
				plan.Commands = odd.OrderBy(c => c, StringComparer.Ordinal).ToList();
				plan.Reason = "cast " + plan.Cast + " uses " + string.Join(", ", plan.Commands.Take(6)) + (plan.Commands.Count > 6 ? " and " + (plan.Commands.Count - 6) + " more" : "");
				return;
			}
			// The same lines under different conditions collapse to one Talk without them
			// when every path says the same thing.
			if (talks.Count > 1 && talks.All(t => t.Lines.SequenceEqual(talks[0].Lines) && t.Then == talks[0].Then))
			{
				talks = new List<TalkPlan> { new TalkPlan { Lines = talks[0].Lines, When = "", Then = talks[0].Then } };
			}
			plan.Talks = talks;
			plan.Kind = talks.Count > 0 ? "talk" : "prop";
			if (anyEmpty && talks.Count > 0 && talks.All(t => t.When.Length > 0))
			{
				// Some path says nothing: without a condition of its own the object stays quiet there, as the game does.
				plan.Reason = "one branch says nothing";
			}
		}

		private static string Flag(ScriptInstruction i)
		{
			return i.Operands.Count >= 2 && Int(i.Operands[0], out int g) && Int(i.Operands[1], out int x)
				? g.ToString(CultureInfo.InvariantCulture) + ":" + x.ToString(CultureInfo.InvariantCulture)
				: "?";
		}

		private static bool Int(object operand, out int value)
		{
			switch (operand)
			{
				case uint u: value = (int)u; return true;
				case int n: value = n; return true;
				case long l: value = (int)l; return true;
				default: value = 0; return false;
			}
		}

		/// <summary>o and w models are the game's map objects (chests, signs, crates); the rest are people.</summary>
		private static bool IsObjectModel(string model)
		{
			char first = string.IsNullOrEmpty(model) ? ' ' : char.ToLowerInvariant(model[0]);
			return first == 'o' || first == 'w';
		}
	}
}
