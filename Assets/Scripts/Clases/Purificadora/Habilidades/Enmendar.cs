using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class Enmendar : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    
    
    public override void  Awake()
    {
      nombre = "Enmendar";
      IDenClase = 3;
      costoAP = 3;
      costoPM = 1;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 2;
      bAfectaObstaculos = false;
      
     
      
      imHab = Resources.Load<Sprite>("imHab/Purificadora_Enmendar");
      ActualizarDescripcion();

      requiereRecurso = 1; //esto es para que el boton no se active al apretar si no tiene X recursos (ej Flecha). Ver en BotonHabilidad.
      if(NIVEL == 4){requiereRecurso = 0;}
    }
    public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
      var statsUI = ObtenerStatsDescripcionUI();

      int poderActual = statsUI.Poder;
      int alcance = 4;
      int bonusPlano = NIVEL > 2 ? 3 : (NIVEL > 1 ? 1 : 0);
      bool consumeFervor = NIVEL != 4;
      int fervorActual = 0;
      ClasePurificadora scPurificadora = Usuario != null ? Usuario.GetComponent<ClasePurificadora>() : null;
      if (scPurificadora != null)
      {
        fervorActual = scPurificadora.ObtenerFervor();
      }

      string tituloEs = "Enmendar I";
      string tituloEn = "Mend I";
      string tituloPt = "Remendar I";
      if (NIVEL == 2) { tituloEs = "Enmendar II"; tituloEn = "Mend II"; }
      if (NIVEL == 3) { tituloEs = "Enmendar III"; tituloEn = "Mend III"; }
      if (NIVEL == 4) { tituloEs = "Enmendar IV a"; tituloEn = "Mend IV a"; }
      if (NIVEL == 5) { tituloEs = "Enmendar IV b"; tituloEn = "Mend IV b"; }
      if (NIVEL == 2) { tituloPt = "Remendar II"; }
      if (NIVEL == 3) { tituloPt = "Remendar III"; }
      if (NIVEL == 4) { tituloPt = "Remendar IV a"; }
      if (NIVEL == 5) { tituloPt = "Remendar IV b"; }

      string bonusPlanoTexto = bonusPlano > 0 ? $" + {bonusPlano}" : "";
      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += $"<b>Type:</b> Ranged ({alcance} range)\n";
        cuerpo += "<b>Target:</b> 1 unit in range\n";
        cuerpo += $"<b>Heal:</b> Random 4-18{bonusPlanoTexto} + <color=#ea0606>Power ({poderActual})</color> + Fervor ({fervorActual})\n";
        cuerpo += "<b>Healing Type:</b> Magical healing\n";
        cuerpo += "<b>Requirement:</b> Needs at least 1 Fervor to activate\n";
        cuerpo += consumeFervor
          ? "<b>On cast:</b> Consumes 1 Fervor"
          : "<b>On cast:</b> Does not consume Fervor";
      }
      else if (esPortugues)
      {
        cuerpo += $"<b>Tipo:</b> Alcance ({alcance} de alcance)\n";
        cuerpo += "<b>Alvo:</b> 1 unidade no alcance\n";
        cuerpo += $"<b>Cura:</b> Aleatorio 4-18{bonusPlanoTexto} + <color=#ea0606>Poder ({poderActual})</color> + Fervor ({fervorActual})\n";
        cuerpo += "<b>Tipo de cura:</b> Cura magica\n";
        cuerpo += "<b>Requisito:</b> Precisa de pelo menos 1 Fervor para ativar\n";
        cuerpo += consumeFervor
          ? "<b>Ao usar:</b> Consome 1 Fervor"
          : "<b>Ao usar:</b> Nao consome Fervor";
      }
      else
      {
        cuerpo += $"<b>Tipo:</b> Rango ({alcance} alcance)\n";
        cuerpo += "<b>Objetivo:</b> 1 unidad en rango\n";
        cuerpo += $"<b>Curacion:</b> Aleatorio 4-18{bonusPlanoTexto} + <color=#ea0606>Pod ({poderActual})</color> + Fervor ({fervorActual})\n";
        cuerpo += "<b>Tipo de curacion:</b> Curacion magica\n";
        cuerpo += "<b>Requisito:</b> Necesita al menos 1 Fervor para activarse\n";
        cuerpo += consumeFervor
          ? "<b>Al lanzar:</b> Consume 1 Fervor"
          : "<b>Al lanzar:</b> No consume Fervor";
      }

      string costos = esIngles
        ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}"
        : esPortugues
          ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}";

      txtDescripcion = ConstruirDescripcionEstandar(
        esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
        esIngles
          ? "A restorative spell that scales with current Fervor and Power."
          : esPortugues
            ? "Uma magia restauradora que escala com o Fervor atual e o Poder."
          : "Un hechizo restaurador que escala con el Fervor actual y el Poder.",
        cuerpo,
        costos,
        "#5dade2");

      string colorEncabezado = "#44d3ec";
      string colorValor = "#ffffff";
      string colorPoder = "#2aa6c8";
      string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
      string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
      string costoSuperior = cooldownMax > 0
        ? $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}"
        : $"{costoAP} {iconoAP}";
      string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
      string subtitulo = esIngles
        ? "Heals one ally, scaling with Power and current Fervor."
        : esPortugues
          ? "Cura um aliado, escalando com Poder e Fervor atual."
          : "Cura a un aliado, escalando con Poder y Fervor actual.";
      string curacion = $"4-18{bonusPlanoTexto} + <color={colorPoder}>{(esIngles ? "Power" : esPortugues ? "Poder" : "Poder")} ({poderActual})</color> + Fervor ({fervorActual})";
      string cuerpoNuevo = "";
      if (esIngles)
      {
        cuerpoNuevo += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Ranged heal ({alcance} range)</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Target:</b></color> <color={colorValor}>1 unit in range</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Heal:</b></color> <color={colorValor}>{curacion}. Type: Magical healing</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Requirement:</b></color> <color={colorValor}>{(consumeFervor ? "Requires 1+ Fervor." : "No Fervor required.")}</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>On cast:</b></color> <color={colorValor}>{(consumeFervor ? "Consumes 1 Fervor." : "Does not consume Fervor.")}</color>";
      }
      else if (esPortugues)
      {
        cuerpoNuevo += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Cura a alcance ({alcance} de alcance)</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Alvo:</b></color> <color={colorValor}>1 unidade no alcance</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Cura:</b></color> <color={colorValor}>{curacion}. Tipo: Cura magica</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Requisito:</b></color> <color={colorValor}>{(consumeFervor ? "Requer 1+ Fervor." : "Nao requer Fervor.")}</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Ao usar:</b></color> <color={colorValor}>{(consumeFervor ? "Consome 1 Fervor." : "Nao consome Fervor.")}</color>";
      }
      else
      {
        cuerpoNuevo += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Curacion a rango ({alcance} alcance)</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Objetivo:</b></color> <color={colorValor}>1 unidad en rango</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Curacion:</b></color> <color={colorValor}>{curacion}. Tipo: Curacion magica</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Requisito:</b></color> <color={colorValor}>{(consumeFervor ? "Requiere 1+ Fervor." : "No requiere Fervor.")}</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Al lanzar:</b></color> <color={colorValor}>{(consumeFervor ? "Consume 1 Fervor." : "No consume Fervor.")}</color>";
      }

      txtDescripcion =
        $"<size=115%><color=#5dade2><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n" +
        $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n" +
        "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n" +
        cuerpoNuevo;

      bool mostrarProximoNivel = EsEscenaCampaña() && CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
      if (!mostrarProximoNivel)
      {
        return;
      }

      if (esIngles)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 flat healing.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +2 flat healing.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (no Fervor consumption) or Option B (keeps Fervor consumption).</color>"; }
      }
      else if (esPortugues)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 cura plana.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +2 cura plana.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (sem consumo de Fervor) ou Opcao B (mantem consumo de Fervor).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 curacion plana.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +2 curacion plana.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (sin consumo de Fervor) u Opcion B (mantiene consumo de Fervor).</color>"; }
      }
    }
    void Start()
    {
       

    }

    Casilla Origen;
    public override void Activar()
    {
       if(Usuario.GetComponent<ClasePurificadora>().ObtenerFervor() > 0)
       {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

    BattleManager.Instance.scUIContadorAP.MarcarCirculos((int)scEstaUnidad.ObtenerAPActual());       }
        
    }
    
    

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {
    
     if(obj is Unidad) //Acá van los efectos a Unidades.
     {
       
      
       Unidad objetivo = (Unidad)obj;
       VFXAplicar(objetivo.gameObject);
      
       BattleManager.Instance.EscribirLog(TRADU.i.Traducir(scEstaUnidad.uNombre) + " " + TRADU.i.Traducir("usa ") + TRADU.i.Traducir(nombre) + " -> " + TRADU.i.Traducir(objetivo.uNombre) + ".");
  
       int random = UnityEngine.Random.Range(4, 19);
       float curacion = random+scEstaUnidad.mod_CarPoder+Usuario.GetComponent<ClasePurificadora>().ObtenerFervor();
       if(NIVEL > 1){curacion++;}
       if(NIVEL > 2){curacion+= 2;}
       if(NIVEL > 5){curacion+= Usuario.GetComponent<ClasePurificadora>().ObtenerFervor()*2;}
       
     
       objetivo.RecibirCuracion(curacion, true);

       if(NIVEL != 4){  Usuario.GetComponent<ClasePurificadora>().CambiarFervor(-1);}
     


       objetivo.Marcar(0);

      
      
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
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_Enmendar");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
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
      
      lObjetivosPosibles.Clear();
     
      
      //Casillas Alrededor al origen
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasAlrededor(4);
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
      }

      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
       
    
        



      
         
    }

   
    

 
}





