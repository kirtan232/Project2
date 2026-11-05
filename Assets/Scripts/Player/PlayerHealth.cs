using System;
using UnityEngine;

/// <summary>Tracks the player's lives. One hit = one life. Health kits restore one life each.</summary>
public class PlayerHealth : MonoBehaviour
{
    public int maxLives = 3;
    public int Lives { get; private set; }

    public event Action<int> LivesChanged;
    public event Action Died;
    public event Action Hit;

    public bool IsDead => Lives <= 0;

    void Awake()
    {
        Lives = maxLives;
    }

    void Start()
    {
        LivesChanged?.Invoke(Lives);
    }

    public float invulnerableTime = 1f;   // brief grace period so one burst doesn't cost all lives
    float lastHitTime = -100f;

    public void TakeHit()
    {
        if (IsDead || Time.time - lastHitTime < invulnerableTime) return;
        lastHitTime = Time.time;
        Lives--;
        Hit?.Invoke();
        LivesChanged?.Invoke(Lives);
        if (Lives <= 0) Died?.Invoke();
    }

    /// <summary>Restores one life. Returns false if already at max (so the kit isn't consumed).</summary>
    public bool Heal()
    {
        if (IsDead || Lives >= maxLives) return false;
        Lives++;
        LivesChanged?.Invoke(Lives);
        return true;
    }
}
