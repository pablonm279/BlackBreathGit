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

  
    public override void  ActualizarDescripcion()
    {

 if(NIVEL < 2)
{
    txtDescripcion = "<color=#5dade2><b>Exceso de Poder I</b></color>\n\n";
    txtDescripcion += "<i>La energía desbordante del Canalizador provoca inestabilidad en cada golpe certero, dejando rastros de su exceso.</i>\n\n";
    txtDescripcion += "<color=#c8c8c8>- Cada vez que realiza un <b>Crítico</b>, crea 1 Residuo Energético.</color>\n";
    txtDescripcion += "<color=#c8c8c8>- Obtiene +1 Dado Crítico permanente.</color>\n";
    txtDescripcion += "<color=#c8c8c8>- Al realizar un crítico, recibe 1d4 daño Arcano.</color>\n\n";

    if (EsEscenaCampaña())
    {
        if(CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
                txtDescripcion += "<color=#dfea02>- Próximo Nivel: -1 Daño recibido al crítico</color>\n\n";
            }
        }
    }
}

if(NIVEL == 2)
{
    txtDescripcion = "<color=#5dade2><b>Exceso de Poder II</b></color>\n\n";
    txtDescripcion += "<i>La energía desbordante del Canalizador provoca inestabilidad en cada golpe certero, dejando rastros de su exceso.</i>\n\n";
    txtDescripcion += "<color=#c8c8c8>- Cada vez que realiza un <b>Crítico</b>, crea 1 Residuo Energético.</color>\n";
    txtDescripcion += "<color=#c8c8c8>- Obtiene +1 Dado Crítico permanente.</color>\n";
    txtDescripcion += "<color=#c8c8c8>- Al realizar un crítico, recibe 1d4-1 daño Arcano.</color>\n\n";

    if (EsEscenaCampaña())
    {
        if(CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
                txtDescripcion += "<color=#dfea02>- Próximo Nivel: -1 Daño recibido al crítico (acumulativo)</color>\n\n";
            }
        }
    }
}

if(NIVEL == 3)
{
    txtDescripcion = "<color=#5dade2><b>Exceso de Poder III</b></color>\n\n";
    txtDescripcion += "<i>La energía desbordante del Canalizador provoca inestabilidad en cada golpe certero, dejando rastros de su exceso.</i>\n\n";
    txtDescripcion += "<color=#c8c8c8>- Cada vez que realiza un <b>Crítico</b>, crea 1 Residuo Energético.</color>\n";
    txtDescripcion += "<color=#c8c8c8>- Obtiene +1 Dado Crítico permanente.</color>\n";
    txtDescripcion += "<color=#c8c8c8>- Al realizar un crítico, recibe 1d4-2 daño Arcano.</color>\n\n";

    if (EsEscenaCampaña())
    {
        if(CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
                txtDescripcion += "<color=#dfea02>- Próximo Nivel:\nOpción A: +1 Residuo por crítico\nOpción B: +1 Dado Crítico adicional</color>\n\n";
            }
        }
    }
}

if(NIVEL == 4)
{
    // Variante A
    txtDescripcion = "<color=#5dade2><b>Exceso de Poder IV a</b></color>\n\n";
    txtDescripcion += "<i>La energía desbordante del Canalizador provoca inestabilidad en cada golpe certero, dejando rastros de su exceso.</i>\n\n";
    txtDescripcion += "<color=#c8c8c8>- Cada vez que realiza un <b>Crítico</b>, crea 2 Residuos Energéticos.</color>\n";
    txtDescripcion += "<color=#c8c8c8>- Obtiene +1 Dado Crítico permanente.</color>\n";
    txtDescripcion += "<color=#c8c8c8>- Al realizar un crítico, recibe 1d4-2 daño Arcano.</color>\n\n";
}

if(NIVEL == 5)
{
    // Variante B
    txtDescripcion = "<color=#5dade2><b>Exceso de Poder IV b</b></color>\n\n";
    txtDescripcion += "<i>La energía desbordante del Canalizador provoca inestabilidad en cada golpe certero, dejando rastros de su exceso.</i>\n\n";
    txtDescripcion += "<color=#c8c8c8>- Cada vez que realiza un <b>Crítico</b>, crea 1 Residuo Energético.</color>\n";
    txtDescripcion += "<color=#c8c8c8>- Obtiene +2 Dados Críticos permanentes.</color>\n";
    txtDescripcion += "<color=#c8c8c8>- Al realizar un crítico, recibe 1d4-2 daño Arcano.</color>\n\n";
}
      

    }


    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}
