using RoR2;
using UnityEngine;

namespace RPGMod {
namespace Questing {


public static class Listener
{
    // Updates the relevant quest according to the parameters.
    public static void UpdateQuest(int value, int type, string target)
    {
        Debug.Log("Quest Updating [VALUE = " + value + "] [TYPE = " + type + "] [TARGET = " + target + "]");

        for (var i = 0; i < MainDefs.QuestServerMessages.Count; i++)
        {
            Debug.Log("Quest Checking [TYPE = " + MainDefs.QuestServerMessages[i].type + "] [TARGET = " + MainDefs.QuestClientMessages[i].target + "]");
            Debug.Log(MainDefs.QuestServerMessages[i].type == type);
            Debug.Log(MainDefs.QuestClientMessages[i].target == target);

            if (MainDefs.QuestServerMessages[i].type == type && MainDefs.QuestClientMessages[i].target == target)
            {
                ServerMessage newServerData = MainDefs.QuestServerMessages[i];
                newServerData.progress += value;
                MainDefs.QuestServerMessages[i] = newServerData;
                MainDefs.QuestClientMessages[i].description = Quest.GetDescription(MainDefs.QuestClientMessages[i], MainDefs.QuestServerMessages[i]);
                CheckQuestStatus(i);
                Debug.Log("UPDATED QUEST");
                Quest.SendQuest(MainDefs.QuestClientMessages[i]);
            }
        }

    }

    // Checks if the quest at the index has fulfilled the objective.
    static void CheckQuestStatus(int index)
    {
        if (MainDefs.QuestServerMessages[index].progress >= MainDefs.QuestServerMessages[index].objective && MainDefs.QuestClientMessages[index].initialised)
        {
            foreach (var player in PlayerCharacterMasterController.instances)
            {
                if (player.master.alive)
                {
                    var transform = player.master.GetBody().coreTransform;
                    if (Config.dropItemsFromPlayers)
                    {
                        PickupDropletController.CreatePickupDroplet(MainDefs.QuestServerMessages[index].drop.pickupIndex, transform.position, transform.forward * 10f);
                    }
                    else
                    {
                        player.master.inventory.GiveItem(MainDefs.QuestServerMessages[index].drop.itemIndex);
                    }
                }
            }
            MainDefs.QuestClientMessages[index].initialised = false;
        }
    }
}


} // namespace Questing
} // namespace RPGMod