using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class MarcarPresa : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;

   
     public override void  Awake()
    {
      nombre = "Marcar Presa";
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
      esHostil = true;
      cooldownMax = 5;
      bAfectaObstaculos = false;

      imHab = Resources.Load<Sprite>("imHab/Explorador_MarcarPresa");
      ActualizarDescripcion();
    }

        public override void ActualizarDescripcion()
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;

    string tituloEs = "Marcar Presa I";
    string tituloEn = "Mark Prey I";
    string tituloPt = "Marcar Presa I";
    if (NIVEL == 2) { tituloEs = "Marcar Presa II"; tituloEn = "Mark Prey II"; }
    if (NIVEL == 3) { tituloEs = "Marcar Presa III"; tituloEn = "Mark Prey III"; }
    if (NIVEL == 4) { tituloEs = "Marcar Presa IV a"; tituloEn = "Mark Prey IV a"; }
    if (NIVEL == 5) { tituloEs = "Marcar Presa IV b"; tituloEn = "Mark Prey IV b"; }
    if (NIVEL == 2) { tituloPt = "Marcar Presa II"; }
    if (NIVEL == 3) { tituloPt = "Marcar Presa III"; }
    if (NIVEL == 4) { tituloPt = "Marcar Presa IV a"; }
    if (NIVEL == 5) { tituloPt = "Marcar Presa IV b"; }

    int bonoAtaqueMarca = NIVEL == 4 ? 2 : 4;
    int bonoCritPorcentajeMarca = (1 + (NIVEL > 2 ? 1 : 0)) * 5;
    int bonoCritDanioMarca = 15 + (NIVEL > 1 ? 5 : 0);
    int recompensaVal = NIVEL == 5 ? 2 : 1;
    int recompensaApMax = NIVEL == 5 ? 2 : 1;
    int recompensaTsMental = NIVEL == 5 ? 3 : 2;
    bool aplicaPenalidadPropia = NIVEL != 4;

    string colorTitulo = "#5dade2";
    string colorEncabezado = "#44d3ec";
    string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
    string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
    string costoSuperior = $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}";

    if (esIngles)
    {
      string marca = TerminoDescripcion(TerminoDescripcionId.MarcaPresa, "Prey Mark", "Estado_marcado");
      string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crit", "critico");
      string valentia = TerminoDescripcion(TerminoDescripcionId.Valentia, "Valour", "Valentía");
      string ap = TerminoDescripcion(TerminoDescripcionId.PuntosAccion, "max AP", "ap");
      string salvacionMental = TerminoDescripcion(TerminoDescripcionId.SalvacionMental, "Mental Save", "ic_mental");
      string proximaMejora = null;
      if (DebeMostrarProximaMejoraDescripcion())
      {
        if (NIVEL < 2) { proximaMejora = "+5% critical damage against the marked target."; }
        else if (NIVEL == 2) { proximaMejora = "+5% Crit against the marked target."; }
        else if (NIVEL == 3) { proximaMejora = "Option A: removes the penalty against other targets.\nOption B: +1 Valour, +1 max AP, and +1 Mental Save to the kill reward."; }
      }

      var lineas = new List<LineaDescripcionNormalizada>
      {
        LineaDescripcion("Target", "1 enemy"),
        LineaDescripcion("Effect", $"Applies {marca} for 3 turns."),
        LineaDescripcion("Against target", $"+{bonoAtaqueMarca} Attack Roll, +{bonoCritPorcentajeMarca}% {critico}, +{bonoCritDanioMarca}% critical damage.", 1)
      };
      if (aplicaPenalidadPropia)
      {
        lineas.Add(LineaDescripcion("Penalty", "-2 to Attack Rolls against other targets (2 turns).", 1));
      }
      lineas.Add(LineaDescripcion("On marked kill", $"+{recompensaVal} {valentia}, +{recompensaApMax} {ap}, +{recompensaTsMental} {salvacionMental} (3 turns).", 1));
      txtDescripcion = ConstruirDescripcionNormalizadaIngles(
        tituloEn,
        "Marks one enemy and improves attacks against it.",
        lineas,
        proximaMejora);
      return;
    }

    if (esPortugues)
    {
      string marca = TerminoDescripcion(TerminoDescripcionId.MarcaPresa, "Marca de Presa", "Estado_marcado");
      string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crítico", "critico");
      string valentia = TerminoDescripcion(TerminoDescripcionId.Valentia, "Valentia", "Valentía");
      string ap = TerminoDescripcion(TerminoDescripcionId.PuntosAccion, "AP máximo", "ap");
      string resistenciaMental = TerminoDescripcion(TerminoDescripcionId.SalvacionMental, "Resistência Mental", "ic_mental");
      string proximaMejora = null;
      if (DebeMostrarProximaMejoraDescripcion())
      {
        if (NIVEL < 2) { proximaMejora = "Próximo nível: +5% de dano crítico contra o alvo marcado."; }
        else if (NIVEL == 2) { proximaMejora = $"Próximo nível: +5% de {critico} contra o alvo marcado."; }
        else if (NIVEL == 3) { proximaMejora = "Próximo nível: Opção A: remove a penalidade contra outros alvos.\nOpção B: +1 Valentia, +1 AP máximo e +1 Resistência Mental na recompensa por abate."; }
      }
      var lineas = new List<LineaDescripcionNormalizada>
      {
        LineaDescripcion("Alvo", "1 inimigo"),
        LineaDescripcion("Efeito", $"Aplica {marca} por 3 turnos."),
        LineaDescripcion("Contra o alvo", $"+{bonoAtaqueMarca} na Rolagem de ataque, +{bonoCritPorcentajeMarca}% {critico}, +{bonoCritDanioMarca}% de dano crítico.", 1)
      };
      if (aplicaPenalidadPropia)
      {
        lineas.Add(LineaDescripcion("Penalidade", "-2 nas Rolagens de ataque contra outros alvos (2 turnos).", 1));
      }
      lineas.Add(LineaDescripcion("Ao abater o marcado", $"+{recompensaVal} {valentia}, +{recompensaApMax} {ap}, +{recompensaTsMental} {resistenciaMental} (3 turnos).", 1));
      txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(
        tituloPt,
        "Marca um inimigo e melhora os ataques contra ele.",
        lineas,
        proximaMejora);
      return;
    }

    {
      string marca = TerminoDescripcion(TerminoDescripcionId.MarcaPresa, "Marca de Presa", "Estado_marcado");
      string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crítico", "critico");
      string valentia = TerminoDescripcion(TerminoDescripcionId.Valentia, "Valentía", "Valentía");
      string ap = TerminoDescripcion(TerminoDescripcionId.PuntosAccion, "AP máximo", "ap");
      string salvacionMental = TerminoDescripcion(TerminoDescripcionId.SalvacionMental, "TS Mental", "ic_mental");
      string proximaMejora = null;
      if (DebeMostrarProximaMejoraDescripcion())
      {
        if (NIVEL < 2) { proximaMejora = "Próximo nivel: +5% de daño crítico contra el objetivo marcado."; }
        else if (NIVEL == 2) { proximaMejora = $"Próximo nivel: +5% de {critico} contra el objetivo marcado."; }
        else if (NIVEL == 3) { proximaMejora = "Próximo nivel: Opción A: elimina la penalización contra otros objetivos.\nOpción B: +1 Valentía, +1 AP máximo y +1 TS Mental a la recompensa por baja."; }
      }
      var lineas = new List<LineaDescripcionNormalizada>
      {
        LineaDescripcion("Objetivo", "1 enemigo"),
        LineaDescripcion("Efecto", $"Aplica {marca} durante 3 turnos."),
        LineaDescripcion("Contra el objetivo", $"+{bonoAtaqueMarca} a la Tirada de ataque, +{bonoCritPorcentajeMarca}% {critico}, +{bonoCritDanioMarca}% de daño crítico.", 1)
      };
      if (aplicaPenalidadPropia)
      {
        lineas.Add(LineaDescripcion("Penalización", "-2 a las Tiradas de ataque contra otros objetivos (2 turnos).", 1));
      }
      lineas.Add(LineaDescripcion("Al matar al marcado", $"+{recompensaVal} {valentia}, +{recompensaApMax} {ap}, +{recompensaTsMental} {salvacionMental} (3 turnos).", 1));
      txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(
        tituloEs,
        "Marca a un enemigo y mejora los ataques contra él.",
        lineas,
        proximaMejora);
      return;
    }

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> Mark\n";
      cuerpo += $"<color={colorEncabezado}><b>Target:</b></color> 1 enemy\n";
      cuerpo += $"<color={colorEncabezado}><b>Mark duration:</b></color> 3 turns\n";
      cuerpo += $"<color={colorEncabezado}><b>Against marked target:</b></color> roll +{bonoAtaqueMarca}, +{bonoCritPorcentajeMarca}% Crit, +{bonoCritDanioMarca}% crit damage\n";
      cuerpo += aplicaPenalidadPropia
        ? $"<color={colorEncabezado}><b>After cast:</b></color> -2 on rolls against non-marked targets for 2 turns\n"
        : $"<color={colorEncabezado}><b>After cast:</b></color> no penalty against non-marked targets\n";
      cuerpo += $"<color={colorEncabezado}><b>On marked kill:</b></color> +{recompensaVal} Valour, +{recompensaApMax} max AP, +{recompensaTsMental} Mental Save for 3 turns";
    }
    else if (esPortugues)
    {
      cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Marca\n";
      cuerpo += $"<color={colorEncabezado}><b>Alvo:</b></color> 1 inimigo\n";
      cuerpo += $"<color={colorEncabezado}><b>Duracao da marca:</b></color> 3 turnos\n";
      cuerpo += $"<color={colorEncabezado}><b>Contra marcado:</b></color> rolagem +{bonoAtaqueMarca}, +{bonoCritPorcentajeMarca}% Critico, +{bonoCritDanioMarca}% dano critico\n";
      cuerpo += aplicaPenalidadPropia
        ? $"<color={colorEncabezado}><b>Depois de usar:</b></color> -2 em rolagens contra alvos nao marcados por 2 turnos\n"
        : $"<color={colorEncabezado}><b>Depois de usar:</b></color> sem penalidade contra alvos nao marcados\n";
      cuerpo += $"<color={colorEncabezado}><b>Ao matar o marcado:</b></color> +{recompensaVal} Valentía, +{recompensaApMax} AP max, +{recompensaTsMental} TS Mental por 3 turnos";
    }
    else
    {
      cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Marca\n";
      cuerpo += $"<color={colorEncabezado}><b>Objetivo:</b></color> 1 enemigo\n";
      cuerpo += $"<color={colorEncabezado}><b>Duración de marca:</b></color> 3 turnos\n";
      cuerpo += $"<color={colorEncabezado}><b>Contra marcado:</b></color> tirada +{bonoAtaqueMarca}, +{bonoCritPorcentajeMarca}% Crítico, +{bonoCritDanioMarca}% daño crítico\n";
      cuerpo += aplicaPenalidadPropia
        ? $"<color={colorEncabezado}><b>Despues de lanzar:</b></color> -2 en tiradas contra no marcados por 2 turnos\n"
        : $"<color={colorEncabezado}><b>Despues de lanzar:</b></color> sin penalidad contra no marcados\n";
      cuerpo += $"<color={colorEncabezado}><b>Al matar al marcado:</b></color> +{recompensaVal} Valentía, +{recompensaApMax} AP max, +{recompensaTsMental} TS Mental por 3 turnos";
    }

    string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
    string subtitulo = esIngles
      ? "Focus one enemy and improves your attacks against it."
      : esPortugues
        ? "Marca um inimigo e melhora seus ataques contra ele."
        : "Marca a un enemigo y mejora sus ataques contra el.";

    txtDescripcion = $"<size=115%><color={colorTitulo}><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n";
    txtDescripcion += $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n";
    txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
    txtDescripcion += cuerpo;

    bool mostrarProximoNivel = CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
    if (!mostrarProximoNivel)
    {
      return;
    }

    if (esIngles)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +5% crit damage against marked target.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +5% Crit against marked target.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A removes the self penalty; Option B improves kill reward (+1 Valour, +1 max AP, +1 Mental Save).</color>"; }
    }
    else if (esPortugues)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +5% de dano critico contra marcado.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +5% Crítico contra marcado.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A remove a penalidade propria; Opcao B melhora a recompensa por morte (+1 Valentia, +1 AP max, +1 TS Mental).</color>"; }
    }
    else
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +5% al daño crítico contra marcado.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +5% Crítico contra marcado.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A elimina la penalidad propia; Opción B mejora la recompensa por muerte (+1 Valentía, +1 AP max, +1 TS Mental).</color>"; }
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

          BattleManager.Instance.EscribirLog(TRADU.i.Traducir(scEstaUnidad.uNombre) + " " + TRADU.i.Traducir("usa ") + TRADU.i.Traducir(nombre) + " -> " + TRADU.i.Traducir(objetivo.uNombre) + ".");

          MarcaMarcarPresa marca = new MarcaMarcarPresa();
          marca.nombre = "Presa Marcada";
          marca.quienMarco = scEstaUnidad;
          marca.NIVEL = NIVEL;
          marca.duracion = 3;

          MarcaMarcarPresa buffComponent = ComponentCopier.CopyComponent(marca, objetivo.gameObject);
          objetivo.Marcar(0);

          objetivo.GenerarTextoFlotante(TRADU.i.Traducir("Marcado"), Color.yellow);

                        
      }
      
      cooldownActual = cooldownMax;
    scEstaUnidad.CambiarAPActual(-costoAP); 

      if(NIVEL != 4) // a Nivel IVa, no recibe el debuff
      {
        /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Marcando Presa";
        buff.boolfDebufftBuff = false;
        buff.DuracionBuffRondas = 2;
        buff.cantAtaque -= 2;
        buff.AplicarBuff(scEstaUnidad);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, scEstaUnidad.gameObject);
      }
    }
    
      void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_MarcarPresa");

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






