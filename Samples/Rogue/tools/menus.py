# Rogue Mode's menu screens: writes menus/<screen>.xml (the game's menulist format) and .json for each.
# The look is menus/styles/rogue.css; the pictures are menus/images (tools/art.py paints them).
# Run from anywhere: python Samples/Rogue/tools/menus.py
#
# The canvas is 480 x 288 with the bottom bar's 32 under it (the game's Back). Every screen has the same parts:
#   bg        the act's backdrop over all 480 x 320 (a class from code: hub, cave, woods, tower)
#   veil      a darkening at the top and the bottom, for the header and the help to read on
#   header    the title at the left, a line at the right, a rule under them
#   help      the description of what the cursor is on, two lines at the foot
# A frame the cursor lands on is at the top level (the cursor moves within its parent's subtree) and carries
# a highlight (hl_<id>, lit by :focus in the sheet) and a cursor point (cur_<id>) where the hand stands - the
# hand reaches some 30 units to the left of that point, so every point has that much room.
import json, os
import xml.etree.ElementTree as ET
from xml.dom import minidom

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "menus")


def frame(fid, x, y, w, h, cls=None, children=(), text=None, focus=False, sides=None, attrs=None, cursor=False):
    """A frame; text (a string, even "") makes it a Text frame showing that literal until code writes it."""
    e = ET.Element("frame")
    if cls: e.set("class", cls)
    for k, v in (attrs or {}).items(): e.set(k, v)
    if focus: ET.SubElement(e, "focus")
    if cursor: ET.SubElement(e, "cursor")
    for tag, val in (("id", fid), ("x", x), ("y", y), ("width", w), ("height", h)):
        ET.SubElement(e, tag).text = val if isinstance(val, str) else str(int(round(val)))
    for side in ("up", "down", "left", "right"):
        if sides and side in sides: ET.SubElement(e, side).text = sides[side]
    if text is not None:
        b = ET.SubElement(e, "behavior", value="Text")
        for p in (-1, 8, 0): ET.SubElement(b, "parameter").text = str(p)
        ET.SubElement(e, "data").text = text if text else " "
    for c in children: e.append(c)
    return e


def label(fid, x, y, w, h, cls, text="", attrs=None):
    return frame(fid, x, y, w, h, cls, text=text, attrs=attrs)


def option(fid, x, y, w, h, sides, text="", cls="opt", children=(), hl=(-10, 0, None, None), cur=(-6, None)):
    """A row the cursor lands on: its text, a highlight behind it and the hand's point at its left, centred."""
    hx, hy, hw, hh = hl
    cx, cy = cur
    kids = [frame("hl_" + fid, hx, hy, hw if hw is not None else w - hx + 6, hh if hh is not None else h, "hl"),
            frame("cur_" + fid, cx, cy if cy is not None else h / 2, 1, 1, cursor=True)]
    s = dict(left="dummy", right="dummy")
    s.update(sides)
    return frame(fid, x, y, w, h, cls, list(kids) + list(children), text=text, focus=True, sides=s)


def panel(fid, x, y, w, h, cls="panel", children=()):
    return frame(fid, x, y, w, h, cls, children)


def icon(fid, x, y, size, cls):
    return frame(fid, x, y, size, size, "icon " + cls)


def common(title_text="", right=""):
    """The backdrop, the veil, the header."""
    return [
        frame("bg", 0, 0, 480, 320, "bg hub"),
        frame("veil", 0, 0, 480, 320, "veil"),
        label("title", 14, 6, 300, 26, "title", title_text),
        label("hright", 200, 9, 266, 22, "hright", right),
        frame("rule", 10, 36, 460, 1, "rule"),
    ]


def help_bar(y=250):
    return panel("w_help", 8, y, 464, 34, "panel help", [
        label("desc1", 12, 3, 440, 14, "desc"),
        label("desc2", 12, 17, 440, 14, "desc dim"),
    ])


def write(name, comment, frames, behaviour, title):
    menu = ET.Element("menu")
    ET.SubElement(menu, "name").text = name
    for f in frames: menu.append(f)
    root = ET.Element("menulist")
    root.append(menu)
    body = minidom.parseString(ET.tostring(root, encoding="unicode")).toprettyxml(indent="  ")
    body = body.replace('<?xml version="1.0" ?>', '<?xml version="1.0" encoding="utf-8"?>\n<!-- ' + comment + ' -->', 1)
    with open(os.path.join(OUT, name + ".xml"), "w", encoding="utf-8", newline="\n") as f: f.write(body)
    d = {"id": name, "layout": name + ".xml", "screen": name, "title": title, "background": -1, "backdropLines": "none",
         "attachments": [{"target": "", "behaviour": behaviour}]}
    with open(os.path.join(OUT, name + ".json"), "w", encoding="utf-8", newline="\n") as f: f.write(json.dumps(d, indent=2) + "\n")


# ---------------------------------------------------------------------------------------------- rogue: the mode's menu
write("rogue", "Rogue Mode's own menu: New Run, Continue Run. RogueHub (RogueScreens.cs) drives it.", [
    frame("bg", 0, 0, 480, 320, "bg hub"),
    frame("veil", 0, 0, 480, 320, "veil"),
    frame("halo", 90, 26, 300, 110, "halo"),
    label("title", 0, 50, 480, 40, "logo", "ROGUE MODE"),
    label("tagline", 0, 90, 480, 16, "tagline", "A run of Final Fantasy III's battles"),
    frame("orn", 150, 114, 180, 1, "rule"),
    option("new", 170, 128, 140, 26, dict(up="continue", down="continue"), "New Run", "btn", hl=(0, 0, 140, 26), cur=(-4, None)),
    option("continue", 170, 160, 140, 26, dict(up="new", down="new"), "Continue Run", "btn", hl=(0, 0, 140, 26), cur=(-4, None)),
    label("sub", 0, 200, 480, 14, "acts", ""),
    label("runinfo", 0, 216, 480, 14, "acts dim", ""),
    help_bar(),
], "RogueHub", "Rogue Mode")

# ---------------------------------------------------------------------------------------------- rogue-party: a new run
rows = []
for h in range(4):
    y = 46 + 34 * h
    order = ["hero0", "hero1", "hero2", "hero3", "begin"]
    up = order[(order.index("hero%d" % h) - 1) % len(order)]
    down = order[order.index("hero%d" % h) + 1]
    rows.append(option("hero%d" % h, 40, y, 400, 30, dict(up=up, down=down), "", "card hero", hl=(0, 0, 400, 30), cur=(-4, None), children=[
        frame("face%d" % h, 4, 3, 24, 24, "face"),
        label("name%d" % h, 36, 0, 84, 30, "name", ""),
        label("larrow%d" % h, 128, 0, 16, 30, "arrow", "<"),
        label("job%d" % h, 144, 0, 140, 30, "jobname", ""),
        label("rarrow%d" % h, 284, 0, 16, 30, "arrow", ">"),
        label("costlbl%d" % h, 306, 0, 50, 30, "costlbl", "cost"),
        frame("badge%d" % h, 360, 5, 20, 20, "badge"),
        label("cost%d" % h, 360, 5, 20, 20, "badgetext", ""),
    ]))
write("rogue-party", "A new run: each hero's starting job (left / right), the cost against the budget, the seed. RogueParty drives it.", [
    *common("New Run"),
    *rows,
    option("seed", 40, 186, 176, 26, dict(up="hero3", down="hero0", left="begin", right="begin"), "", "btn", hl=(0, 0, 176, 26), cur=(-4, None)),
    option("begin", 264, 186, 176, 26, dict(up="hero3", down="hero0", left="seed", right="seed"), "Begin the run", "btn primary", hl=(0, 0, 176, 26), cur=(-4, None)),
    help_bar(),
], "RogueParty", "New Run")

# ---------------------------------------------------------------------------------------------- rogue-camp: between battles
road = []
for i in range(10):
    road.append(frame("pip%d" % i, 20 + 26 * i, 44, 14, 14, "pip off", [
        frame("seg%d" % i, -13, 6, 13, 2, "seg"),
        icon("pi%d" % i, 1, 1, 12, "none"),
    ]))
members = []
for h in range(4):
    y = 5 + 37 * h
    members.append(frame("m%d" % h, 6, y, 280, 35, "member", attrs={"data-source": "party[%d]" % h, "bind-class": "ko: !alive"}, children=[
        frame("mface%d" % h, 2, 2, 31, 31, "face", attrs={"bind-style": "background-image: {face}"}),
        label("mname%d" % h, 42, 2, 104, 16, "name", "", attrs={"bind-text": "{name}"}),
        label("mjob%d" % h, 42, 18, 104, 14, "small dim", "", attrs={"bind-text": "{jobTitle}"}),
        label("mlv%d" % h, 150, 2, 46, 16, "small", "", attrs={"bind-text": "Lv {level}"}),
        label("mhp%d" % h, 196, 2, 80, 16, "small right", "", attrs={"bind-text": "{hp} / {maxHp}"}),
        frame("mbar%d" % h, 150, 22, 126, 6, "bar", [
            frame("mfill%d" % h, 0, 0, 126, 6, "fill", attrs={"bind-style": "width: {hpPercent}%", "bind-class": "low: hpPercent < 30"})]),
    ]))
write("rogue-camp", "Between battles: the act's road, the party, the battle ahead; Fight, Active Effects, Abandon. RogueCamp drives it.", [
    *common(),
    *road,
    icon("i_gil", 318, 43, 16, "gil"),
    label("gil", 336, 41, 64, 20, "small", ""),
    icon("i_fx", 404, 43, 16, "passive"),
    label("fx", 422, 41, 50, 20, "small", ""),
    panel("w_party", 8, 64, 292, 156, "panel", members),
    panel("w_cmd", 306, 64, 166, 106),
    option("fight", 348, 69, 118, 26, dict(up="abandon", down="party"), "Fight", "opt big", hl=(-6, 0, 124, 26), cur=(-14, None)),
    option("party", 348, 96, 118, 23, dict(up="fight", down="effects"), "Party", hl=(-6, 0, 124, 23), cur=(-14, None)),
    option("effects", 348, 120, 118, 23, dict(up="party", down="abandon"), "Active Effects", hl=(-6, 0, 124, 23), cur=(-14, None)),
    option("abandon", 348, 144, 118, 23, dict(up="effects", down="fight"), "Abandon the run", "opt warn", hl=(-6, 0, 124, 23), cur=(-14, None)),
    panel("w_next", 306, 174, 166, 46, "panel", [
        label("nkind", 8, 3, 150, 12, "kind", ""),
        label("next", 8, 16, 150, 14, "small", ""),
        label("next2", 8, 30, 150, 14, "small", ""),
    ]),
    help_bar(),
], "RogueCamp", "Camp")

# ---------------------------------------------------------------------------------------------- rogue-reward: a victory's choices
cards = []
for i in range(3):
    x = 38 + 146 * i
    lines = [label("text%d_%d" % (i, l), 8, 72 + 13 * l, 120, 13, "cardtext", "") for l in range(7)]
    cards.append(option("card%d" % i, x, 44, 136, 176, dict(up="reroll", down="reroll", left="card%d" % ((i + 2) % 3), right="card%d" % ((i + 1) % 3)),
                        # No hand on the cards: the one the cursor is on rises and glows (the hand's point is put off the screen).
                        "", "card reward", hl=(0, 0, 136, 176), cur=(-900, 30), children=[
        frame("band%d" % i, 1, 1, 134, 40, "band"),
        icon("icon%d" % i, 8, 8, 28, "none"),
        label("rarity%d" % i, 42, 9, 90, 12, "rarity", ""),
        label("label%d" % i, 42, 22, 90, 12, "cardlbl", ""),
        label("name%d" % i, 8, 44, 120, 18, "cardname", ""),
        frame("sep%d" % i, 8, 65, 120, 1, "rule"),
        *lines,
    ]))
write("rogue-reward", "A victory's three choices side by side, and a reroll for gil. RogueReward drives it.", [
    *common("Victory!", "Choose one"),
    *cards,
    option("reroll", 92, 226, 170, 20, dict(up="card0", down="card0"), "", "opt", hl=(-26, 0, 196, 20), cur=(-28, None),
           children=[icon("i_reroll", -22, 2, 16, "reroll")]),
    icon("i_gil", 330, 228, 16, "gil"),
    label("gil", 350, 226, 116, 20, "small", ""),
    help_bar(),
], "RogueReward", "Rewards")

# ---------------------------------------------------------------------------------------------- rogue-effects: what the run holds
lrows = []
for r in range(6):
    y = 50 + 31 * r
    lrows.append(option("row%d" % r, 74, y, 370, 16, dict(up="row%d" % ((r + 5) % 6), down="row%d" % ((r + 1) % 6)), "", "opt",
                        hl=(-28, -2, 396, 30), cur=(-30, None), children=[icon("star%d" % r, -24, 1, 14, "passive")]))
    lrows.append(label("sub%d" % r, 74, y + 15, 370, 13, "small dim", ""))
write("rogue-effects", "The run's modifiers in full, six a page. RogueEffects drives it.", [
    *common("Active Effects"),
    panel("w_list", 8, 44, 464, 200),
    *lrows,
    label("none", 0, 130, 480, 20, "empty", ""),
    label("page", 300, 228, 160, 14, "small dim right", ""),
    help_bar(),
], "RogueEffects", "Active Effects")

# ---------------------------------------------------------------------------------------------- rogue-summary: a run's end
tiles = []
for i in range(6):
    col, row = i % 3, i // 3
    x, y = 8 + 152 * col, 8 + 44 * row
    tiles.append(frame("tile%d" % i, x, y, 144, 38, "tile", [
        label("tl%d" % i, 8, 3, 128, 12, "tilelbl", ""),
        label("tv%d" % i, 8, 15, 128, 20, "tileval", ""),
    ]))
party = []
for h in range(4):
    party.append(frame("sface%d" % h, 8 + 114 * h, 98, 26, 26, "face"))
    party.append(label("sjob%d" % h, 38 + 114 * h, 98, 82, 26, "small", ""))
write("rogue-summary", "A run's end: how far it went and what it did. RogueSummary drives it.", [
    frame("bg", 0, 0, 480, 320, "bg hub"),
    frame("veil", 0, 0, 480, 320, "veil"),
    label("title", 0, 8, 480, 30, "verdict", ""),
    label("reached", 0, 38, 480, 14, "acts", ""),
    panel("w_stats", 8, 58, 464, 158, "panel", [
        *tiles,
        *party,
        label("fx1", 10, 128, 444, 13, "small dim", ""),
        label("fx2", 10, 141, 444, 13, "small dim", ""),
    ]),
    option("ok", 160, 222, 160, 22, dict(up="ok", down="ok"), "Back to Rogue Mode", "btn", hl=(0, 0, 160, 22), cur=(-4, None)),
    help_bar(),
], "RogueSummary", "Run Summary")

# ---------------------------------------------------------------------------------------------- rogue-heroes: the party in full
heroes = []
for h in range(4):
    y = 44 + 50 * h
    order = ["hero0", "hero1", "hero2", "hero3"]
    heroes.append(option("hero%d" % h, 40, y, 104, 46, dict(up=order[(h + 3) % 4], down=order[(h + 1) % 4]), "", "card pick", hl=(0, 0, 104, 46), cur=(-4, None),
                         children=[
        frame("hface%d" % h, 4, 6, 34, 34, "face", attrs={"data-source": "party[%d]" % h, "bind-style": "background-image: {face}"}),
        label("hname%d" % h, 44, 6, 58, 16, "name", "", attrs={"data-source": "party[%d]" % h, "bind-text": "{name}"}),
        label("hlv%d" % h, 44, 24, 58, 14, "small dim", "", attrs={"data-source": "party[%d]" % h, "bind-text": "Lv {level}"}),
    ]))
stats = []
for i in range(8):
    col, row = i // 4, i % 4
    stats.append(label("sl%d" % i, 10 + 72 * col, 92 + 18 * row, 52, 16, "small dim", ""))
    stats.append(label("sv%d" % i, 54 + 72 * col, 92 + 18 * row, 16, 16, "small right", ""))
equip = []
for i in range(5):
    equip.append(icon("ei%d" % i, 160, 93 + 18 * i, 14, "none"))
    equip.append(label("ev%d" % i, 178, 92 + 18 * i, 132, 16, "small", ""))
write("rogue-heroes", "The party in full: each hero's job and level, HP and magic, stats and equipment; a job change. RogueHeroes drives it.", [
    *common("Party"),
    *heroes,
    panel("w_hero", 150, 44, 322, 200, "panel", [
        frame("dface", 8, 8, 48, 48, "face"),
        label("dname", 64, 6, 160, 20, "bigname", ""),
        label("djob", 64, 26, 170, 14, "small dim", ""),
        label("dlv", 236, 6, 76, 20, "biglv", ""),
        label("dnext", 200, 26, 112, 14, "small dim right", ""),
        frame("dexp", 64, 44, 248, 4, "bar", [frame("dexpfill", 0, 0, 248, 4, "fill exp")]),
        label("dhpl", 10, 58, 30, 16, "small dim", "HP"),
        frame("dhp", 40, 63, 170, 6, "bar", [frame("dhpfill", 0, 0, 170, 6, "fill")]),
        label("dhpv", 214, 58, 98, 16, "small right", ""),
        label("dmp", 10, 74, 302, 16, "small", ""),
        frame("dsep", 8, 89, 306, 1, "rule"),
        *stats,
        frame("dvsep", 152, 94, 1, 86, "vrule"),
        *equip,
    ]),
    help_bar(),
], "RogueHeroes", "Party")

# ---------------------------------------------------------------------------------------------- rogue-job: a hero's job change
cells = []
for i in range(16):
    col, row = i % 2, i // 2
    x, y = 74 + 200 * col, 92 + 18 * row
    up = "j%d" % ((i - 2) % 16)
    down = "j%d" % ((i + 2) % 16)
    side = "j%d" % (i + 1 if col == 0 else i - 1)
    cells.append(option("j%d" % i, x, y, 170, 17, dict(up=up, down=down, left=side, right=side), "", "opt job",
                        hl=(-10, 0, 186, 17), cur=(-14, None)))
write("rogue-job", "A hero's job, among those the crystals have opened; the game's own job change scene plays. RogueJob drives it.", [
    *common("Job"),
    panel("w_jobs", 8, 44, 464, 200, "panel", [
        frame("jface", 10, 6, 30, 30, "face"),
        label("jwho", 48, 4, 300, 16, "name", ""),
        label("jnow", 48, 20, 300, 14, "small dim", ""),
        frame("jsep", 10, 40, 444, 1, "rule"),
    ]),
    *cells,
    help_bar(),
], "RogueJob", "Job")

print("wrote", len([f for f in os.listdir(OUT) if f.endswith((".xml", ".json"))]), "files to", os.path.normpath(OUT))
