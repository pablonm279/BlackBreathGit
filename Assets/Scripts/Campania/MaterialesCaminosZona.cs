using UnityEngine;

[DisallowMultipleComponent]
public sealed class MaterialesCaminosZona : MonoBehaviour
{
    [Header("Caminos normales")]
    public Material caminoOriginal;
    public Material caminoMarcado;
    public Material caminoUsado;
    public Material caminoHint;

    [Header("Tipos especiales")]
    public Material atajoSubterraneo;
    public Material atajoSuperficie;
    public Material caminoLento;
    public Material caminoAAldea;

    [Header("Ancho")]
    public float multiplicadorAnchoBase = 1f;
}
