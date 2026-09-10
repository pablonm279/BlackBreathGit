"""Prepara solo Caballero apariencia 2; no reescribe el piloto ni la preview.

Pillow, numpy y scipy. Fuentes y prompts en output/animation-caballero-apariencia2.
Procesamiento de transparencia, recorte y registro autorizado por el usuario.
"""
from pathlib import Path
import importlib.util
import json
import re
import uuid

import numpy as np
from PIL import Image
from scipy import ndimage

ROOT = Path(__file__).resolve().parents[2]
WORK = ROOT / "output/animation-caballero-apariencia2"
DEST = ROOT / "Assets/Resources/AnimacionesIlustradas/CaballeroApariencia2"
ORIGINAL = ROOT / "Assets/Scripts/Clases/Caballero/AparienciasAlternativas"
SPEC = importlib.util.spec_from_file_location("pilot", Path(__file__).with_name("prepare_sprites.py"))
PILOT = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(PILOT)

# Coordenadas medidas en las fuentes: centro entre botas, linea de apoyo.
# Cada escala se calibra con cabeza/cuerpo originales, no con la punta de espada.
FRAMES = {
    "reposo": ("lamina-descartada-solapamiento.png", (0, 0, 595, 899), 0.98, 335, 856),
    "anticipacion": ("anticipacion-fuente.png", None, 0.603, 635, 1198),
    "impacto": ("impacto-fuente.png", None, 0.635, 734, 1170),
}
GROUND = 0.054
FOOT_DEST = 0.535


def cutout(image):
    rgba = np.array(image.convert("RGBA"))
    if rgba[:, :, 3].min() < 255:
        return Image.fromarray(rgba)  # Preservar alfa real si cambia la fuente.
    rgb = rgba[:, :, :3].astype(np.float32)
    background = (rgb.min(2) > 190) & (np.ptp(rgb, axis=2) < 40)
    labels, count = ndimage.label(~background)
    areas = np.bincount(labels.ravel())
    areas[0] = 0
    mask = labels == areas.argmax()
    assert mask.sum() > 10000, "No se encontro la figura"
    # Incluye la franja de antialias sin incorporar la cuadricula desconectada.
    fringe = ndimage.binary_dilation(mask) & ~mask
    alpha = mask.astype(np.float32)
    alpha[fringe] = np.clip((240 - rgb[fringe].min(1)) / 130, 0, 1)
    alpha[fringe & background] = np.minimum(alpha[fringe & background], 0.25)
    partial = (alpha > 0) & (alpha < 1)
    rgba[partial, :3] = np.clip((rgb[partial] - 240 * (1-alpha[partial, None]))
                              / alpha[partial, None], 0, 255).astype(np.uint8)
    # Descontamina tambien el matte que el umbral dejo opaco. Solo borde exterior
    # y huecos: no elimina los brillos interiores ni el pelo gris del caballero.
    interior = ndimage.binary_erosion(mask, iterations=2)
    distance, nearest = ndimage.distance_transform_edt(~interior, return_indices=True)
    edge = (alpha > 0) & ~interior & (distance <= 3)
    foreground = rgb[nearest[0][edge], nearest[1][edge]]
    observed = rgb[edge]
    delta = foreground - 240
    coverage = np.clip(np.sum((observed-240)*delta, axis=1)
                       / np.maximum(np.sum(delta*delta, axis=1), 1), 0, 1)
    contaminated = (observed.mean(1) > foreground.mean(1)+8) & (coverage < 0.96)
    ey, ex = np.nonzero(edge)
    ey, ex = ey[contaminated], ex[contaminated]
    rgba[ey, ex, :3] = foreground[contaminated].astype(np.uint8)
    alpha[ey, ex] = np.minimum(alpha[ey, ex], coverage[contaminated])
    rgba[:, :, 3] = np.rint(alpha*255).astype(np.uint8)
    rgba[alpha == 0, :3] = 0
    return Image.fromarray(rgba)


def main():
    DEST.mkdir(parents=True, exist_ok=True)
    folder_meta = Path(str(DEST)+".meta")
    if not folder_meta.exists():
        folder_meta.write_text(f"fileFormatVersion: 2\nguid: {PILOT.guid(DEST)}\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {{}}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n", encoding="utf-8")
    report = {}
    for name, (source, region, scale, foot_x, floor_y) in FRAMES.items():
        image = Image.open(WORK/source)
        if region:
            image = image.crop(region)
        clean = cutout(image)
        bounds = clean.getbbox()
        clean = clean.crop(bounds)
        scaled = clean.resize((round(clean.width*scale), round(clean.height*scale)), Image.Resampling.LANCZOS)
        x = round(512-(foot_x-bounds[0])*scale)
        y = round(768*(1-GROUND)-(floor_y-bounds[1])*scale)
        assert x > 0 and y > 0 and x+scaled.width < 1024 and y+scaled.height < 768, (name, x, y, scaled.size)
        sprite = Image.new("RGBA", (1024, 768))
        sprite.alpha_composite(scaled, (x, y))
        path = DEST/(name+".png")
        sprite.save(path)
        if not Path(str(path)+".meta").exists():
            PILOT.sprite_meta(path)
        report[name] = {"source": source, "region": region, "scale": scale,
                        "source_feet": [foot_x, floor_y], "output_bounds": sprite.getbbox()}
    template = (DEST.parent/"CaballeroApariencia1.asset").read_text(encoding="utf-8")
    template = template.replace("m_Name: CaballeroApariencia1", "m_Name: CaballeroApariencia2")
    origins = {"origenIdle": "Caballero_idle.png", "origenMover": "Caballero_mueve.png",
               "origenAtacar": "Caballero_Ataca.png", "origenTurnoActivo": "Caballero_idle2.png"}
    for field, filename in origins.items():
        template = re.sub(r"  "+field+r": .*", f"  {field}: {{fileID: 21300000, guid: {PILOT.guid(ORIGINAL/filename)}, type: 3}}", template)
    for field in FRAMES:
        template = re.sub(r"  "+field+r": .*", f"  {field}: {{fileID: 21300000, guid: {PILOT.guid(DEST/(field+'.png'))}, type: 3}}", template)
    template = re.sub(r"  apoyo: .*", f"  apoyo: {{x: 0.5, y: {GROUND}}}", template)
    template = re.sub(r"  desplazamientoHorizontal: .*", f"  desplazamientoHorizontal: {FOOT_DEST-0.5}", template)
    profile = DEST.parent/"CaballeroApariencia2.asset"
    profile.write_text(template, encoding="utf-8")
    profile_meta = Path(str(profile)+".meta")
    if not profile_meta.exists():
        profile_meta.write_text(f"fileFormatVersion: 2\nguid: {PILOT.guid(profile)}\nNativeFormatImporter:\n  externalObjects: {{}}\n  mainObjectFileID: 11400000\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n", encoding="utf-8")
    (WORK/"registro.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    print("Caballero apariencia 2: tres sprites RGBA y perfil preparados, sin reescribir otras unidades.")


if __name__ == "__main__":
    main()
