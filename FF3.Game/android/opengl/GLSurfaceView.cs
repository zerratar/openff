using android.content;
using android.view;

namespace android.opengl;

// PORT: was a GLSurfaceView driving a GL ES renderer. FF3.NativeRenderer owns
// rendering now, so the GL10 context and EGLConfig that every callback used to
// carry are gone; what remains is the surface lifecycle the game still relies on.
public class GLSurfaceView : SurfaceView
{
	public interface Renderer
	{
		void onDrawFrame();

		/// <summary>Reports the drawing surface size. The game derives its touch
		/// mapping from this, so it is still meaningful.</summary>
		void onSurfaceChanged(int width, int height);

		void onSurfaceCreated();
	}

	private Renderer m_Renderer;

	public GLSurfaceView(Context context)
		: base(context)
	{
	}

	protected override void onDraw()
	{
		m_Renderer.onDrawFrame();
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
		m_Renderer.onSurfaceCreated();
		m_Renderer.onSurfaceChanged(800, 480);
	}
}
