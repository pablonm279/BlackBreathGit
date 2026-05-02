using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONSigiloso : Habilidad
{
    public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Acechador_Sigiloso");
      ActualizarDescripcion();
      IDenClase = 0;
    }

    public bool seusoEsteTurno = false;

  public override void ActualizarDescripcion()
  {
    string iconoOculto = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_oculto\"></voffset></size><space=-0.35em>";
    string titulo = "Sigiloso";
    string bajada = "Empieza oculto si el combate no es una emboscada enemiga.";
    string tipo = "Pasiva";
    string inicio = $"{iconoOculto} Escondido al iniciar combate";
    string mientras = "Al Acecho: +2 Ataque, +5% Critico, +10% daño";
    string extra = "Sin penalidad por combate nocturno";

    if (TRADU.i.nIdioma == 2) //agrega la traduccion a ingles
    {
      titulo = "Stealthy";
      bajada = "Starts hidden if the combat is not an enemy ambush.";
      tipo = "Passive";
      inicio = $"{iconoOculto} Hidden at combat start";
      mientras = "Stalking: +2 Attack, +5% Critical, +10% damage";
      extra = "No night combat penalty";
    }
    if (TRADU.i.nIdioma == 3)
    {
      titulo = "Furtivo";
      bajada = "Comeca escondido se o combate nao for emboscada inimiga.";
      tipo = "Passiva";
      inicio = $"{iconoOculto} Escondido ao iniciar combate";
      mientras = "A Espreita: +2 Ataque, +5% Critico, +10% de dano";
      extra = "Sem penalidade em combate noturno";
    }

    txtDescripcion = $"<size=115%><color=#5dade2><b>{titulo}</b></color></size>\n\n";
    txtDescripcion += $"<color=#8f8f8f><i>{bajada}</i></color>\n\n";
    txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
    txtDescripcion += $"<color=#44d3ec><b>Tipo:</b></color> <color=#ffffff>{tipo}</color>\n";
    txtDescripcion += $"<color=#44d3ec><b>Inicio:</b></color> <color=#ffffff>{inicio}</color>\n";
    txtDescripcion += $"<color=#44d3ec><b>Mientras esta oculto:</b></color> <color=#ffffff>{mientras}</color>\n";
    txtDescripcion += $"<color=#44d3ec><b>Extra:</b></color> <color=#ffffff>{extra}</color>";
  }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
    }
}
