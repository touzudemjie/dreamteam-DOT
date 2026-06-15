using UnityEngine;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    public PlayerData playerData;
    public static GameManager Instance;
    [SerializeField] private Volume _postProcessing;
    public VolumeProfile postProcessingProfile;
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        Instance = this;

        playerData = SaveSystem.LoadPlayerData() ?? new PlayerData();
    }
    private void Start()
    {
        postProcessingProfile = _postProcessing.profile;
    }
}
