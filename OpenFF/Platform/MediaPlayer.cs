using System;
using System.Threading;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace OpenFF.Platform;

public class MediaPlayer
{
	public interface OnCompletionListener
	{
		void onCompletion(MediaPlayer mp);
	}

	private ContentManager m_Content;

	private SoundEffect m_Sound;

	private SoundEffectInstance m_Instance;

	private OnCompletionListener m_CompletionListener;

	private Thread m_Thread;

	public void pause()
	{
		if (m_Instance != null)
		{
			m_Instance.Pause();
		}
	}

	public void prepare()
	{
		m_Instance = m_Sound.CreateInstance();
	}

	public void release()
	{
		if (m_Instance != null)
		{
			m_Instance.Dispose();
			m_Instance = null;
		}
		if (m_Sound != null)
		{
			if (!m_Shared)
			{
				m_Sound.Dispose();
			}
			m_Sound = null;
		}
		if (m_Content != null)
		{
			m_Content.Dispose();
			m_Content = null;
		}
	}

	public void setDataSource(string path)
	{
		// PORT: the content chain first - a Steam install's sound/<name>.ogg, FF4's
		// .akb - and the XNB the phone build shipped only when there is none.
		m_Sound = OpenFF.Client.OggSound.Load(path);
		if (m_Sound != null)
		{
			m_Shared = true;
			return;
		}
		m_Content = GlobalScope.m_Graphics.CreateContentManager();
		m_Sound = m_Content.Load<SoundEffect>(path);
	}

	// A decoded Ogg is cached and shared between players, so release() must not dispose it.
	private bool m_Shared;

	public void setLooping(bool looping)
	{
		if (m_Instance != null)
		{
			m_Instance.IsLooped = looping;
		}
	}

	public void setOnCompletionListener(OnCompletionListener listener)
	{
		m_CompletionListener = listener;
	}

	public void setVolume(float leftVolume, float rightVolume, bool bMute)
	{
		if (m_Instance != null)
		{
			m_Instance.Volume = (bMute ? 0f : ((leftVolume + rightVolume) * 0.5f));
		}
	}

	public void start()
	{
		if (m_Instance != null)
		{
			m_Instance.Play();
		}
		if (m_CompletionListener != null)
		{
			m_Thread = new Thread(run);
			m_Thread.Start();
		}
	}

	public void stop()
	{
		if (m_Instance != null)
		{
			m_Instance.Stop();
		}
		if (m_Thread != null)
		{
			m_Thread.Join();
			m_Thread = null;
		}
	}

	private void run()
	{
		while (!m_Instance.IsDisposed)
		{
			try
			{
				if (m_Instance.State == SoundState.Stopped)
				{
					break;
				}
			}
			catch (Exception)
			{
				break;
			}
			Thread.Sleep(1);
		}
		if (m_CompletionListener != null)
		{
			m_CompletionListener.onCompletion(this);
		}
	}
}
