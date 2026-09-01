using java.io;

namespace android.content;

public abstract class Context
{
	public const int MODE_PRIVATE = 0;

	public const int MODE_WORLD_READABLE = 1;

	public const string INPUT_METHOD_SERVICE = "input_method";

	public const string KEYGUARD_SERVICE = "keyguard";

	public abstract File getCacheDir();
}
