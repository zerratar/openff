namespace android.content.res;

// PORT: only ever served the language byte through openRawResource(). MainActivity
// now reads that resource directly, so nothing is left but the type itself, which
// Activity.getResources() still returns.
public class Resources
{
}
