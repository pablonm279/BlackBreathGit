using UnityEngine;

[CreateAssetMenu(fileName = "ItemIconGeneratorConfig", menuName = "Items/Item Icon Generator Config")]
public class ItemIconGeneratorConfig : ScriptableObject
{
    [Header("Carpeta de salida (debe comenzar con Assets/)")]
    public string carpetaSalida = "Assets/Generated/ItemIcons";

    [Header("Composicion")]
    [Range(0.1f, 1.2f)] public float escalaOverlay = 0.72f;
    [Range(0.5f, 2.0f)] public float multiplicadorOverlayX = 1f;
    [Range(-1f, 1f)] public float offsetOverlayXNormalizado = 0f;
    [Range(-1f, 1f)] public float offsetOverlayYNormalizado = 0f;

    [Header("Fondos por Rareza")]
    public Sprite fondoComun;
    public Sprite fondoInfrecuente;
    public Sprite fondoRaro;
    public Sprite fondoEpico;
    public Sprite fondoLegendario;
    public Sprite fondoArtefacto;
    public Sprite fondoFallback;

    [Header("Overlays Armas")]
    public Sprite iconoArmaMandoble;
    public Sprite iconoArmaGuantelete;
    public Sprite iconoArmaBaculo;
    public Sprite iconoArmaArco;
    public Sprite iconoArmaEspadaCorta;

    [Header("Overlays Armaduras (por clase permitida)")]
    public Sprite iconoArmaduraCaballero;
    public Sprite iconoArmaduraExplorador;
    public Sprite iconoArmaduraPurificadora;
    public Sprite iconoArmaduraAcechador;
    public Sprite iconoArmaduraCanalizador;

    [Header("Overlays Otros")]
    public Sprite iconoAccesorioAnillo;
    public Sprite iconoConsumible;
    public Sprite iconoFallback;
}
