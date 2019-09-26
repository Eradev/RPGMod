using RoR2;
using UnityEngine;

namespace RPGMod
{
    public static class Listener
    { 
        // Updates the relevant quest according to the parameters.
        public static void UpdateQuest(int value, int type, string target) {
            Debug.Log("Quest Updating [VALUE = " + value + "] [TYPE = " + type + "] [TARGET = " + target + "]");
            int i = 0;
            while (i < MainDefs.questsServerData.Count) {
                Debug.Log("Quest Checking [TYPE = " + MainDefs.questsServerData[i].type + "] [TARGET = " + MainDefs.questsClientData[i].questTarget + "]");
                Debug.Log(MainDefs.questsServerData[i].type == type);
                Debug.Log(MainDefs.questsClientData[i].questTarget == target);
                if (MainDefs.questsServerData[i].type == type && MainDefs.questsClientData[i].questTarget == target) {
                    QuestServerData newServerData = MainDefs.questsServerData[i];
                    newServerData.progress += value;
                    MainDefs.questsServerData[i] = newServerData;
                    MainDefs.questsClientData[i].questDescription = QuestDefs.GetDescription(MainDefs.questsClientData[i], MainDefs.questsServerData[i]);
                    CheckQuestStatus(i);
                    Debug.Log("UPDATED QUEST");
                    QuestDefs.SendQuest(MainDefs.questsClientData[i]);
                }
                i++;
            }
        }

        // Checks if the quest at the index has fulfilled the objective.
        static void CheckQuestStatus(int index)
        {
            if (MainDefs.questsServerData[index].progress >= MainDefs.questsServerData[index].objective && MainDefs.questsClientData[index].questInitialised)
            {
                foreach (var player in PlayerCharacterMasterController.instances)
                {
                    if (player.master.alive)
                    {
                        var transform = player.master.GetBody().coreTransform;
                        if (ModConfig.dropItemsFromPlayers)
                        {
                            PickupDropletController.CreatePickupDroplet(MainDefs.questsServerData[index].drop, transform.position, transform.forward * 10f);
                        }
                        else
                        {
                            player.master.inventory.GiveItem(MainDefs.questsServerData[index].drop.itemIndex);
                        }
                    }
                }
                MainDefs.questsClientData[index].questInitialised = false;
            }
        }
    }
}
