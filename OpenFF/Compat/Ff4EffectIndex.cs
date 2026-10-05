// FF4's scripts name an effect by their own index, not by the effect manager's object: BootEffect_AbsoluteCoordination
// and BootEffect_RelativeCoordination_Foolow (and EffectFollow, PlayEffectEx...) give it one (CEventManager::setEffectIdx,
// 32 of them), DeleteEffect, PauseEffect and SetEffect_Scale look the object up by it (getEffectMngIdx), and a map's
// entry clears them (CEventManager::into). FF3's commands take the index as the object itself: in FF4's opening the
// first DeleteEffect (index 0, the 0x24e effect booted fifth) took away one of the four booted before it instead.

namespace OpenFF.Client
{
	internal static class Ff4EffectIndex
	{
		private static readonly int[] _object = NewTable();

		private static int[] NewTable()
		{
			int[] t = new int[32];
			for (int i = 0; i < t.Length; i++) t[i] = -1;
			return t;
		}

		public static void ClearAll()
		{
			for (int i = 0; i < _object.Length; i++) _object[i] = -1;
		}

		public static void Set(int index, int effect)
		{
			if ((uint)index < (uint)_object.Length) _object[index] = effect;
		}

		public static void Clear(int index)
		{
			if ((uint)index < (uint)_object.Length) _object[index] = -1;
		}

		/// <summary>The effect object a script's index names: FF4's table, FF3's index as it is.</summary>
		public static int Object(int index)
		{
			if (!GameProfile.IsFf4) return index;
			return (uint)index < (uint)_object.Length ? _object[index] : -1;
		}
	}
}
