using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONAcumulacionProtegida : Habilidad
{
   

    
    public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Canalizador_AcumulacionProtegida");
      ActualizarDescripcion();
      IDenClase = 1;
      
    }

  
    public override void  ActualizarDescripcion()
    {
      if (TRADU.i != null && TRADU.i.nIdioma == 2)
      {
        var statsUI = ObtenerStatsDescripcionUI();
        string titulo = "Protected Gathering I";
        if (NIVEL == 2) { titulo = "Protected Gathering II"; }
        else if (NIVEL == 3) { titulo = "Protected Gathering III"; }
        else if (NIVEL == 4) { titulo = "Protected Gathering IV a"; }
        else if (NIVEL == 5) { titulo = "Protected Gathering IV b"; }

        int barreraExtraIngles = NIVEL == 4 ? 6 : NIVEL > 1 ? 2 : 0;
        int bonusMental = NIVEL > 2 ? 2 : 1;
        string energia = TerminoDescripcion(TerminoDescripcionId.Energia, "Energy tier", "Estado_acumularenergia");
        string barrera = TerminoDescripcion(TerminoDescripcionId.Barrera, "Barrier", "Estado_barrera");
        string salvacionMental = TerminoDescripcion(TerminoDescripcionId.SalvacionMental, "Mental saves", "ic_mental");
        string formula = $"1 + Power ({statsUI.Poder}) + 3 x current {energia}";
        if (barreraExtraIngles > 0) { formula += $" + {barreraExtraIngles}"; }

        var lineas = new List<LineaDescripcionNormalizada>
        {
          LineaDescripcion("Trigger", "Uses Gather Energy."),
          LineaDescripcion("Effect", $"Gains {formula} as {barrera} and +{bonusMental} to {salvacionMental}."),
          LineaDescripcion("Duration", "Until the start of the next turn.")
        };
        if (NIVEL == 5)
        {
          lineas.Add(LineaDescripcion("While Gathering", $"+1 {TerminoDescripcion(TerminoDescripcionId.PuntosAccion, "Max AP", "ap")}."));
        }

        string proximaMejora = null;
        if (DebeMostrarProximaMejoraDescripcion())
        {
          if (NIVEL < 2) { proximaMejora = "+2 Barrier."; }
          else if (NIVEL == 2) { proximaMejora = "+1 additional Mental save."; }
          else if (NIVEL == 3) { proximaMejora = "Option A: +4 additional Barrier.\nOption B: +1 Max AP while Gathering."; }
        }

        txtDescripcion = ConstruirDescripcionNormalizadaIngles(
          titulo,
          "Passive: Protects the Channeler while gathering Energy.",
          lineas,
          proximaMejora);
        return;
      }

      {
        bool pt = TRADU.i != null && TRADU.i.nIdioma == 3;
        var statsUI = ObtenerStatsDescripcionUI();
        string titulo = pt ? "Acúmulo Protegido I" : "Acumulación Protegida I";
        if (NIVEL == 2) { titulo = pt ? "Acúmulo Protegido II" : "Acumulación Protegida II"; }
        else if (NIVEL == 3) { titulo = pt ? "Acúmulo Protegido III" : "Acumulación Protegida III"; }
        else if (NIVEL == 4) { titulo = pt ? "Acúmulo Protegido IV a" : "Acumulación Protegida IV a"; }
        else if (NIVEL == 5) { titulo = pt ? "Acúmulo Protegido IV b" : "Acumulación Protegida IV b"; }
        int barreraExtraLocalizada = NIVEL == 4 ? 6 : NIVEL > 1 ? 2 : 0;
        int bonusMental = NIVEL > 2 ? 2 : 1;
        string energia = TerminoDescripcion(TerminoDescripcionId.Energia, pt ? "nível de Energia" : "nivel de Energía", "Estado_acumularenergia");
        string barrera = TerminoDescripcion(TerminoDescripcionId.Barrera, pt ? "Barreira" : "Barrera", "Estado_barrera");
        string salvacionMental = TerminoDescripcion(TerminoDescripcionId.SalvacionMental, pt ? "resistências Mentais" : "salvaciones Mentales", "ic_mental");
        string formula = $"1 + Poder ({statsUI.Poder}) + 3 x {(pt ? "nível atual de" : "nivel actual de")} {energia}";
        if (barreraExtraLocalizada > 0) { formula += $" + {barreraExtraLocalizada}"; }
        var lineas = new List<LineaDescripcionNormalizada>
        {
          LineaDescripcion(pt ? "Gatilho" : "Desencadenante", pt ? "Usa Acumular Energia." : "Usa Acumular Energía."),
          LineaDescripcion(pt ? "Efeito" : "Efecto", $"{(pt ? "Recebe" : "Obtiene")} {formula} como {barrera} {(pt ? "e" : "y")} +{bonusMental} {(pt ? "às" : "a las")} {salvacionMental}."),
          LineaDescripcion(pt ? "Duração" : "Duración", pt ? "Até o início do próximo turno." : "Hasta el inicio del próximo turno.")
        };
        if (NIVEL == 5) { lineas.Add(LineaDescripcion(pt ? "Enquanto acumula" : "Mientras acumula", $"+1 {TerminoDescripcion(TerminoDescripcionId.PuntosAccion, "AP Máx.", "ap")}.")); }
        string proximaMejora = null;
        if (DebeMostrarProximaMejoraDescripcion())
        {
          if (NIVEL < 2) { proximaMejora = pt ? "+2 de Barreira." : "+2 de Barrera."; }
          else if (NIVEL == 2) { proximaMejora = pt ? "+1 adicional às resistências Mentais." : "+1 adicional a las salvaciones Mentales."; }
          else if (NIVEL == 3) { proximaMejora = pt ? "Opção A: +4 de Barreira adicionais.\nOpção B: +1 AP Máx. enquanto acumula." : "Opción A: +4 de Barrera adicionales.\nOpción B: +1 AP Máx. mientras acumula."; }
        }
        txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(
          titulo,
          pt ? "Passiva: protege o Canalizador enquanto acumula Energia." : "Pasiva: protege al Canalizador mientras acumula Energía.",
          lineas,
          proximaMejora,
          costoSuperior: string.Empty);
        return;
      }

      if(NIVEL<2)
{
    txtDescripcion = "<color=#5dade2><b>Acumulación Protegida I</b></color>\n\n"; 
    txtDescripcion += "<i>(Pasiva) El Canalizador recubre su cuerpo con energía protectora al acumular poder.</i>\n\n";
    txtDescripcion += "<color=#c8c8c8>Al Acumular Energía, obtiene <b>1 + Pod + 3 x Energía</b> de Barrera y <b>+1 a TS Mental</b> hasta su siguiente turno.</color>\n\n";

    if (EsEscenaCampaña())
    {
        if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
        {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
                txtDescripcion += $"<color=#dfea02>- Próximo Nivel: +2 Barrera al acumular Energía.</color>\n\n";
            }
        }
    }
}
if(NIVEL==2)
{
    txtDescripcion = "<color=#5dade2><b>Acumulación Protegida II</b></color>\n\n"; 
    txtDescripcion += "<i>(Pasiva) El Canalizador recubre su cuerpo con energía protectora al acumular poder.</i>\n\n";
    txtDescripcion += "<color=#c8c8c8>Al Acumular Energía, obtiene <b>1 + Pod + 3 x Energía +2</b> de Barrera y <b>+1 a TS Mental</b> hasta su siguiente turno.</color>\n\n";

    if (EsEscenaCampaña())
    {
        if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
        {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
                txtDescripcion += $"<color=#dfea02>- Próximo Nivel: +1 a TS Mental adicional.</color>\n\n";
            }
        }
    }
}
if(NIVEL==3)
{
    txtDescripcion = "<color=#5dade2><b>Acumulación Protegida III</b></color>\n\n"; 
    txtDescripcion += "<i>(Pasiva) El Canalizador recubre su cuerpo con energía protectora al acumular poder.</i>\n\n";
    txtDescripcion += "<color=#c8c8c8>Al Acumular Energía, obtiene <b>1 + Pod + 3 x Energía +2</b> de Barrera y <b>+2 a TS Mental</b> hasta su siguiente turno.</color>\n\n";

    if (EsEscenaCampaña())
    {
        if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
        {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
                txtDescripcion += $"<color=#dfea02>- Opción A: Si supera la TS Mental de concentración, el atacante recibe 1-10 daño Arcano.</color>\n";
                txtDescripcion += $"<color=#dfea02>- Opción B: Si completa con Éxito la acumulación, obtiene +1 AP ese turno.</color>\n";
            }
        }
    }
}
if(NIVEL==4)
{
    txtDescripcion = "<color=#5dade2><b>Acumulación Protegida IV a</b></color>\n\n"; 
    txtDescripcion += "<i>(Pasiva) El Canalizador recubre su cuerpo con energía protectora al acumular poder.</i>\n\n";
    txtDescripcion += "<color=#c8c8c8>Al Acumular Energía, obtiene <b>1 + Pod + 3 x Energía +6</b> de Barrera y <b>+2 a TS Mental</b> hasta su siguiente turno.</color>\n\n";
}
if(NIVEL==5)
{
    txtDescripcion = "<color=#5dade2><b>Acumulación Protegida IV b</b></color>\n\n"; 
    txtDescripcion += "<i>(Pasiva) El Canalizador recubre su cuerpo con energía protectora al acumular poder.</i>\n\n";
    txtDescripcion += "<color=#c8c8c8>Al Acumular Energía, obtiene <b>1 + Pod + 3 x Energía +2</b> de Barrera y <b>+2 a TS Mental</b> hasta su siguiente turno.</color>\n\n";
    txtDescripcion += "<color=#c8c8c8>Si completa con Éxito la acumulación, obtiene <b>+1 AP</b> ese turno.</color>\n";
}

    if (TRADU.i.nIdioma == 2) // English translation
    {
        if (NIVEL < 2)
        {
            txtDescripcion = "<color=#5dade2><b>Protected Charging I</b></color>\n\n";
            txtDescripcion += "<i>(Passive) The Channeler covers their body with protective energy when Charging power.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>When Charging Energy, gains <b>1 + Power + 3 x Energy</b> Barrier and <b>+1 to Mental Save</b> until their next turn.</color>\n\n";

            if (EsEscenaCampaña())
            {
                if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
                {
                    if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
                    {
                        txtDescripcion += $"<color=#dfea02>- Next Level: +2 Barrier when accumulating Energy.</color>\n\n";
                    }
                }
            }
        }
        if (NIVEL == 2)
        {
            txtDescripcion = "<color=#5dade2><b>Protected Charging II</b></color>\n\n";
            txtDescripcion += "<i>(Passive) The Channeler covers their body with protective energy when Charging power.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>When Charging Energy, gains <b>1 + Power + 3 x Energy +2</b> Barrier and <b>+1 to Mental Save</b> until their next turn.</color>\n\n";

            if (EsEscenaCampaña())
            {
                if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
                {
                    if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
                    {
                        txtDescripcion += $"<color=#dfea02>- Next Level: +1 additional Mental Save.</color>\n\n";
                    }
                }
            }
        }
        if (NIVEL == 3)
        {
            txtDescripcion = "<color=#5dade2><b>Protected Charging III</b></color>\n\n";
            txtDescripcion += "<i>(Passive) The Channeler covers their body with protective energy when Charging power.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>When Charging Energy, gains <b>1 + Power + 3 x Energy +2</b> Barrier and <b>+2 to Mental Save</b> until their next turn.</color>\n\n";

            if (EsEscenaCampaña())
            {
                if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
                {
                    if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
                    {
                        txtDescripcion += $"<color=#dfea02>- Option A: If the Mental Save for concentration is passed, the attacker takes 1d10 Arcane damage.</color>\n";
                        txtDescripcion += $"<color=#dfea02>- Option B: If Charging is successfully completed, gains +1 AP that turn.</color>\n";
                    }
                }
            }
        }
        if (NIVEL == 4)
        {
            txtDescripcion = "<color=#5dade2><b>Protected Charging IV a</b></color>\n\n";
            txtDescripcion += "<i>(Passive) The Channeler covers their body with protective energy when Charging power.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>When Charging Energy, gains <b>1 + Power + 3 x Energy +6</b> Barrier and <b>+2 to Mental Save</b> until their next turn.</color>\n\n";
        }
        if (NIVEL == 5)
        {
            txtDescripcion = "<color=#5dade2><b>Protected Charging IV b</b></color>\n\n";
            txtDescripcion += "<i>(Passive) The Channeler covers their body with protective energy when Charging power.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>When Charging Energy, gains <b>1 + Power + 3 x Energy +2</b> Barrier and <b>+2 to Mental Save</b> until their next turn.</color>\n\n";
            txtDescripcion += "<color=#c8c8c8>If Charging is successfully completed, gains <b>+1 AP</b> that turn.</color>\n";
        }
    }
    if (TRADU.i.nIdioma == 3)
    {
        if (NIVEL < 2)
        {
            txtDescripcion = "<color=#5dade2><b>Acumulacao Protegida I</b></color>\n\n";
            txtDescripcion += "<i>(Passiva) O Canalizador recobre o corpo com energia protetora ao Acumular poder.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>Ao Acumular Energia, ganha <b>1 + Poder + 3 x Energia</b> de Barreira e <b>+1 em TS Mental</b> ate o proximo turno.</color>\n\n";

            if (EsEscenaCampaña())
            {
                if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
                {
                    if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
                    {
                        txtDescripcion += $"<color=#dfea02>- Proximo Nivel: +2 Barreira ao Acumular Energia.</color>\n\n";
                    }
                }
            }
        }
        if (NIVEL == 2)
        {
            txtDescripcion = "<color=#5dade2><b>Acumulacao Protegida II</b></color>\n\n";
            txtDescripcion += "<i>(Passiva) O Canalizador recobre o corpo com energia protetora ao Acumular poder.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>Ao Acumular Energia, ganha <b>1 + Poder + 3 x Energia +2</b> de Barreira e <b>+1 em TS Mental</b> ate o proximo turno.</color>\n\n";

            if (EsEscenaCampaña())
            {
                if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
                {
                    if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
                    {
                        txtDescripcion += $"<color=#dfea02>- Próximo Nivel: +1 TS Mental adicional.</color>\n\n";
                    }
                }
            }
        }
        if (NIVEL == 3)
        {
            txtDescripcion = "<color=#5dade2><b>Acumulacao Protegida III</b></color>\n\n";
            txtDescripcion += "<i>(Passiva) O Canalizador recobre o corpo com energia protetora ao Acumular poder.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>Ao Acumular Energia, ganha <b>1 + Poder + 3 x Energia +2</b> de Barreira e <b>+2 em TS Mental</b> ate o proximo turno.</color>\n\n";

            if (EsEscenaCampaña())
            {
                if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
                {
                    if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
                    {
                        txtDescripcion += $"<color=#dfea02>- Opcao A: Se passar na TS Mental de concentracao, o atacante recebe 1d10 dano Arcano.</color>\n";
                        txtDescripcion += $"<color=#dfea02>- Opcao B: Se concluir a acumulacao com sucesso, ganha +1 AP nesse turno.</color>\n";
                    }
                }
            }
        }
        if (NIVEL == 4)
        {
            txtDescripcion = "<color=#5dade2><b>Acumulacao Protegida IV a</b></color>\n\n";
            txtDescripcion += "<i>(Passiva) O Canalizador recobre o corpo com energia protetora ao Acumular poder.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>Ao Acumular Energia, ganha <b>1 + Poder + 3 x Energia +6</b> de Barreira e <b>+2 em TS Mental</b> ate o proximo turno.</color>\n\n";
        }
        if (NIVEL == 5)
        {
            txtDescripcion = "<color=#5dade2><b>Acumulacao Protegida IV b</b></color>\n\n";
            txtDescripcion += "<i>(Passiva) O Canalizador recobre o corpo com energia protetora ao Acumular poder.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>Ao Acumular Energia, ganha <b>1 + Poder + 3 x Energia +2</b> de Barreira e <b>+2 em TS Mental</b> ate o proximo turno.</color>\n\n";
            txtDescripcion += "<color=#c8c8c8>Se concluir a acumulacao com sucesso, ganha <b>+1 AP</b> nesse turno.</color>\n";
        }
    }

    bool esInglesFormato = TRADU.i != null && TRADU.i.nIdioma == 2;
    bool esPortuguesFormato = TRADU.i != null && TRADU.i.nIdioma == 3;
    string colorEncabezado = "#44d3ec";
    string colorValor = "#ffffff";
    string colorPoder = "#2aa6c8";
    string iconoEnergia = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_acumularenergia\"></voffset></size><space=-0.35em>";
    string tituloFormato = esInglesFormato ? "Protected Gathering" : esPortuguesFormato ? "Acumulacao Protegida" : "Acumulacion Protegida";
    if (NIVEL < 2) { tituloFormato += " I"; }
    else if (NIVEL == 2) { tituloFormato += " II"; }
    else if (NIVEL == 3) { tituloFormato += " III"; }
    else if (NIVEL == 4) { tituloFormato += " IV a"; }
    else if (NIVEL == 5) { tituloFormato += " IV b"; }
    int barreraExtra = NIVEL == 4 ? 6 : NIVEL > 1 ? 2 : 0;
    int tsMental = NIVEL > 2 ? 2 : 1;
    string formulaBarrera = barreraExtra > 0
      ? $"1 + <color={colorPoder}>Power/Poder</color> + 3 x {iconoEnergia} Energy + {barreraExtra}"
      : $"1 + <color={colorPoder}>Power/Poder</color> + 3 x {iconoEnergia} Energy";
    string subtituloFormato = esInglesFormato
      ? "Gain Barrier and Mental Save when gathering Energy."
      : esPortuguesFormato
        ? "Ganha Barreira e Resistencia Mental ao acumular Energia."
        : "Gana Barrera y TS Mental al acumular Energía.";
    string cuerpoFormato = "";
    if (esInglesFormato)
    {
      cuerpoFormato += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Passive buff</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Trigger:</b></color> <color={colorValor}>When using Gather Energy</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Barrier:</b></color> <color={colorValor}>{formulaBarrera} until next turn</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Mental Save:</b></color> <color={colorValor}>+{tsMental} until next turn</color>";
      if (NIVEL == 5) { cuerpoFormato += $"\n<color={colorEncabezado}><b>On completed gathering:</b></color> <color={colorValor}>+1 Max AP that turn</color>"; }
    }
    else if (esPortuguesFormato)
    {
      cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Buff passivo</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Ativa:</b></color> <color={colorValor}>Ao usar Acumular Energia</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Barreira:</b></color> <color={colorValor}>{formulaBarrera} ate o proximo turno</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Resistencia Mental:</b></color> <color={colorValor}>+{tsMental} ate o próximo turno</color>";
      if (NIVEL == 5) { cuerpoFormato += $"\n<color={colorEncabezado}><b>Ao completar acumulacao:</b></color> <color={colorValor}>+1 AP Max nesse turno</color>"; }
    }
    else
    {
      cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Buff pasivo</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Activa:</b></color> <color={colorValor}>Al usar Acumular Energia</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Barrera:</b></color> <color={colorValor}>{formulaBarrera} hasta el próximo turno</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>TS Mental:</b></color> <color={colorValor}>+{tsMental} hasta el próximo turno</color>";
      if (NIVEL == 5) { cuerpoFormato += $"\n<color={colorEncabezado}><b>Al completar acumulacion:</b></color> <color={colorValor}>+1 AP Max ese turno</color>"; }
    }

    txtDescripcion =
      $"<size=115%><color=#5dade2><b>{tituloFormato}</b></color></size>\n\n" +
      $"<color=#8f8f8f><i>{subtituloFormato}</i></color>\n\n" +
      "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n" +
      cuerpoFormato +
      ObtenerBloqueProximoNivelSiCorresponde(esInglesFormato, esPortuguesFormato);

    }

    string ObtenerBloqueProximoNivelSiCorresponde(bool esInglesFormato, bool esPortuguesFormato)
    {
        if (!EsEscenaCampaña()
            || CampaignManager.Instance == null
            || CampaignManager.Instance.scMenuPersonajes == null
            || CampaignManager.Instance.scMenuPersonajes.pSel == null
            || CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad <= 0)
        {
            return string.Empty;
        }

        string texto = ObtenerTextoProximoNivel(esInglesFormato, esPortuguesFormato);
        if (string.IsNullOrWhiteSpace(texto))
        {
            return string.Empty;
        }

        return "\n\n" + texto;
    }

    string ObtenerTextoProximoNivel(bool esInglesFormato, bool esPortuguesFormato)
    {
        if (NIVEL < 2)
        {
            if (esInglesFormato)
            {
                return "<color=#dfea02><b>Next Level:</b></color> <color=#ffffff>+2 Barrier when gathering Energy</color>";
            }

            if (esPortuguesFormato)
            {
                return "<color=#dfea02><b>Proximo Nivel:</b></color> <color=#ffffff>+2 Barreira ao acumular Energia</color>";
            }

            return "<color=#dfea02><b>Próximo Nivel:</b></color> <color=#ffffff>+2 Barrera al acumular Energía</color>";
        }

        if (NIVEL == 2)
        {
            if (esInglesFormato)
            {
                return "<color=#dfea02><b>Next Level:</b></color> <color=#ffffff>+1 additional Mental Save</color>";
            }

            if (esPortuguesFormato)
            {
                return "<color=#dfea02><b>Proximo Nivel:</b></color> <color=#ffffff>+1 Resistencia Mental adicional</color>";
            }

            return "<color=#dfea02><b>Próximo Nivel:</b></color> <color=#ffffff>+1 TS Mental adicional</color>";
        }

        if (NIVEL == 3)
        {
            if (esInglesFormato)
            {
                return "<color=#dfea02><b>Next Level A:</b></color> <color=#ffffff>+4 additional Barrier when gathering Energy</color>\n"
                    + "<color=#dfea02><b>Next Level B:</b></color> <color=#ffffff>+1 Max AP while gathering Energy</color>";
            }

            if (esPortuguesFormato)
            {
                return "<color=#dfea02><b>Proximo Nivel A:</b></color> <color=#ffffff>+4 Barreira adicional ao acumular Energia</color>\n"
                    + "<color=#dfea02><b>Proximo Nivel B:</b></color> <color=#ffffff>+1 AP Max ao acumular Energia</color>";
            }

            return "<color=#dfea02><b>Próximo Nivel A:</b></color> <color=#ffffff>+4 Barrera adicional al acumular Energía</color>\n"
                + "<color=#dfea02><b>Próximo Nivel B:</b></color> <color=#ffffff>+1 AP Max al acumular Energía</color>";
        }

        return string.Empty;
    }


    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}
