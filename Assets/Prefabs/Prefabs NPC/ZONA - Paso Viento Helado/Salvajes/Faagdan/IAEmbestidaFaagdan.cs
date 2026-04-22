using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class IAEmbestidaFaagdan : IAHabilidad
{
  [SerializeField] public int pPrioridad = 15;
  [SerializeField] private int XdDanio = 3;
  [SerializeField] private int daniodX = 8;
  [SerializeField] private int tipoDanio = 3; // Contundente
  [SerializeField] private int fortitudeDC = 12;

  private const string VfxPath = "VFX/VFX_EmbestidaFaagdan";
  private GameObject vfxPrefab;

  private void Awake()
  {
    nombre = "Embestida Faagdan";
    Usuario = gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    hAncho = 0;
    esMelee = false;
    hAlcance = 5;
    hCooldownMax = 3;
    esHostil = true;
    prioridad = 15;
    costoAP = 3;
    afectaObstaculos = true;

    hActualCooldown = UnityEngine.Random.Range(0, hCooldownMax + 1);

    vfxPrefab = Resources.Load<GameObject>(VfxPath);
  }

  private void Start()
  {
    prioridad = 15;
  }

  public async override Task ActivarHabilidad()
  {
    scEstaUnidad.CambiarAPActual(-costoAP);
    hActualCooldown = hCooldownMax;
    scEstaUnidad.ReproducirAnimacionAtaque();

    object objetivoPrincipal = EstablecerObjetivoPrioritario();
    List<Unidad> unidadesObjetivo = ObtenerUnidadesEnFila(objetivoPrincipal);

    if (objetivoPrincipal is Unidad unidadObjetivo && !unidadesObjetivo.Contains(unidadObjetivo))
    {
      unidadesObjetivo.Add(unidadObjetivo);
    }

    if (unidadesObjetivo.Count > 0)
    {
      PrepararInicioAnimacion(unidadesObjetivo.Cast<object>().ToList(), null);
    }
    else
    {
      PrepararInicioAnimacion(null, objetivoPrincipal);
    }
      
    await BattleManager.DelayCombateAsync(1500);

    AplicarEfectosEnFila(unidadesObjetivo);
  }

  private List<Unidad> ObtenerUnidadesEnFila(object objetivoReferencia)
  {
    var resultado = new HashSet<Unidad>();
    Casilla casillaReferencia = null;

    switch (objetivoReferencia)
    {
      case Unidad unidad:
        casillaReferencia = unidad.CasillaPosicion;
        break;
      case Obstaculo obstaculo:
        casillaReferencia = obstaculo.CasillaPosicion;
        break;
    }

    if (casillaReferencia == null)
    {
      return resultado.ToList();
    }

    foreach (Casilla casilla in casillaReferencia.ObtenerCasillasenMismaFila())
    {
      if (casilla.Presente == null)
      {
        continue;
      }

      Unidad unidad = casilla.Presente.GetComponent<Unidad>();
      if (unidad == null)
      {
        continue;
      }

      if (unidad == scEstaUnidad)
      {
        continue;
      }

      if (unidad.CasillaPosicion.lado == scEstaUnidad.CasillaPosicion.lado)
      {
        continue;
      }

      resultado.Add(unidad);
    }

    return resultado.ToList();
  }

  private void AplicarEfectosEnFila(IEnumerable<Unidad> unidades)
  {
    foreach (Unidad unidad in unidades)
    {
      AplicarEfectosHabilidad(unidad);
    }
  }

  public override void AplicarEfectosHabilidad(object obj)
  {
    if (obj is not Unidad objetivo)
    {
      return;
    }
    VFXAplicar(objetivo.gameObject);
    
  
    float danio = TiradaDeDados.TirarDados(XdDanio, daniodX);
    danio = danio / 100f * (100 + scEstaUnidad.mod_DanioPorcentaje);
    objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
    //objetivo.AplicarDebuffPorAtaquesreiterados(1);

    bool noSeSalva = objetivo.TiradaSalvacion(objetivo.mod_TSFortaleza, fortitudeDC);
    if (noSeSalva)
    {
      EmpujarVerticalAleatorioAsync(objetivo);
       // BUFF ---- Así se aplica un buff/debuff
      Buff buff = new Buff();
      buff.buffNombre = "Derribado";
      buff.boolfDebufftBuff = false;
      buff.DuracionBuffRondas = 1;
      buff.cantAPMax -= 2;
      buff.cantDefensa -= 2;
      buff.AplicarBuff(objetivo);
      // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
      Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);

    }
  }

  private void VFXAplicar(GameObject objetivo)
  {
   
   
    GameObject VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_EmbestidaFaagdan");


    GameObject vfx = Object.Instantiate(vfxPrefab, objetivo.transform.position, Quaternion.identity);
    //vfx.transform.SetParent(objetivo.transform);

    Canvas canvas = vfx.GetComponentInChildren<Canvas>();
    if (canvas != null)
    {
      canvas.overrideSorting = true;
      canvas.sortingOrder = 200;
    }
  }

  private async Task EmpujarVerticalAleatorioAsync(Unidad objetivo)
  {
    await BattleManager.DelayCombateAsync(600);

    if (objetivo == null || objetivo.HP_actual <= 0f || !objetivo.gameObject.activeInHierarchy)
    {
      return;
    }

    Casilla origen = objetivo.CasillaPosicion;
    if (origen == null || origen.ladoGO == null)
    {
      return;
    }

    LadoManager lado = origen.ladoGO.GetComponent<LadoManager>();
    if (lado == null)
    {
      return;
    }

    List<Casilla> candidatos = new List<Casilla>();
    Casilla arriba = lado.ObtenerCasillaPorIndex(origen.posX, origen.posY + 1);
    Casilla abajo = lado.ObtenerCasillaPorIndex(origen.posX, origen.posY - 1);

    if (arriba != null && arriba.Presente == null)
    {
      candidatos.Add(arriba);
    }

    if (abajo != null && abajo.Presente == null)
    {
      candidatos.Add(abajo);
    }

    if (candidatos.Count == 0)
    {
      return;
    }

    Casilla destino = candidatos.Count == 1
      ? candidatos[0]
      : candidatos[UnityEngine.Random.value < 0.5f ? 0 : 1];

    objetivo.IntentarProgramarMovimientoForzado(destino);
  }

  public override object EstablecerObjetivoPrioritario()
  {
    Unidad unidadDuena = gameObject.GetComponent<Unidad>();
    if (unidadDuena == null)
    {
      return null;
    }

    List<Unidad> unidades = objPosibles.OfType<Unidad>().ToList();
    var unidadesOrdenadas = unidades
      .OrderByDescending(unidad => unidad.CasillaPosicion.posX)
      .ThenBy(unidad => Mathf.Abs(unidad.CasillaPosicion.posY - unidadDuena.CasillaPosicion.posY))
      .ToList();

    if (unidadesOrdenadas.Any())
    {
      return unidadesOrdenadas.First();
    }

    return objPosibles.OfType<Obstaculo>().FirstOrDefault();
  }
}



