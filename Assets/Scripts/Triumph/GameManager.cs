using UnityEngine;

public class GameManager : MonoBehaviour
{
    public PlayerData playerData;
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        Instance = this;

        playerData = SaveSystem.LoadPlayerData() ?? new PlayerData();
    }
}
