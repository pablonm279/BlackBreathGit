using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Trabaja sobre una escena temporal en memoria; nunca guarda la campaña.
public static class ValidacionCampoVivo
{
  private static BindingFlags privado = BindingFlags.Instance | BindingFlags.NonPublic;

  [MenuItem("Tools/Combate/Campo vivo/Seleccionar ajustes (reversible)")]
  private static void Seleccionar() { Selection.activeObject = AjustesCampoVivo.Actual; }

  [MenuItem("Tools/Combate/Campo vivo/Comparar - aspecto anterior (Play)")]
  private static void Anterior() { if (Application.isPlaying) AjustesCampoVivo.Comparar(false); }

  [MenuItem("Tools/Combate/Campo vivo/Comparar - campo vivo (Play)")]
  private static void Nuevo() { if (Application.isPlaying) AjustesCampoVivo.Comparar(true); }

  [MenuItem("Tools/Combate/Campo vivo/Usar ajustes del asset (Play)")]
  private static void Preferencias() { AjustesCampoVivo.Comparar(null); }

  public static void Renderizar()
  {
    try
    {
      var escena = EditorSceneManager.OpenScene("Assets/Scenes/ES-Campaña.unity");
      var admin = UnityEngine.Object.FindFirstObjectByType<AdministradorEscenas>(FindObjectsInactive.Include);
      var bm = UnityEngine.Object.FindFirstObjectByType<BattleManager>(FindObjectsInactive.Include);
      admin.EscenaCampaign.SetActive(false);
      admin.EscenaBatalla.SetActive(true);
      admin.escenaActual = 1;
      typeof(BattleManager).GetProperty("Instance").SetValue(null,bm);
      bm.lCasillasTotal = admin.EscenaBatalla.GetComponentsInChildren<Casilla>(true).ToList();
      bm.lUnidadesTotal.Clear();
      var cam = bm.goCamara.GetComponent<Camera>();
      cam.enabled = true;
      cam.aspect = 16f/9f;
      foreach (var canvas in escena.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Canvas>(true)))
        if (canvas.renderMode != RenderMode.WorldSpace) canvas.gameObject.SetActive(false);
      var prefabs = new[] { bm.prefabUnidadCaballero, bm.prefabUnidadExplorador, bm.prefabUnidadPurificadora, bm.prefabUnidadAcechador,
        AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UnidadEnemiga.prefab") };
      for (int i=0;i<6;i++)
      {
        var prefab = prefabs[i%prefabs.Length];
        if (prefab == null) continue;
        var go = UnityEngine.Object.Instantiate(prefab);
        var u = go.GetComponent<Unidad>();
        var tile = bm.lCasillasTotal.FirstOrDefault(c => c.lado==(i<3 ? 2 : 1) && c.posX==i%3+1 && c.posY==2);
        if (tile == null) tile = bm.lCasillasTotal[Mathf.Min(i*5,bm.lCasillasTotal.Count-1)];
        go.transform.position = tile.transform.position;
        go.transform.localScale *= .9f;
        u.CasillaPosicion = tile;
        u.HP_actual=100;
        bm.lUnidadesTotal.Add(u);
        foreach(var c in go.GetComponentsInChildren<Canvas>(true)) { c.worldCamera=cam; c.enabled=true; }
      }
      var campo = admin.EscenaBatalla.AddComponent<CampoVivoBatalla>();
      var zonas = new[] { EncounterZoneType.BosqueAngustiante,EncounterZoneType.PasoVientoHelado,EncounterZoneType.Nedukazal,EncounterZoneType.Subterraneo };
      var fondos = new[] { admin.fondosBosqueLamentos,admin.fondosPasoVientoHelado,admin.fondosNedukazal,admin.fondosSubterraneos };
      var nombres = new[] { "bosque","hielo","nedukazal","subterraneo" };
      Directory.CreateDirectory("output/campo-vivo/capturas");
      for(int i=0;i<zonas.Length;i++)
      {
        if(fondos[i] != null && fondos[i].Count>0)
        {
          admin.mrFondoBatalla.sharedMaterial=fondos[i][0].fondo;
          var frente = admin.mrFondoBatalla.transform.Find("FondoFrente");
          if(frente!=null) { var r=frente.GetComponent<MeshRenderer>(); r.sharedMaterial=fondos[i][0].fondoFrente; r.enabled=r.sharedMaterial!=null; }
        }
        AjustesCampoVivo.Comparar(false);
        campo.Avanzar(.02f);
        Capturar(cam,nombres[i]+"-antes");
        AjustesCampoVivo.Comparar(true);
        campo.Configurar(admin.mrFondoBatalla,zonas[i],i==3);
        for(int frame=0;frame<480;frame++) campo.Avanzar(1f/60f);
        Capturar(cam,nombres[i]+"-despues");
        CampoVivoBatalla.ReproducirImpacto(bm.lUnidadesTotal[0],bm.lUnidadesTotal[3],true,false,50);
        for(int frame=0;frame<5;frame++) campo.Avanzar(1f/60f);
        Capturar(cam,nombres[i]+"-impacto");
      }
      Comprobar(campo,bm,admin);
      AjustesCampoVivo.Comparar(null);
      Debug.Log("Campo vivo: render finalizado.");
    }
    catch(Exception ex) { Debug.LogException(ex); throw; }
  }

  private static void Capturar(Camera cam,string nombre)
  {
    if (Environment.GetCommandLineArgs().Contains("-campoSoloValidacion")) return;
    Canvas.ForceUpdateCanvases();
    var rt = new RenderTexture(1600,900,24);
    var anterior = cam.targetTexture;
    var activo = RenderTexture.active;
    cam.targetTexture=rt;
    // PPSv2 necesita renovar el historial tras cambiar de fondo/camara en Editor.
    cam.Render();
    cam.Render();
    cam.Render();
    RenderTexture.active=rt;
    var tex = new Texture2D(1600,900,TextureFormat.RGB24,false);
    tex.ReadPixels(new Rect(0,0,1600,900),0,0);
    tex.Apply();
    File.WriteAllBytes("output/campo-vivo/capturas/"+nombre+".png",tex.EncodeToPNG());
    cam.targetTexture=anterior;
    RenderTexture.active=activo;
    UnityEngine.Object.DestroyImmediate(tex);
    UnityEngine.Object.DestroyImmediate(rt);
  }

  private static T Campo<T>(object objeto,string nombre)
  { return (T)objeto.GetType().GetField(nombre,privado).GetValue(objeto); }

  private static void Comprobar(CampoVivoBatalla campo,BattleManager bm,AdministradorEscenas admin)
  {
    var resultados = new System.Collections.Generic.List<string>();
    Action<bool,string> verificar = (ok,nombre) => { if(!ok) throw new Exception("Campo vivo: " + nombre); resultados.Add("OK " + nombre); };
    var ajustes=AjustesCampoVivo.Actual;
    var configuracionOriginal=EditorJsonUtility.ToJson(ajustes);
    var unidad=bm.lUnidadesTotal[0];
    Vector3 posicionInicial=unidad.transform.position;
    var rng=UnityEngine.Random.state;
    float escalaTiempo=Time.timeScale;
    try
    {
      verificar(ajustes!=null && ajustes.activo,"Asset activado y shader cargado por Resources");
      verificar(ajustes.fondosQuemados.Length==3 && ajustes.fondosQuemados.All(t=>t!=null),"Tres fondos quemados reconocibles en build");
      foreach(EncounterZoneType z in Enum.GetValues(typeof(EncounterZoneType)))
      {
        campo.Configurar(admin.mrFondoBatalla,z,z==EncounterZoneType.Subterraneo);
        var posiciones=bm.lUnidadesTotal.Select(u=>u.transform.position).ToArray();
        var vidas=bm.lUnidadesTotal.Select(u=>u.HP_actual).ToArray();
        var ap=bm.lUnidadesTotal.Select(u=>u.ObtenerAPActual()).ToArray();
        var camPos=bm.goCamara.transform.position;
        var camRot=bm.goCamara.transform.rotation;
        var inicio=UnityEngine.Random.state;
        for(int f=0;f<360;f++) campo.Avanzar(1f/60f);
        var sistemas=Campo<GameObject>(campo,"raiz").GetComponentsInChildren<ParticleSystem>(true);
        verificar(sistemas.Length==5 && sistemas.Sum(ps=>ps.particleCount)>0,"Emision real en " + z);
        verificar(UnityEngine.Random.state.Equals(inicio),"RNG de combate intacto en " + z);
        verificar(bm.lUnidadesTotal.Select(u=>u.transform.position).SequenceEqual(posiciones)
          && bm.lUnidadesTotal.Select(u=>u.HP_actual).SequenceEqual(vidas)
          && bm.lUnidadesTotal.Select(u=>u.ObtenerAPActual()).SequenceEqual(ap)
          && bm.goCamara.transform.position==camPos && bm.goCamara.transform.rotation==camRot
          && Time.timeScale==escalaTiempo,"Unidades/AP/HP/camara/tiempo intactos en " + z);
        resultados.Add("   Particulas " + string.Join(", ",sistemas.Select(ps=>ps.name+"="+ps.particleCount)));
      }

      var psMotas=Campo<ParticleSystem>(campo,"motasLejanas");
      var muestra=new ParticleSystem.Particle[180];
      psMotas.GetParticles(muestra);
      Vector3 posicionParticula=muestra[0].position;
      float vidaParticula=muestra[0].remainingLifetime;
      float relojAntes=Campo<float>(campo,"reloj");
      // Misma bandera utilizada por los tooltips del tutorial, sin disparar su UI.
      typeof(BattleManager).GetField("pausaTooltipTutorialActiva",privado).SetValue(bm,true);
      for(int f=0;f<30;f++) campo.Avanzar(.016f);
      psMotas.GetParticles(muestra);
      verificar(Campo<float>(campo,"reloj")==relojAntes && muestra[0].position==posicionParticula
        && muestra[0].remainingLifetime==vidaParticula,"Pausa tutorial congela reloj y particulas");
      CampoVivoBatalla.ReproducirImpacto(unidad,bm.lUnidadesTotal[3],true,false,80);
      CampoVivoBatalla.ReproducirArranqueMelee(unidad,unidad.transform.position+Vector3.right);
      verificar(Campo<ParticleSystem>(campo,"polvo").particleCount==0,"Pausa tutorial impide impactos nuevos");
      typeof(BattleManager).GetField("pausaTooltipTutorialActiva",privado).SetValue(bm,false);
      campo.Avanzar(.016f);
      verificar(Campo<float>(campo,"reloj")>relojAntes,"Reanudacion del mismo reloj");

      ajustes.atmosfera=false;
      campo.Avanzar(.016f);
      verificar(psMotas.particleCount==0,"Interruptor de atmosfera limpia las motas");
      Action limpiar = () => { AjustesCampoVivo.Comparar(false); campo.Avanzar(.016f); AjustesCampoVivo.Comparar(true); campo.Avanzar(.016f); };
      limpiar();
      unidad.movimientoEnCurso=true;
      campo.Avanzar(.016f);
      verificar(Campo<ParticleSystem>(campo,"polvo").particleCount>0,"Dash levanta polvo al arrancar");
      var bocanadas = new ParticleSystem.Particle[4];
      Campo<ParticleSystem>(campo,"polvo").GetParticles(bocanadas);
      float tamanoDash = bocanadas[0].startSize3D.x;
      float duracionPolvoDash = bocanadas[0].startLifetime;
      unidad.transform.position+=Vector3.right*.25f;
      campo.Avanzar(.016f);
      int antesAterrizaje=Campo<ParticleSystem>(campo,"polvo").particleCount;
      unidad.movimientoEnCurso=false;
      campo.Avanzar(.016f);
      verificar(Campo<ParticleSystem>(campo,"polvo").particleCount>antesAterrizaje,"Dash levanta polvo al llegar");
      unidad.transform.position=posicionInicial;
      limpiar();
      unidad.unidadVoladora=true;
      CampoVivoBatalla.ReproducirArranqueMelee(unidad,unidad.transform.position+Vector3.right);
      unidad.movimientoEnCurso=true;
      campo.Avanzar(.016f);
      unidad.movimientoEnCurso=false;
      campo.Avanzar(.016f);
      verificar(Campo<ParticleSystem>(campo,"polvo").particleCount==0,"Voladores sin polvo de pisadas");
      unidad.unidadVoladora=false;

      limpiar();
      CampoVivoBatalla.ReproducirArranqueMelee(unidad,unidad.transform.position+Vector3.right);
      verificar(Campo<ParticleSystem>(campo,"polvo").GetParticles(bocanadas)==2
        && bocanadas[0].startSize3D.x>tamanoDash && bocanadas[0].startLifetime<duracionPolvoDash,
        "Arranque melee produce dos bocanadas mayores y mas breves que el dash");
      verificar(unidad.transform.position==posicionInicial && !unidad.movimientoEnCurso,
        "Polvo melee no altera posicion ni movimiento logico");

      var objetivo=bm.lUnidadesTotal[3];
      limpiar();
      CampoVivoBatalla.ReproducirImpacto(unidad,objetivo,true,false,80);
      verificar(Campo<ParticleSystem>(campo,"polvo").particleCount==0 && Campo<ParticleSystem>(campo,"fragmentos").particleCount>0,"Critico conserva esquirlas sin generar polvo ni humo");
      int hijos=campo.transform.childCount;
      for(int golpe=0;golpe<100;golpe++) CampoVivoBatalla.ReproducirImpacto(unidad,objetivo,true,true,100);
      verificar(campo.transform.childCount==hijos && Campo<ParticleSystem>(campo,"fragmentos").particleCount<=100,"Impactos masivos reutilizan pool acotado");
      for(int f=0;f<120;f++) campo.Avanzar(.016f);
      verificar(Campo<ParticleSystem>(campo,"polvo").particleCount==0 && Campo<ParticleSystem>(campo,"fragmentos").particleCount==0,"Efectos transitorios expiran");

      limpiar();
      var oculto=typeof(Unidad).GetField("estaEscondido",BindingFlags.Public|privado);
      var ocultoAntes=oculto.GetValue(objetivo);
      int ladoAntes=objetivo.CasillaPosicion.lado;
      objetivo.CasillaPosicion.lado=1;
      oculto.SetValue(objetivo,1);
      CampoVivoBatalla.ReproducirImpacto(unidad,objetivo,true,true,100);
      verificar(Campo<ParticleSystem>(campo,"polvo").particleCount==0 && Campo<ParticleSystem>(campo,"fragmentos").particleCount==0,"No se revelan enemigos ocultos");
      oculto.SetValue(objetivo,ocultoAntes);
      objetivo.CasillaPosicion.lado=ladoAntes;

      campo.Configurar(admin.mrFondoBatalla,EncounterZoneType.BosqueAngustiante,false);
      campo.Avanzar(.016f);
      campo.EstablecerNoche(false);
      var colorDia=Campo<PaletaCampoVivo>(campo,"paleta").aire;
      campo.EstablecerNoche(true);
      verificar(Campo<PaletaCampoVivo>(campo,"paleta").aire!=colorDia,"Noche adapta la atmosfera");
      campo.EstablecerNoche(false);
      verificar(Campo<PaletaCampoVivo>(campo,"paleta").aire==colorDia,"Volver a dia no acumula tintes");

      AjustesCampoVivo.Comparar(false); campo.Avanzar(.016f);
      verificar(!Campo<GameObject>(campo,"raiz").activeSelf && Campo<GameObject>(campo,"raiz").GetComponentsInChildren<ParticleSystem>(true).Sum(ps=>ps.particleCount)==0,"Desactivar elimina toda la capa y los rastros");
      AjustesCampoVivo.Comparar(true); campo.Avanzar(.016f);
      verificar(Campo<GameObject>(campo,"raiz").activeSelf,"Reactivar reutiliza los recursos");
      ajustes.atmosfera=true;
      var ambiente=admin.EscenaBatalla.AddComponent<BattleAmbientLife>();
      ambiente.Configurar(admin.mrFondoBatalla,EncounterZoneType.Subterraneo,true);
      campo.Avanzar(.016f);
      typeof(BattleAmbientLife).GetMethod("Update",privado).Invoke(ambiente,null);
      verificar(!Campo<GameObject>(ambiente,"goAmbientRoot").activeSelf && admin.EscenaBatalla.GetComponents<CampoVivoBatalla>().Length==1,"Integracion sustituye motas previas sin duplicar director");
      AjustesCampoVivo.Comparar(false); campo.Avanzar(.016f);
      typeof(BattleAmbientLife).GetMethod("Update",privado).Invoke(ambiente,null);
      verificar(Campo<GameObject>(ambiente,"goAmbientRoot").activeSelf,"Desactivar recupera las particulas anteriores");

      AjustesCampoVivo.Comparar(true);
      campo.Avanzar(.016f);
      var copiaRaiz=Campo<GameObject>(campo,"raiz");
      // Fuera de Play Mode Unity no despacha OnDestroy a este MonoBehaviour.
      typeof(CampoVivoBatalla).GetMethod("OnDestroy",privado).Invoke(campo,null);
      UnityEngine.Object.DestroyImmediate(campo);
      verificar(copiaRaiz==null,"El metodo de destruccion libera la raiz y sus recursos");
      resultados.Add("NOTA: prueba de componentes reales en Editor; no recorre el tutorial completo ni una partida en Play Mode.");
      File.WriteAllLines("output/campo-vivo/validacion.txt",resultados);
      Debug.Log("Campo vivo: " + resultados.Count + " comprobaciones y diagnosticos guardados.");
    }
    finally
    {
      EditorJsonUtility.FromJsonOverwrite(configuracionOriginal,ajustes);
      UnityEngine.Random.state=rng;
      unidad.transform.position=posicionInicial;
      unidad.movimientoEnCurso=false;
      unidad.unidadVoladora=false;
      typeof(BattleManager).GetField("pausaTooltipTutorialActiva",privado).SetValue(bm,false);
      AjustesCampoVivo.Comparar(null);
    }
  }

  public static void Inspeccionar()
  {
    EditorSceneManager.OpenScene("Assets/Scenes/ES-Campaña.unity");
    var admin = UnityEngine.Object.FindFirstObjectByType<AdministradorEscenas>(FindObjectsInactive.Include);
    var bm = UnityEngine.Object.FindFirstObjectByType<BattleManager>(FindObjectsInactive.Include);
    Directory.CreateDirectory("output/campo-vivo");
    using (var w = new StreamWriter("output/campo-vivo/escena.txt"))
    {
      w.WriteLine("Admin: " + admin + " Battle: " + bm);
      w.WriteLine("Root: " + admin.EscenaBatalla + " camera: " + bm.goCamara);
      w.WriteLine("Fondo: " + admin.mrFondoBatalla.transform.position + " bounds: " + admin.mrFondoBatalla.bounds);
      foreach (var c in admin.EscenaBatalla.GetComponentsInChildren<Camera>(true))
        w.WriteLine("CAM " + c.name + " " + c.transform.position + " " + c.transform.eulerAngles + " fov=" + c.fieldOfView + " mask=" + c.cullingMask);
      foreach (var c in admin.EscenaBatalla.GetComponentsInChildren<Casilla>(true).Take(12))
        w.WriteLine("TILE " + c.name + " " + c.transform.position + " scale=" + c.transform.lossyScale);
      foreach (var c in admin.EscenaBatalla.GetComponentsInChildren<Renderer>(true).Where(x => x is MeshRenderer).Take(30))
        w.WriteLine("RENDER " + c.name + " " + c.bounds + " order=" + c.sortingOrder + " material=" + c.sharedMaterial);
      foreach (var u in new[] { bm.prefabUnidadCaballero, bm.prefabUnidadEnemiga })
      {
        if (u == null) continue;
        var unit = u.GetComponent<Unidad>();
        w.WriteLine("UNIT " + AssetDatabase.GetAssetPath(u) + " scale=" + u.transform.localScale + " image=" + unit.uImage);
        foreach (var c in u.GetComponentsInChildren<Canvas>(true))
          w.WriteLine("CANVAS " + c.name + " mode=" + c.renderMode + " order=" + c.sortingOrder + " scale=" + c.transform.lossyScale);
      }
    }
    Debug.Log("Campo vivo: inspeccion guardada.");
  }
}
