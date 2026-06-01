using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    [HideInInspector] public LineRenderer lineRenderer;
    [SerializeField] private bool _needCollider;
    private EdgeCollider2D _edgeCollider;
    private List<Vector2> _points = new List<Vector2>();
    [SerializeField] private float _destroyTime;
    private void OnEnable()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if(_needCollider)
        {
            GameObject collider = new GameObject("Collider");
            collider.transform.parent = transform;
           _edgeCollider =  collider.AddComponent<EdgeCollider2D>();
            collider.transform.localPosition = -transform.position;
            
        }
    }
    void Start()
    {
        Destroy(gameObject, _destroyTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setposition(Vector3 position)
    {
        if (!CanAppend(position)) return;

        lineRenderer.positionCount++;
        lineRenderer.SetPosition(lineRenderer.positionCount -1, position);
        if (_edgeCollider != null) 
        {
            Vector3[] positions = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(positions);
            _edgeCollider.points = System.Array.ConvertAll(positions, p => new Vector2(p.x, p.y));
        }
    }
    private bool CanAppend(Vector2 position) 
    {
        if (lineRenderer.positionCount == 0) return true;
      //  Debug.Log(Vector2.Distance(_lineRenderer.GetPosition(_lineRenderer.positionCount - 1), position) > DrawManager.RESOLUTION);
        return Vector2.Distance(lineRenderer.GetPosition(lineRenderer.positionCount - 1), position) > DrawManager.RESOLUTION;
    }


}
