// FF4's battle-event models (BattleScriptEngine::createModel and the BattleEventSymbol it makes, read from libff4.so): a
// model a battle event puts on the stage by name - the friends' spirits over the last fight, a figure at a fight's end -
// shown, faded (its transparency 0..31 over a number of frames), placed, turned and scaled, given a motion set and a
// motion, and deleted again. The event keeps each one's handle in a variable.

using System;
using System.Collections.Generic;
using System.Numerics;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Battle
	{
		private sealed class EventModel
		{
			public Npc Npc;
			public bool Shown = true;
			public int Alpha = 31;     // the transparency the model has, 0..31 (CCharacterMng::setTransparency)
			public int Fade;           // counts the fades: a newer one stops an older one's steps
		}

		private readonly Dictionary<int, EventModel> _eventModels = new Dictionary<int, EventModel>();

		/// <summary>createModel: the model (with its texture, when another) on the stage, its handle the first free one.</summary>
		private int CreateEventModel(string model, string texture)
		{
			int handle = 0;
			while (_eventModels.ContainsKey(handle)) handle++;
			Npc npc = null;
			try { npc = Game.Npcs.SpawnModel(model, Vector3.Zero, 0f); } catch (Exception) { }
			if (npc == null) { Log.Write(LogChannel.File, "battle: event model " + model + " not there"); return handle; }
			npc.Solid = false;
			if (!string.IsNullOrEmpty(texture) && !string.Equals(texture, model, StringComparison.OrdinalIgnoreCase) && npc is LegacyNpc legacy) legacy.Retexture(texture);
			_eventModels[handle] = new EventModel { Npc = npc };
			Log.Write(LogChannel.File, "battle: event model " + handle + " - " + model);
			return handle;
		}

		private EventModel EventModelOf(int handle) => _eventModels.TryGetValue(handle, out EventModel m) && m.Npc != null ? m : null;

		private void DeleteEventModel(int handle)
		{
			if (!_eventModels.TryGetValue(handle, out EventModel m)) return;
			_eventModels.Remove(handle);
			try { m.Npc?.Remove(); } catch (Exception) { }
		}

		private void ClearEventModels()
		{
			foreach (int handle in new List<int>(_eventModels.Keys)) DeleteEventModel(handle);
		}

		/// <summary>setShow: drawn or not - a hidden one keeps its place and its transparency.</summary>
		private void ShowEventModel(int handle, bool shown)
		{
			EventModel m = EventModelOf(handle);
			if (m == null) return;
			m.Shown = shown;
			m.Npc.Hidden = !shown || m.Alpha <= 0;
		}

		/// <summary>setAlpha: to a transparency (0..31) at once, or over <paramref name="frames"/> from the one it has.</summary>
		private void FadeEventModel(int handle, int alpha, int frames)
		{
			EventModel m = EventModelOf(handle);
			if (m == null) return;
			alpha = Math.Clamp(alpha, 0, 31);
			int fade = ++m.Fade;
			void Apply(int a)
			{
				m.Alpha = a;
				m.Npc.Alpha = a * 100 / 31;
				m.Npc.Hidden = !m.Shown || a <= 0;
			}
			if (frames <= 0) { Apply(alpha); return; }
			int from = m.Alpha;
			for (int k = 1; k <= frames; k++)
			{
				int step = k;
				After(k, () => { if (m.Fade == fade && m.Npc != null) Apply(from + (alpha - from) * step / frames); });
			}
		}

		/// <summary>setShadowAlpha: the model's shadow to a transparency (0..31), over frames as setAlpha; hidden as 0.</summary>
		private void FadeEventShadow(int handle, int alpha, int frames)
		{
			if (EventModelOf(handle)?.Npc is not LegacyNpc npc) return;
			alpha = Math.Clamp(alpha, 0, 31);
			if (frames <= 0) { npc.ShadowAlpha(alpha); return; }
			for (int k = 1; k <= frames; k++)
			{
				int step = k;
				After(k, () => npc.ShadowAlpha(alpha * step / frames));
			}
		}

		private void PlaceEventModel(int handle, Vector3 at) => EventModelOf(handle)?.Npc.Teleport(at);

		private void TurnEventModel(int handle, float x, float y, float z)
		{
			if (EventModelOf(handle)?.Npc is LegacyNpc npc) npc.RotateExactly(x, y, z);
		}

		private void ScaleEventModel(int handle, float scale)
		{
			EventModel m = EventModelOf(handle);
			if (m != null) m.Npc.Scale = scale;
		}

		private void EventModelMotions(int handle, string set)
		{
			EventModel m = EventModelOf(handle);
			if (m == null || string.IsNullOrEmpty(set)) return;
			try { m.Npc.BindMotions(set); } catch (Exception ex) { Log.Write(LogChannel.File, "battle: event model " + handle + " motions " + set + ": " + ex.Message); }
		}

		private void EventModelMotion(int handle, int motion, bool loop, int blend)
		{
			EventModel m = EventModelOf(handle);
			if (m == null) return;
			try { m.Npc.PlayMotion(motion, loop, blend); } catch (Exception) { }
		}
	}
}
