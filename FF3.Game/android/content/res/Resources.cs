using java.io;
using syrcusW.res.raw;

namespace android.content.res;

public class Resources
{
	public InputStream openRawResource(int id)
	{
		return new ByteArrayInputStream(language.language_dat);
	}
}
