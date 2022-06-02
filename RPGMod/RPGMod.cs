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

                if (enemyBody != null && attackerNetworkUser?.netId != null)
                {
                    if (enemyBody.isFlying)
                    {
                        if (global::RPGMod.Config.Questing.killFlyingEnabled)
                        {
                            Questing.Server.CheckAllowedType(Questing.QuestType.killFlying);
                        }

                        Questing.Events.FlyingKilled.Invoke(1, attackerNetworkUser);
                    }

                    // ReSharper disable once PossibleNullReferenceException
                    if ((damageInfo.damageType & DamageType.DoT) != DamageType.DoT &&
                        (
                            damageInfo.procChainMask.HasProc(ProcType.Backstab) ||
                            // ReSharper disable once PossibleNullReferenceException
                            BackstabManager.IsBackstab(attackerBody.corePosition - damageInfo.position, enemyBody)
                        ))
                    {
                        if (global::RPGMod.Config.Questing.killByBackstabEnabled)
                        {
                            Questing.Server.CheckAllowedType(Questing.QuestType.killByBackstab);
                        }

                        Questing.Events.KilledByBackstab.Invoke(1, attackerNetworkUser);
                    }

                    // Buffs
                    if (enemyBody.HasBuff(RoR2Content.Buffs.AffixRed))
                    {
                        if (global::RPGMod.Config.Questing.killRedEnabled)
                        {
                            Questing.Server.CheckAllowedType(Questing.QuestType.killRed);
                        }

                        Questing.Events.RedKilled.Invoke(1, attackerNetworkUser);
                    }
                    if (enemyBody.HasBuff(RoR2Content.Buffs.AffixHaunted))
                    {
                        if (global::RPGMod.Config.Questing.killHauntedEnabled)
                        {
                            Questing.Server.CheckAllowedType(Questing.QuestType.killHaunted);
                        }

                        Questing.Events.HauntedKilled.Invoke(1, attackerNetworkUser);
                    }
                    if (enemyBody.HasBuff(RoR2Content.Buffs.AffixWhite))
                    {
                        if (global::RPGMod.Config.Questing.killWhiteEnabled)
                        {
                            Questing.Server.CheckAllowedType(Questing.QuestType.killWhite);
                        }

                        Questing.Events.WhiteKilled.Invoke(1, attackerNetworkUser);
                    }
                    if (enemyBody.HasBuff(RoR2Content.Buffs.AffixPoison))
                    {
                        if (global::RPGMod.Config.Questing.killPoisonEnabled)
                        {
                            Questing.Server.CheckAllowedType(Questing.QuestType.killPoison);
                        }

                        Questing.Events.PoisonKilled.Invoke(1, attackerNetworkUser);
                    }
                    if (enemyBody.HasBuff(RoR2Content.Buffs.AffixBlue))
                    {
                        if (global::RPGMod.Config.Questing.killBlueEnabled)
                        {
                            Questing.Server.CheckAllowedType(Questing.QuestType.killBlue);
                        }

                        Questing.Events.BlueKilled.Invoke(1, attackerNetworkUser);
                    }
                    if (enemyBody.HasBuff(RoR2Content.Buffs.AffixLunar))
                    {
                        if (global::RPGMod.Config.Questing.killLunarEnabled)
                        {
                            Questing.Server.CheckAllowedType(Questing.QuestType.killLunar);
                        }

                        Questing.Events.LunarKilled.Invoke(1, attackerNetworkUser);
                    }

                    // DLC1
                    if (enemyBody.HasBuff(DLC1Content.Buffs.EliteEarth))
                    {
                        if (global::RPGMod.Config.Questing.killEarthEnabled)
                        {
                            Questing.Server.CheckAllowedType(Questing.QuestType.killEarthDLC1);
                        }

                        Questing.Events.EarthKilledDLC1.Invoke(1, attackerNetworkUser);
                    }
                    if (global::RPGMod.Config.Questing.killCommonEnabled && enemyBody.HasBuff(DLC1Content.Buffs.EliteVoid))
                    {
                        if (global::RPGMod.Config.Questing.killVoidEnabled)
                        {
                            Questing.Server.CheckAllowedType(Questing.QuestType.killVoidDLC1);
                        }

                        Questing.Events.VoidKilledDLC1.Invoke(1, attackerNetworkUser);
                    }

                    // By rarity
                    if (enemyBody.isChampion)
                    {
                        if (global::RPGMod.Config.Questing.killChampionEnabled)
                        {
                            Questing.Server.CheckAllowedType(Questing.QuestType.killChampion);
                        }

                        Questing.Events.ChampionKilled.Invoke(1, attackerNetworkUser);
                    }
                    else if (enemyBody.isElite)
                    {
                        if (global::RPGMod.Config.Questing.killEliteEnabled)
                        {
                            Questing.Server.CheckAllowedType(Questing.QuestType.killElite);
                        }

                        Questing.Events.EliteKilled.Invoke(1, attackerNetworkUser);
                    }
                    else
                    {
                        if (global::RPGMod.Config.Questing.killCommonEnabled)
                        {
                            Questing.Server.CheckAllowedType(Questing.QuestType.killCommon);
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