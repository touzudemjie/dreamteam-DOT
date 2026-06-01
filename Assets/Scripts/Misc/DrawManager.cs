using UnityEngine;
using UnityEngine.InputSystem;
using UsefulClasses;

public class DrawManager : MonoBehaviour
{
    public const float RESOLUTION = .001f;
    [SerializeField] private Line _linePrefab;
    [HideInInspector] public Line currentLine;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        DrawLine();
    }
    //private void DrawLine()
    //{
    //    Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
    //    Ray ray = Helpers.Camera.ScreenPointToRay(mouseScreenPos);

    //    if (Physics.Raycast(ray, out RaycastHit hitInfo))
    //    {
    //        if (Mouse.current.leftButton.wasPressedThisFrame)
    //        {
    //            //Debug.Log("normal " + hitInfo.normal);
    //            //Vector3 spawnPoint = hitInfo.point + hitInfo.normal *.5f;
    //            //Quaternion surfaceRotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);

    //            //currentLine = Instantiate(_linePrefab, spawnPoint, surfaceRotation);
    //            currentLine = Instantiate(_linePrefab, new Vector3(hitInfo.point.x, hitInfo.point.y, hitInfo.point.z), Quaternion.identity);

    //        }

    //        if (Mouse.current.leftButton.isPressed && currentLine != null)
    //            currentLine.Setposition(hitInfo.point);
    //    }

    //}

    private void DrawLine()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Ray ray = Helpers.Camera.ScreenPointToRay(mouseScreenPos);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector3 spawnPoint = hitInfo.point + hitInfo.normal * (_linePrefab.gameObject.GetComponent<LineRenderer>().widthMultiplier * 0.125f);
                currentLine = Instantiate(_linePrefab, spawnPoint, Quaternion.identity);
            }
            if (Mouse.current.leftButton.isPressed && currentLine != null)
            {
                // Offset auch beim Setposition mitgeben
                Vector3 offsetPoint = hitInfo.point + hitInfo.normal * (_linePrefab.gameObject.GetComponent<LineRenderer>().widthMultiplier * 0.125f);
                currentLine.Setposition(offsetPoint);
            }
        }
    }

    private void PaintOnPlane()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Ray ray = Helpers.Camera.ScreenPointToRay(mouseScreenPos);
        Plane plane = new Plane(Helpers.Camera.transform.forward, Helpers.Camera.transform.position + Vector3.forward);
        VisualizePlane(plane, Helpers.Camera.transform.position + Vector3.forward);
        if (plane.Raycast(ray, out float distance))
        {
            Vector3 intersectionPos = ray.GetPoint(distance);

            if (Mouse.current.leftButton.wasPressedThisFrame)
                currentLine = Instantiate(_linePrefab, new Vector3(intersectionPos.x, intersectionPos.y, intersectionPos.z), Quaternion.identity);

            if (Mouse.current.leftButton.isPressed && currentLine != null)
                currentLine.Setposition(intersectionPos);
        }
    }
    private void VisualizePlane(Plane plane, Vector3 center, float size = 3f)
    {
        // Normale der Plane visualisieren
        Debug.DrawRay(center, plane.normal * 2f, Color.blue);

        // Plane als Gitter visualisieren
        // Wir brauchen zwei Vektoren die senkrecht zur Normale sind
        Vector3 right = Vector3.Cross(plane.normal, Vector3.up).normalized * size;
        Vector3 up = Vector3.Cross(plane.normal, right).normalized * size;

        Vector3 c1 = center + right + up;
        Vector3 c2 = center - right + up;
        Vector3 c3 = center - right - up;
        Vector3 c4 = center + right - up;

        // Rahmen
        Debug.DrawLine(c1, c2, Color.green);
        Debug.DrawLine(c2, c3, Color.green);
        Debug.DrawLine(c3, c4, Color.green);
        Debug.DrawLine(c4, c1, Color.green);

        // Kreuz in der Mitte
        Debug.DrawLine(c1, c3, Color.yellow);
        Debug.DrawLine(c2, c4, Color.yellow);
    }
}
