# The script language

Event scripts are bytecode. This is a text language for them, and a toolchain that
goes both ways:

```
.script  --[disassembler]-->  .ffs  --[lexer, parser, compiler]-->  .script
```

**All 356 shipped scripts survive that trip byte for byte.** That is the test the whole
thing is built around: if a decompile-and-recompile does not reproduce the original
exactly, something has been lost, and the tool says so rather than letting a modder
find out later.

```bash
# bytecode -> source, with dialogue written in as comments
dotnet run --project FF3.ContentTool -- script ..\extracted\files ..\ffs --text=..\text\en.lproj

# edit ..\ffs\d01_02.ffs, then

dotnet run --project FF3.ContentTool -- script-build ..\ffs\d01_02.ffs Content\Override\files
```

Then run the game. Nothing is repacked and nothing is rebuilt - see
`Docs/Content-Pipeline.md`.

## What a script looks like

```
// d01_02.script

map 602

cast 1 {
    init = none
    main = cast1_main
    exit = cast1_exit
}

function 0xD7D7BCDC = func_3621240028

cast1_main:
    flagOff 10, 0
    bootCharacter 21, 0
    startMessageWindow 0
    startMessage2 0, 0xF456A, 0, 0        // "  %shuyaku2% / There they are!"
    wait 15
    flagOnJump 0, 986, skip_the_scene
    turnCharacter_LookCharacter2 36, 51, 10, 3, 0
skip_the_scene:
    end
```

One statement per line. `//` and `/* */` are comments and are ignored by the compiler,
so the dialogue written beside a `startMessage2` can go stale without breaking a build.

## The grammar

```
file        = item*
item        = "map" number
            | "cast" number "{" entry* "}"
            | "function" number "=" target
            | label ":"
            | "data" number ("," number)*
            | mnemonic [ argument ("," argument)* ]
entry       = ("init" | "main" | "exit") "=" target
target      = label | number | "none"
argument    = number | string | label
```

- **Numbers** are decimal (`15`, `-1`) or hex (`0xF456A`). Negative numbers are written
  as their two's complement, which is what the game reads back.
- **Strings** are double quoted, with `\n \t \r \0 \" \\` escapes. Fifteen instructions
  take one.
- **Labels** name an address. Writing one as an argument compiles to that address, so
  a jump is `jump loop` rather than `jump 0x0A3C`.
- **`data`** is raw bytes, for the handful of places where the code region holds
  something that is not an instruction.
- **`op(512)`** addresses an opcode by number. Opcodes past the dispatch table have no
  handler and take no arguments; they turn up only in code nothing reaches.

## Casts and functions

A **cast** is an actor on the map, with up to three entry points: `init` runs once,
`main` runs every frame, `exit` runs when it stops. `none` means the cast does not have
that one.

A **function** is called by a 32-bit id. `callCommand 2, id` looks the id up in
`global.script`; any other library number looks it up in this file's own table.

Both point at labels rather than owning blocks. That is not a simplification - it is
the only shape that fits the shipped scripts, where two entry points share an address
and execution runs on past the end of the block that appears to own it.

## What the numbers mean

An operand table that says "a word and three dwords" tells you the shape of an
instruction and nothing about what it is for. `Tools/gen_operand_names.py` fills that
in, and like the rest of this it derives rather than guesses: each operand is followed
to the call it ends up in, and the name is taken from the other side.

`ff3Command_PlaySE` hands its four operands to
`MtxSENDS_Play(int SeqArcNo, int SeqNo, int Volume, int Pan)`, so:

```
playSE  seqArcNo:word, seqNo:word, volume:word, pan:word
```

413 of the 772 operands (53%) get a name this way. The rest are left blank, because a
wrong name is worse than none.

**Fixed point.** 126 operands reach `VecFx32`, which holds NDS fixed point - 1/4096ths
of a unit. Those are decoded in a comment beside the line, since `0x64000` is not
something to convert in your head while reading a cutscene:

```
bootCharacter_AbsoluteCoordination 35, 0xFFFA9000, 0, 0xFFFB9000, 0   // x -87  y 0  z -71
moveCharacter_AbsoluteCoordination 53, 0xFFFAC000, 0, 0xFFFD0000, 45  // x -84  y 0  z -48
```

The receiver is checked, not just the method name: `FlagManager` has a `set()` too,
and its arguments are a flag group and an index, not an x and a y.

## The instruction set

298 opcodes. Names come from the game's own handlers, tidied:

| Handler | Mnemonic |
| --- | --- |
| `waitCommand` | `wait` |
| `flagOnJumpCommand` | `flagOnJump` |
| `ff3Command_StartMessage2` | `startMessage2` |

List them, with their arguments:

```bash
dotnet run --project FF3.ContentTool -- ops            # all 298
dotnet run --project FF3.ContentTool -- ops camera     # just the camera ones
```

```
  48  moveCharacter_AbsoluteCoordination   hichIndex:word, x:dword fixed, y:dword fixed, z:dword fixed, frame:word
  91  playBGM                              BGMNo:word, volume:byte, fadeinFrame:word
  93  playSE                               seqArcNo:word, seqNo:word, volume:word, pan:word
```

The editor has the same table live: see `Docs/Editor.md`.

`Docs/Events.md` covers how the engine runs them, and how the operand table is derived
from the handlers rather than guessed.

## Errors

Problems are collected, not thrown one at a time, and each carries a line and column:

```
broken.ffs: 4 problem(s):
  line 195, column 5: startMessageWindow takes 1 argument(s), not 2
  line 247, column 5: unknown instruction 'wiat'
  line 127, column 10: no label called 'nowhere_at_all'
  line 195, column 24: 70000 does not fit in this argument, which holds 0 to 65535
```

## How it is put together

| Piece | File | Job |
| --- | --- | --- |
| Mnemonics | `Ffs/Mnemonics.cs` | opcode ↔ name, derived from the handler names |
| Lexer | `Ffs/Lexer.cs` | text → tokens, with newlines significant |
| AST | `Ffs/Ast.cs` | the shape of a parsed script |
| Parser | `Ffs/Parser.cs` | tokens → AST |
| Compiler | `Ffs/Compiler.cs` | AST → bytecode, in two passes |
| Source writer | `Ffs/SourceWriter.cs` | bytecode → source |

The compiler's first pass assigns an address to every instruction and label - possible
because every instruction has a fixed length - and the second emits bytes with label
references resolved. Then the tables are written around the code: casts before it,
functions after.

## What it does not do yet

**No structured statements.** There is no `if`, no `while`, no block scoping - a jump
is a jump to a label. The AST is built so those can be lowered onto it later without
touching the compiler: an `if` is a conditional jump and a generated label, which is
exactly what this already emits.

**No new message ids.** Adding dialogue means adding a message to a `.msd` file and
referencing its id. Allocating ids automatically wants the two tools to know about each
other, which they do not yet.

**Labels are generated names.** `cast9_main`, `loc_0A3C`. Renaming them is safe and a
good idea when editing - the compiler only cares that they are unique.
