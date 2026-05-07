using UnityEngine;

public enum FruitType
{
    EMPTY, // no effect
    WEB, // + web count
    ANTI_WEB, // - web count
    LIGHT, // weight goes down
    HEAVY // weight goes up
}

public class Fruit : IInteractable
{
    public readonly FruitType fruitType;

    public readonly Coord coord;

    public Fruit(Coord coord, FruitType fruitType)
    {
        this.coord = coord;
        this.fruitType = fruitType;

        if (SaveManager.GetCreatingLevel())
            SaveManager.GetNewLevel().AddInteractable(coord, this);
        else
            LevelManager.GetActiveLevel().AddInteractable(coord, this);
        // TODO:: Have this constructor spawn in a prefab at the desired location
    }

    public Fruit(Coord coord, int fruitType)
        : this(coord, (FruitType)fruitType) { }

    public void Interact()
    {
        switch (fruitType)
        {
            case FruitType.EMPTY:
                break;
            case FruitType.WEB:
                break;
            case FruitType.ANTI_WEB:
                break;
            case FruitType.LIGHT:
                break;
            case FruitType.HEAVY:
                break;
        }

        LevelManager.GetActiveLevel().RemoveInteractable(coord);
        GameManager
            .GetManagerSingleton()
            .GetComponent<LevelManager>()
            .RemoveInteractableFromMap(coord);
    }
}
