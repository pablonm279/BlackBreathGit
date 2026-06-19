using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class SiguesTu : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;

   
    public override void  Awake()
    {
      nombre = "Sigues T\u00FA";
      IDenClase = 8;
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
      cooldownMax = 3;
      bAfectaObstaculos = false;

      imHab = Resources.Load<Sprite>("imHab/Caballero_SiguesTu");
      ActualizarDescripcion();
    }

   public override void ActualizarDescripcion()
   {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;

      int duracionMarca = 3;
      int bonusDanioMarca = NIVEL > 2 ? 10 : 8;
      int bonusCritMarca = NIVEL > 1 ? 2 : 0;
      int bonusCritMarcaPorcentaje = bonusCritMarca * 5;
      int dcSalvacion = NIVEL == 4 ? 110 : 10;
      int durDebuff = NIVEL == 5 ? 4 : 2;
      bool sinSalvacion = NIVEL == 4;
      string lineaSalvacionEs = ConstruirLineaSalvacion(false, TipoSalvacionDescripcion.Mental, dcSalvacion);
      string lineaSalvacionEn = ConstruirLineaSalvacion(true, TipoSalvacionDescripcion.Mental, dcSalvacion);

      string tituloEs = "Sigues Tu I";
      string tituloEn = "You Are Next I";
      string tituloPt = "Voce e o Próximo I";
      if (NIVEL == 2) { tituloEs = "Sigues Tu II"; tituloEn = "You Are Next II"; }
      if (NIVEL == 3) { tituloEs = "Sigues Tu III"; tituloEn = "You Are Next III"; }
      if (NIVEL == 4) { tituloEs = "Sigues Tu IV a"; tituloEn = "You Are Next IV a"; }
      if (NIVEL == 5) { tituloEs = "Sigues Tu IV b"; tituloEn = "You Are Next IV b"; }
      if (NIVEL == 2) { tituloPt = "Voce e o Próximo II"; }
      if (NIVEL == 3) { tituloPt = "Voce e o Próximo III"; }
      if (NIVEL == 4) { tituloPt = "Voce e o Próximo IV a"; }
      if (NIVEL == 5) { tituloPt = "Voce e o Próximo IV b"; }

      string colorEncabezado = "#44d3ec";
      string colorValor = "#ffffff";
      string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
      string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
      string iconoMarca = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_marcado\"></voffset></size><space=-0.35em>";
      string iconoBuff = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_buff\"></voffset></size><space=-0.35em>";
      string iconoDebuff = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_debuff\"></voffset></size><space=-0.35em>";
      string costoSuperior = cooldownMax > 0
        ? $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}"
        : $"{costoAP} {iconoAP}";

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += "<b>Type:</b> Mark + Debuff\n";
        cuerpo += "<b>Target:</b> 1 enemy unit\n";
        cuerpo += $"<b>Mark ({duracionMarca} turns):</b> enables bonuses only for Vertical Cut and Cleave\n";
        cuerpo += $"<b>Marked bonuses:</b> +5 attack, +{bonusDanioMarca} damage";
        if (bonusCritMarca > 0)
        {
          cuerpo += $", +{bonusCritMarcaPorcentaje}% Crit";
        }
        cuerpo += "\n";
        cuerpo += "<b>Mark consumption:</b> consumed on first Vertical Cut/Cleave attempt\n";
        if (sinSalvacion)
        {
          cuerpo += $"<b>Debuff:</b> -2 Attack for {durDebuff} turns (no save)";
        }
        else
        {
          cuerpo += $"{lineaSalvacionEn}\n";
          cuerpo += $"<b>On failed save:</b> -2 Attack for {durDebuff} turns";
        }
      }
      else if (esPortugues)
      {
        cuerpo += "<b>Tipo:</b> Marca + Debuff\n";
        cuerpo += "<b>Alvo:</b> 1 unidade inimiga\n";
        cuerpo += $"<b>Marca ({duracionMarca} turnos):</b> habilita bonus apenas para Corte Vertical e Partir\n";
        cuerpo += $"<b>Bonus no marcado:</b> +5 ataque, +{bonusDanioMarca} dano";
        if (bonusCritMarca > 0)
        {
          cuerpo += $", +{bonusCritMarcaPorcentaje}% Crítico";
        }
        cuerpo += "\n";
        cuerpo += "<b>Consumo da marca:</b> consumida na primeira tentativa de Corte Vertical/Partir\n";
        if (sinSalvacion)
        {
          cuerpo += $"<b>Debuff:</b> -2 Ataque por {durDebuff} turnos (sem resistencia)";
        }
        else
        {
          cuerpo += $"{lineaSalvacionEs}\n";
          cuerpo += $"<b>Se falhar na resistencia:</b> -2 Ataque por {durDebuff} turnos";
        }
      }
      else
      {
        cuerpo += "<b>Tipo:</b> Marca + Debuff\n";
        cuerpo += "<b>Objetivo:</b> 1 unidad enemiga\n";
        cuerpo += $"<b>Marca ({duracionMarca} turnos):</b> habilita bonos solo para Corte Vertical y Partir\n";
        cuerpo += $"<b>Bonos sobre marcado:</b> +5 ataque, +{bonusDanioMarca} daño";
        if (bonusCritMarca > 0)
        {
          cuerpo += $", +{bonusCritMarcaPorcentaje}% Crítico";
        }
        cuerpo += "\n";
        cuerpo += "<b>Consumo de marca:</b> se consume en el primer intento de Corte Vertical/Partir\n";
        if (sinSalvacion)
        {
          cuerpo += $"<b>Debuff:</b> -2 Ataque por {durDebuff} turnos (sin TS)";
        }
        else
        {
          cuerpo += $"{lineaSalvacionEs}\n";
          cuerpo += $"<b>Si falla TS:</b> -2 Ataque por {durDebuff} turnos";
        }
      }

      string costos = esIngles
        ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}"
        : esPortugues
          ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}";

      txtDescripcion = ConstruirDescripcionEstandar(
        esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
        esIngles
          ? "A lethal threat mark that sets up your single-target finishers."
          : esPortugues
            ? "Uma marca de ameaca letal que prepara seus finalizadores de alvo unico."
          : "Una marca de amenaza letal que prepara tus remates de objetivo único.",
        cuerpo,
        costos,
        "#5dade2");

      string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
      string subtitulo = esIngles
        ? "Marks one enemy for stronger Vertical Cut and Cleave hits."
        : esPortugues
          ? "Marca um inimigo para fortalecer Corte Vertical e Partir."
          : "Marca un enemigo para potenciar Corte Vertical y Partir.";

      string cuerpoFormato = "";
      if (esIngles)
      {
        cuerpoFormato += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Mark + debuff</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Target:</b></color> <color={colorValor}>1 enemy unit</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>On cast:</b></color> <color={colorValor}>{iconoMarca} Marks target for {duracionMarca} turns</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Against marked target:</b></color> <color={colorValor}>{iconoBuff} Vertical Cut and Cleave gain +5 attack, +{bonusDanioMarca} damage";
        if (bonusCritMarca > 0)
        {
          cuerpoFormato += $", +{bonusCritMarcaPorcentaje}% Crit";
        }
        cuerpoFormato += "</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Mark use:</b></color> <color={colorValor}>Consumed on first Vertical Cut/Cleave attempt</color>\n";
        if (sinSalvacion)
        {
          cuerpoFormato += $"<color={colorEncabezado}><b>Debuff:</b></color> <color={colorValor}>{iconoDebuff} -2 Attack for {durDebuff} turns, no save</color>";
        }
        else
        {
          cuerpoFormato += $"<color={colorEncabezado}><b>Save:</b></color> <color={colorValor}>Mental vs DC {dcSalvacion}</color>\n";
          cuerpoFormato += $"<color={colorEncabezado}><b>On failed save:</b></color> <color={colorValor}>{iconoDebuff} -2 Attack for {durDebuff} turns</color>";
        }
      }
      else if (esPortugues)
      {
        cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Marca + debuff</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Alvo:</b></color> <color={colorValor}>1 unidade inimiga</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Ao usar:</b></color> <color={colorValor}>{iconoMarca} Marca o alvo por {duracionMarca} turnos</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Contra alvo marcado:</b></color> <color={colorValor}>{iconoBuff} Corte Vertical e Partir ganham +5 ataque, +{bonusDanioMarca} dano";
        if (bonusCritMarca > 0)
        {
          cuerpoFormato += $", +{bonusCritMarcaPorcentaje}% Crítico";
        }
        cuerpoFormato += "</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Uso da marca:</b></color> <color={colorValor}>Consumida na primeira tentativa de Corte Vertical/Partir</color>\n";
        if (sinSalvacion)
        {
          cuerpoFormato += $"<color={colorEncabezado}><b>Debuff:</b></color> <color={colorValor}>{iconoDebuff} -2 Ataque por {durDebuff} turnos, sem resistencia</color>";
        }
        else
        {
          cuerpoFormato += $"<color={colorEncabezado}><b>Resistencia:</b></color> <color={colorValor}>Mental vs CD {dcSalvacion}</color>\n";
          cuerpoFormato += $"<color={colorEncabezado}><b>Se falhar:</b></color> <color={colorValor}>{iconoDebuff} -2 Ataque por {durDebuff} turnos</color>";
        }
      }
      else
      {
        cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Marca + debuff</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Objetivo:</b></color> <color={colorValor}>1 unidad enemiga</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Al usar:</b></color> <color={colorValor}>{iconoMarca} Marca al objetivo por {duracionMarca} turnos</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Contra marcado:</b></color> <color={colorValor}>{iconoBuff} Corte Vertical y Partir ganan +5 ataque, +{bonusDanioMarca} daño";
        if (bonusCritMarca > 0)
        {
          cuerpoFormato += $", +{bonusCritMarcaPorcentaje}% Crítico";
        }
        cuerpoFormato += "</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Uso de marca:</b></color> <color={colorValor}>Se consume en el primer intento de Corte Vertical/Partir</color>\n";
        if (sinSalvacion)
        {
          cuerpoFormato += $"<color={colorEncabezado}><b>Debuff:</b></color> <color={colorValor}>{iconoDebuff} -2 Ataque por {durDebuff} turnos, sin TS</color>";
        }
        else
        {
          cuerpoFormato += $"<color={colorEncabezado}><b>TS:</b></color> <color={colorValor}>Mental vs DC {dcSalvacion}</color>\n";
          cuerpoFormato += $"<color={colorEncabezado}><b>Si falla:</b></color> <color={colorValor}>{iconoDebuff} -2 Ataque por {durDebuff} turnos</color>";
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
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +10% Crit bonus on marked target.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +2 marked bonus damage.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (debuff has no save) or Option B (+2 debuff duration).</color>"; }
      }
      else if (esPortugues)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +10% Crítico sobre o marcado.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +2 de dano extra no marcado.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (debuff sem resistencia) ou Opcao B (+2 turnos de debuff).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +10% Crítico sobre el marcado.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +2 daño extra al marcado.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A (debuff sin TS) u Opción B (+2 turnos de debuff).</color>"; }
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

                BattleManager.Instance.EscribirLog(TRADU.i.Traducir(scEstaUnidad.uNombre) + " " + TRADU.i.Traducir("usa ") + TRADU.i.Traducir(nombre) + " -> " + TRADU.i.Traducir(objetivo.uNombre) + ".");
                VFXAplicar(objetivo.gameObject);
                MarcaSiguesTu marca = new MarcaSiguesTu();
                marca.nombre = "Sigues Tu";
                marca.quienMarco = scEstaUnidad;
                marca.NIVEL = NIVEL;
                marca.duracion = 3;

                MarcaSiguesTu buffComponent = ComponentCopier.CopyComponent(marca, objetivo.gameObject);
                objetivo.Marcar(0);

                objetivo.GenerarTextoFlotante(TRADU.i.Traducir("Marcado"), Color.yellow);

                int salvDC = 10;
                if(NIVEL == 4){salvDC += 100;} //Si nivel 4a, "no hay tirada de salvación"
                int durDebuff = 2;
                if(NIVEL == 5){durDebuff += 2;} //Si nivel 4b, dura 2 turnos+ debuff

                if(objetivo.TiradaSalvacion(objetivo.mod_TSMental, salvDC))
                {
                    //BUFF ---- Así se aplica un buff/debuff
                    Buff debuff = new Buff();
                    debuff.buffNombre = "Amedrentado";
                    debuff.boolfDebufftBuff = false;
                    debuff.DuracionBuffRondas = durDebuff;
                    debuff.cantAtaque = -2;
                    debuff.AplicarBuff(objetivo);
                    // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
                    ComponentCopier.CopyComponent(debuff, objetivo.gameObject);

                }
                
            }
    }
    
     void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_SiguesTu");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
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
      }

      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
       
    
        



      
         
    }

   
 
}








