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
		public static class sound
		{
			public class SoundRequest
			{
				public FSFile _phFile;

				public Array _pDest;

				public uint _unPos;

				public uint _unSize;

				public uint _unID;

				public SoundNotifyHandler _pHandler;

				public SoundRequest(FSFile hFile, Array pDest, uint unPos, uint unSize, SoundNotifyHandler pHandler)
				{
					_phFile = hFile;
					_pDest = pDest;
					_unPos = unPos;
					_unSize = unSize;
					_pHandler = pHandler;
					_unID = 0u;
				}

				public SoundRequest()
				{
				}

				~SoundRequest()
				{
				}
			}

			public class SoundDivideLoaderImp
			{
				public enum enPHASE_STATE
				{
					enPHASE_WAIT,
					enPHASE_SEEK,
					enPHASE_LOAD
				}

				public const int MaxRequestNum = 8;

				public const enPHASE_STATE enPHASE_WAIT = enPHASE_STATE.enPHASE_WAIT;

				public const enPHASE_STATE enPHASE_SEEK = enPHASE_STATE.enPHASE_SEEK;

				public const enPHASE_STATE enPHASE_LOAD = enPHASE_STATE.enPHASE_LOAD;

				private enPHASE_STATE _enPhase;

				private Vector<SoundRequest, OrderSavedErasePolicy<SoundRequest>> _vRequests = new Vector<SoundRequest, OrderSavedErasePolicy<SoundRequest>>(8);

				private SoundRequest _pCurrNode;

				private CFile _fileNon;

				private byte[] _pDest;

				private uint _unRest;

				private uint _unLoadSize;

				private static uint s_unRequestID = 1u;

				public void setReadSize(int size)
				{
					_unLoadSize = (uint)size;
				}

				public int getReadSize()
				{
					return (int)_unLoadSize;
				}

				private void transitPhase(enPHASE_STATE phase)
				{
					_enPhase = phase;
				}

				public SoundDivideLoaderImp()
				{
					_pCurrNode = null;
					_enPhase = enPHASE_STATE.enPHASE_WAIT;
					_pDest = null;
					_unLoadSize = 10240u;
				}

				~SoundDivideLoaderImp()
				{
					destruct();
				}

				public void destruct()
				{
				}

				public uint requestLoad(SoundRequest rReq)
				{
					if (_vRequests.size() >= 8)
					{
						return 0u;
					}
					SoundRequest soundRequest = const_cast<SoundRequest>(rReq);
					soundRequest._unID = s_unRequestID;
					s_unRequestID++;
					_vRequests.push_back(rReq);
					return soundRequest._unID;
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
					case enPHASE_STATE.enPHASE_SEEK:
						executeSeek();
						break;
					case enPHASE_STATE.enPHASE_LOAD:
						executeLoad();
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
						_enPhase = enPHASE_STATE.enPHASE_SEEK;
					}
				}

				public void executeSeek()
				{
					_pDest = reinterpret_cast<byte[]>(_pCurrNode._pDest);
					_unRest = _pCurrNode._unSize;
					if (_pCurrNode._unPos != 0)
					{
						FS_SeekFile(_pCurrNode._phFile, (int)_pCurrNode._unPos, FSSeekFileMode.FS_SEEK_SET);
					}
					transitPhase(enPHASE_STATE.enPHASE_LOAD);
				}

				public void executeLoad()
				{
					int num = (int)((_unLoadSize > _unRest) ? _unRest : _unLoadSize);
					FS_ReadFile(_pCurrNode._phFile, _pDest, num);
					_unRest -= (uint)num;
					if (num <= 0)
					{
						notifyCurrent();
						transitPhase(enPHASE_STATE.enPHASE_WAIT);
					}
				}

				public void notifyCurrent()
				{
					if (_pCurrNode != null)
					{
						_pCurrNode._pHandler?.notifyCompletion();
						_vRequests.erase(0);
						_pCurrNode = null;
					}
				}
			}

			public class SoundDivideLoader
			{
				public static SoundDivideLoader instance_ = new SoundDivideLoader();

				private SoundDivideLoaderImp _imp;

				~SoundDivideLoader()
				{
				}

				public void beginning()
				{
					_imp = createSoundDivideLoaderImp();
				}

				public void ending()
				{
					deleteSoundDivideLoaderImp(_imp);
				}

				public uint requestLoad(SoundRequest rReq)
				{
					if (_imp != null)
					{
						return _imp.requestLoad(rReq);
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

				public static SoundDivideLoader getSingleton()
				{
					return instance_;
				}
			}

			public class SoundNotifyHandler
			{
				~SoundNotifyHandler()
				{
				}

				public virtual void notifyCompletion()
				{
				}
			}

			public static SoundDivideLoaderImp createSoundDivideLoaderImp()
			{
				SoundDivideLoaderImp soundDivideLoaderImp = (SoundDivideLoaderImp)CHeap.alloc_sys(typeof(SoundDivideLoaderImp));
				return new SoundDivideLoaderImp();
			}

			internal static void deleteSoundDivideLoaderImp(SoundDivideLoaderImp pImp)
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
