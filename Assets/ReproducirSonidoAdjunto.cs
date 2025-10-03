using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ReproducirSonidoAdjunto : MonoBehaviour
{
    [Header("Configuración de Sonido")]
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volumen = 1f;
    [Range(-3f, 3f)]
    public float pitch = 1f;
    public float delay = 0f;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = volumen;
        audioSource.pitch = pitch;
        if (clip != null)
        {
            audioSource.PlayDelayed(delay);
        }
    }

    // Si quieres cambiar los valores en tiempo real desde el inspector
    void Update()
    {
        audioSource.volume = volumen;
        audioSource.pitch = pitch;
    }
}
