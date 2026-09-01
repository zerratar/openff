"""Audit the disassembler's control-flow model against the interpreter.

    python Tools/check_jumps.py

The engine sets the program counter in exactly four places: engine.jump(), a function
table lookup in engine.call(), the cast entry points, and popping the return stack.
This checks the first two against every handler, and reports anything the generated
table would miss - a handler that jumps to more than one operand, one that jumps to
something that is not an operand at all, or one that calls by an id it computes.

Reads the game sources; writes nothing.
"""
import io
import os
import re

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)
MEMBERS = os.path.join(ROOT, 'FF3.Game', 'GlobalScope', 'GlobalScope.Members.cs')

NL = chr(10)
BS = chr(92)

src = io.open(MEMBERS, encoding='utf-8-sig', newline='').read()

m = re.search(r'commandTable = new SCRIPT_COMMAND' + re.escape('[') + r'(\d+)'
              + re.escape(']') + r'\s*\{(.*?)\};', src, re.S)
names = [n.strip() for n in m.group(2).replace(NL, ' ').split(',') if n.strip()]


def body_of(name):
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

jump_sites = 0
multi = []
not_an_operand = []
calls = []
handlers_that_jump = set()

for op, name in enumerate(names):
    body = body_of(name)
    if body is None:
        continue

    reads = [(mm.start(), mm.end()) for mm in READ.finditer(body)]
    assigned = {}
    for mm in re.finditer(r'(\w+)\s*=\s*engine\.get(?:Byte|Word|Dword|String)\(\)', body):
        for i, (start, end) in enumerate(reads):
            if start >= mm.start() and end <= mm.end():
                assigned[mm.group(1)] = i
                break

    targets = set()
    unresolved = []
    for mm in re.finditer(r'engine\.jump\(([^;]*?)\);', body):
        jump_sites += 1
        handlers_that_jump.add(name)
        arg = mm.group(1).strip()
        if 'engine.get' in arg:
            for i, (start, end) in enumerate(reads):
                if mm.start() <= start and end <= mm.end():
                    targets.add(i)
            continue
        bare = arg.replace('(uint)', '').replace('(int)', '').strip()
        if bare in assigned:
            targets.add(assigned[bare])
        elif re.match(r'^\w+' + re.escape('[') + r'.*' + re.escape(']') + r'$', bare):
            # array of operands - LabelRandomJump; every Dword read is a target
            targets.update(i for i, _ in enumerate(reads))
        else:
            unresolved.append(bare)

    if len(targets) > 1:
        multi.append((op, name, sorted(targets)))
    if unresolved:
        not_an_operand.append((op, name, unresolved))

    for mm in re.finditer(r'engine\.call\(([^;]*?)\);', body):
        calls.append((op, name, mm.group(1).strip()))

print("engine.jump() call sites:", jump_sites, " in", len(handlers_that_jump), "handlers")
print()
print("handlers that jump to more than one operand:", len(multi))
for op, name, targets in multi:
    print("   %3d %-46s operands %s" % (op, name, targets))
print()
print("handlers that jump to something that is not an operand:", len(not_an_operand))
for op, name, what in not_an_operand:
    print("   %3d %-46s %s" % (op, name, what))
print()
print("engine.call() sites:", len(calls))
for op, name, args in calls:
    print("   %3d %-46s call(%s)" % (op, name, args))
