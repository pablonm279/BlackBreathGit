using System;
using System.Collections;
using System.Reflection;

// Dobles minimos para ejecutar el controlador real fuera del Editor.
// Verifica seleccion/restauracion de sprites; no sustituye Play Mode.
namespace UnityEngine {
 public class HeaderAttribute : Attribute { public HeaderAttribute(string s) {} }
 public class RangeAttribute : Attribute { public RangeAttribute(float a,float b) {} }
 public class MinAttribute : Attribute { public MinAttribute(float a) {} }
 public class Sprite {}
 public class Coroutine { public IEnumerator Routine; }
 public class WaitForSeconds { public WaitForSeconds(float s) {} }
 public static class Mathf { public static int Max(int a,int b) { return Math.Max(a,b); } public static float Max(float a,float b) { return Math.Max(a,b); } }
 public class MonoBehaviour {
  public Coroutine Pending;
  public T GetComponent<T>() where T:class { return null; }
  public Coroutine StartCoroutine(IEnumerator r) { Pending=new Coroutine {Routine=r}; return Pending; }
  public void StopCoroutine(Coroutine c) { if(Pending==c) Pending=null; }
 }
}
namespace UnityEngine.UI { public class Image { public UnityEngine.Sprite sprite; } }
public class Unidad { public UnityEngine.UI.Image uImage; }
public class BattleManager { public static BattleManager Instance; public Unidad unidadActiva; }
public class UnidadAnimacionIlustrada {
 public static UnidadAnimacionIlustrada Mock;
 public bool Disponible=true;
 public UnityEngine.Sprite PosePreparacion;
 public static UnidadAnimacionIlustrada IntentarCrear(UnidadPoseController c) { return Mock; }
 public void Reproducir(UnidadPoseController.TipoPoseActual p,UnityEngine.Sprite s,bool r) {}
 public void Detener() { Disponible=false; }
}
public static class PruebaPreparacion {
 static int checks;
 static void Check(bool value,string name) { if(!value) throw new Exception(name); checks++; }
 public static string Run() {
  var crouch=new UnityEngine.Sprite(); var idle=new UnityEngine.Sprite(); var ready=new UnityEngine.Sprite(); var bow=new UnityEngine.Sprite();
  var c=new UnidadPoseController {targetImage=new UnityEngine.UI.Image(),poseIdle=idle,poseTurnoActivo=ready,poseHabilidad=bow,poseMover=new UnityEngine.Sprite(),poseAtacar=new UnityEngine.Sprite()};
  var motor=new UnidadAnimacionIlustrada {PosePreparacion=crouch}; UnidadAnimacionIlustrada.Mock=motor;
  typeof(UnidadPoseController).GetMethod("Awake",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(c,null);
  Check(c.PlayPreparationPose(),"habilitar cuclillas"); Check(c.targetImage.sprite==crouch,"mostrar cuclillas");
  c.EnterPoseObjetivoHostil(); Check(c.targetImage.sprite==crouch,"refresco objetivo conserva cuclillas");
  var routine=c.Pending.Routine; routine.MoveNext(); routine.MoveNext(); Check(c.targetImage.sprite==ready,"expiracion restaura atento");
  c.ExitPoseObjetivoHostil(); Check(c.targetImage.sprite==idle,"desseleccion restaura idle");
  c.PlayPreparationPose(); c.PlaySkillPose(); Check(c.targetImage.sprite==bow,"disparo conserva original"); c.RefrescarPoseActual(); Check(c.targetImage.sprite==bow,"refresco no revive cuclillas");
  c.PlayPreparationPose(); c.OnStartMove(); Check(c.targetImage.sprite==c.poseMover,"dash interrumpe cuclillas"); c.OnStopMove(); Check(c.targetImage.sprite==idle,"llegada restaura idle");
  c.PlayPreparationPose(); c.EnterSkillPoseHold(); c.RefrescarPoseActual(); Check(c.targetImage.sprite==bow,"hold no reutiliza cuclillas"); Check(!c.PlayPreparationPose(),"respeta hold"); c.ExitPoseHold();
  c.PlayPreparationPose(); UnidadAnimacionIlustrada.Mock=null; var alternate=new UnityEngine.Sprite(); c.ConfigurarPoses(idle,c.poseMover,c.poseAtacar,alternate); Check(c.targetImage.sprite==alternate,"cambio apariencia descarta pose temporal"); Check(!c.PlayPreparationPose(),"apariencia sin perfil conserva fallback");
  c.RestaurarPosesBase(); Check(c.poseHabilidad==bow,"pose base no mutada");
  return checks+" comprobaciones del controlador real correctas (con dobles de Unity)";
 }
}
