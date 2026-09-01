// XNA GamerServices compatibility layer.
//
// MonoGame dropped Microsoft.Xna.Framework.GamerServices (it was Xbox LIVE /
// Windows Phone only). The decompiled game uses it for three things:
//
//   * Guide.BeginShowKeyboardInput / BeginShowMessageBox  - the system text entry
//     and yes/no prompts. MonoGame ships direct replacements for both in
//     Microsoft.Xna.Framework.Input (KeyboardInput / MessageBox), so these forward.
//   * Guide.IsTrialMode / ShowMarketplace                 - store integration; there
//     is no store here, so the game is always the full version.
//   * SignedInGamer achievements                          - forwarded to a local
//     achievement store on disk so the in-game achievement list still works.
//
// Everything lives in the original namespace so the decompiled sources compile
// against it unmodified.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using MgInput = Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework.GamerServices
{
	/// <summary>Minimal stand-in for XNA's asynchronous result objects.</summary>
	internal sealed class GuideAsyncResult<T> : IAsyncResult
	{
		private readonly ManualResetEvent _handle = new ManualResetEvent(initialState: false);

		public T Result { get; private set; }

		public object AsyncState { get; }

		public WaitHandle AsyncWaitHandle => _handle;

		public bool CompletedSynchronously => false;

		public bool IsCompleted { get; private set; }

		public GuideAsyncResult(object asyncState)
		{
			AsyncState = asyncState;
		}

		public void Complete(T result)
		{
			Result = result;
			IsCompleted = true;
			_handle.Set();
		}
	}

	/// <summary>
	/// XNA threw this out of Game.Update when the title had to be updated from the
	/// marketplace before it could keep running. Nothing raises it here; it exists so
	/// the catch block in Game1.Update still compiles.
	/// </summary>
	public class GameUpdateRequiredException : Exception
	{
		public GameUpdateRequiredException()
		{
		}

		public GameUpdateRequiredException(string message)
			: base(message)
		{
		}
	}

	public enum MessageBoxIcon
	{
		None = 0,
		Error = 1,
		Warning = 2,
		Alert = 3
	}

	public static class Guide
	{
		/// <summary>
		/// True while a prompt owns input. The game polls this every frame and pauses
		/// itself when it is set.
		///
		/// Deliberately does NOT consult MonoGame's KeyboardInput.IsVisible: on DesktopGL
		/// KeyboardInput.PlatformShow throws NotImplementedException, and Show() sets
		/// IsVisible before awaiting it, so a single call leaves that flag stuck true for
		/// the rest of the process - which pauses the game forever.
		/// </summary>
		/// Text entry deliberately does NOT set this. Game1.Draw clears the screen to
		/// black whenever the guide is up (that was correct when the phone's keyboard
		/// covered the screen); here you type into a field over the live scene instead,
		/// and FF3.DesktopInput suppresses game input for the duration.
		public static bool IsVisible => MgInput.MessageBox.IsVisible;

		/// <summary>There is no trial build of the desktop port.</summary>
		public static bool IsTrialMode => false;

		public static bool IsScreenSaverEnabled { get; set; }

		public static IAsyncResult BeginShowKeyboardInput(PlayerIndex player, string title, string description,
			string defaultText, AsyncCallback callback, object state)
		{
			return BeginShowKeyboardInput(player, title, description, defaultText, callback, state, usePasswordMode: false);
		}

		public static IAsyncResult BeginShowKeyboardInput(PlayerIndex player, string title, string description,
			string defaultText, AsyncCallback callback, object state, bool usePasswordMode)
		{
			GuideAsyncResult<string> result = new GuideAsyncResult<string>(state);
			FF3.TextEntry entry = FF3.TextEntry.Instance;
			if (entry == null)
			{
				// No component attached (shouldn't happen); cancel rather than hang.
				result.Complete(null);
				callback?.Invoke(result);
				return result;
			}
			entry.Show(title, description, defaultText, 6, delegate(string entered)
			{
				result.Complete(entered);
				callback?.Invoke(result);
			});
			return result;
		}

		/// <summary>Returns the entered text, or null if the user cancelled.</summary>
		public static string EndShowKeyboardInput(IAsyncResult result)
		{
			return ((GuideAsyncResult<string>)result).Result;
		}

		public static IAsyncResult BeginShowMessageBox(PlayerIndex player, string title, string text,
			IEnumerable<string> buttons, int focusButton, MessageBoxIcon icon, AsyncCallback callback, object state)
		{
			GuideAsyncResult<int?> result = new GuideAsyncResult<int?>(state);
			MgInput.MessageBox.Show(title, text, buttons)
				.ContinueWith(delegate(System.Threading.Tasks.Task<int?> task)
				{
					result.Complete(task.Status == System.Threading.Tasks.TaskStatus.RanToCompletion ? task.Result : null);
					callback?.Invoke(result);
				});
			return result;
		}

		public static IAsyncResult BeginShowMessageBox(string title, string text, IEnumerable<string> buttons,
			int focusButton, MessageBoxIcon icon, AsyncCallback callback, object state)
		{
			return BeginShowMessageBox(PlayerIndex.One, title, text, buttons, focusButton, icon, callback, state);
		}

		/// <summary>Index of the chosen button, or null if the box was dismissed.</summary>
		public static int? EndShowMessageBox(IAsyncResult result)
		{
			return ((GuideAsyncResult<int?>)result).Result;
		}

		/// <summary>No marketplace on desktop; the game treats a failure here as "not purchased yet".</summary>
		public static void ShowMarketplace(PlayerIndex player)
		{
		}

		public static void ShowSignIn(int paneCount, bool onlineOnly)
		{
		}
	}

	/// <summary>
	/// Present only so <c>Game.Components.Add(new GamerServicesComponent(game))</c>
	/// in the original Game1 constructor still resolves. It does nothing.
	/// </summary>
	public class GamerServicesComponent : GameComponent
	{
		public GamerServicesComponent(Game game)
			: base(game)
		{
		}
	}

	public sealed class Achievement
	{
		public string Key { get; internal set; }

		public string Name { get; internal set; }

		public string Description { get; internal set; }

		public bool IsEarned { get; internal set; }

		public DateTime EarnedDateTime { get; internal set; }

		public int GamerScore { get; internal set; }
	}

	public sealed class AchievementCollection : IEnumerable<Achievement>
	{
		private readonly List<Achievement> _items;

		internal AchievementCollection(List<Achievement> items)
		{
			_items = items;
		}

		public int Count => _items.Count;

		public Achievement this[int index] => _items[index];

		public IEnumerator<Achievement> GetEnumerator() => _items.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
	}

	public class SignedInEventArgs : EventArgs
	{
		public SignedInGamer Gamer { get; }

		public SignedInEventArgs(SignedInGamer gamer)
		{
			Gamer = gamer;
		}
	}

	/// <summary>
	/// A local, always-signed-in "gamer". Awarded achievements are persisted next to
	/// the save data so the achievement menu survives a restart.
	/// </summary>
	public class SignedInGamer
	{
		private static readonly object _sync = new object();

		private readonly HashSet<string> _earned = new HashSet<string>(StringComparer.Ordinal);

		private string _storePath;

		public string Gamertag { get; internal set; } = Environment.UserName;

		public PlayerIndex PlayerIndex { get; internal set; } = PlayerIndex.One;

		public bool IsSignedInToLive => false;

		/// <summary>Raised once at start-up so the game kicks off its achievement query.</summary>
		public static event EventHandler<SignedInEventArgs> SignedIn;

		public static event EventHandler<SignedOutEventArgs> SignedOut;

		internal SignedInGamer()
		{
			try
			{
				string dir = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FF3");
				Directory.CreateDirectory(dir);
				_storePath = Path.Combine(dir, "achievements.txt");
				if (File.Exists(_storePath))
				{
					foreach (string line in File.ReadAllLines(_storePath))
					{
						string key = line.Trim();
						if (key.Length != 0)
						{
							_earned.Add(key);
						}
					}
				}
			}
			catch (Exception)
			{
				_storePath = null;
			}
		}

		internal static void RaiseSignedIn(SignedInGamer gamer)
		{
			SignedIn?.Invoke(null, new SignedInEventArgs(gamer));
		}

		public IAsyncResult BeginAwardAchievement(string achievementKey, AsyncCallback callback, object state)
		{
			lock (_sync)
			{
				if (_earned.Add(achievementKey))
				{
					Save();
				}
			}
			GuideAsyncResult<bool> result = new GuideAsyncResult<bool>(state);
			result.Complete(result: true);
			callback?.Invoke(result);
			return result;
		}

		public void EndAwardAchievement(IAsyncResult result)
		{
		}

		public IAsyncResult BeginGetAchievements(AsyncCallback callback, object state)
		{
			GuideAsyncResult<bool> result = new GuideAsyncResult<bool>(state);
			result.Complete(result: true);
			callback?.Invoke(result);
			return result;
		}

		public AchievementCollection EndGetAchievements(IAsyncResult result)
		{
			List<Achievement> list = new List<Achievement>();
			lock (_sync)
			{
				foreach (string key in _earned)
				{
					list.Add(new Achievement { Key = key, Name = key, IsEarned = true });
				}
			}
			return new AchievementCollection(list);
		}

		private void Save()
		{
			if (_storePath == null)
			{
				return;
			}
			try
			{
				File.WriteAllLines(_storePath, new List<string>(_earned));
			}
			catch (Exception)
			{
			}
		}
	}

	public class SignedOutEventArgs : EventArgs
	{
		public SignedInGamer Gamer { get; }

		public SignedOutEventArgs(SignedInGamer gamer)
		{
			Gamer = gamer;
		}
	}

	public sealed class SignedInGamerCollection
	{
		private readonly SignedInGamer _local = new SignedInGamer();

		public int Count => 1;

		public SignedInGamer this[PlayerIndex index] => index == PlayerIndex.One ? _local : null;

		public SignedInGamer this[int index] => index == 0 ? _local : null;
	}

	public static class Gamer
	{
		public static SignedInGamerCollection SignedInGamers { get; } = new SignedInGamerCollection();

		/// <summary>
		/// Called once from Program.Main. XNA raised SignedIn asynchronously after the
		/// Guide came up; here we just fire it as soon as the game has subscribed.
		/// </summary>
		internal static void SignalLocalSignIn()
		{
			SignedInGamer gamer = SignedInGamers[PlayerIndex.One];
			if (gamer != null)
			{
				SignedInGamer.RaiseSignedIn(gamer);
			}
		}
	}
}
