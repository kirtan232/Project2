using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum WeaponType { None, Gun, Grenade }

/// <summary>
/// Player inventory + firing. Weapons start hidden and are revealed when picked up.
/// Left mouse fires the current weapon; Q / E switches between owned weapons.
/// </summary>
public class PlayerWeapons : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    public GameObject gunModel;        // child of the camera, hidden until the gun is picked up
    public GameObject grenadeModel;    // child of the camera, hidden until a grenade is picked up
    public GameObject grenadePrefab;
    public Light muzzleFlash;          // short point light at the gun barrel

    [Header("Gun")]
    public int bulletsPerPickup = 6;
    public float gunRange = 100f;
    public float fireCooldown = 0.35f;
    public LayerMask gunHitMask = ~0;

    [Header("Grenade")]
    public float throwForce = 14f;
    public float throwUpward = 3f;
    public float grenadeCooldown = 0.8f;

    [Header("Audio")]
    public AudioClip gunShotClip;
    public AudioClip grenadeThrowClip;

    public bool HasGun { get; private set; }
    public int Ammo { get; private set; }
    public int Grenades { get; private set; }
    public WeaponType Current { get; private set; } = WeaponType.None;

    public event Action Changed;

    InputAction fireAction, nextAction, prevAction;
    float nextFireTime;

    public bool InputEnabled { get; set; } = true;

    void Awake()
    {
        fireAction = new InputAction("Fire", InputActionType.Button, "<Mouse>/leftButton");
        nextAction = new InputAction("NextWeapon", InputActionType.Button, "<Keyboard>/e");
        prevAction = new InputAction("PrevWeapon", InputActionType.Button, "<Keyboard>/q");
        if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
        RefreshModels();
    }

    void OnEnable() { fireAction.Enable(); nextAction.Enable(); prevAction.Enable(); }
    void OnDisable() { fireAction.Disable(); nextAction.Disable(); prevAction.Disable(); }
    void OnDestroy() { fireAction.Dispose(); nextAction.Dispose(); prevAction.Dispose(); }

    void Update()
    {
        if (!InputEnabled) return;
        if (nextAction.WasPressedThisFrame() || prevAction.WasPressedThisFrame()) SwitchWeapon();
        if (fireAction.IsPressed() && Time.time >= nextFireTime) Fire();
    }

    // ---- pickups -------------------------------------------------------

    /// <summary>First gun pickup gives the gun + 6 bullets. There is only one gun in the world.</summary>
    public void PickUpGun()
    {
        HasGun = true;
        Ammo += bulletsPerPickup;
        if (Current == WeaponType.None) Current = WeaponType.Gun;
        RefreshModels();
    }

    public void AddAmmo(int amount)
    {
        Ammo += amount;
        Changed?.Invoke();
    }

    public void AddGrenades(int amount)
    {
        Grenades += amount;
        if (Current == WeaponType.None) Current = WeaponType.Grenade;
        RefreshModels();
    }

    // ---- switching / firing -------------------------------------------

    void SwitchWeapon()
    {
        if (HasGun && Grenades > 0)
            Current = Current == WeaponType.Gun ? WeaponType.Grenade : WeaponType.Gun;
        RefreshModels();
    }

    void Fire()
    {
        if (Current == WeaponType.Gun && HasGun && Ammo > 0) FireGun();
        else if (Current == WeaponType.Grenade && Grenades > 0) ThrowGrenade();
    }

    void FireGun()
    {
        nextFireTime = Time.time + fireCooldown;
        Ammo--;
        Sfx.Play(gunShotClip, cameraTransform.position);
        if (muzzleFlash != null) StartCoroutine(Flash());

        // The ray starts at the camera centre, so it hits what the crosshair points at.
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit,
                gunRange, gunHitMask, QueryTriggerInteraction.Ignore))
        {
            var target = hit.collider.GetComponentInParent<IShootable>();
            target?.OnGunHit();
        }
        Changed?.Invoke();
    }

    System.Collections.IEnumerator Flash()
    {
        muzzleFlash.enabled = true;
        yield return new WaitForSeconds(0.05f);
        muzzleFlash.enabled = false;
    }

    void ThrowGrenade()
    {
        nextFireTime = Time.time + grenadeCooldown;
        Grenades--;
        Sfx.Play(grenadeThrowClip, cameraTransform.position);

        Vector3 spawn = cameraTransform.position + cameraTransform.forward * 0.8f;
        GameObject g = Instantiate(grenadePrefab, spawn, Quaternion.identity);
        var rb = g.GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = cameraTransform.forward * throwForce + Vector3.up * throwUpward;

        if (Grenades <= 0) Current = HasGun ? WeaponType.Gun : WeaponType.None;
        RefreshModels();
    }

    void RefreshModels()
    {
        if (gunModel != null) gunModel.SetActive(HasGun && Current == WeaponType.Gun);
        if (grenadeModel != null) grenadeModel.SetActive(Grenades > 0 && Current == WeaponType.Grenade);
        Changed?.Invoke();
    }
}
