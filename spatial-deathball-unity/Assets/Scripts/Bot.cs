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
        Vector3? targetPosition = null;
        
        // On any frame, move away if the closest player is too close to another entity
        var closestDistance = FindClosestPlayerEntity(out var closestBot);

        // Check if the closest entity is within the minimum distance
        float minimumDistance = config.minDistToPlayer;
        if (closestBot != null && closestDistance < minimumDistance)
        {
            // Move away from the closest entity
            var position = transform.position;
            Vector3 awayDirection = position - closestBot.transform.position;
            targetPosition = position + (awayDirection.normalized * maxMoveDist);
        } 
        else if (newPosTimer >= config.GetRandomRefreshPositionTime())
        {
            newPosTimer = 0;
            
            // Random movement logic
            Vector3 randomDirection = Random.insideUnitSphere * maxMoveDist;
            randomDirection += transform.position;
            targetPosition = randomDirection;
        }
        
        // Update the target - but first check if the target position is valid on the NavMesh
        if (targetPosition != null && NavMesh.SamplePosition(targetPosition.Value, out var hit, maxMoveDist, 1))
        {
            navMeshAgent.SetDestination(hit.position);
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

    private float FindClosestPlayerEntity(out GameObject closestEntity)
    {
        // First, search through all bots
        var allBots = BotManager.GetAllBots();
        float minDistance = float.MaxValue;
        closestEntity = null;

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
                    closestEntity = bot.gameObject;
                }
            }
        }
        
        // Then search through all players
        var allPlayers = SpatialBridge.actorService.actors.Values;
        foreach (var player in allPlayers)
        {
            Transform playerTransform = player.avatar.GetAvatarBoneTransform(HumanBodyBones.Chest);
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestEntity = playerTransform.gameObject;
            }
        }

        return minDistance;
    }
}
