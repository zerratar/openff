using android.content;

namespace net.sqexm.sqmk.android.lib.manager;

public class ContentsInfo
{
	public enum ContentsType
	{
		PAY_ONCE,
		PAY_MONTHLY,
		FREE,
		TRIAL_REGULAR,
		TRIAL_FREE,
		TRIAL_ANYONE
	}

	private string mDomain;

	private string mGroupId;

	private string mId;

	private string mBinId;

	private int mVersion;

	private ContentsType mType;

	public ContentsInfo(Context iContext, string iDomain, string iGroupId, string iId, string iBinId, ContentsType iType)
	{
		mDomain = iDomain;
		mGroupId = iGroupId;
		mId = iId;
		mBinId = iBinId;
		mVersion = 0;
		mType = iType;
	}

	public int getVersion()
	{
		return mVersion;
	}
}
