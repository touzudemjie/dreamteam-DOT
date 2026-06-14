using UnityEngine;

public class DontDestroyTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {

    }
    void Start()
    {
        Debug.Log(TextDisplayManager.Instance.gameObject.name);   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
