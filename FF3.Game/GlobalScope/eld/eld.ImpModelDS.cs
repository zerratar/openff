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
		public class ImpModelDS : IObject
		{
			protected ds.sys3d.CModelSet _dsModel = new ds.sys3d.CModelSet();

			protected ds.sys3d.CMotSet _dsMotion = new ds.sys3d.CMotSet();

			protected ds.sys3d.CAnimSet _dsAnimation = new ds.sys3d.CAnimSet();

			protected ds.sys3d.CRenderObject _dsRender = new ds.sys3d.CRenderObject();

			protected Array _pResModel;

			protected Array _pResTexture;

			protected ds.sys3d.ncap.SMotionFileHeader _pResMotion;

			protected Array _pResAnimation;

			protected ModelDSSetup _setup;

			protected ModelDSExec _exec = new ModelDSExec();

			public ImpModelDS()
			{
				_pResModel = null;
				_pResTexture = null;
				_pResMotion = null;
				_pResAnimation = null;
			}

			~ImpModelDS()
			{
				destruct();
			}

			public void destruct()
			{
				deallocateWork();
				releaseMotion();
			}

			public bool prepare()
			{
				_setup = (ModelDSSetup)m_pTemplate.GetParameter();
				_exec.bPlay = true;
				ModelDSNmdpHeader textureAddr = m_pTemplate.GetTextureAddr<ModelDSNmdpHeader>();
				ModelDSNcapHeader animAddr = m_pTemplate.GetAnimAddr<ModelDSNcapHeader>();
				ModelTexture unModelTexturePointer = textureAddr.unModelTexturePointer;
				_pResModel = textureAddr.unOffsetModel;
				_pResTexture = textureAddr.unOffsetTexture;
				_pResMotion = animAddr.unOffsetMotion;
				_pResAnimation = animAddr.unOffsetAnimation;
				VecFx32 scale = new VecFx32(_setup.vScale.vx, _setup.vScale.vy, _setup.vScale.vz);
				_dsModel.setup(_pResModel);
				_dsModel.bindReplaceTex(unModelTexturePointer);
				_dsRender.setup(_dsModel.getMdlResource());
				_dsRender.setShadowType(0u);
				_dsRender.setBoundingBox(_dsModel.getBoundingBox());
				_dsRender.setScale(scale);
				_dsMotion.setup(_dsModel.getMdlResource());
				_dsMotion.addRenderObject(_dsRender.getRenderObject());
				_dsMotion.addMotion(_pResMotion);
				_dsMotion.start(0u, isLoop(), 0u);
				if (_pResAnimation != null)
				{
					_dsAnimation.setup(_pResAnimation, _dsModel.getMdlResource(), unModelTexturePointer.getTextureResource());
					_dsAnimation.addRenderObject(_dsRender.getRenderObject());
					_dsAnimation.setLoop(b: true, ds.sys3d.CAnimSet.enTYPE.enTYPE_END);
					_dsAnimation.start(0, ds.sys3d.CAnimSet.enTYPE.enTYPE_END);
				}
				DSGL dSGL = (DSGL)IServer.Instance().getIGL();
				dSGL.addSkinModel(_dsRender);
				return allocateWork();
			}

			public override bool Initialize(IGL rGL)
			{
				return true;
			}

			public override bool Calculate()
			{
				if (!isPlay())
				{
					return false;
				}
				_dsMotion.next();
				if (_dsMotion.isEndOfMotion() && !isLoop())
				{
					_exec.bPlay = false;
					StopToDead();
				}
				if (_pResAnimation != null)
				{
					_dsAnimation.next();
				}
				return isPlay();
			}

			public override bool Render(IGL rGL)
			{
				return true;
			}

			public override bool Terminate()
			{
				deallocateWork();
				releaseMotion();
				return true;
			}

			public override void setDrawFlag(bool flag)
			{
			}

			public override bool getDrawFlag()
			{
				return true;
			}

			public override void SetPosition(ds.Vector3<int> pos)
			{
				_exec.vPosition.vx = pos.vx;
				_exec.vPosition.vy = pos.vy;
				_exec.vPosition.vz = pos.vz;
				_dsRender.setPosition(_exec.vPosition);
			}

			public override void SetPosition(int x, int y, int z)
			{
				_exec.vPosition.set(x, y, z);
				_dsRender.setPosition(_exec.vPosition);
			}

			public void GetPosition(VecFx32 pos)
			{
				pos.copy(_exec.vPosition);
			}

			public override void GetPosition(out int x, out int y, out int z)
			{
				x = _exec.vPosition.vx;
				y = _exec.vPosition.vy;
				z = _exec.vPosition.vz;
			}

			public override void SetRotationXYZ(ds.Vector3<int> rot)
			{
				_dsRender.setRotation((ushort)rot.vx, (ushort)rot.vy, (ushort)rot.vz);
			}

			public override void SetRotationXYZ(int x, int y, int z)
			{
				_dsRender.setRotation((ushort)x, (ushort)y, (ushort)z);
			}

			public bool allocateWork()
			{
				return true;
			}

			public void deallocateWork()
			{
			}

			public void releaseMotion()
			{
				DSGL dSGL = (DSGL)IServer.Instance().getIGL();
				dSGL.removeSkinModel(_dsRender);
				_dsModel.cleanup();
				_dsMotion.cleanup();
				_dsRender.cleanup();
				if (_pResAnimation != null)
				{
					_dsAnimation.cleanup();
				}
			}

			public bool OneTimeInit()
			{
				_exec.bPlay = true;
				return true;
			}

			public override bool isPlay()
			{
				return _exec.bPlay;
			}

			public override bool isLoop()
			{
				if ((_setup.flag & 1) == 0)
				{
					return false;
				}
				return true;
			}

			public ModelDSSetup getSetupParam()
			{
				return _setup;
			}

			public ModelDSExec getExecuteParam()
			{
				return _exec;
			}
		}
	}
}
