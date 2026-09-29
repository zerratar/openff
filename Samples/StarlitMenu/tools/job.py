"""python tools/job.py <MenuDefine.xml> - the Job screen (job) of the Starlit Menu: the game's title, its Job Level label and its
list (JobParamList draws each job and its level in the cells 0-0 .. 7-1, and the hero, the job and its level at the frames
player_name, job_name and job_skill), moved and styled; the panels, the portrait and the list's lines the mod's. It replaces
the game's screen (a replacing layout keeps the tags its frames have). The L / R buttons (another hero) are field_hud's."""
import xml.etree.ElementTree as ET
from common import screen, find, place, frame, cursor, panel, bottom, front, write

m = screen("job")
place(find(m, "mbs_title"), 40, 7, 50, 20, "title")
m.append(frame("crystal", 9, 4, 26, 26, "crystal"))
m.append(frame("hsep", 88, 11, 1, 12, "hsep"))
m.append(frame("help", 95, 10, 370, 16, "help", text=True, data="Change a hero's job."))
m.append(frame("hline", 8, 31, 464, 1, "hline"))
front(m, panel("heropanel", 8, 39, 464, 36), panel("listpanel", 8, 83, 464, 199), bottom())

# The hero's bar: the portrait, then where the list's behaviour writes the hero, the job and its level.
m.append(frame("face", 14, 42, 30, 30, "face", extra="portrait"))
place(find(m, "player_name"), 52, 48)
place(find(m, "job_name"), 170, 48)
label = [f for f in m.findall("frame") if f.find("id") is None and f.find("behavior") is not None][0]
label.insert(0, ET.Element("id")); label.find("id").text = "joblevel_label"
place(label, 330, 48, 80, 20, "label2")
place(find(m, "job_skill"), 462, 48)

# The list: eight rows 24 apart in two columns.
lst = find(m, "job_item_list")
place(lst, 16, 89, 448, 192)
for f in lst.findall("frame"):
    r, c = (int(n) for n in f.findtext("id").split("-"))
    place(f, 12 + c * 226, r * 24, 196, 24, "jobrow").append(cursor("cursor_" + f.findtext("id"), -4, 12))
for r in range(1, 8):
    m.append(frame(f"row{r}", 16, 89 + r * 24, 448, 1, "rowline"))
m.append(frame("colsep", 240, 92, 1, 186, "vsep"))

write("job.xml",
      "The Job screen (job): the game's frames as its file has them (JobParamList's list and the frames it writes the hero at), "
      "moved and styled, with the panels, the portrait and the list's lines of the mod's. Generated.", [m])
print("ok")
