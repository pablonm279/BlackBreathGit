using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class CorteVerticalSediento : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano
      public override void  Awake()
    {
      nombre = "Corte Vertical Sediento";
      costoAP = 3;
      costoPM = 0;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 1;
      esCargable = false;
      esMelee = true;
      esHostil = true;
      cooldownMax = 0;
      bAfectaObstaculos = true;

      bonusAtaque = 1;
      XdDanio = 2;
      daniodX = 8; //2d8
      tipoDanio = 2; //Cortante
      criticoRangoHab = 0;



       tipoPorcentaje = 1;




      imHab = Resources.Load<Sprite>("imHab/Caballero_corteVertical");

      if (TRADU.i.nIdioma == 1)
      {
        txtDescripcion = "<color=#5dade2><b>Corte Vertical Sediento</b></color>\n\n"; 
        txtDescripcion += "<i>Con el mandoble, el Caballero efectúa un ataque de arriba hacia abajo, lento, pero capaz de provocar grandes daños.</i>\n\n";
        txtDescripcion += "<i>+1 Ataque y +1 Rango Crítico si el objetivo tiene 50% de vida o menos y no es Constructo.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>MELEE</b> -Ataque: <color=#ea0606>Fuerza +{bonusAtaque}</color> - Daño: Cortante 2d8- </color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Valentía: {costoPM} </color>";
      }
      if (TRADU.i.nIdioma == 2) //Inglés
      {
        txtDescripcion = "<color=#5dade2><b>Thirsty Vertical Slash</b></color>\n\n";
        txtDescripcion += "<i>With the greatsword, the Knight performs a slow downward attack, capable of inflicting great damage.</i>\n\n";
        txtDescripcion += "<i>+1 Attack and +1 Critical Range if the target has 50% health or less and is not a Construct.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>MELEE</b> -Attack: <color=#ea0606>Strength +{bonusAtaque}</color> - Damage: Slashing 2d8- </color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Cooldown: {cooldownMax} \n- AP Cost: {costoAP} \n- Valour Cost: {costoPM} </color>";
      }
      if (TRADU.i.nIdioma == 3)
      {
        txtDescripcion = "<color=#5dade2><b>Corte Vertical Sedento</b></color>\n\n";
        txtDescripcion += "<i>Com o montante, o Cavaleiro executa um ataque de cima para baixo, lento, mas capaz de causar grandes danos.</i>\n\n";
        txtDescripcion += "<i>+1 Ataque e +1 Alcance Critico se o alvo tiver 50% de vida ou menos e nao for Construto.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>MELEE</b> -Ataque: <color=#ea0606>Forca +{bonusAtaque}</color> - Dano: Cortante 2d8- </color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Recarga: {cooldownMax} \n- Custo AP: {costoAP} \n- Custo Valentia: {costoPM} </color>";
      }
      ActualizarDescripcion();
    }
     public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
      StatsDescripcionUI stats = ObtenerStatsDescripcionUI();
      int criticoMin = Mathf.Clamp(19 - (stats.CriticoRango + criticoRangoHab), 2, 20);
      int criticoPorcentaje = Mathf.Clamp(21 - criticoMin, 0, 20) * 5;
      string rangoDanio = FormatearRangoDados(XdDanio, daniodX);
      string bonusTirada = FormatoModificadorDescripcion(stats.Ataque) + FormatoModificadorDescripcion(bonusAtaque);
      string colorEncabezado = "#44d3ec";
      string colorValor = "#ffffff";
      string colorFuerza = "#d9822b";
      string atributo = esIngles
        ? $"<color={colorFuerza}>Strength ({stats.Fuerza})</color>"
        : esPortugues
          ? $"<color={colorFuerza}>Forca ({stats.Fuerza})</color>"
          : $"<color={colorFuerza}>Fuerza ({stats.Fuerza})</color>";
      string titulo = esIngles ? "Thirsty Vertical Slash" : esPortugues ? "Corte Vertical Sedento" : "Corte Vertical Sediento";
      string subtitulo = esIngles ? "Heavy melee attack; stronger against wounded targets." : esPortugues ? "Ataque corpo a corpo pesado; melhora contra alvos feridos." : "Ataque melee pesado; mejora contra objetivos heridos.";
      string efecto = esIngles ? "If target has 50% HP or less and is not Construct: +1 attack, +5% Crit" : esPortugues ? "Se o alvo tem 50% HP ou menos e nao e Construto: +1 ataque, +5% Critico" : "Si el objetivo tiene 50% HP o menos y no es Constructo: +1 ataque, +5% Critico";
      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Melee attack</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Target:</b></color> <color={colorValor}>1 enemy or obstacle in frontal melee range</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Roll:</b></color> <color={colorValor}>1d20 + {atributo}{bonusTirada} vs Defense. Fumble: 10%. Crit: {criticoPorcentaje}%</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Damage:</b></color> <color={colorValor}>{rangoDanio} + {atributo}. Type: Slashing</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Weapon effect:</b></color> <color={colorValor}>{efecto}</color>";
      }
      else if (esPortugues)
      {
        cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Ataque corpo a corpo</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Alvo:</b></color> <color={colorValor}>1 inimigo ou obstaculo no alcance frontal corpo a corpo</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Rolagem:</b></color> <color={colorValor}>1d20 + {atributo}{bonusTirada} vs Defesa. Falha critica: 10%. Critico: {criticoPorcentaje}%</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Dano:</b></color> <color={colorValor}>{rangoDanio} + {atributo}. Tipo: Cortante</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Efeito da arma:</b></color> <color={colorValor}>{efecto}</color>";
      }
      else
      {
        cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Ataque melee</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Objetivo:</b></color> <color={colorValor}>1 enemigo u obstaculo en alcance melee frontal</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Tirada:</b></color> <color={colorValor}>1d20 + {atributo}{bonusTirada} vs Defensa. Pifia: 10%. Critico: {criticoPorcentaje}%</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Dano:</b></color> <color={colorValor}>{rangoDanio} + {atributo}. Tipo: Cortante</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Efecto del arma:</b></color> <color={colorValor}>{efecto}</color>";
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
    
    

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {
    
     if(obj is Unidad) //Acá van los efectos a Unidades.
     {
       Unidad objetivo = (Unidad)obj;
       float defensaObjetivo = objetivo.ObtenerdefensaActual();
       print("Defensa: "+ defensaObjetivo);
       
       int danioMarca = 0;


       float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab;
       int bonusAtaqueTotal = bonusAtaque;
       if(ChequearTieneSiguesTu(objetivo))
       {
         bonusAtaqueTotal += 5;
         danioMarca = 8;
         Destroy(objetivo.GetComponent<MarcaSiguesTu>());

         if(gameObject.GetComponent<SiguesTu>().NIVEL > 1)
         { criticoRango +=2;    }
         if(gameObject.GetComponent<SiguesTu>().NIVEL > 2)
         { danioMarca +=2;    }
        
       }

      if (!objetivo.TieneTag("Constructo"))
      { 
        if (objetivo.PorcentajeVidaActual() <= 50)
        {
          bonusAtaqueTotal += 1;
          criticoRango += 1; 
        }


      }



       int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, scEstaUnidad.mod_CarFuerza, bonusAtaqueTotal, criticoRango, objetivo, 1); // En habilidades caballero +1 a pifia, debilidad de Caballero
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
        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + scEstaUnidad.mod_CarFuerza + danioMarca;

        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        danio -= danio / 2; //Reduce 50% por roce

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
        VFXAplicar(objetivo.gameObject);


      }
      else if (resultadoTirada == 2)
      {//GOLPE
        print("Golpe");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + scEstaUnidad.mod_CarFuerza + danioMarca;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
        VFXAplicar(objetivo.gameObject);


      }
      else if (resultadoTirada == 3)
      {//CRITICO
        print("Critico");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + scEstaUnidad.mod_CarFuerza + danioMarca;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);
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
    
    void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_CorteVertical");
      if (VFXenObjetivo == null || objetivo == null) { return; }

      GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
      vfx.transform.parent = objetivo.transform;

      Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
      RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, objetivo.transform, 500);
    }
    bool ChequearTieneSiguesTu(Unidad obj)
    { 
      if(obj.GetComponent<MarcaSiguesTu>() != null)
      {
        if(obj.GetComponent<MarcaSiguesTu>().quienMarco == scEstaUnidad)
        {
          return true;
        }
      
      }
      return false;
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




