using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BotConfig", menuName = "BotConfig", order = 1)]
    public class BotConfig : ScriptableObject
    {
        [Header("Movement")]
        public Vector2 refreshPositionTimeRange = new Vector2(0f, 5f);
        public Vector2 minDistToPlayersRange = new Vector2(0f, 10f);

        [Range(0, 1f)]
        public float blockChance = 0.9f;

        [Range(0f, 1f)] 
        public float targetClosestPlayerChance = 0.5f;

        // Get a random refresh position time within the range
        public float GetRandomRefreshPositionTime()
        {
            return Random.Range(refreshPositionTimeRange.x, refreshPositionTimeRange.y);
        }

        // Get a random minimum distance to players within the range
        public float GetRandomMinDistToPlayers()
        {
            return Random.Range(minDistToPlayersRange.x, minDistToPlayersRange.y);
        }
    }
