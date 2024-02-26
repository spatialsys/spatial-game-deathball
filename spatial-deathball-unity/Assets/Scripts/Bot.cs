using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpatialSys.UnitySDK;
using UnityEngine.AI;

/// <summary>
/// Controls a bot. Should only be actively used by host client.
/// </summary>
[RequireComponent(typeof(SpatialSyncedObject)), RequireComponent(typeof(NavMeshAgent))]
public class Bot : MonoBehaviour
{
    public SpatialSyncedObject syncedObject { get; private set; }
    private NavMeshAgent navMeshAgent;

    public float newPositionEvery = 2f; // how often to move to a new position
    public float blockChance = 0.5f; // how likely out of 1 is the bot to block the ball

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
        // do bot ai stuff
        // beep boop
        newPosTimer += Time.deltaTime;
        if (newPosTimer >= newPositionEvery)
        {
            newPosTimer = 0;
            Vector3 randomDirection = Random.insideUnitSphere * 10f;
            randomDirection += transform.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, 25f, 1))
            {
                navMeshAgent.SetDestination(hit.position);
            }
        }
    }

    public bool CheckIfBlocking()
    {
        return Random.value < blockChance;
    }

    public void HitBot()
    {
        SpatialBridge.vfxService.CreateFloatingText("Beep Ouch!", FloatingTextAnimStyle.Bouncy, transform.position, Vector3.up, Color.red);
    }

    private bool IsBallTargetingMe()
    {
        return BallControl.instance.ballVariables.targetType == 1 && BallControl.instance.ballVariables.botTargetID == syncedObject.InstanceID;
    }
}
