// Structured statements down to labels and jumps.
//
// This is where if, while, for and functions stop existing. Everything below this
// point is the flat stream the machine runs, which means code generation never had to
// learn about control flow: an if is a conditional jump and a generated label, and
// that is all it ever was.
//
// Conditions are inverted by picking the other half of the opcode pair rather than by
// testing and negating - flagOffJump instead of flagOnJump - so an if costs exactly
// one instruction more than the body it guards.
//
// Generated labels are named after what made them (if_3_else, while_7_top) so a
// compile error inside a lowered construct still points somewhere a person can find.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FF3.ContentTool.Ffs
{
	internal sealed class Lowering
	{
		private readonly List<Diagnostic> _problems;
		private readonly Dictionary<string, (long Library, long Id)> _callable;
		private readonly List<Statement> _flat = new List<Statement>();
		private readonly Stack<(string Break, string Continue)> _loops =
			new Stack<(string, string)>();

		private int _counter;

		private Lowering(List<Diagnostic> problems,
			Dictionary<string, (long Library, long Id)> callable)
		{
			_problems = problems;
			_callable = callable;
		}

		/// <summary>
		/// Flattens the document in place: every structured statement becomes labels
		/// and instructions, and every function or cast written as a block gets its
		/// code appended with a label the declaration then points at.
		/// </summary>
		public static void Flatten(ScriptDocument document, List<Diagnostic> problems)
		{
			Dictionary<string, (long, long)> callable =
				new Dictionary<string, (long, long)>(StringComparer.Ordinal);

			// Externs name something that already exists; functions with a body get an
			// id of their own. Both are callable by name from anywhere in the file.
			foreach (ExternDeclaration external in document.Externs)
			{
				callable[external.Name] = (external.Library, external.Id);
			}
			foreach (FunctionDeclaration function in document.Functions)
			{
				if (function.Name == null)
				{
					continue;
				}
				if (!function.HasId)
				{
					function.Id = IdFor(function.Name);
					function.HasId = true;
				}
				callable[function.Name] = (0, function.Id);
			}

			Lowering lowering = new Lowering(problems, callable);

			foreach (Statement statement in document.Code)
			{
				lowering.Emit(statement);
			}

			// Blocks last, so a cast written as a block does not sit in the middle of
			// the code that was already flat.
			foreach (CastDeclaration cast in document.Casts)
			{
				string name = "cast" + cast.Number.ToString(CultureInfo.InvariantCulture);
				lowering.Place(cast.Init, name + "_init");
				lowering.Place(cast.Main, name + "_main");
				lowering.Place(cast.Exit, name + "_exit");
			}
			foreach (FunctionDeclaration function in document.Functions)
			{
				lowering.Place(function.Target,
					function.Name ?? ("func_" + function.Id.ToString(CultureInfo.InvariantCulture)));
			}

			document.Code = lowering._flat;
		}

		/// <summary>
		/// A stable id for a named function: FNV-1a over the name. The game looks these
		/// up by exact match in the file's own table, so any unused number works - but
		/// deriving it from the name keeps it the same between builds, which matters
		/// the moment two scripts are compiled from the same source.
		/// </summary>
		public static uint IdFor(string name)
		{
			uint hash = 2166136261;
			foreach (char c in name)
			{
				hash = (hash ^ c) * 16777619;
			}
			return hash;
		}

		private void Place(EntryTarget target, string label)
		{
			if (target?.Body == null)
			{
				return;
			}
			_flat.Add(new LabelItem { Name = label, Token = target.Body.Token });
			Emit(target.Body);

			// A cast body that runs off its end would carry on into whatever was laid
			// out next, so it is closed off.
			if (!EndsFlow(target.Body))
			{
				_flat.Add(new InstructionItem
				{
					Mnemonic = "end",
					Token = target.Body.Token
				});
			}
			target.Label = label;
			target.Body = null;
		}

		/// <summary>Whether a block already stops, so no end has to be added.</summary>
		private static bool EndsFlow(BlockStatement block)
		{
			Statement last = block.Statements.LastOrDefault();
			return last is InstructionItem instruction
				&& (instruction.Mnemonic == "end" || instruction.Mnemonic == "return"
					|| instruction.Mnemonic == "jump");
		}

		private string Fresh(string what)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}_{1}", what, ++_counter);
		}

		private void Emit(Statement statement)
		{
			switch (statement)
			{
				case BlockStatement block:
					foreach (Statement inner in block.Statements)
					{
						Emit(inner);
					}
					return;

				case LabelItem label:
					_flat.Add(label);
					return;

				case DataItem data:
					_flat.Add(data);
					return;

				case InstructionItem instruction:
					EmitCallOrInstruction(instruction);
					return;

				case GotoStatement go:
					Jump(go.Label, go.Token);
					return;

				case LoopJump loop:
					EmitLoopJump(loop);
					return;

				case IfStatement branch:
					EmitIf(branch);
					return;

				case WhileStatement loop:
					EmitWhile(loop);
					return;

				case DoWhileStatement loop:
					EmitDoWhile(loop);
					return;

				case ForStatement loop:
					EmitFor(loop);
					return;
			}
		}

		/// <summary>
		/// A name with no matching instruction may still be a function this file
		/// declares, in which case it becomes a call.
		/// </summary>
		private void EmitCallOrInstruction(InstructionItem instruction)
		{
			if (instruction.Opcode < 0
				&& Mnemonics.Opcode(instruction.Mnemonic) < 0
				&& _callable.TryGetValue(instruction.Mnemonic,
					out (long Library, long Id) target))
			{
				if (instruction.Arguments.Count > 0)
				{
					_problems.Add(new Diagnostic(
						instruction.Mnemonic + " takes no arguments - the bytecode has "
						+ "no way to pass any", instruction.Token));
				}

				_flat.Add(new InstructionItem
				{
					Mnemonic = "call",
					Token = instruction.Token,
					Arguments =
					{
						Argument.FromNumber(target.Library, instruction.Token),
						Argument.FromNumber(target.Id, instruction.Token)
					}
				});
				return;
			}

			_flat.Add(instruction);
		}

		private void EmitLoopJump(LoopJump loop)
		{
			if (_loops.Count == 0)
			{
				_problems.Add(new Diagnostic(
					(loop.IsBreak ? "break" : "continue") + " is only meaningful inside a loop",
					loop.Token));
				return;
			}
			(string Break, string Continue) current = _loops.Peek();
			Jump(loop.IsBreak ? current.Break : current.Continue, loop.Token);
		}

		private void EmitIf(IfStatement branch)
		{
			string elseLabel = Fresh("if_else");
			string endLabel = branch.Else == null ? elseLabel : Fresh("if_end");

			// Jump past the body when the condition does not hold.
			EmitConditionalJump(branch.Condition, elseLabel, jumpWhenTrue: false);
			Emit(branch.Then);

			if (branch.Else != null)
			{
				Jump(endLabel, branch.Token);
				_flat.Add(new LabelItem { Name = elseLabel, Token = branch.Token });
				Emit(branch.Else);
			}

			_flat.Add(new LabelItem { Name = endLabel, Token = branch.Token });
		}

		private void EmitWhile(WhileStatement loop)
		{
			string top = Fresh("while_top");
			string end = Fresh("while_end");

			_flat.Add(new LabelItem { Name = top, Token = loop.Token });
			EmitConditionalJump(loop.Condition, end, jumpWhenTrue: false);

			_loops.Push((end, top));
			Emit(loop.Body);
			_loops.Pop();

			Jump(top, loop.Token);
			_flat.Add(new LabelItem { Name = end, Token = loop.Token });
		}

		private void EmitDoWhile(DoWhileStatement loop)
		{
			string top = Fresh("do_top");
			string again = Fresh("do_again");
			string end = Fresh("do_end");

			_flat.Add(new LabelItem { Name = top, Token = loop.Token });

			_loops.Push((end, again));
			Emit(loop.Body);
			_loops.Pop();

			_flat.Add(new LabelItem { Name = again, Token = loop.Token });
			EmitConditionalJump(loop.Condition, top, jumpWhenTrue: true);
			_flat.Add(new LabelItem { Name = end, Token = loop.Token });
		}

		private void EmitFor(ForStatement loop)
		{
			string top = Fresh("for_top");
			string step = Fresh("for_step");
			string end = Fresh("for_end");

			if (loop.Initialiser != null)
			{
				Emit(loop.Initialiser);
			}

			_flat.Add(new LabelItem { Name = top, Token = loop.Token });
			if (loop.Condition != null)
			{
				EmitConditionalJump(loop.Condition, end, jumpWhenTrue: false);
			}

			// continue goes to the step, not the test, so the counter still moves.
			_loops.Push((end, step));
			Emit(loop.Body);
			_loops.Pop();

			_flat.Add(new LabelItem { Name = step, Token = loop.Token });
			if (loop.Step != null)
			{
				Emit(loop.Step);
			}
			Jump(top, loop.Token);
			_flat.Add(new LabelItem { Name = end, Token = loop.Token });
		}

		private void Jump(string label, Token token)
		{
			_flat.Add(new InstructionItem
			{
				Mnemonic = "jump",
				Token = token,
				Arguments = { Argument.FromLabel(label, token) }
			});
		}

		/// <summary>
		/// One conditional jump. Negation is free: every condition is one half of an
		/// On/Off pair, or a comparison with an opposite, so `!` picks the other one
		/// instead of costing an instruction.
		/// </summary>
		private void EmitConditionalJump(Condition condition, string label, bool jumpWhenTrue)
		{
			while (condition is NotCondition negated)
			{
				condition = negated.Inner;
				jumpWhenTrue = !jumpWhenTrue;
			}

			switch (condition)
			{
				case FormCondition form:
				{
					ConditionForm shape = Conditions.Find(form.Name);
					if (shape == null)
					{
						_problems.Add(new Diagnostic(
							"there is no condition called '" + form.Name + "'", form.Token));
						return;
					}
					if (form.Arguments.Count != shape.Arguments)
					{
						_problems.Add(new Diagnostic(string.Format(CultureInfo.InvariantCulture,
							"{0} takes {1} argument(s), not {2}",
							form.Name, shape.Arguments, form.Arguments.Count), form.Token));
						return;
					}

					InstructionItem jump = new InstructionItem
					{
						Opcode = jumpWhenTrue ? shape.JumpIfTrue : shape.JumpIfFalse,
						Mnemonic = Mnemonics.Name(jumpWhenTrue ? shape.JumpIfTrue : shape.JumpIfFalse),
						Token = form.Token
					};
					jump.Arguments.AddRange(form.Arguments);
					jump.Arguments.Add(Argument.FromLabel(label, form.Token));
					_flat.Add(jump);
					return;
				}

				case ValueCondition value:
				{
					string comparison = jumpWhenTrue
						? value.Comparison
						: Conditions.Opposites[value.Comparison];

					InstructionItem jump = new InstructionItem
					{
						Mnemonic = Conditions.ValueJump,
						Token = value.Token
					};
					jump.Arguments.Add(value.Group);
					jump.Arguments.Add(value.Index);
					jump.Arguments.Add(Argument.FromNumber(
						Conditions.Comparisons[comparison], value.Token));
					jump.Arguments.Add(value.Value);
					jump.Arguments.Add(Argument.FromLabel(label, value.Token));
					_flat.Add(jump);
					return;
				}
			}
		}
	}
}
