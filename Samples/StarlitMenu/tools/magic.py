"""python tools/magic.py <MenuDefine.xml> - the Magic screen (magic) of the Starlit Menu: the game's tabs (Use, Learn, Remove,
Exchange) and its list (PramMagic draws each level's charges and its three spells in the cells 1-1 .. 8-3), moved and styled;
the hero's panel, the list's lines and the icons the mod's. It replaces the game's screen (a replacing layout keeps the tags its frames have)."""
from common import screen, find, place, frame, cursor, panel, bar, bottom, front, write

m = screen("magic")
place(find(m, "menu_tag_name"), 40, 7, 70, 20, "title")
m.append(frame("crystal", 9, 4, 26, 26, "crystal"))
m.append(frame("hsep", 116, 11, 1, 12, "hsep"))
m.append(frame("help", 123, 10, 346, 16, "help", text=True, data="Use, learn, remove, or exchange magic."))
m.append(frame("hline", 8, 31, 464, 1, "hline"))
front(m, panel("heropanel", 8, 67, 464, 56), panel("listpanel", 8, 130, 464, 152), bottom())

# The tabs, the width of the screen, each with its icon.
place(find(m, "mm_command"), 0, 0)
for i, (tid, icon) in enumerate((("m_use", "sparkle"), ("m_runing", "book"), ("m_nouse", "trash"), ("m_change", "exchange"))):
    x = 8 + i * 117
    place(find(m, tid), x, 38, 113, 24, "tab2").append(cursor("cursor_" + tid, 8, 12))
    m.append(frame(tid + "_icon", x + 8, 42, 16, 16, "sicon s_" + icon))

# The hero: the game's name and job, the rest bound.
place(find(m, "char_name"), 66, 70, 120, 16, "name smallname")
place(find(m, "char_job"), 66, 86, 120, 14, "job")
m.append(frame("own", 0, 0, 0, 0, attrs={"data-source": "hero"}, children=[
    frame("face", 14, 72, 46, 46, "face", extra="portrait"),
    frame("lvword", 66, 101, 40, 16, "lv", bind="Lv. {level}"),
    bar("expbar", 104, 108, 80, 3, "exp", "expPercent"),
    frame("vsep1", 196, 74, 1, 40, "vsep"),
    frame("hpword", 206, 72, 22, 14, "label", text=True, data="HP"),
    frame("hpvalue", 230, 71, 90, 14, "hp", bind="{hp} / {maxHp}"),
    bar("hpbar", 230, 88, 90, 4, "hpbar", "hpPercent"),
    frame("vsep2", 332, 74, 1, 40, "vsep"),
    frame("mpword", 342, 72, 22, 14, "label", text=True, data="MP"),
    frame("orba", 366, 75, 9, 9, "orb white"),
    frame("mpa", 380, 72, 90, 13, "mp", bind="{charges[0]} / {charges[1]} / {charges[2]} / {charges[3]}"),
    frame("orbb", 366, 91, 9, 9, "orb black"),
    frame("mpb", 380, 88, 90, 13, "mp", bind="{charges[4]} / {charges[5]} / {charges[6]} / {charges[7]}"),
]))

# The list: a header, then the eight levels 16 apart, the game's cells in three columns.
m.append(frame("lvhead", 22, 133, 60, 14, "label", text=True, data="Lv."))
m.append(frame("magichead", 142, 133, 80, 14, "label", text=True, data="Magic"))
m.append(frame("headline", 16, 148, 448, 1, "line"))
area = [f for f in m.findall("frame") if f.find("id") is None and f.find("frame") is not None and any(c.findtext("id") == "magic_list" for c in f)][0]
place(area, 8, 151)
lst = find(m, "magic_list")
place(lst, 0, 0, 464, 128)
for f in lst.findall("frame"):
    r, c = (int(n) for n in f.findtext("id").split("-"))
    place(f, 130 + (c - 1) * 110, (r - 1) * 16, 106, 16, "spell")
for r in range(1, 8):
    m.append(frame(f"row{r}", 16, 151 + r * 16, 448, 1, "rowline"))
for c in range(3):
    m.append(frame(f"col{c}", 134 + c * 110, 153, 1, 126, "vsep"))

# What the spell under the cursor does: in the bottom bar, right of Back.
place(find(m, "caption"), 108, 293, 358, 18, "captiontext")

write("magic.xml",
      "The Magic screen (magic), patched: the game's frames as its file has them (its tabs, and PramMagic's list of the levels' "
      "charges and spells), moved and styled, with the hero's panel, the list's lines and the icons of the mod's. Generated.", [m])
print("ok")
