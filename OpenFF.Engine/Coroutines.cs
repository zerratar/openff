// Coroutines: a script that spans frames, written as one method.
//
// A cutscene is "say this, wait for the tap, walk over there, wait for the walk, ask a
// question, act on the answer". Written as callbacks that is a ladder; written as an
// iterator it reads top to bottom:
//
//   IEnumerator Scene()
//   {
//       Game.Dialogue.Say("Follow me.");
//       yield return Wait.Dialogue();
//       villager.MoveTo(door, 60);
//       yield return Wait.Walk(villager);
//       yield return Wait.Seconds(0.5);
//   }
//   Game.Run(Scene());
//
// The runner steps every routine once per engine frame, after the services and the
// world. A yield of null or an int waits that many frames (null: one). A routine that
// throws is reported and dropped; routines a mod started end with the mod.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OpenFF
{
	/// <summary>What a coroutine yields to wait for something.</summary>
	public abstract class Wait
	{
		/// <summary>True when the wait is over; asked once per frame.</summary>
		public abstract bool Done { get; }

		public static Wait Frames(int frames) => new FrameWait(frames);
		public static Wait Seconds(double seconds) => new TimeWait(seconds);
		public static Wait Until(Func<bool> condition) => new UntilWait(condition);
		/// <summary>Until the message window has been dismissed.</summary>
		public static Wait Dialogue() => new UntilWait(() => Game.Dialogue == null || !Game.Dialogue.IsOpen);
		/// <summary>Until a character's walk ends.</summary>
		public static Wait Walk(Npc npc) => new UntilWait(() => npc == null || !npc.Alive || !npc.Moving);
		/// <summary>Until the hero's scripted walk ends.</summary>
		public static Wait HeroWalk() => new UntilWait(() => Game.Hero == null || !Game.Hero.Moving);

		private sealed class FrameWait : Wait
		{
			private int _left;
			public FrameWait(int frames) { _left = Math.Max(1, frames); }
			public override bool Done => --_left <= 0;
		}

		private sealed class TimeWait : Wait
		{
			private readonly double _until;
			public TimeWait(double seconds) { _until = Game.Time.Total + Math.Max(0, seconds); }
			public override bool Done => Game.Time.Total >= _until;
		}

		private sealed class UntilWait : Wait
		{
			private readonly Func<bool> _condition;
			public UntilWait(Func<bool> condition) { _condition = condition ?? (() => true); }
			public override bool Done
			{
				get
				{
					try { return _condition(); }
					catch (Exception ex) { Game.Warn("Wait.Until: " + ex.Message); return true; }
				}
			}
		}
	}

	/// <summary>A running coroutine; Stop ends it early.</summary>
	public sealed class Coroutine
	{
		internal IEnumerator Routine;
		internal Wait Waiting;
		internal int FramesLeft;
		internal System.Reflection.Assembly Owner;
		public string Name { get; internal set; }
		public bool Running { get; internal set; } = true;

		public void Stop()
		{
			Running = false;
		}
	}

	public sealed class CoroutineRunner
	{
		private readonly List<Coroutine> _running = new List<Coroutine>();

		public int Count => _running.Count;

		public Coroutine Start(IEnumerator routine, string name = null)
		{
			if (routine == null) throw new ArgumentNullException(nameof(routine));
			Coroutine coroutine = new Coroutine
			{
				Routine = routine,
				Name = name ?? routine.GetType().Name,
				Owner = routine.GetType().Assembly,
			};
			_running.Add(coroutine);
			return coroutine;
		}

		internal void Update()
		{
			foreach (Coroutine c in _running.ToArray())
			{
				if (!c.Running)
				{
					_running.Remove(c);
					continue;
				}
				if (c.FramesLeft > 0)
				{
					c.FramesLeft--;
					if (c.FramesLeft > 0) continue;
				}
				else if (c.Waiting != null)
				{
					bool done;
					try { done = c.Waiting.Done; }
					catch (Exception ex) { Game.Warn(c.Name + ": " + ex.Message); done = true; }
					if (!done) continue;
					c.Waiting = null;
				}
				bool more;
				try
				{
					more = c.Routine.MoveNext();
				}
				catch (Exception ex)
				{
					Game.Warn("coroutine " + c.Name + ": " + ex.GetType().Name + ": " + ex.Message);
					more = false;
				}
				if (!more)
				{
					c.Running = false;
					_running.Remove(c);
					continue;
				}
				object current = c.Routine.Current;
				switch (current)
				{
					case null:
						c.FramesLeft = 1;
						break;
					case int frames:
						c.FramesLeft = Math.Max(1, frames);
						break;
					case Wait wait:
						c.Waiting = wait;
						break;
					case IEnumerator nested:
						// A routine yielding another runs it to its end first.
						Coroutine inner = Start(nested, c.Name + ">" + nested.GetType().Name);
						c.Waiting = Wait.Until(() => !inner.Running);
						break;
					default:
						c.FramesLeft = 1;
						break;
				}
			}
		}

		/// <summary>Ends every routine whose code lives in an assembly (a mod being unloaded).</summary>
		internal void RemoveFrom(System.Reflection.Assembly assembly)
		{
			foreach (Coroutine c in _running.Where(c => c.Owner == assembly).ToArray())
			{
				c.Running = false;
				_running.Remove(c);
			}
		}

		public void StopAll()
		{
			foreach (Coroutine c in _running) c.Running = false;
			_running.Clear();
		}
	}

	public static partial class Game
	{
		/// <summary>The running coroutines; Game.Run starts one.</summary>
		public static CoroutineRunner Coroutines { get; } = new CoroutineRunner();

		/// <summary>Starts a coroutine; it advances once per engine frame.</summary>
		public static Coroutine Run(IEnumerator routine, string name = null) => Coroutines.Start(routine, name);
	}
}
