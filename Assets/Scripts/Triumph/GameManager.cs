using UnityEngine;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    public PlayerData playerData;
    public static GameManager Instance;
    [SerializeField] private Volume _postProcessing;
    [HideInInspector]  public VolumeProfile postProcessingProfile;
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        Instance = this;

        playerData = SaveSystem.LoadPlayerData() ?? new PlayerData();
    }
    private void Start()
    {
        if (_postProcessing != null)
        {
          postProcessingProfile = _postProcessing.profile;
        }
    }
}
