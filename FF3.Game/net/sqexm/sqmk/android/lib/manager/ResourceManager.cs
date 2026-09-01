using System;
using System.Collections.Generic;
using net.sqexm.sqmk.android.lib.api;

namespace net.sqexm.sqmk.android.lib.manager;

public class ResourceManager : ApiBaseManager, ResList.OnResListEventListener
{
	public interface OnResourceManagerEventListener
	{
		void onLoaded(string iUrl, string iFilename, bool iSuccess, Exception iE);

		void onReceived(ResourceManager iResourceManager, ApiHandle iApiHandle, byte[] iResKey, Exception iE);
	}

	public ResourceManager(byte[] iResKey)
	{
	}

	public static string keyToString(byte[] iKey)
	{
		return "";
	}

	public void download(string iUrl, string iFilename, OnResourceManagerEventListener iListener)
	{
	}

	public List<ResItem> getItems(ApiHandle iHandle)
	{
		return null;
	}

	public byte[] getResKey()
	{
		return new byte[1];
	}

	public ResList getResList(ApiHandle iHandle)
	{
		return null;
	}

	public static byte[] stringToKey(string iKey)
	{
		return new byte[1];
	}

	public ApiHandle take(string iResourceGropuId, OnResourceManagerEventListener iListener)
	{
		return null;
	}

	public void onReceived(ResList iResList, Exception iE)
	{
	}
}
