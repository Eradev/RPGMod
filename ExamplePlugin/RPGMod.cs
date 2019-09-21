using BepInEx;
using RoR2;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RPGMod
{
    [BepInPlugin("com.ghasttear1.rpgmod", "RPGMod", "2.0.0")]

    public class RPGMod : BaseUnityPlugin
    {
        public QuestDefs QuestDefs = new QuestDefs();
        public bool clientRegistered = false;
        public System.Random random = new System.Random();

        public bool gameStarted = false;
        public bool isDebug = true;
        public bool isSuicide = false;
        public int messageIndex;
        public CharacterBody CachedCharacterBody;
        public CharacterMaster cMaster;
        public GameObject targetBody;
        public List<Notification> Notifications = new List<Notification>();
        public bool Persistent = true;

        public void CheckQuest()
        {
            // Making sure there is one quest at all times
            if (MainDefs.stageChanging)
            {
                MainDefs.setQuests = true;
                if (ModConfig.restartQuestsOnStageChange) {
                    MainDefs.questsClientData = new List<QuestMessage>();
                    MainDefs.questsServerData = new List<QuestServerData>();
                }
                else
                {
                    for (int i = 0; i < MainDefs.questsServerData.Count; i++)
                    {
                        MainDefs.questsClientData[i].questInitialised = false;
                        SendQuest(MainDefs.questsClientData[i]);
                    }
                }
                MainDefs.stageChanging = false;
            }

            if (MainDefs.awaitingSetQuests) {
                Debug.Log(MainDefs.questsClientData.Count);
                Debug.Log(MainDefs.questsServerData.Count);
                for (int i = MainDefs.questsClientData.Count; i < MainDefs.questsServerData.Count; i++)
                {
                    GetNewQuest(i);
                }
                if (MainDefs.questsClientData.Count == MainDefs.questsServerData.Count) {
                    MainDefs.awaitingSetQuests = false;
                }
                MainDefs.setQuests = false;
            }

            if (MainDefs.questsClientData.Count < 1 && !MainDefs.awaitingSetQuests)
            {
                GetNewQuest(-1);
            }
        }

        public void GetNewQuest(int index)
        {
            if (!NetworkServer.active || MainDefs.questsClientData.Count >= ModConfig.questAmountMax) {
                return;
            }

            QuestMessage newQuest = QuestDefs.GetQuest(index);

            if (newQuest.questDescription != "bad") {
                newQuest.questID = QuestDefs.GetUniqueID();
                Debug.Log("Hello?");
                SendQuest(newQuest);
            }
        }

        public void SendQuest(QuestMessage Quest)
        {
            if (!NetworkServer.active)
            {
                return;
            }
            NetworkServer.SendToAll(MainDefs.questPort, Quest);
        }

        public void StartClientHanders()
        {
            Debug.Log("[RPGMod] Client Handlers Added");
            NetworkClient client = NetworkManager.singleton.client;

            client.RegisterHandler(MainDefs.questPort, OnQuestRecieved);
            clientRegistered = true;
        }

        public void OnQuestRecieved(NetworkMessage networkMessage)
        {
            QuestMessage message = networkMessage.ReadMessage<QuestMessage>();
            messageIndex = -1;
            for (int i = 0; i < MainDefs.questsClientData.Count; i++)
            {
                if (MainDefs.questsClientData[i].questID == message.questID)
                {
                    messageIndex = i;
                };
            }

            if (messageIndex == -1)
            {
                messageIndex = MainDefs.questsClientData.Count;
            }

            if (message.questInitialised == false)
            {
                MainDefs.questsClientData.RemoveAt(messageIndex);
                Destroy(Notifications[messageIndex]);
                Notifications.RemoveAt(messageIndex);
                if (NetworkServer.active && MainDefs.questsClientData.Count == 0) {
                    MainDefs.awaitingSetQuests = true;
                }
                if (NetworkServer.active)
                {
                    MainDefs.usedIDs.Remove(message.questID);
                    if (!MainDefs.setQuests) {
                        MainDefs.questsServerData.RemoveAt(messageIndex);
                    }
                }
            }
            else if (messageIndex < MainDefs.questsClientData.Count)
            {
                MainDefs.questsClientData[messageIndex] = message;
            }
            else
            {
                MainDefs.questsClientData.Add(message);
            }

            DisplayQuesting();
            if (message.questInitialised)
            {
                Notifications[messageIndex].GetDescription = () => message.questDescription;
            }
        }

        public void DisplayQuesting()
        {
            LocalUser localUser = LocalUserManager.GetFirstLocalUser();

            if (CachedCharacterBody == null && localUser != null)
            {
                CachedCharacterBody = localUser.cachedBody;
            }

            for (int i=0; i<MainDefs.questsClientData.Count; i++)
            {
                if (i >= Notifications.Count && CachedCharacterBody != null || MainDefs.resetUI)
                {
                    if (MainDefs.resetUI)
                    {
                        if (i < Notifications.Count)
                        {
                            Destroy(Notifications[i]);
                        }
                    }

                    if (isDebug)
                    {
                        Debug.Log(CachedCharacterBody);
                        Debug.Log(ModConfig.sizeX);
                        Debug.Log(ModConfig.sizeY);
                        Debug.Log(Screen.width * ModConfig.screenPosX / 100f);
                        Debug.Log(Screen.height * ModConfig.screenPosY / 100f);
                        Debug.Log(MainDefs.questsClientData[i].questDescription);
                    }

                    Notifications.Add(new Notification());

                    Notifications[i] = CachedCharacterBody.gameObject.AddComponent<Notification>();
                    Notifications[i].index = i;
                    Notifications[i].transform.SetParent(CachedCharacterBody.transform);
                    Notifications[i].GetTitle = () => "QUEST";
                    Notifications[i].UINotification.fadeTime = 1f;
                    Notifications[i].UINotification.duration = 86400f;
                    Notifications[i].SetSize(ModConfig.sizeX, ModConfig.sizeY);
                    if (i == (MainDefs.questsClientData.Count - 1)) {
                        MainDefs.resetUI = false;
                    }
                }

                if (MainDefs.questsClientData[i].questInitialised)
                {
                    Notifications[i].SetIcon(BodyCatalog.FindBodyPrefab(MainDefs.questsClientData[i].questTargetName).GetComponent<CharacterBody>().portraitIcon);
                }

                if (CachedCharacterBody == null && i < Notifications.Count)
                {
                    Destroy(Notifications[i]);
                }

                if (i < Notifications.Count)
                {
                    if (Notifications[i].RootObject != null)
                    {
                        if (Persistent || (localUser != null && localUser.inputPlayer != null && localUser.inputPlayer.GetButton("info")))
                        {
                            Notifications[i].RootObject.SetActive(true);
                        }
                        else
                        {
                            Notifications[i].RootObject.SetActive(false);
                        }
                    }
                }
            }
        }

        public void CheckQuestStatus(int index)
        {
            if (!NetworkServer.active)
            {
                return;
            }
            if (MainDefs.questsServerData[index].Progress >= MainDefs.questsServerData[index].Objective)
            {
                if (MainDefs.questsClientData[index].questInitialised)
                {
                    foreach (var player in PlayerCharacterMasterController.instances)
                    {
                        if (player.master.alive)
                        {
                            var transform = player.master.GetBody().coreTransform;
                            if (ModConfig.dropItemsFromPlayers)
                            {
                                PickupDropletController.CreatePickupDroplet(MainDefs.questsServerData[index].Drop, transform.position, transform.forward * 10f);
                            }
                            else
                            {
                                player.master.inventory.GiveItem(MainDefs.questsServerData[index].Drop.itemIndex);
                            }
                        }
                    }
                }
                MainDefs.questsClientData[index].questInitialised = false;
            }
        }

        //The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            // Refresh values initially
            ModConfig.ReloadConfig(Config, true);

            On.RoR2.Run.Start += (orig, self) =>
            {
                gameStarted = true;
                orig(self);
            };

            if (ModConfig.questingEnabled)
            {
                On.RoR2.Run.OnClientGameOver += (orig, self, runReport) =>
                {
                    MainDefs.resetUI = true;
                    orig(self, runReport);
                };

                On.RoR2.Run.OnDisable += (orig, self) =>
                {
                    gameStarted = false;
                    MainDefs.questsClientData = new List<QuestMessage>();
                    MainDefs.questsServerData = new List<QuestServerData>();

                    clientRegistered = false;
                    CachedCharacterBody = null;

                    for (int i = 0; i < Notifications.Count; i++)
                    {
                        if (Notifications[i] != null)
                        {
                            Destroy(Notifications[i]);
                        }
                    }

                    Notifications = new List<Notification>();

                    orig(self);
                };

                On.RoR2.Run.OnServerSceneChanged += (orig, self, sceneName) =>
                {
                    for (int i=0; i< MainDefs.questsClientData.Count; i++)
                    {
                        MainDefs.questsClientData[i].questInitialised = false;
                    }
                    MainDefs.stageChanging = true;
                    orig(self, sceneName);
                };
            }

            On.RoR2.SceneDirector.PopulateScene += (orig, self) =>
            {
                int credit = self.GetFieldValue<int>("interactableCredit");
                self.SetFieldValue("interactableCredit", (int)(credit * ModConfig.worldSpawnPercentage));
                orig(self);
            };

            On.RoR2.HealthComponent.Suicide += (orig, self, killerOverride, inflictorOverride) =>
            {
                if (self.gameObject.GetComponent<CharacterBody>().isBoss || self.gameObject.GetComponent<CharacterBody>().master.name == "EngiTurretMaster(Clone)")
                {
                    isSuicide = true;
                }
                orig(self, killerOverride, inflictorOverride);
            };

            // Death drop hanlder
            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, self, damageReport) =>
            {
                if (!isSuicide) {
                    float chance;
                    CharacterBody enemyBody = damageReport.victimBody;
                    GameObject attackerMaster = damageReport.damageInfo.attacker.GetComponent<CharacterBody>().masterObject;
                    CharacterMaster attackerController = attackerMaster.GetComponent<CharacterMaster>();

                    if (ModConfig.questingEnabled)
                    {
                        for (int i = 0; i < MainDefs.questsClientData.Count; i++)
                        {
                            if (MainDefs.questsServerData[i].Type == 0 && MainDefs.questsClientData[i].questInitialised == true && enemyBody.GetUserName() == MainDefs.questsClientData[i].questTarget)
                            {
                                QuestServerData newServerData = MainDefs.questsServerData[i];
                                newServerData.Progress += 1;
                                MainDefs.questsServerData[i] = newServerData;
                                
                                MainDefs.questsClientData[i].questDescription = QuestDefs.GetDescription(MainDefs.questsClientData[i], MainDefs.questsServerData[i]);
                            }
                            CheckQuestStatus(i);
                        }
                        for (int i = 0; i < MainDefs.questsClientData.Count; i++)
                        {
                            SendQuest(MainDefs.questsClientData[i]);
                        }
                    }

                    if (ModConfig.enemyItemDropsEnabled)
                    {
                        bool isElite = enemyBody.isElite || enemyBody.isChampion;
                        bool isBoss = enemyBody.isBoss;

                        if (isBoss)
                        {
                            chance = ModConfig.dropChanceBossEnemy;
                        }
                        else
                        {
                            if (isElite)
                            {
                                chance = ModConfig.dropChanceEliteEnemy;
                            }
                            else
                            {
                                chance = ModConfig.dropChanceNormalEnemy;
                            }
                        }

                        chance *= ((1 - ModConfig.playerChanceScaling) + (ModConfig.playerChanceScaling * Run.instance.participatingPlayerCount));
                        if (ModConfig.earlyChanceScaling - Run.instance.difficultyCoefficient > 0)
                        {
                            chance *= (ModConfig.earlyChanceScaling - (Run.instance.difficultyCoefficient - 1));
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
                                    weightedSelection.AddChoice(Run.instance.availableTier1DropList, ModConfig.eliteChanceCommon);
                                    weightedSelection.AddChoice(Run.instance.availableTier2DropList, ModConfig.eliteChanceUncommon);
                                    weightedSelection.AddChoice(Run.instance.availableTier3DropList, ModConfig.eliteChanceLegendary);
                                    weightedSelection.AddChoice(Run.instance.availableLunarDropList, ModConfig.eliteChanceLunar);
                                }
                                else
                                {
                                    weightedSelection.AddChoice(Run.instance.availableTier1DropList, ModConfig.normalChanceCommon);
                                    weightedSelection.AddChoice(Run.instance.availableTier2DropList, ModConfig.normalChanceUncommon);
                                    weightedSelection.AddChoice(Run.instance.availableTier3DropList, ModConfig.normalChanceLegendary);
                                    weightedSelection.AddChoice(Run.instance.availableEquipmentDropList, ModConfig.normalChanceEquip);
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
                    isSuicide = false;
                }
                orig(self, damageReport);
            };

            if (!ModConfig.defaultWorldSpawnsEnabled)
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
                        cardsCopy.cards = cardSelection.categories[i].cards.Where(val => !ModConfig.bannedDirectorSpawns.Any(val.spawnCard.prefab.name.Contains)).ToArray();

                        // Sets category to new edited version
                        cardSelection.categories[i] = cardsCopy;
                    }
                    // Sets new card categories
                    self.SetFieldValue("interactableCategories", cardSelection);

                    // Runs original function
                    orig(self);
                };

            }

            Chat.AddMessage("<color=#13d3dd>RPGMod: </color> Loaded Successfully!");
        }

        public void Update()
        {
            if (gameStarted)
            {
                // Checks for quest
                if (ModConfig.questingEnabled)
                {
                    CheckQuest();
                }

                // Registers Client Handlers
                if (!clientRegistered)
                {
                    StartClientHanders();
                }

                if (Input.GetKeyDown(KeyCode.F6))
                {
                    // ModConfig.ReloadConfig(Config, false);
                    MainDefs.stageChanging = true;
                }

                if (Input.GetKeyDown(KeyCode.F3) && isDebug)
                {
                    GetNewQuest(-1);
                }

                if (Input.GetKeyDown(KeyCode.F4) && isDebug)
                {
                    QuestMessage message = MainDefs.questsClientData[MainDefs.questsClientData.Count - 1];
                    message.questInitialised = false;
                    SendQuest(message);
                }

            }
        }
    }
}