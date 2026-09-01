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
		Android.addActivity(new BootActivity());
	}

	protected override void BeginRun()
	{
		Android.onStart();
		base.BeginRun();
	}

	protected override void EndRun()
	{
		Android.onStop();
		base.EndRun();
	}

	protected override void Initialize()
	{
		base.Initialize();
	}

	protected override void LoadContent()
	{
		GlobalScope.m_Graphics.LoadContent();
		Android.onCreate();
		base.LoadContent();
	}

	protected override void UnloadContent()
	{
		Android.onDestroy();
		base.UnloadContent();
	}

	protected override void OnActivated(object sender, EventArgs args)
	{
		Android.onResume();
		base.OnActivated(sender, args);
	}

	protected override void OnDeactivated(object sender, EventArgs args)
	{
		Android.onPause();
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
		int repeats = FF3.DesktopInput.BeginFrame();
		for (int i = 0; i < repeats; i++)
		{
			Android.onUpdate();
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
			Android.onDraw();
		}
		else
		{
			GlobalScope.m_Graphics.clear();
		}
		base.Draw(gameTime);
	}
}
