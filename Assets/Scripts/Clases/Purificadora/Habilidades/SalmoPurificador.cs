using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class SalmoPurificador : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    
    
    public override void  Awake()
    {
      nombre = "Salmo Purificador";
      IDenClase = 6;
      costoAP = 3;
      costoPM = 0;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 1;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 4;
      if(NIVEL > 2){cooldownMax--;}
      bAfectaObstaculos = false;
      
     
      
      imHab = Resources.Load<Sprite>("imHab/Purificadora_SalmoPurificador");
     

    }
  public override void ActualizarDescripcion()
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;

    int debuffsPorUnidad = 1;
    if (NIVEL > 1) { debuffsPorUnidad += 1; }
    if (NIVEL == 4) { debuffsPorUnidad += 1; }
    bool daValentia = NIVEL == 5;

    string tituloEs = "Salmo Purificador I";
    string tituloEn = "Purifying Psalm I";
    string tituloPt = "Salmo Purificador I";
    if (NIVEL == 2) { tituloEs = "Salmo Purificador II"; tituloEn = "Purifying Psalm II"; }
    if (NIVEL == 3) { tituloEs = "Salmo Purificador III"; tituloEn = "Purifying Psalm III"; }
    if (NIVEL == 4) { tituloEs = "Salmo Purificador IV a"; tituloEn = "Purifying Psalm IV a"; }
    if (NIVEL == 5) { tituloEs = "Salmo Purificador IV b"; tituloEn = "Purifying Psalm IV b"; }
    if (NIVEL == 2) { tituloPt = "Salmo Purificador II"; }
    if (NIVEL == 3) { tituloPt = "Salmo Purificador III"; }
    if (NIVEL == 4) { tituloPt = "Salmo Purificador IV a"; }
    if (NIVEL == 5) { tituloPt = "Salmo Purificador IV b"; }

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += "<b>Type:</b> Ranged (4 range)\n";
      cuerpo += "<b>Target:</b> 1 unit in range\n";
      cuerpo += "<b>Area:</b> Target + adjacent units\n";
      cuerpo += $"<b>Effect:</b> Removes up to {debuffsPorUnidad} removable Debuffs or negative states from each affected unit\n";
      if (daValentia)
      {
        cuerpo += "<b>IV b Extra:</b> +1 Valour to each affected unit per removed Debuff\n";
      }
      cuerpo += "<b>Requirement:</b> Needs at least 1 Fervor to activate\n";
      cuerpo += "<b>On cast:</b> Does not consume Fervor";
    }
    else if (esPortugues)
    {
      cuerpo += "<b>Tipo:</b> Alcance (4 de alcance)\n";
      cuerpo += "<b>Alvo:</b> 1 unidade no alcance\n";
      cuerpo += "<b>Area:</b> Alvo + unidades adjacentes\n";
      cuerpo += $"<b>Efeito:</b> Remove ate {debuffsPorUnidad} Debuffs removiveis ou estados negativos de cada unidade afetada\n";
      if (daValentia)
      {
        cuerpo += "<b>Extra IV b:</b> +1 Valentia para cada unidade afetada por Debuff removido\n";
      }
      cuerpo += "<b>Requisito:</b> Precisa de pelo menos 1 Fervor para ativar\n";
      cuerpo += "<b>Ao usar:</b> Nao consome Fervor";
    }
    else
    {
      cuerpo += "<b>Tipo:</b> Rango (4 alcance)\n";
      cuerpo += "<b>Objetivo:</b> 1 unidad en rango\n";
      cuerpo += "<b>Area:</b> Objetivo + unidades adyacentes\n";
      cuerpo += $"<b>Efecto:</b> Remueve hasta {debuffsPorUnidad} debuffs removibles o estados negativos de cada unidad afectada\n";
      if (daValentia)
      {
        cuerpo += "<b>Extra IV b:</b> +1 Valentía a cada unidad afectada por cada Debuff removido\n";
      }
      cuerpo += "<b>Requisito:</b> Necesita al menos 1 Fervor para activarse\n";
      cuerpo += "<b>Al lanzar:</b> No consume Fervor";
    }

    string costos = esIngles
      ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}"
      : esPortugues
        ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}"
      : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}";

    txtDescripcion = ConstruirDescripcionEstandar(
      esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
      esIngles
        ? "A cleansing chant that removes hostile effects from a small cluster."
        : esPortugues
          ? "Um canto de limpeza que remove efeitos negativos de um pequeno grupo."
        : "Un canto de limpieza que remueve efectos negativos en un pequeno grupo.",
      cuerpo,
      costos,
      "#5dade2");

    string colorEncabezado = "#44d3ec";
    string colorValor = "#ffffff";
    string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
    string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
    string costoSuperior = cooldownMax > 0
      ? $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}"
      : $"{costoAP} {iconoAP}";
    string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
    string subtitulo = esIngles
      ? "Cleanses removable debuffs and negative states from a small allied cluster."
      : esPortugues
        ? "Remove debuffs removiveis e estados negativos de um pequeno grupo aliado."
        : "Remueve debuffs removibles y estados negativos de un pequeno grupo aliado.";
    string cuerpoNuevo = "";
    if (esIngles)
    {
      cuerpoNuevo += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Ranged cleanse (4 range)</color>\n";
      cuerpoNuevo += $"<color={colorEncabezado}><b>Target:</b></color> <color={colorValor}>1 unit and adjacent units</color>\n";
      cuerpoNuevo += $"<color={colorEncabezado}><b>Effect:</b></color> <color={colorValor}>Removes up to {debuffsPorUnidad} removable debuffs or negative states per affected unit</color>\n";
      if (daValentia) { cuerpoNuevo += $"<color={colorEncabezado}><b>Extra:</b></color> <color={colorValor}>+1 Valour per removed debuff.</color>\n"; }
      cuerpoNuevo += $"<color={colorEncabezado}><b>Requirement:</b></color> <color={colorValor}>Requires 1+ Fervor; does not consume it.</color>";
    }
    else if (esPortugues)
    {
      cuerpoNuevo += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Purificacao a alcance (4 de alcance)</color>\n";
      cuerpoNuevo += $"<color={colorEncabezado}><b>Alvo:</b></color> <color={colorValor}>1 unidade e unidades adjacentes</color>\n";
      cuerpoNuevo += $"<color={colorEncabezado}><b>Efeito:</b></color> <color={colorValor}>Remove ate {debuffsPorUnidad} debuffs removiveis ou estados negativos por unidade afetada</color>\n";
      if (daValentia) { cuerpoNuevo += $"<color={colorEncabezado}><b>Extra:</b></color> <color={colorValor}>+1 Valentia por debuff removido.</color>\n"; }
      cuerpoNuevo += $"<color={colorEncabezado}><b>Requisito:</b></color> <color={colorValor}>Requer 1+ Fervor; nao consome.</color>";
    }
    else
    {
      cuerpoNuevo += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Purificacion a rango (4 alcance)</color>\n";
      cuerpoNuevo += $"<color={colorEncabezado}><b>Objetivo:</b></color> <color={colorValor}>1 unidad y unidades adyacentes</color>\n";
      cuerpoNuevo += $"<color={colorEncabezado}><b>Efecto:</b></color> <color={colorValor}>Remueve hasta {debuffsPorUnidad} debuffs removibles o estados negativos por unidad afectada</color>\n";
      if (daValentia) { cuerpoNuevo += $"<color={colorEncabezado}><b>Extra:</b></color> <color={colorValor}>+1 Valentia por debuff removido.</color>\n"; }
      cuerpoNuevo += $"<color={colorEncabezado}><b>Requisito:</b></color> <color={colorValor}>Requiere 1+ Fervor; no lo consume.</color>";
    }

    txtDescripcion =
      $"<size=115%><color=#5dade2><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n" +
      $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n" +
      "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n" +
      cuerpoNuevo;

    bool mostrarProximoNivel = EsEscenaCampaña()
      && CampaignManager.Instance != null
      && CampaignManager.Instance.scMenuPersonajes != null
      && CampaignManager.Instance.scMenuPersonajes.pSel != null
      && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
    if (!mostrarProximoNivel)
    {
      return;
    }

    if (esIngles)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: removes +1 Debuff per unit.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: -1 cooldown.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+1 removed Debuff) or Option B (+1 Valour per removed Debuff).</color>"; }
    }
    else if (esPortugues)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: remove +1 Debuff por unidade.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: -1 recarga.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+1 Debuff removido) ou Opcao B (+1 Valentia por Debuff removido).</color>"; }
    }
    else
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: remueve +1 Debuff por unidad.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: -1 enfriamiento.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (+1 Debuff removido) u Opcion B (+1 Valentía por Debuff removido).</color>"; }
    }
  }
    void Start()
    {
       

    }

    Casilla Origen;
    public override void Activar()
    {
       if(Usuario.GetComponent<ClasePurificadora>().ObtenerFervor() > 0)
       {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

    BattleManager.Instance.scUIContadorAP.MarcarCirculos((int)scEstaUnidad.ObtenerAPActual());       }
        
    }
    
    

    public async override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {
    
     
     if(obj is Unidad) //Acá van los efectos a Unidades.
     {
       
      
       Unidad objetivo = (Unidad)obj;
       VFXAplicar(objetivo.gameObject);

       List<Unidad> aliadosAdyacentes = new List<Unidad>(); 
       aliadosAdyacentes.Add(objetivo);
      
        foreach(Casilla c in objetivo.CasillaPosicion.ObtenerCasillasAlrededor(1))
        {
          if(c.Presente != null)
          {
            if(c.Presente.GetComponent<Unidad>() != null)
            {
                aliadosAdyacentes.Add(c.Presente.GetComponent<Unidad>());
                print("ADD "+c.Presente.GetComponent<Unidad>().uNombre);
            }
           
          }
        }
        
        foreach(Unidad aliado in aliadosAdyacentes)
        {
          int buffsremover = ObtenerCantidadDebuffsARemover();
          buffsremover = RemoverEstadosNegativos(aliado, buffsremover);

          foreach (Buff buff in aliado.GetComponents<Buff>())
          {
            if(buff.esRemovible && !buff.boolfDebufftBuff)
            {
              
              if(buffsremover <= 0)
              {
                break;
              }
               if(NIVEL == 5){aliado.SumarValentia(1, mostrarTextoFlotante: false);}
              buffsremover--;

              if(buff != null)
              {
                RegistrarRemocionDebuff(aliado, buff.buffNombre);
                buff.RemoverBuff(aliado);
               
              }


            }

          }

           
        }
      











     
  
       
     


       objetivo.Marcar(0);

      
      
     }   
   
    }
    
     void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_SalmoPurificador");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);  

    }

    int ObtenerCantidadDebuffsARemover()
    {
      int debuffsRemover = 1;
      if(NIVEL > 1){debuffsRemover++;}
      if(NIVEL == 4){debuffsRemover++;}
      return debuffsRemover;
    }

    int RemoverEstadosNegativos(Unidad aliado, int debuffsRemover)
    {
      debuffsRemover = RemoverEstadoNegativoSiTiene(aliado, ref aliado.estado_ardiendo, debuffsRemover, "Ardiendo", Color.red);
      debuffsRemover = RemoverEstadoNegativoSiTiene(aliado, ref aliado.estado_congelado, debuffsRemover, "Congelado", Color.cyan);
      debuffsRemover = RemoverEstadoNegativoSiTiene(aliado, ref aliado.estado_aturdido, debuffsRemover, "Aturdido", Color.yellow);
      debuffsRemover = RemoverEstadoNegativoSiTiene(aliado, ref aliado.estado_inmovil, debuffsRemover, "Inmovil", Color.yellow);
      debuffsRemover = RemoverEstadoNegativoSiTiene(aliado, ref aliado.estado_acido, debuffsRemover, "Ácido", Color.green);
      debuffsRemover = RemoverEstadoNegativoSiTiene(aliado, ref aliado.estado_sangrado, debuffsRemover, "Sangrado", Color.red);
      debuffsRemover = RemoverEstadoNegativoSiTiene(aliado, ref aliado.estado_veneno, debuffsRemover, "Veneno", Color.green);
      debuffsRemover = RemoverEstadoNegativoSiTiene(aliado, ref aliado.estado_APModificador, debuffsRemover, "AP Reducido", Color.yellow);
      debuffsRemover = RemoverEstadoNegativoSiTiene(aliado, ref aliado.estado_ResistenciasReducidas, debuffsRemover, "Resistencias Reducidas", Color.magenta);
      debuffsRemover = RemoverEstadoNegativoSiTiene(aliado, ref aliado.estado_Condenado, debuffsRemover, "Condenado", new Color(0.4f, 0.24f, 0.5f), true);
      return debuffsRemover;
    }

    int RemoverEstadoNegativoSiTiene(Unidad aliado, ref int estado, int debuffsRemover, string nombreEstado, Color colorTexto, bool limpiarCondenaAcumulada = false)
    {
      if (debuffsRemover <= 0 || estado <= 0)
      {
        return debuffsRemover;
      }

      estado = 0;
      if (limpiarCondenaAcumulada)
      {
        aliado.estado_CondenadoTurnosSeguidos = 0;
      }

      if (NIVEL == 5)
      {
        aliado.SumarValentia(1, mostrarTextoFlotante: false);
      }

      RegistrarRemocionDebuff(aliado, nombreEstado);
      _ = aliado.GenerarTextoFlotante("<s>" + TraducirTexto(nombreEstado) + "</s>", colorTexto, FloatingTextContext.BuffEnd);
      return debuffsRemover - 1;
    }

    void RegistrarRemocionDebuff(Unidad aliado, string nombreDebuff)
    {
      string nombreLanzador = TraducirTexto(scEstaUnidad.uNombre);
      string nombreDebuffTraducido = TraducirTexto(nombreDebuff);
      string nombreAliado = TraducirTexto(aliado.uNombre);
      string verboRemueve = TRADU.i != null ? TRADU.i.Traducir(" remueve ") : " remueve ";
      string conector = (TRADU.i != null && TRADU.i.nIdioma == 2) ? " from " : (TRADU.i != null && TRADU.i.nIdioma == 3) ? " de " : " de ";
      BattleManager.Instance.EscribirLog(nombreLanzador + verboRemueve + nombreDebuffTraducido + conector + nombreAliado + ".");
    }

    string TraducirTexto(string texto)
    {
      return TRADU.i != null ? TRADU.i.Traducir(texto) : texto;
    }

    //Provisorio
    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();

    private void ObtenerObjetivos()
    {
      
      lObjetivosPosibles.Clear();
     
      
      //Casillas Alrededor al origen
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasAlrededor(4);
      lCasillasafectadas.Add(Origen); //Agrega la casilla del caster, para que se pueda targetear
    
      foreach(Casilla c in lCasillasafectadas)
      {
         c.ActivarCapaColorAzul();
        if(c.Presente == null)
        {
            continue;
        }
        
       
           if(c.Presente.GetComponent<Unidad>() == null)
           {
            continue;
           }
             if(c.Presente.GetComponent<Unidad>() != null)
           {
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());
           }
          

        
       

      }
    

      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva = new List<Unidad>(lObjetivosPosibles);

      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
       
    
        



      
         
    }

   
    

 
}





