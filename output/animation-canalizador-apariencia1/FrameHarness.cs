using System;
public class Sprite {}
public class PerfilAnimacionIlustrada { public Sprite origenIdle, origenMover, reposo, anticipacion, impacto; }
public class UnidadPoseController { public enum TipoPoseActual { Idle, Mover, Atacar, Habilidad, RecibirDanio } public Sprite poseMover, poseHabilidad, reposo; public Sprite ObtenerPoseReposoActual(){return reposo;} }
public static class MeleeTimingUtility { public static int CalcularPreImpactoMs(UnidadPoseController d){return 420;} }
public static class FrameHarness {
    public static Sprite EvaluarFrame(PerfilAnimacionIlustrada datos, UnidadPoseController.TipoPoseActual estado,
        float segundos, UnidadPoseController destino, Sprite spriteOrigen = null, float? impactoHabilidad = null)
    {
        switch (estado)
        {
            case UnidadPoseController.TipoPoseActual.Mover:
                return destino.poseMover != null ? destino.poseMover : datos.origenMover;
            case UnidadPoseController.TipoPoseActual.Atacar:
                // Usa el mismo instante de impacto que las habilidades existentes.
                float impactoSeg = impactoHabilidad ?? MeleeTimingUtility.CalcularPreImpactoMs(destino) / 1000f;
                if (segundos < impactoSeg) return datos.anticipacion;
                if (segundos < impactoSeg + 0.18f) return datos.impacto;
                return ResolverReposo(datos, destino);
            case UnidadPoseController.TipoPoseActual.Habilidad:
                return spriteOrigen != null ? spriteOrigen : destino.poseHabilidad;
            default:
                Sprite reposoActual = spriteOrigen != null ? spriteOrigen : destino.ObtenerPoseReposoActual();
                return reposoActual == datos.origenIdle ? datos.reposo : reposoActual;
        }
    }

    private static Sprite ResolverReposo(PerfilAnimacionIlustrada datos, UnidadPoseController destino)
    {
        Sprite reposoActual = destino.ObtenerPoseReposoActual();
        return reposoActual == datos.origenIdle ? datos.reposo : reposoActual;
    }


static void Check(bool condition,string message){if(!condition) throw new Exception(message);}
public static string Run(){
 var p=new PerfilAnimacionIlustrada {origenIdle=new Sprite(),origenMover=new Sprite(),reposo=new Sprite(),anticipacion=new Sprite(),impacto=new Sprite()};
 var c=new UnidadPoseController {poseMover=p.origenMover,poseHabilidad=new Sprite(),reposo=p.origenIdle};
 var attack=UnidadPoseController.TipoPoseActual.Atacar;
 foreach(float delay in new[]{0.18f,0.43f,1.6f}) {
 Check(EvaluarFrame(p,attack,delay+2,c,null,float.PositiveInfinity)==p.anticipacion,"Debe esperar al efecto real");
 Check(EvaluarFrame(p,attack,delay,c,null,delay)==p.impacto,"Debe impactar al confirmar");
 Check(EvaluarFrame(p,attack,delay+0.17f,c,null,delay)==p.impacto,"Ventana de impacto");
 Check(EvaluarFrame(p,attack,delay+0.19f,c,null,delay)==p.reposo,"Recuperacion");
 }
 Check(EvaluarFrame(p,attack,0.41f,c)==p.anticipacion,"Timing anterior preservado");
 Check(EvaluarFrame(p,attack,0.43f,c)==p.impacto,"Impacto anterior preservado");
 Check(EvaluarFrame(p,UnidadPoseController.TipoPoseActual.Habilidad,3,c,c.poseHabilidad,float.PositiveInfinity)==c.poseHabilidad,"Habilidad sostenida preservada");
 c.reposo=new Sprite();Check(EvaluarFrame(p,attack,3,c,null,0.43f)==c.reposo,"Atento original al recuperar");
 return "16 comprobaciones OK sobre EvaluarFrame/ResolverReposo extraidos del fuente actual; dobles de Sprite/controlador, sin Play Mode.";
}}
