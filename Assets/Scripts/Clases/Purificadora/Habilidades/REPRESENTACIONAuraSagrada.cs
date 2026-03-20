using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONAuraSagrada : Habilidad
{
   

    
    public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Purificadora_Aurasagrada");
      ActualizarDescripcion();
      IDenClase = 1;
      
    }

    public bool seusoEsteTurno = false;

  public override void ActualizarDescripcion()
  {

    if (NIVEL < 2)
    {
      txtDescripcion = "<color=#5dade2><b>Aura Sagrada I</b></color>\n\n";
      txtDescripcion += "<i>(Pasiva) Mientras la Purificadora tenga Fervor, da 1 de Barrera y 1 Bonus de daño divino a Aliados.</i>\n\n";

      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Barrera</color>\n\n";
          }
        }
      }

    }
    if (NIVEL == 2)
    {
      txtDescripcion = "<color=#5dade2><b>Aura Sagrada II</b></color>\n\n";
      txtDescripcion += "<i>(Pasiva) Mientras la Purificadora tenga Fervor, da 2 de Barrera y 1 Bonus de daño Divino a Aliados.</i>\n\n";
      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Bonus Daño divino</color>\n\n";
          }
        }
      }
    }
    if (NIVEL == 3)
    {
      txtDescripcion = "<color=#5dade2><b>Aura Sagrada III</b></color>\n\n";
      txtDescripcion += "<i>(Pasiva) Mientras la Purificadora tenga Fervor, da 2 de Barrera y 2 Bonus de daño Divino a Aliados.</i>\n\n";

      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Opción A: Remueve la necesidad de Fervor.</color>\n\n";
            txtDescripcion += $"<color=#dfea02>-Opción B: Si tiene 3 o más Fervor, duplica las bonificaciones.</color>\n";
          }
        }
      }
    }
    if (NIVEL == 4)
    {
      txtDescripcion = "<color=#5dade2><b>Aura Sagrada IV a</b></color>\n\n";
      txtDescripcion += "<i>(Pasiva) Da 2 de Barrera y 2 Bonus de daño Divino a Aliados.</i>\n\n";
    }
    if (NIVEL == 5)
    {
      txtDescripcion = "<color=#5dade2><b>Aura Sagrada IV b</b></color>\n\n";
      txtDescripcion += "<i>(Pasiva)  Mientras la Purificadora tenga Fervor, da 2 de Barrera y 2 Bonus de daño Divino a Aliados.</i>\n\n";
      txtDescripcion += "<i>Si tiene 3 o más Fervor, duplica las bonificaciones.</i>\n\n";
    }
       
      if (TRADU.i.nIdioma == 2) // English translation
      {
        if (NIVEL < 2)
        {
          txtDescripcion = "<color=#5dade2><b>Sacred Aura I</b></color>\n\n";
          txtDescripcion += "<i>(Passive) While the Purifier has Fervor, gives 1 Barrier and 1 Divine damage bonus to Allies.</i>\n\n";

          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Next Level: +1 Barrier</color>\n\n";
              }
            }
          }
        }
        if (NIVEL == 2)
        {
          txtDescripcion = "<color=#5dade2><b>Sacred Aura II</b></color>\n\n";
          txtDescripcion += "<i>(Passive) While the Purifier has Fervor, gives 2 Barrier and 1 Divine damage bonus to Allies.</i>\n\n";
          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Next Level: +1 Divine damage bonus</color>\n\n";
              }
            }
          }
        }
        if (NIVEL == 3)
        {
          txtDescripcion = "<color=#5dade2><b>Sacred Aura III</b></color>\n\n";
          txtDescripcion += "<i>(Passive) While the Purifier has Fervor, gives 2 Barrier and 2 Divine damage bonus to Allies.</i>\n\n";

          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Option A: Removes the need for Fervor.</color>\n\n";
                txtDescripcion += $"<color=#dfea02>-Option B: If has 3 or more Fervor, doubles the bonuses.</color>\n";
              }
            }
          }
        }
        if (NIVEL == 4)
        {
          txtDescripcion = "<color=#5dade2><b>Sacred Aura IV a</b></color>\n\n";
          txtDescripcion += "<i>(Passive) Gives 2 Barrier and 2 Divine damage bonus to Allies.</i>\n\n";
        }
        if (NIVEL == 5)
        {
          txtDescripcion = "<color=#5dade2><b>Sacred Aura IV b</b></color>\n\n";
          txtDescripcion += "<i>(Passive) While the Purifier has Fervor, gives 2 Barrier and 2 Divine damage bonus to Allies.</i>\n\n";
          txtDescripcion += "<i>If has 3 or more Fervor, doubles the bonuses.</i>\n\n";
        }
      }
      if (TRADU.i.nIdioma == 3) // Portuguese translation
      {
        if (NIVEL < 2)
        {
          txtDescripcion = "<color=#5dade2><b>Aura Sagrada I</b></color>\n\n";
          txtDescripcion += "<i>(Passiva) Enquanto a Purificadora tiver Fervor, concede 1 Barreira e 1 bonus de dano Divino aos Aliados.</i>\n\n";

          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Proximo Nivel: +1 Barreira</color>\n\n";
              }
            }
          }
        }
        if (NIVEL == 2)
        {
          txtDescripcion = "<color=#5dade2><b>Aura Sagrada II</b></color>\n\n";
          txtDescripcion += "<i>(Passiva) Enquanto a Purificadora tiver Fervor, concede 2 Barreira e 1 bonus de dano Divino aos Aliados.</i>\n\n";
          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Proximo Nivel: +1 bonus de dano Divino</color>\n\n";
              }
            }
          }
        }
        if (NIVEL == 3)
        {
          txtDescripcion = "<color=#5dade2><b>Aura Sagrada III</b></color>\n\n";
          txtDescripcion += "<i>(Passiva) Enquanto a Purificadora tiver Fervor, concede 2 Barreira e 2 bonus de dano Divino aos Aliados.</i>\n\n";

          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Opcao A: Remove a necessidade de Fervor.</color>\n\n";
                txtDescripcion += $"<color=#dfea02>-Opcao B: Se tiver 3 ou mais Fervor, duplica os bonus.</color>\n";
              }
            }
          }
        }
        if (NIVEL == 4)
        {
          txtDescripcion = "<color=#5dade2><b>Aura Sagrada IV a</b></color>\n\n";
          txtDescripcion += "<i>(Passiva) Concede 2 Barreira e 2 bonus de dano Divino aos Aliados.</i>\n\n";
        }
        if (NIVEL == 5)
        {
          txtDescripcion = "<color=#5dade2><b>Aura Sagrada IV b</b></color>\n\n";
          txtDescripcion += "<i>(Passiva) Enquanto a Purificadora tiver Fervor, concede 2 Barreira e 2 bonus de dano Divino aos Aliados.</i>\n\n";
          txtDescripcion += "<i>Se tiver 3 ou mais Fervor, duplica os bonus.</i>\n\n";
        }
      }
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}



