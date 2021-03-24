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
[BepInPlugin("com.ghasttear1.rpgmod", "RPGMod", "3.0.4")]
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
                Questing.Server.CheckAllowedType(Questing.QuestType.collectGold);
                Questing.Events.goldCollected.Invoke((int)amount, self.GetComponent<PlayerCharacterMasterController>().networkUser);
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
                    Questing.Server.CheckAllowedType(Questing.QuestType.killElite);
                    Questing.Events.eliteKilled.Invoke(1, attackerController.GetComponent<PlayerCharacterMasterController>().networkUser);
                }
                else {
                    Questing.Server.CheckAllowedType(Questing.QuestType.killCommon);
                    Questing.Events.commonKilled.Invoke(1, attackerController.GetComponent<PlayerCharacterMasterController>().networkUser);
                }
            }
            orig(self, damageReport);
        };
        // On.RoR2.SceneDirector.PopulateScene += (orig,self) => {
        //     Xoroshiro128Plus xoroshiro128Plus = new Xoroshiro128Plus((ulong)Run.instance.stageRng.nextUint);
        //     GameObject shrine = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(Resources.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscShrineChance"), new DirectorPlacementRule
        //     {
        //         placementMode = DirectorPlacementRule.PlacementMode.Random
        //     }, xoroshiro128Plus));
        //     Debug.Log(shrine.transform.position);
        //     shrine.GetComponent<ShrineChanceBehavior>().shrineColor = Color.cyan;
        //     shrine.GetComponent<ShrineChanceBehavior>().name = "quest";

        //     orig(self);
        // };
        On.RoR2.ShrineChanceBehavior.AddShrineStack += (orig,self,interactor) => {
            // if (self.name == "quest") {
            //     foreach(Questing.ClientData clientData in Questing.Server.ClientDatas) {
            //         if (Util.LookUpBodyNetworkUser(interactor.GetComponent<CharacterBody>()) == clientData.networkUser) {
            //             clientData.NewQuest();
            //         }
            //     }
            //     FieldInfo purchaseCountInfo = typeof(ShrineChanceBehavior).GetField("successfulPurchaseCount",BindingFlags.NonPublic | BindingFlags.Instance);
            //     purchaseCountInfo.SetValue(self, (int)purchaseCountInfo.GetValue(self) + 1);
            //     if ((int)purchaseCountInfo.GetValue(self) >= self.maxPurchaseCount)
            //     {
            //         self.symbolTransform.gameObject.SetActive(false);
            //     }
            // }
            Questing.Server.CheckAllowedType(Questing.QuestType.attemptShrineChance);
            Questing.Events.chanceShrineAttempted.Invoke(1, Util.LookUpBodyNetworkUser(interactor.GetComponent<CharacterBody>()));
            orig(self,interactor);
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
        // if (Input.GetKeyDown(KeyCode.F8))
        // {
        //     Questing.Server.ClientDatas[0].QuestData.CompleteQuest();
        // }
        // if (Input.GetKeyDown(KeyCode.F7)) {
        //     Debug.Log(PlayerCharacterMasterController.instances[0].GetComponent<CharacterBody>().transform.position);
        // }
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