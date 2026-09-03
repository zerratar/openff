"""Disassemble named functions in libff4.so and print what matters for a record layout:
loads/stores with immediate offsets, multiplies/shifts (strides), immediates, calls."""
import sys, re
from elftools.elf.elffile import ELFFile
from elftools.elf.relocation import RelocationSection
from capstone import Cs, CS_ARCH_ARM64, CS_MODE_ARM
SO = sys.argv[1]; pats = sys.argv[2:]
elf = ELFFile(open(SO, 'rb')); funcs = {}; by_addr = {}
for sec in elf.iter_sections():
    if sec['sh_type'] in ('SHT_SYMTAB', 'SHT_DYNSYM'):
        for s in sec.iter_symbols():
            if s['st_value'] and s.name and s['st_info']['type'] == 'STT_FUNC':
                funcs[s.name] = (s['st_value'], s['st_size']); by_addr[s['st_value']] = s.name
dn = [s.name for s in elf.get_section_by_name('.dynsym').iter_symbols()]; got = {}
for sec in elf.iter_sections():
    if isinstance(sec, RelocationSection):
        for r in sec.iter_relocations():
            if r['r_info_sym']: got[r['r_offset']] = dn[r['r_info_sym']]
md = Cs(CS_ARCH_ARM64, CS_MODE_ARM); md.detail = True
plt = elf.get_section_by_name('.plt'); page = stub = None
for ins in md.disasm(plt.data(), plt['sh_addr']):
    if ins.mnemonic == 'adrp': page = ins.operands[1].imm; stub = ins.address
    elif ins.mnemonic == 'ldr' and page is not None and ins.operands[1].type == 3:
        by_addr[stub] = got.get(page + ins.operands[1].mem.disp, '?'); page = None
text = elf.get_section_by_name('.text'); T0 = text['sh_addr']; TD = text.data()
def short(n):
    m = re.match(r'_ZN?K?(\d+)', n)
    return n
for name in sorted(funcs):
    if not any(re.search(p, name) for p in pats): continue
    a, sz = funcs[name]; body = TD[a - T0:a - T0 + sz]
    print('==', name, '(%d instr)' % (sz // 4))
    for ins in md.disasm(body, a):
        m, o = ins.mnemonic, ins.op_str
        keep = m.startswith(('ldr', 'ldur', 'str', 'stur', 'ldp', 'stp')) and '[x' in o and 'sp' not in o
        keep = keep or m in ('bl', 'b') or m.startswith(('mul', 'madd', 'smaddl', 'umaddl', 'lsl', 'add', 'sub', 'mov', 'sxt', 'uxt', 'cmp', 'ret', 'cset', 'and', 'ubfx', 'sbfx', 'tbz', 'tbnz', 'cbz', 'cbnz', 'b.'))
        if not keep: continue
        if m in ('bl', 'b'):
            tgt = ins.operands[0].imm if ins.operands and ins.operands[0].type == 2 else None
            o = by_addr.get(tgt, o) if tgt else o
        print('   %x  %-7s %s' % (ins.address, m, o))
