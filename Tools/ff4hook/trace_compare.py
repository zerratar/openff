"""Set the Steam FF4.exe's script trace beside OpenFF's - which commands each ran, in order, with their operands.

    python Tools/ff4hook/trace_compare.py <steam trace.tsv> <openff trace.tsv> [--all] [--waits] [--from NAME] [--context N]
    python Tools/ff4hook/trace_compare.py --decode <trace.tsv> [--all]

The traces: Tools/ff4hook (ff4steam.py's 'trace' step) and OpenFF's --script-trace=<file>; both write a line a command
- frame, engine, command index, position, 48 operand bytes in hex - and a wait redone frame after frame once.
Commands are named and their operands read with OpenFF's own FF4 table (Shared/Script/ScriptOpsFf4.cs), the same
table FF4.exe calls through. By default only the scene commands (ce_*) are compared - the story's shape; --all
compares everything. The waits (they poll frame after frame) are left out unless --waits. The output is a diff: '  ' both ran it alike, '- ' only Steam, '+ ' only OpenFF, '~ ' both, the
operands differing.
"""
import os, re, sys, difflib

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))


def load_table():
    text = open(os.path.join(ROOT, "Shared", "Script", "ScriptOpsFf4.cs"), encoding="utf-8").read()
    ops = []
    for m in re.finditer(r'new ScriptOp\("([^"]+)",\s*(None|new\[\]\s*\{([^}]*)\})', text):
        kinds = re.findall(r"Operand\.(\w+)", m.group(3) or "")
        ops.append((m.group(1).replace("babilCommand_", ""), kinds))
    return ops


def decode(hexbytes, kinds):
    b = bytes.fromhex(hexbytes)
    out, at = [], 0
    for k in kinds:
        if k == "Byte":
            out.append(b[at] if at < len(b) else None); at += 1
        elif k == "Word":
            out.append(int.from_bytes(b[at:at + 2], "little")); at += 2
        elif k == "Dword":
            v = int.from_bytes(b[at:at + 4], "little")
            out.append(v - (1 << 32) if v >= 1 << 31 else v); at += 4
        elif k == "String":
            end = b.find(b"\0", at)
            if end < 0:
                out.append(b[at:].decode("latin-1") + "..."); at = len(b)
            else:
                out.append(b[at:end].decode("latin-1")); at = end + 1
    return out


def read(path, table, everything, keep_waits=False):
    rows = []
    for line in open(path, encoding="utf-8", errors="replace"):
        parts = line.rstrip("\n").split("\t")
        if len(parts) < 5:
            continue
        frame, engine, index, pc, hexbytes = parts[:5]
        index = int(index)
        name, kinds = table[index] if index < len(table) else ("op%d" % index, [])
        if not everything and not name.startswith("CE_"):
            continue
        # waits poll frame after frame, interleaved with the other scripts' - the timing, not the story
        if not keep_waits and ("Wait" in name or name in ("CE_setFrameWait",)):
            continue
        rows.append((int(frame), engine, name, int(pc), decode(hexbytes, kinds)))
    return rows


def show(row):
    frame, engine, name, pc, ops = row
    return "%6d  %-34s %s" % (frame, name, ", ".join(repr(o) for o in ops))


def main():
    args = sys.argv[1:]
    everything = "--all" in args
    waits = "--waits" in args
    args = [a for a in args if a not in ("--all", "--waits")]
    start = None
    if "--from" in args:
        i = args.index("--from"); start = args[i + 1]; del args[i:i + 2]
    context = 3
    if "--context" in args:
        i = args.index("--context"); context = int(args[i + 1]); del args[i:i + 2]
    table = load_table()
    if len(table) != 500:
        print("warning: the FF4 table has %d commands, not 500" % len(table))
    if args and args[0] == "--decode":
        for row in read(args[1], table, everything, waits):
            print(show(row))
        return
    if len(args) != 2:
        sys.exit(__doc__)
    steam, ours = read(args[0], table, everything), read(args[1], table, everything, waits)
    if start:
        steam = steam[next((i for i, r in enumerate(steam) if r[2] == start), 0):]
        ours = ours[next((i for i, r in enumerate(ours) if r[2] == start), 0):]
    key = lambda r: (r[2], tuple(r[4]))
    a, b = [key(r) for r in steam], [key(r) for r in ours]
    sm = difflib.SequenceMatcher(None, a, b, autojunk=False)
    print("steam: %d commands, openff: %d; %.0f%% alike" % (len(a), len(b), 100 * sm.ratio()))
    for tag, i1, i2, j1, j2 in sm.get_opcodes():
        if tag == "equal":
            n = i2 - i1
            if n > 2 * context:
                for r in steam[i1:i1 + context]: print("  " + show(r))
                print("  ... %d alike ..." % (n - 2 * context))
                for r in steam[i2 - context:i2]: print("  " + show(r))
            else:
                for r in steam[i1:i2]: print("  " + show(r))
            continue
        # a run where the names line up but the operands do not reads better as pairs
        if tag == "replace" and [r[2] for r in steam[i1:i2]] == [r[2] for r in ours[j1:j2]]:
            for s, o in zip(steam[i1:i2], ours[j1:j2]):
                print("~ " + show(s)); print("~ " + " " * 41 + "openff: " + ", ".join(repr(x) for x in o[4]))
            continue
        for r in steam[i1:i2]: print("- " + show(r))
        for r in ours[j1:j2]: print("+ " + show(r))


if __name__ == "__main__":
    main()
