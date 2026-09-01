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
using android.text;
using android.widget;
using java.io;
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class btl
	{
		public class PlayerWindow : BasicBattleWindow
		{
			private int[] nameMngId_ = new int[4];

			private int[] HpMngId_ = new int[4];

			private int[] maxHpMngId_ = new int[4];

			private bool show_;

			private int nowPlayer_;

			public override void setup()
			{
				window_.Initialize();
				clearAllNameMngId();
				clearAllHpMngId();
				clearAllMaxHpMngId();
				created_ = false;
				show(flag: true);
				nowPlayer_ = -1;
			}

			public override void cleanup()
			{
				release();
			}

			public override void execute()
			{
				if (!created_ || !show_)
				{
					return;
				}
				for (int i = 0; i < 4; i++)
				{
					if (pl.PlayerParty.instance().player((byte)i).isEnable())
					{
						sys2d.PrimitiveQuadDraw primitiveQuadDraw = new sys2d.PrimitiveQuadDraw();
						primitiveQuadDraw.setPosition(CoasterPosition(i, 0), CoasterPosition(i, 1));
						primitiveQuadDraw.setDepth(0);
						if (i == nowPlayer_)
						{
							primitiveQuadDraw.setColor(ds.setGXRgb(24, 24, 0), ds.setGXRgb(24, 24, 0), ds.setGXRgb(31, 31, 0), ds.setGXRgb(31, 31, 0));
						}
						else
						{
							primitiveQuadDraw.setColor(ds.setGXRgb(0, 0, 0), ds.setGXRgb(0, 0, 0), ds.setGXRgb(0, 0, 0), ds.setGXRgb(0, 0, 0));
						}
						primitiveQuadDraw.setAlpha(15);
						primitiveQuadDraw.draw();
					}
				}
			}

			public void clearNameMngId(int i)
			{
				nameMngId_[i] = -1;
			}

			public void clearAllNameMngId()
			{
				for (int i = 0; i < 4; i++)
				{
					clearNameMngId(i);
				}
			}

			public void clearHpMngId(int i)
			{
				HpMngId_[i] = -1;
			}

			public void clearAllHpMngId()
			{
				for (int i = 0; i < 4; i++)
				{
					clearHpMngId(i);
				}
			}

			public void clearMaxHpMngId(int i)
			{
				maxHpMngId_[i] = -1;
			}

			public void clearAllMaxHpMngId()
			{
				for (int i = 0; i < 4; i++)
				{
					clearMaxHpMngId(i);
				}
			}

			public void changeColor(int i)
			{
				if (pl.PlayerParty.instance().player((byte)i).isEnable())
				{
					dgs.TXT_COLOR messageColor = static_cast<dgs.TXT_COLOR>(pl.PlayerParty.instance().player((byte)i).checkHpColor());
					dgs.DGSMessage dGSMessage = (dGSMessage = dgs.msg.CMessageSys.getInstance().Main().Message(HpMngId_[i]));
					dGSMessage.setMessageColor(messageColor);
					dGSMessage = dgs.msg.CMessageSys.getInstance().Main().Message(maxHpMngId_[i]);
					dGSMessage.setMessageColor(messageColor);
				}
			}

			public override void create()
			{
				if (!created_)
				{
					createPlayerName();
					createHp();
					createMaxHp();
					for (int i = 0; i < 4; i++)
					{
						changeColor(i);
					}
					created_ = true;
				}
			}

			public override void release()
			{
				window_.Release();
				releasePlayerName();
				releaseHp();
				releaseMaxHp();
				created_ = false;
			}

			public override void show(bool flag)
			{
				show_ = flag;
				showPlayerName(show_);
				showHp(show_);
				showMaxHp(show_);
			}

			public void createPlayerName()
			{
				for (int i = 0; i < 4; i++)
				{
					if (pl.PlayerParty.instance().player((byte)i).isEnable())
					{
						nameMngId_[i] = dgs.msg.CMessageSys.getInstance().Main().createMessage(pl.PlayerParty.instance().player((byte)i).name(), (ushort)PlayerNamePosition(i).vx, (ushort)PlayerNamePosition(i).vy, dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_COMMON, dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8);
						dgs.DGSMessage dGSMessage = dgs.msg.CMessageSys.getInstance().Main().Message(nameMngId_[i]);
						dGSMessage.setDisplaySpeed(byte.MaxValue);
						dGSMessage.setDisplayWait(0);
						dGSMessage.setShadow(b: true);
					}
				}
			}

			public void releasePlayerName()
			{
				for (int i = 0; i < 4; i++)
				{
					if (nameMngId_[i] != -1)
					{
						dgs.msg.CMessageSys.getInstance().Main().releaseMessage(nameMngId_[i]);
					}
				}
			}

			public void showPlayerName(bool flag)
			{
				for (int i = 0; i < 4; i++)
				{
					if (nameMngId_[i] != -1)
					{
						dgs.msg.CMessageSys.getInstance().Main().setVisibility(nameMngId_[i], flag);
					}
				}
			}

			public void createHp()
			{
				for (int i = 0; i < 4; i++)
				{
					if (pl.PlayerParty.instance().player((byte)i).isEnable())
					{
						string after = "";
						string arg = "";
						dgs.msg.CMessageSys.getInstance().changeValueFont(pl.PlayerParty.instance().player((byte)i).hp()
							.getNow(), out after);
						sprintf(out after, "%s", after);
						ushort num = 0;
						sprintf(out arg, "%s", after);
						num = (ushort)(-getStringWidth(after, 12));
						HpMngId_[i] = dgs.msg.CMessageSys.getInstance().Main().createMessage(arg, (ushort)(PlayerHpPosition(i).vx + num), (ushort)PlayerHpPosition(i).vy, dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_COMMON, dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8);
						dgs.DGSMessage dGSMessage = dgs.msg.CMessageSys.getInstance().Main().Message(HpMngId_[i]);
						dGSMessage.setDisplaySpeed(byte.MaxValue);
						dGSMessage.setDisplayWait(0);
						dGSMessage.setShadow(b: true);
					}
				}
			}

			public void releaseHp()
			{
				for (int i = 0; i < 4; i++)
				{
					if (HpMngId_[i] != -1)
					{
						dgs.msg.CMessageSys.getInstance().Main().releaseMessage(HpMngId_[i]);
					}
				}
			}

			public void showHp(bool flag)
			{
				for (int i = 0; i < 4; i++)
				{
					if (HpMngId_[i] != -1)
					{
						dgs.msg.CMessageSys.getInstance().Main().setVisibility(HpMngId_[i], flag);
					}
				}
			}

			public void updateHp(int i)
			{
				if (HpMngId_[i] != -1 && pl.PlayerParty.instance().player((byte)i).isEnable())
				{
					dgs.msg.CMessageSys.getInstance().Main().releaseMessage(HpMngId_[i]);
					string after = "";
					string arg = "";
					dgs.msg.CMessageSys.getInstance().changeValueFont(pl.PlayerParty.instance().player((byte)i).hp()
						.getNow(), out after);
					sprintf(out after, "%s", after);
					ushort num = 0;
					sprintf(out arg, "%s", after);
					num = (ushort)(-getStringWidth(after, 12));
					HpMngId_[i] = dgs.msg.CMessageSys.getInstance().Main().createMessage(arg, (ushort)(PlayerHpPosition(i).vx + num), (ushort)PlayerHpPosition(i).vy, dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_COMMON, dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8);
					dgs.DGSMessage dGSMessage = dgs.msg.CMessageSys.getInstance().Main().Message(HpMngId_[i]);
					dGSMessage.setDisplaySpeed(byte.MaxValue);
					dGSMessage.setDisplayWait(0);
					dGSMessage.setShadow(b: true);
					changeColor(i);
				}
			}

			public void createMaxHp()
			{
				for (int i = 0; i < 4; i++)
				{
					if (pl.PlayerParty.instance().player((byte)i).isEnable())
					{
						string after = "";
						string arg = "";
						dgs.msg.CMessageSys.getInstance().changeValueFont(pl.PlayerParty.instance().player((byte)i).hp()
							.getLimit(), out after);
						sprintf(out arg, " / %s", after);
						maxHpMngId_[i] = dgs.msg.CMessageSys.getInstance().Main().createMessage(arg, (ushort)PlayerMaxHpPosition(i).vx, (ushort)PlayerMaxHpPosition(i).vy, dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_COMMON, dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8);
						dgs.DGSMessage dGSMessage = dgs.msg.CMessageSys.getInstance().Main().Message(maxHpMngId_[i]);
						dGSMessage.setDisplaySpeed(byte.MaxValue);
						dGSMessage.setDisplayWait(0);
						dGSMessage.setShadow(b: true);
						dGSMessage.setStyle(520u);
					}
				}
			}

			public void releaseMaxHp()
			{
				for (int i = 0; i < 4; i++)
				{
					if (maxHpMngId_[i] != -1)
					{
						dgs.msg.CMessageSys.getInstance().Main().releaseMessage(maxHpMngId_[i]);
					}
				}
			}

			public void showMaxHp(bool flag)
			{
				for (int i = 0; i < 4; i++)
				{
					if (maxHpMngId_[i] != -1)
					{
						dgs.msg.CMessageSys.getInstance().Main().setVisibility(maxHpMngId_[i], flag);
					}
				}
			}

			public PlayerWindow()
			{
				show_ = true;
			}

			public int nowPlayer()
			{
				return nowPlayer_;
			}

			public void nowPlayer_set(int arg0)
			{
				nowPlayer_ = arg0;
			}
		}
	}
}
