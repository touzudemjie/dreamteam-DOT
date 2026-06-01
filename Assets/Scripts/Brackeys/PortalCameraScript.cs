using UnityEngine;
public class PortalCameraScript : MonoBehaviour
{
    [SerializeField] private Transform _playerCameraTr;
    [SerializeField] private Transform _otherPortalTr;
    [SerializeField] private Transform _currentPortalTr;
    void Start()
    {
        
    }
    //void LateUpdate()
    //{
    //    //Vector3 playerPortalOffset = _playerCameraTr.position - _otherPortalTr.position;
    //    //transform.position = _currentPortalTr.position + playerPortalOffset;
    //    //float angularPortalDifference = Quaternion.Angle(_currentPortalTr.rotation, _otherPortalTr.rotation);
    //    //Quaternion portalRotationalDifference = Quaternion.AngleAxis(angularPortalDifference, Vector3.up);
    //    //Vector3 newCameraDirection = portalRotationalDifference * _playerCameraTr.forward;
    //    //transform.rotation = Quaternion.LookRotation(newCameraDirection, Vector3.up);
    //}
    void LateUpdate()
    {
         // 1. Spielerposition in den lokalen Raum des OtherPortals umrechnen
        Vector3 localOffset = _otherPortalTr.InverseTransformPoint(_playerCameraTr.position);

        // 2. Diesen lokalen Offset vom CurrentPortal aus in Weltkoordinaten
        transform.position = _currentPortalTr.TransformPoint(localOffset);

        // 3. Rotation genauso: lokal beim OtherPortal, dann auf CurrentPortal
        Quaternion localRotation = Quaternion.Inverse(_otherPortalTr.rotation) * _playerCameraTr.rotation;
        transform.rotation = _currentPortalTr.rotation * localRotation;
    }


}
