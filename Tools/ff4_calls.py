"""What an FF4 script command's handler calls, by symbol name - a first look at its meaning.

    python Tools/ff4_calls.py <libff4.so> <command name> [more names...]

Finds the handler `<name>CommandR12ScriptEngine` (any prefix) in the unstripped .so, then
lists the functions it calls in address order, demangled and de-duplicated, with the
accessor calls (getByte/getWord/getDword/getString) shown as the operand layout. One level
of helpers is followed for the accessor layout only.
"""
import re
import sys

from capstone import Cs, CS_ARCH_ARM64, CS_MODE_ARM
from elftools.elf.elffile import ELFFile

if len(sys.argv) < 3:
    print(__doc__)
    sys.exit(2)

SO = sys.argv[1]
wanted = sys.argv[2:]
elf = ELFFile(open(SO, 'rb'))
text = elf.get_section_by_name('.text')
TEXT = text.data()
TEXT_BASE = text['sh_addr']
md = Cs(CS_ARCH_ARM64, CS_MODE_ARM)
md.detail = True

funcs, by_addr = {}, {}
for sec in elf.iter_sections():
    if sec['sh_type'] in ('SHT_SYMTAB', 'SHT_DYNSYM'):
        for s in sec.iter_symbols():
            if s['st_value'] and s.name and s['st_info']['type'] == 'STT_FUNC':
                funcs[s.name] = (s['st_value'], s['st_size'])
                by_addr[s['st_value']] = s.name

# PLT stubs -> the imported/GOT symbol they jump through, so a call into the PLT reads as the target.
plt = {}
rel = elf.get_section_by_name('.rela.plt')
pltsec = elf.get_section_by_name('.plt')
if rel is not None and pltsec is not None:
    symtab = elf.get_section(rel['sh_link'])
    entries = list(rel.iter_relocations())
    # ARM64 PLT: header 32 bytes, then 16 bytes a stub, in relocation order.
    for i, r in enumerate(entries):
        name = symtab.get_symbol(r['r_info_sym']).name
        plt[pltsec['sh_addr'] + 32 + 16 * i] = name


def demangle(symbol):
    m = re.match(r'_ZN?(\d+)', symbol)
    if not m:
        return symbol
    rest = symbol[m.end():]
    parts = []
    while True:
        m2 = re.match(r'(\d+)', rest)
        if not m2:
            break
        n = int(m2.group(1))
        parts.append(rest[m2.end():m2.end() + n])
        rest = rest[m2.end() + n:]
        if not symbol.startswith('_ZN'):
            break
    return '::'.join(parts) if parts else symbol


ACCESSORS = {
    '_ZN12ScriptEngine7getByteEv': 'byte', '_ZN12ScriptEngine7getWordEv': 'word',
    '_ZN12ScriptEngine8getDwordEv': 'dword', '_ZN12ScriptEngine9getStringEv': 'string',
}


def callees(name, depth=0):
    address, size = funcs[name]
    body = TEXT[address - TEXT_BASE: address - TEXT_BASE + size]
    out = []
    for ins in md.disasm(body, address):
        if ins.mnemonic not in ('bl', 'b'):
            continue
        target = ins.operands[0].imm
        callee = by_addr.get(target) or plt.get(target)
        if not callee:
            out.append(('?', hex(target)))
            continue
        if callee in ACCESSORS:
            out.append(('operand', ACCESSORS[callee]))
        elif callee in funcs and depth < 1 and not callee.startswith('_ZN12ScriptEngine') and funcs[callee][1] < 600:
            out.append(('call', demangle(callee)))
            for kind, what in callees(callee, depth + 1):
                if kind == 'operand':
                    out.append(('operand', what + ' (in helper)'))
        else:
            out.append(('call', demangle(callee)))
    return out


for want in wanted:
    handlers = [n for n in funcs if re.search(r'Command_?' + re.escape(want) + r'R12ScriptEngine', n, re.I)]
    if not handlers:
        print(f'== {want}: no handler symbol found')
        continue
    for h in handlers:
        print(f'== {want}: {demangle(h)} ({funcs[h][1]} bytes)')
        operands = [w for k, w in callees(h) if k == 'operand']
        print('   operands:', ' '.join(operands) if operands else '(none)')
        seen = []
        for k, w in callees(h):
            if k == 'call' and w not in seen:
                seen.append(w)
        for w in seen:
            print('   calls', w)
