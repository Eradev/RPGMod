using BepInEx;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

// Quests are updated by events -- DONE
// Quests are received by a random time between 30-45s after completion -- DONE
// Quests consist of 1-3 components for the rarity of the reward respectively -- DONE
// C - 1, U - 2, L - 3 -- DONE
// Quests stay at a static difficulty as the game scales anyways
// Quests have an announcer with a radio icon, text AC style
// Top right, quest components listed
// Quests will ensure completion is achievable in the same stage
// Option for individual or group tasks with appropriate scaling
// Players will each be assigned their data - class PlayerData
// ClientData fixed sync every 100ms -- DONE
// ServerData -- DONE
// All functions should run regardless and should be thoroughly thought out
// UI must be flawless and dynamic regardless of situation
// Highlight -- NO

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
            if (NetworkServer.active && self?.GetComponent<PlayerCharacterMasterController>()?.networkUser?.netId != null) {
                Questing.Events.goldCollected.Invoke((int)amount, self.GetComponent<PlayerCharacterMasterController>().networkUser.netId);
            }
            orig(self, amount);
        };
        // On.RoR2.PurchaseInteraction.OnInteractionBegin += (orig, self, activator) => {
        //     Debug.Log("INTERACTION");
        //     Debug.Log(Language.GetString(self?.displayNameToken ?? "???"));
        //     Debug.Log(self?.lockGameObject);
        //     if (NetworkServer.active && self?.GetComponent<ChestBehavior>() != null) {
        //         Questing.Events.chestOpened.Invoke(1, activator.GetComponent<CharacterMaster>().GetComponent<PlayerCharacterMasterController>().networkUser.playerControllerId);
        //     }
        //     orig(self,activator);
        // };
        On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, self, damageReport) =>
        {
            if (!damageReport.victim)
            {
                return;
            }
            CharacterBody enemyBody = damageReport?.victimBody;
            GameObject attackerMaster = damageReport?.damageInfo?.attacker?.GetComponent<CharacterBody>()?.masterObject;
            CharacterMaster attackerController = attackerMaster?.GetComponent<CharacterMaster>();

            if (attackerController?.GetComponent<PlayerCharacterMasterController>()?.networkUser?.netId != null) {
                if (enemyBody?.isElite ?? false) {
                    Questing.Events.eliteKilled.Invoke(1, attackerController.GetComponent<PlayerCharacterMasterController>().networkUser.netId);
                }
                else if (enemyBody?.isChampion ?? false) {
                    //Questing.Events.championKilled.Invoke(1, attackerController.GetComponent<PlayerCharacterMasterController>().networkUser.playerControllerId);
                }
                else {
                    Questing.Events.commonKilled.Invoke(1, attackerController.GetComponent<PlayerCharacterMasterController>().networkUser.netId);
                }
            }
            orig(self, damageReport);
        };
        // NETWORK BYPASS -- REMOVE
        On.RoR2.Networking.GameNetworkManager.OnClientConnect += (self, user, t) => { };
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