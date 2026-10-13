// FF4's field commands that change the party (babilCommand_*, read from libff4.so): the line-up kept and put back around a
// story stretch (savePartySetting / restorePartySetting: pl::storePartyMember, restorePartyMember), a member set into a
// place, the formation, the fallen raised, statuses put on and taken off, HP set, a level matched to another's, the gil
// adjusted, the magnetized (the Magnetic Cavern's metal), a piece of equipment taken off into the bag, and the jumps on the
// last fight's result and on who leads the field.

using System;
using System.Collections.Generic;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal static class Ff4PartyCommands
	{
		/// <summary>Condition ids by the scripts' condition number (DAT_002ba4a4): Poison, Stone, Toad, Silence, Mini, Blind,
		/// Pig, Float.</summary>
		private static readonly int[] ScriptCondition = { 8, 3, 4, 5, 6, 7, 0xC, 0xF };

		private const int KO = 9, Stone = 3, Magnetized = 0x21;

		private static Party P => Ff4Party.Party;

		private static int ConditionOf(uint number) => number < ScriptCondition.Length ? ScriptCondition[number] : -1;

		private static void On(Character c, int id) { if (id >= 0 && id < 64) c.Conditions |= 1UL << id; }

		private static void Off(Character c, int id) { if (id >= 0 && id < 64) c.Conditions &= ~(1UL << id); }

		private static bool Has(Character c, int id) => id >= 0 && id < 64 && (c.Conditions >> id & 1) != 0;

		/// <summary>pl::PlayerParty::member(type): the character if it is in the party.</summary>
		private static Character Member(uint type)
		{
			Character c = P?.Get((int)type);
			return c != null && c.InParty ? c : null;
		}

		// ---- the line-up kept and put back (pl::storePartyMember / restorePartyMember) ----

		private static readonly int[] _keptPlaces = { -1, -1, -1, -1, -1 };
		private static int _keptSymbol = -1, _keptFormation = -1;

		/// <summary>savePartySetting(withFormation, ?): who stands in each of the five places, the field's leading character,
		/// and with 1 the formation.</summary>
		public static void SavePartySetting(GlobalScope.ScriptEngine engine)
		{
			bool formation = engine.getDword() == 1;
			engine.getDword();
			for (int place = 0; place < 5; place++) _keptPlaces[place] = Ff4Party.TypeAt(place);
			_keptSymbol = Ff4Speculation.FieldSymbol;
			if (formation) _keptFormation = Ff4Party.Formation;
			Log.Write(LogChannel.File, "party: line-up kept - " + string.Join(", ", _keptPlaces) + (formation ? ", formation " + Ff4Party.Formation : ""));
		}

		/// <summary>restorePartySetting(withFormation, ?): each kept member back in its place (back in the party if it had
		/// left, whoever stands there out), the leading character and with 1 the formation as kept.</summary>
		public static void RestorePartySetting(GlobalScope.ScriptEngine engine)
		{
			bool formation = engine.getDword() == 1;
			engine.getDword();
			for (int place = 0; place < 5; place++)
			{
				if (_keptPlaces[place] < 0) continue;
				Ff4Party.Seat(_keptPlaces[place], place);
				_keptPlaces[place] = -1;
			}
			Ff4Speculation.FieldSymbol = _keptSymbol;
			_keptSymbol = -1;
			if (formation && _keptFormation >= 0) Ff4Party.Formation = _keptFormation;
			Log.Write(LogChannel.File, "party: line-up put back - " + P.Members.Count + " in the party, formation " + Ff4Party.Formation);
		}

		/// <summary>SetMemberForOrder(type, place): the member out of wherever it stood (releaseMember) and into the place.</summary>
		public static void SetMemberForOrder(GlobalScope.ScriptEngine engine)
		{
			int type = (int)engine.getDword();
			int place = engine.getByte();
			if (place >= 5) return;
			Ff4Party.Seat(type, place);
			Log.Write(LogChannel.File, "party: type " + type + " into place " + place);
		}

		/// <summary>SetFormation(form, ?, ?): changeFormation unless the formation is already the other one (0 keeps 1, any other
		/// keeps 0) - the game's own test, ported as it stands.</summary>
		public static void SetFormation(GlobalScope.ScriptEngine engine)
		{
			int form = (int)engine.getDword();
			engine.getDword();
			engine.getDword();
			int now = Ff4Party.Formation;
			if (form == 0 ? now == 1 : now == 0) return;
			Ff4Party.Formation = now == 0 ? 1 : 0;
		}

		// ---- the fallen, statuses, HP ----

		/// <summary>ResurrectionDeadMember(?, ?, ?): every member in a place - the fallen up with 1 HP, the stoned freed.</summary>
		public static void ResurrectionDeadMember(GlobalScope.ScriptEngine engine)
		{
			engine.getDword();
			engine.getDword();
			engine.getDword();
			foreach (Character c in P.Members)
			{
				if (Has(c, KO))
				{
					c.Hp = c.MaxHp > 0 ? 1 : c.MaxHp;
					Off(c, KO);
				}
				else if (Has(c, Stone)) Off(c, Stone);
			}
		}

		/// <summary>SetCondition(type, condition): the status on the member - 15 every member.</summary>
		public static void SetCondition(GlobalScope.ScriptEngine engine)
		{
			uint type = engine.getDword();
			int id = ConditionOf(engine.getDword());
			if (type == 15) { foreach (Character c in P.Members) On(c, id); }
			else if (Member(type) is Character c) On(c, id);
		}

		/// <summary>ConditionRecoveryByCharacter(type, five condition numbers): those statuses off the member.</summary>
		public static void ConditionRecoveryByCharacter(GlobalScope.ScriptEngine engine)
		{
			Character c = Member(engine.getDword());
			for (int i = 0; i < 5; i++)
			{
				int id = ConditionOf(engine.getDword());
				if (c != null) Off(c, id);
			}
		}

		/// <summary>HPSetting(type, hp, ?): the member's HP, no more than its maximum (nor 9999); 0 fells it.</summary>
		public static void HpSetting(GlobalScope.ScriptEngine engine)
		{
			Character c = Member(engine.getDword());
			int hp = (int)Math.Min(engine.getDword(), 9999u);
			engine.getDword();
			if (c == null) return;
			c.Hp = Math.Min(hp, c.MaxHp);
			if (c.Hp == 0) On(c, KO);
		}

		/// <summary>SyncCharacterLv(a, b, grown, to, test, ?): when a's level against b's holds (0 a&gt;b, 1 a&gt;=b, 2 a&lt;b, 3 a&lt;=b,
		/// 4 equal), <paramref name="engine"/>'s third character grows to the fourth's level (Player::growParameter).</summary>
		public static void SyncCharacterLv(GlobalScope.ScriptEngine engine)
		{
			Character a = P.Get((int)engine.getDword()), b = P.Get((int)engine.getDword());
			int grownType = (int)engine.getDword(), toType = (int)engine.getDword();
			uint test = engine.getDword();
			engine.getDword();
			if (a == null || b == null) return;
			bool holds = test switch
			{
				0 => a.Level > b.Level,
				1 => a.Level >= b.Level,
				2 => a.Level < b.Level,
				3 => a.Level <= b.Level,
				4 => a.Level == b.Level,
				_ => false,
			};
			Character to = P.Get(toType);
			if (!holds || to == null) return;
			OpenFF.Game.Party.SetLevel(grownType, to.Level);
			Log.Write(LogChannel.File, "party: type " + grownType + " grows to level " + to.Level + " (type " + toType + "'s)");
		}

		/// <summary>EvaluateMagneticEquipment(on): with 1 each member wearing metal is magnetized (condition 0x21) and the rest
		/// are not (pl::evaluateMagnetic); with 0 none is.</summary>
		public static void EvaluateMagneticEquipment(GlobalScope.ScriptEngine engine)
		{
			bool on = engine.getDword() != 0;
			foreach (Character c in P.Members)
			{
				if (on && WearsMetal(c)) On(c, Magnetized);
				else Off(c, Magnetized);
			}
		}

		/// <summary>PlayerEquipParameter::isEquipMetal: a piece whose weapon or armour record has +0x12 bit 0.</summary>
		private static bool WearsMetal(Character c)
		{
			foreach (int id in c.Equipment)
			{
				byte[] raw = id > 0 ? Ff4Party.Tables?.Item(id)?.Raw : null;
				if (raw != null && raw.Length > 0x13 && (raw[0x12] & 1) != 0) return true;
			}
			return false;
		}

		/// <summary>DisarmamentByID_SiteSpecification(type, site, item, ?, ?): when the member wears that item in that place
		/// (0 left hand, 1 right hand, 2 head, 3 body, 4 arms), it comes off into the bag.</summary>
		public static void DisarmamentBySite(GlobalScope.ScriptEngine engine)
		{
			Character c = Member(engine.getDword());
			int site = (int)engine.getDword();
			int item = (short)engine.getDword();
			engine.getDword();
			engine.getDword();
			OpenFF.Data.EquipSlot[] slots = { OpenFF.Data.EquipSlot.LeftHand, OpenFF.Data.EquipSlot.RightHand, OpenFF.Data.EquipSlot.Head, OpenFF.Data.EquipSlot.Body, OpenFF.Data.EquipSlot.Arms };
			if (c == null || site < 0 || site > 4) return;
			int at = (int)slots[site];
			if (c.Equipment[at] != item || item <= 0) return;
			c.Equipment[at] = 0;
			P.AddItem(item, 1);
			Log.Write(LogChannel.File, "party: " + c.Name + " takes off " + (Ff4Party.Tables?.Item(item)?.Name ?? item.ToString()));
		}

		// ---- gil and jumps ----

		/// <summary>BudgetAdjust(amount, ?, ?): the gil up or down by an amount, kept within 0..9,999,999.</summary>
		public static void BudgetAdjust(GlobalScope.ScriptEngine engine)
		{
			int amount = (int)engine.getDword();
			engine.getDword();
			engine.getDword();
			P.Gil = (int)Math.Clamp((long)P.Gil + amount, 0, 9999999);
		}

		/// <summary>BattleResultJump(won, lost, escaped): a jump by how the last fight ended.</summary>
		public static void BattleResultJump(GlobalScope.ScriptEngine engine)
		{
			uint won = engine.getDword(), lost = engine.getDword(), escaped = engine.getDword();
			engine.jump(Ff4Battle.LastResult == 2 ? escaped : Ff4Battle.LastResult == 1 ? lost : won);
		}

		// ---- what the world keeps for later: Teleport's way out, the next chest fight's stage ----

		/// <summary>Where the Teleport spell takes the party from this map (WSCMapJump::setTelepo): the map, x, y, z (z negated, as
		/// the doors' arrivals) and the facing (1..7 to an angle, otherwise 0). Kept for the field's Teleport.</summary>
		public static (string Map, int X, int Y, int Z, int Facing)? Telepo;

		/// <summary>SetTelepo(map, x, y, z, facing).</summary>
		public static void SetTelepo(GlobalScope.ScriptEngine engine)
		{
			string map = engine.getString();
			int x = (int)engine.getDword(), y = (int)engine.getDword(), z = -(int)engine.getDword();
			int facing = (int)engine.getDword();
			Telepo = (map, x, y, z, facing >= 1 && facing <= 7 ? facing : 0);
			Log.Write(LogChannel.File, "field: Teleport leads to " + map + " at " + x + ", " + y + ", " + z);
		}

		/// <summary>The battle stage of the next chest fight on this map (WSCEvent +0x2C0: 1 unless set, back to 1 once used).</summary>
		public static int TreasureBattleMap = 1;

		/// <summary>SetTresureBattleMapID(stage, ?, ?).</summary>
		public static void SetTresureBattleMapID(GlobalScope.ScriptEngine engine)
		{
			TreasureBattleMap = (int)engine.getDword();
			engine.getDword();
			engine.getDword();
		}

		/// <summary>CheckTopPlayer(type, label): a jump when the field is led by that character (GameParameter::fieldSymbolID).</summary>
		public static void CheckTopPlayer(GlobalScope.ScriptEngine engine)
		{
			uint type = engine.getDword() & 0xFF;
			uint label = engine.getDword();
			int symbol = Ff4Speculation.FieldSymbol >= 0 ? Ff4Speculation.FieldSymbol : P.Leader?.Id ?? -1;
			if (symbol == (int)type) engine.jump(label);
		}
	}
}
