using UnityEngine;
using System.Collections.Generic;

public class PlaySoundOnAnimatorStates : MonoBehaviour
{
    [System.Serializable]
    public class StateSound
    {
        [Tooltip("Nom exact de l'état dans le layer 0.")]
        public string stateName;

        [Tooltip("Liste des sons possibles. Un sera choisi aléatoirement.")]
        public List<AudioClip> clips = new List<AudioClip>();

        public float volume = 1f;

        [Tooltip("Délai avant de jouer le son (en secondes). 0 = immédiat.")]
        public float delay = 0f;

        [HideInInspector] public int stateHash;
        [HideInInspector] public bool hasPlayed;
        [HideInInspector] public float timer;
    }

    [Header("Animator Reference")]
    public Animator animator;

    [Header("States & Sounds")]
    public List<StateSound> states = new List<StateSound>();

    private AudioSource _audio;
    private int _currentHash = 0;

    void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogError("[PlaySoundOnAnimatorStates] Aucun Animator trouvé ou assigné.", this);
                enabled = false;
                return;
            }
        }

        _audio = GetComponent<AudioSource>();
        if (_audio == null)
            _audio = gameObject.AddComponent<AudioSource>();

        foreach (var s in states)
        {
            s.stateHash = Animator.StringToHash(s.stateName);
            s.hasPlayed = false;
            s.timer = 0f;
        }
    }

    void Update()
    {
        var info = animator.GetCurrentAnimatorStateInfo(0);
        int hash = info.shortNameHash;

        // Détection d'entrée dans un nouvel état
        if (hash != _currentHash)
        {
            _currentHash = hash;

            // Reset uniquement les entrées correspondant à cet état
            foreach (var s in states)
            {
                if (s.stateHash == hash)
                {
                    s.hasPlayed = false;
                    s.timer = 0f;
                }
            }
        }

        // Gestion des sons pour les entrées correspondant à l'état actif
        foreach (var s in states)
        {
            if (s.stateHash != hash)
                continue;

            if (s.hasPlayed)
                continue;

            if (s.delay <= 0f)
            {
                PlayRandomClip(s);
            }
            else
            {
                s.timer += Time.deltaTime;
                if (s.timer >= s.delay)
                    PlayRandomClip(s);
            }
        }
    }

    private void PlayRandomClip(StateSound s)
    {
        if (s.clips == null || s.clips.Count == 0)
            return;

        var clip = s.clips[Random.Range(0, s.clips.Count)];
        if (clip != null)
            _audio.PlayOneShot(clip, s.volume);

        s.hasPlayed = true;
    }
}
