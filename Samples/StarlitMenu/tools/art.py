"""The Starlit Menu's pictures from the painted originals: the two icon sheets cut into grids of 96 px cells (named in
menus/sprites.json), the empty place's silhouette, the backdrop.

    python tools/art.py [originals folder]      (default: the repository's Content/UI, where they were painted)

Needs Pillow and numpy."""
import json, os, sys
from PIL import Image, ImageFilter
import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
SRC = sys.argv[1] if len(sys.argv) > 1 else os.path.normpath(os.path.join(HERE, "..", "..", "..", "Content", "UI"))
OUT = os.path.normpath(os.path.join(HERE, "..", "menus"))
IMG = os.path.join(OUT, "images")

def bleed(im):
    """Colour spread into the see-through pixels (their alpha kept): the colour blurred by its alpha, so linear filtering shows no dark fringe."""
    a = np.array(im.convert("RGBA")).astype(np.float32) / 255
    alpha = a[..., 3:4]
    out = a[..., :3].copy()
    for radius in (2, 6, 16):
        pre = Image.fromarray((np.dstack([a[..., :3] * alpha, alpha]) * 255).astype(np.uint8), "RGBA").filter(ImageFilter.GaussianBlur(radius))
        p = np.array(pre).astype(np.float32) / 255
        spread = p[..., :3] / np.maximum(p[..., 3:4], 1e-4)
        fill = (alpha[..., 0] < 0.03) & (p[..., 3] > 0.001)
        out[fill] = spread[fill]
    return Image.fromarray((np.dstack([out, alpha]) * 255).clip(0, 255).astype(np.uint8), "RGBA")

def runs(profile, gap):
    """The runs of a projection that are not empty, merging gaps narrower than gap."""
    on = profile > 0
    spans, start = [], None
    for i, v in enumerate(on):
        if v and start is None: start = i
        if not v and start is not None: spans.append([start, i]); start = None
    if start is not None: spans.append([start, len(on)])
    merged = []
    for s0, s1 in spans:
        if merged and s0 - merged[-1][1] < gap: merged[-1][1] = s1
        else: merged.append([s0, s1])
    return [m for m in merged if m[1] - m[0] > 40]

def fit(im, size, pad=0.06):
    """The picture trimmed to what is in it and centred in a size x size cell, a margin round it."""
    box = im.getbbox()
    im = im.crop(box)
    inner = int(size * (1 - 2 * pad))
    im.thumbnail((inner, inner), Image.LANCZOS)
    cell = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    cell.alpha_composite(im, ((size - im.width) // 2, (size - im.height) // 2))
    return cell

# ---- the icons: the islands of each generated sheet, in reading order, into a grid of 96 px cells ----
CELL = 96
sheets = {}
def cut(src, out, names, cols, even=False):
    """even: the sheet an even grid of cols x rows (icons whose glow reaches the next row); else its islands, found by the gaps."""
    sheet = Image.open(os.path.join(SRC, src)).convert("RGBA")
    alpha = np.array(sheet)[..., 3] > 24
    boxes = []
    if even:
        rows = (len(names) + cols - 1) // cols
        cw, ch = sheet.width / cols, sheet.height / rows
        for k in range(len(names)):
            c, r = k % cols, k // cols
            boxes.append((slice(int(r * ch), int((r + 1) * ch)), slice(int(c * cw), int((c + 1) * cw))))
    else:
        for r0, r1 in runs(alpha.sum(axis=1), 20):
            for c0, c1 in runs(alpha[r0:r1].sum(axis=0), 20):
                boxes.append((slice(r0, r1), slice(c0, c1)))
    assert len(boxes) == len(names), (src, len(boxes))
    rows = (len(names) + cols - 1) // cols
    grid = Image.new("RGBA", (cols * CELL, rows * CELL), (0, 0, 0, 0))
    sprites = []
    for k, (b, name) in enumerate(zip(boxes, names)):
        piece = sheet.crop((b[1].start, b[0].start, b[1].stop, b[0].stop))
        if even:
            # A neighbour's glow reaching over the line: a band round the cell cleared before trimming.
            band = int(min(piece.width, piece.height) * 0.05)
            px = np.array(piece)
            px[:band, :, 3] = 0; px[-band:, :, 3] = 0; px[:, :band, 3] = 0; px[:, -band:, 3] = 0
            piece = Image.fromarray(px, "RGBA")
        cell = fit(piece, CELL)
        x, y = (k % cols) * CELL, (k // cols) * CELL
        grid.alpha_composite(cell, (x, y))
        sprites.append({"name": name, "x": x, "y": y, "w": CELL, "h": CELL})
    bleed(grid).save(os.path.join(IMG, out), optimize=True)
    sheets["url:images/" + out] = {"sprites": sprites}
cut("commands.png", "commands.png", ["item", "magic", "equipment", "status", "formation", "job", "gambits", "config", "quicksave", "save", "gil", "badge"], 4)
cut("icons2.png", "icons2.png", ["shield", "helmet", "armour", "gauntlet", "boot", "staff", "emblem", "bars", "swords", "trash", "briefcase", "exchange", "speech", "door", "left", "right"], 4, even=True)
json.dump({"sheets": sheets}, open(os.path.join(OUT, "sprites.json"), "w"), indent=2)

# ---- the empty place's silhouette ----
sil = Image.open(os.path.join(SRC, "silhouette.png")).convert("RGBA")
bleed(fit(sil, 192, pad=0.02)).save(os.path.join(IMG, "silhouette.png"), optimize=True)

# ---- the backdrop ----
Image.open(os.path.join(SRC, "backdrop.png")).convert("RGB").save(os.path.join(IMG, "backdrop.jpg"), quality=90, optimize=True)

for f in os.listdir(IMG): print(f, os.path.getsize(os.path.join(IMG, f)))
