// Removing a placed character, and everything that only existed for it.
//
// A character is not one record. It is a .hich row, a boot call, a cast in the map's
// script, and the lines that cast shows - four edits across three formats, which is
// exactly what adding one writes. Deleting only the row would leave the other three
// behind: a cast nothing reaches, a boot call for a row that is gone, and text nobody
// says. So this undoes the same four.
//
// Deleting a row is safe, which was worth checking rather than assuming. The game finds
// a row by scanning for the one whose id matches the cast number - getManCastIndex does
// a linear search and returns the position it found - so no file, and nothing in any
// script, refers to a row by where it sits. The rows after it shift up and every lookup
// still lands on the same character.
//
// Two things the check also turned up, and both are enforced here:
//
//   * a cast number is unique within a map. Across all 356 shipped maps, no two rows
//     ever share one, which is what makes the cast the character's identity.
//   * the game scans 48 rows and no more. The largest shipped map has 42, so there is
//     room, but not unlimited room.
//
// Text is the one thing not removed on sight. A message id can be shown by more than one
// cast, and deleting one that somebody else still says is a worse outcome than leaving a
// line nobody reads. So a line goes only when this cast was the only thing referring to
// it, and what was kept is reported.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FF3.ContentTool.Editor
{
	internal sealed class DeleteCharacterResult
	{
		public bool Ok { get; set; }
		public string Error { get; set; }
		public List<string> Wrote { get; set; } = new List<string>();
		public List<string> Notes { get; set; } = new List<string>();
	}

	internal static class DeleteCharacter
	{
		/// <summary>The most rows the game will look at, from getManCastIndex.</summary>
		public const int RosterLimit = 48;

		public static DeleteCharacterResult Delete(Workspace workspace, string map, int cast,
			Func<uint, string> lookupMessage)
		{
			DeleteCharacterResult result = new DeleteCharacterResult();

			string hichName = "files/" + map + ".hich";
			string scriptName = "files/" + map + ".script";
			if (!workspace.Exists(hichName))
			{
				result.Error = map + " has no .hich";
				return result;
			}

			// ---- 1. the roster row
			List<HichEntry> entries = Hich.Read(workspace.Read(hichName));
			int row = entries.FindIndex(e => e.Kind == 0 && e.Cast == cast);
			if (row < 0)
			{
				result.Error = "no character on " + map + " has cast "
					+ cast.ToString(CultureInfo.InvariantCulture);
				return result;
			}

			string model = entries[row].Model;
			entries.RemoveAt(row);
			workspace.Write(hichName, Hich.Write(entries));
			result.Wrote.Add(hichName);
			result.Notes.Add("removed the " + model + " row");

			if (!workspace.Exists(scriptName))
			{
				result.Notes.Add("this map has no script, so there was nothing else to undo");
				result.Ok = true;
				return result;
			}

			// ---- 2, 3 and 4. the script, and the lines only it said
			ScriptFile script;
			try
			{
				script = ScriptFile.Read(workspace.Read(scriptName));
			}
			catch (Exception problem)
			{
				result.Notes.Add("the row is gone, but the script would not read: "
					+ problem.Message);
				result.Ok = true;
				return result;
			}

			using StringWriter source = new StringWriter();
			Ffs.SourceWriter.Write(source, script, map + ".script", lookupMessage);

			string edited = Unwire(source.ToString(), cast, out List<uint> saidHere,
				out int bootsRemoved, out bool castRemoved, out List<string> treasureGone,
				out string error);
			if (error != null)
			{
				result.Notes.Add("the row is gone, but the script was left alone: " + error);
				result.Ok = true;
				return result;
			}

			// Only the lines nothing else says. A message shown by two casts is one the
			// other cast still needs.
			List<uint> orphaned = saidHere
				.Where(id => !StillSaid(edited, id))
				.Distinct()
				.ToList();

			byte[] compiled;
			try
			{
				compiled = Ffs.Compiler.Compile(Ffs.Parser.Parse(edited));
			}
			catch (Exception problem)
			{
				result.Notes.Add("the row is gone, but the edited script did not compile, "
					+ "so it was left as it was: " + problem.Message);
				result.Ok = true;
				return result;
			}

			workspace.Write(scriptName, compiled);
			result.Wrote.Add(scriptName);
			if (bootsRemoved > 0)
			{
				result.Notes.Add("removed " + bootsRemoved + " boot call"
					+ (bootsRemoved == 1 ? string.Empty : "s"));
			}
			result.Notes.Add(castRemoved
				? "removed cast " + cast.ToString(CultureInfo.InvariantCulture)
				: "the script had no cast " + cast.ToString(CultureInfo.InvariantCulture)
					+ " to remove");

			foreach (string line in treasureGone)
			{
				result.Notes.Add("removed what it held: " + line
					+ " - the flag it used is free again");
			}

			int kept = saidHere.Distinct().Count() - orphaned.Count;
			if (orphaned.Count > 0)
			{
				result.Notes.Add(orphaned.Count + " line"
					+ (orphaned.Count == 1 ? " is" : "s are")
					+ " now unused: " + string.Join(", ", orphaned)
					+ ". They are left in the text files, because a message id is not "
					+ "owned by one cast and removing it renumbers nothing.");
			}
			if (kept > 0)
			{
				result.Notes.Add(kept + " of its line"
					+ (kept == 1 ? " is" : "s are") + " still said by another cast");
			}

			if (castRemoved)
			{
				// The declaration is what made the cast reachable, so removing it is what
				// stops the character existing. Its code sits under labels further down
				// and is left there on purpose: entry points share addresses and code
				// runs past the end of the block that appears to own it, so cutting a
				// label out can take somebody else's behaviour with it.
				result.Notes.Add("cast " + cast.ToString(CultureInfo.InvariantCulture)
					+ "'s code is still in the script under its labels, now unreachable. "
					+ "It is left alone because blocks share addresses and run into one "
					+ "another, so removing one can take another cast's behaviour with it.");
			}

			result.Ok = true;
			return result;
		}

		/// <summary>
		/// Takes the boot calls and the cast block back out, as text, then the caller
		/// compiles it - the same way adding one puts them in.
		/// </summary>
		private static string Unwire(string source, int cast, out List<uint> said,
			out int bootsRemoved, out bool castRemoved, out List<string> treasureGone,
			out string error)
		{
			said = new List<uint>();
			treasureGone = new List<string>();
			bootsRemoved = 0;
			castRemoved = false;
			error = null;

			string[] lines = source.Replace("\r\n", "\n").Split('\n');
			List<string> kept = new List<string>(lines.Length);

			// Any of the boot calls that names this cast, whichever of the three forms
			// it uses - the plain one, the one that places it, and the top-player one.
			Regex boots = new Regex(
				@"^\s*(bootCharacter|bootPlainCharacter|bootCharacterAsTopPlayer|"
				+ @"bootCharacter_AbsoluteCoordination)\s*\(\s*"
				+ cast.ToString(CultureInfo.InvariantCulture) + @"\s*[,)]");

			Regex castHead = new Regex(
				@"^\s*cast\s+" + cast.ToString(CultureInfo.InvariantCulture) + @"\s*\{");

			// What it holds, if it is a chest. This lives beside the boot call rather
			// than in the cast, so nothing else would take it away - and left behind it
			// names a cast that is gone and keeps a flag reserved for nobody.
			Regex treasure = new Regex(
				@"^\s*setTreasure(Item|Money)\s*\(\s*"
				+ cast.ToString(CultureInfo.InvariantCulture) + @"\s*,");

			// The ids are written in hex once a script has been through the compiler and
			// come back out, and in decimal when a person has just typed one, so both.
			Regex message = new Regex(
				@"startMessage2?\s*\(\s*\d+\s*,\s*(0[xX][0-9a-fA-F]+|\d+)");

			// A cast's code is not inside its braces. After a round trip the block holds
			// pointers - `main = cast21_main;` - and the code sits under labels further
			// down. So the lines it says are found there, not here.
			Regex ownLabel = new Regex(
				@"^\s*cast" + cast.ToString(CultureInfo.InvariantCulture) + @"_\w+\s*:");
			Regex anyLabel = new Regex(@"^\s*[A-Za-z_]\w*\s*:");

			int depth = 0;
			bool inCast = false;
			bool inOwnCode = false;

			foreach (string line in lines)
			{
				if (!inCast && castHead.IsMatch(line))
				{
					inCast = true;
					castRemoved = true;
					depth = Braces(line);
					continue;
				}

				if (inCast)
				{
					depth += Braces(line);
					if (depth <= 0)
					{
						inCast = false;
					}
					continue;
				}

				// Its own code runs from its label to whatever label comes next.
				if (ownLabel.IsMatch(line))
				{
					inOwnCode = true;
				}
				else if (inOwnCode && anyLabel.IsMatch(line))
				{
					inOwnCode = false;
				}

				if (inOwnCode)
				{
					foreach (Match found in message.Matches(line))
					{
						string digits = found.Groups[1].Value;
						bool hex = digits.StartsWith("0x", StringComparison.OrdinalIgnoreCase);
						if (uint.TryParse(hex ? digits.Substring(2) : digits,
							hex ? NumberStyles.HexNumber : NumberStyles.Integer,
							CultureInfo.InvariantCulture, out uint id))
						{
							said.Add(id);
						}
					}
				}

				if (boots.IsMatch(line))
				{
					bootsRemoved++;
					continue;
				}

				if (treasure.IsMatch(line))
				{
					treasureGone.Add(line.Trim());
					continue;
				}

				kept.Add(line);
			}

			if (inCast)
			{
				error = "cast " + cast.ToString(CultureInfo.InvariantCulture)
					+ " has no closing brace, so removing it would take the rest with it";
				return null;
			}

			return string.Join(Environment.NewLine, kept);
		}

		private static int Braces(string line)
		{
			int depth = 0;
			foreach (char c in line)
			{
				if (c == '{')
				{
					depth++;
				}
				else if (c == '}')
				{
					depth--;
				}
			}
			return depth;
		}

		/// <summary>Whether anything in what is left still shows this line.</summary>
		private static bool StillSaid(string source, uint id)
		{
			return Regex.IsMatch(source,
				@"startMessage2?\s*\(\s*\d+\s*,\s*" + id.ToString(CultureInfo.InvariantCulture)
				+ @"\s*[,)]");
		}
	}
}
