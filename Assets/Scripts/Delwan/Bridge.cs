using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UsefulClasses;

public class Bridge : MonoBehaviour
{
    [SerializeField] private GameObject _brokenBrigde;
    [SerializeField] private float _explosionForce;
    [SerializeField] private float _explosionRadius;    

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            GameObject brokenBridge = Instantiate(_brokenBrigde, transform.position, transform.rotation);
            foreach (Transform child in brokenBridge.transform) 
            {
                if (child.TryGetComponent<Rigidbody>(out Rigidbody rb))
                {
                    rb.isKinematic = true;
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(Tag.Player.ToString()))
        {
            GameObject brokenBridge = Instantiate(_brokenBrigde, transform.position, transform.rotation);
            foreach(Transform child in brokenBridge.transform)
            {
                if (child.TryGetComponent<Rigidbody>(out Rigidbody rb))
                {
                    rb.AddExplosionForce(_explosionForce, transform.position, _explosionRadius);
                }
            }
            Destroy(gameObject);
        }
    }
}
