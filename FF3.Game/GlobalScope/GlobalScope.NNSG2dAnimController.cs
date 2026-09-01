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
	public class NNSG2dAnimController
	{
		public NNSG2dAnimFrameData pCurrent;

		public NNSG2dAnimFrameData pActiveCurrent;

		public int bReverse;

		public int bActive;

		public int currentTime;

		public int speed;

		public NNSG2dAnimationPlayMode overriddenPlayMode;

		public NNSG2dAnimSequenceData pAnimSequence;

		public NNSG2dCallBackFunctor callbackFunctor;

		public int pCurrent_idx;

		public void copy(NNSG2dAnimController src)
		{
			pCurrent = src.pCurrent;
			pActiveCurrent = src.pActiveCurrent;
			bReverse = src.bReverse;
			bActive = src.bActive;
			currentTime = src.currentTime;
			speed = src.speed;
			overriddenPlayMode = src.overriddenPlayMode;
			pAnimSequence = src.pAnimSequence;
			callbackFunctor = src.callbackFunctor;
			pCurrent_idx = src.pCurrent_idx;
		}

		public void setDefault()
		{
			pCurrent = null;
			pActiveCurrent = null;
			bReverse = 0;
			bActive = 0;
			currentTime = 0;
			speed = 0;
			overriddenPlayMode = NNSG2dAnimationPlayMode.NNS_G2D_ANIMATIONPLAYMODE_INVALID;
			pAnimSequence = null;
			callbackFunctor = null;
			pCurrent_idx = 0;
		}
	}
}
