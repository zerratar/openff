// A parity trace: --trace=<file> writes what the game *did* - flags set, messages shown,
// sounds played, items and gil given, maps entered - one event per line, and a snapshot of
// every character on the map (who, where, which motion) whenever the drive marks progress
// with "say". Two runs of the same drive - the game's own scripts against a mod that
// replaced them (the exact or the components conversion) - should write the same trace;
// Tools/parity.ps1 runs both and diffs them. Nothing here changes what the game does: every
// hook is a line written after the game's own call.
//
//   frame  kind   what
//   0000   map    t01_01
//   0312   flag   1:22 on
//   0340   msg    1000142 item=5001            (a message by id, with the codes set for it)
//   0340   msg    "Hello"                      (a text the engine API showed)
//   0342   se     1/36
//   0350   item   5001 x1
//   0350   gil    +250
//   0400   mark   opened                       (the drive's say)
//   0400   who    cast 16 o001 at 10,0,155 motion 1002 shut? no
//
// Frame numbers are for reading; the comparison ignores them (load times differ run to run)
// and lines up events in order. Characters are keyed by their cast (LogicIndex) when they
// have one - a stand-in running the original cast has the same - else by model, so the
// slot a character happens to occupy (a spawned stand-in sits in a higher one) does not
// count as a difference.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenFF.Client
{
	internal static class Trace
	{
		private static StreamWriter _out;
		private static string _map;
		private static int _frame;

		/// <summary>Whether --trace names a file.</summary>
		public static bool On => _out != null;

		public static void Initialise()
		{
			string path = Options.Get("trace");
			if (string.IsNullOrEmpty(path)) return;
			try
			{
				string directory = Path.GetDirectoryName(Path.GetFullPath(path));
				if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
				_out = new StreamWriter(path, false, new UTF8Encoding(false)) { AutoFlush = true };
				Log.Write(LogChannel.General, "trace: " + path);
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "trace: " + path + " not written: " + ex.Message);
			}
		}

		/// <summary>Once a frame: the frame count, and the map when it changes.</summary>
		public static void Tick()
		{
			if (_out == null) return;
			_frame++;
			string map = null;
			try { map = GlobalScope.stg.CStageMng.CurrentName; } catch (Exception) { }
			if (!string.IsNullOrEmpty(map) && map != _map)
			{
				_map = map;
				Line("map", map);
			}
		}

		// Only a change is an event: the game resets every flag at start, and a set of a flag already set says nothing.
		private static readonly Dictionary<long, bool> _flags = new Dictionary<long, bool>();

		public static void Flag(uint group, uint index, bool on)
		{
			if (_out == null) return;
			long key = ((long)group << 32) | index;
			_flags.TryGetValue(key, out bool was);
			if (was == on) return;
			_flags[key] = on;
			Line("flag", group + ":" + index + (on ? " on" : " off"));
		}

		public static void Message(int id, int itemNameId, int gold)
		{
			if (_out == null) return;
			string codes = (itemNameId != 0 ? " item=" + itemNameId : "") + (gold != 0 ? " gold=" + gold : "");
			Line("msg", id.ToString(CultureInfo.InvariantCulture) + codes);
		}

		public static void Text(string text)
		{
			if (_out == null) return;
			Line("msg", "\"" + (text ?? "").Replace("\r", "").Replace("\n", "\\n") + "\"");
		}

		public static void Se(int archive, int number) => Line("se", archive + "/" + number);
		public static void Item(int id, int count) => Line("item", id + " x" + count);
		public static void Gil(int delta) => Line("gil", (delta >= 0 ? "+" : "") + delta);
		public static void Battle() => Line("battle", "starts");

		/// <summary>A message by id, as menu.MessageWindow sets it - the game's scripts, its chests, the engine's @id lines - with the codes set for it.</summary>
		public static void MessageId(int id)
		{
			if (_out == null) return;
			int item = 0, gold = 0;
			try { item = GlobalScope.dgs.CCtrlCodeInterface.instance().getItemId(); gold = GlobalScope.dgs.CCtrlCodeInterface.instance().getGold(); } catch (Exception) { }
			Message(id, item, gold);
		}

		/// <summary>The drive's say: a mark, then everyone on the map.</summary>
		public static void Mark(string text)
		{
			if (_out == null) return;
			Line("mark", text);
			Snapshot();
		}

		private static void Snapshot()
		{
			GlobalScope.pl.CPlayerManager players;
			try
			{
				GlobalScope.CCastCommandTransit transit = GlobalScope.CCastCommandTransit.getInstance();
				if (transit.cast_BaseSystem() == null) return;
				players = transit.cast_PlayerMng();
			}
			catch (Exception) { return; }
			if (players == null) return;

			List<string> rows = new List<string>();
			for (int i = 0; i < (int)GlobalScope.pl.FIELD_CHARACTER_NUM; i++)
			{
				GlobalScope.pl.CBasePlayer p;
				try { p = players.Player(i); } catch (Exception) { continue; }
				if (p == null || p.getCharacterId() < 0) continue;
				string model = p.getModelName() ?? "?";
				if (string.Equals(model, "Logic", StringComparison.OrdinalIgnoreCase)) continue;
				int cast = -1;
				try { cast = (int)p.LogicIndex(); } catch (Exception) { }
				GlobalScope.VecFx32 at = p.getPosition();
				string where = Units(at.x) + "," + Units(at.y) + "," + Units(at.z);
				uint motion = 0;
				try { motion = p.getMotionIndex(); } catch (Exception) { }
				bool hidden = false;
				try { hidden = p.isHidden(); } catch (Exception) { }
				string key = (i == 0 ? "hero" : cast >= 0 ? "cast " + cast : "model") + " " + model;
				// A wanderer is wherever the game's random walk took it: that it wanders is the fact.
				if (Wandering(players, i)) { rows.Add(key + " wandering" + (hidden ? " hidden" : "")); continue; }
				rows.Add(key + " at " + where + " motion " + motion + (hidden ? " hidden" : "") + (i == 0 ? " facing " + Facing(p) : ""));
			}
			// Sorted, so the order the slots were filled in is not a difference.
			rows.Sort(StringComparer.Ordinal);
			foreach (string row in rows) Line("who", row);
			Party();
		}

		/// <summary>The party's gil and bag: what a chest or a hand-in changed.</summary>
		private static void Party()
		{
			try
			{
				GlobalScope.pl.PlayerParty party = GlobalScope.pl.PlayerParty.instance();
				StringBuilder bag = new StringBuilder();
				for (int i = 0; i < 384; i++)
				{
					GlobalScope.itm.PossessionItem item = party.item().normalItem(i);
					if (item == null || item.itemId() < 0 || item.itemNumber() == 0) continue;
					if (bag.Length > 0) bag.Append(' ');
					bag.Append(item.itemId()).Append('x').Append(item.itemNumber());
				}
				Line("party", "gil " + party.gold().get() + " bag " + (bag.Length > 0 ? bag.ToString() : "-"));
			}
			catch (Exception) { }
		}

		private static bool Wandering(GlobalScope.pl.CPlayerManager players, int slot)
		{
			try
			{
				GlobalScope.pl.CPlayerHuman human = players.PlayerHuman(slot);
				return human != null && human.NPCAiManager().AiKind() == GlobalScope.pl.CNPCAiManager.AI_KIND.AI_KIND_RANDOM_MOVE;
			}
			catch (Exception) { return false; }
		}

		private static string Units(int fx) => ((int)Math.Round(fx / 4096.0)).ToString(CultureInfo.InvariantCulture);

		private static string Facing(GlobalScope.pl.CBasePlayer p)
		{
			try
			{
				GlobalScope.VecFx32 d = p.getDirection();
				// To the nearest of eight, so a turn's last frames are not a difference.
				double deg = Math.Atan2(d.x, d.z) * 180.0 / Math.PI;
				int eighth = (int)Math.Round(deg / 45.0) * 45;
				return ((eighth % 360) + 360) % 360 + "";
			}
			catch (Exception) { return "?"; }
		}

		private static void Line(string kind, string what)
		{
			if (_out == null) return;
			try { _out.WriteLine(_frame.ToString("0000", CultureInfo.InvariantCulture) + "  " + kind.PadRight(6) + " " + what); }
			catch (Exception) { }
		}
	}
}
