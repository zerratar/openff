"""When each scene command runs in the Steam FF4.exe and in OpenFF, counted from the scene's start.

    python Tools/ff4hook/timing_compare.py <steam dir> <openff dir> [--all] [--show N]

The two runs' traces (trace.tsv) are lined up command by command (trace_compare's matching, waits left out); each
command both ran is given its frame since the scene began (the first CE_StartEvent before it) in each, and the
difference. A command OpenFF runs late or early shows as the place the two drift apart - and every command after
it carries the drift until something (a key, a wait for a fixed frame) brings them back together.
"""
import os, sys, difflib
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from trace_compare import load_table, read


def with_scene_frames(rows):
    out, start = [], None
    for r in rows:
        if r[2] == "CE_StartEvent":
            start = r[0]
        out.append((r, r[0] - start if start is not None else None))
    return out


def main():
    args = sys.argv[1:]
    everything = "--all" in args
    args = [a for a in args if a != "--all"]
    show = 0
    if "--show" in args:
        i = args.index("--show"); show = int(args[i + 1]); del args[i:i + 2]
    table = load_table()
    s, o = (with_scene_frames(read(os.path.join(d, "trace.tsv"), table, everything)) for d in args)
    key = lambda x: (x[0][2], tuple(x[0][4]))
    sm = difflib.SequenceMatcher(None, [key(x) for x in s], [key(x) for x in o], autojunk=False)
    last, shown = None, 0
    for tag, i1, i2, j1, j2 in sm.get_opcodes():
        if tag != "equal":
            continue
        for a, b in zip(s[i1:i2], o[j1:j2]):
            if a[1] is None or b[1] is None:
                continue
            d = b[1] - a[1]
            if d != last or shown < show:
                print("%-30s %-40s steam %5d  openff %5d  %+d" % (a[0][2], ", ".join(repr(v) for v in a[0][4])[:40], a[1], b[1], d))
                last = d
                shown += 1


if __name__ == "__main__":
    main()
