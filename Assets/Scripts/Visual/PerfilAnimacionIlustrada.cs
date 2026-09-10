using UnityEngine;

// Las referencias originales identifican la apariencia, incluso si los nombres coinciden.
[CreateAssetMenu(menuName = "GDD/Perfil de animacion ilustrada")]
public sealed class PerfilAnimacionIlustrada : ScriptableObject
{
    public Sprite origenIdle;
    public Sprite origenTurnoActivo;
    public Sprite origenMover;
    public Sprite origenAtacar;
    public Sprite reposo;
    public Sprite anticipacion;
    public Sprite impacto;
    public Sprite preparacion; // Opcional: pose compartida de utilidades del Explorador.
    public Vector2 apoyo = new Vector2(0.56f, 0.075f);
    public float escalaHorizontal = 1f;
    public float desplazamientoHorizontal;
    public float respiracion = 0.006f;

    public bool Coincide(UnidadPoseController controlador)
    {
        return controlador != null && origenMover != null && origenAtacar != null
            && controlador.poseMover == origenMover && controlador.poseAtacar == origenAtacar;
    }

    public bool EstaCompleto => reposo != null && anticipacion != null && impacto != null;
}
