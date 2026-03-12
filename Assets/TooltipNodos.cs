using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TooltipNodos : MonoBehaviour
{
    public static TooltipNodos Instance;

    public GameObject tooltipObject;
    public TextMeshProUGUI tooltipText;
    private UIFadeSlide tooltipAnim;

    void Awake()
    {
        Instance = this;
        if (tooltipObject == null) { return; }

        ConfigurarTooltipSinRaycasts();

        tooltipAnim = UIFadeSlideUtility.Ensure(tooltipObject);
        if (tooltipAnim != null)
        {
            tooltipAnim.SetDurations(0.14f, 0.14f);
            tooltipAnim.SetOffsets(new Vector2(0f, -10f), new Vector2(0f, -8f));
            tooltipAnim.SetFollowMouse(true, new Vector2(14f, -18f));
            tooltipAnim.HideImmediate();
        }
        else
        {
            tooltipObject.SetActive(false);
        }
    }

    void ConfigurarTooltipSinRaycasts()
    {
        CanvasGroup canvasGroup = tooltipObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = tooltipObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        Graphic[] graficos = tooltipObject.GetComponentsInChildren<Graphic>(true);
        foreach (Graphic grafico in graficos)
        {
            grafico.raycastTarget = false;
        }
    }

    public void ShowTooltip(string content, Vector3 position, Nodo nodo)
    {
        if (tooltipObject == null) { return; }

        tooltipText.text = content;
        UIFadeSlideUtility.ShowAt(tooltipObject, position);

        if (nodo.nodoIncendiado)
        {
            tooltipText.text += TRADU.i.Traducir("\n<color=#FF3D00>--Incendiado--</color>");
        }
         if (nodo.nodoRitual)
        {
            tooltipText.text += TRADU.i.Traducir("\n<color=#6A0DAD>--Ritual--</color>");
        }


        

       
    }

    public void HideTooltip()
    {
        UIFadeSlideUtility.Hide(tooltipObject);
    }
    
    void Update()
    {
        if (tooltipObject != null && tooltipObject.activeSelf && tooltipAnim == null)
        {
            tooltipObject.transform.position = Input.mousePosition;
        }
    }
 }

