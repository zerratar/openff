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
		public class MBStatus : MenuBehavior
		{
			public class CONDITION
			{
				public sys2d.Cell[] cell = new sys2d.Cell[8];

				public bool[] bEnable = new bool[8];

				public CONDITION()
				{
					for (int i = 0; i < cell.Length; i++)
					{
						cell[i] = new sys2d.Cell();
					}
				}
			}

			public static dgs.UniqueNumber MBStatus_UN = new dgs.UniqueNumber();

			private CONDITION conP1 = new CONDITION();

			private CONDITION conP2 = new CONDITION();

			private CONDITION conP3 = new CONDITION();

			private CONDITION conP4 = new CONDITION();

			private bool savedVisibility;

			private dgs.DGSMessage[] pMsg = new dgs.DGSMessage[128];

			~MBStatus()
			{
				int i = 0;
				for (; i < 128; i++)
				{
					if (pMsg[i] != null)
					{
						pMsg[i].release();
						pMsg[i] = null;
					}
				}
			}

			public override void bmInitialize(Medget M)
			{
				dgs.msg.CMessageMng.MSF_HANDLE_KIND mSF_HANDLE_KIND = dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8;
				dgs.DGSMessageManager dGSMessageManager = null;
				dGSMessageManager = ((M.display() != 1) ? dgs.msg.CMessageSys.getInstance().Sub() : dgs.msg.CMessageSys.getInstance().Main());
				for (int i = 0; i < 8; i++)
				{
					conP1.bEnable[i] = false;
					conP2.bEnable[i] = false;
					conP3.bEnable[i] = false;
					conP4.bEnable[i] = false;
				}
				int num = M.x();
				int num2 = M.y();
				for (int j = 0; j < 4; j++)
				{
					if (pl.PlayerParty.instance().player((byte)j).isEnable())
					{
						string text = pl.PlayerParty.instance().player((byte)j).name();
						if (text == null)
						{
							continue;
						}
						CreateCharName(dGSMessageManager, mSF_HANDLE_KIND, num, num2, text);
						CreateCharLV(dGSMessageManager, mSF_HANDLE_KIND, num + 160, num2, j);
						CreateJobName(dGSMessageManager, mSF_HANDLE_KIND, num + 74, num2, j);
						CreateHpMpString(dGSMessageManager, mSF_HANDLE_KIND, num + 64, num2 + 16);
						CreateCharHp(dGSMessageManager, mSF_HANDLE_KIND, num + 128, num2 + 16, j);
						CreateCharMHp(dGSMessageManager, mSF_HANDLE_KIND, num + 128, num2 + 16, j);
						CreateStatusIcon(num, num2 + 16, j);
						CreateCharMp(dGSMessageManager, mSF_HANDLE_KIND, num + 90, num2 + 32, j);
					}
					num2 += 72;
				}
			}

			public void bmRefresh(Medget M)
			{
				bmFinalize(M);
				bmInitialize(M);
			}

			public override void bmBehave(Medget M)
			{
			}

			public override void bmFinalize(Medget M)
			{
				for (int i = 0; i < 128; i++)
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
						conP1.cell[j].Release();
						sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(conP1.cell[j]);
						conP1.bEnable[j] = false;
					}
					if (conP2.bEnable[j])
					{
						conP2.cell[j].Release();
						sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(conP2.cell[j]);
						conP2.bEnable[j] = false;
					}
					if (conP3.bEnable[j])
					{
						conP3.cell[j].Release();
						sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(conP3.cell[j]);
						conP3.bEnable[j] = false;
					}
					if (conP4.bEnable[j])
					{
						conP4.cell[j].Release();
						sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(conP4.cell[j]);
						conP4.bEnable[j] = false;
					}
				}
			}

			public override bool bmDecide(Medget M)
			{
				return true;
			}

			public override bool bmCancel(Medget M)
			{
				return false;
			}

			public override bool bmDirection(Medget M, int key)
			{
				return false;
			}

			public override void bmSuspend(Medget M)
			{
				for (int i = 0; i < 128; i++)
				{
					if (pMsg[i] != null)
					{
						savedVisibility = pMsg[i].activity();
						pMsg[i].setActivity(b: false);
					}
				}
			}

			public override void bmResume(Medget M)
			{
				for (int i = 0; i < 128; i++)
				{
					if (pMsg[i] != null)
					{
						pMsg[i].setActivity(savedVisibility);
					}
				}
			}

			public void bmStatusVisibility(bool v)
			{
				for (int i = 0; i < 128; i++)
				{
					if (pMsg[i] != null)
					{
						pMsg[i].setVisibility(v);
					}
				}
			}

			public int CheckEnableMessageNo()
			{
				for (int i = 0; i < 128; i++)
				{
					if (pMsg[i] == null)
					{
						return i;
					}
				}
				return -1;
			}

			public void CreateCharName(dgs.DGSMessageManager pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND font_size, int tx, int ty, string pName)
			{
				int num = CheckEnableMessageNo();
				if (num >= 0)
				{
					pMsg[num] = pm.createMessage(pName, (int)font_size);
					if (pMsg[num] != null)
					{
						pMsg[num].setPosition((short)tx, (short)ty, erase: true);
						pMsg[num].setDisplaySpeed(byte.MaxValue);
						pMsg[num].setDisplayWait(0);
					}
				}
			}

			public void CreateCharLV(dgs.DGSMessageManager pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND font_size, int tx, int ty, int cNo)
			{
				int num = CheckEnableMessageNo();
				if (num < 0)
				{
					return;
				}
				int value = pl.PlayerParty.instance().player((byte)cNo).level()
					.get();
				pMsg[num] = pm.createMessage(50414u, -1, (int)font_size);
				if (pMsg[num] != null)
				{
					pMsg[num].setPosition((short)tx, (short)ty, erase: true);
					pMsg[num].setDisplaySpeed(byte.MaxValue);
					pMsg[num].setDisplayWait(0);
					num = CheckEnableMessageNo();
					if (num >= 0)
					{
						ds.Vector2<short> vector = new ds.Vector2<short>();
						dgs.msg.CMessageSys.getInstance().changeValueFont(value, out var after);
						pMsg[num] = pm.createMessage(after, (int)font_size);
						pMsg[num].setPosition((short)(tx + 40), (short)ty, erase: true);
						pMsg[num].getTextSize(vector);
						pMsg[num].setPosition((short)(tx + 40 - vector.vx), (short)ty, erase: true);
						pMsg[num].setDisplaySpeed(byte.MaxValue);
						pMsg[num].setDisplayWait(0);
					}
				}
			}

			public void CreateJobName(dgs.DGSMessageManager pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND font_size, int tx, int ty, int cNo)
			{
				int num = CheckEnableMessageNo();
				if (num >= 0)
				{
					int msg_number = 50105 + pl.PlayerParty.instance().player((byte)cNo).jobManager()
						.nowJob();
					// PORT: a job of the mods' own has a name of its own (ProgressionLayer).
					string ownJob = OpenFF.Client.ProgressionLayer.JobNameOverride(pl.PlayerParty.instance().player((byte)cNo).playerId(), pl.PlayerParty.instance().player((byte)cNo).jobManager().nowJob());
					pMsg[num] = ownJob != null ? pm.createMessage(ownJob, (int)font_size) : pm.createMessage((uint)msg_number, dgs.INVALID_MSDHANDLE, (int)font_size);
					if (pMsg[num] != null)
					{
						pMsg[num].setPosition((short)tx, (short)ty, erase: true);
						pMsg[num].setDisplaySpeed(byte.MaxValue);
						pMsg[num].setDisplayWait(0);
					}
				}
			}

			public void CreateHpMpString(dgs.DGSMessageManager pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND font_size, int tx, int ty)
			{
				int num = CheckEnableMessageNo();
				if (num < 0)
				{
					return;
				}
				int msg_number = 50415;
				pMsg[num] = pm.createMessage((uint)msg_number, dgs.INVALID_MSDHANDLE, (int)font_size);
				if (pMsg[num] == null)
				{
					return;
				}
				pMsg[num].setPosition((short)tx, (short)ty, erase: true);
				pMsg[num].setDisplaySpeed(byte.MaxValue);
				pMsg[num].setDisplayWait(0);
				num = CheckEnableMessageNo();
				if (num >= 0)
				{
					int msg_number2 = 50416;
					pMsg[num] = pm.createMessage((uint)msg_number2, dgs.INVALID_MSDHANDLE, (int)font_size);
					if (pMsg[num] != null)
					{
						pMsg[num].setPosition((short)tx, (short)(ty + 16), erase: true);
						pMsg[num].setDisplaySpeed(byte.MaxValue);
						pMsg[num].setDisplayWait(0);
					}
				}
			}

			public void CreateCharHp(dgs.DGSMessageManager pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND font_size, int tx, int ty, int cNo)
			{
				int num = CheckEnableMessageNo();
				if (num < 0)
				{
					return;
				}
				int now = pl.PlayerParty.instance().player((byte)cNo).hp()
					.getNow();
				dgs.msg.CMessageSys.getInstance().changeValueFont(now, out var after);
				pMsg[num] = pm.createMessage(after, (int)font_size);
				if (pMsg[num] == null)
				{
					return;
				}
				ds.Vector2<short> vector = new ds.Vector2<short>();
				pMsg[num].getTextSize(vector);
				pMsg[num].setPosition((short)(tx - vector.vx), (short)ty, erase: true);
				pMsg[num].setDisplaySpeed(byte.MaxValue);
				pMsg[num].setDisplayWait(0);
				SetCharLifeColor(cNo, num);
				num = CheckEnableMessageNo();
				if (num >= 0)
				{
					pMsg[num] = pm.createMessage(50418u, dgs.INVALID_MSDHANDLE, (int)font_size);
					if (pMsg[num] != null)
					{
						pMsg[num].setPosition((short)(tx + 3), (short)ty, erase: true);
						pMsg[num].setDisplaySpeed(byte.MaxValue);
						pMsg[num].setDisplayWait(0);
					}
				}
			}

			public void SetCharLifeColor(int cNo, int enableNo)
			{
				pMsg[enableNo].setMessageColor(pl.PlayerParty.instance().player((byte)cNo).checkHpColor());
			}

			public void CreateCharMp(dgs.DGSMessageManager pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND font_size, int tx, int ty, int cNo)
			{
				int[] array = new int[8];
				for (int i = 0; i < 8; i++)
				{
					array[i] = pl.PlayerParty.instance().player((byte)cNo).mp(i)
						.getNow();
				}
				int num = tx;
				for (int j = 0; j < 8; j++)
				{
					ds.Vector2<short> vector = new ds.Vector2<short>();
					int num2 = CheckEnableMessageNo();
					if (num2 < 0)
					{
						break;
					}
					dgs.msg.CMessageSys.getInstance().changeValueFont(array[j], out var after);
					int num3;
					if (j != 3 && j != 7)
					{
						sprintf(out after, "%s /", after);
						num3 = 28;
					}
					else
					{
						num3 = 20;
					}
					pMsg[num2] = pm.createMessage(after, (int)font_size);
					if (pMsg[num2] == null)
					{
						break;
					}
					pMsg[num2].getTextSize(vector);
					pMsg[num2].setPosition((short)(num + num3 - vector.vx), (short)ty, erase: true);
					pMsg[num2].setDisplaySpeed(byte.MaxValue);
					pMsg[num2].setDisplayWait(0);
					num += 28;
					if (j == 3)
					{
						num = tx;
						ty += 12;
					}
				}
			}

			public void CreateCharMHp(dgs.DGSMessageManager pm, dgs.msg.CMessageMng.MSF_HANDLE_KIND _msfHandle, int tx, int ty, int ncNo)
			{
				int num = CheckEnableMessageNo();
				if (num >= 0)
				{
					dgs.msg.CMessageSys.getInstance().changeValueFont(pl.PlayerParty.instance().player((byte)ncNo).hp()
						.getLimit(), out var after);
					pMsg[num] = pm.createMessage(after, (int)_msfHandle);
					if (pMsg[num] != null)
					{
						pMsg[num].setPosition((short)(tx + 11), (short)ty, erase: true);
						pMsg[num].setDisplaySpeed(byte.MaxValue);
						pMsg[num].setDisplayWait(0);
						SetCharLifeColor(ncNo, num);
					}
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
						CreateCondition(b, pTarget, tx + 16 * (b % 3), ty + 16 * (b / 3), array[i]);
						b++;
					}
					num2 <<= 1;
				}
				if (pl.PlayerParty.instance().player((byte)ncNo).jobPenaltyTime() > 0)
				{
					CreateCondition(b, pTarget, tx + 16 * (b % 3), ty + 16 * (b / 3), 46);
					b++;
				}
			}

			public void CreateCondition(int indexNo, CONDITION pTarget, int x, int cy, int cellNo)
			{
				pTarget.bEnable[indexNo] = true;
				pTarget.cell[indexNo].copy(MenuManager.getSingleton().GetSmallIcon2d());
				pTarget.cell[indexNo].SetCell((ushort)cellNo);
				pTarget.cell[indexNo].SetPositionI(x, cy);
				sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(pTarget.cell[indexNo]);
			}

			public new static int classIdentifier()
			{
				return MBStatus_UN.number();
			}

			public override object queryInterface(int class_id)
			{
				if (class_id == classIdentifier())
				{
					return this;
				}
				return null;
			}
		}
	}
}
