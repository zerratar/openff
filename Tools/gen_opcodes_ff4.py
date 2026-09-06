"""Derive FF4's script instruction set from the game's own engine.

    pip install capstone pyelftools
    python Tools/gen_opcodes_ff4.py <path to libff4.so>

FF4 3D shares FF3's script engine but not its command table: 500 entries to FF3's 298,
222 of them the same command at the same number, 48 of those reading different
operands (FF4's bootEventBattle takes three more bytes, its moveCamera an extra word),
and 255 commands FF3 never had. There is no FF4 source to read, but the Android build
of the engine (libff4.so in the APK) is unstripped: every handler is a named symbol,
so is each operand accessor, and the dispatch table is 500 relocations against those
handler symbols. Disassembling each handler and listing its calls to
ScriptEngine::getByte/getWord/getDword/getString in address order gives the operand
layout - exactly what gen_opcodes.py reads from FF3's C# source.

Handlers call the accessors through PLT stubs, so each stub is mapped back to its
symbol through the GOT slot it loads. Reads that sit after a conditional branch are
reported so they can be checked by hand; the three in this build (SetRewardMessage
Interval, SetBattleBGM, CE_SetMapAsysnc) branch on unrelated state and read regardless.

Reads libff4.so; writes Crystal.Editor/ScriptOpsFf4.cs and nothing else.
"""
import io
import os
import re
import sys

from capstone import Cs, CS_ARCH_ARM64, CS_MODE_ARM
from elftools.elf.elffile import ELFFile
from elftools.elf.relocation import RelocationSection

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)
DEST = os.path.join(ROOT, 'Crystal.Editor', 'ScriptOpsFf4.cs')

TAB = chr(9)
NL = chr(10)

if len(sys.argv) < 2:
    print(__doc__)
    sys.exit(2)
SO = sys.argv[1]

ACCESSORS = {
    '_ZN12ScriptEngine7getByteEv': 'Byte',
    '_ZN12ScriptEngine7getWordEv': 'Word',
    '_ZN12ScriptEngine8getDwordEv': 'Dword',
    '_ZN12ScriptEngine9getStringEv': 'String',
    '_ZN12ScriptEngine4jumpEj': 'JUMP',
    '_ZN12ScriptEngine4callEjj': 'CALL',
}
CONDITIONAL = {'cbz', 'cbnz', 'tbz', 'tbnz'}

elf = ELFFile(open(SO, 'rb'))

# ---- symbols ------------------------------------------------------------------------------------
funcs = {}
by_addr = {}
for sec in elf.iter_sections():
    if sec['sh_type'] in ('SHT_SYMTAB', 'SHT_DYNSYM'):
        for s in sec.iter_symbols():
            if s['st_value'] and s.name and s['st_info']['type'] == 'STT_FUNC':
                funcs[s.name] = (s['st_value'], s['st_size'])
                by_addr[s['st_value']] = s.name


def is_handler(name):
    return name.endswith('R12ScriptEngine') and 'Command' in name


# ---- the dispatch table: the one long run of relocations against handler symbols --------------
dynsym = elf.get_section_by_name('.dynsym')
dyn_names = [s.name for s in dynsym.iter_symbols()]
slots = {}
got_symbol = {}
for sec in elf.iter_sections():
    if isinstance(sec, RelocationSection):
        for r in sec.iter_relocations():
            index = r['r_info_sym']
            if not index:
                continue
            got_symbol[r['r_offset']] = dyn_names[index]
            if is_handler(dyn_names[index]):
                slots[r['r_offset']] = dyn_names[index]
addresses = sorted(slots)
best = None
i = 0
while i < len(addresses):
    j = i
    while j + 1 < len(addresses) and addresses[j + 1] - addresses[j] == 8:
        j += 1
    if best is None or j - i > best[1] - best[0]:
        best = (i, j)
    i = j + 1
table = [slots[addresses[k]] for k in range(best[0], best[1] + 1)]
count = len(table)

# ---- PLT stubs -> symbols, by the GOT slot each stub loads -----------------------------------
md = Cs(CS_ARCH_ARM64, CS_MODE_ARM)
md.detail = True
plt = elf.get_section_by_name('.plt')
page = None
stub = None
for ins in md.disasm(plt.data(), plt['sh_addr']):
    if ins.mnemonic == 'adrp':
        page = ins.operands[1].imm
        stub = ins.address
    elif ins.mnemonic == 'ldr' and page is not None and ins.operands[1].type == 3:
        slot = page + ins.operands[1].mem.disp
        if slot in got_symbol:
            by_addr[stub] = got_symbol[slot]
        page = None

text = elf.get_section_by_name('.text')
TEXT_BASE = text['sh_addr']
TEXT = text.data()


def code_of(name):
    address, size = funcs[name]
    start = address - TEXT_BASE
    return address, TEXT[start:start + size]


memo = {}


def calls_of(name, depth=0):
    """The handler's accessor calls in address order, helpers followed; each tagged with
    whether a conditional branch preceded it."""
    if name in memo:
        return memo[name]
    memo[name] = []
    address, body = code_of(name)
    sequence = []
    branched = False
    for ins in md.disasm(body, address):
        if ins.mnemonic in CONDITIONAL or ins.mnemonic.startswith('b.'):
            branched = True
        if ins.mnemonic not in ('bl', 'b'):
            continue
        target = ins.operands[0].imm
        callee = by_addr.get(target)
        if callee in ACCESSORS:
            sequence.append((ACCESSORS[callee], branched))
        elif (callee and depth < 3 and callee != name and callee in funcs
              and funcs[callee][1] and not callee.startswith('_ZN12ScriptEngine')):
            for kind, inner in calls_of(callee, depth + 1):
                sequence.append((kind, inner or branched))
    memo[name] = sequence
    return sequence


def demangle(symbol):
    """_Z29babilCommand_SetInsideMapJumpR12ScriptEngine -> babilCommand_SetInsideMapJump"""
    m = re.match(r'_Z(\d+)', symbol)
    length = int(m.group(1))
    return symbol[m.end():m.end() + length]


rows = []
for opcode, symbol in enumerate(table):
    sequence = calls_of(symbol)
    operands = [kind for kind, _ in sequence if kind not in ('JUMP', 'CALL')]
    conditional = [kind for kind, after in sequence if after and kind not in ('JUMP', 'CALL')]
    # The operand that reaches engine.jump()/engine.call(): the last Dword read before it.
    target = -1
    for k, (kind, _) in enumerate(sequence):
        if kind == 'JUMP':
            before = [n for n, (t, _) in enumerate(sequence[:k]) if t not in ('JUMP', 'CALL')]
            dwords = [n for n in before if sequence[n][0] == 'Dword']
            if dwords:
                target = before.index(dwords[-1])
            break
    name = demangle(symbol)
    any_target = 'RandomJump' in name
    if any_target:
        target = -1
    rows.append((opcode, name, operands, conditional, target, any_target))

print("opcodes:", count, " distinct handlers:", len(set(table)))
print("with a string operand:", sum(1 for r in rows if 'String' in r[2]),
      " that jump:", sum(1 for r in rows if r[4] >= 0),
      " that pick among several targets:", sum(1 for r in rows if r[5]))
for opcode, name, operands, conditional, target, any_target in rows:
    if conditional:
        print("   reads after a branch, check by hand:", opcode, name, operands)
    if 'ump' in name and target < 0 and not any_target and 'Mapjump' not in name and 'MapJump' not in name:
        print("   named Jump, does not jump in this build:", opcode, name)

head = '''// Generated from libff4.so's dispatch table and handlers - see Docs/Editor.md.
//
// FF4 3D runs FF3's script engine with its own command table: 500 entries, 222 of them
// FF3's command at FF3's number, 48 of those reading different operands, and 255 that
// FF3 does not have. Each handler's operand reads were listed from its machine code,
// which is the same evidence gen_opcodes.py takes from FF3's source.
//
// Do not edit by hand - run Tools/gen_opcodes_ff4.py <libff4.so>, which rebuilds it.

namespace FF3.ContentTool
{
@Tinternal static class ScriptOpsFf4
@T{
@T@Tpublic const int Count = @COUNT;

@T@T/// <summary>FF4's exit declaration: trigger, map, arrival x y z, facing, box corners.</summary>
@T@Tpublic const int SetInsideMapJump = @INSIDE;

@T@Tprivate static readonly Operand[] None = new Operand[0];

@T@Tpublic static readonly ScriptOp[] Table =
@T@T{
'''.replace('@COUNT', str(count)).replace(
    '@INSIDE', str(next(r[0] for r in rows if r[1] == 'babilCommand_SetInsideMapJump')))

tail = '''@T@T};
@T}
}
'''

lines = []
for opcode, name, operands, conditional, target, any_target in rows:
    arg = ('new[] { ' + ', '.join('Operand.' + o for o in operands) + ' }') if operands else 'None'
    lines.append('@T@T@Tnew ScriptOp("%s", %s, false, %d%s),'
                 % (name, arg, target, ', true' if any_target else ''))

out = (head + NL.join(lines) + NL + tail).replace('@T', TAB)
io.open(DEST, 'w', encoding='utf-8', newline=chr(13) + NL).write(out)
print("wrote", os.path.relpath(DEST, ROOT))
