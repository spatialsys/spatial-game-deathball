using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpatialSys.UnitySDK;
using TMPro;

public class ActorBlockStateDebug : MonoBehaviour
{
    public TextMeshProUGUI text;

    void Update()
    {
        //lazy, just reprint every frame
        string str = "";
foreach(IActor actor in SpatialBridge.actorService.actors.Values)
{
    string blockState;
    if (actor.customProperties.TryGetValue("isBlocking",out object state))
    {
        blockState = (bool)state ? "<color=green>Blocking</color>" : "<color=red>Not Blocking</color>";
    }
    else
    {
        blockState = "<color=yellow>Missing State</color>";
    }
            str += $"{actor.actorNumber} : {blockState}";
            if (actor.actorNumber == SpatialBridge.actorService.localActor.actorNumber)
            {
                str += " <b>(You)</b>";
            }
            str += "\n";
        }
        text.text = str;
    }
}
