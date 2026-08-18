using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class GritoMotivador : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
      
     public override void  Awake()
    {
      nombre = "Grito Motivador";
      IDenClase = 2;
      costoAP = 2;
      costoPM = 0;
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

      imHab = Resources.Load<Sprite>("imHab/Caballero_GritoMotivador");

       
      ActualizarDescripcion();
    
    }

    void ActualizarParametrosPorNivel()
    {
      cooldownMax = NIVEL >= 3 ? 3 : 4;
    }

    public override void ActualizarDescripcion()
    {
      ActualizarParametrosPorNivel();
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;

      int buffDanio = NIVEL > 1 ? 15 : 10;
      int valorAliados = 1;
      bool afectaEnemigos = NIVEL == 5;
      int duracionDebuffEnemigos = 1;

      string tituloEs = "Grito Motivador I";
      string tituloEn = "War Cry I";
      string tituloPt = "Grito Motivador I";
      if (NIVEL == 2) { tituloEs = "Grito Motivador II"; tituloEn = "War Cry II"; }
      if (NIVEL == 3) { tituloEs = "Grito Motivador III"; tituloEn = "War Cry III"; }
      if (NIVEL == 4) { tituloEs = "Grito Motivador IV a"; tituloEn = "War Cry IV a"; }
      if (NIVEL == 5) { tituloEs = "Grito Motivador IV b"; tituloEn = "War Cry IV b"; }
      if (NIVEL == 2) { tituloPt = "Grito Motivador II"; }
      if (NIVEL == 3) { tituloPt = "Grito Motivador III"; }
      if (NIVEL == 4) { tituloPt = "Grito Motivador IV a"; }
      if (NIVEL == 5) { tituloPt = "Grito Motivador IV b"; }

      if (esIngles)
      {
        string valentia = TerminoDescripcion(TerminoDescripcionId.Valentia, "Valour", "Valentía");
        string proximaMejora = null;
        if (DebeMostrarProximaMejoraDescripcion())
        {
          if (NIVEL < 2) { proximaMejora = "+5% allied damage."; }
          else if (NIVEL == 2) { proximaMejora = "Cooldown decreases from 4 to 3."; }
          else if (NIVEL == 3) { proximaMejora = "Option A: +2 Valour per affected ally to the Knight.\nOption B: enemies deal -10% damage for 1 turn."; }
        }

        var lineas = new List<LineaDescripcionNormalizada>
        {
          LineaDescripcion("Target", afectaEnemigos ? "All units" : "All allies"),
          LineaDescripcion("Effect", $"Allies gain +{buffDanio}% Damage (3 turns)."),
          LineaDescripcion("Other allies", $"Gain +{valorAliados} {valentia}.", 1)
        };
        if (NIVEL == 4)
        {
          lineas.Add(LineaDescripcion("Knight", $"Gains +2 {valentia} per affected ally.", 1));
        }
        if (afectaEnemigos)
        {
          lineas.Add(LineaDescripcion("Enemies", $"Deal -10% Damage ({duracionDebuffEnemigos} turn); no save.", 1));
        }

        txtDescripcion = ConstruirDescripcionNormalizadaIngles(
          tituloEn,
          "A commanding shout that empowers allies.",
          lineas,
          proximaMejora);
        return;
      }

      if (esPortugues)
      {
        string valentia = TerminoDescripcion(TerminoDescripcionId.Valentia, "Valentia", "Valentía");
        string valentiaSinIcono = TerminoDescripcion(TerminoDescripcionId.Valentia, "Valentia");
        string proximaMejora = null;
        if (DebeMostrarProximaMejoraDescripcion())
        {
          if (NIVEL < 2) { proximaMejora = "Próximo nível: +5% de dano dos aliados."; }
          else if (NIVEL == 2) { proximaMejora = "Próximo nível: a recarga diminui de 4 para 3."; }
          else if (NIVEL == 3) { proximaMejora = "Opção A: o Cavaleiro recebe +2 de Valentia por aliado afetado.\nOpção B: os inimigos causam -10% de dano por 1 turno."; }
        }
        var lineas = new List<LineaDescripcionNormalizada>
        {
          LineaDescripcion("Alvo", afectaEnemigos ? "Todas as unidades" : "Todos os aliados"),
          LineaDescripcion("Efeito", $"Aliados recebem +{buffDanio}% de Dano (3 turnos)."),
          LineaDescripcion("Outros aliados", $"Recebem +{valorAliados} de {valentia}.", 1)
        };
        if (NIVEL == 4) { lineas.Add(LineaDescripcion("Cavaleiro", $"Recebe +2 de {valentiaSinIcono} por aliado afetado.", 1)); }
        if (afectaEnemigos) { lineas.Add(LineaDescripcion("Inimigos", $"Causam -10% de Dano ({duracionDebuffEnemigos} turno); sem salvaguarda.", 1)); }
        txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(tituloPt, "Um grito de comando que fortalece os aliados.", lineas, proximaMejora);
        return;
      }

      {
        string valentia = TerminoDescripcion(TerminoDescripcionId.Valentia, "Valentía", "Valentía");
        string valentiaSinIcono = TerminoDescripcion(TerminoDescripcionId.Valentia, "Valentía");
        string proximaMejora = null;
        if (DebeMostrarProximaMejoraDescripcion())
        {
          if (NIVEL < 2) { proximaMejora = "Próximo nivel: +5% de daño de los aliados."; }
          else if (NIVEL == 2) { proximaMejora = "Próximo nivel: el enfriamiento disminuye de 4 a 3."; }
          else if (NIVEL == 3) { proximaMejora = "Opción A: el Caballero obtiene +2 de Valentía por aliado afectado.\nOpción B: los enemigos infligen -10% de daño durante 1 turno."; }
        }
        var lineas = new List<LineaDescripcionNormalizada>
        {
          LineaDescripcion("Objetivo", afectaEnemigos ? "Todas las unidades" : "Todos los aliados"),
          LineaDescripcion("Efecto", $"Los aliados obtienen +{buffDanio}% de Daño (3 turnos)."),
          LineaDescripcion("Otros aliados", $"Obtienen +{valorAliados} de {valentia}.", 1)
        };
        if (NIVEL == 4) { lineas.Add(LineaDescripcion("Caballero", $"Obtiene +2 de {valentiaSinIcono} por aliado afectado.", 1)); }
        if (afectaEnemigos) { lineas.Add(LineaDescripcion("Enemigos", $"Infligen -10% de Daño ({duracionDebuffEnemigos} turno); sin salvación.", 1)); }
        txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(tituloEs, "Un grito de mando que fortalece a los aliados.", lineas, proximaMejora);
        return;
      }

      string colorEncabezado = "#44d3ec";
      string colorValor = "#ffffff";
      string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
      string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
      string iconoBuff = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_buff\"></voffset></size><space=-0.35em>";
      string iconoDebuff = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_debuff\"></voffset></size><space=-0.35em>";
      string costoSuperior = cooldownMax > 0
        ? $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}"
        : $"{costoAP} {iconoAP}";

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += "<b>Type:</b> Support\n";
        cuerpo += "<b>Target:</b> All allied units on your side\n";
        cuerpo += $"<b>Allied buff:</b> +{buffDanio}% Damage for 3 turns\n";
        if (valorAliados > 0)
        {
          cuerpo += $"<b>Allied bonus:</b> +{valorAliados} Valour to other allies\n";
        }
        if (NIVEL == 4)
        {
          cuerpo += "<b>Self bonus:</b> +2 Valour per affected ally\n";
        }
        if (afectaEnemigos)
        {
          cuerpo += "<b>Enemy effect:</b> -10% Damage for 1 turn (no save)";
        }
      }
      else if (esPortugues)
      {
        cuerpo += "<b>Tipo:</b> Suporte\n";
        cuerpo += "<b>Alvo:</b> Todas as unidades aliadas do seu lado\n";
        cuerpo += $"<b>Buff em aliados:</b> +{buffDanio}% Dano por 3 turnos\n";
        if (valorAliados > 0)
        {
          cuerpo += $"<b>Bonus em aliados:</b> +{valorAliados} Valentía para os outros aliados\n";
        }
        if (NIVEL == 4)
        {
          cuerpo += "<b>Bonus proprio:</b> +2 Valentía por aliado afetado\n";
        }
        if (afectaEnemigos)
        {
          cuerpo += $"<b>Efeito em inimigos:</b> -10% Dano por {duracionDebuffEnemigos} turno (sem resistencia)";
        }
      }
      else
      {
        cuerpo += "<b>Tipo:</b> Soporte\n";
        cuerpo += "<b>Objetivo:</b> Todas las unidades aliadas de tu lado\n";
        cuerpo += $"<b>Buff aliados:</b> +{buffDanio}% Daño por 3 turnos\n";
        if (valorAliados > 0)
        {
          cuerpo += $"<b>Bono aliados:</b> +{valorAliados} Valentía a los demás aliados\n";
        }
        if (NIVEL == 4)
        {
          cuerpo += "<b>Bono propio:</b> +2 Valentía por cada aliado afectado\n";
        }
        if (afectaEnemigos)
        {
          cuerpo += $"<b>Efecto enemigos:</b> -10% Daño por {duracionDebuffEnemigos} turno (sin TS)";
        }
      }

      string costos = esIngles
        ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}"
        : esPortugues
          ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}";

      txtDescripcion = ConstruirDescripcionEstandar(
        esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
        esIngles
          ? "A commanding shout that empowers allies."
          : esPortugues
            ? "Um grito de comando que fortalece aliados."
          : "Un grito de mando que fortalece aliados.",
        cuerpo,
        costos,
        "#5dade2");

      string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
      string subtitulo = esIngles
        ? "A commanding shout that empowers allies."
        : esPortugues
          ? "Um grito de comando que fortalece aliados."
          : "Un grito de mando que fortalece aliados.";

      string cuerpoFormato = "";
      if (esIngles)
      {
        cuerpoFormato += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Support</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Target:</b></color> <color={colorValor}>All allied units on your side{(afectaEnemigos ? " and enemies on the opposite side" : string.Empty)}</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Allies:</b></color> <color={colorValor}>{iconoBuff} +{buffDanio}% Damage for 3 turns</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Other allies:</b></color> <color={colorValor}>+{valorAliados} Valour</color>";
        if (NIVEL == 4)
        {
          cuerpoFormato += $"\n<color={colorEncabezado}><b>Self:</b></color> <color={colorValor}>+2 Valour per affected ally</color>";
        }
        if (afectaEnemigos)
        {
          cuerpoFormato += $"\n<color={colorEncabezado}><b>Enemies:</b></color> <color={colorValor}>{iconoDebuff} -10% Damage for {duracionDebuffEnemigos} turn, no save</color>";
        }
      }
      else if (esPortugues)
      {
        cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Suporte</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Alvo:</b></color> <color={colorValor}>Todas as unidades aliadas do seu lado{(afectaEnemigos ? " e inimigos do lado oposto" : string.Empty)}</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Aliados:</b></color> <color={colorValor}>{iconoBuff} +{buffDanio}% Dano por 3 turnos</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Outros aliados:</b></color> <color={colorValor}>+{valorAliados} Valentía</color>";
        if (NIVEL == 4)
        {
          cuerpoFormato += $"\n<color={colorEncabezado}><b>Proprio:</b></color> <color={colorValor}>+2 Valentía por aliado afetado</color>";
        }
        if (afectaEnemigos)
        {
          cuerpoFormato += $"\n<color={colorEncabezado}><b>Inimigos:</b></color> <color={colorValor}>{iconoDebuff} -10% Dano por {duracionDebuffEnemigos} turno, sem resistencia</color>";
        }
      }
      else
      {
        cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Soporte</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Objetivo:</b></color> <color={colorValor}>Todas las unidades aliadas de tu lado{(afectaEnemigos ? " y enemigos del lado opuesto" : string.Empty)}</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Aliados:</b></color> <color={colorValor}>{iconoBuff} +{buffDanio}% Daño por 3 turnos</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Demas aliados:</b></color> <color={colorValor}>+{valorAliados} Valentía</color>";
        if (NIVEL == 4)
        {
          cuerpoFormato += $"\n<color={colorEncabezado}><b>Propio:</b></color> <color={colorValor}>+2 Valentía por aliado afectado</color>";
        }
        if (afectaEnemigos)
        {
          cuerpoFormato += $"\n<color={colorEncabezado}><b>Enemigos:</b></color> <color={colorValor}>{iconoDebuff} -10% Daño por {duracionDebuffEnemigos} turno, sin TS</color>";
        }
      }

      txtDescripcion =
        $"<size=115%><color=#5dade2><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n" +
        $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n" +
        "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n" +
        cuerpoFormato;

      bool mostrarProximoNivel = CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
      if (!mostrarProximoNivel)
      {
        return;
      }

      if (esIngles)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +5% allied damage buff.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: cooldown reduced from 4 to 3.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+2 Valour per ally to self) or Option B (enemy damage debuff).</color>"; }
      }
      else if (esPortugues)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +5% no buff de dano aliado.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: recarga reduzida de 4 para 3.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+2 Valentia por aliado para o Cavaleiro) ou Opcao B (debuff de dano em inimigos).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +5% al buff de daño aliado.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: enfriamiento reducido de 4 a 3.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A (+2 Valentía por aliado para el Caballero) u Opción B (debuff de daño a enemigos).</color>"; }
      }
    }

  public override async Task Resolver(List<object> Objetivos, Casilla cas) //Esto esta hecho para que anuncie el uso de la habilidad en el Log
  {
    ActualizarParametrosPorNivel();
    // El log de uso ahora está centralizado en Habilidad.Resolver
    VFXAplicarPropio(Usuario.gameObject);
   await base.Resolver(Objetivos);
    
  }


    void VFXAplicarPropio(GameObject objetivo)
  {
    VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_GritoMotivadorOrigen");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5); 
            //---

  }
  void VFXAplicarAliado(GameObject objetivo)
  {
    VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_GritoMotivadorEfectoAliado");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5); 
            //---
  }
   void VFXAplicarEnemigo(GameObject objetivo)
  {
    VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_GritoMotivadorEfectoEnemigo");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5); 
            //---
  }
    Casilla Origen;
    public override void Activar()
    {
        ActualizarParametrosPorNivel();
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

        
    }
    
    

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla casillaObjetivo)
    {

    
    if(obj is Unidad) //Acá van los efectos a Unidades.
     {

      Unidad objetivo = (Unidad)obj;
      if(objetivo.CasillaPosicion.lado == scEstaUnidad.CasillaPosicion.lado) //Chequea si son aliados para buffearlos o enemigos para debuffearlos (si nv5)
      { 

       if (objetivo != scEstaUnidad)
       {
        bool enIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
        string nombreObjetivo = TRADU.i != null ? TRADU.i.Traducir(objetivo.uNombre) : objetivo.uNombre;
        string motivoValentia = enIngles
          ? nombreObjetivo + " is emboldened by War Cry"
          : (TRADU.i != null && TRADU.i.nIdioma == 3)
            ? nombreObjetivo + " se encoraja com Grito Motivador"
          : nombreObjetivo + " se envalentona por Grito Motivador";
        objetivo.SumarValentia(1, motivoValentia, mostrarTextoFlotante: false);
       }

       if(NIVEL == 4)
       {
        scEstaUnidad.SumarValentia(2);
       }

        if (objetivo != scEstaUnidad)
        {
            VFXAplicarAliado(objetivo.gameObject);
        }
       /////////////////////////////////////////////
       //BUFF ---- Así se aplica un buff/debuff
       Buff buff = new Buff();
       buff.buffNombre = "Grito Motivador";
       buff.boolfDebufftBuff = true;
       buff.DuracionBuffRondas = 3;
       buff.cantDanioPorcentaje += 10;
       if( NIVEL > 1)
       {
        buff.cantDanioPorcentaje += 5;
       }
       buff.AplicarBuff(objetivo);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);

        objetivo.Marcar(0);
     }
     else if(NIVEL == 5)
     {
        VFXAplicarEnemigo(objetivo.gameObject);
       /////////////////////////////////////////////
       //BUFF ---- Así se aplica un buff/debuff
       Buff buff = new Buff();
       buff.buffNombre = "Grito Desmotivador";
       buff.boolfDebufftBuff = false;
       buff.DuracionBuffRondas = 1;
       buff.cantDanioPorcentaje -= 10;
       buff.AplicarBuff(objetivo);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);
       
       objetivo.Marcar(0);
     }
    }
    }
    
 

    //Provisorio
    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();

    private void ObtenerObjetivos()
    {
      //Cualquier objetivo en 1 de alcance 3 de ancho
      lObjetivosPosibles.Clear();
    
      lCasillasafectadas = Origen.ObtenerCasillasMismoLado();
      if(NIVEL == 5)
      {
         List<Casilla> lCasillop = Origen.ObtenerCasillasLadoOpuesto();
        lCasillasafectadas.AddRange(lCasillop);

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
             c.ActivarCapaColorAzul();
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









