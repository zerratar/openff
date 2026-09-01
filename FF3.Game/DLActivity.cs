using System;
using System.Collections.Generic;
using android.app;
using android.content;
using android.os;
using android.view;
using java.io;
using java.lang;
using java.util;
using net.sqexm.sqmk.android.lib.api;
using net.sqexm.sqmk.android.lib.manager;

public class DLActivity : Activity, ResourceManager.OnResourceManagerEventListener
{
	private const int TABLE_FORMAT = 0;

	private const int TABLE_FILE_COUNT = 4;

	private const int TABLE_ARCHIVE_COUNT = 8;

	private const int TABLE_FILE_ARCHIVE = 12;

	private const int TABLE_FILE_INDEX = 16;

	private const int TABLE_FILE_NANE = 20;

	private const int TABLE_PITCH = 12;

	private const string ARCHIVE_NAME = "data";

	private static ResourceManager mResourceManager;

	private static int index;

	private static int resVersion;

	private static byte[] fileTable;

	private ProgressDialog dlg;

	private List<ResItem> mResItems;

	private Timer timer;

	private int tick;

	public static string getDataPath()
	{
		return "Content";
	}

	protected override void onCreate(Bundle savedInstanceState)
	{
		base.onCreate(savedInstanceState);
		setContentView(new SurfaceView(this));
	}

	protected override void onDestroy()
	{
		if (timer != null)
		{
			timer.cancel();
		}
		base.onDestroy();
	}

	private void download()
	{
		mResourceManager.take("FF3", this);
	}

	private void downloadResource()
	{
		mResourceManager.download(mResItems[index].getResUrl(), "temp.bin", this);
	}

	private void showError(int type)
	{
	}

	public void onReceived(ResourceManager iResourceManager, ApiBaseManager.ApiHandle iHandle, byte[] iResKey, Exception iE)
	{
		int responceCode = iResourceManager.getResponceCode(iHandle);
		if (responceCode != 3)
		{
			showError(0);
			return;
		}
		SharedPreferences preferences = ApiBase.getActivity().getPreferences(0);
		SharedPreferences.Editor editor = preferences.edit();
		editor.putString("reskey", ResourceManager.keyToString(iResKey));
		editor.commit();
		int num = Integer.parseInt(iResourceManager.getResList(iHandle).getVersion());
		if (num != resVersion)
		{
			resVersion = num;
			index = 0;
		}
		List<ResItem> items = iResourceManager.getItems(iHandle);
		mResItems = new ArrayList<ResItem>();
		for (int i = 0; i < items.Count; i++)
		{
			string text = "data" + i / 100 + i / 10 % 10 + i % 10 + ".bin";
			int j;
			for (j = 0; j < items.Count; j++)
			{
				ResItem resItem = items[j];
				string text2 = resItem.getResUrl();
				int num2 = text2.LastIndexOf('?');
				if (num2 != -1)
				{
					text2 = text2.Substring(0, num2);
				}
				if (text.Equals(text2.Substring(text2.LastIndexOf('/') + 1)))
				{
					mResItems.Add(resItem);
					break;
				}
			}
			if (j == items.Count)
			{
				break;
			}
		}
		downloadResource();
	}

	public void onLoaded(string iUrl, string iFilename, bool iSuccess, Exception iE)
	{
		DataInputStream dataInputStream = null;
		int num = iUrl.LastIndexOf('?');
		if (num != -1)
		{
			iUrl = iUrl.Substring(0, num);
		}
		if (iSuccess)
		{
			dataInputStream = null;
			try
			{
				dataInputStream = new DataInputStream(new FileInputStream(iFilename));
				int num2 = dataInputStream.readInt();
				if (num2 != 1095910193)
				{
					iSuccess = false;
				}
			}
			catch (Exception)
			{
				iSuccess = false;
			}
			finally
			{
				try
				{
					dataInputStream.close();
				}
				catch (Exception)
				{
				}
			}
		}
		if (!iSuccess)
		{
			showError(1);
			return;
		}
		dataInputStream = null;
		RandomAccessFile randomAccessFile = null;
		try
		{
			dataInputStream = new DataInputStream(new FileInputStream(iFilename));
			File file = new File(getDataPath());
			file.mkdirs();
			string text = iUrl.Substring(iUrl.LastIndexOf('/') + 1);
			randomAccessFile = new RandomAccessFile(getDataPath() + "/" + text, "rw");
			if (text.Equals("data000.bin"))
			{
				int num3 = (int)new File(iFilename).length();
				byte[] array = new byte[num3];
				for (int i = 0; i < num3; i += dataInputStream.read(array, i, num3 - i))
				{
				}
				encodeLight(array);
				randomAccessFile.write(array);
			}
			else
			{
				initFileTable();
				int val = dataInputStream.readInt();
				int num4 = dataInputStream.readInt();
				int[] array2 = new int[num4 / 4 - 1];
				array2[0] = num4;
				for (int j = 1; j < array2.Length; j++)
				{
					array2[j] = dataInputStream.readInt();
				}
				int[] array3 = new int[array2.Length];
				array3[0] = array2[0];
				randomAccessFile.writeInt(val);
				for (int k = 0; k < array3.Length; k++)
				{
					randomAccessFile.writeInt(array3[k]);
				}
				for (int l = 0; l < array2.Length - 1; l++)
				{
					int num5 = array2[l + 1] - array2[l];
					byte[] array4 = new byte[num5];
					for (int m = 0; m < num5; m += dataInputStream.read(array4, m, num5 - m))
					{
					}
					encodeLight(array4);
					randomAccessFile.write(array4);
					array3[l + 1] = (array3[l] + array4.Length) & 0x7FFFFFFF;
				}
				randomAccessFile.seek(4L);
				for (int n = 0; n < array3.Length; n++)
				{
					randomAccessFile.writeInt(array3[n]);
				}
			}
			index++;
			if (index == mResItems.Count)
			{
				index = -1;
			}
			saveOption();
		}
		catch (Exception)
		{
			iSuccess = false;
		}
		finally
		{
			try
			{
				dataInputStream.close();
			}
			catch (Exception)
			{
			}
			try
			{
				randomAccessFile.close();
			}
			catch (Exception)
			{
			}
		}
		if (iSuccess)
		{
			if (index != -1)
			{
				downloadResource();
				return;
			}
			initFileTable();
			Intent intent = new Intent(this, typeof(MainActivity));
			startActivity(intent);
			finish();
		}
	}

	public static void startDownload(Activity activity, bool force)
	{
		SharedPreferences preferences = ApiBase.getActivity().getPreferences(0);
		string text = preferences.getString("reskey", null);
		mResourceManager = new ResourceManager((text == null) ? null : ResourceManager.stringToKey(text));
		index = 0;
		if (text != null)
		{
			loadOption();
		}
		if (force)
		{
			index = 0;
		}
		if (index == -1 && (!initFileTable() || !checkFileTable()))
		{
			index = 0;
		}
		if (index == -1)
		{
			Intent intent = new Intent(activity, typeof(MainActivity));
			activity.startActivity(intent);
		}
		else
		{
			Intent intent2 = new Intent(activity, typeof(DLActivity));
			intent2.putExtra("type", "execute()");
			activity.startActivity(intent2);
		}
	}

	private static bool initFileTable()
	{
		InputStream inputStream = null;
		try
		{
			string path = getDataPath() + "/data000.bin";
			inputStream = new FileInputStream(path);
			int num = (int)new File(path).length();
			fileTable = new byte[num];
			for (int i = 0; i < num; i += inputStream.read(fileTable, i, num - i))
			{
			}
			encodeLight(fileTable);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
		finally
		{
			try
			{
				inputStream.close();
			}
			catch (Exception)
			{
			}
		}
	}

	private static bool checkFileTable()
	{
		try
		{
			int num = ROM_S4(fileTable, 8);
			for (int i = 0; i < num; i++)
			{
				string path = getDataPath() + "/data" + i / 100 + i / 10 % 10 + i % 10 + ".bin";
				if (new File(path).length() == 0)
				{
					return false;
				}
			}
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	private static int ROM_S4(byte[] data, int pos)
	{
		return (data[pos] << 24) | ((data[pos + 1] & 0xFF) << 16) | ((data[pos + 2] & 0xFF) << 8) | (data[pos + 3] & 0xFF);
	}

	public static byte[] loadFileEntry(string filename)
	{
		int num = 0;
		if (fileTable != null)
		{
			int num2 = 0;
			int num3 = ROM_S4(fileTable, 4);
			byte[] bytes = StringUtil.getBytes(filename);
			while (num3 > num2)
			{
				int num4 = (num2 + num3) / 2;
				int num5 = 0;
				int num6 = ROM_S4(fileTable, num4 * 12 + 20);
				for (int i = 0; i < bytes.Length; i++)
				{
					if (num5 != 0)
					{
						break;
					}
					num5 = (fileTable[num6 + i] & 0xFF) - (bytes[i] & 0xFF);
				}
				if (num5 == 0)
				{
					num5 = fileTable[num6 + bytes.Length] & 0xFF;
				}
				if (num5 == 0)
				{
					num3 = (num2 = num4);
					num = num4 * 12 + 12;
				}
				else if (num5 > 0)
				{
					num3 = num4;
				}
				else
				{
					num2 = num4 + 1;
				}
			}
		}
		if (num == 0)
		{
			return null;
		}
		int num7 = ROM_S4(fileTable, num);
		int num8 = ROM_S4(fileTable, num + 4);
		DataInputStream dataInputStream = null;
		try
		{
			dataInputStream = new DataInputStream(new FileInputStream(getDataPath() + "/data" + num7 / 100 + num7 / 10 % 10 + num7 % 10 + ".bin"));
			dataInputStream.skip((num8 + 1) * 4);
			int num9 = dataInputStream.readInt();
			dataInputStream.readInt();
			dataInputStream.skip((num9 & 0x7FFFFFFF) - num8 * 4 - 12);
			int num10 = dataInputStream.readInt();
			byte[] array = new byte[num10];
			for (int j = 0; j < array.Length; j += dataInputStream.read(array, j, array.Length - j))
			{
			}
			return array;
		}
		catch (Exception)
		{
			return null;
		}
		finally
		{
			try
			{
				dataInputStream.close();
			}
			catch (Exception)
			{
			}
		}
	}

	private static string getFileName(int arcihve, int index)
	{
		if (fileTable != null)
		{
			int num = 0;
			int num2 = ROM_S4(fileTable, 4);
			while (num2 > num)
			{
				int num3 = (num + num2) / 2;
				int num4 = ROM_S4(fileTable, num3 * 12 + 12) - arcihve;
				if (num4 == 0)
				{
					num4 = ROM_S4(fileTable, num3 * 12 + 16) - index;
				}
				if (num4 == 0)
				{
					num = num3;
					int num5 = ROM_S4(fileTable, num3 * 12 + 20);
					int i;
					for (i = 0; fileTable[num5 + i] != 0; i++)
					{
					}
					return StringUtil.createString(fileTable, num5, i);
				}
				if (num4 > 0)
				{
					num2 = num3;
				}
				else
				{
					num = num3 + 1;
				}
			}
		}
		return "";
	}

	private static void loadOption()
	{
		index = 0;
		resVersion = 0;
		DataInputStream dataInputStream = null;
		try
		{
			dataInputStream = new DataInputStream(new FileInputStream(getDataPath() + "/dl.bin"));
			byte[] array = new byte[dataInputStream.available()];
			dataInputStream.read(array);
			dataInputStream.close();
			dataInputStream = null;
			encodeLight(array);
			dataInputStream = new DataInputStream(new ByteArrayInputStream(array));
			int num = dataInputStream.readInt();
			if (num == ApiBase.getInfoManager().getVersion())
			{
				resVersion = dataInputStream.readInt();
				index = dataInputStream.readInt();
			}
		}
		catch (Exception)
		{
			index = 0;
			resVersion = 0;
		}
		finally
		{
			try
			{
				dataInputStream.close();
			}
			catch (Exception)
			{
			}
		}
	}

	private static void saveOption()
	{
		ByteArrayOutputStream byteArrayOutputStream = null;
		DataOutputStream dataOutputStream = null;
		try
		{
			byteArrayOutputStream = new ByteArrayOutputStream();
			dataOutputStream = new DataOutputStream(byteArrayOutputStream);
			dataOutputStream.writeInt(ApiBase.getInfoManager().getVersion());
			dataOutputStream.writeInt(resVersion);
			dataOutputStream.writeInt(index);
		}
		finally
		{
			try
			{
				dataOutputStream.close();
			}
			catch (Exception)
			{
			}
			try
			{
				byteArrayOutputStream.close();
			}
			catch (Exception)
			{
			}
		}
		byte[] array = byteArrayOutputStream.toByteArray();
		encodeLight(array);
		FileOutputStream fileOutputStream = null;
		try
		{
			fileOutputStream = new FileOutputStream(getDataPath() + "/dl.bin");
			fileOutputStream.write(array);
		}
		finally
		{
			try
			{
				fileOutputStream.close();
			}
			catch (Exception)
			{
			}
		}
	}

	private static void encodeLight(byte[] data)
	{
		byte[] resKey = mResourceManager.getResKey();
		int num = 0;
		for (int i = 0; i < 4 && i < resKey.Length; i++)
		{
			num |= (resKey[i] & 0xFF) << i * 8;
		}
		MainActivity.encode(data, num);
	}
}
