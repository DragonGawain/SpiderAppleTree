public class Fruit : IInteractable
{
    public readonly Coord coord;

    // The amount of weight the fruit is exerting on the branch its resting on.
    // Can be negative! (i.e. the fruit is pulling the branch up, supporting it)
    public readonly int weight;
    public readonly int weightValue;
    public readonly int webValue;
    public readonly bool needed;

    public Fruit(
        Coord coord,
        int weight = 0,
        int weightValue = 0,
        int webValue = 0,
        bool needed = true
    )
    {
        this.coord = coord;
        this.weightValue = weightValue;
        this.webValue = webValue;
        this.weight = weight;
        this.needed = needed;

        LevelManager.GetActiveLevel().AddInteractable(coord, this);
        // TODO:: Have this constructor spawn in a prefab at the desired location

        LevelManager.GetActiveLevel().GetWalkables().TryGetValue(coord, out IWalkable target);

        // TODO:: if weight is negative, add it to supports instead of weights
        if (target != null && target.GetType() == typeof(Branch))
            ((Branch)target).UpdateWeightDelta(coord, weight);

        if (needed)
            LevelManager.GetActiveLevel().RegisterNeededFruit(this);
    }

    public void Interact()
    {
        LevelManager.GetActiveLevel().GetWalkables().TryGetValue(coord, out IWalkable target);

        if (target != null && target.GetType() == typeof(Branch))
            ((Branch)target).UpdateWeightDelta(coord, -weight);

        LevelManager.GetActiveLevel().RemoveInteractable(coord);
        GameManager
            .GetManagerSingleton()
            .GetComponent<LevelManager>()
            .RemoveInteractableFromMap(coord);

        // It is very important that we adjust the player's weight *after* we remove the fruit's weight from the branch.
        // This is because if done the other way around,
        // we could end up in a situation where both the player's AND the fruit's weight are applied simultaneously.

        // PROBLEM: If the fruit weight is negative (i.e. flying fruit), then we need it to be the other way around...
        // I'll just deal with that when it happens...
        LevelManager.GetPlayerRef().AlterWeb(webValue);
        LevelManager.GetPlayerRef().AlterWeight(weightValue);
        if (needed)
            LevelManager.GetActiveLevel().ConsumeNeededFruit(this);
    }

    public void Refresh()
    {
        LevelManager.GetActiveLevel().GetWalkables().TryGetValue(coord, out IWalkable target);

        if (target != null && target.GetType() == typeof(Branch))
            ((Branch)target).UpdateWeightDelta(coord, weight);
    }
}
