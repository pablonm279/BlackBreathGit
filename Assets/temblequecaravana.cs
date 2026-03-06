using UnityEngine;

public class temblequecaravana : MonoBehaviour
{
    [Header("Temblor")]
    [SerializeField] private float frecuencia = 1.2f;
    [SerializeField] private float amplitudPosicion = 0.17f;
    [SerializeField] private float amplitudRotacion = 2.5f;

    private Transform _tr;
    private Vector3 _posLocalBase;
    private Quaternion _rotLocalBase;
    private float _semillaRuido;

    private void Awake()
    {
        _tr = transform;
        _posLocalBase = _tr.localPosition;
        _rotLocalBase = _tr.localRotation;
        _semillaRuido = Random.value * 5f;

        frecuencia = 4.8f;
        amplitudPosicion = 0.11f;
    }

    private void Update()
    {
        // La condición de movimiento queda tal cual la tenías.
        if (GetComponent<Animator>().GetBool("IsWalking"))
        {
            AplicarTemblor();
        }
        else
        {
            //ResetTransform();
        }
    }

    private void AplicarTemblor()
    {
        
        float tiempo = (Time.time + _semillaRuido) * frecuencia;

        float offsetX = (Mathf.PerlinNoise(tiempo, _semillaRuido) - 0.5f);
        float offsetY = (Mathf.PerlinNoise(tiempo * 0.8f, _semillaRuido + 1f) - 0.5f);
        float offsetZ = (Mathf.PerlinNoise(tiempo * 1.2f, _semillaRuido + 2f) - 0.5f);

        Vector3 offsetPosicion = new Vector3(0f, offsetY * 0.5f, 0f) * amplitudPosicion;

        float rotX = (Mathf.PerlinNoise(tiempo * 0.9f, _semillaRuido + 3f) - 0.5f) * amplitudRotacion;
        float rotZ = (Mathf.PerlinNoise(tiempo * 0.9f, _semillaRuido + 4f) - 0.5f) * amplitudRotacion;

        _tr.localPosition = _posLocalBase + offsetPosicion;
       // _tr.localRotation = _rotLocalBase * Quaternion.Euler(rotX, 0f, rotZ);
    }

    private void OnDisable()
    {
        //ResetTransform();
    }

    private void ResetTransform()
    {
        if (_tr == null) return;
        _tr.localPosition = _posLocalBase;
        _tr.localRotation = _rotLocalBase;
    }
}



