using android.app;
using net.sqexm.sqmk.android.lib.manager;

namespace net.sqexm.sqmk.android.lib;

public class SQEXMApplication : Application
{
	private AuthManager mAuthManager;

	public SQEXMApplication()
	{
		mAuthManager = new AuthManager();
	}

	public AuthManager getAuthManager()
	{
		return mAuthManager;
	}
}
