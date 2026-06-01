using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{
    public void OnInteract()
    {
        Debug.Log(gameObject.name + " wurde Interacted!");
    }
}
