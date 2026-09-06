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
		public class MBSelectJobParam : MenuBehavior
		{
			public enum CELL_LIST
			{
				CELL_LIST_BOTTOM,
				CELL_LIST_VAR,
				CELL_LIST_FRAME,
				CELL_LIST_MAX
			}

			public enum MESSAGE_LIST
			{
				MESSAGE_LIST_JOB_NAME,
				MESSAGE_LIST_JOB_LEVEL,
				MESSAGE_LIST_JOB_VALUE,
				MESSAGE_LIST_JOB_ABILITY,
				MESSAGE_LIST_JOB_ABILITY_1,
				MESSAGE_LIST_JOB_ABILITY_2,
				MESSAGE_LIST_JOB_ABILITY_3,
				MESSAGE_LIST_JOB_ABILITY_4,
				MESSAGE_LIST_MAX
			}

			public const CELL_LIST CELL_LIST_BOTTOM = CELL_LIST.CELL_LIST_BOTTOM;

			public const CELL_LIST CELL_LIST_VAR = CELL_LIST.CELL_LIST_VAR;

			public const CELL_LIST CELL_LIST_FRAME = CELL_LIST.CELL_LIST_FRAME;

			public const CELL_LIST CELL_LIST_MAX = CELL_LIST.CELL_LIST_MAX;

			public const MESSAGE_LIST MESSAGE_LIST_JOB_NAME = MESSAGE_LIST.MESSAGE_LIST_JOB_NAME;

			public const MESSAGE_LIST MESSAGE_LIST_JOB_LEVEL = MESSAGE_LIST.MESSAGE_LIST_JOB_LEVEL;

			public const MESSAGE_LIST MESSAGE_LIST_JOB_VALUE = MESSAGE_LIST.MESSAGE_LIST_JOB_VALUE;

			public const MESSAGE_LIST MESSAGE_LIST_JOB_ABILITY = MESSAGE_LIST.MESSAGE_LIST_JOB_ABILITY;

			public const MESSAGE_LIST MESSAGE_LIST_JOB_ABILITY_1 = MESSAGE_LIST.MESSAGE_LIST_JOB_ABILITY_1;

			public const MESSAGE_LIST MESSAGE_LIST_JOB_ABILITY_2 = MESSAGE_LIST.MESSAGE_LIST_JOB_ABILITY_2;

			public const MESSAGE_LIST MESSAGE_LIST_JOB_ABILITY_3 = MESSAGE_LIST.MESSAGE_LIST_JOB_ABILITY_3;

			public const MESSAGE_LIST MESSAGE_LIST_JOB_ABILITY_4 = MESSAGE_LIST.MESSAGE_LIST_JOB_ABILITY_4;

			public const MESSAGE_LIST MESSAGE_LIST_MAX = MESSAGE_LIST.MESSAGE_LIST_MAX;

			public static dgs.UniqueNumber MBSelectJobParam_UN = new dgs.UniqueNumber();

			private int m_NowSelectJob;

			private sys2d.Cell[] m_Cell = new sys2d.Cell[3];

			private dgs.DGSMessage[] m_pMessage = new dgs.DGSMessage[8];

			private Medget m_M;

			public MBSelectJobParam()
			{
				m_NowSelectJob = 0;
				for (int i = 0; i < 8; i++)
				{
					m_pMessage[i] = null;
				}
				m_M = null;
			}

			~MBSelectJobParam()
			{
				releaseAll();
			}

			public override void bmInitialize(Medget M)
			{
				m_NowSelectJob = MenuManager.getSingleton().GetTargetItemNo();
				for (int i = 0; i < 8; i++)
				{
					m_pMessage[i] = null;
				}
				m_M = null;
				releaseAll();
				m_M = M;
				setJobName();
				setJobLevelTitle();
				setJobLevelValue();
				setJobLevelBar();
				setJobAbilityTitle();
				setJobAbilityItem();
				updateJobLevelBar();
			}

			public override void bmBehave(Medget M)
			{
				int nowSelectJob = m_NowSelectJob;
				m_NowSelectJob = MenuManager.getSingleton().GetTargetItemNo();
				if (nowSelectJob != m_NowSelectJob)
				{
					releaseJobName();
					releaseJobLevelValue();
					releaseJobAbilityItem();
					setJobName();
					setJobLevelValue();
					setJobAbilityItem();
					updateJobLevelBar();
				}
			}

			public override void bmFinalize(Medget M)
			{
				releaseAll();
			}

			public void setJobName()
			{
				Medget medget = null;
				medget = m_M.childNode();
				dgs.DGSMessageManager dGSMessageManager = null;
				dGSMessageManager = ((medget.display() == 1) ? dgs.msg.CMessageSys.getInstance().Main() : dgs.msg.CMessageSys.getInstance().Sub());
				m_pMessage[0] = dGSMessageManager.createMessage((uint)(FIRST_JOBNAME_ID + m_NowSelectJob), dgs.INVALID_MSDHANDLE, 1);
				if (m_pMessage[0] != null)
				{
					m_pMessage[0].setDisplaySpeed(byte.MaxValue);
					m_pMessage[0].setDisplayWait(0);
					m_pMessage[0].setPosition(medget.x(), medget.y(), erase: true);
				}
			}

			public void setJobLevelTitle()
			{
				dgs.DGSMessageManager dGSMessageManager = null;
				Medget medget = null;
				medget = m_M.childNode().nextSibling();
				dGSMessageManager = ((medget.display() == 1) ? dgs.msg.CMessageSys.getInstance().Main() : dgs.msg.CMessageSys.getInstance().Sub());
				m_pMessage[1] = dGSMessageManager.createMessage("じゅくれんど：", 1);
				if (m_pMessage[1] != null)
				{
					m_pMessage[1].setDisplaySpeed(byte.MaxValue);
					m_pMessage[1].setDisplayWait(0);
					m_pMessage[1].setPosition(medget.x(), medget.y(), erase: true);
				}
			}

			public void setJobLevelValue()
			{
				dgs.DGSMessageManager dGSMessageManager = null;
				Medget medget = null;
				medget = m_M.childNode().nextSibling().childNode();
				dGSMessageManager = ((medget.display() == 1) ? dgs.msg.CMessageSys.getInstance().Main() : dgs.msg.CMessageSys.getInstance().Sub());
				int num = pl.PlayerParty.instance().player(0).jobManager()
					.job(static_cast<pl.JOB_TYPE>(m_NowSelectJob))
					.skill()
					.skillLevel()
					.get();
				dgs.msg.CMessageSys.getInstance().changeValueFont(num / 10, out var after);
				dgs.msg.CMessageSys.getInstance().changeValueFont(num % 10, out var after2);
				sprintf(out after, "%s%s", after, after2);
				m_pMessage[2] = dGSMessageManager.createMessage(after, 1);
				if (m_pMessage[2] != null)
				{
					m_pMessage[2].setDisplaySpeed(byte.MaxValue);
					m_pMessage[2].setDisplayWait(0);
					m_pMessage[2].setPosition(medget.x(), medget.y(), erase: true);
				}
			}

			public void setJobLevelBar()
			{
				m_M.childNode().nextSibling().childNode()
					.childNode();
			}

			public void setJobAbilityTitle()
			{
				Medget medget = null;
				medget = m_M.childNode().nextSibling().nextSibling();
				dgs.DGSMessageManager dGSMessageManager = null;
				dGSMessageManager = ((medget.display() == 1) ? dgs.msg.CMessageSys.getInstance().Main() : dgs.msg.CMessageSys.getInstance().Sub());
				m_pMessage[3] = dGSMessageManager.createMessage("アビリティ", 1);
				if (m_pMessage[3] != null)
				{
					m_pMessage[3].setDisplaySpeed(byte.MaxValue);
					m_pMessage[3].setDisplayWait(0);
					m_pMessage[3].setPosition(medget.x(), medget.y(), erase: true);
				}
			}

			public void setJobAbilityItem()
			{
				Medget medget = null;
				for (int i = 4; i < 8; i++)
				{
					if (i == 4)
					{
						medget = m_M.childNode().nextSibling().nextSibling()
							.childNode();
					}
					if (i == 5)
					{
						medget = m_M.childNode().nextSibling().nextSibling()
							.childNode()
							.nextSibling();
					}
					if (i == 6)
					{
						medget = m_M.childNode().nextSibling().nextSibling()
							.childNode()
							.nextSibling()
							.nextSibling();
					}
					if (i == 7)
					{
						medget = m_M.childNode().nextSibling().nextSibling()
							.childNode()
							.nextSibling()
							.nextSibling()
							.nextSibling();
					}
					dgs.DGSMessageManager dGSMessageManager = null;
					dGSMessageManager = ((medget.display() == 1) ? dgs.msg.CMessageSys.getInstance().Main() : dgs.msg.CMessageSys.getInstance().Sub());
					m_pMessage[i] = dGSMessageManager.createMessage("ダミー１２３４５", 1);
					if (m_pMessage[i] != null)
					{
						m_pMessage[i].setDisplaySpeed(byte.MaxValue);
						m_pMessage[i].setDisplayWait(0);
						m_pMessage[i].setPosition(medget.x(), medget.y(), erase: true);
					}
				}
			}

			public void updateJobLevelBar()
			{
				NNSG2dFVec2 nNSG2dFVec = new NNSG2dFVec2(4096, 4096);
				for (int i = 0; i < 23; i++)
				{
					pl.PlayerParty.instance().player(0).jobManager()
						.job(static_cast<pl.JOB_TYPE>(i))
						.skill()
						.skillExp()
						.set((byte)(10 + i * 10));
				}
				int num = pl.PlayerParty.instance().player(0).jobManager()
					.job(static_cast<pl.JOB_TYPE>(m_NowSelectJob))
					.skill()
					.skillExp()
					.get();
				int num2 = 255;
				if (num == 0)
				{
					nNSG2dFVec.x = 0;
				}
				else
				{
					nNSG2dFVec.x = FX_Div(4096, FX_Div(num2 << 12, num << 12));
				}
				m_Cell[1].SetScale(nNSG2dFVec);
			}

			public void releaseAll()
			{
				releaseJobName();
				releaseJobLevelTitle();
				releaseJobLevelValue();
				releaseJobLevelBar();
				releaseJobAbilityTitle();
				releaseJobAbilityItem();
			}

			public void releaseJobName()
			{
				if (m_pMessage[0] != null)
				{
					m_pMessage[0].release();
					m_pMessage[0] = null;
				}
			}

			public void releaseJobLevelTitle()
			{
				if (m_pMessage[1] != null)
				{
					m_pMessage[1].release();
					m_pMessage[1] = null;
				}
			}

			public void releaseJobLevelValue()
			{
				if (m_pMessage[2] != null)
				{
					m_pMessage[2].release();
					m_pMessage[2] = null;
				}
			}

			public void releaseJobLevelBar()
			{
				for (int i = 0; i < 3; i++)
				{
					m_Cell[i].Release();
					sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(m_Cell[i]);
				}
			}

			public void releaseJobAbilityTitle()
			{
				if (m_pMessage[3] != null)
				{
					m_pMessage[3].release();
					m_pMessage[3] = null;
				}
			}

			public void releaseJobAbilityItem()
			{
				for (int i = 4; i < 8; i++)
				{
					if (m_pMessage[i] != null)
					{
						m_pMessage[i].release();
					}
					m_pMessage[i] = null;
				}
			}

			public new static int classIdentifier()
			{
				return MBSelectJobParam_UN.number();
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
