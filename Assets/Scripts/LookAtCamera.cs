using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public bool invert;
    private Transform cameraTransform;

    private void Awake() 
    {
        cameraTransform = Camera.main.transform;

        
    }

  private void LateUpdate() // para que lo haga último en el frame
{
    if (invert)
    {
        Vector3 dirToCamera = (cameraTransform.position - transform.position).normalized;
        Vector3 lookDirection = new Vector3(-dirToCamera.x, 0, -dirToCamera.z); // Ignorar el eje Y
        transform.rotation = Quaternion.LookRotation(lookDirection);
    }
    else
    {
        Vector3 dirToCamera = (cameraTransform.position - transform.position).normalized;
        Vector3 lookDirection = new Vector3(dirToCamera.x, 0, dirToCamera.z); // Ignorar el eje Y
        transform.rotation = Quaternion.LookRotation(lookDirection);
    }
}

}
