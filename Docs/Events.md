# Events

The `.script` files are the game's event bytecode: what an NPC does when you talk to
it, when a cutscene fires, what a chest holds, how a quest moves forward. 357 files,
3059 casts, 72770 instructions.

```bash
dotnet run --project FF3.ContentTool -- script ..\extracted\files ..\scripts --text=..\text\en.lproj
```

`--text` points at decoded `.msd` JSON and writes each line of dialogue in beside the
instruction that shows it, which is the difference between a legal disassembly and a
readable one:

```
cast6_main:
  0AC1  ff3Command_StartMessageWindow 0
  0ACB  ff3Command_StartMessage2 0 0x1317B85 2 0   ; "Oh yeah? Ghosts don't exist? / Then go there yourself and prove it!"
  0AD5  ff3Command_StartMotionCharacterDX 0x24 0x3FC 0 5 0 0
  0B02  callCommand 2 0x66CAF9A3
  0B0C  ff3Command_StartMessage2 0 0x1317B86 2 0   ; "Ha! You don't have the guts! / You're a coward!"
  0B4C  ff3Command_BootNameEntry 1
```

That is the opening of the game.

## How it runs

`ScriptEngine` fetches a 16-bit opcode and dispatches it through a 298 entry table.
Each handler reads its own operands off the instruction stream. Execution is
cooperative: `waitCommand` and the message commands suspend the script and resume it
on a later frame, which is why `wait` is the single most common instruction.

A **cast** is an actor on the map. Each has up to three entry points - a constructor,
a body run every frame, and a destructor - and the file's header lists them. Casts
talk to each other through `startLogic`/`stopLogic` and through a global flag space
(`flagOnCommand`, `flagOnJumpCommand`), which is also where quest progress is stored.

`callCommand library id` calls a function by a 32-bit id. Library 2 means the shared
global script; anything else means this file's own function table.

## The instruction set

`FF3.ContentTool/ScriptOps.cs` is generated, not written:

```bash
python Tools/gen_opcodes.py
```

It reads the decompiled handlers and records, for each opcode, the operands that
handler reads and in what order. Two things make this reliable rather than a guess:

- **No handler reads an operand inside a branch.** All 298 were checked. Every
  instruction therefore has a fixed length, and a walk cannot drift out of alignment.
- **Jump destinations come from the handler**, specifically the operand it passes to
  `engine.jump()`. Assuming "the first Dword" is wrong: conditional jumps read their
  flags first, and `WithOutCharacterJump` reads a whole coordinate box before the
  destination. `LabelRandomJump` reads six addresses and picks one at random, so all
  six are targets.

236 of the 298 opcodes appear in the shipped scripts. The busiest:

| Count | Opcode |
| ---: | --- |
| 8363 | `waitCommand` |
| 6172 | `callCommand` |
| 5969 | `endCommand` |
| 4910 | `ff3Command_StartMessage2` |
| 2397 | `ff3Command_StartMessageWindow` |
| 2323 | `ff3Command_StartMotionCharacterDX` |

## Is the walk right?

"Nothing calls this code" is a claim about the data, and it is only worth as much as
the reader behind it. Four checks, all reproducible:

**The interpreter can only set the program counter in four places.** `engine.jump()`,
a function table lookup inside `engine.call()`, the cast entry points in
`Logic.setEnable/setExecute/setDisable`, and `LogicContext.pop()` restoring a pushed
address. There is no fifth. Nothing outside a `.script` file can hand the engine a raw
address - external data supplies cast numbers and function ids, which resolve through
tables inside the file, and those are enumerated.

**Every jump destination is an operand.** `python Tools/check_jumps.py` walks all 298
handlers: 52 `engine.jump()` sites across 39 handlers, none of them jumping to
anything but an operand, and exactly one - `LabelRandomJump` - choosing among several.
`engine.call()` appears three times, always with operands.

**Every call resolves.** `python Tools/check_calls.py <disassembly> <files>` checks all
6462 call sites against the table each would look in - `global.script` for library 2,
the file's own table otherwise. All 6462 resolve, including the 559 that sit in the
unreached regions, which is itself evidence that those regions are real code rather
than data being misread.

**No instruction overlaps the next.** Every instruction has a fixed length, so a wrong
operand list would put the walk half an instruction out and it would never recover.
Zero overlaps across all 356 files; the disassembler prints a warning if that ever
changes.

Two of my own mistakes died in that process, which is the argument for running the
checks rather than reasoning about them: `NOPCommand` is not a terminator (its handler
is empty, so execution carries straight on), and the first version of the call check
read `flagOnCallCommand`'s flag pair as its library and id, reporting 279 phantom
unresolved calls.

## What is not resolved

**About an eighth of the bytecode is not reachable.** Walking from every cast entry
and every function in a file's own table reaches 63004 instructions; 9766 more sit in
regions nothing in the file points at. They are not junk - decoded linearly they come
out as clean instructions, only 21 bytes across all 356 files failing to decode at
all. The listing shows them under `; ---- not reached from any entry point in this
file`, so what was walked is never confused with what was swept.

What this means, given the checks above: the shipped engine cannot reach them. Their
addresses are in no cast table and no function table, nothing jumps to them, and the
engine has no other way to obtain an address. They are also not `.hich` entry points -
`.hich` holds character placement and refers to casts and logic ids, not code offsets.

So they are dead in this build. That is not the same as meaningless: they are
well-formed, they call real functions, and they are most likely content that was cut
or logic whose entry points were removed. Worth reading, not worth wiring up blindly.

**`s01_01.script` is version 1.0**, and the loader accepts only 1.1. The game rejects
it too, in `ScriptData.cast`, so it is dead weight in the archive rather than a gap in
the tool.

**Writing events** is covered by `Docs/Script-Language.md`: the disassembler emits a
text language, and a lexer, parser and compiler take it back to bytecode. All 356
shipped scripts survive that round trip byte for byte. What is still missing there is
structured statements and a way to allocate new message ids.
