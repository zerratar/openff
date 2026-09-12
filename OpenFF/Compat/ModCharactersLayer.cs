// The mods' character definitions (defs/characters/*.json) applied to the party where the
// game sets it up: the boot (GlobalScope.sys, a --map start) and the title's New Game
// (GlobalScope.ttl). A slot's hero gets the definition's name, job and level; a save loaded
// afterwards carries its own, as ever. Nothing without definitions; nothing with --nomods.
// FF3 only: FF4's party is its own (GameProfile.Ff3Party).

using System;
using System.Collections.Generic;
using System.Linq;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal static class ModCharactersLayer
	{
		private static List<ModCharacter> _characters;

		private static List<ModCharacter> Characters
		{
			get
			{
				if (_characters != null) return _characters;
				_characters = new List<ModCharacter>();
				if (Options.Get("nomods") != null || !GameProfile.Ff3Party) return _characters;
				List<string> roots = new List<string>();
				if (!string.IsNullOrEmpty(GameArchive.ProjectDirectory)) roots.Add(GameArchive.ProjectDirectory);
				roots.AddRange(GameArchive.ActiveMods.Select(m => m.Directory).Where(d => !string.IsNullOrEmpty(d)));
				List<string> notes = new List<string>();
				List<ModCharacter> all = ModCharacters.Load(roots, notes);
				foreach (string note in notes) Log.Write(LogChannel.General, "characters: " + note);
				// One definition per slot: the first loaded (the project's, then the mods' in order).
				HashSet<int> slots = new HashSet<int>();
				foreach (ModCharacter c in all)
				{
					if (!slots.Add(c.Slot)) { Log.Write(LogChannel.General, "characters: " + c.Id + " (" + c.Source + ") - slot " + c.Slot + " is already defined; skipped"); continue; }
					_characters.Add(c);
				}
				if (_characters.Count > 0) Log.Write(LogChannel.General, "characters: " + _characters.Count + " of the mods' own: " + string.Join(", ", _characters.Select(c => "slot " + c.Slot + " " + (c.Name ?? c.Id) + (c.Job != null ? " (" + c.Job + ")" : "") + (c.Level > 0 ? " L" + c.Level : "") + (c.Progression != Progression.Jobs ? ", " + Progressions.Word(c.Progression) + " (" + Progressions.Game(c.Progression) + ")" : ""))));
				return _characters;
			}
		}

		/// <summary>Which hero's model set a slot wears: the definition's look, or its own. Every j-model site asks (bootCharacterImp, setupHero, the battle, the menus).</summary>
		public static int ModelSet(int playerId)
		{
			try
			{
				foreach (ModCharacter c in Characters) if (c.Slot == playerId && c.Look >= 0 && c.Look <= 3) return c.Look;
			}
			catch (Exception) { }
			return playerId;
		}

		/// <summary>Which job's figures a hero wears for the job the game holds: a job of the mod's own (ProgressionLayer) says whose; the game's own otherwise. Every j-model site asks, beside ModelSet.</summary>
		public static int ModelJob(int playerId, int nowJob)
		{
			try { return ProgressionLayer.LookJob(playerId, nowJob); }
			catch (Exception) { return nowJob; }
		}

		/// <summary>Whether a slot's hero keeps its job (the job menu refuses a change): said outright, or a class (FF4's way).</summary>
		public static bool JobFixed(int playerId)
		{
			try
			{
				foreach (ModCharacter c in Characters) if (c.Slot == playerId && c.JobIsFixed) return true;
			}
			catch (Exception) { }
			return false;
		}

		/// <summary>A slot's definition, or null where the mods say nothing.</summary>
		public static ModCharacter Definition(int playerId)
		{
			try
			{
				foreach (ModCharacter c in Characters) if (c.Slot == playerId) return c;
			}
			catch (Exception) { }
			return null;
		}

		/// <summary>The folders definitions come from: the project's, then the mods' in order.</summary>
		public static List<string> Roots()
		{
			List<string> roots = new List<string>();
			if (Options.Get("nomods") != null) return roots;
			if (!string.IsNullOrEmpty(GameArchive.ProjectDirectory)) roots.Add(GameArchive.ProjectDirectory);
			roots.AddRange(GameArchive.ActiveMods.Select(m => m.Directory).Where(d => !string.IsNullOrEmpty(d)));
			return roots;
		}

		/// <summary>The party as a game begins: each defined slot's hero renamed, re-jobbed, levelled.</summary>
		public static void ApplyToNewParty()
		{
			List<ModCharacter> characters;
			try { characters = Characters; } catch (Exception) { return; }
			ProgressionLayer.Reset();
			if (characters.Count == 0) return;
			foreach (ModCharacter c in characters)
			{
				try
				{
					GlobalScope.pl.Player player = GlobalScope.pl.PlayerParty.instance().playerForId((byte)c.Slot);
					if (player == null) continue;
					if (!string.IsNullOrWhiteSpace(c.Name)) player.setName(c.Name.Trim());
					int job = ModCharacters.JobNumber(c.Job);
					if (job >= 0) player.changeJob((GlobalScope.pl.JOB_TYPE)job);
					if (c.Level > 1)
					{
						// The game's own climb: the experience for the level, then levelUp walks the
						// levels one by one - stats, MP and HP as the growth tables say for the job.
						player.setExp((byte)(c.Level - 1));
						player.levelUp(0);
					}
					player.updateParameter();
					ProgressionLayer.OnPartyReady(player);
					Log.Write(LogChannel.File, "characters: slot " + c.Slot + " is " + player.name() + (job >= 0 ? ", " + ModCharacters.Jobs[job].Name : "") + (c.Level > 1 ? ", level " + player.level().get() : ""));
				}
				catch (Exception ex)
				{
					Log.Write(LogChannel.General, "characters: slot " + c.Slot + " (" + c.Id + ") not applied: " + ex.Message);
				}
			}
		}
	}
}
