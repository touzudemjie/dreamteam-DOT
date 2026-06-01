using UnityEditor;
using UnityEngine;
// Im Editor-Ordner: Editor/TimeReverseEditor.cs
#if UNITY_EDITOR

[InitializeOnLoad]
public static class TimeReverseEditor
{
    static TimeReverseEditor()
    {
        ObjectFactory.componentWasAdded += OnComponentAdded;
    }

    private static void OnComponentAdded(Component component)
    {
        if (component is TimeReverse tr)
        {
            Debug.Log(AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(component as MonoBehaviour)));
            //SerializedObject so = new SerializedObject(component);
            //so.FindProperty("<MaxTimeCaptured>k__BackingField").floatValue = 6f;
            //so.ApplyModifiedProperties();
            tr.SetDefaultValues();
        }
    }
}
#endif