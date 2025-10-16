using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class HombroConHombro : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
   
   
      public override void  Awake()
    {
    

      nombre = "HombroConHombro";
      IDenClase = 10;
      costoAP = 2;
      costoPM = 2;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = true;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 4;
      bAfectaObstaculos = false;
      

       imHab = Resources.Load<Sprite>("imHab/Caballero_HombroconHombro");

        ActualizarDescripcion();
    
    }
    public override void ActualizarDescripcion()
    {
      if (TRADU.i.nIdioma == 1) // Español
      {
        if(NIVEL<2)
        {
          txtDescripcion = "<color=#5dade2><b>Hombro con Hombro I</b></color>\n\n"; 
          txtDescripcion += "<i>El Caballero se posiciona junto a sus aliados y los impulsa a luchar eficazmente.</i>\n\n";
          txtDescripcion += $"<i>Buff a aliados adyacentes y adyacentes a ellos en su misma columna: +2 Defensa y Ataque por 3 Turnos. +1 Valentía a cada uno.</i>\n\n";
          txtDescripcion += $"<i>Se cancela si alguno se mueve.</i>\n\n";
          txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} </color>\n\n";

          if (EsEscenaCampaña())
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
            {
              if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Defensa</color>\n\n";
              }
            }
          }
        }
        if(NIVEL==2)
        {
          txtDescripcion = "<color=#5dade2><b>Hombro con Hombro II</b></color>\n\n"; 
          txtDescripcion += "<i>El Caballero se posiciona junto a sus aliados y los impulsa a luchar eficazmente.</i>\n\n";
          txtDescripcion += $"<i>Buff a aliados adyacentes y adyacentes a ellos en su misma columna: +3 Defensa y +2 Ataque por 3 Turnos. +1 Valentía a cada uno.</i>\n\n";
          txtDescripcion += $"<i>Se cancela si alguno se mueve.</i>\n\n";
          txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} </color>\n\n";

          if (EsEscenaCampaña())
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
            {
              if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Ataque</color>\n\n";
              }
            }
          }
        }
        if(NIVEL==3)
        {
          txtDescripcion = "<color=#5dade2><b>Hombro con Hombro III</b></color>\n\n"; 
          txtDescripcion += "<i>El Caballero se posiciona junto a sus aliados y los impulsa a luchar eficazmente.</i>\n\n";
          txtDescripcion += $"<i>Buff a aliados adyacentes y adyacentes a ellos en su misma columna: +3 Defensa y Ataque por 3 Turnos. +1 Valentía a cada uno.</i>\n\n";
          txtDescripcion += $"<i>Se cancela si alguno se mueve.</i>\n\n";
          txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} </color>\n\n";

          if (EsEscenaCampaña())
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
            {
              if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Opción A: Otorga Invulnerabilidad por 1 Turno</color>\n";
                txtDescripcion += $"<color=#dfea02>-Opción B: Otorga +1 AP</color>\n";
              }
            }
          }
        }
        if(NIVEL==4)
        {
          txtDescripcion = "<color=#5dade2><b>Hombro con Hombro IV a</b></color>\n\n"; 
          txtDescripcion += "<i>El Caballero se posiciona junto a sus aliados y los impulsa a luchar eficazmente.</i>\n\n";
          txtDescripcion += $"<i>Buff a aliados adyacentes y adyacentes a ellos en su misma columna: +3 Defensa y +2 Ataque por 3 Turnos. +1 Valentía a cada uno.</i>\n";
          txtDescripcion += $"<i>Invulnerables el primer turno.</i>\n\n";
          txtDescripcion += $"<i>Se cancela si alguno se mueve.</i>\n\n";
          txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} </color>";
        }
        if(NIVEL==5)
        {
          txtDescripcion = "<color=#5dade2><b>Hombro con Hombro IV b</b></color>\n\n"; 
          txtDescripcion += "<i>El Caballero se posiciona junto a sus aliados y los impulsa a luchar eficazmente.</i>\n\n";
          txtDescripcion += $"<i>Buff a aliados adyacentes y adyacentes a ellos en su misma columna: +3 Defensa y Ataque por 3 Turnos. +1 Valentía y AP a cada uno.</i>\n\n";
          txtDescripcion += $"<i>Se cancela si alguno se mueve.</i>\n\n";
          txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} </color>";
        }
      }
      if (TRADU.i.nIdioma == 2) // Inglés
      {
        if(NIVEL<2)
        {
          txtDescripcion = "<color=#5dade2><b>Shoulder to Shoulder I</b></color>\n\n"; 
          txtDescripcion += "<i>The Knight stands beside his allies and inspires them to fight effectively.</i>\n\n";
          txtDescripcion += $"<i>Buff to adjacent allies and those adjacent to them in the same column: +2 Defense and Attack for 3 Turns. +1 Valor to each.</i>\n\n";
          txtDescripcion += $"<i>Cancels if any move.</i>\n\n";
          txtDescripcion += $"<color=#44d3ec>- Cooldown: {cooldownMax} \n- AP Cost: {costoAP} \n- Valor Cost: {costoPM} </color>\n\n";

          if (EsEscenaCampaña())
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
            {
              if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Next Level: +1 Defense</color>\n\n";
              }
            }
          }
        }
        if(NIVEL==2)
        {
          txtDescripcion = "<color=#5dade2><b>Shoulder to Shoulder II</b></color>\n\n"; 
          txtDescripcion += "<i>The Knight stands beside his allies and inspires them to fight effectively.</i>\n\n";
          txtDescripcion += $"<i>Buff to adjacent allies and those adjacent to them in the same column: +3 Defense and +2 Attack for 3 Turns. +1 Valor to each.</i>\n\n";
          txtDescripcion += $"<i>Cancels if any move.</i>\n\n";
          txtDescripcion += $"<color=#44d3ec>- Cooldown: {cooldownMax} \n- AP Cost: {costoAP} \n- Valor Cost: {costoPM} </color>\n\n";

          if (EsEscenaCampaña())
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
            {
              if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Next Level: +1 Attack</color>\n\n";
              }
            }
          }
        }
        if(NIVEL==3)
        {
          txtDescripcion = "<color=#5dade2><b>Shoulder to Shoulder III</b></color>\n\n"; 
          txtDescripcion += "<i>The Knight stands beside his allies and inspires them to fight effectively.</i>\n\n";
          txtDescripcion += $"<i>Buff to adjacent allies and those adjacent to them in the same column: +3 Defense and Attack for 3 Turns. +1 Valor to each.</i>\n\n";
          txtDescripcion += $"<i>Cancels if any move.</i>\n\n";
          txtDescripcion += $"<color=#44d3ec>- Cooldown: {cooldownMax} \n- AP Cost: {costoAP} \n- Valor Cost: {costoPM} </color>\n\n";

          if (EsEscenaCampaña())
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
            {
              if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Option A: Grants Invulnerability for 1 Turn</color>\n";
                txtDescripcion += $"<color=#dfea02>-Option B: Grants +1 AP</color>\n";
              }
            }
          }
        }
        if(NIVEL==4)
        {
          txtDescripcion = "<color=#5dade2><b>Shoulder to Shoulder IV a</b></color>\n\n"; 
          txtDescripcion += "<i>The Knight stands beside his allies and inspires them to fight effectively.</i>\n\n";
          txtDescripcion += $"<i>Buff to adjacent allies and those adjacent to them in the same column: +3 Defense and +2 Attack for 3 Turns. +1 Valor to each.</i>\n";
          txtDescripcion += $"<i>Invulnerable the first turn.</i>\n\n";
          txtDescripcion += $"<i>Cancels if any move.</i>\n\n";
          txtDescripcion += $"<color=#44d3ec>- Cooldown: {cooldownMax} \n- AP Cost: {costoAP} \n- Valor Cost: {costoPM} </color>";
        }
        if(NIVEL==5)
        {
          txtDescripcion = "<color=#5dade2><b>Shoulder to Shoulder IV b</b></color>\n\n"; 
          txtDescripcion += "<i>The Knight stands beside his allies and inspires them to fight effectively.</i>\n\n";
          txtDescripcion += $"<i>Buff to adjacent allies and those adjacent to them in the same column: +3 Defense and Attack for 3 Turns. +1 Valor and AP to each.</i>\n\n";
          txtDescripcion += $"<i>Cancels if any move.</i>\n\n";
          txtDescripcion += $"<color=#44d3ec>- Cooldown: {cooldownMax} \n- AP Cost: {costoAP} \n- Valor Cost: {costoPM} </color>";
        }
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

    if(obj is Unidad) //Acá van los efectos a Unidades.
     {

      Unidad objetivo = (Unidad)obj;
        VFXAplicar(objetivo.gameObject);
       /////////////////////////////////////////////
       //BUFF ---- Así se aplica un buff/debuff
       Buff buff = new Buff();
       buff.buffNombre = "Hombro Con Hombro";
       buff.boolfDebufftBuff = true;
       buff.DuracionBuffRondas = 3;
       buff.cantDefensa += 2;
       buff.cantAtaque += 2;
       if(NIVEL > 1){  buff.cantDefensa += 1;}
       if(NIVEL > 2){  buff.cantAtaque += 1;}
       if(NIVEL == 4){  objetivo.estado_invulnerable += 1;}
       if(NIVEL == 5){  buff.cantAPMax += 1;}
       buff.AplicarBuff(objetivo);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);

       objetivo.Marcar(0);

       objetivo.SumarValentia(1);
     }
    
    
    }
    
    void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_HombroConHombro");

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

      // Rellena la lista de casillas afectadas de la habilidad (no una variable local)
      lCasillasafectadas.Clear();
      lCasillasafectadas.AddRange(Origen.ObtenerCasillasAdyacentesEnColumna());
      lCasillasafectadas.Add(Origen);

      // Marca visualmente las casillas válidas para el clic de confirmación
      foreach (Casilla cas in lCasillasafectadas)
      {
        cas.ActivarCapaColorAzul();
      }

      foreach(Casilla c in lCasillasafectadas)
      {
        
        
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
