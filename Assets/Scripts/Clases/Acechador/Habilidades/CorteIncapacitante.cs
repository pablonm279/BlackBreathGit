using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;

public class CorteIncapacitante : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano
     ClaseAcechador claseAcechador;
     public override void  Awake()
    {

      
      nombre = "Corte Incapacitante";
      costoAP = 3;
      costoPM = 0;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      claseAcechador = scEstaUnidad as ClaseAcechador;
      IDenClase = 4;
      esZonal = false;
      enArea = 0;
      esforzable = 1;
      esCargable = false;
      esMelee = true;
      esHostil = true;
      cooldownMax = 5;
      bAfectaObstaculos = true;

      bonusAtaque = 1;
      if(NIVEL > 1) { bonusAtaque++; } //Aumenta el ataque en 1 a partir del nivel 2
      XdDanio = 2;
      daniodX = 6; //2d6+3
      tipoDanio = 2; //Cortante
      criticoRangoHab = 0;



       tipoPorcentaje = 1;



      imHab = Resources.Load<Sprite>("imHab/Acechador_CorteIncapacitante");
      ActualizarDescripcion();
    }
    
   void Start()
   {
    if (claseAcechador != null)
    { Invoke("ChequearMaestria", 0.2f); }
   }

   public override void ActualizarDescripcion()
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
    var statsUI = ObtenerStatsDescripcionUI();

    int fuerzaActual = statsUI.Fuerza;
    int agilidadActual = statsUI.Agilidad;
    int ataqueActual = statsUI.Ataque;
    int criticoBaseMin = Mathf.Clamp(19 - (statsUI.CriticoRango + criticoRangoHab), 2, 20);

    int danioFijo = 3 + damExtra;
    int dcBase = NIVEL > 2 ? 8 : 7;
    int duracion = NIVEL == 5 ? 3 : 2;
    int nivelMaestria = claseAcechador != null ? claseAcechador.PASIVA_MaestriaConEspadacorta : 0;
    string rangoDanio = FormatearRangoDados(2, 6, danioFijo);
    int criticoPorcentaje = Mathf.Clamp(21 - criticoBaseMin, 0, 20) * 5;
    int dcTotal = dcBase + agilidadActual;
    string colorTitulo = "#5dade2";
    string colorEncabezado = "#44d3ec";
    string colorFuerza = "#d9822b";
    string colorAgilidad = "#7fa35a";
    string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
    string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
    string costoSuperior = $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}";
    string atributoAtaque = esIngles
      ? $"<color={colorFuerza}>Strength ({fuerzaActual})</color>"
      : esPortugues
        ? $"<color={colorFuerza}>Forca ({fuerzaActual})</color>"
        : $"<color={colorFuerza}>Fuerza ({fuerzaActual})</color>";
    string atributoTS = esIngles
      ? $"<color={colorAgilidad}>Agility ({agilidadActual})</color>"
      : esPortugues
        ? $"<color={colorAgilidad}>Agilidade ({agilidadActual})</color>"
        : $"<color={colorAgilidad}>Agilidad ({agilidadActual})</color>";
    string bonusTirada = TextoModificadorDescripcion(ataqueActual) + TextoModificadorDescripcion(bonusAtaque);

    string tituloEs = "Corte Incapacitante I";
    string tituloEn = "Crippling Slash I";
    string tituloPt = "Corte Incapacitante I";
    if (NIVEL == 2) { tituloEs = "Corte Incapacitante II"; tituloEn = "Crippling Slash II"; }
    if (NIVEL == 3) { tituloEs = "Corte Incapacitante III"; tituloEn = "Crippling Slash III"; }
    if (NIVEL == 4) { tituloEs = "Corte Incapacitante IV a"; tituloEn = "Crippling Slash IV a"; }
    if (NIVEL == 5) { tituloEs = "Corte Incapacitante IV b"; tituloEn = "Crippling Slash IV b"; }
    if (NIVEL == 2) { tituloPt = "Corte Incapacitante II"; }
    if (NIVEL == 3) { tituloPt = "Corte Incapacitante III"; }
    if (NIVEL == 4) { tituloPt = "Corte Incapacitante IV a"; }
    if (NIVEL == 5) { tituloPt = "Corte Incapacitante IV b"; }

    if (esIngles)
    {
      string fuerza = TerminoDescripcion(TerminoDescripcionId.Fuerza, $"Strength ({fuerzaActual})");
      string agilidad = TerminoDescripcion(TerminoDescripcionId.Agilidad, $"Agility ({agilidadActual})");
      string defensa = TerminoDescripcion(TerminoDescripcionId.Defensa, "Defense", "IconoDefensa");
      string fortaleza = TerminoDescripcion(TerminoDescripcionId.SalvacionFortaleza, "Fortitude", "ic_fortaleza");
      string danioCortante = TerminoDescripcion(TerminoDescripcionId.DanioCortante, "Slashing damage", "dano_cortante");
      string sangrado = TerminoDescripcion(TerminoDescripcionId.Sangrado, "Bleed", "Estado_sangrano");
      string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crit", "critico");
      string proximaMejora = null;
      if (DebeMostrarProximaMejoraDescripcion())
      {
        if (NIVEL < 2) proximaMejora = "+1 Attack Roll.";
        else if (NIVEL == 2) proximaMejora = "+1 save DC.";
        else if (NIVEL == 3) proximaMejora = "Option A: +3 Bleed. Option B: +1 turn duration.";
      }
      var lineas = new List<LineaDescripcionNormalizada>
      {
        LineaDescripcion("Target", ObjetivoMeleeUnitarioIngles),
        LineaDescripcion("Effect", $"On hit, deals {rangoDanio} + {fuerza} as {danioCortante}."),
        LineaDescripcion("Attack Roll", $"1d20 + {fuerza}{bonusTirada} vs {defensa}. Fumble: 5%. {critico}: {criticoPorcentaje}%."),
        LineaDescripcion("Save", $"Target's {fortaleza} vs DC {dcBase} + {agilidad} ({dcTotal}).", 1),
        LineaDescripcion("Failed save", $"Crippled: Immobile, -20% damage and -2 Attack ({duracion} turns)" + (NIVEL == 4 ? $"; applies 3 {sangrado}." : "."), 1)
      };
      if (nivelMaestria > 0)
      {
        lineas.Add(LineaDescripcion("Passive", $"Short Sword Mastery (Tier {nivelMaestria})."));
      }
      txtDescripcion = ConstruirDescripcionNormalizadaIngles(
        tituloEn,
        "Short sword attack. Can cripple the target.",
        lineas,
        proximaMejora,
        mostrarIconoMelee: true);
      return;
    }

    if (esPortugues)
    {
      string forca=TerminoDescripcion(TerminoDescripcionId.Fuerza,$"Força ({fuerzaActual})"); string agi=TerminoDescripcion(TerminoDescripcionId.Agilidad,$"Agilidade ({agilidadActual})"); string def=TerminoDescripcion(TerminoDescripcionId.Defensa,"Defesa","IconoDefensa"); string dano=TerminoDescripcion(TerminoDescripcionId.DanioCortante,"dano Cortante","dano_cortante"); string fort=TerminoDescripcion(TerminoDescripcionId.SalvacionFortaleza,"Fortitude","ic_fortaleza"); string sang=TerminoDescripcion(TerminoDescripcionId.Sangrado,"Sangramento","sangrado"); string crit=TerminoDescripcion(TerminoDescripcionId.Critico,"Crítico","critico"); string prox=!DebeMostrarProximaMejoraDescripcion()?null:NIVEL<2?"Próximo nível: +1 na Rolagem de Ataque.":NIVEL==2?"Próximo nível: +1 CD da salvaguarda.":NIVEL==3?"Opção A: +3 Sangramento. Opção B: +1 turno de duração.":null; var l=new List<LineaDescripcionNormalizada>{LineaDescripcion("Alvo","1 alvo ou obstáculo em alcance corpo a corpo"),LineaDescripcion("Efeito",$"Ao acertar, causa {rangoDanio} + {forca} como {dano}."),LineaDescripcion("Rolagem de Ataque",$"1d20 + {forca}{bonusTirada} vs {def}. Falha crítica: 5%. {crit}: {criticoPorcentaje}%."),LineaDescripcion("Salvaguarda",$"{fort} do alvo vs CD {dcBase} + {agi} ({dcTotal}).",1),LineaDescripcion("Falha",$"Incapacitado: Imóvel, -20% de dano e -2 Ataque ({duracion} turnos)"+(NIVEL==4?$"; aplica 3 {sang}.":"."),1)}; if(nivelMaestria>0)l.Add(LineaDescripcion("Passiva",$"Maestria com Espada Curta (Nível {nivelMaestria}).")); txtDescripcion=ConstruirDescripcionNormalizadaLocalizada(tituloPt,"Ataque com espada curta. Pode incapacitar o alvo.",l,prox,mostrarIconoMelee:true); return;
    }
    {
      string fuerza=TerminoDescripcion(TerminoDescripcionId.Fuerza,$"Fuerza ({fuerzaActual})"); string agi=TerminoDescripcion(TerminoDescripcionId.Agilidad,$"Agilidad ({agilidadActual})"); string def=TerminoDescripcion(TerminoDescripcionId.Defensa,"Defensa","IconoDefensa"); string dano=TerminoDescripcion(TerminoDescripcionId.DanioCortante,"daño Cortante","dano_cortante"); string fort=TerminoDescripcion(TerminoDescripcionId.SalvacionFortaleza,"Fortaleza","ic_fortaleza"); string sang=TerminoDescripcion(TerminoDescripcionId.Sangrado,"Sangrado","sangrado"); string crit=TerminoDescripcion(TerminoDescripcionId.Critico,"Crítico","critico"); string prox=!DebeMostrarProximaMejoraDescripcion()?null:NIVEL<2?"Próximo nivel: +1 a la Tirada de Ataque.":NIVEL==2?"Próximo nivel: +1 CD de salvación.":NIVEL==3?"Opción A: +3 Sangrado. Opción B: +1 turno de duración.":null; var l=new List<LineaDescripcionNormalizada>{LineaDescripcion("Objetivo","1 objetivo u obstáculo en alcance cuerpo a cuerpo"),LineaDescripcion("Efecto",$"Al impactar, inflige {rangoDanio} + {fuerza} como {dano}."),LineaDescripcion("Tirada de Ataque",$"1d20 + {fuerza}{bonusTirada} vs {def}. Pifia: 5%. {crit}: {criticoPorcentaje}%."),LineaDescripcion("Salvación",$"{fort} del objetivo vs CD {dcBase} + {agi} ({dcTotal}).",1),LineaDescripcion("Salvación fallida",$"Incapacitado: Inmóvil, -20% de daño y -2 Ataque ({duracion} turnos)"+(NIVEL==4?$"; aplica 3 {sang}.":"."),1)}; if(nivelMaestria>0)l.Add(LineaDescripcion("Pasiva",$"Maestría con Espada Corta (Nivel {nivelMaestria}).")); txtDescripcion=ConstruirDescripcionNormalizadaLocalizada(tituloEs,"Ataque con espada corta. Puede incapacitar al objetivo.",l,prox,mostrarIconoMelee:true); return;
    }

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> Melee attack\n";
      cuerpo += $"<color={colorEncabezado}><b>Target:</b></color> 1 enemy or obstacle in frontal melee range\n";
      cuerpo += $"<color={colorEncabezado}><b>Roll:</b></color> 1d20 + {atributoAtaque}{bonusTirada} vs Defense\n";
      cuerpo += $"<color={colorEncabezado}><b>Fumble:</b></color> 5%   <color={colorEncabezado}><b>Crit:</b></color> {criticoPorcentaje}%\n";
      cuerpo += $"<color={colorEncabezado}><b>Damage:</b></color> {rangoDanio} + {atributoAtaque}. <color={colorEncabezado}><b>Type:</b></color> Slashing\n";
      cuerpo += $"<color={colorEncabezado}><b>Save:</b></color> Fortitude vs DC {dcBase} + {atributoTS} = {dcTotal}\n";
      cuerpo += $"<color={colorEncabezado}><b>On failed save:</b></color> Crippled ({duracion} turns): Immobile, -20% Damage, -2 Attack";
      if (NIVEL == 4)
      {
        cuerpo += ", +3 Bleed";
      }
      if (nivelMaestria > 0)
      {
        cuerpo += $"\n<color={colorEncabezado}><b>Passive:</b></color> Short Sword Mastery (Tier {nivelMaestria})";
      }
    }
    else if (esPortugues)
    {
      cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Ataque corpo a corpo\n";
      cuerpo += $"<color={colorEncabezado}><b>Alvo:</b></color> 1 inimigo ou obstaculo no alcance frontal corpo a corpo\n";
      cuerpo += $"<color={colorEncabezado}><b>Rolagem:</b></color> 1d20 + {atributoAtaque}{bonusTirada} vs Defesa\n";
      cuerpo += $"<color={colorEncabezado}><b>Falha critica:</b></color> 5%   <color={colorEncabezado}><b>Critico:</b></color> {criticoPorcentaje}%\n";
      cuerpo += $"<color={colorEncabezado}><b>Dano:</b></color> {rangoDanio} + {atributoAtaque}. <color={colorEncabezado}><b>Tipo:</b></color> Cortante\n";
      cuerpo += $"<color={colorEncabezado}><b>Resistencia:</b></color> Fortitude vs CD {dcBase} + {atributoTS} = {dcTotal}\n";
      cuerpo += $"<color={colorEncabezado}><b>Se falhar:</b></color> Incapacitado ({duracion} turnos): Imovel, -20% Dano, -2 Ataque";
      if (NIVEL == 4)
      {
        cuerpo += ", +3 Sangramento";
      }
      if (nivelMaestria > 0)
      {
        cuerpo += $"\n<color={colorEncabezado}><b>Passiva:</b></color> Maestria com Espada Curta (Tier {nivelMaestria})";
      }
    }
    else
    {
      cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Ataque melee\n";
      cuerpo += $"<color={colorEncabezado}><b>Objetivo:</b></color> 1 enemigo u obstáculo en alcance melee frontal\n";
      cuerpo += $"<color={colorEncabezado}><b>Tirada:</b></color> 1d20 + {atributoAtaque}{bonusTirada} vs Defensa\n";
      cuerpo += $"<color={colorEncabezado}><b>Pifia:</b></color> 5%   <color={colorEncabezado}><b>Crítico:</b></color> {criticoPorcentaje}%\n";
      cuerpo += $"<color={colorEncabezado}><b>Daño:</b></color> {rangoDanio} + {atributoAtaque}. <color={colorEncabezado}><b>Tipo:</b></color> Cortante\n";
      cuerpo += $"<color={colorEncabezado}><b>TS:</b></color> Fortaleza vs DC {dcBase} + {atributoTS} = {dcTotal}\n";
      cuerpo += $"<color={colorEncabezado}><b>Si falla TS:</b></color> Incapacitado ({duracion} turnos): Inmovil, -20% Daño, -2 Ataque";
      if (NIVEL == 4)
      {
        cuerpo += ", +3 Sangrado";
      }
      if (nivelMaestria > 0)
      {
        cuerpo += $"\n<color={colorEncabezado}><b>Pasiva:</b></color> Maestria con Espada Corta (Tier {nivelMaestria})";
      }
    }

    string subtitulo = esIngles
      ? "Melee attack that can cripple the target."
      : esPortugues
        ? "Ataque corpo a corpo que pode incapacitar o alvo."
        : "Ataque melee que puede incapacitar al objetivo.";
    string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
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
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 attack bonus.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 save DC base.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+3 Bleed) or Option B (+1 turn duration).</color>"; }
    }
    else if (esPortugues)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 no bonus de ataque.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 na CD base da resistencia.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+3 Sangramento) ou Opcao B (+1 turno de duracao).</color>"; }
    }
    else
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 al bono de ataque.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 al DC base de TS.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A (+3 Sangrado) u Opción B (+1 turno de duración).</color>"; }
    }
  }

  private string TextoModificadorDescripcion(int valor)
  {
    if (valor > 0) { return $" + {valor}"; }
    if (valor < 0) { return $" - {Mathf.Abs(valor)}"; }
    return "";
  }

  int damExtra;
  int danioCriticoMaestria;
      Casilla Origen;
    public override void Activar()
    {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

        
    }
    
    
    void ChequearMaestria()
  {
    int NivelMaestria = claseAcechador.PASIVA_MaestriaConEspadacorta;

    if (NivelMaestria == 1)
    {
      bonusAtaque = 1;
      damExtra += 2;
      txtDescripcion += "\n\n<i>Maestría con Espada Corta agrega: +1 Ataque +2 Daño.</i>\n\n";

    }
    else if (NivelMaestria == 2)
    {
      bonusAtaque = 1;
      damExtra += 2;
      criticoRangoHab = 1;
      txtDescripcion += "\n\n<i>Maestría con Espada Corta agrega: +1 Ataque +2 Daño +5% Crítico.</i>\n\n";

    }
    else if (NivelMaestria == 3)
    {
      bonusAtaque = 1;
      damExtra += 2;
      criticoRangoHab = 1;
      danioCriticoMaestria = 10;
      txtDescripcion += "\n\n<i>Maestría con Espada Corta agrega: +1 Ataque +2 Daño +5% Crítico, +10% Daño Crítico.</i>\n\n";


    }
    else if (NivelMaestria == 4)
    {
      bonusAtaque = 1;
      damExtra += 4;
      criticoRangoHab = 2;
      danioCriticoMaestria = 10;
      txtDescripcion += "\n\n<i>Maestría con Espada Corta agrega: +1 Ataque +4 Daño +10% Crítico, +10% Daño Crítico.</i>\n\n";

    }
    else if (NivelMaestria == 5)
    {
      bonusAtaque = 2;
      damExtra += 4;
      criticoRangoHab = 1;
      danioCriticoMaestria = 10;
      txtDescripcion += "\n\n<i>Maestría con Espada Corta agrega: Remueve Cooldown, +2 Ataque +4 Daño +5% Critico, +10% Daño Crítico.</i>\n\n";

    }

      ActualizarDescripcion();
    
  }
    

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
  {

    if (obj is Unidad) //Acá van los efectos a Unidades.
    {
      Unidad objetivo = (Unidad)obj;
      float defensaObjetivo = objetivo.ObtenerdefensaActual();
      print("Defensa: " + defensaObjetivo);

      int danioMarca = 0;


      float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab;

      int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, scEstaUnidad.mod_CarFuerza, bonusAtaque, criticoRango, objetivo, 0);
      print("Resultado tirada " + resultadoTirada);


      if (resultadoTirada == -1)
      {//PIFIA 
        print("Pifia");
        objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
        //BattleManager.Instance.TerminarTurno(); //Al ser Pifia, termina el turno.
       scEstaUnidad.EstablecerAPActualA(0);
      }
      else if (resultadoTirada == 0)
      {//FALLO
        print("Fallo");
        objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);

      }
      else if (resultadoTirada == 1)
      {//ROCE
        print("Roce");
        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 3 + damExtra + scEstaUnidad.mod_CarFuerza + danioMarca;

        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        danio -= danio / 2; //Reduce 50% por roce

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
        VFXAplicar(objetivo.gameObject);
        EfectoAdicional(objetivo);


      }
      else if (resultadoTirada == 2)
      {//GOLPE
        print("Golpe");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 3 + damExtra + scEstaUnidad.mod_CarFuerza + danioMarca;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);
        VFXAplicar(objetivo.gameObject);
        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

        EfectoAdicional(objetivo);

      }
      else if (resultadoTirada == 3)
      {//CRITICO
        print("Crítico");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 3 + damExtra + scEstaUnidad.mod_CarFuerza + danioMarca;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje + danioCriticoMaestria);
        VFXAplicar(objetivo.gameObject);
        objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);

        EfectoAdicional(objetivo);
      }

      objetivo.AplicarDebuffPorAtaquesreiterados(1);
    }
    else if (obj is Obstaculo) //Acá van los efectos a Obstaculos
    {
      Obstaculo objetivo = (Obstaculo)obj;
      //---

      VFXAplicar(objetivo.gameObject);
      float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 3 + damExtra + scEstaUnidad.mod_CarFuerza;
      danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

      objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
    }
  }
    
      void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_CorteIncapacitante");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);  

    }
    
  void EfectoAdicional(Unidad objetivo)
  {

    int DC = 7 + (int)scEstaUnidad.mod_CarAgilidad; //DC de la tirada de salvación

    if (NIVEL > 2) { DC++; }


    if (objetivo.TiradaSalvacion(2, DC))
    {
       int duracion = 2;
       if (NIVEL == 5) { duracion++; }
       if (NIVEL == 4) { Estados.Aplicar_Sangrado(objetivo, 3, scEstaUnidad); }

       if (objetivo.estado_inmovil < 0)
       {
         return;
       }

        /////////////////////////////////////////////
      //BUFF ---- Así se aplica un buff/debuff
      Buff buff = new Buff();
       buff.buffNombre = "Incapacitado";
       buff.buffDescr = "Inmóvil, Melee solo adyacente.";
       buff.boolfDebufftBuff = false;
       buff.DuracionBuffRondas = duracion;
       buff.cantDanioPorcentaje -= 20;
        buff.cantAtAgi -= 2;
        buff.AplicarBuff(objetivo);
        Estados.Aplicar_Inmovil(objetivo, duracion, scEstaUnidad);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);



    }

  }
   
    //Provisorio
  private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();

    private void ObtenerObjetivos()
    {
      //Cualquier objetivo en 1 de alcance 3 de ancho
      lObjetivosPosibles.Clear();
      
      //Melee - Si está en columna 3 de su lado, aumenta el rango ignorando cada columna vacia del lado opuesto
      int rangoPlus = 0;
   
      if(esMelee) 
      {
        if(Usuario.GetComponent<Unidad>().CasillaPosicion.posX == 3)
        {
           rangoPlus = AumentarRangoMelee();
        }

        if (TieneObstaculooUnidadAdelanteDeSuLado() != 0)
        {
          rangoPlus ++;
        }
      }
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasRango(1+rangoPlus,1);
    
      foreach(Casilla c in lCasillasafectadas)
      {
       
       
       c.ActivarCapaColorRojo();
       if(esMelee)//Si hab es melee, activa capa roja, de columna al alcance final, no de las otras también
       {
         if(c.transform.GetChild(2).gameObject.activeInHierarchy){ c.DesactivarCapaColorRojo();}
       } 



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
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());;
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

      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
       
    
        



      
         
    }

    private int AumentarRangoMelee() //aumenta el rango melee si no hay nada en frente ni filas adyacentes al origen de la habilidad
    {
     
      LadoManager scLado = Origen.ladoOpuesto.GetComponent<LadoManager>();

      int posYorigen = scEstaUnidad.CasillaPosicion.posY;
      

      List<Casilla> casillasAdyacentesyFrenteColumna1 = new List<Casilla>();
      List<Casilla> casillasAdyacentesyFrenteColumna2 = new List<Casilla>();
    
      foreach(Transform child in Origen.ladoOpuesto.transform) //Itera en cada casilla del lado opuesto
      {
          Casilla cas = child.GetComponent<Casilla>();

          if(cas.posX == 3) //Columna 1 (frente)
          {
             int calculo = Math.Abs(cas.posY - posYorigen); //distancia en Y al origen para calcular adyacentes o frontal
            
             if(calculo < 2)
             {
               casillasAdyacentesyFrenteColumna1.Add(cas);
             }
          }

          if(cas.posX == 2) //Columna 2 (medio)
          {
             int calculo = Math.Abs(cas.posY - posYorigen); //distancia en Y al origen para calcular adyacentes o frontal
            
             if(calculo < 2)
             {
               casillasAdyacentesyFrenteColumna2.Add(cas);
             }
          }

        
      }

       //Se fija si las 3 casillas de la columna 1 están vacias
       foreach(Casilla cas in casillasAdyacentesyFrenteColumna1)
       {
          if(cas.BloqueaAvanceMeleeDesdeFila(posYorigen, scEstaUnidad)) //si alguna de las 3 tiene algo, no aumenta el rango melee
          {
            return 0;
          }
       }
               foreach(Casilla casOsc in casillasAdyacentesyFrenteColumna1) //si ninguna de las tres tiene algo, las oscurece
               {  casOsc.ActivarCapaColorNegro(); }



       

       foreach(Casilla cas in casillasAdyacentesyFrenteColumna2) 
       {
          if(cas.BloqueaAvanceMeleeDesdeFila(posYorigen, scEstaUnidad)) //y si alguna de las 3 tiene algo, aumenta solo en 1
          {
            return 1;
          }
       }
               foreach(Casilla casOsc in casillasAdyacentesyFrenteColumna2) //si ninguna de las tres tiene algo, las oscurece
               {  casOsc.ActivarCapaColorNegro(); }




       return 2; //si ninguna de las 2 columnas tiene algo, aumenta al maximo
    }

  int TieneObstaculooUnidadAdelanteDeSuLado()
    {
      int orX = Origen.posX;
      int orY = Origen.posY;
      GameObject lado = Origen.ladoGO;

      
      if(orX != 2) //Solamente util en la columna del medio
      {
         return 0;
      }
 
       Casilla casillaRevisar = null;
       foreach(Transform child in lado.transform)
       {
         Casilla cas = child.GetComponent<Casilla>();
         if((cas.posY == orY)&&(cas.posX == orX+1))
         {
          casillaRevisar = cas;
         }

       }

      if(casillaRevisar.Presente != null)
      {
        if(casillaRevisar.Presente.GetComponent<Unidad>() != null)
        {
          return 1; //Devuelve 1 si es unidad
        }

        if(casillaRevisar.Presente.GetComponent<Obstaculo>() != null)
        {
           if(casillaRevisar.Presente.GetComponent<Obstaculo>().bPermiteAtacarDetras)
          {
            return 2; //Devuelve 2 si es obstaculo
          }
          else{ return 0;}
        }
      }
      return 0; //Devuelve 0 si no hay nada 
    }
}





