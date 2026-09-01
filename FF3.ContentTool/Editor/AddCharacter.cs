// Adding an NPC, which is four edits in three formats.
//
// The recipe, worked out by reading what the shipped maps do rather than by guessing:
//
//   1  .hich      a row: model, position, and a free cast number
//   2  .script    bootPlainCharacter <cast>, 0, "<model>" next to the others, so the
//                 character is actually put on the map
//   3  .script    a cast with that number, holding the talk sequence
//   4  .msd       the line it says, with a fresh id
//
// Step 2 is the one that is easy to miss. A .hich row on its own places nothing: it is
// a roster, and something has to boot from it. Every shipped map does that with
// bootPlainCharacter, once per placed character - 36 rows and 36 calls in Ur - and
// that command reads the position straight out of the .hich row.
//
// The talk sequence is copied from a real NPC rather than invented:
//
//   call 2, 0xB6744D73        turn to the player and begin
//   startMessageWindow 0
//   startMessage2 0, <id>, 0, 0
//   deleteMessageWindow 0
//   call 2, 0xC39DEA76        end the conversation
//   end
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
			int x, int z, string text, Func<uint, string> lookupMessage)
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
			List<HichEntry> entries = Hich.Read(workspace.Read(hichName));
			HichEntry template = entries.FirstOrDefault(e =>
				e.Kind == 0 && string.Equals(e.Model, model, StringComparison.Ordinal));
			if (template == null)
			{
				result.Error = "no character on this map uses the model " + model
					+ ", and only models the map already loads can be placed";
				return result;
			}

			int cast = entries.Count == 0 ? 1 : entries.Max(e => e.Cast) + 1;
			result.Cast = cast;

			entries.Add(new HichEntry
			{
				CharacterId = template.CharacterId,
				Model = template.Model,
				ModelRaw = template.ModelRaw,
				Cast = cast,
				Kind = 0,
				KindParameter = template.KindParameter,
				Position = new[] { x, template.Position[1], z, template.Position[3] },
				Posture = (int[])template.Posture.Clone(),
				Scale = (int[])template.Scale.Clone()
			});

			// ---- 4. the line, in every language that has this map's text
			uint messageId = AddMessage(workspace, map, text, out List<string> textFiles,
				out string textError);
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
			workspace.Write(scriptName, compiled);
			result.Wrote.Add(hichName);
			result.Wrote.Add(scriptName);
			result.Wrote.AddRange(textFiles);

			result.Notes.Add("cast " + cast.ToString(CultureInfo.InvariantCulture)
				+ " boots with the other characters and says message "
				+ messageId.ToString(CultureInfo.InvariantCulture));
			result.Notes.Add("the model comes from " + model
				+ ", which this map already loads");
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

			int lastBoot = -1;
			for (int i = 0; i < lines.Length; i++)
			{
				if (lines[i].TrimStart().StartsWith("bootPlainCharacter ",
						StringComparison.Ordinal))
				{
					lastBoot = i;
				}
			}
			if (lastBoot < 0)
			{
				error = "this map never calls bootPlainCharacter, so there is nowhere "
					+ "obvious to boot a new character from";
				return null;
			}

			string number = cast.ToString(CultureInfo.InvariantCulture);
			List<string> edited = new List<string>(lines.Length + 16);
			edited.AddRange(lines.Take(lastBoot + 1));
			edited.Add(string.Format(CultureInfo.InvariantCulture,
				"    bootPlainCharacter {0}, 0, \"{1}\"", cast, model));
			edited.AddRange(lines.Skip(lastBoot + 1));

			edited.Add(string.Empty);
			edited.Add("// added by the editor");
			edited.Add("cast " + number + " {");
			edited.Add("    init = none");
			edited.Add("    main = cast" + number + "_main");
			edited.Add("    exit = cast" + number + "_exit");
			edited.Add("}");
			edited.Add(string.Empty);
			edited.Add("cast" + number + "_main:");
			edited.Add("    call 2, " + TalkBegin);
			edited.Add("    startMessageWindow 0");
			edited.Add(string.Format(CultureInfo.InvariantCulture,
				"    startMessage2 0, {0}, 0, 0", messageId));
			edited.Add("    deleteMessageWindow 0");
			edited.Add("    call 2, " + TalkEnd);
			edited.Add("    end");
			edited.Add(string.Empty);
			edited.Add("cast" + number + "_exit:");
			edited.Add("    end");
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
		private static uint AddMessage(Workspace workspace, string map, string text,
			out List<string> written, out string error)
		{
			written = new List<string>();
			error = null;

			List<string> files = workspace.List(".msd")
				.Select(entry => entry.Name)
				.Where(name => name.EndsWith("/" + map + ".msd", StringComparison.OrdinalIgnoreCase))
				.ToList();

			if (files.Count == 0)
			{
				error = "no .msd file for " + map + ", so there is nowhere to put the line";
				return 0;
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
				workspace.Write(pair.Key, Msd.Write(pair.Value));
				written.Add(pair.Key);
			}

			return next;
		}
	}
}
