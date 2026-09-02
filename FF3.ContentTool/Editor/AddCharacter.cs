// Adding an NPC, which is four edits in three formats.
//
// The recipe, worked out by reading what the shipped maps do rather than by guessing:
//
//   1  .hich      a row: model, position, and a free cast number
//   2  .script    a boot call for that cast next to the others, so the character is
//                 actually put on the map
//   3  .script    a cast with that number, holding the talk sequence
//   4  .msd       the line it says, with a fresh id
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
// The talk sequence is copied from a real NPC rather than invented:
//
//   talkBegin();              turn to the player and begin
//   startMessageWindow(0);
//   startMessage2(0, <id>, 0, 0);
//   deleteMessageWindow(0);
//   talkEnd();                end the conversation
//
// Those two calls into the global script appear 1291 and 1185 times across the game.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace FF3.ContentTool.Editor
{
	internal sealed class AddCharacterResult
	{
		public bool Ok { get; set; }
		public string Error { get; set; }
		public int Cast { get; set; }
		public uint MessageId { get; set; }
		public List<string> Wrote { get; set; } = new List<string>();
		public List<string> Notes { get; set; } = new List<string>();
	}

	internal static class AddCharacter
	{
		private const string TalkBegin = "0xB6744D73";
		private const string TalkEnd = "0xC39DEA76";

		public static AddCharacterResult Add(Workspace workspace, string map, string model,
			int x, int z, string text, Func<uint, string> lookupMessage,
			CharacterIds ids)
		{
			AddCharacterResult result = new AddCharacterResult();

			string hichName = "files/" + map + ".hich";
			string scriptName = "files/" + map + ".script";
			if (!workspace.Exists(hichName) || !workspace.Exists(scriptName))
			{
				result.Error = map + " has no .hich or no .script";
				return result;
			}

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

			// ---- 4. the line, in every language that has this map's text
			//
			// Worked out but not written yet. Everything below can still fail, and a
			// half-added character - lines in nine files, no row and no script - is
			// worse than none: it leaves a mess with nothing pointing at it.
			Dictionary<string, MsdFile> pending = PrepareMessage(workspace, map, text,
				out uint messageId, out string textError);
			if (textError != null)
			{
				result.Error = textError;
				return result;
			}
			result.MessageId = messageId;

			// ---- 2 and 3. the script
			ScriptFile script = ScriptFile.Read(workspace.Read(scriptName));
			using StringWriter source = new StringWriter();
			Ffs.SourceWriter.Write(source, script, map + ".script", lookupMessage);

			string edited = Wire(source.ToString(), cast, model, messageId,
				out string scriptError);
			if (scriptError != null)
			{
				result.Error = scriptError;
				return result;
			}

			byte[] compiled;
			try
			{
				compiled = Ffs.Compiler.Compile(Ffs.Parser.Parse(edited));
			}
			catch (Exception problem)
			{
				result.Error = "the edited script did not compile: " + problem.Message;
				return result;
			}

			// Everything worked out, so write it all.
			workspace.Write(hichName, Hich.Write(entries));
			// Past every check, so now the four edits go out together.
			foreach (KeyValuePair<string, MsdFile> pair in pending)
			{
				workspace.Write(pair.Key, Msd.Write(pair.Value));
				result.Wrote.Add(pair.Key);
			}
			workspace.Write(scriptName, compiled);
			result.Wrote.Add(hichName);
			result.Wrote.Add(scriptName);

			result.Notes.Add("cast " + cast.ToString(CultureInfo.InvariantCulture)
				+ " boots with the other characters and says message "
				+ messageId.ToString(CultureInfo.InvariantCulture));

			result.Ok = true;
			return result;
		}

		/// <summary>
		/// Puts the boot call next to the ones already there, and the new cast at the
		/// end. Both as text, then compiled - so whatever comes out has been through
		/// the same compiler as everything else, and a mistake here is a build error
		/// rather than a corrupt file.
		/// </summary>
		private static string Wire(string source, int cast, string model, uint messageId,
			out string error)
		{
			error = null;
			string[] lines = source.Replace("\r\n", "\n").Split('\n');

			// Whichever form this map uses, and where the last one of them is. The two
			// that take a position are not copied - a new character takes its position
			// from its row, which is what the plain forms do.
			int lastBoot = -1;
			string form = "bootCharacter";
			for (int i = 0; i < lines.Length; i++)
			{
				string line = lines[i].TrimStart();
				if (line.StartsWith("bootCharacter(", StringComparison.Ordinal))
				{
					lastBoot = i;
					form = "bootCharacter";
				}
				else if (line.StartsWith("bootPlainCharacter(", StringComparison.Ordinal))
				{
					lastBoot = i;
					form = "bootPlainCharacter";
				}
				else if (lastBoot < 0
					&& (line.StartsWith("bootCharacter_AbsoluteCoordination(", StringComparison.Ordinal)
						|| line.StartsWith("bootCharacterAsTopPlayer(", StringComparison.Ordinal)))
				{
					// Somewhere to put it, when the map only ever places characters
					// explicitly. The new one still boots from its row.
					lastBoot = i;
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
			edited.AddRange(lines.Skip(lastBoot + 1));

			// Written in the block form, because this is code a person will read and
			// edit next - not the flat form the disassembler emits for shipped scripts.
			edited.Add(string.Empty);
			edited.Add("// added by the editor");
			edited.Add("cast " + number + " {");
			edited.Add("    init = none;");
			edited.Add("    main {");
			edited.Add("        call(2, " + TalkBegin + ");        // turn to the player");
			edited.Add("        startMessageWindow(0);");
			edited.Add(string.Format(CultureInfo.InvariantCulture,
				"        startMessage2(0, {0}, 0, 0);", messageId));
			edited.Add("        deleteMessageWindow(0);");
			edited.Add("        call(2, " + TalkEnd + ");        // end the conversation");
			edited.Add("    }");
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
