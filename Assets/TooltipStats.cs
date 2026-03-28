using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TooltipStats: MonoBehaviour
{
    public static TooltipStats Instance;

    public GameObject tooltipObject;
    public TextMeshProUGUI tooltipText;
    private UIFadeSlide tooltipAnim;

    void Awake()
    {
        Instance = this;
        if (tooltipObject == null) { return; }

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

    public void ShowTooltip(string content, Vector3 position)
    {
        if (tooltipObject == null) { return; }

        tooltipText.text = TRADU.i.Traducir(content);
        UIFadeSlideUtility.ShowAt(tooltipObject, position);
    }

    public void ShowTooltipRaw(string content, Vector3 position)
    {
        if (tooltipObject == null) { return; }

        tooltipText.text = content;
        UIFadeSlideUtility.ShowAt(tooltipObject, position);
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

