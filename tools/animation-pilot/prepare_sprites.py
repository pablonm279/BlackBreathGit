"""Preparacion autorizada: matte, registro de pies y sprites RGBA del piloto.

Requiere Pillow, numpy y scipy. No modifica las ilustraciones originales.
Las laminas fuente se conservan en output/animation-pilot.
"""
from pathlib import Path
import argparse
import json
import re
import uuid

import numpy as np
from PIL import Image, ImageDraw
from scipy import ndimage

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "output/animation-pilot"
ASSETS = ROOT / "Assets/Resources/AnimacionesIlustradas"
NAMES = ["reposo", "paso-a", "paso-b", "anticipacion", "impacto", "danio"]
CONFIG = {
    "CaballeroApariencia1": ("caballero-generado.png", 1.46, 0.074, 0.55,
        "Assets/Scripts/Clases/Caballero/UnidadCaballero.prefab"),
    "DriadaQuemada": ("driada-generada.png", 1.14, 0.012, 0.49,
        "Assets/Prefabs/Prefabs NPC/ZONA - Bosque de los Lamentos/DriadaQuemada/DriadaQuemada.prefab"),
}


def guid(path):
    meta = Path(str(path) + ".meta")
    if meta.exists():
        return re.search(r"^guid: (\w+)", meta.read_text(encoding="utf-8"), re.M)[1]
    return uuid.uuid5(uuid.NAMESPACE_URL, "gdd-animation-pilot/" + path.relative_to(ROOT).as_posix()).hex


def sprite_meta(path):
    template = (ROOT / "Assets/Scripts/Clases/Caballero/Caballero_idle.png.meta").read_text(encoding="utf-8")
    template = re.sub(r"^guid: \w+", "guid: " + guid(path), template, flags=re.M)
    template = template.replace("textureCompression: 2", "textureCompression: 0")
    template = template.replace("spriteGenerateFallbackPhysicsShape: 1", "spriteGenerateFallbackPhysicsShape: 0")
    Path(str(path) + ".meta").write_text(template, encoding="utf-8")


def extract(source):
    rgb = np.array(Image.open(source).convert("RGB"))
    v = rgb.astype(np.float32)
    # El fondo generado es gris casi blanco; los contornos de ambos personajes son oscuros.
    background = (v.min(2) > 190) & ((v.max(2) - v.min(2)) < 40)
    labels, _ = ndimage.label(~background)
    objects = []
    for index, slices in enumerate(ndimage.find_objects(labels), 1):
        if slices is None or np.count_nonzero(labels == index) < 4000:
            continue
        objects.append((index, slices))
    assert len(objects) == 6, f"Expected six sprites in {source}, found {len(objects)}"
    objects.sort(key=lambda item: (item[1][0].start > 490, item[1][1].start))
    result = []
    for frame_index, (index, (ys, xs)) in enumerate(objects):
        mask = labels == index
        # Recupera el antialias de un pixel adyacente sin conservar el cuadriculado.
        fringe = ndimage.binary_dilation(mask) & ~mask
        alpha = mask.astype(np.float32)
        alpha[fringe] = np.clip((240 - v[fringe].min(1)) / 130, 0, 1)
        alpha[fringe & background] = np.minimum(alpha[fringe & background], 0.25)
        rgba = np.dstack([rgb, np.rint(alpha * 255).astype(np.uint8)])
        # Descontamina los bordes semitransparentes de su matte claro.
        partial = (alpha > 0) & (alpha < 1)
        rgba[partial, :3] = np.clip((v[partial] - 240 * (1-alpha[partial, None]))
                                   / alpha[partial, None], 0, 255).astype(np.uint8)
        if NAMES[frame_index] == "reposo":
            # El umbral del recorte deja matte claro incluso en pixeles opacos.
            # Reconstruir solo los dos pixeles de borde usando el color interior;
            # conservar el detalle y los brillos del interior de la ilustracion.
            interior = ndimage.binary_erosion(mask, iterations=2)
            distance, nearest = ndimage.distance_transform_edt(~interior, return_indices=True)
            edge = (alpha > 0) & ~interior & (distance <= 3)
            foreground = v[nearest[0][edge], nearest[1][edge]]
            observed = v[edge]
            matte = 240.0
            delta = foreground - matte
            coverage = np.clip(np.sum((observed-matte)*delta, axis=1)
                               / np.maximum(np.sum(delta*delta, axis=1), 1), 0, 1)
            contaminated = (observed.mean(1) > foreground.mean(1) + 8) & (coverage < 0.96)
            edge_y, edge_x = np.nonzero(edge)
            ey, ex = edge_y[contaminated], edge_x[contaminated]
            rgba[ey, ex, :3] = foreground[contaminated].astype(np.uint8)
            alpha[ey, ex] = np.minimum(alpha[ey, ex], coverage[contaminated])
            rgba[ey, ex, 3] = np.rint(alpha[ey, ex]*255).astype(np.uint8)
        rgba[alpha == 0, :3] = 0
        box = (max(0, xs.start-3), max(0, ys.start-3), min(rgb.shape[1], xs.stop+3), min(rgb.shape[0], ys.stop+3))
        # Centro de los pies en el 18% inferior; incluye el pie elevado durante el paso.
        feet_y = ys.start + round((ys.stop-ys.start)*0.82)
        foot_x = np.nonzero(mask[feet_y:ys.stop])[1]
        foot_center = (foot_x.min()+foot_x.max())/2
        result.append((Image.fromarray(rgba).crop(box), foot_center-box[0], ys.stop-box[1], box))
    return result


def main(idle_only=False):
    report = {}
    profile_script = ROOT / "Assets/Scripts/Visual/PerfilAnimacionIlustrada.cs"
    for path in ([] if idle_only else [profile_script, ROOT / "Assets/Scripts/Visual/UnidadAnimacionIlustrada.cs"]):
        Path(str(path)+".meta").write_text(f"fileFormatVersion: 2\nguid: {guid(path)}\n", encoding="utf-8")
    contact = Image.new("RGB", (3*384, 2*384), "#18212b")
    draw = ImageDraw.Draw(contact)
    for row, (name, (filename, scale, ground, foot_dest, prefab)) in enumerate(CONFIG.items()):
        frames = extract(OUT/filename)
        folder = ASSETS/name
        folder.mkdir(parents=True, exist_ok=True)
        info = []
        for frame_name, (cutout, foot_x, bottom, box) in zip(NAMES, frames):
            if frame_name.startswith("paso-") or frame_name == "danio":
                continue  # Dash original y dano sin sprite propio, segun revision del usuario.
            if idle_only and frame_name != "reposo":
                continue
            # Igualar el cuerpo del caballero con atento sin desplazar los pies.
            frame_scale = scale * (1.08 if name == "CaballeroApariencia1" and frame_name == "reposo" else 1)
            sprite = Image.new("RGBA", (1024, 768))
            scaled = cutout.resize((round(cutout.width*frame_scale), round(cutout.height*frame_scale)), Image.Resampling.LANCZOS)
            x = round(512-foot_x*frame_scale)
            y = round(768*(1-ground)-bottom*frame_scale)
            assert x >= 0 and y >= 0 and x+scaled.width <= 1024 and y+scaled.height <= 768, (name, frame_name, x, y)
            sprite.alpha_composite(scaled, (x, y))
            path = folder/(frame_name+".png")
            sprite.save(path)
            if not idle_only:
                sprite_meta(path)
            info.append({"frame": frame_name, "source_bounds": box, "pixels": list(sprite.size), "alpha": True})
        if idle_only:
            continue
        prefab_text = (ROOT/prefab).read_text(encoding="utf-8")
        origins = {key: re.findall(r"^  "+key+r": (\{[^\n]+\})", prefab_text, re.M)[-1]
                   for key in ["poseIdle", "poseMover", "poseAtacar", "poseTurnoActivo"]}
        profile = ASSETS/(name+".asset")
        text = "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n--- !u!114 &11400000\nMonoBehaviour:\n"
        text += "  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: 0}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n"
        text += f"  m_Script: {{fileID: 11500000, guid: {guid(profile_script)}, type: 3}}\n  m_Name: {name}\n  m_EditorClassIdentifier: \n"
        for key in origins:
            text += f"  origen{key[4:]}: {origins[key]}\n"
        for field, frame_name in zip(["reposo", "anticipacion", "impacto"], ["reposo", "anticipacion", "impacto"]):
            text += f"  {field}: {{fileID: 21300000, guid: {guid(folder/(frame_name+'.png'))}, type: 3}}\n"
        text += f"  apoyo: {{x: 0.5, y: {ground}}}\n  escalaHorizontal: {1024/750}\n  desplazamientoHorizontal: {foot_dest-0.5}\n  respiracion: {0.006 if row == 0 else 0.009}\n"
        profile.write_text(text, encoding="utf-8")
        Path(str(profile)+".meta").write_text(f"fileFormatVersion: 2\nguid: {guid(profile)}\nNativeFormatImporter:\n  externalObjects: {{}}\n  mainObjectFileID: 11400000\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n", encoding="utf-8")
        for col, frame_name in enumerate(["reposo", "anticipacion", "impacto"]):
            preview = Image.open(folder/(frame_name+".png"))
            preview.thumbnail((384, 350))
            contact.paste(preview, (col*384, row*384+28), preview)
            draw.text((col*384+12, row*384+10), f"{name} / {frame_name}", fill="#e6dac5")
        report[name] = info
    if idle_only:
        print("Idle corregido en Unity: escala del caballero y matte de ambos reposos; sin modificar preview ni perfiles.")
        return
    contact.save(OUT/"poses-verificadas.png")
    (OUT/"preparacion.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    print("6 sprites RGBA preparados y 2 perfiles de Unity escritos; dash original y dano sin sprite propio.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--idle-only", action="store_true", help="Actualizar solo los PNG de reposo en Unity, sin preview ni perfiles.")
    main(idle_only=parser.parse_args().idle_only)
