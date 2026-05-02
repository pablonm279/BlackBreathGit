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
          txtDescripcion += "<i>(Pasiva)Su armadura pesada resiste los golpes enemigos más de lo habitual.\nDebe recibir 6 o + daño para que su armadura se reduzca al ser golpeado.</i>\n\n";

          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Próximo Nivel: Solo se pierde 1 de Armadura si el daño físico recibido es mayor a 7</color>\n\n";
              }
            }
          }
        }
        if (NIVEL == 2)
        {
          txtDescripcion = "<color=#5dade2><b>Acorazado II</b></color>\n\n";
          txtDescripcion += "<i>(Pasiva)Su armadura pesada resiste los golpes enemigos más de lo habitual.\nDebe recibir 7 o + daño para que su armadura se reduzca al ser golpeado.</i>\n\n";
          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Próximo Nivel: Solo se pierde 1 de Armadura si el daño físico recibido es mayor a 8</color>\n\n";
              }
            }
          }
        }
        if (NIVEL == 3)
        {
          txtDescripcion = "<color=#5dade2><b>Acorazado III</b></color>\n\n";
          txtDescripcion += "<i>(Pasiva)Su armadura pesada resiste los golpes enemigos más de lo habitual.\nDebe recibir 8 o + daño para que su armadura se reduzca al ser golpeado.</i>\n\n";

          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Opción A: Solo se pierde 1 de Armadura si el daño físico recibido es mayor a 10</color>\n\n";
                txtDescripcion += $"<color=#dfea02>-Opción B: La Armadura no puede bajar a menos de la mitad del valor de inicio</color>\n";
              }
            }
          }
        }
        if (NIVEL == 4)
        {
          txtDescripcion = "<color=#5dade2><b>Acorazado IV a</b></color>\n\n";
          txtDescripcion += "<i>(Pasiva)Su armadura pesada resiste los golpes enemigos más de lo habitual.\nDebe recibir 10 o + daño para que su armadura se reduzca al ser golpeado.</i>\n\n";
        }
        if (NIVEL == 5)
        {
          txtDescripcion = "<color=#5dade2><b>Acorazado IV b</b></color>\n\n";
          txtDescripcion += "<i>(Pasiva)Su armadura pesada resiste los golpes enemigos más de lo habitual.\nDebe recibir 8 o + daño para que su armadura se reduzca al ser golpeado y no puede bajar a menos de la mitad del valor inicial.</i>\n\n";
        }
      }
      if (TRADU.i.nIdioma == 2) // Inglés
      {
        if (NIVEL < 2)
        {
          txtDescripcion = "<color=#5dade2><b>Armored I</b></color>\n\n";
          txtDescripcion += "<i>(Passive)His heavy armor resists enemy blows more than usual.\nMust receive 6 or more damage for his armor to be reduced when hit.</i>\n\n";

          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Next Level: Only loses 1 Armor if physical damage received is greater than 7</color>\n\n";
              }
            }
          }
        }
        if (NIVEL == 2)
        {
          txtDescripcion = "<color=#5dade2><b>Armored II</b></color>\n\n";
          txtDescripcion += "<i>(Passive)His heavy armor resists enemy blows more than usual.\nMust receive 7 or more damage for his armor to be reduced when hit.</i>\n\n";
          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Next Level: Only loses 1 Armor if physical damage received is greater than 8</color>\n\n";
              }
            }
          }
        }
        if (NIVEL == 3)
        {
          txtDescripcion = "<color=#5dade2><b>Armored III</b></color>\n\n";
          txtDescripcion += "<i>(Passive)His heavy armor resists enemy blows more than usual.\nMust receive 8 or more damage for his armor to be reduced when hit.</i>\n\n";

          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Option A: Only loses 1 Armor if physical damage received is greater than 10</color>\n\n";
                txtDescripcion += $"<color=#dfea02>-Option B: Armor cannot drop below half of its initial value</color>\n";
              }
            }
          }
        }
        if (NIVEL == 4)
        {
          txtDescripcion = "<color=#5dade2><b>Armored IV a</b></color>\n\n";
          txtDescripcion += "<i>(Passive)His heavy armor resists enemy blows more than usual.\nMust receive 10 or more damage for his armor to be reduced when hit.</i>\n\n";
        }
        if (NIVEL == 5)
        {
          txtDescripcion = "<color=#5dade2><b>Armored IV b</b></color>\n\n";
          txtDescripcion += "<i>(Passive)His heavy armor resists enemy blows more than usual.\nMust receive 8 or more damage for his armor to be reduced when hit and cannot drop below half of its initial value.</i>\n\n";
        }
      }
      if (TRADU.i.nIdioma == 3) // Portugues
      {
        if (NIVEL < 2)
        {
          txtDescripcion = "<color=#5dade2><b>Encouracado I</b></color>\n\n";
          txtDescripcion += "<i>(Passiva)Sua armadura pesada resiste golpes inimigos mais do que o normal.\nPrecisa receber 6 ou mais de dano para que a Armadura reduza ao ser atingido.</i>\n\n";

          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Proximo Nivel: Perde apenas 1 de Armadura se o dano fisico recebido for maior que 7</color>\n\n";
              }
            }
          }
        }
        if (NIVEL == 2)
        {
          txtDescripcion = "<color=#5dade2><b>Encouracado II</b></color>\n\n";
          txtDescripcion += "<i>(Passiva)Sua armadura pesada resiste golpes inimigos mais do que o normal.\nPrecisa receber 7 ou mais de dano para que a Armadura reduza ao ser atingido.</i>\n\n";
          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Proximo Nivel: Perde apenas 1 de Armadura se o dano fisico recebido for maior que 8</color>\n\n";
              }
            }
          }
        }
        if (NIVEL == 3)
        {
          txtDescripcion = "<color=#5dade2><b>Encouracado III</b></color>\n\n";
          txtDescripcion += "<i>(Passiva)Sua armadura pesada resiste golpes inimigos mais do que o normal.\nPrecisa receber 8 ou mais de dano para que a Armadura reduza ao ser atingido.</i>\n\n";

          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Opcao A: Perde apenas 1 de Armadura se o dano fisico recebido for maior que 10</color>\n\n";
                txtDescripcion += $"<color=#dfea02>-Opcao B: A Armadura nao pode cair abaixo da metade do valor inicial</color>\n";
              }
            }
          }
        }
        if (NIVEL == 4)
        {
          txtDescripcion = "<color=#5dade2><b>Encouracado IV a</b></color>\n\n";
          txtDescripcion += "<i>(Passiva)Sua armadura pesada resiste golpes inimigos mais do que o normal.\nPrecisa receber 10 ou mais de dano para que a Armadura reduza ao ser atingido.</i>\n\n";
        }
        if (NIVEL == 5)
        {
          txtDescripcion = "<color=#5dade2><b>Encouracado IV b</b></color>\n\n";
          txtDescripcion += "<i>(Passiva)Sua armadura pesada resiste golpes inimigos mais do que o normal.\nPrecisa receber 8 ou mais de dano para que a Armadura reduza ao ser atingido e nao pode cair abaixo da metade do valor inicial.</i>\n\n";
        }
      }

      AplicarDescripcionEstandar();
    }

    private void AplicarDescripcionEstandar()
    {
      int umbralDanio = 6;
      if (NIVEL == 2) { umbralDanio = 7; }
      else if (NIVEL == 3 || NIVEL == 5) { umbralDanio = 8; }
      else if (NIVEL == 4) { umbralDanio = 10; }

      string titulo = $"Acorazado {SufijoNivel()}";
      string subtitulo = $"<color=#4f5552>Pasiva: la Armadura solo baja si recibe {umbralDanio}+ dano fisico.</color>";
      string cuerpo = "<color=#44d3ec><b>Tipo:</b></color> <color=#ffffff>Pasiva</color>\n" +
                      $"<color=#44d3ec><b>Efecto:</b></color> <color=#ffffff>La Armadura se reduce solo al recibir {umbralDanio}+ dano fisico.</color>";

      if (NIVEL == 5)
      {
        cuerpo += "\n<color=#44d3ec><b>Extra:</b></color> <color=#ffffff>La Armadura no baja de la mitad de su valor inicial.</color>";
      }

      string proximo = TextoProximoNivel();
      if (!string.IsNullOrEmpty(proximo)) { cuerpo += "\n\n" + proximo; }

      if (TRADU.i.nIdioma == 2)
      {
        titulo = $"Armored {SufijoNivel()}";
        subtitulo = $"<color=#4f5552>Passive: Armor only drops after taking {umbralDanio}+ physical damage.</color>";
        cuerpo = "<color=#44d3ec><b>Type:</b></color> <color=#ffffff>Passive</color>\n" +
                 $"<color=#44d3ec><b>Effect:</b></color> <color=#ffffff>Armor is reduced only after taking {umbralDanio}+ physical damage.</color>";
        if (NIVEL == 5)
        {
          cuerpo += "\n<color=#44d3ec><b>Extra:</b></color> <color=#ffffff>Armor cannot drop below half of its initial value.</color>";
        }
        proximo = TextoProximoNivel();
        if (!string.IsNullOrEmpty(proximo)) { cuerpo += "\n\n" + proximo; }
      }
      else if (TRADU.i.nIdioma == 3)
      {
        titulo = $"Encouracado {SufijoNivel()}";
        subtitulo = $"<color=#4f5552>Passiva: Armadura so baixa ao receber {umbralDanio}+ dano fisico.</color>";
        cuerpo = "<color=#44d3ec><b>Tipo:</b></color> <color=#ffffff>Passiva</color>\n" +
                 $"<color=#44d3ec><b>Efeito:</b></color> <color=#ffffff>A Armadura reduz apenas ao receber {umbralDanio}+ dano fisico.</color>";
        if (NIVEL == 5)
        {
          cuerpo += "\n<color=#44d3ec><b>Extra:</b></color> <color=#ffffff>A Armadura nao baixa da metade do valor inicial.</color>";
        }
        proximo = TextoProximoNivel();
        if (!string.IsNullOrEmpty(proximo)) { cuerpo += "\n\n" + proximo; }
      }

      txtDescripcion = ConstruirDescripcionEstandar($"<size=115%>{titulo}</size>", subtitulo, cuerpo, "", "#5dade2");
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
        if (NIVEL < 2) { return "<color=#dfea02>Next Level: Armor reduction threshold becomes 7+ physical damage.</color>"; }
        if (NIVEL == 2) { return "<color=#dfea02>Next Level: Armor reduction threshold becomes 8+ physical damage.</color>"; }
        if (NIVEL == 3) { return "<color=#dfea02>Option A: threshold becomes 10+ physical damage.\nOption B: Armor cannot drop below half of its initial value.</color>"; }
      }
      else if (TRADU.i.nIdioma == 3)
      {
        if (NIVEL < 2) { return "<color=#dfea02>Proximo Nivel: a Armadura reduz com 7+ dano fisico.</color>"; }
        if (NIVEL == 2) { return "<color=#dfea02>Proximo Nivel: a Armadura reduz com 8+ dano fisico.</color>"; }
        if (NIVEL == 3) { return "<color=#dfea02>Opcao A: reduz com 10+ dano fisico.\nOpcao B: Armadura nao baixa da metade inicial.</color>"; }
      }
      else
      {
        if (NIVEL < 2) { return "<color=#dfea02>Proximo Nivel: la Armadura se reduce con 7+ dano fisico.</color>"; }
        if (NIVEL == 2) { return "<color=#dfea02>Proximo Nivel: la Armadura se reduce con 8+ dano fisico.</color>"; }
        if (NIVEL == 3) { return "<color=#dfea02>Opcion A: se reduce con 10+ dano fisico.\nOpcion B: la Armadura no baja de la mitad inicial.</color>"; }
      }

      return "";
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}



