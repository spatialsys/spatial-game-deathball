using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpatialSys.UnitySDK;
using System;

public enum RPCID : byte
{
    Test = 0,
}

public class RPCManager : MonoBehaviour
{
    public static Action Test;

    private void Awake()
    {
        SpatialBridge.networkingService.remoteEvents.onEvent += HandleEvent;
    }

    private void OnDestroy()
    {
        SpatialBridge.networkingService.remoteEvents.onEvent -= HandleEvent;
        Test = null;
    }

    private void HandleEvent(NetworkingRemoteEventArgs args)
    {
        switch ((RPCID)args.eventID)
        {
            case RPCID.Test:
                Test?.Invoke();
                break;
        }
    }
}
