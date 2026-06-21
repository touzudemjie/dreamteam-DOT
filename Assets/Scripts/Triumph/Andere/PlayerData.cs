using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class PlayerData
{

    [HideInInspector] public AllLogicEvents allLogicEvents = new AllLogicEvents();  
    [field:SerializeField] public Vector3 Position {  get; private set; }
    [field: SerializeField] public List<PickItemList> ItemList { get; private set; } = new List<PickItemList>();
    [field:SerializeField] public List<NPCDialogueSaveData> NPCSaveDatas { get; private set; } = new List<NPCDialogueSaveData>();
    public void SetPosition(Vector3 position)
    {
        Position = position;
        SaveSystem.SavePlayerdata(this);
    }

    public void SaveItem(PickItemList item)
    {
        ItemList.Add(item);
        SaveSystem.SavePlayerdata(this);
    }

    public void RemoveItem(PickItemList item)
    {
        ItemList.Remove(item);
        SaveSystem.SavePlayerdata(this);
    }
    public void SaveNPCData(NPCDialogueSaveData newData)
    {
        if (NPCSaveDatas.Count > 0)
        {
            bool isSameId = false;
            for (int i = 0; i < NPCSaveDatas.Count; i++)
            {
                if (NPCSaveDatas[i].NPCId == newData.NPCId)
                {
                    isSameId = true;
                    NPCSaveDatas[i] = newData;
                }
            }
            if (!isSameId)
            {
                NPCSaveDatas.Add(newData);
            }
        }
        else
        {
            NPCSaveDatas.Add(newData);
        }
    }

    public NPCDialogueSaveData FindNPCData(string NPCId)
    {
        foreach (NPCDialogueSaveData entry in NPCSaveDatas)
        {
            if (entry.NPCId == NPCId) return entry;
        }
        return null;
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

}
[Serializable]
public class NPCDialogueSaveData
{
    public string NPCId;
    public int currentDialogueLineIndex;
    public string currentDialogueId;
    public int dialogueAssetIndex;
    public int NPCSeverityScore;
    public bool hasCrossedSeverityLine;
    public List<string> encounteredSevereIds;
}

