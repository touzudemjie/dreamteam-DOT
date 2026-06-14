using UnityEngine;
using UnityEngine.InputSystem;

public class TextDisplayer : MonoBehaviour, IInteractable
{
    [SerializeField] private Dialogue _pages;
    [SerializeField] private int _dialogueReferenceIndex; // 1 = Tutorial, 2 = Journal
    [SerializeField] private Key[] _nextKeys;
    [SerializeField] private Key[] _prevKeys;
    [SerializeField] private Key[] _closeKeys;
    [SerializeField] private bool _instantText = true; // kein Typewriter
    [SerializeField] private bool _canNavigate = true; // Journal = true, Tutorial = false

    private int _currentPageIndex = 0;
    public static bool IsOpen { get; private set; }

    // Für Tutorial — wird von außen getriggert
    public void ShowTutorialText(int pageIndex = 0)
    {
        _currentPageIndex = pageIndex;
        Open();
    }

    // Für Journal — via IInteractable
    public void OnInteract()
    {
        if (!IsOpen)
            Open();
        else
            Close();
    }

    void Open()
    {
        IsOpen = true;
        TextDisplayManager.Instance.SetNextDialogueObject(_dialogueReferenceIndex);
        TextDisplayManager.Instance.ActivateAllReferences(true);
        ShowPage(_currentPageIndex);
    }

    public void Close()
    {
        IsOpen = false;
        TextDisplayManager.Instance.ActivateAllReferences(false);
    }

    void Update()
    {
        if (!IsOpen || !_canNavigate) return;

        bool next = false;
        bool prev = false;
        bool close = false; 
        foreach (Key key in _nextKeys)
            if (Keyboard.current[key].wasPressedThisFrame) next = true;
        foreach (Key key in _prevKeys)
            if (Keyboard.current[key].wasPressedThisFrame) prev = true;

        if (next && _currentPageIndex < _pages.dialogueLines.Length - 1)
        {
            _currentPageIndex++;
            ShowPage(_currentPageIndex);
        }
        else if (prev && _currentPageIndex > 0)
        {
            _currentPageIndex--;
            ShowPage(_currentPageIndex);
        }
        foreach (Key key in _closeKeys)
            if (Keyboard.current[key].wasPressedThisFrame) close = true;
        if (close)
        {
            Close();
        }
    }

    void ShowPage(int index)
    {
        DialogueLine line = _pages.dialogueLines[index];
        if (_instantText)
            TextDisplayManager.Instance.ShowText(line.textContent);

        TextDisplayManager.Instance.SetLineReferences(line);
    }
}