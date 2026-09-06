"""install / uninstall, exercised on a fake install so no real game is touched.

    python Tools/test_mod_install.py

Covers the cases that would hurt if they were wrong: a second install must not
overwrite the pristine backup with modded bytes, an uninstall must not put a stale
original back over a file the game has since updated, and a file the mod adds must be
removed rather than restored. The last of those was wrong the first time - after one
install the added file exists, so a second install decided it had replaced an
original and uninstall then left it behind.

Sample files come from whatever loose install `crystal installs` finds; set
FF3_STEAM_FILES to a files directory to point it somewhere else.
"""
import hashlib
import os
import shutil
import subprocess
import sys

PROJECT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
TOOL = os.path.join(PROJECT, 'Crystal.Editor', 'bin', 'Debug', 'net8.0', 'crystal.exe')
HERE = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'obj')
INSTALL = os.path.join(HERE, 'fakeinstall')
MOD = os.path.join(HERE, 'fakemod')
BACKUP = MOD + '.backup'

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


def check(what, got, want):
    if got == want:
        print('  ok   %-52s %s' % (what, got))
    else:
        print('  FAIL %-52s got %r want %r' % (what, got, want))
        failures.append(what)


def sha(path):
    if not os.path.exists(path):
        return None
    with open(path, 'rb') as f:
        return hashlib.sha1(f.read()).hexdigest()[:12]


def run(command):
    r = subprocess.run([TOOL, command, '--content=' + INSTALL, '--override=' + MOD],
                       capture_output=True, text=True)
    return r.returncode, (r.stdout or '') + (r.stderr or '')


def fresh():
    for d in (INSTALL, MOD, BACKUP):
        shutil.rmtree(d, ignore_errors=True)
    os.makedirs(os.path.join(INSTALL, 'files'))
    for n in SEED:
        shutil.copy(os.path.join(STEAM, n), os.path.join(INSTALL, 'files', n))
    os.makedirs(os.path.join(MOD, 'files'))


os.makedirs(HERE, exist_ok=True)
live = lambda n: os.path.join(INSTALL, 'files', n)
mine = lambda n: os.path.join(MOD, 'files', n)
kept = lambda n: os.path.join(BACKUP, 'files', 'files', n)

print('== a mod that edits one file and adds another ==')
fresh()
pristine = sha(live('d01_02.script'))
with open(mine('d01_02.script'), 'wb') as f:
    f.write(b'EDITED BY THE MOD')
with open(mine('brand_new.pak'), 'wb') as f:
    f.write(b'ADDED BY THE MOD')

code, out = run('install')
check('install succeeds', code, 0)
check('edited file is the mod', sha(live('d01_02.script')), sha(mine('d01_02.script')))
check('added file is there', sha(live('brand_new.pak')), sha(mine('brand_new.pak')))
check('original was kept', sha(kept('d01_02.script')), pristine)
check('nothing kept for the added one', os.path.exists(kept('brand_new.pak')), False)
check('untouched file untouched', sha(live('d01_01.pak')) is not None, True)

print()
print('== installing again must not clobber the pristine backup ==')
with open(mine('d01_02.script'), 'wb') as f:
    f.write(b'EDITED A SECOND TIME')
code, out = run('install')
check('second install succeeds', code, 0)
check('backup is still the original', sha(kept('d01_02.script')), pristine)
check('install has the newer edit', sha(live('d01_02.script')), sha(mine('d01_02.script')))

print()
print('== uninstall puts it back and removes what was added ==')
code, out = run('uninstall')
check('uninstall succeeds', code, 0)
check('original restored', sha(live('d01_02.script')), pristine)
check('added file removed', os.path.exists(live('brand_new.pak')), False)
check('untouched file still there', os.path.exists(live('d01_01.pak')), True)

print()
print('== a game update since install is left alone ==')
fresh()
pristine = sha(live('d01_02.script'))
with open(mine('d01_02.script'), 'wb') as f:
    f.write(b'EDITED BY THE MOD')
run('install')
# Steam verifies the files, or patches them: the install no longer holds our bytes.
with open(live('d01_02.script'), 'wb') as f:
    f.write(b'A NEWER OFFICIAL VERSION')
after_update = sha(live('d01_02.script'))
code, out = run('uninstall')
check('uninstall succeeds', code, 0)
check('the newer file was not overwritten', sha(live('d01_02.script')), after_update)
check('and it said so', 'left alone' in out, True)

print()
print('== our own archives refuse, rather than pretending ==')
r = subprocess.run([TOOL, 'install', '--content=' + os.path.join(PROJECT, 'Content')],
                   capture_output=True, text=True)
check('refused', r.returncode, 1)
check('with a reason', 'nothing to install' in (r.stdout + r.stderr), True)

print()
shutil.rmtree(HERE, ignore_errors=True)
if failures:
    print('%d FAILED: %s' % (len(failures), ', '.join(failures)))
    sys.exit(1)
print('all checks passed')
