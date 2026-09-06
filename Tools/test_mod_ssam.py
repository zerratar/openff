"""Installing into a mass file, exercised on a copy of FF4's own CAST_SCRIPT.dat.

    python Tools/test_mod_ssam.py

FF4 keeps each map's script inside files/CAST_SCRIPT.dat, so installing an edited
script means rebuilding that container. The cases that would hurt if wrong: the other
388 entries must come out byte for byte, the edited one must decompress to the
project's bytes, the pristine container must be kept once, uninstall must give back the
shipped bytes exactly, reverting one of two edits must rebuild from the pristine copy
plus the other, and a container the game has changed since must be left alone.

The 512-stride layout is covered too, with a portrait installed into FACE.dat.
Runs against a fake install built from copies; the real one is never touched.
"""
import hashlib
import json
import os
import shutil
import struct
import subprocess
import sys
import urllib.request

PROJECT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
TOOL = os.path.join(PROJECT, 'Crystal.Editor', 'bin', 'Debug', 'net8.0', 'crystal.exe')
HERE = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'obj-ssam')
INSTALL = os.path.join(HERE, 'install')
FILES = os.path.join(INSTALL, 'EXTRACTED_DATA', 'files')
MOD = os.path.join(HERE, 'mod')
BACKUP = MOD + '.backup'
PORT = 5078
failures = []


def ff4_files():
    given = os.environ.get('FF4_STEAM_FILES')
    if given and os.path.isdir(given):
        return given
    if not os.path.exists(TOOL):
        return None
    found = subprocess.run([TOOL, 'installs'], capture_output=True, text=True)
    for line in found.stdout.splitlines():
        for candidate in (os.path.join(line.strip(), 'EXTRACTED_DATA', 'files'),):
            if os.path.isfile(os.path.join(candidate, 'CAST_SCRIPT.dat')):
                return candidate
    return None


SRC = ff4_files()
if SRC is None:
    print('no FF4 install to copy CAST_SCRIPT.dat from; set FF4_STEAM_FILES')
    sys.exit(0)


def check(what, got, want):
    ok = got == want
    print('  %-4s %-58s %s' % ('ok' if ok else 'FAIL', what, got if ok else 'got %r want %r' % (got, want)))
    if not ok:
        failures.append(what)


def sha(path_or_bytes):
    data = path_or_bytes if isinstance(path_or_bytes, bytes) else open(path_or_bytes, 'rb').read()
    return hashlib.sha1(data).hexdigest()[:12]


def entries(container):
    d = open(container, 'rb').read()
    n = struct.unpack_from('<I', d, 4)[0]
    base = 8 + 40 * n
    out = {}
    for i in range(n):
        off, size = struct.unpack_from('<II', d, 8 + 40 * i)
        name = d[16 + 40 * i:16 + 40 * i + 32].split(b'\0')[0].decode()
        out[name] = d[base + off:base + off + size]
    return out


def decompress(blob):
    tmp = os.path.join(HERE, 'tmp.lz')
    open(tmp, 'wb').write(blob)
    out = os.path.join(HERE, 'tmp')
    shutil.rmtree(out, ignore_errors=True)
    subprocess.run([TOOL, 'lz', tmp, out], capture_output=True)
    for root, _, fs in os.walk(out):
        for f in fs:
            return open(os.path.join(root, f), 'rb').read()
    return None


def run(command):
    r = subprocess.run([TOOL, command, '--content=' + INSTALL, '--override=' + MOD],
                       capture_output=True, text=True)
    return r.returncode, (r.stdout or '') + (r.stderr or '')


def api(path, body=None):
    data = None if body is None else json.dumps(body).encode()
    req = urllib.request.Request('http://localhost:%d%s' % (PORT, path), data=data,
                                 headers={'content-type': 'application/json'})
    with urllib.request.urlopen(req, timeout=120) as r:
        return json.loads(r.read())


# ---- a fake FF4 install: the real container plus enough loose files to look like one ----------------
shutil.rmtree(HERE, ignore_errors=True)
os.makedirs(FILES)
shutil.copy(os.path.join(SRC, 'CAST_SCRIPT.dat'), FILES)
for n in ('babil_menu.msd', 'd01_01.nmd'):
    if os.path.exists(os.path.join(SRC, n)):
        shutil.copy(os.path.join(SRC, n), FILES)
os.makedirs(os.path.join(MOD, 'files'))

CONTAINER = os.path.join(FILES, 'CAST_SCRIPT.dat')
shipped = open(CONTAINER, 'rb').read()
shipped_entries = entries(CONTAINER)
original_d01_01 = decompress(shipped_entries['d01_01.script.lz'])
original_d01_00 = decompress(shipped_entries['d01_00.script.lz'])

# The edit: the real d01_01 script with one byte of a string changed, so it still
# compresses and decompresses like a script would.
edited = bytearray(original_d01_01)
i = edited.find(b'd01_04')
edited[i:i + 6] = b'd01_09'
edited = bytes(edited)
open(os.path.join(MOD, 'files', 'd01_01.script'), 'wb').write(edited)

print('== install one edited script into the container ==')
code, out = run('install')
check('install succeeds', code, 0)
now = entries(CONTAINER)
check('container still has every entry', len(now), len(shipped_entries))
check('edited entry decompresses to the edit', decompress(now['d01_01.script.lz']) == edited, True)
check('a neighbouring entry is byte-identical', now['d01_00.script.lz'] == shipped_entries['d01_00.script.lz'], True)
untouched = [n for n in shipped_entries if n != 'd01_01.script.lz' and now[n] != shipped_entries[n]]
check('every other entry is byte-identical', untouched, [])
kept = os.path.join(BACKUP, 'files', 'files', 'CAST_SCRIPT.dat')
check('pristine container was kept', os.path.exists(kept) and sha(kept) == sha(shipped), True)

print()
print('== installing again does not clobber the pristine copy ==')
edited2 = edited.replace(b'd01_09', b'd01_08')
open(os.path.join(MOD, 'files', 'd01_01.script'), 'wb').write(edited2)
code, out = run('install')
check('second install succeeds', code, 0)
check('pristine copy unchanged', sha(kept), sha(shipped))
check('container carries the newer edit', decompress(entries(CONTAINER)['d01_01.script.lz']) == edited2, True)

print()
print('== uninstall gives the shipped container back exactly ==')
code, out = run('uninstall')
check('uninstall succeeds', code, 0)
check('container byte-identical to shipped', sha(CONTAINER), sha(shipped))

print()
print('== two edits, revert one: rebuilt from pristine plus the other ==')
edit00 = original_d01_00.replace(b'd01_01', b'd01_07', 1)
open(os.path.join(MOD, 'files', 'd01_00.script'), 'wb').write(edit00)
code, out = run('install')
check('install of two succeeds', code, 0)
both = entries(CONTAINER)
check('both edits in', decompress(both['d01_00.script.lz']) == edit00 and decompress(both['d01_01.script.lz']) == edited2, True)

server = subprocess.Popen([TOOL, 'editor', '--content=' + INSTALL, '--override=' + MOD,
                           '--port=%d' % PORT, '--no-browser'],
                          stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True)
try:
    for _ in range(60):
        try:
            api('/api/status')
            break
        except Exception:
            subprocess.run(['cmd', '/c', 'timeout', '/t', '1', '/nobreak'], capture_output=True)
    status = api('/api/mod/status')
    check('status sees both installed', sorted(status['installed']), ['files/d01_00.script', 'files/d01_01.script'])
    r = api('/api/mod/revert', {'names': ['files/d01_01.script']})
    check('revert reports the one', r['reverted'], ['files/d01_01.script'])
    after = entries(CONTAINER)
    check('reverted entry is the shipped one again', after['d01_01.script.lz'] == shipped_entries['d01_01.script.lz'], True)
    check('the other edit is still in', decompress(after['d01_00.script.lz']) == edit00, True)
    check('project no longer has the reverted edit', os.path.exists(os.path.join(MOD, 'files', 'd01_01.script')), False)

    print()
    print('== a container the game changed since is left alone ==')
    tampered = bytearray(open(CONTAINER, 'rb').read())
    tampered[-1] ^= 0xFF
    open(CONTAINER, 'wb').write(bytes(tampered))
    r = api('/api/mod/revert', {'names': ['files/d01_00.script']})
    check('nothing claimed reverted', r['reverted'], [])
    check('the changed container was not overwritten', sha(CONTAINER), sha(bytes(tampered)))
    check('the edit was kept', os.path.exists(os.path.join(MOD, 'files', 'd01_00.script')), True)
    check('and it said why', bool(r.get('notes')) or bool(r.get('skipped')), True)
finally:
    server.terminate()
    try:
        server.wait(timeout=10)
    except Exception:
        server.kill()

print()
print('== the 512-stride layout: a portrait installed into FACE.dat ==')
shutil.rmtree(MOD, ignore_errors=True); shutil.rmtree(BACKUP, ignore_errors=True)
os.makedirs(os.path.join(MOD, 'files'))
shutil.copy(os.path.join(SRC, 'FACE.dat'), FILES)
FACE = os.path.join(FILES, 'FACE.dat')
face_shipped = open(FACE, 'rb').read()
face_entries = entries(FACE)
first = sorted(face_entries)[0]                       # e.g. m095_001.face.lz
exposed = 'files/' + first[:-3]
portrait = decompress(face_entries[first])
edited_face = bytes(portrait[:-1]) + bytes([portrait[-1] ^ 0x5A])
os.makedirs(os.path.dirname(os.path.join(MOD, exposed)), exist_ok=True)
open(os.path.join(MOD, exposed), 'wb').write(edited_face)
code, out = run('install')
check('install into FACE.dat succeeds', code, 0)
face_now = entries(FACE)
check('FACE.dat still has every entry', len(face_now), len(face_entries))
check('edited portrait decompresses to the edit', decompress(face_now[first]) == edited_face, True)
others = [n for n in face_entries if n != first and face_now[n] != face_entries[n]]
check('every other portrait byte-identical', others, [])
d = open(FACE, 'rb').read(); n = struct.unpack_from('<I', d, 4)[0]
check('first entry still at offset 504', struct.unpack_from('<I', d, 8)[0], 504)
code, out = run('uninstall')
check('uninstall restores FACE.dat byte for byte', sha(FACE), sha(face_shipped))

print()
shutil.rmtree(HERE, ignore_errors=True)
if failures:
    print('%d FAILED: %s' % (len(failures), ', '.join(failures)))
    sys.exit(1)
print('all checks passed')
