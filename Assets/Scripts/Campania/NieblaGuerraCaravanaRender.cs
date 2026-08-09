using UnityEngine;

/// <summary>
/// Paso de cámara que aplica la máscara mundial de niebla después de renderizar
/// el mapa. La UI Overlay se dibuja luego y no queda contaminada.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public sealed class NieblaGuerraCaravanaRender : MonoBehaviour
{
    static readonly int IdMatrizVistaProyeccionInversa =
        Shader.PropertyToID("_InverseViewProjection");

    NieblaGuerraCaravana controlador;
    Material material;
    Camera camara;

    public void Configurar(NieblaGuerraCaravana nuevoControlador, Material nuevoMaterial)
    {
        controlador = nuevoControlador;
        material = nuevoMaterial;
        if (camara == null)
        {
            camara = GetComponent<Camera>();
        }

        if (camara != null)
        {
            camara.depthTextureMode |= DepthTextureMode.Depth;
        }
    }

    void OnRenderImage(RenderTexture origen, RenderTexture destino)
    {
        if (controlador == null || material == null
            || !controlador.PuedeRenderizarNiebla || camara == null)
        {
            Graphics.Blit(origen, destino);
            return;
        }

        Matrix4x4 proyeccionGpu = GL.GetGPUProjectionMatrix(
            camara.projectionMatrix,
            true);
        Matrix4x4 vistaProyeccion = proyeccionGpu * camara.worldToCameraMatrix;
        material.SetMatrix(IdMatrizVistaProyeccionInversa, vistaProyeccion.inverse);
        Graphics.Blit(origen, destino, material, 0);
    }
}
