using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONSueldo : Habilidad
{
   public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Acechador_Sueldo");
      ActualizarDescripcion();
      IDenClase = 0;
    }

    public bool seusoEsteTurno = false;

   public override void ActualizarDescripcion()
   {
      if (TRADU.i != null && TRADU.i.nIdioma == 2)
      {
         txtDescripcion = ConstruirDescripcionNormalizadaIngles(
            "Salary",
            "Campaign Passive: The Stalker works by contract, charging the caravan each rest.",
            new[]
            {
               LineaDescripcion("Cost", "20 gold per rest"),
               LineaDescripcion("Exception", "No payment if Hope is above 70.")
            });
         return;
      }

      if (TRADU.i != null && TRADU.i.nIdioma == 3)
      {
         txtDescripcion=ConstruirDescripcionNormalizadaLocalizada("Salário","Passiva de Campanha: o Espreitador trabalha por contrato e cobra da caravana a cada descanso.",new[]{LineaDescripcion("Custo","20 moedas de ouro por descanso"),LineaDescripcion("Exceção","Não cobra se a Esperança estiver acima de 70.")},costoSuperior:string.Empty); return;
      }
      txtDescripcion=ConstruirDescripcionNormalizadaLocalizada("Sueldo","Pasiva de Campaña: el Acechador trabaja por contrato y cobra a la caravana en cada descanso.",new[]{LineaDescripcion("Costo","20 monedas de oro por descanso"),LineaDescripcion("Excepción","No cobra si la Esperanza supera 70.")},costoSuperior:string.Empty); return;

      string titulo = "Sueldo";
      string bajada = "El Acechador cobra 20 monedas de oro al descansar.";
      string etiquetaTipo = "Tipo";
      string etiquetaCosto = "Costo";
      string etiquetaExcepcion = "Excepcion";
      string costo = "20 monedas de oro en cada descanso";
      string excepcion = "Si la Esperanza es mayor a 70, no cobra";
      string tipo = "Pasiva";

      if (TRADU.i.nIdioma == 2) //agrega la traduccion a ingles
      {
         titulo = "Salary";
         bajada = "The Stalker works by contract, charging the caravan each rest.";
         etiquetaTipo = "Type";
         etiquetaCosto = "Cost";
         etiquetaExcepcion = "Exception";
         costo = "20 gold coins each rest";
         excepcion = "If Hope is greater than 70, no payment is charged";
         tipo = "Passive";
      }
      if (TRADU.i.nIdioma == 3)
      {
         titulo = "Salario";
         bajada = "O Acechador cobra 20 moedas de ouro ao descansar.";
         etiquetaTipo = "Tipo";
         etiquetaCosto = "Custo";
         etiquetaExcepcion = "Excecao";
         costo = "20 moedas de ouro a cada descanso";
         excepcion = "Se a Esperanca for maior que 70, nao cobra";
         tipo = "Passiva";
      }

      txtDescripcion = $"<size=115%><color=#5dade2><b>{titulo}</b></color></size>\n\n";
      txtDescripcion += $"<color=#8f8f8f><i>{bajada}</i></color>\n\n";
      txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
      txtDescripcion += $"<color=#44d3ec><b>{etiquetaTipo}:</b></color> <color=#ffffff>{tipo}</color>\n";
      txtDescripcion += $"<color=#44d3ec><b>{etiquetaCosto}:</b></color> <color=#ffffff>{costo}</color>\n";
      txtDescripcion += $"<color=#44d3ec><b>{etiquetaExcepcion}:</b></color> <color=#ffffff>{excepcion}</color>";
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
    }
}
