using BepInEx;
using BepInEx.Logging;
using RoR2;
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
                if (NetworkServer.active && self?.GetComponent<PlayerCharacterMasterController>()?.networkUser?.netId != null)
                {
                    Questing.Server.CheckAllowedType(Questing.MissionType.CollectGold);
                    Questing.Events.GoldCollected.Invoke((int)amount, self.GetComponent<PlayerCharacterMasterController>().networkUser);
                }
                orig(self, amount);
            };

            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, self, damageReport) =>
            {
                var enemyBody = damageReport.victimBody;
                var damageInfo = damageReport.damageInfo;
                var attackerBody = damageInfo?.attacker?.GetComponent<CharacterBody>();
                var attackerNetworkUser = GetKillerNetworkUser(attackerBody?.masterObject?.GetComponent<CharacterMaster>());
                var enemyHasBuff = false;

                if (enemyBody != null && attackerBody != null && attackerNetworkUser?.netId != null)
                {
                    if (global::RPGMod.Config.Questing.KillAnyEnabled)
                    {
                        Questing.Server.CheckAllowedType(Questing.MissionType.KillAny);
                    }

                    Questing.Events.AnyKilled.Invoke(1, attackerNetworkUser);

                    if (enemyBody.isFlying)
                    {
                        if (global::RPGMod.Config.Questing.KillFlyingEnabled)
                        {
                            Questing.Server.CheckAllowedType(Questing.MissionType.KillFlying);
                        }

                        Questing.Events.FlyingKilled.Invoke(1, attackerNetworkUser);
                    }

                    // Buffs
                    if (enemyBody.HasBuff(RoR2Content.Buffs.AffixRed))
                    {
                        if (global::RPGMod.Config.Questing.KillRedEnabled)
                        {
                            Questing.Server.CheckAllowedType(Questing.MissionType.KillRed);
                        }

                        Questing.Events.RedKilled.Invoke(1, attackerNetworkUser);
                        enemyHasBuff = true;
                    }
                    if (enemyBody.HasBuff(RoR2Content.Buffs.AffixHaunted))
                    {
                        if (global::RPGMod.Config.Questing.KillHauntedEnabled)
                        {
                            Questing.Server.CheckAllowedType(Questing.MissionType.KillHaunted);
                        }

                        Questing.Events.HauntedKilled.Invoke(1, attackerNetworkUser);
                        enemyHasBuff = true;
                    }
                    if (enemyBody.HasBuff(RoR2Content.Buffs.AffixWhite))
                    {
                        if (global::RPGMod.Config.Questing.KillWhiteEnabled)
                        {
                            Questing.Server.CheckAllowedType(Questing.MissionType.KillWhite);
                        }

                        Questing.Events.WhiteKilled.Invoke(1, attackerNetworkUser);
                        enemyHasBuff = true;
                    }
                    if (enemyBody.HasBuff(RoR2Content.Buffs.AffixPoison))
                    {
                        if (global::RPGMod.Config.Questing.KillPoisonEnabled)
                        {
                            Questing.Server.CheckAllowedType(Questing.MissionType.KillPoison);
                        }

                        Questing.Events.PoisonKilled.Invoke(1, attackerNetworkUser);
                        enemyHasBuff = true;
                    }
                    if (enemyBody.HasBuff(RoR2Content.Buffs.AffixBlue))
                    {
                        if (global::RPGMod.Config.Questing.KillBlueEnabled)
                        {
                            Questing.Server.CheckAllowedType(Questing.MissionType.KillBlue);
                        }

                        Questing.Events.BlueKilled.Invoke(1, attackerNetworkUser);
                        enemyHasBuff = true;
                    }
                    if (enemyBody.HasBuff(RoR2Content.Buffs.AffixLunar))
                    {
                        if (global::RPGMod.Config.Questing.KillLunarEnabled)
                        {
                            Questing.Server.CheckAllowedType(Questing.MissionType.KillLunar);
                        }

                        Questing.Events.LunarKilled.Invoke(1, attackerNetworkUser);
                        enemyHasBuff = true;
                    }

                    // DLC1
                    if (enemyBody.HasBuff(DLC1Content.Buffs.EliteEarth))
                    {
                        if (global::RPGMod.Config.Questing.KillEarthEnabled)
                        {
                            Questing.Server.CheckAllowedType(Questing.MissionType.KillEarthDLC1);
                        }

                        Questing.Events.EarthKilledDLC1.Invoke(1, attackerNetworkUser);
                        enemyHasBuff = true;
                    }

                    if (enemyBody.HasBuff(DLC1Content.Buffs.EliteVoid))
                    {
                        if (global::RPGMod.Config.Questing.KillVoidEnabled)
                        {
                            Questing.Server.CheckAllowedType(Questing.MissionType.KillVoidDLC1);
                        }

                        Questing.Events.VoidKilledDLC1.Invoke(1, attackerNetworkUser);
                        enemyHasBuff = true;
                    }

                    // DLC2
                    if (enemyBody.HasBuff(DLC2Content.Buffs.EliteAurelionite))
                    {
                        if (global::RPGMod.Config.Questing.KillAurelioniteEnabled)
                        {
                            Questing.Server.CheckAllowedType(Questing.MissionType.KillAurelioniteDLC2);
                        }

                        Questing.Events.AurelioniteKilledDLC2.Invoke(1, attackerNetworkUser);
                        enemyHasBuff = true;
                    }

                    if (enemyBody.HasBuff(DLC2Content.Buffs.EliteBead))
                    {
                        if (global::RPGMod.Config.Questing.KillBeadEnabled)
                        {
                            Questing.Server.CheckAllowedType(Questing.MissionType.KillBeadDLC2);
                        }

                        Questing.Events.BeadKilledDLC2.Invoke(1, attackerNetworkUser);
                        enemyHasBuff = true;
                    }

                    // Kill any with buff
                    if (enemyHasBuff)
                    {
                        if (global::RPGMod.Config.Questing.KillAnyBuffEnabled)
                        {
                            Questing.Server.CheckAllowedType(Questing.MissionType.KillAnyBuff);
                        }

                        Questing.Events.AnyBuffKilled.Invoke(1, attackerNetworkUser);
                    }

                    // By rarity
                    if (enemyBody.isChampion)
                    {
                        if (global::RPGMod.Config.Questing.KillChampionEnabled)
                        {
                            Questing.Server.CheckAllowedType(Questing.MissionType.KillChampion);
                        }

                        Questing.Events.ChampionKilled.Invoke(1, attackerNetworkUser);
                    }
                    else if (enemyBody.isElite)
                    {
                        if (global::RPGMod.Config.Questing.KillEliteEnabled)
                        {
                            Questing.Server.CheckAllowedType(Questing.MissionType.KillElite);
                        }

                        Questing.Events.EliteKilled.Invoke(1, attackerNetworkUser);
                    }
                    else
                    {
                        if (global::RPGMod.Config.Questing.KillCommonEnabled)
                        {
                            Questing.Server.CheckAllowedType(Questing.MissionType.KillCommon);
                        }

                        Questing.Events.CommonKilled.Invoke(1, attackerNetworkUser);
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