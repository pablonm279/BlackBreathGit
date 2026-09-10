using UnityEngine;
public sealed class AtributosZona : MonoBehaviour
{
    public int ID = 1;
    public GameObject bosqueardienteContenedorGameObjects;
    public GameObject BosqueAngustiante_Llama;
    public MeshRenderer TexturaTerreno, TexturaTerrenoExtension;
}
public sealed class CaminoMesh : MonoBehaviour
{
    public bool VisibleParaDecoracion => true;
    public float GetWidth() => .4f;
}
