using System.Collections;
using System.Collections.Generic;
using SpatialSys.UnitySDK;
using UnityEngine;

[RequireComponent(typeof(BallVariableSync))]
public class BallControl : MonoBehaviour
{
    private const int LOCAL_PLAYER_LAYER = 7;
    private const int BOT_PLAYER_LAYER = 6;

    public float forceByPower = 10f;// how much force we add to the ball
    public float rotationByPower = 1f;// how much we rotate the ball's velocity

    private BallVariableSync ballVariables;
    private Rigidbody rb;

    //super temp
    public SpatialSyncedObject bot;

    private void Start()
    {
        ballVariables = GetComponent<BallVariableSync>();
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (ballVariables.ownedLocally)
        {
            OwnedUpdate();
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.R))
        {
            FindNewTarget();
        }
    }

    private void OwnedUpdate()
    {
        // todo lazy
        Vector3 targetPos = Vector3.zero;
        if (ballVariables.targetType == 0)
        {
            if (SpatialBridge.actorService.actors.TryGetValue(ballVariables.targetID, out var actor))
            {
                targetPos = actor.avatar.position + Vector3.up;
            }
            else
            {
                // Targeting an actor that doesn't exist!
                FindNewTarget();
                return;
            }
        }
        else if (ballVariables.targetType == 1)
        {
            var obj = SpatialBridge.spaceContentService.GetSyncedObjectByID(ballVariables.targetID);
            if (obj != null)
            {
                targetPos = obj.transform.position + Vector3.up;
            }
            else
            {
                // Targeting an object that doesn't exist!
                FindNewTarget();
                return;
            }
        }
        else
        {
            throw new System.Exception("Unknown target type");
        }

        rb.AddForce((targetPos - transform.position).normalized * forceByPower * Time.fixedDeltaTime);
        //rotate rb velocity towards target
        Vector3 velocity = rb.velocity;
        Vector3 targetDir = (targetPos - transform.position).normalized;
        Vector3 newDir = Vector3.RotateTowards(velocity.normalized, targetDir, rotationByPower * Time.fixedDeltaTime, 10f);
        rb.velocity = velocity.magnitude * newDir;
    }

    private void FindNewTarget()
    {
        //super temp
        if (ballVariables.targetType == 0)
        {
            ballVariables.targetType = 1;
            ballVariables.targetID = bot.syncedObjectID;
            rb.AddForce(Vector3.up * 20f, ForceMode.Impulse);
            return;
        }
        ballVariables.targetType = 0;
        ballVariables.targetID = SpatialBridge.actorService.localActorNumber;
        rb.AddForce(Vector3.up * 20f, ForceMode.Impulse);
        return;
    }

    //! Ball Collision
    private void OnTriggerEnter(Collider collider)
    {
        if (!ballVariables.ownedLocally || rb == null)
        {
            return;
        }

        if (collider.gameObject.layer == LOCAL_PLAYER_LAYER && ballVariables.targetType == 0 && ballVariables.targetID == SpatialBridge.actorService.localActorNumber)
        {
            // we just collided with local player, and they where the target.
            if (Input.GetKey(KeyCode.F))
            {
                //blocked
                SpatialBridge.vfxService.CreateFloatingText("BLOCKED!", FloatingTextAnimStyle.Bouncy, transform.position, Vector3.up, Color.green);
            }
            else
            {
                //hit
                SpatialBridge.vfxService.CreateFloatingText("HIT", FloatingTextAnimStyle.Bouncy, transform.position, Vector3.up, Color.red);
            }
            rb.velocity = Vector3.zero;
            FindNewTarget();
        }
        else if (
            collider.gameObject.layer == BOT_PLAYER_LAYER &&
            ballVariables.targetType == 1 &&
            collider.gameObject.TryGetComponent(out SpatialSyncedObject syncedObject) &&
            syncedObject.syncedObjectID == ballVariables.targetID
        )
        {
            // we just collided with a bot
            SpatialBridge.vfxService.CreateFloatingText("Beep Boop", FloatingTextAnimStyle.Bouncy, transform.position, Vector3.up, Color.magenta);
            rb.velocity = Vector3.zero;
            FindNewTarget();
        }
    }
}
