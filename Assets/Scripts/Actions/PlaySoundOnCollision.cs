using UnityEngine;
using System.Collections.Generic;

public class PlaySoundOnCollision : MonoBehaviour
{
    [System.Serializable]
    public class MaterialSound
    {
        public PhysicsMaterial physicMaterial;
        public List<AudioClip> clips;
        public float volume;
        public float delay;

        [System.NonSerialized] public bool hasPlayed;
        [System.NonSerialized] public float timer;

        public MaterialSound()
        {
            physicMaterial = null;
            clips = new List<AudioClip>();
            volume = 1f;
            delay = 0f;
        }
    }

    [Header("Rigidbody Source")]
    public Rigidbody sourceRigidbody;

    [Header("Default Sound")]
    public List<AudioClip> defaultClips = new List<AudioClip>();
    public float defaultVolume = 1f;
    public float defaultDelay = 0f;

    [Header("Material Sounds")]
    public List<MaterialSound> materialSounds = new List<MaterialSound>();

    private AudioSource _audio;

    void Awake()
    {
        _audio = GetComponent<AudioSource>();
        if (_audio == null)
            _audio = gameObject.AddComponent<AudioSource>();

        if (sourceRigidbody == null)
        {
            Debug.LogError("[PlaySoundOnCollision] Aucun Rigidbody assigné.", this);
            return;
        }

        var relay = sourceRigidbody.GetComponent<CollisionRelay>();
        if (relay == null)
            relay = sourceRigidbody.gameObject.AddComponent<CollisionRelay>();

        relay.target = this;
    }

    // Appelé par le relay
    public void OnCollisionReceived(Collider col)
    {
        PhysicsMaterial mat = col.sharedMaterial;

        MaterialSound s = FindMaterialSound(mat);

        if (s != null)
            Play(s.clips, s.volume, s.delay);
        else
            Play(defaultClips, defaultVolume, defaultDelay);
    }

    private MaterialSound FindMaterialSound(PhysicsMaterial mat)
    {
        if (mat == null)
            return null;

        foreach (var s in materialSounds)
        {
            if (s.physicMaterial == mat)
                return s;
        }

        return null;
    }

    private void Play(List<AudioClip> clips, float volume, float delay)
    {
        if (clips == null || clips.Count == 0)
            return;

        if (delay <= 0f)
        {
            PlayRandomClip(clips, volume);
        }
        else
        {
            StartCoroutine(PlayDelayed(clips, volume, delay));
        }
    }

    private System.Collections.IEnumerator PlayDelayed(List<AudioClip> clips, float volume, float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayRandomClip(clips, volume);
    }

    private void PlayRandomClip(List<AudioClip> clips, float volume)
    {
        var clip = clips[Random.Range(0, clips.Count)];
        if (clip != null)
            _audio.PlayOneShot(clip, volume);
    }
}
