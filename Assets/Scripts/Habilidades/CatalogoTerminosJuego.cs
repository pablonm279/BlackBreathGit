using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TerminoJuegoDefinicion
{
  [Tooltip("Debe coincidir con un id de Habilidad.TerminoDescripcionId (ej: armor-penetration).")]
  public string id;

  [Tooltip("Color hexadecimal usado para resaltar este termino en las descripciones (ej: #b9c6cc). Vacio = usa el color por defecto definido en Habilidad.cs.")]
  public string colorHex;

  [TextArea(2, 6)] public string descripcionEs;
  [TextArea(2, 6)] public string descripcionEn;
  [TextArea(2, 6)] public string descripcionPt;

  public string ObtenerDescripcion()
  {
    int idioma = TRADU.i != null ? TRADU.i.nIdioma : PlayerPrefs.GetInt("nIdioma", TRADU.IdiomaEspanol);
    if (idioma == TRADU.IdiomaIngles && !string.IsNullOrEmpty(descripcionEn))
    {
      return descripcionEn;
    }

    if (idioma == TRADU.IdiomaPortugues && !string.IsNullOrEmpty(descripcionPt))
    {
      return descripcionPt;
    }

    return descripcionEs;
  }
}

[CreateAssetMenu(menuName = "GDD/Habilidades/Catalogo de Terminos de Juego", fileName = "CatalogoTerminosJuego")]
public class CatalogoTerminosJuego : ScriptableObject
{
  private const string RutaResources = "Habilidades/CatalogoTerminosJuego";

  private static CatalogoTerminosJuego _instancia;
  private static bool _instanciaCargada;

  // Punto unico de carga del catalogo (via Resources.Load), compartido por el resaltador
  // de colores en Habilidad.cs y por el detector de hover de terminos.
  public static CatalogoTerminosJuego Instancia
  {
    get
    {
      if (!_instanciaCargada)
      {
        _instancia = Resources.Load<CatalogoTerminosJuego>(RutaResources);
        _instanciaCargada = true;
      }

      return _instancia;
    }
  }

  public List<TerminoJuegoDefinicion> terminos = new List<TerminoJuegoDefinicion>();

  private Dictionary<string, TerminoJuegoDefinicion> _porId;

  public TerminoJuegoDefinicion ObtenerPorId(string id)
  {
    if (string.IsNullOrEmpty(id))
    {
      return null;
    }

    if (_porId == null)
    {
      _porId = new Dictionary<string, TerminoJuegoDefinicion>();
      for (int i = 0; i < terminos.Count; i++)
      {
        TerminoJuegoDefinicion termino = terminos[i];
        if (termino != null && !string.IsNullOrEmpty(termino.id) && !_porId.ContainsKey(termino.id))
        {
          _porId[termino.id] = termino;
        }
      }
    }

    _porId.TryGetValue(id, out TerminoJuegoDefinicion resultado);
    return resultado;
  }
}
