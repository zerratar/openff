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
	public static partial class pl
	{
		public class CBasePlayer : chr.CCharacterEureka
		{
			public enum OPERATER
			{
				NPC,
				PC,
				OPERATER_MAX
			}

			public enum CBP_FLAG
			{
				NPC_NOT_TURN_TALKED = 1,
				NPC_RETURN_TALK_ENDS = 2,
				UNDEFINED2 = 4,
				UNDEFINED3 = 8,
				ENVIRONMENT_DAMAGING = 0x10,
				UNDEFINED5 = 0x20,
				UNDEFINED6 = 0x40,
				UNDEFINED7 = 0x80
			}

			public const OPERATER NPC = OPERATER.NPC;

			public const OPERATER PC = OPERATER.PC;

			public const OPERATER OPERATER_MAX = OPERATER.OPERATER_MAX;

			public const CBP_FLAG NPC_NOT_TURN_TALKED = CBP_FLAG.NPC_NOT_TURN_TALKED;

			public const CBP_FLAG NPC_RETURN_TALK_ENDS = CBP_FLAG.NPC_RETURN_TALK_ENDS;

			public const CBP_FLAG UNDEFINED2 = CBP_FLAG.UNDEFINED2;

			public const CBP_FLAG UNDEFINED3 = CBP_FLAG.UNDEFINED3;

			public const CBP_FLAG ENVIRONMENT_DAMAGING = CBP_FLAG.ENVIRONMENT_DAMAGING;

			public const CBP_FLAG UNDEFINED5 = CBP_FLAG.UNDEFINED5;

			public const CBP_FLAG UNDEFINED6 = CBP_FLAG.UNDEFINED6;

			public const CBP_FLAG UNDEFINED7 = CBP_FLAG.UNDEFINED7;

			protected VecFx32 m_MainPos = new VecFx32();

			protected act.CActionManager m_ActionMng = new act.CActionManager();

			protected PLAYER_MOVE_TYPE m_PlayerMoveType;

			protected NPC_RANDOM_MOVE_TYPE m_NPCRandomMoveType;

			protected NPC_AUTO_FOLLOW_TYPE m_NPCAutoFollowType;

			protected sbyte m_GeneralFlag;

			public override void update()
			{
				base.update();
			}

			public override void reset()
			{
				base.reset();
				VEC_Set(m_MainPos, 0, 0, 0);
			}

			public void setAction(act.CBaseAction _Action)
			{
				chr.CCharacterEureka chara = static_cast<chr.CCharacterEureka>(this);
				m_ActionMng.setAction(chara, _Action);
			}

			public void flagOn(CBP_FLAG f)
			{
				m_GeneralFlag |= (sbyte)f;
			}

			public void flagOff(CBP_FLAG f)
			{
				m_GeneralFlag &= (sbyte)(~f);
			}

			public bool checkSightAngle(chr.CBaseCharacter pTarget)
			{
				VecFx32 pl_reuse_v = pl_reuse_v0;
				VecFx32 pl_reuse_v2 = pl_reuse_v1;
				pl_reuse_v.set(getPosition().x, 0, getPosition().z);
				pl_reuse_v2.set(pTarget.getPosition().x, 0, pTarget.getPosition().z);
				VecFx32 pl_reuse_v3 = pl.pl_reuse_v2;
				VEC_Subtract(pl_reuse_v2, pl_reuse_v, pl_reuse_v3);
				VEC_Normalize(pl_reuse_v3, pl_reuse_v3);
				VecFx32 pl_reuse_v4 = pl.pl_reuse_v3;
				pl_reuse_v4.copy(getDirection());
				VEC_Normalize(pl_reuse_v4, pl_reuse_v4);
				return VEC_DotProduct(pl_reuse_v3, pl_reuse_v4) >= 0;
			}

			public virtual int getBattleMapNo()
			{
				if (m_LandFormIndex < 1)
				{
					m_LandFormIndex = 1;
				}
				if (m_LandFormIndex > 12)
				{
					m_LandFormIndex = 12;
				}
				return map.CMapParameterManager.Instance().MapLandFormParameter(0).BattleFieldIndex(m_LandFormIndex - 1);
			}

			public virtual int getMonsterPartyGroupNo()
			{
				return monPartyGroupNo_;
			}

			public CBasePlayer()
			{
				m_PlayerMoveType = PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_ERR;
				m_NPCRandomMoveType = NPC_RANDOM_MOVE_TYPE.NPC_RANDOM_MOVE_TYPE_ERR;
				m_NPCAutoFollowType = NPC_AUTO_FOLLOW_TYPE.NPC_AUTO_FOLLOW_TYPE_ERR;
				VEC_Set(m_MainPos, 0, 0, 0);
				m_GeneralFlag = 0;
			}

			public void setMainPos(VecFx32 _MainPos)
			{
				m_MainPos.copy(_MainPos);
			}

			public VecFx32 MainPos()
			{
				return m_MainPos;
			}

			public PLAYER_MOVE_TYPE getPlayerMoveType()
			{
				return m_PlayerMoveType;
			}

			public PLAYER_MOVE_TYPE PlayerMoveType()
			{
				return m_PlayerMoveType;
			}

			public void PlayerMoveType_set(PLAYER_MOVE_TYPE arg0)
			{
				m_PlayerMoveType = arg0;
			}

			public NPC_RANDOM_MOVE_TYPE getNPCRandomMoveType()
			{
				return m_NPCRandomMoveType;
			}

			public NPC_RANDOM_MOVE_TYPE NPCRandomMoveType()
			{
				return m_NPCRandomMoveType;
			}

			public void NPCRandomMoveType_set(NPC_RANDOM_MOVE_TYPE arg0)
			{
				m_NPCRandomMoveType = arg0;
			}

			public NPC_AUTO_FOLLOW_TYPE getNPCAutoFollowType()
			{
				return m_NPCAutoFollowType;
			}

			public NPC_AUTO_FOLLOW_TYPE NPCAutoFollowType()
			{
				return m_NPCAutoFollowType;
			}

			public void NPCAutoFollowType_set(NPC_AUTO_FOLLOW_TYPE arg0)
			{
				m_NPCAutoFollowType = arg0;
			}

			public bool flagCheck(CBP_FLAG f)
			{
				return (m_GeneralFlag & (sbyte)f) != 0;
			}

			public virtual bool canEncount()
			{
				return false;
			}

			public virtual bool canOpenMenu()
			{
				return true;
			}

			public virtual bool checkJump12()
			{
				return false;
			}

			public virtual void checkCollisionCharacter(chr.CCharacterEureka _Target)
			{
			}
		}
	}
}
