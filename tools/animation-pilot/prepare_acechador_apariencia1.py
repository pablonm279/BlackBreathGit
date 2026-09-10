"""Prepara exclusivamente Acechador apariencia 1, sin regenerar otras unidades.

Requiere Pillow, numpy y scipy. Reutiliza el recorte con descontaminacion de
bordes autorizado; fuentes y prompts en output/animation-acechador-apariencia1.
"""
import importlib.util
import json
from pathlib import Path
import re

from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
WORK = ROOT / "output/animation-acechador-apariencia1"
DEST = ROOT / "Assets/Resources/AnimacionesIlustradas/AcechadorApariencia1"
ORIGINAL = ROOT / "Assets/Scripts/Clases/Acechador"
SPEC = importlib.util.spec_from_file_location("preparacion", Path(__file__).with_name("prepare_caballero_apariencia2.py"))
HELPER = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(HELPER)
PILOT = HELPER.PILOT

# Lienzo mas ancho para la estocada completa; no se reduce el cuerpo para encajar
# la espada corta. La malla corrige el ancho y devuelve el apoyo al encuadre original.
SIZE = (1024, 768)
ANCHOR_X = 0.5
FOOT_DEST = 0.495
GROUND = 16 / 768
FRAMES = {
    "reposo": (0.605, 628),
    "anticipacion": (0.585, 675),
    "impacto": (0.615, 720),
}


def main():
    DEST.mkdir(parents=True, exist_ok=True)
    folder_meta = Path(str(DEST)+".meta")
    if not folder_meta.exists():
        folder_meta.write_text(f"fileFormatVersion: 2\nguid: {PILOT.guid(DEST)}\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {{}}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n", encoding="utf-8")
    report = {}
    for name, (scale, feet_x) in FRAMES.items():
        source = WORK/(name+"-fuente.png")
        clean = HELPER.cutout(Image.open(source))
        bounds = clean.getbbox()
        cropped = clean.crop(bounds)
        scaled = cropped.resize((round(cropped.width*scale), round(cropped.height*scale)), Image.Resampling.LANCZOS)
        x = round(SIZE[0]*ANCHOR_X-(feet_x-bounds[0])*scale)
        y = round(SIZE[1]*(1-GROUND)-scaled.height)
        assert x > 0 and y > 0 and x+scaled.width < SIZE[0] and y+scaled.height < SIZE[1], (name, x, y, scaled.size)
        sprite = Image.new("RGBA", SIZE)
        sprite.alpha_composite(scaled, (x, y))
        path = DEST/(name+".png")
        sprite.save(path)
        meta = Path(str(path)+".meta")
        if not meta.exists():
            PILOT.sprite_meta(path)
        report[name] = {"source": source.name, "scale": scale, "source_feet_x": feet_x,
                        "source_bounds": bounds, "pixels": SIZE, "output_bounds": sprite.getbbox()}
    template = (DEST.parent/"CaballeroApariencia1.asset").read_text(encoding="utf-8")
    template = template.replace("m_Name: CaballeroApariencia1", "m_Name: AcechadorApariencia1")
    origins = {"origenIdle": "Acechador_idle.png", "origenMover": "Acechador_moviendo.png",
               "origenAtacar": "Acechador_atacando.png", "origenTurnoActivo": "Acechador_idle2.png"}
    for field, filename in origins.items():
        template = re.sub(r"  "+field+r": .*", f"  {field}: {{fileID: 21300000, guid: {PILOT.guid(ORIGINAL/filename)}, type: 3}}", template)
    for field in FRAMES:
        template = re.sub(r"  "+field+r": .*", f"  {field}: {{fileID: 21300000, guid: {PILOT.guid(DEST/(field+'.png'))}, type: 3}}", template)
    template = re.sub(r"  apoyo: .*", f"  apoyo: {{x: {ANCHOR_X}, y: {GROUND}}}", template)
    template = re.sub(r"  escalaHorizontal: .*", f"  escalaHorizontal: {SIZE[0]/750}", template)
    template = re.sub(r"  desplazamientoHorizontal: .*", f"  desplazamientoHorizontal: {FOOT_DEST-ANCHOR_X}", template)
    profile = DEST.parent/"AcechadorApariencia1.asset"
    profile.write_text(template, encoding="utf-8")
    meta = Path(str(profile)+".meta")
    if not meta.exists():
        meta.write_text(f"fileFormatVersion: 2\nguid: {PILOT.guid(profile)}\nNativeFormatImporter:\n  externalObjects: {{}}\n  mainObjectFileID: 11400000\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n", encoding="utf-8")
    (WORK/"registro.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    print("Acechador apariencia 1: tres sprites RGBA y perfil preparados.")


if __name__ == "__main__":
    main()
