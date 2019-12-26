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

        // Constructors
        public RPGMod()
        {
            GameStarted = false;
            IgnoreDeath = false;
        }

        // Methods
        private void ManageQuests() // Manages questing system
        {
            // Attempts to create UI elements for stored client quests
            if (Questing.ClientMessage.Instances.Count != Questing.UI.Instances.Count) {
                DisplayQuesting();
            }

            if (!NetworkServer.active)
            {
                return;
            }

            // Ensure that no predefined server messages are awaiting a client message
            bool awaitingClientQuests = false;
            for (int i = 0; i < Questing.ServerMessage.Instances.Count; i++) {
                if (Questing.ServerMessage.Instances[i].awaitingClientMessage && !Run.instance.isRunStopwatchPaused && ArenaMissionController.instance == null) {
                    GetNewQuest(i);
                    awaitingClientQuests = true;
                }
            }

            // Gets a new quest when ready
            if (QuestReady() && !awaitingClientQuests)
            {
                GetNewQuest();
            };
        }

        private bool QuestReady() // Determines if a new quest should be requested
        {
            return (Questing.ClientMessage.Instances.Count < Questing.Config.questAmountMax) && !Run.instance.isRunStopwatchPaused && ArenaMissionController.instance == null && ((Run.instance.GetRunStopwatch() - QuestCooldown) / Questing.Config.questCooldownTime > 1);
        }

        public void GetNewQuest(int serverMessageIndex = -1) // Generates a new quest and sends it to all clients
        {
            if (!NetworkServer.active || Questing.ClientMessage.Instances.Count > Questing.Config.questAmountMax)
            {
                return;
            }


            // Attemots to get a new quest
            Questing.ClientMessage message;
            try
            {
                message = Questing.Quest.GetQuest(serverMessageIndex);
            }
            catch {
                if (Core.debugMode) {
                    Debug.LogError("RPGMOD: Error when obtaining quest");
                }
                message = new Questing.ClientMessage();
            }

            // If the quest has the default description it is deemed bad to send
            if (message.description != "bad")
            {
                QuestCooldown = Run.instance.GetRunStopwatch();
                message.SendToAll();
            }
        }

        public void StartClientHandlers() // Registers the client handlers for recieving of data
        {
            NetworkClient client = NetworkManager.singleton.client;
            client.RegisterHandler(Questing.Config.questPort, OnQuestRecieved);

            Debug.Log("RPGMod: Client handlers added");
        }

        public void OnQuestRecieved(NetworkMessage networkMessage) // Runs when a quest is recieved
        {
            Questing.ClientMessage message = networkMessage.ReadMessage<Questing.ClientMessage>();

            // Checks through unique id the specific index that the message relates to
            int messageIndex = -1;
            int i = 0;
            while (i < Questing.ClientMessage.Instances.Count)
            {
                // Check unique id for match
                if (Questing.ClientMessage.Instances[i].id == message.id)
                {
                    messageIndex = i;
                    break;
                };
                i++;
            }

            if (messageIndex == -1)
            {
                messageIndex = Questing.ClientMessage.Instances.Count;
            }

            //

            if (Core.debugMode)
            {
                Debug.Log("Message recieved at " + messageIndex);
                Debug.Log(Questing.UI.Instances.Count);
                Debug.Log(Questing.ClientMessage.Instances.Count);
                Debug.Log(Questing.ServerMessage.Instances.Count);
            }

            if (message.advancingStage) {
                DestroyUIInstances();
            }

            // Updates the decscription displayed for the quest
            if (message.active && Questing.UI.Instances[messageIndex] != null)
            {
                Questing.UI.Instances[messageIndex].QuestDataDescription = message.description;
            }

            // Removes the message
            if (!message.active && messageIndex < Questing.ClientMessage.Instances.Count)
            {
                Questing.ClientMessage.Instances.RemoveAt(messageIndex);
                if (!message.advancingStage)
                {
                    Destroy(Questing.UI.Instances[messageIndex]);
                    Questing.UI.Instances.RemoveAt(messageIndex);

                    for (int j = messageIndex; j < Questing.UI.Instances.Count; j++)
                    {
                        Questing.UI.Instances[j].Index = j;
                    }
                }

                if (NetworkServer.active)
                {
                    Core.usedIDs.Remove(message.id);
                    Core.usedTypes[((Questing.Type)int.Parse(message.description.Split(',')[0]))] -= 1;

                    if (message.advancingStage && !Questing.Config.restartQuestsOnStageChange)
                    {
                        if (Core.debugMode)
                        {
                            Debug.Log("SOFT removal at " + messageIndex);
                        }
                    }
                    else
                    {
                        Questing.ServerMessage.Instances.RemoveAt(messageIndex);
                        if (Core.debugMode)
                        {
                            Debug.Log("HARD removal at " + messageIndex);
                        }
                    }

                }
            }
            // Updates the existing client message
            else if (messageIndex < Questing.ClientMessage.Instances.Count)
            {
                Questing.ClientMessage.Instances[messageIndex] = message;
            }
            // Adds a client message
            else
            {
                message.RegisterInstance();
                Questing.ClientMessage.Instances.Last().active = true;
                if (Core.debugMode)
                {
                    Debug.Log("Adding new message");
                }
            }

            if (Core.debugMode)
            {
                Debug.Log(message.active);
            }
        }

        public void DisplayQuesting() // Handles the creation of UI elements
        {
            LocalUser localUser = LocalUserManager.GetFirstLocalUser();

            // Updates the local character body
            if (CachedCharacterBody == null && localUser != null)
            {
                CachedCharacterBody = localUser.cachedBody;
            }

            // Adds a new UI instance
            if (CachedCharacterBody != null)
            {
                for (int i = 0; i < Questing.ClientMessage.Instances.Count; i++)
                {
                    if (i >= Questing.UI.Instances.Count)
                    {
                        if (Core.debugMode)
                        {
                            Debug.Log(CachedCharacterBody);
                            Debug.Log(Screen.width * Questing.Config.xPositionUI);
                            Debug.Log(Screen.height * Questing.Config.yPositionUI);
                            Debug.Log(Questing.ClientMessage.Instances[i].description);
                        }

                        AddUIInstance(i);
                    }
                }
            }
        }

        public void ResetUI() // Resets the UI elements
        {
            DestroyUIInstances();
            DisplayQuesting();
        }

        public void AddUIInstance(int index) // Adds a new UI element at a specified index
        {
            Questing.UI.Instances.Add(CachedCharacterBody.gameObject.AddComponent<Questing.UI>());
            Questing.UI.Instances[index].index = index;
            Questing.UI.Instances[index].QuestDataDescription = Questing.ClientMessage.Instances[index].description;

            if (Questing.ClientMessage.Instances[index].iconPath == "custom")
            {
                Questing.UI.Instances[index].ObjectiveIcon = Core.assetBundle.LoadAsset<Texture>(Core.questDefinitions.iconPaths[int.Parse(Questing.ClientMessage.Instances[index].description.Split(',')[0])]);
            }
            else
            {
                switch ((Questing.Type)int.Parse(Questing.ClientMessage.Instances[index].description.Split(',')[0]))
                {
                    case Questing.Type.KillEnemies: Questing.UI.Instances[index].ObjectiveIcon = BodyCatalog.FindBodyPrefab(Questing.ClientMessage.Instances[index].iconPath).GetComponent<CharacterBody>().portraitIcon; break;
                }
            }
        }

        public void DestroyUIInstances() // Destroys all UI elements
        {
            if (Core.debugMode)
            {
                Debug.Log("Deleting all UI INSTANCES");
            }
            for (int i = 0; i < Questing.UI.Instances.Count; i++)
            {
                if (Questing.UI.Instances[i] != null)
                {
                    Destroy(Questing.UI.Instances[i]);
                }
            }
            Questing.UI.Instances.Clear();
        }

        public void Awake()
        {
            // Loads the config and assetbundle
            Questing.Config.Load(Config, false);
            Core.assetBundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(Paths.PluginPath, "RPGMod//assetBundle"));

            if (Core.assetBundle == null)
            {
                Debug.LogError("RPGMOD: Failed to load assetbundle");
                return;
            }

            if (Questing.Config.questingEnabled)
            {

                // Runs when the game starts
                On.RoR2.Run.Start += (orig, self) =>
                {
                    GameStarted = true;
                    QuestCooldown = 0;
                    StartClientHandlers();
                    orig(self);
                };

                // Ensures UI is killed when the game ends
                On.RoR2.Run.OnClientGameOver += (orig, self, runReport) =>
                {
                    DestroyUIInstances();
                    orig(self, runReport);
                };

                // When the game is left all mod variables are reset
                On.RoR2.Run.OnDisable += (orig, self) =>
                {
                    GameStarted = false;
                    Questing.ClientMessage.Instances.Clear();
                    Questing.ServerMessage.Instances.Clear();

                    CachedCharacterBody = null;

                    DestroyUIInstances();
                    Core.Reset();

                    orig(self);
                };

                // When a new stage starts all the quests refresh in order to still be achievable
                On.RoR2.Run.BeginStage += (orig, self) =>
                {
                    if (!NetworkServer.active)
                    {
                        return;
                    }

                    foreach (var message in Questing.ClientMessage.Instances)
                    {
                        message.active = false;
                        message.advancingStage = true;
                        message.SendToAll();
                    }

                    if (Core.debugMode)
                    {
                        Debug.Log("Messages removed!");
                    }

                    for (int i = 0; i < Questing.ServerMessage.Instances.Count; i++)
                    {
                        Questing.ServerMessage.Instances[i].awaitingClientMessage = true;
                    }

                    if (Core.debugMode)
                    {
                        Debug.Log("Messages expected!");
                    }

                    QuestCooldown = (Run.instance.GetRunStopwatch() - Questing.Config.questCooldownTime + Questing.Config.questCooldownTimeOnSceneChange);

                    orig(self);
                };

                // Ensures that objects that have died with no body are ignored by the OnCharacterDeath hook
                On.RoR2.HealthComponent.Suicide += (orig, self, killerOverride, inflictorOverride, damageType) =>
                {
                    if (gameObject.GetComponent<CharacterBody>() == null || self.gameObject.GetComponent<CharacterBody>().isBoss)
                    {
                        IgnoreDeath = true;
                    }
                    orig(self, killerOverride, inflictorOverride, damageType);
                };

                // Handles the enemy drops
                On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, self, damageReport) =>
                {
                    IgnoreDeath = Questing.Listener.HookOnCharacterDeath(IgnoreDeath, damageReport);
                    orig(self, damageReport);
                };

                // Questing Listener hooks

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

                //

            }


            // Injects the spawn percentage into the games director when populating the scene
            On.RoR2.SceneDirector.PopulateScene += (orig, self) =>
            {
                int credit = self.GetFieldValue<int>("interactableCredit");
                self.SetFieldValue("interactableCredit", (int)(credit * Questing.Config.worldSpawnPercentage));
                orig(self);
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

            Debug.Log("RPGMod: Loaded Successfully!");
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
                if (Core.debugMode)
                {
                    if (Input.GetKeyDown(KeyCode.F3))
                    {
                        GetNewQuest();
                    }

                    if (Input.GetKeyDown(KeyCode.F4))
                    {
                        Questing.ClientMessage message = Questing.ClientMessage.Instances.Last();
                        message.active = false;
                        message.SendToAll();
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