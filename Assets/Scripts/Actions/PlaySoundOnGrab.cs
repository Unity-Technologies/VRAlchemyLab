using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PlaySoundOnGrab : MonoBehaviour
{
    [System.Serializable]
    public class SoundSet
    {
        public List<AudioClip> clips = new List<AudioClip>();
        public float volume = 1f;
        public float delay = 0f;
    }

    [Header("Grab Sound")]
    public SoundSet grabSound = new SoundSet();

    [Header("Release Sound")]
    public SoundSet releaseSound = new SoundSet();

    private AudioSource _audio;
    private XRGrabInteractable _grab;

    void Awake()
    {
        _audio = GetComponent<AudioSource>();
        if (_audio == null)
            _audio = gameObject.AddComponent<AudioSource>();

        // Trouve automatiquement le XRGrabInteractable sur le parent
        _grab = GetComponentInParent<XRGrabInteractable>();
        if (_grab == null)
        {
            Debug.LogError("PlaySoundOnGrab : Aucun XRGrabInteractable trouvé dans les parents.", this);
            return;
        }

        _grab.selectEntered.AddListener(OnGrab);
        _grab.selectExited.AddListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        Play(grabSound);
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        Play(releaseSound);
    }

    private void Play(SoundSet s)
    {
        if (s.clips == null || s.clips.Count == 0)
            return;

        if (s.delay <= 0f)
        {
            PlayRandom(s.clips, s.volume);
        }
        else
        {
            StartCoroutine(PlayDelayed(s));
        }
    }

    private System.Collections.IEnumerator PlayDelayed(SoundSet s)
    {
        yield return new WaitForSeconds(s.delay);
        PlayRandom(s.clips, s.volume);
    }

    private void PlayRandom(List<AudioClip> clips, float volume)
    {
        var clip = clips[Random.Range(0, clips.Count)];
        if (clip != null)
            _audio.PlayOneShot(clip, volume);
    }
}
