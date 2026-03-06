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
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      var statsUI = ObtenerStatsDescripcionUI();

      int poderActual = statsUI.Poder;
      int dcBase = NIVEL > 1 ? 10 : 9;
      bool agregaD6Divino = NIVEL > 2;
      bool afectaOtrosEnemigos = NIVEL == 5;

      string tituloEs = "Luz Cegadora I";
      string tituloEn = "Blinding Light I";
      if (NIVEL == 2) { tituloEs = "Luz Cegadora II"; tituloEn = "Blinding Light II"; }
      if (NIVEL == 3) { tituloEs = "Luz Cegadora III"; tituloEn = "Blinding Light III"; }
      if (NIVEL == 4) { tituloEs = "Luz Cegadora IV a"; tituloEn = "Blinding Light IV a"; }
      if (NIVEL == 5) { tituloEs = "Luz Cegadora IV b"; tituloEn = "Blinding Light IV b"; }

      string lineaSalvacionEs = ConstruirLineaSalvacion(false, TipoSalvacionDescripcion.Reflejos, dcBase, "Poder", "Power", poderActual);
      string lineaSalvacionEn = ConstruirLineaSalvacion(true, TipoSalvacionDescripcion.Reflejos, dcBase, "Poder", "Power", poderActual);

      string danioPrincipalEs = agregaD6Divino
        ? $"1d10 + 1 + 1d6 + <color=#ea0606>Poder ({poderActual})</color> | <b>Tipo:</b> Divino"
        : $"1d10 + 1 + <color=#ea0606>Poder ({poderActual})</color> | <b>Tipo:</b> Divino";
      string danioPrincipalEn = agregaD6Divino
        ? $"1d10 + 1 + 1d6 + <color=#ea0606>Power ({poderActual})</color> | <b>Type:</b> Divine"
        : $"1d10 + 1 + <color=#ea0606>Power ({poderActual})</color> | <b>Type:</b> Divine";

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += "<b>Type:</b> Ranged (3 range)\n";
        cuerpo += "<b>Target:</b> Frontal area (2 width)\n";
        cuerpo += lineaSalvacionEn + "\n";
        cuerpo += "<b>On failed save and if not immune to Blind:</b> Blinded for 2 rounds (-3 Attack, -2 Defense, -1 Reflex)\n";
        cuerpo += $"<b>Damage vs Undead/Ethereal:</b> {danioPrincipalEn}";
        if (afectaOtrosEnemigos)
        {
          cuerpo += "\n<b>Other enemies:</b> receive 1/3 of the rolled Divine damage";
        }
      }
      else
      {
        cuerpo += "<b>Tipo:</b> Rango (3 alcance)\n";
        cuerpo += "<b>Objetivo:</b> Area frontal (2 ancho)\n";
        cuerpo += lineaSalvacionEs + "\n";
        cuerpo += "<b>Si falla TS y no es inmune a Ceguera:</b> Ciego por 2 rondas (-3 Ataque, -2 Defensa, -1 Reflejos)\n";
        cuerpo += $"<b>Danio vs Nomuerto/Etereo:</b> {danioPrincipalEs}";
        if (afectaOtrosEnemigos)
        {
          cuerpo += "\n<b>Otros enemigos:</b> reciben 1/3 del danio Divino tirado";
        }
      }

      string costos = esIngles
        ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}\n- Effortable: Yes ({esforzable})"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}\n- Esforzable: Si ({esforzable})";

      txtDescripcion = ConstruirDescripcionEstandar(
        esIngles ? tituloEn : tituloEs,
        esIngles
          ? "The Purifier unleashes divine radiance that hinders enemies and burns impure targets."
          : "La Purificadora desata una radiancia divina que debilita enemigos y quema objetivos impuros.",
        cuerpo,
        costos,
        "#5dade2");

      bool mostrarProximoNivel = EsEscenaCampaña() && CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
      if (!mostrarProximoNivel)
      {
        return;
      }

      if (esIngles)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 save DC.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1d6 Divine damage vs Undead/Ethereal.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (-1 Valour Cost) or Option B (1/3 damage to other enemies).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 DC de salvacion.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1d6 de danio Divino vs Nomuerto/Etereo.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (-1 costo de Valentía) u Opcion B (1/3 de danio a otros enemigos).</color>"; }
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
        objetivo.GenerarTextoFlotante(TRADU.i.Traducir("Inmune"), Color.red);
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
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);  

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





