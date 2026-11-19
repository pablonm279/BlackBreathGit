using UnityEngine;
using UnityEngine.UI;

// Geometria UI: bisel corto, bronce mate y sombra; sin texturas ni bordes estirados.
public sealed class FloatingTextObsidianPanel : MaskableGraphic
{
    private float fontSize = 3f;

    public void SetFontSize(float value)
    {
        value = Mathf.Max(0.01f, value);
        if (Mathf.Approximately(fontSize, value)) { return; }
        fontSize = value;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect r = rectTransform.rect;
        float cut = fontSize * 0.12f;
        for (int i = 3; i >= 1; i--)
        {
            float spread = fontSize * 0.045f * i;
            Rect shadow = Rect.MinMaxRect(r.xMin - spread, r.yMin - spread,
                r.xMax + spread, r.yMax + spread);
            Color shade = new Color(0f, 0f, 0f, 0.09f * color.a);
            AddPolygon(vh, shadow, cut + spread, shade, shade, spread);
        }

        float edge = fontSize * 0.025f;
        AddPolygon(vh, r, cut, new Color(0.49f, 0.40f, 0.25f, 0.9f * color.a),
            new Color(0.26f, 0.22f, 0.15f, 0.9f * color.a), edge);
        Rect inner = Rect.MinMaxRect(r.xMin + edge, r.yMin + edge, r.xMax - edge, r.yMax - edge);
        AddPolygon(vh, inner, Mathf.Max(0f, cut - edge),
            new Color(0.075f, 0.069f, 0.06f, 0.8f * color.a),
            new Color(0.025f, 0.023f, 0.022f, 0.8f * color.a));
    }

    private static void AddPolygon(VertexHelper vh, Rect r, float cut, Color top, Color bottom, float inset = 0f)
    {
        cut = Mathf.Min(cut, Mathf.Min(r.width, r.height) * 0.5f);
        int start = vh.currentVertCount;
        AddVertex(vh, r, r.center, top, bottom);
        AddVertex(vh, r, new Vector2(r.xMin + cut, r.yMin), top, bottom);
        AddVertex(vh, r, new Vector2(r.xMin, r.yMin + cut), top, bottom);
        AddVertex(vh, r, new Vector2(r.xMin, r.yMax - cut), top, bottom);
        AddVertex(vh, r, new Vector2(r.xMin + cut, r.yMax), top, bottom);
        AddVertex(vh, r, new Vector2(r.xMax - cut, r.yMax), top, bottom);
        AddVertex(vh, r, new Vector2(r.xMax, r.yMax - cut), top, bottom);
        AddVertex(vh, r, new Vector2(r.xMax, r.yMin + cut), top, bottom);
        AddVertex(vh, r, new Vector2(r.xMax - cut, r.yMin), top, bottom);
        // Marco y sombra huecos: no acumular opacidad debajo del relleno traslucido.
        if (inset > 0f)
        {
            for (int i = 0; i < 8; i++)
            {
                UIVertex vertex = new UIVertex();
                vh.PopulateUIVertex(ref vertex, start + i + 1);
                Vector2 point = new Vector2(Mathf.Clamp(vertex.position.x, r.xMin + inset, r.xMax - inset),
                    Mathf.Clamp(vertex.position.y, r.yMin + inset, r.yMax - inset));
                AddVertex(vh, r, point, top, bottom);
            }
        }
        for (int i = 0; i < 8; i++)
        {
            int next = (i + 1) % 8;
            if (inset > 0f)
            {
                vh.AddTriangle(start + i + 1, start + next + 1, start + i + 9);
                vh.AddTriangle(start + next + 1, start + next + 9, start + i + 9);
            }
            else
            {
                vh.AddTriangle(start, start + i + 1, start + next + 1);
            }
        }
    }

    private static void AddVertex(VertexHelper vh, Rect r, Vector2 position, Color top, Color bottom)
    {
        vh.AddVert(position, Color.Lerp(bottom, top, Mathf.InverseLerp(r.yMin, r.yMax, position.y)), Vector2.zero);
    }
}
