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
	public static partial class map
	{
							public class RareMonsterGroupMng
							{
								private Array data_;

								private int monPartyId_;

								private ushort rate_;

								public void initialize(string stagename)
								{
									if (data_ == null)
									{
										changeGlobalDirectory();
										string arg = "";
										sprintf(out arg, "%s.rmg", stagename);
										uint size = ds.g_File.getSize(arg);
										if (size != 0)
										{
											data_ = ds.CHeap.alloc_app(size);
											ds.g_File.load(data_, arg);
											changeCompanyDirectory();
											monPartyId_ = ArrayReader.packInt32((byte[])data_, 0);
											rate_ = (ushort)ArrayReader.packInt32((byte[])data_, 4);
										}
									}
								}

								public void terminate()
								{
									if (data_ != null)
									{
										ds.CHeap.free_app(data_);
										data_ = null;
									}
								}

								public int lottery()
								{
									if (data_ == null)
									{
										return -1;
									}
									ushort num = ds.RandomNumber.rand16(100);
									if (num < rate_)
									{
										return monPartyId_;
									}
									return -1;
								}

								public RareMonsterGroupMng()
								{
									data_ = null;
									rate_ = ushort.MaxValue;
									monPartyId_ = -1;
								}

								~RareMonsterGroupMng()
								{
									if (data_ != null)
									{
										ds.CHeap.free_app(data_);
									}
								}
							}
	}
}
