using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UsefulClasses;

public class TimeReverse : MonoBehaviour
{
    [SerializeField] private bool _alwaysReverseToStartPosition;
    [SerializeField] private bool _isReversingRealtime;
    [SerializeField] private UnityTimer _reverseInterval; // Passed time in a set interval
    [field:SerializeField] public float MaxTimeCaptured { get; private set; }
    [SerializeField] private UnityTimer _reverseAmountTimer;
    private Rigidbody _objectRb;
    private LinkedList<TransformSnapshot> _snapshotList = new LinkedList<TransformSnapshot>();
    public bool IsReversing { get; private set; }
    private float _currentTimeCaptured; // Real passed time
    private float _currentElapsedTime;
    private const float _maxTimeResetValue = 6; // Just for the Reset Method
    private bool _startReversing;
    [SerializeField] private Key _timeReverseTrigger;
    [SerializeField] private bool _shouldReverseRotation = true;
    [SerializeField] private bool _forceKinematicWhenReverseEnds;
    private bool _isKinematicTmp;
    private void Reset()
    {
        MaxTimeCaptured = _maxTimeResetValue;
        _isReversingRealtime = true;
        _reverseAmountTimer = new UnityTimer(_maxTimeResetValue);
    }
    public void SetDefaultValues()
    {
        MaxTimeCaptured = _maxTimeResetValue;
        _isReversingRealtime = true;
        _reverseAmountTimer = new UnityTimer(_maxTimeResetValue);
    }
    void Start()
    {
        _reverseAmountTimer.PrepareStart();
        _reverseInterval.PrepareStart();
        _objectRb = GetComponent<Rigidbody>();
        _isKinematicTmp = _objectRb.isKinematic;
    }
    void Update()
    {
        StoreSnapshot();
        StartReversing();
        ReverseMovement();
        TriggerReverse();

    }
    private void TriggerReverse()
    {
        if (Keyboard.current[_timeReverseTrigger].wasPressedThisFrame)
        {
            _startReversing = true;
        }
    }
    public void ActivateReverse()
    {
        _startReversing = true;
    }
    private void SetKinematic(bool isKinematic)
    {
        if (_forceKinematicWhenReverseEnds && !isKinematic && _objectRb != null)
        {
            _objectRb.isKinematic = true;
        }
        else if (_objectRb != null)
        {
            _objectRb.isKinematic = isKinematic;
        }
    }
    private void StartReversing()
    {
        if (_startReversing && !IsReversing)
        {
            SetKinematic(true);
            IsReversing = true;
            if (_snapshotList.First != null)
            {
                _currentElapsedTime = _snapshotList.First.Value.deltaTime;
            }
        }
    }
    private void StoreSnapshot()
    {
        if (IsReversing) return;

        if (_snapshotList.First != null)
        {
            TransformSnapshot last = _snapshotList.First.Value;
            if (last.position == transform.position && last.rotation == transform.rotation)
            {
                return;
            }
        }
        _snapshotList.AddFirst(new TransformSnapshot(
            transform.position,
            transform.rotation,
            Time.deltaTime
        ));
        _currentTimeCaptured += Time.deltaTime;
        if (_currentTimeCaptured > MaxTimeCaptured && _snapshotList.Last != null && !_alwaysReverseToStartPosition)
        {
            _currentTimeCaptured -= _snapshotList.Last.Value.deltaTime;
            _snapshotList.RemoveLast();
        }
    }
    private void ReverseMovement()
    {
        if (!IsReversing || _snapshotList.Count == 0)
        {
            if (_snapshotList.Count == 0 && IsReversing)
            {
                _currentTimeCaptured = 0;
                SetKinematic(_isKinematicTmp);
                IsReversing = false;
                _startReversing = false;
                _reverseAmountTimer.PrepareStart();
            }
            return;
        }
        bool canMoveBackwards = false;
        if (!_alwaysReverseToStartPosition)
        {
            _reverseAmountTimer.Tick();
            if (_reverseAmountTimer.IsFinished())
            {
                IsReversing = false;
                SetKinematic(_isKinematicTmp);
                _currentTimeCaptured = 0;
                _reverseAmountTimer.PrepareStart();
                _snapshotList.Clear();
                return;
            }
        }
        if (_isReversingRealtime)
        {
            _currentElapsedTime -= Time.deltaTime;
            if (_currentElapsedTime <= 0)
            {
                canMoveBackwards = true;
                if (_snapshotList.First != null)
                {
                    _currentElapsedTime = _snapshotList.First.Value.deltaTime;
                }
            }
        }
        else
        {
            _reverseInterval.Tick();
            if (_reverseInterval.IsFinished())
            {
                canMoveBackwards = true;
                _reverseInterval.PrepareStart();
            }
        }
        if (canMoveBackwards && _snapshotList.First != null)
        {
            TransformSnapshot snapshot = _snapshotList.First.Value;
            transform.position = snapshot.position;
            if (_shouldReverseRotation)
            {
                transform.rotation = snapshot.rotation;
            }
            _snapshotList.RemoveFirst();
        }
    }
}
