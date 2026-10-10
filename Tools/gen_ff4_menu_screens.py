# The full screens of FF4's menu (appended by gen_ff4_menu.py): exec'd with its frame() and indent() in scope; returns `screens`.

def window(id_, x, y, w, h, cls, attrs='', inner=''):
    """A window as Steam draws it: the fill (two bands, by its class) and frame_00 6.75 outside it."""
    body = frame('frame', -6.75, -6.75, w + 13.5, h + 13.5, 'm-window')
    if inner: body += '\n' + inner
    return frame(id_, x, y, w, h, cls, attrs, indent(body, 1))

def keys(id_, display, items):
    """Key hints: (cell, x, label binding) at their places along the bottom."""
    fs = []
    for i, (cell, x, label) in enumerate(items):
        fs.append(frame('key%d' % i, x, 0, 60.75, 60.75, 'key-cap', '', '', '-ff-cell: key %d' % cell))
        fs.append(frame('label%d' % i, x + 60.8, 0, 360, 60.75, 'm-key-label', 'bind-text="%s"' % label))
    return frame(id_, 0, 995.6, 1920, 60.75, None, 'bind-display="%s"' % display, indent('\n'.join(fs), 1))

def scroll(id_, x, y, h, source):
    s = '\n'.join([
        frame('up', 0, 0, 60.75, 33.75, 'm-scroll-up'),
        frame('track', 0, 33.75, 60.75, h - 67.5, 'm-scroll-track'),
        frame('down', 0, h - 33.75, 60.75, 33.75, 'm-scroll-down'),
        frame('knob', 3.4, 33.75, 54, h - 67.5, 'm-scroll-knob', 'bind-visible="%s.knob" bind-style="top: {%s.knobTop}px; height: {%s.knobHeight}px"' % (source, source, source)),
    ])
    return frame(id_, x, y, 60.75, h, None, 'bind-display="%s.shown"' % source, indent(s, 1))

def cell(id_, source, x, y, w, h, hand_y, classes='lit: lit; dim: dim'):
    inner = '\n'.join([
        frame('band', 0, 0, w, h, 'lit-band', 'bind-visible="lit"'),
        frame('edge', 0, 0, w, h, 'm-edge'),
        frame('icon', 81.1, 0, 36, h, 'm-icon', 'bind-style="-ff-cell: symbol {icon}"'),
        frame('name', 121.5, 0, w - 200, h, 'm-cell-name', 'bind-text="{name}"'),
        frame('count', 0, 0, w - 78, h, 'm-cell-count', 'bind-text="{count}"'),
        frame('hand', 81, 0, 1, h, 'hand', 'bind-visible="lit" style="-ff-cell-origin: 0 %spx"' % hand_y),
    ])
    return frame(id_, x, y, w, h, 'm-cell', 'data-source="%s" bind-display="present" bind-class="%s"' % (source, classes), indent(inner, 1))

def target(id_, source, y):
    """A member in the use-on-whom panel: 148.5 high, the face 81 in, the name and level 54 right of the main menu's."""
    inner = '\n'.join([
        frame('band', 0, 0, 1210.1, 148.5, 'lit-band m-target-band', 'bind-visible="lit"'),
        frame('edge', 0, 0, 1210.1, 148.5, 'm-edge'),
        frame('face', 81.1, 6.75, 135, 135, 'm-face', 'bind-display="present" bind-style="-ff-cell: face {face}"'),
        frame('name', 243.3, 16.9, 400, 60.75, 'm-text', 'bind-text="{name}"'),
        frame('lv', 243.3, 70.9, 120, 60.75, 'm-text', 'bind-display="present" bind-text="{menu.lvLabel}"'),
        frame('level', 378.6, 70.9, 120, 60.75, 'm-text', 'bind-text="{level}"'),
        frame('hpLabel', 540.8, 16.9, 120, 60.75, 'm-text', 'bind-display="present" bind-text="{menu.hpLabel}"'),
        frame('hp', 600, 16.9, 166, 60.75, 'm-text m-right m-hp', 'bind-text="{hp}"'),
        frame('hpSlash', 774.1, 16.9, 30, 60.75, 'm-text', 'bind-display="present" bind-text="/"'),
        frame('maxHp', 797.7, 16.9, 140, 60.75, 'm-text', 'bind-text="{maxHp}"'),
        frame('mpLabel', 540.8, 70.9, 120, 60.75, 'm-text', 'bind-display="present" bind-text="{menu.mpLabel}"'),
        frame('mp', 600, 70.9, 166, 60.75, 'm-text m-right', 'bind-text="{mp}"'),
        frame('mpSlash', 774.1, 70.9, 30, 60.75, 'm-text', 'bind-display="present" bind-text="/"'),
        frame('maxMp', 797.7, 70.9, 140, 60.75, 'm-text', 'bind-text="{maxMp}"'),
        frame('hand', 87.8, 0, 1, 148.5, 'hand', 'bind-visible="lit" style="-ff-cell-origin: 0 50%"'),
    ])
    return frame(id_, 0, y, 1210.1, 148.5, 'm-member', 'data-source="%s" bind-class="lit: lit; dim: dim; low: low; back: back"' % source, indent(inner, 1))

screens = []

# Every full screen: the wallpaper, the title bar, the footer.
common = '\n'.join([
    frame('wallpaper', 0, 0, 1920, 1080, 'm-wallpaper'),
    window('titlebar', 155.5, 6.75, 1609, 67.5, 'm-title', 'bind-display="!menu.noTitle"', frame('title', 0, 0, 1609, 67.5, 'm-text m-centre', 'bind-text="{menu.title}"')),
    window('footer', 155.5, 965.25, 1609, 108, 'm-pane'),
])
screens.append(frame('full', 0, 0, 1920, 1080, None, 'bind-display="menu.full"', indent(common, 1)))

# Inventory: the help line, the list (two columns of 770.7, rows 101.25, seven in view), the scroll bar; using an item, the
# item and its line in the help window and the party's places in the list's.
cells = [cell('item%d' % k, 'menu.item[%d]' % k, (k % 2) * 770.7, (k // 2) * 101.25, 770.7, 101.25, 37.15) for k in range(14)]
targets = [target('target%d' % k, 'menu.target[%d]' % k, k * 148.5) for k in range(5)]
inv = '\n'.join([
    window('help', 155.5, 87.75, 1609, 108, 'm-pane', '', '\n'.join([
        frame('helpLine', 74.4, 23.6, 1500, 60.75, 'm-text', 'bind-display="!menu.using" bind-text="{menu.help}"'),
        frame('useIcon', 74.4, 0, 36, 60.75, 'm-icon', 'bind-display="menu.using" bind-style="-ff-cell: symbol {menu.useIcon}"'),
        frame('useName', 114.8, 0, 1500, 60.75, 'm-text', 'bind-display="menu.using" bind-text="{menu.useName}"'),
        frame('useCount', 0, 0, 683, 60.75, 'm-text m-right', 'bind-display="menu.using" bind-text="{menu.useCount}"'),
        frame('useLine', 74.4, 47.25, 1500, 60.75, 'm-text', 'bind-display="menu.using" bind-text="{menu.help}"'),
    ])),
    window('list', 155.5, 209.25, 1609, 742.5, 'm-pane', '', '\n'.join([
        frame('cells', 0, 0, 1541.4, 742.5, None, 'bind-display="!menu.using"', indent('\n'.join(cells), 1)),
        scroll('scroll', 1544.8, 0, 742.5, 'menu.itemScroll'),
        frame('targets', 398.9, 0, 1210.1, 742.5, None, 'bind-display="menu.using"', indent('\n'.join(targets), 1)),
    ])),
    keys('browseKeys', '!menu.using', [(48, 929.6, '{menu.keyItemsLabel}'), (16, 1189.9, '{menu.sortLabel}'), (43, 1348.7, '{menu.confirm}'), (15, 1568.5, '{menu.back}')]),
    keys('useKeys', 'menu.using', [(43, 1413.0, '{menu.useLabel}'), (15, 1568.5, '{menu.back}')]),
])
screens.append(frame('inventory', 0, 0, 1920, 1080, None, 'bind-display="menu.inventory"', indent(inv, 1)))

# Magic: the member between the arrows, the spell's cost and line, the spells (three columns of 513.8, rows 108).
spells = [cell('spell%d' % k, 'menu.spell[%d]' % k, (k % 3) * 513.8, (k // 3) * 108, 513.8, 108, 40.5) for k in range(15)]
head = '\n'.join([
    frame('left', 27, 20.25, 27, 108, 'm-arrow-left'),
    frame('right', 1554.9, 20.25, 27, 108, 'm-arrow-right'),
    frame('face', 398.9, 6.75, 135, 135, 'm-face', 'bind-style="-ff-cell: face {menu.head.face}"'),
    frame('name', 561.1, 16.9, 400, 60.75, 'm-text', 'bind-text="{menu.head.name}"'),
    frame('lv', 561.1, 70.9, 120, 60.75, 'm-text', 'bind-text="{menu.lvLabel}"'),
    frame('level', 696.3, 70.9, 120, 60.75, 'm-text', 'bind-text="{menu.head.level}"'),
    frame('hpLabel', 858.6, 16.9, 120, 60.75, 'm-text', 'bind-text="{menu.hpLabel}"'),
    frame('hp', 900, 16.9, 183.8, 60.75, 'm-text m-right', 'bind-text="{menu.head.hp}"'),
    frame('hpSlash', 1091.8, 16.9, 30, 60.75, 'm-text', 'bind-text="/"'),
    frame('maxHp', 1115.5, 16.9, 140, 60.75, 'm-text', 'bind-text="{menu.head.maxHp}"'),
    frame('mpLabel', 858.6, 70.9, 120, 60.75, 'm-text', 'bind-text="{menu.mpLabel}"'),
    frame('mp', 900, 70.9, 183.8, 60.75, 'm-text m-right', 'bind-text="{menu.head.mp}"'),
    frame('mpSlash', 1091.8, 70.9, 30, 60.75, 'm-text', 'bind-text="/"'),
    frame('maxMp', 1115.5, 70.9, 140, 60.75, 'm-text', 'bind-text="{menu.head.maxMp}"'),
])
mag = '\n'.join([
    window('head', 155.5, 87.75, 1609, 148.5, 'm-pane', '', head),
    window('info', 155.5, 249.75, 1609, 94.5, 'm-pane', '', frame('helpLine', 96.5, 16.9, 1500, 60.75, 'm-text', 'bind-text="{menu.help}"')),
    window('list', 155.5, 357.75, 1609, 594, 'm-pane', '', '\n'.join([
        frame('cells', 0, 0, 1541.4, 594, None, '', indent('\n'.join(spells), 1)),
        scroll('scroll', 1544.8, 0, 594, 'menu.spellScroll'),
    ])),
    keys('magicKeys', 'menu.magic', [(46, 554.4, ''), (52, 642.3, '{menu.changeLabel}'), (48, 1054.6, '{menu.schoolLabel}'), (43, 1348.7, '{menu.confirm}'), (15, 1568.5, '{menu.back}')]),
])
screens.append(frame('magic', 0, 0, 1920, 1080, None, 'bind-display="menu.magic"', indent(mag, 1)))

def slots(id_, x, y, lit_band=False):
    """What a member wears: a box of five rows 67.5 high - the slot's word in a 216-wide column, the item in a sunken cell."""
    rows = []
    for k in range(5):
        src = 'menu.slot[%d]' % k
        inner = '\n'.join([
            frame('label', 0, 0, 216.3, 67.5, 'm-text m-centre', 'bind-text="{label}"'),
            frame('band', 216.3, 0, 659.3, 67.5, 'lit-band', 'bind-visible="lit"') if lit_band else '',
            frame('edge', 216.3, 0, 659.3, 67.5, 'm-edge'),
            frame('icon', 244.3, 0, 36, 67.5, 'm-icon', 'bind-style="-ff-cell: symbol {icon}"'),
            frame('name', 284.7, 0, 560, 67.5, 'm-text', 'bind-text="{name}"'),
            frame('hand', 222, 0, 1, 67.5, 'hand', 'bind-visible="lit" style="-ff-cell-origin: 0 50%"') if lit_band else '',
        ])
        inner = '\n'.join(l for l in inner.split('\n') if l)
        rows.append(frame('slot%d' % k, 0, k * 67.5, 875.6, 67.5, 'm-slot', 'data-source="%s" bind-class="lit: lit"' % src, indent(inner, 1)))
    return window(id_, x, y, 875.6, 337.5, 'm-pane', '', '\n'.join(rows))

# Status: the member (face, name, level, job, HP and MP), the figures down the left, the experience, what they wear.
stats = []
for k in range(11):
    top = 172.9 + 54 * k if k < 5 else 469.9 + 54 * (k - 5)
    stats.append(frame('stat%d' % k, 75.5, top, 488, 60.75, None, 'data-source="menu.stat[%d]"' % k, indent('\n'.join([
        frame('label', 0, 0, 488, 60.75, 'm-text', 'bind-text="{label}"'),
        frame('value', 0, 0, 488, 60.75, 'm-text m-right', 'bind-text="{value}"'),
    ]), 1)))
status = '\n'.join([
    frame('face', 74.4, 27, 135, 135, 'm-face', 'bind-style="-ff-cell: face {menu.head.face}"'),
    frame('name', 237.5, 38.9, 400, 60.75, 'm-text', 'bind-text="{menu.head.name}"'),
    frame('lv', 237.5, 92.9, 120, 60.75, 'm-text', 'bind-text="{menu.lvLabel}"'),
    frame('level', 372.7, 92.9, 120, 60.75, 'm-text', 'bind-text="{menu.head.level}"'),
    frame('job', 643.5, 38.9, 480, 60.75, 'm-text', 'bind-text="{menu.job}"'),
    frame('hpLabel', 1130.5, 38.9, 120, 60.75, 'm-text', 'bind-text="{menu.hpLabel}"'),
    frame('hp', 1189.7, 38.9, 166, 60.75, 'm-text m-right', 'bind-text="{menu.head.hp}"'),
    frame('hpSlash', 1363.8, 38.9, 30, 60.75, 'm-text', 'bind-text="/"'),
    frame('maxHp', 1387.4, 38.9, 140, 60.75, 'm-text', 'bind-text="{menu.head.maxHp}"'),
    frame('mpLabel', 1130.5, 92.9, 120, 60.75, 'm-text', 'bind-text="{menu.mpLabel}"'),
    frame('mp', 1189.7, 92.9, 166, 60.75, 'm-text m-right', 'bind-text="{menu.head.mp}"'),
    frame('mpSlash', 1363.8, 92.9, 30, 60.75, 'm-text', 'bind-text="/"'),
    frame('maxMp', 1387.4, 92.9, 140, 60.75, 'm-text', 'bind-text="{menu.head.maxMp}"'),
    '\n'.join(stats),
    frame('expLabel', 643.5, 172.9, 893, 60.75, 'm-text', 'bind-text="{menu.expLabel}"'),
    frame('expValue', 643.5, 172.9, 893, 60.75, 'm-text m-right', 'bind-text="{menu.exp}"'),
    frame('nextLabel', 643.5, 226.9, 893, 60.75, 'm-text', 'bind-text="{menu.nextLabel}"'),
    frame('nextValue', 643.5, 226.9, 893, 60.75, 'm-text m-right', 'bind-text="{menu.next}"'),
    slots('worn', 650.25, 446.65),
])
st = '\n'.join([
    window('main', 155.5, 87.75, 1609, 864, 'm-pane', '', status),
    keys('statusKeys', 'menu.status', [(43, 1357, '{menu.abilitiesLabel}'), (15, 1568.5, '{menu.back}')]),
])
screens.append(frame('status', 0, 0, 1920, 1080, None, 'bind-display="menu.status"', indent(st, 1)))

# Equipment: the member and their figures small in two columns; the lit item's line; the five slots (the hand on one) and,
# beside them, what in the bag fits the lit slot.
small = []
for k in range(11):
    col, row = (0, k) if k < 5 else (1, k - 5)
    small.append(frame('fig%d' % k, 588.5 + 486 * col, 23.75 + 27 * row, 326, 27, None, 'data-source="menu.stat[%d]"' % k, indent('\n'.join([
        frame('label', 0, 0, 326, 27, 'm-small', 'bind-text="{label}"'),
        frame('value', 0, 0, 326, 27, 'm-small m-right', 'bind-text="{value}"'),
    ]), 1)))
eq_head = '\n'.join([
    frame('face', 74.4, 27, 135, 135, 'm-face', 'bind-style="-ff-cell: face {menu.head.face}"'),
    frame('name', 237.5, 38.9, 400, 60.75, 'm-text', 'bind-text="{menu.head.name}"'),
    frame('lv', 237.5, 92.9, 120, 60.75, 'm-text', 'bind-text="{menu.lvLabel}"'),
    frame('level', 372.7, 92.9, 120, 60.75, 'm-text', 'bind-text="{menu.head.level}"'),
    '\n'.join(small),
])
eq_slots = []
for k in range(5):
    inner = '\n'.join([
        frame('label', 0, 0, 193.5, 108, 'm-text m-centre', 'bind-text="{label}"'),
        frame('band', 193.5, 0, 656.5, 108, 'lit-band', 'bind-visible="lit"'),
        frame('edge', 193.5, 0, 656.5, 108, 'm-edge'),
        frame('icon', 274.6, 0, 36, 108, 'm-icon', 'bind-style="-ff-cell: symbol {icon}"'),
        frame('name', 312.5, 0, 500, 108, 'm-cell-name', 'bind-text="{name}"'),
        frame('hand', 274.5, 0, 1, 108, 'hand', 'bind-visible="lit" style="-ff-cell-origin: 0 40.5px"'),
    ])
    eq_slots.append(frame('slot%d' % k, 0, k * 108, 850, 108, 'm-slot', 'data-source="menu.slot[%d]" bind-class="lit: lit"' % k, indent(inner, 1)))
choices = [cell('choice%d' % k, 'menu.choice[%d]' % k, 0, k * 108, 676.7, 108, 40.5) for k in range(5)]
eq = '\n'.join([
    window('head', 155.5, 87.75, 1609, 202.5, 'm-pane', '', eq_head),
    window('info', 155.5, 303.75, 1609, 94.5, 'm-pane', '', frame('helpLine', 74.4, 16.9, 1500, 60.75, 'm-text', 'bind-text="{menu.help}"')),
    window('slots', 155.5, 411.75, 850, 540, 'm-pane', '', '\n'.join(eq_slots)),
    window('choices', 1019, 411.75, 745, 540, 'm-pane', '', '\n'.join([
        frame('cells', 0, 0, 676.7, 540, None, '', indent('\n'.join(choices), 1)),
        scroll('scroll', 680.8, 0, 540, 'menu.choiceScroll'),
    ])),
    keys('equipKeys', 'menu.equipment', [(16, 884.9, '{menu.optimizeLabel}'), (48, 1121.1, '{menu.removeLabel}'), (43, 1348.7, '{menu.confirm}'), (15, 1568.5, '{menu.back}')]),
])
screens.append(frame('equipment', 0, 0, 1920, 1080, None, 'bind-display="menu.equipment"', indent(eq, 1)))

# Abilities: the member (as Magic's head), the lit command's line, the auto-battle command and the five battle commands.
def command_cell(id_, source, y, h, hand_y):
    inner = '\n'.join([
        frame('band', 0, 0, 799, h, 'lit-band', 'bind-visible="lit"'),
        frame('edge', 0, 0, 799, h, 'm-edge'),
        frame('name', 82, 0, 700, h, 'm-cell-name', 'bind-text="{name}"'),
        frame('hand', 82, 0, 1, h, 'hand', 'bind-visible="lit" style="-ff-cell-origin: 0 %spx"' % hand_y),
    ])
    return frame(id_, 805.5, y, 799, h, 'm-cell', 'data-source="%s" bind-class="lit: lit; picked: picked"' % source, indent(inner, 1))
ab_head = '\n'.join(l for l in head.split('\n') if "m-arrow" not in l)
ab = '\n'.join([
    window('head', 155.5, 87.75, 1609, 148.5, 'm-pane', '', ab_head),
    window('info', 155.5, 249.75, 1609, 94.5, 'm-pane', '', frame('helpLine', 74.4, 16.9, 1500, 60.75, 'm-text', 'bind-text="{menu.help}"')),
    window('auto', 155.5, 357.75, 1609, 108, 'm-pane', '', '\n'.join([
        frame('label', 75.5, 0, 700, 108, 'm-text', 'bind-text="{menu.autoLabel}"'),
        command_cell('autoCommand', 'menu.auto', 0, 108, 50.6),
    ])),
    window('commands', 155.5, 479.25, 1609, 472.5, 'm-pane', '', '\n'.join(
        [frame('label', 75.5, 0, 700, 472.5, 'm-text', 'bind-text="{menu.commandsLabel}"')] +
        [command_cell('command%d' % k, 'menu.slot[%d]' % k, 94.5 * k, 94.5, 44.3) for k in range(5)])),
    keys('abilityKeys', 'menu.abilities', [(43, 1348.7, '{menu.confirm}'), (15, 1568.5, '{menu.back}')]),
])
screens.append(frame('abilities', 0, 0, 1920, 1080, None, 'bind-display="menu.abilities"', indent(ab, 1)))

# Gambits (OpenFF's): the member as on Magic, the lit cell's line, six of the twelve rules - the number with ON / OFF, the
# condition, the action, a sunken cell each - or, picking, the choices in two columns in the same window.
def rule_cell(id_, x, w, lit, text_frames):
    inner = '\n'.join([
        frame('band', 0, 0, w, 99, 'lit-band', 'bind-visible="%s"' % lit),
        frame('edge', 0, 0, w, 99, 'm-edge'),
    ] + text_frames + [
        frame('hand', 60, 0, 1, 99, 'hand', 'bind-visible="%s" style="-ff-cell-origin: 0 46px"' % lit),
    ])
    return frame(id_, x, 0, w, 99, 'm-cell', '', indent(inner, 1))
rules = []
for k in range(6):
    cells = '\n'.join([
        rule_cell('state', 0, 250, 'litOn', [
            frame('number', 82, 0, 60, 99, 'm-cell-name', 'bind-text="{number}"'),
            frame('onOff', 0, 0, 226, 99, 'm-cell-count m-state', 'bind-text="{state}"'),
        ]),
        rule_cell('condition', 250, 645.7, 'litCondition', [frame('name', 82, 0, 540, 99, 'm-cell-name m-condition', 'bind-text="{condition}"')]),
        rule_cell('action', 895.7, 645.7, 'litAction', [frame('name', 82, 0, 540, 99, 'm-cell-name', 'bind-text="{action}"')]),
    ])
    rules.append(frame('rule%d' % k, 0, 99 * k, 1541.4, 99, 'm-rule', 'data-source="menu.rule[%d]" bind-class="foe: foe; ally: ally; on: on; off: off; picked: picked"' % k, indent(cells, 1)))
picks = [cell('pick%d' % k, 'menu.pick[%d]' % k, (k % 2) * 770.7, (k // 2) * 99, 770.7, 99, 46, 'lit: lit; dim: dim; foe: foe; ally: ally; gold: gold') for k in range(12)]
gb = '\n'.join([
    window('head', 155.5, 87.75, 1609, 148.5, 'm-pane', '', head),
    window('info', 155.5, 249.75, 1609, 94.5, 'm-pane', '', frame('helpLine', 74.4, 16.9, 1500, 60.75, 'm-text', 'bind-text="{menu.help}"')),
    window('list', 155.5, 357.75, 1609, 594, 'm-pane', '', '\n'.join([
        frame('rules', 0, 0, 1541.4, 594, None, 'bind-display="!menu.gambitPicking"', indent('\n'.join(rules), 1)),
        scroll('ruleScroll', 1544.8, 0, 594, 'menu.ruleScroll'),
        frame('picks', 0, 0, 1541.4, 594, None, 'bind-display="menu.gambitPicking"', indent('\n'.join(picks), 1)),
        scroll('pickScroll', 1544.8, 0, 594, 'menu.pickScroll'),
    ])),
    keys('ruleKeys', '!menu.gambitPicking', [(46, 272, ''), (52, 360, '{menu.changeLabel}'), (48, 790, '{menu.removeLabel}'), (16, 1030, '{menu.moveUpLabel}'), (43, 1348.7, '{menu.confirm}'), (15, 1568.5, '{menu.back}')]),
    keys('pickKeys', 'menu.gambitPicking', [(43, 1348.7, '{menu.confirm}'), (15, 1568.5, '{menu.back}')]),
])
screens.append(frame('gambits', 0, 0, 1920, 1080, None, 'bind-display="menu.gambits"', indent(gb, 1)))

# Party (MSSFormation): the member panel as the root's, the two pills beside it (button_00, the glove 6.7 in).
def party_pill(id_, y, label, lit):
    inner = '\n'.join([
        frame('art', 0, 0, 648.9, 121.5, 'm-pill-art'),
        frame('label', 0, 30.4, 648.9, 60.75, 'm-text m-centre', 'bind-text="{%s}"' % label),
        frame('hand', 87.7, 0, 1, 121.5, 'hand m-pill-hand', 'bind-visible="%s"' % lit),
    ])
    return frame(id_, 1122.3, y, 648.9, 121.5, 'm-pill', '', indent(inner, 1))
pt = '\n'.join([
    frame('partyMembers', 155.5, 6.75, 960, 945, None, '', indent(member_panel, 1)),
    party_pill('swapRows', 0, 'menu.swapRowsLabel', 'menu.swapRowsLit'),
    party_pill('formation', 128.2, 'menu.formationLabel', 'menu.formationLit'),
    keys('partyKeys', 'menu.party', [(43, 1348.7, '{menu.confirm}'), (15, 1568.5, '{menu.back}')]),
])
screens.append(frame('party', 0, 0, 1920, 1080, None, 'bind-display="menu.party"', indent(pt, 1)))

# Settings (Steam's, measured off its draws): five of the nine lines in view, 148.5 high - the word in a sunken cell
# 473.2 wide, the value in one beside it; Help and Quit a cell the row's width.
def setting_box(id_, k, x, w):
    inner = '\n'.join([
        frame('fill', 0, 0, w, 121.5, 'm-box-fill'),
        frame('band', 0, 0, w, 121.5, 'lit-band m-box-band', 'bind-visible="lit"'),
        frame('art', 0, 0, w, 121.5, 'm-box'),
        frame('name', 0, 0, w, 121.5, 'm-label', 'bind-text="{name}"'),
    ])
    return frame(id_, x, 13.5, w, 121.5, 'm-setting-box', 'data-source="box[%d]" bind-display="present" bind-class="lit: lit"' % k, indent(inner, 1))

setting_rows = []
for k in range(5):
    value = '\n'.join([
        frame('edge', 0, 0, 1068.2, 148.5, 'm-edge'),
        # A slider: the bar, the marker over it, the number.
        frame('slider', 0, 0, 1068.2, 148.5, None, 'bind-display="slider"', indent('\n'.join([
            frame('bar', 55, 54.85, 863.7, 79.3, 'm-slider-bar'),
            frame('marker', 0, 7.55, 79.4, 79.3, 'm-slider-marker', 'bind-style="left: {marker}px"'),
            frame('number', 941, 44.6, 100, 60.75, 'm-text m-right', 'bind-text="{number}"'),
        ]), 1)),
        # Two choices (Battle Mode, Subtitles).
        frame('pair', 0, 0, 1068.2, 148.5, None, 'bind-display="pair"', indent('\n'.join([
            setting_box('box0', 0, 40.6, 473.2), setting_box('box1', 1, 540.9, 473.2)]), 1)),
        # Battle Speed: Fast, six steps, Slow.
        frame('steps', 0, 0, 1068.2, 148.5, None, 'bind-display="steps"', indent('\n'.join(
            [frame('fast', 40.6, 43.9, 162.3, 60.75, 'm-text m-centre', 'bind-text="{fastLabel}"')] +
            [setting_box('box%d' % b, b, 202.8 + 108.2 * b, 108.2) for b in range(6)] +
            [frame('slow', 851.9, 43.9, 197, 60.75, 'm-text m-centre', 'bind-text="{slowLabel}"')]), 1)),
        # Window Design: six.
        frame('design', 0, 0, 1068.2, 148.5, None, 'bind-display="design"', indent('\n'.join(
            [setting_box('box%d' % b, b, 40.6 + 162.3 * b, 162.3) for b in range(6)]), 1)),
    ])
    inner = '\n'.join([
        frame('word', 0, 0, 473.2, 148.5, None, 'bind-display="!button"', indent('\n'.join([
            frame('edge', 0, 0, 473.2, 148.5, 'm-edge'),
            frame('label', 0, 0, 473.2, 148.5, 'm-label', 'bind-text="{label}"'),
        ]), 1)),
        frame('value', 473.2, 0, 1068.2, 148.5, None, 'bind-display="!button"', indent(value, 1)),
        frame('button', 0, 0, 1541.4, 148.5, None, 'bind-display="button"', indent('\n'.join([
            frame('edge', 0, 0, 1541.4, 148.5, 'm-edge'),
            frame('label', 0, 0, 1541.4, 148.5, 'm-label', 'bind-text="{label}"'),
        ]), 1)),
        frame('hand', 81.5, 0, 1, 148.5, 'hand', 'bind-visible="lit" style="-ff-cell-origin: 0 50%"'),
    ])
    setting_rows.append(frame('setting%d' % k, 0, 148.5 * k, 1541.4, 148.5, 'm-setting',
                              'data-source="menu.setting[%d]" bind-display="present" bind-class="lit: lit"' % k, indent(inner, 1)))
st_screen = '\n'.join([
    window('info', 155.5, 87.75, 1609, 94.5, 'm-pane', '', frame('helpLine', 74.4, 16.9, 1500, 60.75, 'm-text', 'bind-text="{menu.help}"')),
    window('list', 155.5, 195.75, 1609, 756, 'm-pane', '', '\n'.join([
        frame('lines', 0, 0, 1541.4, 756, None, '', indent('\n'.join(setting_rows), 1)),
        scroll('settingScroll', 1544.8, 0, 756, 'menu.settingScroll'),
    ])),
    keys('settingsKeys', 'menu.settings', [(15, 1568.5, '{menu.back}')]),
    keys('settingsConfirm', 'menu.settingConfirm', [(43, 1348.7, '{menu.confirm}')]),
])
screens.append(frame('settings', 0, 0, 1920, 1080, None, 'bind-display="menu.settings"', indent(st_screen, 1)))

# Save (Steam's, captured at a save point): what the lit slot holds on the left - its party by places, 162 high (face,
# name, level, HP, MP), the place, the play time and the gil - or "No save data found."; Slot 1..3 and Title Menu down
# the right as the root's commands.
save_members = []
for k in range(5):
    inner = '\n'.join([
        frame('edge', 0, 0, 960, 162, 'm-edge'),
        frame('face', 13.5, 13.5, 135, 135, 'm-face', 'bind-display="present" bind-style="-ff-cell: face {face}"'),
        frame('name', 189.3, 16.9, 400, 60.75, 'm-text', 'bind-text="{name}"'),
        frame('lv', 189.3, 70.9, 120, 60.75, 'm-text', 'bind-display="present" bind-text="{menu.lvLabel}"'),
        frame('level', 324.5, 70.9, 120, 60.75, 'm-text', 'bind-text="{level}"'),
        frame('hpLabel', 540.8, 16.9, 120, 60.75, 'm-text', 'bind-display="present" bind-text="{menu.hpLabel}"'),
        frame('hp', 600, 16.9, 166, 60.75, 'm-text m-right m-hp', 'bind-text="{hp}"'),
        frame('hpSlash', 774.1, 16.9, 30, 60.75, 'm-text', 'bind-display="present" bind-text="/"'),
        frame('maxHp', 797.7, 16.9, 140, 60.75, 'm-text', 'bind-text="{maxHp}"'),
        frame('mpLabel', 540.8, 70.9, 120, 60.75, 'm-text', 'bind-display="present" bind-text="{menu.mpLabel}"'),
        frame('mp', 600, 70.9, 166, 60.75, 'm-text m-right', 'bind-text="{mp}"'),
        frame('mpSlash', 774.1, 70.9, 30, 60.75, 'm-text', 'bind-display="present" bind-text="/"'),
        frame('maxMp', 797.7, 70.9, 140, 60.75, 'm-text', 'bind-text="{maxMp}"'),
    ])
    save_members.append(frame('member%d' % k, 0, 162 * k, 960, 162, 'm-member',
                              'data-source="menu.saveMember[%d]" bind-class="dim: dim; low: low; back: back"' % k, indent(inner, 1)))
save_info = '\n'.join([
    frame('edge', 0, 0, 960, 135, 'm-edge'),
    frame('savedPlace', 67.6, 2.9, 860, 60.75, 'm-text', 'bind-text="{menu.savePlace}"'),
    frame('time', 0, 70.9, 486.8, 60.75, 'm-text m-right', 'bind-text="{menu.saveTime}"'),
    frame('gil', 0, 70.9, 882.3, 60.75, 'm-text m-right', 'bind-text="{menu.saveGil}"'),
])
save_rows = []
for k in range(4):
    inner = '\n'.join([
        frame('band', 0, 0, 635.5, 135, 'lit-band', 'bind-visible="lit"'),
        frame('edge', 0, 0, 635.5, 135, 'm-edge'),
        frame('name', 0, 0, 635.5, 135, 'm-label', 'bind-text="{name}"'),
        frame('hand', 87.8, 0, 1, 135, 'hand', 'bind-visible="lit"'),
    ])
    save_rows.append(frame('slot%d' % k, 0, 135 * k, 635.5, 135, 'm-command',
                           'data-source="menu.saveRow[%d]" bind-display="present" bind-class="lit: lit"' % k, indent(inner, 1)))
sv = '\n'.join([
    window('savePanel', 155.5, 6.75, 960, 945, 'm-pane', '', '\n'.join([
        frame('empty', 0, 0, 960, 945, 'm-text m-centre', 'bind-display="!menu.slotFilled" bind-text="{menu.noDataLabel}"'),
        frame('held', 0, 0, 960, 945, None, 'bind-display="menu.slotFilled"', indent('\n'.join(save_members + [
            frame('info', 0, 810, 960, 135, None, '', indent(save_info, 1))]), 1)),
    ])),
    window('saveSlots', 1129, 6.75, 635.5, 540, 'm-pane', '', '\n'.join(save_rows)),
    keys('saveKeys', 'menu.save', [(43, 1348.7, '{menu.confirm}'), (15, 1568.5, '{menu.back}')]),
])
screens.append(frame('save', 0, 0, 1920, 1080, None, 'bind-display="menu.save"', indent(sv, 1)))
