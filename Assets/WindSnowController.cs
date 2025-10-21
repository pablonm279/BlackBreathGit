using UnityEngine;

[ExecuteAlways]
public class WindTrailsController : MonoBehaviour
{
    public ParticleSystem ps;

    public Vector3 windDirection = new Vector3(-1f, -0.05f, 0.25f);
    [Range(0,1)] public float intensity = 0.6f;

    public Vector2 speedRange = new Vector2(12f, 18f);
    public Vector2 rateRange  = new Vector2(70f, 110f);
    public Vector2 trailLifeRange = new Vector2(0.35f, 0.6f);

    void OnValidate(){ Apply(); }
    void Update(){ Apply(); }

    void Apply(){
        if (!ps) return;
        Vector3 dir = windDirection.sqrMagnitude < 1e-4f ? Vector3.right : windDirection.normalized;

        var main = ps.main;
        float spd = Mathf.Lerp(speedRange.x, speedRange.y, intensity);
        main.startSpeed = spd;

        var em = ps.emission;
        var rate = em.rateOverTime; rate.constant = Mathf.Lerp(rateRange.x, rateRange.y, intensity);
        em.rateOverTime = rate;

        var vol = ps.velocityOverLifetime;
        vol.enabled = true; vol.space = ParticleSystemSimulationSpace.World;
        vol.x = spd * dir.x; vol.y = spd * dir.y; vol.z = spd * dir.z;

        var trails = ps.trails;
        trails.lifetime = Mathf.Lerp(trailLifeRange.x, trailLifeRange.y, intensity);
    }
}
