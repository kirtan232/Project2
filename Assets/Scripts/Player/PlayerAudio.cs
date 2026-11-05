using UnityEngine;

/// <summary>Player footsteps (walk / run) and the "you are hit" sound.</summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerAudio : MonoBehaviour
{
    public AudioClip[] footstepClips;
    public AudioClip hitClip;
    public float walkStepInterval = 0.5f;
    public float runStepInterval = 0.3f;
    [Range(0f, 1f)] public float footstepVolume = 0.5f;

    PlayerController controller;
    PlayerHealth health;
    AudioSource source;
    float stepTimer;

    void Awake()
    {
        controller = GetComponent<PlayerController>();
        health = GetComponent<PlayerHealth>();
        source = gameObject.AddComponent<AudioSource>();
        source.spatialBlend = 0f;   // 2D: the player always hears their own sounds
    }

    void OnEnable()
    {
        if (health != null) health.Hit += OnHit;
    }

    void OnDisable()
    {
        if (health != null) health.Hit -= OnHit;
    }

    void OnHit()
    {
        if (hitClip != null) source.PlayOneShot(hitClip);
    }

    void Update()
    {
        if (!controller.InputEnabled || !controller.IsMoving || !controller.IsGrounded
            || footstepClips == null || footstepClips.Length == 0)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer -= Time.deltaTime;
        if (stepTimer <= 0f)
        {
            source.PlayOneShot(footstepClips[Random.Range(0, footstepClips.Length)], footstepVolume);
            stepTimer = controller.IsRunning ? runStepInterval : walkStepInterval;
        }
    }
}
