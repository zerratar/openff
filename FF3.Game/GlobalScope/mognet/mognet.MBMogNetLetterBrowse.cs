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
		public class MBMogNetLetterBrowse : menu.MenuBehavior, SBEventHandler
		{
			public enum MNMB_STATE
			{
				MNLB_SELECT_MAIL,
				MNLB_START,
				MNLB_START_FADEOUT,
				MNLB_START_FADEIN,
				MNLB_VIEW,
				MNLB_END,
				MNLB_END_FADEOUT,
				MNLB_END_FADEIN
			}

			public const MNMB_STATE MNLB_SELECT_MAIL = MNMB_STATE.MNLB_SELECT_MAIL;

			public const MNMB_STATE MNLB_START = MNMB_STATE.MNLB_START;

			public const MNMB_STATE MNLB_START_FADEOUT = MNMB_STATE.MNLB_START_FADEOUT;

			public const MNMB_STATE MNLB_START_FADEIN = MNMB_STATE.MNLB_START_FADEIN;

			public const MNMB_STATE MNLB_VIEW = MNMB_STATE.MNLB_VIEW;

			public const MNMB_STATE MNLB_END = MNMB_STATE.MNLB_END;

			public const MNMB_STATE MNLB_END_FADEOUT = MNMB_STATE.MNLB_END_FADEOUT;

			public const MNMB_STATE MNLB_END_FADEIN = MNMB_STATE.MNLB_END_FADEIN;

			protected static int MAIL_NUM_PAR_PAGE = 5;

			protected MNMB_STATE state_;

			protected int line_;

			protected menu.Medget listTop_;

			protected ds.Vector<NPCMailEntry, ds.OrderSavedErasePolicy<NPCMailEntry>> MNPCMailArray_ = new ds.Vector<NPCMailEntry, ds.OrderSavedErasePolicy<NPCMailEntry>>(NUMBER_OF_RECEIVE_NPC_MAIL);

			protected sys2d.Cell[] arrows = new sys2d.Cell[2];

			protected int select_;

			public ScrollBar composit = new ScrollBar();

			public static MNSMediator mediator_ = null;

			public static dgs.UniqueNumber MBMogNetLetterBrowse_UN = new dgs.UniqueNumber();

			public new static int classIdentifier()
			{
				return MBMogNetLetterBrowse_UN.number();
			}

			public override object queryInterface(int class_id)
			{
				if (class_id == classIdentifier())
				{
					return this;
				}
				return null;
			}

			public static void setMediator(MNSMediator M)
			{
				mediator_ = M;
			}

			public void mnlbInitializeForNPC()
			{
				composit.sbPartsActivateProcess(1);
				composit.sbSetCapacity((short)MAIL_NUM_PAR_PAGE, (short)MNPCMailArray_.size());
				state_ = MNMB_STATE.MNLB_SELECT_MAIL;
				menu.MenuManager.getSingleton().changeFocusGroup(1);
			}

			public void mnlbListClear()
			{
				for (menu.Medget medget = listTop_.childNode(); medget != null; medget = medget.nextSibling())
				{
					((menu.MBText)medget.behavior().queryInterface(menu.MBText.classIdentifier()))?.bmTextVisibility(v: false);
				}
			}

			public void mnlbListUpdate()
			{
				menu.Medget medget = listTop_.childNode();
				for (int i = 0; i < MAIL_NUM_PAR_PAGE && i < MNPCMailArray_.size() - line_; i++)
				{
					if (medget == null)
					{
						break;
					}
					menu.MBText mBText = (menu.MBText)medget.behavior().queryInterface(menu.MBText.classIdentifier());
					if (mBText != null)
					{
						mBText.mbSetTextMsgNo(MNPCMailArray_[line_ + i].title_);
						mBText.bmTextVisibility(v: true);
					}
					medget = medget.nextSibling();
				}
				mnlbMailStatusUpdate();
			}

			public void mnlbNewMarkUpdate()
			{
				menu.Medget medget = listTop_.nextSibling().childNode();
				for (int i = 0; i < MAIL_NUM_PAR_PAGE && i < MNPCMailArray_.size() - line_; i++)
				{
					if (2 == MNPCMailArray_[line_ + i].state_ || MNPCMailArray_[line_ + i].state_ == 0)
					{
						sys2d.Cell cell = mediator_.mnsmNewMarkIcon(i);
						cell.SetShow(show: false);
					}
					else
					{
						sys2d.Cell cell2 = mediator_.mnsmNewMarkIcon(i);
						cell2.SetShow(show: true);
						cell2.SetPositionI(medget.x(), medget.y());
					}
					medget = medget.nextSibling();
				}
			}

			public void mnlbNewMarkAllHide()
			{
				for (int i = 0; MAIL_NUM_PAR_PAGE > i; i++)
				{
					mediator_.mnsmNewMarkIcon(i).SetShow(show: false);
				}
			}

			public void mnlbMailUpdate(int index)
			{
				if (!MNPCMailArray_.empty())
				{
					string buffer = "";
					dgs.msg.CMessageSys.getInstance().Main().writeCharacterStringCC(80, (short)(isIPad() ? 48 : 72), 0, (short)(isIPad() ? 32 : 24), dgs.TXT_COLOR.TXT_COLOR_WHITE, 9u, (uint)MNPCMailArray_[index].body_, ref buffer, shadow: true, 0);
					dgs.msg.CMessageSys.getInstance().Main().writeCharacterString(404, (short)(isIPad() ? 292 : 272), 0, 0, dgs.TXT_COLOR.TXT_COLOR_WHITE, 33u, (uint)MNPCMailArray_[index].name_, shadow: true, 0);
					MNNPCMailData.getSingleton().setNPCMailState(MNPCMailArray_[index].no_, NPCMailState.NPC_MAIL_YET_READ);
					MNPCMailArray_[index].state_ = 2;
					MNEvent mNEvent = new MNEvent();
					mNEvent.mneProgress();
				}
			}

			public void mnlbListUp()
			{
				if (line_ > 0)
				{
					line_--;
					mnlbListUpdate();
					mnlbNewMarkUpdate();
				}
			}

			public void mnlbListDown()
			{
				int num = 0;
				num = MNPCMailArray_.size() - MAIL_NUM_PAR_PAGE;
				if (num > 0 && num > line_)
				{
					line_++;
					mnlbListUpdate();
					mnlbNewMarkUpdate();
				}
			}

			public override void bmInitialize(menu.Medget M)
			{
				mnlbSetupNPCMail();
				listTop_ = ownerMedget.getNodeByID(TRANSCODE("list"));
				line_ = 0;
				composit.sbCreate();
				composit.sbSetPosition(236, 36);
				composit.sbSetHeight(120);
				composit.sbSetHandler(this);
				mnlbInitializeForNPC();
			}

			public override void bmPostInitialize(menu.Medget M)
			{
				mnlbListClear();
				mnlbListUpdate();
				mnlbNewMarkUpdate();
			}

			public override void bmBehave(menu.Medget M)
			{
				if (mediator_.mnsmButtonB().TouchButtonB())
				{
					mnlbCancelAction();
					return;
				}
				switch (state_)
				{
				case MNMB_STATE.MNLB_START:
					dgs.CFade.Main().fadeOut(10, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
					state_ = MNMB_STATE.MNLB_START_FADEOUT;
					break;
				case MNMB_STATE.MNLB_START_FADEOUT:
					if (dgs.CFade.Main().isFaded())
					{
						dgs.CFade.Main().fadeIn(10);
						ds.CVram.setMainPlaneVisiblity(bg0: true, bg1: false, bg2: false, bg3: true, obj: true);
						menu.MenuManager.getSingleton().changeFocusGroup(0);
						menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: false);
						for (menu.Medget medget2 = menu.MenuManager.getSingleton().root().childNode(); medget2 != null; medget2 = medget2.nextSibling())
						{
							((menu.MBText)medget2.behavior().queryInterface(menu.MBText.classIdentifier()))?.bmTextVisibility(v: false);
						}
						mnlbNewMarkAllHide();
						mnlbListClear();
						mnlbMailUpdate(select_);
						mediator_.mnsmCommonInterface(b: false, bLR: false);
						state_ = MNMB_STATE.MNLB_START_FADEIN;
					}
					break;
				case MNMB_STATE.MNLB_START_FADEIN:
					if (dgs.CFade.Main().isCleared())
					{
						menu.MenuManager.getSingleton().inputPermission(b: true);
						state_ = MNMB_STATE.MNLB_VIEW;
					}
					break;
				case MNMB_STATE.MNLB_VIEW:
					if ((ds.g_Pad.edge() & 2) != 0 || ds.g_TouchPanel.isTap())
					{
						mnlbCancelAction();
					}
					break;
				case MNMB_STATE.MNLB_END:
					dgs.CFade.Main().fadeOut(10, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
					state_ = MNMB_STATE.MNLB_END_FADEOUT;
					break;
				case MNMB_STATE.MNLB_END_FADEOUT:
					if (dgs.CFade.Main().isFaded())
					{
						dgs.CFade.Main().fadeIn(10);
						ds.CVram.setMainPlaneVisiblity(bg0: false, bg1: false, bg2: false, bg3: true, obj: true);
						dgs.msg.CMessageSys.getInstance().Main().dgsMMAreaErase(0, 0, 480, 320);
						menu.MenuManager.getSingleton().changeFocusGroup(1);
						menu.MenuManager.getSingleton().setFocuseMedget(select_ - line_);
						menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: true);
						for (menu.Medget medget = menu.MenuManager.getSingleton().root().childNode(); medget != null; medget = medget.nextSibling())
						{
							((menu.MBText)medget.behavior().queryInterface(menu.MBText.classIdentifier()))?.bmTextVisibility(v: true);
						}
						mnlbNewMarkUpdate();
						mnlbListUpdate();
						mediator_.mnsmCommonInterface(b: true, bLR: false);
						state_ = MNMB_STATE.MNLB_END_FADEIN;
					}
					break;
				case MNMB_STATE.MNLB_END_FADEIN:
					if (dgs.CFade.Main().isCleared())
					{
						menu.MenuManager.getSingleton().inputPermission(b: true);
						state_ = MNMB_STATE.MNLB_SELECT_MAIL;
					}
					break;
				}
			}

			public override void bmFinalize(menu.Medget M)
			{
				mnlbNewMarkAllHide();
				mediator_ = null;
				composit.sbDestroy();
				composit.sbSetHandler(null);
				MNPCMailArray_.clear();
			}

			public override void bmActivate(menu.Medget M)
			{
			}

			public override void bmSuspend(menu.Medget M)
			{
			}

			public override void bmResume(menu.Medget M)
			{
			}

			public override bool bmDecide(menu.Medget M)
			{
				if (M.myTag() + line_ < MNPCMailArray_.size())
				{
					select_ = M.myTag() + line_;
					state_ = MNMB_STATE.MNLB_START;
					mnlbNewMarkUpdate();
					menu.MenuManager.getSingleton().playSEDecide();
					menu.MenuManager.getSingleton().inputPermission(b: false);
				}
				else
				{
					menu.MenuManager.getSingleton().playSEBeep();
				}
				return true;
			}

			public override bool bmCancel(menu.Medget M)
			{
				mnlbCancelAction();
				return false;
			}

			public override bool bmDirection(menu.Medget M, int key)
			{
				menu.Medget focuseMedget = menu.MenuManager.getSingleton().getFocuseMedget();
				if ((ds.g_Pad.repeat() & 0x40) != 0)
				{
					if (focuseMedget.prevSibling() != null)
					{
						menu.MenuManager.getSingleton().initFocusM(focuseMedget.prevSibling());
						menu.MenuManager.getSingleton().playSEMoveCursor();
					}
					else
					{
						mnlbListUp();
						composit.sbSetLine((short)line_);
					}
				}
				else if ((ds.g_Pad.repeat() & 0x80) != 0)
				{
					if (focuseMedget.nextSibling() != null)
					{
						menu.MenuManager.getSingleton().initFocusM(focuseMedget.nextSibling());
						menu.MenuManager.getSingleton().playSEMoveCursor();
					}
					else
					{
						mnlbListDown();
						composit.sbSetLine((short)line_);
					}
				}
				return true;
			}

			public void mnlbSetupNPCMail()
			{
				MNEvent mNEvent = new MNEvent();
				mNEvent.mneProgress();
				MNPCMailArray_.clear();
				for (int i = 0; i < NUMBER_OF_RECEIVE_NPC_MAIL; i++)
				{
					if (g_NpcMailEntry[i].name_ == g_NpcName[mediator_.MNSSelectPerson_.getSelectItem()] && (MNNPCMailData.getSingleton().getNPCMailState(i) == NPCMailState.NPC_MAIL_NOT_READ || MNNPCMailData.getSingleton().getNPCMailState(i) == NPCMailState.NPC_MAIL_YET_READ))
					{
						MNPCMailArray_.push_back(g_NpcMailEntry[i]);
						MNPCMailArray_.at(MNPCMailArray_.size() - 1).state_ = (int)MNNPCMailData.getSingleton().getNPCMailState(i);
					}
				}
			}

			public void mnlbMailStatusUpdate()
			{
				ds.Vector<MNMail, ds.OrderSavedErasePolicy<MNMail>> vector = const_cast<ds.Vector<MNMail, ds.OrderSavedErasePolicy<MNMail>>>(MNMemento.getSingleton().mnmMailArray());
				menu.Medget medget = listTop_.childNode();
				for (int i = 0; i < MAIL_NUM_PAR_PAGE && i < vector.size() - line_; i++)
				{
					if (medget == null)
					{
						break;
					}
					vector[i].checkFlag(MNMAIL_FLAGS.MNMF_READ);
					medget = medget.nextSibling();
				}
			}

			public void sbehScrolled(short currentLine)
			{
				line_ = currentLine;
				mnlbListUpdate();
				mnlbNewMarkUpdate();
				menu.MenuManager.getSingleton().playSEMoveCursor();
			}

			public void mnlbCancelAction()
			{
				switch (state_)
				{
				case MNMB_STATE.MNLB_SELECT_MAIL:
					if (mbNotifier != null)
					{
						mbNotifier.mbnNotify(this, 2u, 0u);
					}
					menu.MenuManager.getSingleton().playSECancel();
					break;
				case MNMB_STATE.MNLB_VIEW:
					state_ = MNMB_STATE.MNLB_END;
					menu.MenuManager.getSingleton().playSECancel();
					menu.MenuManager.getSingleton().inputPermission(b: false);
					break;
				}
			}
		}
	}
}
