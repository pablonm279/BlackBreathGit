using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class BosqueRender
{
    static Color32[] ultimaCaptura;
    static string Estado(UnityEngine.Object componente)
    {
        string json = EditorJsonUtility.ToJson(componente);
        if (componente is ParticleSystemRenderer r && !r.name.Contains("Humo")
            && r.sharedMaterial != null && r.sharedMaterial.name.Contains("Llama"))
            json = System.Text.RegularExpressions.Regex.Replace(json,"\"m_Materials\"\\s*:\\s*\\[[^\\]]*\\]", "\"m_Materials\":[]");
        return json;
    }
    static void Campo(object obj, string nombre, object valor) => obj.GetType().GetField(nombre,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(obj,valor);
    static void Ejecutar(IEnumerator rutina)
    {
        try { while (rutina.MoveNext()) if (rutina.Current is IEnumerator hija) Ejecutar(hija); }
        finally { (rutina as IDisposable)?.Dispose(); }
    }
    static readonly string Salida = Path.GetFullPath("../../output/ambiente-bosque");
    static void Check(bool condicion, string texto, StringBuilder log)
    {
        if (!condicion) throw new Exception(texto);
        log.AppendLine("OK " + texto);
    }
    public static void Run()
    {
        Directory.CreateDirectory(Salida);
        var log = new StringBuilder();
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), "Assets/ES-CampañaPrueba.unity");
        foreach (var nombre in new[] { "BosqueArdienteSuelo", "BosqueArdienteAmbiente", "BosqueArdienteLlamas" })
        {
            var shader = Resources.Load<Shader>(nombre);
            Check(shader != null && shader.isSupported && !ShaderUtil.ShaderHasError(shader), "Shader " + nombre, log);
            foreach (var error in ShaderUtil.GetShaderMessages(shader)) log.AppendLine(error.message);
        }
        var go = new GameObject("Decorador generado");
        go.transform.localScale = new Vector3(2.068544f,1f,5.7770762f);
        go.transform.rotation = Quaternion.Euler(0,90,0);
        var zona = go.AddComponent<AtributosZona>();
        var decorador = go.AddComponent<MapDecorator>();
        zona.bosqueardienteContenedorGameObjects = new GameObject("Bosque inicial");
        var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.localScale = new Vector3(6f, 1, 6f);
        zona.TexturaTerreno = plane.GetComponent<MeshRenderer>();
        zona.TexturaTerreno.sharedMaterial = Resources.Load<Material>("ObjetosMapa/MatBosqueArdienteSuelo");
        var sueloOriginal = zona.TexturaTerreno.sharedMaterial;
        Campo(decorador,"planeMesh",plane.GetComponent<MeshFilter>());
        Campo(decorador,"autoBuscarSectoresTerreno",false);
        Campo(decorador,"usarRelieveProcedural",true);
        Campo(decorador,"alturaRelieveBosque",2.4f);
        Campo(decorador,"usarBordesOrganicosSectores",false);
        Campo(decorador,"convertirSombrasBlobCanvas",false);
        Campo(decorador,"margenCamino",.15f);
        Campo(decorador,"anchoCaminoMinWorld",.4f);
        var sur = GameObject.CreatePrimitive(PrimitiveType.Plane);
        sur.transform.position = new Vector3(0,0,-23);
        sur.transform.localScale = new Vector3(6,1,.8f);
        sur.GetComponent<MeshRenderer>().enabled = false;
        Campo(decorador,"sectoresTerreno",new MeshFilter[] {null,plane.GetComponent<MeshFilter>(),sur.GetComponent<MeshFilter>(),null,null});
        typeof(MapDecorator).GetMethod("Awake",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(decorador,null);
        decorador.RegenerarRelieveParaZona(1,1,93217);
        var caminos = new System.Collections.Generic.List<LineRenderer>();
        for (int i=0;i<2;i++)
        {
            var lr = new GameObject("Camino prueba " + i).AddComponent<LineRenderer>();
            lr.gameObject.tag = "Camino";
            lr.positionCount = 2; lr.useWorldSpace = true; lr.widthMultiplier = .4f;
            lr.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = lr.endColor = new Color(.12f,.09f,.06f,1f);
            lr.SetPositions(i==0 ? new[] {new Vector3(-25,0,0),new Vector3(25,0,0)}
                : new[] {new Vector3(10,0,-25),new Vector3(10,0,25)});
            lr.gameObject.SetActive(i==0); // Tambien se debe respetar la ruta todavia oculta.
            caminos.Add(lr);
        }
        var nodoPrueba = new GameObject("Nodo prueba") { tag = "Nodo" };
        nodoPrueba.transform.position = new Vector3(-10,0,10);
        var random = new System.Random(21);
        for (int i=0; i<140; i++)
        {
            float x = (float)random.NextDouble()*23-11.5f, z = (float)random.NextDouble()*23-11.5f;
            if (Mathf.Abs(x-z*.3f) < 1) continue;
            var arbol = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("ObjetosMapa/BosqueArdiente - Arbol1"), new Vector3(x,0,z), Quaternion.identity);
            foreach (var l in arbol.GetComponentsInChildren<Light>()) l.enabled = false;
        }
        var fires = new System.Collections.Generic.List<GameObject>();
        for (int i=0; i<5; i++)
        {
            var prefab = Resources.Load<GameObject>(i==0 ? "LlamaInicial" : i==4 ? "ObjetosMapa/LlamaEspectral" : "ObjetosMapa/Llama1GO");
            var fuego = UnityEngine.Object.Instantiate(prefab, new Vector3(i*2-4,0,i%2*3-2), Quaternion.identity,
                i%2==0 ? zona.bosqueardienteContenedorGameObjects.transform : go.transform);
            fires.Add(fuego);
            VisualPolishRuntime.ApplyGeneratedCampaignVfxQualityScale(fuego);
        }
        var sol = new GameObject("Sol del bosque").AddComponent<Light>();
        sol.type = LightType.Directional;
        sol.transform.rotation = Quaternion.Euler(144.179f,46.791f,-11.956f);
        sol.color = new Color(.8301887f,.78991514f,.5991456f);
        sol.intensity = 3.65f*1.665f;
        sol.shadows = LightShadows.Soft;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(.24f,.22f,.2f);
        RenderSettings.fog = false;
        var cam = new GameObject("Camara de prueba").AddComponent<Camera>();
        cam.transform.position = new Vector3(10,12,-17);
        cam.transform.LookAt(Vector3.zero);
        cam.fieldOfView = 46;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(.025f,.028f,.029f);
        cam.allowHDR = true;
        foreach (var f in fires)
        foreach (var ps in f.GetComponentsInChildren<ParticleSystem>()) ps.Simulate(6,false,true);
        Capturar(cam,"preview-antes");
        var rng = UnityEngine.Random.state;
        var nodos = fires.Select(f=>f.transform.position).ToArray();
        zona.BosqueAngustiante_Llama = Resources.Load<GameObject>("ObjetosMapa/Llama1GO");
        Ejecutar(decorador.GenerarAsyncCR(zona.BosqueAngustiante_Llama,14,.74f,.8f,5.8f,20,2));
        var originales = UnityEngine.Object.FindObjectsByType<ParticleSystem>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        var componentesOriginales = originales.SelectMany(p=>new UnityEngine.Object[] {p,p.transform,p.GetComponent<ParticleSystemRenderer>()})
            .Concat(UnityEngine.Object.FindObjectsByType<LODGroup>(FindObjectsSortMode.None)).ToArray();
        var estadoOriginal = componentesOriginales.ToDictionary(c=>c,c=>Estado(c));
        var materialesOriginales = originales.Select(p=>p.GetComponent<ParticleSystemRenderer>())
            .ToDictionary(r=>r,r=>r.sharedMaterial);
        var assetsOriginales = materialesOriginales.Values.Distinct().ToDictionary(m=>m,m=>EditorJsonUtility.ToJson(m));
        AmbienteBosqueArdiente.Preparar(zona,false);
        var ambiente = zona.bosqueardienteContenedorGameObjects.GetComponent<AmbienteBosqueArdiente>();
        Check(ambiente != null,"Ambiente creado",log);
        Check(UnityEngine.Random.state.Equals(rng),"RNG de juego intacto al preparar",log);
        Check(sueloOriginal.shader.name != zona.TexturaTerreno.sharedMaterial.shader.name,"Suelo mejorado conservado",log);
        Check(estadoOriginal.All(par=>Estado(par.Key)==par.Value),
            "Solo cambia el material: emisores, humo, posiciones y LOD intactos",log);
        Check(assetsOriginales.All(par=>EditorJsonUtility.ToJson(par.Key)==par.Value),"Materiales fuente sin modificar",log);
        foreach (var par in materialesOriginales)
        {
            var actual = par.Key.sharedMaterial;
            if (actual == par.Value) continue;
            Check(actual.GetFloat("_UsarForma")==1,"Mascara refinada activa",log);
            Check(actual.GetColor("_Color")==par.Value.GetColor("_Color")
                && actual.GetColor("_EmissionColor")==par.Value.GetColor("_EmissionColor"),
                "Color y emision HDR originales: " + par.Value.name,log);
            foreach (var propiedad in new[] {"_Glossiness","_Metallic","_SrcBlend","_DstBlend","_ZWrite"})
                Check(actual.GetFloat(propiedad)==par.Value.GetFloat(propiedad),"Propiedad original " + propiedad,log);
            Check(actual.IsKeywordEnabled("_EMISSION")==par.Value.IsKeywordEnabled("_EMISSION"),"Emision original habilitada",log);
        }
        foreach (var raiz in new[] {go,zona.bosqueardienteContenedorGameObjects})
            Check(raiz.GetComponentsInChildren<ParticleSystem>(true).Where(p=>p.name.StartsWith("Fuego"))
                .All(p=>p.GetComponent<ParticleSystemRenderer>().sharedMaterial.shader.name=="Campania/Bosque Ardiente/Llamas"),
                "Llamas refinadas: " + raiz.name,log);
        Check(!go.GetComponentsInChildren<Transform>().Any(t=>t.name.StartsWith("LlamaIncendioLejano")),
            "Sin incendios grandes del rework",log);
        Check(ambiente.GetComponentsInChildren<ParticleSystem>(true).Count(p=>p.name.Contains("Brasas de los")
            ||p.name.Contains("Ceniza tenue")||p.name.Contains("Humo que deriva"))==3,
            "Brasas, ceniza y humo ambiental conservados",log);
        var cantidad = ambiente.GetComponentsInChildren<ParticleSystem>(true).Length;
        var mat = zona.TexturaTerreno.sharedMaterial;
        AmbienteBosqueArdiente.Preparar(zona,false);
        Check(cantidad==ambiente.GetComponentsInChildren<ParticleSystem>(true).Length && mat==zona.TexturaTerreno.sharedMaterial,"Preparar dos veces no duplica",log);
        Check(estadoOriginal.All(par=>Estado(par.Key)==par.Value),"Reaplicar tampoco modifica emisores",log);
        var todos = UnityEngine.Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
        foreach (var ps in todos) ps.Simulate(0,false,true);
        for (int f=0; f<420; f++)
        {
            ambiente.Avanzar(1f/60);
            foreach (var ps in todos) ps.Simulate(1f/60,false,false);
        }
        Check(UnityEngine.Random.state.Equals(rng),"RNG de juego intacto al animar",log);
        Check(fires.Select(f=>f.transform.position).SequenceEqual(nodos),"Posiciones originales intactas",log);
        Capturar(cam,"preview-despues");
        cam.transform.position = new Vector3(24,28,-38);
        cam.transform.LookAt(Vector3.zero);
        Capturar(cam,"terreno-y-particulas-fuego-original");
        for (int f=0; f<100; f++) { ambiente.Avanzar(1f/60); foreach(var ps in todos) ps.Simulate(1f/60,false,false); }
        cam.transform.position = new Vector3(1.5f,3,-6);
        cam.transform.LookAt(new Vector3(0,.4f,-1));
        Capturar(cam,"detalle-llamas");
        var refinados = materialesOriginales.Keys.ToDictionary(r=>r,r=>r.sharedMaterial);
        foreach (var par in materialesOriginales) par.Key.sharedMaterial=par.Value;
        Capturar(cam,"detalle-llamas-originales");
        var pixelesOriginales = ultimaCaptura;
        foreach (var par in refinados) par.Key.sharedMaterial=par.Value;
        foreach (var m in refinados.Values.Distinct()) if (m.HasProperty("_UsarForma")) m.SetFloat("_UsarForma",0);
        Capturar(cam,"material-original-sin-mascara");
        long diferencia=0; int comparados=0;
        for (int i=0;i<pixelesOriginales.Length;i++)
        {
            var a=pixelesOriginales[i]; var b=ultimaCaptura[i];
            if (a.r<128 || a.r<a.g*1.1f || a.g<a.b*1.2f) continue;
            diferencia+=Math.Abs(a.r-b.r)+Math.Abs(a.g-b.g)+Math.Abs(a.b-b.b);
            comparados++;
        }
        double errorMaterial=diferencia/(Math.Max(1,comparados)*3.0*255);
        Check(comparados>50 && errorMaterial<.005,"Sombreado original conservado: error "
            +errorMaterial.ToString("P3")+" en "+comparados+" pixeles de fuego",log);
        foreach (var m in refinados.Values.Distinct()) if (m.HasProperty("_UsarForma")) m.SetFloat("_UsarForma",1);
        sol.intensity=.64f;
        sol.color=new Color(.432f,.546f,.811f);
        RenderSettings.ambientLight=new Color(.359f,.451f,.698f);
        Capturar(cam,"detalle-noche");
        float reloj=mat.GetFloat("_Reloj");
        ambiente.Avanzar(0);
        Check(reloj==mat.GetFloat("_Reloj"),"Reloj respeta dt cero",log);
        ambiente.gameObject.SetActive(false);
        Check(ambiente.GetComponentsInChildren<ParticleSystem>(true).Where(p=>p.name.Contains("incendios")||p.name.Contains("tenue")||p.name.Contains("deriva")).All(p=>p.particleCount==0),"Ambiente limpio al salir de campaña",log);
        File.WriteAllText(Path.Combine(Salida,"validacion.txt"),log.ToString());
        Debug.Log(log.ToString());
    }
    static int Capturar(Camera cam,string nombre)
    {
        var rt = new RenderTexture(1600,900,24);
        var t = new Texture2D(1600,900,TextureFormat.RGB24,false);
        cam.targetTexture=rt; cam.Render(); RenderTexture.active=rt;
        t.ReadPixels(new Rect(0,0,1600,900),0,0); t.Apply();
        ultimaCaptura = t.GetPixels32();
        int fuego = ultimaCaptura.Count(c=>c.r>140 && c.r>c.g*1.3f && c.g>c.b*1.4f);
        File.WriteAllBytes(Path.Combine(Salida,nombre+".png"),t.EncodeToPNG());
        cam.targetTexture=null; RenderTexture.active=null;
        UnityEngine.Object.DestroyImmediate(t); UnityEngine.Object.DestroyImmediate(rt);
        return fuego;
    }
}
