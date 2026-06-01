using System.IO;
using UnityEngine;
using System.Runtime.InteropServices;
public class SaveSystem
{
    private const string playerDataJsonName = "playerData.json";

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void SyncIDBFS_Internal();
#endif

    private static string GetSavePath(string fileName)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return "/idbfs/BOXKAMPF/" + fileName;
#else
        return Path.Combine(Application.persistentDataPath, fileName);
#endif
    }

    private static void SyncIDBFS()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SyncIDBFS_Internal();
#endif
    }

    public static void SavePlayerdata(PlayerData playerData)
    {
        string json = JsonUtility.ToJson(playerData);
        string path = GetSavePath(playerDataJsonName);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, json);
        SyncIDBFS();
    }
    public static PlayerData LoadPlayerData()
    {
        string path = GetSavePath(playerDataJsonName);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            if (!string.IsNullOrEmpty(json))
            {
                return JsonUtility.FromJson<PlayerData>(json);
            }
        }
        return null;
    }
    public static void ClearUpFile()
    {
        string path = GetSavePath(playerDataJsonName);
        if (File.Exists(path))
        {
            File.WriteAllText(path, string.Empty);
            SyncIDBFS();
        }
    }
}
