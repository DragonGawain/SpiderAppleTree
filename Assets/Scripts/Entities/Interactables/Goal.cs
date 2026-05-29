using System.Collections;
using UnityEngine;

// The goal point is a quasi-interactable.
// The object itself is purely a visual indicator and the script does not implement IInteractable,
// but it is a quasi-interactable in the sense that it is interacted with.
// Just, the check for interactoin is based on the position stored in the Level object.
[RequireComponent(typeof(SpriteRenderer))]
public class Goal : MonoBehaviour
{
    bool shrink = false;
    float scale = 1f;
    float rotationSpeed = 1.6f;
    float scaleSpeed = 0.01f;
    float maxScale = 1.75f;
    float minScale = 0.9f;

    // purely visual change
    public void Activate()
    {
        // GetComponent<SpriteRenderer>().color = Color.green;
        StartCoroutine(ActivateEnd());
    }

    // TODO:: As a thought, I can make it so that the speed of rotation and growing/shrinking speeds up a bit when the player gets close?
    // Will need a way to determine how many moves away the player is from winning... Maybe just manually place tiles that change the speed?
    IEnumerator ActivateEnd()
    {
        Vector3 axis = new(0, 0, 1);
        while (true)
        {
            transform.Rotate(axis, rotationSpeed);
            if (shrink)
            {
                scale -= scaleSpeed;
                transform.localScale = new(scale, scale, scale);
                if (scale <= minScale)
                    shrink = false;
            }
            else
            {
                scale += scaleSpeed;
                transform.localScale = new(scale, scale, scale);
                if (scale > maxScale)
                    shrink = true;
            }
            yield return new WaitForFixedUpdate();
        }
    }
}
