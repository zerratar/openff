"""OpenFF's own layout table for FF3 Steam's 2D art.

    python Tools/gen_steam_cells.py <extracted archive files/> <Steam FF3 install>

The Steam build of FF3 authored its 2D sheets at other pixel sizes and rebuilt its UI
for a widescreen desktop, so its cell banks (.NCER) place and size things for screens
this port does not have. The port's screens are the phone's, and they need the phone's
cell geometry over Steam's higher-resolution sheets.

Both builds keep the same cells in the same order (Steam only appends to a few banks),
and each Steam OAM's sheet rectangle covers the same picture as the phone's, at Steam's
pixel size. So the table holds only the phone's placement - x, y, w, h and flags per OAM
as the port draws them - and at runtime SteamCells keeps the sheet rectangle the Steam
file gave that OAM as the source, drawn at the phone's size. Sheet pixel sizes never
enter into it, which matters because the runtime texture is padded to a power of two.

This writes FF3.Game/Data/ff3-steam-cells.json for every cell bank both builds ship
whose cells differ. Banks that are identical in both builds, or that Steam does not
ship, need nothing and are left out.

The numbers are layout metadata, not art: a few hundred positions. They are what makes
a Steam install playable in this client without shipping any of Square's assets.
"""
import json
import os
import struct
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
DEST = os.path.join(os.path.dirname(HERE), 'FF3.Game', 'Data', 'ff3-steam-cells.json')


def cells(path):
    d = open(path, 'rb').read()
    if d[:4] != b'RECN':
        return None
    n = struct.unpack_from('<H', d, 0x18)[0]
    ofs = struct.unpack_from('<I', d, 0x1c)[0]
    base = 0x18 + ofs
    out = []
    for i in range(n):
        num, attr, off = struct.unpack_from('<HHI', d, base + 8 * i)
        ob = base + 8 * n + off
        out.append((attr, [list(struct.unpack_from('<7h', d, ob + 14 * k)) for k in range(num)]))
    return out


def png_size(path):
    if not os.path.exists(path):
        return None
    with open(path, 'rb') as f:
        head = f.read(24)
    return struct.unpack('>II', head[16:24]) if head[:4] == b'\x89PNG' else None


def sheet_for(directory, stem):
    # title_gousei_new_jp.NCER draws title_gousei_new.NCBR; a language suffix is not a sheet.
    for candidate in (stem, stem[:-3] if stem.endswith('_jp') else None):
        if not candidate:
            continue
        for ext in ('.NCGR', '.NCBR'):
            size = png_size(os.path.join(directory, candidate + ext))
            if size:
                return size
    return None


def main(ours, steam):
    steam_files = os.path.join(steam, 'files') if os.path.isdir(os.path.join(steam, 'files')) else steam
    table = {}
    skipped = []
    for name in sorted(os.listdir(steam_files)):
        if not name.lower().endswith('.ncer'):
            continue
        a, b = os.path.join(ours, name), os.path.join(steam_files, name)
        if not os.path.exists(a):
            continue
        if open(a, 'rb').read() == open(b, 'rb').read():
            continue
        ca, cb = cells(a), cells(b)
        if ca is None or cb is None:
            skipped.append((name, 'not a cell bank'))
            continue
        if len(cb) < len(ca) or any(len(sb[1]) != len(sa[1]) for sa, sb in zip(ca, cb)):
            skipped.append((name, 'cells are not in the same order in both builds'))
            continue
        # Phone placement only: x, y, w, h, flags. The sheet rectangle stays Steam's.
        table[name] = {'cells': [[[x, y, w, h, flags] for x, y, w, h, _u, _v, flags in oams] for _attr, oams in ca],
                       'steamCells': len(cb)}
    os.makedirs(os.path.dirname(DEST), exist_ok=True)
    with open(DEST, 'w', encoding='utf-8') as f:
        json.dump({'game': 'ff3', 'source': 'phone cell placement over Steam sheet rectangles', 'banks': table}, f, separators=(',', ':'))
    print('wrote', os.path.relpath(DEST, os.path.dirname(HERE)), '-', len(table), 'cell banks', os.path.getsize(DEST), 'bytes')
    for s in skipped:
        print('  skipped', s)


if __name__ == '__main__':
    if len(sys.argv) < 3:
        print(__doc__)
        sys.exit(2)
    main(sys.argv[1], sys.argv[2])
