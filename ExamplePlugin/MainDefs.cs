using System.Collections.Generic;
using UnityEngine;

namespace RPGMod
{
    internal struct MainDefs
    {
        public static readonly bool debugMode = true;
        public static AssetBundle assetBundle { get; set; }
        public static bool AwaitingDefinedQuests { get; set; } = false;
        public static bool deleteServerData { get; set; } = true;

        public static List<string> questTypes = new List<string>() { "KILL", "COLLECT", "OPEN", "HEAL", "KILL" };
        public static List<string> questTargets = new List<string>() { "", "Gold", "Chests", "Damage", "Elites" };
        public static List<string> questIconPaths = new List<string>() { "", "Assets/textures/coin.png", "Assets/textures/chest.png", "Assets/textures/heal.png", "Assets/textures/aspects.png" };

        public static List<Color> questColors = new List<Color>() {  new Color(0.82f, 0, 0, 0.5f), new Color(0.9f, 0.75f, 0, 0.5f), new Color(0, 0.36f, 0.78f, 0.5f),
                                                                     new Color(0.2f, 0.7f, 0.2f, 0.5f), new Color(0.7f, 0.45f, 0.2f, 0.5f)};

        public static List<Questing.ClientMessage> QuestClientMessages { get; set; } = new List<Questing.ClientMessage>();
        public static List<Questing.ServerMessage> QuestServerMessages { get; set; } = new List<Questing.ServerMessage>();
        public static List<int> usedIDs { get; set; } = new List<int>();
        public static List<int> usedTypes { get; set; } = new List<int>();
    }
}