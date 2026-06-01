using UnityEngine;

public interface IPickable
{
    public PickItemList PickItem { get; }
    public void OnPick();
}
