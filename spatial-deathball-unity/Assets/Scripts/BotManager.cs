using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotManager : MonoBehaviour
{
    public static BotManager instance;
    public static List<Bot> bots = new List<Bot>();
    public static Dictionary<string,Bot> botDict = new Dictionary<string, Bot>();

    private void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        instance = null;
    }

    public static void RegisterBot(Bot bot)
    {
        bots.Add(bot);
        botDict.Add(bot.syncedObject.InstanceID, bot);
    }

    public static void DeregisterBot(Bot bot)
    {
        bots.Remove(bot);
        botDict.Remove(bot.syncedObject.InstanceID);
    }
}
