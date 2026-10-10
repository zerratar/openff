// The menu's side of Sight (MSSMagic::stMSSMPrcMagicSelect, MSSSightro): Enter on Sight (4023) in Magic, when the caster
// can and there is something to see here, spends its MP, plays its sound (SE 0xA4 of bank 3) and opens the Sight screen
// (Ff4NaviMapSight.cs draws its map); Up / Down light another row and show it at once (SE 3), Back returns to Magic (SE 2).
// Elsewhere in Magic a spell is not cast from the menu yet.

using System;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Menu
	{
		private const int SpellSight = 4023;
		private const int SightRows = 7;

		/// <summary>Enter on a spell in Magic: Sight opens its screen; false when the spell was not taken.</summary>
		private bool CastFromMagic(int spellId)
		{
			if (spellId != SpellSight) return false;
			OpenFF.Data.SpellDefinition spell = Ff4Party.Tables?.Spell(spellId);
			Ff4NaviMap navi = Ff4NaviMap.Instance;
			if (spell == null || !spell.UsableInMenu || Member.Mp < spell.MpCost || navi == null || !navi.SightUsable())
			{
				Game.Audio.PlaySe(0, 0);
				return true;
			}
			Member.Mp = Math.Max(0, Member.Mp - Math.Min(spell.MpCost, Member.MaxMp));
			Game.Audio.PlaySe(3, 0xA4);
			navi.OpenSight();
			_screen = Screen.Sight;
			_cursor = Math.Max(0, navi.SightSelected);
			_scroll = Math.Max(0, _cursor - SightRows + 1);
			Log.Write(LogChannel.File, "menu: " + Member.Name + " casts Sight");
			return true;
		}

		private void UpdateSight(InputState input)
		{
			Ff4NaviMap navi = Ff4NaviMap.Instance;
			int n = navi?.SightCount ?? 0;
			if (input.Pressed(Pad.B))
			{
				Game.Audio.PlaySe(0, 2);
				navi?.CloseSight();
				_screen = Screen.Magic;
				_cursor = _scroll = 0;
				return;
			}
			if (n == 0) return;
			int was = _cursor;
			if (input.Pressed(Pad.Up)) _cursor = Math.Max(0, _cursor - 1);
			if (input.Pressed(Pad.Down)) _cursor = Math.Min(n - 1, _cursor + 1);
			if (_cursor < _scroll) _scroll = _cursor;
			if (_cursor >= _scroll + SightRows) _scroll = _cursor - SightRows + 1;
			if (_cursor != was || (input.Pressed(Pad.A) && navi.SightSelected < 0))
			{
				Game.Audio.PlaySe(0, 3);
				navi.SightSelect(_cursor);
			}
		}

		private void FillSight(Ff4MenuHud.Data h)
		{
			Ff4NaviMap navi = Ff4NaviMap.Instance;
			int n = navi?.SightCount ?? 0;
			for (int k = 0; k < h.SightRow.Count; k++)
			{
				int i = _scroll + k;
				h.SightRow[k].Present = i < n;
				h.SightRow[k].Name = i < n ? navi.SightText(i) : "";
				h.SightRow[k].Lit = i == _cursor;
			}
			Ff4MenuHud.Scroll(h.SightScroll, n, SightRows, _scroll, 877.5f, true);
		}
	}
}
