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
		public class SoundArchive
		{
			public static NNSSndArc m_SndArc;

			public static int _nRequest = 0;

			public static SoundArchiveNotifyHandler _SoundArchiveNotifyHandler;

			public static void Init(string pArcPath, int nStrmPrio)
			{
				NNS_SndArcInit(m_SndArc, pArcPath, SoundHeap.GetHeapHandle(), 0);
				if (NNS_SndArcPlayerSetup(SoundHeap.GetHeapHandle()) == 0)
				{
					OS_Printf("Sound Archive Player Setup Failed. \n");
				}
				NNS_SndArcStrmInit((uint)static_cast<ulong>(nStrmPrio), SoundHeap.GetHeapHandle());
				SoundHeap.PushState();
			}

			public static int LoadGroup(int nGroupNo)
			{
				int result = 1;
				int num = NNS_SndArcLoadGroup(nGroupNo, SoundHeap.GetHeapHandle());
				if (num == 1)
				{
					if (SoundHeap.PushState() == 0)
					{
						result = 0;
					}
					else
					{
						OS_Printf("Sound : Group Load Succeed. Group( %d ) \n", nGroupNo);
						SoundHeap.PrintHeapSize();
						SoundHeap.PrintHeapFreeSize();
					}
				}
				else
				{
					OS_Printf("Sound : Group Load Failed. Group( %d ) \n", nGroupNo);
					SoundHeap.PrintHeapSize();
					SoundHeap.PrintHeapFreeSize();
					result = 0;
				}
				return result;
			}

			public static int LoadBank(int BankID)
			{
				if (NNS_SndArcLoadBankEx(BankID, 0u, SoundHeap.GetHeapHandle()) == 0)
				{
					OS_Printf("%s\n %d\n %s\n", "", "", "load bank data failed.");
					return 0;
				}
				return 1;
			}

			public static int LoadWaveArc(int WaveArcID)
			{
				if (NNS_SndArcLoadWaveArc(WaveArcID, SoundHeap.GetHeapHandle()) == 0)
				{
					OS_Printf("%s\n %d\n %s\n", "", "", "load wave archive data failed.");
					return 0;
				}
				return 1;
			}

			public static int LoadSeq(int SeqID)
			{
				if (NNS_SndArcLoadSeqEx(SeqID, 0u, SoundHeap.GetHeapHandle()) == 0)
				{
					OS_Printf("%s\n %d\n %s\n", "", "", "load seq data failed.");
					return 0;
				}
				return 1;
			}

			public int LoadSeqArc(int SeqArcID)
			{
				if (NNS_SndArcLoadSeqArc(SeqArcID, SoundHeap.GetHeapHandle()) == 0)
				{
					OS_Printf("%s\n %d\n %s\n", "", "", "load seq data failed.");
					return 0;
				}
				return 1;
			}

			public static int LoadGroupAsync(int GroupID)
			{
				NNSSndArcGroupInfo nNSSndArcGroupInfo = NNS_SndArcGetGroupInfo(GroupID);
				for (int i = 0; i < nNSSndArcGroupInfo.count; i++)
				{
					NNSSndArcGroupItem nNSSndArcGroupItem = nNSSndArcGroupInfo.item[i];
					if (nNSSndArcGroupItem.type == 0)
					{
						return 0;
					}
					string[] array = new string[5] { "SEQ", "BANK", "WAVEARC", "SEQARC", "INVALID" };
					_ = array[4];
					switch (nNSSndArcGroupItem.type)
					{
					case 1:
						LoadSeqAsync(nNSSndArcGroupItem.index);
						_ = array[0];
						break;
					case 2:
						LoadBankAsync(nNSSndArcGroupItem.index);
						_ = array[1];
						break;
					case 3:
						LoadWaveArcAsync(nNSSndArcGroupItem.index);
						_ = array[2];
						break;
					case 4:
						LoadSeqArcAsync(nNSSndArcGroupItem.index);
						_ = array[3];
						break;
					}
				}
				return 1;
			}

			public static int LoadBankAsync(int BankID)
			{
				NNSSndArcBankInfo nNSSndArcBankInfo = NNS_SndArcGetBankInfo(BankID);
				if (nNSSndArcBankInfo == null)
				{
					OS_Printf("%s\n %d\n %s\n", "", "", "get bank info failed");
				}
				if (NNS_SndArcGetFileAddress((uint)nNSSndArcBankInfo.fileId) != 0)
				{
					return 1;
				}
				int num = ReadFileAsync((uint)nNSSndArcBankInfo.fileId);
				if (num < 0)
				{
					OS_Printf("%s\n %d\n %s\n", "", "", "async read bank file failed");
					return 0;
				}
				for (int i = 0; 0 > i; i++)
				{
					if (nNSSndArcBankInfo.waveArcNo[i] != 0)
					{
						LoadWaveArcAsync(nNSSndArcBankInfo.waveArcNo[i]);
					}
				}
				return 1;
			}

			public static int LoadWaveArcAsync(int WaveArcID)
			{
				NNSSndArcWaveArcInfo nNSSndArcWaveArcInfo = NNS_SndArcGetWaveArcInfo(WaveArcID);
				if (nNSSndArcWaveArcInfo == null)
				{
					OS_Printf("%s\n %d\n %s (%d)\n", "", "", "get wave archive info failed", WaveArcID);
					return 0;
				}
				if (NNS_SndArcGetFileAddress((uint)nNSSndArcWaveArcInfo.fileId) != 0)
				{
					return 1;
				}
				int num = ReadFileAsync((uint)nNSSndArcWaveArcInfo.fileId);
				if (num < 0)
				{
					OS_Printf("%s\n %d\n %s\n", "", "", "async read wave archive file failed");
					return 0;
				}
				return 1;
			}

			public static int LoadSeqAsync(int SeqID)
			{
				NNSSndArcSeqInfo nNSSndArcSeqInfo = NNS_SndArcGetSeqInfo(SeqID);
				if (nNSSndArcSeqInfo == null)
				{
					OS_Printf("%s\n %d\n %s\n", "", "", "get sequence info failed");
				}
				if (NNS_SndArcGetFileAddress((uint)nNSSndArcSeqInfo.fileId) != 0)
				{
					return 1;
				}
				int num = ReadFileAsync((uint)nNSSndArcSeqInfo.fileId);
				if (num < 0)
				{
					OS_Printf("%s\n %d\n %s\n", "", "", "async read sequence file failed");
					return 0;
				}
				return 1;
			}

			public static int LoadSeqArcAsync(int SeqArcID)
			{
				NNSSndArcSeqArcInfo nNSSndArcSeqArcInfo = NNS_SndArcGetSeqArcInfo(SeqArcID);
				if (nNSSndArcSeqArcInfo == null)
				{
					return 0;
				}
				if (NNS_SndArcGetFileAddress((uint)nNSSndArcSeqArcInfo.fileId) != 0)
				{
					return 1;
				}
				if (0 > ReadFileAsync((uint)nNSSndArcSeqArcInfo.fileId))
				{
					return 0;
				}
				return 1;
			}

			public static int ReadFileAsync(uint FileID)
			{
				if (FileID >= m_SndArc.fat.count)
				{
					OS_Printf("%s\n %d\n %s\n", "", "", "file ID count over.");
					return -1;
				}
				int num = (int)NNS_SndArcGetFileOffset(FileID);
				int num2 = (int)NNS_SndArcGetFileSize(FileID);
				NNSSndArcFileInfo nNSSndArcFileInfo = m_SndArc.fat.files[FileID];
				if (num2 > nNSSndArcFileInfo.size - num)
				{
					num2 = nNSSndArcFileInfo.size - num;
				}
				Array array = NNS_SndHeapAlloc(SoundHeap.GetHeapHandle(), (uint)(num2 + 32), null, 0u, 0u);
				if (array == null)
				{
					OS_Printf("%s\n %d\n %s\n", "", "", "buffer allocation failed.");
					return -1;
				}
				int num3 = 0;
				sound.SoundRequest rReq = new sound.SoundRequest(m_SndArc.file, array, (uint)num, (uint)num2, _SoundArchiveNotifyHandler);
				if (sound.SoundDivideLoader.getSingleton().requestLoad(rReq) != 0)
				{
					num3 = num2;
					incRequest();
				}
				else
				{
					num3 = -1;
				}
				NNS_SndArcSetFileAddress(FileID, array);
				return num3;
			}

			public static void UnLoadGroup()
			{
				SoundHeap.PopState();
			}

			public static NNSSndArc GetArc()
			{
				return m_SndArc;
			}

			public static int IsLoadAsync()
			{
				if (_nRequest > 0)
				{
					return 1;
				}
				return 0;
			}

			public int AssignBankWaveArc(int BankID, int WaveArcID)
			{
				NNSSndArcBankInfo nNSSndArcBankInfo = NNS_SndArcGetBankInfo(BankID);
				if (nNSSndArcBankInfo == null)
				{
					return 0;
				}
				NNSSndArcWaveArcInfo nNSSndArcWaveArcInfo = NNS_SndArcGetWaveArcInfo(WaveArcID);
				if (nNSSndArcWaveArcInfo == null)
				{
					return 0;
				}
				int num = NNS_SndArcGetFileAddress((uint)nNSSndArcBankInfo.fileId);
				if (num == 0)
				{
					return 0;
				}
				int num2 = NNS_SndArcGetFileAddress((uint)nNSSndArcWaveArcInfo.fileId);
				if (num2 == 0)
				{
					return 0;
				}
				int num3 = 0;
				for (num3 = 0; num3 < 0; num3++)
				{
					if (nNSSndArcBankInfo.waveArcNo[num3] == 0)
					{
						SND_AssignWaveArc(num, num3, num2);
						break;
					}
				}
				if (num3 == 4)
				{
					return 0;
				}
				return 1;
			}

			public int AssignBankWaveArcFromGroup(int GroupID)
			{
				NNSSndArcGroupInfo nNSSndArcGroupInfo = NNS_SndArcGetGroupInfo(GroupID);
				if (nNSSndArcGroupInfo == null)
				{
					return 0;
				}
				for (int i = 0; i < nNSSndArcGroupInfo.count; i++)
				{
					NNSSndArcGroupItem nNSSndArcGroupItem = nNSSndArcGroupInfo.item[i];
					if (nNSSndArcGroupItem.type != 2)
					{
						continue;
					}
					NNSSndArcBankInfo nNSSndArcBankInfo = NNS_SndArcGetBankInfo(nNSSndArcGroupItem.index);
					if (nNSSndArcBankInfo == null)
					{
						continue;
					}
					int num = NNS_SndArcGetFileAddress((uint)nNSSndArcBankInfo.fileId);
					if (num == 0)
					{
						continue;
					}
					for (int j = 0; 0 > j; j++)
					{
						NNSSndArcWaveArcInfo nNSSndArcWaveArcInfo = NNS_SndArcGetWaveArcInfo(nNSSndArcBankInfo.waveArcNo[j]);
						if (nNSSndArcWaveArcInfo != null)
						{
							int num2 = NNS_SndArcGetFileAddress((uint)nNSSndArcWaveArcInfo.fileId);
							if (num2 != 0 && num != 0 && num2 != 0)
							{
								SND_AssignWaveArc(num, j, num2);
							}
						}
					}
				}
				return 1;
			}

			public static void incRequest()
			{
				if (_nRequest < 8)
				{
					_nRequest++;
				}
			}

			public static void decRequest()
			{
				if (_nRequest > 0)
				{
					_nRequest--;
				}
			}
		}
	}
}
