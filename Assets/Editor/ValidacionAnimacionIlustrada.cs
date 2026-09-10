using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Prueba acotada en una escena vacia y no guardada, mediante -executeMethod.
// No carga ni modifica las escenas de campaña/tutorial, ni instancia su logica.
[InitializeOnLoad]
public static class ValidacionAnimacionIlustrada
{
    private const string Clave = "GDD.ValidacionAnimacionIlustrada";
    private static readonly List<string> resultados = new List<string>();
    private static UnidadPoseController caballero;
    private static UnidadPoseController driada;
    private static PerfilAnimacionIlustrada perfilCaballero;
    private static PerfilAnimacionIlustrada perfilDriada;
    private static UnidadPoseController fuenteCaballero;
    private static UnidadPoseController fuenteDriada;
    private static AparienciaAlternativaCaballero alternativa;
    private static Camera camara;
    private static int etapa;
    private static double siguiente;
    private static Sprite framePausado;

    static ValidacionAnimacionIlustrada()
    {
        EditorApplication.update += Actualizar;
        EditorApplication.delayCall += () =>
        {
            const string solicitud = "outputs/animation-pilot/validar-editor.request";
            if (!EditorApplication.isPlayingOrWillChangePlaymode && File.Exists(solicitud))
            {
                File.Delete(solicitud);
                ValidarEnEditor();
            }
        };
    }

    [MenuItem("Tools/Arte/Validar animaciones Caballero y Driada")]
    public static void ValidarEnEditor()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        GameObject raiz = null;
        resultados.Clear();
        try
        {
            perfilCaballero = Resources.Load<PerfilAnimacionIlustrada>("AnimacionesIlustradas/CaballeroApariencia1");
            perfilDriada = Resources.Load<PerfilAnimacionIlustrada>("AnimacionesIlustradas/DriadaQuemada");
            Exigir(perfilCaballero != null && perfilCaballero.EstaCompleto && perfilDriada != null && perfilDriada.EstaCompleto,
                "Recursos importados: seis sprites y dos perfiles completos");
            fuenteCaballero = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scripts/Clases/Caballero/UnidadCaballero.prefab").GetComponent<UnidadPoseController>();
            fuenteDriada = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Prefabs NPC/ZONA - Bosque de los Lamentos/DriadaQuemada/DriadaQuemada.prefab").GetComponent<UnidadPoseController>();
            alternativa = fuenteCaballero.GetComponent<ClaseCaballero>().aparienciasAlternativas[0];
            raiz = EditorUtility.CreateGameObjectWithHideFlags("ValidacionIlustraciones", HideFlags.HideAndDontSave, typeof(RectTransform));
            caballero = CrearControlador(raiz.transform, fuenteCaballero, -245);
            driada = CrearControlador(raiz.transform, fuenteDriada, 245);
            var awake = typeof(UnidadPoseController).GetMethod("Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            foreach (UnidadPoseController destino in new[] { caballero, driada })
            {
                awake.Invoke(destino, null);
                PerfilAnimacionIlustrada datos = destino == caballero ? perfilCaballero : perfilDriada;
                destino.SetIdle();
                Exigir(destino.targetImage.sprite == datos.reposo, destino.name + ": reposo nuevo");
                destino.EnterPoseObjetivoHostil();
                Exigir(destino.targetImage.sprite == destino.poseTurnoActivo, destino.name + ": atento original distinto de reposo");
                Exigir(UnidadAnimacionIlustrada.EvaluarFrame(datos, UnidadPoseController.TipoPoseActual.RecibirDanio, 0.1f, destino) == destino.poseTurnoActivo,
                    destino.name + ": dano conserva la pose, sin sprite propio");
                Exigir(UnidadAnimacionIlustrada.EvaluarFrame(datos, UnidadPoseController.TipoPoseActual.RecibirDanio, 0.4f, destino) == destino.poseTurnoActivo,
                    destino.name + ": recuperacion de dano vuelve a atento");
                Exigir(UnidadAnimacionIlustrada.EvaluarFrame(datos, UnidadPoseController.TipoPoseActual.Atacar, 1f, destino) == destino.poseTurnoActivo,
                    destino.name + ": recuperacion de ataque vuelve a atento");
                destino.ExitPoseObjetivoHostil();
                destino.OnStartMove();
                Exigir(destino.targetImage.sprite == destino.poseMover, destino.name + ": dash conserva sprite original");
                destino.OnStopMove();
                destino.EnterSkillPoseHold();
                destino.PlayAttackPose();
                Exigir(destino.targetImage.sprite == destino.poseHabilidad, destino.name + ": habilidad sostenida conserva pose y prioridad");
                destino.ExitPoseHold();
                destino.EnterAttackPoseHold();
                Exigir(destino.targetImage.sprite == datos.anticipacion, destino.name + ": ataque comienza con anticipacion");
                float instanteImpacto = MeleeTimingEnEditor(destino);
                Exigir(UnidadAnimacionIlustrada.EvaluarFrame(datos, UnidadPoseController.TipoPoseActual.Atacar, instanteImpacto + 0.02f, destino) == datos.impacto,
                    destino.name + ": impacto conserva timing de combate");
                destino.ExitAttackPoseHold();
            }
            ComprobarApariencia2();
            caballero.RestaurarPosesBase();
            Exigir(caballero.TieneAnimacionIlustrada && caballero.targetImage.sprite == perfilCaballero.reposo,
                "Volver a apariencia 1 restaura su perfil original");
            Sprite defensa = fuenteCaballero.GetComponent<ClaseCaballero>().Pose_PosturaDefensiva;
            caballero.poseIdle = defensa;
            caballero.poseTurnoActivo = defensa;
            caballero.SetIdle();
            Exigir(caballero.targetImage.sprite == defensa, "Postura defensiva original conservada");
            caballero.RestaurarPosesBase();
            ComprobarDuelista(raiz.transform);
            ComprobarRitmoRespiracion(raiz.transform);
            ComprobarEncuadreAlMorir();
        }
        catch (Exception error) { resultados.Add("ERROR: " + error); }
        finally
        {
            if (raiz != null) UnityEngine.Object.DestroyImmediate(raiz);
            Directory.CreateDirectory("outputs/animation-pilot");
            File.WriteAllLines("outputs/animation-pilot/validacion-editor.txt", resultados);
        }
    }

    private static void ComprobarDuelista(Transform raiz)
    {
        var fuente = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scripts/Clases/Duelista/UnidadDuelista.prefab").GetComponent<UnidadPoseController>();
        var datos = Resources.Load<PerfilAnimacionIlustrada>("AnimacionesIlustradas/DuelistaApariencia1");
        Exigir(datos != null && datos.EstaCompleto && datos.Coincide(fuente), "Duelista 1: perfil y tres sprites validos");
        var destino = CrearControlador(raiz, fuente, 0);
        typeof(UnidadPoseController).GetMethod("Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(destino, null);
        destino.SetIdle();
        Exigir(destino.TieneAnimacionIlustrada && destino.targetImage.sprite == datos.reposo, "Duelista 1: idle ilustrado activo");
        destino.EnterPoseObjetivoHostil();
        Exigir(destino.targetImage.sprite == fuente.poseTurnoActivo, "Duelista 1: atento original");
        Exigir(UnidadAnimacionIlustrada.EvaluarFrame(datos, UnidadPoseController.TipoPoseActual.Atacar, 1f, destino)
            == fuente.poseTurnoActivo, "Duelista 1: ataque recupera atento");
        destino.ExitPoseObjetivoHostil();
        destino.OnStartMove();
        Exigir(destino.targetImage.sprite == fuente.poseMover, "Duelista 1: dash original");
        destino.OnStopMove();
        destino.EnterSkillPoseHold();
        destino.PlayAttackPose();
        Exigir(destino.targetImage.sprite == fuente.poseHabilidad, "Duelista 1: habilidad sostenida conserva prioridad");
        destino.ExitPoseHold();
        destino.EnterAttackPoseHold();
        Exigir(destino.targetImage.sprite == datos.anticipacion, "Duelista 1: anticipacion de estocada");
        Exigir(UnidadAnimacionIlustrada.EvaluarFrame(datos, UnidadPoseController.TipoPoseActual.Atacar,
            MeleeTimingEnEditor(destino) + 0.02f, destino) == datos.impacto, "Duelista 1: estocada sincronizada");
        destino.ExitAttackPoseHold();
        Sprite enGarde = fuente.GetComponent<ClaseDuelista>().Pose_Engarde;
        destino.poseIdle = enGarde;
        destino.poseTurnoActivo = enGarde;
        destino.SetIdle();
        Exigir(enGarde != null && destino.targetImage.sprite == enGarde, "Duelista 1: En Garde original");
        Exigir(UnidadAnimacionIlustrada.EvaluarFrame(datos, UnidadPoseController.TipoPoseActual.Atacar, 1f, destino)
            == enGarde, "Duelista 1: ataque recupera En Garde");
        Exigir(destino.poseRecibirDanio == fuente.poseRecibirDanio, "Duelista 1: sin sprite nuevo de dano");
        var alternativaDuelista = fuente.GetComponent<ClaseDuelista>().aparienciasAlternativas[0];
        destino.ConfigurarPoses(alternativaDuelista.poseIdle, alternativaDuelista.poseMover, alternativaDuelista.poseAtacar,
            alternativaDuelista.poseHabilidad, alternativaDuelista.poseRecibirDanio, alternativaDuelista.poseTurnoActivo);
        var datos2 = Resources.Load<PerfilAnimacionIlustrada>("AnimacionesIlustradas/DuelistaApariencia2");
        Exigir(datos2 != null && datos2.EstaCompleto && datos2.Coincide(destino) && !datos2.Coincide(fuente),
            "Duelista 2: perfil completo y separado de apariencia 1");
        Exigir(destino.TieneAnimacionIlustrada && destino.targetImage.sprite == datos2.reposo,
            "Cambiar a Duelista 2 activa su reposo ilustrado");
        destino.EnterPoseObjetivoHostil();
        Exigir(destino.targetImage.sprite == alternativaDuelista.poseTurnoActivo, "Duelista 2: atento original");
        destino.ExitPoseObjetivoHostil();
        destino.OnStartMove();
        Exigir(destino.targetImage.sprite == alternativaDuelista.poseMover, "Duelista 2: dash original");
        destino.OnStopMove();
        destino.EnterSkillPoseHold();
        destino.PlayAttackPose();
        Exigir(destino.targetImage.sprite == alternativaDuelista.poseHabilidad, "Duelista 2: habilidad sostenida original");
        destino.ExitPoseHold();
        destino.EnterAttackPoseHold();
        Exigir(destino.targetImage.sprite == datos2.anticipacion, "Duelista 2: anticipacion ilustrada");
        Exigir(UnidadAnimacionIlustrada.EvaluarFrame(datos2, UnidadPoseController.TipoPoseActual.Atacar,
            MeleeTimingEnEditor(destino) + 0.02f, destino) == datos2.impacto, "Duelista 2: impacto sincronizado");
        destino.ExitAttackPoseHold();
        destino.poseIdle = alternativaDuelista.poseEnGarde;
        destino.poseTurnoActivo = alternativaDuelista.poseEnGarde;
        destino.SetIdle();
        Exigir(destino.targetImage.sprite == alternativaDuelista.poseEnGarde, "Duelista 2: En Garde original");
        Exigir(UnidadAnimacionIlustrada.EvaluarFrame(datos2, UnidadPoseController.TipoPoseActual.Atacar, 1f, destino)
            == alternativaDuelista.poseEnGarde, "Duelista 2: ataque recupera En Garde");
        Exigir(destino.poseRecibirDanio == alternativaDuelista.poseRecibirDanio, "Duelista 2: sin sprite nuevo de dano");
        destino.RestaurarPosesBase();
        Exigir(destino.TieneAnimacionIlustrada && destino.targetImage.sprite == datos.reposo,
            "Volver a Duelista 1 restaura su perfil");
    }

    private static void ComprobarRitmoRespiracion(Transform raiz)
    {
        // Manager desactivado: no ejecuta Awake, carga escenas ni inicia el combate.
        GameObject go = new GameObject("RitmoRespiracion_Prueba");
        go.SetActive(false);
        go.transform.SetParent(raiz, false);
        BattleManager temporal = go.AddComponent<BattleManager>();
        BattleManager anterior = BattleManager.Instance;
        var setter = typeof(BattleManager).GetProperty("Instance").GetSetMethod(true);
        try
        {
            setter.Invoke(null, new object[] { temporal });
            Unidad jugador = fuenteCaballero.GetComponent<Unidad>();
            Unidad enemigo = fuenteDriada.GetComponent<Unidad>();
            temporal.unidadActiva = jugador;
            Exigir(UnidadAmbientMotionFx.ObtenerFactorRespiracion(jugador) == 1f
                && UnidadAmbientMotionFx.ObtenerFactorRespiracion(enemigo) == 0.8f, "Respiracion: jugador activo 100%, enemigo en espera 80%");
            temporal.unidadActiva = enemigo;
            Exigir(UnidadAmbientMotionFx.ObtenerFactorRespiracion(enemigo) == 1f
                && UnidadAmbientMotionFx.ObtenerFactorRespiracion(jugador) == 0.8f, "Respiracion: enemigo activo 100%, jugador en espera 80%");
            var reloj = typeof(UnidadAnimacionIlustrada).GetField("tiempoRespiracion", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var motor = caballero.targetImage.GetComponent<UnidadAnimacionIlustrada>();
            reloj.SetValue(motor, 12.5f);
            caballero.EnterPoseObjetivoHostil();
            Exigir((float)reloj.GetValue(motor) == 12.5f, "Cambiar a atento no reinicia la fase respiratoria");
            caballero.ExitPoseObjetivoHostil();
            caballero.EnterAttackPoseHold();
            Exigir(caballero.targetImage.sprite == perfilCaballero.anticipacion,
                "El ritmo respiratorio no cambia el inicio del ataque");
            caballero.ExitAttackPoseHold();
            setter.Invoke(null, new object[] { null });
            Exigir(UnidadAmbientMotionFx.ObtenerFactorRespiracion(jugador) == 1f, "Sin batalla se conserva la velocidad normal");
        }
        finally { setter.Invoke(null, new object[] { anterior }); }
    }

    private static float MeleeTimingEnEditor(UnidadPoseController destino)
    {
        return Mathf.Round(Mathf.Clamp(destino.duracionPoseAtacar * destino.meleeFraccionImpacto,
            destino.meleePreImpactoMin, destino.meleePreImpactoMax) * 1000f) / 1000f;
    }

    private static void ComprobarApariencia2()
    {
        PerfilAnimacionIlustrada datos = Resources.Load<PerfilAnimacionIlustrada>("AnimacionesIlustradas/CaballeroApariencia2");
        Exigir(datos != null && datos.EstaCompleto, "Apariencia 2: tres sprites y perfil importados");
        Exigir(!datos.Coincide(fuenteCaballero) && !datos.Coincide(fuenteDriada), "Apariencia 2: perfil aislado de las unidades previas");
        caballero.ConfigurarPoses(alternativa.poseIdle, alternativa.poseMover, alternativa.poseAtacar,
            alternativa.poseHabilidad, alternativa.poseRecibirDanio, alternativa.poseTurnoActivo);
        Exigir(caballero.TieneAnimacionIlustrada && caballero.targetImage.sprite == datos.reposo,
            "Apariencia 2 activa su perfil al cambiar las poses despues de Awake");
        Exigir(caballero.targetImage.GetComponents<UnidadAnimacionIlustrada>().Length == 1,
            "Cambiar apariencia reutiliza un solo motor");
        caballero.EnterPoseObjetivoHostil();
        Exigir(caballero.targetImage.sprite == alternativa.poseTurnoActivo, "Apariencia 2: atento original");
        Exigir(UnidadAnimacionIlustrada.EvaluarFrame(datos, UnidadPoseController.TipoPoseActual.Atacar, 1f, caballero)
            == alternativa.poseTurnoActivo, "Apariencia 2: ataque recupera atento si sigue seleccionado");
        caballero.ExitPoseObjetivoHostil();
        caballero.OnStartMove();
        Exigir(caballero.targetImage.sprite == alternativa.poseMover, "Apariencia 2: dash original");
        caballero.OnStopMove();
        caballero.EnterSkillPoseHold();
        caballero.PlayAttackPose();
        Exigir(caballero.targetImage.sprite == alternativa.poseHabilidad, "Apariencia 2: habilidad original con prioridad");
        caballero.ExitPoseHold();
        caballero.EnterAttackPoseHold();
        Exigir(caballero.targetImage.sprite == datos.anticipacion, "Apariencia 2: anticipacion nueva");
        Exigir(UnidadAnimacionIlustrada.EvaluarFrame(datos, UnidadPoseController.TipoPoseActual.Atacar,
            MeleeTimingEnEditor(caballero) + 0.02f, caballero) == datos.impacto, "Apariencia 2: impacto sincronizado");
        caballero.ExitAttackPoseHold();
        caballero.poseIdle = alternativa.posePosturaDefensiva;
        caballero.poseTurnoActivo = alternativa.posePosturaDefensiva;
        caballero.SetIdle();
        Exigir(caballero.targetImage.sprite == alternativa.posePosturaDefensiva, "Apariencia 2: defensa original");
        Exigir(UnidadAnimacionIlustrada.EvaluarFrame(datos, UnidadPoseController.TipoPoseActual.Atacar, 1f, caballero)
            == alternativa.posePosturaDefensiva, "Apariencia 2: ataque recupera defensa");
        Exigir(caballero.poseRecibirDanio == alternativa.poseRecibirDanio, "Apariencia 2: sin sprite nuevo de dano");
        caballero.ConfigurarPoses(alternativa.poseIdle, alternativa.poseIdle, alternativa.poseIdle, alternativa.poseHabilidad);
        Exigir(!caballero.TieneAnimacionIlustrada && caballero.targetImage.sprite == alternativa.poseIdle,
            "Una apariencia sin perfil mantiene sus sprites originales");
    }

    public static void Ejecutar()
    {
        if (!Application.isBatchMode && Array.IndexOf(Environment.GetCommandLineArgs(), "-gddAnimationValidation") < 0)
            throw new InvalidOperationException("Esta validacion requiere una instancia dedicada del editor.");
        Directory.CreateDirectory("outputs/animation-pilot");
        SessionState.SetBool(Clave, true);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorApplication.EnterPlaymode();
    }

    private static void Exigir(bool condicion, string mensaje)
    {
        if (!condicion) throw new InvalidOperationException(mensaje);
        resultados.Add("OK: " + mensaje);
    }

    private static void Actualizar()
    {
        if (!SessionState.GetBool(Clave, false) || !EditorApplication.isPlaying
            || EditorApplication.isCompiling || EditorApplication.timeSinceStartup < siguiente) return;
        try
        {
            switch (etapa++)
            {
                case 0:
                    Preparar();
                    siguiente = EditorApplication.timeSinceStartup + 0.3;
                    break;
                case 1:
                    Exigir(caballero.TieneAnimacionIlustrada && driada.TieneAnimacionIlustrada, "Ambos perfiles se activan en Play Mode");
                    Exigir(caballero.targetImage.sprite == perfilCaballero.reposo && driada.targetImage.sprite == perfilDriada.reposo,
                        "Reposo usa los nuevos sprites");
                    caballero.EnterPoseObjetivoHostil();
                    caballero.EnterPoseObjetivoHostil();
                    Exigir(caballero.targetImage.sprite == fuenteCaballero.poseTurnoActivo,
                        "Ser objetivo conserva la pose atento original, distinta de reposo");
                    Exigir(UnidadAnimacionIlustrada.EvaluarFrame(perfilCaballero,
                        UnidadPoseController.TipoPoseActual.RecibirDanio, 0.1f, caballero) == fuenteCaballero.poseTurnoActivo,
                        "Dano conserva atento sin sprite propio");
                    Exigir(UnidadAnimacionIlustrada.EvaluarFrame(perfilCaballero,
                        UnidadPoseController.TipoPoseActual.RecibirDanio, 0.4f, caballero) == fuenteCaballero.poseTurnoActivo,
                        "Tras el impacto recupera atento si sigue siendo objetivo");
                    Exigir(UnidadAnimacionIlustrada.EvaluarFrame(perfilCaballero,
                        UnidadPoseController.TipoPoseActual.Atacar, 1f, caballero) == fuenteCaballero.poseTurnoActivo,
                        "La recuperacion de ataque respeta la pose atento");
                    caballero.ExitPoseObjetivoHostil();
                    Exigir(caballero.targetImage.sprite == fuenteCaballero.poseTurnoActivo,
                        "Liberar solo un seleccionador no borra atento");
                    caballero.ExitPoseObjetivoHostil();
                    Exigir(caballero.targetImage.sprite == perfilCaballero.reposo, "Liberar todos los objetivos restaura reposo");
                    caballero.EnterSkillPoseHold();
                    caballero.PlayAttackPose();
                    Exigir(caballero.targetImage.sprite == fuenteCaballero.poseHabilidad, "La habilidad sostenida conserva prioridad");
                    caballero.ExitPoseHold();
                    Sprite defensa = fuenteCaballero.GetComponent<ClaseCaballero>().Pose_PosturaDefensiva;
                    caballero.poseIdle = defensa;
                    caballero.poseTurnoActivo = defensa;
                    caballero.SetIdle();
                    Exigir(caballero.targetImage.sprite == defensa, "La postura defensiva original sigue visible");
                    caballero.RestaurarPosesBase();
                    driada.OnStartMove();
                    siguiente = EditorApplication.timeSinceStartup + 0.23;
                    break;
                case 2:
                    Exigir(driada.targetImage.sprite == fuenteDriada.poseMover, "El dash conserva el sprite de movimiento original");
                    driada.OnStopMove();
                    Exigir(driada.targetImage.sprite == perfilDriada.reposo, "Detener movimiento restaura reposo");
                    caballero.PlayAttackPose();
                    driada.PlayAttackPose();
                    Exigir(caballero.targetImage.sprite == perfilCaballero.anticipacion
                        && driada.targetImage.sprite == perfilDriada.anticipacion, "Ambos ataques comienzan con anticipacion");
                    Capturar("unity-anticipacion.png");
                    siguiente = EditorApplication.timeSinceStartup + 0.44;
                    break;
                case 3:
                    // El muestreo del impacto es determinista, independiente del FPS del batch runner.
                    Exigir(UnidadAnimacionIlustrada.EvaluarFrame(perfilCaballero,
                        UnidadPoseController.TipoPoseActual.Atacar, 0.43f, caballero) == perfilCaballero.impacto,
                        "Caballero: impacto sincronizado con MeleeTimingUtility (0.42 s)");
                    Exigir(UnidadAnimacionIlustrada.EvaluarFrame(perfilDriada,
                        UnidadPoseController.TipoPoseActual.Atacar, 0.36f, driada) == perfilDriada.impacto,
                        "Driada: impacto sincronizado con MeleeTimingUtility (0.35 s)");
                    Exigir(caballero.targetImage.sprite != perfilCaballero.anticipacion, "La secuencia de ataque avanza en Play Mode");
                    Capturar("unity-ataque.png");
                    Time.timeScale = 0f;
                    framePausado = caballero.targetImage.sprite;
                    siguiente = EditorApplication.timeSinceStartup + 0.25;
                    break;
                case 4:
                    Exigir(caballero.targetImage.sprite == framePausado, "Time.timeScale=0 congela la animacion");
                    Time.timeScale = 1f;
                    caballero.SetIdle();
                    driada.SetIdle();
                    caballero.PlayDamagePose();
                    driada.PlayDamagePose();
                    Exigir(caballero.targetImage.sprite == fuenteCaballero.poseTurnoActivo && driada.targetImage.sprite == fuenteDriada.poseTurnoActivo,
                        "Dano mantiene el fallback original a atento cuando no hay pose propia");
                    Capturar("unity-danio.png");
                    siguiente = EditorApplication.timeSinceStartup + 1.15;
                    break;
                case 5:
                    Exigir(caballero.targetImage.sprite == perfilCaballero.reposo && driada.targetImage.sprite == perfilDriada.reposo,
                        "Las corutinas existentes restauran reposo tras dano");
                    ComprobarApariencia2();
                    caballero.RestaurarPosesBase();
                    Exigir(caballero.TieneAnimacionIlustrada && caballero.targetImage.sprite == perfilCaballero.reposo,
                        "Volver a apariencia 1 reactiva su perfil");
                    caballero.EnterAttackPoseHold();
                    caballero.SetIdle();
                    Exigir(caballero.targetImage.sprite == perfilCaballero.anticipacion, "El ataque sostenido bloquea SetIdle");
                    caballero.ExitAttackPoseHold();
                    Exigir(caballero.targetImage.sprite == perfilCaballero.reposo, "Liberar ataque sostenido restaura reposo");
                    Exigir(caballero.targetImage.rectTransform.localScale == Vector3.one,
                        "No se modifica la escala del RectTransform");
                    Exigir(caballero.targetImage.rectTransform.anchoredPosition == new Vector2(-245, -15),
                        "No se modifica la posicion del RectTransform");
                    ComprobarEncuadreAlMorir();
                    Capturar("unity-reposo.png");
                    Finalizar(null);
                    break;
            }
        }
        catch (Exception error) { Finalizar(error); }
    }

    private static void Preparar()
    {
        perfilCaballero = Resources.Load<PerfilAnimacionIlustrada>("AnimacionesIlustradas/CaballeroApariencia1");
        perfilDriada = Resources.Load<PerfilAnimacionIlustrada>("AnimacionesIlustradas/DriadaQuemada");
        Exigir(perfilCaballero != null && perfilCaballero.EstaCompleto && perfilDriada != null && perfilDriada.EstaCompleto,
                        "Unity importa los 6 sprites y los dos perfiles con referencias validas");
        fuenteCaballero = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scripts/Clases/Caballero/UnidadCaballero.prefab").GetComponent<UnidadPoseController>();
        fuenteDriada = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Prefabs NPC/ZONA - Bosque de los Lamentos/DriadaQuemada/DriadaQuemada.prefab").GetComponent<UnidadPoseController>();
        alternativa = fuenteCaballero.GetComponent<ClaseCaballero>().aparienciasAlternativas[0];
        camara = new GameObject("CamaraValidacion", typeof(Camera)).GetComponent<Camera>();
        camara.clearFlags = CameraClearFlags.SolidColor;
        camara.backgroundColor = new Color(0.065f, 0.083f, 0.105f);
        camara.orthographic = true;
        camara.orthographicSize = 4f;
        camara.transform.position = new Vector3(0, 0, -10);
        camara.targetTexture = new RenderTexture(1200, 900, 24);
        Canvas canvas = new GameObject("LienzoValidacion", typeof(Canvas)).GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camara;
        canvas.planeDistance = 5;
        caballero = CrearControlador(canvas.transform, fuenteCaballero, -245);
        driada = CrearControlador(canvas.transform, fuenteDriada, 245);
    }

    private static UnidadPoseController CrearControlador(Transform padre, UnidadPoseController fuente, float x)
    {
        GameObject go = new GameObject(fuente.name + "_Prueba", typeof(RectTransform));
        go.SetActive(false);
        go.transform.SetParent(padre, false);
        Image imagen = go.AddComponent<Image>();
        imagen.rectTransform.sizeDelta = new Vector2(360, 620);
        imagen.rectTransform.anchoredPosition = new Vector2(x, -15);
        UnidadPoseController destino = go.AddComponent<UnidadPoseController>();
        destino.targetImage = imagen;
        destino.poseIdle = fuente.poseIdle;
        destino.poseMover = fuente.poseMover;
        destino.poseAtacar = fuente.poseAtacar;
        destino.poseHabilidad = fuente.poseHabilidad;
        destino.poseTurnoActivo = fuente.poseTurnoActivo;
        destino.duracionPoseAtacar = fuente.duracionPoseAtacar;
        destino.duracionPoseDanyo = fuente.duracionPoseDanyo;
        go.SetActive(true);
        return destino;
    }

    private static void ComprobarEncuadreAlMorir()
    {
        UnidadAnimacionIlustrada motor = caballero.targetImage.GetComponent<UnidadAnimacionIlustrada>();
        Vector3 antes = VerticeDeApoyo(motor);
        caballero.DetenerAnimacionIlustrada();
        Exigir(!caballero.TieneAnimacionIlustrada, "Morir detiene la animacion nueva");
        Exigir(Vector3.Distance(antes, VerticeDeApoyo(motor)) < 0.001f,
            "Morir conserva el encuadre y evita saltos de tamano");
    }

    private static Vector3 VerticeDeApoyo(UnidadAnimacionIlustrada motor)
    {
        using (VertexHelper vh = new VertexHelper())
        {
            Rect rect = caballero.targetImage.rectTransform.rect;
            vh.AddVert(new Vector3(rect.xMin, rect.yMin + rect.height * perfilCaballero.apoyo.y), Color.white, Vector2.zero);
            motor.ModifyMesh(vh);
            UIVertex vertex = default;
            vh.PopulateUIVertex(ref vertex, 0);
            return vertex.position;
        }
    }

    private static void Capturar(string nombre)
    {
        Canvas.ForceUpdateCanvases();
        camara.Render();
        RenderTexture anterior = RenderTexture.active;
        RenderTexture.active = camara.targetTexture;
        Texture2D imagen = new Texture2D(1200, 900, TextureFormat.RGB24, false);
        imagen.ReadPixels(new Rect(0, 0, 1200, 900), 0, 0);
        imagen.Apply();
        File.WriteAllBytes("outputs/animation-pilot/" + nombre, imagen.EncodeToPNG());
        UnityEngine.Object.Destroy(imagen);
        RenderTexture.active = anterior;
    }

    private static void Finalizar(Exception error)
    {
        SessionState.SetBool(Clave, false);
        Time.timeScale = 1f;
        if (error != null) resultados.Add("ERROR: " + error);
        File.WriteAllLines("outputs/animation-pilot/validacion-unity.txt", resultados);
        EditorApplication.Exit(error == null ? 0 : 1);
    }
}
