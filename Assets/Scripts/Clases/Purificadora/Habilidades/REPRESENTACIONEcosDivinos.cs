using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONEcosDivinos : Habilidad
{
   

    
    public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Purificadora_EcosDivinos");
      ActualizarDescripcion();
      IDenClase = 2;
      
    }

    public bool seusoEsteTurno = false;

  public override void ActualizarDescripcion()
  {
    if (NIVEL < 2)
    {
      txtDescripcion = "<color=#5dade2><b>Ecos Divinos I</b></color>\n\n";
      txtDescripcion += "<i>(Pasiva) Cada turno genera un Eco divino en cualquier lado de la batalla, dañando a los enemigos o curando a aliados que los tocan.</i>\n\n";
      txtDescripcion += "<i>A aliados: Cura 1-10 y suma +1 Valentía. Si es la Purificadora, gana 1 Fervor. Curación mágica.</i>\n\n";
      txtDescripcion += "<i>A enemigos: Causa 1-10 daño Divino.</i>\n\n";

      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +2 Daño y Curación</color>\n\n";
          }
        }
      }

    }
    if (NIVEL == 2)
    {
      txtDescripcion = "<color=#5dade2><b>Ecos Divinos II</b></color>\n\n";
      txtDescripcion += "<i>(Pasiva) Cada turno genera un Eco divino en cualquier lado de la batalla, dañando a los enemigos o curando a aliados que los tocan.</i>\n\n";
      txtDescripcion += "<i>A aliados: Cura 3-12 y suma +1 Valentía. Si es la Purificadora, gana 1 Fervor. Curación mágica.</i>\n\n";
      txtDescripcion += "<i>A enemigos: Causa 3-12 daño Divino.</i>\n\n";

      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +2 Daño y Curación</color>\n\n";
          }
        }
      }
    }
    if (NIVEL == 3)
    {
      txtDescripcion = "<color=#5dade2><b>Ecos Divinos III</b></color>\n\n";
      txtDescripcion += "<i>(Pasiva) Cada turno genera un Eco divino en cualquier lado de la batalla, dañando a los enemigos o curando a aliados que los tocan.</i>\n\n";
      txtDescripcion += "<i>A aliados: Cura 5-14 y suma +1 Valentía. Si es la Purificadora, gana 1 Fervor. Curación mágica.</i>\n\n";
      txtDescripcion += "<i>A enemigos: Causa 5-14 daño Divino.</i>\n\n";
      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Opción A: +5 Curación.</color>\n\n";
            txtDescripcion += $"<color=#dfea02>-Opción B: +5 Daño.</color>\n";
          }
        }
      }
    }
    if (NIVEL == 4)
    {
      txtDescripcion = "<color=#5dade2><b>Ecos Divinos IV a</b></color>\n\n";
      txtDescripcion += "<i>(Pasiva) Cada turno genera un Eco divino en cualquier lado de la batalla, dañando a los enemigos o curando a aliados que los tocan.</i>\n\n";
      txtDescripcion += "<i>A aliados: Cura 10-19 y suma +1 Valentía. Si es la Purificadora, gana 1 Fervor. Curación mágica.</i>\n\n";
      txtDescripcion += "<i>A enemigos: Causa 5-14 daño Divino.</i>\n\n";
    }
    if (NIVEL == 5)
    {
      txtDescripcion = "<color=#5dade2><b>Ecos Divinos IV b</b></color>\n\n";
      txtDescripcion += "<i>(Pasiva) Cada turno genera un Eco divino en cualquier lado de la batalla, dañando a los enemigos o curando a aliados que los tocan.</i>\n\n";
      txtDescripcion += "<i>A aliados: Cura 5-14 y suma +1 Valentía. Si es la Purificadora, gana 1 Fervor. Curación mágica.</i>\n\n";
      txtDescripcion += "<i>A enemigos: Causa 10-19 daño Divino.</i>\n\n";
    }
       
      if (TRADU.i.nIdioma == 2) // English translation
      {
        if (NIVEL < 2)
        {
          txtDescripcion = "<color=#5dade2><b>Divine Echoes I</b></color>\n\n";
          txtDescripcion += "<i>(Passive) Each turn generates a Divine Echo anywhere on the battlefield, damaging enemies or healing allies who touch them.</i>\n\n";
          txtDescripcion += "<i>To allies: Heals 1d10 and adds +1 Valour. If it's the Purifier, gains 1 Fervor. Magical healing.</i>\n\n";
          txtDescripcion += "<i>To enemies: Deals 1d10 Divine damage.</i>\n\n";

          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Next Level: +2 Damage and Healing</color>\n\n";
              }
            }
          }
        }
        if (NIVEL == 2)
        {
          txtDescripcion = "<color=#5dade2><b>Divine Echoes II</b></color>\n\n";
          txtDescripcion += "<i>(Passive) Each turn generates a Divine Echo anywhere on the battlefield, damaging enemies or healing allies who touch them.</i>\n\n";
          txtDescripcion += "<i>To allies: Heals 1d10+2 and adds +1 Valour. If it's the Purifier, gains 1 Fervor. Magical healing.</i>\n\n";
          txtDescripcion += "<i>To enemies: Deals 1d10+2 Divine damage.</i>\n\n";

          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Next Level: +2 Damage and Healing</color>\n\n";
              }
            }
          }
        }
        if (NIVEL == 3)
        {
          txtDescripcion = "<color=#5dade2><b>Divine Echoes III</b></color>\n\n";
          txtDescripcion += "<i>(Passive) Each turn generates a Divine Echo anywhere on the battlefield, damaging enemies or healing allies who touch them.</i>\n\n";
          txtDescripcion += "<i>To allies: Heals 1d10+4 and adds +1 Valour. If it's the Purifier, gains 1 Fervor. Magical healing.</i>\n\n";
          txtDescripcion += "<i>To enemies: Deals 1d10+4 Divine damage.</i>\n\n";
          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Option A: +5 Healing.</color>\n\n";
                txtDescripcion += $"<color=#dfea02>-Option B: +5 Damage.</color>\n";
              }
            }
          }
        }
        if (NIVEL == 4)
        {
          txtDescripcion = "<color=#5dade2><b>Divine Echoes IV a</b></color>\n\n";
          txtDescripcion += "<i>(Passive) Each turn generates a Divine Echo anywhere on the battlefield, damaging enemies or healing allies who touch them.</i>\n\n";
          txtDescripcion += "<i>To allies: Heals 1d10+9 and adds +1 Valour. If it's the Purifier, gains 1 Fervor. Magical healing.</i>\n\n";
          txtDescripcion += "<i>To enemies: Deals 1d10+4 Divine damage.</i>\n\n";
        }
        if (NIVEL == 5)
        {
          txtDescripcion = "<color=#5dade2><b>Divine Echoes IV b</b></color>\n\n";
          txtDescripcion += "<i>(Passive) Each turn generates a Divine Echo anywhere on the battlefield, damaging enemies or healing allies who touch them.</i>\n\n";
          txtDescripcion += "<i>To allies: Heals 1d10+4 and adds +1 Valour. If it's the Purifier, gains 1 Fervor. Magical healing.</i>\n\n";
          txtDescripcion += "<i>To enemies: Deals 1d10+9 Divine damage.</i>\n\n";
        }
      }
      if (TRADU.i.nIdioma == 3) // Portuguese translation
      {
        if (NIVEL < 2)
        {
          txtDescripcion = "<color=#5dade2><b>Ecos Divinos I</b></color>\n\n";
          txtDescripcion += "<i>(Passiva) A cada turno gera um Eco divino em qualquer lado da batalha, causando dano aos inimigos ou curando aliados que o tocam.</i>\n\n";
          txtDescripcion += "<i>Em aliados: Cura 1d10 e soma +1 Valentia. Se for a Purificadora, ganha 1 Fervor. Cura magica.</i>\n\n";
          txtDescripcion += "<i>Em inimigos: Causa 1d10 de dano Divino.</i>\n\n";

          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Proximo Nivel: +2 Dano e Cura</color>\n\n";
              }
            }
          }
        }
        if (NIVEL == 2)
        {
          txtDescripcion = "<color=#5dade2><b>Ecos Divinos II</b></color>\n\n";
          txtDescripcion += "<i>(Passiva) A cada turno gera um Eco divino em qualquer lado da batalha, causando dano aos inimigos ou curando aliados que o tocam.</i>\n\n";
          txtDescripcion += "<i>Em aliados: Cura 1d10+2 e soma +1 Valentia. Se for a Purificadora, ganha 1 Fervor. Cura magica.</i>\n\n";
          txtDescripcion += "<i>Em inimigos: Causa 1d10+2 de dano Divino.</i>\n\n";

          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Proximo Nivel: +2 Dano e Cura</color>\n\n";
              }
            }
          }
        }
        if (NIVEL == 3)
        {
          txtDescripcion = "<color=#5dade2><b>Ecos Divinos III</b></color>\n\n";
          txtDescripcion += "<i>(Passiva) A cada turno gera um Eco divino em qualquer lado da batalha, causando dano aos inimigos ou curando aliados que o tocam.</i>\n\n";
          txtDescripcion += "<i>Em aliados: Cura 1d10+4 e soma +1 Valentia. Se for a Purificadora, ganha 1 Fervor. Cura magica.</i>\n\n";
          txtDescripcion += "<i>Em inimigos: Causa 1d10+4 de dano Divino.</i>\n\n";
          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Opcao A: +5 Cura.</color>\n\n";
                txtDescripcion += $"<color=#dfea02>-Opcao B: +5 Dano.</color>\n";
              }
            }
          }
        }
        if (NIVEL == 4)
        {
          txtDescripcion = "<color=#5dade2><b>Ecos Divinos IV a</b></color>\n\n";
          txtDescripcion += "<i>(Passiva) A cada turno gera um Eco divino em qualquer lado da batalha, causando dano aos inimigos ou curando aliados que o tocam.</i>\n\n";
          txtDescripcion += "<i>Em aliados: Cura 1d10+9 e soma +1 Valentia. Se for a Purificadora, ganha 1 Fervor. Cura magica.</i>\n\n";
          txtDescripcion += "<i>Em inimigos: Causa 1d10+4 de dano Divino.</i>\n\n";
        }
        if (NIVEL == 5)
        {
          txtDescripcion = "<color=#5dade2><b>Ecos Divinos IV b</b></color>\n\n";
          txtDescripcion += "<i>(Passiva) A cada turno gera um Eco divino em qualquer lado da batalha, causando dano aos inimigos ou curando aliados que o tocam.</i>\n\n";
          txtDescripcion += "<i>Em aliados: Cura 1d10+4 e soma +1 Valentia. Se for a Purificadora, ganha 1 Fervor. Cura magica.</i>\n\n";
          txtDescripcion += "<i>Em inimigos: Causa 1d10+9 de dano Divino.</i>\n\n";
        }
      }
      AplicarDescripcionEstandar();
      }

    private void AplicarDescripcionEstandar()
    {
      string rangoAliados = RangoAliados();
      string rangoEnemigos = RangoEnemigos();

      if (TRADU.i != null && TRADU.i.nIdioma == 2)
      {
        string pasiva = TerminoDescripcion(TerminoDescripcionId.Pasiva, "Passive");
        string ecoDivino = TerminoDescripcion(TerminoDescripcionId.EcoDivino, "Divine Echo");
        string valentia = TerminoDescripcion(TerminoDescripcionId.Valentia, "Valour", "Valentía");
        string fervor = TerminoDescripcion(TerminoDescripcionId.Fervor, "Fervor");
        string danioDivino = TerminoDescripcion(TerminoDescripcionId.DanioDivino, "Divine damage", "dano_divino");
        string proximaMejora = null;
        if (DebeMostrarProximaMejoraDescripcion())
        {
          if (NIVEL <= 2) { proximaMejora = "+2 damage and healing."; }
          else if (NIVEL == 3) { proximaMejora = "Option A: +5 healing.\nOption B: +5 damage."; }
        }

        txtDescripcion = ConstruirDescripcionNormalizadaIngles(
          $"Divine Echoes {SufijoNivel()}",
          $"{pasiva}: Creates one {ecoDivino} each turn.",
          new[]
          {
            LineaDescripcion("Effect", $"Creates 1 {ecoDivino} each turn on a random tile on either side of the battlefield."),
            LineaDescripcion("On contact", ""),
            LineaDescripcion("Allies", $"Restores {rangoAliados} HP and grants +1 {valentia}; the Purifier also gains +1 {fervor}.", 1),
            LineaDescripcion("Enemies", $"Suffer {rangoEnemigos} {danioDivino}.", 1)
          },
          proximaMejora,
          costoSuperior: "");
        return;
      }

      if (TRADU.i != null && TRADU.i.nIdioma == 3)
      {
        string passiva = TerminoDescripcion(TerminoDescripcionId.Pasiva, "Passiva");
        string ecoDivino = TerminoDescripcion(TerminoDescripcionId.EcoDivino, "Eco Divino");
        string valentia = TerminoDescripcion(TerminoDescripcionId.Valentia, "Valentia", "Valentía");
        string fervor = TerminoDescripcion(TerminoDescripcionId.Fervor, "Fervor");
        string danoDivino = TerminoDescripcion(TerminoDescripcionId.DanioDivino, "dano Divino", "dano_divino");
        string proximaMejora = null;
        if (DebeMostrarProximaMejoraDescripcion())
        {
          if (NIVEL <= 2) { proximaMejora = "Próximo nível: +2 de dano e cura."; }
          else if (NIVEL == 3) { proximaMejora = "Próximo nível: Opção A: +5 de cura.\nOpção B: +5 de dano."; }
        }

        txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(
          $"Ecos Divinos {SufijoNivel()}",
          $"{passiva}: cria um {ecoDivino} a cada turno.",
          new[]
          {
            LineaDescripcion("Efeito", $"Cria 1 {ecoDivino} a cada turno em uma célula aleatória de qualquer lado do campo de batalha."),
            LineaDescripcion("Ao contato", ""),
            LineaDescripcion("Aliados", $"Restaura {rangoAliados} HP e concede +1 {valentia}; a Purificadora também ganha +1 {fervor}.", 1),
            LineaDescripcion("Inimigos", $"Sofrem {rangoEnemigos} {danoDivino}.", 1)
          },
          proximaMejora,
          costoSuperior: "");
        return;
      }

      {
        string pasiva = TerminoDescripcion(TerminoDescripcionId.Pasiva, "Pasiva");
        string ecoDivino = TerminoDescripcion(TerminoDescripcionId.EcoDivino, "Eco Divino");
        string valentia = TerminoDescripcion(TerminoDescripcionId.Valentia, "Valentía", "Valentía");
        string fervor = TerminoDescripcion(TerminoDescripcionId.Fervor, "Fervor");
        string danioDivino = TerminoDescripcion(TerminoDescripcionId.DanioDivino, "daño Divino", "dano_divino");
        string proximaMejora = null;
        if (DebeMostrarProximaMejoraDescripcion())
        {
          if (NIVEL <= 2) { proximaMejora = "Próximo nivel: +2 de daño y curación."; }
          else if (NIVEL == 3) { proximaMejora = "Próximo nivel: Opción A: +5 de curación.\nOpción B: +5 de daño."; }
        }

        txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(
          $"Ecos Divinos {SufijoNivel()}",
          $"{pasiva}: crea un {ecoDivino} cada turno.",
          new[]
          {
            LineaDescripcion("Efecto", $"Crea 1 {ecoDivino} cada turno en una casilla aleatoria de cualquier lado del campo de batalla."),
            LineaDescripcion("Al contacto", ""),
            LineaDescripcion("Aliados", $"Restaura {rangoAliados} HP y otorga +1 {valentia}; la Purificadora también gana +1 {fervor}.", 1),
            LineaDescripcion("Enemigos", $"Sufren {rangoEnemigos} {danioDivino}.", 1)
          },
          proximaMejora,
          costoSuperior: "");
        return;
      }

      string titulo = $"Ecos Divinos {SufijoNivel()}";
      string subtitulo = "<color=#4f5552>Pasiva: crea un eco por turno.</color>";
      string cuerpo = "<color=#44d3ec><b>Tipo:</b></color> <color=#ffffff>Pasiva</color>\n" +
                      "<color=#44d3ec><b>Frecuencia:</b></color> <color=#ffffff>1 eco por turno.</color>\n" +
                      $"<color=#44d3ec><b>Aliados:</b></color> <color=#ffffff>cura {rangoAliados}, +1 Valentía; si es la Purificadora, +1 Fervor.</color>\n" +
                      $"<color=#44d3ec><b>Enemigos:</b></color> <color=#ffffff>{rangoEnemigos} daño Divino.</color>";

      string proximo = TextoProximoNivel();
      if (!string.IsNullOrEmpty(proximo)) { cuerpo += "\n\n" + proximo; }

      if (TRADU.i.nIdioma == 2)
      {
        titulo = $"Divine Echoes {SufijoNivel()}";
        subtitulo = "<color=#4f5552>Passive: creates one echo each turn.</color>";
        cuerpo = "<color=#44d3ec><b>Type:</b></color> <color=#ffffff>Passive</color>\n" +
                 "<color=#44d3ec><b>Frequency:</b></color> <color=#ffffff>1 echo each turn.</color>\n" +
                 $"<color=#44d3ec><b>Allies:</b></color> <color=#ffffff>heals {rangoAliados}, +1 Valour; if it is the Purifier, +1 Fervor.</color>\n" +
                 $"<color=#44d3ec><b>Enemies:</b></color> <color=#ffffff>{rangoEnemigos} Divine damage.</color>";
        proximo = TextoProximoNivel();
        if (!string.IsNullOrEmpty(proximo)) { cuerpo += "\n\n" + proximo; }
      }
      else if (TRADU.i.nIdioma == 3)
      {
        titulo = $"Ecos Divinos {SufijoNivel()}";
        subtitulo = "<color=#4f5552>Passiva: cria um eco por turno.</color>";
        cuerpo = "<color=#44d3ec><b>Tipo:</b></color> <color=#ffffff>Passiva</color>\n" +
                 "<color=#44d3ec><b>Frequencia:</b></color> <color=#ffffff>1 eco por turno.</color>\n" +
                 $"<color=#44d3ec><b>Aliados:</b></color> <color=#ffffff>cura {rangoAliados}, +1 Valentía; se for a Purificadora, +1 Fervor.</color>\n" +
                 $"<color=#44d3ec><b>Inimigos:</b></color> <color=#ffffff>{rangoEnemigos} dano Divino.</color>";
        proximo = TextoProximoNivel();
        if (!string.IsNullOrEmpty(proximo)) { cuerpo += "\n\n" + proximo; }
      }

      txtDescripcion = ConstruirDescripcionEstandar($"<size=115%>{titulo}</size>", subtitulo, cuerpo, "", "#5dade2");
    }

    private string RangoAliados()
    {
      if (NIVEL < 2) { return "1-10"; }
      if (NIVEL == 2) { return "3-12"; }
      if (NIVEL == 4) { return "10-19"; }
      return "5-14";
    }

    private string RangoEnemigos()
    {
      if (NIVEL < 2) { return "1-10"; }
      if (NIVEL == 2) { return "3-12"; }
      if (NIVEL == 5) { return "10-19"; }
      return "5-14";
    }

    private string SufijoNivel()
    {
      if (NIVEL < 2) { return "I"; }
      if (NIVEL == 2) { return "II"; }
      if (NIVEL == 3) { return "III"; }
      if (NIVEL == 4) { return "IV a"; }
      return "IV b";
    }

    private string TextoProximoNivel()
    {
      if (!EsEscenaCampaña() || CampaignManager.Instance.scMenuPersonajes.pSel == null || CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad <= 0)
      {
        return "";
      }

      if (TRADU.i.nIdioma == 2)
      {
        if (NIVEL < 2 || NIVEL == 2) { return "<color=#dfea02>Next Level: +2 damage and healing.</color>"; }
        if (NIVEL == 3) { return "<color=#dfea02>Option A: +5 healing.\nOption B: +5 damage.</color>"; }
      }
      else if (TRADU.i.nIdioma == 3)
      {
        if (NIVEL < 2 || NIVEL == 2) { return "<color=#dfea02>Proximo Nivel: +2 dano e cura.</color>"; }
        if (NIVEL == 3) { return "<color=#dfea02>Opcao A: +5 cura.\nOpcao B: +5 dano.</color>"; }
      }
      else
      {
        if (NIVEL < 2 || NIVEL == 2) { return "<color=#dfea02>Próximo Nivel: +2 daño y curación.</color>"; }
        if (NIVEL == 3) { return "<color=#dfea02>Opción A: +5 curación.\nOpción B: +5 daño.</color>"; }
      }

      return "";
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}




