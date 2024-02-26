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

    // Potentially called the frame after set() is called. Not sure rn.
    public event Action<bool> OnOwnershipChanged;
    public event Action<int> OnTargetTypeChanged;
    public event Action<int> OnTargetIDChanged;
    public event Action<string> OnBotTargetIDChanged;
    public event Action<float> OnPowerChanged;

    // 0 = actor, 1 = `bot.cs`
    private int _targetType;
    public int targetType
    {
        get
        {
            return _targetType;
        }
        set
        {
            VisualScriptingUtility.TriggerCustomEvent(gameObject, "SetTargetType", value);
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
            VisualScriptingUtility.TriggerCustomEvent(gameObject, "SetTargetID", value);
        }
    }

    private string _botTargetID;
    public string botTargetID
    {
        get
        {
            return _botTargetID;
        }
        set
        {
            VisualScriptingUtility.TriggerCustomEvent(gameObject, "SetBotTargetID", value);
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
            VisualScriptingUtility.TriggerCustomEvent(gameObject, "SetPower", value);
        }
    }

    private Delegate listener;

    private void OnEnable()
    {
        _eventListener = EventListener;
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
            case "OnBotTargetIDChanged":
                _botTargetID = (string)args[0];
                OnBotTargetIDChanged?.Invoke((string)args[0]);
                break;
        }
    }
}
