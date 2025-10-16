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
            txtDescripcion += "<color=#c8c8c8>- Obtiene +1 Dado Crítico permanente.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Al realizar un crítico, recibe 1d4 daño Arcano.</color>\n\n";

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
            txtDescripcion += "<color=#c8c8c8>- Obtiene +1 Dado Crítico permanente.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Al realizar un crítico, recibe 1d4-1 daño Arcano.</color>\n\n";

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
            txtDescripcion += "<color=#c8c8c8>- Obtiene +1 Dado Crítico permanente.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Al realizar un crítico, recibe 1d4-2 daño Arcano.</color>\n\n";

            if (EsEscenaCampaña())
            {
                if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
                {
                    if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
                    {
                        txtDescripcion += "<color=#dfea02>- Próximo Nivel:\nOpción A: +1 Residuo por crítico\nOpción B: +1 Dado Crítico adicional</color>\n\n";
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
            txtDescripcion += "<color=#c8c8c8>- Obtiene +1 Dado Crítico permanente.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Al realizar un crítico, recibe 1d4-2 daño Arcano.</color>\n\n";
        }

        if (NIVEL == 5)
        {
            // Variante B
            txtDescripcion = "<color=#5dade2><b>Exceso de Poder IV b</b></color>\n\n";
            txtDescripcion += "<i>La energía desbordante del Canalizador provoca inestabilidad en cada golpe certero, dejando rastros de su exceso.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>- Cada vez que realiza un <b>Crítico</b>, crea 1 Residuo Energético.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Obtiene +2 Dados Críticos permanentes.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Al realizar un crítico, recibe 1d4-2 daño Arcano.</color>\n\n";
        }

    if (TRADU.i.nIdioma == 2) // English translation
    {
        if (NIVEL < 2)
        {
            txtDescripcion = "<color=#5dade2><b>Excess of Power I</b></color>\n\n";
            txtDescripcion += "<i>The overflowing energy of the Channeler causes instability with every precise hit, leaving traces of its excess.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>- Each time a <b>Critical</b> is made, creates 1 Energy Residue.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Gains +1 permanent Critical Die.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Upon making a critical, receives 1d4 Arcane damage.</color>\n\n";

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
            txtDescripcion += "<color=#c8c8c8>- Gains +1 permanent Critical Die.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Upon making a critical, receives 1d4-1 Arcane damage.</color>\n\n";

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
            txtDescripcion += "<color=#c8c8c8>- Gains +1 permanent Critical Die.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Upon making a critical, receives 1d4-2 Arcane damage.</color>\n\n";

            if (EsEscenaCampaña())
            {
                if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
                {
                    if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
                    {
                        txtDescripcion += "<color=#dfea02>- Next Level:\nOption A: +1 Residue per critical\nOption B: +1 additional Critical Die</color>\n\n";
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
            txtDescripcion += "<color=#c8c8c8>- Gains +1 permanent Critical Die.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Upon making a critical, receives 1d4-2 Arcane damage.</color>\n\n";
        }

        if (NIVEL == 5)
        {
            // Variant B
            txtDescripcion = "<color=#5dade2><b>Excess of Power IV b</b></color>\n\n";
            txtDescripcion += "<i>The overflowing energy of the Channeler causes instability with every precise hit, leaving traces of its excess.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>- Each time a <b>Critical</b> is made, creates 1 Energy Residue.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Gains +2 permanent Critical Dice.</color>\n";
            txtDescripcion += "<color=#c8c8c8>- Upon making a critical, receives 1d4-2 Arcane damage.</color>\n\n";
        }
    }


    }


    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}
