from pathlib import Path
from PIL import Image, ImageDraw
import numpy as np
import re

ROOT = Path(__file__).resolve().parents[2]
WORK = Path(__file__).resolve().parent
DEST = ROOT/'Assets/Resources/AnimacionesIlustradas/ExploradorApariencia2'
names = ['reposo', 'anticipacion', 'impacto', 'preparacion']
profile = (DEST.parent/'ExploradorApariencia2.asset').read_text()
prefab = (ROOT/'Assets/Scripts/Clases/Explorador/UnidadExplorador.prefab').read_text()
for field in ['origenIdle', 'origenMover', 'origenAtacar', 'origenTurnoActivo']:
    assert re.search(field+r': .*guid: ([0-9a-f]+)', profile)[1] in prefab
imgs = []
for name in names:
    p = DEST/(name+'.png')
    im = Image.open(p)
    a = np.array(im)
    assert im.mode == 'RGBA' and im.size == (1024, 768)
    assert a[:, :, 3].min() == 0 and a[:, :, 3].max() == 255
    assert not np.any(np.concatenate([a[0, :, 3], a[-1, :, 3], a[:, 0, 3], a[:, -1, 3]]))
    meta = Path(str(p)+'.meta').read_text()
    assert re.search('guid: ([0-9a-f]+)', meta)[1] in profile
    for flag in ['alphaIsTransparency: 1', 'enableMipMap: 0', 'textureCompression: 0', 'spriteMode: 1', 'spriteMeshType: 1', 'filterMode: 1', 'wrapU: 1']:
        assert flag in meta
    imgs.append(im)
pairs = []
for p in DEST.parent.glob('*.asset'):
    data = p.read_text()
    pair = tuple(re.search(f+r': .*guid: ([0-9a-f]+)', data)[1] for f in ['origenMover', 'origenAtacar'])
    assert pair not in pairs
    pairs.append(pair)
original = Image.open(ROOT/'Assets/Scripts/Clases/Explorador/Aparienciaalternatia/Explorador_idle2.png').convert('RGBA').resize((750, 768), Image.Resampling.LANCZOS)
canvas = Image.new('RGBA', (1024, 768))
canvas.alpha_composite(original, (114, 0))
all_images = [canvas]+imgs
sheet = Image.new('RGB', (1600, 780))
draw = ImageDraw.Draw(sheet)
for row, bg in enumerate(['#181c22', '#ece8df', '#24746a']):
    for col, im in enumerate(all_images):
        panel = Image.new('RGBA', im.size, bg)
        panel.alpha_composite(im)
        sheet.paste(panel.resize((320, 240), Image.Resampling.LANCZOS), (col*320, row*260+20))
        draw.text((col*320+8, row*260+4), (['atento original']+names)[col], fill='white')
sheet.save(WORK/'revision.png')
zoom = Image.new('RGB', (1536, 720), '#111820')
for i, im in enumerate([imgs[0], imgs[2], imgs[3]]):
    panel = Image.new('RGBA', im.size, '#111820')
    panel.alpha_composite(im)
    zoom.paste(panel.crop((210, 28, 722, 748)), (i*512, 0))
zoom.save(WORK/'bordes.png')
print('Cuatro sprites verificados;', len(pairs), 'perfiles con origen unico. QA regenerada.')
