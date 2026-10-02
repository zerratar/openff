# Rogue Mode's menu screens: writes menus/<screen>.xml (the game's menulist format) and .json for each.
# Run from anywhere: python Samples/Rogue/tools/menus.py
import json, os
OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "menus")
os.makedirs(OUT, exist_ok=True)

def esc(s): return (s or "").replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")

def text(id, x, y, w, h, data="", colour=None, font=None, align=None):
    s = f"<frame>\n  <id>{id}</id><x>{x}</x><y>{y}</y><width>{w}</width><height>{h}</height>\n"
    if align: s += f"  <align>{align}</align>"
    if font: s += f"<font>{font}</font>"
    if colour: s += f"<colour>{colour}</colour>"
    if align or font or colour: s += "\n"
    s += '  <behavior value="Text"><parameter>-1</parameter><parameter>8</parameter><parameter>0</parameter></behavior>\n'
    s += f"  <data>{esc(data)}</data>\n</frame>"
    return s

def focus(id, x, y, w, h, up, down, left="dummy", right="dummy", data="", font=None):
    s = f"<frame>\n  <focus />\n  <id>{id}</id><x>{x}</x><y>{y}</y><width>{w}</width><height>{h}</height>\n"
    s += f"  <up>{up}</up><down>{down}</down><left>{left}</left><right>{right}</right>\n  <align>menu</align>"
    if font: s += f"<font>{font}</font>"
    s += '\n  <behavior value="Text"><parameter>-1</parameter><parameter>8</parameter><parameter>0</parameter></behavior>\n'
    s += f"  <data>{esc(data)}</data>\n</frame>"
    return s

def window(id, x, y, w, h, children):
    inner = "\n".join(children)
    inner = "\n".join("  " + l for l in inner.split("\n")) if inner else ""
    return f"<frame>\n  <window />\n  <id>{id}</id><x>{x}</x><y>{y}</y><width>{w}</width><height>{h}</height>\n{inner}\n</frame>"

def desc():
    return window("w_desc", 4, 240, 472, 44, [text("desc1", 12, 3, 448, 18), text("desc2", 12, 23, 448, 18)])

def screen(name, comment, frames, behaviour, title, background=-1):
    body = "\n".join(frames)
    body = "\n".join("    " + l for l in body.split("\n"))
    xml = f'<?xml version="1.0" encoding="utf-8"?>\n<!-- {comment} -->\n<menulist>\n  <menu>\n    <name>{name}</name>\n{body}\n  </menu>\n</menulist>\n'
    open(os.path.join(OUT, name + ".xml"), "w", encoding="utf-8", newline="\n").write(xml)
    d = {"id": name, "layout": name + ".xml", "screen": name, "title": title, "background": background,
         "attachments": [{"target": "", "behaviour": behaviour}]}
    open(os.path.join(OUT, name + ".json"), "w", encoding="utf-8", newline="\n").write(json.dumps(d, indent=2) + "\n")

# The mode's own menu.
screen("rogue", "Rogue Mode's own menu: New Run, Continue Run. 480 x 288, over the camp. RogueHub (RogueScreens.cs) drives it.", [
    window("w_top", 4, 4, 472, 52, [text("title", 12, 4, 448, 24, "Rogue Mode", "pale-yellow"), text("sub", 12, 28, 448, 18, "", "pale-blue", 10)]),
    window("w_body", 120, 92, 240, 96, []),
    focus("new", 150, 112, 200, 24, "continue", "continue", data="New Run"),
    focus("continue", 150, 146, 200, 24, "new", "new", data="Continue Run"),
    desc(),
], "RogueHub", "Rogue Mode")

# A new run.
hero_rows = []
order = ["hero0", "hero1", "hero2", "hero3", "seed", "begin"]
for h in range(4):
    up = order[(order.index("hero%d" % h) - 1) % len(order)]
    down = order[order.index("hero%d" % h) + 1]
    hero_rows.append(focus("hero%d" % h, 30, 56 + 28 * h, 330, 24, up, down, data=""))
    hero_rows.append(text("cost%d" % h, 380, 58 + 28 * h, 80, 20, "", "pale-blue", 10, "right"))
screen("rogue-party", "A new run: each hero's starting job (left / right), the cost against the budget, the seed. RogueParty drives it.", [
    window("w_top", 4, 4, 472, 36, [text("title", 12, 4, 240, 28, "New Run", "pale-yellow"), text("total", 240, 6, 220, 24, "", None, None, "right")]),
    window("w_body", 4, 44, 472, 192, [text("costs", 360, 2, 100, 14, "Cost", "pale-blue", 10, "right")]),
    *hero_rows,
    focus("seed", 30, 176, 330, 24, "hero3", "begin", data="Seed"),
    focus("begin", 30, 204, 330, 24, "seed", "hero0", data="Begin the run"),
    desc(),
], "RogueParty", "New Run")

# Between battles.
camp_party = []
for h in range(4):
    camp_party.append(text("p%d" % h, 12, 6 + 26 * h, 70, 22))
    camp_party.append(text("job%d" % h, 84, 6 + 26 * h, 100, 22))
    camp_party.append(text("lv%d" % h, 180, 6 + 26 * h, 44, 22))
    camp_party.append(text("hp%d" % h, 210, 9 + 26 * h, 86, 18, "", "pale-blue", 10, "right"))
screen("rogue-camp", "Between battles: the act and the battle ahead, the party, gil; Fight, Active Effects, Abandon. RogueCamp drives it.", [
    window("w_top", 4, 4, 472, 52, [text("title", 12, 4, 448, 24, "", "pale-yellow"), text("step", 12, 28, 448, 18, "", "pale-blue", 10)]),
    window("w_party", 4, 60, 304, 116, camp_party),
    window("w_cmd", 312, 60, 164, 116, []),
    focus("fight", 330, 70, 140, 24, "abandon", "effects", data="Fight"),
    focus("effects", 330, 104, 140, 24, "fight", "abandon", data="Active Effects"),
    focus("abandon", 330, 138, 140, 24, "effects", "fight", data="Abandon the run", font=12),
    window("w_info", 4, 180, 472, 56, [text("next", 12, 4, 448, 18, "", None, 10), text("elite", 12, 24, 280, 18, "", "pale-red", 10), text("gil", 280, 24, 180, 18, "", "pale-blue", 10, "right")]),
    desc(),
], "RogueCamp", "Camp")

# A victory's choices.
cards = []
for i in range(3):
    x = 4 + 159 * i
    lines = [text("label%d" % i, 10, 30, 136, 16, "", "pale-blue", 10)] + [text("text%d_%d" % (i, l), 10, 52 + 18 * l, 136, 16, "", None, 10) for l in range(4)]
    cards.append(window("w_c%d" % i, x, 44, 154, 156, lines))
for i in range(3):
    x = 4 + 159 * i
    cards.append(focus("card%d" % i, x + 30, 50, 120, 22, "reroll", "reroll", "card%d" % ((i + 2) % 3), "card%d" % ((i + 1) % 3), font=12))
screen("rogue-reward", "A victory's three choices side by side, and a reroll for gil. RogueReward drives it.", [
    window("w_top", 4, 4, 472, 36, [text("title", 12, 4, 240, 28, "Victory!", "pale-yellow"), text("sub", 240, 6, 220, 24, "Choose one", "pale-blue", None, "right")]),
    *cards,
    window("w_re", 4, 204, 472, 32, [text("gil", 280, 7, 180, 18, "", "pale-blue", 10, "right")]),
    focus("reroll", 30, 208, 250, 24, "card0", "card0", data="Reroll"),
    desc(),
], "RogueReward", "Rewards")

# The modifiers held.
rows = []
for r in range(6):
    rows.append(focus("row%d" % r, 30, 50 + 30 * r, 420, 18, "row%d" % ((r + 5) % 6), "row%d" % ((r + 1) % 6)))
    rows.append(text("sub%d" % r, 52, 66 + 30 * r, 400, 14, "", "pale-blue", 10))
screen("rogue-effects", "The run's modifiers in full, six a page. RogueEffects drives it.", [
    window("w_top", 4, 4, 472, 36, [text("title", 12, 4, 448, 28, "Active Effects", "pale-yellow")]),
    window("w_list", 4, 44, 472, 192, [text("none", 12, 8, 448, 20), text("page", 12, 172, 448, 14, "", "pale-blue", 10, "right")]),
    *rows,
    desc(),
], "RogueEffects", "Active Effects")

# A run's end.
screen("rogue-summary", "A run's end: how far it went and what it did. RogueSummary drives it.", [
    window("w_top", 4, 4, 472, 36, [text("title", 12, 4, 448, 28, "", "pale-yellow")]),
    window("w_body", 4, 44, 472, 192, [t for i in range(8) for t in (text("s%d" % i, 16, 10 + 22 * i, 130, 18, "", "pale-blue", 10), text("v%d" % i, 150, 10 + 22 * i, 306, 18, "", None, 10))]),
    window("w_end", 4, 240, 472, 44, []),
    focus("ok", 150, 250, 200, 24, "ok", "ok", data="Back to Rogue Mode"),
], "RogueSummary", "Run Summary")
print("wrote", len(os.listdir(OUT)), "files to", os.path.normpath(OUT))
