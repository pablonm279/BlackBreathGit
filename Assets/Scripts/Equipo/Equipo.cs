using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Equipo : MonoBehaviour
{
  
  [SerializeField] Image imItemArma;
  [SerializeField] Image imItemArmadura;
  [SerializeField] Image imItemAccesorio1;
  [SerializeField] Image imItemAccesorio2;
  [SerializeField] Image imItemConsumible1;
  [SerializeField] Image imItemConsumible2;
  [SerializeField] Sprite vacio;
  [SerializeField] TextMeshProUGUI txtdisponibles;
 

  public List<GameObject> listInventario = new List<GameObject>();
 


  public GameObject goInventario;
  public GameObject prefabNtnInventario;
  public Transform listaItems;
  private int tipoInventarioAbierto = 5;
  public int TipoInventarioAbierto => tipoInventarioAbierto;



 public int accesorioACambiar;
 public int consumibleACambiar;

 public void ConfigurarClickDerechoSlots(MenuPersonajes menuPersonajes)
 {
    ConfigurarClickDerechoSlot(imItemArma, menuPersonajes, EquipoSlotPointerHandler.TipoSlot.Arma);
    ConfigurarClickDerechoSlot(imItemArmadura, menuPersonajes, EquipoSlotPointerHandler.TipoSlot.Armadura);
    ConfigurarClickDerechoSlot(imItemAccesorio1, menuPersonajes, EquipoSlotPointerHandler.TipoSlot.Accesorio1);
    ConfigurarClickDerechoSlot(imItemAccesorio2, menuPersonajes, EquipoSlotPointerHandler.TipoSlot.Accesorio2);
    ConfigurarClickDerechoSlot(imItemConsumible1, menuPersonajes, EquipoSlotPointerHandler.TipoSlot.Consumible1);
    ConfigurarClickDerechoSlot(imItemConsumible2, menuPersonajes, EquipoSlotPointerHandler.TipoSlot.Consumible2);
 }

 private static Transform ObtenerRaizSlotInteractivo(Transform origen)
  {
    Transform actual = origen;
    while (actual != null)
    {
      if (actual.GetComponent<Button>() != null)
      {
        return actual;
      }

      actual = actual.parent;
    }

    return origen;
 }

  private static void ConfigurarClickDerechoSlot(Image imagenSlot, MenuPersonajes menuPersonajes, EquipoSlotPointerHandler.TipoSlot tipoSlot)
  {
    if (imagenSlot == null || menuPersonajes == null)
    {
      return;
    }

    Transform raizSlot = ObtenerRaizSlotInteractivo(imagenSlot.transform);
    EquipoSlotPointerHandler handler = raizSlot.GetComponent<EquipoSlotPointerHandler>();
    if (handler == null)
    {
      handler = raizSlot.gameObject.AddComponent<EquipoSlotPointerHandler>();
    }

    handler.Configurar(menuPersonajes, tipoSlot);
  }

  private void CrearBotonInventario(Item item, bool oscurecido)
  {
    if (item == null)
    {
      return;
    }

    if (!AsegurarListaItems())
    {
      return;
    }

    GameObject btnItem = Instantiate(prefabNtnInventario, listaItems);
    btnItemInventario scBtnItem = btnItem.GetComponent<btnItemInventario>();
    if (scBtnItem == null)
    {
      return;
    }

    scBtnItem.imageMuestraItem.sprite = item.imItem;
    scBtnItem.itemRepresentado = item;
    scBtnItem.SetOscurecido(oscurecido);
  }

  private void MostrarItemsOrdenadosPorUsabilidad<T>() where T : Item
  {
    List<T> itemsUsables = new List<T>();
    List<T> itemsNoUsables = new List<T>();

    foreach (GameObject goItem in listInventario)
    {
      T item = goItem != null ? goItem.GetComponent<T>() : null;
      if (item == null)
      {
        continue;
      }

      if (PuedeMostrarPorClase(item))
      {
        itemsUsables.Add(item);
      }
      else
      {
        itemsNoUsables.Add(item);
      }
    }

    foreach (T item in itemsUsables)
    {
      CrearBotonInventario(item, false);
    }

    foreach (T item in itemsNoUsables)
    {
      CrearBotonInventario(item, true);
    }
  }

  private bool PuedeMostrarPorClase(Item item)
  {
    if (item == null)
    {
      return false;
    }

    MenuPersonajes menuPersonajes = CampaignManager.Instance != null ? CampaignManager.Instance.scMenuPersonajes : null;
    Personaje personajeSeleccionado = menuPersonajes != null ? menuPersonajes.pSel : null;
    if (personajeSeleccionado == null)
    {
      return true;
    }

    return item.PuedeUsarClase(personajeSeleccionado.IDClase);
 }

  public void MostrarInventario(int tipo) //1 Armas
  {
    tipoInventarioAbierto = tipo;
    ActualizarTextoDisponibles(tipo);

    if (!AsegurarListaItems())
    {
      return;
    }

    foreach (Transform transform in listaItems)//Esto remueve los botones anteriores antes de recalcular que botones corresponden
    {
            Destroy(transform.gameObject);
    }
   
   
    goInventario.SetActive(true);

    if(tipo==1)
    {
        MostrarItemsOrdenadosPorUsabilidad<Arma>();

    }
     if(tipo==2)
    {
        MostrarItemsOrdenadosPorUsabilidad<Armadura>();

    }
    if(tipo==3)
    {
        MostrarItemsOrdenadosPorUsabilidad<Accesorio>();

    }
    if(tipo==4)
    {
        MostrarItemsOrdenadosPorUsabilidad<Consumible>();

    }
    if(tipo==5) //todos
    {
        MostrarItemsOrdenadosPorUsabilidad<Item>();

    }

  }

  private bool AsegurarListaItems()
  {
    if (goInventario == null)
    {
      return listaItems != null;
    }

    Transform contenedorInventario = goInventario.transform.Find("Inventario");
    if (contenedorInventario != null)
    {
      listaItems = contenedorInventario;
      return true;
    }

    if (goInventario.name == "Inventario")
    {
      listaItems = goInventario.transform;
      return true;
    }

    return listaItems != null;
  }

  private void ActualizarTextoDisponibles(int tipo)
  {
    if (txtdisponibles == null)
    {
      return;
    }

    MenuPersonajes menuPersonajes = CampaignManager.Instance != null ? CampaignManager.Instance.scMenuPersonajes : null;
    Personaje personajeSeleccionado = menuPersonajes != null ? menuPersonajes.pSel : null;
    if (personajeSeleccionado == null)
    {
      txtdisponibles.text = string.Empty;
      return;
    }

    string textoTipo = ObtenerTextoTipoInventario(tipo);
    string textoClase = ObtenerTextoClase(personajeSeleccionado.IDClase);
    txtdisponibles.text =
      TRADU.i.Traducir(textoTipo)
      + TRADU.i.Traducir(" disponibles para ")
      + TRADU.i.Traducir(textoClase)
      + ".";
  }

  private static string ObtenerTextoTipoInventario(int tipo)
  {
    switch (tipo)
    {
      case 1: return "Armas";
      case 2: return "Armaduras";
      case 3: return "Accesorios";
      case 4: return "Consumibles";
      case 5: return "Items";
      default: return "Items";
    }
  }

  private static string ObtenerTextoClase(int idClase)
  {
    switch (idClase)
    {
      case 1: return "Caballero";
      case 2: return "Explorador";
      case 3: return "Purificadora";
      case 4: return "Acechador";
      case 5: return "Canalizador";
      case 6: return "Duelista";
      default: return "Personaje";
    }
  }

  public void RefrescarInventarioSiAbierto()
  {
    if (goInventario != null && goInventario.activeInHierarchy)
    {
      MostrarInventario(tipoInventarioAbierto);
    }
  }

  public void CerrarInventario()
  {
    goInventario.SetActive(false);

 }
 

  public void ActualizarEquipo(Personaje scPerssel)
  {
    ResetearBuffs();


   #region //ARMAS
   if(scPerssel.itemArma != null)
   {
    imItemArma.sprite = scPerssel.itemArma.imItem;
    //Aplica a buff total los buffs del arma
    BuffTOTALEQUIPOFuerza += scPerssel.itemArma.buffFuerza;
    BuffTOTALEQUIPOAgi += scPerssel.itemArma.buffAgi;
    BuffTOTALEQUIPOPoder += scPerssel.itemArma.buffPoder;
    BuffTOTALEQUIPOIniciativa += scPerssel.itemArma.buffIniciativa;
    BuffTOTALEQUIPOApMax += scPerssel.itemArma.buffApMax;
    BuffTOTALEQUIPOValMax += scPerssel.itemArma.buffValMax;
    BuffTOTALEQUIPOhpMax += scPerssel.itemArma.buffhpMax;
    BuffTOTALEQUIPOArmadura += scPerssel.itemArma.buffArmadura;
    BuffTOTALEQUIPODefensa += scPerssel.itemArma.buffDefensa;
    BuffTOTALEQUIPOTSReflejo += scPerssel.itemArma.buffTSReflejo;
    BuffTOTALEQUIPOTSFortaleza += scPerssel.itemArma.buffTSFortaleza;
    BuffTOTALEQUIPOTSMental += scPerssel.itemArma.buffTSMental;
    BuffTOTALEQUIPOResFuego += scPerssel.itemArma.buffResFuego;
    BuffTOTALEQUIPOResRayo += scPerssel.itemArma.buffResRayo;
    BuffTOTALEQUIPOResHielo += scPerssel.itemArma.buffResHielo;
    BuffTOTALEQUIPOResArcano += scPerssel.itemArma.buffResArcano;
    BuffTOTALEQUIPOResAcido += scPerssel.itemArma.buffResAcido;
    BuffTOTALEQUIPOResNecro += scPerssel.itemArma.buffResNecro;
    BuffTOTALEQUIPOResDivino += scPerssel.itemArma.buffResDivino;
 
 
//Todo esto agrega los componentes de habilidad del objeto a la lista de habilidades
if (scPerssel.itemArma.habilidadAtaque != null) // Agrega la habilidad del arma
{   
   
    // Obtener el tipo de la habilidad de ataque
    System.Type tipoHabilidad = scPerssel.itemArma.habilidadAtaque.GetType();

    // Verificar si el componente ya está presente
    if (scPerssel.gameObject.GetComponent(tipoHabilidad) == null)
    {
        // Añadir el componente del tipo específico a scPerssel
        Habilidad habilidadComponente = (Habilidad)scPerssel.gameObject.AddComponent(tipoHabilidad);
       
        habilidadComponente.agregaDesdeArmaUI = scPerssel.itemArma;
    }
}
if (scPerssel.itemArma.habilidadExtra1 != null) // Agrega la habilidad extra 1 del arma
{ 
    // Obtener el tipo de la habilidad extra 1
    System.Type tipoHabilidad = scPerssel.itemArma.habilidadExtra1.GetType();

    // Verificar si el componente ya está presente
    if (scPerssel.gameObject.GetComponent(tipoHabilidad) == null)
    {
        // Añadir el componente del tipo específico a scPerssel
        Habilidad habilidadComponente = (Habilidad)scPerssel.gameObject.AddComponent(tipoHabilidad);
       
        habilidadComponente.agregaDesdeArmaUI = scPerssel.itemArma;
    }
}
if (scPerssel.itemArma.habilidadExtra2 != null) // Agrega la habilidad extra 2 del arma
{   
    // Obtener el tipo de la habilidad extra 2
    System.Type tipoHabilidad = scPerssel.itemArma.habilidadExtra2.GetType();

    // Verificar si el componente ya está presente
    if (scPerssel.gameObject.GetComponent(tipoHabilidad) == null)
    {
        // Añadir el componente del tipo específico a scPerssel
        Habilidad habilidadComponente = (Habilidad)scPerssel.gameObject.AddComponent(tipoHabilidad);
       
        habilidadComponente.agregaDesdeArmaUI = scPerssel.itemArma;
    }
}


   }else{ imItemArma.sprite = vacio;}
 #endregion

   #region //ARMADURAS
   if(scPerssel.itemArmadura != null)
   {
    imItemArmadura.sprite = scPerssel.itemArmadura.imItem;
    //Aplica a buff total los buffs del Armadura
    BuffTOTALEQUIPOFuerza += scPerssel.itemArmadura.buffFuerza;
    BuffTOTALEQUIPOAgi += scPerssel.itemArmadura.buffAgi;
    BuffTOTALEQUIPOPoder += scPerssel.itemArmadura.buffPoder;
    BuffTOTALEQUIPOIniciativa += scPerssel.itemArmadura.buffIniciativa;
    BuffTOTALEQUIPOApMax += scPerssel.itemArmadura.buffApMax;
    BuffTOTALEQUIPOValMax += scPerssel.itemArmadura.buffValMax;
    BuffTOTALEQUIPOhpMax += scPerssel.itemArmadura.buffhpMax;
    BuffTOTALEQUIPOArmadura += scPerssel.itemArmadura.buffArmadura;
    BuffTOTALEQUIPODefensa += scPerssel.itemArmadura.buffDefensa;
    BuffTOTALEQUIPOTSReflejo += scPerssel.itemArmadura.buffTSReflejo;
    BuffTOTALEQUIPOTSFortaleza += scPerssel.itemArmadura.buffTSFortaleza;
    BuffTOTALEQUIPOTSMental += scPerssel.itemArmadura.buffTSMental;
    BuffTOTALEQUIPOResFuego += scPerssel.itemArmadura.buffResFuego;
    BuffTOTALEQUIPOResRayo += scPerssel.itemArmadura.buffResRayo;
    BuffTOTALEQUIPOResHielo += scPerssel.itemArmadura.buffResHielo;
    BuffTOTALEQUIPOResArcano += scPerssel.itemArmadura.buffResArcano;
    BuffTOTALEQUIPOResAcido += scPerssel.itemArmadura.buffResAcido;
    BuffTOTALEQUIPOResNecro += scPerssel.itemArmadura.buffResNecro;
    BuffTOTALEQUIPOResDivino += scPerssel.itemArmadura.buffResDivino;

//Todo esto agrega los componentes de habilidad del objeto a la lista de habilidades
if (scPerssel.itemArmadura.habilidadExtra1 != null) // Agrega la habilidad extra 1 del Armadura
{
    // Obtener el tipo de la habilidad extra 1
    System.Type tipoHabilidad = scPerssel.itemArmadura.habilidadExtra1.GetType();

    // Verificar si el componente ya está presente
    if (scPerssel.gameObject.GetComponent(tipoHabilidad) == null)
    {
        // Añadir el componente del tipo específico a scPerssel
        Habilidad habilidadComponente = (Habilidad)scPerssel.gameObject.AddComponent(tipoHabilidad);
       
        habilidadComponente.agregaDesdeArmaUI = scPerssel.itemArmadura;
    }
}
if (scPerssel.itemArmadura.habilidadExtra2 != null) // Agrega la habilidad extra 2 del Armadura
{
    // Obtener el tipo de la habilidad extra 2
    System.Type tipoHabilidad = scPerssel.itemArmadura.habilidadExtra2.GetType();

    // Verificar si el componente ya está presente
    if (scPerssel.gameObject.GetComponent(tipoHabilidad) == null)
    {
        // Añadir el componente del tipo específico a scPerssel
        Habilidad habilidadComponente = (Habilidad)scPerssel.gameObject.AddComponent(tipoHabilidad);
       
        habilidadComponente.agregaDesdeArmaUI = scPerssel.itemArmadura;
    }
}


   }else{ imItemArmadura.sprite = vacio;}
 #endregion

   #region //Accesorio1
   if(scPerssel.Accesorio1 != null)
   {
    imItemAccesorio1.sprite = scPerssel.Accesorio1.imItem;
    //Aplica a buff total los buffs del Accesorio1
    BuffTOTALEQUIPOFuerza += scPerssel.Accesorio1.buffFuerza;
    BuffTOTALEQUIPOAgi += scPerssel.Accesorio1.buffAgi;
    BuffTOTALEQUIPOPoder += scPerssel.Accesorio1.buffPoder;
    BuffTOTALEQUIPOIniciativa += scPerssel.Accesorio1.buffIniciativa;
    BuffTOTALEQUIPOApMax += scPerssel.Accesorio1.buffApMax;
    BuffTOTALEQUIPOValMax += scPerssel.Accesorio1.buffValMax;
    BuffTOTALEQUIPOhpMax += scPerssel.Accesorio1.buffhpMax;
    BuffTOTALEQUIPOArmadura += scPerssel.Accesorio1.buffArmadura;
    BuffTOTALEQUIPODefensa += scPerssel.Accesorio1.buffDefensa;
    BuffTOTALEQUIPOTSReflejo += scPerssel.Accesorio1.buffTSReflejo;
    BuffTOTALEQUIPOTSFortaleza += scPerssel.Accesorio1.buffTSFortaleza;
    BuffTOTALEQUIPOTSMental += scPerssel.Accesorio1.buffTSMental;
    BuffTOTALEQUIPOResFuego += scPerssel.Accesorio1.buffResFuego;
    BuffTOTALEQUIPOResRayo += scPerssel.Accesorio1.buffResRayo;
    BuffTOTALEQUIPOResHielo += scPerssel.Accesorio1.buffResHielo;
    BuffTOTALEQUIPOResArcano += scPerssel.Accesorio1.buffResArcano;
    BuffTOTALEQUIPOResAcido += scPerssel.Accesorio1.buffResAcido;
    BuffTOTALEQUIPOResNecro += scPerssel.Accesorio1.buffResNecro;
    BuffTOTALEQUIPOResDivino += scPerssel.Accesorio1.buffResDivino;

//Todo esto agrega los componentes de habilidad del objeto a la lista de habilidades

if (scPerssel.Accesorio1.habilidadExtra1 != null) // Agrega la habilidad extra 1 del Accesorio1
{
    // Obtener el tipo de la habilidad extra 1
    System.Type tipoHabilidad = scPerssel.Accesorio1.habilidadExtra1.GetType();

    // Verificar si el componente ya está presente
    if (scPerssel.gameObject.GetComponent(tipoHabilidad) == null)
    {
        // Añadir el componente del tipo específico a scPerssel
        Habilidad habilidadComponente = (Habilidad)scPerssel.gameObject.AddComponent(tipoHabilidad);
       
        habilidadComponente.agregaDesdeArmaUI = scPerssel.Accesorio1;
    }
}
if (scPerssel.Accesorio1.habilidadExtra2 != null) // Agrega la habilidad extra 2 del Accesorio1
{
    // Obtener el tipo de la habilidad extra 2
    System.Type tipoHabilidad = scPerssel.Accesorio1.habilidadExtra2.GetType();

    // Verificar si el componente ya está presente
    if (scPerssel.gameObject.GetComponent(tipoHabilidad) == null)
    {
        // Añadir el componente del tipo específico a scPerssel
        Habilidad habilidadComponente = (Habilidad)scPerssel.gameObject.AddComponent(tipoHabilidad);
       
        habilidadComponente.agregaDesdeArmaUI = scPerssel.Accesorio1;
    }
}


   }else{ imItemAccesorio1.sprite = vacio;}
 #endregion

   #region //Accesorio2
   if(scPerssel.Accesorio2 != null)
   {
    imItemAccesorio2.sprite = scPerssel.Accesorio2.imItem;
    //Aplica a buff total los buffs del Accesorio2
    BuffTOTALEQUIPOFuerza += scPerssel.Accesorio2.buffFuerza;
    BuffTOTALEQUIPOAgi += scPerssel.Accesorio2.buffAgi;
    BuffTOTALEQUIPOPoder += scPerssel.Accesorio2.buffPoder;
    BuffTOTALEQUIPOIniciativa += scPerssel.Accesorio2.buffIniciativa;
    BuffTOTALEQUIPOApMax += scPerssel.Accesorio2.buffApMax;
    BuffTOTALEQUIPOValMax += scPerssel.Accesorio2.buffValMax;
    BuffTOTALEQUIPOhpMax += scPerssel.Accesorio2.buffhpMax;
    BuffTOTALEQUIPOArmadura += scPerssel.Accesorio2.buffArmadura;
    BuffTOTALEQUIPODefensa += scPerssel.Accesorio2.buffDefensa;
    BuffTOTALEQUIPOTSReflejo += scPerssel.Accesorio2.buffTSReflejo;
    BuffTOTALEQUIPOTSFortaleza += scPerssel.Accesorio2.buffTSFortaleza;
    BuffTOTALEQUIPOTSMental += scPerssel.Accesorio2.buffTSMental;
    BuffTOTALEQUIPOResFuego += scPerssel.Accesorio2.buffResFuego;
    BuffTOTALEQUIPOResRayo += scPerssel.Accesorio2.buffResRayo;
    BuffTOTALEQUIPOResHielo += scPerssel.Accesorio2.buffResHielo;
    BuffTOTALEQUIPOResArcano += scPerssel.Accesorio2.buffResArcano;
    BuffTOTALEQUIPOResAcido += scPerssel.Accesorio2.buffResAcido;
    BuffTOTALEQUIPOResNecro += scPerssel.Accesorio2.buffResNecro;
    BuffTOTALEQUIPOResDivino += scPerssel.Accesorio2.buffResDivino;

//Todo esto agrega los componentes de habilidad del objeto a la lista de habilidades

if (scPerssel.Accesorio2.habilidadExtra1 != null) // Agrega la habilidad extra 1 del Accesorio2
{
    // Obtener el tipo de la habilidad extra 1
    System.Type tipoHabilidad = scPerssel.Accesorio2.habilidadExtra1.GetType();

    // Verificar si el componente ya está presente
    if (scPerssel.gameObject.GetComponent(tipoHabilidad) == null)
    {
        // Añadir el componente del tipo específico a scPerssel
        Habilidad habilidadComponente = (Habilidad)scPerssel.gameObject.AddComponent(tipoHabilidad);
       
        habilidadComponente.agregaDesdeArmaUI = scPerssel.Accesorio2;
    }
}
if (scPerssel.Accesorio2.habilidadExtra2 != null) // Agrega la habilidad extra 2 del Accesorio2
{
    // Obtener el tipo de la habilidad extra 2
    System.Type tipoHabilidad = scPerssel.Accesorio2.habilidadExtra2.GetType();

    // Verificar si el componente ya está presente
    if (scPerssel.gameObject.GetComponent(tipoHabilidad) == null)
    {
        // Añadir el componente del tipo específico a scPerssel
        Habilidad habilidadComponente = (Habilidad)scPerssel.gameObject.AddComponent(tipoHabilidad);
       
        habilidadComponente.agregaDesdeArmaUI = scPerssel.Accesorio2;
    }
}


   }else{ imItemAccesorio2.sprite = vacio;}
 #endregion

   #region //Consumible1
   if(scPerssel.Consumible1 != null)
   {
     imItemConsumible1.sprite = scPerssel.Consumible1.imItem;
   }else{ imItemConsumible1.sprite = vacio;}
   #endregion

   #region //Consumible2
   if(scPerssel.Consumible2 != null)
   {
     imItemConsumible2.sprite = scPerssel.Consumible2.imItem;
   }else{ imItemConsumible2.sprite = vacio;}
   #endregion
   
  }


  void ResetearBuffs()
  {

        BuffTOTALEQUIPOFuerza = 0;
        BuffTOTALEQUIPOAgi = 0;
        BuffTOTALEQUIPOPoder = 0;
        BuffTOTALEQUIPOIniciativa = 0;
        BuffTOTALEQUIPOApMax = 0;
        BuffTOTALEQUIPOhpMax = 0;
        BuffTOTALEQUIPOValMax = 0;
        BuffTOTALEQUIPOArmadura = 0;
        BuffTOTALEQUIPODefensa = 0;
        BuffTOTALEQUIPOTSReflejo = 0;
        BuffTOTALEQUIPOTSFortaleza = 0;
        BuffTOTALEQUIPOTSMental = 0;
        BuffTOTALEQUIPOResFuego = 0;
        BuffTOTALEQUIPOResRayo = 0;
        BuffTOTALEQUIPOResHielo = 0;
        BuffTOTALEQUIPOResArcano = 0;
        BuffTOTALEQUIPOResAcido = 0;
        BuffTOTALEQUIPOResNecro = 0;
        BuffTOTALEQUIPOResDivino = 0;


  }
 //BuffTOTALEQUIPOs
    public int BuffTOTALEQUIPOFuerza;
    public int BuffTOTALEQUIPOAgi;
    public int BuffTOTALEQUIPOPoder;
    public int BuffTOTALEQUIPOIniciativa;
    public int BuffTOTALEQUIPOApMax;
    public int BuffTOTALEQUIPOhpMax;
    public int BuffTOTALEQUIPOValMax;
    public int BuffTOTALEQUIPOArmadura;
    public int BuffTOTALEQUIPODefensa;
    public int BuffTOTALEQUIPOTSReflejo;
    public int BuffTOTALEQUIPOTSFortaleza;
    public int BuffTOTALEQUIPOTSMental;
    public int BuffTOTALEQUIPOResFuego;
    public int BuffTOTALEQUIPOResRayo;
    public int BuffTOTALEQUIPOResHielo;
    public int BuffTOTALEQUIPOResArcano;
    public int BuffTOTALEQUIPOResAcido;
    public int BuffTOTALEQUIPOResNecro;
    public int BuffTOTALEQUIPOResDivino;


}



