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
	public static partial class wld
	{
		public class ChangePlayerDisplay
		{
			public void execute(CBaseSystem baseSys)
			{
				pl.CBasePlayer cBasePlayer = baseSys.PlayerMng().Player(0);
				if (cBasePlayer == null)
				{
					return;
				}
				for (int i = 0; 4 > i; i++)
				{
					if (pl.PlayerParty.instance().player((byte)i).isEnable() && !pl.PlayerParty.instance().player((byte)i).condition()
						.isDeath() && !pl.PlayerParty.instance().player((byte)i).condition()
						.isStone())
					{
						CWorldOutSideData.getInstance().PlayerData().setFrontPlayerID(pl.PlayerParty.instance().player((byte)i).playerId());
						break;
					}
				}
				int nowAct = cBasePlayer.getNowAct();
				chr.CHARA_OBJECT cHARA_OBJECT = new chr.CHARA_OBJECT(cBasePlayer.getParamObj());
				pl.CBasePlayer cBasePlayer2 = null;
				cBasePlayer.terminate();
				int num = baseSys.setupHero();
				cBasePlayer2 = baseSys.PlayerMng().Player(num);
				cBasePlayer2.into();
				cBasePlayer2.setMCLCol(b: true);
				cBasePlayer2.getColFlag_set(cBasePlayer.getColFlag());
				cBasePlayer2.getColType_set(cBasePlayer.getColType());
				cBasePlayer2.setAutoPilot(_AutoPilot: false);
				cBasePlayer2.setNextAct(nowAct);
				cBasePlayer2.getParamObj_set(cHARA_OBJECT);
				cBasePlayer2.setPosition(cHARA_OBJECT.m_Pos);
				cBasePlayer2.setRotation(cHARA_OBJECT.m_Rot);
				cBasePlayer2.setTargetDirectionFromRotation();
				cBasePlayer2.getTargetDirection();
				VecFx32 targetDirection = new VecFx32(0, 0, 0);
				cBasePlayer2.setDirection(cBasePlayer2.getTargetDirection());
				cBasePlayer2.setTargetDirection(targetDirection);
				cBasePlayer2.getPreParamObj_set(cBasePlayer2.getParamObj());
				CBaseSystem.WORLD_MODE wORLD_MODE = baseSys.Mode();
				if (wORLD_MODE == CBaseSystem.WORLD_MODE.WORLD_MODE_MENU)
				{
					wORLD_MODE = baseSys.PreviousMode();
				}
				switch (wORLD_MODE)
				{
				case CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD:
					cBasePlayer2.PlayerMoveType_set(pl.PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_HERO_FIELD);
					break;
				case CBaseSystem.WORLD_MODE.WORLD_MODE_TOWN:
					cBasePlayer2.PlayerMoveType_set(pl.PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_HERO_TOWN);
					break;
				}
				chr.CBaseCharacter.setLookIndex(num);
				CWorldOutSideData.getInstance().PlayerData().setPlayCharacterIndex(num);
				if (pl.PlayerParty.instance().npc().isEnable() && pl.PlayerParty.instance().npc().npcId() == baseSys.npcId())
				{
					baseSys.PlayerMng().PlayerHuman(baseSys.npcEntryId()).NPCAiManager()
						.NPC()
						.setLookPlayer(baseSys.PlayerMng().Player(num));
				}
				if (baseSys.Mode() == CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD || (baseSys.Mode() == CBaseSystem.WORLD_MODE.WORLD_MODE_MENU && baseSys.PreviousMode() == CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD))
				{
					if (evt.CEventManager.getInstance().FlagMng().get(0u, (uint)evt.EVENT_VEHICLE_FLAG[0]) == 1)
					{
						cBasePlayer2.getColFlag_not_and(32);
					}
				}
				else if (baseSys.Mode() == CBaseSystem.WORLD_MODE.WORLD_MODE_TOWN || (baseSys.Mode() == CBaseSystem.WORLD_MODE.WORLD_MODE_MENU && baseSys.PreviousMode() == CBaseSystem.WORLD_MODE.WORLD_MODE_TOWN))
				{
					cBasePlayer2.getColFlag_not_and(64);
					cBasePlayer2.getColFlag_not_and(128);
					cBasePlayer2.getColFlag_not_and(256);
					cBasePlayer2.getColFlag_not_and(512);
				}
				pl.CPlayerHuman cPlayerHuman = baseSys.PlayerMng().getNpc();
				if (cPlayerHuman != null)
				{
					cPlayerHuman.setMCLCol(b: true);
					cPlayerHuman.getColFlag_not_and(2);
					cPlayerHuman.getColFlag_not_and(4);
					cPlayerHuman.getColFlag_or(8);
					cPlayerHuman.getColFlag_not_and(16);
					cPlayerHuman.getColRadius_set(0);
					cPlayerHuman.getCckRadius_set(0);
				}
				int num2 = 0;
				for (num2 = 0; (long)num2 < 4L && baseSys.PlayerMng().PlayerVehicle(num2).getBoardPlayer() == null; num2++)
				{
				}
				if ((long)num2 >= 4L)
				{
					return;
				}
				pl.CPlayerVehicle cPlayerVehicle = baseSys.PlayerMng().PlayerVehicle(num2);
				if (cPlayerVehicle != null)
				{
					chr.CBaseCharacter.setLookIndex((int)((long)num2 + 24L));
					CWorldOutSideData.getInstance().PlayerData().setPlayCharacterIndex((int)((long)num2 + 24L));
					cBasePlayer2.setTarget(cPlayerVehicle);
					cBasePlayer2.setMCLCol(b: false);
					cBasePlayer2.getColFlag_not_and(2);
					cBasePlayer2.setTransparency(0);
					cBasePlayer2.setShadowAlpha(0);
					pl.CPlayerHuman cPlayerHuman2 = (pl.CPlayerHuman)cBasePlayer2;
					cPlayerHuman2.setMenuIcon(null);
					cPlayerHuman2.setCameraIcon(null);
					cPlayerHuman2.setTalkIcon(null);
					cPlayerHuman2.setOnVehicle(b: true);
					static_cast<pl.CPlayerCharacter>(cBasePlayer2).InputPermission_set(arg0: false);
					if (cPlayerHuman != null)
					{
						cPlayerHuman.setMCLCol(b: false);
						cPlayerHuman.getColFlag_not_and(2);
						cPlayerHuman.getColFlag_not_and(4);
						cPlayerHuman.setTransparency(0);
						cPlayerHuman.setShadowAlpha(0);
						cPlayerHuman.setAutoPilot(_AutoPilot: true);
						cPlayerHuman.InputPermission_set(arg0: false);
					}
				}
			}
		}
	}
}
