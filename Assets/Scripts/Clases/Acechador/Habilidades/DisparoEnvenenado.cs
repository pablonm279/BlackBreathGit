using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;


public class DisparoEnvenenado : Habilidad
{


  [SerializeField] private GameObject VFXenObjetivo;
  [SerializeField] private int bonusAtaque;
  [SerializeField] private int XdDanio;
  [SerializeField] private int daniodX;
  [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
  [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano

  private int hAlcance = 3;
  private int hAncho = 1; //1 - adyancentes también
  ClaseAcechador claseAcechador;
    public override void  Awake()
    {
    nombre = "Disparo Envenenado";
    costoAP = 3;
    costoPM = 0;
    IDenClase = 3;
    Usuario = this.gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    claseAcechador = scEstaUnidad as ClaseAcechador;
    esZonal = false;
    enArea = 0;
    esforzable = 1;
    esCargable = false;
    esMelee = false;
    esHostil = true;
    cooldownMax = 5;
    if (NIVEL == 5) {cooldownMax -= 1; } //Nivel 5 reduce cooldown en 1
    bAfectaObstaculos = true;


    XdDanio = 3;
    daniodX = 4; //1d10
    tipoDanio = 2; //Perforante
    criticoRangoHab = 0;

     tipoPorcentaje = 2;
    imHab = Resources.Load<Sprite>("imHab/Acechador_DisparoEnvenenado");
    ActualizarDescripcion();

 

  }

  void Start()
  {
    if (claseAcechador != null)
    { Invoke("ChequearMaestria", 0.2f); }
  }

  int damExtra;
  void ChequearMaestria()
  {
    int NivelMaestria = claseAcechador.PASIVA_MaestriaConBallestaMano;



    if (NivelMaestria == 1)
    {
      bonusAtaque = 1;
      damExtra += 2;

      if (TRADU.i.nIdioma == 1)
      { txtDescripcion += "\n\n<i>Maestría con Ballesta de Mano agrega: +1 Ataque +2 Daño.</i>\n\n"; }
      if (TRADU.i.nIdioma == 2)
      { txtDescripcion += "\n\n<i>Hand Crossbow Mastery adds: +1 Attack +2 Damage.</i>\n\n"; }
      if (TRADU.i.nIdioma == 3)
      { txtDescripcion += "\n\n<i>Maestria com Besta de Mao adiciona: +1 Ataque +2 Dano.</i>\n\n"; }

    }
    else if (NivelMaestria == 2)
    {
      bonusAtaque = 1;
      damExtra += 2;
      criticoRangoHab = 1;
      if (TRADU.i.nIdioma == 1)
      { txtDescripcion += "\n\n<i>Maestría con Ballesta de Mano agrega: +1 Ataque +2 Daño +5% Critico.</i>\n\n"; }
      if (TRADU.i.nIdioma == 2)
      {  txtDescripcion += "\n\n<i>Hand Crossbow Mastery adds: +1 Attack +2 Damage +5% Critical.</i>\n\n"; }
      if (TRADU.i.nIdioma == 3)
      {  txtDescripcion += "\n\n<i>Maestria com Besta de Mao adiciona: +1 Ataque +2 Dano +5% Critico.</i>\n\n"; }

    }
    else if (NivelMaestria == 3)
    {
      bonusAtaque = 1;
      damExtra += 2;
      criticoRangoHab = 1;
      costoAP -= 1; //costo AP -1
      if(TRADU.i.nIdioma== 1)
      { txtDescripcion += "\n\n<i>Maestría con Ballesta de Mano agrega: +1 Ataque +2 Daño +5% Critico, -1 AP.</i>\n\n"; }
      if(TRADU.i.nIdioma== 2)
      { txtDescripcion += "\n\n<i>Hand Crossbow Mastery adds: +1 Attack +2 Damage +5% Critical, -1 AP.</i>\n\n"; }
      if(TRADU.i.nIdioma== 3)
      { txtDescripcion += "\n\n<i>Maestria com Besta de Mao adiciona: +1 Ataque +2 Dano +5% Critico, -1 AP.</i>\n\n"; }


    }
    else if (NivelMaestria == 4)
    {
      bonusAtaque = 1;
      damExtra += 2;
      criticoRangoHab = 1;
      costoAP -= 1; //costo AP -1
      hAlcance += 1; //Alcance +1
      if(TRADU.i.nIdioma== 1)
      { txtDescripcion += "\n\n<i>Maestría con Ballesta de Mano agrega: +1 Alcance +1 Ataque +2 Daño +5% Critico.</i>\n\n"; }
      if(TRADU.i.nIdioma== 2)
      { txtDescripcion += "\n\n<i>Hand Crossbow Mastery adds: +1 Range +1 Attack +2 Damage +5% Critical.</i>\n\n"; }
      if(TRADU.i.nIdioma== 3)
      { txtDescripcion += "\n\n<i>Maestria com Besta de Mao adiciona: +1 Alcance +1 Ataque +2 Dano +5% Critico.</i>\n\n"; }

    }
    else if (NivelMaestria == 5)
    {
      bonusAtaque = 1;
      damExtra += 2;
      criticoRangoHab = 1;
      cooldownMax -= 1; //Cooldown -1
      costoAP -= 1; //costo AP -1
      cooldownActual = 0;
      if(TRADU.i.nIdioma== 1)
      { txtDescripcion += "\n\n<i>Maestría con Ballesta de Mano agrega: Remueve Cooldown, +1 Ataque +2 Daño +5% Critico.</i>\n\n"; }
      if(TRADU.i.nIdioma== 2)
      { txtDescripcion += "\n\n<i>Hand Crossbow Mastery adds: Removes Cooldown, +1 Attack +2 Damage +5% Critical.</i>\n\n"; }
      if(TRADU.i.nIdioma== 3)
      { txtDescripcion += "\n\n<i>Maestria com Besta de Mao adiciona: Remove Recarga, +1 Ataque +2 Dano +5% Critico.</i>\n\n"; }

    }

      ActualizarDescripcion();

  }






  public override void ActualizarDescripcion()
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
    var statsUI = ObtenerStatsDescripcionUI();

    int agilidadActual = statsUI.Agilidad;
    int ataqueActual = statsUI.Ataque;
    int criticoBaseMin = Mathf.Clamp(19 - (statsUI.CriticoRango + criticoRangoHab), 2, 20);

    int dcBase = NIVEL > 2 ? 13 : 12;
    int venenoAplicado = 2 + (NIVEL > 1 ? 1 : 0) + (NIVEL == 4 ? 2 : 0);
    int nivelMaestria = claseAcechador != null ? claseAcechador.PASIVA_MaestriaConBallestaMano : 0;
    string rangoDanio = FormatearRangoDados(3, 4, damExtra);
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

    string tituloEs = "Disparo Envenenado I";
    string tituloEn = "Poison Shot I";
    string tituloPt = "Disparo Envenenado I";
    if (NIVEL == 2) { tituloEs = "Disparo Envenenado II"; tituloEn = "Poison Shot II"; }
    if (NIVEL == 3) { tituloEs = "Disparo Envenenado III"; tituloEn = "Poison Shot III"; }
    if (NIVEL == 4) { tituloEs = "Disparo Envenenado IV a"; tituloEn = "Poison Shot IV a"; }
    if (NIVEL == 5) { tituloEs = "Disparo Envenenado IV b"; tituloEn = "Poison Shot IV b"; }
    if (NIVEL == 2) { tituloPt = "Disparo Envenenado II"; }
    if (NIVEL == 3) { tituloPt = "Disparo Envenenado III"; }
    if (NIVEL == 4) { tituloPt = "Disparo Envenenado IV a"; }
    if (NIVEL == 5) { tituloPt = "Disparo Envenenado IV b"; }

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> Ranged attack ({hAlcance} range)\n";
      cuerpo += $"<color={colorEncabezado}><b>Target:</b></color> 1 enemy or obstacle in range\n";
      cuerpo += $"<color={colorEncabezado}><b>Roll:</b></color> 1d20 + {atributo}{bonusTirada} vs Defense\n";
      cuerpo += $"<color={colorEncabezado}><b>Fumble:</b></color> 5%   <color={colorEncabezado}><b>Crit:</b></color> {criticoPorcentaje}%\n";
      cuerpo += $"<color={colorEncabezado}><b>Damage:</b></color> {rangoDanio} + {atributo}. <color={colorEncabezado}><b>Type:</b></color> Piercing\n";
      cuerpo += $"<color={colorEncabezado}><b>Save:</b></color> Fortitude vs DC {dcBase}\n";
      cuerpo += $"<color={colorEncabezado}><b>On failed save:</b></color> applies {venenoAplicado} Poison";
      if (nivelMaestria > 0)
      {
        cuerpo += $"\n<color={colorEncabezado}><b>Passive:</b></color> Hand Crossbow Mastery (Tier {nivelMaestria})";
      }
    }
    else if (esPortugues)
    {
      cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Ataque a distancia ({hAlcance} de alcance)\n";
      cuerpo += $"<color={colorEncabezado}><b>Alvo:</b></color> 1 inimigo ou obstaculo no alcance\n";
      cuerpo += $"<color={colorEncabezado}><b>Rolagem:</b></color> 1d20 + {atributo}{bonusTirada} vs Defesa\n";
      cuerpo += $"<color={colorEncabezado}><b>Falha critica:</b></color> 5%   <color={colorEncabezado}><b>Critico:</b></color> {criticoPorcentaje}%\n";
      cuerpo += $"<color={colorEncabezado}><b>Dano:</b></color> {rangoDanio} + {atributo}. <color={colorEncabezado}><b>Tipo:</b></color> Perfurante\n";
      cuerpo += $"<color={colorEncabezado}><b>Resistencia:</b></color> Fortitude vs CD {dcBase}\n";
      cuerpo += $"<color={colorEncabezado}><b>Se falhar:</b></color> aplica {venenoAplicado} Veneno";
      if (nivelMaestria > 0)
      {
        cuerpo += $"\n<color={colorEncabezado}><b>Passiva:</b></color> Maestria com Besta de Mao (Tier {nivelMaestria})";
      }
    }
    else
    {
      cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Ataque a distancia ({hAlcance} alcance)\n";
      cuerpo += $"<color={colorEncabezado}><b>Objetivo:</b></color> 1 enemigo u obstaculo en rango\n";
      cuerpo += $"<color={colorEncabezado}><b>Tirada:</b></color> 1d20 + {atributo}{bonusTirada} vs Defensa\n";
      cuerpo += $"<color={colorEncabezado}><b>Pifia:</b></color> 5%   <color={colorEncabezado}><b>Critico:</b></color> {criticoPorcentaje}%\n";
      cuerpo += $"<color={colorEncabezado}><b>Danio:</b></color> {rangoDanio} + {atributo}. <color={colorEncabezado}><b>Tipo:</b></color> Perforante\n";
      cuerpo += $"<color={colorEncabezado}><b>TS:</b></color> Fortaleza vs DC {dcBase}\n";
      cuerpo += $"<color={colorEncabezado}><b>Si falla TS:</b></color> aplica {venenoAplicado} Veneno";
      if (nivelMaestria > 0)
      {
        cuerpo += $"\n<color={colorEncabezado}><b>Pasiva:</b></color> Maestria con Ballesta de Mano (Tier {nivelMaestria})";
      }
    }

    string subtitulo = esIngles
      ? "Ranged attack that applies Poison on failed save."
      : esPortugues
        ? "Ataque a distancia que aplica Veneno se falhar na resistencia."
        : "Ataque a distancia que aplica Veneno si falla TS.";
    string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
    txtDescripcion = $"<size=115%><color={colorTitulo}><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n";
    txtDescripcion += $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n";
    txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
    txtDescripcion += cuerpo;

    bool mostrarProximoNivel = CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
    if (!mostrarProximoNivel)
    {
      return;
    }

    if (esIngles)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 Poison on failed save.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 save DC base.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+2 Poison) or Option B (-1 cooldown).</color>"; }
    }
    else if (esPortugues)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 Veneno se falhar na resistencia.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 na CD base da resistencia.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+2 Veneno) ou Opcao B (-1 recarga).</color>"; }
    }
    else
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 Veneno si falla TS.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 al DC base de TS.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (+2 Veneno) u Opcion B (-1 enfriamiento).</color>"; }
    }
  }

  private string TextoModificadorDescripcion(int valor)
  {
    if (valor > 0) { return $" + {valor}"; }
    if (valor < 0) { return $" - {Mathf.Abs(valor)}"; }
    return "";
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
        if (objetivos == null || objetivos.Count == 0)
        {
            return base.EsperarPreImpactoAsync(objetivos, casillaOrigenTrampas);
        }

        List<Task> impactos = new List<Task>();
        foreach (var objetivo in objetivos)
        {
            var impacto = CrearProyectil(objetivo);
            if (impacto != null)
            {
                impactos.Add(impacto);
            }
        }

        if (impactos.Count == 0)
        {
            return base.EsperarPreImpactoAsync(objetivos, casillaOrigenTrampas);
        }

        return Task.WhenAll(impactos);
    }

    protected override Task EsperarPostImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        return Task.CompletedTask;
    }

  public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla casillaOrigenTrampas = null)
  {

    if (obj is Unidad) //Acá van los efectos a Unidades.
    {
      Unidad objetivo = (Unidad)obj;
      float defensaObjetivo = objetivo.ObtenerdefensaActual();

      int danioMarca = 0;float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab;



      int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, scEstaUnidad.mod_CarAgilidad, bonusAtaque, criticoRango, objetivo, 0);


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
        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + damExtra + scEstaUnidad.mod_CarAgilidad;

        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        danio -= danio / 2; //Reduce 50% por roce

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

        EfectoAdicional(objetivo);


      }
      else if (resultadoTirada == 2)
      {//GOLPE
        print("Golpe");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + damExtra + scEstaUnidad.mod_CarAgilidad;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

        EfectoAdicional(objetivo);

      }
      else if (resultadoTirada == 3)
      {//CRITICO
        print("Critico");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + damExtra + scEstaUnidad.mod_CarAgilidad;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje + danioMarca);

        objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);

        EfectoAdicional(objetivo);
      }

      objetivo.AplicarDebuffPorAtaquesreiterados(1);
    }
    else if (obj is Obstaculo) //Acá van los efectos a Obstaculos
    {
      Obstaculo objetivo = (Obstaculo)obj;
      //---


      float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + damExtra + scEstaUnidad.mod_CarAgilidad;
      danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

      objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
    }
  }
    private Task CrearProyectil(object objetivo)
    {
        if (objetivo == null)
        {
            return Task.CompletedTask;
        }

        return LanzarProyectilAsync(objetivo);
    }

    private async Task LanzarProyectilAsync(object objetivo)
    {
        await BattleManager.DelayCombateAsync(100);

        GameObject proyPrefab = BattleManager.Instance.contenedorPrefabs.ViroteBallestadeManoVeneno;
        if (proyPrefab == null)
        {
            return;
        }

        GameObject proyectil = Instantiate(proyPrefab);
        ArrowFlight flight = proyectil.GetComponent<ArrowFlight>();

        Transform destino = null;
        if (objetivo is Unidad unidadObjetivo)
        {
            destino = unidadObjetivo.transform;
        }
        else if (objetivo is Obstaculo obstaculoObjetivo)
        {
            destino = obstaculoObjetivo.transform;
        }

        if (flight != null && destino != null)
        {
            flight.Configure(transform, destino, 0.055f, 6.3f);
            await flight.EsperarImpactoAsync();
        }
        else
        {
            await BattleManager.DelayCombateAsync(200);
        }
    }

    void VFXAplicar(GameObject objetivo)
  {
    //GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation); 

  }

  //Provisorio
  private List<Unidad> lObjetivosPosibles = new List<Unidad>();
  private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();

  private void ObtenerObjetivos()
  {
    //Cualquier objetivo en 1 de alcance 3 de ancho
    lObjetivosPosibles.Clear();



    List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasRango(hAlcance, hAncho);

    foreach (Casilla c in lCasillasafectadas)
    {


      c.ActivarCapaColorRojo();

      if (c.Presente == null)
      {
        continue;
      }

      if (!bAfectaObstaculos) //Si no afecta obstaculos se descarta la casilla si no hay unidad, si afecta obstaculo se descarta si tampoco hay obstaculo
      {
        if (c.Presente.GetComponent<Unidad>() == null)
        {
          continue;
        }
        if (c.Presente.GetComponent<Unidad>() != null)
        {
          lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>()); ;
        }


      }
      else
      {
        if (c.Presente.GetComponent<Unidad>() == null && c.Presente.GetComponent<Obstaculo>() == null)
        {
          continue;
        }

        if (c.Presente.GetComponent<Unidad>() != null)
        {
          lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>()); ;
        }

        if (c.Presente.GetComponent<Obstaculo>() != null)
        {
          lObstaculosPosibles.Add(c.Presente.GetComponent<Obstaculo>()); ;
        }

      }

    }


    BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();
    BattleManager.Instance.lUnidadesPosiblesHabilidadActiva = new List<Unidad>(lObjetivosPosibles);

    BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
    BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);




  }

 
  void EfectoAdicional(Unidad objetivo)
  {

    int DC = 12;

    if (NIVEL > 2) { DC++; }


    if (objetivo.TiradaSalvacion(objetivo.mod_TSFortaleza, DC))
    {

      objetivo.estado_veneno += 2;
      if (NIVEL > 1) { objetivo.estado_veneno += 1; }
      if (NIVEL == 4) { objetivo.estado_veneno += 2;}



    }

  }
  


}






