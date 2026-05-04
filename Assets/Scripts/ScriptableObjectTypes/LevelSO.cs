using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Level")]
public class LevelSO : ScriptableObject
{
    [Header("Trees")]
    public List<Trunk> trees = new();
    public int asda;
}
