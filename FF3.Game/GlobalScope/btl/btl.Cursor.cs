using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using android.content;
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class btl
	{
		public class Cursor
		{
			public enum CURSOR_TYPE
			{
				COMMAND,
				TARGET,
				TARGET_0,
				TARGET_1,
				TARGET_2,
				TARGET_3,
				TARGET_4,
				TARGET_5,
				TARGET_6,
				TARGET_7,
				TARGET_8,
				TARGET_9,
				TARGET_10,
				TARGET_11,
				TARGET_12,
				TARGET_W,
				TARGET_W0,
				TARGET_W1,
				TARGET_W2,
				TARGET_W3,
				TARGET_W4,
				TARGET_W5,
				TARGET_W6,
				TARGET_W7,
				TARGET_W8,
				TARGET_W9,
				TARGET_W10,
				TARGET_W11,
				TARGET_W12,
				CURSOR_TYPE_MAX
			}

			public enum CURSOR_STATE
			{
				ACTIVE,
				PASSIVE,
				CURSOR_STATE_MAX
			}

			public const CURSOR_TYPE COMMAND = CURSOR_TYPE.COMMAND;

			public const CURSOR_TYPE TARGET = CURSOR_TYPE.TARGET;

			public const CURSOR_TYPE TARGET_0 = CURSOR_TYPE.TARGET_0;

			public const CURSOR_TYPE TARGET_1 = CURSOR_TYPE.TARGET_1;

			public const CURSOR_TYPE TARGET_2 = CURSOR_TYPE.TARGET_2;

			public const CURSOR_TYPE TARGET_3 = CURSOR_TYPE.TARGET_3;

			public const CURSOR_TYPE TARGET_4 = CURSOR_TYPE.TARGET_4;

			public const CURSOR_TYPE TARGET_5 = CURSOR_TYPE.TARGET_5;

			public const CURSOR_TYPE TARGET_6 = CURSOR_TYPE.TARGET_6;

			public const CURSOR_TYPE TARGET_7 = CURSOR_TYPE.TARGET_7;

			public const CURSOR_TYPE TARGET_8 = CURSOR_TYPE.TARGET_8;

			public const CURSOR_TYPE TARGET_9 = CURSOR_TYPE.TARGET_9;

			public const CURSOR_TYPE TARGET_10 = CURSOR_TYPE.TARGET_10;

			public const CURSOR_TYPE TARGET_11 = CURSOR_TYPE.TARGET_11;

			public const CURSOR_TYPE TARGET_12 = CURSOR_TYPE.TARGET_12;

			public const CURSOR_TYPE TARGET_W = CURSOR_TYPE.TARGET_W;

			public const CURSOR_TYPE TARGET_W0 = CURSOR_TYPE.TARGET_W0;

			public const CURSOR_TYPE TARGET_W1 = CURSOR_TYPE.TARGET_W1;

			public const CURSOR_TYPE TARGET_W2 = CURSOR_TYPE.TARGET_W2;

			public const CURSOR_TYPE TARGET_W3 = CURSOR_TYPE.TARGET_W3;

			public const CURSOR_TYPE TARGET_W4 = CURSOR_TYPE.TARGET_W4;

			public const CURSOR_TYPE TARGET_W5 = CURSOR_TYPE.TARGET_W5;

			public const CURSOR_TYPE TARGET_W6 = CURSOR_TYPE.TARGET_W6;

			public const CURSOR_TYPE TARGET_W7 = CURSOR_TYPE.TARGET_W7;

			public const CURSOR_TYPE TARGET_W8 = CURSOR_TYPE.TARGET_W8;

			public const CURSOR_TYPE TARGET_W9 = CURSOR_TYPE.TARGET_W9;

			public const CURSOR_TYPE TARGET_W10 = CURSOR_TYPE.TARGET_W10;

			public const CURSOR_TYPE TARGET_W11 = CURSOR_TYPE.TARGET_W11;

			public const CURSOR_TYPE TARGET_W12 = CURSOR_TYPE.TARGET_W12;

			public const CURSOR_TYPE CURSOR_TYPE_MAX = CURSOR_TYPE.CURSOR_TYPE_MAX;

			public static ushort MOVE_CURSOR = 0;

			public static ushort STOP_CURSOR = 3;

			private sys2d.Sprite3d[] cursor_ = new sys2d.Sprite3d[29];

			public void setup()
			{
				changeGlobalDirectory();
				cursor_[0].Load2(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, "icon_yubi");
				for (int i = 1; i < 29; i++)
				{
					cursor_[i].copy(cursor_[0]);
				}
				for (int i = 0; i < 29; i++)
				{
					cursor_[i].SetShow(show: false);
					sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(cursor_[i]);
				}
			}

			public void cleanup()
			{
				for (int i = 0; i < 29; i++)
				{
					sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(cursor_[i]);
					cursor_[i].Release();
				}
			}

			public void setPosition3Dto2D(int type, VecFx32 pos)
			{
				NNS_G3dWorldPosToScrPos(pos, out var px, out var py);
				px = MATH_MAX(px, 240 - LCD_WIDTH / 2 + 16);
				px = MATH_MIN(px, 240 + LCD_WIDTH / 2);
				py = MATH_MAX(py, 160 - LCD_HEIGHT / 2 + 8);
				py = MATH_MIN(py, 160 + LCD_HEIGHT / 2 - 8);
				setPosition(type, px, py);
			}

			public void setPositionCommand(int type, int line)
			{
				setPosition(type, CommandCursorPosition(line));
			}

			public void setPositionTargetMonster(int type, BattleMonster monster)
			{
				sys2d.Window window = monster.targetWindow();
				NNSG2dFVec2 pos = new NNSG2dFVec2(4096 * (window.GetPositionUL().vx + 8), 4096 * window.GetPositionCC().vy);
				setPosition(type, pos);
			}

			public void setPositionTargetPlayer(int type, BaseBattleCharacter player)
			{
				BattlePlayer battlePlayer = static_cast<BattlePlayer>(player);
				sys2d.Window window = battlePlayer.targetWindow();
				NNSG2dFVec2 pos = new NNSG2dFVec2(4096 * (window.GetPositionUL().vx + 8), 4096 * window.GetPositionCC().vy);
				setPosition(type, pos);
			}

			public void setPositionTargetAll(int type)
			{
				setPosition(type, 328, 16);
			}

			public void setPositionTargetGroup(int type)
			{
				setPosition(type, 248, 16);
			}

			public void setPositionMonster(int type, BattleMonster monster)
			{
				VecFx32 vecFx = new VecFx32();
				VecFx32 vecFx2 = new VecFx32();
				characterMng.getPosition(monster.characterMngId(), vecFx2);
				vecFx.x = 4096 * mon.MonsterManager.instance().offset(monster.monsterId()).cursorPosition()
					.x;
				vecFx.y = 4096 * mon.MonsterManager.instance().offset(monster.monsterId()).cursorPosition()
					.y;
				vecFx.z = 4096 * mon.MonsterManager.instance().offset(monster.monsterId()).cursorPosition()
					.z;
				VEC_Add(vecFx2, vecFx, vecFx2);
				setPosition3Dto2D(type, vecFx2);
			}

			public void setPositionPlayer(int type, BaseBattleCharacter player)
			{
				VecFx32 vecFx = new VecFx32();
				characterMng.getPosition(player.characterMngId(), vecFx);
				VEC_Add(vecFx, CursorOffset, vecFx);
				setPosition3Dto2D(type, vecFx);
			}

			public void setPositionPlayerAll(BattleParty party, int recover)
			{
				for (int i = 0; i < 4; i++)
				{
					if (recover == 0)
					{
						if (!party.battlePlayer(i).isBattle())
						{
							continue;
						}
					}
					else if (!party.battlePlayer(i).isEnable())
					{
						continue;
					}
					if (!party.battlePlayer(i).flag(PLAYER_FLAG.PF_JUMP))
					{
						VecFx32 vecFx = new VecFx32();
						characterMng.getPosition(party.battlePlayer(i).characterMngId(), vecFx);
						VEC_Add(vecFx, CursorOffset, vecFx);
						int type = 2 + party.battlePlayer(i).battleCharacterId();
						setPosition3Dto2D(type, vecFx);
						plural(type);
						type = 16 + party.battlePlayer(i).battleCharacterId();
						setPositionTargetPlayer(type, party.battlePlayer(i));
						plural(type);
					}
				}
			}

			public void setPositionMonsterAll(BattleMonsterParty monsterParty)
			{
				for (int i = 0; i < 6; i++)
				{
					if (monsterParty.battleMonster(i).isBattle())
					{
						int type = 2 + monsterParty.battleMonster(i).battleCharacterId();
						setPositionMonster(type, monsterParty.battleMonster(i));
						plural(type);
						type = 16 + monsterParty.battleMonster(i).battleCharacterId();
						setPositionTargetMonster(type, monsterParty.battleMonster(i));
						plural(type);
					}
				}
			}

			public void setPositionMonsterGroup(BattleMonsterParty monsterParty, int monsterId)
			{
				for (int i = 0; i < 6; i++)
				{
					if (monsterParty.battleMonster(i).isBattle() && monsterParty.battleMonster(i).monsterId() == monsterId)
					{
						int type = 2 + monsterParty.battleMonster(i).battleCharacterId();
						setPositionMonster(type, monsterParty.battleMonster(i));
						plural(type);
						type = 16 + monsterParty.battleMonster(i).battleCharacterId();
						setPositionTargetMonster(type, monsterParty.battleMonster(i));
						plural(type);
					}
				}
			}

			public void nondisplayAll()
			{
				for (int i = 0; i < 29; i++)
				{
					hidden(i);
				}
			}

			public void setAnimation(int type, bool flag)
			{
				cursor_[type].SetAnimation(flag);
			}

			public void active(int type)
			{
				cursor_[type].SetCell(MOVE_CURSOR);
				cursor_[type].SetAlpha(31);
				setAnimation(type, flag: true);
				setShow(type, show: true);
			}

			public void passive(int type)
			{
				cursor_[type].SetCell(STOP_CURSOR);
				cursor_[type].SetAlpha(31);
				setAnimation(type, flag: false);
				setShow(type, show: true);
			}

			public void plural(int type)
			{
				cursor_[type].SetCell(MOVE_CURSOR);
				cursor_[type].SetAlpha(16);
				setAnimation(type, flag: false);
				setShow(type, show: true);
			}

			public void hidden(int type)
			{
				cursor_[type].SetCell(MOVE_CURSOR);
				cursor_[type].SetAlpha(31);
				setAnimation(type, flag: false);
				setShow(type, show: false);
			}

			public Cursor()
			{
				for (int i = 0; i < cursor_.Length; i++)
				{
					cursor_[i] = new sys2d.Sprite3d();
				}
			}

			public sys2d.Sprite3d cell(int type)
			{
				return cursor_[type];
			}

			public void setPosition(int type, int x, int y)
			{
				cursor_[type].SetPositionI(x, y);
			}

			public void setPosition(int type, NNSG2dFVec2 pos)
			{
				cursor_[type].SetPosition(pos);
			}

			public NNSG2dFVec2 position(int type)
			{
				return cursor_[type].GetPosition();
			}

			public void setShow(int type, bool show)
			{
				cursor_[type].SetShow(show);
			}

			public bool isShow(int type)
			{
				return cursor_[type].IsShow();
			}
		}
	}
}
