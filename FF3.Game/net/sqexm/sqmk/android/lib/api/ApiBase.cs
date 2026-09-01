using android.app;
using net.sqexm.sqmk.android.lib.manager;
using net.sqexm.sqmk.lib.utils.net;

namespace net.sqexm.sqmk.android.lib.api;

public abstract class ApiBase : NetUtils.OnGetAsyncHttpListener
{
	public const int RESULT_MAINTENANCE = 0;

	public const int RESULT_NG = 1;

	public const int RESULT_SESSION_NG = 2;

	public const int RESULT_SUCCESS = 3;

	private static Activity sActivity;

	private static InfoManager sInfoManager;

	private static string sOneTimePass;

	public static void init(Activity iActivity, InfoManager iInfoManager, string iOneTimePass)
	{
		sActivity = iActivity;
		sInfoManager = iInfoManager;
		sOneTimePass = iOneTimePass;
	}

	public static Activity getActivity()
	{
		return sActivity;
	}

	public static InfoManager getInfoManager()
	{
		return sInfoManager;
	}
}
