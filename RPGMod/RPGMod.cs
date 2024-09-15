using BepInEx;
using BepInEx.Logging;
using RoR2;
using RPGMod.Extensions;
using RPGMod.SoftDependencies;
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

    [BepInDependency(DependenciesManager.RiskOfOptionsGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class RpgMod : BaseUnityPlugin
    {
        private ModState _modState;

        public static ManualLogSource Log;

        public static RpgMod Instance;

        internal ModState ModState => _modState;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            _modState = ModState.Awaiting;

            // Load AssetBundle
            var execAssembly = Assembly.GetExecutingAssembly();
            var stream = execAssembly.GetManifestResourceStream("RPGMod.rpgmodbundle");
            UI.Utils.AssetBundle = AssetBundle.LoadFromStream(stream);

            if (UI.Utils.AssetBundle is null)
            {
                return;
            }

            DependenciesManager.CheckForDependencies();
            ConfigurationManager.Init(Config);

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
                if (ConfigValues.Questing.MissionCollectGoldEnabled &&
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
                    if (ConfigValues.Questing.MissionKillAnyEnabled)
                    {
                        Questing.Server.UnlockMissionType(Questing.MissionType.KillAny);
                        Questing.Events.AnyKilled.Invoke(1, null, attackerNetworkUser);
                    }

                    if (enemyBody.isFlying && ConfigValues.Questing.MissionKillFlyingEnabled)
                    {
                        Questing.Server.UnlockMissionType(Questing.MissionType.KillFlying);
                        Questing.Events.FlyingKilled.Invoke(1, null, attackerNetworkUser);
                    }

                    // Dynamic
                    if (!enemyBody.isBoss && ConfigValues.Questing.KillSpecificTypeEnabled)
                    {
                        Questing.Server.UnlockMissionType(Questing.MissionType.KillSpecificName);
                        Questing.Server.UnlockMonsterType(enemyBody);
                        Questing.Events.SpecificNameKilled.Invoke(1, enemyBody.baseNameToken, attackerNetworkUser);
                    }

                    if (enemyBody.isElite && ConfigValues.Questing.MissionKillSpecificBuffEnabled)
                    {
                        Questing.Server.UnlockMissionType(Questing.MissionType.KillSpecificBuff);

                        foreach (var buff in enemyBody.ActiveBuffsList().Select(BuffCatalog.GetBuffDef).Where(x => x.isElite))
                        {
                            Questing.Server.UnlockBuffType(buff);
                            Questing.Events.SpecificBuffKilled.Invoke(1, buff.eliteDef.modifierToken, attackerNetworkUser);
                        }
                    }

                    // By rarity
                    if (enemyBody.isChampion && ConfigValues.Questing.MissionKillChampionEnabled)
                    {
                        Questing.Server.UnlockMissionType(Questing.MissionType.KillChampion);
                        Questing.Events.ChampionKilled.Invoke(1, null, attackerNetworkUser);
                    }
                    else if (enemyBody.isElite && ConfigValues.Questing.MissionKillEliteEnabled)
                    {
                        Questing.Server.UnlockMissionType(Questing.MissionType.KillElite);
                        Questing.Events.EliteKilled.Invoke(1, null, attackerNetworkUser);
                    }
                    else if (ConfigValues.Questing.MissionKillCommonEnabled)
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
        }

        private void Setup()
        {
            Networking.Setup();
            RefreshUI();
        }

        private static void CleanUp()
        {
            Questing.Manager.CleanUp();
        }

        internal void RefreshUI()
        {
            StartCoroutine(UI.Utils.Setup());
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