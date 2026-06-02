using UnityEngine;
using UsefulClasses;

public class GhostLine : MonoBehaviour
{
    public Vector3[] Points; 
    private LineRenderer _ghostLine;
    [SerializeField] private DrawManager _drawManager;
    private LineRenderer _playerLine;
    [Range(0,1f)]
    [SerializeField] private float _expectedAccuracyPercentage;
    [SerializeField] private float _drawAccuracy;
    [SerializeField] private UnityTimer _resetColorTimer;
    [SerializeField] private float _lineClosedThreshold;
    private int _expectedAccuracyCount;
    private bool _hasWon;
    private bool _hasLost;
    private void OnEnable()
    {
        _ghostLine = GetComponent<LineRenderer>();
        DrawGhostLine();
    }
    private void Start()
    {
        _resetColorTimer.PrepareStart();
        _expectedAccuracyCount = Mathf.RoundToInt(_ghostLine.positionCount * _expectedAccuracyPercentage);
    }
    public void DrawGhostLine()
    {
        if (Points == null || Points.Length == 0) return;
        _ghostLine.positionCount = Points.Length;
        _ghostLine.SetPositions(Points);
    }
    private void Update()
    {
        EvaluateLine();
        ResetColor();
        GetCurrentPlayerline();
    }
    private void GetCurrentPlayerline()
    {
        if(_drawManager.currentLine != null)
        {
            _playerLine = _drawManager.currentLine.lineRenderer;
        }
    }
    private void ResetColor()
    {
        if (_hasWon || _hasLost)
        {
            _resetColorTimer.Tick();
            if (_resetColorTimer.IsFinishedAndReset())
            {
                _hasWon = false;
                _hasLost = false;
                _resetColorTimer.PrepareStart();
                if(_playerLine != null)
                {
                    Destroy(_playerLine.gameObject);
                }
                _ghostLine.startColor = Color.white;
                _ghostLine.endColor = Color.white;
            }
        }
    }
    private void EvaluateLine()
    {
        if (_playerLine == null) return;
        if (!IsPlayerlineClosed(_playerLine, _lineClosedThreshold) || _hasWon || _hasLost) return;

        int correctIndex = 0;

        for (int i = 0; i < _ghostLine.positionCount; i++)
        {
            Vector2 ghostPos = _ghostLine.GetPosition(i);
            float minDistance = float.MaxValue;

            for (int j = 0; j < _playerLine.positionCount; j++)
            {
                Vector2 playerPos = _playerLine.GetPosition(j);
                float dist = Vector2.Distance(ghostPos, playerPos);
                if (dist < minDistance)
                    minDistance = dist;
            }
            Debug.Log(minDistance);
            if (minDistance < _drawAccuracy)
                correctIndex++;
        }

        if (correctIndex >= _expectedAccuracyCount)
        {
            _ghostLine.startColor = Color.green;
            _ghostLine.endColor = Color.blue;
            _hasWon = true;
            Debug.LogError("Gewonnen");
            return;
        }

        Debug.Log("Verloren " + correctIndex);
        _ghostLine.startColor = Color.red;
        _ghostLine.endColor = Color.pink;
        _hasLost = true;
    }

    private bool IsPlayerlineClosed(LineRenderer playerLine, float threshold)
    {
        if (playerLine.positionCount < 50)
        {
            return false; 
        }
        Vector3 firstPosition = playerLine.GetPosition(0);
        Vector3 lastPosition = playerLine.GetPosition(playerLine.positionCount - 1);

        return Vector2.Distance(firstPosition, lastPosition) < threshold;
    }
}