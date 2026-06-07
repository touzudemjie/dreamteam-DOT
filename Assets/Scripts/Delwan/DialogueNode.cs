using UnityEngine;

[CreateAssetMenu(fileName = "DialogueNode",menuName = "SO/DialogueNode")]
public class DialogueNode : ScriptableObject
{
    public string speaker;
    [TextArea] public string textContent;
    public DialogueNode nextLine;
    public DialogueChoice[] choices;
    public Dialogue nextDialogue;
}
[CreateAssetMenu(fileName = "Dialogue", menuName = "SO/DialogAsset")]
public class DialogAsset : ScriptableObject
{
    public DialogueNode[] nodes;
}