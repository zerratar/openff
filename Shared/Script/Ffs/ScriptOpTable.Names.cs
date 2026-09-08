// The editor's half of ScriptOpTable: the mnemonic index the script language uses.
// The table itself lives in Shared/Script, compiled into the client too.

using Ffs = Crystal.Ffs;

namespace OpenFF.Script
{
	internal sealed partial class ScriptOpTable
	{
		private Ffs.Mnemonics _names;

		public Ffs.Mnemonics Names
		{
			get
			{
				if (_names == null)
				{
					_names = new Ffs.Mnemonics(Ops);
				}
				return _names;
			}
		}
	}
}
