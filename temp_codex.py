from pathlib import Path

path = Path(r"Assets/Scripts/Unidad.cs")
text = path.read_text(encoding="utf-8")

old_header = '  [Header("Sonidos")]\n  public List<AudioClip> sonidosRecibirDanio = new List<AudioClip>();\n  private AudioSource audioSource;\n'
new_header = '  [Header("Sonidos")]\n  public bool reproducirSonidoMovimiento = true;\n  public List<AudioClip> sonidosRecibirDanio = new List<AudioClip>();\n  private AudioSource audioSource;\n'

if old_header not in text:
    raise SystemExit("Header block not matched")

text = text.replace(old_header, new_header, 1)

old_method_anchor = '  private int ultimoSonidoDanioIndex = -1;\n  public async Task ReproducirSonidoRecibirDanio(int tipodanio)\n  {\n'
new_method_block = '  private int ultimoSonidoDanioIndex = -1;\n  private void ReproducirSonidoMovimientoLigero()\n  {\n    if (!reproducirSonidoMovimiento) { return; }\n\n    var contenedor = BattleManager.Instance?.contenedorPrefabs;\n    var clip = contenedor?.sonidoMovimientoLigero;\n    if (clip == null) { return; }\n\n    if (audioSource == null)\n    {\n      audioSource = GetComponent<AudioSource>();\n      if (audioSource == null)\n      {\n        audioSource = gameObject.AddComponent<AudioSource>();\n      }\n    }\n\n    audioSource.PlayOneShot(clip);\n  }\n\n  public async Task ReproducirSonidoRecibirDanio(int tipodanio)\n  {\n'

if old_method_anchor not in text:
    raise SystemExit("Damage sound method anchor not found")

text = text.replace(old_method_anchor, new_method_block, 1)

segment = '        if (!movimientoEnCurso)\n        {\n          if (poseController != null) { poseController.OnStartMove(); }\n          // Guardar casilla origen y limpiar Presente una sola vez\n'
replacement = '        if (!movimientoEnCurso)\n        {\n          if (poseController != null) { poseController.OnStartMove(); }\n          ReproducirSonidoMovimientoLigero();\n          // Guardar casilla origen y limpiar Presente una sola vez\n'

if text.count(segment) < 2:
    raise SystemExit("Movement block not found twice")

text = text.replace(segment, replacement, 2)

path.write_text(text, encoding="utf-8")