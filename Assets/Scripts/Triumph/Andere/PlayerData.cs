using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class PlayerData
{

    [HideInInspector] public AllLogicEvents allLogicEvents = new AllLogicEvents();  
    [field:SerializeField] public Vector3 Position {  get; private set; }
    [field: SerializeField] public List<PickItemList> itemList = new List<PickItemList>();

    public void SetPosition(Vector3 position)
    {
        Position = position;
        SaveSystem.SavePlayerdata(this);
    }

    public void SaveItem(PickItemList item)
    {
        itemList.Add(item);
        SaveSystem.SavePlayerdata(this);
    }

    public void RemoveItem(PickItemList item)
    {
        itemList.Remove(item);
        SaveSystem.SavePlayerdata(this);
    }

}
[System.Serializable]
public class AllLogicEvents
{
    public bool hasPlayedStaminaTutorial;
    public bool hasPlayedStrengthTutorial;
    public bool hasPlayedBoxKampfTutorial;
    public bool isInWaitingRoom;
    public bool canShowCreditDialogue;
    public bool hasPlayedThroughGame;
    public AllLogicEvents()
    {

    }
}
