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
	public static class u2d
	{
		public class PopUp
		{
			public enum puSTATE
			{
				pusSTAND_BY,
				pusCOME_IN,
				pusWAIT,
				pusMAX
			}

			public enum puKIND
			{
				pukX,
				pukDAMAGE,
				pukHIT,
				pukMAX
			}

			public enum PALETTE
			{
				ORANGE,
				PINK,
				BLUE,
				GREEN,
				WHITE,
				PALETTE_MAX
			}

			public const puKIND pukX = puKIND.pukX;

			public const puKIND pukDAMAGE = puKIND.pukDAMAGE;

			public const puKIND pukHIT = puKIND.pukHIT;

			public const puKIND pukMAX = puKIND.pukMAX;

			public const PALETTE ORANGE = PALETTE.ORANGE;

			public const PALETTE PINK = PALETTE.PINK;

			public const PALETTE BLUE = PALETTE.BLUE;

			public const PALETTE GREEN = PALETTE.GREEN;

			public const PALETTE WHITE = PALETTE.WHITE;

			public const PALETTE PALETTE_MAX = PALETTE.PALETTE_MAX;

			public static sys2d.Sprite3d[] g_PopUpSprite = new sys2d.Sprite3d[3]
			{
				new sys2d.Sprite3d(),
				new sys2d.Sprite3d(),
				new sys2d.Sprite3d()
			};

			public static void puInitializeSystem()
			{
				changeCompanyDirectory();
				g_PopUpSprite[1].Load2(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, "battle_suuji_i");
				g_PopUpSprite[2].copy(g_PopUpSprite[1]);
				g_PopUpSprite[0].copy(g_PopUpSprite[1]);
			}

			public static void puReleaseSystem()
			{
				for (int i = 0; i < 3; i++)
				{
					g_PopUpSprite[i].Release();
				}
			}

			public virtual void Release()
			{
			}
		}

		public class PopUpDamageNumber : PopUp
		{
			public static int pudnFIGURES_MAX = 4;

			private sys2d.Sprite3d[] m_Number = new sys2d.Sprite3d[pudnFIGURES_MAX];

			public PopUpDamageNumber()
			{
				for (int i = 0; i < m_Number.Length; i++)
				{
					m_Number[i] = new sys2d.Sprite3d();
				}
			}

			public bool pudnCreate(int no, NNSG2dFVec2 pos, int damage)
			{
				int num = no;
				if (num < 0)
				{
					num *= -1;
				}
				bool flag = false;
				int num2 = ds.clamp(abs(num), 0, 9999);
				int num3 = 1;
				NNSG2dFVec2 nNSG2dFVec = new NNSG2dFVec2(pos);
				for (int i = 1; i < pudnFIGURES_MAX; i++)
				{
					num3 *= 10;
				}
				for (int i = 0; i < pudnFIGURES_MAX; i++)
				{
					ushort num4 = (ushort)(num2 / num3);
					if (flag || num4 != 0 || i == pudnFIGURES_MAX - 1)
					{
						m_Number[i].copy(PopUp.g_PopUpSprite[1]);
						m_Number[i].SetPosition(nNSG2dFVec);
						m_Number[i].SetCell(num4);
						m_Number[i].SetPriority(0);
						m_Number[i].SetAutoDelete(auto_delete: true);
						if (damage == 0)
						{
							m_Number[i].SetColor((uint)COLOR_PINK());
						}
						else
						{
							m_Number[i].SetColor((uint)COLOR_GREEN());
						}
						sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(m_Number[i]);
						nNSG2dFVec.x += ds.S32toFX32(12);
						flag = true;
					}
					num2 -= num4 * num3;
					num3 /= 10;
				}
				return true;
			}

			public bool pudnIsExist()
			{
				if (sys2d.DS2DManager.d2dGetInstance().d2dGetEntriedSpriteIndex(m_Number[pudnFIGURES_MAX - 1]) == -1)
				{
					return false;
				}
				return true;
			}

			public override void Release()
			{
				for (int i = 0; i < pudnFIGURES_MAX; i++)
				{
					m_Number[i].Release();
				}
			}
		}

		public class PopUpHitNumber : PopUp
		{
			public enum puhnKIND
			{
				puhnkNORMAL,
				puhnkCRITICAL,
				puhnkMISS,
				puhnkSTART_BAR,
				puhnkEND_BAR,
				puhnkMAX
			}

			public const puhnKIND puhnkNORMAL = puhnKIND.puhnkNORMAL;

			public const puhnKIND puhnkCRITICAL = puhnKIND.puhnkCRITICAL;

			public const puhnKIND puhnkMISS = puhnKIND.puhnkMISS;

			public const puhnKIND puhnkSTART_BAR = puhnKIND.puhnkSTART_BAR;

			public const puhnKIND puhnkEND_BAR = puhnKIND.puhnkEND_BAR;

			public const puhnKIND puhnkMAX = puhnKIND.puhnkMAX;

			public static int puhnFIGURES_MAX = 2;

			public static int puhnHIT_STR_NO = 10;

			public static int CRITICAL_STR_NO = 11;

			public static int MISS_STR_NO = 12;

			public static int START_BAR_NO = 13;

			public static int END_BAR_NO = 14;

			public static int puhnHIT_X = 15;

			public static int puhtHIT_11 = 16;

			private sys2d.Sprite3d[] m_Number = new sys2d.Sprite3d[puhnFIGURES_MAX];

			private sys2d.Sprite3d m_Hits = new sys2d.Sprite3d();

			private sys2d.Sprite3d m_HitsX = new sys2d.Sprite3d();

			private sys2d.Sprite3d m_Critical = new sys2d.Sprite3d();

			private sys2d.Sprite3d m_Miss = new sys2d.Sprite3d();

			private sys2d.Sprite3d startBar_ = new sys2d.Sprite3d();

			private sys2d.Sprite3d endBar_ = new sys2d.Sprite3d();

			public PopUpHitNumber()
			{
				for (int i = 0; i < m_Number.Length; i++)
				{
					m_Number[i] = new sys2d.Sprite3d();
				}
			}

			public bool puhnCreate(int no, NNSG2dFVec2 pos, puhnKIND kind)
			{
				bool flag = false;
				int num = ds.clamp(abs(no), 0, 9999);
				int num2 = 1;
				NNSG2dFVec2 nNSG2dFVec = new NNSG2dFVec2(pos);
				bool flag2 = false;
				switch (OS_GetLanguage())
				{
				case 2:
				case 3:
				case 4:
				case 5:
					flag2 = true;
					break;
				}
				switch (kind)
				{
				case puhnKIND.puhnkNORMAL:
				{
					nNSG2dFVec.x -= ds.S32toFX32(6);
					if (flag2)
					{
						nNSG2dFVec.x -= ds.S32toFX32(6);
						m_HitsX.copy(PopUp.g_PopUpSprite[2]);
						m_HitsX.SetPosition(nNSG2dFVec);
						m_HitsX.SetCell((ushort)puhnHIT_X);
						m_HitsX.SetPriority(0);
						m_HitsX.SetColor((uint)COLOR_ORANGE());
						m_HitsX.SetAutoDelete(auto_delete: true);
						sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(m_HitsX);
						nNSG2dFVec.x += ds.S32toFX32(32);
					}
					for (int i = 1; i < puhnFIGURES_MAX; i++)
					{
						num2 *= 10;
					}
					for (int i = 0; i < puhnFIGURES_MAX; i++)
					{
						ushort num3 = (ushort)(num / num2);
						if (flag || num3 != 0 || i == puhnFIGURES_MAX - 1)
						{
							m_Number[i].copy(PopUp.g_PopUpSprite[2]);
							m_Number[i].SetPosition(nNSG2dFVec);
							m_Number[i].SetCell(num3);
							m_Number[i].SetPriority(0);
							m_Number[i].SetAutoDelete(auto_delete: true);
							m_Number[i].SetColor((uint)COLOR_ORANGE());
							sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(m_Number[i]);
							nNSG2dFVec.x += ds.S32toFX32(12);
							flag = true;
						}
						num -= num3 * num2;
						num2 /= 10;
					}
					m_Hits.copy(PopUp.g_PopUpSprite[2]);
					m_Hits.SetPosition(nNSG2dFVec);
					m_Hits.SetCell((ushort)(flag2 ? puhtHIT_11 : puhnHIT_STR_NO));
					m_Hits.SetPriority(0);
					m_Hits.SetColor((uint)COLOR_ORANGE());
					m_Hits.SetAutoDelete(auto_delete: true);
					sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(m_Hits);
					break;
				}
				case puhnKIND.puhnkCRITICAL:
					nNSG2dFVec.x += ds.S32toFX32(12);
					m_Critical.copy(PopUp.g_PopUpSprite[2]);
					m_Critical.SetPosition(nNSG2dFVec);
					m_Critical.SetCell((ushort)CRITICAL_STR_NO);
					m_Critical.SetPriority(0);
					m_Critical.SetPalette(4);
					m_Critical.SetAutoDelete(auto_delete: true);
					sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(m_Critical);
					break;
				case puhnKIND.puhnkMISS:
					m_Miss.copy(PopUp.g_PopUpSprite[2]);
					m_Miss.SetPosition(nNSG2dFVec);
					m_Miss.SetCell((ushort)MISS_STR_NO);
					m_Miss.SetPriority(0);
					m_Miss.SetPalette(2);
					m_Miss.SetAutoDelete(auto_delete: true);
					sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(m_Miss);
					break;
				case puhnKIND.puhnkSTART_BAR:
					startBar_.copy(PopUp.g_PopUpSprite[2]);
					startBar_.SetPosition(nNSG2dFVec);
					startBar_.SetCell((ushort)START_BAR_NO);
					startBar_.SetPriority(0);
					startBar_.SetPalette(4);
					startBar_.SetAnimation(anm: false);
					sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(startBar_);
					break;
				case puhnKIND.puhnkEND_BAR:
					endBar_.copy(PopUp.g_PopUpSprite[2]);
					endBar_.SetCell((ushort)END_BAR_NO);
					endBar_.SetPriority(0);
					endBar_.SetPalette(4);
					endBar_.SetAnimation(anm: false);
					endBar_.SetScaleF(65536, 4096);
					nNSG2dFVec.x += 229376;
					endBar_.SetPosition(nNSG2dFVec);
					sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(endBar_);
					break;
				}
				return true;
			}

			public bool puhnIsExist()
			{
				bool flag = false;
				if (!flag)
				{
					flag = sys2d.DS2DManager.d2dGetInstance().d2dGetEntriedSpriteIndex(m_Number[puhnFIGURES_MAX - 1]) != -1;
				}
				if (!flag)
				{
					flag = sys2d.DS2DManager.d2dGetInstance().d2dGetEntriedSpriteIndex(m_Hits) != -1;
				}
				if (!flag)
				{
					flag = sys2d.DS2DManager.d2dGetInstance().d2dGetEntriedSpriteIndex(m_HitsX) != -1;
				}
				if (!flag)
				{
					flag = sys2d.DS2DManager.d2dGetInstance().d2dGetEntriedSpriteIndex(m_Critical) != -1;
				}
				if (!flag)
				{
					flag = sys2d.DS2DManager.d2dGetInstance().d2dGetEntriedSpriteIndex(m_Miss) != -1;
				}
				return flag;
			}

			public override void Release()
			{
				for (int i = 0; i < puhnFIGURES_MAX; i++)
				{
					sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(m_Number[i]);
					m_Number[i].Release();
				}
				sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(m_Hits);
				m_Hits.Release();
				sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(m_HitsX);
				m_HitsX.Release();
				sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(m_Critical);
				m_Critical.Release();
				sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(m_Miss);
				m_Miss.Release();
				sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(startBar_);
				startBar_.Release();
				sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(endBar_);
				endBar_.Release();
			}
		}
	}
}
