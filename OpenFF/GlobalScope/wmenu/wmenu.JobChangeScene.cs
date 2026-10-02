// PORT: the game's job change scene (CWMenuJob's STATE_UPDATE_JOB_CHANGE, from its Yes on), taken out of the Job screen
// so that a mod's menu screen can show it too (Game.Party.ChangeJob with a done callback - Mastery's Jobs). The same
// steps as the game's: a fade to black and the hero on a stage of their own, the crystal's light (effect 430, the white
// curtain, sound 98) and the camera pressing in, the job taken at the flash and the new job's figure in the old one's
// place, the turn of the w_jobchange motion, a fade back to the screen. What the change does is the caller's (the game's
// own also takes the hero's gear off; Mastery's change is without penalty) - the scene only shows it.

using System;

internal static partial class GlobalScope
{
	public static partial class wmenu
	{
		public static class JobChangeScene
		{
			private enum Step { None, Start, StartFadeOut, StartFadeIn, Direction1, Direction2, JobChange, WaitTextures, Direction3, Direction4, End, EndWait, EndFadeOut, EndFadeIn }

			private static Step _step;
			private static int _partyIndex = -1, _playerId = -1, _model = -1, _moveFrame, _countFrame;
			private static Func<bool> _change;
			private static Action<bool> _done;
			private static bool _changed;
			private static readonly ds.sys3d.CCamera Camera_ = new ds.sys3d.CCamera();
			private static VecFx32 rot_ = new VecFx32(), org_ = new VecFx32(), tar_ = new VecFx32(), cur_ = new VecFx32();
			// The sub screen's planes as the mod's screen had them (its portrait, its texts): put back as they were, not as the Job screen leaves them.
			private static int _subPlanes = -1;
			// The menu's buttons (A, B, L, R) as the screen had them: out of sight for the scene, those back after.
			private static readonly bool[] _buttons = new bool[4];
			private static bool _buttonsTaken;

			/// <summary>Whether the scene is playing: the mod's screen under it hears nothing until it has ended.</summary>
			public static bool Active => _step != Step.None;

			/// <summary>
			/// The scene for the hero (by id), on a mod's menu screen: change takes the job at the flash, done hears once
			/// the scene has ended whether it did. False when there is no scene to play here (no mod screen up, the hero not
			/// in the party, one playing already) - the caller then changes the job itself.
			/// </summary>
			public static bool Start(int playerId, Func<bool> change, Action<bool> done)
			{
				if (Active || change == null || !OpenFF.Client.ModMenus.ScreenUp) return false;
				int index = -1;
				try
				{
					for (int i = 0; i < 4; i++)
						if (pl.PlayerParty.instance().player((byte)i).isEnable() && pl.PlayerParty.instance().player((byte)i).playerId() == playerId) index = i;
					if (index < 0 || wld.WorldPart.getInstance()?.getWorldSystem()?.PlayerMng() == null) return false;
					eff.CEffectMng.instance().loadEfp("/EFFECT/e430.efp");
				}
				catch (Exception ex) { OpenFF.Client.Log.Write(OpenFF.Client.LogChannel.General, "menus: no job change scene: " + ex.Message); return false; }
				_partyIndex = index;
				_playerId = playerId;
				_change = change;
				_done = done;
				_changed = false;
				_model = -1;
				_step = Step.Start;
				menu.MenuManager.getSingleton().inputPermission(b: false);
				menu.MenuManager.getSingleton().playSEDecide();
				return true;
			}

			/// <summary>One step of the scene, from the mod screen's run in place of its own.</summary>
			public static void Run()
			{
				try { Advance(); }
				catch (Exception ex)
				{
					OpenFF.Client.Log.Write(OpenFF.Client.LogChannel.General, "menus: job change scene: " + ex.GetType().Name + ": " + ex.Message);
					Finish(restore: true);
					return;
				}
				if (_model != -1)
				{
					rot_.y = (rot_.y + 72) & 0xFFFF;
					Players().Player(_model).setRotation(rot_);
				}
				if (Active) Camera_.execute();
			}

			/// <summary>The screen is going away under the scene: everything put back, done told at once.</summary>
			public static void Abort()
			{
				if (Active) Finish(restore: true);
			}

			private static pl.CPlayerManager Players() => wld.WorldPart.getInstance().getWorldSystem().PlayerMng();

			private static pl.Player Hero() => pl.PlayerParty.instance().player((byte)_partyIndex);

			/// <summary>The hero's figure for the job held now, on the scene's stage, at its first frame of the motion.</summary>
			private static void Figure(int motion, bool loop)
			{
				string name = "";
				sprintf(out name, "j%d%02d", OpenFF.Client.ModCharactersLayer.ModelSet(Hero().playerId()) + 1, OpenFF.Client.ModCharactersLayer.ModelJob(Hero().playerId(), Hero().jobManager().nowJob()) + 1);
				VecFx32 position = new VecFx32(JOB_MODEL_X, JOB_MODEL_Y, JOB_MODEL_Z);
				VecFx32 rotation = new VecFx32(0, 61532, 0);
				rot_.copy(rotation);
				_model = Players().setUpPlayerHuman(name, _AutoPilot: false, _Operater: false);
				Players().PlayerHuman(_model).addMotion("w_jobchange");
				Players().Player(_model).setScale(new VecFx32(4096, 4096, 4096));
				Players().Player(_model).setRotation(rotation);
				Players().Player(_model).setPosition(position);
				Players().Player(_model).startMotion(motion, _Loop: loop, 5u);
			}

			private static void Advance()
			{
				switch (_step)
				{
				case Step.Start:
					dgs.CFade.Main().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
					_step = Step.StartFadeOut;
					break;
				case Step.StartFadeOut:
					if (!dgs.CFade.Main().isFaded()) break;
					dgs.CFade.Main().fadeIn(15);
					// The screen's own out of sight: the game's planes, its texts, the hand, and the mod screen's panels.
					OpenFF.Client.ModMenus.SceneCovering = true;
					_subPlanes = GXS_GetVisiblePlane();
					ds.CVram.setSubPlaneVisiblity(bg0: false, bg1: false, bg2: false, bg3: false, obj: false);
					Buttons(take: true);
					// The screen's texts, as the Job screen erases its own (the screen the scene ends on writes its again).
					dgs.msg.CMessageSys.getInstance().Sub().dgsMMAreaErase(0, 0, 480, 320);
					menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: false);
					GX_Power3D(1);
					stageMng.setHidden(flag: true);
					{
						VecFx32 eye = new VecFx32(CAMERA_POS_OFFSET_X, CAMERA_POS_OFFSET_Y, CAMERA_POS_OFFSET_Z);
						eye.y += JOB_MODEL_Y;
						VecFx32 at = new VecFx32(CAMERA_TAR_X, CAMERA_TAR_Y, CAMERA_TAR_Z);
						at.x += eye.x;
						at.y += eye.y;
						at.z += eye.z;
						Camera_.initialize();
						Camera_.setMoveMode(1);
						Camera_.setPosition(eye);
						Camera_.setTarget(at);
						Camera_.setAngle(0, 32768, 0);
						Camera_.setCamUp(0, 4096, 0);
						Camera_.setDistance(65536);
						Camera_.setClip(40960, 2048000);
						Camera_.setFOV(1060, 3956);
					}
					wld.WorldPart.getInstance().getWorldSystem().WorldCamera().setActivity(b: false);
					Figure(1001, loop: true);
					_step = Step.StartFadeIn;
					break;
				case Step.StartFadeIn:
					if (dgs.CFade.Main().isCleared()) _step = Step.Direction1;
					break;
				case Step.Direction1:
				{
					dgs.CFade.Sub().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_WHITE);
					dgs.CCurtain.Top().setEnable(enable: true);
					dgs.CCurtain.Top().setVisible(visible: true);
					dgs.CCurtain.Top().setColor(0, GX_RGB(31, 31, 31));
					dgs.CCurtain.Top().setAlpha(0, 0);
					dgs.CCurtain.Top().setAlpha(15, 31);
					int light = eff.CEffectMng.instance().create(430, 1);
					if (light != -1) eff.CEffectMng.instance().setPosition(light, new VecFx32(JOB_MODEL_X, JOB_MODEL_Y, JOB_MODEL_Z));
					Players().Player(_model).startMotion(10354, _Loop: true, 5u);
					org_.copy(Camera_.getPosition());
					tar_.x = org_.x;
					tar_.y = org_.y + 20480;
					tar_.z = org_.z + 163840;
					_moveFrame = 15;
					_countFrame = 0;
					MatrixSound.MtxSENDS_Play(98, 0, 192, 127);
					_step = Step.Direction2;
					break;
				}
				case Step.Direction2:
					if (++_countFrame > _moveFrame) { Camera_.setPosition(tar_); _step = Step.JobChange; break; }
					cur_.x = org_.x + (tar_.x - org_.x) / _moveFrame * _countFrame;
					cur_.y = org_.y + (tar_.y - org_.y) / _moveFrame * _countFrame;
					cur_.z = org_.z + (tar_.z - org_.z) / _moveFrame * _countFrame;
					Camera_.setPosition(cur_);
					break;
				case Step.JobChange:
					if (!dgs.CCurtain.Top().isChangedAlpha()) break;
					// The flash: the job taken, and the new job's figure in the old one's place, part way into the motion.
					try { _changed = _change(); }
					catch (Exception ex) { OpenFF.Client.Log.Write(OpenFF.Client.LogChannel.General, "menus: job change: " + ex.Message); _changed = false; }
					if (_changed)
					{
						try { CWMenuManager.Instance().GetPcFace().pcfmSetJob((uint)_playerId, (uint)Hero().jobManager().nowJob()); } catch (Exception) { }
					}
					Players().Player(_model).terminate();
					Figure(10354, loop: true);
					Players().Player(_model).setCurrentFrame(15u);
					Players().Player(_model).setMotionLoop(loop: false);
					_step = Step.WaitTextures;
					break;
				case Step.WaitTextures:
					if (!TexDivideLoader.getSingleton().tdlIsEmpty()) break;
					dgs.CFade.Sub().fadeIn(15);
					dgs.CCurtain.Top().setAlpha(15, 0);
					tar_.copy(org_);
					org_.copy(Camera_.getPosition());
					cur_.copy(Camera_.getPosition());
					_moveFrame = 15;
					_countFrame = 0;
					_step = Step.Direction3;
					break;
				case Step.Direction3:
					if (++_countFrame <= _moveFrame)
					{
						cur_.x = org_.x + (tar_.x - org_.x) / _moveFrame * _countFrame;
						cur_.y = org_.y + (tar_.y - org_.y) / _moveFrame * _countFrame;
						cur_.z = org_.z + (tar_.z - org_.z) / _moveFrame * _countFrame;
						Camera_.setPosition(cur_);
					}
					else if (Players().Player(_model).isEndOfMotion())
					{
						Players().Player(_model).startMotion(1001, _Loop: true, 5u);
						_step = Step.Direction4;
					}
					break;
				case Step.Direction4:
					if (!dgs.CCurtain.Top().isChangedAlpha()) break;
					dgs.CCurtain.Top().setEnable(enable: false);
					dgs.CCurtain.Top().setVisible(visible: false);
					_step = Step.End;
					break;
				case Step.End:
					_moveFrame = 15;
					_countFrame = 0;
					_step = Step.EndWait;
					break;
				case Step.EndWait:
					if (++_countFrame > _moveFrame)
					{
						dgs.CFade.Main().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
						_step = Step.EndFadeOut;
					}
					break;
				case Step.EndFadeOut:
					if (!dgs.CFade.Main().isFaded()) break;
					dgs.CFade.Main().fadeIn(15);
					Restore();
					_step = Step.EndFadeIn;
					break;
				case Step.EndFadeIn:
					if (dgs.CFade.Main().isCleared()) Finish(restore: false);
					break;
				}
			}

			/// <summary>The stage put away and the world's camera and the screen's planes back, as the game's Job screen leaves them.</summary>
			private static void Restore()
			{
				GX_Power3D(0);
				if (_subPlanes >= 0) GXS_SetVisiblePlane(_subPlanes); else ds.CVram.setSubPlaneVisiblity(bg0: false, bg1: false, bg2: false, bg3: true, obj: true);
				_subPlanes = -1;
				Buttons(take: false);
				menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: true);
				stageMng.setHidden(flag: false);
				wld.WorldPart.getInstance().getScene().setCamera(wld.WorldPart.getInstance().getWorldSystem().WorldCamera());
				wld.WorldPart.getInstance().getWorldSystem().WorldCamera().setActivity(b: true);
				if (_model != -1) { Players().Player(_model).terminate(); _model = -1; }
				OpenFF.Client.ModMenus.SceneCovering = false;
			}

			/// <summary>The menu's buttons hidden for the scene (the field HUD's look of them follows), or the ones that were shown shown again - their words with them.</summary>
			private static void Buttons(bool take)
			{
				try
				{
					CWMenuButton b = CWMenuManager.Instance().GetMenuButton();
					if (take)
					{
						for (int i = 0; i < 4; i++) _buttons[i] = b.IsButtonShown(i);
						_buttonsTaken = true;
						b.SetButtonAActivity(b: false); b.SetButtonBActivity(b: false); b.SetButtonLActivity(b: false); b.SetButtonRActivity(b: false);
					}
					else if (_buttonsTaken)
					{
						_buttonsTaken = false;
						if (_buttons[0]) b.SetButtonAActivity(b: true);
						if (_buttons[1]) b.SetButtonBActivity(b: true);
						if (_buttons[2]) b.SetButtonLActivity(b: true);
						if (_buttons[3]) b.SetButtonRActivity(b: true);
					}
				}
				catch (Exception ex) { OpenFF.Client.Log.Write(OpenFF.Client.LogChannel.General, "menus: job change scene buttons: " + ex.Message); }
			}

			private static void Finish(bool restore)
			{
				if (restore)
				{
					try
					{
						dgs.CCurtain.Top().setEnable(enable: false);
						dgs.CCurtain.Top().setVisible(visible: false);
						if (!dgs.CFade.Main().isCleared()) dgs.CFade.Main().fadeIn(1);
						if (_step >= Step.StartFadeIn && _step <= Step.EndFadeOut) Restore();
					}
					catch (Exception) { }
					OpenFF.Client.ModMenus.SceneCovering = false;
				}
				try { eff.CEffectMng.instance().unLoadEfp2(); } catch (Exception) { }
				_step = Step.None;
				_change = null;
				Action<bool> done = _done;
				_done = null;
				menu.MenuManager.getSingleton().inputPermission(b: true);
				bool changed = _changed;
				OpenFF.Client.Log.Write(OpenFF.Client.LogChannel.General, "menus: job change scene ended - " + (changed ? "the job taken" : "no change"));
				try { done?.Invoke(changed); }
				catch (Exception ex) { OpenFF.Client.Log.Write(OpenFF.Client.LogChannel.General, "menus: job change done: " + ex.Message); }
			}
		}
	}
}
