using System.Collections.Generic;
using UnityEngine;

namespace RPGMod
{
    struct MainDefs
    {
        public static readonly bool debugMode = true;
        public static AssetBundle assetBundle { get; set; }
        public static bool resetUI = false;
        public static bool stageChanging = false;
        public static bool awaitingSetQuests = false;
        public static bool deleteServerData = true;
        public static List<string> questTypes = new List<string>() { "KILL", "COLLECT", "OPEN", "USE", "HEAL", "KILL" };
        public static List<string> questTargets = new List<string>() { "", "Gold", "Chests", "Abilities", "Damage", "Elites" };
        public static List<string> questIconPaths = new List<string>() { "", "Assets/coin.png", "Assets/chest.png", "Assets/ability.png", "Assets/heal.png", "Assets/aspects.png" };
        public static List<Color> questColors = new List<Color>() {  new Color(0.82f, 0, 0, 0.5f), new Color(0.9f, 0.75f, 0, 0.5f), new Color(0, 0.36f, 0.78f, 0.5f), new Color(0.2f, 0.7f, 0.65f, 0.5f),
                                                                     new Color(0.2f, 0.7f, 0.2f, 0.5f), new Color(0.7f, 0.45f, 0.2f, 0.5f)};
        public static List<Questing.ClientMessage> QuestClientMessages { get; set; } = new List<Questing.ClientMessage>();
        public static List<Questing.ServerMessage> QuestServerMessages { get; set; } = new List<Questing.ServerMessage>();
        public static List<int> usedIDs = new List<int>();
        public static List<int> usedTypes = new List<int>();
    }
}