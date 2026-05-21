using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
    int webCount,
        length,
        weight;

    Coord currentPos;

    Grid gridRef;

    // This should maybe be an IWalkable?
    // Fundamentally, all weight will find its way back to the source of a branch,
    // but maybe I should consider tensile strength of web strings and give them a snapping point as well?
    Branch occupiedBranch = null;

    // default is 50. Mock FU timer => divide value by 50 to determine the number of seconds the animation will take
    const int MOVE_SPEED = 2;

    public void Initialize(InitialLevelDataContainer ldc, Grid gridRef)
    {
        webCount = ldc.initWebCount;
        weight = ldc.initWeight;
        length = ldc.initLength;
        currentPos = ldc.spawnPoint;
        this.gridRef = gridRef;

        Branch.OnGlobalBranchSnap += OnBranchSnap;
    }

    public void AlterWeight(int delta) => weight += delta;

    public float GetWeight() => weight;

    public void AlterLength(int delta) => length += delta;

    public int GetLength() => length;

    public Coord GetCurentPos() => currentPos;

    public void Move(Direction dir)
    {
        Coord oldCoord = currentPos;

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

        StartCoroutine(SmoothMoveAnimation(oldCoord));
    }

    IEnumerator SmoothMoveAnimation(Coord oldCoord)
    {
        InputManager.DisableAllInputs();

        Vector3 oldPos = transform.position;
        Vector3 targetPos =
            gridRef.CellToWorld(currentPos.ToVector3Int()) + new Vector3(0.5f, 0.5f, 0);
        float lerp = 0.0f;

        for (int i = 0; i < MOVE_SPEED; i++)
        {
            transform.position = Vector3.Lerp(oldPos, targetPos, lerp);
            lerp += 1f / MOVE_SPEED;
            yield return new WaitForFixedUpdate();
        }

        // ensure that player is at the desired position
        transform.position = targetPos;
        LevelManager.GetActiveLevel().PostMove(LevelManager.GetPlayerRef().GetCurentPos());

        IWalkable target = LevelManager.GetActiveLevel().GetWalkables()[currentPos];

        if (occupiedBranch != null)
            occupiedBranch.UpdateWeightDelta(oldCoord, -weight);
        if (target.GetType() == typeof(Branch))
        {
            occupiedBranch = (Branch)target;
            Debug.Log("OCCUPYING BRANCH OF ID: " + occupiedBranch.instanceID);
            occupiedBranch.UpdateWeightDelta(currentPos, weight);
        }
        else
            occupiedBranch = null;

        InputManager.EnableMovementInputs();
    }

    public void OnBranchSnap()
    {
        Debug.Log("ON GLOBAL BRANCH SNAP");
        LevelManager
            .GetActiveLevel()
            .GetWalkables()
            .TryGetValue(currentPos, out IWalkable walkable);
        occupiedBranch = null;

        if (walkable == null)
            Debug.LogWarning(
                "<color=red><b>THE PLAYER HAS DIED! SQUASHED BY A FALLING BRANCH!</b></color>"
            );
        else
        {
            if (walkable.GetType() == typeof(Branch))
            {
                occupiedBranch = (Branch)walkable;
                Debug.Log("OCCUPYING BRANCH OF ID: " + occupiedBranch.instanceID);
                occupiedBranch.UpdateWeightDelta(currentPos, weight);
            }
        }
    }

    void OnDestroy()
    {
        Branch.OnGlobalBranchSnap -= OnBranchSnap;
    }
}
