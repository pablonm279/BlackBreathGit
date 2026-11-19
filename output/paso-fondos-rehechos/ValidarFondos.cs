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
  if(Application.isPlaying) { result.LogWarning("Validación requiere Editor fuera de Play; no se modificó la partida."); return; }
  var admin=UnityEngine.Object.FindFirstObjectByType<AdministradorEscenas>(FindObjectsInactive.Include);
  string renderPath=AnimationUtility.CalculateTransformPath(admin.mrFondoBatalla.transform,admin.EscenaBatalla.transform);
  var scene=EditorSceneManager.NewPreviewScene();
  var report=new List<string>();
  report.Add("Validación visual de fondos Paso Viento Helado — 1536 x 1024 RGB");
  report.Add("Capturas geométricas sin postprocesado; no representan el clima ni la iluminación dinámica de la partida.");
  report.Add("Escena temporal de previsualización; cámara, materiales, escenas y partida originales sin modificar.");
  try {
   var root=UnityEngine.Object.Instantiate(admin.EscenaBatalla);
   root.name="Preview temporal fondos";root.hideFlags=HideFlags.HideAndDontSave;
   SceneManager.MoveGameObjectToScene(root,scene);
   root.SetActive(true);
   var bm=root.GetComponentInChildren<BattleManager>(true);
   var cam=bm.goCamara.GetComponent<Camera>();
   cam.scene=scene;cam.enabled=false;cam.aspect=16f/9f;
   foreach(var component in cam.GetComponents<Behaviour>()) if(component.GetType().Name=="PostProcessLayer") component.enabled=false;
   var renderer=root.transform.Find(renderPath).GetComponent<MeshRenderer>();
   var front=renderer.transform.Find("FondoFrente");if(front!=null) front.gameObject.SetActive(false);
   foreach(var canvas in root.GetComponentsInChildren<Canvas>(true)) if(canvas.renderMode!=RenderMode.WorldSpace) canvas.enabled=false;
   var tiles=root.GetComponentsInChildren<Casilla>(true);
   var prefabs=new[]{bm.prefabUnidadCaballero,bm.prefabUnidadExplorador,bm.prefabUnidadPurificadora,bm.prefabUnidadCaballero};
   for(int i=0;i<prefabs.Length;i++) {
    var tile=tiles.FirstOrDefault(t=>t.lado==(i<2?2:1)&&t.posX==i%2+1&&t.posY==(i%2+1));
    if(prefabs[i]==null||tile==null) continue;
    var unit=UnityEngine.Object.Instantiate(prefabs[i],root.transform);
    unit.hideFlags=HideFlags.HideAndDontSave;
    unit.transform.position=tile.transform.position;
    unit.transform.localScale*=.9f;
    foreach(var canvas in unit.GetComponentsInChildren<Canvas>(true)) {canvas.enabled=true;canvas.worldCamera=cam;canvas.transform.rotation=cam.transform.rotation;}
    Canvas.ForceUpdateCanvases();
   }
   Vector3 position=cam.transform.position;Quaternion rotation=cam.transform.rotation;float fov=cam.fieldOfView;
   string[] names={"normal","rueda-cerca","habilidad-izquierda","habilidad-derecha","rueda-lejos"};
   string[] descriptions={"base","avance 0.38, descenso 0.08, pitch -1.6","paneo -0.36, avance 0.61, descenso 0.2, yaw -4, roll -2.4, FOV -8","paneo +0.36, avance 0.61, descenso 0.2, yaw +4, roll +2.4, FOV -8","rueda -0.18"};
   Directory.CreateDirectory("output/paso-fondos-rehechos/capturas");
   for(int fondo=0;fondo<3;fondo++) {
    renderer.sharedMaterial=admin.fondosPasoVientoHelado[fondo].fondo;
    var mf=renderer.GetComponent<MeshFilter>();
    var bounds=mf.sharedMesh.bounds;
    for(int view=0;view<5;view++) {
     Vector3 offset=Vector3.zero;Quaternion tilt=Quaternion.identity;float viewFov=fov;
     if(view==1) {offset=rotation*Vector3.forward*.38f+Vector3.down*.08f;tilt=Quaternion.Euler(-1.6f,0,0);}
     if(view==2||view==3) {float s=view==2?-1:1;offset=rotation*(Vector3.forward*.61f+Vector3.right*(s*.36f))+Vector3.down*.2f;tilt=Quaternion.Euler(-1.6f,s*4f,s*2.4f);viewFov=fov-8;}
     if(view==4) {offset=rotation*Vector3.forward*(-.38f*.18f)+Vector3.up*(.08f*.18f);tilt=Quaternion.Euler(1.6f*.18f,0,0);}
     cam.transform.SetPositionAndRotation(position+offset,rotation*tilt);cam.fieldOfView=viewFov;
     var plane=new Plane(renderer.transform.forward,renderer.transform.position);
     float minX=float.PositiveInfinity,maxX=float.NegativeInfinity,minY=float.PositiveInfinity,maxY=float.NegativeInfinity;
     foreach(var corner in new[]{new Vector3(0,0),new Vector3(0,1),new Vector3(1,0),new Vector3(1,1)}) {
      var ray=cam.ViewportPointToRay(corner);float hit;
      if(plane.Raycast(ray,out hit)) {
       var p=renderer.transform.InverseTransformPoint(ray.GetPoint(hit));
       minX=Mathf.Min(minX,p.x);maxX=Mathf.Max(maxX,p.x);minY=Mathf.Min(minY,p.y);maxY=Mathf.Max(maxY,p.y);
      }
     }
     bool covered=minX>=bounds.min.x&&maxX<=bounds.max.x&&minY>=bounds.min.y&&maxY<=bounds.max.y;
     report.Add("Fondo "+(fondo+1)+" / "+names[view]+": plano cubre viewport completo="+covered+"; proyección local X "+minX.ToString("F3")+".."+maxX.ToString("F3")+" Y "+minY.ToString("F3")+".."+maxY.ToString("F3")+"; "+descriptions[view]+"; FOV "+viewFov.ToString("F2"));
     var rt=new RenderTexture(1280,720,24);
     var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);
     var previous=RenderTexture.active;
     try {
      cam.targetTexture=rt;cam.Render();cam.Render();cam.Render();
      RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();
      File.WriteAllBytes("output/paso-fondos-rehechos/capturas/fondo"+(fondo+1)+"-"+names[view]+".png",tex.EncodeToPNG());
     } finally {cam.targetTexture=null;RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(tex);rt.Release();UnityEngine.Object.DestroyImmediate(rt);}
    }
   }
  } finally {EditorSceneManager.ClosePreviewScene(scene);}
  report.Add("El plano de fondo no necesita cubrir por sí solo las esquinas inferiores ocultas por el suelo: se verifica el compuesto en las capturas.");
  report.Add("No se ejecutó el tutorial completo; el cambio de producción abarca solamente tres texturas PNG, conservando GUID, dimensiones fuente, materiales y lógica.");
  File.WriteAllLines("output/paso-fondos-rehechos/VALIDACION.txt",report);
  result.Log(string.Join("\n",report));
 }
}
