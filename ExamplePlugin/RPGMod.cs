using BepInEx;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace RPGMod
{
    [BepInPlugin("com.ghasttear1.rpgmod", "RPGMod", "2.0.0")]
    internal sealed class RPGMod : BaseUnityPlugin
    {
        // Attributes
        private bool GameStarted { get; set; }
        private bool IgnoreDeath { get; set; }
        private bool ClientRegistered { get; set; }
        private float QuestCooldown { get; set; }
        private CharacterBody CachedCharacterBody { get; set; }
        private List<Questing.UI> UIInstances { get; set; }

        // Methods
        public RPGMod()
        { // Default Constructor
            GameStarted = false;
            IgnoreDeath = false;
            UIInstances = new List<Questing.UI>();
        }

        private void ManageQuests() // Handles quest generation.
        {
            if (!NetworkServer.active)
            {
                return;
            }

            // Gets any defined quests that are needed
            if (MainDefs.AwaitingDefinedQuests)
            {
                if (MainDefs.debugMode)
                {
                    Debug.Log(MainDefs.QuestClientMessages.Count);
                    Debug.Log(MainDefs.QuestServerMessages.Count);
                }

                for (int i = MainDefs.QuestClientMessages.Count; i < MainDefs.QuestServerMessages.Count; i++)
                {
                    GetNewQuest(i);
                }
                if (MainDefs.QuestClientMessages.Count == MainDefs.QuestServerMessages.Count)
                {
                    MainDefs.AwaitingDefinedQuests = false;
                    MainDefs.deleteServerData = true;
                }
            }
            else if (QuestReady())
            {
                // Generates a new generic quest
                GetNewQuest();
            };
        }

        private bool QuestReady()
        {
            return (MainDefs.QuestClientMessages.Count < Questing.Config.questAmountMax) && ((Time.time - QuestCooldown) / Questing.Config.questCooldownTime > 1);
        }

        // Generates and sends a new quest.
        public void GetNewQuest(int specificSeverDataIndex = -1)
        {
            if (!NetworkServer.active || MainDefs.QuestClientMessages.Count >= Questing.Config.questAmountMax)
            {
                return;
            }

            Questing.ClientMessage message = Questing.Quest.GetQuest(specificSeverDataIndex);

            if (message.description != "bad")
            {
                QuestCooldown = Time.time;
                Questing.Quest.SendQuest(message);
            }
        }

        // Registers the Client handlers for recieving of data.
        public void StartClientHanders()
        {
            NetworkClient client = NetworkManager.singleton.client;
            client.RegisterHandler(Questing.Config.questPort, OnQuestRecieved);
            ClientRegistered = true;

            Debug.Log("RPGMod: Client handlers added");
        }

        public void OnQuestRecieved(NetworkMessage networkMessage) // Method run when quest is received
        {
            Questing.ClientMessage message = networkMessage.ReadMessage<Questing.ClientMessage>();

            int messageIndex = -1;
            for (int i = 0; i < MainDefs.QuestClientMessages.Count; i++)
            {
                // Check unique id for match
                if (MainDefs.QuestClientMessages[i].id == message.id)
                {
                    messageIndex = i;
                };
            }

            if (messageIndex == -1)
            {
                messageIndex = MainDefs.QuestClientMessages.Count;
            }

            // Removing client quests
            if (message.initialised == false && messageIndex < MainDefs.QuestClientMessages.Count)
            {
                MainDefs.QuestClientMessages.RemoveAt(messageIndex);
                Destroy(UIInstances[messageIndex]);
                UIInstances.RemoveAt(messageIndex);

                for (int i = messageIndex; i < UIInstances.Count; i++)
                {
                    UIInstances[i].Index = i;
                }
                if (NetworkServer.active)
                {
                    MainDefs.usedIDs.Remove(message.id);
                    MainDefs.usedTypes.Remove(int.Parse(message.description.Split(',')[0]));
                    if (MainDefs.deleteServerData)
                    {
                        MainDefs.QuestServerMessages.RemoveAt(messageIndex);
                        Debug.Log("HARD - Removed all data at " + messageIndex);
                    }
                    else
                    {
                        Debug.Log("SOFT - Removed all data at " + messageIndex);
                    }
                }
            }
            else if (messageIndex < MainDefs.QuestClientMessages.Count)
            {
                MainDefs.QuestClientMessages[messageIndex] = message;
            }
            else
            {
                MainDefs.QuestClientMessages.Add(message);
                Debug.Log("Adding new message");
            }

            // Update quest display
            DisplayQuesting();

            if (message.initialised)
            {
                // Update contents of the quest
                UIInstances[messageIndex].QuestDataDescription = message.description;
            }
        }

        public void DisplayQuesting()
        {
            LocalUser localUser = LocalUserManager.GetFirstLocalUser();

            if (CachedCharacterBody == null && localUser != null)
            {
                CachedCharacterBody = localUser.cachedBody;
            }

            if (CachedCharacterBody != null)
            {
                for (int i = 0; i < MainDefs.QuestClientMessages.Count; i++)
                {
                    if (i >= UIInstances.Count)
                    {
                        if (MainDefs.debugMode)
                        {
                            Debug.Log(CachedCharacterBody);
                            Debug.Log(Questing.Config.widthUI);
                            Debug.Log(Questing.Config.heightUI);
                            Debug.Log(Screen.width * Questing.Config.xPositionUI);
                            Debug.Log(Screen.height * Questing.Config.yPositionUI);
                            Debug.Log(MainDefs.QuestClientMessages[i].description);
                        }

                        AddUIInstance(i);
                    }
                }
            }
        }

        public void ResetUI()
        {
            DestroyUIInstances();
            for (int i = 0; i < MainDefs.QuestClientMessages.Count; i++)
            {
                AddUIInstance(i);
            }
        }

        public void AddUIInstance(int index)
        {
            UIInstances.Add(CachedCharacterBody.gameObject.AddComponent<Questing.UI>());
            UIInstances[index].index = index;
            UIInstances[index].QuestDataDescription = MainDefs.QuestClientMessages[index].description;

            if (MainDefs.QuestClientMessages[index].iconPath == "custom")
            {
                UIInstances[index].ObjectiveIcon = MainDefs.assetBundle.LoadAsset<Texture>(MainDefs.questIconPaths[int.Parse(MainDefs.QuestClientMessages[index].description.Split(',')[0])]);
            }
            else
            {
                switch (int.Parse(MainDefs.QuestClientMessages[index].description.Split(',')[0]))
                {
                    case 0: UIInstances[index].ObjectiveIcon = BodyCatalog.FindBodyPrefab(MainDefs.QuestClientMessages[index].iconPath).GetComponent<CharacterBody>().portraitIcon; break;
                }
            }
        }

        public void DestroyUIInstances()
        {
            for (int i = 0; i < MainDefs.QuestClientMessages.Count; i++)
            {
                if (UIInstances[i] != null)
                {
                    Destroy(UIInstances[i]);
                }
            }
            UIInstances.Clear();
        }

        public void Awake()
        {
            Questing.Config.Load(Config, false);
            QuestCooldown = -Questing.Config.questCooldownTime;
            MainDefs.assetBundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(Paths.PluginPath, "RPGMod//assetBundle"));

            if (MainDefs.assetBundle == null)
            {
                Debug.Log("RPGMOD: Failed to load assetbundle");
                return;
            }

            On.RoR2.Run.Start += (orig, self) =>
            {
                GameStarted = true;
                orig(self);
            };

            if (Questing.Config.questingEnabled)
            {
                On.RoR2.Run.OnClientGameOver += (orig, self, runReport) =>
                {
                    DestroyUIInstances();
                    orig(self, runReport);
                };

                On.RoR2.Run.OnDisable += (orig, self) =>
                {
                    GameStarted = false;
                    MainDefs.QuestClientMessages.Clear();
                    MainDefs.QuestServerMessages.Clear();

                    ClientRegistered = false;
                    CachedCharacterBody = null;

                    DestroyUIInstances();

                    orig(self);
                };

                On.RoR2.Run.OnServerSceneChanged += (orig, self, sceneName) =>
                {
                    if (Questing.Config.restartQuestsOnStageChange)
                    {
                        MainDefs.QuestClientMessages.Clear();
                        MainDefs.QuestServerMessages.Clear();
                    }
                    else
                    {
                        if (MainDefs.QuestServerMessages.Count > 0)
                        {
                            MainDefs.deleteServerData = false;
                            for (int i = 0; i < MainDefs.QuestServerMessages.Count; i++)
                            {
                                MainDefs.QuestClientMessages[i].initialised = false;
                                Questing.Quest.SendQuest(MainDefs.QuestClientMessages[i]);
                            }
                        }
                        MainDefs.AwaitingDefinedQuests = true;
                    }

                    orig(self, sceneName);
                };
            }

            On.RoR2.SceneDirector.PopulateScene += (orig, self) =>
            {
                int credit = self.GetFieldValue<int>("interactableCredit");
                self.SetFieldValue("interactableCredit", (int)(credit * Questing.Config.worldSpawnPercentage));
                orig(self);
            };

            On.RoR2.HealthComponent.Suicide += (orig, self, killerOverride, inflictorOverride, damageType) =>
            {
                if (self.gameObject.GetComponent<CharacterBody>().isBoss || self.gameObject.GetComponent<CharacterBody>().master.name == "EngiTurretMaster(Clone)") // TODO: Fix Gauss Turret
                {
                    IgnoreDeath = true;
                }
                orig(self, killerOverride, inflictorOverride, damageType);
            };

            // Death drop handler
            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, self, damageReport) =>
            {
                if (!IgnoreDeath)
                {
                    float chance;
                    CharacterBody enemyBody = damageReport.victimBody;
                    GameObject attackerMaster = damageReport.damageInfo.attacker.GetComponent<CharacterBody>().masterObject;
                    CharacterMaster attackerController = attackerMaster.GetComponent<CharacterMaster>();

                    Questing.Listener.UpdateQuest(1, 0, enemyBody.GetUserName());

                    if (Questing.Config.enemyItemDropsEnabled)
                    {
                        bool isElite = enemyBody.isElite || enemyBody.isChampion;
                        bool isBoss = enemyBody.isBoss;

                        if (isBoss)
                        {
                            chance = Questing.Config.bossDropChance;
                        }
                        else
                        {
                            if (isElite)
                            {
                                chance = Questing.Config.eliteDropChance;
                            }
                            else
                            {
                                chance = Questing.Config.normalDropChance;
                            }
                        }

                        if (enemyBody.isElite)
                        {
                            Questing.Listener.UpdateQuest(1, 4, "Elites");
                        }

                        chance *= ((1 - Questing.Config.playerChanceScaling) + (Questing.Config.playerChanceScaling * Run.instance.participatingPlayerCount));
                        if (Questing.Config.earlyChanceScaling - Run.instance.difficultyCoefficient > 0)
                        {
                            chance *= (Questing.Config.earlyChanceScaling - (Run.instance.difficultyCoefficient - 1));
                        }

                        // rng check
                        bool didDrop = Util.CheckRoll(chance, attackerController ? attackerController.luck : 0f, null);

                        // Gets Item and drops in world
                        if (didDrop)
                        {
                            if (!isBoss)
                            {
                                // Create a weighted selection for rng
                                WeightedSelection<List<PickupIndex>> weightedSelection = new WeightedSelection<List<PickupIndex>>(8);
                                // Check if enemy is boss, elite or normal
                                if (isElite)
                                {
                                    weightedSelection.AddChoice(Run.instance.availableTier1DropList, Questing.Config.eliteChanceCommon);
                                    weightedSelection.AddChoice(Run.instance.availableTier2DropList, Questing.Config.eliteChanceUncommon);
                                    weightedSelection.AddChoice(Run.instance.availableTier3DropList, Questing.Config.eliteChanceLegendary);
                                    weightedSelection.AddChoice(Run.instance.availableLunarDropList, Questing.Config.eliteChanceLunar);
                                }
                                else
                                {
                                    weightedSelection.AddChoice(Run.instance.availableTier1DropList, Questing.Config.normalChanceCommon);
                                    weightedSelection.AddChoice(Run.instance.availableTier2DropList, Questing.Config.normalChanceUncommon);
                                    weightedSelection.AddChoice(Run.instance.availableTier3DropList, Questing.Config.normalChanceLegendary);
                                    weightedSelection.AddChoice(Run.instance.availableEquipmentDropList, Questing.Config.normalChanceEquip);
                                }
                                // Get a Tier
                                List<PickupIndex> list = weightedSelection.Evaluate(Run.instance.spawnRng.nextNormalizedFloat);
                                // Pick random from tier
                                PickupIndex item = list[Run.instance.spawnRng.RangeInt(0, list.Count)];
                                // Spawn item
                                PickupDropletController.CreatePickupDroplet(item, enemyBody.transform.position, Vector3.up * 20f);
                            }
                        }
                    }
                }
                else
                {
                    IgnoreDeath = false;
                }
                orig(self, damageReport);
            };

            if (!Questing.Config.defaultWorldSpawnsEnabled)
            {
                // Handles banned scene spawns
                On.RoR2.ClassicStageInfo.Awake += (orig, self) =>
                {
                    // Gets card catergories using reflection
                    DirectorCardCategorySelection cardSelection = self.GetFieldValue<DirectorCardCategorySelection>("interactableCategories");
                    for (int i = 0; i < cardSelection.categories.Length; i++)
                    {
                        // Makes copy of category to make changes
                        var cardsCopy = cardSelection.categories[i];
                        cardsCopy.cards = cardSelection.categories[i].cards.Where(val => !Questing.Config.bannedDirectorSpawns.Any(val.spawnCard.prefab.name.Contains)).ToArray();

                        // Sets category to new edited version
                        cardSelection.categories[i] = cardsCopy;
                    }
                    // Sets new card categories
                    self.SetFieldValue("interactableCategories", cardSelection);

                    // Runs original function
                    orig(self);
                };
            }

            On.RoR2.CharacterMaster.GiveMoney += (orig, self, amount) =>
            {
                Questing.Listener.UpdateQuest((int)amount, 1, "Gold");
                orig(self, amount);
            };

            On.RoR2.ChestBehavior.ItemDrop += (orig, self) =>
            {
                Questing.Listener.UpdateQuest(1, 2, "Chests");
                orig(self);
            };

            On.RoR2.HealthComponent.Heal += (orig, self, amount, procChainMask, nonRegen) =>
            {
                if (self.body.isPlayerControlled)
                {
                    Questing.Listener.UpdateQuest((int)amount, 3, "Damage");
                }
                return orig(self, amount, procChainMask, nonRegen);
            };

            Debug.Log("<color=#13d3dd>RPGMod: </color> Loaded Successfully!");
        }

        public void Update()
        {
            if (GameStarted)
            {
                if (Questing.Config.questingEnabled)
                {
                    ManageQuests();
                }

                // Registers Client Handlers
                if (!ClientRegistered)
                {
                    StartClientHanders();
                }

                // Reload config key
                if (Input.GetKeyDown(KeyCode.F6))
                {
                    Questing.Config.Load(Config, true);
                    ResetUI();
                }

                // Debug Keys
                if (MainDefs.debugMode)
                {
                    if (Input.GetKeyDown(KeyCode.F3))
                    {
                        GetNewQuest();
                    }

                    if (Input.GetKeyDown(KeyCode.F4))
                    {
                        Questing.ClientMessage message = MainDefs.QuestClientMessages.Last();
                        message.initialised = false;
                        Questing.Quest.SendQuest(message);
                    }
                }
            }
        }
    }
} // namespace RPGMod