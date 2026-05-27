using UnityEngine;

public class FruitEditor : EditorElement
{
    protected override EditorIdentity GetEditorIdentity() => EditorIdentity.INTERACTABLE;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        coord = LevelManager.WorldSpaceToCoord(transform.position);
        OnStart();
    }

    public int weight;
    public int deltaWeight;
    public int deltaWeb;
}
