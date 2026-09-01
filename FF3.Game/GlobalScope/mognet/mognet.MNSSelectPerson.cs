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
	public static partial class mognet
	{
		public class MNSSelectPerson : MogNetState
		{
			public const int NPC_0 = 0;

			public const int NPC_1 = 1;

			public const int NPC_2 = 2;

			public const int NPC_3 = 3;

			public const int NPC_4 = 4;

			public const int NPC_5 = 5;

			public const int CANCEL = 6;

			private int select_;

			private short nb_person;

			private short[] person_index_ = new short[6];

			private short[] person_str_pos_x_ = new short[6];

			private bool restriction_;

			private bool npc_restriction_;

			private int localState_;

			public MNSSelectPerson()
				: base("MBMogNetSelectPerson")
			{
			}

			public override menu.MenuBehavior mbfCreate()
			{
				return this;
			}

			public override void mnsInitialize(MNSMediator M)
			{
				MNEvent mNEvent = new MNEvent();
				mNEvent.mneProgress();
				M.changeMainBGScr(MAIN_BG_SCR.MBS_DISPLAY);
				M.changeSubBGScr(SUB_BG_SCR.SBS_SELECT_PERSON);
				M.mnsmCommonInterface(b: true, bLR: false);
				menu.MenuManager.getSingleton().buildMenu("select_person");
				menu.MenuManager.getSingleton().initFocus(0);
				for (int i = 0; i < 4; i++)
				{
					menu.Medget nodeByID = menu.MenuManager.getSingleton().root().getNodeByID(id_help[i]);
					if (nodeByID != null && nodeByID.behavior() != null)
					{
						((menu.MBText)nodeByID.behavior().queryInterface(menu.MBText.classIdentifier()))?.bmTextVisibility(v: false);
					}
				}
				setHelpMsg(0);
				restriction_ = MNMemento.getSingleton().mnmCheckDateTimeForEvent();
				npc_restriction_ = MNMemento.getSingleton().mnmCheckDateTimeForNPC();
				nb_person = 0;
				for (int j = 0; j < 6; j++)
				{
					menu.Medget nodeByID2 = menu.MenuManager.getSingleton().root().getNodeByID(id_name[j]);
					if (nodeByID2 == null || nodeByID2.behavior() == null)
					{
						continue;
					}
					menu.MBText mBText = (menu.MBText)nodeByID2.behavior().queryInterface(menu.MBText.classIdentifier());
					if (mBText == null)
					{
						continue;
					}
					if (j >= 0 && !MNSLetterBrowse.checkMail(j, bNew: false))
					{
						mBText.bmTextVisibility(v: false);
						nodeByID2.setPosX(-200);
						nodeByID2.setPosY(-200);
						nodeByID2.setTag(10);
						continue;
					}
					int num = 110;
					if (mBText.getMessage() != null)
					{
						mBText.getMessage().position(out var x, out var _);
						num = x;
					}
					person_index_[nb_person] = (short)j;
					person_str_pos_x_[nb_person] = (short)num;
					nodeByID2.setTag(nb_person);
					if (MNSLetterBrowse.checkMail(j, bNew: true))
					{
						sys2d.Cell cell = M.mnsmNewMarkIcon(j);
						cell.SetShow(show: true);
						cell.SetPositionI(nodeByID2.x() + nodeByID2.width() + 16, nodeByID2.y() + 12);
					}
					nb_person++;
				}
				for (int k = 0; k < nb_person; k++)
				{
					menu.Medget nodeByID3 = menu.MenuManager.getSingleton().root().getNodeByID(id_name[person_index_[k]]);
					if (nodeByID3 != null && nodeByID3.behavior() != null)
					{
						if (k == 0)
						{
							nodeByID3.setUp(id_name[person_index_[nb_person - 1]]);
							nodeByID3.setDown(id_name[person_index_[k + 1]]);
						}
						else if (k == nb_person - 1)
						{
							nodeByID3.setUp(id_name[person_index_[k - 1]]);
							nodeByID3.setDown(id_name[person_index_[0]]);
						}
						else
						{
							nodeByID3.setUp(id_name[person_index_[k - 1]]);
							nodeByID3.setDown(id_name[person_index_[k + 1]]);
						}
					}
				}
				menu.MenuManager.getSingleton().initFocus(select_);
				select_ = -1;
				localState_ = 2;
			}

			public override bool mnsProcess(MNSMediator M)
			{
				switch (localState_)
				{
				case 0:
					if (0 <= select_ && 5 >= select_)
					{
						M.shiftState(M.MNSLetterBrowse_);
					}
					break;
				case 1:
					if (dgs.CFade.Main().isFaded() && dgs.CFade.Sub().isFaded())
					{
						return false;
					}
					break;
				case 2:
					if (dgs.CFade.Main().isCleared() && dgs.CFade.Main().isCleared())
					{
						localState_ = 0;
					}
					break;
				}
				return true;
			}

			public override void mnsTerminate(MNSMediator M)
			{
				for (int i = 0; i < 6; i++)
				{
					M.mnsmNewMarkIcon(i).SetShow(show: false);
				}
				M.mnsmDrawHelp(-1);
				menu.MenuManager.getSingleton().release();
			}

			public override bool mnsCancel(MNSMediator M)
			{
				if (localState_ == 1 || localState_ == 2)
				{
					return true;
				}
				M.mnsmCommonInterface(b: false, bLR: false);
				localState_++;
				dgs.CFade.Main().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
				dgs.CFade.Sub().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
				menu.MenuManager.getSingleton().playSECancel();
				return true;
			}

			public int getSelectItem()
			{
				return select_;
			}

			public void setHelpMsg(int num)
			{
			}

			public override bool bmDecide(menu.Medget M)
			{
				if (localState_ == 1 || localState_ == 2)
				{
					return true;
				}
				select_ = person_index_[M.myTag()];
				OS_Printf("decide %d.\n", select_);
				menu.MenuManager.getSingleton().playSEDecide();
				return true;
			}

			public override bool bmCancel(menu.Medget M)
			{
				mnsCancel(ownerMediator);
				return true;
			}

			public override void bmActivate(menu.Medget M)
			{
				setHelpMsg(M.myTag());
			}

			public void setNextPage(int offset)
			{
				do
				{
					select_ = (select_ + offset + 6) % 6;
				}
				while (!MNSLetterBrowse.checkMail(select_, bNew: false));
			}

			public override void mbDelete()
			{
			}

			public override void bmInitialize(menu.Medget arg0)
			{
			}

			public override void bmBehave(menu.Medget arg0)
			{
			}

			public override void bmFinalize(menu.Medget arg0)
			{
			}
		}
	}
}
