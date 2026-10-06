// FF4's battle cameras beyond the standing shot (btl::CBattleDisplay, BossNormalAttack, AbilityInvokeCameraController),
// read from libff4.so and checked against Steam's FF4.exe: a boss's entrance - the camera on its close-up, its name in the
// help window for 30 frames, then a cosine ease to the standing shot while the party runs in - and the close-up on a
// member invoking a command: the member stood at the stage's origin facing the camera's side, everyone else hidden (the party window and the help line stay, as Steam's frames show),
// one of three shots at random for the invoke, then a cut back.

using System;
using System.Collections.Generic;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Battle
	{
		/// <summary>Frames from the battle's start to the boss camera's move: a frame, the 5-frame fade in, the name's 30 (BossNormalAttack's sub-states 0..2).</summary>
		private const int BossNameFrom = 7, BossMoveFrom = 37;

		private BossCamera _bossCamera;

		/// <summary>partyBossParameter: the first of the group's monsters with an entrance record, or null.</summary>
		private static (int Monster, BossCamera Camera) BossOf(IEnumerable<int> monsterIds)
		{
			GameTables t = Ff4Party.Tables;
			if (t == null) return (-1, null);
			foreach (int id in monsterIds) if (t.BossCameras.TryGetValue(id, out BossCamera camera)) return (id, camera);
			return (-1, null);
		}

		/// <summary>The boss's name for its entrance: its own, or the group's (the Magus Sisters, Calcabrina, the Elemental Archfiends).</summary>
		private string BossName(int monster)
		{
			if (monster >= 172 && monster <= 174) return BattleText(0x11300, "Magus Sisters");
			if (monster == 177 || monster == 178) return BattleText(0x49C, "Calcabrina");
			if (monster == 197) return BattleText(0x1138C, "Elemental Archfiends");
			return Ff4Party.Tables.Monster(monster)?.Name ?? "";
		}

		/// <summary>The boss's entrance begun: the camera on its close-up at once, the name after the fade in.</summary>
		private void BeginBossEntrance(int monster, BossCamera camera)
		{
			_bossCamera = camera;
			SetCamera(new Vector3(camera.Position[0], camera.Position[1], camera.Position[2]), new Vector3(camera.Target[0], camera.Target[1], camera.Target[2]));
			string name = BossName(monster);
			After(BossNameFrom, () => { _help = name; _helpUntil = _clock + (BossMoveFrom - BossNameFrom); });
			Note("boss entrance: " + name + " over " + camera.Frames + " frames");
		}

		/// <summary>The frames the opening takes before the fight: a boss's entrance, or the plain opening's 30.</summary>
		private int IntroFrames => _bossCamera != null ? BossMoveFrom + _bossCamera.Frames : 30;

		/// <summary>updateBossAppearCamera: from the close-up to the standing shot, pos = stand + (start - stand) x cos(k/N x 90 deg), the snap at N.</summary>
		private void StepBossCamera()
		{
			if (_bossCamera == null || _phase != Phase.Intro || _timer < BossMoveFrom) return;
			int k = _timer - BossMoveFrom, n = Math.Max(1, _bossCamera.Frames);
			Vector3 standPos = Ff4BattleStage.CameraPosition(_cameraType), standTgt = Ff4BattleStage.CameraTarget(_cameraType);
			if (k >= n) { SetCamera(standPos, standTgt, cut: false); return; }
			float s = (float)Math.Cos(k / (double)n * Math.PI / 2);
			Vector3 startPos = new Vector3(_bossCamera.Position[0], _bossCamera.Position[1], _bossCamera.Position[2]);
			Vector3 startTgt = new Vector3(_bossCamera.Target[0], _bossCamera.Target[1], _bossCamera.Target[2]);
			SetCamera(standPos + (startPos - standPos) * s, standTgt + (startTgt - standTgt) * s, cut: false);
		}

		/// <summary>The battle camera put somewhere at once - a cut, unless it is a step of a move driven a frame at a time (<paramref name="cut"/> false), whose in-between frames are smoothed.</summary>
		private static void SetCamera(Vector3 position, Vector3 target, bool cut = true)
		{
			if (!cut)
			{
				Ff4EventCamera.Place((int)(position.X * 4096), (int)(position.Y * 4096), (int)(position.Z * 4096), (int)(target.X * 4096), (int)(target.Y * 4096), (int)(target.Z * 4096));
				return;
			}
			Ff4EventCamera.MoveTo((int)(position.X * 4096), (int)(position.Y * 4096), (int)(position.Z * 4096), 0, false);
			Ff4EventCamera.LookAt((int)(target.X * 4096), (int)(target.Y * 4096), (int)(target.Z * 4096), 0);
		}

		/// <summary>
		/// doShakeCamera: the standing shot jittered - position and target moved together by a random offset within half
		/// the amplitude each way, every frame - and put back on the last.
		/// </summary>
		private void ShakeCamera(int frames, float amplitude)
		{
			if (!Ff4BattleStage.Active) return;
			Vector3 pos = Ff4BattleStage.CameraPosition(_cameraType), tgt = Ff4BattleStage.CameraTarget(_cameraType);
			for (int k = 1; k < frames; k++)
			{
				After(k, () =>
				{
					if (_invokeShot >= 0) return;
					Vector3 d = new Vector3((float)(_random.NextDouble() - 0.5) * amplitude, (float)(_random.NextDouble() - 0.5) * amplitude, (float)(_random.NextDouble() - 0.5) * amplitude);
					SetCamera(pos + d, tgt + d);
				});
			}
			After(frames, () => { if (_invokeShot < 0) SetCamera(pos, tgt); });
		}

		// ---- the invoke close-up (setInvokeCameraForNormal, AbilityInvokeCameraController::update) ----

		private int _invokeShot = -1, _invokeFrame;
		private float _invokeHeight;
		private Fighter _invokeActor;
		private readonly List<Npc> _invokeHidden = new List<Npc>();

		/// <summary>Whether a command's invoke gets the close-up: a member's, not a counter, not Attack, Items, Summon or Auto-Potion, on the stage.</summary>
		private bool InvokeCloseUp(Fighter actor, int command) => Ff4BattleStage.Active && !actor.IsMonster && !_isCounter && command != 1 && command != 4 && command != 13 && command != 166 && actor.Npc != null;

		/// <summary>The close-up begun: the actor at the stage's origin facing +z, the others hidden, a shot of three at random.</summary>
		private Fighter _invokePartner;

		private void BeginInvokeCamera(Fighter actor, Fighter partner = null)
		{
			_invokeActor = actor;
			_invokePartner = partner;
			FrameCapture.CameraCut();   // the standing shot to the close-up: a cut
			_invokeShot = _random.Next(3);
			_invokeFrame = 0;
			int form = actor.Member != null ? PlayerForm[Math.Clamp(actor.Member.Id, 0, PlayerForm.Length - 1)] : 0;
			_invokeHeight = form == 4 || form == 7 || form == 8 ? 5f : 9f;   // the small characters' look-at 5
			actor.Npc.Teleport(Vector3.Zero);
			Face(actor, 0f);
			if (partner?.Npc != null)
			{
				// setInvokeCameraForPairMagic (Steam's frames): the two side by side, the first of the party at (-5, 0, 0).
				bool actorFirst = _party.IndexOf(actor) < _party.IndexOf(partner);
				actor.Npc.Teleport(new Vector3(actorFirst ? -5f : 5f, 0f, 0f));
				partner.Npc.Teleport(new Vector3(actorFirst ? 5f : -5f, 0f, 0f));
				Face(partner, 0f);
			}
			_invokeHidden.Clear();
			foreach (Fighter f in AllFighters())
			{
				if (f == actor || f == partner) continue;
				foreach (Npc model in ModelsOf(f)) if (model != null && !model.Hidden) { model.Hidden = true; _invokeHidden.Add(model); }
			}
			StepInvokeCamera();
		}

		/// <summary>A frame of the shot (t = k / 24 in fx12, past 1 when the invoke runs longer): 0 low and close pulling back and up, 1 a crane down, 2 an eye-level orbit to 15 degrees.</summary>
		private void StepInvokeCamera()
		{
			if (_invokeShot < 0) return;
			float t = _invokeFrame / 24f;
			const double a = 20.0 * Math.PI / 180;
			float sa = (float)Math.Sin(a), ca = (float)Math.Cos(a);
			Vector3 position = _invokeShot switch
			{
				0 => Vector3.Lerp(new Vector3(40 * sa, 4.5f, 40 * ca), new Vector3(60 * sa, 10f, 60 * ca), t),
				1 => Vector3.Lerp(new Vector3(60 * sa, 30f, 60 * ca), new Vector3(60 * sa, 4f, 60 * ca), t),
				_ => new Vector3(60f * (float)Math.Sin(t * 15.0 * Math.PI / 180), 9f, 60f * (float)Math.Cos(t * 15.0 * Math.PI / 180)),
			};
			SetCamera(position, new Vector3(0f, _invokeHeight, 0f), cut: false);
			_invokeFrame++;
		}

		/// <summary>The close-up over (AbilityInvokeBehavior::setBattleCamera): everyone shown, the actor back at its place, the cut to the standing shot.</summary>
		private void EndInvokeCamera()
		{
			if (_invokeShot < 0) return;
			_invokeShot = -1;
			foreach (Npc model in _invokeHidden) { try { model.Hidden = false; } catch (Exception) { } }
			_invokeHidden.Clear();
			if (_invokeActor?.Npc != null) { _invokeActor.Npc.Teleport(_invokeActor.Home); Face(_invokeActor, _invokeActor.Facing); }
			if (_invokePartner?.Npc != null) { _invokePartner.Npc.Teleport(_invokePartner.Home); Face(_invokePartner, _invokePartner.Facing); }
			_invokeActor = null;
			_invokePartner = null;
			SetCamera(Ff4BattleStage.CameraPosition(_cameraType), Ff4BattleStage.CameraTarget(_cameraType));
		}

		private IEnumerable<Fighter> AllFighters()
		{
			foreach (Fighter f in _party) yield return f;
			foreach (Fighter f in _foes) yield return f;
		}

		private static IEnumerable<Npc> ModelsOf(Fighter f)
		{
			yield return f.Npc;
			yield return f.FormNpc;
			yield return f.MistNpc;
			if (f.Legs != null) foreach (Npc leg in f.Legs) yield return leg;
		}
	}
}
