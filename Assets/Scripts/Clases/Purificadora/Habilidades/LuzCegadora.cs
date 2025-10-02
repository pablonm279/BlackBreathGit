using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class LuzCegadora : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano

   
     public override void  Awake()
    {
      nombre = "Luz Cegadora";
      IDenClase = 4;
      costoAP = 4;
      costoPM = 1;
      if(NIVEL == 4){costoPM--;}
      
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = true;
      enArea = 0;
      esforzable = 1;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 3; 
      bAfectaObstaculos = false;

      targetEspecial = 6; 

      bonusAtaque +=0; //0
      XdDanio = 0;
      daniodX = 0; 
      tipoDanio = 11; //Divino
     

      imHab = Resources.Load<Sprite>("imHab/Purificadora_LuzCegadora");
      
     

      
    }

    public override void ActualizarDescripcion()
    {
      if(NIVEL<2)
      {
        txtDescripcion = "<color=#5dade2><b>Luz Cegadora I</b></color>\n\n"; 
        txtDescripcion += "<i>La Purificadora emite una luz divina en zona que ciega a enemigos y daña a seres impuros.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8>Cegar: -3 Ataque, -2 Defensa, - Reflejos; <color=#ea0606>Tirada Salvación Reflejos: DC:9 + Poder </color> - Daño: Divino 2d6 a Nomuertos y Etereos- </color>\n";
        txtDescripcion += $"<color=#44d3ec>-Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";

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
      if(NIVEL== 2)
      {
       txtDescripcion = "<color=#5dade2><b>Luz Cegadora II</b></color>\n\n"; 
        txtDescripcion += "<i>La Purificadora emite una luz divina en zona que ciega a enemigos y daña a seres impuros.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8>Cegar: -3 Ataque, -2 Defensa, - Reflejos; <color=#ea0606>Tirada Salvación Reflejos: DC:10 + Poder </color> - Daño: Divino 2d6 a Nomuertos y Etereos- </color>\n";
        txtDescripcion += $"<color=#44d3ec>-Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";

  
    
       if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1d6 daño</color>\n\n";
          }
          }
        }
      }
      if(NIVEL== 3)
      {
       txtDescripcion = "<color=#5dade2><b>Luz Cegadora III</b></color>\n\n"; 
        txtDescripcion += "<i>La Purificadora emite una luz divina en zona que ciega a enemigos y daña a seres impuros.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8>Cegar: -3 Ataque, -2 Defensa, - Reflejos; <color=#ea0606>Tirada Salvación Reflejos: DC:10 + Poder </color> - Daño: Divino 3d6 a Nomuertos y Etereos- </color>\n";
        txtDescripcion += $"<color=#44d3ec>-Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";

  
      if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Opción A: -1 costo Valentía</color>\n";
             txtDescripcion += $"<color=#dfea02>-Opción B: Hace 1/3 daño a todos los otros enemigos. </color>\n";
          }
          }
        }

      }
      if(NIVEL== 4)
      {
        txtDescripcion = "<color=#5dade2><b>Luz Cegadora IV a</b></color>\n\n"; 
        txtDescripcion += "<i>La Purificadora emite una luz divina en zona que ciega a enemigos y daña a seres impuros.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8>Cegar: -3 Ataque, -2 Defensa, - Reflejos; <color=#ea0606>Tirada Salvación Reflejos: DC:10 + Poder </color> - Daño: Divino 3d6 a Nomuertos y Etereos- </color>\n";
        txtDescripcion += $"<color=#44d3ec>-Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM-1} </color>\n\n";
      }
      if(NIVEL== 5)
      {
        txtDescripcion = "<color=#5dade2><b>Luz Cegadora IV b</b></color>\n\n"; 
        txtDescripcion += "<i>La Purificadora emite una luz divina en zona que ciega a enemigos y daña a seres impuros.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8>Cegar: -3 Ataque, -2 Defensa, - Reflejos; <color=#ea0606>Tirada Salvación Reflejos: DC:10 + Poder </color> - Daño: Divino 3d6 a Nomuertos y Etereos- 1d6 al Resto </color>\n";
        txtDescripcion += $"<color=#44d3ec>-Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";
        }



    }

    Casilla Origen;
    public override void Activar()
    { 
        seTiroFlechaVFX = false;
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();
        
      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

        
    }
    
    
    bool seTiroFlechaVFX = false;
    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {
    
     if(obj is Unidad) //Acá van los efectos a Unidades.
     { 
      
        Unidad objetivo = (Unidad)obj;
        float dificultadAtributo = 9+scEstaUnidad.mod_CarPoder;
        if(NIVEL > 1){dificultadAtributo++;}
        VFXAplicar(objetivo.gameObject);
      if (objetivo.inmunidad_Ceguera)
      {
        objetivo.GenerarTextoFlotante("Inmune", Color.red);
      }
      else if (objetivo.TiradaSalvacion(objetivo.mod_TSReflejos, dificultadAtributo)) //Si la tirada de salvacion es mayor a la tirada del usuario, no se aplica el efecto
      {

        /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Ciego";
        buff.boolfDebufftBuff = false;
        buff.DuracionBuffRondas = 2;
        buff.cantAtaque -= 3;
        buff.cantDefensa -= 2;
        buff.cantTsReflejos -= 1;
        buff.AplicarBuff(objetivo);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);


      }

         float damDivino = UnityEngine.Random.Range(2, 12)+scEstaUnidad.mod_CarPoder;
         if(NIVEL > 2){damDivino += UnityEngine.Random.Range(1, 7);}

        if(objetivo.TieneTag("Nomuerto") || objetivo.TieneTag("Etereo"))
        {
           
            objetivo.RecibirDanio(damDivino,11, false,scEstaUnidad); 

        }
        else
        {
           if(NIVEL == 5)
           {
             objetivo.RecibirDanio(damDivino/3, tipoDanio, false,scEstaUnidad); 
           }
        }



     }
    }
  
  
      void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_LuzCegadora");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity);
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
