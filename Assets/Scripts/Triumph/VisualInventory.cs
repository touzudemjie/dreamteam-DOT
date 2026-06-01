using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class VisualInventory : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            HideItem("Kamera");
        }
    }

    public void DisplayItem(string text)
    {
        GameObject textObj = new GameObject(text);

        textObj.transform.SetParent(transform);

        TextMeshProUGUI newText = textObj.AddComponent<TextMeshProUGUI>();

        newText.text = text;
        newText.fontSize = 36;
        newText.color = Color.white;

        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 50);
        rect.anchoredPosition = Vector2.zero;
    }

    public void HideItem(string text)
    {
        foreach(Transform child in transform)
        {
            if(child.gameObject.name == text)
            {
                Destroy(child.gameObject);
            }
        }
    }
}