"""python tools/mastery.py - the Mastery sample's screens (jobs, abilities, job-confirm) in the Starlit Menu's look. Their
definitions (menus/mastery_<screen>.json) say "restyles": "Mastery": with Mastery installed each takes the place of Mastery's
screen of its id - Mastery's main menu entry, hero pick and code (JobsScreen, AbilitiesScreen, JobConfirm) kept - and without
it they are passed over. Mastery's code writes its texts by frame id, so every frame of Mastery's it writes to is copied as
Mastery's layout has it - its behaviour, its neighbours - then moved into the Starlit Menu's frame of the screen (the header
above, the bottom bar below) and given classes; the panels and the header are the mod's. Reads ../../Mastery/menus/<screen>.xml, the sample beside this one."""
import copy, os
import xml.etree.ElementTree as ET
from common import frame, panel, bottom, write

MASTERY = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "Mastery", "menus"))


def mastery(name):
    root = ET.parse(os.path.join(MASTERY, name + ".xml")).getroot()
    return root if root.tag == "menu" else root.find("menu")


def take(src, fid, x, y, w, h, cls):
    """One of Mastery's frames, copied with what it says (behaviour, data, neighbours), placed and given a class; its own frames dropped (they are taken one by one)."""
    f = copy.deepcopy([e for e in src.iter("frame") if e.findtext("id") == fid][0])
    for child in f.findall("frame"):
        f.remove(child)
    for tag, val in (("x", x), ("y", y), ("width", w), ("height", h)):
        e = f.find(tag)
        if e is None: e = ET.SubElement(f, tag)
        e.text = str(val)
    f.set("class", cls)
    return f


def lit(f):
    """A frame the hand lands on, lit while it is there (slotitem's highlight, behind its text)."""
    w, h = int(f.findtext("width")), int(f.findtext("height"))
    f.append(frame(f.findtext("id") + "_hl", -6, 0, w + 10, h, "hl"))
    return f


def window(fid, x, y, w, h, children):
    """One of Mastery's windows as a navy panel in a gold frame, its texts in it."""
    p = panel(fid, x, y, w, h)
    for c in children: p.append(c)
    return p


def chrome(title, help_text):
    """What every Starlit screen has: the crystal and the title, the help line, the gold rule (the castle behind is the sheet's)."""
    return [
        frame("crystal", 9, 4, 26, 26, "crystal"),
        frame("title", 40, 7, 80, 20, "title", text=True, data=title),
        frame("hsep", 122, 11, 1, 12, "hsep"),
        frame("help", 129, 10, 340, 16, "help", text=True, data=help_text),
        frame("hline", 8, 31, 464, 1, "hline"),
    ]


def hero_bar(src, at_left):
    """Mastery's top window - the hero, the job, the ABP - as the Starlit hero bar, the portrait at its left."""
    return [window("w_top", 8, 39, 464, 44, [
        take(src, "hero", at_left, 0, 200, 22, "name"),
        take(src, "job", at_left, 22, 200, 22, "job"),
        take(src, "abp_label", 236, 0, 220, 22, "label2"),
        take(src, "abp", 236, 22, 220, 22, "value2"),
    ]), frame("face", 14, 46, 30, 30, "face", extra="portrait")]


def desc(src):
    return window("w_desc", 8, 247, 464, 36, [
        take(src, "desc1", 8, 0, 448, 18, "captiontext"),
        take(src, "desc2", 8, 18, 448, 18, "captiontext"),
    ])


def menu(name, frames):
    m = ET.Element("menu")
    ET.SubElement(m, "name").text = name
    for f in frames: m.append(f)
    return m


# Jobs: twelve jobs a page, three columns of four rows; each job's level and ABP under its name, in the grid's window.
src = mastery("jobs")
grid = [take(src, f"lv{i}", 42 + (i % 3) * 152, 18 + (i // 3) * 32, 112, 14, "label2") for i in range(12)]
grid.append(take(src, "page", 8, 130, 448, 16, "hint"))
jobs = chrome("Jobs", "A job and the abilities its ladder teaches.") + hero_bar(src, 52) + [
    window("w_grid", 8, 91, 464, 148, grid),
    desc(src),
    bottom(),
] + [lit(take(src, f"job{i}", 50 + (i % 3) * 152, 93 + (i // 3) * 32, 112, 16, "slotitem")) for i in range(12)]

# Abilities: the job's commands and the hero's free slots at the left, what has been learned at the right.
src = mastery("abilities")
left = [take(src, "cmd_title", 8, 2, 168, 18, "subtitle")]
left += [take(src, f"cmd{i}", 42, 20 + i * 18, 136, 18, "listrow") for i in range(3)]
left.append(take(src, "slot_title", 8, 76, 168, 18, "subtitle"))
right = [take(src, "list_title", 8, 2, 120, 18, "subtitle"), take(src, "page", 132, 2, 132, 18, "hint")]
abilities = chrome("Abilities", "What the hero has learned, and the slots it goes in.") + hero_bar(src, 52) + [
    window("w_left", 8, 91, 184, 148, left),
    window("w_list", 200, 91, 272, 148, right),
    desc(src),
    bottom(),
] + [lit(take(src, f"slot{i}", 50, 185 + i * 18, 136, 18, "slotitem")) for i in range(3)] \
  + [lit(take(src, f"opt{i}", 246, 113 + i * 21, 220, 20, "slotitem")) for i in range(6)]

# The confirmation: the job asked about in a window of its own, Yes and No under it.
src = mastery("job-confirm")
confirm = chrome("Jobs", "Change to this job?") + hero_bar(src, 52) + [
    window("w_ask", 104, 100, 272, 112, [
        take(src, "ask_job", 16, 12, 240, 24, "subtitle"),
        take(src, "ask", 16, 44, 240, 24, "label2"),
    ]),
    desc(src),
    bottom(),
    lit(take(src, "yes", 140, 176, 80, 24, "slotitem")),
    lit(take(src, "no", 260, 176, 80, 24, "slotitem")),
]

for name, frames in (("jobs", jobs), ("abilities", abilities), ("job-confirm", confirm)):
    write("mastery_" + name + ".xml",
          "Mastery's " + name + " screen in the Starlit Menu's look, in place of Mastery's own when it is installed "
          "(restyles: the frames Mastery's code writes to are Mastery's, placed and styled; the panels and the header the mod's). "
          "Generated by tools/mastery.py.", [menu(name, frames)])
print("ok")
