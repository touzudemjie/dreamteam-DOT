using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UsefulClasses;

public class ButtonNavigationScript : MonoBehaviour
{
    private Button[] _menuButtons;
    private int _selectedIndex = 0;
    [SerializeField] private Sprite _normalSprite;
    [Space]
    [SerializeField] private Sprite _selectedSprite;
    [SerializeField] private Sprite _pressedSprite;
    [SerializeField] private float[] _arrowYPosition = {130f,50f,-30 };
    [SerializeField] private RectTransform _arrowPosition;
    [SerializeField] private UnityTimer _blinkTimer;
    private bool _isArrowOn = true;
    private int _pressedIndex;
    public bool canNavigate = true;
    void Start()
    {
        SetAllButtons();
        _blinkTimer.PrepareStart();
        UpdateSelection();
    }
    public void SetAllButtons()
    {
        _menuButtons = new Button[transform.childCount];
        for (int i = 0; i < _menuButtons.Length; i++)
        {
            _menuButtons[i] = transform.GetChild(i).GetComponent<Button>();
        }
    }
    //&& !UNITY_EDITOR
    // Update is called once per frame
    void Update()
    {
        if (canNavigate)
        {
            NavigateButtons();
            BlinkArrow();
        }
    }
    private void BlinkArrow()
    {
        _blinkTimer.TickUnscaled();
        if (_blinkTimer.IsFinished())
        {
            bool stayActive = _isArrowOn ? false : true;
            if(_arrowPosition != null)
            {
                _arrowPosition.gameObject.SetActive(stayActive);
            }
            _isArrowOn = !_isArrowOn;
            _blinkTimer.PrepareStart();
        }
    }
    private void OnDisable()
    {
        if(_arrowPosition != null)
        {
            _blinkTimer.PrepareStart();
            _arrowPosition.gameObject.SetActive(true);
            _isArrowOn = true;
        }
    }
    private void NavigateButtons()
    {
        bool up = Keyboard.current[Key.UpArrow].wasPressedThisFrame
               || Keyboard.current[Key.W].wasPressedThisFrame;

        bool down = Keyboard.current[Key.DownArrow].wasPressedThisFrame
                 || Keyboard.current[Key.S].wasPressedThisFrame;

        bool confirm = Keyboard.current[Key.Enter].wasPressedThisFrame
                    || Keyboard.current[Key.NumpadEnter].wasPressedThisFrame
                    || Keyboard.current[Key.K].wasPressedThisFrame
                    || Keyboard.current[Key.Space].wasPressedThisFrame;

        bool confirmUp = Keyboard.current[Key.Enter].wasReleasedThisFrame
                      || Keyboard.current[Key.NumpadEnter].wasReleasedThisFrame
                      || Keyboard.current[Key.K].wasReleasedThisFrame
                      || Keyboard.current[Key.Space].wasReleasedThisFrame;

        if (_menuButtons.Length > 0)
        {
            if (up)
            {
                _selectedIndex = (_selectedIndex - 1 + _menuButtons.Length) % _menuButtons.Length;
                SetArrow();
                UpdateSelection();
            }
            if (down)
            {
                _selectedIndex = (_selectedIndex + 1) % _menuButtons.Length;
                SetArrow();
                UpdateSelection();
            }
            if (confirm)
            {
                SetArrow();
                if (_pressedSprite != null)
                {
                    Image image = _menuButtons[_selectedIndex].targetGraphic as Image;
                    image.sprite = _pressedSprite;
                    _pressedIndex = _selectedIndex;
                }
            }
            if (confirmUp)
            {
                Debug.Log("Selected Index " + _selectedIndex);
                if (_pressedIndex == _selectedIndex && _menuButtons[_selectedIndex].enabled)
                    _menuButtons[_selectedIndex].onClick.Invoke();
            }
        }
    }
    private void SetArrow()
    {
        if(_arrowPosition != null)
        {
            _arrowPosition.anchoredPosition = new Vector2(_arrowPosition.anchoredPosition.x, _arrowYPosition[_selectedIndex]);
        }
    }
    private void UpdateSelection()
    {
        if (_menuButtons != null)
        {
            for (int i = 0; i < _menuButtons.Length; i++)
            {
                Image buttonImage = _menuButtons[i].targetGraphic as Image;
                if (buttonImage != null)
                {
                    buttonImage.sprite = (i == _selectedIndex) ? _selectedSprite : _normalSprite;
                }
            }
        }
    }
    private void OnEnable()
    {
        _selectedIndex = 0;
        if(_arrowPosition != null)
        {
            _arrowPosition.anchoredPosition = new Vector2(_arrowPosition.anchoredPosition.x, _arrowYPosition[0]);
        }
        UpdateSelection();
    }
}
