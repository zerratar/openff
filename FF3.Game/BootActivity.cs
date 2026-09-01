using android.os;
using net.sqexm.sqmk.android.lib;
using net.sqexm.sqmk.android.lib.manager;

public class BootActivity : LoginActivity
{
	protected override void onCreate(Bundle savedInstanceState)
	{
		base.onCreate(savedInstanceState);
	}

	public override ContentsInfo getContentsInfo()
	{
		return new ContentsInfo(this, "an.sqexm.net", "FF3", "app", "common", ContentsInfo.ContentsType.PAY_ONCE);
	}

	public override void execute()
	{
		DLActivity.startDownload(this, force: false);
		finish();
	}
}
