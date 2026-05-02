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

        string colorTitulo = "#5dade2";
        string colorEncabezado = "#44d3ec";
        string cuerpo = "";
        if (esIngles)
        {
            cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> Passive\n";
            cuerpo += $"<color={colorEncabezado}><b>Trigger:</b></color> when the Duelist dodges a melee attack\n";
            cuerpo += $"<color={colorEncabezado}><b>Movement:</b></color> steps 1 tile backward; if occupied, tries back diagonals\n";
            cuerpo += $"<color={colorEncabezado}><b>Restriction:</b></color> never moves into trapped tiles\n";
            cuerpo += $"<color={colorEncabezado}><b>Interrupt:</b></color> attacker loses all remaining AP\n";
            cuerpo += $"<color={colorEncabezado}><b>Uses per turn:</b></color> {usosPorTurno}";
            if (daImpulso) { cuerpo += "\n" + $"<color={colorEncabezado}><b>Self:</b></color> +1 Impulse"; }
            if (daEvasion) { cuerpo += "\n" + $"<color={colorEncabezado}><b>Self:</b></color> +1 Evasion"; }
            if (aplicaTambaleando) { cuerpo += "\n" + $"<color={colorEncabezado}><b>On trigger:</b></color> attacker rolls Reflex Save vs DC 10; on fail gains Staggering (-1 max AP, -2 Defense, 1 turn)"; }
        }
        else if (esPortugues)
        {
            cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Passiva\n";
            cuerpo += $"<color={colorEncabezado}><b>Gatilho:</b></color> quando esquiva de um ataque melee\n";
            cuerpo += $"<color={colorEncabezado}><b>Movimento:</b></color> recua 1 casa; se ocupada, tenta diagonais para tras\n";
            cuerpo += $"<color={colorEncabezado}><b>Restricao:</b></color> nunca move para casas com armadilhas\n";
            cuerpo += $"<color={colorEncabezado}><b>Interrupcao:</b></color> atacante perde todo AP restante\n";
            cuerpo += $"<color={colorEncabezado}><b>Usos por turno:</b></color> {usosPorTurno}";
            if (daImpulso) { cuerpo += "\n" + $"<color={colorEncabezado}><b>Proprio:</b></color> +1 Impulso"; }
            if (daEvasion) { cuerpo += "\n" + $"<color={colorEncabezado}><b>Proprio:</b></color> +1 Evasao"; }
            if (aplicaTambaleando) { cuerpo += "\n" + $"<color={colorEncabezado}><b>Ao ativar:</b></color> atacante rola Reflexos vs CD 10; se falhar ganha Cambaleando (-1 AP max, -2 Defesa, 1 turno)"; }
        }
        else
        {
            cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Pasiva\n";
            cuerpo += $"<color={colorEncabezado}><b>Disparo:</b></color> cuando esquiva un ataque melee\n";
            cuerpo += $"<color={colorEncabezado}><b>Movimiento:</b></color> retrocede 1 casilla; si esta ocupada, intenta diagonales atras\n";
            cuerpo += $"<color={colorEncabezado}><b>Restriccion:</b></color> nunca se mueve a casillas con trampas\n";
            cuerpo += $"<color={colorEncabezado}><b>Interrupcion:</b></color> atacante pierde todo su AP restante\n";
            cuerpo += $"<color={colorEncabezado}><b>Usos por turno:</b></color> {usosPorTurno}";
            if (daImpulso) { cuerpo += "\n" + $"<color={colorEncabezado}><b>Propio:</b></color> +1 Impulso"; }
            if (daEvasion) { cuerpo += "\n" + $"<color={colorEncabezado}><b>Propio:</b></color> +1 Evasion"; }
            if (aplicaTambaleando) { cuerpo += "\n" + $"<color={colorEncabezado}><b>Al activarse:</b></color> atacante tira TS Reflejos vs DC 10; si falla gana Tambaleando (-1 AP max, -2 Defensa, 1 turno)"; }
        }

        string titulo = esIngles ? "Master Evasion " + sufijoNivel : esPortugues ? "Evasao Mestra " + sufijoNivel : "Evasion Maestra " + sufijoNivel;
        string subtitulo = esIngles
            ? "Dodge melee attacks, step back and interrupt the attacker."
            : esPortugues
                ? "Esquiva ataques melee, recua e interrompe o atacante."
                : "Esquiva ataques melee, retrocede e corta el turno del atacante.";

        txtDescripcion = $"<size=115%><color={colorTitulo}><b>{titulo}</b></color></size>\n\n";
        txtDescripcion += $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n";
        txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
        txtDescripcion += cuerpo;

        bool mostrarProximoNivel = CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
        if (!mostrarProximoNivel) { return; }

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
