// Boot.
//
// PORT: the phone build booted through a Square Enix login (LoginActivity) and a
// resource download (DLActivity), which decided whether the game data was present
// before handing over to MainActivity. On Windows the data ships with the game, so
// this loads the archive table and starts the game directly. That removed the whole
// net.sqexm account/DRM layer along with it.

using android.app;
using android.content;
using android.os;

#nullable disable
public class BootActivity : Activity
{
	protected override void onCreate(Bundle savedInstanceState)
	{
		base.onCreate(savedInstanceState);

		if (!FF3.GameArchive.Load())
		{
			FF3.Log.Write(FF3.LogChannel.General,
				"FATAL: game archives could not be loaded from " + FF3.GameArchive.DataPath);
			return;
		}

		startActivity(new Intent(this, typeof(MainActivity)));
		finish();
	}
}
