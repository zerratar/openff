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
	public static partial class mognet
	{
		public class MNNPCMailData
		{
			public static MNNPCMailData instance_ = new MNNPCMailData();

			private NPCMailData NPCMailData_ = new NPCMailData();

			public void initialize()
			{
			}

			public void finalize()
			{
			}

			public NPCMailState getNPCMailState(int No)
			{
				return (NPCMailState)NPCMailData_.receivedMail_[No];
			}

			public void setNPCMailState(int Npc, int No, NPCMailState State)
			{
			}

			public void setNPCMailState(int No, NPCMailState State)
			{
				NPCMailData_.receivedMail_[No] = (byte)State;
			}

			public void incSendTotal()
			{
				if (WIFI_MAIL_SEND_MAX > NPCMailData_.sendTotal_)
				{
					NPCMailData_.sendTotal_++;
				}
			}

			public int getSendTotal()
			{
				return NPCMailData_.sendTotal_;
			}

			public NPCMailActivity getNPCMailActivity(int Npc)
			{
				return NPCMailActivity.NPC_MAIL_ACTIVE;
			}

			public void setNPCMailActivity(int Npc, NPCMailActivity Activity)
			{
				NPCMailData_.npcMailActivity_ = (byte)((NPCMailData_.npcMailActivity_ & ~(1 << Npc)) | ((int)Activity << Npc));
			}

			public NPCReceiveState getNPCReceiveState(int Npc)
			{
				return (NPCReceiveState)((NPCMailData_.npcMailReceiveState_ >> Npc) & 1);
			}

			public void setNPCReceiveState(int Npc, NPCReceiveState ReceiveState)
			{
				NPCMailData_.npcMailReceiveState_ = (byte)((NPCMailData_.npcMailReceiveState_ & ~(1 << Npc)) | ((int)ReceiveState << Npc));
			}

			public void clearNPCMailData()
			{
				NPCMailData_.setDefault();
			}

			public void storeNPCMailData(NPCMailData pData)
			{
				pData.copy(NPCMailData_);
			}

			public void loadNPCMailData(NPCMailData pData)
			{
				NPCMailData_.copy(pData);
			}

			public static MNNPCMailData getSingleton()
			{
				return instance_;
			}
		}
	}
}
