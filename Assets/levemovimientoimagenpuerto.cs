using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class levemovimientoimagenpuerto : MonoBehaviour
{
    [SerializeField] private Vector2 movementAmplitude = new Vector2(10f, 6f); // Max displacement on X/Y
    [SerializeField] private float speed = 0.15f; // Lower is slower and smoother

    private Vector3 _initialPosition;
    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _initialPosition = _rectTransform ? _rectTransform.anchoredPosition3D : transform.localPosition;
    }

    private void OnEnable()
    {
        // Reset to the starting point so the loop always begins from the same spot
        if (_rectTransform)
            _rectTransform.anchoredPosition3D = _initialPosition;
        else
            transform.localPosition = _initialPosition;
    }

    private void Update()
    {
        float t = Time.time * speed;

        // Slight pan left-right and up-down using different phases to avoid a repetitive feel
        Vector3 offset = new Vector3(
            Mathf.Sin(t) * movementAmplitude.x,
            Mathf.Cos(t * 0.7f) * movementAmplitude.y,
            0f);

        if (_rectTransform)
            _rectTransform.anchoredPosition3D = _initialPosition + offset;
        else
            transform.localPosition = _initialPosition + offset;
    }
}
