using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class botoneraNews : MonoBehaviour
{
  private const string NombreBotonContinuar = "bt_Continuar";
  private const string NombreBotonCargarPartida = "bt_CargarPartida";
  private const string NombreBotonNuevaPartida = "bt_NuevaPartida";
  private const string UrlDiscord = "https://discord.gg/dZTkFGAU4z";
  private const string UrlX = "https://x.com/BlackBreathGame";
  private const string UrlWishlist = "https://store.steampowered.com/app/4227530/The_Black_Breath/";
  private const string UrlForms = "https://docs.google.com/forms/d/e/1FAIpQLScw6OQLZtVQs1ESOWW7UONEKQuufsNh5XB4mi5S2J5wI4sdeQ/viewform";

  private const string UrlYoutube = "https://www.youtube.com/@TheBlackBreathGame";
  private static readonly Color ColorBotonDeshabilitado = new Color(0.4f, 0.55f, 0.4f, 1f);
  private static readonly Color ColorTextoDeshabilitado = new Color(0.47f, 0.47f, 0.47f, 1f);

  private Button botonContinuar;
  private Button botonNuevaPartida;
  private readonly Dictionary<TMP_Text, Color> coloresTextoOriginales = new Dictionary<TMP_Text, Color>();

  void Awake()
  {
    RefrescarBotonesBloqueables();
  }

  void OnEnable()
  {
    RefrescarBotonesBloqueables();
  }

  public void RefrescarBotonesBloqueables()
  {
    RefrescarBotonContinuar();
    RefrescarBotonNuevaPartida();
  }

  public void RefrescarBotonContinuar()
  {
    if (botonContinuar == null)
    {
      botonContinuar = BuscarBotonContinuar();
    }

    if (botonContinuar == null)
    {
      return;
    }

    bool haySave = SaveGameService.HasSaveFile();
    botonContinuar.gameObject.SetActive(true);
    botonContinuar.interactable = haySave;
    AplicarEstadoVisualDeshabilitado(botonContinuar, !haySave);
  }

  public void RefrescarBotonNuevaPartida()
  {
    if (botonNuevaPartida == null)
    {
      botonNuevaPartida = BuscarBotonNuevaPartida();
    }

    if (botonNuevaPartida == null)
    {
      return;
    }

    bool tutorialIniciado = MenuController.TutorialFueIniciado();
    botonNuevaPartida.gameObject.SetActive(true);
    botonNuevaPartida.interactable = tutorialIniciado;
    AplicarEstadoVisualDeshabilitado(botonNuevaPartida, !tutorialIniciado);
  }

  public void Continuar()
  {
    if (!SaveGameService.HasSaveFile())
    {
      RefrescarBotonContinuar();
      return;
    }

    MenuController menuController = FindFirstObjectByType<MenuController>(FindObjectsInactive.Include);
    if (menuController != null)
    {
      menuController.OnContinuarPartida();
    }
  }

  public void NuevaPartida()
  {
    if (!MenuController.TutorialFueIniciado())
    {
      RefrescarBotonNuevaPartida();
      return;
    }

    MenuController menuController = FindFirstObjectByType<MenuController>(FindObjectsInactive.Include);
    if (menuController != null)
    {
      menuController.OnNuevaPartida();
    }
  }

  public void AbrirDiscord()
  {
    Application.OpenURL(UrlDiscord);
  }

  public void AbrirX()
  {
    Application.OpenURL(UrlX);
  }

  public void AbrirYoutube()
  {
    Application.OpenURL(UrlYoutube);
  }
  public void AbrirWishlist()
  {
    Application.OpenURL(UrlWishlist);
  }
  public void AbrirForms()
  {
    Application.OpenURL(UrlForms);
  }

  private Button BuscarBotonContinuar()
  {
    Button[] botones = GetComponentsInChildren<Button>(true);
    for (int i = 0; i < botones.Length; i++)
    {
      Button boton = botones[i];
      if (boton != null && boton.name == NombreBotonContinuar)
      {
        return boton;
      }
    }

    for (int i = 0; i < botones.Length; i++)
    {
      Button boton = botones[i];
      if (boton != null && boton.name == NombreBotonCargarPartida)
      {
        return boton;
      }
    }

    for (int i = 0; i < botones.Length; i++)
    {
      Button boton = botones[i];
      if (boton != null && TieneTextoContinuar(boton))
      {
        return boton;
      }
    }

    return null;
  }

  private void AplicarEstadoVisualDeshabilitado(Button boton, bool deshabilitado)
  {
    if (boton == null)
    {
      return;
    }

    ColorBlock colores = boton.colors;
    colores.disabledColor = ColorBotonDeshabilitado;
    boton.colors = colores;

    TMP_Text texto = boton.GetComponentInChildren<TMP_Text>(true);
    if (texto == null)
    {
      return;
    }

    if (!coloresTextoOriginales.ContainsKey(texto))
    {
      coloresTextoOriginales[texto] = texto.color;
    }

    texto.color = deshabilitado ? ColorTextoDeshabilitado : coloresTextoOriginales[texto];
  }

  private Button BuscarBotonNuevaPartida()
  {
    Button[] botones = GetComponentsInChildren<Button>(true);
    for (int i = 0; i < botones.Length; i++)
    {
      Button boton = botones[i];
      if (boton != null && boton.name == NombreBotonNuevaPartida)
      {
        return boton;
      }
    }

    for (int i = 0; i < botones.Length; i++)
    {
      Button boton = botones[i];
      if (boton != null && TieneTextoNuevaPartida(boton))
      {
        return boton;
      }
    }

    return null;
  }

  private static bool TieneTextoContinuar(Button boton)
  {
    TMP_Text texto = boton.GetComponentInChildren<TMP_Text>(true);
    if (texto == null || string.IsNullOrWhiteSpace(texto.text))
    {
      return false;
    }

    string contenido = texto.text.Trim();
    return contenido == "Continuar"
      || contenido == "Cargar Partida"
      || contenido == "Continue"
      || contenido == "Load Game"
      || contenido == "Carregar Partida";
  }

  private static bool TieneTextoNuevaPartida(Button boton)
  {
    TMP_Text texto = boton.GetComponentInChildren<TMP_Text>(true);
    if (texto == null || string.IsNullOrWhiteSpace(texto.text))
    {
      return false;
    }

    string contenido = texto.text.Trim();
    return contenido == "Nueva Partida"
      || contenido == "New Game"
      || contenido == "Novo Jogo";
  }
}
