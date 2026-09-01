using android.content;
using android.graphics;
using android.view;
using javax.microedition.khronos.egl;
using javax.microedition.khronos.opengles;

namespace android.opengl;

public class GLSurfaceView : SurfaceView
{
	public interface Renderer
	{
		void onDrawFrame(GL10 gl);

		void onSurfaceChanged(GL10 gl, int width, int height);

		void onSurfaceCreated(GL10 gl, EGLConfig config);
	}

	private class NullGL10 : GL10
	{
		public void glViewport(int x, int y, int width, int height)
		{
		}
	}

	private class NullEGLConfig : EGLConfig
	{
	}

	private NullGL10 m_Gl10;

	private NullEGLConfig m_Config;

	private Renderer m_Renderer;

	public GLSurfaceView(Context context)
		: base(context)
	{
		m_Gl10 = new NullGL10();
		m_Config = new NullEGLConfig();
	}

	protected override void onDraw(Canvas canvas)
	{
		m_Renderer.onDrawFrame(m_Gl10);
	}

	public void onPause()
	{
	}

	public void onResume()
	{
	}

	public void setRenderer(Renderer renderer)
	{
		m_Renderer = renderer;
		m_Renderer.onSurfaceCreated(m_Gl10, m_Config);
		m_Renderer.onSurfaceChanged(m_Gl10, 800, 480);
	}
}
