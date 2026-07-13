using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum EncounterZoneType
{
   BosqueAngustiante,
   PasoVientoHelado,
   Nedukazal,
   Generico,
   Subterraneo
}

public enum BattleEncounterType
{
   Normal,
   Elite,
   AtaqueCaravana,
   Subterraneo
}

[Serializable]
public class EnemyTierPool
{
   public List<GameObject> tier1 = new List<GameObject>();
   public List<GameObject> tier2 = new List<GameObject>();
   public List<GameObject> tier3 = new List<GameObject>();
   public List<GameObject> tier4 = new List<GameObject>();
   public List<GameObject> tier5 = new List<GameObject>();
}

[Serializable]
public class EnemyFactionConfig
{
   public string factionId;
   public string displayName;
   public EnemyTierPool tiers = new EnemyTierPool();
}

[Serializable]
public class BattleFactionPool
{
   public BattleEncounterType battleType;
   public List<EnemyFactionConfig> factions = new List<EnemyFactionConfig>();
}

[Serializable]
public class EncounterZoneConfig
{
   public string inspectorLabel;
   [Range(0f, 100f)] public float chanceEncuentroPropio = 70f;
   public List<BattleFactionPool> battlePools = new List<BattleFactionPool>();

   public BattleFactionPool GetPool(BattleEncounterType type)
   {
      return battlePools.Find(pool => pool != null && pool.battleType == type);
   }
}

[Serializable]
public class TexturaProceduralPasoVientoHeladoConfig
{
   [Header("Salida")]
   [Tooltip("+ mas resolucion y detalle, - menos costo de memoria.")]
   [Range(128, 2048)] public int tamanoTextura = 1024;
   [Tooltip("+ mas claro/teñido el material final, - mas oscuro o neutro.")]
   public Color tintMaterial = new Color(0.94f, 0.98f, 1f, 1f);

   [Header("Colores")]
   [Tooltip("Color principal de la nieve. Mas claro aclara toda la base, mas oscuro ensucia el suelo.")]
   public Color nieveBase = new Color(0.69f, 0.81f, 0.85f, 1f);
   [Tooltip("Color de los parches claros. Mas claro da nieve fresca, mas oscuro reduce brillo.")]
   public Color nieveClara = new Color(0.82f, 0.91f, 0.94f, 1f);
   [Tooltip("Color de manchas grises. Mas oscuro marca nieve pisada, mas claro suaviza manchas.")]
   public Color grisCompactado = new Color(0.45f, 0.56f, 0.61f, 1f);
   [Tooltip("Color de zonas heladas. Mas saturado/azul suma hielo, mas neutro lo disimula.")]
   public Color hieloLavado = new Color(0.53f, 0.73f, 0.80f, 1f);
   [Tooltip("Color de sombras suaves. Mas oscuro aumenta contraste, mas claro aplana la textura.")]
   public Color sombraSuave = new Color(0.36f, 0.45f, 0.50f, 1f);

   [Header("Escalas de ruido")]
   [Tooltip("+ deforma mas las manchas, - deja patrones mas rectos y limpios.")]
   [Range(0f, 0.25f)] public float intensidadWarp = 0.07f;
   [Tooltip("+ mas ondulaciones horizontales chicas, - manchas mas anchas en X.")]
   [Range(0.1f, 12f)] public float escalaWarpX = 3.1f;
   [Tooltip("+ mas ondulaciones verticales chicas, - manchas mas anchas en Y.")]
   [Range(0.1f, 12f)] public float escalaWarpY = 3.4f;
   [Tooltip("+ manchas grandes mas chicas y repetidas, - manchas grandes mas amplias.")]
   [Range(0.1f, 24f)] public float escalaManchasGrandes = 5.2f;
   [Tooltip("+ hielo medio mas picado, - placas de hielo mas grandes.")]
   [Range(0.1f, 48f)] public float escalaHieloMedio = 13.5f;
   [Tooltip("+ grano fino mas cerrado, - textura fina mas amplia y suave.")]
   [Range(1f, 96f)] public float escalaNieveFina = 48f;
   [Tooltip("+ vetas mas juntas, - vetas mas largas y separadas.")]
   [Range(0.1f, 64f)] public float escalaVetas = 23f;
   [Tooltip("+ vetas mas cortadas en Y, - vetas mas estiradas y horizontales.")]
   [Range(0.1f, 16f)] public float escalaVetasY = 3f;

   [Header("Intensidades")]
   [Tooltip("+ mas parches de nieve clara, - base mas uniforme.")]
   [Range(0f, 1f)] public float intensidadNieveClara = 0.18f;
   [Tooltip("+ mas manchas grises/pisadas, - suelo mas limpio.")]
   [Range(0f, 1f)] public float intensidadGris = 0.42f;
   [Tooltip("+ mas presencia de hielo lavado, - menos tonos celestes.")]
   [Range(0f, 1f)] public float intensidadHielo = 0.26f;
   [Tooltip("+ mas lineas/vetas visibles, - superficie mas lisa.")]
   [Range(0f, 1f)] public float intensidadVetas = 0.18f;
   [Tooltip("+ mas ruido fino, - textura mas plana y suave.")]
   [Range(0f, 0.15f)] public float intensidadGrano = 0.035f;

   public int GetCacheHash()
   {
      unchecked
      {
         int hash = 17;
         hash = hash * 31 + tamanoTextura;
         hash = hash * 31 + HashColor(tintMaterial);
         hash = hash * 31 + HashColor(nieveBase);
         hash = hash * 31 + HashColor(nieveClara);
         hash = hash * 31 + HashColor(grisCompactado);
         hash = hash * 31 + HashColor(hieloLavado);
         hash = hash * 31 + HashColor(sombraSuave);
         hash = hash * 31 + HashFloat(intensidadWarp);
         hash = hash * 31 + HashFloat(escalaWarpX);
         hash = hash * 31 + HashFloat(escalaWarpY);
         hash = hash * 31 + HashFloat(escalaManchasGrandes);
         hash = hash * 31 + HashFloat(escalaHieloMedio);
         hash = hash * 31 + HashFloat(escalaNieveFina);
         hash = hash * 31 + HashFloat(escalaVetas);
         hash = hash * 31 + HashFloat(escalaVetasY);
         hash = hash * 31 + HashFloat(intensidadNieveClara);
         hash = hash * 31 + HashFloat(intensidadGris);
         hash = hash * 31 + HashFloat(intensidadHielo);
         hash = hash * 31 + HashFloat(intensidadVetas);
         hash = hash * 31 + HashFloat(intensidadGrano);
         return hash;
      }
   }

   static int HashFloat(float value)
   {
      return Mathf.RoundToInt(value * 10000f);
   }

   static int HashColor(Color color)
   {
      unchecked
      {
         int hash = 17;
         hash = hash * 31 + HashFloat(color.r);
         hash = hash * 31 + HashFloat(color.g);
         hash = hash * 31 + HashFloat(color.b);
         hash = hash * 31 + HashFloat(color.a);
         return hash;
      }
   }
}

[Serializable]
public class TexturaProceduralBosqueAngustianteConfig
{
   [Header("Salida")]
   [Tooltip("+ mas resolucion y detalle, - menos costo de memoria.")]
   [Range(128, 2048)] public int tamanoTextura = 1024;
   [Tooltip("+ mas claro/teñido el material final, - mas oscuro o neutro.")]
   public Color tintMaterial = new Color(1f, 0.96f, 0.90f, 1f);

   [Header("Colores")]
   [Tooltip("Color principal del suelo quemado. Mas claro levanta el terreno, mas oscuro lo apaga.")]
   public Color tierraQuemadaBase = new Color(0.22f, 0.20f, 0.17f, 1f);
   [Tooltip("Color de ceniza clara. Mas claro genera parches secos visibles, mas oscuro los integra.")]
   public Color cenizaClara = new Color(0.43f, 0.41f, 0.36f, 1f);
   [Tooltip("Color de manchas carbonizadas. Mas oscuro marca quemado fuerte, mas claro suaviza.")]
   public Color carbonOscuro = new Color(0.08f, 0.07f, 0.06f, 1f);
   [Tooltip("Color de tierra rojiza. Mas rojo/calido suma calor residual, mas neutro reduce contraste.")]
   public Color tierraRojiza = new Color(0.32f, 0.15f, 0.09f, 1f);
   [Tooltip("Color de brasas apagadas. Mas saturado deja puntos calidos, mas oscuro los vuelve hollin.")]
   public Color brasaApagada = new Color(0.55f, 0.18f, 0.05f, 1f);

   [Header("Escalas de ruido")]
   [Tooltip("+ deforma mas las manchas, - deja patrones mas rectos y limpios.")]
   [Range(0f, 0.25f)] public float intensidadWarp = 0.08f;
   [Tooltip("+ mas ondulaciones horizontales chicas, - manchas mas anchas en X.")]
   [Range(0.1f, 12f)] public float escalaWarpX = 2.7f;
   [Tooltip("+ mas ondulaciones verticales chicas, - manchas mas anchas en Y.")]
   [Range(0.1f, 12f)] public float escalaWarpY = 3.2f;
   [Tooltip("+ manchas grandes mas chicas y repetidas, - quemados grandes mas amplios.")]
   [Range(0.1f, 24f)] public float escalaManchasGrandes = 4.4f;
   [Tooltip("+ ceniza media mas picada, - placas de ceniza mas amplias.")]
   [Range(0.1f, 48f)] public float escalaCenizaMedia = 11.5f;
   [Tooltip("+ grano fino mas cerrado, - textura fina mas suave.")]
   [Range(1f, 96f)] public float escalaGrano = 44f;
   [Tooltip("+ vetas quemadas mas juntas, - vetas mas largas y separadas.")]
   [Range(0.1f, 64f)] public float escalaVetasQuemadas = 18f;
   [Tooltip("+ vetas mas cortadas en Y, - vetas mas estiradas.")]
   [Range(0.1f, 16f)] public float escalaVetasY = 4f;

   [Header("Intensidades")]
   [Tooltip("+ mas ceniza clara, - base mas tierra quemada.")]
   [Range(0f, 1f)] public float intensidadCeniza = 0.34f;
   [Tooltip("+ mas carbon oscuro, - suelo menos manchado.")]
   [Range(0f, 1f)] public float intensidadCarbon = 0.46f;
   [Tooltip("+ mas zonas rojizas, - paleta mas fria/neutra.")]
   [Range(0f, 1f)] public float intensidadRojiza = 0.22f;
   [Tooltip("+ mas puntos calidos tipo brasa apagada, - menos acentos naranjas.")]
   [Range(0f, 1f)] public float intensidadBrasas = 0.10f;
   [Tooltip("+ mas lineas/vetas quemadas, - superficie mas lisa.")]
   [Range(0f, 1f)] public float intensidadVetas = 0.18f;
   [Tooltip("+ mas ruido fino, - textura mas plana y suave.")]
   [Range(0f, 0.15f)] public float intensidadGrano = 0.04f;

   public int GetCacheHash()
   {
      unchecked
      {
         int hash = 17;
         hash = hash * 31 + tamanoTextura;
         hash = hash * 31 + HashColor(tintMaterial);
         hash = hash * 31 + HashColor(tierraQuemadaBase);
         hash = hash * 31 + HashColor(cenizaClara);
         hash = hash * 31 + HashColor(carbonOscuro);
         hash = hash * 31 + HashColor(tierraRojiza);
         hash = hash * 31 + HashColor(brasaApagada);
         hash = hash * 31 + HashFloat(intensidadWarp);
         hash = hash * 31 + HashFloat(escalaWarpX);
         hash = hash * 31 + HashFloat(escalaWarpY);
         hash = hash * 31 + HashFloat(escalaManchasGrandes);
         hash = hash * 31 + HashFloat(escalaCenizaMedia);
         hash = hash * 31 + HashFloat(escalaGrano);
         hash = hash * 31 + HashFloat(escalaVetasQuemadas);
         hash = hash * 31 + HashFloat(escalaVetasY);
         hash = hash * 31 + HashFloat(intensidadCeniza);
         hash = hash * 31 + HashFloat(intensidadCarbon);
         hash = hash * 31 + HashFloat(intensidadRojiza);
         hash = hash * 31 + HashFloat(intensidadBrasas);
         hash = hash * 31 + HashFloat(intensidadVetas);
         hash = hash * 31 + HashFloat(intensidadGrano);
         return hash;
      }
   }

   static int HashFloat(float value)
   {
      return Mathf.RoundToInt(value * 10000f);
   }

   static int HashColor(Color color)
   {
      unchecked
      {
         int hash = 17;
         hash = hash * 31 + HashFloat(color.r);
         hash = hash * 31 + HashFloat(color.g);
         hash = hash * 31 + HashFloat(color.b);
         hash = hash * 31 + HashFloat(color.a);
         return hash;
      }
   }
}

public static class TexturaProceduralSueloZona
{
   const int TamanoMinimo = 128;
   const int TamanoMaximo = 2048;

   static readonly Dictionary<string, Texture2D> texturasCache = new Dictionary<string, Texture2D>();

   public static Texture2D CrearTexturaSuelo(EncounterZoneType zona, int seed, int size = 1024)
   {
      return CrearTexturaSuelo(zona, seed, (TexturaProceduralPasoVientoHeladoConfig)null, size);
   }

   public static Texture2D CrearTexturaSuelo(EncounterZoneType zona, int seed, TexturaProceduralPasoVientoHeladoConfig pasoConfig, int size = 1024)
   {
      int tamanoSolicitado = pasoConfig != null ? pasoConfig.tamanoTextura : size;
      int tamano = Mathf.Clamp(tamanoSolicitado, TamanoMinimo, TamanoMaximo);
      int configHash = pasoConfig != null ? pasoConfig.GetCacheHash() : 0;
      string cacheKey = $"{zona}_paso_{seed}_{tamano}_{configHash}";

      if (texturasCache.TryGetValue(cacheKey, out Texture2D texturaCacheada) && texturaCacheada != null)
      {
         return texturaCacheada;
      }

      Texture2D textura = zona == EncounterZoneType.PasoVientoHelado
         ? CrearTexturaPasoVientoHelado(seed, tamano, pasoConfig)
         : CrearTexturaBaseSuave(seed, tamano);

      texturasCache[cacheKey] = textura;
      return textura;
   }

   public static Texture2D CrearTexturaSuelo(EncounterZoneType zona, int seed, TexturaProceduralBosqueAngustianteConfig bosqueConfig, int size = 1024)
   {
      int tamanoSolicitado = bosqueConfig != null ? bosqueConfig.tamanoTextura : size;
      int tamano = Mathf.Clamp(tamanoSolicitado, TamanoMinimo, TamanoMaximo);
      int configHash = bosqueConfig != null ? bosqueConfig.GetCacheHash() : 0;
      string cacheKey = $"{zona}_bosque_{seed}_{tamano}_{configHash}";

      if (texturasCache.TryGetValue(cacheKey, out Texture2D texturaCacheada) && texturaCacheada != null)
      {
         return texturaCacheada;
      }

      Texture2D textura = zona == EncounterZoneType.BosqueAngustiante
         ? CrearTexturaBosqueAngustiante(seed, tamano, bosqueConfig)
         : CrearTexturaBaseSuave(seed, tamano);

      texturasCache[cacheKey] = textura;
      return textura;
   }

   public static void AplicarTexturaSueloZona(MeshRenderer renderer, Texture2D textura, Color tint)
   {
      if (renderer == null || textura == null)
      {
         return;
      }

      Material material = renderer.material;
      if (material == null)
      {
         return;
      }

      if (material.HasProperty("_MainTex"))
      {
         material.SetTexture("_MainTex", textura);
      }

      if (material.HasProperty("_BaseMap"))
      {
         material.SetTexture("_BaseMap", textura);
      }

      if (material.HasProperty("_Color"))
      {
         material.SetColor("_Color", tint);
      }

      if (material.HasProperty("_BaseColor"))
      {
         material.SetColor("_BaseColor", tint);
      }
   }

   static Texture2D CrearTexturaPasoVientoHelado(int seed, int size, TexturaProceduralPasoVientoHeladoConfig config)
   {
      if (config == null)
      {
         config = new TexturaProceduralPasoVientoHeladoConfig();
      }

      Texture2D textura = CrearTexturaVacia("TexturaProcedural_PasoVientoHelado", size);
      Color32[] pixels = new Color32[size * size];

      float seedA = (seed & 0x7fffffff) * 0.00037f;
      float seedB = ((seed >> 8) & 0x7fffffff) * 0.00053f;

      for (int y = 0; y < size; y++)
      {
         float ny = y / (float)(size - 1);
         for (int x = 0; x < size; x++)
         {
            float nx = x / (float)(size - 1);
            float warpX = RuidoFirmado(nx * config.escalaWarpX + seedA, ny * config.escalaWarpX - seedB) * config.intensidadWarp;
            float warpY = RuidoFirmado(nx * config.escalaWarpY - seedB, ny * config.escalaWarpY + seedA) * config.intensidadWarp;
            float ux = nx + warpX;
            float uy = ny + warpY;

            float manchasGrandes = Mathf.PerlinNoise(ux * config.escalaManchasGrandes + seedA, uy * config.escalaManchasGrandes - seedB);
            float hieloMedio = Mathf.PerlinNoise(ux * config.escalaHieloMedio - seedB * 0.7f, uy * (config.escalaHieloMedio * 0.95f) + seedA * 0.8f);
            float nieveFina = Mathf.PerlinNoise(ux * config.escalaNieveFina + seedB, uy * (config.escalaNieveFina * 0.98f) - seedA);
            float vetaLarga = Mathf.PerlinNoise((ux + uy * 0.18f) * config.escalaVetas + seedA * 0.4f, uy * config.escalaVetasY - seedB * 0.4f);

            Color color = Color.Lerp(config.nieveBase, config.nieveClara, Mathf.InverseLerp(0.15f, 0.85f, nieveFina) * config.intensidadNieveClara);

            float pesoGris = Mathf.SmoothStep(0.46f, 0.86f, manchasGrandes) * config.intensidadGris;
            color = Color.Lerp(color, config.grisCompactado, pesoGris);

            float pesoHielo = Mathf.SmoothStep(0.58f, 0.91f, hieloMedio) * config.intensidadHielo;
            color = Color.Lerp(color, config.hieloLavado, pesoHielo);

            float pesoVeta = Mathf.SmoothStep(0.72f, 0.95f, vetaLarga) * config.intensidadVetas;
            color = Color.Lerp(color, config.sombraSuave, pesoVeta);

            float grano = (nieveFina - 0.5f) * config.intensidadGrano;
            color.r = Mathf.Clamp01(color.r + grano);
            color.g = Mathf.Clamp01(color.g + grano);
            color.b = Mathf.Clamp01(color.b + grano);

            pixels[y * size + x] = color;
         }
      }

      textura.SetPixels32(pixels);
      textura.Apply(true, true);
      return textura;
   }

   static Texture2D CrearTexturaBosqueAngustiante(int seed, int size, TexturaProceduralBosqueAngustianteConfig config)
   {
      if (config == null)
      {
         config = new TexturaProceduralBosqueAngustianteConfig();
      }

      Texture2D textura = CrearTexturaVacia("TexturaProcedural_BosqueAngustiante", size);
      Color32[] pixels = new Color32[size * size];

      float seedA = (seed & 0x7fffffff) * 0.00043f;
      float seedB = ((seed >> 8) & 0x7fffffff) * 0.00059f;

      for (int y = 0; y < size; y++)
      {
         float ny = y / (float)(size - 1);
         for (int x = 0; x < size; x++)
         {
            float nx = x / (float)(size - 1);
            float warpX = RuidoFirmado(nx * config.escalaWarpX + seedA, ny * config.escalaWarpX - seedB) * config.intensidadWarp;
            float warpY = RuidoFirmado(nx * config.escalaWarpY - seedB, ny * config.escalaWarpY + seedA) * config.intensidadWarp;
            float ux = nx + warpX;
            float uy = ny + warpY;

            float manchasGrandes = Mathf.PerlinNoise(ux * config.escalaManchasGrandes + seedA, uy * config.escalaManchasGrandes - seedB);
            float cenizaMedia = Mathf.PerlinNoise(ux * config.escalaCenizaMedia - seedB * 0.6f, uy * (config.escalaCenizaMedia * 0.9f) + seedA * 0.7f);
            float grano = Mathf.PerlinNoise(ux * config.escalaGrano + seedB, uy * (config.escalaGrano * 1.07f) - seedA);
            float veta = Mathf.PerlinNoise((ux - uy * 0.12f) * config.escalaVetasQuemadas + seedA * 0.35f, uy * config.escalaVetasY - seedB * 0.35f);
            float brasa = Mathf.PerlinNoise(ux * (config.escalaCenizaMedia * 1.9f) + seedA * 2.1f, uy * (config.escalaCenizaMedia * 1.7f) - seedB * 1.8f);

            Color color = config.tierraQuemadaBase;

            float pesoCeniza = Mathf.SmoothStep(0.52f, 0.88f, cenizaMedia) * config.intensidadCeniza;
            color = Color.Lerp(color, config.cenizaClara, pesoCeniza);

            float pesoCarbon = Mathf.SmoothStep(0.42f, 0.84f, manchasGrandes) * config.intensidadCarbon;
            color = Color.Lerp(color, config.carbonOscuro, pesoCarbon);

            float pesoRojizo = Mathf.SmoothStep(0.58f, 0.90f, 1f - manchasGrandes) * config.intensidadRojiza;
            color = Color.Lerp(color, config.tierraRojiza, pesoRojizo);

            float pesoVeta = Mathf.SmoothStep(0.74f, 0.96f, veta) * config.intensidadVetas;
            color = Color.Lerp(color, config.carbonOscuro, pesoVeta);

            float pesoBrasa = Mathf.SmoothStep(0.91f, 0.995f, brasa) * Mathf.SmoothStep(0.45f, 0.85f, manchasGrandes) * config.intensidadBrasas;
            color = Color.Lerp(color, config.brasaApagada, pesoBrasa);

            float granoFirmado = (grano - 0.5f) * config.intensidadGrano;
            color.r = Mathf.Clamp01(color.r + granoFirmado);
            color.g = Mathf.Clamp01(color.g + granoFirmado);
            color.b = Mathf.Clamp01(color.b + granoFirmado);

            pixels[y * size + x] = color;
         }
      }

      textura.SetPixels32(pixels);
      textura.Apply(true, true);
      return textura;
   }

   static Texture2D CrearTexturaBaseSuave(int seed, int size)
   {
      Texture2D textura = CrearTexturaVacia("TexturaProcedural_SueloZona", size);
      Color32[] pixels = new Color32[size * size];
      float seedA = (seed & 0x7fffffff) * 0.00041f;

      for (int y = 0; y < size; y++)
      {
         float ny = y / (float)(size - 1);
         for (int x = 0; x < size; x++)
         {
            float nx = x / (float)(size - 1);
            float ruido = Mathf.PerlinNoise(nx * 8f + seedA, ny * 8f - seedA);
            float valor = Mathf.Lerp(0.82f, 0.96f, ruido);
            pixels[y * size + x] = new Color(valor, valor, valor, 1f);
         }
      }

      textura.SetPixels32(pixels);
      textura.Apply(true, true);
      return textura;
   }

   static Texture2D CrearTexturaVacia(string nombre, int size)
   {
      Texture2D textura = new Texture2D(size, size, TextureFormat.RGBA32, true, false);
      textura.name = nombre;
      textura.wrapMode = TextureWrapMode.Repeat;
      textura.filterMode = FilterMode.Trilinear;
      textura.anisoLevel = 4;
      return textura;
   }

   static float RuidoFirmado(float x, float y)
   {
      return (Mathf.PerlinNoise(x, y) - 0.5f) * 2f;
   }
}

public class AtributosZona : MonoBehaviour
{
   bool restaurandoDesdeSave;
   public bool DecoracionZonaEnCurso { get; private set; }
   public string Nombre;
   public int ID; //1 Bosque Ardiente, 2 Paso Vientohelado, 3 Nedukazal

   public TextMeshProUGUI txtNombreZona;
   public int FASE; //En que posición sale la zona, para determinar dificultad de encuentros
   public int modRecoleccionMateriales;
   public int modRecoleccionSuministros;
   public int modChanceEmboscada;

   public int modChanceExploracion;

   public int Clima_chances_Sol;
   public int Clima_chances_Calor;
   public int Clima_chances_Lluvia;
   public int Clima_chances_Nieve;
   public int Clima_chances_Niebla;
   public int Clima_chances_EspecialZona1;
   public int Clima_chances_EspecialZona2;
   public int PasoVientoHelado_FuerzaKaleTav = 0;

   [Header("Encuentros dinámicos")]
   public EncounterZoneConfig bosqueAngustianteEncuentros = new EncounterZoneConfig();
   public EncounterZoneConfig pasoVientoHeladoEncuentros = new EncounterZoneConfig();
   public EncounterZoneConfig NedukazalEncuentros = new EncounterZoneConfig();

   public EncounterZoneConfig genericosEncuentros = new EncounterZoneConfig();
   public EncounterZoneConfig subterraneosEncuentros = new EncounterZoneConfig();

   [Header("Debug de encuentros")]
   public List<GameObject> debugEncounterUnits = new List<GameObject>();

   [Header("Textura procedural suelo - Bosque Ardiente")]
   [SerializeField, Tooltip("Activa o desactiva la capa procedural de manchas del suelo del Bosque Ardiente.")]
   bool usarTexturaProceduralBosqueAngustiante = true;
   [SerializeField] TexturaProceduralBosqueAngustianteConfig texturaSueloBosqueAngustiante = new TexturaProceduralBosqueAngustianteConfig();

   [Header("Textura procedural suelo - Paso Viento Helado")]
   [SerializeField] TexturaProceduralPasoVientoHeladoConfig texturaSueloPasoVientoHelado = new TexturaProceduralPasoVientoHeladoConfig();

   [Header("Hielo visible - Paso Viento Helado")]
   [SerializeField, Tooltip("Si esta activo, los adornos no se generan donde el relieve queda a la altura del hielo o por debajo.")]
   bool evitarAdornosSobreHieloPasoVientoHelado = true;
   [SerializeField, Tooltip("Altura Y del plano de hielo. El decorador descarta puntos con superficie <= este valor.")]
   float alturaHieloPasoVientoHelado = -0.7f;

   [Header("Distribucion adornos - Paso Viento Helado")]
   [SerializeField, Tooltip("Si esta activo, cada tanda de adornos empieza en una posicion aleatoria en vez de forzar el centro.")]
   bool evitarPrimerPuntoCentricoPasoVientoHelado = true;

   MapDecorator scMapDecorator;

   void Awake()
   {
      scMapDecorator = GetComponent<MapDecorator>();
      EnsureTexturaProceduralDefaults();
      EnsureEncounterLabels();
   }

   void OnValidate()
   {
      EnsureTexturaProceduralDefaults();
      EnsureEncounterLabels();
   }

   void EnsureTexturaProceduralDefaults()
   {
      if (texturaSueloBosqueAngustiante == null)
      {
         texturaSueloBosqueAngustiante = new TexturaProceduralBosqueAngustianteConfig();
      }

      if (texturaSueloPasoVientoHelado == null)
      {
         texturaSueloPasoVientoHelado = new TexturaProceduralPasoVientoHeladoConfig();
      }
   }

   void EnsureEncounterLabels()
   {
      if (bosqueAngustianteEncuentros != null && string.IsNullOrWhiteSpace(bosqueAngustianteEncuentros.inspectorLabel))
      {
         bosqueAngustianteEncuentros.inspectorLabel = "Bosque Angustiante";
      }
      if (pasoVientoHeladoEncuentros != null && string.IsNullOrWhiteSpace(pasoVientoHeladoEncuentros.inspectorLabel))
      {
         pasoVientoHeladoEncuentros.inspectorLabel = "Paso Vientohelado";
      }
      if (genericosEncuentros != null && string.IsNullOrWhiteSpace(genericosEncuentros.inspectorLabel))
      {
         genericosEncuentros.inspectorLabel = "Genéricos";
      }
      if (subterraneosEncuentros != null && string.IsNullOrWhiteSpace(subterraneosEncuentros.inspectorLabel))
      {
         subterraneosEncuentros.inspectorLabel = "Subterráneos";
      }
   }

   public EncounterZoneConfig GetEncounterConfig(EncounterZoneType zoneType)
   {
      switch (zoneType)
      {
         case EncounterZoneType.BosqueAngustiante:
            return bosqueAngustianteEncuentros;
         case EncounterZoneType.PasoVientoHelado:
            return pasoVientoHeladoEncuentros;
         case EncounterZoneType.Nedukazal:
            return NedukazalEncuentros;
         case EncounterZoneType.Generico:
            return genericosEncuentros;
         case EncounterZoneType.Subterraneo:
            return subterraneosEncuentros;
         default:
            return null;
      }
   }

   public float GetChanceEncuentroPropio(EncounterZoneType zoneType)
   {
      var config = GetEncounterConfig(zoneType);
      return config != null ? config.chanceEncuentroPropio : 70f;
   }

   public EncounterZoneType GetZoneTypeById(int zoneId)
   {
      switch (zoneId)
      {
         case 1:
            return EncounterZoneType.BosqueAngustiante;
         case 2:
            return EncounterZoneType.PasoVientoHelado;
         case 3:
            return EncounterZoneType.Nedukazal;
         default:
            return EncounterZoneType.Generico;
      }
   }

   public MeshRenderer TexturaTerreno;
   public MeshRenderer TexturaTerrenoExtension;
   public MeshRenderer TexturaBordeMapa;





   public Material MaterialBosqueAngustiante_Terreno;
   public Material MaterialBosqueAngustiante_BordeMapa;

   public Material MaterialPasoVientoHelado_Terreno;
   public Material MaterialPasoVientoHelado_BordeMapa;

   public Material MaterialNedukazal_Terreno;
   public Material MaterialNedukazal_BordeMapa;

   public GameObject bosqueardienteContenedorGameObjects;
   public GameObject pasovientoheladoContenedorGameObjects;
   public GameObject nedukazalContenedorGameObjects;

   public GameObject BosqueAngustiante_ArbolQuemado1;
   public GameObject BosqueAngustiante_ArbolQuemado2;
   public GameObject BosqueAngustiante_ArbolQuemado3;
   public GameObject BosqueAngustiante_Raices;
   public GameObject BosqueAngustiante_ManchaCeniza1;
   public GameObject BosqueAngustiante_Maleza1;
   public GameObject BosqueAngustiante_Piedra1;
   public GameObject BosqueAngustiante_Piedra2;
   public GameObject BosqueAngustiante_Llama;

   public GameObject PasoVientoHelado_Arbol1;
   public GameObject PasoVientoHelado_Arbol2;
   public GameObject PasoVientoHelado_Mancha2;
   public GameObject PasoVientoHelado_Manchahielo;
   public GameObject PasoVientoHelado_Maleza1;
   public GameObject PasoVientoHelado_Piedra1;
   public GameObject PasoVientoHelado_Piedra2;
   public GameObject PasoVientoHelado_Piedra3;
   public GameObject PasoVientoHelado_Piedra4;
   public GameObject PasoVientoHelado_Piedra5;
   public GameObject PasoVientoHelado_Piedra6;
   public GameObject PasoVientoHelado_grieta1;
   public GameObject PasoVientoHelado_aldeatribal;
   public GameObject PasoVientoHelado_simbolopagano;
   public GameObject PasoVientoHelado_efigie;
   public GameObject PasoVientoHelado_HuesoGrande;
   public GameObject PasoVientoHelado_HuesoChico;


   public GameObject BosqueArdiente_Descripcion;
   public GameObject Pasovientohelado_Descripcion;
   public GameObject Nedukazal_Descripcion;

   void ActualizarDescripcionZonaVisible(bool mostrarBosque, bool mostrarPaso, bool mostrarNedukazal)
   {
      bool mostrarDescripciones = !restaurandoDesdeSave;

      if (BosqueArdiente_Descripcion != null)
      {
         BosqueArdiente_Descripcion.SetActive(mostrarDescripciones && mostrarBosque);
      }

      if (Pasovientohelado_Descripcion != null)
      {
         Pasovientohelado_Descripcion.SetActive(mostrarDescripciones && mostrarPaso);
      }

      if (Nedukazal_Descripcion != null)
      {
         Nedukazal_Descripcion.SetActive(mostrarDescripciones && mostrarNedukazal);
      }
   }

   public void ConstruirZonaBosqueAngustiante(int iFASE)
   {
      Nombre = "Bosque Angustiante"; //dejar asi por ahora
      FASE = iFASE;
      ID = 1;
      modRecoleccionMateriales = -10;
      modRecoleccionSuministros = 5;
      modChanceEmboscada = 15;


      if (!restaurandoDesdeSave)
      {
         Invoke("AumentarDifconDelayPorPeligroBosqueArdiente", 1.5f);
      }

      modChanceExploracion = 5;

      Clima_chances_Sol = 40;
      Clima_chances_Calor = 50;
      Clima_chances_Lluvia = 60;
      Clima_chances_Nieve = 60;
      Clima_chances_Niebla = 80;
      Clima_chances_EspecialZona1 = 100;



      if (TRADU.i != null)
      { txtNombreZona.text = TRADU.i.Traducir("El Bosque Ardiente"); }

      ActualizarDescripcionZonaVisible(true, false, false);

      if (!restaurandoDesdeSave)
      {
         CampaignManager.Instance.BosqueArdienteMecanicaIncendio(100);
      }


      Invoke("PlayMusic", 0.2f);
      // Usar fader como tapón mientras se adorna el mapa (async, sin freeze)
      DecoracionZonaEnCurso = true;
      StartCoroutine(AdornarBosqueArdienteConFadeAsync());


      Nedukazal_CaravanaLuz.SetActive(false);
      VFX_AlientoNegroNedukazal.SetActive(true);

   }

   void PlayMusic()
   {
      MusicManager.Instance.PlayCampania(ID);
   }
   IEnumerator AdornarBosqueArdienteConFadeAsync()
   {

      ConfigurarExclusionHieloPasoVientoHelado(false);
      ConfigurarPrimerPuntoCentricoPasoVientoHelado(false);

      TexturaTerreno.material = MaterialBosqueAngustiante_Terreno;
      TexturaTerrenoExtension.material = MaterialBosqueAngustiante_Terreno;
      TexturaBordeMapa.material = MaterialBosqueAngustiante_BordeMapa;
      pasovientoheladoContenedorGameObjects.SetActive(false);
      nedukazalContenedorGameObjects.SetActive(false);
      bosqueardienteContenedorGameObjects.SetActive(true);
      CampaignManager.Instance.sunController = bosqueardienteContenedorGameObjects.transform.GetChild(0).gameObject.GetComponent<SunController>();
      // Respetar timing previo y dejar terminar el fade inicial del AdministradorEscenas
      yield return new WaitForSecondsRealtime(0.5f);

      AplicarTexturaProceduralBosqueAngustiante();

      var admin = CampaignManager.Instance != null ? CampaignManager.Instance.scAdministradorEscenas : null;
      if (admin != null)
      {
         // Tapón negro inmediato (sin fade-in) y bloqueo de fades concurrentes
         admin.SetFaderHold(true); // fuerza alpha=1 inmediatamente
      }

      // Async sin congelar: replicamos las llamadas Generar pero con yield
      yield return scMapDecorator.GenerarAsyncCR(
         BosqueAngustiante_ArbolQuemado1,
         cantidad: 2750,
         sector: 0,  //EN QUE SECTOR SE GENERA - 0 es en todos MENOS sectorno: x.
         //sectorno: 3, por ejemplo no se generarian en el sector TerrenoSur(3)
         distCaminoOverride: 0.11f,
         distNodoOverride: 0.13f,
         rOverride: 0.58f,
         kOverride: 20);
         
       yield return scMapDecorator.GenerarAsyncCR(
        BosqueAngustiante_ArbolQuemado1,
        cantidad: 550,
        sector: 0,
        distCaminoOverride: 0.16f,
        distNodoOverride: 0.14f,
        rOverride: 1.20f,
        kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         BosqueAngustiante_ArbolQuemado2,
         cantidad: 305,
         sector: 0,
         distCaminoOverride: 0.14f,
         distNodoOverride: 0.145f,
         rOverride: 2.7f,
         kOverride: 20);
         
       yield return scMapDecorator.GenerarAsyncCR(
        BosqueAngustiante_ArbolQuemado3,
        cantidad: 100,
        sector: 0,
        distCaminoOverride: 0.18f,
        distNodoOverride: 0.18f,
        rOverride:6.8f,
        kOverride: 20);
        
       yield return scMapDecorator.GenerarAsyncCR(
        BosqueAngustiante_Raices,
        cantidad: 100,
        sector: 0,
        distCaminoOverride: 0.15f,
        distNodoOverride: 0.15f,
        rOverride: 4.8f,
        kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         BosqueAngustiante_ManchaCeniza1,
         cantidad: 85,
         sector: 0,
         distCaminoOverride: 0.10f,
         distNodoOverride: 0.10f,
         rOverride: 10.8f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         BosqueAngustiante_Piedra1,
         cantidad: 70,
         sector: 0,
         distCaminoOverride: 0.6f,
         distNodoOverride: 0.8f,
         rOverride: 7.0f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         BosqueAngustiante_Piedra2,
         cantidad: 10,
         sector: 0,
         distCaminoOverride: 2.0f,
         distNodoOverride: 2.2f,
         rOverride: 11.0f,
         kOverride: 20);
      yield return scMapDecorator.GenerarAsyncCR(
         BosqueAngustiante_Llama,
         cantidad: 50,
         sector: 0,
         sectorno: 4,
         distCaminoOverride: 0.65f,
         distNodoOverride: 0.95f,
         rOverride: 5.0f,
         kOverride: 20);

      if (admin != null)
      {
         DecoracionZonaEnCurso = false;
         yield return LiberarFaderDecoracionZona(admin);
      }
      else
      {
         DecoracionZonaEnCurso = false;
      }
   }


   public void ConstruirZonaPasoVientoHelado(int iFASE)
   {
      Nombre = "Paso Vientohelado";
      FASE = iFASE;
      ID = 2;
      modRecoleccionMateriales = 10;
      modRecoleccionSuministros = -15;
      modChanceEmboscada = 10;
      PasoVientoHelado_FuerzaKaleTav = 0;

      modChanceExploracion = -10;

      if (!restaurandoDesdeSave)
      {
         Invoke("AumentarDifconDelayPorPeligroPasoVientoHelado", 1.5f);
      }

      Clima_chances_Sol = 40;
      Clima_chances_Calor = 40;
      Clima_chances_Lluvia = 43;
      Clima_chances_Nieve = 75;
      Clima_chances_Niebla = 91;
      Clima_chances_EspecialZona1 = 100;

       



      ActualizarDescripcionZonaVisible(false, true, false);

      txtNombreZona.text = TRADU.i.Traducir(Nombre);


      Invoke("PlayMusic", 0.2f);
      // Usar fader como tapón mientras se adorna el mapa (async, sin freeze)
      DecoracionZonaEnCurso = true;
      StartCoroutine(AdornarPasoVientoHeladoConFadeAsync());

      Nedukazal_CaravanaLuz.SetActive(false);
      VFX_AlientoNegroNedukazal.SetActive(true);




   }
   IEnumerator AdornarPasoVientoHeladoConFadeAsync()
   {

      TexturaTerreno.material = MaterialPasoVientoHelado_Terreno;
      TexturaTerrenoExtension.material = MaterialPasoVientoHelado_Terreno;
      TexturaBordeMapa.material = MaterialPasoVientoHelado_BordeMapa;
      pasovientoheladoContenedorGameObjects.SetActive(true);
      bosqueardienteContenedorGameObjects.SetActive(false);
      nedukazalContenedorGameObjects.SetActive(false);
      CampaignManager.Instance.sunController = pasovientoheladoContenedorGameObjects.transform.GetChild(0).gameObject.GetComponent<SunController>();

      // Respetar timing previo y dejar terminar el fade inicial del AdministradorEscenas
      yield return new WaitForSecondsRealtime(0.5f);

      AplicarTexturaProceduralPasoVientoHelado();
      ConfigurarExclusionHieloPasoVientoHelado(true);
      ConfigurarPrimerPuntoCentricoPasoVientoHelado(true);

      var admin = CampaignManager.Instance != null ? CampaignManager.Instance.scAdministradorEscenas : null;
      if (admin != null)
      {
         // Tapón negro inmediato (sin fade-in) y bloqueo de fades concurrentes
         admin.SetFaderHold(true); // fuerza alpha=1 inmediatamente
      }


      // Async sin congelar: replicamos las llamadas Generar pero con yield
      yield return scMapDecorator.GenerarAsyncCR(
         BosqueAngustiante_ArbolQuemado1,
         cantidad: 34,
         sectorno: 3,
         distCaminoOverride: 0.11f,
         distNodoOverride: 0.15f,
         rOverride: 6.25f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
       PasoVientoHelado_Arbol1,
       cantidad: 48,
       sectorno: 3,
       distCaminoOverride: 0.11f,
       distNodoOverride: 0.45f,
       rOverride: 5.95f,
       kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
       PasoVientoHelado_Arbol2,
       cantidad: 65,
       sectorno: 3,
       distCaminoOverride: 0.11f,
       distNodoOverride: 0.45f,
       rOverride: 5.85f,
       kOverride: 20);


      yield return scMapDecorator.GenerarAsyncCR(
        PasoVientoHelado_Manchahielo,
        cantidad: 10,
        sector: 1,  //EN QUE SECTOR SE GENERA - 0 es en todos MENOS sectorno: x.
        sectorno: 3,
        distCaminoOverride: 1.25f,
        distNodoOverride: 0.93f,
        rOverride: 13.85f,
        kOverride: 20);
      yield return scMapDecorator.GenerarAsyncCR(
        PasoVientoHelado_Mancha2,
        cantidad: 0,
        sectorno: 3,
        distCaminoOverride: 0.11f,
        distNodoOverride: 0.15f,
        rOverride: 7.85f,
        kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         PasoVientoHelado_Maleza1,
         cantidad: 1150,
         sectorno: 3,
         distCaminoOverride: 0.12f,
         distNodoOverride: 0.85f,
         rOverride: 1.27f,
         kOverride: 30);

      yield return scMapDecorator.GenerarAsyncCR(
         PasoVientoHelado_Maleza1,
         cantidad: 1450,
         sectorno: 3,
         distCaminoOverride: 0.1f,
         distNodoOverride: 0.8f,
         rOverride: 0.82f,
         kOverride: 30);

      yield return scMapDecorator.GenerarAsyncCR(
          BosqueAngustiante_ManchaCeniza1,
          cantidad: 60,
          sectorno: 3,
          distCaminoOverride: 0.10f,
          distNodoOverride: 0.10f,
          rOverride: 10.8f,
          kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
        PasoVientoHelado_Piedra1,
        cantidad: 85,
        sectorno: 3,
        distCaminoOverride: 0.10f,
        distNodoOverride: 0.10f,
        rOverride: 8.8f,
        kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
        PasoVientoHelado_Piedra2,
        cantidad: 68,
        sectorno: 3,
        distCaminoOverride: 0.10f,
        distNodoOverride: 0.10f,
        rOverride: 10.8f,
        kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         PasoVientoHelado_Piedra3,
         cantidad: 10,
         sectorno: 3,
         distCaminoOverride: 1.80f,
         distNodoOverride: 2.10f,
         rOverride: 17.8f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         PasoVientoHelado_Piedra4,
         cantidad: 14,
         sectorno: 3,
         distCaminoOverride: 1.60f,
         distNodoOverride: 1.75f,
         rOverride: 18.8f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         PasoVientoHelado_Piedra5,
         cantidad: 20,
         sectorno: 3,
         distCaminoOverride: 1.40f,
         distNodoOverride: 1.70f,
         rOverride: 14.8f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         PasoVientoHelado_Piedra6,
         cantidad: 13,
         sectorno: 3,
         distCaminoOverride: 1.60f,
         distNodoOverride: 1.95f,
         rOverride: 16.8f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
       PasoVientoHelado_grieta1,
       cantidad: 4,
       sectorno: 3,
       distCaminoOverride: 1.4f,
       distNodoOverride: 1.40f,
       rOverride: 15.8f,
       kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
       PasoVientoHelado_aldeatribal,
       cantidad: 10,
       sectorno: 3,
       distCaminoOverride: 0.95f,
       distNodoOverride: 1.60f,
       rOverride: 9.8f,
       kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
        PasoVientoHelado_efigie,
        cantidad: 15,
        sectorno: 3,
        distCaminoOverride: 0.3f,
        distNodoOverride: 0.50f,
        rOverride: 7.8f,
        kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
        PasoVientoHelado_simbolopagano,
        cantidad: 4,
        sectorno: 3,
        distCaminoOverride: 0.8f,
        distNodoOverride: 0.50f,
        rOverride: 10.8f,
        kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
        PasoVientoHelado_HuesoGrande,
        cantidad: 2,
        sector: 1,  //EN QUE SECTOR SE GENERA - 0 es en todos MENOS sectorno: x.
        sectorno: 3,
         //sectorno: 3, por ejemplo no se generarian en el sector TerrenoSur(3)
        distCaminoOverride: 3.1f,
        distNodoOverride: 2.5f,
        rOverride: 50.8f,
        kOverride: 20);

      if (admin != null)
      {
         DecoracionZonaEnCurso = false;
         yield return LiberarFaderDecoracionZona(admin);
      }
      else
      {
         DecoracionZonaEnCurso = false;
      }

      ConfigurarExclusionHieloPasoVientoHelado(false);
      ConfigurarPrimerPuntoCentricoPasoVientoHelado(false);
   }

   void AplicarTexturaProceduralBosqueAngustiante()
   {
      if (!usarTexturaProceduralBosqueAngustiante)
      {
         return;
      }

      int seed = ObtenerSeedTexturaSueloZona();
      Texture2D texturaSuelo = TexturaProceduralSueloZona.CrearTexturaSuelo(EncounterZoneType.BosqueAngustiante, seed, texturaSueloBosqueAngustiante);
      Color tint = texturaSueloBosqueAngustiante != null ? texturaSueloBosqueAngustiante.tintMaterial : Color.white;

      TexturaProceduralSueloZona.AplicarTexturaSueloZona(TexturaTerreno, texturaSuelo, tint);
      TexturaProceduralSueloZona.AplicarTexturaSueloZona(TexturaTerrenoExtension, texturaSuelo, tint);
   }

   void AplicarTexturaProceduralPasoVientoHelado()
   {
      int seed = ObtenerSeedTexturaSueloZona();
      Texture2D texturaSuelo = TexturaProceduralSueloZona.CrearTexturaSuelo(EncounterZoneType.PasoVientoHelado, seed, texturaSueloPasoVientoHelado);
      Color tint = texturaSueloPasoVientoHelado != null ? texturaSueloPasoVientoHelado.tintMaterial : Color.white;

      TexturaProceduralSueloZona.AplicarTexturaSueloZona(TexturaTerreno, texturaSuelo, tint);
      TexturaProceduralSueloZona.AplicarTexturaSueloZona(TexturaTerrenoExtension, texturaSuelo, tint);
   }

   int ObtenerSeedTexturaSueloZona()
   {
      int reliefSeed = scMapDecorator != null ? scMapDecorator.GetReliefSeed() : 0;

      unchecked
      {
         return (ID * 73856093) ^ (FASE * 19349663) ^ reliefSeed;
      }
   }

   void ConfigurarExclusionHieloPasoVientoHelado(bool activa)
   {
      if (scMapDecorator == null)
      {
         return;
      }

      bool activarFiltro = activa && evitarAdornosSobreHieloPasoVientoHelado;
      scMapDecorator.ConfigurarExclusionDecoracionPorAltura(activarFiltro, alturaHieloPasoVientoHelado);
   }

   void ConfigurarPrimerPuntoCentricoPasoVientoHelado(bool pasoActivo)
   {
      if (scMapDecorator == null)
      {
         return;
      }

      bool usarCentro = !(pasoActivo && evitarPrimerPuntoCentricoPasoVientoHelado);
      scMapDecorator.ConfigurarPrimerPuntoCentrico(usarCentro);
   }

   public GameObject Nedukazal_CaravanaLuz;
   public void ConstruirZonaNedukazal(int iFASE)
   {
      Nombre = "Nedukazal";
      FASE = iFASE;
      ID = 3;
      modRecoleccionMateriales = 20;
      modRecoleccionSuministros = -25;
      modChanceEmboscada = 20;

      modChanceExploracion = -25;


      if (!restaurandoDesdeSave)
      {
         Invoke("AumentarDifconDelayPorPeligroNedukazal", 1.5f);
      }


      Clima_chances_Sol = 00;
      Clima_chances_Calor = 00;
      Clima_chances_Lluvia = 00;
      Clima_chances_Nieve = 00;
      Clima_chances_Niebla = 00;
      Clima_chances_EspecialZona1 = 60; //60
      Clima_chances_EspecialZona2 = 100;



      ActualizarDescripcionZonaVisible(false, false, true);

      txtNombreZona.text = TRADU.i.Traducir(Nombre);


      Invoke("PlayMusic", 0.2f);
      // Usar fader como tapón mientras se adorna el mapa (async, sin freeze)
      DecoracionZonaEnCurso = true;
      StartCoroutine(AdornarNedukazalConFadeAsync());


      Nedukazal_CaravanaLuz.SetActive(true);
      ActualizarLuzNedukazal();
      VFX_AlientoNegroNedukazal.SetActive(false);



   }

   public void ActualizarLuzNedukazal()
   {
      if (Nedukazal_CaravanaLuz != null)
      {
         var luz = Nedukazal_CaravanaLuz.GetComponent<Light>();
         if (luz != null)
         {
            luz.range = 6 + CampaignManager.Instance.mejoraCaravanaAntorchas;
         }
      }
   }


   public GameObject Nedukazal_Escombro1;
   public GameObject Nedukazal_Escombro2;
   public GameObject Nedukazal_Escombro3;
   public GameObject Nedukazal_Edificio1;
   public GameObject Nedukazal_Edificio2;
   public GameObject Nedukazal_Maleza1;
   public GameObject Nedukazal_Aldea1;
   public GameObject VFX_AlientoNegroNedukazal;





   IEnumerator AdornarNedukazalConFadeAsync()
   {

      ConfigurarExclusionHieloPasoVientoHelado(false);
      ConfigurarPrimerPuntoCentricoPasoVientoHelado(false);

      TexturaTerreno.material = MaterialNedukazal_Terreno;
      TexturaTerrenoExtension.material = MaterialNedukazal_Terreno;
      //TexturaBordeMapa.material = MaterialNedukazal_BordeMapa;
      pasovientoheladoContenedorGameObjects.SetActive(false);
      bosqueardienteContenedorGameObjects.SetActive(false);
      nedukazalContenedorGameObjects.SetActive(true);
      CampaignManager.Instance.sunController = nedukazalContenedorGameObjects.transform.GetChild(0).gameObject.GetComponent<SunController>();
      // Respetar timing previo y dejar terminar el fade inicial del AdministradorEscenas
      yield return new WaitForSecondsRealtime(0.5f);

      var admin = CampaignManager.Instance != null ? CampaignManager.Instance.scAdministradorEscenas : null;
      if (admin != null)
      {
         // Tapón negro inmediato (sin fade-in) y bloqueo de fades concurrentes
         admin.SetFaderHold(true); // fuerza alpha=1 inmediatamente
      }

      // Async sin congelar: replicamos las llamadas Generar pero con yield
      yield return scMapDecorator.GenerarAsyncCR(
         Nedukazal_Escombro1,
         cantidad: 120,
         distCaminoOverride: 0.12f,
         distNodoOverride: 0.125f,
         rOverride: 6.7f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
           BosqueAngustiante_ManchaCeniza1,
           cantidad: 105,
           distCaminoOverride: 0.10f,
           distNodoOverride: 0.40f,
           rOverride: 10.8f,
           kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         Nedukazal_Escombro3,
         cantidad: 1005,
         distCaminoOverride: 0.09f,
         distNodoOverride: 0.6f,
         rOverride: 1.6f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         Nedukazal_Edificio1,
         cantidad: 5,
         distCaminoOverride: 1.5f,
         distNodoOverride: 1.9f,
         rOverride: 13.0f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         Nedukazal_Edificio2,
         cantidad: 3,
         distCaminoOverride: 1.5f,
         distNodoOverride: 2.2f,
         rOverride: 27.0f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
           Nedukazal_Maleza1,
           cantidad: 300,
           distCaminoOverride: 0.7f,
           distNodoOverride: 2.5f,
           rOverride: 1.67f,
           kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
          Nedukazal_Aldea1,
          cantidad: 15,
          distCaminoOverride: 0.28f,
          distNodoOverride: 0.85f,
          rOverride: 7.5f,
          kOverride: 20);

      /* yield return scMapDecorator.GenerarAsyncCR(
          BosqueAngustiante_Llama,
          cantidad: 25,
          distCaminoOverride: 0.6f,
          distNodoOverride: 0.9f,
          rOverride: 8.0f,
          kOverride: 20);
 */
      if (admin != null)
      {
         DecoracionZonaEnCurso = false;
         yield return LiberarFaderDecoracionZona(admin);
      }
      else
      {
         DecoracionZonaEnCurso = false;
      }
   }

   IEnumerator LiberarFaderDecoracionZona(AdministradorEscenas admin)
   {
      if (admin == null)
      {
         yield break;
      }

      if (CampaignManager.Instance != null && CampaignManager.Instance.IntroCampaniaActivaOPendiente)
      {
         yield break;
      }

      // Liberar bloqueo y volver a mostrar la escena
      admin.SetFaderHold(false);
      yield return admin.FadeOut(0.25f);
   }

   // Lista para llevar registro del estado de las zonas
   // 0: No cruzada, 1: Cruzada, 2: Descartada
   public List<int> ZonasEstado = new List<int>();

   /// <summary>
   /// Inicializa la lista de estados de las zonas.
   /// Debe llamarse al inicio del juego o cuando se reinicia la campaña.
   /// </summary>



   /// <summary>
   /// Actualiza el estado de una zona específica.
   /// </summary>
   /// <param name="zonaID">El ID de la zona a actualizar (índice en la lista).</param>
   /// <param name="estado">El nuevo estado de la zona (0: No cruzada, 1: Cruzada, 2: Descartada).</param>
   public void ActualizarEstadoZona(int zonaID, int estado)
   {
      zonaID -= 1; // Ajustar para índice basado en cero
      if (zonaID >= 0 && zonaID < ZonasEstado.Count)
      {
         ZonasEstado[zonaID] = estado;
      }
      else
      {
         Debug.LogWarning($"ZonaID {zonaID} está fuera de rango.");
      }
   }


   public void GenerarZona(int ID = 0)
   {
      if (CampaignManager.Instance != null)
      {
         CampaignManager.Instance.ResetearEventosAleatoriosUsadosMapa();
      }

      int zona = ID;

      FASE++;
      // Si no se pasa ID, seleccionar aleatoriamente de las zonas con estado 0
      if (zona == 0)
      {
         var zonasDisponibles = new List<int>();
         for (int i = 0; i < ZonasEstado.Count; i++)
         {
            if (ZonasEstado[i] == 0)
            {
               zonasDisponibles.Add(i + 1); // Los IDs de las zonas comienzan desde 1
            }
         }

         if (zonasDisponibles.Count > 0)
         {
            zona = zonasDisponibles[UnityEngine.Random.Range(0, zonasDisponibles.Count)];
         }
         else
         {
            Debug.LogWarning("No hay zonas disponibles con estado 0.");
            return;
         }
      }



      switch (zona)
      {
         case 1:
            ConstruirZonaBosqueAngustiante(FASE);
            break;
         case 2:
            ConstruirZonaPasoVientoHelado(FASE);
            break;
         case 3:
            ConstruirZonaNedukazal(FASE);
            break;

      }

      if (scMapDecorator == null)
      {
         scMapDecorator = GetComponent<MapDecorator>();
      }

      if (scMapDecorator != null)
      {
         scMapDecorator.RegenerarRelieveParaZona(zona, FASE);
      }

      CampaignManager.Instance.scMapaManager.GenerarNodos();
      CampaignManager.Instance.BloquearOlaDeCalorEnSiguienteTiradaClima();
      CampaignManager.Instance.ForzarTiradaClima();
      CampaignManager.Instance.AplicarEfectosMejorasPuerto();

   }

   public void RestaurarZonaDesdeSave(int zonaId, int fase, List<int> zonasEstadoGuardadas = null, int? reliefSeedGuardado = null)
   {
      if (zonasEstadoGuardadas != null && zonasEstadoGuardadas.Count > 0)
      {
         ZonasEstado = new List<int>(zonasEstadoGuardadas);
      }

      restaurandoDesdeSave = true;
      try
      {
         switch (zonaId)
         {
            case 1:
               ConstruirZonaBosqueAngustiante(fase);
               break;
            case 2:
               ConstruirZonaPasoVientoHelado(fase);
               break;
            case 3:
               ConstruirZonaNedukazal(fase);
               break;
            default:
               Debug.LogWarning($"[SaveGame] Zona {zonaId} no reconocida al restaurar campania.");
               return;
         }

         if (scMapDecorator == null)
         {
            scMapDecorator = GetComponent<MapDecorator>();
         }

         if (scMapDecorator != null)
         {
            scMapDecorator.RegenerarRelieveParaZona(zonaId, fase, reliefSeedGuardado);
         }
      }
      finally
      {
         restaurandoDesdeSave = false;
      }
   }


   void AumentarDifconDelayPorPeligroNedukazal()
   {
      CampaignManager.Instance.IncrementarDificultadSegunPeligroRegion(MetaprogresionManager.Instance.NivelPeligroNedukazal);
      MetaprogresionManager.Instance.NivelPeligroNedukazal++;
   }
   void AumentarDifconDelayPorPeligroBosqueArdiente()
   {
      CampaignManager.Instance.IncrementarDificultadSegunPeligroRegion(MetaprogresionManager.Instance.NivelPeligroBosqueArdiente);
      MetaprogresionManager.Instance.NivelPeligroBosqueArdiente++;
   }
   void AumentarDifconDelayPorPeligroPasoVientoHelado()
   { 
      CampaignManager.Instance.IncrementarDificultadSegunPeligroRegion(MetaprogresionManager.Instance.NivelPeligroPasoVientohelado);
      MetaprogresionManager.Instance.NivelPeligroPasoVientohelado++;
   }
}



