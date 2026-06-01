using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UsefulClasses;

public class StaticTest : MonoBehaviour
{
    public static int _staticZahl;
    [SerializeField] private GameScene _gameScene;
    public static StaticTest staticTest;
    public static GameObject staticGameobject;
    private void Awake()
    {
    }
    void Update()
    {
        AddOne();
        LoadScene();
        Debug.Log(_staticZahl);
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            staticTest = this;
            staticGameobject = new GameObject("Test");
        }
        if(staticTest != null)
        {
            Debug.Log(staticTest.ToString());
        }
        if(staticGameobject != null)
        {
            Debug.Log(staticGameobject.ToString());
        }
    }
    void AddOne()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            _staticZahl++;
        }
    }
    void LoadScene()
    {
        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(_gameScene.ToString());
        }
    }
}
