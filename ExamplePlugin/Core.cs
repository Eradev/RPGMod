using System;
using System.Collections.Generic;
using RPGMod.Questing;
using UnityEngine;
using Random = System.Random;

namespace RPGMod
{
    internal static class Core
    {
        public static readonly bool DebugMode = false;
        public static readonly Random Random = new Random();
        public static AssetBundle AssetBundle { get; set; }
        public static QuestDefinitions QuestDefinitions { get; set; }
        public static List<int> UsedIDs { get; set; } = new List<int>();
        public static Dictionary<QuestType, int> UsedTypes { get; set; } = new Dictionary<QuestType, int>();

        public static void Reset()
        {
            UsedTypes.Clear();
            foreach (QuestType type in Enum.GetValues(typeof(QuestType)))
            {
                UsedTypes.Add(type, 0);
            }
        }

        static Core()
        {
            QuestDefinitions = new QuestDefinitions
            {
                Types = new List<string> { "KILL", "COLLECT", "OPEN", "HEAL", "KILL" },
                Targets = new List<string> { "", "Gold", "Chests", "Damage", "Elites" },
                IconPaths = new List<string> { "", "Assets/textures/coin.png", "Assets/textures/chest.png", "Assets/textures/heal.png", "Assets/textures/aspects.png" },
                Colors = new List<Color>
                { 
                    new Color(0.82f, 0, 0, 0.5f), 
                    new Color(0.9f, 0.75f, 0, 0.5f),
                    new Color(0, 0.36f, 0.78f, 0.5f), 
                    new Color(0.2f, 0.7f, 0.2f, 0.5f), 
                    new Color(0.7f, 0.45f, 0.2f, 0.5f)
                },
                Items = 5
            };
            Reset();
        }
    }
}