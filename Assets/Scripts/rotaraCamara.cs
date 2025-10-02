using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotaraCamara : MonoBehaviour
{
    public float rotationSpeed; 

    private void LateUpdate()
    {
       
        if (Camera.main != null)
        {
            Vector3 cameraPosition = Camera.main.transform.position;

            Vector3 lookDirection = cameraPosition - transform.position;
            lookDirection.y = 0f; 

           
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);

           
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
          
        }
        
    }
}
