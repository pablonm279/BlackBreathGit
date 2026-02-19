using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class chequearclick : MonoBehaviour
{
    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        var es = EventSystem.current;
        if (es == null)
        {
            Debug.LogWarning("No hay EventSystem en escena.");
            return;
        }

        var data = new PointerEventData(es)
        {
            position = Input.mousePosition
        };

        var results = new List<RaycastResult>();
        es.RaycastAll(data, results);

        Debug.Log($"Raycast UI hits: {results.Count}");
        for (int i = 0; i < results.Count; i++)
        {
            var r = results[i];
            Debug.Log($"{i}. {r.gameObject.name} | Canvas: {r.module.gameObject.name} | Depth: {r.depth} | SortingLayer: {r.sortingLayer} | SortingOrder: {r.sortingOrder}");
        }
    }
}
