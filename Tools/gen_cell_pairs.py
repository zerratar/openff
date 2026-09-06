"""Which picture each cell bank draws from.

A cell bank (.NCER, and 106 of the 109 .NSCR, which are cell banks despite the
extension) says "copy this rectangle to that position" but never names the sheet it
copies from. The game names it at the call site instead, in one of two shapes:

    bg.bgLoad("menu_000_main.NSCR", "menu_bg_01.NCGR", "new_menu_bg.NCLR")
    sprite.Load("name_i.NCER", null, "name_i.NCGR", "name_i.NCLR")

So the pairings are read out of the game's own source rather than guessed. Where a
bank is never loaded by name, the sheet of the same name is used - which is what the
second shape does anyway, for all but a handful.

    python Tools/gen_cell_pairs.py

Writes Crystal.Editor/CellPairs.cs. Read-only against the game source.
"""
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)
GAME = os.path.join(ROOT, 'OpenFF')
OUT = os.path.join(ROOT, 'Crystal.Editor', 'CellPairs.cs')

# "cell.NSCR", "sheet.NCGR", ...    - the background form, sheet second
BG = re.compile(r'"([A-Za-z0-9_]+\.(?:NSCR|NCER))"\s*,\s*"([A-Za-z0-9_]+\.(?:NCGR|NCBR))"')

# "cell.NCER", <anim or null>, "sheet.NCGR", ...  - the sprite form, sheet third
SPRITE = re.compile(
    r'"([A-Za-z0-9_]+\.(?:NCER|NSCR))"\s*,\s*'
    r'(?:null|"[A-Za-z0-9_]+\.NANR")\s*,\s*'
    r'"([A-Za-z0-9_]+\.(?:NCGR|NCBR))"')

# The menus do not name their banks at the call site - they index an array of them:
#
#     private static string[] main_bg_nscr = new string[15] { "menu_001_item.NSCR", ... };
#     ...
#     primaryBG.bgLoad(main_bg_nscr[no], "menu_bg_01.NCGR", "new_menu_bg.NCLR");
#
# which is most of the banks that are not covered by the two forms above, so the
# array is read and every name in it takes the sheet the call passes.
ARRAY = re.compile(
    r'string\[\]\s+([A-Za-z0-9_]+)\s*=\s*new\s+string\[\d*\]\s*\{(.*?)\}',
    re.DOTALL)

INDEXED = re.compile(
    r'bgLoad\(\s*([A-Za-z0-9_]+)\s*\[[^\]]*\]\s*,\s*"([A-Za-z0-9_]+\.(?:NCGR|NCBR))"')

CELL_NAME = re.compile(r'"([A-Za-z0-9_]+\.(?:NCER|NSCR))"')

# Some arrays are never passed to bgLoad at all. The menu swaps which layout is drawn
# on a plane whose sheet was set once, earlier:
#
#     primaryBG.bgLoad("menu_000_main.NSCR", "menu_bg_01.NCGR", "new_menu_bg.NCLR");
#     ...
#     public void SetPrimaryBG(int no) { g_File.load(scrDataPtr, main_bg_nscr[no]); ... }
#
# Following that properly means tracking which plane object holds which sheet. Instead
# the rule is narrower and checkable: if a file names exactly one sheet anywhere in it,
# then every bank named in that file uses it. A file naming two is reported rather than
# guessed at, so this cannot be quietly wrong.
SHEET_NAME = re.compile(r'"([A-Za-z0-9_]+\.(?:NCGR|NCBR))"')

# One call picks its bank with a ternary, and both branches draw from the same sheet:
#
#     Load(plane, (OS_GetLanguage() == 0) ? "title_gousei_new_jp.NCER"
#                                         : "title_gousei_new.NCER",
#          null, "title_gousei_new.NCBR", ...)
#
# so the first branch needs picking up too - the plain forms only see the second.
TERNARY = re.compile(
    r'\?\s*"([A-Za-z0-9_]+\.(?:NCER|NSCR))"\s*:\s*"([A-Za-z0-9_]+\.(?:NCER|NSCR))"\s*,\s*'
    r'(?:null|"[A-Za-z0-9_]+\.NANR")\s*,\s*'
    r'"([A-Za-z0-9_]+\.(?:NCGR|NCBR))"')


def main():
    pairs = {}
    conflicts = []
    files = 0
    sources = []
    arrays = {}

    for folder, _, names in os.walk(GAME):
        for name in names:
            if not name.endswith('.cs'):
                continue
            files += 1
            with open(os.path.join(folder, name), encoding='utf-8-sig',
                      errors='replace') as handle:
                text = handle.read()
            sources.append(text)
            for array, body in ARRAY.findall(text):
                cells = CELL_NAME.findall(body)
                if cells:
                    arrays.setdefault(array, []).extend(cells)

    by_file = set()

    def add(cell, sheet):
        if cell in pairs and pairs[cell] != sheet:
            conflicts.append((cell, pairs[cell], sheet))
            return
        pairs.setdefault(cell, sheet)

    for text in sources:
        for pattern in (SPRITE, BG):
            for cell, sheet in pattern.findall(text):
                add(cell, sheet)

        for first, second, sheet in TERNARY.findall(text):
            add(first, sheet)
            add(second, sheet)

        for array, sheet in INDEXED.findall(text):
            for cell in arrays.get(array, []):
                add(cell, sheet)

    # The one-sheet-per-file rule, applied last so an explicit call always wins.
    ambiguous = []
    for text in sources:
        cells = set(CELL_NAME.findall(text))
        if not cells:
            continue
        found = set(SHEET_NAME.findall(text))
        if len(found) == 1:
            sheet = found.pop()
            for cell in cells:
                if cell not in pairs:
                    pairs[cell] = sheet
                    by_file.add(cell)
        elif len(found) > 1:
            ambiguous.append((sorted(cells)[0], sorted(found)))

    if not pairs:
        sys.exit('found no pairings - has the source moved?')

    same = sum(1 for cell, sheet in pairs.items()
               if os.path.splitext(cell)[0] == os.path.splitext(sheet)[0])

    lines = [
        '// Which picture each cell bank draws from.',
        '//',
        '// GENERATED by Tools/gen_cell_pairs.py from the game\'s own source. Do not edit.',
        '//',
        '// A cell bank says "copy this rectangle to that position" and never names the',
        '// sheet it copies from - the game names it at the call site. These are those',
        '// call sites, read out of %d C# files, following the arrays the menus' % files,
        '// index rather than only the names written at the call.',
        '//',
        '// %d of the %d pair with the sheet of the same name, which is also the fallback'
        % (same, len(pairs)),
        '// for a bank that is never loaded by name. The rest do not, and those are the',
        '// ones worth having this table for: every menu background draws from a single',
        '// shared sheet.',
        '',
        'using System;',
        'using System.Collections.Generic;',
        '',
        'namespace FF3.ContentTool',
        '{',
        '\tinternal static class CellPairs',
        '\t{',
        '\t\t/// <summary>Cell bank file name -> the sheet it copies from.</summary>',
        '\t\tpublic static readonly Dictionary<string, string> Sheet =',
        '\t\t\tnew Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)',
        '\t\t\t{',
    ]

    for cell in sorted(pairs, key=str.lower):
        lines.append('\t\t\t\t["%s"] = "%s",' % (cell, pairs[cell]))

    lines += [
        '\t\t\t};',
        '\t}',
        '}',
        '',
    ]

    with open(OUT, 'w', encoding='utf-8-sig', newline='\r\n') as handle:
        handle.write('\n'.join(lines))

    print('%d pairing(s) from %d files -> %s' % (len(pairs), files, OUT))
    print('%d pair with the sheet of the same name' % same)
    print('%d came from a file that names exactly one sheet' % len(by_file))
    if ambiguous:
        print('%d file(s) name more than one sheet, so their banks were left alone:'
              % len(ambiguous))
        for cell, found in ambiguous[:5]:
            print('   near %s: %s' % (cell, ', '.join(found)))
    if conflicts:
        print('%d cell bank(s) loaded with more than one sheet, first kept:' % len(conflicts))
        for cell, kept, other in conflicts[:5]:
            print('   %s: %s, also %s' % (cell, kept, other))


if __name__ == '__main__':
    main()
