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
		public class ImpBaseParticleLarge : IObject
		{
			protected RangeController _ctrlRange = new RangeController();

			protected SizeController _ctrlSize = new SizeController();

			protected IParticleSetup _isetup;

			protected IParticleExec _iexec = new IParticleExec();

			protected ds.Texture _tex;

			protected spr.Eff_AnimationHeader _sprite;

			protected IGroupLarge[] _group;

			protected IParticleLarge[] _particle;

			protected ds.pt.LargeParticle[] _primitive;

			protected float fTrans_;

			protected ds.sys3d.LargeParticleElement _elem;

			public ImpBaseParticleLarge()
			{
				_isetup = null;
				_tex = null;
				_group = null;
				_particle = null;
				_primitive = null;
				_elem = null;
			}

			~ImpBaseParticleLarge()
			{
			}

			public override bool Initialize(IGL rGL)
			{
				_ctrlRange.initialize(_isetup.suRange);
				_ctrlSize.initialize(_isetup.suSize);
				_sprite = m_pTemplate.GetAnimAddr<spr.Eff_AnimationHeader>();
				_iexec.state = 0;
				_iexec.usTimeNextGroup = _isetup.usTimeInterval;
				return true;
			}

			public override bool Calculate()
			{
				if (_elem != null && isOffsetMove())
				{
					_elem.setCenterPosition(_iexec.posBase);
				}
				switch (_iexec.state)
				{
				case 0:
					statePlay();
					break;
				case 1:
					stateWaitEnd();
					break;
				}
				return isPlay();
			}

			public override bool Render(IGL rGL)
			{
				return true;
			}

			public override bool Terminate()
			{
				return true;
			}

			public override void setDrawFlag(bool flag)
			{
			}

			public override bool getDrawFlag()
			{
				return true;
			}

			public override void Pause()
			{
			}

			public override bool Restart()
			{
				return false;
			}

			public override void SetPosition(ds.Vector3<int> pos)
			{
				_iexec.posBase.copy(pos);
			}

			public override void SetPosition(int x, int y, int z)
			{
				_iexec.posBase.set(x, y, z);
			}

			public void GetPosition(ds.Vector3<int> pos)
			{
				pos.copy(_iexec.posBase);
			}

			public void GetPosition(ref int x, ref int y, ref int z)
			{
				x = _iexec.posBase.vx;
				y = _iexec.posBase.vy;
				z = _iexec.posBase.vz;
			}

			public override void SetScale(ds.Vector3<int> scl)
			{
			}

			public override void SetScale(int x, int y, int z)
			{
			}

			public override void GetScale(ds.Vector3<int> scl)
			{
			}

			public virtual void statePlay()
			{
				_iexec.usTimeLife++;
				_iexec.usTimeNextGroup++;
				if (_iexec.usTimeNextGroup >= _isetup.usTimeInterval && _iexec.numCurrentGroup < _isetup.nbGroups)
				{
					getGroup(_iexec.numCurrentGroup).create(this);
					_iexec.numCurrentGroup++;
					_iexec.usTimeNextGroup = 0;
				}
				for (int i = 0; i < _isetup.nbGroups; i++)
				{
					if (getGroup((uint)i).isPlay())
					{
						getGroup((uint)i).update(this);
					}
				}
				if (_iexec.usTimeLife >= _isetup.usTimePlay)
				{
					if (isLoop() & (CheckStop() == 0))
					{
						_iexec.numCurrentGroup = 0;
						_iexec.usTimeLife = 0;
						_iexec.usTimeNextGroup = 0;
						_iexec.state = 0;
					}
					else
					{
						_iexec.state = 1;
					}
				}
			}

			public virtual void stateWaitEnd()
			{
				for (int i = 0; i < _isetup.nbGroups; i++)
				{
					if (getGroup((uint)i).isPlay())
					{
						getGroup((uint)i).update(this);
					}
				}
				int index = ((_iexec.numCurrentGroup != 0) ? (_iexec.numCurrentGroup - 1) : 0);
				if (!getGroup((uint)index).isPlay())
				{
					StopToDead();
					_iexec.bPlay = false;
				}
			}

			public ds.sys3d.LargeParticleElement createElement(ds.pt.LargeParticle[] pPrims, ushort usNbPrims, ds.Texture pTexture)
			{
				DSGL dSGL = (DSGL)IServer.Instance().getIGL();
				return dSGL.createLargeParticleElement(reinterpret_cast<ds.pt.LargeParticle[]>(pPrims), usNbPrims, pTexture);
			}

			public void deleteElement(ds.sys3d.LargeParticleElement pElem)
			{
				if (pElem != null)
				{
					DSGL dSGL = (DSGL)IServer.Instance().getIGL();
					dSGL.deleteLargeParticleElement(pElem);
				}
			}

			public virtual bool prepare()
			{
				return true;
			}

			public override bool isPlay()
			{
				return _iexec.bPlay;
			}

			public override bool isLoop()
			{
				if ((_isetup.flag & 1) == 0)
				{
					return false;
				}
				return true;
			}

			public virtual bool isAfterImage()
			{
				if ((_isetup.flag & 2) == 0)
				{
					return false;
				}
				return true;
			}

			public virtual bool isFade()
			{
				if ((_isetup.flag & 4) == 0)
				{
					return false;
				}
				return true;
			}

			public bool isOffsetMove()
			{
				if ((_isetup.flag & 8) == 0)
				{
					return false;
				}
				return true;
			}

			public spr.Eff_AnimationHeader getSpriteData()
			{
				return _sprite;
			}

			public IParticleSetup getISetupParam()
			{
				return _isetup;
			}

			public IParticleExec getIExecuteParam()
			{
				return _iexec;
			}

			public ushort getNextID()
			{
				return (ushort)g_PolyID.publishID();
			}

			public override void setTransparency(float fTrans)
			{
				fTrans_ = fTrans;
			}

			public override float getTransparency()
			{
				return fTrans_;
			}

			public RangeController getRangeController()
			{
				return _ctrlRange;
			}

			public SizeController getSizeController()
			{
				return _ctrlSize;
			}

			protected virtual IGroupLarge getGroup(uint index)
			{
				return _group[index];
			}

			protected virtual bool allocateWork()
			{
				return true;
			}

			protected virtual void deallocateWork()
			{
			}
		}
	}
}
