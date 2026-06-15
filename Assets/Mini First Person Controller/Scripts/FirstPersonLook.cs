using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using UsefulClasses;

public class FirstPersonLook : MonoBehaviour
{
    [SerializeField]
    Transform character;
    public float sensitivity = 2;
    public float smoothing = 1.5f;

    Vector2 velocity;
    Vector2 frameVelocity;
    [SerializeField] private bool _canLockMouse;
    private TimeReverse _timeReverse;
    [SerializeField] private Vector2 _cursorPosition;
    private LinkedList<CameraSnapshot> _cameraSnapshots = new LinkedList<CameraSnapshot>();
    private FirstPersonMovement _firstPersonMovement;
    void Reset()
    {
        // Get the character from the FirstPersonMovement in parents.
        character = GetComponentInParent<FirstPersonMovement>().transform;
        _firstPersonMovement = GetComponentInParent<FirstPersonMovement>();
        character.TryGetComponent(out _timeReverse);
    }

    void Start()
    {
        _firstPersonMovement = GetComponentInParent<FirstPersonMovement>();
        character.TryGetComponent(out _timeReverse);
        if (_canLockMouse)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    void Update()
    {
        if (_firstPersonMovement.CanMove)
        {
            Rotate();
            ChangeMousePosition();
        }
    }
    public void ChangeMousePosition()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            Mouse.current.WarpCursorPosition(_cursorPosition);
        }
    }
    private void Rotate()
    {
        Vector2 mouseDelta = Mouse.current?.delta.ReadValue() ?? default;
        Vector2 rawFrameVelocity = Vector2.Scale(mouseDelta, Vector2.one * sensitivity);
        frameVelocity = Vector2.Lerp(frameVelocity, rawFrameVelocity, 1 / smoothing);
        velocity += frameVelocity;
        velocity.y = Mathf.Clamp(velocity.y, -90, 90);
        transform.localRotation = Quaternion.AngleAxis(-velocity.y, Vector3.right);
        character.localRotation = Quaternion.AngleAxis(velocity.x, Vector3.up);
    }

}

