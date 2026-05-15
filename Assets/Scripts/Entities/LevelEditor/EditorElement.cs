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

    protected void OnStart()
    {
        SaveManager.RegisterEditorElement(GetEditorIdentity(), coord, this);
    }
}
