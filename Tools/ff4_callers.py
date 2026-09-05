"""Who calls a function in libff4.so - the callers, by symbol, with the call sites.

    python Tools/ff4_callers.py <libff4.so> <symbol substring> [more...] [--disasm]

Scans .text for `bl`/`b` to every function whose mangled name contains a pattern - directly
or through its PLT stub, which is how most calls in this binary go - and prints the enclosing
functions (the symbol table's sizes bound them). With --disasm the twenty instructions before
each call site are printed too - where the arguments come from.
"""
import sys, re, bisect
from elftools.elf.elffile import ELFFile
from elftools.elf.relocation import RelocationSection
from capstone import Cs, CS_ARCH_ARM64, CS_MODE_ARM

args = [a for a in sys.argv[1:] if not a.startswith('--')]
disasm = '--disasm' in sys.argv
SO = args[0]; pats = args[1:]
elf = ELFFile(open(SO, 'rb'))
funcs = {}
for sec in elf.iter_sections():
    if sec['sh_type'] in ('SHT_SYMTAB', 'SHT_DYNSYM'):
        for s in sec.iter_symbols():
            if s['st_value'] and s.name and s['st_info']['type'] == 'STT_FUNC':
                funcs[s['st_value']] = (s.name, s['st_size'])
starts = sorted(funcs)
def owner(addr):
    i = bisect.bisect_right(starts, addr) - 1
    if i < 0: return None
    name, size = funcs[starts[i]]
    return name if starts[i] <= addr < starts[i] + max(size, 4) else None
# PLT stubs: adrp/ldr pairs whose GOT slot a relocation names.
dn = [s.name for s in elf.get_section_by_name('.dynsym').iter_symbols()]; got = {}
for sec in elf.iter_sections():
    if isinstance(sec, RelocationSection):
        for r in sec.iter_relocations():
            if r['r_info_sym']: got[r['r_offset']] = dn[r['r_info_sym']]
md = Cs(CS_ARCH_ARM64, CS_MODE_ARM); md.detail = True
stub_name = {}
plt = elf.get_section_by_name('.plt')
if plt is not None:
    page = stub = None
    for ins in md.disasm(plt.data(), plt['sh_addr']):
        if ins.mnemonic == 'adrp': page = ins.operands[1].imm; stub = ins.address
        elif ins.mnemonic == 'ldr' and page is not None and ins.operands[1].type == 3:
            stub_name[stub] = got.get(page + ins.operands[1].mem.disp, '?'); page = None
targets = {a: n for a, (n, _) in funcs.items() if any(re.search(p, n) for p in pats)}
for a, n in list(stub_name.items()):
    if any(re.search(p, n) for p in pats): targets[a] = n
for a, n in sorted(targets.items()): print('target %x %s' % (a, n))
text = elf.get_section_by_name('.text'); T0 = text['sh_addr']; TD = text.data()
md2 = Cs(CS_ARCH_ARM64, CS_MODE_ARM)
hits = []
insns = list(md2.disasm(TD, T0))
for i, ins in enumerate(insns):
    if ins.mnemonic in ('bl', 'b') and ins.op_str.startswith('#'):
        dest = int(ins.op_str[1:], 16)
        if dest in targets:
            hits.append((ins.address, dest, i))
seen = {}
for addr, dest, i in hits:
    o = owner(addr) or '?'
    seen.setdefault((o, targets[dest]), []).append((addr, i))
for (o, t), sites in sorted(seen.items()):
    print('%s  <- %s  (%d site%s: %s)' % (t[-48:], o, len(sites), '' if len(sites) == 1 else 's', ', '.join('%x' % a for a, _ in sites)))
    if disasm:
        for addr, i in sites:
            for ins in insns[max(0, i - 20):i + 1]:
                print('      %x  %-8s %s' % (ins.address, ins.mnemonic, ins.op_str))
            print()
