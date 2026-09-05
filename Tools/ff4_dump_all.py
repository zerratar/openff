"""Disassemble the whole FF4 binary to disk, one file per namespace, for reading and grepping.

    python Tools/ff4_dump_all.py <libff4.so> [out dir = Reference/libff4]

Writes:
  symbols.txt     every function and data symbol: address, size, kind, demangled name, mangled name
  strings.txt     every printable C string of .rodata with its address (the "= 0x..." the
                  disassembly notes for adrp/add pairs)
  asm/<ns>.asm    the functions of each top-level namespace (btl, world, menu, ui, layout, ds,
                  mon, itm, pl, evt, ...; "global" for free functions), in address order, with
                  call targets and adrp/add addresses resolved as Tools/ff4_disasm.py does
  INDEX.md        what is where and how to regenerate

The .asm files are large (the binary has about 4.9 MB of code) and are not committed; the
index, symbols and strings are. The disassembly is the reference for Docs/FF4-Internals.md,
which keeps what was read out of it as prose and pseudo-code.
"""
import os
import re
import sys
import time

from capstone import Cs, CS_ARCH_ARM64, CS_MODE_ARM
from elftools.elf.elffile import ELFFile
from elftools.elf.sections import SymbolTableSection

if len(sys.argv) < 2:
    print(__doc__)
    sys.exit(2)
SO = sys.argv[1]
OUT = sys.argv[2] if len(sys.argv) > 2 else os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), 'Reference', 'libff4')
os.makedirs(os.path.join(OUT, 'asm'), exist_ok=True)

elf = ELFFile(open(SO, 'rb'))
text = elf.get_section_by_name('.text')
TEXT, TEXT_BASE = text.data(), text['sh_addr']
md = Cs(CS_ARCH_ARM64, CS_MODE_ARM)
md.detail = True


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


def namespace(symbol):
    m = re.match(r'_ZNK?(\d+)', symbol)
    if not m:
        return 'global'
    n = int(m.group(1))
    return symbol[m.end():m.end() + n]


symbols = {}
for sec in elf.iter_sections():
    if isinstance(sec, SymbolTableSection):
        for s in sec.iter_symbols():
            if not s.name or not s['st_value']:
                continue
            kind = s['st_info']['type']
            if kind not in ('STT_FUNC', 'STT_OBJECT'):
                continue
            symbols[s.name] = (s['st_value'], s['st_size'], kind)
funcs = {n: v for n, v in symbols.items() if v[2] == 'STT_FUNC' and v[1] > 0}
by_addr = {v[0]: n for n, v in funcs.items()}
plt = {}
rel = elf.get_section_by_name('.rela.plt')
pltsec = elf.get_section_by_name('.plt')
if rel is not None and pltsec is not None:
    symtab = elf.get_section(rel['sh_link'])
    for i, r in enumerate(rel.iter_relocations()):
        plt[pltsec['sh_addr'] + 32 + 16 * i] = symtab.get_symbol(r['r_info_sym']).name

started = time.time()
with open(os.path.join(OUT, 'symbols.txt'), 'w', encoding='utf-8') as f:
    f.write('# address size kind demangled | mangled\n')
    for name, (addr, size, kind) in sorted(symbols.items(), key=lambda kv: kv[1][0]):
        f.write(f'{addr:08x} {size:6d} {"F" if kind == "STT_FUNC" else "D"} {demangle(name)} | {name}\n')
print('symbols', len(symbols))

with open(os.path.join(OUT, 'strings.txt'), 'w', encoding='utf-8') as f:
    for secname in ('.rodata', '.data.rel.ro', '.data'):
        sec = elf.get_section_by_name(secname)
        if sec is None:
            continue
        data, base = sec.data(), sec['sh_addr']
        for m in re.finditer(rb'[\x20-\x7e]{4,}\x00', data):
            f.write(f'{base + m.start():08x} {secname} {m.group(0)[:-1].decode("ascii")}\n')
print('strings written')

files = {}
count = 0
for name, (addr, size, _) in sorted(funcs.items(), key=lambda kv: kv[1][0]):
    if addr < TEXT_BASE or addr + size > TEXT_BASE + len(TEXT):
        continue
    ns = namespace(name)
    f = files.get(ns)
    if f is None:
        f = files[ns] = open(os.path.join(OUT, 'asm', re.sub(r'[^A-Za-z0-9_]', '_', ns) + '.asm'), 'w', encoding='utf-8')
    body = TEXT[addr - TEXT_BASE: addr - TEXT_BASE + size]
    f.write(f'\n== {demangle(name)}  @{hex(addr)} ({size} bytes)  {name}\n')
    page = None
    for ins in md.disasm(body, addr):
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
        elif ins.mnemonic == 'ldr' and page and len(ins.operands) == 2 and ins.operands[1].type == 3 and ins.operands[1].mem.base == page[0]:
            note = f'   ; [{hex(page[1] + ins.operands[1].mem.disp)}]'
        f.write(f'   {hex(ins.address)[-6:]}  {ins.mnemonic:8} {ins.op_str}{note}\n')
    count += 1
    if count % 2000 == 0:
        print(count, 'functions', int(time.time() - started), 's')
for f in files.values():
    f.close()

with open(os.path.join(OUT, 'INDEX.md'), 'w', encoding='utf-8') as f:
    f.write('# libff4.so, disassembled\n\n')
    f.write(f'Source: `{os.path.basename(SO)}` (FF4 3D v2.0.4, Android arm64, the phone build - the complete game).\n')
    f.write(f'Regenerate: `python Tools/ff4_dump_all.py <path to libff4.so>` ({count} functions, {len(symbols)} symbols).\n\n')
    f.write('- `symbols.txt` - every function and data symbol with address, size and demangled name.\n')
    f.write('- `strings.txt` - the C strings of .rodata/.data with addresses (what `= 0x...` notes point at).\n')
    f.write('- `asm/<namespace>.asm` - the code by top-level C++ namespace (not committed; regenerate). Each function\n')
    f.write('  starts with `== demangled @address (size) mangled`; `bl` targets are named, `adrp/add` pairs resolved\n')
    f.write('  to `= address`, `adrp/ldr` pairs to `[address]` (a GOT slot - see the relocations for the symbol).\n\n')
    f.write('What has been read out of it lives in `Docs/FF4-Internals.md`.\n\nNamespaces:\n\n')
    for ns in sorted(files):
        f.write(f'- `{ns}`\n')
print('done', count, 'functions in', int(time.time() - started), 's ->', OUT)
