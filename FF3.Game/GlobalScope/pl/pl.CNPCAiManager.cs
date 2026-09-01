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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class pl
	{
		public class CNPCAiManager
		{
			public enum AI_KIND
			{
				AI_KIND_ERR = -1,
				AI_KIND_DEFAULT,
				AI_KIND_RANDOM_MOVE,
				AI_KIND_AUTO_FOLLOW,
				AI_KIND_MAX
			}

			public const AI_KIND AI_KIND_ERR = AI_KIND.AI_KIND_ERR;

			public const AI_KIND AI_KIND_DEFAULT = AI_KIND.AI_KIND_DEFAULT;

			public const AI_KIND AI_KIND_RANDOM_MOVE = AI_KIND.AI_KIND_RANDOM_MOVE;

			public const AI_KIND AI_KIND_AUTO_FOLLOW = AI_KIND.AI_KIND_AUTO_FOLLOW;

			public const AI_KIND AI_KIND_MAX = AI_KIND.AI_KIND_MAX;

			private AI_KIND m_AiKind;

			private CBaseNPCAi[] m_BaseNPCAi = new CBaseNPCAi[3];

			private CNPCAiDefault m_NPCAiDefault = new CNPCAiDefault();

			private CNPCAiRandomMove m_NPCAiRandomMove = new CNPCAiRandomMove();

			private CNPCAiAutoFollow m_NPCAiAutoFollow = new CNPCAiAutoFollow();

			public void initialize(CBasePlayer _pPlayer)
			{
				m_AiKind = AI_KIND.AI_KIND_ERR;
				m_NPCAiDefault.initialize(_pPlayer);
				m_NPCAiRandomMove.initialize(_pPlayer);
				m_NPCAiAutoFollow.initialize(_pPlayer);
				for (int i = 0; i < 3; i++)
				{
					m_BaseNPCAi[i] = null;
				}
				setUpRegister();
			}

			public void execute()
			{
				if (m_AiKind != AI_KIND.AI_KIND_ERR)
				{
					m_BaseNPCAi[(int)m_AiKind].execute();
				}
			}

			public void terminate()
			{
				for (int i = 0; i < 3; i++)
				{
					if (m_BaseNPCAi[i] != null)
					{
						m_BaseNPCAi[i].terminate();
					}
				}
			}

			public void setUpRegister()
			{
				m_BaseNPCAi[0] = m_NPCAiDefault;
				m_BaseNPCAi[1] = m_NPCAiRandomMove;
				m_BaseNPCAi[2] = m_NPCAiAutoFollow;
			}

			public CNPCAiManager()
			{
				m_AiKind = AI_KIND.AI_KIND_ERR;
			}

			public AI_KIND getAiKind()
			{
				return m_AiKind;
			}

			public AI_KIND AiKind()
			{
				return m_AiKind;
			}

			public void AiKind_set(AI_KIND arg0)
			{
				m_AiKind = arg0;
			}

			public CBaseNPCAi NPC()
			{
				return m_BaseNPCAi[(int)m_AiKind];
			}

			public void reset()
			{
				if (m_AiKind != AI_KIND.AI_KIND_ERR)
				{
					m_NPCAiDefault.reset();
					m_NPCAiRandomMove.reset();
					m_NPCAiAutoFollow.reset();
				}
			}
		}
	}
}
