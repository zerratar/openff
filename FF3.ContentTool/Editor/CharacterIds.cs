// Which model is which, as far as a .hich row is concerned.
//
// A row names its model twice: as eight bytes of text, and as a number in front of them.
// The number is what actually matters, and until now the editor could not place a model
// unless the same map already had a row using it - because that row was the only place
// to copy the number from.
//
// It did not have to be. Read across all 356 shipped maps, the pairing is strict:
//
//   * 144 distinct models are placed, and every one of them always carries the same id
//   * no two models share an id
//   * they run from 2 to 477
//
// So the ids are global, and one pass over the maps recovers the whole table. That is
// done here rather than baked in at build time, because reading it from the workspace
// means it sees your own maps too - place a model somewhere once and every map can use
// it afterwards.
//
// For objects there is a rule as well as a table. Every one of the 66 `oNNN` models
// placed anywhere has id 400 + NNN, without exception, so an object that appears in no
// shipped map can still be given the right number.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace FF3.ContentTool.Editor
{
	internal sealed class PlaceableModel
	{
		public string Model { get; set; }
		public uint CharacterId { get; set; }

		/// <summary>"placed in the game" or "by the rule for objects".</summary>
		public string From { get; set; }

		/// <summary>How many shipped maps use it, as a rough measure of how ordinary it is.</summary>
		public int Uses { get; set; }
	}

	internal sealed class CharacterIds
	{
		private static readonly Regex Digits = new Regex(@"\d+", RegexOptions.Compiled);

		private readonly Workspace _workspace;
		private Dictionary<string, PlaceableModel> _known;

		public CharacterIds(Workspace workspace)
		{
			_workspace = workspace;
		}

		/// <summary>Called after a .hich is written, so a newly placed model counts.</summary>
		public void Invalidate()
		{
			_known = null;
		}

		/// <summary>Every model that can be placed, most used first.</summary>
		public List<PlaceableModel> All()
		{
			return Build().Values
				.OrderByDescending(m => m.Uses)
				.ThenBy(m => m.Model, StringComparer.OrdinalIgnoreCase)
				.ToList();
		}

		/// <summary>
		/// The id a row should carry for this model, or null if nothing here knows one.
		/// Saying so is better than inventing a number: a wrong id loads the wrong model,
		/// or nothing at all, and neither reports itself.
		/// </summary>
		public uint? For(string model)
		{
			if (string.IsNullOrWhiteSpace(model))
			{
				return null;
			}
			if (Build().TryGetValue(model, out PlaceableModel found))
			{
				return found.CharacterId;
			}
			return ByRule(model);
		}

		/// <summary>
		/// The object rule: oNNN carries 400 + NNN. It holds for all 66 objects placed
		/// anywhere in the game, which is what makes it safe to apply to the rest.
		/// </summary>
		private static uint? ByRule(string model)
		{
			if (!model.StartsWith("o", StringComparison.OrdinalIgnoreCase))
			{
				return null;
			}

			Match digits = Digits.Match(model);
			if (!digits.Success
				|| !int.TryParse(digits.Value, NumberStyles.Integer,
					CultureInfo.InvariantCulture, out int number)
				|| number > 99)
			{
				return null;
			}
			return (uint)(400 + number);
		}

		private Dictionary<string, PlaceableModel> Build()
		{
			if (_known != null)
			{
				return _known;
			}

			Dictionary<string, PlaceableModel> known =
				new Dictionary<string, PlaceableModel>(StringComparer.Ordinal);

			foreach (WorkspaceEntry entry in _workspace.List(".hich"))
			{
				List<HichEntry> rows;
				try
				{
					rows = Hich.Read(_workspace.Read(entry.Name));
				}
				catch (Exception)
				{
					continue;                    // one bad map should not cost the rest
				}

				foreach (HichEntry row in rows)
				{
					if (row.Kind != 0 || string.IsNullOrWhiteSpace(row.Model))
					{
						continue;
					}
					if (known.TryGetValue(row.Model, out PlaceableModel seen))
					{
						seen.Uses++;
						continue;
					}
					known[row.Model] = new PlaceableModel
					{
						Model = row.Model,
						CharacterId = row.CharacterId,
						From = "placed in the game",
						Uses = 1
					};
				}
			}

			// Objects that exist but are placed nowhere still have a number the rule can
			// work out, so they can be used too.
			foreach (WorkspaceEntry entry in _workspace.List(".lz"))
			{
				if (!entry.Name.EndsWith(".nmdp.lz", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				int slash = entry.Name.LastIndexOf('/');
				string model = entry.Name.Substring(slash + 1,
					entry.Name.Length - slash - 1 - ".nmdp.lz".Length);
				if (known.ContainsKey(model))
				{
					continue;
				}

				uint? id = ByRule(model);
				if (id.HasValue)
				{
					known[model] = new PlaceableModel
					{
						Model = model,
						CharacterId = id.Value,
						From = "by the rule for objects",
						Uses = 0
					};
				}
			}

			_known = known;
			return known;
		}
	}
}
