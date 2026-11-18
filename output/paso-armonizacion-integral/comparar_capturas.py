from pathlib import Path
from PIL import Image, ImageDraw

root = Path(__file__).resolve().parent
climas = [(1, 'Despejado'), (3, 'Lluvia'), (4, 'Nieve'), (5, 'Niebla'), (7, 'Aurora')]
posterior = next((fase for fase in ('aldea-restaurada', 'validado', 'final') if (root / fase).exists()), 'despues')
for fase in ('antes', posterior):
    for hora in ('dia', 'noche'):
        sheet = Image.new('RGB', (1280, 1320), '#111923')
        draw = ImageDraw.Draw(sheet)
        draw.text((12, 8), f'{fase.upper()} | {hora} | camara real: normal / cerca / lejos con paneo', fill='white')
        for row, (clima, nombre) in enumerate(climas):
            for cam in range(3):
                path = root / fase / f'{hora}-clima{clima}-cam{cam}.png'
                if not path.exists():
                    continue
                with Image.open(path) as img:
                    img.thumbnail((426, 240))
                    x, y = cam * 426, 30 + row * 258
                    sheet.paste(img, (x, y + 18))
                    draw.text((x + 8, y + 2), f'{nombre} / cam {cam}', fill='white')
        sheet.save(root / f'{fase}-{hora}-contacto.jpg', quality=92)

for hora in ('dia', 'noche'):
    path = root / posterior / f'{hora}-clima1-cam0.png'
    if path.exists():
        sheet = Image.new('RGB', (1280, 760), '#111923')
        draw = ImageDraw.Draw(sheet)
        for index, fase in enumerate(('antes', posterior)):
            with Image.open(root / fase / f'{hora}-clima1-cam0.png') as img:
                img = img.resize((640, 360))
                sheet.paste(img, (index * 640, 35))
            draw.text((index * 640 + 12, 12), fase.upper(), fill='white')
            with Image.open(root / fase / f'{hora}-clima4-cam1.png') as img:
                img = img.resize((640, 360))
                sheet.paste(img, (index * 640, 400))
        sheet.save(root / f'comparacion-{hora}.jpg', quality=94)
