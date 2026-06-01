using UnityEngine;
using UnityEngine.UI;

public class Scroller : MonoBehaviour
{
    private RawImage _rawImage;
    [SerializeField] private float _xPos, _yPos;
    void Start()
    {
        _rawImage = GetComponent<RawImage>();
    }

    // Update is called once per frame
    void Update()
    {
        _rawImage.uvRect = new Rect(_rawImage.uvRect.position + new Vector2(_xPos, _yPos) * Time.deltaTime,_rawImage.uvRect.size);
    }
}
