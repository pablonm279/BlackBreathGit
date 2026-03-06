using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Serialization;

[System.Serializable]
public class DebuffImpactoArmaData
{
     public bool activo = true;
     public string nombreDebuff = "Debuff de impacto";

     [Range(0, 100)]
     public int probabilidadAplicar = 100;

     [Min(1)]
     public int duracionRondas = 1;

     [Header("Salvacion")]
     public bool requiereTiradaSalvacion = false;

     [Tooltip("0 = Sin TS, 1 = Fortaleza, 2 = Reflejos, 3 = Mental")]
     [Range(0, 3)]
     public int tipoTiradaSalvacion = 1;

     public int dificultadSalvacion = 12;

     [Header("Modificadores al objetivo (valores planos)")]
     public int modFuerza;
     public int modAgilidad;
     public int modPoder;
     public int modIniciativa;
     public int modAtaque;
     public int modDefensa;
     public int modArmadura;
     public int modDanioPorcentaje;
     public int modTSReflejos;
     public int modTSFortaleza;
     public int modTSMental;
     public int modResFuego;
     public int modResHielo;
     public int modResRayo;
     public int modResAcido;
     public int modResArcano;
     public int modResNecro;
     public int modResDivino;
     public int modCritDado;
     public int modCritDanioPorcentaje;

     [Header("Estados por impacto")]
     public int stacksSangrado;
     public int stacksArdiendo;
     public int stacksCongelado;
     public int stacksAcido;
     public int stacksAturdido;
     public int reduccionAPPorTurno;
     public int reduccionResistencias;
     public int stacksCondenado;

     [Header("Impacto directo")]
     [Min(0)]
     public int ignorarArmaduraPlano;

     [Min(0)]
     public int roboVidaPorcentaje;

     [Min(0)]
     public int empujeCasillas;

     [Min(0)]
     public int jalonCasillas;

     public bool TieneModificadores()
     {
         return modFuerza != 0
             || modAgilidad != 0
             || modPoder != 0
             || modIniciativa != 0
             || modAtaque != 0
             || modDefensa != 0
             || modArmadura != 0
             || modDanioPorcentaje != 0
             || modTSReflejos != 0
             || modTSFortaleza != 0
             || modTSMental != 0
             || modResFuego != 0
             || modResHielo != 0
             || modResRayo != 0
             || modResAcido != 0
             || modResArcano != 0
             || modResNecro != 0
             || modResDivino != 0
             || modCritDado != 0
             || modCritDanioPorcentaje != 0;
     }

     public bool TieneEstadosImpacto()
     {
         return stacksSangrado != 0
             || stacksArdiendo != 0
             || stacksCongelado != 0
             || stacksAcido != 0
             || stacksAturdido != 0
             || reduccionAPPorTurno != 0
             || reduccionResistencias != 0
             || stacksCondenado != 0;
     }

     public bool TieneEfectosImpactoDirecto()
     {
         return ignorarArmaduraPlano != 0
             || roboVidaPorcentaje != 0
             || empujeCasillas != 0
             || jalonCasillas != 0;
     }

     public bool TieneEfectos()
     {
         return TieneModificadores() || TieneEstadosImpacto() || TieneEfectosImpactoDirecto();
     }
}

public abstract class Item : MonoBehaviour
{
     public Sprite imItem;
     public string sNombreItem;
     public int IDEfectoEspecial; //Solamente a las que son necesarias referenciar en AdministradorEscenas, como las armaduras con efectos especiales
     public List<int> IDClasesQuePuedenUsarEsteItem = new List<int>();


     public int iPrecio;

     [FormerlySerializedAs("itemDescrpicion")]
     [TextArea(7, 12)]
     public string itemDescripcion;

     public int iRareza = 0; // 0 = Común, 1 = Infrecuente, 2 = Raro, 3 = Épico, 4 = Legendario, 5 = Artefacto
     public int nivelMejora = 0; // +1,+2,+3,+4,+5
     [Header("Inicio de combate")]
     public int barreraInicioCombate;
     public int evasionInicioCombate;
     public int bonusDanioFuegoInicioCombate;
     public int bonusDanioHieloInicioCombate;
     public int bonusDanioRayoInicioCombate;
     public int bonusDanioAcidoInicioCombate;
     public int bonusDanioArcanoInicioCombate;
     public int bonusDanioNecroInicioCombate;
     public int bonusDanioDivinoInicioCombate;
     public int regeneracionVidaInicioCombate;
     public int regeneracionArmaduraInicioCombate;

     [Header("Defensa al equipar")]
     [Tooltip("Porcentaje (%) de reduccion de dano recibido.")]
     [Range(0, 95)]
     public int reduccionDanioRecibidoPorcentaje;

     [Tooltip("Porcentaje (%) de reduccion de dano critico recibido.")]
     [Range(0, 95)]
     public int reduccionDanioCriticoRecibidoPorcentaje;

     [Tooltip("Porcentaje (%) de chance de resistir estados/debuffs.")]
     [Range(0, 100)]
     public int resistenciaEstadosPorcentaje;

     [Tooltip("Dano fijo reflejado al atacante en impactos fisicos recibidos.")]
     [Min(0)]
     public int espinasDanioPlano;

     [Tooltip("Porcentaje (%) del dano recibido que se refleja al atacante en impactos fisicos.")]
     [Min(0)]
     public int espinasDanioPorcentaje;

     public bool UsaTodasLasClases()
     {
         return IDClasesQuePuedenUsarEsteItem == null
             || IDClasesQuePuedenUsarEsteItem.Count == 0
             || IDClasesQuePuedenUsarEsteItem.Contains(-1);
     }

     public bool PuedeUsarClase(int idClase)
     {
         if (UsaTodasLasClases())
         {
             return true;
         }

         return IDClasesQuePuedenUsarEsteItem.Contains(idClase);
     }
}



