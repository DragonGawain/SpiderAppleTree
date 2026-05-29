using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class LevelEditorManager : MonoBehaviour
{
    enum EditMode
    {
        ADD,
        SELECT,
        ERASE
    }

    [Header("Tilemaps")]
    [SerializeField]
    Tilemap walkables_e;

    [SerializeField]
    Tilemap interactable_e,
        support_e,
        walkables_l,
        interactable_l,
        numbers_l;

    [Header("Toggleables")]
    [SerializeField]
    GameObject saveB;

    [SerializeField]
    GameObject loadB,
        initValuesGroup,
        testLevelB,
        contEditB,
        editModeGroup,
        tileSelectGroup;

    // DEBUG: these fields are public only so I can verify stuff in the inspector.
    // This field should be made private
    public Tile selectedTile;

    [SerializeField]
    EditMode editMode = EditMode.ADD;

    public Tilemap targetTilemap;

    bool selectingTile = false;

    public void TestLevel() { }

    public void ContinueEditingLevel() { }

    public void AddMode()
    {
        ToggleTileSelectPanel();
        editMode = EditMode.ADD;
    }

    public void SelectMode()
    {
        editMode = EditMode.SELECT;
        CLoseTileSelectPanel();
    }

    public void EraseMode()
    {
        editMode = EditMode.ERASE;
        CLoseTileSelectPanel();
    }

    void OpenTileSelectPanel()
    {
        selectingTile = false;
        ToggleTileSelectPanel();
    }

    void CLoseTileSelectPanel()
    {
        selectingTile = true;
        ToggleTileSelectPanel();
    }

    void ToggleTileSelectPanel()
    {
        selectingTile = !selectingTile;
        tileSelectGroup.SetActive(selectingTile);
    }

    public (int, int, int, int) GetInitialValues()
    {
        TMP_InputField[] inputFields = initValuesGroup.GetComponentsInChildren<TMP_InputField>();

        return (
            int.TryParse(inputFields[0].text, out _) ? int.Parse(inputFields[0].text) : 2, // weight
            int.TryParse(inputFields[1].text, out _) ? int.Parse(inputFields[1].text) : 0, // web
            int.TryParse(inputFields[2].text, out _) ? int.Parse(inputFields[2].text) : 1, // length
            int.TryParse(inputFields[3].text, out _) ? int.Parse(inputFields[3].text) : 6 // base trunk support
        );
    }

    public void SetSelectedTile(TileSelectButton tsb)
    {
        CLoseTileSelectPanel();
        selectedTile = tsb.Tile;
        targetTilemap = tsb.TileType switch
        {
            TileType.INTERACTABLE => interactable_e,
            TileType.SUPPORT => support_e,
            TileType.WALKABLE or _ => walkables_e
        };
    }
}
