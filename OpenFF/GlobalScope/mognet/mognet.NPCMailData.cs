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
	public static partial class mognet
	{
		public class NPCMailData
		{
			public ushort sendTotal_;

			public byte npcMailActivity_;

			public byte npcMailReceiveState_;

			public byte[] receivedMail_ = new byte[NUMBER_OF_RECEIVE_NPC_MAIL];

			public void setDefault()
			{
				sendTotal_ = 0;
				npcMailActivity_ = 0;
				npcMailReceiveState_ = 0;
				memset(receivedMail_, 0, NUMBER_OF_RECEIVE_NPC_MAIL);
			}

			public void copy(NPCMailData src)
			{
				sendTotal_ = src.sendTotal_;
				npcMailActivity_ = src.npcMailActivity_;
				npcMailReceiveState_ = src.npcMailReceiveState_;
				memcpy(receivedMail_, src.receivedMail_, NUMBER_OF_RECEIVE_NPC_MAIL);
			}

			public void parse(ArrayReader reader)
			{
				sendTotal_ = reader.readUInt16();
				npcMailActivity_ = reader.readByte();
				npcMailReceiveState_ = reader.readByte();
				reader.read(receivedMail_, 0, NUMBER_OF_RECEIVE_NPC_MAIL);
			}

			public void store(ArrayWriter writer)
			{
				writer.writeUInt16(sendTotal_);
				writer.writeByte(npcMailActivity_);
				writer.writeByte(npcMailReceiveState_);
				writer.write(receivedMail_, 0, NUMBER_OF_RECEIVE_NPC_MAIL);
			}
		}
	}
}
