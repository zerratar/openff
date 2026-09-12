// The Abilities menu for heroes on the mastery progression (FF5's way; ProgressionLayer):
// from the client's Esc menu, when such a hero is in the party. Three pages - the heroes,
// one hero (the free command slots of their job, each set from what they have learned,
// then every job ladder's level and ABP), and the pick for a slot. Drawn as the client's
// own menus are (Ui), so it fits any window; the game waits behind it. A mod can set the
// same things from C# through Game.Party.SetAbility.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenFF.Data;

namespace OpenFF.Client
{
	using Game = Microsoft.Xna.Framework.Game;
	using GameTime = Microsoft.Xna.Framework.GameTime;
	using Color = Microsoft.Xna.Framework.Color;

	internal sealed class AbilitiesMenu : DrawableGameComponent
	{
		private const float ListTop = 100f;
		private const float RowHeight = 22f;

		public static AbilitiesMenu Instance { get; private set; }
		public static bool IsOpen => Instance != null && Instance._page != Page.Closed;

		private enum Page { Closed, Heroes, Hero, Pick, Jobs }
		private Page _page = Page.Closed;
		private int _selected, _scroll;
		private int _hero;          // the hero's id (slot 0..3) on the Hero and Pick pages
		private int _slot;          // the free slot being filled on the Pick page
		private string _note = "";
		private int _previousPad;
		private KeyboardState _previousKeys;
		private bool _mouseWasDown;
		private readonly HashSet<Keys> _wasDown = new HashSet<Keys>();
		private SpriteBatch _batch;

		/// <summary>One row of the current page: a label, a value on the right, and whether it can be chosen.</summary>
		private sealed class Row { public string Label; public string Value; public bool Pick = true; public int Tag; public bool Heading; }
		private List<Row> _rows = new List<Row>();

		private AbilitiesMenu(Game game) : base(game)
		{
			DrawOrder = int.MaxValue - 3;
			UpdateOrder = int.MaxValue - 3;
		}

		public static void Attach(Game game)
		{
			Instance = new AbilitiesMenu(game);
			game.Components.Add(Instance);
		}

		public static void Open()
		{
			if (Instance == null || IsOpen) return;
			Instance._page = Page.Heroes;
			Instance._selected = 0;
			Instance._scroll = 0;
			Instance._note = "";
			Instance._previousKeys = Keyboard.GetState();
			Instance._previousPad = DesktopInput.RawPadBits();
			Instance._wasDown.Clear();
			foreach (Keys k in Watched) if (Down(Instance._previousKeys, k)) Instance._wasDown.Add(k);
			Instance.Build();
			// One hero on the progression: straight to them.
			List<int> heroes = Instance.Heroes();
			if (heroes.Count == 1) { Instance._hero = heroes[0]; Instance._page = Page.Hero; Instance._selected = 0; Instance.Build(); }
			Log.Write(LogChannel.General, "abilities: opened");
		}

		private void Close()
		{
			_page = Page.Closed;
			Log.Write(LogChannel.General, "abilities: closed");
		}

		private static readonly Keys[] Watched = { Keys.Up, Keys.Down, Keys.Left, Keys.Right, Keys.W, Keys.A, Keys.S, Keys.D, Keys.Enter, Keys.Space, Keys.Z, Keys.X, Keys.Back, Keys.Escape };
		private static bool Down(KeyboardState keys, Keys key) => keys.IsKeyDown(key) || DesktopInput.Injected.Contains(key);
		private bool Pressed(KeyboardState keys, Keys key) => Down(keys, key) && !_wasDown.Contains(key);

		/// <summary>The heroes in the party on the mastery progression, by id.</summary>
		private List<int> Heroes()
		{
			List<int> ids = new List<int>();
			try
			{
				for (byte i = 0; i < 4; i++)
				{
					GlobalScope.pl.Player p = GlobalScope.pl.PlayerParty.instance().player(i);
					if (p != null && p.isEnable() && ProgressionLayer.IsMastery(p.playerId())) ids.Add(p.playerId());
				}
			}
			catch (Exception) { }
			return ids;
		}

		private static GlobalScope.pl.Player PlayerOf(int id)
		{
			try { return GlobalScope.pl.PlayerParty.instance().playerForId((byte)id); } catch (Exception) { return null; }
		}

		private static string JobName(int job) => ProgressionLayer.JobName(job);

		/// <summary>An ability's name as FF5 writes them: commands with a leading "!", passives plain.</summary>
		private static string Shown(int id)
		{
			return (ProgressionLayer.IsPassive(id) ? "" : "!") + ProgressionLayer.AbilityName(id);
		}

		/// <summary>Which ladders teach an ability, for the pick page's right column.</summary>
		private static string FromLadders(int id)
		{
			List<string> from = ProgressionLayer.Ladders.Where(l => l.Abilities.Any(a => a.Id == id)).Select(l => l.Name).ToList();
			return from.Count > 0 ? "from " + string.Join(", ", from) : "";
		}

		/// <summary>The rows of the current page, from the party and the ladders.</summary>
		private void Build()
		{
			_rows = new List<Row>();
			switch (_page)
			{
				case Page.Heroes:
					foreach (int id in Heroes())
					{
						GlobalScope.pl.Player p = PlayerOf(id);
						if (p == null) continue;
						int job = ProgressionLayer.HeldJob(p);
						_rows.Add(new Row { Label = p.name(), Value = JobName(job) + "  Lv " + ProgressionLayer.JobLevel(id, job), Tag = id });
					}
					if (_rows.Count == 0) _rows.Add(new Row { Label = "No hero grows by abilities", Pick = false });
					break;
				case Page.Hero:
				{
					GlobalScope.pl.Player p = PlayerOf(_hero);
					if (p == null) { _rows.Add(new Row { Label = "?", Pick = false }); break; }
					int job = ProgressionLayer.HeldJob(p);
					int[] layout = ProgressionLayer.CommandLayout(job);
					ProgressionLayer.HeroState s = ProgressionLayer.StateOf(_hero);
					if (!ModCharactersLayer.JobFixed(_hero)) _rows.Add(new Row { Label = "Change job...", Value = JobName(job) + (ProgressionLayer.BaseOf(job) != job ? "  (on the " + ModCharacters.Jobs[ProgressionLayer.BaseOf(job)].Name + ")" : ""), Tag = -1 });
					_rows.Add(new Row { Label = "Commands as " + JobName(job), Heading = true, Pick = false });
					int free = 0;
					for (int i = 0; i < layout.Length; i++)
					{
						if (layout[i] >= 0) { _rows.Add(new Row { Label = "   " + (Ff3Abilities.ById(layout[i])?.Name ?? "?"), Value = "the job's", Pick = false }); continue; }
						int set = free < ProgressionLayer.FreeSlotMax ? s.Set[free] : 0;
						_rows.Add(new Row { Label = "   Free slot " + (free + 1), Value = set > 0 ? Shown(set) : "- none -", Tag = free });
						free++;
					}
					List<int> innate = ProgressionLayer.Innate(_hero, job);
					if (innate.Count > 0) _rows.Add(new Row { Label = "   Innate", Value = string.Join(", ", innate.Select(Shown)), Pick = false });
					int[] mods = ProgressionLayer.StatModifiers(_hero, job);
					if (mods.Any(m => m != 0))
					{
						string[] names = { "Str", "Agi", "Vit", "Int", "Mnd" };
						_rows.Add(new Row { Label = "   Stat modifiers", Value = string.Join("  ", Enumerable.Range(0, 5).Where(i => mods[i] != 0).Select(i => names[i] + " " + (mods[i] > 0 ? "+" : "") + mods[i])), Pick = false });
					}
					_rows.Add(new Row { Label = "Job ladders", Heading = true, Pick = false });
					foreach (ModJob ladder in ProgressionLayer.Ladders.OrderBy(l => l.JobNumber))
					{
						int j = ladder.JobNumber, level = ProgressionLayer.JobLevel(_hero, j);
						string value = ladder.Abilities.Count == 0 ? (ladder.Inherits ? "no steps; inherits mastered jobs' passives" : "no steps")
							: ProgressionLayer.IsMastered(_hero, j) ? "Lv " + level + "  mastered" : "Lv " + level + "  " + ProgressionLayer.Abp(_hero, j) + " / " + ProgressionLayer.AbpToNext(_hero, j) + " ABP";
						string next = level < ladder.Abilities.Count ? "  next: " + ladder.Abilities[level].ShownName : "";
						_rows.Add(new Row { Label = "   " + ladder.Name + (j == job ? "  (held)" : ""), Value = value + next, Pick = false });
					}
					if (ProgressionLayer.Ladders.Count == 0) _rows.Add(new Row { Label = "   No ladders: defs/jobs/<id>.json gives a job one", Pick = false });
					break;
				}
				case Page.Jobs:
				{
					// Every job the hero may take: FF3's the crystals have opened, and the mods' own whose base is open.
					GlobalScope.pl.Player p = PlayerOf(_hero);
					int held = p != null ? ProgressionLayer.HeldJob(p) : -1;
					foreach (int j in ProgressionLayer.AllJobs)
					{
						if (!ProgressionLayer.JobOpen(j)) continue;
						ModJob ladder = ProgressionLayer.LadderOf(j);
						string value = ladder == null ? "no ladder" : ladder.Abilities.Count == 0 ? (ladder.Inherits ? "inherits" : "") : ProgressionLayer.IsMastered(_hero, j) ? "mastered" : "Lv " + ProgressionLayer.JobLevel(_hero, j) + "  " + ProgressionLayer.Abp(_hero, j) + " / " + ProgressionLayer.AbpToNext(_hero, j) + " ABP";
						if (ladder != null && ladder.IsOwn) value = "on the " + ModCharacters.Jobs[ladder.BaseJob].Name + "  " + value;
						_rows.Add(new Row { Label = JobName(j) + (j == held ? "  (held)" : ""), Value = value, Tag = j, Pick = j != held });
					}
					if (_rows.Count == 0) _rows.Add(new Row { Label = "No job is open yet", Pick = false });
					break;
				}
				case Page.Pick:
				{
					GlobalScope.pl.Player p = PlayerOf(_hero);
					int job = p != null ? ProgressionLayer.HeldJob(p) : 0;
					List<int> innate = ProgressionLayer.Innate(_hero, job);
					ProgressionLayer.HeroState s = ProgressionLayer.StateOf(_hero);
					_rows.Add(new Row { Label = "- none -", Tag = 0 });
					List<int> learned = ProgressionLayer.Learned(_hero);
					// FF5's two kinds apart: the commands (marked !) first, then the passives.
					foreach (bool passives in new[] { false, true })
					{
						List<int> ids = learned.Where(id => ProgressionLayer.IsPassive(id) == passives).ToList();
						if (ids.Count == 0) continue;
						_rows.Add(new Row { Label = passives ? "Passives - work while set" : "Commands - shown in the slot's place in battle", Heading = true, Pick = false });
						foreach (int id in ids)
						{
							string kind = passives ? "passive" : "command";
							bool elsewhere = Enumerable.Range(0, ProgressionLayer.FreeSlotMax).Any(k => k != _slot && s.Set[k] == id);
							if (passives && innate.Contains(id)) kind = "innate in this job already";
							else if (elsewhere) kind = "set in another slot";
							else kind = FromLadders(id);
							_rows.Add(new Row { Label = "   " + Shown(id), Value = kind, Tag = id });
						}
					}
					if (_rows.Count == 1) _rows.Add(new Row { Label = "Nothing learned yet - ABP come from battles won", Pick = false });
					break;
				}
			}
			if (_selected >= _rows.Count) _selected = Math.Max(0, _rows.Count - 1);
			if (_rows.Count > 0 && !_rows[_selected].Pick) StepSelection(1, wrap: false);
		}

		private void StepSelection(int by, bool wrap = true)
		{
			if (_rows.Count == 0) return;
			int start = _selected;
			for (int n = 0; n < _rows.Count; n++)
			{
				int next = _selected + by;
				if (next < 0 || next >= _rows.Count)
				{
					if (!wrap) { _selected = start; break; }
					next = (next + _rows.Count) % _rows.Count;
				}
				_selected = next;
				if (_rows[_selected].Pick) return;
			}
			if (!_rows[_selected].Pick) _selected = start;
		}

		private int VisibleRows => (int)((Ui.H - 80 - ListTop) / RowHeight);

		public override void Update(GameTime gameTime)
		{
			if (_page == Page.Closed || !Game.IsActive) return;
			KeyboardState keys = Keyboard.GetState();
			int pad = DesktopInput.RawPadBits(), edge = pad & ~_previousPad;
			_previousPad = pad;
			bool up = Pressed(keys, Keys.Up) || Pressed(keys, Keys.W) || (edge & 64) != 0;
			bool down = Pressed(keys, Keys.Down) || Pressed(keys, Keys.S) || (edge & 128) != 0;
			bool confirm = Pressed(keys, Keys.Enter) || Pressed(keys, Keys.Space) || Pressed(keys, Keys.Z) || (edge & 1) != 0;
			bool cancel = Pressed(keys, Keys.X) || Pressed(keys, Keys.Back) || Pressed(keys, Keys.Escape) || (edge & 2) != 0;
			_previousKeys = keys;
			_wasDown.Clear();
			foreach (Keys k in Watched) if (Down(keys, k)) _wasDown.Add(k);

			if (up) StepSelection(-1);
			if (down) StepSelection(1);

			bool mouseDown = DesktopInput.MouseInView(out int mx, out int my);
			if (mouseDown && !_mouseWasDown)
			{
				int row = (int)((my - ListTop) / RowHeight) + _scroll;
				if (mx >= 120 && mx <= 680 && row >= 0 && row < _rows.Count && _rows[row].Pick) { _selected = row; confirm = true; }
			}
			_mouseWasDown = mouseDown;

			if (_selected < _scroll) _scroll = _selected;
			if (_selected >= _scroll + VisibleRows) _scroll = _selected - VisibleRows + 1;

			switch (_page)
			{
				case Page.Heroes:
					if (cancel) { Close(); return; }
					if (confirm && _rows.Count > 0 && _rows[_selected].Pick) { _hero = _rows[_selected].Tag; _page = Page.Hero; _selected = 0; _scroll = 0; _note = ""; Build(); }
					break;
				case Page.Hero:
					if (cancel)
					{
						if (Heroes().Count > 1) { _page = Page.Heroes; _selected = 0; _scroll = 0; Build(); }
						else Close();
						return;
					}
					if (confirm && _rows.Count > 0 && _rows[_selected].Pick)
					{
						if (_rows[_selected].Tag < 0) { _page = Page.Jobs; _selected = 0; _scroll = 0; _note = ""; Build(); }
						else { _slot = _rows[_selected].Tag; _page = Page.Pick; _selected = 0; _scroll = 0; _note = ""; Build(); }
					}
					break;
				case Page.Jobs:
					if (cancel) { _page = Page.Hero; _selected = 0; _scroll = 0; Build(); return; }
					if (confirm && _rows.Count > 0 && _rows[_selected].Pick)
					{
						if (ProgressionLayer.ChangeJob(_hero, _rows[_selected].Tag)) { _page = Page.Hero; _selected = 0; _scroll = 0; Build(); }
						else _note = "that job cannot be taken";
					}
					break;
				case Page.Pick:
					if (cancel) { _page = Page.Hero; _selected = 0; _scroll = 0; Build(); return; }
					if (confirm && _rows.Count > 0 && _rows[_selected].Pick)
					{
						int id = _rows[_selected].Tag;
						if (ProgressionLayer.SetAbility(_hero, _slot, id))
						{
							Log.Write(LogChannel.General, "abilities: " + (PlayerOf(_hero)?.name() ?? ("hero " + _hero)) + " slot " + (_slot + 1) + " = " + (id > 0 ? ProgressionLayer.AbilityName(id) : "none"));
							_page = Page.Hero; _selected = 0; _scroll = 0; Build();
						}
						else _note = "that cannot go there";
					}
					break;
			}
		}

		public override void Draw(GameTime gameTime)
		{
			if (_page == Page.Closed || RenderTest.Active) return;
			GlobalScope.Graphics graphics = GlobalScope.m_Graphics;
			if (graphics == null) return;
			if (_batch == null) _batch = new SpriteBatch(GraphicsDevice);
			Ui.Ensure(GraphicsDevice);
			Viewport view = GraphicsDevice.Viewport;
			Rectangle panel = new Rectangle(100, 40, 600, 400);
			int left = panel.X + 24, right = panel.Right - 24;
			const int titleSize = 14, rowSize = 10, hintSize = 9;
			float hintY = panel.Bottom - 22;
			int shown = Math.Min(VisibleRows, _rows.Count - _scroll);

			_batch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
			Ui.PanelPlate(_batch, panel, view);
			for (int i = 0; i < shown; i++)
			{
				int row = _scroll + i;
				Rectangle r = new Rectangle(left, (int)(ListTop + i * RowHeight), right - left, (int)RowHeight - 3);
				if (row == _selected && _rows[row].Pick) Ui.Key(_batch, r, true, view);
			}
			Ui.Fill(_batch, Ui.Scale(new Rectangle(left, panel.Y + 44, right - left, 1), view), new Color(70, 96, 170, 255));
			float hx = left;
			foreach ((Ui.PadButton button, string word) in Hints())
			{
				Ui.HintShape(_batch, button, hx, hintY, 18, view);
				hx += Ui.HintWidth(graphics, word, hintSize, 18) + 22;
			}
			_batch.End();

			graphics.SetImageOrigin(0f, 0f);
			graphics.SetImageRotation(0f);
			graphics.SetImageScale(1f, 1f);
			graphics.DrawStringStart();
			string title = _page == Page.Heroes ? "Abilities" : _page == Page.Hero ? (PlayerOf(_hero)?.name() ?? "Abilities") : _page == Page.Jobs ? "Change job" : "Set into free slot " + (_slot + 1);
			string sub = _page == Page.Heroes ? "Who to set up." : _page == Page.Hero ? "A free slot takes any ability learned from any job ladder." : _page == Page.Jobs ? "Every open job, the mod's own included; no penalty for changing." : "Learned abilities; a passive works while set.";
			Ui.Left(graphics, title, left, panel.Y + 10, Ui.LineHeight(titleSize), titleSize, Ui.Text);
			Ui.Left(graphics, sub, left + Ui.Width(graphics, title, titleSize) + 14, panel.Y + 10, Ui.LineHeight(titleSize), 9, Ui.Muted);
			for (int i = 0; i < shown; i++)
			{
				int row = _scroll + i;
				Row r = _rows[row];
				Rectangle rect = new Rectangle(left, (int)(ListTop + i * RowHeight), right - left, (int)RowHeight - 3);
				bool on = row == _selected && r.Pick;
				Color colour = on ? Ui.TextOnLit : r.Heading ? Ui.Accent : r.Pick ? Ui.Text : Ui.Muted;
				Ui.Left(graphics, r.Label, rect.X + 12, rect.Y, rect.Height, rowSize, colour);
				if (r.Value != null)
				{
					float w = Ui.Width(graphics, r.Value, rowSize);
					Ui.Left(graphics, r.Value, rect.Right - 12 - w, rect.Y, rect.Height, rowSize, on ? Ui.TextOnLit : Ui.Muted);
				}
			}
			if (_rows.Count > VisibleRows) Ui.Left(graphics, (_scroll + 1) + "-" + (_scroll + shown) + " of " + _rows.Count, right - 90, panel.Bottom - 52, Ui.LineHeight(9), 9, Ui.Muted);
			if (!string.IsNullOrEmpty(_note)) Ui.Left(graphics, _note, left, panel.Bottom - 52, Ui.LineHeight(9), 9, Ui.Accent);
			hx = left;
			foreach ((Ui.PadButton button, string word) in Hints()) hx = Ui.HintText(graphics, button, word, hx, hintY, 18, hintSize) - 4;
			graphics.DrawStringEnd();
		}

		private (Ui.PadButton, string)[] Hints()
		{
			if (_page == Page.Pick) return new[] { (Ui.PadButton.A, "Set"), (Ui.PadButton.B, "Back") };
			if (_page == Page.Hero) return new[] { (Ui.PadButton.A, "Change"), (Ui.PadButton.B, "Back") };
			if (_page == Page.Jobs) return new[] { (Ui.PadButton.A, "Take the job"), (Ui.PadButton.B, "Back") };
			return new[] { (Ui.PadButton.A, "Select"), (Ui.PadButton.B, "Close") };
		}
	}
}
