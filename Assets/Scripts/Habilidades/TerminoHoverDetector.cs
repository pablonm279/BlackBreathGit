using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// Detecta cuando el mouse pasa sobre un termino de juego (envuelto por Habilidad.TerminoDescripcion
// en un <link="skill-term:id">) dentro de un texto de descripcion, y muestra su definicion via
// TerminoHoverPopup. Se agrega automaticamente al mismo GameObject del TextMeshProUGUI que
// muestra la descripcion (ver BotonHabilidad.MostrarDescripcion y BattleManager.TryMostrarPanelDescripcionHabilidad).
[RequireComponent(typeof(TextMeshProUGUI))]
public class TerminoHoverDetector : MonoBehaviour, IPointerMoveHandler, IPointerExitHandler
{
  private const string PrefijoLink = "skill-term:";
  private const string PrefijoDado = "dice:";

  private TextMeshProUGUI texto;
  private string idTerminoActual;

  public static void AsegurarEn(TextMeshProUGUI destino)
  {
    if (destino == null)
    {
      return;
    }

    if (destino.gameObject.GetComponent<TerminoHoverDetector>() == null)
    {
      destino.gameObject.AddComponent<TerminoHoverDetector>();
    }
  }

  private void Awake()
  {
    texto = GetComponent<TextMeshProUGUI>();
  }

  private void OnDisable()
  {
    OcultarSiCorresponde();
  }

  public void OnPointerMove(PointerEventData eventData)
  {
    int indiceLink = TMP_TextUtilities.FindIntersectingLink(texto, eventData.position, eventData.enterEventCamera);
    if (indiceLink < 0)
    {
      OcultarSiCorresponde();
      return;
    }

    string linkId = texto.textInfo.linkInfo[indiceLink].GetLinkID();
    if (string.IsNullOrEmpty(linkId) || !linkId.StartsWith(PrefijoLink))
    {
      OcultarSiCorresponde();
      return;
    }

    string idTermino = linkId.Substring(PrefijoLink.Length);
    string descripcion;
    if (idTermino.StartsWith(PrefijoDado, StringComparison.Ordinal))
    {
      descripcion = GenerarDescripcionDado(idTermino.Substring(PrefijoDado.Length));
    }
    else
    {
      CatalogoTerminosJuego catalogo = CatalogoTerminosJuego.Instancia;
      TerminoJuegoDefinicion definicion = catalogo != null ? catalogo.ObtenerPorId(idTermino) : null;
      descripcion = definicion != null ? definicion.ObtenerDescripcion() : null;
    }

    if (string.IsNullOrEmpty(descripcion))
    {
      OcultarSiCorresponde();
      return;
    }

    idTerminoActual = idTermino;
    TerminoHoverPopup.Instancia.Mostrar(descripcion, eventData.position);
  }

  public void OnPointerExit(PointerEventData eventData)
  {
    OcultarSiCorresponde();
  }

  // Arma en el momento la descripcion de una notacion de dados generica (ej: "dice:2:6" -> 2d6),
  // ya que no seria practico tener una entrada de catalogo por cada combinacion posible.
  private static string GenerarDescripcionDado(string cantidadYCaras)
  {
    string[] partes = cantidadYCaras.Split(':');
    if (partes.Length != 2 || !int.TryParse(partes[0], out int cantidad) || !int.TryParse(partes[1], out int caras))
    {
      return null;
    }

    int idioma = TRADU.i != null ? TRADU.i.nIdioma : PlayerPrefs.GetInt("nIdioma", TRADU.IdiomaEspanol);
    if (idioma == TRADU.IdiomaIngles)
    {
      string palabraDado = cantidad == 1 ? "die" : "dice";
      return $"Roll of {cantidad} {palabraDado} with {caras} sides, summed to resolve certain mechanics, such as damage.";
    }

    if (idioma == TRADU.IdiomaPortugues)
    {
      string cantidadTextoPt = cantidad == 1 ? "1 dado" : $"{cantidad} dados";
      string verboPt = cantidad == 1 ? "soma" : "somam";
      return $"Rolagem de {cantidadTextoPt} de {caras} lados que {verboPt} o total para resolver certas mecânicas, como o dano.";
    }

    string cantidadTextoEs = cantidad == 1 ? "1 dado" : $"{cantidad} dados";
    string verboEs = cantidad == 1 ? "suma" : "suman";
    return $"Tirada de {cantidadTextoEs} de {caras} caras que {verboEs} el total para resolver determinadas mecánicas, como el daño.";
  }

  private void OcultarSiCorresponde()
  {
    if (idTerminoActual == null)
    {
      return;
    }

    idTerminoActual = null;
    TerminoHoverPopup.Instancia.Ocultar();
  }
}
