using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class PrimerosAuxilios : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] int usosBatalla;
  
    
     public override void  Awake()
    {
      nombre = "Primeros Auxilios";
      IDenClase = 4;
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
      cooldownMax = 0;
      bAfectaObstaculos = false;
      
     
      usosBatalla = 2;

      imHab = Resources.Load<Sprite>("imHab/Caballero_PrimerosAuxilios");
      ActualizarDescripcion();
    }
    public override void ActualizarDescripcion()
    {
      if (TRADU.i.nIdioma == 1) // Español
      {
        if(NIVEL<2)
        {
          txtDescripcion = "<color=#5dade2><b>Primeros Auxilios I</b></color>\n\n"; 
          txtDescripcion += "<i>El Caballero utiliza sus conocimientos de primeros auxilios para curarse a si mismo o a un aliado cercano.</i>\n";
          txtDescripcion += "<i>A resguardo: si hay un aliado en una columna más frontal que el Caballero, cura un 30% más.</i>\n";
          txtDescripcion += "<i>La cantidad a curar depende de sus AP disponibles, termina el turno.</i>\n\n";
          txtDescripcion += $"<color=#c8c8c8><b>Alcance: 1 Efectos: Cura 1+ 1d4 por AP. Remueve Sangrado y Veneno.</b>  </color>\n\n";
          txtDescripcion += $"<color=#44d3ec>-Curación no mágica. Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} \n- 2 usos por Batalla</color>\n\n";
          if (EsEscenaCampaña())
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
            {
              if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Próximo Nivel: cura 1d6 por AP</color>\n\n";
              }
            }
          }
        }
        if(NIVEL==2)
        {
          txtDescripcion = "<color=#5dade2><b>Primeros Auxilios II</b></color>\n\n"; 
          txtDescripcion += "<i>El Caballero utiliza sus conocimientos de primeros auxilios para curarse a si mismo o a un aliado cercano.</i>\n";
          txtDescripcion += "<i>A resguardo: si hay un aliado en una columna más frontal que el Caballero, cura un 30% más.</i>\n";
          txtDescripcion += "<i>La cantidad a curar depende de sus AP disponibles, termina el turno.</i>\n\n";
          txtDescripcion += $"<color=#c8c8c8><b>Alcance: 1 Efectos: Cura 1+ 1d6 por AP. Remueve Sangrado y Veneno.</b>  </color>\n\n";
          txtDescripcion += $"<color=#44d3ec>-Curación no mágica. Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} \n- 2 usos por Batalla</color>\n\n";
          if (EsEscenaCampaña())
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
            {
              if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Uso por Batalla</color>\n\n";
              }
            }
          }
        }
        if(NIVEL==3)
        {
          txtDescripcion = "<color=#5dade2><b>Primeros Auxilios III</b></color>\n\n"; 
          txtDescripcion += "<i>El Caballero utiliza sus conocimientos de primeros auxilios para curarse a si mismo o a un aliado cercano.</i>\n";
          txtDescripcion += "<i>A resguardo: si hay un aliado en una columna más frontal que el Caballero, cura un 30% más.</i>\n";
          txtDescripcion += "<i>La cantidad a curar depende de sus AP disponibles, termina el turno.</i>\n\n";
          txtDescripcion += $"<color=#c8c8c8><b>Alcance: 1 Efectos: Cura 1+ 1d6 por AP. Remueve Sangrado y Veneno.</b>  </color>\n\n";
          txtDescripcion += $"<color=#44d3ec>-Curación no mágica. Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} \n- 3 usos por Batalla</color>\n\n";
          if (EsEscenaCampaña())
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
            {
              if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Opción A: si hay un aliado en una columna mas frontal que el Caballero, cura un 40% más. </color>\n";
                txtDescripcion += $"<color=#dfea02>-Opción B: La Curación se traslada a la Campaña</color>\n";
              }
            }
          }
        }
        if(NIVEL==4)
        {
          txtDescripcion = "<color=#5dade2><b>Primeros Auxilios IV a</b></color>\n\n"; 
          txtDescripcion += "<i>El Caballero utiliza sus conocimientos de primeros auxilios para curarse a si mismo o a un aliado cercano.</i>\n";
          txtDescripcion += "<i>A resguardo: si hay un aliado en una columna más frontal que el Caballero, cura un 40% más.</i>\n";
          txtDescripcion += "<i>La cantidad a curar depende de sus AP disponibles, termina el turno.</i>\n\n";
          txtDescripcion += $"<color=#c8c8c8><b>Alcance: 1 Efectos: Cura 1+ 1d6 por AP. Remueve Sangrado y Veneno.</b>  </color>\n\n";
          txtDescripcion += $"<color=#44d3ec>-Curación no mágica. Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} \n- 3 usos por Batalla</color>";
        }
        if(NIVEL==5)
        {
          txtDescripcion = "<color=#5dade2><b>Primeros Auxilios IV a</b></color>\n\n"; 
          txtDescripcion += "<i>El Caballero utiliza sus conocimientos de primeros auxilios para curarse a si mismo o a un aliado cercano.</i>\n";
          txtDescripcion += "<i>A resguardo: si hay un aliado en una columna más frontal que el Caballero, cura un 30% más.</i>\n";
          txtDescripcion += "<i>La cantidad a curar depende de sus AP disponibles, termina el turno. La curación se traslada a la campaña.</i>\n\n";
          txtDescripcion += $"<color=#c8c8c8><b>Alcance: 1 Efectos: Cura 1+ 1d6 por AP. Remueve Sangrado y Veneno.</b>  </color>\n\n";
          txtDescripcion += $"<color=#44d3ec>-Curación no mágica. Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} \n- 3 usos por Batalla</color>";
        }
      }
      if (TRADU.i.nIdioma == 2) // Inglés
      {
        if(NIVEL<2)
        {
          txtDescripcion = "<color=#5dade2><b>First Aid I</b></color>\n\n"; 
          txtDescripcion += "<i>The Knight uses his first aid knowledge to heal himself or a nearby ally.</i>\n";
          txtDescripcion += "<i>In cover: if there is an ally in a column ahead of the Knight, heals 30% more.</i>\n";
          txtDescripcion += "<i>The amount healed depends on available AP, ends the turn.</i>\n\n";
          txtDescripcion += $"<color=#c8c8c8><b>Range: 1 Effects: Heals 1+ 1d4 per AP. Removes Bleeding and Poison.</b>  </color>\n\n";
          txtDescripcion += $"<color=#44d3ec>-Non-magical healing. Cooldown: {cooldownMax} \n- AP Cost: {costoAP} \n- Val Cost: {costoPM} \n- 2 uses per Battle</color>\n\n";
          if (EsEscenaCampaña())
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
            {
              if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Next Level: heals 1d6 per AP</color>\n\n";
              }
            }
          }
        }
        if(NIVEL==2)
        {
          txtDescripcion = "<color=#5dade2><b>First Aid II</b></color>\n\n"; 
          txtDescripcion += "<i>The Knight uses his first aid knowledge to heal himself or a nearby ally.</i>\n";
          txtDescripcion += "<i>In cover: if there is an ally in a column ahead of the Knight, heals 30% more.</i>\n";
          txtDescripcion += "<i>The amount healed depends on available AP, ends the turn.</i>\n\n";
          txtDescripcion += $"<color=#c8c8c8><b>Range: 1 Effects: Heals 1+ 1d6 per AP. Removes Bleeding and Poison.</b>  </color>\n\n";
          txtDescripcion += $"<color=#44d3ec>-Non-magical healing. Cooldown: {cooldownMax} \n- AP Cost: {costoAP} \n- Val Cost: {costoPM} \n- 2 uses per Battle</color>\n\n";
          if (EsEscenaCampaña())
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
            {
              if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Next Level: +1 Use per Battle</color>\n\n";
              }
            }
          }
        }
        if(NIVEL==3)
        {
          txtDescripcion = "<color=#5dade2><b>First Aid III</b></color>\n\n"; 
          txtDescripcion += "<i>The Knight uses his first aid knowledge to heal himself or a nearby ally.</i>\n";
          txtDescripcion += "<i>In cover: if there is an ally in a column ahead of the Knight, heals 30% more.</i>\n";
          txtDescripcion += "<i>The amount healed depends on available AP, ends the turn.</i>\n\n";
          txtDescripcion += $"<color=#c8c8c8><b>Range: 1 Effects: Heals 1+ 1d6 per AP. Removes Bleeding and Poison.</b>  </color>\n\n";
          txtDescripcion += $"<color=#44d3ec>-Non-magical healing. Cooldown: {cooldownMax} \n- AP Cost: {costoAP} \n- Val Cost: {costoPM} \n- 3 uses per Battle</color>\n\n";
          if (EsEscenaCampaña())
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
            {
              if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Option A: if there is an ally in a column ahead of the Knight, heals 40% more. </color>\n";
                txtDescripcion += $"<color=#dfea02>-Option B: Healing transfers to Campaign</color>\n";
              }
            }
          }
        }
        if(NIVEL==4)
        {
          txtDescripcion = "<color=#5dade2><b>First Aid IV a</b></color>\n\n"; 
          txtDescripcion += "<i>The Knight uses his first aid knowledge to heal himself or a nearby ally.</i>\n";
          txtDescripcion += "<i>In cover: if there is an ally in a column ahead of the Knight, heals 40% more.</i>\n";
          txtDescripcion += "<i>The amount healed depends on available AP, ends the turn.</i>\n\n";
          txtDescripcion += $"<color=#c8c8c8><b>Range: 1 Effects: Heals 1+ 1d6 per AP. Removes Bleeding and Poison.</b>  </color>\n\n";
          txtDescripcion += $"<color=#44d3ec>-Non-magical healing. Cooldown: {cooldownMax} \n- AP Cost: {costoAP} \n- Val Cost: {costoPM} \n- 3 uses per Battle</color>";
        }
        if(NIVEL==5)
        {
          txtDescripcion = "<color=#5dade2><b>First Aid IV a</b></color>\n\n"; 
          txtDescripcion += "<i>The Knight uses his first aid knowledge to heal himself or a nearby ally.</i>\n";
          txtDescripcion += "<i>In cover: if there is an ally in a column ahead of the Knight, heals 30% more.</i>\n";
          txtDescripcion += "<i>The amount healed depends on available AP, ends the turn. Healing transfers to campaign.</i>\n\n";
          txtDescripcion += $"<color=#c8c8c8><b>Range: 1 Effects: Heals 1+ 1d6 per AP. Removes Bleeding and Poison.</b>  </color>\n\n";
          txtDescripcion += $"<color=#44d3ec>-Non-magical healing. Cooldown: {cooldownMax} \n- AP Cost: {costoAP} \n- Val Cost: {costoPM} \n- 3 uses per Battle</color>";
        }
      }
    }
    void Start()
    {
        if(NIVEL > 2)
        {
          usosBatalla++;
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

        
    }
    
    

    public async override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {
    
     if(obj is Unidad) //Acá van los efectos a Unidades.
     {
        
       Unidad objetivo = (Unidad)obj;
        VFXAplicar(objetivo.gameObject);
       BattleManager.Instance.EscribirLog($"{scEstaUnidad.uNombre} usa {nombre} en {objetivo.uNombre}");

       int APusados = (int)scEstaUnidad.ObtenerAPActual();

       float curacion = 1;

      if(NIVEL == 1) //Cura 1d4 por AP
      {
        for (int veces = 0; veces < APusados; veces++)
        {
            curacion += UnityEngine.Random.Range(1,5);

        }
      }
      else //Cura 1d6 por AP
      {
        for (int veces = 0; veces < APusados; veces++)
        {
            curacion += UnityEngine.Random.Range(1,7);

        }

      }

       if(ChequearSiHayAliadoAdelantado(objetivo))
       {
        
        if(NIVEL == 4)
        {
          curacion = curacion*1.4f;
        }
        else{ curacion = curacion*1.3f;}

       }

      scEstaUnidad.EstablecerAPActualA(0);
       
     
       if(objetivo.estado_sangrado > 0)
       {
         objetivo.estado_sangrado = 0;
        await objetivo.GenerarTextoFlotante("<s>" + "Sangrado" + "</s>", Color.red);
       }
       if(objetivo.estado_veneno > 0)
       {
         objetivo.estado_veneno = 0;
        await objetivo.GenerarTextoFlotante("<s>" + "Veneno" + "</s>", Color.green);
       }
        objetivo.RecibirCuracion(curacion, false);

       objetivo.Marcar(0);

       usosBatalla--;
       if(usosBatalla == 0)//Al gastarse los usos, se borra la habilidad
       {
        Destroy(this); 
        BattleManager.Instance.scUIBotonesHab.UIDesactivarHabilidades();
        BattleManager.Instance.scUIBotonesHab.ActualizarBotonesHabilidad();
       } 
      
     }   
   
    }
    bool ChequearSiHayAliadoAdelantado(Unidad obj)
    {
      int casX = Origen.posX;

      foreach(Casilla cas in BattleManager.Instance.lCasillasTotal)
      {
        if(cas.lado != Origen.lado){ continue;} //Si es del lado opuesto la descarta
        if(cas.posX <= Origen.posX){ continue;} //Si esta en la misma culomna o una mas atras la descarta

        if(cas.Presente != null)
        {
            if(cas.Presente.GetComponent<Unidad>() != null)
            {
               if(cas.Presente.GetComponent<Unidad>() != obj) //Si hay una unidad, y no es el objetivo de la habilidad, entonces devuelve SI
               {
                    return true;
               }

            }

        }
        

      }

      return false;
    }
    void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_PrimerosAuxilios");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
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
      
      lObjetivosPosibles.Clear();
     
      
      //Casillas Alrededor al origen
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasAlrededor(1);
      lCasillasafectadas.Add(Origen); //Agrega la casilla del caster, para que se pueda targetear
    
      foreach(Casilla c in lCasillasafectadas)
      {
         c.ActivarCapaColorAzul();
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
