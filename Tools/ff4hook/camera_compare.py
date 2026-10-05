"""The camera of the Steam FF4.exe beside OpenFF's, shot by shot.

    python Tools/ff4hook/camera_compare.py <steam dir> <openff dir> [--after FRAMES]

Each dir holds a run's camera.tsv and trace.tsv (Tools/ff4hook's 'camera' and 'trace'; OpenFF's --camera-trace and
--script-trace). For each camera cut (CE_PlayCameraMotion) of the Steam run and the same cut of OpenFF's, the camera
<FRAMES> (10) into it and halfway through it: position, target, the up vector and the projection's vertical scale
(1 / tan(fovY / 2) - the field of view); a shot is flagged when OpenFF's camera stands a unit or more away, looks
2 degrees or more another way, or sees a degree more or less.
"""
import math, os, sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from trace_compare import load_table
from chars_compare import load_trace


def load_camera(path):
    rows = {}
    for line in open(path, encoding="utf-8", errors="replace"):
        p = line.rstrip("\n").split("\t")
        if len(p) >= 12:
            rows[int(p[0])] = [float(x) for x in p[1:12]]
    return rows, sorted(rows)


def at(cam, frames, want):
    f = min(frames, key=lambda x: abs(x - want)) if frames else None
    return cam[f] if f is not None and abs(f - want) <= 15 else None


def cuts(trace, cam_op):
    out = []
    for f, i, hexbytes in trace:
        if i != cam_op:
            continue
        motion = int.from_bytes(bytes.fromhex(hexbytes[8:16]), "little")
        if not out or out[-1][1] != motion or f - out[-1][0] > 5:
            out.append((f, motion))
    return out


def angle(a, b):
    la, lb = math.hypot(*a), math.hypot(*b)
    if not la or not lb:
        return 0.0
    return math.degrees(math.acos(max(-1.0, min(1.0, sum(x * y for x, y in zip(a, b)) / (la * lb)))))


def fov(p11):
    return math.degrees(2 * math.atan(1 / p11)) if p11 else 0.0


def show(c):
    return "pos %8.1f %6.1f %8.1f  look %8.1f %6.1f %8.1f  fov %5.1f" % (c[0], c[1], c[2], c[3], c[4], c[5], fov(c[10]))


def main():
    args = sys.argv[1:]
    after = 10
    if "--after" in args:
        i = args.index("--after"); after = int(args[i + 1]); del args[i:i + 2]
    if len(args) != 2:
        sys.exit(__doc__)
    table = [n for n, _ in load_table()]
    cam_op = table.index("CE_PlayCameraMotion")
    (sc, sf), (oc, of) = (load_camera(os.path.join(d, "camera.tsv")) for d in args)
    s_cuts, o_cuts = (cuts(load_trace(os.path.join(d, "trace.tsv")), cam_op) for d in args)
    j = 0
    for k, (f, motion) in enumerate(s_cuts):
        jj = j
        while jj < len(o_cuts) and o_cuts[jj][1] != motion:
            jj += 1
        if jj >= len(o_cuts):
            print("cut %2d camera %5d: not in openff" % (k, motion))
            continue
        j = jj + 1
        g = o_cuts[jj][0]
        s_len = (s_cuts[k + 1][0] - f) if k + 1 < len(s_cuts) else 60
        for label, ds in (("start", after), ("mid", max(after, s_len // 2))):
            s, o = at(sc, sf, f + ds), at(oc, of, g + ds)
            if not s or not o:
                continue
            dpos = math.dist(s[0:3], o[0:3])
            # the target's distance is the game's own business (Steam keeps it a unit ahead): the direction counts
            dang = angle([b - a for a, b in zip(s[0:3], s[3:6])], [b - a for a, b in zip(o[0:3], o[3:6])])
            dfov = fov(o[10]) - fov(s[10])
            flag = "" if dpos < 1 and dang < 2 and abs(dfov) < 1 else "   <- off by %.1f units, %.1f degrees, fov %+.1f" % (dpos, dang, dfov)
            print("cut %2d camera %5d %-5s steam  %s" % (k, motion, label, show(s)))
            print("%25s openff %s%s" % ("", show(o), flag))


if __name__ == "__main__":
    main()
