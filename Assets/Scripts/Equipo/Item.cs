using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public abstract class Item : MonoBehaviour
{
     public Sprite imItem;
     public string sNombreItem;

     public List<int> IDClasesQuePuedenUsarEsteItem = new List<int>();


     public int iPrecio;

     [TextArea(7, 12)]
     public string itemDescripcion;

     public int iRareza = 0; // 0 = Común, 1 = Infrecuente, 2 = Raro, 3 = Épico, 4 = Legendario, 5 = Artefacto
     public int nivelMejora = 0; // +1,+2,+3,+4,+5
     
}
