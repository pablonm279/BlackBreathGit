using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;

public class Vigilancia : Habilidad
{
   

   
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de crpitico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano
  public override void Awake()
  {
    nombre = "Vigilancia";
    costoAP = 3;
    costoPM = 2;
    IDenClase = 6;

    if (NIVEL == 4)
    { costoPM--; }

    Usuario = this.gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    esZonal = false;
    enArea = 1;//esto es solamente para que marque de azul las casillas afectadas, no tiene otro efecto
    poneTrampas = true;
    esforzable = 0;
    esCargable = false;
    esMelee = false;
    esHostil = true;
    cooldownMax = 3;
    bAfectaObstaculos = false;



    requiereRecurso = 2; //esto es para que el boton no se active al apretar si no tiene X recursos (ej Flecha). Ver en BotonHabilidad.
    if (NIVEL == 5) { requiereRecurso++; }


    imHab = Resources.Load<Sprite>("imHab/Explorador_Vigilancia");

    txtDescripcion = "<color=#5dade2><b>Vigilancia I</b></color>\n\n";
    txtDescripcion += "<i>El explorador atacará a cualquier enemigo que entre en la zona de Vigilancia. Hará hasta 2 ataques.</i>\n\n";
    txtDescripcion += $"<color=#c8c8c8><b>RANGO</b> -Usa Ataque: <color=#ea0606>Tiro con Arco</color> - Requiere 2 Flechas</color>\n\n";
    txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP}- termina Turno \n- Costo Val: {costoPM} </color>";
      
      if (TRADU.i.nIdioma == 2) //agrega la traduccion a ingles
      {
        txtDescripcion = "<color=#5dade2><b>Surveillance I</b></color>\n\n";
        txtDescripcion += "<i>The explorer will attack any enemy that enters the Surveillance zone. Will make up to 2 attacks.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>RANGE</b> -Uses Attack: <color=#ea0606>Archery Shot</color> - Requires 2 Arrows</color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Cooldown: {cooldownMax} \n- AP Cost: {costoAP}- ends Turn \n- Valor Cost: {costoPM} </color>";
      }

    
    }

   
    public override void ActualizarDescripcion()
    {
      if(NIVEL<2)
      {
         txtDescripcion = "<color=#5dade2><b>Vigilancia I</b></color>\n\n"; 
         txtDescripcion += "<i>El explorador atacará a cualquier enemigo que entre en la zona de Vigilancia. Hará hasta 2 ataques.</i>\n\n";
         txtDescripcion += $"<color=#c8c8c8><b>RANGO</b> -Usa Ataque: <color=#ea0606>Tiro con Arco</color> con -2 Ataque. Requiere 2 Flechas</color>\n\n";
         txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP}- termina Turno \n- Costo Val: {costoPM} </color>";

        if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Ataque</color>\n\n";
          }
          }
        }
   
      }
      if(NIVEL== 2)
      {
         txtDescripcion = "<color=#5dade2><b>Vigilancia II</b></color>\n\n"; 
         txtDescripcion += "<i>El explorador atacará a cualquier enemigo que entre en la zona de Vigilancia. Hará hasta 2 ataques.</i>\n\n";
         txtDescripcion += $"<color=#c8c8c8><b>RANGO</b> -Usa Ataque: <color=#ea0606>Tiro con Arco</color> con -1 Ataque. Requiere 2 Flechas</color>\n\n";
         txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP}- termina Turno \n- Costo Val: {costoPM} </color>";

        if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Ataque</color>\n\n";
          }
          }
        }
      }
      if(NIVEL== 3)
      {
         txtDescripcion = "<color=#5dade2><b>Vigilancia III</b></color>\n\n"; 
         txtDescripcion += "<i>El explorador atacará a cualquier enemigo que entre en la zona de Vigilancia. Hará hasta 2 ataques.</i>\n\n";
         txtDescripcion += $"<color=#c8c8c8><b>RANGO</b> -Usa Ataque: <color=#ea0606>Tiro con Arco</color>. Requiere 2 Flechas</color>\n\n";
         txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP}- termina Turno \n- Costo Val: {costoPM} </color>";

         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"\n\n<color=#dfea02>-Opción A: -1 costo Valentía</color>\n";
             txtDescripcion += $"<color=#dfea02>-Opción B: +1 cantidad de Ataques, +1 uso Flecha</color>\n";
          }
          }
        }
      }
      if(NIVEL== 4)
      {
         txtDescripcion = "<color=#5dade2><b>Vigilancia IV a</b></color>\n\n"; 
         txtDescripcion += "<i>El explorador atacará a cualquier enemigo que entre en la zona de Vigilancia. Hará hasta 2 ataques.</i>\n\n";
         txtDescripcion += $"<color=#c8c8c8><b>RANGO</b> -Usa Ataque: <color=#ea0606>Tiro con Arco</color>. Requiere 2 Flechas</color>\n\n";
         txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP}- termina Turno \n- Costo Val: {costoPM-1} </color>";

     }
      if(NIVEL== 5)
      {
         txtDescripcion = "<color=#5dade2><b>Vigilancia IV b</b></color>\n\n"; 
         txtDescripcion += "<i>El explorador atacará a cualquier enemigo que entre en la zona de Vigilancia. Hará hasta 3 ataques.</i>\n\n";
         txtDescripcion += $"<color=#c8c8c8><b>RANGO</b> -Usa Ataque: <color=#ea0606>Tiro con Arco</color>. Requiere 3 Flechas</color>\n\n";
         txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP}- termina Turno \n- Costo Val: {costoPM} </color>";
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
    
    
    public int disparosEsteTurno = 0;
    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla casillaObjetivo)
    {
     
     disparosEsteTurno = 2; //2 disparos por uso de habilidad
     if(NIVEL == 5){disparosEsteTurno++;}

     List<Casilla> lCasillas = new List<Casilla>();
     lCasillas = casillaObjetivo.ObtenerCasillasAlrededor(1);
     lCasillas.Add(casillaObjetivo);

     foreach(Casilla cas in lCasillas)
     {
      if (cas.Presente != null)
      {
        continue; //Si la casilla tiene unidad, no se pone trampa
      }
      
        cas.AddComponent<VigilanciaTrampa>();
        cas.GetComponent<VigilanciaTrampa>().InicializarCreador(scEstaUnidad);
      

     }

      //Usarla termina el turno
      BattleManager.Instance.TerminarTurno();

    }


    public List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();
   
    private void ObtenerObjetivos()
    {
      //Cualquier objetivo en 1 de alcance 3 de ancho
      lObjetivosPosibles.Clear();
      
      lCasillasafectadas = Origen.ObtenerCasillasRango(6,3);
    
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
