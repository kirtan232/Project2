using UnityEngine;

public enum PickupType { Gun, Ammo, Grenade, HealthKit }

/// <summary>
/// Walk-over pickup (trigger collider). The player collects it by touching it.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Pickup : MonoBehaviour
{
    public PickupType type;
    public int amount = 1;
    public AudioClip pickupClip;
    public float spinSpeed = 90f;

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out PlayerController _)) return;

        bool consumed = true;
        switch (type)
        {
            case PickupType.Gun:
                other.GetComponent<PlayerWeapons>().PickUpGun();
                break;
            case PickupType.Ammo:
                other.GetComponent<PlayerWeapons>().AddAmmo(amount);
                break;
            case PickupType.Grenade:
                other.GetComponent<PlayerWeapons>().AddGrenades(amount);
                break;
            case PickupType.HealthKit:
                // Don't waste the kit if the player is already at full lives.
                consumed = other.GetComponent<PlayerHealth>().Heal();
                break;
        }

        if (!consumed) return;
        Sfx.Play(pickupClip, transform.position);
        Destroy(gameObject);
    }
}
