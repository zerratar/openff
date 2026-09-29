"""The Starlit Menu's layouts written from here: the main menu, Status, and the menus' OK / Back buttons (field_hud).

    python tools/layouts.py

The game's screens keep their frames as their code reads them (ids, works, tags, the order some are walked in); the
comments at each screen say which. The Config screens are tools/config.py's."""
import os, xml.dom.minidom
ROOT = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "menus"))

def text(id, x, y, w, h, bind=None, data=None, cls=None, msg=-1, align=0, extra="", attrs=""):
    a = attrs
    if cls: a += f' class="{cls}"'
    if bind: a += f' bind-text="{bind}"'
    if data is None: d = "" if bind else "<data />"
    else: d = f'<data type="string">{data}</data>' if data.strip().isdigit() else f"<data>{data}</data>"
    return (f'<frame{a}><id>{id}</id><x>{x}</x><y>{y}</y><width>{w}</width><height>{h}</height>{extra}'
            f'<behavior value="Text"><parameter>{msg}</parameter><parameter>8</parameter><parameter>{align}</parameter></behavior>{d}</frame>')

def box(id, x, y, w, h, cls=None, inner="", attrs="", extra=""):
    a = attrs
    if cls: a += f' class="{cls}"'
    return f'<frame{a}><id>{id}</id><x>{x}</x><y>{y}</y><width>{w}</width><height>{h}</height>{extra}{inner}</frame>'

def cursor(id, x, y):
    """Where the hand stands on the frame it is in (OpenFF's <cursor/>): its point, the tip of the finger a few units right of it."""
    return box(id, x, y, 1, 1, extra="<cursor />")

HERO = ' bind-class="gone: !this"'    # a hero's parts: away where there is no hero
NOONE = ' bind-class="gone: this"'    # an empty place's: away where there is one

def write(name, comment, body):
    s = '<?xml version="1.0" encoding="utf-8"?><menulist>' + body + '</menulist>'
    s = xml.dom.minidom.parseString(s.encode("utf-8")).toprettyxml(indent="  ", encoding="utf-8").decode("utf-8")
    lines = [l for l in s.splitlines() if l.strip()]
    lines.insert(1, "<!-- " + comment + " -->")
    open(os.path.join(ROOT, name), "w", encoding="utf-8", newline="\n").write("\n".join(lines) + "\n")

# ---------------- main_menu ----------------

M = ['<menu><name>main_menu</name>']

# The header: a crystal, the title, and what the command under the hand does; a gold line under it.
hdr = [box("crystal", 9, 4, 26, 26, "crystal"), text("title", 40, 7, 50, 20, data="Menu", cls="title"), box("hsep", 97, 11, 1, 12, "hsep")]
helps = [("com_item", "View and use items."), ("com_magic", "Cast and manage spells."), ("com_equip", "Change weapons and armour."),
         ("com_status", "See a hero's full status."), ("com_tairetu", "Change the party's order and rows."), ("com_job", "Change a hero's job."),
         ("com_mod_gambits", "Set the heroes' auto-battle rules."), ("com_config", "Change the game's settings."),
         ("com_half", "Save for now and quit."), ("com_save", "Record your journey.")]
for cid, t in helps:
    hdr.append(text("help_" + cid, 104, 10, 236, 16, data=t, cls="help", attrs=f' bind-class="gone: menu.focused != \'{cid}\'"'))
picking = " &amp;&amp; ".join(f"menu.focused != 'p{i}'" for i in range(1, 5))
hdr.append(text("help_pick", 104, 10, 236, 16, data="Choose a hero.", cls="help", attrs=f' bind-class="gone: {picking}"'))
hdr.append(box("hline", 8, 31, 334, 1, "hline"))
M.append(box("header", 0, 0, 480, 34, "header", "".join(hdr)))

# The party: a panel a place, a hero's or an empty one; each reads its hero from party[i].
rows = []
for i in range(4):
    inner = [box(f"face{i}", 8, 5, 50, 50, "face", attrs=HERO, extra=f"<portrait>{i}</portrait>"),
             box(f"ghost{i}", 8, 5, 50, 50, "ghost", attrs=NOONE, inner=box(f"shade{i}", 3, 3, 44, 44, f"shade shade{i}")),
             box(f"num{i}", -2, -3, 23, 23, "badge", box(f"diamond{i}", 0, 0, 23, 23, "diamond") + text(f"numtext{i}", 0, 5, 23, 13, data=str(i + 1), cls="badgetext"))]
    hero = [text(f"name{i}", 0, 1, 110, 16, bind="{name}", cls="name"),
            text(f"job{i}", 0, 19, 110, 13, bind="{jobTitle}", cls="job"),
            text(f"lv{i}", 0, 34, 36, 14, bind="Lv. {level}", cls="lv"),
            box(f"exp{i}", 36, 41, 66, 3, "bar exp", box(f"expfill{i}", 0, 0, 66, 3, "fill", attrs=' bind-style="width: {expPercent}%"')),
            box(f"vsep{i}", 112, 2, 1, 48, "vsep"),
            text(f"hpl{i}", 120, 3, 20, 12, data="HP", cls="label"),
            text(f"hp{i}", 142, 2, 108, 14, bind="{hp} / {maxHp}", cls="hp"),
            box(f"hpbar{i}", 142, 18, 108, 4, "bar hpbar", box(f"hpfill{i}", 0, 0, 108, 4, "fill", attrs=' bind-style="width: {hpPercent}%" bind-class="low: hpPercent &lt; 30"')),
            text(f"mpl{i}", 120, 28, 20, 12, data="MP", cls="label"),
            box(f"orb{i}a", 142, 29, 9, 9, "orb white"),
            text(f"mpa{i}", 156, 26, 96, 13, bind="{charges[0]} / {charges[1]} / {charges[2]} / {charges[3]}", cls="mp"),
            box(f"orb{i}b", 142, 41, 9, 9, "orb black"),
            text(f"mpb{i}", 156, 38, 96, 13, bind="{charges[4]} / {charges[5]} / {charges[6]} / {charges[7]}", cls="mp")]
    inner.append(box(f"hero{i}", 68, 4, 254, 52, "hero", "".join(hero), attrs=HERO))
    inner.append(box(f"none{i}", 68, 4, 254, 52, "none",
                     box(f"dashl{i}", 44, 26, 26, 1, "dash") + text(f"empty{i}", 70, 18, 60, 16, data="Empty", cls="empty") +
                     box(f"dashr{i}", 130, 26, 26, 1, "dash") + box(f"crest{i}", 200, 3, 46, 46, "crest"), attrs=NOONE))
    rows.append(box(f"m{i}", 4, 3 + i * 60, 326, 60, "member", "".join(inner), attrs=f' data-source="places[{i}]"'))
M.append(box("party", 8, 36, 334, 248, "panel party", "".join(rows) + box("party_frame", -5, -5, 344, 258, "goldframe")))

# The commands: the game's rows (the client re-spaces them when Gambits goes in, copying Job's), each with its icon, the lit
# box the hand's row shows, and where the hand stands - left of the icon, over the panel's edge.
cmds = [("com_item", 50003, 0), ("com_magic", 50004, 1), ("com_equip", 50005, 2), ("com_status", 50006, 3), ("com_tairetu", 50011, 4),
        ("com_job", 50002, 5), ("com_config", 50007, 6), ("com_half", 50010, 7), ("com_save", 50008, 8)]
M.append(box("cmdpanel", 350, 36, 122, 248, "panel commands", box("cmdpanel_frame", -5, -5, 132, 258, "goldframe")))
crow = []
for k, (cid, msg, work) in enumerate(cmds):
    up, down = cmds[(k - 1) % len(cmds)][0], cmds[(k + 1) % len(cmds)][0]
    extra = f"<focus /><work>{work}</work><myTag>{k}</myTag><up>{up}</up><down>{down}</down><left>dummy</left><right>dummy</right>"
    inner = (box("hl_" + cid, -30, 0, 114, 22, "hl") + box("line_" + cid, -28, 23, 110, 1, "line") +
             box("icon_" + cid, -25, 2, 18, 18, "icon") + cursor("cursor_" + cid, -34, 11))
    crow.append(f'<frame class="cmd"><id>{cid}</id><x>34</x><y>{k * 26}</y><width>86</width><height>22</height>{extra}'
                f'<behavior value="Text"><parameter>{msg}</parameter><parameter>8</parameter><parameter>6</parameter></behavior>{inner}</frame>')
M.append(f'<frame><id>main_command</id><x>352</x><y>44</y><width>120</width><height>0</height>{"".join(crow)}</frame>')

# The hero pick (Status, Equipment...): the game's frames, one over each panel, the hand on the portrait. It must follow the
# command list: the game finds the heroes as the list's next frame (CWMenuManager.CSelectInitialize).
ps = []
for i in range(4):
    ps.append(f'<frame class="pick"><id>p{i + 1}</id><x>0</x><y>{3 + i * 60}</y><width>326</width><height>60</height><work>{i}</work><myTag>{9 + i}</myTag>'
              f'<up>p{(i - 1) % 4 + 1}</up><down>p{(i + 1) % 4 + 1}</down>'
              f'<behavior value="Text"><parameter>-1</parameter><parameter>8</parameter><parameter>5</parameter></behavior>'
              + cursor(f"cursor_p{i + 1}", 4, 32) + '</frame>')
M.append(f'<frame><id>char_select</id><x>12</x><y>36</y><width>0</width><height>0</height>{"".join(ps)}</frame>')

# The bottom bar, with the party's gil; Back is the menus' own button (field_hud's b_button, styled beside this).
bar = [box("coins", 374, 4, 18, 18, "coins"), box("gsep", 396, 8, 1, 12, "gsep"), text("gil", 398, 6, 58, 16, bind="{gil:N0} Gil", cls="gil")]
M.append(box("bottom", 8, 290, 464, 24, "panel bottom", "".join(bar) + box("bottom_frame", -5, -5, 474, 34, "goldframe")))
M.append('</menu>')

write("main_menu.xml",
      "The main menu, laid out again: the game's own screen - CWMenuMain still drives it, so the command ids, the hero pick p1..p4 and "
      "their tags stay - its hero panels drawn from bindings in place of the game's CStatus. The canvas is the game's 480 x 320. "
      "The look is styles/starlit.css. Generated.", "".join(M))

# ---------------- field_hud: the menus' OK and Back buttons ----------------

def button(id, action):
    """A pill: the button to press as the player's input shows it (a pad's mark or letter, or the keyboard's key), then the game's label."""
    on = f"input.{action}Button"
    disc = box(id + "_disc", 0, 0, 14, 14, "disc",
               text(id + "_mark", 0, 1, 14, 12, bind="{input." + action + "}", cls="mark"),
               attrs=f' bind-display="input.pad" bind-class="ps: input.device == \'playstation\'; xbox: input.device == \'xbox\'; '
                     f'cross: {on} == \'cross\'; circle: {on} == \'circle\'; square: {on} == \'square\'; triangle: {on} == \'triangle\'"')
    cap = box(id + "_cap", 0, 0, 26, 14, "cap", text(id + "_capkey", 0, 1, 26, 12, bind="{input." + action + "}", cls="capkey"),
              attrs=' bind-display="!input.pad"')
    return box(id, 8, 290, 80, 24, "pill", disc + cap + text("text", 0, 0, 44, 16, cls="pilltext"),
               attrs=' style="flex-direction: row; align-items: center; gap: 6px; padding-left: 6px; padding-right: 10px; width: auto"')

def game(id, x, y, w, h, behaviour, params, cls=None, inner="", attrs="", extra=""):
    """One of the game's frames as its screen's code reads it: its behaviour and parameters as the game's file has them."""
    a = attrs + (f' class="{cls}"' if cls else "")
    ident = f"<id>{id}</id>" if id else ""
    ps = "".join(f"<parameter>{p}</parameter>" for p in params)
    beh = f'<behavior value="{behaviour}">{ps}</behavior>' if behaviour else ""
    return f'<frame{a}>{ident}<x>{x}</x><y>{y}</y><width>{w}</width><height>{h}</height>{extra}{beh}{inner}</frame>'

def panel(id, x, y, w, h, inner="", cls=""):
    """A navy panel, its gold frame a frame of its own round it."""
    return box(id, x, y, w, h, ("panel " + cls).strip(), inner + box(id + "_frame", -5, -5, w + 10, h + 10, "goldframe"))

def header(help_text, title=None, sep=100):
    """The crystal, the title (the game's own text frame, or ours), the line, what the screen is for, the gold line under it."""
    parts = [box("crystal", 9, 4, 26, 26, "crystal")]
    if title: parts.append(title)
    parts.append(box("hsep", sep, 11, 1, 12, "hsep"))
    parts.append(text("help", sep + 7, 10, 330, 16, data=help_text, cls="help"))
    parts.append(box("hline", 8, 31, 464, 1, "hline"))
    return box("header", 0, 0, 480, 34, "header", "".join(parts))

# ---------------- status ----------------

S = ['<menu><name>status</name>']
S.append(header("View character status and attributes."))
S.append(panel("top", 8, 38, 464, 98))
S.append(panel("stats", 8, 144, 464, 138))
S.append(box("bottom", 8, 290, 464, 24, "panel bottom", box("bottom_frame", -5, -5, 474, 34, "goldframe")))

# The mod's own frames over the panels: the portrait, bars, lines, icons and labels (the game's texts are placed below).
own = [box("face", 16, 46, 76, 76, "face bigface", extra="<portrait />"),
       text("jobword", 100, 70, 96, 16, bind="{jobTitle}", cls="job"),
       text("lvword", 100, 96, 60, 16, bind="Lv. {level}", cls="lv"),
       text("expword", 358, 84, 60, 16, data="EXP", cls="label2"), text("expvalue", 404, 84, 56, 16, bind="{experience}", cls="value2 right"),
       text("nextword", 358, 102, 70, 16, data="For Next Level", cls="label2"), text("nextvalue", 424, 102, 36, 16, bind="{expToNext}", cls="value2 right"),
       box("expbar", 100, 116, 88, 3, "bar exp", box("expfill", 0, 0, 88, 3, "fill", attrs=' bind-style="width: {expPercent}%"')),
       box("vsep1", 198, 46, 1, 82, "vsep"), box("vsep2", 348, 46, 1, 82, "vsep"),
       box("hpbar", 234, 68, 94, 4, "bar hpbar", box("hpfill", 0, 0, 94, 4, "fill", attrs=' bind-style="width: {hpPercent}%" bind-class="low: hpPercent &lt; 30"')),
       box("jline", 358, 74, 104, 1, "line"),
       box("attr_icon", 18, 150, 16, 16, "sicon s_bars"), text("attr_title", 38, 150, 100, 16, data="Attributes", cls="subtitle"),
       box("attr_line", 16, 168, 214, 1, "line"),
       box("vsep3", 240, 152, 1, 122, "vsep"),
       box("battle_icon", 254, 150, 16, 16, "sicon s_swords"), text("battle_title", 274, 150, 100, 16, data="Battle Stats", cls="subtitle"),
       box("battle_line", 252, 168, 212, 1, "line")]
for r in range(8):
    col, row = r % 2, r // 2
    own.append(text(f"mplv{r}", 238 + col * 48, 79 + row * 12, 8, 12, data=str(r + 1), cls="mplv"))
attrs = [("STR", 50409, "sword"), ("AGI", 50410, "boot"), ("VIT", 50411, "shield"), ("INT", 50412, "book"), ("MND", 50413, "staff")]
stat_of = {"STR": "strength", "AGI": "agility", "VIT": "vitality", "INT": "intellect", "MND": "mind"}
for r, (sid, _, icon) in enumerate(attrs):
    y = 176 + r * 20
    own.append(box(f"{sid}_icon", 20, y, 16, 16, f"sicon s_{icon}"))
    own.append(box(f"{sid}_bar", 106, y + 6, 88, 4, "bar statbar", box(f"{sid}_fill", 0, 0, 88, 4, "fill", attrs=f' bind-style="width: {{stats.{stat_of[sid]} * 100 / 99}}%"')))
    if r < 4: own.append(box(f"{sid}_line", 16, y + 18, 214, 1, "rowline"))
battle = [("ATK", 50504, "sword", "attack"), ("DEF", 50506, "shield", "defense"), ("MDF", 50508, "emblem", "magicDefense")]
for r, (sid, _, icon, stat) in enumerate(battle):
    y = 180 + r * 26
    own.append(box(f"{sid}_icon", 256, y, 18, 18, f"sicon s_{icon}"))
    own.append(box(f"{sid}_bar", 374, y + 7, 58, 4, "bar statbar", box(f"{sid}_fill", 0, 0, 58, 4, "fill", attrs=f' bind-style="width: {{stats.{stat} * 100 / 255}}%"')))
    if r < 2: own.append(box(f"{sid}_line", 252, y + 22, 212, 1, "rowline"))
S.append(box("own", 0, 0, 0, 0, None, "".join(own), attrs=' data-source="hero"'))


# The game's frames (CWMenuStatus writes them by id and walks some by their order: MP's values in pairs then its slashes,
# a stat's label then its ':' then its value) - kept as the game has them, placed and styled.
g = [game("mbs_title", 40, 7, 80, 20, "Text", [50006, 8, 0], "title"),
     game("mbs_name", 100, 46, 96, 20, "Text", [-1, 12], "name bigname"),
     game("mbs_job", 600, 70, 96, 16, "JName", [0, 8]),   # the game's job name (not a text the sheets reach): off the screen, ours in its place
     game("lvlabel", 100, 96, 0, 16, "Text", [50414, 8], "hidden", game("LV", 22, 0, 16, 16, "Text", [-1, 8, 0], "lv")),
     game("skilllabel", 358, 50, 0, 16, "Text", [50503, 8], "label2", game("SKILL", 86, 0, 16, 16, "Text", [50424, 8, 1], "value2")),
     game("hplabel", 210, 50, 8, 16, "Text", [50415, 8], "label",
          game("HP", 58, -1, 24, 16, "Text", [50420, 8, 1], "hp") + game("HPSLASH", 82, -1, 10, 16, "Text", [50418, 8, 2], "hp") + game("HPMax", 92, -1, 26, 16, "Text", [50421, 8, 0], "hp"))]
mp = []
for r in range(8):
    col, row = r % 2, r // 2
    mp.append(game(None, 34 + col * 48, row * 12 - 2, 12, 12, "Text", [-1, 8, 1], "mpv"))
    mp.append(game(None, 52 + col * 48, row * 12 - 2, 12, 12, "Text", [-1, 8, 0], "mpv"))
for r in range(8):
    col, row = r % 2, r // 2
    mp.append(game("MPSLASH" if r == 0 else None, 46 + col * 48, row * 12 - 2, 6, 12, "Text", [50418, 8, 2], "mpv"))
g.append(game("MP", 210, 78, 8, 16, "Text", [50416, 8], "label", "".join(mp)))
g.append(game("explabel", 358, 84, 0, 16, "Text", [50407, 8], "hidden", game("EXP", 86, 0, 16, 16, "Text", [50422, 8, 1], "hidden")))
g.append(game("nextlabel", 358, 102, 0, 16, "Text", [50408, 8], "hidden", game("EXPMax", 86, 0, 16, 16, "Text", [50423, 8, 1], "hidden")))
rows = "".join(game(sid, 0, r * 20, 60, 16, "Text", [msg, 8], "statname",
                    game(None, 96, 0, 8, 16, "Text", [50435, 8], "hidden") + game(None, 150, 0, 22, 16, "Text", [-1, 8, 1], "statvalue"))
               for r, (sid, msg, _) in enumerate(attrs))
g.append(game("attrs", 42, 176, 0, 0, None, [], None, rows))
rows = "".join(game(sid, 0, r * 26, 70, 16, "Text", [msg, 8], "statname",
                    game(None, 96, 0, 8, 16, "Text", [50435, 8], "hidden") + game(None, 145, 0, 24, 16, "Text", [-1, 8, 1], "statvalue"))
               for r, (sid, msg, _, _) in enumerate(battle))
g.append(game("battle", 283, 181, 0, 0, None, [], None, rows))
S.append(game("status_root", 0, 0, 0, 0, "MBStatus", [], None, "".join(g)))
S.append('</menu>')
write("status.xml",
      "The Status screen, laid out again: the game's frames (CWMenuStatus writes them by id and walks MP's values and each stat's label, "
      "':' and value in order - that order kept) placed and styled, with the portrait, the bars and the panels of the mod's. Generated.",
      "".join(S))

write("field_hud.xml",
      "The menus' OK and Back buttons (CWMenuButton), and L / R (another hero) as two small pills at the right, merged into the game's field_hud (\"patch\": true): a gold framed pill at the "
      "bottom left in place of the game's bar across the bottom, the button to press shown the player's way - input.ok / input.back "
      "and the button each is on. Everything else of the field's HUD is the game's.",
      '<menu><name>field_hud</name>' + button("a_button", "ok") + button("b_button", "back") +
      box("l_button", 392, 291, 38, 22, "lrpill", box("l_button_icon", 11, 4, 14, 14, "sicon s_left")) +
      box("r_button", 434, 291, 38, 22, "lrpill", box("r_button_icon", 13, 4, 14, 14, "sicon s_right")) + '</menu>')
print("ok")
