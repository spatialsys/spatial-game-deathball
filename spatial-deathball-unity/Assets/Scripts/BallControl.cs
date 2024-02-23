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
    private const int PLAYER_TRIGGER_LAYER = 7;
    private const int BOT_PLAYER_LAYER = 6;

    public float powerPerSecond = .1f;// power resets on HIT
    public float maxPower = 10f;
    public float forceByPower = 10f;// how much force we add to the ball per power
    public float rotationByPower = 1f;// how much we rotate the ball's velocity per power

    private BallVariableSync ballVariables;
    private Rigidbody rb;
    private SpatialSyncedObject syncedObject;

    private Transform previousTarget;
    private Transform target;

    private void Start()
    {
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

    private bool TryFindNewTarget(bool resetVelocity)
    {
        previousTarget = target;
        int totalPlayers = SpatialBridge.actorService.actors.Count + BotManager.bots.Count;
        int randomPlayer;

        // cringe:
        // todo proper find next target logic
        int lastTargetExtendedID = ballVariables.targetID + (ballVariables.targetType == 0 ? 0 : SpatialBridge.actorService.actors.Count);
        do 
        {
            randomPlayer = Random.Range(0, totalPlayers);
        } while (randomPlayer == lastTargetExtendedID);

        if (randomPlayer < SpatialBridge.actorService.actors.Count)
        {
            try
            {
                int targetActorID = SpatialBridge.actorService.actors.ElementAt(randomPlayer).Key;
                target = SpatialBridge.actorService.actors[targetActorID].avatar.GetAvatarBoneTransform(HumanBodyBones.Chest);
                ballVariables.targetID = targetActorID;
                ballVariables.targetType = 0;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to find chest bone on player " + randomPlayer + " " + e);
                target = null;
                return false;
            }
        }
        else
        {
            target = BotManager.bots[randomPlayer - SpatialBridge.actorService.actors.Count].transform;
            ballVariables.targetID = randomPlayer - SpatialBridge.actorService.actors.Count;
            ballVariables.targetType = 1;
        }
        Debug.LogError($"New target: {target.name}, {ballVariables.targetID}, {ballVariables.targetType}");

        if (resetVelocity)
        {
            rb.velocity = Vector3.up * 6f;
        }
        else
        {
            rb.velocity = (target.position - transform.position).normalized * rb.velocity.magnitude;
        }
        return true;
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
            BotManager.bots[ballVariables.targetID] == bot
        )
        {
            //Ball just hit target BOT
            if (!bot.CheckIfBlocking())
            {
                ballVariables.power = 1f;
                bot.HitBot();
                TryFindNewTarget(true);
            }
            else
            {
                SpatialBridge.vfxService.CreateFloatingText("BLOCKED!", FloatingTextAnimStyle.Bouncy, transform.position, Vector3.up, Color.green);
                TryFindNewTarget(false);
            }
        }
    }
}
