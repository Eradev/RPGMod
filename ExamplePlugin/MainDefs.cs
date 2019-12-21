using System.Collections.Generic;
using UnityEngine;

namespace RPGMod
{
    struct QuestDefinitions {
        public List<string> types;
        public List<string> targets;
        public List<string> iconPaths;
        public List<Color> colors;
        public int items;
    }

    struct MainDefs
    {
        static MainDefs() {
            questDefinitions = new QuestDefinitions
            {
                types = new List<string>() { "KILL", "COLLECT", "OPEN", "HEAL", "KILL" },
                targets = new List<string>() { "", "Gold", "Chests", "Damage", "Elites" },
                iconPaths = new List<string>() { "", "Assets/textures/coin.png", "Assets/textures/chest.png", "Assets/textures/heal.png", "Assets/textures/aspects.png" },
                colors = new List<Color>() { new Color(0.82f, 0, 0, 0.5f), new Color(0.9f, 0.75f, 0, 0.5f),
                    new Color(0, 0.36f, 0.78f, 0.5f), new Color(0.2f, 0.7f, 0.2f, 0.5f), new Color(0.7f, 0.45f, 0.2f, 0.5f) },
                items = 5
            };
        }

        public static readonly bool debugMode = true;
        public static readonly System.Random random = new System.Random();
        public static AssetBundle assetBundle { get; set; }
        public static bool AwaitingDefinedQuests { get; set; } = false;
        public static bool AdvancingStage { get; set; } = false;
        public static QuestDefinitions questDefinitions { get; set; }
        public static List<Questing.ClientMessage> QuestClientMessages { get; set; } = new List<Questing.ClientMessage>();
        public static List<Questing.ServerMessage> QuestServerMessages { get; set; } = new List<Questing.ServerMessage>();
        public static List<int> usedIDs { get; set; } = new List<int>();
        public static List<Questing.Type> usedTypes { get; set; } = new List<Questing.Type>();
    }
}