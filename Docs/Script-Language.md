# The script language

Event scripts are bytecode. This is a text language for them, and a toolchain that
goes both ways:

```
.script  --[disassembler]-->  .ffs  --[lexer, parser, lowering, compiler]-->  .script
```

**All 356 shipped scripts survive that trip byte for byte.** That is the test the whole
thing is built around: if a decompile-and-recompile does not reproduce the original
exactly, something has been lost, and the tool says so rather than letting a modder
find out later.

```bash
dotnet run --project FF3.ContentTool -- script ..\extracted\files ..\ffs --text=..\text\en.lproj
dotnet run --project FF3.ContentTool -- script-build ..\ffs\d01_02.ffs Content\Override\files
```

## What it looks like

```
map 602;

extern talkBegin() = 2, 0xB6744D73;
extern talkEnd()   = 2, 0xC39DEA76;

func sayHello() {
    startMessageWindow(0);
    startMessage2(0, 0x98E569, 0, 0);
    deleteMessageWindow(0);
}

cast 1 {
    main {
        talkBegin();

        if (flag(0, 986)) {
            sayHello();
        } else {
            wait(15);
        }

        for (setValue(0, 3, 0); value(0, 3) < 4; incValue(0, 3)) {
            wait(2);
            if (value(0, 3) == 2) { continue; }
            playSE(270, 1, 192, 127);
        }

        while (!touch()) { wait(1); }

        talkEnd();
    }
    exit { }
}
```

Braces, brackets and semicolons; a statement can be laid out over as many lines as
reads well. `//` and `/* */` are comments and the compiler ignores them, so the
dialogue written beside a `startMessage2` can go stale without breaking a build.

## Control flow

`if`, `else`, `while`, `do ... while`, `for`, `break`, `continue`, `goto`.

There are no expressions in this bytecode - there are conditional jumps - so a
condition is one of those, and **negating one is free**. Every condition is half of an
opcode pair, and `!` picks the other half:

```
if (flag(0, 986)) { ... }     ->   flagOffJump(0, 986, if_else_1);   // jump past
if (!flag(0, 13))  { ... }    ->   flagOnJump(0, 13, if_else_2);     // the other half
```

The conditions are the seven On/Off jump pairs the game has, found by name rather than
listed, so a wider opcode table would widen the language with it. The editor is served
the same list rather than keeping its own, so the two cannot drift:

| Condition | Arguments |
| --- | --- |
| `flag(group, index)` | a story flag |
| `touch()` | the screen is being touched |
| `button()` | a button is down |
| `partyTalkEvent()` | |
| `useItem_Flag()` | |
| `checkPartyPCMemberEnale(who)` | |
| `checkParty_NPCMemberEnale(who)` | |

And **values**, which are the script's variables: `ifValueJump` reads one and compares
it, with the comparison as an operand, so all six work and each has an opposite.

```
while (value(0, 5) < 3) { ... }
if (value(0, 3) != 2) { ... }
```

`setValue`, `incValue`, `decValue`, `addValue`, `mulValue` and `divValue` write them.

A `for` puts `continue` on the step, not the test, so the counter still moves.

## Functions

The format has a function table - id to offset - and `call library, id` looks an id up
in it. Library 2 means `global.script`; anything else means this file's own table. So
functions are not something the language invents; they are already there.

**Your own:**

```
func sayHello() {
    startMessageWindow(0);
}

sayHello();          // compiles to call(0, <id>)
```

The compiler gives the function an id - FNV-1a of its name - and adds the table entry.
Any unused number would work, since the game matches ids exactly, but deriving it from
the name keeps it stable between builds.

**Someone else's:** the game's shared routines have ids but no names we know, so name
them yourself:

```
extern talkBegin() = 2, 0xB6744D73;
talkBegin();
```

Neither kind takes arguments. That is not a gap in the language - the bytecode's
`call` has nowhere to put them. Values are the way to pass something.

## Casts

A **cast** is an actor on a map, with up to three entry points: `init` runs once,
`main` every frame, `exit` when it stops. Write one either way:

```
cast 66 {
    init = none;
    main { talkBegin(); startMessageWindow(0); }   // block: laid out for you
    exit { }
}

cast 2 { main = cast2_main; exit = none; init = none; }   // pointer: at a label
```

The pointer form is what the disassembler emits, because it is the only shape that
fits every shipped script - entry points share addresses, and code runs on past the
end of the block that appears to own it. The block form is what a person writes. They
mix freely in one file.

A block that would run off its end gets an `end()` so it cannot fall into whatever was
laid out after it.

## The grammar

```
file        = declaration*
declaration = "map" number ";"
            | "cast" number "{" entry* "}"
            | "function" number "=" target ";"
            | "func" name "(" ")" block
            | "extern" name "(" ")" "=" number "," number ";"
            | statement
entry       = ("init" | "main" | "exit") ( "=" target ";" | block )
target      = label | number | "none"

statement   = label ":"
            | "if" "(" condition ")" block [ "else" (block | if) ]
            | "while" "(" condition ")" block
            | "do" block "while" "(" condition ")" ";"
            | "for" "(" [call] ";" [condition] ";" [call] ")" block
            | "goto" label ";" | "break" ";" | "continue" ";"
            | "data" number ("," number)* ";"
            | name "(" [ argument ("," argument)* ] ")" ";"
            | block

condition   = "!" condition
            | "value" "(" argument "," argument ")" compare argument
            | name "(" [ argument ("," argument)* ] ")"
argument    = number | string | label
```

- **Numbers** are decimal (`15`, `-1`) or hex (`0xF456A`). Negative numbers are written
  as their two's complement, which is what the game reads back.
- **Strings** are double quoted, with `\n \t \r \0 \" \\`. Fifteen instructions take one.
- **Labels** name an address; writing one as an argument compiles to that address.
- **`data`** is raw bytes, for the places the code region holds something that is not
  an instruction.
- **`op(512);`** names an opcode by number. Ones past the dispatch table have no
  handler and take no arguments; they turn up only in code nothing reaches.

## What the numbers mean

`Tools/gen_operand_names.py` derives operand names by following each one to the call
it ends up in and taking the name from the other side. `ff3Command_PlaySE` hands its
four to `MtxSENDS_Play(int SeqArcNo, int SeqNo, int Volume, int Pan)`:

```
playSE  seqArcNo:word, seqNo:word, volume:word, pan:word
```

413 of the 772 operands get a name that way. The rest stay blank, because a wrong name
is worse than none.

**Fixed point.** 126 operands reach `VecFx32`, which is 1/4096ths of a unit, and those
are decoded in a comment on the line:

```
bootCharacter_AbsoluteCoordination(35, 0xFFFA9000, 0, 0xFFFB9000, 0);   // x -87  y 0  z -71
```

```bash
dotnet run --project FF3.ContentTool -- ops            # all 298
dotnet run --project FF3.ContentTool -- ops camera     # just the camera ones
```

The editor has the same table live - highlighting, completion and a signature strip.
See `Docs/Editor.md`.

## How it is put together

| Piece | File | Job |
| --- | --- | --- |
| Mnemonics | `Ffs/Mnemonics.cs` | opcode ↔ name, from the handler names |
| Conditions | `Ffs/Conditions.cs` | the On/Off pairs, found by name |
| Lexer | `Ffs/Lexer.cs` | text → tokens |
| AST | `Ffs/Ast.cs` | the shape of a parsed script |
| Parser | `Ffs/Parser.cs` | tokens → AST |
| Lowering | `Ffs/Lowering.cs` | if/while/for/functions → labels and jumps |
| Compiler | `Ffs/Compiler.cs` | flat AST → bytecode, in two passes |
| Source writer | `Ffs/SourceWriter.cs` | bytecode → source |

Lowering is the whole trick: by the time the compiler runs, there is no such thing as
an `if`. So adding a structured statement never means touching code generation, and
code generation stays the two passes it always was - assign addresses, then emit with
labels resolved.

## Staying vanilla

Everything the compiler emits runs on the shipped engine. No opcode here is invented,
no handler has been changed, and a script built by this tool is a script the original
game could have shipped. That is deliberate: it means edited content runs for anyone
with the game, not only for someone with our build of it.

Extending the interpreter - a new opcode, a new handler - is a different kind of
change, and the line is worth crossing deliberately rather than by accident. When it
happens, the format already has the place to say so: the header carries a major and a
minor version, `ScriptData.cast` accepts only 1.1, and anything else is refused. So a
script using opcodes outside the vanilla set should be marked 1.2, which makes an old
engine reject it loudly instead of running it wrongly, and lets the tools tell the two
apart.

Until then, `s01_01.script` is the reminder of what that check is for: it is version
1.0, and the game will not load it.

## What it does not do yet

**The disassembler emits the flat form.** Recovering an `if` from a pair of jumps is
decompilation proper, and guessing wrong would silently change what a script does. So
shipped scripts come back as labels and jumps; what you write stays as you wrote it.

**No new message ids from the language.** Adding dialogue means adding a message to a
`.msd` and referencing its id. The editor's *Add character* does allocate one - see
`Docs/Editor.md` - but the compiler on its own does not.

**Functions take no arguments**, because `call` has nowhere to put them.

**Parse errors stop at the first one.** Compile errors are all collected and reported
together; a syntax error is not, because after one the parser no longer knows where it
is.
