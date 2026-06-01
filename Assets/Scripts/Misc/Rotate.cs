using UnityEngine;
using UnityEngine.EventSystems;
using UsefulClasses;

public class Rotate : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private float _rotationSpeed;
    [SerializeField] private Quaternion _startRotation;
    [SerializeField] private Vector3 _rotationMovement;
    private float _dragZ;

    void Start()
    {
        transform.rotation = _startRotation;
    }

    void Update()
    {
        transform.Rotate(_rotationMovement * Time.deltaTime);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _dragZ = transform.position.z; 
    }
    public void OnDrag(PointerEventData eventData)
    {
        Vector3 screenPos = new Vector3(eventData.position.x, eventData.position.y, Helpers.Camera.nearClipPlane);
        Ray ray = Helpers.Camera.ScreenPointToRay(screenPos);

        // Plane auf der Z Position des Objekts:
        Plane plane = new Plane(Vector3.forward, new Vector3(0, 0, _dragZ));

        if (plane.Raycast(ray, out float distance))
        {
            transform.position = new Vector3(ray.GetPoint(distance).x, ray.GetPoint(distance).y, transform.position.z);
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("End Drag");
    }

}