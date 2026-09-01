using System;

namespace net.sqexm.sqmk.android.lib.api;

public class ResList : ApiBase
{
	public interface OnResListEventListener
	{
		void onReceived(ResList iResList, Exception iE);
	}

	public string getVersion()
	{
		return "";
	}
}
