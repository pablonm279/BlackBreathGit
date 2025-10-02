using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TooltipNodos : MonoBehaviour
{
    public static TooltipNodos Instance;

    public GameObject tooltipObject;
    public TextMeshProUGUI tooltipText;

    void Awake()
    {
        Instance = this;
        tooltipObject.SetActive(false);
    }

    public void ShowTooltip(string content, Vector3 position)
    {
        tooltipObject.SetActive(true);
        tooltipText.text = content;
        tooltipObject.transform.position = position;
    }

    public void HideTooltip()
    {
        tooltipObject.SetActive(false);
    }
    
    void Update()
    {
        if (tooltipObject.activeSelf)
        {
            tooltipObject.transform.position = Input.mousePosition;
        }
    }
 }

