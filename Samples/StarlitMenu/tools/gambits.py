"""python tools/gambits.py - the Gambits screen of the Starlit Menu: the client's own screen (OpenFF/Data/menus/gambits.xml), taken
into the mod by its id and laid out again. GambitsScreen writes its texts by id - help, hero (and hero_hint when there is one),
num0.. / on0.. / cond0.. / act0.. for the seven rows - and puts the scroll arrows at the right of w_row0 .. w_row6; the cells stay
at the screen's top level so their neighbours reach each other."""
from common import frame, cursor, panel, bottom, write
import xml.etree.ElementTree as ET

ROWS = 7
m = ET.Element("menu")
ET.SubElement(m, "name").text = "gambits"

# A frame holds at most 32 frames, the screen too: the cells the cursor lands on (21) stay at its top level, so that their
# neighbours reach each other; the rest goes in a few groups (a group at 0,0 is only a holder).
def group(gid, children):
    return frame(gid, 0, 0, 0, 0, None, children)

rows, marks, seps = [], [], []
for r in range(ROWS):
    y = 84 + r * 28
    cells = " || ".join(f"menu.focused == '{c}{r}'" for c in ("on", "cond", "act"))
    rows.append(frame(f"w_row{r}", 8, y, 464, 25, "rowpanel", attrs={"bind-class": "lit: " + cells}))
    marks.append(frame(f"marker{r}", 14, y + 7, 11, 11, "marker"))
    marks.append(frame(f"num{r}", 28, y + 3, 18, 19, "num", text=True))
    seps.append(frame(f"sep{r}a", 104, y + 5, 1, 15, "vsep"))
    seps.append(frame(f"sep{r}b", 318, y + 5, 1, 15, "vsep"))
m.append(group("chrome", [
    panel("w_top", 8, 38, 464, 38),
    # The header: the crystal, the title, and what the cell under the hand does (the screen writes help).
    frame("crystal", 9, 4, 26, 26, "crystal"),
    frame("title", 40, 7, 80, 20, "title", text=True, data="Gambits"),
    frame("hsep", 122, 11, 1, 12, "hsep"),
    frame("help", 129, 10, 340, 16, "help", text=True),
    frame("hline", 8, 31, 464, 1, "hline"),
    # The hero's bar.
    frame("portrait", 14, 40, 34, 34, "face", extra="portrait"),
    frame("hero", 56, 45, 240, 22, "name", text=True),
    frame("hero_hint", 296, 48, 168, 16, "hint", text=True),
]))
m.append(group("rows", rows))
m.append(bottom())
m.append(group("marks", marks))
m.append(group("seps", seps))

# The cells the cursor lands on: ON / OFF, the condition, the action.
for r in range(ROWS):
    y = 84 + r * 28
    up = lambda c: f"{c}{r - 1}" if r > 0 else "dummy"
    down = lambda c: f"{c}{r + 1}" if r < ROWS - 1 else "dummy"
    m.append(frame(f"on{r}", 50, y + 3, 42, 19, "onpill", text=True, focus=True,
                   sides={"up": up("on"), "down": down("on"), "left": f"act{r}", "right": f"cond{r}"},
                   children=[cursor(f"cursor_on{r}", -38, 9)]))
    m.append(frame(f"cond{r}", 114, y + 3, 196, 19, "cond", text=True, focus=True,
                   sides={"up": up("cond"), "down": down("cond"), "left": f"on{r}", "right": f"act{r}"},
                   children=[cursor(f"cursor_cond{r}", -6, 9)]))
    m.append(frame(f"act{r}", 330, y + 3, 132, 19, "act", text=True, focus=True,
                   sides={"up": up("act"), "down": down("act"), "left": f"cond{r}", "right": f"on{r}"},
                   children=[cursor(f"cursor_act{r}", -6, 9)]))

write("gambits.xml",
      "The Gambits screen, the client's own taken into the mod (a mod's screen of its id takes the client's place): the ids "
      "GambitsScreen writes kept, laid out after the Starlit Menu's other screens. Generated.", [m])
print("ok")
