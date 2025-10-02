using System.Transactions;
using UnityEngine;

public class ArrowFlight : MonoBehaviour
{
    public Transform startMarker; // Punto de inicio (personaje)
    public Transform endMarker;   // Punto final (enemigo)
 

    private float startTime;
    private float journeyLength;

    public float parabola;
    public float velocidad;
    public float offsetStartY; //para que salga mas arriba o abajo
    public float offsetEndY; //para que pegue mas arriba o abajo
    

    void Start()
    {
        startTime = Time.time;
       journeyLength = Vector3.Distance(startMarker.position + new Vector3(0, offsetStartY, 0), endMarker.position + new Vector3(0, offsetEndY, 0));

      transform.LookAt(endMarker.position);

     
    }

    void Destruir()
    {
      Destroy(gameObject);

    }

   void Update()
{
    // Calcula el tiempo de vuelo desde el inicio
    float distCovered = (Time.time - startTime) * velocidad; // Velocidad

    // Calcula el ratio del viaje completado
    float fracJourney = distCovered / journeyLength;

    // Calcula la posición de la flecha usando una parábola
    Vector3 nextPosition = CalculateParabolicPath(startMarker.position + new Vector3(0, offsetStartY, 0), endMarker.position + new Vector3(0, offsetEndY, 0), parabola, fracJourney);
    transform.position = nextPosition;

    // Verifica si la flecha está cerca del objetivo
    float remainingDistance = Vector3.Distance(nextPosition, endMarker.position + new Vector3(0, offsetEndY, 0));

    if (remainingDistance > 0.12f)
    {
        // Rota la flecha hacia la dirección de movimiento si está lejos del objetivo
        transform.LookAt(CalculateParabolicPath(startMarker.position + new Vector3(0, offsetStartY, 0),endMarker.position + new Vector3(0, offsetEndY, 0), parabola, fracJourney + 0.01f));
    }
    else
    {
      Destruir();
    }
}



    Vector3 CalculateParabolicPath(Vector3 start, Vector3 end, float height, float t)
    {
        float parabolicT = Mathf.Sin(t * Mathf.PI);

        return Vector3.Lerp(start, end, t) + Vector3.up * parabolicT * height;
    }
}
