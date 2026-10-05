"""python Tools/ghidra/match_builds.py <named facts.tsv> <nameless facts.tsv> <names.tsv>

The Steam FF4.exe has no symbols; the Android libff4.so keeps its C++ names. Both are the same game compiled twice, so a
function of one is very often the function of the other. ExportDecomp.java writes each build's facts (name, size, the
strings each function refers to, the functions it calls); this pairs them:

1. anchors - a string only one function uses in each build ties those two, when they are each other's best match
   (most such strings shared); a run of the same strings (a set used by one function each side) does too;
2. the call graph - around a matched pair, its callees and callers still unmatched: alone on both sides they pair,
   and in the same number they pair in order, when their sizes are within 3x; repeated until nothing new is found.

names.tsv: the nameless build's address, the name from the named build, how it was found, the named build's address.
ApplyNames.java puts them on the nameless program. A wrong pair is possible - the way column says how much to trust it.
"""
import sys
from collections import defaultdict


def load(path):
    funcs = {}
    for line in open(path, encoding="utf-8"):
        parts = line.rstrip("\n").split("\t")
        if len(parts) < 5:
            continue
        addr, name, size, strings, callees = parts[:5]
        funcs[addr] = {
            "name": name,
            "size": int(size or 0),
            "strings": set(s for s in strings.split("\x1f") if s),
            "callees": [c for c in callees.split(",") if c],
        }
    callers = defaultdict(list)
    for a, f in funcs.items():
        for c in f["callees"]:
            if c in funcs:
                callers[c].append(a)
    for a, f in funcs.items():
        f["callers"] = callers.get(a, [])
    return funcs


def anchors(so, exe):
    by_string_so, by_string_exe = defaultdict(set), defaultdict(set)
    for a, f in so.items():
        for s in f["strings"]:
            by_string_so[s].add(a)
    for a, f in exe.items():
        for s in f["strings"]:
            by_string_exe[s].add(a)
    # Strings used by exactly one function on each side: votes for that pair.
    votes = defaultdict(int)
    for s, owners in by_string_so.items():
        other = by_string_exe.get(s)
        if len(owners) == 1 and other and len(other) == 1:
            votes[(next(iter(owners)), next(iter(other)))] += 1
    best_so, best_exe = {}, {}
    for (a, b), n in votes.items():
        if n > best_so.get(a, (0, None))[0]:
            best_so[a] = (n, b)
        if n > best_exe.get(b, (0, None))[0]:
            best_exe[b] = (n, a)
    pairs = {}
    for a, (n, b) in best_so.items():
        if best_exe.get(b, (0, None))[1] == a:
            pairs[a] = (b, "string x%d" % n)
    # A whole set of strings that only one function has on each side.
    sig_so, sig_exe = defaultdict(list), defaultdict(list)
    for a, f in so.items():
        if len(f["strings"]) >= 2:
            sig_so[frozenset(f["strings"])].append(a)
    for a, f in exe.items():
        if len(f["strings"]) >= 2:
            sig_exe[frozenset(f["strings"])].append(a)
    used = {b for b, _ in pairs.values()}
    for sig, owners in sig_so.items():
        other = sig_exe.get(sig)
        if len(owners) == 1 and other and len(other) == 1 and owners[0] not in pairs and other[0] not in used:
            pairs[owners[0]] = (other[0], "string set")
            used.add(other[0])
    return pairs


def propagate(so, exe, pairs):
    taken = {b for b, _ in pairs.values()}
    changed = True
    rounds = 0
    while changed:
        changed = False
        rounds += 1
        for a, (b, _) in list(pairs.items()):
            for kind in ("callees", "callers"):
                left = [x for x in dict.fromkeys(so[a][kind]) if x in so and x not in pairs]
                right = [y for y in dict.fromkeys(exe[b][kind]) if y in exe and y not in taken]
                if not left or not right:
                    continue
                if len(left) == 1 and len(right) == 1:
                    found = [(left[0], right[0], kind[:-1] + " (alone)")]
                elif len(left) == len(right) and kind == "callees":
                    found = [(x, y, "callee (in order)") for x, y in zip(left, right)]
                else:
                    continue
                for x, y, how in found:
                    # The same function compiled twice is about the same size (x86 against arm64: 0.84x at the
                    # string anchors' median); a pair more than 3x apart is a wrong one, and would spread.
                    sx, sy = so[x]["size"], exe[y]["size"]
                    if sx and sy and not (1 / 3 <= sy / sx <= 3):
                        continue
                    if x not in pairs and y not in taken:
                        pairs[x] = (y, how)
                        taken.add(y)
                        changed = True
    return rounds


def main():
    if len(sys.argv) != 4:
        sys.exit(__doc__)
    so, exe = load(sys.argv[1]), load(sys.argv[2])
    pairs = anchors(so, exe)
    anchored = len(pairs)
    rounds = propagate(so, exe, pairs)
    named = [(b, so[a]["name"], how, a) for a, (b, how) in pairs.items() if not so[a]["name"].startswith("FUN_")]
    with open(sys.argv[3], "w", encoding="utf-8", newline="\n") as out:
        for b, name, how, a in sorted(named):
            out.write("%s\t%s\t%s\t%s\n" % (b, name, how, a))
    ways = defaultdict(int)
    for _, _, how, _ in named:
        ways[how.split(" x")[0]] += 1
    print("named build: %d functions, nameless: %d" % (len(so), len(exe)))
    print("anchored by strings: %d; after %d rounds of the call graph: %d pairs, %d named" % (anchored, rounds, len(pairs), len(named)))
    for how, n in sorted(ways.items(), key=lambda kv: -kv[1]):
        print("  %-20s %d" % (how, n))
    print("nameless functions named: %.1f%%" % (100.0 * len(named) / max(1, len(exe))))


if __name__ == "__main__":
    main()
