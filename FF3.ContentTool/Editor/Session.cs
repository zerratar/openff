// One game open in the editor: a workspace and everything derived from it.
//
// The editor used to hold one workspace and the five indexes built over it as loose
// fields, and opening a project swapped the lot. A project can target two games, and a
// modder wants both on screen - the FF3 script beside the FF4 one - so the lot is now a
// Session, the server keeps one per target, and each request says which it means.

using System;

namespace FF3.ContentTool.Editor
{
	internal sealed class Session
	{
		/// <summary>The project target this is for: ours, steam, ff4steam - or "content" when opened by path.</summary>
		public string Target { get; }

		public Workspace Workspace { get; }
		public MessageIndex Messages { get; }
		public CharacterIds CharacterIds { get; }
		public FlagIndex Flags { get; }
		public References References { get; }
		public Fonts Fonts { get; }

		/// <summary>Message id to text, bound to this session's index.</summary>
		public Func<uint, string> LookupMessage { get; }

		public Session(string target, Workspace workspace, MessageIndex messages, string language)
		{
			Target = target;
			Workspace = workspace;
			Messages = messages ?? new MessageIndex(workspace, language);
			LookupMessage = id => Messages.Text(id);
			CharacterIds = new CharacterIds(workspace);
			Flags = new FlagIndex(workspace, LookupMessage);
			References = new References(workspace, LookupMessage);
			Fonts = new Fonts(workspace.ContentDirectory);
		}

		/// <summary>"ff3" or "ff4".</summary>
		public string Game => Workspace.Game;

		/// <summary>What to show a person for this session: "FF3 on Steam".</summary>
		public string Label => Target == "content" ? Workspace.Game.ToUpperInvariant() : Targets.Describe(Target);
	}
}
