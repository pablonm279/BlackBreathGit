using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class DescargaDesintegradora : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano

   
    public override void  Awake()
    {
      nombre = "Descarga Desintegradora";
      IDenClase = 9;
      costoAP = 6;
      costoPM = 1;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 3;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 5;
      bAfectaObstaculos = true;

      targetEspecial = 9; //Piramide
       tipoPorcentaje = 3;
      bonusAtaque = 5;
      XdDanio = 3;
      daniodX = 12; //3d12
      tipoDanio = 8; //Arcano
      criticoRangoHab = 2;

      imHab = Resources.Load<Sprite>("imHab/Canalizador_DescargaDesintegradora");
      

      requiereRecurso = 2; //Requiere tener 2 Tier energía 

    }

  public override void ActualizarDescripcion()
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
    var statsUI = ObtenerStatsDescripcionUI();

    int poderActual = statsUI.Poder;
    int ataqueActual = statsUI.Ataque;
    int criticoMin = Mathf.Clamp(19 - (statsUI.CriticoRango + 2), 2, 20);
    int danioFijo = NIVEL > 1 ? 8 : 0;
    int dcDesintegracion = NIVEL > 2 ? 10 : 9;
    int energiaRequerida = NIVEL == 5 ? 1 : 2;
    bool consumeEnergia = NIVEL != 5;
    bool aturdeCaster = NIVEL != 4;
    string rangoDanioEs = FormatearRangoDados(3, 12, danioFijo);
    string lineaSalvacionEs = ConstruirLineaSalvacion(false, TipoSalvacionDescripcion.Fortaleza, dcDesintegracion);
    string lineaSalvacionEn = ConstruirLineaSalvacion(true, TipoSalvacionDescripcion.Fortaleza, dcDesintegracion);

    string tituloEs = "Descarga Desintegradora I";
    string tituloEn = "Disintegrating Discharge I";
    string tituloPt = "Descarga Desintegradora I";
    if (NIVEL == 2) { tituloEs = "Descarga Desintegradora II"; tituloEn = "Disintegrating Discharge II"; }
    if (NIVEL == 3) { tituloEs = "Descarga Desintegradora III"; tituloEn = "Disintegrating Discharge III"; }
    if (NIVEL == 4) { tituloEs = "Descarga Desintegradora IV a"; tituloEn = "Disintegrating Discharge IV a"; }
    if (NIVEL == 5) { tituloEs = "Descarga Desintegradora IV b"; tituloEn = "Disintegrating Discharge IV b"; }
    if (NIVEL == 2) { tituloPt = "Descarga Desintegradora II"; }
    if (NIVEL == 3) { tituloPt = "Descarga Desintegradora III"; }
    if (NIVEL == 4) { tituloPt = "Descarga Desintegradora IV a"; }
    if (NIVEL == 5) { tituloPt = "Descarga Desintegradora IV b"; }

    string danioEs = $"{rangoDanioEs} + <color=#ea0606>Pod ({poderActual})</color>";
    string danioEn = danioFijo > 0
      ? $"3d12 + {danioFijo} + <color=#ea0606>Power ({poderActual})</color>"
      : $"3d12 + <color=#ea0606>Power ({poderActual})</color>";
    string danioPt = danioFijo > 0
      ? $"3d12 + {danioFijo} + <color=#ea0606>Poder ({poderActual})</color>"
      : $"3d12 + <color=#ea0606>Poder ({poderActual})</color>";

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += "<b>Type:</b> Ranged (5 range)\n";
      cuerpo += "<b>Target:</b> Pyramid area\n";
      cuerpo += $"<b>Roll:</b> 1d20 + <color=#ea0606>Power ({poderActual})</color>   + 5 vs Defense. Fumble: 1. Crit: {criticoMin}-20\n";
      cuerpo += $"<b>Damage:</b> {danioEn} | <b>Type:</b> Arcane\n";
      cuerpo += $"{lineaSalvacionEn}. On failed save: disintegrated (instant kill)\n";
      cuerpo += consumeEnergia
        ? "<b>Cast Drawback:</b> -1 Energy Tier"
        : "<b>Cast Drawback:</b> Does not consume Energy Tier";
      cuerpo += "\n";
      cuerpo += aturdeCaster
        ? "<b>Cast Drawback:</b> User is Stunned for 1 turn"
        : "<b>Cast Drawback:</b> Does not Stun the user";
    }
    else if (esPortugues)
    {
      cuerpo += "<b>Tipo:</b> Distancia (5 alcance)\n";
      cuerpo += "<b>Alvo:</b> Area em piramide\n";
      cuerpo += $"<b>Rolagem:</b> 1d20 + <color=#ea0606>Poder ({poderActual})</color> + Ataque ({ataqueActual}) + 5 vs Defesa. Falha critica: 1. Critico: {criticoMin}-20\n";
      cuerpo += $"<b>Dano:</b> {danioPt} | <b>Tipo:</b> Arcano\n";
      cuerpo += $"{ConstruirLineaSalvacion(false, TipoSalvacionDescripcion.Fortaleza, dcDesintegracion)}. Se falhar TS: desintegrado (morte instantanea)\n";
      cuerpo += consumeEnergia
        ? "<b>Custo ao usar:</b> -1 Nivel de Energia"
        : "<b>Custo ao usar:</b> Nao consome Nivel de Energia";
      cuerpo += "\n";
      cuerpo += aturdeCaster
        ? "<b>Custo ao usar:</b> O usuario fica Atordoado por 1 turno"
        : "<b>Custo ao usar:</b> Nao atordoa o usuario";
    }
    else
    {
      cuerpo += "<b>Tipo:</b> Rango (5 alcance)\n";
      cuerpo += "<b>Objetivo:</b> Área en piramide\n";
      cuerpo += $"<b>Tirada:</b> 1d20 + <color=#ea0606>Pod ({poderActual})</color> + Ataque ({ataqueActual}) + 5 vs Defensa. Pifia: 1. Crítico: {criticoMin}-20\n";
      cuerpo += $"<b>Daño:</b> {danioEs} | <b>Tipo:</b> Arcano\n";
      cuerpo += $"{lineaSalvacionEs}. Si falla TS: desintegrado (muerte instantanea)\n";
      cuerpo += consumeEnergia
        ? "<b>Costo al lanzar:</b> -1 Nivel de Energía"
        : "<b>Costo al lanzar:</b> No consume Nivel de Energía";
      cuerpo += "\n";
      cuerpo += aturdeCaster
        ? "<b>Costo al lanzar:</b> El usuario queda Aturdido 1 turno"
        : "<b>Costo al lanzar:</b> No Aturde al usuario";
    }

    string costos = esIngles
      ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}\n- Effortable: Yes ({esforzable})\n- Requires Energy Tier: {energiaRequerida}+"
      : esPortugues
        ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}\n- Esforcavel: Sim ({esforzable})\n- Requer Nivel de Energia: {energiaRequerida}+"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}\n- Esforzable: Si ({esforzable})\n- Requiere Nivel de Energía: {energiaRequerida}+";

    txtDescripcion = ConstruirDescripcionEstandar(
      esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
      esIngles
        ? "At peak charge, the Channeler unleashes a high-risk detonation that can erase targets outright."
        : esPortugues
          ? "No auge da energia, o Canalizador libera uma detonacao de alto risco capaz de apagar alvos."
        : "Con la energía al máximo, el Canalizador libera una detonacion de alto riesgo capaz de borrar objetivos.",
      cuerpo,
      costos,
      "#e67e22");

    int pifiaPorcentaje = 5;
    int criticoPorcentaje = Mathf.Clamp(21 - criticoMin, 0, 20) * 5;
    int modificadorAtaqueExtra = ataqueActual + 5;
    string ataqueTxt = modificadorAtaqueExtra == 0
      ? string.Empty
      : modificadorAtaqueExtra > 0 ? $" + {modificadorAtaqueExtra}" : $" - {Mathf.Abs(modificadorAtaqueExtra)}";
    string colorEncabezado = "#44d3ec";
    string colorValor = "#ffffff";
    string colorPoder = "#2aa6c8";
    string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
    string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
    string iconoEnergia = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_acumularenergia\"></voffset></size><space=-0.35em>";
    string iconoDebuff = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_debuff\"></voffset></size><space=-0.35em>";
    string iconoAturdido = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_aturdido\"></voffset></size><space=-0.35em>";
    string costoSuperior = cooldownMax > 0
      ? $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}"
      : $"{costoAP} {iconoAP}";
    string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
    string subtituloFormato = esIngles
      ? "High-damage pyramid attack; failed save kills the target."
      : esPortugues
        ? "Ataque em piramide de alto dano; falha na resistencia mata o alvo."
        : "Ataque en piramide de alto daño; fallar la TS mata al objetivo.";
    string cuerpoFormato = "";
    if (esIngles)
    {
      cuerpoFormato += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Ranged attack (5 range)</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Target:</b></color> <color={colorValor}>Pyramid area</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Requirement:</b></color> <color={colorValor}>{iconoEnergia} Energy Tier {energiaRequerida}+</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Roll:</b></color> <color={colorValor}>1d20 + <color={colorPoder}>Power ({poderActual})</color>{ataqueTxt} vs Defense. Fumble: {pifiaPorcentaje}%. Crit: {criticoPorcentaje}%</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Damage:</b></color> <color={colorValor}>{rangoDanioEs} + <color={colorPoder}>Power ({poderActual})</color>. Type: Arcane</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Save:</b></color> <color={colorValor}>Fortitude vs DC {dcDesintegracion}; on failed save: {iconoDebuff} instant kill</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Cast cost:</b></color> <color={colorValor}>{(consumeEnergia ? $"{iconoEnergia} -1 Energy Tier" : "No Energy loss")}, {(aturdeCaster ? $"{iconoAturdido} user Stunned 1 turn" : "no self Stun")}</color>";
    }
    else if (esPortugues)
    {
      cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Ataque a distancia (5 alcance)</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Alvo:</b></color> <color={colorValor}>Area em piramide</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Requisito:</b></color> <color={colorValor}>{iconoEnergia} Nivel de Energia {energiaRequerida}+</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Rolagem:</b></color> <color={colorValor}>1d20 + <color={colorPoder}>Poder ({poderActual})</color>{ataqueTxt} vs Defesa. Falha critica: {pifiaPorcentaje}%. Critico: {criticoPorcentaje}%</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Daño:</b></color> <color={colorValor}>{rangoDanioEs} + <color={colorPoder}>Poder ({poderActual})</color>. Tipo: Arcano</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Resistencia:</b></color> <color={colorValor}>Fortitude vs CD {dcDesintegracion}; se falhar: {iconoDebuff} morte instantanea</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Custo ao usar:</b></color> <color={colorValor}>{(consumeEnergia ? $"{iconoEnergia} -1 Nivel de Energia" : "Sem perda de Energia")}, {(aturdeCaster ? $"{iconoAturdido} usuario Atordoado 1 turno" : "sem auto Atordoar")}</color>";
    }
    else
    {
      cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Ataque a distancia (5 alcance)</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Objetivo:</b></color> <color={colorValor}>Área en piramide</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Requisito:</b></color> <color={colorValor}>{iconoEnergia} Nivel de Energia {energiaRequerida}+</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Tirada:</b></color> <color={colorValor}>1d20 + <color={colorPoder}>Poder ({poderActual})</color>{ataqueTxt} vs Defensa. Pifia: {pifiaPorcentaje}%. Crítico: {criticoPorcentaje}%</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Daño:</b></color> <color={colorValor}>{rangoDanioEs} + <color={colorPoder}>Poder ({poderActual})</color>. Tipo: Arcano</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>TS:</b></color> <color={colorValor}>Fortaleza vs DC {dcDesintegracion}; si falla: {iconoDebuff} muerte instantanea</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Costo al lanzar:</b></color> <color={colorValor}>{(consumeEnergia ? $"{iconoEnergia} -1 Nivel de Energia" : "Sin perdida de Energia")}, {(aturdeCaster ? $"{iconoAturdido} usuario Aturdido 1 turno" : "sin auto Aturdir")}</color>";
    }

    txtDescripcion =
      $"<size=115%><color=#e67e22><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n" +
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
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +8 damage.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 save DC.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (no stun) or Option B (no Energy loss).</color>"; }
    }
    else if (esPortugues)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +8 de dano.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 CD de resistencia.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (sem atordoamento) ou Opcao B (sem perda de Energia).</color>"; }
    }
    else
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +8 de daño.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 DC de salvación.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A (sin aturdimiento) u Opción B (sin perdida de Energía).</color>"; }
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
  await  base.Resolver(Objetivos);

    if (NIVEL != 4) { scEstaUnidad.estado_aturdido+=1; print(6565); }
    if(scEstaUnidad is ClaseCanalizador can){ if (NIVEL != 5) { can.CambiarEnergia(-1); } }
  }
    
    protected override Task EsperarPreImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        float delay = 0.7f;
        var pose = scEstaUnidad.GetComponent<UnidadPoseController>();
        if (pose != null)
        {
            delay = pose.duracionPoseHabilidad;
        }

        int ms = Mathf.RoundToInt(Mathf.Max(0.1f, delay) * 1000f);
        return BattleManager.DelayCombateAsync(ms);
    }

    protected override Task EsperarPostImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        return Task.CompletedTask;
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {
    
     if(obj is Unidad) //Acá van los efectos a Unidades.
     { 
      
        Unidad objetivo = (Unidad)obj;       
       int danioExtra = 0;
       if (NIVEL > 1) { danioExtra += 3; }

       float defensaObjetivo = objetivo.ObtenerdefensaActual();
       print("Defensa: "+ defensaObjetivo);

       float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab;
       int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, scEstaUnidad.mod_CarPoder, bonusAtaque, criticoRango, objetivo, 0); 
       print("Resultado tirada "+resultadoTirada);


      //----

      if (resultadoTirada == -1)
      {//PIFIA 
        print("Pifia");
        objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
        //BattleManager.Instance.TerminarTurno(); //Al ser Pifia, termina el turno.
        scEstaUnidad.EstablecerAPActualA(0);
       VFXAplicar(objetivo.gameObject);
       }
      else if (resultadoTirada == 0)
      {//FALLO
        print("Fallo");
        objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
       VFXAplicar(objetivo.gameObject);
      }
      else if (resultadoTirada == 1)
      {//ROCE
        print("Roce");
        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + danioExtra + scEstaUnidad.mod_CarPoder;
       VFXAplicar(objetivo.gameObject);
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);
        if (NIVEL > 1) { danio += 5; }

        danio -= danio / 2; //Reduce 50% por roce

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

        EfectoAdicional(objetivo);

      }
      else if (resultadoTirada == 2)
      {//GOLPE
        print("Golpe");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + danioExtra + scEstaUnidad.mod_CarPoder;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);
        if (NIVEL > 1) { danio += 5; }

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

        EfectoAdicional(objetivo);
       VFXAplicar(objetivo.gameObject);
      }
      else if (resultadoTirada == 3)
      {//CRITICO
        print("Crítico");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + danioExtra + scEstaUnidad.mod_CarPoder;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje + danioExtra);
        if (NIVEL > 1) { danio += 5; }

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


       float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+scEstaUnidad.mod_CarPoder+2;
       danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
       
        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
     }
    }

    void EfectoAdicional(Unidad Objetivo)
    {
        int dc = 9;
        if (NIVEL > 2) { dc += 1; }
    if (Objetivo.TiradaSalvacion(Objetivo.mod_TSFortaleza, dc))
    {
      Objetivo.RecibirDanio(Objetivo.mod_maxHP, 10, false, scEstaUnidad);
      string objetivoNombre = TRADU.i != null ? TRADU.i.Traducir(Objetivo.uNombre) : Objetivo.uNombre;
      string textoDesintegrado = TRADU.i != null ? TRADU.i.Traducir("fue Desintegrado.") : "fue Desintegrado.";
      BattleManager.Instance.EscribirLog(objetivoNombre + " " + textoDesintegrado);
    }
       
    }
  
    
    void VFXAplicar(GameObject objetivo)
  {
    VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_DescargaDesintegradora");

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
      lCasillasafectadas.Clear();
      
      lCasillasafectadas = Origen.ObtenerCasillasRango(6,0);
    
       foreach(Casilla c in lCasillasafectadas)
      {
       
       c.ActivarCapaColorRojo();
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

      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
       
    
        



      
    
         
    }

    
}






