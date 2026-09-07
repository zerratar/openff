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
		/// <summary>A changeColorCharacter in the boot: the recoloured model the game really shows.</summary>
		public string ColorModel { get; set; }
		/// <summary>Whether it is a person (walks, turns, is talked to) rather than a thing.</summary>
		public bool Character { get; set; }
		/// <summary>The flags the boot cast tests on its way to booting this one - "!0:14": the object is there only while they hold.</summary>
		public string When { get; set; } = "";
		/// <summary>chest: the game's flag for it, "1:22", from the treasure command.</summary>
		public string TreasureFlag { get; set; } = "";
		/// <summary>The motion set the boot binds (bindMotion) and the motion it starts (startMotionCharacter), for the idle.</summary>
		public string MotionSet { get; set; } = "";
		public int MotionIndex { get; set; }
		public bool MotionLoop { get; set; } = true;
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
			HashSet<int> wander = new HashSet<int>();
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
					wander.Add(wanderer);
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
			Dictionary<int, string> bootWhen = BootConditions(script, at, names, bootCast);

			foreach (MapCharacter character in data.Characters)
			{
				// The logic rows are the script's casts without a body - the boot, the scenes.
				if (string.Equals(character.KindName, "logic", StringComparison.OrdinalIgnoreCase) || string.Equals(character.Model, "Logic", StringComparison.OrdinalIgnoreCase)) continue;
				CastPlan plan = new CastPlan
				{
					Index = character.Index, Cast = character.Cast, Model = character.Model,
					X = character.X, Y = character.Y, Z = character.Z, RotationY = character.RotationY,
					Wander = wander.Contains(character.Cast),
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
					plan.Kind = "unknown";
					plan.Reason = "cast " + plan.Cast + " is placed by cast " + string.Join(", ", booters.OrderBy(b => b)) + " (a scene), not the map's boot";
					plan.Commands.Add("(booted by a scene: cast " + string.Join(", ", booters.OrderBy(b => b)) + ")");
					plans.Add(plan);
					continue;
				}
				if (bootWhen.TryGetValue(character.Cast, out string when)) plan.When = when;
				if (treasure.TryGetValue(character.Cast, out (string kind, int value, string flag) held))
				{
					plan.Kind = "chest";
					// A chest's own flag guards its boot (opened chests are not booted): the Chest keeps that flag itself.
					plan.When = "";
					plan.Treasure = held.kind;
					plan.TreasureValue = held.value;
					plan.TreasureFlag = held.flag;
					plan.MotionSet = "";
					plan.MotionIndex = 0;
					// Spawned the game's way (setUpWorldCharacter sends an o/w model through
					// setUpMapObject): that is where a chest's lid motions come from.
					plan.Character = true;
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
			return plans;
		}

		/// <summary>
		/// For every cast the boot cast boots: the flags tested on the way there, as a When
		/// ("!0:14" - booted only while flag 0:14 is off). Empty when booted unconditionally;
		/// absent when booted under several different conditions (then the object is simply there).
		/// </summary>
		private static Dictionary<int, string> BootConditions(ScriptFile script, Dictionary<uint, ScriptInstruction> at, Ffs.Mnemonics names, int bootCast)
		{
			Dictionary<int, HashSet<string>> found = new Dictionary<int, HashSet<string>>();
			ScriptCast boot = script?.Casts.FirstOrDefault(c => (int)c.Number == bootCast);
			if (boot == null) return new Dictionary<int, string>();
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
						// Ends the boot when the flag says so: what follows runs only when it does not.
						when = new List<string>(when) { (name == "flagOnEnd" ? "!" : "") + Flag(i) };
					}
					if (name.StartsWith("boot", StringComparison.Ordinal) && CastArgs.Positions.TryGetValue(name, out int[] positions))
					{
						foreach (int position in positions)
						{
							if (position < i.Operands.Count && Int(i.Operands[position], out int booted))
							{
								if (!found.TryGetValue(booted, out HashSet<string> set)) found[booted] = set = new HashSet<string>(StringComparer.Ordinal);
								set.Add(string.Join(" ", when));
							}
						}
					}
					if ((name == "call" || name == "flagOnCall" || name == "flagOffCall") && i.Operands.Count >= 2
						&& i.Operands[i.Operands.Count - 2] is uint library && i.Operands[i.Operands.Count - 1] is uint function && library == 0)
					{
						ScriptFunction f = script.Functions.FirstOrDefault(x => x.Id == function);
						if (f != null) Follow(f.Offset, new List<string>(when), new HashSet<uint>(seen), depth);
					}
					pc = i.At + Math.Max(i.Length, 1u);
				}
			}

			foreach (uint entry in new[] { boot.Constructor, boot.Normal }) if (entry != ScriptFile.NoScript) Follow(entry, new List<string>(), new HashSet<uint>(), 0);
			// The flags every path that boots the cast agrees on: the chest flags tested on
			// the way there vary from path to path and drop out; the one test that guards this
			// boot on every path stays.
			Dictionary<int, string> conditions = new Dictionary<int, string>();
			foreach (KeyValuePair<int, HashSet<string>> pair in found)
			{
				HashSet<string> common = null;
				foreach (string path in pair.Value)
				{
					HashSet<string> parts = new HashSet<string>(path.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries), StringComparer.Ordinal);
					if (common == null) common = parts; else common.IntersectWith(parts);
				}
				conditions[pair.Key] = common == null ? "" : string.Join(" ", common.OrderBy(c => c, StringComparer.Ordinal));
			}
			return conditions;
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
