using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UsefulClasses;
public class TextDisplayManager : MonoBehaviour
{
    [System.Serializable]
    private class DialogueReferences
    {
        public GameObject dialoguePanel;
        public TextMeshProUGUI nameText;
        public GameObject namePanel;
        public TextMeshProUGUI textBox;
        public Image dialogueSprite;
        public GameObject dialogueParent;
        public GameObject continueSign;
        public GameObject choicesObject;
        public SliderLogic decisionSlider;
    }
    [SerializeField] private DialogueReferences[] dialogueReferences;
    [SerializeField] private DialogueReferences _currentReference;
    [SerializeField] private GameObject _letterBoxParent;
    [SerializeField] private Vector3[] _letterBoxGoalPositions;
    private Vector3[] _letterBoxStartPositions;
    private RectTransform[] _letterBoxes;
    public Key[] continueKeys;
    public MouseButton mouseContinueButton;
    private Button[] _choiceButtons;
    public static TextDisplayManager Instance { get; private set; }
    private Canvas _dialogueCanvas;
    private List<DialogueEffect> _effects = new List<DialogueEffect>();
    private NPCDialogue _currentNPCDialogue;
    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        SetUp();
    }
    public void SaveCurrentNPCDialogue(NPCDialogueSaveData npcSaveData)
    {
        GameManager.Instance._playerData.SaveNPCData(npcSaveData);
        SaveSystem.SavePlayerdata(GameManager.Instance._playerData);
    }
    private void SetUp()
    {
        // Letterboxes
        if (_letterBoxParent == null)
        {
            Debug.LogError("TextDisplayManager: _letterBoxParent is not set to anything.");
            return;
        }
        _letterBoxes = _letterBoxParent.GetComponentsInChildren<RectTransform>()
            .Where(lb => lb.gameObject != _letterBoxParent)
            .ToArray();
        _letterBoxStartPositions = new Vector3[_letterBoxes.Length];
        for (int i = 0; i < _letterBoxes.Length; i++)
        {
            _letterBoxStartPositions[i] = _letterBoxes[i].anchoredPosition;
        }
        // Canvas
        _dialogueCanvas = GetComponentInChildren<Canvas>();
        if (_dialogueCanvas == null)
        {
            Debug.LogError("TextDisplayManager: found no Canvas.");
            return;
        }
        _dialogueCanvas.worldCamera = Helpers.Camera;
        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        // References
        if (dialogueReferences == null || dialogueReferences.Length == 0)
        {
            Debug.LogError("TextDisplayManager: dialogueReferences is empty.");
            return;
        }
        _currentReference = dialogueReferences[0];
        if (_currentReference.choicesObject == null)
        {
            Debug.LogError("TextDisplayManager: choicesObject is null.");
            return;
        }
        _choiceButtons = new Button[_currentReference.choicesObject.transform.childCount];
        for (int i = 0; i < _currentReference.choicesObject.transform.childCount; i++)
        {
            _choiceButtons[i] = _currentReference.choicesObject.transform.GetChild(i).GetComponent<Button>();
        }

        if (_currentReference.decisionSlider == null)
        {
            Debug.LogError("TextDisplayManager: decisionSlider ist null.");
            return;
        }
        _currentReference.decisionSlider.OnValueReachedMax += DecideRandom;
    }
    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        if (_currentReference?.decisionSlider != null)
        {
            _currentReference.decisionSlider.OnValueReachedMax -= DecideRandom;
        }
    }
    private void SceneManager_activeSceneChanged(Scene prev, Scene next)
    {
        _dialogueCanvas = GetComponentInChildren<Canvas>();
        if (_dialogueCanvas == null)
        {
            Debug.LogError("TextDisplayManager: No Canvas after Scene transition found.");
            return;
        }
        _dialogueCanvas.worldCamera = Helpers.Camera;
    }
    public void SetNextDialogueObject(int index)
    {
        if (index < 0 || index >= dialogueReferences.Length)
        {
            Debug.LogWarning($"TextDisplayManager: Index {index} is outside the bounds of the array.");
            return;
        }
        _currentReference = dialogueReferences[index];
    }
    public void ApplyEffect(DialogueEffect nextEffect)
    {
        if (_effects.Count > 0)
        {
            if (nextEffect.type == EffectType.none || _effects.Any(e => e.type == nextEffect.type))
                return;
        }
        if (!nextEffect.shouldAccumalateEffect)
        {
            CancelEffect();
        }
        _effects.Add(nextEffect);   
        switch (nextEffect.type)
        {
            case EffectType.CameraShake:
                StartCoroutine(CameraShake(nextEffect));
                break;
            case EffectType.Vignette:
                StartCoroutine(ShowVignette(nextEffect));
                break;
            case EffectType.Letterbox:
                StartCoroutine(ShowLetterBox(nextEffect));
                break;
        }
    }

    private IEnumerator ShowLetterBox(DialogueEffect effect, bool playNormal = true)
    {
        float elapsed = 0f;
        Vector3 letterBoxDownGoalPosition = playNormal ? _letterBoxGoalPositions[0] : _letterBoxStartPositions[0];
        Vector3 letterBoxDownStartPosition = playNormal ? _letterBoxStartPositions[0] : _letterBoxGoalPositions[0];
        Vector3 letterBoxUpGoalPosition = playNormal ? _letterBoxGoalPositions[1] : _letterBoxStartPositions[1];
        Vector3 letterBoxUpStartPosition = playNormal ? _letterBoxStartPositions[1] : _letterBoxGoalPositions[1];
        while (elapsed < effect.duration)
        {
            float t = elapsed / effect.duration;
            float curveValue = effect.curve.Evaluate(t);
            _letterBoxes[0].anchoredPosition = Vector3.Lerp(letterBoxDownStartPosition, letterBoxDownGoalPosition, t);
            _letterBoxes[1].anchoredPosition = Vector3.Lerp(letterBoxUpStartPosition, letterBoxUpGoalPosition, t);
            float currentIntensity = effect.intensity * curveValue;
            elapsed += Time.deltaTime;
            yield return null;
        }
        _letterBoxes[0].anchoredPosition = letterBoxDownGoalPosition;
        _letterBoxes[1].anchoredPosition = letterBoxUpGoalPosition;
    }

    public void CancelEffect(bool cancelLastEffect = true)
    {
        if (_effects.Count > 0)
        {
            if (GameManager.Instance.postProcessingProfile == null)
            {
                Debug.LogError("PostProcessingProfile is not set to an instance babes");
                return;
            }
            List<DialogueEffect> cancelEffects = new List<DialogueEffect>();
            if (cancelLastEffect)
            {
                cancelEffects.Add(_effects[_effects.Count - 1]);
            }
            else
            {
                cancelEffects = _effects;
            }
            for (int i = cancelEffects.Count -1; i >= 0; i--)
            {
                DialogueEffect lastEffect = cancelEffects[i];
                _effects.Remove(lastEffect);
                switch (lastEffect.type)
                {
                    case EffectType.Vignette:
                        if (GameManager.Instance.postProcessingProfile.TryGet(out Vignette vignette))
                        {
                            StartCoroutine(ShowVignette(lastEffect, false));
                        }
                        break;
                    case EffectType.Letterbox:
                        StartCoroutine(ShowLetterBox(lastEffect, false));
                        break;
                }
            }
        }
    }
    private IEnumerator CameraShake(DialogueEffect effect)
    {
        float elapsed = 0f;
        Vector3 originalPos = Helpers.Camera.transform.localPosition;
        while (elapsed < effect.duration)
        {
            float t = elapsed / effect.duration;
            float curveValue = effect.curve.Evaluate(t);
            float currentIntensity = effect.intensity * curveValue;
            Vector3 shakeOffset = UnityEngine.Random.insideUnitSphere * currentIntensity;
            Helpers.Camera.transform.localPosition = originalPos + shakeOffset;
            elapsed += Time.deltaTime;
            yield return null;
        }
        Helpers.Camera.transform.localPosition = originalPos;
    }
    private IEnumerator ShowVignette(DialogueEffect effect, bool playNormal = true)
    {
        if (GameManager.Instance == null || GameManager.Instance.postProcessingProfile == null)
        {
            Debug.LogError("TextDisplayManager: GameManager or postProcessingProfile is null.");
            yield break;
        }
        float elapsed = 0f;
        if (GameManager.Instance.postProcessingProfile.TryGet(out Vignette vignette))
        {
            while (elapsed < effect.duration)
            {
                float t = elapsed / effect.duration;
                if (!playNormal) t = 1f - t;
                float curveValue = effect.curve.Evaluate(t);
                float currentIntensity = effect.intensity * curveValue;
                vignette.intensity.value = currentIntensity;
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
    }
    private void DecideRandom()
    {
        int random = 0;
        do
        {
          if (!_choiceButtons.Any(b => b.gameObject.activeSelf)) return;
          random = UnityEngine.Random.Range(0, _choiceButtons.Length);
        } while (!_choiceButtons[random].gameObject.activeSelf);
        _choiceButtons[random].onClick.Invoke();
        DeactivateChoiceButtons();
    }
    public void SetLineReferences(DialogueLine line)
    {
        if (line == null)
        {
            Debug.LogWarning("TextDisplayManager: DialogueLine is null.");
            return;
        }
        if (_currentReference.dialogueSprite != null)
        {
            _currentReference.dialogueSprite.sprite = line.sprite;
        }
        if (_currentReference.nameText != null)
        {
            _currentReference.nameText.text = line.speaker;
        }
    }
    public void SetContinueSign(bool isActive)
    {
        _currentReference.continueSign.SetActive(isActive);
    }
    public void ShowText(string text)
    {
        _currentReference.textBox.text = text;
    }
    public bool TextFitsInTextLine(string text) 
    {
        Vector2 size = _currentReference.textBox.GetPreferredValues(text);
        return size.x < _currentReference.textBox.rectTransform.rect.width;
    }
    public void ActivateChoices(bool isActive)
    {
        foreach(Transform child in _currentReference.choicesObject.transform)
        {
            child.gameObject.SetActive(isActive);
        }
    }
    public void ActivateAllReferences(bool isActive)
    {
        foreach (object reference in _currentReference.GetAllFields())
        {
            if (reference == null)
                continue;
            if (reference is GameObject go)
            {
                if (go != null)
                {
                    if (!go.transform.parent.CompareTag(Tag.DontActivate.ToString()))
                    {
                        go.SetActive(isActive);
                    }
                }
            }
            else if (reference is Component component)
            {
                if (component is TextMeshProUGUI textMesh)
                {
                    textMesh.text = string.Empty;
                }
                if (!component.transform.parent.CompareTag(Tag.DontActivate.ToString()))
                {
                    component.gameObject.SetActive(isActive);
                }
            }
        }
    }
    public void SetUpChoiceButtons(int amount, string[] buttonTexts, DialogueChoice[] choices, Action<DialogueChoice> onChoiceSelected)
    {
        amount = Mathf.Clamp(amount, 0, _choiceButtons.Length);
        for (int i = 0; i < amount; i++)
        {
            _choiceButtons[i].gameObject.SetActive(true);
            _choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = buttonTexts[i];
            _choiceButtons[i].onClick.RemoveAllListeners();
            DialogueChoice choice = choices[i];
            _choiceButtons[i].onClick.AddListener(() => onChoiceSelected(choice));
            _currentReference.decisionSlider.gameObject.SetActive(true);
            _currentReference.decisionSlider.CanIncreaseValue();
        }
    }
    public IEnumerator SetUpChoiceButtonsNextFrame(int amount, string[] buttonTexts, DialogueChoice[] choices, Action<DialogueChoice> onChoiceSelected)
    {
        yield return null;
        SetUpChoiceButtons(amount,buttonTexts,choices,onChoiceSelected);
    }
    public void ShowContinueSign(bool canShow)
    {
        if (Instance == null) return;
        _currentReference.continueSign.SetActive(canShow);
    }
    public void DeactivateChoiceButtons()
    {
        for (int i = 0; i <_choiceButtons.Length ; i++)
        {
            if (_choiceButtons[i] == null) break;
            _choiceButtons[i].onClick.RemoveAllListeners();
            _choiceButtons[i].gameObject.SetActive(false);
        }
        if (_currentReference.decisionSlider != null)
        {
            _currentReference.decisionSlider.gameObject.SetActive(false);
        }
    }
    public void AlignText(TextAlignmentOptions textAlignmentOptions)
    {
        _currentReference.textBox.alignment = textAlignmentOptions;
    }
    public void ActivateText(bool isActive)
    {
        _currentReference.dialogueParent.SetActive(isActive);
        _currentReference.textBox.text = string.Empty;
        _currentReference.textBox.gameObject.SetActive(isActive);
    }
}
