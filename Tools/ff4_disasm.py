"""Disassemble functions of the unstripped FF4 binary by symbol substring.

    python Tools/ff4_disasm.py <libff4.so> <substring> [more substrings...] [--max=N]

Prints each matching function (demangled) with its ARM64 instructions; call targets and
PLT stubs are resolved to symbol names, adrp/add pairs to their absolute address. For
reading a data format out of the code that parses it.
"""
import re
import sys

from capstone import Cs, CS_ARCH_ARM64, CS_MODE_ARM
from elftools.elf.elffile import ELFFile

args = [a for a in sys.argv[1:] if not a.startswith('--')]
opts = {a.split('=')[0][2:]: a.split('=', 1)[1] for a in sys.argv[1:] if a.startswith('--') and '=' in a}
if len(args) < 2:
    print(__doc__)
    sys.exit(2)
SO, wanted = args[0], args[1:]
MAX = int(opts.get('max', '400'))

elf = ELFFile(open(SO, 'rb'))
text = elf.get_section_by_name('.text')
TEXT, TEXT_BASE = text.data(), text['sh_addr']
md = Cs(CS_ARCH_ARM64, CS_MODE_ARM)
md.detail = True

funcs, by_addr = {}, {}
for sec in elf.iter_sections():
    if sec['sh_type'] in ('SHT_SYMTAB', 'SHT_DYNSYM'):
        for s in sec.iter_symbols():
            if s['st_value'] and s.name and s['st_info']['type'] == 'STT_FUNC':
                funcs[s.name] = (s['st_value'], s['st_size'])
                by_addr[s['st_value']] = s.name
plt = {}
rel = elf.get_section_by_name('.rela.plt')
pltsec = elf.get_section_by_name('.plt')
if rel is not None and pltsec is not None:
    symtab = elf.get_section(rel['sh_link'])
    for i, r in enumerate(rel.iter_relocations()):
        plt[pltsec['sh_addr'] + 32 + 16 * i] = symtab.get_symbol(r['r_info_sym']).name


def demangle(symbol):
    m = re.match(r'_ZN?K?(\d+)', symbol)
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
    return '::'.join(parts) + (' ' + rest if rest else '') if parts else symbol


def dump(name):
    address, size = funcs[name]
    body = TEXT[address - TEXT_BASE: address - TEXT_BASE + size]
    print(f'== {demangle(name)}  @{hex(address)} ({size} bytes)')
    page = None
    count = 0
    for ins in md.disasm(body, address):
        count += 1
        if count > MAX:
            print('   ...')
            break
        note = ''
        if ins.mnemonic in ('bl', 'b') and ins.operands and ins.operands[0].type == 2:
            target = ins.operands[0].imm
            callee = by_addr.get(target) or plt.get(target)
            if callee:
                note = '   ; ' + demangle(callee)
        if ins.mnemonic == 'adrp':
            page = (ins.operands[0].reg, ins.operands[1].imm)
        elif ins.mnemonic == 'add' and page and len(ins.operands) == 3 and ins.operands[1].reg == page[0] and ins.operands[2].type == 2:
            note = f'   ; = {hex(page[1] + ins.operands[2].imm)}'
        print(f'   {hex(ins.address)[-6:]}  {ins.mnemonic:8} {ins.op_str}{note}')


def dump_at(address, size):
    funcs['at_' + hex(address)] = (address, size)
    dump('at_' + hex(address))


for want in wanted:
    if want.startswith('0x'):
        # An address, optionally with a size: 0x45be68:96 - for functions without a symbol.
        parts = want.split(':')
        dump_at(int(parts[0], 16), int(parts[1]) if len(parts) > 1 else 96)
        continue
    matches = [n for n in funcs if want in n or want in demangle(n)]
    if not matches:
        print(f'== {want}: nothing matches')
    for n in sorted(set(matches), key=lambda n: funcs[n][0]):
        dump(n)
