using System;

namespace net.sqexm.sqmk.lib.utils.net;

public class NetUtils
{
	public class OnGetAsyncHttpListener
	{
		public void onReceive(string iUri, string iResult, GetAsyncHttpException iE)
		{
		}
	}

	public class GetAsyncHttpException : Exception
	{
	}
}
