// The engine API: the verbs a script uses to act on the game.
//
// Each group is an interface a service implements. Today the client implements them on
// the legacy game (FF3.Game/Compat/EngineApi.cs), which is why they read like the FF3
// script commands they lower to - BootCharacter, MoveCharacter, StartMessage2, PlaySE,
// AddItem, MapWarp - and why a verb can be a no-op outside a map. A mod reaches them as
// Game.Dialogue, Game.Hero, Game.Npcs, Game.Flags, Game.Party, Game.Audio, Game.Screen,
// Game.Field; a mod can also register its own implementation of any of them, later in
// the load order, and every other script then talks to that one.
//
// Positions are in world units, the map's own (a character stands about one unit tall;
// the legacy code keeps them in 1/4096ths); yaw is in degrees about the up axis.

using System;
using System.Collections.Generic;

namespace OpenFF
{
	public static partial class Game
	{
		public static IDialogue Dialogue => Services.Get<IDialogue>();
		public static IHero Hero => Services.Get<IHero>();
		public static INpcs Npcs => Services.Get<INpcs>();
		public static IFlags Flags => Services.Get<IFlags>();
		public static IParty Party => Services.Get<IParty>();
		public static IAudio Audio => Services.Get<IAudio>();
		public static IScreen Screen => Services.Get<IScreen>();
		public static IField Field => Services.Get<IField>();
	}

	/// <summary>The message window at the bottom of the field.</summary>
	public interface IDialogue
	{
		/// <summary>Whether a message is up.</summary>
		bool IsOpen { get; }

		/// <summary>
		/// Shows a text in the message window, with the "tap to continue" mark; the window
		/// closes when the player taps (or presses A). One at a time: while IsOpen, a new
		/// Say replaces the text. Optionally a speaker's name.
		/// </summary>
		void Say(string text, string speaker = null);

		/// <summary>Closes the window now.</summary>
		void Close();

		/// <summary>Fired when a message the script showed has been dismissed.</summary>
		event Action Closed;
	}

	/// <summary>The character the player controls.</summary>
	public interface IHero
	{
		/// <summary>Whether there is a hero on a map right now.</summary>
		bool Present { get; }
		Vector3 Position { get; }
		/// <summary>Degrees about the up axis.</summary>
		float Yaw { get; }
		/// <summary>The model the hero wears (j101 and the like).</summary>
		string Model { get; }
		void Teleport(Vector3 position);
		void Face(float yaw);
		/// <summary>Takes control from the player: no walking, no menu button, the way an event does.</summary>
		void Freeze();
		void Unfreeze();
		bool Frozen { get; }
	}

	public enum NpcAi
	{
		/// <summary>Stands where put, turns to the player when talked to.</summary>
		Still = 0,
		/// <summary>Wanders about its spot.</summary>
		Wander = 1,
		/// <summary>Follows the hero.</summary>
		Follow = 2,
	}

	/// <summary>A character a script put on the map.</summary>
	public abstract class Npc
	{
		public string Model { get; protected set; }
		/// <summary>The mod that spawned it, or null.</summary>
		public Modding.LoadedMod Owner { get; set; }
		public abstract bool Alive { get; }
		public abstract Vector3 Position { get; }
		public abstract float Yaw { get; }
		public abstract void Teleport(Vector3 position);
		/// <summary>Walks to a point over a number of frames (0 teleports).</summary>
		public abstract void MoveTo(Vector3 position, int frames);
		public abstract bool Moving { get; }
		public abstract void Face(float yaw);
		public abstract void LookAt(Vector3 point);
		public abstract void SetAi(NpcAi ai);
		/// <summary>
		/// Whether the character blocks and shoves other characters. Off by default: a solid
		/// character standing beside the hero pushes the hero away, frame after frame.
		/// </summary>
		public abstract bool Solid { get; set; }
		/// <summary>Takes the character off the map.</summary>
		public abstract void Remove();

		/// <summary>How close the hero must be, in world units, for A to count as talking to this one.</summary>
		public float InteractRadius { get; set; } = 3f;

		/// <summary>The player pressed A (or tapped) within InteractRadius.</summary>
		public event Action<Npc> Interacted;

		protected void RaiseInteracted()
		{
			Action<Npc> handler = Interacted;
			if (handler != null)
			{
				Game.Guard("Npc.Interacted", () => handler(this));
			}
		}

		public bool HasInteractHandler => Interacted != null;
	}

	/// <summary>Characters on the map: spawning and finding them.</summary>
	public interface INpcs
	{
		/// <summary>
		/// Puts a character model (n011, n272, j101...) on the current map. Null when there
		/// is no map, the model is unknown, or the map's character slots are full.
		/// </summary>
		Npc Spawn(string model, Vector3 position, float yaw = 0f);

		/// <summary>The characters scripts have spawned and not removed.</summary>
		IReadOnlyList<Npc> Spawned { get; }
	}

	/// <summary>The game's flag space: what the scripts store quest progress in.</summary>
	public interface IFlags
	{
		bool Get(uint group, uint index);
		void Set(uint group, uint index, bool value);
	}

	/// <summary>The party: money, items.</summary>
	public interface IParty
	{
		int Gil { get; set; }
		void AddItem(int itemId, int count);
		/// <summary>How many of an item the party carries.</summary>
		int ItemCount(int itemId);
	}

	public interface IAudio
	{
		/// <summary>A sound effect by archive and number, as the scripts play them.</summary>
		void PlaySe(int archive, int number, int volume = 127, int pan = 64);
		void PlayBgm(int number, int volume = 127, int fadeInFrames = 0);
		void StopBgm(int fadeOutFrames = 15);
	}

	public interface IScreen
	{
		void FadeOut(int frames, bool white = false);
		void FadeIn(int frames);
		bool Faded { get; }
	}

	/// <summary>The map and moving between maps.</summary>
	public interface IField
	{
		/// <summary>The current map's name (d01_05, f00...), or null.</summary>
		string Map { get; }
		/// <summary>Goes to another map, as a script's MapWarp does: name, position, facing 0-7 (eighths of a turn).</summary>
		void Warp(string map, Vector3 position, int facing = 0);
	}
}
