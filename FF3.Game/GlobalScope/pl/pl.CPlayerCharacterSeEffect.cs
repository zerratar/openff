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
		public class CPlayerCharacterSeEffect
		{
			public enum SE_EFFECT_TYPE
			{
				SE_EFFECT_TYPE_WAIT,
				SE_EFFECT_TYPE_WALK,
				SE_EFFECT_TYPE_RUN,
				SE_EFFECT_TYPE_MAX
			}

			private int m_PlayIntervalFrame;

			public void initialize()
			{
				m_PlayIntervalFrame = 0;
			}

			public void execute(CBasePlayer player, int effect_add_y)
			{
				if (player.getLandFormIndex() < 0 || chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_VEHICLE == player.CharaKind() || chr.CHARACTER_KIND.CHARACTER_KIND_MAP_OBJECT == player.CharaKind())
				{
					return;
				}
				PLAYER_MOVE_TYPE playerMoveType = player.getPlayerMoveType();
				short land_form = (short)(map.CMapParameterManager.Instance().MapLandFormParameter(0).LandAttr(player.getLandFormIndex() - 1) - 1);
				CPlayerHuman cPlayerHuman = static_cast<CPlayerHuman>(player);
				int num = 40;
				if (cPlayerHuman.isOperater() || CNPCAiManager.AI_KIND.AI_KIND_AUTO_FOLLOW == cPlayerHuman.NPCAiManager().AiKind())
				{
					if (1 == player.getNowAct())
					{
						num = 10;
					}
					else if (2 == player.getNowAct())
					{
						num = 5;
					}
				}
				if (++m_PlayIntervalFrame >= num)
				{
					m_PlayIntervalFrame = 0;
					int num2 = playLandFormWaitEffect(land_form);
					if (num2 != -1)
					{
						VecFx32 vecFx = new VecFx32(player.getPosition());
						vecFx.y += effect_add_y;
						eff.CEffectMng.instance().setPosition(num2, vecFx);
					}
				}
				if (!player.isOperater())
				{
					return;
				}
				int currentFrame = (int)player.getCurrentFrame();
				if (player.CharaKind() != chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_HUMAN)
				{
					return;
				}
				if (player.getNowAct() == 1)
				{
					if (currentFrame == CPlayerWorldParameterManager.Instance().PlayerWorldSeEffectPlayParameter((int)playerMoveType).PlayMoveFrame(0) || currentFrame == CPlayerWorldParameterManager.Instance().PlayerWorldSeEffectPlayParameter((int)playerMoveType).PlayMoveFrame(1))
					{
						playLandFormFeetSE(land_form);
					}
				}
				else if (player.getNowAct() == 2 && (currentFrame == CPlayerWorldParameterManager.Instance().PlayerWorldSeEffectPlayParameter((int)playerMoveType).PlaySpecialMoveFrame(0) || currentFrame == CPlayerWorldParameterManager.Instance().PlayerWorldSeEffectPlayParameter((int)playerMoveType).PlaySpecialMoveFrame(1)))
				{
					playLandFormFeetSE(land_form);
				}
			}

			public void terminate()
			{
				initialize();
			}

			public MatrixSound.MtxSEHandle playLandFormFeetSE(short land_form)
			{
				if (land_form != -1)
				{
					int num = CPlayerWorldParameterManager.Instance().PlayerWorldSeEffectMapParameter(land_form).SeNumber();
					if (num == -1)
					{
						return null;
					}
					return MatrixSound.MtxSENDS_Play(4, num, 192, 127);
				}
				return null;
			}

			public int playLandFormWaitEffect(short land_form)
			{
				if (land_form == -1)
				{
					return -1;
				}
				int num = CPlayerWorldParameterManager.Instance().PlayerWorldSeEffectMapParameter(land_form).WaitEffectCategory();
				int num2 = CPlayerWorldParameterManager.Instance().PlayerWorldSeEffectMapParameter(land_form).WaitEffectNumber();
				if (num == -1)
				{
					return -1;
				}
				if (num2 == -1)
				{
					return -1;
				}
				return eff.CEffectMng.instance().create(num, num2);
			}

			public int playLandFormMoveEffect(short land_form)
			{
				if (land_form == -1)
				{
					return -1;
				}
				int num = CPlayerWorldParameterManager.Instance().PlayerWorldSeEffectMapParameter(land_form).MoveEffectCategory();
				int num2 = CPlayerWorldParameterManager.Instance().PlayerWorldSeEffectMapParameter(land_form).MoveEffectNumber();
				if (num == -1)
				{
					return -1;
				}
				if (num2 == -1)
				{
					return -1;
				}
				return eff.CEffectMng.instance().create(num, num2);
			}
		}
	}
}
