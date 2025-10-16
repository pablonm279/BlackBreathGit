using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TooltipItems: MonoBehaviour
{
    public static TooltipItems Instance;

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
        tooltipText.text = TRADU.i.Traducir(content);
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

