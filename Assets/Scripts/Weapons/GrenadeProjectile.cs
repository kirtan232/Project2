using UnityEngine;

/// <summary>
/// Thrown grenade. Explodes after a short fuse (or on the first hard impact after a delay),
/// killing every IShootable within the blast radius in one hit.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class GrenadeProjectile : MonoBehaviour
{
    public float fuseTime = 2f;
    public float blastRadius = 5f;
    public AudioClip explosionClip;
    public GameObject explosionEffectPrefab;   // optional

    bool exploded;

    void Start()
    {
        Invoke(nameof(Explode), fuseTime);
    }

    void Explode()
    {
        if (exploded) return;
        exploded = true;

        Sfx.Play(explosionClip, transform.position);
        Vfx.Explosion(transform.position, blastRadius);
        if (explosionEffectPrefab != null)
            Destroy(Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity), 3f);

        // One guard can have several colliders, so collect unique targets first.
        var hits = Physics.OverlapSphere(transform.position, blastRadius, ~0, QueryTriggerInteraction.Ignore);
        var done = new System.Collections.Generic.HashSet<IShootable>();
        foreach (var c in hits)
        {
            var target = c.GetComponentInParent<IShootable>();
            if (target != null && done.Add(target)) target.OnGrenadeHit();
        }
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, blastRadius);
    }
}
