using System.Collections;
using System.Collections.Generic;
using SpatialSys.UnitySDK;
using UnityEngine;

[RequireComponent(typeof(SpatialSyncedObject))]
public class PlayerTrigger : MonoBehaviour
{
    public int actorID => syncedObject.ownerActorID;

    private SpatialSyncedObject syncedObject;

    private void Awake()
    {
        syncedObject = GetComponent<SpatialSyncedObject>();
    }

    void Update()
    {
        transform.position = SpatialBridge.actorService.actors[actorID].avatar.position;
    }
}
