using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oscilacioncamara : MonoBehaviour
{
 
    public float velocidad = 0.5f;
    public float amplitud = 0.05f;
    Vector3 inicio;
    void Start() { inicio = transform.position; }
    void Update()
    {
        transform.position = inicio + new Vector3(
            Mathf.Sin(Time.time * velocidad) * amplitud,
            Mathf.Cos(Time.time * velocidad * 0.5f) * amplitud,
            0);
    }


}
