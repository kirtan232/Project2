using UnityEngine;

/// <summary>Plays looping background music for the whole level.</summary>
public class MusicPlayer : MonoBehaviour
{
    public AudioClip music;
    [Range(0f, 1f)] public float volume = 0.35f;

    void Start()
    {
        var source = gameObject.AddComponent<AudioSource>();
        source.clip = music;
        source.loop = true;
        source.volume = volume;
        source.spatialBlend = 0f;
        source.Play();
    }
}
