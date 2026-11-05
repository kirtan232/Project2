using UnityEngine;

/// <summary>Tiny null-safe helper so scripts can play optional sound effects.</summary>
public static class Sfx
{
    public static void Play(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip != null) AudioSource.PlayClipAtPoint(clip, position, volume);
    }
}

/// <summary>Anything a bullet or grenade can damage (guards).</summary>
public interface IShootable
{
    void OnGunHit();
    void OnGrenadeHit();
}
