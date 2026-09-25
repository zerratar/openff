// Auto battle: the heroes choose for themselves, by their rules (Gambits).
//
// F on the keyboard, or the pad's "auto" button (R3 unless settings.json says otherwise), turns it on
// and off. While it is on, a hero whose command turn begins gets no window: their rules are read
// against the battle as it stands, and the command goes onto the hero as the game's own windows put
// it (BattleSetupPlayer's paths for Attack, Guard, a spell, an item, Run Away) - the target, the
// target type, and the spell's charge or the item taken off at once, as selecting it does, so the
// next hero sees what is left. A hero already choosing when it is turned on finishes by hand.
//
// Steam's build shows the key and "Auto battle" above the party's lines during the command turn;
// the hint here is its key cap (icon_keyboard_64) with the key's name on it, the words gold while
// auto battle is on.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace OpenFF.Client
{
	// MonoGame's types by name: the engine's OpenFF.Game, GameTime, Color and vectors sit a namespace up.
	using Game = Microsoft.Xna.Framework.Game;
	using GameTime = Microsoft.Xna.Framework.GameTime;
	using Color = Microsoft.Xna.Framework.Color;
	using btl = GlobalScope.btl;

	internal static class AutoBattle
	{
		/// <summary>Whether auto battle is on (for the session: a new run starts with it off).</summary>
		public static bool On { get; private set; }

		/// <summary>Whether the command turn is up, so the hint shows (BattleSetupPlayer sets it as it runs and clears it as it ends).</summary>
		public static bool CommandTurn;

		/// <summary>Whether the battle is ending - won or lost (BattleWin / BattleLose): no hint over it. A round's start clears it.</summary>
		public static bool Ending;

		private static bool _keyWas, _padWas;

		/// <summary>The toggle, once a step (GameHost): F, or the pad's auto button, pressed in a battle.</summary>
		public static void Update()
		{
			bool key = false, pad = false;
			try
			{
				key = DesktopInput.Injected.Contains(Keys.F) || (DesktopInput.GameHasKeyboard && Keyboard.GetState().IsKeyDown(Keys.F));
				pad = DesktopInput.GameHasKeyboard && DesktopInput.PadAutoHeld();
			}
			catch (Exception) { }
			bool pressed = (key && !_keyWas) || (pad && !_padWas);
			_keyWas = key;
			_padWas = pad;
			if (!pressed || !InBattle) return;
			On = !On;
			Log.Write(LogChannel.General, "auto battle: " + (On ? "on" : "off"));
			try { GlobalScope.menu.MenuManager.getSingleton().playSEMoveCursor(); } catch (Exception) { }
		}

		private static bool InBattle
		{
			get
			{
				try { return (GlobalScope.GAMEPART)GlobalScope.sys.FF3PartSys.getCurrentPart() == GlobalScope.GAMEPART.GAMEPART_BATTLE; }
				catch (Exception) { return false; }
			}
		}

		/// <summary>
		/// Whether the battle's camera stays on the fight between rounds: auto battle is on and the camera is
		/// there already (the fight's view, MAIN_CAMERA), so the round begins without the cut to the command
		/// view. Turned off, the next round goes to the command view as ever.
		/// </summary>
		public static bool HoldsCamera
		{
			get
			{
				if (!On) return false;
				try { return btl.OutsideToBattle.getInstance().battleCamera() == btl.BATTLE_CAMERA.MAIN_CAMERA; }
				catch (Exception) { return false; }
			}
		}

		/// <summary>Whether the hint shows: in a battle's rounds (the command view or the fight's), not its opening or its ending.</summary>
		public static bool HintShown
		{
			get
			{
				if (!(CommandTurn || On) || Ending || !InBattle) return false;
				try
				{
					btl.BATTLE_CAMERA camera = btl.OutsideToBattle.getInstance().battleCamera();
					return camera == btl.BATTLE_CAMERA.COMMAND_CAMERA || camera == btl.BATTLE_CAMERA.MAIN_CAMERA
						|| camera == btl.BATTLE_CAMERA.MOVE_CAMERA || camera == btl.BATTLE_CAMERA.RETURN_CAMERA;
				}
				catch (Exception) { return false; }
			}
		}

		/// <summary>Whether auto battle takes this hero's turn: it is on, and the turn is only beginning (no window open yet).</summary>
		public static bool Takes(btl.BattlePlayer player, bool turnBeginning)
		{
			return On && turnBeginning && player != null && !BattleSync.IsRemote(player.playerId());
		}

		// ---- the choice ----

		/// <summary>The hero's command by their rules, committed. Their first rule that finds a usable target wins; none, the first foe is attacked.</summary>
		public static void Decide(btl.BattleSetupPlayer setup, btl.BattlePlayer player, btl.BattleSystem system)
		{
			try
			{
				Choose(setup, player, system);
			}
			catch (Exception ex)
			{
				// A rule that throws must not take the battle with it: the hero guards this round.
				Log.Write(LogChannel.General, "auto battle: the rules failed (" + ex.Message + "); Guard");
				player.setActionId(3);
				player.clearTargetId();
			}
		}

		private static void Choose(btl.BattleSetupPlayer setup, btl.BattlePlayer player, btl.BattleSystem system)
		{
			List<GambitTarget> foes = Foes(system), allies = Allies(system);
			GambitTarget self = allies.FirstOrDefault(a => a.Character == player);
			int hero = player.playerId();
			List<Gambit> rules = Gambits.For(hero);
			for (int i = 0; i < rules.Count; i++)
			{
				Gambit rule = rules[i];
				if (!rule.On || !Gambits.Conditions.TryGetValue(rule.Condition, out GambitCondition condition)) continue;
				IEnumerable<GambitTarget> side = condition.Side == GambitSide.Foe ? foes : condition.Side == GambitSide.Ally ? allies : (self != null ? new[] { self } : Enumerable.Empty<GambitTarget>());
				foreach (GambitTarget target in condition.Find(side, rule.ConditionParam))
				{
					if (Commit(setup, player, system, rule, target))
					{
						Log.Write(LogChannel.File, "auto battle: " + player.player().name() + " - rule " + (i + 1) + " " + rule + " on " + Describe(target));
						return;
					}
				}
			}
			// Nothing held: the first foe (the game's own order), or Guard with none left.
			GambitTarget first = foes.Where(f => f.Alive).OrderBy(f => f.Order).FirstOrDefault();
			Gambit fallback = new Gambit { Condition = "foe.any", Action = first != null ? "attack" : "guard" };
			Commit(setup, player, system, fallback, first);
			Log.Write(LogChannel.File, "auto battle: " + player.player().name() + " - no rule held; " + fallback.Action);
		}

		private static string Describe(GambitTarget t)
		{
			if (t == null) return "-";
			try
			{
				if (t.Side == GambitSide.Foe && t.Character is btl.BattleMonster m) return BattleCommands.BattleName((uint)m.monster().nameId()) ?? "a foe";
				if (t.Character is btl.BattlePlayer p) return p.player().name();
			}
			catch (Exception) { }
			return t.Side.ToString();
		}

		/// <summary>The foes, the game's front row first (its target windows' order, 4 3 5 1 0 2).</summary>
		private static List<GambitTarget> Foes(btl.BattleSystem system)
		{
			int[] order = { 4, 3, 5, 1, 0, 2 };
			List<GambitTarget> list = new List<GambitTarget>();
			btl.BattleMonsterParty party = system.characterManager().monsterParty();
			for (int i = 0; i < order.Length; i++)
			{
				btl.BattleMonster m = party.battleMonster(order[i]);
				if (m == null || !m.isBattle()) continue;
				list.Add(Target(m, GambitSide.Foe, i));
			}
			return list;
		}

		/// <summary>The party in battle, KO'd and stone included (a rule can be about them).</summary>
		private static List<GambitTarget> Allies(btl.BattleSystem system)
		{
			List<GambitTarget> list = new List<GambitTarget>();
			btl.BattleParty party = system.characterManager().playerParty();
			for (int i = 0; i < 4; i++)
			{
				btl.BattlePlayer p = party.battlePlayer(i);
				if (p == null || p.player() == null || !p.player().isEnable()) continue;
				list.Add(Target(p, GambitSide.Ally, i));
			}
			return list;
		}

		private static GambitTarget Target(btl.BaseBattleCharacter c, GambitSide side, int order)
		{
			GlobalScope.ys.Condition k = c.condition();
			GambitStatus status = GambitStatus.None;
			if (k.isPoison()) status |= GambitStatus.Poison;
			if (k.isDarkness()) status |= GambitStatus.Blind;
			if (k.isSilence()) status |= GambitStatus.Silence;
			if (k.isLilliput()) status |= GambitStatus.Mini;
			if (k.isFrog()) status |= GambitStatus.Toad;
			if (k.isStone()) status |= GambitStatus.Stone;
			if (k.isDeath()) status |= GambitStatus.KO;
			if (k.isSleep()) status |= GambitStatus.Sleep;
			if (k.isParalysis()) status |= GambitStatus.Paralysis;
			if (k.isConfusion()) status |= GambitStatus.Confusion;
			return new GambitTarget
			{
				Character = c,
				Side = side,
				Hp = c.hp().getNow(),
				MaxHp = c.hp().getLimit(),
				Dead = k.isDeath(),
				Stone = k.isStone(),
				Alive = c.isBattle(),
				Status = (int)status,
				Order = order
			};
		}

		// ---- committing, as the game's windows do ----

		/// <summary>The rule's action on the target, if it can be used on it: onto the hero, true. Otherwise nothing is changed, false.</summary>
		private static bool Commit(btl.BattleSetupPlayer setup, btl.BattlePlayer player, btl.BattleSystem system, Gambit rule, GambitTarget target)
		{
			switch (rule.Action)
			{
				case "attack":
					if (target == null || !target.Alive || target.Character == player) return false;
					player.setActionId(1);
					Aim(player, target);
					return true;
				case "guard":
					player.setActionId(3);
					player.clearTargetId();
					return true;
				case "run":
					player.setActionId(2);
					player.clearTargetId();
					return true;
				case "magic":
					return CommitMagic(setup, player, system, rule.ActionParam, target);
				case "item":
					return CommitItem(setup, player, system, rule.ActionParam, target);
			}
			return false;
		}

		/// <summary>One target, and the side it is on (the game retargets a fallen one by it).</summary>
		private static void Aim(btl.BattlePlayer player, GambitTarget target)
		{
			player.clearTargetType();
			player.clearTargetId();
			player.setTargetId(0, target.Character.battleCharacterId());
			player.setFlag(target.Side == GambitSide.Foe ? btl.PLAYER_FLAG.PF_TARGET_MONSTER : btl.PLAYER_FLAG.PF_TARGET_PLAYER);
			player.setLastTargetId();
		}

		private static bool HasSpell(btl.BattlePlayer player, int id)
		{
			for (int level = 0; level < 8; level++)
			{
				GlobalScope.pl.EquipmentMagic slots = player.player().equipParameter().equipMagic((GlobalScope.pl.MAGIC_LEVEL)level);
				if (slots == null) continue;
				for (int i = 0; i < GlobalScope.pl.MAGIC_ONCE_LEVEL_EQUIP_MAX; i++) if (slots.magicId(i) == id) return true;
			}
			return false;
		}

		private static bool CommitMagic(btl.BattleSetupPlayer setup, btl.BattlePlayer player, btl.BattleSystem system, int id, GambitTarget target)
		{
			GlobalScope.itm.MagicParameter spell = GlobalScope.itm.ItemManager.instance().magicParameter((short)id);
			if (spell == null || !HasSpell(player, id) || !setup.isUseMagic(id, player)) return false;
			short oldMagic = player.useMagicId();
			player.setUseMagicId((short)id);
			if (!Fits(player, spell.targetPosition(), target))
			{
				player.setUseMagicId(oldMagic);
				return false;
			}
			player.setActionId(spell.system() == 3 ? 20 : spell.system() == 2 ? 6 : 5);
			if (spell.system() != 3)
			{
				// Selecting a spell takes its charge at once, so the next hero sees what is left; the round's
				// end gives it back and the cast takes it for good (BattleSetupPlayer.releaseMagicWindow).
				player.player().mp(spell.magicClass()).subNow(1);
				player.player().setJobChangeMp(spell.magicClass(), (byte)player.player().mp(spell.magicClass()).getNow());
			}
			if (spell.system() == 2)
			{
				player.clearTargetId();   // a summon picks its own targets as it is cast
				return true;
			}
			Targets(player, system, spell.targetPosition(), spell.targetPossible(), target, id == 4008);
			return true;
		}

		private static bool CommitItem(btl.BattleSetupPlayer setup, btl.BattlePlayer player, btl.BattleSystem system, int id, GambitTarget target)
		{
			GlobalScope.itm.ItemBaseParameter item = GlobalScope.itm.ItemManager.instance().itemParameter((short)id);
			GlobalScope.itm.PossessionItem held = GlobalScope.pl.PlayerParty.instance().item().serchNormalItem((short)id);
			if (item == null || held == null || held.itemNumber() <= 0 || !setup.isUseItem(id, player)) return false;
			int oldItem = player.useItemId();
			player.setUseItemId(id);
			if (!Fits(player, item.targetPosition(), target))
			{
				player.setUseItemId(oldItem);
				return false;
			}
			player.setActionId(7);
			if (GlobalScope.itm.ItemManager.instance().consumptionParameter((short)id) != null)
			{
				held.setItemNumber(held.itemNumber() - 1);   // taken at once, as selecting it does (releaseItemWindow)
			}
			Targets(player, system, item.targetPosition(), item.targetPossible(), target, false);
			return true;
		}

		/// <summary>
		/// Whether a spell's or item's side (its targetPosition: 0/1 foes, 2 the user, 3/4 allies) is the
		/// target's, and the target can take it: alive, or KO'd / stone for a use that raises or cures them.
		/// </summary>
		private static bool Fits(btl.BattlePlayer player, short position, GambitTarget target)
		{
			if (target == null) return false;
			switch (position)
			{
				case 0:
				case 1:
					return target.Side == GambitSide.Foe && target.Alive;
				case 2:
					return target.Character == player && target.Alive;
				case 3:
				case 4:
					if (target.Side == GambitSide.Foe) return false;
					if (target.Alive) return true;
					return (target.Dead && player.isSelectDeadTarget()) || (target.Stone && player.isSelectStoneTarget());
			}
			return false;
		}

		/// <summary>
		/// The targets a spell or item takes, as its target window would have left them: one where it
		/// can take one (the target), else the group or everyone on that side (targetPossible bits: foes
		/// 2 one / 4 group / 8 all, allies 0x80 / 0x100 / 0x200).
		/// </summary>
		private static void Targets(btl.BattlePlayer player, btl.BattleSystem system, short position, short possible, GambitTarget target, bool everyone)
		{
			btl.BattleCharacterManager manager = system.characterManager();
			player.clearTargetType();
			player.clearTargetId();
			if (everyone)
			{
				manager.setPlayerAllTarget(player, 0);
				player.setFlag(btl.PLAYER_FLAG.PF_TARGET_PLAYER);
			}
			else if (position == 2)
			{
				player.setTargetIdMyself();
				player.setFlag(btl.PLAYER_FLAG.PF_TARGET_PLAYER);
			}
			else if (position == 0 || position == 1)
			{
				if ((possible & 2) != 0) player.setTargetId(0, target.Character.battleCharacterId());
				else if ((possible & 8) != 0) manager.setMonsterAllTarget(player);
				else { player.setTargetId(0, target.Character.battleCharacterId()); manager.setMonsterGroupTarget(player); }
				player.setFlag(btl.PLAYER_FLAG.PF_TARGET_MONSTER);
			}
			else
			{
				if ((possible & 0x80) != 0) player.setTargetId(0, target.Character.battleCharacterId());
				else manager.setPlayerAllTarget(player, player.isSelectDeadOrStoneTargetCommand() ? 1 : 0);
				player.setFlag(btl.PLAYER_FLAG.PF_TARGET_PLAYER);
			}
			player.setLastTargetId();
		}
	}

	/// <summary>
	/// The hint above the party's lines while the heroes choose, as Steam's: the key (F, or the pad's
	/// button) on Steam's own key cap, and "Auto battle" - gold while it is on.
	/// </summary>
	internal sealed class AutoBattleHint : DrawableGameComponent
	{
		private const float TextSpaceWidth = 800f;
		private const float TextSpaceHeight = 480f;
		// Where Steam's hint stands, in the game's 480 x 320 units: the words end over the party lines' right end.
		private const float RightX = 440f, MiddleY = 243f, CapSize = 16f, Gap = 3f;
		private const int TextSize = 10;

		private SpriteBatch _batch;
		private Texture2D _keys;
		private bool _tried;

		private AutoBattleHint(Game game) : base(game)
		{
			DrawOrder = int.MaxValue - 6;
		}

		public static void Attach(Game game) => game.Components.Add(new AutoBattleHint(game));

		public override void Draw(GameTime gameTime)
		{
			if (RenderTest.Active || GameProfile.IsFf4 || !AutoBattle.HintShown) return;
			if (!(GlobalScope.m_Graphics is GlobalScope.Graphics graphics)) return;
			if (!TrueTypeText.Enabled) return;
			EnsureKeys();

			string words = "Auto battle";
			string key = DesktopInput.PadConnected ? "R3" : "F";
			float wordsWidth = TrueTypeText.Width(words, TextSize) / TextSpaceWidth * 480f;
			float wordsX = RightX - wordsWidth;
			float capX = wordsX - Gap - CapSize;
			float capY = MiddleY - CapSize / 2f;

			Viewport view = GraphicsDevice.Viewport;
			float sx = view.Width / 480f, sy = view.Height / 320f;
			if (_keys != null)
			{
				_batch ??= new SpriteBatch(GraphicsDevice);
				_batch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearClamp);
				// The blank cap: icon_keyboard_64's first 64 x 64.
				_batch.Draw(_keys, new Rectangle((int)(capX * sx), (int)(capY * sy), (int)(CapSize * sx), (int)(CapSize * sy)), new Rectangle(0, 0, 64, 64), Color.White);
				_batch.End();
			}

			graphics.SetImageOrigin(0f, 0f);
			graphics.SetImageRotation(0f);
			graphics.SetImageScale(1f, 1f);
			graphics.DrawStringStart();
			// The key's name on the cap, in the title's serif as Steam's key caps are lettered.
			TrueTypeText.TitleFace = true;
			try
			{
				float kw = TrueTypeText.Width(key, TextSize) / TextSpaceWidth * 480f;
				graphics.SetColor(255, 255, 255, 255);
				graphics.DrawString(key, (capX + (CapSize - kw) / 2f) / 480f * TextSpaceWidth, (capY + 2f) / 320f * TextSpaceHeight, TextSize);
			}
			finally
			{
				TrueTypeText.TitleFace = false;
			}
			if (AutoBattle.On) graphics.SetColor(255, 214, 90, 255);
			else graphics.SetColor(255, 255, 255, 255);
			graphics.DrawString(words, wordsX / 480f * TextSpaceWidth, (MiddleY - 6f) / 320f * TextSpaceHeight, TextSize);
			graphics.DrawStringEnd();
		}

		/// <summary>Steam's key caps (icon_keyboard_64's sheet), when the content has them; the hint goes without a cap otherwise.</summary>
		private void EnsureKeys()
		{
			if (_tried) return;
			_tried = true;
			try
			{
				byte[] png = AppShell.loadFileEntry("files/icon_keyboard_64.NCGR");
				if (png == null || png.Length < 8 || png[0] != 0x89) return;
				using MemoryStream stream = new MemoryStream(png);
				_keys = Texture2D.FromStream(GraphicsDevice, stream);
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "auto battle: no key caps (" + ex.Message + ")");
			}
		}
	}
}
