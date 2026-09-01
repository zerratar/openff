"""Check that every function call in every script resolves to a known function.

    python Tools/check_calls.py <disassembly-dir> <extracted-files-dir>

engine.call(library, id) looks the id up in a script's function table: library 2 means
global.script, anything else means the calling file's own table. If every id in every
script resolves, the function tables are being read completely - which is what makes
"nothing calls this code" a statement about the data rather than about the reader.

Reads only.
"""
import glob
import os
import re
import struct
import sys

CALLS = re.compile(r'^  [0-9A-F]{4}  ((?:flagOn|flagOff)?[Cc]allCommand) (.+)$', re.M)

# Where the library and id sit in each command's operand list. callCommand reads them
# first; the conditional forms read the flag group and bit before them, so taking
# "the first two operands" reads a flag pair as a function call.
CALL_ARGS = {
    'callCommand': (0, 1),
    'flagOnCallCommand': (2, 3),
    'flagOffCallCommand': (2, 3),
}


def number(text):
    text = text.strip()
    return int(text, 16) if text.startswith('0x') else int(text)


def function_ids(path):
    """The (id -> offset) table of one .script, or None if it is not readable."""
    data = open(path, 'rb').read()
    if data[:4] != b'MHCS':
        return None
    major, minor = struct.unpack_from('<HH', data, 4)
    if (major, minor) != (1, 1):
        return None
    table = struct.unpack_from('<I', data, 8)[0]
    if table + 4 > len(data):
        return None
    count = struct.unpack_from('<I', data, table)[0]
    out = {}
    for i in range(count):
        at = table + 4 + i * 8
        if at + 8 > len(data):
            break
        ident, offset = struct.unpack_from('<II', data, at)
        out[ident] = offset
    return out


def main(disasm_dir, files_dir):
    global_table = function_ids(os.path.join(files_dir, 'global.script'))
    if global_table is None:
        print("global.script not found or unreadable in", files_dir)
        return 1
    print("global.script:", len(global_table), "functions")

    total = 0
    unresolved = []
    by_library = {}
    for listing in sorted(glob.glob(os.path.join(disasm_dir, '*.txt'))):
        name = os.path.basename(listing)[:-4]
        local = function_ids(os.path.join(files_dir, name + '.script')) or {}
        text = open(listing, encoding='utf-8').read()

        for command, operands in CALLS.findall(text):
            first, second = CALL_ARGS[command]
            parts = operands.split(';')[0].split()
            if len(parts) <= second:
                continue
            library, ident = number(parts[first]), number(parts[second])
            total += 1
            by_library[library] = by_library.get(library, 0) + 1
            table = global_table if library == 2 else local
            if ident not in table:
                unresolved.append((name, library, ident))

    print("calls found:", total, " by library:", by_library)
    print("calls whose id is not in the table they would look in:", len(unresolved))
    for name, library, ident in unresolved[:20]:
        print("   %-24s library %d  id %08X" % (name, library, ident))
    if len(unresolved) > 20:
        print("   ...", len(unresolved) - 20, "more")

    # And the other direction: is every function in a table actually reached?
    print()
    orphan_functions = 0
    for listing in sorted(glob.glob(os.path.join(disasm_dir, '*.txt'))):
        name = os.path.basename(listing)[:-4]
        local = function_ids(os.path.join(files_dir, name + '.script')) or {}
        text = open(listing, encoding='utf-8').read()
        for ident, offset in local.items():
            if ('func_%d:' % ident) not in text:
                orphan_functions += 1
    print("functions in a table that the walk did not label:", orphan_functions)
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1], sys.argv[2]))
