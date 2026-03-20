using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONDeterminacion : Habilidad
{
   

     public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Caballero_Determinacion");
      IDenClase = 5;

      ActualizarDescripcion();
       
 

    }

  public override void ActualizarDescripcion()
  {
    if (TRADU.i.nIdioma == 1) // Español
    {
      if(NIVEL<2)
      {
        txtDescripcion = "<color=#5dade2><b>Determinación I</b></color>\n\n"; 
        txtDescripcion += "<i>(Pasiva)Su compromiso con la causa es inquebrantable.\n +5% daño causado por cada Punto de Valentía.</i>\n\n";

        if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>-Próximo Nivel: al estar Motivado +1 Tiradas de Salvación extra</color>\n\n";
            }
          }
        }
      }
      if(NIVEL==2)
      {
        txtDescripcion = "<color=#5dade2><b>Determinación II</b></color>\n\n"; 
        txtDescripcion += "<i>(Pasiva)Su compromiso con la causa es inquebrantable.\n +5% daño causado por cada Punto de Valentía.</i>\n\n";
        txtDescripcion += "<i>Al estar Motivado gana +1 a las Tiradas de Salvación.\n\n";

        if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>-Próximo Nivel: al estar Eufórico +1 Ataque</color>\n\n";
            }
          }
        }
      }
      if(NIVEL==3)
      {
        txtDescripcion = "<color=#5dade2><b>Determinación III</b></color>\n\n"; 
        txtDescripcion += "<i>(Pasiva)Su compromiso con la causa es inquebrantable.\n +5% daño causado por cada Punto de Valentía.</i>\n\n";
        txtDescripcion += "<i>Al estar Motivado gana +1 a las Tiradas de Salvación.\n\n";
        txtDescripcion += "<i>Al estar Eufórico gana +1 de Ataque.\n\n";

        if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>-Opción A: Arranca la batalla con 5 Puntos de Valentía</color>\n";
              txtDescripcion += $"<color=#dfea02>-Opción B: +7% daño causado por cada Punto de Valentía</color>\n";
            }
          }
        }
      }
      if(NIVEL==4)
      {
        txtDescripcion = "<color=#5dade2><b>Determinación IV a</b></color>\n\n"; 
        txtDescripcion += "<i>(Pasiva)Su compromiso con la causa es inquebrantable.\n +5% daño causado por cada Punto de Valentía.</i>\n\n";
        txtDescripcion += "<i>Al estar Motivado gana +1 a las Tiradas de Salvación.\n";
        txtDescripcion += "<i>Al estar Eufórico gana +1 de Ataque.\n";  
        txtDescripcion += "<i>Arranca la batalla con 5 P. Valentía.\n";    
      }
      if(NIVEL==5)
      {
        txtDescripcion = "<color=#5dade2><b>Determinación IV b</b></color>\n\n"; 
        txtDescripcion += "<i>(Pasiva)Su compromiso con la causa es inquebrantable.\n +7% daño causado por cada Punto de Valentía.</i>\n\n";
        txtDescripcion += "<i>Al estar Motivado gana +1 a las Tiradas de Salvación.\n";
        txtDescripcion += "<i>Al estar Eufórico gana +1 de Ataque.\n";  
      }
    }
    if (TRADU.i.nIdioma == 2) // Inglés
    {
      if(NIVEL<2)
      {
        txtDescripcion = "<color=#5dade2><b>Determination I</b></color>\n\n"; 
        txtDescripcion += "<i>(Passive)Their commitment to the cause is unwavering.\n +5% damage dealt per Valour Point.</i>\n\n";

        if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>-Next Level: when Motivated +1 extra Saving Throw</color>\n\n";
            }
          }
        }
      }
      if(NIVEL==2)
      {
        txtDescripcion = "<color=#5dade2><b>Determination II</b></color>\n\n"; 
        txtDescripcion += "<i>(Passive)Their commitment to the cause is unwavering.\n +5% damage dealt per Valour Point.</i>\n\n";
        txtDescripcion += "<i>When Motivated gains +1 to Saving Throws.\n\n";

        if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>-Next Level: when Euphoric +1 Attack</color>\n\n";
            }
          }
        }
      }
      if(NIVEL==3)
      {
        txtDescripcion = "<color=#5dade2><b>Determination III</b></color>\n\n"; 
        txtDescripcion += "<i>(Passive)Their commitment to the cause is unwavering.\n +5% damage dealt per Valour Point.</i>\n\n";
        txtDescripcion += "<i>When Motivated gains +1 to Saving Throws.\n\n";
        txtDescripcion += "<i>When Euphoric gains +1 Attack.\n\n";

        if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>-Option A: Start the battle with 5 Valour Points</color>\n";
              txtDescripcion += $"<color=#dfea02>-Option B: +7% damage dealt per Valour Point</color>\n";
            }
          }
        }
      }
      if(NIVEL==4)
      {
        txtDescripcion = "<color=#5dade2><b>Determination IV a</b></color>\n\n"; 
        txtDescripcion += "<i>(Passive)Their commitment to the cause is unwavering.\n +5% damage dealt per Valour Point.</i>\n\n";
        txtDescripcion += "<i>When Motivated gains +1 to Saving Throws.\n";
        txtDescripcion += "<i>When Euphoric gains +1 Attack.\n";  
        txtDescripcion += "<i>Starts the battle with 5 Valour Points.\n";    
      }
      if(NIVEL==5)
      {
        txtDescripcion = "<color=#5dade2><b>Determination IV b</b></color>\n\n"; 
        txtDescripcion += "<i>(Passive)Their commitment to the cause is unwavering.\n +7% damage dealt per Valour Point.</i>\n\n";
        txtDescripcion += "<i>When Motivated gains +1 to Saving Throws.\n";
        txtDescripcion += "<i>When Euphoric gains +1 Attack.\n";  
      }
    }
    if (TRADU.i.nIdioma == 3) // Portugues
    {
      if(NIVEL<2)
      {
        txtDescripcion = "<color=#5dade2><b>Determinacao I</b></color>\n\n"; 
        txtDescripcion += "<i>(Passiva)Seu compromisso com a causa e inquebravel.\n +5% de dano causado por cada Ponto de Valentia.</i>\n\n";

        if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>-Proximo Nivel: ao estar Motivado +1 Resistencia extra</color>\n\n";
            }
          }
        }
      }
      if(NIVEL==2)
      {
        txtDescripcion = "<color=#5dade2><b>Determinacao II</b></color>\n\n"; 
        txtDescripcion += "<i>(Passiva)Seu compromisso com a causa e inquebravel.\n +5% de dano causado por cada Ponto de Valentia.</i>\n\n";
        txtDescripcion += "<i>Ao estar Motivado ganha +1 em Resistencias.\n\n";

        if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>-Proximo Nivel: ao estar Euforico +1 Ataque</color>\n\n";
            }
          }
        }
      }
      if(NIVEL==3)
      {
        txtDescripcion = "<color=#5dade2><b>Determinacao III</b></color>\n\n"; 
        txtDescripcion += "<i>(Passiva)Seu compromisso com a causa e inquebravel.\n +5% de dano causado por cada Ponto de Valentia.</i>\n\n";
        txtDescripcion += "<i>Ao estar Motivado ganha +1 em Resistencias.\n\n";
        txtDescripcion += "<i>Ao estar Euforico ganha +1 de Ataque.\n\n";

        if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>-Opcao A: Comeca a batalha com 5 Pontos de Valentia</color>\n";
              txtDescripcion += $"<color=#dfea02>-Opcao B: +7% de dano causado por cada Ponto de Valentia</color>\n";
            }
          }
        }
      }
      if(NIVEL==4)
      {
        txtDescripcion = "<color=#5dade2><b>Determinacao IV a</b></color>\n\n"; 
        txtDescripcion += "<i>(Passiva)Seu compromisso com a causa e inquebravel.\n +5% de dano causado por cada Ponto de Valentia.</i>\n\n";
        txtDescripcion += "<i>Ao estar Motivado ganha +1 em Resistencias.\n";
        txtDescripcion += "<i>Ao estar Euforico ganha +1 de Ataque.\n";  
        txtDescripcion += "<i>Comeca a batalha com 5 P. de Valentia.\n";    
      }
      if(NIVEL==5)
      {
        txtDescripcion = "<color=#5dade2><b>Determinacao IV b</b></color>\n\n"; 
        txtDescripcion += "<i>(Passiva)Seu compromisso com a causa e inquebravel.\n +7% de dano causado por cada Ponto de Valentia.</i>\n\n";
        txtDescripcion += "<i>Ao estar Motivado ganha +1 em Resistencias.\n";
        txtDescripcion += "<i>Ao estar Euforico ganha +1 de Ataque.\n";  
      }
    }
  }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}




