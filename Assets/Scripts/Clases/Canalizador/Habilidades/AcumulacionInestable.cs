using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;
using UnityEditor;

public class AcumulacionInestable : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
      
    public override void  Awake()
    {
      nombre = "Acumulación Inestable";
      IDenClase = 4; 
      costoAP = 1;
      costoPM = 0;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 4;
      if (NIVEL > 1 ) { cooldownMax--; }
      bAfectaObstaculos = false;

      imHab = Resources.Load<Sprite>("imHab/Canalizador_AcumulacionInestable");


    Invoke("PonerCD", 1.2f);
      
    
    }

  void PonerCD()
  { 
     if (NIVEL != 4 ) { cooldownActual = cooldownMax; }

  }
  public override void ActualizarDescripcion()
  {

    if (NIVEL < 2)
    {
      txtDescripcion = "<color=#5dade2><b>Acumulación Inestable I</b></color>\n\n";
      txtDescripcion += "<i>Manipulando su energía interna de forma riesgosa, el personaje incrementa inmediatamente su energía a costa de su bienestar.</i>\n\n";
      txtDescripcion += $"<color=#c8c8c8>- Gana <b>+1 Tier de Energía</b> al instante.\n- Obtiene <color=#ea0606>+5 Daño Arcano</color> por el turno.\n- Recibe <color=#ea0606>1d6 Daño Arcano</color> al usarla.</color>\n\n";
      txtDescripcion += $"<color=#44d3ec>- Cooldown: {cooldownMax} (arranca en Cooldown)\n- Costo AP: {costoAP} \n- Costo Val: {costoPM}</color>\n\n";

      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>- Próximo Nivel: -1 Cooldown</color>\n\n";
          }
        }
      }
    }

    if (NIVEL == 2)
    {
      txtDescripcion = "<color=#5dade2><b>Acumulación Inestable II</b></color>\n\n";
      txtDescripcion += "<i>Manipulando su energía interna de forma riesgosa, el personaje incrementa inmediatamente su energía a costa de su bienestar.</i>\n\n";
      txtDescripcion += $"<color=#c8c8c8>- Gana <b>+1 Tier de Energía</b> al instante.\n- Obtiene <color=#ea0606>+5 Daño Arcano</color> por el turno.\n- Recibe <color=#ea0606>1d6 Daño Arcano</color> al usarla.</color>\n\n";
      txtDescripcion += $"<color=#44d3ec>- Cooldown: {cooldownMax - 1} (arranca en Cooldown)\n- Costo AP: {costoAP} \n- Costo Val: {costoPM}</color>\n\n";

      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>- Próximo Nivel: +2 Daño Arcano</color>\n\n";
          }
        }
      }
    }

    if (NIVEL == 3)
    {
      txtDescripcion = "<color=#5dade2><b>Acumulación Inestable III</b></color>\n\n";
      txtDescripcion += "<i>Manipulando su energía interna de forma riesgosa, el personaje incrementa inmediatamente su energía a costa de su bienestar.</i>\n\n";
      txtDescripcion += $"<color=#c8c8c8>- Gana <b>+1 Tier de Energía</b> al instante.\n- Obtiene <color=#ea0606>+7 Daño Arcano</color> por el turno.\n- Recibe <color=#ea0606>1d6 Daño Arcano</color> al usarla.</color>\n\n";
      txtDescripcion += $"<color=#44d3ec>- Cooldown: {cooldownMax - 1} (arranca en Cooldown)\n- Costo AP: {costoAP} \n- Costo Val: {costoPM}</color>\n\n";

      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>- Opción A: No arranca en Cooldown</color>\n";
            txtDescripcion += $"<color=#dfea02>- Opción B: No recibe daño al usarla</color>\n\n";
          }
        }
      }
    }

    if (NIVEL == 4)
    {
      txtDescripcion = "<color=#5dade2><b>Acumulación Inestable IV a</b></color>\n\n";
      txtDescripcion += "<i>Manipulando su energía interna de forma riesgosa, el personaje incrementa inmediatamente su energía a costa de su bienestar.</i>\n\n";
      txtDescripcion += $"<color=#c8c8c8>- Gana <b>+1 Tier de Energía</b> al instante.\n- Obtiene <color=#ea0606>+7 Daño Arcano</color> por el turno.\n- Recibe <color=#ea0606>1d6 Daño Arcano</color> al usarla.</color>\n\n";
      txtDescripcion += $"<color=#44d3ec>- Cooldown: {cooldownMax - 1}\n- Costo AP: {costoAP} \n- Costo Val: {costoPM}</color>\n\n";
    }

    if (NIVEL == 5)
    {
      txtDescripcion = "<color=#5dade2><b>Acumulación Inestable IV b</b></color>\n\n";
      txtDescripcion += "<i>Manipulando su energía interna de forma riesgosa, el personaje incrementa inmediatamente su energía a costa de su bienestar.</i>\n\n";
      txtDescripcion += $"<color=#c8c8c8>- Gana <b>+1 Tier de Energía</b> al instante.\n- Obtiene <color=#ea0606>+7 Daño Arcano</color> por el turno.\n- <b>No recibe daño al usarla.</b></color>\n\n";
      txtDescripcion += $"<color=#44d3ec>- Cooldown: {cooldownMax - 1} (arranca en Cooldown)\n- Costo AP: {costoAP} \n- Costo Val: {costoPM}</color>\n\n";
    }

  }

   
    public override async Task Resolver(List<object> Objetivos, Casilla cas) //Esto esta hecho para que anuncie el uso de la habilidad en el Log
    {
        // El log de uso ahora está centralizado en Habilidad.Resolver
        base.Resolver(Objetivos);
    }



    Casilla Origen;
    public override void Activar()
    {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

        
    }
    
    

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla casillaObjetivo)
    {

     ClaseCanalizador scClaseCana = (ClaseCanalizador)scEstaUnidad;
     int NivelAcumulacionProtegida = scClaseCana.PASIVA_AcumulacionProtegida;

    if (obj is ClaseCanalizador scCana) //Acá van los efectos a Unidades.
    {

      Unidad objetivo = (Unidad)obj;
      VFXAplicar(objetivo.gameObject);
      /////////////////////////////////////////////
      //BUFF ---- Así se aplica un buff/debuff
      Buff buff = new Buff();
      buff.buffNombre = "Acumulacion Inestable";
      buff.boolfDebufftBuff = true;
      buff.DuracionBuffRondas = 1;
      buff.cantDamBonusElementalArc = 5;
      if (NIVEL > 2 ) { buff.cantDamBonusElementalArc += 2; }
      buff.AplicarBuff(objetivo);
      // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
      Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);
      objetivo.Marcar(0);


      scCana.CambiarEnergia(1);

      int rand = UnityEngine.Random.Range(1,6);
      if (NIVEL != 5 ) { scCana.RecibirDanio(rand, 8, false, null);  }
 

      }
    }
    
         void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_AcumulacionInestable");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   canvasObjeto.overrideSorting = true;
   canvasObjeto.sortingOrder =  200;  

    }

    //Provisorio
    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();

    private void ObtenerObjetivos()
    {
      //Cualquier objetivo en 1 de alcance 3 de ancho
      lObjetivosPosibles.Clear();
    
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasMismoLado();
     
    
      foreach(Casilla c in lCasillasafectadas)
      {
       
        
        if(c.Presente == null)
        {
            continue;
        }
        
       
        if(c.Presente.GetComponent<Unidad>() == null)
        {
            continue;
        }
           
        if(c.Presente.GetComponent<Unidad>() != null)
        {
            if(c.Presente.GetComponent<Unidad>() == scEstaUnidad)
            {
             c.ActivarCapaColorAzul();
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());
            }
        }

      }
    

      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva = new List<Unidad>(lObjetivosPosibles);

      foreach(Unidad uni in BattleManager.Instance.lUnidadesPosiblesHabilidadActiva)
      {
            uni.Marcar(1);
      }

      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
       
    }
 
}
