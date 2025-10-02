using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContenedorDeNodos : MonoBehaviour
{
    

  
  

    void Start()
    {
        ObtenerTodosLosNodos();


    }

    
    public List<Nodo> listTodosNodos = new List<Nodo>();
    void ObtenerTodosLosNodos()
    {
        foreach(Nodo child in transform.GetComponentsInChildren<Nodo>())
        {
            listTodosNodos.Add(child.GetComponent<Nodo>());
        }
    }
   
   
    public Nodo ObtenerNodoSegunXY(int X, int Y)
    {
        foreach(Nodo n in listTodosNodos)
        {
            if(n.posXNodo == X  &&  n.posYNodo == Y)
            {
                return n;

            }

        }
       
     return null;
    } 
   
}
