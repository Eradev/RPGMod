using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using RoR2;
using RPGMod.Questing;
using UnityEngine;
using UnityEngine.Networking;
using CharacterMaster = On.RoR2.CharacterMaster;
using ChestBehavior = On.RoR2.ChestBehavior;
using ClassicStageInfo = On.RoR2.ClassicStageInfo;
using Console = RoR2.Console;
using GlobalEventManager = On.RoR2.GlobalEventManager;
using HealthComponent = On.RoR2.HealthComponent;
using SceneDirector = On.RoR2.SceneDirector;

namespace RPGMod
{
    [BepInPlugin("com.ghasttear1.rpgmod", "RPGMod", "3.0.0")]
    internal sealed class RpgMod : BaseUnityPlugin
    {
        // Attributes
        private bool GameStarted { get; set; }
        private bool IgnoreDeath { get; set; }
        private float QuestCooldown { get; set; }
        private CharacterBody CachedCharacterBody { get; set; }

        public RpgMod()
        {
            GameStarted = false;
            IgnoreDeath = false;
        }

        private void ManageQuests() // Manages questing system
        {
            // Attempts to create UI elements for stored client quests
            if (ClientMessage.Instances.Count != UI.Instances.Count) {
                DisplayQuesting();
            }

            if (!NetworkServer.active)
            {
                return;
            }

            // Ensure that no predefined server messages are awaiting a client message
            var awaitingClientQuests = false;
            for (var i = 0; i < ServerMessage.Instances.Count; i++)
            {
                if (!ServerMessage.Instances[i].AwaitingClientMessage || 
                    Run.instance.isRunStopwatchPaused ||
                    ArenaMissionController.instance != null)
                {
                    continue;
                }

                GetNewQuest(i);
                awaitingClientQuests = true;
            }

            // Gets a new quest when ready
            if (QuestReady() && !awaitingClientQuests)
            {
                GetNewQuest();
            }
        }

        private bool QuestReady() // Determines if a new quest should be requested
        {
            return ClientMessage.Instances.Count < Questing.Config.QuestAmountMax && 
                   !Run.instance.isRunStopwatchPaused && ArenaMissionController.instance == null && 
                   (Run.instance.GetRunStopwatch() - QuestCooldown) / Questing.Config.QuestCooldownTime > 1;
        }

        public void GetNewQuest(int serverMessageIndex = -1) // Generates a new quest and sends it to all clients
        {
            if (!NetworkServer.active || ClientMessage.Instances.Count > Questing.Config.QuestAmountMax)
            {
                return;
            }

            // Attempts to get a new quest
            ClientMessage message;
            try
            {
                if (Core.DebugMode)
                {
                    Debug.LogError($"RPGMOD: Trying to get quest. ServerMessageIndex: {serverMessageIndex}");
                }
                message = Quest.GetQuest(serverMessageIndex);
            }
            catch (Exception ex) {
                if (Core.DebugMode) {
                    Debug.LogError("RPGMOD: Error when obtaining quest");
                    Debug.LogError(ex.ToString());
                }
                message = new ClientMessage();
            }

            // If the quest has the default description it is deemed bad to send
            if (message.Description == "bad")
            {
                return;
            }

            QuestCooldown = Run.instance.GetRunStopwatch();
            message.SendToAll();
        }

        public void StartClientHandlers() // Registers the client handlers for receiving of data
        {
            var client = NetworkManager.singleton.client;
            client.RegisterHandler(Questing.Config.QuestPort, OnQuestReceived);

            Debug.Log("RPGMod: Client handlers added");
        }

        public void OnQuestReceived(NetworkMessage networkMessage) // Runs when a quest is received
        {
            var message = networkMessage.ReadMessage<ClientMessage>();

            // Checks through unique id the specific index that the message relates to
            var messageIndex = -1;
            var i = 0;
            while (i < ClientMessage.Instances.Count)
            {
                // Check unique id for match
                if (ClientMessage.Instances[i].Id == message.Id)
                {
                    messageIndex = i;
                    break;
                };
                i++;
            }

            if (messageIndex == -1)
            {
                messageIndex = ClientMessage.Instances.Count;
            }

            if (Core.DebugMode)
            {
                Debug.Log($"Message received at {messageIndex}");
                Debug.Log(UI.Instances.Count);
                Debug.Log(ClientMessage.Instances.Count);
                Debug.Log(ServerMessage.Instances.Count);
            }

            if (message.AdvancingStage) {
                DestroyUIInstances();
            }

            // Updates the description displayed for the quest
            if (message.Active && UI.Instances[messageIndex] != null)
            {
                UI.Instances[messageIndex].QuestDataDescription = message.Description;
            }

            // Removes the message
            if (!message.Active && messageIndex < ClientMessage.Instances.Count)
            {
                ClientMessage.Instances.RemoveAt(messageIndex);
                if (!message.AdvancingStage)
                {
                    Destroy(UI.Instances[messageIndex]);
                    UI.Instances.RemoveAt(messageIndex);

                    for (var j = messageIndex; j < UI.Instances.Count; j++)
                    {
                        UI.Instances[j].Index = j;
                    }
                }

                if (NetworkServer.active)
                {
                    Core.UsedIDs.Remove(message.Id);
                    Core.UsedTypes[(QuestType)int.Parse(message.Description.Split(',')[0])] -= 1;

                    if (message.AdvancingStage && !Questing.Config.RestartQuestsOnStageChange)
                    {
                        if (Core.DebugMode)
                        {
                            Debug.Log("SOFT removal at " + messageIndex);
                        }
                    }
                    else
                    {
                        ServerMessage.Instances.RemoveAt(messageIndex);
                        if (Core.DebugMode)
                        {
                            Debug.Log("HARD removal at " + messageIndex);
                        }
                    }

                }
            }
            // Updates the existing client message
            else if (messageIndex < ClientMessage.Instances.Count)
            {
                ClientMessage.Instances[messageIndex] = message;
            }
            // Adds a client message
            else
            {
                message.RegisterInstance();
                ClientMessage.Instances.Last().Active = true;
                if (Core.DebugMode)
                {
                    Debug.Log("Adding new message");
                }
            }

            if (Core.DebugMode)
            {
                Debug.Log(message.Active);
            }
        }

        public void DisplayQuesting() // Handles the creation of UI elements
        {
            var localUser = LocalUserManager.GetFirstLocalUser();

            // Updates the local character body
            if (CachedCharacterBody == null && localUser != null)
            {
                CachedCharacterBody = localUser.cachedBody;
            }

            if (CachedCharacterBody == null)
            {
                return;
            }

            // Adds a new UI instance
            for (var i = 0; i < ClientMessage.Instances.Count; i++)
            {
                if (i < UI.Instances.Count)
                {
                    continue;
                }

                if (Core.DebugMode)
                {
                    Debug.Log(CachedCharacterBody);
                    Debug.Log(Screen.width * Questing.Config.XPositionUI);
                    Debug.Log(Screen.height * Questing.Config.YPositionUI);
                    Debug.Log(ClientMessage.Instances[i].Description);
                }

                AddUIInstance(i);
            }
        }

        public void ResetUI() // Resets the UI elements
        {
            DestroyUIInstances();
            DisplayQuesting();
        }

        public void AddUIInstance(int index) // Adds a new UI element at a specified index
        {
            UI.Instances.Add(CachedCharacterBody.gameObject.AddComponent<UI>());
            UI.Instances[index].index = index;
            UI.Instances[index].questDataDescription = ClientMessage.Instances[index].Description;

            if (ClientMessage.Instances[index].IconPath == "custom")
            {
                UI.Instances[index].ObjectiveIcon = Core.AssetBundle.LoadAsset<Texture>(Core.QuestDefinitions.IconPaths[int.Parse(ClientMessage.Instances[index].Description.Split(',')[0])]);
            }
            else
            {
                if ((QuestType) int.Parse(ClientMessage.Instances[index].Description.Split(',')[0]) == QuestType.KillEnemies)
                {
                    UI.Instances[index].ObjectiveIcon = BodyCatalog.FindBodyPrefab(ClientMessage.Instances[index].IconPath)?.GetComponent<CharacterBody>().portraitIcon;
                }
            }
        }

        public void DestroyUIInstances() // Destroys all UI elements
        {
            if (Core.DebugMode)
            {
                Debug.Log("Deleting all UI INSTANCES");
            }

            foreach (var t in UI.Instances.Where(t => t != null))
            {
                Destroy(t);
            }

            UI.Instances.Clear();
        }

        public void Awake()
        {
            // Loads the config and asset bundle
            Questing.Config.Load(Config, false);

            var execAssembly = Assembly.GetExecutingAssembly();
            var stream = execAssembly.GetManifestResourceStream("RPGMod.assetbundle");
            Core.AssetBundle = AssetBundle.LoadFromStream(stream);

            if (Core.AssetBundle == null)
            {
                Debug.LogError("RPGMOD: Failed to load asset bundle");
                return;
            }

            if (Questing.Config.QuestingEnabled)
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
                    ClientMessage.Instances.Clear();
                    ServerMessage.Instances.Clear();

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

                    foreach (var clientMessage in ClientMessage.Instances)
                    {
                        clientMessage.Active = false;
                        clientMessage.AdvancingStage = true;
                        clientMessage.SendToAll();
                    }

                    if (Core.DebugMode)
                    {
                        Debug.Log("Messages removed!");
                    }

                    foreach (var serverMessage in ServerMessage.Instances)
                    {
                        serverMessage.AwaitingClientMessage = true;
                    }

                    if (Core.DebugMode)
                    {
                        Debug.Log("Messages expected!");
                    }

                    QuestCooldown = Run.instance.GetRunStopwatch() - Questing.Config.QuestCooldownTime + Questing.Config.QuestCooldownTimeOnSceneChange;

                    orig(self);
                };

                // Ensures that objects that have died with no body are ignored by the OnCharacterDeath hook
                HealthComponent.Suicide += (orig, self, killerOverride, inflictorOverride, damageType) =>
                {
                    if (gameObject.GetComponent<CharacterBody>() == null || self.gameObject.GetComponent<CharacterBody>().isBoss)
                    {
                        IgnoreDeath = true;
                    }
                    orig(self, killerOverride, inflictorOverride, damageType);
                };

                // Handles the enemy drops
                GlobalEventManager.OnCharacterDeath += (orig, self, damageReport) =>
                {
                    IgnoreDeath = QuestUpdateHandler.HookOnCharacterDeath(IgnoreDeath, damageReport);
                    orig(self, damageReport);
                };

                // QuestUpdateHandler hooks

                CharacterMaster.GiveMoney += (orig, self, amount) =>
                {
                    QuestUpdateHandler.UpdateQuest((int)amount, QuestType.CollectGold, "Gold");
                    orig(self, amount);
                };

                ChestBehavior.ItemDrop += (orig, self) =>
                {
                    QuestUpdateHandler.UpdateQuest(1, QuestType.OpenChests, "Chests");
                    orig(self);
                };

                HealthComponent.Heal += (orig, self, amount, procChainMask, nonRegen) =>
                {
                    if (self.body.isPlayerControlled)
                    {
                        QuestUpdateHandler.UpdateQuest((int)amount, QuestType.Heal, "Damage");
                    }
                    return orig(self, amount, procChainMask, nonRegen);
                };
            }

            // Injects the spawn percentage into the games director when populating the scene
            SceneDirector.PopulateScene += (orig, self) =>
            {
                var credit = self.interactableCredit;
                self.interactableCredit = (int)(credit * Questing.Config.WorldSpawnPercentage);
                orig(self);
            };

            if (!Questing.Config.DefaultWorldSpawnsEnabled)
            {
                // Handles banned scene spawns
                ClassicStageInfo.Awake += (orig, self) =>
                {
                    // Gets card categories
                    var cardSelection = self.interactableCategories;
                    for (var i = 0; i < cardSelection.categories.Length; i++)
                    {
                        // Makes copy of category to make changes
                        var cardsCopy = cardSelection.categories[i];
                        cardsCopy.cards = cardSelection.categories[i].cards.Where(val => !Questing.Config.BannedDirectorSpawns.Any(val.spawnCard.prefab.name.Contains)).ToArray();

                        // Sets category to new edited version
                        cardSelection.categories[i] = cardsCopy;
                    }
                    // Sets new card categories
                    self.interactableCategories = cardSelection;

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
                if (Questing.Config.QuestingEnabled)
                {
                    ManageQuests();
                }

                // Debug Keys
                if (Core.DebugMode)
                {
                    if (Input.GetKeyDown(KeyCode.F3))
                    {
                        GetNewQuest();
                    }

                    if (Input.GetKeyDown(KeyCode.F4))
                    {
                        var message = ClientMessage.Instances.Last();
                        message.Active = false;
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
}