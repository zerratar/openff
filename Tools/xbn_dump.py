"""Dump an XBN file (Square Enix's binary XML - the FF3 and FF4 menu layouts) as an indented tree.

    python Tools/xbn_dump.py <file.xbn | file.xbn.lz> [more files...]

The format, as the game reads it (XbnFile / XbnNode in the decompiled FF3 client): a header of
four int32 - id ("XBN "), version, node count, reserved - then one record of five int32 per
node - offset to the node's name (from the end of the record table), data type (0 void, 1
string, 2 decimal), data (an int, or for a string the offset to its text, likewise),
number of children, size of the family (the node and everything under it) - and the strings
after the records, Shift-JIS, NUL-terminated. Children follow their parent in order, so the
tree is rebuilt from the family sizes.
"""
import struct
import sys


def lz_decompress(data):
    if not data or data[0] not in (0x10, 0x11):
        return data
    size = data[1] | (data[2] << 8) | (data[3] << 16)
    out = bytearray()
    at = 4
    while len(out) < size and at < len(data):
        flags = data[at]; at += 1
        for bit in range(8):
            if len(out) >= size or at >= len(data):
                break
            if flags & (0x80 >> bit):
                b1 = data[at]; b2 = data[at + 1]; at += 2
                if data[0] == 0x10:
                    length = (b1 >> 4) + 3
                    disp = ((b1 & 0xF) << 8 | b2) + 1
                else:
                    ind = b1 >> 4
                    if ind == 0:
                        b3 = data[at]; at += 1
                        length = ((b1 & 0xF) << 4 | b2 >> 4) + 0x11
                        disp = ((b2 & 0xF) << 8 | b3) + 1
                    elif ind == 1:
                        b3 = data[at]; b4 = data[at + 1]; at += 2
                        length = ((b1 & 0xF) << 12 | b2 << 4 | b3 >> 4) + 0x111
                        disp = ((b3 & 0xF) << 8 | b4) + 1
                    else:
                        length = ind + 1
                        disp = ((b1 & 0xF) << 8 | b2) + 1
                for _ in range(length):
                    out.append(out[-disp])
            else:
                out.append(data[at]); at += 1
    return bytes(out)


def cstring(data, at):
    end = data.index(b'\0', at)
    return data[at:end].decode('shift_jis', 'replace')


def parse(data):
    ident, version, count, _reserved = struct.unpack_from('<4sIII', data, 0)
    nodes = []
    strings = 16 + 20 * count   # the name and string offsets count from the end of the record table
    for i in range(count):
        base = 16 + 20 * i
        off, dtype, ndata, children, family = struct.unpack_from('<iiiii', data, base)
        name = cstring(data, strings + off)
        value = cstring(data, strings + ndata) if dtype == 1 else (ndata if dtype == 2 else None)
        nodes.append({'name': name, 'type': dtype, 'value': value, 'children': children, 'family': family})
    return ident, version, nodes


def tree(nodes):
    """Yields (depth, node) walking the records in order, the family sizes giving the nesting."""
    stack = []   # (end index) of open families
    for i, n in enumerate(nodes):
        while stack and i >= stack[-1]:
            stack.pop()
        yield len(stack), n
        stack.append(i + max(1, n['family']))


def dump(path):
    data = open(path, 'rb').read()
    data = lz_decompress(data)
    ident, version, nodes = parse(data)
    print('%s: %s v%d, %d nodes' % (path, ident, version, len(nodes)))
    for depth, n in tree(nodes):
        val = '' if n['value'] is None else (' = "%s"' % n['value'] if n['type'] == 1 else ' = %d' % n['value'])
        print('  ' * depth + n['name'] + val)


if __name__ == '__main__':
    if len(sys.argv) < 2:
        print(__doc__)
        sys.exit(1)
    for p in sys.argv[1:]:
        dump(p)
