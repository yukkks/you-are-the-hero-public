using UnityEngine;
using UnityEngine.AI;

// All sound in one place: background music, café room tone, footsteps that
// follow the hero's actual movement, and one-shot chimes fired by the
// interaction scripts. WebGL unmutes audio on the first user click, which is
// always the player's first move command — nothing special needed.
public class GameAudio : MonoBehaviour
{
    public static GameAudio Instance { get; private set; }

    [Header("Loops")]
    public AudioClip music;
    public AudioClip roomTone;
    public AudioClip footsteps;

    [Header("One-shots")]
    public AudioClip greetChime;
    public AudioClip photoShimmer;
    public AudioClip completeJingle;

    [Header("Mix")]
    [Range(0f, 1f)] public float musicVolume = 0.4f;
    [Range(0f, 1f)] public float roomToneVolume = 0.5f;
    [Range(0f, 1f)] public float footstepsVolume = 0.55f;
    [Range(0f, 1f)] public float sfxVolume = 0.9f;

    AudioSource musicSrc, roomSrc, stepsSrc, sfxSrc;
    NavMeshAgent hero;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        musicSrc = MakeSource(music, true, musicVolume);
        roomSrc = MakeSource(roomTone, true, roomToneVolume);
        stepsSrc = MakeSource(footsteps, true, footstepsVolume);
        sfxSrc = MakeSource(null, false, sfxVolume);
        if (stepsSrc) stepsSrc.Pause();
    }

    void Start()
    {
        var mover = FindObjectOfType<ClickToMove>();
        if (mover) hero = mover.GetComponent<NavMeshAgent>();
        if (GameProgress.Instance != null)
            GameProgress.Instance.OnAllComplete.AddListener(() => PlayOneShot(completeJingle));
    }

    AudioSource MakeSource(AudioClip clip, bool loop, float volume)
    {
        var src = gameObject.AddComponent<AudioSource>();
        src.clip = clip;
        src.loop = loop;
        src.volume = volume;
        src.playOnAwake = false;
        src.spatialBlend = 0f;
        if (clip != null && loop) src.Play();
        return src;
    }

    void Update()
    {
        if (hero == null || stepsSrc == null || stepsSrc.clip == null) return;
        bool moving = hero.velocity.magnitude > 0.4f;
        if (moving && !stepsSrc.isPlaying) stepsSrc.UnPause();
        else if (!moving && stepsSrc.isPlaying) stepsSrc.Pause();
    }

    void PlayOneShot(AudioClip clip)
    {
        if (clip != null && sfxSrc != null) sfxSrc.PlayOneShot(clip, sfxVolume);
    }

    public static void PlayGreet() { if (Instance) Instance.PlayOneShot(Instance.greetChime); }
    public static void PlayPhoto() { if (Instance) Instance.PlayOneShot(Instance.photoShimmer); }
}
