"""python tools/equip.py <MenuDefine.xml> - the Equipment screen (equip) and the item list it appends under itself
(equip_under_list) of the Starlit Menu. CWMenuEquip reads its frames by id - the tabs, char_name, attack_now / difence_now and
their _max, the slots by name, caption - so they are the game's, moved; the hero's picture, bars and panels are the mod's.
Patched into the game's screens, so the tags stay the game's."""
from common import screen, find, place, link, frame, cursor, panel, bar, bottom, front, write

# ---------------- equip ----------------

def under(fid):
    """An icon the hand stands on - the cursor's, or the hand the game leaves where the cursor came from - hidden under it."""
    return "under: menu.focused == '%s' || menu.marked == '%s'" % (fid, fid)

e = screen("equip")
place(find(e, "command"), 40, 7, 110, 20, "title")
e.append(frame("crystal", 9, 4, 26, 26, "crystal"))
e.append(frame("hline", 8, 31, 464, 1, "hline"))
front(e, panel("hero", 8, 39, 184, 136), panel("slotpanel", 198, 39, 274, 136), panel("listpanel", 8, 183, 464, 98), bottom())

# The tabs, at the header's right, each with its icon.
place(find(e, "command_select"), 0, 0)
place(find(e, "select_equip_command"), 0, 0)
# Each label a little right of the middle, clear of its icon: the text is centred in the frame, so the frame starts past the icon.
for i, (tid, icon, x, w) in enumerate((("m_equip", "sword", 188, 96), ("m_nouse", "briefcase", 287, 86), ("m_removeall", "trash", 376, 96))):
    # The hand over the tab's icon, all of it: on the focused tab the hand stands where its icon was.
    place(find(e, tid), x + 18, 5, w - 18, 24, "tab2label").append(cursor("cursor_" + tid, -4, 12))
    e.append(frame(tid + "_tab", x, 5, w, 24, "tab2", attrs={"bind-class": "lit: menu.focused == '%s'" % tid}))
    e.append(frame(tid + "_icon", x + 5, 9, 16, 16, "sicon s_" + icon, attrs={"bind-class": under(tid)}))

# The hero: the game's name and its attack and defence (now, and with what the list's cursor is on), the rest bound.
place(find(e, "char_name"), 78, 46, 110, 18, "name")
for gid, y in (("attack_name", 142), ("difence_name", 157)):
    place(find(e, gid), 18, y, 60, 14, "statname")
    # The game draws its better / worse arrow 8 left of the _max value, over 16: room kept for it between the two.
    place(find(e, gid.replace("_name", "_now")), 84, 0, 30, 14, "statvalue")
    place(find(e, gid.replace("_name", "_max")), 132, 0, 34, 14, "statvalue2")
e.append(frame("own", 0, 0, 0, 0, attrs={"data-source": "hero"}, children=[
    frame("face", 16, 46, 54, 54, "face", extra="portrait"),
    frame("jobword", 78, 64, 110, 14, "job", bind="{jobTitle}"),
    frame("lvword", 78, 80, 40, 16, "lv", bind="Lv. {level}"),
    bar("expbar", 116, 88, 70, 3, "exp", "expPercent"),
    frame("line1", 14, 104, 172, 1, "line"),
    frame("hpword", 18, 108, 22, 14, "label", text=True, data="HP"),
    frame("hpvalue", 40, 108, 50, 14, "hp", bind="{hp}/{maxHp}"),
    bar("hpbar", 94, 113, 90, 4, "hpbar", "hpPercent"),
    frame("mpword", 18, 123, 22, 14, "label", text=True, data="MP"),
    frame("mpvalue", 40, 123, 50, 14, "hp", bind="{charges[0]}/{maxCharges[0]}"),
    bar("mpbar", 94, 128, 90, 4, "exp", "mpPercent"),
    frame("line2", 14, 138, 172, 1, "line"),
]))

# The slots in one column: a row each, its icon, the game's label and its item; the cursor runs down them and round.
place(find(e, "equip_slot_all"), 0, 0)
place(find(e, "slots"), 0, 0)
slots = [("right", "migite", "sword"), ("left", "hidarite", "shield"), ("head", "atama", "helmet"), ("body", "karada", "armour"), ("arm", "ude", "gauntlet")]
for i, (sid, label, icon) in enumerate(slots):
    y = 46 + i * 25
    place(find(e, label), 228, y, 80, 23, "slotname")
    item = place(find(e, sid), 320, y, 146, 23, "slotitem")
    link(item, up=slots[i - 1][0], down=slots[(i + 1) % len(slots)][0], left="dummy", right="dummy")
    item.append(frame("hl_" + sid, -118, 0, 264, 23, "hl"))
    item.append(cursor("cursor_" + sid, -106, 11))
    e.append(frame(sid + "_icon", 206, y + 3, 18, 18, "sicon s_" + icon, attrs={"bind-class": under(sid)}))
    e.append(frame(sid + "_sep", 312, y + 3, 1, 17, "vsep"))
    if i < len(slots) - 1: e.append(frame(sid + "_line", 204, y + 24, 262, 1, "rowline"))

# What the item under the cursor does: in the bottom bar, right of Back.
place(find(e, "caption"), 108, 293, 358, 18, "captiontext")

# ---------------- equip_under_list: the list of what can go in the slot ----------------

u = screen("equip_under_list")
root = u.find("frame")
lst = [f for f in root.iter("frame") if f.findtext("id") == "item_list"][0]
place(lst, 8, 186, 464, 96)
# Each row with a hand of its own left of it, the item's icon clear of the fingertip (the game's hand stands on the icon).
for f in lst.findall("frame"):
    r, c = (int(n) for n in f.findtext("id").split("-"))
    place(f, 30 + c * 232, r * 31, 196, 30, "listrow").append(cursor("cursor_" + f.findtext("id"), -12, 15))
e.append(frame("listsep", 240, 190, 1, 86, "vsep"))

write("equip.xml",
      "The Equipment screen (equip) and the list it appends under itself (equip_under_list), patched: the game's frames as its file "
      "has them (CWMenuEquip reads them by id), moved and styled, with the hero's picture and bars, the panels and the slots' icons "
      "of the mod's. Generated.", [e, u])
print("ok")
