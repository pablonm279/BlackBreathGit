using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCameraNOX : MonoBehaviour
{
    public bool invert;
    private Transform cameraTransform;

    private void Awake()
    {
        cameraTransform = Camera.main != null ? Camera.main.transform : null;
    }

    private void LateUpdate() // para que lo haga último en el frame
    {
        if (cameraTransform == null) return;

        // Mirar hacia la cámara sin inclinarse (sin girar en X)
        // Proyectamos el objetivo al mismo Y del objeto para evitar pitch.
        Vector3 target = cameraTransform.position;
        target.y = transform.position.y;

        transform.LookAt(target, Vector3.up);

        // Ajuste final: sólo yaw, sin pitch ni roll
        Vector3 e = transform.eulerAngles;
        float y = invert ? e.y + 180f : e.y;
        transform.rotation = Quaternion.Euler(0f, y, 0f);
    }
}

