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
    [BepInPlugin("com.ghasttear1.rpgmod", "RPGMod", "1.3.0")]

    public class RPGMod : BaseUnityPlugin
    {   
        // Networking params
        public short msgQuestDrop = 1337;
        public bool isClientRegistered = false;

        // Misc params
        public System.Random random = new System.Random();
        public CharacterMaster cMaster;

        public GameObject targetBody;
        public bool isLoaded = false;
        public bool isDebug = true;
        public bool isSuicide = false;
        public bool Persistent = true;
        public CharacterBody CachedCharacterBody;
        public List<Notification> Notifications = new List<Notification>();
        public QuestDefinitions QuestDefinitions = new QuestDefinitions();
        public int messageIndex;

        // Handles questing
        public void CheckQuest()
            {
                // Making sure there is one quest at all times
                if (GlobalDefs.Quests.Count < 1)
                {
                    GetNewQuest();
                }
            }

        // Sets quest parameters
        public void GetNewQuest()
        {
            if (!NetworkServer.active || GlobalDefs.Quests.Count >= ModConfig.questAmount) {
                return;
            }
            QuestMessage newQuest = QuestDefinitions.GetQuest();
            if (newQuest.questDescription != "bad") {
                int newID = random.Next();
                while (GlobalDefs.currentIDs.Contains(newID))
                {
                    newID = random.Next();
                }
                GlobalDefs.currentIDs.Add(newID);
                newQuest.questID = newID;
                SendQuest(newQuest);
            }
        }

        // Check if quest fulfilled
        public void CheckQuestStatus(int index)
        {
            if (!NetworkServer.active) {
                return;
            }
            if (GlobalDefs.QuestsServerData[index].Progress >= GlobalDefs.QuestsServerData[index].Objective)
            {
                if (GlobalDefs.Quests[index].questInitialised)
                {
                    foreach (var player in PlayerCharacterMasterController.instances)
                    {
                        if (player.master.alive)
                        {
                            var transform = player.master.GetBody().coreTransform;
                            if (ModConfig.itemDroppingFromPlayers)
                            {
                                PickupDropletController.CreatePickupDroplet(GlobalDefs.QuestsServerData[index].Drop, transform.position, transform.forward * 10f);
                            }
                            else
                            {
                                player.master.inventory.GiveItem(GlobalDefs.QuestsServerData[index].Drop.itemIndex);
                            }
                        }
                    }
                }
                GlobalDefs.Quests[index].questInitialised = false;
            }
        }

        // Handles the display of the UI
        public void DisplayQuesting()
        {
            LocalUser localUser = LocalUserManager.GetFirstLocalUser();

            if (CachedCharacterBody == null && localUser != null)
            {
                CachedCharacterBody = localUser.cachedBody;
            }
            Debug.Log(GlobalDefs.Quests.Count);
            for (int i=0; i<GlobalDefs.Quests.Count; i++)
            {
                if (i >= Notifications.Count && CachedCharacterBody != null || GlobalDefs.resetUI)
                {
                    if (GlobalDefs.resetUI)
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
                        Debug.Log(GlobalDefs.Quests[i].questDescription);
                    }

                    Notifications.Add(new Notification());

                    Notifications[i] = CachedCharacterBody.gameObject.AddComponent<Notification>();
                    Notifications[i].index = i;
                    Notifications[i].transform.SetParent(CachedCharacterBody.transform);
                    Notifications[i].GetTitle = () => "QUEST";
                    Notifications[i].QuestHolder.fadeTime = 1f;
                    Notifications[i].QuestHolder.duration = 86400f;
                    Notifications[i].SetSize(ModConfig.sizeX, ModConfig.sizeY);
                    if (i == (GlobalDefs.Quests.Count - 1)) {
                        GlobalDefs.resetUI = false;
                    }
                }

                if (GlobalDefs.Quests[i].questInitialised)
                {
                    Notifications[i].SetIcon(BodyCatalog.FindBodyPrefab(GlobalDefs.Quests[i].questTargetName).GetComponent<CharacterBody>().portraitIcon);
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

        // Set Client Handlers
        public void InitClientHanders()
        {
            Debug.Log("[RPGMod] Client Handlers Added");
            NetworkClient client = NetworkManager.singleton.client;

            client.RegisterHandler(msgQuestDrop, OnQuestRecieved);
            isClientRegistered = true;
        }

        // Send data message
        public void SendQuest(QuestMessage Quest)
        {
            if (!NetworkServer.active) {
                return;
            }
            NetworkServer.SendToAll(msgQuestDrop, Quest);
        }

        // Handler function for quest drop message
        public void OnQuestRecieved(NetworkMessage netMsg) {
            QuestMessage message = netMsg.ReadMessage<QuestMessage>();
            messageIndex = -1;
            for (int i = 0; i < GlobalDefs.Quests.Count; i++) {
                if (GlobalDefs.Quests[i].questID == message.questID) {
                    messageIndex = i;
                };
            }
            if (messageIndex == -1) {
                messageIndex = GlobalDefs.Quests.Count;
            }
            Debug.Log(messageIndex);
            if (message.questInitialised == false)
            {
                GlobalDefs.Quests.RemoveAt(messageIndex);
                GlobalDefs.currentIDs.Remove(message.questID);
                Destroy(Notifications[messageIndex]);
                Notifications.RemoveAt(messageIndex);
                if (NetworkServer.active)
                {
                    GlobalDefs.QuestsServerData.RemoveAt(messageIndex);
                    Debug.Log("Removed serverquestdata and quests");
                }
                //for (int i = 0; i < GlobalDefs.Quests.Count; i++)
                //{
                //    Debug.Log("changing messageIndexes");
                //    GlobalDefs.Quests[i].questIndex = i;
                //}
            }
            else if (messageIndex < GlobalDefs.Quests.Count)
            {
                GlobalDefs.Quests[messageIndex] = message;
                Debug.Log("Changing message");
            }
            else
            {
                GlobalDefs.Quests.Add(message);
                Debug.Log("Adding message");
            }
            DisplayQuesting();
            if (message.questInitialised)
            {
                Notifications[messageIndex].GetDescription = () => message.questDescription;
            }
        }

       

        // Drops Boss Chest
        public void DropBoss(SpawnCard spawnCard, Transform transform)
        {
            transform.Translate(Vector3.down * 0.5f);
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit))
            {
                transform.Translate(Vector3.down * hit.distance);
                spawnCard.DoSpawn(transform.position, transform.rotation, null);
            }
        }

        //The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            Chat.AddMessage("<color=#13d3dd>RPGMod: </color> Loaded Successfully!");

            // Refresh values initially
            ModConfig.RefreshValues(Config, true);

            On.RoR2.Run.Start += (orig, self) =>
            {
                isLoaded = true;
                GlobalDefs.questFirst = true;
                orig(self);
            };

            if (ModConfig.isQuesting)
            {
                On.RoR2.Run.OnClientGameOver += (orig, self, runReport) =>
                {
                    GlobalDefs.resetUI = true;
                    orig(self, runReport);
                };

                On.RoR2.Run.OnDisable += (orig, self) =>
                {
                    isLoaded = false;
                    GlobalDefs.Quests = new List<QuestMessage>();
                    GlobalDefs.QuestsServerData = new List<QuestServerData>();

                    isClientRegistered = false;

                    CachedCharacterBody = null;

                    for (int i = 0; i < Notifications.Count; i++)
                    {
                        if (Notifications[i] != null)
                        {
                            Destroy(Notifications[i]);
                        }
                    }

                    orig(self);
                };

                On.RoR2.Run.OnServerSceneChanged += (orig, self, sceneName) =>
                {
                    for (int i=0; i< GlobalDefs.Quests.Count; i++)
                    {
                        GlobalDefs.Quests[i].questInitialised = false;
                    }
                    GlobalDefs.stageChange = true;
                    GlobalDefs.resetUI = true;
                    orig(self, sceneName);
                };
            }

            On.RoR2.SceneDirector.PopulateScene += (orig, self) =>
            {
                int credit = self.GetFieldValue<int>("interactableCredit");
                self.SetFieldValue("interactableCredit", (int)(credit * ModConfig.percentSpawns));
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

                    if (ModConfig.isQuesting)
                    {
                        for (int i = 0; i < GlobalDefs.Quests.Count; i++)
                        {
                            if (GlobalDefs.QuestsServerData[i].Type == 0 && GlobalDefs.Quests[i].questInitialised == true && enemyBody.GetUserName() == GlobalDefs.Quests[i].questTarget)
                            {
                                QuestServerData newServerData = GlobalDefs.QuestsServerData[i];
                                newServerData.Progress += 1;
                                GlobalDefs.QuestsServerData[i] = newServerData;
                                
                                GlobalDefs.Quests[i].questDescription = QuestDefinitions.GetDescription(GlobalDefs.Quests[i], GlobalDefs.QuestsServerData[i]);
                            }
                            CheckQuestStatus(i);
                        }
                        for (int i = 0; i < GlobalDefs.Quests.Count; i++)
                        {
                            SendQuest(GlobalDefs.Quests[i]);
                        }
                    }

                    if (ModConfig.isEnemyDrops)
                    {
                        bool isElite = enemyBody.isElite || enemyBody.isChampion;
                        bool isBoss = enemyBody.isBoss;

                        if (isBoss)
                        {
                            chance = ModConfig.chanceBoss;
                        }
                        else
                        {
                            if (isElite)
                            {
                                chance = ModConfig.chanceElite;
                            }
                            else
                            {
                                chance = ModConfig.chanceNormal;
                            }
                        }

                        chance *= ((1 - ModConfig.dropsPlayerScaling) + (ModConfig.dropsPlayerScaling * Run.instance.participatingPlayerCount));
                        if (ModConfig.gameStartScaling - Run.instance.difficultyCoefficient > 0)
                        {
                            chance *= (ModConfig.gameStartScaling - (Run.instance.difficultyCoefficient - 1));
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
                                    weightedSelection.AddChoice(Run.instance.availableTier1DropList, ModConfig.eliteChanceTier1);
                                    weightedSelection.AddChoice(Run.instance.availableTier2DropList, ModConfig.eliteChanceTier2);
                                    weightedSelection.AddChoice(Run.instance.availableTier3DropList, ModConfig.eliteChanceTier3);
                                    weightedSelection.AddChoice(Run.instance.availableLunarDropList, ModConfig.eliteChanceTierLunar);
                                }
                                else
                                {
                                    weightedSelection.AddChoice(Run.instance.availableTier1DropList, ModConfig.normalChanceTier1);
                                    weightedSelection.AddChoice(Run.instance.availableTier2DropList, ModConfig.normalChanceTier2);
                                    weightedSelection.AddChoice(Run.instance.availableTier3DropList, ModConfig.normalChanceTier3);
                                    weightedSelection.AddChoice(Run.instance.availableEquipmentDropList, ModConfig.normalChanceTierEquip);
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

            if (ModConfig.isChests)
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

        }

        public void Update()
        {
            if (isLoaded)
            {
                // Checks for quest
                if (ModConfig.isQuesting)
                {
                    CheckQuest();
                }

                // Registers Client Handlers
                if (!isClientRegistered)
                {
                    InitClientHanders();
                }

                if (Input.GetKeyDown(KeyCode.F6))
                {
                    ModConfig.RefreshValues(Config, false);
                }

                if (Input.GetKeyDown(KeyCode.F3) && isDebug)
                {
                    GetNewQuest();
                }

                if (Input.GetKeyDown(KeyCode.F4) && isDebug)
                {

                    QuestMessage message = GlobalDefs.Quests[GlobalDefs.Quests.Count - 1];
                    message.questInitialised = false;
                    SendQuest(message);
                }

            }
        }
    }
}