using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONExcesoDePoder : Habilidad
{
   

    
    public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Canalizador_ExcesoDePoder");
      ActualizarDescripcion();
      IDenClase = 8;
      
    }


    public override void ActualizarDescripcion()
    {

        if (NIVEL < 2)
        {
            txtDescripcion = "<color=#5dade2><b>Exceso de Poder I</b></color>\n\n";
            txtDescripcion += "<i>La energía desbordante del Canalizador provoca inestabilidad en cada golpe certero, dejando rastros de su exceso.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>- Cada vez que realiza un <b>Crítico</b>, crea 1 Residuo Energético.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Obtiene +5% Critico permanente.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Al realizar un crítico, recibe 1-4 daño Arcano.</color>\n\n";

            if (EsEscenaCampaña())
            {
                if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
                {
                    if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
                    {
                        txtDescripcion += "<color=#dfea02>- Próximo Nivel: -1 Daño recibido al crítico</color>\n\n";
                    }
                }
            }
        }

        if (NIVEL == 2)
        {
            txtDescripcion = "<color=#5dade2><b>Exceso de Poder II</b></color>\n\n";
            txtDescripcion += "<i>La energía desbordante del Canalizador provoca inestabilidad en cada golpe certero, dejando rastros de su exceso.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>- Cada vez que realiza un <b>Crítico</b>, crea 1 Residuo Energético.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Obtiene +5% Critico permanente.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Al realizar un crítico, recibe 0-3 daño Arcano.</color>\n\n";

            if (EsEscenaCampaña())
            {
                if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
                {
                    if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
                    {
                        txtDescripcion += "<color=#dfea02>- Próximo Nivel: -1 Daño recibido al crítico (acumulativo)</color>\n\n";
                    }
                }
            }
        }

        if (NIVEL == 3)
        {
            txtDescripcion = "<color=#5dade2><b>Exceso de Poder III</b></color>\n\n";
            txtDescripcion += "<i>La energía desbordante del Canalizador provoca inestabilidad en cada golpe certero, dejando rastros de su exceso.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>- Cada vez que realiza un <b>Crítico</b>, crea 1 Residuo Energético.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Obtiene +5% Critico permanente.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Al realizar un crítico, recibe -1-2 daño Arcano.</color>\n\n";

            if (EsEscenaCampaña())
            {
                if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
                {
                    if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
                    {
                        txtDescripcion += "<color=#dfea02>- Próximo Nivel:\nOpción A: +1 Residuo por crítico\nOpción B: +5% Critico adicional</color>\n\n";
                    }
                }
            }
        }

        if (NIVEL == 4)
        {
            // Variante A
            txtDescripcion = "<color=#5dade2><b>Exceso de Poder IV a</b></color>\n\n";
            txtDescripcion += "<i>La energía desbordante del Canalizador provoca inestabilidad en cada golpe certero, dejando rastros de su exceso.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>- Cada vez que realiza un <b>Crítico</b>, crea 2 Residuos Energéticos.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Obtiene +5% Critico permanente.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Al realizar un crítico, recibe -1-2 daño Arcano.</color>\n\n";
        }

        if (NIVEL == 5)
        {
            // Variante B
            txtDescripcion = "<color=#5dade2><b>Exceso de Poder IV b</b></color>\n\n";
            txtDescripcion += "<i>La energía desbordante del Canalizador provoca inestabilidad en cada golpe certero, dejando rastros de su exceso.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>- Cada vez que realiza un <b>Crítico</b>, crea 1 Residuo Energético.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Obtiene +10% Critico permanente.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Al realizar un crítico, recibe -1-2 daño Arcano.</color>\n\n";
        }

    if (TRADU.i.nIdioma == 2) // English translation
    {
        if (NIVEL < 2)
        {
            txtDescripcion = "<color=#5dade2><b>Excess of Power I</b></color>\n\n";
            txtDescripcion += "<i>The overflowing energy of the Channeler causes instability with every precise hit, leaving traces of its excess.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>- Each time a <b>Critical</b> is made, creates 1 Energy Residue.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Gains +5% permanent Critical.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Upon making a critical, receives 1-4 Arcane damage.</color>\n\n";

            if (EsEscenaCampaña())
            {
                if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
                {
                    if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
                    {
                        txtDescripcion += "<color=#dfea02>- Next Level: -1 damage received on critical</color>\n\n";
                    }
                }
            }
        }

        if (NIVEL == 2)
        {
            txtDescripcion = "<color=#5dade2><b>Excess of Power II</b></color>\n\n";
            txtDescripcion += "<i>The overflowing energy of the Channeler causes instability with every precise hit, leaving traces of its excess.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>- Each time a <b>Critical</b> is made, creates 1 Energy Residue.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Gains +5% permanent Critical.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Upon making a critical, receives 0-3 Arcane damage.</color>\n\n";

            if (EsEscenaCampaña())
            {
                if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
                {
                    if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
                    {
                        txtDescripcion += "<color=#dfea02>- Next Level: -1 damage received on critical (cumulative)</color>\n\n";
                    }
                }
            }
        }

        if (NIVEL == 3)
        {
            txtDescripcion = "<color=#5dade2><b>Excess of Power III</b></color>\n\n";
            txtDescripcion += "<i>The overflowing energy of the Channeler causes instability with every precise hit, leaving traces of its excess.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>- Each time a <b>Critical</b> is made, creates 1 Energy Residue.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Gains +5% permanent Critical.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Upon making a critical, receives 0-2 Arcane damage.</color>\n\n";

            if (EsEscenaCampaña())
            {
                if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
                {
                    if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
                    {
                        txtDescripcion += "<color=#dfea02>- Next Level:\nOption A: +1 Residue per critical\nOption B: +5% additional Critical</color>\n\n";
                    }
                }
            }
        }

        if (NIVEL == 4)
        {
            // Variant A
            txtDescripcion = "<color=#5dade2><b>Excess of Power IV a</b></color>\n\n";
            txtDescripcion += "<i>The overflowing energy of the Channeler causes instability with every precise hit, leaving traces of its excess.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>- Each time a <b>Critical</b> is made, creates 2 Energy Residues.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Gains +5% permanent Critical.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Upon making a critical, receives 0-2 Arcane damage.</color>\n\n";
        }

        if (NIVEL == 5)
        {
            // Variant B
            txtDescripcion = "<color=#5dade2><b>Excess of Power IV b</b></color>\n\n";
            txtDescripcion += "<i>The overflowing energy of the Channeler causes instability with every precise hit, leaving traces of its excess.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>- Each time a <b>Critical</b> is made, creates 1 Energy Residue.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Gains +10% permanent Critical.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Upon making a critical, receives 0-2 Arcane damage.</color>\n\n";
        }
    }
    if (TRADU.i.nIdioma == 3)
    {
        if (NIVEL < 2)
        {
            txtDescripcion = "<color=#5dade2><b>Excesso de Poder I</b></color>\n\n";
            txtDescripcion += "<i>A energia transbordante do Canalizador gera instabilidade a cada golpe certeiro, deixando rastros do excesso.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>- Sempre que faz um <b>Critico</b>, cria 1 Residuo Energetico.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Ganha +5% Critico permanente.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Ao realizar um critico, recebe 1-4 de dano Arcano.</color>\n\n";

            if (EsEscenaCampaña())
            {
                if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
                {
                    if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
                    {
                        txtDescripcion += "<color=#dfea02>- Proximo Nivel: -1 dano recebido no critico</color>\n\n";
                    }
                }
            }
        }

        if (NIVEL == 2)
        {
            txtDescripcion = "<color=#5dade2><b>Excesso de Poder II</b></color>\n\n";
            txtDescripcion += "<i>A energia transbordante do Canalizador gera instabilidade a cada golpe certeiro, deixando rastros do excesso.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>- Sempre que faz um <b>Critico</b>, cria 1 Residuo Energetico.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Ganha +5% Critico permanente.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Ao realizar um critico, recebe 0-3 de dano Arcano.</color>\n\n";

            if (EsEscenaCampaña())
            {
                if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
                {
                    if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
                    {
                        txtDescripcion += "<color=#dfea02>- Proximo Nivel: -1 dano recebido no critico (acumulativo)</color>\n\n";
                    }
                }
            }
        }

        if (NIVEL == 3)
        {
            txtDescripcion = "<color=#5dade2><b>Excesso de Poder III</b></color>\n\n";
            txtDescripcion += "<i>A energia transbordante do Canalizador gera instabilidade a cada golpe certeiro, deixando rastros do excesso.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>- Sempre que faz um <b>Critico</b>, cria 1 Residuo Energetico.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Ganha +5% Critico permanente.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Ao realizar um critico, recebe 0-2 de dano Arcano.</color>\n\n";

            if (EsEscenaCampaña())
            {
                if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
                {
                    if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
                    {
                        txtDescripcion += "<color=#dfea02>- Proximo Nivel:\nOpcao A: +1 Residuo por critico\nOpcao B: +5% Critico adicional</color>\n\n";
                    }
                }
            }
        }

        if (NIVEL == 4)
        {
            txtDescripcion = "<color=#5dade2><b>Excesso de Poder IV a</b></color>\n\n";
            txtDescripcion += "<i>A energia transbordante do Canalizador gera instabilidade a cada golpe certeiro, deixando rastros do excesso.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>- Sempre que faz um <b>Critico</b>, cria 2 Residuos Energeticos.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Ganha +5% Critico permanente.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Ao realizar um critico, recebe 0-2 de dano Arcano.</color>\n\n";
        }

        if (NIVEL == 5)
        {
            txtDescripcion = "<color=#5dade2><b>Excesso de Poder IV b</b></color>\n\n";
            txtDescripcion += "<i>A energia transbordante do Canalizador gera instabilidade a cada golpe certeiro, deixando rastros do excesso.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>- Sempre que faz um <b>Critico</b>, cria 1 Residuo Energetico.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Ganha +10% Critico permanente.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Ao realizar um critico, recebe 0-2 de dano Arcano.</color>\n\n";
        }
    }

    bool esInglesFormato = TRADU.i != null && TRADU.i.nIdioma == 2;
    bool esPortuguesFormato = TRADU.i != null && TRADU.i.nIdioma == 3;
    string colorEncabezado = "#44d3ec";
    string colorValor = "#ffffff";
    string iconoEnergia = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_acumularenergia\"></voffset></size><space=-0.35em>";
    int residuos = NIVEL == 4 ? 2 : 1;
    int criticoPorcentaje = NIVEL == 5 ? 10 : 5;
    string danioPropio = NIVEL < 2 ? "1-4" : NIVEL == 2 ? "0-3" : "0-2";
    string tituloFormato = esInglesFormato ? "Excess of Power" : esPortuguesFormato ? "Excesso de Poder" : "Exceso de Poder";
    if (NIVEL < 2) { tituloFormato += " I"; }
    else if (NIVEL == 2) { tituloFormato += " II"; }
    else if (NIVEL == 3) { tituloFormato += " III"; }
    else if (NIVEL == 4) { tituloFormato += " IV a"; }
    else if (NIVEL == 5) { tituloFormato += " IV b"; }
    string subtituloFormato = esInglesFormato
      ? "Critical hits create Energy Residues and deal Arcane backlash."
      : esPortuguesFormato
        ? "Criticos criam Residuos Energeticos e causam retorno Arcano."
        : "Los criticos crean Residuos Energeticos y causan retorno Arcano.";
    string cuerpoFormato = "";
    if (esInglesFormato)
    {
      cuerpoFormato += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Passive buff</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Passive bonus:</b></color> <color={colorValor}>+{criticoPorcentaje}% Critical</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>On critical:</b></color> <color={colorValor}>{iconoEnergia} creates {residuos} Energy Residue{(residuos > 1 ? "s" : "")}</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Backlash:</b></color> <color={colorValor}>Takes {danioPropio} Arcane damage</color>";
    }
    else if (esPortuguesFormato)
    {
      cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Buff passivo</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Bonus passivo:</b></color> <color={colorValor}>+{criticoPorcentaje}% Critico</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Ao critar:</b></color> <color={colorValor}>{iconoEnergia} cria {residuos} Residuo{(residuos > 1 ? "s" : "")} Energetico{(residuos > 1 ? "s" : "")}</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Retorno:</b></color> <color={colorValor}>Recebe {danioPropio} dano Arcano</color>";
    }
    else
    {
      cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Buff pasivo</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Bonus pasivo:</b></color> <color={colorValor}>+{criticoPorcentaje}% Critico</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Al critico:</b></color> <color={colorValor}>{iconoEnergia} crea {residuos} Residuo{(residuos > 1 ? "s" : "")} Energetico{(residuos > 1 ? "s" : "")}</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Retorno:</b></color> <color={colorValor}>Recibe {danioPropio} daño Arcano</color>";
    }

    txtDescripcion =
      $"<size=115%><color=#5dade2><b>{tituloFormato}</b></color></size>\n\n" +
      $"<color=#8f8f8f><i>{subtituloFormato}</i></color>\n\n" +
      "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n" +
      cuerpoFormato;


    }


    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}



