using UnityEngine;

public class CheckEventTest : MonoBehaviour
{
    private PlayerData _playerData;
    [SerializeField] Vector3 _savedPos;
    private GameObject _player;

    public void CheckSuccess()
    {
        Debug.Log("Event Success");
        _savedPos = _player.transform.position;
        _playerData.SetPosition(_savedPos);
        Debug.Log(_savedPos);

        
    }

    private void Start()
    {
        _playerData = GameManager.Instance._playerData;
        _player = GameObject.FindWithTag("Player");

        _savedPos = _playerData.Position;
        // _player.transform.position = _savedPos;
        Debug.Log(_playerData.Position.ToString());
       
    }
}
