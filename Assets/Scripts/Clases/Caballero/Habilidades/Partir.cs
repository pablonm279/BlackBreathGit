using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class Partir : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de crpitico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano

 

      public override void  Awake()
    {
      nombre = "Partir";
      IDenClase = 6;
      costoAP = 4;
      costoPM = 0;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 1;
      esCargable = false;
      esMelee = true;
      esHostil = true;
      cooldownMax = 3;
      bAfectaObstaculos = true;

      bonusAtaque = 0; 
      XdDanio = 2;
      daniodX = 10; //2d10 +5
      tipoDanio = 2; //Cortante
      criticoRangoHab = 0;
       tipoPorcentaje = 1;
       imHab = Resources.Load<Sprite>("imHab/Caballero_Partir");
      
      ActualizarDescripcion();
    
    }

    public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
      var statsUI = ObtenerStatsDescripcionUI();

      int fuerzaActual = statsUI.Fuerza;
      int ataqueActual = statsUI.Ataque;
      int bonusAtaqueNivel = NIVEL > 2 ? 2 : 0;
      int bonusCritNivel = NIVEL == 5 ? 1 : 0;
      int criticoMin = Mathf.Clamp(19 - (statsUI.CriticoRango + criticoRangoHab + bonusCritNivel), 2, 20);
      int danioBaseFijo = 5 + (NIVEL > 1 ? 4 : 0);
      int dcMiedo = NIVEL == 4 ? 15 : 13;
      string rangoDanio = FormatearRangoDados(XdDanio, daniodX, danioBaseFijo);
      string bonusAtaqueTxt = bonusAtaqueNivel >= 0 ? $" + {bonusAtaqueNivel}" : $" - {Mathf.Abs(bonusAtaqueNivel)}";
      string lineaSalvacionEs = ConstruirLineaSalvacion(false, TipoSalvacionDescripcion.Mental, dcMiedo);
      string lineaSalvacionEn = ConstruirLineaSalvacion(true, TipoSalvacionDescripcion.Mental, dcMiedo);

      string tituloEs = "Partir I";
      string tituloEn = "Cleave I";
      string tituloPt = "Partir I";
      if (NIVEL == 2) { tituloEs = "Partir II"; tituloEn = "Cleave II"; }
      if (NIVEL == 3) { tituloEs = "Partir III"; tituloEn = "Cleave III"; }
      if (NIVEL == 4) { tituloEs = "Partir IV a"; tituloEn = "Cleave IV a"; }
      if (NIVEL == 5) { tituloEs = "Partir IV b"; tituloEn = "Cleave IV b"; }
      if (NIVEL == 2) { tituloPt = "Partir II"; }
      if (NIVEL == 3) { tituloPt = "Partir III"; }
      if (NIVEL == 4) { tituloPt = "Partir IV a"; }
      if (NIVEL == 5) { tituloPt = "Partir IV b"; }

      int pifiaPorcentaje = 10;
      int criticoPorcentaje = Mathf.Clamp(21 - criticoMin, 0, 20) * 5;
      int modificadorAtaqueExtra = ataqueActual + bonusAtaqueNivel;
      string ataqueTxt = modificadorAtaqueExtra == 0
        ? string.Empty
        : modificadorAtaqueExtra > 0 ? $" + {modificadorAtaqueExtra}" : $" - {Mathf.Abs(modificadorAtaqueExtra)}";
      string colorEncabezado = "#44d3ec";
      string colorValor = "#ffffff";
      string colorFuerza = "#d9822b";
      string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
      string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
      string iconoDebuff = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_debuff\"></voffset></size><space=-0.35em>";
      string costoSuperior = cooldownMax > 0
        ? $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}"
        : $"{costoAP} {iconoAP}";

      if (esIngles)
      {
        string fuerza = TerminoDescripcion(TerminoDescripcionId.Fuerza, $"Strength ({fuerzaActual})");
        string defensa = TerminoDescripcion(TerminoDescripcionId.Defensa, "Defense", "IconoDefensa");
        string danioCortante = TerminoDescripcion(TerminoDescripcionId.DanioCortante, "Slashing damage", "dano_cortante");
        string mental = TerminoDescripcion(TerminoDescripcionId.SalvacionMental, "Mental", "ic_mental");
        string aterrorizado = TerminoDescripcion(TerminoDescripcionId.Aterrorizado, "Terrified", "Estado_debuff");
        string ap = TerminoDescripcion(TerminoDescripcionId.PuntosAccion, "max AP", "ap");
        string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crit", "critico");
        string proximaMejora = null;
        if (DebeMostrarProximaMejoraDescripcion())
        {
          if (NIVEL < 2) { proximaMejora = "+4 damage."; }
          else if (NIVEL == 2) { proximaMejora = "+2 Attack Roll bonus."; }
          else if (NIVEL == 3) { proximaMejora = "Option A: +2 Save DC on kill.\nOption B: +5% Crit."; }
        }

        var lineas = new List<LineaDescripcionNormalizada>
        {
          LineaDescripcion("Target", ObjetivoMeleeUnitarioIngles),
          LineaDescripcion("Effect", $"On hit, deals {rangoDanio} + {fuerza} as {danioCortante}."),
          LineaDescripcion("Attack Roll", $"1d20 + {fuerza}{ataqueTxt} vs {defensa}. Fumble: {pifiaPorcentaje}%. {critico}: {criticoPorcentaje}%.", 1)
        };
        if (penetracionArmadura > 0)
        {
          string penetracion = TerminoDescripcion(TerminoDescripcionId.PenetracionArmadura, "Armor Penetration", "IconoArmadura");
          lineas.Add(LineaDescripcion("Penetration", $"{penetracion}: {penetracionArmadura}"));
        }
        lineas.Add(LineaDescripcion("On kill", "All enemies make a Mental save."));
        lineas.Add(LineaDescripcion("Save", $"{mental} vs DC {dcMiedo}", 1));
        lineas.Add(LineaDescripcion("Failed save", $"Becomes {aterrorizado} for 2 turns: -2 Attack, -1 {ap}, -2 Mental Save.", 1));

        txtDescripcion = ConstruirDescripcionNormalizadaIngles(
          tituloEn,
          "A powerful execution strike that may terrify every enemy on kill.",
          lineas,
          proximaMejora,
          mostrarIconoMelee: true);
        return;
      }

      if (esPortugues)
      {
        string forca = TerminoDescripcion(TerminoDescripcionId.Fuerza, $"Força ({fuerzaActual})");
        string defesa = TerminoDescripcion(TerminoDescripcionId.Defensa, "Defesa", "IconoDefensa");
        string danoCortante = TerminoDescripcion(TerminoDescripcionId.DanioCortante, "dano Cortante", "dano_cortante");
        string mental = TerminoDescripcion(TerminoDescripcionId.SalvacionMental, "Mental", "ic_mental");
        string aterrorizado = TerminoDescripcion(TerminoDescripcionId.Aterrorizado, "Aterrorizado");
        string ap = TerminoDescripcion(TerminoDescripcionId.PuntosAccion, "AP", "ap");
        string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crítico", "critico");
        string proximaMejora = null;
        if (DebeMostrarProximaMejoraDescripcion())
        {
          if (NIVEL < 2) { proximaMejora = "Próximo nível: +4 de dano."; }
          else if (NIVEL == 2) { proximaMejora = "Próximo nível: +2 na Rolagem de Ataque."; }
          else if (NIVEL == 3) { proximaMejora = "Opção A: +2 CD da salvaguarda ao matar.\nOpção B: +5% de Crítico."; }
        }
        var lineas = new List<LineaDescripcionNormalizada>
        {
          LineaDescripcion("Alvo", "1 alvo ou obstáculo em alcance corpo a corpo"),
          LineaDescripcion("Efeito", $"Ao acertar, causa {rangoDanio} + {forca} como {danoCortante}."),
          LineaDescripcion("Rolagem de Ataque", $"1d20 + {forca}{ataqueTxt} vs {defesa}. Falha crítica: {pifiaPorcentaje}%. {critico}: {criticoPorcentaje}%.", 1)
        };
        if (penetracionArmadura > 0) { string penetracao = TerminoDescripcion(TerminoDescripcionId.PenetracionArmadura, "Penetração de Armadura", "IconoArmadura"); lineas.Add(LineaDescripcion("Penetração", $"{penetracao}: {penetracionArmadura}")); }
        lineas.Add(LineaDescripcion("Ao matar", "Todos os inimigos fazem uma salvaguarda Mental."));
        lineas.Add(LineaDescripcion("Salvaguarda", $"{mental} vs CD {dcMiedo}", 1));
        lineas.Add(LineaDescripcion("Falha", $"Fica {aterrorizado} por 2 turnos: -2 Ataque, -1 {ap}, -2 Salvaguarda Mental.", 1));
        txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(tituloPt, "Um poderoso golpe de execução que pode aterrorizar todos os inimigos ao matar.", lineas, proximaMejora, mostrarIconoMelee: true);
        return;
      }

      {
        string fuerza = TerminoDescripcion(TerminoDescripcionId.Fuerza, $"Fuerza ({fuerzaActual})");
        string defensa = TerminoDescripcion(TerminoDescripcionId.Defensa, "Defensa", "IconoDefensa");
        string danoCortante = TerminoDescripcion(TerminoDescripcionId.DanioCortante, "daño Cortante", "dano_cortante");
        string mental = TerminoDescripcion(TerminoDescripcionId.SalvacionMental, "Mental", "ic_mental");
        string aterrorizado = TerminoDescripcion(TerminoDescripcionId.Aterrorizado, "Aterrorizado");
        string ap = TerminoDescripcion(TerminoDescripcionId.PuntosAccion, "AP", "ap");
        string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crítico", "critico");
        string proximaMejora = null;
        if (DebeMostrarProximaMejoraDescripcion())
        {
          if (NIVEL < 2) { proximaMejora = "Próximo nivel: +4 de daño."; }
          else if (NIVEL == 2) { proximaMejora = "Próximo nivel: +2 a la Tirada de Ataque."; }
          else if (NIVEL == 3) { proximaMejora = "Opción A: +2 CD de salvación al matar.\nOpción B: +5% de Crítico."; }
        }
        var lineas = new List<LineaDescripcionNormalizada>
        {
          LineaDescripcion("Objetivo", "1 objetivo u obstáculo en alcance cuerpo a cuerpo"),
          LineaDescripcion("Efecto", $"Al impactar, inflige {rangoDanio} + {fuerza} como {danoCortante}."),
          LineaDescripcion("Tirada de Ataque", $"1d20 + {fuerza}{ataqueTxt} vs {defensa}. Pifia: {pifiaPorcentaje}%. {critico}: {criticoPorcentaje}%.", 1)
        };
        if (penetracionArmadura > 0) { string penetracion = TerminoDescripcion(TerminoDescripcionId.PenetracionArmadura, "Penetración de Armadura", "IconoArmadura"); lineas.Add(LineaDescripcion("Penetración", $"{penetracion}: {penetracionArmadura}")); }
        lineas.Add(LineaDescripcion("Al matar", "Todos los enemigos hacen una salvación Mental."));
        lineas.Add(LineaDescripcion("Salvación", $"{mental} vs CD {dcMiedo}", 1));
        lineas.Add(LineaDescripcion("Salvación fallida", $"Se vuelve {aterrorizado} durante 2 turnos: -2 Ataque, -1 {ap}, -2 Salvación Mental.", 1));
        txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(tituloEs, "Un poderoso golpe de ejecución que puede aterrorizar a todos los enemigos al matar.", lineas, proximaMejora, mostrarIconoMelee: true);
        return;
      }

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += "<b>Type:</b> Melee\n";
        cuerpo += "<b>Target:</b> 1 enemy in front range\n";
        cuerpo += $"<b>Roll:</b> 1d20 + <color=#ea0606>Strength ({fuerzaActual})</color>  {bonusAtaqueTxt} vs Defense. Fumble: 1-2. Crit: {criticoMin}-20\n";
        cuerpo += $"<b>Damage:</b> 2d10 + {danioBaseFijo} + <color=#ea0606>Strength ({fuerzaActual})</color> | <b>Type:</b> Slashing\n";
        cuerpo += $"<b>Armor Penetration:</b> {penetracionArmadura}\n";
        cuerpo += "<b>On kill:</b> All enemies roll save\n";
        cuerpo += $"{lineaSalvacionEn}\n";
        cuerpo += "<b>On failed save:</b> Terrified for 2 turns (-2 Attack, -1 Max AP, -2 Mental Save)";
      }
      else if (esPortugues)
      {
        cuerpo += "<b>Tipo:</b> Corpo a corpo\n";
        cuerpo += "<b>Alvo:</b> 1 inimigo no alcance frontal\n";
        cuerpo += $"<b>Rolagem:</b> 1d20 + <color=#ea0606>Forca ({fuerzaActual})</color> + Ataque ({ataqueActual}){bonusAtaqueTxt} vs Defesa. Falha critica: 1-2. Critico: {criticoMin}-20\n";
        cuerpo += $"<b>Dano:</b> 2d10 + {danioBaseFijo} + <color=#ea0606>Forca ({fuerzaActual})</color> | <b>Tipo:</b> Cortante\n";
        cuerpo += $"<b>Penetracao de armadura:</b> {penetracionArmadura}\n";
        cuerpo += "<b>Se matar o alvo:</b> todos os inimigos fazem resistencia\n";
        cuerpo += $"{lineaSalvacionEs}\n";
        cuerpo += "<b>Se falhar na resistencia:</b> Aterrorizado por 2 turnos (-2 Ataque, -1 AP Max, -2 Resistencia Mental)";
      }
      else
      {
        cuerpo += "<b>Tipo:</b> Melee\n";
        cuerpo += "<b>Objetivo:</b> 1 enemigo en alcance frontal\n";
        cuerpo += $"<b>Tirada:</b> 1d20 + <color=#ea0606>Fue ({fuerzaActual})</color> + Ataque ({ataqueActual}){bonusAtaqueTxt} vs Defensa. Pifia: 1-2. Crítico: {criticoMin}-20\n";
        cuerpo += $"<b>Daño:</b> {rangoDanio} + <color=#ea0606>Fue ({fuerzaActual})</color> | <b>Tipo:</b> Cortante\n";
        cuerpo += $"<b>Penetracion de armadura:</b> {penetracionArmadura}\n";
        cuerpo += "<b>Si mata al objetivo:</b> todos los enemigos hacen TS\n";
        cuerpo += $"{lineaSalvacionEs}\n";
        cuerpo += "<b>Si falla TS:</b> Aterrorizado por 2 turnos (-2 Ataque, -1 AP Max, -2 TS Mental)";
      }

      string costos = esIngles
        ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}\n- Effortable: Yes ({esforzable})"
        : esPortugues
          ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}\n- Esforcavel: Sim ({esforzable})"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}\n- Esforzable: Si ({esforzable})";

      txtDescripcion = ConstruirDescripcionEstandar(
        esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
        esIngles
          ? "A crushing execution cut that can panic the entire enemy side on kill."
          : esPortugues
            ? "Um corte de execucao brutal que pode causar panico no lado inimigo ao matar."
          : "Un corte de ejecución brutal que puede entrar en panico al lado enemigo al matar.",
        cuerpo,
        costos,
        "#5dade2");

      string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
      string subtitulo = esIngles
        ? "High-damage melee attack; on kill, may terrify all enemies."
        : esPortugues
          ? "Ataque corpo a corpo de alto dano; ao matar, pode aterrorizar todos os inimigos."
          : "Ataque melee de alto daño; al matar, puede aterrorizar a todos los enemigos.";

      string cuerpoFormato = "";
      if (esIngles)
      {
        cuerpoFormato += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Melee attack</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Target:</b></color> <color={colorValor}>1 enemy or obstacle in front range</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Roll:</b></color> <color={colorValor}>1d20 + <color={colorFuerza}>Strength ({fuerzaActual})</color>{ataqueTxt} vs Defense. Fumble: {pifiaPorcentaje}%. Crit: {criticoPorcentaje}%</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Damage:</b></color> <color={colorValor}>{rangoDanio} + <color={colorFuerza}>Strength ({fuerzaActual})</color>. Type: Slashing</color>\n";
        if (penetracionArmadura > 0)
        {
          cuerpoFormato += $"<color={colorEncabezado}><b>Armor penetration:</b></color> <color={colorValor}>{penetracionArmadura}</color>\n";
        }
        cuerpoFormato += $"<color={colorEncabezado}><b>On kill:</b></color> <color={colorValor}>All enemies roll Mental save vs DC {dcMiedo}</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>On failed save:</b></color> <color={colorValor}>{iconoDebuff} Terrified for 2 turns: -2 Attack, -1 Max AP, -2 Mental Save</color>";
      }
      else if (esPortugues)
      {
        cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Ataque corpo a corpo</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Alvo:</b></color> <color={colorValor}>1 inimigo ou obstáculo no alcance frontal</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Rolagem:</b></color> <color={colorValor}>1d20 + <color={colorFuerza}>Força ({fuerzaActual})</color>{ataqueTxt} vs Defesa. Falha crítica: {pifiaPorcentaje}%. Crítico: {criticoPorcentaje}%</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Daño:</b></color> <color={colorValor}>{rangoDanio} + <color={colorFuerza}>Força ({fuerzaActual})</color>. Tipo: Cortante</color>\n";
        if (penetracionArmadura > 0)
        {
          cuerpoFormato += $"<color={colorEncabezado}><b>Penetração de armadura:</b></color> <color={colorValor}>{penetracionArmadura}</color>\n";
        }
        cuerpoFormato += $"<color={colorEncabezado}><b>Ao matar:</b></color> <color={colorValor}>Todos os inimigos fazem resistência Mental vs CD {dcMiedo}</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Se falhar:</b></color> <color={colorValor}>{iconoDebuff} Aterrorizado por 2 turnos: -2 Ataque, -1 AP Max, -2 Resistência Mental</color>";
      }
      else
      {
        cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Ataque melee</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Objetivo:</b></color> <color={colorValor}>1 enemigo u obstáculo en alcance frontal</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Tirada:</b></color> <color={colorValor}>1d20 + <color={colorFuerza}>Fuerza ({fuerzaActual})</color>{ataqueTxt} vs Defensa. Pifia: {pifiaPorcentaje}%. Crítico: {criticoPorcentaje}%</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Daño:</b></color> <color={colorValor}>{rangoDanio} + <color={colorFuerza}>Fuerza ({fuerzaActual})</color>. Tipo: Cortante</color>\n";
        if (penetracionArmadura > 0)
        {
          cuerpoFormato += $"<color={colorEncabezado}><b>Penetración de armadura:</b></color> <color={colorValor}>{penetracionArmadura}</color>\n";
        }
        cuerpoFormato += $"<color={colorEncabezado}><b>Si mata:</b></color> <color={colorValor}>Todos los enemigos hacen TS Mental vs DC {dcMiedo}</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Si falla:</b></color> <color={colorValor}>{iconoDebuff} Aterrorizado por 2 turnos: -2 Ataque, -1 AP Max, -2 TS Mental</color>";
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
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +4 damage.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +2 attack roll bonus.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+2 save DC on kill) or Option B (+5% Crit).</color>"; }
      }
      else if (esPortugues)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +4 de dano.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +2 no bonus de ataque.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+2 CD do efeito ao matar) ou Opcao B (+5% Critico).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +4 de daño.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +2 al bono de ataque.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A (+2 DC del efecto al matar) u Opción B (+5% Crítico).</color>"; }
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

       float defensaObjetivo = objetivo.ObtenerdefensaActual();
       print("Defensa: "+ defensaObjetivo);

       int danioMarca = 0;
       int bonusAtaqueTotal = bonusAtaque;
       if(NIVEL > 2)
       {bonusAtaqueTotal += 2;}
       float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab;
       if(NIVEL == 5)
       {criticoRango += 1;}
       if(ChequearTieneSiguesTu(objetivo))
       {
         bonusAtaqueTotal += 5;
         danioMarca = 8;
         Destroy(objetivo.GetComponent<MarcaSiguesTu>());

         if(gameObject.GetComponent<SiguesTu>().NIVEL > 1)
         { criticoRango +=2;    }
         if(gameObject.GetComponent<SiguesTu>().NIVEL > 2)
         { danioMarca +=2;    }
       }

      

      
       int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, scEstaUnidad.mod_CarFuerza, bonusAtaqueTotal, criticoRango, objetivo, 1); // En habilidades caballero +1 a pifia, debilidad de Caballero
       print("Resultado tirada "+resultadoTirada);


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
        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + scEstaUnidad.mod_CarFuerza + 5 + danioMarca;
        if (NIVEL > 1) { danio += 4; }

        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        danio -= danio / 2; //Reduce 50% por roce

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

        VFXAplicar(objetivo.gameObject);


      }
      else if (resultadoTirada == 2)
      {//GOLPE
        print("Golpe");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + scEstaUnidad.mod_CarFuerza + 5 + danioMarca;
        if (NIVEL > 1) { danio += 4; }

        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

        VFXAplicar(objetivo.gameObject);


      }
      else if (resultadoTirada == 3)
      {//CRITICO
        print("Crítico");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + scEstaUnidad.mod_CarFuerza + 5 + danioMarca;
        if (NIVEL > 1) { danio += 4; }

        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);
        
                 VFXAplicar(objetivo.gameObject);

      }
     
    

       fueElObjetivoAsesinado = objetivo;
      Invoke("ChequeoMuerteObjetivo", 3.0f); //Chequea si el objetivo murió, y aplica efectos de ser así.





        objetivo.AplicarDebuffPorAtaquesreiterados(1);
       }   
     else if (obj is Obstaculo) //Acá van los efectos a Obstaculos
     {
       Obstaculo objetivo = (Obstaculo)obj;
       //---


       float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+scEstaUnidad.mod_CarFuerza;
       danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
       
       objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
     }
    }
     Unidad fueElObjetivoAsesinado;
    bool ChequearTieneSiguesTu(Unidad obj)
    { 
      if(obj.GetComponent<MarcaSiguesTu>() != null)
      {
        if(obj.GetComponent<MarcaSiguesTu>().quienMarco == scEstaUnidad)
        {
          return true;
        }
      
      }
      return false;
    }
    void VFXAplicar(GameObject objetivo)
    {
         VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_Partir");

    for (int i = 0; i < 3; i++)
    {
      GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
      vfx.transform.parent = objetivo.transform;
     
      //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
      Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
      RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);
    }

    }

    //Provisorio
    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();
     void ChequeoMuerteObjetivo()
  {
    bool aplicarEfectos = false;
    if (fueElObjetivoAsesinado == null)
    {
      aplicarEfectos = true; //Si no existe se asume que murio
    } //Si no había objetivo, no hace nada
    else if (fueElObjetivoAsesinado.HP_actual < 1)
    {
      aplicarEfectos = true; //Si no tiene vida, murio
    }

    if (aplicarEfectos)
    { 
      PARTIREfectoAEnemigosPorMuerte();
    }
    fueElObjetivoAsesinado = null;
  }
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
    private int AumentarRangoMelee() //aumenta el rango melee si no hay nada en frente ni filas adyacentes al origen de la habilidad
  {

    LadoManager scLado = Origen.ladoOpuesto.GetComponent<LadoManager>();

    int posYorigen = scEstaUnidad.CasillaPosicion.posY;


    List<Casilla> casillasAdyacentesyFrenteColumna1 = new List<Casilla>();
    List<Casilla> casillasAdyacentesyFrenteColumna2 = new List<Casilla>();

    foreach (Transform child in Origen.ladoOpuesto.transform) //Itera en cada casilla del lado opuesto
    {
      Casilla cas = child.GetComponent<Casilla>();

      if (cas.posX == 3) //Columna 1 (frente)
      {
        int calculo = Math.Abs(cas.posY - posYorigen); //distancia en Y al origen para calcular adyacentes o frontal

        if (calculo < 2)
        {
          casillasAdyacentesyFrenteColumna1.Add(cas);
        }
      }

      if (cas.posX == 2) //Columna 2 (medio)
      {
        int calculo = Math.Abs(cas.posY - posYorigen); //distancia en Y al origen para calcular adyacentes o frontal

        if (calculo < 2)
        {
          casillasAdyacentesyFrenteColumna2.Add(cas);
        }
      }


    }

    //Se fija si las 3 casillas de la columna 1 están vacias
    foreach (Casilla cas in casillasAdyacentesyFrenteColumna1)
    {
      if (cas.BloqueaAvanceMeleeDesdeFila(posYorigen, scEstaUnidad)) //si alguna de las 3 tiene algo, no aumenta el rango melee
      {
        return 0;
      }
    }
    foreach (Casilla casOsc in casillasAdyacentesyFrenteColumna1) //si ninguna de las tres tiene algo, las oscurece
    { casOsc.ActivarCapaColorNegro(); }





    foreach (Casilla cas in casillasAdyacentesyFrenteColumna2)
    {
      if (cas.BloqueaAvanceMeleeDesdeFila(posYorigen, scEstaUnidad)) //y si alguna de las 3 tiene algo, aumenta solo en 1
      {
        return 1;
      }
    }
    foreach (Casilla casOsc in casillasAdyacentesyFrenteColumna2) //si ninguna de las tres tiene algo, las oscurece
    { casOsc.ActivarCapaColorNegro(); }




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




    void PARTIREfectoAEnemigosPorMuerte()
    {
       
        List<Unidad> enemigos = new List<Unidad>();

        foreach(Casilla cas in scEstaUnidad.CasillaPosicion.ObtenerCasillasLadoOpuesto())
        {
           if(cas.Presente != null)
           {
             if(cas.Presente.GetComponent<Unidad>() != null)
             {
                Unidad uni = cas.Presente.GetComponent<Unidad>();
                int nDif = 13;
                if(NIVEL == 4){nDif += 2;}

                if(uni.TiradaSalvacion(3, nDif))
                {
                    /////////////////////////////////////////////
                    //BUFF ---- Así se aplica un buff/debuff
                    Buff buff = new Buff();
                    buff.buffNombre = "Aterrorizado";
                    buff.boolfDebufftBuff = false;
                    buff.DuracionBuffRondas = 2;
                    buff.cantAtaque -= 2;
                    buff.cantAPMax -= 1;
                    buff.cantTsMental -= 2;
                    buff.AplicarBuff(uni);
                    // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
                    Buff buffComponent = ComponentCopier.CopyComponent(buff, uni.gameObject);
                    
                    VFXAplicarEnemigo(uni.gameObject);
                 
                }
                else
                {
                   // uni.GenerarTextoFlotante("Resiste Aterrorizado", Color.cyan);
                }


             }



           }         



        }




    }
}








