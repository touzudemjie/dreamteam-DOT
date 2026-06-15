using UnityEngine;
using UnityEngine.Events;
[System.Serializable]
public class Dialogue
{
    public DialogueLine[] dialogueLines;
    //public SFX[] SFXs;
    //[System.Serializable]
    //public class SFX
    //{
    //    public float pitch;
    //}

}

[System.Serializable]
public class DialogueEffect
{
    public EffectType type;
    public float intensity;
    public float duration;
    public AnimationCurve curve;
    public float _sfxPitch = 1;
}

public enum EffectType
{
    CameraShake,
    CameraZoom,
    ScreenFlash,
    Vignette,
    Letterbox,
    PitchShift,
    SlowMotion,
    ParticleEffect,
}

[System.Serializable]
public class DialogueLine
{
    public DialogueEffect dialogueEffect;
    public string dialogueID;
    public string nextDialogueID;
    public string speaker;
    [TextArea(3, 10)]
    public string textContent;
    public Sprite sprite;
    public AudioClip music;
    public AudioClip talkSFX;
    [SerializeField] private float musicVolume;
    public float MusicVolume { get { return musicVolume; } set { if (value <= 0) musicVolume = 0; else musicVolume = value; } }
    public bool hasSpecialEffect;
    public DialogueChoice[] choices;

}
[System.Serializable]
public class DialogueChoice
{
    public DialogueCondition condition;
    public string failedConditionId;
    public bool saveDialogueLineIndex;
    public bool lockDialogueAssetIndex;
    public string choiceText;
    public int severity;
    public string nextDialogueID;
    public UnityEvent onChosen;
}

