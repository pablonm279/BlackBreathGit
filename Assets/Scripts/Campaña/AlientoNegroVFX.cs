using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AlientoNegroVFX : MonoBehaviour
{
    public Slider sliderAlientoNegro; // Referencia al slider
    float estadoAlientoNegro;  // Estado actual del Aliento Negro

    public void AvanzarAlientoNegro(int cant)
    {
        // Calcula la nueva posición basada en la cantidad multiplicada por una unidad de movimiento.
        int mover = cant*160;
        Vector3 nuevaPosicion = transform.localPosition + Vector3.right * mover;
        

        // Calcula el nuevo valor del slider
        estadoAlientoNegro = CampaignManager.Instance.GetValorAlientoNegro();
        float nuevoValorSlider = estadoAlientoNegro / 20f;

        // Inicia la corrutina para mover el objeto y actualizar el slider.
        StartCoroutine(MoverAlientoNegro(nuevaPosicion, nuevoValorSlider, 3f)); // 3 segundos para completar el movimiento.
    }

    private IEnumerator MoverAlientoNegro(Vector3 destino, float nuevoValorSlider, float duracion)
    {
        Vector3 posicionInicial = transform.localPosition;
        float valorInicialSlider = sliderAlientoNegro.value;
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < duracion)
        {
            tiempoTranscurrido += Time.deltaTime;
            float t = tiempoTranscurrido / duracion;

            // Interpolación del movimiento
            transform.localPosition = Vector3.Lerp(posicionInicial, destino, t);

            // Interpolación del valor del slider
            sliderAlientoNegro.value = Mathf.Lerp(valorInicialSlider, nuevoValorSlider, t);

            yield return null;
        }

        // Asegúrate de que el objeto alcance la posición final exacta.
        transform.localPosition = destino;
        sliderAlientoNegro.value = nuevoValorSlider;
    }
}
