"""What the screen scripts share: the game's screens read from a decoded MenuDefine.xml, frames of the mod's made, and a
layout written. A game's frame is copied as it is (its works, tags, neighbours and behaviour - the screen's code reads them),
then moved and given classes; the frames of the mod's are added around it."""
import copy, os, sys, xml.dom.minidom
import xml.etree.ElementTree as ET

# The game's MenuDefine.xbn decoded - the player's own, so not the sample's:
#   crystal xbn "<Steam install>/files/MenuDefine.xbn" <somewhere>/MenuDefine.xml
#   python tools/<screen>.py <somewhere>/MenuDefine.xml
ROOT = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "menus"))
_game = None

def screen(name):
    """One of the game's screens, copied (from the MenuDefine.xml named on the command line)."""
    global _game
    if _game is None:
        if len(sys.argv) < 2:
            sys.exit("usage: python " + os.path.basename(sys.argv[0]) + " <MenuDefine.xml: the game's MenuDefine.xbn decoded by crystal xbn>")
        _game = ET.parse(sys.argv[1]).getroot()
    return copy.deepcopy([m for m in _game.findall("menu") if m.findtext("name") == name][0])

def find(menu, fid):
    return [f for f in menu.iter("frame") if f.findtext("id") == fid][0]

def place(f, x, y, w=None, h=None, cls=None):
    f.find("x").text = str(x); f.find("y").text = str(y)
    if w is not None: f.find("width").text = str(w)
    if h is not None: f.find("height").text = str(h)
    if cls: f.set("class", cls)
    return f

def link(f, **sides):
    """A frame's neighbours set (up, down, left, right)."""
    for side, target in sides.items():
        e = f.find(side)
        if e is None: e = ET.SubElement(f, side)
        e.text = target
    return f

def frame(fid, x, y, w, h, cls=None, children=(), attrs=None, data=None, text=False, bind=None, extra=None, focus=False, sides=None):
    """A frame of the mod's; text=True (or a bind) makes it a text (the Text behaviour, message -1); focus=True one the cursor lands
    on, its neighbours in sides (up, down, left, right)."""
    e = ET.Element("frame")
    if focus: ET.SubElement(e, "focus")
    if cls: e.set("class", cls)
    for k, v in (attrs or {}).items(): e.set(k, v)
    if bind: e.set("bind-text", bind)
    for tag, val in (("id", fid), ("x", x), ("y", y), ("width", w), ("height", h)):
        ET.SubElement(e, tag).text = str(val)
    for side, target in (sides or {}).items(): ET.SubElement(e, side).text = target
    if extra is not None: ET.SubElement(e, extra)
    if text or bind:
        b = ET.SubElement(e, "behavior", value="Text")
        for p in (-1, 8, 0): ET.SubElement(b, "parameter").text = str(p)
        if not bind: ET.SubElement(e, "data").text = data if data is not None else " "
    for c in children: e.append(c)
    return e

def cursor(fid, x, y):
    """Where the hand stands on the frame it is in (OpenFF's <cursor/>): its point, the fingertip a few units right of it."""
    return frame(fid, x, y, 1, 1, extra="cursor")

def panel(fid, x, y, w, h, cls=""):
    """A navy panel, its gold frame a frame of its own round it."""
    return frame(fid, x, y, w, h, ("panel " + cls).strip(), [frame(fid + "_frame", -5, -5, w + 10, h + 10, "goldframe")])

def bar(fid, x, y, w, h, cls, percent):
    """A bar and its fill, the fill's width bound to a percent."""
    return frame(fid, x, y, w, h, "bar " + cls, [frame(fid + "_fill", 0, 0, w, h, "fill", attrs={"bind-style": "width: {%s}%%" % percent})])

def bottom():
    """The bottom bar's panel (the Back button is field_hud's)."""
    return frame("bottom", 8, 290, 464, 24, "panel bottom", [frame("bottom_frame", -5, -5, 474, 34, "goldframe")])

def front(menu, *frames):
    """Frames put first in the screen, after its name: the windows stack in the screen's order, so the game's frames come in front of them."""
    at = list(menu).index(menu.find("name")) + 1
    for i, f in enumerate(frames): menu.insert(at + i, f)

def write(name, comment, menus):
    root = ET.Element("menulist")
    for m in menus: root.append(m)
    s = xml.dom.minidom.parseString(ET.tostring(root, encoding="utf-8")).toprettyxml(indent="  ", encoding="utf-8").decode("utf-8")
    lines = [l for l in s.splitlines() if l.strip()]
    lines.insert(1, "<!-- " + comment + " -->")
    open(os.path.join(ROOT, name), "w", encoding="utf-8", newline="\n").write("\n".join(lines) + "\n")
