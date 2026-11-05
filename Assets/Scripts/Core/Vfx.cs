using UnityEngine;

/// <summary>
/// Builds simple particle effects in code (explosion, hit sparks). Put one on the GameManager
/// object and assign a particle material; other scripts call Vfx.Explosion / Vfx.Sparks.
/// </summary>
public class Vfx : MonoBehaviour
{
    public static Vfx Instance { get; private set; }

    public Material particleMaterial;

    void Awake() { Instance = this; }
    void OnDestroy() { if (Instance == this) Instance = null; }

    public static void Explosion(Vector3 position, float radius)
    {
        if (Instance == null) return;
        Instance.Burst(position, 60, 0.9f, radius * 1.2f, 0.8f, 2.5f,
            new Color(1f, 0.85f, 0.3f), new Color(0.9f, 0.2f, 0.05f, 0f));
        Instance.Burst(position, 25, 1.6f, radius * 0.4f, 1.2f, 3f,
            new Color(0.3f, 0.3f, 0.3f, 0.8f), new Color(0.1f, 0.1f, 0.1f, 0f));
        Instance.FlashLight(position, new Color(1f, 0.6f, 0.2f), 8f, radius * 2f, 0.25f);
    }

    public static void Sparks(Vector3 position)
    {
        if (Instance == null) return;
        Instance.Burst(position, 12, 0.3f, 3f, 0.08f, 0.2f,
            new Color(1f, 0.9f, 0.4f), new Color(1f, 0.3f, 0.1f, 0f));
    }

    void Burst(Vector3 pos, int count, float lifetime, float speed, float startSize, float endSize,
               Color startColor, Color endColor)
    {
        var go = new GameObject("VFX");
        go.transform.position = pos;
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = lifetime;
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.5f, speed);
        main.startSize = new ParticleSystem.MinMaxCurve(startSize, startSize * 1.6f);
        main.gravityModifier = 0.2f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(startColor, 0f), new GradientColorKey(endColor, 1f) },
            new[] { new GradientAlphaKey(startColor.a, 0f), new GradientAlphaKey(endColor.a, 1f) });
        col.color = grad;

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        // Grow (or shrink) from the start size to the end size over the particle's life.
        size.size = new ParticleSystem.MinMaxCurve(1f,
            AnimationCurve.Linear(0f, 1f, 1f, endSize / Mathf.Max(startSize, 0.01f)));

        var rend = go.GetComponent<ParticleSystemRenderer>();
        rend.sharedMaterial = particleMaterial;

        ps.Play();
        Destroy(go, lifetime + 1f);
    }

    void FlashLight(Vector3 pos, Color color, float intensity, float range, float duration)
    {
        var go = new GameObject("VFX Light");
        go.transform.position = pos + Vector3.up * 0.5f;
        var l = go.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = color;
        l.intensity = intensity;
        l.range = range;
        Destroy(go, duration);
    }
}


