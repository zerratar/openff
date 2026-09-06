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
using OpenFF.Platform;
using OpenFF.Resources;

internal static partial class GlobalScope
{
	public static partial class menu
	{
		public class MBItemUse : MenuBehavior
		{
			public class CONDITION
			{
				public int saveLife;

				public int saveCondition;

				public int msg_no;

				public sys2d.Cell[] conditionCell = new sys2d.Cell[8];

				public bool[] bEnable = new bool[8];

				public ds.Vector2<short>[] box = new ds.Vector2<short>[8];

				public CONDITION()
				{
					for (int i = 0; i < conditionCell.Length; i++)
					{
						conditionCell[i] = new sys2d.Cell();
					}
				}
			}

			public const int USE_MESSAGE_NUM = 64;

			public const int USE_ICONS_MAX = 8;

			public static dgs.UniqueNumber MBItemUse_UN = new dgs.UniqueNumber();

			private int itemNo;

			private bool createAllRangeFlag;

			private CONDITION conP1 = new CONDITION();

			private CONDITION conP2 = new CONDITION();

			private CONDITION conP3 = new CONDITION();

			private CONDITION conP4 = new CONDITION();

			private dgs.DGSMessage[] pMsg = new dgs.DGSMessage[64];

			public MBItemUse()
			{
				for (int i = 0; i < 64; i++)
				{
					pMsg[i] = null;
				}
			}

			~MBItemUse()
			{
				int i = 0;
				for (; i < 64; i++)
				{
					if (pMsg[i] != null)
					{
						pMsg[i].release();
						pMsg[i] = null;
					}
				}
			}

			public int CheckEnableMessageNo()
			{
				for (int i = 0; i < 64; i++)
				{
					if (pMsg[i] == null)
					{
						return i;
					}
				}
				return -1;
			}

			public override void bmInitialize(Medget M)
			{
				for (int i = 0; i < 64; i++)
				{
					pMsg[i] = null;
				}
				for (int j = 0; j < 8; j++)
				{
					conP1.bEnable[j] = false;
					conP2.bEnable[j] = false;
					conP3.bEnable[j] = false;
					conP4.bEnable[j] = false;
				}
				createAllRangeFlag = false;
				conP1.saveCondition = 0;
				conP2.saveCondition = 0;
				conP3.saveCondition = 0;
				conP4.saveCondition = 0;
				conP1.saveLife = 0;
				conP2.saveLife = 0;
				conP3.saveLife = 0;
				conP4.saveLife = 0;
				conP1.msg_no = -1;
				conP2.msg_no = -1;
				conP3.msg_no = -1;
				conP4.msg_no = -1;
				itemNo = MenuManager.getSingleton().GetTargetItemNo();
				dgs.DGSMessageManager pm = null;
				if (M.display() == 1)
				{
					pm = dgs.msg.CMessageSys.getInstance().Main();
				}
				else if (M.display() == 0)
				{
					pm = dgs.msg.CMessageSys.getInstance().Sub();
				}
				int num = 0;
				Medget[] array = new Medget[5];
				Medget[] array2 = array;
				Medget medget = M.childNode();
				for (int k = 0; k < 4; k++)
				{
					if (pl.PlayerParty.instance().player((byte)k).isEnable())
					{
						string text = pl.PlayerParty.instance().player((byte)k).name();
						if (text == null)
						{
							continue;
						}
						CreateCharName(pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8, medget.x() + 40, medget.y() + 8, text);
						CreateLifeString(pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8, medget.x() + 40 + 48, medget.y() + 20, k);
						CreateMaxHp(pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8, medget.x() + 40 + 48, medget.y() + 20, k);
						CreateStatusIcon(medget.x() + 40 + 16, medget.y() + 32, k);
						array2[num] = medget;
						num++;
						int num2 = pl.PlayerParty.instance().player((byte)k).playerId();
						wmenu.CWMenuManager.Instance().SetShowPcFace(num2, show: true);
						NNS_G2dSetBGCellScale(8 + num2, 0.5714286f);
						NNS_G2dSetBGCellPositon(8 + num2, medget.x(), medget.y() + 8);
					}
					else
					{
						MenuManager.getSingleton().leaveFocusList(medget);
					}
					medget = medget.nextSibling();
				}
				MenuManager.getSingleton().leaveFocusList(medget);
				if (num >= 2 && SetFocusAllTarget(ref array2[num], M, pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8))
				{
					array2[num] = medget;
					num++;
					for (int l = 0; l < num; l++)
					{
						OS_Printf("pValid[%d] = 0x%08x\n", l, array2[l]);
					}
				}
				for (int m = 0; m < num && array2[m] != null; m++)
				{
					if (m > 0)
					{
						SET_PREV(array2[m], array2[m - 1]);
						if (array2[m - 1] != null)
						{
							SET_NEXT(array2[m - 1], array2[m]);
						}
					}
					if (m < num - 1)
					{
						SET_NEXT(array2[m], array2[m + 1]);
						if (array2[m + 1] != null)
						{
							SET_PREV(array2[m + 1], array2[m]);
						}
					}
				}
				if (array2[0] != null && num > 0)
				{
					SET_PREV(array2[0], array2[num - 1]);
					SET_NEXT(array2[num - 1], array2[0]);
				}
				OS_Printf("itemNo = %d\n", itemNo);
				MenuManager.getSingleton().initFocusM(array2[0]);
				MenuManager.getSingleton().SetTargetCharNo((sbyte)array2[0].work());
			}

			public override void bmBehave(Medget M)
			{
			}

			public override void bmFinalize(Medget M)
			{
				for (int i = 0; i < 64; i++)
				{
					if (pMsg[i] != null)
					{
						pMsg[i].release();
						pMsg[i] = null;
					}
				}
				for (int j = 0; j < 8; j++)
				{
					if (conP1.bEnable[j])
					{
						conP1.conditionCell[j].Release();
						sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(conP1.conditionCell[j]);
					}
					if (conP2.bEnable[j])
					{
						conP2.conditionCell[j].Release();
						sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(conP2.conditionCell[j]);
					}
					if (conP3.bEnable[j])
					{
						conP3.conditionCell[j].Release();
						sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(conP3.conditionCell[j]);
					}
					if (conP4.bEnable[j])
					{
						conP4.conditionCell[j].Release();
						sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(conP4.conditionCell[j]);
					}
				}
				for (int k = 0; k < 4; k++)
				{
					wmenu.CWMenuManager.Instance().SetShowPcFace(k, show: false);
				}
			}

			public override bool bmDecide(Medget M)
			{
				MenuManager.getSingleton().SetDecideButtonState(0);
				MenuManager.getSingleton().SetTargetCharNo((sbyte)MenuManager.getSingleton().getFocuseMedget().work());
				return true;
			}

			public override bool bmCancel(Medget M)
			{
				MenuManager.getSingleton().SetCancelButtonState(0);
				return false;
			}

			public override bool bmDirection(Medget M, int key)
			{
				Medget medget = MenuManager.getSingleton().getFocuseMedget();
				if ((ds.g_Pad.repeat() & 0x60) != 0)
				{
					medget = GET_PREV(medget);
				}
				else if ((ds.g_Pad.repeat() & 0x90) != 0)
				{
					medget = GET_NEXT(medget);
				}
				MenuManager.getSingleton().initFocusM(medget);
				MenuManager.getSingleton().SetTargetCharNo((int)medget.work());
				OS_Printf("SetTargetCharNo %d\n", medget.work());
				MenuManager.getSingleton().playSEMoveCursor();
				return true;
			}

			public override void bmSuspend(Medget M)
			{
				for (int i = 0; i < 64; i++)
				{
					if (pMsg[i] != null)
					{
						pMsg[i].setActivity(b: false);
					}
				}
			}

			public override void bmResume(Medget M)
			{
				for (int i = 0; i < 64; i++)
				{
					if (pMsg[i] != null)
					{
						pMsg[i].setActivity(b: true);
					}
				}
			}

			public void bmItemUseVisibility(bool v)
			{
				for (int i = 0; i < 64; i++)
				{
					if (pMsg[i] != null)
					{
						pMsg[i].setVisibility(v);
					}
				}
			}

			public void CreateCharName(dgs.DGSMessageManager pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND font_size, int tx, int ty, string pName)
			{
				int num = CheckEnableMessageNo();
				pMsg[num] = pm.createMessage(pName, (int)font_size);
				if (pMsg[num] != null)
				{
					pMsg[num].setPosition((short)tx, (short)ty, erase: true);
					pMsg[num].setDisplaySpeed(byte.MaxValue);
					pMsg[num].setDisplayWait(0);
				}
			}

			public void CreateLifeString(dgs.DGSMessageManager pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND font_size, int tx, int ty, int no)
			{
				int num = CheckEnableMessageNo();
				ds.Vector2<short> vector = new ds.Vector2<short>();
				CONDITION cONDITION = null;
				switch (no)
				{
				case 0:
					cONDITION = conP1;
					break;
				case 1:
					cONDITION = conP2;
					break;
				case 2:
					cONDITION = conP3;
					break;
				case 3:
					cONDITION = conP4;
					break;
				}
				int now = pl.PlayerParty.instance().player((byte)no).hp()
					.getNow();
				dgs.msg.CMessageSys.getInstance().changeValueFont(now, out var after);
				pMsg[num] = pm.createMessage(after, (int)font_size);
				if (pMsg[num] != null)
				{
					pMsg[num].getTextSize(vector);
					pMsg[num].setPosition((short)(tx - vector.vx), (short)ty, erase: true);
					pMsg[num].setDisplaySpeed(byte.MaxValue);
					pMsg[num].setDisplayWait(0);
					cONDITION.msg_no = num;
					ChangeLifeColor(no, num);
				}
				num = CheckEnableMessageNo();
				pMsg[num] = pm.createMessage(50418u, dgs.INVALID_MSDHANDLE, (int)font_size);
				if (pMsg[num] != null)
				{
					pMsg[num].setPosition((short)(tx + 3), (short)ty, erase: true);
					pMsg[num].setDisplaySpeed(byte.MaxValue);
					pMsg[num].setDisplayWait(0);
				}
				cONDITION.saveLife = now;
			}

			public void ChangeLifeColor(int ncNo, int enableNo)
			{
				pMsg[enableNo].setMessageColor(pl.PlayerParty.instance().player((byte)ncNo).checkHpColor());
			}

			public void CreateAllRange(dgs.DGSMessageManager pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND font_size, int tx, int ty)
			{
				int num = CheckEnableMessageNo();
				pMsg[num] = pm.createMessage(50209u, dgs.INVALID_MSDHANDLE, (int)font_size);
				pMsg[num].setPosition((short)tx, (short)ty, erase: true);
				pMsg[num].setDisplaySpeed(byte.MaxValue);
				pMsg[num].setDisplayWait(0);
			}

			public void CreateCondition(int indexNo, CONDITION pTarget, int x, int cy, int cellNo)
			{
				pTarget.bEnable[indexNo] = true;
				pTarget.conditionCell[indexNo].copy(MenuManager.getSingleton().GetSmallIcon2d());
				pTarget.conditionCell[indexNo].SetCell((ushort)cellNo);
				pTarget.conditionCell[indexNo].SetPositionI(x, cy);
				sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(pTarget.conditionCell[indexNo]);
			}

			public void CreateMaxHp(dgs.DGSMessageManager pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND font_size, int tx, int ty, int ncNo)
			{
				int num = CheckEnableMessageNo();
				dgs.msg.CMessageSys.getInstance().changeValueFont(pl.PlayerParty.instance().player((byte)ncNo).hp()
					.getLimit(), out var after);
				pMsg[num] = pm.createMessage(after, (int)font_size);
				if (pMsg[num] != null)
				{
					ds.Vector2<short> vec = new ds.Vector2<short>();
					pMsg[num].getTextSize(vec);
					pMsg[num].setPosition((short)(tx + 11), (short)ty, erase: true);
					pMsg[num].setDisplaySpeed(byte.MaxValue);
					pMsg[num].setDisplayWait(0);
					pMsg[num].setMessageColor(pl.PlayerParty.instance().player((byte)ncNo).checkHpColor());
				}
			}

			public void CreateStatusIcon(int tx, int ty, int ncNo)
			{
				int[] array = new int[8] { 0, 1, 3, 4, 5, 6, 7, 46 };
				int num = pl.PlayerParty.instance().player((byte)ncNo).condition()
					.normalCondition();
				num &= -129;
				sbyte b = 0;
				CONDITION pTarget = null;
				switch (ncNo)
				{
				case 0:
					pTarget = conP1;
					break;
				case 1:
					pTarget = conP2;
					break;
				case 2:
					pTarget = conP3;
					break;
				case 3:
					pTarget = conP4;
					break;
				}
				int num2 = 1;
				for (int i = 0; i < 7; i++)
				{
					if ((num & num2) != 0)
					{
						CreateCondition(b, pTarget, tx + 16 * b, ty, array[i]);
						b++;
					}
					num2 <<= 1;
				}
				if (pl.PlayerParty.instance().player((byte)ncNo).jobPenaltyTime() > 0)
				{
					CreateCondition(b, pTarget, tx + 16 * b, ty, 46);
					b++;
				}
			}

			public bool SetFocusAllTarget(ref Medget pTail, Medget pMedget, dgs.DGSMessageManager pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND font_size)
			{
				Medget medget = pMedget.childNode();
				medget = medget.nextSibling();
				medget = medget.nextSibling();
				medget = medget.nextSibling();
				medget = medget.nextSibling();
				if (wmenu.CWMenuManager.Instance().GetKind() == wmenu.CWMenuMemberBase.WMENU_KIND.WMENU_KIND_ITEM && 4001 <= itemNo && itemNo <= 4228)
				{
					createAllRangeFlag = false;
					MenuManager.getSingleton().leaveFocusList(medget);
					return false;
				}
				if (wmenu.CWMenuManager.Instance().GetKind() == wmenu.CWMenuMemberBase.WMENU_KIND.WMENU_KIND_MAGIC && (itemNo == 4014 || itemNo == 4017 || itemNo == 4023))
				{
					pTail = medget;
					createAllRangeFlag = true;
					CreateAllRange(pm, font_size, medget.x(), medget.y() + (medget.height() - 12) / 2);
					MenuManager.getSingleton().joinFocusList(medget);
					return true;
				}
				if (itemNo >= 1000 && (itm.ItemManager.instance().itemParameter((short)itemNo).targetPossible() & 0x200) != 0)
				{
					pTail = medget;
					createAllRangeFlag = true;
					CreateAllRange(pm, font_size, medget.x(), medget.y() + (medget.height() - 12) / 2);
					MenuManager.getSingleton().joinFocusList(medget);
					return true;
				}
				createAllRangeFlag = false;
				MenuManager.getSingleton().leaveFocusList(medget);
				return false;
			}

			public void UpdateConditionLife(Medget pMedget)
			{
				Medget medget = pMedget.childNode();
				dgs.DGSMessageManager pm = dgs.msg.CMessageSys.getInstance().Sub();
				dgs.msg.CMessageMng.MSF_HANDLE_KIND font_size = dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8;
				bmFinalize(pMedget);
				for (int i = 0; i < 4; i++)
				{
					if (pl.PlayerParty.instance().player((byte)i).isEnable())
					{
						string text = pl.PlayerParty.instance().player((byte)i).name();
						if (text == null)
						{
							continue;
						}
						CreateCharName(pm, font_size, medget.x() + 40, medget.y() + 8, text);
						CreateLifeString(pm, font_size, medget.x() + 40 + 48, medget.y() + 20, i);
						CreateMaxHp(pm, font_size, medget.x() + 40 + 48, medget.y() + 20, i);
						CreateStatusIcon(medget.x() + 40 + 16, medget.y() + 32, i);
						int num = pl.PlayerParty.instance().player((byte)i).playerId();
						wmenu.CWMenuManager.Instance().SetShowPcFace(num, show: true);
						NNS_G2dSetBGCellScale(8 + num, 0.5714286f);
						NNS_G2dSetBGCellPositon(8 + num, medget.x(), medget.y() + 8);
					}
					medget = medget.nextSibling();
				}
				if (createAllRangeFlag)
				{
					CreateAllRange(pm, font_size, medget.x(), medget.y() + (medget.height() - 12) / 2);
				}
			}

			public new static int classIdentifier()
			{
				return MBItemUse_UN.number();
			}

			public override object queryInterface(int class_id)
			{
				if (class_id == classIdentifier())
				{
					return this;
				}
				return null;
			}

			public override bool bmUseTap(Medget M)
			{
				return true;
			}
		}
	}
}
