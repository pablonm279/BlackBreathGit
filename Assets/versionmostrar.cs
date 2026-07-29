using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class versionmostrar : MonoBehaviour
{
    void Start()
    {
        GetComponent<TMP_Text>().text = "v" + Application.version;
    }
}
