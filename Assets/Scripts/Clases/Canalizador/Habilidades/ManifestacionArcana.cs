using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class ManifestacionArcana : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] int usosBatalla;
  
    
    public override void  Awake()
    {
      nombre = "Manifestación Arcana";
      IDenClase = 10;
      costoAP = 7;
      costoPM = 1;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0; 
      esforzable = 3;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 7;
      bAfectaObstaculos = false;
      poneTrampas = false;
      poneObstaculo = true;
      
      requiereRecurso = 2; //Requiere tener 2 Tier energía 
      if (NIVEL == 5) { requiereRecurso--; }
      
      imHab = Resources.Load<Sprite>("imHab/Canalizador_ManifestacionArcana");
    }
  public override void ActualizarDescripcion()
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;

    int energiaRequerida = NIVEL == 5 ? 1 : 2;
    int bonusAtaqueBase = NIVEL > 1 ? 1 : 0;
    int bonusDefensaBase = NIVEL > 2 ? 1 : 0;
    int bonusAPMax = NIVEL == 4 ? 1 : 0;

    string tituloEs = "Manifestacion Arcana I";
    string tituloEn = "Arcane Manifestation I";
    string tituloPt = "Manifestacao Arcana I";
    if (NIVEL == 2) { tituloEs = "Manifestacion Arcana II"; tituloEn = "Arcane Manifestation II"; }
    if (NIVEL == 3) { tituloEs = "Manifestacion Arcana III"; tituloEn = "Arcane Manifestation III"; }
    if (NIVEL == 4) { tituloEs = "Manifestacion Arcana IV a"; tituloEn = "Arcane Manifestation IV a"; }
    if (NIVEL == 5) { tituloEs = "Manifestacion Arcana IV b"; tituloEn = "Arcane Manifestation IV b"; }
    if (NIVEL == 2) { tituloPt = "Manifestacao Arcana II"; }
    if (NIVEL == 3) { tituloPt = "Manifestacao Arcana III"; }
    if (NIVEL == 4) { tituloPt = "Manifestacao Arcana IV a"; }
    if (NIVEL == 5) { tituloPt = "Manifestacao Arcana IV b"; }

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += "<b>Type:</b> Summon\n";
      cuerpo += "<b>Target:</b> 1 tile in 4 range\n";
      cuerpo += "<b>Summon Effect:</b> Creates 1 Arcane Manifestation\n";
      cuerpo += "<b>On summon:</b> Absorbs all Energy Residues on the field\n";
      cuerpo += "<b>Per absorbed residue:</b> +5% Damage and +6 Max HP\n";
      if (bonusAtaqueBase > 0) { cuerpo += $"<b>Base Bonus:</b> +{bonusAtaqueBase} Attack\n"; }
      if (bonusDefensaBase > 0) { cuerpo += $"<b>Base Bonus:</b> +{bonusDefensaBase} Defense\n"; }
      if (bonusAPMax > 0) { cuerpo += $"<b>Base Bonus:</b> +{bonusAPMax} Max AP\n"; }
      cuerpo += "<b>Summon Turn:</b> Starts with 0 AP";
    }
    else if (esPortugues)
    {
      cuerpo += "<b>Tipo:</b> Invocacao\n";
      cuerpo += "<b>Alvo:</b> 1 casa em 4 de alcance\n";
      cuerpo += "<b>Efeito de invocacao:</b> Cria 1 Manifestacao Arcana\n";
      cuerpo += "<b>Ao invocar:</b> Absorve todos os Residuos Energeticos do campo\n";
      cuerpo += "<b>Por Residuo absorvido:</b> +5% Dano e +6 Vida Maxima\n";
      if (bonusAtaqueBase > 0) { cuerpo += $"<b>Bonus base:</b> +{bonusAtaqueBase} Ataque\n"; }
      if (bonusDefensaBase > 0) { cuerpo += $"<b>Bonus base:</b> +{bonusDefensaBase} Defesa\n"; }
      if (bonusAPMax > 0) { cuerpo += $"<b>Bonus base:</b> +{bonusAPMax} AP Máximo\n"; }
      cuerpo += "<b>Turno da invocacao:</b> Surge com 0 AP";
    }
    else
    {
      cuerpo += "<b>Tipo:</b> Invocación\n";
      cuerpo += "<b>Objetivo:</b> 1 casilla en 4 de alcance\n";
      cuerpo += "<b>Efecto de invocación:</b> Crea 1 Manifestacion Arcana\n";
      cuerpo += "<b>Al invocarse:</b> Absorbe todos los Residuos Energeticos del campo\n";
      cuerpo += "<b>Por cada Residuo absorbido:</b> +5% Daño y +6 Vida Máxima\n";
      if (bonusAtaqueBase > 0) { cuerpo += $"<b>Bonus base:</b> +{bonusAtaqueBase} Ataque\n"; }
      if (bonusDefensaBase > 0) { cuerpo += $"<b>Bonus base:</b> +{bonusDefensaBase} Defensa\n"; }
      if (bonusAPMax > 0) { cuerpo += $"<b>Bonus base:</b> +{bonusAPMax} AP Máximo\n"; }
      cuerpo += "<b>Turno de invocación:</b> Aparece con 0 AP";
    }

    string costos = esIngles
      ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}\n- Effortable: Yes ({esforzable})\n- Requires Energy Tier: {energiaRequerida}+"
      : esPortugues
        ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}\n- Esforcavel: Sim ({esforzable})\n- Requer Nivel de Energia: {energiaRequerida}+"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}\n- Esforzable: Si ({esforzable})\n- Requiere Nivel de Energía: {energiaRequerida}+";

    txtDescripcion = ConstruirDescripcionEstandar(
      esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
      esIngles
        ? "A high-investment summon that converts every Energy Residue into raw stats."
        : esPortugues
          ? "Uma invocacao de alto investimento que converte cada Residuo Energetico em atributos brutos."
        : "Una invocación de alta inversion que convierte cada Residuo Energetico en estadisticas brutas.",
      cuerpo,
      costos,
      "#ab47bc");

    string colorEncabezado = "#44d3ec";
    string colorValor = "#ffffff";
    string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
    string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
    string iconoEnergia = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_acumularenergia\"></voffset></size><space=-0.35em>";
    string iconoBuff = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_buff\"></voffset></size><space=-0.35em>";
    string costoSuperior = cooldownMax > 0
      ? $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}"
      : $"{costoAP} {iconoAP}";
    string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
    string subtituloFormato = esIngles
      ? "Summons an Arcane Manifestation and absorbs all Energy Residues."
      : esPortugues
        ? "Invoca uma Manifestacao Arcana e absorve todos os Residuos Energeticos."
        : "Invoca una Manifestacion Arcana y absorbe todos los Residuos Energeticos.";
    string bonusBase = "";
    if (bonusAtaqueBase > 0) { bonusBase += $"+{bonusAtaqueBase} Attack"; }
    if (bonusDefensaBase > 0) { bonusBase += (bonusBase.Length > 0 ? ", " : "") + $"+{bonusDefensaBase} Defense"; }
    if (bonusAPMax > 0) { bonusBase += (bonusBase.Length > 0 ? ", " : "") + $"+{bonusAPMax} Max AP"; }
    string bonusBasePt = "";
    if (bonusAtaqueBase > 0) { bonusBasePt += $"+{bonusAtaqueBase} Ataque"; }
    if (bonusDefensaBase > 0) { bonusBasePt += (bonusBasePt.Length > 0 ? ", " : "") + $"+{bonusDefensaBase} Defesa"; }
    if (bonusAPMax > 0) { bonusBasePt += (bonusBasePt.Length > 0 ? ", " : "") + $"+{bonusAPMax} AP Máximo"; }
    string bonusBaseEs = "";
    if (bonusAtaqueBase > 0) { bonusBaseEs += $"+{bonusAtaqueBase} Ataque"; }
    if (bonusDefensaBase > 0) { bonusBaseEs += (bonusBaseEs.Length > 0 ? ", " : "") + $"+{bonusDefensaBase} Defensa"; }
    if (bonusAPMax > 0) { bonusBaseEs += (bonusBaseEs.Length > 0 ? ", " : "") + $"+{bonusAPMax} AP Máximo"; }

    string cuerpoFormato = "";
    if (esIngles)
    {
      cuerpoFormato += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Summon</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Target:</b></color> <color={colorValor}>1 tile in 4 range</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Requirement:</b></color> <color={colorValor}>{iconoEnergia} Energy Tier {energiaRequerida}+</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>On summon:</b></color> <color={colorValor}>Creates 1 Arcane Manifestation and absorbs all {iconoEnergia} Energy Residues</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Per residue:</b></color> <color={colorValor}>{iconoBuff} +5% Damage, +6 Max HP</color>\n";
      if (bonusBase.Length > 0) { cuerpoFormato += $"<color={colorEncabezado}><b>Base bonus:</b></color> <color={colorValor}>{iconoBuff} {bonusBase}</color>\n"; }
      cuerpoFormato += $"<color={colorEncabezado}><b>Summon turn:</b></color> <color={colorValor}>Starts with 0 AP</color>";
    }
    else if (esPortugues)
    {
      cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Invocacao</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Alvo:</b></color> <color={colorValor}>1 casa em 4 de alcance</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Requisito:</b></color> <color={colorValor}>{iconoEnergia} Nivel de Energia {energiaRequerida}+</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Ao invocar:</b></color> <color={colorValor}>Cria 1 Manifestacao Arcana e absorve todos os {iconoEnergia} Residuos Energeticos</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Por residuo:</b></color> <color={colorValor}>{iconoBuff} +5% Dano, +6 Vida Maxima</color>\n";
      if (bonusBasePt.Length > 0) { cuerpoFormato += $"<color={colorEncabezado}><b>Bonus base:</b></color> <color={colorValor}>{iconoBuff} {bonusBasePt}</color>\n"; }
      cuerpoFormato += $"<color={colorEncabezado}><b>Turno da invocacao:</b></color> <color={colorValor}>Surge com 0 AP</color>";
    }
    else
    {
      cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Invocación</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Objetivo:</b></color> <color={colorValor}>1 casilla en 4 de alcance</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Requisito:</b></color> <color={colorValor}>{iconoEnergia} Nivel de Energia {energiaRequerida}+</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Al invocar:</b></color> <color={colorValor}>Crea 1 Manifestacion Arcana y absorbe todos los {iconoEnergia} Residuos Energeticos</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Por residuo:</b></color> <color={colorValor}>{iconoBuff} +5% Daño, +6 Vida Máxima</color>\n";
      if (bonusBaseEs.Length > 0) { cuerpoFormato += $"<color={colorEncabezado}><b>Bonus base:</b></color> <color={colorValor}>{iconoBuff} {bonusBaseEs}</color>\n"; }
      cuerpoFormato += $"<color={colorEncabezado}><b>Turno de invocación:</b></color> <color={colorValor}>Aparece con 0 AP</color>";
    }

    txtDescripcion =
      $"<size=115%><color=#ab47bc><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n" +
      $"<color=#8f8f8f><i>{subtituloFormato}</i></color>\n\n" +
      "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n" +
      cuerpoFormato;

    bool mostrarProximoNivel = EsEscenaCampaña() && CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
    if (!mostrarProximoNivel)
    {
      return;
    }

    if (esIngles)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 base defense for the summon.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 base attack for the summon.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+1 Max AP) or Option B (-1 Energy requirement).</color>"; }
    }
    else if (esPortugues)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 defesa base para a invocacao.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 ataque base para a invocacao.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+1 AP Maximo) ou Opcao B (-1 requisito de Energia).</color>"; }
    }
    else
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 defensa base para la invocación.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 ataque base para la invocación.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A (+1 AP Máximo) u Opción B (-1 requisito de Energía).</color>"; }
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
      
     GameObject manifestacion = Instantiate(BattleManager.Instance.contenedorPrefabs.Canalizador_ManifestacionArcana);
     Unidad uMani = manifestacion.GetComponent<Unidad>();
     
       int cantidadResiduos = 0;
      // Buscar en todas las casillas de ambos lados
      List<Casilla> todasLasCasillas = BattleManager.Instance.lCasillasTotal;

      foreach (var casilla in todasLasCasillas)
      {
        Trampa trampa = casilla.GetComponent<Trampa>();
        if (trampa != null && trampa.nombre == "Residuo Energetico")
        {
          trampa.DestruirTrampa();

           /////////////////////////////////////////////
          //BUFF ---- Así se aplica un buff/debuff
          Buff buff = new Buff();
          buff.buffNombre = "Energía Absorbida";
          buff.boolfDebufftBuff = true;
          buff.DuracionBuffRondas = -1;
          buff.cantDanioPorcentaje += 5;
          buff.cantHPMax += 6;
          buff.AplicarBuff(uMani);
          // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
          Buff buffComponent = ComponentCopier.CopyComponent(buff, uMani.gameObject);
          uMani.HP_actual = uMani.mod_maxHP;


          cantidadResiduos++;
        }
      }

    if (NIVEL > 1)
    { uMani.mod_Ataque += 1;}
    if (NIVEL > 2)
    { uMani.mod_Defensa += 1;}
    if (NIVEL == 4)
    { uMani.mod_maxAccionP += 1; uMani.CambiarAPActual((int)uMani.ObtenerAPActual()); }


    uMani.TirarIniciativa();



    

     manifestacion.SetActive(true);
     cas.PonerObjetoEnCasilla(manifestacion);
     invocado = manifestacion;
     invocado.transform.rotation = Quaternion.Euler(0, 180, 0);
     BattleManager.Instance.scUIBarraOrdenTurno.ActualizarBarraOrdenTurno();
     manifestacion.GetComponent<Unidad>().EstablecerAPActualA(0);
    

     
       BattleManager.Instance.HabilidadActiva = null;// desactiva la habilidad activa, para que no se pueda usar de nuevo
    }
  GameObject invocado;
  void rotarInvocado()
  { 
    invocado.transform.rotation = Quaternion.Euler(0, 180, 0);

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
      
      lObjetivosPosibles.Clear();
      lCasillasafectadas.Clear();
     
     
      
      //Casillas Alrededor al origen
      lCasillasafectadas = Origen.ObtenerCasillasAlrededor(4);
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
    
         
    }

   
    

 
}





