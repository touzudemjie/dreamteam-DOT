using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class DialogueScript : MonoBehaviour, IInteractable
{
    [SerializeField] Dialogue[] dialogueAsset;
    [SerializeField] private Color _highlightColor;
    public bool HasDialogueEndedNaturally { get; private set; }
    private Color _defaultColor;
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
    public event Action<string> OnTextChanged;
    Dictionary<string, DialogueLine> dialogueDict = new Dictionary<string, DialogueLine>();
    string currentDialogueID;
    StringBuilder currentText = new StringBuilder();
    bool isTyping = false;
    public bool IsDialogueFinished { get; private set; } = true;
    float typeTimer = 0f;

    [SerializeField] private int _dialogueReferenceIndex;

    void Start()
    {
        currentDialogueID = dialogueAsset[dialogueAssetIndex].dialogueLines[0].dialogueID;
    }
    public void OnInteract()
    {
        if (IsDialogueFinished)
        {
            BuildDialogueDictionary();
            StartDialogue();
        }
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
        //DialogueMangerScript.instance.currentReference.dialogueObject.SetActive(false);
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
        foreach (DialogueLine line in dialogueAsset[dialogueAssetIndex].dialogueLines)
        {
            if (dialogueDict.ContainsKey(line.dialogueID))
                Debug.LogWarning($"Duplicate dialogueID: {line.dialogueID}");
            else
                dialogueDict.Add(line.dialogueID, line);
        }
    }
    [ContextMenu(nameof(StartDialogue))]
    public void StartDialogue()
    {
        Transform textTr = null;
        DialogueMangerScript.Instance.SetNextDialogueObject(_dialogueReferenceIndex);
        if (onlyText)
        {
            DialogueMangerScript.Instance.ActivateText(true);
        }
        else
        {
            DialogueMangerScript.Instance.ActivateAllReferences(true);
        }
        if (changeUIPosition)
        {
            textTr.localPosition = textPosition;
            textTr.gameObject.GetComponent<TextMeshProUGUI>().alignment = textAlignment;
        }
        ShowDialogueLine(currentDialogueID);
        OnStartDialogue?.Invoke();
        IsDialogueFinished = false;
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
        currentText.Clear();
        isTyping = true;
        typeTimer = 0f;
        SetLineReferences(line);
    }
    private void SetLineReferences(DialogueLine line)
    {
        DialogueMangerScript.Instance.SetLineReferences(line);
    }

    private IEnumerator StartWhenManagerReady()
    {
        yield return null;

    }
    void Update()
    {
        if (!IsDialogueFinished)
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
        foreach (Key continueKeyCode in DialogueMangerScript.Instance.continueKeys)
        {
            if (Keyboard.current[continueKeyCode].wasPressedThisFrame)
            {
                pressedContinueButton = true;
                break;
            }
        }
        if (line != null)
        {

            if ((pressedContinueButton && line.nextDialogueID == "END") || line.nextDialogueID == "END" && skipDialogue)
            {
                HasDialogueEndedNaturally = true;
                EndDialogue();
            }
            else if  ((pressedContinueButton && !IsDialogueFinished && !isTyping) || skipDialogue)
            {
                Debug.Log("YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY");
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
            else if((pressedContinueButton && !IsDialogueFinished && isTyping) || skipDialogue)
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
                string nextCharacter = fullText[currentText.Length].ToString();
                if (!DialogueMangerScript.Instance.TextFitsInTextLine(currentText.ToString() + fullText[currentText.Length]))
                {
                    Debug.Log(fullText[currentText.Length]);
                    currentText.Append("\n");
                    fullText += "\n";
                }
                currentText.Append(nextCharacter);
                DialogueMangerScript.Instance.ShowText(currentText.ToString());
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
                    IsDialogueFinished = true;
                    enabled = false;
                    DialogueMangerScript.Instance.ActivateAllReferences(false);
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
        if (IsDialogueFinished) return;
        if (onlyText)
        {
            DialogueMangerScript.Instance.ActivateText(false);
        }
        else
        {
            DialogueMangerScript.Instance.ActivateAllReferences(false);
        }
        dialogueAssetIndex++;
        dialogueAssetIndex = dialogueAssetIndex % dialogueAsset.Length;
        isTyping = false;
        IsDialogueFinished = true;
        currentDialogueID = dialogueAsset[dialogueAssetIndex].dialogueLines[0].dialogueID;
        skipDialogue = false;
        dialogueDict.Clear();
        OnEndDialogue?.Invoke();
    }

}
