using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class AtaqueEspadaCortaFilonegro : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano
     ClaseAcechador claseAcechador;
  public override void Awake()
  {


    nombre = "Corte de Espada Corta Filonegro";
    costoAP = 3;
    costoPM = 0;
    Usuario = this.gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    claseAcechador = scEstaUnidad as ClaseAcechador;

    esZonal = false;
    enArea = 0;
    esforzable = 1;
    esCargable = false;
    esMelee = true;
    esHostil = true;
    cooldownMax = 0;
    bAfectaObstaculos = true;

    bonusAtaque = 1;
    XdDanio = 1;
    daniodX = 6; //1d6
    tipoDanio = 2; //Cortante
    criticoRangoHab = 1; //Rango critico de la habilidad, se suma al rango critico del dado



     tipoPorcentaje = 1;



    imHab = Resources.Load<Sprite>("imHab/Acechador_EspadaCorta");

    txtDescripcion = "<color=#5dade2><b>Corte de Espada corta Filonegro</b></color>\n\n";
    txtDescripcion += "<i>Con su mano hábil, el Acechador asesta un golpe con la espada corta.</i>\n\n";
    txtDescripcion += $"<color=#c8c8c8><b>MELEE</b> -Ataque: <color=#ea0606>Fuerza + {bonusAtaque}</color> - Daño: Cortante 1d6+2- +1 Dado Crítico, Daño crítico x2. </color>\n\n";
    txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Valentía: {costoPM} </color>";

    if (TRADU.i.nIdioma == 2) //agrega la traduccion a ingles
    {
            txtDescripcion = "<color=#5dade2><b>Blackblade Short Sword Slash</b></color>\n\n";
            txtDescripcion += "<i>With skilled hand, the Stalker delivers a blow with the short sword.</i>\n\n";
            txtDescripcion += $"<color=#c8c8c8><b>MELEE</b> -Attack: <color=#ea0606>Strength + {bonusAtaque}</color> - Damage: Slashing 1d6+2- +1 Critical Die, Critical damage x2. </color>\n\n";
            txtDescripcion += $"<color=#44d3ec>- Cooldown: {cooldownMax} \n- AP Cost: {costoAP} \n- Valour Cost: {costoPM} </color>";


     }
    else if (TRADU.i.nIdioma == 3)
    {
            nombre = "Corte de Espada Curta Fio Negro";
            txtDescripcion = "<color=#5dade2><b>Corte de Espada Curta Fio Negro</b></color>\n\n";
            txtDescripcion += "<i>Com mao habilidosa, o Acechador desfere um golpe com a espada curta.</i>\n\n";
            txtDescripcion += $"<color=#c8c8c8><b>MELEE</b> -Ataque: <color=#ea0606>Forca + {bonusAtaque}</color> - Dano: Cortante 1d6+2- +1 Dado Critico, Dano critico x2. </color>\n\n";
            txtDescripcion += $"<color=#44d3ec>- Recarga: {cooldownMax} \n- Custo AP: {costoAP} \n- Custo Valentia: {costoPM} </color>";
    }
    ActualizarDescripcion();
  }
    
   void Start()
   {
    if (claseAcechador != null)
    { Invoke("ChequearMaestria", 0.2f); }
   }

   int damExtra;
     public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
      StatsDescripcionUI stats = ObtenerStatsDescripcionUI();
      int criticoMin = Mathf.Clamp(19 - (stats.CriticoRango + criticoRangoHab), 2, 20);
      int criticoPorcentaje = Mathf.Clamp(21 - criticoMin, 0, 20) * 5;
      string rangoDanio = FormatearRangoDados(XdDanio, daniodX, 2 + damExtra);
      string bonusTirada = FormatoModificadorDescripcion(stats.Ataque) + FormatoModificadorDescripcion(bonusAtaque);
      string colorEncabezado = "#44d3ec";
      string colorValor = "#ffffff";
      string colorFuerza = "#d9822b";
      string atributo = esIngles
        ? $"<color={colorFuerza}>Strength ({stats.Fuerza})</color>"
        : esPortugues
          ? $"<color={colorFuerza}>Forca ({stats.Fuerza})</color>"
          : $"<color={colorFuerza}>Fuerza ({stats.Fuerza})</color>";
      int nivelMaestria = claseAcechador != null ? claseAcechador.PASIVA_MaestriaConEspadacorta : 0;
      string titulo = esIngles ? "Blackblade Short Sword Slash" : esPortugues ? "Corte de Espada Curta Fio Negro" : "Corte de Espada Corta Filonegro";
      string subtitulo = esIngles ? "Melee cut with higher critical chance and doubled critical damage." : esPortugues ? "Corte corpo a corpo com mais chance critica e dano critico dobrado." : "Corte melee con mas critico y dano critico duplicado.";
      if (esIngles)
      {
        string fuerza = TerminoDescripcion(TerminoDescripcionId.Fuerza, $"Strength ({stats.Fuerza})");
        string defensa = TerminoDescripcion(TerminoDescripcionId.Defensa, "Defense", "IconoDefensa");
        string danioCortante = TerminoDescripcion(TerminoDescripcionId.DanioCortante, "Slashing damage", "dano_cortante");
        string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crit", "critico");
        string criticoResumen = TerminoDescripcion(TerminoDescripcionId.Critico, "critical");
        var lineas = new List<LineaDescripcionNormalizada>
        {
          LineaDescripcion("Target", ObjetivoMeleeUnitarioIngles),
          LineaDescripcion("Effect", $"On hit, deals {rangoDanio} + {fuerza} as {danioCortante}."),
          LineaDescripcion("Attack Roll", $"1d20 + {fuerza}{bonusTirada} vs {defensa}. Fumble: 5%. {critico}: {criticoPorcentaje}%."),
          LineaDescripcion(TerminoDescripcion(TerminoDescripcionId.Critico, "Critical"), "Deals double damage.")
        };
        if (nivelMaestria > 0) lineas.Add(LineaDescripcion("Passive", $"Short Sword Mastery (Tier {nivelMaestria})."));
        txtDescripcion = ConstruirDescripcionNormalizadaIngles(titulo, $"A short sword attack with enhanced {criticoResumen} strikes.", lineas, mostrarIconoMelee: true);
        return;
      }
      if (esPortugues)
      { string forca=TerminoDescripcion(TerminoDescripcionId.Fuerza,$"Força ({stats.Fuerza})"); string def=TerminoDescripcion(TerminoDescripcionId.Defensa,"Defesa","IconoDefensa"); string cort=TerminoDescripcion(TerminoDescripcionId.DanioCortante,"dano Cortante","dano_cortante"); string crit=TerminoDescripcion(TerminoDescripcionId.Critico,"Crítico","critico"); var l=new List<LineaDescripcionNormalizada>{LineaDescripcion("Alvo","1 alvo ou obstáculo em alcance corpo a corpo"),LineaDescripcion("Efeito",$"Ao acertar, causa {rangoDanio} + {forca} como {cort}."),LineaDescripcion("Rolagem de Ataque",$"1d20 + {forca}{bonusTirada} vs {def}. Falha crítica: 5%. {crit}: {criticoPorcentaje}%."),LineaDescripcion(TerminoDescripcion(TerminoDescripcionId.Critico,"Acerto crítico"),"Causa dano dobrado.")}; if(nivelMaestria>0)l.Add(LineaDescripcion("Passiva",$"Maestria com Espada Curta (Nível {nivelMaestria}).")); txtDescripcion=ConstruirDescripcionNormalizadaLocalizada(titulo,"Um ataque com espada curta com acertos críticos aprimorados.",l,mostrarIconoMelee:true); return; }
      { string fuerza=TerminoDescripcion(TerminoDescripcionId.Fuerza,$"Fuerza ({stats.Fuerza})"); string def=TerminoDescripcion(TerminoDescripcionId.Defensa,"Defensa","IconoDefensa"); string cort=TerminoDescripcion(TerminoDescripcionId.DanioCortante,"daño Cortante","dano_cortante"); string crit=TerminoDescripcion(TerminoDescripcionId.Critico,"Crítico","critico"); var l=new List<LineaDescripcionNormalizada>{LineaDescripcion("Objetivo","1 objetivo u obstáculo en alcance cuerpo a cuerpo"),LineaDescripcion("Efecto",$"Al impactar, inflige {rangoDanio} + {fuerza} como {cort}."),LineaDescripcion("Tirada de Ataque",$"1d20 + {fuerza}{bonusTirada} vs {def}. Pifia: 5%. {crit}: {criticoPorcentaje}%."),LineaDescripcion(TerminoDescripcion(TerminoDescripcionId.Critico,"Impacto crítico"),"Inflige daño doble.")}; if(nivelMaestria>0)l.Add(LineaDescripcion("Pasiva",$"Maestría con Espada Corta (Nivel {nivelMaestria}).")); txtDescripcion=ConstruirDescripcionNormalizadaLocalizada(titulo,"Un ataque con espada corta con impactos críticos mejorados.",l,mostrarIconoMelee:true); return; }
      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Melee attack</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Target:</b></color> <color={colorValor}>1 enemy or obstacle in frontal melee range</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Roll:</b></color> <color={colorValor}>1d20 + {atributo}{bonusTirada} vs Defense. Fumble: 5%. Crit: {criticoPorcentaje}%</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Damage:</b></color> <color={colorValor}>{rangoDanio} + {atributo}. Type: Slashing</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Critical:</b></color> <color={colorValor}>x2 damage</color>";
        if (nivelMaestria > 0) { cuerpo += $"\n<color={colorEncabezado}><b>Passive:</b></color> <color={colorValor}>Short Sword Mastery Tier {nivelMaestria}</color>"; }
      }
      else if (esPortugues)
      {
        cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Ataque corpo a corpo</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Alvo:</b></color> <color={colorValor}>1 inimigo ou obstaculo no alcance frontal corpo a corpo</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Rolagem:</b></color> <color={colorValor}>1d20 + {atributo}{bonusTirada} vs Defesa. Falha critica: 5%. Critico: {criticoPorcentaje}%</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Dano:</b></color> <color={colorValor}>{rangoDanio} + {atributo}. Tipo: Cortante</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Critico:</b></color> <color={colorValor}>x2 dano</color>";
        if (nivelMaestria > 0) { cuerpo += $"\n<color={colorEncabezado}><b>Passiva:</b></color> <color={colorValor}>Maestria com Espada Curta Tier {nivelMaestria}</color>"; }
      }
      else
      {
        cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Ataque melee</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Objetivo:</b></color> <color={colorValor}>1 enemigo u obstaculo en alcance melee frontal</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Tirada:</b></color> <color={colorValor}>1d20 + {atributo}{bonusTirada} vs Defensa. Pifia: 5%. Critico: {criticoPorcentaje}%</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Dano:</b></color> <color={colorValor}>{rangoDanio} + {atributo}. Tipo: Cortante</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Critico:</b></color> <color={colorValor}>x2 dano</color>";
        if (nivelMaestria > 0) { cuerpo += $"\n<color={colorEncabezado}><b>Pasiva:</b></color> <color={colorValor}>Maestria con Espada Corta Tier {nivelMaestria}</color>"; }
      }

      txtDescripcion = ConstruirDescripcionTooltipNueva(titulo, subtitulo, cuerpo);
    }
    Casilla Origen;
    public override void Activar()
    {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

        
    }
    
    
    void ChequearMaestria()
  {
    int NivelMaestria = claseAcechador.PASIVA_MaestriaConEspadacorta;

    if (NivelMaestria == 1)
    {
      bonusAtaque = 1;
      damExtra += 2;
      if (TRADU.i.nIdioma == 3)
      { txtDescripcion += "\n\n<i>Maestria com Espada Curta adiciona: +1 Ataque +2 Dano.</i>\n\n"; }
      else
      { txtDescripcion += "\n\n<i>Maestría con Espada Corta agrega: +1 Ataque +2 Daño.</i>\n\n"; }

    }
    else if (NivelMaestria == 2)
    {
      bonusAtaque = 1;
      damExtra += 2;
      criticoRangoHab = 1;
      if (TRADU.i.nIdioma == 3)
      { txtDescripcion += "\n\n<i>Maestria com Espada Curta adiciona: +1 Ataque +2 Dano +5% Critico.</i>\n\n"; }
      else
      { txtDescripcion += "\n\n<i>Maestría con Espada Corta agrega: +1 Ataque +2 Daño +5% Critico.</i>\n\n"; }

    }
    else if (NivelMaestria == 3)
    {
      bonusAtaque = 1;
      damExtra += 2;
      criticoRangoHab = 1;
      costoAP -= 1; //costo AP -1
      if (TRADU.i.nIdioma == 3)
      { txtDescripcion += "\n\n<i>Maestria com Espada Curta adiciona: +1 Ataque +2 Dano +5% Critico, -1 AP.</i>\n\n"; }
      else
      { txtDescripcion += "\n\n<i>Maestría con Espada Corta agrega: +1 Ataque +2 Daño +5% Critico, -1 AP.</i>\n\n"; }


    }
    else if (NivelMaestria == 4)
    {
      bonusAtaque = 1;
      damExtra += 4;
      criticoRangoHab = 2;
      costoAP -= 1; //costo AP -1
      if (TRADU.i.nIdioma == 3)
      { txtDescripcion += "\n\n<i>Maestria com Espada Curta adiciona: +1 Ataque +4 Dano +10% Critico.</i>\n\n"; }
      else
      { txtDescripcion += "\n\n<i>Maestría con Espada Corta agrega: +1 Ataque +4 Daño +10% Critico.</i>\n\n"; }

    }
    else if (NivelMaestria == 5)
    {
      bonusAtaque = 2;
      damExtra += 4;
      criticoRangoHab = 1;
      costoAP -= 1; //costo AP -1
      if (TRADU.i.nIdioma == 3)
      { txtDescripcion += "\n\n<i>Maestria com Espada Curta adiciona: +2 Ataque +4 Dano +5% Critico.</i>\n\n"; }
      else
      { txtDescripcion += "\n\n<i>Maestría con Espada Corta agrega: +2 Ataque +4 Daño +5% Critico.</i>\n\n"; }

    }
      ActualizarDescripcion();
  }
    

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
  {

    if (obj is Unidad) //Acá van los efectos a Unidades.
    {
      Unidad objetivo = (Unidad)obj;
      float defensaObjetivo = objetivo.ObtenerdefensaActual();
      print("Defensa: " + defensaObjetivo);

      int danioMarca = 0;


      float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab;

      int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, scEstaUnidad.mod_CarFuerza, bonusAtaque, criticoRango, objetivo, 0);
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
        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 2 + damExtra + scEstaUnidad.mod_CarFuerza + danioMarca;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);
        danio -= danio / 2; //Reduce 50% por roce


        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

      }
      else if (resultadoTirada == 2)
      { //GOLPE
        print("Golpe");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 2 + damExtra + scEstaUnidad.mod_CarFuerza + danioMarca;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

      

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
        


      }
      else if (resultadoTirada == 3)
      { //CRITICO
        print("Critico");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 2 + damExtra + scEstaUnidad.mod_CarFuerza + danioMarca;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);
        danio *= 2; //Multiplica por  el daño crítico

        objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);

      }

      objetivo.AplicarDebuffPorAtaquesreiterados(1);
    }
    else if (obj is Obstaculo) //Acá van los efectos a Obstaculos
    {
      Obstaculo objetivo = (Obstaculo)obj;
      //---


      float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 2 + damExtra + scEstaUnidad.mod_CarFuerza;
      danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

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
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasRango(1+rangoPlus,1);
    
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

