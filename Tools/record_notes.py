"""What each .pak chain is for, in a sentence.

Imported by gen_records.py. These are written by hand rather than derived, because
"what is this table for" is not something the code says - it is something you work out
by reading the loader and looking at the decoded rows.

So the rule for this file is: only describe what the loader and the data actually
show. A chain nobody has looked at properly gets no entry, and the editor says nothing
rather than something invented.
"""

NOTES = {
    ('Item', 'consumables'):
        'Items that are used up: potions, antidotes, tents, status cures.',
    ('Item', 'weapons'):
        'Every weapon. aggressivity is attack power, armsAttribute is the element, '
        'equipJob is a bitmask of the jobs allowed to hold it.',
    ('Item', 'armour'):
        'Shields, helmets, armour and accessories, with the defence they give and '
        'the jobs that can wear them.',
    ('Item', 'magic'):
        'Spells as items - what a shop sells and what a character carries.',
    ('Item', 'keyItems'):
        'Story items that are never used up: the fangs, the keys, the ship parts.',

    ('Monster', 'monsters'):
        'One row per monster: level, hp, the body/attack/defence blocks, and the ids '
        'of its name and its model.',
    ('Monster', 'drops'):
        'What a monster can leave behind when it dies.',
    ('Monster', 'normalAttacks'):
        'The ordinary attack a monster makes.',
    ('Monster', 'specialAttacks'):
        'The special moves a monster can use.',
    ('Monster', 'offsets'):
        'Placement numbers used when a monster is put on the battle field.',
    ('Monster', 'specialAttackEffects'):
        'The effect that plays with a special attack.',

    ('Map', 'jumps'):
        'Where each exit on this map leads: the position and facing the player '
        'arrives at, the map they arrive on, and the flag that has to be set for the '
        'exit to work at all.',
    ('Map', 'landForms'):
        'Terrain attributes per tile type, and which battle field each one fights on.',
    ('Map', 'monsterParties'):
        'The groups of monsters this map can throw at you.',
    ('Map', 'sounds'):
        'The music and ambience this map sets up.',
    ('Map', 'encounters'):
        'How dangerous the map is: an area level and a run of encounter rates.',
    ('Map', 'cameras'):
        'Camera positions and angles the map switches between.',

    ('Player', 'expCurve'):
        'Experience needed for each level. The first entries are 0, 16, 47, 105.',
    ('Player', 'jobGrowUpTypes'):
        'Which growth pattern each job uses.',
    ('Player', 'growth'):
        'How stats climb with level, per growth pattern.',
    ('Player', 'normalAttacks'):
        'The 55 ordinary attacks, looked up by motion id.',
    ('Player', 'jobEquipment'):
        'What each job is allowed to equip.',
    ('Player', 'normalMagic'):
        'Spell entries: level, cost and effect.',
    ('Player', 'abilities'):
        'Battle commands - Attack, Run Away, and the job abilities - with the message '
        'id each one shows.',
    ('Player', 'jobAbilities'):
        'Which abilities each job starts with.',
}

# mpGrowth1..7 are the same table once per magic level, so they share a description.
for level in range(1, 8):
    NOTES[('Player', 'mpGrowth%d' % level)] = (
        'MP growth for level %d spells, per job.' % level)
