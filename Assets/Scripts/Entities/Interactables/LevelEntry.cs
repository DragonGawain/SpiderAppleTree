using UnityEngine;

public class LevelEntry : IInteractable
{
    public readonly int levelID;
    public readonly Coord coord;

    public LevelEntry(Coord coord, int levelID)
    {
        this.coord = coord;
        this.levelID = levelID;
        GameManager.DebugLog("Created level entry object!");

        LevelManager.GetActiveLevel().AddInteractable(coord, this);
    }

    public void Interact()
    {
        // TODO:: Show level title card
        // Title card should be in the following format:
        // Level nb: level name
        // ex:
        // 1: Climbing trunks
        // 2: Crossing branches
        // 3: Yummy apples!
        // 4: They can fall?!

        // TODO:: Set this entity as the 'hovered level' (?) in the input manager
        // I'm sotring that data in the input manager because nothing else will ever need it...
        // There should never be a case where two level entry points are adjacent!
        // (i.e. you should not be able to move from one level to another in a single move)
        // This rule will make detecting when there is no selected level easier.
        InputManager.SetHoveredLevel(this);
    }

    public void EnterLevel()
    {
        GameManager.SelectLevel(levelID);
    }

    public void Refresh() { }
}
