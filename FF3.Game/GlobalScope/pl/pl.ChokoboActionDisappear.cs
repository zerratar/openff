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
	public static partial class pl
	{
		public class ChokoboActionDisappear : CPlayerVehicleAction
		{
			private CPlayerHuman pBoardHuman_;

			public override void start()
			{
				VehicleChokobo vehicleChokobo = static_cast<VehicleChokobo>(Player());
				vehicleChokobo.setMCLCol(b: false);
				vehicleChokobo.getColFlag_set(0);
				vehicleChokobo.startMotion(CHOKOBO_MOTIONNO_RUN, _Loop: true, 5u);
				vehicleChokobo.setAutoPilot(_AutoPilot: true);
				vehicleChokobo.setOperater(_Operater: false);
				vehicleChokobo.TurnSys().setStop(b: true);
				pBoardHuman_ = (CPlayerHuman)vehicleChokobo.getBoardPlayer();
				vehicleChokobo.setTarget(null);
				vehicleChokobo.setBoardPlayer(null);
				pBoardHuman_.setNextAct(0);
				pBoardHuman_.getParamObj_set(vehicleChokobo.getParamObj());
				pBoardHuman_.getPreParamObj_set(vehicleChokobo.getParamObj());
				pBoardHuman_.getParamObj().m_Pos.y = 0;
				pBoardHuman_.setAutoPilot(_AutoPilot: true);
				pBoardHuman_.InputPermission_set(arg0: false);
				pBoardHuman_.setOnVehicle(b: false);
				pBoardHuman_.setSucAlpha(100);
				pBoardHuman_.setAutoAlphaFrame(10);
				pBoardHuman_.setWorkAutoAlphaFrame(0);
				pBoardHuman_.setSucShadowAlpha(10);
				pBoardHuman_.setAutoShadowAlphaFrame(10);
				pBoardHuman_.setWorkAutoShadowAlphaFrame(0);
				pBoardHuman_.setShadowType((uint)pBoardHuman_.getShadowType());
				chr.CBaseCharacter.setLookIndex(0);
				wld.CWorldOutSideData.getInstance().PlayerData().setPlayCharacterIndex(0);
				MatrixSound.MtxSENDS_Play(1, 26, 192, 127);
				m_Counter = 0;
				wld.CWorldOutSideData.getInstance().MapData().setRideOnChokobo(b: false);
			}

			public override void update()
			{
				VehicleChokobo vehicleChokobo = static_cast<VehicleChokobo>(Player());
				m_Counter++;
				if (!wld.MapSound.canChangeBGM())
				{
					if (m_Counter >= 40)
					{
						vehicleChokobo.setNextAct(0);
					}
				}
				else if (!wld.MapSound.isPlaying() && m_Counter >= 40)
				{
					vehicleChokobo.setNextAct(0);
				}
			}

			public override void end()
			{
				VehicleChokobo vehicleChokobo = static_cast<VehicleChokobo>(Player());
				vehicleChokobo.returnToFieldBGM();
				VecFx32 position = new VecFx32(0, -4096000, 0);
				vehicleChokobo.setPosition(position);
				vehicleChokobo.setShadowAlpha(0);
				pBoardHuman_.setNextAct(8);
				pBoardHuman_.setMenuIcon(vehicleChokobo.getMenuIcon());
				pBoardHuman_.setCameraIcon(vehicleChokobo.getCameraIcon());
				pBoardHuman_.setTalkIcon(vehicleChokobo.getTalkIcon());
				vehicleChokobo.setMenuIcon(null);
				vehicleChokobo.setCameraIcon(null);
				vehicleChokobo.setTalkIcon(null);
			}
		}
	}
}
