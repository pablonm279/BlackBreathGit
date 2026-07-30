using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Obstaculo : MonoBehaviour
{
  private const string RutaSonidoImpactoRoca = "rocahit";
  private static AudioClip sfxImpactoRoca;
  private static Material materialFallbackParticulas;
  private static Texture2D texturaCirculoSuave;
  private bool destruccionIniciada;
  protected virtual bool PermiteVfxImpactoFisico => true;

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
  if (scBattleManager == null || scBattleManager.EntradaBatallaBloqueadaPorUI)
  {
    return;
  }

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

    ReproducirSonidoImpactoRoca();
    hpCurr -= danioFinal;
    bool seDestruye = hpCurr <= 0;
    if (PermiteVfxImpactoFisico && !seDestruye)
    {
      ReproducirVfxImpactoFisico();
    }

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

    if (seDestruye)
    {
        ObstaculoDestruir(PermiteVfxImpactoFisico);
    }
}
  public Casilla CasillaPosicion;
  void ObstaculoDestruir()
  {
     ObstaculoDestruir(false);
  }

  void ObstaculoDestruir(bool reproducirVfxDestruccion)
  {
     if (destruccionIniciada)
     {
       return;
     }

     destruccionIniciada = true;
     if (reproducirVfxDestruccion && PermiteVfxImpactoFisico)
     {
       ReproducirVfxDestruccionEspecial();
     }

     Invoke("DesactivarGOconDelay", 0.5f);
     if (CasillaPosicion != null && CasillaPosicion.Presente == gameObject)
     {
       CasillaPosicion.Presente = null;
     }

  }

  /// <summary>
  /// Permite a otras lógicas forzar la destrucción del obstáculo reutilizando
  /// el mismo flujo visual que se dispara al llegar a 0 de vida.
  /// </summary>
  public void ForzarDestruccion(bool reproducirVfxDestruccion = false)
  {
    ObstaculoDestruir(reproducirVfxDestruccion);
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

    public void ReproducirSonidoImpactoRoca()
    {
      if (!EsRoca())
      {
        return;
      }

      if (sfxImpactoRoca == null)
      {
        sfxImpactoRoca = Resources.Load<AudioClip>(RutaSonidoImpactoRoca);
      }

      if (sfxImpactoRoca != null)
      {
        AjustesAudio.ReproducirClipEnPunto(sfxImpactoRoca, transform.position);
      }
    }

    private bool EsRoca()
    {
      return !string.IsNullOrEmpty(oName) && oName.ToLowerInvariant().Contains("roca");
    }

    private void ReproducirVfxDestruccionEspecial()
    {
      ReproducirVfxFisico("VFX_DestruirObstaculo", 1f);
    }

    private void ReproducirVfxImpactoFisico()
    {
      ReproducirVfxFisico("VFX_ImpactoObstaculo", 0.55f);
    }

    private void ReproducirVfxFisico(string nombre, float intensidad)
    {
      intensidad = Mathf.Clamp01(intensidad);
      GameObject vfxRoot = new GameObject(nombre);
      vfxRoot.transform.position = transform.position + new Vector3(0f, 0.12f, 0f);

      SpriteRenderer referenciaOrden = GetComponentInChildren<SpriteRenderer>();
      int sortingLayerId = referenciaOrden != null ? referenciaOrden.sortingLayerID : 0;
      int sortingOrder = referenciaOrden != null ? referenciaOrden.sortingOrder + 1 : 0;

      CrearPolvo(vfxRoot.transform, sortingLayerId, sortingOrder, intensidad);
      CrearPiedritas(vfxRoot.transform, sortingLayerId, sortingOrder, intensidad);

      AutodestruirDelay autodestruir = vfxRoot.AddComponent<AutodestruirDelay>();
      autodestruir.SetDelay(1.2f);
    }

    private void CrearPolvo(Transform padre, int sortingLayerId, int sortingOrder, float intensidad)
    {
      GameObject goPolvo = new GameObject("Polvo");
      goPolvo.transform.SetParent(padre, false);

      ParticleSystem ps = goPolvo.AddComponent<ParticleSystem>();
      ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
      ParticleSystemRenderer renderer = goPolvo.GetComponent<ParticleSystemRenderer>();
      renderer.sharedMaterial = ObtenerMaterialParticulasSuaves();
      renderer.sortingLayerID = sortingLayerId;
      renderer.sortingOrder = sortingOrder;
      renderer.renderMode = ParticleSystemRenderMode.Billboard;

      var main = ps.main;
      main.playOnAwake = true;
      main.loop = false;
      main.duration = 0.36f;
      main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.42f);
      main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.26f);
      main.startSize = new ParticleSystem.MinMaxCurve(0.025f * intensidad, 0.05f * intensidad);
      main.startColor = new Color(0.72f, 0.66f, 0.58f, 0.18f * intensidad);
      main.gravityModifier = 0.03f;
      main.simulationSpace = ParticleSystemSimulationSpace.World;
      main.maxParticles = 6;

      var emission = ps.emission;
      emission.enabled = false;

      var shape = ps.shape;
      shape.enabled = true;
      shape.shapeType = ParticleSystemShapeType.Circle;
      shape.radius = 0.05f;

      var velocity = ps.velocityOverLifetime;
      velocity.enabled = true;
      velocity.space = ParticleSystemSimulationSpace.World;
      velocity.x = new ParticleSystem.MinMaxCurve(0f);
      velocity.y = new ParticleSystem.MinMaxCurve(0.05f, 0.14f);
      velocity.z = new ParticleSystem.MinMaxCurve(0f);

      var colorOverLifetime = ps.colorOverLifetime;
      colorOverLifetime.enabled = true;
      Gradient gradiente = new Gradient();
      gradiente.SetKeys(
        new GradientColorKey[] {
          new GradientColorKey(new Color(0.72f, 0.66f, 0.58f), 0f),
          new GradientColorKey(new Color(0.62f, 0.58f, 0.52f), 1f)
        },
        new GradientAlphaKey[] {
          new GradientAlphaKey(0f, 0f),
          new GradientAlphaKey(0.18f * intensidad, 0.18f),
          new GradientAlphaKey(0.1f * intensidad, 0.62f),
          new GradientAlphaKey(0f, 1f)
        });
      colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradiente);

      var sizeOverLifetime = ps.sizeOverLifetime;
      sizeOverLifetime.enabled = true;
      AnimationCurve curvaTam = new AnimationCurve();
      curvaTam.AddKey(0f, 0.72f);
      curvaTam.AddKey(0.35f, 0.95f);
      curvaTam.AddKey(1f, 1.05f);
      sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curvaTam);
      ps.Play();
      ps.Emit(intensidad < 0.75f ? 2 : 4);
    }

    private void CrearPiedritas(Transform padre, int sortingLayerId, int sortingOrder, float intensidad)
    {
      GameObject goPiedras = new GameObject("Piedritas");
      goPiedras.transform.SetParent(padre, false);

      ParticleSystem ps = goPiedras.AddComponent<ParticleSystem>();
      ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
      ParticleSystemRenderer renderer = goPiedras.GetComponent<ParticleSystemRenderer>();
      renderer.sharedMaterial = ObtenerMaterialParticulasSuaves();
      renderer.sortingLayerID = sortingLayerId;
      renderer.sortingOrder = sortingOrder + 1;
      renderer.renderMode = ParticleSystemRenderMode.Billboard;

      var main = ps.main;
      main.playOnAwake = true;
      main.loop = false;
      main.duration = 0.34f;
      main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.34f);
      main.startSpeed = new ParticleSystem.MinMaxCurve(0.25f, 0.58f);
      main.startSize = new ParticleSystem.MinMaxCurve(0.006f * intensidad, 0.014f * intensidad);
      main.startColor = new Color(0.55f, 0.5f, 0.46f, 0.58f * intensidad);
      main.gravityModifier = 0.85f;
      main.simulationSpace = ParticleSystemSimulationSpace.World;
      main.maxParticles = 4;

      var emission = ps.emission;
      emission.enabled = false;

      var shape = ps.shape;
      shape.enabled = true;
      shape.shapeType = ParticleSystemShapeType.Circle;
      shape.radius = 0.03f;

      var velocity = ps.velocityOverLifetime;
      velocity.enabled = true;
      velocity.space = ParticleSystemSimulationSpace.World;
      velocity.x = new ParticleSystem.MinMaxCurve(-0.09f, 0.09f);
      velocity.y = new ParticleSystem.MinMaxCurve(0f);
      velocity.z = new ParticleSystem.MinMaxCurve(0f);

      var limit = ps.limitVelocityOverLifetime;
      limit.enabled = true;
      limit.limit = 0.95f;
      limit.dampen = 0.5f;
      ps.Play();
      ps.Emit(intensidad < 0.75f ? 2 : 3);
    }

    private static Material ObtenerMaterialParticulasSuaves()
    {
      if (materialFallbackParticulas != null)
      {
        return materialFallbackParticulas;
      }

      Shader shaderFallback = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
      if (shaderFallback == null)
      {
        shaderFallback = Shader.Find("Particles/Standard Unlit");
      }
      if (shaderFallback == null)
      {
        shaderFallback = Shader.Find("Sprites/Default");
      }
      if (shaderFallback == null)
      {
        return null;
      }

      materialFallbackParticulas = new Material(shaderFallback);
      materialFallbackParticulas.name = "ObstaculoDestruccion_Particulas";

      if (materialFallbackParticulas.HasProperty("_MainTex"))
      {
        materialFallbackParticulas.mainTexture = ObtenerTexturaCirculoSuave();
      }
      if (materialFallbackParticulas.HasProperty("_Color"))
      {
        materialFallbackParticulas.color = Color.white;
      }

      return materialFallbackParticulas;
    }

    private static Texture2D ObtenerTexturaCirculoSuave()
    {
      if (texturaCirculoSuave != null)
      {
        return texturaCirculoSuave;
      }

      const int size = 32;
      Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
      tex.name = "ObstaculoDestruccion_CirculoSuave";
      tex.wrapMode = TextureWrapMode.Clamp;
      tex.filterMode = FilterMode.Bilinear;

      float half = (size - 1) * 0.5f;
      for (int y = 0; y < size; y++)
      {
        for (int x = 0; x < size; x++)
        {
          float nx = (x - half) / half;
          float ny = (y - half) / half;
          float r = Mathf.Sqrt(nx * nx + ny * ny);
          float alpha = Mathf.Clamp01(1f - r);
          alpha = alpha * alpha * (3f - 2f * alpha);
          tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
        }
      }

      tex.Apply(false, false);
      texturaCirculoSuave = tex;
      return texturaCirculoSuave;
    }

}



