using UnityEngine;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;

[DisallowMultipleComponent]
public class TooltipAtributoHojaPersonaje : MonoBehaviour
{
  public enum TipoAtributo
  {
    Fuerza = 1,
    Agilidad = 2,
    Poder = 3,
    Iniciativa = 4,
    PA = 5,
    Valentia = 6,
    Armadura = 7,
    Defensa = 8,
    TSReflejos = 9,
    TSFortaleza = 10,
    TSMental = 11,
    ResFuego = 12,
    ResRayo = 13,
    ResHielo = 14,
    ResArcano = 15,
    ResAcido = 16,
    ResNecro = 17,
    ResDivino = 18,
    Vida = 19,
    Personalizado = 99
  }

  [Header("Configuracion")]
  public TipoAtributo atributo = TipoAtributo.Fuerza;
  [TextArea(3, 8)] public string textoPersonalizadoES = "";
  [TextArea(3, 8)] public string textoPersonalizadoEN = "";
  [TextArea(3, 8)] public string textoPersonalizadoPT = "";
  public bool usarPosicionMouse = true;

  public void Hover(int estado)
  {
    if (estado == 1) { MostrarTooltip(); }
    else { OcultarTooltip(); }
  }

  public void MostrarTooltip()
  {
    if (TooltipStats.Instance == null)
    {
      return;
    }

    string texto = ObtenerTextoTooltip();
    if (string.IsNullOrWhiteSpace(texto))
    {
      return;
    }

    Vector3 posicion = usarPosicionMouse ? Input.mousePosition : transform.position;
    TooltipStats.Instance.ShowTooltipRaw(texto, posicion);
  }

 

  public void MostrarTooltip(BaseEventData _eventData)
  {
    MostrarTooltip();
  }

  public void OcultarTooltip()
  {
    if (TooltipStats.Instance == null)
    {
      return;
    }

    TooltipStats.Instance.HideTooltip();
  }

  public void OcultarTooltip(BaseEventData _eventData)
  {
    OcultarTooltip();
  }

  string ObtenerTextoTooltip()
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == TRADU.IdiomaPortugues;

    if (atributo == TipoAtributo.Personalizado)
    {
      return ObtenerTextoPersonalizado(esIngles, esPortugues);
    }

    return ObtenerTextoAtributo(esIngles, esPortugues);
  }

  Personaje ObtenerPersonajeContexto()
  {
    
      return CampaignManager.Instance.scMenuPersonajes.pSel;
    

    
  }

  public void MostrarTooltipNivelInterno()
  {
   
    Personaje personaje = ObtenerPersonajeContexto();
    string texto = ObtenerTextoNivel(personaje);
    if (string.IsNullOrWhiteSpace(texto))
    {
      return;
    }

    Vector3 posicion = usarPosicionMouse ? Input.mousePosition : transform.position;
    TooltipStats.Instance.ShowTooltipRaw(texto, posicion);
  }

  string ObtenerTextoNivel(Personaje personaje)
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == TRADU.IdiomaPortugues;

    int nivel = Mathf.Max(1, Mathf.RoundToInt(personaje.fNivelActual));
    int experienciaActual = Mathf.Max(0, Mathf.FloorToInt(personaje.fExperienciaActual));
    int experienciaNecesaria = Mathf.Max(1, Mathf.CeilToInt(personaje.ObtenerExperienciaNecesariaParaProximoNivel()));

    string etiquetaNivel = TRADU.i != null ? TRADU.i.Traducir("Nivel: ") : "Nivel: ";
    string etiquetaExp = TRADU.i != null ? TRADU.i.Traducir("Exp: ") : "Exp: ";

    if (esIngles)
    {
      etiquetaNivel = "Level: ";
      etiquetaExp = "EXP: ";
    }
    else if (esPortugues)
    {
      etiquetaNivel = "Nível: ";
      etiquetaExp = "EXP: ";
    }

    return etiquetaNivel + nivel + " " + etiquetaExp + experienciaActual + "/" + experienciaNecesaria;
  }

  string ObtenerTextoPersonalizado(bool esIngles, bool esPortugues)
  {
    if (esIngles)
    {
      return string.IsNullOrWhiteSpace(textoPersonalizadoEN) ? textoPersonalizadoES : textoPersonalizadoEN;
    }

    if (esPortugues)
    {
      if (!string.IsNullOrWhiteSpace(textoPersonalizadoPT))
      {
        return textoPersonalizadoPT;
      }

      string textoBase = !string.IsNullOrWhiteSpace(textoPersonalizadoES)
        ? textoPersonalizadoES
        : textoPersonalizadoEN;

      return TraducirPersonalizadoAPortugues(textoBase);
    }

    return textoPersonalizadoES;
  }

  string ObtenerTextoAtributo(bool esIngles, bool esPortugues)
  {
    switch (atributo)
    {
      case TipoAtributo.Fuerza:
        return esIngles
          ? "Determines Attack and Damage of Melee abilities. It also increases maximum life."
          : esPortugues
            ? "Determina o Ataque e o Dano de habilidades de Corpo a Corpo. Além disso, aumenta a vida máxima."
            : "Determina Ataque y Daño de habilidades Cuerpo a Cuerpo. Además aumenta la vida máxima.";
      case TipoAtributo.Agilidad:
        return esIngles
          ? "Determines Attack and Damage of Ranged abilities. It also increases Initiative."
          : esPortugues
            ? "Determina o Ataque e o Dano de habilidades de Alcance. Além disso, aumenta a Iniciativa."
            : "Determina Ataque y Daño de habilidades de Rango. Además aumenta la iniciativa.";
      case TipoAtributo.Poder:
        return esIngles
          ? "Determines Attack and Damage of Magic abilities. It also increases elemental resistances."
          : esPortugues
            ? "Determina o Ataque e o Dano de habilidades Mágicas. Além disso, aumenta as resistências elementais."
            : "Determina Ataque y Daño de habilidades Mágicas. Además aumenta las resistencias elementales.";
      case TipoAtributo.Iniciativa:
        return esIngles
          ? "After a roll, determines turn order at the start of combat."
          : esPortugues
            ? "Determina, após uma rolagem, a ordem dos turnos no início do combate."
            : "Determina, tras una tirada, el orden del turno al comienzo del combate.";
      case TipoAtributo.PA:
        return esIngles
          ? "Quantity of Action Points available each turn."
          : esPortugues
            ? "Quantidade de Pontos de Ação disponíveis a cada turno."
            : "Cantidad de Puntos de Acción disponibles cada Turno.";
      case TipoAtributo.Valentia:
        return esIngles
          ? "Maximum Valour Points."
          : esPortugues
            ? "Quantidade máxima de Pontos de Valentia."
            : "Cantidad de Puntos de Valentía máximos.";
      case TipoAtributo.Armadura:
        return esIngles
          ? "Each point reduces 1 point of physical damage taken."
          : esPortugues
            ? "Cada ponto reduz 1 ponto de dano físico sofrido."
            : "Cada punto reduce 1 punto de daño físico sufrido.";
      case TipoAtributo.Defensa:
        return esIngles
          ? "Reduces the chance of being hit in combat."
          : esPortugues
            ? "Reduz a chance de ser atingido em combate."
            : "Reduce las chances de ser golpeado en combate.";
      case TipoAtributo.TSReflejos:
        return esIngles
          ? "Saving Throw against environmental threats."
          : esPortugues
            ? "Teste de Salvaguarda contra ameaças do ambiente."
            : "Tirada de Salvación contra amenazas del entorno.";
      case TipoAtributo.TSFortaleza:
        return esIngles
          ? "Saving Throw against physical afflictions."
          : esPortugues
            ? "Teste de Salvaguarda contra afecções do corpo."
            : "Tirada de Salvación contra afecciones del cuerpo.";
      case TipoAtributo.TSMental:
        return esIngles
          ? "Saving Throw against mental afflictions."
          : esPortugues
            ? "Teste de Salvaguarda contra afecções da mente."
            : "Tirada de Salvación contra afecciones de la mente.";
      case TipoAtributo.ResFuego:
        return esIngles
          ? "Reduces incoming Fire damage."
          : esPortugues
            ? "Reduz o dano de Fogo recebido."
            : "Reduce el daño Fuego recibido.";
      case TipoAtributo.ResRayo:
        return esIngles
          ? "Reduces incoming Electric damage."
          : esPortugues
            ? "Reduz o dano Elétrico recebido."
            : "Reduce el daño Eléctrico recibido.";
      case TipoAtributo.ResHielo:
        return esIngles
          ? "Reduces incoming Frost damage."
          : esPortugues
            ? "Reduz o dano de Frio recebido."
            : "Reduce el daño Frío recibido.";
      case TipoAtributo.ResArcano:
        return esIngles
          ? "Reduces incoming Arcane damage."
          : esPortugues
            ? "Reduz o dano Arcano recebido."
            : "Reduce el daño Arcano recibido.";
      case TipoAtributo.ResAcido:
        return esIngles
          ? "Reduces incoming Acid damage."
          : esPortugues
            ? "Reduz o dano de Ácido recebido."
            : "Reduce el daño Ácido recibido.";
      case TipoAtributo.ResNecro:
        return esIngles
          ? "Reduces incoming Necrotic damage."
          : esPortugues
            ? "Reduz o dano Necrótico recebido."
            : "Reduce el daño Necrótico recibido.";
      case TipoAtributo.ResDivino:
        return esIngles
          ? "Reduces incoming Divine damage."
          : esPortugues
            ? "Reduz o dano Divino recebido."
            : "Reduce el daño Divine recibido.";
      case TipoAtributo.Vida:
        return esIngles
          ? "Maximum Life points."
          : esPortugues
            ? "Quantidade máxima de Vida."
            : "Cantidad máxima de Vida.";
      default:
        return esIngles ? textoPersonalizadoEN : esPortugues ? textoPersonalizadoPT : textoPersonalizadoES;
    }
  }

  string TraducirPersonalizadoAPortugues(string texto)
  {
    string clave = NormalizarTexto(texto);

    switch (clave)
    {
      case "Reduce el daño Ácido recibido.":
        return "Reduz o dano de Ácido recebido.";
      case "Cantidad de Puntos de Acción disponibles cada Turno.":
        return "Quantidade de Pontos de Ação disponíveis a cada turno.";
      case "Reduce el daño Divine recibido.":
      case "Reduce el daño Divino recibido.":
        return "Reduz o dano Divino recebido.";
      case "Cantidad de Puntos de Valentía máximos.":
        return "Quantidade máxima de Pontos de Valentia.";
      case "Vanguardia: comienza el combate en la columna frontal.":
        return "Vanguarda: começa o combate na coluna frontal.";
      case "Retaguardia: comienza el combate en la columna trasera.":
        return "Retaguarda: começa o combate na coluna traseira.";
      case "Cada punto reduce 1 punto de daño físico sufrido.":
        return "Cada ponto reduz 1 ponto de dano físico sofrido.";
      case "Determina Ataque y Daño de habilidades Mágicas. Además aumenta las resistencias elementales.":
        return "Determina o Ataque e o Dano de habilidades Mágicas. Além disso, aumenta as resistências elementais.";
      case "Tirada de Salvación contra amenazas del entorno.":
        return "Teste de Salvaguarda contra ameaças do ambiente.";
      case "Determina Ataque y Daño de habilidades de Rango. Además aumenta la iniciativa.":
        return "Determina o Ataque e o Dano de habilidades de Alcance. Além disso, aumenta a Iniciativa.";
      case "Determina, tras una tirada, el orden del turno al comienzo del combate.":
        return "Determina, após uma rolagem, a ordem dos turnos no início do combate.";
      case "Reduce las chances de ser golpeado en combate.":
        return "Reduz a chance de ser atingido em combate.";
      case "Reduce el daño Fuego recibido.":
        return "Reduz o dano de Fogo recebido.";
      case "Reduce el daño Necrótico recibido.":
        return "Reduz o dano Necrótico recebido.";
      case "Reduce el daño Frío recibido.":
        return "Reduz o dano de Frio recebido.";
      case "Reduce el daño Arcano recibido.":
        return "Reduz o dano Arcano recebido.";
      case "Tirada de Salvación contra affecciones del cuerpo.":
      case "Tirada de Salvación contra afecciones del cuerpo.":
        return "Teste de Salvaguarda contra afecções do corpo.";
      case "Reduce el daño Eléctrico recibido.":
        return "Reduz o dano Elétrico recebido.";
      case "Tirada de Salvación contra afecciones de la mente.":
        return "Teste de Salvaguarda contra afecções da mente.";
      default:
        return texto;
    }
  }

  string NormalizarTexto(string texto)
  {
    if (string.IsNullOrWhiteSpace(texto))
    {
      return string.Empty;
    }

    return Regex.Replace(texto, @"\s+", " ").Trim();
  }
}
