using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Device;

public class PortalTeleportScript : MonoBehaviour
{
    [SerializeField] private Transform _playerTr;
    [SerializeField] private Transform _goalPositionTr;
    private bool _isOverlapping;
    private float _previousDot;
    private Vector3 _previousOffsetFromPortal;
    [Tooltip("The camera where i want to teleport")]
    [SerializeField] private Camera _portalCamera;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Teleport();

    }
    private void LateUpdate()
    {
    }

    void AdjustNearClip()
    {
        // Abstand zwischen Portal-Kamera und Portal-Plane
        float distanceToPortal = Vector3.Dot(
            _portalCamera.transform.position - transform.position,
            transform.up
        );
       if(distanceToPortal < 0.01)
        {
            Debug.LogError("Ist kleiner" + distanceToPortal );
        }
        // Near Clip genau auf die Portal-Oberfläche setzen
        _portalCamera.nearClipPlane = Mathf.Max(0.01f, distanceToPortal);
    }
    private void Teleport()
    {
        Vector3 portalToPlayer = _playerTr.position - transform.position;
        float dotProduct = Vector3.Dot(transform.up, portalToPlayer);
        if (_isOverlapping)
        {
            if (dotProduct < 0 && _previousDot > 0)
            {
                var m = _goalPositionTr.transform.localToWorldMatrix * transform.parent.worldToLocalMatrix * _playerTr.localToWorldMatrix;
                _playerTr.position = m.GetColumn(3);
                _playerTr.rotation = m.rotation; 
                
               // Debug.LogError("PlayerPosition " + _playerTr.position);
            }
        }
        _previousDot = dotProduct;
    }
    float ProtectScreenFromClipping(Vector3 viewPoint)
    {
        float halfHeight = Camera.main.nearClipPlane * Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float halfWidth = halfHeight * Camera.main.aspect;
        float screenThickness = new Vector3(halfWidth, halfHeight, Camera.main.nearClipPlane).magnitude;

        bool camFacingSameDirAsPortal = Vector3.Dot(transform.forward, transform.position - viewPoint) > 0;

        // Z-Achse weil localPosition immer relativ zur eigenen Rotation ist
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, screenThickness);
        transform.localPosition = new Vector3(0, 0, screenThickness * (camFacingSameDirAsPortal ? 0.5f : -0.5f));

        return screenThickness;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Vector3 portalToPlayer = _playerTr.position - transform.position;

            _isOverlapping = true;

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isOverlapping = false;
        }
    }
}
