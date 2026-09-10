"""Prepara exclusivamente Purificadora apariencia 1, sin regenerar otras unidades.

Requiere Pillow, numpy y scipy. Reutiliza el recorte con descontaminacion de
bordes autorizado; fuentes y prompts en output/animation-purificadora-apariencia1.
"""
import importlib.util
import json
from pathlib import Path
import re

from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
WORK = ROOT / "output/animation-purificadora-apariencia1"
DEST = ROOT / "Assets/Resources/AnimacionesIlustradas/PurificadoraApariencia1"
ORIGINAL = ROOT / "Assets/Scripts/Clases/Purificadora"
SPEC = importlib.util.spec_from_file_location("preparacion", Path(__file__).with_name("prepare_caballero_apariencia2.py"))
HELPER = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(HELPER)
PILOT = HELPER.PILOT

# El lienzo incluye el empuje de ambas manos; se calibra por cuerpo y pies.
SIZE = (1024, 768)
ANCHOR_X = 0.5
FOOT_DEST = 0.49
GROUND = 8 / 768
FRAMES = {
    "reposo": (0.583, 600),
    "anticipacion": (0.60, 670),
    "impacto": (0.55, 650),
}


def cutout(image):
    """Preserva alfa real; extrae magenta sin confundir pelo y tunica claros."""
    import numpy as np
    from scipy import ndimage
    rgba = np.array(image.convert("RGBA"))
    if rgba[:, :, 3].min() < 255:
        return image.convert("RGBA")
    rgb = rgba[:, :, :3].astype(float)
    key = np.array([255., 0., 255.])
    # Magenta no pertenece a la paleta del personaje. Incluye huecos internos.
    chroma = np.minimum(rgb[:, :, 0], rgb[:, :, 2]) - rgb[:, :, 1]
    solid = chroma < 20
    interior = ndimage.binary_erosion(solid, iterations=2)
    dist, near = ndimage.distance_transform_edt(~interior, return_indices=True)
    fg = rgb[near[0], near[1]]
    direction = fg-key
    coverage = np.clip(np.sum((rgb-key)*direction, axis=2) / np.maximum(np.sum(direction*direction, axis=2), 1), 0, 1)
    alpha = solid.astype(float)
    edge = (dist <= 5) & (~interior)
    alpha[edge] = coverage[edge]
    rgb[edge] = fg[edge]
    alpha[alpha < 0.035] = 0
    rgba[:, :, :3] = np.clip(rgb, 0, 255).astype("uint8")
    rgba[:, :, 3] = np.round(alpha*255).astype("uint8")
    rgba[rgba[:, :, 3] == 0, :3] = 0
    return Image.fromarray(rgba)


def main():
    DEST.mkdir(parents=True, exist_ok=True)
    folder_meta = Path(str(DEST)+".meta")
    if not folder_meta.exists():
        folder_meta.write_text(f"fileFormatVersion: 2\nguid: {PILOT.guid(DEST)}\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {{}}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n", encoding="utf-8")
    report = {}
    for name, (scale, feet_x) in FRAMES.items():
        source = WORK/(name+"-fuente.png")
        clean = cutout(Image.open(source))
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
    template = template.replace("m_Name: CaballeroApariencia1", "m_Name: PurificadoraApariencia1")
    origins = {"origenIdle": "Purificadora_Idle.png", "origenMover": "purificadora_mover.png",
               "origenAtacar": "Purificadora_ataque.png", "origenTurnoActivo": "Purificadora_Idle2.png"}
    for field, filename in origins.items():
        template = re.sub(r"  "+field+r": .*", f"  {field}: {{fileID: 21300000, guid: {PILOT.guid(ORIGINAL/filename)}, type: 3}}", template)
    for field in FRAMES:
        template = re.sub(r"  "+field+r": .*", f"  {field}: {{fileID: 21300000, guid: {PILOT.guid(DEST/(field+'.png'))}, type: 3}}", template)
    template = re.sub(r"  apoyo: .*", f"  apoyo: {{x: {ANCHOR_X}, y: {GROUND}}}", template)
    template = re.sub(r"  escalaHorizontal: .*", f"  escalaHorizontal: {SIZE[0]/750}", template)
    template = re.sub(r"  desplazamientoHorizontal: .*", f"  desplazamientoHorizontal: {FOOT_DEST-ANCHOR_X}", template)
    profile = DEST.parent/"PurificadoraApariencia1.asset"
    profile.write_text(template, encoding="utf-8")
    meta = Path(str(profile)+".meta")
    if not meta.exists():
        meta.write_text(f"fileFormatVersion: 2\nguid: {PILOT.guid(profile)}\nNativeFormatImporter:\n  externalObjects: {{}}\n  mainObjectFileID: 11400000\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n", encoding="utf-8")
    (WORK/"registro.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    print("Purificadora apariencia 1: tres sprites RGBA y perfil preparados.")


if __name__ == "__main__":
    main()
