#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class CrearRufianEspadachin
{
    private const string Carpeta = "Assets/Prefabs/Prefabs NPC/BANDIDOS/Rufián Espadachín";
    private const string BasePrefab = "Assets/Prefabs/Prefabs NPC/BANDIDOS/Rufián con Mazo/RufianConMazo.prefab";
    private const string PrefabDestino = Carpeta + "/RufianEspadachin.prefab";

    [InitializeOnLoadMethod]
    private static void CrearAutomaticamenteSiFalta()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDestino) != null)
        {
            return;
        }

        EditorApplication.delayCall += () =>
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDestino) == null && !EditorApplication.isCompiling)
            {
                Crear();
            }
        };
    }

    [InitializeOnLoadMethod]
    private static void ConfigurarTexturaDagaAlImportar()
    {
        EditorApplication.delayCall += () => ConfigurarSpriteRecurso("Assets/Resources/VFX/daga_rufian.png");
    }

    [MenuItem("Tools/Unidades/Crear Rufián Espadachín")]
    public static void Crear()
    {
        ConfigurarSprite("rufian_espadachin_idle.png");
        ConfigurarSprite("rufian_espadachin_activo.png");
        ConfigurarSprite("rufian_espadachin_movimiento.png");
        ConfigurarSprite("rufian_espadachin_ataque.png");
        ConfigurarSprite("rufian_espadachin_habilidad.png");

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BasePrefab);
        if (basePrefab == null)
        {
            throw new FileNotFoundException("No se encontró el prefab base.", BasePrefab);
        }

        GameObject instancia = PrefabUtility.InstantiatePrefab(basePrefab) as GameObject;
        if (instancia == null)
        {
            throw new UnityException("No se pudo instanciar el prefab base del rufián.");
        }

        try
        {
            instancia.name = "RufianEspadachin";

            ReemplazarComponenteUnidad(instancia);
            LimpiarHabilidadesPropiasDelMazo(instancia);

            EnemigoUnidadRufianEspadachin unidad = instancia.GetComponent<EnemigoUnidadRufianEspadachin>();
            IAUnidad ia = instancia.GetComponent<IAUnidad>();
            unidad.uNombre = "Rufián Espadachín";
            unidad.tags = new System.Collections.Generic.List<string> { "Humanoide" };

            ConfigurarAtributos(unidad);
            ia.esRango = false;
            ia.tierEnemigo = 3;
            ia.costoMovimientoAP = 1;

            instancia.AddComponent<IAEspadaLargaRufian>();
            instancia.AddComponent<IALanzamientoDagaRufian>();
            instancia.AddComponent<ReaccionOportunistaRufian>();

            Sprite idle = CargarSprite("rufian_espadachin_idle.png");
            Sprite activo = CargarSprite("rufian_espadachin_activo.png");
            Sprite movimiento = CargarSprite("rufian_espadachin_movimiento.png");
            Sprite ataque = CargarSprite("rufian_espadachin_ataque.png");
            Sprite habilidad = CargarSprite("rufian_espadachin_habilidad.png");

            unidad.uRetrato = activo;
            if (unidad.uImage != null)
            {
                unidad.uImage.sprite = idle;
                unidad.uImage.preserveAspect = true;
            }

            UnidadPoseController pose = instancia.GetComponent<UnidadPoseController>();
            if (pose == null) pose = instancia.AddComponent<UnidadPoseController>();
            pose.targetImage = unidad.uImage;
            pose.poseIdle = idle;
            pose.poseTurnoActivo = activo;
            pose.poseMover = movimiento;
            pose.poseAtacar = ataque;
            pose.poseHabilidad = habilidad;
            pose.duracionPoseAtacar = 1f;
            pose.duracionPoseHabilidad = 1.1f;

            PrefabUtility.SaveAsPrefabAsset(instancia, PrefabDestino);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(instancia);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Rufián Espadachín creado en " + PrefabDestino);
    }

    private static void ReemplazarComponenteUnidad(GameObject instancia)
    {
        EnemigoUnidadRufianMazo anterior = instancia.GetComponent<EnemigoUnidadRufianMazo>();
        if (anterior == null)
        {
            throw new MissingComponentException("El prefab base no contiene EnemigoUnidadRufianMazo.");
        }

        EnemigoUnidadRufianEspadachin destino = instancia.AddComponent<EnemigoUnidadRufianEspadachin>();
        EditorUtility.CopySerializedManagedFieldsOnly(anterior, destino);
        UnityEngine.Object.DestroyImmediate(anterior, true);
    }

    private static void LimpiarHabilidadesPropiasDelMazo(GameObject instancia)
    {
        IAGolpeMazoRufian golpeMazo = instancia.GetComponent<IAGolpeMazoRufian>();
        if (golpeMazo != null) UnityEngine.Object.DestroyImmediate(golpeMazo, true);

        foreach (Reaccion reaccion in instancia.GetComponents<Reaccion>())
        {
            UnityEngine.Object.DestroyImmediate(reaccion, true);
        }
    }

    private static void ConfigurarAtributos(EnemigoUnidadRufianEspadachin unidad)
    {
        SerializedObject datos = new SerializedObject(unidad);
        AsignarEntero(datos, "at_iniciativa", 3);
        AsignarEntero(datos, "mod_iniciativa", 3);
        AsignarFlotante(datos, "at_maxHP", 30f);
        AsignarFlotante(datos, "mod_maxHP", 30f);
        AsignarFlotante(datos, "HP_actual", 30f);
        AsignarEntero(datos, "at_maxAccionP", 3);
        AsignarEntero(datos, "mod_maxAccionP", 3);
        AsignarEntero(datos, "AccionP_actual", 3);
        AsignarEntero(datos, "at_CarFuerza", 5);
        AsignarEntero(datos, "mod_CarFuerza", 5);
        AsignarEntero(datos, "at_CarAgilidad", 6);
        AsignarEntero(datos, "mod_CarAgilidad", 6);
        AsignarEntero(datos, "at_CarPoder", 1);
        AsignarEntero(datos, "mod_CarPoder", 1);
        AsignarEntero(datos, "at_Armadura", 0);
        AsignarEntero(datos, "mod_Armadura", 0);
        AsignarEntero(datos, "at_Defensa", 15);
        AsignarEntero(datos, "mod_Defensa", 15);
        AsignarEntero(datos, "at_Ataque", 4);
        AsignarEntero(datos, "mod_Ataque", 4);
        AsignarEntero(datos, "at_TSReflejos", 6);
        AsignarEntero(datos, "mod_TSReflejos", 6);
        AsignarEntero(datos, "at_TSFortaleza", 3);
        AsignarEntero(datos, "mod_TSFortaleza", 3);
        AsignarEntero(datos, "at_TSMental", 2);
        AsignarEntero(datos, "mod_TSMental", 2);
        AsignarEntero(datos, "estado_evasion", 1);
        datos.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AsignarEntero(SerializedObject objeto, string nombre, int valor)
    {
        SerializedProperty propiedad = objeto.FindProperty(nombre);
        if (propiedad == null)
        {
            throw new MissingFieldException(typeof(Unidad).Name, nombre);
        }
        propiedad.intValue = valor;
    }

    private static void AsignarFlotante(SerializedObject objeto, string nombre, float valor)
    {
        SerializedProperty propiedad = objeto.FindProperty(nombre);
        if (propiedad == null)
        {
            throw new MissingFieldException(typeof(Unidad).Name, nombre);
        }
        propiedad.floatValue = valor;
    }

    private static void ConfigurarSprite(string archivo)
    {
        string ruta = Carpeta + "/" + archivo;
        TextureImporter importer = AssetImporter.GetAtPath(ruta) as TextureImporter;
        if (importer == null) return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.CompressedHQ;
        importer.SaveAndReimport();
    }

    private static Sprite CargarSprite(string archivo)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(Carpeta + "/" + archivo);
    }

    private static void ConfigurarSpriteRecurso(string ruta)
    {
        TextureImporter importer = AssetImporter.GetAtPath(ruta) as TextureImporter;
        if (importer == null || (importer.textureType == TextureImporterType.Sprite && importer.spriteImportMode == SpriteImportMode.Single))
        {
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.CompressedHQ;
        importer.SaveAndReimport();
    }
}
#endif
