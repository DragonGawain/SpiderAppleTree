public interface IInteractable : ILevelElement
{
    void Interact();

    /// <summary>
    /// Calls for this interactable to re-apply its weight to a potential walkable. Called when the branch it was sitting on snaps.
    /// </summary>
    void Refresh();
}
