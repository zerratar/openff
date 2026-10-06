// FF4's Jump as libff4.so runs it (BattleActionJumpStart / JumpEnd, stateJumpStart / stateJumpEnd, executeJumpEnd,
// PhysicalDamageCalculator::reviseJump), frame by frame. After the invoke close-up the member leaps: motion 6401, on
// its fourth frame the sound (0x99, 1) and effect 262 at its feet, then up 20 on frames 4, 5 and 6; at the motion's end
// its weapon and shield go and it stands in its idle 60 up (the body is not hidden - the standing shot leaves it out).
// The tick after, flag 0x15: no one's target, the gauge standing, a counter adding the battle speed rate each tick
// until 0x78000 (120 ticks at the normal speed, agility nothing to do with it); then the landing joins the back of the
// queue. The landing: the target settled (a fallen one: the first monster standing), the blow the Attack formula's
// doubled with no back row; once no number is on the screen, 6403 from 21 short of the target and 50 up, down 25 and
// on 8 a frame to 5 short of it on the ground; on the second frame effect 263 at the target and (0x99, 2) - a miss as
// well - its number; frames 3 to 10 a hop back, a tenth of the way a frame; on 11 home, idle; on 12 out of the air,
// the gauge from empty.

using System;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Battle
	{
		private const int CmdJump = 31;
		private const int JumpAirTime = 0x78000;   // BattleActiveTimeMain::requestBehavior: the counter that lands it

		private void JumpStart(Fighter member, Fighter foe)
		{
			Acted(member, CmdJump);   // the take-off has no target and no result
			Play(member, 0x1901, false, 0);
			LoadEffect(262);
			LoadEffect(263);
			for (int k = 4; k <= 6; k++)
			{
				int frame = k;
				After(k, () =>
				{
					if (frame == 4)
					{
						Game.Audio.PlaySe(0x99, 1);
						PlayEffect(262, Where(member));   // at the feet, before the rise
					}
					member.Npc?.Teleport(member.Npc.Position + new Vector3(0f, 20f, 0f));
					if (frame < 6) return;
					// The motion's end: the weapon and shield away, back to the idle (setConditionMotion(0)).
					ShowEquipment(member, false);
					Play(member, member.IdleMotion, true, 0);
					member.Acted = false;
				});
			}
			After(7, () =>
			{
				member.Airborne = true;
				member.JumpCounter = 0;
				member.JumpTarget = foe;
				member.Queued = false;   // no resetATG: the gauge stays where it was, standing
				Note(member.Name + " is in the air");
			});
		}

		/// <summary>The air time, each tick the gauge could charge: true once the landing is due.</summary>
		private static bool JumpCounts(Fighter f)
		{
			if (f.JumpCounter < JumpAirTime) f.JumpCounter += SpeedRate;
			return f.JumpCounter >= JumpAirTime;
		}

		private void JumpLand(Fighter member, Fighter foe)
		{
			if (foe == null || !foe.Alive) foe = FirstAlive(_foes);   // retargeting: the first monster standing
			Acted(member, CmdJump, foe != null ? new[] { foe } : new Fighter[0]);
			member.JumpCounter = 0;
			bool hit = foe != null && Hits(member, foe);
			_critical = false;
			int damage = hit ? Damage(member, foe, fromAir: true) * 2 : 0;   // reviseJump: x2; no back row from the air
			bool critical = hit && _critical;
			Vector3 home = member.Home;
			Vector3 t = foe != null ? Where(foe) : Vector3.Zero;
			Vector3 dir = new Vector3(t.X - home.X, 0f, t.Z - home.Z);
			dir = dir.Length > 0.01f ? dir * (1f / dir.Length) : new Vector3(0f, 0f, 1f);
			Vector3 ground = new Vector3(t.X, home.Y, t.Z);
			member.Npc?.Teleport(ground - dir * 21f + new Vector3(0f, 50f, 0f));
			// stateJumpEnd 0x30: nothing until every number is gone.
			void Wait()
			{
				if (_pops.Count > 0) { After(1, Wait); return; }
				Descend();
			}
			void Descend()
			{
				if (member.Npc == null) { member.Airborne = false; return; }
				ShowEquipment(member, true);
				Face(member, member.Facing);   // its own battle facing, not the target's way
				Play(member, 0x1903, false, 0);
				member.Npc.Teleport(ground - dir * 13f + new Vector3(0f, 25f, 0f));
				After(1, () => member.Npc?.Teleport(ground - dir * 5f));
				float step = 0f;
				Vector3 back = Vector3.Zero, at = Vector3.Zero;
				After(2, () =>
				{
					at = member.Npc != null ? member.Npc.Position : ground - dir * 5f;
					back = new Vector3(home.X - at.X, 0f, home.Z - at.Z);
					step = back.Length / 10f;
					if (step > 0f) back = back * (1f / back.Length);
					// The blow: effect and sound whatever came of it.
					PlayEffect(263, foe != null ? HitEffectSpot(foe) : Where(member));
					Game.Audio.PlaySe(0x99, 2);
					if (foe == null) return;
					if (!hit)
					{
						PopWord(DamageSpot(foe), Ff4Ui.WordMiss);
						Note(member.Name + " lands and misses " + foe.Name + ".");
						return;
					}
					if (critical) WhiteFlash();
					foe.Hp = Math.Max(0, foe.Hp - damage);
					if (foe.Member != null) foe.Member.Hp = foe.Hp;
					BlowLands(member, foe);
					if (foe.Member != null && foe.Alive) Play(foe, _heroMotionHurt, false, 0);
					Pop(DamageSpot(foe), damage);
					Note(member.Name + " lands on " + foe.Name + " for " + damage + (critical ? " (critical)" : "") + ".");
					if (!foe.Alive) Fell(foe, damage);
				});
				// Frames 3 to 10: a tenth of the way home a frame, y up by 5 sin(phase), the phase on 0x1744 a frame.
				float y = 0f;
				for (int k = 3; k <= 10; k++)
				{
					int n = k - 3;
					After(k, () =>
					{
						y += 5f * (float)Math.Sin(n * 0x1744 * Math.PI * 2 / 65536.0);
						at += back * step;
						member.Npc?.Teleport(new Vector3(at.X, home.Y + y, at.Z));
					});
				}
				After(11, () =>
				{
					if (member.Npc == null) return;
					member.Npc.Teleport(home);
					Face(member, member.Facing);
					Play(member, member.IdleMotion, true, 8);
					member.Acted = false;
				});
				After(12, () =>
				{
					member.Airborne = false;
					member.JumpCounter = 0;
					member.Gauge = 0f;   // battleBehaved: the gauge from empty
				});
			}
			After(1, Wait);
		}

		/// <summary>PlayerEquipmentSymbol shown or not (setShowEquipment): the weapon and shield bound at the member's hands.</summary>
		private static void ShowEquipment(Fighter f, bool shown)
		{
			try { if (f.Npc is LegacyNpc held && held.CharacterId >= 0) Ff4Cutscene.ShowBound(held.CharacterId, shown); } catch (Exception) { }
		}

		/// <summary>createCriticalFlash: ScreenFlash::setFlash(1, 3, 0x7fff) - white, gone over 3 frames.</summary>
		private static void WhiteFlash()
		{
			GlobalScope.dgs.CFade.Main().fadeOut(0, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_WHITE);
			GlobalScope.dgs.CFade.Sub().fadeOut(0, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_WHITE);
			GlobalScope.dgs.CFade.Main().fadeIn(3);
			GlobalScope.dgs.CFade.Sub().fadeIn(3);
		}
	}
}
