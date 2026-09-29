"""python tools/config.py <MenuDefine.xml> - the Config screens (config, config2) of the Starlit Menu: the game's frames copied from MenuDefine.xml as they are (their
works, tags, neighbours and behaviours - CWMenuConfig's code reads them all), moved and given classes; the mod's own frames
(the tabs' look, the panels, the rows' lines, the sliders) added. Patched into the game's screens, so the tags stay the game's."""
import xml.etree.ElementTree as ET
from common import screen, find, place, frame, cursor, panel, bottom, front, write

def chrome(menu):
    """What both pages share: the title, the gold line, the tabs, the panels, the Back bar's panel."""
    place(find(menu, "config_title"), 40, 7, 80, 20, "title")
    menu.append(frame("crystal", 9, 4, 26, 26, "crystal"))
    menu.append(frame("hline", 8, 31, 464, 1, "hline"))
    tabs = find(menu, "mm_command")
    place(tabs, 0, 0)
    for i, tid in enumerate(("m_setting1", "m_setting2")):
        t = place(find(menu, tid), 12 + i * 118, 38, 114, 24, "tab")
        t.append(cursor("cursor_" + tid, 8, 12))
    front(menu, panel("main", 8, 68, 464, 166), panel("explain", 8, 242, 464, 40, "explainpanel"), bottom())
    place(find(menu, "m_explanation"), 22, 250, 436, 24, "explanation")

# ---------------- page 1: the choices ----------------

c1 = screen("config")
chrome(c1)
place(find(c1, "config_list"), 0, 0)
rows = [("m_mes", ["m_slow_mes", "m_normal_mes", "m_quick_mes"]), ("m_cursor", ["m_init", "m_memory"]),
        ("m_move", ["m_walk", "m_run"]), ("m_battle", ["m_alpha", "m_not_alpha"])]
for i, (rid, choices) in enumerate(rows):
    y = 74 + i * 40
    place(find(c1, rid), 24, y, 120, 32, "rowlabel")
    w = 80 if len(choices) == 3 else 100
    for k, cid in enumerate(choices):
        ch = place(find(c1, cid), 156 + k * (w + 2), 4, w, 24, "choice")
        ch.append(cursor("cursor_" + cid, 4, 12))
    c1.append(frame(f"rowcrest{i}", 156, y + 6, 20, 20, "rowcrest"))
    if i < 3: c1.append(frame(f"rowline{i}", 20, y + 36, 440, 1, "rowline"))
# the crest sits under the choices: move them right of it
for i, (rid, choices) in enumerate(rows):
    for cid in choices:
        f = find(c1, cid); f.find("x").text = str(int(f.findtext("x")) + 26)

# ---------------- page 2: the volumes and the rest ----------------

c2 = screen("config2")
chrome(c2)
place(find(c2, "config_list"), 0, 0)
TRACK_X, TRACK_W = 204, 150
for i, (rid, sid, key) in enumerate((("m_bgm", "m_bgm_vol", "music"), ("m_se", "m_se_vol", "sound"))):
    y = 76 + i * 34
    row = place(find(c2, rid), 24, y, 120, 28, "rowlabel")
    # The slider: its frame is where a touch sets the volume (the game maps x over its width), so it is the track.
    place(find(c2, sid), TRACK_X - 24, 4, TRACK_W, 20, "slider").append(cursor("cursor_" + sid, -22, 10))
    value = [f for f in row.findall("frame") if f.find("id") is None][0]
    value.insert(0, ET.Element("id")); value.find("id").text = sid + "_value"   # an id for the sheets (the code finds it as the slider's next frame)
    place(value, TRACK_X + TRACK_W + 22 - 24, 4, 30, 20, "volume")
    c2.append(frame(f"{key}_left", TRACK_X - 16, y + 8, 10, 12, "sicon s_left"))
    c2.append(frame(f"{key}_track", TRACK_X, y + 11, TRACK_W, 6, "track",
                    [frame(f"{key}_fill", 0, 0, TRACK_W, 6, "trackfill", attrs={"bind-style": "width: {config.%sPercent}%%" % key})]))
    c2.append(frame(f"{key}_knob", TRACK_X - 8, y + 6, 16, 16, "knob",
                    attrs={"bind-style": "translate: {config.%sPercent * %d / 100}px 0" % (key, TRACK_W)}))
    c2.append(frame(f"{key}_right", TRACK_X + TRACK_W + 6, y + 8, 10, 12, "sicon s_right"))
c2.append(frame("miscline", 20, 144, 440, 1, "line"))
for i, (rid, bid, icon) in enumerate((("m_tip", "m_tips", "speech"), ("m_title", "m_title_back", "book"), ("m_quit", "m_quit_game", "door"))):
    y = 152 + i * 26
    place(find(c2, rid), 24, y, 120, 22, "rowlabel")   # the first's label is the group's (the others have none)
    b = place(find(c2, bid), 156, 0, 180, 22, "button")
    b.append(cursor("cursor_" + bid, 4, 11))
    c2.append(frame(f"{bid}_icon", 190, y + 3, 16, 16, f"sicon s_{icon}"))

write("config.xml",
      "The Config screens (config, config2), patched: the game's frames as the game's file has them (CWMenuConfig reads their "
      "works, tags and neighbours), moved and styled, and the mod's frames added - the tabs' look, the panels, the rows' lines, "
      "the volume sliders drawn from the config binding root. Generated.", [c1, c2])
print("ok")
