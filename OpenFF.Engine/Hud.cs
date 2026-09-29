// The field's HUD's data: what a mod hands the field_hud layout to bind to, by name - a quest log's
// title and next step, a mod's own counters - beside the game's own roots (hero, party, gil,
// dialogue, banner). In a mod's code:
//
//   Game.Hud.Set("quest", new { title = "The Wind Crystal", step = "Talk to the elder of Ur", active = true });
//   Game.Hud.Set("quest", null);           // gone: the frames that bind it read nothing
//
// and in the layout: <frame bind-visible="quest.active" bind-text="{quest.step}">. A value is any
// object (its public properties and fields, any case), a dictionary or a list, as the menus' bindings
// read them. The HUD reads the bindings again a few times a second, and at once when a value is set;
// a value changed inside (a field of an object already set) shows on the next of those, or at once
// after Refresh(). Menu screens reach the same names, after their own Menu.Data.

using System;
using System.Collections.Generic;

namespace OpenFF
{
	/// <summary>The field's HUD's data for its layout's bindings, by name, and whether its overlay (the layout's own frames at the top) is up.</summary>
	public sealed class HudData
	{
		private readonly Dictionary<string, object> _values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		private readonly object _sync = new object();

		/// <summary>Counts every change (Set, Remove, Refresh), for the host to read the bindings again.</summary>
		public int Version { get; private set; }

		/// <summary>Whether the overlay - the frames of the layout's own at field_hud's top (a party panel, a quest log) - is up while the field's buttons are. True unless a mod takes it down (a key that hides the HUD, a cutscene).</summary>
		public bool Overlay
		{
			get => _overlay;
			set { if (_overlay == value) return; _overlay = value; Version++; }
		}
		private bool _overlay = true;

		/// <summary>A value under a name for the bindings ("quest" for {quest.step}); null takes it away.</summary>
		public void Set(string name, object value)
		{
			if (string.IsNullOrWhiteSpace(name)) return;
			lock (_sync)
			{
				if (value == null) _values.Remove(name.Trim());
				else _values[name.Trim()] = value;
				Version++;
			}
		}

		/// <summary>The value under a name, or null.</summary>
		public object Get(string name) => TryGet(name, out object value) ? value : null;

		/// <summary>Whether a value is set under the name, and what it is.</summary>
		public bool TryGet(string name, out object value)
		{
			value = null;
			if (string.IsNullOrWhiteSpace(name)) return false;
			lock (_sync) return _values.TryGetValue(name.Trim(), out value);
		}

		/// <summary>Takes a value away; true when there was one.</summary>
		public bool Remove(string name)
		{
			if (string.IsNullOrWhiteSpace(name)) return false;
			lock (_sync)
			{
				if (!_values.Remove(name.Trim())) return false;
				Version++;
				return true;
			}
		}

		/// <summary>The names set now.</summary>
		public IReadOnlyCollection<string> Names
		{
			get { lock (_sync) return new List<string>(_values.Keys); }
		}

		/// <summary>Read the bindings again at once: a value changed inside (an object's field set since).</summary>
		public void Refresh() => Version++;
	}

	public static partial class Game
	{
		/// <summary>The field's HUD's data for its layout's bindings (Set("quest", ...) for {quest.step}), and its overlay's switch.</summary>
		public static HudData Hud { get; } = new HudData();
	}
}
