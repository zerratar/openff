# Rogue Mode's pictures: the backdrops behind its screens, one an act and one for the mode's own menu, painted
# here (no picture of the game's is copied). Writes menus/images/*.jpg.
# Run from anywhere: python Samples/Rogue/tools/art.py   (needs Pillow and numpy)
import math, os, random
import numpy as np
from PIL import Image, ImageDraw, ImageFilter

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "menus", "images")
W, H = 960, 640   # twice the menus' 480 x 320


def glow(img, x, y, r, colour, strength):
    """A soft light: a radial falloff added to the picture."""
    yy, xx = np.mgrid[0:H, 0:W]
    d = np.sqrt((xx - x) ** 2 + (yy - y) ** 2) / r
    a = np.clip(1 - d, 0, 1) ** 2 * strength
    for c in range(3):
        img[..., c] += a * colour[c]


def rock(seed):
    """Low, soft noise for the stone: a few octaves of random values, smoothly enlarged."""
    rng = np.random.default_rng(seed)
    out = np.zeros((H, W))
    for octave, weight in ((6, 1.0), (14, 0.5), (34, 0.25), (80, 0.12)):
        small = Image.fromarray((rng.random((max(2, octave * H // W), octave)) * 255).astype(np.uint8))
        out += np.array(small.resize((W, H), Image.BICUBIC)).astype(float) / 255 * weight
    return out / 1.87


def shards(draw, rng, count, base_y, hue, scale):
    """Crystal shards standing up from the floor in clusters: long thin facets, lit on one side."""
    centres = [rng.uniform(0, W) for _ in range(max(1, count // 5))]
    for _ in range(count):
        x = rng.choice(centres) + rng.gauss(0, 50)
        h = rng.uniform(60, 220) * scale
        w = h * rng.uniform(0.16, 0.28)
        lean = rng.uniform(-0.25, 0.25) * h
        y = base_y + rng.uniform(-20, 60)
        tip = (x + lean, y - h)
        left, right, mid = (x - w / 2, y), (x + w / 2, y), (x + lean * 0.4 + w * 0.08, y - h * 0.15)
        dark = tuple(int(c * 0.35) for c in hue) + (235,)
        lit = tuple(int(min(255, c * 0.9)) for c in hue) + (235,)
        draw.polygon([left, tip, mid], fill=dark)
        draw.polygon([mid, tip, right], fill=lit)


def pine(draw, x, base, h, fill):
    """A fir in silhouette: a trunk and four tiers of boughs, each narrower and higher."""
    w = h * 0.42
    draw.rectangle([x - w * 0.05, base - h * 0.18, x + w * 0.05, base], fill=fill)
    for k in range(4):
        y0 = base - h * (0.12 + 0.2 * k)
        ww = w * (1 - 0.2 * k)
        draw.polygon([(x - ww / 2, y0), (x, y0 - h * 0.38), (x + ww / 2, y0)], fill=fill)


def pines(draw, rng, far, hue):
    if far:
        for _ in range(46):
            pine(draw, rng.uniform(-30, W + 30), H * 0.8 + rng.uniform(-10, 40), rng.uniform(120, 260), tuple(int(c * 0.28) for c in hue) + (235,))
    else:
        for side in (0, 1):
            for _ in range(4):
                x = rng.uniform(-80, 170) if side == 0 else rng.uniform(W - 170, W + 80)
                pine(draw, x, H + 30, rng.uniform(380, 620), tuple(int(c * 0.09) for c in hue) + (255,))


def column(draw, x, base, w, h, fill, window):
    """A column of the tower: a shaft, a capital, an arch at the top and a lit window or two."""
    draw.rectangle([x - w / 2, base - h, x + w / 2, base], fill=fill)
    draw.rectangle([x - w * 0.62, base - h - w * 0.18, x + w * 0.62, base - h], fill=fill)
    draw.pieslice([x - w / 2, base - h - w * 0.62, x + w / 2, base - h + w * 0.38], 180, 360, fill=fill)
    if window:
        for k in range(2):
            wy = base - h * (0.35 + 0.3 * k)
            draw.rectangle([x - w * 0.12, wy - w * 0.32, x + w * 0.12, wy], fill=window)


def columns(draw, rng, far, hue):
    if far:
        x = -20
        while x < W + 40:
            w = rng.uniform(26, 44)
            column(draw, x, H * 0.84, w, rng.uniform(170, 330), tuple(int(c * 0.26) for c in hue) + (235,),
                   tuple(min(255, int(c * 1.1)) for c in hue) + (200,) if rng.random() < 0.5 else None)
            x += w + rng.uniform(26, 70)
    else:
        for x in (rng.uniform(30, 80), rng.uniform(W - 80, W - 30)):
            column(draw, x, H + 10, 120, H * 0.95, tuple(int(c * 0.1) for c in hue) + (255,), None)


def crystals(draw, rng, far, hue):
    if far:
        shards(draw, rng, 26, H * 0.78, hue, 0.8)
        return
    for side in (0, 1):
        for _ in range(7):
            x = rng.uniform(-60, 220) if side == 0 else rng.uniform(W - 220, W + 60)
            h = rng.uniform(120, 300)
            w = h * rng.uniform(0.18, 0.3)
            lean = rng.uniform(-0.2, 0.2) * h
            y = H + rng.uniform(0, 40)
            tip, left, right = (x + lean, y - h), (x - w / 2, y), (x + w / 2, y)
            mid = (x + lean * 0.4, y - h * 0.18)
            draw.polygon([left, tip, mid], fill=tuple(int(c * 0.22) for c in hue) + (255,))
            draw.polygon([mid, tip, right], fill=tuple(int(c * 0.55) for c in hue) + (255,))


def backdrop(name, top, bottom, hue, glows, seed, shape=crystals):
    rng = random.Random(seed)
    t = np.linspace(0, 1, H)[:, None, None]
    img = (np.array(top, float) * (1 - t) + np.array(bottom, float) * t) * np.ones((H, W, 3))
    for (x, y, r, colour, s) in glows:
        glow(img, x, y, r, colour, s)
    stone = rock(seed)
    img *= (0.72 + 0.56 * stone)[..., None]
    # The place's shapes: a far row (dim, blurred) and a near one at the sides.
    layer = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    shape(ImageDraw.Draw(layer), rng, True, hue)
    far = layer.filter(ImageFilter.GaussianBlur(3))
    near = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    shape(ImageDraw.Draw(near), rng, False, hue)
    near = near.filter(ImageFilter.GaussianBlur(1.2))
    base = Image.fromarray(np.clip(img, 0, 255).astype(np.uint8)).convert("RGBA")
    far_a = np.array(far).astype(float)
    far_a[..., 3] *= 0.55
    base.alpha_composite(Image.fromarray(far_a.astype(np.uint8)))
    # Mist over the far shards' feet, the floor fading into it.
    mist = np.zeros((H, W, 4))
    t2 = np.clip((np.linspace(0, 1, H) - 0.62) / 0.3, 0, 1)[:, None]
    mist[..., :3] = np.array(bottom, float) * 0.6 + np.array(hue, float) * 0.08
    mist[..., 3] = (np.sin(t2 * math.pi / 2) * 215) * np.ones((1, W))
    base.alpha_composite(Image.fromarray(mist.astype(np.uint8)))
    base.alpha_composite(near)
    # Motes of light drifting in the air.
    motes = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    md = ImageDraw.Draw(motes)
    for _ in range(140):
        x, y, r = rng.uniform(0, W), rng.uniform(0, H * 0.85), rng.uniform(0.6, 2.2)
        a = int(rng.uniform(40, 170))
        md.ellipse([x - r, y - r, x + r, y + r], fill=tuple(min(255, int(c * 1.2) + 40) for c in hue) + (a,))
    base.alpha_composite(motes.filter(ImageFilter.GaussianBlur(0.8)))
    # Grain and a vignette.
    out = np.array(base.convert("RGB")).astype(float)
    out += np.random.default_rng(seed).normal(0, 3.2, out.shape)
    yy, xx = np.mgrid[0:H, 0:W]
    v = np.sqrt(((xx - W / 2) / (W * 0.62)) ** 2 + ((yy - H * 0.45) / (H * 0.7)) ** 2)
    out *= np.clip(1.15 - v ** 2 * 0.55, 0.35, 1)[..., None]
    Image.fromarray(np.clip(out, 0, 255).astype(np.uint8)).save(os.path.join(OUT, name + ".jpg"), quality=90)


os.makedirs(OUT, exist_ok=True)
# The mode's own menu: the crystal's blue.
backdrop("hub", (6, 10, 22), (2, 4, 10), (90, 170, 255),
         [(W * 0.5, H * 0.42, 420, (40, 90, 170), 0.9), (W * 0.5, H * 0.95, 380, (30, 70, 140), 0.6)], 11)
# Act 1, the Altar Cave: teal stone.
backdrop("cave", (6, 14, 18), (2, 5, 8), (80, 210, 220),
         [(W * 0.3, H * 0.3, 380, (20, 80, 90), 0.8), (W * 0.78, H * 0.62, 340, (20, 70, 100), 0.7)], 21)
# Act 2, the Living Woods: moss and amber light.
backdrop("woods", (8, 16, 9), (3, 6, 3), (130, 210, 110),
         [(W * 0.7, H * 0.25, 400, (90, 110, 30), 0.7), (W * 0.25, H * 0.7, 320, (30, 90, 40), 0.7)], 31, pines)
# Act 3, the Tower of Owen: violet.
backdrop("tower", (14, 8, 22), (5, 3, 9), (190, 130, 255),
         [(W * 0.5, H * 0.2, 420, (90, 40, 140), 0.8), (W * 0.15, H * 0.8, 300, (70, 30, 110), 0.6)], 41, columns)
print("wrote", sorted(os.listdir(OUT)))
