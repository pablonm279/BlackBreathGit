using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class CorteHorizontal : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano


  public override void Awake()
  {
    nombre = "Corte Horizontal";
    IDenClase = 3;
    costoAP = 4;
    costoPM = 2;
    if (NIVEL == 4) { costoPM -= 1; }
    Usuario = this.gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    esZonal = true;
    enArea = 0;
    esforzable = 1;
    esCargable = false;
    esMelee = true;
    esHostil = true;
    cooldownMax = 2;
    bAfectaObstaculos = true;

    targetEspecial = 4;

    bonusAtaque = 0;
    if (NIVEL > 2) { bonusAtaque += 1; }
    XdDanio = 2;
    daniodX = 6; //2d6
    tipoDanio = 2; //Cortante
    criticoRangoHab = 0;

    imHab = Resources.Load<Sprite>("imHab/Caballero_CorteHorizontal");

    ActualizarDescripcion();
      
     tipoPorcentaje = 1;
      
    }

    public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
      var statsUI = ObtenerStatsDescripcionUI();

      int fuerzaActual = statsUI.Fuerza;
      int ataqueActual = statsUI.Ataque;
      int criticoMin = Mathf.Clamp(19 - (statsUI.CriticoRango + criticoRangoHab), 2, 20);
      int danioFijo = NIVEL > 1 ? 2 : 0;
      int dcSangrado = NIVEL == 5 ? 13 : 12;
      int sangradoAplicado = NIVEL == 5 ? 4 : 3;
      string rangoDanio = FormatearRangoDados(XdDanio, daniodX, danioFijo);
      int pifiaPorcentaje = 10;
      int criticoPorcentaje = Mathf.Clamp(21 - criticoMin, 0, 20) * 5;
      int modificadorAtaqueExtra = ataqueActual + bonusAtaque;
      string ataqueTxt = modificadorAtaqueExtra == 0
        ? string.Empty
        : modificadorAtaqueExtra > 0 ? $" + {modificadorAtaqueExtra}" : $" - {Mathf.Abs(modificadorAtaqueExtra)}";
      string colorEncabezado = "#44d3ec";
      string colorValor = "#ffffff";
      string colorFuerza = "#d9822b";
      string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
      string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
      string iconoSangrado = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_sangrano\"></voffset></size><space=-0.35em>";
      string costoSuperior = cooldownMax > 0
        ? $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}"
        : $"{costoAP} {iconoAP}";

      string bonusAtaqueTxt = bonusAtaque >= 0 ? $" + {bonusAtaque}" : $" - {Mathf.Abs(bonusAtaque)}";
      string lineaSalvacionEs = ConstruirLineaSalvacion(false, TipoSalvacionDescripcion.Fortaleza, dcSangrado);
      string lineaSalvacionEn = ConstruirLineaSalvacion(true, TipoSalvacionDescripcion.Fortaleza, dcSangrado);

      string tituloEs = "Corte Horizontal I";
      string tituloEn = "Horizontal Slash I";
      string tituloPt = "Corte Horizontal I";
      if (NIVEL == 2) { tituloEs = "Corte Horizontal II"; tituloEn = "Horizontal Slash II"; }
      if (NIVEL == 3) { tituloEs = "Corte Horizontal III"; tituloEn = "Horizontal Slash III"; }
      if (NIVEL == 4) { tituloEs = "Corte Horizontal IV a"; tituloEn = "Horizontal Slash IV a"; }
      if (NIVEL == 5) { tituloEs = "Corte Horizontal IV b"; tituloEn = "Horizontal Slash IV b"; }
      if (NIVEL == 2) { tituloPt = "Corte Horizontal II"; }
      if (NIVEL == 3) { tituloPt = "Corte Horizontal III"; }
      if (NIVEL == 4) { tituloPt = "Corte Horizontal IV a"; }
      if (NIVEL == 5) { tituloPt = "Corte Horizontal IV b"; }

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += "<b>Type:</b> Melee\n";
        cuerpo += "<b>Target:</b> Front area (3 width)\n";
        cuerpo += $"<b>Roll:</b> 1d20 + <color=#ea0606>Strength ({fuerzaActual})</color>  {bonusAtaqueTxt} vs Defense. Fumble: 1-2. Crit: {criticoMin}-20\n";
        cuerpo += danioFijo > 0
          ? $"<b>Damage:</b> 2d6 + {danioFijo} + <color=#ea0606>Strength ({fuerzaActual})</color> | <b>Type:</b> Slashing\n"
          : $"<b>Damage:</b> 2d6 + <color=#ea0606>Strength ({fuerzaActual})</color> | <b>Type:</b> Slashing\n";
        cuerpo += $"{lineaSalvacionEn}\n";
        cuerpo += $"<b>On failed save:</b> +{sangradoAplicado} Bleed";
      }
      else if (esPortugues)
      {
        cuerpo += "<b>Tipo:</b> Corpo a corpo\n";
        cuerpo += "<b>Alvo:</b> Area frontal (3 de largura)\n";
        cuerpo += $"<b>Rolagem:</b> 1d20 + <color=#ea0606>Forca ({fuerzaActual})</color> + Ataque ({ataqueActual}){bonusAtaqueTxt} vs Defesa. Falha critica: 1-2. Critico: {criticoMin}-20\n";
        cuerpo += danioFijo > 0
          ? $"<b>Dano:</b> 2d6 + {danioFijo} + <color=#ea0606>Forca ({fuerzaActual})</color> | <b>Tipo:</b> Cortante\n"
          : $"<b>Dano:</b> 2d6 + <color=#ea0606>Forca ({fuerzaActual})</color> | <b>Tipo:</b> Cortante\n";
        cuerpo += $"{lineaSalvacionEs}\n";
        cuerpo += $"<b>Se falhar na resistencia:</b> +{sangradoAplicado} Sangramento";
      }
      else
      {
        cuerpo += "<b>Tipo:</b> Melee\n";
        cuerpo += "<b>Objetivo:</b> Área frontal (3 de ancho)\n";
        cuerpo += $"<b>Tirada:</b> 1d20 + <color=#ea0606>Fue ({fuerzaActual})</color> + Ataque ({ataqueActual}){bonusAtaqueTxt} vs Defensa. Pifia: 1-2. Crítico: {criticoMin}-20\n";
        cuerpo += $"<b>Daño:</b> {rangoDanio} + <color=#ea0606>Fue ({fuerzaActual})</color> | <b>Tipo:</b> Cortante\n";
        cuerpo += $"{lineaSalvacionEs}\n";
        cuerpo += $"<b>Si falla TS:</b> +{sangradoAplicado} Sangrado";
      }

      string costos = esIngles
        ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}\n- Effortable: Yes ({esforzable})"
        : esPortugues
          ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}\n- Esforcavel: Sim ({esforzable})"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}\n- Esforzable: Si ({esforzable})";

      txtDescripcion = ConstruirDescripcionEstandar(
        esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
        esIngles
          ? "The Knight sweeps the greatsword through the front line."
          : esPortugues
            ? "O Cavaleiro varre a linha frontal com a espada montante."
          : "El Caballero barre la linea frontal con el mandoble.",
        cuerpo,
        costos,
        "#5dade2");

      string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
      string subtitulo = esIngles
        ? "Melee sweep that hits the front area and can apply bleed."
        : esPortugues
          ? "Varredura corpo a corpo que atinge a área frontal e pode aplicar sangramento."
          : "Barrido melee que golpea el área frontal y puede aplicar sangrado.";

      string cuerpoFormato = "";
      if (esIngles)
      {
        cuerpoFormato += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Melee area attack</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Target:</b></color> <color={colorValor}>Front area, 3 tiles wide</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Roll:</b></color> <color={colorValor}>1d20 + <color={colorFuerza}>Strength ({fuerzaActual})</color>{ataqueTxt} vs Defense. Fumble: {pifiaPorcentaje}%. Crit: {criticoPorcentaje}%</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Damage:</b></color> <color={colorValor}>{rangoDanio} + <color={colorFuerza}>Strength ({fuerzaActual})</color>. Type: Slashing</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Save:</b></color> <color={colorValor}>Fortitude vs DC {dcSangrado}</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>On failed save:</b></color> <color={colorValor}>{iconoSangrado} +{sangradoAplicado} Bleed</color>";
      }
      else if (esPortugues)
      {
        cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Ataque corpo a corpo em área</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Alvo:</b></color> <color={colorValor}>Área frontal, 3 casas de largura</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Rolagem:</b></color> <color={colorValor}>1d20 + <color={colorFuerza}>Força ({fuerzaActual})</color>{ataqueTxt} vs Defesa. Falha crítica: {pifiaPorcentaje}%. Crítico: {criticoPorcentaje}%</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Daño:</b></color> <color={colorValor}>{rangoDanio} + <color={colorFuerza}>Força ({fuerzaActual})</color>. Tipo: Cortante</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Resistência:</b></color> <color={colorValor}>Fortaleza vs CD {dcSangrado}</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Se falhar:</b></color> <color={colorValor}>{iconoSangrado} +{sangradoAplicado} Sangramento</color>";
      }
      else
      {
        cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Ataque melee en área</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Objetivo:</b></color> <color={colorValor}>Área frontal, 3 casillas de ancho</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Tirada:</b></color> <color={colorValor}>1d20 + <color={colorFuerza}>Fuerza ({fuerzaActual})</color>{ataqueTxt} vs Defensa. Pifia: {pifiaPorcentaje}%. Crítico: {criticoPorcentaje}%</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Daño:</b></color> <color={colorValor}>{rangoDanio} + <color={colorFuerza}>Fuerza ({fuerzaActual})</color>. Tipo: Cortante</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>TS:</b></color> <color={colorValor}>Fortaleza vs DC {dcSangrado}</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Si falla:</b></color> <color={colorValor}>{iconoSangrado} +{sangradoAplicado} Sangrado</color>";
      }

      txtDescripcion =
        $"<size=115%><color=#5dade2><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n" +
        $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n" +
        "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n" +
        cuerpoFormato;

      bool mostrarProximoNivel = CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
      if (!mostrarProximoNivel)
      {
        return;
      }

      if (esIngles)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +2 damage.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 attack roll bonus.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (-1 Valour Cost) or Option B (+1 Save DC and +1 Bleed).</color>"; }
      }
      else if (esPortugues)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nível: +2 de dano.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nível: +1 no bônus de ataque.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nível: Opção A (-1 custo de Valentia) ou Opção B (+1 CD e +1 Sangramento).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +2 de daño.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 al bono de ataque.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A (-1 costo de Valentía) u Opción B (+1 DC y +1 Sangrado).</color>"; }
      }
    }

    Casilla Origen;
    public override void Activar()
    {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

        
    }
    
    
   public override async Task Resolver(List<object> Objetivos, Casilla cas) //Esto esta hecho para que anuncie el uso de la habilidad en el Log
   {
    // El log de uso ahora está centralizado en Habilidad.Resolver
     VFXAplicarOrigen(Usuario.gameObject);

     MeleeApproachMover acercamientoMelee = MeleeApproachMover.ObtenerOCrear(scEstaUnidad);
     bool hizoAproximacion = false;

     try
     {
       object objetivoVisual = ObtenerObjetivoVisualAproximacion(Objetivos, cas);
       if (acercamientoMelee != null && objetivoVisual != null)
       {
         hizoAproximacion = await acercamientoMelee.PrepararAproximacionIAAsync(esMelee, 1, objetivoVisual, false);
       }

       await base.Resolver(Objetivos, cas);
     }
     finally
     {
       if (hizoAproximacion && acercamientoMelee != null)
       {
         await acercamientoMelee.VolverAPosicionInicialAsync();
       }
     }
   }

   object ObtenerObjetivoVisualAproximacion(List<object> objetivos, Casilla casillaClickeada)
   {
     if (casillaClickeada != null && casillaClickeada.Presente != null)
     {
       Unidad unidadCentro = casillaClickeada.Presente.GetComponent<Unidad>();
       if (unidadCentro != null)
       {
         return unidadCentro;
       }

       Obstaculo obstaculoCentro = casillaClickeada.Presente.GetComponent<Obstaculo>();
       if (obstaculoCentro != null)
       {
         return obstaculoCentro;
       }
     }

     if (objetivos == null)
     {
       return null;
     }

     foreach (object objetivo in objetivos)
     {
       if (objetivo is Unidad)
       {
         return objetivo;
       }
     }

     foreach (object objetivo in objetivos)
     {
       if (objetivo is Obstaculo)
       {
         return objetivo;
       }
     }

     return null;
   }

    protected override Task EsperarPreImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        return base.EsperarPreImpactoAsync(objetivos, casillaOrigenTrampas);
    }

    protected override Task EsperarPostImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        return base.EsperarPostImpactoAsync(objetivos, casillaOrigenTrampas);
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla casillaObjetivo)
    {
    
     if(obj is Unidad) //Acá van los efectos a Unidades.
     {
       Unidad objetivo = (Unidad)obj;
       float defensaObjetivo = objetivo.ObtenerdefensaActual();
       print("Defensa: "+ defensaObjetivo);

       float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab;
       int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, scEstaUnidad.mod_CarFuerza, bonusAtaque, criticoRango, objetivo, 1/*!!1 solo en caballero*/); // En habilidades caballero +1 a pifia, debilidad de Caballero
       print("Resultado tirada "+resultadoTirada);


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
        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + scEstaUnidad.mod_CarFuerza;

        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);
        if (NIVEL > 1) { danio += 2; }

        danio -= danio / 2; //Reduce 50% por roce

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

        EfectoAdicional(objetivo);
        VFXAplicar(objetivo.gameObject);

      }
      else if (resultadoTirada == 2)
      {//GOLPE
        print("Golpe");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + scEstaUnidad.mod_CarFuerza;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);
        if (NIVEL > 1) { danio += 2; }

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

        EfectoAdicional(objetivo);
        VFXAplicar(objetivo.gameObject);

      }
      else if (resultadoTirada == 3)
      {//CRITICO
        print("Crítico");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + scEstaUnidad.mod_CarFuerza;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);
        if (NIVEL > 1) { danio += 2; }

        objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);

        EfectoAdicional(objetivo);
        VFXAplicar(objetivo.gameObject);
      }
     
        objetivo.AplicarDebuffPorAtaquesreiterados(1);
       }   
     else if (obj is Obstaculo) //Acá van los efectos a Obstaculos
     {
       Obstaculo objetivo = (Obstaculo)obj;
       //---


       float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+scEstaUnidad.mod_CarFuerza;
       danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
       
       objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
     }
    }
    void EfectoAdicional(Unidad Objetivo)
    {
       if(NIVEL !=5)
       {
        if(Objetivo.TiradaSalvacion(1, 12))
        {
           Estados.Aplicar_Sangrado(Objetivo, 3, scEstaUnidad);
            
        }
       }else //Si es nivel 4b, +1DC sangrado +1 stack
       {
         if(Objetivo.TiradaSalvacion(1, 13))
        {
          Estados.Aplicar_Sangrado(Objetivo, 4, scEstaUnidad);
            
        }
       }
    }
    void VFXAplicar(GameObject objetivo)
  {
    VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_CorteHorizontalImpacto");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5); 
            //---

  }
  
   void VFXAplicarOrigen(GameObject objetivo)
  {
    VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_CorteHorizontalOrigen");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5); 
            //---

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
   
      if(esMelee) 
      {
        if(Usuario.GetComponent<Unidad>().CasillaPosicion.posX == 3)
        {
           rangoPlus = AumentarRangoMelee();
        }

        if (TieneObstaculooUnidadAdelanteDeSuLado() != 0)
        {
          rangoPlus ++;
        }
      }
      List<Casilla> alCasillasafectadas = Origen.ObtenerCasillasRango(2+rangoPlus,0);
    
      foreach(Casilla c in alCasillasafectadas)
      {
       lCasillasafectadas.Add(c);
       
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

    private int AumentarRangoMelee() //aumenta el rango melee si no hay nada en frente ni filas adyacentes al origen de la habilidad
    {
     
      LadoManager scLado = Origen.ladoOpuesto.GetComponent<LadoManager>();

      int posYorigen = scEstaUnidad.CasillaPosicion.posY;
      

      List<Casilla> casillasAdyacentesyFrenteColumna1 = new List<Casilla>();
      List<Casilla> casillasAdyacentesyFrenteColumna2 = new List<Casilla>();
    
      foreach(Transform child in Origen.ladoOpuesto.transform) //Itera en cada casilla del lado opuesto
      {
          Casilla cas = child.GetComponent<Casilla>();

          if(cas.posX == 3) //Columna 1 (frente)
          {
             int calculo = Math.Abs(cas.posY - posYorigen); //distancia en Y al origen para calcular adyacentes o frontal
            
             if(calculo < 2)
             {
               casillasAdyacentesyFrenteColumna1.Add(cas);
             }
          }

          if(cas.posX == 2) //Columna 2 (medio)
          {
             int calculo = Math.Abs(cas.posY - posYorigen); //distancia en Y al origen para calcular adyacentes o frontal
            
             if(calculo < 2)
             {
               casillasAdyacentesyFrenteColumna2.Add(cas);
             }
          }

        
      }

       //Se fija si las 3 casillas de la columna 1 están vacias
       foreach(Casilla cas in casillasAdyacentesyFrenteColumna1)
       {
          if(cas.BloqueaAvanceMeleeDesdeFila(posYorigen, scEstaUnidad)) //si alguna de las 3 tiene algo, no aumenta el rango melee
          {
            return 0;
          }
       }
               foreach(Casilla casOsc in casillasAdyacentesyFrenteColumna1) //si ninguna de las tres tiene algo, las oscurece
               {  casOsc.ActivarCapaColorNegro(); }



       

       foreach(Casilla cas in casillasAdyacentesyFrenteColumna2) 
       {
          if(cas.BloqueaAvanceMeleeDesdeFila(posYorigen, scEstaUnidad)) //y si alguna de las 3 tiene algo, aumenta solo en 1
          {
            return 1;
          }
       }
               foreach(Casilla casOsc in casillasAdyacentesyFrenteColumna2) //si ninguna de las tres tiene algo, las oscurece
               {  casOsc.ActivarCapaColorNegro(); }




       return 2; //si ninguna de las 2 columnas tiene algo, aumenta al maximo
    }

  int TieneObstaculooUnidadAdelanteDeSuLado()
    {
      int orX = Origen.posX;
      int orY = Origen.posY;
      GameObject lado = Origen.ladoGO;

      
      if(orX != 2) //Solamente util en la columna del medio
      {
         return 0;
      }
 
       Casilla casillaRevisar = null;
       foreach(Transform child in lado.transform)
       {
         Casilla cas = child.GetComponent<Casilla>();
         if((cas.posY == orY)&&(cas.posX == orX+1))
         {
          casillaRevisar = cas;
         }

       }

      if(casillaRevisar.Presente != null)
      {
        if(casillaRevisar.Presente.GetComponent<Unidad>() != null)
        {
          return 1; //Devuelve 1 si es unidad
        }

        if(casillaRevisar.Presente.GetComponent<Obstaculo>() != null)
        {
           if(casillaRevisar.Presente.GetComponent<Obstaculo>().bPermiteAtacarDetras)
          {
            return 2; //Devuelve 2 si es obstaculo
          }
          else{ return 0;}
        }
      }
      return 0; //Devuelve 0 si no hay nada 
    }
}






