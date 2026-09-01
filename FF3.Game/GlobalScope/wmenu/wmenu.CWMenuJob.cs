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
	public static partial class wmenu
	{
							public class CWMenuJob : CWMenuMemberBase
							{
								public enum STATE
								{
									STATE_JOB_SELECT,
									STATE_BEGIN_JOB_CHANGE,
									STATE_UPDATE_JOB_CHANGE,
									STATE_BEGIN_STRONGEST_EQUIP,
									STATE_UPDATE_STRONGEST_EQUIP,
									STATE_MAX
								}

								public enum STATE_UPDATEJOB
								{
									STATE_UPDATEJOB_CONFIRMATION,
									STATE_UPDATEJOB_DIRECTION_1,
									STATE_UPDATEJOB_DIRECTION_2,
									STATE_UPDATEJOB_JOBCHANGE,
									STATE_UPDATEJOB_WAITTDL,
									STATE_UPDATEJOB_DIRECTION_3,
									STATE_UPDATEJOB_DIRECTION_4,
									STATE_UPDATEJOB_CANCEL,
									STATE_UPDATEJOB_FINISH,
									STATE_UPDATEJOB_START,
									STATE_UPDATEJOB_START_FADEOUT,
									STATE_UPDATEJOB_START_FADEIN,
									STATE_UPDATEJOB_END,
									STATE_UPDATEJOB_END_WAIT,
									STATE_UPDATEJOB_END_FADEOUT,
									STATE_UPDATEJOB_END_FADEIN
								}

								private delegate void _state();

								public const STATE STATE_JOB_SELECT = STATE.STATE_JOB_SELECT;

								public const STATE STATE_BEGIN_JOB_CHANGE = STATE.STATE_BEGIN_JOB_CHANGE;

								public const STATE STATE_UPDATE_JOB_CHANGE = STATE.STATE_UPDATE_JOB_CHANGE;

								public const STATE STATE_BEGIN_STRONGEST_EQUIP = STATE.STATE_BEGIN_STRONGEST_EQUIP;

								public const STATE STATE_UPDATE_STRONGEST_EQUIP = STATE.STATE_UPDATE_STRONGEST_EQUIP;

								public const STATE STATE_MAX = STATE.STATE_MAX;

								public const STATE_UPDATEJOB STATE_UPDATEJOB_CONFIRMATION = STATE_UPDATEJOB.STATE_UPDATEJOB_CONFIRMATION;

								public const STATE_UPDATEJOB STATE_UPDATEJOB_DIRECTION_1 = STATE_UPDATEJOB.STATE_UPDATEJOB_DIRECTION_1;

								public const STATE_UPDATEJOB STATE_UPDATEJOB_DIRECTION_2 = STATE_UPDATEJOB.STATE_UPDATEJOB_DIRECTION_2;

								public const STATE_UPDATEJOB STATE_UPDATEJOB_JOBCHANGE = STATE_UPDATEJOB.STATE_UPDATEJOB_JOBCHANGE;

								public const STATE_UPDATEJOB STATE_UPDATEJOB_WAITTDL = STATE_UPDATEJOB.STATE_UPDATEJOB_WAITTDL;

								public const STATE_UPDATEJOB STATE_UPDATEJOB_DIRECTION_3 = STATE_UPDATEJOB.STATE_UPDATEJOB_DIRECTION_3;

								public const STATE_UPDATEJOB STATE_UPDATEJOB_DIRECTION_4 = STATE_UPDATEJOB.STATE_UPDATEJOB_DIRECTION_4;

								public const STATE_UPDATEJOB STATE_UPDATEJOB_CANCEL = STATE_UPDATEJOB.STATE_UPDATEJOB_CANCEL;

								public const STATE_UPDATEJOB STATE_UPDATEJOB_FINISH = STATE_UPDATEJOB.STATE_UPDATEJOB_FINISH;

								public const STATE_UPDATEJOB STATE_UPDATEJOB_START = STATE_UPDATEJOB.STATE_UPDATEJOB_START;

								public const STATE_UPDATEJOB STATE_UPDATEJOB_START_FADEOUT = STATE_UPDATEJOB.STATE_UPDATEJOB_START_FADEOUT;

								public const STATE_UPDATEJOB STATE_UPDATEJOB_START_FADEIN = STATE_UPDATEJOB.STATE_UPDATEJOB_START_FADEIN;

								public const STATE_UPDATEJOB STATE_UPDATEJOB_END = STATE_UPDATEJOB.STATE_UPDATEJOB_END;

								public const STATE_UPDATEJOB STATE_UPDATEJOB_END_WAIT = STATE_UPDATEJOB.STATE_UPDATEJOB_END_WAIT;

								public const STATE_UPDATEJOB STATE_UPDATEJOB_END_FADEOUT = STATE_UPDATEJOB.STATE_UPDATEJOB_END_FADEOUT;

								public const STATE_UPDATEJOB STATE_UPDATEJOB_END_FADEIN = STATE_UPDATEJOB.STATE_UPDATEJOB_END_FADEIN;

								private _state[] state = new _state[5];

								private bool _IsFlag;

								private STATE _State;

								private dgs.DGSMessage pMsgName;

								private dgs.DGSMessage pMsgJName;

								private dgs.DGSMessage pMsgJobSkill;

								private dgs.DGSMessage pMsgCheckJName;

								private menu.MBJobParamList pJPL;

								private int playerIndex;

								private int[] playerBox = new int[4];

								private int playerMCount;

								private int jobModelID_;

								private ds.sys3d.CCamera Camera_ = new ds.sys3d.CCamera();

								private int moveFrame_;

								private int countFrame_;

								private VecFx32 org_ = new VecFx32();

								private VecFx32 cur_ = new VecFx32();

								private VecFx32 tar_ = new VecFx32();

								private VecFx32 rot_ = new VecFx32();

								private bool hasNpc_;

								private VecFx32 plPos_;

								private VecFx32 plRot_;

								private VecFx32 npcPos_;

								private VecFx32 npcRot_;

								private int topPlayerJobIdx_;

								private int boardVehicleIdx_;

								private int efpID_;

								private int slideTime;

								private int slideDir;

								private static STATE_UPDATEJOB state2 = STATE_UPDATEJOB.STATE_UPDATEJOB_CONFIRMATION;

								private static sbyte yesno = -1;

								public override bool cSelectInitialize()
								{
									return true;
								}

								public CWMenuJob()
								{
									state[0] = stateJobSelect;
									state[1] = stateBeginJobChange;
									state[2] = stateUpdataJobChange;
									state[3] = stateBeginStrongestEquip;
									state[4] = stateUpdataStrongestEquip;
									pMsgName = (pMsgJobSkill = (pMsgJName = (pMsgCheckJName = null)));
								}

								public override void initialize()
								{
									setFinalize(val: false);
									SVC_WaitVBlankIntr();
									for (int i = 0; i < 4; i++)
									{
										CWMenuManager.Instance().SetShowPcFace(i, show: false);
									}
									CWMenuManager.Instance().SetPrimaryBG(5);
									CWMenuManager.Instance().GetMenuButton().SetUpNormalVer();
									CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_RUN);
									CWMenuManager.Instance().SetSecondlyBG(3);
									CWMenuManager.Instance().SetSecondlyBGVisibility(b: false);
									menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: true);
									menu.MenuManager.getSingleton().buildMenu("job");
									menu.MenuManager.getSingleton().ClearBehaviorButton();
									menu.Medget medget = menu.MenuManager.getSingleton().GetBaseMedget().childNode()
										.nextSibling()
										.nextSibling();
									if (medget != null && medget.behavior() != null)
									{
										pJPL = (menu.MBJobParamList)medget.behavior().queryInterface(menu.MBJobParamList.classIdentifier());
									}
									_IsFlag = false;
									_State = STATE.STATE_JOB_SELECT;
									playerBox[0] = (playerBox[1] = (playerBox[2] = (playerBox[3] = -1)));
									playerMCount = 0;
									playerIndex = menu.MenuManager.getSingleton().GetTargetCharNo();
									for (int j = 0; j < 4; j++)
									{
										if (pl.PlayerParty.instance().player((byte)j).isEnable())
										{
											playerBox[j] = pl.PlayerParty.instance().player((byte)j).playerId();
											playerMCount++;
										}
									}
									jobModelID_ = -1;
									slideTime = 0;
									slideDir = 0;
									eff.CEffectMng.instance().loadEfp("/EFFECT/e430.efp");
									RefreshData();
								}

								public override void run()
								{
									menu.MenuManager.getSingleton().execute();
									state[(int)_State]();
									rotationDisplayCharacter();
									Camera_.execute();
								}

								public override void terminate()
								{
									eff.CEffectMng.instance().unLoadEfp2();
									if (!isFinalize())
									{
										if (pMsgName != null)
										{
											pMsgName.release();
											pMsgName = null;
										}
										if (pMsgJName != null)
										{
											pMsgJName.release();
											pMsgJName = null;
										}
										if (pMsgJobSkill != null)
										{
											pMsgJobSkill.release();
											pMsgJobSkill = null;
										}
										if (pMsgCheckJName != null)
										{
											pMsgCheckJName.release();
											pMsgCheckJName = null;
										}
										menu.MenuManager.getSingleton().releaseWindowAll();
										menu.MenuManager.getSingleton().release();
										setFinalize(val: true);
									}
								}

								public int searchWindow()
								{
									return -1;
								}

								public void RefreshData()
								{
									RefreshCharName(playerBox[playerIndex]);
									RefreshJobSkill(playerBox[playerIndex]);
									RefreshJobName(playerBox[playerIndex]);
									if (pJPL != null)
									{
										pJPL.wmsRefresh();
									}
								}

								public void RefreshCharName(int no)
								{
									if (pMsgName != null)
									{
										pMsgName.release();
										pMsgName = null;
									}
									string str = pl.PlayerParty.instance().playerForId((byte)no).name();
									dgs.DGSMessageManager dGSMessageManager = dgs.msg.CMessageSys.getInstance().Sub();
									pMsgName = dGSMessageManager.createMessage(str, 1);
									_ = pMsgName;
									menu.Medget nodeByID = menu.MenuManager.getSingleton().GetBaseMedget().getNodeByID("player_name");
									pMsgName.setPosition(nodeByID.x(), (short)(nodeByID.y() + (nodeByID.height() - 12) / 2), erase: true);
									pMsgName.setDisplaySpeed(byte.MaxValue);
									pMsgName.setDisplayWait(0);
								}

								public void RefreshJobSkill(int no)
								{
									if (pMsgJobSkill != null)
									{
										pMsgJobSkill.release();
										pMsgJobSkill = null;
									}
									int type = pl.PlayerParty.instance().playerForId((byte)no).jobManager()
										.nowJob();
									int value = pl.PlayerParty.instance().playerForId((byte)no).jobManager()
										.job((pl.JOB_TYPE)type)
										.skill()
										.skillLevel()
										.get();
									string after = "";
									dgs.msg.CMessageSys.getInstance().changeValueFont(value, out after);
									dgs.DGSMessageManager dGSMessageManager = dgs.msg.CMessageSys.getInstance().Sub();
									pMsgJobSkill = dGSMessageManager.createMessage(after, 1);
									_ = pMsgJobSkill;
									menu.Medget nodeByID = menu.MenuManager.getSingleton().GetBaseMedget().getNodeByID("job_skill");
									ds.Vector2<short> vector = new ds.Vector2<short>(0, 0);
									pMsgJobSkill.getTextSize(vector);
									pMsgJobSkill.setPosition((short)(nodeByID.x() + nodeByID.width() - vector.vx), (short)(nodeByID.y() + (nodeByID.height() - 12) / 2), erase: true);
									pMsgJobSkill.setDisplaySpeed(byte.MaxValue);
									pMsgJobSkill.setDisplayWait(0);
								}

								public void RefreshJobList(int no)
								{
									menu.Medget medget = null;
									menu.MBJobParamList mBJobParamList = null;
									if ((medget = menu.MenuManager.getSingleton().root().getNodeByID(TRANSCODE("job_item_list"))) != null && (mBJobParamList = (menu.MBJobParamList)medget.behavior().queryInterface(menu.MBJobParamList.classIdentifier())) != null)
									{
										mBJobParamList.updataJobList();
										mBJobParamList.releaseItemMessage();
										mBJobParamList.createItemMessage();
									}
								}

								public void RefreshJobName(int CharNo)
								{
									if (pMsgJName != null)
									{
										pMsgJName.release();
										pMsgJName = null;
									}
									int msg_number = 50105 + pl.PlayerParty.instance().playerForId((byte)CharNo).jobManager()
										.nowJob();
									dgs.DGSMessageManager dGSMessageManager = dgs.msg.CMessageSys.getInstance().Sub();
									pMsgJName = dGSMessageManager.createMessage((uint)msg_number, dgs.INVALID_MSDHANDLE, 1);
									if (pMsgJName != null)
									{
										menu.Medget nodeByID = menu.MenuManager.getSingleton().GetBaseMedget().getNodeByID("job_name");
										pMsgJName.setPosition(nodeByID.x(), (short)(nodeByID.y() + (nodeByID.height() - 12) / 2), erase: true);
										pMsgJName.setDisplaySpeed(byte.MaxValue);
										pMsgJName.setDisplayWait(0);
									}
								}

								public void stateJobSelect()
								{
									int num = -1;
									if (slideDir != 0)
									{
										slideTime++;
										G2_SetScreenOffset(((slideTime < 4) ? slideTime : (slideTime - 8)) * slideDir * 120, 0);
										if (slideTime == 4)
										{
											RefreshData();
										}
										if (slideTime == 8)
										{
											menu.MenuManager.getSingleton().inputPermission(b: true);
											slideTime = 0;
											slideDir = 0;
										}
										return;
									}
									if (ds.g_TouchPanel.isRelease())
									{
										if (menu.MenuManager.getSingleton().GetDecideButtonState() == 0)
										{
											num = 0;
										}
										else if (CWMenuManager.Instance().GetMenuButton().TouchButtonB())
										{
											num = 1;
										}
										else if (CWMenuManager.Instance().GetMenuButton().TouchButtonL())
										{
											num = 3;
										}
										else if (CWMenuManager.Instance().GetMenuButton().TouchButtonR())
										{
											num = 4;
										}
									}
									else if (menu.MenuManager.getSingleton().GetDecideButtonState() == 0)
									{
										num = 0;
									}
									else if (menu.MenuManager.getSingleton().GetCancelButtonState() == 0 || CWMenuManager.Instance().GetMenuButton().TouchButtonB())
									{
										num = 1;
									}
									else if ((ds.g_Pad.edge() & 0x200) != 0 || CWMenuManager.Instance().GetMenuButton().TouchButtonL())
									{
										num = 3;
									}
									else if ((ds.g_Pad.edge() & 0x100) != 0 || CWMenuManager.Instance().GetMenuButton().TouchButtonR())
									{
										num = 4;
									}
									switch (num)
									{
									case 0:
									{
										if (!menu.MenuManager.getSingleton().GetCursor2d().IsShow())
										{
											menu.MenuManager.getSingleton().playSEBeep();
											break;
										}
										pl.JOB_TYPE jOB_TYPE = static_cast<pl.JOB_TYPE>(pl.PlayerParty.instance().playerForId((byte)playerBox[playerIndex]).jobManager()
											.nowJob());
										bool flag = pl.PlayerParty.instance().playerForId((byte)playerBox[playerIndex]).jobManager()
											.setNowJob(static_cast<pl.JOB_TYPE>(menu.MenuManager.getSingleton().GetTargetItemNo()));
										pl.PlayerParty.instance().playerForId((byte)playerBox[playerIndex]).jobManager()
											.setNowJob(jOB_TYPE);
										if (jOB_TYPE == static_cast<pl.JOB_TYPE>(menu.MenuManager.getSingleton().GetTargetItemNo()))
										{
											flag = false;
										}
										if (flag)
										{
											_State = STATE.STATE_BEGIN_JOB_CHANGE;
											_IsFlag = false;
											menu.MenuManager.getSingleton().playSEDecide();
											menu.MenuManager.getSingleton().inputPermission(b: false);
										}
										else
										{
											menu.MenuManager.getSingleton().playSEBeep();
										}
										break;
									}
									case 1:
										menu.MenuManager.getSingleton().playSECancel();
										CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_TERMINATE);
										CWMenuManager.Instance().SetNextKind(WMENU_KIND.WMENU_KIND_MAIN_MENU);
										break;
									case 3:
									case 4:
										menu.MenuManager.getSingleton().playSEMoveCursor();
										if (playerMCount <= 1)
										{
											menu.MenuManager.getSingleton().playSEBeep();
											break;
										}
										if (num == 3)
										{
											do
											{
												if (--playerIndex < 0)
												{
													playerIndex = 3;
												}
											}
											while (-1 == playerBox[playerIndex] || pl.PlayerParty.instance().player((byte)playerIndex).condition()
												.isNotBattleCondition() || pl.PlayerParty.instance().player((byte)playerIndex).condition()
												.isStone());
										}
										if (num == 4)
										{
											do
											{
												if (++playerIndex >= 4)
												{
													playerIndex = 0;
												}
											}
											while (-1 == playerBox[playerIndex] || pl.PlayerParty.instance().player((byte)playerIndex).condition()
												.isNotBattleCondition() || pl.PlayerParty.instance().player((byte)playerIndex).condition()
												.isStone());
										}
										menu.MenuManager.getSingleton().SetTargetCharNo(playerIndex);
										menu.MenuManager.getSingleton().inputPermission(b: false);
										slideDir = ((num != 3) ? 1 : (-1));
										slideTime = 0;
										break;
									}
								}

								public void stateBeginJobChange()
								{
									for (menu.Medget medget = menu.MenuManager.getSingleton().root().getNodeByID(TRANSCODE("job_item_list"))
										.childNode(); medget != null; medget = medget.nextSibling())
									{
										((menu.MBText)medget.behavior().queryInterface(menu.MBText.classIdentifier()))?.bmTextVisibility(v: false);
									}
									dgs.msg.CMessageSys.getInstance().Sub().dgsMMAreaErase(0, 32, 480, 288);
									pl.JOB_TYPE nextJob = static_cast<pl.JOB_TYPE>(menu.MenuManager.getSingleton().GetTargetItemNo());
									dgs.CCtrlCodeInterface.instance().setJobPenalty(pl.PlayerParty.instance().playerForId((byte)playerBox[playerIndex]).formulaJobPenaltyTime(nextJob));
									dgs.CCtrlCodeInterface.instance().setJobMessageID(50105 + menu.MenuManager.getSingleton().GetTargetItemNo());
									dgs.CCtrlCodeInterface.instance().setArgument1(dgs.msg.CMessageSys.getInstance().Sub().getMessage((uint)(50105 + menu.MenuManager.getSingleton().GetTargetItemNo())));
									menu.MenuManager.getSingleton().Push("job_question2");
									CWMenuManager.Instance().SetSecondlyBGVisibility(b: true);
									CWMenuManager.Instance().GetMenuButton().SetButtonLActivity(b: false);
									CWMenuManager.Instance().GetMenuButton().SetButtonRActivity(b: false);
									CWMenuManager.Instance().GetMenuButton().SetButtonBActivity(b: false);
									_State = STATE.STATE_UPDATE_JOB_CHANGE;
									_IsFlag = false;
									menu.MenuManager.getSingleton().inputPermission(b: true);
								}

								public void stateUpdataJobChange()
								{
									if (!_IsFlag)
									{
										state2 = STATE_UPDATEJOB.STATE_UPDATEJOB_CONFIRMATION;
										yesno = -1;
									}
									switch (state2)
									{
									case STATE_UPDATEJOB.STATE_UPDATEJOB_CONFIRMATION:
										if (menu.MenuManager.getSingleton().GetDecideButtonState() == 0)
										{
											_IsFlag = true;
											if (menu.MenuManager.getSingleton().getFocuseMedget().myTag() == 0)
											{
												CWMenuManager.Instance().GetPcFace().pcfmSetJob(pl.PlayerParty.instance().player((byte)menu.MenuManager.getSingleton().GetTargetCharNo()).playerId(), (uint)menu.MenuManager.getSingleton().GetTargetItemNo());
												menu.MenuManager.getSingleton().inputPermission(b: false);
												menu.MenuManager.getSingleton().playSEDecide();
												yesno = 1;
												state2 = STATE_UPDATEJOB.STATE_UPDATEJOB_START;
											}
											else if (menu.MenuManager.getSingleton().getFocuseMedget().myTag() == 1)
											{
												menu.MenuManager.getSingleton().playSECancel();
												yesno = 0;
												state2 = STATE_UPDATEJOB.STATE_UPDATEJOB_CANCEL;
											}
										}
										else if (menu.MenuManager.getSingleton().GetCancelButtonState() == 0 || CWMenuManager.Instance().GetMenuButton().TouchButtonB())
										{
											_IsFlag = true;
											menu.MenuManager.getSingleton().playSECancel();
											yesno = 0;
											state2 = STATE_UPDATEJOB.STATE_UPDATEJOB_CANCEL;
										}
										break;
									case STATE_UPDATEJOB.STATE_UPDATEJOB_START:
										dgs.CFade.Main().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
										state2 = STATE_UPDATEJOB.STATE_UPDATEJOB_START_FADEOUT;
										break;
									case STATE_UPDATEJOB.STATE_UPDATEJOB_START_FADEOUT:
										if (dgs.CFade.Main().isFaded())
										{
											dgs.CFade.Main().fadeIn(15);
											CWMenuManager.Instance().SetSecondlyBGVisibility(b: false);
											ds.CVram.setSubPlaneVisiblity(bg0: false, bg1: false, bg2: false, bg3: false, obj: false);
											dgs.msg.CMessageSys.getInstance().Sub().dgsMMAreaErase(0, 0, 480, 320);
											menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: false);
											for (menu.Medget medget = menu.MenuManager.getSingleton().root().getNodeByID(TRANSCODE("job_question_main"))
												.childNode(); medget != null; medget = medget.nextSibling())
											{
												((menu.MBText)medget.behavior().queryInterface(menu.MBText.classIdentifier()))?.bmTextVisibility(v: false);
											}
											GX_Power3D(1);
											stageMng.setHidden(flag: true);
											VecFx32 vecFx = new VecFx32(CAMERA_POS_OFFSET_X, CAMERA_POS_OFFSET_Y, CAMERA_POS_OFFSET_Z);
											vecFx.y += JOB_MODEL_Y;
											VecFx32 vecFx2 = new VecFx32(CAMERA_TAR_X, CAMERA_TAR_Y, CAMERA_TAR_Z);
											vecFx2.x += vecFx.x;
											vecFx2.y += vecFx.y;
											vecFx2.z += vecFx.z;
											Camera_.initialize();
											Camera_.setMoveMode(1);
											Camera_.setPosition(vecFx);
											Camera_.setTarget(vecFx2);
											Camera_.setAngle(0, 32768, 0);
											Camera_.setCamUp(0, 4096, 0);
											Camera_.setDistance(65536);
											Camera_.setClip(40960, 2048000);
											Camera_.setFOV(1060, 3956);
											wld.WorldPart.getInstance().getWorldSystem().WorldCamera()
												.setActivity(b: false);
											pl.CPlayerManager cPlayerManager = wld.WorldPart.getInstance().getWorldSystem().PlayerMng();
											if (cPlayerManager != null)
											{
												string arg = "";
												VecFx32 position = new VecFx32(JOB_MODEL_X, JOB_MODEL_Y, JOB_MODEL_Z);
												VecFx32 vecFx3 = new VecFx32(0, 61532, 0);
												VecFx32 scale = new VecFx32(4096, 4096, 4096);
												rot_.copy(vecFx3);
												sprintf(out arg, "j%d%02d", pl.PlayerParty.instance().player((byte)playerIndex).playerId() + 1, pl.PlayerParty.instance().player((byte)playerIndex).jobManager()
													.nowJob() + 1);
												jobModelID_ = cPlayerManager.setUpPlayerHuman(arg, _AutoPilot: false, _Operater: false);
												wld.WorldPart.getInstance().getWorldSystem().PlayerMng()
													.PlayerHuman(jobModelID_)
													.addMotion("w_jobchange");
												wld.WorldPart.getInstance().getWorldSystem().PlayerMng()
													.PlayerHuman(jobModelID_)
													.setScale(scale);
												wld.WorldPart.getInstance().getWorldSystem().PlayerMng()
													.PlayerHuman(jobModelID_)
													.setRotation(vecFx3);
												wld.WorldPart.getInstance().getWorldSystem().PlayerMng()
													.PlayerHuman(jobModelID_)
													.setPosition(position);
												wld.WorldPart.getInstance().getWorldSystem().PlayerMng()
													.PlayerHuman(jobModelID_)
													.startMotion(1001, _Loop: true, 5u);
											}
											state2 = STATE_UPDATEJOB.STATE_UPDATEJOB_START_FADEIN;
										}
										break;
									case STATE_UPDATEJOB.STATE_UPDATEJOB_START_FADEIN:
										if (dgs.CFade.Main().isCleared())
										{
											state2 = STATE_UPDATEJOB.STATE_UPDATEJOB_DIRECTION_1;
										}
										break;
									case STATE_UPDATEJOB.STATE_UPDATEJOB_DIRECTION_1:
									{
										dgs.CFade.Sub().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_WHITE);
										dgs.CCurtain.Top().setEnable(enable: true);
										dgs.CCurtain.Top().setVisible(visible: true);
										dgs.CCurtain.Top().setColor(0, GX_RGB(31, 31, 31));
										dgs.CCurtain.Top().setAlpha(0, 0);
										dgs.CCurtain.Top().setAlpha(15, 31);
										int num3 = eff.CEffectMng.instance().create(430, 1);
										if (num3 != -1)
										{
											VecFx32 pos = new VecFx32(JOB_MODEL_X, JOB_MODEL_Y, JOB_MODEL_Z);
											eff.CEffectMng.instance().setPosition(num3, pos);
										}
										wld.WorldPart.getInstance().getWorldSystem().PlayerMng()
											.PlayerHuman(jobModelID_)
											.startMotion(10354, _Loop: true, 5u);
										org_.copy(Camera_.getPosition());
										tar_.x = org_.x;
										tar_.y = org_.y + 20480;
										tar_.z = org_.z + 163840;
										moveFrame_ = 15;
										countFrame_ = 0;
										state2 = STATE_UPDATEJOB.STATE_UPDATEJOB_DIRECTION_2;
										MatrixSound.MtxSENDS_Play(98, 0, 192, 127);
										break;
									}
									case STATE_UPDATEJOB.STATE_UPDATEJOB_DIRECTION_2:
										if (++countFrame_ > moveFrame_)
										{
											state2 = STATE_UPDATEJOB.STATE_UPDATEJOB_JOBCHANGE;
											Camera_.setPosition(tar_);
											break;
										}
										cur_.x = org_.x + (tar_.x - org_.x) / moveFrame_ * countFrame_;
										cur_.y = org_.y + (tar_.y - org_.y) / moveFrame_ * countFrame_;
										cur_.z = org_.z + (tar_.z - org_.z) / moveFrame_ * countFrame_;
										Camera_.setPosition(cur_);
										break;
									case STATE_UPDATEJOB.STATE_UPDATEJOB_JOBCHANGE:
										if (dgs.CCurtain.Top().isChangedAlpha())
										{
											pl.PlayerParty.instance().playerForId((byte)playerBox[playerIndex]).changeJob(static_cast<pl.JOB_TYPE>(menu.MenuManager.getSingleton().GetTargetItemNo()));
											string arg2 = "";
											VecFx32 position2 = new VecFx32(JOB_MODEL_X, JOB_MODEL_Y, JOB_MODEL_Z);
											VecFx32 vecFx4 = new VecFx32(0, 61532, 0);
											VecFx32 scale2 = new VecFx32(4096, 4096, 4096);
											rot_.copy(vecFx4);
											wld.WorldPart.getInstance().getWorldSystem().PlayerMng()
												.Player(jobModelID_)
												.terminate();
											sprintf(out arg2, "j%d%02d", pl.PlayerParty.instance().player((byte)playerIndex).playerId() + 1, pl.PlayerParty.instance().player((byte)playerIndex).jobManager()
												.nowJob() + 1);
											jobModelID_ = wld.WorldPart.getInstance().getWorldSystem().PlayerMng()
												.setUpPlayerHuman(arg2, _AutoPilot: false, _Operater: false);
											wld.WorldPart.getInstance().getWorldSystem().PlayerMng()
												.PlayerHuman(jobModelID_)
												.addMotion("w_jobchange");
											wld.WorldPart.getInstance().getWorldSystem().PlayerMng()
												.Player(jobModelID_)
												.setScale(scale2);
											wld.WorldPart.getInstance().getWorldSystem().PlayerMng()
												.Player(jobModelID_)
												.setRotation(vecFx4);
											wld.WorldPart.getInstance().getWorldSystem().PlayerMng()
												.Player(jobModelID_)
												.setPosition(position2);
											wld.WorldPart.getInstance().getWorldSystem().PlayerMng()
												.Player(jobModelID_)
												.startMotion(10354, _Loop: true, 5u);
											wld.WorldPart.getInstance().getWorldSystem().PlayerMng()
												.Player(jobModelID_)
												.setCurrentFrame(15u);
											wld.WorldPart.getInstance().getWorldSystem().PlayerMng()
												.Player(jobModelID_)
												.setMotionLoop(loop: false);
											state2 = STATE_UPDATEJOB.STATE_UPDATEJOB_WAITTDL;
										}
										break;
									case STATE_UPDATEJOB.STATE_UPDATEJOB_WAITTDL:
										if (TexDivideLoader.getSingleton().tdlIsEmpty())
										{
											dgs.CFade.Sub().fadeIn(15);
											dgs.CCurtain.Top().setAlpha(15, 0);
											state2 = STATE_UPDATEJOB.STATE_UPDATEJOB_DIRECTION_3;
											tar_.copy(org_);
											org_.copy(Camera_.getPosition());
											cur_.copy(Camera_.getPosition());
											moveFrame_ = 15;
											countFrame_ = 0;
										}
										break;
									case STATE_UPDATEJOB.STATE_UPDATEJOB_DIRECTION_3:
										if (++countFrame_ <= moveFrame_)
										{
											cur_.x = org_.x + (tar_.x - org_.x) / moveFrame_ * countFrame_;
											cur_.y = org_.y + (tar_.y - org_.y) / moveFrame_ * countFrame_;
											cur_.z = org_.z + (tar_.z - org_.z) / moveFrame_ * countFrame_;
											Camera_.setPosition(cur_);
										}
										else if (wld.WorldPart.getInstance().getWorldSystem().PlayerMng()
											.Player(jobModelID_)
											.isEndOfMotion())
										{
											wld.WorldPart.getInstance().getWorldSystem().PlayerMng()
												.Player(jobModelID_)
												.startMotion(1001, _Loop: true, 5u);
											state2 = STATE_UPDATEJOB.STATE_UPDATEJOB_DIRECTION_4;
										}
										break;
									case STATE_UPDATEJOB.STATE_UPDATEJOB_DIRECTION_4:
										if (dgs.CCurtain.Top().isChangedAlpha())
										{
											dgs.CCurtain.Top().setEnable(enable: false);
											dgs.CCurtain.Top().setVisible(visible: false);
											state2 = STATE_UPDATEJOB.STATE_UPDATEJOB_END;
										}
										break;
									case STATE_UPDATEJOB.STATE_UPDATEJOB_END:
										moveFrame_ = 15;
										countFrame_ = 0;
										state2 = STATE_UPDATEJOB.STATE_UPDATEJOB_END_WAIT;
										break;
									case STATE_UPDATEJOB.STATE_UPDATEJOB_END_WAIT:
										if (++countFrame_ > moveFrame_)
										{
											dgs.CFade.Main().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
											state2 = STATE_UPDATEJOB.STATE_UPDATEJOB_END_FADEOUT;
										}
										break;
									case STATE_UPDATEJOB.STATE_UPDATEJOB_END_FADEOUT:
									{
										if (!dgs.CFade.Main().isFaded())
										{
											break;
										}
										GX_Power3D(0);
										dgs.CFade.Main().fadeIn(15);
										ds.CVram.setSubPlaneVisiblity(bg0: false, bg1: false, bg2: false, bg3: true, obj: true);
										menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: true);
										menu.MenuManager.getSingleton().Pop();
										int num2 = 0;
										for (int j = 0; j < 4; j++)
										{
											if (pl.PlayerParty.instance().player((byte)j).isEnable())
											{
												num2++;
											}
										}
										if (num2 >= 2)
										{
											CWMenuManager.Instance().GetMenuButton().SetButtonLActivity(b: true);
											CWMenuManager.Instance().GetMenuButton().SetButtonRActivity(b: true);
										}
										CWMenuManager.Instance().GetMenuButton().SetButtonBActivity(b: true);
										for (int k = 0; k < 5; k++)
										{
											pl.EquipItemInfo equipItemInfo = pl.PlayerParty.instance().player((byte)menu.MenuManager.getSingleton().GetTargetCharNo()).equipParameter()
												.equipPoint(k)
												.release();
											if (equipItemInfo.itemId_ > 0 && equipItemInfo.itemNumber_ > 0)
											{
												pl.PlayerParty.instance().item().storeItem(equipItemInfo.itemId_, equipItemInfo.itemNumber_);
											}
										}
										RefreshData();
										stageMng.setHidden(flag: false);
										wld.WorldPart.getInstance().getScene().setCamera(wld.WorldPart.getInstance().getWorldSystem().WorldCamera());
										wld.WorldPart.getInstance().getWorldSystem().WorldCamera()
											.setActivity(b: true);
										wld.WorldPart.getInstance().getWorldSystem().PlayerMng()
											.Player(jobModelID_)
											.terminate();
										jobModelID_ = -1;
										state2 = STATE_UPDATEJOB.STATE_UPDATEJOB_END_FADEIN;
										break;
									}
									case STATE_UPDATEJOB.STATE_UPDATEJOB_END_FADEIN:
										if (dgs.CFade.Main().isCleared())
										{
											state2 = STATE_UPDATEJOB.STATE_UPDATEJOB_FINISH;
										}
										break;
									case STATE_UPDATEJOB.STATE_UPDATEJOB_CANCEL:
									{
										CWMenuManager.Instance().SetSecondlyBGVisibility(b: false);
										menu.MenuManager.getSingleton().Pop();
										dgs.msg.CMessageSys.getInstance().Sub().dgsMMAreaErase(0, 32, 480, 288);
										int num = 0;
										for (int i = 0; i < 4; i++)
										{
											if (pl.PlayerParty.instance().player((byte)i).isEnable())
											{
												num++;
											}
										}
										if (num >= 2)
										{
											CWMenuManager.Instance().GetMenuButton().SetButtonLActivity(b: true);
											CWMenuManager.Instance().GetMenuButton().SetButtonRActivity(b: true);
										}
										CWMenuManager.Instance().GetMenuButton().SetButtonBActivity(b: true);
										_State = STATE.STATE_JOB_SELECT;
										_IsFlag = false;
										break;
									}
									case STATE_UPDATEJOB.STATE_UPDATEJOB_FINISH:
										_State = STATE.STATE_JOB_SELECT;
										_IsFlag = false;
										menu.MenuManager.getSingleton().inputPermission(b: true);
										break;
									}
								}

								public void stateBeginStrongestEquip()
								{
									_State = STATE.STATE_UPDATE_STRONGEST_EQUIP;
									_IsFlag = false;
								}

								public void stateUpdataStrongestEquip()
								{
									for (int i = 0; i < 5; i++)
									{
										pl.EquipItemInfo equipItemInfo = pl.PlayerParty.instance().player((byte)menu.MenuManager.getSingleton().GetTargetCharNo()).equipParameter()
											.equipPoint(i)
											.release();
										if (equipItemInfo.itemId_ > 0 && equipItemInfo.itemNumber_ > 0)
										{
											pl.PlayerParty.instance().item().storeItem(equipItemInfo.itemId_, equipItemInfo.itemNumber_);
										}
									}
									_State = STATE.STATE_JOB_SELECT;
									RefreshData();
									_IsFlag = false;
								}

								public void rotationDisplayCharacter()
								{
									if (-1 != jobModelID_)
									{
										rot_.y = (rot_.y + 72) & 0xFFFF;
										wld.WorldPart.getInstance().getWorldSystem().PlayerMng()
											.Player(jobModelID_)
											.setRotation(rot_);
									}
								}
							}
	}
}
