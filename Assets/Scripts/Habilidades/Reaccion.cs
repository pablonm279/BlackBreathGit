using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.VisualScripting;
using System.Threading.Tasks;

public abstract class Reaccion : MonoBehaviour
{
   

    public int TipoTrigger;
    // 1 - al "esquivar" ataque // 2 - al recibir daño  // 3 - al morir // 4 - Al dañar a un Enemigo // 5 - Al terminar Turno
    public int usos;

    public bool permanente; //Esta reaccion siempre está, no se remueve al comenzar el próximo turno
    public int NIVEL; //Nivel, por si es de una habilidad, para pasarle el valor
    public String nombre;

    public String descripcion;

    public Unidad scEstaUnidad;
    public Unidad variableUnidad; //Para reacciones que necesiten guardar otra unidad en la reaccion como "castigar malvados"
    public int variableFlexible3;

    public abstract void AplicarEfectos(Unidad uTriggerer, bool melee, float variableFlexible1 = 0, float variableFlexible2 = 0);
    


    












}
