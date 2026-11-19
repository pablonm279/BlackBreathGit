using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Preview aislado con la grilla y camara reales. No abre, guarda ni ejecuta la campana.
public static class ValidacionTablerosIntegrados
{
  private const string Salida = "output/tableros-integrados-validacion";

  [MenuItem("Tools/Combate/Tableros integrados/Validar y capturar sin alterar partida")]
  public static void Ejecutar()
  {
    if (Application.isPlaying) throw new InvalidOperationException("Esta comprobacion usa el Editor fuera de Play.");
    var admin = Resources.FindObjectsOfTypeAll<AdministradorEscenas>().First(x => x.gameObject.scene.IsValid());
    var batalla = Resources.FindObjectsOfTypeAll<BattleManager>().First(x => x.gameObject.scene.IsValid());
    var origen = admin.EscenaBatalla.GetComponentsInChildren<Casilla>(true);
    string fuenteAntes = JsonUtility.ToJson(admin);
    var estadoRandom = UnityEngine.Random.state;
    float tiempo = Time.timeScale;
    var resultados = new List<string>();
    var escena = EditorSceneManager.NewPreviewScene();
    try
    {
      Directory.CreateDirectory(Salida);
      var grillaFuente = origen[0].transform.parent.parent;
      var grilla = Object.Instantiate(grillaFuente.gameObject);
      SceneManager.MoveGameObjectToScene(grilla, escena);
      grilla.transform.SetPositionAndRotation(grillaFuente.position, grillaFuente.rotation);
      grilla.transform.localScale = grillaFuente.lossyScale;
      grilla.SetActive(true);
      foreach (var script in grilla.GetComponentsInChildren<MonoBehaviour>(true)) script.enabled = false;
      var tiles = grilla.GetComponentsInChildren<Casilla>(true).ToList();
      var suelo = grilla.transform.Find("Suelo");
      foreach (var c in tiles.Where(c => (c.lado == 2 && c.posX == 1 && c.posY == 1)
        || (c.lado == 1 && c.posX == 3 && c.posY == 4) || (c.lado == 1 && c.posX == 2 && c.posY == 1)))
      {
        var obstaculo = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Obstaculo_rocamayor.prefab"));
        SceneManager.MoveGameObjectToScene(obstaculo, escena);
        obstaculo.transform.position = c.transform.position;
        obstaculo.transform.localScale *= .9f;
        foreach (var script in obstaculo.GetComponentsInChildren<MonoBehaviour>(true))
          if (!(script is UnityEngine.UI.Graphic)) script.enabled = false;
        c.Presente = obstaculo;
      }
      var posiciones = tiles.Select(c => c.transform.position).ToArray();
      var coordenadas = tiles.Select(c => (c.lado, c.posX, c.posY, c.costoMovimiento, c.Presente)).ToArray();
      var colliders = tiles.SelectMany(c => c.GetComponentsInChildren<Collider>(true)).ToArray();
      var datosColliders = colliders.Select(c => EditorJsonUtility.ToJson(c)).ToArray();
      var transformsOriginales = tiles.SelectMany(c => c.GetComponentsInChildren<Transform>(true)).ToArray();
      var activacion = transformsOriginales.Select(t => t.gameObject.activeSelf).ToArray();
      var materiales = tiles.SelectMany(c => c.GetComponentsInChildren<Renderer>(true)).ToDictionary(r => r, r => r.sharedMaterials);
      var renderersSuelo = suelo.GetComponentsInChildren<Renderer>(true);
      var estadoSuelo = renderersSuelo.Select(r => r.enabled).ToArray();

      var goCam = new GameObject("Camara de preview", typeof(Camera));
      SceneManager.MoveGameObjectToScene(goCam, escena);
      var cam = goCam.GetComponent<Camera>();
      var camFuente = batalla.goCamara.GetComponent<Camera>();
      cam.CopyFrom(camFuente);
      cam.scene = escena;
      cam.targetTexture = null;
      cam.transform.SetPositionAndRotation(camFuente.transform.position, camFuente.transform.rotation);
      cam.aspect = 16f / 9f;
      cam.enabled = false;
      foreach (var c in tiles.Where(c => c.Presente != null))
        foreach (var canvas in c.Presente.GetComponentsInChildren<Canvas>(true)) canvas.worldCamera = cam;
      foreach (var luz in admin.EscenaBatalla.GetComponentsInChildren<Light>(true).Where(l => l.name.StartsWith("Directional")))
      {
        var goLuz = new GameObject(luz.name, typeof(Light));
        SceneManager.MoveGameObjectToScene(goLuz, escena);
        EditorUtility.CopySerialized(luz, goLuz.GetComponent<Light>());
        goLuz.transform.SetPositionAndRotation(luz.transform.position, luz.transform.rotation);
      }
      ColocarSprite(batalla.prefabUnidadCaballero, tiles.First(c => c.lado == 2 && c.posX == 2 && c.posY == 5), cam, escena);
      ColocarSprite(batalla.prefabUnidadPurificadora, tiles.First(c => c.lado == 2 && c.posX == 1 && c.posY == 3), cam, escena);
      ColocarSprite(batalla.prefabUnidadExplorador, tiles.First(c => c.lado == 2 && c.posX == 3 && c.posY == 3), cam, escena);
      var goFondo = new GameObject("Fondo real", typeof(MeshFilter), typeof(MeshRenderer));
      SceneManager.MoveGameObjectToScene(goFondo, escena);
      var fuente = admin.mrFondoBatalla;
      goFondo.transform.SetPositionAndRotation(fuente.transform.position, fuente.transform.rotation);
      goFondo.transform.localScale = fuente.transform.lossyScale;
      goFondo.GetComponent<MeshFilter>().sharedMesh = fuente.GetComponent<MeshFilter>().sharedMesh;
      var fondo = goFondo.GetComponent<MeshRenderer>();
      fondo.sortingOrder = fuente.sortingOrder;
      var go = new GameObject("Validacion suelo");
      SceneManager.MoveGameObjectToScene(go, escena);
      var tablero = go.AddComponent<TableroIntegradoBatalla>();
      var barras = tiles.Where(c => c.Presente != null).SelectMany(c => c.Presente.GetComponentsInChildren<UnityEngine.UI.Slider>(true)).ToArray();
      var valoresBarras = barras.Select(b => b.value).ToArray();
      var rotacionesBarras = barras.Select(b => b.transform.localRotation).ToArray();
      var graficosBarras = barras.SelectMany(b => b.GetComponentsInChildren<UnityEngine.UI.Graphic>(true)).ToArray();
      var graficosHabilitados = graficosBarras.Select(g => g.enabled).ToArray();
      var vidasObstaculos = tiles.Where(c => c.Presente != null).Select(c => JsonUtility.ToJson(c.Presente.GetComponent<Obstaculo>())).ToArray();

      Verificar(tiles.Count == 30 && tiles.GroupBy(c => c.lado).All(g => g.Count() == 15), "30 casillas en dos lados de 15", resultados);
      foreach (var pareja in new[] {
        (EncounterZoneType.PasoVientoHelado, admin.fondosPasoVientoHelado, "paso"),
        (EncounterZoneType.BosqueAngustiante, admin.fondosBosqueLamentos, "bosque"),
        (EncounterZoneType.Nedukazal, admin.fondosNedukazal, "nedukazal") })
      {
        for (int variante = 0; variante < pareja.Item2.Count; variante++)
        {
          tablero.Limpiar();
          fondo.sharedMaterial = pareja.Item2[variante].fondo;
          Verificar(tablero.Preparar(tiles, suelo, fondo, pareja.Item1, false), "Preparacion " + pareja.Item3 + " " + variante, resultados);
          tablero.AlinearBarras(cam);
          Verificar(BarrasLegibles(go, cam, barras.Length), "Barras horizontales y finas en pantalla " + pareja.Item3, resultados);
          Verificar(rotacionesBarras.SequenceEqual(barras.Select(b => b.transform.localRotation)), "Transform de Slider original intacto " + pareja.Item3, resultados);
          Verificar(renderersSuelo.All(r => !r.enabled), "Suelo anterior oculto " + pareja.Item3, resultados);
          Verificar(go.GetComponentsInChildren<Collider>(true).Length == 0, "Ningun collider nuevo " + pareja.Item3, resultados);
          Verificar(valoresBarras.SequenceEqual(barras.Select(b => b.value)), "Vida y fill de barras intactos " + pareja.Item3, resultados);
          Verificar(vidasObstaculos.SequenceEqual(tiles.Where(c => c.Presente != null).Select(c => JsonUtility.ToJson(c.Presente.GetComponent<Obstaculo>()))), "Obstaculos sin cambios de estado " + pareja.Item3, resultados);
          Verificar(go.GetComponentsInChildren<MeshRenderer>(true).Count(r => r.name == "Fondo vida integrado") == barras.Length,
            "Barras con fondo sin UI ni colliders nuevos " + pareja.Item3, resultados);
          Verificar(tiles.Where(c => c.Presente != null).All(c => c.transform.Find("Contacto obstaculo integrado") != null
            && c.transform.Find("Contacto obstaculo integrado").GetComponent<Collider>() == null), "Contactos sin colliders " + pareja.Item3, resultados);
          Verificar(posiciones.SequenceEqual(tiles.Select(c => c.transform.position)), "Posiciones intactas " + pareja.Item3, resultados);
          Verificar(coordenadas.SequenceEqual(tiles.Select(c => (c.lado, c.posX, c.posY, c.costoMovimiento, c.Presente))), "Coordenadas, ocupacion y costes intactos " + pareja.Item3, resultados);
          Verificar(activacion.SequenceEqual(transformsOriginales.Select(t => t.gameObject.activeSelf)), "Activacion de capas intacta " + pareja.Item3, resultados);
          if (variante == 0)
          {
            MostrarEstados(tiles);
            Capturar(cam, pareja.Item3 + "-implementado");
            RestaurarActivacion(transformsOriginales, activacion);
            var barra = barras[0];
            float valorAnterior = barra.value;
            barra.SetValueWithoutNotify(Mathf.Lerp(barra.minValue, barra.maxValue, .25f));
            tablero.AlinearBarras(cam);
            var dibujo = go.GetComponentsInChildren<MeshFilter>(true).First(f => f.name == "Fondo vida integrado");
            var vertices = dibujo.sharedMesh.vertices;
            float proporcion = Vector3.Distance(vertices[4], vertices[5]) / Vector3.Distance(vertices[0], vertices[1]);
            Verificar(proporcion > .2f && proporcion < .3f, "Relleno visual sigue 25 por ciento del Slider real " + pareja.Item3, resultados);
            barra.SetValueWithoutNotify(valorAnterior);
            var ocupada = tiles.First(c => c.Presente != null);
            ocupada.Presente.SetActive(false);
            tablero.Activar(false);
            tablero.Activar(true);
            tablero.AlinearBarras(cam);
            Verificar(go.GetComponentsInChildren<MeshRenderer>().Count(r => r.name == "Fondo vida integrado") == barras.Length - 1
              && !ocupada.transform.Find("Contacto obstaculo integrado").gameObject.activeSelf,
              "Barra y sombra se ocultan al retirar obstaculo " + pareja.Item3, resultados);
            ocupada.Presente.SetActive(true);
            tablero.Activar(false);
            tablero.Activar(true);
            tablero.AlinearBarras(cam);
          }
          tablero.Activar(false);
          Verificar(rotacionesBarras.SequenceEqual(barras.Select(b => b.transform.localRotation)), "Orientacion original de barras restaurada " + pareja.Item3, resultados);
          Verificar(materiales.All(p => p.Key.sharedMaterials.SequenceEqual(p.Value)), "Materiales originales restaurados " + pareja.Item3, resultados);
          Verificar(estadoSuelo.SequenceEqual(renderersSuelo.Select(r => r.enabled)), "Placa original restaurada " + pareja.Item3, resultados);
          Verificar(graficosHabilitados.SequenceEqual(graficosBarras.Select(g => g.enabled)), "Graficos originales de barras restaurados " + pareja.Item3, resultados);
          tablero.Activar(true);
          Verificar(go.GetComponentsInChildren<MeshRenderer>(true).Count(r => r.name.StartsWith("Tablero integrado")) == 1, "Un solo suelo sin acumulacion " + pareja.Item3, resultados);
          tablero.Limpiar();
          Verificar(tiles.All(c => c.transform.Find("Contacto obstaculo integrado") == null), "Sombras liberadas " + pareja.Item3, resultados);
        }
      }
      Verificar(!tablero.Preparar(tiles, suelo, fondo, EncounterZoneType.Subterraneo, true), "Subterraneo conserva su suelo", resultados);
      // Una caverna puede ser el encuentro anterior: su diorama debe liberar la placa antes del tablero nuevo.
      var area = new DioramaBatalla.Area { centro = suelo.position, derecha = suelo.right, normal = suelo.up,
        adelante = suelo.forward, mitadTablero = Vector2.one * 2f, celda = .7f };
      var perfil = DioramaBatalla.Perfil.Para(EncounterZoneType.Subterraneo, true, false, Color.gray, Color.white, Color.gray);
      var anterior = DioramaBatalla.Crear(go.transform, area, null, null, suelo, perfil, 1f);
      anterior?.Activar(false);
      anterior?.Activar(true);
      Verificar(anterior != null && renderersSuelo.All(r => !r.enabled), "Diorama anterior de caverna preparado", resultados);
      anterior.Liberar();
      Verificar(tablero.Preparar(tiles, suelo, fondo, EncounterZoneType.PasoVientoHelado, false), "Transicion caverna a Paso", resultados);
      var atmosferico = DioramaBatalla.Crear(go.transform, area, null, null, suelo, perfil, 1f, true);
      Verificar(!go.GetComponentsInChildren<MeshRenderer>(true).Any(r => r.name == "Placa del tablero" || r.name == "Borde del tablero"), "Sin placa ni borde duplicados en zonas integradas", resultados);
      atmosferico.Liberar();
      Verificar(renderersSuelo.All(r => !r.enabled), "Ambiente no reactiva la placa bajo el suelo integrado", resultados);
      tablero.Limpiar();
      Verificar(estadoSuelo.SequenceEqual(renderersSuelo.Select(r => r.enabled)), "Restauracion tras cambiar de bioma", resultados);
      Verificar(materiales.All(p => p.Key.sharedMaterials.SequenceEqual(p.Value)), "Sin sustituciones fuera de las zonas aprobadas", resultados);
      Verificar(datosColliders.SequenceEqual(colliders.Select(c => EditorJsonUtility.ToJson(c))), "Colliders sin cambios", resultados);
      Verificar(UnityEngine.Random.state.Equals(estadoRandom), "RNG de combate intacto", resultados);
      Verificar(Time.timeScale == tiempo, "Reloj de combate intacto", resultados);
      Verificar(JsonUtility.ToJson(admin) == fuenteAntes, "Administrador real sin cambios", resultados);
      foreach (var nombre in new[] { "Suelo", "Borde" })
      {
        var shader = Resources.Load<Shader>("TablerosIntegrados/" + nombre);
        Verificar(shader != null && shader.isSupported && ShaderUtil.GetShaderMessages(shader).All(m => m.severity != UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error), "Shader " + nombre + " compilado", resultados);
      }
      File.WriteAllLines(Salida + "/validacion.txt", resultados);
      Debug.Log("Tableros integrados: " + resultados.Count + " comprobaciones correctas. Capturas en " + Salida);
    }
    catch (Exception ex)
    {
      resultados.Add("FALLO " + ex);
      Directory.CreateDirectory(Salida);
      File.WriteAllLines(Salida + "/validacion.txt", resultados);
      throw;
    }
    finally { EditorSceneManager.ClosePreviewScene(escena); }
  }

  private static void Verificar(bool ok, string nombre, List<string> resultados)
  {
    if (!ok) throw new Exception(nombre);
    resultados.Add("OK " + nombre);
  }

  private static bool BarrasLegibles(GameObject raiz, Camera camara, int cantidad)
  {
    var barras = raiz.GetComponentsInChildren<MeshFilter>(true).Where(f => f.name == "Fondo vida integrado").ToArray();
    if (barras.Length != cantidad) return false;
    foreach (var barra in barras)
    {
      var vertices = barra.sharedMesh.vertices;
      var p = vertices.Take(4).Select(v => camara.WorldToViewportPoint(barra.transform.TransformPoint(v))).ToArray();
      float ancho = p.Max(v => v.x) - p.Min(v => v.x), alto = p.Max(v => v.y) - p.Min(v => v.y);
      if (Mathf.Abs(p[0].y - p[1].y) > .001f || ancho <= .005f || ancho > .12f || alto <= 0f || alto > .018f) return false;
    }
    return true;
  }

  private static void ColocarSprite(GameObject prefab, Casilla tile, Camera cam, Scene escena)
  {
    // Copia solo el grafico del prefab: ningun script de unidad se ejecuta en el preview.
    var fuente = prefab != null ? prefab.GetComponent<Unidad>()?.uImage : null;
    if (fuente == null || fuente.sprite == null) return;
    var canvasGO = new GameObject("Sprite de referencia", typeof(RectTransform), typeof(Canvas));
    SceneManager.MoveGameObjectToScene(canvasGO, escena);
    var canvas = canvasGO.GetComponent<Canvas>();
    canvas.renderMode = RenderMode.WorldSpace;
    canvas.worldCamera = cam;
    canvas.overrideSorting = true;
    canvas.sortingOrder = RenderOrderHelper.CalcularOrdenPorY(tile.posY);
    var imagenGO = new GameObject("Grafico original", typeof(RectTransform), typeof(UnityEngine.UI.Image));
    imagenGO.transform.SetParent(canvasGO.transform, false);
    var imagen = imagenGO.GetComponent<UnityEngine.UI.Image>();
    EditorUtility.CopySerialized(fuente, imagen);
    imagen.raycastTarget = false;
    imagen.enabled = true;
    imagen.rectTransform.sizeDelta = fuente.rectTransform.rect.size;
    imagen.rectTransform.pivot = fuente.rectTransform.pivot;
    imagen.transform.SetPositionAndRotation(tile.transform.position + (fuente.transform.position - prefab.transform.position) * .9f, fuente.transform.rotation);
    imagen.transform.localScale = fuente.transform.lossyScale * .9f;
  }

  private static void MostrarEstados(List<Casilla> tiles)
  {
    // Solo en la copia: representa los estados reales sin ejecutar BattleManager ni Casilla.Update.
    foreach (var c in tiles)
    {
      foreach (var nombre in new[] { "GraficoBorde Habilidad", "GraficoBorde Habilidad amiga", "GraficoBorde Actual" })
        foreach (var t in c.GetComponentsInChildren<Transform>(true)) if (t.name == nombre) t.gameObject.SetActive(false);
      foreach (var objeto in new[] { c.Actual, c.Mover, c.MoverCostoso, c.Desplazable, c.MarcaMelee, c.OcupadoNegro, c.Sombra })
        if (objeto != null) objeto.SetActive(false);
      foreach (var objeto in new[] { c.MarcaMovX0Y1, c.MarcaMovX1Y1, c.MarcaMovX1Y0, c.MarcaMovXv1Y1,
        c.MarcaMovXv1Y0, c.MarcaMovX1Yv1, c.MarcaMovX0Yv1, c.MarcaMovXv1Yv1 })
        if (objeto != null) objeto.SetActive(false);
      if (c.Borde != null) c.Borde.SetActive(true);
      if (c.Presente != null) { c.Borde.SetActive(false); c.OcupadoNegro.SetActive(true); continue; }
      if (c.lado != 2) continue;
      if (c.posX == 2 && c.posY == 5) { c.Borde.SetActive(false); c.Actual.SetActive(true); }
      else if ((c.posX == 1 && c.posY == 5) || (c.posX == 2 && c.posY == 4)) { c.Borde.SetActive(false); c.Mover.SetActive(true); }
      else if (c.posX == 1 && c.posY == 4) { c.Borde.SetActive(false); c.MoverCostoso.SetActive(true); }
      if (c.posX == 3 && c.posY <= 3) c.MarcaMelee.SetActive(true);
    }
  }

  private static void RestaurarActivacion(Transform[] transforms, bool[] estados)
  {
    for (int i = 0; i < transforms.Length; i++) transforms[i].gameObject.SetActive(estados[i]);
  }

  private static void Capturar(Camera cam, string nombre)
  {
    var destino = new RenderTexture(1600, 900, 24);
    var previo = RenderTexture.active;
    var tex = new Texture2D(1600, 900, TextureFormat.RGB24, false);
    try
    {
      cam.targetTexture = destino;
      cam.Render();
      RenderTexture.active = destino;
      tex.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0);
      tex.Apply();
      File.WriteAllBytes(Salida + "/" + nombre + ".png", tex.EncodeToPNG());
    }
    finally
    {
      cam.targetTexture = null;
      RenderTexture.active = previo;
      Object.DestroyImmediate(tex);
      Object.DestroyImmediate(destino);
    }
  }
}
