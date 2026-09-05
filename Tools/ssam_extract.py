"""Extract the entries of an FF4 SSAM mass file to a directory.

    python Tools/ssam_extract.py <file.dat> <out dir>

The container: 'SSAM' | count, then 40-byte records offset | size | name[32]; data offsets
are relative to the end of the directory (8 + 40 * count). Entries whose name ends in .lz
are LZ-compressed; they are written as they are (the editor's `lz` command decompresses them).
"""
import os
import struct
import sys

if len(sys.argv) < 3:
    print(__doc__)
    sys.exit(2)

path, out = sys.argv[1], sys.argv[2]
data = open(path, 'rb').read()
if data[:4] != b'SSAM':
    print('not an SSAM file:', path)
    sys.exit(1)
count = struct.unpack_from('<I', data, 4)[0]
directory_end = 8 + 40 * count
os.makedirs(out, exist_ok=True)
written = 0
for i in range(count):
    base = 8 + i * 40
    offset, size = struct.unpack_from('<II', data, base)
    raw = data[base + 8:base + 40]
    name = raw.split(bytes([0]), 1)[0].decode('ascii', 'replace')
    start = directory_end + offset
    if not name or size == 0 or start + size > len(data):
        continue
    with open(os.path.join(out, name), 'wb') as f:
        f.write(data[start:start + size])
    written += 1
print(f'{written} of {count} entries -> {out}')
