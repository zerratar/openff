// Message id -> text, read through the workspace.
//
// It matters that this goes through the workspace rather than a folder of extracted
// files: a line added or changed in the editor lives in the override, and if the
// lookup could not see overrides then new dialogue would show up as a bare id in the
// very views that were used to write it.
//
// Built on first use and dropped whenever a .msd is written, which is cheap - one
// language is about 200 files.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace FF3.ContentTool.Editor
{
	internal sealed class MessageIndex
	{
		private readonly Workspace _workspace;
		private readonly string _language;
		private Dictionary<uint, string> _messages;

		public MessageIndex(Workspace workspace, string language)
		{
			_workspace = workspace;
			_language = (language ?? "en") + ".lproj/";
		}

		public int Count => Build().Count;

		public string Text(uint id)
		{
			return Build().TryGetValue(id, out string text) ? text : null;
		}

		/// <summary>Called after writing a .msd, so the next lookup sees the change.</summary>
		public void Invalidate()
		{
			_messages = null;
		}

		/// <summary>
		/// The highest id in the files for one map, so a new line can be given one
		/// past it. Ids are matched by a linear scan at runtime, so anything unused
		/// works; staying just above the map's own range keeps it recognisable.
		/// </summary>
		public uint NextIdFor(string map)
		{
			uint highest = 0;
			foreach (WorkspaceEntry entry in _workspace.List(".msd"))
			{
				if (!entry.Name.EndsWith("/" + map + ".msd", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				try
				{
					foreach (MsdMessage message in Msd.Read(_workspace.Read(entry.Name)).Messages)
					{
						highest = Math.Max(highest, message.Id);
					}
				}
				catch (Exception)
				{
					// A file that will not decode cannot contribute an id.
				}
			}
			return highest + 1;
		}

		private Dictionary<uint, string> Build()
		{
			if (_messages != null)
			{
				return _messages;
			}

			Dictionary<uint, string> messages = new Dictionary<uint, string>();
			foreach (WorkspaceEntry entry in _workspace.List(".msd"))
			{
				if (!entry.Name.StartsWith(_language, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				try
				{
					foreach (MsdMessage message in Msd.Read(_workspace.Read(entry.Name)).Messages)
					{
						if (message.Pages.Count > 0 && !messages.ContainsKey(message.Id))
						{
							messages[message.Id] = message.Pages[0];
						}
					}
				}
				catch (Exception)
				{
					// One unreadable file should not cost every other line.
				}
			}

			_messages = messages;
			return messages;
		}
	}
}
