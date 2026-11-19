using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
  string dir="Assets/Resources/Imagenes/Fondos/Bosque Lamentos/";
  for(int i=1;i<=3;i++) {
   AssetDatabase.ImportAsset(dir+"Fondo"+i+"BosqueArdiente/fondobosque"+i+"_frente.png",ImportAssetOptions.ForceUpdate);
   AssetDatabase.ImportAsset(dir+"Fondo"+i+"BosqueArdiente/"+(i==1?"bosquefondobatalla.png":"bosquefondobatalla"+i+".png"),ImportAssetOptions.ForceUpdate);
   AssetDatabase.ImportAsset(dir+"Fondo"+i+"BosqueArdiente/FondoBosqueLamentos"+i+"Frente.mat",ImportAssetOptions.ForceUpdate);
  }
  var source=UnityEngine.Object.FindFirstObjectByType<AdministradorEscenas>(FindObjectsInactive.Include);
  var fronts=new Material[3];
  for(int i=0;i<3;i++) fronts[i]=AssetDatabase.LoadAssetAtPath<Material>(dir+"Fondo"+(i+1)+"BosqueArdiente/FondoBosqueLamentos"+(i+1)+"Frente.mat");
  for(int i=0;i<3;i++) {
   if(source.fondosBosqueLamentos[i].fondoFrente!=fronts[i])throw new Exception("Vinculo de frente incorrecto");
   var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(dir+"Fondo"+(i+1)+"BosqueArdiente/fondobosque"+(i+1)+"_frente.png");
   if(fronts[i].mainTexture!=texture)throw new Exception("Frente no usa su textura propia");
  }
  var report=new List<string>();
  for(int i=0;i<3;i++) {
   var tex=(Texture2D)fronts[i].mainTexture;
   report.Add("Pareja "+(i+1)+": "+source.fondosBosqueLamentos[i].fondo.name+" + "+fronts[i].name+"; textura "+tex.width+"x"+tex.height+"; cola="+fronts[i].renderQueue+"; alpha Fade="+fronts[i].IsKeywordEnabled("_ALPHABLEND_ON"));
  }
  Directory.CreateDirectory("output/bosque-estilo-aprobado/capturas");
  if(Application.isPlaying) {
   var current=source.mrFondoBatalla.sharedMaterial;
   int selected=Array.FindIndex(source.fondosBosqueLamentos.ToArray(), f=> f.fondo!=null && current!=null && f.fondo.mainTexture==current.mainTexture);
   if(selected>=0) {
    var renderer=source.mrFondoBatalla.transform.Find("FondoFrente").GetComponent<MeshRenderer>();
    result.RegisterObjectModification(renderer);
    renderer.sharedMaterial=fronts[selected];renderer.enabled=true;
   }
   report.Add("Vinculos aplicados en Play. Captura aislada pospuesta para no interferir con la partida.");
   File.WriteAllLines("output/bosque-estilo-aprobado/VALIDACION.txt",report);result.Log(string.Join("\n",report));return;
  }
  var randomState=UnityEngine.Random.state;
  var scene=EditorSceneManager.NewPreviewScene();
  var transientMaterials=new List<Material>();
  try {
   string renderPath=AnimationUtility.CalculateTransformPath(source.mrFondoBatalla.transform,source.EscenaBatalla.transform);
   var root=UnityEngine.Object.Instantiate(source.EscenaBatalla);
   root.hideFlags=HideFlags.HideAndDontSave;SceneManager.MoveGameObjectToScene(root,scene);root.SetActive(true);
   var bm=root.GetComponentInChildren<BattleManager>(true);
   var cam=bm.goCamara.GetComponent<Camera>();cam.scene=scene;cam.enabled=false;cam.aspect=16f/9f;
   foreach(var c in cam.GetComponents<Behaviour>()) if(c.GetType().Name=="PostProcessLayer") c.enabled=false;
   foreach(var c in root.GetComponentsInChildren<Canvas>(true)) if(c.renderMode!=RenderMode.WorldSpace) c.enabled=false;
   var background=root.transform.Find(renderPath).GetComponent<MeshRenderer>();
   var front=background.transform.Find("FondoFrente").GetComponent<MeshRenderer>();
   front.gameObject.SetActive(true);
   front.gameObject.layer=31;cam.cullingMask |= 1<<31;
   var temp=new GameObject("Prueba aislada de asociaciones");SceneManager.MoveGameObjectToScene(temp,scene);
   var admin=temp.AddComponent<AdministradorEscenas>();admin.mrFondoBatalla=background;
   var tempSerialized=new SerializedObject(admin);tempSerialized.FindProperty("mrFondoFrente").objectReferenceValue=front;tempSerialized.ApplyModifiedPropertiesWithoutUndo();
   var tiles=root.GetComponentsInChildren<Casilla>(true);
   var prefabs=new[]{bm.prefabUnidadCaballero,bm.prefabUnidadExplorador,bm.prefabUnidadPurificadora,bm.prefabUnidadCaballero};
   for(int i=0;i<4;i++) {
    var tile=tiles.FirstOrDefault(t=>t.lado==(i<2?2:1)&&t.posX==i%2+1&&t.posY==i%2+1);
    if(tile==null||prefabs[i]==null)continue;
    var unit=UnityEngine.Object.Instantiate(prefabs[i],root.transform);unit.transform.position=tile.transform.position;unit.transform.localScale*=.9f;
    foreach(var c in unit.GetComponentsInChildren<Canvas>(true)){c.enabled=true;c.worldCamera=cam;c.transform.rotation=cam.transform.rotation;}
   }
   Canvas.ForceUpdateCanvases();
   Vector3 pos=cam.transform.position;Quaternion rot=cam.transform.rotation;float fov=cam.fieldOfView;
   string[] views={"normal","rueda-cerca","habilidad-izquierda","habilidad-derecha","rueda-lejos"};
   for(int i=0;i<3;i++) {
    temp.SendMessage("AplicarFondoAleatorio",new List<FondoBatalla>{source.fondosBosqueLamentos[i]},SendMessageOptions.RequireReceiver);
    bool applied=background.sharedMaterial!=null&&background.sharedMaterial.mainTexture==source.fondosBosqueLamentos[i].fondo.mainTexture;
    transientMaterials.Add(background.sharedMaterial);
    if(!applied||!front.enabled||front.sharedMaterial!=fronts[i]) throw new Exception("No se activo el frente "+(i+1));
    if(front.sharedMaterial.renderQueue!=3999)throw new Exception("Cola transparente incorrecta");
    report.Add("Seleccion real AplicarFondoAleatorio "+(i+1)+": OK, frente activo y material correcto.");
    for(int j=0;j<5;j++) {
     Vector3 offset=Vector3.zero;Quaternion tilt=Quaternion.identity;float vf=fov;
     if(j==1){offset=rot*Vector3.forward*.38f+Vector3.down*.08f;tilt=Quaternion.Euler(-1.6f,0,0);}
     if(j==2||j==3){float s=j==2?-1:1;offset=rot*(Vector3.forward*.61f+Vector3.right*s*.36f)+Vector3.down*.2f;tilt=Quaternion.Euler(-1.6f,s*4,s*2.4f);vf=fov-8;}
     if(j==4){offset=rot*Vector3.forward*(-.38f*.18f)+Vector3.up*(.08f*.18f);tilt=Quaternion.Euler(1.6f*.18f,0,0);}
     cam.transform.SetPositionAndRotation(pos+offset,rot*tilt);cam.fieldOfView=vf;
     var rt=new RenderTexture(1280,720,24,RenderTextureFormat.ARGB32);
     var pixels=new Texture2D(1280,720,TextureFormat.RGBA32,false);
     var previous=RenderTexture.active;int cull=cam.cullingMask;var flags=cam.clearFlags;var color=cam.backgroundColor;bool hdr=cam.allowHDR;
     try {
      cam.targetTexture=rt;cam.Render();cam.Render();
      RenderTexture.active=rt;pixels.ReadPixels(new Rect(0,0,1280,720),0,0);pixels.Apply();
      File.WriteAllBytes("output/bosque-estilo-aprobado/capturas/fondo"+(i+1)+"-"+views[j]+".png",pixels.EncodeToPNG());
      cam.cullingMask=1<<31;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Color.clear;cam.allowHDR=false;cam.Render();
      RenderTexture.active=rt;pixels.ReadPixels(new Rect(0,0,1280,720),0,0);pixels.Apply();
      var colors=pixels.GetPixels32();int visible=0,center=0;
      for(int y=0;y<720;y++)for(int x=0;x<1280;x++) if(colors[y*1280+x].a>16){visible++;if(x>=320&&x<960&&y>=144&&y<612)center++;}
      report.Add("Fondo "+(i+1)+" / "+views[j]+": frente visible="+(visible*100f/colors.Length).ToString("F2")+"%; pixeles en centro protegido="+center);
      if(j==0&&visible==0)throw new Exception("Frente invisible en camara base "+(i+1));
      if(center>0)throw new Exception("Frente invade el centro "+(i+1)+" "+views[j]);
     }finally{cam.cullingMask=cull;cam.clearFlags=flags;cam.backgroundColor=color;cam.allowHDR=hdr;cam.targetTexture=null;RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(pixels);rt.Release();UnityEngine.Object.DestroyImmediate(rt);}
    }
   }
   // Un frente vacio debe seguir desactivandose para otras zonas y tutorial.
   var forest=source.fondosPasoVientoHelado[0];
   temp.SendMessage("AplicarFondoAleatorio",new List<FondoBatalla>{forest},SendMessageOptions.RequireReceiver);
   bool other=background.sharedMaterial.mainTexture==forest.fondo.mainTexture;
   transientMaterials.Add(background.sharedMaterial);
   if(!other||front.sharedMaterial!=forest.fondoFrente||front.enabled!=(forest.fondoFrente!=null))throw new Exception("Cambio de Paso Viento Helado alterado");
   temp.SendMessage("AplicarFondoAleatorio",new List<FondoBatalla>{new FondoBatalla{fondo=forest.fondo,fondoFrente=null}},SendMessageOptions.RequireReceiver);
   bool empty=background.sharedMaterial.mainTexture==forest.fondo.mainTexture;
   transientMaterials.Add(background.sharedMaterial);
   if(!empty||front.enabled)throw new Exception("No se desactiva frente ausente");
   report.Add("Paso Viento Helado y frente nulo: comportamiento de seleccion conservado.");
  } finally {
   EditorSceneManager.ClosePreviewScene(scene);
   foreach(var m in transientMaterials.Distinct())if(m!=null&&string.IsNullOrEmpty(AssetDatabase.GetAssetPath(m)))UnityEngine.Object.DestroyImmediate(m);
   UnityEngine.Random.state=randomState;
  }
  report.Add("Capturas en escena temporal sin postprocesado. Tutorial completo no ejecutado; se conserva la seleccion de Paso Viento Helado y el apagado para fondos sin frente. Sin cambios en scripts, casillas, camara o secuencias. La seleccion de los tres fondos de bosque usados por el tutorial pasa la prueba; no se recorre el tutorial entero.");
  File.WriteAllLines("output/bosque-estilo-aprobado/VALIDACION.txt",report);result.Log("Validacion final: tres parejas, quince encuadres, centro despejado y Paso/frente nulo OK. Informe en output/bosque-estilo-aprobado/VALIDACION.txt.");
 }
}


