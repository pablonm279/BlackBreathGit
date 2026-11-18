using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotaraCamara : MonoBehaviour
{
    public float rotationSpeed; 

    private void LateUpdate()
    {
       
        Camera camara = Camera.main;
        if (camara != null)
        {
            Vector3 cameraPosition = camara.transform.position;

            Vector3 lookDirection = cameraPosition - transform.position;
            lookDirection.y = 0f;

            // Evita el warning "Look rotation viewing vector is zero" con la camara justo encima.
            if (lookDirection.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);

           
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
          
        }
        
    }
}
