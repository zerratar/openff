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
		public class ImpSequenceDS : IObject
		{
			public delegate void _CommandFuncTbl();

			public _CommandFuncTbl[] CommandFuncTbl;

			private List m_BootIDList = new List();

			private List m_PathList = new List();

			private ds.Vector3<int> m_vOffset = new ds.Vector3<int>();

			private ds.Vector3<int> m_vBasePos = new ds.Vector3<int>();

			private MtxFx43 m_mRotate = new MtxFx43();

			private ds.Vector3<int> m_vScale = new ds.Vector3<int>();

			private READ_SEQ m_ReadSeq = new READ_SEQ();

			private SequencePathData m_PathData = new SequencePathData();

			private bool m_bPlay;

			private sbyte m_byFirst;

			public ImpSequenceDS()
			{
				CommandFuncTbl = new _CommandFuncTbl[6] { commnadWaitTime, commandSetPosition, commandBootEffect, commandHaltEffect, commandMovePath, commandDataEnd };
				m_vOffset.zero();
				m_vBasePos.zero();
				m_byFirst = 0;
				EffLoadIdentity(m_mRotate);
				m_vScale.set(4096, 4096, 4096);
			}

			~ImpSequenceDS()
			{
				destruct();
			}

			public void destruct()
			{
				m_BootIDList.EraseAll();
				EffAllocator<ImpSequencePath> effAllocator = new EffAllocator<ImpSequencePath>();
				for (uint num = 0u; num < m_PathList.getSize(); num++)
				{
					effAllocator.deallocate((ImpSequencePath)m_PathList.getValue(num));
				}
				m_PathList.EraseAll();
			}

			public bool OneTimeInit()
			{
				SequenceParameter sequenceParameter = (SequenceParameter)m_pTemplate.GetParameter();
				m_ReadSeq.pData = sequenceParameter.pSeqData;
				return true;
			}

			public void Execute()
			{
				uint num = 0u;
				uint num2 = 0u;
				for (uint num3 = 0u; num3 < m_PathList.getSize(); num3++)
				{
					ImpSequencePath impSequencePath = (ImpSequencePath)m_PathList.getValue(num2);
					impSequencePath.update(m_mRotate, m_vScale);
					if (impSequencePath.isPlay())
					{
						num |= 1;
						num2++;
						continue;
					}
					uint pathListIndex = GetPathListIndex(impSequencePath);
					m_BootIDList.Erase(m_BootIDList.getValue(pathListIndex));
					m_PathList.Erase(impSequencePath);
					EffAllocator<ImpSequencePath> effAllocator = new EffAllocator<ImpSequencePath>();
					effAllocator.deallocate(impSequencePath);
				}
				if (m_ReadSeq.wait_time == uint.MaxValue)
				{
					if (GetStatus() == 4)
					{
						StopNowPlayEffect();
					}
					if (num == 0)
					{
						StopToDead();
						m_bPlay = false;
					}
				}
				else
				{
					m_ReadSeq.wait_time--;
					while (m_ReadSeq.wait_time == 0 && m_ReadSeq.wait_time != uint.MaxValue)
					{
						CommandFuncTbl[m_ReadSeq.pData.getInt32()]();
					}
				}
			}

			public ImpSequencePath GetNowPlayObject(uint ID)
			{
				uint size = m_BootIDList.getSize();
				for (uint num = 0u; num < size; num++)
				{
					uint num2 = (uint)m_BootIDList.getValue(num);
					if (ID == num2)
					{
						return (ImpSequencePath)m_PathList.getValue(num);
					}
				}
				return null;
			}

			public uint GetPathListIndex(ImpSequencePath pPath)
			{
				uint size = m_PathList.getSize();
				for (uint num = 0u; num < size; num++)
				{
					ImpSequencePath impSequencePath = (ImpSequencePath)m_PathList.getValue(num);
					if (pPath == impSequencePath)
					{
						return num;
					}
				}
				return uint.MaxValue;
			}

			public uint StopNowPlayEffect()
			{
				for (uint num = 0u; num < m_PathList.getSize(); num++)
				{
					ImpSequencePath impSequencePath = (ImpSequencePath)m_PathList.getValue(num);
					if (impSequencePath.GetObject() != null && impSequencePath.GetObject().GetStatus() == 1)
					{
						impSequencePath.Stop();
					}
				}
				return 0u;
			}

			public override bool Initialize(IGL rGL)
			{
				m_ReadSeq.wait_time = 1u;
				m_bPlay = true;
				return true;
			}

			public override bool Calculate()
			{
				for (uint num = 0u; num < m_PathList.getSize(); num++)
				{
					ImpSequencePath impSequencePath = (ImpSequencePath)m_PathList.getValue(num);
					if (!impSequencePath.isPlay())
					{
						impSequencePath.DeleteObject();
					}
				}
				if (CheckStop() != 0)
				{
					SetStatus(4u);
				}
				Execute();
				if (m_ReadSeq.wait_time == uint.MaxValue)
				{
					return false;
				}
				return true;
			}

			public override bool Render(IGL rGL)
			{
				return true;
			}

			public override bool Terminate()
			{
				return true;
			}

			public override bool isPlay()
			{
				return m_bPlay;
			}

			public override void SetPosition(int x, int y, int z)
			{
				ds.Vector3<int> vector = new ds.Vector3<int>();
				vector.set(x, y, z);
				SetPosition(vector);
			}

			public override void SetPosition(ds.Vector3<int> pos)
			{
				m_vOffset.copy(pos);
				ds.Vector3<int> basePosition = new ds.Vector3<int>(m_vBasePos);
				basePosition += m_vOffset;
				for (uint num = 0u; num < m_PathList.getSize(); num++)
				{
					ImpSequencePath impSequencePath = (ImpSequencePath)m_PathList.getValue(num);
					impSequencePath.setBasePosition(basePosition);
				}
			}

			public override void GetPosition(out int x, out int y, out int z)
			{
				x = m_vOffset.vx;
				y = m_vOffset.vy;
				z = m_vOffset.vz;
			}

			public void GetPosition(ds.Vector3<int> pos)
			{
				pos.copy(m_vOffset);
			}

			public override void SetRotationXYZ(int xa, int ya, int za)
			{
				ds.Vector3<int> vector = new ds.Vector3<int>();
				vector.set(xa, ya, za);
				EffSetRotation(m_mRotate, vector);
			}

			public override void SetRotateMatrix(MtxFx43 mat)
			{
				m_mRotate.copy(mat);
			}

			public override void SetScale(int x, int y, int z)
			{
				m_vScale.set(x, y, z);
			}

			public override void SetScale(ds.Vector3<int> scale)
			{
				m_vScale.copy(scale);
			}

			public override bool isLoop()
			{
				SequenceParameter sequenceParameter = (SequenceParameter)m_pTemplate.GetParameter();
				if (checkSequenceFlag(sequenceParameter.uiFlag, enSEQ_FLAG.enSEQ_FLAG_LOOP) == 0)
				{
					return false;
				}
				return true;
			}

			public override void SetBright(int r, int g, int b, int _alpha)
			{
			}

			public void commnadWaitTime()
			{
				SEQ_WAIT_TIME sEQ_WAIT_TIME = (SEQ_WAIT_TIME)m_ReadSeq.pData;
				m_ReadSeq.wait_time = sEQ_WAIT_TIME.time;
			}

			public void commandSetPosition()
			{
				SEQ_SET_POSITION sEQ_SET_POSITION = (SEQ_SET_POSITION)m_ReadSeq.pData;
				m_vBasePos.set(FX_F32_TO_FX32(sEQ_SET_POSITION.pos[0]), FX_F32_TO_FX32(sEQ_SET_POSITION.pos[1]), FX_F32_TO_FX32(sEQ_SET_POSITION.pos[2]));
				ds.Vector3<int> basePosition = new ds.Vector3<int>(m_vBasePos);
				basePosition += m_vOffset;
				for (uint num = 0u; num < m_PathList.getSize(); num++)
				{
					ImpSequencePath impSequencePath = (ImpSequencePath)m_PathList.getValue(num);
					impSequencePath.setBasePosition(basePosition);
				}
			}

			public void commandBootEffect()
			{
				SEQ_BOOT_EFFECT sEQ_BOOT_EFFECT = (SEQ_BOOT_EFFECT)m_ReadSeq.pData;
				bool flag = true;
				if (m_byFirst != 0)
				{
					ImpSequencePath nowPlayObject = GetNowPlayObject(sEQ_BOOT_EFFECT._id);
					if (nowPlayObject != null && nowPlayObject.GetObject().isLoop())
					{
						flag = false;
					}
				}
				if (!flag)
				{
					return;
				}
				IObject obj = IServer.Instance().createObject(sEQ_BOOT_EFFECT.category, sEQ_BOOT_EFFECT.member);
				if (obj == null)
				{
					return;
				}
				if (!m_BootIDList.Add(sEQ_BOOT_EFFECT._id))
				{
					obj.DeleteObject();
					return;
				}
				ds.Vector3<int> vector = new ds.Vector3<int>();
				vector.copy(m_vOffset);
				vector.vx += FX_F32_TO_FX32(sEQ_BOOT_EFFECT.pos[0]);
				vector.vy += FX_F32_TO_FX32(sEQ_BOOT_EFFECT.pos[1]);
				vector.vz += FX_F32_TO_FX32(sEQ_BOOT_EFFECT.pos[2]);
				obj.Start(0u);
				obj.SetPosition(vector);
				EffAllocator<ImpSequencePath> effAllocator = new EffAllocator<ImpSequencePath>();
				ImpSequencePath impSequencePath = effAllocator.allocate(1u);
				if (impSequencePath != null)
				{
					SequenceParameter sequenceParameter = (SequenceParameter)m_pTemplate.GetParameter();
					m_PathData.SetData(sequenceParameter.pHeader);
					impSequencePath.SetData(m_PathData.GetPathData(sEQ_BOOT_EFFECT.path_id));
					impSequencePath.initialize(obj, m_vOffset);
					if (!m_PathList.Add(impSequencePath))
					{
						effAllocator.deallocate(impSequencePath);
						obj.DeleteObject();
					}
					else
					{
						impSequencePath.updatePositionS(m_mRotate, m_vScale);
						impSequencePath.updatePositionM(m_mRotate, 0, m_vScale);
					}
				}
				else
				{
					obj.DeleteObject();
				}
			}

			public void commandHaltEffect()
			{
				SEQ_HALT_EFFECT sEQ_HALT_EFFECT = (SEQ_HALT_EFFECT)m_ReadSeq.pData;
				GetNowPlayObject(sEQ_HALT_EFFECT._id)?.Stop();
			}

			public void commandMovePath()
			{
				_ = (SEQ_MOVE_PATH)m_ReadSeq.pData;
			}

			public void commandDataEnd()
			{
				if (GetStatus() == 4)
				{
					m_ReadSeq.wait_time = uint.MaxValue;
				}
				else if (!isLoop())
				{
					m_ReadSeq.wait_time = uint.MaxValue;
					SetStatus(4u);
				}
				else
				{
					m_ReadSeq.wait_time = 1u;
					OneTimeInit();
					m_byFirst = 1;
				}
			}
		}
	}
}
