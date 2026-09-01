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
	public class CFileData
	{
		private Array pAddr;

		private uint fsize;

		public object m_Object;

		public CFileData()
		{
			pAddr = null;
			m_Object = null;
		}

		~CFileData()
		{
			cleanup();
		}

		public bool setup(string filename, ds.fs.enFDL_FILETYPE type)
		{
			switch (type)
			{
			case ds.fs.enFDL_FILETYPE.enFDL_FILETYPE_NORMAL:
				fsize = ds.g_File.getSize(filename);
				if (fsize != 0)
				{
					pAddr = ds.CHeap.alloc_app(fsize);
					m_Object = null;
					_ = pAddr;
					ds.g_File.load(pAddr, filename);
					break;
				}
				return false;
			case ds.fs.enFDL_FILETYPE.enFDL_FILETYPE_COMPRESS:
			{
				if (0 >= ds.g_File.getSize(filename))
				{
					return false;
				}
				ds.FileArchiver fileArchiver = new ds.FileArchiver();
				ds.Archive.CompressInfo compressInfo = new ds.Archive.CompressInfo();
				if (fileArchiver.analysisFile(compressInfo, filename) != ds.Archive.enRESULT.enRESULT_OK)
				{
					return false;
				}
				if (compressInfo.unExtractSize == 0)
				{
					return false;
				}
				fsize = compressInfo.unExtractSize;
				pAddr = ds.CHeap.alloc_app(fsize);
				m_Object = null;
				if (pAddr == null)
				{
					cleanup();
					return false;
				}
				if (ds.Archive.enRESULT.enRESULT_STREAMING_END != fileArchiver.uncompressFile(pAddr))
				{
					cleanup();
					return false;
				}
				break;
			}
			}
			return true;
		}

		public bool setupAsync(string filename, ds.fs.enFDL_FILETYPE type, ds.fs.RequestObject.NotifyHandler pNH)
		{
			switch (type)
			{
			case ds.fs.enFDL_FILETYPE.enFDL_FILETYPE_NORMAL:
				fsize = ds.g_File.getSize(filename);
				if (fsize != 0)
				{
					pAddr = ds.CHeap.alloc_app(fsize);
					m_Object = null;
					if (pAddr == null)
					{
						return false;
					}
					ds.fs.RequestObject rReq2 = new ds.fs.RequestObject(pAddr, filename, type, pNH);
					ds.fs.FileDivideLoader.getSingleton().requestLoad(rReq2);
					break;
				}
				return false;
			case ds.fs.enFDL_FILETYPE.enFDL_FILETYPE_COMPRESS:
			{
				if (0 >= ds.g_File.getSize(filename))
				{
					return false;
				}
				ds.FileArchiver fileArchiver = new ds.FileArchiver();
				ds.Archive.CompressInfo compressInfo = new ds.Archive.CompressInfo();
				if (fileArchiver.analysisFile(compressInfo, filename) != ds.Archive.enRESULT.enRESULT_OK)
				{
					return false;
				}
				if (compressInfo.unExtractSize == 0)
				{
					return false;
				}
				fsize = compressInfo.unExtractSize;
				pAddr = ds.CHeap.alloc_app(fsize);
				m_Object = null;
				if (pAddr == null)
				{
					cleanup();
					return false;
				}
				ds.fs.RequestObject rReq = new ds.fs.RequestObject(pAddr, filename, type, pNH);
				ds.fs.FileDivideLoader.getSingleton().requestLoad(rReq);
				break;
			}
			}
			return true;
		}

		public void cleanup()
		{
			if (pAddr != null)
			{
				ds.CHeap.free_app(pAddr);
				pAddr = null;
				m_Object = null;
				fsize = 0u;
			}
		}

		public Array getAddr()
		{
			return pAddr;
		}

		public T getAddr<T>()
		{
			if (m_Object == null)
			{
				Type typeFromHandle = typeof(T);
				MethodInfo method = typeFromHandle.GetMethod("cast");
				m_Object = method.Invoke(null, new object[1] { pAddr });
			}
			return (T)m_Object;
		}

		public uint getSize()
		{
			return fsize;
		}
	}
}
