using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class ArrojarAbrojos : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] int usosBatalla;
  
    
      public override void  Awake()
    {
      nombre = "Arrojar Abrojos";
      IDenClase = 8;
      costoAP = 2;
      costoPM = 0;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0; 
      esforzable = 1;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 4;
      bAfectaObstaculos = false;
      poneTrampas = true;
      poneObstaculo = false;
      
      targetEspecial = 7; 
      esDiscreta = true; //No quita sigilo
     
      
      imHab = Resources.Load<Sprite>("imHab/Acechador_ArrojarAbrojos");
      ActualizarDescripcion();
    }
    public override void ActualizarDescripcion()
    {
       if(NIVEL<2)
       {
        txtDescripcion = "<color=#5dade2><b>Arrojar Abrojos I</b></color>\n\n"; 
       
        txtDescripcion += "<i>Arroja abrojos en la casilla objetivo y en las diagonales inmediatas que no tengan enemigos.</i>\n";
        txtDescripcion += "<i>Cada enemigo que se mueva a las casillas afectadas sufrirá daño perforante 2d8 y Sangrado 2.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Si el enemigo supera una tirada de Reflejos DC 11, evita el sangrado y reduce el daño a la mitad.</b>  </color>\n\n";
        txtDescripcion += $"<color=#44d3ec>-Discreta - Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} \n</color>\n\n";

         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +2 Daño</color>\n\n";
          }
          }
        }
       }
       if(NIVEL==2)
       {
        txtDescripcion = "<color=#5dade2><b>Arrojar Abrojos II</b></color>\n\n"; 

        txtDescripcion += "<i>Arroja abrojos en la casilla objetivo y en las diagonales inmediatas que no tengan enemigos.</i>\n";
        txtDescripcion += "<i>Cada enemigo que se mueva a las casillas afectadas sufrirá daño perforante 2d8 +2 y Sangrado 2.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Si el enemigo supera una tirada de Reflejos DC 11, evita el sangrado y reduce el daño a la mitad.</b>  </color>\n\n";
        txtDescripcion += $"<color=#44d3ec>-Discreta - Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} \n</color>\n\n";


         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 DC</color>\n\n";
          }
          }
        }
       }
       if(NIVEL==3)
       {
        txtDescripcion = "<color=#5dade2><b>Arrojar Abrojos III</b></color>\n\n"; 

        txtDescripcion += "<i>Arroja abrojos en la casilla objetivo y en las diagonales inmediatas que no tengan enemigos.</i>\n";
        txtDescripcion += "<i>Cada enemigo que se mueva a las casillas afectadas sufrirá daño perforante 2d8 +2 y Sangrado 2.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Si el enemigo supera una tirada de Reflejos DC 12, evita el sangrado y reduce el daño a la mitad.</b>  </color>\n\n";
        txtDescripcion += $"<color=#44d3ec>-Discreta - Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} \n</color>\n\n";

         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Opción A: +1 Sangrado. </color>\n";
             txtDescripcion += $"<color=#dfea02>-Opción B: Saca 1 Ap al enemigo.</color>\n";
          }
          }
        }
       }
       if(NIVEL==4)
       {
        txtDescripcion = "<color=#5dade2><b>Arrojar Abrojos IVa</b></color>\n\n"; 

        txtDescripcion += "<i>Arroja abrojos en la casilla objetivo y en las diagonales inmediatas que no tengan enemigos.</i>\n";
        txtDescripcion += "<i>Cada enemigo que se mueva a las casillas afectadas sufrirá daño perforante 2d8 +2 y Sangrado 3.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Si el enemigo supera una tirada de Reflejos DC 12, evita el sangrado y reduce el daño a la mitad.</b>  </color>\n\n";
        txtDescripcion += $"<color=#44d3ec>-Discreta - Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} \n</color>\n\n";

       }
       if(NIVEL==5)
       {
        txtDescripcion = "<color=#5dade2><b>Arrojar Abrojos IVb</b></color>\n\n"; 

        txtDescripcion += "<i>Arroja abrojos en la casilla objetivo y en las diagonales inmediatas que no tengan enemigos.</i>\n";
        txtDescripcion += "<i>Cada enemigo que se mueva a las casillas afectadas sufrirá daño perforante 2d8 +2, Sangrado 2 y perderá 1 AP.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Si el enemigo supera una tirada de Reflejos DC 12, evita el sangrado y reduce el daño a la mitad.</b>  </color>\n\n";
        txtDescripcion += $"<color=#44d3ec>-Discreta - Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} \n</color>\n\n";

       }



    }
    void Start()
    {
      
    }

    Casilla Origen;
    public override void Activar()
    {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

    BattleManager.Instance.scUIContadorAP.MarcarCirculos((int)scEstaUnidad.ObtenerAPActual());
        
    }
    
    

    public async override void AplicarEfectosHabilidad(object obj, int tirada, Casilla cas)
    {
      
      List<Casilla> CasillasXcas = new List<Casilla>();

    CasillasXcas.Add(cas); // Agregar la casilla original
    foreach (Casilla c in BattleManager.Instance.lCasillasTotal)
    {

      if (c != null && cas != null)
      {
        if (c.Presente != null)
        {
          continue; // Si la casilla  tiene presente, no la agregamos
        }
        if (c != cas && (cas.lado == c.lado))
        {

          if (c.posX + 1 == cas.posX && (c.posY == cas.posY - 1 || c.posY == cas.posY + 1))
          {
            CasillasXcas.Add(c);
          }

          if (c.posX - 1 == cas.posX && (c.posY == cas.posY - 1 || c.posY == cas.posY + 1))
          {
            CasillasXcas.Add(c);
          }

        }
      }
    }

    foreach (Casilla c in CasillasXcas)
      {
        c.AddComponent<Abrojo>();
        c.GetComponent<Abrojo>().InicializarCreador(scEstaUnidad, NIVEL);

      }
    


       BattleManager.Instance.HabilidadActiva = null;// desactiva la habilidad activa, para que no se pueda usar de nuevo
    }
    void VFXAplicar(GameObject objetivo)
    {
       //GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation); 

    }

    //Provisorio
    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();

    private void ObtenerObjetivos()
    {
      
     //Cualquier objetivo en 1 de alcance 3 de ancho
      lObjetivosPosibles.Clear();
      lCasillasafectadas.Clear();
      
      lCasillasafectadas = Origen.ObtenerCasillasRango(3,2);
    
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
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());
           }
          

        }
        else
        {
           if(c.Presente.GetComponent<Unidad>() == null && c.Presente.GetComponent<Obstaculo>())
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

      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
       
    
        



      
    
         
    }

   
    

 
}
