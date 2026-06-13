using UnityEngine;
using UnityEngine.Events;
[System.Serializable]
public class Dialogue
{
    public DialogueLine[] dialogueLines;
    [TextArea(3, 10)]
    public SFX[] SFXs;
    [System.Serializable]
    public class SFX
    {
        public float pitch;
    }

}
[System.Serializable]
public class DialogueLine
{
    public string dialogueID;
    //public bool hasDecision;
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
    public string choiceText;
    public int severity;
    public string nextDialogueID;
    public UnityEvent onChosen;
}

