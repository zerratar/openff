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
	internal delegate void OSAlarmHandler(Array arg0);

	internal delegate void OSIrqFunction();

	internal delegate void MIDmaCallback(Array arg1);

	internal delegate void SNDAlarmHandler(object dummy);

	internal delegate int CARDPulledOutCallback();

	internal delegate void PreCallback(Array arg0);

	internal delegate void NNSFndHeapVisitor(Array memBlock, int heap, uint userParam);

	internal delegate int NNSG2dOamRegisterFunction(GXOamAttr arg0, ushort arg1, int arg2);

	internal delegate ushort NNSG2dAffineRegisterFunction(MtxFx22 arg0);

	internal delegate void NNSG2dTagCallback(ushort c, NNSG2dTagCallbackInfo pInfo);

	internal delegate int NNSG3dFuncAnmBlendMat(NNSG3dMatAnmResult arg0, NNSG3dAnmObj arg1, uint arg2);

	internal delegate int NNSG3dFuncAnmBlendJnt(NNSG3dJntAnmResult arg0, NNSG3dAnmObj arg1, uint arg2);

	internal delegate int NNSG3dFuncAnmBlendVis(NNSG3dVisAnmResult arg0, NNSG3dAnmObj arg1, uint arg2);

	internal delegate void NNSG3dSbcCallBackFunc(NNSG3dRS arg0);

	internal delegate void NNSSndStrmCallback(NNSSndStrmCallbackStatus status, int numChannles, Array[] buffer, uint len, int format, Array arg);

	internal delegate int NNSSndArcStrmCallback(int status, NNSSndArcStrmCallbackInfo info, int param, Array arg);

	internal delegate void NNSSndHeapDisposeCallback(Array mem, uint size, uint data1, uint data2);

	internal delegate void CtrlCodeProcessor(string code, char[] dest, int iDestOffset);

	internal delegate void SCRIPT_COMMAND(ScriptEngine engine);

	internal delegate void SWCFileStorageCallback(int arg0, int arg1, Array arg2);

	internal delegate Array _func1(int arg0, uint arg1, int arg2);

	internal delegate void _DeallocatorForDWC(int arg0, Array arg1, uint arg2);

	internal delegate void _cb_login(int arg0, int arg1, Array arg2);

	internal delegate void _cb_connect(int arg0, int arg1, Array arg2);

	internal delegate void _cb_close(int arg0, int arg1, int arg2, byte arg3, int arg4, Array arg5);

	internal delegate void _cb_send(int arg0, byte arg1);

	internal delegate void _cb_recv(byte arg0, byte[] arg1, int arg2);

	internal delegate void _cb_export(int arg0, int arg1, Array arg2);

	internal delegate void _cb_import(int arg0, int arg1, Array arg2);

	internal delegate void _cb_load(int arg0, int arg1, Array arg2);

	internal delegate void _cb_import_count(int arg0, int arg1, Array arg2);

	internal delegate void _cb_friendlist_update_end(int arg0, int arg1, Array arg2);

	internal delegate void _cb_friendstatus_update(int arg0, byte arg1, sbyte[] arg2, Array arg3);

	internal delegate void _cb_friendlist_delete(int arg0, int arg1, Array arg2);

	internal delegate void _cb_friendlist_set_buddy(int arg0, Array arg1);

	internal delegate void _cb_gsLogin(int arg0, Array arg1);

	internal delegate void _cb_gsSave(int success, int arg0, Array arg1);

	internal delegate void _cb_gsLoad(int arg0, int arg1, sbyte[] arg2, int arg3, Array arg4);

	internal delegate void _cb_storagelogin(int arg0, int arg1, Array arg2);

}
