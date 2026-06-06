using System;
using System.Collections;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class AudioManagerScript : MonoBehaviour
{
    public static AudioManagerScript Instance {  get; set; }
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public event Action OnSfxEnd;
    public bool isPlayingSfx3D;
    bool isPlayingSfx;
    float sfxTime;
    float sfx3DTime;

    [SerializeField] private AudioSource _musicSourceA;
    [SerializeField] private AudioSource _musicSourceB;
    [SerializeField] private float _transitionDuration = 1f;

    private AudioSource _activeMusicSource;
    private Coroutine _transitionCoroutine;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }

    private void Start()
    {
        
    }
    public void PlayMusicTransitionally(AudioClip clip)
    {
        if (_transitionCoroutine != null)
            StopCoroutine(_transitionCoroutine);

        _transitionCoroutine = StartCoroutine(TransitionMusic(clip));
    }
    private void Update()
    {
        InvokeSfx();
        CheckSfx3D();
    }
    void InvokeSfx()
    {
        if (isPlayingSfx) 
        {
            sfxTime -= Time.deltaTime;
            if (sfxTime <= 0)
            {
                OnSfxEnd?.Invoke();
                isPlayingSfx = false;
            }
        }
    }
    void CheckSfx3D()
    {
        if (isPlayingSfx3D)
        {
            sfx3DTime -= Time.deltaTime;
            if (sfx3DTime <= 0)
            {
                isPlayingSfx3D = false;
                sfx3DTime = 0;
            }
        }
    }

    private IEnumerator TransitionMusic(AudioClip clip)
    {
        AudioSource outgoing = _activeMusicSource;
        AudioSource incoming = _activeMusicSource == _musicSourceA ? _musicSourceB : _musicSourceA;

        float startVolume = outgoing.volume;
        float targetVolume = startVolume; // neuer clip soll gleich laut werden

        incoming.clip = clip;
        incoming.volume = 0f;
        incoming.Play();

        float elapsed = 0f;
        while (elapsed < _transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _transitionDuration;

            outgoing.volume = Mathf.Lerp(startVolume, 0f, t);
            incoming.volume = Mathf.Lerp(0f, targetVolume, t);

            yield return null;
        }

        outgoing.volume = 0f;
        outgoing.Stop();
        incoming.volume = targetVolume;

        _activeMusicSource = incoming;
        _transitionCoroutine = null;
    }
    public void PlayMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.Play();
    }
    public void PlaySfx(AudioClip clip, bool skipPreviousClip = false)
    {
        if (!isPlayingSfx || skipPreviousClip)
        {
            sfxSource.clip = clip;
            sfxTime = clip.length;
            isPlayingSfx = true;
            sfxSource.Play();

        }
    }
    public void StopSfx()
    {
        if (isPlayingSfx)
        {
            sfxSource.Stop();
        }
    }
    public void PlaySfx3D(AudioClip clip, Vector3 position, float volume = 1f)
    {
        GameObject tempAudio = new GameObject("TempAudio");
        tempAudio.transform.position = position;
        AudioSource audioSource = tempAudio.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.spatialBlend = 1f;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 12f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        isPlayingSfx3D = true;
        sfx3DTime = clip.length;
        audioSource.Play();
        Destroy(tempAudio, clip.length);
    }
}
