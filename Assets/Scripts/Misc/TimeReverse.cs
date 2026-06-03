using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UsefulClasses;
public class TimeReverse : MonoBehaviour
{
    private interface IRigidbodyWrapper
    {
        bool IsKinematic { get; set; }
    }
    private class Rigidbody3DWrapper : IRigidbodyWrapper
    {
        private readonly Rigidbody _rb;
        public Rigidbody3DWrapper(Rigidbody rb) => _rb = rb;

        public bool IsKinematic
        {
            get => _rb.isKinematic;
            set => _rb.isKinematic = value;
        }
    }
    private class Rigidbody2DWrapper : IRigidbodyWrapper
    {
        private readonly Rigidbody2D _rb;
        public Rigidbody2DWrapper(Rigidbody2D rb) => _rb = rb;

        public bool IsKinematic
        {
            get => _rb.bodyType == RigidbodyType2D.Kinematic;
            set => _rb.bodyType = value
                ? RigidbodyType2D.Kinematic
                : RigidbodyType2D.Dynamic;
        }
    }
    [SerializeField] private bool _alwaysReverseToStartPosition;
    [SerializeField] private bool _isReversingRealtime;
    [SerializeField] private Key _timeReverseTrigger = Key.L;
    [SerializeField] private bool _shouldReverseRotation = true;
    [SerializeField] private bool _forceKinematicWhenReverseEnds;
    [SerializeField] private bool _reverseWhilePressingDown;
    [field: SerializeField] public float MaxTimeCaptured { get; private set; }
    [Tooltip("The Interval in which the reversing occurs")]
    [SerializeField] private UnityTimer _reverseInterval; // Passed time in a set interval
    [Tooltip("The time allowed to reverse when not reversing in realtime")]
    [SerializeField] private UnityTimer _reverseAmountTimer;
    private IRigidbodyWrapper _objectRbWrapper;
    private LinkedList<TransformSnapshot> _snapshots = new LinkedList<TransformSnapshot>();
    public bool IsReversing { get; private set; }
    private float _currentTimeCaptured; // Real passed time
    private float _currentElapsedTime;
    private const float _maxTimeResetValue = 6; // Just for the Reset Method
    private bool _startReversing;
    private bool _isKinematicTmp;
    public event Action OnReverseStart;
    public event Action OnReverseStep;
    public event Action OnReverseEnd;
    private CircularBuffer<int> _circularTest = new CircularBuffer<int>(3);
    private void Reset()
    {
        MaxTimeCaptured = _maxTimeResetValue;
        _isReversingRealtime = true;
        _reverseAmountTimer = new UnityTimer(_maxTimeResetValue);
    }
    public void SetDefaultValues()
    {
        _timeReverseTrigger = Key.L;
        MaxTimeCaptured = _maxTimeResetValue;
        _isReversingRealtime = true;
        _reverseAmountTimer = new UnityTimer(_maxTimeResetValue);
    }
    void Start()
    {
        ConfigureRigidbody();
    }
    private void ConfigureRigidbody()
    {
        if (TryGetComponent(out Rigidbody rigidbody))
        {
            _objectRbWrapper = new Rigidbody3DWrapper(rigidbody);
        }
        else if (TryGetComponent(out Rigidbody2D rigidbody2D))
        {
            _objectRbWrapper = new Rigidbody2DWrapper(rigidbody2D);
        }
        if (_objectRbWrapper != null)
        {
            _isKinematicTmp = _objectRbWrapper.IsKinematic;
        }
    }
    void Update()
    {
        StoreSnapshot();
        StartReversing();
        TriggerReverse();
        ReverseMovement();
    }
    private void TriggerReverse()
    {
        if (Keyboard.current[_timeReverseTrigger].wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)
        {
            _startReversing = true;
        }
        if (_reverseWhilePressingDown && (Keyboard.current[_timeReverseTrigger].wasReleasedThisFrame || Mouse.current.rightButton.wasReleasedThisFrame))
        {
            StopReversing();
        }
    }
    public void ActivateReverse()
    {
        _startReversing = true;
    }
    private void SetKinematic(bool isKinematic)
    {
        if (_forceKinematicWhenReverseEnds && !isKinematic && _objectRbWrapper != null)
        {
            _objectRbWrapper.IsKinematic = true;
        }
        else if (_objectRbWrapper != null)
        {
            _objectRbWrapper.IsKinematic = isKinematic;
        }
    }
    private void StartReversing()
    {
        if (_startReversing && !IsReversing)
        {
            OnReverseStart?.Invoke();
            SetKinematic(true);
            IsReversing = true;
            if (_snapshots.First != null)
            {
                _currentElapsedTime = _snapshots.First.Value.deltaTime;
            }
        }
    }
    private void StoreSnapshot()
    {
        if (IsReversing) return;

        if (_snapshots.First != null)
        {
            TransformSnapshot last = _snapshots.First.Value;
            if (last.position == transform.position && last.rotation == transform.rotation)
            {
                return;
            }
        }
        _snapshots.AddFirst(new TransformSnapshot(
            transform.position,
            transform.rotation,
            Time.deltaTime
        ));
        //if (_snapshots.Count == 1) 
        //{
        //    Debug.Log("Gameobject "+ gameObject.name + " " + _snapshots.First.Value.position);

        //}
        _currentTimeCaptured += Time.deltaTime;
        if (_currentTimeCaptured > MaxTimeCaptured && _snapshots.Last != null && !_alwaysReverseToStartPosition)
        {
            _currentTimeCaptured -= _snapshots.Last.Value.deltaTime;
            _snapshots.RemoveLast();
        }
    }
    private void ReverseMovement()
    {
        if (!IsReversing)
        {
            return;
        }
        if ((!_alwaysReverseToStartPosition && ReverseTimerExpired()) || _snapshots.Count == 0)
        {
            Debug.Log("Name: " +  gameObject.name + " " + _snapshots.Count);
            StopReversing();
            _snapshots.Clear();
            return;
        }
        if (CanStepBackwards())
        {
            ApplySnapshot();
        }
    }
    private bool ReverseTimerExpired()
    {
        _reverseAmountTimer.Tick();
        return _reverseAmountTimer.IsFinishedAndReset();
    }
    private bool CanStepBackwards()
    {
        if (_isReversingRealtime)
        {
            _currentElapsedTime -= Time.deltaTime;
            if (_currentElapsedTime <= 0)
            {
                if (_snapshots.First != null)
                {
                    _currentElapsedTime = _snapshots.First.Value.deltaTime;
                }
                return true;
            }
            return false;
        }
        else
        {
            _reverseInterval.Tick();
            Debug.Log( "Gamobecjt " + gameObject.name + " " + _reverseInterval.IsFinished());
            return _reverseInterval.IsFinishedAndReset();
        }
    }

    private void ApplySnapshot()
    {
        TransformSnapshot snapshot = _snapshots.First.Value;
        transform.position = snapshot.position;
        if (_shouldReverseRotation)
        {
            transform.rotation = snapshot.rotation;
        }
        _snapshots.RemoveFirst();
        OnReverseStep?.Invoke();
    }

    private void StopReversing()
    {
        _currentTimeCaptured = 0;
        _startReversing = false;
        IsReversing = false;
        SetKinematic(_isKinematicTmp);
        OnReverseEnd?.Invoke();
    }
}
public class CircularBuffer<T>
{
    private readonly T[] _buffer;
    private int _head;      // Index des nächsten Eintrags (neuester Eintrag liegt bei head-1)
    private int _count;     // Wie viele Einträge aktuell im Buffer

    public CircularBuffer(int capacity)
    {
        if (capacity <= 0)
            throw new System.ArgumentOutOfRangeException(nameof(capacity));

        _buffer = new T[capacity];
        _head = 0;
        _count = 0;
    }

    public int Count => _count;
    public bool IsEmpty => _count == 0;
    public bool IsFull => _count == _buffer.Length;

    public void Add(T item)
    {
        _buffer[_head] = item;

        if (_count < _buffer.Length)
        {
            _count++;
        }
        else
        {
            // Buffer voll: ältester Eintrag wird überschrieben
            _head++;
            if (_head == _buffer.Length)
                _head = 0;
        }
    }

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= _count)
                throw new System.ArgumentOutOfRangeException(nameof(index));

            int i = (_head - _count + index);
            if (i < 0)
                i += _buffer.Length;
            return _buffer[i];
        }
    }

    /// <summary>
    /// Entfernt den ältesten Eintrag (Index 0). Count verringert sich.
    /// </summary>
    public bool RemoveFirst()
    {
        if (_count == 0)
            return false;

        // head bleibt gleich, count wird kleiner
        _count--;

        return true;
    }

    /// <summary>
    /// Entfernt den jüngsten Eintrag (Index Count-1). Count verringert sich.
    /// </summary>
    public bool RemoveLast()
    {
        if (_count == 0)
            return false;

        // head einen Schritt zurücksetzen
        _head--;
        if (_head < 0)
            _head = _buffer.Length - 1;

        _count--;
        return true;
    }

    public void Clear()
    {
        _head = 0;
        _count = 0;
    }
}

