using BepInEx;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

// Quests are updated by events
// Quests are received by a random time between 30-45s after completion
// Quests consist of 1-3 components for the rarity of the reward respectively
// C - 1, U - 2, L - 3
// Quests stay at a static difficulty as the game scales anyways
// Quests have an announcer with a radio icon, text AC style
// Top right, quest components listed
// Quests will ensure completion is achievable in the same stage
// Option for individual or group tasks with appropriate scaling
// Players will each be assigned their data - class PlayerData
// ClientData fixed sync every 100ms
// ServerData
// All functions should run regardless and should be thoroughly thought out
// UI must be flawless and dynamic regardless of situation

namespace RPGMod
{
[BepInPlugin("com.ghasttear1.rpgmod", "RPGMod", "3.0.0")]
class RPGMod : BaseUnityPlugin
{
    void Awake()
    {

    }

    void Update()
    {
        Questing.Handler.Update();


        Networking.Sync();
    }

    void Setup()
    {

    }
    void Cleanup()
    {

    }
}
} // namespace RPGMod