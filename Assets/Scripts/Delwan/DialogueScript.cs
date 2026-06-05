using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueScript : MonoBehaviour
{
    [SerializeField] Dialogue[] dialogueAsset;
    [SerializeField] private Color _highlightColor;
    public bool HasDialogueEndedNaturally {  get; private set; }
    private Color _defaultColor;
   // [SerializeField] GameObject dialogueObject;
    [SerializeField] int severityLine;
    [SerializeField] bool onlyText;
    [SerializeField] bool shouldSkipWithoutPressing;
    [SerializeField] bool changeUIPosition;
    [SerializeField] bool playOnlyOneLine;
    [SerializeField] Vector3 textPosition;
    [SerializeField] TextAlignmentOptions textAlignment;
    [SerializeField] float typeSpeed = 0.05f;
    int dialogueAssetIndex;
    bool skipDialogue;

    public event Action OnStartDialogue;
    public event Action OnWhileDialogue;
    public event Action OnEndDialogue;
    Dictionary<string, DialogueLine> dialogueDict;
    string currentDialogueID;
    StringBuilder currentText = new StringBuilder();
    bool isTyping = false;
    private bool isDialogueFinished = true;
    public bool IsDialogueFinished { get => isDialogueFinished; private set => isDialogueFinished = value; }
    float typeTimer = 0f;
    [SerializeField] KeyCode[] dialogueButtons;
    public event Action<string> OnTextChanged;

    void Start()
    {
        currentDialogueID = dialogueAsset[dialogueAssetIndex].dialogueLines[0].dialogueID;
        BuildDialogueDictionary();
    }
    public void ChangeDialogueID(string dialogueID)
    {
        currentDialogueID = dialogueID;
        StartDialogue();
    }
    public string GetCurrentDialogueID()
    {
        return currentDialogueID;
    }
    public void EndDialogueAfterTyping(bool canEndDialogue)
    {
        playOnlyOneLine = canEndDialogue;
    }
    public void CloseCanvas()
    {
        DialogueMangerScript.instance.currentReference.dialogueObject.SetActive(false);
        typeTimer = typeSpeed;
    }
    public bool ReachedEnd()
    {
        dialogueDict.TryGetValue(currentDialogueID,out DialogueLine dialogueLine);
        if(dialogueLine != null)
        {
            return dialogueLine.nextDialogueID.ToUpper() == "END";
        }
        else
        {
            return false;
        }
    }
    void BuildDialogueDictionary()
    {
        dialogueDict = new Dictionary<string, DialogueLine>();
        foreach (Dialogue asset in dialogueAsset)
        {
            foreach (DialogueLine line in asset.dialogueLines)
            {
                if (dialogueDict.ContainsKey(line.dialogueID))
                    Debug.LogWarning($"Duplicate dialogueID: {line.dialogueID} in Asset: {asset.dialogueLines[0].dialogueID}");
                else
                    dialogueDict.Add(line.dialogueID, line);
            }
        }
    }
    [ContextMenu(nameof(StartDialogue))]
    public void StartDialogue()
    {
        StartCoroutine(StartWhenManagerReady());
    }
    public void SetNextLine()
    {
        if (dialogueDict.TryGetValue(currentDialogueID, out DialogueLine dialogueLine))
        {
            currentDialogueID = dialogueLine.nextDialogueID;
            currentText.Clear();
            if (currentDialogueID.ToUpper() == "END")
            {
                EndDialogue();
            }
        }
    }
    void ShowDialogueLine(string dialogueID)
    {
        if (!dialogueDict.TryGetValue(dialogueID, out DialogueLine line))
        {
         //   Debug.LogWarning($"no Dialogueline with ID '{dialogueID}' found.");
            //EndDialogue();
            return;
        }
        DialogueMangerScript.instance.currentReference.textBox.text = "";
        currentText.Clear();
        SetDialogueReferences(line);
        isTyping = true;
        typeTimer = 0f;
    }
    private IEnumerator StartWhenManagerReady()
    {
        yield return new WaitUntil(() => DialogueMangerScript.instance != null);

        ShowDialogueLine(currentDialogueID);
        DialogueMangerScript.instance.currentReference.dialogueObject.SetActive(true);
        Transform textTr = null;
        if (onlyText)
        {
            foreach (Transform child in DialogueMangerScript.instance.currentReference.dialogueObject.transform)
            {
                if (child.gameObject != DialogueMangerScript.instance.currentReference.textBox.gameObject)
                {
                    child.gameObject.SetActive(false);
                }
                textTr = child;
            }
        }
        if (changeUIPosition)
        {
            textTr.localPosition = textPosition;
            textTr.gameObject.GetComponent<TextMeshProUGUI>().alignment = textAlignment;
        }
        OnStartDialogue?.Invoke();
        isDialogueFinished = false;
    }
    void Update()
    {
        if (!isDialogueFinished)
        {
            DialogueCheck();
        }
    }
    void DialogueCheck()
    {
        //if (GameManagerScript.instance.IsMenuOn)
        //{
        //    return;
        //}
        dialogueDict.TryGetValue(currentDialogueID, out DialogueLine line);
        bool pressedContinueButton = false;
        //foreach (KeyCode continueKeyCode in dialogueButtons)
        //{
        //    if (Input.GetKeyDown(continueKeyCode))
        //    {
        //        pressedContinueButton = true;
        //        break;
        //    }
        //}
        if (line != null)
        {
            if ((pressedContinueButton && !isDialogueFinished && !isTyping) || skipDialogue)
            {
                currentDialogueID = line.nextDialogueID;
                dialogueDict.TryGetValue(currentDialogueID, out DialogueLine nextLine);
                if (nextLine != null)
                {
                    if(nextLine.music != null)
                    {
                        //AudioManagerScript.Instance.PlayMusic(nextLine.music);
                    }
                }
                ShowDialogueLine(currentDialogueID);

            }
            else if((pressedContinueButton && !isDialogueFinished && isTyping) || skipDialogue)
            {
                currentDialogueID = line.nextDialogueID;
                dialogueDict.TryGetValue(currentDialogueID, out DialogueLine nextLine);
                if (nextLine != null)
                {
                    if (nextLine.music != null)
                    {
                      //  AudioManagerScript.Instance.PlayMusic(nextLine.music);
                    }
                }
                ShowDialogueLine(currentDialogueID);
            }
            if ((pressedContinueButton) && line.nextDialogueID == "END" || line.nextDialogueID == "END" && skipDialogue)
            {
                HasDialogueEndedNaturally = true;
                EndDialogue();
            }
        }
        if (isTyping)
            TypewriterTick();
    }
    void TypewriterTick()
    {
        if (!dialogueDict.TryGetValue(currentDialogueID, out DialogueLine line))
        {
            Debug.Log("There is no line " + currentDialogueID);
            return;
        }
        OnWhileDialogue?.Invoke();
        skipDialogue = false;
        string fullText = line.textContent;
        typeTimer -= Time.deltaTime;
        if (typeTimer <= 0)
        {
           // AudioManagerScript.Instance.PlaySfx(AudioManagerScript.Instance.dialogueSound);
            typeTimer = typeSpeed;
            if (currentText.Length < fullText.Length)
            {
                if (fullText[currentText.Length] == '<')
                {
                    while (fullText[currentText.Length] != '>')
                    {
                        currentText.Append(fullText[currentText.Length]);
                    }
                }
                currentText.Append(fullText[currentText.Length]);
                DialogueMangerScript.instance.currentReference.textBox.text = currentText.ToString();
                //if (line.audioClip != null)
                 //   AudioManagerScript.Instance.PlayDialogue(line.audioClip, line.AudioVolume, 1);
            }
            else
            {
                isTyping = false;
                if (shouldSkipWithoutPressing)
                {
                    skipDialogue = true;
                }
                if (playOnlyOneLine)
                {
                    if (line.nextDialogueID == "END")
                    {
                        EndDialogue();
                        return;
                    }
                    isDialogueFinished = true;
                    DialogueMangerScript.instance.currentReference.dialogueObject.SetActive(false);
                    enabled = false;
                    OnEndDialogue?.Invoke();
                }
            }

        }
    }
    private void OnDestroy()
    {
        EndDialogue();
    }
    public void EndDialogue()
    {
        if (isDialogueFinished) return;
        if (onlyText)
        {
            foreach (Transform child in  DialogueMangerScript.instance.currentReference.dialogueObject.transform)
            {
                child.gameObject.SetActive(true);
            }
        }
        dialogueAssetIndex++;
        if (dialogueAssetIndex >= dialogueAsset.Length)
        {
            dialogueAssetIndex = 0;
        }
        isTyping = false;
        isDialogueFinished = true;
        currentDialogueID = dialogueAsset[dialogueAssetIndex].dialogueLines[0].dialogueID;
        skipDialogue = false;
        if(DialogueMangerScript.instance != null)
        {
            DialogueMangerScript.instance.currentReference.dialogueObject.SetActive(false);
        }
        OnEndDialogue?.Invoke();
    }
    void SetDialogueReferences(DialogueLine line)
    {
        if (DialogueMangerScript.instance.currentReference.dialogueSprite != null)
        {
            DialogueMangerScript.instance.currentReference.dialogueSprite.sprite = line.sprite;
        }
        if (DialogueMangerScript.instance.currentReference.nameText != null) 
        {
            DialogueMangerScript.instance.currentReference.nameText.text = line.speaker;
        }
    }
}
