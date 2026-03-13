using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using System.Data.Common;
using UnityEngine.UI;
using System.Threading.Tasks;

public class EnemigoUnidadBrujaKaleTav : Unidad
{

    public Sprite imagenConCuervo;
    public Sprite imagenSinCuervo;

        public async Task MostrarImagenSinCuervoPorTresSegundos()
        {
            uImage.sprite = imagenSinCuervo;
            await BattleManager.DelayCombateAsync(3000);
            uImage.sprite = imagenConCuervo;
        }

}


