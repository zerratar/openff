// The engine API: the verbs a script uses to act on the game.
//
// Each group is an interface a service implements. Today the client implements them on
// the legacy game (OpenFF/Compat/EngineApi.cs), which is why they read like the FF3
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
		/// <summary>The field's message window: say a line, ask a yes/no question, know when it closed.</summary>
		public static IDialogue Dialogue => Services.Get<IDialogue>();
		/// <summary>The player's character on the map: where it is, where it looks, walking, freezing, motions.</summary>
		public static IHero Hero => Services.Get<IHero>();
		/// <summary>Characters and models on the map: spawn, move, turn, talk, and the ones the map already has.</summary>
		public static INpcs Npcs => Services.Get<INpcs>();
		/// <summary>The scripts' flag space, shared with the game's own event scripts.</summary>
		public static IFlags Flags => Services.Get<IFlags>();
		/// <summary>The party: members and their sheets, gil, the bag, equipment, experience, jobs, conditions.</summary>
		public static IParty Party => Services.Get<IParty>();
		/// <summary>Music and sound effects by the game's own names.</summary>
		public static IAudio Audio => Services.Get<IAudio>();
		/// <summary>Fades, flashes and the battle's floating numbers over the whole screen.</summary>
		public static IScreen Screen => Services.Get<IScreen>();
		/// <summary>The map: its name, warping, ground height and walkability, random encounters on and off.</summary>
		public static IField Field => Services.Get<IField>();
		/// <summary>The field camera: move, look, follow, shake, zoom, and world-to-screen for HUD markers.</summary>
		public static ICamera Camera => Services.Get<ICamera>();
		/// <summary>The game's visual effects by table id, spawned at a point or following a character.</summary>
		public static IEffects Effects => Services.Get<IEffects>();
		/// <summary>The game's own battle: start one against a monster party and hear how it ended.</summary>
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
		/// Say replaces the text. Optionally a speaker's name. A text of the form "@1000142"
		/// is one of the game's own lines by its id in the .msd, in the player's language;
		/// "@1000142 item=5001 gold=250 color=9" fills the line's item and gold codes and sets
		/// the window's text colour (dgs.TXT_COLOR; 9 is the chests' gold).
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
		/// <summary>Puts them at a position at once, no walk.</summary>
		void Teleport(Vector3 position);
		/// <summary>Turns to a yaw in degrees (0 = +Z, 90 = +X).</summary>
		void Face(float yaw);
		/// <summary>Turns to face a point on the ground.</summary>
		void LookAt(Vector3 point);
		/// <summary>Walks the hero to a point over frames (a scripted walk; freeze first so the player does not fight it).</summary>
		void MoveTo(Vector3 position, int frames);
		/// <summary>Whether walking right now.</summary>
		bool Moving { get; }
		/// <summary>Ends a walk where they stand.</summary>
		void Stop();
		/// <summary>Plays a motion by its index in the character's set (1001 is the talk pose).</summary>
		void PlayMotion(int index, bool loop = false, int blendFrames = 5);
		/// <summary>Adds a motion set to the hero's model so PlayMotion can play its ids (HeroMotion has the ids). The battle's sets: "b_b01" idle, poise, damage, death, wins; "b_b02_040" the magic motions; "b_b02_" + a weapon's graph id (Item.Model without the w) that weapon's swings; the job's own set (see BindBattleMotions); "b_b04_002" the rest. Once per map.</summary>
		void BindMotions(string set = "b_b01");
		/// <summary>Binds what the battle binds on a party member: the common set, the magic set, the current job's set and the extra set - enough for HeroMotion's ids except a weapon's swings.</summary>
		void BindBattleMotions();
		/// <summary>Whether the motion PlayMotion started has finished (looping ones never do).</summary>
		bool MotionDone { get; }
		/// <summary>
		/// Plays one of the clips the hero's own model carries (a glTF look with animations, defs/models)
		/// over whatever motion the game plays, until it ends - or, looping, until StopClip. The game's
		/// motion keeps running underneath (its timing, its walk); only the picture changes. False when
		/// the hero's look has no clip of that name.
		/// </summary>
		bool PlayClip(string clip, bool loop = false, float speed = 1f);
		/// <summary>Ends a PlayClip; the game's motion shows again (or the clip the definition maps to it).</summary>
		void StopClip();
		/// <summary>Whether a PlayClip is still showing.</summary>
		bool ClipPlaying { get; }
		/// <summary>The clips the hero's look carries, by name; none without a glTF look, or with one that has no animations.</summary>
		System.Collections.Generic.IReadOnlyList<string> Clips { get; }
		/// <summary>Draws the hero as a glTF of the mod's from now on (null: the definition's look, or the game's own) - see Npc.SetLook.</summary>
		bool SetLook(string gltf, bool fitted = false, Modding.LoadedMod mod = null);
		/// <summary>The "!" over the head.</summary>
		bool Balloon { get; set; }
		/// <summary>Takes control from the player: no walking, no menu button, the way an event does.</summary>
		void Freeze();
		/// <summary>Freeze, but with keepInput the pad stays on: the hero stands still while the player can still advance a message - what a cutscene wants.</summary>
		void Freeze(bool keepInput);
		/// <summary>Lets the hero move again after Freeze.</summary>
		void Unfreeze();
		/// <summary>Whether the hero is held still (Freeze); the field does not move them.</summary>
		bool Frozen { get; }
	}

	/// <summary>The random walk's pattern and pace, as the map scripts name them (moveCharacter_StartRandom's second operand; the game's NPC_RANDOM_MOVE_TYPE).</summary>
	public enum WanderGait
	{
		Default = 0,
		Man = 1,
		Woman = 2,
		Boy = 3,
		Girl = 4,
		Uncle = 5,
		Aunt = 6,
		OldMan = 7,
		OldWoman = 8,
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
		/// <summary>Whether the character is still on the map (not removed, the map not left).</summary>
		public abstract bool Alive { get; }
		/// <summary>Where it stands, in world units.</summary>
		public abstract Vector3 Position { get; }
		/// <summary>Which way it faces, in degrees (0 = +Z, 90 = +X).</summary>
		public abstract float Yaw { get; }
		/// <summary>Puts it at a position at once.</summary>
		public abstract void Teleport(Vector3 position);
		/// <summary>Walks to a point over a number of frames (0 teleports). The character faces where it walks.</summary>
		public abstract void MoveTo(Vector3 position, int frames);
		/// <summary>Whether walking right now.</summary>
		public abstract bool Moving { get; }
		/// <summary>Ends a walk where the character stands.</summary>
		public abstract void Stop();
		/// <summary>Turns to a yaw in degrees.</summary>
		public abstract void Face(float yaw);
		/// <summary>Turns to face a point.</summary>
		public abstract void LookAt(Vector3 point);
		/// <summary>What it does on its own: stand, wander, follow.</summary>
		public abstract void SetAi(NpcAi ai);
		/// <summary>
		/// Whether the character blocks and shoves other characters. Off by default: a solid
		/// character standing beside the hero pushes the hero away, frame after frame.
		/// </summary>
		public abstract bool Solid { get; set; }
		/// <summary>Plays a motion by its index in the character's set (1001 is the talk pose).</summary>
		public abstract void PlayMotion(int index, bool loop = false, int blendFrames = 5);
		/// <summary>
		/// The pose a motion ends in, at once: the motion set to its last frame with no blend
		/// from the pose before. For a state an object should simply be in when it appears - a
		/// chest's shut lid (1003) - where PlayMotion would be seen closing it.
		/// </summary>
		public virtual void HoldMotion(int index) { PlayMotion(index, false, 0); }
		/// <summary>Adds a motion set to the character's model: "b_b01" for a party member's model, a monster's Monster.MotionSet ("b_f" + family) for its attack and idle (MonsterMotion).</summary>
		public abstract void BindMotions(string set);
		public abstract bool MotionDone { get; }
		/// <summary>Plays one of the clips the character's own model carries (a glTF look with animations) over the game's motion, until it ends or StopClip; false without such a clip. See IHero.PlayClip.</summary>
		public virtual bool PlayClip(string clip, bool loop = false, float speed = 1f) => false;
		public virtual void StopClip() { }
		public virtual bool ClipPlaying => false;
		/// <summary>The clips the character's look carries, by name.</summary>
		public virtual System.Collections.Generic.IReadOnlyList<string> Clips => System.Array.Empty<string>();
		/// <summary>
		/// Draws this character as a glTF of the mod's from now on (null puts the game's own draw
		/// back): the file, rigged to the game's bone names, skinned by this character's model and
		/// motions - what a defs/models definition does for every instance of a model, for this one
		/// character. The path is the mod's (assets/x.glb); fitted for a fitted skeleton. False when
		/// there is no such file or no character to dress.
		/// </summary>
		public virtual bool SetLook(string gltf, bool fitted = false, Modding.LoadedMod mod = null) => false;
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

		/// <summary>
		/// Makes this character the one the map's script means by a cast number: talking to it
		/// runs that cast's code, and the script's commands on that cast (motions, moves,
		/// recolours) land here. What a mod's stand-in for one of the game's characters does
		/// to behave exactly as the original did. Nothing on a host without casts.
		/// </summary>
		public virtual void RunCast(int cast) { }

		/// <summary>
		/// RunCast without the logic: the .hich row's character index points here, so every
		/// command the script addresses to the cast lands on this character, but talking to it
		/// runs nothing of the game's - a CastScript's code, started by the component, does.
		/// </summary>
		public virtual void BindCast(int cast) { }

		/// <summary>
		/// Sets the character up as a treasure chest the game's way (setTreasureItem /
		/// setTreasureMoney): the item or gil, the game's flag for it (opened when set), the
		/// chest's own opening - sound, lid, message, flag, the treasure count. A chest model
		/// (o000, o001) spawned as a character.
		/// </summary>
		public virtual void SetTreasure(int itemId, int gil, int flagGroup, int flagIndex) { }

		/// <summary>
		/// The opposite of SetTreasure: the game's own chest logic steps aside for this map
		/// object (o001 is a treasure box to the game, which would open it itself on A, with its
		/// own flag and message), so a component - the Chest - runs the opening. The player's
		/// talk still reaches Interacted.
		/// </summary>
		public virtual void OwnChest() { }

		/// <summary>The game's changeColorCharacter: the model's texture replaced by a variant named &lt;model&gt;_&lt;variant&gt; (n021 with "n024").</summary>
		public virtual void Recolour(string variant) { }

		/// <summary>
		/// The map scripts' moveCharacter_StartRandom, exactly: the character walks about its spot
		/// on its own, with the gait the script names (a boy's, an old woman's - the pattern and
		/// pace of the random walk). SetAi(Wander) alone keeps the gait it had.
		/// </summary>
		public virtual void StartWander(WanderGait gait) { SetAi(NpcAi.Wander); }

		/// <summary>The scripts' moveCharacter_EndRandom: the walk stops, the character stands where it is.</summary>
		public virtual void EndWander() { SetAi(NpcAi.Still); }

		/// <summary>
		/// Runs one command of the map script's language as its boot would have - the line as
		/// Crystal's disassembly writes it: "setCharacterDetectionRadius(21, 12)",
		/// "setSignEffect(15, 1, 22, 0, 0x5000, 0, 0, 0)". Cast numbers in it resolve as the
		/// script's do, so after RunCast(n) a command on cast n lands on this character. What
		/// GameCast.Setup replays, so that nothing the boot did is approximated. False, with the
		/// reason in the log, when the line does not parse or the host has no script engine.
		/// </summary>
		public virtual bool RunScript(string line) { return false; }

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

		/// <summary>
		/// Puts a character on the map the way the map scripts' bootPlainCharacter does: the
		/// light walker with the model's own scale and kind (the children's models at 0.8, the
		/// chocobo, the frog, the fairy). The stand-in for a character the script booted that
		/// way; Spawn is the bootCharacter kind.
		/// </summary>
		Npc SpawnPlain(string model, Vector3 position, float yaw = 0f);

		/// <summary>The map's own character by its player slot, as a handle: move it, turn it, hear Interacted when the hero talks to it (the map's own script still runs). Null off a map or for no such character. A scene file's object:&lt;n&gt; is a .hich row, not a slot - ByRow is for that.</summary>
		Npc Existing(int index);

		/// <summary>The character the map's .hich row was booted into (the editor's Characters, a scene file's object:&lt;n&gt;), or null while nothing has booted it.</summary>
		Npc ByRow(int row);
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
		/// <summary>
		/// How the character grows, from the mod's character definition: "jobs" (FF3's system, the
		/// default), "class" (FF4's: one class for good, learning by level) or "mastery" (FF5's: job
		/// ladders climbed on ABP, learned abilities set into free command slots).
		/// </summary>
		public string Progression { get; set; } = "jobs";
		/// <summary>Mastery: the abilities in play in the held job - innate passives and what is set into the free slots.</summary>
		public List<AbilityInfo> Abilities { get; } = new List<AbilityInfo>();
		/// <summary>Mastery: everything learned from the job ladders so far.</summary>
		public List<AbilityInfo> Learned { get; } = new List<AbilityInfo>();
		/// <summary>Mastery: the ability set into each free slot of the held job (0 for none) - as many entries as the job leaves free.</summary>
		public int[] Slots { get; set; } = System.Array.Empty<int>();
		/// <summary>Mastery: the held job's four battle commands as laid out - the job's own by ability, a free slot as an entry with Id -1 (what is set in it is in Slots).</summary>
		public List<AbilityInfo> Commands { get; } = new List<AbilityInfo>();
		/// <summary>The job really held, by word: FF3's ("knight") or a mod's own from defs/jobs ("samurai", standing on the FF3 job in Job).</summary>
		public string JobWord { get; set; }
		/// <summary>The held job's name as the menus print it.</summary>
		public string JobTitle { get; set; }
		public override string ToString() => Name + " L" + Level + " (" + Hp + "/" + MaxHp + " hp, " + JobName + ")";
	}

	/// <summary>An ability on the mastery progression: one of the game's battle commands or passives, or a passive of the mod's own.</summary>
	public sealed class AbilityInfo
	{
		/// <summary>The game's ability id (1 Attack, 3 Guard, 7 Steal, 10 Cover...) or 100 up for a mod's own passive.</summary>
		public int Id { get; set; }
		/// <summary>The word a definition uses: "cover", "white-magic", or the mod passive's own.</summary>
		public string Word { get; set; }
		public string Name { get; set; }
		public bool Passive { get; set; }
		public override string ToString() => Name + (Passive ? " (passive)" : "");
	}

	/// <summary>A stat by name, for IParty.SetStat.</summary>
	public enum Stat { Strength, Vitality, Agility, Intellect, Mind }

	/// <summary>The party: money, items, members.</summary>
	public interface IParty
	{
		/// <summary>The party's money; set it to give or take.</summary>
		int Gil { get; set; }
		/// <summary>Puts items in the bag (Game.Items.Find for the id).</summary>
		void AddItem(int itemId, int count);
		/// <summary>How many of an item the party carries.</summary>
		int ItemCount(int itemId);
		/// <summary>The characters in the party, in slot order.</summary>
		IReadOnlyList<PartyMember> Members { get; }
		/// <summary>A character by the game's id, in the party or not.</summary>
		PartyMember Member(int id);
		/// <summary>Puts a character into the party (the game's id); false when the party is full or the id unknown.</summary>
		bool AddMember(int id);
		/// <summary>Takes a character out of the party; false when not in it.</summary>
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
		/// <summary>Takes a spell out of the character's slots; false when they did not have it.</summary>
		bool ForgetSpell(int id, int spellId);
		/// <summary>Puts a character into conditions (Poison, Blind...).</summary>
		void Inflict(int id, Condition conditions);
		/// <summary>Takes conditions off.</summary>
		void Cure(int id, Condition conditions);
		/// <summary>The bag: every item the party carries and how many.</summary>
		IReadOnlyList<ItemStack> Items { get; }
		/// <summary>Takes items out of the bag; false when there are not that many.</summary>
		bool RemoveItem(int itemId, int count);
		/// <summary>Equips an item from the bag on a character (the item's own slot when Auto); false when they cannot wear it or the bag has none.</summary>
		bool Equip(int id, int itemId, EquipSlot slot = EquipSlot.Auto);
		/// <summary>Takes off what a slot holds, back into the bag.</summary>
		void Unequip(int id, EquipSlot slot);
		/// <summary>The item in a slot, or 0.</summary>
		int Equipped(int id, EquipSlot slot);

		// ---- the mastery progression (FF5's way; defs/characters "progression": "mastery", defs/jobs ladders) ----

		/// <summary>Sets a learned ability into a free command slot of the character's held job (0 clears it); false when not on the progression, not learned, or no such slot. The battle commands follow at once.</summary>
		bool SetAbility(int id, int slot, int abilityId);
		/// <summary>Gives ABP toward a job's ladder, climbing it as battles do; returns how many steps were reached (abilities learned). Nothing without a ladder for the job.</summary>
		int GiveAbp(int id, Job job, int amount);
		/// <summary>The level a character has reached on a job's ladder (0 at the bottom or without a ladder).</summary>
		int JobLevel(int id, Job job);
		/// <summary>ABP gathered toward the next step of a job's ladder.</summary>
		int Abp(int id, Job job);
		/// <summary>Whether a job's ladder is climbed to the top.</summary>
		bool Mastered(int id, Job job);
		/// <summary>An ability by its word ("cover", "white-magic", a mod passive's own word) or the game's name; null for none.</summary>
		AbilityInfo Ability(string word);
		/// <summary>Whether an ability is in play for the character now: innate in the held job or set into a free slot.</summary>
		bool HasAbility(int id, int abilityId);
		/// <summary>
		/// A mastery hero takes a job by word - one of FF3's ("knight") or a job of the mod's own from
		/// defs/jobs ("samurai") - with no penalty, the field figure following; false when the hero is not
		/// on the progression, the job is unknown, not yet opened by the crystals (a mod's job needs its
		/// base open), or fixed. A mod's job stands on an FF3 base the game's party holds (PartyMember.Job
		/// is the base; JobWord and JobTitle say the job really held).
		/// </summary>
		bool ChangeJob(int id, string job);
		/// <summary>The jobs a character may take now, by word: FF3's the crystals have opened and the mod's own whose base is open.</summary>
		IReadOnlyList<string> OpenJobs(int id);
		/// <summary>Every job in play, by word: FF3's 23 then the mods' own, open or not.</summary>
		IReadOnlyList<string> AllJobs { get; }
		/// <summary>A job as it stands for a character: its ladder's level and ABP, whether open, held, mastered; null for no such job.</summary>
		JobInfo JobInfo(int id, string job);
	}

	/// <summary>A job as it stands for one character, for menus (IParty.JobInfo).</summary>
	public sealed class JobInfo
	{
		/// <summary>The word ("knight", "samurai").</summary>
		public string Word { get; set; }
		/// <summary>The name the menus print.</summary>
		public string Title { get; set; }
		/// <summary>A job of a mod's own (standing on an FF3 base) rather than one of FF3's.</summary>
		public bool Own { get; set; }
		/// <summary>Whether the character may take it now.</summary>
		public bool Open { get; set; }
		/// <summary>Whether the character holds it.</summary>
		public bool Held { get; set; }
		/// <summary>Whether it has a ladder (mastery progression).</summary>
		public bool HasLadder { get; set; }
		public int Level { get; set; }
		public int Abp { get; set; }
		/// <summary>ABP the next step costs; 0 at the top or without a ladder.</summary>
		public int AbpToNext { get; set; }
		public int Steps { get; set; }
		public bool Mastered { get; set; }
		/// <summary>The next ability the ladder teaches, or null.</summary>
		public AbilityInfo Next { get; set; }
		public override string ToString() => Title + (HasLadder ? " Lv " + Level : "");
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
		/// <summary>The colour behind everything the 3D scene does not cover - black on the game's maps. What a map of the mod's own has for a sky; set it back on leaving.</summary>
		Color Background { get; set; }
		/// <summary>A short line over the game, top right, for a few seconds - the way ABP and learned abilities are announced. Never blocks a scene.</summary>
		void Notice(string text);
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
		/// <summary>Whether a wall (the map's, or a Solid Mesh) stands in the way of a step from one point to the next, for a body of that radius - the test the hero's own walk makes. False where there is no wall.</summary>
		bool Blocked(Vector3 from, Vector3 to, float radius = 3f);
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
		/// <summary>
		/// Sets the follow camera's frame as a map's parameters would: where the camera stands
		/// relative to the hero (the game's maps use about (0, 110, 110): high and on the +z side,
		/// which the field controls are laid out for - a negative Z puts it opposite and turns right into left),
		/// where it looks relative to the hero ((0, 10, 0): a little above the feet), and how
		/// far the player may zoom in (0 for no zoom). What a map of the mod's own has instead
		/// of a parameter file; Reset puts the map's own back.
		/// </summary>
		void Configure(Vector3 positionOffset, Vector3 targetOffset, float zoomRange = 60f);
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

namespace OpenFF
{
	/// <summary>The battle's motion ids for a party member's model, playable on the field after Hero.BindBattleMotions() (idle/poise/damage/death/wins are in "b_b01", MagicPoise/MagicShot in "b_b02_040", a weapon's swings in its own "b_b02_" set).</summary>
	public static class HeroMotion
	{
		public const int Idle = 101, Poise = 201, PoiseNearDeath = 301, PoisePoison = 401, PoiseMagic = 501;
		public const int Front = 601, Back = 604;
		public const int UseItem = 701, Escape = 702, GuardStart = 703, Guard = 704, Damage = 705, Death = 706, Comeback = 707;
		public const int Hand1 = 1101, Hand2 = 1102, HandFinish = 1107;
		public const int ShortSword1 = 1201, ShortSword2 = 1202, ShortSwordFinish = 1207;
		public const int LongSword1 = 1301, LongSword2 = 1302, LongSwordFinish = 1307;
		public const int Katana1 = 1401, Axe1 = 1501, Spear1 = 1601, Rod1 = 1701, RodFinish = 1707, Bow = 1801, BowFinish = 1807;
		public const int Throw = 2101, Harp = 2201, Bell1 = 2301, Book1 = 2401;
		public const int MagicPoise = 4001, MagicShotStart = 4002, MagicShot = 4003;
		public const int Win1 = 4101, Win2 = 4102, Win3 = 4103, Win4 = 4104;
		public const int LevelUp1 = 4201, LevelUp2 = 4202, LevelUp3 = 4203, LevelUp4 = 4204;
		public const int Cover = 6001, Steal = 6101, Check = 6201, Geomancy = 6301, JumpStart = 6401, JumpEnd = 6403, Provoke = 6501, Dark = 6601, Song = 6901;
	}

	/// <summary>The battle's motion ids for a monster's model, playable after Npc.BindMotions(monster.MotionSet).</summary>
	public static class MonsterMotion
	{
		public const int Idle = 101, Attack = 201, Special = 202;
	}

	public enum ItemCategory { Consumable = 0, Weapon = 1, Armor = 2, Magic = 3, Key = 4 }

	/// <summary>Where a piece of equipment goes.</summary>
	public enum EquipSlot { RightHand = 0, LeftHand = 1, Head = 2, Body = 3, Arm = 4, Auto = -1, None = -2 }

	/// <summary>An item from the game's tables.</summary>
	public sealed class Item
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Caption { get; set; }
		public ItemCategory Category { get; set; }
		public int Price { get; set; }
		/// <summary>Jobs that may equip it, a bitmask over Job; 0 for anything that is not equipment.</summary>
		public int Jobs { get; set; }
		/// <summary>Where it is worn: a weapon in a hand, a shield in the left, helmet/armour/gauntlet on head/body/arm.</summary>
		public EquipSlot Slot { get; set; } = EquipSlot.None;
		public int Attack { get; set; }
		public int Accuracy { get; set; }
		public int Defense { get; set; }
		public int MagicDefense { get; set; }
		public int Evasion { get; set; }
		public int MagicEvasion { get; set; }
		public Element Elements { get; set; }
		/// <summary>Armour: elements it halves; weapons: none.</summary>
		public Element Resist { get; set; }
		public Element Weakness { get; set; }
		/// <summary>Stat bonuses while equipped.</summary>
		public Stats Bonus { get; set; } = new Stats();
		public int Weight { get; set; }
		/// <summary>The weapon's model ("w" + graph id), for a hand.</summary>
		public string Model { get; set; }
		public bool UsableInBattle { get; set; }
		public bool UsableInField { get; set; }
		public override string ToString() => (Name ?? ("item " + Id)) + " (" + Category + (Price > 0 ? ", " + Price + " gil" : "") + ")";
	}

	public sealed class ItemStack
	{
		public int ItemId { get; set; }
		public int Count { get; set; }
		public override string ToString() => ItemId + " x" + Count;
	}

	/// <summary>The game's items as data: consumables, weapons, armour, magic (also spells), key items.</summary>
	public interface IItems
	{
		IReadOnlyList<Item> All { get; }
		Item Find(int id);
		Item Find(string name);
		IEnumerable<Item> Of(ItemCategory category);
	}

	/// <summary>One of a shop table's shops.</summary>
	public sealed class ShopInfo
	{
		public int Index { get; set; }
		/// <summary>0 weapons, 1 armour, 2 magic, 3 items.</summary>
		public int Kind { get; set; }
		public List<int> ItemIds { get; } = new List<int>();
	}

	/// <summary>
	/// The game's own script language, run by the engine: a cast's code as text - the lines
	/// Crystal's disassembly writes (talkBegin, a message window, flag tests and jumps, end) -
	/// compiled and run on the game's script loop under the engine's own script, beside the
	/// map's. Every command means exactly what it means in the game, since it is the game's
	/// interpreter that runs it; what is the engine's is where the code lives (a scene file, a
	/// component the editor shows and a modder edits) and who starts it (a component's
	/// Activate, not the game's talk). FF3's command set; FF4's comes with its script work.
	/// </summary>
	public interface IScripts
	{
		/// <summary>Defines (or replaces) the code a cast number runs on this map: a label per function is not needed, the lines are one function's body. False, with the problems logged, when it does not compile.</summary>
		bool Define(int cast, IReadOnlyList<string> lines);
		/// <summary>Starts the cast's code, as the game's talk starts a cast's main. False when nothing is defined for it or it is running already.</summary>
		bool Start(int cast);
		/// <summary>Whether the cast's code is running.</summary>
		bool IsRunning(int cast);
		/// <summary>A cast number no cast of the map's script has, for an object of the mod's own with code: counted from 5000 on this map. In the lines, "@me" stands for it.</summary>
		int Allocate();
	}

	/// <summary>The game's shop screens.</summary>
	public interface IShops
	{
		/// <summary>Opens the game's shop screen: shop number index of a shop table ("t01" is the first town's; the current map's own when table is null). Buying and selling are the game's. Mind that the game's shop is a map of its own: the field leaves for the shop interior and comes back (MapLeaving/MapEntered fire, spawned characters go), so a mod that must stay on its map draws its own shop with Game.Draw and Game.Items instead. False off a map.</summary>
		bool Open(int index, string table = null);
		bool IsOpen { get; }
		/// <summary>What a shop sells, from its table.</summary>
		ShopInfo Info(int index, string table = null);
	}

	/// <summary>A model of the mod's own standing on the map: a glTF file drawn by the client directly, moved and removed through this.</summary>
	public abstract class MeshHandle
	{
		/// <summary>The file it was loaded from (assets/hut.glb in the mod).</summary>
		public abstract string Path { get; }
		/// <summary>Where it stands; set to move it.</summary>
		public abstract Vector3 Position { get; set; }
		/// <summary>Its turn about the up axis, in degrees (the scene's yaw).</summary>
		public abstract float Yaw { get; set; }
		/// <summary>Its scale; the file's own units times this.</summary>
		public abstract float Scale { get; set; }
		public abstract bool Hidden { get; set; }
		/// <summary>Whether its triangles are ground and walls to the characters: a floor to stand on (faces up), a wall to bump into (faces sideways). Off, it is walked through. The bind pose's triangles, at the mesh's place.</summary>
		public abstract bool Solid { get; set; }
		/// <summary>The file's extent, in its own units before Scale - to stand it on the ground, to size a box around it.</summary>
		public abstract Vector3 Min { get; }
		public abstract Vector3 Max { get; }
		/// <summary>How many triangles it draws.</summary>
		public abstract int Triangles { get; }
		/// <summary>What went wrong reading it, or null.</summary>
		public abstract string Problem { get; }
		/// <summary>The animation clips the file has, by name (Blender's actions).</summary>
		public abstract IReadOnlyList<string> Clips { get; }
		/// <summary>The clip playing, or null.</summary>
		public abstract string Clip { get; }
		/// <summary>Plays a clip by name from its start, looping or once; false when the file has none by that name.</summary>
		public abstract bool Play(string clip, bool loop = true, float speed = 1f);
		/// <summary>Back to the bind pose.</summary>
		public abstract void Stop();
		/// <summary>Takes it off the map.</summary>
		public abstract void Remove();
	}

	/// <summary>
	/// Models in the mod's own format - glTF (.glb, .gltf), as Blender exports them - drawn by
	/// the OpenFF client with the field's camera, on top of the game's own scene. No DS format
	/// in between: this is what an OpenFF mod may do that a Steam mod cannot. A scene object
	/// whose Model names such a file (assets/hut.glb) is one of these; from code, Spawn.
	/// Read: meshes, materials with a base colour and texture, the node tree; not yet: skins,
	/// animations, collision (a mesh is walked through - a Solid box is on the list).
	/// </summary>
	public interface IMeshes
	{
		/// <summary>Puts a model on the map at a position, turned by yaw degrees, scaled; the path is the mod's own file (relative to the mod's folder) or absolute.</summary>
		MeshHandle Spawn(string path, Vector3 position, float yaw = 0f, float scale = 1f);
		/// <summary>Every mesh standing.</summary>
		IReadOnlyList<MeshHandle> All { get; }
	}

	public static partial class Game
	{
		/// <summary>The item tables as data: names, categories, prices, who can equip what, stats.</summary>
		public static IItems Items => Services.Get<IItems>();
		/// <summary>The game's shop screen, opened on any map with any shop table; what a shop sells.</summary>
		public static IShops Shops => Services.Get<IShops>();
		public static IScripts Scripts => Services.Get<IScripts>();
		/// <summary>The mod's own models (glTF), drawn by the client.</summary>
		public static IMeshes Meshes => Services.Get<IMeshes>();
	}
}
