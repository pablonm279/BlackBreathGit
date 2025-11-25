using System.Collections.Generic;
using UnityEngine;

public class CameraObstaculosFader : MonoBehaviour
{
    public Transform objetivo;              // La caravana / centro
    public LayerMask capaObstaculos;        // Layer "ObstaculosCamara"

    List<FadeObstaculo> obstaculosActuales = new List<FadeObstaculo>();

    void LateUpdate()
    {
        if (objetivo == null) return;

        // Dejo de tapar todos los que tapaban en el frame anterior
        foreach (var o in obstaculosActuales)
        {
            if (o != null)
                o.DejarDeTapar();
        }
        obstaculosActuales.Clear();

        // Ray desde la cámara al objetivo
        Vector3 direccion = objetivo.position - transform.position;
        float distancia = direccion.magnitude;

        RaycastHit[] hits = Physics.RaycastAll(
            transform.position,
            direccion.normalized,
            distancia,
            capaObstaculos
        );

        foreach (var hit in hits)
        {
            FadeObstaculo f = hit.collider.GetComponent<FadeObstaculo>();
            if (f != null)
            {
                f.Tapando();
                obstaculosActuales.Add(f);
            }
        }
    }
}
