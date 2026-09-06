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
	public static partial class eld
	{
		public static class spr
		{
			public class EffSprAnim
			{
				private ScaleAnimation m_SclAnim = new ScaleAnimation();

				private UVAnimation m_UVAnim = new UVAnimation();

				private ColorAnimation m_ClrAnim = new ColorAnimation();

				private Eff_AnimationHeader m_pSprAnim;

				public void SetData(Eff_AnimationHeader pData)
				{
					m_pSprAnim = pData;
					Initialize();
					GetSprFormData();
				}

				public void Initialize()
				{
					m_UVAnim.Initialize(m_pSprAnim);
					m_ClrAnim.Initialize(m_pSprAnim);
					m_SclAnim.Initialize(m_pSprAnim);
				}

				public void Update()
				{
					m_UVAnim.Update(m_pSprAnim.NumUVAnim);
					m_ClrAnim.Update(m_pSprAnim.NumClrAnim);
					m_SclAnim.Update(m_pSprAnim.NumSclAnim);
				}

				public static void ToAbsoluteAddress(Eff_AnimationHeader pData)
				{
				}

				public EffSprForm GetSprFormData()
				{
					m_UVAnim.GetData(sSprForm.uiUVData);
					m_ClrAnim.GetData(sSprForm.fColData, m_pSprAnim.NumClrAnim);
					m_SclAnim.GetData(sSprForm.fSclData, m_pSprAnim.NumSclAnim);
					return sSprForm;
				}

				public void GetSprFormData(EffSprForm SprForm)
				{
					m_UVAnim.GetData(SprForm.uiUVData);
					m_ClrAnim.GetData(SprForm.fColData, m_pSprAnim.NumClrAnim);
					m_SclAnim.GetData(SprForm.fSclData, m_pSprAnim.NumSclAnim);
				}

				~EffSprAnim()
				{
				}
			}

			public class UVAnimation
			{
				private Eff_UVAnimation m_pUVAnim;

				private short m_ssWait;

				private ushort m_usTblPos;

				private byte m_byGridX;

				private byte m_byGridY;

				public void Initialize(Eff_AnimationHeader pSprAnim)
				{
					m_byGridX = 0;
					m_byGridY = 0;
					m_usTblPos = 0;
					m_pUVAnim = pSprAnim.pUVAnim;
					m_ssWait = ((pSprAnim.NumUVAnim != 0) ? GetWaitData(m_usTblPos) : EFF_ANIMATION_END);
				}

				public void Update(uint TblMax)
				{
					if (m_ssWait == EFF_ANIMATION_END)
					{
						return;
					}
					if (m_ssWait <= 0)
					{
						if (m_usTblPos + 1 >= TblMax)
						{
							if (CheckLoopFlag(m_pUVAnim.unUvFlag) == 0)
							{
								m_ssWait = EFF_ANIMATION_END;
								return;
							}
							m_usTblPos = 0;
						}
						else
						{
							m_usTblPos++;
						}
						m_ssWait = GetCurrentUVWait();
						if (m_ssWait <= 0)
						{
							m_ssWait = 1;
						}
						SetGridNo(GetCurrentUVPtn());
					}
					m_ssWait--;
				}

				public void GetData(uint[] uiUV)
				{
					uiUV[0] = (uint)(m_pUVAnim.unStartU + m_pUVAnim.unSizeU * m_byGridX << 12);
					uiUV[1] = (uint)(m_pUVAnim.unStartV + m_pUVAnim.unSizeV * m_byGridY << 12);
					if (CheckInterpFlag(m_pUVAnim.unUvFlag) != 0)
					{
						float _slide = 0f;
						GetInterpValue(ref _slide);
						uiUV[0] += (uint)_slide;
					}
					uiUV[2] = (uint)(uiUV[0] + (m_pUVAnim.unSizeU << 12));
					uiUV[3] = (uint)(uiUV[1] + (m_pUVAnim.unSizeV << 12));
				}

				~UVAnimation()
				{
				}

				public short GetWaitData(ushort pos)
				{
					return (short)m_pUVAnim.pSeq[pos].unaUvTimeTbl;
				}

				private short GetCurrentUVWait()
				{
					return (short)m_pUVAnim.pSeq[m_usTblPos].unaUvTimeTbl;
				}

				private ushort GetCurrentUVPtn()
				{
					return m_pUVAnim.pSeq[m_usTblPos].unaUvNoTbl;
				}

				private void GetInterpValue(ref float _slide)
				{
					short currentUVWait = GetCurrentUVWait();
					if (m_pUVAnim.ucNumInterpolate == 0 || currentUVWait <= m_pUVAnim.ucNumInterpolate)
					{
						if (currentUVWait != 0)
						{
							float num = 1f - (float)m_ssWait / (float)currentUVWait;
							_slide = (float)(int)m_pUVAnim.unSizeU * num;
						}
						else
						{
							_slide = 0f;
						}
					}
					else
					{
						float num2 = ((m_pUVAnim.ucNumInterpolate == 0) ? ((float)(1 / currentUVWait)) : ((float)(1 / m_pUVAnim.ucNumInterpolate)));
						float num = (float)currentUVWait * num2;
						num = 1f - (float)m_ssWait / num;
						num *= num2;
					}
				}

				private void SetGridNo(ushort usPettern)
				{
					ushort num = (ushort)(m_pUVAnim.unTextureWidth / m_pUVAnim.unSizeU);
					_ = m_pUVAnim.unTextureHeight / m_pUVAnim.unSizeV;
					if (usPettern == 0)
					{
						m_byGridX = 0;
						m_byGridY = 0;
					}
					else if (num != 0)
					{
						m_byGridX = (byte)(usPettern % num);
						m_byGridY = (byte)(usPettern / num);
					}
					else
					{
						m_byGridX = 0;
						m_byGridY = 0;
					}
				}
			}

			public class ColorAnimation
			{
				private Eff_ColorAnimation m_pClrAnim;

				private ushort m_usTblPos;

				private int m_ssWait;

				private int _nInterpolate;

				public void Initialize(Eff_AnimationHeader pSprAnim)
				{
					m_usTblPos = 0;
					m_pClrAnim = pSprAnim.pClrAnim;
					m_ssWait = ((pSprAnim.NumClrAnim != 0) ? GetWaitData(m_usTblPos) : EFF_ANIMATION_END);
					if (CheckInterpFlag(m_pClrAnim.unClrFlag) != 0)
					{
						calculateInterpolate();
					}
				}

				public void Update(uint TblMax)
				{
					if (m_ssWait == EFF_ANIMATION_END)
					{
						return;
					}
					if (m_ssWait <= 0)
					{
						if (m_usTblPos + 1 >= TblMax)
						{
							if (CheckLoopFlag(m_pClrAnim.unClrFlag) == 0)
							{
								m_ssWait = EFF_ANIMATION_END;
								return;
							}
							m_usTblPos = 0;
						}
						else
						{
							m_usTblPos++;
						}
						m_ssWait = GetCurrentColorWait();
						if (m_ssWait <= 0)
						{
							m_ssWait = 1;
						}
						if (CheckInterpFlag(m_pClrAnim.unClrFlag) != 0)
						{
							calculateInterpolate();
						}
					}
					m_ssWait--;
				}

				public void GetData(Eff_FRGBA fRGBA, uint TblMax)
				{
					if (CheckInterpFlag(m_pClrAnim.unClrFlag) != 0)
					{
						ushort num = (ushort)(m_usTblPos + 1);
						if (num < TblMax)
						{
							int num2 = ((GetCurrentColorWait() != 0) ? (4096 - FX_Mul(m_ssWait, _nInterpolate)) : 0);
							Eff_FRGBA colorData = GetColorData(m_usTblPos);
							Eff_FRGBA colorData2 = GetColorData(num);
							fRGBA.nR = ((colorData2.nR - colorData.nR) * num2 >> 12) + colorData.nR;
							fRGBA.nG = ((colorData2.nG - colorData.nG) * num2 >> 12) + colorData.nG;
							fRGBA.nB = ((colorData2.nB - colorData.nB) * num2 >> 12) + colorData.nB;
							fRGBA.nA = ((colorData2.nA - colorData.nA) * num2 >> 12) + colorData.nA;
							return;
						}
					}
					Eff_ColorSeq[] pSeq = m_pClrAnim.pSeq;
					fRGBA.nR = pSeq[m_usTblPos].faRgbaTbl.nR;
					fRGBA.nG = pSeq[m_usTblPos].faRgbaTbl.nG;
					fRGBA.nB = pSeq[m_usTblPos].faRgbaTbl.nB;
					fRGBA.nA = pSeq[m_usTblPos].faRgbaTbl.nA;
				}

				public void calculateInterpolate()
				{
					_nInterpolate = FX_Div(4096, GetCurrentColorWait());
				}

				~ColorAnimation()
				{
				}

				public short GetWaitData(ushort pos)
				{
					return (short)m_pClrAnim.pSeq[pos].unaClrTimeTbl;
				}

				private short GetCurrentColorWait()
				{
					return (short)m_pClrAnim.pSeq[m_usTblPos].unaClrTimeTbl;
				}

				private Eff_FRGBA GetColorData(ushort pos)
				{
					return m_pClrAnim.pSeq[pos].faRgbaTbl;
				}
			}

			public class ScaleAnimation
			{
				private Eff_ScaleAnimation m_pSclAnim;

				private int _nWait;

				private ushort m_usTblPos;

				private int _nInterpolate;

				public void Initialize(Eff_AnimationHeader pSprAnim)
				{
					m_usTblPos = 0;
					m_pSclAnim = pSprAnim.pSclAnim;
					_nWait = ((pSprAnim.NumSclAnim != 0) ? GetWaitData(m_usTblPos) : EFF_ANIMATION_END);
					if (CheckInterpFlag(m_pSclAnim.unSclFlag) != 0)
					{
						calculateInterpolate();
					}
				}

				public void Update(uint TblMax)
				{
					if (_nWait == EFF_ANIMATION_END)
					{
						return;
					}
					if (_nWait <= 0)
					{
						if (m_usTblPos + 1 >= TblMax)
						{
							if (CheckLoopFlag(m_pSclAnim.unSclFlag) == 0)
							{
								_nWait = EFF_ANIMATION_END;
								return;
							}
							m_usTblPos = 0;
						}
						else
						{
							m_usTblPos++;
						}
						_nWait = GetCurrentScaleWait();
						if (_nWait <= 0)
						{
							_nWait = 1;
						}
						if (CheckInterpFlag(m_pSclAnim.unSclFlag) != 0)
						{
							calculateInterpolate();
						}
					}
					_nWait--;
				}

				public void GetData(Eff_FPoint point, uint TblMax)
				{
					if (CheckInterpFlag(m_pSclAnim.unSclFlag) != 0)
					{
						int num = 0;
						ushort num2 = (ushort)(m_usTblPos + 1);
						if (num2 < TblMax)
						{
							if (GetCurrentScaleWait() != 0)
							{
								num = 4096 - FX_Mul(_nWait, _nInterpolate);
							}
							Eff_FPoint scaleData = GetScaleData(m_usTblPos);
							Eff_FPoint scaleData2 = GetScaleData(num2);
							point.nx = ((scaleData2.nx - scaleData.nx) * num >> 12) + scaleData.nx;
							point.ny = ((scaleData2.ny - scaleData.ny) * num >> 12) + scaleData.ny;
							return;
						}
					}
					Eff_ScaleSeq[] pSeq = m_pSclAnim.pSeq;
					point.nx = pSeq[m_usTblPos].faSclRateTbl.nx;
					point.ny = pSeq[m_usTblPos].faSclRateTbl.ny;
				}

				public void calculateInterpolate()
				{
					_nInterpolate = FX_Div(4096, GetCurrentScaleWait());
				}

				~ScaleAnimation()
				{
				}

				public short GetWaitData(ushort pos)
				{
					return (short)m_pSclAnim.pSeq[pos].unaSclTimeTbl;
				}

				private short GetCurrentScaleWait()
				{
					return (short)m_pSclAnim.pSeq[m_usTblPos].unaSclTimeTbl;
				}

				private Eff_FPoint GetScaleData(ushort pos)
				{
					return m_pSclAnim.pSeq[pos].faSclRateTbl;
				}
			}

			public class Eff_FRGBA
			{
				public float red;

				public float blue;

				public float green;

				public float alpha;

				public int nR
				{
					get
					{
						return (int)red;
					}
					set
					{
						red = value;
					}
				}

				public int nB
				{
					get
					{
						return (int)blue;
					}
					set
					{
						blue = value;
					}
				}

				public int nG
				{
					get
					{
						return (int)green;
					}
					set
					{
						green = value;
					}
				}

				public int nA
				{
					get
					{
						return (int)alpha;
					}
					set
					{
						alpha = value;
					}
				}

				public static Eff_FRGBA operator +(Eff_FRGBA self, Eff_FRGBA col)
				{
					self.red += col.red;
					self.green += col.green;
					self.blue += col.blue;
					self.alpha += col.alpha;
					return self;
				}

				public static Eff_FRGBA operator -(Eff_FRGBA self, Eff_FRGBA col)
				{
					self.red -= col.red;
					self.green -= col.green;
					self.blue -= col.blue;
					self.alpha -= col.alpha;
					return self;
				}

				public Eff_FRGBA()
				{
				}

				public Eff_FRGBA(Eff_FRGBA src)
				{
					copy(src);
				}

				public Eff_FRGBA(float arg0, float arg1, float arg2, float arg3)
				{
					red = arg0;
					blue = arg1;
					green = arg2;
					alpha = arg3;
				}

				public static explicit operator Eff_FRGBA(ArrayReader src)
				{
					Eff_FRGBA eff_FRGBA = new Eff_FRGBA();
					eff_FRGBA.nR = src.readInt32();
					eff_FRGBA.nB = src.readInt32();
					eff_FRGBA.nG = src.readInt32();
					eff_FRGBA.nA = src.readInt32();
					return eff_FRGBA;
				}

				public void copy(Eff_FRGBA src)
				{
					red = src.red;
					blue = src.blue;
					green = src.green;
					alpha = src.alpha;
				}
			}

			public class Eff_FPoint
			{
				private float _x;

				private float _y;

				public int nx
				{
					get
					{
						return (int)_x;
					}
					set
					{
						_x = value;
					}
				}

				public int ny
				{
					get
					{
						return (int)_y;
					}
					set
					{
						_y = value;
					}
				}

				public static explicit operator Eff_FPoint(ArrayReader src)
				{
					Eff_FPoint eff_FPoint = new Eff_FPoint();
					eff_FPoint.nx = src.readInt32();
					eff_FPoint.ny = src.readInt32();
					return eff_FPoint;
				}

				public void copy(Eff_FPoint src)
				{
					_x = src._x;
					_y = src._y;
				}
			}

			public class Eff_UVAnimSeq
			{
				public ushort unaUvTimeTbl;

				public ushort unaUvNoTbl;

				public static explicit operator Eff_UVAnimSeq(ArrayReader src)
				{
					Eff_UVAnimSeq eff_UVAnimSeq = new Eff_UVAnimSeq();
					eff_UVAnimSeq.unaUvTimeTbl = src.readUInt16();
					eff_UVAnimSeq.unaUvNoTbl = src.readUInt16();
					return eff_UVAnimSeq;
				}
			}

			public class Eff_UVAnimation
			{
				public ushort unSizeU;

				public ushort unSizeV;

				public ushort unStartU;

				public ushort unStartV;

				public ushort unTextureWidth;

				public ushort unTextureHeight;

				public ushort ucNumInterpolate;

				public ushort usDummy1;

				public uint unUvFlag;

				public uint[] Reserve = new uint[3];

				public Eff_UVAnimSeq[] pSeq;

				public void parse(ArrayReader reader, uint NumUVAnim)
				{
					unSizeU = reader.readUInt16();
					unSizeV = reader.readUInt16();
					unStartU = reader.readUInt16();
					unStartV = reader.readUInt16();
					unTextureWidth = reader.readUInt16();
					unTextureHeight = reader.readUInt16();
					ucNumInterpolate = reader.readUInt16();
					usDummy1 = reader.readUInt16();
					unUvFlag = reader.readUInt32();
					reader.read(Reserve, 0, 3);
					pSeq = new Eff_UVAnimSeq[NumUVAnim];
					for (int i = 0; i < NumUVAnim; i++)
					{
						pSeq[i] = (Eff_UVAnimSeq)reader;
					}
				}
			}

			public class Eff_ScaleSeq
			{
				public uint unaSclTimeTbl;

				public Eff_FPoint faSclRateTbl;

				public uint reserve;

				public static explicit operator Eff_ScaleSeq(ArrayReader src)
				{
					Eff_ScaleSeq eff_ScaleSeq = new Eff_ScaleSeq();
					eff_ScaleSeq.unaSclTimeTbl = src.readUInt32();
					eff_ScaleSeq.faSclRateTbl = (Eff_FPoint)src;
					eff_ScaleSeq.reserve = src.readUInt32();
					return eff_ScaleSeq;
				}
			}

			public class Eff_ScaleAnimation
			{
				public uint unSclFlag;

				public uint[] Reserve = new uint[3];

				public Eff_ScaleSeq[] pSeq;

				public void parse(ArrayReader reader, uint NumSclAnim)
				{
					unSclFlag = reader.readUInt32();
					reader.read(Reserve, 0, 3);
					pSeq = new Eff_ScaleSeq[NumSclAnim];
					for (int i = 0; i < NumSclAnim; i++)
					{
						pSeq[i] = (Eff_ScaleSeq)reader;
					}
				}
			}

			public class Eff_ColorSeq
			{
				public uint unaClrTimeTbl;

				public uint[] reserve = new uint[3];

				public Eff_FRGBA faRgbaTbl;

				public static explicit operator Eff_ColorSeq(ArrayReader src)
				{
					Eff_ColorSeq eff_ColorSeq = new Eff_ColorSeq();
					eff_ColorSeq.unaClrTimeTbl = src.readUInt32();
					src.read(eff_ColorSeq.reserve, 0, 3);
					eff_ColorSeq.faRgbaTbl = (Eff_FRGBA)src;
					return eff_ColorSeq;
				}
			}

			public class Eff_ColorAnimation
			{
				public uint unClrFlag;

				public uint[] Reserve = new uint[3];

				public Eff_ColorSeq[] pSeq;

				public void parse(ArrayReader reader, uint NumClrAnim)
				{
					unClrFlag = reader.readUInt32();
					reader.read(Reserve, 0, 3);
					pSeq = new Eff_ColorSeq[NumClrAnim];
					for (int i = 0; i < NumClrAnim; i++)
					{
						pSeq[i] = (Eff_ColorSeq)reader;
					}
				}
			}

			public class Eff_AnimationHeader
			{
				public uint NumUVAnim;

				public uint NumSclAnim;

				public uint NumClrAnim;

				public uint uireserve1;

				public Eff_UVAnimation pUVAnim;

				public Eff_ScaleAnimation pSclAnim;

				public Eff_ColorAnimation pClrAnim;

				public uint uireserve2;

				public static Eff_AnimationHeader cast(Array src, uint offset)
				{
					Eff_AnimationHeader eff_AnimationHeader = new Eff_AnimationHeader();
					ArrayReader arrayReader = new ArrayReader(src);
					arrayReader.setPosition(offset);
					long position = arrayReader.getPosition();
					eff_AnimationHeader.NumUVAnim = arrayReader.readUInt32();
					eff_AnimationHeader.NumSclAnim = arrayReader.readUInt32();
					eff_AnimationHeader.NumClrAnim = arrayReader.readUInt32();
					eff_AnimationHeader.uireserve1 = arrayReader.readUInt32();
					int num = arrayReader.readInt32();
					int num2 = arrayReader.readInt32();
					int num3 = arrayReader.readInt32();
					eff_AnimationHeader.uireserve2 = arrayReader.readUInt32();
					arrayReader.setPosition(position + num);
					eff_AnimationHeader.pUVAnim = new Eff_UVAnimation();
					eff_AnimationHeader.pUVAnim.parse(arrayReader, eff_AnimationHeader.NumUVAnim);
					arrayReader.setPosition(position + num2);
					eff_AnimationHeader.pSclAnim = new Eff_ScaleAnimation();
					eff_AnimationHeader.pSclAnim.parse(arrayReader, eff_AnimationHeader.NumSclAnim);
					arrayReader.setPosition(position + num3);
					eff_AnimationHeader.pClrAnim = new Eff_ColorAnimation();
					eff_AnimationHeader.pClrAnim.parse(arrayReader, eff_AnimationHeader.NumClrAnim);
					arrayReader.dispose();
					return eff_AnimationHeader;
				}
			}

			public class EffSprForm
			{
				public uint[] uiUVData = new uint[4];

				public Eff_FRGBA fColData = new Eff_FRGBA();

				public Eff_FPoint fSclData = new Eff_FPoint();

				public void copy(EffSprForm src)
				{
					memcpy(uiUVData, src.uiUVData, 16);
					fColData.copy(src.fColData);
					fSclData.copy(src.fSclData);
				}
			}

			private static EffSprForm sSprForm = new EffSprForm();

			private static uint EFF_ANIMATION_FLAG_LOOP = 2147483648u;

			private static uint EFF_ANIMATION_FLAG_INTERPOLATE = 1073741824u;

			private static short EFF_ANIMATION_END = -1;

			internal static uint CheckLoopFlag(uint _flag)
			{
				return _flag & EFF_ANIMATION_FLAG_LOOP;
			}

			internal static uint CheckInterpFlag(uint _flag)
			{
				return _flag & EFF_ANIMATION_FLAG_INTERPOLATE;
			}
		}
	}
}
