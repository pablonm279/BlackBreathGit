using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesactivarenPlay : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        gameObject.SetActive(false);
    }

}
