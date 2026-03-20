using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONAcorazado : Habilidad
{
     public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Caballero_Acorazado");
       ActualizarDescripcion();
      IDenClase = 1;
      
    }

    public override void ActualizarDescripcion()
    {
      if (TRADU.i.nIdioma == 1) // Español
      {
        if (NIVEL < 2)
        {
          txtDescripcion = "<color=#5dade2><b>Acorazado I</b></color>\n\n";
          txtDescripcion += "<i>(Pasiva)Su armadura pesada resiste los golpes enemigos más de lo habitual.\nDebe recibir 4 o + daño para que su armadura se reduzca al ser golpeado.</i>\n\n";

          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Próximo Nivel: Solo se pierde 1 de Armadura si el daño físico recibido es mayor a 5</color>\n\n";
              }
            }
          }
        }
        if (NIVEL == 2)
        {
          txtDescripcion = "<color=#5dade2><b>Acorazado II</b></color>\n\n";
          txtDescripcion += "<i>(Pasiva)Su armadura pesada resiste los golpes enemigos más de lo habitual.\nDebe recibir 5 o + daño para que su armadura se reduzca al ser golpeado.</i>\n\n";
          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Próximo Nivel: Solo se pierde 1 de Armadura si el daño físico recibido es mayor a 6</color>\n\n";
              }
            }
          }
        }
        if (NIVEL == 3)
        {
          txtDescripcion = "<color=#5dade2><b>Acorazado III</b></color>\n\n";
          txtDescripcion += "<i>(Pasiva)Su armadura pesada resiste los golpes enemigos más de lo habitual.\nDebe recibir 6 o + daño para que su armadura se reduzca al ser golpeado.</i>\n\n";

          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Opción A: Solo se pierde 1 de Armadura si el daño físico recibido es mayor a 8</color>\n\n";
                txtDescripcion += $"<color=#dfea02>-Opción B: La Armadura no puede bajar a menos de la mitad del valor de inicio</color>\n";
              }
            }
          }
        }
        if (NIVEL == 4)
        {
          txtDescripcion = "<color=#5dade2><b>Acorazado IV a</b></color>\n\n";
          txtDescripcion += "<i>(Pasiva)Su armadura pesada resiste los golpes enemigos más de lo habitual.\nDebe recibir 8 o + daño para que su armadura se reduzca al ser golpeado.</i>\n\n";
        }
        if (NIVEL == 5)
        {
          txtDescripcion = "<color=#5dade2><b>Acorazado IV b</b></color>\n\n";
          txtDescripcion += "<i>(Pasiva)Su armadura pesada resiste los golpes enemigos más de lo habitual.\nDebe recibir 6 o + daño para que su armadura se reduzca al ser golpeado y no puede bajar a menos de la mitad del valor inicial.</i>\n\n";
        }
      }
      if (TRADU.i.nIdioma == 2) // Inglés
      {
        if (NIVEL < 2)
        {
          txtDescripcion = "<color=#5dade2><b>Armored I</b></color>\n\n";
          txtDescripcion += "<i>(Passive)His heavy armor resists enemy blows more than usual.\nMust receive 4 or more damage for his armor to be reduced when hit.</i>\n\n";

          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Next Level: Only loses 1 Armor if physical damage received is greater than 5</color>\n\n";
              }
            }
          }
        }
        if (NIVEL == 2)
        {
          txtDescripcion = "<color=#5dade2><b>Armored II</b></color>\n\n";
          txtDescripcion += "<i>(Passive)His heavy armor resists enemy blows more than usual.\nMust receive 5 or more damage for his armor to be reduced when hit.</i>\n\n";
          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Next Level: Only loses 1 Armor if physical damage received is greater than 6</color>\n\n";
              }
            }
          }
        }
        if (NIVEL == 3)
        {
          txtDescripcion = "<color=#5dade2><b>Armored III</b></color>\n\n";
          txtDescripcion += "<i>(Passive)His heavy armor resists enemy blows more than usual.\nMust receive 6 or more damage for his armor to be reduced when hit.</i>\n\n";

          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Option A: Only loses 1 Armor if physical damage received is greater than 8</color>\n\n";
                txtDescripcion += $"<color=#dfea02>-Option B: Armor cannot drop below half of its initial value</color>\n";
              }
            }
          }
        }
        if (NIVEL == 4)
        {
          txtDescripcion = "<color=#5dade2><b>Armored IV a</b></color>\n\n";
          txtDescripcion += "<i>(Passive)His heavy armor resists enemy blows more than usual.\nMust receive 8 or more damage for his armor to be reduced when hit.</i>\n\n";
        }
        if (NIVEL == 5)
        {
          txtDescripcion = "<color=#5dade2><b>Armored IV b</b></color>\n\n";
          txtDescripcion += "<i>(Passive)His heavy armor resists enemy blows more than usual.\nMust receive 6 or more damage for his armor to be reduced when hit and cannot drop below half of its initial value.</i>\n\n";
        }
      }
      if (TRADU.i.nIdioma == 3) // Portugues
      {
        if (NIVEL < 2)
        {
          txtDescripcion = "<color=#5dade2><b>Encouracado I</b></color>\n\n";
          txtDescripcion += "<i>(Passiva)Sua armadura pesada resiste golpes inimigos mais do que o normal.\nPrecisa receber 4 ou mais de dano para que a Armadura reduza ao ser atingido.</i>\n\n";

          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Proximo Nivel: Perde apenas 1 de Armadura se o dano fisico recebido for maior que 5</color>\n\n";
              }
            }
          }
        }
        if (NIVEL == 2)
        {
          txtDescripcion = "<color=#5dade2><b>Encouracado II</b></color>\n\n";
          txtDescripcion += "<i>(Passiva)Sua armadura pesada resiste golpes inimigos mais do que o normal.\nPrecisa receber 5 ou mais de dano para que a Armadura reduza ao ser atingido.</i>\n\n";
          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Proximo Nivel: Perde apenas 1 de Armadura se o dano fisico recebido for maior que 6</color>\n\n";
              }
            }
          }
        }
        if (NIVEL == 3)
        {
          txtDescripcion = "<color=#5dade2><b>Encouracado III</b></color>\n\n";
          txtDescripcion += "<i>(Passiva)Sua armadura pesada resiste golpes inimigos mais do que o normal.\nPrecisa receber 6 ou mais de dano para que a Armadura reduza ao ser atingido.</i>\n\n";

          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Opcao A: Perde apenas 1 de Armadura se o dano fisico recebido for maior que 8</color>\n\n";
                txtDescripcion += $"<color=#dfea02>-Opcao B: A Armadura nao pode cair abaixo da metade do valor inicial</color>\n";
              }
            }
          }
        }
        if (NIVEL == 4)
        {
          txtDescripcion = "<color=#5dade2><b>Encouracado IV a</b></color>\n\n";
          txtDescripcion += "<i>(Passiva)Sua armadura pesada resiste golpes inimigos mais do que o normal.\nPrecisa receber 8 ou mais de dano para que a Armadura reduza ao ser atingido.</i>\n\n";
        }
        if (NIVEL == 5)
        {
          txtDescripcion = "<color=#5dade2><b>Encouracado IV b</b></color>\n\n";
          txtDescripcion += "<i>(Passiva)Sua armadura pesada resiste golpes inimigos mais do que o normal.\nPrecisa receber 6 ou mais de dano para que a Armadura reduza ao ser atingido e nao pode cair abaixo da metade do valor inicial.</i>\n\n";
        }
      }
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}



