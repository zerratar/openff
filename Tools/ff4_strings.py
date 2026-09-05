"""Print the C strings at addresses in the FF4 binary.

    python Tools/ff4_strings.py <libff4.so> <addr> [more addrs...]

Addresses are the "= 0x..." values Tools/ff4_disasm.py prints for adrp/add pairs; each
is looked up in whatever section holds it (.rodata, .data, .data.rel.ro) and printed as
the NUL-terminated string starting there, or as the first bytes when it is not text.
"""
import sys

from elftools.elf.elffile import ELFFile

if len(sys.argv) < 3:
    print(__doc__)
    sys.exit(2)
elf = ELFFile(open(sys.argv[1], 'rb'))
sections = [(s['sh_addr'], s.data(), s.name) for s in elf.iter_sections() if s['sh_addr'] and s['sh_type'] != 'SHT_NOBITS']
for arg in sys.argv[2:]:
    address = int(arg, 16)
    for base, data, name in sections:
        if base <= address < base + len(data):
            at = address - base
            end = data.find(b'\0', at)
            chunk = data[at:end if 0 <= end - at <= 200 else at + 32]
            text = chunk.decode('ascii', 'replace') if all(32 <= b < 127 for b in chunk) else chunk.hex()
            print(f'{arg} ({name}+{at:#x}): {text}')
            break
    else:
        print(f'{arg}: not in a loaded section')
