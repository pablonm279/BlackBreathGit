using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class CastigaraLosMalvados : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;

   
      public override void  Awake()
    {
      nombre = "Castigar a los Malvados";
      IDenClase = 8;
      costoAP = 2;
      costoPM = 1;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 5;
      if(NIVEL > 2){cooldownMax--;}
      bAfectaObstaculos = false;

      imHab = Resources.Load<Sprite>("imHab/Purificadora_CastigarMalvados");
     
    }

   public override void ActualizarDescripcion()
   {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
      var statsUI = ObtenerStatsDescripcionUI();

      int poderActual = statsUI.Poder;
      int dcBase = NIVEL > 1 ? 11 : 10;
      int usos = NIVEL == 4 ? 3 : 2;
      string fraccionDanio = NIVEL == 5 ? "1/2" : "1/3";
      string rangoDanioEs = FormatearRangoDados(1, 6);

      string tituloEs = "Castigar a los Malvados I";
      string tituloEn = "Punish the Wicked I";
      string tituloPt = "Castigar os Malvados I";
      if (NIVEL == 2) { tituloEs = "Castigar a los Malvados II"; tituloEn = "Punish the Wicked II"; }
      if (NIVEL == 3) { tituloEs = "Castigar a los Malvados III"; tituloEn = "Punish the Wicked III"; }
      if (NIVEL == 4) { tituloEs = "Castigar a los Malvados IV a"; tituloEn = "Punish the Wicked IV a"; }
      if (NIVEL == 5) { tituloEs = "Castigar a los Malvados IV b"; tituloEn = "Punish the Wicked IV b"; }
      if (NIVEL == 2) { tituloPt = "Castigar os Malvados II"; }
      if (NIVEL == 3) { tituloPt = "Castigar os Malvados III"; }
      if (NIVEL == 4) { tituloPt = "Castigar os Malvados IV a"; }
      if (NIVEL == 5) { tituloPt = "Castigar os Malvados IV b"; }

      string lineaSalvacionEs = ConstruirLineaSalvacion(false, TipoSalvacionDescripcion.Mental, dcBase, "Pod", "Power", poderActual, "Poder");
      string lineaSalvacionEn = ConstruirLineaSalvacion(true, TipoSalvacionDescripcion.Mental, dcBase, "Poder", "Power", poderActual);

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += "<b>Type:</b> Reactive mark\n";
        cuerpo += "<b>Target:</b> 1 enemy unit on the opposite side\n";
        cuerpo += "<b>Trigger:</b> Each time the marked unit damages one of your allies\n";
        cuerpo += lineaSalvacionEn + "\n";
        cuerpo += $"<b>On failed save:</b> Lose all remaining AP and take 1d6 + <color=#ea0606>Power ({poderActual})</color> + {fraccionDanio} of damage dealt | <b>Type:</b> Divine\n";
        cuerpo += $"<b>Duration:</b> Up to {usos} failed saves, or ends early if the target succeeds the save";
      }
      else if (esPortugues)
      {
        cuerpo += "<b>Tipo:</b> Marca reativa\n";
        cuerpo += "<b>Alvo:</b> 1 unidade inimiga do lado oposto\n";
        cuerpo += "<b>Gatilho:</b> Cada vez que a unidade marcada causar dano a um aliado seu\n";
        cuerpo += lineaSalvacionEs + "\n";
        cuerpo += $"<b>Se falhar na resistencia:</b> Perde todo AP restante e sofre 1d6 + <color=#ea0606>Poder ({poderActual})</color> + {fraccionDanio} do dano causado | <b>Tipo:</b> Divino\n";
        cuerpo += $"<b>Duracao:</b> Ate {usos} falhas na resistencia, ou termina antes se o alvo passar na resistencia";
      }
      else
      {
        cuerpo += "<b>Tipo:</b> Marca reactiva\n";
        cuerpo += "<b>Objetivo:</b> 1 unidad enemiga del lado opuesto\n";
        cuerpo += "<b>Disparo:</b> Cada vez que la unidad marcada daña a un aliado tuyo\n";
        cuerpo += lineaSalvacionEs + "\n";
        cuerpo += $"<b>Si falla TS:</b> Pierde todo su AP restante y recibe {rangoDanioEs} + <color=#ea0606>Pod ({poderActual})</color> + {fraccionDanio} del daño infligido | <b>Tipo:</b> Divino\n";
        cuerpo += $"<b>Duración:</b> Hasta {usos} fallos de TS, o termina antes si el objetivo supera la TS";
      }

      string costos = esIngles
        ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}"
        : esPortugues
          ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}";

      txtDescripcion = ConstruirDescripcionEstandar(
        esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
        esIngles
          ? "Curses a target with divine retribution after attacking an ally."
          : esPortugues
            ? "Amaldiçoa um alvo com retribuição divina após atacar um aliado."
          : "Maldice a un objetivo con retribución divina después de atacar a un aliado.",
        cuerpo,
        costos,
        "#5dade2");

      string colorEncabezado = "#44d3ec";
      string colorValor = "#ffffff";
      string colorPoder = "#2aa6c8";
      string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
      string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
      string iconoReaccion = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_reaccion\"></voffset></size><space=-0.35em>";
      string iconoDebuff = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_debuff\"></voffset></size><space=-0.35em>";
      string costoSuperior = cooldownMax > 0
        ? $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}"
        : $"{costoAP} {iconoAP}";
      string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
      string subtitulo = esIngles
        ? "Marks an enemy; damaging allies triggers Divine punishment."
        : esPortugues
          ? "Marca um inimigo; causar dano a aliados ativa punicao Divina."
          : "Marca a un enemigo; dañar aliados activa castigo Divino.";
      string danio = $"{rangoDanioEs} + <color={colorPoder}>{(esIngles ? "Power" : esPortugues ? "Poder" : "Poder")} ({poderActual})</color> + {fraccionDanio}";
      string cuerpoNuevo = "";
      if (esIngles)
      {
        cuerpoNuevo += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Reactive mark {iconoReaccion}</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Target:</b></color> <color={colorValor}>1 enemy on the opposite side</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Trigger:</b></color> <color={colorValor}>When the marked unit damages an ally</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Save:</b></color> <color={colorValor}>Mental vs DC {dcBase} + <color={colorPoder}>Power ({poderActual})</color></color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Failed save:</b></color> <color={colorValor}>{iconoDebuff} loses all remaining AP; {danio} of damage dealt. Type: Divine</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Duration:</b></color> <color={colorValor}>{usos} failed saves, or ends if target succeeds.</color>";
      }
      else if (esPortugues)
      {
        cuerpoNuevo += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Marca reativa {iconoReaccion}</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Alvo:</b></color> <color={colorValor}>1 inimigo do lado oposto</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Gatilho:</b></color> <color={colorValor}>Quando a unidade marcada causa dano a um aliado</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Resistencia:</b></color> <color={colorValor}>Mental vs DC {dcBase} + <color={colorPoder}>Poder ({poderActual})</color></color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Se falhar:</b></color> <color={colorValor}>{iconoDebuff} perde todo AP restante; {danio} do dano causado. Tipo: Divino</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Duracao:</b></color> <color={colorValor}>{usos} falhas, ou termina se o alvo passar.</color>";
      }
      else
      {
        cuerpoNuevo += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Marca reactiva {iconoReaccion}</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Objetivo:</b></color> <color={colorValor}>1 enemigo del lado opuesto</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Disparo:</b></color> <color={colorValor}>Cuando la unidad marcada daña a un aliado</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>TS:</b></color> <color={colorValor}>Mental vs DC {dcBase} + <color={colorPoder}>Poder ({poderActual})</color></color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Si falla:</b></color> <color={colorValor}>{iconoDebuff} pierde todo AP restante; {danio} del daño infligido. Tipo: Divino</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Duración:</b></color> <color={colorValor}>{usos} fallos de TS, o termina si el objetivo supera la TS.</color>";
      }

      txtDescripcion =
        $"<size=115%><color=#5dade2><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n" +
        $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n" +
        "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n" +
        cuerpoNuevo;

      bool mostrarProximoNivel = EsEscenaCampaña()
        && CampaignManager.Instance != null
        && CampaignManager.Instance.scMenuPersonajes != null
        && CampaignManager.Instance.scMenuPersonajes.pSel != null
        && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
      if (!mostrarProximoNivel)
      {
        return;
      }

      if (esIngles)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 save DC.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: -1 cooldown.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+1 use) or Option B (damage share to 1/2).</color>"; }
      }
      else if (esPortugues)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 CD de resistencia.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: -1 recarga.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+1 uso) ou Opcao B (proporcao de dano para 1/2).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 DC de salvación.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: -1 enfriamiento.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (+1 uso) u Opcion B (proporcion de dano a 1/2).</color>"; }
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

    if (obj is Unidad) //Acá van los efectos a Unidades.
    {

      Unidad objetivo = (Unidad)obj;

      BattleManager.Instance.EscribirLog(
        TRADU.i.Traducir(scEstaUnidad.uNombre) + " " +
        TRADU.i.Traducir("usa ") +
        TRADU.i.Traducir(nombre) + " -> " +
        TRADU.i.Traducir(objetivo.uNombre) + ".");
      VFXAplicar(objetivo.gameObject);
      //Agrega la reacción 
      ReaccionCastigarMalvados reaccion = new ReaccionCastigarMalvados();
      reaccion.variableUnidad = scEstaUnidad;
      reaccion.NIVEL = NIVEL;
      reaccion.nombre = "Castigar a los Malvados";
      ReaccionCastigarMalvados reaccionPosturaDefensiva = ComponentCopier.CopyComponent(reaccion, objetivo.gameObject);

      objetivo.GenerarTextoFlotante(TRADU.i.Traducir("Castigar a los Malvados"), Color.yellow);
      }
     
    }
    
  
     void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_CastigarMalvados");

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
      }

      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
       
    
        



      
         
    }

   
 
}




