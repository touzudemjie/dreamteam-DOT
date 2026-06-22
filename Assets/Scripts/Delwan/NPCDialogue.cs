using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
public class NPCDialogue : MonoBehaviour, IInteractable
{
    [SerializeField] private Dialogue[] _dialogueAsset;
    [SerializeField] public DialogueLine[] _severeLines;
    [SerializeField] public DialogueLine[] _crossedSevereLines;
    [SerializeField] private int _severityLine;
    [SerializeField] private int _NPCSeverityScore;
    [SerializeField] private TextAlignmentOptions _textAlignment;
    [SerializeField] private float _typeSpeed = 0.05f;
    private float _typeTimer = 0f;
    [SerializeField] private float _periodTypeSpeed;
    private int _currentDialogueLineIndex;
    private int _currentDialogueLineIndexTmp;
    private int _dialogueAssetIndex;
    private int _sourceIndex;
    public event Action OnStartDialogue;
    public event Action<string> OnLineFinshed;
    public event Action OnEndDialogue;
    public event Action OnSeverityLineCrossed;
    private Dictionary<string, DialogueLine> _dialogueDict = new Dictionary<string, DialogueLine>();
    private string _currentDialogueID;
    private StringBuilder _currentText = new StringBuilder();
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

    public bool HasDialogueEndedNaturally { get; private set; } = true;
    [SerializeField, ReadOnly] private string _NPCId;
    private HashSet<string> _seenIds = new HashSet<string>();

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(_NPCId))
        {
            _NPCId = Guid.NewGuid().ToString();
        }
        EnsureChoiceIds();
        EditorUtility.SetDirty(this);
    }

    private void EnsureChoiceIds()
    {
        if (_dialogueAsset != null)
        {
            foreach (Dialogue dialogue in _dialogueAsset)
            {
                ProcessLinesArray(dialogue?.dialogueLines, _seenIds);
            }
        }
        ProcessLinesArray(_severeLines, _seenIds);
        ProcessLinesArray(_crossedSevereLines, _seenIds);
    }

    private void ProcessLinesArray(DialogueLine[] lines, HashSet<string> seenIds)
    {
        if (lines == null) return;
        foreach (DialogueLine line in lines)
        {
            ProcessChoices(line?.choices, seenIds);
        }
    }

    private void ProcessChoices(DialogueChoice[] choices, HashSet<string> seenIds)
    {
        if (choices == null) return;
        foreach (DialogueChoice choice in choices)
        {
            if (choice == null) continue;

            choice.GenerateIdIfMissing();
        }
    }
#endif
    void Start()
    {
        if (_dialogueAsset.Length != 0)
        {
            _currentDialogueID = _dialogueAsset[_dialogueAssetIndex].dialogueLines[0].dialogueID;
        }
       SetNPCData();
    }
    void SetNPCData()
    {
        NPCDialogueSaveData NPCSaveData = GameManager.Instance._playerData.FindNPCData(_NPCId);
        if (NPCSaveData == null)
        {
            SaveNPCData();
        }
        else
        {
            _NPCId = NPCSaveData.NPCId;
            _currentDialogueLineIndex = NPCSaveData.currentDialogueLineIndex;
            HascrossedSeverityLine = NPCSaveData.hasCrossedSeverityLine;
            _dialogueAssetIndex = NPCSaveData.dialogueAssetIndex;
            _NPCSeverityScore = NPCSaveData.NPCSeverityScore;
            _currentDialogueID = NPCSaveData.currentDialogueId;
            _seenIds = new HashSet<string>(NPCSaveData.encounteredSevereIds);
        }

    }
    private void SaveNPCData()
    {
        TextDisplayManager.Instance.SaveCurrentNPCDialogue(new NPCDialogueSaveData
        {
            currentDialogueLineIndex = _currentDialogueLineIndex,
            hasCrossedSeverityLine = HascrossedSeverityLine,
            dialogueAssetIndex = _dialogueAssetIndex,
            NPCSeverityScore = _NPCSeverityScore,
            NPCId = _NPCId,
            currentDialogueId = _currentDialogueID,
            encounteredSevereIds = _seenIds.ToList()
        });
    }
    public void OnInteract()
    {
        if (HasDialogueEndedNaturally && _inputLockedFrames <= 0 && _dialogueAsset.Length != 0)
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
            Debug.LogWarning($"No DialogueLine with ID '{_currentDialogueID} {gameObject.name}' found.");
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
        _dialogueDict.TryGetValue(_currentDialogueID, out DialogueLine dialogueLine);
        if (dialogueLine != null)
        {
            Debug.Log(new System.Diagnostics.StackTrace(true).ToString());
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
        if (_dialogueDict.TryGetValue(GetCurrentDialogueID(), out DialogueLine line))
        {
            if (line.dialogueEffect != null)
            {
                TextDisplayManager.Instance.ApplyEffect(line.dialogueEffect);
            }
        }
        HasDialogueEndedNaturally = false;
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
        else if (line.music == null && AudioManagerScript.Instance != null)
        {
            AudioManagerScript.Instance.StopMusic();
        }
        _currentText.Clear();
        _sourceIndex = 0; 
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
        StartCoroutine(TextDisplayManager.Instance.SetUpChoiceButtonsNextFrame(choices.Length, buttonText, choices, OnChoiceSelected));
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
        bool newChoiceId = _seenIds.Add(choice.ChoiceId);
        if (newChoiceId)
        {
            _NPCSeverityScore += choice.severity;
        }
        else
        {
            Debug.Log($"Already Punished");
        }
        if (_NPCSeverityScore > _severityLine)
        {
            OnSeverityLineCrossed?.Invoke();
            HascrossedSeverityLine = true;
            TextDisplayManager.Instance.DeactivateChoiceButtons();
            _currentDialogueLineIndex = 0;
            _lockDialogueAssetIndex = false;
            _saveDialogueIndex = false;
            _currentDialogueLineIndexTmp = 1;
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
        if (!HasDialogueEndedNaturally)
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
        string nextId = GetNextDialogueID();
        bool nextIsEnd = IsEndId(nextId);

        bool pressedContinueButton = false;
        foreach (Key continueKeyCode in TextDisplayManager.Instance.continueKeys)
        {
            pressedContinueButton = (Keyboard.current?[continueKeyCode].wasPressedThisFrame ?? false) || (Mouse.current?.leftButton.wasPressedThisFrame ?? false);
            if (pressedContinueButton)
            {
                break;
            }
        }
        if (line != null)
        {
            if (pressedContinueButton && _sourceIndex != line.textContent.Length)
            {
                TypeWholeText(line);
            }
            else if ((pressedContinueButton && nextIsEnd && !_playOnlyOneLine && line.choices.Length == 0) || (nextIsEnd && _skipDialogue && !_playOnlyOneLine && line.choices.Length == 0))
            {
                HasDialogueEndedNaturally = true;
                EndDialogue();
            }
            else if ((pressedContinueButton && !HasDialogueEndedNaturally && !_playOnlyOneLine && line.choices.Length == 0) || (_skipDialogue && line.choices.Length == 0))
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
        _currentText.Clear();
        _currentText.Append(line.textContent);
        _sourceIndex = line.textContent.Length;
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
            if (_sourceIndex < fullText.Length)
            {
                if (fullText[_sourceIndex] == '<')
                {
                    while (_sourceIndex < fullText.Length - 1 && fullText[_sourceIndex] != '>')
                    {
                        _currentText.Append(fullText[_sourceIndex]);
                        _sourceIndex++;
                    }
                }
                if (line.talkSFX != null)
                {
                    AudioManagerScript.Instance.PlaySfx(line.talkSFX);
                }
                char nextChar = fullText[_sourceIndex]; 
                AppendNewLine(nextChar, fullText, _sourceIndex);

                _currentText.Append(nextChar);
                _sourceIndex++;

                if (_sourceIndex < fullText.Length)
                {
                    if (nextChar == '.' && fullText[_sourceIndex] == '.')
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
        string nextId = GetNextDialogueID();
        TextDisplayManager.Instance.ShowContinueSign(IsEndId(nextId) && !_onlyText);
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

    private void AppendNewLine(char nextChar, string fullText, int sourceIndex)
    {
        if (nextChar != ' ' && nextChar != '\n')
        {
            bool isStartOfWord = _currentText.Length == 0 ||
                                    _currentText[_currentText.Length - 1] == ' ' ||
                                    _currentText[_currentText.Length - 1] == '\n';
            if (isStartOfWord)
            {
                int wordStart = sourceIndex; 
                int wordEnd = fullText.IndexOfAny(new char[] { ' ', '\n' }, wordStart);
                if (wordEnd < 0)
                {
                    wordEnd = fullText.Length;
                }
                string nextWord = fullText.Substring(wordStart, wordEnd - wordStart);
                string nextWordStripped = _tagRegex.Replace(nextWord, string.Empty);
                string lastLine = _currentText.ToString();
                int lastNewline = lastLine.LastIndexOf('\n');
                string currentLine = lastNewline >= 0 ? lastLine.Substring(lastNewline + 1) : lastLine;
                string currentLineStripped = _tagRegex.Replace(currentLine, string.Empty);
                if (!TextDisplayManager.Instance.TextFitsInTextLine(currentLineStripped + nextWordStripped))
                {
                    _currentText.Append('\n');
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
        }
        _currentDialogueLineIndex = 0;
        if (_saveDialogueIndex)
        {
            _currentDialogueLineIndex = _currentDialogueLineIndexTmp;
            _saveDialogueIndex = false;
        }
        TextDisplayManager.Instance.CancelEffect(false);
        TextDisplayManager.Instance.ShowContinueSign(false);
        TextDisplayManager.Instance.DeactivateChoiceButtons();
        _isTyping = false;
        _currentDialogueID = HascrossedSeverityLine ? _crossedSevereLines[_currentDialogueLineIndex].dialogueID : _dialogueAsset[_dialogueAssetIndex].dialogueLines[_currentDialogueLineIndex].dialogueID;
        _skipDialogue = false;
        OnEndDialogue?.Invoke();
        SaveNPCData();
        _inputLockedFrames = LOCKEDFRAMES; // Need to lock 2 frames because of the input handling without it the Dialogue would not end properly
        Cursor.lockState = CursorLockMode.Locked;
    }

    private bool IsEndId(string id) => !string.IsNullOrEmpty(id) && string.Equals(id, "END", StringComparison.OrdinalIgnoreCase);
}