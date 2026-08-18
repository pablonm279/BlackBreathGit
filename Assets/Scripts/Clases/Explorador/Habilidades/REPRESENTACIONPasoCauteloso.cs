using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONPasoCauteloso : Habilidad
{
   

    
   public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Explorador_PasoCauteloso");
       ActualizarDescripcion();
      IDenClase = 0;
      
    }

    public bool seusoEsteTurno = false;

  public override void ActualizarDescripcion()
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
    string colorTitulo = "#5dade2";
    string colorEncabezado = "#44d3ec";

    if (esIngles)
    {
      txtDescripcion = ConstruirDescripcionNormalizadaIngles(
        "Cautious Step",
        "Passive: Avoids one hostile tile effect each turn.",
        new[]
        {
          LineaDescripcion("Trigger", "Enters a tile with a hostile effect."),
          LineaDescripcion("Effect", "Evades that effect."),
          LineaDescripcion("Limit", "Once per turn")
        },
        costoSuperior: string.Empty);
      return;
    }

    if (esPortugues)
    {
      txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(
        "Passo Cauteloso",
        "Passiva: evita um efeito de casa hostil por turno.",
        new[]
        {
          LineaDescripcion("Ativação", "Entra em uma casa com um efeito hostil."),
          LineaDescripcion("Efeito", "Evita esse efeito."),
          LineaDescripcion("Limite", "Uma vez por turno")
        },
        costoSuperior: string.Empty);
      return;
    }

    txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(
      "Paso Cauteloso",
      "Pasiva: evita un efecto de casilla hostil por turno.",
      new[]
      {
        LineaDescripcion("Activación", "Entra en una casilla con un efecto hostil."),
        LineaDescripcion("Efecto", "Evita ese efecto."),
        LineaDescripcion("Límite", "Una vez por turno")
      },
      costoSuperior: string.Empty);
    return;

    string titulo = "Paso Cauteloso";
    string subtitulo = "Evita una casilla hostil una vez por turno.";
    string cuerpo = $"<color={colorEncabezado}><b>Tipo:</b></color> Pasiva\n" +
                    $"<color={colorEncabezado}><b>Activacion:</b></color> Al entrar en una casilla con efecto hostil\n" +
                    $"<color={colorEncabezado}><b>Efecto:</b></color> Evade ese efecto\n" +
                    $"<color={colorEncabezado}><b>Limite:</b></color> 1 vez por turno";

    if (esIngles)
    {
      titulo = "Cautious Step";
      subtitulo = "Avoids one hostile tile each turn.";
      cuerpo = $"<color={colorEncabezado}><b>Type:</b></color> Passive\n" +
               $"<color={colorEncabezado}><b>Trigger:</b></color> Entering a tile with a hostile effect\n" +
               $"<color={colorEncabezado}><b>Effect:</b></color> Evades that effect\n" +
               $"<color={colorEncabezado}><b>Limit:</b></color> 1 time per turn";
    }
    else if (esPortugues)
    {
      titulo = "Passo Cauteloso";
      subtitulo = "Evita uma casa hostil uma vez por turno.";
      cuerpo = $"<color={colorEncabezado}><b>Tipo:</b></color> Passiva\n" +
               $"<color={colorEncabezado}><b>Ativacao:</b></color> Ao entrar em uma casa com efeito hostil\n" +
               $"<color={colorEncabezado}><b>Efeito:</b></color> Evita esse efeito\n" +
               $"<color={colorEncabezado}><b>Limite:</b></color> 1 vez por turno";
    }

    txtDescripcion = $"<size=115%><color={colorTitulo}><b>{titulo}</b></color></size>\n\n";
    txtDescripcion += $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n";
    txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
    txtDescripcion += cuerpo;
  }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}
