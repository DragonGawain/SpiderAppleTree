using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is used to visualize anything that exists solely on the tilemap.
/// ex: falling branches
/// This should NOT be used for tree/branch wavy animations.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class EntityVisualizer : MonoBehaviour
{
    [Serializable]
    public struct VizData
    {
        public Visualizer viz;
        public Sprite sprite;
    }

    [SerializeField]
    List<VizData> visualizers;

    static readonly Dictionary<Visualizer, Sprite> spriteMap = new();

    static bool isVisualizing = false;

    private void Awake()
    {
        if (spriteMap.Count > 0)
            return;

        foreach (VizData vd in visualizers)
            spriteMap.Add(vd.viz, vd.sprite);
    }

    public void Setup(Visualizer viz)
    {
        GetComponent<SpriteRenderer>().sprite = spriteMap[viz];
    }

    /// <summary>
    /// This corouting should only step 1 unit of displacement at a time
    /// </summary>
    /// <param name="delta">Direction and magnitudeto move.</param>
    /// <param name="speed">Total time in FUs that the move takes to complete.</param>
    /// <returns></returns>
    public IEnumerator SmoothMove(Vector3 delta, float speed, bool postDestroy = false)
    {
        isVisualizing = true;
        Vector3 oldPos = transform.position;
        float lerp = 0.0f;
        for (int i = 0; i <= speed; i++)
        {
            transform.position = Vector3.Lerp(oldPos, oldPos + delta, lerp);
            lerp += 1f / speed;
            yield return new WaitForFixedUpdate();
        }

        isVisualizing = false;

        if (postDestroy)
            Destroy(gameObject);
    }

    public static bool GetIsVisualizing() => isVisualizing;
}

public enum Visualizer
{
    BRANCH,
    WEB_BRANCH,
    HOLLOW_BRANCH
}
