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

    [SerializeField] private DialogueNode[] dialogueNodes;
    [SerializeField] Dialogue[] dialogueAsset;
    [SerializeField] public DialogueLine[] _severeLines;
    [SerializeField] public DialogueLine[] _crossedSevereLines;
    public bool HasDialogueEndedNaturally { get; private set; }
    [SerializeField] private int _severityLine;
    [SerializeField] private int _NPCSeverityScore;
    [SerializeField] private bool _onlyText;
    [SerializeField] private bool _shouldSkipWithoutPressing;
    [SerializeField] private bool _changeUIPosition;
    [SerializeField] private bool _playOnlyOneLine;
    [SerializeField] private Vector3 _textPosition;
    [SerializeField] private TextAlignmentOptions _textAlignment;
    [SerializeField] private float _typeSpeed = 0.05f;
    private int _currentDialogueLineIndex;
    private int _dialogueAssetIndex;
    private bool _skipDialogue;
    public event Action OnStartDialogue;
    public event Action<string> OnTextChanged;
    public event Action OnEndDialogue;
    private Dictionary<string, DialogueLine> _dialogueDict = new Dictionary<string, DialogueLine>();
    private string _currentDialogueID;
    StringBuilder currentText = new StringBuilder();
    private bool _isTyping = false;
    public bool IsDialogueFinished { get; private set; } = true;
    private float _typeTimer = 0f;

    [SerializeField] private int _dialogueReferenceIndex;
    private int _inputLockedFrames;
    private const int LOCKEDFRAMES = 2;
    public bool HascrossedSeverityLine { get; private set; }
    void Start()
    {
        _currentDialogueID = dialogueAsset[_dialogueAssetIndex].dialogueLines[0].dialogueID;
    }
    public void OnInteract()
    {
        if (IsDialogueFinished && _inputLockedFrames <= 0)
        {
            StartDialogue();
           _inputLockedFrames = LOCKEDFRAMES;
        }
    }
    public string GetNextDialogueID()
    {
        _dialogueDict.TryGetValue(_currentDialogueID, out DialogueLine dialogueLine);
        if (dialogueLine != null)
        {
            return dialogueLine.nextDialogueID;
        }
        else
        {
            Debug.LogWarning($"No DialogueLine with ID '{_currentDialogueID}' found.");
            return null;
        }
    }
    public string GetCurrentDialogueID()
    {
        Debug.Log("Current DialogueID: " + _currentDialogueID);
        return _currentDialogueID;
    }
    public void EndDialogueAfterTyping(bool canEndDialogue)
    {
        _playOnlyOneLine = canEndDialogue;
    }
    public bool ReachedEnd()
    {
        _dialogueDict.TryGetValue(_currentDialogueID,out DialogueLine dialogueLine);
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
        foreach (DialogueLine line in dialogueAsset[_dialogueAssetIndex].dialogueLines)
        {
            if (_dialogueDict.ContainsKey(line.dialogueID))
                Debug.LogWarning($"Duplicate dialogueID: {line.dialogueID}");
            else
                _dialogueDict.Add(line.dialogueID, line);
        }
        foreach (DialogueLine line in _severeLines)
        {
            if (_dialogueDict.ContainsKey(line.dialogueID))
                Debug.LogWarning($"Duplicate dialogueID: {line.dialogueID}");
            else
                _dialogueDict.Add(line.dialogueID, line);
        }
        foreach (DialogueLine line in _crossedSevereLines)
        {
            if (_dialogueDict.ContainsKey(line.dialogueID))
                Debug.LogWarning($"Duplicate dialogueID: {line.dialogueID}");
            else
                _dialogueDict.Add(line.dialogueID, line);
        }

    }
    [ContextMenu(nameof(StartDialogue))]
    public void StartDialogue()
    {
        BuildDialogueDictionary();
        DialogueMangerScript.Instance.SetNextDialogueObject(_dialogueReferenceIndex);
        if (_onlyText)
        {
            DialogueMangerScript.Instance.ActivateText(true);
        }
        else
        {
            DialogueMangerScript.Instance.ActivateAllReferences(true);
        }
        if (_changeUIPosition)
        {
            //Changes Position and Alignment of the Dialogue Text, if specified
        }
        ShowDialogueLine(_currentDialogueID);
        OnStartDialogue?.Invoke();
        IsDialogueFinished = false;
    }
    void ShowDialogueLine(string dialogueID)
    {
        if (!_dialogueDict.TryGetValue(dialogueID, out DialogueLine line))
        {
            Debug.LogWarning($"no Dialogueline with ID '{dialogueID}' found.");
            return;
        }
        if (line.music != null && AudioManagerScript.Instance != null)
        {
            AudioManagerScript.Instance.PlayMusicTransitionally(line.music, line.MusicVolume);
        }
        else if(line.music == null && AudioManagerScript.Instance != null)
        {
            AudioManagerScript.Instance.StopMusic();
        }
        currentText.Clear();
        _isTyping = true;
        _typeTimer = 0f;
        SetLineReferences(line);
    }

    void ShowChoices(DialogueChoice[] choices)
    {
        string[] buttonText = new string[choices.Length];
        for (int i = 0; i < choices.Length; i++)
        {
            buttonText[i] = choices[i].choiceText;
        }
        DialogueMangerScript.Instance.SetUpChoiceButtons(choices.Length, buttonText, choices,OnChoiceSelected);
    }
    void OnChoiceSelected(DialogueChoice choice)
    {
        _NPCSeverityScore += choice.severity;
        if (_NPCSeverityScore > _severityLine)
        {
            HascrossedSeverityLine = true;
            DialogueMangerScript.Instance.DeactivateChoiceButtons();
            _currentDialogueLineIndex = 0;
            _currentDialogueID = _severeLines[_currentDialogueLineIndex].dialogueID;
            ShowDialogueLine(_severeLines[_currentDialogueLineIndex].dialogueID);
        }
        else
        {
            DialogueMangerScript.Instance.DeactivateChoiceButtons();
            ShowDialogueLine(choice.nextDialogueID);
            _currentDialogueID = choice.nextDialogueID;
        }
    }
    private void SetLineReferences(DialogueLine line)
    {
        DialogueMangerScript.Instance.SetLineReferences(line);
    }
    void Update()
    {
        if (!IsDialogueFinished)
        {
            DialogueCheck();
        }
        if (_inputLockedFrames > 0)
        {
            _inputLockedFrames--;
        }
    }
    void DialogueCheck()
    {
        //if (GameManagerScript.instance.IsMenuOn)
        //{
        //    return;
        //} 
        if (_inputLockedFrames > 0)
        {
            return;
        }
        _dialogueDict.TryGetValue(_currentDialogueID, out DialogueLine line);
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
            if ((pressedContinueButton && currentText.Length != line.textContent.Length))
            {
                TypeWholeText(line);
            }
            else if ((pressedContinueButton && line.nextDialogueID.ToUpper() == "END" && !_playOnlyOneLine && line.choices.Length == 0) || line.nextDialogueID.ToUpper() == "END" && _skipDialogue && !_playOnlyOneLine && line.choices.Length == 0)
            {
                _currentDialogueLineIndex++;
                HasDialogueEndedNaturally = true;
                EndDialogue();
            }
            else if  ((pressedContinueButton && !IsDialogueFinished && !_playOnlyOneLine && line.choices.Length == 0) || _skipDialogue && line.choices.Length == 0)
            {
                _currentDialogueLineIndex++;
                _currentDialogueID = line.nextDialogueID;
                _dialogueDict.TryGetValue(_currentDialogueID, out DialogueLine nextLine);
                ShowDialogueLine(_currentDialogueID);
            }
        }
        if (_isTyping)
        {
            TypewriterTick();
        }
    }
    private void TypeWholeText(DialogueLine line)
    {
        DialogueMangerScript.Instance.ShowText(line.textContent);
        _isTyping = false;
        _typeTimer = _typeSpeed;
        string oldValue = currentText.ToString();
        Debug.Log(oldValue);
        currentText.Clear();
        currentText.Append(line.textContent);
        if (line.choices.Length > 0)
        {
            ShowChoices(line.choices);
        }
    }

    private void InputHandling()
    {

    }
    void TypewriterTick()
    {
        if (!_dialogueDict.TryGetValue(_currentDialogueID, out DialogueLine line))
        {
            Debug.Log("There is no line " + _currentDialogueID);
            return;
        }
        _skipDialogue = false;
        string fullText = line.textContent;
        _typeTimer -= Time.deltaTime;
        if (_typeTimer <= 0)
        {
            if (line.talkSFX != null)
            {
                AudioManagerScript.Instance.PlaySfx(line.talkSFX);
            }
            _typeTimer = _typeSpeed;
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
                OnTextChanged?.Invoke(currentText.ToString());
                DialogueMangerScript.Instance.ShowText(currentText.ToString());
            }
            else
            {
                _isTyping = false;
                if (line.choices.Length > 0)
                {
                    ShowChoices(line.choices);
                }
                if (_shouldSkipWithoutPressing)
                {
                    _skipDialogue = true;
                }
                if (_playOnlyOneLine)
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
            if (_onlyText)
            {
                DialogueMangerScript.Instance.ActivateText(false);
            }
            else
            {
                DialogueMangerScript.Instance.ActivateAllReferences(false);
            }
        }
        _currentDialogueLineIndex = _currentDialogueLineIndex % dialogueAsset[_dialogueAssetIndex].dialogueLines.Length;
        if (GetNextDialogueID().ToUpper() == "END")
        {
            _dialogueAssetIndex++;
            _currentDialogueLineIndex = 0;
        }
        _dialogueAssetIndex = _dialogueAssetIndex % dialogueAsset.Length;
        _isTyping = false;
        IsDialogueFinished = true;
        _currentDialogueID = HascrossedSeverityLine ? _crossedSevereLines[_currentDialogueLineIndex].dialogueID : dialogueAsset[_dialogueAssetIndex].dialogueLines[_currentDialogueLineIndex].dialogueID;
        _skipDialogue = false;
        _dialogueDict.Clear();
        OnEndDialogue?.Invoke();
        _inputLockedFrames = LOCKEDFRAMES; // Need to lock 2 frames because of the input handling without it the Dialogue would not end properly
    }

}
