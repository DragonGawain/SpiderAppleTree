using UnityEngine;

// The goal point is a quasi-interactable.
// The object itself is purely a visual indicator and the script does not implement IInteractable,
// but it is a quasi-interactable in the sense that it is interacted with.
// Just, the check for interactoin is based on the position stored in the Level object.
[RequireComponent(typeof(SpriteRenderer))]
public class Goal : MonoBehaviour
{
    // purely visual change
    public void Activate()
    {
        GetComponent<SpriteRenderer>().color = Color.green;
    }
}
