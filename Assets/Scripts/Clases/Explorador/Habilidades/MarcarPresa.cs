using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class MarcarPresa : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;

   
     public override void  Awake()
    {
      nombre = "Marcar Presa";
      IDenClase = 4;
      costoAP = 1;
      costoPM = 1;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 5;
      bAfectaObstaculos = false;

      imHab = Resources.Load<Sprite>("imHab/Explorador_MarcarPresa");
      ActualizarDescripcion();
    }

        public override void ActualizarDescripcion()
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;

    string tituloEs = "Marcar Presa I";
    string tituloEn = "Mark Prey I";
    string tituloPt = "Marcar Presa I";
    if (NIVEL == 2) { tituloEs = "Marcar Presa II"; tituloEn = "Mark Prey II"; }
    if (NIVEL == 3) { tituloEs = "Marcar Presa III"; tituloEn = "Mark Prey III"; }
    if (NIVEL == 4) { tituloEs = "Marcar Presa IV a"; tituloEn = "Mark Prey IV a"; }
    if (NIVEL == 5) { tituloEs = "Marcar Presa IV b"; tituloEn = "Mark Prey IV b"; }
    if (NIVEL == 2) { tituloPt = "Marcar Presa II"; }
    if (NIVEL == 3) { tituloPt = "Marcar Presa III"; }
    if (NIVEL == 4) { tituloPt = "Marcar Presa IV a"; }
    if (NIVEL == 5) { tituloPt = "Marcar Presa IV b"; }

    int bonoAtaqueMarca = NIVEL == 4 ? 2 : 4;
    int bonoCritPorcentajeMarca = (1 + (NIVEL > 2 ? 1 : 0)) * 5;
    int bonoCritDanioMarca = 15 + (NIVEL > 1 ? 5 : 0);
    int recompensaVal = NIVEL == 5 ? 2 : 1;
    int recompensaApMax = NIVEL == 5 ? 2 : 1;
    int recompensaTsMental = NIVEL == 5 ? 3 : 2;
    bool aplicaPenalidadPropia = NIVEL != 4;

    string colorTitulo = "#5dade2";
    string colorEncabezado = "#44d3ec";
    string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
    string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
    string costoSuperior = $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}";

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> Mark\n";
      cuerpo += $"<color={colorEncabezado}><b>Target:</b></color> 1 enemy on opposite side\n";
      cuerpo += $"<color={colorEncabezado}><b>Roll/Save:</b></color> none\n";
      cuerpo += $"<color={colorEncabezado}><b>Cost:</b></color> {costoPM} Valour\n";
      cuerpo += $"<color={colorEncabezado}><b>Mark duration:</b></color> 3 turns\n";
      cuerpo += $"<color={colorEncabezado}><b>Against marked target:</b></color> roll +{bonoAtaqueMarca}, +{bonoCritPorcentajeMarca}% Crit, +{bonoCritDanioMarca}% crit damage\n";
      cuerpo += aplicaPenalidadPropia
        ? $"<color={colorEncabezado}><b>After cast:</b></color> -2 on rolls against non-marked targets for 2 turns\n"
        : $"<color={colorEncabezado}><b>After cast:</b></color> no penalty against non-marked targets\n";
      cuerpo += $"<color={colorEncabezado}><b>On marked kill:</b></color> +{recompensaVal} Valour, +{recompensaApMax} max AP, +{recompensaTsMental} Mental Save for 3 turns";
    }
    else if (esPortugues)
    {
      cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Marca\n";
      cuerpo += $"<color={colorEncabezado}><b>Alvo:</b></color> 1 inimigo do lado oposto\n";
      cuerpo += $"<color={colorEncabezado}><b>Rolagem/TS:</b></color> nao tem\n";
      cuerpo += $"<color={colorEncabezado}><b>Custo:</b></color> {costoPM} Valentia\n";
      cuerpo += $"<color={colorEncabezado}><b>Duracao da marca:</b></color> 3 turnos\n";
      cuerpo += $"<color={colorEncabezado}><b>Contra marcado:</b></color> rolagem +{bonoAtaqueMarca}, +{bonoCritPorcentajeMarca}% Critico, +{bonoCritDanioMarca}% dano critico\n";
      cuerpo += aplicaPenalidadPropia
        ? $"<color={colorEncabezado}><b>Depois de usar:</b></color> -2 em rolagens contra alvos nao marcados por 2 turnos\n"
        : $"<color={colorEncabezado}><b>Depois de usar:</b></color> sem penalidade contra alvos nao marcados\n";
      cuerpo += $"<color={colorEncabezado}><b>Ao matar o marcado:</b></color> +{recompensaVal} Valentia, +{recompensaApMax} AP max, +{recompensaTsMental} TS Mental por 3 turnos";
    }
    else
    {
      cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Marca\n";
      cuerpo += $"<color={colorEncabezado}><b>Objetivo:</b></color> 1 enemigo del lado opuesto\n";
      cuerpo += $"<color={colorEncabezado}><b>Tirada/TS:</b></color> no tiene\n";
      cuerpo += $"<color={colorEncabezado}><b>Costo:</b></color> {costoPM} Valentia\n";
      cuerpo += $"<color={colorEncabezado}><b>Duracion de marca:</b></color> 3 turnos\n";
      cuerpo += $"<color={colorEncabezado}><b>Contra marcado:</b></color> tirada +{bonoAtaqueMarca}, +{bonoCritPorcentajeMarca}% Critico, +{bonoCritDanioMarca}% danio critico\n";
      cuerpo += aplicaPenalidadPropia
        ? $"<color={colorEncabezado}><b>Despues de lanzar:</b></color> -2 en tiradas contra no marcados por 2 turnos\n"
        : $"<color={colorEncabezado}><b>Despues de lanzar:</b></color> sin penalidad contra no marcados\n";
      cuerpo += $"<color={colorEncabezado}><b>Al matar al marcado:</b></color> +{recompensaVal} Valentia, +{recompensaApMax} AP max, +{recompensaTsMental} TS Mental por 3 turnos";
    }

    string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
    string subtitulo = esIngles
      ? "Marks one enemy and improves your attacks against it."
      : esPortugues
        ? "Marca um inimigo e melhora seus ataques contra ele."
        : "Marca un enemigo y mejora tus ataques contra el.";

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
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +5% crit damage against marked target.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +5% Crit against marked target.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A removes the self penalty; Option B improves kill reward (+1 Valour, +1 max AP, +1 Mental Save).</color>"; }
    }
    else if (esPortugues)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +5% de dano critico contra marcado.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +5% Critico contra marcado.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A remove a penalidade propria; Opcao B melhora a recompensa por morte (+1 Valentia, +1 AP max, +1 TS Mental).</color>"; }
    }
    else
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +5% al danio critico contra marcado.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +5% Critico contra marcado.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A elimina la penalidad propia; Opcion B mejora la recompensa por muerte (+1 Valentia, +1 AP max, +1 TS Mental).</color>"; }
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
    
    

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {

      if(obj is Unidad) //Acá van los efectos a Unidades.
      {
          
          Unidad objetivo = (Unidad)obj;
          VFXAplicar(objetivo.gameObject);

          BattleManager.Instance.EscribirLog(TRADU.i.Traducir(scEstaUnidad.uNombre) + " " + TRADU.i.Traducir("usa ") + TRADU.i.Traducir(nombre) + " -> " + TRADU.i.Traducir(objetivo.uNombre) + ".");

          MarcaMarcarPresa marca = new MarcaMarcarPresa();
          marca.nombre = "Presa Marcada";
          marca.quienMarco = scEstaUnidad;
          marca.NIVEL = NIVEL;
          marca.duracion = 3;

          MarcaMarcarPresa buffComponent = ComponentCopier.CopyComponent(marca, objetivo.gameObject);
          objetivo.Marcar(0);

          objetivo.GenerarTextoFlotante(TRADU.i.Traducir("Marcado"), Color.yellow);

                        
      }
      
      cooldownActual = cooldownMax;
    scEstaUnidad.CambiarAPActual(-costoAP); 

      if(NIVEL != 4) // a Nivel IVa, no recibe el debuff
      {
        /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Marcando Presa";
        buff.boolfDebufftBuff = false;
        buff.DuracionBuffRondas = 2;
        buff.cantAtaque -= 2;
        buff.AplicarBuff(scEstaUnidad);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, scEstaUnidad.gameObject);
      }
    }
    
      void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_MarcarPresa");

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
    
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasLadoOpuesto();
    
    
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
             c.ActivarCapaColorRojo();
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
            uni.Marcar(1);
      }

      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
       
    
        



      
         
    }

   
 
}






