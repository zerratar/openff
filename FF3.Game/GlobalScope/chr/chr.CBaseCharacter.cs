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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class chr
	{
		public class CBaseCharacter : dgs.CRestricted
		{
			public static int m_LookIndex;

			protected uint m_ShadowType;

			protected string m_ModelName;

			protected bool m_AutoPilot;

			protected int m_CharaID;

			protected CHARA_PARAMETER m_PreParam = new CHARA_PARAMETER();

			protected CHARA_PARAMETER m_Param = new CHARA_PARAMETER();

			public static void setLookIndex(int _LookIndex)
			{
				m_LookIndex = _LookIndex;
			}

			public static int getLookIndex()
			{
				return m_LookIndex;
			}

			public virtual void update()
			{
				m_PreParam.copy(m_Param);
			}

			public void setAutoPilot(bool _AutoPilot)
			{
				m_AutoPilot = _AutoPilot;
			}

			public void setMove(VecFx32 _Mass)
			{
				getParamMove().m_Mass.copy(_Mass);
			}

			public void setMoveAcc(int _MassAcc)
			{
				getParamMove().m_MassAcc = _MassAcc;
			}

			public void setMoveDec(int _MassDec)
			{
				getParamMove().m_MassDec = _MassDec;
			}

			public void setMoveMax(int _MassMax)
			{
				getParamMove().m_MassMax = _MassMax;
			}

			public void setTurn(VecFx32 _Mass)
			{
				getParamTurn().m_Mass.copy(_Mass);
			}

			public void setTurnAcc(int _MassAcc)
			{
				getParamTurn().m_MassAcc = _MassAcc;
			}

			public void setTurnDec(int _MassDec)
			{
				getParamTurn().m_MassDec = _MassDec;
			}

			public void setTurnMax(int _MassMax)
			{
				getParamTurn().m_MassMax = _MassMax;
			}

			public void setGrv(bool _GrvFlag)
			{
				getParamGrv().m_GrvFlag = _GrvFlag;
			}

			public void setGrvAcc(int _GrvAcc)
			{
				getParamGrv().m_GrvAcc = _GrvAcc;
			}

			public void setDirection(VecFx32 _Dir)
			{
				getParamObj().m_Dir.copy(_Dir);
			}

			public void setTargetDirection(VecFx32 _TargetDir)
			{
				getParamObj().m_TargetDir.copy(_TargetDir);
			}

			public void setPosition(VecFx32 _Pos)
			{
				getParamObj().m_Pos.copy(_Pos);
				characterMng.setPosition(m_CharaID, _Pos);
			}

			public void setRotation(VecFx32 _Rot)
			{
				getParamObj().m_Rot.copy(_Rot);
				characterMng.setRotation(m_CharaID, (ushort)_Rot.x, (ushort)_Rot.y, (ushort)_Rot.z);
			}

			public void setScale(VecFx32 _Scl)
			{
				getParamObj().m_Scl.copy(_Scl);
				characterMng.setScale(m_CharaID, _Scl);
			}

			public void setColType(int _ColType)
			{
				getColType_set(_ColType);
			}

			public void setColRadius(int _ColRadius)
			{
				getColRadius_set(_ColRadius);
			}

			public void setCckRadius(int _CckRadius)
			{
				getCckRadius_set(_CckRadius);
			}

			public void setTchRadius(int _TchRadius)
			{
				getTchRadius_set(_TchRadius);
			}

			public void setColOffset(VecFx32 _ColOffset)
			{
				getColOffset_set(_ColOffset);
			}

			public void setCckOffset(VecFx32 _CckOffset)
			{
				getCckOffset_set(_CckOffset);
			}

			public void setTchOffset(VecFx32 _TchOffset)
			{
				getTchOffset_set(_TchOffset);
			}

			public void setTargetDirectionFromRotation()
			{
				ds.Vector3<int> vRot = ds.FxToDsVector(getParamObj().m_Rot);
				VecFx32 vecFx = new VecFx32(0, 0, 4096);
				MtxFx43 m = new MtxFx43();
				ds.CpuMatrix.setRotate(m, vRot);
				MTX_MultVec43(vecFx, m, vecFx);
				if (!IS_ZERO_NORM(vecFx))
				{
					VEC_Normalize(vecFx, vecFx);
					vecFx.x /= 682;
					vecFx.y /= 682;
					vecFx.z /= 682;
				}
				getParamObj().m_TargetDir.copy(vecFx);
			}

			public void startMotion(int _Index, bool _Loop, uint _BlendFrame)
			{
				characterMng.startMotion(m_CharaID, _Index, _Loop, _BlendFrame);
			}

			public bool isEndOfMotion()
			{
				return characterMng.isEndOfMotion(m_CharaID);
			}

			public void setCurrentFrame(uint _frame)
			{
				characterMng.setCurrentFrame(m_CharaID, _frame);
			}

			public uint getCurrentFrame()
			{
				return characterMng.getCurrentFrame(m_CharaID);
			}

			public uint getMaxFrame()
			{
				return characterMng.getMaxFrame(m_CharaID);
			}

			public uint getMotionIndex()
			{
				return (uint)characterMng.getMotionIndex(m_CharaID);
			}

			public void setMotionSpeed(int _MotionSpeed)
			{
				characterMng.setMotionSpeed(m_CharaID, _MotionSpeed);
			}

			public int getMotionSpeed()
			{
				return characterMng.getMotionSpeed(m_CharaID);
			}

			public void setMotionLoop(bool loop)
			{
				characterMng.setMotionLoop(m_CharaID, loop);
			}

			public void setShadowType(uint _ShadowType)
			{
				characterMng.setShadowType(m_CharaID, (int)_ShadowType);
				m_ShadowType = _ShadowType;
			}

			public void setShadowScale(int _X, int _Y, int _Z)
			{
				VecFx32 shadowScale = new VecFx32(_X, _Y, _Z);
				setShadowScale(shadowScale);
			}

			public void setShadowScale(VecFx32 _ShadowScale)
			{
				characterMng.setShadowScale(m_CharaID, _ShadowScale);
			}

			public VecFx32 getShadowScale()
			{
				VecFx32 vecFx = new VecFx32(0, 0, 0);
				characterMng.getShadowScale(m_CharaID, vecFx);
				return vecFx;
			}

			public void setShadowAlpha(int _Alpha)
			{
				characterMng.setShadowAlpha(m_CharaID, (short)_Alpha);
			}

			public int getShadowAlpha()
			{
				return characterMng.getShadowAlpha(m_CharaID);
			}

			public void setLight(ds.sys3d.CLightObject _Light)
			{
				characterMng.setLight(m_CharaID, _Light);
			}

			public bool isHidden()
			{
				return characterMng.isHidden(m_CharaID);
			}

			public void setHidden(bool _Hidden)
			{
				characterMng.setHidden(m_CharaID, _Hidden);
			}

			public void setTransparency(int _Alpha)
			{
				characterMng.setTransparency(m_CharaID, _Alpha);
			}

			public int getTransparency()
			{
				return (int)characterMng.getTransparency(m_CharaID);
			}

			public void setTransparencyRate(int _Rate)
			{
				characterMng.setTransparencyRate(m_CharaID, _Rate);
			}

			public int getTransparencyRate()
			{
				return characterMng.getTransparencyRate(m_CharaID);
			}

			public VecFx32 getDirectionForRotY()
			{
				ushort idx = (ushort)getRotation().y;
				int sinVal = FX_SinIdx(idx);
				int cosVal = FX_CosIdx(idx);
				MtxFx33 mtxFx = new MtxFx33();
				VecFx32 vecFx = new VecFx32(0, 0, 4096);
				MTX_RotY33(mtxFx, sinVal, cosVal);
				MTX_MultVec33(vecFx, mtxFx, vecFx);
				VEC_Normalize(vecFx, vecFx);
				return vecFx;
			}

			public void setRotYForDirection(VecFx32 dir)
			{
				VecFx32 vecFx = new VecFx32(0, 0, 0);
				VEC_Normalize(dir, vecFx);
				ushort y = FX_Atan2Idx(vecFx.x, vecFx.z);
				getRotation().y = y;
			}

			public void setMass(int _MoveAcc, int _MoveDec, int _MoveMax, int _TurnAcc, int _TurnDec, int _TurnMax)
			{
				setMoveAcc(_MoveAcc);
				setMoveDec(_MoveDec);
				setMoveMax(_MoveMax);
				setTurnAcc(_TurnAcc);
				setTurnDec(_TurnDec);
				setTurnMax(_TurnMax);
			}

			public virtual void reset()
			{
				m_LookIndex = 0;
				m_AutoPilot = false;
				m_CharaID = -1;
				m_ShadowType = 0u;
				initAll();
			}

			public void initAll()
			{
				initPreParam();
				initParam();
			}

			public void initPreParam()
			{
				m_PreParam.init();
			}

			public void initParam()
			{
				m_Param.init();
			}

			public CBaseCharacter()
			{
				m_AutoPilot = false;
				m_CharaID = -1;
				initAll();
			}

			public void setModelName(string _ModelName)
			{
				strcpy(out m_ModelName, _ModelName);
			}

			public string getModelName()
			{
				return m_ModelName;
			}

			public bool getAutoPilot()
			{
				return m_AutoPilot;
			}

			public bool isAutoPilot()
			{
				return m_AutoPilot;
			}

			public void isAutoPilot_set(bool arg0)
			{
				m_AutoPilot = arg0;
			}

			public void setCharacterId(int _CharaID)
			{
				m_CharaID = _CharaID;
			}

			public int getCharacterId()
			{
				return m_CharaID;
			}

			public CHARA_PARAMETER getParam()
			{
				return m_Param;
			}

			public CHARA_MASS getParamMove()
			{
				return getParam().m_Mov;
			}

			public CHARA_MASS getParamTurn()
			{
				return getParam().m_Trn;
			}

			public CHARA_GRV getParamGrv()
			{
				return getParam().m_Grv;
			}

			public CHARA_OBJECT getParamObj()
			{
				return getParam().m_Obj;
			}

			public void getParamObj_set(CHARA_OBJECT arg0)
			{
				getParam().m_Obj.copy(arg0);
			}

			public CHARA_COLLISION getParamCol()
			{
				return getParam().m_Col;
			}

			public VecFx32 getMove()
			{
				return getParamMove().m_Mass;
			}

			public int getMoveAcc()
			{
				return getParamMove().m_MassAcc;
			}

			public int getMoveDec()
			{
				return getParamMove().m_MassDec;
			}

			public int getMoveMax()
			{
				return getParamMove().m_MassMax;
			}

			public VecFx32 getTurn()
			{
				return getParamTurn().m_Mass;
			}

			public int getTurnAcc()
			{
				return getParamTurn().m_MassAcc;
			}

			public int getTurnDec()
			{
				return getParamTurn().m_MassDec;
			}

			public int getTurnMax()
			{
				return getParamTurn().m_MassMax;
			}

			public bool isGrv()
			{
				return getParamGrv().m_GrvFlag;
			}

			public void isGrv_set(bool arg0)
			{
				getParamGrv().m_GrvFlag = arg0;
			}

			public int getGrvAcc()
			{
				return getParamGrv().m_GrvAcc;
			}

			public void getGrvAcc_set(int arg0)
			{
				getParamGrv().m_GrvAcc = arg0;
			}

			public VecFx32 getDirection()
			{
				return getParamObj().m_Dir;
			}

			public VecFx32 getTargetDirection()
			{
				return getParamObj().m_TargetDir;
			}

			public VecFx32 getPosition()
			{
				return getParamObj().m_Pos;
			}

			public void getPosition_set(VecFx32 arg0)
			{
				getParamObj().m_Pos.copy(arg0);
			}

			public VecFx32 getRotation()
			{
				return getParamObj().m_Rot;
			}

			public VecFx32 getScale()
			{
				return getParamObj().m_Scl;
			}

			public int getColFlag()
			{
				return getParamCol().m_ColFlag;
			}

			public void getColFlag_set(int arg0)
			{
				getParamCol().m_ColFlag = arg0;
			}

			public void getColFlag_or(int arg0)
			{
				getParamCol().m_ColFlag |= arg0;
			}

			public void getColFlag_not_and(int arg0)
			{
				getParamCol().m_ColFlag &= ~arg0;
			}

			public int getColType()
			{
				return getParamCol().m_ColType;
			}

			public void getColType_set(int arg0)
			{
				getParamCol().m_ColType = arg0;
			}

			public void getColType_or(int arg0)
			{
				getParamCol().m_ColType |= arg0;
			}

			public void getColType_not_and(int arg0)
			{
				getParamCol().m_ColType &= ~arg0;
			}

			public int getColRadius()
			{
				return getParamCol().m_ColRadius;
			}

			public void getColRadius_set(int arg0)
			{
				getParamCol().m_ColRadius = arg0;
			}

			public int getCckRadius()
			{
				return getParamCol().m_CckRadius;
			}

			public void getCckRadius_set(int arg0)
			{
				getParamCol().m_CckRadius = arg0;
			}

			public int getTchRadius()
			{
				return getParamCol().m_TchRadius;
			}

			public void getTchRadius_set(int arg0)
			{
				getParamCol().m_TchRadius = arg0;
			}

			public VecFx32 getColOffset()
			{
				return getParamCol().m_ColOffset;
			}

			public void getColOffset_set(VecFx32 arg0)
			{
				getParamCol().m_ColOffset.copy(arg0);
			}

			public VecFx32 getCckOffset()
			{
				return getParamCol().m_CckOffset;
			}

			public void getCckOffset_set(VecFx32 arg0)
			{
				getParamCol().m_CckOffset.copy(arg0);
			}

			public VecFx32 getTchOffset()
			{
				return getParamCol().m_TchOffset;
			}

			public void getTchOffset_set(VecFx32 arg0)
			{
				getParamCol().m_TchOffset.copy(arg0);
			}

			public VecFx32 getColAabbRadius()
			{
				return getParamCol().m_ColAabbRadius;
			}

			public void getColAabbRadius_set(VecFx32 arg0)
			{
				getParamCol().m_ColAabbRadius.copy(arg0);
			}

			public CHARA_PARAMETER getPreParam()
			{
				return m_PreParam;
			}

			public CHARA_MASS getPreParamMove()
			{
				return getPreParam().m_Mov;
			}

			public CHARA_MASS getPreParamTurn()
			{
				return getPreParam().m_Trn;
			}

			public CHARA_GRV getPreParamGrv()
			{
				return getPreParam().m_Grv;
			}

			public CHARA_OBJECT getPreParamObj()
			{
				return getPreParam().m_Obj;
			}

			public void getPreParamObj_set(CHARA_OBJECT arg0)
			{
				getPreParam().m_Obj.copy(arg0);
			}

			public CHARA_COLLISION getPreParamCol()
			{
				return getPreParam().m_Col;
			}

			public VecFx32 getPreMove()
			{
				return getPreParamMove().m_Mass;
			}

			public int getPreMoveAcc()
			{
				return getPreParamMove().m_MassAcc;
			}

			public int getPreMoveDec()
			{
				return getPreParamMove().m_MassDec;
			}

			public int getPreMoveMax()
			{
				return getPreParamMove().m_MassMax;
			}

			public VecFx32 getPreTurn()
			{
				return getPreParamTurn().m_Mass;
			}

			public int getPreTurnAcc()
			{
				return getPreParamTurn().m_MassAcc;
			}

			public int getPreTurnDec()
			{
				return getPreParamTurn().m_MassDec;
			}

			public int getPreTurnMax()
			{
				return getPreParamTurn().m_MassMax;
			}

			public bool isPreGrv()
			{
				return getPreParamGrv().m_GrvFlag;
			}

			public int getPreGrvAcc()
			{
				return getPreParamGrv().m_GrvAcc;
			}

			public VecFx32 getPreDirection()
			{
				return getPreParamObj().m_Dir;
			}

			public VecFx32 getPreTargetDirection()
			{
				return getPreParamObj().m_TargetDir;
			}

			public VecFx32 getPrePosition()
			{
				return getPreParamObj().m_Pos;
			}

			public void getPrePosition_set(VecFx32 arg0)
			{
				getPreParamObj().m_Pos.copy(arg0);
			}

			public VecFx32 getPreRotation()
			{
				return getPreParamObj().m_Rot;
			}

			public void getPreRotation_set(VecFx32 arg0)
			{
				getPreParamObj().m_Rot.copy(arg0);
			}

			public VecFx32 getPreScale()
			{
				return getPreParamObj().m_Scl;
			}

			public int isPreColType()
			{
				return getPreParamCol().m_ColType;
			}

			public int getPreColRadius()
			{
				return getPreParamCol().m_ColRadius;
			}

			public int getPreCckRadius()
			{
				return getPreParamCol().m_CckRadius;
			}

			public int getPreTchRadius()
			{
				return getPreParamCol().m_TchRadius;
			}

			public VecFx32 getPreColOffset()
			{
				return getPreParamCol().m_ColOffset;
			}

			public VecFx32 getPreCckOffset()
			{
				return getPreParamCol().m_CckOffset;
			}

			public VecFx32 getPreTchOffset()
			{
				return getPreParamCol().m_TchOffset;
			}

			public virtual void initialize()
			{
			}

			public virtual void execute()
			{
			}

			public virtual void terminate()
			{
			}
		}
	}
}
