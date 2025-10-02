using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;
using UnityEditor;

public class SifonArcano : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;

   
      public override void  Awake()
    {
      nombre = "Sifón Arcano";
      IDenClase = 7;
      costoAP = 3;
      costoPM = 1;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 1;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 5;
      bAfectaObstaculos = false;

      imHab = Resources.Load<Sprite>("imHab/Canalizador_SifonArcano");
     
    }

   public override void ActualizarDescripcion()
   {
      if(NIVEL < 2)
{
    txtDescripcion = "<color=#5dade2><b>Sifón Arcano I</b></color>\n\n";
    txtDescripcion += "<i>Marca al objetivo con un vínculo inestable que drena su vitalidad, amplificado por la presencia de Residuos Energéticos.</i>\n\n";
    txtDescripcion += $"<color=#c8c8c8><b>TS:</b> 7+Poder vs Fortaleza. - Dura 3 turnos.</color>\n";
    txtDescripcion += $"<color=#c8c8c8>Al final de cada turno del objetivo, recibe 1d6 daño arcano x ({1}+1 por cada Residuo Energético en juego).</color>\n";
    txtDescripcion += $"<color=#44d3ec>- Costo AP: 3\n- Costo Val: 1\n- Cooldown: {cooldownMax}</color>\n";
    txtDescripcion += $"<color=#44d3ec>- Si muere bajo este efecto: el Canalizador obtiene +1 AP permanente y +10% Daño.</color>\n\n";

    if (EsEscenaCampaña())
    {
        if(CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
                txtDescripcion += $"<color=#dfea02>- Próximo Nivel: +1 Daño por Residuo</color>\n\n";
            }
        }
    }
}

if(NIVEL == 2)
{
    txtDescripcion = "<color=#5dade2><b>Sifón Arcano II</b></color>\n\n";
    txtDescripcion += "<i>Marca al objetivo con un vínculo inestable que drena su vitalidad, amplificado por la presencia de Residuos Energéticos.</i>\n\n";
    txtDescripcion += $"<color=#c8c8c8><b>TS:</b> 7+Poder vs Fortaleza. - Dura 3 turnos.</color>\n";
    txtDescripcion += $"<color=#c8c8c8>Al final de cada turno del objetivo, recibe 1d6 daño arcano x ({2}+1 por cada Residuo Energético en juego).</color>\n";
    txtDescripcion += $"<color=#44d3ec>- Costo AP: 3\n- Costo Val: 1\n- Cooldown: {cooldownMax}</color>\n";
    txtDescripcion += $"<color=#44d3ec>- Si muere bajo este efecto: el Canalizador obtiene +1 AP permanente y +10% Daño.</color>\n\n";

    if (EsEscenaCampaña())
    {
        if(CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
                txtDescripcion += $"<color=#dfea02>- Próximo Nivel: +1 DC</color>\n\n";
            }
        }
    }
}

if(NIVEL == 3)
{
    txtDescripcion = "<color=#5dade2><b>Sifón Arcano III</b></color>\n\n";
    txtDescripcion += "<i>Marca al objetivo con un vínculo inestable que drena su vitalidad, amplificado por la presencia de Residuos Energéticos.</i>\n\n";
    txtDescripcion += $"<color=#c8c8c8><b>TS:</b> 8+Poder vs Fortaleza. - Dura 3 turnos.</color>\n";
    txtDescripcion += $"<color=#c8c8c8>Al final de cada turno del objetivo, recibe 1d6 daño arcano x ({2}+1 por cada Residuo Energético en juego).</color>\n";
    txtDescripcion += $"<color=#44d3ec>- Costo AP: 3\n- Costo Val: 1\n- Cooldown: {cooldownMax}</color>\n";
    txtDescripcion += $"<color=#44d3ec>- Si muere bajo este efecto: el Canalizador obtiene +1 AP permanente y +10% Daño.</color>\n\n";

    if (EsEscenaCampaña())
    {
        if(CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
                txtDescripcion += $"<color=#dfea02>- Próximo Nivel:\nOpción A: Al matar con el efecto, gana 1 Nivel de Energía\nOpción B: +1 Turno de duración</color>\n\n";
            }
        }
    }
}

if(NIVEL == 4)
{
    // Variante A
    txtDescripcion = "<color=#5dade2><b>Sifón Arcano IV a</b></color>\n\n";
    txtDescripcion += "<i>Marca al objetivo con un vínculo inestable que drena su vitalidad, amplificado por la presencia de Residuos Energéticos.</i>\n\n";
    txtDescripcion += $"<color=#c8c8c8><b>TS:</b> 8+Poder vs Fortaleza. - Dura 3 turnos.</color>\n";
    txtDescripcion += $"<color=#c8c8c8>Al final de cada turno del objetivo, recibe 1d6 daño arcano x ({2}+1 por cada Residuo Energético en juego).</color>\n";
    txtDescripcion += $"<color=#44d3ec>- Costo AP: 3\n- Costo Val: 1\n- Cooldown: {cooldownMax}</color>\n";
    txtDescripcion += $"<color=#44d3ec>- Si muere bajo este efecto: el Canalizador obtiene +1 AP permanente, +10% Daño y +1 Nivel de Energía.</color>\n\n";
}

if(NIVEL == 5)
{
    // Variante B
    txtDescripcion = "<color=#5dade2><b>Sifón Arcano IV b</b></color>\n\n";
    txtDescripcion += "<i>Marca al objetivo con un vínculo inestable que drena su vitalidad, amplificado por la presencia de Residuos Energéticos.</i>\n\n";
    txtDescripcion += $"<color=#c8c8c8><b>TS:</b> 8+Poder vs Fortaleza. - Dura 4 turnos.</color>\n";
    txtDescripcion += $"<color=#c8c8c8>Al final de cada turno del objetivo, recibe 1d6 daño arcano x ({2}+1 por cada Residuo Energético en juego).</color>\n";
    txtDescripcion += $"<color=#44d3ec>- Costo AP: 3\n- Costo Val: 1\n- Cooldown: {cooldownMax}</color>\n";
    txtDescripcion += $"<color=#44d3ec>- Si muere bajo este efecto: el Canalizador obtiene +1 AP permanente y +10% Daño.</color>\n\n";
}






   }

    Casilla Origen;
    public override void Activar()
    {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

        
    }
    
    

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {

    if (obj is Unidad uni) //Acá van los efectos a Unidades.
    {
      float dc = 7 + scEstaUnidad.mod_CarPoder; 
      VFXAplicar(uni.gameObject);
      if (NIVEL > 2) { dc += 1; }

      if (uni.TiradaSalvacion(uni.mod_TSFortaleza, dc))
      {

        //Agrega la reacción 
        ReaccionSifonArcano reaccion = new ReaccionSifonArcano();
        reaccion.variableUnidad = scEstaUnidad;
        reaccion.NIVEL = NIVEL;
        reaccion.nombre = "Sifón Arcano";
        reaccion.variableUnidad = scEstaUnidad;
        ReaccionSifonArcano reaccionPosturaDefensiva = ComponentCopier.CopyComponent(reaccion, uni.gameObject);


      }
                        
      }
     
    }
    
       void VFXAplicar(GameObject objetivo)
  {
    VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_SifonArcano");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   canvasObjeto.overrideSorting = true;
   canvasObjeto.sortingOrder =  200; 
            //---

  }

    //Provisorio
    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();

    private void ObtenerObjetivos()
    {
     
      lObjetivosPosibles.Clear();
    
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasLadoOpuesto();
    
    
      foreach(Casilla c in lCasillasafectadas)
      {
        c.ActivarCapaColorRojo();
        
        if(c.Presente == null)
        {
            continue;
        }
        
        if(!bAfectaObstaculos) //Si no afecta obstaculos se descarta la casilla si no hay unidad, si afecta obstaculo se descarta si tampoco hay obstaculo
        {
           if(c.Presente.GetComponent<Unidad>() == null)
           {
            continue;
           }
           
           if(c.Presente.GetComponent<Unidad>() != null)
           {
             c.ActivarCapaColorRojo();
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());
           }
          

        }
        else
        {
           if(c.Presente.GetComponent<Unidad>() == null && c.Presente.GetComponent<Obstaculo>() == null)
           {
            continue;
           }

           if(c.Presente.GetComponent<Unidad>() != null)
           {
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());;
           }

           if(c.Presente.GetComponent<Obstaculo>() != null)
           {
             lObstaculosPosibles.Add(c.Presente.GetComponent<Obstaculo>());;
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
