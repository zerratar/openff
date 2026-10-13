// FF4's treasure chests (babilCommand_SetTreasureItem / SetTreasureMoney, object::createItemBox / createGillBox, and the
// world states that open them - WSItemBoxOpen, WSGillBoxOpen, WSEncountDirection1, WSTresureEncountSetting and
// WSMove::evaluteTresureInfo - read from libff4.so).
//
// FF4's script makes each chest itself: the command names the map's cast (its model, o000, and its place from the map's
// cast table), what it holds (an item, one of it, or gil), the flag that marks it taken (group, index), the encounter
// group of the monsters in it (0 none) and whether it is a chest at all (0) or a hidden find (a lid-less spot: "Found ...").
// A taken chest stands open (o000's motion 2). Opened: 6 frames, then the lid (motion 1), effect 804 at the chest and the
// menu's sound (0, 11); with the lid up the message window (sound 0, 6) and the line - "Obtained %item%.", "... Enemy
// ambush!" for monsters, "... but you cannot carry any more." when the bag holds 99 of it, which shuts the lid again (3)
// and leaves it. What it held is the party's once the window closes (one of the item, or the gil, within 9,999,999) and
// its flag set - but a chest of monsters first fights them (Direction1's zoom, the battle on the map's chest stage,
// setTresureBattleMapID, back to 1 after) and pays only for a win.

using System;
using System.Collections.Generic;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal sealed class Ff4Chests : GameService
	{
		public override bool WantsUpdate => true;

		internal static Ff4Chests Instance { get; private set; }
		public Ff4Chests() { Instance = this; }

		private sealed class Chest
		{
			public Npc Npc;
			public string Map;
			public int Item, Gil, FlagGroup, FlagIndex, Monster = -1;
			public bool Hidden;
			public bool HasLid => !Hidden && string.Equals(Model, "o000", StringComparison.OrdinalIgnoreCase);
			public string Model;
		}

		private readonly List<Chest> _chests = new List<Chest>();
		private Chest _opening;
		private int _step, _wait;
		private bool _won, _fighting;

		/// <summary>A chest being opened, or its fight under way: the field is the chest's.</summary>
		public static bool Busy => Instance?._opening != null;

		public static void SetTreasureItem(GlobalScope.ScriptEngine engine) => Instance?.Make(engine, false);

		public static void SetTreasureMoney(GlobalScope.ScriptEngine engine) => Instance?.Make(engine, true);

		/// <summary>SetTreasureItem / SetTreasureMoney(cast, item or gil, flag group, flag index, encounter, hidden).</summary>
		private void Make(GlobalScope.ScriptEngine engine, bool gil)
		{
			uint cast = engine.getWord();
			int value = engine.getWord();
			int group = engine.getWord(), index = engine.getWord();
			int monster = engine.getWord();
			int hidden = engine.getByte();
			string map = Game.Field.Map;
			_chests.RemoveAll(c => !string.Equals(c.Map, map, StringComparison.OrdinalIgnoreCase) || c.Npc == null || !c.Npc.Alive);
			var table = GlobalScope.evt.CHichParameterManager.getInstance().getHichManParam();
			GlobalScope.evt.CHichManParameter.HICH_MAN_INDIVIDUAL entry = null;
			for (int i = 0; i < table.m_NumMan && i < table.m_HichInd.Length; i++) if (table.m_HichInd[i].m_Id == cast) { entry = table.m_HichInd[i]; break; }
			if (entry == null) { Log.Write(LogChannel.General, "chest: cast " + cast + " is not in " + map + "'s cast table"); return; }
			Chest chest = new Chest
			{
				Map = map, Model = entry.m_CharaName, FlagGroup = group, FlagIndex = index, Monster = monster == 0 ? -1 : monster,
				Hidden = hidden != 0, Item = gil ? 0 : value, Gil = gil ? value : 0,
			};
			Vector3 at = new Vector3(entry.m_Position[0], entry.m_Position[1], entry.m_Position[2]);
			try { chest.Npc = Game.Npcs.SpawnModel(chest.Model, at, 0f); } catch (Exception ex) { Log.Write(LogChannel.General, "chest: " + chest.Model + ": " + ex.Message); }
			if (chest.Npc == null) return;
			chest.Npc.Solid = true;
			if (string.Equals(chest.Model, "o000", StringComparison.OrdinalIgnoreCase))
			{
				try { chest.Npc.BindMotions("o000"); } catch (Exception) { }
				if (Taken(chest)) { try { chest.Npc.PlayMotion(2, true, 0); } catch (Exception) { } }
			}
			chest.Npc.Interacted += _ => Open(chest);
			_chests.Add(chest);
			Log.Write(LogChannel.File, "chest: cast " + cast + " on " + map + " at " + at + " - " + (gil ? value + " gil" : "item " + value) + (chest.Monster > 0 ? ", monsters " + chest.Monster : "") + ", flag " + group + ":" + index + (Taken(chest) ? " (taken)" : ""));
		}

		private static bool Taken(Chest c) => GlobalScope.FlagManager.singleton().get((uint)c.FlagGroup, (uint)c.FlagIndex) == 1;

		private void Open(Chest chest)
		{
			// WSMove: a chest already taken is passed by (isCheckedTresureObject).
			if (_opening != null || Taken(chest) || Ff4Battle.Active) return;
			_opening = chest;
			_step = 0;
			_wait = 6;
			_won = _fighting = false;
			try { Game.Hero.Freeze(); } catch (Exception) { }
			Log.Write(LogChannel.File, "chest: opened - flag " + chest.FlagGroup + ":" + chest.FlagIndex);
		}

		public override void OnUpdate()
		{
			if (!GameProfile.IsFf4 || _opening == null) return;
			Chest c = _opening;
			switch (_step)
			{
				case 0:   // wsProcessWait: 6 frames, then the lid, its effect and sound - a hidden find has none
					if (--_wait > 0) return;
					if (c.HasLid)
					{
						try { c.Npc.PlayMotion(1, false, 0); } catch (Exception) { }
						Effect(804, c.Npc.Position);   // e804's own pack, loaded once
						Sound(0, 11);
						_step = 1;
					}
					else _step = 2;
					return;
				case 1:   // wsProcessOpenBox: the lid up
					if (c.Npc != null && !c.Npc.MotionDone) return;
					_step = 2;
					return;
				case 2:   // the window and the line
				{
					Sound(0, 6);
					bool full = c.Item > 0 && Ff4Party.Party.CountItem(c.Item) > 98;
					int line;
					if (c.Gil > 0) line = c.Monster > 0 ? 1000123 : c.Hidden ? 1000129 : 1000121;
					else if (full) line = c.Hidden ? 1000127 : 1000125;
					else line = c.Monster > 0 ? 1000124 : c.Hidden ? 1000126 : 1000122;
					// The game's line (babil_common.msd), its %item% and %money% filled in.
					string text = Ff4Layouts.CommonText((uint)line, c.Gil > 0 ? "Obtained %money% gil." : "Obtained %item%.")
						.Replace("%item%", ItemString(c.Item))
						.Replace("%money%", c.Gil.ToString());
					Game.Dialogue.Say(text);
					_full = full;
					_step = 3;
					return;
				}
				case 3:   // wsProcessMessageWait: the window closed
					if (Game.Dialogue.IsOpen) return;
					if (_full)
					{
						if (c.HasLid) { try { c.Npc.PlayMotion(3, false, 0); } catch (Exception) { } }
						Finish();
						return;
					}
					if (c.Monster <= 0) { PayOut(c); Finish(); return; }
					// "world encount1": Direction1's zoom, then WSTresureEncountSetting - the chest's monsters on the chest stage.
					_step = 4;
					_fighting = true;
					int stage = Ff4PartyCommands.TreasureBattleMap;
					Ff4PartyCommands.TreasureBattleMap = 1;
					EncounterZoom.Start(() =>
					{
						Ff4Battle.Instance?.SetNextOpening(Ff4Battle.OpenNormal);
						if (Ff4Battle.Instance == null || !Ff4Battle.Instance.StartParty(c.Monster, false, stage))
						{
							Log.Write(LogChannel.General, "chest: the fight " + c.Monster + " could not start");
							_fighting = false;
						}
					});
					return;
				case 4:   // the fight, then evaluteTresureInfo: paid only for a win
					if (EncounterZoom.Playing || Ff4Battle.Active || BlurRotate.Playing) { _won = true; return; }
					if (_won && Ff4Battle.LastResult == 0) PayOut(c);
					else Log.Write(LogChannel.File, "chest: its monsters not beaten - the chest stays");
					Finish();
					return;
			}
		}

		private bool _full;

		/// <summary>%item% as the game fills it (DGSMsdGetString of the item's id): its icon (U+E040 + its cell), then its name.</summary>
		private static string ItemString(int id)
		{
			ItemDefinition item = Ff4Party.Tables?.Item(id);
			if (item == null) return "item " + id;
			string name = item.Name ?? "";
			bool hasIcon = name.Length > 0 && name[0] >= '' && name[0] <= '';
			return (item.Icon >= 0 && !hasIcon ? ((char)(0xE040 + item.Icon)).ToString() : "") + name;
		}

		private void PayOut(Chest c)
		{
			if (c.Item > 0) Ff4Party.Party.AddItem(c.Item, 1);
			if (c.Gil > 0) Ff4Party.Party.Gil = (int)Math.Min(9999999L, (long)Ff4Party.Party.Gil + c.Gil);
			GlobalScope.FlagManager.singleton().set((uint)c.FlagGroup, (uint)c.FlagIndex);
			Log.Write(LogChannel.File, "chest: " + (c.Item > 0 ? "item " + c.Item : c.Gil + " gil") + " taken, flag " + c.FlagGroup + ":" + c.FlagIndex + " set");
		}

		private void Finish()
		{
			_opening = null;
			_fighting = false;
			try { Game.Hero.Unfreeze(); } catch (Exception) { }
		}

		private static void Effect(int id, Vector3 at)
		{
			try
			{
				GlobalScope.eff.CEffectMng.instance().loadEfpNamed("e" + id.ToString("000"), "/EFFECT/e" + id.ToString("000") + ".efp");
				GlobalScope.eff.CEffectMng effects = GlobalScope.eff.CEffectMng.instance();
				int made = effects.create(id, 1);
				if (made >= 0) effects.setPosition(made, EngineApi.ToFx(at));
			}
			catch (Exception) { }
		}

		private static void Sound(int bank, int number)
		{
			try { GlobalScope.MatrixSound.MtxSENDS_Play(bank, number, 127, 128); } catch (Exception) { }
		}
	}
}
