using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaravanFollowerSimple : MonoBehaviour
{
    /*[Header("Refs")]
    public Transform leader;

    [Header("Follow")]
    public float delaySeconds = 0.3f;
    public float moveSmoothTime = 0.1f;

    [Header("Rotation")]
    public float rotationSpeed = 6f;
    public bool lockY = true;

    struct Snap
    {
        public float time;
        public Vector3 leaderPos;
        public Snap(float time, Vector3 pos)
        {
            this.time = time;
            this.leaderPos = pos;
        }
    }

    readonly List<Snap> buffer = new List<Snap>(256);

    Vector3 followerVelocity;

    Vector3 initialOffset;
    bool ready = false;

    IEnumerator Start()
    {   
        Invoke("Sinpadre", 0.5f);
        if (!leader) yield break;

        // Esperar 1 frame para que el líder y cualquier sistema (coroutines, snaps, etc.)
        // ya hayan seteado posiciones reales.
        yield return null;

        // Offset definido por tu pose en el editor (pero tomado DESPUÉS del settle)
        initialOffset = transform.position - leader.position;

        buffer.Clear();

        // Seed: llenamos el buffer como si el líder hubiera estado quieto,
        // para que el target con delay NO te pida una posición diferente al inicio.
        float now = Time.time;
        Vector3 lp = leader.position;

        int seedCount = Mathf.Max(2, Mathf.CeilToInt(delaySeconds / Mathf.Max(Time.deltaTime, 0.016f)));
        for (int i = 0; i < seedCount; i++)
            buffer.Add(new Snap(now - (seedCount - i) * 0.016f, lp));

        // Reset de inercia para evitar tirón
        followerVelocity = Vector3.zero;

        ready = true;
    }

    void LateUpdate()
    {
        if (!leader || !ready) return;

        float now = Time.time;

        // Registrar posición del líder
        buffer.Add(new Snap(now, leader.position));

        // Limpiar buffer viejo
        float keepFrom = now - (delaySeconds + 1.0f);
        while (buffer.Count > 0 && buffer[0].time < keepFrom)
            buffer.RemoveAt(0);

        // Target temporal
        float targetTime = now - delaySeconds;

        // Buscar snap para targetTime
        int i = 0;
        while (i < buffer.Count - 1 && buffer[i + 1].time < targetTime) i++;

        Snap a = buffer[i];
        Snap b = buffer[Mathf.Min(i + 1, buffer.Count - 1)];

        float t = Mathf.InverseLerp(a.time, b.time, targetTime);
        Vector3 leaderDelayedPos = Vector3.Lerp(a.leaderPos, b.leaderPos, t);

        Vector3 desiredPos = leaderDelayedPos + initialOffset;

        if (lockY)
            desiredPos.y = transform.position.y;

        // Movimiento suave
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPos,
            ref followerVelocity,
            moveSmoothTime
        );

        RotateToLeader();
    }

    void RotateToLeader()
    {
        Vector3 dir = leader.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
            float k = 1f - Mathf.Exp(-rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, k);
        }
    }
    void Sinpadre()
    {
        transform.parent = null;
    }*/
}
