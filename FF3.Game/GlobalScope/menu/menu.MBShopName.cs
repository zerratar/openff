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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class menu
	{
		public class MBShopName : MenuBehavior
		{
			public static dgs.UniqueNumber MBShopName_UN = new dgs.UniqueNumber();

			private dgs.DGSMessage pMsg;

			public MBShopName()
			{
				pMsg = null;
			}

			~MBShopName()
			{
				if (pMsg != null)
				{
					pMsg.release();
				}
				pMsg = null;
			}

			public override void bmInitialize(Medget M)
			{
				int[] array = new int[4] { 52000, 52001, 52002, 52003 };
				dgs.msg.CMessageMng.MSF_HANDLE_KIND font = dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8;
				dgs.DGSMessageManager dGSMessageManager = ((M.display() == 0) ? dgs.msg.CMessageSys.getInstance().Sub() : dgs.msg.CMessageSys.getInstance().Main());
				pMsg = dGSMessageManager.createMessage((uint)array[(int)shop.CShopManager.Instance().Kind()], dgs.INVALID_MSDHANDLE, (int)font);
				if (pMsg != null)
				{
					pMsg.setDisplaySpeed(byte.MaxValue);
					pMsg.setDisplayWait(0);
					ds.Vector2<short> vector = new ds.Vector2<short>();
					pMsg.getCompleteTextSize(vector);
					ushort num = (ushort)mbGetOwner().x();
					ushort num2 = (ushort)mbGetOwner().y();
					ushort num3 = (ushort)(mbGetOwner().width() - vector.vx);
					num3 /= 2;
					pMsg.setPosition((short)(num + num3), (short)(num2 + (mbGetOwner().height() - 12) / 2), erase: true);
				}
			}

			public override void bmBehave(Medget M)
			{
			}

			public override void bmFinalize(Medget M)
			{
				if (pMsg != null)
				{
					pMsg.release();
				}
				pMsg = null;
			}

			public new static int classIdentifier()
			{
				return MBShopName_UN.number();
			}

			public override object queryInterface(int class_id)
			{
				if (class_id == classIdentifier())
				{
					return this;
				}
				return null;
			}
		}
	}
}
