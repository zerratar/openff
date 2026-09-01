using net.sqexm.sqmk.android.lib.api.authapi;

namespace net.sqexm.sqmk.android.lib.manager;

public class AuthManager : AuthApi.OnStateListener
{
	public bool getEndSucceeded()
	{
		return true;
	}

	public void responce(AuthApi iApi, bool iSuccess, int iResult, string iMessage)
	{
	}
}
