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
	public static partial class wld
	{
							public class CStateMenuMove : CBaseState
							{
								public override void start(CBaseSystem _sys)
								{
									OS_AssignBackButton(1);
									checkFrontPlayerCondition();
								}

								public override void update(CBaseSystem _sys)
								{
									OS_AssignBackButton(1);
									int index = 0;
									bool flag = false;
									int frontPlayerID = CWorldOutSideData.getInstance().PlayerData().getFrontPlayerID();
									if (old_lillput != pl.PlayerParty.instance().playerForId((byte)frontPlayerID).condition()
										.isLilliput() && _sys.canChangeLilliput())
									{
										if (!old_lillput)
										{
											_sys.PlayerMng().PlayerHuman(index).changeLilliput(frontPlayerID, -1);
											flag = true;
										}
										else
										{
											_sys.PlayerMng().PlayerHuman(index).returnHuman(model_change: false, frontPlayerID, -1);
											flag = true;
										}
									}
									else if (old_frog != pl.PlayerParty.instance().playerForId((byte)frontPlayerID).condition()
										.isFrog())
									{
										if (!old_frog)
										{
											_sys.PlayerMng().PlayerHuman(index).changeFrog(frontPlayerID, -1);
											flag = true;
										}
										else
										{
											_sys.PlayerMng().PlayerHuman(index).returnHuman(model_change: true, frontPlayerID, -1);
											flag = true;
										}
									}
									checkFrontPlayerCondition();
									int num = _sys.npcEntryId();
									bool flag2 = false;
									pl.CPlayerHuman cPlayerHuman = null;
									if (-1 != num && 3 != pl.PlayerParty.instance().npc().npcId())
									{
										cPlayerHuman = _sys.PlayerMng().PlayerHuman(_sys.npcEntryId());
										if (pl.PlayerParty.instance().isFrogAll())
										{
											if (!pl.PlayerParty.instance().npc().isFrog())
											{
												cPlayerHuman.changeFrogForNpc();
												flag2 = true;
											}
										}
										else if (pl.PlayerParty.instance().isLilliputAll() && _sys.canChangeLilliput())
										{
											if (!pl.PlayerParty.instance().npc().isLilliput())
											{
												cPlayerHuman.changeLilliputForNpc();
												flag2 = true;
											}
										}
										else if (pl.PlayerParty.instance().npc().isFrog() || pl.PlayerParty.instance().npc().isLilliput())
										{
											cPlayerHuman.returnHumanForNpc(null);
											flag2 = true;
										}
									}
									int i;
									for (i = 0; (long)i < 4L && _sys.PlayerMng().PlayerVehicle(i).getBoardPlayer() == null; i++)
									{
									}
									if ((long)i < 4L)
									{
										if (flag)
										{
											_sys.PlayerMng().PlayerHuman(index).setTransparency(0);
											_sys.PlayerMng().PlayerHuman(index).setShadowAlpha(0);
										}
										if (cPlayerHuman != null && flag2)
										{
											cPlayerHuman.setTransparency(0);
											cPlayerHuman.setShadowAlpha(0);
										}
									}
									wmenu.CWMenuManager.Instance().run();
									if (!wmenu.CWMenuManager.Instance().GetMyState())
									{
										wmenu.CWMenuManager.Instance().SetMyState(val: true);
										setPhase(PHASE.END);
									}
								}

								public override void end(CBaseSystem _sys)
								{
									OS_AssignBackButton(1);
									checkFrontPlayerCondition();
									_sys.setState(CBaseSystem.WORLD_STATE.WORLD_STATE_END);
								}

								public override bool canExecuteEvent(CBaseSystem arg0)
								{
									return false;
								}
							}
	}
}
