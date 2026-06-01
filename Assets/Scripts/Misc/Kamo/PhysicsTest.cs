using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UsefulClasses;

public class PhysicsTest : MonoBehaviour
{
    public enum Movement
    {
        Addforce,
        Velocity
    }
    [SerializeField] private float _movementSpeed;
    private Rigidbody _playerRb;
    [SerializeField] private Vector2 _direction;
    [SerializeField] private Movement _movement;
    [SerializeField] private float _yOffset;
    private InputAction _moveAction;
    private Vector3 _startPositon;
    [SerializeField] private ForceMode _forceMode;

    void Start()
    {
        _playerRb = GetComponent<Rigidbody>();
        Debug.Log("Local " + transform.worldToLocalMatrix);
        Debug.Log("World " + transform.localToWorldMatrix);
        _moveAction = PlayerInputHandler.GetInputAction(PlayerInputHandler.PlayerAction.Move);
        _moveAction.Enable();
        _startPositon = transform.position;
        ArrayIndizierung arrayIndizierung = new ArrayIndizierung();
        Debug.Log(arrayIndizierung[0]);
    }
    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            transform.position += Vector3.up * _yOffset;
        }
        if (Keyboard.current.oKey.wasPressedThisFrame)
        {
            transform.position = _startPositon;
            transform.rotation = Quaternion.identity;
        }
    }
    private void FixedUpdate()
    {
        Move();
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            Vector3 explosionPos = transform.position + Vector3.forward * 2f;

            _playerRb.AddExplosionForce(10, transform.position, 5f,1.5f,ForceMode.Impulse);
            
        }
    }
    private void Move() 
    {
        switch (_movement)
        {
            case Movement.Addforce:
                _playerRb.AddForce(_moveAction.ReadValue<Vector2>() *_direction * _movementSpeed, _forceMode);
                break;
            case Movement.Velocity:
                if(_playerRb.linearVelocity.y >= 0)
                {
                    _playerRb.linearVelocity = _moveAction.ReadValue<Vector2>() * _direction * _movementSpeed;
                }
                break;
        }
    }
}

public class ArrayIndizierung
{
    private int[] zahlen = { 1, 2, 3 };

    public int this[int index]
    {
        get
        {
            if (index < 0 || index >= zahlen.Length) throw new IndexOutOfRangeException();
            return zahlen[index];
        }
        set
        {
            zahlen[index] = value;
        }
    }
}
