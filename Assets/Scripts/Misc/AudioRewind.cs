using System;
using UnityEngine;

/// <summary>
/// AudioRewind unterstützt TimeRewind-Systeme, indem es den Zustand eines AudioSource
/// speichert und bei Rewind-Events wiederherstellt.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioRewind : MonoBehaviour
{
    [Header("Verhalten")]
    [Tooltip("Audio beim Rewind-Start stoppen (bei Rewind-Ende neu an der gespeicherten Zeit starten).")]
    [SerializeField] private bool _stopOnReverseStart = true;

    [Tooltip("Während Rewind das Audio aktiv zurückspulen (Frame-by-Frame).")]
    [SerializeField] private bool _rewindAudioDuringReverse = true;

    [Tooltip("Wie viel Zeit pro Frame zurückgespult wird (in Sekunden).")]
    [SerializeField] private float _rewindStepSize = 0.016f;

    [Tooltip("Wenn Audio während Rewind nicht gestoppt wird, hier nur Zeit zurücksetzen beim Rewind-Ende.")]
    [SerializeField] private bool _playAudioWhileRewinding = false;

    private AudioSource _audioSource;

    // Zustand zum Zeitpunkt des Rewind-Starts
    private float _audioTimeAtReverseStart;
    private bool _wasPlayingAtReverseStart;
    private float _pitchAtReverseStart;
    private float _volumeAtReverseStart;
    private bool _wasLoopingAtReverseStart;

    // Status
    private bool _isRewinding;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        // Eigener TimeReverse auf demselben GameObject suchen
        var timeReverse = GetComponent<TimeReverse>();
        if (timeReverse != null)
        {
            timeReverse.OnReverseStart += OnReverseStart;
            timeReverse.OnReverseEnd += OnReverseEnd;
            timeReverse.OnReverseStep += OnReverseStep;
        }
    }

    private void OnDisable()
    {
        var timeReverse = GetComponent<TimeReverse>();
        if (timeReverse != null)
        {
            timeReverse.OnReverseStart -= OnReverseStart;
            timeReverse.OnReverseEnd -= OnReverseEnd;
            timeReverse.OnReverseStep -= OnReverseStep;
        }
    }

    /// <summary>
    /// Wird beim Start von Rewind aufgerufen.
    /// Speichert den aktuellen Zustand und stoppt/pausiert ggf. das Audio.
    /// </summary>
    public void OnReverseStart()
    {
        _isRewinding = true;

        // Zustand speichern
        _audioTimeAtReverseStart = _audioSource.time;
        _wasPlayingAtReverseStart = _audioSource.isPlaying;
        _pitchAtReverseStart = _audioSource.pitch;
        _volumeAtReverseStart = _audioSource.volume;
        _wasLoopingAtReverseStart = _audioSource.loop;

        if (_stopOnReverseStart)
        {
            // Audio stoppen, um Rückwärts-Effekte zu vermeiden
            _audioSource.Stop();
        }
    }

    /// <summary>
    /// Wird bei jedem Rewind-Frame aufgerufen (falls TimeReverse OnReverseStep sendet).
    /// Hier wird das Audio aktiv zurückgespult, falls gewünscht.
    /// </summary>
    public void OnReverseStep()
    {
        if (!_isRewinding)
            return;

        if (!_rewindAudioDuringReverse)
            return;

        // Wenn Audio nicht played, aber wir wollen zurückspulen:
        // Zeit einfach reduzieren, egal ob played oder nicht.
        float newTime = _audioSource.time - _rewindStepSize;
        if (newTime < 0f)
            newTime = 0f;

        _audioSource.time = newTime;

        // Optional: wenn Audio während Rewind laufen soll (ohne Ton)
        // Hier: selbst wenn not playing, setzen wir time; bei Play wird es sichtbar.
    }

    /// <summary>
    /// Wird beim Ende von Rewind aufgerufen.
    /// Stellt den Zustand wieder her und startet ggf. das Audio neu.
    /// </summary>
    public void OnReverseEnd()
    {
        _isRewinding = false;

        // Zeit zurücksetzen
        _audioSource.time = _audioTimeAtReverseStart;

        // Pitch / Volume / Loop wiederherstellen
        _audioSource.pitch = _pitchAtReverseStart;
        _audioSource.volume = _volumeAtReverseStart;
        _audioSource.loop = _wasLoopingAtReverseStart;

        // Play-Status wiederherstellen
        if (_wasPlayingAtReverseStart && !_stopOnReverseStart)
        {
            // Audio war während Rewind nicht gestoppt, einfach weiterlaufen lassen
            if (!_audioSource.isPlaying)
                _audioSource.Play();
        }
        else if (_wasPlayingAtReverseStart)
        {
            // Audio wurde gestoppt, jetzt neu an der gespeicherten Zeit starten
            _audioSource.Play();
        }
        else
        {
            // Audio war nicht playing, einfach so lassen
        }
    }

    /// <summary>
    /// Manuelle Hilfs-Methode, falls du kein TimeReverse benutzt,
    /// sondern das Rewind selbst steuerst.
    /// </summary>
    public void StartRewind()
    {
        OnReverseStart();
    }

    public void EndRewind()
    {
        OnReverseEnd();
    }

    public void StepRewind()
    {
        OnReverseStep();
    }
}
