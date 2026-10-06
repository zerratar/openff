// FF4's Jump as libff4.so runs it (BattleActionJumpStart / JumpEnd, BattlePlayerBehavior's jump states,
// PhysicalDamageCalculator::reviseJump): after the invoke the member leaps - motion 6401, on its fourth frame the sound
// (0x99, 1) and effect 262 at its feet, then up 20 a frame until the motion is over and it is gone (flag 0x15: in the
// air, no one's target, its gauge standing). The landing comes back as an action after the command's wait: the
// member over its target, motion 6403, down 25 a frame; on the second frame effect 263 on the target with (0x99, 2),
// the blow doubled, its number; then back to its place.

using System;
using System.Collections.Generic;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Battle
	{
		private const int CmdJump = 31;

		private void JumpStart(Fighter member, Fighter foe)
		{
			Acted(member, CmdJump, foe);
			Play(member, 0x1901, false, 0);
			LoadEffect(262);
			LoadEffect(263);
			After(4, () =>
			{
				Game.Audio.PlaySe(0x99, 1);
				PlayEffect(262, Where(member));
			});
			// From the fourth frame up 20 a frame until the motion is over (about 12 frames on), then gone.
			for (int k = 4; k <= 14; k++)
			{
				After(k, () =>
				{
					if (member.Npc == null) return;
					member.Npc.Teleport(member.Npc.Position + new Vector3(0f, 20f, 0f));
					Face(member, member.Facing);
				});
			}
			After(15, () =>
			{
				member.Airborne = true;
				if (member.Npc != null) member.Npc.Hidden = true;
				// The gauge fills again in the air; when it is full the landing is the member's action (cheakBehaviorRequest:
				// a full gauge with flag 0x15 asks for the landing, not the command window).
				member.Gauge = 0f;
				member.Queued = false;
				member.JumpTarget = foe;
				Note(member.Name + " is in the air");
			});
		}

		private void JumpLand(Fighter member, Fighter foe)
		{
			if (!foe.Alive) foe = FirstAlive(_foes);
			member.Airborne = false;
			if (foe == null || member.Npc == null)
			{
				if (member.Npc != null) { member.Npc.Teleport(member.Home); member.Npc.Hidden = false; }
				return;
			}
			Acted(member, CmdJump, foe);
			bool hit = Hits(member, foe);
			int damage = hit ? Damage(member, foe) * 2 : 0;   // reviseJump: x2 from the air
			Vector3 over = Where(foe) + new Vector3(0f, 50f, 0f);
			member.Npc.Teleport(over);
			member.Npc.Hidden = false;
			Play(member, 0x1903, false, 0);
			for (int k = 1; k <= 2; k++)
			{
				int step = k;
				After(k, () => member.Npc?.Teleport(over - new Vector3(0f, 25f * step, 0f)));
			}
			After(2, () =>
			{
				if (hit)
				{
					PlayEffect(263, HitEffectSpot(foe));
					Game.Audio.PlaySe(0x99, 2);
					foe.Hp = Math.Max(0, foe.Hp - damage);
					if (foe.Member != null) foe.Member.Hp = foe.Hp;
					BlowLands(member, foe);
					Pop(DamageSpot(foe), damage);
					Note(member.Name + " lands on " + foe.Name + " for " + damage + ".");
					if (!foe.Alive) Fell(foe, damage);
				}
				else
				{
					PopWord(DamageSpot(foe), Ff4Ui.WordMiss);
					Note(member.Name + " lands and misses " + foe.Name + ".");
				}
			});
			After(20, () =>
			{
				if (member.Npc == null) return;
				member.Npc.Teleport(member.Home);
				Face(member, member.Facing);
				Play(member, member.IdleMotion, true, 0);
				member.Acted = false;
			});
			member.Gauge = 0f;
		}
	}
}
