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
// Positions are in world units, the map's own (the legacy code keeps them in 1/4096ths).
// For a feel of the scale: two characters standing together are about 8 units apart, and
// a talk from arm's length happens within 12 or so. Yaw is in degrees about the up axis.

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
		public static ICamera Camera => Services.Get<ICamera>();
		public static IEffects Effects => Services.Get<IEffects>();
		public static IBattle Battle => Services.Get<IBattle>();
	}

	public enum BattleResult { Unknown, Won, Lost, Escaped }

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

		/// <summary>
		/// A yes/no question: the text in the message window and the game's own Yes/No box.
		/// The answer comes back once, then both close. While IsAsking, Say waits its turn.
		/// </summary>
		void Ask(string question, Action<bool> answered);
		bool IsAsking { get; }
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
		void LookAt(Vector3 point);
		/// <summary>Walks the hero to a point over frames (a scripted walk; freeze first so the player does not fight it).</summary>
		void MoveTo(Vector3 position, int frames);
		bool Moving { get; }
		void Stop();
		/// <summary>Plays a motion by its index in the character's set (1001 is the talk pose).</summary>
		void PlayMotion(int index, bool loop = false, int blendFrames = 5);
		/// <summary>The "!" over the head.</summary>
		bool Balloon { get; set; }
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
		/// <summary>Walks to a point over a number of frames (0 teleports). The character faces where it walks.</summary>
		public abstract void MoveTo(Vector3 position, int frames);
		public abstract bool Moving { get; }
		/// <summary>Ends a walk where the character stands.</summary>
		public abstract void Stop();
		public abstract void Face(float yaw);
		public abstract void LookAt(Vector3 point);
		public abstract void SetAi(NpcAi ai);
		/// <summary>
		/// Whether the character blocks and shoves other characters. Off by default: a solid
		/// character standing beside the hero pushes the hero away, frame after frame.
		/// </summary>
		public abstract bool Solid { get; set; }
		/// <summary>Plays a motion by its index in the character's set (1001 is the talk pose).</summary>
		public abstract void PlayMotion(int index, bool loop = false, int blendFrames = 5);
		/// <summary>Opacity, 0 (gone) to 100.</summary>
		public abstract int Alpha { get; set; }
		/// <summary>Drawn or not; a hidden character is still there.</summary>
		public abstract bool Hidden { get; set; }
		/// <summary>The "!" over the head.</summary>
		public abstract bool Balloon { get; set; }
		/// <summary>Size, 1 = as modelled.</summary>
		public abstract float Scale { get; set; }
		/// <summary>Takes the character off the map.</summary>
		public abstract void Remove();

		/// <summary>How close the hero must be, in world units, for A to count as talking to this one (two characters together stand about 8 apart).</summary>
		public float InteractRadius { get; set; } = 14f;

		/// <summary>
		/// The player talked to this character: pressed A within InteractRadius, or did what
		/// the game itself counts as talking to it (a tap on it, or A while facing it).
		/// </summary>
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

		/// <summary>
		/// Puts any character-format model on the map as a plain figure - a monster
		/// (b_m005), an object, a character without the walker's behaviour - with no AI and
		/// no talk of its own. Moved, turned, scaled, hidden and removed like a spawned
		/// character; the raw material of a real-time fight or a set piece.
		/// </summary>
		Npc SpawnModel(string model, Vector3 position, float yaw = 0f, float scale = 1f);
	}

	/// <summary>The game's flag space: what the scripts store quest progress in.</summary>
	public interface IFlags
	{
		bool Get(uint group, uint index);
		void Set(uint group, uint index, bool value);
	}

	/// <summary>One of the party's characters as the game keeps them.</summary>
	public sealed class PartyMember
	{
		/// <summary>The game's id for the character (FF3: 0 Luneth, 1 Arc, 2 Refia, 3 Ingus).</summary>
		public int Id { get; set; }
		/// <summary>Position in the party, 0-3, or -1 when not in it.</summary>
		public int Slot { get; set; }
		public string Name { get; set; }
		public int Level { get; set; }
		public int Experience { get; set; }
		public int Hp { get; set; }
		public int MaxHp { get; set; }
		/// <summary>Charges left of magic level 1 (the game's MP are charges per level; see Charges).</summary>
		public int Mp { get; set; }
		/// <summary>Charges left per magic level, index 0 = level 1 .. 7 = level 8.</summary>
		public int[] Charges { get; set; } = new int[8];
		public int[] MaxCharges { get; set; } = new int[8];
		/// <summary>The job's index in the game's job table (the Job enum names them).</summary>
		public int Job { get; set; }
		public Job JobName => (Job)Job;
		/// <summary>The job's skill level, as the formulas use it.</summary>
		public int JobSkill { get; set; }
		/// <summary>Stats with equipment and job bonuses, as the formulas read them.</summary>
		public Stats Stats { get; set; } = new Stats();
		public Condition Conditions { get; set; }
		/// <summary>The spells equipped, by id, in the order of their levels.</summary>
		public List<int> Spells { get; } = new List<int>();
		public bool Alive { get; set; }
		public override string ToString() => Name + " L" + Level + " (" + Hp + "/" + MaxHp + " hp, " + JobName + ")";
	}

	/// <summary>A stat by name, for IParty.SetStat.</summary>
	public enum Stat { Strength, Vitality, Agility, Intellect, Mind }

	/// <summary>The party: money, items, members.</summary>
	public interface IParty
	{
		int Gil { get; set; }
		void AddItem(int itemId, int count);
		/// <summary>How many of an item the party carries.</summary>
		int ItemCount(int itemId);
		/// <summary>The characters in the party, in slot order.</summary>
		IReadOnlyList<PartyMember> Members { get; }
		/// <summary>A character by the game's id, in the party or not.</summary>
		PartyMember Member(int id);
		/// <summary>Puts a character into the party (the game's id); false when the party is full or the id unknown.</summary>
		bool AddMember(int id);
		bool RemoveMember(int id);
		/// <summary>Sets a character's level, growing their parameters as the game does.</summary>
		void SetLevel(int id, int level);
		/// <summary>Heals everyone to full.</summary>
		void HealAll();
		/// <summary>Takes hit points off a character; with canKill false it stops at 1 (as the game's floors do), otherwise 0 kills. Returns the new HP.</summary>
		int Hurt(int id, int amount, bool canKill = false);
		/// <summary>Gives hit points back, up to the maximum (a dead character stays dead unless revive is true). Returns the new HP.</summary>
		int Heal(int id, int amount, bool revive = false);
		/// <summary>Sets HP now and, when max is given, the maximum too.</summary>
		void SetHp(int id, int now, int max = -1);
		/// <summary>Sets the charges of one magic level (1-8) now and, when max is given, the maximum too.</summary>
		void SetCharges(int id, int level, int now, int max = -1);
		/// <summary>Adds experience, levelling up as the game does; true when a level was gained.</summary>
		bool GiveExperience(int id, int amount);
		/// <summary>Changes job, with the game's own bookkeeping (abilities, charges, the penalty time).</summary>
		void SetJob(int id, Job job);
		/// <summary>Sets a base stat (bonuses are recomputed).</summary>
		void SetStat(int id, Stat stat, int value);
		/// <summary>Equips a spell into the character's slots for its level; false when the slots are full.</summary>
		bool LearnSpell(int id, int spellId);
		bool ForgetSpell(int id, int spellId);
		/// <summary>Puts a character into conditions (Poison, Blind...).</summary>
		void Inflict(int id, Condition conditions);
		/// <summary>Takes conditions off.</summary>
		void Cure(int id, Condition conditions);
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
		/// <summary>Flashes the screen a colour, as the game does for a damage floor: total frames, frames per flash.</summary>
		void Flash(Color color, int frames = 8, int interval = 2);
		/// <summary>The battle's floating number over a world point: white for damage, green-pink for healing (heal = true). Up to 9999.</summary>
		void PopNumber(Vector3 at, int value, bool heal = false);
		/// <summary>The battle's "Miss" over a world point.</summary>
		void PopMiss(Vector3 at);
	}

	/// <summary>The map and moving between maps.</summary>
	public interface IField
	{
		/// <summary>The current map's name (d01_05, f00...), or null.</summary>
		string Map { get; }
		/// <summary>Goes to another map, as a script's MapWarp does: name, position, facing 0-7 (eighths of a turn).</summary>
		void Warp(string map, Vector3 position, int facing = 0);
		/// <summary>Whether walking can start the game's random battles. Off for a mod that runs its own fights.</summary>
		bool Encounters { get; set; }
		/// <summary>The height of the walkable ground under a point (looking down from a little above it), or null where there is none - off the map, over a pit.</summary>
		float? GroundHeight(Vector3 at);
		/// <summary>The point moved down (or up) onto the ground; the point itself where there is no ground.</summary>
		Vector3 OnGround(Vector3 at);
		/// <summary>Whether something could stand there: ground under it, no wall between here and the ground.</summary>
		bool Walkable(Vector3 at);
	}

	/// <summary>The game's own battles, started from a mod.</summary>
	public interface IBattle
	{
		/// <summary>Starts the game's battle with a monster party (the game's table) on a battle background; BattleEnded follows.</summary>
		void Start(int monsterParty, int battleMap = 0);
		/// <summary>Whether the party may run from battles.</summary>
		bool EscapeAllowed { get; set; }
		bool InBattle { get; }
	}

	/// <summary>The field camera.</summary>
	public interface ICamera
	{
		/// <summary>Where the camera is.</summary>
		Vector3 Position { get; }
		/// <summary>What it looks at.</summary>
		Vector3 Target { get; }
		/// <summary>Frees the camera from the hero and puts it at a point (it keeps its target).</summary>
		void MoveTo(Vector3 position);
		/// <summary>Frees the camera from the hero and points it at a point (it keeps its place).</summary>
		void LookAt(Vector3 target);
		/// <summary>Back to following the hero, the map's own way.</summary>
		void Follow();
		/// <summary>Shakes for a number of frames; strength in world units, speed in frames per swing.</summary>
		void Shake(int frames, float strength = 1f, int speed = 2);
		/// <summary>Zoom in degrees of the map's default (positive closer), as the scripts set it.</summary>
		void Zoom(int degrees);
		/// <summary>Restores the map's camera settings.</summary>
		void Reset();
		/// <summary>Where a world point falls on the screen, in the 800x480 units Game.Draw uses; null when it is behind the camera. For HUD markers, names over heads, health bars.</summary>
		Vector2? WorldToScreen(Vector3 world);
		/// <summary>Whether a world point is in front of the camera and inside the screen.</summary>
		bool OnScreen(Vector3 world);
	}

	/// <summary>Particle and sprite effects, by the game's own effect table.</summary>
	public interface IEffects
	{
		/// <summary>Starts an effect (category and member as the scripts number them) at a point; the id to remove it, or -1.</summary>
		int Spawn(int category, int member, Vector3 position);
		void Remove(int id);
		bool Alive(int id);
		/// <summary>Loads an effect pack (e + category as three digits + .efp: the battle spells' packs) so its members can be spawned on this map. The field has room for a few at a time; false when none is left.</summary>
		bool Load(int category);
		/// <summary>Whether a pack is loaded on this map (the game's own field packs count as loaded for their categories).</summary>
		bool Loaded(int category);
		void Move(int id, Vector3 position);
		void Scale(int id, float scale);
		void Pause(int id, bool paused);
		/// <summary>Keeps the effect on a character (plus an offset in world units) while both live.</summary>
		void Follow(int id, Npc target, Vector3 offset = default);
		void FollowHero(int id, Vector3 offset = default);
	}
}
