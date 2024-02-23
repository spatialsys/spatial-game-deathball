using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpatialSys.UnitySDK;
using TMPro;

public class ActorListDebug : MonoBehaviour
{
    public TextMeshProUGUI text;

    void Update()
    {
        string str = "";
        foreach (IActor actor in SpatialBridge.actorService.actors.Values)
        {
            string actorState;
            if (actor.isDisposed)
            {
                actorState = "<color=red>Disposed</color>";
            }
            else
            {
                actorState = $"<color=green>{actor.username}</color>";
            }
            str += $"{actor.actorNumber} : {actorState}";
            if (actor.actorNumber == SpatialBridge.actorService.localActor.actorNumber)
            {
                str += " <b>(You)</b>";
            }
            str += "\n";
        }
        text.text = str;
    }
}
