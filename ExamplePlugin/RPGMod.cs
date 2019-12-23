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
        private float QuestCooldown { get; set; }
        private CharacterBody CachedCharacterBody { get; set; }
        private List<Questing.UI> UIInstances { get; set; }

        // Methods
        public RPGMod()
        { // Default Constructor
            GameStarted = false;
            IgnoreDeath = false;
            UIInstances = new List<Questing.UI>();
            QuestCooldown = 0;
        }

        private void ManageQuests() // Handles quest generation.
        {
            if (!NetworkServer.active)
            {
                return;
            }

            if (QuestReady())
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
        public void GetNewQuest(int serverMessageIndex = -1)
        {
            if (!NetworkServer.active || MainDefs.QuestClientMessages.Count > Questing.Config.questAmountMax)
            {
                Debug.Log("Returning from GetNewQuest call!");
                return;
            }

            Questing.ClientMessage message = Questing.Quest.GetQuest(serverMessageIndex);
            QuestCooldown = Time.time;
            Questing.Quest.SendQuest(message);
        }

        // Registers the Client handlers for recieving of data.
        public void StartClientHanders()
        {
            NetworkClient client = NetworkManager.singleton.client;
            client.RegisterHandler(Questing.Config.questPort, OnQuestRecieved);

            Debug.Log("RPGMod: Client handlers added");
        }

        public void OnQuestRecieved(NetworkMessage networkMessage) // Method run when quest is received
        {
            Questing.ClientMessage message = networkMessage.ReadMessage<Questing.ClientMessage>();

            int messageIndex = -1;
            int i = 0;
            while (i < MainDefs.QuestClientMessages.Count)
            {
                // Check unique id for match
                if (MainDefs.QuestClientMessages[i].id == message.id)
                {
                    messageIndex = i;
                    break;
                };
                i++;
            }

            if (messageIndex == -1)
            {
                messageIndex = MainDefs.QuestClientMessages.Count;
            }

            Debug.Log("Message recieved at " + messageIndex);
            Debug.Log(UIInstances.Count);
            Debug.Log(MainDefs.QuestClientMessages.Count);
            Debug.Log(MainDefs.QuestServerMessages.Count);

            if (message.advancingStage) {
                DestroyUIInstances();
            }

            // Removing client quests
            if (!message.active && messageIndex < MainDefs.QuestClientMessages.Count)
            {
                Debug.Log(messageIndex);
                MainDefs.QuestClientMessages.RemoveAt(messageIndex);
                if (!message.advancingStage)
                {
                    Destroy(UIInstances[messageIndex]);
                    UIInstances.RemoveAt(messageIndex);

                    for (int j = messageIndex; j < UIInstances.Count; j++)
                    {
                        UIInstances[j].Index = j;
                    }
                }

                if (NetworkServer.active)
                {
                    MainDefs.usedIDs.Remove(message.id);
                    MainDefs.usedTypes.Remove((Questing.Type)int.Parse(message.description.Split(',')[0]));

                    if (message.advancingStage && !Questing.Config.restartQuestsOnStageChange)
                    {
                        Debug.Log("SOFT - Removed all data at " + messageIndex);
                    }
                    else
                    {
                        MainDefs.QuestServerMessages.RemoveAt(messageIndex);
                        Debug.Log("HARD - Removed all data at " + messageIndex);
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
                MainDefs.QuestClientMessages.Last().active = true;
                DisplayQuesting();
                Debug.Log("Adding new message");
            }

            // Update quest display
            if (message.active) {
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
                UIInstances[index].ObjectiveIcon = MainDefs.assetBundle.LoadAsset<Texture>(MainDefs.questDefinitions.iconPaths[int.Parse(MainDefs.QuestClientMessages[index].description.Split(',')[0])]);
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
            Debug.Log("Deleting all UI INSTANCES");
            for (int i = 0; i < UIInstances.Count; i++)
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
            MainDefs.assetBundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(Paths.PluginPath, "RPGMod//assetBundle"));

            if (MainDefs.assetBundle == null)
            {
                Debug.LogError("RPGMOD: Failed to load assetbundle");
                return;
            }

            On.RoR2.Run.Start += (orig, self) =>
            {
                GameStarted = true;
                StartClientHanders();
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

                    CachedCharacterBody = null;

                    DestroyUIInstances();

                    orig(self);
                };

                On.RoR2.Run.BeginStage += (orig, self) =>
                {
                    foreach (var message in MainDefs.QuestClientMessages)
                    {
                        message.active = false;
                        message.advancingStage = true;
                        Questing.Quest.SendQuest(message);
                    }

                    Debug.Log("Messages removed!");

                    for (int i = 0; i < MainDefs.QuestServerMessages.Count; i++)
                    {
                        GetNewQuest(i);
                    }

                    Debug.Log("Messages added!");

                    QuestCooldown = (Time.time - Questing.Config.questCooldownTime + 10);

                    orig(self);
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
                IgnoreDeath = Questing.Listener.HookOnCharacterDeath(IgnoreDeath, damageReport);
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
                Questing.Listener.UpdateQuest((int)amount, Questing.Type.CollectGold, "Gold");
                orig(self, amount);
            };

            On.RoR2.ChestBehavior.ItemDrop += (orig, self) =>
            {
                Questing.Listener.UpdateQuest(1, Questing.Type.OpenChests, "Chests");
                orig(self);
            };

            On.RoR2.HealthComponent.Heal += (orig, self, amount, procChainMask, nonRegen) =>
            {
                if (self.body.isPlayerControlled)
                {
                    Questing.Listener.UpdateQuest((int)amount, Questing.Type.Heal, "Damage");
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
                        message.active = false;
                        Questing.Quest.SendQuest(message);
                    }
                    if (Input.GetKeyDown(KeyCode.F5)) {
                        Debug.Log(Run.instance.time);
                        foreach (var x in ClassicStageInfo.instance.monsterSelection.choices)
                        {
                            if (x.value.spawnCard.name != null)
                            {
                                Debug.Log(x.value.spawnCard.name);
                                Debug.Log(x.value.spawnCard.directorCreditCost);
                            }
                        }
                    }
                }
            }

            // Reload config key
            if (Input.GetKeyDown(KeyCode.F6))
            {
                Questing.Config.Load(Config, true);
                ResetUI();
            }
        }
    }
} // namespace RPGMod