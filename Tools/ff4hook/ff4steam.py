"""Drive the Steam FF4.exe through ff4hook (Tools/ff4hook/ff4hook.c) - keys and screenshots from a script, nothing
sent to the desktop. For comparing OpenFF's FF4 with the real game.

    python Tools/ff4hook/ff4steam.py run <script> <out dir>   install, start FF4.exe, play the script, quit, uninstall
    python Tools/ff4hook/ff4steam.py install | uninstall | send <command...>

install puts two files beside FF4.exe, both removed by uninstall (and only those two, only when they are ours):
  version.dll       the hook (built by Tools/ff4hook/build.cmd)
  steam_appid.txt   312750 - with it SteamAPI_RestartAppIfNecessary lets FF4.exe start as it is, instead of
                    restarting it through Steam and the launcher (Steam itself must be running)

A script is one step a line ('#' comments):
  wait <seconds>                 real time
  frames <n>                     n of the game's frames (60 a second)
  key <name> [frames]            enter back tab esc space up down left right (FF4.ini: Select = Return, Cancel = Backspace, Menu = Tab)
  shot <name>                    the next frame to <out dir>/<name>.png
  every <frames>                 a frame every <frames> into <out dir>/frames (0 stops)
  trace [off]                    every event-script command the game runs, into <out dir>/trace.tsv
  autokey <command> [frames]    Return [frames] (60) after the scripts first reach <command> (StartMessage) at a place
  chars [frames]                 every character slot in use every [frames] (30), into <out dir>/chars.tsv
  camera [frames]                the camera every [frames] (15), into <out dir>/camera.tsv
  joints <frames> <model> <a,b>  the named joints' world positions for each character of <model>, into <out dir>/joints.tsv
  dumpchars [name]               the character slots' raw bytes now, into <out dir>/<name>.bin
  repeat <n> ... end             the steps between, n times
The .bmp files the hook writes are turned into .png at the end.
"""
import os, sys, time, shutil, subprocess, hashlib

GAME = os.environ.get("FF4_DIR", r"G:\SteamLibrary\steamapps\common\Final Fantasy IV")
HERE = os.path.dirname(os.path.abspath(__file__))
DLL = os.path.join(HERE, "bin", "version.dll")
HOOK = os.path.join(os.environ["TEMP"], "ff4hook")
CMD, STATUS, LOG = (os.path.join(HOOK, n) for n in ("cmd.txt", "status.txt", "hook.log"))
APPID = "312750"
# The Steam Cloud save, which a run must leave as it found it (the game writes it only when told to save).
SAVE = os.environ.get("FF4_SAVE", r"C:\Program Files (x86)\Steam\userdata\40094255\312750\remote\SAVE.BIN")


def digest(path):
    return hashlib.sha256(open(path, "rb").read()).hexdigest() if os.path.exists(path) else None


def install():
    target = os.path.join(GAME, "version.dll")
    if os.path.exists(target) and digest(target) != digest(DLL):
        sys.exit("a version.dll that is not ff4hook's is already in " + GAME + " - not touching it")
    shutil.copyfile(DLL, target)
    appid = os.path.join(GAME, "steam_appid.txt")
    if os.path.exists(appid) and open(appid).read().strip() != APPID:
        sys.exit("a steam_appid.txt with something else in it is in " + GAME + " - not touching it")
    open(appid, "w").write(APPID)
    print("installed: version.dll, steam_appid.txt in", GAME)


def uninstall():
    target = os.path.join(GAME, "version.dll")
    if os.path.exists(target) and digest(target) == digest(DLL):
        os.remove(target)
    appid = os.path.join(GAME, "steam_appid.txt")
    if os.path.exists(appid) and open(appid).read().strip() == APPID:
        os.remove(appid)
    print("uninstalled: the game folder is as shipped")


def send(line):
    with open(CMD, "a", encoding="ascii") as f:
        f.write(line + "\n")


def frame():
    try:
        return int(open(STATUS).read().split()[1])
    except Exception:
        return -1


def parse(lines):
    steps, stack = [], [[]]
    for raw in lines:
        line = raw.split("#", 1)[0].strip()
        if not line:
            continue
        word = line.split()[0]
        if word == "repeat":
            stack.append([("repeat", int(line.split()[1]))])
        elif word == "end":
            block = stack.pop()
            stack[-1].append(block)
        else:
            stack[-1].append(line)
    return stack[0]


def play(steps, out):
    for step in steps:
        if isinstance(step, list):
            _, n = step[0]
            for _ in range(n):
                play(step[1:], out)
            continue
        word, _, rest = step.partition(" ")
        if word == "wait":
            time.sleep(float(rest))
        elif word == "frames":
            start = frame()
            while frame() < start + int(rest):
                time.sleep(0.05)
        elif word == "key":
            send("key " + rest)
        elif word == "shot":
            send("shot " + os.path.join(out, rest + ".bmp"))
            time.sleep(0.2)
        elif word == "trace":
            # every event-script command FF4.exe runs, into <out dir>/trace.tsv (frame, engine, index, position, operand bytes)
            send("trace off" if rest.strip() == "off" else "trace " + os.path.join(out, "trace.tsv"))
        elif word == "autokey":
            # autokey <command name> [frames]: Return that many frames (60) after the scripts first reach the command
            sys.path.insert(0, HERE)
            from trace_compare import load_table
            names = [n for n, _ in load_table()]
            parts = rest.split()
            send("autokey %d %s" % (names.index(parts[0]), parts[1] if len(parts) > 1 else "60"))
        elif word == "chars":
            # each character slot in use every <frames>, into <out dir>/chars.tsv (0 stops)
            n = rest.split()[0] if rest.strip() else "30"
            send("chars " + n + " " + os.path.join(out, "chars.tsv"))
        elif word == "camera":
            # the camera every <frames>, into <out dir>/camera.tsv
            n = rest.split()[0] if rest.strip() else "15"
            send("camera " + n + " " + os.path.join(out, "camera.tsv"))
        elif word == "joints":
            # joints <frames> <model> <node,node,...>: those joints' world positions, into <out dir>/joints.tsv
            n, model, names = rest.split()[:3]
            send("joints %s %s %s %s" % (n, os.path.join(out, "joints.tsv"), model, names))
        elif word == "dumpchars":
            send("dumpchars " + os.path.join(out, (rest.strip() or "slots") + ".bin"))
            time.sleep(0.2)
        elif word == "every":
            send("every " + rest.split()[0] + " " + os.path.join(out, "frames"))
        else:
            print("unknown step:", step)


def to_png(out):
    try:
        from PIL import Image
    except ImportError:
        return
    for root, _, files in os.walk(out):
        for name in files:
            if name.endswith(".bmp"):
                path = os.path.join(root, name)
                Image.open(path).save(path[:-4] + ".png")
                os.remove(path)


def run(script, out):
    os.makedirs(out, exist_ok=True)
    os.makedirs(HOOK, exist_ok=True)
    for p in (CMD, STATUS):
        if os.path.exists(p):
            os.remove(p)
    open(CMD, "w").close()
    save_before = digest(SAVE)
    install()
    proc = subprocess.Popen([os.path.join(GAME, "FF4.exe")], cwd=GAME)
    try:
        for _ in range(300):
            if frame() >= 0:
                break
            if proc.poll() is not None:
                sys.exit("FF4.exe ended before its first frame (exit %s) - is Steam running?" % proc.returncode)
            time.sleep(0.1)
        print("FF4.exe up (pid %d); %s" % (proc.pid, open(LOG).read().strip().splitlines()[-1] if os.path.exists(LOG) else "no hook log"))
        play(parse(open(script).read().splitlines()), out)
    finally:
        send("quit")
        try:
            proc.wait(10)
        except subprocess.TimeoutExpired:
            proc.kill()
        time.sleep(0.5)
        uninstall()
        to_png(out)
        if digest(SAVE) != save_before:
            print("WARNING: the Steam save changed during the run:", SAVE)


if __name__ == "__main__":
    if len(sys.argv) < 2:
        sys.exit(__doc__)
    what = sys.argv[1]
    if what == "install":
        install()
    elif what == "uninstall":
        uninstall()
    elif what == "send":
        send(" ".join(sys.argv[2:]))
    elif what == "run" and len(sys.argv) == 4:
        run(sys.argv[2], sys.argv[3])
    else:
        sys.exit(__doc__)
