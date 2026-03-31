using UnityEngine;

public class REPRESENTACIONEvasionMaestra : Habilidad
{
    public override void Awake()
    {
        nombre = "Evasion Maestra";
        IDenClase = 2;
        costoAP = 0;
        costoPM = 0;
        cooldownMax = 0;
        cooldownActual = 0;
        requiereRecurso = 0;
        Usuario = gameObject;
        scEstaUnidad = Usuario.GetComponent<Unidad>();
        esZonal = false;
        enArea = 0;
        esforzable = 0;
        esCargable = false;
        esMelee = false;
        bAfectaObstaculos = false;
        poneTrampas = false;
        poneObstaculo = false;
        esHostil = false;
        esDiscreta = true;

        imHab = Resources.Load<Sprite>("imHab/Duelista_EvasionMaestra");
        if (imHab == null)
        {
            imHab = Resources.Load<Sprite>("imHab/Duelista_habilidad");
        }

        ActualizarDescripcion();
    }

    public override void ActualizarDescripcion()
    {
        bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
        bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;

        int usosPorTurno = NIVEL == 4 ? 2 : 1;
        bool daImpulso = NIVEL > 1;
        bool daEvasion = NIVEL > 2;
        bool aplicaTambaleando = NIVEL == 5;

        string sufijoNivel = "I";
        if (NIVEL == 2) { sufijoNivel = "II"; }
        else if (NIVEL == 3) { sufijoNivel = "III"; }
        else if (NIVEL == 4) { sufijoNivel = "IV a"; }
        else if (NIVEL == 5) { sufijoNivel = "IV b"; }

        string cuerpo = "";
        if (esIngles)
        {
            cuerpo += "<b>Type:</b> Passive\n";
            cuerpo += "<b>Trigger:</b> when the Duelist dodges a melee attack\n";
            cuerpo += "<b>Movement:</b> steps 1 tile backward; if occupied, tries back diagonals\n";
            cuerpo += "<b>Restriction:</b> never moves into trapped tiles\n";
            cuerpo += "<b>Interrupt:</b> the attacker loses all remaining AP\n";
            cuerpo += $"<b>Uses per turn:</b> {usosPorTurno}\n";
            if (daImpulso) { cuerpo += "<b>Self:</b> gains +1 Impulse (discounted movement)\n"; }
            if (daEvasion) { cuerpo += "<b>Self:</b> gains +1 Evasion\n"; }
            if (aplicaTambaleando) { cuerpo += "<b>Lv4 B:</b> attacker rolls Reflex save vs DC 10; on fail gains Staggering (-1 max AP, -2 Defense, 1 turn)\n"; }
        }
        else if (esPortugues)
        {
            cuerpo += "<b>Tipo:</b> Passiva\n";
            cuerpo += "<b>Gatilho:</b> quando a Duelista esquiva de um ataque melee\n";
            cuerpo += "<b>Movimento:</b> recua 1 casa; se ocupada, tenta diagonais para tras\n";
            cuerpo += "<b>Restricao:</b> nunca move para casas com armadilhas\n";
            cuerpo += "<b>Interrupcao:</b> o atacante perde todo AP restante\n";
            cuerpo += $"<b>Usos por turno:</b> {usosPorTurno}\n";
            if (daImpulso) { cuerpo += "<b>Proprio:</b> ganha +1 Impulso (movimento com desconto)\n"; }
            if (daEvasion) { cuerpo += "<b>Proprio:</b> ganha +1 Evasao\n"; }
            if (aplicaTambaleando) { cuerpo += "<b>Nv4 B:</b> atacante rola Reflexos vs CD 10; se falhar ganha Cambaleando (-1 AP max, -2 Defesa, 1 turno)\n"; }
        }
        else
        {
            cuerpo += "<b>Tipo:</b> Pasiva\n";
            cuerpo += "<b>Disparo:</b> cuando la Duelista esquiva un ataque melee\n";
            cuerpo += "<b>Movimiento:</b> retrocede 1 casilla; si esta ocupada, intenta diagonales atras\n";
            cuerpo += "<b>Restriccion:</b> nunca se mueve a casillas con trampas\n";
            cuerpo += "<b>Interrupcion:</b> el atacante pierde todo su AP restante\n";
            cuerpo += $"<b>Usos por turno:</b> {usosPorTurno}\n";
            if (daImpulso) { cuerpo += "<b>Propio:</b> gana +1 Impulso (movimiento abaratado)\n"; }
            if (daEvasion) { cuerpo += "<b>Propio:</b> gana +1 Evasion\n"; }
            if (aplicaTambaleando) { cuerpo += "<b>Nv4 B:</b> el atacante tira TS Reflejos vs DC 10; si falla gana Tambaleando (-1 AP max, -2 Defensa, 1 turno)\n"; }
        }

        txtDescripcion = ConstruirDescripcionEstandar(
          esIngles ? "Master Evasion " + sufijoNivel : esPortugues ? "Evasao Mestra " + sufijoNivel : "Evasion Maestra " + sufijoNivel,
          esIngles
            ? "A clean backstep after dodging melee attacks that disrupts enemy tempo."
            : esPortugues
              ? "Um recuo preciso apos esquivar de ataques melee que quebra o ritmo inimigo."
              : "Un retroceso preciso tras esquivar ataques melee que rompe el ritmo enemigo.",
          cuerpo,
          "",
          "#5dade2");

        bool mostrarProximoNivel = CampaignManager.Instance != null
            && CampaignManager.Instance.scMenuPersonajes != null
            && CampaignManager.Instance.scMenuPersonajes.pSel != null
            && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
        if (!mostrarProximoNivel)
        {
            return;
        }

        if (esIngles)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: gains +1 Impulse on trigger.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: gains +1 Evasion on trigger.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+1 use per turn) or Option B (Reflex save for Staggering).</color>"; }
        }
        else if (esPortugues)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: ganha +1 Impulso ao ativar.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: ganha +1 Evasao ao ativar.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+1 uso por turno) ou Opcao B (Reflexos para Cambaleando).</color>"; }
        }
        else
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: gana +1 Impulso al activarse.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: gana +1 Evasion al activarse.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (+1 uso por turno) u Opcion B (TS Reflejos para Tambaleando).</color>"; }
        }
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada) { }

    public override void Activar() { }
}
