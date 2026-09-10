# Correccion del ataque de Acechador 1

El frame anterior de anticipacion orientaba la hoja hacia la derecha, cruzando el cuerpo, mientras atento e impacto la orientaban hacia la izquierda. Tambien cambiaba el apoyo corporal antes de la estocada.

Se reemplazo solamente `Assets/Resources/AnimacionesIlustradas/AcechadorApariencia1/anticipacion.png`. La nueva preparacion recoge el codo y mantiene la hoja hacia delante y ligeramente abajo. Cabeza y botas se registran contra impacto: techo de figura y suelo aproximadamente 92/752 px frente a 96/752 px del impacto. La mano y la punta avanzan hacia la izquierda al extender el brazo.

Generado con image_gen integrado. Prompt final exacto en `prompt-final.md`, fuente final en `fuente-final.png`; primer intento descartado en `fuente.png`. Se conserva la anticipacion anterior para comparacion. Recorte magenta, descontaminacion y registro reproducibles con `tools/animation-pilot/fix_acechador1_anticipacion.py`; copiar su `anticipacion-corregida.png` al destino para instalar. Si se ejecuta el preparador historico completo de Acechador 1, volver a aplicar esta correccion al final.

Validado: RGBA 1024x768, fondo transparente, contorno sobre fondos oscuro/claro/verde, margen y suelo. Perfil, metas/GUID, impacto, reposo, otras apariencias y codigo de combate intactos. No se regenero la preview. Pendiente verificacion del ataque en Play Mode; no se ejecuto el tutorial. Al ser un cambio de textura no cambia su logica ni sus tiempos.
