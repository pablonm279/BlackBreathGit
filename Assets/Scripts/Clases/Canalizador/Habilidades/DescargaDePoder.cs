using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class DescargaDePoder : Habilidad
{
   
    private const int DificultadSalvacionReflejos = 13;

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano

   
    public override void  Awake()
    {
      nombre = "Descarga De Poder";
      IDenClase = 2;
      costoAP = 4;
      if (NIVEL == 4) { costoAP--; }

      costoPM = 0;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 2;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 4;
      if (NIVEL == 5) { cooldownMax--; }
      bAfectaObstaculos = true;

      targetEspecial = 8; //T    
       tipoPorcentaje = 3;
      bonusAtaque = 0;
      if (NIVEL > 2) { bonusAtaque += 1; }
      XdDanio = 3;
      daniodX = 6; //3d6
      tipoDanio = 8; //Arcano
      criticoRangoHab = 2;

      imHab = Resources.Load<Sprite>("imHab/Canalizador_DescargaDePoder");
      

      
    }
   
    public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
      var statsUI = ObtenerStatsDescripcionUI();

      int poderActual = statsUI.Poder;
      int danioFijo = NIVEL > 1 ? 5 : 0;
      int dcSalvacion = ObtenerDificultadSalvacionReflejos();

      string tituloEs = "Descarga de Poder I";
      string tituloEn = "Power Discharge I";
      string tituloPt = "Descarga de Poder I";
      if (NIVEL == 2) { tituloEs = "Descarga de Poder II"; tituloEn = "Power Discharge II"; }
      if (NIVEL == 3) { tituloEs = "Descarga de Poder III"; tituloEn = "Power Discharge III"; }
      if (NIVEL == 4) { tituloEs = "Descarga de Poder IV a"; tituloEn = "Power Discharge IV a"; }
      if (NIVEL == 5) { tituloEs = "Descarga de Poder IV b"; tituloEn = "Power Discharge IV b"; }
      if (NIVEL == 2) { tituloPt = "Descarga de Poder II"; }
      if (NIVEL == 3) { tituloPt = "Descarga de Poder III"; }
      if (NIVEL == 4) { tituloPt = "Descarga de Poder IV a"; }
      if (NIVEL == 5) { tituloPt = "Descarga de Poder IV b"; }

      string rangoDanioEs = FormatearRangoDados(3, 6, danioFijo);

      if (esIngles)
      {
        string poder = TerminoDescripcion(TerminoDescripcionId.Poder, $"Power ({poderActual})");
        string danioArcano = TerminoDescripcion(TerminoDescripcionId.DanioArcano, "Arcane damage", "dano_arcano");
        string salvacionReflejos = TerminoDescripcion(TerminoDescripcionId.SalvacionReflejos, "Reflex", "ic_Reflejos");
        string proximaMejora = null;
        if (DebeMostrarProximaMejoraDescripcion())
        {
          if (NIVEL < 2) { proximaMejora = "+5 damage."; }
          else if (NIVEL == 2) { proximaMejora = "+1 Save DC."; }
          else if (NIVEL == 3) { proximaMejora = "Option A: -1 AP cost.\nOption B: -1 cooldown."; }
        }

        txtDescripcion = ConstruirDescripcionNormalizadaIngles(
          tituloEn,
          "Unleashes an Arcane shockwave through a T-shaped area.",
          new[]
          {
            LineaDescripcion("Target", "T-shaped area"),
            LineaDescripcion("Effect", $"Units in the area suffer {rangoDanioEs} + {poder} as {danioArcano}."),
            LineaDescripcion("Save", $"{salvacionReflejos} vs DC {dcSalvacion}.", 1),
            LineaDescripcion("Successful save", "Suffers 50% damage.", 1),
            LineaDescripcion("Effort", $"Up to {esforzable} AP.")
          },
          proximaMejora);
        return;
      }

      {
        bool pt = esPortugues;
        string poder = TerminoDescripcion(TerminoDescripcionId.Poder, $"Poder ({poderActual})");
        string danioArcano = TerminoDescripcion(TerminoDescripcionId.DanioArcano, pt ? "dano Arcano" : "daño Arcano", "dano_arcano");
        string salvacionReflejos = TerminoDescripcion(TerminoDescripcionId.SalvacionReflejos, pt ? "Reflexos" : "Reflejos", "ic_Reflejos");
        string proximaMejora = null;
        if (DebeMostrarProximaMejoraDescripcion())
        {
          if (NIVEL < 2) { proximaMejora = pt ? "+5 de dano." : "+5 de daño."; }
          else if (NIVEL == 2) { proximaMejora = "+1 CD de salvación."; }
          else if (NIVEL == 3) { proximaMejora = pt ? "Opção A: -1 de custo de AP.\nOpção B: -1 de recarga." : "Opción A: -1 al costo de AP.\nOpción B: -1 de recarga."; }
        }
        txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(
          pt ? tituloPt : tituloEs,
          pt ? "Libera uma onda de choque Arcana através de uma área em forma de T." : "Libera una onda de choque Arcana a través de un área en forma de T.",
          new[]
          {
            LineaDescripcion(pt ? "Alvo" : "Objetivo", pt ? "Área em forma de T" : "Área en forma de T"),
            LineaDescripcion(pt ? "Efeito" : "Efecto", $"{(pt ? "As unidades na área sofrem" : "Las unidades en el área sufren")} {rangoDanioEs} + {poder} como {danioArcano}."),
            LineaDescripcion(pt ? "Salvamento" : "Salvación", $"{salvacionReflejos} vs CD {dcSalvacion}.", 1),
            LineaDescripcion(pt ? "Salvamento bem-sucedido" : "Salvación exitosa", pt ? "Sofre 50% do dano." : "Sufre el 50% del daño.", 1),
            LineaDescripcion(pt ? "Esforço" : "Esfuerzo", $"{(pt ? "Até" : "Hasta")} {esforzable} AP.")
          },
          proximaMejora);
        return;
      }

      string danioEs = $"{rangoDanioEs} + <color=#ea0606>Pod ({poderActual})</color>";
      string danioEn = danioFijo > 0
        ? $"3d6 + {danioFijo} + <color=#ea0606>Power ({poderActual})</color>"
        : $"3d6 + <color=#ea0606>Power ({poderActual})</color>";
      string danioPt = danioFijo > 0
        ? $"3d6 + {danioFijo} + <color=#ea0606>Poder ({poderActual})</color>"
        : $"3d6 + <color=#ea0606>Poder ({poderActual})</color>";

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += "<b>Type:</b> Ranged (5 range)\n";
        cuerpo += "<b>Target:</b> T area (3 horizontal + 2 at the far end)\n";
        cuerpo += "<b>Attack Roll:</b> None. Always hits.\n";
        cuerpo += $"<b>Save:</b> Reflex DC {dcSalvacion}. Success: half damage.\n";
        cuerpo += $"<b>Damage:</b> {danioEn} | <b>Type:</b> Arcane";
      }
      else if (esPortugues)
      {
        cuerpo += "<b>Tipo:</b> Distancia (5 alcance)\n";
        cuerpo += "<b>Alvo:</b> Area em T (3 horizontal + 2 no fundo)\n";
        cuerpo += "<b>Rolagem de ataque:</b> Nenhuma. Acerta automaticamente.\n";
        cuerpo += $"<b>TS:</b> Reflexos DC {dcSalvacion}. Sucesso: metade do dano.\n";
        cuerpo += $"<b>Dano:</b> {danioPt} | <b>Tipo:</b> Arcano";
      }
      else
      {
        cuerpo += "<b>Tipo:</b> Rango (5 alcance)\n";
        cuerpo += "<b>Objetivo:</b> Área en T (3 horizontal + 2 al fondo)\n";
        cuerpo += "<b>Tirada de ataque:</b> No tiene. Pega si o si.\n";
        cuerpo += $"<b>TS:</b> Reflejos DC {dcSalvacion}. Si la supera, recibe mitad de daño.\n";
        cuerpo += $"<b>Daño:</b> {danioEs} | <b>Tipo:</b> Arcano";
      }

      string costos = esIngles
        ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}\n- Effortable: Yes ({esforzable})"
        : esPortugues
          ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}\n- Esforcavel: Sim ({esforzable})"
          : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}\n- Esforzable: Si ({esforzable})";

      txtDescripcion = ConstruirDescripcionEstandar(
        esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
        esIngles
          ? "The Channeler releases a concentrated shockwave that sweeps enemies in a T pattern."
          : esPortugues
            ? "O Canalizador libera uma descarga concentrada que varre inimigos em padrao de T."
          : "El Canalizador libera una descarga concentrada que barre enemigos en patron de T.",
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
      string subtituloFormato = esIngles
        ? "Ranged arcane discharge in a T-shaped area."
        : esPortugues
          ? "Descarga arcana a distancia em area de T."
          : "Descarga arcana a distancia en area de T.";
      string cuerpoFormato = "";
      if (esIngles)
      {
        cuerpoFormato += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Ranged ability (5 range)</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Target:</b></color> <color={colorValor}>T area (3 horizontal + 2 at far end)</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Attack Roll:</b></color> <color={colorValor}>None. Always hits.</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Save:</b></color> <color={colorValor}>Reflex DC {dcSalvacion}. Success: half damage.</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Damage:</b></color> <color={colorValor}>{rangoDanioEs} + <color={colorPoder}>Power ({poderActual})</color>. Type: Arcane</color>";
      }
      else if (esPortugues)
      {
        cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Habilidade a distancia (5 alcance)</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Alvo:</b></color> <color={colorValor}>Area em T (3 horizontal + 2 no fundo)</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Rolagem de ataque:</b></color> <color={colorValor}>Nenhuma. Acerta automaticamente.</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>TS:</b></color> <color={colorValor}>Reflexos DC {dcSalvacion}. Sucesso: metade do dano.</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Daño:</b></color> <color={colorValor}>{rangoDanioEs} + <color={colorPoder}>Poder ({poderActual})</color>. Tipo: Arcano</color>";
      }
      else
      {
        cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Habilidad a distancia (5 alcance)</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Objetivo:</b></color> <color={colorValor}>Área en T (3 horizontal + 2 al fondo)</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Tirada de ataque:</b></color> <color={colorValor}>No tiene. Pega si o si.</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>TS:</b></color> <color={colorValor}>Reflejos DC {dcSalvacion}. Si la supera, recibe mitad de daño.</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Daño:</b></color> <color={colorValor}>{rangoDanioEs} + <color={colorPoder}>Poder ({poderActual})</color>. Tipo: Arcano</color>";
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
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +5 damage.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 save DC.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (-1 AP) or Option B (-1 cooldown).</color>"; }
      }
      else if (esPortugues)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +5 de dano.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 DC de salvacao.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (-1 AP) ou Opcao B (-1 recarga).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +5 de daño.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 DC de salvacion.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A (-1 AP) u Opción B (-1 enfriamiento).</color>"; }
      }
    }

    Casilla Origen;
    private Task preImpactoPendiente;

    public override void Activar()
    {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;
    }

    protected override Task EsperarPreImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        Casilla referencia = casillaOrigenTrampas;

        if (objetivos != null && objetivos.Count > 0)
        {
            if (objetivos[0] is Unidad unidadObjetivo)
            {
                referencia = unidadObjetivo.CasillaPosicion;
            }
            else if (objetivos[0] is Obstaculo obstaculoObjetivo)
            {
                referencia = obstaculoObjetivo.CasillaPosicion;
            }
        }

        if (referencia == null && BattleManager.Instance.casillaClickHabilidad != null)
        {
            referencia = BattleManager.Instance.casillaClickHabilidad;
        }

        if (referencia == null)
        {
            referencia = Origen;
        }

        preImpactoPendiente = CrearProyectilFila(referencia);
        return preImpactoPendiente ?? Task.CompletedTask;
    }

    protected override Task EsperarPostImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        return Task.CompletedTask;
    }

    private int ObtenerDificultadSalvacionReflejos()
    {
      return DificultadSalvacionReflejos + (NIVEL > 2 ? 1 : 0);
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {
    
     if(obj is Unidad) //Acá van los efectos a Unidades.
     { 
      
        Unidad objetivo = (Unidad)obj;

        int danioExtra = 0;
        if (NIVEL > 1) { danioExtra += 3; }

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + danioExtra + scEstaUnidad.mod_CarPoder;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);
        if (NIVEL > 1) { danio += 2; }

        bool fallaSalvacion = objetivo.TiradaSalvacion(2, ObtenerDificultadSalvacionReflejos());
        if (!fallaSalvacion)
        {
          danio *= 0.5f;
        }

        DescargaArcanaImpactoFx.Crear(objetivo);
        DescargaDePoderImpactoFx.Crear(objetivo.transform.position, fallaSalvacion ? 0.16f : 0.14f);
        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
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
  
  
    void VFXAplicar(GameObject objetivo)
    {
       //GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation); 

    }

    //Provisorio
    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();
    private Task CrearProyectilFila(Casilla casillaClick)
    {
        if (casillaClick == null)
        {
            return Task.CompletedTask;
        }

        return LanzarProyectilFilaAsync(casillaClick);
    }

    private async Task LanzarProyectilFilaAsync(Casilla casillaClick)
    {
        await BattleManager.DelayCombateAsync(10);

        int filaY = casillaClick.posY;
        int ladoRef = casillaClick.lado;
        List<Casilla> filaFull = new List<Casilla>();
        foreach (var c in BattleManager.Instance.lCasillasTotal)
        {
            if (c.lado == ladoRef && c.posY == filaY)
            {
                filaFull.Add(c);
            }
        }
        if (filaFull.Count == 0)
        {
            return;
        }

        Casilla startCas = null;
        foreach (var c in filaFull)
        {
            if (c.posX == 3)
            {
                startCas = c;
                break;
            }
        }
        if (startCas == null)
        {
            foreach (var c in filaFull)
            {
                if (startCas == null || c.posX > startCas.posX)
                {
                    startCas = c;
                }
            }
        }

        Casilla endCas = startCas;
        foreach (var c in filaFull)
        {
            if (c.posX < endCas.posX)
            {
                endCas = c;
            }
        }

        Vector3 dir = (endCas.transform.position - startCas.transform.position).normalized;
        if (dir.sqrMagnitude < 0.0001f)
        {
            dir = Vector3.right;
        }

        float offsetBehind = 2.2f;
        Vector3 spawnPos = startCas.transform.position - dir * offsetBehind;

        GameObject vfxPrefab = BattleManager.Instance.contenedorPrefabs.VFXDescargaDePoder_Fila;
        if (vfxPrefab == null)
        {
            return;
        }

        Quaternion rotacion = dir.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(dir) * Quaternion.Euler(0f, -90f, 0f) : Quaternion.Euler(0f, -90f, 0f);
        GameObject vfx = Instantiate(vfxPrefab, spawnPos, rotacion);
        vfx.transform.localScale *= 0.85f;
        FlechaPotenteVuelo vuelo = vfx.GetComponent<FlechaPotenteVuelo>();
        if (vuelo != null)
        {
            vuelo.Configure(dir);
            await vuelo.EsperarFinalAsync();
        }
        else
        {
            await BattleManager.DelayCombateAsync(400);
        }
    }

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
           if(c.Presente.GetComponent<Unidad>() == null && c.Presente.GetComponent<Obstaculo>())
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

public class DescargaDePoderImpactoFx : MonoBehaviour
{
  private const float Duracion = 0.5f;

  private SpriteRenderer spriteRenderer;
  private Vector3 escalaBase;
  private float tiempo;

  private static Sprite spriteImpacto;
  private static Material materialImpacto;

  public static void Crear(Vector3 posicion, float escala)
  {
    Sprite sprite = ObtenerSpriteImpacto();
    if (sprite == null)
    {
      return;
    }

    GameObject go = new GameObject("VFX_DescargaDePoder_Impacto");
    go.transform.position = posicion + Vector3.up * 0.12f;
    if (Camera.main != null)
    {
      go.transform.rotation = Camera.main.transform.rotation;
    }

    DescargaDePoderImpactoFx fx = go.AddComponent<DescargaDePoderImpactoFx>();
    fx.spriteRenderer = go.AddComponent<SpriteRenderer>();
    fx.spriteRenderer.sprite = sprite;
    fx.spriteRenderer.material = ObtenerMaterialImpacto();
    fx.spriteRenderer.sortingOrder = 190;
    fx.spriteRenderer.color = new Color(0.5f, 0.88f, 1.35f, 0.82f);
    fx.escalaBase = Vector3.one * escala;
    go.transform.localScale = fx.escalaBase * 0.58f;
  }

  private void Update()
  {
    tiempo += Time.deltaTime;
    float p = Mathf.Clamp01(tiempo / Duracion);
    float entrada = Mathf.Clamp01(p / 0.12f);
    float salida = 1f - Mathf.Clamp01((p - 0.42f) / 0.58f);
    float alpha = entrada * salida;
    float pulso = 0.92f + Mathf.Sin(tiempo * 24f) * 0.08f;

    transform.localScale = Vector3.Lerp(escalaBase * 0.58f, escalaBase * 0.92f, p) * pulso;
    if (spriteRenderer != null)
    {
      spriteRenderer.color = new Color(0.62f, 0.95f, 1.45f, 0.72f * alpha);
    }

    if (p >= 1f)
    {
      Destroy(gameObject);
    }
  }

  private static Sprite ObtenerSpriteImpacto()
  {
    if (spriteImpacto == null)
    {
      spriteImpacto = Resources.Load<Sprite>("VFX/descargadesintegradora");
    }

    return spriteImpacto;
  }

  private static Material ObtenerMaterialImpacto()
  {
    if (materialImpacto == null)
    {
      Shader shader = Shader.Find("Legacy Shaders/Particles/Additive");
      if (shader == null)
      {
        shader = Shader.Find("Sprites/Default");
      }
      materialImpacto = new Material(shader);
      materialImpacto.hideFlags = HideFlags.HideAndDontSave;
    }

    return materialImpacto;
  }
}











