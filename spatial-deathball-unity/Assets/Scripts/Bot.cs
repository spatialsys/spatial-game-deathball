using UnityEngine;
using SpatialSys.UnitySDK;
using UnityEngine.AI;

/// <summary>
/// Controls a bot. Should only be actively used by host client.
/// </summary>
[RequireComponent(typeof(SpatialSyncedObject)), RequireComponent(typeof(NavMeshAgent))]
public class Bot : MonoBehaviour
{
    const float maxMoveDist = 25f;

    public BotConfig config;

    public SpatialSyncedObject syncedObject { get; private set; }
    private NavMeshAgent navMeshAgent;
    private float newPosTimer;

    private void OnEnable()
    {
        syncedObject = GetComponent<SpatialSyncedObject>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        BotManager.RegisterBot(this);
    }

    private void OnDisable()
    {
        BotManager.DeregisterBot(this);
    }

    private void Update()
    {
        UnownedUpdate();
        if (syncedObject.isLocallyOwned)
        {
            OwnedUpdate();
        }
    }

    // called on all clients every frame
    private void UnownedUpdate()
    {
        // do visuals and stuff here probably...
    }

    // called on owner client every frame
    private void OwnedUpdate()
    {
        newPosTimer += Time.deltaTime;
        if (newPosTimer >= config.refreshPositionTime)
        {
            newPosTimer = 0;
            Vector3 targetPosition;

            Bot closestBot = null;
            var minDistance = FindClosestBot(out closestBot);

            // Check if the closest bot is within the minimum distance
            float minimumDistance = config.minDistToPlayers;
            if (closestBot != null && minDistance < minimumDistance)
            {
                // Move away from the closest bot
                Vector3 awayDirection = transform.position - closestBot.transform.position;
                targetPosition = transform.position + (awayDirection.normalized * maxMoveDist);
            }
            else
            {
                // Random movement logic
                Vector3 randomDirection = Random.insideUnitSphere * maxMoveDist;
                randomDirection += transform.position;
                targetPosition = randomDirection;
            }

            // Check if the target position is valid on the NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, maxMoveDist, 1))
            {
                navMeshAgent.SetDestination(hit.position);
            }
        }
    }


    public bool CheckIfBlocking()
    {
        return Random.value < config.blockChance;
    }

    public bool CheckIfTargetClosest()
    {
        return Random.value < config.targetClosestPlayerChance;
    }

    public void HitBot()
    {
        SpatialBridge.vfxService.CreateFloatingText("Beep Ouch!", FloatingTextAnimStyle.Bouncy, transform.position, Vector3.up, Color.red);
    }

    private bool IsBallTargetingMe()
    {
        return BallControl.instance.ballVariables.targetType == 1 && BallControl.instance.ballVariables.botTargetID == syncedObject.InstanceID;
    }

    private float FindClosestBot(out Bot closestBot)
    {
        // Find all bots
        var allBots = BotManager.GetAllBots();
        float minDistance = float.MaxValue;
        closestBot = null;

        // Find the closest bot
        foreach (var bot in allBots)
        {
            // Ensure we don't compare the bot to itself
            if (bot.gameObject != gameObject)
            {
                float distance = Vector3.Distance(transform.position, bot.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestBot = bot;
                }
            }
        }

        return minDistance;
    }
}
