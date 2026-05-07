using System;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    int webCount,
        weight,
        length;

    Coord currentPos;

    Grid gridRef;

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

    public Coord GetCuurentPos() => currentPos;

    public void Move(Direction dir)
    {
        StartCoroutine(SmoothMoveAnimation(dir));
    }

    IEnumerator SmoothMoveAnimation(Direction dir)
    {
        InputManager.DisableMovementInputs();

        Vector3Int targetCoord = dir switch
        {
            Direction.UP => new(currentPos.x, currentPos.y + 1, 0),
            Direction.RIGHT => new(currentPos.x + 1, currentPos.y, 0),
            Direction.DOWN => new(currentPos.x, currentPos.y - 1, 0),
            Direction.LEFT => new(currentPos.x - 1, currentPos.y, 0),
            _
                => throw new Exception(
                    "ERROR: Attempted to move the player in an undefined Direction"
                )
        };

        Vector3 oldPos = transform.position;
        Vector3 targetPos = gridRef.CellToWorld(targetCoord);
        float lerp = 0.0f;

        for (int i = 0; i < 50; i++)
        {
            transform.position = Vector3.Lerp(oldPos, targetPos, lerp);
            lerp += 0.02f;
            yield return new WaitForFixedUpdate();
        }

        // ensure that player is at the desired position
        transform.position = targetPos;

        InputManager.EnableMovementInputs();
    }
}
