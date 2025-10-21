using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapaManager : MonoBehaviour
{
    
    public ContenedorDeNodos scContenedordeNodos;

    public Nodo nodoActual;
    public GameObject goCaravana;

    void Start()
    {
        GenerarNodos();
    }

    
  

    public void GenerarNodos()
    {

       Nodo origen = scContenedordeNodos.ObtenerNodoSegunXY(0,0);
       origen.DeterminarConexiones();
       nodoActual = origen;
       DesactivarNodosSinUsar();
    
       origen.PosicionarObjetoEnNodo(goCaravana);
    }

    // Reset total del mapa y regeneración para la siguiente zona
    public void ResetearYGenerarSiguienteZona()
    {
        // 1) Reactivar todos y resetear estado por nodo
        foreach (Nodo n in scContenedordeNodos.GetComponentsInChildren<Nodo>(true))
        {
            n.gameObject.SetActive(true);
            n.ResetearParaNuevaZona();
        }

        // 2) Reconstruir la lista del contenedor (se había podado previamente)
        scContenedordeNodos.RecolectarNodos();

        // 3) Regenerar conexiones y posicionar caravana en el origen
        GenerarNodos();
    }

    void DesactivarNodosSinUsar()
    {
       
        for (int i = scContenedordeNodos.listTodosNodos.Count - 1; i >= 0; i--)
        {   
            Nodo n = scContenedordeNodos.listTodosNodos[i];
            if (!n.yatiroConexiones && n.posXNodo != 11)
            {   
                n.gameObject.SetActive(false);
                scContenedordeNodos.listTodosNodos.RemoveAt(i);
            }
        }

    }

}
