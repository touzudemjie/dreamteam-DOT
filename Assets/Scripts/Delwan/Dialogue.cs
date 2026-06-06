using UnityEngine;
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
    public DialogueLine severeLine;
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
    [SerializeField] private float audioVolume;
    public float AudioVolume { get { return audioVolume; } set { if (value <= 0) audioVolume = 0; else audioVolume = value; } }
    public bool hasSpecialEffect;
    public DialogueChoice[] choices = new DialogueChoice[3];

}
[System.Serializable]
public struct DialogueChoice
{
    public string choiceText;
    public int severity;
    public string nextDialogueID;
}

