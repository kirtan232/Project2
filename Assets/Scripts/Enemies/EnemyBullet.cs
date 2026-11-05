using UnityEngine;

/// <summary>Guard projectile. Moves in a straight line; a hit on the player costs one life.</summary>
[RequireComponent(typeof(SphereCollider))]
public class EnemyBullet : MonoBehaviour
{
    public float speed = 25f;
    public float lifetime = 5f;

    Vector3 direction;

    public void Fire(Vector3 dir)
    {
        direction = dir.normalized;
        Destroy(gameObject, lifetime);
    }

    void Awake()
    {
        GetComponent<SphereCollider>().isTrigger = true;
        var rb = gameObject.AddComponent<Rigidbody>();   // needed so trigger events fire
        rb.isKinematic = true;
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;
        if (other.GetComponentInParent<Guard>() != null) return;   // don't hit guards

        var player = other.GetComponent<PlayerHealth>();
        if (player != null) player.TakeHit();
        Destroy(gameObject);
    }
}
