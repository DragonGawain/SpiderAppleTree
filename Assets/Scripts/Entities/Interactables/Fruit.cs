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

    // The amount of weight the fruit is exerting on the branch its resting on.
    // Can be negative! (i.e. the fruit is pulling the branch up, supporting it)
    public readonly int weight;

    public Fruit(Coord coord, FruitType fruitType, int weight = 0)
    {
        this.coord = coord;
        this.fruitType = fruitType;
        this.weight = weight;

        LevelManager.GetActiveLevel().AddInteractable(coord, this);
        // TODO:: Have this constructor spawn in a prefab at the desired location

        LevelManager.GetActiveLevel().GetWalkables().TryGetValue(coord, out IWalkable target);

        if (target != null && target.GetType() == typeof(Branch))
            ((Branch)target).UpdateWeightDelta(coord, weight);
    }

    public Fruit(Coord coord, int fruitType, int weight = 0)
        : this(coord, (FruitType)fruitType, weight) { }

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

        LevelManager.GetActiveLevel().GetWalkables().TryGetValue(coord, out IWalkable target);

        if (target != null && target.GetType() == typeof(Branch))
            ((Branch)target).UpdateWeightDelta(coord, -weight);

        LevelManager.GetActiveLevel().RemoveInteractable(coord);
        GameManager
            .GetManagerSingleton()
            .GetComponent<LevelManager>()
            .RemoveInteractableFromMap(coord);
    }
}
