using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UsefulClasses;

public class Explosion : MonoBehaviour
{
    public enum ExplosionType
    {
        InputTrigger,
        Collsion
    }
    [SerializeField] private float _explosionForce;
    [SerializeField] private float _radius;
    [SerializeField] private Key _triggerKey;
    [SerializeField] private float _upwardsModifier;
    [SerializeField] private ExplosionType _explosionType;
    [SerializeField] private bool _neglectPlayer;
    void Update()
    {
        if (Keyboard.current[_triggerKey].wasPressedThisFrame)
        {
            Explode(transform.position, _explosionForce, _radius);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(_explosionType == ExplosionType.Collsion)
        {
            Explode(transform.position,_explosionForce,_radius);
        }
        
    }
    public void Explode(Vector3 explosionPos, float force, float radius)
    {
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                if (_neglectPlayer && rb.gameObject.CompareTag(Tag.Player.ToString()))
                {
                    return;
                }
                rb.isKinematic = false;
                rb.AddExplosionForce(force, explosionPos, radius, _upwardsModifier);
                

            }
        }
    }
}
