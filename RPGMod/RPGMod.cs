using BepInEx;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using System.Reflection;
using BepInEx.Logging;

namespace RPGMod
{
    internal enum ModState
    {
        awaiting,
        starting,
        started,
        ending
    }

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class RPGMod : BaseUnityPlugin
    {
        private ModState modState;

        public static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;

            global::RPGMod.Config.Load(Config, false);
            modState = ModState.awaiting;

            // Load AssetBundle
            var execAssembly = Assembly.GetExecutingAssembly();
            var stream = execAssembly.GetManifestResourceStream("RPGMod.rpgmodbundle");
            UI.Utils.assetBundle = AssetBundle.LoadFromStream(stream);

            if (UI.Utils.assetBundle is null)
            {
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
                if (NetworkServer.active && self?.GetComponent<PlayerCharacterMasterController>()?.networkUser?.netId != null)
                {
                    Questing.Server.CheckAllowedType(Questing.QuestType.collectGold);
                    Questing.Events.goldCollected.Invoke((int)amount, self.GetComponent<PlayerCharacterMasterController>().networkUser);
                }
                orig(self, amount);
            };

            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, self, damageReport) =>
            {
                var enemyBody = damageReport?.victimBody;
                var attackerMaster = damageReport?.damageInfo?.attacker?.GetComponent<CharacterBody>()?.masterObject;
                var attackerController = attackerMaster?.GetComponent<CharacterMaster>();
                var attackerNetworkUser = GetKillerNetworkUser(attackerController);

                if (enemyBody != null && attackerNetworkUser?.netId != null)
                {
                    if (enemyBody.isFlying)
                    {
                        Questing.Server.CheckAllowedType(Questing.QuestType.killFlying);
                        Questing.Events.flyingKilled.Invoke(1, attackerNetworkUser);
                    }

                    // Buffs
                    if (enemyBody.HasBuff(RoR2Content.Buffs.AffixHaunted))
                    {
                        Questing.Server.CheckAllowedType(Questing.QuestType.killHaunted);
                        Questing.Events.hauntedKilled.Invoke(1, attackerNetworkUser);
                    }
                    if (enemyBody.HasBuff(RoR2Content.Buffs.AffixPoison))
                    {
                        Questing.Server.CheckAllowedType(Questing.QuestType.killPoison);
                        Questing.Events.poisonKilled.Invoke(1, attackerNetworkUser);
                    }

                    // By rarity
                    if (enemyBody.isChampion)
                    {
                        Questing.Server.CheckAllowedType(Questing.QuestType.killChampion);
                        Questing.Events.championKilled.Invoke(1, attackerNetworkUser);
                    }
                    else if (enemyBody.isElite)
                    {
                        Questing.Server.CheckAllowedType(Questing.QuestType.killElite);
                        Questing.Events.eliteKilled.Invoke(1, attackerNetworkUser);
                    }
                    else
                    {
                        Questing.Server.CheckAllowedType(Questing.QuestType.killCommon);
                        Questing.Events.commonKilled.Invoke(1, attackerNetworkUser);
                    }
                }

                orig(self, damageReport);
            };

            On.RoR2.Run.OnServerSceneChanged += (orig, self, interactor) =>
            {
                Questing.Manager.CleanUp();

                orig(self, interactor);
            };
        }

        private void Update()
        {
            switch (modState)
            {
                case ModState.starting:
                    Setup();
                    modState = ModState.started;

                    break;

                case ModState.started:
                    if (UI.Utils.ready)
                    {
                        Questing.Client.Update();

                        if (NetworkServer.active)
                        {
                            Questing.Manager.Update();
                            Networking.Sync();
                        }
                    }

                    break;

                case ModState.ending:
                    CleanUp();
                    modState = ModState.awaiting;

                    break;
            }

            // Reload config
            // ReSharper disable once InvertIf
            if (Input.GetKeyDown(KeyCode.F6))
            {
                global::RPGMod.Config.Load(Config, true);
                if (modState == ModState.started)
                {
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

        private void Setup()
        {
            Networking.Setup();
            StartCoroutine(UI.Utils.Setup());
        }

        private static void CleanUp()
        {
            Questing.Manager.CleanUp();
        }

        private static NetworkUser GetKillerNetworkUser(CharacterMaster master)
        {
            if (master?.playerCharacterMasterController?.networkUser)
            {
                return master?.playerCharacterMasterController?.networkUser;
            }

            if (master?.minionOwnership?.ownerMaster?.playerCharacterMasterController?.networkUser)
            {
                return master?.minionOwnership?.ownerMaster?.playerCharacterMasterController?.networkUser;
            }

            return null;
        }
    }
}