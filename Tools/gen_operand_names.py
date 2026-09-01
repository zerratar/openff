"""Work out what a script instruction's operands actually mean.

    python Tools/gen_operand_names.py

The operand table says an instruction takes a word and three dwords. It does not say
that the word is a character and the dwords are an x, y, z position in fixed point,
which is the difference between reading a script and staring at it.

The handlers know. `ff3Command_PlaySE` passes its four operands straight to
`MtxSENDS_Play(int SeqArcNo, int SeqNo, int Volume, int Pan)`, and
`ff3Command_MoveCharacter_AbsoluteCoordination` feeds three of its reads into
`VecFx32.set`, whose fields are x, y and z. So a name is looked up by following each
operand to the call it ends up in and taking the parameter name from the other side.

Anything that cannot be followed is left unnamed rather than guessed at.

Writes FF3.ContentTool/ScriptOperands.cs; reads everything else.
"""
import io
import os
import re

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)
GAME = os.path.join(ROOT, 'FF3.Game')
MEMBERS = os.path.join(GAME, 'GlobalScope', 'GlobalScope.Members.cs')
DEST = os.path.join(ROOT, 'FF3.ContentTool', 'ScriptOperands.cs')

TAB = chr(9)
NL = chr(10)
BS = chr(92)

# Decompiler names that say nothing. A local called these is not worth reporting.
NOISE = re.compile(r'^(word|dword|byte|b|num|flag|arg|result|value|text|str|i|j|k|n|'
                   r'v|vec|vecFx|obj|ptr|tmp)[0-9]*$', re.IGNORECASE)

# Fixed point: the NDS stores positions as 1/4096ths, and VecFx32 is where they land.
# The receiver has to be checked, not just the method name - FlagManager has a set()
# too, and its arguments are a flag group and an index, not an x and a y.
FIXED_METHOD = 'set'
FIXED_TYPE = 'VecFx32' 


def read_all(root):
    out = {}
    for folder, _, names in os.walk(root):
        for name in sorted(names):
            if name.endswith('.cs'):
                path = os.path.join(folder, name)
                out[path] = io.open(path, encoding='utf-8-sig', newline='').read()
    return out


SOURCES = read_all(GAME)
src = io.open(MEMBERS, encoding='utf-8-sig', newline='').read()

table = re.search(r'commandTable = new SCRIPT_COMMAND' + re.escape('[') + r'(\d+)'
                  + re.escape(']') + r'\s*\{(.*?)\};', src, re.S)
count = int(table.group(1))
handlers = [n.strip() for n in table.group(2).replace(NL, ' ').split(',') if n.strip()]


def block_after(text, at):
    i = text.index('{', at)
    depth, j = 0, i
    while j < len(text):
        c = text[j]
        if c == '{':
            depth += 1
        elif c == '}':
            depth -= 1
            if depth == 0:
                break
        j += 1
    return text[i + 1:j]


def handler_body(name):
    mm = re.search(r'internal static void ' + re.escape(name)
                   + r'\(ScriptEngine \w+\)\s*\n\s*\{', src)
    return None if mm is None else block_after(src, mm.end() - 1)


_signatures = {}


def parameters_of(method, arity):
    """Parameter names of a method taking `arity` arguments.

    The count has to match: engine.startLogic(castNo) takes one argument, and picking
    up LogicManager.startLogic(mapNo, castNo) instead would call the cast a map.
    """
    key = (method, arity)
    if key in _signatures:
        return _signatures[key]

    pattern = re.compile(r'(?:static\s+)?[A-Za-z_][A-Za-z_0-9.<>' + re.escape('[]')
                         + r']*\s+' + re.escape(method)
                         + r'\s*\(([^)]*)\)\s*\{')
    for text in SOURCES.values():
        for mm in pattern.finditer(text):
            names = []
            for part in mm.group(1).split(','):
                part = part.strip()
                if not part:
                    continue
                names.append(part.split('=')[0].strip().split()[-1])
            if len(names) == arity:
                _signatures[key] = names
                return names
    _signatures[key] = None
    return None


def split_arguments(text):
    """Top level comma separated arguments of a call."""
    args, depth, current = [], 0, ''
    for c in text:
        if c in '([':
            depth += 1
        elif c in ')]':
            depth -= 1
        if c == ',' and depth == 0:
            args.append(current.strip())
            current = ''
        else:
            current += c
    if current.strip():
        args.append(current.strip())
    return args


READ = re.compile(r'engine\.get(Byte|Word|Dword|String)\(\)')
CALL = re.compile(r'(?:(\w+)\s*\.\s*)?(\w+)\s*\(')


def clean(argument):
    return re.sub(r'^\((?:u?int|byte|sbyte|short|ushort|long|uint)\)\s*', '',
                  argument).strip()


def describe(name):
    """(operand names, fixed point flags) for one handler."""
    body = handler_body(name)
    if body is None:
        return None, None

    reads = [(mm.start(), mm.end()) for mm in READ.finditer(body)]
    if not reads:
        return [], []

    # Locals that actually hold a position.
    vectors = set(re.findall(r'VecFx32\s+(\w+)\s*=', body))

    # local name -> read index, and the local's own name where it says something
    assigned = {}
    for mm in re.finditer(r'(\w+)\s*(?:=|\.set\()\s*engine\.get\w*\(\)', body):
        for i, (start, end) in enumerate(reads):
            if start >= mm.start() and end <= mm.end():
                assigned[mm.group(1)] = i
                break

    named = [None] * len(reads)
    fixed = [False] * len(reads)

    # A local the decompiler managed to name is already an answer.
    for local, index in assigned.items():
        if not NOISE.match(local):
            named[index] = local.strip('_')

    # Then follow each operand into the call it is used in.
    for mm in CALL.finditer(body):
        receiver, method = mm.group(1), mm.group(2)
        if method in ('get', 'getByte', 'getWord', 'getDword', 'getString'):
            continue
        open_at = mm.end() - 1
        depth, j = 0, open_at
        while j < len(body):
            if body[j] == '(':
                depth += 1
            elif body[j] == ')':
                depth -= 1
                if depth == 0:
                    break
            j += 1
        arguments = split_arguments(body[open_at + 1:j])
        signature = parameters_of(method, len(arguments))

        for position, argument in enumerate(arguments):
            bare = clean(argument)
            index = None
            if bare in assigned:
                index = assigned[bare]
            elif 'engine.get' in bare:
                for i, (start, end) in enumerate(reads):
                    if open_at <= start and end <= j:
                        # inline reads inside this call, in order
                        inline = [i for i, (s, e) in enumerate(reads)
                                  if open_at <= s and e <= j]
                        if position < len(inline):
                            index = inline[position]
                        break
            if index is None:
                continue

            if method == FIXED_METHOD and receiver in vectors:
                fixed[index] = True
                if named[index] is None and position < 3:
                    named[index] = 'xyz'[position]
                continue

            if signature and position < len(signature) and named[index] is None:
                candidate = signature[position]
                candidate = candidate.strip('_')
                if candidate and not NOISE.match(candidate):
                    # Lower the first letter, but leave an acronym alone: BGMNo
                    # should not come out as bGMNo.
                    if len(candidate) > 1 and candidate[1].isupper():
                        named[index] = candidate
                    else:
                        named[index] = candidate[0].lower() + candidate[1:]

    return named, fixed


rows = []
for opcode, handler in enumerate(handlers):
    names, fixed = describe(handler)
    rows.append((opcode, handler, names, fixed))

total = sum(len(r[2]) for r in rows if r[2])
knownNames = sum(1 for r in rows if r[2] for n in r[2] if n)
knownFixed = sum(1 for r in rows if r[3] for f in r[3] if f)
print("operands:", total, " named:", knownNames,
      "(%.0f%%)" % (100.0 * knownNames / max(total, 1)),
      " fixed point:", knownFixed)
print()
for opcode, handler, names, fixed in rows:
    if names and any(names):
        shown = ', '.join((n or '?') + ('*' if fixed[i] else '')
                          for i, n in enumerate(names))
        print("  %3d  %-46s %s" % (opcode, handler, shown))

head = '''// Generated: what each script operand means - see Docs/Script-Language.md.
//
// The operand table says an instruction takes a word and three dwords. This says the
// word is a character and the dwords are a position, which is the difference between
// reading a script and staring at it.
//
// The names are not invented. Each operand is followed to the call it ends up in and
// the name is taken from the other side: ff3Command_PlaySE hands its four operands to
// MtxSENDS_Play(int SeqArcNo, int SeqNo, int Volume, int Pan), so that is what they
// are called. Operands that cannot be followed are left null rather than guessed at.
//
// Fixed is set where a value reaches VecFx32, which holds NDS fixed point - 1/4096ths
// of a unit, so 0x64000 is 100.0.
//
// Do not edit by hand - run Tools/gen_operand_names.py.

namespace FF3.ContentTool
{
@Tinternal static class ScriptOperands
@T{
@T@T/// <summary>Operand names per opcode; an entry or a name may be null.</summary>
@T@Tpublic static readonly string[][] Names =
@T@T{
'''

middle = '''@T@T};

@T@T/// <summary>Which operands are NDS fixed point, 1/4096ths of a unit.</summary>
@T@Tpublic static readonly bool[][] Fixed =
@T@T{
'''

tail = '''@T@T};

@T@Tpublic static string Name(int opcode, int operand)
@T@T{
@T@T@Treturn opcode >= 0 && opcode < Names.Length && Names[opcode] != null
@T@T@T@T&& operand >= 0 && operand < Names[opcode].Length
@T@T@T@T? Names[opcode][operand] : null;
@T@T}

@T@Tpublic static bool IsFixed(int opcode, int operand)
@T@T{
@T@T@Treturn opcode >= 0 && opcode < Fixed.Length && Fixed[opcode] != null
@T@T@T@T&& operand >= 0 && operand < Fixed[opcode].Length
@T@T@T@T&& Fixed[opcode][operand];
@T@T}
@T}
}
'''


def csharp_strings(names):
    if names is None:
        return '@T@T@Tnull,'
    if not names:
        return '@T@T@Tnew string[0],'
    parts = ', '.join('null' if n is None else '"%s"' % n for n in names)
    # Spelt out: an array of nothing but nulls cannot be inferred.
    return '@T@T@Tnew string[] { %s },' % parts


def csharp_bools(fixed):
    if fixed is None:
        return '@T@T@Tnull,'
    if not fixed:
        return '@T@T@Tnew bool[0],'
    return '@T@T@Tnew bool[] { %s },' % ', '.join('true' if f else 'false' for f in fixed)


text = (head
        + NL.join(csharp_strings(r[2]) for r in rows) + NL
        + middle
        + NL.join(csharp_bools(r[3]) for r in rows) + NL
        + tail).replace('@T', TAB)
io.open(DEST, 'w', encoding='utf-8', newline=chr(13) + NL).write(text)
print()
print("wrote", os.path.relpath(DEST, ROOT))
