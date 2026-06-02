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
            EditorApplication.delayCall += () =>
            {
                if (tr == null)
                {
                    return;
                }

                //Debug.Log(AssetDatabase.GetAssetPath(
                //    MonoScript.FromMonoBehaviour(tr)));
                tr.SetDefaultValues();
            };
        }
    }
}
#endif