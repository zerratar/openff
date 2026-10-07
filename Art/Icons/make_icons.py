"""OpenFF's icons from the PNGs here: each cropped to its shape, centred on a square with a little margin, written as a
multi-size .ico (16..256), and for the game a 256 x 256 32-bit BMP with alpha (MonoGame's window icon, Icon.bmp)."""
import struct
from PIL import Image

SIZES = [16, 20, 24, 32, 40, 48, 64, 128, 256]

def square(path, margin=0.04):
    im = Image.open(path).convert('RGBA')
    im = im.crop(im.getbbox())
    side = int(max(im.size) * (1 + 2 * margin))
    out = Image.new('RGBA', (side, side), (0, 0, 0, 0))
    out.paste(im, ((side - im.width) // 2, (side - im.height) // 2), im)
    return out

def ico(img, path):
    img.save(path, format='ICO', sizes=[(s, s) for s in SIZES])

def bmp_with_alpha(img, path, size=256):
    im = img.resize((size, size), Image.LANCZOS)
    w, h = im.size
    px = im.tobytes('raw', 'BGRA', 0, -1)   # bottom-up rows
    header = struct.pack('<IiiHHIIiiII', 108, w, h, 1, 32, 3, len(px), 2835, 2835, 0, 0)
    masks = struct.pack('<IIII', 0x00FF0000, 0x0000FF00, 0x000000FF, 0xFF000000) + b'BGRs' + b'\0' * 36 + b'\0' * 12
    info = header + masks
    off = 14 + len(info)
    open(path, 'wb').write(b'BM' + struct.pack('<IHHI', off + len(px), 0, 0, off) + info + px)

openff = square('openff.png')
ico(openff, '../../OpenFF/Icon.ico')
bmp_with_alpha(openff, '../../OpenFF/Icon.bmp')
crystal = square('crystal.png')
ico(crystal, '../../Crystal.Editor/Icon.ico')
crystal.save('../../Crystal.Editor/wwwroot/favicon.ico', format='ICO', sizes=[(16, 16), (32, 32), (48, 48)])   # the editor page's
ico(square('updater.png'), '../../OpenFF.Updater/Icon.ico')
print('ok')
