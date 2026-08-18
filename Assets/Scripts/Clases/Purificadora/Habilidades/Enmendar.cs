using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System;

public class Enmendar : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    
    
    public override void  Awake()
    {
      nombre = "Enmendar";
      IDenClase = 3;
      costoAP = 3;
      costoPM = 1;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 2;
      bAfectaObstaculos = false;
      
     
      
      imHab = Resources.Load<Sprite>("imHab/Purificadora_Enmendar");
      ActualizarDescripcion();

      requiereRecurso = 1; //esto es para que el boton no se active al apretar si no tiene X recursos (ej Flecha). Ver en BotonHabilidad.
      if(NIVEL == 4){requiereRecurso = 0;}
    }
    public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
      var statsUI = ObtenerStatsDescripcionUI();

      int poderActual = statsUI.Poder;
      int alcance = 4;
      int bonusPlano = NIVEL > 2 ? 3 : (NIVEL > 1 ? 1 : 0);
      bool consumeFervor = NIVEL != 4;
      int fervorActual = 0;
      ClasePurificadora scPurificadora = Usuario != null ? Usuario.GetComponent<ClasePurificadora>() : null;
      if (scPurificadora != null)
      {
        fervorActual = scPurificadora.ObtenerFervor();
      }

      string tituloEs = "Enmendar I";
      string tituloEn = "Mend I";
      string tituloPt = "Remendar I";
      if (NIVEL == 2) { tituloEs = "Enmendar II"; tituloEn = "Mend II"; }
      if (NIVEL == 3) { tituloEs = "Enmendar III"; tituloEn = "Mend III"; }
      if (NIVEL == 4) { tituloEs = "Enmendar IV a"; tituloEn = "Mend IV a"; }
      if (NIVEL == 5) { tituloEs = "Enmendar IV b"; tituloEn = "Mend IV b"; }
      if (NIVEL == 2) { tituloPt = "Remendar II"; }
      if (NIVEL == 3) { tituloPt = "Remendar III"; }
      if (NIVEL == 4) { tituloPt = "Remendar IV a"; }
      if (NIVEL == 5) { tituloPt = "Remendar IV b"; }

      string bonusPlanoTexto = bonusPlano > 0 ? $" + {bonusPlano}" : "";
      if (esIngles)
      {
        string poder = TerminoDescripcion(TerminoDescripcionId.Poder, $"Power ({poderActual})");
        string fervor = TerminoDescripcion(TerminoDescripcionId.Fervor, $"your Fervor ({fervorActual})");
        string proximaMejora = null;
        if (DebeMostrarProximaMejoraDescripcion())
        {
          if (NIVEL < 2) { proximaMejora = "Next Level: +1 flat healing."; }
          else if (NIVEL == 2) { proximaMejora = "Next Level: +2 flat healing."; }
          else if (NIVEL == 3) { proximaMejora = "Option A: no Fervor requirement or consumption.\nOption B: retains Fervor requirement and consumption."; }
        }

        txtDescripcion = ConstruirDescripcionNormalizadaIngles(
          tituloEn,
          "Heals one ally, scaling with Power and current Fervor.",
          new[]
          {
            LineaDescripcion("Target", "1 ally"),
            LineaDescripcion("Effect", $"Restores 4-18{bonusPlanoTexto} + {poder} + {fervor} HP as magical healing."),
            LineaDescripcion("Cost", consumeFervor ? "1 Fervor" : "None")
          },
          proximaMejora);
        return;
      }

      if (esPortugues)
      {
        string poder = TerminoDescripcion(TerminoDescripcionId.Poder, $"Poder ({poderActual})");
        string fervor = TerminoDescripcion(TerminoDescripcionId.Fervor, $"Fervor atual ({fervorActual})");
        string proximaMejora = null;
        if (DebeMostrarProximaMejoraDescripcion())
        {
          if (NIVEL < 2) { proximaMejora = "Próximo nível: +1 de cura fixa."; }
          else if (NIVEL == 2) { proximaMejora = "Próximo nível: +2 de cura fixa."; }
          else if (NIVEL == 3) { proximaMejora = "Próximo nível: Opção A: sem requisito nem consumo de Fervor.\nOpção B: mantém o requisito e o consumo de Fervor."; }
        }

        txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(
          tituloPt,
          "Cura um aliado, escalando com Poder e o Fervor atual.",
          new[]
          {
            LineaDescripcion("Alvo", "1 aliado"),
            LineaDescripcion("Efeito", $"Restaura 4-18{bonusPlanoTexto} + {poder} + {fervor} HP como cura mágica."),
            LineaDescripcion("Custo", consumeFervor ? "1 Fervor" : "Nenhum")
          },
          proximaMejora);
        return;
      }

      {
        string poder = TerminoDescripcion(TerminoDescripcionId.Poder, $"Poder ({poderActual})");
        string fervor = TerminoDescripcion(TerminoDescripcionId.Fervor, $"Fervor actual ({fervorActual})");
        string proximaMejora = null;
        if (DebeMostrarProximaMejoraDescripcion())
        {
          if (NIVEL < 2) { proximaMejora = "Próximo nivel: +1 de curación fija."; }
          else if (NIVEL == 2) { proximaMejora = "Próximo nivel: +2 de curación fija."; }
          else if (NIVEL == 3) { proximaMejora = "Próximo nivel: Opción A: sin requisito ni consumo de Fervor.\nOpción B: mantiene el requisito y el consumo de Fervor."; }
        }

        txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(
          tituloEs,
          "Cura a un aliado, escalando con Poder y el Fervor actual.",
          new[]
          {
            LineaDescripcion("Objetivo", "1 aliado"),
            LineaDescripcion("Efecto", $"Restaura 4-18{bonusPlanoTexto} + {poder} + {fervor} HP como curación mágica."),
            LineaDescripcion("Costo", consumeFervor ? "1 Fervor" : "Ninguno")
          },
          proximaMejora);
        return;
      }

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += $"<b>Type:</b> Ranged ({alcance} range)\n";
        cuerpo += "<b>Target:</b> 1 unit in range\n";
        cuerpo += $"<b>Heal:</b> Random 4-18{bonusPlanoTexto} + <color=#ea0606>Power ({poderActual})</color> + Fervor ({fervorActual})\n";
        cuerpo += "<b>Healing Type:</b> Magical healing\n";
        cuerpo += "<b>Requirement:</b> Needs at least 1 Fervor to activate\n";
        cuerpo += consumeFervor
          ? "<b>On cast:</b> Consumes 1 Fervor"
          : "<b>On cast:</b> Does not consume Fervor";
      }
      else if (esPortugues)
      {
        cuerpo += $"<b>Tipo:</b> Alcance ({alcance} de alcance)\n";
        cuerpo += "<b>Alvo:</b> 1 unidade no alcance\n";
        cuerpo += $"<b>Cura:</b> Aleatorio 4-18{bonusPlanoTexto} + <color=#ea0606>Poder ({poderActual})</color> + Fervor ({fervorActual})\n";
        cuerpo += "<b>Tipo de cura:</b> Cura magica\n";
        cuerpo += "<b>Requisito:</b> Precisa de pelo menos 1 Fervor para ativar\n";
        cuerpo += consumeFervor
          ? "<b>Ao usar:</b> Consome 1 Fervor"
          : "<b>Ao usar:</b> Nao consome Fervor";
      }
      else
      {
        cuerpo += $"<b>Tipo:</b> Rango ({alcance} alcance)\n";
        cuerpo += "<b>Objetivo:</b> 1 unidad en rango\n";
        cuerpo += $"<b>Curación:</b> Aleatorio 4-18{bonusPlanoTexto} + <color=#ea0606>Pod ({poderActual})</color> + Fervor ({fervorActual})\n";
        cuerpo += "<b>Tipo de curación:</b> Curación mágica\n";
        cuerpo += "<b>Requisito:</b> Necesita al menos 1 Fervor para activarse\n";
        cuerpo += consumeFervor
          ? "<b>Al lanzar:</b> Consume 1 Fervor"
          : "<b>Al lanzar:</b> No consume Fervor";
      }

      string costos = esIngles
        ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}"
        : esPortugues
          ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}";

      txtDescripcion = ConstruirDescripcionEstandar(
        esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
        esIngles
          ? "A restorative spell that scales with current Fervor."
          : esPortugues
            ? "Uma magia restauradora que escala com o Fervor atual."
          : "Un hechizo restaurador que escala con el Fervor actual.",
        cuerpo,
        costos,
        "#5dade2");

      string colorEncabezado = "#44d3ec";
      string colorValor = "#ffffff";
      string colorPoder = "#2aa6c8";
      string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
      string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
      string costoSuperior = cooldownMax > 0
        ? $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}"
        : $"{costoAP} {iconoAP}";
      string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
      string subtitulo = esIngles
        ? "Heals one ally, scaling with Power and current Fervor."
        : esPortugues
          ? "Cura um aliado, escalando com Poder e Fervor atual."
          : "Cura a un aliado, escalando con Poder y Fervor actual.";
      string curacion = $"4-18{bonusPlanoTexto} + <color={colorPoder}>{(esIngles ? "Power" : esPortugues ? "Poder" : "Poder")} ({poderActual})</color> + Fervor ({fervorActual})";
      string cuerpoNuevo = "";
      if (esIngles)
      {
        cuerpoNuevo += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Ranged heal ({alcance} range)</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Target:</b></color> <color={colorValor}>1 unit in range</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Heal:</b></color> <color={colorValor}>{curacion}. Type: Magical healing</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Requirement:</b></color> <color={colorValor}>{(consumeFervor ? "Requires 1+ Fervor." : "No Fervor required.")}</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>On cast:</b></color> <color={colorValor}>{(consumeFervor ? "Consumes 1 Fervor." : "Does not consume Fervor.")}</color>";
      }
      else if (esPortugues)
      {
        cuerpoNuevo += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Cura a alcance ({alcance} de alcance)</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Alvo:</b></color> <color={colorValor}>1 unidade no alcance</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Cura:</b></color> <color={colorValor}>{curacion}. Tipo: Cura magica</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Requisito:</b></color> <color={colorValor}>{(consumeFervor ? "Requer 1+ Fervor." : "Nao requer Fervor.")}</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Ao usar:</b></color> <color={colorValor}>{(consumeFervor ? "Consome 1 Fervor." : "Nao consome Fervor.")}</color>";
      }
      else
      {
        cuerpoNuevo += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Curación a rango ({alcance} alcance)</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Objetivo:</b></color> <color={colorValor}>1 unidad en rango</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Curación:</b></color> <color={colorValor}>{curacion}. Tipo: Curación mágica</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Requisito:</b></color> <color={colorValor}>{(consumeFervor ? "Requiere 1+ Fervor." : "No requiere Fervor.")}</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Al lanzar:</b></color> <color={colorValor}>{(consumeFervor ? "Consume 1 Fervor." : "No consume Fervor.")}</color>";
      }

      txtDescripcion =
        $"<size=115%><color=#5dade2><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n" +
        $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n" +
        "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n" +
        cuerpoNuevo;

      bool mostrarProximoNivel = EsEscenaCampaña() && CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
      if (!mostrarProximoNivel)
      {
        return;
      }

      if (esIngles)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 flat healing.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +2 flat healing.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (no Fervor consumption) or Option B (keeps Fervor consumption).</color>"; }
      }
      else if (esPortugues)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 cura plana.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +2 cura plana.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (sem consumo de Fervor) ou Opcao B (mantem consumo de Fervor).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 curación plana.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +2 curación plana.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A (sin consumo de Fervor) u Opción B (mantiene consumo de Fervor).</color>"; }
      }
    }
    void Start()
    {
       

    }

    Casilla Origen;
    public override void Activar()
    {
       if(Usuario.GetComponent<ClasePurificadora>().ObtenerFervor() > 0)
       {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

    BattleManager.Instance.scUIContadorAP.MarcarCirculos((int)scEstaUnidad.ObtenerAPActual());       }
        
    }
    
    

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {
    
     if(obj is Unidad) //Acá van los efectos a Unidades.
     {
       
      
       Unidad objetivo = (Unidad)obj;
       VFXAplicar(objetivo.gameObject);
      
       BattleManager.Instance.EscribirLog(TRADU.i.Traducir(scEstaUnidad.uNombre) + " " + TRADU.i.Traducir("usa ") + TRADU.i.Traducir(nombre) + " -> " + TRADU.i.Traducir(objetivo.uNombre) + ".");
  
       int random = UnityEngine.Random.Range(4, 19);
       float curacion = random+scEstaUnidad.mod_CarPoder+Usuario.GetComponent<ClasePurificadora>().ObtenerFervor();
       if(NIVEL > 1){curacion++;}
       if(NIVEL > 2){curacion+= 2;}
       if(NIVEL > 5){curacion+= Usuario.GetComponent<ClasePurificadora>().ObtenerFervor()*2;}
       
     
       objetivo.RecibirCuracion(curacion, true);

       if(NIVEL != 4){  Usuario.GetComponent<ClasePurificadora>().CambiarFervor(-1);}
     


       objetivo.Marcar(0);

      
      
     }   
   
    }
    bool ChequearSiHayAliadoAdelantado(Unidad obj)
    {
      int casX = Origen.posX;

      foreach(Casilla cas in BattleManager.Instance.lCasillasTotal)
      {
        if(cas.lado != Origen.lado){ continue;} //Si es del lado opuesto la descarta
        if(cas.posX <= Origen.posX){ continue;} //Si esta en la misma culomna o una mas atras la descarta

        if(cas.Presente != null)
        {
            if(cas.Presente.GetComponent<Unidad>() != null)
            {
               if(cas.Presente.GetComponent<Unidad>() != obj) //Si hay una unidad, y no es el objetivo de la habilidad, entonces devuelve SI
               {
                    return true;
               }

            }

        }
        

      }

      return false;
    }
     void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_Enmendar");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);  
   PurificadoraReceptorSutilFx.CrearEnmendar(objetivo.GetComponent<Unidad>());

    }

    //Provisorio
    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();

    private void ObtenerObjetivos()
    {
      
      lObjetivosPosibles.Clear();
     
      
      //Casillas Alrededor al origen
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasAlrededor(4);
      lCasillasafectadas.Add(Origen); //Agrega la casilla del caster, para que se pueda targetear
    
      foreach(Casilla c in lCasillasafectadas)
      {
         c.ActivarCapaColorAzul();
        if(c.Presente == null)
        {
            continue;
        }
        
        if(!bAfectaObstaculos) //Si no afecta obstaculos se descarta la casilla si no hay unidad, si afecta obstaculo se descarta si tampoco hay obstaculo
        {
           if(c.Presente.GetComponent<Unidad>() == null)
           {
            continue;
           }
             if(c.Presente.GetComponent<Unidad>() != null)
           {
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());
           }
          

        }
        else
        {
           if(c.Presente.GetComponent<Unidad>() == null && c.Presente.GetComponent<Obstaculo>() == null)
           {
            continue;
           }

           if(c.Presente.GetComponent<Unidad>() != null)
           {
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());;
           }

           if(c.Presente.GetComponent<Obstaculo>() != null)
           {
             lObstaculosPosibles.Add(c.Presente.GetComponent<Obstaculo>());;
           }

        }

      }
    

      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva = new List<Unidad>(lObjetivosPosibles);

      foreach(Unidad uni in BattleManager.Instance.lUnidadesPosiblesHabilidadActiva)
      {
      }

      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
       
    
        



      
         
    }

   
    

 
}


public class PurificadoraReceptorSutilFx : MonoBehaviour
{
  private enum TipoFx
  {
    Enmendar,
    Purificacion
  }

  private const int CantidadParticulas = 7;

  private RectTransform root;
  private CanvasGroup canvasGroup;
  private RectTransform imagenUnidad;
  private Image aura;
  private Image nucleo;
  private Image anillo;
  private Image barraVertical;
  private Image barraHorizontal;
  private readonly Image[] particulas = new Image[CantidadParticulas];
  private readonly float[] fases = new float[CantidadParticulas];
  private readonly float[] offsetsX = new float[CantidadParticulas];
  private readonly float[] velocidades = new float[CantidadParticulas];
  private readonly float[] tamanos = new float[CantidadParticulas];
  private float tiempo;
  private float duracion;
  private Vector2 tamanoBase;
  private Vector2 posicionBase;
  private TipoFx tipo;

  private static Sprite spriteSuave;
  private static Sprite spriteAnillo;
  private static Sprite spriteRayo;
  private static Sprite spriteGota;
  private static Texture2D texturaSuave;
  private static Texture2D texturaAnillo;
  private static Texture2D texturaRayo;
  private static Texture2D texturaGota;

  public static void CrearEnmendar(Unidad unidad)
  {
    Crear(unidad, TipoFx.Enmendar);
  }

  public static void CrearPurificacion(Unidad unidad)
  {
    Crear(unidad, TipoFx.Purificacion);
  }

  private static void Crear(Unidad unidad, TipoFx tipoFx)
  {
    if (unidad == null || unidad.uImage == null)
    {
      return;
    }

    RectTransform imagen = unidad.uImage.rectTransform;
    if (imagen == null || !(imagen.parent is RectTransform padre))
    {
      return;
    }

    GameObject go = new GameObject("PurificadoraReceptorSutilFx", typeof(RectTransform), typeof(CanvasGroup), typeof(PurificadoraReceptorSutilFx));
    PurificadoraReceptorSutilFx fx = go.GetComponent<PurificadoraReceptorSutilFx>();
    fx.Inicializar(padre, imagen, tipoFx);

    Canvas canvas = unidad.uImage.GetComponentInParent<Canvas>(true);
    RenderOrderHelper.OrdenarCanvasEncima(canvas, unidad.transform, tipoFx == TipoFx.Purificacion ? 8 : 7);
  }

  private void Inicializar(RectTransform padre, RectTransform imagen, TipoFx tipoFx)
  {
    root = GetComponent<RectTransform>();
    canvasGroup = GetComponent<CanvasGroup>();
    imagenUnidad = imagen;
    tipo = tipoFx;
    duracion = tipo == TipoFx.Purificacion ? 1.15f : 0.95f;
    root.SetParent(padre, false);
    canvasGroup.interactable = false;
    canvasGroup.blocksRaycasts = false;

    tamanoBase = imagenUnidad.rect.size;
    if (tamanoBase.x <= 0.01f || tamanoBase.y <= 0.01f)
    {
      tamanoBase = imagenUnidad.sizeDelta;
    }
    if (tamanoBase.x <= 0.01f || tamanoBase.y <= 0.01f)
    {
      tamanoBase = new Vector2(36f, 42f);
    }

    posicionBase = new Vector2(0f, tamanoBase.y * 0.08f);
    root.anchorMin = new Vector2(0.5f, 0.5f);
    root.anchorMax = new Vector2(0.5f, 0.5f);
    root.pivot = new Vector2(0.5f, 0.5f);
    root.anchoredPosition = imagenUnidad.anchoredPosition + posicionBase;
    root.localScale = imagenUnidad.localScale;
    root.sizeDelta = tipo == TipoFx.Purificacion
      ? new Vector2(tamanoBase.x * 0.9f, tamanoBase.y * 1.35f)
      : new Vector2(tamanoBase.x * 0.9f, tamanoBase.y * 1.05f);

    int targetSibling = Mathf.Min(padre.childCount - 1, imagenUnidad.GetSiblingIndex() + 1);
    root.SetSiblingIndex(targetSibling);

    aura = CrearImagen("Aura", ObtenerSpriteSuave(), root);
    nucleo = CrearImagen("Nucleo", ObtenerSpriteSuave(), root);
    anillo = CrearImagen("Anillo", ObtenerSpriteAnillo(), root);
    barraVertical = CrearImagen("BarraVertical", ObtenerSpriteRayo(), root);
    barraHorizontal = CrearImagen("BarraHorizontal", ObtenerSpriteRayo(), root);

    for (int i = 0; i < particulas.Length; i++)
    {
      particulas[i] = CrearImagen("Particula" + i, tipo == TipoFx.Purificacion ? ObtenerSpriteGota() : ObtenerSpriteSuave(), root);
      fases[i] = UnityEngine.Random.Range(0f, 1f);
      offsetsX[i] = UnityEngine.Random.Range(-0.42f, 0.42f);
      velocidades[i] = UnityEngine.Random.Range(0.75f, 1.35f);
      tamanos[i] = UnityEngine.Random.Range(0.75f, 1.15f);
    }

    ActualizarVisual(0f);
  }

  private Image CrearImagen(string nombre, Sprite sprite, RectTransform padre)
  {
    GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
    RectTransform rect = go.GetComponent<RectTransform>();
    rect.SetParent(padre, false);
    rect.anchorMin = new Vector2(0.5f, 0.5f);
    rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.pivot = new Vector2(0.5f, 0.5f);

    Image image = go.GetComponent<Image>();
    image.sprite = sprite;
    image.raycastTarget = false;
    image.maskable = false;
    image.preserveAspect = false;
    return image;
  }

  private void Update()
  {
    tiempo += Time.deltaTime;
    float t = Mathf.Clamp01(tiempo / duracion);
    ActualizarVisual(t);

    if (tiempo >= duracion)
    {
      Destroy(gameObject);
    }
  }

  private void ActualizarVisual(float t)
  {
    if (root == null || canvasGroup == null)
    {
      return;
    }

    float entrada = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.16f));
    float salida = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.62f) / 0.38f));
    float intensidad = entrada * salida;
    canvasGroup.alpha = intensidad;
    root.anchoredPosition = imagenUnidad != null ? imagenUnidad.anchoredPosition + posicionBase : root.anchoredPosition;

    if (tipo == TipoFx.Purificacion)
    {
      ActualizarPurificacion(t, intensidad);
    }
    else
    {
      ActualizarEnmendar(t, intensidad);
    }
  }

  private void ActualizarEnmendar(float t, float intensidad)
  {
    float pulso = 0.94f + (0.06f * Mathf.Sin(Time.time * 8f));
    float escala = Mathf.Lerp(0.72f, 1.08f, Mathf.SmoothStep(0f, 1f, t));

    Configurar(aura, Vector2.zero, tamanoBase * (0.86f * escala), 0f, new Color(0.58f, 1f, 0.68f, 0.18f * intensidad));
    Configurar(nucleo, new Vector2(0f, tamanoBase.y * 0.04f), tamanoBase * (0.34f * pulso), 0f, new Color(0.94f, 1f, 0.78f, 0.36f * intensidad));
    Configurar(anillo, Vector2.zero, tamanoBase * (0.48f + (0.22f * t)), Mathf.Sin(Time.time * 5f) * 4f, new Color(0.82f, 1f, 0.74f, 0.26f * intensidad));
    Configurar(barraVertical, Vector2.zero, new Vector2(tamanoBase.x * 0.06f, tamanoBase.y * 0.44f), 90f, new Color(0.9f, 1f, 0.82f, 0.3f * intensidad));
    Configurar(barraHorizontal, Vector2.zero, new Vector2(tamanoBase.x * 0.42f, tamanoBase.y * 0.055f), 0f, new Color(0.9f, 1f, 0.82f, 0.3f * intensidad));

    for (int i = 0; i < particulas.Length; i++)
    {
      float avance = Mathf.Repeat((t * velocidades[i]) + fases[i], 1f);
      float alpha = Mathf.Sin(avance * Mathf.PI) * intensidad;
      Vector2 posicion = new Vector2(
        (tamanoBase.x * offsetsX[i] * 0.55f) + (Mathf.Sin(Time.time * 4.5f + i) * tamanoBase.x * 0.025f),
        Mathf.Lerp(-tamanoBase.y * 0.28f, tamanoBase.y * 0.42f, avance));
      Vector2 tamano = tamanoBase * (0.075f * tamanos[i] * Mathf.Lerp(1f, 0.65f, avance));
      Configurar(particulas[i], posicion, tamano, 0f, new Color(0.82f, 1f, 0.74f, 0.34f * alpha));
    }
  }

  private void ActualizarPurificacion(float t, float intensidad)
  {
    float velo = 0.88f + (0.05f * Mathf.Sin(Time.time * 10f));

    Configurar(aura, new Vector2(0f, tamanoBase.y * 0.02f), new Vector2(tamanoBase.x * 0.58f * velo, tamanoBase.y * 1.18f), 0f, new Color(0.4f, 0.86f, 1f, 0.12f * intensidad));
    Configurar(nucleo, new Vector2(0f, -tamanoBase.y * 0.05f), new Vector2(tamanoBase.x * 0.24f, tamanoBase.y * 1.04f), 0f, new Color(0.82f, 0.98f, 1f, 0.18f * intensidad));
    Configurar(anillo, new Vector2(0f, -tamanoBase.y * 0.34f), new Vector2(tamanoBase.x * 0.58f, tamanoBase.y * 0.13f), Mathf.Sin(Time.time * 6f) * 6f, new Color(0.62f, 0.95f, 1f, 0.24f * intensidad));
    Configurar(barraVertical, Vector2.zero, Vector2.zero, 0f, Color.clear);
    Configurar(barraHorizontal, Vector2.zero, Vector2.zero, 0f, Color.clear);

    for (int i = 0; i < particulas.Length; i++)
    {
      float avance = Mathf.Repeat((t * velocidades[i]) + fases[i], 1f);
      float alpha = Mathf.Sin(avance * Mathf.PI) * intensidad;
      Vector2 posicion = new Vector2(
        (tamanoBase.x * offsetsX[i]) + (Mathf.Sin(Time.time * (5f + i)) * tamanoBase.x * 0.035f),
        Mathf.Lerp(tamanoBase.y * 0.58f, -tamanoBase.y * 0.5f, avance));
      Vector2 tamano = new Vector2(tamanoBase.x * 0.085f * tamanos[i], tamanoBase.y * 0.16f * tamanos[i]);
      Configurar(particulas[i], posicion, tamano, Mathf.Sin(Time.time * 7f + i) * 8f, new Color(0.68f, 0.94f, 1f, 0.36f * alpha));
    }
  }

  private static void Configurar(Image image, Vector2 posicion, Vector2 tamano, float rotacionZ, Color color)
  {
    if (image == null)
    {
      return;
    }

    RectTransform rect = image.rectTransform;
    rect.anchoredPosition = posicion;
    rect.sizeDelta = tamano;
    rect.localEulerAngles = new Vector3(0f, 0f, rotacionZ);
    rect.localScale = Vector3.one;
    image.color = color;
  }

  private static Sprite ObtenerSpriteSuave()
  {
    if (spriteSuave != null)
    {
      return spriteSuave;
    }

    const int size = 64;
    texturaSuave = new Texture2D(size, size, TextureFormat.ARGB32, false);
    texturaSuave.name = "PurificadoraSoftRuntime";
    texturaSuave.wrapMode = TextureWrapMode.Clamp;
    texturaSuave.filterMode = FilterMode.Bilinear;
    texturaSuave.hideFlags = HideFlags.HideAndDontSave;

    Color[] pixels = new Color[size * size];
    float centro = (size - 1) * 0.5f;
    float radio = size * 0.5f;
    for (int y = 0; y < size; y++)
    {
      for (int x = 0; x < size; x++)
      {
        float dx = (x - centro) / radio;
        float dy = (y - centro) / radio;
        float distancia = Mathf.Sqrt((dx * dx) + (dy * dy));
        float alpha = Mathf.Pow(Mathf.Clamp01(1f - distancia), 2.2f);
        pixels[(y * size) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaSuave.SetPixels(pixels);
    texturaSuave.Apply(false, true);
    spriteSuave = Sprite.Create(texturaSuave, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    spriteSuave.name = "PurificadoraSoftRuntime";
    return spriteSuave;
  }

  private static Sprite ObtenerSpriteAnillo()
  {
    if (spriteAnillo != null)
    {
      return spriteAnillo;
    }

    const int size = 64;
    texturaAnillo = new Texture2D(size, size, TextureFormat.ARGB32, false);
    texturaAnillo.name = "PurificadoraRingRuntime";
    texturaAnillo.wrapMode = TextureWrapMode.Clamp;
    texturaAnillo.filterMode = FilterMode.Bilinear;
    texturaAnillo.hideFlags = HideFlags.HideAndDontSave;

    Color[] pixels = new Color[size * size];
    float centro = (size - 1) * 0.5f;
    float radio = size * 0.5f;
    for (int y = 0; y < size; y++)
    {
      for (int x = 0; x < size; x++)
      {
        float dx = (x - centro) / radio;
        float dy = (y - centro) / radio;
        float distancia = Mathf.Sqrt((dx * dx) + (dy * dy));
        float borde = Mathf.Abs(distancia - 0.58f);
        float alpha = Mathf.Pow(Mathf.Clamp01(1f - (borde / 0.16f)), 1.7f) * Mathf.Clamp01(1f - distancia);
        pixels[(y * size) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaAnillo.SetPixels(pixels);
    texturaAnillo.Apply(false, true);
    spriteAnillo = Sprite.Create(texturaAnillo, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    spriteAnillo.name = "PurificadoraRingRuntime";
    return spriteAnillo;
  }

  private static Sprite ObtenerSpriteRayo()
  {
    if (spriteRayo != null)
    {
      return spriteRayo;
    }

    const int width = 64;
    const int height = 12;
    texturaRayo = new Texture2D(width, height, TextureFormat.ARGB32, false);
    texturaRayo.name = "PurificadoraRayRuntime";
    texturaRayo.wrapMode = TextureWrapMode.Clamp;
    texturaRayo.filterMode = FilterMode.Bilinear;
    texturaRayo.hideFlags = HideFlags.HideAndDontSave;

    Color[] pixels = new Color[width * height];
    float centroY = (height - 1) * 0.5f;
    for (int y = 0; y < height; y++)
    {
      for (int x = 0; x < width; x++)
      {
        float nx = x / (width - 1f);
        float distanciaY = Mathf.Abs(y - centroY);
        float grosor = Mathf.Clamp01(1f - (distanciaY / 3.4f));
        float extremos = Mathf.SmoothStep(0f, 0.12f, nx) * (1f - Mathf.SmoothStep(0.88f, 1f, nx));
        float alpha = Mathf.Pow(grosor, 1.65f) * extremos;
        pixels[(y * width) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaRayo.SetPixels(pixels);
    texturaRayo.Apply(false, true);
    spriteRayo = Sprite.Create(texturaRayo, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    spriteRayo.name = "PurificadoraRayRuntime";
    return spriteRayo;
  }

  private static Sprite ObtenerSpriteGota()
  {
    if (spriteGota != null)
    {
      return spriteGota;
    }

    const int width = 32;
    const int height = 48;
    texturaGota = new Texture2D(width, height, TextureFormat.ARGB32, false);
    texturaGota.name = "PurificadoraDropRuntime";
    texturaGota.wrapMode = TextureWrapMode.Clamp;
    texturaGota.filterMode = FilterMode.Bilinear;
    texturaGota.hideFlags = HideFlags.HideAndDontSave;

    Color[] pixels = new Color[width * height];
    float cx = (width - 1) * 0.5f;
    for (int y = 0; y < height; y++)
    {
      float ny = y / (height - 1f);
      float radioX = Mathf.Lerp(width * 0.16f, width * 0.36f, Mathf.SmoothStep(0f, 1f, 1f - Mathf.Abs(ny - 0.34f) * 1.45f));
      float cy = height * 0.44f;
      for (int x = 0; x < width; x++)
      {
        float dx = (x - cx) / Mathf.Max(1f, radioX);
        float dy = (y - cy) / (height * 0.36f);
        float forma = (dx * dx) + (dy * dy);
        float punta = Mathf.SmoothStep(0.52f, 1f, ny) * Mathf.Clamp01(1f - Mathf.Abs(x - cx) / (width * 0.16f));
        float alpha = Mathf.Clamp01((1f - forma) + (punta * 0.55f));
        alpha = Mathf.Pow(alpha, 1.55f);
        pixels[(y * width) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaGota.SetPixels(pixels);
    texturaGota.Apply(false, true);
    spriteGota = Sprite.Create(texturaGota, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    spriteGota.name = "PurificadoraDropRuntime";
    return spriteGota;
  }
}
