using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIInfoChar : MonoBehaviour
{
  [SerializeField] private Image ImagenFondo; 
  [SerializeField] private Slider barraVida;
  [SerializeField] private Image Retrato; 
  [SerializeField] private Image vIndicadorBando;
  [SerializeField] private Sprite vSpriteStatAliado;
  [SerializeField] private Sprite vSpriteStatEnemigo;
  [SerializeField] private TextMeshProUGUI vNombre;
  [SerializeField] private TextMeshProUGUI vTags;
  [SerializeField] private TextMeshProUGUI vHP;
  [SerializeField] private TextMeshProUGUI vHPMax;
  [SerializeField] private TextMeshProUGUI vDefensa;
  [SerializeField] private TextMeshProUGUI vArmadura;
  [SerializeField] private TextMeshProUGUI vReflejos;
  [SerializeField] private TextMeshProUGUI vFortaleza;
  [SerializeField] private TextMeshProUGUI vMental;
  [SerializeField] private TextMeshProUGUI vResFue;
  [SerializeField] private TextMeshProUGUI vResHie;
  [SerializeField] private TextMeshProUGUI vResRay;
  [SerializeField] private TextMeshProUGUI vResAci;
  [SerializeField] private TextMeshProUGUI vResArca;
  [SerializeField] private TextMeshProUGUI vResNecro;
  [SerializeField] private TextMeshProUGUI vResDivino;
  [SerializeField] private TextMeshProUGUI vMerito;
  [SerializeField] private Color vMeritoColorGanancia = new Color(0.27f, 0.94f, 0.58f, 1f);
  [SerializeField] private Color vMeritoColorPerdida = new Color(1f, 0.36f, 0.36f, 1f);
  [SerializeField] private float vMeritoPulseEscala = 1.18f;
  [SerializeField] private float vMeritoPulseDuracion = 0.22f;
  [SerializeField] private GameObject vValentiaContenedor;
  
  [SerializeField] private GameObject vDescenemigoGO;
  [SerializeField] private TextMeshProUGUI vDescEnemigo;
  
  public GameObject contenedorCasillasEstados;
  public GameObject casillaEstadoPrefab;

  public GameObject infoEnemigos;
  public GameObject btninfoEnemigos;
  private Color vMeritoColorBase = Color.white;
  private Vector3 vMeritoEscalaBase = Vector3.one;
  private bool vMeritoVisualInicializado;
  private Coroutine vMeritoPulseCoroutine;
  private Unidad unidadUltimoMerito;
  private int valorUltimoMerito;


  

  public bool hayUnidadSeleccionadaParaInfo;
  public Unidad unidadMostrada;
  private void Awake()
  {
    InicializarVisualMerito();
  }

  public void ActualizarInfoChar(Unidad scUnidadMostrada)
  {
    if(unidadMostrada != null){unidadMostrada.Marcar(0);}
    if(scUnidadMostrada != null)
    {
     bool cambioUnidad = unidadMostrada != scUnidadMostrada;
     unidadMostrada = scUnidadMostrada;
     //ActualizarColoresFondo();
     gameObject.SetActive(true);

    
     BotonSalir.SetActive(hayUnidadSeleccionadaParaInfo);
    
   vNombre.text = TRADU.i.Traducir(scUnidadMostrada.uNombre);
   vHP.text = ((int)scUnidadMostrada.HP_actual) + "/";
   vHPMax.text = ((int)scUnidadMostrada.mod_maxHP) + "";
   vDefensa.text = ((int)scUnidadMostrada.ObtenerdefensaActual()) + "";
   vArmadura.text = ((int)scUnidadMostrada.ObtenerArmaduraActual()) + "";
   vReflejos.text = ((int)scUnidadMostrada.mod_TSReflejos) + "";
   vFortaleza.text = ((int)scUnidadMostrada.mod_TSFortaleza) + "";
   vMental.text = ((int)scUnidadMostrada.mod_TSMental) + "";
   vResFue.text = ((int)scUnidadMostrada.ObtenerResistenciaA(1)) + "";
   vResHie.text = ((int)scUnidadMostrada.ObtenerResistenciaA(2)) + "";
   vResRay.text = ((int)scUnidadMostrada.ObtenerResistenciaA(3)) + "";
   vResAci.text = ((int)scUnidadMostrada.ObtenerResistenciaA(4)) + "";
   vResArca.text = ((int)scUnidadMostrada.ObtenerResistenciaA(5)) + "";
   vResNecro.text = ((int)scUnidadMostrada.ObtenerResistenciaA(6)) + "";
   vResDivino.text = ((int)scUnidadMostrada.ObtenerResistenciaA(7)) + "";

     bool esEnemigo = EsUnidadEnemiga(scUnidadMostrada);
     ActualizarTags(scUnidadMostrada, esEnemigo);
     if(esEnemigo)
     {
        vValentiaContenedor.SetActive(false);
        ResetearVisualMerito();
        unidadUltimoMerito = null;
        vDescEnemigo.text = ActualizarDescripcionAI(); 
        btninfoEnemigos.SetActive(true);
     }
     else
     { 
        mostrardesc = false;
        ActualizarVisualMerito(scUnidadMostrada);
        vValentiaContenedor.SetActive(true);
        btninfoEnemigos.SetActive(false);
       
     }

     if (cambioUnidad && esEnemigo)
     {
       mostrardesc = false;
     }

     if (!hayUnidadSeleccionadaParaInfo)
     {
       mostrardesc = false;
     }

     ActualizarVisibilidadInfoEnemigos(esEnemigo && mostrardesc);
     
     barraVida.value = scUnidadMostrada.HP_actual / scUnidadMostrada.mod_maxHP;

     Retrato.sprite = scUnidadMostrada.uRetrato;
     ActualizarSpriteIndicadorBando(scUnidadMostrada);

     //Estados
     foreach (Transform buttonEstado in contenedorCasillasEstados.transform)//Esto remueve los retratos anteriores antes de recalcular que retratos corresponden
     {
            Destroy(buttonEstado.gameObject);
     }

     if(scUnidadMostrada.estado_ardiendo > 0)
     {
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(1,scUnidadMostrada.estado_ardiendo);
     }
     if(scUnidadMostrada.estado_aturdido > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(2,scUnidadMostrada.estado_aturdido);
     }
     if(scUnidadMostrada.estado_acido > 0)
     {  GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(3,scUnidadMostrada.estado_acido);
     }
     if(scUnidadMostrada.estado_congelado > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(4,scUnidadMostrada.estado_congelado);
     }
     if(scUnidadMostrada.estado_ResistenciasReducidas > 0)
     {
       GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
       GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(5,scUnidadMostrada.estado_ResistenciasReducidas);
     }
     if(scUnidadMostrada.estado_armaduraModificador > 0)
     {
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(6,scUnidadMostrada.estado_armaduraModificador);
     }
     if(scUnidadMostrada.estado_sangrado > 0)
     {  GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(8,scUnidadMostrada.estado_sangrado);
     }
     if(scUnidadMostrada.estado_veneno > 0)
     {  GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(9,scUnidadMostrada.estado_veneno);
     }
     if(scUnidadMostrada.estado_APModificador > 0)
     {
        /*GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(7,scUnidadMostrada.estado_APModificador); */
     }
     if(scUnidadMostrada.estado_regeneravida > 0)
     {
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(10,scUnidadMostrada.estado_regeneravida);
     }
     if(scUnidadMostrada.estado_regeneraarmadura > 0)
     {
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(11,scUnidadMostrada.estado_regeneraarmadura);
     }
      if(scUnidadMostrada.estado_evasion > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(12,scUnidadMostrada.estado_evasion);
     }
    
     if (scUnidadMostrada is ClaseExplorador)
     {
            ClaseExplorador exp = (ClaseExplorador)scUnidadMostrada;
            GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
            GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(13, exp.ObtenerCantidadFlechas());
     }
     if(scUnidadMostrada.bonusdam_acido > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(14,scUnidadMostrada.bonusdam_acido);
     }
       if(scUnidadMostrada.bonusdam_arcano > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(15,scUnidadMostrada.bonusdam_arcano);
     }
       if(scUnidadMostrada.bonusdam_fuego > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(16,scUnidadMostrada.bonusdam_fuego);
     }
       if(scUnidadMostrada.bonusdam_hielo > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(17,scUnidadMostrada.bonusdam_hielo);
     }
       if(scUnidadMostrada.bonusdam_necro > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(18,scUnidadMostrada.bonusdam_necro);
     }
       if(scUnidadMostrada.bonusdam_rayo > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(19,scUnidadMostrada.bonusdam_rayo);
     }
      if(scUnidadMostrada is ClasePurificadora)
     { 
        ClasePurificadora exp = (ClasePurificadora)scUnidadMostrada;
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(20,exp.ObtenerFervor());
     }
     if (scUnidadMostrada.bonusdam_divino > 0)
     {
            GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
            GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(21, scUnidadMostrada.bonusdam_divino);
     }
      if(scUnidadMostrada.barreraDeDanio > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(22,(int)scUnidadMostrada.barreraDeDanio);
     }
      if(scUnidadMostrada.tejidoCuracMagica > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(23,(int)scUnidadMostrada.tejidoCuracMagica);
     }
     if(scUnidadMostrada.ObtenerEstaEscondido() == 1)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(24,-1);
     }
     if(scUnidadMostrada.ObtenerEstaEscondido() == 2)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(25,-1);
     }
      if(scUnidadMostrada is ClaseCanalizador)
     { 
        ClaseCanalizador exp = (ClaseCanalizador)scUnidadMostrada;
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(26,exp.ObtenerEnergia());
     }
      if(scUnidadMostrada.estado_Corrupto)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(27,1);
     }
     if(scUnidadMostrada.estado_Volando)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(28,1);
     }
     if(scUnidadMostrada.estado_Condenado > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(29,scUnidadMostrada.estado_Condenado);
     }
     if(scUnidadMostrada.estado_Escudado > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(30,scUnidadMostrada.estado_Escudado);
     }
     if(scUnidadMostrada.estado_MovimientoAbaratado > 0)
     {
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(31,scUnidadMostrada.estado_MovimientoAbaratado);
     }
         //AGREGAR LOS NUEVOS TMB EN UNIDADCANVAS PARA QUE APAREZCAN EN LA BARRA DE VIDA----!! 
         //Y en stacks poner -1 para que no muestre numero en la barra de vida.
         //Y que el parametro desdeBarraVida sea true.



        //MostrarBuffs/Debuffs
        List<BuffUIHelper.BuffStack> buffStacks = BuffUIHelper.GetVisibleBuffStacks(scUnidadMostrada);
        foreach (BuffUIHelper.BuffStack stack in buffStacks)
        {
           GameObject buffCuadro = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
           buffCuadro.GetComponent<UIEstadoCuadro>().RepresentarBuff(stack.AggregatedBuff, false, stack.StackCount);
        }

       //MostrarReacciones
      foreach (Reaccion buff in scUnidadMostrada.gameObject.GetComponents<Reaccion>())
      {
         GameObject buffCuadro =  Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
         buffCuadro.GetComponent<UIEstadoCuadro>().RepresentarReaccion(buff);

      }
       //MostrarMarcas
      foreach (Marca buff in scUnidadMostrada.gameObject.GetComponents<Marca>())
      {
         GameObject buffCuadro =  Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
         buffCuadro.GetComponent<UIEstadoCuadro>().RepresentarMarca(buff);

      }


    }
    else
    { 
        ActualizarTags(null, false);
        mostrardesc = false;
        ActualizarVisibilidadInfoEnemigos(false);
        ResetearVisualMerito();
        unidadUltimoMerito = null;
        gameObject.SetActive(false);
    }

  }

  private void ActualizarTags(Unidad unidad, bool esUnidadEnemiga)
  {
    if (vTags == null)
    {
      return;
    }

    bool mostrarTags = esUnidadEnemiga && (EsTurnoEnemigo() || hayUnidadSeleccionadaParaInfo || EsUnidadMostradaPorHover(unidad));
    vTags.gameObject.SetActive(mostrarTags);

    if (!mostrarTags || unidad == null || unidad.tags == null || unidad.tags.Count == 0)
    {
      vTags.text = string.Empty;
      return;
    }

    List<string> tagsTraducidos = new List<string>();
    foreach (string tag in unidad.tags)
    {
      if (string.IsNullOrWhiteSpace(tag))
      {
        continue;
      }

      tagsTraducidos.Add(TRADU.i != null ? TRADU.i.Traducir(tag) : tag);
    }

    vTags.text = string.Join(", ", tagsTraducidos);
  }

  private bool EsTurnoEnemigo()
  {
    if (BattleManager.Instance == null || BattleManager.Instance.unidadActiva == null)
    {
      return false;
    }

    Unidad unidadActiva = BattleManager.Instance.unidadActiva;
    return unidadActiva.CasillaPosicion != null && unidadActiva.CasillaPosicion.lado == 1;
  }

  private bool EsUnidadMostradaPorHover(Unidad unidad)
  {
    if (BattleManager.Instance == null || BattleManager.Instance.unidadActiva == null || unidad == null)
    {
      return false;
    }

    return unidadMostrada == unidad && unidad != BattleManager.Instance.unidadActiva;
  }

  private void InicializarVisualMerito()
  {
    if (vMeritoVisualInicializado || vMerito == null)
    {
      return;
    }

    vMeritoColorBase = vMerito.color;
    vMeritoEscalaBase = vMerito.rectTransform.localScale;
    vMeritoVisualInicializado = true;
  }

  private void ResetearVisualMerito()
  {
    InicializarVisualMerito();
    if (vMerito == null)
    {
      return;
    }

    if (vMeritoPulseCoroutine != null)
    {
      StopCoroutine(vMeritoPulseCoroutine);
      vMeritoPulseCoroutine = null;
    }

    vMerito.color = vMeritoColorBase;
    vMerito.rectTransform.localScale = vMeritoEscalaBase;
  }

  private void ActualizarVisualMerito(Unidad unidad)
  {
    if (unidad == null || vMerito == null)
    {
      return;
    }

    InicializarVisualMerito();

    int valorActual = Mathf.RoundToInt(unidad.ValentiaP_actual);
    vMerito.text = FormatearValorValentia(unidad, valorActual);

    if (unidadUltimoMerito != unidad)
    {
      unidadUltimoMerito = unidad;
      valorUltimoMerito = valorActual;
      ResetearVisualMerito();
      return;
    }

    int delta = valorActual - valorUltimoMerito;
    valorUltimoMerito = valorActual;
    if (delta == 0)
    {
      return;
    }

    if (vMeritoPulseCoroutine != null)
    {
      StopCoroutine(vMeritoPulseCoroutine);
    }
    vMeritoPulseCoroutine = StartCoroutine(PulsoCambioValentia(delta > 0));
  }

  private IEnumerator PulsoCambioValentia(bool esGanancia)
  {
    if (vMerito == null)
    {
      yield break;
    }

    InicializarVisualMerito();

    float duracionTotal = Mathf.Max(0.1f, vMeritoPulseDuracion);
    float mediaDuracion = duracionTotal * 0.5f;
    float factorEscala = Mathf.Max(1f, vMeritoPulseEscala);
    Vector3 escalaObjetivo = vMeritoEscalaBase * factorEscala;
    Color colorObjetivo = esGanancia ? vMeritoColorGanancia : vMeritoColorPerdida;

    float tiempo = 0f;
    while (tiempo < mediaDuracion)
    {
      tiempo += Time.deltaTime;
      float t = Mathf.Clamp01(tiempo / mediaDuracion);
      vMerito.color = Color.Lerp(vMeritoColorBase, colorObjetivo, t);
      vMerito.rectTransform.localScale = Vector3.Lerp(vMeritoEscalaBase, escalaObjetivo, t);
      yield return null;
    }

    tiempo = 0f;
    while (tiempo < mediaDuracion)
    {
      tiempo += Time.deltaTime;
      float t = Mathf.Clamp01(tiempo / mediaDuracion);
      vMerito.color = Color.Lerp(colorObjetivo, vMeritoColorBase, t);
      vMerito.rectTransform.localScale = Vector3.Lerp(escalaObjetivo, vMeritoEscalaBase, t);
      yield return null;
    }

    vMerito.color = vMeritoColorBase;
    vMerito.rectTransform.localScale = vMeritoEscalaBase;
    vMeritoPulseCoroutine = null;
  }

  private string FormatearValorValentia(Unidad unidad, int valorActual)
  {
    string estado = ObtenerEtiquetaEstadoValentia(unidad, valorActual);
    if (string.IsNullOrEmpty(estado))
    {
      return valorActual.ToString();
    }

    return valorActual + " (" + estado + ")";
  }

  private string ObtenerEtiquetaEstadoValentia(Unidad unidad, int valorActual)
  {
    if (unidad == null)
    {
      return string.Empty;
    }

    if (valorActual >= 5)
    {
      return TRADU.i != null ? TRADU.i.Traducir("Euforia") : "Euforia";
    }

    if (valorActual >= 3)
    {
      return TRADU.i != null ? TRADU.i.Traducir("Motivado") : "Motivado";
    }

    if (valorActual <= -5)
    {
      return TRADU.i != null ? TRADU.i.Traducir("Desesperanzado") : "Desesperanzado";
    }

    if (valorActual <= -3)
    {
      return TRADU.i != null ? TRADU.i.Traducir("Desmotivado") : "Desmotivado";
    }

    return string.Empty;
  }
  
  public GameObject BotonSalir;
  public void botonSalirDeseleccionar()
  {
    hayUnidadSeleccionadaParaInfo = false;
    mostrardesc = false;
    ActualizarVisibilidadInfoEnemigos(false);
    ActualizarInfoChar(BattleManager.Instance.unidadActiva);

  }
  public void ActualizarColoresFondo()
  { 
     if(unidadMostrada != null )
    {   
      if(unidadMostrada.CasillaPosicion != null)
      {
      
      if (unidadMostrada == BattleManager.Instance.unidadActiva)
      {
         // Amarillo muy suave
         ImagenFondo.color = new Color(0.95f, 0.95f, 0.75f, 0.4f);
      }
      else if (unidadMostrada.CasillaPosicion.lado == 1)
      {
         // Rojo muy suave
         ImagenFondo.color = new Color(0.95f, 0.75f, 0.75f, 0.4f);
      }
      else
      {
         // Azul muy suave
         ImagenFondo.color = new Color(0.75f, 0.75f, 0.95f, 0.4f);
      }
      }
    }

  }

  private void ActualizarSpriteIndicadorBando(Unidad unidad)
  {
    if (vIndicadorBando == null)
    {
      return;
    }

    bool esEnemigo = unidad != null && unidad.GetComponent<IAUnidad>() != null;
    Sprite spriteObjetivo = esEnemigo ? vSpriteStatEnemigo : vSpriteStatAliado;

    vIndicadorBando.sprite = spriteObjetivo;
    vIndicadorBando.enabled = spriteObjetivo != null;
  }

  private bool EsUnidadEnemiga(Unidad unidad)
  {
    return unidad != null && unidad.GetComponent<IAUnidad>() != null;
  }

  private void ActualizarVisibilidadInfoEnemigos(bool mostrarPanel)
  {
    if (infoEnemigos != null)
    {
      infoEnemigos.SetActive(mostrarPanel);
    }

    if (vDescenemigoGO != null)
    {
      vDescenemigoGO.SetActive(mostrarPanel);
    }
  }


  string ActualizarDescripcionAI()
  {
   string desc = "";
   if(unidadMostrada.uNombre == "Lobo Espectral")
   {
      desc = TRADU.i.Traducir("<i>El Lobo Espectral es un enemigo feroz que se mueve y ataca rápidamente, mientras su destreza animal le brinda una buena defensa.</i>\n\n<color=#199F10>-Posee un mordisco imbuido en fuego que además de dañar, puede hacer arder a sus enemigos.</color>\n<color=#EE0000>-Estadísticas débiles.</color>");
   }
   if(unidadMostrada.uNombre == "Lobo Alfa Espectral")
   {
      desc = TRADU.i.Traducir("<i>El Lobo Alfa Espectral es el líder de la manada, posee una complexión mas fuerte y resistente que los demás lobos aunque es un poco menos ágil.</i>\n\n<color=#199F10>-Tiene la capacidad de aullar para motivar a los demás lobos.</color>\n<color=#EE0000>-Si queda sólo no podrá motivar a nadie.</color>");
   }
   if(unidadMostrada.uNombre == "Driada Quemada")
   {
      desc = TRADU.i.Traducir("<i>Antes siervas y cuidadoras del bosque, ahora manifestaciones de venganza y odio en contra de cualquier invasor del Bosque Ardiente.</i>\n\n<color=#199F10>-Puede enredar con raíces ignífugas.\n-Ataque de rango.</color>\n<color=#EE0000>-Relativamente débil.</color>");
   }
   if(unidadMostrada.uNombre == "Espectro del Bosque")
   {
      desc = TRADU.i.Traducir("<i>El Espectro del Bosque es un alma en pena atrapada entre las cenizas de un bosque calcinado, su ira alimentada por la destrucción que no pudo evitar. Errante y vengativo, ataca a quienes osan cruzar su tierra calcinada.</i>\n\n<color=#199F10>-Inmune a ataques físicos.\n-Puede maldecir con Perdición.</color>\n<color=#EE0000>-Pierde parte de su inmunidad física momentáneamente al atacar.</color>");
   }
   if(unidadMostrada.uNombre == "Fuego Fatuo")
   {
      desc = TRADU.i.Traducir("<i>Un eco etéreo de las llamas que lo consumieron, danzando entre las cenizas como un recordatorio del desastre. Aunque parece inofensivo, guía a los incautos hacia la perdición, vengando la memoria del bosque caído.</i>\n\n<color=#199F10>-Resistente a ataques físicos.\n-Puede encarnarse en sus enemigos.</color>\n<color=#EE0000>-Tiene poca vida.</color>");
   }
   if(unidadMostrada.uNombre == "Treant Espectral")
   {
      desc = TRADU.i.Traducir("<i>Con su madera marcada y deformada por el fuego, estos antes pastores de árboles ahora deambulan trayendo muerte a los invasores de su hogar.</i>\n\n<color=#199F10>-Buena armadura que se regenera.\n-Puede enredar al golpear a sus enemigos.</color>\n<color=#EE0000>-Débil al fuego.</color>");
   }
   if(unidadMostrada.uNombre == "Manifestación Arcana")
   {
      desc = TRADU.i.Traducir("<i>Constituido por pura energía arcana, este ente etéreo defiende al Canalizador que le dio forma.</i>\n\n<color=#199F10>-Resistente a ataques físicos.</color>");
   }
   if(unidadMostrada.uNombre == "Vagranilo")
   {
      desc = TRADU.i.Traducir("<i>Un ser volador cuasihumano oriundo de las profundidades, no tiene vision pero compensa con una capacidad de audición excepcional.</i>\n\n<color=#199F10>-Evasivo.\n-Puede aturdir.\n-Puede atacar a enemigos escondidos.</color>\n<color=#EE0000>-Débil al daño Divino.</color>");
   }
   if(unidadMostrada.uNombre == "Vagranilo Mayor")
   {
      desc = TRADU.i.Traducir("<i>Un ser terrible cuasihumano oriundo de las profundidades, no tiene vision pero compensa con una capacidad de audición excepcional.</i>\n\n<color=#199F10>-Chirrido Ensordecedor.\n-Puede atacar a enemigos escondidos.\n-Se cura al morder victimas con Sangre Contaminada.</color>\n<color=#EE0000>-Débil al daño Divino.</color>");
   }
   if(unidadMostrada.uNombre == "Ladrón")
   {
      desc = TRADU.i.Traducir("<i>Este hombre ya era malvado antes, y ahora la situación desesperada ha acentuado su crueldad.</i>\n\n<color=#199F10>-Buena capacidad de Crítico.\n-Arranca escondido.\n-Puede envenenar su arma.</color>\n<color=#EE0000>-Bastante débil.</color>");
   }
   if(unidadMostrada.uNombre == "Rufián con Ballesta")
   {
      desc = TRADU.i.Traducir("<i>Este hombre ya era malvado antes, y ahora la situación desesperada ha acentuado su crueldad.</i>\n\n<color=#199F10>-Resistente.\n-Puede empujar.</color>");
   }
   if (unidadMostrada.uNombre == "Rufián con Mazo")
   {
      desc = TRADU.i.Traducir("<i>Este hombre ya era malvado antes, y ahora la situación desesperada ha acentuado su crueldad.</i>\n\n<color=#199F10>-Resistente.\n-Golpes devastadores.\n-Se enfurece.</color>\n<color=#EE0000>-Lento para actuar.</color>");
   }
   if (unidadMostrada.uNombre == "Perro Adiestrado")
   {
      desc = TRADU.i.Traducir("<i>Un perro adiestrado para la batalla, fiel a su amo y feroz con sus enemigos.</i>\n\n<color=#199F10>-Puede Inmovilizar al morder.</color>\n<color=#EE0000>-Relativamente débil.</color>");
   }
   if (unidadMostrada.uNombre == "Devorador Corrompido")
   {
      desc = TRADU.i.Traducir("<i>Otrora un habitante de las tierras, ahora corrompido por el Aliento Negro, deformado y hambriento.</i>\n\n<color=#A020F0>-Corrupto.</color>\n<color=#199F10>-Puede debilitar.\n-Absorbe vida de Personajes Corruptos.</color>\n<color=#EE0000>-Relativamente débil.</color>");
   }
   if (unidadMostrada.uNombre == "Guerrero Corrompido")
   {
      desc = TRADU.i.Traducir("<i>Otrora un habitante de las tierras, ahora corrompido por el Aliento Negro, deformado y hambriento.</i>\n\n<color=#A020F0>-Corrupto.</color>\n<color=#199F10>-Fuerte.\n-Golpea en zona.</color>\n<color=#EE0000>-Posee sólo un tipo de ataque.</color>");
   }
   if (unidadMostrada.uNombre == "Alimaña Corrompida")
   {
      desc = TRADU.i.Traducir("<i>No se logra discernir facilmente que animal fue originalmente, pero ahora es una criatura corrompida y muy nociva.</i>\n\n<color=#A020F0>-Corrupto.</color>\n<color=#199F10>-Largo alcance.\n-Crea Masa Contaminada.</color>\n<color=#EE0000>-Movimiento limitado.</color>");
   }
   if (unidadMostrada.uNombre == "Caníbal Kale'Tav")
   {
      desc = TRADU.i.Traducir("<i>Tribu oriundas del paso Vientohelado, estos seres salvajes son temidos por su ferocidad y rituales paganos.</i>\n\n<color=#199F10>-Empieza combate con Evasión.\n-Se potencia si el enemigo está lastimado.</color>\n<color=#EE0000>-Una vez que perdió la evasión, es fácil de eliminar.</color>");
   }
   if (unidadMostrada.uNombre == "Lancero Kale'Tav")
   {
      desc = TRADU.i.Traducir("<i>Tribu oriundas del paso Vientohelado, estos seres salvajes son temidos por su ferocidad y rituales paganos.</i>\n\n<color=#199F10>-Ataque de lanza arrojadiza peligroso.</color>\n<color=#EE0000>-Poca Precisión.</color>");
   }
   if (unidadMostrada.uNombre == "Guerrero Kale'Tav")
   {
      desc = TRADU.i.Traducir("<i>Tribu oriundas del paso Vientohelado, estos seres salvajes son temidos por su ferocidad y rituales paganos.</i>\n\n<color=#199F10>-Recibir Herida lo potencia.\n-Al matar a un enemigo se potencia.</color>\n<color=#EE0000>-Posee sólo un tipo de ataque.</color>");
   }
    if (unidadMostrada.uNombre == "Bruja Kale'Tav")
   {
      desc = TRADU.i.Traducir("<i>Tribu oriundas del paso Vientohelado, estos seres salvajes son temidos por su ferocidad y rituales paganos.</i>\n\n<color=#199F10>-Potencia Aliados.\n-Su cuervo la defiende.</color>\n<color=#EE0000>-Poco resistente.</color>");
   }
   if (unidadMostrada.uNombre == "Fagdaan")
   {
      desc = TRADU.i.Traducir("<i>Una criatura feroz nativa de la tundra. Es uno de los depredadores más temidos de la región y fuente de varias leyendas entre los Kale'Tav</i>\n\n<color=#199F10>-Regeneración leve.\n-Ataque de embestida en fila.</color>\n<color=#EE0000>-Suelen aparecer sólos o con una pareja como mucho.</color>");
   }
   if (unidadMostrada.uNombre == "Pájaro Rompe-Hielos")
   {
      desc = TRADU.i.Traducir("<i>Este pájaro es muy territorial y ataca en grupo, su pico está hecho para romper el hielo grueso y poder pescar peces de gran tamaño, por lo tanto es muy peligroso.</i>\n\n<color=#199F10>-Vuela.\n-Su ataque baja defensa</color>\n<color=#EE0000>-Una vez que pierde su vuelo, es vulnerable.</color>");
   }
   if (unidadMostrada.uNombre == "Efigie Animada")
   {
      desc = TRADU.i.Traducir("<i>Armadas por la magia oscura de los Kale'Tav, estas efigies están por todo su territorio como primer linea de defensa en contra de quienes se atrevan a cruzar el Paso.</i>\n\n<color=#199F10>-Al ser destruida condena a su atacante.\n-Provoca sangrado.</color>\n<color=#EE0000>-Débiles.</color>");
   }
   if (unidadMostrada.uNombre == "Gulek-Gul")
   {
      desc = TRADU.i.Traducir("<i>Gulek-Gul es un Ettin muy venerado por los Kale'Tav. No habita con ellos, pero cuando se encuentran intrusos en la zona, baja de su colina decidido a proteger su territorio.</i>\n\n<color=#199F10>-Fuerza descomunal.\n-Golpea en zona.\n-Doble intento en tiradas de voluntad.</color>\n<color=#EE0000>-Necesita levantar el martillo grande antes de usarlo.\n-Si recibe daño o falla tirada de voluntad, deja caer el martillo.</color>");
   }
   if (unidadMostrada.uNombre == "Soldado Vengador de Kadryn")
   {
      desc = TRADU.i.Traducir("<i>Organización de mercenarios humanos que eran parte del ejército derrotado del Liche Kadryn. Ahora buscan venganza tratando de que nadie escape al Aliento Negro de su amo.</i>\n\n<color=#199F10>-Unidad Escudada.\n-Buena Armadura.\n-Al morir deja una nube de aliento negro.</color>\n<color=#EE0000>-Movimiento limitado.</color>");
   }
   if (unidadMostrada.uNombre == "Alabardero Vengador de Kadryn")
   {
      desc = TRADU.i.Traducir("<i>Organización de mercenarios humanos que eran parte del ejército derrotado del Liche Kadryn. Ahora buscan venganza tratando de que nadie escape al Aliento Negro de su amo.</i>\n\n<color=#199F10>-Ataque de oportunidad.\n-Buena Armadura.\n-Al morir deja una nube de aliento negro.</color>\n<color=#EE0000>-Movimiento limitado.</color>");
   }
   if (unidadMostrada.uNombre == "Arquero Vengador de Kadryn")
   {
      desc = TRADU.i.Traducir("<i>Organización de mercenarios humanos que eran parte del ejército derrotado del Liche Kadryn. Ahora buscan venganza tratando de que nadie escape al Aliento Negro de su amo.</i>\n\n<color=#199F10>-Buen ataque.\n-Flecha envenenada.\n-Al morir deja una nube de aliento negro.</color>\n<color=#EE0000>-Poco resistente.</color>");
   }
   if (unidadMostrada.uNombre == "Predicador del Aliento Negro")
   {
      desc = TRADU.i.Traducir("<i>Organización de mercenarios humanos que eran parte del ejército derrotado del Liche Kadryn. Ahora buscan venganza tratando de que nadie escape al Aliento Negro de su amo.</i>\n\n<color=#199F10>-Ataque de rango infalible.\n-Potencia Aliados.\n-Al morir deja una nube de aliento negro.</color>\n<color=#EE0000>-Poco resistente.</color>");
   }
   if (unidadMostrada.uNombre == "Raiz-Viva Ardiendo")
   {
      desc = TRADU.i.Traducir("<i>Raiz-Viva del bosque mismo que ha salido a la superficie obligada por las llamas, ahora atacará furiosa a cualquier invasor del bosque.</i>\n\n<color=#199F10>-Ataque de llamas infalible.\n-Se entierra para curarse.</color>\n<color=#EE0000>-Inmóvil.</color>");
   }
   if (unidadMostrada.uNombre == "Oso Espectral")
   {
      desc = TRADU.i.Traducir("<i>Este oso se ha convertido en un feroz espectro que deambula el bosque ardiente. Su potencia física es aterradora.</i>\n\n<color=#199F10>-Ataques abrumadores.\n-Gran cantidad de vida.</color>\n<color=#EE0000>-Mayor probabilidad de pifia.</color>");
   }
   if (unidadMostrada.uNombre == "Faagdan")
   {
      desc = TRADU.i.Traducir("<i>Esta bestia oriunda del Paso es material de varias leyendas y pesadillas entre los Kale'Tav. De cuerpo robusto y cuernos afilados, supone un peligro para los viajeros incautos.</i>\n\n<color=#199F10>-Ataques de carga en fila.\n-Regeneración leve.</color>\n<color=#EE0000>-Lento.</color>");
   }
   if (unidadMostrada.uNombre == "Zarkil Acechador")
   {
      desc = TRADU.i.Traducir("<i>Raza de criaturas demoníacas que invaden Nedulkazan desde abajo en busca de sacrificios y oro. </i>\n\n<color=#199F10>-Al esquivar un ataque se moverán.\n-Puede ver escondidos.</color>\n<color=#EE0000>-Posee sólo un tipo de ataque.</color>");
   }
   if (unidadMostrada.uNombre == "Zarkil Guerrero")
   {
      desc = TRADU.i.Traducir("<i>Raza de criaturas demoníacas que invaden Nedulkazan desde abajo en busca de sacrificios y oro. </i>\n\n<color=#199F10>-Puede aterrar a criaturas enfrente.\n-Puede ver escondidos.</color>\n<color=#EE0000>-Posee sólo un tipo de ataque.</color>");
   }
   if (unidadMostrada.uNombre == "Zarkil Vociferador")
   {
      desc = TRADU.i.Traducir("<i>Raza de criaturas demoníacas que invaden Nedulkazan desde abajo en busca de sacrificios y oro. </i>\n\n<color=#199F10>-Grito aturdidor que además motiva aliados.\n-Puede ver escondidos.\n-Puede atacar repetidamente.</color>\n<color=#EE0000></color>");
   }
   if (unidadMostrada.uNombre == "Zarkil Alado")
   {
      desc = TRADU.i.Traducir("<i>Raza de criaturas demoníacas que invaden Nedulkazan desde abajo en busca de sacrificios y oro. </i>\n\n<color=#199F10>-Ataque debilitador infalible.\n-Puede ver escondidos.\n-Volador.</color>\n<color=#EE0000>-Débil</color>");
   }
   if (unidadMostrada.uNombre == "Zarkilever")
   {
      desc = TRADU.i.Traducir("<i>Criatura muy feroz controlada porlos Zarkils utilizada como fuerza de impacto y para causar grietas en superficies duras. </color>\n\n<color=#199F10>-Buena Armadura.\n-Saborea a las víctimas.</color>\n<color=#EE0000></color>");
   }
    if (unidadMostrada.uNombre == "Comandante Zarkil")
   {
      desc = TRADU.i.Traducir("<i>Tiene una legión entera de Zarkils bajo su liderazgo, simplemente debe señalar un objetivo y sus súbditos se encargarán del resto.</color>\n\n<color=#199F10>-Llama refuerzos sin fin.\n-Ataque debilitador infalible.</color>\n<color=#EE0000>-No es fuerte por si solo.</color>");
   }
    if (unidadMostrada.uNombre == "Arbol Vengativo")
   {
      desc = TRADU.i.Traducir("<i>Manifestación de la energía espectral del bosque. Desde su interior emana un fulgor fantasmal frío, como un espíritu atrapado que se retuerce para escapar.</color>\n\n<color=#199F10>-Llama refuerzos sin fin.\n-Ataque necrótico que condena a dos objetivos.</color>\n<color=#EE0000>-Inmóvil.</color>");
   }
   if (unidadMostrada.uNombre == "Draco de Hielo")
   {
      desc = TRADU.i.Traducir("<i>Estas criaturas aladas habitan en las regiones más frías del Paso. Son conocidas por ser muy territoriales y por su aliento gélido.</i>\n\n<color=#199F10>-Vuelo.\n-Aliento gélido en zona.\n-Regenera armadura.</color>\n<color=#EE0000>-Débil al fuego.</color>");
   }
   
  
   return desc;
  }
  
  public bool mostrardesc;
  public void BotonInfoenemigos()
  {
    if (!EsUnidadEnemiga(unidadMostrada))
    {
      mostrardesc = false;
      ActualizarVisibilidadInfoEnemigos(false);
      return;
    }

    mostrardesc = !mostrardesc;
    ActualizarVisibilidadInfoEnemigos(mostrardesc);
  }
}



