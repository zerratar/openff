"""Derive the .pak record layouts from the decompiled parameter classes.

    python Tools/gen_records.py

A .pak is a container of chains, each chain an array of fixed size records. Every
record type has a parse(ArrayReader) that reads its fields in order, so the field
list, the field types and the record size can be read straight out of the game rather
than guessed from hex.

A parse method is not always flat. A monster embeds its body, attack and defence
parameters as sub records, plus an array of special actions read in a loop, so nested
parse calls are followed and loops with a constant bound are unrolled. Counting only
the direct reads makes a monster 27 bytes instead of 100, and every record after the
first would then be read from the wrong offset.

Which is why every layout is checked against the stride the game itself divides by:
ItemManager counts weapons in steps of 56, mon counts monsters in steps of 100. A
layout that does not add up is reported and not emitted.

Reads the game sources; writes Crystal.Editor/PakRecords.cs and nothing else.
"""
import io
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from record_notes import NOTES

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)
GAME = os.path.join(ROOT, 'OpenFF', 'GlobalScope')
DEST = os.path.join(ROOT, 'Crystal.Editor', 'PakRecords.cs')

TAB = chr(9)
NL = chr(10)

# One entry per kind of .pak: the record class per chain, and the stride the game
# strides by when it counts that chain. Strides come from the loaders themselves -
# itm.ItemManager.load and GlobalScope.mon. The map chains are not counted with a
# literal stride anywhere, so those are validated against the shipped files instead,
# by Tools/check_records.py.
FAMILIES = [
    ('Item', 'item_parameter.pak', [
        ('ConsumptionParameter', 'consumables', 44),
        ('WeaponParameter', 'weapons', 56),
        ('ProtectionParameter', 'armour', 60),
        ('MagicParameter', 'magic', 52),
        ('ImportantParameter', 'keyItems', 28),
    ]),
    ('Monster', 'monster.chaindata', [
        ('MonsterParameter', 'monsters', 100),
        ('DropItemParameter', 'drops', 18),
        ('MonsterNormalAttackParameter', 'normalAttacks', 28),
        ('MonsterSpecialAttackParameter', 'specialAttacks', 16),
        ('MonsterOffsetParameter', 'offsets', 160),
        ('MonsterSpecialAttackEffects', 'specialAttackEffects', 56),
    ]),
    # pl.PlayerParty.load: the player and job side of the game. Seven of the chains
    # are the same MP growth table, one per magic level.
    ('Player', 'player.chaindata', [
        ('PlayerExp', 'expCurve', None),
        ('JobGrowUpType', 'jobGrowUpTypes', None),
        ('GrowUp', 'growth', None),
        ('PlayerNormalAttackParameter', 'normalAttacks', None),
        ('GrowUpMp', 'mpGrowth1', None),
        ('GrowUpMp', 'mpGrowth2', None),
        ('GrowUpMp', 'mpGrowth3', None),
        ('GrowUpMp', 'mpGrowth4', None),
        ('GrowUpMp', 'mpGrowth5', None),
        ('GrowUpMp', 'mpGrowth6', None),
        ('GrowUpMp', 'mpGrowth7', None),
        ('JobEquipInfo', 'jobEquipment', None),
        ('PlayerNormalMagicParameter', 'normalMagic', 32),
        ('AbilityParameter', 'abilities', None),
        ('PlayerAbility', 'jobAbilities', None),
    ]),
    ('Map', '<stage>.pak', [
        ('CMapJumpParameter', 'jumps', None),
        ('CMapLandFormParameter', 'landForms', None),
        ('CMapMonsterPartyParameter', 'monsterParties', None),
        ('CMapSoundParameter', 'sounds', None),
        ('CMapEnCountParameter', 'encounters', None),
        ('CMapCameraParameter', 'cameras', None),
    ]),
]

SIZES = {
    'readByte': 1, 'readSByte': 1,
    'readInt16': 2, 'readUInt16': 2,
    'readInt32': 4, 'readUInt32': 4, 'readSingle': 4,
}

ARRAY_TYPE = {
    'byte': 'readByte', 'sbyte': 'readSByte',
    'short': 'readInt16', 'ushort': 'readUInt16',
    'int': 'readInt32', 'uint': 'readUInt32', 'float': 'readSingle',
}

EMIT_TYPE = {
    'readByte': 'U8', 'readSByte': 'S8',
    'readInt16': 'S16', 'readUInt16': 'U16',
    'readInt32': 'S32', 'readUInt32': 'U32', 'readSingle': 'F32',
}

_SOURCES = None


def _all_sources():
    global _SOURCES
    if _SOURCES is None:
        _SOURCES = {}
        for folder, _, names in os.walk(GAME):
            for name in sorted(names):
                if name.endswith('.cs'):
                    path = os.path.join(folder, name)
                    _SOURCES[path] = io.open(path, encoding='utf-8-sig', newline='').read()
    return _SOURCES


def _block_after(text, at):
    """Contents of the brace block starting at or after `at`, and where it ends."""
    i = text.index('{', at)
    depth, j = 0, i
    while j < len(text):
        c = text[j]
        if c == '{':
            depth += 1
        elif c == '}':
            depth -= 1
            if depth == 0:
                break
        j += 1
    return text[i + 1:j], j + 1


def class_body(name):
    """The body of one class. Several share a file - the six monster tables all sit in
    GlobalScope.mon.cs - so the class is found by its declaration and cut out by brace
    matching rather than by file name."""
    needle = re.compile('class ' + re.escape(name) + '(?![A-Za-z0-9_])')
    for path, text in _all_sources().items():
        mm = needle.search(text)
        if mm:
            return _block_after(text, mm.end())[0]
    return None


def constant(name, src, depth=0):
    """An integer constant used as a length. One level of indirection is normal -
    MAP_ENCOUNT_BTLFIELD_PARAM_MAX is defined as MAP_LANDFORM_PARAM_MAX - so an alias
    is followed rather than treated as unknown."""
    if name.isdigit():
        return int(name)
    if depth > 4:
        return None
    pattern = re.compile(re.escape(name) + r'\s*=\s*([A-Za-z_0-9]+)')
    for text in [src] + list(_all_sources().values()):
        mm = pattern.search(text)
        if mm:
            value = mm.group(1)
            return int(value) if value.isdigit() else constant(value, src, depth + 1)
    return None


def field_type(field, src):
    """The declared type name of a field, without its namespace."""
    mm = re.search(r'(?:private|protected|public)\s+(?:new\s+)?([A-Za-z_0-9.]+)'
                   r'(\[\])?\s+' + re.escape(field) + r'(?![A-Za-z0-9_])', src)
    if not mm:
        mm = re.search(r'([A-Za-z_0-9.]+)(\[\])?\s+' + re.escape(field) + r'\s*=', src)
    if not mm:
        return None
    return mm.group(1).split('.')[-1]


def parse_body(src):
    """A class's parse body, the reader parameter's name, and the name of a count
    parameter where the record sizes a trailing array from the chain length."""
    mm = re.search(r'public void parse\(ArrayReader (\w+)\)\s*\n\s*\{', src)
    if mm:
        return _block_after(src, mm.end() - 1)[0], mm.group(1), None
    mm = re.search(r'public void parse\(ArrayReader (\w+), int (\w+)\)\s*\n\s*\{', src)
    if mm:
        return _block_after(src, mm.end() - 1)[0], mm.group(1), mm.group(2)
    return None, None, None


def tidy(name):
    """m_NextMapIndex -> nextMapIndex. The decompiler's prefixes are noise to anyone
    reading a table, and the leading capital is a C++ habit, not information."""
    if not name:
        return name
    name = name.strip('_')
    if name.startswith('m_'):
        name = name[2:]
    name = name.strip('_')
    if not name:
        return 'unnamed'
    # Leave an acronym alone: HPMax should not become hPMax.
    if len(name) > 1 and name[1].isupper():
        return name
    return name[0].lower() + name[1:]


def destination_of(buffer, body):
    """The field a local buffer is copied into, if it is."""
    mm = re.search(r'(\w+)\s*=\s*\w+\.\w+\(\s*' + re.escape(buffer) + r'', body)
    return mm.group(1) if mm else None


class Unsupported(Exception):
    """A parse method shaped in a way this script will not guess at."""


def fields_of(body, src, reader, counted, prefix='', depth=0):
    """Ordered (name, read, count) triples for one parse body."""
    if depth > 6:
        raise Unsupported('nested parse calls go deeper than expected')

    loop = re.compile(r'for \(int (\w+) = 0; \1 < ([A-Za-z_0-9]+); \1\+\+\)')
    # Two shapes name a field: "x_ = reader.readInt16()" and, where the field is a
    # small wrapper type, "x_.set(reader.readInt32())".
    read = re.compile(r'(?:(\w+)\s*(?:=|\.set\()\s*)?' + re.escape(reader)
                      + r'\.(read\w*)\(([^)]*)\)')
    sub = re.compile(r'(\w+)(?:\[\w+\])?\.parse\(' + re.escape(reader) + r'\)')

    fields = []
    i, n = 0, len(body)
    while i < n:
        mm = loop.match(body, i)
        if mm:
            bound = constant(mm.group(2), src)
            if bound is None:
                raise Unsupported('loop bound ' + mm.group(2) + ' is not a constant')
            inner, after = _block_after(body, mm.end())
            for index in range(bound):
                fields.extend(fields_of(inner, src, reader, counted,
                                        '%s%d.' % (prefix, index), depth + 1))
            i = after
            continue

        mm = sub.match(body, i)
        if mm:
            field = mm.group(1)
            type_name = field_type(field, src)
            nested = class_body(type_name) if type_name else None
            if nested is None:
                raise Unsupported('cannot find the class behind ' + field)
            nested_body, nested_reader, _ = parse_body(nested)
            if nested_body is None:
                raise Unsupported(type_name + ' has no parse method')
            fields.extend(fields_of(nested_body, nested, nested_reader, None,
                                    prefix + field.rstrip('_') + '.', depth + 1))
            i = mm.end()
            continue

        mm = read.match(body, i)
        if mm:
            name, kind, args = mm.group(1), mm.group(2), mm.group(3)
            if kind == 'read':
                parts = [a.strip() for a in args.split(',')]
                if len(parts) != 3:
                    raise Unsupported('array read with unexpected arguments: ' + args)
                # The parse body first: ChainPointer declares its own local called
                # "array", and searching the whole class would find that one instead
                # of the buffer this read fills.
                element = (ARRAY_TYPE.get(field_type(parts[0], body))
                           or ARRAY_TYPE.get(field_type(parts[0], src)))
                if element is None:
                    raise Unsupported('cannot type the array ' + parts[0])
                # A read into a local buffer that is then stored in a field should
                # carry the field's name: CMapJumpParameter reads 16 bytes into a
                # local called "array" and turns it into m_NextMapName, and "array"
                # tells a reader nothing at all.
                label = destination_of(parts[0], body) or parts[0]
                if parts[2] == counted:
                    fields.append((prefix + tidy(label), element, -1))
                else:
                    length = constant(parts[2], src)
                    if length is None:
                        raise Unsupported('cannot size the array ' + parts[0])
                    fields.append((prefix + tidy(label), element, length))
            elif kind in SIZES:
                label = tidy(name) if name else ('unnamed%d' % len(fields))
                fields.append((prefix + label, kind, 1))
            else:
                raise Unsupported('unhandled read: ' + kind)
            i = mm.end()
            continue

        i += 1
    return fields


def layout_of(class_name):
    src = class_body(class_name)
    if src is None:
        return None, 'class not found'
    body, reader, counted = parse_body(src)
    if body is None:
        return None, 'no parse method'
    try:
        fields = fields_of(body, src, reader, counted)
    except Unsupported as problem:
        return None, str(problem)
    if not fields:
        return None, 'parse method reads nothing'
    return fields, None


def size_of(fields):
    """Bytes one record occupies. A count of -1 fills whatever the chain has left, so
    it contributes nothing to the fixed part."""
    return sum(SIZES[kind] * count for _, kind, count in fields if count > 0)


def is_variable(fields):
    return any(count < 0 for _, _, count in fields)


emitted = []
problems = []
for family, filename, chains in FAMILIES:
    for index, (class_name, label, stride) in enumerate(chains):
        fields, error = layout_of(class_name)
        if fields is None:
            problems.append((class_name, error))
            continue
        size = size_of(fields)
        if stride is not None and not is_variable(fields) and size != stride:
            problems.append((class_name, 'fields add up to %d, the game strides by %d'
                             % (size, stride)))
            continue
        emitted.append((family, filename, index, label, class_name, size, fields))

print("record types emitted:", len(emitted))
for family, filename, index, label, class_name, size, fields in emitted:
    print("  %-8s chain %d  %-22s %-34s %4d bytes%s, %3d fields"
          % (family, index, label, class_name, size,
             " + fill" if is_variable(fields) else "       ", len(fields)))
if problems:
    print()
    print("not emitted:")
    for class_name, error in problems:
        print("   %-40s %s" % (class_name, error))

head = '''// Generated from the decompiled parameter classes - see Docs/Tables.md.
//
// A .pak holds chains, each chain an array of fixed size records. These layouts come
// from each record type's parse(ArrayReader) - nested sub records followed, constant
// bound loops unrolled - so the field names are the game's own.
//
// Every layout with a known stride is checked against it: the game counts weapons in
// steps of 56 and monsters in steps of 100, so a layout that does not add up is
// rejected rather than shifting every record after the first.
//
// Do not edit by hand - run Tools/gen_records.py, which rebuilds it from the sources.

namespace Crystal
{
@Tinternal enum FieldType
@T{
@T@TU8,
@T@TS8,
@T@TU16,
@T@TS16,
@T@TU32,
@T@TS32,
@T@TF32
@T}

@Tinternal sealed class PakField
@T{
@T@Tpublic readonly string Name;
@T@Tpublic readonly FieldType Type;

@T@T/// <summary>Elements, or -1 for an array that fills the rest of the chain.</summary>
@T@Tpublic readonly int Count;

@T@Tpublic PakField(string name, FieldType type, int count)
@T@T{
@T@T@TName = name;
@T@T@TType = type;
@T@T@TCount = count;
@T@T}
@T}

@Tinternal sealed class PakChain
@T{
@T@Tpublic readonly string Family;
@T@Tpublic readonly int Index;
@T@Tpublic readonly string Label;
@T@Tpublic readonly string Source;

@T@T/// <summary>Fixed bytes per record; with a filling field, the size of the head.</summary>
@T@Tpublic readonly int Stride;
@T@Tpublic readonly PakField[] Fields;

@T@T/// <summary>
@T@T/// What the table is for, in a sentence, or null. Written by hand in
@T@T/// Tools/record_notes.py - the code says what the fields are, not what the
@T@T/// table is for - and left out where nobody has looked properly yet.
@T@T/// </summary>
@T@Tpublic readonly string Note;

@T@Tpublic PakChain(string family, int index, string label, string source, int stride,
@T@T@TPakField[] fields, string note = null)
@T@T{
@T@T@TFamily = family;
@T@T@TIndex = index;
@T@T@TLabel = label;
@T@T@TSource = source;
@T@T@TStride = stride;
@T@T@TFields = fields;
@T@T@TNote = note;
@T@T}
@T}

@Tinternal static class PakRecords
@T{
@T@Tpublic static readonly PakChain[] Chains =
@T@T{
'''

tail = '''@T@T};

@T@Tpublic static PakChain Find(string family, int index)
@T@T{
@T@T@Tforeach (PakChain chain in Chains)
@T@T@T{
@T@T@T@Tif (chain.Family == family && chain.Index == index)
@T@T@T@T{
@T@T@T@T@Treturn chain;
@T@T@T@T}
@T@T@T}
@T@T@Treturn null;
@T@T}
@T}
}
'''

lines = []
for family, filename, index, label, class_name, size, fields in emitted:
    lines.append('@T@T@Tnew PakChain("%s", %d, "%s", "%s", %d, new[]'
                 % (family, index, label, class_name, size))
    lines.append('@T@T@T{')
    for name, kind, count in fields:
        lines.append('@T@T@T@Tnew PakField("%s", FieldType.%s, %d),'
                     % (name, EMIT_TYPE[kind], count))
    note = NOTES.get((family, label))
    if note:
        lines.append('@T@T@T}, "%s"),' % note.replace('"', "'"))
    else:
        lines.append('@T@T@T}),')

text = (head + NL.join(lines) + NL + tail).replace('@T', TAB)
io.open(DEST, 'w', encoding='utf-8', newline=chr(13) + NL).write(text)
print()
print("wrote", os.path.relpath(DEST, ROOT))
