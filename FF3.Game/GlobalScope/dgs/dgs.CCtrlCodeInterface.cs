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
	public static partial class dgs
	{
		public class CCtrlCodeInterface
		{
			public static CCtrlCodeInterface m_Instance = new CCtrlCodeInterface();

			private bool m_FontType;

			private int m_PlayerId;

			private int m_Gold;

			private int m_Exp;

			private int m_ItemId;

			private int m_MaxHp;

			private int m_Hp;

			private int m_InnPrice;

			private int m_jobPenalty;

			private int m_JobMessageID;

			private int m_Slot;

			private string m_FriendName;

			private string m_Argument1;

			private string m_Argument2;

			private string m_Argument3;

			public CCtrlCodeInterface()
			{
				m_FontType = false;
				m_Gold = 0;
				m_Exp = 0;
			}

			public static CCtrlCodeInterface instance()
			{
				return m_Instance;
			}

			public void clear()
			{
				m_FontType = false;
				m_PlayerId = 0;
				m_Gold = 0;
				m_Exp = 0;
				m_ItemId = 0;
				m_MaxHp = 0;
				m_Hp = 0;
				m_InnPrice = 0;
				m_jobPenalty = 0;
				m_JobMessageID = 0;
				m_Slot = 0;
			}

			public void setFontSize(bool _FontType)
			{
				m_FontType = _FontType;
			}

			public void setPlayerId(int _PlayerId)
			{
				m_PlayerId = _PlayerId;
			}

			public void setGold(int _Gold)
			{
				m_Gold = _Gold;
			}

			public void setExp(int _Exp)
			{
				m_Exp = _Exp;
			}

			public void setItemId(int _ItemId)
			{
				m_ItemId = _ItemId;
			}

			public void setMaxHp(int maxHp)
			{
				m_MaxHp = maxHp;
			}

			public void setHp(int hp)
			{
				m_Hp = hp;
			}

			public void setInnPrice(int price)
			{
				m_InnPrice = price;
			}

			public void setJobPenalty(int penalty)
			{
				m_jobPenalty = penalty;
			}

			public void setJobMessageID(int JobMessageID)
			{
				m_JobMessageID = JobMessageID;
			}

			public void setSlot(int Slot)
			{
				m_Slot = Slot;
			}

			public void setFriendName(string name)
			{
				strncpy(out m_FriendName, name, EXPANDED_MARGIN);
			}

			public void setArgument1(string arg)
			{
				strncpy(out m_Argument1, arg, EXPANDED_MARGIN);
			}

			public bool isFontSize()
			{
				return m_FontType;
			}

			public int getPlayerId()
			{
				return m_PlayerId;
			}

			public int getGold()
			{
				return m_Gold;
			}

			public int getExp()
			{
				return m_Exp;
			}

			public int getItemId()
			{
				return m_ItemId;
			}

			public int getMaxHp()
			{
				return m_MaxHp;
			}

			public int getHp()
			{
				return m_Hp;
			}

			public int getInnPrice()
			{
				return m_InnPrice;
			}

			public int getJobPenalty()
			{
				return m_jobPenalty;
			}

			public int getJobMessageID()
			{
				return m_JobMessageID;
			}

			public int getSlot()
			{
				return m_Slot;
			}

			public string getFriendName()
			{
				return m_FriendName;
			}

			public string getArgument1()
			{
				return m_Argument1;
			}
		}
	}
}
