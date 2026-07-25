using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class PresagioDefinition
{
  public int id;
  public bool positivo;
  public string categoria;
  public int[] presagiosIncompatibles = Array.Empty<int>();
  public int[] regionesDisponibles = Array.Empty<int>();
  public string nombreEs;
  public string descripcionEs;
  public string nombreEn;
  public string descripcionEn;
  public string nombrePt;
  public string descripcionPt;

  public bool EstaDisponibleEnRegion(int regionId)
  {
    if (regionId <= 0)
    {
      return false;
    }

    if (regionesDisponibles == null || regionesDisponibles.Length == 0)
    {
      return true;
    }

    for (int i = 0; i < regionesDisponibles.Length; i++)
    {
      if (regionesDisponibles[i] == regionId)
      {
        return true;
      }
    }

    return false;
  }

  public string ObtenerTextoLocalizado()
  {
    int idioma = PresagioCatalog.ObtenerIdiomaActual();
    string nombre;
    string descripcion;
    switch (idioma)
    {
      case TRADU.IdiomaIngles:
        nombre = nombreEn;
        descripcion = descripcionEn;
        break;
      case TRADU.IdiomaPortugues:
        nombre = nombrePt;
        descripcion = descripcionPt;
        break;
      default:
        nombre = nombreEs;
        descripcion = descripcionEs;
        break;
    }

    if (string.IsNullOrWhiteSpace(descripcion))
    {
      return PresagioCatalog.FormatearTitulo(nombre, positivo);
    }

    return string.IsNullOrWhiteSpace(nombre)
      ? descripcion
      : PresagioCatalog.FormatearTitulo(nombre, positivo) + " " + descripcion;
  }
}

/// <summary>
/// Catálogo único de presagios y de sus traducciones locales.
/// Los nuevos tipos de presagio se agregan solamente en definiciones.
/// </summary>
public static class PresagioCatalog
{
  public const int RutasQuebradas = 1;
  public const int RutasAbiertas = 2;
  public const int Subsuelo = 3;
  public const int Derrumbado = 4;
  public const int CaminosIntrincados = 5;
  public const int CaminosCuidados = 6;
  public const int ViejosSenderos = 7;
  public const int CaminosBorrados = 8;
  public const int MaterialesAbundantes = 9;
  public const int MaterialesEscasos = 10;
  public const int PresasFaciles = 11;
  public const int FaunaReducida = 12;
  public const int ComercioActivo = 13;
  public const int ComercioMenguado = 14;
  public const int AuraPositiva = 15;
  public const int AuraNegativa = 16;
  public const int SenalesClaras = 17;
  public const int SenalesConfusas = 18;
  public const int PilasDeRecursos = 19;
  public const int RecursosEscondidos = 20;
  public const int RumoresComeciales = 21;
  public const int SenalesSagradas = 22;
  public const int TierraProfana = 23;
  public const int AdvertenciasDeAmenazas = 24;
  public const int AmenazasEscondidas = 25;
  public const int PobladosEscasos = 26;
  public const int PobladosVividos = 27;
  public const int ZonaCartografiada = 28;
  public const int ZonaDesconocida = 29;
  public const int SensacionPositiva = 30;
  public const int SensacionNegativa = 31;
  public const int NochesPacificas = 32;
  public const int NochesTurbulentas = 33;
  public const int CaminosPeligrosos = 34;
  public const int EnemigosDesprevenidos = 35;
  public const int AmenazasVigilantes = 36;
  public const int SinVigilancia = 37;
  public const int AventuraMemorable = 38;
  public const int AventuraOlvidable = 39;
  public const int PlagaEnLaRegion = 40;
  public const int PlantasCurativas = 41;
  public const int CorrupcionInsoportable = 42;
  public const int RegionBendecida = 43;
  public const int Espejismos = 44;
  public const int VientoAFavor = 45;
  public const int VientoEnContra = 46;
  public const int AireLimpio = 47;
  public const int AirePutrido = 48;
  public const int LeyDelMasFuerte = 49;
  public const int CorrompidosAlAcecho = 50;
  public const int VenganadoresCazando = 51;
  public const int CentinelasLocales = 52;
  public const int AmenazaSuperior = 53;

  private const string ColorTituloPresagioPositivo = "#69A3A0";
  private const string ColorTituloPresagioNegativo = "#C06A55";

  private static readonly List<PresagioDefinition> definiciones = new List<PresagioDefinition>
  {
    new PresagioDefinition
    {
      id = RutasQuebradas,
      positivo = false,
      categoria = "caminos",
      presagiosIncompatibles = new[] { RutasAbiertas },
      nombreEs = "Rutas Quebradas",
      descripcionEs = "los caminos tienen menos bifurcaciones.",
      nombreEn = "Broken Routes",
      descripcionEn = "roads have fewer branches.",
      nombrePt = "Rotas Quebradas",
      descripcionPt = "os caminhos têm menos bifurcações."
    },
    new PresagioDefinition
    {
      id = RutasAbiertas,
      positivo = true,
      categoria = "caminos",
      presagiosIncompatibles = new[] { RutasQuebradas },
      nombreEs = "Rutas Abiertas",
      descripcionEs = "los caminos tienen mas bifurcaciones.",
      nombreEn = "Open Routes",
      descripcionEn = "roads have more branches.",
      nombrePt = "Rotas Abertas",
      descripcionPt = "os caminhos têm mais bifurcações."
    },
    new PresagioDefinition
    {
      id = Subsuelo,
      positivo = true,
      categoria = "caminos",
      presagiosIncompatibles = new[] { Derrumbado },
      nombreEs = "Subsuelo",
      descripcionEs = "se encuentran mas atajos subterráneos.",
      nombreEn = "Underground",
      descripcionEn = "more underground shortcuts are found.",
      nombrePt = "Subsolo",
      descripcionPt = "mais atalhos subterrâneos são encontrados."
    },
    new PresagioDefinition
    {
      id = Derrumbado,
      positivo = false,
      categoria = "caminos",
      presagiosIncompatibles = new[] { Subsuelo },
      nombreEs = "Derrumbado",
      descripcionEs = "no se encuentran atajos subterráneos.",
      nombreEn = "Collapsed",
      descripcionEn = "no underground shortcuts are found.",
      nombrePt = "Desmoronado",
      descripcionPt = "não são encontrados atalhos subterrâneos."
    },
    new PresagioDefinition
    {
      id = CaminosIntrincados,
      positivo = false,
      categoria = "caminos",
      presagiosIncompatibles = new[] { CaminosCuidados },
      nombreEs = "Caminos Intrincados",
      descripcionEs = "se encuentran mas caminos sinuosos.",
      nombreEn = "Winding Roads",
      descripcionEn = "more winding roads are found.",
      nombrePt = "Caminhos Intrincados",
      descripcionPt = "mais caminhos sinuosos são encontrados."
    },
    new PresagioDefinition
    {
      id = CaminosCuidados,
      positivo = true,
      categoria = "caminos",
      presagiosIncompatibles = new[] { CaminosIntrincados },
      nombreEs = "Caminos Cuidados",
      descripcionEs = "se reduce la cantidad de caminos sinuosos.",
      nombreEn = "Well-Kept Roads",
      descripcionEn = "the number of winding roads is reduced.",
      nombrePt = "Caminhos Bem Cuidados",
      descripcionPt = "a quantidade de caminhos sinuosos é reduzida."
    },
    new PresagioDefinition
    {
      id = ViejosSenderos,
      positivo = true,
      categoria = "caminos",
      presagiosIncompatibles = new[] { CaminosBorrados },
      nombreEs = "Viejos Senderos",
      descripcionEs = "se encuentran más atajos de superficie.",
      nombreEn = "Old Trails",
      descripcionEn = "more surface shortcuts are found.",
      nombrePt = "Trilhas Antigas",
      descripcionPt = "mais atalhos de superfície são encontrados."
    },
    new PresagioDefinition
    {
      id = CaminosBorrados,
      positivo = false,
      categoria = "caminos",
      presagiosIncompatibles = new[] { ViejosSenderos },
      nombreEs = "Caminos Borrados",
      descripcionEs = "no se encuentran atajos de superficie.",
      nombreEn = "Faded Roads",
      descripcionEn = "no surface shortcuts are found.",
      nombrePt = "Caminhos Apagados",
      descripcionPt = "não são encontrados atalhos de superfície."
    },
    new PresagioDefinition
    {
      id = MaterialesAbundantes,
      positivo = true,
      categoria = "recursos",
      presagiosIncompatibles = new[] { MaterialesEscasos },
      nombreEs = "Materiales Abundantes",
      descripcionEs = "+15% de recolección de Materiales",
      nombreEn = "Abundant Materials",
      descripcionEn = "+15% Materials gathering",
      nombrePt = "Materiais Abundantes",
      descripcionPt = "+15% de coleta de Materiais"
    },
    new PresagioDefinition
    {
      id = MaterialesEscasos,
      positivo = false,
      categoria = "recursos",
      presagiosIncompatibles = new[] { MaterialesAbundantes },
      nombreEs = "Materiales Escasos",
      descripcionEs = "-15% Recomección de Materiales",
      nombreEn = "Scarce Materials",
      descripcionEn = "-15% Materials gathering",
      nombrePt = "Materiais Escassos",
      descripcionPt = "-15% de coleta de Materiais"
    },
    new PresagioDefinition
    {
      id = PresasFaciles,
      positivo = true,
      categoria = "recursos",
      presagiosIncompatibles = new[] { FaunaReducida },
      nombreEs = "Presas Fáciles",
      descripcionEs = "+15% de recolección de Suministros",
      nombreEn = "Easy Prey",
      descripcionEn = "+15% Supplies gathering",
      nombrePt = "Presas Fáceis",
      descripcionPt = "+15% de coleta de Suprimentos"
    },
    new PresagioDefinition
    {
      id = FaunaReducida,
      positivo = false,
      categoria = "recursos",
      presagiosIncompatibles = new[] { PresasFaciles },
      nombreEs = "Fauna Reducida",
      descripcionEs = "-15% Recomección de Suministros",
      nombreEn = "Dwindling Wildlife",
      descripcionEn = "-15% Supplies gathering",
      nombrePt = "Fauna Reduzida",
      descripcionPt = "-15% de coleta de Suprimentos"
    },
    new PresagioDefinition
    {
      id = ComercioActivo,
      positivo = true,
      categoria = "recursos",
      presagiosIncompatibles = new[] { ComercioMenguado },
      nombreEs = "Comercio Activo",
      descripcionEs = "-10% costo de compra de objetos y recursos.",
      nombreEn = "Active Trade",
      descripcionEn = "-10% purchase cost of items and resources.",
      nombrePt = "Comércio Ativo",
      descripcionPt = "-10% no custo de compra de objetos e recursos."
    },
    new PresagioDefinition
    {
      id = ComercioMenguado,
      positivo = false,
      categoria = "recursos",
      presagiosIncompatibles = new[] { ComercioActivo },
      nombreEs = "Comercio Menguado",
      descripcionEs = "+10% costo de compra de objetos y recursos.",
      nombreEn = "Dwindling Trade",
      descripcionEn = "+10% purchase cost of items and resources.",
      nombrePt = "Comércio Enfraquecido",
      descripcionPt = "+10% no custo de compra de objetos e recursos."
    },
    new PresagioDefinition
    {
      id = AuraPositiva,
      positivo = true,
      categoria = "eventos",
      presagiosIncompatibles = new[] { AuraNegativa },
      nombreEs = "Aura Positiva",
      descripcionEs = "aumenta las chances de que los Eventos sean positivos",
      nombreEn = "Positive Aura",
      descripcionEn = "increases the chance of Events being positive",
      nombrePt = "Aura Positiva",
      descripcionPt = "aumenta as chances de os Eventos serem positivos"
    },
    new PresagioDefinition
    {
      id = AuraNegativa,
      positivo = false,
      categoria = "eventos",
      presagiosIncompatibles = new[] { AuraPositiva },
      nombreEs = "Aura Negativa",
      descripcionEs = "aumenta las chances de que los Eventos sean negativos",
      nombreEn = "Negative Aura",
      descripcionEn = "increases the chance of Events being negative",
      nombrePt = "Aura Negativa",
      descripcionPt = "aumenta as chances de os Eventos serem negativos"
    },
    new PresagioDefinition
    {
      id = SenalesClaras,
      positivo = true,
      categoria = "nodos",
      presagiosIncompatibles = new[] { SenalesConfusas },
      nombreEs = "Señales Claras",
      descripcionEs = "los nodos de Eventos arrancan revelados en el mapa.",
      nombreEn = "Clear Signs",
      descripcionEn = "Event nodes start revealed on the map.",
      nombrePt = "Sinais Claros",
      descripcionPt = "os nodos de Eventos começam revelados no mapa."
    },
    new PresagioDefinition
    {
      id = SenalesConfusas,
      positivo = false,
      categoria = "nodos",
      presagiosIncompatibles = new[] { SenalesClaras },
      nombreEs = "Señales Confusas",
      descripcionEs = "los nodos de Eventos siempre serán Misteriosos.",
      nombreEn = "Confusing Signs",
      descripcionEn = "Event nodes will always be Mysterious.",
      nombrePt = "Sinais Confusos",
      descripcionPt = "os nodos de Eventos sempre serão Misteriosos."
    },
    new PresagioDefinition
    {
      id = PilasDeRecursos,
      positivo = true,
      categoria = "nodos",
      presagiosIncompatibles = new[] { RecursosEscondidos },
      nombreEs = "Pilas de Recursos",
      descripcionEs = "los nodos de Recursos arrancan revelados en el mapa.",
      nombreEn = "Resource Piles",
      descripcionEn = "Resource nodes start revealed on the map.",
      nombrePt = "Pilhas de Recursos",
      descripcionPt = "os nodos de Recursos começam revelados no mapa."
    },
    new PresagioDefinition
    {
      id = RecursosEscondidos,
      positivo = false,
      categoria = "nodos",
      presagiosIncompatibles = new[] { PilasDeRecursos },
      nombreEs = "Recursos Escondidos",
      descripcionEs = "los nodos de Recursos siempre serán Misteriosos.",
      nombreEn = "Hidden Resources",
      descripcionEn = "Resource nodes will always be Mysterious.",
      nombrePt = "Recursos Escondidos",
      descripcionPt = "os nodos de Recursos sempre serão Misteriosos."
    },
    new PresagioDefinition
    {
      id = RumoresComeciales,
      positivo = true,
      categoria = "nodos",
      nombreEs = "Rumores Comeciales",
      descripcionEs = "los nodos de Puesto Comercial arrancan revelados en el mapa.",
      nombreEn = "Trade Rumors",
      descripcionEn = "Trading Post nodes start revealed on the map.",
      nombrePt = "Rumores Comerciais",
      descripcionPt = "os nodos de Posto Comercial começam revelados no mapa."
    },
    new PresagioDefinition
    {
      id = SenalesSagradas,
      positivo = true,
      categoria = "nodos",
      presagiosIncompatibles = new[] { TierraProfana },
      nombreEs = "Señales Sagradas",
      descripcionEs = "los nodos de Altares arrancan revelados en el mapa.",
      nombreEn = "Sacred Signs",
      descripcionEn = "Shrine nodes start revealed on the map.",
      nombrePt = "Sinais Sagrados",
      descripcionPt = "os nodos de Altares começam revelados no mapa."
    },
    new PresagioDefinition
    {
      id = TierraProfana,
      positivo = false,
      categoria = "nodos",
      presagiosIncompatibles = new[] { SenalesSagradas },
      nombreEs = "Tierra Profana",
      descripcionEs = "no habrá nodos de Altares en el mapa.",
      nombreEn = "Profane Land",
      descripcionEn = "there will be no Shrine nodes on the map.",
      nombrePt = "Terra Profana",
      descripcionPt = "não haverá nodos de Altares no mapa."
    },
    new PresagioDefinition
    {
      id = AdvertenciasDeAmenazas,
      positivo = true,
      categoria = "nodos",
      presagiosIncompatibles = new[] { AmenazasEscondidas },
      nombreEs = "Advertencias de Amenazas",
      descripcionEs = "los nodos de Batalla Elite arrancan revelados en el mapa.",
      nombreEn = "Threat Warnings",
      descripcionEn = "Elite Battle nodes start revealed on the map.",
      nombrePt = "Avisos de Ameaças",
      descripcionPt = "os nodos de Batalha de Elite começam revelados no mapa."
    },
    new PresagioDefinition
    {
      id = AmenazasEscondidas,
      positivo = false,
      categoria = "nodos",
      presagiosIncompatibles = new[] { AdvertenciasDeAmenazas },
      nombreEs = "Amenazas Escondidas",
      descripcionEs = "los nodos de Batalla Elite siempre serán Misteriosos.",
      nombreEn = "Hidden Threats",
      descripcionEn = "Elite Battle nodes will always be Mysterious.",
      nombrePt = "Ameaças Escondidas",
      descripcionPt = "os nodos de Batalha de Elite sempre serão Misteriosos."
    },
    new PresagioDefinition
    {
      id = PobladosEscasos,
      positivo = false,
      categoria = "nodos",
      presagiosIncompatibles = new[] { PobladosVividos },
      nombreEs = "Poblados Escasos",
      descripcionEs = "Menor cantidad de Asentamientos en la región y menos acciones disponibles.",
      nombreEn = "Sparse Settlements",
      descripcionEn = "Fewer Settlements in the region and fewer available actions.",
      nombrePt = "Povoados Escassos",
      descripcionPt = "Menos Assentamentos na região e menos ações disponíveis."
    },
    new PresagioDefinition
    {
      id = PobladosVividos,
      positivo = true,
      categoria = "nodos",
      presagiosIncompatibles = new[] { PobladosEscasos },
      nombreEs = "Poblados Vívidos",
      descripcionEs = "los Asentamientos dan mas civiles y sus precios bajan.",
      nombreEn = "Thriving Settlements",
      descripcionEn = "Settlements provide more civilians and their prices are lower.",
      nombrePt = "Povoados Vigorosos",
      descripcionPt = "os Assentamentos fornecem mais civis e seus preços são menores."
    },
    new PresagioDefinition
    {
      id = ZonaCartografiada,
      positivo = true,
      categoria = "exploracion",
      presagiosIncompatibles = new[] { ZonaDesconocida },
      nombreEs = "Región Cartografiada",
      descripcionEs = "una porción del mapa estará completamente explorada.",
      nombreEn = "Charted Region",
      descripcionEn = "a section of the map will be fully explored.",
      nombrePt = "Região Cartografada",
      descripcionPt = "uma parte do mapa estará completamente explorada."
    },
    new PresagioDefinition
    {
      id = ZonaDesconocida,
      positivo = false,
      categoria = "exploracion",
      presagiosIncompatibles = new[] { ZonaCartografiada },
      nombreEs = "Región Desconocida",
      descripcionEs = "La Exploración Pasiva será mas difícil.",
      nombreEn = "Unknown Region",
      descripcionEn = "Passive Exploration will be more difficult.",
      nombrePt = "Região Desconhecida",
      descripcionPt = "A Exploração Passiva será mais difícil."
    },
    new PresagioDefinition
    {
      id = SensacionPositiva,
      positivo = true,
      categoria = "esperanza",
      presagiosIncompatibles = new[] { SensacionNegativa },
      nombreEs = "Sensación Positiva",
      descripcionEs = "la Caravana arrancará con mas Esperanza.",
      nombreEn = "Positive Feeling",
      descripcionEn = "the Caravan will start with more Hope.",
      nombrePt = "Sensação Positiva",
      descripcionPt = "a Caravana começará com mais Esperança."
    },
    new PresagioDefinition
    {
      id = SensacionNegativa,
      positivo = false,
      categoria = "esperanza",
      presagiosIncompatibles = new[] { SensacionPositiva },
      nombreEs = "Sensación Negativa",
      descripcionEs = "la Caravana arrancará con menos Esperanza.",
      nombreEn = "Negative Feeling",
      descripcionEn = "the Caravan will start with less Hope.",
      nombrePt = "Sensação Negativa",
      descripcionPt = "a Caravana começará com menos Esperança."
    },
    new PresagioDefinition
    {
      id = NochesPacificas,
      positivo = true,
      categoria = "esperanza",
      presagiosIncompatibles = new[] { NochesTurbulentas },
      nombreEs = "Noches Pacíficas",
      descripcionEs = "al descansar se ganará esperanza.",
      nombreEn = "Peaceful Nights",
      descripcionEn = "resting will grant Hope.",
      nombrePt = "Noites Pacíficas",
      descripcionPt = "descansar concederá Esperança."
    },
    new PresagioDefinition
    {
      id = NochesTurbulentas,
      positivo = false,
      categoria = "esperanza",
      presagiosIncompatibles = new[] { NochesPacificas },
      nombreEs = "Noches Turbulentas",
      descripcionEs = "al descansar se perderá esperanza.",
      nombreEn = "Turbulent Nights",
      descripcionEn = "resting will reduce Hope.",
      nombrePt = "Noites Turbulentas",
      descripcionPt = "descansar reduzirá a Esperança."
    },
    new PresagioDefinition
    {
      id = CaminosPeligrosos,
      positivo = false,
      categoria = "peligros",
      presagiosIncompatibles = new[] { EnemigosDesprevenidos },
      nombreEs = "Caminos Peligrosos",
      descripcionEs = "aumenta levemente las chances de Emboscada de enemigos.",
      nombreEn = "Dangerous Roads",
      descripcionEn = "slightly increases the chance of enemy Ambushes.",
      nombrePt = "Caminhos Perigosos",
      descripcionPt = "aumenta levemente as chances de Emboscadas inimigas."
    },
    new PresagioDefinition
    {
      id = EnemigosDesprevenidos,
      positivo = true,
      categoria = "peligros",
      presagiosIncompatibles = new[] { CaminosPeligrosos },
      nombreEs = "Enemigos Desprevenidos",
      descripcionEs = "aumenta levemente las chances de Emboscada a enemigos.",
      nombreEn = "Unprepared Enemies",
      descripcionEn = "slightly increases the chance of Ambushing enemies.",
      nombrePt = "Inimigos Desprevenidos",
      descripcionPt = "aumenta levemente as chances de Emboscar inimigos."
    },
    new PresagioDefinition
    {
      id = AmenazasVigilantes,
      positivo = false,
      categoria = "peligros",
      presagiosIncompatibles = new[] { SinVigilancia },
      nombreEs = "Amenazas Vigilantes",
      descripcionEs = "la Alerta de la Región aumentará 1 mas al empezar el viaje.",
      nombreEn = "Watchful Threats",
      descripcionEn = "Region Alert will increase by 1 more when the journey begins.",
      nombrePt = "Ameaças Vigilantes",
      descripcionPt = "o Alerta da Região aumentará em mais 1 quando a jornada começar."
    },
    new PresagioDefinition
    {
      id = SinVigilancia,
      positivo = true,
      categoria = "peligros",
      presagiosIncompatibles = new[] { AmenazasVigilantes },
      nombreEs = "Sin Vigilancia",
      descripcionEs = "la Alerta de la Región no aumentará al empezar el viaje.",
      nombreEn = "No Surveillance",
      descripcionEn = "Region Alert will not increase when the journey begins.",
      nombrePt = "Sem Vigilância",
      descripcionPt = "o Alerta da Região não aumentará quando a jornada começar."
    },
    new PresagioDefinition
    {
      id = AventuraMemorable,
      positivo = true,
      categoria = "personajes",
      nombreEs = "Aventura Memorable",
      descripcionEs = "los personajes obtendrán más Experiencia.",
      nombreEn = "Memorable Adventure",
      descripcionEn = "characters will gain more Experience.",
      nombrePt = "Aventura Memorável",
      descripcionPt = "os personagens receberão mais Experiência."
    },
    new PresagioDefinition
    {
      id = AventuraOlvidable,
      positivo = false,
      categoria = "personajes",
      nombreEs = "Aventura Olvidable",
      descripcionEs = "los personajes obtendrán menos Experiencia.",
      nombreEn = "Forgettable Adventure",
      descripcionEn = "characters will gain less Experience.",
      nombrePt = "Aventura Esquecível",
      descripcionPt = "os personagens receberão menos Experiência."
    },
    new PresagioDefinition
    {
      id = PlagaEnLaRegion,
      positivo = false,
      categoria = "personajes",
      nombreEs = "Plaga en la Región",
      descripcionEs = "al descansar, los personajes deberan resitir con Fortaleza o Enfermarán.",
      nombreEn = "Plague in the Region",
      descripcionEn = "when resting, characters must resist with Fortitude or become Sick.",
      nombrePt = "Praga na Região",
      descripcionPt = "ao descansar, os personagens deverão resistir com Fortitude ou ficarão Doentes."
    },
    new PresagioDefinition
    {
      id = PlantasCurativas,
      positivo = true,
      categoria = "personajes",
      nombreEs = "Plantas Curativas",
      descripcionEs = "los personajes se curarán más rápido de forma pasiva.",
      nombreEn = "Healing Plants",
      descripcionEn = "characters will heal faster passively.",
      nombrePt = "Plantas Curativas",
      descripcionPt = "os personagens se curarão mais rápido passivamente."
    },
    new PresagioDefinition
    {
      id = CorrupcionInsoportable,
      positivo = false,
      categoria = "personajes",
      nombreEs = "Corrupción Insoportable",
      descripcionEs = "al comenzar, los personajes deberan resitir con Fortaleza o serán Corrompidos.",
      nombreEn = "Unbearable Corruption",
      descripcionEn = "at the beginning, characters must resist with Fortitude or become Corrupted.",
      nombrePt = "Corrupção Insuportável",
      descripcionPt = "no início, os personagens deverão resistir com Fortitude ou serão Corrompidos."
    },
    new PresagioDefinition
    {
      id = RegionBendecida,
      positivo = true,
      categoria = "personajes",
      nombreEs = "Región Bendecida",
      descripcionEs = "al comenzar los personajes que superen prueba Mental serán Bendecidos.",
      nombreEn = "Blessed Region",
      descripcionEn = "at the beginning, characters who pass a Mental test will become Blessed.",
      nombrePt = "Região Abençoada",
      descripcionPt = "no início, os personagens que passarem em um teste Mental serão Abençoados."
    },
    new PresagioDefinition
    {
      id = Espejismos,
      positivo = false,
      categoria = "exploracion",
      nombreEs = "Espejismos",
      descripcionEs = "al descansar, los nodos conocidos podrán cambiar de tipo aleatoriamente.",
      nombreEn = "Mirages",
      descripcionEn = "when resting, known nodes may randomly change type.",
      nombrePt = "Miragens",
      descripcionPt = "ao descansar, os nós conhecidos poderão mudar de tipo aleatoriamente."
    },
    new PresagioDefinition
    {
      id = VientoAFavor,
      positivo = false,
      categoria = "aliento_negro",
      nombreEs = "Viento a favor",
      descripcionEs = "El Aliento Negro comenzará un poco mas adelante.",
      nombreEn = "Tailwind",
      descripcionEn = "The Black Breath will begin a little farther ahead.",
      nombrePt = "Vento a Favor",
      descripcionPt = "O Hálito Negro começará um pouco mais adiante."
    },
    new PresagioDefinition
    {
      id = VientoEnContra,
      positivo = true,
      categoria = "aliento_negro",
      nombreEs = "Viento en contra",
      descripcionEs = "El Aliento Negro comenzará mas atrás.",
      nombreEn = "Headwind",
      descripcionEn = "The Black Breath will begin farther behind.",
      nombrePt = "Vento Contrário",
      descripcionPt = "O Hálito Negro começará mais atrás."
    },
    new PresagioDefinition
    {
      id = AireLimpio,
      positivo = true,
      categoria = "aliento_negro",
      nombreEs = "Aire Limpio",
      descripcionEs = "Los efectos del Aliento Negro serán menos nocivos para la caravana.",
      nombreEn = "Clean Air",
      descripcionEn = "The effects of the Black Breath will be less harmful to the caravan.",
      nombrePt = "Ar Limpo",
      descripcionPt = "Os efeitos do Hálito Negro serão menos nocivos para a caravana."
    },
    new PresagioDefinition
    {
      id = AirePutrido,
      positivo = false,
      categoria = "aliento_negro",
      nombreEs = "Aire Pútrido",
      descripcionEs = "Los efectos del Aliento Negro serán mas nocivos para la caravana.",
      nombreEn = "Putrid Air",
      descripcionEn = "The effects of the Black Breath will be more harmful to the caravan.",
      nombrePt = "Ar Pútrido",
      descripcionPt = "Os efeitos do Hálito Negro serão mais nocivos para a caravana."
    },
    new PresagioDefinition
    {
      id = LeyDelMasFuerte,
      positivo = false,
      categoria = "enemigos",
      nombreEs = "Ley del Más Fuerte",
      descripcionEs = "mayor presencia de Bandidos en la Región.",
      nombreEn = "Law of the Strongest",
      descripcionEn = "greater Bandit presence in the Region.",
      nombrePt = "Lei do Mais Forte",
      descripcionPt = "maior presença de Bandidos na Região."
    },
    new PresagioDefinition
    {
      id = CorrompidosAlAcecho,
      positivo = false,
      categoria = "enemigos",
      nombreEs = "Corrompidos al Acecho",
      descripcionEs = "mayor presencia de Corrompidos en la Región.",
      nombreEn = "Corrupted Lurking",
      descripcionEn = "greater Corrupted presence in the Region.",
      nombrePt = "Corrompidos à Espreita",
      descripcionPt = "maior presença de Corrompidos na Região."
    },
    new PresagioDefinition
    {
      id = VenganadoresCazando,
      positivo = false,
      categoria = "enemigos",
      nombreEs = "Venganadores Cazando",
      descripcionEs = "mayor presencia de Vengadores de Kadryn en la Región.",
      nombreEn = "Avengers Hunting",
      descripcionEn = "greater presence of Kadryn Avengers in the Region.",
      nombrePt = "Vingadores Caçando",
      descripcionPt = "maior presença de Vingadores de Kadryn na Região."
    },
    new PresagioDefinition
    {
      id = CentinelasLocales,
      positivo = false,
      categoria = "enemigos",
      nombreEs = "Centinelas Locales",
      descripcionEs = "mayor presencia de Enemigos autóctonos de la Región.",
      nombreEn = "Local Sentinels",
      descripcionEn = "greater presence of native Enemies in the Region.",
      nombrePt = "Sentinelas Locais",
      descripcionPt = "maior presença de Inimigos nativos da Região."
    },
    new PresagioDefinition
    {
      id = AmenazaSuperior,
      positivo = false,
      categoria = "enemigos",
      nombreEs = "Amenaza Superior",
      descripcionEs = "mayor cantidad de refuerzos enemigos.",
      nombreEn = "Superior Threat",
      descripcionEn = "greater numbers of enemy reinforcements.",
      nombrePt = "Ameaça Superior",
      descripcionPt = "maior quantidade de reforços inimigos."
    }
  };

  public static int ObtenerIdiomaActual()
  {
    return TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol;
  }

  public static string ObtenerTextoSinPresagios()
  {
    switch (ObtenerIdiomaActual())
    {
      case TRADU.IdiomaIngles:
        return "No omens.";
      case TRADU.IdiomaPortugues:
        return "Sem presságios.";
      default:
        return "Sin presagios.";
    }
  }

  public static string FormatearTitulo(string titulo, bool positivo)
  {
    if (string.IsNullOrWhiteSpace(titulo))
    {
      return string.Empty;
    }

    string tituloSinDosPuntos = titulo.Trim().TrimEnd(':').TrimEnd();
    string color = positivo ? ColorTituloPresagioPositivo : ColorTituloPresagioNegativo;
    return "<color=" + color + "><b>" + tituloSinDosPuntos + ":</b></color>";
  }

  public static PresagioDefinition ObtenerPorId(int presagioId)
  {
    for (int i = 0; i < definiciones.Count; i++)
    {
      PresagioDefinition definicion = definiciones[i];
      if (definicion != null && definicion.id == presagioId)
      {
        return definicion;
      }
    }

    return null;
  }

  public static string ObtenerTextoLocalizado(int presagioId)
  {
    PresagioDefinition definicion = ObtenerPorId(presagioId);
    return definicion != null ? definicion.ObtenerTextoLocalizado() : string.Empty;
  }

  public static string ObtenerNombreLocalizado(int presagioId)
  {
    PresagioDefinition definicion = ObtenerPorId(presagioId);
    if (definicion == null)
    {
      return string.Empty;
    }

    switch (ObtenerIdiomaActual())
    {
      case TRADU.IdiomaIngles:
        return definicion.nombreEn;
      case TRADU.IdiomaPortugues:
        return definicion.nombrePt;
      default:
        return definicion.nombreEs;
    }
  }

  public static bool HayPresagiosDisponiblesParaRegion(int regionId)
  {
    for (int i = 0; i < definiciones.Count; i++)
    {
      if (definiciones[i] != null && definiciones[i].EstaDisponibleEnRegion(regionId))
      {
        return true;
      }
    }

    return false;
  }

#if UNITY_EDITOR
  public static List<PresagioDefinition> ObtenerDisponiblesParaDebug(int regionId)
  {
    List<PresagioDefinition> resultado = new List<PresagioDefinition>();
    for (int i = 0; i < definiciones.Count; i++)
    {
      PresagioDefinition definicion = definiciones[i];
      if (definicion != null && definicion.EstaDisponibleEnRegion(regionId))
      {
        resultado.Add(definicion);
      }
    }

    return resultado;
  }
#endif

  public static List<int> SortearParaRegion(int regionId)
  {
    List<PresagioDefinition> disponibles = new List<PresagioDefinition>();
    for (int i = 0; i < definiciones.Count; i++)
    {
      PresagioDefinition definicion = definiciones[i];
      if (definicion != null && definicion.EstaDisponibleEnRegion(regionId))
      {
        disponibles.Add(definicion);
      }
    }

    List<int> resultado = new List<int>();
    if (disponibles.Count == 0)
    {
      return resultado;
    }

    int tirada = UnityEngine.Random.Range(1, 101);
    int cantidad = tirada <= 30 ? 0 : tirada <= 80 ? 1 : 2;
    cantidad = Mathf.Min(cantidad, disponibles.Count);

    for (int i = 0; i < cantidad && disponibles.Count > 0; i++)
    {
      int indice = UnityEngine.Random.Range(0, disponibles.Count);
      PresagioDefinition elegida = disponibles[indice];
      resultado.Add(elegida.id);
      disponibles.RemoveAll(candidata => !SonCompatibles(elegida, candidata));
    }

    return resultado;
  }

  private static bool SonCompatibles(PresagioDefinition primera, PresagioDefinition segunda)
  {
    if (primera == null || segunda == null || primera.id == segunda.id)
    {
      return false;
    }

    if (!string.IsNullOrWhiteSpace(primera.categoria)
      && !string.IsNullOrWhiteSpace(segunda.categoria)
      && string.Equals(primera.categoria, segunda.categoria, StringComparison.OrdinalIgnoreCase))
    {
      return false;
    }

    return !ContieneId(primera.presagiosIncompatibles, segunda.id)
      && !ContieneId(segunda.presagiosIncompatibles, primera.id);
  }

  private static bool ContieneId(int[] ids, int id)
  {
    if (ids == null)
    {
      return false;
    }

    for (int i = 0; i < ids.Length; i++)
    {
      if (ids[i] == id)
      {
        return true;
      }
    }

    return false;
  }
}

[Serializable]
public sealed class PresagioRegionPendienteSaveData
{
  public int regionId;
  public List<int> presagioIds = new List<int>();
}

[Serializable]
internal sealed class PresagiosPendientesGlobalSaveData
{
  public List<PresagioRegionPendienteSaveData> regiones = new List<PresagioRegionPendienteSaveData>();
}

/// <summary>
/// Persistencia global de regiones pendientes. Es independiente de una campaña
/// para impedir que cerrar y volver a abrir Prepartida rerollee los presagios.
/// </summary>
public static class PresagioRegionPendienteStore
{
  private const string PlayerPrefsKey = "presagios_regiones_pendientes_v1";
  private static PresagiosPendientesGlobalSaveData datos;
  private static bool cargado;

#if UNITY_EDITOR
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
  private static void ReiniciarPendientesAlEntrarEnPlayMode()
  {
    datos = new PresagiosPendientesGlobalSaveData();
    cargado = true;
    Guardar();
  }
#endif

  public static List<int> ObtenerOCrear(int regionId)
  {
    CargarSiHaceFalta();
    PresagioRegionPendienteSaveData existente = BuscarRegion(regionId);
    if (existente != null)
    {
      return CopiarIds(existente.presagioIds);
    }

    // Mientras el catálogo esté vacío no se fija una tirada artificial.
    if (!PresagioCatalog.HayPresagiosDisponiblesParaRegion(regionId))
    {
      return new List<int>();
    }

    PresagioRegionPendienteSaveData nueva = new PresagioRegionPendienteSaveData
    {
      regionId = regionId,
      presagioIds = PresagioCatalog.SortearParaRegion(regionId)
    };
    datos.regiones.Add(nueva);
    Guardar();
    return CopiarIds(nueva.presagioIds);
  }

  public static List<int> Consumir(int regionId)
  {
    List<int> resultado = ObtenerOCrear(regionId);
    PresagioRegionPendienteSaveData existente = BuscarRegion(regionId);
    if (existente != null)
    {
      datos.regiones.Remove(existente);
      Guardar();
    }

    return resultado;
  }

#if UNITY_EDITOR
  public static void ForzarParaDebug(int regionId, List<int> presagioIds)
  {
    if (regionId <= 0)
    {
      return;
    }

    CargarSiHaceFalta();
    PresagioRegionPendienteSaveData existente = BuscarRegion(regionId);
    if (existente != null)
    {
      datos.regiones.Remove(existente);
    }

    List<int> idsValidos = new List<int>();
    if (presagioIds != null)
    {
      for (int i = 0; i < presagioIds.Count && idsValidos.Count < 2; i++)
      {
        int id = presagioIds[i];
        PresagioDefinition definicion = PresagioCatalog.ObtenerPorId(id);
        if (definicion != null
          && definicion.EstaDisponibleEnRegion(regionId)
          && !idsValidos.Contains(id))
        {
          idsValidos.Add(id);
        }
      }
    }

    datos.regiones.Add(new PresagioRegionPendienteSaveData
    {
      regionId = regionId,
      presagioIds = idsValidos
    });
    Guardar();
  }
#endif

  public static List<PresagioRegionPendienteSaveData> Exportar()
  {
    CargarSiHaceFalta();
    List<PresagioRegionPendienteSaveData> copia = new List<PresagioRegionPendienteSaveData>();
    for (int i = 0; i < datos.regiones.Count; i++)
    {
      PresagioRegionPendienteSaveData region = datos.regiones[i];
      if (region == null || region.regionId <= 0)
      {
        continue;
      }

      copia.Add(new PresagioRegionPendienteSaveData
      {
        regionId = region.regionId,
        presagioIds = CopiarIds(region.presagioIds)
      });
    }

    return copia;
  }

  public static void ImportarSiNoHayEstadoGlobal(List<PresagioRegionPendienteSaveData> regionesGuardadas)
  {
    CargarSiHaceFalta();
    if (PlayerPrefs.HasKey(PlayerPrefsKey)
      || datos.regiones.Count > 0
      || regionesGuardadas == null
      || regionesGuardadas.Count == 0)
    {
      return;
    }

    for (int i = 0; i < regionesGuardadas.Count; i++)
    {
      PresagioRegionPendienteSaveData region = regionesGuardadas[i];
      if (region == null || region.regionId <= 0 || BuscarRegion(region.regionId) != null)
      {
        continue;
      }

      datos.regiones.Add(new PresagioRegionPendienteSaveData
      {
        regionId = region.regionId,
        presagioIds = CopiarIds(region.presagioIds)
      });
    }

    Guardar();
  }

  private static void CargarSiHaceFalta()
  {
    if (cargado)
    {
      return;
    }

    cargado = true;
    datos = new PresagiosPendientesGlobalSaveData();
    if (!PlayerPrefs.HasKey(PlayerPrefsKey))
    {
      return;
    }

    string json = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);
    if (string.IsNullOrWhiteSpace(json))
    {
      return;
    }

    try
    {
      PresagiosPendientesGlobalSaveData cargados = JsonUtility.FromJson<PresagiosPendientesGlobalSaveData>(json);
      if (cargados != null && cargados.regiones != null)
      {
        datos = cargados;
      }
    }
    catch (Exception ex)
    {
      Debug.LogWarning("[Presagios] No se pudo leer el estado pendiente: " + ex.Message);
    }

    Normalizar();
  }

  private static void Normalizar()
  {
    if (datos.regiones == null)
    {
      datos.regiones = new List<PresagioRegionPendienteSaveData>();
      return;
    }

    HashSet<int> regionesVistas = new HashSet<int>();
    for (int i = datos.regiones.Count - 1; i >= 0; i--)
    {
      PresagioRegionPendienteSaveData region = datos.regiones[i];
      if (region == null || region.regionId <= 0 || !regionesVistas.Add(region.regionId))
      {
        datos.regiones.RemoveAt(i);
        continue;
      }

      if (region.presagioIds == null)
      {
        region.presagioIds = new List<int>();
      }
    }
  }

  private static PresagioRegionPendienteSaveData BuscarRegion(int regionId)
  {
    if (regionId <= 0 || datos == null || datos.regiones == null)
    {
      return null;
    }

    for (int i = 0; i < datos.regiones.Count; i++)
    {
      PresagioRegionPendienteSaveData region = datos.regiones[i];
      if (region != null && region.regionId == regionId)
      {
        return region;
      }
    }

    return null;
  }

  private static List<int> CopiarIds(List<int> ids)
  {
    return ids != null ? new List<int>(ids) : new List<int>();
  }

  private static void Guardar()
  {
    PlayerPrefs.SetString(PlayerPrefsKey, JsonUtility.ToJson(datos));
    PlayerPrefs.Save();
  }
}
