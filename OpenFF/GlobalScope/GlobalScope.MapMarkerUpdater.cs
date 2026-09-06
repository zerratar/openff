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
						public class MapMarkerUpdater : wld.CParallelWorldSystem
						{
							public static MapMarkerUpdater instance_ = new MapMarkerUpdater();

							private ds.Vector<MapMarkerAccepter, ds.FastErasePolicy<MapMarkerAccepter>> AccepterVector_ = new ds.Vector<MapMarkerAccepter, ds.FastErasePolicy<MapMarkerAccepter>>(32);

							public override void initialize(wld.CBaseSystem Sys)
							{
								AccepterVector_.clear();
							}

							public override bool execute(wld.CBaseSystem Sys)
							{
								if (AccepterVector_.empty())
								{
									return true;
								}
								if (Sys.Mode() == wld.CBaseSystem.WORLD_MODE.WORLD_MODE_SITE && Sys.State() != wld.CBaseSystem.WORLD_STATE.WORLD_STATE_END)
								{
									wld.MapMarker mapMarker = null;
									ds.Vector2<int> vector = new ds.Vector2<int>(0, 0);
									for (int i = 0; AccepterVector_.size() > i; i++)
									{
										mapMarker = wld.WorldPart.getInstance().getWorldSystem().World2DMng()
											.refWorldMap()
											.getMapMarker(AccepterVector_[i].id_);
										if (mapMarker == null)
										{
											continue;
										}
										vector = wld.transCoordWorldToAreaFx32(AccepterVector_[i].acceptPos(), wld.WorldPart.getInstance().getWorldSystem().World2DMng()
											.refWorldMap()
											.getAreaOrg(), wld.WorldPart.getInstance().getWorldSystem().World2DMng()
											.refWorldMap()
											.getAreaWH());
										int x = FX_Mul(LCD_HEIGHT * 4096 * 6 / 5, vector.vx - 2048) + 983040;
										int num = FX_Mul(LCD_HEIGHT * 4096, vector.vy - 2048) + 655360;
										if (vector.vx < 0 || 4096 < vector.vx || vector.vy < 0 || 4096 < vector.vy)
										{
											x = -131072;
											num = -131072;
										}
										mapMarker.Cell_.SetPositionF(x, num);
										mapMarker.Cell_.SetDepth(-1);
										mapMarker.Cell_.SetShow(AccepterVector_[i].acceptVisibility());
										mapMarker.Cell_.SetAnimation(AccepterVector_[i].acceptAnimationState());
										if (mapMarker.m_bCell_YSeparate)
										{
											num = ((num >= LCD_HEIGHT / 2 * 4096) ? (num - LCD_HEIGHT * 4096) : (num + LCD_HEIGHT * 4096));
											mapMarker.Cell_YSeparate.SetPositionF(x, num);
											mapMarker.Cell_YSeparate.SetDepth(-1);
											mapMarker.Cell_YSeparate.SetShow(AccepterVector_[i].acceptVisibility());
											mapMarker.Cell_YSeparate.SetAnimation(AccepterVector_[i].acceptAnimationState());
										}
										_ = ds.g_Pad.pad() & 0x2000;
										int num2 = AccepterVector_[i].acceptDir();
										if (-1 != num2 && mapMarker.dir != num2)
										{
											mapMarker.dir = num2;
											mapMarker.Cell_.PlayAnimation((ushort)num2, NNSG2dAnimationPlayMode.NNS_G2D_ANIMATIONPLAYMODE_FORWARD_LOOP);
											if (mapMarker.m_bCell_YSeparate)
											{
												mapMarker.Cell_YSeparate.PlayAnimation((ushort)num2, NNSG2dAnimationPlayMode.NNS_G2D_ANIMATIONPLAYMODE_FORWARD_LOOP);
											}
										}
									}
								}
								return true;
							}

							public override void terminate(wld.CBaseSystem Sys)
							{
								int num = AccepterVector_.size() - 1;
								while (0 <= num)
								{
									wld.WorldPart.getInstance().getWorldSystem().World2DMng()
										.refWorldMap()
										.delMapMarker(AccepterVector_[num].id_);
									AccepterVector_[num] = null;
									AccepterVector_.erase(num);
									num--;
								}
								AccepterVector_.clear();
							}

							public void changeAnimation(MapMarkerAccepter pAccepter, int Pattern)
							{
								if (pAccepter == null)
								{
									return;
								}
								wld.MapMarker mapMarker = wld.WorldPart.getInstance().getWorldSystem().World2DMng()
									.refWorldMap()
									.getMapMarker(pAccepter.id_);
								if (mapMarker != null)
								{
									mapMarker.Cell_.PlayAnimation((ushort)Pattern, NNSG2dAnimationPlayMode.NNS_G2D_ANIMATIONPLAYMODE_FORWARD_LOOP);
									if (mapMarker.m_bCell_YSeparate)
									{
										mapMarker.Cell_YSeparate.PlayAnimation((ushort)Pattern, NNSG2dAnimationPlayMode.NNS_G2D_ANIMATIONPLAYMODE_FORWARD_LOOP);
									}
								}
							}

							public void registerAccepter(MapMarkerAccepter pAccepter, int Type)
							{
								if (pAccepter != null && 32 > AccepterVector_.size() && !checkRepeatAccepter(pAccepter))
								{
									pAccepter.id_ = wld.WorldPart.getInstance().getWorldSystem().World2DMng()
										.refWorldMap()
										.newMapMarker((wld.MapMarkerType)Type);
									pAccepter.type_ = Type;
									if (-1 != pAccepter.id_)
									{
										AccepterVector_.push_back(pAccepter);
									}
								}
							}

							public void deregisterAccepter(MapMarkerAccepter pAccepter)
							{
								if (pAccepter == null || -1 == pAccepter.id_ || AccepterVector_.empty())
								{
									return;
								}
								int num = AccepterVector_.size() - 1;
								while (0 <= num)
								{
									if (AccepterVector_[num] == pAccepter)
									{
										wld.WorldPart.getInstance().getWorldSystem().World2DMng()
											.refWorldMap()
											.delMapMarker(pAccepter.id_);
										AccepterVector_[num] = null;
										AccepterVector_.erase(num);
									}
									num--;
								}
							}

							public void resetAccepter()
							{
								int num = AccepterVector_.size() - 1;
								while (0 <= num)
								{
									AccepterVector_.at(num).id_ = wld.WorldPart.getInstance().getWorldSystem().World2DMng()
										.refWorldMap()
										.newMapMarker((wld.MapMarkerType)AccepterVector_.at(num).type_);
									wld.MapMarker mapMarker = wld.WorldPart.getInstance().getWorldSystem().World2DMng()
										.refWorldMap()
										.getMapMarker(AccepterVector_.at(num).id_);
									if (mapMarker != null)
									{
										mapMarker.dir = -1;
									}
									num--;
								}
							}

							public bool checkRepeatAccepter(MapMarkerAccepter pAccepter)
							{
								if (AccepterVector_.empty())
								{
									return false;
								}
								for (int i = 0; AccepterVector_.size() > i; i++)
								{
									if (pAccepter == AccepterVector_[i])
									{
										return true;
									}
								}
								return false;
							}

							public static MapMarkerUpdater getSingleton()
							{
								return instance_;
							}
						}
}
