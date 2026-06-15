using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UsefulClasses;

public class ButtonNavigationScript : MonoBehaviour
{
    private Button[] _menuButtons;
    private List<Button> _activeButtons = new List<Button>();
    private int _selectedIndex = 0;
    [SerializeField] private Sprite _normalSprite;
    [Space]
    [SerializeField] private Sprite _selectedSprite;
    [SerializeField] private Sprite _pressedSprite;
    [SerializeField] private float[] _arrowYPosition = { 130f, 50f, -30 };
    [SerializeField] private RectTransform _arrowPosition;
    [SerializeField] private UnityTimer _blinkTimer;

    [SerializeField] private Key[] _confirmKeys;

    private int _activeButtonAmount;
    private int _previousActiveButtonAmount = -1;
    private bool _isArrowOn = true;
    private int _pressedIndex;
    public bool canNavigate = true;
    private bool _hasConfirmed;
    void Start()
    {
        SetAllButtons();
    }
    public void SetAllButtons()
    {
        _menuButtons = new Button[transform.childCount];
        for (int i = 0; i < _menuButtons.Length; i++)
        {
            _menuButtons[i] = transform.GetChild(i).GetComponent<Button>();
        }
    }
    void Update()
    {
        if (canNavigate)
        {
            EvaluateButtonAmount();
            NavigateButtons();
            BlinkArrow();
        }
    }
    void EvaluateButtonAmount()
    {
        int newAmount = 0;
        foreach (Button button in _menuButtons)
        {
            if (button.gameObject.activeSelf)
            {
                newAmount++;
            }
        }
        if (newAmount == _previousActiveButtonAmount) return;
        _previousActiveButtonAmount = newAmount;
        _activeButtons.Clear();
        foreach (var button in _menuButtons)
        {
            if (button.gameObject.activeSelf)
            {
                _activeButtons.Add(button);
            }
        }
        UpdateSelection();
        _activeButtonAmount = _activeButtons.Count;
    }

    private void BlinkArrow()
    {
        _blinkTimer.TickUnscaled();
        if (_blinkTimer.IsFinishedAndReset())
        {
            bool stayActive = _isArrowOn ? false : true;
            if (_arrowPosition != null)
            {
                _arrowPosition.gameObject.SetActive(stayActive);
            }
            _isArrowOn = !_isArrowOn;
        }
    }
   
    private void OnDisable()
    {
        if (_arrowPosition != null)
        {
            _blinkTimer.PrepareStart();
            _arrowPosition.gameObject.SetActive(true);
            _isArrowOn = true;
        }
        _hasConfirmed = false;
    }
    private void NavigateButtons()
    {
        bool left = Keyboard.current[Key.LeftArrow].wasPressedThisFrame
               || Keyboard.current[Key.A].wasPressedThisFrame;
        
        bool right = Keyboard.current[Key.RightArrow].wasPressedThisFrame
               || Keyboard.current[Key.D].wasPressedThisFrame;
        bool up = Keyboard.current[Key.UpArrow].wasPressedThisFrame
               || Keyboard.current[Key.W].wasPressedThisFrame;

        bool down = Keyboard.current[Key.DownArrow].wasPressedThisFrame
                 || Keyboard.current[Key.S].wasPressedThisFrame;

        bool pressedConfirmkey = false;
        foreach (Key key in _confirmKeys)
        {
            if (Keyboard.current[key].wasPressedThisFrame)
            {
                pressedConfirmkey = true;
                break;
            }
        }
        bool confirm = pressedConfirmkey;
        bool releasedConfirmKey = false;
        foreach (Key key in _confirmKeys)
        {
            if (Keyboard.current[key].wasReleasedThisFrame)
            {
                releasedConfirmKey = true;
                break;
            }
        }
        bool confirmUp = releasedConfirmKey;

        if (_activeButtonAmount > 0)
        {
            if (up || right)
            {
                _selectedIndex = (_selectedIndex - 1 + _activeButtonAmount) % _activeButtonAmount;
                SetArrow();
                UpdateSelection();
            }
            if (down || left)
            {
                _selectedIndex = (_selectedIndex + 1) % _activeButtonAmount;
                SetArrow();
                UpdateSelection();
            }
            if (confirm)
            {
                _hasConfirmed = true;
                SetArrow();
                if (_pressedSprite != null)
                {
                    Image image = _activeButtons[_selectedIndex].targetGraphic as Image;
                    image.sprite = _pressedSprite;
                }
                _pressedIndex = _selectedIndex;
            }
            if (confirmUp && _hasConfirmed)
            {
                _hasConfirmed = false;
                if (_pressedIndex == _selectedIndex && _activeButtons[_selectedIndex].enabled)
                {
                    _activeButtons[_selectedIndex].onClick.Invoke();
                }
            }
        }
    }
    private void SetArrow()
    {
        if (_arrowPosition != null)
        {
            _arrowPosition.anchoredPosition = new Vector2(_arrowPosition.anchoredPosition.x, _arrowYPosition[_selectedIndex]);
        }
    }

    private void UpdateSelection()
    {
        for (int i = 0; i < _activeButtons.Count; i++)
        {
            Image img = _activeButtons[i].targetGraphic as Image;
            if (img != null)
            {
                img.sprite = (i == _selectedIndex) ? _selectedSprite : _normalSprite;
            }
        }
    }

    private void OnEnable()
    {
        _hasConfirmed = false;
        _selectedIndex = 0;
        if (_arrowPosition != null)
        {
            _arrowPosition.anchoredPosition = new Vector2(_arrowPosition.anchoredPosition.x, _arrowYPosition[0]);
        }
    }
}