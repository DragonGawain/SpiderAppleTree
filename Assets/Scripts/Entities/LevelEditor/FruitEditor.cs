using UnityEngine;

public class FruitEditor : EditorElement
{
    protected override EditorIdentity GetEditorIdentity() => EditorIdentity.INTERACTABLE;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        coord = LevelManager.WorldSpaceToCoord(transform.position);
        OnStart();
        SaveManager.RemoveInteractable += OnLevelEditorRemove;
    }

    public int weight;
    public int deltaWeight;
    public int deltaWeb;
    public bool needed = true;

    void OnLevelEditorRemove(Coord crd)
    {
        if (crd.Equals(coord))
            Destroy(this.gameObject);
    }

    void OnDestroy()
    {
        SaveManager.RemoveInteractable -= OnLevelEditorRemove;
    }
}
