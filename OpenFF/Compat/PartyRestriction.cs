// A party held to certain heroes (Game.Party.Restrict).
//
// FF3's story adds its four heroes one by one through its events (pl.PlayerParty.addPlayer). A
// game played by several people gives each hero to one player: the others' heroes must not
// join this client's party when the story says so - the join is refused here and told to the
// mods (Events.PartyJoinRefused), so the owning player can be told the story has reached
// their hero. Null is no restriction, the game as it is.

using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenFF.Client
{
	internal static class PartyRestriction
	{
		private static HashSet<int> _owned;

		/// <summary>The heroes the party may hold, or null.</summary>
		public static IReadOnlyList<int> Owned => _owned?.OrderBy(i => i).ToList();

		public static void Set(IEnumerable<int> ids)
		{
			_owned = ids == null ? null : new HashSet<int>(ids.Where(i => i >= 0 && i < 4));
			Log.Write(LogChannel.General, "party: " + (_owned == null ? "any hero may join" : "held to hero(es) " + string.Join(", ", _owned.OrderBy(i => i))));
		}

		/// <summary>PlayerParty.addPlayer asks: a hero outside the restriction is refused, and the mods hear of it.</summary>
		public static bool Refuses(int heroId)
		{
			if (_owned == null || _owned.Contains(heroId)) return false;
			Log.Write(LogChannel.General, "party: hero " + heroId + " would join, but the party is held to others");
			try { OpenFF.Game.Events.Publish(new OpenFF.Events.PartyJoinRefused { HeroId = heroId }); } catch (Exception ex) { Log.Write(LogChannel.General, "party: " + ex.Message); }
			return true;
		}
	}
}
