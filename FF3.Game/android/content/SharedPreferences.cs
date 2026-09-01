using java.util;

namespace android.content;

public abstract class SharedPreferences
{
	public interface Editor
	{
		void apply();

		Editor clear();

		bool commit();

		Editor putBoolean(string key, bool value);

		Editor putFloat(string key, float value);

		Editor putInt(string key, int value);

		Editor putLong(string key, long value);

		Editor putString(string key, string value);

		Editor putStringSet(string key, Set<string> values);

		Editor remove(string key);
	}

	public abstract Editor edit();

	public abstract string getString(string key, string defValue);
}
