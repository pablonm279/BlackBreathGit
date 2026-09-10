$ErrorActionPreference = 'Stop'
# Ejecuta el motor real con dobles de Unity; no reemplaza una prueba en Play Mode.
$stub = @'
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
namespace UnityEngine {
 public class DisallowMultipleComponent:Attribute {}
 public class Object { public int GetInstanceID()=>17; }
 public class GameObject:Object {
  public Dictionary<Type,Component> Components=new Dictionary<Type,Component>();
  public T AddComponent<T>() where T:Component,new(){var c=new T();c.gameObject=this;Components[typeof(T)]=c;return c;}
  public T GetComponent<T>() where T:class {Component c;return Components.TryGetValue(typeof(T),out c)?c as T:null;}
 }
 public class Component:Object {public GameObject gameObject; public T GetComponent<T>() where T:class=>gameObject.GetComponent<T>();}
 public class Sprite:Object {}
 public struct Vector2 {
  public float x,y;public Vector2(float x,float y){this.x=x;this.y=y;}
  public static Vector2 Scale(Vector2 a,Vector2 b)=>new Vector2(a.x*b.x,a.y*b.y);
  public static Vector2 operator +(Vector2 a,Vector2 b)=>new Vector2(a.x+b.x,a.y+b.y);
 }
 public struct Vector3 {public float x,y,z;public Vector3(float x,float y,float z=0){this.x=x;this.y=y;this.z=z;}}
 public struct Rect {public Vector2 min,size;public float width=>size.x;}
 public class RectTransform {public Rect rect=new Rect{min=new Vector2(-50,-50),size=new Vector2(100,100)};}
 public struct UIVertex {public Vector3 position;}
 public static class Time {public static float deltaTime=0.1f;}
 public static class Resources {public static PerfilAnimacionIlustrada[] Profiles=new PerfilAnimacionIlustrada[0];public static T[] LoadAll<T>(string s)=>Profiles as T[];}
 public static class Mathf {
  public const float PI=(float)Math.PI;
  public static int Abs(int a)=>Math.Abs(a);
  public static float Max(float a,float b)=>Math.Max(a,b);
  public static float Min(float a,float b)=>Math.Min(a,b);
  public static float Sin(float a)=>(float)Math.Sin(a);
  public static float Exp(float a)=>(float)Math.Exp(a);
  public static float Clamp01(float a)=>Max(0,Min(1,a));
  public static float SmoothStep(float a,float b,float t){t=Clamp01(t);return a+(b-a)*t*t*(3-2*t);}
 }
 public struct AnimatorStateInfo {public string Name;public float normalizedTime;public bool IsName(string s)=>s==Name;}
 public class Animator:Component {public bool isActiveAndEnabled=true;public object runtimeAnimatorController=new object();public AnimatorStateInfo State;public AnimatorStateInfo GetCurrentAnimatorStateInfo(int i)=>State;}
}
namespace UnityEngine.UI {
 public class Image:UnityEngine.Component {public UnityEngine.Sprite sprite;public bool isActiveAndEnabled=true;public UnityEngine.RectTransform rectTransform=new UnityEngine.RectTransform();public void SetVerticesDirty(){}}
 public class BaseMeshEffect:UnityEngine.Component {protected Image graphic=>GetComponent<Image>();protected virtual void OnEnable(){}protected virtual void OnDisable(){}protected bool IsActive()=>true;public virtual void ModifyMesh(VertexHelper v){}}
 public class VertexHelper {public List<UnityEngine.UIVertex> v=new List<UnityEngine.UIVertex>();public int currentVertCount=>v.Count;public void PopulateUIVertex(ref UnityEngine.UIVertex x,int i){x=v[i];}public void SetUIVertex(UnityEngine.UIVertex x,int i){v[i]=x;}}
}
public class Unidad:UnityEngine.Component {public bool esInmobil,movimientoEnCurso;public float HP_actual=10;public int estado_congelado,estado_aturdido;public UnityEngine.UI.Image uImage;}
public class IAUnidad:UnityEngine.Component {}
public class UnidadPoseController:UnityEngine.Component {
 public enum TipoPoseActual{Idle,Mover,Atacar,Habilidad,RecibirDanio}
 public UnityEngine.UI.Image targetImage;public UnityEngine.Sprite poseMover,poseHabilidad,poseAtacar,poseIdle;
 public UnityEngine.Sprite ObtenerPoseReposoActual()=>poseIdle;
}
public class PerfilAnimacionIlustrada {
 public UnityEngine.Sprite preparacion,reposo,anticipacion,impacto,origenMover,origenIdle,origenAtacar;
 public UnityEngine.Vector2 apoyo;public float desplazamientoHorizontal,respiracion=.006f,escalaHorizontal=1;
 public bool EstaCompleto=>reposo!=null&&anticipacion!=null&&impacto!=null;
 public bool Coincide(UnidadPoseController c)=>c!=null&&c.poseMover==origenMover&&c.poseAtacar==origenAtacar;
}
public static class MeleeTimingUtility {public static int CalcularPreImpactoMs(UnidadPoseController c)=>350;}
public static class UnidadAmbientMotionFx {public static float Factor=1;public static float ObtenerFactorRespiracion(Unidad u)=>Factor;}
public static class PruebaMovimientoIA {
 static int n;
 static void Check(bool b,string s){if(!b)throw new Exception(s);n++;}
 static void Tick(UnidadAnimacionIlustrada m)=>typeof(UnidadAnimacionIlustrada).GetMethod("Update",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(m,null);
 static float Clock(UnidadAnimacionIlustrada m)=>(float)typeof(UnidadAnimacionIlustrada).GetField("tiempoRespiracion",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(m);
 static UnityEngine.UI.VertexHelper Mesh(){var v=new UnityEngine.UI.VertexHelper();v.v.Add(new UnityEngine.UIVertex{position=new UnityEngine.Vector3(-30,-20)});v.v.Add(new UnityEngine.UIVertex{position=new UnityEngine.Vector3(30,80)});return v;}
 public static string Run(){
  var go=new UnityEngine.GameObject();var u=go.AddComponent<Unidad>();go.AddComponent<IAUnidad>();
  var c=go.AddComponent<UnidadPoseController>();var img=new UnityEngine.GameObject().AddComponent<UnityEngine.UI.Image>();u.uImage=c.targetImage=img;img.sprite=c.poseIdle=new UnityEngine.Sprite();
  var m=UnidadAnimacionIlustrada.IntentarCrear(c);Check(m!=null&&m.Disponible&&m.UsaSpritesOriginalesIA,"IA sin perfil incluida");Check(m.PosePreparacion==null,"Sin preparacion inventada");
  m.Reproducir(UnidadPoseController.TipoPoseActual.Idle,img.sprite,true);Tick(m);
  var v=Mesh();m.ModifyMesh(v);Check(v.v[0].position.y==-20&&v.v[0].position.x==-30,"Apoyo fijo con preserveAspect");Check(v.v[1].position.y!=80,"Respira la imagen principal");
  var original=img.sprite;
  foreach(UnidadPoseController.TipoPoseActual p in Enum.GetValues(typeof(UnidadPoseController.TipoPoseActual))){m.Reproducir(p,original,true);Tick(m);Check(img.sprite==original,"Sprite preservado: "+p);}
  m.Reproducir(UnidadPoseController.TipoPoseActual.Habilidad,original,true);v=Mesh();m.ModifyMesh(v);Check(v.v[1].position.x==30&&v.v[1].position.y==80,"Habilidad sin deformacion");
  m.Reproducir(UnidadPoseController.TipoPoseActual.Mover,original,true);Tick(m);v=Mesh();m.ModifyMesh(v);Check(v.v[1].position.x<30,"Inclinacion dash");
  m.Reproducir(UnidadPoseController.TipoPoseActual.Idle,original,false);v=Mesh();m.ModifyMesh(v);Check(v.v[1].position.x<30,"Asentamiento al llegar");
  float a=Clock(m);UnidadAmbientMotionFx.Factor=.8f;Tick(m);Check(Math.Abs(Clock(m)-a-.08f)<.00001,"Respiracion fuera de turno al 80%");
  a=Clock(m);UnityEngine.Time.deltaTime=0;Tick(m);Check(Clock(m)==a,"Pausa");UnityEngine.Time.deltaTime=.1f;
  u.estado_congelado=1;Tick(m);Check(Clock(m)==a,"Congelacion");u.estado_congelado=0;u.estado_aturdido=1;Tick(m);Check(Clock(m)==a,"Aturdimiento");u.estado_aturdido=0;
  u.esInmobil=true;v=Mesh();m.ModifyMesh(v);Check(!m.Disponible&&v.v[1].position.y==80,"Inmovil excluida");u.esInmobil=false;
  u.esInmobil=true;Check(UnidadAnimacionIlustrada.IntentarCrear(c)==null,"No incorporar controlador de IA inmovil");u.esInmobil=false;
  u.HP_actual=0;Tick(m);v=Mesh();m.ModifyMesh(v);Check(Clock(m)==a&&v.v[1].position.y==80,"Muerte");u.HP_actual=10;
  img.sprite=new UnityEngine.Sprite();Tick(m);v=Mesh();m.ModifyMesh(v);Check(v.v[1].position.y==80,"Sustitucion externa respetada");
  var legacy=new UnityEngine.GameObject();var lu=legacy.AddComponent<Unidad>();legacy.AddComponent<IAUnidad>();var anim=legacy.AddComponent<UnityEngine.Animator>();lu.uImage=new UnityEngine.GameObject().AddComponent<UnityEngine.UI.Image>();lu.uImage.sprite=original;
  UnidadAnimacionIlustrada.InicializarIASinControlador(lu);var lm=lu.uImage.GetComponent<UnidadAnimacionIlustrada>();Check(lm!=null&&legacy.GetComponent<UnidadPoseController>()==null,"Legacy conserva Animator sin agregar controlador");
  anim.State=new UnityEngine.AnimatorStateInfo{Name="Animacion_idle"};Tick(lm);lu.movimientoEnCurso=true;Tick(lm);v=Mesh();lm.ModifyMesh(v);Check(v.v[1].position.x<30,"Legacy dash");lu.movimientoEnCurso=false;
  anim.State=new UnityEngine.AnimatorStateInfo{Name="Animacion_Ataque",normalizedTime=.1f};Tick(lm);v=Mesh();lm.ModifyMesh(v);Check(v.v[1].position.x!=30&&lu.uImage.sprite==original,"Legacy ataque con sprite original");
  anim.State=new UnityEngine.AnimatorStateInfo{Name="Animacion_RecibeDaño"};Tick(lm);v=Mesh();lm.ModifyMesh(v);Check(v.v[1].position.x==30,"Legacy dano respetado");
  var pg=new UnityEngine.GameObject();pg.AddComponent<Unidad>();var pc=pg.AddComponent<UnidadPoseController>();pc.targetImage=new UnityEngine.GameObject().AddComponent<UnityEngine.UI.Image>();Check(UnidadAnimacionIlustrada.IntentarCrear(pc)==null,"Jugador sin perfil fuera del alcance");
  var profile=new PerfilAnimacionIlustrada{origenMover=new UnityEngine.Sprite(),origenAtacar=new UnityEngine.Sprite(),origenIdle=original,reposo=new UnityEngine.Sprite(),anticipacion=new UnityEngine.Sprite(),impacto=new UnityEngine.Sprite()};c.poseMover=profile.origenMover;c.poseAtacar=profile.origenAtacar;
  UnityEngine.Resources.Profiles=new[]{profile};typeof(UnidadAnimacionIlustrada).GetField("perfiles",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,null);
  var same=UnidadAnimacionIlustrada.IntentarCrear(c);Check(same==m&&!m.UsaSpritesOriginalesIA,"Perfil Driada tiene prioridad, sin duplicar motor");img.sprite=original;m.Reproducir(UnidadPoseController.TipoPoseActual.Idle,original,true);Check(img.sprite==profile.reposo,"Reposo aprobado conservado");
  return n+" comprobaciones del motor real OK (dobles de Unity; no Play Mode).";
 }
}
'@
Add-Type -TypeDefinition ($stub + "`n" + (Get-Content 'Assets/Scripts/Visual/UnidadAnimacionIlustrada.cs' -Raw).Replace('using UnityEngine;', '').Replace('using UnityEngine.UI;', ''))
[PruebaMovimientoIA]::Run()
