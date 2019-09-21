using System.Collections.Generic;

namespace RPGMod
{
    public static class MainDefs
    {
        public static bool resetUI = false;
        public static bool stageChanging = false;
        public static bool awaitingSetQuests = false;
        public static bool setQuests = false;
        public static short questPort = 1337;
        public static List<QuestMessage> questsClientData = new List<QuestMessage>();
        public static List<QuestServerData> questsServerData = new List<QuestServerData>();
        public static List<int> usedIDs = new List<int>();
    }
}