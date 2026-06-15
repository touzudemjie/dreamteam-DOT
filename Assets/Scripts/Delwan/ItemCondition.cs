// Beispiel-Implementation
using UnityEngine;

[CreateAssetMenu(fileName = "HasItemCondition", menuName = "SO/Conditions/Item")]
public class ItemCondition : DialogueCondition
{
    [SerializeField] private PickItemList _requiredItem;

    //public override bool Evaluate()
    //{
    //    return PlayerInventory.HasItem(_requiredItem);
    //}
    public override bool Evaluate()
    {
        return Inventory.Instance.HasItem(_requiredItem);
    }
}