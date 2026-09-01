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
	public static partial class eld
	{
		public class GroupGatherDS : IGroup
		{
			private short _sDisp;

			private ds.Vector4<float> _vFadeColor = new ds.Vector4<float>();

			~GroupGatherDS()
			{
				destruct();
			}

			public void destruct()
			{
			}

			public override void create(ImpBaseParticle parent)
			{
				base.create(parent);
				ImpParticleGatherDS impParticleGatherDS = (ImpParticleGatherDS)parent;
				IParticleSetup iSetupParam = parent.getISetupParam();
				ParticleGatherDSSetup setupParam = ((ImpParticleGatherDS)parent).getSetupParam();
				RangeController rangeController = impParticleGatherDS.getRangeController();
				SizeController sizeController = impParticleGatherDS.getSizeController();
				GatherController gatherController = impParticleGatherDS.getGatherController();
				int num = 0;
				for (int i = 0; i < iSetupParam.nbChilds; i++)
				{
					ParticleGatherDS particleGatherDS = (ParticleGatherDS)getParticle((uint)num);
					ds.pt.Particle primitive = particleGatherDS.getPrimitive();
					do
					{
						rangeController.getCreatePosition(primitive);
					}
					while (primitive.Center[0] + primitive.Center[1] + primitive.Center[2] == 0);
					gatherController.calculateGatherInfo(particleGatherDS._vSpeed, particleGatherDS._vAdd, out particleGatherDS._eLimitLength, primitive.Center);
					gatherController.createRotateMatrix(particleGatherDS._mtxRotate);
					particleGatherDS._bGather = true;
					particleGatherDS._eLength = 0;
					primitive.Size.vx = (primitive.Size.vy = (short)(particleGatherDS._enSize = sizeController.getSize()));
					primitive.Disp = 2;
					primitive.PolygonID = parent.getNextID();
					num++;
					int num2 = 0;
					while (num2 < setupParam.suAfter.usNbAfters)
					{
						ds.pt.Particle primitive2 = getParticle((uint)num).getPrimitive();
						primitive2.Disp = 0;
						primitive2.PolygonID = parent.getNextID();
						num2++;
						num++;
					}
				}
				_sDisp = 2;
				_usTimeLife = 0;
				_bLife = true;
			}

			public override void update(ImpBaseParticle parent)
			{
				base.update(parent);
				int num = 0;
				ImpParticleGatherDS impParticleGatherDS = (ImpParticleGatherDS)parent;
				AfterImageController afterImageController = impParticleGatherDS.getAfterImageController();
				GatherController gatherController = impParticleGatherDS.getGatherController();
				if (_usTimeLife++ > impParticleGatherDS.getISetupParam().usTimeGroupLife + impParticleGatherDS.getSetupParam().suAfter.usNbAfters)
				{
					_bLife = false;
					return;
				}
				if (_usTimeLife == impParticleGatherDS.getISetupParam().usTimeGroupLife)
				{
					_sDisp = 0;
				}
				if (impParticleGatherDS.isFade())
				{
					impParticleGatherDS.getFadeController().getFadeColor(_vFadeColor, _usTimeLife);
				}
				else
				{
					_vFadeColor.zero();
				}
				ushort num2 = (ushort)(impParticleGatherDS.getSetupParam().suAfter.usNbAfters + 1);
				spr.EffSprForm animation = getAnimation();
				ds.Vector4<float> lastColorSub = afterImageController.getLastColorSub();
				float num3 = 1f / (float)(int)num2;
				spr.Eff_FRGBA eff_FRGBA = new spr.Eff_FRGBA(EffClamp((float)animation.fColData.nR + _vFadeColor.cr, 0f, 31f), EffClamp((float)animation.fColData.nB + _vFadeColor.cb, 0f, 31f), EffClamp((float)animation.fColData.nG + _vFadeColor.cg, 0f, 31f), EffClamp((float)animation.fColData.nA + _vFadeColor.ca, 0f, 31f));
				spr.Eff_FRGBA eff_FRGBA2 = new spr.Eff_FRGBA((eff_FRGBA.red - EffClamp(eff_FRGBA.red + lastColorSub.cr, 0f, 31f)) * num3, (eff_FRGBA.blue - EffClamp(eff_FRGBA.blue + lastColorSub.cb, 0f, 31f)) * num3, (eff_FRGBA.green - EffClamp(eff_FRGBA.green + lastColorSub.cg, 0f, 31f)) * num3, (eff_FRGBA.alpha - EffClamp(eff_FRGBA.alpha + lastColorSub.ca, 0f, 31f)) * num3);
				int num4 = 0;
				while (num4 < _nbParticles)
				{
					ParticleGatherDS particleGatherDS = (ParticleGatherDS)getParticle((uint)num);
					int num5 = num + impParticleGatherDS.getSetupParam().suAfter.usNbAfters;
					spr.Eff_FRGBA color = new spr.Eff_FRGBA(eff_FRGBA);
					int num6 = num5;
					int num7 = num;
					while (num6 > num)
					{
						ds.pt.Particle primitive = getParticle((uint)num6).getPrimitive();
						ds.pt.Particle primitive2 = getParticle((uint)(num6 - 1)).getPrimitive();
						ds.pt.Particle primitive3 = getParticle((uint)num7).getPrimitive();
						primitive.Center[0] = primitive2.Center[0];
						primitive.Center[1] = primitive2.Center[1];
						primitive.Center[2] = primitive2.Center[2];
						primitive.Size.vx = primitive2.Size.vx;
						primitive.Size.vy = primitive2.Size.vy;
						ut.setColorToPrimitive(primitive3, color);
						primitive.St[0].vx = primitive2.St[0].vx;
						primitive.St[0].vy = primitive2.St[0].vy;
						primitive.St[1].vx = primitive2.St[1].vx;
						primitive.St[1].vy = primitive2.St[1].vy;
						primitive.Disp = primitive2.Disp;
						color -= eff_FRGBA2;
						num6--;
						num7++;
					}
					particleGatherDS.update(this, gatherController, eff_FRGBA);
					particleGatherDS.getPrimitive().Disp = _sDisp;
					num4++;
					num += num2;
				}
			}

			public override IParticle getParticle(uint index)
			{
				return ((ParticleGatherDS[])_pParticles)[index];
			}

			public ds.Vector4<float> getFadeColor()
			{
				return _vFadeColor;
			}
		}
	}
}
