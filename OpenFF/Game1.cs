using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace OpenFF.Client;

// MonoGame's Game and GameTime by name: the engine's OpenFF.Game and GameTime sit a namespace up.
using Game = Microsoft.Xna.Framework.Game;
using GameTime = Microsoft.Xna.Framework.GameTime;

public class Game1 : Game
{
	// Field initialisers rather than constructor statements: the original IL assigns
	// these before chaining to Game's constructor, which is precisely what C# field
	// initialisers compile to. ILSpy rendered the chained call as `base._002Ector()`,
	// which has no spelling in C#.
	private int[] m_aiTouchX = new int[16];

	private int[] m_aiTouchY = new int[16];

	public Game1()
	{
		GlobalScope.m_Graphics = new GlobalScope.Graphics(this);
		base.Content.RootDirectory = "Content";
		try
		{
			base.Components.Add((IGameComponent)new GamerServicesComponent((Game)this));
		}
		catch (Exception)
		{
		}
		GlobalScope.UserInfo.init();
		GlobalScope.setBuildTimeStamp();
		base.TargetElapsedTime = TimeSpan.FromTicks(333333L);
		base.IsFixedTimeStep = false;
		base.InactiveSleepTime = TimeSpan.FromSeconds(1.0);
		// The archives load and the game is constructed in LoadContent, once the
		// graphics device exists - texture upload needs it.
	}

	protected override void BeginRun()
	{
		OpenFF.Client.GameHost.Start();
		base.BeginRun();
	}

	protected override void EndRun()
	{
		OpenFF.Client.GameHost.Stop();
		base.EndRun();
	}

	protected override void Initialize()
	{
		base.Initialize();
	}

	protected override void LoadContent()
	{
		GlobalScope.m_Graphics.LoadContent();
		// The window whose display's refresh paces the frames, and whose focus the VSync check heeds.
		OpenFF.Client.FramePacer.Attach(this);
		OpenFF.Client.GameHost.Create();
		base.LoadContent();
	}

	protected override void UnloadContent()
	{
		OpenFF.Client.GameHost.Destroy();
		base.UnloadContent();
	}

	protected override void OnActivated(object sender, EventArgs args)
	{
		OpenFF.Client.GameHost.Resume();
		base.OnActivated(sender, args);
	}

	protected override void OnDeactivated(object sender, EventArgs args)
	{
		OpenFF.Client.GameHost.Pause();
		base.OnDeactivated(sender, args);
	}

	protected override void Update(GameTime gameTime)
	{
		// PORT: the original read TouchPanel here and folded the touch points into a
		// single gesture. This is a Windows build, so input comes from the mouse and
		// keyboard instead; OpenFF.Client.DesktopInput raises the same Android.onTouch* callbacks
		// the rest of the game listens to. See Compat/DesktopInput.cs.
		if (!Guide.IsVisible)
		{
			OpenFF.Client.DesktopInput.Update();
		}

		// Fast-forward (hold Tab). This flips the game's own boost flag, which render()
		// reads to run three of its frames a step, exactly as the original did. --speed's
		// multiple on top is run by the next paced frame (GameHost.Speed): Android.onUpdate()
		// was empty and the game's whole tick runs from the draw callback, and running extra
		// ticks here, once per display frame, would tie the speed to the display's rate.
		OpenFF.Client.GameHost.Speed = OpenFF.Client.DesktopInput.BeginFrame();

		try
		{
			base.Update(gameTime);
		}
		catch (GameUpdateRequiredException)
		{
			if (!Guide.IsVisible)
			{
				AppShell.updateApp();
			}
		}
	}

	protected override void Draw(GameTime gameTime)
	{
		if (Guide.IsVisible)
		{
			GlobalScope.m_Graphics.setPause(bPause: true);
		}
		else
		{
			GlobalScope.m_Graphics.setPause(bPause: false);
		}
		// PORT: a render test owns the whole frame, so the game must not draw over
		// it or leave device state behind that the test would then inherit.
		if (OpenFF.Client.RenderTest.Active)
		{
			base.Draw(gameTime);
			return;
		}
		if (!GlobalScope.m_Graphics.isPause())
		{
			OpenFF.Client.GameHost.Frame(GraphicsDevice);
		}
		else
		{
			GlobalScope.m_Graphics.clear();
		}
		base.Draw(gameTime);
		// Before the present: the swap interval the display's refresh calls for, and Hold's wait -
		// the Fps setting's rate by the clock with VSync off (or not holding), only a ceiling above
		// it where VSync holds the loop.
		OpenFF.Client.FramePacer.Hold();
	}
}
