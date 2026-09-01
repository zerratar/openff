"""Check the generated .pak record layouts against the shipped data.

    python Tools/check_records.py <extracted-files-dir>

A chain is an array of fixed size records, so its byte length has to divide exactly by
the record size the layout implies. If it does not, the layout is wrong and every
record after the first would be read from the wrong offset. This is the same idea as
the disassembler's overlap check: a cheap invariant that a wrong guess cannot satisfy
by accident.

Reads only.
"""
import collections
import glob
import os
import re
import struct
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)
RECORDS = os.path.join(ROOT, 'FF3.ContentTool', 'PakRecords.cs')


def layouts():
    """family -> {chain index: (label, stride, variable)} from the generated table."""
    text = open(RECORDS, encoding='utf-8-sig').read()
    out = collections.defaultdict(dict)
    for mm in re.finditer(r'new PakChain\("(\w+)", (\d+), "(\w+)", "([\w.]+)", (\d+), new'
                          r'(.*?)\n\t\t\t\}\),', text, re.S):
        family, index, label, source, stride, fields = mm.groups()
        out[family][int(index)] = (label, int(stride), ', -1)' in fields)
    return out


def chains(path):
    """(offset, size) per chain of one .pak, or None if it does not look like one."""
    data = open(path, 'rb').read()
    if len(data) < 16:
        return None
    count = struct.unpack_from('<I', data, 0)[0]
    relocated = struct.unpack_from('<I', data, 8)[0]
    if relocated != 0 or count == 0 or count > 64:
        return None
    if 16 + count * 8 > len(data):
        return None
    out = []
    for i in range(count):
        offset, size = struct.unpack_from('<II', data, 16 + i * 8)
        if offset > len(data) or offset + size > len(data):
            return None
        out.append((offset, size))
    return out


def check(path, family, table):
    found = chains(path)
    if found is None:
        return None
    problems = []
    for index, (offset, size) in enumerate(found):
        if index not in table:
            continue                                # chain the game reads raw
        label, stride, variable = table[index]
        if variable:
            if size < stride:
                problems.append('%s: %d bytes, less than the %d byte head'
                                % (label, size, stride))
            continue
        if stride and size % stride:
            problems.append('%s: %d bytes is not a multiple of %d'
                            % (label, size, stride))
    return problems


def main(files_dir):
    table = layouts()
    print("layouts:", {f: len(c) for f, c in table.items()})
    print()

    total = collections.Counter()
    failures = []
    unmodelled = []

    for path in sorted(glob.glob(os.path.join(files_dir, '*.pak'))
                       + glob.glob(os.path.join(files_dir, '*.chaindata'))):
        name = os.path.basename(path)
        found = chains(path)
        if found is None:
            total['not a pak'] += 1
            continue

        # Families this script does not model yet, named rather than lumped in with
        # the map paks and reported as failures:
        #   *_world_move_parameter.pak       CPlayerWorldParameterManager and its NPC twin
        #   s01_01.pak                       5 chains, the older layout - its .script is
        #                                    version 1.0 too, which the game refuses to load
        if name == 'item_parameter.pak':
            family = 'Item'
        elif name == 'monster.chaindata':
            family = 'Monster'
        elif name == 'player.chaindata':
            family = 'Player'
        elif name.endswith('_world_move_parameter.pak'):
            total['not modelled'] += 1
            continue
        elif len(found) != 7:
            total['not the 7 chain map layout'] += 1
            unmodelled.append((name, len(found)))
            continue
        else:
            family = 'Map'

        problems = check(path, family, table[family])
        total[family] += 1
        if problems:
            failures.append((name, family, problems))

    print("files checked:", dict(total))
    for name, count in unmodelled:
        print("   %-28s %d chains, not the map layout" % (name, count))
    print("files whose chains do not divide by the record size:", len(failures))
    for name, family, problems in failures[:15]:
        print("   %-28s %-8s %s" % (name, family, problems[0]))
    if len(failures) > 15:
        print("   ...", len(failures) - 15, "more")
    return 1 if failures else 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1]))
