using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
    int webCount,
        weight,
        length;

    Coord currentPos;

    Grid gridRef;

    // This should maybe be an IWalkable?
    // Fundamentally, all weight will find its way back to the source of a branch,
    // but maybe I should consider tensile strength of web strings and give them a snapping point as well?
    Branch occupiedBranch = null;

    public void Initialize(InitialLevelDataContainer ldc, Grid gridRef)
    {
        webCount = ldc.initWebCount;
        weight = ldc.initWeight;
        length = ldc.initLength;
        currentPos = ldc.spawnPoint;
        this.gridRef = gridRef;
    }

    public void AlterWeight(int delta) => weight += delta;

    public int GetWeight() => weight;

    public void AlterLength(int delta) => length += delta;

    public int GetLength() => length;

    public Coord GetCurentPos() => currentPos;

    public void Move(Direction dir)
    {
        if (occupiedBranch != null)
            occupiedBranch.UpdateWeightDelta(currentPos, -weight);

        currentPos = dir switch
        {
            Direction.UP => new(currentPos.x, currentPos.y + 1),
            Direction.RIGHT => new(currentPos.x + 1, currentPos.y),
            Direction.DOWN => new(currentPos.x, currentPos.y - 1),
            Direction.LEFT => new(currentPos.x - 1, currentPos.y),
            _
                => throw new Exception(
                    "ERROR: Attempted to move the player in an undefined Direction"
                )
        };

        StartCoroutine(SmoothMoveAnimation(dir));
    }

    IEnumerator SmoothMoveAnimation(Direction dir)
    {
        InputManager.DisableAllInputs();

        Vector3 oldPos = transform.position;
        Vector3 targetPos =
            gridRef.CellToWorld(currentPos.ToVector3Int()) + new Vector3(0.5f, 0.5f, 0);
        float lerp = 0.0f;

        for (int i = 0; i < 50; i++)
        {
            transform.position = Vector3.Lerp(oldPos, targetPos, lerp);
            lerp += 0.02f;
            yield return new WaitForFixedUpdate();
        }

        // ensure that player is at the desired position
        transform.position = targetPos;
        LevelManager.GetActiveLevel().PostMove(LevelManager.GetPlayerRef().GetCurentPos());

        IWalkable target = LevelManager.GetActiveLevel().GetWalkables()[currentPos];
        if (target.GetType() == typeof(Branch))
        {
            occupiedBranch = (Branch)target;
            occupiedBranch.UpdateWeightDelta(currentPos, weight);
        }
        else
            occupiedBranch = null;

        InputManager.EnableMovementInputs();
    }
}
