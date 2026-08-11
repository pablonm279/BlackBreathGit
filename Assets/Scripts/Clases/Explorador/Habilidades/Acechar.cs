using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class Acechar : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
      
     public override void  Awake()
    {
      nombre = "Acechar";
      costoAP = 1;
      costoPM = 0;
      IDenClase = 7;

      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 4;
      bAfectaObstaculos = false;

      imHab = Resources.Load<Sprite>("imHab/Explorador_Acechar");

       
      ActualizarDescripcion();
    
    }

  public override void ActualizarDescripcion()
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
    int buffAtaque = 2 + (NIVEL > 1 ? 1 : 0);
    int buffCrit = NIVEL > 2 ? 1 : 0;
    if (NIVEL == 4) { buffCrit += 2; }
    int buffCritPorcentaje = buffCrit * 5;
    int duracionTurnos = 2;
    bool seRemueveAlDanar = NIVEL != 5;

    string tituloEs = "Acechar I";
    string tituloEn = "Hide I";
    string tituloPt = "Espreitar I";
    if (NIVEL == 2) { tituloEs = "Acechar II"; tituloEn = "Hide II"; }
    if (NIVEL == 3) { tituloEs = "Acechar III"; tituloEn = "Hide III"; }
    if (NIVEL == 4) { tituloEs = "Acechar IV a"; tituloEn = "Hide IV a"; }
    if (NIVEL == 5) { tituloEs = "Acechar IV b"; tituloEn = "Hide IV b"; }
    if (NIVEL == 2) { tituloPt = "Espreitar II"; }
    if (NIVEL == 3) { tituloPt = "Espreitar III"; }
    if (NIVEL == 4) { tituloPt = "Espreitar IV a"; }
    if (NIVEL == 5) { tituloPt = "Espreitar IV b"; }

    string colorTitulo = "#5dade2";
    string colorEncabezado = "#44d3ec";
    string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
    string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
    string costoSuperior = $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}";

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> Self buff\n";
      cuerpo += $"<color={colorEncabezado}><b>On cast:</b></color> gains Hidden (1); ends turn\n";
      cuerpo += $"<color={colorEncabezado}><b>Effect ({duracionTurnos} turns):</b></color> +15% Damage, +{buffAtaque}";
      if (buffCritPorcentaje > 0)
      {
        cuerpo += $", +{buffCritPorcentaje}% Crit";
      }
      cuerpo += "\n";
      cuerpo += seRemueveAlDanar
        ? $"<color={colorEncabezado}><b>Removal:</b></color> removed after dealing damage"
        : $"<color={colorEncabezado}><b>Removal:</b></color> not removed after dealing damage";
    }
    else if (esPortugues)
    {
      cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Auto buff\n";
      cuerpo += $"<color={colorEncabezado}><b>Ao usar:</b></color> ganha Escondido (1); termina turno\n";
      cuerpo += $"<color={colorEncabezado}><b>Efeito ({duracionTurnos} turnos):</b></color> +15% Dano, +{buffAtaque}";
      if (buffCritPorcentaje > 0)
      {
        cuerpo += $", +{buffCritPorcentaje}% Crítico";
      }
      cuerpo += "\n";
      cuerpo += seRemueveAlDanar
        ? $"<color={colorEncabezado}><b>Remocao:</b></color> remove ao causar dano"
        : $"<color={colorEncabezado}><b>Remocao:</b></color> nao remove ao causar dano";
    }
    else
    {
      cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Auto buff\n";
      cuerpo += $"<color={colorEncabezado}><b>Al lanzarla:</b></color> gana Escondido (1); termina turno\n";
      cuerpo += $"<color={colorEncabezado}><b>Efecto ({duracionTurnos} turnos):</b></color> +15% Daño, +{buffAtaque}";
      if (buffCritPorcentaje > 0)
      {
        cuerpo += $", +{buffCritPorcentaje}% Crítico";
      }
      cuerpo += "\n";
      cuerpo += seRemueveAlDanar
        ? $"<color={colorEncabezado}><b>Remoción:</b></color> se elimina al hacer daño"
        : $"<color={colorEncabezado}><b>Remoción:</b></color> no se elimina al hacer daño";
    }

    string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
    string subtitulo = esIngles
      ? "Hide and prepare your next attacks."
      : esPortugues
        ? "Esconda-se e prepare seus próximos ataques."
        : "Se esconde y prepara sus próximos ataques.";

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
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 to the roll bonus.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +5% Crit.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+10% Crit) or Option B (effect persists after damage).</color>"; }
    }
    else if (esPortugues)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 no bonus de rolagem.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +5% Crítico.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+10% Critico) ou Opcao B (o efeito persiste ao causar dano).</color>"; }
    }
    else
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 al bonus de tirada.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +5% Crítico.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A (+10% Crítico) u Opción B (el efecto persiste al danar).</color>"; }
    }
  }
  public override async Task Resolver(List<object> Objetivos, Casilla cas) //Esto esta hecho para que anuncie el uso de la habilidad en el Log
    {
        // El log de uso ahora está centralizado en Habilidad.Resolver
      await  base.Resolver(Objetivos);
    }



    Casilla Origen;
    public override void Activar()
    {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

        
    }
    
    

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla casillaObjetivo)
    {

    
      if(obj is Unidad) //Acá van los efectos a Unidades.
      {

        Unidad objetivo = (Unidad)obj;
        BattleManager.Instance.EscribirLog(TRADU.i.Traducir(scEstaUnidad.uNombre) + " " + TRADU.i.Traducir("usa ") + TRADU.i.Traducir(nombre) + ".");
        VFXAplicar(objetivo.gameObject);
        /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Acechando";
        buff.boolfDebufftBuff = true;
        buff.DuracionBuffRondas = 2;
        buff.cantAtaque += 2;
        buff.cantDanioPorcentaje += 15;
        if(NIVEL > 1){ buff.cantAtaque += 1;}
        if(NIVEL > 2){ buff.cantCritDado += 1;}
        if(NIVEL == 4){ buff.cantCritDado += 2;}
        buff.AplicarBuff(objetivo);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);
        objetivo.Marcar(0);

        
        //Agrega acechar
        objetivo.GanarEscondido(1);

        //Usarla termina el turno
        BattleManager.Instance.TerminarTurno();
      }
    }
    
     void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_Acechar");

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
      //Cualquier objetivo en 1 de alcance 3 de ancho
      lObjetivosPosibles.Clear();
    
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasMismoLado();
     
    
      foreach(Casilla c in lCasillasafectadas)
      {
       
        
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
            if(c.Presente.GetComponent<Unidad>() == scEstaUnidad)
            {
             c.ActivarCapaColorAzul();
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());
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





