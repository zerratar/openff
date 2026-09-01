using android.app;
using android.content;

namespace net.sqexm.sqmk.android.lib.manager;

public class InfoManager
{
	private static InfoManager sInstance;

	private Context mContext;

	private ContentsInfo mContentsInfo;

	public static InfoManager getInfoManager(Activity iActivity, ContentsInfo iContentsInfo)
	{
		if (sInstance == null)
		{
			sInstance = new InfoManager(iActivity, iContentsInfo);
		}
		return sInstance;
	}

	public int getVersion()
	{
		return mContentsInfo.getVersion();
	}

	private InfoManager(Activity iActivity, ContentsInfo iContentsInfo)
	{
		mContext = iActivity.getApplicationContext();
		mContentsInfo = iContentsInfo;
	}
}
