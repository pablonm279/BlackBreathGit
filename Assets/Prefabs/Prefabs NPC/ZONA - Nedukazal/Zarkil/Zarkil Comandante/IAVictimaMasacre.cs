using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class IAVictimaMasacre : IAHabilidad
{
  const float DuracionVfx = 1f;
  const int SegmentosVfx = 28;

  static Material materialVfx;

  void Awake()
  {
    nombre = "Victima de la masacre";
    Usuario = gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    hAncho = 2;
    esMelee = false;
    hAlcance = 6;
    hCooldownMax = 3;
    esHostil = true;
    prioridad = 5;
    costoAP = 2;
    afectaObstaculos = false;

    hActualCooldown = 0;
  }

  void Start()
  {
    prioridad = 5;
  }

  public async override Task ActivarHabilidad()
  {
    scEstaUnidad = scEstaUnidad ?? GetComponent<Unidad>();
    Unidad objetivo = EstablecerObjetivoPrioritario() as Unidad;
    if (objetivo == null)
    {
      return;
    }

    scEstaUnidad.CambiarAPActual(-costoAP);
    hActualCooldown = hCooldownMax;

    PrepararInicioAnimacion(null, objetivo);
    scEstaUnidad.ReproducirAnimacionHabilidadNoHostil();

    await BattleManager.DelayCombateAsync(450);
    AplicarEfectosHabilidad(objetivo);
    await BattleManager.DelayCombateAsync(650);
  }

  public override void AplicarEfectosHabilidad(object obj)
  {
    if (!(obj is Unidad objetivo))
    {
      return;
    }

    Buff debuff = new Buff();
    debuff.buffNombre = "Victima de la masacre";
    debuff.boolfDebufftBuff = false;
    debuff.DuracionBuffRondas = 2;
    debuff.cantResFue -= 6;
    debuff.cantResHie -= 6;
    debuff.cantResRay -= 6;
    debuff.cantResAci -= 6;
    debuff.cantResArc -= 6;
    debuff.cantResNec -= 6;
    debuff.cantResDiv -= 6;
    debuff.cantDefensa -= 2;
    debuff.esStackeable = false;
    debuff.AplicarBuff(objetivo);
    ComponentCopier.CopyComponent(debuff, objetivo.gameObject);

    MostrarVfx(objetivo);
  }

  public override object EstablecerObjetivoPrioritario()
  {
    Unidad caster = scEstaUnidad ?? GetComponent<Unidad>();
    if (caster == null)
    {
      return null;
    }

    return objPosibles.OfType<Unidad>()
      .OrderByDescending(u => u.CasillaPosicion.posX)
      .ThenBy(u => Mathf.Abs(u.CasillaPosicion.posY - caster.CasillaPosicion.posY))
      .FirstOrDefault();
  }

  void MostrarVfx(Unidad objetivo)
  {
    StartCoroutine(AnimarVfx(objetivo));
  }

  IEnumerator AnimarVfx(Unidad objetivo)
  {
    if (objetivo == null)
    {
      yield break;
    }

    GameObject vfxGO = new GameObject("VFX_VictimaMasacre");
    vfxGO.transform.SetParent(objetivo.transform, false);

    LineRenderer lr = vfxGO.AddComponent<LineRenderer>();
    lr.useWorldSpace = false;
    lr.loop = true;
    lr.positionCount = SegmentosVfx;
    lr.material = ObtenerMaterialVfx();
    lr.widthMultiplier = 0.03f;
    lr.alignment = LineAlignment.View;
    lr.textureMode = LineTextureMode.DistributePerSegment;
    lr.numCapVertices = 2;
    lr.numCornerVertices = 2;

    float tiempo = 0f;
    while (tiempo < DuracionVfx && lr != null)
    {
      tiempo += Time.deltaTime;
      float radio = Mathf.Lerp(0.18f, 0.32f, 0.5f * (Mathf.Sin(tiempo * 7f) + 1f));
      ActualizarCirculo(lr, radio);

      float pulso = Mathf.PingPong(tiempo * 1.6f, 1f);
      Color c = Color.Lerp(new Color(0.55f, 0.05f, 0.05f, 0.5f), new Color(0.8f, 0.18f, 0.1f, 0.2f), pulso);
      lr.startColor = c;
      lr.endColor = c;

      yield return null;
    }

    if (vfxGO != null)
    {
      Destroy(vfxGO);
    }
  }

  void ActualizarCirculo(LineRenderer lr, float radio)
  {
    if (lr == null)
    {
      return;
    }

    for (int i = 0; i < SegmentosVfx; i++)
    {
      float angulo = i / (float)SegmentosVfx * Mathf.PI * 2f;
      float x = Mathf.Cos(angulo) * radio;
      float z = Mathf.Sin(angulo) * radio;
      lr.SetPosition(i, new Vector3(x, 0.03f, z));
    }
  }

  static Material ObtenerMaterialVfx()
  {
    if (materialVfx == null)
    {
      Shader shader = Shader.Find("Sprites/Default");
      materialVfx = new Material(shader);
      materialVfx.name = "Mat_VictimaMasacre";
    }

    return materialVfx;
  }
}
