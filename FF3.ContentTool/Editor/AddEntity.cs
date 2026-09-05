// Putting something new on a map, which is several edits in three formats.
//
// The recipe, worked out by reading what the shipped maps do rather than by guessing:
//
//   1  .hich      a row: model, position, and a free cast number
//   2  .script    a boot call for that cast next to the others, so it is actually put
//                 on the map
//   3  .script    a cast with that number, holding whatever it does
//   4  .msd       the line it says, with a fresh id - if it says anything
//
// Step 2 is the one that is easy to miss. A .hich row on its own places nothing: it is
// a roster, and something has to boot from it. That command reads the position straight
// out of the row it names.
//
// There are four boot calls, not one, and which a map uses matters: bootCharacter turns
// up in 282 of the 356 maps and bootPlainCharacter in 11. Anchoring on the wrong one is
// why adding a character used to fail almost everywhere. The new call copies whichever
// form the map already uses.
//
// What goes in the cast is the behaviour, and all three are copied from real ones:
//
//   talk    talkBegin, a message window, the line, talkEnd. Those two calls into the
//           global script appear 1291 and 1185 times across the game.
//
//   chest   nothing at all in the cast - a chest's whole behaviour is one command in
//           the map's boot cast:
//
//             bootCharacter(21, 0);
//             setTreasureItem(21, 3101, 1, 8, 0, 0);
//
//           which is Altar Cave's first chest, holding a Leather Cap. The arguments are
//           cast, item, flag group, flag index, encounter, quiet - read off
//           ff3Command_SetTreasureItem, which looks the cast up, remembers the flag on
//           the object, and either sets it to the opened pose or gives it the item and
//           closes it. How many of the item is not an argument: the game works that out
//           from the item itself. setTreasureMoney is the same shape with gold in place
//           of the item.
//
//   prop    an empty cast. It stands there, and something else can drive it later.
//
// A chest must be an object model - one whose name starts with o or w. That is not a
// convention: setUpWorldCharacter switches on the first letter and only those two go
// through setUpMapObject, and setTreasureItem then subtracts the field-character offset
// to get back to a map-object slot. Give it a person and the subtraction lands outside
// the array.

using FF3.Content;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace FF3.ContentTool.Editor
{
	/// <summary>What a new entity does once it is on the map.</summary>
	internal enum EntityBehaviour
	{
		/// <summary>Turns to the player and says a line.</summary>
		Talk,

		/// <summary>A chest holding an item.</summary>
		Chest,

		/// <summary>A chest holding gold.</summary>
		Money,

		/// <summary>Stands there and does nothing.</summary>
		Prop
	}

	internal sealed class AddEntityResult
	{
		public bool Ok { get; set; }
		public string Error { get; set; }
		public int Cast { get; set; }
		public uint MessageId { get; set; }
		public int FlagGroup { get; set; }
		public int FlagIndex { get; set; }
		public List<string> Wrote { get; set; } = new List<string>();
		public List<string> Notes { get; set; } = new List<string>();
	}

	internal static class AddEntity
	{
		private const string TalkBegin = "0xB6744D73";
		private const string TalkEnd = "0xC39DEA76";

		/// <summary>The group every one of the game's 423 chests keeps its flag in.</summary>
		public const int TreasureFlagGroup = 1;

		/// <summary>pl.MAP_OBJECT_NUM - how many objects a map can have at once.</summary>
		public const int MapObjectLimit = 24;

		public static AddEntityResult Add(Workspace workspace, string map, string model,
			int x, int z, EntityBehaviour behaviour, string text, int item, int gold,
			Func<uint, string> lookupMessage, CharacterIds ids, FlagIndex flags)
		{
			AddEntityResult result = new AddEntityResult();

			string hichName = "files/" + map + ".hich";
			string scriptName = "files/" + map + ".script";
			if (!workspace.Exists(hichName) || !workspace.Exists(scriptName))
			{
				result.Error = map + " has no .hich or no .script";
				return result;
			}

			bool treasure = behaviour == EntityBehaviour.Chest
				|| behaviour == EntityBehaviour.Money;

			// ---- 1. the roster row
			//
			// A row needs the model's id, not just its name. It used to have to be
			// copied off another row on the same map, which meant only models already
			// there could be placed; the id is global, so it can be looked up instead.
			List<HichEntry> entries = Hich.Read(workspace.Read(hichName));
			uint? characterId = ids?.For(model);
			if (characterId == null)
			{
				result.Error = "nothing in the game says what id the model " + model
					+ " should carry, so a row for it would load the wrong thing";
				return result;
			}

			if (entries.Count >= DeleteCharacter.RosterLimit)
			{
				result.Error = map + " already has " + entries.Count
					+ " rows, and the game only ever looks at "
					+ DeleteCharacter.RosterLimit;
				return result;
			}

			if (treasure && !IsObject(model))
			{
				result.Error = "a chest has to be an object model - one whose name starts "
					+ "with o or w. " + model + " is set up as a person, and the treasure "
					+ "command looks its cast up in the map-object list, which a person is "
					+ "not in";
				return result;
			}

			if (IsObject(model))
			{
				int objects = entries.Count(e => e.Kind == 0 && IsObject(e.Model));
				if (objects >= MapObjectLimit)
				{
					result.Error = map + " already has " + objects
						+ " object models, and a map holds " + MapObjectLimit;
					return result;
				}
			}

			if (behaviour == EntityBehaviour.Talk && string.IsNullOrWhiteSpace(text))
			{
				result.Error = "a talking character needs a line to say";
				return result;
			}

			HichEntry template = entries.FirstOrDefault(e =>
				e.Kind == 0 && string.Equals(e.Model, model, StringComparison.Ordinal))
				?? entries.FirstOrDefault(e => e.Kind == 0);

			int cast = entries.Count == 0 ? 1 : entries.Max(e => e.Cast) + 1;
			result.Cast = cast;

			entries.Add(new HichEntry
			{
				CharacterId = characterId.Value,
				Model = model,
				ModelRaw = null,
				Cast = cast,
				Kind = 0,
				KindParameter = template?.KindParameter ?? 0,
				Position = new[] { x, template?.Position[1] ?? 0, z, 1 },
				Posture = new[] { 0, 0, 0, 1 },
				Scale = new[] { 1, 1, 1, 1 }
			});

			if (template == null || !string.Equals(template.Model, model, StringComparison.Ordinal))
			{
				result.Notes.Add("this map had no " + model
					+ " already, so its id came from the rest of the game");
			}

			// ---- the flag, for a chest
			//
			// Two chests sharing a flag is not a small bug - opening one empties the
			// other - so this is the lowest index in group 1 that nothing anywhere in
			// the game refers to.
			int flagIndex = 0;
			if (treasure)
			{
				flagIndex = flags?.Free(TreasureFlagGroup) ?? -1;
				if (flagIndex < 0)
				{
					result.Error = "every flag in group " + TreasureFlagGroup
						+ " is spoken for, so this chest would share one with something else";
					return result;
				}
				result.FlagGroup = TreasureFlagGroup;
				result.FlagIndex = flagIndex;
				if (flags != null && flags.Unreadable > 0)
				{
					result.Notes.Add("flag " + flagIndex + " is free in the "
						+ flags.Read + " scripts that could be read. "
						+ string.Join(", ", flags.UnreadableNames)
						+ (flags.Unreadable == 1 ? " could not be" : " could not be")
						+ ", so its flags are unknown");
				}
			}

			// ---- 4. the line, in every language that has this map's text
			//
			// Worked out but not written yet. Everything below can still fail, and a
			// half-added character - lines in nine files, no row and no script - is
			// worse than none: it leaves a mess with nothing pointing at it.
			Dictionary<string, MsdFile> pending = null;
			uint messageId = 0;
			if (behaviour == EntityBehaviour.Talk)
			{
				pending = PrepareMessage(workspace, map, text, out messageId,
					out string textError);
				if (textError != null)
				{
					result.Error = textError;
					return result;
				}
				result.MessageId = messageId;
			}

			// ---- 2 and 3. the script
			ScriptFile script = ScriptFile.Read(workspace.Read(scriptName), workspace.Ops);
			using StringWriter source = new StringWriter();
			Ffs.SourceWriter.Write(source, script, map + ".script", lookupMessage);

			string edited = Wire(source.ToString(), cast, model, behaviour, messageId,
				item, gold, flagIndex, out string scriptError);
			if (scriptError != null)
			{
				result.Error = scriptError;
				return result;
			}

			byte[] compiled;
			try
			{
				compiled = Ffs.Compiler.Compile(Ffs.Parser.Parse(edited), workspace.Ops);
			}
			catch (Exception problem)
			{
				result.Error = "the edited script did not compile: " + problem.Message;
				return result;
			}

			// Everything worked out, so write it all.
			workspace.Write(hichName, Hich.Write(entries));
			// Past every check, so now the edits go out together.
			if (pending != null)
			{
				foreach (KeyValuePair<string, MsdFile> pair in pending)
				{
					workspace.Write(pair.Key, Msd.Write(pair.Value));
					result.Wrote.Add(pair.Key);
				}
			}
			workspace.Write(scriptName, compiled);
			result.Wrote.Add(hichName);
			result.Wrote.Add(scriptName);

			string number = cast.ToString(CultureInfo.InvariantCulture);
			switch (behaviour)
			{
				case EntityBehaviour.Talk:
					result.Notes.Add("cast " + number
						+ " boots with the other characters and says message "
						+ messageId.ToString(CultureInfo.InvariantCulture));
					break;
				case EntityBehaviour.Chest:
					result.Notes.Add("cast " + number + " holds item " + item
						+ ", and remembers being opened with flag "
						+ TreasureFlagGroup + ", " + flagIndex);
					break;
				case EntityBehaviour.Money:
					result.Notes.Add("cast " + number + " holds " + gold
						+ " gil, and remembers being opened with flag "
						+ TreasureFlagGroup + ", " + flagIndex);
					break;
				default:
					result.Notes.Add("cast " + number
						+ " stands there - its cast is empty, ready for something to go in");
					break;
			}

			result.Ok = true;
			return result;
		}

		/// <summary>Whether a model is set up as a map object rather than a person.</summary>
		public static bool IsObject(string model)
		{
			if (string.IsNullOrEmpty(model)) return false;
			char first = char.ToLowerInvariant(model[0]);
			return first == 'o' || first == 'w';
		}

		/// <summary>
		/// Puts the boot call next to the ones already there, and the new cast at the
		/// end. Both as text, then compiled - so whatever comes out has been through
		/// the same compiler as everything else, and a mistake here is a build error
		/// rather than a corrupt file.
		/// </summary>
		private static string Wire(string source, int cast, string model,
			EntityBehaviour behaviour, uint messageId, int item, int gold, int flagIndex,
			out string error)
		{
			error = null;
			string[] lines = source.Replace("\r\n", "\n").Split('\n');

			// Which block, and then where in it. Both halves matter.
			//
			// A map script is several labelled blocks, and only one of them is the map's
			// init - the one that runs every time you walk in. The others are cutscenes,
			// and a boot call in one of those runs once, or never again after the event
			// has been seen. This used to anchor on the last boot call anywhere in the
			// file, which is the init block in 199 of the 283 maps that boot anything
			// and a cutscene in the rest.
			//
			// d01_05 is one of the rest, and it is the first map you can walk around, so
			// it is what somebody tries first: its init is cast1_main, which boots casts
			// 21 to 26 and sets their treasure, while the last boot in the file is in the
			// opening cutscene, past a name entry and an event battle. A chest added
			// there appears on a new game and never on a loaded save.
			//
			// The first boot call is the better anchor - it is in cast1_main in 251 of
			// those 283 maps, against 199 for the last - and setTreasureItem agrees: it
			// sits in cast1_main in 374 of the places the shipped maps use it. So: find
			// the block the first boot is in, and insert after the last boot in that same
			// block, which keeps the new one beside its siblings.
			int firstBoot = -1;
			int lastBoot = -1;
			string bootBlock = null;
			string block = null;
			string form = "bootCharacter";

			for (int i = 0; i < lines.Length; i++)
			{
				string line = lines[i].TrimStart();

				// A label on its own line opens a block: "cast1_main:", "loc_0208:".
				if (line.EndsWith(":", StringComparison.Ordinal)
					&& line.IndexOf(' ') < 0 && line.IndexOf('(') < 0)
				{
					block = line.Substring(0, line.Length - 1);
					continue;
				}

				bool plain = line.StartsWith("bootCharacter(", StringComparison.Ordinal);
				bool named = line.StartsWith("bootPlainCharacter(", StringComparison.Ordinal);
				bool placed = line.StartsWith("bootCharacter_AbsoluteCoordination(", StringComparison.Ordinal)
					|| line.StartsWith("bootCharacterAsTopPlayer(", StringComparison.Ordinal);

				if (!plain && !named && !placed)
				{
					continue;
				}

				if (firstBoot < 0)
				{
					// The two that take a position are not copied - a new character
					// takes its position from its row - but they still say which block
					// this map does its booting in.
					firstBoot = i;
					bootBlock = block;
					if (plain) form = "bootCharacter";
					else if (named) form = "bootPlainCharacter";
				}

				if (block == bootBlock)
				{
					lastBoot = i;
					if (plain) form = "bootCharacter";
					else if (named) form = "bootPlainCharacter";
				}
			}

			if (lastBoot < 0)
			{
				error = "this map never boots a character, so there is nowhere obvious "
					+ "to boot a new one from";
				return null;
			}

			string number = cast.ToString(CultureInfo.InvariantCulture);
			List<string> edited = new List<string>(lines.Length + 16);
			edited.AddRange(lines.Take(lastBoot + 1));
			edited.Add(form == "bootPlainCharacter"
				? string.Format(CultureInfo.InvariantCulture,
					"    bootPlainCharacter({0}, 0, \"{1}\");", cast, model)
				: string.Format(CultureInfo.InvariantCulture,
					"    bootCharacter({0}, 0);", cast));

			// A chest's behaviour goes here, beside the boot call, not in its cast -
			// which is where the shipped maps put it, and it has to run once at the
			// start rather than when the player walks up.
			if (behaviour == EntityBehaviour.Chest)
			{
				edited.Add(string.Format(CultureInfo.InvariantCulture,
					"    setTreasureItem({0}, {1}, {2}, {3}, 0, 0);",
					cast, item, TreasureFlagGroup, flagIndex));
			}
			else if (behaviour == EntityBehaviour.Money)
			{
				edited.Add(string.Format(CultureInfo.InvariantCulture,
					"    setTreasureMoney({0}, {1}, {2}, {3}, 0, 0);",
					cast, gold, TreasureFlagGroup, flagIndex));
			}

			edited.AddRange(lines.Skip(lastBoot + 1));

			// Written in the block form, because this is code a person will read and
			// edit next - not the flat form the disassembler emits for shipped scripts.
			edited.Add(string.Empty);
			edited.Add("// added by the editor");
			edited.Add("cast " + number + " {");
			edited.Add("    init = none;");

			if (behaviour == EntityBehaviour.Talk)
			{
				edited.Add("    main {");
				edited.Add("        call(2, " + TalkBegin + ");        // turn to the player");
				edited.Add("        startMessageWindow(0);");
				edited.Add(string.Format(CultureInfo.InvariantCulture,
					"        startMessage2(0, {0}, 0, 0);", messageId));
				edited.Add("        deleteMessageWindow(0);");
				edited.Add("        call(2, " + TalkEnd + ");        // end the conversation");
				edited.Add("    }");
			}
			else
			{
				// Empty, like every chest in the game. The compiler closes a block off
				// with an end of its own, so this is the one line the shipped ones have.
				edited.Add("    main { }");
			}

			edited.Add("    exit { }");
			edited.Add("}");
			edited.Add(string.Empty);

			return string.Join(Environment.NewLine, edited);
		}

		/// <summary>
		/// Adds the line to every language that has this map's text, under one id.
		///
		/// The same id everywhere, because the script names one number and the game
		/// picks the file for the language it is running in. The id is one past the
		/// highest already in use, which keeps it inside the range this map's messages
		/// live in and cannot collide - lookup is a linear scan, so where it sits in
		/// the file does not matter.
		/// </summary>
		private static Dictionary<string, MsdFile> PrepareMessage(Workspace workspace,
			string map, string text, out uint id, out string error)
		{
			id = 0;
			error = null;

			List<string> files = workspace.List(".msd")
				.Select(entry => entry.Name)
				.Where(name => name.EndsWith("/" + map + ".msd", StringComparison.OrdinalIgnoreCase))
				.ToList();

			if (files.Count == 0)
			{
				error = "no .msd file for " + map + ", so there is nowhere to put the line";
				return null;
			}

			uint next = 0;
			Dictionary<string, MsdFile> decoded = new Dictionary<string, MsdFile>();
			foreach (string name in files)
			{
				MsdFile file = Msd.Read(workspace.Read(name));
				decoded[name] = file;
				foreach (MsdMessage message in file.Messages)
				{
					next = Math.Max(next, message.Id);
				}
			}
			next++;

			foreach (KeyValuePair<string, MsdFile> pair in decoded)
			{
				pair.Value.Messages.Add(new MsdMessage
				{
					Id = next,
					Pages = new List<string> { text }
				});
			}

			id = next;
			return decoded;
		}
	}
}
