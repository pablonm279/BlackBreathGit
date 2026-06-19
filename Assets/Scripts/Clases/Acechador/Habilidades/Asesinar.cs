using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
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
      cuerpo += $"<color={colorEncabezado}><b>Requirement:</b></color> Hidden (1)\n";
      cuerpo += $"<color={colorEncabezado}><b>Roll:</b></color> 1d20 + {atributo}{bonusTirada} vs Defense\n";
      cuerpo += $"<color={colorEncabezado}><b>Fumble:</b></color> 5%   <color={colorEncabezado}><b>Crit:</b></color> {criticoPorcentaje}%\n";
      cuerpo += $"<color={colorEncabezado}><b>Damage:</b></color> {rangoDanio} + {atributo}. <color={colorEncabezado}><b>Type:</b></color> Piercing\n";
      cuerpo += $"<color={colorEncabezado}><b>Humanoid:</b></color> +2 flat damage\n";
      cuerpo += $"<color={colorEncabezado}><b>If isolated:</b></color> +{bonoAtaqueAislado} and x2 final damage\n";
      cuerpo += $"<color={colorEncabezado}><b>On kill:</b></color> gains Hidden (1), skill cooldown becomes 1";
      if (NIVEL == 4)
      {
        cuerpo += ", +2 Valour";
      }
    }
    else if (esPortugues)
    {
      cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Ataque a distancia (4 de alcance)\n";
      cuerpo += $"<color={colorEncabezado}><b>Alvo:</b></color> 1 inimigo\n";
      cuerpo += $"<color={colorEncabezado}><b>Requisito:</b></color> Escondido (1)\n";
      cuerpo += $"<color={colorEncabezado}><b>Rolagem:</b></color> 1d20 + {atributo}{bonusTirada} vs Defesa\n";
      cuerpo += $"<color={colorEncabezado}><b>Falha critica:</b></color> 5%   <color={colorEncabezado}><b>Critico:</b></color> {criticoPorcentaje}%\n";
      cuerpo += $"<color={colorEncabezado}><b>Dano:</b></color> {rangoDanio} + {atributo}. <color={colorEncabezado}><b>Tipo:</b></color> Perfurante\n";
      cuerpo += $"<color={colorEncabezado}><b>Humanoide:</b></color> +2 dano fixo\n";
      cuerpo += $"<color={colorEncabezado}><b>Se estiver isolado:</b></color> +{bonoAtaqueAislado} e x2 no dano final\n";
      cuerpo += $"<color={colorEncabezado}><b>Ao matar:</b></color> ganha Escondido (1), o cooldown da habilidade fica em 1";
      if (NIVEL == 4)
      {
        cuerpo += ", +2 Valentía";
      }
    }
    else
    {
      cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Ataque a distancia (4 alcance)\n";
      cuerpo += $"<color={colorEncabezado}><b>Objetivo:</b></color> 1 enemigo\n";
      cuerpo += $"<color={colorEncabezado}><b>Requisito:</b></color> Escondido (1)\n";
      cuerpo += $"<color={colorEncabezado}><b>Tirada:</b></color> 1d20 + {atributo}{bonusTirada} vs Defensa\n";
      cuerpo += $"<color={colorEncabezado}><b>Pifia:</b></color> 5%   <color={colorEncabezado}><b>Crítico:</b></color> {criticoPorcentaje}%\n";
      cuerpo += $"<color={colorEncabezado}><b>Daño:</b></color> {rangoDanio} + {atributo}. <color={colorEncabezado}><b>Tipo:</b></color> Perforante\n";
      cuerpo += $"<color={colorEncabezado}><b>Humanoide:</b></color> +2 daño plano\n";
      cuerpo += $"<color={colorEncabezado}><b>Si esta aislado:</b></color> +{bonoAtaqueAislado} y x2 al daño final\n";
      cuerpo += $"<color={colorEncabezado}><b>Al matar:</b></color> gana Escondido (1), el cooldown de la habilidad se fija en 1";
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

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);  

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










