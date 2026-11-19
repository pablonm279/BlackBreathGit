using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class AdornosPasoVientoHelado
{
    readonly List<Vector4> vegetacionExistente = new List<Vector4>();
    readonly List<Vector4> obstaculosTroncos = new List<Vector4>();
    static readonly Vector3[] BordesDetalle = { Vector3.right, Vector3.left, Vector3.forward, Vector3.back };

    IEnumerator GenerarDetallesNaturales(int calidad)
    {
        if (materialHierba == null) yield break;
        vegetacionExistente.Clear();
        obstaculosTroncos.Clear();
        // Respeta el apoyo de árboles, rocas y aldeas ya colocados por el decorador.
        foreach (Transform hijo in decorador.transform)
        {
            if (!hijo.gameObject.activeSelf || !hijo.name.EndsWith("(Clone)")) continue;
            var renderer = hijo.GetComponentInChildren<MeshRenderer>();
            float radio = renderer != null ? Mathf.Clamp(Mathf.Min(renderer.bounds.size.x, renderer.bounds.size.z) * .25f, .18f, paso * .4f) : .25f;
            vegetacionExistente.Add(new Vector4(hijo.position.x, 0, hijo.position.z, radio));
            // La hierba menuda y los decals pueden quedar debajo de un tronco; árboles, rocas y aldeas no.
            if (renderer != null && (renderer.bounds.size.y > .75f || radio > paso * .2f))
                obstaculosTroncos.Add(new Vector4(hijo.position.x, 0, hijo.position.z, radio));
        }

        // Un puñado de candidatos por manchón favorece los huecos sin crear una grilla visible.
        int cantidad = Mathf.Clamp(Mathf.RoundToInt(terreno.size.x * terreno.size.z / (paso * paso * 1.5f)), 32, 90);
        if (calidad == 0) cantidad = Mathf.RoundToInt(cantidad * .6f);
        else if (calidad == 1) cantidad = Mathf.RoundToInt(cantidad * .8f);
        for (int i = 0; i < cantidad; i++)
        {
            float radio = paso * Rango(.13f, .23f), mejor = -1f;
            Vector3 centro = Vector3.zero;
            for (int intento = 0; intento < 16; intento++)
            {
                var candidato = new Vector3(Rango(terreno.min.x + radio, terreno.max.x - radio), terreno.center.y,
                    Rango(terreno.min.z + radio, terreno.max.z - radio));
                if (!LugarNatural(candidato, radio, paso * .22f, out candidato)) continue;
                float espacio = paso * 2;
                foreach (var adorno in adornos) espacio = Mathf.Min(espacio, DistanciaXZ(candidato, adorno.centro) - adorno.radio);
                foreach (var ocupado in vegetacionExistente) espacio = Mathf.Min(espacio, DistanciaXZ(candidato, ocupado) - ocupado.w);
                if (espacio > mejor) { mejor = espacio; centro = candidato; }
            }
            if (mejor >= 0) CrearDetalleNatural(centro, radio, i % 4);
            if (i % 4 == 3) yield return null;
        }

        // Detalles laterales en el terreno de los nodos, nunca un aro ni una plataforma.
        // Misma mezcla que en los huecos para que no señalen el tipo ni el estado de un nodo oculto.
        var ordenados = new List<Vector3>(nodos);
        ordenados.Sort((a, b) => a.x == b.x ? a.z.CompareTo(b.z) : a.x.CompareTo(b.x));
        foreach (var nodo in ordenados)
        {
            int colocados = 0, objetivo = calidad == 0 ? 1 : 2;
            for (int intento = 0; intento < 18 && colocados < objetivo; intento++)
            {
                float angulo = Rango(0, Mathf.PI * 2), distancia = paso * Rango(.25f, .46f);
                float radio = paso * Rango(.075f, .12f);
                var centro = nodo + new Vector3(Mathf.Cos(angulo), 0, Mathf.Sin(angulo)) * distancia;
                if (!LugarNatural(centro, radio, paso * .15f, out centro)) continue;
                CrearDetalleNatural(centro, radio, rng.Next(0, 4));
                colocados++;
            }
            yield return null;
        }
        vegetacionExistente.Clear();
        obstaculosTroncos.Clear();
    }

    IEnumerator GenerarTroncosNevados()
    {
        var prefab = Resources.Load<GameObject>("ObjetosMapa/PasoVientoHelado_TroncoCaidoNevado");
        if (prefab == null) yield break;
        int colocados = 0;
        for (int intento = 0; intento < 1200 && colocados < 3; intento++)
        {
            float escala = (intento < 400 ? Rango(.9f, 1.12f) : Rango(.82f, 1f)) * .7f;
            float radio = 3f * escala;
            var giro = Quaternion.Euler(0, Rango(-70, 70), 0);
            var candidato = new Vector3(Rango(terreno.min.x + radio, terreno.max.x - radio), terreno.center.y,
                Rango(terreno.min.z + radio, terreno.max.z - radio));
            if (LugarTronco(candidato, escala, giro, out var centro, out var normal))
            {
                var objeto = Instantiate(prefab, raiz.transform);
                objeto.name = "Tronco ancestral caído nevado";
                objeto.transform.position = centro + normal * .025f;
                objeto.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal) * giro;
                objeto.transform.localScale = Vector3.one * escala;
                // Huella alargada para que revelar un camino cercano no retire un tronco que está a su lado.
                adornos.Add(new Adorno { objeto = objeto, centro = centro, radio = radio,
                    eje = giro * Vector3.right, semilargo = 1.9f * escala, semiancho = 1.05f * escala });
                colocados++;
            }
            if (intento % 8 == 7) yield return null;
        }
    }

    bool LugarTronco(Vector3 punto, float escala, Quaternion giro, out Vector3 centro, out Vector3 normal)
    {
        if (!decorador.TrySampleSurface(punto, out centro, out normal) || normal.y < .97f) return false;
        // Muestrea toda la huella, incluidas raíces y ramas, en vez de exigir un claro circular enorme.
        for (int x = 0; x < 6; x++)
        for (int z = -1; z <= 1; z += 2)
        {
            var offset = giro * new Vector3((x * .9f - 2.25f) * escala, 0, z * .55f * escala);
            if (!LugarNatural(centro + offset, .55f * escala, paso * .16f, out var suelo, true)) return false;
            float alturaPlano = centro.y - (normal.x * offset.x + normal.z * offset.z) / normal.y;
            if (Mathf.Abs(suelo.y - alturaPlano) > .20f) return false;
        }
        return true;
    }

    bool LugarNatural(Vector3 punto, float radio, float margenNodo, out Vector3 centro, bool paraTronco = false)
    {
        centro = punto;
        if (punto.x - radio < terreno.min.x || punto.x + radio > terreno.max.x
            || punto.z - radio < terreno.min.z || punto.z + radio > terreno.max.z
            || !decorador.PasaMargenCornisaFrontal(punto, radio)) return false;
        if (!decorador.TrySampleSurface(punto, out centro, out var normal) || normal.y < .86f
            || centro.y < zona.TexturaTerreno.transform.position.y - .65f) return false;
        foreach (var nodo in nodos)
            if (DistanciaXZ(nodo, centro) < radio + margenNodo) return false;
        foreach (var tramo in caminos)
            if (DistanciaSegmentoXZ(centro, tramo.a, tramo.b) < radio + tramo.margen) return false;
        foreach (var ocupado in paraTronco ? obstaculosTroncos : vegetacionExistente)
            if (DistanciaXZ(centro, ocupado) < radio + ocupado.w) return false;
        foreach (var adorno in adornos)
            if (DistanciaXZ(centro, adorno.centro) < radio + adorno.radio + paso * .04f) return false;
        foreach (var direccion in BordesDetalle)
            if (!decorador.TrySampleSurface(centro + direccion * radio, out var borde, out var normalBorde)
                || normalBorde.y < .8f || Mathf.Abs(borde.y - centro.y) > .32f) return false;
        return true;
    }

    void CrearDetalleNatural(Vector3 centro, float radio, int tipo)
    {
        var vertices = new List<Vector3>(); var colores = new List<Color>(); var indices = new List<int>();
        var giro = Quaternion.Euler(0, Rango(0, 360), 0);
        // Una base discontinua de lajas pequeñas integra las matas y la madera con la nieve.
        for (int i = 0; i < (tipo == 2 ? 15 : 7); i++)
        {
            var offset = giro * new Vector3(Rango(-.65f, .65f) * radio, 0, Rango(-.35f, .35f) * radio);
            offset.y = Superficie(centro + offset).y - centro.y;
            float tam = radio * Rango(.065f, tipo == 2 ? .23f : .14f);
            int inicio = colores.Count;
            AgregarPiedra(vertices, colores, indices, offset, tam, tam * Rango(.18f, .4f), false, false);
            for (int j = inicio; j < colores.Count; j++)
            {
                Color color = colores[j] * .62f; color.a = 1;
                colores[j] = color;
            }
            // Líquenes oliva y gris, apagados: sin manchas blancas ni emisión.
            if (i % 3 == 0)
                for (int j = inicio + 6; j < colores.Count; j++) colores[j] = Color.Lerp(colores[j], new Color(.23f, .26f, .16f), .65f);
        }
        if (tipo == 0 || tipo == 3)
        {
            // Sauce enano / enebro rastrero: silueta ancha y baja, ramas desiguales.
            for (int i = 0; i < (tipo == 0 ? 9 : 6); i++)
            {
                float angulo = Rango(0, Mathf.PI * 2), largo = radio * Rango(.35f, .78f);
                var direccion = new Vector3(Mathf.Cos(angulo), 0, Mathf.Sin(angulo));
                var baseRama = Superficie(centro + direccion * radio * .07f) - centro;
                var medio = Superficie(centro + direccion * largo * .48f) - centro + Vector3.up * radio * Rango(.15f, .30f);
                var punta = Superficie(centro + direccion * largo) - centro + Vector3.up * radio * Rango(.17f, .36f);
                var corteza = new Color(.22f, .21f, .18f);
                AgregarRama(vertices, colores, indices, baseRama, medio, radio * .025f, corteza);
                AgregarRama(vertices, colores, indices, medio, punta, radio * .016f, corteza);
                for (int j = 0; j < 3; j++)
                {
                    float t = .3f + j * .23f;
                    var nacimiento = Vector3.Lerp(medio, punta, t);
                    var lateral = Vector3.Cross(direccion, Vector3.up) * (j % 2 == 0 ? 1 : -1);
                    var extremo = nacimiento + lateral * radio * Rango(.12f, .25f) + Vector3.up * radio * .10f;
                    AgregarRama(vertices, colores, indices, nacimiento, extremo, radio * .009f, corteza);
                    if (tipo == 0) AgregarFollaje(vertices, colores, indices, extremo, direccion, radio * Rango(.08f, .13f));
                }
                if (tipo == 0) AgregarFollaje(vertices, colores, indices, punta, direccion, radio * .11f);
            }
        }
        else if (tipo == 1)
        {
            // Rama caída semienterrada, con bifurcaciones y madera expuesta en el extremo.
            var a = Superficie(centro + giro * Vector3.left * radio * .73f) - centro + Vector3.up * radio * .04f;
            var b = Superficie(centro + giro * Vector3.right * radio * .70f) - centro + Vector3.up * radio * .08f;
            AgregarRama(vertices, colores, indices, a, b, radio * .065f, new Color(.23f, .20f, .17f));
            for (int i = 0; i < 4; i++)
            {
                var inicio = Vector3.Lerp(a, b, .22f + i * .16f);
                var fin = inicio + giro * new Vector3(radio * .16f, radio * .04f, radio * Rango(.22f, .35f) * (i % 2 == 0 ? 1 : -1));
                fin.y = Superficie(centro + fin).y - centro.y + radio * .045f;
                AgregarRama(vertices, colores, indices, inicio, fin, radio * .024f, new Color(.27f, .24f, .20f));
            }
        }
        string nombre = tipo == 0 ? "Enebro rastrero escarchado" : tipo == 1 ? "Rama caída semienterrada"
            : tipo == 2 ? "Lajas de pizarra con líquenes" : "Sauce enano seco";
        var objeto = CrearObjeto(nombre, centro, radio, vertices, colores, indices);
        objeto.GetComponent<MeshRenderer>().sharedMaterial = materialHierba;
    }

    void AgregarRama(List<Vector3> vertices, List<Color> colores, List<int> indices, Vector3 a, Vector3 b, float grosor, Color color)
    {
        int inicio = vertices.Count;
        var eje = (b - a).normalized;
        var lateral = Vector3.Cross(eje, Vector3.up).normalized;
        if (lateral.sqrMagnitude < .01f) lateral = Vector3.right;
        var arriba = Vector3.Cross(lateral, eje).normalized;
        for (int extremo = 0; extremo < 2; extremo++)
        for (int i = 0; i < 5; i++)
        {
            float angulo = i * Mathf.PI * 2 / 5;
            var normal = lateral * Mathf.Cos(angulo) + arriba * Mathf.Sin(angulo);
            vertices.Add((extremo == 0 ? a : b) + normal * grosor * (extremo == 0 ? 1 : .45f));
            colores.Add(Color.Lerp(color * .78f, new Color(.46f, .49f, .48f), Mathf.Max(0, normal.y) * .55f));
        }
        for (int i = 0; i < 5; i++)
        {
            int j = (i + 1) % 5;
            Triangulo(indices, inicio + i, inicio + j, inicio + i + 5);
            Triangulo(indices, inicio + j, inicio + j + 5, inicio + i + 5);
        }
        Triangulo(indices, inicio + 5, inicio + 7, inicio + 6);
        Triangulo(indices, inicio + 5, inicio + 8, inicio + 7);
        Triangulo(indices, inicio + 5, inicio + 9, inicio + 8);
    }

    void AgregarFollaje(List<Vector3> vertices, List<Color> colores, List<int> indices, Vector3 centro, Vector3 direccion, float tam)
    {
        var lado = Vector3.Cross(direccion, Vector3.up);
        // Racimos de acículas estrechas: evitan hojas grandes o paneles rectangulares al acercar la cámara.
        for (int i = 0; i < 7; i++)
        {
            int inicio = vertices.Count;
            float t = i / 6f, signo = i % 2 == 0 ? 1 : -1;
            var baseHoja = centro + direccion * tam * (t - .6f);
            var punta = baseHoja + (direccion * .6f + lado * signo * .65f + Vector3.up * .32f) * tam;
            var ancho = lado * tam * .09f;
            vertices.Add(baseHoja - ancho); vertices.Add(baseHoja + ancho);
            vertices.Add((baseHoja + punta) * .5f + Vector3.up * tam * .12f); vertices.Add(punta);
            var oscuro = new Color(.14f, .21f, .18f); var claro = new Color(.30f, .37f, .33f);
            colores.Add(oscuro); colores.Add(oscuro); colores.Add(claro); colores.Add(claro);
            Triangulo(indices, inicio, inicio + 2, inicio + 1);
            Triangulo(indices, inicio, inicio + 3, inicio + 2); Triangulo(indices, inicio + 1, inicio + 2, inicio + 3);
        }
    }
}
