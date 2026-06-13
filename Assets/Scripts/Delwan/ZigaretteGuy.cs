using UnityEngine;

public class ZigaretteGuy : MonoBehaviour
{
    private Material _material;
    void Start()
    {
        _material = GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void GaveZigarette()
    {
        Debug.Log("DANKEEEE");
    }
    public void ChangeColor()
    {
        _material.color = Color.red;
    }
}
