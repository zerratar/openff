// What can go in an if or a while, and how it becomes a jump.
//
// The bytecode has no expressions. It has conditional jumps, and they come in pairs:
// flagOnJump and flagOffJump, touchOnJump and touchOffJump, and five more. So a
// condition is one of those pairs, and negating it is choosing the other half - which
// costs nothing, unlike inverting a test at runtime.
//
// The pairs are found by name rather than listed here, so an opcode table that grows
// grows the language with it.
//
// Values are the other kind: ifValueJump reads a variable and compares it, with the
// comparison as an operand - 0 >, 1 >=, 2 <, 3 <=, 4 ==, 5 != - so `value(0,3) < 4`
// is one instruction and its negation is the opposite comparison.

using System;
using System.Collections.Generic;
using System.Linq;

namespace FF3.ContentTool.Ffs
{
	/// <summary>A condition that becomes one conditional jump.</summary>
	internal sealed class ConditionForm
	{
		/// <summary>What it is called in the language: flag, touch, button.</summary>
		public string Name { get; }

		/// <summary>The opcode that jumps when the condition holds.</summary>
		public int JumpIfTrue { get; }

		/// <summary>The opcode that jumps when it does not.</summary>
		public int JumpIfFalse { get; }

		/// <summary>Arguments before the jump target.</summary>
		public int Arguments { get; }

		public ConditionForm(string name, int jumpIfTrue, int jumpIfFalse, int arguments)
		{
			Name = name;
			JumpIfTrue = jumpIfTrue;
			JumpIfFalse = jumpIfFalse;
			Arguments = arguments;
		}
	}

	internal static class Conditions
	{
		/// <summary>Comparison operator to the number ifValueJump wants.</summary>
		public static readonly Dictionary<string, int> Comparisons =
			new Dictionary<string, int>(StringComparer.Ordinal)
			{
				{ ">", 0 }, { ">=", 1 }, { "<", 2 }, { "<=", 3 }, { "==", 4 }, { "!=", 5 }
			};

		/// <summary>The opposite comparison, for negating a value test.</summary>
		public static readonly Dictionary<string, string> Opposites =
			new Dictionary<string, string>(StringComparer.Ordinal)
			{
				{ ">", "<=" }, { ">=", "<" }, { "<", ">=" },
				{ "<=", ">" }, { "==", "!=" }, { "!=", "==" }
			};

		public const string ValueName = "value";
		public const string ValueJump = "ifValueJump";

		private static Dictionary<string, ConditionForm> _forms;

		public static IReadOnlyDictionary<string, ConditionForm> Forms => Build();

		public static ConditionForm Find(string name)
		{
			return Build().TryGetValue(name, out ConditionForm form) ? form : null;
		}

		/// <summary>
		/// Every xxxOnJump with a matching xxxOffJump becomes a condition called xxx.
		/// Both halves must take the same operands, or they are not really a pair.
		/// </summary>
		private static Dictionary<string, ConditionForm> Build()
		{
			if (_forms != null)
			{
				return _forms;
			}

			Dictionary<string, int> byName = Mnemonics.All
				.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);

			Dictionary<string, ConditionForm> forms =
				new Dictionary<string, ConditionForm>(StringComparer.Ordinal);

			foreach (KeyValuePair<string, int> entry in byName)
			{
				if (!entry.Key.EndsWith("OnJump", StringComparison.Ordinal))
				{
					continue;
				}

				string stem = entry.Key.Substring(0, entry.Key.Length - "OnJump".Length);
				if (stem.Length == 0 || !byName.TryGetValue(stem + "OffJump", out int offJump))
				{
					continue;
				}

				ScriptOp onOp = ScriptOps.Get(entry.Value);
				ScriptOp offOp = ScriptOps.Get(offJump);
				int onCount = onOp.Operands?.Length ?? 0;
				if (onCount == 0 || onCount != (offOp.Operands?.Length ?? 0))
				{
					continue;
				}

				// The last operand is where it jumps to; the rest are its arguments.
				forms[stem] = new ConditionForm(stem, entry.Value, offJump, onCount - 1);
			}

			_forms = forms;
			return forms;
		}
	}
}
