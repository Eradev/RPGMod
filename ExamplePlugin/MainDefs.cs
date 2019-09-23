using System.Collections.Generic;
using Unity;
using UnityEngine;

namespace RPGMod
{
    public static class MainDefs
    {
        public static AssetBundle assetBundle;
        public static bool resetUI = false;
        public static bool stageChanging = false;
        public static bool awaitingSetQuests = false;
        public static bool deleteServerData = true;
        public static short questPort = 1337;
        public static List<string> questTypes = new List<string>() { "<b>Kill</b>", "<b>Collect</b>" };
        public static List<Color> questColors = new List<Color>() { Color.red, Color.yellow };
        public static List<QuestMessage> questsClientData = new List<QuestMessage>();
        public static List<QuestServerData> questsServerData = new List<QuestServerData>();
        public static List<int> usedIDs = new List<int>();
    }
}