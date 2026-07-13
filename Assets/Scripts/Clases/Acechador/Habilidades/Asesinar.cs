using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;

public class Asesinar : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano
     ClaseAcechador claseAcechador;
     public override void  Awake()
    {


      nombre = "Asesinar";
      costoAP = 3; 
      costoPM = 1;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      claseAcechador = scEstaUnidad as ClaseAcechador;
      IDenClase = 6;
      esZonal = false;
      enArea = 0;
      esforzable = 1;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 4; 
      bAfectaObstaculos = false;

      bonusAtaque = 0;
    
      XdDanio = 2;
      daniodX = 8; //2d8+2
      tipoDanio = 1; //Cortante
      criticoRangoHab = 0;


      tipoPorcentaje = 2;

      requiereRecurso = 1; //No requiere recurso


      imHab = Resources.Load<Sprite>("imHab/Acechador_Asesinar");
      ActualizarDescripcion();
    }
    
   void Start()
   {
  
   }

   public override void ActualizarDescripcion()
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
    var statsUI = ObtenerStatsDescripcionUI();

    int agilidadActual = statsUI.Agilidad;
    int ataqueActual = statsUI.Ataque;
    int criticoBaseMin = Mathf.Clamp(19 - (statsUI.CriticoRango + criticoRangoHab), 2, 20);

    int danioFijo = 2 + (NIVEL > 1 ? 2 : 0) + (NIVEL == 5 ? 3 : 0);
    int bonoAtaqueAislado = 2 + (NIVEL > 2 ? 1 : 0);
    string rangoDanio = FormatearRangoDados(2, 8, danioFijo);
    int criticoPorcentaje = Mathf.Clamp(21 - criticoBaseMin, 0, 20) * 5;
    string colorTitulo = "#5dade2";
    string colorEncabezado = "#44d3ec";
    string colorAgilidad = "#7fa35a";
    string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
    string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
    string costoSuperior = $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}";
    string atributo = esIngles
      ? $"<color={colorAgilidad}>Agility ({agilidadActual})</color>"
      : esPortugues
        ? $"<color={colorAgilidad}>Agilidade ({agilidadActual})</color>"
        : $"<color={colorAgilidad}>Agilidad ({agilidadActual})</color>";
    string bonusTirada = TextoModificadorDescripcion(ataqueActual) + TextoModificadorDescripcion(bonusAtaque);

    string tituloEs = "Asesinar I";
    string tituloEn = "Assassinate I";
    string tituloPt = "Assassinar I";
    if (NIVEL == 2) { tituloEs = "Asesinar II"; tituloEn = "Assassinate II"; }
    if (NIVEL == 3) { tituloEs = "Asesinar III"; tituloEn = "Assassinate III"; }
    if (NIVEL == 4) { tituloEs = "Asesinar IV a"; tituloEn = "Assassinate IV a"; }
    if (NIVEL == 5) { tituloEs = "Asesinar IV b"; tituloEn = "Assassinate IV b"; }
    if (NIVEL == 2) { tituloPt = "Assassinar II"; }
    if (NIVEL == 3) { tituloPt = "Assassinar III"; }
    if (NIVEL == 4) { tituloPt = "Assassinar IV a"; }
    if (NIVEL == 5) { tituloPt = "Assassinar IV b"; }

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> Ranged attack (4 range)\n";
      cuerpo += $"<color={colorEncabezado}><b>Target:</b></color> 1 enemy\n";
      cuerpo += $"<color={colorEncabezado}><b>Requirement:</b></color> Stalker Hidden\n";
      cuerpo += $"<color={colorEncabezado}><b>Roll:</b></color> 1d20 + {atributo}{bonusTirada} vs Defense\n";
      cuerpo += $"<color={colorEncabezado}><b>Fumble:</b></color> 5%   <color={colorEncabezado}><b>Crit:</b></color> {criticoPorcentaje}%\n";
      cuerpo += $"<color={colorEncabezado}><b>Damage:</b></color> {rangoDanio} + {atributo}. <color={colorEncabezado}><b>Type:</b></color> Piercing\n";
      cuerpo += $"<color={colorEncabezado}><b>Humanoid:</b></color> +2 flat damage\n";
      cuerpo += $"<color={colorEncabezado}><b>If target isolated (has no adyacent enemies):</b></color> +{bonoAtaqueAislado} and x2 final damage\n";
      cuerpo += $"<color={colorEncabezado}><b>On kill:</b></color> gains Hidden, skill cooldown becomes 1";
      if (NIVEL == 4)
      {
        cuerpo += ", +2 Valour";
      }
    }
    else if (esPortugues)
    {
      cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Ataque a distancia (4 de alcance)\n";
      cuerpo += $"<color={colorEncabezado}><b>Alvo:</b></color> 1 inimigo\n";
      cuerpo += $"<color={colorEncabezado}><b>Requisito:</b></color> Acechador Escondido\n";
      cuerpo += $"<color={colorEncabezado}><b>Rolagem:</b></color> 1d20 + {atributo}{bonusTirada} vs Defesa\n";
      cuerpo += $"<color={colorEncabezado}><b>Falha critica:</b></color> 5%   <color={colorEncabezado}><b>Critico:</b></color> {criticoPorcentaje}%\n";
      cuerpo += $"<color={colorEncabezado}><b>Dano:</b></color> {rangoDanio} + {atributo}. <color={colorEncabezado}><b>Tipo:</b></color> Perfurante\n";
      cuerpo += $"<color={colorEncabezado}><b>Humanoide:</b></color> +2 dano fixo\n";
      cuerpo += $"<color={colorEncabezado}><b>Se inimigo isolado (sem inimigos adjacentes):</b></color> +{bonoAtaqueAislado} e x2 no dano final\n";
      cuerpo += $"<color={colorEncabezado}><b>Ao matar:</b></color> ganha Escondido, o cooldown da habilidade fica em 1";
      if (NIVEL == 4)
      {
        cuerpo += ", +2 Valentía";
      }
    }
    else
    {
      cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Ataque a distancia (4 alcance)\n";
      cuerpo += $"<color={colorEncabezado}><b>Objetivo:</b></color> 1 enemigo\n";
      cuerpo += $"<color={colorEncabezado}><b>Requisito:</b></color> Acechador Escondido\n";
      cuerpo += $"<color={colorEncabezado}><b>Tirada:</b></color> 1d20 + {atributo}{bonusTirada} vs Defensa\n";
      cuerpo += $"<color={colorEncabezado}><b>Pifia:</b></color> 5%   <color={colorEncabezado}><b>Crítico:</b></color> {criticoPorcentaje}%\n";
      cuerpo += $"<color={colorEncabezado}><b>Daño:</b></color> {rangoDanio} + {atributo}. <color={colorEncabezado}><b>Tipo:</b></color> Perforante\n";
      cuerpo += $"<color={colorEncabezado}><b>Humanoide:</b></color> +2 daño plano\n";
      cuerpo += $"<color={colorEncabezado}><b>Si objetivo aislado (sin enemigos adyacentes):</b></color> +{bonoAtaqueAislado} y x2 al daño final\n";
      cuerpo += $"<color={colorEncabezado}><b>Al matar:</b></color> gana Escondido, el cooldown de la habilidad se fija en 1";
      if (NIVEL == 4)
      {
        cuerpo += ", +2 Valentía";
      }
    }

    string subtitulo = esIngles
      ? "High-damage stealth attack; stronger against isolated targets."
      : esPortugues
        ? "Ataque de furtividade de alto dano; mais forte contra alvos isolados."
        : "Ataque desde sigilo de alto daño; mas fuerte contra objetivos aislados.";
    string costoValor = esIngles
      ? $"<color={colorEncabezado}><b>Valour cost:</b></color> {costoPM}"
      : esPortugues
        ? $"<color={colorEncabezado}><b>Custo Valentia:</b></color> {costoPM}"
        : $"<color={colorEncabezado}><b>Costo Valentía:</b></color> {costoPM}";

    txtDescripcion = $"<size=115%><color={colorTitulo}><b>{(esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs)}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n";
    txtDescripcion += $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n";
    txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
    txtDescripcion += cuerpo + "\n";
    txtDescripcion += costoValor;

    bool mostrarProximoNivel = CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
    if (!mostrarProximoNivel)
    {
      return;
    }

    if (esIngles)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +2 flat damage.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 attack if target is isolated.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+2 Valour on kill) or Option B (+3 flat damage).</color>"; }
    }
    else if (esPortugues)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +2 de dano fixo.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 ataque se o alvo estiver isolado.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+2 Valentia ao matar) ou Opcao B (+3 de dano fixo).</color>"; }
    }
    else
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +2 de daño plano.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 ataque si el objetivo esta aislado.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A (+2 Valentía al matar) u Opción B (+3 de daño plano).</color>"; }
    }
  }

  private string TextoModificadorDescripcion(int valor)
  {
    if (valor > 0) { return $" + {valor}"; }
    if (valor < 0) { return $" - {Mathf.Abs(valor)}"; }
    return "";
  }

  int damExtra;
      Casilla Origen;
    public override void Activar()
    {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

        
    }
    
    protected override Task EsperarPreImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        float delay = 0.6f;
        var pose = scEstaUnidad.GetComponent<UnidadPoseController>();
        if (pose != null)
        {
            delay = pose.duracionPoseAtacar;
        }

        int ms = Mathf.RoundToInt(Mathf.Max(0.1f, delay * 0.5f) * 1000f);
        return BattleManager.DelayCombateAsync(ms);
    }

    protected override Task EsperarPostImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        return BattleManager.DelayCombateAsync(250);
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
  {

    if (obj is Unidad) //Acá van los efectos a Unidades.
    {
      Unidad objetivo = (Unidad)obj;
      float defensaObjetivo = objetivo.ObtenerdefensaActual();
      print("Defensa: " + defensaObjetivo);

      int danioMarca = 0;

      if (NIVEL > 1) { damExtra += 2; } //A partir del nivel 2, +2 de daño extra
      if (NIVEL == 5) { damExtra += 3; } //A Nv 5, +3 de daño extra

      if (objetivo.ChequearEstaAislado(2))
      {
        bonusAtaque += 2; //Si está aislado, +2 Ataque
        if (NIVEL > 2) { bonusAtaque++; } //A partir del nivel 3, +3 Ataque si está aislado
      }

      float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab;

      int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, scEstaUnidad.mod_CarAgilidad, bonusAtaque, criticoRango, objetivo, 0);
      print("Resultado tirada " + resultadoTirada);


      if (resultadoTirada == -1)
      {//PIFIA 
        print("Pifia");
        objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
        //BattleManager.Instance.TerminarTurno(); //Al ser Pifia, termina el turno.
       scEstaUnidad.EstablecerAPActualA(0);
      }
      else if (resultadoTirada == 0)
      {//FALLO
        print("Fallo");
        objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);

      }
      else if (resultadoTirada == 1)
      {//ROCE
        print("Roce");
        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 2 + damExtra + scEstaUnidad.mod_CarAgilidad + danioMarca;
        VFXAplicar(objetivo.gameObject);
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        danio -= danio / 2; //Reduce 50% por roce

        if (objetivo.TieneTag("Humanoide"))
        { 
            danio += 2; //Si es humanoide, +2 de daño
        }

         if (objetivo.ChequearEstaAislado(2))
        {
          danio *= 2; //Si está aislado, duplica el daño
        }

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

      }
      else if (resultadoTirada == 2)
      {//GOLPE
        print("Golpe");
        VFXAplicar(objetivo.gameObject);
        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 2 + damExtra + scEstaUnidad.mod_CarAgilidad + danioMarca;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);
        if (objetivo.TieneTag("Humanoide"))
        { 
            danio += 2; //Si es humanoide, +2 de daño
        }
        
         if (objetivo.ChequearEstaAislado(2))
        {
          danio *= 2; //Si está aislado, duplica el daño
        }

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

      

      }
      else if (resultadoTirada == 3)
      {//CRITICO
                print("Crítico");
        VFXAplicar(objetivo.gameObject);
        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 2 + damExtra + scEstaUnidad.mod_CarAgilidad + danioMarca;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        if (objetivo.TieneTag("Humanoide"))
        { 
            danio += 2; //Si es humanoide, +2 de daño
        } 
        
        if (objetivo.ChequearEstaAislado(2))
        {
          danio *= 2; //Si está aislado, duplica el daño
        }


        objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);

       
      }

      fueElObjetivoAsesinado = objetivo;
      Invoke("ChequeoMuerteObjetivo", 3.0f); //Chequea si el objetivo murió, y aplica efectos de ser así.

      objetivo.AplicarDebuffPorAtaquesreiterados(1);
    }
    else if (obj is Obstaculo) //Acá van los efectos a Obstaculos
    {
      Obstaculo objetivo = (Obstaculo)obj;
      //---

      VFXAplicar(objetivo.gameObject);
      float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 3 + damExtra + scEstaUnidad.mod_CarAgilidad;
      danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

      objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
    }
  }
 Unidad fueElObjetivoAsesinado;
  void ChequeoMuerteObjetivo()
  {
    bool aplicarEfectos = false;
    if (fueElObjetivoAsesinado == null)
    {
      aplicarEfectos = true; //Si no existe se asume que murio
    } //Si no había objetivo, no hace nada
    else if (fueElObjetivoAsesinado.HP_actual < 1)
    {
      aplicarEfectos = true; //Si no tiene vida, murio
    }

    if (aplicarEfectos)
    { 
      scEstaUnidad.GanarEscondido(1);
      cooldownActual = 1; //Si mata, reduce el cooldown a 1 turno.

      if (NIVEL == 4) { scEstaUnidad.SumarValentia(2); }
    }
    fueElObjetivoAsesinado = null;
  }



 
       void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_ASesinar");
      if (objetivo == null)
      {
        return;
      }

    if (VFXenObjetivo != null)
    {
      GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
      vfx.transform.parent = objetivo.transform;
      VFXSoloSonido.OcultarVisuales(vfx);
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
      Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
      RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);
    }

    AsesinarImpactoVFX.Crear(objetivo);

    }
    
  
    //Provisorio
    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();

    private void ObtenerObjetivos()
    {
      //Cualquier objetivo en 1 de alcance 3 de ancho
      lObjetivosPosibles.Clear();
      
      //Melee - Si está en columna 3 de su lado, aumenta el rango ignorando cada columna vacia del lado opuesto
      int rangoPlus = 0;
   
      
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasRango(4,0);
    
      foreach(Casilla c in lCasillasafectadas)
      {
       
       
       c.ActivarCapaColorRojo();
       if(esMelee)//Si hab es melee, activa capa roja, de columna al alcance final, no de las otras también
       {
         if(c.transform.GetChild(2).gameObject.activeInHierarchy){ c.DesactivarCapaColorRojo();}
       } 



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
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());;
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

      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
       
    
        



      
         
    }

   
 
}

public class AsesinarImpactoVFX : MonoBehaviour
{
  private const float Duracion = 0.72f;
  private const int Particulas = 10;

  private RectTransform root;
  private CanvasGroup canvasGroup;
  private Image sombra;
  private Image pulso;
  private Image corteA;
  private Image corteB;
  private Image[] particulas;
  private Vector2[] particulaDir;
  private float[] particulaFase;
  private Vector2 tamanoBase;
  private float tiempo;

  private static Sprite spriteSuave;
  private static Sprite spriteCorte;
  private static Sprite spriteParticula;
  private static Texture2D texturaSuave;
  private static Texture2D texturaCorte;
  private static Texture2D texturaParticula;

  public static void Crear(GameObject objetivo)
  {
    if (objetivo == null)
    {
      return;
    }

    RectTransform imagenBase = null;
    Unidad unidad = objetivo.GetComponent<Unidad>();
    if (unidad != null && unidad.uImage != null)
    {
      imagenBase = unidad.uImage.rectTransform;
    }

    Canvas canvas = imagenBase != null
      ? imagenBase.GetComponentInParent<Canvas>(true)
      : objetivo.GetComponentInChildren<Canvas>(true);

    if (canvas == null)
    {
      return;
    }

    RectTransform parent = imagenBase != null
      ? imagenBase.parent as RectTransform
      : canvas.transform as RectTransform;

    if (parent == null)
    {
      return;
    }

    GameObject go = new GameObject("VFX_AsesinarImpacto", typeof(RectTransform), typeof(CanvasGroup), typeof(AsesinarImpactoVFX));
    RectTransform rect = go.GetComponent<RectTransform>();
    rect.SetParent(parent, false);
    rect.anchorMin = new Vector2(0.5f, 0.5f);
    rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.pivot = new Vector2(0.5f, 0.5f);

    Vector2 tamano = imagenBase != null ? ObtenerTamano(imagenBase) : new Vector2(70f, 92f);
    rect.sizeDelta = new Vector2(Mathf.Max(32f, tamano.x * 0.56f), Mathf.Max(38f, tamano.y * 0.54f));

    if (imagenBase != null)
    {
      rect.anchoredPosition = imagenBase.anchoredPosition + new Vector2(0f, tamano.y * 0.06f);
      int targetSibling = Mathf.Min(imagenBase.GetSiblingIndex() + 2, parent.childCount - 1);
      rect.SetSiblingIndex(targetSibling);
    }
    else
    {
      rect.anchoredPosition = Vector2.zero;
    }

    AsesinarImpactoVFX fx = go.GetComponent<AsesinarImpactoVFX>();
    fx.Inicializar(rect.sizeDelta);
  }

  private void Inicializar(Vector2 tamano)
  {
    root = GetComponent<RectTransform>();
    canvasGroup = GetComponent<CanvasGroup>();
    canvasGroup.interactable = false;
    canvasGroup.blocksRaycasts = false;
    tamanoBase = tamano;

    Sprite suave = ObtenerSpriteSuave();
    Sprite corte = ObtenerSpriteCorte();
    Sprite particula = ObtenerSpriteParticula();

    sombra = CrearCapa("Sombra", suave);
    pulso = CrearCapa("Pulso", suave);
    corteA = CrearCapa("CorteA", corte);
    corteB = CrearCapa("CorteB", corte);

    particulas = new Image[Particulas];
    particulaDir = new Vector2[Particulas];
    particulaFase = new float[Particulas];
    for (int i = 0; i < Particulas; i++)
    {
      particulas[i] = CrearCapa("Particula" + i, particula);
      float angulo = ((Mathf.PI * 2f) / Particulas) * i + UnityEngine.Random.Range(-0.28f, 0.28f);
      float xScale = UnityEngine.Random.Range(0.9f, 1.46f);
      float yScale = UnityEngine.Random.Range(0.62f, 1.18f);
      particulaDir[i] = new Vector2(Mathf.Cos(angulo) * xScale, Mathf.Sin(angulo) * yScale);
      particulaFase[i] = UnityEngine.Random.Range(0f, 1f);
    }
  }

  private void Update()
  {
    tiempo += Time.deltaTime;
    float p = Mathf.Clamp01(tiempo / Duracion);
    float golpe = 1f - Mathf.Pow(p, 1.55f);
    float entrada = Mathf.Clamp01(p / 0.07f);
    float salida = 1f - Mathf.SmoothStep(0.5f, 1f, p);
    float flashImpacto = 1f - Mathf.SmoothStep(0f, 0.18f, p);
    float alpha = Mathf.Min(1.35f, (entrada * salida) + (flashImpacto * 0.38f));

    if (canvasGroup != null)
    {
      canvasGroup.alpha = alpha;
    }

    float ancho = tamanoBase.x;
    float alto = tamanoBase.y;
    float expansion = Mathf.Lerp(0.48f, 1.38f, Mathf.SmoothStep(0f, 1f, p));
    float flash = Mathf.Sin(Mathf.Clamp01(p / 0.16f) * Mathf.PI);
    float golpeSeco = 1f + (flashImpacto * 0.58f);
    float escalaXImpacto = 0.3f;

    ConfigurarCapa(
      sombra,
      Vector2.zero,
      new Vector2(ancho * 1.1f, alto * 0.96f) * expansion,
      new Color(0.1f, 0f, 0.01f, 0.74f * golpe));

    ConfigurarCapa(
      pulso,
      Vector2.zero,
      new Vector2(ancho * 0.88f, alto * 0.74f) * Mathf.Lerp(0.32f, 1.42f, p),
      new Color(1f, 0.025f, 0.015f, Mathf.Lerp(0.86f, 0f, p) + (flash * 0.28f)));

    ConfigurarCapa(
      corteA,
      Vector2.zero,
      new Vector2(ancho * Mathf.Lerp(0.32f, 1.02f, entrada), Mathf.Max(7f, alto * 0.16f)) * escalaXImpacto * (1.02f + flash * 0.12f) * golpeSeco,
      new Color(1f, 0.02f, 0.01f, Mathf.Lerp(1f, 0.28f, p) + flashImpacto * 0.35f),
      45f);

    ConfigurarCapa(
      corteB,
      Vector2.zero,
      new Vector2(ancho * Mathf.Lerp(0.28f, 0.96f, entrada), Mathf.Max(6f, alto * 0.135f)) * escalaXImpacto * (1f + flash * 0.1f) * golpeSeco,
      new Color(1f, 0f, 0f, Mathf.Lerp(0.96f, 0.22f, p) + flashImpacto * 0.28f),
      -45f);

    for (int i = 0; i < particulas.Length; i++)
    {
      float local = Mathf.Clamp01((p - 0.035f - (particulaFase[i] * 0.045f)) / 0.58f);
      float vis = Mathf.Sin(local * Mathf.PI);
      Vector2 dir = particulaDir[i];
      Vector2 posicion = new Vector2(dir.x * ancho * Mathf.Lerp(0.08f, 0.72f, local), dir.y * alto * Mathf.Lerp(0.06f, 0.48f, local));
      float tam = Mathf.Lerp(Mathf.Max(4f, ancho * 0.075f), Mathf.Max(1.6f, ancho * 0.022f), local);
      Color color = Color.Lerp(new Color(1f, 0.06f, 0.025f, 0.92f), new Color(0.34f, 0f, 0f, 0f), local);
      color.a *= vis;
      ConfigurarCapa(particulas[i], posicion, Vector2.one * tam, color);
    }

    if (p >= 1f)
    {
      Destroy(gameObject);
    }
  }

  private Image CrearCapa(string nombre, Sprite sprite)
  {
    GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
    RectTransform rect = go.GetComponent<RectTransform>();
    rect.SetParent(root, false);
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

  private static Vector2 ObtenerTamano(RectTransform rect)
  {
    Vector2 tamano = rect.rect.size;
    if (tamano.x <= 0.01f || tamano.y <= 0.01f)
    {
      tamano = rect.sizeDelta;
    }

    if (tamano.x <= 0.01f || tamano.y <= 0.01f)
    {
      tamano = new Vector2(64f, 88f);
    }

    return tamano;
  }

  private static void ConfigurarCapa(Image image, Vector2 posicion, Vector2 tamano, Color color, float rotacionZ = 0f)
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

    const int size = 96;
    texturaSuave = new Texture2D(size, size, TextureFormat.ARGB32, false);
    texturaSuave.name = "AsesinarImpactoSuaveRuntime";
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
        float distancia = Mathf.Sqrt(dx * dx + dy * dy);
        float alpha = Mathf.Pow(Mathf.Clamp01(1f - distancia), 2.2f);
        pixels[(y * size) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaSuave.SetPixels(pixels);
    texturaSuave.Apply(false, true);
    spriteSuave = Sprite.Create(texturaSuave, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    spriteSuave.name = "AsesinarImpactoSuaveRuntime";
    return spriteSuave;
  }

  private static Sprite ObtenerSpriteCorte()
  {
    if (spriteCorte != null)
    {
      return spriteCorte;
    }

    const int width = 128;
    const int height = 24;
    texturaCorte = new Texture2D(width, height, TextureFormat.ARGB32, false);
    texturaCorte.name = "AsesinarImpactoCorteRuntime";
    texturaCorte.wrapMode = TextureWrapMode.Clamp;
    texturaCorte.filterMode = FilterMode.Bilinear;
    texturaCorte.hideFlags = HideFlags.HideAndDontSave;

    Color[] pixels = new Color[width * height];
    float centroY = (height - 1) * 0.5f;
    float centroX = (width - 1) * 0.5f;
    for (int y = 0; y < height; y++)
    {
      for (int x = 0; x < width; x++)
      {
        float bordeX = 1f - Mathf.Abs((x - centroX) / centroX);
        float bordeY = 1f - Mathf.Abs((y - centroY) / centroY);
        float alpha = Mathf.Pow(Mathf.Clamp01(bordeX), 0.22f) * Mathf.Pow(Mathf.Clamp01(bordeY), 1.6f);
        pixels[(y * width) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaCorte.SetPixels(pixels);
    texturaCorte.Apply(false, true);
    spriteCorte = Sprite.Create(texturaCorte, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    spriteCorte.name = "AsesinarImpactoCorteRuntime";
    return spriteCorte;
  }

  private static Sprite ObtenerSpriteParticula()
  {
    if (spriteParticula != null)
    {
      return spriteParticula;
    }

    const int size = 32;
    texturaParticula = new Texture2D(size, size, TextureFormat.ARGB32, false);
    texturaParticula.name = "AsesinarImpactoParticulaRuntime";
    texturaParticula.wrapMode = TextureWrapMode.Clamp;
    texturaParticula.filterMode = FilterMode.Bilinear;
    texturaParticula.hideFlags = HideFlags.HideAndDontSave;

    Color[] pixels = new Color[size * size];
    float centro = (size - 1) * 0.5f;
    float radio = size * 0.5f;
    for (int y = 0; y < size; y++)
    {
      for (int x = 0; x < size; x++)
      {
        float dx = (x - centro) / radio;
        float dy = (y - centro) / radio;
        float distancia = Mathf.Sqrt(dx * dx + dy * dy);
        float alpha = Mathf.Pow(Mathf.Clamp01(1f - distancia), 1.35f);
        pixels[(y * size) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaParticula.SetPixels(pixels);
    texturaParticula.Apply(false, true);
    spriteParticula = Sprite.Create(texturaParticula, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    spriteParticula.name = "AsesinarImpactoParticulaRuntime";
    return spriteParticula;
  }
}










