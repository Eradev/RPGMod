using System.Collections.Generic;
using UnityEngine;

namespace RPGMod
{
    // The variables that are used many times in the mod.
    public static class MainDefs
    {
        public static AssetBundle assetBundle;
        public static bool resetUI = false;
        public static bool stageChanging = false;
        public static bool awaitingSetQuests = false;
        public static bool deleteServerData = true;
        public static short questPort = 1337;
        public static List<string> questTypes = new List<string>() { "KILL", "COLLECT", "OPEN", "USE", "HEAL", "KILL" };
        public static List<string> questTargets = new List<string>() { "", "Gold", "Chest", "Ability", "Damage", "Elite" };
        public static List<string> questIconPaths = new List<string>() { "", "Assets/coin.png", "Assets/chest.png", "Assets/ability.png", "Assets/heal.png", "Assets/aspects.png" };
        public static List<Color> questColors = new List<Color>() {  new Color(0.82f, 0, 0, 0.5f), new Color(0.9f, 0.75f, 0, 0.5f), new Color(0, 0.36f, 0.78f, 0.5f), new Color(0.2f, 0.7f, 0.65f, 0.5f),
                                                                     new Color(0.2f, 0.7f, 0.2f, 0.5f), new Color(0.7f, 0.45f, 0.2f, 0.5f)};
        public static List<QuestMessage> questsClientData = new List<QuestMessage>();
        public static List<QuestServerData> questsServerData = new List<QuestServerData>();
        public static List<int> usedIDs = new List<int>();
        public static List<int> usedTypes = new List<int>();
    }
}