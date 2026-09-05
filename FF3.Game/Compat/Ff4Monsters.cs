// Game.Monsters on FF4: the unified tables through the engine's monster interface. FF3
// keeps LegacyMonsters over mon.MonsterManager. Encounter groups (the map parameters'
// monsterParty chain) are not read yet, so Group answers an empty group.

using System;
using System.Collections.Generic;
using OpenFF;
using OpenFF.Data;

namespace FF3
{
	internal sealed class Ff4Monsters : GameService, IMonsters
	{
		private List<Monster> _all;
		private Dictionary<int, Monster> _byId;

		private void Ensure()
		{
			if (_all != null) return;
			_all = new List<Monster>();
			_byId = new Dictionary<int, Monster>();
			GameTables tables = Ff4Party.Tables;
			if (tables == null) return;
			foreach (MonsterDefinition d in tables.Monsters)
			{
				Monster m = new Monster
				{
					Id = d.Id,
					Name = d.Name,
					Family = d.Family,
					// FF4's monster models are m<model>_00.nmdp, their battle motions b_m<model>.ncap.
					Model = "m" + d.ModelId.ToString("000") + "_00",
					MotionSet = "b_m" + d.ModelId.ToString("000"),
					Level = d.Level,
					MaxHp = d.MaxHp,
					Size = d.Size,
					Gil = d.Gil,
					Experience = d.Experience,
				};
				m.Stats.Strength = d.Stats.Strength;
				m.Stats.Vitality = d.Stats.Vitality;
				m.Stats.Agility = d.Stats.Agility;
				m.Stats.Intellect = d.Stats.Intellect;
				m.Stats.Mind = d.Stats.Spirit;
				_all.Add(m);
				if (!_byId.ContainsKey(m.Id)) _byId[m.Id] = m;
			}
		}

		public IReadOnlyList<Monster> All { get { Ensure(); return _all; } }

		public Monster Find(int id)
		{
			Ensure();
			return _byId.TryGetValue(id, out Monster m) ? m : null;
		}

		public Monster Find(string name)
		{
			Ensure();
			foreach (Monster m in _all) if (string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase)) return m;
			return null;
		}

		public MonsterGroup Group(int partyId)
		{
			MonsterGroup group = new MonsterGroup { Id = partyId };
			MonsterParty party = Ff4Party.Tables?.MonsterParty(partyId);
			if (party != null)
			{
				foreach (MonsterPartySlot slot in party.Slots) group.Members.Add(new MonsterCount { MonsterId = slot.MonsterId, Min = slot.Count, Max = slot.Count });
			}
			return group;
		}
	}
}
