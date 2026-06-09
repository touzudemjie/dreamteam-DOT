using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UsefulClasses;

public class DialogueMangerScript : MonoBehaviour
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
    public Key[] continueKeys;
    private Button[] _choiceButtons;
    public static DialogueMangerScript Instance { get; private set; }
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
      _currentReference = dialogueReferences[0];
      _choiceButtons = new Button[_currentReference.choicesObject.transform.childCount];
       for (int i = 0; i < _currentReference.choicesObject.transform.childCount; i++)
        {
            _choiceButtons[i] = _currentReference.choicesObject.transform.GetChild(i).GetComponent<Button>();
        }
       _currentReference.decisionSlider.OnValueReachedMax += DecideRandom;
    }
    public void SetNextDialogueObject(int index)
    {
        if (index > dialogueReferences.Length) return;
        _currentReference = dialogueReferences[index];
    }
    private void Update()
    {
    }
    private void DecideRandom()
    {
        int random = 0;
        do
        {
            random = UnityEngine.Random.Range(0, _choiceButtons.Length);

        } while (_choiceButtons[random].onClick == null);
        _choiceButtons[random].onClick.Invoke();
        DeactivateChoiceButtons();
    }
    public void SetLineReferences(DialogueLine line)
    {
        _currentReference.dialogueSprite.sprite = line.sprite;   
        _currentReference.nameText.text = line.speaker;
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
                    if (!go.CompareTag(Tag.Choices.ToString()) || !go.CompareTag(Tag.Decision.ToString()))
                    {
                        Debug.Log(go.name);
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
                if (!component.gameObject.CompareTag(Tag.Choices.ToString()) && !component.gameObject.CompareTag(Tag.Decision.ToString()))
                {
                    Debug.Log(component.gameObject.tag);
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
    public void DeactivateChoiceButtons()
    {
        for (int i = 0; i <_choiceButtons.Length ; i++)
        {
            _choiceButtons[i].onClick.RemoveAllListeners();
            _choiceButtons[i].gameObject.SetActive(false);
        }
        _currentReference.decisionSlider.gameObject.SetActive(false);
    }
    public void ActivateText(bool isActive)
    {
        _currentReference.dialogueParent.SetActive(isActive);
        _currentReference.textBox.text = string.Empty;
        _currentReference.textBox.gameObject.SetActive(isActive);
    }
}
