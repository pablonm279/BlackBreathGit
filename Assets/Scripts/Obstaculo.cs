using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Obstaculo : MonoBehaviour
{

  public string oName;
  public float hpMax;
  public float hpCurr;
  public float iDureza; // el daño que absorbe
  public bool destruiblePorMismoLado = true; // el daño que absorbe

  public int intDuracionTurnos; 

  public bool bPermiteAtacarDetras; //Determina sin las unidades del mismo lado melee, suman 1 a su rango para atacar atravez de este obstaculo si esta adelante
  //tratar de que no haya muchos que lo impidan, para mayor fluidez
  BattleManager scBattleManager;
  private void Awake()
  {   
    scBattleManager = BattleManager.Instance;
  }
  
  private void Start()
  {
    // Initialize stats as they are set in the Unity Inspector
    hpCurr = hpMax;
    ActualizarBarraVidaPropia();
  }

private async void OnMouseDown() 
{

  if(scBattleManager.lObstaculosPosiblesHabilidadActiva.Contains(this) && scBattleManager.SeleccionandoObjetivo)
  {
    
    string sss = "Se resuelve la habilidad "+scBattleManager.HabilidadActiva.nombre+" hecha por "+scBattleManager.HabilidadActiva.gameObject+ " a "+ this;
  

   
   
    if(scBattleManager.HabilidadActiva.esZonal)
    {
      List<object> listResolver = new List<object>();
      listResolver.AddRange(scBattleManager.lObstaculosPosiblesHabilidadActiva);

      await scBattleManager.HabilidadActiva.Resolver(listResolver);
    }
    else
    {
      List<object> listaUno = new List<object> { this };
      await scBattleManager.HabilidadActiva.Resolver(listaUno);
    }



  }
  
}

 public GameObject PrefabtxtDaño;
 public GameObject unidadCanvas;
 public Transform puntoEntrante;
public TextMeshProUGUI txtDaño;
public virtual void RecibirDanio(float danio, int tipoDanio, bool esCritico, Unidad uCausante)
{
    float danioFinal = danio - iDureza;
    if (danioFinal < 0) danioFinal = 0;

    hpCurr -= danioFinal;

    // Mostrar el daño recibido (también cuando el resultado es 0)
    if (PrefabtxtDaño != null && unidadCanvas != null)
    {
        GameObject goDanioRecibido = Instantiate(PrefabtxtDaño, unidadCanvas.transform, false);
        TextMeshProUGUI textoTMP = goDanioRecibido.GetComponent<TextMeshProUGUI>();
        string textoDanio = ((int)danioFinal).ToString();
        Color colorDanio = textoTMP != null ? textoTMP.color : Color.white;
        FloatingTextContext contexto = danioFinal > 0 ? FloatingTextContext.Damage : FloatingTextContext.Resist;

        FloatingTextAnimator animator = goDanioRecibido.GetComponent<FloatingTextAnimator>();
        if (animator != null)
        {
            animator.Play(textoDanio, colorDanio, contexto);
        }
        else
        {
            if (textoTMP != null)
            {
                textoTMP.text = textoDanio;
                textoTMP.color = colorDanio;
            }

            if (TextoFlotanteManager.Instance != null)
            {
                TextoFlotanteManager.Instance.GenerarTextoFlotante(textoDanio, colorDanio, contexto);
            }
        }
    }

    ActualizarBarraVidaPropia();

    if (hpCurr <= 0)
    {
        ObstaculoDestruir();
    }
}
  public Casilla CasillaPosicion;
  void ObstaculoDestruir()
  {
     Invoke("DesactivarGOconDelay", 0.5f);
     CasillaPosicion.Presente = null;

  }

  /// <summary>
  /// Permite a otras lógicas forzar la destrucción del obstáculo reutilizando
  /// el mismo flujo visual que se dispara al llegar a 0 de vida.
  /// </summary>
  public void ForzarDestruccion()
  {
    ObstaculoDestruir();
  }
  void DesactivarGOconDelay()
  {
    gameObject.SetActive(false);
  }

[SerializeField] private Slider barraVida;
  void ActualizarBarraVidaPropia()
 {
    if (barraVida != null)
    {
      barraVida.value = hpCurr / hpMax;
    }
 }

  public void LlamarReacciones(int tipo, Unidad triggerer, bool melee)  //tipo de Trigger de la reaccion en cuestión
  {
    foreach(Reaccion reaccion in gameObject.GetComponents<Reaccion>())
    {
      if(reaccion.TipoTrigger == tipo)
      {
        reaccion.AplicarEfectos(triggerer, melee);
      }
    }
  }

  public bool ChequearTieneReaccionesTipo(int tipo)  //Para la IA - Si tipo -1, chequea simplemente si tiene reaciiones
  {
    foreach(Reaccion reaccion in gameObject.GetComponents<Reaccion>())
    {
      if(tipo == -1){return true;} //Si encuentra alguna reacción y el tipo buscado es -1 (cualquiera) devuelve true
      if(reaccion.TipoTrigger == tipo)
      {
        return true;
      }
    }
    return false;
  }

   public void ReducirDuracion(int cant)
    {
       intDuracionTurnos -= cant;

       if(intDuracionTurnos < 1)
       {
        DestruirObstaculo();
       }
    }

    public void DestruirObstaculo()
    {
     Destroy(gameObject);
    
    }

}



