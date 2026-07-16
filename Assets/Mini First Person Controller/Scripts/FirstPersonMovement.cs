using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;
using UsefulClasses;
public class FirstPersonMovement : MonoBehaviour
{
    public float speed = 5;

    [Header("Running")]
    public bool canRun = true;
    public bool IsRunning { get; private set; }
    public float runSpeed = 9;
    private InputActionAsset _playerActions;

    Rigidbody playerRb;
    /// <summary> Functions to override movement speed. Will use the last added override. </summary>
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    [HideInInspector] public bool isGrappling;

    private InputAction _sprintAction;
    private InputAction _moveAction;
    public bool CanMove { get; private set; }
    void Awake()
    {
        // Get the rigidbody on this.
        playerRb = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        _playerActions = InputSystem.actions;
        _playerActions.FindActionMap("Player").Enable();
        _sprintAction = PlayerInputHandler.GetInputAction(PlayerInputHandler.PlayerAction.Sprint);
        _moveAction = PlayerInputHandler.GetInputAction(PlayerInputHandler.PlayerAction.Move);

    }


    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        CanMove = NPCDialogue.IsDialogueFinished;
        if (CanMove)
        {
            // Update IsRunning from input.
            IsRunning = canRun && _sprintAction.IsPressed();

            // Get targetMovingSpeed.
            float targetMovingSpeed = IsRunning ? runSpeed : speed;
            if (speedOverrides.Count > 0)
            {
                targetMovingSpeed = speedOverrides[speedOverrides.Count - 1]();
            }
            Vector2 targetVelocity = _moveAction.ReadValue<Vector2>() * targetMovingSpeed;
            if (!playerRb.isKinematic)
            {
                Debug.Log("Velocity " + playerRb.linearVelocity);
                playerRb.linearVelocity = transform.rotation * new Vector3(targetVelocity.x, playerRb.linearVelocity.y, targetVelocity.y);
            }
        }
    }
    private void Update()
    {
        if (Mouse.current != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit))
            {
                // print("HIT : " + hit.transform.gameObject.name);
            }
        }


        // transform the forward vector from local to world space
        // Vector3 forward = transform.TransformDirection(Vector3.forward);
        // // calculate a unit vector from the other object to this object
        //// Vector3 toOther = Vector3.Normalize(enteTr.position - transform.position);
        // // use the dot product sign to determine whether other is in front or behind
        // if (Vector3.Dot(forward, toOther) < 0)
        // {
        //     //print("The other transform is behind me!");
        // }

    }
    //void OnDrawGizmos()
    //{
    //    if (Camera.main == null) return;
    //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

    //    // Draw the ray (arbitrary length, e.g. 100 units for visualization)
    //    Gizmos.color = Color.green;
    //    Gizmos.DrawRay(ray.origin, ray.direction * 100);

    //    // Perform the raycast in the Scene view
    //    if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, 100))
    //    {
    //        // Draw a red line from the origin to the hit point
    //        Gizmos.color = Color.red;
    //        Gizmos.DrawLine(ray.origin, hit.point);

    //        // Draw a sphere at the hit point
    //        Gizmos.DrawSphere(hit.point, 0.2f);
    //    }
    //}
    public void JumpToPosition(Vector3 startPosition, Vector3 targetPosition, float trajectoryHeight)
    {
        playerRb.linearVelocity = CalculateJumpVelocity(startPosition, targetPosition, trajectoryHeight);
    }
    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity) + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));
        return velocityXZ + velocityY;
    }
}