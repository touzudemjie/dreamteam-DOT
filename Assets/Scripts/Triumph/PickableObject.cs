using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class PickableObject : MonoBehaviour, IPickable
{
    [SerializeField] private PickItemList pickItem;
    public PickItemList PickItem => pickItem;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => GameManager.Instance._playerData != null);
        Debug.Log("Start: " + GameManager.Instance._playerData);
        if (Inventory.Instance.HasItem(pickItem))
        {
            gameObject.SetActive(false);
        }
    }

    public void OnPick()
    {
        Debug.Log(pickItem);
        Notification.Instance.ShowNotification($"{pickItem} wurde aufgehoben.");
        Destroy(gameObject);
    }
}
