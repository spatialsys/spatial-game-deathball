using System.Collections;
using System.Collections.Generic;
using SpatialSys.UnitySDK;
using UnityEngine;

public class SceneINIT : MonoBehaviour
{
    public GameObject playerPrefab;
    void Start()
    {
        StartCoroutine(ConnectCoroutine());
    }

    private IEnumerator ConnectCoroutine()
    {
        yield return new WaitUntil(() => SpatialBridge.networkingService.connectionStatus == ServerConnectionStatus.Connected);
        var trig = Instantiate(playerPrefab).GetComponent<SpatialSyncedObject>();
    }
}
