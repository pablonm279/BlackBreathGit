using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Fondo cubierto por humo de incendio, sin terreno ni colisiones adicionales.
[DisallowMultipleComponent]
public sealed class FondoBosqueArdiente : MonoBehaviour
{
    readonly List<Mesh> mallas = new List<Mesh>();
    readonly List<Material> materiales = new List<Material>();
    ParticleSystemRenderer humoProvisional;
    bool humoProvisionalVisible;

    public static FondoBosqueArdiente Preparar(AtributosZona zona, Transform padre)
    {
        if (zona == null || zona.ID != 1 || zona.TexturaTerreno == null) return null;
        var humo = Resources.Load<Shader>("BosqueArdienteFondo");
        if (humo == null) return null;
        var fondo = padre.GetComponentInChildren<FondoBosqueArdiente>(true);
        if (fondo == null)
        {
            var go = new GameObject("Muro de humo e incendio del bosque (Runtime)") { layer = zona.TexturaTerreno.gameObject.layer };
            go.transform.SetParent(padre, true);
            fondo = go.AddComponent<FondoBosqueArdiente>();
        }
        fondo.Construir(zona, humo);
        return fondo;
    }

    void Construir(AtributosZona zona, Shader shaderHumo)
    {
        Limpiar();
        if (humoProvisional == null)
        {
            foreach (var renderer in zona.bosqueardienteContenedorGameObjects.GetComponentsInChildren<ParticleSystemRenderer>(true))
                if (renderer.name == "NieblaFondo")
                {
                    humoProvisional = renderer; humoProvisionalVisible = renderer.enabled;
                    break;
                }
        }
        if (humoProvisional != null) humoProvisional.enabled = false;
        var bounds = zona.TexturaTerreno.bounds;
        transform.position = new Vector3(bounds.center.x, bounds.min.y, bounds.max.z + 1.5f);
        float ancho = Mathf.Max(bounds.size.x * 1.7f, 90);
        for (int capa = 0; capa < 3; capa++)
        {
            var material = new Material(shaderHumo) { name = "Pared de humo de incendio " + capa, hideFlags = HideFlags.DontSave };
            material.SetFloat("_Capa", capa);
            material.SetColor("_Color", new Color(.037f + capa * .010f, .033f + capa * .008f, .030f + capa * .006f, .96f));
            materiales.Add(material);
            var mesh = new Mesh { name = "Cortina de humo " + capa, hideFlags = HideFlags.DontSave };
            float z = 3 + capa * 7, pie = -6 - capa * 2, alto = 70 + capa * 8;
            mesh.vertices = new[] { new Vector3(-ancho, pie, z), new Vector3(ancho, pie, z),
                new Vector3(-ancho, pie + alto, z), new Vector3(ancho, pie + alto, z) };
            mesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.up, Vector2.one };
            mesh.triangles = new[] { 0, 2, 1, 1, 2, 3 }; mesh.RecalculateBounds();
            mallas.Add(mesh);
            var go = new GameObject("Humo denso del frente " + capa) { layer = gameObject.layer };
            go.transform.SetParent(transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>(); renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
        }
    }

    public static Gradient GradienteFuego()
    {
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
            new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(.75f, .12f), new GradientAlphaKey(.48f, .58f), new GradientAlphaKey(0, 1) });
        return g;
    }

    public void Actualizar(float reloj, float dia)
    {
        foreach (var material in materiales)
        {
            material.SetFloat("_Reloj", reloj);
            if (material.HasProperty("_Dia")) material.SetFloat("_Dia", dia);
        }
    }
    void Limpiar()
    {
        var recursos = new HashSet<Object>();
        foreach (var mesh in mallas) recursos.Add(mesh);
        foreach (var material in materiales) recursos.Add(material);
        // También recoge los recursos de la versión anterior después de una recarga de scripts.
        var hijos = new List<Transform>();
        foreach (Transform hijo in transform) hijos.Add(hijo);
        foreach (var hijo in hijos)
        {
            var filtro = hijo.GetComponent<MeshFilter>();
            if (filtro != null && filtro.sharedMesh != null && (filtro.sharedMesh.hideFlags & HideFlags.DontSave) != 0) recursos.Add(filtro.sharedMesh);
            var renderer = hijo.GetComponent<Renderer>();
            if (renderer != null && renderer.sharedMaterial != null && (renderer.sharedMaterial.hideFlags & HideFlags.DontSave) != 0) recursos.Add(renderer.sharedMaterial);
            hijo.gameObject.SetActive(false); Liberar(hijo.gameObject);
        }
        foreach (var recurso in recursos) Liberar(recurso);
        mallas.Clear(); materiales.Clear();
    }
    void OnDestroy()
    {
        if (humoProvisional != null) humoProvisional.enabled = humoProvisionalVisible;
        Limpiar();
    }
    static void Liberar(Object o) { if (o == null) return; if (Application.isPlaying) Destroy(o); else DestroyImmediate(o); }
}
