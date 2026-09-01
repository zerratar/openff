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
		public class COverlay : CFlag<byte>
		{
			private static uint NUM_OVERLAY_MAX = 24u;

			private static MIProcessor DEFAULT_PROCESSOR = MIProcessor.MI_PROCESSOR_ARM9;

			private static byte ATTR_LOAD_END = 1;

			private COverlayObject[] objlist = new COverlayObject[NUM_OVERLAY_MAX];

			private uint _currentObjID;

			public COverlay()
			{
				_currentObjID = uint.MaxValue;
				for (int i = 0; i < objlist.Length; i++)
				{
					objlist[i] = new COverlayObject();
				}
			}

			public void Initialize()
			{
				_currentObjID = uint.MaxValue;
				LoadOverlay(objlist[0]);
				UnLoadOverlay(objlist[0]);
			}

			public void RegisterOverlay(int ovl_id, uint num)
			{
				if (num >= NUM_OVERLAY_MAX)
				{
					OS_Printf("WARNING num>=NUM_OVERLAY_MAX  nuym = %d \n", num);
				}
				else
				{
					objlist[num].setID(ovl_id);
				}
			}

			public void ChangeOverlay(uint ovl_id)
			{
				if (_currentObjID != ovl_id)
				{
					if (isFlag(ATTR_LOAD_END))
					{
						UnLoadOverlay(objlist[_currentObjID]);
					}
					LoadOverlay(objlist[ovl_id]);
					_currentObjID = ovl_id;
				}
			}

			public void LoadOverlay(int _id)
			{
				FS_LoadOverlay(DEFAULT_PROCESSOR, _id);
				onFlag(ATTR_LOAD_END);
			}

			public void UnLoadOverlay(int _id)
			{
				FS_UnloadOverlay(DEFAULT_PROCESSOR, _id);
				offFlag(ATTR_LOAD_END);
			}

			private void LoadOverlay(COverlayObject obj)
			{
				LoadOverlay(obj.getID());
			}

			private void UnLoadOverlay(COverlayObject obj)
			{
				UnLoadOverlay(obj.getID());
			}
		}
	}
}
