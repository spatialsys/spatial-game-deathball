using System.Collections;
using System.Collections.Generic;
using SpatialSys.UnitySDK;
using UnityEngine;

public class SnapToPlayer : MonoBehaviour
{
    void Update()
    {
        transform.position = SpatialBridge.actorService.localActor.avatar.position;
    }
}
