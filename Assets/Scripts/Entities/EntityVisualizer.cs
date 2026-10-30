using System;

/// <summary>
/// This is used to visualize anything that exists solely on the tilemap.
/// ex: falling branches
/// This should NOT be used for tree/branch wavy animations.
/// </summary>
public class EntityVisualizer : Monobehaviour
{
    /// <summary>
    /// This corouting should only step 1 unit of displacement at a time
    /// </summary>
    /// <param name="dir">Direction to move. SHould be passed in as a direction, i.e. Vector3.up</param>
    /// <param name="speed">Total time in FUs that the move takes to complete.</param>
    /// <returns></returns>
    IEnumerator SmoothMove(Vector3 dir, float speed)
    {
        Vector3 oldPos = transform.position;
        for (int i = 0; i < speed; i++)
        {
            transform.position = Vector3.Lerp(oldPos, oldPos + dir, lerp);
            lerp += 1f / speed;
            yield return new WaitForFixedUpdate();
        }
    }
}
