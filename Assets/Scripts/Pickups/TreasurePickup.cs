using UnityEngine;

/// <summary>On the chest child of a TreasureBox. Touching it with the player steals the treasure.</summary>
[RequireComponent(typeof(Collider))]
public class TreasurePickup : MonoBehaviour
{
    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() == null) return;
        var box = GetComponentInParent<TreasureBox>();
        if (box != null) box.Collect();
    }
}
