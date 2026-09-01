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
		public static class fs
		{
			public class RequestObject
			{
				public class NotifyHandler
				{
					~NotifyHandler()
					{
					}

					public virtual void notifyCompletion(enFDL_RESULT ret)
					{
					}
				}

				public const int MaxNameBufferSize = 32;

				private string _szFilename;

				private Array _pDest;

				private enFDL_FILETYPE _enType;

				public uint _unRequestID;

				private NotifyHandler _pHandler;

				public RequestObject()
				{
					_pDest = null;
					_enType = enFDL_FILETYPE.enFDL_FILETYPE_NORMAL;
					_unRequestID = 0u;
					_pHandler = null;
				}

				public RequestObject(Array pDest, string szFilename, enFDL_FILETYPE type, NotifyHandler pHandler)
				{
					_unRequestID = 0u;
					strcpy(out _szFilename, szFilename);
					_pDest = pDest;
					_enType = type;
					_pHandler = pHandler;
				}

				public void setNotifyHandler(NotifyHandler pHandler)
				{
					_pHandler = pHandler;
				}

				public NotifyHandler getNotifyHandler()
				{
					return _pHandler;
				}

				public string getFilename()
				{
					return _szFilename;
				}

				public Array getDestination()
				{
					return _pDest;
				}

				public enFDL_FILETYPE getFileType()
				{
					return _enType;
				}

				public uint getRequestID()
				{
					return _unRequestID;
				}
			}

			public class FileDivideLoaderImp
			{
				public enum enPHASE_STATE
				{
					enPHASE_WAIT,
					enPHASE_NON_COMP_OPEN,
					enPHASE_NON_COMP_LOAD,
					enPHASE_COMPRESS_OPEN,
					enPHASE_COMPRESS_LOAD
				}

				public const int MaxRequestNum = 32;

				public const enPHASE_STATE enPHASE_WAIT = enPHASE_STATE.enPHASE_WAIT;

				public const enPHASE_STATE enPHASE_NON_COMP_OPEN = enPHASE_STATE.enPHASE_NON_COMP_OPEN;

				public const enPHASE_STATE enPHASE_NON_COMP_LOAD = enPHASE_STATE.enPHASE_NON_COMP_LOAD;

				public const enPHASE_STATE enPHASE_COMPRESS_OPEN = enPHASE_STATE.enPHASE_COMPRESS_OPEN;

				public const enPHASE_STATE enPHASE_COMPRESS_LOAD = enPHASE_STATE.enPHASE_COMPRESS_LOAD;

				private enPHASE_STATE _enPhase;

				private Vector<RequestObject, OrderSavedErasePolicy<RequestObject>> _vRequests = new Vector<RequestObject, OrderSavedErasePolicy<RequestObject>>(32);

				private RequestObject _pCurrNode;

				private CFile _fileNon;

				private byte[] _pDest;

				private uint _unLoadSizeNon;

				private StreamArchiver _fileComp = new StreamArchiver();

				private Archive.CompressInfo _infoComp = new Archive.CompressInfo();

				private uint _unLoadSizeComp;

				private static uint s_unRequestID = 1u;

				public void setReadSize(int size)
				{
					_unLoadSizeNon = (uint)size;
				}

				public int getReadSize()
				{
					return (int)_unLoadSizeNon;
				}

				public void setCompressReadSize(int size)
				{
					_unLoadSizeComp = (uint)size;
				}

				public int getCompressReadSize()
				{
					return (int)_unLoadSizeComp;
				}

				private void transitPhase(enPHASE_STATE phase)
				{
					_enPhase = phase;
				}

				public FileDivideLoaderImp()
				{
					_pCurrNode = null;
					_enPhase = enPHASE_STATE.enPHASE_WAIT;
					_unLoadSizeNon = 10240u;
					_unLoadSizeComp = 10240u;
				}

				~FileDivideLoaderImp()
				{
				}

				public void destruct()
				{
				}

				public bool requestLoad(RequestObject rReq)
				{
					if (_vRequests.size() >= 32)
					{
						return false;
					}
					const_cast<RequestObject>(rReq)._unRequestID = s_unRequestID;
					s_unRequestID++;
					_vRequests.push_back(rReq);
					return true;
				}

				public void forceLoad()
				{
					while (_vRequests.size() != 0)
					{
						updateRequests();
					}
				}

				public void updateRequests()
				{
					switch (_enPhase)
					{
					case enPHASE_STATE.enPHASE_WAIT:
						executeWait();
						break;
					case enPHASE_STATE.enPHASE_NON_COMP_OPEN:
						executeNonCompressOpen();
						break;
					case enPHASE_STATE.enPHASE_NON_COMP_LOAD:
						executeNonCompressLoad();
						break;
					case enPHASE_STATE.enPHASE_COMPRESS_OPEN:
						executeCompressOpen();
						break;
					case enPHASE_STATE.enPHASE_COMPRESS_LOAD:
						executeCompressLoad();
						break;
					}
				}

				public void clearRequests()
				{
					_enPhase = enPHASE_STATE.enPHASE_WAIT;
					_pCurrNode = null;
					_vRequests.clear();
				}

				public bool isEmpty()
				{
					if (!_vRequests.empty() || _pCurrNode != null)
					{
						return false;
					}
					return true;
				}

				public void executeWait()
				{
					if (_pCurrNode == null && _vRequests.size() != 0)
					{
						_pCurrNode = _vRequests.at(0);
						if (_pCurrNode.getFileType() == enFDL_FILETYPE.enFDL_FILETYPE_NORMAL)
						{
							_enPhase = enPHASE_STATE.enPHASE_NON_COMP_OPEN;
						}
						else
						{
							_enPhase = enPHASE_STATE.enPHASE_COMPRESS_OPEN;
						}
					}
				}

				public void executeNonCompressOpen()
				{
					_pDest = reinterpret_cast<byte[]>(_pCurrNode.getDestination());
					if (!_fileNon.open(_pCurrNode.getFilename()))
					{
						notifyCurrent(enFDL_RESULT.enFDL_RESULT_FAILED);
						transitPhase(enPHASE_STATE.enPHASE_WAIT);
					}
					else
					{
						transitPhase(enPHASE_STATE.enPHASE_NON_COMP_LOAD);
					}
				}

				public void executeNonCompressLoad()
				{
					int num = _fileNon.read(_pDest, (int)_unLoadSizeNon);
					if (num == -1)
					{
						notifyCurrent(enFDL_RESULT.enFDL_RESULT_FAILED);
						transitPhase(enPHASE_STATE.enPHASE_WAIT);
					}
					else if (num != _unLoadSizeNon)
					{
						notifyCurrent(enFDL_RESULT.enFDL_RESULT_OK);
						transitPhase(enPHASE_STATE.enPHASE_WAIT);
					}
				}

				public void executeCompressOpen()
				{
					_pDest = reinterpret_cast<byte[]>(_pCurrNode.getDestination());
					if (_fileComp.analysisReadFile(_infoComp, _pCurrNode.getFilename()) != Archive.enRESULT.enRESULT_OK)
					{
						notifyCurrent(enFDL_RESULT.enFDL_RESULT_FAILED);
						transitPhase(enPHASE_STATE.enPHASE_WAIT);
					}
					else if (_fileComp.prepareReadFile(_pDest, _unLoadSizeComp) != Archive.enRESULT.enRESULT_OK)
					{
						notifyCurrent(enFDL_RESULT.enFDL_RESULT_FAILED);
						transitPhase(enPHASE_STATE.enPHASE_WAIT);
					}
					else
					{
						transitPhase(enPHASE_STATE.enPHASE_COMPRESS_LOAD);
					}
				}

				public void executeCompressLoad()
				{
					Archive.enRESULT enRESULT = _fileComp.uncompressReadFile(_unLoadSizeComp);
					if (enRESULT == Archive.enRESULT.enRESULT_STREAMING_END)
					{
						notifyCurrent(enFDL_RESULT.enFDL_RESULT_OK);
						transitPhase(enPHASE_STATE.enPHASE_WAIT);
					}
				}

				public void notifyCurrent(enFDL_RESULT res)
				{
					if (_pCurrNode != null)
					{
						_pCurrNode.getNotifyHandler()?.notifyCompletion(res);
						_vRequests.erase(0);
						_pCurrNode = null;
					}
				}
			}

			public class FileDivideLoader
			{
				public static FileDivideLoader instance_ = new FileDivideLoader();

				private FileDivideLoaderImp _imp;

				~FileDivideLoader()
				{
				}

				public void beginning()
				{
					_imp = createFileDivideLoaderImp();
				}

				public void ending()
				{
					deleteFileDivideLoaderImp(_imp);
				}

				public uint requestLoad(RequestObject rReq)
				{
					if (_imp != null && _imp.requestLoad(rReq))
					{
						return rReq.getRequestID();
					}
					return 0u;
				}

				public void forceLoad()
				{
					if (_imp != null)
					{
						_imp.forceLoad();
					}
				}

				public void updateRequests()
				{
					if (_imp != null)
					{
						_imp.updateRequests();
					}
				}

				public void clearRequests()
				{
					if (_imp != null)
					{
						_imp.clearRequests();
					}
				}

				public bool isEmpty()
				{
					if (_imp != null)
					{
						return _imp.isEmpty();
					}
					return true;
				}

				public void setReadSize(int size)
				{
					if (_imp != null)
					{
						_imp.setReadSize(size);
					}
				}

				public int getReadSize()
				{
					if (_imp != null)
					{
						return _imp.getReadSize();
					}
					return 0;
				}

				public void setCompressReadSize(int size)
				{
					if (_imp != null)
					{
						_imp.setCompressReadSize(size);
					}
				}

				public int getCompressReadSize()
				{
					if (_imp != null)
					{
						return _imp.getCompressReadSize();
					}
					return 0;
				}

				public static FileDivideLoader getSingleton()
				{
					return instance_;
				}
			}

			public enum enFDL_RESULT
			{
				enFDL_RESULT_OK,
				enFDL_RESULT_FAILED
			}

			public enum enFDL_FILETYPE
			{
				enFDL_FILETYPE_NORMAL,
				enFDL_FILETYPE_COMPRESS
			}

			public const enFDL_RESULT enFDL_RESULT_OK = enFDL_RESULT.enFDL_RESULT_OK;

			public const enFDL_RESULT enFDL_RESULT_FAILED = enFDL_RESULT.enFDL_RESULT_FAILED;

			public const enFDL_FILETYPE enFDL_FILETYPE_NORMAL = enFDL_FILETYPE.enFDL_FILETYPE_NORMAL;

			public const enFDL_FILETYPE enFDL_FILETYPE_COMPRESS = enFDL_FILETYPE.enFDL_FILETYPE_COMPRESS;

			public static FileDivideLoaderImp createFileDivideLoaderImp()
			{
				return (FileDivideLoaderImp)CHeap.alloc_sys(typeof(FileDivideLoaderImp));
			}

			internal static void deleteFileDivideLoaderImp(FileDivideLoaderImp pImp)
			{
				if (pImp != null)
				{
					pImp.destruct();
					CHeap.free_sys(pImp);
				}
			}
		}
	}
}
