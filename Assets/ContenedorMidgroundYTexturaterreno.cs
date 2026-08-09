using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ContenedorMidgroundYTexturaterreno : MonoBehaviour
{
    public int midgrounddeseado;
    public Material SueloTexturaParaMidground1;
    public Material SueloTexturaParaMidground2;
    public Material SueloTexturaParaMidground3;

    public GameObject Terreno;
    public GameObject Midground1;
    public GameObject Midground2;
    public GameObject Midground3;


    public SpriteRenderer FondoMostrado;
    public Sprite Fondo1;
    public Sprite Fondo2;
    public Sprite Fondo3;

    private void Awake()
    {
        AplicarMidground();
    }

    private void OnValidate()
    {
        AplicarMidground();
    }

    [ContextMenu("Sortear nueva composición")]
    public void SortearNuevaComposicion()
    {
        midgrounddeseado = Random.Range(1, 4);
        AplicarMidground();
    }

    private void AplicarMidground()
    {
        int indiceSeleccionado = midgrounddeseado > 0 ? midgrounddeseado : Random.Range(1, 4);

        if (Midground1 != null)
            Midground1.SetActive(indiceSeleccionado == 1);

        if (Midground2 != null)
            Midground2.SetActive(indiceSeleccionado == 2);

        if (Midground3 != null)
            Midground3.SetActive(indiceSeleccionado == 3);

        AplicarFondo(indiceSeleccionado);

        Material materialSeleccionado = null;

        switch (indiceSeleccionado)
        {
            case 1:
                materialSeleccionado = SueloTexturaParaMidground1;
                break;
            case 2:
                materialSeleccionado = SueloTexturaParaMidground2;
                break;
            case 3:
                materialSeleccionado = SueloTexturaParaMidground3;
                break;
        }

        if (Terreno != null)
        {
            Renderer rendererTerreno = Terreno.GetComponent<Renderer>();
            if (rendererTerreno != null && materialSeleccionado != null)
            {
                rendererTerreno.material = materialSeleccionado;
            }
        }
    }

    private void AplicarFondo(int indiceSeleccionado)
    {
        if (FondoMostrado == null)
            return;

        Sprite spriteSeleccionado = null;

        switch (indiceSeleccionado)
        {
            case 1:
                spriteSeleccionado = Fondo1;
                break;
            case 2:
                spriteSeleccionado = Fondo2;
                break;
            case 3:
                spriteSeleccionado = Fondo3;
                break;
        }

        if (spriteSeleccionado != null)
            FondoMostrado.sprite = spriteSeleccionado;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ContenedorMidgroundYTexturaterreno))]
public class ContenedorMidgroundYTexturaterrenoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Sortear nueva composición"))
        {
            var script = (ContenedorMidgroundYTexturaterreno)target;
            script.SortearNuevaComposicion();
            EditorUtility.SetDirty(script);
        }
    }
}
#endif



