using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType
{
    WALKABLE,
    INTERACTABLE,
    SUPPORT
}

public class TileSelectButton : MonoBehaviour
{
    [SerializeField]
    Tile tile;
    public Tile Tile
    {
        get => tile;
    }

    [SerializeField]
    TileType tileType;

    public TileType TileType
    {
        get => tileType;
    }

    [SerializeField]
    GameObject editorObj;

    public GameObject EditorObj
    {
        get => editorObj;
    }
}
