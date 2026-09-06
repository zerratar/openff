"""Reverting an edit takes it out of the game as well as out of the project.

    python Tools/test_revert.py

The half that is easy to get wrong is the installed one: deleting the override alone
leaves the game holding the modded bytes, so the file reads as shipped everywhere
except where it matters. These run against a fake install, so no real game is touched.
"""
import hashlib
import json
import os
import shutil
import subprocess
import sys

PROJECT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
TOOL = os.path.join(PROJECT, 'Crystal.Editor', 'bin', 'Debug', 'net8.0', 'crystal.exe')
HERE = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'obj-revert')
INSTALL = os.path.join(HERE, 'install')
MOD = os.path.join(HERE, 'mod')
BACKUP = MOD + '.backup'
PORT = 5077

SEED = ['MenuDefine.xbn', 'd01_02.script', 'd01_02.hich', 'd01_01.pak']
failures = []


def source_files():
    given = os.environ.get('FF3_STEAM_FILES')
    if given and os.path.isdir(given):
        return given
    if not os.path.exists(TOOL):
        return None
    found = subprocess.run([TOOL, 'installs'], capture_output=True, text=True)
    for line in found.stdout.splitlines():
        candidate = os.path.join(line.strip(), 'files')
        if os.path.isdir(candidate):
            return candidate
    return None


STEAM = source_files()
if STEAM is None:
    print('no loose install to take sample files from; '
          'set FF3_STEAM_FILES to a files directory')
    sys.exit(0)

import urllib.request


def api(path, body=None):
    data = None if body is None else json.dumps(body).encode()
    req = urllib.request.Request('http://localhost:%d%s' % (PORT, path), data=data,
                                 headers={'content-type': 'application/json'})
    with urllib.request.urlopen(req, timeout=120) as r:
        return json.loads(r.read())


def check(what, got, want):
    print('  %-4s %-50s %s' % ('ok' if got == want else 'FAIL', what,
                               got if got == want else 'got %r want %r' % (got, want)))
    if got != want:
        failures.append(what)


def sha(path):
    if not os.path.exists(path):
        return None
    with open(path, 'rb') as f:
        return hashlib.sha1(f.read()).hexdigest()[:12]


live = lambda n: os.path.join(INSTALL, 'files', n)
mine = lambda n: os.path.join(MOD, 'files', n)

shutil.rmtree(HERE, ignore_errors=True)
os.makedirs(os.path.join(INSTALL, 'files'))
for n in SEED:
    shutil.copy(os.path.join(STEAM, n), os.path.join(INSTALL, 'files', n))
os.makedirs(os.path.join(MOD, 'files'))

pristine = {n: sha(live(n)) for n in SEED}
for n in ('d01_02.script', 'd01_02.hich'):
    with open(mine(n), 'wb') as f:
        f.write(b'EDITED ' + n.encode())
with open(mine('added.pak'), 'wb') as f:
    f.write(b'ADDED BY THE MOD')

server = subprocess.Popen(
    [TOOL, 'editor', '--content=' + INSTALL, '--override=' + MOD,
     '--port=%d' % PORT, '--no-browser'],
    stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True)
try:
    for _ in range(60):
        try:
            api('/api/status')
            break
        except Exception:
            subprocess.run(['cmd', '/c', 'timeout', '/t', '1', '/nobreak'],
                           capture_output=True)
    print('== an edit that was never installed ==')
    status = api('/api/mod/status')
    check('three edits seen', len(status['edited']), 3)
    r = api('/api/mod/revert', {'names': ['files/d01_02.hich']})
    check('reverted', r['reverted'], ['files/d01_02.hich'])
    check('the edit is gone', os.path.exists(mine('d01_02.hich')), False)
    check('the game was untouched', sha(live('d01_02.hich')), pristine['d01_02.hich'])
    check('the others are still there', len(api('/api/mod/status')['edited']), 2)

    print()
    print('== an edit that IS installed ==')
    api('/api/mod/install', {})
    check('the game holds the mod', sha(live('d01_02.script')), sha(mine('d01_02.script')))
    check('and the added file', os.path.exists(live('added.pak')), True)

    r = api('/api/mod/revert', {'names': ['files/d01_02.script']})
    check('reverted', r['reverted'], ['files/d01_02.script'])
    check('the edit is gone', os.path.exists(mine('d01_02.script')), False)
    check('THE GAME GOT ITS ORIGINAL BACK', sha(live('d01_02.script')),
          pristine['d01_02.script'])
    check('it said it restored one', r['restored'], ['files/d01_02.script'])
    check('the added file is still installed', os.path.exists(live('added.pak')), True)

    print()
    print('== reverting a file the mod added removes it from the game ==')
    r = api('/api/mod/revert', {'names': ['files/added.pak']})
    check('reverted', r['reverted'], ['files/added.pak'])
    check('gone from the game', os.path.exists(live('added.pak')), False)
    check('gone from the project', os.path.exists(mine('added.pak')), False)
    check('nothing edited left', len(api('/api/mod/status')['edited']), 0)

    print()
    print('== a game updated since install is left alone, edit kept ==')
    with open(mine('d01_01.pak'), 'wb') as f:
        f.write(b'EDITED AGAIN')
    api('/api/mod/install', {})
    with open(live('d01_01.pak'), 'wb') as f:
        f.write(b'A NEWER OFFICIAL VERSION')
    newer = sha(live('d01_01.pak'))
    r = api('/api/mod/revert', {'names': ['files/d01_01.pak']})
    check('the newer file was not overwritten', sha(live('d01_01.pak')), newer)
    check('nothing claimed reverted', r['reverted'], [])
    check('the edit was kept', os.path.exists(mine('d01_01.pak')), True)
    check('and it said why', bool(r['notes']), True)
finally:
    server.terminate()
    try:
        server.wait(timeout=10)
    except Exception:
        server.kill()

print()
shutil.rmtree(HERE, ignore_errors=True)
if failures:
    print('%d FAILED: %s' % (len(failures), ', '.join(failures)))
    sys.exit(1)
print('all checks passed')
