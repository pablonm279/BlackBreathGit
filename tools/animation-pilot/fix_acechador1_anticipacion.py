"""Corrige solo anticipacion de Acechador 1; conserva perfil, metas y otros frames."""
from pathlib import Path
import importlib.util
import json
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
WORK = ROOT / 'output/animation-acechador-apariencia1/correccion-espada'
DEST = ROOT / 'Assets/Resources/AnimacionesIlustradas/AcechadorApariencia1/anticipacion.png'
spec = importlib.util.spec_from_file_location('helper', Path(__file__).with_name('prepare_explorador_apariencia2.py'))
helper = importlib.util.module_from_spec(spec)
spec.loader.exec_module(helper)

def main():
    clean = helper.cutout(Image.open(WORK / 'fuente-final.png'))
    bounds = clean.getbbox()
    # Cabeza y botas registradas contra impacto; no normalizar por la espada.
    scale, foot_x = 0.711, 707
    crop = clean.crop(bounds)
    crop = crop.resize((round(crop.width*scale), round(crop.height*scale)), Image.Resampling.LANCZOS)
    x = round(512-(foot_x-bounds[0])*scale)
    y = 752-crop.height
    assert x > 0 and y > 0 and x+crop.width < 1024
    sprite = Image.new('RGBA', (1024,768))
    sprite.alpha_composite(crop,(x,y))
    sprite.save(WORK / 'anticipacion-corregida.png')
    report = {'source_bounds':bounds,'scale':scale,'source_feet_x':foot_x,'output_bounds':sprite.getbbox()}
    (WORK / 'registro.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
    # Contacto sobre tres fondos, solo QA interna; no modifica la preview HTML.
    sheet = Image.new('RGB',(1024,768*3))
    for i,bg in enumerate(('#171b20','#d5d5d5','#174c49')):
        panel = Image.new('RGBA',sprite.size,bg)
        panel.alpha_composite(sprite)
        sheet.paste(panel.convert('RGB'),(0,i*768))
    sheet.resize((512,1152)).save(WORK / 'qa.png')
    print(report)

if __name__ == '__main__':
    main()
