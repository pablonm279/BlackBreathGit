using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class PrimerosAuxilios : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] int usosBatalla;
    private const int CuracionBase = 5;
  
    
     public override void  Awake()
    {
      nombre = "Primeros Auxilios";
      IDenClase = 4;
      costoAP = 0;
      costoPM = 0;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 0;
      bAfectaObstaculos = false;
      
     
      usosBatalla = 2;

      imHab = Resources.Load<Sprite>("imHab/Caballero_PrimerosAuxilios");
      ActualizarDescripcion();
    }
    public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;

      string dadoCuracion = NIVEL == 1 ? "1d8" : "1d12";
      string rangoCuracion = NIVEL == 1 ? $"{CuracionBase} + 1-8 por AP" : $"{CuracionBase} + 1-12 por AP";
      string rangoCuracionEn = NIVEL == 1 ? $"{CuracionBase} + 1-8 per AP" : $"{CuracionBase} + 1-12 per AP";
      string rangoCuracionPt = NIVEL == 1 ? $"{CuracionBase} + 1-8 por AP" : $"{CuracionBase} + 1-12 por AP";
      string curacionPorAP = NIVEL == 1 ? "1-8" : "1-12";
      int usos = NIVEL > 2 ? 3 : 2;
      bool trasladaCampania = NIVEL == 5;
      string colorEncabezado = "#44d3ec";
      string colorValor = "#ffffff";
      string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
      string costoSuperior = $"{costoAP} {iconoAP}";

      string tituloEs = "Primeros Auxilios I";
      string tituloEn = "First Aid I";
      string tituloPt = "Primeiros Socorros I";
      if (NIVEL == 2) { tituloEs = "Primeros Auxilios II"; tituloEn = "First Aid II"; }
      if (NIVEL == 3) { tituloEs = "Primeros Auxilios III"; tituloEn = "First Aid III"; }
      if (NIVEL == 4) { tituloEs = "Primeros Auxilios IV a"; tituloEn = "First Aid IV a"; }
      if (NIVEL == 5) { tituloEs = "Primeros Auxilios IV b"; tituloEn = "First Aid IV b"; }
      if (NIVEL == 2) { tituloPt = "Primeiros Socorros II"; }
      if (NIVEL == 3) { tituloPt = "Primeiros Socorros III"; }
      if (NIVEL == 4) { tituloPt = "Primeiros Socorros IV a"; }
      if (NIVEL == 5) { tituloPt = "Primeiros Socorros IV b"; }

      if (esIngles)
      {
        string ap = TerminoDescripcion(TerminoDescripcionId.PuntosAccion, "AP", "ap");
        string curacion = TerminoDescripcion(TerminoDescripcionId.Curacion, "HP");
        string sangrado = TerminoDescripcion(TerminoDescripcionId.Sangrado, "Bleed", "Estado_sangrano");
        string veneno = TerminoDescripcion(TerminoDescripcionId.Veneno, "Poison");
        string proximaMejora = null;
        if (DebeMostrarProximaMejoraDescripcion())
        {
          if (NIVEL < 2) { proximaMejora = "Healing per AP improves from 1-8 to 1-12."; }
          else if (NIVEL == 2) { proximaMejora = "+1 use per battle."; }
          else if (NIVEL == 3) { proximaMejora = "Option A: +4 base healing.\nOption B: enables its campaign transfer effect."; }
        }

        var lineas = new List<LineaDescripcionNormalizada>
        {
          LineaDescripcion("Target", "Self or 1 ally"),
          LineaDescripcion("Effect", $"Restores {CuracionBase} + ({curacionPorAP} × current {ap}) {curacion}; consumes all current AP."),
          LineaDescripcion("Removes", $"{sangrado} and {veneno}.", 1),
          LineaDescripcion("Uses", $"{usos} per battle")
        };
        if (trasladaCampania)
        {
          lineas.Add(LineaDescripcion("Campaign", "Enables its campaign transfer effect."));
        }

        txtDescripcion = ConstruirDescripcionNormalizadaIngles(
          tituloEn,
          "Treats one ally, scaling with current AP.",
          lineas,
          proximaMejora);
        return;
      }

      if (esPortugues)
      {
        string ap = TerminoDescripcion(TerminoDescripcionId.PuntosAccion, "AP", "ap");
        string cura = TerminoDescripcion(TerminoDescripcionId.Curacion, "Cura");
        string sangramento = TerminoDescripcion(TerminoDescripcionId.Sangrado, "Sangramento", "sangrado");
        string veneno = TerminoDescripcion(TerminoDescripcionId.Veneno, "Veneno", "veneno");
        string proximaMejora = null;
        if (DebeMostrarProximaMejoraDescripcion())
        {
          if (NIVEL < 2) { proximaMejora = "Próximo nível: a cura por AP melhora de 1-8 para 1-12."; }
          else if (NIVEL == 2) { proximaMejora = "Próximo nível: +1 uso por batalha."; }
          else if (NIVEL == 3) { proximaMejora = "Opção A: +4 de cura base.\nOpção B: habilita seu efeito de transferência na campanha."; }
        }
        var lineas = new List<LineaDescripcionNormalizada>
        {
          LineaDescripcion("Alvo", "O próprio usuário ou 1 aliado"),
          LineaDescripcion("Efeito", $"Restaura {CuracionBase} + ({curacionPorAP} × {ap} atual) de {cura}; consome todo o AP atual."),
          LineaDescripcion("Remove", $"{sangramento} e {veneno}.", 1),
          LineaDescripcion("Usos", $"{usos} por batalha")
        };
        if (trasladaCampania) { lineas.Add(LineaDescripcion("Campanha", "Habilita seu efeito de transferência na campanha.")); }
        txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(tituloPt, "Trata um aliado, escalando com o AP atual.", lineas, proximaMejora);
        return;
      }

      {
        string ap = TerminoDescripcion(TerminoDescripcionId.PuntosAccion, "AP", "ap");
        string curacion = TerminoDescripcion(TerminoDescripcionId.Curacion, "Curación");
        string sangrado = TerminoDescripcion(TerminoDescripcionId.Sangrado, "Sangrado", "sangrado");
        string veneno = TerminoDescripcion(TerminoDescripcionId.Veneno, "Veneno", "veneno");
        string proximaMejora = null;
        if (DebeMostrarProximaMejoraDescripcion())
        {
          if (NIVEL < 2) { proximaMejora = "Próximo nivel: la curación por AP mejora de 1-8 a 1-12."; }
          else if (NIVEL == 2) { proximaMejora = "Próximo nivel: +1 uso por batalla."; }
          else if (NIVEL == 3) { proximaMejora = "Opción A: +4 de curación base.\nOpción B: habilita su efecto de transferencia en campaña."; }
        }
        var lineas = new List<LineaDescripcionNormalizada>
        {
          LineaDescripcion("Objetivo", "Uno mismo o 1 aliado"),
          LineaDescripcion("Efecto", $"Restaura {CuracionBase} + ({curacionPorAP} × {ap} actual) de {curacion}; consume todo el AP actual."),
          LineaDescripcion("Elimina", $"{sangrado} y {veneno}.", 1),
          LineaDescripcion("Usos", $"{usos} por batalla")
        };
        if (trasladaCampania) { lineas.Add(LineaDescripcion("Campaña", "Habilita su efecto de transferencia en campaña.")); }
        txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(tituloEs, "Trata a un aliado y escala con el AP actual.", lineas, proximaMejora);
        return;
      }

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += "<b>Type:</b> Heal\n";
        cuerpo += "<b>Target:</b> Self or ally at range 1\n";
        cuerpo += $"<b>Healing:</b> {CuracionBase} + ({dadoCuracion} x current AP)\n";
        cuerpo += "<b>Additional:</b> Removes Bleed and Poison\n";
        cuerpo += "<b>On cast:</b> spends all current AP";
        if (trasladaCampania)
        {
          cuerpo += "\n<b>Campaign:</b> this upgrade enables campaign transfer effect";
        }
      }
      else if (esPortugues)
      {
        cuerpo += "<b>Tipo:</b> Cura\n";
        cuerpo += "<b>Alvo:</b> O proprio usuario ou aliado em alcance 1\n";
        cuerpo += $"<b>Cura:</b> {CuracionBase} + ({dadoCuracion} x AP atuais)\n";
        cuerpo += "<b>Adicional:</b> remove Sangramento e Veneno\n";
        cuerpo += "<b>Ao usar:</b> consome todo AP atual";
        if (trasladaCampania)
        {
          cuerpo += "\n<b>Campanha:</b> esta melhoria habilita o efeito de traslado para campanha";
        }
      }
      else
      {
        cuerpo += "<b>Tipo:</b> Curación\n";
        cuerpo += "<b>Objetivo:</b> Uno mismo o aliado a rango 1\n";
        cuerpo += $"<b>Curación:</b> {CuracionBase} + ({dadoCuracion} x AP actuales)\n";
        cuerpo += "<b>Adicional:</b> remueve Sangrado y Veneno\n";
        cuerpo += "<b>Al lanzarla:</b> consume todos los AP actuales";
        if (trasladaCampania)
        {
          cuerpo += "\n<b>Campania:</b> esta mejora habilita el efecto de traslado a campania";
        }
      }

      string costos = esIngles
        ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}\n- Uses per battle: {usos}"
        : esPortugues
          ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}\n- Usos por batalha: {usos}"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}\n- Usos por batalla: {usos}";

      txtDescripcion = ConstruirDescripcionEstandar(
        esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
        esIngles
          ? "Field treatment that scales with the AP you are willing to spend right now."
          : esPortugues
            ? "Tratamento de campo que escala com o AP que voce decidir gastar agora."
          : "Atencion de campo que escala segun los AP que decidas gastar en ese momento.",
        cuerpo,
        costos,
        "#5dade2");

      string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
      string subtitulo = esIngles
        ? "Heal an ally at close range; consumes all current AP."
        : esPortugues
          ? "Cura um aliado próximo; consome todo o AP atual."
          : "Cura a un aliado cercano; consume todos los AP actuales.";

      string cuerpoFormato = "";
      if (esIngles)
      {
        cuerpoFormato += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Healing support</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Target:</b></color> <color={colorValor}>Self or ally at range 1</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Healing:</b></color> <color={colorValor}>{rangoCuracionEn}; spends all current AP</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Removes:</b></color> <color={colorValor}>Bleed and Poison</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Uses:</b></color> <color={colorValor}>{usos} per battle</color>";
        if (trasladaCampania)
        {
          cuerpoFormato += $"\n<color={colorEncabezado}><b>Campaign:</b></color> <color={colorValor}>Enables campaign transfer effect</color>";
        }
      }
      else if (esPortugues)
      {
        cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Suporte de cura</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Alvo:</b></color> <color={colorValor}>O próprio usuário ou aliado em alcance 1</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Cura:</b></color> <color={colorValor}>{rangoCuracionPt}; consome todo o AP atual</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Remove:</b></color> <color={colorValor}>Sangramento e Veneno</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Usos:</b></color> <color={colorValor}>{usos} por batalha</color>";
        if (trasladaCampania)
        {
          cuerpoFormato += $"\n<color={colorEncabezado}><b>Campanha:</b></color> <color={colorValor}>Habilita o efeito de traslado para campanha</color>";
        }
      }
      else
      {
        cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Soporte de curación</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Objetivo:</b></color> <color={colorValor}>Uno mismo o aliado a rango 1</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Curación:</b></color> <color={colorValor}>{rangoCuracion}; consume todos los AP actuales</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Remueve:</b></color> <color={colorValor}>Sangrado y Veneno</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Usos:</b></color> <color={colorValor}>{usos} por batalla</color>";
        if (trasladaCampania)
        {
          cuerpoFormato += $"\n<color={colorEncabezado}><b>Campaña:</b></color> <color={colorValor}>Habilita el efecto de traslado a campaña</color>";
        }
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
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: healing die changes from 1d4 to 1d6 per AP.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 use per battle.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+4 base healing) or Option B (campaign transfer upgrade).</color>"; }
      }
      else if (esPortugues)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: o dado de cura passa de 1d4 para 1d6 por AP.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 uso por batalha.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+4 cura base) ou Opcao B (melhoria de traslado para campanha).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: el dado de curación pasa de 1d4 a 1d6 por AP.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 uso por batalla.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A (+4 curación base) u Opción B (mejora de traslado a campania).</color>"; }
      }
    }

    void Start()
    {
        if(NIVEL > 2)
        {
          usosBatalla++;
        }

    }

    Casilla Origen;
    public override void Activar()
    {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

        BattleManager.Instance.scUIContadorAP.MarcarCirculos((int)scEstaUnidad.ObtenerAPActual());

        
    }
    
    

    public async override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {
    
     if(obj is Unidad) //Acá van los efectos a Unidades.
     {
        
       Unidad objetivo = (Unidad)obj;
        VFXAplicar(objetivo.gameObject);
       BattleManager.Instance.EscribirLog(TRADU.i.Traducir(scEstaUnidad.uNombre) + " " + TRADU.i.Traducir("usa ") + TRADU.i.Traducir(nombre) + " -> " + TRADU.i.Traducir(objetivo.uNombre) + ".");

       int APusados = (int)scEstaUnidad.ObtenerAPActual();

       float curacion = CuracionBase;

      if(NIVEL == 1) //Cura 1d4 por AP
      {
        for (int veces = 0; veces < APusados; veces++)
        {
            curacion += UnityEngine.Random.Range(1,9);

        }
      }
      else //Cura 1d6 por AP
      {
        for (int veces = 0; veces < APusados; veces++)
        {
            curacion += UnityEngine.Random.Range(1,13);

        }

      }

      scEstaUnidad.EstablecerAPActualA(0);
       
     
       if(objetivo.estado_sangrado > 0)
       {
         objetivo.estado_sangrado = 0;
         await objetivo.GenerarTextoFlotante("<s>" + TRADU.i.Traducir("Sangrado") + "</s>", Color.red);
       }
       if(objetivo.estado_veneno > 0)
       {
         objetivo.estado_veneno = 0;
         await objetivo.GenerarTextoFlotante("<s>" + TRADU.i.Traducir("Veneno") + "</s>", Color.green);
       }
        objetivo.RecibirCuracion(curacion, false);

       objetivo.Marcar(0);

       usosBatalla--;
       if(usosBatalla == 0)//Al gastarse los usos, se borra la habilidad
       {
        Destroy(this); 
        BattleManager.Instance.scUIBotonesHab.UIDesactivarHabilidades();
        BattleManager.Instance.scUIBotonesHab.ActualizarBotonesHabilidad();
       } 
      
     }   
   
    }
    void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_PrimerosAuxilios");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
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
      
      lObjetivosPosibles.Clear();
     
      
      //Casillas Alrededor al origen
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasAlrededor(1);
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






