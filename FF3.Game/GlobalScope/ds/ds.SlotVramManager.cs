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
	public static partial class ds
	{
		public class SlotVramManager
		{
			protected SlotVramManagerParam _prm;

			protected uint _unTopAddress;

			protected uint _unLastAddress;

			protected SLList<VramSendInfo> _listUse;

			public bool initialize(SlotVramManagerParam prm)
			{
				cleanup();
				uint num = 0u;
				switch (prm.enBankType)
				{
				case enVRAM_BANK.enVRAM_A:
				case enVRAM_BANK.enVRAM_B:
				case enVRAM_BANK.enVRAM_C:
				case enVRAM_BANK.enVRAM_D:
					num = 131072u;
					break;
				case enVRAM_BANK.enVRAM_E:
					num = 65536u;
					break;
				case enVRAM_BANK.enVRAM_F:
				case enVRAM_BANK.enVRAM_G:
				case enVRAM_BANK.enVRAM_I:
					num = 16384u;
					break;
				case enVRAM_BANK.enVRAM_H:
					num = 32768u;
					break;
				}
				_prm = prm;
				_unTopAddress = prm.unTopAddress + prm.unSlotNum * num;
				_unLastAddress = (prm.unSlotNum + 1) * num;
				return true;
			}

			public void cleanup()
			{
				while (_listUse.size() != 0)
				{
					SLNode<VramSendInfo> sLNode = _listUse.front();
					_listUse.erase(sLNode);
					deallocate(sLNode.data());
				}
			}

			public VramSendInfo allocate(VramAllocInfo infoAlloc)
			{
				VramSendInfo vramSendInfo = null;
				uint num = infoAlloc.unSize + (240 - (infoAlloc.unSize & 0xF));
				if (_listUse.size() >= _prm.unNbRegister)
				{
					return error("RegisterSize Over");
				}
				if (_listUse.size() == 0)
				{
					if (_unLastAddress - _unTopAddress < num)
					{
						return error("Fill");
					}
					if ((vramSendInfo = allocVramSendInfo()) == null)
					{
						return error("Enough Node Heap");
					}
					vramSendInfo.set(_unTopAddress, infoAlloc.unSize, num, comp: false);
					_listUse.insert(null, new SLNode<VramSendInfo>[1] { vramSendInfo.node() }, 1u);
					return vramSendInfo;
				}
				SLNode<VramSendInfo> sLNode = _listUse.front();
				VramSendInfo vramSendInfo2 = sLNode.data();
				if (_unTopAddress < vramSendInfo2.address() && num <= vramSendInfo2.address() - _unTopAddress)
				{
					if ((vramSendInfo = allocVramSendInfo()) == null)
					{
						return error("Enough Node Heap");
					}
					vramSendInfo.set(_unTopAddress, infoAlloc.unSize, num, comp: false);
					_listUse.insertFront(new SLNode<VramSendInfo>[1] { vramSendInfo.node() }, 1u);
					return vramSendInfo;
				}
				sLNode = _listUse.back();
				vramSendInfo2 = sLNode.data();
				if (_unLastAddress > vramSendInfo2.tale() && num <= _unLastAddress - vramSendInfo2.tale())
				{
					if ((vramSendInfo = allocVramSendInfo()) == null)
					{
						return error("Enough Node Heap");
					}
					vramSendInfo.set(vramSendInfo2.tale(), infoAlloc.unSize, num, comp: false);
					_listUse.insertBack(new SLNode<VramSendInfo>[1] { vramSendInfo.node() }, 1u);
					return vramSendInfo;
				}
				VramSendInfo vramSendInfo3 = _listUse.front().data();
				for (int i = 1; i < _listUse.size(); i++)
				{
					VramSendInfo vramSendInfo4 = vramSendInfo3;
					vramSendInfo3 = _listUse.get(i).data();
					if (num <= vramSendInfo3.address() - vramSendInfo4.tale())
					{
						if ((vramSendInfo = allocVramSendInfo()) == null)
						{
							return error("Enough Node Heap");
						}
						vramSendInfo.set(vramSendInfo4.tale(), infoAlloc.unSize, num, comp: false);
						_listUse.insert(_listUse.get(i), new SLNode<VramSendInfo>[1] { vramSendInfo.node() }, 1u);
						return vramSendInfo;
					}
				}
				return null;
			}

			public void deallocate(VramSendInfo pInfoSend)
			{
				for (int i = 0; i < _listUse.size(); i++)
				{
					SLNode<VramSendInfo> sLNode = _listUse.get(i);
					if (sLNode.data() == pInfoSend)
					{
						_listUse.erase(sLNode);
						deallocateVramSendInfo(pInfoSend);
						break;
					}
				}
			}

			public void dumpManageInformation()
			{
			}

			public VramSendInfo allocVramSendInfo()
			{
				VramSendInfo vramSendInfo = (VramSendInfo)CHeap.alloc_app(typeof(VramSendInfo));
				if (vramSendInfo == null)
				{
					return null;
				}
				return new VramSendInfo();
			}

			public void deallocateVramSendInfo(VramSendInfo pInfoSend)
			{
				if (pInfoSend != null)
				{
					pInfoSend.destruct();
					CHeap.free_app(pInfoSend);
				}
			}

			public VramSendInfo error(string szMsg)
			{
				return null;
			}

			~SlotVramManager()
			{
				cleanup();
			}
		}
	}
}
