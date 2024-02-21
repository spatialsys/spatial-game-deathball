using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpatialSys.UnitySDK;

/// <summary>
/// Controls a bot. Should only be actively used by host client.
/// </summary>
[RequireComponent(typeof(SpatialSyncedObject))]
public class Bot : MonoBehaviour
{
    private SpatialSyncedObject syncedObject;
    //! isHostClient param does not exist in current SDK version. This is a placeholder.
    private bool isHostClient => true;

    public float blockChance = 0.5f; // how likely out of 1 is the bot to block the ball

    private void OnEnable()
    {
        syncedObject = GetComponent<SpatialSyncedObject>();
        BotManager.RegisterBot(this);
    }

    private void OnDisable()
    {
        BotManager.DeregisterBot(this);
    }

    private void Update()
    {
        if (!OwnershipCheck()) return;
        // Do bot things:
        // beep boop
        // move around?
    }   

    public bool CheckIfBlocking()
    {
        return Random.value < blockChance;
    }

    public void HitBot()
    {
        SpatialBridge.vfxService.CreateFloatingText("Beep Ouch!", FloatingTextAnimStyle.Bouncy, transform.position, Vector3.up, Color.red);
    }

    private bool OwnershipCheck()
    {
        if (!isHostClient)
        {
            return false;
        }
        if (!syncedObject.isLocallyOwned)
        {
            syncedObject.TakeoverOwnership();
        }
        return true;
    }
}
