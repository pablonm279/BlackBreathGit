using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// Herramienta de Editor para cerrar la brecha de contenido de items detectada para
// Duelista (le falta el escalon Epico) y Canalizador (no tiene ni un arma ni una
// armadura propia todavia). Sigue la misma filosofia que Editor/ItemDatabaseTools.cs:
// no escribe .prefab a mano, deja que Unity lo serialice via AssetDatabase/PrefabUtility.
//
// Uso: Tools > Items > Sets Faltantes > ... (correr en este orden, o usar la opcion 3
// que hace todo y refresca el Item Database al final).
//
// Generado por Claude a pedido de Pablo, en base al analisis de:
// - Item.cs / Arma.cs / Armadura.cs / ItemDatabase.cs (modelo de datos)
// - Editor/ItemDatabaseTools.cs (patron de duplicar/editar prefabs desde Editor)
// - ClaseCanalizador.cs (API real de energia arcana: CambiarEnergia/ObtenerEnergia)
// - Habilidad.cs (contrato abstracto: Awake/ActualizarDescripcion/Activar/AplicarEfectosHabilidad)
public static class GenerarItemsClasesFaltantes
{
    private const string MenuRoot = "Tools/Items/Sets Faltantes/";

    [MenuItem(MenuRoot + "1. Generar Epico Duelista (Estoque + Gambeson)")]
    public static void GenerarEpicoDuelista()
    {
        GenerarEstoqueEpicoDuelista();
        GenerarGambesonEpicoDuelista();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog(
            "Items Duelista",
            "Listo. Se generaron (o ya existian) los items Epicos de Duelista.\n\nAhora corre 'Tools/Items/Create Or Refresh Item Database' para registrarlos en el ItemDatabase.",
            "OK");
    }

    [MenuItem(MenuRoot + "2. Generar Set Completo Canalizador (Guantelete + Manto)")]
    public static void GenerarSetCanalizador()
    {
        GenerarLineaGuanteleteCanalizador();
        GenerarLineaMantoCanalizador();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog(
            "Items Canalizador",
            "Listo. Se genero (o ya existia) el set completo de Guantelete y Manto para Canalizador (5 items por categoria: Comun, 2x Infrecuente, Raro, Epico).\n\nAhora corre 'Tools/Items/Create Or Refresh Item Database' para registrarlos en el ItemDatabase.\n\nNota: el Manto no tiene sprite propio todavia (imItem queda vacio) - hace falta arte o correr el generador de iconos para esa linea.",
            "OK");
    }

    [MenuItem(MenuRoot + "3. Generar Todo + Refrescar Item Database")]
    public static void GenerarTodoYRefrescar()
    {
        GenerarEstoqueEpicoDuelista();
        GenerarGambesonEpicoDuelista();
        GenerarLineaGuanteleteCanalizador();
        GenerarLineaMantoCanalizador();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        ItemDatabaseTools.CreateOrRefreshItemDatabase();
    }

    // ==================== DUELISTA - EPICO ====================
    // Completa el escalon que le faltaba (Comun/Infrecuente/Raro ya existian).

    private static void GenerarEstoqueEpicoDuelista()
    {
        const string templatePath = "Assets/Scripts/Clases/Duelista/_ItemsExclusivos/Armas/Estoque/_Estoque Base/armaEstoque.prefab";
        const string carpetaDestino = "Assets/Scripts/Clases/Duelista/_ItemsExclusivos/Armas/Estoque/Epico_EstoqueDeLaMedianoche";
        const string destinoPath = carpetaDestino + "/armaEstoqueDeLaMedianoche.prefab";

        if (AssetDatabase.LoadAssetAtPath<GameObject>(destinoPath) != null)
        {
            Debug.Log("[ItemsFaltantes] Ya existe " + destinoPath + ", no se regenera.");
            return;
        }

        GameObject template = AssetDatabase.LoadAssetAtPath<GameObject>(templatePath);
        if (template == null)
        {
            Debug.LogWarning("[ItemsFaltantes] No se encontro la plantilla base de Estoque en '" + templatePath + "'. Se aborta la generacion del arma Epica de Duelista. Revisa que la ruta siga siendo correcta.");
            return;
        }

        EnsureFolder(carpetaDestino);
        if (!AssetDatabase.CopyAsset(templatePath, destinoPath))
        {
            Debug.LogWarning("[ItemsFaltantes] No se pudo duplicar '" + templatePath + "' a '" + destinoPath + "'.");
            return;
        }

        AssetDatabase.Refresh();
        GameObject nuevo = AssetDatabase.LoadAssetAtPath<GameObject>(destinoPath);
        Arma arma = nuevo != null ? nuevo.GetComponent<Arma>() : null;
        if (arma == null)
        {
            Debug.LogWarning("[ItemsFaltantes] El prefab duplicado en '" + destinoPath + "' no tiene un componente Arma. Revisar a mano.");
            return;
        }

        arma.sNombreItem = "Estoque de la Medianoche";
        arma.itemDescripcion = "Un estoque templado bajo la luna nueva. Cada estocada parece recordar al duelo anterior.";
        arma.iRareza = 3; // Epico
        arma.nivelMejora = 0;
        arma.iPrecio = 520;
        arma.requisitoFue = 2;
        arma.requisitoAgi = 6;
        arma.buffFuerza = 1;
        arma.buffAgi = 4;
        arma.buffIniciativa = 2;
        arma.buffTSReflejo = 2;

        DebuffImpactoArmaData sangradoCertero = new DebuffImpactoArmaData();
        sangradoCertero.nombreDebuff = "Sangrado Certero";
        sangradoCertero.probabilidadAplicar = 25;
        sangradoCertero.duracionRondas = 2;
        sangradoCertero.requiereTiradaSalvacion = false;
        sangradoCertero.stacksSangrado = 2;
        arma.debuffsImpactoArma = new List<DebuffImpactoArmaData> { sangradoCertero };

        CrearHijoHabilidadExtra1<MedianocheFilo>(nuevo, arma);

        EditorUtility.SetDirty(arma);
        EditorUtility.SetDirty(nuevo);
        Debug.Log("[ItemsFaltantes] Generado: " + destinoPath);
    }

    private static void GenerarGambesonEpicoDuelista()
    {
        const string templatePath = "Assets/Scripts/Clases/Duelista/_ItemsExclusivos/Armaduras/Gambeson/_Gambeson Base/armaduraGambeson.prefab";
        const string carpetaDestino = "Assets/Scripts/Clases/Duelista/_ItemsExclusivos/Armaduras/Gambeson/Epico_GambesonDelCompasPerfecto";
        const string destinoPath = carpetaDestino + "/armaduraGambesonDelCompasPerfecto.prefab";

        if (AssetDatabase.LoadAssetAtPath<GameObject>(destinoPath) != null)
        {
            Debug.Log("[ItemsFaltantes] Ya existe " + destinoPath + ", no se regenera.");
            return;
        }

        GameObject template = AssetDatabase.LoadAssetAtPath<GameObject>(templatePath);
        if (template == null)
        {
            Debug.LogWarning("[ItemsFaltantes] No se encontro la plantilla base de Gambeson en '" + templatePath + "'. Se aborta la generacion de la armadura Epica de Duelista. Revisa que la ruta siga siendo correcta.");
            return;
        }

        EnsureFolder(carpetaDestino);
        if (!AssetDatabase.CopyAsset(templatePath, destinoPath))
        {
            Debug.LogWarning("[ItemsFaltantes] No se pudo duplicar '" + templatePath + "' a '" + destinoPath + "'.");
            return;
        }

        AssetDatabase.Refresh();
        GameObject nuevo = AssetDatabase.LoadAssetAtPath<GameObject>(destinoPath);
        Armadura armadura = nuevo != null ? nuevo.GetComponent<Armadura>() : null;
        if (armadura == null)
        {
            Debug.LogWarning("[ItemsFaltantes] El prefab duplicado en '" + destinoPath + "' no tiene un componente Armadura. Revisar a mano.");
            return;
        }

        armadura.sNombreItem = "Gambesón del Compás Perfecto";
        armadura.itemDescripcion = "Cosido para acompañar cada paso, sin estorbar ni uno solo.";
        armadura.iRareza = 3;
        armadura.nivelMejora = 0;
        armadura.iPrecio = 480;
        armadura.requisitoAgi = 5;
        armadura.buffAgi = 3;
        armadura.buffDefensa = 3;
        armadura.buffTSReflejo = 2;
        armadura.buffhpMax = 8;
        armadura.reduccionDanioCriticoRecibidoPorcentaje = 10;

        CrearHijoHabilidadExtra1<CompasPerfecto>(nuevo, armadura);

        EditorUtility.SetDirty(armadura);
        EditorUtility.SetDirty(nuevo);
        Debug.Log("[ItemsFaltantes] Generado: " + destinoPath);
    }

    // ==================== CANALIZADOR - SET COMPLETO ====================
    // No existia ningun arma/armadura propia. Arma = Guantelete (unico tipo de
    // overlay de icono libre en ItemIconGeneratorConfig). Armadura = linea nueva "Manto".
    // Distribucion de rareza igual a la que ya usan Caballero/Explorador/etc:
    // 1 Comun + 2 Infrecuente + 1 Raro + 1 Epico.

    private static void GenerarLineaGuanteleteCanalizador()
    {
        const string carpetaDestino = "Assets/Scripts/Clases/Canalizador/_ItemsExclusivos/Armas";
        EnsureFolder(carpetaDestino);
        Sprite iconoBase = ObtenerIconoGuanteleteExistente();

        CrearArma(
            carpeta: carpetaDestino, nombreArchivo: "armaGuanteleteCanalizador",
            nombre: "Guantelete Canalizador",
            descripcion: "Un guantelete de cuero reforzado con hilos conductores. El punto de partida de todo Canalizador.",
            rareza: 0, precio: 90, requisitoPoder: 2,
            buffPoder: 2, buffIniciativa: 0, buffAgi: 0, buffTSMental: 0,
            icono: iconoBase, crearHabilidadExtra1: null);

        CrearArma(
            carpeta: carpetaDestino, nombreArchivo: "armaGuanteleteResonancia",
            nombre: "Guantelete de Resonancia",
            descripcion: "Vibra levemente al canalizar, como si respondiera antes de ser invocado.",
            rareza: 1, precio: 165, requisitoPoder: 3,
            buffPoder: 3, buffIniciativa: 0, buffAgi: 0, buffTSMental: 2,
            icono: iconoBase, crearHabilidadExtra1: null);

        CrearArma(
            carpeta: carpetaDestino, nombreArchivo: "armaGuanteleteCircuitoRoto",
            nombre: "Guantelete del Circuito Roto",
            descripcion: "Las runas estan fracturadas, pero eso solo lo vuelve mas impredecible.",
            rareza: 1, precio: 175, requisitoPoder: 3,
            buffPoder: 3, buffIniciativa: 2, buffAgi: 0, buffTSMental: 0,
            icono: iconoBase, crearHabilidadExtra1: null);

        CrearArma(
            carpeta: carpetaDestino, nombreArchivo: "armaGuanteleteSobrecargaControlada",
            nombre: "Guantelete de Sobrecarga Controlada",
            descripcion: "Deja escapar un poco de energía arcana en cada golpe, a propósito.",
            rareza: 2, precio: 260, requisitoPoder: 5,
            buffPoder: 4, buffIniciativa: 0, buffAgi: 1, buffTSMental: 0,
            icono: iconoBase, crearHabilidadExtra1: raiz => CrearHijoHabilidad<SobrecargaControlada>(raiz));

        CrearArma(
            carpeta: carpetaDestino, nombreArchivo: "armaGuanteleteVorticeArcano",
            nombre: "Guantelete del Vórtice Arcano",
            descripcion: "Un vórtice diminuto y estable gira en la palma. Nunca deja de girar del todo.",
            rareza: 3, precio: 380, requisitoPoder: 7,
            buffPoder: 6, buffIniciativa: 2, buffAgi: 0, buffTSMental: 0,
            icono: iconoBase, crearHabilidadExtra1: raiz => CrearHijoHabilidad<VorticeArcano>(raiz));
    }

    private static void GenerarLineaMantoCanalizador()
    {
        const string carpetaDestino = "Assets/Scripts/Clases/Canalizador/_ItemsExclusivos/Armaduras";
        EnsureFolder(carpetaDestino);

        CrearArmadura(
            carpeta: carpetaDestino, nombreArchivo: "armaduraMantoAprendizArcano",
            nombre: "Manto del Aprendiz Arcano",
            descripcion: "Tela simple, tejida para no interferir con el flujo de energía.",
            rareza: 0, precio: 90, requisitoPoder: 0,
            buffTSMental: 2, buffhpMax: 4, buffDefensa: 0, buffResArcano: 0,
            crearHabilidadExtra1: null);

        CrearArmadura(
            carpeta: carpetaDestino, nombreArchivo: "armaduraMantoContencion",
            nombre: "Manto de Contención",
            descripcion: "Cosido con hilo resistente al arcano, para que la energía no se escape antes de tiempo.",
            rareza: 1, precio: 165, requisitoPoder: 2,
            buffTSMental: 2, buffhpMax: 0, buffDefensa: 0, buffResArcano: 2,
            crearHabilidadExtra1: null);

        CrearArmadura(
            carpeta: carpetaDestino, nombreArchivo: "armaduraMantoCircuitoEstable",
            nombre: "Manto del Circuito Estable",
            descripcion: "Las runas de este manto se mantienen en su lugar incluso bajo presión.",
            rareza: 1, precio: 175, requisitoPoder: 2,
            buffTSMental: 2, buffhpMax: 0, buffDefensa: 1, buffResArcano: 0,
            crearHabilidadExtra1: null);

        CrearArmadura(
            carpeta: carpetaDestino, nombreArchivo: "armaduraMantoDisipacion",
            nombre: "Manto de Disipación",
            descripcion: "Dispersa el exceso de energía antes de que desestabilice a quien lo lleva.",
            rareza: 2, precio: 260, requisitoPoder: 4,
            buffTSMental: 3, buffhpMax: 0, buffDefensa: 0, buffResArcano: 3,
            crearHabilidadExtra1: raiz => CrearHijoHabilidad<DisipacionArcana>(raiz));

        CrearArmadura(
            carpeta: carpetaDestino, nombreArchivo: "armaduraMantoNucleoInestable",
            nombre: "Manto del Núcleo Inestable",
            descripcion: "En su centro late un núcleo que absorbe parte del desgaste de canalizar demasiado.",
            rareza: 3, precio: 380, requisitoPoder: 6,
            buffTSMental: 4, buffhpMax: 6, buffDefensa: 0, buffResArcano: 4,
            crearHabilidadExtra1: raiz => CrearHijoHabilidad<NucleoInestable>(raiz));
    }

    // ==================== HELPERS ====================

    private static void CrearArma(
        string carpeta, string nombreArchivo, string nombre, string descripcion,
        int rareza, int precio, int requisitoPoder,
        int buffPoder, int buffIniciativa, int buffAgi, int buffTSMental,
        Sprite icono, Func<GameObject, Habilidad> crearHabilidadExtra1)
    {
        string path = carpeta + "/" + nombreArchivo + ".prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            Debug.Log("[ItemsFaltantes] Ya existe " + path + ", no se regenera.");
            return;
        }

        GameObject go = new GameObject(nombreArchivo);
        ArmaGuanteleteCanalizador arma = go.AddComponent<ArmaGuanteleteCanalizador>();

        arma.sNombreItem = nombre;
        arma.itemDescripcion = descripcion;
        arma.iRareza = rareza;
        arma.nivelMejora = 0;
        arma.iPrecio = precio;
        arma.requisitoPoder = requisitoPoder;
        arma.buffPoder = buffPoder;
        arma.buffIniciativa = buffIniciativa;
        arma.buffAgi = buffAgi;
        arma.buffTSMental = buffTSMental;
        arma.IDClasesQuePuedenUsarEsteItem = new List<int> { 5 }; // 5 = Canalizador (ver Equipo.cs)
        if (icono != null)
        {
            arma.imItem = icono;
        }

        if (crearHabilidadExtra1 != null)
        {
            Habilidad hab = crearHabilidadExtra1(go);
            arma.habilidadExtra1 = hab;
        }

        PrefabUtility.SaveAsPrefabAsset(go, path);
        UnityEngine.Object.DestroyImmediate(go);
        Debug.Log("[ItemsFaltantes] Generado: " + path);
    }

    private static void CrearArmadura(
        string carpeta, string nombreArchivo, string nombre, string descripcion,
        int rareza, int precio, int requisitoPoder,
        int buffTSMental, int buffhpMax, int buffDefensa, int buffResArcano,
        Func<GameObject, Habilidad> crearHabilidadExtra1)
    {
        string path = carpeta + "/" + nombreArchivo + ".prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            Debug.Log("[ItemsFaltantes] Ya existe " + path + ", no se regenera.");
            return;
        }

        GameObject go = new GameObject(nombreArchivo);
        ArmaduraMantoCanalizador armadura = go.AddComponent<ArmaduraMantoCanalizador>();

        armadura.sNombreItem = nombre;
        armadura.itemDescripcion = descripcion;
        armadura.iRareza = rareza;
        armadura.nivelMejora = 0;
        armadura.iPrecio = precio;
        armadura.requisitoPoder = requisitoPoder;
        armadura.buffTSMental = buffTSMental;
        armadura.buffhpMax = buffhpMax;
        armadura.buffDefensa = buffDefensa;
        armadura.buffResArcano = buffResArcano;
        armadura.IDClasesQuePuedenUsarEsteItem = new List<int> { 5 }; // 5 = Canalizador

        if (crearHabilidadExtra1 != null)
        {
            Habilidad hab = crearHabilidadExtra1(go);
            armadura.habilidadExtra1 = hab;
        }

        PrefabUtility.SaveAsPrefabAsset(go, path);
        UnityEngine.Object.DestroyImmediate(go);
        Debug.Log("[ItemsFaltantes] Generado: " + path);
    }

    private static T CrearHijoHabilidad<T>(GameObject raiz) where T : Habilidad
    {
        GameObject hijo = new GameObject("HabilidadExtra1");
        hijo.transform.SetParent(raiz.transform);
        return hijo.AddComponent<T>();
    }

    private static void CrearHijoHabilidadExtra1<T>(GameObject raiz, Arma arma) where T : Habilidad
    {
        arma.habilidadExtra1 = CrearHijoHabilidad<T>(raiz);
    }

    private static void CrearHijoHabilidadExtra1<T>(GameObject raiz, Armadura armadura) where T : Habilidad
    {
        armadura.habilidadExtra1 = CrearHijoHabilidad<T>(raiz);
    }

    private static Sprite ObtenerIconoGuanteleteExistente()
    {
        // Reusa el sprite de un guantelete que ya existe en el proyecto (quedo
        // huerfano bajo la carpeta de Explorador). Es un placeholder razonable
        // hasta que se genere/asigne un icono propio para esta linea.
        const string prefabConIcono = "Assets/Scripts/Equipo/Armas/ArcoDeExplorador/GuanteletedeEstrella.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabConIcono);
        Item item = prefab != null ? prefab.GetComponent<Item>() : null;
        if (item == null || item.imItem == null)
        {
            Debug.LogWarning("[ItemsFaltantes] No se encontro un sprite de guantelete existente en '" + prefabConIcono + "'. Los items de Guantelete de Canalizador quedaran sin icono propio (imItem vacio) hasta asignar uno a mano.");
            return null;
        }

        return item.imItem;
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        string[] parts = folderPath.Split('/');
        if (parts.Length == 0)
        {
            return;
        }

        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }
            current = next;
        }
    }
}
