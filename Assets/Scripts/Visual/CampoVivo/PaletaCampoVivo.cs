using UnityEngine;

// Paletas del escenario, compartidas por todas las unidades y encuentros.
public struct PaletaCampoVivo
{
  public Color aire, motas, polvo, luz;
  public float viento, caida, densidad;
  public bool nieve, brasas;

  public static PaletaCampoVivo Resolver(EncounterZoneType zona, bool subterraneo, bool noche, bool bosqueQuemado)
  {
    PaletaCampoVivo p;
    if (subterraneo || zona == EncounterZoneType.Subterraneo)
      p = new PaletaCampoVivo { aire = new Color(.29f,.43f,.48f), motas = new Color(.57f,.8f,.83f), polvo = new Color(.43f,.48f,.5f), luz = new Color(.4f,.65f,.7f), viento = .055f, caida = -.035f, densidad = .6f };
    else if (zona == EncounterZoneType.PasoVientoHelado)
      p = new PaletaCampoVivo { aire = new Color(.56f,.7f,.79f), motas = new Color(.84f,.93f,1f), polvo = new Color(.69f,.83f,.92f), luz = new Color(.56f,.74f,.88f), viento = .42f, caida = -.3f, densidad = 1.35f, nieve = true };
    else if (zona == EncounterZoneType.Nedukazal)
      p = new PaletaCampoVivo { aire = new Color(.57f,.44f,.3f), motas = new Color(.9f,.71f,.44f), polvo = new Color(.65f,.49f,.32f), luz = new Color(.93f,.7f,.39f), viento = .2f, caida = -.08f, densidad = .85f };
    else if (bosqueQuemado)
      p = new PaletaCampoVivo { aire = new Color(.38f,.36f,.32f), motas = new Color(1f,.39f,.095f), polvo = new Color(.46f,.4f,.32f), luz = new Color(.7f,.52f,.34f), viento = .15f, caida = .13f, densidad = .85f, brasas = true };
    else
      p = new PaletaCampoVivo { aire = new Color(.35f,.45f,.4f), motas = new Color(.74f,.8f,.49f), polvo = new Color(.45f,.43f,.3f), luz = new Color(.7f,.76f,.47f), viento = .13f, caida = -.07f, densidad = .8f };

    if (noche)
    {
      p.aire = Color.Lerp(p.aire, new Color(.23f,.32f,.44f), .5f);
      p.polvo *= new Color(.75f,.84f,.95f,1f);
      p.luz = Color.Lerp(p.luz, new Color(.35f,.52f,.75f), .55f);
    }
    return p;
  }
}
