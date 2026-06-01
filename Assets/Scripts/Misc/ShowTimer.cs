using TMPro;
using UnityEngine;
using UsefulClasses;

public class ShowTimer : MonoBehaviour
{
    private TextMeshProUGUI _timerText;
    void Start()
    {
        _timerText = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        _timerText.text = $"Time : {Mathf.Round(Time.time)}";
    }
}
