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
	public static partial class ds
	{
		public class ArchiveImp
		{
			private FSFile _hFile = new FSFile();

			private MICompressionHeader _hMiComp;

			private ITypesArchiver _pCurrImp;

			private Array _pWork;

			private uint _unWorkSize;

			private ArchiveImpLz77 _impLz77 = new ArchiveImpLz77();

			private ArchiveImpHuffman _impHuffman;

			private ArchiveImpRL _impRL;

			private ArchiveImpDiff _impDiff;

			public bool isOpen()
			{
				if (FS_IsFile(_hFile) == 0)
				{
					return false;
				}
				return true;
			}

			public ArchiveImp()
			{
				_pCurrImp = null;
				_pWork = null;
				_unWorkSize = 0u;
			}

			~ArchiveImp()
			{
			}

			public void destruct()
			{
				cancelReadFile();
				releaseWork();
			}

			public Archive.enRESULT uncompressData(Array pDest, Archive.CompressInfo info)
			{
				Archive.enRESULT result = info.enCompType switch
				{
					Archive.enCOMP_TYPE.enCOMP_LZ => _impLz77.uncompress(pDest, info), 
					Archive.enCOMP_TYPE.enCOMP_HUFFMAN => _impHuffman.uncompress(pDest, info), 
					Archive.enCOMP_TYPE.enCOMP_RL => _impRL.uncompress(pDest, info), 
					Archive.enCOMP_TYPE.enCOMP_DIFF => _impDiff.uncompress(pDest, info), 
					_ => Archive.enRESULT.enRESULT_INVALID_TYPE, 
				};
				DC_FlushRange(pDest, info.unExtractSize);
				return result;
			}

			public Archive.enRESULT analysisReadFile(Archive.CompressInfo info, string szFile)
			{
				if (isReadFile())
				{
					cancelReadFile();
				}
				FS_InitFile(_hFile);
				if (FS_OpenFile(_hFile, szFile) == 0)
				{
					return Archive.enRESULT.enRESULT_NOT_FOUND_FILE;
				}
				FS_ReadFile(_hFile, m_abyReadCache, 4);
				_hMiComp = (MICompressionHeader)m_abyReadCache;
				if (Archive.isSupportCompressType(_hMiComp))
				{
					setCompressInfo(info, _hMiComp);
					info.pSrc = null;
					return Archive.enRESULT.enRESULT_OK;
				}
				cancelReadFile();
				return Archive.enRESULT.enRESULT_INVALID_TYPE;
			}

			public Archive.enRESULT prepareReadFile(Array pDest, uint unWorkSize, Archive.CompressInfo info)
			{
				if (FS_IsFile(_hFile) == 0 || _pCurrImp != null)
				{
					return Archive.enRESULT.enRESULT_UNINITIALIZED;
				}
				reserveWork(unWorkSize);
				FS_ReadFile(_hFile, _pWork, 508);
				switch (info.enCompType)
				{
				case Archive.enCOMP_TYPE.enCOMP_LZ:
					_pCurrImp = _impLz77;
					break;
				case Archive.enCOMP_TYPE.enCOMP_HUFFMAN:
					_pCurrImp = _impHuffman;
					break;
				case Archive.enCOMP_TYPE.enCOMP_RL:
					_pCurrImp = _impRL;
					break;
				default:
					cancelReadFile();
					return Archive.enRESULT.enRESULT_INVALID_TYPE;
				}
				_pCurrImp.prepareReadFile(pDest, _hMiComp);
				FS_WaitAsync(_hFile);
				return _pCurrImp.updateReadFile(_pWork, 508u);
			}

			public Archive.enRESULT uncompressReadFile(uint unReadSize)
			{
				if (_pCurrImp != null)
				{
					uint unReadSize2 = (uint)FS_ReadFileAsync(_hFile, _pWork, (int)unReadSize);
					FS_WaitAsync(_hFile);
					Archive.enRESULT enRESULT = _pCurrImp.updateReadFile(_pWork, unReadSize2);
					if (enRESULT == Archive.enRESULT.enRESULT_STREAMING_END)
					{
						FS_CloseFile(_hFile);
						_pCurrImp = null;
					}
					return enRESULT;
				}
				cancelReadFile();
				return Archive.enRESULT.enRESULT_UNINITIALIZED;
			}

			public void cancelReadFile()
			{
				if (FS_IsFile(_hFile) != 0)
				{
					FS_CloseFile(_hFile);
				}
				_pCurrImp = null;
			}

			public bool isReadFile()
			{
				if (FS_IsFile(_hFile) != 0 && _pCurrImp != null)
				{
					return true;
				}
				return false;
			}

			public uint getReadFileSize()
			{
				if (FS_IsFile(_hFile) != 0)
				{
					return FS_GetLength(_hFile);
				}
				return 0u;
			}

			public bool reserveWork(uint unWorkSize)
			{
				if (_pWork != null)
				{
					if (unWorkSize <= _unWorkSize)
					{
						return true;
					}
					releaseWork();
				}
				if (unWorkSize > unLimitWorkSize)
				{
					_pWork = CHeap.alloc_app(unWorkSize);
				}
				else
				{
					_pWork = CHeap.alloc_sys(unWorkSize);
				}
				if (_pWork != null)
				{
					_unWorkSize = unWorkSize;
					return true;
				}
				return false;
			}

			public void releaseWork()
			{
				if (_pWork != null)
				{
					if (_unWorkSize > unLimitWorkSize)
					{
						CHeap.free_app(_pWork);
					}
					else
					{
						CHeap.free_sys(_pWork);
					}
					_pWork = null;
					_unWorkSize = 0u;
				}
			}
		}
	}
}
