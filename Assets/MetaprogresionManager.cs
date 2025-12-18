using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetaprogresionManager : MonoBehaviour
{

    public static MetaprogresionManager Instance { get; private set; }


    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    public int CorrupcionGlobal;
    public int CorrupcionMax;
    public int ValordeTrabajoDisponible;

    public int NivelPeligroBosqueArdiente;
    public int NivelPeligroPasoVientohelado;
    public int NivelPeligroNedukazal;

    public int CantidadCiviles;
    public int MisionesSalvamento;
    

    public int SerriaTierBarcos;
    public int SerriaTierAlmenaras;
    public int SerriaTierPalacio;
    public int SerriaTierCuartel;
    public int SerriaTierGranjas;
    public int SerriaTierBarricadas;
    public int SerriaTierTemplo;

    public int SerriaPuntosAlmacenadosBarcos;
    public int SerriaPuntosAlmacenadosAlmenaras;
    public int SerriaPuntosAlmacenadosPalacio;
    public int SerriaPuntosAlmacenadosCuartel;
    public int SerriaPuntosAlmacenadosGranjas;
    public int SerriaPuntosAlmacenadosBarricadas;
    public int SerriaPuntosAlmacenadosTemplo;



    public void AumentarAlmacenadosBarcos()
    { 
        SerriaPuntosAlmacenadosBarcos += 5;
        ValordeTrabajoDisponible -= 5;
        int threshold = 40 - 5 * SerriaTierGranjas + 15 * SerriaTierBarcos;
        if (SerriaPuntosAlmacenadosBarcos >= threshold)
        {
            SerriaTierBarcos++;
            SerriaPuntosAlmacenadosBarcos = 0;
        }
    }

    public void AumentarAlmacenadosAlmenaras()
    {
        if (SerriaTierAlmenaras > 2) { return; } // Limitar a 3 niveles
        SerriaPuntosAlmacenadosAlmenaras += 5;
        ValordeTrabajoDisponible -= 5;
        int threshold = 40 - 5 * SerriaTierGranjas + 15 * SerriaTierAlmenaras;
        if (SerriaPuntosAlmacenadosAlmenaras >= threshold)
        {
            SerriaTierAlmenaras++;
            SerriaPuntosAlmacenadosAlmenaras = 0;
        }
    }

    public void AumentarAlmacenadosPalacio()
    {
        if (SerriaTierPalacio > 2) { return; } // Limitar a 3 niveles
        SerriaPuntosAlmacenadosPalacio += 5;
        ValordeTrabajoDisponible -= 5;
        int threshold = 40 - 5 * SerriaTierGranjas + 15 * SerriaTierPalacio;
        if (SerriaPuntosAlmacenadosPalacio >= threshold)
        {
            MisionesSalvamento+=2;
            SerriaTierPalacio++;
            SerriaPuntosAlmacenadosPalacio = 0;
        }
    }

    public void AumentarAlmacenadosCuartel()
    {
        if (SerriaTierCuartel > 2) { return; } // Limitar a 3 niveles
        SerriaPuntosAlmacenadosCuartel += 5;
        ValordeTrabajoDisponible -= 5;
        int threshold = 40 - 5 * SerriaTierGranjas + 15 * SerriaTierCuartel;
        if (SerriaPuntosAlmacenadosCuartel >= threshold)
        {
            SerriaTierCuartel++;
            SerriaPuntosAlmacenadosCuartel = 0;
        }
    }

    public void AumentarAlmacenadosGranjas()
    {
        if (SerriaTierGranjas > 2) { return; } // Limitar a 3 niveles
        SerriaPuntosAlmacenadosGranjas += 5;
        ValordeTrabajoDisponible -= 5;
        // La granja no se afecta a sí misma para el valor base
        int threshold = 40 + 15 * SerriaTierGranjas;
        if (SerriaPuntosAlmacenadosGranjas >= threshold)
        {
            SerriaTierGranjas++;
            SerriaPuntosAlmacenadosGranjas = 0;
        }
    }

    public void AumentarAlmacenadosBarricadas()
    {
          if (SerriaTierBarricadas > 2) { return; } // Limitar a 3 niveles
        SerriaPuntosAlmacenadosBarricadas += 5;
        ValordeTrabajoDisponible -= 5;
        int threshold = 40 - 5 * SerriaTierGranjas + 15 * SerriaTierBarricadas;
        if (SerriaPuntosAlmacenadosBarricadas >= threshold)
        {
            SerriaTierBarricadas++;
            SerriaPuntosAlmacenadosBarricadas = 0;
        }
    }

    public void AumentarAlmacenadosTemplo()
    {
          if (SerriaTierTemplo > 2) { return; } // Limitar a 3 niveles
        SerriaPuntosAlmacenadosTemplo += 5;
        ValordeTrabajoDisponible -= 5;
        int threshold = 40 - 5 * SerriaTierGranjas + 15 * SerriaTierTemplo;
        if (SerriaPuntosAlmacenadosTemplo >= threshold)
        {
            SerriaTierTemplo++;
            SerriaPuntosAlmacenadosTemplo = 0;
        }
    }
}
