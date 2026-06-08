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
    string titulo = "Sigiloso";
    string bajada = "Empieza en sigilo si el combate no es una emboscada enemiga.";
    string etiquetaTipo = "Tipo";
    string etiquetaInicio = "Inicio";
    string etiquetaMientras = "Mientras esta en sigilo";
    string etiquetaExtra = "Extra";
    string tipo = "Pasiva";
    string inicio = "En sigilo al iniciar combate";
    string mientras = "+2 Ataque, +5% Crítico, +10% daño";
    string extra = "Sin penalidad por combate nocturno";

    if (TRADU.i.nIdioma == 2) //agrega la traduccion a ingles
    {
      titulo = "Stealthy";
      bajada = "Begins in stealth if the combat is not an enemy ambush.";
      etiquetaTipo = "Type";
      etiquetaInicio = "Start";
      etiquetaMientras = "While in stealth";
      etiquetaExtra = "Extra";
      tipo = "Passive";
      inicio = "In stealth at combat start";
      mientras = "+2 Attack, +5% Critical, +10% damage";
      extra = "No night combat penalty";
    }
    if (TRADU.i.nIdioma == 3)
    {
      titulo = "Furtivo";
      bajada = "Começa em furtividade se o combate não for uma emboscada inimiga.";
      etiquetaTipo = "Tipo";
      etiquetaInicio = "Início";
      etiquetaMientras = "Enquanto estiver em furtividade";
      etiquetaExtra = "Extra";
      tipo = "Passiva";
      inicio = "Em furtividade ao iniciar combate";
      mientras = "+2 Ataque, +5% Critico, +10% de dano";
      extra = "Sem penalidade em combate noturno";
    }

    txtDescripcion = $"<size=115%><color=#5dade2><b>{titulo}</b></color></size>\n\n";
    txtDescripcion += $"<color=#8f8f8f><i>{bajada}</i></color>\n\n";
    txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
    txtDescripcion += $"<color=#44d3ec><b>{etiquetaTipo}:</b></color> <color=#ffffff>{tipo}</color>\n";
    txtDescripcion += $"<color=#44d3ec><b>{etiquetaInicio}:</b></color> <color=#ffffff>{inicio}</color>\n";
    txtDescripcion += $"<color=#44d3ec><b>{etiquetaMientras}:</b></color> <color=#ffffff>{mientras}</color>\n";
    txtDescripcion += $"<color=#44d3ec><b>{etiquetaExtra}:</b></color> <color=#ffffff>{extra}</color>";
  }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
    }
}
