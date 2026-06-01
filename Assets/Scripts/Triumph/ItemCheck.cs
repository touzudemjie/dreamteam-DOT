using UnityEngine;
using UnityEngine.Events;
using UnityEngineInternal;

public class ItemCheck : MonoBehaviour, IInteractable
{
    [SerializeField] PickItemList[] item;
    [SerializeField] bool _destroyAfterUse;
    public UnityEvent checkEvent;
    
    public void OnInteract()
    {
        CheckItem();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.CompareTag("Player"))
        {
            CheckItem();
        }
    }

    void CheckItem()
    {
        bool itemsThere = true;
        foreach(PickItemList i in item)
        {
            if (!Inventory.Instance.HasItem(i))
            {
                itemsThere = false;
                break;
            }
        }

        if (itemsThere)
        {
            Debug.Log(string.Join(", ", item) + " vorhanden");
            Notification.Instance.ShowNotification(string.Join(", ", item) + " vorhanden");
            checkEvent?.Invoke();
            if (_destroyAfterUse)
            {
                foreach (PickItemList i in item)
                {
                   Inventory.Instance.Remove(i);
                }
            }
            return;
        }
        // Item nicht vorhanden
        Notification.Instance.ShowNotification(string.Join(", ", item) + " nicht vorhanden");
        Debug.LogWarning(string.Join(", ", item) + " nicht vorhanden");
    }
}
