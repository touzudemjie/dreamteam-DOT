using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
public class NPCDialogue : MonoBehaviour, IInteractable
{
    [SerializeField] private Dialogue[] _dialogueAsset;
    [SerializeField] public DialogueLine[] _severeLines;
    [SerializeField] public DialogueLine[] _crossedSevereLines;
    [SerializeField] private int _severityLine;
    [SerializeField] private int _NPCSeverityScore;
    [SerializeField] private Vector3 _textPosition;
    [SerializeField] private TextAlignmentOptions _textAlignment;
    [SerializeField] private float _typeSpeed = 0.05f;
    [SerializeField] private float _periodTypeSpeed;
    private int _currentDialogueLineIndex;
    private int _currentDialogueLineIndexTmp;
    private int _dialogueAssetIndex;
    public event Action OnStartDialogue;
    public event Action<string> OnLineFinshed;
    public event Action OnEndDialogue;
    public event Action OnSeverityLineCrossed;
    private Dictionary<string, DialogueLine> _dialogueDict = new Dictionary<string, DialogueLine>();
    private string _currentDialogueID;
    private StringBuilder _currentText = new StringBuilder();
    private float _typeTimer = 0f;
    [SerializeField] private int _textDisplayReferenceIndex;
    private int _inputLockedFrames;
    private const int LOCKEDFRAMES = 2;
    private static readonly System.Text.RegularExpressions.Regex _tagRegex = new System.Text.RegularExpressions.Regex(@"<[^>]+>");
    [SerializeField] private bool _playOnlyOneLine;
    [SerializeField] private bool _shouldSkipWithoutPressing;
    [SerializeField] private bool _shouldLoopOver = true;
    private bool _skipDialogue;
    public static bool IsDialogueFinished { get; private set; } = true;
    [SerializeField] private bool _onlyText;
    private bool _isTyping = false;
    public bool HascrossedSeverityLine { get; private set; }
    private bool _lockDialogueAssetIndex;
    private bool _saveDialogueIndex;

    public bool HasDialogueEndedNaturally { get; private set; }
    void Start()
    {
        if (_dialogueAsset != null)
        {
          _currentDialogueID = _dialogueAsset[_dialogueAssetIndex].dialogueLines[0].dialogueID;
        }
    }
    public void OnInteract()
    {
        if (IsDialogueFinished && _inputLockedFrames <= 0 && _dialogueAsset != null)
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
            return IsEndId(GetNextDialogueID());
        }
        else
        {
            return false;
        }
    }
    void BuildDialogueDictionary()
    {
        _dialogueDict.Clear();
        foreach (DialogueLine line in _dialogueAsset[_dialogueAssetIndex].dialogueLines)
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
        TextDisplayManager.Instance.SetNextDialogueObject(_textDisplayReferenceIndex);
        TextDisplayManager.Instance.AlignText(_textAlignment);
        if (_onlyText)
        {
            TextDisplayManager.Instance.ActivateText(true);
        }
        else
        {
            TextDisplayManager.Instance.ActivateAllReferences(true);
        }
        ShowDialogueLine(_currentDialogueID);
        if(_dialogueDict.TryGetValue(GetCurrentDialogueID(), out DialogueLine line))
        {
            if (line.dialogueEffect != null)
            {
                TextDisplayManager.Instance.ApplyEffect(line.dialogueEffect);
            }
        }
        OnStartDialogue?.Invoke();
        IsDialogueFinished = false;
        Cursor.lockState = CursorLockMode.None;
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
            AudioManagerScript.Instance.PlayMusicTransitionally(line.music);
        }
        else if(line.music == null && AudioManagerScript.Instance != null)
        {
            AudioManagerScript.Instance.StopMusic();
        }
        _currentText.Clear();
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
        StartCoroutine(TextDisplayManager.Instance.SetUpChoiceButtonsNextFrame(choices.Length, buttonText, choices,OnChoiceSelected));
    }
    void OnChoiceSelected(DialogueChoice choice)
    {
        _lockDialogueAssetIndex = choice.lockDialogueAssetIndex;
        _saveDialogueIndex = choice.saveDialogueLineIndex;
        _currentDialogueLineIndexTmp = _saveDialogueIndex ? _currentDialogueLineIndex : -1;
      
        if (choice.condition != null)
        {
            if (!choice.condition.Evaluate())
            {
                TextDisplayManager.Instance.DeactivateChoiceButtons();
                ShowDialogueLine(choice.failedConditionId);
                _currentDialogueID = choice.failedConditionId;
                if (_dialogueDict.TryGetValue(choice.failedConditionId, out DialogueLine failedLine))
                {
                    if (failedLine.dialogueEffect != null)
                    {
                        TextDisplayManager.Instance.ApplyEffect(failedLine.dialogueEffect);
                    }
                }
                return;
            }
            else
            {
                _lockDialogueAssetIndex = false;
                _saveDialogueIndex = false;
            }
        }
        _NPCSeverityScore += choice.severity;
        if (_NPCSeverityScore > _severityLine)
        {
            OnSeverityLineCrossed?.Invoke();
            HascrossedSeverityLine = true;
            TextDisplayManager.Instance.DeactivateChoiceButtons();
            _currentDialogueLineIndex = 0;
            _lockDialogueAssetIndex = false;
            _saveDialogueIndex = false;
            _currentDialogueID = _severeLines[_currentDialogueLineIndex].dialogueID;
            ShowDialogueLine(_severeLines[_currentDialogueLineIndex].dialogueID);
        }
        else
        {
            choice.onChosen?.Invoke();
            TextDisplayManager.Instance.DeactivateChoiceButtons();
            ShowDialogueLine(choice.nextDialogueID);
            _currentDialogueID = choice.nextDialogueID;
        }
        if (_dialogueDict.TryGetValue(choice.nextDialogueID, out DialogueLine line))
        {
            if (line.dialogueEffect != null)
            {
                TextDisplayManager.Instance.ApplyEffect(line.dialogueEffect);
            }
        }
    }

    private void SetLineReferences(DialogueLine line)
    {
        TextDisplayManager.Instance.SetLineReferences(line);
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
        if (_inputLockedFrames > 0)
        {
            return;
        }
        _dialogueDict.TryGetValue(_currentDialogueID, out DialogueLine line);
        bool pressedContinueButton = false;
        foreach (Key continueKeyCode in TextDisplayManager.Instance.continueKeys)
        {
            pressedContinueButton = (Keyboard.current?[continueKeyCode].wasPressedThisFrame ?? false ) || (Mouse.current?.leftButton.wasPressedThisFrame ?? false);
            if (pressedContinueButton)
            {
                break;
            }
        }
        if (line != null)
        {
            if (pressedContinueButton && _currentText.Length != line.textContent.Length)
            {
                TypeWholeText(line);
            }
            else if ((pressedContinueButton && IsEndId(GetNextDialogueID()) && !_playOnlyOneLine && line.choices.Length == 0) || (IsEndId(GetNextDialogueID()) && _skipDialogue && !_playOnlyOneLine && line.choices.Length == 0))
            {
                TextDisplayManager.Instance.CancelEffect(false);
                HasDialogueEndedNaturally = true;
                EndDialogue();
            }
            else if  ((pressedContinueButton && !IsDialogueFinished && !_playOnlyOneLine && line.choices.Length == 0) || (_skipDialogue && line.choices.Length == 0))
            {
                _currentDialogueLineIndex++;
                _currentDialogueID = line.nextDialogueID;
                _dialogueDict.TryGetValue(_currentDialogueID, out DialogueLine nextLine);
                if (nextLine?.dialogueEffect != null)
                {
                    TextDisplayManager.Instance.ApplyEffect(nextLine.dialogueEffect);
                }
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
        TextDisplayManager.Instance.ShowText(line.textContent);
        _isTyping = false;
        _typeTimer = _typeSpeed;
        string oldValue = _currentText.ToString();
        _currentText.Clear();
        _currentText.Append(line.textContent);
        EvaluateOptionalBools(line);
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
            _typeTimer = _typeSpeed;
            if (_currentText.Length < fullText.Length)
            {
                if (fullText[_currentText.Length] == '<')
                {
                    int i = _currentText.Length;
                    while (i < fullText.Length - 1 && fullText[i] != '>')
                    {
                        _currentText.Append(fullText[i]);
                        i++;
                    }
                }
                if (line.talkSFX != null)
                {
                    AudioManagerScript.Instance.PlaySfx(line.talkSFX);
                }
                char nextChar = fullText[_currentText.Length];
                AppendNewLine(nextChar, line);
                _currentText.Append(nextChar);
                if (_currentText.Length < fullText.Length)
                {
                    if (nextChar == '.' && fullText[_currentText.Length] == '.')
                    {
                        _typeTimer = _periodTypeSpeed;
                    }
                    else
                    {
                        _typeTimer = _typeSpeed;
                    }
                }
                TextDisplayManager.Instance.ShowText(_currentText.ToString());
            }
            else
            {
                OnLineFinshed?.Invoke(_currentText.ToString());
                _isTyping = false;
                EvaluateOptionalBools(line);
            }

        }
    }
    private void EvaluateOptionalBools(DialogueLine line)
    {
        if (IsEndId(GetNextDialogueID()))
        {
            TextDisplayManager.Instance.ShowContinueSign(true);
        }
        else
        {
            TextDisplayManager.Instance.ShowContinueSign(false);
        }
        if (line.choices.Length > 0)
        {
            ShowChoices(line.choices);
        }
        if (_shouldSkipWithoutPressing && line.choices.Length == 0)
        {
            _skipDialogue = true;
        }
        if (_playOnlyOneLine && line.choices.Length == 0)
        {
            _currentDialogueLineIndex++;
            EndDialogue();
        }
    }
    private void AppendNewLine(char nextChar, DialogueLine line)
    {
        string fullText = line.textContent;
        if (nextChar != ' ' && nextChar != '\n')
        {
            bool isStartOfWord = _currentText.Length == 0 ||
                                    _currentText[_currentText.Length - 1] == ' ' ||
                                    _currentText[_currentText.Length - 1] == '\n';
            if (isStartOfWord)
            {
                // Komplettes nächstes Wort ermitteln
                int wordStart = _currentText.Length;
                int wordEnd = fullText.IndexOfAny(new char[] { ' ', '\n' }, wordStart);
                if (wordEnd < 0)
                {
                    wordEnd = fullText.Length;
                }
                string nextWord = fullText.Substring(wordStart, wordEnd - wordStart);
                string nextWordStripped = _tagRegex.Replace(nextWord, string.Empty);
                // Aktuelle Zeile + Wort testen
                string lastLine = _currentText.ToString();
                int lastNewline = lastLine.LastIndexOf('\n');
                string currentLine = lastNewline >= 0 ? lastLine.Substring(lastNewline + 1) : lastLine;
                string currentLineStripped = _tagRegex.Replace(currentLine, string.Empty);
                if (!TextDisplayManager.Instance.TextFitsInTextLine(currentLineStripped + nextWordStripped))
                {
                    // Newline vor dem Wort einfügen
                    _currentText.Append('\n');
                    line.textContent = line.textContent.Insert(_currentText.Length - 1, "\n");
                }
            }
        }
    }
    private void OnDestroy()
    {
        if (Application.isPlaying)
        {
            EndDialogue();
        }
    }
    public void EndDialogue()
    {
        if (IsDialogueFinished) return;
        IsDialogueFinished = true;
        if (TextDisplayManager.Instance != null)
        {
            if (_onlyText)
            {
                TextDisplayManager.Instance.ActivateText(false);
            }
            else
            {
                TextDisplayManager.Instance.ActivateAllReferences(false);
            }
        }
        if (IsEndId(GetNextDialogueID()))
        {
            if (!_lockDialogueAssetIndex)
            {
                if (_shouldLoopOver)
                {
                    _dialogueAssetIndex = (_dialogueAssetIndex + 1) % _dialogueAsset.Length;
                }
                else
                {
                    _dialogueAssetIndex = Mathf.Clamp(_dialogueAssetIndex + 1, 0, _dialogueAsset.Length - 1);
                }
            }
            _currentDialogueLineIndex = 0;
        }
        if (_saveDialogueIndex)
        {
            _currentDialogueLineIndex = _currentDialogueLineIndexTmp;
            _saveDialogueIndex = false;
        }
        TextDisplayManager.Instance.ShowContinueSign(false);
        TextDisplayManager.Instance.DeactivateChoiceButtons();
        _isTyping = false;
        _currentDialogueID = HascrossedSeverityLine ? _crossedSevereLines[_currentDialogueLineIndex].dialogueID : _dialogueAsset[_dialogueAssetIndex].dialogueLines[_currentDialogueLineIndex].dialogueID;
        _skipDialogue = false;
        OnEndDialogue?.Invoke();
        _inputLockedFrames = LOCKEDFRAMES; // Need to lock 2 frames because of the input handling without it the Dialogue would not end properly
        Cursor.lockState = CursorLockMode.Locked;
    }

    private bool IsEndId(string id) =>!string.IsNullOrEmpty(id) && string.Equals(id, "END", StringComparison.OrdinalIgnoreCase);
}
