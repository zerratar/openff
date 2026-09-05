"""Who calls a function in libff4.so - the callers, by symbol, with the call sites.

    python Tools/ff4_callers.py <libff4.so> <symbol substring> [more...] [--disasm]

Scans .text for `bl`/`b` to every function whose mangled name contains a pattern and
prints the enclosing functions (the symbol table's sizes bound them). With --disasm the
twenty instructions before each call site are printed too - where the arguments come from.
"""
import sys, re, bisect
from elftools.elf.elffile import ELFFile
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
targets = {a: n for a, (n, _) in funcs.items() if any(re.search(p, n) for p in pats)}
for a, n in sorted(targets.items()): print('target %x %s' % (a, n))
text = elf.get_section_by_name('.text'); T0 = text['sh_addr']; TD = text.data()
md = Cs(CS_ARCH_ARM64, CS_MODE_ARM)
hits = []
insns = list(md.disasm(TD, T0))
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
    print('%s  <- %s  (%d site%s: %s)' % (t.split('E')[0][-40:], o, len(sites), '' if len(sites) == 1 else 's', ', '.join('%x' % a for a, _ in sites)))
    if disasm:
        for addr, i in sites:
            for ins in insns[max(0, i - 20):i + 1]:
                print('      %x  %-8s %s' % (ins.address, ins.mnemonic, ins.op_str))
            print()
