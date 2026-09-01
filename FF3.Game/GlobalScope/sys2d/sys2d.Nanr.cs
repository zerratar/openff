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
	public static partial class sys2d
	{
		public class Nanr : NCData
		{
			public enum ANIMATION_HISTORY
			{
				ahNOW,
				ahBEFORE,
				ahMAX
			}

			private static int anSIZE = 1;

			private NNSG2dCellAnimation[] m_CellAnimation = new NNSG2dCellAnimation[1]
			{
				new NNSG2dCellAnimation()
			};

			private int m_Speed;

			public Nanr()
			{
				m_Speed = ds.S32toFX32(1);
			}

			public override bool Load(string pFile_name)
			{
				base.Load(pFile_name);
				if (m_pAnimation == null)
				{
					m_pAnimation = new NNSG2dAnimBankData();
				}
				NNS_G2dGetUnpackedAnimBank(pData(), m_pAnimation);
				return true;
			}

			public void Play(ushort index, NNSG2dAnimationPlayMode mode)
			{
				if (pDataAn() != null)
				{
					NNSG2dAnimSequenceData nNSG2dAnimSequenceData = NNS_G2dGetAnimSequenceByIdx(pDataAn(), index);
					if (nNSG2dAnimSequenceData != null)
					{
						NNS_G2dSetCellAnimationSequence(m_CellAnimation[0], nNSG2dAnimSequenceData);
						NNS_G2dStartAnimCtrl(NNS_G2dGetCellAnimationAnimCtrl(m_CellAnimation[0]));
					}
				}
			}

			public void ChangePlayMode(NNSG2dAnimationPlayMode mode)
			{
			}

			public void Update()
			{
				if (pDataAn() != null)
				{
					NNS_G2dTickCellAnimation(m_CellAnimation[0], m_Speed);
				}
			}

			public NNSG2dAnimBankData pDataAn()
			{
				return m_pAnimation;
			}

			public NNSG2dCellAnimation GetCellAnimation()
			{
				return m_CellAnimation[0];
			}

			public void copy(Nanr src)
			{
				copy((NCData)src);
				m_CellAnimation[0].copy(src.m_CellAnimation[0]);
				m_Speed = src.m_Speed;
			}
		}
	}
}
