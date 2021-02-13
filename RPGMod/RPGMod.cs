using BepInEx;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using System.Reflection;

namespace RPGMod
{
enum ModState {
    awaiting,
    starting,
    started,
    ending
}
[BepInPlugin("com.ghasttear1.rpgmod", "RPGMod", "3.0.3")]
class RPGMod : BaseUnityPlugin
{
    private ModState modState;
    void Awake()
    {
        global::RPGMod.Config.Load(Config, false);
        modState = ModState.awaiting;

        // Load assetbundle
        var execAssembly = Assembly.GetExecutingAssembly();
        var stream = execAssembly.GetManifestResourceStream("RPGMod.rpgmodbundle");
        UI.Utils.assetBundle = AssetBundle.LoadFromStream(stream);

        if (UI.Utils.assetBundle is null)
        {
            Debug.LogError("RPGMOD: Failed to load assetbundle");
            return;
        }

        // Hooks
        On.RoR2.Run.Start += (orig, self) =>
        {
            modState = ModState.starting;
            orig(self);
        };
        On.RoR2.Run.OnDisable += (orig, self) =>
        {
            modState = ModState.ending;
            orig(self);
        };
        On.RoR2.CharacterMaster.GiveMoney += (orig, self, amount) =>
        {
            if (NetworkServer.active && self?.GetComponent<PlayerCharacterMasterController>()?.networkUser?.netId != null) {
                Questing.Events.goldCollected.Invoke((int)amount, self.GetComponent<PlayerCharacterMasterController>().networkUser.netId);
            }
            orig(self, amount);
        };
        On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, self, damageReport) =>
        {
            CharacterBody enemyBody = damageReport?.victimBody;
            GameObject attackerMaster = damageReport?.damageInfo?.attacker?.GetComponent<CharacterBody>()?.masterObject;
            CharacterMaster attackerController = attackerMaster?.GetComponent<CharacterMaster>();

            if (attackerController?.GetComponent<PlayerCharacterMasterController>()?.networkUser?.netId != null) {
                if (enemyBody?.isElite ?? false) {
                    Questing.Events.eliteKilled.Invoke(1, attackerController.GetComponent<PlayerCharacterMasterController>().networkUser.netId);
                }
                else {
                    Questing.Events.commonKilled.Invoke(1, attackerController.GetComponent<PlayerCharacterMasterController>().networkUser.netId);
                }
            }
            orig(self, damageReport);
        };
        // NETWORK BYPASS -- REMOVE
        // On.RoR2.Networking.GameNetworkManager.OnClientConnect += (self, user, t) => { };
    }

    void Update()
    {
        switch(modState) {
            case ModState.starting:
                Setup();
                modState = ModState.started;
                break;
            case ModState.started:
                if (UI.Utils.ready) {
                    Questing.Client.Update();
                    if (NetworkServer.active) {
                        Questing.Manager.Update();
                        Networking.Sync();
                    }
                }
                break;
            case ModState.ending:
                CleanUp();
                modState = ModState.awaiting;
                break;
            default:
                break;
        }
        // Reload config
        if (Input.GetKeyDown(KeyCode.F6))
        {
            global::RPGMod.Config.Load(Config, true);
            if (modState == ModState.started) {
                StartCoroutine(UI.Utils.Setup());
            }
            Questing.Client.CleanUp();
        }
    }

    void Setup() {
        Networking.Setup();
        StartCoroutine(UI.Utils.Setup());
    }
    void CleanUp() {
        Questing.Manager.CleanUp();
    }
}
} // namespace RPGMod