using UnityEngine;
using UsefulClasses;
public class ExplosionTrigger : MonoBehaviour
{
    [SerializeField] Rigidbody _explosionRb;
    private Transform _explosionTr;
    private Vector3 _startPos;
    [SerializeField] private GameObject _explosionTimeBlockParent;
    private TimeReverse[] _timeReversers;
    void Start()
    {
        _explosionTr = _explosionRb.transform;
        _startPos = _explosionTr.position;
        _timeReversers = _explosionTimeBlockParent.GetComponentsInChildren<TimeReverse>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag(Tag.Player.ToString()))
        {
            _explosionRb.isKinematic = false;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.transform.CompareTag(Tag.Player.ToString()))
        {
            _explosionRb.isKinematic = true;
            _explosionTr.position = _startPos;
            _explosionTr.rotation = Quaternion.identity;
            Debug.Log("HAAAAAA");
            foreach (TimeReverse timeReverse in _timeReversers) 
            {
                timeReverse.ActivateReverse();
                if (timeReverse.gameObject.TryGetComponent(out Rigidbody rigidbody))
                {
                    rigidbody.isKinematic = true;
                } 
            }
        }
    }
}
