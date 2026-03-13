using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapaManager : MonoBehaviour
{
    bool inicioCompletado;
    bool generacionDiferidaPendiente;
    bool omitirAutoGeneracionEnStart;
    const float OffsetNodoSobreRelieve = 0.08f;
    const float OffsetConvoySobreRelieve = 0.03f;

    public ContenedorDeNodos scContenedordeNodos;

    public Nodo nodoActual;
    public GameObject goCaravana;
    public GameObject goCaravanafollower1;
    public GameObject goCaravanafollower2;
    public GameObject goCaravanafollower3;
    public GameObject goCaravanafollower4;
    public GameObject goCaravanafollower5;
    public GameObject goCaravanafollower6;

    void Start()
    {
       inicioCompletado = true;
       if (omitirAutoGeneracionEnStart)
       {
           generacionDiferidaPendiente = false;
           return;
       }

       if (!generacionDiferidaPendiente)
       {
           StartCoroutine(GenerarNodosDiferido());
       }
    }

    


  public void GenerarNodos()
  {
       if (scContenedordeNodos == null) return;

       if (!inicioCompletado)
       {
           if (!generacionDiferidaPendiente)
           {
               StartCoroutine(GenerarNodosDiferido());
           }
           return;
       }

       scContenedordeNodos.RecolectarNodos();

       int zonaId = -1;
       if (CampaignManager.Instance != null && CampaignManager.Instance.scAtributosZona != null)
       {
           zonaId = CampaignManager.Instance.scAtributosZona.ID;
       }

       Nodo origen = scContenedordeNodos.ObtenerNodoSegunXY(0,0);
       if (origen == null) return;
       if (origen.yatiroConexiones)
       {
           if (nodoActual == null)
           {
               nodoActual = origen;
           }
           return;
       }

       PrepararNodosParaGeneracion();
       origen.DeterminarConexiones();
       nodoActual = origen;
       ForzarNodosObligatorios(zonaId);
       DesactivarNodosSinUsar(zonaId);
       origen.PosicionarObjetoEnNodo(goCaravana);
       AlinearConvoyAlSuelo();
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

        //3) Eliminar adornos de la zona anterior
        GameObject[] objetosMapa = GameObject.FindGameObjectsWithTag("MapaObjeto");
        foreach (GameObject objeto in objetosMapa)
        {
            Destroy(objeto);
        }
    }

    IEnumerator GenerarNodosDiferido()
    {
        generacionDiferidaPendiente = true;
        yield return null;
        generacionDiferidaPendiente = false;
        GenerarNodos();
    }

    public void OmitirAutoGeneracionEnStart()
    {
        omitirAutoGeneracionEnStart = true;
    }

    public void RehabilitarAutoGeneracionEnStart()
    {
        omitirAutoGeneracionEnStart = false;
    }

    void PrepararNodosParaGeneracion()
    {
        MapDecorator mapDecorator = ObtenerMapDecorator();

        foreach (Nodo nodo in scContenedordeNodos.listTodosNodos)
        {
            if (nodo == null) continue;
            nodo.PrepararCostoMovimientoParaGeneracion();
            if (mapDecorator != null)
            {
                mapDecorator.AlinearTransformASuelo(nodo.transform, OffsetNodoSobreRelieve);
            }
        }
    }

    public void AlinearNodosAlSueloActual()
    {
        if (scContenedordeNodos == null)
            return;

        scContenedordeNodos.RecolectarNodos();
        MapDecorator mapDecorator = ObtenerMapDecorator();
        if (mapDecorator == null)
            return;

        foreach (Nodo nodo in scContenedordeNodos.listTodosNodos)
        {
            if (nodo == null)
                continue;

            mapDecorator.AlinearTransformASuelo(nodo.transform, OffsetNodoSobreRelieve);
        }
    }

    MapDecorator ObtenerMapDecorator()
    {
        if (CampaignManager.Instance == null || CampaignManager.Instance.scAtributosZona == null)
            return null;

        return CampaignManager.Instance.scAtributosZona.GetComponent<MapDecorator>();
    }

    void AlinearConvoyAlSuelo()
    {
        MapDecorator mapDecorator = ObtenerMapDecorator();
        if (mapDecorator == null)
            return;

        mapDecorator.AlinearTransformASuelo(goCaravana != null ? goCaravana.transform : null, OffsetConvoySobreRelieve);
        mapDecorator.AlinearTransformASuelo(goCaravanafollower1 != null ? goCaravanafollower1.transform : null, OffsetConvoySobreRelieve);
        mapDecorator.AlinearTransformASuelo(goCaravanafollower2 != null ? goCaravanafollower2.transform : null, OffsetConvoySobreRelieve);
        mapDecorator.AlinearTransformASuelo(goCaravanafollower3 != null ? goCaravanafollower3.transform : null, OffsetConvoySobreRelieve);
        mapDecorator.AlinearTransformASuelo(goCaravanafollower4 != null ? goCaravanafollower4.transform : null, OffsetConvoySobreRelieve);
        mapDecorator.AlinearTransformASuelo(goCaravanafollower5 != null ? goCaravanafollower5.transform : null, OffsetConvoySobreRelieve);
        mapDecorator.AlinearTransformASuelo(goCaravanafollower6 != null ? goCaravanafollower6.transform : null, OffsetConvoySobreRelieve);
    }

    public void PosicionarCaravanaEnNodoActual()
    {
        if (nodoActual != null && goCaravana != null)
        {
            nodoActual.PosicionarObjetoEnNodo(goCaravana);
        }

        AlinearConvoyAlSuelo();
    }

    void DesactivarNodosSinUsar(int zonaId)
    {
       
        for (int i = scContenedordeNodos.listTodosNodos.Count - 1; i >= 0; i--)
        {   
            Nodo n = scContenedordeNodos.listTodosNodos[i];
            bool prohibido = n.ProhibidoEnZona != null && n.ProhibidoEnZona.Contains(zonaId);
            if (prohibido || (!n.yatiroConexiones && n.posXNodo != 11))
            {   
                n.gameObject.SetActive(false);
                scContenedordeNodos.listTodosNodos.RemoveAt(i);
            }
        }

    }

    void ForzarNodosObligatorios(int zonaId)
    {
        if (scContenedordeNodos == null) return;

        foreach (Nodo nodo in scContenedordeNodos.listTodosNodos)
        {
            if (nodo == null) continue;
            if (nodo.ProhibidoEnZona != null && nodo.ProhibidoEnZona.Contains(zonaId)) continue;
            if (nodo.ObligatorioEnZona == null || !nodo.ObligatorioEnZona.Contains(zonaId)) continue;
            if (nodo.yatiroConexiones) continue;

            Nodo origen = BuscarNodoConectadoMasCercano(nodo, zonaId);
            if (origen != null)
            {
                origen.ConectarConNodo(nodo);
            }
        }
    }

    Nodo BuscarNodoConectadoMasCercano(Nodo objetivo, int zonaId)
    {
        if (scContenedordeNodos == null) return null;

        Nodo candidato = null;
        int menorDistanciaX = int.MaxValue;
        int menorDistanciaY = int.MaxValue;

        foreach (Nodo nodo in scContenedordeNodos.listTodosNodos)
        {
            if (nodo == null) continue;
            if (!nodo.yatiroConexiones) continue;
            if (nodo.ProhibidoEnZona != null && nodo.ProhibidoEnZona.Contains(zonaId)) continue;

            int distanciaX = objetivo.posXNodo - nodo.posXNodo;
            if (distanciaX <= 0) continue;
            int distanciaY = Mathf.Abs(objetivo.posYNodo - nodo.posYNodo);

            bool esMejor = false;

            if (distanciaY < menorDistanciaY)
            {
                esMejor = true;
            }
            else if (distanciaY == menorDistanciaY && distanciaX < menorDistanciaX)
            {
                esMejor = true;
            }

            if (esMejor)
            {
                menorDistanciaX = distanciaX;
                menorDistanciaY = distanciaY;
                candidato = nodo;
                if (distanciaY == 0 && distanciaX == 1) break;
            }
        }

        return candidato;
    }
}


