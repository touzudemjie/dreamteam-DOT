using System;
using System.Collections;
using UnityEngine;
public class AudioManagerScript : MonoBehaviour
{
    public static AudioManagerScript Instance {  get; set; }
    [SerializeField] private AudioSource _musicSourceA;
    [SerializeField] private AudioSource _musicSourceB;
    [SerializeField] private AudioSource sfxSource;
    public event Action OnSfxEnd;
    public bool IsPlayingSfx3D { get; private set; }
    public bool IsPlayingSfx {  get; private set; }
    private float _sfxTime;
    private float _sfx3DTime;

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
        _activeMusicSource = _musicSourceA;
    }

    private void Start()
    {
        
    }
    public void PlayMusicTransitionally(AudioClip clip, float targetVolume = 1f)
    {
        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
        }
        _transitionCoroutine = StartCoroutine(TransitionMusic(clip, targetVolume));
    }
    private void Update()
    {
        InvokeSfx();
        CheckSfx3D();
    }
    void InvokeSfx()
    {
        if (IsPlayingSfx) 
        {
            _sfxTime -= Time.deltaTime;
            if (_sfxTime <= 0)
            {
                OnSfxEnd?.Invoke();
                IsPlayingSfx = false;
            }
        }
    }
    void CheckSfx3D()
    {
        if (IsPlayingSfx3D)
        {
            _sfx3DTime -= Time.deltaTime;
            if (_sfx3DTime <= 0)
            {
                IsPlayingSfx3D = false;
                _sfx3DTime = 0;
            }
        }
    }
    public void StopMusic()
    {
        if (_activeMusicSource != null)
        {
            if (_activeMusicSource.isPlaying)
            {
                _activeMusicSource.Stop();
            }
        }
    }
    private IEnumerator TransitionMusic(AudioClip clip, float targetVolume)
    {
        AudioSource outgoing = _activeMusicSource;
        AudioSource incoming = _activeMusicSource == _musicSourceA ? _musicSourceB : _musicSourceA;
        float startVolume = outgoing.volume;
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
       _activeMusicSource.clip = clip;
       _activeMusicSource.Play();
    }
    public void PlaySfx(AudioClip clip, bool skipPreviousClip = false)
    {
        if (!IsPlayingSfx || skipPreviousClip)
        {
            sfxSource.clip = clip;
            _sfxTime = clip.length;
            IsPlayingSfx = true;
            sfxSource.Play();
        }
    }
    public void StopSfx()
    {
        if (IsPlayingSfx)
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
        IsPlayingSfx3D = true;
        _sfx3DTime = clip.length;
        audioSource.Play();
        Destroy(tempAudio, clip.length);
    }
}
