using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;
using System.Data.Common;

public class HojaDeEnergia : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano

   
     public override void  Awake()
    {
      nombre = "Hoja de Energía";
      IDenClase = 5;
      costoAP = 3;
      costoPM = 1;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = true;
      enArea = 0;
      esforzable = 1;
      esCargable = false;
      esMelee = true;
      esHostil = true;
      cooldownMax = 3;
      bAfectaObstaculos = true;
       tipoPorcentaje = 1;
      targetEspecial = 3;
      if (NIVEL == 4) { targetEspecial = 4;}

      bonusAtaque = -1;
      if(NIVEL > 2){bonusAtaque += 1000;}
      XdDanio = 2;
      daniodX = 6; //2d6
      tipoDanio = 10; //Verdadero
      criticoRangoHab = 0;

      imHab = Resources.Load<Sprite>("imHab/Canalizador_HojaDeEnergia");
      
      
    }

  public override void ActualizarDescripcion()
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
    var statsUI = ObtenerStatsDescripcionUI();

    int fuerzaActual = statsUI.Fuerza;
    int ataqueActual = statsUI.Ataque;
    int criticoMin = Mathf.Clamp(19 - statsUI.CriticoRango, 2, 20);
    int bonusAtaqueNivel = NIVEL > 2 ? 0 : -1;
    int bonusDanioNivel = NIVEL > 1 ? 2 : 0;
    int ancho = NIVEL == 4 ? 3 : 2;
    int sangrado = NIVEL == 5 ? 3 : 2;
    int reduccionRes = NIVEL == 5 ? 5 : 3;
    string lineaSalvacionEs = ConstruirLineaSalvacion(false, TipoSalvacionDescripcion.Fortaleza, 12);
    string lineaSalvacionEn = ConstruirLineaSalvacion(true, TipoSalvacionDescripcion.Fortaleza, 12);

    string tituloEs = "Hoja Energetica I";
    string tituloEn = "Energy Blade I";
    string tituloPt = "Lamina Energetica I";
    if (NIVEL == 2) { tituloEs = "Hoja Energetica II"; tituloEn = "Energy Blade II"; }
    if (NIVEL == 3) { tituloEs = "Hoja Energetica III"; tituloEn = "Energy Blade III"; }
    if (NIVEL == 4) { tituloEs = "Hoja Energetica IV a"; tituloEn = "Energy Blade IV a"; }
    if (NIVEL == 5) { tituloEs = "Hoja Energetica IV b"; tituloEn = "Energy Blade IV b"; }
    if (NIVEL == 2) { tituloPt = "Lamina Energetica II"; }
    if (NIVEL == 3) { tituloPt = "Lamina Energetica III"; }
    if (NIVEL == 4) { tituloPt = "Lamina Energetica IV a"; }
    if (NIVEL == 5) { tituloPt = "Lamina Energetica IV b"; }

    string bonusAtaqueEs = bonusAtaqueNivel >= 0 ? $" + {bonusAtaqueNivel}" : $" - {Mathf.Abs(bonusAtaqueNivel)}";
    string bonusAtaqueEn = bonusAtaqueNivel >= 0 ? $" + {bonusAtaqueNivel}" : $" - {Mathf.Abs(bonusAtaqueNivel)}";
    string rangoDanioEs = FormatearRangoDados(2, 6, bonusDanioNivel);

    string danioEs = $"{rangoDanioEs} + <color=#ea0606>Fue ({fuerzaActual})</color>";
    string danioEn = bonusDanioNivel > 0
      ? $"2d6 + {bonusDanioNivel} + <color=#ea0606>Strength ({fuerzaActual})</color>"
      : $"2d6 + <color=#ea0606>Strength ({fuerzaActual})</color>";
    string danioPt = bonusDanioNivel > 0
      ? $"2d6 + {bonusDanioNivel} + <color=#ea0606>Forca ({fuerzaActual})</color>"
      : $"2d6 + <color=#ea0606>Forca ({fuerzaActual})</color>";

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += "<b>Type:</b> Melee\n";
      cuerpo += $"<b>Target:</b> Front area ({ancho} width)\n";
      cuerpo += $"<b>Roll:</b> 1d20 + <color=#ea0606>Strength ({fuerzaActual})</color>  {bonusAtaqueEn} vs Defense. Fumble: 1. Crit: {criticoMin}-20\n";
      cuerpo += $"<b>Damage:</b> {danioEn} | <b>Type:</b> True\n";
      cuerpo += lineaSalvacionEn + "\n";
      cuerpo += $"<b>On failed save:</b> +{sangrado} Bleed and -{reduccionRes} to all Resistances";
    }
    else if (esPortugues)
    {
      cuerpo += "<b>Tipo:</b> Melee\n";
      cuerpo += $"<b>Alvo:</b> Area frontal ({ancho} de largura)\n";
      cuerpo += $"<b>Rolagem:</b> 1d20 + <color=#ea0606>Forca ({fuerzaActual})</color> + Ataque ({ataqueActual}){bonusAtaqueEs} vs Defesa. Falha critica: 1. Critico: {criticoMin}-20\n";
      cuerpo += $"<b>Dano:</b> {danioPt} | <b>Tipo:</b> Verdadeiro\n";
      cuerpo += $"{ConstruirLineaSalvacion(false, TipoSalvacionDescripcion.Fortaleza, 12)}\n";
      cuerpo += $"<b>Se falhar TS:</b> +{sangrado} Sangramento e -{reduccionRes} em todas as Resistencias";
    }
    else
    {
      cuerpo += "<b>Tipo:</b> Melee\n";
      cuerpo += $"<b>Objetivo:</b> Área frontal ({ancho} de ancho)\n";
      cuerpo += $"<b>Tirada:</b> 1d20 + <color=#ea0606>Fue ({fuerzaActual})</color> + Ataque ({ataqueActual}){bonusAtaqueEs} vs Defensa. Pifia: 1. Crítico: {criticoMin}-20\n";
      cuerpo += $"<b>Daño:</b> {danioEs} | <b>Tipo:</b> Verdadero\n";
      cuerpo += lineaSalvacionEs + "\n";
      cuerpo += $"<b>Si falla TS:</b> +{sangrado} Sangrado y -{reduccionRes} a todas las Resistencias";
    }

    string costos = esIngles
      ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}\n- Effortable: Yes ({esforzable})"
      : esPortugues
        ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}\n- Esforcavel: Sim ({esforzable})"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}\n- Esforzable: Si ({esforzable})";

    txtDescripcion = ConstruirDescripcionEstandar(
      esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
      esIngles
        ? "A condensed arcane blade cuts through the front line with true damage."
        : esPortugues
          ? "Uma lamina arcana condensada atravessa a linha de frente com dano verdadeiro."
        : "Una hoja arcana condensada atraviesa la primera línea con daño verdadero.",
      cuerpo,
      costos,
      "#5dade2");

    int pifiaPorcentaje = 5;
    int criticoPorcentaje = Mathf.Clamp(21 - criticoMin, 0, 20) * 5;
    int modificadorAtaqueExtra = ataqueActual + bonusAtaqueNivel;
    string ataqueTxt = modificadorAtaqueExtra == 0
      ? string.Empty
      : modificadorAtaqueExtra > 0 ? $" + {modificadorAtaqueExtra}" : $" - {Mathf.Abs(modificadorAtaqueExtra)}";
    string colorEncabezado = "#44d3ec";
    string colorValor = "#ffffff";
    string colorFuerza = "#d9822b";
    string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
    string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
    string iconoSangrado = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_sangrano\"></voffset></size><space=-0.35em>";
    string iconoDebuff = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_debuff\"></voffset></size><space=-0.35em>";
    string costoSuperior = cooldownMax > 0
      ? $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}"
      : $"{costoAP} {iconoAP}";
    string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
    string subtituloFormato = esIngles
      ? $"Melee true-damage attack against a frontal {ancho}-tile area."
      : esPortugues
        ? $"Ataque melee de dano verdadeiro em area frontal de {ancho} casas."
        : $"Ataque melee de daño verdadero en área frontal de {ancho} casillas.";
    string cuerpoFormato = "";
    if (esIngles)
    {
      cuerpoFormato += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Melee attack</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Target:</b></color> <color={colorValor}>Frontal area ({ancho} width)</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Roll:</b></color> <color={colorValor}>1d20 + <color={colorFuerza}>Strength ({fuerzaActual})</color>{ataqueTxt} vs Defense. Fumble: {pifiaPorcentaje}%. Crit: {criticoPorcentaje}%</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Damage:</b></color> <color={colorValor}>{rangoDanioEs} + <color={colorFuerza}>Strength ({fuerzaActual})</color>. Type: True</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Save:</b></color> <color={colorValor}>Fortitude vs DC 12</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>On failed save:</b></color> <color={colorValor}>{iconoSangrado} +{sangrado} Bleed, {iconoDebuff} -{reduccionRes} all Resistances</color>";
    }
    else if (esPortugues)
    {
      cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Ataque melee</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Alvo:</b></color> <color={colorValor}>Area frontal ({ancho} largura)</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Rolagem:</b></color> <color={colorValor}>1d20 + <color={colorFuerza}>Forca ({fuerzaActual})</color>{ataqueTxt} vs Defesa. Falha critica: {pifiaPorcentaje}%. Critico: {criticoPorcentaje}%</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Daño:</b></color> <color={colorValor}>{rangoDanioEs} + <color={colorFuerza}>Forca ({fuerzaActual})</color>. Tipo: Verdadeiro</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Resistencia:</b></color> <color={colorValor}>Fortitude vs CD 12</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Se falhar:</b></color> <color={colorValor}>{iconoSangrado} +{sangrado} Sangramento, {iconoDebuff} -{reduccionRes} todas as Resistencias</color>";
    }
    else
    {
      cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Ataque melee</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Objetivo:</b></color> <color={colorValor}>Área frontal ({ancho} ancho)</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Tirada:</b></color> <color={colorValor}>1d20 + <color={colorFuerza}>Fuerza ({fuerzaActual})</color>{ataqueTxt} vs Defensa. Pifia: {pifiaPorcentaje}%. Crítico: {criticoPorcentaje}%</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Daño:</b></color> <color={colorValor}>{rangoDanioEs} + <color={colorFuerza}>Fuerza ({fuerzaActual})</color>. Tipo: Verdadero</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>TS:</b></color> <color={colorValor}>Fortaleza vs DC 12</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Si falla:</b></color> <color={colorValor}>{iconoSangrado} +{sangrado} Sangrado, {iconoDebuff} -{reduccionRes} todas las Resistencias</color>";
    }

    txtDescripcion =
      $"<size=115%><color=#5dade2><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n" +
      $"<color=#8f8f8f><i>{subtituloFormato}</i></color>\n\n" +
      "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n" +
      cuerpoFormato;

    bool mostrarProximoNivel = EsEscenaCampaña() && CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
    if (!mostrarProximoNivel)
    {
      return;
    }

    if (esIngles)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +2 damage.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 attack roll bonus.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+1 width) or Option B (+1 Bleed and -2 Resistances).</color>"; }
    }
    else if (esPortugues)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +2 de dano.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 no bonus de ataque.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+1 largura) ou Opcao B (+1 Sangramento e -2 Resistencias).</color>"; }
    }
    else
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +2 de daño.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 al bono de ataque.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A (+1 ancho) u Opción B (+1 Sangrado y -2 Resistencias).</color>"; }
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
       int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, scEstaUnidad.mod_CarFuerza, bonusAtaque, criticoRango, objetivo, 0); // En habilidades caballero +1 a pifia, debilidad de Caballero
       print("Resultado tirada "+resultadoTirada);
     
     
       if(resultadoTirada == -1)
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
         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+scEstaUnidad.mod_CarFuerza;

         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
         if(NIVEL > 1){danio += 2;}

         danio -= danio/2; //Reduce 50% por roce

         objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
    
         EfectoAdicional(objetivo);
          VFXAplicar(objetivo.gameObject);
       }
       else if (resultadoTirada == 2)
       {//GOLPE
         print("Golpe");

         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+scEstaUnidad.mod_CarFuerza;
         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
         if(NIVEL > 1){danio += 2;}

         objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

         EfectoAdicional(objetivo);
        VFXAplicar(objetivo.gameObject);
       }
       else if (resultadoTirada == 3)
       {//CRITICO
         print("Crítico");

         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+scEstaUnidad.mod_CarFuerza;
         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
         if(NIVEL > 1){danio += 2;}
      
         objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);
          VFXAplicar(objetivo.gameObject);
         EfectoAdicional(objetivo);
       }
     
        objetivo.AplicarDebuffPorAtaquesreiterados(1);
       }   
     else if (obj is Obstaculo) //Acá van los efectos a Obstaculos
     {
       Obstaculo objetivo = (Obstaculo)obj;
       //---

        VFXAplicar(objetivo.gameObject);
       float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+scEstaUnidad.mod_CarFuerza;
       danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
       
       objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
     }
    }
    void EfectoAdicional(Unidad Objetivo)
    {
      
        if(Objetivo.TiradaSalvacion(1, 12))
        {
          Objetivo.estado_sangrado += 2;
          Objetivo.estado_ResistenciasReducidas += 3;
          if (NIVEL == 5)
          {
            Objetivo.estado_sangrado += 1;
            Objetivo.estado_ResistenciasReducidas += 2;
          }
        }
       
    }
      void VFXAplicar(GameObject objetivo)
  {
    VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_HojaEnergetica");

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
      List<Casilla> alCasillasafectadas = Origen.ObtenerCasillasRango(2+rangoPlus,1);
    
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
          if(cas.BloqueaAvanceMeleeDesdeFila(posYorigen)) //si alguna de las 3 tiene algo, no aumenta el rango melee
          {
            return 0;
          }
       }
               foreach(Casilla casOsc in casillasAdyacentesyFrenteColumna1) //si ninguna de las tres tiene algo, las oscurece
               {  casOsc.ActivarCapaColorNegro(); }



       

       foreach(Casilla cas in casillasAdyacentesyFrenteColumna2) 
       {
          if(cas.BloqueaAvanceMeleeDesdeFila(posYorigen)) //y si alguna de las 3 tiene algo, aumenta solo en 1 
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





