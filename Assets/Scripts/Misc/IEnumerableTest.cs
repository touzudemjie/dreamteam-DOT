using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class IEnumerableTest : MonoBehaviour , IEnumerable<GameObject>
{
    [SerializeField] private GameObject[] objects;
    public IEnumerator<GameObject> GetEnumerator()
    {
        foreach (GameObject obj in objects)
        {
            yield return obj; 
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
public class GameobjectEnumerator : IEnumerator<GameObject>
{
    private int _currentIndex = -1;
    private GameObject[] _objects;
    public GameObject Current => _objects[_currentIndex];

    object IEnumerator.Current => Current;

    public GameobjectEnumerator(GameObject[] objects)
    {
        _objects = objects;
    }

    public void Dispose()
    {

    }

    public bool MoveNext()
    {
        _currentIndex++;
        return _currentIndex < _objects.Length;
    }

    public void Reset()
    {
        _currentIndex = -1;
    }
}

public static class TestClass
{
    #if UNITY_EDITOR
    [MenuItem("Developer/Debug")]
    public static void Debug()
    {
        UnityEngine.Debug.Log("Debug wurde aufgerufen!");
    }

    [MenuItem("Developer/Clear Console")]
    public static void ClearConsole()
    {
        var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor");

        if (logEntries == null)
        {
            UnityEngine.Debug.LogError("LogEntries Type nicht gefunden!");
            return;
        }

        var clearMethod = logEntries.GetMethod("Clear");

        if (clearMethod == null)
        {
            UnityEngine.Debug.LogError("Clear Methode nicht gefunden!");
            return;
        }

        clearMethod.Invoke(null, null);
    }
    [MenuItem("Developer/DeactivatePlayer")]
    public static void DeactivatePlayer()
    {
        UnityEngine.GameObject.FindWithTag("Player").SetActive(false);
    }

    [MenuItem("Developer/Take Screenshot")]
    public static void TakeScreenshot()
    {
        string folder = "Assets/Screenshots";

        if (!System.IO.Directory.Exists(folder))
            System.IO.Directory.CreateDirectory(folder);

        string path = $"{folder}/screenshot_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
        UnityEngine.ScreenCapture.CaptureScreenshot(path);
        UnityEngine.Debug.Log($"Screenshot gespeichert: {path}");
    }
    [MenuItem("Developer/Log FPS")]
    public static void LogFPS()
    {
        float fps = 1f / UnityEngine.Time.deltaTime;
        UnityEngine.Debug.Log($"Aktuelle FPS: {fps:F1}");
    }

    [MenuItem("Developer/Log Memory Usage")]
    public static void LogMemoryUsage()
    {
        long memory = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
        float mb = memory / 1024f / 1024f;
        UnityEngine.Debug.Log($"Genutzter RAM: {mb:F2} MB");
    }

    [MenuItem("Developer/Find Missing Scripts")]
    public static void FindMissingScripts()
    {
        var all = UnityEngine.Object.FindObjectsByType<UnityEngine.GameObject>(
            UnityEngine.FindObjectsSortMode.None
        );
        int count = 0;
        foreach (var obj in all)
        {
            var components = obj.GetComponents<UnityEngine.Component>();
            foreach (var c in components)
            {
                if (c == null)
                {
                    UnityEngine.Debug.LogWarning($"Missing Script auf: {obj.name}");
                    count++;
                }
            }
        }
        UnityEngine.Debug.Log($"Insgesamt {count} fehlende Scripts gefunden!");
    }
    #endif
}
