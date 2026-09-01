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
	public static class npc
	{
		public class NpcManager
		{
			public enum NPC_BAD_STATE
			{
				NPC_BAD_STATE_NORMAL,
				NPC_BAD_STATE_FROG,
				NPC_BAD_STATE_LILLIPUT
			}

			public const NPC_BAD_STATE NPC_BAD_STATE_NORMAL = NPC_BAD_STATE.NPC_BAD_STATE_NORMAL;

			public const NPC_BAD_STATE NPC_BAD_STATE_FROG = NPC_BAD_STATE.NPC_BAD_STATE_FROG;

			public const NPC_BAD_STATE NPC_BAD_STATE_LILLIPUT = NPC_BAD_STATE.NPC_BAD_STATE_LILLIPUT;

			private NpcParameter npc_ = new NpcParameter();

			private NPC_BAD_STATE badState_;

			public void initialize()
			{
				npc_.offIsEnable();
				npc_.clearNpcId();
				setBadStateNormal();
			}

			public void terminate()
			{
			}

			public bool isEnable()
			{
				return npc_.isEnable();
			}

			public void onIsEnable()
			{
				npc_.onIsEnable();
			}

			public void offIsEnable()
			{
				npc_.offIsEnable();
			}

			public byte npcId()
			{
				return npc_.npcId();
			}

			public void npcId_set(byte arg0)
			{
				npc_.npcId_set(arg0);
			}

			public bool addNpc(byte npcId)
			{
				return npc_.addNpc(npcId);
			}

			public bool releaseNpc(byte npcId)
			{
				return npc_.releaseNpc(npcId);
			}

			public void setBadStateNormal()
			{
				badState_ = NPC_BAD_STATE.NPC_BAD_STATE_NORMAL;
			}

			public void setFrog()
			{
				badState_ = NPC_BAD_STATE.NPC_BAD_STATE_FROG;
			}

			public void setLilliput()
			{
				badState_ = NPC_BAD_STATE.NPC_BAD_STATE_LILLIPUT;
			}

			public bool isFrog()
			{
				return badState_ == NPC_BAD_STATE.NPC_BAD_STATE_FROG;
			}

			public bool isLilliput()
			{
				return badState_ == NPC_BAD_STATE.NPC_BAD_STATE_LILLIPUT;
			}

			public void setDefault()
			{
				npc_.setDefault();
			}

			public void copy(NpcManager src)
			{
				npc_.copy(src.npc_);
			}

			public void parse(ArrayReader reader)
			{
				npc_.parse(reader);
			}

			public void store(ArrayWriter writer)
			{
				npc_.store(writer);
			}
		}

		public class NpcParameter
		{
			private bool isEnable_;

			private byte npcId_;

			public bool addNpc(byte npcId)
			{
				setNpcId(npcId);
				onIsEnable();
				return true;
			}

			public bool releaseNpc(byte npcId)
			{
				clearNpcId();
				offIsEnable();
				return true;
			}

			public bool isEnable()
			{
				return isEnable_;
			}

			public void onIsEnable()
			{
				isEnable_ = true;
			}

			public void offIsEnable()
			{
				isEnable_ = false;
			}

			public byte npcId()
			{
				return npcId_;
			}

			public void npcId_set(byte arg0)
			{
				npcId_ = arg0;
			}

			public void setNpcId(byte npcId)
			{
				npcId_ = npcId;
			}

			public void clearNpcId()
			{
				npcId_ = byte.MaxValue;
			}

			public void setDefault()
			{
				isEnable_ = false;
				npcId_ = 0;
			}

			public void copy(NpcParameter src)
			{
				isEnable_ = src.isEnable_;
				npcId_ = src.npcId_;
			}

			public void parse(ArrayReader reader)
			{
				isEnable_ = reader.readByte() != 0;
				npcId_ = reader.readByte();
			}

			public void store(ArrayWriter writer)
			{
				writer.writeByte((byte)(isEnable_ ? 1u : 0u));
				writer.writeByte(npcId_);
			}
		}

		public enum NPC_ID
		{
			NPC_CID = 0,
			NPC_SALA = 1,
			NPC_DESCH = 2,
			NPC_GUTSUKO = 3,
			NPC_ELIA = 4,
			NPC_ARS = 5,
			NPC_DOGA = 6,
			NPC_UNE = 7,
			NPC_ID_MAX = 8,
			INVALID_NPC_ID = 255
		}

		public const NPC_ID NPC_CID = NPC_ID.NPC_CID;

		public const NPC_ID NPC_SALA = NPC_ID.NPC_SALA;

		public const NPC_ID NPC_DESCH = NPC_ID.NPC_DESCH;

		public const NPC_ID NPC_GUTSUKO = NPC_ID.NPC_GUTSUKO;

		public const NPC_ID NPC_ELIA = NPC_ID.NPC_ELIA;

		public const NPC_ID NPC_ARS = NPC_ID.NPC_ARS;

		public const NPC_ID NPC_DOGA = NPC_ID.NPC_DOGA;

		public const NPC_ID NPC_UNE = NPC_ID.NPC_UNE;

		public const NPC_ID NPC_ID_MAX = NPC_ID.NPC_ID_MAX;

		public const NPC_ID INVALID_NPC_ID = NPC_ID.INVALID_NPC_ID;
	}
}
