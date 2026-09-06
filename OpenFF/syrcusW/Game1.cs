using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace syrcusW;

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
		FF3.GameHost.Start();
		base.BeginRun();
	}

	protected override void EndRun()
	{
		FF3.GameHost.Stop();
		base.EndRun();
	}

	protected override void Initialize()
	{
		base.Initialize();
	}

	protected override void LoadContent()
	{
		GlobalScope.m_Graphics.LoadContent();
		FF3.GameHost.Create();
		base.LoadContent();
	}

	protected override void UnloadContent()
	{
		FF3.GameHost.Destroy();
		base.UnloadContent();
	}

	protected override void OnActivated(object sender, EventArgs args)
	{
		FF3.GameHost.Resume();
		base.OnActivated(sender, args);
	}

	protected override void OnDeactivated(object sender, EventArgs args)
	{
		FF3.GameHost.Pause();
		base.OnDeactivated(sender, args);
	}

	protected override void Update(GameTime gameTime)
	{
		// PORT: the original read TouchPanel here and folded the touch points into a
		// single gesture. This is a Windows build, so input comes from the mouse and
		// keyboard instead; FF3.DesktopInput raises the same Android.onTouch* callbacks
		// the rest of the game listens to. See Compat/DesktopInput.cs.
		if (!Guide.IsVisible)
		{
			FF3.DesktopInput.Update();
		}

		// Fast-forward (hold Tab). Normally this just flips the game's own boost flag
		// and runs a single update, exactly as the original did.
		// Fast-forward. Android.onUpdate() was empty, so the game's whole tick ran
		// from the draw callback; --speed therefore has to run extra ticks here, on
		// top of the one Draw performs.
		int repeats = FF3.DesktopInput.BeginFrame();
		for (int i = 1; i < repeats; i++)
		{
			FF3.GameHost.Tick();
		}

		try
		{
			base.Update(gameTime);
		}
		catch (GameUpdateRequiredException)
		{
			if (!Guide.IsVisible)
			{
				MainActivity.updateApp();
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
		if (FF3.RenderTest.Active)
		{
			base.Draw(gameTime);
			return;
		}
		if (!GlobalScope.m_Graphics.isPause())
		{
			FF3.GameHost.Tick();
		}
		else
		{
			GlobalScope.m_Graphics.clear();
		}
		base.Draw(gameTime);
	}
}
