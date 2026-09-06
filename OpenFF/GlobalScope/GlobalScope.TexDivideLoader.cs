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
	public class TexDivideLoader : sys.CBlankTask
	{
		public class Handler
		{
			~Handler()
			{
			}

			public virtual void tdlhCompletion(int receipt_number)
			{
			}
		}

		public enum RESOURCE_TYPE
		{
			RT_TEXTURE = 0,
			RT_PALETTE = 1,
			RT_4X4_TEX = 2,
			RT_INVALID = -1
		}

		public class TDL_REQUEST
		{
			public RESOURCE_TYPE type;

			public Array source;

			public Array destination;

			public int size;

			public int number;

			public Handler handler;

			public TDL_REQUEST()
			{
				source = (destination = null);
				size = (number = 0);
				handler = null;
			}

			public TDL_REQUEST(RESOURCE_TYPE tp, Array src, Array dst, int siz, int num, Handler hdl)
			{
				type = tp;
				source = src;
				destination = dst;
				size = siz;
				number = num;
				handler = hdl;
			}
		}

		public const RESOURCE_TYPE RT_TEXTURE = RESOURCE_TYPE.RT_TEXTURE;

		public const RESOURCE_TYPE RT_PALETTE = RESOURCE_TYPE.RT_PALETTE;

		public const RESOURCE_TYPE RT_4X4_TEX = RESOURCE_TYPE.RT_4X4_TEX;

		public const RESOURCE_TYPE RT_INVALID = RESOURCE_TYPE.RT_INVALID;

		public static TexDivideLoader instance_ = new TexDivideLoader();

		public static int LIMIT_OF_REQUEST = 64;

		private static int MaxSize = 32768;

		private ds.Vector<TDL_REQUEST, ds.OrderSavedErasePolicy<TDL_REQUEST>> requests = new ds.Vector<TDL_REQUEST, ds.OrderSavedErasePolicy<TDL_REQUEST>>(LIMIT_OF_REQUEST);

		private int receipt_number;

		private bool activity;

		private bool isQueueing;

		public TexDivideLoader()
		{
			receipt_number = 10000;
			activity = false;
			isQueueing = false;
		}

		~TexDivideLoader()
		{
			tdlStop();
		}

		public void tdlStart()
		{
			beginVTask();
			activity = true;
		}

		public void tdlStop()
		{
			activity = false;
			endVTask();
		}

		public int tdlLoadTexRequest(Array src_ptr, Array dest_ptr, int size, Handler handler)
		{
			isQueueing = true;
			if (requests.size() + divr(size, MaxSize) > LIMIT_OF_REQUEST)
			{
				return -1;
			}
			TDL_REQUEST tDL_REQUEST = new TDL_REQUEST();
			tDL_REQUEST.type = RESOURCE_TYPE.RT_TEXTURE;
			tDL_REQUEST.source = src_ptr;
			tDL_REQUEST.destination = dest_ptr;
			tDL_REQUEST.size = size;
			tDL_REQUEST.number = receipt_number;
			tDL_REQUEST.handler = handler;
			receipt_number++;
			while (tDL_REQUEST.size > 0)
			{
				if (tDL_REQUEST.size > MaxSize)
				{
					requests.push_back(new TDL_REQUEST(tDL_REQUEST.type, tDL_REQUEST.source, tDL_REQUEST.destination, MaxSize, tDL_REQUEST.number, null));
					tDL_REQUEST.size -= MaxSize;
					continue;
				}
				requests.push_back(tDL_REQUEST);
				break;
			}
			if (!activity)
			{
				tdlStart();
			}
			isQueueing = false;
			return tDL_REQUEST.number;
		}

		public int tdlLoadPlttRequest(Array src_ptr, Array dest_ptr, int size, Handler handler)
		{
			isQueueing = true;
			if (requests.size() > LIMIT_OF_REQUEST)
			{
				return -1;
			}
			TDL_REQUEST tDL_REQUEST = new TDL_REQUEST();
			tDL_REQUEST.type = RESOURCE_TYPE.RT_PALETTE;
			tDL_REQUEST.source = src_ptr;
			tDL_REQUEST.destination = dest_ptr;
			tDL_REQUEST.size = size;
			tDL_REQUEST.number = receipt_number;
			tDL_REQUEST.handler = handler;
			requests.push_back(tDL_REQUEST);
			receipt_number++;
			if (!activity)
			{
				tdlStart();
			}
			isQueueing = false;
			return tDL_REQUEST.number;
		}

		public int tdlLoad4x4TexRequest(Array src_texel_ptr, Array src_plttIdx_ptr, Array dest_ptr, int size, Handler handler)
		{
			isQueueing = true;
			if (requests.size() + divr(size, MaxSize) > LIMIT_OF_REQUEST)
			{
				return -1;
			}
			TDL_REQUEST tDL_REQUEST = new TDL_REQUEST();
			tDL_REQUEST.type = RESOURCE_TYPE.RT_TEXTURE;
			tDL_REQUEST.source = src_texel_ptr;
			tDL_REQUEST.destination = dest_ptr;
			tDL_REQUEST.size = size;
			tDL_REQUEST.number = receipt_number;
			tDL_REQUEST.handler = null;
			receipt_number++;
			while (tDL_REQUEST.size > 0)
			{
				if (tDL_REQUEST.size > MaxSize)
				{
					requests.push_back(new TDL_REQUEST(tDL_REQUEST.type, tDL_REQUEST.source, tDL_REQUEST.destination, MaxSize, tDL_REQUEST.number, null));
					tDL_REQUEST.size -= MaxSize;
					continue;
				}
				requests.push_back(tDL_REQUEST);
				break;
			}
			TDL_REQUEST tDL_REQUEST2 = new TDL_REQUEST();
			tDL_REQUEST2.type = RESOURCE_TYPE.RT_TEXTURE;
			tDL_REQUEST2.source = src_plttIdx_ptr;
			tDL_REQUEST2.size = size >> 1;
			tDL_REQUEST2.number = receipt_number;
			tDL_REQUEST2.handler = handler;
			receipt_number++;
			while (tDL_REQUEST2.size > 0)
			{
				if (tDL_REQUEST2.size > MaxSize)
				{
					requests.push_back(new TDL_REQUEST(tDL_REQUEST2.type, tDL_REQUEST2.source, tDL_REQUEST2.destination, MaxSize, tDL_REQUEST2.number, null));
					tDL_REQUEST2.size -= MaxSize;
					continue;
				}
				requests.push_back(tDL_REQUEST2);
				break;
			}
			if (!activity)
			{
				tdlStart();
			}
			isQueueing = false;
			return tDL_REQUEST2.number;
		}

		public bool tdlIsEmpty()
		{
			return requests.empty();
		}

		public int tdlLoadResTexRequest(NNSG3dResTex src_ptr, Handler handler)
		{
			isQueueing = true;
			NNS_GfdGetTexKeyAddr(src_ptr.texInfo.vramKey);
			_ = src_ptr.texInfo.sizeTex;
			NNS_GfdGetTexKeyAddr(src_ptr.plttInfo.vramKey);
			_ = src_ptr.plttInfo.sizePltt;
			uint num = (uint)(src_ptr.tex4x4Info.sizeTex << 3);
			src_ptr.texInfo.flag |= 1;
			src_ptr.plttInfo.flag |= 1;
			isQueueing = false;
			return -1;
		}

		public void tdlForceLoad()
		{
			tdlStop();
			while (!requests.empty())
			{
				tdlLoad();
			}
		}

		public void tdlCancel()
		{
			tdlStop();
			requests.clear();
		}

		public void tdlLoad()
		{
			if (!requests.empty())
			{
				TDL_REQUEST tDL_REQUEST = requests[0];
				requests.erase(0);
				if (requests.size() <= 0)
				{
					tdlStop();
				}
				switch (tDL_REQUEST.type)
				{
				case RESOURCE_TYPE.RT_TEXTURE:
					GX_BeginLoadTex();
					GX_EndLoadTex();
					break;
				case RESOURCE_TYPE.RT_PALETTE:
					GX_BeginLoadTexPltt();
					GX_EndLoadTexPltt();
					break;
				}
				if (tDL_REQUEST.handler != null)
				{
					tDL_REQUEST.handler.tdlhCompletion(tDL_REQUEST.number);
				}
			}
		}

		public override void vbTask()
		{
			if (GX_GetVCount() <= 197 && GX_GetBankForTex() != GXVRamTex.GX_VRAM_TEX_NONE && GX_GetBankForTexPltt() != GXVRamTexPltt.GX_VRAM_TEXPLTT_NONE && !isQueueing)
			{
				tdlLoad();
			}
		}

		public static TexDivideLoader getSingleton()
		{
			return instance_;
		}

		public override void hbTask(ushort line)
		{
		}
	}
}
