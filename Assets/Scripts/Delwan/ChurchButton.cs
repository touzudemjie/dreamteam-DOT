using UnityEngine;
using UsefulClasses;

public class ChurchButton : MonoBehaviour, IInteractable 
{
    private static int _churchButtonslength;
    private static int _pressedButtonCount;
    private bool _isButtonPressed;
    [SerializeField] private Color _pressedColor;
    [SerializeField] private UnityTimer _colorToDefaultTimer;
    private Material _buttonMaterial;
    private Color _startColor;
    void Start()
    {
        _churchButtonslength++;
        _buttonMaterial = GetComponent<MeshRenderer>().material;
        _startColor = _buttonMaterial.color;
    }
    private void OnDestroy()
    {
        _churchButtonslength = Mathf.Max(0, _churchButtonslength - 1);
        if (_isButtonPressed)
        {
            _pressedButtonCount = Mathf.Max(0, _pressedButtonCount - 1);
        }
    }
    // Update is called once per frame
    void Update()
    {
        SetColorToDefault();
    }
    public void OnInteract()
    {
        ActivateButton();
    }
    private void SetColorToDefault()
    {
        if (_isButtonPressed)
        {
            _colorToDefaultTimer.Tick();
            if (_colorToDefaultTimer.IsFinishedAndReset())
            {
                Debug.Log("Set color to default");
                _isButtonPressed = false;
              //  _colorToDefaultTimer.PrepareStart();
                _pressedButtonCount--;
                _buttonMaterial.color = _startColor;
            }
        }
    }
    public void ActivateButton()
    {
        if (_isButtonPressed)
        {
            return;
        }
        Debug.Log("Activat Button");
        _isButtonPressed = true;
        _pressedButtonCount++;
        _buttonMaterial.color = _pressedColor;
        if (_churchButtonslength == _pressedButtonCount)
        {
            Debug.LogWarning("All buttons are pressed");
            // ChurchDoor.Instance.OpenDoor();
        }
    }


}
