using net.sqexm.sqmk.lib.utils.net;

namespace net.sqexm.sqmk.android.lib.api.authapi;

public abstract class AuthApi : NetUtils.OnGetAsyncHttpListener
{
	public interface OnStateListener
	{
		void responce(AuthApi iApi, bool iSuccess, int iResult, string iMessage);
	}
}
