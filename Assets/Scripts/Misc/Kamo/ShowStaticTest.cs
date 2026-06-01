using TMPro;
using UnityEngine;

public class ShowStaticTest : MonoBehaviour
{
    private TextMeshProUGUI _text;
    void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();    
    }

    // Update is called once per frame
    void Update()
    {
        _text.text = StaticTest._staticZahl.ToString();
    }
}
