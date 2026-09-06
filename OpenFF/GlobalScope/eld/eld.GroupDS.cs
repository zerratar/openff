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
		public class GroupDS : IGroup
		{
			private short _sDisp;

			private ds.Vector4<float> _vFadeColor = new ds.Vector4<float>();

			private ds.Vector3<int> _vGravity;

			~GroupDS()
			{
				destruct();
			}

			public void destruct()
			{
			}

			public override void create(ImpBaseParticle parent)
			{
				base.create(parent);
				ImpParticleDS impParticleDS = (ImpParticleDS)parent;
				IParticleSetup iSetupParam = parent.getISetupParam();
				IParticleExec iExecuteParam = parent.getIExecuteParam();
				ParticleDSSetup setupParam = ((ImpParticleDS)parent).getSetupParam();
				MtxFx43 mtxFx = new MtxFx43();
				RangeController rangeController = impParticleDS.getRangeController();
				SizeController sizeController = impParticleDS.getSizeController();
				SpeedController speedController = impParticleDS.getSpeedController();
				EmmitController emmitController = impParticleDS.getEmmitController();
				GravityController gravityController = new GravityController(setupParam.suGravity);
				CircleController circleController = impParticleDS.getCircleController();
				int num = 0;
				for (int i = 0; i < iSetupParam.nbChilds; i++)
				{
					ParticleDS particleDS = (ParticleDS)getParticle((uint)num);
					ds.pt.Particle primitive = particleDS.getPrimitive();
					rangeController.getCreatePosition(primitive);
					emmitController.getEmmitTransform(mtxFx);
					speedController.getSpeed(particleDS._vVelocity, mtxFx);
					gravityController.getGravity(particleDS._vGravity);
					if (!parent.isOffsetMove())
					{
						primitive.Center[0] += iExecuteParam.posBase.vx;
						primitive.Center[1] += iExecuteParam.posBase.vy;
						primitive.Center[2] += iExecuteParam.posBase.vz;
					}
					particleDS.setCenterPosition(primitive.Center);
					particleDS.setCircleParam(circleController.getStartRadius(), circleController.getRandomAngle());
					primitive.Size.vx = (primitive.Size.vy = (short)(particleDS._enSize = (short)sizeController.getSize()));
					primitive.Disp = 3;
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
				_sDisp = 3;
				_usTimeLife = 0;
				_bLife = true;
			}

			public override void update(ImpBaseParticle parent)
			{
				base.update(parent);
				int num = 0;
				ImpParticleDS impParticleDS = (ImpParticleDS)parent;
				AfterImageController afterImageController = impParticleDS.getAfterImageController();
				CircleController circleController = impParticleDS.getCircleController();
				if (_usTimeLife++ > impParticleDS.getISetupParam().usTimeGroupLife + impParticleDS.getSetupParam().suAfter.usNbAfters)
				{
					_bLife = false;
					return;
				}
				if (_usTimeLife == impParticleDS.getISetupParam().usTimeGroupLife)
				{
					_sDisp = 0;
				}
				if (impParticleDS.isFade())
				{
					impParticleDS.getFadeController().getFadeColor(_vFadeColor, _usTimeLife);
				}
				else
				{
					_vFadeColor.zero();
				}
				ushort num2 = (ushort)(impParticleDS.getSetupParam().suAfter.usNbAfters + 1);
				spr.EffSprForm animation = getAnimation();
				ds.Vector4<float> lastColorSub = afterImageController.getLastColorSub();
				float num3 = 1f / (float)(int)num2;
				spr.Eff_FRGBA eff_FRGBA = new spr.Eff_FRGBA(EffClamp((float)animation.fColData.nR + _vFadeColor.cr, 0f, 31f), EffClamp((float)animation.fColData.nB + _vFadeColor.cb, 0f, 31f), EffClamp((float)animation.fColData.nG + _vFadeColor.cg, 0f, 31f), EffClamp((float)animation.fColData.nA + _vFadeColor.ca, 0f, 31f));
				spr.Eff_FRGBA eff_FRGBA2 = new spr.Eff_FRGBA((eff_FRGBA.red - EffClamp(eff_FRGBA.red + lastColorSub.cr, 0f, 31f)) * num3, (eff_FRGBA.blue - EffClamp(eff_FRGBA.blue + lastColorSub.cb, 0f, 31f)) * num3, (eff_FRGBA.green - EffClamp(eff_FRGBA.green + lastColorSub.cg, 0f, 31f)) * num3, (eff_FRGBA.alpha - EffClamp(eff_FRGBA.alpha + lastColorSub.ca, 0f, 31f)) * num3);
				ds.Vector3<int> vector = new ds.Vector3<int>();
				vector.set(0, 0, 0);
				int num4 = 0;
				while (num4 < _nbParticles)
				{
					ParticleDS particleDS = (ParticleDS)getParticle((uint)num);
					int num5 = num + impParticleDS.getSetupParam().suAfter.usNbAfters;
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
					particleDS.addCircleRadius(circleController.getFrameAddRadius());
					particleDS.addCircleAngle(circleController.getFrameAddAngle());
					particleDS.update(this, vector, eff_FRGBA);
					particleDS.getPrimitive().Disp = _sDisp;
					num4++;
					num += num2;
				}
			}

			public override IParticle getParticle(uint index)
			{
				return ((ParticleDS[])_pParticles)[index];
			}

			public ds.Vector4<float> getFadeColor()
			{
				return _vFadeColor;
			}

			public ds.Vector3<int> getGravity()
			{
				return _vGravity;
			}
		}
	}
}
