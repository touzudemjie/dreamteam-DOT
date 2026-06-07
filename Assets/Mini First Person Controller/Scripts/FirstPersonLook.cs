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
    void Reset()
    {
        // Get the character from the FirstPersonMovement in parents.
        character = GetComponentInParent<FirstPersonMovement>().transform;
        character.TryGetComponent(out _timeReverse);
    }

    void Start()
    {
        character.TryGetComponent(out _timeReverse);
        if (_canLockMouse)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void Update()
    {
        Rotate();
        ChangeMousePosition();
    }
    public void ChangeMousePosition()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            Mouse.current.WarpCursorPosition(_cursorPosition);
        }
    }
    //private void Rotate()
    //{
    //    if (_timeReverse != null)
    //    {
    //        if (!_timeReverse.IsReversing)
    //        {
    //            // Get smooth velocity.
    //            _currentTimeCaptured += Time.deltaTime;
    //            Vector2 mouseDelta = default;
    //            if (Mouse.current != null)
    //            {
    //                mouseDelta = Mouse.current.delta.ReadValue();
    //                Vector2 mousePos = Mouse.current.position.ReadValue();
    //                bool isInScreen = mousePos.x > 0 && mousePos.x < Screen.width && mousePos.y > 0 && mousePos.y < Screen.height;
    //                if (_currentTimeCaptured < _timeReverse.MaxTimeCaptured && isInScreen)
    //                {
    //                    CameraSnapshot snapshot = new CameraSnapshot(Mouse.current.position.ReadValue(), Time.deltaTime);
    //                    _cameraSnapshots.AddFirst(snapshot);
    //                }
    //                else if (_currentTimeCaptured > _timeReverse.MaxTimeCaptured && isInScreen && _cameraSnapshots.Last != null)
    //                {
    //                    CameraSnapshot lastSnapshot = _cameraSnapshots.Last.Value;
    //                    _currentTimeCaptured -= lastSnapshot.timeDeltaTime;
    //                    _cameraSnapshots.RemoveLast();
    //                }
    //            }
    //            Vector2 rawFrameVelocity = Vector2.Scale(mouseDelta, Vector2.one * sensitivity);
    //            frameVelocity = Vector2.Lerp(frameVelocity, rawFrameVelocity, 1 / smoothing);
    //            velocity += frameVelocity;
    //            velocity.y = Mathf.Clamp(velocity.y, -90, 90);

    //            // Rotate camera up-down and controller left-right from velocity.
    //            transform.localRotation = Quaternion.AngleAxis(-velocity.y, Vector3.right);
    //            character.localRotation = Quaternion.AngleAxis(velocity.x, Vector3.up);
    //        }
    //        else
    //        {
    //            _currentReverseTimer -= Time.deltaTime;
    //            if (_currentReverseTimer < 0 && _cameraSnapshots.First != null)
    //            {
    //                _currentTimeCaptured = 0;
    //                Debug.Log("First " + _cameraSnapshots.First.Value.mousePosition);
    //                Mouse.current.WarpCursorPosition(_cameraSnapshots.First.Value.mousePosition);
    //                _currentReverseTimer = _cameraSnapshots.First.Value.timeDeltaTime;
    //                _cameraSnapshots.RemoveFirst();
    //            }
    //        }

    //    }
    //    else
    //    {
    //        Vector2 mouseDelta = default;
    //        if (Mouse.current != null)
    //        {
    //            mouseDelta = Mouse.current.delta.ReadValue();
    //        }
    //        Vector2 rawFrameVelocity = Vector2.Scale(mouseDelta, Vector2.one * sensitivity);
    //        frameVelocity = Vector2.Lerp(frameVelocity, rawFrameVelocity, 1 / smoothing);
    //        velocity += frameVelocity;
    //        velocity.y = Mathf.Clamp(velocity.y, -90, 90);
    //        // Rotate camera up-down and controller left-right from velocity.
    //        transform.localRotation = Quaternion.AngleAxis(-velocity.y, Vector3.right);
    //        character.localRotation = Quaternion.AngleAxis(velocity.x, Vector3.up);
    //    }
    //}

    private void Rotate()
    {
        //if (_timeReverse != null)
        //{
        //    if (!_timeReverse.IsReversing)
        //    {
        //        Vector2 mouseDelta = Mouse.current?.delta.ReadValue() ?? default;

        //        Vector2 rawFrameVelocity = Vector2.Scale(mouseDelta, Vector2.one * sensitivity);
        //        frameVelocity = Vector2.Lerp(frameVelocity, rawFrameVelocity, 1 / smoothing);
        //        velocity += frameVelocity;
        //        velocity.y = Mathf.Clamp(velocity.y, -90, 90);
        //        // Snapshot NACH dem Berechnen speichern
        //        _currentTimeCaptured += Time.deltaTime;
        //        if (_currentTimeCaptured < _timeReverse.MaxTimeCaptured)
        //        {
        //            _cameraSnapshots.AddFirst(new CameraSnapshot(velocity, Time.deltaTime));
        //        }
        //        else
        //        {
        //            if(_cameraSnapshots.Last != null)
        //            {
        //                CameraSnapshot last = _cameraSnapshots.Last.Value;
        //                _currentTimeCaptured -= last.timeDeltaTime;
        //                _cameraSnapshots.RemoveLast();
        //            }
        //        }
             
        //        transform.localRotation = Quaternion.AngleAxis(-velocity.y, Vector3.right);
        //        character.localRotation = Quaternion.AngleAxis(velocity.x, Vector3.up);
        //    }
        //    else // IsReversing
        //    {
        //        _currentReverseTimer -= Time.deltaTime;

        //        if (_currentReverseTimer < 0 && _cameraSnapshots.First != null)
        //        {
        //            CameraSnapshot snap = _cameraSnapshots.First.Value;
        //            _currentTimeCaptured = 0;
        //            // Direkt velocity setzen – kein Warpen nötig
        //            velocity = snap.velocity;
        //            Debug.Log("Test");
        //            transform.localRotation = Quaternion.AngleAxis(-velocity.y, Vector3.right);

        //            _currentReverseTimer = snap.timeDeltaTime;
        //            _cameraSnapshots.RemoveFirst();
        //        }
        //    }
        //}
        //else
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

}

