using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpatialSys.UnitySDK;
using System;

/// <summary>
/// As of writing we don't have proper synced object C# support.
/// They where originally built for Visual Scripting and the Variables component.
/// This script handles passing variable events and changes to the ballControl
/// </summary>
public class BallVariableSync : MonoBehaviour
{
    public delegate void EventListenerDelegate(string eventName, object[] args);
    private EventListenerDelegate _eventListener;

    public bool ownedLocally { get; private set; }

    // Potentially called the frame after set() is called. Not sure rn.
    public event Action<bool> OnOwnershipChanged;
    public event Action<int> OnTargetTypeChanged;
    public event Action<int> OnTargetIDChanged;
    public event Action<float> OnPowerChanged;

    private int _targetType;
    public int targetType
    {
        get
        {
            return _targetType;
        }
        set
        {
            if (ownedLocally)
            {
                VisualScriptingUtility.TriggerCustomEvent(gameObject, "SetTargetType", value);
            }
        }
    }

    private int _targetID;
    public int targetID
    {
        get
        {
            return _targetID;
        }
        set
        {
            if (ownedLocally)
            {
                VisualScriptingUtility.TriggerCustomEvent(gameObject, "SetTargetID", value);
            }
        }
    }

    private float _power;
    public float power
    {
        get
        {
            return _power;
        }
        set
        {
            if (ownedLocally)
            {
                VisualScriptingUtility.TriggerCustomEvent(gameObject, "SetPower", value);
            }
        }
    }

    private Delegate listener;

    private void OnEnable()
    {
        _eventListener = EventListener;
        ownedLocally = GetComponent<SpatialSyncedObject>().isLocallyOwned;
        listener = VisualScriptingUtility.AddCustomEventListener(gameObject, EventListener);
    }

    private void OnDisable()
    {
        VisualScriptingUtility.RemoveCustomEventListener(gameObject, listener);
    }

    private void EventListener(string eventName, object[] args)
    {
        switch (eventName)
        {
            case "OnTargetTypeChanged":
                _targetType = (int)args[0];
                OnTargetTypeChanged?.Invoke(_targetType);
                break;
            case "OnTargetIDChanged":
                _targetID = (int)args[0];
                OnTargetIDChanged?.Invoke(_targetID);
                break;
            case "OnPowerChanged":
                _power = (float)args[0];
                OnPowerChanged?.Invoke(_power);
                break;
            case "OnOwnerChanged":
                ownedLocally = (int)args[0] == SpatialBridge.actorService.localActorNumber;
                OnOwnershipChanged?.Invoke(ownedLocally);
                break;
        }
    }

    public void TakeoverOwnership()
    {
        if (!ownedLocally)
        {
            VisualScriptingUtility.TriggerCustomEvent(gameObject, "TakeoverOwnership");
        }
    }
}
