using System;
using System.Collections.Generic;
using System.Text;

namespace RPGMod
{
    public static class GlobalDefs
    {
        public static bool resetUI = false;
        public static bool questFirst = true;
        public static bool stageChange = false;
        public static List<QuestMessage> Quests = new List<QuestMessage>();
        public static List<QuestServerData> QuestsServerData = new List<QuestServerData>();
        public static List<int> currentIDs = new List<int>();
    }
}
