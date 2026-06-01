using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Interact : MonoBehaviour
{
    [SerializeField] private float __interactRange;
    [SerializeField] private LayerMask _layers;
    [SerializeField] private Image _crosshair;
    [SerializeField] private Key _interactKey = Key.E;
    [SerializeField] private Color _interactColor;
    [SerializeField] private Color _pickupColor;
    [SerializeField] private bool _canInteract;
    [SerializeField] private bool _canPickup;

    public Inventory inventory;

    private void Update()
    {
        _canInteract = false;
        _canPickup = false;
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, __interactRange, _layers))
        {
            IInteractable[] interactables = hit.transform.GetComponents<IInteractable>();
            if (interactables.Length > 0)
            {
                _canInteract = true;
                if (Keyboard.current[_interactKey].wasPressedThisFrame)
                {
                    foreach (IInteractable interactable in interactables)
                    {
                        interactable.OnInteract();
                    }
                }
            }

            IPickable[] pickables = hit.transform.GetComponents<IPickable>();
            if (pickables.Length > 0)
            {
                _canPickup = true;
                if (Keyboard.current[_interactKey].wasPressedThisFrame)
                {
                    foreach (IPickable pickable in pickables)
                    {
                        pickable.OnPick();
                        Inventory.Instance.Add(pickable.PickItem);
                    }
                    //Destroy(hit.transform.gameObject);
                }
            }
        }

        if (_canInteract)
        _crosshair.color = _interactColor;
        else if (_canPickup)
        _crosshair.color = _pickupColor;
        else
        _crosshair.color = Color.white;
    }  
}