using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;


public class TiroconArcoRalentizante : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7cido - 8: Arcano

    private int hAlcance = 7;
    private int hAncho = 2; //1 - adyancentes tam
  public override void Awake()
  {
    nombre = "Tiro con Arco Ralentizante";
    costoAP = 3;
    costoPM = 0;
    Usuario = this.gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    esZonal = false;
    enArea = 0;
    esforzable = 1;
    esCargable = false;
    esMelee = false;
    esHostil = true;
    cooldownMax = 0;
    bAfectaObstaculos = true;

    bonusAtaque = 0;
    XdDanio = 1;
    daniodX = 10; //1d10
    tipoDanio = 1; //Perforante
    criticoRangoHab = 0;

    requiereRecurso = 1; //esto es para que el boton no se active al apretar si no tiene X recursos (ej Flecha). Ver en BotonHabilidad.






      tipoPorcentaje = 2;



    imHab = Resources.Load<Sprite>("imHab/Explorador_Tiroconarco");

    txtDescripcion = "<color=#5dade2><b>Tiro con Arco Ralentizante</b></color>\n\n";
    txtDescripcion += "<i>El explorador ataca con su arco al enemigo.</i>\n\n";
    txtDescripcion += "<i>Al golpear TS Fortaleza DC 11: si falla, Ralentiza 2 turnos.</i>\n\n";
    txtDescripcion += $"<color=#c8c8c8><b>Alcance: 7</b> -Ataque: <color=#ea0606>Agilidad +{bonusAtaque}</color> - Daño: Perforante 1d10+1- Requiere 1 Flecha</color>\n\n";
    txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Valentía: {costoPM} </color>";
     if (TRADU.i.nIdioma == 2)
    {
      txtDescripcion = "<color=#5dade2><b>Slowing Bow Shot</b></color>\n\n";
      txtDescripcion += "<i>The explorer attacks the enemy with his bow.</i>\n\n";
      txtDescripcion += "<i>On hit, Fortitude Save DC 11: if failed, Slowed for 2 turns.</i>\n\n";
      txtDescripcion += $"<color=#c8c8c8><b>Range: 7</b> -Attack: <color=#ea0606>Agility +{bonusAtaque}</color> - Damage: Piercing 1d10+1- Requires 1 Arrow</color>\n\n";
      txtDescripcion += $"<color=#44d3ec>- Cooldown: {cooldownMax} \n- AP Cost: {costoAP} \n- Valour Cost: {costoPM} </color>";
    }
    else if (TRADU.i.nIdioma == 3)
    {
      nombre = "Tiro com Arco Ralentizante";
      txtDescripcion = "<color=#5dade2><b>Tiro com Arco Ralentizante</b></color>\n\n";
      txtDescripcion += "<i>O explorador ataca o inimigo com seu arco.</i>\n\n";
      txtDescripcion += "<i>Ao acertar, TS Fortaleza CD 11: se falhar, fica Lento por 2 turnos.</i>\n\n";
      txtDescripcion += $"<color=#c8c8c8><b>Alcance: 7</b> -Ataque: <color=#ea0606>Agilidade +{bonusAtaque}</color> - Dano: Perfurante 1d10+1- Requer 1 Flecha</color>\n\n";
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
      string rangoDanio = FormatearRangoDados(XdDanio, daniodX, 1);
      string bonusTirada = FormatoModificadorDescripcion(stats.Ataque) + FormatoModificadorDescripcion(bonusAtaque);
      string colorEncabezado = "#44d3ec";
      string colorValor = "#ffffff";
      string colorAgilidad = "#7fa35a";
      string atributo = esIngles
        ? $"<color={colorAgilidad}>Agility ({stats.Agilidad})</color>"
        : esPortugues
          ? $"<color={colorAgilidad}>Agilidade ({stats.Agilidad})</color>"
          : $"<color={colorAgilidad}>Agilidad ({stats.Agilidad})</color>";
      string titulo = esIngles ? "Slowing Bow Shot" : esPortugues ? "Tiro com Arco Ralentizante" : "Tiro con Arco Ralentizante";
      string subtitulo = esIngles ? "Bow shot that can slow the target." : esPortugues ? "Disparo de arco que pode lentificar o alvo." : "Disparo de arco que puede ralentizar al objetivo.";
      string efecto = esIngles ? "On hit: Fortitude save vs DC 11; on failed save, Slowed for 2 turns" : esPortugues ? "Ao acertar: resistencia Fortitude vs CD 11; se falhar, Lento por 2 turnos" : "Al impactar: TS Fortaleza DC 11; si falla, Ralentizado 2 turnos";
      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Ranged attack</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Range:</b></color> <color={colorValor}>{hAlcance}</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Target:</b></color> <color={colorValor}>1 enemy or obstacle in range</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Roll:</b></color> <color={colorValor}>1d20 + {atributo}{bonusTirada} vs Defense. Fumble: 5%. Crit: {criticoPorcentaje}%</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Damage:</b></color> <color={colorValor}>{rangoDanio} + {atributo}. Type: Piercing</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Cost:</b></color> <color={colorValor}>1 Arrow</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Weapon effect:</b></color> <color={colorValor}>{efecto}</color>";
      }
      else if (esPortugues)
      {
        cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Ataque a distancia</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Alcance:</b></color> <color={colorValor}>{hAlcance}</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Alvo:</b></color> <color={colorValor}>1 inimigo ou obstaculo no alcance</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Rolagem:</b></color> <color={colorValor}>1d20 + {atributo}{bonusTirada} vs Defesa. Falha critica: 5%. Critico: {criticoPorcentaje}%</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Dano:</b></color> <color={colorValor}>{rangoDanio} + {atributo}. Tipo: Perfurante</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Custo:</b></color> <color={colorValor}>1 Flecha</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Efeito da arma:</b></color> <color={colorValor}>{efecto}</color>";
      }
      else
      {
        cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Ataque a distancia</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Alcance:</b></color> <color={colorValor}>{hAlcance}</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Objetivo:</b></color> <color={colorValor}>1 enemigo u obstaculo en alcance</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Tirada:</b></color> <color={colorValor}>1d20 + {atributo}{bonusTirada} vs Defensa. Pifia: 5%. Critico: {criticoPorcentaje}%</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Dano:</b></color> <color={colorValor}>{rangoDanio} + {atributo}. Tipo: Perforante</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Costo:</b></color> <color={colorValor}>1 Flecha</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Efecto del arma:</b></color> <color={colorValor}>{efecto}</color>";
      }

      txtDescripcion = ConstruirDescripcionTooltipNueva(titulo, subtitulo, cuerpo);
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
    
      

    public async override void AplicarEfectosHabilidad(object obj, int tirada, Casilla casillaOrigenTrampas = null)
    {
    
     if(obj is Unidad) //A van los efectos a Unidades.
     {
       Unidad objetivo = (Unidad)obj;
       float defensaObjetivo = objetivo.ObtenerdefensaActual();
             
       int danioMarca = 0;
       
       Usuario.GetComponent<ClaseExplorador>().CambiarCantidadFlechas(-1);
       CrearProyectil(objetivo);

       await BattleManager.DelayCombateAsync(1300);
       float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab;
       
       //Chequear si tiene Marcar Presa
       if(ChequearTieneMarcarPresa(objetivo)) //Copiar este metodo, ver bien lo de danio marca, para prdel explorador
       {
         bonusAtaque += 4;
         criticoRango += 1;
         danioMarca += 15; //Esto se suma al porcentaje de dao solamente al ser golpe critico, ver mas abajo. Ya que esta marca agrega %co.

         if(objetivo.GetComponent<MarcaMarcarPresa>().NIVEL > 1)
         {  danioMarca += 5;   }
         if(objetivo.GetComponent<MarcaMarcarPresa>().NIVEL > 2)
         {  criticoRango += 1;  }
         if(objetivo.GetComponent<MarcaMarcarPresa>().NIVEL == 4)
         {  bonusAtaque -= 2;  } //NV 4 Quita el debuff al marcar, entonces se resta los 2 que se ponia como compensacion
       }
       //----

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
        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 1 + scEstaUnidad.mod_CarAgilidad;

        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        danio -= danio / 2; //Reduce 50% por roce

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
        
        AplicarEfectoRalentizante(objetivo); 


      }
      else if (resultadoTirada == 2)
      {//GOLPE
        print("Golpe");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 1 + scEstaUnidad.mod_CarAgilidad;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

        AplicarEfectoRalentizante(objetivo); 

      }
      else if (resultadoTirada == 3)
      {//CRITICO
        print("Critico");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 1 + scEstaUnidad.mod_CarAgilidad;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje + danioMarca);

        objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);


        AplicarEfectoRalentizante(objetivo); 
      }
     
        objetivo.AplicarDebuffPorAtaquesreiterados(1);
       }   
     else if (obj is Obstaculo) //Ac van los efectos a Obstaculos
     {
       Obstaculo objetivo = (Obstaculo)obj;
       //---


       float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+1+scEstaUnidad.mod_CarAgilidad;
       danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
       
       objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
     }
    }

  void AplicarEfectoRalentizante(Unidad objetivo)
  {
    if (objetivo.TiradaSalvacion(objetivo.mod_TSFortaleza,12))
    {
         /////////////////////////////////////////////
        //BUFF ---- As se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Ralentizado";
        buff.boolfDebufftBuff = false;
        buff.DuracionBuffRondas = 2;
        buff.cantAPMax -= 1;
        buff.cantTsReflejos -= 2;
        buff.cantDefensa -= 1;
        buff.cantAtaque -= 1;
        buff.AplicarBuff(objetivo);
        // Agrega el componente Buff al objeto objetivo y asigna la configuracin del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);


    }
  }
    
    async Task CrearProyectil(object Objetivo)
  {
    await BattleManager.DelayCombateAsync(200);
    GameObject flechaPrefab = BattleManager.Instance.contenedorPrefabs.Flecha;
    GameObject Proyectil = Instantiate(flechaPrefab);
    Proyectil.GetComponent<ArrowFlight>().startMarker = transform;
    Proyectil.GetComponent<ArrowFlight>().parabola = 0.9f;
    Proyectil.GetComponent<ArrowFlight>().velocidad = 4.9f;


    if (Objetivo != null)
    {

      if (Objetivo is Unidad)
      {
        Unidad obj = (Unidad)Objetivo;
        Proyectil.GetComponent<ArrowFlight>().endMarker = obj.transform;
      }
      else if (Objetivo is Obstaculo)
      {
        Obstaculo obj = (Obstaculo)Objetivo;
        Proyectil.GetComponent<ArrowFlight>().endMarker = obj.transform;
      }
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




