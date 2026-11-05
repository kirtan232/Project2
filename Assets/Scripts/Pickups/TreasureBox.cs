using UnityEngine;

/// <summary>
/// A treasure box plus its invisible protection zone.
/// Put this on an empty root object. It owns a trigger SphereCollider (the zone) and a child
/// "chest" object with a TreasurePickup. The root stays alive after the chest is stolen so
/// guards can still read Stolen.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class TreasureBox : MonoBehaviour
{
    public float protectionRadius = 15f;
    public GameObject chest;
    public AudioClip collectClip;

    public bool PlayerInside { get; private set; }
    public bool Stolen { get; private set; }

    void Awake()
    {
        var zone = GetComponent<SphereCollider>();
        zone.isTrigger = true;
        zone.radius = protectionRadius;
    }

    // Zone callbacks: the guards read PlayerInside to decide between patrol and chase.
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null) PlayerInside = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null) PlayerInside = false;
    }

    public void Collect()
    {
        if (Stolen) return;
        Stolen = true;
        Sfx.Play(collectClip, transform.position);
        if (chest != null) Destroy(chest);
        if (GameManager.Instance != null) GameManager.Instance.TreasureCollected();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, protectionRadius);
    }
}
