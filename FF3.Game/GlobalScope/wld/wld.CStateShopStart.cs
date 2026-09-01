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
							public class CStateShopStart : CBaseState
							{
								public override void start(CBaseSystem _sys)
								{
									shop.CShopUpDisplayComposition.Instance().initialize();
									shop.CShopManager.Instance().initialize();
									GX_SetDispSelect(GXDispSelect.GX_DISP_SELECT_MAIN_SUB);
									G3X_SetClearColor(GX_RGB(0, 0, 0), 1, 32767, 1, 0);
									dv.CDeviceManager.getInstance().initialize();
									_sys.WorldCamera().initialize();
									_sys.WorldCamera().setMode(cmr.CWorldCamera.MODE.MODE_FREE);
									_sys.WorldCamera().setPosOffsetMoveType(cmr.CWorldCamera.MOVE_TYPE.MOVE_TYPE_ERR);
									_sys.WorldCamera().setTrgOffsetMoveType(cmr.CWorldCamera.MOVE_TYPE.MOVE_TYPE_ERR);
									_sys.WorldCamera().setType(cmr.CWorldCamera.TYPE.TYPE_ERR);
									_sys.WorldCamera().composit.setZoomEnable(b: false);
									sprintf(out var arg, "s02_0%d", shop.CShopManager.Instance().ShopParameterMng().ShopParameter(shop.CShopManager.Instance().ShopIndex())
										.ShopKind() + 1);
									_sys.setupStage(arg, CBaseSystem.WORLD_MODE.WORLD_MODE_SHOP);
									VecFx32 vecFx = new VecFx32();
									VecFx32 vecFx2 = new VecFx32();
									VecFx32[] array = new VecFx32[4]
									{
										new VecFx32(),
										new VecFx32(),
										new VecFx32(),
										new VecFx32()
									};
									int[,] array2 = new int[4, 4];
									int deg = 0;
									int aspect = 0;
									XbnNode xbnNode = menu.MenuManager.getSingleton().xbnRoot();
									XbnNodeList xbnNodeList = new XbnNodeList();
									xbnNode.getNodesByTagNameFromChildren(TRANSCODE("structure"), xbnNodeList);
									for (int num = xbnNodeList.size() - 1; num >= 0; num--)
									{
										if (strcmp(TRANSCODE("CAMERA_PROJECTION"), xbnNodeList[num].nodeValueString()) == 0)
										{
											XbnNode xbnNode2 = xbnNodeList[num].firstChild();
											int iData = xbnNode2.firstChild().nodeValueInt();
											float x = BitReader.convertSingle(iData);
											deg = FX_F32_TO_FX32(x);
											xbnNode2 = xbnNode2.nextSibling();
											iData = xbnNode2.firstChild().nodeValueInt();
											x = BitReader.convertSingle(iData);
											aspect = FX_F32_TO_FX32(x);
										}
										else if (strcmp(TRANSCODE("CAMERA_POSITION"), xbnNodeList[num].nodeValueString()) == 0)
										{
											XbnNode xbnNode3 = xbnNodeList[num].firstChild();
											int iData = xbnNode3.firstChild().nodeValueInt();
											float x = BitReader.convertSingle(iData);
											vecFx.x = FX_F32_TO_FX32(x);
											xbnNode3 = xbnNode3.nextSibling();
											iData = xbnNode3.firstChild().nodeValueInt();
											x = BitReader.convertSingle(iData);
											vecFx.y = FX_F32_TO_FX32(x);
											xbnNode3 = xbnNode3.nextSibling();
											iData = xbnNode3.firstChild().nodeValueInt();
											x = BitReader.convertSingle(iData);
											vecFx.z = FX_F32_TO_FX32(x);
										}
										else if (strcmp(TRANSCODE("TARGET_POSITION"), xbnNodeList[num].nodeValueString()) == 0)
										{
											XbnNode xbnNode4 = xbnNodeList[num].firstChild();
											int iData = xbnNode4.firstChild().nodeValueInt();
											float x = BitReader.convertSingle(iData);
											vecFx2.x = FX_F32_TO_FX32(x);
											xbnNode4 = xbnNode4.nextSibling();
											iData = xbnNode4.firstChild().nodeValueInt();
											x = BitReader.convertSingle(iData);
											vecFx2.y = FX_F32_TO_FX32(x);
											xbnNode4 = xbnNode4.nextSibling();
											iData = xbnNode4.firstChild().nodeValueInt();
											x = BitReader.convertSingle(iData);
											vecFx2.z = FX_F32_TO_FX32(x);
										}
										else if (strcmp(TRANSCODE("PLAYERS_POSITION"), xbnNodeList[num].nodeValueString()) == 0)
										{
											XbnNode xbnNode5 = xbnNodeList[num].firstChild();
											int num2 = 0;
											while (num2 < 4 && xbnNode5 != null)
											{
												int iData = xbnNode5.firstChild().nodeValueInt();
												float x = BitReader.convertSingle(iData);
												array[num2].x = FX_F32_TO_FX32(x);
												xbnNode5 = xbnNode5.nextSibling();
												iData = xbnNode5.firstChild().nodeValueInt();
												x = BitReader.convertSingle(iData);
												array[num2].y = FX_F32_TO_FX32(x);
												xbnNode5 = xbnNode5.nextSibling();
												iData = xbnNode5.firstChild().nodeValueInt();
												x = BitReader.convertSingle(iData);
												array[num2].z = FX_F32_TO_FX32(x);
												num2++;
												xbnNode5 = xbnNode5.nextSibling();
											}
										}
										else if (strcmp(TRANSCODE("PLAYERS_ROTATION"), xbnNodeList[num].nodeValueString()) == 0)
										{
											XbnNode xbnNode6 = xbnNodeList[num].firstChild();
											for (int i = 0; i < 4; i++)
											{
												if (xbnNode6 == null)
												{
													break;
												}
												int num3 = 0;
												while (num3 < 4 && xbnNode6 != null)
												{
													int iData = xbnNode6.firstChild().nodeValueInt();
													float x = BitReader.convertSingle(iData);
													array2[i, num3] = FX_F32_TO_FX32(x);
													num3++;
													xbnNode6 = xbnNode6.nextSibling();
												}
											}
										}
									}
									VecFx32 camUp = new VecFx32(0, 4096, 0);
									NNS_G3dGlbPerspective(FX_SinIdx(FX_DEG_TO_IDX(deg)), FX_CosIdx(FX_DEG_TO_IDX(deg)), aspect, 4096, 8388608);
									NNS_G3dGlbLookAt(vecFx, camUp, vecFx2);
									for (int j = 0; j < 4; j++)
									{
										if (shop.CShopUpDisplayComposition.Instance().charIndex(j) >= 0 && pl.PlayerParty.instance().player((byte)j).isEnable())
										{
											OS_Printf("Setting PartyPlayer[%d], ID[%d]\n", j, pl.PlayerParty.instance().player((byte)j).playerId());
											OS_Printf("\tPOS( %f, %f, %f )\n", FX_FX32_TO_F32(array[j].x), FX_FX32_TO_F32(array[j].y), FX_FX32_TO_F32(array[j].z));
											OS_Printf("\tROT %f\n", FX_FX32_TO_F32(array2[pl.PlayerParty.instance().player((byte)j).playerId(), j]));
											characterMng.setPosition(shop.CShopUpDisplayComposition.Instance().charIndex(j), array[j]);
											characterMng.setRotation(shop.CShopUpDisplayComposition.Instance().charIndex(j), 0, (ushort)FX_DEG_TO_IDX(array2[pl.PlayerParty.instance().player((byte)j).playerId(), j]), 0);
										}
									}
									stageMng.enableFakeMaterialColor(enable: true, stg.CStageMng.FAKEMATERIAL_TYPE.TYPE_TOON);
									stageMng.setFakeMaterialColor(0, GX_RGB(18, 17, 14));
									dgs.CFade.Main().fadeIn(5);
									dgs.CFade.Sub().fadeIn(5);
									_sys.World2DMng().refWorldMap().hideMapMarker();
								}

								public override void update(CBaseSystem _sys)
								{
									if (dgs.CFade.Main().isCleared() && dgs.CFade.Sub().isCleared())
									{
										setPhase(PHASE.END);
									}
								}

								public override void end(CBaseSystem _sys)
								{
									_sys.setState(CBaseSystem.WORLD_STATE.WORLD_STATE_MOVE);
								}

								public override bool canExecuteEvent(CBaseSystem arg0)
								{
									return false;
								}
							}
	}
}
