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

    AudioSource musicSrc, roomSrc, stepsSrc, sfxSrc, voiceSrc;
    NavMeshAgent hero;
    Coroutine duck;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        musicSrc = MakeSource(music, true, musicVolume);
        roomSrc = MakeSource(roomTone, true, roomToneVolume);
        stepsSrc = MakeSource(footsteps, true, footstepsVolume);
        sfxSrc = MakeSource(null, false, sfxVolume);
        voiceSrc = MakeSource(null, false, 1f);
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

    float retryTimer;

    void Update()
    {
        // Self-heal the ambience loops: if the audio device hiccupped (editor
        // FMOD re-init, browser autoplay unlock timing), quietly try again.
        retryTimer += Time.deltaTime;
        if (retryTimer > 2f)
        {
            retryTimer = 0f;
            if (musicSrc != null && musicSrc.clip != null && !musicSrc.isPlaying) musicSrc.Play();
            if (roomSrc != null && roomSrc.clip != null && !roomSrc.isPlaying) roomSrc.Play();
        }

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
    public static void PlayClip(AudioClip clip) { if (Instance) Instance.PlayOneShot(clip); }

    // A colleague speaks: play their voice and duck the music underneath it.
    public static void PlayVoice(AudioClip clip)
    {
        if (Instance == null || clip == null || Instance.voiceSrc == null) return;
        Instance.voiceSrc.Stop();
        Instance.voiceSrc.clip = clip;
        Instance.voiceSrc.Play();
        if (Instance.duck != null) Instance.StopCoroutine(Instance.duck);
        Instance.duck = Instance.StartCoroutine(Instance.DuckMusic(clip.length));
    }

    System.Collections.IEnumerator DuckMusic(float voiceLength)
    {
        float t = 0f;
        while (t < 0.4f)
        {
            t += Time.deltaTime;
            SetBedVolume(Mathf.Lerp(1f, 0.25f, t / 0.4f));
            yield return null;
        }
        yield return new WaitForSeconds(Mathf.Max(0f, voiceLength - 0.4f));
        t = 0f;
        while (t < 1.2f)
        {
            t += Time.deltaTime;
            SetBedVolume(Mathf.Lerp(0.25f, 1f, t / 1.2f));
            yield return null;
        }
        SetBedVolume(1f);
        duck = null;
    }

    void SetBedVolume(float mul)
    {
        if (musicSrc != null) musicSrc.volume = musicVolume * mul;
        if (roomSrc != null) roomSrc.volume = roomToneVolume * mul;
    }
}
