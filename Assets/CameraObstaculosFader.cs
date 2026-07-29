using System.Collections.Generic;
using UnityEngine;

public class CameraObstaculosFader : MonoBehaviour
{
    const int CapacidadRaycastObstaculos = 32;

    public Transform objetivo;              // La caravana / centro
    public LayerMask capaObstaculos;        // Layer "ObstaculosCamara"

    List<FadeObstaculo> obstaculosActuales = new List<FadeObstaculo>();
    readonly RaycastHit[] hitsRaycastObstaculos = new RaycastHit[CapacidadRaycastObstaculos];

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

        // Limitar posición Z de la cámara
        Vector3 posicion = transform.position;
        if (posicion.z < -21.45f)
        {
            posicion.z = -21.45f;
            transform.position = posicion;
        }

        // Ray desde la cámara al objetivo
        Vector3 direccion = objetivo.position - transform.position;
        float distancia = direccion.magnitude;

        int cantidadHits = Physics.RaycastNonAlloc(
            transform.position,
            direccion.normalized,
            hitsRaycastObstaculos,
            distancia,
            capaObstaculos,
            QueryTriggerInteraction.UseGlobal
        );

        if (cantidadHits >= hitsRaycastObstaculos.Length)
        {
            RaycastHit[] hits = Physics.RaycastAll(
                transform.position,
                direccion.normalized,
                distancia,
                capaObstaculos,
                QueryTriggerInteraction.UseGlobal
            );

            for (int i = 0; i < hits.Length; i++)
                RegistrarObstaculo(hits[i].collider);
            return;
        }

        for (int i = 0; i < cantidadHits; i++)
            RegistrarObstaculo(hitsRaycastObstaculos[i].collider);
    }

    void RegistrarObstaculo(Collider collider)
    {
        if (collider == null || !collider.TryGetComponent(out FadeObstaculo obstaculo))
            return;

        obstaculo.Tapando();
        obstaculosActuales.Add(obstaculo);
    }
}


