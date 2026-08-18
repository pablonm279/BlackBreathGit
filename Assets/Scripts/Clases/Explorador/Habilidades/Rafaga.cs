using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;


public class Rafaga : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano

    private int hAlcance = 7;
    private int hAncho = 3; //1 - adyancentes también
      public override void  Awake()
    {
      nombre = "Ráfaga";
      IDenClase = 10; // Termina turno
      costoAP = 0;
      costoPM = 2;
      if(NIVEL == 4){costoPM--;}

      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 5;
      if(NIVEL > 2){cooldownMax--;}
      bAfectaObstaculos = false;
       tipoPorcentaje = 2;
      bonusAtaque = -2;
      if(NIVEL > 1){bonusAtaque++;}
      if(NIVEL == 5){bonusAtaque++;}
     
      XdDanio = 1;
      daniodX = 10; //1d10
      tipoDanio = 1; //Perforante
      criticoRangoHab = 0;

      requiereRecurso = 1; //esto es para que el boton no se active al apretar si no tiene X recursos (ej Flecha). Ver en BotonHabilidad.

      







      imHab = Resources.Load<Sprite>("imHab/Explorador_Rafaga");
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
    int criticoPorcentaje = Mathf.Clamp(21 - criticoBaseMin, 0, 20) * 5;

    string tituloEs = "Rafaga I";
    string tituloEn = "Barrage I";
    string tituloPt = "Rajada I";
    if (NIVEL == 2) { tituloEs = "Rafaga II"; tituloEn = "Barrage II"; }
    if (NIVEL == 3) { tituloEs = "Rafaga III"; tituloEn = "Barrage III"; }
    if (NIVEL == 4) { tituloEs = "Rafaga IV a"; tituloEn = "Barrage IV a"; }
    if (NIVEL == 5) { tituloEs = "Rafaga IV b"; tituloEn = "Barrage IV b"; }
    if (NIVEL == 2) { tituloPt = "Rajada II"; }
    if (NIVEL == 3) { tituloPt = "Rajada III"; }
    if (NIVEL == 4) { tituloPt = "Rajada IV a"; }
    if (NIVEL == 5) { tituloPt = "Rajada IV b"; }

    int bonusAtaqueNivel = bonusAtaque;
    string rangoDanio = FormatearRangoDados(1, 10, 1);
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
    string bonusTirada = TextoModificadorDescripcion(ataqueActual) + TextoModificadorDescripcion(bonusAtaqueNivel);

    if (esIngles)
    {
      string agilidad = TerminoDescripcion(TerminoDescripcionId.Agilidad, $"Agility ({agilidadActual})");
      string defensa = TerminoDescripcion(TerminoDescripcionId.Defensa, "Defense", "IconoDefensa");
      string danioPerforante = TerminoDescripcion(TerminoDescripcionId.DanioPerforante, "Piercing damage", "dano_perforante");
      string flecha = TerminoDescripcion(TerminoDescripcionId.Flecha, "Arrow");
      string valentia = TerminoDescripcion(TerminoDescripcionId.Valentia, "Valour", "Valentía");
      string ap = TerminoDescripcion(TerminoDescripcionId.PuntosAccion, "AP", "ap");
      string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crit", "critico");
      string proximaMejora = null;
      if (DebeMostrarProximaMejoraDescripcion())
      {
        if (NIVEL < 2) { proximaMejora = "+1 Attack Roll bonus."; }
        else if (NIVEL == 2) { proximaMejora = "-1 cooldown."; }
        else if (NIVEL == 3) { proximaMejora = "Option A: -1 Valour cost.\nOption B: +2 Attack Roll bonus."; }
      }

      txtDescripcion = ConstruirDescripcionNormalizadaIngles(
        tituloEn,
        "Spends remaining AP and Arrows on repeated shots.",
        new[]
        {
          LineaDescripcion("Target", "1 enemy"),
          LineaDescripcion("Effect", "Repeatedly attacks the target; selects another enemy if it dies."),
          LineaDescripcion("Repeat", $"While current {ap} and {flecha}s remain; each shot consumes 1 AP and 1 Arrow.", 1),
          LineaDescripcion("Attack Roll", $"1d20 + {agilidad}{bonusTirada} vs {defensa}. Fumble: 5%. {critico}: {criticoPorcentaje}%.", 1),
          LineaDescripcion("On hit", $"Suffers {rangoDanio} + {agilidad} as {danioPerforante}.", 1),
          LineaDescripcion("Cost", $"{costoPM} {valentia}; requires 1 {flecha}"),
          LineaDescripcion("Ends", "Ends the turn")
        },
        proximaMejora);

      if (EsEscenaCampaña())
      {
        ClaseExplorador clase = Usuario.GetComponent<ClaseExplorador>();
        if (clase != null && clase.ObtenerCantidadFlechas() < 1)
        {
          txtDescripcion += $"\n\n<color=#ea0606><b>{TRADU.i.Traducir("No tienes flechas para usar esta habilidad.")}</b></color>";
        }
      }
      return;
    }

    if (esPortugues)
    {
      string agilidade = TerminoDescripcion(TerminoDescripcionId.Agilidad, $"Agilidade ({agilidadActual})");
      string defesa = TerminoDescripcion(TerminoDescripcionId.Defensa, "Defesa", "IconoDefensa");
      string danoPerfurante = TerminoDescripcion(TerminoDescripcionId.DanioPerforante, "dano Perfurante", "dano_perforante");
      string flecha = TerminoDescripcion(TerminoDescripcionId.Flecha, "Flecha");
      string ap = TerminoDescripcion(TerminoDescripcionId.PuntosAccion, "AP", "ap");
      string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crítico", "critico");
      string proximaMejora = null;
      if (DebeMostrarProximaMejoraDescripcion())
      {
        if (NIVEL < 2) { proximaMejora = "Próximo nível: +1 na Rolagem de ataque."; }
        else if (NIVEL == 2) { proximaMejora = "Próximo nível: -1 de recarga."; }
        else if (NIVEL == 3) { proximaMejora = "Próximo nível: Opção A: -1 de custo de Valentia.\nOpção B: +2 na Rolagem de ataque."; }
      }
      txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(
        tituloPt,
        "Gasta o AP e as Flechas restantes em tiros repetidos.",
        new[]
        {
          LineaDescripcion("Alvo", "1 inimigo"),
          LineaDescripcion("Efeito", "Ataca o alvo repetidamente; seleciona outro inimigo se ele morrer."),
          LineaDescripcion("Repetição", $"Enquanto restarem {ap} atual e {flecha}s; cada tiro consome 1 AP e 1 Flecha.", 1),
          LineaDescripcion("Rolagem de ataque", $"1d20 + {agilidade}{bonusTirada} vs {defesa}. Falha crítica: 5%. {critico}: {criticoPorcentaje}%.", 1),
          LineaDescripcion("Ao acertar", $"Sofre {rangoDanio} + {agilidade} como {danoPerfurante}.", 1),
          LineaDescripcion("Requisito", $"1 {flecha}"),
          LineaDescripcion("Uso", "Encerra o turno")
        },
        proximaMejora);
      return;
    }

    {
      string agilidad = TerminoDescripcion(TerminoDescripcionId.Agilidad, $"Agilidad ({agilidadActual})");
      string defensa = TerminoDescripcion(TerminoDescripcionId.Defensa, "Defensa", "IconoDefensa");
      string danioPerforante = TerminoDescripcion(TerminoDescripcionId.DanioPerforante, "daño Perforante", "dano_perforante");
      string flecha = TerminoDescripcion(TerminoDescripcionId.Flecha, "Flecha");
      string ap = TerminoDescripcion(TerminoDescripcionId.PuntosAccion, "AP", "ap");
      string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crítico", "critico");
      string proximaMejora = null;
      if (DebeMostrarProximaMejoraDescripcion())
      {
        if (NIVEL < 2) { proximaMejora = "Próximo nivel: +1 a la Tirada de ataque."; }
        else if (NIVEL == 2) { proximaMejora = "Próximo nivel: -1 de enfriamiento."; }
        else if (NIVEL == 3) { proximaMejora = "Próximo nivel: Opción A: -1 de costo de Valentía.\nOpción B: +2 a la Tirada de ataque."; }
      }
      txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(
        tituloEs.Replace("Rafaga", "Ráfaga"),
        "Gasta el AP y las Flechas restantes en disparos repetidos.",
        new[]
        {
          LineaDescripcion("Objetivo", "1 enemigo"),
          LineaDescripcion("Efecto", "Ataca repetidamente al objetivo; selecciona otro enemigo si muere."),
          LineaDescripcion("Repetición", $"Mientras queden {ap} actual y {flecha}s; cada disparo consume 1 AP y 1 Flecha.", 1),
          LineaDescripcion("Tirada de ataque", $"1d20 + {agilidad}{bonusTirada} vs {defensa}. Pifia: 5%. {critico}: {criticoPorcentaje}%.", 1),
          LineaDescripcion("Al impactar", $"Sufre {rangoDanio} + {agilidad} como {danioPerforante}.", 1),
          LineaDescripcion("Requisito", $"1 {flecha}"),
          LineaDescripcion("Uso", "Termina el turno")
        },
        proximaMejora);
      return;
    }

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> Ranged attack ({hAlcance} range, width {hAncho})\n";
      cuerpo += $"<color={colorEncabezado}><b>Target:</b></color> 1 enemy; continues to the next enemy if target dies\n";
      cuerpo += $"<color={colorEncabezado}><b>Cost:</b></color> {costoPM} Valour; requires 1 Arrow\n";
      cuerpo += $"<color={colorEncabezado}><b>Loop:</b></color> repeats while current AP and Arrows are above 0\n";
      cuerpo += $"<color={colorEncabezado}><b>Per shot:</b></color> consumes 1 AP and 1 Arrow\n";
      cuerpo += $"<color={colorEncabezado}><b>Roll:</b></color> 1d20 + {atributo}{bonusTirada} vs Defense\n";
      cuerpo += $"<color={colorEncabezado}><b>Fumble:</b></color> 5%   <color={colorEncabezado}><b>Crit:</b></color> {criticoPorcentaje}%\n";
      cuerpo += $"<color={colorEncabezado}><b>Damage:</b></color> {rangoDanio} + {atributo}. <color={colorEncabezado}><b>Type:</b></color> Piercing\n";
      cuerpo += $"<color={colorEncabezado}><b>Turn flow:</b></color> ends turn";
    }
    else if (esPortugues)
    {
      cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Ataque a distancia ({hAlcance} alcance, largura {hAncho})\n";
      cuerpo += $"<color={colorEncabezado}><b>Alvo:</b></color> 1 inimigo; continua no proximo inimigo se o alvo morrer\n";
      cuerpo += $"<color={colorEncabezado}><b>Custo:</b></color> {costoPM} Valentia; requer 1 Flecha\n";
      cuerpo += $"<color={colorEncabezado}><b>Loop:</b></color> repete enquanto AP atuais e Flechas forem maiores que 0\n";
      cuerpo += $"<color={colorEncabezado}><b>Por disparo:</b></color> consome 1 AP e 1 Flecha\n";
      cuerpo += $"<color={colorEncabezado}><b>Rolagem:</b></color> 1d20 + {atributo}{bonusTirada} vs Defesa\n";
      cuerpo += $"<color={colorEncabezado}><b>Falha critica:</b></color> 5%   <color={colorEncabezado}><b>Critico:</b></color> {criticoPorcentaje}%\n";
      cuerpo += $"<color={colorEncabezado}><b>Dano:</b></color> {rangoDanio} + {atributo}. <color={colorEncabezado}><b>Tipo:</b></color> Perfurante\n";
      cuerpo += $"<color={colorEncabezado}><b>Fluxo de turno:</b></color> termina turno";
    }
    else
    {
      cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Ataque a distancia ({hAlcance} alcance, ancho {hAncho})\n";
      cuerpo += $"<color={colorEncabezado}><b>Objetivo:</b></color> 1 enemigo; continua al siguiente enemigo si el objetivo muere\n";
      cuerpo += $"<color={colorEncabezado}><b>Costo:</b></color> {costoPM} Valentía; requiere 1 Flecha\n";
      cuerpo += $"<color={colorEncabezado}><b>Bucle:</b></color> repite mientras AP actuales y Flechas sean mayores que 0\n";
      cuerpo += $"<color={colorEncabezado}><b>Por disparo:</b></color> consume 1 AP y 1 Flecha\n";
      cuerpo += $"<color={colorEncabezado}><b>Tirada:</b></color> 1d20 + {atributo}{bonusTirada} vs Defensa\n";
      cuerpo += $"<color={colorEncabezado}><b>Pifia:</b></color> 5%   <color={colorEncabezado}><b>Crítico:</b></color> {criticoPorcentaje}%\n";
      cuerpo += $"<color={colorEncabezado}><b>Daño:</b></color> {rangoDanio} + {atributo}. <color={colorEncabezado}><b>Tipo:</b></color> Perforante\n";
      cuerpo += $"<color={colorEncabezado}><b>Flujo de turno:</b></color> termina turno";
    }

    string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
    string subtitulo = esIngles
      ? "Spend current AP and Arrows on repeated shots."
      : esPortugues
        ? "Gasta AP atuais e Flechas em disparos repetidos."
        : "Gasta AP actuales y Flechas en disparos repetidos.";

    txtDescripcion = $"<size=115%><color={colorTitulo}><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n";
    txtDescripcion += $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n";
    txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
    txtDescripcion += cuerpo;

    bool mostrarProximoNivel = CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
    if (mostrarProximoNivel)
    {
      if (esIngles)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 roll bonus.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: -1 cooldown.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (-1 Valour Cost) or Option B (+2 roll bonus).</color>"; }
      }
      else if (esPortugues)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 no bonus de rolagem.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: -1 recarga.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (-1 custo de Valentia) ou Opcao B (+2 no bonus de rolagem).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 al bonus de tirada.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: -1 enfriamiento.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A (-1 costo de Valentía) u Opción B (+2 al bonus de tirada).</color>"; }
      }
    }

    if (CampaignManager.Instance != null && CampaignManager.Instance.gameObject != null && CampaignManager.Instance.gameObject.transform.parent != null && CampaignManager.Instance.gameObject.transform.parent.parent != null)
    {
      AdministradorEscenas admin = CampaignManager.Instance.gameObject.transform.parent.parent.GetComponent<AdministradorEscenas>();
      if (admin != null && admin.escenaActual == 1)
      {
        ClaseExplorador clase = Usuario.GetComponent<ClaseExplorador>();
        if (clase != null && clase.ObtenerCantidadFlechas() < 1)
        {
          txtDescripcion += $"\n\n<color=#ea0606><b>{TRADU.i.Traducir("No tienes flechas para usar esta habilidad.")}</b></color>";
        }
      }
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
        if(Usuario.GetComponent<ClaseExplorador>().ObtenerCantidadFlechas() > 0)
        {
          Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
          ObtenerObjetivos();

        
          BattleManager.Instance.SeleccionandoObjetivo = true;
          BattleManager.Instance.HabilidadActiva = this;
        }

        
    }
    
      

    protected override Task EsperarPreImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        return Task.CompletedTask;
    }

    protected override Task EsperarPostImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        return Task.CompletedTask;
    }

    public async override void AplicarEfectosHabilidad(object obj, int tirada, Casilla casillaOrigenTrampas = null)
    {
      Unidad objetivo = (Unidad)obj;
      ClaseExplorador scEstaUnidadExp = Usuario.GetComponent<ClaseExplorador>();
      while(scEstaUnidad.ObtenerAPActual() > 0 && scEstaUnidadExp.ObtenerCantidadFlechas() > 0)
      {

          BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(scEstaUnidad);
          BattleManager.Instance.scUIContadorAP.ActualizarAPCirculos();

      scEstaUnidad.CambiarAPActual(-1);  //Gasta 1 AP por cada ataque
          int tir = UnityEngine.Random.Range(1,21); 
          await Atacar(objetivo, tir);
          await BattleManager.DelayCombateAsync(800);

          if(objetivo.HP_actual < 1)
          {
            List<Unidad> lEnemigos = new List<Unidad>();
            lEnemigos = objetivo.ObtenerListaAliados(false);
            if(lEnemigos.Count > 0)
            {
              objetivo = lEnemigos[0]; //Ataca al siguiente enemigo en la lista
            }
            else
            {

              break;

            }


          }

                
      }
      
      //Usarla termina el turno
      BattleManager.Instance.TerminarTurno();
           
    }

    async Task Atacar(object obj, int tirada)
    {
      
     if(obj is Unidad) //Acá van los efectos a Unidades.
     {
       Unidad objetivo = (Unidad)obj;
       float defensaObjetivo = objetivo.ObtenerdefensaActual();
             
       int danioMarca = 0;
       
      Usuario.GetComponent<ClaseExplorador>().CambiarCantidadFlechas(-1);
      Task impacto = CrearProyectil(objetivo);
      scEstaUnidad.ReproducirAnimacionHabilidadNoHostil();

      if (impacto != null)
      {
        await impacto;
      }
      else
      {
        await BattleManager.DelayCombateAsync(200);
      }
      float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab;
       int bonusAtaqueTotal = bonusAtaque;
       
       //Chequear si tiene Marcar Presa
       if(ChequearTieneMarcarPresa(objetivo)) //Copiar este metodo, ver bien lo de danio marca, para próximas habilidades de daño del explorador
       {
         bonusAtaqueTotal += 4;
         criticoRango += 1;
         danioMarca += 15; //Esto se suma al porcentaje de daño solamente al ser golpe critico, ver mas abajo. Ya que esta amrca agrega % daño crítico.

         if(objetivo.GetComponent<MarcaMarcarPresa>().NIVEL > 1)
         {  danioMarca += 5;   }
         if(objetivo.GetComponent<MarcaMarcarPresa>().NIVEL > 2)
         {  criticoRango += 1;  }
         if(objetivo.GetComponent<MarcaMarcarPresa>().NIVEL == 4)
         {  bonusAtaqueTotal -= 2;  } //NV 4 Quita el debuff al marcar, entonces se resta los 2 que se ponia como compensacion

         
        
       }
       //----

       int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, scEstaUnidad.mod_CarAgilidad, bonusAtaqueTotal, criticoRango, objetivo, 0); 
            
     
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
         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+1+scEstaUnidad.mod_CarAgilidad;

         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);

         danio -= danio/2; //Reduce 50% por roce

         objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);


       }
       else if (resultadoTirada == 2)
       {//GOLPE
         print("Golpe");

         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+1+scEstaUnidad.mod_CarAgilidad;
         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);

         objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

       }
       else if (resultadoTirada == 3)
       {//CRITICO
         print("Crítico");

         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+1+scEstaUnidad.mod_CarAgilidad;
         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje+danioMarca);
      
         objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);
       }
     
        objetivo.AplicarDebuffPorAtaquesreiterados(1);
       }   
     else if (obj is Obstaculo) //Acá van los efectos a Obstaculos
     {
       Obstaculo objetivo = (Obstaculo)obj;
       //---


       float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+1+scEstaUnidad.mod_CarAgilidad;
       danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
       
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
        await BattleManager.DelayCombateAsync(200);

        GameObject flechaPrefab = BattleManager.Instance.contenedorPrefabs.Flecha;
        if (flechaPrefab == null)
        {
            return;
        }

        GameObject proyectil = Instantiate(flechaPrefab);
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
            flight.Configure(transform, destino, 0.7f, 4.9f);
            await flight.EsperarImpactoAsync();
        }
        else
        {
            await BattleManager.DelayCombateAsync(150);
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
      
      
      
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasRango(hAlcance,hAncho);
    
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

    bool ChequearTieneMarcarPresa(Unidad obj)
    { 
      if(obj.GetComponent<MarcaMarcarPresa>() != null)
      {
        if(obj.GetComponent<MarcaMarcarPresa>().quienMarco == scEstaUnidad)
        {
          return true;
        }
      
      }
      return false;
    }

   
 
}





