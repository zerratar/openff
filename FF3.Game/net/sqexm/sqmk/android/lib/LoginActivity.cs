using android.app;
using android.os;
using net.sqexm.sqmk.android.lib.api;
using net.sqexm.sqmk.android.lib.manager;

namespace net.sqexm.sqmk.android.lib;

public abstract class LoginActivity : Activity
{
	public virtual void execute()
	{
	}

	public abstract ContentsInfo getContentsInfo();

	protected override void onCreate(Bundle savedInstanceState)
	{
		ApiBase.init(this, InfoManager.getInfoManager(this, getContentsInfo()), "");
		execute();
	}
}
