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
	public class SaveDataMng
	{
		public const int SDMNG_SLOT_NUM = 3;

		public const int SDMNG_SLOT_HAS_BROKEN = 0;

		public const int SDMNG_SLOT_NOT_BROKEN = 1;

		public static SaveDataMng instance_ = new SaveDataMng();

		private card.CSaveData _SaveData = new card.CSaveData();

		private int _nState;

		private int _nActiveSlot;

		private int[] _nActivate = new int[3];

		private int[] _nSlotState = new int[3];

		private card.CSaveData[] _pSD = new card.CSaveData[3];

		public SaveDataMng()
		{
			_pSD[0] = (_pSD[1] = (_pSD[2] = null));
		}

		public int initialize()
		{
			deactivate(0);
			deactivate(1);
			deactivate(2);
			return 1;
		}

		public void finalize()
		{
			deactivate(0);
			deactivate(1);
			deactivate(2);
		}

		public void update()
		{
			if (_SaveData.sdExecute())
			{
				if (!_SaveData.sdCheck())
				{
					_nState = 0;
				}
				else
				{
					_nState = 1;
				}
			}
		}

		public int loadSync(int nSlot)
		{
			_SaveData.sdLoad(nSlot, sub: false);
			while (!_SaveData.sdExecute())
			{
			}
			if (_SaveData.sdExecute())
			{
				return 1;
			}
			return 0;
		}

		public int saveSync(int nSlot)
		{
			_SaveData.sdCreate();
			_SaveData.sdSave(nSlot, sub: false);
			while (!_SaveData.sdExecute())
			{
			}
			if (_SaveData.sdGetResult() != card.RESULT.RESULT_SUCCESS)
			{
				return 1;
			}
			return 0;
		}

		public int loadAsync(int nSlot)
		{
			_SaveData.sdLoad(nSlot, sub: false);
			return 1;
		}

		public int saveAsync(int nSlot)
		{
			_SaveData.sdCreate();
			_SaveData.sdSave(nSlot, sub: false);
			return 1;
		}

		public int loadAsyncState()
		{
			if (card.Manager.GetInstance().IsExecute())
			{
				return 1;
			}
			return 0;
		}

		public int saveAsyncState()
		{
			if (card.Manager.GetInstance().IsExecute())
			{
				return 1;
			}
			return 0;
		}

		public card.CSaveData SaveData()
		{
			return _SaveData;
		}

		public card.CSaveData SaveData(int nSlot)
		{
			if (3 > nSlot && 0 <= nSlot && _pSD[nSlot] != null)
			{
				return _pSD[nSlot];
			}
			return null;
		}

		public int activate(int nSlot)
		{
			if (3 > nSlot && 0 <= nSlot && _pSD[nSlot] == null)
			{
				_pSD[nSlot] = reinterpret_cast<card.CSaveData>(ds.CHeap.alloc_app(typeof(card.CSaveData)));
				if (_pSD[nSlot] != null)
				{
					_pSD[nSlot].sdLoad(nSlot, sub: false);
					while (!_pSD[nSlot].sdExecute())
					{
					}
					if (_pSD[nSlot].sdCheck())
					{
						_nSlotState[nSlot] = 1;
						_nActivate[nSlot] = 1;
						return 1;
					}
					_nSlotState[nSlot] = 0;
				}
			}
			return 0;
		}

		public int deactivate(int nSlot)
		{
			if (3 > nSlot && 0 <= nSlot && _pSD[nSlot] != null)
			{
				ds.CHeap.free_app(_pSD[nSlot]);
				_pSD[nSlot] = null;
				_nActivate[nSlot] = 0;
				return 1;
			}
			return 0;
		}

		public int refreshActiveData(int nSlot, card.CSaveData sdSrc)
		{
			if (3 > nSlot && 0 <= nSlot && _pSD[nSlot] != null)
			{
				_pSD[nSlot].copy(sdSrc);
				return 1;
			}
			return 0;
		}

		public static SaveDataMng getSingleton()
		{
			return instance_;
		}

		public int getState()
		{
			return _nState;
		}

		public int getSlotState(int nSlot)
		{
			return _nSlotState[nSlot];
		}

		public void setSlotState(int nSlot, int nState)
		{
			_nSlotState[nSlot] = nState;
		}
	}
}
