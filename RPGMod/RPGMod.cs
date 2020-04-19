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
// Highlight

namespace RPGMod
{
[BepInPlugin("com.ghasttear1.rpgmod", "RPGMod", "3.0.0")]
class RPGMod : BaseUnityPlugin
{
    public bool gameStarted = false;
    void Awake()
    {
        global::RPGMod.Config.Load(Config, false);
        On.RoR2.Run.Start += (orig, self) =>
        {
            Setup();
            orig(self);
        };
        On.RoR2.Run.OnDisable += (orig, self) =>
        {
            CleanUp();
            orig(self);
        };
        On.RoR2.CharacterMaster.GiveMoney += (orig, self, amount) =>
        {
            if (NetworkServer.active && self?.GetComponent<PlayerCharacterMasterController>()?.networkUser?.playerControllerId != null) {
                Questing.QuestEvents.goldCollected.Invoke((int)amount, self.GetComponent<PlayerCharacterMasterController>().networkUser.playerControllerId);
            }
            orig(self, amount);
        };
        On.RoR2.PurchaseInteraction.OnInteractionBegin += (orig, self, activator) => {
            Debug.Log("INTERACTION");
            Debug.Log(Language.GetString(self?.displayNameToken ?? "???"));
            if (NetworkServer.active && self?.GetComponent<ChestBehavior>() != null) {
                Questing.QuestEvents.chestOpened.Invoke(1, activator.GetComponent<CharacterMaster>().GetComponent<PlayerCharacterMasterController>().networkUser.playerControllerId);
            }
            orig(self,activator);
        };
        On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, self, damageReport) =>
        {
            if (!damageReport.victim)
            {
                return;
            }
            CharacterBody enemyBody = damageReport?.victimBody;
            GameObject attackerMaster = damageReport?.damageInfo?.attacker?.GetComponent<CharacterBody>()?.masterObject;
            CharacterMaster attackerController = attackerMaster?.GetComponent<CharacterMaster>();

            if (attackerController?.GetComponent<PlayerCharacterMasterController>()?.networkUser?.playerControllerId != null) {
                if (enemyBody?.isElite ?? false) {
                    Questing.QuestEvents.eliteKilled.Invoke(1, attackerController.GetComponent<PlayerCharacterMasterController>().networkUser.playerControllerId);
                }
                else if (enemyBody?.isChampion ?? false) {
                    Questing.QuestEvents.championKilled.Invoke(1, attackerController.GetComponent<PlayerCharacterMasterController>().networkUser.playerControllerId);
                }
                else {
                    Questing.QuestEvents.commonKilled.Invoke(1, attackerController.GetComponent<PlayerCharacterMasterController>().networkUser.playerControllerId);
                }
            }
            orig(self, damageReport);
        };
    }

    void Update()
    {
        if (gameStarted) {
            if (NetworkServer.active) {
                Questing.Handler.Update();
                Networking.Sync();
            }
        }
    }

    void Setup() {
        Networking.RegisterHandlers();
        gameStarted = true;
        Questing.Handler.Setup();
    }
    void CleanUp() {
        gameStarted = false;
        Questing.Handler.CleanUp();
    }
}
} // namespace RPGMod