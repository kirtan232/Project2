using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Armed guard driven by a NavMeshAgent.
/// Patrol : wander to random points inside the treasure box's protection zone.
/// Chase  : player is inside the zone, the treasure was stolen, or the guard was shot recently.
///          Chases with the agent and shoots when it has a clear line of sight.
/// Flee   : after all treasures are collected, run away from the player.
/// Dies after 3 gun hits or 1 grenade hit.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class Guard : MonoBehaviour, IShootable
{
    public enum State { Patrol, Chase, Flee, Dead }

    [Header("Setup")]
    public TreasureBox home;
    public int gunHitsToKill = 3;
    public Transform patrolCenter;         // optional; defaults to the treasure box
    public float patrolRadius = 0f;        // 0 = use the full protection radius

    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4.5f;
    public float fleeSpeed = 5.5f;
    public float patrolWaitTime = 1.5f;

    [Header("Combat")]
    public EnemyBullet bulletPrefab;
    public float attackRange = 14f;
    public float fireInterval = 1.4f;
    public float aimSpread = 0.8f;
    public float alertDuration = 6f;       // chase time after being shot outside the zone

    [Header("Audio")]
    public AudioClip shootClip;
    public AudioClip hitClip;
    public AudioClip deathClip;

    public State Current { get; private set; } = State.Patrol;

    NavMeshAgent agent;
    Transform player;
    PlayerHealth playerHealth;
    int hits;
    float nextShotTime, patrolWaitTimer, repathTimer, alertTimer;
    bool hasPatrolTarget;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null) player = playerHealth.transform;
        if (GameManager.Instance != null) GameManager.Instance.RegisterGuard();
        SetState(State.Patrol);
    }

    void Update()
    {
        if (Current == State.Dead || player == null) return;

        if (GameManager.Instance != null && GameManager.Instance.Ended)
        {
            agent.isStopped = true;
            return;
        }

        alertTimer -= Time.deltaTime;
        ChooseState();

        switch (Current)
        {
            case State.Patrol: UpdatePatrol(); break;
            case State.Chase: UpdateChase(); break;
            case State.Flee: UpdateFlee(); break;
        }
    }

    // ---- state selection ---------------------------------------------

    void ChooseState()
    {
        State wanted;
        if (GameManager.Instance != null && GameManager.Instance.AllTreasuresCollected)
            wanted = State.Flee;                                       // hunt phase: guards run
        else if (home != null && (home.Stolen || home.PlayerInside) || alertTimer > 0f)
            wanted = State.Chase;                                      // zone breached / treasure stolen
        else
            wanted = State.Patrol;                                     // back to patrol

        if (wanted != Current) SetState(wanted);
    }

    void SetState(State s)
    {
        Current = s;
        agent.isStopped = false;
        hasPatrolTarget = false;
        repathTimer = 0f;
        switch (s)
        {
            case State.Patrol: agent.speed = patrolSpeed; break;
            case State.Chase: agent.speed = chaseSpeed; break;
            case State.Flee: agent.speed = fleeSpeed; break;
        }
    }

    // ---- patrol ------------------------------------------------------

    void UpdatePatrol()
    {
        if (!hasPatrolTarget)
        {
            if (TryRandomZonePoint(out Vector3 p))
            {
                agent.SetDestination(p);
                hasPatrolTarget = true;
            }
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.3f)
        {
            patrolWaitTimer += Time.deltaTime;
            if (patrolWaitTimer >= patrolWaitTime)
            {
                patrolWaitTimer = 0f;
                hasPatrolTarget = false;
            }
        }
    }

    bool TryRandomZonePoint(out Vector3 result)
    {
        // Optional override lets two guards of one box patrol different parts of the zone
        // (e.g. one inside the house, one outside).
        Vector3 center = patrolCenter != null ? patrolCenter.position
                       : home != null ? home.transform.position : transform.position;
        float radius = patrolRadius > 0f ? patrolRadius
                     : home != null ? home.protectionRadius : 10f;
        for (int i = 0; i < 10; i++)
        {
            Vector2 r = Random.insideUnitCircle * radius;
            Vector3 candidate = center + new Vector3(r.x, 0f, r.y);
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = transform.position;
        return false;
    }

    // ---- chase + shoot -----------------------------------------------

    void UpdateChase()
    {
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        bool canShoot = toPlayer.magnitude <= attackRange && HasLineOfSight();

        if (canShoot)
        {
            agent.isStopped = true;                       // stand and fire
            if (toPlayer.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(toPlayer), 8f * Time.deltaTime);
            TryShoot();
        }
        else
        {
            agent.isStopped = false;
            repathTimer -= Time.deltaTime;
            if (repathTimer <= 0f)
            {
                agent.SetDestination(player.position);
                repathTimer = 0.25f;
            }
        }
    }

    bool HasLineOfSight()
    {
        Vector3 eye = transform.position + Vector3.up * 1.5f;
        Vector3 target = player.position + Vector3.up * 0.5f;
        if (Physics.Raycast(eye, target - eye, out RaycastHit hit, attackRange + 2f,
                ~0, QueryTriggerInteraction.Ignore))
            return hit.collider.GetComponentInParent<PlayerController>() != null;
        return false;
    }

    void TryShoot()
    {
        if (Time.time < nextShotTime || bulletPrefab == null) return;
        if (playerHealth != null && playerHealth.IsDead) return;
        nextShotTime = Time.time + fireInterval;

        Vector3 muzzle = transform.position + Vector3.up * 1.5f + transform.forward * 0.6f;
        Vector3 aim = player.position + Vector3.up * 0.5f
                      + Random.insideUnitSphere * aimSpread;
        EnemyBullet b = Instantiate(bulletPrefab, muzzle, Quaternion.identity);
        b.Fire(aim - muzzle);
        Sfx.Play(shootClip, muzzle);
    }

    // ---- flee --------------------------------------------------------

    void UpdateFlee()
    {
        repathTimer -= Time.deltaTime;
        if (repathTimer > 0f) return;
        repathTimer = 0.5f;

        Vector3 away = transform.position - player.position;
        away.y = 0f;
        if (away.sqrMagnitude < 0.01f) away = Random.insideUnitSphere;
        away.Normalize();

        // Try straight away from the player first, then fan out to the sides.
        float[] angles = { 0f, 40f, -40f, 80f, -80f, 120f, -120f };
        foreach (float a in angles)
        {
            Vector3 dir = Quaternion.Euler(0f, a, 0f) * away;
            Vector3 spot = transform.position + dir * 12f;
            if (NavMesh.SamplePosition(spot, out NavMeshHit hit, 4f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                return;
            }
        }
    }

    // ---- damage ------------------------------------------------------

    public void OnGunHit()
    {
        if (Current == State.Dead) return;
        hits++;
        Sfx.Play(hitClip, transform.position);
        Vfx.Sparks(transform.position + Vector3.up * 1.3f);
        alertTimer = alertDuration;       // being shot makes the guard chase back
        if (hits >= gunHitsToKill) Die();
    }

    public void OnGrenadeHit()
    {
        if (Current == State.Dead) return;
        Sfx.Play(hitClip, transform.position);
        Die();
    }

    void Die()
    {
        Current = State.Dead;
        Sfx.Play(deathClip, transform.position);
        agent.enabled = false;
        foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
        transform.Rotate(90f, 0f, 0f);    // fall over
        if (GameManager.Instance != null) GameManager.Instance.GuardKilled();
        Destroy(gameObject, 3f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
