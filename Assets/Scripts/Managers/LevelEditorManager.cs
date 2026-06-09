using System;
using System.Net.Sockets;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class LevelEditorManager : MonoBehaviour
{
    // editing mode. ADD mode includes the ability to erase.
    enum EditMode
    {
        ADD,
        SELECT
    }

    // the target tile map of select mode
    enum SelectTarget
    {
        INTERACTABLE,
        SUPPORT,
    }

    [Header("Tilemaps")]
    [SerializeField]
    Grid grid;

    [SerializeField]
    Tilemap walkables_e,
        interactable_e,
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

    [Header("Display")]
    [SerializeField]
    Image displayImage;

    [SerializeField]
    TextMeshProUGUI displayText;

    [Header("Miscellaneous")]
    [SerializeField]
    Transform editorObjectParent;

    // DEBUG: these fields are public only so I can verify stuff in the inspector.
    // This field should be made private
    public Tile selectedTile;

    [SerializeField]
    EditMode editMode = EditMode.ADD;

    [SerializeField]
    SelectTarget selectTarget = SelectTarget.INTERACTABLE;

    public Tilemap targetTilemap;

    bool selectingTile = false;
    GameObject editorObj;

    [Header("EditorControllers")]
    [SerializeField]
    FruitEditorController fruitEditorController;

    public void TestLevel()
    {
        saveB.SetActive(false);
        loadB.SetActive(false);
        initValuesGroup.SetActive(false);
        testLevelB.SetActive(false);
        editModeGroup.SetActive(false);
        contEditB.SetActive(true);
        CLoseTileSelectPanel();
    }

    public void ContinueEditingLevel()
    {
        saveB.SetActive(true);
        loadB.SetActive(true);
        initValuesGroup.SetActive(true);
        testLevelB.SetActive(true);
        editModeGroup.SetActive(true);
        contEditB.SetActive(false);
        InputManager.EnableEditorInputs();
    }

    public void AddMode()
    {
        DisableAllControllers();
        ToggleTileSelectPanel();
        editMode = EditMode.ADD;
    }

    public void SelectMode()
    {
        DisableAllControllers();
        if (editMode == EditMode.SELECT)
            selectTarget = (SelectTarget)(
                ((int)selectTarget + 1) % Enum.GetValues(typeof(SelectTarget)).Length
            );
        else
            selectTarget = SelectTarget.INTERACTABLE;
        editMode = EditMode.SELECT;
        CLoseTileSelectPanel();
        displayImage.color = new(1, 1, 1, 0);
        displayText.text = selectTarget.ToString().ToLower();
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
        editorObj = tsb.EditorObj;
        displayImage.sprite = tsb.GetComponent<Image>().sprite;
        displayImage.color = tsb.GetComponent<Image>().color;
        displayText.text = tsb.GetComponentInChildren<TextMeshProUGUI>().text;
    }

    public void OnClick()
    {
        // first, determine the location of the click
        Vector3Int mouseCellCoords = grid.WorldToCell(
            Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue())
        );
        // verify that the click is in bounds
        if (
            !(
                mouseCellCoords.x >= SaveManager.X_BOUND_LEFT
                && mouseCellCoords.x <= SaveManager.X_BOUND_RIGHT
                && mouseCellCoords.y >= SaveManager.Y_BOUND_BOTTOM
                && mouseCellCoords.y <= SaveManager.Y_BOUND_TOP
            )
        )
            return;
        switch (editMode)
        {
            case EditMode.ADD:
                PlaceTileOnCell(mouseCellCoords);
                break;
            case EditMode.SELECT:
                SelectInteractable(mouseCellCoords);
                break;
        }
    }

    void PlaceTileOnCell(Vector3Int mouseCellCoords)
    {
        targetTilemap.SetTile(mouseCellCoords, selectedTile);
        if (editorObj != null)
        {
            Instantiate(
                editorObj,
                grid.CellToWorld(mouseCellCoords) + new Vector3(0.5f, 0.5f, 0),
                quaternion.identity,
                editorObjectParent
            );
        }
    }

    void DisableAllControllers()
    {
        fruitEditorController.gameObject.SetActive(false);
    }

    void SelectInteractable(Vector3Int mouseCellCoords)
    {
        DisableAllControllers();
        if (!(interactable_e.HasTile(mouseCellCoords) || support_e.HasTile(mouseCellCoords)))
            return;
        EditorIdentity ei = selectTarget switch
        {
            SelectTarget.SUPPORT => EditorIdentity.SUPPORT,
            SelectTarget.INTERACTABLE or _ => EditorIdentity.INTERACTABLE,
        };

        EditorElement ee = SaveManager.GetEditorElementAtCoord(
            ei,
            new(mouseCellCoords.x, mouseCellCoords.y)
        );
        if (ee == null)
            return;
        if (ee.GetType() == typeof(FruitEditor))
        {
            fruitEditorController.gameObject.SetActive(true);
            fruitEditorController.SelectFruit((FruitEditor)ee);
        }
    }
}
