using UnityEngine;
using UnityEngine.InputSystem;

public class KinematicBlock : MonoBehaviour
{
    [SerializeField] private GameObject _kinematicObject;
    private bool _isActive;
    private void Start()
    {
        _isActive = _kinematicObject.activeSelf;
    }
    void Update()
    {
        if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            _isActive = !_isActive;
            _kinematicObject.SetActive(_isActive);
        }
    }
}
