using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

public abstract class Trampa : MonoBehaviour
{
    public BattleManager scBattleManager;
    public String nombre;
    public GameObject prefabModelo;
    public int intDificultadVer;
    public bool esTrampaFavorable = false; //true si es una trampa favorable, false si es desfavorable
    public int intUsos;
    public int intDuracionTurnos;   //duracion de la trampa en turnos, si es persistente se aplica cada turno
    public bool esPersistente; //persistente significa que aplica sus efectos cada turno a quien esté arriba o solo cuando lo pisan
   

    public GameObject GOvfx; //Aca se guarda el GO del vfx que se crea, para poder eliminarlo junto a la trampa
    private void Awake()
    {   
     scBattleManager = BattleManager.Instance;
    }

    public void ReducirUsos()
    { 
        intUsos--;
        if (intUsos < 1)
        {
            DestruirTrampa(); 
        }
    }
    
    public abstract void AplicarEfectosTrampa(Unidad unidad);
    
    void Start()
    {
        Invoke("AplicarEfectosUnidadEnCasilla", 0.3f);
    }

    private void AplicarEfectosUnidadEnCasilla()
    {
        if (gameObject.GetComponent<Casilla>() != null)
        {
            if (gameObject.GetComponent<Casilla>().Presente != null)
            { 
                if (gameObject.GetComponent<Casilla>().Presente.GetComponent<Unidad>() != null)
                {
                    if (!gameObject.GetComponent<Casilla>().Presente.GetComponent<Unidad>().inmunidad_Trampas)
                    {
                        //Si la casilla tiene una unidad, se aplica la trampa
                        AplicarEfectosTrampa(gameObject.GetComponent<Casilla>().Presente.GetComponent<Unidad>());
                    }
                }

            }
        }
    }
    public void ReducirDuracion(int cant)
    {
        intDuracionTurnos -= cant;

        if (intDuracionTurnos < 1)
        {
            DestruirTrampa();
        }
    }

    public void DestruirTrampa()
    {
     Destroy(GOvfx);
     Destroy(this); //Destruye el componente Trampa
    }



    private async void StartCanvasSorting()
    {
        await Task.Delay(100); // Delay mínimo de 100ms

        var canvas = GetComponentInChildren<Canvas>();
        var casilla = GetComponent<Casilla>();
        if (canvas != null && casilla != null)
        {
            canvas.overrideSorting = true;
            int posY = Mathf.RoundToInt(casilla.transform.position.y);
            canvas.sortingOrder = 60 - 10 * posY + 2;
        }
    }

    private void OnEnable()
    {
        StartCanvasSorting();
    }



    
}
