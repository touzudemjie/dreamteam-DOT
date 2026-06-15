using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance {  get; private set; }
    [SerializeField] VisualInventory _visual;
    [SerializeField] private Key _inventoryKey = Key.Tab;
    [SerializeField] private Key _clearKey = Key.I;
    bool showInventory = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        Instance = this;
    }

    

    private void Update()
    {
        if (Keyboard.current[_inventoryKey].wasPressedThisFrame)
        {
            showInventory = !showInventory;
        }

        if (Keyboard.current[_clearKey].wasPressedThisFrame)
        {
            ClearInventory();
        }
        _visual.gameObject.SetActive(showInventory);
        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            Debug.Log("Hii");
            Add(PickItemList.Apfel);
        }
    }

    

    private void Start()
    {   
        RecoverLastSession();
    }

    public void Add(PickItemList item)
    {
        GameManager.Instance.playerData.SaveItem(item);
        _visual.DisplayItem(item.ToString());
    }

    public void Remove(PickItemList item)
    {
        GameManager.Instance.playerData.RemoveItem(item);
        _visual.HideItem(item.ToString());
    }

    public bool HasItem(PickItemList item)
    {
        if (GameManager.Instance.playerData != null)
            return GameManager.Instance.playerData.itemList.Contains(item);
        Debug.LogWarning("PlayerData nicht verfügbar");
            return false;
    }

    public void ClearInventory()
    {
        foreach (PickItemList item in new List<PickItemList>(GameManager.Instance.playerData.itemList))
        {
            Remove(item);
        }
        Notification.Instance.ShowNotification("Items gelöscht");
    }

    void RecoverLastSession()
    {
        if(GameManager.Instance.playerData != null)
        {
            foreach (PickItemList item in GameManager.Instance.playerData.itemList)
            {
                _visual.DisplayItem(item.ToString());
            }
        }
    }
}
