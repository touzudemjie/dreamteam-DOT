using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
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
        public TextMeshProUGUI textBox;
        public Image dialogueSprite;
        public GameObject dialogueParent;
        public GameObject continueSign;
        public GameObject choicesObject;
    }
    [SerializeField] private DialogueReferences[] dialogueReferences;

    [SerializeField] private DialogueReferences _currentReference;
    public Key[] continueKeys;

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
    }
    public void SetNextDialogueObject(int index)
    {
        if (index > dialogueReferences.Length) return;
        _currentReference = dialogueReferences[index];
    }
    private void Update()
    {
    }
    public void SetLineReferences(DialogueLine line)
    {
        _currentReference.dialogueSprite.sprite = line.sprite;   
        _currentReference.nameText.text = line.speaker;
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
                    if (!go.CompareTag(Tag.Choices.ToString()))
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

                if (!component.gameObject.CompareTag(Tag.Choices.ToString()))
                {
                    component.gameObject.SetActive(isActive);
                }
            }
        }
    }
    public void ActivateText(bool isActive)
    {
        _currentReference.dialogueParent.SetActive(isActive);
        _currentReference.textBox.text = string.Empty;
        _currentReference.textBox.gameObject.SetActive(isActive);
    }
}
