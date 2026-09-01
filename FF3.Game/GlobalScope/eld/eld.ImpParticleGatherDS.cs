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
		public class ImpParticleGatherDS : ImpBaseParticle
		{
			protected FadeController _ctrlFade = new FadeController();

			protected AfterImageController _ctrlAfter = new AfterImageController();

			protected GatherController _ctrlGather = new GatherController();

			protected ParticleGatherDSSetup _setup;

			protected ParticleGatherDSExec _exec;

			~ImpParticleGatherDS()
			{
				destruct();
			}

			public void destruct()
			{
				deallocateWork();
			}

			public override bool prepare()
			{
				return allocateWork();
			}

			public override bool Initialize(IGL rGL)
			{
				base.Initialize(rGL);
				_ctrlFade.initialize(_setup.suFade);
				_ctrlAfter.initialize(_setup.suAfter);
				_ctrlGather.initialize(_setup.suGather);
				return true;
			}

			public override bool Calculate()
			{
				if (isPlay())
				{
					return base.Calculate();
				}
				return false;
			}

			public override bool Render(IGL rGL)
			{
				return base.Render(rGL);
			}

			public override bool Terminate()
			{
				deallocateWork();
				return true;
			}

			public override void setDrawFlag(bool flag)
			{
			}

			public override bool getDrawFlag()
			{
				return true;
			}

			public override IGroup getGroup(uint index)
			{
				return ((GroupGatherDS[])_group)[index];
			}

			public override void statePlay()
			{
				base.statePlay();
			}

			public override void stateWaitEnd()
			{
				base.stateWaitEnd();
			}

			public override bool allocateWork()
			{
				EffAllocator<ds.pt.Particle> effAllocator = new EffAllocator<ds.pt.Particle>();
				EffAllocator<GroupGatherDS> effAllocator2 = new EffAllocator<GroupGatherDS>();
				EffAllocator<ParticleGatherDS> effAllocator3 = new EffAllocator<ParticleGatherDS>();
				_setup = (ParticleGatherDSSetup)m_pTemplate.GetParameter();
				_isetup = _setup.isetup;
				_iexec.posBase.zero();
				_iexec.numCurrentGroup = 0;
				_iexec.usTimeLife = 0;
				_iexec.usTimeNextGroup = 0;
				_iexec.nextID = 0;
				_iexec.bPlay = true;
				ushort num = (ushort)(_isetup.nbChilds + _isetup.nbChilds * _setup.suAfter.usNbAfters);
				_iexec.nbParticles = (isAfterImage() ? ((ushort)(num * _isetup.nbGroups)) : ((ushort)(_isetup.nbChilds * _isetup.nbGroups)));
				if ((_group = effAllocator2.allocateMemoryArry(_isetup.nbGroups)) != null && (_particle = effAllocator3.allocateMemoryArry(_iexec.nbParticles)) != null && (_primitive = effAllocator.allocateMemoryArry(_iexec.nbParticles)) != null)
				{
					_tex = IServer.Instance().getIGL().CreateTexture(m_pTemplate.GetTextureAddr<ds.Texture>());
					if ((_elem = createElement(_primitive, _iexec.nbParticles, _tex)) != null)
					{
						GroupGatherDS[] array = (GroupGatherDS[])_group;
						ParticleGatherDS[] array2 = (ParticleGatherDS[])_particle;
						ds.pt.Particle[] primitive = _primitive;
						int num2 = 0;
						int num3 = 0;
						int num4 = 0;
						int num5 = 0;
						while (num5 < _isetup.nbGroups)
						{
							ParticleGatherDS[] array3 = new ParticleGatherDS[primitive.Length];
							for (int i = 0; i + num3 < primitive.Length; i++)
							{
								array3[i] = array2[num3 + i];
							}
							array[num2].setParticle(array3, _isetup.nbChilds);
							int num6 = 0;
							while (num6 < num)
							{
								array2[num3].setPrimitive(primitive[num4]);
								primitive[num4].Disp = 0;
								num6++;
								num3++;
								num4++;
							}
							num5++;
							num2++;
						}
						return true;
					}
				}
				deallocateWork();
				return false;
			}

			public override void deallocateWork()
			{
				EffAllocator<ds.pt.Particle> effAllocator = new EffAllocator<ds.pt.Particle>();
				EffAllocator<IGroup> effAllocator2 = new EffAllocator<IGroup>();
				EffAllocator<IParticle> effAllocator3 = new EffAllocator<IParticle>();
				if (_group != null)
				{
					effAllocator2.deallocateMemoryArry(_group);
					_group = null;
				}
				if (_particle != null)
				{
					effAllocator3.deallocateMemoryArry(_particle);
					_particle = null;
				}
				if (_primitive != null)
				{
					effAllocator.deallocateMemoryArry(_primitive);
					_primitive = null;
				}
				deleteElement(_elem);
				_elem = null;
			}

			public ParticleGatherDSSetup getSetupParam()
			{
				return _setup;
			}

			public ParticleGatherDSExec getExecuteParam()
			{
				return _exec;
			}

			public FadeController getFadeController()
			{
				return _ctrlFade;
			}

			public AfterImageController getAfterImageController()
			{
				return _ctrlAfter;
			}

			public GatherController getGatherController()
			{
				return _ctrlGather;
			}
		}
	}
}
