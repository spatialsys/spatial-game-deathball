using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BotConfig", menuName = "BotConfig", order = 1)]
public class BotConfig : ScriptableObject
{
    [Header("Movement")]
    [Range(0f, 5f)]
    public float refreshPositionTime = 2f;

    [Range(0f, 10f)]
    public float minDistToPlayers = 5f;
    
    [Header("Ball")]
    [Range(0, 1f)]
    public float blockChance = 0.9f;

    [Range(0f, 1f)] 
    public float targetClosestPlayerChance = 0.5f;
}
