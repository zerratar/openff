"""Derive the script instruction set from the decompiled handlers.

    python Tools/gen_opcodes.py

The engine dispatches a 16-bit opcode through commandTable[298]; each handler reads
its operands off the instruction stream with getByte/getWord/getDword/getString.
Reading those calls in source order gives the operand layout - but only if they are
unconditional, so anything read inside a branch is reported rather than guessed.

Also works out which operand a branching handler passes to engine.jump(), because
"the first Dword" is wrong: conditional jumps read their flags first, and some read a
whole coordinate box before the destination.

Reads the game sources; writes FF3.ContentTool/ScriptOps.cs and nothing else.
"""
import io
import os
import re

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)
MEMBERS = os.path.join(ROOT, 'FF3.Game', 'GlobalScope', 'GlobalScope.Members.cs')
DEST = os.path.join(ROOT, 'FF3.ContentTool', 'ScriptOps.cs')

TAB = chr(9)
NL = chr(10)
BS = chr(92)

src = io.open(MEMBERS, encoding='utf-8-sig', newline='').read()

m = re.search(r'commandTable = new SCRIPT_COMMAND' + re.escape('[') + r'(\d+)'
              + re.escape(']') + r'\s*\{(.*?)\};', src, re.S)
count = int(m.group(1))
names = [n.strip() for n in m.group(2).replace(NL, ' ').split(',') if n.strip()]
assert len(names) == count, (len(names), count)


def body_of(name):
    """The handler's body, by brace matching. Skips string and char literals."""
    mm = re.search(r'internal static void ' + re.escape(name)
                   + r'\(ScriptEngine \w+\)\s*\n\s*\{', src)
    if not mm:
        return None
    i = src.index('{', mm.end() - 1)
    depth, j = 0, i
    while j < len(src):
        c = src[j]
        if c == '"' or c == chr(39):
            quote = c
            j += 1
            while src[j] != quote:
                j += 2 if src[j] == BS else 1
        elif c == '{':
            depth += 1
        elif c == '}':
            depth -= 1
            if depth == 0:
                break
        j += 1
    return src[i + 1:j]


READ = re.compile(r'engine\.get(Byte|Word|Dword|String)\(\)')


def operands(body):
    """Ordered reads, and whether any of them sits inside a branch."""
    out, conditional, depth, i, n = [], False, 0, 0, len(body)
    while i < n:
        c = body[i]
        if c == '"' or c == chr(39):
            quote = c
            i += 1
            while i < n and body[i] != quote:
                i += 2 if body[i] == BS else 1
            i += 1
            continue
        if c == '/' and body[i:i + 2] == '//':
            i = body.find(NL, i)
            if i < 0:
                break
            continue
        if c == '{':
            depth += 1
        elif c == '}':
            depth -= 1
        else:
            mm = READ.match(body, i)
            if mm:
                out.append(mm.group(1))
                if depth > 0:
                    conditional = True
                i = mm.end()
                continue
        i += 1
    return out, conditional


def jumps_to_any(body):
    """True where the handler picks its destination among several reads.

    ff3Command_LabelRandomJump reads six addresses and jumps to one at random, so
    every one of them is a target and none of them is *the* target.
    """
    return re.search(r'engine\.jump\(\w+' + re.escape('[') + r'[^;]*' + re.escape(']')
                     + r'\)', body) is not None


def jump_operand(body):
    """Index of the read whose value reaches engine.jump(), or -1."""
    reads = [(mm.start(), mm.end()) for mm in READ.finditer(body)]

    assigned = {}
    for mm in re.finditer(r'(\w+)\s*=\s*engine\.get(?:Byte|Word|Dword|String)\(\)', body):
        for i, (start, end) in enumerate(reads):
            if start >= mm.start() and end <= mm.end():
                assigned[mm.group(1)] = i
                break

    for mm in re.finditer(r'engine\.jump\(([^;]*?)\);', body):
        arg = mm.group(1).strip()
        if 'engine.get' in arg:                     # engine.jump(engine.getDword())
            for i, (start, end) in enumerate(reads):
                if mm.start() <= start and end <= mm.end():
                    return i
        arg = arg.replace('(uint)', '').replace('(int)', '').strip()
        if arg in assigned:                         # engine.jump(dword)
            return assigned[arg]
    return -1


rows = []
for op, name in enumerate(names):
    body = body_of(name)
    if body is None:
        rows.append((op, name, None, False, -1, False))
        continue
    ops, conditional = operands(body)
    rows.append((op, name, ops, conditional, jump_operand(body), jumps_to_any(body)))

print("opcodes:", count,
      " handlers not found:", sum(1 for r in rows if r[2] is None),
      " with conditional reads:", sum(1 for r in rows if r[3]))
print("distinct handlers:", len(set(names)))
print("fixed length:", sum(1 for r in rows if r[2] is not None and 'String' not in r[2]),
      " with a string operand:", sum(1 for r in rows if r[2] and 'String' in r[2]))
print("opcodes that jump:", sum(1 for r in rows if r[4] >= 0),
      " that pick among several targets:", sum(1 for r in rows if r[5]))
for op, name, ops, conditional, jump, any_target in rows:
    if 'ump' in name and jump < 0 and not any_target:
        print("   named Jump, does not jump in this build:", name)

head = '''// Generated from the decompiled command table - see Docs/Events.md.
//
// The engine dispatches a 16 bit opcode through a 298 entry table, and each handler
// reads its operands off the instruction stream. This table is those handlers' names
// and the operands each one reads, in order, which is what makes the bytecode
// walkable: an instruction is the opcode word plus the bytes its operands occupy.
//
// Do not edit by hand - run Tools/gen_opcodes.py, which rebuilds it from the sources.

namespace FF3.ContentTool
{
@Tinternal enum Operand
@T{
@T@TByte,
@T@TWord,
@T@TDword,
@T@TString
@T}

@Tinternal sealed class ScriptOp
@T{
@T@Tpublic readonly string Name;
@T@Tpublic readonly Operand[] Operands;

@T@T/// <summary>
@T@T/// True where the handler reads operands inside a branch, so the instruction's
@T@T/// length would depend on state the disassembler does not have. None of the 298
@T@T/// currently do, which is why a linear walk is exact.
@T@T/// </summary>
@T@Tpublic readonly bool Variable;

@T@T/// <summary>
@T@T/// Index of the operand the handler passes to engine.jump(), or -1. Not simply
@T@T/// the first Dword: a conditional jump reads its flags first, and some read a
@T@T/// whole coordinate box before the destination.
@T@T/// </summary>
@T@Tpublic readonly int JumpOperand;

@T@T/// <summary>
@T@T/// True where the destination is chosen among several operands rather than being
@T@T/// one of them - LabelRandomJump reads six addresses and takes one at random.
@T@T/// </summary>
@T@Tpublic readonly bool JumpsToAny;

@T@Tpublic ScriptOp(string name, Operand[] operands, bool variable, int jumpOperand,
@T@T@Tbool jumpsToAny = false)
@T@T{
@T@T@TName = name;
@T@T@TOperands = operands;
@T@T@TVariable = variable;
@T@T@TJumpOperand = jumpOperand;
@T@T@TJumpsToAny = jumpsToAny;
@T@T}
@T}

@Tinternal static class ScriptOps
@T{
@T@Tpublic const int Count = @COUNT;

@T@Tprivate static readonly Operand[] None = new Operand[0];

@T@Tpublic static readonly ScriptOp[] Table =
@T@T{
'''.replace('@COUNT', str(count))

tail = '''@T@T};

@T@Tpublic static ScriptOp Get(int opcode)
@T@T{
@T@T@Treturn opcode >= 0 && opcode < Table.Length ? Table[opcode] : null;
@T@T}
@T}
}
'''

lines = []
for op, name, ops, conditional, jump, any_target in rows:
    if ops is None:
        lines.append('@T@T@Tnew ScriptOp("%s", null, true, -1),   // handler not found' % name)
        continue
    arg = ('new[] { ' + ', '.join('Operand.' + o for o in ops) + ' }') if ops else 'None'
    lines.append('@T@T@Tnew ScriptOp("%s", %s, %s, %d%s),'
                 % (name, arg, 'true' if conditional else 'false', jump,
                    ', true' if any_target else ''))

text = (head + NL.join(lines) + NL + tail).replace('@T', TAB)
io.open(DEST, 'w', encoding='utf-8', newline=chr(13) + NL).write(text)
print("wrote", os.path.relpath(DEST, ROOT))
