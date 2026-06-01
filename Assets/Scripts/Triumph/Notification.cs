using System.Collections;
using TMPro;
using UnityEngine;

public class Notification : MonoBehaviour
{
    public static Notification Instance { get; private set; }
    [SerializeField] TMP_Text _notiText;
    private float _currentNotiDuration;
    private Coroutine _coroutine;
    private bool _notiIsShowing;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        Instance = this;
    }

    private void Start()
    {
        _notiText.gameObject.SetActive(false);
    }

    public void ShowNotification(string notification, float duration = 3)
    {
        _notiText.text = notification;
        _currentNotiDuration = duration;
        Debug.Log(notification);
        if (_notiIsShowing)
        {
            StopCoroutine(_coroutine);
        }
        _coroutine = StartCoroutine(NotificationProcess());
    }

    IEnumerator NotificationProcess()
    {
        _notiIsShowing = true;
        _notiText.gameObject.SetActive(true);
        yield return new WaitForSeconds(_currentNotiDuration);
        _notiText.gameObject.SetActive(false);
        _notiIsShowing = false;
    }
}
