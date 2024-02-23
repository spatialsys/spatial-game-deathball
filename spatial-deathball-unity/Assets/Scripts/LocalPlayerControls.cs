using System.Collections;
using System.Collections.Generic;
using SpatialSys.UnitySDK;
using UnityEngine;

public class LocalPlayerControls : MonoBehaviour
{
    void Update()
    {
        //super basic implementation of how ACTORS will block. TLDR they set a custom property bool. 
        //These get auto synced to all clients so host can read them.
        if (Input.GetKeyDown(KeyCode.F))
        {
            SpatialBridge.actorService.localActor.SetCustomProperty("isBlocking", true);
        }
        if (Input.GetKeyUp(KeyCode.F))
        {
            SpatialBridge.actorService.localActor.SetCustomProperty("isBlocking", false);
        }
    }
}
