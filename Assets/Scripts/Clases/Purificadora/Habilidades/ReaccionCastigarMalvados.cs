using System.Collections;
using System.Collections.Generic;
//using UnityEditor.SearchService;
using UnityEngine;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class ReaccionCastigarMalvados : Reaccion
{
  
   void Start()
   {
    int NIVEL = variableUnidad.GetComponent<CastigaraLosMalvados>().NIVEL;
    TipoTrigger =4;
    usos = 2;
    if(NIVEL == 4){usos++;}
    
    permanente = true;
    scEstaUnidad = gameObject.GetComponent<Unidad>();
    nombre = "Castigar a los Malvados";
    
    

    descripcion = $"Reacción: Cada vez que dañe a un enemigo, esta unidad deberá superar una tirada Mental o sufrir daño y perder sus AP restantes.";
    if (TRADU.i.nIdioma == 2)
    {
      descripcion = "Reaction: Each time this unit damages an enemy, it must pass a Mental saving throw or suffer damage and lose its remaining AP.";
    }
    else if (TRADU.i.nIdioma == 3)
    {
      descripcion = "Reacao: Cada vez que esta unidade causar dano a um inimigo, ela deve passar em uma resistencia Mental ou sofrer dano e perder seu AP restante.";
    }

   }

   public async override void AplicarEfectos(Unidad uTriggerer, bool melee, float variableFlexible1 = 0,  float variableFlexible2 = 0)
   {
      float DC = variableUnidad.mod_CarPoder + 10;
      if(NIVEL > 1){DC++;}

      if(scEstaUnidad.TiradaSalvacion(3, DC))
      {

      scEstaUnidad.EstablecerAPActualA(0); //Cuando a una IA le reacciona un personaje, se queda sin AP, para que no haga cosas mientras el pj reacciona
        
        float danioBase =UnityEngine.Random.Range(1, 7);
        danioBase += variableUnidad.mod_CarPoder;

        if(NIVEL == 5)
        { danioBase += variableFlexible2/2;}
        else{ danioBase += variableFlexible2/3;}
       
        VFXCastigoSagrado.Reproducir(scEstaUnidad.transform);
        scEstaUnidad.RecibirDanio(danioBase,11, false, variableUnidad);
       
        usos--;
        if(usos == 0)
        {
          Destroy(this);
        }
      }
      else  
      {
          Destroy(this);

      }
      
   }


}

internal sealed class VFXCastigoSagrado : MonoBehaviour
{
   private const float Duracion = 0.45f;
   private Material material;
   private LineRenderer hazExterior;
   private LineRenderer hazInterior;
   private LineRenderer halo;
   private LineRenderer destello;
   private float tiempo;

   public static void Reproducir(Transform objetivo)
   {
      GameObject contenedor = new GameObject("VFX_CastigoSagrado_Impacto");
      contenedor.transform.SetParent(objetivo, false);
      contenedor.transform.localPosition = Vector3.zero;
      contenedor.AddComponent<VFXCastigoSagrado>().Inicializar(objetivo);
   }

   private void Inicializar(Transform objetivo)
   {
      Shader shader = Shader.Find("Sprites/Default");
      if (shader == null)
      {
         Destroy(gameObject);
         return;
      }

      material = new Material(shader);
      int orden = 100;
      Canvas canvasObjetivo = objetivo.GetComponentInChildren<Canvas>();
      if (canvasObjetivo != null)
      {
         orden = canvasObjetivo.sortingOrder + 8;
      }

      hazExterior = CrearLinea("Halo dorado", 2, orden, false);
      hazExterior.SetPosition(0, new Vector3(0f, 1.9f, 0f));
      hazExterior.SetPosition(1, new Vector3(0f, 0.12f, 0f));

      hazInterior = CrearLinea("Nucleo de luz", 2, orden + 1, false);
      hazInterior.SetPosition(0, new Vector3(0f, 1.9f, 0f));
      hazInterior.SetPosition(1, new Vector3(0f, 0.12f, 0f));

      halo = CrearLinea("Sello de impacto", 25, orden + 1, true);
      destello = CrearLinea("Destello sagrado", 8, orden + 2, false);
   }

   private LineRenderer CrearLinea(string nombreLinea, int posiciones, int orden, bool bucle)
   {
      GameObject objetoLinea = new GameObject(nombreLinea);
      objetoLinea.transform.SetParent(transform, false);
      LineRenderer linea = objetoLinea.AddComponent<LineRenderer>();
      linea.useWorldSpace = false;
      linea.loop = bucle;
      linea.positionCount = posiciones;
      linea.sharedMaterial = material;
      linea.sortingOrder = orden;
      linea.numCapVertices = 4;
      linea.numCornerVertices = 2;
      return linea;
   }

   private void Update()
   {
      tiempo += Time.deltaTime;
      float progreso = Mathf.Clamp01(tiempo / Duracion);
      float entrada = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progreso / 0.16f));
      float salida = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.32f, 1f, progreso));
      float intensidad = entrada * salida;

      hazExterior.startWidth = 0.08f * intensidad;
      hazExterior.endWidth = 0.24f * intensidad;
      hazExterior.startColor = new Color(1f, 0.72f, 0.08f, 0.025f * intensidad);
      hazExterior.endColor = new Color(1f, 0.78f, 0.12f, 0.24f * intensidad);

      hazInterior.startWidth = 0.025f * intensidad;
      hazInterior.endWidth = 0.065f * intensidad;
      hazInterior.startColor = new Color(1f, 0.96f, 0.6f, 0.22f * intensidad);
      hazInterior.endColor = new Color(1f, 1f, 0.86f, 0.58f * intensidad);

      float radio = Mathf.Lerp(0.08f, 0.34f, Mathf.SmoothStep(0f, 1f, progreso));
      for (int i = 0; i < halo.positionCount; i++)
      {
         float angulo = i * Mathf.PI * 2f / halo.positionCount;
         halo.SetPosition(i, new Vector3(Mathf.Cos(angulo) * radio, 0.12f + Mathf.Sin(angulo) * radio * 0.3f, 0f));
      }
      halo.startWidth = halo.endWidth = 0.02f * intensidad;
      halo.startColor = halo.endColor = new Color(1f, 0.82f, 0.25f, 0.42f * intensidad);

      float largoDestello = Mathf.Lerp(0.28f, 0.52f, progreso);
      Vector3 centro = new Vector3(0f, 0.15f, 0f);
      destello.SetPosition(0, centro + Vector3.left * largoDestello);
      destello.SetPosition(1, centro + Vector3.right * largoDestello);
      destello.SetPosition(2, centro);
      destello.SetPosition(3, centro + Vector3.up * largoDestello * 0.65f);
      destello.SetPosition(4, centro);
      destello.SetPosition(5, centro + new Vector3(-0.48f, 0.42f, 0f) * largoDestello);
      destello.SetPosition(6, centro);
      destello.SetPosition(7, centro + new Vector3(0.48f, 0.42f, 0f) * largoDestello);
      destello.startWidth = destello.endWidth = 0.015f * intensidad;
      destello.startColor = destello.endColor = new Color(1f, 0.93f, 0.5f, 0.45f * intensidad);

      if (progreso >= 1f)
      {
         Destroy(gameObject);
      }
   }

   private void OnDestroy()
   {
      if (material != null)
      {
         Destroy(material);
      }
   }
}



