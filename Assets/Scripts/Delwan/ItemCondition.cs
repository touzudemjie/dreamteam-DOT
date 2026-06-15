// Beispiel-Implementation
using UnityEngine;

[CreateAssetMenu(fileName = "HasItemCondition", menuName = "Dialogue/Conditions/HasItem")]
public class ItemCondition : DialogueCondition
{
    //[SerializeField] private ItemSO _requiredItem;

    //public override bool Evaluate()
    //{
    //    return PlayerInventory.HasItem(_requiredItem);
    //}
    public override bool Evaluate()
    {
        return false;
    }
}