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
        public static List<Color> questColors = new List<Color>() {  new Color(0.82f, 0, 0), new Color(0.9f, 0.75f, 0) };
        public static List<QuestMessage> questsClientData = new List<QuestMessage>();
        public static List<QuestServerData> questsServerData = new List<QuestServerData>();
        public static List<int> usedIDs = new List<int>();
    }
}