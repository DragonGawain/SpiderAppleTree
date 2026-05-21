using UnityEngine;

public enum EditorIdentity
{
    SUPPORT,
    INTERACTABLE
}

public abstract class EditorElement : MonoBehaviour
{
    protected Coord coord;
    protected abstract EditorIdentity GetEditorIdentity();

    protected Coord GetCoord() => coord;

    public int supportValue;

    protected void OnStart()
    {
        coord = LevelManager.WorldSpaceToCoord(transform.position);
        SaveManager.RegisterEditorElement(GetEditorIdentity(), coord, this);
    }
}
