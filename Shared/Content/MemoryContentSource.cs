// Content the program makes itself, served like any other source.
//
// A few tables the FF3 logic reads at start-up were compiled into FF4's executable rather
// than shipped as files. Rather than ship FF3's copy, the client synthesises them (see the
// client's MovementDefaults) and puts them at the end of the content chain under the
// names the logic asks for. Whatever the install or a mod has under those names wins.

using System;
using System.Collections.Generic;

namespace OpenFF.Content
{
	internal sealed class MemoryContentSource : IContentSource
	{
		private readonly Dictionary<string, byte[]> _files = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
		private readonly string _kind;

		public MemoryContentSource(string kind)
		{
			_kind = kind ?? "synthesised";
		}

		/// <summary>Adds (or replaces) one file, named the way the game asks: "files/x.pak".</summary>
		public void Add(string name, byte[] data)
		{
			if (!string.IsNullOrEmpty(name) && data != null)
			{
				_files[name] = data;
			}
		}

		public string Kind => _kind;

		public int Count => _files.Count;

		public IEnumerable<string> Names => _files.Keys;

		public bool TryRead(string name, out byte[] data)
		{
			if (name != null && _files.TryGetValue(name, out byte[] found))
			{
				data = (byte[])found.Clone();
				return true;
			}
			data = null;
			return false;
		}
	}
}
