using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueMangerScript : MonoBehaviour
{
    [System.Serializable]
    public class DialogueReferences
    {
        [SerializeField] public GameObject dialoguePanel;
        [SerializeField] public TextMeshProUGUI nameText;
        [SerializeField] public TextMeshProUGUI textBox;
        [SerializeField] public Image dialogueSprite;
        [SerializeField] public GameObject dialogueObject;
        public GameObject continueSign;
    }
    [SerializeField] private DialogueReferences[] dialogueReferences;

    public DialogueReferences currentReference;
    public static DialogueMangerScript instance;
    private GameObject[] dialogObjects;
    private void Awake()
    {
        if (instance != this && instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
        }
    }
    void Start()
    {
        currentReference = dialogueReferences[0];
    }
    private void GetAllGameObjects()
    {
        int index = 0;
        dialogObjects = new GameObject[transform.childCount];
        foreach (Transform child in gameObject.transform)
        {
            dialogObjects[index] = child.gameObject;
            index++; 
        }
    }
    public void SetNextDialogueObject(int index)
    {
        if (index > dialogueReferences.Length) return;
        currentReference = dialogueReferences[index];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
