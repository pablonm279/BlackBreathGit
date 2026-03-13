using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class IALlamadaEspectral : IAHabilidad
{
  [SerializeField] private GameObject prefabRefuerzoA;
  [SerializeField] private GameObject prefabRefuerzoB;
  [SerializeField] private GameObject prefabRefuerzoC;

  const int CantidadRefuerzos = 2;

  void Awake()
  {
    nombre = "Llamada Espectral";
    Usuario = gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    hAncho = 0;
    esMelee = false;
    hAlcance = 0;
    hCooldownMax = 6;
    esHostil = false;
    prioridad = 20;
    costoAP = 4;
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
      GameObject VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_RefuerzosLlamadaEspectral");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);  

  }
}
