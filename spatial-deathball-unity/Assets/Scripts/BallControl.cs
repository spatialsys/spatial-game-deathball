using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SpatialSys.UnitySDK;
using UnityEngine;

/// <summary>
/// Keeping it dead simple and not worrying about quality of netcode at all. No owner swapping.
/// 
/// Server host client controls ball. Host is in charge of checking for hits/blocks.
/// </summary>
[RequireComponent(typeof(BallVariableSync)), RequireComponent(typeof(Rigidbody))]
public class BallControl : MonoBehaviour
{
    public static BallControl instance;

    private const int PLAYER_TRIGGER_LAYER = 7;
    private const int BOT_PLAYER_LAYER = 6;

    public float powerPerSecond = .1f;// power resets on HIT
    public float maxPower = 10f;
    public float forceByPower = 10f;// how much force we add to the ball per power
    public float rotationByPower = 1f;// how much we rotate the ball's velocity per power

    public BallVariableSync ballVariables { get; private set; }

    private Rigidbody rb;
    private SpatialSyncedObject syncedObject;

    private Transform previousTarget;
    private Transform target;

    private void Start()
    {
        instance = this;
        ballVariables = GetComponent<BallVariableSync>();
        rb = GetComponent<Rigidbody>();
        syncedObject = GetComponent<SpatialSyncedObject>();
    }

    private void FixedUpdate()
    {
        if (syncedObject.isLocallyOwned)
        {
            OwnedUpdate();
        }
    }

    private bool TryFindNewTarget(bool resetVelocity, bool targetClosest = false)
    {
        bool foundTarget = targetClosest ? FindClosestTarget() : FindRandomTarget();

        if (!foundTarget)
        {
            Debug.LogError("Failed to find a new target.");
            return false;
        }

        if (resetVelocity)
        {
            rb.velocity = Vector3.up * 6f; // Resetting the velocity upwards
        }
        else
        {
            // Adjusting the velocity towards the new target
            rb.velocity = (target.position - transform.position).normalized * rb.velocity.magnitude;
        }

        Debug.LogError($"New target: {target.name}, {ballVariables.targetID}, {ballVariables.targetType}");
        return true;
    }

    private bool FindClosestTarget()
    {
        float minDistance = float.MaxValue;
        Transform closestTarget = null;
        int closestTargetID = -1;
        int closestBotIndex = -1;
        int targetType = -1; // 0 for player, 1 for bot

        int previousPlayerTargetID = ballVariables.targetType == 0 ? ballVariables.targetID : -1;
        string previousBotTargetID = ballVariables.targetType == 1 ? ballVariables.botTargetID : null;

        // Check for closest player
        foreach (var player in SpatialBridge.actorService.actors)
        {
            try
            {
                if (player.Value == null || player.Value.avatar == null)
                {
                    Debug.LogError($"Player value or avatar is null for player ID: {player.Key}");
                    continue;
                }

                // Skip if this was the last target and it's a player
                if (ballVariables.targetType == 0 && player.Key == previousPlayerTargetID)
                    continue;

                Transform playerTransform = player.Value.avatar.GetAvatarBoneTransform(HumanBodyBones.Chest);
                float distance = Vector3.Distance(transform.position, playerTransform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestTarget = playerTransform;
                    closestTargetID = player.Key;
                    targetType = 0;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to find chest bone on player {player.Key}: {e}");
            }
        }

        // Check for closest bot
        for (int i = 0; i < BotManager.bots.Count; i++)
        {
            var bot = BotManager.bots[i];

            // Skip if this was the last target and it's a bot
            if (ballVariables.targetType == 1 && bot.syncedObject.InstanceID == previousBotTargetID)
                continue;

            float distance = Vector3.Distance(transform.position, bot.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestTarget = bot.transform;
                closestBotIndex = i;
                targetType = 1;
            }
        }

        // Set the target if one is found
        if (closestTarget != null)
        {
            target = closestTarget;
            if (targetType == 1)
            {
                // When the target is a bot, set both targetID and botTargetID
                ballVariables.targetID = closestBotIndex;
                ballVariables.botTargetID = BotManager.bots[closestBotIndex].syncedObject.InstanceID;
            }
            else
            {
                // For players, just set the targetID
                ballVariables.targetID = closestTargetID;
            }
            ballVariables.targetType = targetType;
            return true;
        }

        return false;
    }

    private bool FindRandomTarget()
    {
        int totalEntities = SpatialBridge.actorService.actors.Count + BotManager.bots.Count;
        int randomIndex;
        int attempts = 0;

        int previousPlayerTargetID = ballVariables.targetType == 0 ? ballVariables.targetID : -1;
        string previousBotTargetID = ballVariables.targetType == 1 ? ballVariables.botTargetID : null;

        do
        {
            randomIndex = Random.Range(0, totalEntities);
            if (randomIndex < SpatialBridge.actorService.actors.Count)
            {
                // If targeting a player, check if it's the previously targeted player
                int potentialTargetID = SpatialBridge.actorService.actors.ElementAt(randomIndex).Key;
                if (ballVariables.targetType == 0 && potentialTargetID == previousPlayerTargetID && totalEntities > 1)
                {
                    continue; // Skip and reselect if it's the same as the previous target
                }

                // If it's a new target, set the target
                try
                {
                    target = SpatialBridge.actorService.actors[potentialTargetID].avatar.GetAvatarBoneTransform(HumanBodyBones.Chest);
                    ballVariables.targetID = potentialTargetID;
                    ballVariables.targetType = 0;
                    return true;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to find chest bone on player {randomIndex}: {e}");
                    return false; // Exit if there's an exception
                }
            }
            else
            {
                // If targeting a bot, check if it's the previously targeted bot
                int botIndex = randomIndex - SpatialBridge.actorService.actors.Count;
                string potentialBotTargetID = BotManager.bots[botIndex].syncedObject.InstanceID;
                if (ballVariables.targetType == 1 && potentialBotTargetID == previousBotTargetID && totalEntities > 1)
                {
                    continue; // Skip and reselect if it's the same as the previous target
                }

                // If it's a new target, set the target
                target = BotManager.bots[botIndex].transform;
                ballVariables.targetID = botIndex; // Local identifier for the bot
                ballVariables.botTargetID = potentialBotTargetID; // Synced identifier for networked environments
                ballVariables.targetType = 1;
                return true;
            }

            attempts++;
        } while (attempts < totalEntities);

        return false; // In case no new target is found after checking all entities
    }

    private void OwnedUpdate()
    {
        if (target == null && !TryFindNewTarget(true))
        {
            return;
        }

        ballVariables.power = Mathf.Min(ballVariables.power + powerPerSecond * Time.fixedDeltaTime, maxPower);

        rb.AddForce((target.position - transform.position).normalized * forceByPower * ballVariables.power * Time.fixedDeltaTime);
        //rotate rb velocity towards target
        //this might break the rigidbody syncing we will see...
        Vector3 velocity = rb.velocity;
        Vector3 targetDir = (target.position - transform.position).normalized;
        Vector3 newDir = Vector3.RotateTowards(velocity.normalized, targetDir, rotationByPower * ballVariables.power * Time.fixedDeltaTime, 10f);
        rb.velocity = velocity.magnitude * newDir;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (!syncedObject.isLocallyOwned || rb == null)
        {
            return;
        }

        if (
            collider.gameObject.layer == PLAYER_TRIGGER_LAYER &&
            ballVariables.targetType == 0 &&
            collider.gameObject.TryGetComponent(out PlayerTrigger playerTrigger) &&
            playerTrigger.actorID == ballVariables.targetID
        )
        {
            //Ball just hit target ACTOR
            if (SpatialBridge.actorService.actors[ballVariables.targetID].customProperties.TryGetValue("isBlocking", out object isBlocking) && (bool)isBlocking)
            {
                //blocked
                SpatialBridge.vfxService.CreateFloatingText("BLOCKED!", FloatingTextAnimStyle.Bouncy, transform.position, Vector3.up, Color.green);
                TryFindNewTarget(false);
            }
            else
            {
                //hit
                ballVariables.power = 1f;
                SpatialBridge.vfxService.CreateFloatingText("HIT", FloatingTextAnimStyle.Bouncy, transform.position, Vector3.up, Color.red);
                TryFindNewTarget(true);
            }
        }
        else if (
            collider.gameObject.layer == BOT_PLAYER_LAYER &&
            ballVariables.targetType == 1 &&
            collider.gameObject.TryGetComponent(out Bot bot) &&
            BotManager.botDict[ballVariables.botTargetID] == bot
        )
        {
            //Ball just hit target BOT
            if (!bot.CheckIfBlocking())
            {
                ballVariables.power = 1f;
                bot.HitBot();
                TryFindNewTarget(true, bot.CheckIfTargetClosest());
            }
            else
            {
                SpatialBridge.vfxService.CreateFloatingText("BLOCKED!", FloatingTextAnimStyle.Bouncy, transform.position, Vector3.up, Color.green);
                TryFindNewTarget(false, bot.CheckIfTargetClosest());
            }
        }
    }
}
