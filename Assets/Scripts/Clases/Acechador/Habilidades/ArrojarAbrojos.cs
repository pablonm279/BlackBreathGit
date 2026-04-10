using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class ArrojarAbrojos : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] int usosBatalla;
  
    
      public override void  Awake()
    {
      nombre = "Arrojar Abrojos";
      IDenClase = 8;
      costoAP = 2;
      costoPM = 0;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0; 
      esforzable = 1;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 4;
      bAfectaObstaculos = false;
      poneTrampas = true;
      poneObstaculo = false;
      
      targetEspecial = 7; 
      esDiscreta = true; //No quita sigilo
     
      
      imHab = Resources.Load<Sprite>("imHab/Acechador_ArrojarAbrojos");
      ActualizarDescripcion();
    }
    public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;

      int dcBase = NIVEL > 2 ? 12 : 11;
      int bleedAplicado = 4 + (NIVEL == 4 ? 1 : 0);
      bool drenaAp = NIVEL == 5;
      string danioBase = NIVEL > 1 ? "1d12 + 1" : "1d12";

      string tituloEs = "Arrojar Abrojos I";
      string tituloEn = "Throw Caltrops I";
      string tituloPt = "Lancar Abrolhos I";
      if (NIVEL == 2) { tituloEs = "Arrojar Abrojos II"; tituloEn = "Throw Caltrops II"; }
      if (NIVEL == 3) { tituloEs = "Arrojar Abrojos III"; tituloEn = "Throw Caltrops III"; }
      if (NIVEL == 4) { tituloEs = "Arrojar Abrojos IV a"; tituloEn = "Throw Caltrops IV a"; }
      if (NIVEL == 5) { tituloEs = "Arrojar Abrojos IV b"; tituloEn = "Throw Caltrops IV b"; }
      if (NIVEL == 2) { tituloPt = "Lancar Abrolhos II"; }
      if (NIVEL == 3) { tituloPt = "Lancar Abrolhos III"; }
      if (NIVEL == 4) { tituloPt = "Lancar Abrolhos IV a"; }
      if (NIVEL == 5) { tituloPt = "Lancar Abrolhos IV b"; }

      string lineaSalvacion = ConstruirLineaSalvacion(esIngles, TipoSalvacionDescripcion.Reflejos, dcBase);

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += "<b>Type:</b> Ranged (3 range)\n";
        cuerpo += "<b>Target:</b> 1 tile (plus empty diagonals around it)\n";
        cuerpo += "<b>On cast:</b> places caltrop traps on the target tile and valid diagonals (same side)\n";
        cuerpo += "<b>Trap profile:</b> 1 use, 10 turns duration\n";
        cuerpo += $"<b>Trap trigger damage:</b> {danioBase} | <b>Type:</b> Piercing\n";
        cuerpo += lineaSalvacion + "\n";
        cuerpo += $"<b>On failed save:</b> damage x2, +{bleedAplicado} Bleed";
        if (drenaAp)
        {
          cuerpo += ", -1 AP";
        }
        cuerpo += "\n<b>Stealth interaction:</b> Discreet (does not reveal the caster)";
      }
      else if (esPortugues)
      {
        cuerpo += "<b>Tipo:</b> Alcance (3 de alcance)\n";
        cuerpo += "<b>Alvo:</b> 1 celula (mais diagonais vazias ao redor)\n";
        cuerpo += "<b>Ao usar:</b> coloca armadilhas de abrolhos na celula alvo e diagonais validas (mesmo lado)\n";
        cuerpo += "<b>Perfil da armadilha:</b> 1 uso, 10 turnos de duracao\n";
        cuerpo += $"<b>Dano ao ativar armadilha:</b> {danioBase} | <b>Tipo:</b> Perfurante\n";
        cuerpo += lineaSalvacion + "\n";
        cuerpo += $"<b>Se falhar no teste:</b> dano x2, +{bleedAplicado} Sangramento";
        if (drenaAp)
        {
          cuerpo += ", -1 AP";
        }
        cuerpo += "\n<b>Interacao com furtividade:</b> Discreta (nao revela o lancador)";
      }
      else
      {
        cuerpo += "<b>Tipo:</b> Rango (3 alcance)\n";
        cuerpo += "<b>Objetivo:</b> 1 casilla (mas diagonales vacias alrededor)\n";
        cuerpo += "<b>Al lanzarla:</b> coloca trampas de abrojos en la casilla objetivo y diagonales validas (mismo lado)\n";
        cuerpo += "<b>Perfil de trampa:</b> 1 uso, 10 turnos de duracion\n";
        cuerpo += $"<b>Danio al activar trampa:</b> {danioBase} | <b>Tipo:</b> Perforante\n";
        cuerpo += lineaSalvacion + "\n";
        cuerpo += $"<b>Si falla TS:</b> danio x2, +{bleedAplicado} Sangrado";
        if (drenaAp)
        {
          cuerpo += ", -1 AP";
        }
        cuerpo += "\n<b>Interaccion con sigilo:</b> Discreta (no revela al lanzador)";
      }

      string costos = esIngles
        ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}\n- Effortable: Yes ({esforzable})"
        : esPortugues
          ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}\n- Esforcavel: Sim ({esforzable})"
          : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}\n- Esforzable: Si ({esforzable})";

      txtDescripcion = ConstruirDescripcionEstandar(
        esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
        esIngles
          ? "Seeds a movement denial zone with high punishment on failed reflex saves."
          : esPortugues
            ? "Cria uma zona de negacao de movimento com alto castigo ao falhar em Reflexos."
          : "Siembra una zona de negacion de movimiento con alto castigo al fallar Reflejos.",
        cuerpo,
        costos,
        "#5dade2");

      bool mostrarProximoNivel = CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
      if (!mostrarProximoNivel)
      {
        return;
      }

      if (esIngles)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 trap damage.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 save DC base.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+1 Bleed) or Option B (-1 AP on failed save).</color>"; }
      }
      else if (esPortugues)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 dano da armadilha.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 na CD base da resistencia.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+1 Sangramento) ou Opcao B (-1 AP ao falhar na resistencia).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 danio de trampa.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 al DC base de TS.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (+1 Sangrado) u Opcion B (-1 AP al fallar TS).</color>"; }
      }
    }
    void Start()
    {
      
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
    
    

    public async override void AplicarEfectosHabilidad(object obj, int tirada, Casilla cas)
    {
      
      List<Casilla> CasillasXcas = new List<Casilla>();

    if (cas != null)
    {
      // Solo agregamos la casilla objetivo si está libre de unidades
      var unidadEnCasilla = cas.Presente != null ? cas.Presente.GetComponent<Unidad>() : null;
      if (unidadEnCasilla == null)
      {
        CasillasXcas.Add(cas); // Agregar la casilla original si está libre
      }
    }
    foreach (Casilla c in BattleManager.Instance.lCasillasTotal)
    {

      if (c != null && cas != null)
      {
        if (c.Presente != null)
        {
          continue; // Si la casilla  tiene presente, no la agregamos
        }
        if (c != cas && (cas.lado == c.lado))
        {

          if (c.posX + 1 == cas.posX && (c.posY == cas.posY - 1 || c.posY == cas.posY + 1))
          {
            CasillasXcas.Add(c);
          }

          if (c.posX - 1 == cas.posX && (c.posY == cas.posY - 1 || c.posY == cas.posY + 1))
          {
            CasillasXcas.Add(c);
          }

        }
      }
    }

    foreach (Casilla c in CasillasXcas)
      {
        c.AddComponent<Abrojo>();
        c.GetComponent<Abrojo>().InicializarCreador(scEstaUnidad, NIVEL);

      }
    


       BattleManager.Instance.HabilidadActiva = null;// desactiva la habilidad activa, para que no se pueda usar de nuevo
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
      lCasillasafectadas.Clear();
      
      lCasillasafectadas = Origen.ObtenerCasillasRango(3,2);
    
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




