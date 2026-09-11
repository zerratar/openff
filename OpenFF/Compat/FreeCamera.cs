// A free-flying camera for looking closely at what the game draws - a seam in a mod's
// texture, a sleeve's weights - toggled from the debug overlay (F1, then F7).
//
// The game's camera code keeps running and keeps calling NNS_G3dGlbLookAt with where it
// wants the eye; while the free camera is on, that call takes this camera's eye, target and
// up instead, so every 3D pass (the field, a battle, the shadows and billboards that read
// NNS_G3dGlb.camPos) sees the world from here, and nothing about the game's state changes.
// Player input is held off meanwhile (DesktopInput gates the pad and the mouse on Active),
// so the keys steer the camera and not the hero:
//
//   mouse           look (the cursor is hidden and held at the window's middle)
//   W A S D         forward / left / back / right, along the view
//   E / Space       up        Q / C   down
//   Shift           four times the pace     Ctrl   a quarter of it
//   wheel           the pace itself (in world units a second; a character is about 8 tall)
//   R               back to where the game's camera is
//   F7 (or Esc)     off; the game's camera is where it was all along
//
// Units are the game's world units (VecFx32 / 4096); yaw 0 looks down +Z, as the game's
// yaw does. Tab (fast-forward) and F-keys are the client's own and go on working.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace OpenFF.Client
{
	using Game = Microsoft.Xna.Framework.Game;
	using Vector3 = Microsoft.Xna.Framework.Vector3;

	internal static class FreeCamera
	{
		public static bool Active { get; private set; }

		private static Vector3 _position;
		private static float _yaw;      // radians, 0 = +Z, positive turns toward +X
		private static float _pitch;    // radians, positive looks up
		private static float _speed = 12f;
		private static int _lastWheel;
		private static bool _centred;
		private static bool _mouseWasVisible;
		private static KeyboardState _previous;

		private static readonly GlobalScope.VecFx32 _eye = new GlobalScope.VecFx32();
		private static readonly GlobalScope.VecFx32 _at = new GlobalScope.VecFx32();
		private static readonly GlobalScope.VecFx32 _up = new GlobalScope.VecFx32();

		/// <summary>On from where the game's camera is now; off back to the game's own.</summary>
		public static void Toggle(Game game)
		{
			if (!Active)
			{
				SeedFromGame();
				Active = true;
				_centred = false;
				if (game != null) { _mouseWasVisible = game.IsMouseVisible; game.IsMouseVisible = false; }
				_lastWheel = Mouse.GetState().ScrollWheelValue;
				Log.Write(LogChannel.General, "free camera: on at " + Fmt(_position) + " - mouse looks, WASD fly, E/Q up and down, Shift fast, wheel sets the pace, R back to the game's eye, F7 off");
			}
			else
			{
				Active = false;
				if (game != null) game.IsMouseVisible = _mouseWasVisible;
				Log.Write(LogChannel.General, "free camera: off");
			}
		}

		/// <summary>The camera on, at a spot, looking a way (degrees) - a drive's "camera x y z yaw pitch", for a screenshot from a chosen eye.</summary>
		public static void Place(Vector3 position, float yawDegrees, float pitchDegrees, Game game)
		{
			if (!Active)
			{
				Active = true;
				_centred = false;
				if (game != null) { _mouseWasVisible = game.IsMouseVisible; game.IsMouseVisible = false; }
				_lastWheel = Mouse.GetState().ScrollWheelValue;
			}
			_position = position;
			_yaw = MathHelper.ToRadians(yawDegrees);
			_pitch = Math.Clamp(MathHelper.ToRadians(pitchDegrees), -1.55f, 1.55f);
			Log.Write(LogChannel.General, "free camera: placed at " + Fmt(_position) + " yaw " + yawDegrees.ToString("0") + " pitch " + pitchDegrees.ToString("0"));
		}

		/// <summary>Off, if on (a drive's "camera off").</summary>
		public static void Off(Game game) { if (Active) Toggle(game); }

		/// <summary>Where the game's camera is this frame: the eye, and the yaw and pitch that look at its target.</summary>
		private static void SeedFromGame()
		{
			Vector3 eye = Units(GlobalScope.NNS_G3dGlb.camPos);
			Vector3 target = Units(GlobalScope.NNS_G3dGlb.camTarget);
			Vector3 dir = target - eye;
			if (dir.LengthSquared() < 1e-6f) dir = Vector3.UnitZ;
			dir.Normalize();
			_position = eye;
			_yaw = (float)Math.Atan2(dir.X, dir.Z);
			_pitch = (float)Math.Asin(Math.Clamp(dir.Y, -1f, 1f));
		}

		/// <summary>One frame of steering, called with the input each frame (DesktopInput.Update) while the window is active.</summary>
		public static void Update(Game game, float seconds)
		{
			if (!Active || game == null) return;
			KeyboardState keys = Keyboard.GetState();
			MouseState mouse = Mouse.GetState();
			// Escape is the way out as well as F7 (the overlay's), so a hand on the mouse can leave.
			if (keys.IsKeyDown(Keys.Escape) && !_previous.IsKeyDown(Keys.Escape)) { _previous = keys; Toggle(game); return; }
			if (keys.IsKeyDown(Keys.R) && !_previous.IsKeyDown(Keys.R)) SeedFromGame();
			_previous = keys;

			// The mouse: its move from the middle turns the view, then it is put back there.
			int cx = game.Window.ClientBounds.Width / 2, cy = game.Window.ClientBounds.Height / 2;
			if (_centred)
			{
				float dx = mouse.X - cx, dy = mouse.Y - cy;
				_yaw -= dx * 0.0035f;
				_pitch = Math.Clamp(_pitch - dy * 0.0035f, -1.55f, 1.55f);
			}
			Mouse.SetPosition(cx, cy);
			_centred = true;

			// The wheel sets the pace, a fifth a notch either way.
			int wheel = mouse.ScrollWheelValue;
			if (wheel != _lastWheel)
			{
				_speed = Math.Clamp(_speed * (float)Math.Pow(1.2, (wheel - _lastWheel) / 120.0), 0.2f, 500f);
				_lastWheel = wheel;
			}

			// The keys move along the view.
			Vector3 forward = Forward();
			Vector3 right = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, forward));
			if (right.LengthSquared() < 1e-6f) right = Vector3.UnitX;
			Vector3 move = Vector3.Zero;
			if (keys.IsKeyDown(Keys.W) || keys.IsKeyDown(Keys.Up)) move += forward;
			if (keys.IsKeyDown(Keys.S) || keys.IsKeyDown(Keys.Down)) move -= forward;
			if (keys.IsKeyDown(Keys.D) || keys.IsKeyDown(Keys.Right)) move += right;
			if (keys.IsKeyDown(Keys.A) || keys.IsKeyDown(Keys.Left)) move -= right;
			if (keys.IsKeyDown(Keys.E) || keys.IsKeyDown(Keys.Space)) move += Vector3.UnitY;
			if (keys.IsKeyDown(Keys.Q) || keys.IsKeyDown(Keys.C)) move -= Vector3.UnitY;
			if (move.LengthSquared() > 0)
			{
				move.Normalize();
				float pace = _speed;
				if (keys.IsKeyDown(Keys.LeftShift) || keys.IsKeyDown(Keys.RightShift)) pace *= 4f;
				if (keys.IsKeyDown(Keys.LeftControl) || keys.IsKeyDown(Keys.RightControl)) pace *= 0.25f;
				_position += move * pace * Math.Clamp(seconds, 0f, 0.1f);
			}
		}

		private static Vector3 Forward()
		{
			float cp = (float)Math.Cos(_pitch);
			return new Vector3((float)Math.Sin(_yaw) * cp, (float)Math.Sin(_pitch), (float)Math.Cos(_yaw) * cp);
		}

		/// <summary>
		/// The eye, target and up the game's LookAt takes while the camera is on: this one's, in
		/// fixed point. False (and the game's own untouched) while off.
		/// </summary>
		public static bool Take(ref GlobalScope.VecFx32 camPos, ref GlobalScope.VecFx32 camUp, ref GlobalScope.VecFx32 target)
		{
			if (!Active) return false;
			Vector3 at = _position + Forward();
			Set(_eye, _position); Set(_at, at); Set(_up, Vector3.UnitY);
			camPos = _eye; target = _at; camUp = _up;
			return true;
		}

		private static Vector3 Units(GlobalScope.VecFx32 v) => v == null ? Vector3.Zero : new Vector3(v.x / 4096f, v.y / 4096f, v.z / 4096f);

		private static void Set(GlobalScope.VecFx32 v, Vector3 units)
		{
			v.x = (int)Math.Round(units.X * 4096f);
			v.y = (int)Math.Round(units.Y * 4096f);
			v.z = (int)Math.Round(units.Z * 4096f);
		}

		/// <summary>A line for the debug overlay: where the eye is, which way it looks, the pace.</summary>
		public static string Describe()
		{
			return "free camera  at " + Fmt(_position) + "  yaw " + MathHelper.ToDegrees(_yaw).ToString("0") + "\u00b0  pitch " + MathHelper.ToDegrees(_pitch).ToString("0") + "\u00b0  pace " + _speed.ToString("0.#") + "/s   (mouse looks, WASD, E/Q, Shift, wheel, R, F7/Esc off)";
		}

		private static string Fmt(Vector3 v) => v.X.ToString("0.0") + ", " + v.Y.ToString("0.0") + ", " + v.Z.ToString("0.0");
	}
}
