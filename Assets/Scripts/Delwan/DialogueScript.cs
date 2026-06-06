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
    public bool HasDialogueEndedNaturally { get; private set; }
    [SerializeField] int severityLine;
    [SerializeField] bool onlyText;
    [SerializeField] bool shouldSkipWithoutPressing;
    [SerializeField] bool changeUIPosition;
    [SerializeField] bool playOnlyOneLine;
    [SerializeField] Vector3 textPosition;
    [SerializeField] TextAlignmentOptions textAlignment;
    [SerializeField] float typeSpeed = 0.05f;
    private int _currentDialogueLineIndex;
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
            StartDialogue();
        }
    }
    public void ChangeDialogueID(string dialogueID)
    {
        currentDialogueID = dialogueID;
        StartDialogue();
    }
    public string GetNextDialogueID()
    {
        dialogueDict.TryGetValue(currentDialogueID, out DialogueLine dialogueLine);
        if (dialogueLine != null)
        {
            return dialogueLine.nextDialogueID;
        }
        else
        {
            Debug.LogWarning($"No DialogueLine with ID '{currentDialogueID}' found.");
            return null;
        }
    }
    public string GetCurrentDialogueID()
    {
        Debug.Log("Current DialogueID: " + currentDialogueID);
        return currentDialogueID;
    }
    public void EndDialogueAfterTyping(bool canEndDialogue)
    {
        playOnlyOneLine = canEndDialogue;
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
        BuildDialogueDictionary();
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
            //Changes Position and Alignment of the Dialogue Text, if specified
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
            if ((pressedContinueButton && line.nextDialogueID == "END" && !playOnlyOneLine) || line.nextDialogueID == "END" && skipDialogue && !playOnlyOneLine)
            {
                _currentDialogueLineIndex++;
                HasDialogueEndedNaturally = true;
                EndDialogue();
            }
            else if  ((pressedContinueButton && !IsDialogueFinished && !playOnlyOneLine) || skipDialogue)
            {
                _currentDialogueLineIndex++;
                currentDialogueID = line.nextDialogueID;
                dialogueDict.TryGetValue(currentDialogueID, out DialogueLine nextLine);
                if (nextLine != null)
                {
                    if(nextLine.music != null)
                    {
                        AudioManagerScript.Instance.PlayMusic(nextLine.music);
                    }
                }
                ShowDialogueLine(currentDialogueID);
            }
        }
        if (isTyping)
        {
            TypewriterTick();
        }
    }

    private void InputHandling()
    {

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
            if (line.talkSFX != null)
            {
                AudioManagerScript.Instance.PlaySfx(line.talkSFX);
            }
            typeTimer = typeSpeed;
            if (currentText.Length < fullText.Length)
            {
                if (fullText[currentText.Length] == '<')
                {
                    int i = currentText.Length;
                    while (i < fullText.Length -1 && fullText[i] != '>')
                    {
                        currentText.Append(fullText[i]);
                        i++;
                    }
                }
                char nextChar = fullText[currentText.Length];
                AppendNewLine(nextChar, line);
                currentText.Append(nextChar);
                DialogueMangerScript.Instance.ShowText(currentText.ToString());
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
                    _currentDialogueLineIndex++;
                    EndDialogue();
                }
            }

        }
    }

    private void AppendNewLine(char nextChar, DialogueLine line)
    {
        string fullText = line.textContent;
        if (nextChar != ' ' && nextChar != '\n')
        {
            bool isStartOfWord = currentText.Length == 0 ||
                                    currentText[currentText.Length - 1] == ' ' ||
                                    currentText[currentText.Length - 1] == '\n';
            if (isStartOfWord)
            {
                // Komplettes nächstes Wort ermitteln
                int wordStart = currentText.Length;
                int wordEnd = fullText.IndexOfAny(new char[] { ' ', '\n' }, wordStart);
                if (wordEnd < 0)
                {
                    wordEnd = fullText.Length;
                }
                string nextWord = fullText.Substring(wordStart, wordEnd - wordStart);

                // Aktuelle Zeile + Wort testen
                string lastLine = currentText.ToString();
                int lastNewline = lastLine.LastIndexOf('\n');
                string currentLine = lastNewline >= 0 ? lastLine.Substring(lastNewline + 1) : lastLine;
                if (!DialogueMangerScript.Instance.TextFitsInTextLine(currentLine + nextWord))
                {
                    // Newline vor dem Wort einfügen
                    currentText.Append('\n');
                    line.textContent = line.textContent.Insert(currentText.Length - 1, "\n");
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
        if (DialogueMangerScript.Instance != null)
        {
            if (onlyText)
            {
                DialogueMangerScript.Instance.ActivateText(false);
            }
            else
            {
                DialogueMangerScript.Instance.ActivateAllReferences(false);
            }
        }
        if (GetNextDialogueID() == "END")
        {
            dialogueAssetIndex++;
        }
        dialogueAssetIndex = dialogueAssetIndex % dialogueAsset.Length;
        _currentDialogueLineIndex = _currentDialogueLineIndex % dialogueAsset[dialogueAssetIndex].dialogueLines.Length;
        isTyping = false;
        IsDialogueFinished = true;
        currentDialogueID = dialogueAsset[dialogueAssetIndex].dialogueLines[_currentDialogueLineIndex].dialogueID;
        skipDialogue = false;
        dialogueDict.Clear();
        OnEndDialogue?.Invoke();
    }

}
