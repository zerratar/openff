"""The characters of the Steam FF4.exe beside OpenFF's, shot by shot - who is up, hidden or shown, where, playing
which motion.

    python Tools/ff4hook/chars_compare.py <steam dir> <openff dir> [--after FRAMES] [--only-diff]
    python Tools/ff4hook/chars_compare.py <steam dir> <openff dir> --time [--every SECONDS]

Each dir holds a run's chars.tsv and trace.tsv (Tools/ff4hook's 'chars' and 'trace'; OpenFF's --chars-trace and
--script-trace). By default the runs are compared shot by shot: <FRAMES> (10) after each camera cut
(CE_PlayCameraMotion), the k-th cut of each run with the same camera motion - wherever the two are in time, since a
message's wait ends when a key comes and the runs drift apart. --time compares them by the time since each run's
first CE_StartEvent instead. Characters are matched by model, in slot order (a scene makes its cast in the script's
order in both); OpenFF keeps the field's own characters (hidden) through a scene, which Steam clears - those show as
"only in openff (hidden)".
"""
import os, sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from trace_compare import load_table

COLS = ("frame", "slot", "hidden", "x", "y", "z", "rx", "ry", "rz", "motion", "mframe", "alpha", "transparency")


def load_chars(path):
    by_frame = {}
    for line in open(path, encoding="utf-8", errors="replace"):
        p = line.rstrip("\n").split("\t")
        if len(p) < 13:
            continue
        row = dict(zip(COLS, p))
        row["name"] = p[13].replace(" (loading)", "") if len(p) > 13 else ""
        for k in ("frame", "slot", "hidden", "motion"):
            row[k] = int(row[k])
        for k in ("x", "y", "z"):
            row[k] = float(row[k])
        by_frame.setdefault(row["frame"], []).append(row)
    return by_frame


def load_trace(path):
    rows = []
    for line in open(path, encoding="utf-8", errors="replace"):
        p = line.rstrip("\n").split("\t")
        if len(p) >= 5:
            rows.append((int(p[0]), int(p[2]), p[4]))
    return rows


def describe(r):
    if r is None:
        return "-"
    return "%s%-9d (%7.1f %6.1f %7.1f) %s" % ("H " if r["hidden"] else "  ", r["motion"], r["x"], r["y"], r["z"], r["name"])


def at(chars, frames, want):
    f = min(frames, key=lambda x: abs(x - want)) if frames else None
    return chars.get(f, []) if f is not None and abs(f - want) <= 30 else []


def compare(steam, ours, label, only_diff):
    lines = []
    left = sorted(ours, key=lambda r: r["slot"])
    for s in sorted(steam, key=lambda r: r["slot"]):
        match = next((o for o in left if o["name"] == s["name"]), None) if s["name"] else None
        if match is None and not s["name"] and left:
            # Steam's characters set up asynchronously keep no name where the dump reads it: the nearest one, then
            # the one with the same motion, of those OpenFF has that Steam's named ones have not taken
            named = {r["name"] for r in steam if r["name"]}
            pool = [o for o in left if o["name"] not in named] or left
            match = min(pool, key=lambda o: (sum((s[k] - o[k]) ** 2 for k in "xyz"), o["motion"] != s["motion"]))
        if match is not None:
            left.remove(match)
        flag = ""
        if match is None:
            flag = "  <- missing in openff"
        else:
            if match["hidden"] != s["hidden"]:
                flag += "  <- hidden differs"
            if not s["hidden"] and match["motion"] != s["motion"]:
                flag += "  <- motion differs"
            if not s["hidden"] and max(abs(s[k] - match[k]) for k in "xyz") > 2:
                flag += "  <- place differs"
        if flag or not only_diff:
            lines.append("  %-50s %-50s%s" % (describe(s), describe(match), flag))
    for o in left:
        if not o["hidden"] or not only_diff:
            lines.append("  %-50s %-50s  <- only in openff%s" % ("-", describe(o), " (hidden)" if o["hidden"] else ""))
    if lines or not only_diff:
        print("== %s  (steam %d, openff %d up)" % (label, len(steam), len(ours)))
        print("\n".join(lines))


def main():
    args = sys.argv[1:]

    def opt(name, default):
        if name in args:
            i = args.index(name); v = float(args[i + 1]); del args[i:i + 2]; return v
        return default

    def flag(name):
        if name in args:
            args.remove(name); return True
        return False

    by_time, only_diff = flag("--time"), flag("--only-diff")
    after, every = int(opt("--after", 10)), opt("--every", 5.0)
    if len(args) != 2:
        sys.exit(__doc__)
    table = [n for n, _ in load_table()]
    runs = []
    for d in args:
        chars = load_chars(os.path.join(d, "chars.tsv"))
        runs.append((chars, sorted(chars), load_trace(os.path.join(d, "trace.tsv"))))
    (sc, sf, st), (oc, of, ot) = runs

    if by_time:
        start_event = table.index("CE_StartEvent")
        z = [next(f for f, i, _ in tr if i == start_event) for tr in (st, ot)]
        t = 0.0
        while True:
            s, o = at(sc, sf, z[0] + t * 30), at(oc, of, z[1] + t * 30)
            if not s and not o and t > 60:
                break
            compare(s, o, "%.0f s" % t, only_diff)
            t += every
        return

    cam = table.index("CE_PlayCameraMotion")

    def cuts(trace):
        out = []
        for f, i, hexbytes in trace:
            if i != cam:
                continue
            motion = int.from_bytes(bytes.fromhex(hexbytes[8:16]), "little")   # (slot dword, motion dword, ...)
            if not out or out[-1][1] != motion or f - out[-1][0] > 5:
                out.append((f, motion))
        return out

    steam_cuts, our_cuts = cuts(st), cuts(ot)
    j = 0
    for k, (f, motion) in enumerate(steam_cuts):
        jj = j
        while jj < len(our_cuts) and our_cuts[jj][1] != motion:
            jj += 1
        if jj >= len(our_cuts):
            print("== cut %d, camera %d: not in openff" % (k, motion))
            continue
        j = jj + 1
        compare(at(sc, sf, f + after), at(oc, of, our_cuts[jj][0] + after), "cut %d, camera %d" % (k, motion), only_diff)


if __name__ == "__main__":
    main()
