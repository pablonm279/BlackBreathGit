using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class IALlamaDaZarkil : IAHabilidad
{
  [SerializeField] private GameObject prefabRefuerzoA;
  [SerializeField] private GameObject prefabRefuerzoB;
  [SerializeField] private GameObject prefabRefuerzoC;

  const int CantidadRefuerzos = 2;

  void Awake()
  {
    nombre = "Llamada Zarkil";
    Usuario = gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    hAncho = 0;
    esMelee = false;
    hAlcance = 0;
    hCooldownMax = 4;
    esHostil = false;
    prioridad = 20;
    costoAP = 5;
    afectaObstaculos = false;

    hActualCooldown = 0; // Arranca en cooldown.
  }

  void Start()
  {
    prioridad = 20;
  }

  public override List<object> ListaHayObjetivosAlAlcance()
  {
    objPosibles.Clear();

    if (PrefabsDisponibles().Count == 0)
    {
      return objPosibles;
    }

    if (scEstaUnidad == null)
    {
      scEstaUnidad = GetComponent<Unidad>();
    }

    if (scEstaUnidad != null)
    {
      objPosibles.Add(scEstaUnidad); // Usa la propia unidad como ancla para habilitar el uso.
    }

    return objPosibles;
  }

  public async override Task ActivarHabilidad()
  {
    if (BattleManager.Instance == null)
    {
      return;
    }

    List<GameObject> prefabs = PrefabsDisponibles();
    if (prefabs.Count == 0)
    {
      return;
    }

    scEstaUnidad = scEstaUnidad ?? GetComponent<Unidad>();
    scEstaUnidad?.CambiarAPActual(-costoAP);
    hActualCooldown = hCooldownMax;

    PrepararInicioAnimacion(null, null);
    VFXAplicar(scEstaUnidad.gameObject);
 //   scEstaUnidad?.ReproducirAnimacionHabilidadNoHostil();
    await BattleManager.DelayCombateAsync(450);
    
    AplicarEfectosEnZonaEnemiga();
    AplicarEfectosEnZonaAliada();

    for (int i = 0; i < CantidadRefuerzos; i++)
    {
      GameObject prefabElegido = prefabs[UnityEngine.Random.Range(0, prefabs.Count)];
      if (prefabElegido == null)
      {
        continue;
      }

      GameObject refuerzo = Instantiate(prefabElegido);
      refuerzo.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(refuerzo);
    }

    // Deja el contador de refuerzos en 1 turno.
    BattleManager.Instance.delayRefuerzo = BattleManager.Instance.RondaNro;
    BattleManager.Instance.ActualizarRefuerzosUI();
  }

  public override void AplicarEfectosHabilidad(object unidad)
  {
    // La habilidad no aplica efectos directos a objetivos individuales.
  }

  public void AplicarEfectosEnZonaAliada()
  { 
    List<Casilla> casillasAliadas = new List<Casilla>();
    casillasAliadas = scEstaUnidad.CasillaPosicion.ObtenerCasillasMismoLado();


    foreach (Casilla cas in casillasAliadas)
    {
      if (cas.Presente != null)
      {
        if (cas.Presente.GetComponent<Unidad>() != null)
        {
          Unidad obj = cas.Presente.GetComponent<Unidad>();

          if (obj.TieneTag("Zarkil") && obj != scEstaUnidad)
          {

            // BUFF ---- Así se aplica un buff/debuff
            Buff buff = new Buff();
            buff.buffNombre = "Orden Recibida";
            buff.boolfDebufftBuff = true;
            buff.DuracionBuffRondas = 1;
            buff.cantAPMax += 2;
            buff.AplicarBuff(obj);
            // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
            Buff buffComponent = ComponentCopier.CopyComponent(buff, obj.gameObject);
          }
          
        }
        
      }
    }




  }

   public void AplicarEfectosEnZonaEnemiga()
  { 
    List<Casilla> casillasAliadas = new List<Casilla>();
    casillasAliadas = scEstaUnidad.CasillaPosicion.ObtenerCasillasLadoOpuesto();


    foreach (Casilla cas in casillasAliadas)
    {
      if (cas.Presente != null)
      {
        if (cas.Presente.GetComponent<Unidad>() != null)
        {
          Unidad obj = cas.Presente.GetComponent<Unidad>();

          if (obj.TiradaSalvacion(obj.mod_TSMental, 10))
          {

            obj.estado_aturdido += 1;
            obj.GenerarTextoFlotante("Aturdido", Color.red);

          }

        }
      }

    }


  }

  public override object EstablecerObjetivoPrioritario()
  {
    return scEstaUnidad != null ? scEstaUnidad : gameObject.GetComponent<Unidad>();
  }

  List<GameObject> PrefabsDisponibles()
  {
    List<GameObject> prefabs = new List<GameObject>();
    if (prefabRefuerzoA != null) { prefabs.Add(prefabRefuerzoA); }
    if (prefabRefuerzoB != null) { prefabs.Add(prefabRefuerzoB); }
    if (prefabRefuerzoC != null) { prefabs.Add(prefabRefuerzoC); }
    return prefabs;
  }

  void VFXAplicar(GameObject objetivo)
  {
      GameObject VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_RefuerzosComandanteZarkil");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);  

  }
}



