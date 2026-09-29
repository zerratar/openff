"""python tools/item.py <MenuDefine.xml> - the Item screen (menu_item) of the Starlit Menu: the game's tabs (Use, Sort, Key Items),
its list (ItemList draws the items and their counts in the cells 1-1 .. 7-2) and its caption, moved and styled; the panels, the
list's lines and the icons the mod's. It replaces the game's screen (a replacing layout keeps the tags its frames have)."""
from common import screen, find, place, frame, cursor, panel, bottom, front, write

m = screen("menu_item")
place(find(m, "menu_target_name"), 40, 7, 60, 20, "title")
m.append(frame("crystal", 9, 4, 26, 26, "crystal"))
m.append(frame("hline", 8, 31, 464, 1, "hline"))
front(m, panel("listpanel", 8, 39, 464, 208), panel("captionpanel", 8, 255, 464, 28), bottom())

# The tabs, at the header's right, each with its icon.
place(find(m, "mm_command"), 0, 0)
for tid, icon, x, w in (("m_use", "sparkle", 196, 86), ("m_seiton", "exchange", 284, 86), ("m_important", "crystal", 372, 100)):
    place(find(m, tid), x, 5, w, 24, "tab2").append(cursor("cursor_" + tid, 6, 12))
    m.append(frame(tid + "_icon", x + 4, 10, 14, 14, "sicon s_" + icon))

# The list: seven rows 29 apart in two columns.
place(find(m, "item_list"), 8, 43, 464, 203)
for f in find(m, "item_list").findall("frame"):
    r, c = (int(n) for n in f.findtext("id").split("-"))
    place(f, 22 + (c - 1) * 226, (r - 1) * 29, 208, 29, "listrow")
for r in range(1, 7):
    m.append(frame(f"row{r}", 16, 43 + r * 29, 448, 1, "rowline"))
m.append(frame("colsep", 240, 47, 1, 196, "vsep"))

place(find(m, "caption"), 22, 260, 440, 18, "captiontext")

write("item.xml",
      "The Item screen (menu_item): the game's frames as its file has them (its tabs, ItemList's list, the caption), moved and "
      "styled, with the panels, the list's lines and the icons of the mod's. Generated.", [m])
print("ok")
