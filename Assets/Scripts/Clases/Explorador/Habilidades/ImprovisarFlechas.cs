using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class ImprovisarFlechas : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] int usosBatalla;
  
    
     public override void  Awake()
    {
      nombre = "Improvisar Flechas";
      IDenClase = 3;
      costoAP = 0;
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
      bAfectaObstaculos = false;
      
     
      usosBatalla = 2;

      imHab = Resources.Load<Sprite>("imHab/Explorador_ImprovisarFlechas");
      ActualizarDescripcion();
    }
    public override void ActualizarDescripcion()
    {
         if(NIVEL<2)
       {
        txtDescripcion = "<color=#5dade2><b>Improvisar Flechas I</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Explorador crea 1 flecha por AP restante, termina el Turno. Además obtiene +1 Probabilidad de crítico por 1 turno.</i>\n\n";

        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM}</color>\n\n";

         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Flecha creada.</color>\n\n";
          }
          }
        }
       }
       if(NIVEL==2)
       {
       
        txtDescripcion = "<color=#5dade2><b>Improvisar Flechas II</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Explorador crea 1 flecha por AP restante +1, termina el Turno. Además obtiene +1 Probabilidad de crítico por 1 turno.</i>\n\n";

        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM}</color>\n\n";

         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Probabilidad de crítico </color>\n\n";
          }
          }
        }
       }
       if(NIVEL==3)
       {
        txtDescripcion = "<color=#5dade2><b>Improvisar Flechas III</b></color>\n\n"; 
        txtDescripcion += "<i>El Explorador crea 1 flecha por AP restante +1, termina el Turno. Además obtiene +2 Probabilidad de crítico por 1 turno.</i>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM}</color>\n\n";

         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Opción A: +1 Flecha Recuperada. </color>\n";
             txtDescripcion += $"<color=#dfea02>-Opción B: +1 Turno Duración Buff.</color>\n";
          }
          }
        }
       }
       if(NIVEL==4)
       {
        txtDescripcion = "<color=#5dade2><b>Improvisar Flechas IVa</b></color>\n\n"; 
        txtDescripcion += "<i>El Explorador crea 1 flecha por AP restante +2, termina el Turno. Además obtiene +2 Probabilidad de crítico por 1 turno.</i>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM}</color>\n\n";

       }
       if(NIVEL==5)
       {
        txtDescripcion = "<color=#5dade2><b>Improvisar Flechas IVb</b></color>\n\n"; 
        txtDescripcion += "<i>El Explorador crea 1 flecha por AP restante +1, termina el Turno. Además obtiene +2 Probabilidad de crítico por 2 turnos.</i>\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM}</color>\n\n";

       }



    }

    Casilla Origen;
    public override void Activar()
    {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

    BattleManager.Instance.scUIContadorAP.MarcarCirculos((int)scEstaUnidad.ObtenerAPActual());       
        
        //AplicarEfectosHabilidad(scEstaUnidad, 0);
        
    }
    
    

    public async override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {
      cooldownActual = cooldownMax;
      
      
       BattleManager.Instance.EscribirLog($"{scEstaUnidad.uNombre} usa {nombre}");

       int APusados = (int)scEstaUnidad.ObtenerAPActual();
       int flechasCreadas = 0;
      
        for (int veces = 0; veces < APusados; veces++)
        {
           flechasCreadas++;
        }
        if( NIVEL > 1)
        {
         flechasCreadas++;
        }
        if( NIVEL == 4)
        {
         flechasCreadas++;
        }
        
        Usuario.GetComponent<ClaseExplorador>().Cantidad_flechas += flechasCreadas;

        VFXAplicar(Usuario);
       scEstaUnidad.EstablecerAPActualA(0);
       /////////////////////////////////////////////
       //BUFF ---- Así se aplica un buff/debuff
       Buff buff = new Buff();
       buff.buffNombre = "Flechas Preparadas";
       buff.boolfDebufftBuff = true;
       buff.DuracionBuffRondas = 2;
       buff.cantCritDado += 1;
       if( NIVEL > 2)
       {
         buff.cantCritDado += 1;
       }
        if( NIVEL == 5)
       {
         buff.cantDanioPorcentaje += 15;
       }
       buff.AplicarBuff(scEstaUnidad);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(buff, scEstaUnidad.gameObject);
       

      

    }
    void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_ImprovisarFlechas");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   canvasObjeto.overrideSorting = true;
   canvasObjeto.sortingOrder =  200;  

    }

    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private void ObtenerObjetivos()
    {
      lObjetivosPosibles.Add(scEstaUnidad);

      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva = new List<Unidad>(lObjetivosPosibles);

      foreach(Unidad uni in BattleManager.Instance.lUnidadesPosiblesHabilidadActiva)
      {
            uni.Marcar(1);
      }
     
    }
      
         
   

   
    

 
}
