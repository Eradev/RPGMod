using BepInEx;
using BepInEx.Logging;
using RoR2;
using RPGMod.Extensions;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace RPGMod
{
    internal enum ModState
    {
        Awaiting,
        Starting,
        Started,
        Ending
    }

    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class RpgMod : BaseUnityPlugin
    {
        private ModState _modState;

        public static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;

            global::RPGMod.Config.Load(Config, false);
            _modState = ModState.Awaiting;

            // Load AssetBundle
            var execAssembly = Assembly.GetExecutingAssembly();
            var stream = execAssembly.GetManifestResourceStream("RPGMod.rpgmodbundle");
            UI.Utils.AssetBundle = AssetBundle.LoadFromStream(stream);

            if (UI.Utils.AssetBundle is null)
            {
                return;
            }

            // Hooks
            On.RoR2.Run.Start += (orig, self) =>
            {
                _modState = ModState.Starting;
                orig(self);
            };

            On.RoR2.Run.OnDisable += (orig, self) =>
            {
                _modState = ModState.Ending;
                orig(self);
            };

            On.RoR2.CharacterMaster.GiveMoney += (orig, self, amount) =>
            {
                if (global::RPGMod.Config.Questing.CollectGoldEnabled &&
                    NetworkServer.active &&
                    self?.GetComponent<PlayerCharacterMasterController>()?.networkUser?.netId != null)
                {
                    Questing.Server.UnlockMissionType(Questing.MissionType.CollectGold);
                    Questing.Events.GoldCollected.Invoke((int)amount, null, self.GetComponent<PlayerCharacterMasterController>().networkUser);
                }

                orig(self, amount);
            };

            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, self, damageReport) =>
            {
                var enemyBody = damageReport.victimBody;
                var damageInfo = damageReport.damageInfo;
                var attackerBody = damageInfo?.attacker?.GetComponent<CharacterBody>();
                var attackerNetworkUser = GetKillerNetworkUser(attackerBody?.masterObject?.GetComponent<CharacterMaster>());

                if (enemyBody != null && attackerBody != null && attackerNetworkUser?.netId != null)
                {
                    if (global::RPGMod.Config.Questing.KillAnyEnabled)
                    {
                        Questing.Server.UnlockMissionType(Questing.MissionType.KillAny);
                        Questing.Events.AnyKilled.Invoke(1, null, attackerNetworkUser);
                    }

                    if (enemyBody.isFlying && global::RPGMod.Config.Questing.KillFlyingEnabled)
                    {
                        Questing.Server.UnlockMissionType(Questing.MissionType.KillFlying);
                        Questing.Events.FlyingKilled.Invoke(1, null, attackerNetworkUser);
                    }

                    // Dynamic
                    if (!enemyBody.isBoss && global::RPGMod.Config.Questing.KillSpecificNameEnabled)
                    {
                        Questing.Server.UnlockMissionType(Questing.MissionType.KillSpecificName);
                        Questing.Server.UnlockMonsterType(enemyBody);
                        Questing.Events.SpecificNameKilled.Invoke(1, enemyBody.baseNameToken, attackerNetworkUser);
                    }

                    if (enemyBody.isElite && global::RPGMod.Config.Questing.KillSpecificBuffEnabled)
                    {
                        Questing.Server.UnlockMissionType(Questing.MissionType.KillSpecificBuff);

                        foreach (var buff in enemyBody.ActiveBuffsList().Select(BuffCatalog.GetBuffDef).Where(x => x.isElite))
                        {
                            Questing.Server.UnlockBuffType(buff);
                            Questing.Events.SpecificBuffKilled.Invoke(1, buff.eliteDef.modifierToken, attackerNetworkUser);
                        }
                    }

                    // By rarity
                    if (enemyBody.isChampion && global::RPGMod.Config.Questing.KillChampionEnabled)
                    {
                        Questing.Server.UnlockMissionType(Questing.MissionType.KillChampion);
                        Questing.Events.ChampionKilled.Invoke(1, null, attackerNetworkUser);
                    }
                    else if (enemyBody.isElite && global::RPGMod.Config.Questing.KillEliteEnabled)
                    {
                        Questing.Server.UnlockMissionType(Questing.MissionType.KillElite);
                        Questing.Events.EliteKilled.Invoke(1, null, attackerNetworkUser);
                    }
                    else if (global::RPGMod.Config.Questing.KillCommonEnabled)
                    {
                        Questing.Server.UnlockMissionType(Questing.MissionType.KillCommon);
                        Questing.Events.CommonKilled.Invoke(1, null, attackerNetworkUser);
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
            switch (_modState)
            {
                case ModState.Starting:
                    Setup();
                    _modState = ModState.Started;

                    break;

                case ModState.Started:
                    if (UI.Utils.IsReady)
                    {
                        Questing.Client.Update();

                        if (NetworkServer.active)
                        {
                            Questing.Manager.Update();
                            Networking.Sync();
                        }
                    }

                    break;

                case ModState.Ending:
                    CleanUp();
                    _modState = ModState.Awaiting;

                    break;
            }

            // Reload config
            // ReSharper disable once InvertIf
            if (Input.GetKeyDown(KeyCode.F6))
            {
                global::RPGMod.Config.Load(Config, true);
                if (_modState == ModState.Started)
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

            return master?.minionOwnership?.ownerMaster?.playerCharacterMasterController?.networkUser
                ? master?.minionOwnership?.ownerMaster?.playerCharacterMasterController?.networkUser
                : null;
        }
    }
}