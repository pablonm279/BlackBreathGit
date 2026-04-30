using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oscilacioncamara : MonoBehaviour
{
 
    public float velocidad = 0.5f;
    public float amplitud = 0.05f;
    Vector3 inicio;
    Vector3 offsetExterno;
    bool inicioInicializado;

    public Vector3 PosicionBase
    {
        get
        {
            InicializarInicioSiHaceFalta();
            return inicio;
        }
    }

    public Vector3 OffsetExterno => offsetExterno;

    void Awake() { InicializarInicioSiHaceFalta(); }

    void Start() { InicializarInicioSiHaceFalta(); }

    public void EstablecerOffsetExterno(Vector3 offset)
    {
        InicializarInicioSiHaceFalta();
        offsetExterno = offset;
    }

    public void EstablecerPosicionBase(Vector3 posicion)
    {
        inicio = posicion;
        inicioInicializado = true;
    }

    void InicializarInicioSiHaceFalta()
    {
        if (inicioInicializado)
        {
            return;
        }

        inicio = transform.position;
        inicioInicializado = true;
    }

    void Update()
    {
        InicializarInicioSiHaceFalta();
        transform.position = inicio + offsetExterno + new Vector3(
            Mathf.Sin(Time.time * velocidad) * amplitud,
            Mathf.Cos(Time.time * velocidad * 0.5f) * amplitud,
            0);
    }


}
