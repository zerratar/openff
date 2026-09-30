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
	public static partial class btl
	{
		public class SummonDataManager
		{
			public static string fileName_ = "summon_script_command.pack";

			private SummonData summonData_;

			private int maxSize_;

			private CommandParameter[,][] CommandParameterTable_ = new CommandParameter[8, 3][];

			private Array pData_;

			public void load()
			{
				uint size = static_cast<uint>(ds.g_File.getSize(fileName_));
				pData_ = ds.CHeap.alloc_app(size);
				ds.g_File.load(pData_, fileName_);
				for (int i = 0; i < 8; i++)
				{
					for (int j = 0; j < 3; j++)
					{
						uint iId = (uint)(j + 3 * i);
						CommandParameterTable_[i, j] = CommandParameter.ChainPointer((byte[])pData_, (int)iId);
					}
				}
			}

			public void free()
			{
				if (pData_ != null)
				{
					ds.CHeap.free_app(pData_);
					pData_ = null;
				}
			}

			/// <summary>PORT: one of the pack's chains by its number - a mod's new summon's, after the game's 24; null when the pack has no such chain.</summary>
			public CommandParameter[] chain(int id)
			{
				byte[] data = pData_ as byte[];
				if (data == null || data.Length < 4 || id < 0 || id >= BitConverter.ToInt32(data, 0)) return null;
				return CommandParameter.ChainPointer(data, id);
			}

			public CommandParameter[] commandParameter(int summonLevel, int summonType)
			{
				return CommandParameterTable_[summonLevel, summonType];
			}

			public SummonDataManager()
			{
				summonData_ = null;
				pData_ = null;
			}
		}
	}
}
