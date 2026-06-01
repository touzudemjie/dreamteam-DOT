#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GhostLine))]
public class GhostLineEditor : Editor
{
    private const float HANDLE_SIZE = 0.1f;

    private void OnSceneGUI()
    {
        GhostLine ghostLine = (GhostLine)target;
        LineRenderer lr = ghostLine.GetComponent<LineRenderer>();
        Event e = Event.current;

        if (e.type == EventType.MouseUp && e.button == 0)
        {
            if (GUIUtility.hotControl != 0) return;
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Prüfen ob Klick nahe an einem existierenden Punkt
                if (!IsNearExistingPoint(hit.point, ghostLine.Points))
                {
                    Undo.RecordObject(ghostLine, "Add Line Point");
                    ArrayUtility.Add(ref ghostLine.Points, hit.point);
                    lr.positionCount = ghostLine.Points.Length;
                    lr.SetPosition(ghostLine.Points.Length - 1, hit.point);
                    e.Use();
                }
            }
        }

        if (ghostLine.Points != null)
        {
            lr.positionCount = ghostLine.Points.Length;
            for (int i = 0; i < ghostLine.Points.Length; i++)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 newPos = Handles.PositionHandle(ghostLine.Points[i], Quaternion.identity);
                if (EditorGUI.EndChangeCheck()) // Nur updaten wenn wirklich verschoben
                {
                    Undo.RecordObject(ghostLine, "Move Line Point");
                    ghostLine.Points[i] = newPos;
                    lr.SetPosition(i, newPos);
                }

                Handles.SphereHandleCap(0, ghostLine.Points[i],
                    Quaternion.identity, HANDLE_SIZE, EventType.Repaint);
            }
        }
    }

    private bool IsNearExistingPoint(Vector3 worldPos, Vector3[] points)
    {
        if (points == null) return false;
        foreach (Vector3 point in points)
        {
            // Vergleich in Screen-Space damit die Threshold unabhängig von Zoom ist
            Vector2 screenPos = HandleUtility.WorldToGUIPoint(worldPos);
            Vector2 screenPoint = HandleUtility.WorldToGUIPoint(point);
            if (Vector2.Distance(screenPos, screenPoint) < 20f) // 20 Pixel Threshold
                return true;
        }
        return false;
    }
}
#endif