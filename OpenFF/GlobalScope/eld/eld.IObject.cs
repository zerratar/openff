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
		public class IObject
		{
			public static uint _s_nInvalidLogical = 0u;

			public static uint _s_nNextLogical = 1u;

			protected uint _code;

			protected IObject _pChildEffect;

			protected uint _uiStatus;

			protected uint _uiStatusOld;

			protected uint _uiCurrentCommandOld;

			protected Template m_pTemplate;

			protected uint[] _CommandBuff = new uint[CMD_BUFF_MAX];

			protected uint _CommandCount;

			protected uint _CommandTop;

			protected uint _uiStartWait;

			protected uint _nNoLogical;

			protected uint _nNoCategory;

			protected uint _nNoMember;

			public IObject()
			{
				_code = 1245859653u;
				_pChildEffect = null;
				_uiStatus = 0u;
				m_pTemplate = null;
				_nNoLogical = getNewLogicalIndex();
				FlushCommand();
			}

			~IObject()
			{
			}

			public uint GetTemplateID()
			{
				return m_pTemplate.getOwnID();
			}

			public void SetObjectCommand(uint com)
			{
				_CommandBuff[WrapRound(_CommandTop + _CommandCount, (uint)CMD_BUFF_MAX)] = com;
				_CommandCount++;
			}

			public void FlushCommand()
			{
				_CommandTop = 0u;
				_CommandCount = 0u;
			}

			public bool StartCheck()
			{
				if (_uiStartWait == 0)
				{
					return true;
				}
				_uiStartWait--;
				return false;
			}

			public bool Start(uint wait)
			{
				if (wait != 0)
				{
					SetObjectCommand(1u);
					_uiStartWait = wait;
				}
				else if (!Initialize(IServer.Instance().getIGL()))
				{
					Terminate();
					Ready();
				}
				else
				{
					SetStatus(1u);
				}
				return true;
			}

			public virtual void Pause()
			{
				if (_uiStatus != 2)
				{
					_uiStatusOld = _uiStatus;
					_uiCurrentCommandOld = _CommandBuff[_CommandTop];
					_uiStatus = 2u;
				}
			}

			public virtual bool Restart()
			{
				uint uiStatus = _uiStatus;
				if (uiStatus == 2)
				{
					_uiStatus = _uiStatusOld;
					_CommandBuff[_CommandTop] = _uiCurrentCommandOld;
					return true;
				}
				return true;
			}

			public uint GetCurrentCommand()
			{
				if (_CommandCount != 0)
				{
					return _CommandBuff[_CommandTop];
				}
				return 0u;
			}

			public void Advance()
			{
				_CommandCount--;
				_CommandTop = WrapRound(_CommandTop + 1, (uint)CMD_BUFF_MAX);
			}

			public virtual void DeleteObjReq()
			{
				StopToDead();
				if (_pChildEffect != null)
				{
					_pChildEffect.DeleteObjReq();
				}
			}

			public virtual void SetRotationXYZ(int _xAngle, int _yAngle, int _zAngle)
			{
			}

			public virtual void SetScale(int _xScale, int _yScale, int _zScale)
			{
			}

			public virtual void SetBright(int r, int g, int b, int _alpha)
			{
			}

			public virtual void SetRotationXYZ(ds.Vector3<int> Angle)
			{
			}

			public virtual void SetScale(ds.Vector3<int> scale)
			{
			}

			public virtual void GetScale(ds.Vector3<int> scale)
			{
				scale.set(4096, 4096, 4096);
			}

			public virtual void SetRotateMatrix(MtxFx43 mat)
			{
			}

			public virtual void DeleteObject()
			{
				FlushCommand();
				SetObjectCommand(16u);
				if (_pChildEffect != null)
				{
					_pChildEffect.DeleteObject();
				}
			}

			public uint getNewLogicalIndex()
			{
				uint s_nNextLogical = _s_nNextLogical;
				_s_nNextLogical++;
				if (_s_nNextLogical >= uint.MaxValue)
				{
					_s_nNextLogical = 1u;
				}
				return s_nNextLogical;
			}

			public virtual bool Initialize(IGL rGL)
			{
				return true;
			}

			public virtual bool Calculate()
			{
				return true;
			}

			public virtual bool Render(IGL rGL)
			{
				return true;
			}

			public virtual bool Terminate()
			{
				return true;
			}

			public virtual void setDrawFlag(bool flag)
			{
			}

			public virtual bool getDrawFlag()
			{
				return true;
			}

			public virtual bool isPlay()
			{
				return false;
			}

			public virtual bool isLoop()
			{
				return false;
			}

			public virtual void setNumberInformation(int ctgr, int mem)
			{
				_nNoCategory = (uint)ctgr;
				_nNoMember = (uint)mem;
			}

			public int getCategoryNo()
			{
				return (int)_nNoCategory;
			}

			public int getMemberNo()
			{
				return (int)_nNoMember;
			}

			public uint getLogicalIndex()
			{
				return _nNoLogical;
			}

			public virtual void SetPosition(ds.Vector3<int> pos)
			{
			}

			public virtual void SetPosition(int x, int y, int z)
			{
			}

			public virtual void GetPosition(out int x, out int y, out int z)
			{
				x = 0;
				y = 0;
				z = 0;
			}

			public virtual void setTransparency(float fTrans)
			{
			}

			public virtual float getTransparency()
			{
				return 0f;
			}

			public void SetTemplate(Template pTemp)
			{
				m_pTemplate = pTemp;
			}

			public uint GetStatus()
			{
				return _uiStatus;
			}

			public void SetStatus(uint status)
			{
				_uiStatus = status;
			}

			public Guid GetGUID()
			{
				return m_pTemplate.getFactoryGUID();
			}

			public void Stop()
			{
				SetObjectCommand(8u);
				SetObjectCommand(4u);
			}

			public void CarriageStop()
			{
				SetObjectCommand(8u);
			}

			public void Ready()
			{
				SetObjectCommand(0u);
			}

			protected uint CheckStop()
			{
				return _uiStatus & 8;
			}

			protected void StopToDead()
			{
				SetObjectCommand(8u);
				SetObjectCommand(4u);
				SetObjectCommand(16u);
			}
		}
	}
}
